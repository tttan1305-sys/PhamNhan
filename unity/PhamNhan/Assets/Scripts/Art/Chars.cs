using System;
using System.Collections.Generic;

namespace PhamNhan.Art
{
    /// <summary>Tạo hình các tướng và binh chủng. Mỗi loại có 4 khung: 0 đứng, 1 và 2 bước đi, 3 ra đòn.</summary>
    public static class Chars
    {
        public const int FRAMES = 4;

        public class Spec
        {
            public string key, name; public float ax0 = -60, ax1 = 60, ay0 = -12; // vùng vẽ vuông
            public float face = 68, faceR = 26;  // tâm và bán kính khung chân dung (toạ độ vẽ, quanh đầu)
            public float faceX = 2;
            public Action<Rig, Pose> draw;
        }

        static readonly Dictionary<string, Spec> S = new Dictionary<string, Spec>();
        public static readonly List<string> Keys = new List<string>();
        static float Sin(float a) { return (float)Math.Sin(a); }
        static float Cos(float a) { return (float)Math.Cos(a); }
        const float PI = (float)Math.PI;
        static Col H(string h, float a = 1) { return Col.H(h, a); }

        static void Add(string key, Action<Rig, Pose> d, Action<Spec> cfg = null)
        {
            var s = new Spec { key = key, draw = d }; cfg?.Invoke(s); S[key] = s; Keys.Add(key);
        }
        static void Hum(string key, Func<Look> mk, Action<Spec> cfg = null) { Add(key, (r, p) => r.Human(mk(), p), cfg); }

        public static Spec Get(string key) { Spec s; return S.TryGetValue(key, out s) ? s : null; }

        /// <summary>Vẽ một khung hình ra tấm vẽ px × px.</summary>
        public static Canvas Render(string key, int frame, int px)
        {
            var sp = S[key]; var c = new Canvas(px, px, sp.ax0, sp.ax1, sp.ay0);
            c.L = Math.Max(1.1f, 1.35f * 120f / (sp.ax1 - sp.ax0) * (px >= 120 ? 1 : 1.25f));
            var r = new Rig { c = c }; sp.draw(r, Pose.Of(frame)); return c;
        }

