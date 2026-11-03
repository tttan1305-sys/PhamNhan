using System;
using System.Collections.Generic;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.Sim
{
    /// <summary>Đầu vào của người chơi trong một khung hình.</summary>
    public class PlayerInput
    {
        public float mx, my; public bool slash;
    }

    /// <summary>Trạng thái và vòng cập nhật của trận công thành Phù Vân.</summary>
    public partial class Siege
    {
        public Rng rng;
        public float R(float a, float b) { return rng.R(a, b); }
        public float Rnd() { return rng.Next(); }

        public bool AUTO;
        public PlayerInput IN = new PlayerInput();

        public float t, time, intro, overT, flag, flagNeed = 15, cmdT, mcmdT, lastStrat = -99, shake, flashA, breakT = -1, veinGlow, warnT, lastHeroFeed = -9;
        public Rgb flashCol = new Rgb(255, 255, 255);
        public int phase, open = -1, killed, totalEnemy, lost, nid = 1, pha, dan, loi, an, heroKills, maDown, castN, breakD = -1, k0, need, startAlly, flagOwner = 1;
        public float anT, loiT, hoCd, phaCd, danCd, anCd;
        public bool barrier, contested, stratOn;
        public string over; // null, "win", "lose"
        public bool[] sec = new bool[4];
        public string mode = "auto";
        public Plan plan; public Strat strat; public Banner banner; public string warnS;

        public List<Unit> units = new List<Unit>();
        public List<Eye> eyes = new List<Eye>();
        public List<Proj> proj = new List<Proj>();
        public List<Fx> fx = new List<Fx>();
        public List<Decal> decals = new List<Decal>();
        public List<FloatTxt> floats = new List<FloatTxt>();
        public List<Zone> zones = new List<Zone>();
        public List<Tele> teles = new List<Tele>();
        public List<Ripple> ripples = new List<Ripple>();
        public List<Wave> waves = new List<Wave>();
        public List<FeedItem> feed = new List<FeedItem>();
        public List<FeedItem> log = new List<FeedItem>();
        public List<Building> builds = new List<Building>();
        Dictionary<int, List<Building>> bgrid = new Dictionary<int, List<Building>>();
        const float BC = 128;

        public Unit hero, boss;
        public Unit[] allies, foes;

        public Siege(bool auto) { AUTO = auto; GenWorld(); NewGame(); }

        // ================= tiện ích =================
        public static float Dist(Body a, Body b) { return M.Dist(a.x, a.y, b.x, b.y); }
        public static float Dist(Body a, V2 b) { return M.Dist(a.x, a.y, b.x, b.y); }
        public static float Dist(V2 a, V2 b) { return M.Dist(a.x, a.y, b.x, b.y); }
        public static float Dist(Body a, float x, float y) { return M.Dist(a.x, a.y, x, y); }
        public static float Ang(Body a, Body b) { return M.Atan2(b.y - a.y, b.x - a.x); }
        public static float Ang(Body a, V2 b) { return M.Atan2(b.y - a.y, b.x - a.x); }
        public static float Ang(V2 a, Body b) { return M.Atan2(b.y - a.y, b.x - a.x); }
        public static float AngC(float x, float y) { return M.Atan2(y - C.y, x - C.x); }
        public static float AngC(Body b) { return AngC(b.x, b.y); }

        public void PushFeed(string s) { feed.Add(new FeedItem { s = s, t = t }); log.Add(new FeedItem { s = s, t = t }); if (feed.Count > 6) feed.RemoveAt(0); }
        public void ShowBanner(string title, string sub, float d, string g) { banner = new Banner { t = d, max = d, title = title, sub = sub ?? "", g = g }; }
        public void Warn(string s) { warnS = s; warnT = 3.5f; }
        public void FloatT(float x, float y, string s, Rgb col, float size)
        {
            floats.Add(new FloatTxt { x = x, y = y, s = s, col = col, size = size, t = 0.9f, max = 0.9f, vy = -55 });
            if (floats.Count > 60) floats.RemoveAt(0);
        }
        public void Sparks(float x, float y, Rgb col, int n)
        {
            for (int i = 0; i < n; i++) { float a = R(0, M.TAU), v = R(60, 220); fx.Add(new Fx { type = FxT.P, x = x, y = y, vx = M.Cos(a) * v, vy = M.Sin(a) * v - 40, grav = 300, t = R(0.25f, 0.5f), max = 0.5f, col = col, s = R(1.5f, 3) }); }
        }
        public void Boom(float x, float y, Rgb col, int n, float r) { fx.Add(new Fx { type = FxT.Ring, x = x, y = y, r0 = 10, r1 = r, t = 0.5f, max = 0.5f, col = col }); Sparks(x, y, col, n); }
        public void Glyph(string g, float x, float y, float d, Rgb col) { fx.Add(new Fx { type = FxT.Glyph, g = g, x = x, y = y, t = d, max = d, col = col }); }
        public void RingFx(float x, float y, float r0, float r1, float d, Rgb col) { fx.Add(new Fx { type = FxT.Ring, x = x, y = y, r0 = r0, r1 = r1, t = d, max = d, col = col }); }
        void Splat(float x, float y, int team, float r) { decals.Add(new Decal { x = x, y = y, r = r * R(1.3f, 2), t = 18, type = team == 1 ? "inkR" : "inkB", seed = (int)(Rnd() * 1000) }); }
        public void Shake(float s) { if (s > shake) shake = s; }
        public void Flash(float a, Rgb col) { if (a > flashA) flashA = a; flashCol = col; }

        public static string DirName(int d) { return DIRS[d].n; }
        public string NameOf(Unit u) { return u.T.name; }
        public string SrcName(Unit s) { return s == hero ? "Ngao Thiên" : (s != null ? s.T.name : "Quân ta"); }

        // ================= bản đồ thành =================
        void AddBuild(Building b)
        {
            b.c = M.Cos(b.rot); b.s = M.Sin(b.rot); b.hw = b.w / 2; b.hh = b.h / 2; builds.Add(b);
            float Rr = M.Hypot(b.hw, b.hh) + 28;
            for (int gx = (int)Math.Floor((b.x - Rr) / BC); gx <= (int)Math.Floor((b.x + Rr) / BC); gx++)
                for (int gy = (int)Math.Floor((b.y - Rr) / BC); gy <= (int)Math.Floor((b.y + Rr) / BC); gy++)
                {
                    int k = gx * 4096 + gy; List<Building> a;
                    if (!bgrid.TryGetValue(k, out a)) { a = new List<Building>(); bgrid[k] = a; }
                    a.Add(b);
                }
        }

        void GenWorld()
        {
            var wr = new Rng(777);
            builds.Clear(); bgrid.Clear();
            // vành ngoài và vành trong: dãy nhà theo vòng, chừa đại lộ và vài quảng trường
            void Band(float r0, float r1, float step, string kind)
            {
                for (float r = r0; r <= r1; r += step)
                {
                    float a = wr.R(0, 0.05f);
                    while (a < M.TAU)
                    {
                        float w = wr.R(44, 78), h = wr.R(32, step - 18);
                        float da = (w + wr.R(10, 22)) / r;
                        float am = a + da / 2;
                        bool skip = false;
                        foreach (var g in G_OUT) if (M.AngDiff(am, g) * r < AV_W + w / 2 + 14) skip = true;
                        if (!skip && wr.Next() < 0.1f) skip = true;
                        if (!skip)
                        {
                            var p = Polar(am, r);
                            AddBuild(new Building { x = p.x, y = p.y, rot = am + M.PI / 2, w = w, h = h, kind = kind, v = (int)(wr.Next() * 20) });
                        }
                        a += da;
                    }
                }
            }
            Band(RING_R + RING_W + 50, WALL_R - WALL_T / 2 - 50, 72, "house");
            Band(IN_R + IN_T / 2 + 56, RING_R - RING_W - 40, 66, "shop");
            // nội thành: điện các quanh quảng trường trung tâm
            for (int k = 0; k < 10; k++)
            {
                float am = M.Deg(18 + k * 36 + wr.R(-6, 6)), r = wr.R(420, 560);
                bool skip = false;
                foreach (var g in G_IN) if (M.AngDiff(am, g) * r < AV_W + 50) skip = true;
                if (M.AngDiff(am, -M.PI / 2) * r < AV_W + 50) skip = true;
                if (skip) continue;
                var p = Polar(am, r);
                AddBuild(new Building { x = p.x, y = p.y, rot = am + M.PI / 2, w = wr.R(110, 150), h = wr.R(70, 90), kind = "hall", v = k });
            }
            var pal = Polar(-M.PI / 2, 300);
            AddBuild(new Building { x = pal.x, y = pal.y, rot = 0, w = 260, h = 120, kind = "palace", v = 0 });
        }

        public bool BlockedAt(float x, float y, float r)
        {
            List<Building> a;
            if (!bgrid.TryGetValue((int)Math.Floor(x / BC) * 4096 + (int)Math.Floor(y / BC), out a)) return false;
            foreach (var b in a)
            {
                float dx = x - b.x, dy = y - b.y; float lx = dx * b.c + dy * b.s, ly = -dx * b.s + dy * b.c;
                if (M.Abs(lx) < b.hw + r && M.Abs(ly) < b.hh + r) return true;
            }
            return false;
        }

        void PushOBB(Unit u, Building b)
        {
            float dx = u.x - b.x, dy = u.y - b.y; float lx = dx * b.c + dy * b.s, ly = -dx * b.s + dy * b.c;
            float ex = b.hw + u.r, ey = b.hh + u.r;
            if (M.Abs(lx) >= ex || M.Abs(ly) >= ey) return;
            float px = ex - M.Abs(lx), py = ey - M.Abs(ly);
            float nx = lx, ny = ly;
            if (px < py) nx = (lx < 0 ? -1 : 1) * ex; else ny = (ly < 0 ? -1 : 1) * ey;
            u.x = b.x + nx * b.c - ny * b.s; u.y = b.y + nx * b.s + ny * b.c;
        }

        // ================= tạo trận =================
        Unit AddUnit(string ty, float x, float y)
        {
            var T = TYPES[ty];
            var u = new Unit { id = nid++, t = ty, T = T, team = T.team, x = x, y = y, hp = T.hp, max = T.hp, cd = Rnd() * (T.cd > 0 ? T.cd : 1), face = T.team == 1 ? M.PI : 0, retgt = Rnd() * 0.3f, r = T.r, ox = R(-70, 70), oy = R(-70, 70), steer = Rnd() < 0.5f ? 1 : -1 };
            u.pr = Dist(u, C); units.Add(u); return u;
        }

        List<V2> MakeVein(Eye e, float off)
        {
            float a2 = e.a + M.Deg(off);
            var P0 = new V2(e.x, e.y); var P2 = Polar(a2, BAR_R - 6);
            float mx = (P0.x + P2.x) / 2, my = (P0.y + P2.y) / 2; float dx = P2.x - P0.x, dy = P2.y - P0.y, L = M.Hypot(dx, dy); if (L < 1) L = 1;
            float bend = (off > 0 ? 1 : -1) * M.Min(140, L * 0.35f);
            var P1 = new V2(mx - dy / L * bend, my + dx / L * bend);
            var pts = new List<V2>(); int N = 36;
            for (int k = 0; k <= N; k++)
            {
                float tt = (float)k / N;
                float x = (1 - tt) * (1 - tt) * P0.x + 2 * (1 - tt) * tt * P1.x + tt * tt * P2.x, y = (1 - tt) * (1 - tt) * P0.y + 2 * (1 - tt) * tt * P1.y + tt * tt * P2.y;
                float j = (k == 0 || k == N) ? 0 : 7;
                pts.Add(new V2(x + R(-j, j), y + R(-j, j)));
            }
            e.va = a2; return pts;
        }

        public V2 PostPt(int d, float r = 2060) { float ea = M.Deg(EYE_DEF[d].a); return Polar(ea + M.AngSigned(ea, DIRS[d].a) * 0.45f, r); }
        public V2 OpenPt(float r) { return Polar(DIRS[open < 0 ? 0 : open].a, r); }

        void InitChamp(Unit u, string key) { u.champ = true; u.key = key; u.D = CH_DEF[key]; u.skT = R(1.5f, 3.5f); u.yanT = 0; u.healT = 4; u.sumT = 8; u.respawn = 0; u.retreat = false; u.hitT = -9; u.asg = 0; }

        void Squad(Unit lead, (string, int)[] list, float x0, float y0)
        {
            int k = 0;
            foreach (var (ty, n) in list) for (int i = 0; i < n; i++, k++) { var u = AddUnit(ty, x0 - (k % 12) * 24 - ((k / 12) % 2) * 12, y0 + (k / 12) * 26); u.lead = lead; }
        }

        void Def(string ty, float x, float y, Role role) { var u = AddUnit(ty, x, y); u.role = role; }

        public void NewGame()
        {
            rng = new Rng(20261102);
            t = 0; time = TIME; phase = 1; barrier = true; sec = new bool[4]; open = -1;
            units.Clear(); eyes.Clear(); proj.Clear(); fx.Clear(); decals.Clear(); floats.Clear(); zones.Clear(); teles.Clear(); ripples.Clear(); waves.Clear(); feed.Clear(); log.Clear();
            banner = null; warnS = null; warnT = 0; shake = 0; flashA = 0; killed = 0; totalEnemy = 0; lost = 0; flag = 0; contested = false; over = null; overT = 0;
            mode = "auto"; plan = null; cmdT = 3; mcmdT = 2; strat = null; lastStrat = -99; stratOn = false; nid = 1; lastHeroFeed = -9; intro = INTRO;
            pha = 3; dan = 3; loi = 5; an = 3; anT = 0; loiT = 0; hoCd = 0; phaCd = 0; danCd = 0; anCd = 0; breakT = -1; breakD = -1; flagOwner = 1; heroKills = 0; maDown = 0; castN = 0; veinGlow = 0;

            for (int i = 0; i < EYE_DEF.Length; i++)
            {
                var d = EYE_DEF[i]; var p = EyePos(i);
                var e = new Eye { i = i, el = d.el, weak = d.weak, x = p.x, y = p.y, a = M.Deg(d.a), hp = d.hp, max = d.hp, r = 30, team = 1, seals = d.seals, maxSeals = d.seals, name = ELEM[d.el].n, place = d.place, toolT = d.toolT };
                e.vein = MakeVein(e, d.voff);
                float ha = R(0, M.TAU), hr = R(60, 150); e.hx = e.x + M.Cos(ha) * hr; e.hy = e.y + M.Sin(ha) * hr;
                eyes.Add(e);
            }
            float X = CAMP.x, Y = CAMP.y;
            hero = AddUnit("hero", X + 200, Y + 350);
            hero.mp = 420; hero.mpMax = 420; hero.st = 100; hero.face = 0.3f; hero.champ = true; hero.key = "hero"; hero.D = CH_DEF["hero"]; hero.asg = 3;
            Unit Mk(string k, float x, float y) { var u = AddUnit(k, x, y); InitChamp(u, k); u.face = 0; return u; }
            var thach = Mk("thach", X + 180, Y - 580); var nguyet = Mk("nguyet", X + 160, Y - 500); var viem = Mk("viem", X + 420, Y - 90); var thanh = Mk("thanh", X + 420, Y + 60);
            allies = new[] { hero, viem, thach, nguyet, thanh };
            Squad(thach, new[] { ("long", 10), ("kiem", 8), ("phu", 4) }, X + 130, Y - 610);
            Squad(nguyet, new[] { ("kiem", 10), ("phu", 6), ("long", 4) }, X + 130, Y - 530);
            Squad(viem, new[] { ("kiem", 10), ("phu", 8), ("long", 4), ("quy", 1) }, X + 380, Y - 110);
            Squad(hero, new[] { ("loan", 6), ("kiem", 8), ("long", 4), ("phu", 4), ("quy", 1) }, X + 130, Y + 390);
            Squad(thanh, new[] { ("long", 8), ("loan", 4), ("kiem", 8) }, X + 380, Y + 10);
            foreach (var u in units) if (u.t == "loan" && u.lead == hero) u.x += 60;

            Unit MkM(string k)
            {
                int d = CH_DEF[k].post; var p = PostPt(d); var u = AddUnit(k, p.x, p.y); InitChamp(u, k); u.asg = d; u.face = AngC(p.x, p.y); u.home = p; u.hasHome = true; return u;
            }
            var lang = MkM("lang"); var thi = MkM("thi"); var bo = MkM("bo"); var ta = MkM("ta");
            foreach (var m in new[] { lang, thi, bo, ta })
            {
                int d = m.D.post;
                for (int i = 0; i < 7; i++) { var u = AddUnit(i < 6 ? "no" : "tuong", m.x + R(-60, 60), m.y + R(-60, 60)); u.role = Role.Ret; u.lead = m; }
                float ea = M.Deg(EYE_DEF[d].a);
                for (int i = 0; i < 15; i++) { float a = ea + M.Deg(R(-9, 9)), rr = R(2040, 2390); Def(i < 10 ? "no" : i < 14 ? "cung" : "tuong", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Guard); }
            }
            for (int i = 0; i < 16; i++) { float a = M.Deg(R(175, 187)), rr = R(2280, 2400); Def(i < 10 ? "no" : i < 15 ? "cung" : "tuong", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Van); }
            foreach (var g in G_OUT)
            {
                for (int i = 0; i < 8; i++) { float a = g + M.Deg(R(-14, 14)), rr = R(1695, 1770); Def(i < 5 ? "no" : "cung", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Def); }
                float px = -M.Sin(g), py = M.Cos(g);
                for (int i = 0; i < 5; i++) { var p = Polar(g, R(1250, 1580)); float o = R(-55, 55); Def(i < 4 ? "no" : "tuong", p.x + px * o, p.y + py * o, Role.Def); }
                for (int i = 0; i < 6; i++) { float a = g + M.Deg(R(-3.5f, 3.5f)), rr = RING_R + R(-35, 35); Def(i < 4 ? "no" : "cung", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Def); }
            }
            foreach (var g in G_IN)
            {
                float px = -M.Sin(g), py = M.Cos(g);
                for (int i = 0; i < 8; i++) { var p = Polar(g, R(740, 830)); float o = R(-55, 55); Def(i < 5 ? "no" : i < 7 ? "cung" : "tuong", p.x + px * o, p.y + py * o, Role.Def); }
            }
            for (int i = 0; i < 17; i++) { float a = M.Deg(R(-25, 205)), rr = R(170, 470); Def(i < 14 ? "no" : "tuong", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Def); }
            foreach (var ad in new float[] { 45, 135, 225, 315 })
            {
                for (int i = 0; i < 5; i++) { float a = M.Deg(ad + R(-3, 3)), rr = RING_R + R(-30, 30); Def("no", C.x + M.Cos(a) * rr, C.y + M.Sin(a) * rr, Role.Pat); }
                var p = Polar(M.Deg(ad), RING_R); Def("tuong", p.x, p.y, Role.Pat);
            }
            foreach (var u in units) if (u.team == 1 && !u.hasHome) { u.home = new V2(u.x, u.y); u.hasHome = true; }
            // đẩy quân ra khỏi nhà
            foreach (var u in units) for (int k = 0; k < 6 && BlockedAt(u.x, u.y, u.r); k++) { u.x += R(-30, 30); u.y += R(-30, 30); }
            boss = AddUnit("boss", C.x - 230, C.y); boss.role = Role.Boss; boss.active = false; boss.cdT = 3; boss.cdK = 7; boss.home = new V2(C.x - 230, C.y); boss.hasHome = true; boss.key = "boss"; boss.D = CH_DEF["boss"];
            foes = new[] { boss, lang, thi, bo, ta };
            totalEnemy = EnemyCount(); startAlly = AllyCount();
            PushFeed("Năm tướng Chính đạo áp sát Phù Vân Thành");
            ShowBanner("Công thành · Phù Vân Thành", "Năm tướng mỗi phe · bốn trận nhãn ẩn nuôi bốn cung kết giới", 8.4f, "攻");
            SetMode("auto", true); PlanSet("split", -1, "", true);
        }

        public int AllyCount() { int n = 0; foreach (var u in units) if (!u.dead && u.team == 0 && !u.champ && u.t != "quy") n++; return n; }
        public int EnemyCount() { int n = 0; foreach (var u in units) if (!u.dead && u.team == 1 && u != boss && !u.champ) n++; return n; }

        // ================= tiện ích chiến đấu =================
        public int SecOf(float a) { int b = 0; float bd = 9; for (int d = 0; d < 4; d++) { float q = M.AngDiff(a, DIRS[d].a); if (q < bd) { bd = q; b = d; } } return b; }
        public bool SecOpenA(float a) { return !barrier || sec[SecOf(a)]; }

        public bool Reachable(Unit u, Body e)
        {
            if (!barrier || u.team == 1) return true;
            if (e.IsEye) return true;
            float epr = M.Dist(e.x, e.y, C.x, C.y);
            bool ui = u.pr < BAR_R, ei = epr < BAR_R; if (ui == ei) return true;
            return SecOpenA(AngC(e)) && SecOpenA(AngC(u));
        }
        public bool BarBlock(Unit u, V2 pt) { return barrier && u.team == 0 && u.pr >= BAR_R && M.Dist(pt.x, pt.y, C.x, C.y) < BAR_R && !SecOpenA(AngC(pt.x, pt.y)); }

        public Unit NearestEnemy(Body u, float maxR, Func<Unit, bool> filter = null)
        {
            Unit best = null; float bd = maxR;
            foreach (var e in units)
            {
                if (e.dead || e.team == u.team) continue;
                if (e == boss && !boss.active && u.team == 0 && Dist(u, e) > 260) continue;
                float dx = e.x - u.x, dy = e.y - u.y; if (M.Abs(dx) > bd + 30 || M.Abs(dy) > bd + 30) continue;
                if (filter != null && !filter(e)) continue;
                float d = M.Hypot(dx, dy) - e.r; if (d < bd) { bd = d; best = e; }
            }
            return best;
        }
        public Unit NearestEnemyTeam(V2 p, int team, float maxR)
        {
            Unit best = null; float bd = maxR;
            foreach (var e in units) { if (e.dead || e.team == team) continue; float d = Dist(e, p) - e.r; if (d < bd) { bd = d; best = e; } }
            return best;
        }
        public IEnumerable<Eye> LiveEyes() { foreach (var e in eyes) if (!e.dead && e.revealed) yield return e; }
        public Eye VulnEye(Body u, float r) { foreach (var e in eyes) if (!e.dead && e.revealed && e.vuln > 0 && Dist(u, e) < r) return e; return null; }
        public int CountEnemies(V2 p, float r, int team = 1) { int n = 0; foreach (var e in units) { if (e.dead || e.team != team) continue; if (Dist(e, p) < r) n++; } return n; }
        public Zone InZone(Body u, string type) { foreach (var z in zones) if (z.type == type && M.Dist(u.x, u.y, z.x, z.y) < z.r) return z; return null; }
        public int Pow(int d, int team, float Rr)
        {
            var e = eyes[d]; int s = 0;
            foreach (var u in units)
            {
                if (u.dead || u.team != team || u == boss) continue;
                if (team == 0 && M.Abs(u.x - CAMP.x) < 650 && M.Abs(u.y - CAMP.y) < 650) continue;
                float dx = u.x - e.x, dy = u.y - e.y; if (dx * dx + dy * dy < Rr * Rr) s += u.champ ? 14 : u.t == "tuong" ? 3 : u.t == "quy" ? 4 : 1;
            }
            return s;
        }
        static int RegOf(float r) { return r > WALL_R ? 0 : r > IN_R ? 1 : 2; }
        static float? OnAvenue(float a, float r) { foreach (var g in G_OUT) if (M.AngDiff(a, g) * r < AV_W - 12) return g; return null; }
        static V2 RingRoute(float au, float ru, float ad, V2 dest)
        {
            float dA = M.AngDiff(au, ad); if (dA < 0.26f) return dest;
            if (M.Abs(ru - RING_R) > 42) { var g = OnAvenue(au, ru); return Polar(g ?? au, RING_R); }
            float s = M.AngSigned(au, ad); return Polar(au + M.Sign(s) * M.Min(0.2f, dA), RING_R);
        }
        static V2 CircDetour(Unit u, V2 dest, float Rr, float ru, float au)
        {
            float dx = dest.x - u.x, dy = dest.y - u.y, L2 = dx * dx + dy * dy; if (L2 < 1) return dest;
            float tt = M.Clamp(((C.x - u.x) * dx + (C.y - u.y) * dy) / L2, 0, 1); float px = u.x + dx * tt, py = u.y + dy * tt;
            if (M.Dist(px, py, C.x, C.y) >= Rr) return dest;
            float ad = M.Atan2(dest.y - C.y, dest.x - C.x); float s = M.AngSigned(au, ad);
            return Polar(au + (s == 0 ? 1 : M.Sign(s)) * M.Min(0.3f, M.Abs(s)), M.Max(ru, Rr + 110));
        }
        int BestOpen(float au, float ad) { int best = -1; float bd = 1e9f; for (int s = 0; s < 4; s++) { if (!sec[s]) continue; float d = M.AngDiff(au, DIRS[s].a) + M.AngDiff(ad, DIRS[s].a) * 0.5f; if (d < bd) { bd = d; best = s; } } return best; }

        public V2 NavPoint(Unit u, V2 dest)
        {
            float ru = M.Dist(u.x, u.y, C.x, C.y); float rd = M.Dist(dest.x, dest.y, C.x, C.y);
            float au = M.Atan2(u.y - C.y, u.x - C.x); float ad = M.Atan2(dest.y - C.y, dest.x - C.x);
            float[] gates = G_OUT;
            if (u.team == 0 && barrier)
            {
                bool uo = ru >= BAR_R - 4, dout = rd >= BAR_R;
                if (uo && dout) return CircDetour(u, dest, BAR_R + 50, ru, au);
                if (uo && !dout)
                {
                    int s = BestOpen(au, ad);
                    if (s < 0) return CircDetour(u, Polar(ad, BAR_R + 140), BAR_R + 50, ru, au);
                    float sa = DIRS[s].a; if (!(SecOf(au) == s && M.AngDiff(au, sa) < SEC_H - 0.12f)) return CircDetour(u, Polar(sa, BAR_R + 140), BAR_R + 50, ru, au);
                    var gl0 = new List<float>(); foreach (var g in G_OUT) if (sec[SecOf(g)]) gl0.Add(g); gates = gl0.ToArray();
                }
                else if (!uo && dout)
                {
                    int s = BestOpen(au, ad);
                    if (s >= 0) { float sa = DIRS[s].a; if (!(SecOf(au) == s && ru > WALL_R + 10)) { dest = Polar(sa, BAR_R + 140); rd = BAR_R + 140; ad = sa; } }
                }
            }
            int gu = RegOf(ru), gd = RegOf(rd);
            if (gu == 0 && gd == 0) return CircDetour(u, dest, WALL_R + 40, ru, au);
            if (gu == gd) return gu == 1 ? RingRoute(au, ru, ad, dest) : dest;
            var Rg = (gu == 0 || (gu == 1 && gd == 0)) ? RINGS[0] : RINGS[1]; bool inward = gu == 0 || (gu == 1 && gd == 2);
            var gl = Rg == RINGS[0] && gu == 0 ? gates : Rg.gates;
            if (gl.Length == 0) gl = Rg.gates;
            float best = gl[0], bdd = 1e9f; foreach (var g in gl) { float d = M.AngDiff(au, g) + 0.6f * M.AngDiff(ad, g); if (d < bdd) { bdd = d; best = g; } }
            float side = inward ? 1 : -1;
            if (M.AngDiff(au, best) < Rg.half * 0.7f && M.Abs(ru - Rg.R) < 90) return Polar(best, Rg.R - side * 80);
            var ap = Polar(best, Rg.R + side * 70);
            if (gu == 1) return RingRoute(au, ru, best, ap);
            if (gu == 0) return CircDetour(u, ap, WALL_R + 40, ru, au);
            return ap;
        }

        public void MoveTo(Unit u, V2 dest, float spd, float dt)
        {
            var wp = NavPoint(u, dest); float a = Ang(u, wp); float d = Dist(u, wp); if (d < 2) return; float s = M.Min(d, spd * dt);
            float la = u.r + 8;
            if (BlockedAt(u.x + M.Cos(a) * la, u.y + M.Sin(a) * la, u.r * 0.8f))
            {
                foreach (var da in new[] { 0.6f, 1.2f, 1.8f })
                {
                    float a2 = a + da * u.steer; if (!BlockedAt(u.x + M.Cos(a2) * la, u.y + M.Sin(a2) * la, u.r * 0.8f)) { a = a2; break; }
                    float a3 = a - da * u.steer; if (!BlockedAt(u.x + M.Cos(a3) * la, u.y + M.Sin(a3) * la, u.r * 0.8f)) { a = a3; break; }
                }
            }
            u.x += M.Cos(a) * s; u.y += M.Sin(a) * s; u.face = a; u.moving = true;
        }
        public void MoveTo(Unit u, Body b, float spd, float dt) { MoveTo(u, b.P, spd, dt); }
        float SlowOf(Unit u) { if (u.team == 0 && InZone(u, "khon") != null) return 0.6f; return 1; }

        public class HitOpt { public bool crit, show; public float stun; public Elem elem = Elem.None; public int cast = -1; }

        public void Damage(Body tg, float amt, Unit src, HitOpt o = null)
        {
            if (o == null) o = new HitOpt();
            if (tg == null || tg.dead) return;
            if (tg is Eye) { HitEye((Eye)tg, amt, src, o); return; }
            var u = (Unit)tg;
            if (u == hero) { if (hero.invul > 0 || hero.dead) return; amt *= 0.7f; }
            else if (u.champ) amt *= 0.8f;
            if (u.team == 0 && InZone(u, "ho") != null) amt *= 0.7f;
            u.hp -= amt; u.flash = 0.09f; u.hitT = t;
            if (src == hero || o.show) FloatT(u.x + R(-8, 8), u.y - u.r * 2 - 6, (o.crit ? "Bạo kích " : "") + "-" + M.Fmt(amt), o.crit ? Rgb.Hex("#ffd36b") : new Rgb(255, 255, 255), o.crit ? 18 : (u == boss || u.champ ? 16 : 13));
            else if (u == hero) FloatT(u.x + R(-8, 8), u.y - 40, "-" + M.Fmt(amt), Rgb.Hex("#ff7a6b"), 14);
            else if (u.champ && amt >= 120) FloatT(u.x + R(-8, 8), u.y - 44, "-" + M.Fmt(amt), u.team == 1 ? Rgb.Hex("#ffd0c0") : Rgb.Hex("#ffb0a0"), 13);
            if (o.stun > 0) u.stun = M.Max(u.stun, u == boss ? o.stun * 0.3f : u.champ ? o.stun * 0.6f : o.stun);
            if (u.hp <= 0) Kill(u, src);
        }

        void HitEye(Eye e, float amt, Unit src, HitOpt o)
        {
            if (e.dead || !e.revealed) return; var E = ELEM[e.el];
            if (e.seals > 0)
            {
                var el = o.elem;
                if (el != Elem.None && (el == e.weak || el == Elem.Pha))
                {
                    if (o.cast >= 0 && e.lastCast == o.cast) return; e.lastCast = o.cast;
                    e.seals--; e.flash = 0.25f;
                    Glyph(ELEM[el].g, e.x, e.y - 150, 1, ELEM[el].col);
                    RingFx(e.x, e.y - 50, 20, 120, 0.5f, E.col); Sparks(e.x, e.y - 60, E.col, 22); Shake(Dist(hero, e) < 800 ? 8 : 0);
                    FloatT(e.x, e.y - 178, (el == Elem.Pha ? "Phá Trận Phù phá 1 ấn" : (src != null && src != hero ? NameOf(src) + ": " : "") + ELEM[el].n + " khắc " + E.n + ", phá 1 ấn") + (e.seals > 0 ? " · còn " + e.seals : ""), Rgb.Hex("#ffd36b"), 16);
                    if (e.seals == 0)
                    {
                        e.vuln = VULN_T; e.lastCast = -1;
                        if (Dist(hero, e) < 1500) Warn("Lõi trận nhãn " + E.n + " lộ ra: " + VULN_T + " giây trước khi ấn tái lập");
                        PushFeed("Ấn bảo hộ trận nhãn " + E.n + " hướng " + DIRS[e.i].n + " vỡ hết");
                        Boom(e.x, e.y - 60, E.col, 30, 200); cmdT = M.Min(cmdT, 0.3f);
                    }
                }
                else if (src == hero)
                {
                    if (t - e.vainT > 0.9f) { e.vainT = t; FloatT(e.x, e.y - 178, "Vô hiệu · cần " + ELEM[e.weak].n + " khắc " + E.n, Rgb.Hex("#c9c0b0"), 14); RingFx(e.x, e.y - 50, 40, 70, 0.3f, new Rgb(200, 200, 200)); }
                }
                return;
            }
            if (e.vuln <= 0) return;
            if (src != null && src != hero && !src.champ && src.t != "quy") amt *= 0.35f;
            e.hp -= amt; e.flash = 0.1f; e.lastHit = t;
            if (src == hero || o.show) FloatT(e.x + R(-12, 12), e.y - 150, M.Fmt(amt), o.crit ? Rgb.Hex("#ffd36b") : Rgb.Hex("#f3d9a0"), o.crit ? 20 : 15);
            if (e.hp <= 0) EyeDown(e, src);
        }

        void Kill(Unit u, Unit src)
        {
            if (u.dead) return; u.dead = true; u.hp = 0;
            Splat(u.x, u.y, u.team, u.r);
            if (u == hero) { hero.respawn = 6; PushFeed("Ngao Thiên trọng thương, lui về hậu phương"); return; }
            if (u.champ)
            {
                if (u.team == 0) { u.respawn = 12; u.retreat = false; PushFeed(NameOf(u) + " trọng thương, lui về doanh trại"); }
                else
                {
                    maDown++; PushFeed(SrcName(src) + " hạ " + NameOf(u));
                    ShowBanner(NameOf(u) + " đã ngã", u.D.sect + " mất một tướng trấn giữ", 2.4f, u.D.g);
                    Boom(u.x, u.y, u.D.col, 40, 220); Shake(Dist(hero, u) < 700 ? 12 : 0); mcmdT = M.Min(mcmdT, 0.5f);
                }
                return;
            }
            if (u.team == 1)
            {
                if (u == boss) { BossDown(src); return; }
                killed++;
                if (src == hero) { heroKills++; if (u.t == "tuong" || t - lastHeroFeed > 5) { PushFeed("Ngao Thiên hạ " + u.T.name); lastHeroFeed = t; } }
            }
            else lost++;
        }

        public void RevealEye(Eye e, Unit by)
        {
            if (e.revealed) return; e.revealed = true; e.shock = 3.5f; warnT = 0; var E = ELEM[e.el]; var W = ELEM[e.weak];
            bool near = Dist(hero, e) < 1500;
            if (near) { Flash(0.3f, E.col); Shake(12); }
            Boom(e.x, e.y - 40, E.col, 40, 260);
            Glyph(E.g, e.x, e.y - 120, 1.2f, E.col);
            if (near) ShowBanner("Trận nhãn " + E.n + " hiện hình", e.maxSeals + " ấn " + E.n + " bảo hộ · " + W.n + " khắc " + E.n + ": " + e.toolT, 3.2f, E.g);
            PushFeed(SrcName(by) + " soi ra trận nhãn " + E.n + " ở " + e.place);
            int n = 5;
            for (int k = 0; k < n + 1; k++)
            {
                float a = R(0, M.TAU), rr = R(140, 270); float x = e.x + M.Cos(a) * rr, y = e.y + M.Sin(a) * rr; float rc = M.Dist(x, y, C.x, C.y);
                if (rc < BAR_R + 40) { x = C.x + (x - C.x) / rc * (BAR_R + 40); y = C.y + (y - C.y) / rc * (BAR_R + 40); }
                var u = AddUnit(k < n ? "no" : "tuong", x, y); u.role = Role.Amb; u.rise = 1; u.stun = 1; u.home = new V2(e.x, e.y); u.hasHome = true;
                RingFx(x, y, 4, 34, 0.6f, new Rgb(90, 20, 20));
            }
            totalEnemy += n + 1;
        }

        void EyeDown(Eye e, Unit src)
        {
            if (e.dead) return; e.dead = true; e.hp = 0; e.vuln = 0; warnT = 0; var E = ELEM[e.el]; int d = e.i;
            PushFeed(SrcName(src) + " phá trận nhãn " + E.n + " hướng " + DIRS[d].n);
            Shake(Dist(hero, e) < 1500 ? 18 : 6); Flash(0.35f, E.col); Boom(e.x, e.y - 40, E.col, 50, 300);
            decals.Add(new Decal { x = e.x, y = e.y, r = 52, t = 999, type = "crater" }); ripples.Add(new Ripple { a = e.va, t = 0, big = true });
            sec[d] = true; breakT = 0; breakD = d;
            if (open < 0) open = d; else ShowBanner("Cung kết giới hướng " + DIRS[d].n + " sụp", "Thêm một cửa vào thành", 2.4f, "破");
            bool all = true; foreach (var s in sec) if (!s) all = false; if (all) barrier = false;
            cmdT = 0; mcmdT = 0;
        }

        void BossDown(Unit src)
        {
            PushFeed(SrcName(src) + " chém hạ Huyết Sát Phó Tướng");
            ShowBanner("Trấn thủ đã ngã", "Quân Ma đạo rối loạn, thủ quân mất ý chí", 3, "煞");
            shake = 20; Flash(0.5f, new Rgb(255, 210, 150)); Boom(boss.x, boss.y, new Rgb(229, 96, 77), 70, 420);
            foreach (var u in units) if (!u.dead && u.team == 1 && Dist(u, boss) < 600) u.stun = M.Max(u.stun, 1.5f);
        }

        // ================= cập nhật =================
        public void Update(float dt)
        {
            t += dt;
            if (strat != null) { strat.t -= dt; if (strat.t <= 0) strat = null; }
            if (intro > 0) { intro -= dt; UpdateFx(dt); return; }
            if (over != null) overT += dt;
            else { time -= dt; if (time <= 0) { time = 0; over = "lose"; overT = 0; ShowBanner("Hết giờ", "Thủ quân Phù Vân Thành giữ được thành", 4, null); } }
            if (AUTO) Autopilot();
            hoCd -= dt; phaCd -= dt; danCd -= dt; anCd -= dt;
            if (an < 3) { anT += dt; if (anT >= 20) { anT = 0; an++; } } else anT = 0;
            if (loi < 5) { loiT += dt; if (loiT >= 25) { loiT = 0; loi++; } } else loiT = 0;
            UpdateHero(dt);
            for (int i = 0; i < units.Count; i++) { var u = units[i]; if (u == hero || u == boss) continue; if (u.champ) { UpdateChamp(u, dt); continue; } if (u.dead) continue; UpdateUnit(u, dt); }
            UpdateBoss(dt);
            cmdT -= dt; if (cmdT <= 0) { cmdT = 2.5f; CmdAtk(); }
            mcmdT -= dt; if (mcmdT <= 0) { mcmdT = 2.5f; CmdDef(); }
            UpdateEyes(dt);
            Separation(); Collide();
            UpdateProj(dt); UpdateZones(dt); UpdateFx(dt);
            PhaseLogic(dt);
            if (t % 2 < dt) units.RemoveAll(u => u.dead && !u.champ && u != boss);
        }

        void UpdateFx(float dt)
        {
            for (int fi = 0; fi < fx.Count; fi++)
            {
                var f = fx[fi]; f.t -= dt;
                if (f.type == FxT.P) { f.x += f.vx * dt; f.y += f.vy * dt; f.vx *= 0.94f; f.vy *= 0.94f; f.vy += f.grav * dt; }
                if (f.type == FxT.Fall && f.t <= 0 && !f.hit)
                {
                    f.hit = true;
                    for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 1) continue; if (M.Dist(e.x, e.y, f.x, f.y) < 44 + e.r) Damage(e, R(80, 105), hero, new HitOpt { crit = Rnd() < 0.12f }); }
                    Sparks(f.x, f.y, new Rgb(243, 217, 160), 5); RingFx(f.x, f.y, 6, 46, 0.25f, new Rgb(243, 217, 160)); Shake(3);
                }
            }
            fx.RemoveAll(f => f.t <= 0); if (fx.Count > 900) fx.RemoveRange(0, fx.Count - 900);
            foreach (var f in floats) { f.t -= dt; f.y += f.vy * dt; f.vy *= 0.95f; }
            floats.RemoveAll(f => f.t <= 0);
            foreach (var d in decals) d.t -= dt; decals.RemoveAll(d => d.t <= 0); if (decals.Count > 260) decals.RemoveRange(0, decals.Count - 260);
            foreach (var r in ripples) r.t += dt; ripples.RemoveAll(r => r.t >= 0.8f);
            if (breakT >= 0) breakT += dt;
            shake *= M.Pow(0.02f, dt); flashA = M.Max(0, flashA - dt * 1.6f);
            if (banner != null) { banner.t -= dt; if (banner.t <= 0) banner = null; }
            if (warnT > 0) warnT -= dt;
        }

        readonly Dictionary<long, List<Unit>> sgrid = new Dictionary<long, List<Unit>>();
        void Separation()
        {
            const float cell = 48; foreach (var l in sgrid.Values) l.Clear();
            foreach (var u in units)
            {
                if (u.dead) continue; long k = (long)(u.x / cell) * 8192 + (long)(u.y / cell); List<Unit> a;
                if (!sgrid.TryGetValue(k, out a)) { a = new List<Unit>(); sgrid[k] = a; }
                a.Add(u);
            }
            foreach (var u in units)
            {
                if (u.dead) continue; long cx = (long)(u.x / cell), cy = (long)(u.y / cell);
                for (int i = -1; i <= 1; i++) for (int j = -1; j <= 1; j++)
                    {
                        List<Unit> a; if (!sgrid.TryGetValue((cx + i) * 8192 + (cy + j), out a)) continue;
                        foreach (var v in a)
                        {
                            if (v.id <= u.id) continue; float dx = v.x - u.x, dy = v.y - u.y, rr = u.r + v.r; float d2 = dx * dx + dy * dy; if (d2 >= rr * rr || d2 == 0) continue;
                            float d = M.Sqrt(d2), o = rr - d; float mu = MassOf(u), mv = MassOf(v), tot = mu + mv; float nx = dx / d, ny = dy / d;
                            if (!(u == hero && hero.dashT > 0)) { u.x -= nx * o * (mv / tot); u.y -= ny * o * (mv / tot); }
                            if (!(v == hero && hero.dashT > 0)) { v.x += nx * o * (mu / tot); v.y += ny * o * (mu / tot); }
                        }
                    }
            }
        }
        static float MassOf(Unit u) { return u.t == "boss" ? 12 : u.t == "quy" ? 10 : u.champ ? 4 : u.t == "long" || u.t == "tuong" ? 2 : 1; }

        void Collide()
        {
            foreach (var u in units)
            {
                if (u.dead) continue;
                foreach (var Rg in RINGS)
                {
                    float dx = u.x - C.x, dy = u.y - C.y, r = M.Hypot(dx, dy); if (r < 1) r = 1;
                    float inner = Rg.R - Rg.T / 2 - u.r, outer = Rg.R + Rg.T / 2 + u.r;
                    if (r > inner && r < outer)
                    {
                        float a = M.Atan2(dy, dx); bool gate = false; foreach (var g in Rg.gates) if (M.AngDiff(a, g) < Rg.half - u.r / Rg.R) gate = true;
                        if (!gate) { float nr = u.pr < Rg.R ? inner : outer; u.x = C.x + dx / r * nr; u.y = C.y + dy / r * nr; }
                    }
                }
                if (barrier && u.team == 0)
                {
                    float dx = u.x - C.x, dy = u.y - C.y, r = M.Hypot(dx, dy); if (r < 1) r = 1;
                    if (M.Abs(r - BAR_R) < u.r + 2 && !sec[SecOf(M.Atan2(dy, dx))])
                    {
                        bool outS = u.pr >= BAR_R; float nr = outS ? BAR_R + u.r + 2 : BAR_R - u.r - 2; u.x = C.x + dx / r * nr; u.y = C.y + dy / r * nr;
                        if (u == hero && outS && Rnd() < 0.05f) ripples.Add(new Ripple { a = M.Atan2(dy, dx), t = 0 });
                    }
                }
                List<Building> arr; if (bgrid.TryGetValue((int)Math.Floor(u.x / BC) * 4096 + (int)Math.Floor(u.y / BC), out arr)) foreach (var b in arr) PushOBB(u, b);
                foreach (var p in eyes)
                {
                    if (p.dead || !p.revealed) continue; float d = M.Dist(u.x, u.y, p.x, p.y);
                    if (d < p.r + u.r && d > 0) { u.x = p.x + (u.x - p.x) / d * (p.r + u.r); u.y = p.y + (u.y - p.y) / d * (p.r + u.r); }
                }
                u.x = M.Clamp(u.x, 30, WW - 30); u.y = M.Clamp(u.y, 30, WH - 30);
                u.pr = M.Dist(u.x, u.y, C.x, C.y);
            }
        }

        void PhaseLogic(float dt)
        {
            if (phase == 1 && open >= 0)
            {
                phase = 2; int inside = 0; foreach (var u in units) if (!u.dead && u.team == 1 && !u.champ && u != boss && u.pr < BAR_R) inside++;
                k0 = killed; need = Math.Max(20, (int)Math.Ceiling(inside * 0.5));
                PushFeed("Kết giới hướng " + DirName(open) + " sụp đổ");
            }
            if (phase == 2 && killed - k0 >= need && boss.dead) { phase = 3; ShowBanner("Cắm cờ", "Giữ đài trung tâm " + flagNeed + " giây", 2.6f, "旗"); PushFeed("Thủ quân tan vỡ, tiến lên đài trung tâm"); }
            if (phase == 3 && over == null)
            {
                bool inZ = false; foreach (var a in allies) if (!a.dead && Dist(a, C) < FLAG_R + 6) inZ = true;
                contested = CountEnemies(C, FLAG_R + 40) > 0;
                if (inZ && !contested) { int al = CountEnemies(C, FLAG_R + 30, 0); flag += dt * (al >= 5 ? 1.25f : 1); }
                if (flag >= flagNeed)
                {
                    over = "win"; overT = 0; flagOwner = 0; Flash(0.8f, new Rgb(243, 217, 160));
                    ShowBanner("Công phá Phù Vân Thành", "Cờ Chính đạo tung bay trên đài trung tâm", 3.6f, "捷"); PushFeed("Phù Vân Thành về tay Chính đạo");
                    foreach (var u in units) if (!u.dead && u.team == 1) u.stun = 99;
                }
            }
        }
    }
}
