/* ================= trạng thái ================= */
let G,hero,boss,vign;
const IN={mx:0,my:0,slash:false};
function addUnit(t,x,y,extra){const T=TYPES[t];const u=Object.assign({id:G.nid++,t,team:T.team,x,y,hp:T.hp,max:T.hp,cd:rnd()*(T.cd||1),face:T.team?Math.PI:0,anim:0,stun:0,flash:0,tgt:null,retgt:rnd()*0.3,r:T.r,dead:false,ox:RR(-70,70),oy:RR(-70,70),pr:0,steer:rnd()<0.5?1:-1},extra||{});u.pr=dist(u,C);G.units.push(u);return u;}
function makeVein(e,off){const a2=e.a+deg(off);const P0={x:e.x,y:e.y},P2=polar(a2,BAR_R-6);const mx=(P0.x+P2.x)/2,my=(P0.y+P2.y)/2;const dx=P2.x-P0.x,dy=P2.y-P0.y,L=Math.hypot(dx,dy)||1;const bend=(off>0?1:-1)*Math.min(140,L*0.35);const P1={x:mx-dy/L*bend,y:my+dx/L*bend};const pts=[];const N=36;for(let k=0;k<=N;k++){const t=k/N;const x=(1-t)*(1-t)*P0.x+2*(1-t)*t*P1.x+t*t*P2.x,y=(1-t)*(1-t)*P0.y+2*(1-t)*t*P1.y+t*t*P2.y;const j=(k===0||k===N)?0:7;pts.push([x+RR(-j,j),y+RR(-j,j)]);}e.va=a2;return pts;}
function secOf(a){let b=0,bd=9;for(let d=0;d<4;d++){const q=angDiff(a,DIRS[d].a);if(q<bd){bd=q;b=d;}}return b;}
function secOpenA(a){return !G.barrier||G.sec[secOf(a)];}
function angC(p){return Math.atan2(p.y-C.y,p.x-C.x);}
function postPt(d,r){const ea=deg(EYE_DEF[d].a);return polar(ea+angSigned(ea,DIRS[d].a)*0.45,r||2060);}
function openPt(r){return polar(DIRS[G.open<0?0:G.open].a,r);}
function srcName(s){return s===hero?'Ngao Thiên':(s&&TYPES[s.t]?TYPES[s.t].name:'Quân ta');}
function nameOf(u){return TYPES[u.t].name;}
function initChamp(u,key){u.champ=true;u.key=key;u.D=CH_DEF[key];u.skT=RR(1.5,3.5);u.yanT=0;u.healT=4;u.sumT=8;u.respawn=0;u.retreat=false;u.hitT=-9;u.asg=0;}
function squad(lead,list,x0,y0){let k=0;for(const [t,n] of list)for(let i=0;i<n;i++,k++){addUnit(t,x0-(k%12)*24-(Math.floor(k/12)%2)*12,y0+Math.floor(k/12)*26,{lead});}}
function newGame(){
 seed=20261102;
 G={t:0,time:TIME,phase:1,barrier:true,sec:[false,false,false,false],open:-1,units:[],eyes:[],proj:[],fx:[],decals:[],floats:[],zones:[],teles:[],ripples:[],shards:[],waves:[],feed:[],banner:null,warn:null,warnT:0,shake:0,flash:0,flashCol:'255,255,255',killed:0,totalEnemy:0,lost:0,flag:0,flagNeed:15,contested:false,over:null,overT:0,mode:'auto',plan:null,cmdT:3,mcmdT:2,strat:null,lastStrat:-99,stratOn:false,cam:{x:C.x-VW/Z0/2,y:C.y-VH/Z0/2},nid:1,lastHeroFeed:-9,intro:INTRO,zoom:Z0,pha:3,dan:3,loi:5,an:3,anT:0,loiT:0,hoCd:0,phaCd:0,danCd:0,anCd:0,breakT:-1,breakD:-1,flagOwner:1,heroKills:0,need:1,k0:0,castN:0,veinGlow:0,allies:[],foes:[],maDown:0,maFlow:false,log:[],dlast:[-1,-1,-1,-1]};
 EYE_DEF.forEach((d,i)=>{const a=deg(d.a),p=EYE_POS[i];const e={i,el:d.el,weak:d.weak,x:p.x,y:p.y,a,hp:d.hp,max:d.hp,r:30,team:1,isEye:true,revealed:false,dead:false,seals:d.seals,maxSeals:d.seals,reforms:0,vuln:0,flash:0,lastHit:-9,shock:4,lastCast:-1,name:ELEM[d.el].n,place:d.place,tool:d.tool,toolT:d.toolT,sensed:false,vainT:-9};e.vein=makeVein(e,d.voff);const ha=RR(0,TAU),hr=RR(60,150);e.hx=e.x+Math.cos(ha)*hr;e.hy=e.y+Math.sin(ha)*hr;G.eyes.push(e);});
 // ===== năm tướng Chính đạo, mỗi tướng dẫn một đạo quân =====
 const X=CAMP.x,Y=CAMP.y;
 hero=addUnit('hero',X+200,Y+350);hero.mp=420;hero.mpMax=420;hero.st=100;hero.cdS=0;hero.cd2={yu:0,loi:0,van:0,don:0,yan:0};hero.invul=0;hero.dash=null;hero.respawn=0;hero.flyT=0;hero.face=0.3;hero.champ=true;hero.key='hero';hero.D=CH_DEF.hero;hero.asg=3;hero.hitT=-9;
 const mk=(k,x,y)=>{const u=addUnit(k,x,y);initChamp(u,k);u.face=0;return u;};
 const thach=mk('thach',X+180,Y-580),nguyet=mk('nguyet',X+160,Y-500),viem=mk('viem',X+420,Y-90),thanh=mk('thanh',X+420,Y+60);
 G.allies=[hero,viem,thach,nguyet,thanh];
 squad(thach,[['long',10],['kiem',8],['phu',4]],X+130,Y-610);
 squad(nguyet,[['kiem',10],['phu',6],['long',4]],X+130,Y-530);
 squad(viem,[['kiem',10],['phu',8],['long',4],['quy',1]],X+380,Y-110);
 squad(hero,[['loan',6],['kiem',8],['long',4],['phu',4],['quy',1]],X+130,Y+390);
 squad(thanh,[['long',8],['loan',4],['kiem',8]],X+380,Y+10);
 for(const u of G.units)if(u.t==='loan'&&u.lead===hero){u.x+=60;}
 // ===== năm tướng Ma đạo: Trấn thủ ở nội thành, bốn Ma tướng giữ bốn hướng =====
 const mkM=k=>{const d=CH_DEF[k].post;const p=postPt(d);const u=addUnit(k,p.x,p.y);initChamp(u,k);u.asg=d;u.face=angC(p);u.home={x:p.x,y:p.y};return u;};
 const lang=mkM('lang'),thi=mkM('thi'),bo=mkM('bo'),ta=mkM('ta');
 for(const m of [lang,thi,bo,ta]){const d=m.D.post;
  for(let i=0;i<7;i++)addUnit(i<6?'no':'tuong',m.x+RR(-60,60),m.y+RR(-60,60),{role:'ret',lead:m});
  const ea=deg(EYE_DEF[d].a);
  for(let i=0;i<15;i++){const a=ea+deg(RR(-9,9)),rr=RR(2040,2390);addUnit(i<10?'no':i<14?'cung':'tuong',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'guard'});}}
 // tiền đồn Tây chắn trước doanh trại
 for(let i=0;i<16;i++){const a=deg(RR(175,187)),rr=RR(2280,2400);addUnit(i<10?'no':i<15?'cung':'tuong',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'van'});}
 // giữa kết giới và tường, đại lộ, ngã tư vành đai ở cả bốn cửa
 for(const g of G_OUT){
  for(let i=0;i<8;i++){const a=g+deg(RR(-14,14)),rr=RR(1695,1770);addUnit(i<5?'no':'cung',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'def'});}
  const px=-Math.sin(g),py=Math.cos(g);
  for(let i=0;i<5;i++){const p=polar(g,RR(1250,1580)),o=RR(-55,55);addUnit(i<4?'no':'tuong',p.x+px*o,p.y+py*o,{role:'def'});}
  for(let i=0;i<6;i++){const a=g+deg(RR(-3.5,3.5)),rr=RING_R+RR(-35,35);addUnit(i<4?'no':'cung',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'def'});}}
 // cổng nội thành và quảng trường
 for(const g of G_IN){const px=-Math.sin(g),py=Math.cos(g);for(let i=0;i<8;i++){const p=polar(g,RR(740,830)),o=RR(-55,55);addUnit(i<5?'no':i<7?'cung':'tuong',p.x+px*o,p.y+py*o,{role:'def'});}}
 for(let i=0;i<17;i++){const a=deg(RR(-25,205)),rr=RR(170,470);addUnit(i<14?'no':'tuong',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'def'});}
 // tuần tra vành đai
 for(const ad of [45,135,225,315]){for(let i=0;i<5;i++){const a=deg(ad+RR(-3,3)),rr=RING_R+RR(-30,30);addUnit('no',C.x+Math.cos(a)*rr,C.y+Math.sin(a)*rr,{role:'pat'});}const p=polar(deg(ad),RING_R);addUnit('tuong',p.x,p.y,{role:'pat'});}
 G.units.forEach(u=>{if(u.team===1&&!u.home)u.home={x:u.x,y:u.y};});
 boss=addUnit('boss',C.x-230,C.y,{role:'boss',active:false,cdT:3,cdK:7,casting:0,home:{x:C.x-230,y:C.y}});boss.key='boss';boss.D=CH_DEF.boss;
 G.foes=[boss,lang,thi,bo,ta];
 G.totalEnemy=enemyCount();
 G.startAlly=allyCount();
 pushFeed('Năm tướng Chính đạo áp sát Phù Vân Thành');
 showBanner('Công thành · Phù Vân Thành','Năm tướng mỗi phe · bốn trận nhãn ẩn nuôi bốn cung kết giới',8.4,'攻');
 $('end').style.display='none';$('end').style.opacity=0;$('boss').style.display='none';G.endShown=false;
 setMode('auto',true);planSet('split',null,'',true);
}
function allyCount(){let n=0;for(const u of G.units)if(!u.dead&&u.team===0&&!u.champ&&u.t!=='quy')n++;return n;}
function enemyCount(){let n=0;for(const u of G.units)if(!u.dead&&u.team===1&&u!==boss&&!u.champ)n++;return n;}