        static Chars()
        {
            // ================= CHÍNH ĐẠO =================

            // Ngao Thiên · Vạn Kiếm Tông: kiếm tu áo trắng viền lam, buộc tóc đuôi ngựa, ba phi kiếm sau lưng
            Hum("hero", () =>
            {
                var L = new Look("#f4f7fb", "#2f8fe0", "#f3d9a0", "#1f5f9e") { hair = H("#1e2030"), pants = H("#1f3550"), boot = H("#26324a") };
                L.back = r =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float a = 1.15f + i * 0.42f + (r.p.Atk ? -0.5f : 0), bx = -7 + r.lx + Cos(a) * 7, by = 52 + r.p.bob + Sin(a) * 7;
                        r.c.Glow(new Seg(bx, by, bx + Cos(a) * 24, by + Sin(a) * 24, 2), H("#8fd8ff", 0.35f), 4);
                        r.Sword(bx, by, a, 24, H("#e9f6ff"), H("#f3c860"), 1.8f);
                    }
                };
                L.hairBack = r =>
                {
                    float sw = r.p.f == 0 ? 0 : r.p.f == 3 ? -6 : (r.p.f == 1 ? -3 : 3);
                    r.F(new Seg(r.hx - 7, r.hy + 10, r.hx - 17 + sw * 0.5f, r.hy - 6, 4.2f, 2.4f), r.L.hair);
                    r.F(new Seg(r.hx - 17 + sw * 0.5f, r.hy - 6, r.hx - 19 + sw, r.hy - 20, 2.4f, 0.8f), r.L.hair);
                };
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair);
                    r.Bangs(r.L.hair, r.hx - 4, r.hx + 14, r.hy + 10.5f, r.hy + 4f, 5);
                    r.F(new Seg(r.hx + 13, r.hy + 6, r.hx + 15, r.hy - 6, 2, 0.6f), r.L.hair, 0.3f, false);
                    r.F(new RRect(r.hx - 3, r.hy + 14.5f, 4, 3, 1.2f, 0.2f), H("#f3c860"), 0.5f);
                    r.F(new Seg(r.hx - 9, r.hy + 15.5f, r.hx + 4, r.hy + 13.5f, 0.8f), H("#f3c860"), 0, true, 0.8f);
                };
                L.held = r => r.Sword(r.fx, r.fy, r.ArmDirF + (r.p.Atk ? 0.15f : 0.3f), 30, H("#eef8ff"), H("#f3c860"));
                L.chest = r => r.F(new Ell(r.lx + 1, r.hipY + 5, 2.2f), H("#f3c860"), 0.3f, true, 1);
                return L;
            }, s => s.name = "Ngao Thiên");

            // Viêm Dương · Hạo Nhiên Tông: phù sư tóc lửa, áo đỏ cam, kẹp phù chú, hỏa cầu lơ lửng
            Hum("viem", () =>
            {
                var L = new Look("#ef6a3a", "#8a3a22", "#ffd08a", "#ffcf6b") { hair = H("#c6392a"), pants = H("#5a2a1c"), boot = H("#3a2420") };
                L.back = r => r.c.Glow(new Ell(r.lx, 50, 20, 26), H("#ff9a50", 0.22f), 14);
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair);
                    for (int i = 0; i < 5; i++)
                    {
                        float x = r.hx - 10 + i * 5.5f, top = r.hy + 18 + (i % 2) * 4 - Math.Abs(i - 2) * 1.5f;
                        r.F(new Poly(0.8f, x - 4, r.hy + 9, x + 4, r.hy + 9, x + 2 + i * 0.4f, top), i % 2 == 0 ? r.L.hair : H("#ff7a3a"), 0.5f);
                    }
                    r.Bangs(r.L.hair, r.hx - 3, r.hx + 14, r.hy + 10.5f, r.hy + 4.5f, 4);
                    r.F(new Seg(r.hx - 11, r.hy + 9, r.hx + 14, r.hy + 9.5f, 1.4f), H("#ffcf6b"), 0.3f, true, 1);
                };
                L.held = r =>
                {
                    r.Talisman(r.fx, r.fy, r.ArmDirF + 0.6f, 1.1f);
                    if (r.p.Atk) { r.c.Glow(new Ell(r.fx + 14, r.fy + 6, 6), H("#ff9a40", 0.7f), 10); r.F(new Ell(r.fx + 14, r.fy + 6, 4), H("#fff0a0"), 0, false); }
                };
                L.heldBack = r =>
                {
                    float y = r.by + 9 + (r.p.f == 1 ? 1.5f : 0);
                    r.c.Glow(new Ell(r.bx, y, 4.5f), H("#ff7a30", 0.75f), 7);
                    r.F(new Poly(1, r.bx - 3.4f, y - 1, r.bx + 3.4f, y - 1, r.bx + 0.5f, y + 8), H("#ffb050"), 0.3f, false);
                    r.F(new Ell(r.bx, y, 2.6f), H("#fff2b0"), 0, false);
                };
                return L;
            }, s => s.name = "Viêm Dương");

            // Thạch Nham · Vô Cực Trận Tông: lực sĩ trọc đầu râu quai nón, giáp đá, ôm Trấn Sơn Ấn, trận bàn sau lưng
            Hum("thach", () =>
            {
                var L = new Look("#c49a62", "#6a5434", "#e7c88a", "#4a3a26") { hair = H("#4a3424"), skin = H("#eec39c"), pants = H("#4a3a28"), boot = H("#3a2e24"), bodyW = 15, hemW = 19, armR = 6, legR = 6, headR = 14, armLen = 18, hemY = 10, blush = false };
                L.back = r =>
                {
                    float x = -4 + r.lx, y = 50 + r.p.bob;
                    r.F(new Ell(x, y, 17), H("#8a7a5a"), 0.4f);
                    r.F(new Ell(x, y, 12.5f), H("#d9c08a"), 0.4f);
                    for (int i = 0; i < 8; i++) { float a = i * PI / 4; r.F(new Seg(x + Cos(a) * 13.5f, y + Sin(a) * 13.5f, x + Cos(a) * 16, y + Sin(a) * 16, 1), H("#3a2e1e"), 0, false); }
                    r.F(new Ell(x, y, 5), H("#8a6a3a"), 0.4f);
                };
                L.hairFront = r =>
                {
                    r.F(new Poly(2, r.hx - 4, r.hy - 4, r.hx + 15, r.hy - 4, r.hx + 12, r.hy - 15, r.hx + 2, r.hy - 18, r.hx - 6, r.hy - 11), r.L.hair, 0.4f);
                    r.F(new Ell(r.hx + 8, r.hy - 7.5f, 3.4f, 1.6f), H("#c47060"), 0, false);
                    r.F(new Seg(r.hx + 1.5f, r.hy + 4.5f, r.hx + 6.5f, r.hy + 3.5f, 1.3f), r.L.hair, 0, false);
                    r.F(new Seg(r.hx + 9, r.hy + 3.5f, r.hx + 13.5f, r.hy + 4.5f, 1.2f), r.L.hair, 0, false);
                    r.F(new Ell(r.hx - 4, r.hy + 15, 4.5f, 3.6f), r.L.hair, 0.4f);
                    r.F(new Seg(r.hx - 6, r.hy + 12.5f, r.hx - 2, r.hy + 12.5f, 1), H("#e7c88a"), 0, true, 0.8f);
                };
                L.chest = r =>
                {
                    r.F(new Ell(r.sbx + 1, r.shY - 1, 7, 5.5f), H("#8a7a62"), 0.6f);
                    r.F(new Ell(r.sfx, r.shY - 1, 7.5f, 6), H("#9a8a6a"), 0.6f);
                    r.F(new Seg(r.sfx - 4, r.shY - 2, r.sfx + 4, r.shY - 3, 0.6f), H("#5a4a3a"), 0, false);
                };
                L.held = r =>
                {
                    float x = r.fx + 4, y = r.fy + (r.p.Atk ? 5 : -2);
                    if (r.p.Atk) r.c.Glow(new RRect(x, y, 8, 8, 2), H("#ffd88a", 0.55f), 8);
                    r.F(new RRect(x, y, 7, 7, 1.5f, r.p.Atk ? -0.3f : 0), H("#d9b878"), 0.6f);
                    r.F(new RRect(x, y + 8, 3, 2.5f, 1, r.p.Atk ? -0.3f : 0), H("#8a6a3a"), 0.4f);
                    r.F(new RRect(x, y, 4.2f, 4.2f, 0.5f, r.p.Atk ? -0.3f : 0), H("#a8844c"), 0, false);
                    r.F(new Seg(x - 2.5f, y, x + 2.5f, y, 0.6f), H("#f6e2b0"), 0, false); r.F(new Seg(x, y - 2.5f, x, y + 2.5f, 0.6f), H("#f6e2b0"), 0, false);
                };
                return L;
            }, s => s.name = "Thạch Nham");

            // Thủy Nguyệt · Vạn Pháp Tông: nữ cung thủ tóc bạc dài, trâm trăng khuyết, cung băng
            Hum("nguyet", () =>
            {
                var L = new Look("#bfe2ff", "#5ea8f0", "#ffffff", "#2a4f7a") { hair = H("#d6e4f7"), pants = H("#3a5f8a"), boot = H("#3a4f6a"), bodyW = 10.5f, hemW = 18, hemY = 5, eyes = EyeStyle.Lash, eye = H("#2a4f7a") };
                L.hairBack = r =>
                {
                    float sw = r.p.f == 1 ? -2 : r.p.f == 2 ? 2 : 0;
                    r.F(new Poly(4, r.hx - 14, r.hy + 6, r.hx + 2, r.hy + 4, r.hx - 4 + sw, r.hy - 40, r.hx - 18 + sw, r.hy - 38), r.L.hair, 0.5f);
                    // dải lụa
                    r.F(new Seg(r.lx - 14, 46, r.lx - 22 + sw * 2, 20, 1.6f, 1), H("#9fd8ff", 0.85f), 0.2f, true, 0.8f);
                };
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair, 0.5f);
                    r.Bangs(r.L.hair, r.hx - 5, r.hx + 14, r.hy + 10.5f, r.hy + 3.5f, 6);
                    r.F(new Seg(r.hx + 13.5f, r.hy + 7, r.hx + 15, r.hy - 12, 2.2f, 0.8f), r.L.hair, 0.4f);
                    // trâm trăng khuyết
                    r.F(new Sub(new Ell(r.hx - 5, r.hy + 14, 5), new Ell(r.hx - 3, r.hy + 15.5f, 4.2f)), H("#ffd76a"), 0.3f, true, 0.9f);
                    r.F(new Ell(r.hx - 1, r.hy + 11.5f, 1.4f), H("#8fd8ff"), 0, true, 0.6f);
                };
                L.held = r =>
                {
                    float a = r.p.Atk ? 0.15f : r.ArmDirF + 0.9f;
                    if (r.p.Atk) r.c.Glow(new Ell(r.fx + 10, r.fy, 4), H("#bfefff", 0.6f), 10);
                    r.Bow(r.fx, r.fy, a, 15, H("#9fe2ff"), r.p.Atk);
                };
                return L;
            }, s => s.name = "Thủy Nguyệt");

            // Thanh Mộc · Vạn Thú Tông · Long tộc: sừng ngọc, đuôi rồng, giáo ngọc bích
            Hum("thanh", () =>
            {
                var L = new Look("#5fbf78", "#2f6a44", "#d9f0a0", "#1f4a30") { hair = H("#2a6e44"), pants = H("#244a34"), boot = H("#2a3a2c") };
                L.back = r =>
                {
                    float sw = r.p.f == 1 ? 3 : r.p.f == 2 ? -3 : 0;
                    r.F(new Seg(r.lx - 6, 22, r.lx - 20, 12 + sw, 5, 3.5f), H("#4fae6a"));
                    r.F(new Seg(r.lx - 20, 12 + sw, r.lx - 30, 18 + sw * 1.5f, 3.5f, 1), H("#4fae6a"));
                    for (int i = 0; i < 3; i++) r.F(new Poly(0.5f, r.lx - 10 - i * 7, 23 - i * 3 + sw * i * 0.3f, r.lx - 14 - i * 7, 27 - i * 3 + sw * i * 0.3f, r.lx - 16 - i * 7, 21 - i * 3 + sw * i * 0.3f), H("#d9f0a0"), 0, true, 0.8f);
                };
                L.hairFront = r =>
                {
                    // sừng
                    for (int k = 0; k < 2; k++)
                    {
                        float x = r.hx - 6 + k * 10, d = k == 0 ? -1 : 1;
                        r.F(new Seg(x, r.hy + 10, x + d * 3, r.hy + 22, 2.2f, 1), H("#e6f6d6"), 0.4f);
                        r.F(new Seg(x + d * 1.5f, r.hy + 16, x + d * 6, r.hy + 19, 1.2f, 0.6f), H("#e6f6d6"), 0.4f);
                    }
                    r.HairCap(r.L.hair);
                    r.Bangs(r.L.hair, r.hx - 4, r.hx + 14, r.hy + 10.5f, r.hy + 3.5f, 5);
                    r.F(new Seg(r.hx + 13, r.hy + 6, r.hx + 14, r.hy - 7, 2.2f, 0.7f), r.L.hair, 0.3f, false);
                    // vảy trên má
                    r.F(new Ell(r.hx + 13, r.hy - 4, 1, 0.8f), H("#4fae6a"), 0, false); r.F(new Ell(r.hx + 12, r.hy - 6, 0.8f), H("#4fae6a"), 0, false);
                };
                L.held = r =>
                {
                    float a = r.p.Atk ? 0.12f : 1.35f;
                    r.Pole(r.fx, r.fy, a, 20, 26, H("#7a5a3a"), 1.4f);
                    float tx = r.fx + Cos(a) * 26, ty = r.fy + Sin(a) * 26, nx = -Sin(a), ny = Cos(a);
                    r.F(new Poly(0.3f, tx + Cos(a) * 9, ty + Sin(a) * 9, tx + nx * 3, ty + ny * 3, tx - nx * 3, ty - ny * 3), H("#8ff0b0"), 0.3f);
                    r.F(new Ell(tx, ty, 1.8f), H("#e74c3c"), 0, true, 0.8f);
                };
                return L;
            }, s => s.name = "Thanh Mộc");

            // ================= MA ĐẠO =================

            // Hắc Lang Vương · Vạn Ma Tông: đầu sói xám, bờm, áo choàng đỏ, móng vuốt
            Hum("lang", () =>
            {
                var L = new Look("#46404a", "#2a2224", "#c8c0b8", "#e65a46") { hair = H("#5a5560"), skin = H("#6a6470"), pants = H("#2a2428"), boot = H("#1e1a20"), bodyW = 14, hemW = 18, armR = 5.6f, legR = 5.6f, hemY = 12, ear = false };
                L.back = r =>
                {
                    float sw = r.p.f == 1 ? -3 : r.p.f == 2 ? 2 : r.p.Atk ? -6 : 0;
                    r.F(new Poly(3, r.lx - 12, 54, r.lx + 6, 54, r.lx - 6 + sw * 0.4f, 6, r.lx - 24 + sw, 10), H("#c8372a"), 0.6f);
                };
                L.head = r =>
                {
                    float x = r.hx, y = r.hy - 1;
                    // tai
                    r.F(new Poly(1, x - 9, y + 8, x - 2, y + 10, x - 8, y + 22), H("#5a5560"), 0.4f);
                    r.F(new Poly(1, x + 1, y + 10, x + 8, y + 9, x + 5, y + 23), H("#5a5560"), 0.4f);
                    r.F(new Poly(0.5f, x + 2.5f, y + 11, x + 6, y + 10.5f, x + 4.8f, y + 18), H("#c08a8a"), 0, false);
                    r.F(new Ell(x - 1, y + 1, 13, 12), H("#5f5a66"), 0.5f);
                    // mõm
                    r.F(new Poly(3, x + 2, y + 2, x + 21, y - 3, x + 20, y - 8, x + 3, y - 10), H("#7a7480"), 0.4f);
                    r.F(new Poly(1, x + 4, y - 7, x + 19, y - 8, x + 16, y - 12, x + 5, y - 11), H("#d8d0c8"), 0.3f);
                    r.F(new Ell(x + 20.5f, y - 4.2f, 2.4f, 1.9f), H("#1a1418"), 0.2f, false);
                    r.F(new Poly(0.2f, x + 9, y - 8, x + 11, y - 8, x + 10, y - 11.5f), Col.White, 0, false);
                    r.F(new Poly(0.2f, x + 14, y - 8, x + 15.8f, y - 8, x + 15, y - 11), Col.White, 0, false);
                    // mắt đỏ
                    r.c.Glow(new Ell(x + 6, y + 2.5f, 2.2f, 1.2f, -0.3f), H("#ff3a2a", 0.8f), 3);
                    r.F(new Ell(x + 6, y + 2.5f, 2.2f, 1.2f, -0.3f), H("#ffd0a0"), 0, false);
                    r.F(new Seg(x + 3, y + 5, x + 9, y + 3.6f, 0.8f), H("#1e1a20"), 0, false);
                    // bờm
                    for (int i = 0; i < 6; i++) { float a = -2.3f + i * 0.55f; r.F(new Poly(1, x - 2 + Cos(a) * 6, y - 12 + Sin(a) * 4, x - 2 + Cos(a + 0.45f) * 6, y - 12 + Sin(a + 0.45f) * 4, x - 2 + Cos(a + 0.2f) * 15, y - 12 + Sin(a + 0.2f) * 9), H("#b8b0a8"), 0.4f); }
                };
                L.held = r => Claws(r, r.fx, r.fy, r.p.Atk ? 0.1f : -1.2f);
                L.heldBack = r => Claws(r, r.bx, r.by, -1.5f);
                return L;
            }, s => { s.name = "Hắc Lang Vương"; s.faceX = 6; });

            // Âm Thi Lão Quỷ · Quỷ Dị Môn: lão già còng da xám xanh, tóc râu trắng, gậy treo đèn hồn
            Hum("thi", () =>
            {
                var L = new Look("#2c3a32", "#1e2822", "#8fe0a0", "#3a4a3e") { hair = H("#f4f6f0"), skin = H("#93ad8e"), pants = H("#1e2420"), boot = H("#1a1e1c"), hunch = 4, jagged = true, eyes = EyeStyle.Glow, eye = H("#7dff9a"), blush = false, hemY = 6 };
                L.back = r =>
                {
                    for (int i = 0; i < 3; i++) { float x = r.lx - 18 + i * 4, y = 40 + i * 12 + r.p.f * 1.5f; r.c.Glow(new Ell(x, y, 3.5f, 5), H("#7dff9a", 0.3f), 6); r.F(new Ell(x, y, 2.3f, 3.4f), H("#d6ffe0", 0.7f), 0, false); }
                };
                L.hairBack = r => r.F(new Poly(3, r.hx - 13, r.hy + 4, r.hx - 2, r.hy + 8, r.hx - 8, r.hy - 30, r.hx - 18, r.hy - 24), r.L.hair, 0.4f);
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair, 1);
                    r.F(new Poly(2, r.hx + 3, r.hy - 7.5f, r.hx + 14, r.hy - 7.5f, r.hx + 8, r.hy - 22), r.L.hair, 0.4f);
                    r.F(new Ell(r.hx - 6, r.hy + 15, 4, 3.4f), r.L.hair, 0.4f);
                    r.F(new Seg(r.hx + 2, r.hy + 4, r.hx + 6, r.hy + 2.5f, 1.4f), r.L.hair, 0, false); r.F(new Seg(r.hx + 9, r.hy + 2.5f, r.hx + 13.5f, r.hy + 4, 1.3f), r.L.hair, 0, false);
                };
                L.heldBack = r =>
                {
                    float a = r.p.Atk ? 1.75f : 1.62f;
                    r.Pole(r.bx, r.by, a, 22, 46, H("#4a3a2a"), 1.6f);
                    float tx = r.bx + Cos(a) * 46, ty = r.by + Sin(a) * 46;
                    r.F(new Seg(tx, ty, tx - 6, ty - 2, 1.2f), H("#4a3a2a"), 0.3f);
                    float lx = tx - 6, ly = ty - 8;
                    r.F(new Seg(lx, ty - 2, lx, ly + 4, 0.5f), H("#2a2a2a"), 0, false);
                    r.c.Glow(new Ell(lx, ly, 5), H("#7dff9a", 0.55f), 10);
                    r.F(new RRect(lx, ly, 3.2f, 4.2f, 1.5f), H("#d6ffe0"), 0.3f);
                    r.F(new Seg(lx - 3.5f, ly + 4.2f, lx + 3.5f, ly + 4.2f, 0.8f), H("#2a3a2e"), 0, false);
                };
                L.held = r =>
                {
                    float y = r.fy + 6 + (r.p.Atk ? 2 : 0), x = r.fx + (r.p.Atk ? 8 : 2);
                    r.c.Glow(new Ell(x, y, r.p.Atk ? 6 : 3.5f), H("#7dff9a", 0.7f), r.p.Atk ? 12 : 7);
                    r.F(new Poly(1, x - 2.6f, y - 1, x + 2.6f, y - 1, x + 0.4f, y + 7), H("#bfffd0", 0.9f), 0, false);
                };
                return L;
            }, s => { s.name = "Âm Thi Lão Quỷ"; s.face = 62; s.faceX = 7; });

            // Bổ Thiên Khách · Bổ Thiên Các: nón lá rộng có mạng che, mặt nạ vàng, la bàn trận
            Hum("bo", () =>
            {
                var L = new Look("#2e2620", "#3a2e1e", "#e8c870", "#e8c870") { hair = H("#1a1614"), skin = H("#e8c870"), pants = H("#2a2018"), boot = H("#1e1814"), bodyW = 13, eyes = EyeStyle.None, blush = false, ear = false };
                L.chest = r =>
                {
                    r.F(new Seg(r.lx - 8, r.shY - 6, r.lx + 8, r.shY - 6, 0.6f), H("#e8c870"), 0, false);
                    r.F(new Ell(r.lx + 1, r.shY - 13, 2.4f), H("#e8c870"), 0.2f, true, 0.8f);
                };
                L.head = r =>
                {
                    float x = r.hx, y = r.hy;
                    r.F(new Ell(x, y, 14, 13.5f), H("#e8c870"), 0.7f);
                    r.F(new Ell(x + 5, y - 2, 9, 9), H("#f6dd8a"), 0.3f, false);
                    r.F(new Seg(x + 1.5f, y - 0.5f, x + 6.5f, y - 1.5f, 1.1f), H("#2a1e14"), 0, false);
                    r.F(new Seg(x + 9.5f, y - 1.5f, x + 13, y - 0.5f, 1), H("#2a1e14"), 0, false);
                    r.F(new Seg(x + 8, y - 5, x + 8, y - 10, 0.6f), H("#b08a3a"), 0, false);
                    // mạng che hai bên
                    r.F(new Poly(1, x - 18, y + 7, x - 13, y + 7, x - 12, y - 18, x - 19, y - 16), H("#3a2e22", 0.75f), 0, true, 0.8f);
                    // nón lá
                    r.F(new Poly(1.5f, x - 24, y + 6, x + 28, y + 6, x + 4, y + 22), H("#b89a5a"), 0.6f);
                    for (int i = 0; i < 4; i++) r.F(new Seg(x + 4, y + 21, x - 18 + i * 13, y + 7, 0.45f), H("#8a6a3a"), 0, false);
                    r.F(new Seg(x - 22, y + 6.5f, x + 26, y + 6.5f, 1), H("#e8c870"), 0, true, 0.8f);
                };
                L.held = r =>
                {
                    float x = r.fx + 7, y = r.fy + (r.p.Atk ? 6 : 3);
                    if (r.p.Atk) r.c.Glow(new Ell(x, y, 9), H("#ffe08a", 0.5f), 10);
                    r.F(new Ell(x, y, 8.5f), H("#c8a050"), 0.5f);
                    r.F(new Ell(x, y, 6), H("#3a2e1e"), 0, false);
                    for (int i = 0; i < 8; i++) { float a = i * PI / 4 + 0.2f; r.F(new Seg(x + Cos(a) * 3, y + Sin(a) * 3, x + Cos(a) * 5.4f, y + Sin(a) * 5.4f, 0.55f), H("#ffe08a"), 0, false); }
                    r.F(new Seg(x - 1.5f, y - 2.5f, x + 1.5f, y + 2.5f, 0.6f), H("#ff6a4a"), 0, false);
                };
                return L;
            }, s => { s.name = "Bổ Thiên Khách"; s.faceR = 30; s.face = 70; });

            // Tà Cực Ma Nữ · Tà Cực Tông: tóc đen búi đôi, sừng nhỏ, áo tím hồng, sáo ma âm và dải lụa
            Hum("ta", () =>
            {
                var L = new Look("#c03ea8", "#3a1838", "#ffb0e8", "#ff6ad0") { hair = H("#1e1624"), pants = H("#2a1228"), boot = H("#2a1828"), bodyW = 10.5f, hemW = 18, hemY = 5, eyes = EyeStyle.Lash, eye = H("#b0207a") };
                L.back = r =>
                {
                    float sw = r.p.f == 1 ? -3 : r.p.f == 2 ? 3 : r.p.Atk ? -5 : 0;
                    Ribbon(r, r.lx - 8, 52, r.lx - 30 + sw, 12, 3, 0, H("#ff9ae0", 0.9f));
                    Ribbon(r, r.lx + 2, 54, r.lx - 18 + sw, 6, 2.5f, 1.7f, H("#ffc0ec", 0.85f));
                };
                L.hairBack = r => r.F(new Poly(4, r.hx - 13, r.hy + 4, r.hx + 1, r.hy + 4, r.hx - 6, r.hy - 30, r.hx - 17, r.hy - 26), r.L.hair, 0.4f);
                L.hairFront = r =>
                {
                    r.F(new Ell(r.hx - 9, r.hy + 13, 5.5f), r.L.hair, 0.5f);
                    r.F(new Ell(r.hx + 7, r.hy + 15, 5), r.L.hair, 0.5f);
                    r.F(new Seg(r.hx - 6, r.hy + 17, r.hx + 5, r.hy + 19, 0.8f), H("#ff6ad0"), 0, true, 0.6f);
                    // sừng
                    r.F(new Poly(0.6f, r.hx - 2, r.hy + 12, r.hx + 2, r.hy + 12, r.hx - 2, r.hy + 20), H("#d0303a"), 0.3f);
                    r.F(new Poly(0.6f, r.hx + 10, r.hy + 11, r.hx + 13, r.hy + 10, r.hx + 13, r.hy + 18), H("#d0303a"), 0.3f);
                    r.HairCap(r.L.hair);
                    r.Bangs(r.L.hair, r.hx - 4, r.hx + 14, r.hy + 10.5f, r.hy + 3.5f, 6);
                    r.F(new Seg(r.hx + 13.5f, r.hy + 6, r.hx + 14.5f, r.hy - 12, 2.2f, 0.8f), r.L.hair, 0.4f);
                    r.F(new Ell(r.hx + 7.5f, r.hy + 6.5f, 1.2f, 1.6f), H("#ff4a7a"), 0, false); // ấn trán
                };
                L.held = r =>
                {
                    float a = r.p.Atk ? 0.35f : 1.25f;
                    r.Pole(r.fx, r.fy, a, 8, 15, H("#2a1a2a"), 1.3f);
                    for (int i = 0; i < 3; i++) { float t = -4 + i * 7; r.F(new Seg(r.fx + Cos(a) * t, r.fy + Sin(a) * t, r.fx + Cos(a) * (t + 1), r.fy + Sin(a) * (t + 1), 1.4f), H("#e8c870"), 0, false); }
                    if (r.p.Atk) for (int i = 0; i < 3; i++) r.F(new Seg(r.fx + 18 + i * 5, r.fy + 9 + i * 3, r.fx + 21 + i * 5, r.fy + 13 + i * 3, 0.9f), H("#ff7ae0", 0.9f - i * 0.2f), 0, false);
                };
                return L;
            }, s => s.name = "Tà Cực Ma Nữ");

            // Huyết Sát Phó Tướng · trấn thủ Kết Đan: giáp đỏ đen, mũ sừng, đại đao
            Hum("boss", () =>
            {
                var L = new Look("#8a1e1e", "#3a1214", "#e8b860", "#2a0e10") { hair = H("#1a0e10"), skin = H("#d8a080"), pants = H("#2a1012"), boot = H("#1a0c0e"), bodyW = 15, hemW = 19, armR = 6, legR = 6, hemY = 11, headR = 14, eyes = EyeStyle.Glow, eye = H("#ff4a2a"), blush = false, ear = false };
                L.back = r =>
                {
                    r.c.Glow(new Ell(r.lx, 48, 24, 30), H("#ff3a2a", 0.18f), 14);
                    float sw = r.p.f == 1 ? -3 : r.p.f == 2 ? 2 : r.p.Atk ? -6 : 0;
                    r.F(new Poly(3, r.lx - 13, 56, r.lx + 8, 56, r.lx - 4 + sw * 0.4f, 4, r.lx - 28 + sw, 8), H("#b0201e"), 0.6f);
                };
                L.chest = r =>
                {
                    r.F(new Poly(2, r.lx - 11, r.shY - 3, r.lx + 13, r.shY - 3, r.lx + 11, r.hipY + 6, r.lx - 9, r.hipY + 6), H("#3a1a1c"), 0.6f);
                    r.F(new Ell(r.lx + 2, r.shY - 12, 5, 5), H("#e8b860"), 0.5f);
                    r.F(new Ell(r.lx + 2, r.shY - 12, 2.4f), H("#ff5a3a"), 0, false);
                    // giáp vai có gai
                    r.F(new Ell(r.sbx, r.shY, 8, 6), H("#5a1a1c"), 0.6f);
                    r.F(new Ell(r.sfx + 1, r.shY, 8.5f, 6.5f), H("#6a2224"), 0.6f);
                    r.F(new Poly(0.5f, r.sfx - 2, r.shY + 5, r.sfx + 3, r.shY + 5, r.sfx + 1, r.shY + 12), H("#e8b860"), 0.3f);
                };
                L.head = r =>
                {
                    float x = r.hx, y = r.hy;
                    r.F(new Ell(x, y, 14, 13.5f), H("#3a1214"), 0.6f);
                    r.F(new Poly(3, x - 2, y + 2, x + 15, y + 2, x + 13, y - 11, x - 1, y - 13), H("#5a2a24"), 0.3f, false);
                    r.Face(x, y);
                    // mũ trụ
                    r.F(new Poly(2, x - 15, y - 4, x + 16, y + 3, x + 14, y + 9, x + 2, y + 16, x - 12, y + 12), H("#6a1a1a"), 0.6f);
                    r.F(new Seg(x - 13, y + 1, x + 15, y + 5, 1.3f), H("#e8b860"), 0.3f);
                    r.F(new Seg(x - 6, y + 12, x - 18, y + 26, 2.6f, 0.7f), H("#e8d8b0"), 0.4f);
                    r.F(new Seg(x + 8, y + 12, x + 18, y + 27, 2.6f, 0.7f), H("#e8d8b0"), 0.4f);
                    r.F(new Poly(0.6f, x + 1, y + 10, x + 5, y + 10, x + 3, y + 18), H("#ff5a3a"), 0.3f);
                };
                L.held = r =>
                {
                    float a = r.p.Atk ? 0.25f : 1.42f;
                    r.Pole(r.fx, r.fy, a, 26, 30, H("#3a2420"), 1.8f);
                    float tx = r.fx + Cos(a) * 30, ty = r.fy + Sin(a) * 30, nx = -Sin(a), ny = Cos(a), dx = Cos(a), dy = Sin(a);
                    if (r.p.Atk) r.c.Glow(new Ell(tx + dx * 8, ty + dy * 8, 10), H("#ff5a3a", 0.4f), 8);
                    r.F(new Poly(1, tx - dx * 2 - nx * 1.5f, ty - dy * 2 - ny * 1.5f, tx + dx * 20 - nx * 1, ty + dy * 20 - ny * 1, tx + dx * 15 - nx * 9, ty + dy * 15 - ny * 9, tx + dx * 2 - nx * 9, ty + dy * 2 - ny * 8), H("#d8dde6"), 0.5f);
                    r.F(new Seg(tx + dx * 3 - nx * 7.5f, ty + dy * 3 - ny * 7.5f, tx + dx * 15 - nx * 7.5f, ty + dy * 15 - ny * 7.5f, 0.8f), H("#ff5a3a"), 0, false);
                    r.F(new RRect(tx, ty, 2, 4, 1, a), H("#e8b860"), 0.3f);
                };
                return L;
            }, s => { s.name = "Huyết Sát Phó Tướng"; s.ax0 = -74; s.ax1 = 86; s.ay0 = -20; s.faceR = 30; });

            // ================= BINH CHỦNG CHÍNH ĐẠO =================

            // Kiếm Tu Vệ: đệ tử áo lam búi tóc, cầm kiếm
            Hum("kiem", () =>
            {
                var L = new Look("#3a86ad", "#24566f", "#a8e0ff", "#1c3e52") { hair = H("#1e2030"), pants = H("#1c3a4c"), boot = H("#24303a") };
                L.hairFront = r => { r.HairCap(r.L.hair); r.Bangs(r.L.hair, r.hx - 3, r.hx + 14, r.hy + 10.5f, r.hy + 4.5f, 4); r.F(new Ell(r.hx - 4, r.hy + 15, 4.5f, 4), r.L.hair, 0.4f); r.F(new Seg(r.hx - 8, r.hy + 14, r.hx, r.hy + 16, 0.8f), H("#a8e0ff"), 0, true, 0.7f); };
                L.held = r => r.Sword(r.fx, r.fy, r.ArmDirF + (r.p.Atk ? 0.1f : 0.35f), 25, H("#e6f2fa"), H("#a8e0ff"));
                return L;
            }, s => s.name = "Kiếm Tu Vệ");

            // Long Nhân Chiến Sĩ: giáp lam viền vàng, sừng nhỏ, khiên tròn, đao
            Hum("long", () =>
            {
                var L = new Look("#2a6a8a", "#1f4a62", "#e0bc70", "#14303e") { hair = H("#1a3a4a"), skin = H("#e8c8a8"), pants = H("#163040"), boot = H("#1e2830"), bodyW = 13.5f, armR = 5.4f, legR = 5.4f, hemY = 11 };
                L.hairFront = r =>
                {
                    r.F(new Seg(r.hx - 5, r.hy + 10, r.hx - 9, r.hy + 20, 2, 0.7f), H("#e0bc70"), 0.4f);
                    r.F(new Seg(r.hx + 6, r.hy + 11, r.hx + 9, r.hy + 21, 2, 0.7f), H("#e0bc70"), 0.4f);
                    // mũ giáp
                    r.F(new Poly(3, r.hx - 15, r.hy - 2, r.hx - 12, r.hy + 13, r.hx + 4, r.hy + 16, r.hx + 15, r.hy + 7, r.hx + 15, r.hy + 3, r.hx - 4, r.hy + 4), H("#2a6a8a"), 0.6f);
                    r.F(new Seg(r.hx - 13, r.hy + 3, r.hx + 15, r.hy + 4.5f, 1.1f), H("#e0bc70"), 0.3f, true, 0.8f);
                };
                L.heldBack = r =>
                {
                    float x = r.bx - 1, y = r.by + 4;
                    r.F(new Ell(x, y, 11, 12), H("#b8863a"), 0.6f);
                    r.F(new Ell(x, y, 7.5f, 8.2f), H("#2a6a8a"), 0.4f);
                    r.F(new Ell(x, y, 3), H("#e0bc70"), 0.3f);
                };
                L.held = r =>
                {
                    float a = r.ArmDirF + (r.p.Atk ? 0.1f : 0.3f), dx = Cos(a), dy = Sin(a), nx = -dy, ny = dx, x = r.fx, y = r.fy;
                    r.F(new Seg(x - dx * 5, y - dy * 5, x, y, 1.3f), H("#4a3a30"), 0);
                    r.F(new Poly(0.4f, x + dx * 2 + nx * 1.5f, y + dy * 2 + ny * 1.5f, x + dx * 22 + nx * 2.5f, y + dy * 22 + ny * 2.5f, x + dx * 25, y + dy * 25, x + dx * 18 - nx * 2.5f, y + dy * 18 - ny * 2.5f, x + dx * 2 - nx * 1.5f, y + dy * 2 - ny * 1.5f), H("#dfe6ee"), 0.4f);
                    r.F(new RRect(x + dx * 1.5f, y + dy * 1.5f, 1.4f, 3.6f, 1, a), H("#e0bc70"), 0.4f);
                };
                return L;
            }, s => s.name = "Long Nhân Chiến Sĩ");

            // Phù Sư: nón mành rộng vành, áo xanh nhạt, phù chú
            Hum("phu", () =>
            {
                var L = new Look("#4a97a8", "#2f6a78", "#f3ead6", "#1e4a54") { hair = H("#2a2420"), pants = H("#244a54"), boot = H("#24343a"), hemY = 5 };
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair); r.Bangs(r.L.hair, r.hx - 2, r.hx + 14, r.hy + 10.5f, r.hy + 5f, 4);
                    r.F(new Poly(2, r.hx - 8, r.hy + 11, r.hx + 9, r.hy + 12, r.hx + 7, r.hy + 22, r.hx - 6, r.hy + 21), H("#26363c"), 0.5f);
                    r.F(new Seg(r.hx - 8, r.hy + 12, r.hx + 9, r.hy + 13, 1.2f), H("#f3ead6"), 0.2f, true, 0.8f);
                    r.F(new Seg(r.hx - 1, r.hy + 26, r.hx + 1, r.hy + 17, 0.8f), H("#d9b46a"), 0, true, 0.7f);
                    r.F(new Seg(r.hx - 7, r.hy + 15, r.hx - 13, r.hy + 4, 0.9f, 0.5f), H("#ffd76a"), 0, true, 0.6f);
                };
                L.held = r => { r.Talisman(r.fx, r.fy, r.ArmDirF + 0.5f); if (r.p.Atk) r.c.Glow(new Ell(r.fx + 12, r.fy + 4, 3), H("#ffe08a", 0.8f), 8); };
                L.heldBack = r => r.Talisman(r.bx, r.by, r.ArmDirB - 0.3f, 0.8f);
                return L;
            }, s => s.name = "Phù Sư");

            // Thanh Loan Kỵ: kỵ sĩ cưỡi chim loan xanh, cầm thương
            Add("loan", (r, p) =>
            {
                var c = r.c; float b = p.bob, w = p.f == 1 ? 1 : p.f == 2 ? -1 : 0;
                Col body = H("#2f9a84"), wing = H("#5fd0b0"), tail = H("#a8f0d8");
                r.p = p;
                // đuôi
                for (int i = 0; i < 3; i++) c.F(new Seg(-14, 24 + b, -48 + i * 2, 18 + i * 9 + w * 2, 3.2f - i * 0.4f, 1.2f), i == 1 ? H("#ffd76a") : tail, 0.4f);
                // chân chim
                c.F(new Seg(-4, 14 + b, -4 + p.legB * 0.6f, 1, 1.4f), H("#e0a040"), 0); c.F(new Seg(6, 14 + b, 6 + p.legF * 0.6f, 1, 1.4f), H("#e0a040"), 0);
                c.F(new Ell(-4 + p.legB * 0.6f + 1.5f, 1, 3, 1.2f), H("#e0a040"), 0, true, 1); c.F(new Ell(6 + p.legF * 0.6f + 1.5f, 1, 3, 1.2f), H("#e0a040"), 0, true, 1);
                // cánh sau
                c.F(new Poly(3, -6, 30 + b, 8, 30 + b, -10, 50 + b + w * 4, -24, 44 + b + w * 3), wing.Mul(0.8f), 0.5f);
                // thân, cổ, đầu
                c.F(new Ell(0, 24 + b, 17, 11, -0.12f), body);
                c.F(new Seg(12, 26 + b, 20, 44 + b, 5, 4), body);
                c.F(new Ell(22, 47 + b, 6, 5.2f), body, 0.5f);
                c.F(new Poly(0.6f, 26, 49 + b, 34, 46 + b, 26, 44.5f + b), H("#ffb040"), 0.3f);
                c.F(new Ell(23.5f, 48.5f + b, 1.3f, 1.6f), H("#1e2030"), 0, false);
                for (int i = 0; i < 3; i++) c.F(new Seg(19, 51 + b, 12 - i * 3, 57 + b + i * 2, 1, 0.4f), H("#ffd76a"), 0, true, 0.6f);
                // yên
                c.F(new RRect(-1, 34 + b, 9, 3, 1.5f), H("#c0503a"), 0.4f);
                // kỵ sĩ (thân trên)
                float rx = 0, ry = 34 + b;
                c.F(new Seg(rx - 2, ry + 1, rx + 6, ry - 4, 3.4f), H("#244a60"));
                c.F(new Poly(2, rx - 7, ry + 2, rx + 6, ry + 2, rx + 7, ry + 18, rx - 6, ry + 18), H("#3a86ad"));
                c.F(new Poly(0.5f, rx - 2, ry + 18, rx + 2, ry + 18, rx + 6, ry + 10, rx + 4.5f, ry + 9), H("#a8e0ff"), 0.4f);
                float hx = rx + 2, hy = ry + 27;
                c.F(new Ell(hx, hy, 9, 8.7f), H("#f7d4b6"), 0.5f);
                c.F(new Ell(hx + 3, hy - 1, 1.2f, 1.8f), H("#2b2236"), 0, false); c.F(new Ell(hx + 7.5f, hy - 1, 1, 1.6f), H("#2b2236"), 0, false);
                c.F(new Poly(2, hx - 10, hy - 1, hx - 8, hy + 8, hx + 2, hy + 10, hx + 10, hy + 5, hx + 9, hy + 2, hx - 3, hy + 3), H("#2a6a8a"), 0.5f);
                c.F(new Seg(hx - 2, hy + 9, hx - 8, hy + 16, 1.2f, 0.5f), H("#e74c3c"), 0, true, 0.7f);
                // thương
                float a = p.Atk ? 0.05f : 0.45f, gx = rx + 8, gy = ry + 10;
                c.F(new Seg(gx - Cos(a) * 18, gy - Sin(a) * 18, gx + Cos(a) * 30, gy + Sin(a) * 30, 1.2f), H("#7a5a3a"), 0.3f);
                float tx = gx + Cos(a) * 30, ty = gy + Sin(a) * 30;
                c.F(new Poly(0.3f, tx + Cos(a) * 8, ty + Sin(a) * 8, tx - Sin(a) * 2.5f, ty + Cos(a) * 2.5f, tx + Sin(a) * 2.5f, ty - Cos(a) * 2.5f), H("#e6f2fa"), 0.3f);
                c.F(new Ell(gx, gy, 2.6f), H("#f7d4b6"));
                // cánh trước
                c.F(new Poly(3, -8, 28 + b, 10, 28 + b, -6, 44 + b + w * 5, -22, 36 + b + w * 3), wing, 0.5f);
                for (int i = 0; i < 3; i++) c.F(new Seg(-4 - i * 5, 30 + b, -8 - i * 5, 38 + b + w * 3, 0.5f), H("#2f7a6a"), 0, false);
            }, s => { s.name = "Thanh Loan Kỵ"; s.face = 61; s.faceX = 2; s.faceR = 16; });

            // Huyền Quy Công Thành: rùa đen khổng lồ cõng chòi công thành có chày húc
            Add("quy", (r, p) =>
            {
                var c = r.c; float b = p.bob * 0.4f, ram = p.Atk ? 8 : 0;
                Col skin = H("#3f6e62"), shell = H("#2d5a5a"), pat = H("#6fc3a8");
                // chân sau
                c.F(new Ell(-22 + p.legB * 0.4f, 6 + p.liftB * 0.5f, 8, 7), skin.Mul(0.8f)); c.F(new Ell(16 + p.legF * 0.4f, 6 + p.liftF * 0.5f, 8, 7), skin.Mul(0.8f));
                // đuôi
                c.F(new Poly(1, -40, 16 + b, -50, 12 + b, -38, 10 + b), skin);
                // đầu
                c.F(new Seg(34, 18 + b, 44 + ram * 0.2f, 22 + b, 6.5f, 6), skin);
                c.F(new Ell(48 + ram * 0.2f, 23 + b, 8.5f, 7), skin, 0.5f);
                c.F(new Ell(51 + ram * 0.2f, 26 + b, 1.6f, 1.8f), H("#ffd76a"), 0, false);
                c.F(new Seg(50 + ram * 0.2f, 19 + b, 56 + ram * 0.2f, 20 + b, 0.6f), H("#1e2a26"), 0, false);
                // mai rùa
                c.F(new Ell(-2, 22 + b, 40, 22), shell, 0.8f);
                c.F(new Ell(-2, 12 + b, 41, 4.5f), pat.Mul(0.75f), 0.3f);
                for (int i = -2; i <= 2; i++) c.F(new Poly(1.5f, -2 + i * 14 - 6, 26 + b - Math.Abs(i) * 3, -2 + i * 14 + 6, 26 + b - Math.Abs(i) * 3, -2 + i * 14 + 8, 18 + b - Math.Abs(i) * 2, -2 + i * 14 - 8, 18 + b - Math.Abs(i) * 2), pat, 0.3f, true, 1);
                // chân trước
                c.F(new Ell(-14 + p.legB * 0.4f, 4 + p.liftB * 0.5f, 8.5f, 7.5f), skin); c.F(new Ell(24 + p.legF * 0.4f, 4 + p.liftF * 0.5f, 8.5f, 7.5f), skin);
                // chòi công thành
                c.F(new RRect(-6, 50 + b, 17, 10, 1.5f), H("#8a5a34"), 0.5f);
                for (int i = 0; i < 3; i++) c.F(new RRect(-16 + i * 10, 50 + b, 2.5f, 4, 0.5f), H("#2a1a12"), 0, false);
                c.F(new Poly(1.5f, -28, 60 + b, 16, 60 + b, 8, 70 + b, -20, 70 + b), H("#c0503a"), 0.6f);
                c.F(new Seg(-6, 70 + b, -6, 82 + b, 0.8f), H("#4a3a30"), 0, false);
                c.F(new Poly(0.5f, -6, 82 + b, 6, 79 + b, -6, 76 + b), H("#6fc3a8"), 0.3f, true, 0.8f);
                // chày húc
                c.F(new Seg(-20 + ram, 40 + b, 44 + ram, 38 + b, 3.6f), H("#7a5232"), 0.4f);
                c.F(new Ell(46 + ram, 38 + b, 5, 5.5f), H("#9aa0a8"), 0.5f);
                c.F(new Seg(10, 42 + b, 10, 40 + b, 3), H("#4a3a30"), 0, false);
            }, s => { s.name = "Huyền Quy Công Thành"; s.ax0 = -66; s.ax1 = 66; s.ay0 = -16; s.face = 23; s.faceX = 48; s.faceR = 16; });

            // ================= BINH CHỦNG MA ĐẠO =================

            // Huyết Nô: nô binh trùm mũ, mắt đỏ, đao cong
            Hum("no", () =>
            {
                var L = new Look("#8a2a24", "#5a1a18", "#ff9a8a", "#3a1212") { hair = H("#2a1414"), skin = H("#c89a8a"), pants = H("#3a1414"), boot = H("#241010"), hunch = 2.5f, jagged = true, eyes = EyeStyle.Glow, eye = H("#ff3a2a"), blush = false, ear = false };
                L.hairFront = r =>
                {
                    r.F(new Sub(new Ell(r.hx - 1, r.hy + 1, 16.5f, 16), new Ell(r.hx + 6, r.hy - 3, 10, 9.5f)), H("#6a1e1c"), 0.6f);
                    r.F(new Poly(1, r.hx - 14, r.hy + 6, r.hx - 4, r.hy + 16, r.hx - 20, r.hy + 2), H("#6a1e1c"), 0.6f);
                };
                L.held = r =>
                {
                    float a = r.ArmDirF + (r.p.Atk ? 0.1f : 0.35f), dx = Cos(a), dy = Sin(a), nx = -dy, ny = dx, x = r.fx, y = r.fy;
                    r.F(new Seg(x - dx * 5, y - dy * 5, x, y, 1.3f), H("#2a1a14"), 0);
                    r.F(new Poly(0.4f, x + dx * 2 + nx * 1.4f, y + dy * 2 + ny * 1.4f, x + dx * 14 + nx * 3.5f, y + dy * 14 + ny * 3.5f, x + dx * 22 + nx * 1, y + dy * 22 + ny * 1, x + dx * 14 - nx * 1, y + dy * 14 - ny * 1, x + dx * 2 - nx * 1.4f, y + dy * 2 - ny * 1.4f), H("#cfc8c4"), 0.4f);
                };
                return L;
            }, s => { s.name = "Huyết Nô"; s.faceX = 5; });

            // Huyết Cung Thủ: cung thủ trùm khăn đỏ thẫm, cung đen
            Hum("cung", () =>
            {
                var L = new Look("#6a2226", "#3a1416", "#e5604d", "#2a0e10") { hair = H("#1e1214"), skin = H("#d8b098"), pants = H("#2a1214"), boot = H("#1e1012"), eyes = EyeStyle.Narrow, blush = false, ear = false, bodyW = 11 };
                L.hairFront = r =>
                {
                    r.HairCap(r.L.hair); r.Bangs(r.L.hair, r.hx - 2, r.hx + 14, r.hy + 10.5f, r.hy + 5.5f, 4);
                    r.F(new Sub(new Ell(r.hx - 1, r.hy + 2, 16, 15.5f), new Ell(r.hx + 7, r.hy - 2, 9.5f, 9.5f)), H("#8a2a2a"), 0.6f);
                    r.F(new Poly(1, r.hx + 2, r.hy - 7, r.hx + 16, r.hy - 7, r.hx + 15, r.hy - 13, r.hx + 2, r.hy - 13), H("#3a1416"), 0.3f); // khăn che miệng
                };
                L.held = r => r.Bow(r.fx, r.fy, r.p.Atk ? 0.15f : r.ArmDirF + 0.9f, 14, H("#3a2a2a"), r.p.Atk);
                L.back = r => { r.F(new RRect(r.lx - 9, 48, 3.2f, 10, 1.5f, -0.4f), H("#4a2a1e"), 0.4f); for (int i = 0; i < 3; i++) r.F(new Seg(r.lx - 6 + i * 1.4f, 56, r.lx - 3 + i * 1.6f, 63, 0.6f), H("#e5604d"), 0, false); };
                return L;
            }, s => s.name = "Huyết Cung Thủ");

            // Ma Tướng: giáp đen viền đỏ, mũ sừng, rìu lớn
            Hum("tuong", () =>
            {
                var L = new Look("#3a1a1e", "#2a1214", "#d9534f", "#1a0a0c") { hair = H("#1a0e10"), skin = H("#c89078"), pants = H("#1e0e10"), boot = H("#140a0c"), bodyW = 14, hemW = 18, armR = 5.6f, legR = 5.6f, hemY = 11, eyes = EyeStyle.Glow, eye = H("#ff5a3a"), blush = false, ear = false };
                L.chest = r => { r.F(new Ell(r.sfx + 1, r.shY, 7.5f, 6), H("#4a1e22"), 0.6f); r.F(new Poly(0.5f, r.sfx - 1, r.shY + 4, r.sfx + 3, r.shY + 4, r.sfx + 1, r.shY + 10), H("#d9534f"), 0.3f); };
                L.head = r =>
                {
                    float x = r.hx, y = r.hy;
                    r.F(new Ell(x, y, 14, 13.5f), H("#2a1214"), 0.6f);
                    r.F(new Poly(3, x - 1, y + 2, x + 15, y + 2, x + 13, y - 11, x, y - 12), H("#4a2420"), 0.3f, false);
                    r.Face(x, y);
                    r.F(new Poly(2, x - 15, y - 3, x + 16, y + 3, x + 14, y + 9, x + 2, y + 16, x - 12, y + 12), H("#3a1a1e"), 0.6f);
                    r.F(new Seg(x - 13, y + 1, x + 15, y + 5, 1.1f), H("#d9534f"), 0.3f);
                    r.F(new Poly(1, x - 8, y + 11, x - 3, y + 13, x - 14, y + 24), H("#e0d0c0"), 0.4f);
                    r.F(new Poly(1, x + 6, y + 13, x + 11, y + 11, x + 13, y + 24), H("#e0d0c0"), 0.4f);
                };
                L.held = r =>
                {
                    float a = r.p.Atk ? 0.3f : 1.4f;
                    r.Pole(r.fx, r.fy, a, 14, 20, H("#3a2420"), 1.5f);
                    float tx = r.fx + Cos(a) * 18, ty = r.fy + Sin(a) * 18, nx = -Sin(a), ny = Cos(a), dx = Cos(a), dy = Sin(a);
                    r.F(new Poly(1.5f, tx - dx * 3, ty - dy * 3, tx + dx * 3, ty + dy * 3, tx + dx * 7 - nx * 11, ty + dy * 7 - ny * 11, tx - dx * 8 - nx * 11, ty - dy * 8 - ny * 11), H("#c8ccd4"), 0.5f);
                    r.F(new Seg(tx + dx * 6 - nx * 10.5f, ty + dy * 6 - ny * 10.5f, tx - dx * 7 - nx * 10.5f, ty - dy * 7 - ny * 10.5f, 0.7f), H("#ff6a5a"), 0, false);
                };
                return L;
            }, s => s.name = "Ma Tướng");
        }

        static void Ribbon(Rig r, float x0, float y0, float x1, float y1, float amp, float ph, Col col)
        {
            int n = 8; float px = x0, py = y0; float dx = x1 - x0, dy = y1 - y0, l = (float)Math.Sqrt(dx * dx + dy * dy), nx = -dy / l, ny = dx / l;
            for (int i = 1; i <= n; i++)
            {
                float t = i / (float)n, w = Sin(t * 7 + ph + r.p.f) * amp * t;
                float qx = x0 + dx * t + nx * w, qy = y0 + dy * t + ny * w;
                r.F(new Seg(px, py, qx, qy, 1.5f - t * 0.6f), col, 0.2f, true, 0.6f); px = qx; py = qy;
            }
        }

        static void Claws(Rig r, float x, float y, float a)
        {
            for (int i = 0; i < 3; i++)
            {
                float b = a + (i - 1) * 0.35f;
                r.F(new Seg(x + Cos(b) * 2, y + Sin(b) * 2, x + Cos(b) * 9, y + Sin(b) * 9, 1.1f, 0.3f), H("#f0e8e0"), 0.2f, true, 1);
            }
            r.F(new Ell(x, y, 4.2f), H("#6a6470"), 0.4f);
        }
    }
}
