using System.Collections.Generic;
using UnityEngine;
using PhamNhan.Sim;
using static PhamNhan.Sim.Defs;

namespace PhamNhan.View
{
    /// <summary>Màn xem tạo hình nhân vật và chân dung trên bảng tướng.</summary>
    public partial class App
    {
        float rosterT; string rosterSel = "hero";

        static readonly string[] ROSTER_CHINH = { "hero", "viem", "thach", "nguyet", "thanh", "kiem", "long", "phu", "loan", "quy" };
        static readonly string[] ROSTER_MA = { "lang", "thi", "bo", "ta", "boss", "no", "cung", "tuong" };

        static readonly Dictionary<string, string> DESC = new Dictionary<string, string> {
            {"hero","Kiếm tu áo trắng của Vạn Kiếm Tông. Ba phi kiếm luôn treo sau lưng, Ngự Kiếm phá được ấn Mộc."},
            {"viem","Phù sư tóc lửa của Hạo Nhiên Tông, kẹp phù chú giữa hai ngón, tay kia giữ một đốm hỏa. Liệt Diễm Phù phá được ấn Kim."},
            {"thach","Trận sư vạm vỡ của Vô Cực Trận Tông, ôm Trấn Sơn Ấn, lưng đeo trận bàn bát quái. Ấn đá phá được ấn Thủy."},
            {"nguyet","Nữ cung thủ tóc bạc của Vạn Pháp Tông, cài trâm trăng khuyết. Huyền Băng Tiễn phá được ấn Hỏa."},
            {"thanh","Long tộc của Vạn Thú Tông, có sừng ngọc và đuôi rồng, cầm giáo ngọc bích. Đằng La Triền trói chân địch."},
            {"lang","Vua sói của Vạn Ma Tông, khoác áo choàng đỏ máu, móng vuốt xé giáp. Huyết Lang Trảo lao tới vồ mồi."},
            {"thi","Lão quỷ còng lưng của Quỷ Dị Môn, gậy treo đèn hồn xanh, hồn ma lởn vởn quanh người. Triệu Thi gọi thi binh."},
            {"bo","Khách đội nón lá của Bổ Thiên Các, mặt nạ vàng che kín, tay cầm la bàn trận. Bổ Thiên Trận giữ vững kết giới."},
            {"ta","Ma nữ của Tà Cực Tông, búi tóc đôi có sừng nhỏ, cầm sáo ma âm, dải lụa bay sau lưng. Tà Cực Ma Âm làm rối quân Chính đạo."},
            {"boss","Trấn thủ Kết Đan của thành Phù Vân. Giáp đỏ đen, mũ trụ hai sừng, đại đao dài. Biết bày Khốn Long Trận."},
            {"kiem","Đệ tử cầm kiếm, giữ hàng đầu khi công thành."},
            {"long","Chiến binh Long nhân mang khiên đồng và đao, máu dày, đứng chặn cửa vỡ."},
            {"phu","Phù sư đội mũ đạo, ném phù chú từ xa."},
            {"loan","Kỵ binh cưỡi chim loan xanh, lao nhanh, đâm thương."},
            {"quy","Rùa khổng lồ cõng chòi gỗ và chày húc, chuyên phá tường thành."},
            {"no","Nô binh trùm đầu cầm đao cong, đông và liều mạng."},
            {"cung","Cung thủ che mặt, bắn tên từ xa."},
            {"tuong","Ma tướng giáp đen mũ sừng, cầm rìu lớn chỉ huy đám nô binh."},
        };

        /// <summary>Vẽ chân dung tròn cho bảng tướng. Trả về false khi ảnh chưa dựng xong.</summary>
        bool Portrait(Rect r, string key, bool dim)
        {
            if (CharSprites.Get(key) == null) return false;
            var old = GUI.color;
            Ui.Box(r, dim ? new Color(0.75f, 0.75f, 0.75f, 0.9f) : new Color(0.93f, 0.97f, 0.95f, 1));
            GUI.color = dim ? new Color(0.55f, 0.55f, 0.55f, 0.8f) : Color.white;
            CharSprites.Face(r, key);
            GUI.color = old;
            return true;
        }

