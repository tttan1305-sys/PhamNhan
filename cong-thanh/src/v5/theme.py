# Bản 5: đổi toàn bộ màu sang ban ngày, xanh tươi, giao diện hiện đại.
# Đọc index.html do v4/build.py tạo ra, thay màu theo từng hàm, ghi lại index.html.
import os, sys, re
D = os.path.dirname(os.path.abspath(__file__))
P = os.path.dirname(D)
SRC = os.path.join(P, 'index.html')
H = open(SRC, encoding='utf-8').read()
bad = []

def rep(s, old, new, cnt=1, where=''):
    n = s.count(old)
    if (cnt == 0 and n == 0) or (cnt and n != cnt):
        bad.append(f'{where}: {n} != {cnt}: {old[:80]}')
        return s
    return s.replace(old, new)

def fn_span(s, head):
    i = s.index(head)
    # hàm kết thúc ở dòng "}" đầu tiên đứng một mình, hoặc ở định nghĩa kế tiếp
    j = s.find('\n}\n', i)
    k = s.find('\nfunction ', i + len(head))
    if k != -1 and (j == -1 or k < j):
        return i, k
    return i, j + 2

def in_fn(head, pairs):
    global H
    i, j = fn_span(H, head)
    body = H[i:j]
    for p in pairs:
        old, new = p[0], p[1]
        cnt = p[2] if len(p) > 2 else 1
        body = rep(body, old, new, cnt, head)
    H = H[:i] + body + H[j:]

def glob(pairs):
    global H
    for p in pairs:
        old, new = p[0], p[1]
        cnt = p[2] if len(p) > 2 else 1
        H = rep(H, old, new, cnt, 'glob')

