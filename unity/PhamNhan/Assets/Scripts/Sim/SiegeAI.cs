using System;
using System.Collections.Generic;
using System.Linq;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.Sim
{
    public partial class Siege
    {
        public void SetMode(string m, bool silent = false)
        {
            mode = m;
            if (!silent) { PushFeed(m == "auto" ? "Thế trận: để quân sư tự quyết" : m == "split" ? "Thế trận: phân binh ba hướng" : "Thế trận: năm tướng hợp binh theo Ngao Thiên"); cmdT = 0; }
        }

        // ================= quân lính =================
        Unit LeaderOf(Unit u)
        {
            var L = u.lead; if (L != null && !L.dead) return L; if (L == null) return null;
            if (u.tmpT < 0 || t > u.tmpT)
            {
                u.tmpT = t + 2; Unit best = null; float bd = 1e9f;
                foreach (var c in (u.team == 1 ? foes : allies)) { if (c == null || c.dead || c == boss) continue; float d = Dist(u, c); if (d < bd) { bd = d; best = c; } }
                u.tmpL = best;
            }
            return u.tmpL != null && !u.tmpL.dead ? u.tmpL : null;
        }

        Body ChooseTarget(Unit u)
        {
            var T = u.T;
            if (u.team == 0)
            {
                var L = LeaderOf(u); bool far = L != null && phase < 3 && Dist(u, L) > 620;
                if (u.t == "loan" && L == hero && !far) return NearestEnemy(u, 280, e => Reachable(u, e) && Dist(e, hero) < 500 && e != boss);
                if (u.t == "quy") { var ey0 = VulnEye(u, 900); if (ey0 != null) return ey0; return NearestEnemy(u, 280, e => Reachable(u, e) && e != boss); }
                float ag = far ? 46 : (T.kind == Kind.Caster ? 320 : 250);
                var e1 = NearestEnemy(u, ag, e => e != boss && Reachable(u, e)); if (e1 != null) return e1;
                if (far) return null;
                var ey = VulnEye(u, 650); if (ey != null) return ey;
                if (L != null) { var f = NearestEnemy(L, 380, e => e != boss && Reachable(u, e)); if (f != null && Dist(u, f) < 720) return f; }
                if (phase == 2 && L == null) return NearestEnemy(u, 700, e => e != boss);
                if (phase == 3) return NearestEnemy(u, 420);
                return null;
            }
            float arch = T.range + 15;
            switch (u.role)
            {
                case Role.Ret:
                    {
                        var L = LeaderOf(u); if (L != null && Dist(u, L) > 600) return NearestEnemy(u, 110);
                        var e = NearestEnemy(u, T.kind == Kind.Archer ? arch : 260); if (e != null) return e; if (L != null) return NearestEnemy(L, 360); return NearestEnemy(u, 300);
                    }
                case Role.Guard: return NearestEnemy(u, T.kind == Kind.Archer ? arch : 380, e => Dist(e, u.home) < 650);
                case Role.Van: return NearestEnemy(u, T.kind == Kind.Archer ? arch : 300, e => Dist(e, u.home) < 560);
                case Role.Amb: return NearestEnemy(u, 560, e => Dist(e, u.home) < 850);
            }
            if (open < 0) { if (T.kind == Kind.Archer) return NearestEnemy(u, arch); return NearestEnemy(u, 200, e => Dist(e, u.home) < 420); }
            if (T.kind == Kind.Archer) return NearestEnemy(u, phase == 3 ? 600 : 430);
            return NearestEnemy(u, phase == 3 ? 1200 : (u.role == Role.Pat ? 560 : 650));
        }

        V2 Objective(Unit u)
        {
            if (u.team == 0)
            {
                if (phase == 3 || over != null) return new V2(C.x + u.ox * 1.3f, C.y + u.oy * 1.3f);
                var L = LeaderOf(u);
                if (L != null) { float a = L.face, back = u.t == "loan" ? 40 : 90; return new V2(L.x - M.Cos(a) * back + u.ox * 1.15f, L.y - M.Sin(a) * back + u.oy * 1.15f); }
                return u.P;
            }
            if (phase == 3) return C;
            if (u.role == Role.Ret) { var L = LeaderOf(u); if (L != null) return new V2(L.x + u.ox * 0.9f, L.y + u.oy * 0.9f); }
            if (u.role == Role.Pat && open >= 0) return OpenPt(RING_R);
            return u.home;
        }

        void UpdateUnit(Unit u, float dt)
        {
            var T = u.T;
            if (u.anim > 0) u.anim -= dt; if (u.flash > 0) u.flash -= dt; if (u.rise > 0) u.rise -= dt;
            if (u.stun > 0) { u.stun -= dt; return; }
            u.cd -= dt; u.retgt -= dt; u.moving = false;
            if (over != null) { if (u.team == 1) u.stun = 9; return; }
            if (u.retgt <= 0) { u.retgt = 0.25f + Rnd() * 0.15f; u.tgt = ChooseTarget(u); }
            var tg = u.tgt; if (tg != null && (tg.dead || (tg is Eye && ((Eye)tg).vuln <= 0))) { tg = u.tgt = null; u.retgt = 0; }
            V2? dest = null;
            if (tg != null)
            {
                float d = Dist(u, tg) - tg.r - u.r;
                if (d <= T.range) { u.face = Ang(u, tg); if (u.cd <= 0) { Attack(u, tg); u.cd = T.cd * R(0.9f, 1.1f); } }
                else dest = tg.P;
            }
            else dest = Objective(u);
            float spd = T.spd * SlowOf(u);
            if (tg == null && dest.HasValue && Dist(u, dest.Value) > 160) spd *= (u.team == 0 || u.role == Role.Ret) ? 2.4f : 1.3f;
            if (u.t == "loan" && u.lead == hero && !hero.dead && Dist(u, hero) > 260) spd = T.spd * 2.1f;
            if (dest.HasValue && Dist(u, dest.Value) > 6) MoveTo(u, dest.Value, spd, dt);
        }

        void Attack(Unit u, Body tg)
        {
            var T = u.T; u.anim = 0.25f;
            if (T.kind == Kind.Archer) { proj.Add(new Proj { kind = PKind.Arrow, x = u.x, y = u.y - 12, tx = tg.x + R(-8, 8), ty = tg.y - 6 + R(-6, 6), tgt = tg, spd = 560, dmg = T.atk * R(0.85f, 1.15f), team = 1, src = u, life = 1.2f }); return; }
            if (T.kind == Kind.Caster) { proj.Add(new Proj { kind = PKind.Bolt, x = u.x, y = u.y - 12, tgt = tg, spd = 430, dmg = T.atk * R(0.9f, 1.2f), team = 0, src = u, life = 1.5f, ob = u.pr >= BAR_R }); return; }
            float dmg = T.atk * R(0.85f, 1.15f);
            if (u.t == "quy") { if (tg.IsEye) { Shake(Dist(hero, u) < 500 ? 5 : 0); Sparks(tg.x, tg.y - 20, new Rgb(200, 170, 255), 12); } else dmg *= 0.25f; }
            Damage(tg, dmg, u);
            if (Rnd() < 0.35f) Sparks((u.x + tg.x) / 2, (u.y + tg.y) / 2 - 8, u.team == 1 ? new Rgb(255, 140, 120) : new Rgb(200, 230, 255), 3);
        }

        // ================= tướng =================
        Eye AsgEye(Unit u) { return u.asg >= 0 ? eyes[u.asg] : null; }
        V2? BestClusterU(Unit u, float range, int minN)
        {
            Unit best = null; int bn = minN - 1;
            foreach (var e in units)
            {
                if (e.dead || e.team == u.team || e == boss || Dist(u, e) > range || !Reachable(u, e)) continue;
                int n = 0; foreach (var f in units) if (!f.dead && f.team == e.team && M.Abs(e.x - f.x) < 110 && Dist(e, f) < 110) n++;
                if (n > bn) { bn = n; best = e; }
            }
            return best != null ? best.P : (V2?)null;
        }

        struct Goal { public V2 p; public float stand; public Goal(V2 p, float s) { this.p = p; stand = s; } }

        Goal? ChampGoal(Unit u)
        {
            if (u.team == 0)
            {
                if (phase == 3 || over != null) return new Goal(new V2(C.x + (u.id % 3 - 1) * 60, C.y + 60), 40);
                if (phase == 2)
                {
                    float r = M.Dist(u.x, u.y, C.x, C.y);
                    if (r > WALL_R - 60) return new Goal(OpenPt(WALL_R - 200), 60);
                    if (!boss.dead) return new Goal(boss.P, boss.active ? 140 : 430);
                    return new Goal(C, 200);
                }
                var ey = AsgEye(u); if (ey == null) return null;
                if (ey.dead) return new Goal(OpenPt(BAR_R + 120), 60);
                if (WP[u.asg].HasValue && Dist(u, CAMP) < 1000) return new Goal(WP[u.asg].Value, 10);
                if (!ey.revealed) return new Goal(new V2(ey.hx, ey.hy), 150);
                return new Goal(ey.P, ey.vuln > 0 ? 60 : (u.D.el == ey.weak ? 250 : 200));
            }
            if (u.asg == Unit.BREACH && open >= 0)
            {
                bool deep = allies.Any(a => !a.dead && M.Dist(a.x, a.y, C.x, C.y) < WALL_R - 300);
                return new Goal(deep ? Polar(DIRS[open].a, IN_R + 150) : OpenPt(WALL_R - 180), 70);
            }
            if (u.asg >= 0) return new Goal(PostPt(u.asg), 60);
            return new Goal(new V2(C.x, C.y + 150), 80);
        }

        Body ChampTarget(Unit u)
        {
            var T = u.T;
            if (u.team == 0)
            {
                var ec = NearestEnemy(u, 340, e => e.champ && Reachable(u, e)); if (ec != null) return ec;
                if (phase >= 2) { if (boss.active && !boss.dead && Dist(u, boss) < 520) return boss; return NearestEnemy(u, 330, e => e != boss && Reachable(u, e)); }
                var ey = AsgEye(u);
                if (ey != null && !ey.dead && ey.revealed && ey.vuln > 0 && Dist(u, ey) < 520) return ey;
                bool near = ey != null && Dist(u, ey) < 900;
                return NearestEnemy(u, near ? 300 : 190, e => e != boss && Reachable(u, e));
            }
            var g = ChampGoal(u); var gp = g.HasValue ? g.Value.p : u.P;
            var ec2 = NearestEnemy(u, 380, e => e.champ && Dist(e, gp) < 820); if (ec2 != null) return ec2;
            return NearestEnemy(u, T.range > 100 ? T.range + 20 : 300, e => Dist(e, gp) < 720);
        }

        void HealPulse(Unit u, float Rr, float kc, float kt, Rgb col, string glyph)
        {
            foreach (var a in units)
            {
                if (a.dead || a.team != u.team || Dist(a, u) > Rr) continue; float k = a.champ || a == boss ? kc : kt; if (a.hp >= a.max) continue;
                float add = a.max * k; a.hp = M.Min(a.max, a.hp + add); if (a.champ && add > 100) FloatT(a.x, a.y - 46, "+" + M.Fmt(add), u.team == 1 ? Rgb.Hex("#ffd07a") : Rgb.Hex("#7fd18b"), 13);
            }
            RingFx(u.x, u.y, 20, Rr, 0.7f, col); Glyph(glyph, u.x, u.y - 70, 0.9f, col);
            for (int i = 0; i < 18; i++) { float a = R(0, M.TAU), r = R(20, Rr * 0.8f); fx.Add(new Fx { type = FxT.P, x = u.x + M.Cos(a) * r, y = u.y + M.Sin(a) * r * 0.6f, vx = 0, vy = R(-60, -25), t = 0.9f, max = 0.9f, col = col, s = 3 }); }
        }

        void ChampSkill(Unit u, float dt)
        {
            var D = u.D;
            if (u.team == 0)
            {
                var ey = AsgEye(u);
                if (phase == 1 && ey != null && !ey.dead && !ey.revealed && u.yanT <= 0 && Dist(u, ey) < 520) { u.yanT = 10; waves.Add(new Wave { x = u.x, y = u.y, r = 20, max = 620, by = u }); Glyph("眼", u.x, u.y - 70, 1, new Rgb(230, 210, 255)); }
                if (u.key == "thanh" && u.healT <= 0)
                {
                    int needH = 0; foreach (var a in allies) if (!a.dead && Dist(a, u) < 300 && a.hp < a.max * 0.7f) needH += 3;
                    foreach (var tt in units) if (!tt.dead && tt.team == 0 && !tt.champ && tt.hp < tt.max * 0.6f && Dist(tt, u) < 260) needH++;
                    if (needH >= 4) { u.healT = 9; HealPulse(u, 300, 0.22f, 0.5f, new Rgb(127, 209, 139), "春"); }
                }
                if (u.skT > 0) return;
                V2? pt = null; Eye eyeT = null;
                foreach (var e in eyes) { if (e.dead || !e.revealed || Dist(u, e) > 430) continue; if ((e.seals > 0 && e.weak == D.el) || e.vuln > 0) { eyeT = e; break; } }
                if (eyeT != null) pt = eyeT.P;
                else { var ec = NearestEnemy(u, 380, e => e.champ && Reachable(u, e)); pt = ec != null ? ec.P : BestClusterU(u, 380, 4); }
                if (!pt.HasValue || BarBlock(u, pt.Value)) return;
                u.skT = D.cd * R(0.92f, 1.08f); u.anim = 0.35f; u.face = Ang(u, pt.Value);
                bool anK = D.tk == "an";
                teles.Add(new Tele { x = pt.Value.x, y = pt.Value.y, r = anK ? 115 : 110, t = anK ? 0.6f : 0.5f, max = anK ? 0.6f : 0.5f, team = 0, kind = D.tk, elem = D.el, src = u, cast = ++castN, dmg = 190 });
                if (eyeT != null && eyeT.seals > 0) FloatT(u.x, u.y - 58, D.sk, ELEM[D.el].col, 13);
                return;
            }
            if (u.key == "thi")
            {
                u.sumT -= dt;
                if (u.sumT <= 0 && NearestEnemy(u, 520) != null)
                {
                    u.sumT = 15; int n = 0; foreach (var tt in units) if (!tt.dead && tt.lead == u) n++;
                    if (n < 12)
                    {
                        for (int i = 0; i < 4; i++) { float a = R(0, M.TAU); var s = AddUnit("no", u.x + M.Cos(a) * 60, u.y + M.Sin(a) * 60); s.role = Role.Ret; s.lead = u; s.rise = 1; s.stun = 1; s.home = s.P; s.hasHome = true; RingFx(s.x, s.y, 4, 34, 0.6f, new Rgb(90, 200, 120)); }
                        totalEnemy += 4; FloatT(u.x, u.y - 58, "Triệu Thi", Rgb.Hex("#8fe0a0"), 14);
                    }
                }
            }
            if (u.key == "bo")
            {
                u.healT -= dt;
                if (u.healT <= 0 && NearestEnemy(u, 600) != null)
                {
                    u.healT = 12; HealPulse(u, 280, 0.15f, 0.4f, new Rgb(232, 200, 112), "補");
                    foreach (var e in eyes) if (!e.dead && e.revealed && Dist(e, u) < 420 && e.hp < e.max) { e.hp = M.Min(e.max, e.hp + e.max * 0.18f); FloatT(e.x, e.y - 160, "Bổ Thiên Trận vá trận nhãn", Rgb.Hex("#ffd07a"), 14); }
                }
            }
            if (u.skT > 0) return;
            var tg = NearestEnemy(u, 360, e => e.champ); V2? p2 = tg != null ? tg.P : BestClusterU(u, 340, 4);
            if (!p2.HasValue) return;
            var pt2 = p2.Value;
            u.skT = (u.key == "ta" ? 6.5f : 7.5f) * R(0.92f, 1.08f); u.anim = 0.4f; u.face = Ang(u, pt2);
            if (u.key == "lang" && tg != null && Dist(u, tg) > 110 && RegOf(u.pr) == RegOf(tg.pr))
            {
                float a = Ang(tg, u);
                for (int i = 0; i < 5; i++) fx.Add(new Fx { type = FxT.Ghost, x = u.x + (tg.x - u.x) * i / 5, y = u.y + (tg.y - u.y) * i / 5, a = u.face, t = 0.3f, max = 0.3f, col = new Rgb(230, 90, 70) });
                u.x = tg.x + M.Cos(a) * 55; u.y = tg.y + M.Sin(a) * 55;
            }
            teles.Add(new Tele { x = pt2.x, y = pt2.y, r = u.key == "ta" ? 140 : 120, t = u.key == "lang" ? 0.55f : 0.95f, max = u.key == "lang" ? 0.55f : 0.95f, team = 1, kind = "mtele", col = D.col, src = u, dmg = 320, stun = u.key == "ta" ? 1.4f : 0, g = D.g });
        }

        void ChampAttack(Unit u, Body tg)
        {
            var T = u.T; u.anim = 0.3f;
            float mult = tg.IsEye ? 1.6f : (tg is Unit && (((Unit)tg).champ || tg == boss)) ? 1 : 1.5f;
            var col = u.team == 1 ? u.D.col : ELEM[u.D.el].col;
            if (T.range > 100) { proj.Add(new Proj { kind = PKind.Bolt, x = u.x, y = u.y - 16, tgt = tg, spd = 520, dmg = T.atk * mult * R(0.9f, 1.1f), team = u.team, src = u, life = 1.6f, col = col, hasCol = true, ob = u.pr >= BAR_R, big = true }); return; }
            Damage(tg, T.atk * mult * R(0.9f, 1.1f), u);
            fx.Add(new Fx { type = FxT.SlashC, x = u.x, y = u.y, a = u.face, t = 0.18f, max = 0.18f, col = col });
        }

        V2 RetreatPt(Unit u)
        {
            if (u.team == 1) return new V2(C.x + 60, C.y + 170);
            if (phase >= 2 && open >= 0) return OpenPt(BAR_R + 260);
            float a = AngC(u), r = M.Max(M.Dist(u.x, u.y, C.x, C.y) + 450, 2650); var p = Polar(a, r);
            return new V2(M.Clamp(p.x, 120, WW - 120), M.Clamp(p.y, 120, WH - 120));
        }
        void RespawnChamp(Unit u)
        {
            u.dead = false; u.hp = u.max; u.retreat = false; u.stun = 0;
            var p = open >= 0 ? OpenPt(BAR_R + 260) : new V2(CAMP.x + 400, CAMP.y + (u.id % 5 - 2) * 50);
            u.x = p.x; u.y = p.y; u.pr = Dist(u, C); PushFeed(NameOf(u) + " trở lại chiến trường");
        }

        void UpdateChamp(Unit u, float dt)
        {
            var T = u.T;
            if (u.flash > 0) u.flash -= dt; if (u.anim > 0) u.anim -= dt;
            if (u.dead) { if (u.team == 0) { u.respawn -= dt; if (u.respawn <= 0) RespawnChamp(u); } return; }
            u.skT -= dt; u.yanT -= dt; u.cd -= dt; if (u.team == 0) u.healT -= dt;
            if (t - u.hitT > 4) u.hp = M.Min(u.max, u.hp + u.max * 0.012f * dt);
            if (u.stun > 0) { u.stun -= dt; return; }
            u.moving = false;
            if (over != null) { if (u.team == 1) u.stun = 9; else { var g0 = ChampGoal(u); if (g0.HasValue && Dist(u, g0.Value.p) > g0.Value.stand) MoveTo(u, g0.Value.p, T.spd, dt); } return; }
            if (!u.retreat && u.hp < u.max * 0.22f && phase < 3) { u.retreat = true; PushFeed(NameOf(u) + " bị thương nặng, tạm lui"); }
            if (u.retreat)
            {
                if (u.hp > u.max * 0.72f) u.retreat = false;
                else { u.hp = M.Min(u.max, u.hp + u.max * 0.03f * dt); var p = RetreatPt(u); if (Dist(u, p) > 40) MoveTo(u, p, T.spd * 1.1f, dt); return; }
            }
            if (u.team == 1 && u.reinf && u.asg >= 0) { if (Dist(u, eyes[u.asg]) < 950) u.reinf = false; } else if (u.team == 1) u.reinf = false;
            ChampSkill(u, dt);
            u.retgt -= dt; if (u.retgt <= 0) { u.retgt = 0.3f; u.tgt = ChampTarget(u); }
            var tg = u.tgt; if (tg != null && (tg.dead || (tg is Eye && !(((Eye)tg).vuln > 0)))) tg = u.tgt = null;
            if (tg != null) { float d = Dist(u, tg) - tg.r - u.r; if (d <= T.range) { u.face = Ang(u, tg); if (u.cd <= 0) { ChampAttack(u, tg); u.cd = T.cd * R(0.9f, 1.1f); } } else MoveTo(u, tg, T.spd, dt); }
            else { var g = ChampGoal(u); if (g.HasValue && Dist(u, g.Value.p) > g.Value.stand) MoveTo(u, g.Value.p, T.spd * (NearestEnemy(u, 320) != null ? 1 : 1.3f), dt); }
        }

        // ================= quân sư hai phe =================
        int PlayerDir() { if (AUTO || hero.dead || M.Dist(hero.x, hero.y, C.x, C.y) < BAR_R) return hero.asg; return SecOf(AngC(hero)); }
        int MaAt(int d) { int n = 0; var e = eyes[d]; foreach (var m in foes) { if (m == boss || m.dead) continue; if (m.asg == d || Dist(m, e) < 1000) n++; } return n; }
        bool ChampNear(int d) { if (d < 0) return false; var e = eyes[d]; return allies.Any(a => !a.dead && Dist(a, e) < 1100 && Dist(a, CAMP) > 650); }
        Unit CounterAlive(int d)
        {
            var w = EYE_DEF[d].weak;
            foreach (var a in allies) { if (a == hero ? (w == Elem.Hoa || w == Elem.Tho || w == Elem.Kim) : CH_DEF[a.key].el == w) return a; }
            return null;
        }

        public void PlanSet(string kind, int tgt, string why, bool silent = false)
        {
            foreach (var a in allies) a.asg = kind == "split" ? SPLIT0[a.key] : tgt;
            plan = new Plan { kind = kind, tgt = tgt, t = t, why = why };
            string g, title, sub;
            if (kind == "split") { g = "分"; title = "Phân binh ba hướng"; sub = "Viêm Dương, Thanh Mộc đánh Tây · Thạch Nham, Thủy Nguyệt đánh Bắc · Ngao Thiên một mình đánh Nam. Hướng Đông xa doanh trại, tạm bỏ"; }
            else if (kind == "mass") { g = "合"; title = "Hợp binh hướng " + DirName(tgt); sub = why; }
            else { g = "破"; title = "Cửa " + DirName(tgt) + " đã mở · toàn quân dồn vào"; sub = why; }
            plan.g = g; plan.title = title; plan.sub = sub;
            PushFeed(kind == "split" ? "Quân sư: phân binh đánh Tây, Bắc, Nam" : kind == "mass" ? "Quân sư: năm tướng hợp binh hướng " + DirName(tgt) : "Năm tướng dồn về cửa " + DirName(tgt));
            if (!silent) StratEvent(g, title, sub, 0);
        }

        void CmdAtk()
        {
            if (over != null) return;
            if (open >= 0) { if (plan == null || plan.kind != "breach") PlanSet("breach", open, "Kết giới hướng " + DirName(open) + " sụp. Năm tướng theo cửa " + DirName(open) + " vào thành, nhắm thẳng nội thành"); return; }
            if (mode == "mass") { int d = PlayerDir(); if (plan == null || plan.kind != "mass" || plan.tgt != d) PlanSet("mass", d, "Theo lệnh Ngao Thiên: bốn tướng còn lại kéo quân về hướng " + DirName(d)); return; }
            if (mode == "split") { if (plan == null || plan.kind != "split") PlanSet("split", -1, ""); return; }
            if (plan == null) { PlanSet("split", -1, "", true); return; }
            if (plan.kind != "split" || t - plan.t < 28) return;
            int[] dirs = { 0, 1, 3 };
            foreach (var d in dirs)
            {
                var e = eyes[d];
                if (!e.dead && e.revealed && e.seals == 0 && e.hp < e.max * 0.8f && allies.Any(a => a.asg == d && !a.dead)) { PlanSet("mass", d, "Trận nhãn " + e.name + " hướng " + DirName(d) + " sắp vỡ. Các hướng khác rút quân dồn về đây"); return; }
            }
            foreach (var d in dirs)
            {
                int n = MaAt(d); if (n < 2) continue;
                int best = -1; float bs = -1e9f;
                foreach (var q in dirs)
                {
                    var e = eyes[q]; if (e.dead || CounterAlive(q) == null) continue;
                    int prog = (e.revealed ? 1 : 0) + (e.revealed ? e.maxSeals - e.seals : 0);
                    float s = Pow(q, 0, 1100) - Pow(q, 1, 900) + 14 * prog; if (s > bs) { bs = s; best = q; }
                }
                if (best == d) { PlanSet("mass", best, "Ma đạo dồn " + n + " tướng về hướng " + DirName(d) + ". Trận nhãn ở đây đã lung lay, năm tướng dồn về " + DirName(d) + " quyết chiến trước khi địch kịp vững"); return; }
                if (best >= 0) { PlanSet("mass", best, "Ma đạo dồn " + n + " tướng giữ hướng " + DirName(d) + ". Tránh chỗ mạnh, năm tướng dồn đánh hướng " + DirName(best)); return; }
            }
        }

        void CmdDef()
        {
            if (over != null) return;
            var ms = foes.Where(m => m != boss && !m.dead).ToList();
            if (open >= 0) { bool any = false; foreach (var m in ms) if (m.asg != Unit.BREACH) { m.asg = Unit.BREACH; any = true; } if (any) PushFeed("Ma tướng kéo về giữ cửa " + DirName(open)); return; }
            if (t < 22) return;
            foreach (var m in ms)
            {
                if (m.retreat) continue;
                int own = m.D.post; int thrOwn = Pow(own, 0, 1100);
                if (m.asg != own && ((ChampNear(own) && thrOwn >= 14) || thrOwn >= 30)) { m.asg = own; PushFeed(NameOf(m) + " quay về giữ hướng " + DirName(own)); continue; }
                int cur = m.asg; if (cur < 0) { m.asg = own; continue; }
                if (ChampNear(cur) || Pow(cur, 0, 1100) >= 24) continue;
                float am = AngC(m); int best = -1; float bg = 8;
                for (int d = 0; d < 4; d++) { if (d == cur || eyes[d].dead) continue; float g = Pow(d, 0, 1100) - Pow(d, 1, 900) - M.AngDiff(am, DIRS[d].a) * 2300 / 350; if (g > bg) { bg = g; best = d; } }
                if (best >= 0 && m.want != best) { m.want = best; continue; }
                if (best >= 0)
                {
                    m.want = -1; m.asg = best; m.reinf = true; string txt = NameOf(m) + " thấy hướng " + DirName(cur) + " yên ắng, chi viện hướng " + DirName(best); PushFeed(txt);
                    int n = foes.Count(f => f != boss && !f.dead && f.asg == best);
                    if (t - lastStrat > 7) StratEvent("援", n >= 3 ? "Ma đạo dồn " + n + " tướng về hướng " + DirName(best) : "Ma đạo điều tướng chi viện", txt, 1); else Warn(txt);
                    return;
                }
                if (cur != own) m.asg = own;
            }
        }

        void StratEvent(string g, string title, string sub, int side)
        {
            lastStrat = t;
            if (AUTO) { if (strat != null && strat.t > 0.5f) { strat.g = g; strat.title = title; strat.sub = sub; strat.side = side; strat.t = M.Max(strat.t, 3); } else strat = new Strat { t = 4, max = 4, g = g, title = title, sub = sub, side = side }; }
            else ShowBanner(title, sub, 3.4f, g);
        }

        void UpdateBoss(float dt)
        {
            var b = boss; if (b.dead) return;
            if (b.flash > 0) b.flash -= dt; if (b.anim > 0) b.anim -= dt;
            if (b.stun > 0) { b.stun -= dt; return; }
            if (over != null) return;
            if (!b.active)
            {
                if (phase >= 2 && allies.Any(a => !a.dead && Dist(b, a) < 720)) { b.active = true; PushFeed("Huyết Sát Phó Tướng xuất trận"); ShowBanner("Trấn thủ xuất trận", "Huyết Sát Phó Tướng · Kết Đan sơ kỳ", 2.2f, "煞"); }
                else return;
            }
            b.cd -= dt; b.cdT -= dt; b.cdK -= dt; b.moving = false;
            if (b.casting > 0) { b.casting -= dt; return; }
            Unit tg = null; float bd = 900; foreach (var a in allies) { if (a.dead) continue; float d = Dist(b, a) * (a == hero ? 0.85f : 1); if (d < bd) { bd = d; tg = a; } }
            if (tg == null) tg = NearestEnemy(b, 900);
            if (tg == null) return;
            if (tg.champ && b.cdK <= 0 && Dist(b, tg) < 420) { b.cdK = 15; zones.Add(new Zone { type = "khon", x = tg.x, y = tg.y, r = 165, t = 6, max = 6 }); PushFeed("Huyết Sát Phó Tướng bày Khốn Long Trận vây " + NameOf(tg)); if (tg == hero) Warn("Khốn Long Trận: độn thuật bị chặn trong vùng"); b.casting = 0.5f; b.anim = 0.5f; return; }
            if (b.cdT <= 0 && Dist(b, tg) < 360) { b.cdT = 6; teles.Add(new Tele { x = tg.x, y = tg.y, r = 120, t = 1.15f, max = 1.15f, team = 1, kind = "tram", dmg = 620 }); b.casting = 0.7f; b.anim = 0.7f; b.face = Ang(b, tg); return; }
            float dd = Dist(b, tg) - tg.r - b.r;
            if (dd <= b.T.range)
            {
                b.face = Ang(b, tg);
                if (b.cd <= 0)
                {
                    b.cd = 1.5f; b.anim = 0.3f;
                    for (int i = 0; i < units.Count; i++) { var e = units[i]; if (e.dead || e.team != 0) continue; if (Dist(b, e) - e.r < b.r + 52 && M.AngDiff(Ang(b, e), b.face) < 1.1f) Damage(e, e.champ ? 110 : R(30, 45), b); }
                    fx.Add(new Fx { type = FxT.SlashR, x = b.x, y = b.y, a = b.face, t = 0.22f, max = 0.22f });
                }
            }
            else MoveTo(b, tg, b.T.spd, dt);
        }
    }
}
