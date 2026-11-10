using System;

namespace PhamNhan.Art
{
    /// <summary>Tư thế một khung hình. Góc tay đo từ hướng thẳng xuống, dương là đưa ra trước.</summary>
    public struct Pose
    {
        public int f; public float bob, legF, legB, liftF, liftB, armF, armB, lean;
        public bool Atk { get { return f == 3; } }

        public static Pose Of(int f)
        {
            switch (f)
            {
                case 1: return new Pose { f = 1, bob = 1.6f, legF = 8, legB = -7, liftB = 2.5f, armF = -0.15f, armB = 0.5f, lean = 0.6f };
                case 2: return new Pose { f = 2, bob = 1.6f, legF = -6, legB = 7, liftF = 2.5f, armF = 0.8f, armB = -0.45f, lean = 0.6f };
                case 3: return new Pose { f = 3, bob = 0.4f, legF = 10, legB = -9, armF = 2.15f, armB = -0.75f, lean = 3 };
                default: return new Pose { f = 0, bob = 0, legF = 3, legB = -3, armF = 0.35f, armB = -0.15f, lean = 0 };
            }
        }
    }

    public enum EyeStyle { Normal, Lash, Glow, None, Narrow }

    /// <summary>Thông số dáng người kiểu chibi đầu to, nhìn 3/4 quay sang phải.</summary>
    public class Look
    {
        public Col skin = Col.H("#f7d4b6"), hair = Col.H("#22202c"), robe, robe2, trim, sash, sleeve, pants, boot, eye = Col.H("#2b2236");
        public float hipY = 24, shY = 50, headY = 68, headR = 15, bodyW = 12, hemY = 8, hemW = 17, armLen = 19, armR = 4.6f, legR = 4.8f, hunch = 0;
        public EyeStyle eyes = EyeStyle.Normal; public bool blush = true, ear = true, jagged = false;
        public Action<Rig> back, hairBack, hairBack2, hairFront, head, held, heldBack, chest, over;
        public Look(string robe, string robe2, string trim, string sash)
        {
            this.robe = Col.H(robe); this.robe2 = Col.H(robe2); this.trim = Col.H(trim); this.sash = Col.H(sash);
            sleeve = this.robe; pants = this.robe2.Mul(0.7f); boot = Col.H("#3a3040");
        }
    }

    /// <summary>Khung xương dựng từ Look + Pose, giữ các điểm khớp để từng nhân vật vẽ chi tiết riêng.</summary>
    public class Rig
    {
        public Canvas c; public Look L; public Pose p;
        public float hx, hy;                 // tâm đầu
        public float sfx, sfy, sbx, sby;     // vai trước, vai sau
        public float fx, fy, bx, by;         // bàn tay trước, sau
        public float hipY, shY, lx;
        public float time; // pha để lệch nhẹ tóc, dải lụa

        public static float Sin(float a) { return (float)Math.Sin(a); }
        public static float Cos(float a) { return (float)Math.Cos(a); }
        public const float PI = (float)Math.PI;

        /// <summary>Góc cánh tay trước dạng hướng (0 = sang phải, PI/2 = lên).</summary>
        public float ArmDirF { get { return p.armF - PI / 2; } }
        public float ArmDirB { get { return p.armB - PI / 2; } }

        public void F(Shp s, Col col, float shade = 1, bool line = true, float lw = -1) { c.F(s, col, shade, line, lw); }