# ===================== CSS: khung giao diện hiện đại =====================
CSS_OLD_ROOT = ':root{--ink:#120f0b;--gold:#d9b46a;--gold2:#f3d9a0;--txt:#efe4cf;--mut:#a8987c;--chinh:#5fb3d9;--ma:#d9534f}\n@media (prefers-color-scheme: dark){:root:not([data-theme="light"]){--ink:#120f0b;--txt:#efe4cf}}\n:root[data-theme="dark"]{--ink:#120f0b;--txt:#efe4cf}'
CSS_NEW_ROOT = ':root{--ink:#17322b;--gold:#10a37f;--gold2:#0f6b55;--txt:#17322b;--mut:#5d7a71;--chinh:#2f8fe0;--ma:#e5483d;--pnl:rgba(255,255,255,.88);--line:#d5e6df;--soft:#eef6f2;--acc:#10a37f;--acc2:#19c39a}\n@media (prefers-color-scheme: dark){:root:not([data-theme="light"]){--ink:#17322b;--txt:#17322b}}\n:root[data-theme="dark"]{--ink:#17322b;--txt:#17322b}'
glob([(CSS_OLD_ROOT, CSS_NEW_ROOT)])
glob([
 ("html,body{margin:0;height:100%;background:#0b0907;", "html,body{margin:0;height:100%;background:#cfe8da;"),
 ("overflow:hidden;background:#15130e}", "overflow:hidden;background:#86c95c}"),
])
OVR = r"""
/* ===== bản 5: giao diện sáng kiểu hiện đại ===== */
.pnl{background:var(--pnl);border:1px solid rgba(255,255,255,.95);border-radius:16px;box-shadow:0 8px 24px rgba(16,70,50,.18);backdrop-filter:blur(6px);-webkit-backdrop-filter:blur(6px)}
.por{border:3px solid var(--acc);background:radial-gradient(circle at 40% 35%,#ffffff,#dff3ea);color:var(--acc)}
.nm,.ttl,#banner .bt,#end h1{font-family:'Be Vietnam Pro',sans-serif;letter-spacing:-.2px}
.nm{color:#0f3d32}
.nm span{color:var(--mut)}
.bar{background:#e3eee9;border:none}
.bar b{color:#0f3d32;text-shadow:0 0 3px #fff,0 0 5px rgba(255,255,255,.9)}
.ttl{color:#0f3d32}
.ttl b{color:#0f3d32}
.ph{color:#93aaa2}
.ph .n{border-color:#c9dcd5;background:#fff}
.ph .pb{background:#e3eee9}
.ph .pb i{background:linear-gradient(90deg,#19c39a,#0ea37f)}
.ph.on{color:var(--txt)}.ph.on .n{border-color:var(--acc);color:#fff;background:var(--acc)}
.ph.done{color:#16a34a}.ph.done .n{border-color:#16a34a;background:#dcfce7;color:#16a34a}
#ohint{border-top:1px solid var(--line);color:#3f5f56}
#ohint b{color:#0f6b55}
#boss{border-color:rgba(229,72,61,.5)}
#boss .t{color:#b3261e}
#warn{color:#fff;background:linear-gradient(90deg,transparent,rgba(229,72,61,.88) 18%,rgba(229,72,61,.88) 82%,transparent);text-shadow:0 1px 2px rgba(0,0,0,.25)}
#camcap .cc{background:rgba(255,255,255,.94);border:1px solid #fff;color:var(--txt);box-shadow:0 6px 18px rgba(16,70,50,.22)}
#camcap .g{color:var(--acc)}
#banner .bg{background:linear-gradient(90deg,transparent,rgba(255,255,255,.94) 18%,rgba(255,255,255,.94) 82%,transparent);padding:12px 70px}
#banner .bt{color:#0f3d32;text-shadow:none}
#banner .bs{color:#3f5f56}
#mmc{border-radius:10px}
#feed div{background:rgba(255,255,255,.9);border-left:3px solid var(--acc);color:var(--txt);box-shadow:0 2px 8px rgba(16,70,50,.12)}
#autob{border:1px solid #fff;background:rgba(255,255,255,.9);color:var(--txt);box-shadow:0 4px 12px rgba(16,70,50,.15)}
#autob.on{background:linear-gradient(180deg,#22c99d,#0ea37f);color:#fff;border-color:#0ea37f}
#joy{background:radial-gradient(circle,rgba(255,255,255,.35),rgba(255,255,255,.18));border:2px solid rgba(255,255,255,.75)}
#knob{background:radial-gradient(circle at 40% 35%,#ffffff,#d9f1e7);border:3px solid var(--acc);box-shadow:0 4px 12px rgba(16,70,50,.25)}
.tm{color:var(--mut)}
.tm i{background:#e3eee9}
#foe .tm b{color:#8f1f17}
.hd{color:var(--acc);font-weight:700;letter-spacing:1.5px}
.cbt{border:1px solid var(--line);background:var(--soft);color:var(--txt)}
.cbt .g{color:#0f6b55}
.cbt.on{border-color:var(--acc);background:linear-gradient(180deg,#22c99d,#0ea37f);color:#fff}
.cbt.on .g{color:#fff}
.slot{border:1px solid var(--line);background:var(--soft);color:var(--txt)}
.slot .g{color:#0f6b55}
.slot span[style]{color:var(--mut)!important}
.cdn{text-shadow:0 1px 3px rgba(0,0,0,.5)}
.sk{border:3px solid #fff;background:radial-gradient(circle at 35% 30%,#ffffff,#dcf2e8);color:#0f6b55;box-shadow:0 6px 16px rgba(16,70,50,.28)}
.sk.big{background:radial-gradient(circle at 35% 30%,#3ee0b0,#0ea37f);color:#fff}
.sk .lb{color:#0f3d32;font-weight:600;text-shadow:0 0 3px #fff,0 0 6px #fff}
.sk .ct{background:#f59e0b;color:#fff;box-shadow:0 2px 6px rgba(0,0,0,.2)}
.sk .k,.slot .k{color:#7d968e}
.sk.off{filter:grayscale(.85) opacity(.6)}
.sk.glow{box-shadow:0 0 22px rgba(150,90,240,.8);border-color:#c4a8ff}
#hint{color:#0f3d32;font-weight:600;text-shadow:0 0 4px #fff,0 0 8px #fff}
#end{background:radial-gradient(ellipse at center,rgba(240,252,247,.9),rgba(214,238,228,.97))}
#end .big{color:var(--acc);text-shadow:0 6px 24px rgba(16,163,127,.35)}
#end h1{color:#0f3d32}
#end p{color:#3f5f56}
#end .st div{background:#fff;border:1px solid var(--line);color:var(--mut);box-shadow:0 4px 12px rgba(16,70,50,.1)}
#end .st b{color:var(--txt)}
#end button{border:none;background:linear-gradient(180deg,#22c99d,#0ea37f);color:#fff;box-shadow:0 6px 16px rgba(16,163,127,.35)}
#tag{color:rgba(15,61,50,.6);font-weight:600}
"""
glob([("\n</style>", OVR + "</style>")])
glob([
 ('<i id="bHp" style="background:linear-gradient(90deg,#a32d22,#e5604d)">', '<i id="bHp" style="background:linear-gradient(90deg,#f0524a,#ff7a62)">'),
 ('<i id="bMp" style="background:linear-gradient(90deg,#1f5670,#5fb3d9)">', '<i id="bMp" style="background:linear-gradient(90deg,#2f8fe0,#5ec4ff)">'),
 ('<i id="bSt" style="background:linear-gradient(90deg,#7a5f1a,#d9b46a)">', '<i id="bSt" style="background:linear-gradient(90deg,#f59e0b,#ffd166)">'),
 ('<i id="bBoss" style="background:linear-gradient(90deg,#6a1a14,#e5604d)">', '<i id="bBoss" style="background:linear-gradient(90deg,#d63a2e,#ff7a62)">'),
 ('<b style="color:#fff">Huyết Sát Phó Tướng</b>', '<b style="color:#0f3d32">Huyết Sát Phó Tướng</b>'),
 ('<span style="color:#8fd0ef" id="cTa">', '<span style="color:#1f78d1" id="cTa">'),
 ('<span style="color:#ff8c84" id="cDich">', '<span style="color:#d9342b" id="cDich">'),
 ('<div class="hd" style="color:#ff9c90;margin-bottom:0">MA ĐẠO', '<div class="hd" style="color:#d9342b;margin-bottom:0">MA ĐẠO'),
 ('<span class="g" style="color:#e0b878">鎮</span>', '<span class="g" style="color:#a5672a">鎮</span>'),
 ('width:66px;height:66px;border-color:#b99be0"><span class="k">4</span><span class="g" style="font-size:30px;color:#e6d4ff">眼</span>', 'width:66px;height:66px;border-color:#d8c6ff"><span class="k">4</span><span class="g" style="font-size:30px;color:#7c4dcc">眼</span>'),
 ('width:56px;height:56px;border-color:#7fc77f"><span class="k">H</span><span class="g" style="font-size:24px;color:#b5e3b5">丹</span><span class="ct" id="danN" style="background:#7fc77f">', 'width:56px;height:56px;border-color:#c8f0d2"><span class="k">H</span><span class="g" style="font-size:24px;color:#1e9b4f">丹</span><span class="ct" id="danN" style="background:#22a55a">'),
 ('Bản thử nghiệm 4 · năm tướng mỗi phe · số liệu chưa cân bằng', 'Bản thử nghiệm 5 · màu sáng hiện đại · số liệu chưa cân bằng'),
 ("$('tag').textContent='Bản thử nghiệm 4 · nhân vật đang tự đánh'", "$('tag').textContent='Bản thử nghiệm 5 · nhân vật đang tự đánh'"),
])

glob([('conic-gradient(rgba(8,6,4,.72) ${k*360}deg', 'conic-gradient(rgba(15,61,50,.55) ${k*360}deg')])

# ===================== ngũ hành: thêm màu chữ đậm cho nền sáng =====================
glob([
 ("kim:{n:'Kim',g:'金',col:'236,226,190'}", "kim:{n:'Kim',g:'金',col:'236,226,190',tc:'#9a7b1c'}"),
 ("moc:{n:'Mộc',g:'木',col:'127,209,139'}", "moc:{n:'Mộc',g:'木',col:'127,209,139',tc:'#1e9b4f'}"),
 ("thuy:{n:'Thủy',g:'水',col:'111,183,255'}", "thuy:{n:'Thủy',g:'水',col:'111,183,255',tc:'#1f7fd6'}"),
 ("hoa:{n:'Hỏa',g:'火',col:'255,138,74'}", "hoa:{n:'Hỏa',g:'火',col:'255,138,74',tc:'#e0602a'}"),
 ("tho:{n:'Thổ',g:'土',col:'214,170,106'}", "tho:{n:'Thổ',g:'土',col:'214,170,106',tc:'#a5672a'}"),
 ("pha:{n:'Phá',g:'破',col:'255,215,120'}", "pha:{n:'Phá',g:'破',col:'255,215,120',tc:'#c9901a'}"),
])
in_fn('function hintText(){', [
 ('`<b style="color:rgb(${E.col})">Trận nhãn ${E.n}</b>', '`<b style="color:${E.tc}">Trận nhãn ${E.n}</b>'),
 ('<b style="color:#ffd36b">Lõi trận nhãn', '<b style="color:#d97706">Lõi trận nhãn'),
])
in_fn('function teamRow(c,ally){', [
 ('<span class="g" style="color:rgb(${E.col})">', '<span class="g" style="color:${E.tc}">'),
 ("background:${ally?'#5fb3d9':'#e5604d'}", "background:${ally?'#2f8fe0':'#e5483d'}"),
])

