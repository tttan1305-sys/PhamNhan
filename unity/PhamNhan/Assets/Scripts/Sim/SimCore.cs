// Phàm Nhân · lõi mô phỏng màn công thành (C# thuần, không phụ thuộc UnityEngine)
// Toạ độ thế giới giống bản HTML: x sang phải, y xuống dưới, đơn vị là điểm ảnh bản đồ.
using System;
using System.Collections.Generic;

namespace PhamNhan.Sim
{
    public struct V2
    {
        public float x, y;
        public V2(float x, float y) { this.x = x; this.y = y; }
    }

    /// <summary>Bộ sinh số ngẫu nhiên Park–Miller, giống hệt bản HTML để tái lập trận.</summary>
    public class Rng
    {
        public long seed;
        public Rng(long s) { seed = s; }
        public float Next() { seed = (seed * 16807) % 2147483647; return (float)((seed - 1) / 2147483646.0); }
        public float R(float a, float b) { return a + Next() * (b - a); }
    }

    public static class M
    {
        public const float PI = (float)Math.PI;
        public const float TAU = PI * 2f;
        public static float Deg(float d) { return d * PI / 180f; }
        public static float Hypot(float x, float y) { return (float)Math.Sqrt(x * x + y * y); }
        public static float Dist(float ax, float ay, float bx, float by) { return Hypot(ax - bx, ay - by); }
        public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }
        public static float Cos(float a) { return (float)Math.Cos(a); }
        public static float Sin(float a) { return (float)Math.Sin(a); }
        public static float Abs(float a) { return a < 0 ? -a : a; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static float Sqrt(float a) { return (float)Math.Sqrt(a); }
        public static float Pow(float a, float b) { return (float)Math.Pow(a, b); }
        public static float Sign(float a) { return a > 0 ? 1 : a < 0 ? -1 : 0; }
        public static float Clamp(float v, float a, float b) { return v < a ? a : v > b ? b : v; }
        public static float AngDiff(float a, float b)
        {
            float d = Abs(a - b) % TAU;
            return d > PI ? TAU - d : d;
        }
        public static float AngSigned(float a, float b)
        {
            float s = b - a;
            while (s > PI) s -= TAU;
            while (s < -PI) s += TAU;
            return s;
        }
        public static string Fmt(float n)
        {
            int v = (int)Math.Round(Math.Max(0, n));
            return v.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
        }
    }

    /// <summary>Màu RGB 0–255 cho tầng hiển thị.</summary>
    public struct Rgb
    {
        public byte r, g, b;
        public Rgb(int r, int g, int b) { this.r = (byte)r; this.g = (byte)g; this.b = (byte)b; }
        public static Rgb Hex(string h)
        {
            if (h.StartsWith("#")) h = h.Substring(1);
            int v = Convert.ToInt32(h, 16);
            return new Rgb((v >> 16) & 255, (v >> 8) & 255, v & 255);
        }
    }

    public enum Elem { Kim, Moc, Thuy, Hoa, Tho, Pha, None }

    public class ElemDef
    {
        public string n, g; public Rgb col;
        public ElemDef(string n, string g, Rgb c) { this.n = n; this.g = g; col = c; }
    }

    public enum Kind { Hero, Champ, Melee, Caster, Cav, Siege, Archer, Boss }

    public class UType
    {
        public string key, name; public int team; public float hp, atk, range, spd, cd, r; public Kind kind; public Rgb body, trim;
    }

    public class ChampDef
    {
        public string key; public Elem el; public string g, sect, sk; public string tk; public float cd; public int post = -1; public Rgb col, robe, trim;
    }

    public class EyeDef
    {
        public Elem el, weak; public float a, r; public int seals; public float hp; public string place, toolT; public float voff;
    }

    public class DirDef { public string k, n; public float a; }

    public static class Defs
    {
        public const float VW = 1280, VH = 720, WW = 6400, WH = 5800;
        public static readonly V2 C = new V2(3700, 2950);
        public const float WALL_R = 1650, WALL_T = 40, IN_R = 700, IN_T = 32, BAR_R = 1800, RING_R = 1150, RING_W = 50, AV_W = 80, FLAG_R = 110;
        public static readonly float[] G_OUT = { -M.PI / 2, 0, M.PI / 2, M.PI };
        public static readonly float[] G_IN = { 0, M.PI / 2, M.PI };
        public static readonly V2 CAMP = new V2(C.x - 2750, C.y);
        public const float Z_FIGHT = 0.88f, Z_FLY = 0.74f, Z0 = 0.218f, TIME = 900, VULN_T = 9, INTRO = 8.6f;
        public static readonly float SEC_H = M.PI / 4;

