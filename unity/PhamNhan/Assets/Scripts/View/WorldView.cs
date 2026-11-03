using System.Collections.Generic;
using UnityEngine;
using PhamNhan.Sim;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.View
{
    /// <summary>Vẽ chiến trường: nền, thành, nhà, cây (tĩnh) và quân, chiêu, hiệu ứng (mỗi khung hình).</summary>
    public class WorldView
    {
        // bảng màu sáng hiện đại (theo bản 5–7)
        public static readonly Color ALLY = Shapes.Hex("#2f8fe0"), FOE = Shapes.Hex("#e5483d"), JADE = Shapes.Hex("#10a37f"), GOLD = Shapes.Hex("#f3c64a");
        const int O_DECAL = 2, O_ZONE = 3, O_BUILD = 5, O_UNIT = 10, O_TREE = 30, O_FX = 40, O_TOP = 60;

        readonly Transform root;
        public Painter P;
        Siege G;

        public WorldView(Transform parent)
        {
            root = new GameObject("World").transform; root.SetParent(parent, false);
            P = new Painter(root);
        }

        // ================= cảnh tĩnh =================
        public void BuildStatic(Siege g)
        {
            G = g;
            var st = new GameObject("Static").transform; st.SetParent(root, false);
            // nền: một tấm ảnh vẽ bằng mã, 4 điểm bản đồ trên một điểm ảnh
            const int S = 4; int tw = (int)(WW / S), th = (int)(WH / S);
            var tex = new Texture2D(tw, th, TextureFormat.RGB24, false); tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[tw * th];
            var eyeP = new V2[4]; for (int i = 0; i < 4; i++) eyeP[i] = EyePos(i);
            var rng = new System.Random(7);
            float[] noise = new float[64 * 64]; for (int i = 0; i < noise.Length; i++) noise[i] = (float)rng.NextDouble();
            float Noise(float x, float y)
            {
                x /= 220f; y /= 220f; int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y); float fx = x - xi, fy = y - yi;
                float a = noise[((yi & 63) * 64) + (xi & 63)], b = noise[((yi & 63) * 64) + ((xi + 1) & 63)], c = noise[(((yi + 1) & 63) * 64) + (xi & 63)], d = noise[(((yi + 1) & 63) * 64) + ((xi + 1) & 63)];
                fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
                return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
            }
            Color grassA = Shapes.Hex("#8fd062"), grassB = Shapes.Hex("#72bd55"), stone = Shapes.Hex("#e9e3d2"), stone2 = Shapes.Hex("#dcd4bf"), road = Shapes.Hex("#cfc3a2"), path = Shapes.Hex("#c9b98c");
            Color forest = Shapes.Hex("#3f9a52"), pond = Shapes.Hex("#5ec4e8"), rocks = Shapes.Hex("#b9b6a8"), lava = Shapes.Hex("#e0a06a"), camp = Shapes.Hex("#d8c99a"), plaza = Shapes.Hex("#f3eddc");
            for (int j = 0; j < th; j++)
            {
                float y = (th - 1 - j + 0.5f) * S; // hàng 0 của ảnh nằm ở đáy bản đồ
                for (int i = 0; i < tw; i++)
                {
                    float x = (i + 0.5f) * S;
                    float n = Noise(x, y);
                    Color c = Color.Lerp(grassA, grassB, n);
                    float dx = x - C.x, dy = y - C.y, r = Mathf.Sqrt(dx * dx + dy * dy), a = Mathf.Atan2(dy, dx);
                    // vùng bốn trận nhãn
                    for (int e = 0; e < 4; e++)
                    {
                        float de = M.Dist(x, y, eyeP[e].x, eyeP[e].y); if (de > 560) continue; float k = Mathf.Clamp01((560 - de) / 160f) * (0.75f + 0.25f * n);
                        if (r < BAR_R + 40) k *= Mathf.Clamp01((r - BAR_R) / 40f);
                        Color z = e == 0 ? rocks : e == 1 ? pond : e == 2 ? lava : forest;
                        if (e == 1) { float wv = Noise(x * 3.1f, y * 3.1f); if (wv < 0.5f) z = Color.Lerp(grassB, forest, 0.3f); }
                        c = Color.Lerp(c, z, k);
                    }
                    float dc = M.Dist(x, y, CAMP.x + 150, CAMP.y); if (dc < 520) c = Color.Lerp(c, camp, Mathf.Clamp01((520 - dc) / 120f) * 0.8f);
                    // đường tiến quân
                    if (r > WALL_R && Mathf.Abs(y - C.y) < 70 && x < C.x) c = Color.Lerp(c, path, 0.55f);
                    foreach (var g0 in G_OUT) { if (r > WALL_R && r < WALL_R + 900 && M.AngDiff(a, g0) * r < 60) c = Color.Lerp(c, path, 0.45f); }
                    if (r < WALL_R + 30)
                    {
                        Color cs = Color.Lerp(stone, stone2, n);
                        if (r > WALL_R - 6) cs = Color.Lerp(cs, grassB, 0.4f);
                        // vành đai và đại lộ
                        if (Mathf.Abs(r - RING_R) < RING_W) cs = road;
                        foreach (var g0 in G_OUT) if (M.AngDiff(a, g0) * r < AV_W && r > IN_R - 10) cs = road;
                        foreach (var g0 in G_IN) if (M.AngDiff(a, g0) * r < AV_W - 10 && r < IN_R + 10) cs = road;
                        if (r < IN_R - 20) cs = Color.Lerp(cs, plaza, 0.6f);
                        if (r < FLAG_R + 70) cs = Shapes.Hex("#f6e7c1");
                        c = cs;
                    }
                    px[j * tw + i] = c;
                }
            }
            tex.SetPixels32(px); tex.Apply();
            var gs = Sprite.Create(tex, new Rect(0, 0, tw, th), new Vector2(0, 0), 1f / S);
            var ground = new GameObject("Ground").AddComponent<SpriteRenderer>(); ground.transform.SetParent(st, false);
            ground.sprite = gs; ground.sortingOrder = 0; ground.transform.localPosition = new Vector3(0, -WH, 0);

            void Rect(float x, float y, float w, float h, float rot, Color col, int order)
            {
                var sr = new GameObject("b").AddComponent<SpriteRenderer>(); sr.transform.SetParent(st, false);
                sr.sprite = Shapes.Square; sr.color = col; sr.sortingOrder = order;
                sr.transform.localPosition = new Vector3(x, -y, 0); sr.transform.localScale = new Vector3(w, h, 1); sr.transform.localRotation = Quaternion.Euler(0, 0, -rot * Mathf.Rad2Deg);
            }
            void Disc(Sprite s, float x, float y, float d, Color col, int order)
            {
                var sr = new GameObject("d").AddComponent<SpriteRenderer>(); sr.transform.SetParent(st, false);
                sr.sprite = s; sr.color = col; sr.sortingOrder = order; sr.transform.localPosition = new Vector3(x, -y, 0); sr.transform.localScale = new Vector3(d, d, 1);
            }
            // nhà cửa
            Color[] roofs = { Shapes.Hex("#3d8a7a"), Shapes.Hex("#d9774a"), Shapes.Hex("#5a7fb0"), Shapes.Hex("#7a9a4a"), Shapes.Hex("#c9a24a") };
            foreach (var b in g.builds)
            {
                Color roof = b.kind == "palace" ? Shapes.Hex("#c8463c") : b.kind == "hall" ? Shapes.Hex("#b5523e") : roofs[b.v % roofs.Length];
                Rect(b.x + 6, b.y + 8, b.w, b.h, b.rot, new Color(0, 0, 0, 0.18f), O_BUILD);
                Rect(b.x, b.y, b.w, b.h, b.rot, roof, O_BUILD + 1);
                Rect(b.x, b.y, b.w, b.h * 0.12f, b.rot, Color.Lerp(roof, Color.white, 0.35f), O_BUILD + 2);
                Rect(b.x - M.Sin(b.rot) * b.h * 0.25f, b.y + M.Cos(b.rot) * b.h * 0.25f, b.w, b.h * 0.5f, b.rot, Color.Lerp(roof, Color.black, 0.12f), O_BUILD + 1);
                if (b.kind == "palace") Rect(b.x, b.y, b.w + 12, b.h + 12, 0, Shapes.Hex("#f3c64a", 0.9f), O_BUILD);
            }
            // tường thành và tháp canh
            foreach (var Rg in RINGS)
            {
                int n = Mathf.CeilToInt(M.TAU * Rg.R / 50f);
                for (int k = 0; k < n; k++)
                {
                    float am = (k + 0.5f) / n * M.TAU; bool gate = false; foreach (var gg in Rg.gates) if (M.AngDiff(am, gg) < Rg.half) gate = true;
                    if (gate) continue;
                    var p = Polar(am, Rg.R); float seg = M.TAU * Rg.R / n + 2;
                    Rect(p.x + 5, p.y + 7, seg, Rg.T, am + M.PI / 2, new Color(0, 0, 0, 0.2f), O_BUILD + 3);
                    Rect(p.x, p.y, seg, Rg.T, am + M.PI / 2, Shapes.Hex("#b9b2a4"), O_BUILD + 4);
                    Rect(p.x, p.y, seg, Rg.T * 0.45f, am + M.PI / 2, Shapes.Hex("#d6d0c2"), O_BUILD + 5);
                }
                foreach (var gg in Rg.gates) foreach (var s in new[] { -1f, 1f })
                    {
                        var p = Polar(gg + s * Rg.half, Rg.R);
                        Rect(p.x, p.y, Rg.T + 30, Rg.T + 30, gg, Shapes.Hex("#8d8475"), O_BUILD + 6);
                        Rect(p.x, p.y, Rg.T + 14, Rg.T + 14, gg, Shapes.Hex("#c8463c"), O_BUILD + 7);
                    }
            }
            // đài cờ trung tâm
            Disc(Shapes.Disc, C.x, C.y, FLAG_R * 2 + 20, Shapes.Hex("#d8c08a"), O_DECAL);
            Disc(Shapes.Ring, C.x, C.y, FLAG_R * 2, Shapes.Hex("#b08a40", 0.8f), O_DECAL);
            // doanh trại và lều Ma đạo
            foreach (var d in new[] { new V2(-180, -260), new V2(-120, -140), new V2(-200, -20), new V2(-130, 110), new V2(-190, 230), new V2(-60, -330), new V2(-60, 320), new V2(-260, 120), new V2(-250, -150) })
            {
                float x = CAMP.x + d.x + 60, y = CAMP.y + d.y;
                Rect(x + 5, y + 6, 64, 46, 0, new Color(0, 0, 0, 0.18f), O_BUILD); Rect(x, y, 64, 46, 0, Shapes.Hex("#f4f1e6"), O_BUILD + 1); Rect(x, y, 64, 8, 0, ALLY, O_BUILD + 2);
            }
            var wr = new System.Random(11);
            for (int dd = 0; dd < 4; dd++)
            {
                float ea = M.Deg(EYE_DEF[dd].a); int cnt = dd == 0 ? 6 : 3;
                for (int k = 0; k < cnt; k++)
                {
                    var p = Polar(ea + M.Deg(-6 + k * (dd == 0 ? 3 : 6)), 2470 + (k % 2) * 60);
                    Rect(p.x + 5, p.y + 6, 60, 44, 0, new Color(0, 0, 0, 0.18f), O_BUILD); Rect(p.x, p.y, 60, 44, 0, Shapes.Hex("#5a2a2a"), O_BUILD + 1); Rect(p.x, p.y, 60, 8, 0, FOE, O_BUILD + 2);
                }
            }
            // cây, đá, sen
            for (int i = 0; i < 1400; i++)
            {
                float x = (float)wr.NextDouble() * WW, y = (float)wr.NextDouble() * WH, rr = 18 + (float)wr.NextDouble() * 26;
                float rc = M.Dist(x, y, C.x, C.y); bool edge = y < 260 || y > WH - 260 || x < 220 || x > WW - 220;
                bool forestZone = M.Dist(x, y, eyeP[3].x, eyeP[3].y) < 520;
                if (!edge && !forestZone && wr.NextDouble() < 0.8) continue;
                if (rc < BAR_R + 110) continue; if (M.Dist(x, y, CAMP.x, CAMP.y) < 560) continue; if (x < C.x - WALL_R && Mathf.Abs(y - C.y) < 260) continue;
                bool nearEye = false; for (int e = 0; e < 4; e++) if (M.Dist(x, y, eyeP[e].x, eyeP[e].y) < 220) nearEye = true; if (nearEye) continue;
                Disc(Shapes.Soft, x + rr * 0.35f, y + rr * 0.45f, rr * 2.4f, new Color(0, 0, 0, 0.22f), O_TREE - 2);
                Color tc = forestZone ? Shapes.Hex("#2f7d40") : Color.Lerp(Shapes.Hex("#3f9a4a"), Shapes.Hex("#5fb85a"), (float)wr.NextDouble());
                Disc(Shapes.Disc, x, y, rr * 2, tc, O_TREE - 1);
                Disc(Shapes.Disc, x - rr * 0.22f, y - rr * 0.25f, rr * 1.2f, Color.Lerp(tc, Color.white, 0.18f), O_TREE);
            }
            for (int i = 0; i < 26; i++)
            {
                float a = (float)wr.NextDouble() * M.TAU, rr = 95 + (float)wr.NextDouble() * 320; float x = eyeP[1].x + Mathf.Cos(a) * rr, y = eyeP[1].y + Mathf.Sin(a) * rr * 0.85f;
                if (M.Dist(x, y, C.x, C.y) < BAR_R + 70) continue;
                Disc(Shapes.Disc, x, y, 26, Shapes.Hex("#6fcf7a"), O_DECAL); Disc(Shapes.Disc, x + 4, y - 3, 9, Shapes.Hex("#ff9ec0"), O_DECAL + 1);
            }
            for (int i = 0; i < 90; i++)
            {
                float a = (float)wr.NextDouble() * M.TAU, rr = 70 + (float)wr.NextDouble() * 430; int e = wr.NextDouble() < 0.6 ? 0 : 2;
                float x = eyeP[e].x + Mathf.Cos(a) * rr, y = eyeP[e].y + Mathf.Sin(a) * rr; if (M.Dist(x, y, C.x, C.y) < BAR_R + 60) continue;
                float s = 10 + (float)wr.NextDouble() * 22;
                Disc(Shapes.Disc, x + 3, y + 4, s * 2, new Color(0, 0, 0, 0.18f), O_BUILD); Disc(Shapes.Disc, x, y, s * 2, e == 0 ? Shapes.Hex("#9c9a90") : Shapes.Hex("#8a5a46"), O_BUILD + 1);
                Disc(Shapes.Disc, x - s * 0.25f, y - s * 0.3f, s, e == 0 ? Shapes.Hex("#c4c2b6") : Shapes.Hex("#e0784a"), O_BUILD + 2);
            }
        }

        // ================= mỗi khung hình =================
        static Color A(Color c, float a) { c.a = a; return c; }

        float vx0, vy0, vx1, vy1;
        bool In(float x, float y, float m = 80) { return x > vx0 - m && x < vx1 + m && y > vy0 - m && y < vy1 + m; }

        public void Render(Siege g, float time, bool strat, Rect view)
        {
            G = g; P.Begin(); vx0 = view.xMin; vy0 = view.yMin; vx1 = view.xMax; vy1 = view.yMax;
            float big = strat ? 3.2f : 1f; // phóng to biểu tượng khi xem sa bàn

            foreach (var d in g.decals)
            {
                if (!In(d.x, d.y, d.r)) continue;
                float al = Mathf.Clamp01(d.t / 4f);
                Color c = d.type == "inkR" ? new Color(0.65f, 0.1f, 0.08f, 0.28f) : d.type == "inkB" ? new Color(0.1f, 0.3f, 0.6f, 0.22f) : d.type == "crater" ? new Color(0.2f, 0.15f, 0.12f, 0.55f) : d.type == "blood" ? new Color(0.7f, 0.1f, 0.1f, 0.25f) : new Color(0.25f, 0.18f, 0.1f, 0.35f);
                P.Glow(d.x, d.y, d.r, A(c, c.a * al), O_DECAL);
            }
            // vân mạch trận nhãn khi Phá Vọng Nhãn đang soi
            if (g.veinGlow > 0)
                foreach (var e in g.eyes)
                {
                    if (e.dead || e.revealed) continue; var l = P.Line(e.vein.Count, 6, A(Shapes.C(ELEM[e.el].col), 0.35f * Mathf.Clamp01(g.veinGlow)), O_ZONE);
                    for (int i = 0; i < e.vein.Count; i++) l.SetPosition(i, new Vector3(e.vein[i].x, -e.vein[i].y, 0));
                }
            foreach (var z in g.zones)
            {
                Color c = z.type == "ho" ? Shapes.Hex("#2f8fe0") : z.type == "khon" ? FOE : GOLD;
                float pulse = 0.5f + 0.5f * Mathf.Sin(time * 4);
                P.Circle(z.x, z.y, z.r, A(c, 0.12f + 0.04f * pulse), O_ZONE); P.RingAt(z.x, z.y, z.r, A(c, 0.75f), O_ZONE + 1);
            }
            foreach (var t in g.teles)
            {
                float k = 1 - t.t / t.max; Color c = t.team == 1 ? (t.kind == "shock" ? Shapes.C(t.col) : (t.kind == "mtele" ? Shapes.C(t.col) : FOE)) : t.kind == "loi" ? Shapes.Hex("#ffd36b") : t.elem != Elem.None ? Shapes.C(ELEM[t.elem].col) : Shapes.Hex("#d6aa6a");
                P.Circle(t.x, t.y, t.r, A(c, 0.1f + 0.12f * k), O_ZONE); P.Circle(t.x, t.y, t.r * k, A(c, 0.22f), O_ZONE + 1); P.RingAt(t.x, t.y, t.r, A(c, 0.6f + 0.4f * k), O_ZONE + 2);
            }
            foreach (var w in g.waves) P.RingAt(w.x, w.y, w.r, new Color(0.78f, 0.66f, 1f, 0.7f * (1 - w.r / w.max) + 0.15f), O_ZONE + 2);

            // kết giới bốn cung
            if (g.barrier)
                for (int d = 0; d < 4; d++)
                {
                    if (g.sec[d]) continue; float A0 = DIRS[d].a - SEC_H + 0.01f, A1 = DIRS[d].a + SEC_H - 0.01f;
                    float pul = 0.5f + 0.5f * Mathf.Sin(time * 2 + d);
                    P.Arc(C.x, C.y, BAR_R, A0, A1, 60, new Color(1f, 0.55f, 0.8f, 0.12f + 0.05f * pul), O_FX - 2);
                    P.Arc(C.x, C.y, BAR_R, A0, A1, 10, new Color(1f, 0.45f, 0.75f, 0.7f), O_FX - 1);
                }
            if (g.breakT >= 0 && g.breakT < 1.6f && g.breakD >= 0)
            {
                float k = g.breakT / 1.6f, a0 = DIRS[g.breakD].a;
                P.Arc(C.x, C.y, BAR_R + k * 240, a0 - SEC_H, a0 + SEC_H, 14 * (1 - k) + 1, new Color(1f, 0.7f, 0.82f, 1 - k), O_FX);
            }
            foreach (var r in g.ripples) { var p = Polar(r.a, BAR_R); P.RingAt(p.x, p.y, 30 + r.t * (r.big ? 260 : 90), new Color(1f, 0.6f, 0.85f, 1 - r.t / 0.8f), O_FX); }

            // cờ
            if (g.phase == 3) { float k = g.flag / g.flagNeed; P.Arc(C.x, C.y, FLAG_R + 18, -M.PI / 2, -M.PI / 2 + M.TAU * k, 10, ALLY, O_ZONE + 3); }
            P.Rect(C.x, C.y - 40, 5, 80, Shapes.Hex("#6a5a40"), O_UNIT + 500);
            P.Rect(C.x + 22, C.y - 66, 40, 26, g.flagOwner == 0 ? ALLY : FOE, O_UNIT + 501);

            // trận nhãn
            foreach (var e in g.eyes)
            {
                if (!e.revealed || e.dead) continue; Color c = Shapes.C(ELEM[e.el].col);
                P.Glow(e.x, e.y, 120 * big, A(c, 0.35f), O_UNIT - 1);
                P.Circle(e.x, e.y, e.r * big, e.flash > 0 ? Color.white : Color.Lerp(c, Color.black, 0.15f), O_UNIT + 1);
                P.Draw(Shapes.Diamond, e.x, e.y - 40, 30 * big, 70 * big, e.vuln > 0 ? Color.white : c, O_UNIT + 2, 0);
                for (int s = 0; s < e.seals; s++) P.RingAt(e.x, e.y, (e.r + 16 + s * 14) * big, A(c, 0.85f), O_UNIT + 3);
                if (e.vuln > 0) P.RingAt(e.x, e.y, (e.r + 30 + 10 * Mathf.Sin(time * 10)) * big, new Color(1, 1, 1, 0.9f), O_UNIT + 3);
                Bar(e.x, e.y - 110 * (strat ? 1.6f : 1), 90 * big, 9 * big, e.hp / e.max, c);
            }

            // quân và tướng
            foreach (var u in g.units)
            {
                if (u.dead || !In(u.x, u.y)) continue;
                float r = u.r * (u.champ || u == g.boss ? big : (strat ? 1.8f : 1));
                bool ally = u.team == 0;
                float yo = u.rise > 0 ? u.rise * 12 : 0;
                int ord = O_UNIT + 2 + Mathf.Clamp((int)(u.y / 30f), 0, 220);
                P.Glow(u.x + 2, u.y + r * 0.45f, r * 1.2f, new Color(0, 0, 0, 0.22f), ord - 1);
                if (u.champ || u == g.boss)
                {
                    Color robe = Shapes.C(u.D.robe), ring = ally ? ALLY : FOE;
                    if (u == g.hero) ring = GOLD;
                    P.Circle(u.x, u.y - yo, r * 1.45f, ring, ord);
                    P.Circle(u.x, u.y - yo, r * 1.2f, u.flash > 0 ? Color.white : robe, ord + 1);
                    float fa = u.face; P.Circle(u.x + Mathf.Cos(fa) * r * 1.1f, u.y + Mathf.Sin(fa) * r * 1.1f - yo, r * 0.42f, Shapes.C(u.D.trim), ord + 2);
                    if (u == g.hero && g.hero.swing > 0) { }
                    if (u.stun > 0 && u.stun < 50) P.RingAt(u.x, u.y - r * 1.8f, r * 0.7f, new Color(1, 0.95f, 0.5f, 0.9f), ord + 3);
                    if (u.retreat) P.RingAt(u.x, u.y, r * 1.9f, new Color(1, 1, 1, 0.5f), ord + 3);
                    Bar(u.x, u.y - r * 2.1f, 54 * (strat ? big * 0.8f : 1), 6 * (strat ? big * 0.8f : 1), u.hp / u.max, ally ? Shapes.Hex("#1e9b4f") : FOE);
                }
                else
                {
                    Color body = Shapes.C(u.T.body), trim = Shapes.C(u.T.trim);
                    if (strat) { body = ally ? ALLY : FOE; }
                    P.Circle(u.x, u.y - yo, r, u.flash > 0 ? Color.white : body, ord);
                    P.Circle(u.x + Mathf.Cos(u.face) * r * 0.55f, u.y + Mathf.Sin(u.face) * r * 0.55f - yo, r * 0.45f, trim, ord + 1);
                    if (u.t == "quy") P.RingAt(u.x, u.y, r * 1.1f, trim, ord + 1);
                    if (u.hp < u.max && !strat && u.T.hp >= 200) Bar(u.x, u.y - r - 8, 26, 4, u.hp / u.max, ally ? Shapes.Hex("#1e9b4f") : FOE);
                }
            }

            // đạn
            foreach (var p in g.proj)
            {
                if (!In(p.x, p.y)) continue;
                if (p.kind == PKind.Arrow) P.Rect(p.x, p.y, 18, 2.5f, Shapes.Hex("#5a2a20"), O_FX, p.a);
                else if (p.kind == PKind.Bolt) { Color c = p.hasCol ? Shapes.C(p.col) : Shapes.Hex("#f3d9a0"); for (int i = 0; i < p.trail.Count; i++) P.Circle(p.trail[i].x, p.trail[i].y, 3 + i * 0.4f, A(c, 0.25f + i * 0.05f), O_FX); P.Glow(p.x, p.y, p.big ? 16 : 11, A(c, 0.9f), O_FX + 1); P.Circle(p.x, p.y, 3.5f, Color.white, O_FX + 2); }
                else if (p.kind == PKind.Sword) { float a = Mathf.Atan2(p.vy, p.vx); for (int i = 0; i < p.trail.Count; i++) P.Circle(p.trail[i].x, p.trail[i].y, 2 + i * 0.3f, new Color(0.63f, 0.9f, 1f, 0.08f + i * 0.04f), O_FX); P.Draw(Shapes.Diamond, p.x, p.y, 34, 8, new Color(0.85f, 0.97f, 1f, 1), O_FX + 1, a); }
                else if (p.kind == PKind.Pha) { P.Glow(p.x, p.y, 22, new Color(1, 0.84f, 0.47f, 0.8f), O_FX); P.Rect(p.x, p.y, 14, 20, Shapes.Hex("#ffd36b"), O_FX + 1, time * 6); }
            }

            // hiệu ứng
            foreach (var f in g.fx)
            {
                if (f.type == FxT.Glyph || (f.type != FxT.Slash && f.type != FxT.Bolt && !In(f.x, f.y, 300))) continue;
                float k = f.max > 0 ? Mathf.Clamp01(f.t / f.max) : 0; Color c = Shapes.C(f.col);
                switch (f.type)
                {
                    case FxT.P: P.Circle(f.x, f.y, f.s, A(c, k), O_FX + 3); break;
                    case FxT.Ring: P.RingAt(f.x, f.y, f.r0 + (f.r1 - f.r0) * (1 - k), A(c, k), O_FX + 2); break;
                    case FxT.Slash: P.Draw(Shapes.Arc, g.hero.x, g.hero.y - 6, 230, 230 * (f.dir > 0 ? 1 : -1), new Color(1, 0.96f, 0.8f, k), O_FX + 4, f.a); break;
                    case FxT.SlashC: P.Draw(Shapes.Arc, f.x, f.y - 6, 120, 120, A(c, k), O_FX + 4, f.a); break;
                    case FxT.SlashR: P.Draw(Shapes.Arc, f.x, f.y - 6, 200, 200, new Color(0.9f, 0.3f, 0.25f, k), O_FX + 4, f.a); break;
                    case FxT.Ghost: P.Circle(f.x, f.y, 14, A(c, 0.35f * k), O_FX + 1); break;
                    case FxT.Trail: P.Glow(f.x, f.y, 14, new Color(0.7f, 0.9f, 1f, 0.4f * k), O_FX + 1); break;
                    case FxT.Fall: { float q = 1 - k; var l = P.Line(2, 5, new Color(0.9f, 0.97f, 1f, 0.9f), O_FX + 5); l.SetPosition(0, new Vector3(f.x, -(f.y - 160 * (1 - q) - 40), 0)); l.SetPosition(1, new Vector3(f.x, -(f.y - 160 * (1 - q)), 0)); break; }
                    case FxT.Bolt: { var l = P.Line(f.pts.Length, 7 * k + 2, new Color(1f, 0.95f, 0.7f, k), O_FX + 6); for (int i = 0; i < f.pts.Length; i++) l.SetPosition(i, new Vector3(f.pts[i].x, -f.pts[i].y, 0)); break; }
                    case FxT.Ice: P.RingAt(f.x, f.y, f.r * (1.2f - 0.2f * k), new Color(0.7f, 0.88f, 1f, k), O_FX + 2); P.Circle(f.x, f.y, f.r * 0.9f, new Color(0.8f, 0.93f, 1f, 0.3f * k), O_FX + 1); break;
                    case FxT.Vine: P.RingAt(f.x, f.y, f.r, new Color(0.5f, 0.82f, 0.55f, k), O_FX + 2); P.Circle(f.x, f.y, f.r, new Color(0.5f, 0.82f, 0.55f, 0.18f * k), O_FX + 1); break;
                }
            }
            P.End();
        }

        void Bar(float x, float y, float w, float h, float k, Color c)
        {
            k = Mathf.Clamp01(k);
            P.Rect(x, y, w + 3, h + 3, new Color(0.06f, 0.16f, 0.13f, 0.75f), O_TOP);
            P.Rect(x - w / 2 + w * k / 2, y, w * k, h, c, O_TOP + 1);
        }
    }
}
