/* ================= HUD ================= */
const mm=$('mmc'),mx=mm.getContext('2d');let frame=0,lastHint='';
const MS=Math.min(202/WW,170/WH),MOX=(202-WW*MS)/2,MOY=(170-WH*MS)/2;
function cdSet(el,left,tot,num){const cdv=el.querySelector('.cdv'),cdn=el.querySelector('.cdn');if(left>0){const k=clamp(left/tot,0,1);cdv.style.background=`conic-gradient(rgba(8,6,4,.72) ${k*360}deg, rgba(0,0,0,0) 0)`;if(cdn)cdn.textContent=num?Math.ceil(left):'';}else{cdv.style.background='none';if(cdn)cdn.textContent='';}}
function hintText(){
 const h=hero;
 if(G.phase===1){
  let e=null,bd=1e9;for(const q of liveEyes()){const d=dist(h,q);if(d<bd){bd=d;e=q;}}
  if(e&&bd<850){const E=ELEM[e.el],W=ELEM[e.weak];
   if(e.seals>0)return `<b style="color:rgb(${E.col})">Trận nhãn ${E.n}</b> hướng ${DIRS[e.i].n} còn ${e.seals} ấn. ${W.n} khắc ${E.n}: ${e.tool}`;
   return `<b style="color:#ffd36b">Lõi trận nhãn ${E.n} lộ ra</b>: chém và dồn kỹ năng trong ${Math.ceil(e.vuln)} giây trước khi ấn tái lập`;}
  if(G.eyes.some(q=>!q.dead&&!q.revealed&&!h.dead&&dist(h,q)<500))return 'Linh khí dao động quanh đây: dùng <b><span class="g">眼</span> Phá Vọng Nhãn (4)</b> để trận nhãn ẩn hiện hình';
  const p=G.plan,d=h.asg;
  return `Bốn trận nhãn ẩn nuôi bốn cung kết giới, phá một cái là mở một cửa. Kế: <b>${p&&p.kind==='mass'?'hợp binh hướng '+DIRS[p.tgt].n:'phân binh ba hướng'}</b> · Ngao Thiên đánh hướng <b>${DIRS[d].n}</b>`;
 }
 if(G.phase===2){if(!boss.dead)return `Qua <b>cửa ${DIRS[G.open].n}</b>, theo đại lộ vào nội thành hạ <b>Huyết Sát Phó Tướng</b>`;return 'Trấn thủ đã ngã. Diệt nốt thủ quân trong các khu phố';}
 if(G.phase===3)return 'Một tướng Chính đạo đứng trên <b>đài trung tâm</b>, không để quân địch tranh chấp';
 return '';
}
function teamRow(c,ally){const D=CH_DEF[c.key],E=ELEM[D.el];let st;
 if(c.dead)st=ally?'hồi '+Math.ceil(c.respawn)+'s':'đã ngã';
 else if(c.retreat)st='tạm lui';
 else if(ally)st=G.phase===1?'→ '+DIRS[c.asg].n:(G.phase===2?'công thành':'đài trung tâm');
 else if(c===boss)st=boss.active?'xuất trận':'nội thành';
 else st=c.asg==='breach'?'giữ cửa '+DIRS[G.open].n:(c.asg===D.post?'giữ '+DIRS[D.post].n:'viện '+DIRS[c.asg].n);
 const k=c.dead?0:clamp(c.hp/c.max,0,1);
 return `<div class="tm${c.dead?' dead':''}"><span class="g" style="color:rgb(${E.col})">${D.g}</span><b>${nameOf(c)}${c===hero?' (ta)':''}</b><span>${st}</span><i><u style="width:${(k*100).toFixed(1)}%;background:${ally?'#5fb3d9':'#e5604d'}"></u></i></div>`;}