/* ================= tiện ích chiến đấu ================= */
// kết giới một chiều: chặn quân Chính đạo ở các cung còn trận nhãn, quân Ma đạo đi qua được
function reachable(u,e){
 if(!G.barrier||u.team===1)return true;
 if(e.isEye)return true;
 const ui=u.pr<BAR_R,ei=e.pr<BAR_R;if(ui===ei)return true;
 return secOpenA(angC(e))&&secOpenA(angC(u));
}
function barBlock(u,pt){return G.barrier&&u.team===0&&u.pr>=BAR_R&&Math.hypot(pt.x-C.x,pt.y-C.y)<BAR_R&&!secOpenA(angC(pt));}
function nearestEnemy(u,maxR,filter){let best=null,bd=maxR;for(const e of G.units){if(e.dead||e.team===u.team)continue;if(e===boss&&!boss.active&&u.team===0&&dist(u,e)>260)continue;if(filter&&!filter(e))continue;const d=dist(u,e)-e.r;if(d<bd){bd=d;best=e;}}return best;}
function liveEyes(){return G.eyes.filter(e=>!e.dead&&e.revealed);}
function vulnEye(u,r){for(const e of G.eyes)if(!e.dead&&e.revealed&&e.vuln>0&&dist(u,e)<r)return e;return null;}
function countEnemies(p,r,team){let n=0;for(const e of G.units){if(e.dead||e.team!==(team===undefined?1:team))continue;if(dist(p,e)<r)n++;}return n;}
function inZone(u,type){for(const z of G.zones){if(z.type===type&&dist(u,z)<z.r)return z;}return null;}
function pow(d,team,R){const e=G.eyes[d];let s=0;for(const u of G.units){if(u.dead||u.team!==team||u===boss)continue;if(team===0&&Math.abs(u.x-CAMP.x)<650&&Math.abs(u.y-CAMP.y)<650)continue;const dx=u.x-e.x,dy=u.y-e.y;if(dx*dx+dy*dy<R*R)s+=u.champ?14:u.t==='tuong'?3:u.t==='quy'?4:1;}return s;}
function regOf(r){return r>WALL_R?0:r>IN_R?1:2;}
function onAvenue(a,r){for(const g of G_OUT)if(angDiff(a,g)*r<AV_W-12)return g;return null;}
function ringRoute(au,ru,ad,dest){
 const dA=angDiff(au,ad);if(dA<0.26)return dest;
 if(Math.abs(ru-RING_R)>42){const g=onAvenue(au,ru);return polar(g!==null?g:au,RING_R);}
 const s=angSigned(au,ad);return polar(au+Math.sign(s)*Math.min(0.2,dA),RING_R);
}
function circDetour(u,dest,R,ru,au){
 const dx=dest.x-u.x,dy=dest.y-u.y,L2=dx*dx+dy*dy;if(L2<1)return dest;
 const t=clamp(((C.x-u.x)*dx+(C.y-u.y)*dy)/L2,0,1);const px=u.x+dx*t,py=u.y+dy*t;
 if(Math.hypot(px-C.x,py-C.y)>=R)return dest;
 const ad=Math.atan2(dest.y-C.y,dest.x-C.x);const s=angSigned(au,ad);
 return polar(au+Math.sign(s||1)*Math.min(0.3,Math.abs(s)),Math.max(ru,R+110));
}
function bestOpen(au,ad){let best=-1,bd=1e9;for(let s=0;s<4;s++){if(!G.sec[s])continue;const d=angDiff(au,DIRS[s].a)+angDiff(ad,DIRS[s].a)*0.5;if(d<bd){bd=d;best=s;}}return best;}
function navPoint(u,dest){
 const ru=Math.hypot(u.x-C.x,u.y-C.y);let rd=Math.hypot(dest.x-C.x,dest.y-C.y);
 const au=Math.atan2(u.y-C.y,u.x-C.x);let ad=Math.atan2(dest.y-C.y,dest.x-C.x);
 let gates=G_OUT;
 if(u.team===0&&G.barrier){
  const uo=ru>=BAR_R-4,dout=rd>=BAR_R;
  if(uo&&dout)return circDetour(u,dest,BAR_R+50,ru,au);
  if(uo&&!dout){const s=bestOpen(au,ad);
   if(s<0)return circDetour(u,polar(ad,BAR_R+140),BAR_R+50,ru,au);
   const sa=DIRS[s].a;if(!(secOf(au)===s&&angDiff(au,sa)<SEC_H-0.12))return circDetour(u,polar(sa,BAR_R+140),BAR_R+50,ru,au);
   gates=G_OUT.filter(g=>G.sec[secOf(g)]);}
  else if(!uo&&dout){const s=bestOpen(au,ad);if(s>=0){const sa=DIRS[s].a;if(!(secOf(au)===s&&ru>WALL_R+10)){dest=polar(sa,BAR_R+140);rd=BAR_R+140;ad=sa;}}}
 }
 const gu=regOf(ru),gd=regOf(rd);
 if(gu===0&&gd===0)return circDetour(u,dest,WALL_R+40,ru,au);
 if(gu===gd)return gu===1?ringRoute(au,ru,ad,dest):dest;
 const R=(gu===0||(gu===1&&gd===0))?RINGS[0]:RINGS[1];const inward=gu===0||(gu===1&&gd===2);
 const gl=R===RINGS[0]&&gu===0?gates:R.gates;
 let best=gl[0],bd=1e9;for(const g of gl){const d=angDiff(au,g)+0.6*angDiff(ad,g);if(d<bd){bd=d;best=g;}}
 const side=inward?1:-1;
 if(angDiff(au,best)<R.half*0.7&&Math.abs(ru-R.R)<90)return polar(best,R.R-side*80);
 const ap=polar(best,R.R+side*70);
 if(gu===1)return ringRoute(au,ru,best,ap);
 if(gu===0)return circDetour(u,ap,WALL_R+40,ru,au);
 return ap;
}
function moveTo(u,dest,spd,dt){const wp=navPoint(u,dest);let a=ang(u,wp);const d=dist(u,wp);if(d<2)return;const s=Math.min(d,spd*dt);
 const la=u.r+8;if(blockedAt(u.x+Math.cos(a)*la,u.y+Math.sin(a)*la,u.r*0.8)){for(const da of [0.6,1.2,1.8]){const a2=a+da*u.steer;if(!blockedAt(u.x+Math.cos(a2)*la,u.y+Math.sin(a2)*la,u.r*0.8)){a=a2;break;}const a3=a-da*u.steer;if(!blockedAt(u.x+Math.cos(a3)*la,u.y+Math.sin(a3)*la,u.r*0.8)){a=a3;break;}}}
 u.x+=Math.cos(a)*s;u.y+=Math.sin(a)*s;u.face=a;u.moving=true;}
