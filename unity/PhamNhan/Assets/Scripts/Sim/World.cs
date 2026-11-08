using System;
using System.Collections.Generic;
using System.Text;

namespace PhamNhan.Sim
{
    public enum Tier { Town = 0, City = 1, Great = 2, Holy = 3 }

    /// <summary>Thông số một trận công thành theo cấp cứ điểm. Thành trì = trận Phù Vân gốc.</summary>
    public class BattleCfg
    {
        public string name = "Phù Vân Thành", tierName = "Thành trì";
        public Tier tier = Tier.City; public bool neutral;
        public float dens = 1, hp = 1, atk = 1, eyeHp = 1, bossHp = 1; public int sealAdd = 0; public int seed = 20261102;

        public static BattleCfg For(World.Node n)
        {
            var c = new BattleCfg { name = n.name, tier = n.tier, tierName = World.TIER_N[(int)n.tier], neutral = n.owner == World.NEUTRAL, seed = 20261102 + n.id * 7919 };
            switch (n.tier)
            {
                case Tier.Town: c.dens = 0.45f; c.hp = 0.7f; c.atk = 0.75f; c.eyeHp = 0.5f; c.sealAdd = -1; c.bossHp = 0.55f; break;
                case Tier.Great: c.dens = 1.3f; c.hp = 1.2f; c.atk = 1.12f; c.eyeHp = 1.25f; c.bossHp = 1.35f; break;
                case Tier.Holy: c.dens = 1.6f; c.hp = 1.4f; c.atk = 1.25f; c.eyeHp = 1.5f; c.sealAdd = 1; c.bossHp = 1.7f; break;
            }
            if (c.neutral) { c.hp *= 0.9f; c.atk *= 0.9f; }
            if (n.name == "Phù Vân Thành") c.seed = 20261102;
            return c;
        }
    }

    /// <summary>Bản đồ thiên hạ 50 cứ điểm và lượt đi của hai phe.</summary>
    public class World
    {
        public const int CHINH = 0, MA = 1, NEUTRAL = 2;
        public const float CX = 500, CY = 380, MW = 1000, MH = 760;
        public static readonly string[] TIER_N = { "Thôn trấn", "Thành trì", "Đại thành", "Thánh địa" };
        public static readonly int[] GARRISON = { 200, 1000, 5000, 20000 };
        public static readonly string[] OWNER_N = { "Chính đạo", "Ma đạo", "Trung lập" };

        static readonly string[] TOWN_N = { "Thanh Thạch Trấn", "Lạc Diệp Thôn", "Bạch Vân Trấn", "Hồng Diệp Thôn", "Liễu Khê Trấn", "Tùng Phong Thôn", "Mặc Trúc Trấn", "Ngọc Tuyền Thôn", "Kim Sa Trấn", "Hàn Mai Thôn", "Vân Hà Trấn", "Thạch Lâm Thôn", "Phong Linh Trấn", "Lam Thủy Thôn", "Tử Trúc Trấn", "Đào Hoa Thôn", "Cổ Đằng Trấn", "Thanh Ngưu Thôn", "Hoàng Sa Trấn", "Bích Ba Thôn", "Dạ Nguyệt Trấn", "Hắc Thạch Thôn", "Huyết Lĩnh Trấn", "Âm Phong Thôn", "Quỷ Khóc Trấn", "Tàn Dương Thôn", "Xích Thổ Trấn", "Ô Nha Thôn", "Cốt Sơn Trấn", "Mê Vụ Thôn" };
        static readonly string[] CITY_N = { "Phù Vân Thành", "Thanh Long Thành", "Bạch Hổ Thành", "Lưu Vân Thành", "Thiên Thủy Thành", "Hắc Thủy Thành", "U Lan Thành", "Xích Viêm Thành", "Huyền Vũ Thành", "Lạc Nhật Thành" };
        static readonly string[] GREAT_N = { "Thiên Kiếm Quan", "Thái Hư Cung", "Vạn Ma Lĩnh", "Huyết Sát Điện", "Trấn Yêu Tháp" };
        static readonly string[] HOLY_N = { "Long Mạch Thánh Đàn", "Cửu Thiên Linh Tuyền", "Thái Cổ Tinh Thạch Quáng", "Hỗn Độn Thiên Môn", "Thiên Đạo Bia" };

        public class Node { public int id; public string name; public Tier tier; public float x, y; public int owner = NEUTRAL; public List<int> adj = new List<int>(); }

        public readonly List<Node> nodes = new List<Node>();
        public readonly List<int[]> edges = new List<int[]>();
        public int day = 1, hold = 0, holdM = 0; public const int HOLD_DAYS = 3; public string over; // "win" | "lose"
        public readonly List<string> log = new List<string>();
        int seed = 7;

