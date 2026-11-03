using System;
using System.Collections.Generic;
using System.Linq;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.Sim
{
    public partial class Siege
    {
        static readonly Rgb GOLD = new Rgb(243, 217, 160);

        // ================= kỹ năng Ngao Thiên =================
        Body FaceNearest(float r)
        {
            Body tt = NearestEnemy(hero, r, e => Reachable(hero, e));
            foreach (var p in LiveEyes()) if (Dist(hero, p) - p.r < r && (tt == null || Dist(hero, p) < Dist(hero, tt))) tt = p;
            if (tt != null) hero.face = Ang(hero, tt);
            return tt;
        }

        public void CastSlash()
        {
            var h = hero; if (h.dead || h.cdS > 0 || h.dashT > 0) return; h.cdS = 0.42f; h.swing = 0.2f; h.swingDir *= -1;
            FaceNearest(120);
            float Rr = 108, half = M.Deg(65);
            var tg = new List<Body>();
            foreach (var e in units) { if (e.dead || e.team != 1 || !Reachable(h, e)) continue; if (Dist(h, e) - e.r < Rr && M.AngDiff(Ang(h, e), h.face) < half) tg.Add(e); }
            foreach (var p in LiveEyes()) if (Dist(h, p) - p.r < Rr && M.AngDiff(Ang(h, p), h.face) < half + 0.3f) tg.Add(p);
            foreach (var e in tg) { bool crit = Rnd() < 0.16f; float d = R(95, 115) * (crit ? 1.8f : 1); if (e.IsEye) d *= 1.25f; Damage(e, d, h, new HitOpt { crit = crit }); Sparks(e.x, e.y - 8, GOLD, 6); }
            fx.Add(new Fx { type = FxT.Slash, x = h.x, y = h.y, a = h.face, dir = h.swingDir, t = 0.2f, max = 0.2f });
            if (tg.Count > 0) Shake(2.5f);
        }

        public bool CastYu()
        {
            var h = hero; if (h.dead || h.cdYu > 0 || h.mp < 40) return false;
            var ey = eyes.FirstOrDefault(e => e.revealed && !e.dead && Dist(h, e) < 470 && ((e.seals > 0 && e.weak == Elem.Kim) || e.vuln > 0));
            List<Body> list;
            if (ey != null) list = new List<Body> { ey, ey, ey };
            else list = units.Where(e => !e.dead && e.team == 1 && Dist(h, e) < 460 && Reachable(h, e) && (e != boss || boss.active)).OrderBy(e => Dist(h, e)).Take(3).Cast<Body>().ToList();
            if (list.Count == 0) return false;
            h.cdYu = 4; h.mp -= 40; int cast = ++castN;
            for (int i = 0; i < list.Count; i++)
            {
                float a = h.face + (i - 1) * 0.9f;
                proj.Add(new Proj { kind = PKind.Sword, x = h.x + M.Cos(a) * 20, y = h.y - 20 + M.Sin(a) * 20, vx = M.Cos(a) * 260, vy = M.Sin(a) * 260 - 120, tgt = list[i], spd = 820, dmg = R(125, 150), team = 0, life = 2.2f, delay = i * 0.07f, cast = cast });
            }
            return true;
        }

        V2? BestCluster(Unit h, float range, int minN)
        {
            Unit best = null; int bn = minN - 1;
            foreach (var e in units)
            {
                if (e.dead || e.team != 1 || Dist(h, e) > range || !Reachable(h, e)) continue;
                int n = 0; foreach (var f in units) if (!f.dead && f.team == 1 && M.Abs(e.x - f.x) < 110 && Dist(e, f) < 110) n++;
                if (n > bn) { bn = n; best = e; }
            }
            return best != null ? best.P : (V2?)null;
        }
        Eye EyeFor(Elem elem, float r) { foreach (var e in eyes) if (e.revealed && !e.dead && Dist(hero, e) < r && ((e.weak == elem && e.seals > 0) || e.vuln > 0)) return e; return null; }

        public bool CastLoi(V2? pt = null)
        {
            var h = hero; if (h.dead || h.cdLoi > 0 || loi <= 0) return false;
            if (!pt.HasValue) { var ey = EyeFor(Elem.Hoa, 430); if (ey != null) pt = ey.P; else { pt = BestCluster(h, 380, 1); if (!pt.HasValue) pt = new V2(h.x + M.Cos(h.face) * 200, h.y + M.Sin(h.face) * 200); } }
            if (barrier && Dist(pt.Value, C) < BAR_R) return false;
            h.cdLoi = 4; loi--;
            teles.Add(new Tele { x = pt.Value.x, y = pt.Value.y, r = 115, t = 0.32f, max = 0.32f, team = 0, kind = "loi", cast = ++castN });
            return true;
        }
        void LoiStrike(Tele tl)
        {
            for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 1) continue; if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) Damage(e, R(190, 230), hero, new HitOpt { stun = 1.4f, crit = Rnd() < 0.2f }); }
            foreach (var e in eyes) if (!e.dead && e.revealed && M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r + 10) Damage(e, 260, hero, new HitOpt { elem = Elem.Hoa, cast = tl.cast, show = true });
            var pts = new V2[11]; float cx = tl.x + R(-40, 40), cy = tl.y - 520;
            for (int i = 0; i <= 10; i++) { float k = i / 10f; pts[i] = new V2(cx + (tl.x - cx) * k + (i > 0 && i < 10 ? R(-26, 26) : 0), cy + (tl.y - cy) * k); }
            fx.Add(new Fx { type = FxT.Bolt, x = tl.x, y = tl.y, t = 0.35f, max = 0.35f, pts = pts });
            decals.Add(new Decal { x = tl.x, y = tl.y, r = 70, t = 14, type = "scorch" });
            Flash(0.35f, new Rgb(255, 200, 150)); Shake(9); Sparks(tl.x, tl.y, new Rgb(255, 190, 120), 22);
        }

        public bool CastAn(V2? pt = null)
        {
            var h = hero; if (h.dead || anCd > 0 || an <= 0) return false;
            if (!pt.HasValue) { var ey = EyeFor(Elem.Tho, 430); if (ey != null) pt = ey.P; else { pt = BestCluster(h, 380, 1); if (!pt.HasValue) pt = new V2(h.x + M.Cos(h.face) * 180, h.y + M.Sin(h.face) * 180); } }
            if (barrier && Dist(pt.Value, C) < BAR_R) return false;
            anCd = 3; an--;
            teles.Add(new Tele { x = pt.Value.x, y = pt.Value.y, r = 110, t = 0.6f, max = 0.6f, team = 0, kind = "an", cast = ++castN });
            return true;
        }
        void AnStrike(Tele tl)
        {
            for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 1) continue; if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) Damage(e, R(150, 185), hero, new HitOpt { stun = 1.2f }); }
            foreach (var e in eyes) if (!e.dead && e.revealed && M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r + 10) Damage(e, 320, hero, new HitOpt { elem = Elem.Tho, cast = tl.cast, show = true });
            Boom(tl.x, tl.y, new Rgb(214, 170, 106), 30, tl.r * 1.6f); decals.Add(new Decal { x = tl.x, y = tl.y, r = 85, t = 16, type = "crack", seed = (int)(Rnd() * 1000) });
            Glyph("鎮", tl.x, tl.y - 40, 0.8f, new Rgb(224, 184, 120)); Shake(13);
        }

        public bool CastVan()
        {
            var h = hero; if (h.dead || h.cdVan > 0 || h.mp < 160) return false;
            if (InZone(h, "ho") == null) { FloatT(h.x, h.y - 50, "Cần đứng trong Hộ Sơn Đại Trận", Rgb.Hex("#ffb3a8"), 13); return false; }
            h.cdVan = 28; h.mp -= 160;
            zones.Add(new Zone { type = "van", x = h.x, y = h.y, r = 270, t = 3.4f, max = 3.4f, tick = 0 });
            ShowBanner("Vạn Kiếm Quy Tông", "Muôn kiếm từ trời giáng xuống", 1.8f, "萬");
            Flash(0.3f, GOLD); Shake(8);
            return true;
        }
        public bool CastYan()
        {
            var h = hero; if (h.dead || h.cdYan > 0 || h.mp < 30) return false;
            h.cdYan = 10; h.mp -= 30; waves.Add(new Wave { x = h.x, y = h.y, r = 20, max = 720 }); veinGlow = 5;
            Glyph("眼", h.x, h.y - 70, 1, new Rgb(230, 210, 255)); Shake(4);
            return true;
        }
        public bool CastDon(V2? dir = null)
        {
            var h = hero; if (h.dead || h.cdDon > 0 || h.st < 30 || h.dashT > 0) return false;
            if (InZone(h, "khon") != null) { if (t - h.blkT > 1) { FloatT(h.x, h.y - 50, "Độn thuật bị chặn", Rgb.Hex("#ff8c84"), 14); h.blkT = t; } return false; }
            if (!dir.HasValue) { float m = M.Hypot(IN.mx, IN.my); dir = m > 0.1f ? new V2(IN.mx / m, IN.my / m) : new V2(M.Cos(h.face), M.Sin(h.face)); }
            h.cdDon = 1.8f; h.st -= 30; h.dashDir = dir.Value; h.dashT = 0.18f; h.invul = 0.32f;
            return true;
        }
        public bool PlaceHo()
        {
            if (hero.dead || hoCd > 0) return false; hoCd = 24;
            zones.Add(new Zone { type = "ho", x = hero.x, y = hero.y, r = 150, t = 16, max = 16 });
            PushFeed("Ngao Thiên bày Hộ Sơn Đại Trận");
            return true;
        }
        public bool UsePha(Body target = null, Zone zt = null)
        {
            var h = hero; if (h.dead || pha <= 0 || phaCd > 0) return false;
            if (target == null && zt == null)
            {
                var kz = InZone(h, "khon"); if (kz != null) zt = kz;
                else { Body best = null; float bd = 370; foreach (var e in LiveEyes()) { float d = Dist(h, e); if (d < bd) { bd = d; best = e; } } target = best; }
            }
            if (target == null && zt == null) return false;
            pha--; phaCd = 1.2f;
            float tx = zt != null ? zt.x : target.x, ty = zt != null ? zt.y : target.y;
            proj.Add(new Proj { kind = PKind.Pha, x = h.x, y = h.y - 24, sx = h.x, sy = h.y - 24, tgt = target, zoneTgt = zt, t = 0, max = M.Max(0.35f, M.Dist(h.x, h.y, tx, ty) / 700), team = 0 });
            return true;
        }
        public void UseDan()
        {
            if (hero.dead || dan <= 0 || danCd > 0 || hero.hp >= hero.max) return; dan--; danCd = 6; hero.hp = M.Min(hero.max, hero.hp + hero.max * 0.35f);
            FloatT(hero.x, hero.y - 50, "+" + M.Fmt(hero.max * 0.35f), Rgb.Hex("#7fd18b"), 16);
            for (int i = 0; i < 14; i++) fx.Add(new Fx { type = FxT.P, x = hero.x + R(-16, 16), y = hero.y + R(-10, 10), vx = 0, vy = R(-70, -30), t = 0.8f, max = 0.8f, col = new Rgb(127, 209, 139), s = 3 });
        }

        // ================= cập nhật anh hùng, đạn, vùng, trận nhãn =================
        void UpdateHero(float dt)
        {
            var h = hero;
            if (h.flash > 0) h.flash -= dt; if (h.swing > 0) h.swing -= dt;
            if (h.dead)
            {
                h.respawn -= dt;
                if (h.respawn <= 0) { h.dead = false; h.hp = h.max; h.mp = h.mpMax; var p = open >= 0 ? OpenPt(BAR_R + 260) : new V2(CAMP.x + 420, CAMP.y); h.x = p.x; h.y = p.y; h.pr = Dist(h, C); h.invul = 2; PushFeed("Ngao Thiên trở lại chiến trường"); }
                return;
            }
            h.mp = M.Min(h.mpMax, h.mp + 14 * dt); h.st = M.Min(100, h.st + 16 * dt); h.hp = M.Min(h.max, h.hp + 12 * dt);
            h.cdS -= dt; h.cdYu -= dt; h.cdLoi -= dt; h.cdVan -= dt; h.cdDon -= dt; h.cdYan -= dt; h.invul -= dt;
            var foe = NearestEnemy(h, 350, e => Reachable(h, e));
            if (foe == null && InZone(h, "khon") == null) h.flyT = M.Min(3, h.flyT + dt); else h.flyT = 0;
            if (h.stun > 0) { h.stun -= dt; return; }
            h.moving = false;
            if (h.dashT > 0)
            {
                float s = 1300 * dt; h.x += h.dashDir.x * s; h.y += h.dashDir.y * s; h.face = M.Atan2(h.dashDir.y, h.dashDir.x); h.dashT -= dt;
                fx.Add(new Fx { type = FxT.Ghost, x = h.x, y = h.y, a = h.face, t = 0.25f, max = 0.25f, col = new Rgb(160, 220, 255) });
                return;
            }
            float m = M.Hypot(IN.mx, IN.my);
            if (m > 0.08f && over == null)
            {
                float k = M.Min(1, m); bool fly = h.flyT > 0.8f; float s = h.T.spd * (fly ? 1.55f : 1) * SlowOf(h) * k * dt;
                h.x += IN.mx / m * s; h.y += IN.my / m * s; h.face = M.Atan2(IN.my, IN.mx); h.moving = true;
                if (fly && Math.Floor(t * 20) != Math.Floor((t - dt) * 20)) fx.Add(new Fx { type = FxT.Trail, x = h.x, y = h.y - 4, t = 0.45f, max = 0.45f });
            }
            if (IN.slash) CastSlash();
        }

        void UpdateProj(float dt)
        {
            for (int i = 0; i < proj.Count; i++)
            {
                var p = proj[i]; if (p.dead) continue;
                if (p.kind == PKind.Arrow)
                {
                    float a = M.Atan2(p.ty - p.y, p.tx - p.x), d = M.Dist(p.tx, p.ty, p.x, p.y), s = p.spd * dt; p.a = a;
                    if (d <= s) { p.dead = true; if (p.tgt != null && !p.tgt.dead && M.Dist(p.tgt.x, p.tgt.y - 6, p.tx, p.ty) < p.tgt.r + 12) Damage(p.tgt, p.dmg, p.src); }
                    else { p.x += M.Cos(a) * s; p.y += M.Sin(a) * s; }
                    continue;
                }
                if (p.kind == PKind.Bolt)
                {
                    var tg = p.tgt; if (tg == null || tg.dead) { p.dead = true; continue; }
                    float tx = tg.x, ty = tg.y - (tg.IsEye ? 40 : 8);
                    if (p.team == 0 && p.ob && barrier && !tg.IsEye && M.Dist(tg.x, tg.y, C.x, C.y) < BAR_R && M.Dist(p.x, p.y, C.x, C.y) < BAR_R + 4 && !SecOpenA(AngC(p.x, p.y))) { p.dead = true; ripples.Add(new Ripple { a = AngC(p.x, p.y), t = 0 }); continue; }
                    float a = M.Atan2(ty - p.y, tx - p.x), d = M.Dist(tx, ty, p.x, p.y), s = p.spd * dt; p.trail.Add(new V2(p.x, p.y)); if (p.trail.Count > 6) p.trail.RemoveAt(0);
                    if (d <= s + tg.r * 0.5f) { p.dead = true; Damage(tg, p.dmg, p.src); Sparks(tx, ty, p.hasCol ? p.col : new Rgb(240, 220, 140), p.big ? 7 : 4); }
                    else { p.x += M.Cos(a) * s; p.y += M.Sin(a) * s; }
                    continue;
                }
                if (p.kind == PKind.Sword)
                {
                    if (p.delay > 0) { p.delay -= dt; p.x = hero.x; p.y = hero.y - 20; continue; }
                    var tg = p.tgt; p.life -= dt; p.trail.Add(new V2(p.x, p.y)); if (p.trail.Count > 10) p.trail.RemoveAt(0);
                    if (tg == null || tg.dead || p.life <= 0) { p.vx *= 0.9f; p.vy *= 0.9f; p.fade -= dt; p.x += p.vx * dt; p.y += p.vy * dt; if (p.fade <= 0) p.dead = true; continue; }
                    float tx = tg.x, ty = tg.y - (tg.IsEye ? 55 : 10); float a = M.Atan2(ty - p.y, tx - p.x);
                    float wx = M.Cos(a) * p.spd, wy = M.Sin(a) * p.spd; float k = M.Min(1, 7 * dt); p.vx += (wx - p.vx) * k; p.vy += (wy - p.vy) * k; p.x += p.vx * dt; p.y += p.vy * dt;
                    if (M.Dist(tx, ty, p.x, p.y) < tg.r + 10) { p.dead = true; Damage(tg, p.dmg, hero, new HitOpt { crit = Rnd() < 0.2f, elem = Elem.Kim, cast = p.cast }); Sparks(tx, ty, new Rgb(160, 230, 255), 10); }
                    continue;
                }
                if (p.kind == PKind.Pha)
                {
                    p.t += dt; float k = M.Min(1, p.t / p.max);
                    float tx = p.zoneTgt != null ? p.zoneTgt.x : p.tgt.x, ty = p.zoneTgt != null ? p.zoneTgt.y : (p.tgt.IsEye ? p.tgt.y - 50 : p.tgt.y);
                    p.x = p.sx + (tx - p.sx) * k; p.y = p.sy + (ty - p.sy) * k - M.Sin(k * M.PI) * 120;
                    if (k >= 1)
                    {
                        p.dead = true;
                        if (p.zoneTgt != null) { p.zoneTgt.t = 0; PushFeed("Phá Trận Phù phá vỡ Khốn Long Trận"); }
                        else if (p.tgt is Eye) { var e = (Eye)p.tgt; if (e.seals > 0) Damage(e, 0, hero, new HitOpt { elem = Elem.Pha, cast = ++castN }); else Damage(e, 900, hero, new HitOpt { show = true, crit = true }); }
                        Glyph("破", tx, ty - 30, 0.9f, new Rgb(255, 215, 120)); Boom(tx, ty, new Rgb(255, 200, 110), 26, 200); Shake(10);
                    }
                }
            }
            proj.RemoveAll(p => p.dead);
        }

        void ElemStrike(Tele tl)
        {
            var src = tl.src ?? hero;
            for (int i = 0; i < units.Count; i++)
            {
                var e = units[i]; if (e.dead || e.team == tl.team) continue; if (e == boss && !boss.active) continue;
                if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) { bool big = e.champ || e == boss; Damage(e, big ? tl.dmg * 1.5f : tl.dmg * R(0.9f, 1.1f), src, new HitOpt { stun = tl.kind == "vine" ? 1.6f : tl.kind == "ice" ? 0.8f : tl.kind == "an" ? 1.2f : 0, show = big }); }
            }
            foreach (var e in eyes) if (!e.dead && e.revealed && M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r + 10) Damage(e, 260, src, new HitOpt { elem = tl.elem, cast = tl.cast, show = true });
            bool near = M.Dist(hero.x, hero.y, tl.x, tl.y) < 700;
            if (tl.kind == "an") { Boom(tl.x, tl.y, new Rgb(214, 170, 106), 30, tl.r * 1.6f); decals.Add(new Decal { x = tl.x, y = tl.y, r = 85, t = 16, type = "crack" }); Glyph("鎮", tl.x, tl.y - 40, 0.8f, new Rgb(224, 184, 120)); if (near) Shake(11); }
            else if (tl.kind == "fire") { Boom(tl.x, tl.y, new Rgb(255, 138, 74), 30, tl.r * 1.5f); decals.Add(new Decal { x = tl.x, y = tl.y, r = 75, t = 14, type = "scorch" }); for (int i = 0; i < 18; i++) fx.Add(new Fx { type = FxT.P, x = tl.x + R(-45, 45), y = tl.y + R(-22, 22), vx = R(-20, 20), vy = R(-150, -60), t = R(0.5f, 0.9f), max = 0.9f, col = new Rgb(255, 150, 60), s = R(2, 4) }); Glyph("炎", tl.x, tl.y - 40, 0.7f, new Rgb(255, 150, 80)); if (near) Shake(8); }
            else if (tl.kind == "ice") { Boom(tl.x, tl.y, new Rgb(150, 210, 255), 24, tl.r * 1.4f); fx.Add(new Fx { type = FxT.Ice, x = tl.x, y = tl.y, t = 0.8f, max = 0.8f, r = tl.r }); Glyph("冰", tl.x, tl.y - 40, 0.7f, new Rgb(170, 220, 255)); if (near) Shake(6); }
            else if (tl.kind == "vine") { fx.Add(new Fx { type = FxT.Vine, x = tl.x, y = tl.y, t = 1.5f, max = 1.5f, r = tl.r }); Sparks(tl.x, tl.y, new Rgb(127, 209, 139), 16); Glyph("藤", tl.x, tl.y - 40, 0.7f, new Rgb(127, 209, 139)); }
        }
        void MaStrike(Tele tl)
        {
            for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 0) continue; if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) Damage(e, e.champ ? tl.dmg : R(45, 65), tl.src, new HitOpt { stun = tl.stun }); }
            Boom(tl.x, tl.y, tl.col, 26, tl.r * 1.5f); decals.Add(new Decal { x = tl.x, y = tl.y, r = tl.r * 0.7f, t = 10, type = "blood" }); Glyph(tl.g, tl.x, tl.y - 40, 0.8f, tl.col);
            Shake(M.Dist(hero.x, hero.y, tl.x, tl.y) < 600 ? 8 : 0);
        }

        void UpdateZones(float dt)
        {
            foreach (var z in zones)
            {
                z.t -= dt;
                if (z.type == "van")
                {
                    z.x = hero.x; z.y = hero.y; z.tick -= dt;
                    while (z.tick <= 0 && z.t > 0.4f)
                    {
                        z.tick += 0.055f; float tx, ty;
                        var es = units.Where(e => !e.dead && e.team == 1 && M.Dist(e.x, e.y, z.x, z.y) < z.r).ToList();
                        if (es.Count > 0 && Rnd() < 0.7f) { var e = es[(int)(Rnd() * es.Count) % es.Count]; tx = e.x + R(-14, 14); ty = e.y + R(-14, 14); }
                        else { float a = R(0, M.TAU), r = M.Sqrt(Rnd()) * z.r; tx = z.x + M.Cos(a) * r; ty = z.y + M.Sin(a) * r; }
                        fx.Add(new Fx { type = FxT.Fall, x = tx, y = ty, t = 0.34f, max = 0.34f });
                    }
                }
                if (z.type == "khon") foreach (var a in allies) if (!a.dead && M.Dist(a.x, a.y, z.x, z.y) < z.r) { a.hp -= 26 * dt; if (a.hp <= 0) Kill(a, boss); }
            }
            zones.RemoveAll(z => z.t <= 0);
            var tl0 = teles.ToArray();
            foreach (var tl in tl0)
            {
                tl.t -= dt; if (tl.t > 0) continue; tl.done = true;
                switch (tl.kind)
                {
                    case "loi": LoiStrike(tl); break;
                    case "an": if (tl.src != null && tl.src != hero) ElemStrike(tl); else AnStrike(tl); break;
                    case "fire": case "ice": case "vine": ElemStrike(tl); break;
                    case "mtele": MaStrike(tl); break;
                    case "shock":
                        for (int i = 0; i < units.Count; i++)
                        {
                            var e = units[i]; if (e.dead || e.team != 0) continue;
                            if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) { Damage(e, e == hero ? 200 : e.champ ? 120 : R(18, 26), null); float a = M.Atan2(e.y - tl.y, e.x - tl.x), push = e.t == "quy" ? 18 : 70; e.x += M.Cos(a) * push; e.y += M.Sin(a) * push; if (e != hero) e.stun = M.Max(e.stun, 0.4f); }
                        }
                        RingFx(tl.x, tl.y, 20, tl.r + 25, 0.45f, tl.col); Sparks(tl.x, tl.y - 70, tl.col, 16); Shake(M.Dist(hero.x, hero.y, tl.x, tl.y) < 700 ? 6 : 0);
                        break;
                    case "tram":
                        for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 0) continue; if (M.Dist(e.x, e.y, tl.x, tl.y) < tl.r + e.r) Damage(e, e == hero ? tl.dmg : e.champ ? tl.dmg * 0.75f : tl.dmg * 0.12f, boss); }
                        Boom(tl.x, tl.y, new Rgb(229, 96, 77), 30, tl.r * 1.6f); Shake(M.Dist(hero.x, hero.y, tl.x, tl.y) < 600 ? 10 : 0); decals.Add(new Decal { x = tl.x, y = tl.y, r = tl.r * 0.8f, t = 10, type = "blood" });
                        break;
                }
            }
            teles.RemoveAll(x => x.done);
        }

        void UpdateEyes(float dt)
        {
            foreach (var w in waves)
            {
                w.r += 900 * dt;
                foreach (var e in eyes) if (!e.revealed && !e.dead && M.Dist(w.x, w.y, e.x, e.y) <= w.r) { RevealEye(e, w.by ?? hero); w.found = true; }
                if (w.r >= w.max && !w.found && w.by == null && !AUTO && eyes.Any(e => !e.dead && !e.revealed)) FloatT(hero.x, hero.y - 60, "Không có trận nhãn ẩn trong tầm soi", Rgb.Hex("#c9c0b0"), 13);
            }
            waves.RemoveAll(w => w.r >= w.max);
            if (veinGlow > 0) veinGlow -= dt;
            foreach (var e in eyes)
            {
                if (e.flash > 0) e.flash -= dt; if (e.dead) continue;
                if (!e.revealed)
                {
                    if (!e.sensed && !hero.dead && Dist(hero, e) < 500) { e.sensed = true; PushFeed("Linh khí dao động mạnh quanh " + e.place); Warn("Linh khí dao động: dùng Phá Vọng Nhãn để soi trận nhãn ẩn"); }
                    continue;
                }
                if (e.vuln > 0)
                {
                    e.vuln -= dt;
                    if (e.vuln <= 0) { e.vuln = 0; e.reforms++; e.seals = Math.Max(1, e.maxSeals - e.reforms); e.lastCast = -1; if (Dist(hero, e) < 1500) Warn("Ấn trận nhãn " + e.name + " tái lập: " + e.seals + " ấn"); PushFeed("Trận nhãn " + e.name + " hướng " + DIRS[e.i].n + " tái lập ấn"); Boom(e.x, e.y - 50, ELEM[e.el].col, 20, 150); }
                }
                else if (e.hp < e.max) e.hp = M.Min(e.max, e.hp + 45 * dt);
                e.shock -= dt;
                if (e.shock <= 0 && CountEnemies(e.P, 175, 0) > 0) { e.shock = 6.5f; teles.Add(new Tele { x = e.x, y = e.y, r = 165, t = 0.9f, max = 0.9f, team = 1, kind = "shock", col = ELEM[e.el].col }); }
            }
        }

        // ================= tự đánh (chế độ xem và trợ giúp) =================
        void Autopilot()
        {
            var h = hero; IN.mx = 0; IN.my = 0; IN.slash = false; if (h.dead || intro > 0) return;
            foreach (var tl in teles)
                if (tl.team == 1 && M.Dist(h.x, h.y, tl.x, tl.y) < tl.r + h.r + 8)
                {
                    float a = M.Atan2(h.y - tl.y, h.x - tl.x); var dir = new V2(M.Cos(a), M.Sin(a));
                    if (tl.t < 0.8f && CastDon(dir)) return; IN.mx = dir.x; IN.my = dir.y; return;
                }
            var kz = InZone(h, "khon");
            if (kz != null) { if (pha > 0 && phaCd <= 0 && kz.t < kz.max - 0.6f) UsePha(null, kz); }
            if (h.hp < h.max * 0.42f) UseDan();
            V2? goal = null; float goalR = 0; float stand = 62;
            var near = NearestEnemy(h, 140, e => Reachable(h, e));
            var ech = NearestEnemy(h, 270, e => e.champ && Reachable(h, e));
            int resL = !eyes[0].dead && phase == 1 ? 2 : 0, resA = !eyes[1].dead && phase == 1 ? 2 : 0;
            void SetGoal(Body b) { goal = b.P; goalR = b.r; }
            if (phase == 1)
            {
                var e = h.asg >= 0 ? eyes[h.asg] : null;
                if (e != null && !e.dead && WP[h.asg].HasValue && Dist(h, CAMP) < 1000) { goal = WP[h.asg].Value; stand = 10; }
                else if (e != null && !e.dead)
                {
                    float d = Dist(h, e);
                    if (!e.revealed)
                    {
                        if (d < 560 && h.cdYan <= 0 && h.mp >= 30) CastYan();
                        if (ech != null && d < 900) SetGoal(ech); else if (near != null && d < 800) SetGoal(near); else { SetGoal(e); stand = d < 560 && waves.Count > 0 ? 9999 : 240; }
                    }
                    else if (e.seals > 0)
                    {
                        if (e.weak == Elem.Hoa && d < 430) CastLoi(e.P);
                        else if (e.weak == Elem.Kim && d < 450) CastYu();
                        else if (e.weak == Elem.Tho && d < 430) CastAn(e.P);
                        bool pend = teles.Any(x => x.team == 0 && (x.kind == "an" || x.kind == "loi" || x.kind == "ice" || x.kind == "fire")) || proj.Any(p => p.kind == PKind.Sword && p.tgt == e);
                        bool aiCtr = allies.Any(a => a != h && !a.dead && CH_DEF[a.key].el == e.weak && Dist(a, e) < 700);
                        bool noCounter = !pend && !aiCtr && ((e.weak == Elem.Hoa && loi <= 0) || (e.weak == Elem.Tho && an <= 0) || e.weak == Elem.Thuy);
                        if (phaCd <= 0 && d < 350 && noCounter && pha > 0) UsePha(e);
                        if (ech != null) SetGoal(ech); else { var n3 = NearestEnemy(h, 320, q => Reachable(h, q)); if (n3 != null) SetGoal(n3); else { SetGoal(e); stand = 170; } }
                    }
                    else
                    {
                        SetGoal(e); stand = e.r + 46; CastYu();
                        if (d < 350 && pha > 1 && phaCd <= 0 && e.vuln < 3.5f && e.hp > e.max * 0.35f) UsePha(e);
                        if (d < 430 && loi > (h.asg == 0 ? 0 : 1)) CastLoi(e.P);
                        if (d < 430 && an > (h.asg == 1 ? 0 : 1)) CastAn(e.P);
                    }
                }
                else { if (near != null) SetGoal(near); else goal = OpenPt(BAR_R + 120); }
            }
            else if (phase == 2)
            {
                bool outside = M.Dist(h.x, h.y, C.x, C.y) > WALL_R - 40;
                if (boss.active && !boss.dead) SetGoal(near != null && Dist(h, boss) > 420 ? near : boss);
                else if (ech != null && !outside) SetGoal(ech);
                else if (outside) { if (near != null && Dist(near, h) < 120) SetGoal(near); else goal = OpenPt(WALL_R - 220); stand = 30; }
                else if (!boss.dead)
                {
                    var n3 = NearestEnemy(h, 300); int cn = allies.Count(a => a != h && !a.dead && Dist(a, h) < 750);
                    if (n3 != null) SetGoal(n3); else if (cn < 2 && !boss.active) { goal = null; h.face = Ang(h, boss); } else SetGoal(boss);
                }
                else { var nn = near ?? NearestEnemy(h, 1500); if (nn != null) SetGoal(nn); else goal = C; }
                if (!boss.dead && boss.active && Dist(h, boss) < 330 && hoCd <= 0 && InZone(h, "ho") == null) PlaceHo();
            }
            else
            {
                if (near != null && Dist(near, C) < FLAG_R + 150) SetGoal(near); else if (Dist(h, C) > FLAG_R * 0.45f) { goal = C; stand = 8; } else if (near != null) SetGoal(near);
            }
            float reserve = (phase == 2 && !boss.dead && h.cdVan < 6) ? 160 : 0;
            if (InZone(h, "ho") != null && h.cdVan <= 0 && h.mp >= 160) { int n = CountEnemies(h.P, 260); if (n >= 5 || (boss.active && !boss.dead && Dist(h, boss) < 240)) CastVan(); }
            if (h.cdYu <= 0 && h.mp >= 40 + reserve + (phase == 1 ? 30 : 0)) CastYu();
            if (h.cdLoi <= 0 && loi > resL) { var pt = BestCluster(h, 380, phase == 1 ? 4 : 5); if (pt.HasValue) CastLoi(pt); }
            if (anCd <= 0 && an > resA) { var pt = BestCluster(h, 380, 5); if (pt.HasValue) CastAn(pt); }
            if (kz != null) { float a = M.Atan2(h.y - kz.y, h.x - kz.x); if (M.Dist(h.x, h.y, kz.x, kz.y) < kz.r + 20) { IN.mx = M.Cos(a); IN.my = M.Sin(a); } }
            else if (goal.HasValue) { float d = Dist(h, goal.Value) - goalR; if (d > stand) { var wp = NavPoint(h, goal.Value); float a = Ang(h, wp); IN.mx = M.Cos(a); IN.my = M.Sin(a); } }
            var any = NearestEnemy(h, 105, e => Reachable(h, e)); var ve = eyes.FirstOrDefault(q => q.revealed && !q.dead && q.vuln > 0 && Dist(h, q) - q.r < 100);
            if (any != null || ve != null) CastSlash();
        }
    }
}
