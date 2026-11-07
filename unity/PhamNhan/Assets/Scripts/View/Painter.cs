using System.Collections.Generic;
using UnityEngine;
using PhamNhan.Sim;

namespace PhamNhan.View
{
    /// <summary>Kho hình cơ bản sinh bằng mã: tròn mềm, vòng, ô vuông, lưỡi chém.</summary>
    public static class Shapes
    {
        public static Sprite Disc, Soft, Ring, Square, Arc, Diamond;
        public static Material LineMat;
        static bool ready;

        public static void Init()
        {
            if (ready) return; ready = true;
            Disc = Make(64, (x, y) => { float d = Mathf.Sqrt(x * x + y * y); return Mathf.Clamp01((1f - d) * 32f); });
            Soft = Make(64, (x, y) => { float d = Mathf.Sqrt(x * x + y * y); float k = Mathf.Clamp01(1f - d); return k * k; });
            Ring = Make(128, (x, y) => { float d = Mathf.Sqrt(x * x + y * y); return Mathf.Clamp01((0.06f - Mathf.Abs(d - 0.93f)) * 64f); });
            Square = Make(8, (x, y) => 1f, true);
            Arc = Make(128, (x, y) =>
            {
                float d = Mathf.Sqrt(x * x + y * y); float a = Mathf.Atan2(y, x);
                if (Mathf.Abs(a) > 1.2f) return 0; float w = 0.16f * (1f - Mathf.Abs(a) / 1.2f);
                return Mathf.Clamp01((w - Mathf.Abs(d - 0.8f)) * 40f);
            });
            Diamond = Make(64, (x, y) => Mathf.Clamp01((1f - (Mathf.Abs(x) + Mathf.Abs(y))) * 24f));
            var sh = Shader.Find("Sprites/Default");
            LineMat = new Material(sh);
        }

        delegate float Fn(float x, float y);
        static Sprite Make(int n, Fn f, bool point = false)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;
            var px = new Color32[n * n];
            for (int j = 0; j < n; j++) for (int i = 0; i < n; i++)
                {
                    float x = (i + 0.5f) / n * 2 - 1, y = (j + 0.5f) / n * 2 - 1;
                    byte a = (byte)(Mathf.Clamp01(f(x, y)) * 255);
                    px[j * n + i] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px); tex.Apply();
            // 1 đơn vị = đường kính hình
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        public static Color C(Rgb c, float a = 1) { return new Color(c.r / 255f, c.g / 255f, c.b / 255f, a); }
        public static Color Hex(string h, float a = 1) { var c = Rgb.Hex(h); return C(c, a); }
    }

    /// <summary>Vẽ kiểu tức thời: mỗi khung hình mượn lại các SpriteRenderer và LineRenderer từ kho.</summary>
    public class Painter
    {
        readonly Transform root;
        readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        readonly List<LineRenderer> lines = new List<LineRenderer>();
        int used, lused;

        public Painter(Transform parent) { root = new GameObject("Painter").transform; root.SetParent(parent, false); }

        public void Begin() { used = 0; lused = 0; }
        public void End()
        {
            for (int i = used; i < pool.Count; i++) if (pool[i].enabled) pool[i].enabled = false;
            for (int i = lused; i < lines.Count; i++) if (lines[i].enabled) lines[i].enabled = false;
        }

        /// <summary>Vẽ một hình tại toạ độ bản đồ (x phải, y xuống).</summary>
        public void Draw(Sprite s, float x, float y, float w, float h, Color col, int order, float rotRad = 0)
        {
            SpriteRenderer r;
            if (used < pool.Count) r = pool[used];
            else { var go = new GameObject("s"); go.transform.SetParent(root, false); r = go.AddComponent<SpriteRenderer>(); pool.Add(r); }
            used++;
            if (!r.enabled) r.enabled = true;
            if (r.sprite != s) r.sprite = s;
            r.color = col; r.sortingOrder = order;
            var tr = r.transform;
            tr.localPosition = new Vector3(x, -y, 0);
            tr.localScale = new Vector3(w, h, 1);
            tr.localRotation = rotRad == 0 ? Quaternion.identity : Quaternion.Euler(0, 0, -rotRad * Mathf.Rad2Deg);
        }
        public void Circle(float x, float y, float r, Color col, int order) { Draw(Shapes.Disc, x, y, r * 2, r * 2, col, order); }
        public void Glow(float x, float y, float r, Color col, int order) { Draw(Shapes.Soft, x, y, r * 2, r * 2, col, order); }
        public void RingAt(float x, float y, float r, Color col, int order) { Draw(Shapes.Ring, x, y, r * 2, r * 2, col, order); }
        public void Rect(float x, float y, float w, float h, Color col, int order, float rot = 0) { Draw(Shapes.Square, x, y, w, h, col, order, rot); }

        public LineRenderer Line(int n, float width, Color col, int order)
        {
            LineRenderer l;
            if (lused < lines.Count) l = lines[lused];
            else
            {
                var go = new GameObject("l"); go.transform.SetParent(root, false); l = go.AddComponent<LineRenderer>();
                l.material = Shapes.LineMat; l.useWorldSpace = true; l.numCapVertices = 2; l.numCornerVertices = 1;
                l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; l.receiveShadows = false; lines.Add(l);
            }
            lused++;
            if (!l.enabled) l.enabled = true;
            l.positionCount = n; l.startWidth = l.endWidth = width; l.startColor = l.endColor = col; l.sortingOrder = order;
            return l;
        }
        public void Arc(float cx, float cy, float r, float a0, float a1, float width, Color col, int order)
        {
            int n = Mathf.Clamp((int)(Mathf.Abs(a1 - a0) * r / 40f), 6, 120);
            var l = Line(n, width, col, order);
            for (int i = 0; i < n; i++) { float a = a0 + (a1 - a0) * i / (n - 1); l.SetPosition(i, new Vector3(cx + Mathf.Cos(a) * r, -(cy + Mathf.Sin(a) * r), 0)); }
        }
    }
}
