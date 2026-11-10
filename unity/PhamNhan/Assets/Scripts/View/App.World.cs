using UnityEngine;
using PhamNhan.Sim;

namespace PhamNhan.View
{
    /// <summary>Màn bản đồ thiên hạ: 50 cứ điểm, chọn nơi tấn công, theo dõi lượt Ma đạo.</summary>
    public partial class App
    {
        World W; int wSel = -1; bool campaign, campResolved; int campNode = -1; float newSeasonT = -9, worldT;

        void LoadWorld()
        {
            W = new World();
            var s = PlayerPrefs.GetString("world", "");
            if (s.Length > 0 && !W.Load(s)) W.Reset();
        }
        void SaveWorld() { PlayerPrefs.SetString("world", W.Save()); PlayerPrefs.Save(); }

        void OpenWorld()
        {
            scr = Screen_.World; campaign = false; G = new Siege(true); G.intro = 0;
            if (wSel < 0 || wSel >= W.nodes.Count) { wSel = -1; foreach (var n in W.nodes) if (W.CanAttack(n.id) && (wSel < 0 || n.tier < W.nodes[wSel].tier)) wSel = n.id; }
        }

        static Color OwnerCol(int o) { return o == World.CHINH ? Ui.ALLY : o == World.MA ? Ui.FOE : new Color(0.62f, 0.6f, 0.54f); }
        static readonly float[] NODE_R = { 8, 12, 15, 18 };
        static readonly string[] NODE_G = { "", "城", "關", "聖" };

        void DrawWorldMap(Rect sa)
        {
            worldT += Time.unscaledDeltaTime;
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.88f, 0.94f, 0.9f, 1));
            float panelW = Mathf.Min(360, RW * 0.32f);
            var area = new Rect(sa.x + 10, 10, RW - sa.x - (RW - sa.xMax) - panelW - 30, 700);
            float k = Mathf.Min(area.width / World.MW, area.height / World.MH);
            Vector2 o = new Vector2(area.x + (area.width - World.MW * k) / 2, area.y + (area.height - World.MH * k) / 2);
            System.Func<float, float, Vector2> P = (x, y) => new Vector2(o.x + x * k, o.y + y * k);

            // nền: vùng Ma phía bắc, vùng Chính phía nam, vòng Thánh địa ở giữa
            Ui.Box(new Rect(o.x, o.y, World.MW * k, World.MH * k), new Color(0.97f, 0.99f, 0.97f, 1));
            var c0 = P(World.CX, World.CY);
            Ui.Ellipse(c0, 470 * k, 352 * k, new Color(0.56f, 0.82f, 0.62f, 0.18f));
            Ui.Ellipse(c0, 330 * k, 250 * k, new Color(0.56f, 0.82f, 0.62f, 0.16f));
            Ui.Ellipse(c0, 165 * k, 140 * k, new Color(1f, 0.85f, 0.45f, 0.22f));
            Ui.Ellipse(c0, 165 * k, 140 * k, new Color(0.85f, 0.65f, 0.25f, 0.5f), true);
            Ui.Text(new Rect(o.x + 12, o.y + 8, 300, 26), "北 · MA VỰC", 16, new Color(0.9f, 0.3f, 0.25f, 0.7f), TextAnchor.UpperLeft, true);
            Ui.Text(new Rect(o.x + 12, o.y + World.MH * k - 32, 300, 26), "南 · CHÍNH VỰC", 16, new Color(0.18f, 0.55f, 0.88f, 0.75f), TextAnchor.UpperLeft, true);

            // đường nối
            foreach (var e in W.edges)
            {
                var a = W.nodes[e[0]]; var b = W.nodes[e[1]]; bool same = a.owner == b.owner && a.owner != World.NEUTRAL;
                Ui.Line(P(a.x, a.y), P(b.x, b.y), same ? 3 : 1.6f, same ? OwnerCol(a.owner) * new Color(1, 1, 1, 0.6f) : new Color(0.45f, 0.5f, 0.45f, 0.45f));
            }

