using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PhamNhan.Art;

namespace PhamNhan.View
{
    /// <summary>Dựng ảnh tạo hình nhân vật ở luồng phụ lúc mở game, rồi tạo Sprite trên luồng chính.</summary>
    public static class CharSprites
    {
        public class Set
        {
            public Sprite[] f = new Sprite[Chars.FRAMES], m = new Sprite[Chars.FRAMES];
            public Texture2D face; public Rect faceUV; public Chars.Spec spec;
            public float cx, cy; // tâm ảnh so với chân, tính theo cạnh ảnh (x phải, y lên)
        }

        class Raw { public string key; public int px; public byte[][] f = new byte[Chars.FRAMES][]; }

        static Task<List<Raw>> task;
        static readonly Dictionary<string, Set> sets = new Dictionary<string, Set>();
        public static bool Ready { get; private set; }

        static int Px(string k)
        {
            switch (k)
            {
                case "boss": return 200;
                case "hero": case "viem": case "thach": case "nguyet": case "thanh": case "lang": case "thi": case "bo": case "ta": case "quy": return 168;
                case "loan": case "tuong": case "long": return 120;
                default: return 104;
            }
        }

        public static void Begin()
        {
            if (task != null) return;
            task = Task.Run(() =>
            {
                var list = new List<Raw>();
                foreach (var k in Chars.Keys)
                {
                    var r = new Raw { key = k, px = Px(k) };
                    for (int i = 0; i < Chars.FRAMES; i++) r.f[i] = Chars.Render(k, i, r.px).Bytes(true);
                    list.Add(r);
                }
                return list;
            });
        }

        /// <summary>Gọi mỗi khung hình cho tới khi xong.</summary>
        public static void Poll()
        {
            if (Ready || task == null || !task.IsCompleted) return;
            if (task.Status != TaskStatus.RanToCompletion) { Debug.LogWarning("Dựng tạo hình lỗi: " + task.Exception); task = null; return; }
            foreach (var r in task.Result)
            {
                var sp = Chars.Get(r.key); var s = new Set { spec = sp };
                float side = sp.ax1 - sp.ax0;
                s.cx = ((sp.ax0 + sp.ax1) / 2) / side; s.cy = (sp.ay0 + side / 2) / side;
                for (int i = 0; i < Chars.FRAMES; i++)
                {
                    s.f[i] = Make(r.f[i], r.px, false);
                    s.m[i] = Make(r.f[i], r.px, true);
                    if (i == 0) s.face = s.f[i].texture;
                }
                // khung chân dung quanh đầu (toạ độ uv, gốc ở dưới trái)
                float u0 = (sp.faceX - sp.faceR - sp.ax0) / side, v0 = (sp.face - sp.faceR - sp.ay0) / side, w = sp.faceR * 2 / side;
                s.faceUV = new Rect(u0, v0, w, w);
                sets[r.key] = s;
            }
            task = null; Ready = true;
        }

        static Sprite Make(byte[] src, int px, bool mask)
        {
            byte[] b = src;
            if (mask) { b = (byte[])src.Clone(); for (int i = 0; i < b.Length; i += 4) { b[i] = 255; b[i + 1] = 255; b[i + 2] = 255; } }
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Trilinear; tex.anisoLevel = 0;
            tex.SetPixelData(b, 0); tex.Apply(true, true);
            return Sprite.Create(tex, new Rect(0, 0, px, px), new Vector2(0.5f, 0.5f), px, 0, SpriteMeshType.FullRect);
        }

        public static Set Get(string key) { Set s; return Ready && sets.TryGetValue(key, out s) ? s : null; }

        /// <summary>Vẽ chân dung vào ô giao diện.</summary>
        public static bool Face(Rect r, string key)
        {
            var s = Get(key); if (s == null) return false;
            GUI.DrawTextureWithTexCoords(r, s.face, s.faceUV, true);
            return true;
        }
    }
}