# ===================== thế giới: ban ngày, cỏ xanh =====================
in_fn('function makeTiles(){', [
 ("mk('pave',64,stones('#4a4336','rgba(20,16,12,.55)','140,125,100'));", "mk('pave',64,stones('#d3cab6','rgba(150,132,104,.5)','255,255,255'));"),
 ("mk('paveL',64,stones('#575041','rgba(24,20,15,.5)','170,152,120'));", "mk('paveL',64,stones('#e8e1cf','rgba(170,152,122,.45)','255,255,255'));"),
 ("mk('paveR',64,stones('#4b3c33','rgba(22,14,12,.55)','160,100,80'));", "mk('paveR',64,stones('#e2bfa6','rgba(170,110,80,.45)','255,240,230'));"),
 ("mk('paveJ',64,stones('#3e463c','rgba(16,20,16,.55)','110,150,120'));", "mk('paveJ',64,stones('#c4dcc8','rgba(100,140,110,.45)','255,255,255'));"),
 ("mk('earth',96,speck('#3a342b',['rgba(80,70,55,.5)','rgba(25,20,15,.5)','rgba(95,85,65,.35)'],240));", "mk('earth',96,speck('#e4d8b4',['rgba(200,182,140,.55)','rgba(255,255,255,.4)','rgba(175,155,115,.35)'],240));"),
 ("mk('sand',96,speck('#4c4434',['rgba(115,102,78,.5)','rgba(40,34,26,.4)'],280));", "mk('sand',96,speck('#eedcab',['rgba(210,185,130,.55)','rgba(255,255,255,.45)'],280));"),
 ("mk('soot',96,speck('#2f2b27',['rgba(15,12,10,.6)','rgba(70,60,50,.4)','rgba(130,60,30,.25)'],260));", "mk('soot',96,speck('#c9c1b5',['rgba(140,130,118,.5)','rgba(255,255,255,.4)','rgba(220,120,70,.25)'],260));"),
])
in_fn('function roofBox(', [
 ("c.strokeStyle='rgba(12,10,8,.5)';c.lineWidth=1;", "c.strokeStyle='rgba(0,0,0,.16)';c.lineWidth=1;"),
])
glob([
 ("const MINP=[['#2b2422','#352c29','#4a3b2e','#6b5a45'],['#3a3029','#463a31','#4f4032','#7a6648'],['#5a4a2e','#6a5836','#4a3b2e','#8a7646'],['#262628','#303034','#45403a','#6b6658']];",
  "const MINP=[['#5f93b0','#74a9c6','#efe5d0','#3d6e8a'],['#d06a4c','#e0805f','#f3e8d4','#a24c35'],['#e0aa4c','#ecbe64','#f3e8d4','#b07a2c'],['#3fa395','#56baac','#efe5d0','#2a7a6e']];"),
 ("const AWN=['#8a2a22','#2a4a6a','#8a6a22','#3a5a3a','#6a2a5a'];", "const AWN=['#ef5b47','#3d9be9','#f6c445','#42c27a','#a774e0'];"),
])
in_fn('function drawBuilding(c,b){', [
 ("c.fillStyle='rgba(0,0,0,.42)';c.fillRect(-w/2+ox-4,", "c.fillStyle='rgba(30,70,45,.22)';c.fillRect(-w/2+ox-4,"),
 ("case 'farm':roofBox(c,w,h,'#6a5a34','#7a683c','#4a3b2e','#8a7646',2);", "case 'farm':roofBox(c,w,h,'#e6b85c','#f0c870','#f3e8d4','#b38638',2);"),
 ("case 'jiu':roofBox(c,w,h,'#3d1d1a','#4d2622','#4a3527','#d9b46a',3);c.fillStyle='#8a1f1c';", "case 'jiu':roofBox(c,w,h,'#d0453a','#e05a48','#f3e8d4','#f6c445',3);c.fillStyle='#e74c3c';"),
 ("c.fillStyle='#f3d9a0';c.font='12px \"Ma Shan Zheng\"';c.textAlign='center';c.textBaseline='middle';c.fillText('酒'", "c.fillStyle='#fff6d6';c.font='12px \"Ma Shan Zheng\"';c.textAlign='center';c.textBaseline='middle';c.fillText('酒'"),
 ("case 'ku':roofBox(c,w,h,'#22252a','#2c3036','#3b3a38','#b08840',4);c.fillStyle='#5a4320';", "case 'ku':roofBox(c,w,h,'#3f78ad','#4d8cc4','#efe5d0','#f2b84a',4);c.fillStyle='#b07a2c';"),
 ("case 'ying':roofBox(c,w,h,'#2a1e1e','#352626','#3a2c26','#7a3a30',3);c.fillStyle='#b8261e';", "case 'ying':roofBox(c,w,h,'#9a4652','#b05a66','#efe5d0','#6a2a36',3);c.fillStyle='#e8473c';"),
 ("c.strokeStyle='#2a1f14';c.lineWidth=2;for(const s of [-1,1]){c.beginPath();c.moveTo(s*(w/2+2),-h/2-16);", "c.strokeStyle='#7a5a3a';c.lineWidth=2;for(const s of [-1,1]){c.beginPath();c.moveTo(s*(w/2+2),-h/2-16);"),
 ("case 'duan':roofBox(c,w,h,'#252220','#2f2a27','#3a3029','#5a4a3a',3);c.fillStyle='#1a1614';", "case 'duan':roofBox(c,w,h,'#73828f','#8594a1','#e6dccb','#4d5965',3);c.fillStyle='#4a4f55';"),
 ("case 'dan':roofBox(c,w,h,'#1d2c28','#26392f','#3a3a30','#c9a85a',3);", "case 'dan':roofBox(c,w,h,'#2fa37f','#40ba93','#f3e8d4','#f2c95c',3);"),
 ("case 'palace':{c.fillStyle='#5a2a22';c.fillRect(-w/2,-h/2,w,h);roofBox(c,w+20,h+16,'#2a1c1a','#352320','#4a2a22','#d9b46a',5);roofBox(c,w*0.6,h*0.5,'#3a2622','#46302a','#4a2a22','#f3d9a0',4);c.fillStyle='#b8261e';",
  "case 'palace':{c.fillStyle='#c9402f';c.fillRect(-w/2,-h/2,w,h);roofBox(c,w+20,h+16,'#e3a636','#f0b94a','#c9402f','#fff1b8',5);roofBox(c,w*0.6,h*0.5,'#eab040','#f6c75a','#c9402f','#fff6cc',4);c.fillStyle='#d63a2e';"),
 ("c.fillStyle='#f3d9a0';c.font='28px \"Ma Shan Zheng\"';c.textAlign='center';c.textBaseline='middle';c.fillText('城主府'", "c.fillStyle='#8a3a22';c.font='28px \"Ma Shan Zheng\"';c.textAlign='center';c.textBaseline='middle';c.fillText('城主府'"),
 ("case 'hall':roofBox(c,w,h,'#2a1c1a','#352320','#4a2a22','#d9b46a',4);c.fillStyle='#b8261e';", "case 'hall':roofBox(c,w,h,'#e3a636','#f0b94a','#c9402f','#fff1b8',4);c.fillStyle='#d63a2e';"),
 ("case 'pav':{c.fillStyle='#2e2622';", "case 'pav':{c.fillStyle='#3fa395';"),
 ("c.fillStyle='#3a302a';c.beginPath();c.moveTo(w/2+6,-h/2-6);", "c.fillStyle='#56baac';c.beginPath();c.moveTo(w/2+6,-h/2-6);"),
 ("c.fillStyle='#262020';c.beginPath();c.moveTo(-w/2-6,h/2+6);", "c.fillStyle='#2e857a';c.beginPath();c.moveTo(-w/2-6,h/2+6);"),
 ("c.fillStyle='#332a26';c.beginPath();c.moveTo(-w/2-6,-h/2-6);c.lineTo(-w/2-6,h/2+6);c.lineTo(0,0);c.fill();c.fillStyle='#d9b46a';", "c.fillStyle='#4aae9f';c.beginPath();c.moveTo(-w/2-6,-h/2-6);c.lineTo(-w/2-6,h/2+6);c.lineTo(0,0);c.fill();c.fillStyle='#f6c445';"),
 ("c.fillStyle=k%2?'#2d2422':'#3a2e2a';octa(c,0,-k*7,r);c.fill();c.strokeStyle='#b08840';", "c.fillStyle=k%2?'#d0453a':'#e25e4c';octa(c,0,-k*7,r);c.fill();c.strokeStyle='#f6c445';"),
 ("c.fillStyle='#d9b46a';cc(c,0,-30,7);", "c.fillStyle='#f6c445';cc(c,0,-30,7);"),
])
in_fn('function tree(c,x,y,r){', [
 ("c.fillStyle='rgba(0,0,0,.35)';", "c.fillStyle='rgba(20,80,40,.26)';"),
 ("c.fillStyle=k===0?'#1e2a1c':k===1?'#28382440':'#3a4e3088';", "c.fillStyle=k===0?'#2c8a4a':k===1?'#3fa95cdd':'#74cf7ccc';"),
])
in_fn('function rock(c,x,y,s,v){', [
 ("c.fillStyle='rgba(0,0,0,.4)';", "c.fillStyle='rgba(30,70,40,.22)';"),
 ("c.fillStyle=v?'#6f685c':'#5d574d';", "c.fillStyle=v?'#c4c0b4':'#adb0a6';"),
 ("c.fillStyle='rgba(255,255,255,.09)';", "c.fillStyle='rgba(255,255,255,.45)';"),
])
in_fn('function tent(c,x,y,ma){', [
 ("c.fillStyle='rgba(0,0,0,.4)';", "c.fillStyle='rgba(30,70,40,.22)';"),
 ("c.fillStyle=ma?'#3a2a26':'#cfc4ad';", "c.fillStyle=ma?'#8e3d48':'#ffffff';"),
 ("c.fillStyle=ma?'#2a1c1a':'#a89c84';", "c.fillStyle=ma?'#6e2a35':'#d6e6ee';"),
 ("c.strokeStyle='#3a2f20';", "c.strokeStyle='#7a5a3a';"),
 ("c.fillStyle=ma?'#d9534f':'#5fb3d9';", "c.fillStyle=ma?'#e5483d':'#2f8fe0';"),
])
in_fn('function wallSeg(R,T,a0,a1,cren){', [
 ("c.strokeStyle='rgba(0,0,0,.45)';c.lineWidth=T+10;", "c.strokeStyle='rgba(30,70,45,.22)';c.lineWidth=T+10;"),
 ("c.strokeStyle='#4f483d';c.lineWidth=T;", "c.strokeStyle='#c2b9a4';c.lineWidth=T;"),
 ("c.strokeStyle='#6e6555';c.lineWidth=T-12;", "c.strokeStyle='#e3dbc8';c.lineWidth=T-12;"),
 ("c.strokeStyle='rgba(30,26,20,.45)';c.lineWidth=1;", "c.strokeStyle='rgba(150,130,100,.4)';c.lineWidth=1;"),
 ("c.fillStyle='#7d7362';", "c.fillStyle='#f6f1e4';"),
])
in_fn('function towerP(p,a,s,gate){', [
 ("c.fillStyle='rgba(0,0,0,.45)';", "c.fillStyle='rgba(30,70,45,.22)';"),
 ("c.fillStyle='#5d5446';", "c.fillStyle='#e0d6bf';"),
 ("c.fillStyle='#2b2422';", "c.fillStyle='#d0453a';"),
 ("c.fillStyle='#352c29';", "c.fillStyle='#e25e4c';"),
 ("c.fillStyle='#241e1c';", "c.fillStyle='#b8392f';"),
 ("c.fillStyle='#2f2724';", "c.fillStyle='#c94a3c';"),
 ("c.strokeStyle='#b08840';", "c.strokeStyle='#f6c445';"),
 ("c.fillStyle='#d9b46a';", "c.fillStyle='#f6c445';"),
])
in_fn('function genWorld(){', [
 ("const tone=rnd()<0.55?'64,78,46':'52,42,30',al=0.16+rnd()*0.22;", "const q=rnd(),tone=q<0.4?'176,214,96':q<0.75?'84,168,72':'215,222,140',al=0.16+rnd()*0.24;"),
 ("blob(1,CAMP.x+150,CAMP.y,560,420,'rgba(110,95,68,.24)');", "blob(1,CAMP.x+150,CAMP.y,560,420,'rgba(240,224,170,.55)');"),
 ("blob(1,EM.x,EM.y,540,500,'rgba(14,26,14,.62)');", "blob(1,EM.x,EM.y,540,500,'rgba(36,128,64,.5)');"),
 ("blob(1,ET.x,ET.y,520,460,'rgba(24,46,44,.5)');", "blob(1,ET.x,ET.y,520,460,'rgba(70,190,165,.4)');"),
 ("blob(1,EK.x-60,EK.y+40,520,460,'rgba(95,88,76,.26)');", "blob(1,EK.x-60,EK.y+40,520,460,'rgba(222,214,184,.6)');"),
 ("blob(1,EK.x-170,EK.y+120,340,260,'rgba(60,24,20,.22)');", "blob(1,EK.x-170,EK.y+120,340,260,'rgba(205,170,120,.35)');"),
 ("blob(1,EH.x,EH.y,540,480,'rgba(60,22,10,.5)');", "blob(1,EH.x,EH.y,540,480,'rgba(214,120,66,.5)');"),
 ("blob(1,EH.x+60,EH.y+30,320,260,'rgba(150,60,20,.2)');", "blob(1,EH.x+60,EH.y+30,320,260,'rgba(250,160,80,.35)');"),
 ("for(const [w,a] of [[190,.08],[140,.12],[100,.16],[60,.1]]){c.strokeStyle=`rgba(110,95,68,${a})`;", "for(const [w,a] of [[190,.16],[140,.24],[100,.32],[60,.24]]){c.strokeStyle=`rgba(240,226,180,${a})`;"),
 ("c=>{c.strokeStyle='rgba(110,95,68,.13)';c.lineWidth=110;", "c=>{c.strokeStyle='rgba(240,226,180,.4)';c.lineWidth=110;"),
 ("for(const [w,a] of [[120,.07],[80,.1],[44,.12]]){c.strokeStyle=`rgba(110,95,68,${a})`;", "for(const [w,a] of [[120,.16],[80,.22],[44,.28]]){c.strokeStyle=`rgba(240,226,180,${a})`;"),
 ("c=>{c.fillStyle='#1b3234';c.beginPath();c.ellipse(x,y,rx,ry,0,0,TAU);c.fill();c.strokeStyle='rgba(140,190,190,.16)';", "c=>{c.fillStyle='#4cb7d6';c.beginPath();c.ellipse(x,y,rx,ry,0,0,TAU);c.fill();c.strokeStyle='rgba(255,255,255,.55)';"),
 ("c.fillStyle='rgba(120,170,120,.5)';for(let k=0;k<3;k++)", "c.fillStyle='rgba(70,170,80,.95)';for(let k=0;k<3;k++)"),
 ("c.strokeStyle='rgba(140,150,80,.7)';c.lineWidth=1.3;", "c.strokeStyle='rgba(50,140,60,.85)';c.lineWidth=1.3;"),
 ("c.strokeStyle='rgba(18,8,4,.85)';c.lineWidth=6;c.stroke();c.strokeStyle='rgba(255,110,40,.6)';", "c.strokeStyle='rgba(150,60,30,.75)';c.lineWidth=6;c.stroke();c.strokeStyle='rgba(255,200,90,.9)';"),
 ("c.fillStyle='rgba(140,46,20,.35)';", "c.fillStyle='rgba(230,110,50,.35)';"),
 ("c=>{c.strokeStyle='#1a120e';c.lineWidth=3;c.lineCap='round';", "c=>{c.strokeStyle='#8a6446';c.lineWidth=3;c.lineCap='round';"),
 ("c=>{c.fillStyle='#2a2018';c.beginPath();c.ellipse(x,y+3,11,5,0,0,TAU);c.fill();}", "c=>{c.fillStyle='#8a6a4a';c.beginPath();c.ellipse(x,y+3,11,5,0,0,TAU);c.fill();}"),
 ("c.fillStyle='#303b26';c.fill();", "c.fillStyle='#93cf62';c.fill();"),
 ("const col=['#3d4a2a','#4a5530','#56583a','#3a4428','#4d4a2c'][Math.floor(rnd()*5)];", "const col=['#7fca56','#9ed862','#bcdc6c','#6cbd50','#cfd872'][Math.floor(rnd()*5)];"),
 ("c.strokeStyle='rgba(20,26,14,.45)';c.lineWidth=1.2;", "c.strokeStyle='rgba(60,120,40,.3)';c.lineWidth=1.2;"),
 ("c.strokeStyle='rgba(110,95,68,.4)';c.lineWidth=2;sector(c,fa,fb,rr,rr+100);", "c.strokeStyle='rgba(236,222,176,.95)';c.lineWidth=3;sector(c,fa,fb,rr,rr+100);"),
 ("c.fillStyle='rgba(150,130,95,.22)';c.fill();c.strokeStyle='rgba(60,50,36,.6)';", "c.fillStyle='rgba(246,232,190,.7)';c.fill();c.strokeStyle='rgba(190,165,120,.7)';"),
 ("c=>{c.strokeStyle='#2a1f14';c.lineWidth=2.5;c.beginPath();c.moveTo(p.x,p.y);c.lineTo(p.x,p.y-18);", "c=>{c.strokeStyle='#8a6446';c.lineWidth=2.5;c.beginPath();c.moveTo(p.x,p.y);c.lineTo(p.x,p.y-18);"),
 ("c.fillStyle='#8a6a40';cc(c,p.x,p.y-20,4);", "c.fillStyle='#e0a95a';cc(c,p.x,p.y-20,4);"),
 ("c.fillStyle=pat(c,'pave');c.fill();c.strokeStyle='rgba(18,14,10,.6)';c.lineWidth=3;for(const r of", "c.fillStyle=pat(c,'pave');c.fill();c.strokeStyle='rgba(160,140,110,.7)';c.lineWidth=3;for(const r of"),
 ("c.fillStyle=pat(c,'pave');c.fill();c.strokeStyle='rgba(18,14,10,.6)';c.lineWidth=3;c.beginPath();", "c.fillStyle=pat(c,'pave');c.fill();c.strokeStyle='rgba(160,140,110,.7)';c.lineWidth=3;c.beginPath();"),
 ("c.strokeStyle='rgba(170,150,115,.16)';c.lineWidth=34;", "c.strokeStyle='rgba(255,255,255,.4)';c.lineWidth=34;"),
 ("c.fillStyle='rgba(60,80,50,.5)';c.beginPath();c.ellipse(C.x-300,C.y+300,190,150,0,0,TAU);", "c.fillStyle='rgba(110,200,100,.8)';c.beginPath();c.ellipse(C.x-300,C.y+300,190,150,0,0,TAU);"),
 ("c.fillStyle='rgba(170,150,115,.2)';c.fillRect(C.x-IN_R,C.y-40,IN_R,80);", "c.fillStyle='rgba(255,255,255,.4)';c.fillRect(C.x-IN_R,C.y-40,IN_R,80);"),
 ("c.fillStyle='rgba(30,24,18,.25)';c.fillRect(C.x-290,C.y-610,580,300);c.strokeStyle='#6a5a44';", "c.fillStyle='rgba(255,244,214,.6)';c.fillRect(C.x-290,C.y-610,580,300);c.strokeStyle='#d2ab62';"),
 ("c.fillStyle='#4a4436';c.beginPath();c.ellipse(pp.x,pp.y,200,110,0,0,TAU);c.fill();c.fillStyle='#1b3234';", "c.fillStyle='#d6ccb6';c.beginPath();c.ellipse(pp.x,pp.y,200,110,0,0,TAU);c.fill();c.fillStyle='#4cb7d6';"),
 ("c.strokeStyle='rgba(140,190,190,.2)';c.lineWidth=2;c.beginPath();c.ellipse(pp.x,pp.y,170,84,", "c.strokeStyle='rgba(255,255,255,.6)';c.lineWidth=2;c.beginPath();c.ellipse(pp.x,pp.y,170,84,"),
 ("c.fillStyle='rgba(110,170,110,.6)';for(let k=0;k<14;k++)", "c.fillStyle='rgba(70,170,80,.95)';for(let k=0;k<14;k++)"),
 ("c.fillStyle='#e8a0b0';for(let k=0;k<5;k++)", "c.fillStyle='#ff8fb3';for(let k=0;k<5;k++)"),
 ("c.fillStyle='rgba(0,0,0,.35)';octa(c,C.x+4,C.y+6,FLAG_R+8);c.fill();c.fillStyle='#5a5244';octa(c,C.x,C.y,FLAG_R+8);c.fill();c.fillStyle='#6b6252';octa(c,C.x,C.y,FLAG_R-4);c.fill();c.strokeStyle='rgba(217,180,106,.38)';",
  "c.fillStyle='rgba(30,70,45,.2)';octa(c,C.x+4,C.y+6,FLAG_R+8);c.fill();c.fillStyle='#d9cfb8';octa(c,C.x,C.y,FLAG_R+8);c.fill();c.fillStyle='#f2ead8';octa(c,C.x,C.y,FLAG_R-4);c.fill();c.strokeStyle='rgba(16,163,127,.6)';"),
 ("c.fillStyle='rgba(30,24,18,.35)';c.fillText('浮雲',C.x,C.y+34);", "c.fillStyle='rgba(16,120,95,.28)';c.fillText('浮雲',C.x,C.y+34);"),
 ("prim(3,x-4,y-4,x+4,y+4,c=>{c.fillStyle='#2a221a';c.fillRect(x-3,y-3,6,6);});", "prim(3,x-4,y-4,x+4,y+4,c=>{c.fillStyle='#8a6446';c.fillRect(x-3,y-3,6,6);});"),
 ("c.fillStyle='rgba(0,0,0,.4)';c.beginPath();c.ellipse(p.x+3,p.y+5,12,6,0,0,TAU);c.fill();c.fillStyle='#6a5430';cc(c,p.x,p.y,10);c.fill();c.strokeStyle='#c9a85a';c.lineWidth=2;cc(c,p.x,p.y,10);c.stroke();c.fillStyle='#2a3a2a';",
  "c.fillStyle='rgba(30,70,45,.22)';c.beginPath();c.ellipse(p.x+3,p.y+5,12,6,0,0,TAU);c.fill();c.fillStyle='#d6b06a';cc(c,p.x,p.y,10);c.fill();c.strokeStyle='#f2c95c';c.lineWidth=2;cc(c,p.x,p.y,10);c.stroke();c.fillStyle='#3fbf8f';"),
])
in_fn('function bgFill(c){', [
 ("bg.addColorStop(0,'#2a2b1f');bg.addColorStop(1,'#24221a');", "bg.addColorStop(0,'#8fd062');bg.addColorStop(1,'#72bd55');"),
])
in_fn('function tufts(c,i,j){', [
 ("c.strokeStyle=`rgba(${95+a*40|0},${112+b*40|0},${62+q*24|0},${0.22+d1*0.35})`;", "c.strokeStyle=d1<0.7?`rgba(${48+a*30|0},${132+b*36|0},${44+q*24|0},${0.3+d1*0.4})`:`rgba(${200+a*40|0},${236+b*19|0},${130+q*40|0},${0.4+d2*0.3})`;"),
])
in_fn('function genVign(){', [
 ("gr.addColorStop(1,'rgba(6,4,2,.6)');", "gr.addColorStop(1,'rgba(10,50,35,.16)');"),
])