        public void Human(Look look, Pose pose)
        {
            L = look; p = pose; time = pose.f * 0.9f;
            float b = p.bob; lx = p.lean + L.hunch;
            hipY = L.hipY + b * 0.5f; shY = L.shY + b - L.hunch * 0.6f;
            hx = 2 + lx + L.hunch * 0.6f; hy = L.headY + b - L.hunch * 0.9f;
            sbx = -L.bodyW * 0.55f + lx; sby = shY - 2.5f; sfx = L.bodyW * 0.62f + lx; sfy = shY - 2.5f;
            fx = sfx + L.armLen * Sin(p.armF); fy = sfy - L.armLen * Cos(p.armF);
            bx = sbx + L.armLen * Sin(p.armB); by = sby - L.armLen * Cos(p.armB);

            L.back?.Invoke(this);
            L.hairBack?.Invoke(this);
            // tay sau
            F(new Seg(sbx, sby, bx, by, L.armR, L.armR * 0.85f), L.sleeve.Mul(0.8f));
            L.heldBack?.Invoke(this);
            F(new Ell(bx, by, L.armR * 0.72f), L.skin.Mul(0.86f));
            // chân
            Leg(-3.5f, p.legB, p.liftB, 0.82f);
            Leg(3.5f, p.legF, p.liftF, 1f);
            // áo dưới và thân
            float hf = L.hemW + p.legF * 0.45f + lx * 0.3f, hb = -L.hemW + p.legB * 0.45f + lx * 0.3f, wy = hipY + 5;
            if (L.jagged)
                F(new Poly(1, -L.bodyW - 1 + lx, wy, L.bodyW + 1 + lx, wy, hf, L.hemY + 2, hf - 4, L.hemY - 3, hf - 9, L.hemY + 1, (hf + hb) / 2, L.hemY - 4, hb + 8, L.hemY + 1, hb + 3, L.hemY - 3, hb, L.hemY + 2), L.robe2);
            else
                F(new Poly(2, -L.bodyW - 1 + lx, wy, L.bodyW + 1 + lx, wy, hf, L.hemY, hb, L.hemY), L.robe2);
            // đường xẻ vạt áo phía trước
            F(new Seg(L.bodyW * 0.45f + lx, wy - 2, hf - 3.5f, L.hemY + 1.5f, 0.6f), L.robe2.Mul(0.6f), 0, false);
            F(new Poly(2.5f, -L.bodyW + lx - L.hunch * 0.3f, shY + 1, L.bodyW + lx, shY + 1, L.bodyW + 1.5f + lx * 0.5f, wy - 1, -L.bodyW - 1.5f + lx * 0.5f, wy - 1), L.robe);
            // cổ áo giao lĩnh
            float cx = lx + 3;
            F(new Poly(0.6f, cx - 6, shY + 1, cx - 1.5f, shY + 1, cx + 6.5f, shY - 9.5f, cx + 4, shY - 11), L.trim, 0.5f);
            F(new Poly(0.6f, cx + 7, shY + 1, cx + 2.5f, shY + 1, cx - 1.5f, shY - 6.5f, cx + 0.5f, shY - 8.5f), L.trim.Mul(0.92f), 0.5f);
            // đai lưng
            F(new RRect(lx * 0.6f + 0.5f, wy, L.bodyW + 2.2f, 3.2f, 1.6f), L.sash);
            F(new Seg(lx * 0.6f + 6, wy - 2, lx * 0.6f + 7.5f + p.legF * 0.2f, wy - 11, 1.6f, 1.1f), L.sash.Mul(0.9f));
            L.chest?.Invoke(this);
            // đầu
            F(new Seg(hx - 1, shY - 1, hx - 1, hy - 8, 3.4f), L.skin.Mul(0.85f), 0);
            if (L.head != null) L.head(this); else Head();
            // tay trước, vũ khí, bàn tay
            F(new Seg(sfx, sfy, fx, fy, L.armR, L.armR * 0.85f), L.sleeve);
            F(new Ell(sfx + (fx - sfx) * 0.82f, sfy + (fy - sfy) * 0.82f, L.armR * 1.05f, L.armR * 0.9f, ArmDirF), L.trim, 0.4f);
            L.held?.Invoke(this);
            F(new Ell(fx, fy, L.armR * 0.75f), L.skin);
            L.over?.Invoke(this);
        }

        void Leg(float x, float off, float lift, float k)
        {
            float fx0 = x + off, fy0 = lift;
            F(new Seg(x + lx * 0.3f, hipY, fx0, fy0 + 4, L.legR, L.legR * 0.88f), L.pants.Mul(k));
            F(new Ell(fx0 + 2, fy0 + 2.6f, 5.4f, 3.1f), L.boot.Mul(k));
        }

