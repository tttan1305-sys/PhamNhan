using System; using PhamNhan.Sim; using System.Linq;
class P{static void Main(string[] a){
 if(a.Length>0&&a[0]=="json"){var w=new World(); Console.WriteLine("["+string.Join(",",w.nodes.Select(n=>$"{{\"n\":\"{n.name}\",\"t\":{(int)n.tier},\"x\":{n.x:F1},\"y\":{n.y:F1},\"o\":{n.owner},\"a\":[{string.Join(",",n.adj)}]}}"))+"]"); return;}
 if(a.Length>0&&a[0]=="world"){var w=new World(); Console.WriteLine($"nodes={w.nodes.Count} edges={w.edges.Count} chinh={w.Count(0)} ma={w.Count(1)} neutral={w.Count(2)}");
  foreach(var n in w.nodes) if(n.tier!=Tier.Town) Console.WriteLine($" {n.id} {n.name} {n.tier} owner={n.owner} ({n.x:F0},{n.y:F0}) adj={n.adj.Count}");
  int W=0,L=0,O=0; for(int sd=1;sd<=200;sd++){var ww=new World(); var rr=new Random(sd); int dd=0; while(ww.over==null&&dd<150){var c=ww.nodes.Where(n=>ww.CanAttack(n.id)).ToList(); if(c.Count==0)break; var t=c.OrderBy(n=>M.Dist(n.x,n.y,World.CX,World.CY)+rr.Next(120)).First(); ww.Resolve(t.id, rr.NextDouble()<(t.tier==Tier.Holy?0.65:0.85)); dd++;} if(ww.over=="win")W++; else if(ww.over=="lose")L++; else O++; if(sd==1)Console.WriteLine("mau: "+dd+" tran "+ww.over);} Console.WriteLine($"thang={W} thua={L} chua xong={O}");
  var r=new Random(1); int d=0; while(w.over==null&&d<200){var c=w.nodes.Where(n=>w.CanAttack(n.id)).ToList(); if(c.Count==0)break; var t=c.OrderBy(n=>(int)n.tier).First(); w.Resolve(t.id, r.NextDouble()<0.7); d++;}
  Console.WriteLine($"sau {d} tran: day={w.day} over={w.over} chinh={w.Count(0)} ma={w.Count(1)} holyC={w.Holy(0)} holyM={w.Holy(1)}"); foreach(var l in w.log.Take(6))Console.WriteLine(" "+l);
  var w2=new World(); Console.WriteLine("load="+w2.Load(w.Save())+" same="+(w2.Save()==w.Save())); return;}
 BattleCfg cfg=null; if(a.Length>1){var w=new World(); var tier=(Tier)int.Parse(a[1]); var n=w.nodes.First(x=>x.tier==tier && (a.Length<3 || x.owner==int.Parse(a[2]))); cfg=BattleCfg.For(n); Console.WriteLine("cu diem "+n.name+" "+World.TIER_N[(int)tier]+" owner="+n.owner);}
 var g=new Siege(true,cfg); Console.WriteLine("dich="+g.EnemyCount()); float dt=1f/60; var sw=System.Diagnostics.Stopwatch.StartNew();
 int secs=a.Length>0?int.Parse(a[0]):900;
 for(int i=0;i<secs*60;i++){g.Update(dt);
  if(i%1800==0)Console.WriteLine($"t={g.t:F0} ph={g.phase} kill={g.killed} en={g.EnemyCount()} al={g.AllyCount()} hp={g.hero.hp:F0} eyes={string.Join(" ",g.eyes.Select(e=>e.dead?"X":e.revealed?e.seals+"/"+(int)e.hp:"?"))} boss={(g.boss.dead?"dead":g.boss.active?"act":"-")} flag={g.flag:F0} over={g.over} hero=({g.hero.x:F0},{g.hero.y:F0}) plan={g.plan?.kind}{g.plan?.tgt}");
  if(g.over!=null&&g.overT>1)break;}
 Console.WriteLine($"END t={g.t:F0} over={g.over} ms={sw.ElapsedMilliseconds}");
 foreach(var f in g.log.TakeLast(12))Console.WriteLine($"  {f.t:F0} {f.s}");
}}