            // cứ điểm
            var ev = Event.current; float pul = 0.5f + 0.5f * Mathf.Sin(worldT * 4);
            foreach (var n in W.nodes)
            {
                var p = P(n.x, n.y); float r = NODE_R[(int)n.tier] * Mathf.Clamp(k * 1.25f, 0.9f, 1.4f);
                bool sel = n.id == wSel, att = W.CanAttack(n.id);
                if (n.tier == Tier.Holy) Ui.Circle(p, r * 2, OwnerCol(n.owner) * new Color(1, 1, 1, 0.18f));
                if (att) Ui.Circle(p, r + 6 + pul * 3, new Color(0.95f, 0.72f, 0.2f, 0.85f), true);
                if (sel) Ui.Circle(p, r + 5, Ui.INK, true);
                Ui.Circle(p, r + 2, new Color(0.1f, 0.12f, 0.12f, 0.85f));
                Ui.Circle(p, r, OwnerCol(n.owner));
                if (NODE_G[(int)n.tier].Length > 0) Ui.Text(new Rect(p.x - r, p.y - r, r * 2, r * 2), NODE_G[(int)n.tier], Mathf.RoundToInt(r * 1.15f), Color.white, TextAnchor.MiddleCenter, true);
                if (n.tier == Tier.Holy)
                    Ui.Text(new Rect(p.x - 62, p.y + r + 2, 124, 36), n.name, 12, Ui.INK, TextAnchor.UpperCenter, true, true);
                else if (n.tier != Tier.Town || sel)
                    Ui.Text(new Rect(p.x - 90, p.y + r + 2, 180, 18), n.name, 12, Ui.INK, TextAnchor.UpperCenter, sel || n.tier >= Tier.Great, false);
            }
            if (ev.type == EventType.MouseDown && area.Contains(ev.mousePosition))
            {
                int best = -1; float bd = 9999;
                foreach (var n in W.nodes) { float d = Vector2.Distance(P(n.x, n.y), ev.mousePosition); if (d < bd) { bd = d; best = n.id; } }
                if (best >= 0 && bd < 30) { wSel = best; snd.Play("tap"); ev.Use(); }
            }

            // ---- bảng bên phải ----
            var pn = new Rect(sa.xMax - panelW - 10, 10, panelW, 700);
            Ui.Box(pn, new Color(1, 1, 1, 0.96f));
            float x = pn.x + 18, w = pn.width - 36, y = pn.y + 14;
            Ui.Text(new Rect(x, y, w, 30), "Thiên hạ · Ngày " + W.day, 24, Ui.INK, TextAnchor.UpperLeft, true); y += 36;
            int cc = W.Count(World.CHINH), cm = W.Count(World.MA), cn = 50 - cc - cm;
            Ui.Text(new Rect(x, y, w, 20), "Chính đạo " + cc, 15, Ui.ALLY, TextAnchor.UpperLeft, true);
            Ui.Text(new Rect(x, y, w, 20), "Ma đạo " + cm, 15, Ui.FOE, TextAnchor.UpperRight, true); y += 22;
            Ui.Fill(new Rect(x, y, w * cc / 50f, 8), Ui.ALLY); Ui.Fill(new Rect(x + w * cc / 50f, y, w * cn / 50f, 8), new Color(0.75f, 0.74f, 0.7f)); Ui.Fill(new Rect(x + w * (cc + cn) / 50f, y, w * cm / 50f, 8), Ui.FOE); y += 14;
            Ui.Text(new Rect(x, y, w, 20), "Thánh địa · Chính " + W.Holy(World.CHINH) + "/5 · Ma " + W.Holy(World.MA) + "/5", 14, Ui.SUB); y += 20;
            Ui.Text(new Rect(x, y, w, 36), "Giữ cả năm Thánh địa " + World.HOLD_DAYS + " ngày để thắng mùa" + (W.hold > 0 ? " (đã giữ " + W.hold + ")" : ""), 13, Ui.SUB, TextAnchor.UpperLeft, false, true); y += 40;

