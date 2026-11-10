// Phàm Nhân · bộ tạo âm thanh bằng mã (C# thuần, không phụ thuộc Unity).
// Mọi tiếng động và nhạc nền được tổng hợp lúc khởi động, không cần file âm thanh.
using System;
using System.Collections.Generic;

namespace PhamNhan.Audio
{
    public static class Synth
    {
        public const int SR = 44100;
        const double TAU = Math.PI * 2;

        // ---------- công cụ cơ bản ----------
        sealed class Noise
        {
            uint s;
            public Noise(uint seed) { s = seed == 0 ? 1u : seed; }
            public float Next() { s ^= s << 13; s ^= s >> 17; s ^= s << 5; return (s / 4294967295f) * 2f - 1f; }
        }

        /// <summary>Bộ lọc hai cực (dải thông / thông thấp / thông cao), kiểu biến trạng thái.</summary>
        sealed class Svf
        {
            float lp, bp;
            public float Low, Band, High;
            public void Run(float x, float freq, float q)
            {
                float f = (float)(2 * Math.Sin(Math.PI * Math.Min(freq, SR * 0.22) / SR));
                float damp = 1f / Math.Max(0.5f, q);
                High = x - lp - damp * bp; bp += f * High; lp += f * bp;
                Low = lp; Band = bp;
            }
        }

        static float[] Buf(float sec) { return new float[Math.Max(1, (int)(sec * SR))]; }
        static float Env(float t, float a, float d) { return t < a ? t / a : (float)Math.Exp(-(t - a) / d); }
        static float Note(int semitoneFromA4) { return 440f * (float)Math.Pow(2, semitoneFromA4 / 12.0); }

        /// <summary>Đàn gảy kiểu tranh (Karplus–Strong), cộng dồn vào bộ đệm tại vị trí start, có quấn vòng cho nhạc lặp.</summary>
        static void Pluck(float[] dst, int start, float freq, float amp, float decay, uint seed, bool wrap, float bright = 0.5f)
        {
            int n = Math.Max(2, (int)(SR / freq));
            var ring = new float[n]; var nz = new Noise(seed);
            for (int i = 0; i < n; i++) ring[i] = nz.Next();
            float prev = 0;
            for (int i = 0; i < n; i++) { prev = prev + (ring[i] - prev) * (0.3f + bright * 0.6f); ring[i] = prev; }
            int len = (int)(SR * decay * 4.2f); int idx = 0;
            float fb = (float)Math.Pow(0.001, 1.0 / (decay * freq)); // về -60 dB sau decay giây
            for (int i = 0; i < len; i++)
            {
                int j = start + i; if (wrap) j %= dst.Length; else if (j >= dst.Length) break;
                float a = ring[idx], b = ring[(idx + 1) % n];
                float v = (a + b) * 0.5f * fb; ring[idx] = v; idx = (idx + 1) % n;
                float att = i < 40 ? i / 40f : 1f;
                dst[j] += a * amp * att;
            }
        }

        static void Sine(float[] dst, int start, float f0, float f1, float dur, float amp, float att, float dec, bool wrap, float vib = 0, float vibF = 5)
        {
            int len = (int)(dur * SR); double ph = 0;
            for (int i = 0; i < len; i++)
            {
                int j = start + i; if (wrap) j %= dst.Length; else if (j >= dst.Length) break;
                float t = (float)i / SR, k = (float)i / len;
                float f = f0 * (float)Math.Pow(f1 / f0, k) * (1 + vib * (float)Math.Sin(TAU * vibF * t));
                ph += TAU * f / SR;
                float e = Env(t, att, dec) * (k > 0.92f ? (1 - k) / 0.08f : 1);
                dst[j] += (float)Math.Sin(ph) * amp * e;
            }
        }

        static void NoiseBand(float[] dst, int start, float dur, float f0, float f1, float q, float amp, float att, float dec, uint seed, bool wrap = false, int mode = 1)
        {
            int len = (int)(dur * SR); var nz = new Noise(seed); var sv = new Svf();
            for (int i = 0; i < len; i++)
            {
                int j = start + i; if (wrap) j %= dst.Length; else if (j >= dst.Length) break;
                float t = (float)i / SR, k = (float)i / len;
                sv.Run(nz.Next(), f0 * (float)Math.Pow(f1 / f0, k), q);
                float v = mode == 0 ? sv.Low : mode == 1 ? sv.Band : sv.High;
                float e = Env(t, att, dec) * (k > 0.9f ? (1 - k) / 0.1f : 1);
                dst[j] += v * amp * e;
            }
        }

