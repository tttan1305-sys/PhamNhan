function drawChamp(u,x,y){
 const D=u.D,E=ELEM[D.el],t=G.t,f=u.face,cf=Math.cos(f),sf=Math.sin(f),ma=u.team===1;
 const big=(u.key==='thach'||u.key==='bo'||u.key==='lang')?1.12:1;
 const pul=0.5+0.5*Math.sin(t*3+u.id);
 ctx.fillStyle='rgba(0,0,0,.35)';ell(x,y+8,16*big,6);ctx.fill();
 ctx.strokeStyle=`rgba(${E.col},${0.4+pul*0.35})`;ctx.lineWidth=2;ell(x,y+8,(21+pul*3)*big,8+pul);ctx.stroke();
 ctx.strokeStyle=ma?`rgba(229,96,77,${0.35+pul*0.2})`:`rgba(95,179,217,${0.35+pul*0.2})`;ctx.lineWidth=1.2;ell(x,y+8,(27+pul*3)*big,10.5+pul);ctx.stroke();
 ctx.save();ctx.translate(x,y);ctx.scale(big,big);
 const sw=u.anim>0?Math.sin(clamp(u.anim/0.35,0,1)*Math.PI):0;
 const hx=cf*8,hy=-6+sf*5;
 const wpn=()=>{
  switch(D.w){
   case 'staff':{const a=f+0.5-sw*1.0;const tx=hx+Math.cos(a)*30,ty=hy+Math.sin(a)*22-16;ctx.strokeStyle='#4a3420';ctx.lineWidth=2.6;ctx.beginPath();ctx.moveTo(hx-Math.cos(a)*8,hy-Math.sin(a)*6+6);ctx.lineTo(tx,ty);ctx.stroke();const og=ctx.createRadialGradient(tx,ty,0,tx,ty,11);og.addColorStop(0,`rgba(${u.team?D.col:E.col},.95)`);og.addColorStop(1,`rgba(${u.team?D.col:E.col},0)`);ctx.fillStyle=og;circ(tx,ty,11);ctx.fill();ctx.fillStyle='#fff3d0';circ(tx,ty,2.6);ctx.fill();break;}
   case 'shield':{ctx.save();ctx.translate(hx+cf*4,hy+sf*3+2);ctx.rotate(f);ctx.fillStyle='#4e3c22';ctx.fillRect(-3,-13,8,26);ctx.strokeStyle=D.trim;ctx.lineWidth=1.6;ctx.strokeRect(-3,-13,8,26);ctx.restore();const a=f-0.9+sw*1.8;ctx.strokeStyle='#3a2a18';ctx.lineWidth=3;ctx.beginPath();ctx.moveTo(-cf*4,-6);ctx.lineTo(-cf*4+Math.cos(a)*22,-6+Math.sin(a)*18);ctx.stroke();ctx.fillStyle='#8a7a60';circ(-cf*4+Math.cos(a)*22,-6+Math.sin(a)*18,4.5);ctx.fill();break;}
   case 'orb':case 'orbG':{const col=D.w==='orbG'?'120,230,140':E.col;const ox=cf*16,oy=-14+sf*7+Math.sin(t*3+u.id)*3;const og=ctx.createRadialGradient(ox,oy,0,ox,oy,12);og.addColorStop(0,`rgba(${col},.9)`);og.addColorStop(1,`rgba(${col},0)`);ctx.fillStyle=og;circ(ox,oy,12);ctx.fill();ctx.fillStyle=D.w==='orbG'?'#d8ffd8':'#e8f6ff';circ(ox,oy,3.4);ctx.fill();break;}
   case 'spear':{const a=f-0.3+sw*0.6;const L=40+sw*10;ctx.strokeStyle='#3a2a18';ctx.lineWidth=2.2;ctx.beginPath();ctx.moveTo(hx-Math.cos(a)*14,hy-Math.sin(a)*10);ctx.lineTo(hx+Math.cos(a)*L,hy+Math.sin(a)*L*0.8);ctx.stroke();ctx.fillStyle='#e8eef0';const px=hx+Math.cos(a)*L,py=hy+Math.sin(a)*L*0.8;ctx.beginPath();ctx.moveTo(px+Math.cos(a)*9,py+Math.sin(a)*7);ctx.lineTo(px-Math.sin(a)*3,py+Math.cos(a)*3);ctx.lineTo(px+Math.sin(a)*3,py-Math.cos(a)*3);ctx.closePath();ctx.fill();break;}
   case 'claw':{ctx.strokeStyle='#e0d6cc';ctx.lineWidth=1.6;for(const s of [-1,1]){const bx=hx+s*sf*6,by=hy-s*cf*4;const a=f+s*0.25-sw*0.8*s;for(let k=-1;k<=1;k++){ctx.beginPath();ctx.moveTo(bx,by);ctx.lineTo(bx+Math.cos(a+k*0.18)*14,by+Math.sin(a+k*0.18)*11);ctx.stroke();}}break;}
   case 'blade':{const a=f+0.6-sw*1.6;ctx.strokeStyle=`rgba(${D.col},.5)`;ctx.lineWidth=6;ctx.beginPath();ctx.arc(hx,hy,16,a-0.9,a+0.9);ctx.stroke();ctx.strokeStyle='#ffe0f6';ctx.lineWidth=2;ctx.stroke();break;}
  }};
 if(sf<0)wpn();
 if(D.hat==='long'){ctx.fillStyle='#14100c';ctx.beginPath();ctx.moveTo(-7,-20);ctx.quadraticCurveTo(-10-cf*4,-2,-6-cf*3,4);ctx.lineTo(6-cf*3,4);ctx.quadraticCurveTo(10-cf*4,-2,7,-20);ctx.closePath();ctx.fill();}
 ctx.fillStyle=D.cape;ctx.beginPath();ctx.moveTo(-12,8);ctx.quadraticCurveTo(-16-cf*6,-6,-6,-14);ctx.lineTo(6,-14);ctx.quadraticCurveTo(16-cf*6,-6,12,8);ctx.closePath();ctx.fill();
 ctx.fillStyle=u.flash>0?'#fff':D.robe;ctx.beginPath();ctx.moveTo(-11,7);ctx.quadraticCurveTo(-11,-10,0,-13);ctx.quadraticCurveTo(11,-10,11,7);ctx.closePath();ctx.fill();
 ctx.strokeStyle=D.trim;ctx.lineWidth=1.4;ctx.stroke();
 ctx.fillStyle=D.trim;ctx.fillRect(-10,-3,20,2.6);
 if(D.hat==='hood'){ctx.fillStyle=D.cape;circ(0,-19,8.6);ctx.fill();ctx.fillStyle=D.skin;ell(cf*2,-17,4.6,5);ctx.fill();}
 else{ctx.fillStyle=D.skin||'#ecd6b4';circ(0,-18,6.3);ctx.fill();}
 switch(D.hat){
  case 'crown':ctx.fillStyle='#14100c';ctx.beginPath();ctx.arc(0,-19,6.6,Math.PI*1.05,Math.PI*1.95);ctx.fill();ctx.fillStyle='#e8c060';ctx.beginPath();ctx.moveTo(-5,-24);ctx.lineTo(-3,-30);ctx.lineTo(0,-26);ctx.lineTo(3,-30);ctx.lineTo(5,-24);ctx.closePath();ctx.fill();break;
  case 'bun':ctx.fillStyle='#14100c';ctx.beginPath();ctx.arc(0,-19,6.6,Math.PI*1.05,Math.PI*1.95);ctx.fill();circ(0,-27,3.4);ctx.fill();ctx.strokeStyle=D.trim;ctx.lineWidth=1.4;ctx.beginPath();ctx.moveTo(-4,-28);ctx.lineTo(4,-28);ctx.stroke();break;
  case 'long':ctx.fillStyle='#14100c';ctx.beginPath();ctx.arc(0,-19,6.8,Math.PI*1.0,Math.PI*2.0);ctx.fill();ctx.fillStyle=D.trim;circ(3,-25,1.8);ctx.fill();break;
  case 'horns':ctx.fillStyle='#1c2a1c';ctx.beginPath();ctx.arc(0,-19,6.6,Math.PI*1.05,Math.PI*1.95);ctx.fill();ctx.strokeStyle='#e8c870';ctx.lineWidth=2;ctx.beginPath();ctx.moveTo(-3,-24);ctx.quadraticCurveTo(-8,-30,-6,-35);ctx.moveTo(3,-24);ctx.quadraticCurveTo(8,-30,6,-35);ctx.stroke();break;
  case 'ears':ctx.fillStyle='#3a3230';ctx.beginPath();ctx.arc(0,-19,6.8,Math.PI*1.0,Math.PI*2.0);ctx.fill();ctx.beginPath();ctx.moveTo(-6,-22);ctx.lineTo(-7,-32);ctx.lineTo(-2,-25);ctx.closePath();ctx.moveTo(6,-22);ctx.lineTo(7,-32);ctx.lineTo(2,-25);ctx.closePath();ctx.fill();break;
  case 'halo':ctx.fillStyle='#14100c';ctx.beginPath();ctx.arc(0,-19,6.6,Math.PI*1.05,Math.PI*1.95);ctx.fill();ctx.strokeStyle=`rgba(232,200,112,${0.6+0.3*pul})`;ctx.lineWidth=1.8;ell(0,-30,9,3);ctx.stroke();break;
 }
 if(ma){ctx.fillStyle='#ff5a4a';ctx.fillRect(cf*2-3,-19,1.8,1.6);ctx.fillRect(cf*2+1,-19,1.8,1.6);}
 if(sf>=0)wpn();
 ctx.restore();
 const ly=y-44*big-(D.hat==='horns'?4:0);const nm=nameOf(u);
 ctx.font='600 11px "Be Vietnam Pro"';ctx.textAlign='center';ctx.lineWidth=3;ctx.strokeStyle='rgba(0,0,0,.85)';ctx.strokeText(nm,x+7,ly);ctx.fillStyle=ma?'#ffb3a8':'#cfeeff';ctx.fillText(nm,x+7,ly);
 const tw=ctx.measureText(nm).width;ctx.font='14px "Ma Shan Zheng"';ctx.strokeText(D.g,x-tw/2-3,ly+1);ctx.fillStyle=`rgb(${E.col})`;ctx.fillText(D.g,x-tw/2-3,ly+1);
 hpBar(x-20,ly+5,40,4,u.hp/u.max,ma?'#e5604d':'#5fb3d9');
 if(u.stun>0){ctx.fillStyle='rgba(190,225,255,.9)';for(let i=0;i<3;i++){const a=t*6+i*2.1;circ(x+Math.cos(a)*10,y-34+Math.sin(a)*3,1.8);ctx.fill();}}
 else if(u.retreat){ctx.font='500 10px "Be Vietnam Pro"';ctx.strokeText('tạm lui',x,ly-12);ctx.fillStyle='#e6d6b0';ctx.fillText('tạm lui',x,ly-12);}
}
function drawBarrier(vis){
 if(!G.barrier)return;
 const life=G.eyes.filter(p=>!p.dead).length/4;
 for(let d=0;d<4;d++){if(G.sec[d])continue;const a0=DIRS[d].a-SEC_H,a1=DIRS[d].a+SEC_H;
  const mid=polar(DIRS[d].a,BAR_R);const chord=2*BAR_R*Math.sin(SEC_H);if(!vis(mid.x,mid.y,chord*0.75+200))continue;
  ctx.beginPath();ctx.arc(C.x,C.y,BAR_R,a0,a1);ctx.arc(C.x,C.y,BAR_R*0.84,a1,a0,true);ctx.closePath();
  const g=ctx.createRadialGradient(C.x,C.y,BAR_R*0.84,C.x,C.y,BAR_R);g.addColorStop(0,'rgba(200,80,140,0)');g.addColorStop(0.8,`rgba(200,80,140,${0.05+0.05*life})`);g.addColorStop(1,`rgba(255,120,170,${0.15+0.1*life})`);ctx.fillStyle=g;ctx.fill();
  ctx.strokeStyle=`rgba(255,140,180,${0.28+0.2*life})`;ctx.lineWidth=10;ctx.beginPath();ctx.arc(C.x,C.y,BAR_R,a0,a1);ctx.stroke();
  ctx.strokeStyle=`rgba(255,200,220,${0.6+0.3*life})`;ctx.lineWidth=2;ctx.stroke();
  for(let i=0;i<2;i++){const a=a0+((G.t*(0.08+i*0.05)+i*0.5+d*0.3)%1)*(a1-a0-0.12);ctx.strokeStyle='rgba(255,220,235,.55)';ctx.lineWidth=3;ctx.beginPath();ctx.arc(C.x,C.y,BAR_R,a,a+0.12);ctx.stroke();}
  for(const [ae,nb] of [[a0,secOf(a0-0.05)],[a1,secOf(a1+0.05)]]){if(!G.sec[nb])continue;const p=polar(ae,BAR_R);const gg=ctx.createRadialGradient(p.x,p.y,0,p.x,p.y,46);gg.addColorStop(0,'rgba(255,210,230,.8)');gg.addColorStop(1,'rgba(255,140,190,0)');ctx.fillStyle=gg;circ(p.x,p.y,46);ctx.fill();}
 }
 ctx.font='20px "Ma Shan Zheng"';ctx.textAlign='center';ctx.textBaseline='middle';const runes='天地玄黃宇宙洪荒日月盈昃辰宿列張寒來暑往秋收冬藏閏餘成歲律呂調陽';
 for(let i=0;i<160;i++){const a=G.t*0.03+i*TAU/160;if(G.sec[secOf(a)])continue;const x=C.x+Math.cos(a)*(BAR_R-18),y=C.y+Math.sin(a)*(BAR_R-18);if(!vis(x,y,20))continue;ctx.fillStyle=`rgba(255,170,200,${0.25+0.25*Math.sin(G.t*2+i)})`;ctx.fillText(runes[i%runes.length],x,y);}
 ctx.textBaseline='alphabetic';
}
/* ---- sa bàn thế trận ---- */
function champTgtPt(c){
 if(c.dead)return null;
 if(c.team===0){
  if(G.phase>=2&&G.open>=0){if(Math.hypot(c.x-C.x,c.y-C.y)>WALL_R-60)return openPt(WALL_R-200);return boss.dead?C:boss;}
  const e=asgEye(c);return e?(e.revealed?e:{x:e.hx,y:e.hy}):null;}
 return champGoal(c);
}
function stratArrow(x0,y0,x1,y1,col,cx,cy,w){
 const L=Math.hypot(x1-x0,y1-y0);if(L<16)return;
 const mx=(x0+x1)/2,my=(y0+y1)/2;let nx=mx-cx,ny=my-cy;const nl=Math.hypot(nx,ny)||1;const k=Math.min(L*0.35,70);const qx=mx+nx/nl*k,qy=my+ny/nl*k;
 ctx.save();ctx.lineCap='round';ctx.strokeStyle=`rgba(${col},.3)`;ctx.lineWidth=w+6;ctx.beginPath();ctx.moveTo(x0,y0);ctx.quadraticCurveTo(qx,qy,x1,y1);ctx.stroke();
 ctx.setLineDash([10,7]);ctx.lineDashOffset=-G.t*34;ctx.strokeStyle=`rgba(${col},.95)`;ctx.lineWidth=w;ctx.stroke();ctx.setLineDash([]);
 const ta=Math.atan2(y1-qy,x1-qx);ctx.fillStyle=`rgb(${col})`;ctx.beginPath();ctx.moveTo(x1+Math.cos(ta)*5,y1+Math.sin(ta)*5);ctx.lineTo(x1-Math.cos(ta-0.45)*14,y1-Math.sin(ta-0.45)*14);ctx.lineTo(x1-Math.cos(ta+0.45)*14,y1-Math.sin(ta+0.45)*14);ctx.closePath();ctx.fill();ctx.restore();
}
function stratBadge(c,x,y){const D=CH_DEF[c.key],E=ELEM[D.el],ma=c.team===1;const r=c===boss?13:11;
 ctx.fillStyle='rgba(12,10,8,.92)';circ(x,y,r);ctx.fill();ctx.strokeStyle=ma?'#e5604d':(c===hero?'#f3d9a0':'#5fb3d9');ctx.lineWidth=2.2;circ(x,y,r);ctx.stroke();
 ctx.font=(r+4)+'px "Ma Shan Zheng"';ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillStyle=`rgb(${E.col})`;ctx.fillText(D.g,x,y+1);ctx.textBaseline='alphabetic';
 SB_L.push({c,x,y:y+r+12,ma});}