        float Rnd() { seed = (int)((seed * 16807L) % 2147483647); return (seed - 1) / 2147483646f; }

        public World() { Gen(); Reset(); }

        void Ring(int n, float rx, float ry, float off, float jit, Tier tier, string[] names, int from)
        {
            for (int i = 0; i < n; i++)
            {
                float a = off + i * 2 * M.PI / n + (Rnd() - 0.5f) * jit, r = 1 + (Rnd() - 0.5f) * jit * 0.6f;
                nodes.Add(new Node { id = nodes.Count, tier = tier, name = names[from + i], x = CX + M.Cos(a) * rx * r, y = CY + M.Sin(a) * ry * r });
            }
        }

        /// <summary>Sinh vị trí và đường nối, giống bản đồ prototype.</summary>
        void Gen()
        {
            seed = 7; nodes.Clear(); edges.Clear();
            Ring(5, 115, 95, -M.PI / 2, 0, Tier.Holy, HOLY_N, 0);
            Ring(5, 225, 170, -M.PI / 2 + M.PI / 5, 0.1f, Tier.Great, GREAT_N, 0);
            Ring(10, 320, 240, -M.PI / 2, 0.12f, Tier.City, CITY_N, 0);
            Ring(14, 400, 295, -M.PI / 2 + 0.2f, 0.12f, Tier.Town, TOWN_N, 0);
            Ring(16, 465, 345, -M.PI / 2, 0.12f, Tier.Town, TOWN_N, 14);
            var set = new HashSet<long>();
            foreach (var a in nodes)
            {
                var near = new List<Node>(nodes); near.Remove(a);
                near.Sort((p, q) => M.Dist(a.x, a.y, p.x, p.y).CompareTo(M.Dist(a.x, a.y, q.x, q.y)));
                for (int k = 0; k < 3; k++) { int i = Math.Min(a.id, near[k].id), j = Math.Max(a.id, near[k].id); if (set.Add(i * 1000L + j)) edges.Add(new[] { i, j }); }
            }
            foreach (var e in edges) { nodes[e[0]].adj.Add(e[1]); nodes[e[1]].adj.Add(e[0]); }
            // đặt lại tên Đại thành theo hướng như prototype: Ma ở bắc, Chính ở nam
            var gr = nodes.FindAll(n => n.tier == Tier.Great); gr.Sort((p, q) => p.y.CompareTo(q.y));
            string[] gn = { "Vạn Ma Lĩnh", "Huyết Sát Điện", "Thái Hư Cung", "Trấn Yêu Tháp", "Thiên Kiếm Quan" };
            for (int i = 0; i < 5; i++) gr[i].name = gn[i];
        }

        /// <summary>Bắt đầu mùa mới: Chính đạo giữ phía nam, Ma đạo giữ phía bắc.</summary>
        public void Reset()
        {
            day = 1; hold = 0; holdM = 0; over = null; log.Clear(); seed = 7919;
            foreach (var n in nodes) n.owner = NEUTRAL;
            foreach (var n in nodes) if (n.tier == Tier.Town) { if (n.y > CY + 190) n.owner = CHINH; else if (n.y < CY - 190) n.owner = MA; }
            int c = 0, m = 0;
            foreach (var n in nodes) if (n.tier == Tier.City) { if (n.y > CY + 120 && c < 2) { n.owner = CHINH; c++; } else if (n.y < CY - 120 && m < 2) { n.owner = MA; m++; } }
            foreach (var n in nodes) if (n.tier == Tier.Great) { if (n.name == "Vạn Ma Lĩnh") n.owner = MA; if (n.name == "Thiên Kiếm Quan") n.owner = CHINH; }
            Log("Mùa I bắt đầu. Chính đạo giữ phía nam, Ma đạo giữ phía bắc.");
        }

        void Log(string s) { log.Insert(0, "Ngày " + day + " · " + s); if (log.Count > 40) log.RemoveAt(log.Count - 1); }

        public int Count(int owner) { int k = 0; foreach (var n in nodes) if (n.owner == owner) k++; return k; }
        public int Holy(int owner) { int k = 0; foreach (var n in nodes) if (n.owner == owner && n.tier == Tier.Holy) k++; return k; }

        /// <summary>Chính đạo đánh được nếu cứ điểm chưa thuộc mình và kề một cứ điểm của mình.</summary>
        public bool CanAttack(int id)
        {
            var n = nodes[id]; if (over != null || n.owner == CHINH) return false;
            foreach (var a in n.adj) if (nodes[a].owner == CHINH) return true;
            return false;
        }