            // cứ điểm đang chọn
            var cr = new Rect(x - 6, y, w + 12, 236); Ui.Box(cr, new Color(0.93f, 0.97f, 0.95f, 1)); float cy = y + 10;
            if (W.over != null)
            {
                bool win = W.over == "win";
                Ui.Text(new Rect(x, cy + 20, w, 40), win ? "捷 Thắng mùa I" : "Thua mùa I", 28, win ? Ui.JADE : Ui.FOE, TextAnchor.UpperCenter, true);
                Ui.Text(new Rect(x, cy + 70, w, 60), win ? "Chính đạo nắm trọn năm Thánh địa." : "Ma đạo nắm thiên hạ.", 16, Ui.INK, TextAnchor.UpperCenter, false, true);
                if (MenuButton(new Rect(x + 10, cy + 150, w - 20, 60), "Mùa mới", true)) { W.Reset(); SaveWorld(); wSel = -1; OpenWorld(); }
            }
            else if (wSel >= 0)
            {
                var n = W.nodes[wSel];
                Ui.Text(new Rect(x, cy, w, 28), n.name, 21, Ui.INK, TextAnchor.UpperLeft, true); cy += 30;
                Ui.Text(new Rect(x, cy, w, 20), World.TIER_N[(int)n.tier] + " · " + World.OWNER_N[n.owner], 15, OwnerCol(n.owner), TextAnchor.UpperLeft, true); cy += 24;
                Ui.Text(new Rect(x, cy, w, 20), "Thủ quân khoảng " + World.GARRISON[(int)n.tier].ToString("N0").Replace(',', '.'), 14, Ui.SUB); cy += 20;
                Ui.Text(new Rect(x, cy, w, 20), "Độ khó " + new string('★', (int)n.tier + 1) + new string('☆', 3 - (int)n.tier), 14, Ui.SUB); cy += 26;
                if (W.CanAttack(n.id))
                {
                    if (MenuButton(new Rect(x + 4, cy + 4, w - 8, 66), "Tấn công", true))
                    {
                        campaign = true; campNode = n.id; StartGame(false, BattleCfg.For(n));
                    }
                }
                else
                {
                    string why = n.owner == World.CHINH ? "Cứ điểm của Chính đạo." : "Cần chiếm một cứ điểm kề bên trước.";
                    Ui.Text(new Rect(x, cy + 6, w, 60), why, 15, Ui.SUB, TextAnchor.UpperLeft, false, true);
                }
            }
            else Ui.Text(new Rect(x, cy + 10, w, 60), "Chạm vào một cứ điểm để xem.", 15, Ui.SUB, TextAnchor.UpperLeft, false, true);
            y += 246;

            // nhật ký chiến cục
            Ui.Text(new Rect(x, y, w, 20), "CHIẾN BÁO", 12, Ui.JADE, TextAnchor.UpperLeft, true); y += 20;
            float ly = y, lmax = pn.yMax - 86;
            for (int i = 0; i < W.log.Count; i++)
            {
                float h = Ui.TextH(W.log[i], 13, w); if (ly + h > lmax) break;
                Ui.Text(new Rect(x, ly, w, h), W.log[i], 13, i == 0 ? Ui.INK : Ui.SUB, TextAnchor.UpperLeft, false, true); ly += h + 2;
            }

            float bw = (w - 8) / 2, by = pn.yMax - 72;
            if (MenuButton(new Rect(x, by, bw, 58), "Thoát", false)) GoMenu();
            bool confirm = worldT - newSeasonT < 3;
            if (MenuButtonSmall(new Rect(x + bw + 8, by, bw, 58), confirm ? "Bấm lần nữa" : "Mùa mới", confirm))
            {
                if (confirm) { W.Reset(); SaveWorld(); wSel = -1; OpenWorld(); newSeasonT = -9; } else newSeasonT = worldT;
            }
        }
    }
}
