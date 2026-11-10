using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PhamNhan.Audio;

namespace PhamNhan.View
{
    /// <summary>Phát tiếng động và nhạc nền. Âm thanh được tổng hợp ở luồng phụ lúc mở game.</summary>
    public class Sound : MonoBehaviour
    {
        public enum Track { None, Menu, Battle, Climax }

        public bool MusicOn { get { return musicOn; } set { musicOn = value; PlayerPrefs.SetInt("music", value ? 1 : 0); PlayerPrefs.Save(); } }
        public bool SfxOn { get { return sfxOn; } set { sfxOn = value; PlayerPrefs.SetInt("sfx", value ? 1 : 0); PlayerPrefs.Save(); } }
        bool musicOn = true, sfxOn = true;

        Task<Dictionary<string, float[]>> sfxTask; Task<float[][]> musTask;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioClip[] music; // menu, battle, climax
        readonly List<AudioSource> pool = new List<AudioSource>();
        AudioSource musA, musB; Track want = Track.None, playing = Track.None; float fade = 1;
        readonly Dictionary<string, float> last = new Dictionary<string, float>();

        // âm lượng gốc và khoảng cách tối thiểu giữa hai lần phát cùng một tiếng
        static readonly Dictionary<string, float> VOL = new Dictionary<string, float> {
            {"tap",0.45f},{"slash",0.5f},{"hit",0.32f},{"crit",0.45f},{"hitC",0.22f},{"yu",0.6f},{"loiCast",0.4f},{"loi",0.85f},{"anCast",0.4f},{"an",0.8f},
            {"van",0.8f},{"yan",0.6f},{"don",0.5f},{"dan",0.55f},{"ho",0.6f},{"phaCast",0.45f},{"pha",0.7f},{"reveal",0.85f},{"seal",0.7f},{"vain",0.4f},
            {"eyeHit",0.3f},{"eyeDown",1f},{"foeDown",0.8f},{"allyDown",0.55f},{"heroDown",0.7f},{"banner",0.35f},{"maCast",0.35f},{"ma",0.55f},{"shock",0.45f},
            {"bossRoar",0.9f},{"bossCast",0.5f},{"tram",0.8f},{"bossSlash",0.55f},{"khon",0.55f},{"heal",0.45f},{"fall",0.22f},{"fire",0.6f},{"ice",0.55f},{"vine",0.5f},
            {"win",0.9f},{"lose",0.85f},
        };
        static readonly Dictionary<string, float> GAP = new Dictionary<string, float> {
            {"hit",0.06f},{"crit",0.08f},{"hitC",0.1f},{"fall",0.045f},{"eyeHit",0.12f},{"ma",0.12f},{"maCast",0.2f},{"shock",0.25f},{"heal",0.4f},{"banner",0.6f},{"slash",0.08f},
            {"fire",0.15f},{"ice",0.15f},{"vine",0.15f},{"an",0.1f},{"bossSlash",0.2f},{"yan",0.3f},
        };
        // tiếng luôn nghe rõ dù ở xa (sự kiện lớn của trận)
        static readonly HashSet<string> GLOBAL = new HashSet<string> { "reveal", "eyeDown", "foeDown", "allyDown", "heroDown", "win", "lose", "banner", "bossRoar", "tap" };

