using System;

namespace PhamNhan.Art
{
    /// <summary>Màu RGBA số thực, không phụ thuộc Unity.</summary>
    public struct Col
    {
        public float r, g, b, a;
        public Col(float r, float g, float b, float a = 1) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Col H(string h, float a = 1)
        {
            if (h[0] == '#') h = h.Substring(1);
            int v = Convert.ToInt32(h, 16);
            return new Col(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, a);
        }
        static float C1(float x) { return x < 0 ? 0 : x > 1 ? 1 : x; }
        public Col Mul(float k) { return new Col(C1(r * k), C1(g * k), C1(b * k), a); }
        public Col Mix(Col o, float t) { return new Col(r + (o.r - r) * t, g + (o.g - g) * t, b + (o.b - b) * t, a + (o.a - a) * t); }
        public Col A(float na) { return new Col(r, g, b, na); }
        public static readonly Col White = new Col(1, 1, 1), Black = new Col(0, 0, 0);
    }

    /// <summary>Hình học có hàm khoảng cách có dấu (âm ở trong). Toạ độ vẽ: x phải, y lên, chân nhân vật ở y = 0.</summary>
    public abstract class Shp
    {
        public float x0, y0, x1, y1;
        public abstract float D(float x, float y);
        protected void Box(float ax, float ay, float bx, float by) { x0 = Math.Min(ax, bx); x1 = Math.Max(ax, bx); y0 = Math.Min(ay, by); y1 = Math.Max(ay, by); }
        protected void Grow(float x, float y) { if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y; }
    }

    public class Ell : Shp
    {
        readonly float cx, cy, rx, ry, c, s;
        public Ell(float cx, float cy, float rx, float ry, float rot = 0)
        {
            this.cx = cx; this.cy = cy; this.rx = Math.Max(rx, 0.01f); this.ry = Math.Max(ry, 0.01f); c = (float)Math.Cos(rot); s = (float)Math.Sin(rot);
            float m = Math.Max(rx, ry); Box(cx - m, cy - m, cx + m, cy + m);
        }
        public Ell(float cx, float cy, float r) : this(cx, cy, r, r) { }
        public override float D(float x, float y)
        {
            float px = x - cx, py = y - cy; float qx = c * px + s * py, qy = -s * px + c * py;
            float ux = qx / rx, uy = qy / ry; float k = (float)Math.Sqrt(ux * ux + uy * uy);
            if (k < 1e-5f) return -Math.Min(rx, ry);
            float gx = ux / rx / k, gy = uy / ry / k; float gl = (float)Math.Sqrt(gx * gx + gy * gy);
            return (k - 1) / gl;
        }
    }

    /// <summary>Đoạn thẳng có độ dày thay đổi từ ra tới rb.</summary>
    public class Seg : Shp
    {
        readonly float ax, ay, bx, by, ra, rb;
        public Seg(float ax, float ay, float bx, float by, float ra, float rb = -1)
        {
            if (rb < 0) rb = ra; this.ax = ax; this.ay = ay; this.bx = bx; this.by = by; this.ra = ra; this.rb = rb;
            float m = Math.Max(ra, rb); Box(ax, ay, bx, by); x0 -= m; y0 -= m; x1 += m; y1 += m;
        }
        public override float D(float x, float y)
        {
            float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay; float dd = bax * bax + bay * bay;
            float h = dd > 0 ? (pax * bax + pay * bay) / dd : 0; h = h < 0 ? 0 : h > 1 ? 1 : h;
            float dx = pax - bax * h, dy = pay - bay * h;
            return (float)Math.Sqrt(dx * dx + dy * dy) - (ra + (rb - ra) * h);
        }
    }

