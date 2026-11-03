import os,sys
D=os.path.dirname(os.path.abspath(__file__))
P=os.path.dirname(D)
L=open(os.path.join(P,'index_v3.html'),encoding='utf-8').read().split('\n')
def seg(a,b):  # 1-based inclusive
    return '\n'.join(L[a-1:b])
def part(n):
    return open(os.path.join(D,n),encoding='utf-8').read().rstrip('\n')
def rep(s,old,new,cnt=1):
    n=s.count(old)
    if n!=cnt:
        print('REPLACE MISMATCH',n,'!=',cnt,':',old[:90]);sys.exit(1)
    return s.replace(old,new)

head=seg(1,180)
# ---------- CSS ----------
head=rep(head,"#feed{position:absolute;left:1046px;top:234px;","#feed{position:absolute;left:1046px;top:372px;")
head=rep(head,"#cmd{left:206px;bottom:16px;width:252px;padding:8px 10px}","#cmd{left:206px;bottom:16px;width:256px;padding:8px 10px}\n#foe{left:1046px;top:226px;width:220px;padding:7px 10px 8px}\n#team,#foeL{margin-top:6px;display:flex;flex-direction:column;gap:2px}\n.tm{display:grid;grid-template-columns:18px 1fr auto;align-items:center;column-gap:6px;font-size:11px;color:#cdbf9f;line-height:1.2}\n.tm .g{font-size:15px;text-align:center}\n.tm b{font-weight:600;color:var(--txt);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}\n.tm span:last-of-type{font-size:10.5px;white-space:nowrap}\n.tm i{grid-column:2/4;display:block;height:3px;border-radius:2px;background:#2a231a;overflow:hidden}\n.tm i u{display:block;height:100%}\n.tm.dead{opacity:.45}\n#foe .tm b{color:#ffd6cf}\n#stage.strat .pnl,#stage.strat .sk,#stage.strat #feed,#stage.strat #autob,#stage.strat #joy,#stage.strat #banner,#stage.strat #warn,#stage.strat #hint,#stage.strat #tag{opacity:0!important}\n.pnl,.sk,#feed,#autob,#joy,#tag{transition:opacity .25s}")
head=rep(head,'<div id="warn"></div>','<div id="warn"></div>\n<div id="camcap"><span class="cc"><span class="g">鏡</span><b id="camT"></b></span></div>')
head=rep(head,"#banner{position:absolute;","#camcap{position:absolute;left:0;right:0;top:188px;text-align:center;pointer-events:none;opacity:0}\n#camcap .cc{display:inline-block;padding:5px 16px 5px 12px;border-radius:999px;background:rgba(10,8,6,.8);border:1px solid rgba(217,180,106,.5);font-size:13px;color:var(--txt);box-shadow:0 4px 14px rgba(0,0,0,.5)}\n#camcap .g{color:#d9b46a;margin-right:8px;font-size:15px}\n#camcap b{font-weight:600}\n#stage.strat #camcap{opacity:0!important}\n#banner{position:absolute;")
head=rep(head,"#troops{margin-top:7px;font-size:11px;display:grid;grid-template-columns:1fr auto;row-gap:2px;color:#cdbf9f}\n#troops b{color:var(--txt);font-weight:600;text-align:right}\n","")
# ---------- HTML ----------
head=rep(head,'<span class="l">Phá trận nhãn ẩn</span>','<span class="l">Phá trận nhãn, mở cửa</span>')
head=rep(head,'<span class="l">Diệt thủ quân</span>','<span class="l">Hạ Trấn thủ, diệt thủ quân</span>')
head=rep(head,'.ph{display:grid;grid-template-columns:20px 118px 1fr;','.ph{display:grid;grid-template-columns:20px 168px 1fr;')
head=rep(head,'<div id="feed"></div>','<div class="pnl" id="foe"><div class="hd" style="color:#ff9c90;margin-bottom:0">MA ĐẠO · NĂM TƯỚNG</div><div id="foeL"></div></div>\n<div id="feed"></div>')
old_cmd=head[head.index('<div class="pnl" id="cmd">'):head.index('<div class="pnl" id="tray">')]
head=head.replace(old_cmd,'''<div class="pnl" id="cmd">
 <div class="hd">THẾ TRẬN · CHÍNH ĐẠO</div>
 <div class="cb">
  <button class="cbt on" id="c_auto" type="button"><span class="g">謀</span>Tự quyết · Q</button>
  <button class="cbt" id="c_split" type="button"><span class="g">分</span>Phân binh · E</button>
  <button class="cbt" id="c_mass" type="button"><span class="g">合</span>Hợp binh · R</button>
 </div>
 <div id="team"></div>
</div>
''')
head=rep(head,'Q E R lệnh quân</div>','Q E R thế trận · M sa bàn</div>')
head=rep(head,'Bản thử nghiệm 3 · số liệu chưa cân bằng','Bản thử nghiệm 4 · năm tướng mỗi phe · số liệu chưa cân bằng')
head=rep(head,'<div>Linh thạch thu được<b id="e4"></b></div>','<div>Ma tướng bị hạ<b id="e4"></b></div>')

