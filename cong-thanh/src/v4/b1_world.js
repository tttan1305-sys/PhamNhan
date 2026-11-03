function genWorld(){
 const sv=seed;seed=777;
 PRIMS=[];BUILD=[];ANIM=[];DLAB=[];BGRID.clear();
 const bandO=[RING_R+RING_W+18,WALL_R-WALL_T/2-24],bandI=[IN_R+IN_T/2+24,RING_R-RING_W-18];
 // L0 vệt cỏ đất
 for(let i=0;i<2700;i++){const x=rnd()*WW,y=rnd()*WH,r=40+rnd()*180;const tone=rnd()<0.55?'64,78,46':'52,42,30',al=0.16+rnd()*0.22;if(Math.hypot(x-C.x,y-C.y)<WALL_R-r)continue;
  prim(0,x-r,y-r,x+r,y+r,c=>{const gr=c.createRadialGradient(x,y,0,x,y,r);gr.addColorStop(0,`rgba(${tone},${al})`);gr.addColorStop(1,`rgba(${tone},0)`);c.fillStyle=gr;c.fillRect(x-r,y-r,2*r,2*r);});}
 // L1 ngoài thành: bốn vùng trận nhãn
 const EK=EYE_POS[0],ET=EYE_POS[1],EH=EYE_POS[2],EM=EYE_POS[3];
 blob(1,CAMP.x+150,CAMP.y,560,420,'rgba(110,95,68,.24)');
 blob(1,EM.x,EM.y,540,500,'rgba(14,26,14,.62)');
 blob(1,ET.x,ET.y,520,460,'rgba(24,46,44,.5)');
 blob(1,EK.x-60,EK.y+40,520,460,'rgba(95,88,76,.26)');
 blob(1,EK.x-170,EK.y+120,340,260,'rgba(60,24,20,.22)');
 blob(1,EH.x,EH.y,540,480,'rgba(60,22,10,.5)');
 blob(1,EH.x+60,EH.y+30,320,260,'rgba(150,60,20,.2)');
 prim(1,0,C.y-240,C.x-WALL_R+40,C.y+300,c=>{c.lineCap='round';for(const [w,a] of [[190,.08],[140,.12],[100,.16],[60,.1]]){c.strokeStyle=`rgba(110,95,68,${a})`;c.lineWidth=w;c.beginPath();c.moveTo(0,C.y+40);c.bezierCurveTo(C.x-2600,C.y+140,C.x-2100,C.y-80,C.x-WALL_R-10,C.y);c.stroke();}});
 for(const g of [-Math.PI/2,0,Math.PI/2]){const p0=polar(g,WALL_R),p1=polar(g,WALL_R+1000);prim(1,Math.min(p0.x,p1.x)-60,Math.min(p0.y,p1.y)-60,Math.max(p0.x,p1.x)+60,Math.max(p0.y,p1.y)+60,c=>{c.strokeStyle='rgba(110,95,68,.13)';c.lineWidth=110;c.lineCap='round';c.beginPath();c.moveTo(p0.x,p0.y);c.lineTo(p1.x,p1.y);c.stroke();});}
 // lối quân Chính đạo vòng lên Bắc và xuống Nam
 for(const s of [-1,1]){const P0={x:CAMP.x+200,y:CAMP.y+s*150},P1={x:C.x-2250,y:C.y+s*1950},P2={x:C.x-760,y:C.y+s*2160};
  prim(1,Math.min(P0.x,P1.x,P2.x)-90,Math.min(P0.y,P1.y,P2.y)-90,Math.max(P0.x,P1.x,P2.x)+90,Math.max(P0.y,P1.y,P2.y)+90,c=>{c.lineCap='round';for(const [w,a] of [[120,.07],[80,.1],[44,.12]]){c.strokeStyle=`rgba(110,95,68,${a})`;c.lineWidth=w;c.beginPath();c.moveTo(P0.x,P0.y);c.quadraticCurveTo(P1.x,P1.y,P2.x,P2.y);c.stroke();}});}
 // Đầm Sen (Bắc)
 for(let i=0;i<20;i++){const a=RR(0,TAU),rr=RR(95,400);const x=ET.x+Math.cos(a)*rr,y=ET.y+Math.sin(a)*rr*0.85;if(Math.hypot(x-C.x,y-C.y)<BAR_R+70)continue;const rx=RR(40,115),ry=RR(20,52);
  prim(1,x-rx-4,y-ry-4,x+rx+4,y+ry+4,c=>{c.fillStyle='#1b3234';c.beginPath();c.ellipse(x,y,rx,ry,0,0,TAU);c.fill();c.strokeStyle='rgba(140,190,190,.16)';c.lineWidth=2;c.beginPath();c.ellipse(x,y,rx-4,ry-3,0,Math.PI*1.1,Math.PI*1.7);c.stroke();c.fillStyle='rgba(120,170,120,.5)';for(let k=0;k<3;k++){c.beginPath();c.ellipse(x+(k-1)*rx*0.4,y+((k*37)%11-5)*ry*0.08,5,3.5,0,0,TAU);c.fill();}});}
 for(let i=0;i<140;i++){const a=RR(0,TAU),rr=RR(70,460);const x=ET.x+Math.cos(a)*rr,y=ET.y+Math.sin(a)*rr*0.85;if(Math.hypot(x-C.x,y-C.y)<BAR_R+50)continue;const n=3+Math.floor(rnd()*4);
  prim(3,x-10,y-16,x+10,y+3,c=>{c.strokeStyle='rgba(140,150,80,.7)';c.lineWidth=1.3;c.beginPath();for(let k=0;k<n;k++){c.moveTo(x+(k-n/2)*2.4,y);c.lineTo(x+(k-n/2)*3.4,y-9-((k*7)%5));}c.stroke();});}
 // Hắc Lâm (Nam)
 for(let i=0;i<230;i++){const a=RR(0,TAU),rr=RR(118,500);const x=EM.x+Math.cos(a)*rr,y=EM.y+Math.sin(a)*rr;if(Math.hypot(x-C.x,y-C.y)<BAR_R+40)continue;treeP(x,y,18+rnd()*22);}
 // Loạn Thạch Cương (Tây)
 for(let i=0;i<90;i++){const a=RR(0,TAU),rr=RR(70,470);const x=EK.x+Math.cos(a)*rr,y=EK.y+Math.sin(a)*rr;if(Math.hypot(x-C.x,y-C.y)<BAR_R+60)continue;if(Math.abs(y-C.y-30)<60&&x>EK.x-400)continue;const s=6+rnd()*18,v=rnd()<0.5;if(s>15)prim(4,x-s-2,y-s,x+s*1.5,y+s,c=>rock(c,x,y,s,v),y);else prim(3,x-s-2,y-s,x+s*1.5,y+s,c=>rock(c,x,y,s,v));}
 // Hỏa Diệm Cốc (Đông): khe nham thạch, đá cháy, cây chết, than hồng
 for(let i=0;i<30;i++){const a=RR(0,TAU),rr=RR(70,440);const x=EH.x+Math.cos(a)*rr,y=EH.y+Math.sin(a)*rr*0.9;if(Math.hypot(x-C.x,y-C.y)<BAR_R+70)continue;const L=RR(50,120),aa=RR(0,TAU),bend=RR(-0.6,0.6);
  prim(1,x-L-8,y-L-8,x+L+8,y+L+8,c=>{c.lineCap='round';c.beginPath();let px=x,py=y;c.moveTo(px,py);for(let k=1;k<=4;k++){const q=aa+bend*k;px+=Math.cos(q)*L/4;py+=Math.sin(q)*L/4;c.lineTo(px,py);}c.strokeStyle='rgba(18,8,4,.85)';c.lineWidth=6;c.stroke();c.strokeStyle='rgba(255,110,40,.6)';c.lineWidth=1.8;c.stroke();});}
 for(let i=0;i<60;i++){const a=RR(0,TAU),rr=RR(80,470);const x=EH.x+Math.cos(a)*rr,y=EH.y+Math.sin(a)*rr;if(Math.hypot(x-C.x,y-C.y)<BAR_R+60)continue;const s=6+rnd()*16;
  if(s>14)prim(4,x-s-2,y-s,x+s*1.5,y+s,c=>{rock(c,x,y,s,false);c.fillStyle='rgba(140,46,20,.35)';c.beginPath();c.ellipse(x,y,s*0.8,s*0.4,0,0,TAU);c.fill();},y);else prim(3,x-s-2,y-s,x+s*1.5,y+s,c=>rock(c,x,y,s,false));}
 for(let i=0;i<34;i++){const a=RR(0,TAU),rr=RR(150,500);const x=EH.x+Math.cos(a)*rr,y=EH.y+Math.sin(a)*rr;if(Math.hypot(x-C.x,y-C.y)<BAR_R+60)continue;const h=RR(18,34),lean=RR(-0.3,0.3);
  prim(4,x-h,y-h-4,x+h,y+6,c=>{c.strokeStyle='#1a120e';c.lineWidth=3;c.lineCap='round';c.beginPath();c.moveTo(x,y);c.lineTo(x+lean*h,y-h);c.moveTo(x+lean*h*0.6,y-h*0.6);c.lineTo(x+lean*h*0.6+h*0.35,y-h*0.85);c.moveTo(x+lean*h*0.7,y-h*0.7);c.lineTo(x+lean*h*0.7-h*0.3,y-h*0.95);c.stroke();},y);}
 for(let k=0;k<9;k++){const a=RR(0,TAU),rr=RR(60,380);const x=EH.x+Math.cos(a)*rr,y=EH.y+Math.sin(a)*rr;if(Math.hypot(x-C.x,y-C.y)<BAR_R+60)continue;ANIM.push({x,y,r:52,col:'255,100,40',k:'forge',ph:rnd()*6,sp:5});}
 // đá rải rác
 for(let i=0;i<200;i++){const x=rnd()*WW,y=rnd()*WH;const s=4+rnd()*8,v=rnd()<0.5;if(Math.hypot(x-C.x,y-C.y)<BAR_R+80)continue;prim(3,x-s-2,y-s,x+s*1.5,y+s,c=>rock(c,x,y,s,v));}
 // doanh trại Chính đạo
 for(const [dx,dy] of [[-180,-260],[-120,-140],[-200,-20],[-130,110],[-190,230],[-60,-330],[-60,320],[-260,120],[-250,-150]]){const x=CAMP.x+dx+60,y=CAMP.y+dy;prim(4,x-36,y-50,x+40,y+30,c=>tent(c,x,y,false),y);}
 for(const [dx,dy] of [[-90,-60],[-110,180],[-160,60]]){const x=CAMP.x+dx+60,y=CAMP.y+dy;ANIM.push({x,y,r:46,col:'255,170,80',k:'fire',ph:rnd()*6,sp:9});prim(3,x-12,y-8,x+12,y+8,c=>{c.fillStyle='#2a2018';c.beginPath();c.ellipse(x,y+3,11,5,0,0,TAU);c.fill();});}
 // tiền đồn Ma quân: Tây đông nhất, mỗi hướng còn lại vài lều
 for(let k=0;k<6;k++){const a=deg(174+k*3),rr=2470+(k%2)*60;const p=polar(a,rr);prim(4,p.x-36,p.y-50,p.x+40,p.y+30,c=>tent(c,p.x,p.y,true),p.y);}
 for(let k=0;k<2;k++){const p=polar(deg(177+k*7),2430);ANIM.push({x:p.x,y:p.y,r:44,col:'255,90,60',k:'fire',ph:rnd()*6,sp:10});}
 for(let d=1;d<4;d++){const ea=deg(EYE_DEF[d].a);for(let k=0;k<3;k++){const p=polar(ea+deg(-6+k*6),2480+(k%2)*60);prim(4,p.x-36,p.y-50,p.x+40,p.y+30,c=>tent(c,p.x,p.y,true),p.y);}const f=polar(ea+deg(3),2440);ANIM.push({x:f.x,y:f.y,r:44,col:'255,90,60',k:'fire',ph:rnd()*6,sp:10});}
 // cây ngoài
 for(let i=0;i<1800;i++){const x=rnd()*WW,y=rnd()*WH,r=18+rnd()*26;const rc=Math.hypot(x-C.x,y-C.y);const edge=y<220||y>WH-220||x<180||x>WW-180;if(!edge&&rnd()<0.8)continue;if(rc<BAR_R+120)continue;if(dist({x,y},CAMP)<560)continue;if(x<C.x-WALL_R&&Math.abs(y-C.y)<260)continue;if(EYE_POS.some(e=>dist(e,{x,y})<240))continue;treeP(x,y,r);}