# ===================== render: vết, khói, nhãn khu =====================
in_fn('function render(){', [
 ("ctx.fillStyle=d.type==='inkR'?`rgba(70,10,10,${0.45*a})`:`rgba(15,40,60,${0.45*a})`;", "ctx.fillStyle=d.type==='inkR'?`rgba(160,50,40,${0.15*a})`:`rgba(40,100,160,${0.13*a})`;"),
 ("g.addColorStop(0,`rgba(10,8,6,${0.6*a})`);g.addColorStop(1,'rgba(10,8,6,0)');", "g.addColorStop(0,`rgba(90,80,60,${0.3*a})`);g.addColorStop(1,'rgba(90,80,60,0)');"),
 ("g.addColorStop(0,`rgba(90,10,10,${0.5*a})`);g.addColorStop(1,'rgba(90,10,10,0)');", "g.addColorStop(0,`rgba(170,40,40,${0.35*a})`);g.addColorStop(1,'rgba(170,40,40,0)');"),
 ("ctx.fillStyle='rgba(15,12,18,.55)';ell(d.x,d.y+4,d.r,d.r*0.45);", "ctx.fillStyle='rgba(90,80,70,.45)';ell(d.x,d.y+4,d.r,d.r*0.45);"),
 ("ctx.strokeStyle=`rgba(30,20,12,${0.75*a})`;ctx.lineWidth=2.5;", "ctx.strokeStyle=`rgba(110,90,60,${0.7*a})`;ctx.lineWidth=2.5;"),
 ("ctx.fillStyle=`rgba(70,64,58,${0.32*k})`;circ(f.x,f.y,f.s*(2-k));", "ctx.fillStyle=`rgba(240,240,236,${0.45*k})`;circ(f.x,f.y,f.s*(2-k));"),
 ("ctx.strokeStyle=`rgba(10,8,6,${0.75*al})`;ctx.strokeText(L.g,px,py-10);ctx.fillStyle=`rgba(243,217,160,${0.95*al})`;", "ctx.strokeStyle=`rgba(15,61,50,${0.7*al})`;ctx.strokeText(L.g,px,py-10);ctx.fillStyle=`rgba(255,255,255,${0.98*al})`;"),
 ("ctx.fillStyle=`rgba(239,228,207,${0.92*al})`;ctx.fillText(L.n,px,py+20);", "ctx.fillStyle=`rgba(255,255,255,${0.96*al})`;ctx.fillText(L.n,px,py+20);"),
 ("ctx.strokeStyle=`rgba(10,8,6,${0.75*al})`;ctx.strokeText(L.n,px,py);ctx.fillStyle=`rgba(200,220,180,${0.9*al})`;", "ctx.strokeStyle=`rgba(15,61,50,${0.7*al})`;ctx.strokeText(L.n,px,py);ctx.fillStyle=`rgba(255,255,255,${0.96*al})`;"),
 ("if(hero.dead){ctx.fillStyle='rgba(60,10,10,.35)';", "if(hero.dead){ctx.fillStyle='rgba(120,30,30,.22)';"),
])
in_fn('function drawUnit(u,x,y){', [
 ("ctx.fillStyle='rgba(0,0,0,.35)';ell(x,y+r*0.55,r*1.1,r*0.42);ctx.fill();", "ctx.fillStyle='rgba(20,70,40,.3)';ell(x,y+r*0.55,r*1.1,r*0.42);ctx.fill();"),
 ("ctx.strokeStyle=u.team?'rgba(229,96,77,.55)':'rgba(95,179,217,.55)';ctx.lineWidth=1.5;", "ctx.strokeStyle=u.team?'rgba(229,72,61,.85)':'rgba(47,143,224,.85)';ctx.lineWidth=1.8;"),
 ("hpBar(x-r,y-r*1.75,r*2,3,u.hp/u.max,u.team?'#e5604d':'#5fb3d9');", "hpBar(x-r,y-r*1.75,r*2,3,u.hp/u.max,u.team?'#e5483d':'#2f8fe0');"),
])

