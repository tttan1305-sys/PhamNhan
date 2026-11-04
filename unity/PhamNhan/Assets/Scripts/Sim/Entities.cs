using System.Collections.Generic;

namespace PhamNhan.Sim
{
    /// <summary>Thứ có thể bị đánh: quân lính, tướng, trận nhãn.</summary>
    public class Body
    {
        public int id;
        public float x, y, r, hp, max;
        public int team;
        public bool dead;
        public float flash;
        public virtual bool IsEye { get { return false; } }
        public V2 P { get { return new V2(x, y); } }
    }

    public enum Role { None, Ret, Guard, Van, Amb, Def, Pat, Boss }

    public class Unit : Body
    {
        public string t; public UType T;
        public float cd, face, anim, stun, retgt, ox, oy, pr, steer, rise, tmpT = -1;
        public Body tgt; public Unit lead, tmpL;
        public Role role; public V2 home; public bool hasHome;
        public bool moving;
        // tướng
        public bool champ; public string key; public ChampDef D;
        public float skT, yanT, healT, sumT, respawn, hitT = -9; public bool retreat, reinf;
        public int asg = -1; public int want = -1;  // asg: chỉ số hướng, BREACH = giữ cửa vỡ
        public const int BREACH = -2;
        // anh hùng
        public float mp, mpMax, st, cdS, invul, flyT, swing, swingDir = 1, blkT = -9;
        public float cdYu, cdLoi, cdVan, cdDon, cdYan;
        public V2 dashDir; public float dashT;
        // trấn thủ
        public bool active; public float cdT, cdK, casting;
    }

    public class Eye : Body
    {
        public int i; public Elem el, weak; public float a;
        public bool revealed, sensed; public int seals, maxSeals, reforms; public float vuln, lastHit = -9, shock = 4, vainT = -9; public int lastCast = -1;
        public string name, place, toolT; public float hx, hy, va;
        public List<V2> vein = new List<V2>();
        public override bool IsEye { get { return true; } }
    }

    public enum PKind { Arrow, Bolt, Sword, Pha }
    public class Proj
    {
        public PKind kind; public float x, y, tx, ty, spd, dmg, life, a, vx, vy, delay, fade = 0.3f, t, max, sx, sy;
        public Body tgt; public Unit src; public int team, cast; public bool dead, ob, big; public Rgb col; public bool hasCol;
        public List<V2> trail = new List<V2>();
        public Zone zoneTgt;
    }

    public class Tele
    {
        public float x, y, r, t, max, dmg, stun; public int team, cast; public string kind; public Elem elem = Elem.None; public Unit src; public Rgb col; public string g; public bool done;
    }

    public class Zone { public string type; public float x, y, r, t, max, tick; }
    public class Wave { public float x, y, r, max; public bool found; public Unit by; }

    public enum FxT { P, Ring, Glyph, Slash, SlashC, SlashR, Ghost, Trail, Bolt, Fall, Ice, Vine }
    public class Fx
    {
        public FxT type; public float x, y, t, max, vx, vy, grav, s, r0, r1, a, dir, r; public Rgb col; public string g; public bool hit; public V2[] pts;
    }
    public class FloatTxt { public float x, y, t, max, vy, size; public string s; public Rgb col; }
    public class Decal { public float x, y, r, t; public string type; public int seed; }
    public class Ripple { public float a, t; public bool big; }
    public class Banner { public float t, max; public string title, sub, g; }
    public class Strat { public float t, max; public string g, title, sub; public int side; }
    public class Plan { public string kind; public int tgt; public float t; public string why, g, title, sub; }
    public class FeedItem { public string s; public float t; }
    public struct SndEv { public string id; public float x, y; }
    public class Building { public float x, y, rot, w, h, c, s, hw, hh; public string kind; public int v; }
}