        public void Head()
        {
            L.hairBack2?.Invoke(this);
            F(new Ell(hx, hy, L.headR, L.headR * 0.96f), L.skin, 0.5f);
            Face(hx, hy);
            L.hairFront?.Invoke(this);
            if (L.ear) F(new Ell(hx - 7.5f, hy - 2.5f, 2.3f, 3.1f), L.skin.Mul(0.95f), 0.3f, true, 1);
        }

        public void Face(float x, float y)
        {
            var ink = L.eye;
            switch (L.eyes)
            {
                case EyeStyle.None: break;
                case EyeStyle.Glow:
                    c.Glow(new Ell(x + 4, y - 1, 1.6f, 1.2f), ink.A(0.7f), 3); c.Glow(new Ell(x + 11, y - 1, 1.3f, 1.1f), ink.A(0.7f), 3);
                    F(new Ell(x + 4, y - 1, 1.8f, 1.2f), Col.White.Mix(ink, 0.4f), 0, false); F(new Ell(x + 11, y - 1, 1.4f, 1.1f), Col.White.Mix(ink, 0.4f), 0, false);
                    break;
                case EyeStyle.Narrow:
                    F(new Seg(x + 2, y - 0.5f, x + 6, y - 1.2f, 0.9f), ink, 0, false); F(new Seg(x + 9.5f, y - 1, x + 12.5f, y - 0.4f, 0.8f), ink, 0, false);
                    break;
                default:
                    F(new Ell(x + 4, y - 1.4f, 2.1f, 3f), ink, 0, false); F(new Ell(x + 11, y - 1.4f, 1.7f, 2.8f), ink, 0, false);
                    F(new Ell(x + 4.5f, y - 0.2f, 0.7f), Col.White, 0, false); F(new Ell(x + 11.4f, y - 0.3f, 0.6f), Col.White, 0, false);
                    if (L.eyes == EyeStyle.Lash) { F(new Seg(x + 2, y + 1.4f, x + 6.3f, y + 2, 0.55f), ink, 0, false); F(new Seg(x + 9.6f, y + 1.6f, x + 13, y + 1.6f, 0.5f), ink, 0, false); F(new Seg(x + 2.2f, y + 1.3f, x + 1, y + 2.4f, 0.45f), ink, 0, false); }
                    break;
            }
            if (L.eyes != EyeStyle.None)
            {
                F(new Seg(x + 2.3f, y + 3.6f, x + 5.8f, y + 4.2f, 0.55f), L.hair.Mul(0.8f), 0, false);
                F(new Seg(x + 9.6f, y + 4.1f, x + 12.6f, y + 3.6f, 0.5f), L.hair.Mul(0.8f), 0, false);
                F(new Seg(x + 7, y - 7, x + 8.6f, y - 7.2f, 0.5f), ink.Mix(Col.H("#c05050"), 0.5f), 0, false);
            }
            if (L.blush) { c.Glow(new Ell(x + 2.5f, y - 4.5f, 1.4f, 0.8f), Col.H("#ff8a8a", 0.35f), 2); c.Glow(new Ell(x + 12.5f, y - 4.5f, 1.1f, 0.7f), Col.H("#ff8a8a", 0.3f), 1.6f); }
        }

        // ---------------- đồ vật dùng chung ----------------

        /// <summary>Kiếm cầm tại (x,y) hướng a. len: chiều dài lưỡi.</summary>
        public void Sword(float x, float y, float a, float len, Col blade, Col guard, float w = 2.2f)
        {
            float dx = Cos(a), dy = Sin(a), nx = -dy, ny = dx;
            F(new Seg(x - dx * 6, y - dy * 6, x, y, 1.3f), Col.H("#4a3a30"), 0);
            F(new Ell(x - dx * 7, y - dy * 7, 1.6f), guard, 0.3f);
            F(new Poly(0.3f, x + dx * 2 + nx * w, y + dy * 2 + ny * w, x + dx * (len - 3) + nx * w * 0.8f, y + dy * (len - 3) + ny * w * 0.8f, x + dx * len, y + dy * len,
                x + dx * (len - 3) - nx * w * 0.8f, y + dy * (len - 3) - ny * w * 0.8f, x + dx * 2 - nx * w, y + dy * 2 - ny * w), blade, 0.4f);
            F(new Seg(x + dx * 3, y + dy * 3, x + dx * (len - 4), y + dy * (len - 4), 0.45f), Col.White.A(0.8f), 0, false);
            F(new RRect(x + dx * 1.5f, y + dy * 1.5f, 1.4f, 4.2f, 1, a), guard, 0.4f);
        }