        /// <summary>Trống lớn kiểu taiko: sóng sin hạ cao độ và tiếng da trống.</summary>
        static void Drum(float[] dst, int start, float amp, float f0, float f1, float dur, uint seed, bool wrap)
        {
            Sine(dst, start, f0, f1, dur, amp, 0.002f, dur * 0.35f, wrap);
            NoiseBand(dst, start, Math.Min(0.12f, dur), 900, 300, 1.2f, amp * 0.5f, 0.001f, 0.03f, seed, wrap, 0);
        }

        /// <summary>Chiêng, chuông: các họa âm không điều hòa tắt dần.</summary>
        static void Gong(float[] dst, int start, float f, float amp, float dur, bool wrap, float[] ratios = null)
        {
            ratios = ratios ?? new[] { 1f, 1.48f, 2.03f, 2.74f, 3.51f, 4.3f };
            for (int k = 0; k < ratios.Length; k++)
                Sine(dst, start, f * ratios[k], f * ratios[k] * 0.996f, dur, amp / (1 + k * 0.8f), 0.004f, dur * 0.32f / (1 + k * 0.35f), wrap, 0.002f, 3 + k);
        }

        static void Flute(float[] dst, int start, float f, float dur, float amp, bool wrap, uint seed)
        {
            Sine(dst, start, f, f, dur, amp, 0.08f, dur * 0.9f, wrap, 0.006f, 5.2f);
            Sine(dst, start, f * 2, f * 2, dur, amp * 0.18f, 0.1f, dur * 0.6f, wrap, 0.006f, 5.2f);
            NoiseBand(dst, start, dur, f * 2, f * 2, 6, amp * 0.25f, 0.06f, dur * 0.5f, seed, wrap);
        }

        static float[] Norm(float[] b, float peak = 0.85f)
        {
            float m = 1e-6f; foreach (var v in b) m = Math.Max(m, Math.Abs(v));
            float g = peak / m; for (int i = 0; i < b.Length; i++) b[i] = (float)Math.Tanh(b[i] * g * 1.1f) * 0.95f;
            return b;
        }

        /// <summary>Vang giả bằng vài đường trễ, cho tiếng chiêu có độ rộng.</summary>
        static float[] Echo(float[] b, float mix, float[] delays, bool wrap)
        {
            var o = (float[])b.Clone();
            foreach (var d in delays)
            {
                int D = (int)(d * SR); float g = mix * (float)Math.Exp(-d * 2.2);
                for (int i = 0; i < b.Length; i++) { int j = i + D; if (wrap) j %= b.Length; else if (j >= b.Length) break; o[j] += b[i] * g; }
            }
            return o;
        }

        // ---------- âm ngũ cung: Rê, Fa, Sol, La, Đô ----------
        static readonly int[] PENTA = { -7, -4, -2, 0, 3 }; // so với La 440: D4 F4 G4 A4 C5
        static float Pen(int degree, int octave = 0)
        {
            int o = (int)Math.Floor(degree / 5.0); int d = degree - o * 5;
            return Note(PENTA[d] + 12 * (o + octave));
        }

