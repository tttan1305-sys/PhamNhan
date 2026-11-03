function setMode(m,silent){
 G.mode=m;['auto','split','mass'].forEach(k=>$('c_'+k).classList.toggle('on',k===m));
 if(!silent){pushFeed(m==='auto'?'Thế trận: để quân sư tự quyết':m==='split'?'Thế trận: phân binh ba hướng':'Thế trận: năm tướng hợp binh theo Ngao Thiên');G.cmdT=0;}
}

/* ================= AI quân lính ================= */
function leaderOf(u){const L=u.lead;if(L&&!L.dead)return L;if(!L)return null;
 if(u.tmpT===undefined||G.t>u.tmpT){u.tmpT=G.t+2;let best=null,bd=1e9;for(const c of (u.team?G.foes:G.allies)){if(!c||c.dead||c===boss)continue;const d=dist(u,c);if(d<bd){bd=d;best=c;}}u.tmpL=best;}
 return u.tmpL&&!u.tmpL.dead?u.tmpL:null;}
function chooseTarget(u){
 const T=TYPES[u.t];
 if(u.team===0){
  const L=leaderOf(u);const far=L&&G.phase<3&&dist(u,L)>620;
  if(u.t==='loan'&&L===hero&&!far)return nearestEnemy(u,280,e=>reachable(u,e)&&dist(e,hero)<500&&e!==boss);
  if(u.t==='quy'){const ey=vulnEye(u,900);if(ey)return ey;return nearestEnemy(u,280,e=>reachable(u,e)&&e!==boss);}
  const ag=far?46:(T.kind==='caster'?320:250);
  const e=nearestEnemy(u,ag,e=>e!==boss&&reachable(u,e));if(e)return e;
  if(far)return null;
  const ey=vulnEye(u,650);if(ey)return ey;
  if(L){const f=nearestEnemy(L,380,e=>e!==boss&&reachable(u,e));if(f&&dist(u,f)<720)return f;}
  if(G.phase===2&&!L)return nearestEnemy(u,700,e=>e!==boss);
  if(G.phase===3)return nearestEnemy(u,420);
  return null;
 }
 if(u.role==='ret'){const L=leaderOf(u);if(L&&dist(u,L)>600)return nearestEnemy(u,110);const e=nearestEnemy(u,T.kind==='archer'?T.range+15:260);if(e)return e;if(L)return nearestEnemy(L,360);return nearestEnemy(u,300);}
 if(u.role==='guard')return nearestEnemy(u,T.kind==='archer'?T.range+15:380,e=>dist(e,u.home)<650);
 if(u.role==='van')return nearestEnemy(u,T.kind==='archer'?T.range+15:300,e=>dist(e,u.home)<560);
 if(u.role==='amb')return nearestEnemy(u,560,e=>dist(e,u.home)<850);
 if(G.open<0){if(T.kind==='archer')return nearestEnemy(u,T.range+15);return nearestEnemy(u,200,e=>dist(e,u.home)<420);}
 if(T.kind==='archer')return nearestEnemy(u,G.phase===3?600:430);
 return nearestEnemy(u,G.phase===3?1200:(u.role==='pat'?560:650));
}
function objective(u){
 if(u.team===0){
  if(G.phase===3||G.over)return{x:C.x+u.ox*1.3,y:C.y+u.oy*1.3};
  const L=leaderOf(u);
  if(L){const a=L.face,back=u.t==='loan'?40:90;return{x:L.x-Math.cos(a)*back+u.ox*1.15,y:L.y-Math.sin(a)*back+u.oy*1.15};}
  return{x:u.x,y:u.y};
 }
 if(G.phase===3)return C;
 if(u.role==='ret'){const L=leaderOf(u);if(L)return{x:L.x+u.ox*0.9,y:L.y+u.oy*0.9};}
 if(u.role==='pat'&&G.open>=0)return openPt(RING_R);
 return u.home;
}
function updateUnit(u,dt){
 const T=TYPES[u.t];
 if(u.anim>0)u.anim-=dt;if(u.flash>0)u.flash-=dt;if(u.rise>0)u.rise-=dt;
 if(u.stun>0){u.stun-=dt;return;}
 u.cd-=dt;u.retgt-=dt;u.moving=false;
 if(G.over){if(u.team===1){u.stun=9;}return;}
 if(u.retgt<=0){u.retgt=0.25+rnd()*0.15;u.tgt=chooseTarget(u);}
 let tg=u.tgt;if(tg&&(tg.dead||(tg.isEye&&tg.vuln<=0))){tg=u.tgt=null;u.retgt=0;}
 let dest=null;
 if(tg){
  const d=dist(u,tg)-(tg.r||0)-u.r;
  if(d<=T.range){u.face=ang(u,tg);if(u.cd<=0){attack(u,tg);u.cd=T.cd*RR(0.9,1.1);}}
  else dest=tg;
 }else dest=objective(u);
 let spd=T.spd*slowOf(u);
 if(!tg&&dest&&dist(u,dest)>160)spd*=(u.team===0||u.role==='ret')?2.4:1.3;
 if(u.t==='loan'&&u.lead===hero&&!hero.dead&&dist(u,hero)>260)spd=T.spd*2.1;
 if(dest&&dist(u,dest)>6)moveTo(u,dest,spd,dt);
}
function attack(u,tg){
 const T=TYPES[u.t];u.anim=0.25;
 if(T.kind==='archer'){G.proj.push({kind:'arrow',x:u.x,y:u.y-12,tx:tg.x+RR(-8,8),ty:tg.y-6+RR(-6,6),tgt:tg,spd:560,dmg:T.atk*RR(0.85,1.15),team:1,src:u,life:1.2});return;}
 if(T.kind==='caster'){G.proj.push({kind:'bolt',x:u.x,y:u.y-12,tgt:tg,spd:430,dmg:T.atk*RR(0.9,1.2),team:0,src:u,life:1.5,trail:[],ob:u.pr>=BAR_R});return;}
 let dmg=T.atk*RR(0.85,1.15);
 if(u.t==='quy'){if(tg.isEye){G.shake=Math.max(G.shake,dist(hero,u)<500?5:0);sparks(tg.x,tg.y-20,'200,170,255',12);}else dmg*=0.25;}
 damage(tg,dmg,u);
 if(rnd()<0.35)sparks((u.x+tg.x)/2,(u.y+tg.y)/2-8,u.team?'255,140,120':'200,230,255',3);
}