# ===================== quân: màu tươi hơn =====================
glob([
 ("body:'#2d6a8a',trim:'#8fd0ef'", "body:'#2f86c8',trim:'#d6efff'"),
 ("body:'#1f5670',trim:'#d9b46a'", "body:'#2369a8',trim:'#ffd36b'"),
 ("body:'#3b7d8f',trim:'#efe4cf'", "body:'#3fa2bc',trim:'#ffffff'"),
 ("body:'#2f8a76',trim:'#a8f0d8'", "body:'#1fa98a',trim:'#d8fff2'"),
 ("body:'#2d5a5a',trim:'#6fc3a8'", "body:'#2f8a7a',trim:'#b4f2dc'"),
 ("body:'#7a2420',trim:'#ff8c84'", "body:'#c0392b',trim:'#ffb3aa'"),
 ("body:'#5a1f22',trim:'#e5604d'", "body:'#9b2d30',trim:'#ff8c84'"),
 ("body:'#2a1214',trim:'#d9534f'", "body:'#5e1a20',trim:'#ff6a5a'"),
 ("robe:'#8a3a22',cape:'#4a1a12',trim:'#ffb070'", "robe:'#e0582e',cape:'#a23a1c',trim:'#ffd08a'"),
 ("robe:'#6a5434',cape:'#3a2c1c',trim:'#e0b878'", "robe:'#b58a4a',cape:'#7a5a30',trim:'#ffe2a6'"),
 ("robe:'#2a4f7a',cape:'#18304e',trim:'#9fd0ff'", "robe:'#3a84d6',cape:'#22568f',trim:'#d6ecff'"),
 ("robe:'#2f6a44',cape:'#1c4029',trim:'#9fe0b0'", "robe:'#2faa62',cape:'#1c7040',trim:'#ccf7d8'"),
 ("robe:'#2a2224',cape:'#120e10',trim:'#c8c0b8'", "robe:'#433a3e',cape:'#241e22',trim:'#e0d8d0'"),
 ("robe:'#26302a',cape:'#101810',trim:'#8fe0a0'", "robe:'#35503e',cape:'#1c2c22',trim:'#a8f0b8'"),
 ("robe:'#3a2e1e',cape:'#1c140a',trim:'#e8c870'", "robe:'#6e5532',cape:'#3e2e18',trim:'#ffe08a'"),
 ("robe:'#3a1838',cape:'#1e0a1c',trim:'#e080d0'", "robe:'#7a2a72',cape:'#4a1446',trim:'#ffa8f0'"),
])