skills=seg(594,686)
skills=rep(skills,"if(G.barrier&&dist(pt,C)<BAR_R)return false;","if(barBlock(h,pt))return false;",2)
skills=rep(skills,"showBanner('Vạn Kiếm Quy Tông'","cueHero(2);showBanner('Vạn Kiếm Quy Tông'")

draw=seg(990,1159)
draw=rep(draw,"function pushFeed(s){G.feed.push({s,t:G.t});if(G.feed.length>6)G.feed.shift();}","function pushFeed(s){G.feed.push({s,t:G.t});G.log.push({s,t:G.t});if(G.feed.length>6)G.feed.shift();}")
draw=rep(draw," if(u.t==='hero')return drawHero(u,x,y);"," if(u.champ&&u!==hero)return drawChamp(u,x,y);\n if(u.t==='hero')return drawHero(u,x,y);")

render=seg(1161,1247)
i0=render.index(" if(G.barrier){const life=G.eyes.filter(p=>!p.dead).length/3;")
i1=render.index(" for(const r of G.ripples)")
render=render[:i0]+" drawBarrier(vis);\n"+render[i1:]
render=rep(render,"if(G.breakT>=0&&G.breakT<1.6){const k=G.breakT/1.6;ctx.strokeStyle=`rgba(255,180,210,${1-k})`;ctx.lineWidth=14*(1-k)+1;circ(C.x,C.y,BAR_R+k*240);ctx.stroke();}",
 "if(G.breakT>=0&&G.breakT<1.6&&G.breakD>=0){const k=G.breakT/1.6,A=DIRS[G.breakD].a;ctx.strokeStyle=`rgba(255,180,210,${1-k})`;ctx.lineWidth=14*(1-k)+1;ctx.beginPath();ctx.arc(C.x,C.y,BAR_R+k*240,A-SEC_H,A+SEC_H);ctx.stroke();}")
render=rep(render,"  else{ctx.strokeStyle=`rgba(255,200,150,${0.5+k*0.5})`;ctx.lineWidth=2;circ(t.x,t.y,t.r);ctx.stroke();ctx.fillStyle=`rgba(255,190,140,${0.15*k})`;circ(t.x,t.y,t.r);ctx.fill();}}",
 "  else if(t.kind==='mtele'){ctx.fillStyle=`rgba(${t.col},${0.08+0.1*k})`;circ(t.x,t.y,t.r);ctx.fill();ctx.fillStyle=`rgba(${t.col},${0.25*k})`;circ(t.x,t.y,t.r*k);ctx.fill();ctx.strokeStyle=`rgba(${t.col},${0.6+0.4*k})`;ctx.lineWidth=2.5;circ(t.x,t.y,t.r);ctx.stroke();}\n"
 "  else if(t.kind==='fire'||t.kind==='ice'||t.kind==='vine'){const col=t.kind==='fire'?'255,150,80':t.kind==='ice'?'150,210,255':'127,209,139';ctx.fillStyle=`rgba(${col},${0.08+0.18*k})`;circ(t.x,t.y,t.r);ctx.fill();ctx.strokeStyle=`rgba(${col},${0.5+0.5*k})`;ctx.lineWidth=2.5;circ(t.x,t.y,t.r);ctx.stroke();ctx.lineWidth=1.5;circ(t.x,t.y,t.r*(1-k*0.7));ctx.stroke();}\n"
 "  else{ctx.strokeStyle=`rgba(255,200,150,${0.5+k*0.5})`;ctx.lineWidth=2;circ(t.x,t.y,t.r);ctx.stroke();ctx.fillStyle=`rgba(255,190,140,${0.15*k})`;circ(t.x,t.y,t.r);ctx.fill();}}")