        /// <summary>Cán dài từ tay ra hai phía: back phía sau tay, fwd phía trước.</summary>
        public void Pole(float x, float y, float a, float back, float fwd, Col col, float r = 1.5f)
        {
            float dx = Cos(a), dy = Sin(a);
            F(new Seg(x - dx * back, y - dy * back, x + dx * fwd, y + dy * fwd, r), col, 0.3f);
        }

        /// <summary>Cung cong, dây cung thẳng. Tay cầm ở giữa, hướng bắn a.</summary>
        public void Bow(float x, float y, float a, float h, Col col, bool arrow)
        {
            float dx = Cos(a), dy = Sin(a), nx = -dy, ny = dx; int n = 7; float bend = h * 0.32f;
            float px = 0, py = 0;
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n * 2 - 1; float qx = x + nx * t * h + dx * bend * (1 - t * t), qy = y + ny * t * h + dy * bend * (1 - t * t);
                if (i > 0) F(new Seg(px, py, qx, qy, 1.5f - Math.Abs(t) * 0.6f), col, 0.3f);
                px = qx; py = qy;
            }
            float sx = x - dx * (arrow ? 7 : 0), sy = y - dy * (arrow ? 7 : 0);
            F(new Seg(x + nx * h, y + ny * h, sx, sy, 0.35f), Col.H("#f4efe2"), 0, false);
            F(new Seg(x - nx * h, y - ny * h, sx, sy, 0.35f), Col.H("#f4efe2"), 0, false);
            if (arrow)
            {
                F(new Seg(sx, sy, x + dx * 14, y + dy * 14, 0.7f), Col.H("#6a4a30"), 0, false);
                F(new Poly(0.2f, x + dx * 17, y + dy * 17, x + dx * 13 + nx * 1.8f, y + dy * 13 + ny * 1.8f, x + dx * 13 - nx * 1.8f, y + dy * 13 - ny * 1.8f), Col.H("#d8dde6"), 0.2f, true, 0.8f);
            }
        }

        /// <summary>Lá bùa giấy vàng có nét chữ đỏ.</summary>
        public void Talisman(float x, float y, float a, float sc = 1)
        {
            float dx = Cos(a), dy = Sin(a);
            float cx = x + dx * 6 * sc, cy = y + dy * 6 * sc;
            F(new RRect(cx, cy, 3 * sc, 6.5f * sc, 0.6f, a - PI / 2), Col.H("#ffe08a"), 0.3f, true, 1);
            F(new Seg(cx - dx * 4 * sc, cy - dy * 4 * sc, cx + dx * 4 * sc, cy + dy * 4 * sc, 0.55f * sc), Col.H("#d0342c"), 0, false);
            F(new Seg(cx - dy * 1.6f * sc, cy + dx * 1.6f * sc, cx + dy * 1.6f * sc, cy - dx * 1.6f * sc, 0.5f * sc), Col.H("#d0342c"), 0, false);
        }

        /// <summary>Tóc mái dạng các mũi nhọn dọc trán, từ x0 tới x1.</summary>
        public void Bangs(Col col, float x0, float x1, float top, float tip, int n)
        {
            float w = (x1 - x0) / n;
            for (int i = 0; i < n; i++)
            {
                float a = x0 + i * w, t = tip - (i % 2) * 2.2f + (i == n - 1 ? 2 : 0);
                F(new Poly(0.8f, a - 0.6f, top, a + w + 0.6f, top, a + w * 0.65f, t), col, 0.4f, false);
            }
        }

        /// <summary>Khối tóc ôm đỉnh đầu (không che mặt).</summary>
        public void HairCap(Col col, float lift = 0)
        {
            F(new Ell(hx - 8, hy + 1 + lift, L.headR * 0.56f, L.headR * 0.9f), col, 0.5f);
            F(new Ell(hx + 0.5f, hy + 11f + lift, L.headR * 0.97f, L.headR * 0.55f), col, 0.6f);
        }
    }
}