# ===================== bản đồ nhỏ =====================
in_fn('function drawMini(){', [
 ("mx.fillStyle='#1e1d16';mx.fillRect(0,0,202,170);", "mx.fillStyle='#a6d987';mx.fillRect(0,0,202,170);"),
 ("mx.fillStyle='rgba(70,62,50,.9)';", "mx.fillStyle='#ece3cc';"),
 ("mx.strokeStyle='rgba(140,125,100,.5)';", "mx.strokeStyle='rgba(200,185,150,.8)';"),
 ("mx.strokeStyle='rgba(217,180,106,.7)';mx.lineWidth=2;", "mx.strokeStyle='rgba(150,125,80,.9)';mx.lineWidth=2;"),
 ("mx.strokeStyle='rgba(255,140,180,.85)';", "mx.strokeStyle='rgba(236,72,153,.9)';"),
 ("mx.fillStyle=G.flagOwner===0?'#5fb3d9':'#d9534f';", "mx.fillStyle=G.flagOwner===0?'#1f78d1':'#d9342b';"),
 ("mx.strokeStyle='#999';", "mx.strokeStyle='#5d7a71';"),
 ("mx.strokeStyle=`rgba(205,170,255,${0.35+0.25*Math.sin(G.t*3+e.i)})`;", "mx.strokeStyle=`rgba(124,77,204,${0.45+0.3*Math.sin(G.t*3+e.i)})`;"),
 ("mx.fillStyle=u.team?'#e5604d':'#5fb3d9';", "mx.fillStyle=u.team?'#d9342b':'#1f78d1';"),
 ("mx.strokeStyle='rgba(143,208,239,.7)';", "mx.strokeStyle='rgba(31,120,209,.85)';"),
 ("mx.strokeStyle='rgba(255,140,120,.7)';", "mx.strokeStyle='rgba(217,52,43,.85)';"),
 ("mx.fillStyle=c===hero?'#f3d9a0':c.team?'#ff6a5a':'#8fd0ef';mx.strokeStyle='#0a0806';", "mx.fillStyle=c===hero?'#ffc531':c.team?'#e5302a':'#1f78d1';mx.strokeStyle='#ffffff';"),
 ("mx.strokeStyle='rgba(239,228,207,.6)';mx.lineWidth=1;", "mx.strokeStyle='rgba(15,61,50,.8)';mx.lineWidth=1.2;"),
])

