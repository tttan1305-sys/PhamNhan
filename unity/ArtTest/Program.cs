using System; using System.IO; using System.IO.Compression; using System.Diagnostics; using PhamNhan.Art;
// Vẽ bảng tạo hình ra PNG để xem thử: dotnet run --project unity/ArtTest -- out.png [px]
static class P {
  static void Main(string[] a) {
    string outp = a.Length > 0 ? a[0] : "sheet.png"; int px = a.Length > 1 ? int.Parse(a[1]) : 160;
    var keys = Chars.Keys; int cols = Chars.FRAMES + 1, small = px * 3 / 8;
    int W = cols * px, Hh = keys.Count * px; var img = new byte[W * Hh * 4];
    bool tr = a.Length > 2; if (!tr) for (int i = 0; i < img.Length; i += 4) { img[i] = 143; img[i+1] = 208; img[i+2] = 98; img[i+3] = 255; }
    var sw = Stopwatch.StartNew();
    for (int k = 0; k < keys.Count; k++) {
      for (int f = 0; f < Chars.FRAMES; f++) Blit(img, W, Chars.Render(keys[k], f, px), f * px, k * px, tr);
      var c = Chars.Render(keys[k], 0, small); Blit(img, W, c, Chars.FRAMES * px + 4, k * px + 4);
      var c2 = Chars.Render(keys[k], 1, small); Blit(img, W, c2, Chars.FRAMES * px + 4 + small, k * px + 4);
    }
    Console.WriteLine("ve xong " + keys.Count + " loai trong " + sw.ElapsedMilliseconds + " ms");
    Png(outp, img, W, Hh);
  }
  static void Blit(byte[] img, int W, Canvas c, int ox, int oy, bool copy = false) {
    var b = c.Bytes(false);
    if (copy) { for (int j = 0; j < c.H; j++) Array.Copy(b, j * c.W * 4, img, ((oy + j) * W + ox) * 4, c.W * 4); return; }
    for (int j = 0; j < c.H; j++) for (int i = 0; i < c.W; i++) {
      int s = (j * c.W + i) * 4, d = ((oy + j) * W + ox + i) * 4; float al = b[s+3] / 255f;
      for (int q = 0; q < 3; q++) img[d+q] = (byte)(img[d+q] * (1 - al) + b[s+q] * al);
    }
  }
  static void Png(string path, byte[] rgba, int w, int h) {
    using var fs = File.Create(path);
    fs.Write(new byte[]{137,80,78,71,13,10,26,10});
    var ihdr = new byte[13]; BE(ihdr,0,w); BE(ihdr,4,h); ihdr[8]=8; ihdr[9]=6; Chunk(fs,"IHDR",ihdr);
    var ms = new MemoryStream(); using (var z = new ZLibStream(ms, CompressionLevel.Optimal, true)) {
      for (int j = 0; j < h; j++) { z.WriteByte(0); z.Write(rgba, j * w * 4, w * 4); } }
    Chunk(fs,"IDAT",ms.ToArray()); Chunk(fs,"IEND",new byte[0]);
  }
  static void BE(byte[] b,int o,int v){b[o]=(byte)(v>>24);b[o+1]=(byte)(v>>16);b[o+2]=(byte)(v>>8);b[o+3]=(byte)v;}
  static void Chunk(Stream s,string t,byte[] d){var l=new byte[4];BE(l,0,d.Length);s.Write(l);var td=new byte[4+d.Length];for(int i=0;i<4;i++)td[i]=(byte)t[i];Array.Copy(d,0,td,4,d.Length);s.Write(td);var c=new byte[4];BE(c,0,(int)Crc(td));s.Write(c);}
  static uint Crc(byte[] d){uint c=0xffffffff;foreach(var b in d){c^=b;for(int k=0;k<8;k++)c=(c&1)!=0?0xedb88320^(c>>1):c>>1;}return ~c;}
}
