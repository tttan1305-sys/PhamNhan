using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PhamNhan.Sim;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.View
{
    /// <summary>Điều khiển toàn bộ trò chơi: màn hình chính, trận đấu, máy quay, cảm ứng và giao diện.</summary>
    public partial class App : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public const bool THREADS = false; // trình duyệt: không có luồng phụ
#else
        public const bool THREADS = true;
#endif
        float loadK = 1; string loadMsg;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindObjectOfType<App>() != null) return;
            new GameObject("PhamNhan").AddComponent<App>();
        }

        enum Screen_ { Menu, Help, Play, Pause, Roster, World }
        Screen_ scr = Screen_.Menu;
        Siege G; WorldView view; Camera cam;
        Vector2 camPos; float zoom = Z0;
        bool autoMode; float menuT; Sound snd;

        void Awake()
        {
            if (THREADS) Application.targetFrameRate = 60;
            UnityEngine.Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (Application.isMobilePlatform && THREADS) UnityEngine.Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;
            Shapes.Init(); Ui.Init(); CharSprites.Begin(); LoadWorld();
            cam = Camera.main;
            if (cam == null) { var go = new GameObject("Camera"); cam = go.AddComponent<Camera>(); go.tag = "MainCamera"; }
            cam.orthographic = true; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Shapes.Hex("#72bd55");
            cam.nearClipPlane = -100; cam.farClipPlane = 100; cam.transform.position = new Vector3(C.x, -C.y, -10);
            snd = gameObject.AddComponent<Sound>();
            view = new WorldView(transform);
            G = new Siege(true); // trận nền cho màn hình chính: tự đánh
            G.intro = 0;
            view.BuildStatic(G);
            camPos = new Vector2(C.x, C.y); zoom = 0.3f;
        }

        void Start() { if (!THREADS) StartCoroutine(LoadWeb()); }

        /// <summary>Bản web: dựng tạo hình và âm thanh rải qua nhiều khung hình, có màn chờ.</summary>
        System.Collections.IEnumerator LoadWeb()
        {
            loadK = 0; loadMsg = "Đang vẽ nhân vật"; yield return null; yield return null;
            var a = CharSprites.BuildSteps(); while (a.MoveNext()) { loadK = a.Current * 0.5f; yield return null; }
            loadMsg = "Đang tạo âm thanh và nhạc"; yield return null;
            var b = snd.BuildSteps(); while (b.MoveNext()) { loadK = 0.5f + b.Current * 0.5f; yield return null; }
            loadK = 1;
        }

        void DrawLoading()
        {
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.55f));
            var card = new Rect(RW / 2 - 260, 250, 520, 200); Ui.Box(card, new Color(1, 1, 1, 0.95f));
            Ui.Text(new Rect(card.x, card.y + 26, card.width, 50), "PHÀM NHÂN", 40, Ui.INK, TextAnchor.MiddleCenter, true);
            Ui.Text(new Rect(card.x, card.y + 86, card.width, 30), loadMsg + "...", 18, Ui.SUB, TextAnchor.MiddleCenter);
            Ui.Bar(new Rect(card.x + 40, card.y + 136, card.width - 80, 10), loadK, Ui.JADE);
        }

        void StartGame(bool auto, BattleCfg cfg = null)
        {
            if (cfg == null && campaign && G != null) cfg = G.cfg; // chơi lại cùng cứ điểm
            campResolved = false;
            autoMode = auto; G = new Siege(auto, cfg); scr = Screen_.Play; zoom = Z0; camPos = new Vector2(C.x - 600, C.y);
            joyId = -1; slashId = -1;
        }

        // ================= vòng lặp =================
        void Update()
        {
            float dt = Mathf.Min(1f / 30f, Time.deltaTime);
            CharSprites.Poll();
            if (scr == Screen_.Menu || scr == Screen_.Help || scr == Screen_.Roster || scr == Screen_.World)
            {
                menuT += dt; G.Update(dt); G.snd.Clear(); snd.SetTrack(Sound.Track.Menu);
                if (G.over != null && G.overT > 4) { G = new Siege(true); G.intro = 0; }
                float a = menuT * 0.05f; var target = new Vector2(C.x + Mathf.Cos(a) * 500, C.y + Mathf.Sin(a) * 300);
                camPos = Vector2.Lerp(camPos, target, dt); zoom = Mathf.Lerp(zoom, 0.32f, dt);
                return;
            }
            if (scr == Screen_.Pause) return;
            if (campaign && !campResolved && G.over != null) { campResolved = true; W.Resolve(campNode, G.over == "win"); SaveWorld(); }
            ReadInput();
            float scale = (!G.AUTO && G.stratOn) ? 0.3f : 1f;
            G.Update(dt * scale);
            CamUpdate(dt);
            PlaySounds();
        }

        void LateUpdate()
        {
            bool strat = scr == Screen_.Play && StratView();
            float sz = 360f / zoom;
            cam.orthographicSize = sz;
            float sh = G.shake * 0.6f; var off = sh > 0.3f ? Random.insideUnitCircle * sh : Vector2.zero;
            cam.transform.position = new Vector3(camPos.x + off.x, -(camPos.y + off.y), -10);
            float vw = sz * 2 * cam.aspect, vh2 = sz * 2;
            view.Render(G, Time.time, strat || scr == Screen_.Menu || scr == Screen_.Help || scr == Screen_.Roster || scr == Screen_.World, new Rect(camPos.x - vw / 2, camPos.y - vh2 / 2, vw, vh2));
        }

        void PlaySounds()
        {
            float halfW = cam.orthographicSize * cam.aspect;
            foreach (var e in G.snd)
            {
                float d = Vector2.Distance(camPos, new Vector2(e.x, e.y));
                float near = Mathf.Clamp01(1 - (d - halfW * 0.7f) / (halfW * 1.3f));
                snd.Play(e.id, near);
            }
            G.snd.Clear();
            snd.SetTrack(G.over == "lose" ? Sound.Track.None : (G.phase >= 2 || G.boss.active) ? Sound.Track.Climax : Sound.Track.Battle);
        }

        bool StratView() { return G.intro > 4.7f || G.stratOn || (G.AUTO && G.strat != null); }

        void CamUpdate(float dt)
        {
            float tx, ty;
            var h = G.hero;
            if (G.intro > 0)
            {
                float k = Mathf.Clamp01(1 - (G.intro - 0.7f) / 4.0f); float e = k * k * (3 - 2 * k); float sx = C.x - 600;
                tx = sx + (h.x - sx) * e; ty = C.y + (h.y - 40 - C.y) * e; zoom = Z0 * Mathf.Pow(Z_FIGHT / Z0, e);
                camPos = new Vector2(tx, ty); return;
            }
            if (StratView())
            {
                tx = C.x - 400; ty = C.y; zoom += (Z0 * 1.05f - zoom) * Mathf.Min(1, dt * 3);
            }
            else if (G.over == "win" && G.overT > 0.6f) { tx = C.x; ty = C.y; zoom += (0.42f - zoom) * Mathf.Min(1, dt * 0.8f); }
            else
            {
                bool fly = h.flyT > 0.8f && h.moving; float z = fly ? Z_FLY : Z_FIGHT;
                // màn hình điện thoại nhỏ: lùi máy quay một chút cho dễ nhìn
                if (UnityEngine.Screen.dpi > 0 && UnityEngine.Screen.height / UnityEngine.Screen.dpi < 3.2f) z *= 0.82f;
                tx = h.x + Mathf.Cos(h.face) * (fly ? 120 : 60); ty = h.y - 40 + Mathf.Sin(h.face) * (fly ? 80 : 30);
                zoom += (z - zoom) * Mathf.Min(1, dt * 1.4f);
            }
            float aspect = (float)UnityEngine.Screen.width / UnityEngine.Screen.height;
            float vh = VH / zoom, vw = vh * aspect;
            float cx = Mathf.Clamp(tx, Mathf.Min(vw / 2, WW / 2), Mathf.Max(WW - vw / 2, WW / 2)), cy = Mathf.Clamp(ty, Mathf.Min(vh / 2, WH / 2), Mathf.Max(WH - vh / 2, WH / 2));
            float kk = Mathf.Min(1, dt * (G.over != null ? 1.5f : StratView() ? 3 : 4));
            camPos += (new Vector2(cx, cy) - camPos) * kk;
        }

        // ================= điều khiển =================
        class Btn { public string id; public Rect r; public bool round; }
        readonly List<Btn> btns = new List<Btn>();
        int joyId = -1, slashId = -1; Vector2 joyBase, joyNow;
        float S => UnityEngine.Screen.height / 720f;
        float RW => UnityEngine.Screen.width / S;
        Vector2 ToGui(Vector2 p) { return new Vector2(p.x / S, (UnityEngine.Screen.height - p.y) / S); }
        Rect Safe()
        {
            var sa = UnityEngine.Screen.safeArea; float s = S;
            return new Rect(sa.x / s, (UnityEngine.Screen.height - sa.yMax) / s, sa.width / s, sa.height / s);
        }

        Btn Hit(Vector2 g)
        {
            for (int i = btns.Count - 1; i >= 0; i--)
            {
                var b = btns[i];
                if (b.round) { var c = b.r.center; float rr = b.r.width / 2 + 10; if ((g - c).sqrMagnitude < rr * rr) return b; }
                else if (b.r.Contains(g)) return b;
            }
            return null;
        }

        void ReadInput()
        {
            var IN = G.IN; bool slash = false;
            // cảm ứng nhiều ngón
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i); var g = ToGui(t.position);
                if (t.phase == TouchPhase.Began)
                {
                    var b = Hit(g);
                    if (b != null) { if (b.id == "slash") slashId = t.fingerId; else Press(b.id); }
                    else if (joyId < 0 && g.x < RW * 0.5f && !G.stratOn) { joyId = t.fingerId; joyBase = g; joyNow = g; G.AUTO = false; SkipIntro(); }
                }
                else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) { if (t.fingerId == joyId) joyNow = g; }
                else { if (t.fingerId == joyId) joyId = -1; if (t.fingerId == slashId) slashId = -1; }
                if (t.fingerId == slashId) slash = true;
            }
            // chuột (máy tính, trình soạn Unity)
            if (Input.touchCount == 0)
            {
                var g = ToGui(Input.mousePosition);
                if (Input.GetMouseButtonDown(0))
                {
                    var b = Hit(g);
                    if (b != null) { if (b.id == "slash") slashId = 99; else Press(b.id); }
                    else if (g.x < RW * 0.5f && !G.stratOn) { joyId = 99; joyBase = g; joyNow = g; G.AUTO = false; SkipIntro(); }
                }
                if (Input.GetMouseButton(0)) { if (joyId == 99) joyNow = g; if (slashId == 99) slash = true; }
                if (Input.GetMouseButtonUp(0)) { if (joyId == 99) joyId = -1; if (slashId == 99) slashId = -1; }
            }
            // bàn phím
            float kx = 0, ky = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) kx--; if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) kx++;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ky--; if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ky++;
            if (Input.anyKeyDown) SkipIntro();
            if ((kx != 0 || ky != 0) && G.AUTO) G.AUTO = false;
            if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Space)) slash = true;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Press("yu"); if (Input.GetKeyDown(KeyCode.Alpha2)) Press("loi"); if (Input.GetKeyDown(KeyCode.Alpha3)) Press("van"); if (Input.GetKeyDown(KeyCode.Alpha4)) Press("yan");
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.L)) Press("don"); if (Input.GetKeyDown(KeyCode.H)) Press("dan");
            if (Input.GetKeyDown(KeyCode.T)) Press("ho"); if (Input.GetKeyDown(KeyCode.F)) Press("pha"); if (Input.GetKeyDown(KeyCode.G)) Press("an");
            if (Input.GetKeyDown(KeyCode.Q)) Press("m_auto"); if (Input.GetKeyDown(KeyCode.E)) Press("m_split"); if (Input.GetKeyDown(KeyCode.R)) Press("m_mass");
            if (Input.GetKeyDown(KeyCode.M)) Press("map"); if (Input.GetKeyDown(KeyCode.Escape)) Press("pause");

            if (G.AUTO) return;
            IN.mx = kx; IN.my = ky;
            if (joyId >= 0)
            {
                var d = joyNow - joyBase; float m = 70; if (d.magnitude > m) { joyBase = joyNow - d.normalized * m; d = joyNow - joyBase; }
                IN.mx = d.x / m; IN.my = d.y / m; if (d.magnitude < 6) { IN.mx = 0; IN.my = 0; }
            }
            float mm = Mathf.Sqrt(IN.mx * IN.mx + IN.my * IN.my); if (mm > 1) { IN.mx /= mm; IN.my /= mm; }
            IN.slash = slash;
        }

        void SkipIntro() { if (G.intro > 0) G.intro = Mathf.Min(G.intro, 0.3f); }

        void Press(string id)
        {
            if (scr != Screen_.Play && scr != Screen_.Pause && !id.StartsWith("menu_")) return;
            if (id.StartsWith("m_") || id == "map" || id == "autob" || id == "pause" || id == "again" || id == "menu") snd.Play("tap");
            switch (id)
            {
                case "yu": Manual(); G.CastYu(); break;
                case "loi": Manual(); G.CastLoi(); break;
                case "van": Manual(); G.CastVan(); break;
                case "yan": Manual(); G.CastYan(); break;
                case "don": Manual(); G.CastDon(); break;
                case "dan": Manual(); G.UseDan(); break;
                case "ho": Manual(); G.PlaceHo(); break;
                case "pha": Manual(); G.UsePha(); break;
                case "an": Manual(); G.CastAn(); break;
                case "m_auto": G.SetMode("auto"); break;
                case "m_split": G.SetMode("split"); break;
                case "m_mass": G.SetMode("mass"); break;
                case "map": G.stratOn = !G.stratOn; SkipIntro(); break;
                case "autob": G.AUTO = !G.AUTO; break;
                case "pause": scr = scr == Screen_.Pause ? Screen_.Play : Screen_.Pause; break;
                case "again": StartGame(autoMode); break;
                case "menu": GoMenu(); break;
            }
        }
        void Manual() { SkipIntro(); if (G.AUTO) G.AUTO = false; }
        void GoMenu() { campaign = false; scr = Screen_.Menu; G = new Siege(true); G.intro = 0; }

        // ================= giao diện =================
        void OnGUI()
        {
            float s = S; GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            if (Event.current.type == EventType.Layout) return;
            if (Event.current.type == EventType.Repaint) btns.Clear();
            var sa = Safe();
            if (loadK < 1) { DrawLoading(); return; }
            switch (scr)
            {
                case Screen_.Menu: DrawMenu(sa); break;
                case Screen_.Help: DrawHelp(sa); break;
                case Screen_.Roster: DrawRoster(sa); break;
                case Screen_.World: DrawWorldMap(sa); break;
                default: DrawWorldLabels(); DrawHud(sa); if (scr == Screen_.Pause) DrawPause(sa); break;
            }
            // nút trên màn hình chính, trợ giúp, tạm dừng dùng sự kiện chuột/chạm IMGUI
        }

        bool MenuButton(Rect r, string label, bool primary)
        {
            Ui.Box(r, primary ? Ui.JADE : new Color(1, 1, 1, 0.92f));
            Ui.Text(r, label, 26, primary ? Color.white : Ui.INK, TextAnchor.MiddleCenter, true);
            var e = Event.current;
            if ((e.type == EventType.MouseDown) && r.Contains(e.mousePosition)) { e.Use(); snd.Play("tap"); return true; }
            return false;
        }

        void DrawMenu(Rect sa)
        {
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.35f));
            float cx = RW / 2;
            var card = new Rect(cx - 300, 110, 600, 500);
            Ui.Box(card, new Color(1, 1, 1, 0.9f));
            Ui.Text(new Rect(card.x, card.y + 30, card.width, 70), "PHÀM NHÂN", 56, Ui.INK, TextAnchor.MiddleCenter, true);
            Ui.Text(new Rect(card.x, card.y + 98, card.width, 30), "Năm mươi cứ điểm · năm tướng đấu năm tướng", 22, Ui.SUB, TextAnchor.MiddleCenter);
            if (MenuButton(new Rect(cx - 220, card.y + 146, 440, 74), "Chinh chiến thiên hạ", true)) OpenWorld();
            if (MenuButton(new Rect(cx - 220, card.y + 234, 216, 60), "Trận nhanh", false)) { campaign = false; StartGame(false); }
            if (MenuButton(new Rect(cx + 4, card.y + 234, 216, 60), "Nhân vật", false)) { scr = Screen_.Roster; rosterT = 0; }
            if (MenuButton(new Rect(cx - 220, card.y + 306, 216, 60), "Xem tự đánh", false)) { campaign = false; StartGame(true); }
            if (MenuButton(new Rect(cx + 4, card.y + 306, 216, 60), "Cách chơi", false)) scr = Screen_.Help;
            Ui.Text(new Rect(card.x, card.y + 400, card.width, 60), "Bản thử nghiệm Unity 4 · bản đồ 50 cứ điểm", 18, Ui.SUB, TextAnchor.MiddleCenter);
            SoundToggles(new Rect(sa.xMax - 300, sa.yMax - 64, 288, 52));
        }

        void SoundToggles(Rect r)
        {
            float w = (r.width - 8) / 2;
            if (MenuButtonSmall(new Rect(r.x, r.y, w, r.height), snd.MusicOn ? "Nhạc: bật" : "Nhạc: tắt", snd.MusicOn)) snd.MusicOn = !snd.MusicOn;
            if (MenuButtonSmall(new Rect(r.x + w + 8, r.y, w, r.height), snd.SfxOn ? "Âm thanh: bật" : "Âm thanh: tắt", snd.SfxOn)) snd.SfxOn = !snd.SfxOn;
        }
        bool MenuButtonSmall(Rect r, string label, bool on)
        {
            Ui.Box(r, on ? Ui.JADE : new Color(1, 1, 1, 0.92f));
            Ui.Text(r, label, 18, on ? Color.white : Ui.INK, TextAnchor.MiddleCenter, true);
            var e = Event.current;
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) { e.Use(); snd.Play("tap"); return true; }
            return false;
        }

        void DrawHelp(Rect sa)
        {
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.45f));
            var card = new Rect(RW / 2 - 470, 40, 940, 640);
            Ui.Box(card, new Color(1, 1, 1, 0.95f));
            Ui.Text(new Rect(card.x + 36, card.y + 24, 800, 44), "Cách chơi", 34, Ui.INK, TextAnchor.UpperLeft, true);
            string body =
                "<b>Chinh chiến thiên hạ</b>: bản đồ 50 cứ điểm (30 thôn trấn, 10 thành trì, 5 đại thành, 5 thánh địa). Chỉ đánh được cứ điểm kề lãnh thổ Chính đạo, cấp càng cao thủ quân càng đông và mạnh. Sau mỗi trận Ma đạo đi một lượt. Giữ cả năm Thánh địa 3 ngày để thắng mùa.\n\n" +
                "<b>Mục tiêu trận</b>: phá trận nhãn để mở kết giới, vào thành hạ Trấn thủ Huyết Sát Phó Tướng, rồi giữ đài trung tâm 15 giây. Có 15 phút.\n\n" +
                "<b>Trận nhãn ẩn</b>: bốn hướng có bốn trận nhãn ẩn nuôi bốn cung kết giới. Đến gần thấy linh khí dao động thì bấm <b>眼 Phá Vọng Nhãn</b> để soi. Ấn bảo hộ chỉ vỡ bằng hành khắc: Kim cần Hỏa (Lôi Hỏa Phù), Thủy cần Thổ (Trấn Sơn Ấn), Hỏa cần Thủy (Thủy Nguyệt), Mộc cần Kim (Ngự Kiếm). Phá Trận Phù phá được mọi ấn. Ấn vỡ hết thì lõi lộ ra 9 giây.\n\n" +
                "<b>Điều khiển</b>: kéo nửa trái màn hình để đi. Giữ nút 劍 lớn để chém. Bốn nút quanh là Ngự Kiếm, Lôi Hỏa Phù, Vạn Kiếm (đứng trong Hộ Sơn Đại Trận), Phá Vọng Nhãn. Nút nhỏ: 遁 độn thuật, 丹 uống đan. Hàng trên: Hộ Sơn Đại Trận, Phá Trận Phù, Trấn Sơn Ấn.\n\n" +
                "<b>Thế trận</b>: Tự quyết để quân sư chọn, Phân binh đánh ba hướng, Hợp binh để bốn tướng theo hướng bạn đang đánh. Nút Sa bàn xem toàn cục.\n\n" +
                "Trên máy tính: WASD đi, J chém, 1 2 3 4 kỹ năng, Shift độn, T F G trận pháp, H đan, Q E R thế trận, M sa bàn.";
            Ui.Text(new Rect(card.x + 36, card.y + 80, card.width - 72, 480), body, 16, Ui.INK, TextAnchor.UpperLeft, false, true);
            if (MenuButton(new Rect(card.x + card.width / 2 - 160, card.yMax - 84, 320, 60), "Đã hiểu", true)) scr = Screen_.Menu;
        }

        void DrawPause(Rect sa)
        {
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.5f));
            float cx = RW / 2; var card = new Rect(cx - 240, 140, 480, 420); Ui.Box(card, new Color(1, 1, 1, 0.95f));
            Ui.Text(new Rect(card.x, card.y + 24, card.width, 50), "Tạm dừng", 34, Ui.INK, TextAnchor.MiddleCenter, true);
            if (MenuButton(new Rect(cx - 180, card.y + 100, 360, 64), "Tiếp tục", true)) scr = Screen_.Play;
            if (MenuButton(new Rect(cx - 180, card.y + 180, 360, 60), "Chơi lại", false)) StartGame(autoMode);
            if (MenuButton(new Rect(cx - 180, card.y + 256, 360, 60), campaign ? "Rút quân về bản đồ" : "Về màn hình chính", false)) { if (campaign) OpenWorld(); else GoMenu(); }
            SoundToggles(new Rect(cx - 180, card.y + 328, 360, 52));
        }

        void DrawWorldLabels()
        {
            bool strat = StratView();
            Vector2 W2G(float x, float y) { var p = cam.WorldToScreenPoint(new Vector3(x, -y, 0)); return new Vector2(p.x / S, (UnityEngine.Screen.height - p.y) / S); }
            float px = 1f / (cam.orthographicSize * 2 / 720f); // điểm giao diện trên một đơn vị bản đồ
            // chữ trên đầu tướng
            foreach (var u in G.units)
            {
                if (u.dead || !(u.champ || u == G.boss)) continue;
                var p = W2G(u.x, u.y);
                if (p.x < -60 || p.x > RW + 60 || p.y < -60 || p.y > 780) continue;
                if (!strat && CharSprites.Get(u.t) != null)
                {
                    var sp = CharSprites.Get(u.t).spec; var tp = W2G(u.x, u.y + u.r * 0.55f - u.r * WorldView.Side(u.t) * (sp.ax1 - sp.ax0) / 120f * 0.92f);
                    if (px > 0.5f) Ui.Text(new Rect(tp.x - 90, tp.y - 26, 180, 20), u == G.hero ? "Ngao Thiên (bạn)" : u.T.name, 12, Color.white, TextAnchor.LowerCenter, true, false, true);
                    continue;
                }
                float rr = Mathf.Max(9, u.r * 1.2f * px * (strat ? 3.2f : 1));
                Ui.Text(new Rect(p.x - 40, p.y - rr - 2, 80, rr * 2), u.D.g, Mathf.RoundToInt(rr * 1.35f), Color.white, TextAnchor.MiddleCenter, true, false, true);
                if (strat || px > 0.5f) Ui.Text(new Rect(p.x - 90, p.y + rr + (strat ? 4 : 10), 180, 22), u == G.hero ? "Ngao Thiên (bạn)" : u.T.name, strat ? 15 : 13, Color.white, TextAnchor.UpperCenter, true, false, true);
            }
            if (strat)
            {
                foreach (var e in G.eyes)
                {
                    var p = W2G(e.x, e.y);
                    string st = e.dead ? "đã phá" : !e.revealed ? "chưa soi ra" : e.seals > 0 ? e.seals + " ấn" : "lõi lộ";
                    Ui.Text(new Rect(p.x - 110, p.y - 64, 220, 22), "Hướng " + DIRS[e.i].n + " · nhãn " + e.name, 15, Color.white, TextAnchor.MiddleCenter, true, false, true);
                    Ui.Text(new Rect(p.x - 110, p.y - 44, 220, 22), st, 14, new Color(1, 1, 1, 0.85f), TextAnchor.MiddleCenter, false, false, true);
                }
                return;
            }
            foreach (var f in G.floats)
            {
                var p = W2G(f.x, f.y); float a = Mathf.Clamp01(f.t / f.max * 1.6f);
                Ui.Text(new Rect(p.x - 160, p.y - 14, 320, 28), f.s, Mathf.RoundToInt(f.size * 1.1f), Shapes.C(f.col, a), TextAnchor.MiddleCenter, true, false, true);
            }
            foreach (var f in G.fx)
            {
                if (f.type != FxT.Glyph) continue; var p = W2G(f.x, f.y); float k = Mathf.Clamp01(f.t / f.max);
                Ui.Text(new Rect(p.x - 60, p.y - 40 - (1 - k) * 20, 120, 80), f.g, Mathf.RoundToInt(44 * Mathf.Max(0.5f, px)), Shapes.C(f.col, k), TextAnchor.MiddleCenter, true, false, true);
            }
        }

        // ---------- HUD trong trận ----------
        void AddBtn(string id, Rect r, bool round) { if (Event.current.type == EventType.Repaint) btns.Add(new Btn { id = id, r = r, round = round }); }

        void SkillBtn(string id, Vector2 c, float r, string glyph, string label, Color col, float cd, float cdMax, bool ok, string badge = null)
        {
            AddBtn(id, new Rect(c.x - r, c.y - r, r * 2, r * 2), true);
            Ui.Circle(c, r + 3, new Color(1, 1, 1, 0.95f));
            Ui.Circle(c, r, ok ? col : Color.Lerp(col, new Color(0.6f, 0.65f, 0.63f), 0.65f));
            Ui.Text(new Rect(c.x - r, c.y - r - 2, r * 2, r * 2), glyph, Mathf.RoundToInt(r * 0.95f), Color.white, TextAnchor.MiddleCenter, true, false, true);
            if (cd > 0 && cdMax > 0)
            {
                float k = Mathf.Clamp01(cd / cdMax);
                Ui.Circle(c, r, new Color(0.06f, 0.16f, 0.13f, 0.35f + 0.25f * k));
                Ui.Text(new Rect(c.x - r, c.y - r, r * 2, r * 2), Mathf.CeilToInt(cd).ToString(), Mathf.RoundToInt(r * 0.7f), Color.white, TextAnchor.MiddleCenter, true, false, true);
            }
            if (label != null) Ui.Text(new Rect(c.x - 70, c.y + r + 2, 140, 20), label, 13, Color.white, TextAnchor.UpperCenter, true, false, true);
            if (badge != null) { var bc = new Vector2(c.x + r * 0.72f, c.y - r * 0.72f); Ui.Circle(bc, 13, Ui.JADE); Ui.Text(new Rect(bc.x - 13, bc.y - 13, 26, 26), badge, 14, Color.white, TextAnchor.MiddleCenter, true); }
        }

        void SmallBtn(string id, Rect r, string label, bool on)
        {
            AddBtn(id, r, false);
            Ui.Box(r, on ? Ui.JADE : new Color(1, 1, 1, 0.88f));
            Ui.Text(r, label, 15, on ? Color.white : Ui.INK, TextAnchor.MiddleCenter, true);
        }

        void DrawHud(Rect sa)
        {
            var g = G; var h = g.hero; bool strat = StratView();
            float L = sa.x + 12, Rr = sa.xMax - 12, T = sa.y + 10;
            var panelC = new Color(1, 1, 1, 0.88f);

            // ---- trên phải: tạm dừng, sa bàn, tự đánh ----
            SmallBtn("pause", new Rect(Rr - 64, T, 64, 46), "II", false);
            SmallBtn("map", new Rect(Rr - 64 - 120, T, 112, 46), g.stratOn ? "Đóng sa bàn" : "Sa bàn", g.stratOn);
            SmallBtn("autob", new Rect(Rr - 64 - 120 - 120, T, 112, 46), g.AUTO ? "Đang tự đánh" : "Tự đánh", g.AUTO);

            // ---- trên trái: mục tiêu ----
            var obj = new Rect(L, T, 330, 108); Ui.Box(obj, panelC);
            string ph = g.phase == 1 ? "Phá trận nhãn, mở cửa" : g.phase == 2 ? "Hạ Trấn thủ, diệt thủ quân" : "Giữ đài trung tâm";
            Ui.Text(new Rect(obj.x + 14, obj.y + 8, 220, 24), "Giai đoạn " + g.phase + " · " + ph, 15, Ui.INK, TextAnchor.UpperLeft, true);
            Ui.Text(new Rect(obj.xMax - 90, obj.y + 6, 78, 26), Ui.Clock(g.time), 20, g.time < 120 ? Ui.FOE : Ui.INK, TextAnchor.UpperRight, true);
            for (int i = 0; i < 4; i++)
            {
                var e = g.eyes[i]; var c = new Vector2(obj.x + 34 + i * 76, obj.y + 58);
                var col = Shapes.C(ELEM[e.el].col);
                Ui.Circle(c, 17, e.dead ? new Color(0.75f, 0.78f, 0.77f) : e.revealed ? col : new Color(0.85f, 0.88f, 0.87f));
                Ui.Text(new Rect(c.x - 17, c.y - 17, 34, 34), e.dead ? "✓" : e.revealed ? ELEM[e.el].g : "?", 17, e.dead ? Ui.JADE : Color.white, TextAnchor.MiddleCenter, true, false, !e.dead);
                string st = e.dead ? "đã phá" : !e.revealed ? DIRS[i].n : e.seals > 0 ? e.seals + " ấn" : "lõi";
                Ui.Text(new Rect(c.x - 38, c.y + 18, 76, 18), st, 12, Ui.SUB, TextAnchor.UpperCenter);
            }
            if (g.phase >= 2)
            {
                string pr = g.phase == 2 ? "Thủ quân " + Mathf.Min(g.need, g.killed - g.k0) + "/" + g.need + (g.boss.dead ? " · Trấn thủ đã ngã" : "") : "Cờ " + g.flag.ToString("0") + "/" + g.flagNeed + (g.contested ? " · đang bị tranh" : "");
                Ui.Text(new Rect(obj.x + 14, obj.yMax - 20, 300, 18), pr, 12, Ui.JADE, TextAnchor.UpperLeft, true);
            }

            // ---- trái: năm tướng Chính đạo + thế trận ----
            var team = new Rect(L, obj.yMax + 8, 250, 186); Ui.Box(team, panelC);
            Ui.Text(new Rect(team.x + 12, team.y + 6, 220, 18), "CHÍNH ĐẠO · NĂM TƯỚNG", 12, Ui.ALLY, TextAnchor.UpperLeft, true);
            for (int i = 0; i < 5; i++)
            {
                var a = g.allies[i]; float y = team.y + 28 + i * 23;
                if (!Portrait(new Rect(team.x + 9, y - 3, 24, 24), a.t, a.dead)) Ui.Text(new Rect(team.x + 10, y - 2, 22, 22), a.D.g, 16, Shapes.C(a.D.col), TextAnchor.UpperCenter, true);
                Ui.Text(new Rect(team.x + 36, y, 120, 18), a == h ? "Ngao Thiên (bạn)" : a.T.name, 13, a.dead ? Ui.SUB : Ui.INK, TextAnchor.UpperLeft, true);
                string where = a.dead ? "hồi " + Mathf.CeilToInt(a.respawn) + "s" : a.asg >= 0 ? "→ " + DIRS[a.asg].n : g.open >= 0 ? "→ cửa " + DIRS[g.open].n : "";
                Ui.Text(new Rect(team.xMax - 82, y, 72, 18), where, 12, Ui.SUB, TextAnchor.UpperRight);
                Ui.Bar(new Rect(team.x + 36, y + 17, team.width - 48, 3), a.hp / a.max, Shapes.Hex("#1e9b4f"));
            }
            float my = team.y + 148; float bw = (team.width - 24) / 3f;
            SmallBtn("m_auto", new Rect(team.x + 8, my, bw - 2, 32), "Tự quyết", g.mode == "auto");
            SmallBtn("m_split", new Rect(team.x + 10 + bw, my, bw - 2, 32), "Phân binh", g.mode == "split");
            SmallBtn("m_mass", new Rect(team.x + 12 + bw * 2, my, bw - 2, 32), "Hợp binh", g.mode == "mass");

            // ---- phải: năm tướng Ma đạo ----
            var foe = new Rect(Rr - 230, T + 56, 230, 140); Ui.Box(foe, panelC);
            Ui.Text(new Rect(foe.x + 12, foe.y + 6, 210, 18), "MA ĐẠO · NĂM TƯỚNG", 12, Ui.FOE, TextAnchor.UpperLeft, true);
            for (int i = 0; i < 5; i++)
            {
                var m = g.foes[i]; float y = foe.y + 28 + i * 22;
                if (!Portrait(new Rect(foe.x + 7, y - 3, 24, 24), m.t, m.dead)) Ui.Text(new Rect(foe.x + 8, y - 2, 22, 22), m.D.g, 15, Shapes.C(m.D.col), TextAnchor.UpperCenter, true);
                Ui.Text(new Rect(foe.x + 32, y, 120, 18), m.T.name, 12, m.dead ? Ui.SUB : Ui.INK, TextAnchor.UpperLeft, true);
                string where = m.dead ? "đã ngã" : m == g.boss ? (m.active ? "xuất trận" : "nội thành") : m.asg == Unit.BREACH ? "giữ cửa" : m.asg >= 0 ? DIRS[m.asg].n : "";
                Ui.Text(new Rect(foe.xMax - 78, y, 70, 18), where, 11, Ui.SUB, TextAnchor.UpperRight);
                Ui.Bar(new Rect(foe.x + 32, y + 16, foe.width - 44, 3), m.hp / m.max, Ui.FOE);
            }

            // ---- giữa trên: Trấn thủ, thông báo, cảnh báo ----
            float cx = (sa.x + sa.xMax) / 2;
            float topY = T + 4;
            if (g.boss.active && !g.boss.dead)
            {
                var bb = new Rect(cx - 220, topY, 440, 40); Ui.Box(bb, panelC);
                Ui.Text(new Rect(bb.x + 12, bb.y + 3, 300, 18), "煞 Huyết Sát Phó Tướng · Kết Đan", 13, Ui.FOE, TextAnchor.UpperLeft, true);
                Ui.Bar(new Rect(bb.x + 12, bb.y + 24, bb.width - 24, 8), g.boss.hp / g.boss.max, Ui.FOE);
                topY = bb.yMax + 6;
            }
            if (strat && (g.strat != null || g.intro > 0 || g.stratOn) && g.plan != null)
            {
                string title = g.strat != null ? g.strat.title : g.plan.title, sub = g.strat != null ? g.strat.sub : g.plan.sub, gl = g.strat != null ? g.strat.g : g.plan.g;
                float w = Mathf.Min(720, RW - 560); var r = new Rect(cx - w / 2, topY, w, 30 + Ui.TextH(sub ?? "", 15, w - 80) + 14);
                Ui.Box(r, new Color(1, 1, 1, 0.93f));
                Ui.Text(new Rect(r.x + 14, r.y + 8, 40, 40), gl, 30, Ui.JADE, TextAnchor.UpperLeft, true);
                Ui.Text(new Rect(r.x + 60, r.y + 8, w - 74, 24), (g.intro > 0 ? g.Name + " · " : "") + title, 18, Ui.INK, TextAnchor.UpperLeft, true);
                Ui.Text(new Rect(r.x + 60, r.y + 34, w - 74, 80), sub ?? "", 15, Ui.SUB, TextAnchor.UpperLeft, false, true);
                topY = r.yMax + 6;
            }
            else if (g.banner != null && g.intro <= 0)
            {
                float a = Mathf.Clamp01(g.banner.t / 0.4f) * Mathf.Clamp01((g.banner.max - g.banner.t) / 0.3f + 0.2f);
                float w = Mathf.Min(620, RW - 600); var r = new Rect(cx - w / 2, topY + 60, w, 80);
                Ui.Box(r, new Color(1, 1, 1, 0.92f * a));
                if (g.banner.g != null) Ui.Text(new Rect(r.x + 16, r.y + 10, 60, 60), g.banner.g, 40, Shapes.C(new Rgb(16, 163, 127), a), TextAnchor.MiddleCenter, true);
                Ui.Text(new Rect(r.x + 84, r.y + 12, w - 96, 30), g.banner.title, 22, Shapes.C(new Rgb(23, 48, 42), a), TextAnchor.UpperLeft, true);
                Ui.Text(new Rect(r.x + 84, r.y + 44, w - 96, 30), g.banner.sub, 15, Shapes.C(new Rgb(74, 106, 96), a), TextAnchor.UpperLeft, false, true);
            }
            if (g.warnT > 0 && g.warnS != null && !strat)
            {
                float a = Mathf.Clamp01(g.warnT); float w = Mathf.Min(640, RW - 600);
                var r = new Rect(cx - w / 2, 250, w, 36); Ui.Box(r, new Color(0.9f, 0.28f, 0.24f, 0.9f * a));
                Ui.Text(r, g.warnS, 15, new Color(1, 1, 1, a), TextAnchor.MiddleCenter, true);
            }
            if (g.intro > 0) Ui.Text(new Rect(cx - 200, 640, 400, 30), "Chạm để vào trận", 18, Color.white, TextAnchor.MiddleCenter, true, false, true);

            if (g.over == null && !strat)
            {
                // ---- nhật ký chiến trường ----
                float fy = team.yMax + 10;
                int nF = g.feed.Count; for (int i = 0; i < nF; i++)
                {
                    var f = g.feed[i]; float age = g.t - f.t; if (age > 9) continue; float a = Mathf.Clamp01((9 - age) / 2f);
                    Ui.Text(new Rect(L + 4, fy, 380, 18), f.s, 13, new Color(1, 1, 1, a), TextAnchor.UpperLeft, true, false, true); fy += 18;
                }
                // ---- thanh máu, linh lực, thể lực của Ngao Thiên ----
                var hb = new Rect(cx - 170, 720 - 14 - 58 - (720 - sa.yMax), 340, 58);
                Ui.Box(hb, panelC);
                Ui.Text(new Rect(hb.x + 12, hb.y + 5, 200, 18), "Ngao Thiên · Trúc Cơ", 13, Ui.INK, TextAnchor.UpperLeft, true);
                Ui.Text(new Rect(hb.xMax - 130, hb.y + 5, 118, 18), M.Fmt(h.hp) + " / " + M.Fmt(h.max), 12, Ui.SUB, TextAnchor.UpperRight);
                Ui.Bar(new Rect(hb.x + 12, hb.y + 25, hb.width - 24, 9), h.hp / h.max, Shapes.Hex("#1e9b4f"));
                Ui.Bar(new Rect(hb.x + 12, hb.y + 38, hb.width - 24, 6), h.mp / h.mpMax, Ui.ALLY);
                Ui.Bar(new Rect(hb.x + 12, hb.y + 48, hb.width - 24, 4), h.st / 100f, Ui.GOLD);
                if (h.dead) Ui.Text(new Rect(cx - 200, hb.y - 40, 400, 30), "Trọng thương · trở lại sau " + Mathf.CeilToInt(h.respawn) + " giây", 18, Color.white, TextAnchor.MiddleCenter, true, false, true);

                // ---- cần gạt ----
                if (joyId >= 0)
                {
                    Ui.Circle(joyBase, 80, new Color(1, 1, 1, 0.25f)); Ui.Circle(joyBase, 80, new Color(1, 1, 1, 0.8f), true);
                    var d = joyNow - joyBase; if (d.magnitude > 70) d = d.normalized * 70; Ui.Circle(joyBase + d, 34, new Color(1, 1, 1, 0.9f));
                }
                else
                {
                    var jb = new Vector2(L + 130, sa.yMax - 130);
                    Ui.Circle(jb, 80, new Color(1, 1, 1, 0.14f)); Ui.Circle(jb, 80, new Color(1, 1, 1, 0.5f), true); Ui.Circle(jb, 30, new Color(1, 1, 1, 0.5f));
                    if (g.t < 40 && !g.AUTO) Ui.Text(new Rect(jb.x - 120, jb.y + 86, 240, 20), "Kéo ở nửa trái để di chuyển", 13, Color.white, TextAnchor.UpperCenter, true, false, true);
                }

                // ---- nút chiêu ----
                var sc = new Vector2(Rr - 96, sa.yMax - 100);
                SkillBtn("slash", sc, 66, "劍", null, Shapes.Hex("#d6a13a"), 0, 0, true);
                Vector2 At(float deg, float rad) { float a = deg * Mathf.Deg2Rad; return sc + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * rad; }
                SkillBtn("yu", At(180, 140), 38, "御", "Ngự Kiếm", Shapes.Hex("#c99a2a"), h.cdYu, 4, h.mp >= 40);
                SkillBtn("loi", At(145, 140), 38, "雷", "Lôi Hỏa Phù", Shapes.Hex("#e0602a"), h.cdLoi, 4, g.loi > 0, g.loi.ToString());
                SkillBtn("van", At(110, 140), 38, "萬", "Vạn Kiếm", Shapes.Hex("#0f8a6a"), h.cdVan, 28, h.mp >= 160 && g.InZone(h, "ho") != null);
                SkillBtn("yan", At(76, 150), 38, "眼", "Phá Vọng Nhãn", Shapes.Hex("#7c4dcc"), h.cdYan, 10, h.mp >= 30);
                SkillBtn("don", At(205, 240), 30, "遁", "Độn", Shapes.Hex("#1f7fd6"), h.cdDon, 1.8f, h.st >= 30);
                SkillBtn("dan", At(165, 245), 30, "丹", "Đan", Shapes.Hex("#1e9b4f"), g.danCd, 6, g.dan > 0, g.dan.ToString());
                // ---- trận pháp và bùa ----
                var ty = sc.y - 268; float tx0 = Rr - 330;
                TrayBtn("ho", new Rect(tx0, ty, 104, 52), "護", "Hộ Sơn Trận", g.hoCd, 24, true, null, Shapes.Hex("#1f7fd6"));
                TrayBtn("pha", new Rect(tx0 + 110, ty, 104, 52), "破", "Phá Trận Phù", g.phaCd, 1.2f, g.pha > 0, g.pha.ToString(), Shapes.Hex("#c9901a"));
                TrayBtn("an", new Rect(tx0 + 220, ty, 104, 52), "鎮", "Trấn Sơn Ấn", g.anCd, 3, g.an > 0, g.an.ToString(), Shapes.Hex("#a5672a"));
            }

            if (g.over != null && g.overT > 3.8f) DrawEnd(sa);
        }

        void TrayBtn(string id, Rect r, string glyph, string label, float cd, float cdMax, bool ok, string badge, Color col)
        {
            AddBtn(id, r, false);
            Ui.Box(r, new Color(1, 1, 1, 0.9f));
            Ui.Circle(new Vector2(r.x + 24, r.center.y), 17, ok ? col : new Color(0.7f, 0.74f, 0.72f));
            Ui.Text(new Rect(r.x + 7, r.center.y - 17, 34, 34), glyph, 17, Color.white, TextAnchor.MiddleCenter, true);
            Ui.Text(new Rect(r.x + 44, r.y + 6, r.width - 48, 40), label, 12, ok ? Ui.INK : Ui.SUB, TextAnchor.MiddleLeft, true, true);
            if (cd > 0) { Ui.Box(r, new Color(0.06f, 0.16f, 0.13f, 0.35f)); Ui.Text(r, Mathf.CeilToInt(cd) + "s", 18, Color.white, TextAnchor.MiddleCenter, true, false, true); }
            if (badge != null) { var bc = new Vector2(r.xMax - 4, r.y + 4); Ui.Circle(bc, 12, Ui.JADE); Ui.Text(new Rect(bc.x - 12, bc.y - 12, 24, 24), badge, 13, Color.white, TextAnchor.MiddleCenter, true); }
        }

        void DrawEnd(Rect sa)
        {
            var g = G; bool win = g.over == "win";
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.45f));
            float cx = RW / 2; var card = new Rect(cx - 300, 120, 600, 480); Ui.Box(card, new Color(1, 1, 1, 0.95f));
            Ui.Text(new Rect(card.x, card.y + 26, card.width, 60), win ? "捷 Công phá " + g.Name : "Thủ quân giữ được " + g.Name, 32, win ? Ui.JADE : Ui.FOE, TextAnchor.MiddleCenter, true);
            string[] rows = {
                "Thời gian|" + Ui.Clock(TIME - g.time),
                "Quân địch bị diệt|" + g.killed,
                "Ngao Thiên hạ|" + g.heroKills,
                "Ma tướng bị hạ|" + g.maDown,
                "Quân ta tổn thất|" + g.lost,
                "Trận nhãn đã phá|" + g.eyes.Count(e => e.dead) + "/4",
            };
            for (int i = 0; i < rows.Length; i++)
            {
                var p = rows[i].Split('|'); float y = card.y + 110 + i * 38;
                Ui.Text(new Rect(card.x + 70, y, 300, 30), p[0], 20, Ui.SUB);
                Ui.Text(new Rect(card.xMax - 270, y, 200, 30), p[1], 20, Ui.INK, TextAnchor.UpperRight, true);
            }
            if (campaign)
            {
                if (MenuButton(new Rect(cx - 250, card.yMax - 96, 240, 64), "Về bản đồ", true)) OpenWorld();
                if (MenuButton(new Rect(cx + 10, card.yMax - 96, 240, 64), "Màn hình chính", false)) GoMenu();
            }
            else
            {
                if (MenuButton(new Rect(cx - 250, card.yMax - 96, 240, 64), "Chơi lại", true)) StartGame(autoMode);
                if (MenuButton(new Rect(cx + 10, card.yMax - 96, 240, 64), "Màn hình chính", false)) GoMenu();
            }
        }
    }
}