        public class Ring { public float R, T, half; public float[] gates; }
        public static readonly Ring[] RINGS = {
            new Ring{R=WALL_R,T=WALL_T,half=100/WALL_R,gates=G_OUT},
            new Ring{R=IN_R,T=IN_T,half=90/IN_R,gates=G_IN}
        };

        public static readonly Dictionary<Elem, ElemDef> ELEM = new Dictionary<Elem, ElemDef>{
            {Elem.Kim,new ElemDef("Kim","金",new Rgb(236,226,190))},
            {Elem.Moc,new ElemDef("Mộc","木",new Rgb(127,209,139))},
            {Elem.Thuy,new ElemDef("Thủy","水",new Rgb(111,183,255))},
            {Elem.Hoa,new ElemDef("Hỏa","火",new Rgb(255,138,74))},
            {Elem.Tho,new ElemDef("Thổ","土",new Rgb(214,170,106))},
            {Elem.Pha,new ElemDef("Phá","破",new Rgb(255,215,120))},
        };

        public static readonly DirDef[] DIRS = {
            new DirDef{k="W",n="Tây",a=M.PI}, new DirDef{k="N",n="Bắc",a=-M.PI/2},
            new DirDef{k="E",n="Đông",a=0}, new DirDef{k="S",n="Nam",a=M.PI/2}
        };

        public static readonly EyeDef[] EYE_DEF = {
            new EyeDef{el=Elem.Kim,weak=Elem.Hoa,a=194,r=2230,seals=2,hp=6500,place="Loạn Thạch Cương",toolT="Hỏa pháp: Lôi Hỏa Phù, Liệt Diễm",voff=-8},
            new EyeDef{el=Elem.Thuy,weak=Elem.Tho,a=-104,r=2200,seals=2,hp=5000,place="Đầm Sen",toolT="Thổ pháp: Trấn Sơn Ấn",voff=9},
            new EyeDef{el=Elem.Hoa,weak=Elem.Thuy,a=14,r=2200,seals=2,hp=5000,place="Hỏa Diệm Cốc",toolT="Thủy pháp: Huyền Băng Tiễn",voff=-9},
            new EyeDef{el=Elem.Moc,weak=Elem.Kim,a=104,r=2200,seals=3,hp=6000,place="Hắc Lâm",toolT="Kim pháp: Ngự Kiếm",voff=-9},
        };
        public static V2 EyePos(int i) { var d = EYE_DEF[i]; float a = M.Deg(d.a); return new V2(C.x + M.Cos(a) * d.r, C.y + M.Sin(a) * d.r); }

        static UType T(string key, string name, int team, float hp, float atk, float range, float spd, float cd, float r, Kind kind, string body = null, string trim = null)
        {
            return new UType { key = key, name = name, team = team, hp = hp, atk = atk, range = range, spd = spd, cd = cd, r = r, kind = kind, body = body != null ? Rgb.Hex(body) : new Rgb(200, 200, 200), trim = trim != null ? Rgb.Hex(trim) : new Rgb(255, 255, 255) };
        }