        // =============== tiếng động ===============
        public static Dictionary<string, float[]> MakeSfx()
        {
            var S = new Dictionary<string, float[]>();
            float[] b;

            b = Buf(0.09f); Sine(b, 0, 880, 1320, 0.09f, 0.6f, 0.002f, 0.03f, false); S["tap"] = Norm(b, 0.5f);
            b = Buf(0.22f); NoiseBand(b, 0, 0.22f, 600, 3800, 1.4f, 1, 0.01f, 0.07f, 3); Sine(b, 0, 420, 900, 0.12f, 0.15f, 0.002f, 0.04f, false); S["slash"] = Norm(b, 0.7f);
            b = Buf(0.12f); NoiseBand(b, 0, 0.1f, 2200, 900, 1.5f, 1, 0.001f, 0.025f, 5); Sine(b, 0, 170, 70, 0.1f, 0.7f, 0.001f, 0.03f, false); S["hit"] = Norm(b, 0.55f);
            b = Buf(0.22f); NoiseBand(b, 0, 0.14f, 3000, 1200, 1.5f, 1, 0.001f, 0.03f, 6); Sine(b, 0, 220, 80, 0.16f, 0.8f, 0.001f, 0.05f, false); Gong(b, 0, 1400, 0.25f, 0.2f, false); S["crit"] = Norm(b, 0.7f);
            b = Buf(0.1f); NoiseBand(b, 0, 0.08f, 1500, 700, 1.2f, 1, 0.001f, 0.02f, 7); Sine(b, 0, 140, 60, 0.08f, 0.5f, 0.001f, 0.025f, false); S["hitC"] = Norm(b, 0.4f);
            b = Buf(0.5f); for (int k = 0; k < 3; k++) { Sine(b, (int)(k * 0.06f * SR), 700, 2400, 0.3f, 0.35f, 0.01f, 0.12f, false); NoiseBand(b, (int)(k * 0.06f * SR), 0.25f, 2000, 7000, 3, 0.4f, 0.01f, 0.08f, (uint)(11 + k)); } S["yu"] = Norm(Echo(b, 0.3f, new[] { 0.07f, 0.13f }, false), 0.6f);
            b = Buf(0.3f); NoiseBand(b, 0, 0.3f, 800, 2500, 2, 0.6f, 0.05f, 0.1f, 13); Sine(b, 0, 300, 900, 0.3f, 0.3f, 0.05f, 0.1f, false); S["loiCast"] = Norm(b, 0.45f);
            b = Buf(1.1f); NoiseBand(b, 0, 0.12f, 6000, 2500, 0.8f, 1, 0.0005f, 0.03f, 17, false, 2); NoiseBand(b, 0, 1.1f, 260, 70, 0.7f, 1.2f, 0.01f, 0.35f, 19, false, 0); Sine(b, 0, 90, 40, 0.8f, 0.6f, 0.002f, 0.25f, false); S["loi"] = Norm(b, 0.9f);
            b = Buf(0.25f); Sine(b, 0, 220, 180, 0.25f, 0.4f, 0.01f, 0.08f, false); NoiseBand(b, 0, 0.2f, 500, 300, 2, 0.3f, 0.01f, 0.06f, 21); S["anCast"] = Norm(b, 0.45f);
            b = Buf(0.8f); Drum(b, 0, 1, 110, 38, 0.7f, 23, false); NoiseBand(b, 0, 0.8f, 400, 90, 0.8f, 0.8f, 0.005f, 0.22f, 25, false, 0); S["an"] = Norm(b, 0.9f);
            b = Buf(1.4f); for (int k = 0; k < 9; k++) Gong(b, (int)(k * 0.09f * SR), Pen(k + 3, 1), 0.35f, 0.9f, false, new[] { 1f, 2.76f, 5.4f }); NoiseBand(b, 0, 1.4f, 3000, 8000, 2, 0.15f, 0.2f, 0.5f, 27); S["van"] = Norm(Echo(b, 0.35f, new[] { 0.11f, 0.23f, 0.37f }, false), 0.75f);
            b = Buf(1.0f); Sine(b, 0, 300, 1200, 0.9f, 0.5f, 0.05f, 0.4f, false, 0.01f, 7); Gong(b, (int)(0.15f * SR), 1568, 0.3f, 0.8f, false, new[] { 1f, 2.4f }); S["yan"] = Norm(Echo(b, 0.3f, new[] { 0.09f, 0.19f }, false), 0.6f);
            b = Buf(0.2f); NoiseBand(b, 0, 0.2f, 1500, 5000, 1.2f, 1, 0.005f, 0.05f, 29); S["don"] = Norm(b, 0.55f);
            b = Buf(0.5f); for (int k = 0; k < 5; k++) Sine(b, (int)(k * 0.07f * SR), 500 + k * 160, 900 + k * 200, 0.08f, 0.4f, 0.005f, 0.03f, false); Gong(b, (int)(0.3f * SR), 1320, 0.3f, 0.2f, false, new[] { 1f, 2f }); S["dan"] = Norm(b, 0.55f);
            b = Buf(1.0f); foreach (var d in new[] { 0, 2, 4 }) Sine(b, 0, Pen(d, -1), Pen(d, -1), 1.0f, 0.3f, 0.25f, 0.5f, false); Gong(b, 0, 196, 0.25f, 1f, false); S["ho"] = Norm(b, 0.6f);
            b = Buf(0.3f); NoiseBand(b, 0, 0.25f, 1200, 3000, 2.5f, 0.8f, 0.01f, 0.07f, 31); S["phaCast"] = Norm(b, 0.45f);
            b = Buf(0.6f); NoiseBand(b, 0, 0.4f, 4000, 1500, 1, 0.8f, 0.001f, 0.08f, 33); Gong(b, 0, 880, 0.5f, 0.5f, false); Sine(b, 0, 160, 60, 0.3f, 0.5f, 0.001f, 0.1f, false); S["pha"] = Norm(b, 0.75f);
            b = Buf(2.0f); Gong(b, 0, 98, 0.8f, 2f, false); Gong(b, (int)(0.25f * SR), 1175, 0.25f, 1.2f, false, new[] { 1f, 2.5f, 4.1f }); S["reveal"] = Norm(Echo(b, 0.3f, new[] { 0.15f, 0.31f }, false), 0.8f);
            b = Buf(0.6f); Gong(b, 0, 2093, 0.5f, 0.5f, false, new[] { 1f, 1.33f, 2.9f, 4.4f }); NoiseBand(b, 0, 0.12f, 6000, 3000, 1, 0.6f, 0.001f, 0.03f, 35, false, 2); S["seal"] = Norm(b, 0.7f);
            b = Buf(0.25f); Sine(b, 0, 300, 250, 0.2f, 0.4f, 0.002f, 0.06f, false); NoiseBand(b, 0, 0.1f, 900, 900, 4, 0.2f, 0.001f, 0.03f, 37); S["vain"] = Norm(b, 0.35f);
            b = Buf(0.14f); Sine(b, 0, 600, 420, 0.12f, 0.5f, 0.001f, 0.04f, false); NoiseBand(b, 0, 0.08f, 2500, 1500, 2, 0.3f, 0.001f, 0.02f, 39); S["eyeHit"] = Norm(b, 0.4f);
            b = Buf(2.4f); Drum(b, 0, 1, 80, 28, 1.8f, 41, false); NoiseBand(b, 0, 2.2f, 1200, 60, 0.6f, 1, 0.003f, 0.6f, 43, false, 0); NoiseBand(b, 0, 0.6f, 7000, 2000, 1, 0.4f, 0.001f, 0.15f, 45, false, 2); Gong(b, (int)(0.1f * SR), 147, 0.4f, 2f, false); S["eyeDown"] = Norm(Echo(b, 0.25f, new[] { 0.2f, 0.41f }, false), 0.95f);
            b = Buf(1.6f); Gong(b, 0, 131, 0.8f, 1.6f, false); Drum(b, 0, 0.6f, 90, 40, 0.6f, 47, false); S["foeDown"] = Norm(b, 0.8f);
            b = Buf(1.2f); Sine(b, 0, Pen(3), Pen(1), 1.0f, 0.4f, 0.02f, 0.4f, false); Sine(b, (int)(0.3f * SR), Pen(1), Pen(-1), 0.8f, 0.4f, 0.02f, 0.3f, false); S["allyDown"] = Norm(b, 0.5f);
            b = Buf(1.4f); Sine(b, 0, Pen(2), Pen(0), 0.6f, 0.4f, 0.01f, 0.3f, false); Sine(b, (int)(0.35f * SR), Pen(0), Pen(-3), 1.0f, 0.4f, 0.01f, 0.4f, false); Drum(b, 0, 0.5f, 70, 35, 0.5f, 49, false); S["heroDown"] = Norm(b, 0.6f);
            b = Buf(1.2f); Gong(b, 0, 392, 0.5f, 1.2f, false); S["banner"] = Norm(b, 0.4f);
            b = Buf(0.5f); NoiseBand(b, 0, 0.45f, 350, 900, 2, 1, 0.04f, 0.12f, 51); Sine(b, 0, 110, 70, 0.45f, 0.4f, 0.03f, 0.15f, false); S["maCast"] = Norm(b, 0.45f);
            b = Buf(0.7f); Drum(b, 0, 0.9f, 120, 45, 0.5f, 53, false); NoiseBand(b, 0, 0.6f, 1800, 300, 1, 0.7f, 0.001f, 0.15f, 55); S["ma"] = Norm(b, 0.75f);
            b = Buf(0.6f); for (int i = 0; i < b.Length; i++) { float t = (float)i / SR; b[i] = (float)(Math.Sign(Math.Sin(TAU * 60 * t)) * 0.3 + Math.Sin(TAU * 1800 * t + 6 * Math.Sin(TAU * 31 * t)) * 0.4) * Env(t, 0.005f, 0.15f); } S["shock"] = Norm(b, 0.55f);
            b = Buf(1.0f); for (int i = 0; i < b.Length; i++) { float t = (float)i / SR; double f = 140 - 70 * t; b[i] = (float)(((t * f) % 1.0) * 2 - 1) * 0.5f * Env(t, 0.03f, 0.35f); } NoiseBand(b, 0, 1.0f, 600, 200, 1, 0.6f, 0.03f, 0.3f, 57, false, 0); S["bossRoar"] = Norm(b, 0.85f);
            b = Buf(0.45f); NoiseBand(b, 0, 0.4f, 300, 1200, 1.5f, 0.8f, 0.08f, 0.12f, 59); S["bossCast"] = Norm(b, 0.55f);
            b = Buf(1.0f); Drum(b, 0, 1, 95, 30, 0.8f, 61, false); NoiseBand(b, 0, 0.9f, 2500, 200, 0.8f, 0.9f, 0.001f, 0.25f, 63); S["tram"] = Norm(b, 0.9f);
            b = Buf(0.3f); NoiseBand(b, 0, 0.25f, 900, 2500, 1.5f, 1, 0.005f, 0.06f, 65); Sine(b, 0, 180, 80, 0.2f, 0.5f, 0.001f, 0.06f, false); S["bossSlash"] = Norm(b, 0.65f);
            b = Buf(0.8f); for (int k = 0; k < 3; k++) Sine(b, (int)(k * 0.1f * SR), 900 - k * 150, 600 - k * 100, 0.5f, 0.4f, 0.05f, 0.2f, false); NoiseBand(b, 0, 0.8f, 200, 600, 2, 0.5f, 0.1f, 0.3f, 67); S["khon"] = Norm(b, 0.55f);
            b = Buf(1.0f); foreach (var d in new[] { 0, 2, 4, 5 }) Sine(b, (int)(d * 0.05f * SR), Pen(d), Pen(d), 0.9f, 0.3f, 0.15f, 0.35f, false); S["heal"] = Norm(b, 0.5f);
            b = Buf(0.15f); Sine(b, 0, 1900, 1200, 0.1f, 0.3f, 0.001f, 0.03f, false); NoiseBand(b, 0, 0.09f, 3500, 1500, 1.5f, 0.6f, 0.001f, 0.025f, 69); S["fall"] = Norm(b, 0.45f);
            b = Buf(0.9f); NoiseBand(b, 0, 0.8f, 700, 2000, 0.8f, 1, 0.01f, 0.25f, 71); for (int k = 0; k < 14; k++) NoiseBand(b, (int)(k * 0.05f * SR), 0.03f, 5000, 3000, 2, 0.6f, 0.001f, 0.01f, (uint)(73 + k)); S["fire"] = Norm(b, 0.75f);
            b = Buf(0.9f); Gong(b, 0, 1760, 0.6f, 0.8f, false, new[] { 1f, 1.59f, 2.83f, 4.1f }); NoiseBand(b, 0, 0.3f, 7000, 4000, 2, 0.4f, 0.001f, 0.08f, 91, false, 2); S["ice"] = Norm(b, 0.7f);
            b = Buf(0.9f); for (int k = 0; k < 5; k++) Pluck(b, (int)(k * 0.07f * SR), 110 + k * 25, 0.6f, 0.25f, (uint)(93 + k), false, 0.2f); NoiseBand(b, 0, 0.6f, 400, 900, 2, 0.4f, 0.02f, 0.2f, 99); S["vine"] = Norm(b, 0.6f);
            // khúc thắng, khúc thua
            b = Buf(3.2f); int[] up = { 0, 1, 2, 3, 4, 5, 7 }; for (int k = 0; k < up.Length; k++) Pluck(b, (int)(k * 0.13f * SR), Pen(up[k]), 0.8f, 1.2f, (uint)(101 + k), false, 0.6f);
            Gong(b, (int)(0.95f * SR), 196, 0.9f, 2.2f, false); Drum(b, (int)(0.95f * SR), 1, 90, 35, 0.8f, 111, false); Flute(b, (int)(1.0f * SR), Pen(7), 1.8f, 0.4f, false, 113); S["win"] = Norm(Echo(b, 0.3f, new[] { 0.17f, 0.33f }, false), 0.85f);
            b = Buf(3.0f); int[] dn = { 4, 3, 2, 0, -1 }; for (int k = 0; k < dn.Length; k++) Pluck(b, (int)(k * 0.28f * SR), Pen(dn[k], -1), 0.8f, 1.4f, (uint)(121 + k), false, 0.3f); Gong(b, (int)(1.4f * SR), 110, 0.7f, 1.6f, false); S["lose"] = Norm(Echo(b, 0.3f, new[] { 0.2f, 0.4f }, false), 0.75f);
            return S;
        }