/* ================= AI tướng ================= */
function asgEye(u){return typeof u.asg==='number'?G.eyes[u.asg]:null;}
function bestClusterU(u,range,minN){let best=null,bn=minN-1;for(const e of G.units){if(e.dead||e.team===u.team||e===boss||!reachable(u,e)||dist(u,e)>range)continue;let n=0;for(const f of G.units){if(!f.dead&&f.team===e.team&&dist(e,f)<110)n++;}if(n>bn){bn=n;best=e;}}return best?{x:best.x,y:best.y}:null;}
function champGoal(u){
 if(u.team===0){
  if(G.phase===3||G.over)return{x:C.x+(u.id%3-1)*60,y:C.y+60,stand:40};
  if(G.phase===2){
   const r=Math.hypot(u.x-C.x,u.y-C.y);
   if(r>WALL_R-60)return Object.assign(openPt(WALL_R-200),{stand:60});
   if(!boss.dead)return{x:boss.x,y:boss.y,stand:boss.active?140:430};
   return{x:C.x,y:C.y,stand:200};
  }
  const ey=asgEye(u);if(!ey)return null;
  if(ey.dead)return Object.assign(openPt(BAR_R+120),{stand:60});
  if(WP[u.asg]&&dist(u,CAMP)<1000)return{x:WP[u.asg].x,y:WP[u.asg].y,stand:10};
  if(!ey.revealed)return{x:ey.hx,y:ey.hy,stand:150};
  return{x:ey.x,y:ey.y,stand:ey.vuln>0?60:(u.D.el===ey.weak?250:200)};
 }
 if(u.asg==='breach'&&G.open>=0){
  const deep=G.allies.some(a=>!a.dead&&Math.hypot(a.x-C.x,a.y-C.y)<WALL_R-300);
  return Object.assign(deep?polar(DIRS[G.open].a,IN_R+150):openPt(WALL_R-180),{stand:70});
 }
 if(typeof u.asg==='number')return Object.assign(postPt(u.asg),{stand:60});
 return{x:C.x,y:C.y+150,stand:80};
}
function champTarget(u){
 const T=TYPES[u.t];
 if(u.team===0){
  const ec=nearestEnemy(u,340,e=>e.champ&&reachable(u,e));if(ec)return ec;
  if(G.phase>=2){if(boss.active&&!boss.dead&&dist(u,boss)<520)return boss;return nearestEnemy(u,330,e=>e!==boss&&reachable(u,e));}
  const ey=asgEye(u);
  if(ey&&!ey.dead&&ey.revealed&&ey.vuln>0&&dist(u,ey)<520)return ey;
  const near=ey&&dist(u,ey)<900;
  return nearestEnemy(u,near?300:190,e=>e!==boss&&reachable(u,e));
 }
 const g=champGoal(u);
 const ec=nearestEnemy(u,380,e=>e.champ&&dist(e,g)<820);if(ec)return ec;
 return nearestEnemy(u,T.range>100?T.range+20:300,e=>dist(e,g)<720);
}
function healPulse(u,R,kc,kt,col,glyph){
 for(const a of G.units){if(a.dead||a.team!==u.team||dist(a,u)>R)continue;const k=a.champ||a===boss?kc:kt;if(a.hp>=a.max)continue;const add=a.max*k;a.hp=Math.min(a.max,a.hp+add);if(a.champ&&add>100)floatTxt(a.x,a.y-46,'+'+fmt(add),u.team?'#ffd07a':'#7fd18b',13);}
 G.fx.push({type:'ring',x:u.x,y:u.y,r0:20,r1:R,t:0.7,max:0.7,col});G.fx.push({type:'glyph',g:glyph,x:u.x,y:u.y-70,t:0.9,max:0.9,col});
 for(let i=0;i<18;i++){const a=RR(0,TAU),r=RR(20,R*0.8);G.fx.push({type:'p',x:u.x+Math.cos(a)*r,y:u.y+Math.sin(a)*r*0.6,vx:0,vy:RR(-60,-25),t:0.9,max:0.9,col,s:3});}
}
function champSkill(u,dt){
 const D=u.D;
 if(u.team===0){
  const ey=asgEye(u);
  if(G.phase===1&&ey&&!ey.dead&&!ey.revealed&&u.yanT<=0&&dist(u,ey)<520){u.yanT=10;G.waves.push({x:u.x,y:u.y,r:20,max:620,found:false,by:u});G.fx.push({type:'glyph',g:'眼',x:u.x,y:u.y-70,t:1,max:1,col:'230,210,255'});}
  if(u.key==='thanh'&&u.healT<=0){let need=0;for(const a of G.allies)if(!a.dead&&dist(a,u)<300&&a.hp<a.max*0.7)need+=3;for(const t of G.units)if(!t.dead&&t.team===0&&!t.champ&&t.hp<t.max*0.6&&dist(t,u)<260)need++;
   if(need>=4){u.healT=9;healPulse(u,300,0.22,0.5,'127,209,139','春');}}
  if(u.skT>0)return;
  let pt=null,eyeT=null;
  for(const e of G.eyes){if(e.dead||!e.revealed||dist(u,e)>430)continue;if((e.seals>0&&e.weak===D.el)||e.vuln>0){eyeT=e;break;}}
  if(eyeT)pt={x:eyeT.x,y:eyeT.y};
  else{const ec=nearestEnemy(u,380,e=>e.champ&&reachable(u,e));pt=ec?{x:ec.x,y:ec.y}:bestClusterU(u,380,4);}
  if(!pt||barBlock(u,pt))return;
  u.skT=D.cd*RR(0.92,1.08);u.anim=0.35;u.face=ang(u,pt);
  const an=D.tk==='an';
  G.teles.push({x:pt.x,y:pt.y,r:an?115:110,t:an?0.6:0.5,max:an?0.6:0.5,team:0,kind:D.tk,elem:D.el,src:u,cast:++G.castN,dmg:190});
  if(eyeT&&eyeT.seals>0)floatTxt(u.x,u.y-58,D.sk,`rgb(${ELEM[D.el].col})`,13);
  return;
 }
 if(u.key==='thi'){u.sumT-=dt;if(u.sumT<=0&&nearestEnemy(u,520)){u.sumT=15;let n=0;for(const t of G.units)if(!t.dead&&t.lead===u)n++;
  if(n<12){for(let i=0;i<4;i++){const a=RR(0,TAU);const s=addUnit('no',u.x+Math.cos(a)*60,u.y+Math.sin(a)*60,{role:'ret',lead:u,rise:1,stun:1});s.home={x:s.x,y:s.y};G.fx.push({type:'ring',x:s.x,y:s.y,r0:4,r1:34,t:0.6,max:0.6,col:'90,200,120'});}G.totalEnemy+=4;floatTxt(u.x,u.y-58,'Triệu Thi','#8fe0a0',14);}}}
 if(u.key==='bo'){u.healT-=dt;if(u.healT<=0&&nearestEnemy(u,600)){u.healT=12;healPulse(u,280,0.15,0.4,'232,200,112','補');for(const e of G.eyes){if(!e.dead&&e.revealed&&dist(e,u)<420&&e.hp<e.max){e.hp=Math.min(e.max,e.hp+e.max*0.18);floatTxt(e.x,e.y-160,'Bổ Thiên Trận vá trận nhãn','#ffd07a',14);}}}}
 if(u.skT>0)return;
 const tg=nearestEnemy(u,360,e=>e.champ);const pt=tg?{x:tg.x,y:tg.y}:bestClusterU(u,340,4);
 if(!pt)return;
 u.skT=(u.key==='ta'?6.5:7.5)*RR(0.92,1.08);u.anim=0.4;u.face=ang(u,pt);
 if(u.key==='lang'&&tg&&dist(u,tg)>110&&regOf(u.pr)===regOf(tg.pr)){const a=ang(tg,u);for(let i=0;i<5;i++)G.fx.push({type:'ghost',x:u.x+(tg.x-u.x)*i/5,y:u.y+(tg.y-u.y)*i/5,a:u.face,t:0.3,max:0.3,col:'230,90,70'});u.x=tg.x+Math.cos(a)*55;u.y=tg.y+Math.sin(a)*55;}
 G.teles.push({x:pt.x,y:pt.y,r:u.key==='ta'?140:120,t:u.key==='lang'?0.55:0.95,max:u.key==='lang'?0.55:0.95,team:1,kind:'mtele',col:D.col,src:u,dmg:320,stun:u.key==='ta'?1.4:0,g:D.g});
}
function champAttack(u,tg){
 const T=TYPES[u.t];u.anim=0.3;
 const mult=tg.isEye?1.6:(tg.champ||tg===boss)?1:1.5;
 if(T.range>100){G.proj.push({kind:'bolt',x:u.x,y:u.y-16,tgt:tg,spd:520,dmg:T.atk*mult*RR(0.9,1.1),team:u.team,src:u,life:1.6,trail:[],col:u.team?u.D.col:ELEM[u.D.el].col,ob:u.pr>=BAR_R,big:true});return;}
 damage(tg,T.atk*mult*RR(0.9,1.1),u);
 G.fx.push({type:'slashC',x:u.x,y:u.y,a:u.face,t:0.18,max:0.18,col:u.team?u.D.col:ELEM[u.D.el].col});
}
function retreatPt(u){
 if(u.team===1)return{x:C.x+60,y:C.y+170};
 if(G.phase>=2&&G.open>=0)return openPt(BAR_R+260);
 const a=angC(u),r=Math.max(Math.hypot(u.x-C.x,u.y-C.y)+450,2650);const p=polar(a,r);return{x:clamp(p.x,120,WW-120),y:clamp(p.y,120,WH-120)};
}
function respawnChamp(u){u.dead=false;u.hp=u.max;u.retreat=false;u.stun=0;const p=G.open>=0?openPt(BAR_R+260):{x:CAMP.x+400,y:CAMP.y+(u.id%5-2)*50};u.x=p.x;u.y=p.y;u.pr=dist(u,C);pushFeed(nameOf(u)+' trở lại chiến trường');}
function updateChamp(u,dt){
 const T=TYPES[u.t];
 if(u.flash>0)u.flash-=dt;if(u.anim>0)u.anim-=dt;
 if(u.dead){if(u.team===0){u.respawn-=dt;if(u.respawn<=0)respawnChamp(u);}return;}
 u.skT-=dt;u.yanT-=dt;u.cd-=dt;if(u.team===0)u.healT-=dt;
 if(G.t-u.hitT>4)u.hp=Math.min(u.max,u.hp+u.max*0.012*dt);
 if(u.stun>0){u.stun-=dt;return;}
 u.moving=false;
 if(G.over){if(u.team===1)u.stun=9;else{const g=champGoal(u);if(g&&dist(u,g)>g.stand)moveTo(u,g,T.spd,dt);}return;}
 if(!u.retreat&&u.hp<u.max*0.22&&G.phase<3){u.retreat=true;pushFeed(nameOf(u)+' bị thương nặng, tạm lui');}
 if(u.retreat){if(u.hp>u.max*0.72)u.retreat=false;else{u.hp=Math.min(u.max,u.hp+u.max*0.03*dt);const p=retreatPt(u);if(dist(u,p)>40)moveTo(u,p,T.spd*1.1,dt);return;}}
 if(u.team===1&&u.reinf&&typeof u.asg==='number'){if(dist(u,G.eyes[u.asg])<950){u.reinf=false;const ey=G.eyes[u.asg],mu=u;cue({get x(){return mu.x*0.6+ey.x*0.4},get y(){return mu.y*0.6+ey.y*0.4}},4,'Hướng '+DIRS[u.asg].n+' · '+nameOf(u)+' đến chi viện',2);}}else if(u.team===1)u.reinf=false;
 champSkill(u,dt);
 u.retgt-=dt;if(u.retgt<=0){u.retgt=0.3;u.tgt=champTarget(u);}
 let tg=u.tgt;if(tg&&(tg.dead||(tg.isEye&&!(tg.vuln>0)))){tg=u.tgt=null;}
 if(tg){const d=dist(u,tg)-(tg.r||0)-u.r;if(d<=T.range){u.face=ang(u,tg);if(u.cd<=0){champAttack(u,tg);u.cd=T.cd*RR(0.9,1.1);}}else moveTo(u,tg,T.spd,dt);}
 else{const g=champGoal(u);if(g&&dist(u,g)>g.stand)moveTo(u,g,T.spd*(nearestEnemy(u,320)?1:1.3),dt);}
}