# ===================== sa bàn: lớp phủ xanh đậm kiểu hiện đại =====================
in_fn('function drawStrat(a,S0){', [
 ("ctx.fillStyle='#0e0c09';ctx.fillRect(0,0,VW,VH);", "ctx.fillStyle='#0f2a22';ctx.fillRect(0,0,VW,VH);"),
 ("ctx.fillStyle='rgba(30,22,12,.34)';ctx.fillRect(ox,0,WW*S,VH);", "ctx.fillStyle='rgba(6,28,20,.16)';ctx.fillRect(ox,0,WW*S,VH);"),
 ("ctx.strokeStyle='rgba(217,180,106,.55)';ctx.lineWidth=2;ctx.strokeRect(ox+1,1,WW*S-2,VH-2);", "ctx.strokeStyle='rgba(110,231,183,.6)';ctx.lineWidth=2;ctx.strokeRect(ox+1,1,WW*S-2,VH-2);"),
 ("ctx.strokeStyle='rgba(255,140,190,.35)';ctx.lineWidth=8;", "ctx.strokeStyle='rgba(236,72,153,.4)';ctx.lineWidth=8;"),
 ("ctx.strokeStyle='rgba(210,200,180,.55)';ctx.lineWidth=1.5;", "ctx.strokeStyle='rgba(255,255,255,.75)';ctx.lineWidth=1.5;"),
 ("ctx.fillStyle=e.dead?'#c9e6b0':'#f3d9a0';ctx.fillText(s,lx,ly);", "ctx.fillStyle=e.dead?'#b9f5d0':'#ffffff';ctx.fillText(s,lx,ly);"),
 ("ctx.fillStyle=u.team?'rgba(235,100,80,.9)':'rgba(130,205,245,.95)';", "ctx.fillStyle=u.team?'rgba(229,48,42,.95)':'rgba(31,120,209,.95)';"),
 ("ctx.fillStyle='rgba(12,10,8,.82)';ctx.fillRect(ox+20,14,tw,T.sub?82:52);ctx.strokeStyle='rgba(217,180,106,.5)';", "ctx.fillStyle='rgba(255,255,255,.95)';ctx.fillRect(ox+20,14,tw,T.sub?82:52);ctx.strokeStyle='rgba(16,163,127,.5)';"),
 ("ctx.fillStyle=T.side?'#e5604d':'#d9b46a';ctx.fillText(T.g||'謀',ox+34,41);", "ctx.fillStyle=T.side?'#e5483d':'#10a37f';ctx.fillText(T.g||'謀',ox+34,41);"),
 ("ctx.font='700 21px \"Noto Serif\"';ctx.fillStyle='#f3d9a0';", "ctx.font='700 21px \"Be Vietnam Pro\"';ctx.fillStyle='#0f3d32';"),
 ("ctx.fillStyle='rgba(217,180,106,.8)';ctx.fillText('SA BÀN THẾ TRẬN'", "ctx.fillStyle='rgba(16,163,127,.9)';ctx.fillText('SA BÀN THẾ TRẬN'"),
 ("ctx.fillStyle='#e8dcc4';const ls=wrapTxt(", "ctx.fillStyle='#3f5f56';const ls=wrapTxt("),
 ("ctx.font='700 13.5px \"Noto Serif\"';", "ctx.font='700 13.5px \"Be Vietnam Pro\"';"),
 ("ctx.fillStyle='#bfb092';ctx.fillText(E.n+' · '+D.sect", "ctx.fillStyle='#a8c4ba';ctx.fillText(E.n+' · '+D.sect"),
 ("ctx.fillStyle='rgba(0,0,0,.6)';ctx.fillRect(x0+40,y+29,150,4);ctx.fillStyle=ally?'#5fb3d9':'#e5604d';", "ctx.fillStyle='rgba(255,255,255,.18)';ctx.fillRect(x0+40,y+29,150,4);ctx.fillStyle=ally?'#38a8ff':'#ff5a4a';"),
 ("ctx.fillStyle='rgba(12,10,8,.9)';circ(x0+16,y,15);", "ctx.fillStyle='rgba(255,255,255,.96)';circ(x0+16,y,15);"),
 ("ctx.strokeStyle=ally?(c===hero?'#f3d9a0':'#5fb3d9'):'#e5604d';", "ctx.strokeStyle=ally?(c===hero?'#ffc531':'#2f8fe0'):'#e5483d';"),
 ("ctx.fillStyle=`rgb(${E.col})`;ctx.fillText(D.g,x0+16,y+1);", "ctx.fillStyle=E.tc;ctx.fillText(D.g,x0+16,y+1);"),
 ("col(G.allies,14,'CHÍNH ĐẠO · NĂM TƯỚNG','#8fd0ef',true);", "col(G.allies,14,'CHÍNH ĐẠO · NĂM TƯỚNG','#7cc8ff',true);"),
 ("col(G.foes,VW-ox+14,'MA ĐẠO · NĂM TƯỚNG','#ff8c84',false);", "col(G.foes,VW-ox+14,'MA ĐẠO · NĂM TƯỚNG','#ff8c84',false);"),
])
in_fn('function stratBadge(c,x,y){', [
 ("ctx.fillStyle='rgba(12,10,8,.92)';circ(x,y,r);ctx.fill();ctx.strokeStyle=ma?'#e5604d':(c===hero?'#f3d9a0':'#5fb3d9');", "ctx.fillStyle='rgba(255,255,255,.96)';circ(x,y,r);ctx.fill();ctx.strokeStyle=ma?'#e5483d':(c===hero?'#ffc531':'#2f8fe0');"),
 ("ctx.fillStyle=`rgb(${E.col})`;ctx.fillText(D.g,x,y+1);", "ctx.fillStyle=E.tc;ctx.fillText(D.g,x,y+1);"),
])

if bad:
    print('THEME MISMATCH'); print('\n'.join(bad)); sys.exit(1)
open(SRC, 'w', encoding='utf-8').write(H)
print('theme ok', len(H))