        // =============== nhạc nền ===============
        /// <summary>Nhạc màn hình chính: đàn tranh rải ngũ cung, sáo trúc, nền ngân, nhịp chậm. Lặp liền mạch.</summary>
        public static float[] MusicMenu()
        {
            const float bpm = 72; float beat = 60f / bpm; int bars = 8; var b = Buf(bars * 4 * beat);
            int[][] chords = { new[] { 0, 2, 4 }, new[] { -2, 0, 3 }, new[] { 1, 3, 5 }, new[] { -1, 1, 4 } };
            for (int bar = 0; bar < bars; bar++)
            {
                var ch = chords[bar % 4];
                // nền ngân: hai nốt trầm
                int s0 = (int)(bar * 4 * beat * SR);
                Sine(b, s0, Pen(ch[0], -2), Pen(ch[0], -2), 4 * beat + 0.6f, 0.16f, 1.2f, 3.5f, true, 0.003f, 0.3f);
                Sine(b, s0, Pen(ch[2], -2), Pen(ch[2], -2), 4 * beat + 0.6f, 0.08f, 1.5f, 3.5f, true, 0.003f, 0.23f);
                // đàn tranh rải
                int[] pat = { 0, 1, 2, 1, 2, 0, 2, 1 };
                for (int k = 0; k < 8; k++)
                {
                    int deg = ch[pat[k]] + (k >= 4 ? 5 : 0);
                    Pluck(b, s0 + (int)(k * beat * 0.5f * SR), Pen(deg), 0.32f, 1.6f, (uint)(bar * 31 + k * 7 + 1), true, 0.45f);
                }
            }
            // sáo: câu nhạc ở nửa sau
            int[] mel = { 7, 6, 5, 7, 8, 7, 5, 4, 5, 6, 5, 3, 2, 3, 4, 2 };
            float[] dur = { 1, 0.5f, 0.5f, 2, 1, 1, 1.5f, 0.5f, 1, 0.5f, 0.5f, 2, 1, 1, 1, 1 };
            float tb = bars * 4 * beat * 0.5f;
            for (int i = 0; i < mel.Length; i++) { Flute(b, (int)(tb * SR), Pen(mel[i]), dur[i] * beat * 1.05f, 0.16f, true, (uint)(500 + i)); tb += dur[i] * beat; }
            return Norm(Echo(b, 0.35f, new[] { 0.21f, 0.43f, 0.67f }, true), 0.6f);
        }