/* ================= quân sư hai phe ================= */
function dirName(d){return DIRS[d].n;}
function playerDir(){if(AUTO||hero.dead||Math.hypot(hero.x-C.x,hero.y-C.y)<BAR_R)return hero.asg;return secOf(angC(hero));}
function maAt(d){let n=0;const e=G.eyes[d];for(const m of G.foes){if(m===boss||m.dead)continue;if(m.asg===d||dist(m,e)<1000)n++;}return n;}
function champNear(d){const e=G.eyes[d];return G.allies.some(a=>!a.dead&&dist(a,e)<1100&&dist(a,CAMP)>650);}
function counterAlive(d){const w=EYE_DEF[d].weak;for(const a of G.allies){if(a===hero?(w==='hoa'||w==='tho'||w==='kim'):CH_DEF[a.key].el===w)return a;}return null;}
function planSet(kind,tgt,why,silent){
 for(const a of G.allies)a.asg=kind==='split'?SPLIT0[a.key]:tgt;
 G.plan={kind,tgt,t:G.t,why};
 let g,title,sub;
 if(kind==='split'){g='分';title='Phân binh ba hướng';sub='Viêm Dương, Thanh Mộc đánh Tây · Thạch Nham, Thủy Nguyệt đánh Bắc · Ngao Thiên một mình đánh Nam. Hướng Đông xa doanh trại, tạm bỏ';}
 else if(kind==='mass'){g='合';title='Hợp binh hướng '+dirName(tgt);sub=why;}
 else{g='破';title='Cửa '+dirName(tgt)+' đã mở · toàn quân dồn vào';sub=why;}
 G.plan.g=g;G.plan.title=title;G.plan.sub=sub;
 pushFeed(kind==='split'?'Quân sư: phân binh đánh Tây, Bắc, Nam':kind==='mass'?'Quân sư: năm tướng hợp binh hướng '+dirName(tgt):'Năm tướng dồn về cửa '+dirName(tgt));
 if(!silent)stratEvent(g,title,sub);
}
function cmdAtk(){
 if(G.over)return;
 if(G.open>=0){if(!G.plan||G.plan.kind!=='breach')planSet('breach',G.open,'Kết giới hướng '+dirName(G.open)+' sụp. Năm tướng theo cửa '+dirName(G.open)+' vào thành, nhắm thẳng nội thành');return;}
 if(G.mode==='mass'){const d=playerDir();if(!G.plan||G.plan.kind!=='mass'||G.plan.tgt!==d)planSet('mass',d,'Theo lệnh Ngao Thiên: bốn tướng còn lại kéo quân về hướng '+dirName(d));return;}
 if(G.mode==='split'){if(!G.plan||G.plan.kind!=='split')planSet('split',null,'');return;}
 if(!G.plan){planSet('split',null,'',true);return;}
 if(G.plan.kind!=='split'||G.t-G.plan.t<28)return;
 const dirs=[0,1,3];
 for(const d of dirs){const e=G.eyes[d];if(!e.dead&&e.revealed&&e.seals===0&&e.hp<e.max*0.8&&G.allies.some(a=>a.asg===d&&!a.dead)){planSet('mass',d,'Trận nhãn '+e.name+' hướng '+dirName(d)+' sắp vỡ. Các hướng khác rút quân dồn về đây');return;}}
 for(const d of dirs){const n=maAt(d);if(n<2)continue;
  let best=-1,bs=-1e9;for(const q of dirs){const e=G.eyes[q];if(e.dead||!counterAlive(q))continue;const prog=(e.revealed?1:0)+(e.revealed?e.maxSeals-e.seals:0);const s=pow(q,0,1100)-pow(q,1,900)+14*prog;if(s>bs){bs=s;best=q;}}
  if(best===d){planSet('mass',best,'Ma đạo dồn '+n+' tướng về hướng '+dirName(d)+'. Trận nhãn ở đây đã lung lay, năm tướng dồn về '+dirName(d)+' quyết chiến trước khi địch kịp vững');return;}
  if(best>=0){planSet('mass',best,'Ma đạo dồn '+n+' tướng giữ hướng '+dirName(d)+'. Tránh chỗ mạnh, năm tướng dồn đánh hướng '+dirName(best));return;}}
}
function cmdDef(){
 if(G.over)return;
 const ms=G.foes.filter(m=>m!==boss&&!m.dead);
 if(G.open>=0){let any=false;for(const m of ms)if(m.asg!=='breach'){m.asg='breach';any=true;}if(any)pushFeed('Ma tướng kéo về giữ cửa '+dirName(G.open));return;}
 if(G.t<22)return;
 for(const m of ms){
  if(m.retreat)continue;
  const own=m.D.post,thrOwn=pow(own,0,1100);
  if(m.asg!==own&&((champNear(own)&&thrOwn>=14)||thrOwn>=30)){m.asg=own;pushFeed(nameOf(m)+' quay về giữ hướng '+dirName(own));continue;}
  const cur=m.asg;if(champNear(cur)||pow(cur,0,1100)>=24)continue;
  const am=angC(m);let best=-1,bg=8;
  for(let d=0;d<4;d++){if(d===cur||G.eyes[d].dead)continue;const g=pow(d,0,1100)-pow(d,1,900)-angDiff(am,DIRS[d].a)*2300/350;if(g>bg){bg=g;best=d;}}
  if(best>=0&&m.want!==best){m.want=best;continue;}
  if(best>=0){m.want=-1;m.asg=best;m.reinf=true;const txt=nameOf(m)+' thấy hướng '+dirName(cur)+' yên ắng, chi viện hướng '+dirName(best);pushFeed(txt);
   const n=G.foes.filter(f=>f!==boss&&!f.dead&&f.asg===best).length;
   if(G.t-G.lastStrat>7)stratEvent('援',n>=3?'Ma đạo dồn '+n+' tướng về hướng '+dirName(best):'Ma đạo điều tướng chi viện',txt,1);else warn(txt);
   return;}
  if(cur!==own)m.asg=own;
 }
}
function stratEvent(g,title,sub,side){
 G.lastStrat=G.t;
 if(AUTO){if(G.strat&&G.strat.t>0.5){Object.assign(G.strat,{g,title,sub,side});G.strat.t=Math.max(G.strat.t,3);}else G.strat={t:4,max:4,g,title,sub,side};}
 else showBanner(title,sub,3.4,g);
}