        /// <summary>Ghi kết quả trận người chơi đánh, rồi cho hai phe AI đi lượt.</summary>
        public void Resolve(int id, bool win)
        {
            if (over != null) return;
            var n = nodes[id];
            if (win) { Log("Chính đạo công phá " + n.name + " (" + OWNER_N[n.owner] + ")."); n.owner = CHINH; }
            else Log("Công " + n.name + " thất bại, thủ quân giữ được thành.");
            AiTurn();
            day++;
            Check();
        }

        float Take(Tier t) { return t == Tier.Town ? 0.5f : t == Tier.City ? 0.38f : t == Tier.Great ? 0.3f : 0.25f; }

        void AiTurn()
        {
            // Ma đạo: một đến hai đợt tấn công vào cứ điểm kề lãnh thổ
            int waves = day >= 12 ? 3 : day >= 5 ? 2 : 1;
            for (int w = 0; w < waves; w++)
            {
                var t = Pick(MA); if (t == null) break;
                float p = t.owner == NEUTRAL ? 0.85f - (int)t.tier * 0.2f : Take(t.tier);
                if (Rnd() < p) { Log("Ma đạo chiếm " + t.name + (t.owner == CHINH ? " của Chính đạo!" : ".")); t.owner = MA; }
                else Log("Ma đạo đánh " + t.name + " nhưng bị đẩy lùi.");
            }
            // đồng môn Chính đạo tự chiếm thêm thôn trấn trung lập
            if (Rnd() < 0.45f)
            {
                var t = Pick(CHINH, true);
                if (t != null && t.owner == NEUTRAL) { t.owner = CHINH; Log("Đồng môn Chính đạo thu phục " + t.name + "."); }
            }
        }

        Node Pick(int side, bool townOnly = false)
        {
            Node best = null; float bs = -1;
            foreach (var n in nodes)
            {
                if (n.owner == side) continue; if (townOnly && (n.tier != Tier.Town || n.owner != NEUTRAL)) continue;
                bool touch = false; foreach (var a in n.adj) if (nodes[a].owner == side) touch = true; if (!touch) continue;
                float s = (n.owner == NEUTRAL ? 1.5f : 1) + (3 - (int)n.tier) * 0.4f + Rnd() * 1.5f + (side == MA && n.owner == CHINH && n.tier == Tier.Holy ? 2.5f : 0);
                if (s > bs) { bs = s; best = n; }
            }
            return best;
        }

        void Check()
        {
            if (Holy(CHINH) == 5)
            {
                hold++;
                if (hold >= HOLD_DAYS) { over = "win"; Log("Chính đạo giữ đủ năm Thánh địa " + HOLD_DAYS + " ngày. Thắng mùa I!"); }
                else Log("Chính đạo nắm đủ năm Thánh địa. Giữ thêm " + (HOLD_DAYS - hold) + " ngày nữa để thắng mùa.");
            }
            else hold = 0;
            if (over != null) return;
            if (Holy(MA) == 5) { holdM++; if (holdM < HOLD_DAYS) Log("Ma đạo nắm đủ năm Thánh địa! Còn " + (HOLD_DAYS - holdM) + " ngày để giành lại."); } else holdM = 0;
            if (holdM >= HOLD_DAYS || Count(CHINH) == 0) { over = "lose"; Log("Ma đạo nắm thiên hạ. Thua mùa I."); }
        }

        // ---------------- lưu và tải ----------------
        public string Save()
        {
            var sb = new StringBuilder(); sb.Append(day).Append('|').Append(seed).Append('|').Append(over ?? "").Append('|');
            foreach (var n in nodes) sb.Append(n.owner);
            sb.Append('|').Append(hold).Append(',').Append(holdM).Append('|').Append(string.Join("\u001f", log));
            return sb.ToString();
        }

        public bool Load(string s)
        {
            try
            {
                var p = s.Split('|'); if (p.Length < 4 || p[3].Length != nodes.Count) return false;
                day = int.Parse(p[0]); seed = int.Parse(p[1]); over = p[2] == "" ? null : p[2];
                for (int i = 0; i < nodes.Count; i++) nodes[i].owner = p[3][i] - '0';
                if (p.Length > 4) { var h = p[4].Split(','); hold = int.Parse(h[0]); holdM = h.Length > 1 ? int.Parse(h[1]) : 0; }
                log.Clear(); if (p.Length > 5 && p[5].Length > 0) log.AddRange(p[5].Split('\u001f'));
                return true;
            }
            catch { return false; }
        }
    }
}
