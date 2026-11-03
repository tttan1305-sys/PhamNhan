/* ================= cập nhật ================= */
function updateHero(dt){
 const h=hero;
 if(h.flash>0)h.flash-=dt;if(h.swing>0)h.swing-=dt;
 if(h.dead){h.respawn-=dt;if(h.respawn<=0){h.dead=false;h.hp=h.max;h.mp=h.mpMax;const p=G.open>=0?openPt(BAR_R+260):{x:CAMP.x+420,y:CAMP.y};h.x=p.x;h.y=p.y;h.pr=dist(h,C);h.invul=2;pushFeed('Ngao Thiên trở lại chiến trường');}return;}
 h.mp=Math.min(h.mpMax,h.mp+14*dt);h.st=Math.min(100,h.st+16*dt);h.hp=Math.min(h.max,h.hp+12*dt);
 h.cdS-=dt;for(const k in h.cd2)h.cd2[k]-=dt;h.invul-=dt;
 const foe=nearestEnemy(h,350,e=>reachable(h,e));
 if(!foe&&!inZone(h,'khon'))h.flyT=Math.min(3,h.flyT+dt);else h.flyT=0;
 if(h.stun>0){h.stun-=dt;return;}
 h.moving=false;
 if(h.dash){const s=1300*dt;h.x+=h.dash.x*s;h.y+=h.dash.y*s;h.face=Math.atan2(h.dash.y,h.dash.x);h.dash.t-=dt;G.fx.push({type:'ghost',x:h.x,y:h.y,a:h.face,t:0.25,max:0.25});if(h.dash.t<=0)h.dash=null;return;}
 const m=Math.hypot(IN.mx,IN.my);
 if(m>0.08&&!G.over){const k=Math.min(1,m);const fly=h.flyT>0.8;const s=TYPES.hero.spd*(fly?1.55:1)*slowOf(h)*k*dt;h.x+=IN.mx/m*s;h.y+=IN.my/m*s;h.face=Math.atan2(IN.my,IN.mx);h.moving=true;
  if(fly&&Math.floor(G.t*20)!==Math.floor((G.t-dt)*20))G.fx.push({type:'trail',x:h.x,y:h.y-4,t:0.45,max:0.45});}
 if(IN.slash)castSlash();
}
function updateProj(dt){
 for(const p of G.proj){
  if(p.dead)continue;
  if(p.kind==='arrow'){const a=Math.atan2(p.ty-p.y,p.tx-p.x),d=Math.hypot(p.tx-p.x,p.ty-p.y),s=p.spd*dt;p.a=a;if(d<=s){p.dead=true;if(p.tgt&&!p.tgt.dead&&Math.hypot(p.tgt.x-p.tx,p.tgt.y-6-p.ty)<p.tgt.r+12)damage(p.tgt,p.dmg,p.src);}else{p.x+=Math.cos(a)*s;p.y+=Math.sin(a)*s;}continue;}
  if(p.kind==='bolt'){const t=p.tgt;if(!t||t.dead){p.dead=true;continue;}const tx=t.x,ty=t.y-(t.isEye?40:8);
   if(p.team===0&&p.ob&&G.barrier&&!t.isEye&&t.pr<BAR_R&&Math.hypot(p.x-C.x,p.y-C.y)<BAR_R+4&&!secOpenA(angC(p))){p.dead=true;G.ripples.push({a:angC(p),t:0});continue;}
   const a=Math.atan2(ty-p.y,tx-p.x),d=Math.hypot(tx-p.x,ty-p.y),s=p.spd*dt;p.trail.push([p.x,p.y]);if(p.trail.length>6)p.trail.shift();
   if(d<=s+t.r*0.5){p.dead=true;damage(t,p.dmg,p.src);sparks(tx,ty,p.col||'240,220,140',p.big?7:4);}else{p.x+=Math.cos(a)*s;p.y+=Math.sin(a)*s;}continue;}
  if(p.kind==='sword'){if(p.delay>0){p.delay-=dt;p.x=hero.x;p.y=hero.y-20;continue;}const t=p.tgt;p.life-=dt;
   p.trail.push([p.x,p.y]);if(p.trail.length>10)p.trail.shift();
   if(!t||t.dead||p.life<=0){p.vx*=0.9;p.vy*=0.9;p.fade=(p.fade||0.3)-dt;p.x+=p.vx*dt;p.y+=p.vy*dt;if(p.fade<=0)p.dead=true;continue;}
   const tx=t.x,ty=t.y-(t.isEye?55:10);const a=Math.atan2(ty-p.y,tx-p.x);const want={x:Math.cos(a)*p.spd,y:Math.sin(a)*p.spd};const k=Math.min(1,7*dt);p.vx+=(want.x-p.vx)*k;p.vy+=(want.y-p.vy)*k;p.x+=p.vx*dt;p.y+=p.vy*dt;
   if(Math.hypot(tx-p.x,ty-p.y)<t.r+10){p.dead=true;damage(t,p.dmg,hero,{crit:rnd()<0.2,elem:'kim',cast:p.cast});sparks(tx,ty,'160,230,255',10);}
   continue;}
  if(p.kind==='pha'){p.t+=dt;const t=p.tgt;const k=Math.min(1,p.t/p.max);const tx=t.x,ty=(t.isEye?t.y-50:t.y);p.x=p.sx+(tx-p.sx)*k;p.y=p.sy+(ty-p.sy)*k-Math.sin(k*Math.PI)*120;
   if(k>=1){p.dead=true;
    if(t.isEye){if(t.seals>0)damage(t,0,hero,{elem:'pha',cast:++G.castN});else damage(t,900,hero,{show:true,crit:true});}
    else if(t.type==='khon'){t.t=0;pushFeed('Phá Trận Phù phá vỡ Khốn Long Trận');}
    G.fx.push({type:'glyph',g:'破',x:tx,y:ty-30,t:0.9,max:0.9,col:'255,215,120'});boom(tx,ty,'255,200,110',26,200);G.shake=Math.max(G.shake,10);}
   continue;}
 }
 G.proj=G.proj.filter(p=>!p.dead);
}
function elemStrike(t){
 const src=t.src||hero;
 for(const e of G.units){if(e.dead||e.team===t.team)continue;if(e===boss&&!boss.active)continue;if(dist(e,t)<t.r+e.r){const big=e.champ||e===boss;damage(e,big?t.dmg*1.5:t.dmg*RR(0.9,1.1),src,{stun:t.kind==='vine'?1.6:t.kind==='ice'?0.8:t.kind==='an'?1.2:0,show:big});}}
 for(const e of G.eyes){if(!e.dead&&e.revealed&&dist(e,t)<t.r+e.r+10)damage(e,260,src,{elem:t.elem,cast:t.cast,show:true});}
 const near=dist(hero,t)<700;
 if(t.kind==='an'){boom(t.x,t.y,'214,170,106',30,t.r*1.6);G.decals.push({x:t.x,y:t.y,r:85,t:16,type:'crack',seed:Math.floor(rnd()*1000)});G.fx.push({type:'glyph',g:'鎮',x:t.x,y:t.y-40,t:0.8,max:0.8,col:'224,184,120'});if(near)G.shake=Math.max(G.shake,11);}
 else if(t.kind==='fire'){boom(t.x,t.y,'255,138,74',30,t.r*1.5);G.decals.push({x:t.x,y:t.y,r:75,t:14,type:'scorch'});for(let i=0;i<18;i++)G.fx.push({type:'p',x:t.x+RR(-45,45),y:t.y+RR(-22,22),vx:RR(-20,20),vy:RR(-150,-60),t:RR(0.5,0.9),max:0.9,col:'255,150,60',s:RR(2,4)});G.fx.push({type:'glyph',g:'炎',x:t.x,y:t.y-40,t:0.7,max:0.7,col:'255,150,80'});if(near)G.shake=Math.max(G.shake,8);}
 else if(t.kind==='ice'){boom(t.x,t.y,'150,210,255',24,t.r*1.4);G.fx.push({type:'ice',x:t.x,y:t.y,t:0.8,max:0.8,r:t.r});G.fx.push({type:'glyph',g:'冰',x:t.x,y:t.y-40,t:0.7,max:0.7,col:'170,220,255'});if(near)G.shake=Math.max(G.shake,6);}
 else if(t.kind==='vine'){G.fx.push({type:'vine',x:t.x,y:t.y,t:1.5,max:1.5,r:t.r});sparks(t.x,t.y,'127,209,139',16);G.fx.push({type:'glyph',g:'藤',x:t.x,y:t.y-40,t:0.7,max:0.7,col:'127,209,139'});}
}
function maStrike(t){
 for(const e of G.units){if(e.dead||e.team!==0)continue;if(dist(e,t)<t.r+e.r)damage(e,e.champ?t.dmg:RR(45,65),t.src,{stun:t.stun||0});}
 boom(t.x,t.y,t.col,26,t.r*1.5);G.decals.push({x:t.x,y:t.y,r:t.r*0.7,t:10,type:'blood'});G.fx.push({type:'glyph',g:t.g,x:t.x,y:t.y-40,t:0.8,max:0.8,col:t.col});
 G.shake=Math.max(G.shake,dist(hero,t)<600?8:0);
}
function updateZones(dt){
 for(const z of G.zones){
  z.t-=dt;
  if(z.type==='van'){z.x=hero.x;z.y=hero.y;z.tick-=dt;while(z.tick<=0&&z.t>0.4){z.tick+=0.055;let tx,ty;const es=G.units.filter(e=>!e.dead&&e.team===1&&dist(e,z)<z.r);if(es.length&&rnd()<0.7){const e=es[Math.floor(rnd()*es.length)];tx=e.x+RR(-14,14);ty=e.y+RR(-14,14);}else{const a=RR(0,TAU),r=Math.sqrt(rnd())*z.r;tx=z.x+Math.cos(a)*r;ty=z.y+Math.sin(a)*r;}G.fx.push({type:'fall',x:tx,y:ty,t:0.34,max:0.34});}}
  if(z.type==='khon'){for(const a of G.allies){if(!a.dead&&dist(a,z)<z.r){a.hp-=26*dt;if(a.hp<=0)kill(a,boss);}}}
 }
 G.zones=G.zones.filter(z=>z.t>0);
 for(const t of G.teles){t.t-=dt;if(t.t<=0){t.done=true;
  if(t.kind==='loi')loiStrike(t);
  else if(t.kind==='an'){if(t.src&&t.src!==hero)elemStrike(t);else anStrike(t);}
  else if(t.kind==='fire'||t.kind==='ice'||t.kind==='vine')elemStrike(t);
  else if(t.kind==='mtele')maStrike(t);
  else if(t.kind==='shock'){for(const e of G.units){if(e.dead||e.team!==0)continue;if(dist(e,t)<t.r+e.r){damage(e,e===hero?200:e.champ?120:RR(18,26),null);const a=ang(t,e),push=e.t==='quy'?18:70;e.x+=Math.cos(a)*push;e.y+=Math.sin(a)*push;if(e!==hero)e.stun=Math.max(e.stun,0.4);}}G.fx.push({type:'ring',x:t.x,y:t.y,r0:20,r1:t.r+25,t:0.45,max:0.45,col:t.col||'255,140,190'});sparks(t.x,t.y-70,t.col||'255,150,200',16);G.shake=Math.max(G.shake,dist(hero,t)<700?6:0);}
  else if(t.kind==='tram'){for(const e of G.units){if(e.dead||e.team!==0)continue;if(dist(e,t)<t.r+e.r)damage(e,e===hero?t.dmg:e.champ?t.dmg*0.75:t.dmg*0.12,boss);}boom(t.x,t.y,'229,96,77',30,t.r*1.6);G.shake=Math.max(G.shake,dist(hero,t)<600?10:0);G.decals.push({x:t.x,y:t.y,r:t.r*0.8,t:10,type:'blood'});}
 }}
 G.teles=G.teles.filter(t=>!t.done);
}
function updateEyes(dt){
 for(const w of G.waves){w.r+=900*dt;for(const e of G.eyes){if(!e.revealed&&!e.dead&&dist(w,e)<=w.r){revealEye(e,w.by||hero);w.found=true;}}
  if(w.r>=w.max&&!w.found&&!w.by&&!AUTO&&G.eyes.some(e=>!e.dead&&!e.revealed))floatTxt(hero.x,hero.y-60,'Không có trận nhãn ẩn trong tầm soi','#c9c0b0',13);}
 G.waves=G.waves.filter(w=>w.r<w.max);
 if(G.veinGlow>0)G.veinGlow-=dt;
 for(const e of G.eyes){if(e.flash>0)e.flash-=dt;if(e.dead)continue;
  if(!e.revealed){if(!e.sensed&&!hero.dead&&dist(hero,e)<500){e.sensed=true;pushFeed('Linh khí dao động mạnh quanh '+e.place);warn('Linh khí dao động: dùng 眼 Phá Vọng Nhãn (4) để soi trận nhãn ẩn');}continue;}
  if(e.vuln>0){e.vuln-=dt;if(e.vuln<=0){e.vuln=0;e.reforms++;e.seals=Math.max(1,e.maxSeals-e.reforms);e.lastCast=-1;if(dist(hero,e)<1500)warn('Ấn trận nhãn '+e.name+' tái lập: '+e.seals+' ấn');pushFeed('Trận nhãn '+e.name+' hướng '+DIRS[e.i].n+' tái lập ấn');boom(e.x,e.y-50,ELEM[e.el].col,20,150);}}
  else if(e.hp<e.max)e.hp=Math.min(e.max,e.hp+45*dt);
  e.shock-=dt;if(e.shock<=0&&countEnemies(e,175,0)>0){e.shock=6.5;G.teles.push({x:e.x,y:e.y,r:165,t:0.9,max:0.9,team:1,kind:'shock',col:ELEM[e.el].col});}
 }
}
function updateFx(dt){
 for(const f of G.fx){f.t-=dt;
  if(f.type==='p'||f.type==='smoke'){f.x+=f.vx*dt;f.y+=f.vy*dt;f.vx*=0.94;f.vy*=f.type==='smoke'?0.99:0.94;if(f.g)f.vy+=f.g*dt;}
  if(f.type==='fall'&&f.t<=0&&!f.hit){f.hit=true;for(const e of G.units){if(e.dead||e.team!==1)continue;if(Math.hypot(e.x-f.x,e.y-f.y)<44+e.r)damage(e,RR(80,105),hero,{crit:rnd()<0.12});}sparks(f.x,f.y,'243,217,160',5);G.fx.push({type:'ring',x:f.x,y:f.y,r0:6,r1:46,t:0.25,max:0.25,col:'243,217,160'});G.shake=Math.max(G.shake,3);}
 }
 G.fx=G.fx.filter(f=>f.t>0);if(G.fx.length>900)G.fx.splice(0,G.fx.length-900);
 for(const f of G.floats){f.t-=dt;f.y+=f.vy*dt;f.vy*=0.95;}
 G.floats=G.floats.filter(f=>f.t>0);
 for(const d of G.decals)d.t-=dt;G.decals=G.decals.filter(d=>d.t>0);if(G.decals.length>260)G.decals.splice(0,G.decals.length-260);
 for(const r of G.ripples)r.t+=dt;G.ripples=G.ripples.filter(r=>r.t<0.8);
 for(const s of G.shards){s.t-=dt;s.x+=s.vx*dt;s.y+=s.vy*dt;s.z+=s.vz*dt;s.vz-=420*dt;s.rot+=s.vr*dt;}
 G.shards=G.shards.filter(s=>s.t>0);
 if(G.breakT>=0)G.breakT+=dt;
 G.shake*=Math.pow(0.02,dt);G.flash=Math.max(0,G.flash-dt*1.6);
 if(G.banner){G.banner.t-=dt;if(G.banner.t<=0)G.banner=null;}
 if(G.warnT>0)G.warnT-=dt;
}
function separation(){
 const cell=48,grid=new Map();
 for(const u of G.units){if(u.dead)continue;const k=((u.x/cell)|0)*8192+((u.y/cell)|0);let a=grid.get(k);if(!a){a=[];grid.set(k,a);}a.push(u);}
 for(const u of G.units){if(u.dead)continue;const cx=(u.x/cell)|0,cy=(u.y/cell)|0;
  for(let i=-1;i<=1;i++)for(let j=-1;j<=1;j++){const a=grid.get((cx+i)*8192+(cy+j));if(!a)continue;
   for(const v of a){if(v.id<=u.id)continue;const dx=v.x-u.x,dy=v.y-u.y,rr=u.r+v.r;const d2=dx*dx+dy*dy;if(d2>=rr*rr||d2===0)continue;const d=Math.sqrt(d2),o=(rr-d);
    const mu=massOf(u),mv=massOf(v),tot=mu+mv;const nx=dx/d,ny=dy/d;
    if(!(u===hero&&hero.dash)){u.x-=nx*o*(mv/tot);u.y-=ny*o*(mv/tot);}
    if(!(v===hero&&hero.dash)){v.x+=nx*o*(mu/tot);v.y+=ny*o*(mu/tot);}}}}
}
function massOf(u){return u.t==='boss'?12:u.t==='quy'?10:u.champ?4:u.t==='long'||u.t==='tuong'?2:1;}
function collide(){
 for(const u of G.units){if(u.dead)continue;
  for(const R of RINGS){const dx=u.x-C.x,dy=u.y-C.y,r=Math.hypot(dx,dy)||1;const inner=R.R-R.T/2-u.r,outer=R.R+R.T/2+u.r;
   if(r>inner&&r<outer){const a=Math.atan2(dy,dx);let gate=false;for(const g of R.gates)if(angDiff(a,g)<R.half-u.r/R.R)gate=true;
    if(!gate){const nr=u.pr<R.R?inner:outer;u.x=C.x+dx/r*nr;u.y=C.y+dy/r*nr;}}}
  if(G.barrier&&u.team===0){const dx=u.x-C.x,dy=u.y-C.y,r=Math.hypot(dx,dy)||1;
   if(Math.abs(r-BAR_R)<u.r+2&&!G.sec[secOf(Math.atan2(dy,dx))]){const out=u.pr>=BAR_R;const nr=out?BAR_R+u.r+2:BAR_R-u.r-2;u.x=C.x+dx/r*nr;u.y=C.y+dy/r*nr;if(u===hero&&out&&rnd()<0.05)G.ripples.push({a:Math.atan2(dy,dx),t:0});}}
  const arr=BGRID.get(Math.floor(u.x/BC)*4096+Math.floor(u.y/BC));if(arr)for(const b of arr)pushOBB(u,b);
  for(const p of G.eyes){if(p.dead||!p.revealed)continue;const d=Math.hypot(u.x-p.x,u.y-p.y);if(d<p.r+u.r&&d>0){u.x=p.x+(u.x-p.x)/d*(p.r+u.r);u.y=p.y+(u.y-p.y)/d*(p.r+u.r);}}
  u.x=clamp(u.x,30,WW-30);u.y=clamp(u.y,30,WH-30);
  u.pr=Math.hypot(u.x-C.x,u.y-C.y);
 }
}
function phaseLogic(dt){
 if(G.phase===1&&G.open>=0){
  G.phase=2;let inside=0;for(const u of G.units)if(!u.dead&&u.team===1&&!u.champ&&u!==boss&&u.pr<BAR_R)inside++;
  G.k0=G.killed;G.need=Math.max(20,Math.ceil(inside*0.5));
  pushFeed('Kết giới hướng '+dirName(G.open)+' sụp đổ');
 }
 if(G.phase===2&&G.killed-G.k0>=G.need&&boss.dead){G.phase=3;showBanner('Cắm cờ','Giữ đài trung tâm '+G.flagNeed+' giây',2.6,'旗');pushFeed('Thủ quân tan vỡ, tiến lên đài trung tâm');}
 if(G.phase===3&&!G.over){
  const inZ=G.allies.some(a=>!a.dead&&dist(a,C)<FLAG_R+6);
  G.contested=countEnemies(C,FLAG_R+40)>0;
  if(inZ&&!G.contested){const allies=countEnemies(C,FLAG_R+30,0);G.flag+=dt*(allies>=5?1.25:1);}
  if(G.flag>=G.flagNeed){G.over='win';G.overT=0;G.flagOwner=0;G.flash=0.8;G.flashCol='243,217,160';showBanner('Công phá Phù Vân Thành','Cờ Chính đạo tung bay trên đài trung tâm',3.6,'捷');pushFeed('Phù Vân Thành về tay Chính đạo');
   for(const u of G.units)if(!u.dead&&u.team===1)u.stun=99;}
 }
}
function update(dt){
 G.t+=dt;
 if(G.strat){G.strat.t-=dt;if(G.strat.t<=0)G.strat=null;}
 if(G.intro>0){G.intro-=dt;updateFx(dt);camUpdate(dt);return;}
 if(G.over){G.overT+=dt;if(G.overT>3.8&&!G.endShown)showEnd();}
 else{G.time-=dt;if(G.time<=0){G.time=0;G.over='lose';G.overT=0;showBanner('Hết giờ','Thủ quân Phù Vân Thành giữ được thành',4);}}
 if(AUTO)autopilot();else readInput();
 G.hoCd-=dt;G.phaCd-=dt;G.danCd-=dt;G.anCd-=dt;
 if(G.an<3){G.anT+=dt;if(G.anT>=20){G.anT=0;G.an++;}}else G.anT=0;
 if(G.loi<5){G.loiT+=dt;if(G.loiT>=25){G.loiT=0;G.loi++;}}else G.loiT=0;
 updateHero(dt);
 for(const u of G.units){if(u===hero||u===boss)continue;if(u.champ){updateChamp(u,dt);continue;}if(u.dead)continue;updateUnit(u,dt);}
 updateBoss(dt);
 G.cmdT-=dt;if(G.cmdT<=0){G.cmdT=2.5;cmdAtk();}
 G.mcmdT-=dt;if(G.mcmdT<=0){G.mcmdT=2.5;cmdDef();}
 updateEyes(dt);
 separation();collide();
 updateProj(dt);updateZones(dt);updateFx(dt);
 phaseLogic(dt);
 if(G.t%2<dt)G.units=G.units.filter(u=>!u.dead||u.champ||u===boss);
 camUpdate(dt);
}
function camUpdate(dt){
 let tx,ty;
 if(G.intro>0){const k=clamp(1-(G.intro-0.7)/4.0,0,1);const e=k*k*(3-2*k);const sx=C.x-600;tx=sx+(hero.x-sx)*e;ty=C.y+(hero.y-40-C.y)*e;G.zoom=Z0*Math.pow(Z_FIGHT/Z0,e);}
 else if(G.over==='win'&&G.overT>0.6){tx=C.x;ty=C.y;G.zoom+=(0.42-G.zoom)*Math.min(1,dt*0.8);}
 else{
  director(dt);
  const f=G.cue&&G.cue.t>0?G.cue.o:null;let z;
  if(f){tx=f.x;ty=f.y-40;z=0.72;}
  else{const fly=hero.flyT>0.8&&hero.moving;z=fly?Z_FLY:Z_FIGHT;tx=hero.x+Math.cos(hero.face)*(fly?120:60);ty=hero.y-40+Math.sin(hero.face)*(fly?80:30);}
  const far=Math.hypot(tx-(G.cam.x+VW/G.zoom/2),ty-(G.cam.y+VH/G.zoom/2));G.camFar=DIRECTOR?far:0;
  if(G.camFar>1300)z=Math.min(z,0.36);
  G.zoom+=(z-G.zoom)*Math.min(1,dt*(G.camFar>1300?2.6:1.4));
 }
 const vw=VW/G.zoom,vh=VH/G.zoom;
 const cx=clamp(tx-vw/2,0,WW-vw),cy=clamp(ty-vh/2,0,WH-vh);
 const k=G.intro>0?1:Math.min(1,dt*(G.over?1.5:G.camFar>1300?2.2:4));
 G.cam.x+=(cx-G.cam.x)*k;G.cam.y+=(cy-G.cam.y)*k;
}
/* ================= máy quay chiến trường: chỉ bật ở bản quay video và chế độ xem ================= */
const DIRECTOR=REC||QS.has('demo');
function cue(o,dur,txt,pri){
 if(!DIRECTOR||!AUTO||G.intro>0||G.over)return;pri=pri||2;
 const c=G.cue;if(c&&c.t>0.6&&c.pri>pri)return;
 if(dist(hero,o)<700&&!hero.dead)return;
 if(pri<3&&G.t<(G.heroHold||0)){G.cueQ={o,dur,txt,pri,at:G.t};return;}
 G.cue={o,t:dur,max:dur,txt,pri};G.heroT=0;
}
function cueHero(hold){if(!DIRECTOR)return;if(G.cue&&G.cue.pri<3)G.cue=null;G.heroT=0;G.heroHold=G.t+(hold||3);}
function frontText(c){const d=secOf(angC(c));const al=G.allies.filter(a=>!a.dead&&dist(a,c)<750).map(nameOf);const fo=G.foes.filter(m=>!m.dead&&m!==boss&&dist(m,c)<800).map(nameOf);return 'Hướng '+DIRS[d].n+' · '+al.join(', ')+(fo.length?' đấu '+fo.join(', '):'');}
function heatOf(c){let s=0;for(const u of G.units){if(u.dead||u.team===c.team||dist(u,c)>420)continue;s+=u.champ?6:1;}for(const e of G.eyes){if(!e.dead&&e.revealed&&dist(e,c)<520)s+=e.vuln>0?8:4;}return s;}
function director(dt){
 if(!DIRECTOR||!AUTO){G.cue=null;return;}
 const c=G.cue;
 if(c){c.t-=dt;if(c.t<=0||(c.o.dead&&!c.o.isEye))G.cue=null;}
 if(G.cue)return;
 const q=G.cueQ;if(q&&G.t>=(G.heroHold||0)){G.cueQ=null;if(G.t-q.at<5&&!(q.o.dead&&!q.o.isEye)){cue(q.o,q.dur-0.5,q.txt,q.pri);if(G.cue)return;}}
 G.heroT=(G.heroT||0)+dt;
 const busy=nearestEnemy(hero,420)!==null;
 if(G.phase!==1||G.strat||G.t<(G.heroHold||0)||G.heroT<(busy?9:5))return;
 let best=null,bs=8;
 for(const a of G.allies){if(a===hero||a.dead||dist(a,hero)<1100||dist(a,CAMP)<700)continue;let s=heatOf(a);if(G.lastFront===secOf(angC(a)))s*=0.55;if(s>bs){bs=s;best=a;}}
 if(best){G.lastFront=secOf(angC(best));cue(best,6.5,frontText(best),1);}else G.heroT=4;
}