function updateBoss(dt){
 const b=boss;if(b.dead)return;
 if(b.flash>0)b.flash-=dt;if(b.anim>0)b.anim-=dt;
 if(b.stun>0){b.stun-=dt;return;}
 if(G.over)return;
 if(!b.active){if(G.phase>=2&&G.allies.some(a=>!a.dead&&dist(b,a)<720)){b.active=true;$('boss').style.display='block';pushFeed('Huyết Sát Phó Tướng xuất trận');showBanner('Trấn thủ xuất trận','Huyết Sát Phó Tướng · Kết Đan sơ kỳ',2.2,'煞');}else return;}
 b.cd-=dt;b.cdT-=dt;b.cdK-=dt;b.moving=false;
 if(b.casting>0){b.casting-=dt;return;}
 let tg=null,bd=900;for(const a of G.allies){if(a.dead)continue;const d=dist(b,a)*(a===hero?0.85:1);if(d<bd){bd=d;tg=a;}}
 if(!tg)tg=nearestEnemy(b,900);
 if(!tg)return;
 if(tg.champ&&b.cdK<=0&&dist(b,tg)<420){b.cdK=15;G.zones.push({type:'khon',x:tg.x,y:tg.y,r:165,t:6,max:6});pushFeed('Huyết Sát Phó Tướng bày Khốn Long Trận vây '+nameOf(tg));if(tg===hero)warn('Khốn Long Trận: độn thuật bị chặn trong vùng');b.casting=0.5;b.anim=0.5;return;}
 if(b.cdT<=0&&dist(b,tg)<360){b.cdT=6;G.teles.push({x:tg.x,y:tg.y,r:120,t:1.15,max:1.15,team:1,kind:'tram',dmg:620});b.casting=0.7;b.anim=0.7;b.face=ang(b,tg);return;}
 const d=dist(b,tg)-tg.r-b.r;
 if(d<=TYPES.boss.range){b.face=ang(b,tg);if(b.cd<=0){b.cd=1.5;b.anim=0.3;for(const e of G.units){if(e.dead||e.team!==0)continue;if(dist(b,e)-e.r<b.r+52&&angDiff(ang(b,e),b.face)<1.1)damage(e,e.champ?110:RR(30,45),b);}G.fx.push({type:'slashR',x:b.x,y:b.y,a:b.face,t:0.22,max:0.22});}}
 else moveTo(b,tg,TYPES.boss.spd,dt);
}