    /// <summary>Đa giác, có thể bo tròn góc bằng rad.</summary>
    public class Poly : Shp
    {
        readonly float[] v; readonly float rad;
        public Poly(float rad, params float[] xy)
        {
            v = xy; this.rad = rad; Box(xy[0], xy[1], xy[0], xy[1]);
            for (int i = 0; i < xy.Length; i += 2) Grow(xy[i], xy[i + 1]);
            x0 -= rad; y0 -= rad; x1 += rad; y1 += rad;
        }
        public Poly(params float[] xy) : this(0, xy) { }
        public override float D(float x, float y)
        {
            int n = v.Length / 2; float d = (x - v[0]) * (x - v[0]) + (y - v[1]) * (y - v[1]); float sg = 1;
            for (int i = 0, j = n - 1; i < n; j = i, i++)
            {
                float vix = v[i * 2], viy = v[i * 2 + 1], vjx = v[j * 2], vjy = v[j * 2 + 1];
                float ex = vjx - vix, ey = vjy - viy, wx = x - vix, wy = y - viy;
                float ee = ex * ex + ey * ey; float t = ee > 0 ? (wx * ex + wy * ey) / ee : 0; t = t < 0 ? 0 : t > 1 ? 1 : t;
                float bx = wx - ex * t, by = wy - ey * t; float bb = bx * bx + by * by; if (bb < d) d = bb;
                bool c1 = y >= viy, c2 = y < vjy, c3 = ex * wy > ey * wx;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) sg = -sg;
            }
            return sg * (float)Math.Sqrt(d) - rad;
        }
    }

    /// <summary>Chữ nhật bo góc, xoay quanh tâm.</summary>
    public class RRect : Shp
    {
        readonly float cx, cy, hw, hh, rad, c, s;
        public RRect(float cx, float cy, float hw, float hh, float rad = 0, float rot = 0)
        {
            this.cx = cx; this.cy = cy; this.hw = hw; this.hh = hh; this.rad = Math.Min(rad, Math.Min(hw, hh)); c = (float)Math.Cos(rot); s = (float)Math.Sin(rot);
            float m = (float)Math.Sqrt(hw * hw + hh * hh); Box(cx - m, cy - m, cx + m, cy + m);
        }
        public override float D(float x, float y)
        {
            float px = x - cx, py = y - cy; float qx = Math.Abs(c * px + s * py) - hw + rad, qy = Math.Abs(-s * px + c * py) - hh + rad;
            float ox = Math.Max(qx, 0), oy = Math.Max(qy, 0);
            return (float)Math.Sqrt(ox * ox + oy * oy) + Math.Min(Math.Max(qx, qy), 0) - rad;
        }
    }

    /// <summary>Phần chung: chỉ tô ở chỗ a và b cùng phủ.</summary>
    public class Inter : Shp
    {
        readonly Shp a, b;
        public Inter(Shp a, Shp b) { this.a = a; this.b = b; x0 = Math.Max(a.x0, b.x0); y0 = Math.Max(a.y0, b.y0); x1 = Math.Min(a.x1, b.x1); y1 = Math.Min(a.y1, b.y1); }
        public override float D(float x, float y) { return Math.Max(a.D(x, y), b.D(x, y)); }
    }

    /// <summary>Hiệu: phần của a không nằm trong b.</summary>
    public class Sub : Shp
    {
        readonly Shp a, b;
        public Sub(Shp a, Shp b) { this.a = a; this.b = b; x0 = a.x0; y0 = a.y0; x1 = a.x1; y1 = a.y1; }
        public override float D(float x, float y) { return Math.Max(a.D(x, y), -b.D(x, y)); }
    }

    /// <summary>Tấm vẽ RGBA (đã nhân alpha). Tô hình có viền mực và đổ sáng từ trên xuống.</summary>
    public class Canvas
    {
        public readonly int W, H; public readonly float[] P;
        public readonly float ax0, ay1, s; // toạ độ vẽ ứng với điểm ảnh (0,0) và số điểm ảnh mỗi đơn vị
        public float L = 1.5f; public Col Ink = Col.H("#2b2236");

        public Canvas(int w, int h, float ax0, float ax1, float ay0)
        {
            W = w; H = h; P = new float[w * h * 4]; s = w / (ax1 - ax0); this.ax0 = ax0; ay1 = ay0 + h / s;
        }

        static float Cl(float x) { return x < 0 ? 0 : x > 1 ? 1 : x; }

        void Over(int k, Col c, float cov)
        {
            float a = c.a * cov; if (a <= 0) return; float ia = 1 - a;
            P[k] = P[k] * ia + c.r * a; P[k + 1] = P[k + 1] * ia + c.g * a; P[k + 2] = P[k + 2] * ia + c.b * a; P[k + 3] = P[k + 3] * ia + a;
        }

        void Range(Shp sh, float pad, out int i0, out int i1, out int j0, out int j1)
        {
            i0 = Math.Max(0, (int)((sh.x0 - pad - ax0) * s)); i1 = Math.Min(W - 1, (int)((sh.x1 + pad - ax0) * s) + 1);
            j0 = Math.Max(0, (int)((ay1 - sh.y1 - pad) * s)); j1 = Math.Min(H - 1, (int)((ay1 - sh.y0 + pad) * s) + 1);
        }

        /// <summary>Tô hình. shade: độ đổ sáng (0 = phẳng). line: có viền mực.</summary>
        public void F(Shp sh, Col c, float shade = 1, bool line = true, float lw = -1)
        {
            float l = line ? (lw < 0 ? L : lw) : 0; float pad = l + 2 / s;
            Range(sh, pad, out int i0, out int i1, out int j0, out int j1);
            Col top = c.Mul(1 + 0.13f * shade), bot = c.Mul(1 - 0.22f * shade); float hy = sh.y1 - sh.y0;
            for (int j = j0; j <= j1; j++)
            {
                float y = ay1 - (j + 0.5f) / s; float t = hy > 0 ? Cl((sh.y1 - y) / hy) : 0.5f; Col cc = top.Mix(bot, t);
                for (int i = i0; i <= i1; i++)
                {
                    float x = ax0 + (i + 0.5f) / s; float d = sh.D(x, y); int k = (j * W + i) * 4;
                    if (l > 0) { float co = Cl(0.5f - (d - l) * s); if (co > 0) Over(k, Ink, co); }
                    float cf = Cl(0.5f - d * s); if (cf > 0) Over(k, cc, cf);
                }
            }
        }

        /// <summary>Quầng mềm: đậm trong hình, nhạt dần ra ngoài trong khoảng w.</summary>
        public void Glow(Shp sh, Col c, float w)
        {
            Range(sh, w, out int i0, out int i1, out int j0, out int j1);
            for (int j = j0; j <= j1; j++)
            {
                float y = ay1 - (j + 0.5f) / s;
                for (int i = i0; i <= i1; i++)
                {
                    float x = ax0 + (i + 0.5f) / s; float d = sh.D(x, y); if (d >= w) continue;
                    float k = d <= 0 ? 1 : 1 - d / w; Over((j * W + i) * 4, c, k * k);
                }
            }
        }

        /// <summary>RGBA 8 bit, bỏ nhân alpha. flipY: hàng dưới cùng trước (kiểu Unity).</summary>
        public byte[] Bytes(bool flipY)
        {
            var o = new byte[W * H * 4];
            for (int j = 0; j < H; j++)
            {
                int sj = flipY ? H - 1 - j : j;
                for (int i = 0; i < W; i++)
                {
                    int k = (sj * W + i) * 4, q = (j * W + i) * 4; float a = P[k + 3];
                    float ia = a > 1e-4f ? 1 / a : 0;
                    o[q] = (byte)(Cl(P[k] * ia) * 255 + 0.5f); o[q + 1] = (byte)(Cl(P[k + 1] * ia) * 255 + 0.5f); o[q + 2] = (byte)(Cl(P[k + 2] * ia) * 255 + 0.5f); o[q + 3] = (byte)(Cl(a) * 255 + 0.5f);
                }
            }
            return o;
        }
    }
}