render=rep(render,"else if(p.kind==='bolt'){ctx.fillStyle='rgba(243,217,160,.35)';for(const [x,y] of p.trail){circ(x,y,3);ctx.fill();}ctx.fillStyle='rgba(243,217,160,.5)';circ(p.x,p.y,7);ctx.fill();ctx.fillStyle='#fff3c8';circ(p.x,p.y,3);ctx.fill();}",
 "else if(p.kind==='bolt'){const col=p.col||'243,217,160',s=p.big?1.5:1;ctx.fillStyle=`rgba(${col},.35)`;for(const [x,y] of p.trail){circ(x,y,3*s);ctx.fill();}ctx.fillStyle=`rgba(${col},.55)`;circ(p.x,p.y,7*s);ctx.fill();ctx.fillStyle='#fff6e0';circ(p.x,p.y,3*s);ctx.fill();}")
render=rep(render,"  else if(f.type==='ghost'){ctx.fillStyle=`rgba(143,208,239,${0.4*k})`;ell(f.x,f.y-6,10,16);ctx.fill();}",
 "  else if(f.type==='ghost'){ctx.fillStyle=`rgba(${f.col||'143,208,239'},${0.4*k})`;ell(f.x,f.y-6,10,16);ctx.fill();}\n"
 "  else if(f.type==='slashC'){const p=1-k;ctx.strokeStyle=`rgba(${f.col},${k})`;ctx.lineWidth=7*k+2;ctx.beginPath();ctx.arc(f.x,f.y-8,38,f.a-0.9,f.a-0.9+1.8*Math.min(1,p*1.7));ctx.stroke();}\n"
 "  else if(f.type==='ice'){ctx.fillStyle=`rgba(205,236,255,${Math.min(1,k*1.4)})`;ctx.strokeStyle=`rgba(120,190,255,${k})`;ctx.lineWidth=1;for(let i=0;i<10;i++){const a=i*TAU/10+0.3;const rr=f.r*(0.25+0.6*((i*37)%10)/10);const px=f.x+Math.cos(a)*rr,py=f.y+Math.sin(a)*rr*0.6;const h=8+26*Math.min(1,(1-k)*5);ctx.beginPath();ctx.moveTo(px-5,py);ctx.lineTo(px,py-h);ctx.lineTo(px+5,py);ctx.closePath();ctx.fill();ctx.stroke();}}\n"
 "  else if(f.type==='vine'){ctx.strokeStyle=`rgba(96,190,110,${Math.min(1,k*1.6)})`;ctx.lineWidth=3;for(let i=0;i<8;i++){const a=i*TAU/8;ctx.beginPath();ctx.moveTo(f.x,f.y);for(let s=1;s<=6;s++){const rr=f.r*s/6*Math.min(1,(1-k)*3+0.2);const aa=a+Math.sin(s*1.3+i)*0.5;ctx.lineTo(f.x+Math.cos(aa)*rr,f.y+Math.sin(aa)*rr*0.6);}ctx.stroke();}ctx.fillStyle=`rgba(150,220,140,${k*0.8})`;for(let i=0;i<8;i++){const a=i*TAU/8+0.2;circ(f.x+Math.cos(a)*f.r*0.7,f.y+Math.sin(a)*f.r*0.42,3);ctx.fill();}}")