function slowOf(u){if(u.team===0&&inZone(u,'khon'))return 0.6;return 1;}

function damage(tg,amt,src,o){
 o=o||{};if(!tg||tg.dead)return;
 if(tg.isEye)return hitEye(tg,amt,src,o);
 if(tg===hero){if(hero.invul>0||hero.dead)return;amt*=0.7;}
 else if(tg.champ)amt*=0.8;
 if(tg.team===0&&inZone(tg,'ho'))amt*=0.7;
 tg.hp-=amt;tg.flash=0.09;tg.hitT=G.t;
 if(src===hero||o.show)floatTxt(tg.x+RR(-8,8),tg.y-tg.r*2-6,(o.crit?'Bạo kích ':'')+'-'+fmt(amt),o.crit?'#ffd36b':'#fff',o.crit?18:(tg===boss||tg.champ?16:13));
 else if(tg===hero)floatTxt(tg.x+RR(-8,8),tg.y-40,'-'+fmt(amt),'#ff7a6b',14);
 else if(tg.champ&&amt>=120)floatTxt(tg.x+RR(-8,8),tg.y-44,'-'+fmt(amt),tg.team?'#ffd0c0':'#ffb0a0',13);
 if(o.stun)tg.stun=Math.max(tg.stun,tg===boss?o.stun*0.3:tg.champ?o.stun*0.6:o.stun);
 if(tg.hp<=0)kill(tg,src);
}
function hitEye(e,amt,src,o){
 if(e.dead||!e.revealed)return;const E=ELEM[e.el];
 if(e.seals>0){
  const el=o.elem;
  if(el&&(el===e.weak||el==='pha')){
   if(o.cast!==undefined&&e.lastCast===o.cast)return;e.lastCast=o.cast;
   e.seals--;e.flash=0.25;
   G.fx.push({type:'glyph',g:ELEM[el].g,x:e.x,y:e.y-150,t:1,max:1,col:ELEM[el].col});
   G.fx.push({type:'ring',x:e.x,y:e.y-50,r0:20,r1:120,t:0.5,max:0.5,col:E.col});sparks(e.x,e.y-60,E.col,22);G.shake=Math.max(G.shake,dist(hero,e)<800?8:0);
   floatTxt(e.x,e.y-178,(el==='pha'?'Phá Trận Phù phá 1 ấn':(src&&src!==hero?nameOf(src)+': ':'')+ELEM[el].n+' khắc '+E.n+', phá 1 ấn')+(e.seals?' · còn '+e.seals:''),'#ffd36b',16);
   if(e.seals===0){e.vuln=VULN_T;e.lastCast=-1;if(dist(hero,e)<1500)warn('Lõi trận nhãn '+E.n+' lộ ra: '+VULN_T+' giây trước khi ấn tái lập');pushFeed('Ấn bảo hộ trận nhãn '+E.n+' hướng '+DIRS[e.i].n+' vỡ hết');cue(e,4.5,'Hướng '+DIRS[e.i].n+' · '+(src&&src!==hero?nameOf(src)+' phá ấn cuối, ':'')+'lõi trận nhãn '+E.n+' lộ ra',2);boom(e.x,e.y-60,E.col,30,200);G.cmdT=Math.min(G.cmdT,0.3);}
  }else if(src===hero){
   if(G.t-e.vainT>0.9){e.vainT=G.t;floatTxt(e.x,e.y-178,'Vô hiệu · cần '+ELEM[e.weak].n+' khắc '+E.n,'#c9c0b0',14);G.fx.push({type:'ring',x:e.x,y:e.y-50,r0:40,r1:70,t:0.3,max:0.3,col:'200,200,200'});}
  }
  return;
 }
 if(e.vuln<=0)return;
 if(src&&src!==hero&&!src.champ&&src.t!=='quy')amt*=0.35;
 e.hp-=amt;e.flash=0.1;e.lastHit=G.t;
 if(src===hero||o.show)floatTxt(e.x+RR(-12,12),e.y-150,fmt(amt),o.crit?'#ffd36b':'#f3d9a0',o.crit?20:15);
 if(e.hp<=0)eyeDown(e,src);
}
function kill(u,src){
 if(u.dead)return;u.dead=true;u.hp=0;
 splat(u.x,u.y,u.team,u.r);
 if(u===hero){hero.respawn=6;pushFeed('Ngao Thiên trọng thương, lui về hậu phương');return;}
 if(u.champ){
  if(u.team===0){u.respawn=12;u.retreat=false;pushFeed(nameOf(u)+' trọng thương, lui về doanh trại');cue({x:u.x,y:u.y},2.6,nameOf(u)+' trọng thương, 12 giây sau trở lại',2);}
  else{G.maDown++;pushFeed(srcName(src)+' hạ '+nameOf(u));cue({x:u.x,y:u.y},3,'',3);showBanner(nameOf(u)+' đã ngã',u.D.sect+' mất một tướng trấn giữ',2.4,u.D.g);boom(u.x,u.y,u.D.col,40,220);G.shake=Math.max(G.shake,dist(hero,u)<700?12:0);G.mcmdT=Math.min(G.mcmdT,0.5);}
  return;
 }
 if(u.team===1){
  if(u===boss){bossDown(src);return;}
  G.killed++;
  if(src===hero){G.heroKills++;if(u.t==='tuong'||G.t-G.lastHeroFeed>5){pushFeed('Ngao Thiên hạ '+TYPES[u.t].name);G.lastHeroFeed=G.t;}}
 }else{G.lost++;}
}
function revealEye(e,by){
 if(e.revealed)return;e.revealed=true;e.shock=3.5;G.warnT=0;const E=ELEM[e.el],W=ELEM[e.weak];
 const near=dist(hero,e)<1500;
 if(near){G.flash=Math.max(G.flash,0.3);G.flashCol=E.col;G.shake=Math.max(G.shake,12);}
 boom(e.x,e.y-40,E.col,40,260);
 G.fx.push({type:'glyph',g:E.g,x:e.x,y:e.y-120,t:1.2,max:1.2,col:E.col});
 if(near)cueHero(3.4);
 if(near)showBanner('Trận nhãn '+E.n+' hiện hình',e.maxSeals+' ấn '+E.n+' bảo hộ · '+W.n+' khắc '+E.n+': '+e.toolT,3.2,E.g);
 pushFeed(srcName(by)+' soi ra trận nhãn '+E.n+' ở '+e.place);
 cue(e,4.5,'Hướng '+DIRS[e.i].n+' · '+srcName(by)+' soi ra trận nhãn '+E.n+' ở '+e.place,2);
 const n=5;for(let k=0;k<n+1;k++){const a=RR(0,TAU),rr=RR(140,270);let x=e.x+Math.cos(a)*rr,y=e.y+Math.sin(a)*rr;const rc=Math.hypot(x-C.x,y-C.y);if(rc<BAR_R+40){x=C.x+(x-C.x)/rc*(BAR_R+40);y=C.y+(y-C.y)/rc*(BAR_R+40);}
  const u=addUnit(k<n?'no':'tuong',x,y,{role:'amb',rise:1,stun:1});u.home={x:e.x,y:e.y};G.fx.push({type:'ring',x,y,r0:4,r1:34,t:0.6,max:0.6,col:'90,20,20'});}
 G.totalEnemy+=n+1;
}
function eyeDown(e,src){
 if(e.dead)return;e.dead=true;e.hp=0;e.vuln=0;G.warnT=0;const E=ELEM[e.el],d=e.i;
 pushFeed(srcName(src)+' phá trận nhãn '+E.n+' hướng '+DIRS[d].n);
 cue(e,4,'Hướng '+DIRS[d].n+' · '+srcName(src)+' phá trận nhãn '+E.n,3);
 G.shake=Math.max(G.shake,dist(hero,e)<1500?18:6);G.flash=Math.max(G.flash,0.35);G.flashCol=E.col;boom(e.x,e.y-40,E.col,50,300);
 G.decals.push({x:e.x,y:e.y,r:52,t:999,type:'crater'});G.ripples.push({a:e.va,t:0,big:true});
 // cung kết giới hướng này sụp
 G.sec[d]=true;G.breakT=0;G.breakD=d;const A=DIRS[d].a;
 for(let i=0;i<170;i++){const a=A+RR(-SEC_H,SEC_H);G.shards.push({x:C.x+Math.cos(a)*BAR_R,y:C.y+Math.sin(a)*BAR_R,z:RR(0,160),vx:Math.cos(a)*RR(30,160),vy:Math.sin(a)*RR(30,160),vz:RR(60,260),rot:RR(0,TAU),vr:RR(-8,8),s:RR(5,14),t:RR(1,1.8)});}
 if(G.open<0)G.open=d;else showBanner('Cung kết giới hướng '+DIRS[d].n+' sụp','Thêm một cửa vào thành',2.4,'破');
 if(G.sec.every(s=>s))G.barrier=false;
 G.cmdT=DIRECTOR?2.4:0;G.mcmdT=0;
}
function bossDown(src){
 pushFeed(srcName(src)+' chém hạ Huyết Sát Phó Tướng');
 showBanner('Trấn thủ đã ngã','Quân Ma đạo rối loạn, thủ quân mất ý chí',3,'煞');
 G.shake=20;G.flash=0.5;G.flashCol='255,210,150';boom(boss.x,boss.y,'229,96,77',70,420);
 $('boss').style.display='none';
 for(const u of G.units)if(!u.dead&&u.team===1&&dist(u,boss)<600)u.stun=Math.max(u.stun,1.5);
}
