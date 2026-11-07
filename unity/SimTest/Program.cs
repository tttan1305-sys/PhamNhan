using System; using PhamNhan.Sim; using System.Linq;
class P{static void Main(string[] a){
 var g=new Siege(true); float dt=1f/60; var sw=System.Diagnostics.Stopwatch.StartNew();
 int secs=a.Length>0?int.Parse(a[0]):900;
 for(int i=0;i<secs*60;i++){g.Update(dt);
  if(i%1800==0)Console.WriteLine($"t={g.t:F0} ph={g.phase} kill={g.killed} en={g.EnemyCount()} al={g.AllyCount()} hp={g.hero.hp:F0} eyes={string.Join(" ",g.eyes.Select(e=>e.dead?"X":e.revealed?e.seals+"/"+(int)e.hp:"?"))} boss={(g.boss.dead?"dead":g.boss.active?"act":"-")} flag={g.flag:F0} over={g.over} hero=({g.hero.x:F0},{g.hero.y:F0}) plan={g.plan?.kind}{g.plan?.tgt}");
  if(g.over!=null&&g.overT>1)break;}
 Console.WriteLine($"END t={g.t:F0} over={g.over} ms={sw.ElapsedMilliseconds}");
 foreach(var f in g.log.TakeLast(12))Console.WriteLine($"  {f.t:F0} {f.s}");
}}