        void Awake()
        {
            musicOn = PlayerPrefs.GetInt("music", 1) == 1; sfxOn = PlayerPrefs.GetInt("sfx", 1) == 1;
            if (App.THREADS)
            {
                sfxTask = Task.Run(() => Synth.MakeSfx());
                musTask = Task.Run(() => new[] { Synth.MusicMenu(), Synth.MusicBattle(false), Synth.MusicBattle(true) });
            }
            for (int i = 0; i < 14; i++) { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; s.spatialBlend = 0; pool.Add(s); }
            musA = gameObject.AddComponent<AudioSource>(); musB = gameObject.AddComponent<AudioSource>();
            foreach (var m in new[] { musA, musB }) { m.loop = true; m.playOnAwake = false; m.volume = 0; m.spatialBlend = 0; }
            if (FindObjectOfType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
        }

        static AudioClip Clip(string name, float[] data)
        {
            var c = AudioClip.Create(name, data.Length, 1, Synth.SR, false); c.SetData(data, 0); return c;
        }

        void Update()
        {
            if (sfxTask != null && sfxTask.IsCompleted)
            {
                if (sfxTask.Status == TaskStatus.RanToCompletion) foreach (var kv in sfxTask.Result) clips[kv.Key] = Clip(kv.Key, kv.Value);
                else Debug.LogWarning("Tạo tiếng động lỗi: " + sfxTask.Exception);
                sfxTask = null;
            }
            if (musTask != null && musTask.IsCompleted)
            {
                if (musTask.Status == TaskStatus.RanToCompletion) { var r = musTask.Result; music = new[] { Clip("menu", r[0]), Clip("battle", r[1]), Clip("climax", r[2]) }; }
                else Debug.LogWarning("Tạo nhạc lỗi: " + musTask.Exception);
                musTask = null;
            }
            // nhạc nền: chuyển bài bằng cách mờ dần
            if (music != null && want != playing)
            {
                var next = want == Track.None ? null : music[(int)want - 1];
                var t = musA; musA = musB; musB = t; // musA là bài mới
                musA.clip = next; musA.volume = 0; if (next != null) { musA.time = 0; musA.Play(); }
                playing = want; fade = 0;
            }
            fade = Mathf.Min(1, fade + Time.unscaledDeltaTime / 1.8f);
            float target = musicOn ? 0.55f : 0;
            musA.volume = Mathf.MoveTowards(musA.volume, target * fade, Time.unscaledDeltaTime);
            musB.volume = Mathf.MoveTowards(musB.volume, 0, Time.unscaledDeltaTime / 1.5f);
            if (musB.volume <= 0 && musB.isPlaying) musB.Stop();
        }

        public void SetTrack(Track t) { want = t; }

        /// <summary>Tổng hợp âm thanh từng phần mỗi khung hình (bản web không có luồng phụ).</summary>
        public IEnumerator<float> BuildSteps()
        {
            foreach (var kv in Synth.MakeSfx()) clips[kv.Key] = Clip(kv.Key, kv.Value);
            yield return 0.25f;
            var menu = Clip("menu", Synth.MusicMenu()); yield return 0.5f;
            var battle = Clip("battle", Synth.MusicBattle(false)); yield return 0.75f;
            var climax = Clip("climax", Synth.MusicBattle(true));
            music = new[] { menu, battle, climax }; yield return 1;
        }

        /// <summary>Phát một tiếng. near: 1 khi ở giữa màn hình, giảm dần về 0 khi ở xa ngoài khung nhìn.</summary>
        public void Play(string id, float near = 1, float pitchJitter = 0.05f)
        {
            if (!sfxOn) return;
            AudioClip c; if (!clips.TryGetValue(id, out c)) return;
            float gap; float now = Time.unscaledTime;
            if (GAP.TryGetValue(id, out gap)) { float lt; if (last.TryGetValue(id, out lt) && now - lt < gap) return; }
            if (!GLOBAL.Contains(id)) { if (near <= 0.02f) return; } else near = Mathf.Max(near, 0.6f);
            last[id] = now;
            AudioSource src = null;
            foreach (var s in pool) if (!s.isPlaying) { src = s; break; }
            if (src == null) { // thay tiếng nhỏ nhất
                float mv = 9; foreach (var s in pool) if (s.volume < mv) { mv = s.volume; src = s; }
            }
            float v; if (!VOL.TryGetValue(id, out v)) v = 0.5f;
            src.clip = c; src.volume = v * near; src.pitch = 1 + Random.Range(-pitchJitter, pitchJitter); src.Play();
        }
    }
}