/* ================= tự đánh ================= */
function autopilot(){
 const h=hero;IN.mx=0;IN.my=0;if(h.dead||G.intro>0)return;
 for(const t of G.teles){if(t.team===1&&dist(h,t)<t.r+h.r+8){const a=ang(t,h);const dir={x:Math.cos(a),y:Math.sin(a)};if(t.t<0.8&&castDon(dir))return;IN.mx=dir.x;IN.my=dir.y;return;}}
 const kz=inZone(h,'khon');
 if(kz){if(G.pha>0&&G.phaCd<=0&&kz.t<kz.max-0.6)usePha(kz);}
 if(h.hp<h.max*0.42)useDan();
 let goal=null,stand=62;
 const near=nearestEnemy(h,140,e=>reachable(h,e));
 const ech=nearestEnemy(h,270,e=>e.champ&&reachable(h,e));
 const resL=!G.eyes[0].dead&&G.phase===1?2:0,resA=!G.eyes[1].dead&&G.phase===1?2:0;
 if(G.phase===1){
  const e=G.eyes[h.asg];
  if(e&&!e.dead&&WP[h.asg]&&dist(h,CAMP)<1000){goal={x:WP[h.asg].x,y:WP[h.asg].y,r:0};stand=10;}
  else if(e&&!e.dead){const d=dist(h,e);
   if(!e.revealed){
    if(d<560&&h.cd2.yan<=0&&h.mp>=30)castYan();
    if(ech&&d<900)goal=ech;else if(near&&d<800)goal=near;else{goal=e;stand=d<560&&G.waves.length?9999:240;}
   }else if(e.seals>0){
    if(e.weak==='hoa'&&d<430)castLoi({x:e.x,y:e.y});
    else if(e.weak==='kim'&&d<450)castYu();
    else if(e.weak==='tho'&&d<430)castAn({x:e.x,y:e.y});
    const pend=G.teles.some(t=>t.team===0&&(t.kind==='an'||t.kind==='loi'||t.kind==='ice'||t.kind==='fire'))||G.proj.some(p=>p.kind==='sword'&&p.tgt===e);
    const aiCtr=G.allies.some(a=>a!==h&&!a.dead&&CH_DEF[a.key].el===e.weak&&dist(a,e)<700);
    const noCounter=!pend&&!aiCtr&&((e.weak==='hoa'&&G.loi<=0)||(e.weak==='tho'&&G.an<=0)||e.weak==='thuy');
    if(G.phaCd<=0&&d<350&&noCounter&&G.pha>0)usePha(e);
    if(ech)goal=ech;else{const n3=nearestEnemy(h,320,q=>reachable(h,q));if(n3)goal=n3;else{goal=e;stand=170;}}
   }else{goal=e;stand=e.r+46;castYu();
    if(d<350&&G.pha>1&&G.phaCd<=0&&e.vuln<3.5&&e.hp>e.max*0.35)usePha(e);
    if(d<430&&G.loi>(h.asg===0?0:1))castLoi({x:e.x,y:e.y});
    if(d<430&&G.an>(h.asg===1?0:1))castAn({x:e.x,y:e.y});}
  }else goal=near||Object.assign(openPt(BAR_R+120),{r:0});
 }else if(G.phase===2){
  const outside=Math.hypot(h.x-C.x,h.y-C.y)>WALL_R-40;
  if(boss.active&&!boss.dead)goal=(near&&dist(h,boss)>420)?near:boss;
  else if(ech&&!outside)goal=ech;
  else if(outside){goal=near&&dist(near,h)<120?near:Object.assign(openPt(WALL_R-220),{r:0});stand=30;}
  else if(!boss.dead){const n3=nearestEnemy(h,300);const cn=G.allies.filter(a=>a!==h&&!a.dead&&dist(a,h)<750).length;
   if(n3)goal=n3;else if(cn<2&&!boss.active){goal=null;h.face=Math.atan2(boss.y-h.y,boss.x-h.x);}else goal=boss;}
  else{goal=near||nearestEnemy(h,1500)||{x:C.x,y:C.y,r:0};}
  if(!boss.dead&&boss.active&&dist(h,boss)<330&&G.hoCd<=0&&!inZone(h,'ho'))placeHo();
 }else{
  if(near&&dist(near,C)<FLAG_R+150)goal=near;else if(dist(h,C)>FLAG_R*0.45){goal={x:C.x,y:C.y,r:0};stand=8;}else goal=near;
 }
 const reserve=(G.phase===2&&!boss.dead&&h.cd2.van<6)?160:0;
 if(inZone(h,'ho')&&h.cd2.van<=0&&h.mp>=160){const n=countEnemies(h,260);if(n>=5||(boss.active&&!boss.dead&&dist(h,boss)<240))castVan();}
 if(h.cd2.yu<=0&&h.mp>=40+reserve+(G.phase===1?30:0))castYu();
 if(h.cd2.loi<=0&&G.loi>resL){const pt=bestCluster(h,380,G.phase===1?4:5);if(pt)castLoi(pt);}
 if(G.anCd<=0&&G.an>resA){const pt=bestCluster(h,380,5);if(pt)castAn(pt);}
 if(kz){const a=ang(kz,h);if(dist(h,kz)<kz.r+20){IN.mx=Math.cos(a);IN.my=Math.sin(a);}}
 else if(goal){const d=dist(h,goal)-(goal.r||0);if(d>stand){const wp=navPoint(h,goal);const a=ang(h,wp);IN.mx=Math.cos(a);IN.my=Math.sin(a);}}
 const any=nearestEnemy(h,105,e=>reachable(h,e));const ve=G.eyes.find(q=>q.revealed&&!q.dead&&q.vuln>0&&dist(h,q)-q.r<100);
 if(any||ve)castSlash();
}