let SB_L=[];
function stratLabels(){
 ctx.font='600 10px "Be Vietnam Pro"';ctx.lineWidth=3;ctx.strokeStyle='rgba(0,0,0,.9)';ctx.textAlign='center';
 const pri=l=>l.c===hero?0:l.c===boss?1:l.ma?2:3;SB_L.sort((p,q)=>pri(p)-pri(q));
 const placed=[];
 for(const l of SB_L){const nm=nameOf(l.c),w=ctx.measureText(nm).width+4;const r={x0:l.x-w/2,x1:l.x+w/2,y0:l.y-10,y1:l.y+3};
  if(placed.some(p=>r.x0<p.x1&&r.x1>p.x0&&r.y0<p.y1&&r.y1>p.y0))continue;placed.push(r);
  ctx.strokeText(nm,l.x,l.y);ctx.fillStyle=l.ma?'#ffb3a8':(l.c===hero?'#f3d9a0':'#cfeeff');ctx.fillText(nm,l.x,l.y);}
 SB_L=[];
}
function wrapTxt(s,maxW){const w=s.split(' ');const lines=[];let cur='';for(const t of w){const n=cur?cur+' '+t:t;if(ctx.measureText(n).width>maxW&&cur){lines.push(cur);cur=t;}else cur=n;}if(cur)lines.push(cur);return lines;}
function champStatus(c){
 const ally=c.team===0;
 if(c.dead)return ally?'trọng thương · hồi sau '+Math.ceil(c===hero?c.respawn:c.respawn)+'s':'đã ngã';
 if(c.retreat)return 'tạm lui dưỡng thương';
 if(ally){if(G.phase===1)return '→ đánh hướng '+DIRS[c.asg].n;if(G.phase===2)return 'công vào nội thành';return 'giữ đài trung tâm';}
 if(c===boss)return boss.active?'xuất trận':'trấn giữ nội thành';
 if(c.asg==='breach')return 'giữ cửa '+DIRS[G.open].n;
 return c.asg===c.D.post?'giữ hướng '+DIRS[c.asg].n:'chi viện hướng '+DIRS[c.asg].n;
}
function drawStrat(a,S0){
 const S=VH/WH,ox=(VW-WW*S)/2;const X=x=>ox+x*S,Y=y=>y*S;const cx=X(C.x),cy=Y(C.y);
 ctx.save();ctx.globalAlpha=a;
 ctx.fillStyle='#0e0c09';ctx.fillRect(0,0,VW,VH);
 ctx.drawImage(OV,ox,0,WW*S,WH*S);
 ctx.fillStyle='rgba(30,22,12,.34)';ctx.fillRect(ox,0,WW*S,VH);
 ctx.strokeStyle='rgba(217,180,106,.55)';ctx.lineWidth=2;ctx.strokeRect(ox+1,1,WW*S-2,VH-2);
 const BR=BAR_R*S;
 for(let d=0;d<4;d++){const a0=DIRS[d].a-SEC_H,a1=DIRS[d].a+SEC_H;
  if(G.barrier&&!G.sec[d]){ctx.strokeStyle='rgba(255,140,190,.35)';ctx.lineWidth=8;ctx.beginPath();ctx.arc(cx,cy,BR,a0+0.012,a1-0.012);ctx.stroke();ctx.strokeStyle='rgba(255,205,228,.95)';ctx.lineWidth=2;ctx.stroke();}
  else{ctx.save();ctx.setLineDash([5,6]);ctx.strokeStyle='rgba(210,200,180,.55)';ctx.lineWidth=1.5;ctx.beginPath();ctx.arc(cx,cy,BR,a0,a1);ctx.stroke();ctx.restore();}}
 for(const e of G.eyes){const x=X(e.x),y=Y(e.y),E=ELEM[e.el];
  if(e.dead){ctx.strokeStyle='#bbb';ctx.lineWidth=2.5;ctx.beginPath();ctx.moveTo(x-7,y-7);ctx.lineTo(x+7,y+7);ctx.moveTo(x+7,y-7);ctx.lineTo(x-7,y+7);ctx.stroke();}
  else{ctx.fillStyle='rgba(12,10,8,.85)';circ(x,y,12);ctx.fill();ctx.strokeStyle=e.revealed?`rgb(${E.col})`:'rgba(205,170,255,.9)';ctx.lineWidth=2;circ(x,y,12);ctx.stroke();ctx.font='15px "Ma Shan Zheng"';ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillStyle=e.revealed?`rgb(${E.col})`:'#dcc6ff';ctx.fillText(e.revealed?E.g:'眼',x,y+1);ctx.textBaseline='alphabetic';}
  const lp=polar(e.a,e.r+430);const lx=clamp(X(lp.x),ox+60,ox+WW*S-60),ly=clamp(Y(lp.y),16,VH-10);
  ctx.font='600 12px "Be Vietnam Pro"';ctx.textAlign='center';ctx.lineWidth=3;ctx.strokeStyle='rgba(0,0,0,.85)';const s='Hướng '+DIRS[e.i].n+' · '+(e.dead?'đã mở':'nhãn '+E.n);ctx.strokeText(s,lx,ly);ctx.fillStyle=e.dead?'#c9e6b0':'#f3d9a0';ctx.fillText(s,lx,ly);}
 for(const u of G.units){if(u.dead||u.champ||u===boss)continue;ctx.fillStyle=u.team?'rgba(235,100,80,.9)':'rgba(130,205,245,.95)';ctx.fillRect(X(u.x)-1.3,Y(u.y)-1.3,2.6,2.6);}
 for(const m of G.foes){if(m===boss||m.dead)continue;const tp=champGoal(m);if(dist(m,tp)<260)continue;stratArrow(X(m.x),Y(m.y),X(tp.x),Y(tp.y),'235,95,80',cx,cy,2.6);}
 for(const c of G.allies){const tp=champTgtPt(c);if(!tp||dist(c,tp)<220)continue;stratArrow(X(c.x),Y(c.y),X(tp.x),Y(tp.y),'120,200,245',cx,cy,3);}
 SB_L=[];for(const c of G.foes)if(!c.dead)stratBadge(c,X(c.x),Y(c.y));
 for(const c of G.allies)if(!c.dead)stratBadge(c,X(c.x),Y(c.y));
 stratLabels();
 // tiêu đề
 const T=S0||{};const tw=WW*S-40;
 ctx.fillStyle='rgba(12,10,8,.82)';ctx.fillRect(ox+20,14,tw,T.sub?82:52);ctx.strokeStyle='rgba(217,180,106,.5)';ctx.lineWidth=1;ctx.strokeRect(ox+20,14,tw,T.sub?82:52);
 ctx.font='34px "Ma Shan Zheng"';ctx.textAlign='left';ctx.textBaseline='middle';ctx.fillStyle=T.side?'#e5604d':'#d9b46a';ctx.fillText(T.g||'謀',ox+34,41);
 ctx.font='700 21px "Noto Serif"';ctx.fillStyle='#f3d9a0';ctx.fillText(T.title||'Sa bàn thế trận',ox+76,40);
 ctx.font='600 10px "Be Vietnam Pro"';ctx.textAlign='right';ctx.fillStyle='rgba(217,180,106,.8)';ctx.fillText('SA BÀN THẾ TRẬN',ox+20+tw-12,30);
 if(T.sub){ctx.textAlign='left';ctx.font='500 12.5px "Be Vietnam Pro"';ctx.fillStyle='#e8dcc4';const ls=wrapTxt(T.sub,tw-30).slice(0,2);ls.forEach((l,i)=>ctx.fillText(l,ox+34,66+i*17));}
 ctx.textBaseline='alphabetic';
 // hai cột tướng
 const col=(list,x0,head,hc,ally)=>{ctx.textAlign='left';ctx.font='700 12px "Be Vietnam Pro"';ctx.fillStyle=hc;ctx.fillText(head,x0,128);ctx.fillStyle=hc;ctx.fillRect(x0,136,200,1.5);
  list.forEach((c,i)=>{const y=168+i*62,D=CH_DEF[c.key],E=ELEM[D.el];ctx.globalAlpha=a*(c.dead?0.5:1);
   ctx.fillStyle='rgba(12,10,8,.9)';circ(x0+16,y,15);ctx.fill();ctx.strokeStyle=ally?(c===hero?'#f3d9a0':'#5fb3d9'):'#e5604d';ctx.lineWidth=2;circ(x0+16,y,15);ctx.stroke();
   ctx.font='19px "Ma Shan Zheng"';ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillStyle=`rgb(${E.col})`;ctx.fillText(D.g,x0+16,y+1);ctx.textBaseline='alphabetic';ctx.textAlign='left';
   ctx.font='700 13.5px "Noto Serif"';ctx.fillStyle=ally?'#e6f4ff':'#ffd6cf';ctx.fillText(nameOf(c),x0+40,y-6);
   ctx.font='500 11px "Be Vietnam Pro"';ctx.fillStyle='#bfb092';ctx.fillText(E.n+' · '+D.sect,x0+40,y+9);
   ctx.font='600 11px "Be Vietnam Pro"';ctx.fillStyle=ally?'#9fdcff':'#ff9c90';ctx.fillText(champStatus(c),x0+40,y+24);
   ctx.fillStyle='rgba(0,0,0,.6)';ctx.fillRect(x0+40,y+29,150,4);ctx.fillStyle=ally?'#5fb3d9':'#e5604d';ctx.fillRect(x0+40,y+29,150*clamp(c.hp/c.max,0,1),4);ctx.globalAlpha=a;});};
 col(G.allies,14,'CHÍNH ĐẠO · NĂM TƯỚNG','#8fd0ef',true);
 col(G.foes,VW-ox+14,'MA ĐẠO · NĂM TƯỚNG','#ff8c84',false);
 ctx.restore();
}