render=rep(render," else if(hero.hp<hero.max*0.25){ctx.strokeStyle=`rgba(229,60,50,${0.35+0.25*Math.sin(G.t*8)})`;ctx.lineWidth=24;ctx.strokeRect(0,0,VW,VH);}\n}",
 " else if(hero.hp<hero.max*0.25){ctx.strokeStyle=`rgba(229,60,50,${0.35+0.25*Math.sin(G.t*8)})`;ctx.lineWidth=24;ctx.strokeRect(0,0,VW,VH);}\n"
 " let sa=0,S0=null;\n"
 " if(G.intro>0){sa=clamp((G.intro-4.7)/0.4,0,1);S0={g:'攻',title:'Phù Vân Thành · năm tướng đấu năm tướng',sub:G.plan?G.plan.title+': '+G.plan.sub:''};}\n"
 " else if(G.strat){const s=G.strat;sa=clamp(Math.min((s.max-s.t)/0.35,s.t/0.35),0,1);S0=s;}\n"
 " else if(G.stratOn){sa=1;S0={g:G.plan?G.plan.g:'謀',title:'Sa bàn · '+(G.plan?G.plan.title:'thế trận'),sub:G.plan?G.plan.sub:''};}\n"
 " if(sa>0.01)drawStrat(sa,S0);\n"
 " const so=sa>0.5;if(so!==stratShown){stratShown=so;stage.classList.toggle('strat',so);}\n}\nlet stratShown=false;")

ctl=seg(1318,len(L))
ctl=rep(ctl,"else if(k==='q')setCmd('atk');else if(k==='e')setCmd('hold');else if(k==='r')setCmd('ret');","else if(k==='q')setMode('auto');else if(k==='e')setMode('split');else if(k==='r')setMode('mass');else if(k==='m')G.stratOn=!G.stratOn;")
ctl=rep(ctl,"bindBtn('c_atk',()=>setCmd('atk'));bindBtn('c_hold',()=>setCmd('hold'));bindBtn('c_ret',()=>setCmd('ret'));","bindBtn('c_auto',()=>setMode('auto'));bindBtn('c_split',()=>setMode('split'));bindBtn('c_mass',()=>setMode('mass'));\n$('mm').addEventListener('click',()=>{G.stratOn=!G.stratOn;});")
ctl=rep(ctl,"update(dt);render();hud();requestAnimationFrame(loop);","update(dt*((!AUTO&&G.stratOn)?0.3:1));render();hud();requestAnimationFrame(loop);")
ctl=rep(ctl,"Bản thử nghiệm 3 · nhân vật đang tự đánh","Bản thử nghiệm 4 · nhân vật đang tự đánh")
ctl=rep(ctl,"out.push([Math.round(G.t),G.phase,G.killed,enemyCount(),allyCount(),Math.round(hero.hp),boss.dead,G.eyes.map(e=>e.dead?'X':e.revealed?(e.seals+'/'+Math.round(e.hp)):'?').join(' '),Math.round(G.flag),G.over,Math.round(hero.x),Math.round(hero.y),G.loi,G.an,G.pha]);",
 "out.push([Math.round(G.t),G.phase,G.killed,enemyCount(),allyCount(),Math.round(hero.hp),boss.dead,G.eyes.map(e=>e.dead?'X':e.revealed?(e.seals+'/'+Math.round(e.hp)):'?').join(' '),Math.round(G.flag),G.over,Math.round(hero.x),Math.round(hero.y),G.plan?G.plan.kind+(G.plan.tgt!=null?G.plan.tgt:''):'',G.allies.map(a=>a.key.slice(0,2)+(a.dead?'X':Math.round(a.hp/a.max*100))+'@'+a.asg).join(' '),G.foes.map(m=>m.key.slice(0,2)+(m.dead?'X':Math.round(m.hp/m.max*100))+'@'+m.asg).join(' '),G.allies.concat(G.foes).map(a=>Math.round(a.x/10)+','+Math.round(a.y/10)).join(' ')]);")

out='\n'.join([
 head,
 part('a_const.js'),
 seg(208,319),
 part('b1_world.js'),
 seg(355,407),
 part('b2_labels.js'),
 seg(413,436),
 part('c_state.js'),
 '',
 skills,
 part('d_ai.js'),
 '',
 part('e_update.js'),
 '',
 draw,
 part('f_draw.js'),
 '',
 render,
 '',
 part('h_hud.js'),
 '',
 ctl,
])
open(os.path.join(P,'index.html'),'w',encoding='utf-8').write(out)
print('ok',len(out))