        public static readonly Dictionary<string, UType> TYPES = new Dictionary<string, UType>{
            {"hero",T("hero","Ngao Thiên",0,3600,0,0,165,0,14,Kind.Hero)},
            {"viem",T("viem","Viêm Dương",0,3000,44,230,150,1.1f,14,Kind.Champ)},
            {"thach",T("thach","Thạch Nham",0,4200,56,30,140,1.0f,15,Kind.Champ)},
            {"nguyet",T("nguyet","Thủy Nguyệt",0,2900,42,240,152,1.0f,14,Kind.Champ)},
            {"thanh",T("thanh","Thanh Mộc",0,3300,40,32,150,0.95f,14,Kind.Champ)},
            {"kiem",T("kiem","Kiếm Tu Vệ",0,130,15,22,74,0.9f,9,Kind.Melee,"#2d6a8a","#8fd0ef")},
            {"long",T("long","Long Nhân Chiến Sĩ",0,250,22,24,64,1.1f,12,Kind.Melee,"#1f5670","#d9b46a")},
            {"phu",T("phu","Phù Sư",0,90,14,210,66,1.4f,9,Kind.Caster,"#3b7d8f","#efe4cf")},
            {"loan",T("loan","Thanh Loan Kỵ",0,180,19,24,128,0.8f,13,Kind.Cav,"#2f8a76","#a8f0d8")},
            {"quy",T("quy","Huyền Quy Công Thành",0,1800,240,40,36,2.4f,22,Kind.Siege,"#2d5a5a","#6fc3a8")},
            {"lang",T("lang","Hắc Lang Vương",1,4600,68,30,152,1.0f,15,Kind.Champ)},
            {"thi",T("thi","Âm Thi Lão Quỷ",1,4200,46,220,134,1.2f,14,Kind.Champ)},
            {"bo",T("bo","Bổ Thiên Khách",1,4400,58,30,148,1.0f,15,Kind.Champ)},
            {"ta",T("ta","Tà Cực Ma Nữ",1,4000,54,200,158,1.0f,14,Kind.Champ)},
            {"no",T("no","Huyết Nô",1,115,16,22,72,0.95f,9,Kind.Melee,"#7a2420","#ff8c84")},
            {"cung",T("cung","Huyết Cung Thủ",1,85,12,240,62,1.7f,9,Kind.Archer,"#5a1f22","#e5604d")},
            {"tuong",T("tuong","Ma Tướng",1,460,34,26,68,1.2f,13,Kind.Melee,"#2a1214","#d9534f")},
            {"boss",T("boss","Huyết Sát Phó Tướng",1,9000,110,40,84,1.5f,24,Kind.Boss)},
        };

        static ChampDef CD(string key, Elem el, string g, string sect, string sk, string tk, float cd, int post, string col, string robe, string trim)
        {
            return new ChampDef { key = key, el = el, g = g, sect = sect, sk = sk, tk = tk, cd = cd, post = post, col = col != null ? Rgb.Hex(col) : new Rgb(255, 255, 255), robe = robe != null ? Rgb.Hex(robe) : new Rgb(80, 80, 80), trim = trim != null ? Rgb.Hex(trim) : new Rgb(255, 255, 255) };
        }

        public static readonly Dictionary<string, ChampDef> CH_DEF = new Dictionary<string, ChampDef>{
            {"hero",CD("hero",Elem.Kim,"劍","Vạn Kiếm Tông","Ngự Kiếm",null,0,-1,"#2f8fe0","#1f5f9e","#f3d9a0")},
            {"viem",CD("viem",Elem.Hoa,"炎","Hạo Nhiên Tông","Liệt Diễm Phù","fire",7.5f,-1,"#ff8a4a","#8a3a22","#ffb070")},
            {"thach",CD("thach",Elem.Tho,"岩","Vô Cực Trận Tông","Trấn Sơn Ấn","an",7,-1,"#d6aa6a","#6a5434","#e0b878")},
            {"nguyet",CD("nguyet",Elem.Thuy,"月","Vạn Pháp Tông","Huyền Băng Tiễn","ice",6.5f,-1,"#6fb7ff","#2a4f7a","#9fd0ff")},
            {"thanh",CD("thanh",Elem.Moc,"龍","Vạn Thú Tông · Long tộc","Đằng La Triền","vine",7,-1,"#7fd18b","#2f6a44","#9fe0b0")},
            {"lang",CD("lang",Elem.Kim,"狼","Vạn Ma Tông","Huyết Lang Trảo",null,0,0,"#e65a46","#2a2224","#c8c0b8")},
            {"thi",CD("thi",Elem.Thuy,"屍","Quỷ Dị Môn","Triệu Thi",null,0,1,"#78dc8c","#26302a","#8fe0a0")},
            {"bo",CD("bo",Elem.Tho,"補","Bổ Thiên Các","Bổ Thiên Trận",null,0,2,"#e8c870","#3a2e1e","#e8c870")},
            {"ta",CD("ta",Elem.Hoa,"邪","Tà Cực Tông","Tà Cực Ma Âm",null,0,3,"#e06ed2","#3a1838","#e080d0")},
            {"boss",CD("boss",Elem.Hoa,"煞","Trấn thủ · Kết Đan",null,null,0,-1,"#e5604d","#3a1214","#d9534f")},
        };

        public static readonly V2?[] WP = { null, new V2(860, 1750), null, new V2(860, 4150) };
        public static readonly Dictionary<string, int> SPLIT0 = new Dictionary<string, int> { { "hero", 3 }, { "viem", 0 }, { "thach", 1 }, { "nguyet", 1 }, { "thanh", 0 } };
        public static V2 Polar(float a, float r) { return new V2(C.x + M.Cos(a) * r, C.y + M.Sin(a) * r); }
    }
}