        /// <summary>Nhạc trận: trống taiko, đàn tranh gảy dồn, nền trầm, kèn sáo dẫn giai điệu. Mức "intense" thêm trống và nhịp dồn.</summary>
        public static float[] MusicBattle(bool intense)
        {
            float bpm = intense ? 120 : 104; float beat = 60f / bpm; int bars = 16; var b = Buf(bars * 4 * beat);
            int[] roots = { 0, 0, -2, -1, 0, 0, 1, -2, 0, 2, -2, -1, 0, 1, -1, -2 };
            uint sd = intense ? 2000u : 1000u;
            for (int bar = 0; bar < bars; bar++)
            {
                int s0 = (int)(bar * 4 * beat * SR); int r = roots[bar];
                Sine(b, s0, Pen(r, -2), Pen(r, -2), 4 * beat + 0.3f, 0.22f, 0.2f, 2.5f, true);
                Sine(b, s0, Pen(r + 2, -2) , Pen(r + 2, -2), 4 * beat + 0.3f, 0.1f, 0.3f, 2.5f, true);
                // trống
                float[] hits = intense ? new[] { 0f, 0.75f, 1.5f, 2f, 2.5f, 3f, 3.5f } : new[] { 0f, 1.5f, 2f, 3f };
                foreach (var h in hits) Drum(b, s0 + (int)(h * beat * SR), h == 0 || h == 2 ? 0.9f : 0.55f, 105, 42, 0.45f, sd++, true);
                // trống nhỏ, gõ phách
                for (int k = 0; k < 8; k++) if (k % 2 == 1 || intense) NoiseBand(b, s0 + (int)(k * beat * 0.5f * SR), 0.06f, 3500, 2500, 2, k % 4 == 3 ? 0.3f : 0.15f, 0.001f, 0.015f, sd++, true);
                // đàn tranh dồn
                int[] ost = { 0, 2, 3, 2, 5, 3, 2, 3 };
                for (int k = 0; k < 8; k++) Pluck(b, s0 + (int)(k * beat * 0.5f * SR), Pen(r + ost[k], -1), 0.28f, 0.6f, sd++, true, 0.55f);
            }
            // giai điệu
            int[] mel = intense ? new[] { 5, 7, 8, 7, 5, 4, 5, 7, 9, 8, 7, 5, 4, 3, 4, 5 } : new[] { 5, 4, 5, 7, 5, 4, 2, 3, 4, 5, 4, 2, 0, 2, 3, 2 };
            float tb = 0;
            for (int i = 0; i < mel.Length * 2; i++)
            {
                float d = new[] { 1.5f, 1.5f, 1f, 4f }[i % 4] * beat; // mỗi cụm 4 nốt = 2 ô nhịp
                int deg = mel[i % mel.Length] + (i >= mel.Length && i % 4 != 3 ? 2 : 0); // lượt hai lên cao hơn
                Flute(b, (int)(tb * SR), Pen(deg), d * 0.95f, 0.2f, true, (uint)(700 + i));
                tb += d;
            }
            return Norm(Echo(b, 0.25f, new[] { 0.19f, 0.39f }, true), 0.65f);
        }

        /// <summary>Ghi bộ đệm ra file WAV 16 bit (dùng để nghe thử ngoài Unity).</summary>
        public static byte[] Wav(float[] s)
        {
            int n = s.Length; var o = new byte[44 + n * 2];
            void W(int p, int v, int bytes) { for (int i = 0; i < bytes; i++) o[p + i] = (byte)(v >> (8 * i)); }
            System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(o, 0); W(4, 36 + n * 2, 4); System.Text.Encoding.ASCII.GetBytes("WAVEfmt ").CopyTo(o, 8);
            W(16, 16, 4); W(20, 1, 2); W(22, 1, 2); W(24, SR, 4); W(28, SR * 2, 4); W(32, 2, 2); W(34, 16, 2);
            System.Text.Encoding.ASCII.GetBytes("data").CopyTo(o, 36); W(40, n * 2, 4);
            for (int i = 0; i < n; i++) { int v = (int)(Math.Max(-1, Math.Min(1, s[i])) * 32767); W(44 + i * 2, v, 2); }
            return o;
        }
    }
}
