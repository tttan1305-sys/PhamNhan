using UnityEngine;

namespace PhamNhan.View
{
    /// <summary>Kiểu chữ, khung bo góc và hàm vẽ giao diện IMGUI theo khung tham chiếu cao 720.</summary>
    public static class Ui
    {
        public static Font font;
        public static Texture2D round, roundSm, circle, white, ringT;
        public static GUIStyle panel, panelDark, txt;
        public static readonly Color INK = Shapes.Hex("#17302a"), SUB = Shapes.Hex("#4a6a60"), JADE = Shapes.Hex("#10a37f"), ALLY = Shapes.Hex("#2f8fe0"), FOE = Shapes.Hex("#e5483d"), GOLD = Shapes.Hex("#f3c64a");
        static bool ready;

        public static void Init()
        {
            if (ready) return; ready = true;
            try { font = Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "Noto Sans", "Segoe UI", "Helvetica Neue", "Arial", "Noto Sans CJK SC", "PingFang SC", "Microsoft YaHei" }, 32); } catch { font = null; }
            round = Rounded(48, 16); roundSm = Rounded(24, 8);
            circle = Disc(96, false); ringT = Disc(96, true);
            white = new Texture2D(1, 1); white.SetPixel(0, 0, Color.white); white.Apply();
            panel = new GUIStyle { normal = { background = round }, border = new RectOffset(16, 16, 16, 16) };
            panelDark = new GUIStyle { normal = { background = round }, border = new RectOffset(16, 16, 16, 16) };
            txt = new GUIStyle { font = font, richText = true, wordWrap = false, clipping = TextClipping.Overflow };
        }

        static Texture2D Rounded(int n, int rad)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false); t.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, rad, n - rad), cy = Mathf.Clamp(y + 0.5f, rad, n - rad);
                    float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(rad - d + 0.5f)));
                }
            t.Apply(); return t;
        }
        static Texture2D Disc(int n, bool ring)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false); t.wrapMode = TextureWrapMode.Clamp;
            float R = n / 2f;
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - R) * (x + 0.5f - R) + (y + 0.5f - R) * (y + 0.5f - R));
                    float a = Mathf.Clamp01(R - d - 0.5f);
                    if (ring) a = Mathf.Min(a, Mathf.Clamp01(d - (R - 5)));
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            t.Apply(); return t;
        }

        public static void Box(Rect r, Color c)
        {
            var old = GUI.color; GUI.color = c; GUI.Box(r, GUIContent.none, panel); GUI.color = old;
        }
        public static void Fill(Rect r, Color c) { var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, white); GUI.color = old; }
        public static void Circle(Vector2 c, float r, Color col, bool ring = false) { var old = GUI.color; GUI.color = col; GUI.DrawTexture(new Rect(c.x - r, c.y - r, r * 2, r * 2), ring ? ringT : circle); GUI.color = old; }

        public static void Text(Rect r, string s, int size, Color c, TextAnchor anchor = TextAnchor.UpperLeft, bool bold = false, bool wrap = false, bool shadow = false)
        {
            txt.fontSize = size; txt.alignment = anchor; txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; txt.wordWrap = wrap;
            if (shadow) { txt.normal.textColor = new Color(0, 0, 0, 0.55f * c.a); GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), s, txt); }
            txt.normal.textColor = c; GUI.Label(r, s, txt);
        }
        public static float TextH(string s, int size, float w)
        {
            txt.fontSize = size; txt.wordWrap = true; return txt.CalcHeight(new GUIContent(s), w);
        }
        public static void Bar(Rect r, float k, Color c)
        {
            Fill(r, new Color(0.06f, 0.16f, 0.13f, 0.18f));
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(k), r.height), c);
        }
        public static string Clock(float s) { int v = Mathf.Max(0, Mathf.CeilToInt(s)); return (v / 60).ToString("00") + ":" + (v % 60).ToString("00"); }
    }
}