function hud(){
 const h=hero;
 $('bHp').style.width=(h.hp/h.max*100)+'%';$('tHp').textContent=fmt(h.hp)+' / '+fmt(h.max);
 $('bMp').style.width=(h.mp/h.mpMax*100)+'%';$('tMp').textContent=fmt(h.mp)+' / '+fmt(h.mpMax);
 $('bSt').style.width=h.st+'%';
 const tm=Math.max(0,Math.ceil(G.time));$('timer').textContent=String(Math.floor(tm/60)).padStart(2,'0')+':'+String(tm%60).padStart(2,'0');
 const dead=G.eyes.filter(e=>e.dead).length,rev=G.eyes.filter(e=>e.revealed).length;
 let prog=0;for(const e of G.eyes){const p=e.dead?1:e.revealed?0.15+0.45*(1-e.seals/e.maxSeals)+(e.seals===0?0.4*(1-e.hp/e.max):0):0;prog=Math.max(prog,p);}
 $('v1').textContent=G.phase>1?'Đã mở cửa '+DIRS[G.open].n:'Đã tìm '+rev+'/4 · phá '+dead+'/4';$('p1').style.width=(prog*100)+'%';
 $('v2').textContent=G.phase<2?'Mở sau khi phá cửa':fmt(Math.min(G.killed-G.k0,G.need)*QUAN)+' / '+fmt(G.need*QUAN)+' · Trấn thủ '+(boss.dead?'✓':'✗');$('p2').style.width=(G.phase<2?0:Math.min(1,(G.killed-G.k0)/G.need)*80+(boss.dead?20:0))+'%';
 $('v3').textContent=G.phase===3?(G.contested?'Bị tranh chấp · ':'')+'Giữ đài '+Math.floor(G.flag)+'/'+G.flagNeed+' giây':(G.over==='win'?'Đã cắm cờ':'Giữ đài '+G.flagNeed+' giây');$('p3').style.width=(G.flag/G.flagNeed*100)+'%';
 for(let i=1;i<=3;i++){const el=$('ph'+i);const done=G.phase>i||(G.over==='win');el.classList.toggle('done',done);el.classList.toggle('on',G.phase===i&&!done);}
 const ht=hintText();if(ht!==lastHint){$('ohint').innerHTML=ht;lastHint=ht;}
 if(boss.active&&!boss.dead){$('bBoss').style.width=(boss.hp/boss.max*100)+'%';$('bhpT').textContent=fmt(boss.hp)+' / '+fmt(boss.max);}
 const ta=allyCount(),dc=enemyCount();$('cTa').textContent='Ta '+fmt(ta*QUAN);$('cDich').textContent='Địch '+fmt(dc*QUAN);
 const bn=$('banner');if(G.banner){const b=G.banner;const k=Math.min(1,(b.max-b.t)/0.3,b.t/0.5);bn.style.opacity=k;$('bt').innerHTML=(b.g?'<span class="g" style="font-size:40px;color:#d9534f;margin-right:12px;vertical-align:-4px">'+b.g+'</span>':'')+b.title;$('bs').textContent=b.sub;}else bn.style.opacity=0;
 const cc=$('camcap');if(G.cue&&G.cue.t>0&&G.cue.txt){if(cc.dataset.t!==G.cue.txt){cc.dataset.t=G.cue.txt;$('camT').textContent=G.cue.txt;}cc.style.opacity=Math.min(1,G.cue.t/0.4,(G.cue.max-G.cue.t)/0.4);}else cc.style.opacity=0;
 const w=$('warn');if(G.warnT>0){w.textContent=G.warn;w.style.opacity=Math.min(1,G.warnT);}else w.style.opacity=0;
 if(frame%6===0){$('feed').innerHTML=G.feed.slice(-3).map(f=>{const a=clamp(1-(G.t-f.t-6)/3,0.25,1);return `<div style="opacity:${a}">${f.s}</div>`;}).join('');
  $('team').innerHTML=G.allies.map(c=>teamRow(c,true)).join('');$('foeL').innerHTML=G.foes.map(c=>teamRow(c,false)).join('');}
 cdSet($('k_slash'),h.cdS,0.42,false);cdSet($('k_yu'),h.cd2.yu,4,true);cdSet($('k_loi'),h.cd2.loi,4,true);cdSet($('k_van'),h.cd2.van,28,true);cdSet($('k_don'),h.cd2.don,1.8,true);cdSet($('k_dan'),G.danCd,6,true);cdSet($('k_yan'),h.cd2.yan,10,true);
 cdSet($('s_ho'),G.hoCd,24,true);cdSet($('s_pha'),G.phaCd,1.2,false);cdSet($('s_an'),G.anCd,3,true);
 $('loiN').textContent=G.loi;$('danN').textContent=G.dan;$('phaN').textContent='còn '+G.pha;$('anN').textContent=G.an<3?'còn '+G.an+' · hồi '+Math.ceil(20-G.anT)+'s':'còn '+G.an+' · Thổ';
 $('k_van').classList.toggle('off',!inZone(h,'ho')||h.mp<160);$('k_loi').classList.toggle('off',G.loi<=0);$('k_dan').classList.toggle('off',G.dan<=0);$('s_pha').classList.toggle('lock',G.pha<=0);$('s_an').classList.toggle('lock',G.an<=0);
 $('k_don').classList.toggle('off',!!inZone(h,'khon'));
 $('k_yan').classList.toggle('glow',G.phase===1&&G.eyes.some(q=>!q.dead&&!q.revealed&&!h.dead&&dist(h,q)<500));
 $('autob').textContent='Tự đánh: '+(AUTO?'Bật':'Tắt');$('autob').classList.toggle('on',AUTO);
 if(G.endShown)$('end').style.opacity=clamp((G.overT-3.8)/0.8,0,1);
 if(frame%2===0)drawMini();
 frame++;
}
function drawMini(){
 const X=x=>MOX+x*MS,Y=y=>MOY+y*MS;
 mx.fillStyle='#1e1d16';mx.fillRect(0,0,202,170);
 mx.fillStyle='rgba(70,62,50,.9)';mx.beginPath();mx.arc(X(C.x),Y(C.y),WALL_R*MS,0,TAU);mx.fill();
 mx.strokeStyle='rgba(140,125,100,.5)';mx.lineWidth=RING_W*2*MS;mx.beginPath();mx.arc(X(C.x),Y(C.y),RING_R*MS,0,TAU);mx.stroke();
 mx.strokeStyle='rgba(217,180,106,.7)';mx.lineWidth=2;mx.beginPath();mx.arc(X(C.x),Y(C.y),WALL_R*MS,0,TAU);mx.stroke();mx.lineWidth=1.2;mx.beginPath();mx.arc(X(C.x),Y(C.y),IN_R*MS,0,TAU);mx.stroke();
 for(let d=0;d<4;d++){if(!G.barrier||G.sec[d])continue;mx.strokeStyle='rgba(255,140,180,.85)';mx.lineWidth=1.8;mx.beginPath();mx.arc(X(C.x),Y(C.y),BAR_R*MS,DIRS[d].a-SEC_H+0.02,DIRS[d].a+SEC_H-0.02);mx.stroke();}
 mx.fillStyle=G.flagOwner===0?'#5fb3d9':'#d9534f';mx.fillRect(X(C.x)-2,Y(C.y)-2,4,4);
 for(const e of G.eyes){if(e.dead){mx.strokeStyle='#999';mx.lineWidth=1.2;mx.beginPath();mx.moveTo(X(e.x)-3,Y(e.y)-3);mx.lineTo(X(e.x)+3,Y(e.y)+3);mx.moveTo(X(e.x)+3,Y(e.y)-3);mx.lineTo(X(e.x)-3,Y(e.y)+3);mx.stroke();}
  else if(e.revealed){mx.fillStyle=`rgb(${ELEM[e.el].col})`;mx.fillRect(X(e.x)-3,Y(e.y)-3,6,6);}
  else{mx.strokeStyle=`rgba(205,170,255,${0.35+0.25*Math.sin(G.t*3+e.i)})`;mx.lineWidth=1;mx.beginPath();mx.arc(X(e.hx),Y(e.hy),300*MS,0,TAU);mx.stroke();}}
 for(const u of G.units){if(u.dead||u.champ||u===boss)continue;mx.fillStyle=u.team?'#e5604d':'#5fb3d9';mx.fillRect(X(u.x)-0.9,Y(u.y)-0.9,1.8,1.8);}
 mx.setLineDash([2,2]);mx.lineWidth=1;
 for(const c of G.allies){const tp=champTgtPt(c);if(!tp||dist(c,tp)<220)continue;mx.strokeStyle='rgba(143,208,239,.7)';mx.beginPath();mx.moveTo(X(c.x),Y(c.y));mx.lineTo(X(tp.x),Y(tp.y));mx.stroke();}
 for(const m of G.foes){if(m===boss||m.dead)continue;const tp=champGoal(m);if(dist(m,tp)<260)continue;mx.strokeStyle='rgba(255,140,120,.7)';mx.beginPath();mx.moveTo(X(m.x),Y(m.y));mx.lineTo(X(tp.x),Y(tp.y));mx.stroke();}
 mx.setLineDash([]);
 for(const c of [...G.foes,...G.allies]){if(c.dead)continue;mx.fillStyle=c===hero?'#f3d9a0':c.team?'#ff6a5a':'#8fd0ef';mx.strokeStyle='#0a0806';mx.lineWidth=1;mx.beginPath();mx.arc(X(c.x),Y(c.y),c===hero||c===boss?3.2:2.6,0,TAU);mx.fill();mx.stroke();}
 mx.strokeStyle='rgba(239,228,207,.6)';mx.lineWidth=1;mx.strokeRect(X(G.cam.x),Y(G.cam.y),VW/G.zoom*MS,VH/G.zoom*MS);
}
function showEnd(){G.endShown=true;const e=$('end');e.style.display='flex';
 const win=G.over==='win';
 e.querySelector('.big').textContent=win?'大捷':'敗';$('endT').textContent=win?'Công phá Phù Vân Thành':'Công thành thất bại';$('endS').textContent=win?'Cờ Chính đạo đã cắm trên đài trung tâm':'Thủ quân giữ được thành';
 const used=TIME-G.time;$('e1').textContent=Math.floor(used/60)+' phút '+Math.floor(used%60)+' giây';$('e2').textContent=fmt(G.killed*QUAN);$('e3').textContent=fmt(G.lost*QUAN);$('e4').textContent=(G.maDown+(boss.dead?1:0))+' / 5';}