        void DrawRoster(Rect sa)
        {
            rosterT += Time.unscaledDeltaTime;
            Ui.Fill(new Rect(0, 0, RW, 720), new Color(0.06f, 0.16f, 0.13f, 0.5f));
            var card = new Rect(Mathf.Max(sa.x + 12, RW / 2 - 620), 24, Mathf.Min(1240, sa.width - 24), 672);
            Ui.Box(card, new Color(1, 1, 1, 0.95f));
            Ui.Text(new Rect(card.x + 28, card.y + 16, 400, 40), "Nhân vật", 30, Ui.INK, TextAnchor.UpperLeft, true);

            if (!CharSprites.Ready)
            {
                Ui.Text(new Rect(card.x, card.y + 300, card.width, 40), "Đang vẽ tạo hình...", 22, Ui.SUB, TextAnchor.MiddleCenter);
            }
            else
            {
                float cw = 112, ch = 128, gx = card.x + 24;
                float gy = card.y + 64;
                Ui.Text(new Rect(gx, gy, 400, 22), "CHÍNH ĐẠO", 14, Ui.ALLY, TextAnchor.UpperLeft, true);
                Grid(ROSTER_CHINH, gx, gy + 24, cw, ch, 5);
                gy += 24 + ch * 2 + 14;
                Ui.Text(new Rect(gx, gy, 400, 22), "MA ĐẠO", 14, Ui.FOE, TextAnchor.UpperLeft, true);
                Grid(ROSTER_MA, gx, gy + 24, cw, ch, 5);

                // khung chi tiết
                var d = new Rect(gx + cw * 5 + 24, card.y + 64, card.xMax - (gx + cw * 5 + 24) - 24, card.height - 160);
                Ui.Box(d, new Color(0.93f, 0.97f, 0.95f, 1));
                var cs = CharSprites.Get(rosterSel); var T = TYPES[rosterSel];
                int fr = (int)(rosterT * 1.6f) % Chars_Frames;
                float side = Mathf.Min(d.height - 150, d.width * 0.6f);
                var img = new Rect(d.x + 12, d.y + 8, side, side);
                if (cs != null) GUI.DrawTexture(img, cs.f[fr].texture, ScaleMode.ScaleToFit, true);
                float tx = img.xMax + 6, tw = d.xMax - tx - 14;
                Ui.Text(new Rect(tx, d.y + 24, tw, 34), T.name, 26, Ui.INK, TextAnchor.UpperLeft, true, true);
                ChampDef D; bool champ = CH_DEF.TryGetValue(rosterSel, out D);
                string sub = champ ? D.sect : T.team == 0 ? "Binh chủng Chính đạo" : "Binh chủng Ma đạo";
                Ui.Text(new Rect(tx, d.y + 62, tw, 44), sub, 16, T.team == 0 ? Ui.ALLY : Ui.FOE, TextAnchor.UpperLeft, true, true);
                float sy = d.y + 112;
                if (champ)
                {
                    Ui.Text(new Rect(tx, sy, tw, 44), D.g, 36, Shapes.C(D.col), TextAnchor.UpperLeft, true); sy += 48;
                    if (D.sk != null) { Ui.Text(new Rect(tx, sy, tw, 40), "Tuyệt kỹ: " + D.sk, 15, Ui.INK, TextAnchor.UpperLeft, false, true); sy += 40; }
                }
                Ui.Text(new Rect(tx, sy, tw, 22), "Máu " + T.hp.ToString("0"), 15, Ui.SUB); sy += 22;
                if (T.atk > 0) { Ui.Text(new Rect(tx, sy, tw, 22), "Công " + T.atk.ToString("0") + (T.range > 100 ? " · đánh xa" : " · cận chiến"), 15, Ui.SUB); sy += 22; }
                string desc; DESC.TryGetValue(rosterSel, out desc);
                Ui.Text(new Rect(d.x + 18, img.yMax + 6, d.width - 36, d.yMax - img.yMax - 12), desc ?? "", 17, Ui.INK, TextAnchor.UpperLeft, false, true);
            }
            if (MenuButton(new Rect(card.xMax - 300, card.yMax - 80, 270, 60), "Quay lại", true)) scr = Screen_.Menu;
        }

        const int Chars_Frames = Art.Chars.FRAMES;

        void Grid(string[] keys, float x0, float y0, float cw, float ch, int cols)
        {
            var e = Event.current;
            for (int i = 0; i < keys.Length; i++)
            {
                var k = keys[i]; var r = new Rect(x0 + (i % cols) * cw, y0 + (i / cols) * ch, cw - 8, ch - 8);
                bool sel = k == rosterSel;
                Ui.Box(r, sel ? new Color(0.06f, 0.64f, 0.5f, 0.22f) : new Color(0.93f, 0.97f, 0.95f, 1));
                var cs = CharSprites.Get(k);
                // ô đang chọn thì bước đi tại chỗ
                int fr = sel ? new[] { 1, 0, 2, 0 }[(int)(rosterT * 6) & 3] : 0;
                if (cs != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 2, r.width - 8, r.height - 26), cs.f[fr].texture, ScaleMode.ScaleToFit, true);
                Ui.Text(new Rect(r.x + 2, r.yMax - 30, r.width - 4, 28), TYPES[k].name, 11, Ui.INK, TextAnchor.MiddleCenter, sel, true);
                if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) { e.Use(); rosterSel = k; rosterT = 0; snd.Play("tap"); }
            }
        }
    }
}
