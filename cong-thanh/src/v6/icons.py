# Bản 6: thay chữ Hán trong nút bằng biểu tượng vẽ (SVG nội tuyến), giữ chữ Hán nhỏ ở góc.
import os, re, sys
D = os.path.dirname(os.path.abspath(__file__))
P = os.path.dirname(D)
SRC = os.path.join(P, 'index.html')
H = open(SRC, encoding='utf-8').read()

def svg(body):
    return ('<svg class="ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" '
            'stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + body + '</svg>')
F = 'fill="currentColor" fill-opacity=".18"'
ICON = {
 'k_slash': svg(f'<path d="M20.5 3.5 20 6.5 10.5 16 8 13.5 17.5 4z" {F}/><path d="M6.5 12l5.5 5.5M9 15l-5 5"/><path d="M3 12.5A9.5 9.5 0 0 1 12 3" stroke-opacity=".55"/>'),
 'k_yu': svg(f'<path d="M8 10.3h10.5l3 1.7-3 1.7H8z" {F}/><path d="M8 8v8M8 12H3.5"/><path d="M4 5.5h7M5 18.5h6" stroke-opacity=".55"/>'),
 'k_loi': svg(f'<rect x="5" y="2.5" width="14" height="19" rx="2" {F}/><path d="M13.5 5.5 9 12.5h4l-2 6 5-7.5h-4z" fill="currentColor"/>'),
 'k_van': svg(f'<path d="M5 3v9.5l1 2.5 1-2.5V3M11 2v11.5l1 2.5 1-2.5V2M17 3v9.5l1 2.5 1-2.5V3" {F}/><path d="M4 5h4M10 4h4M16 5h4"/><path d="M3 20.5h18" stroke-opacity=".55"/>'),
 'k_yan': svg(f'<path d="M2 12.5s3.6-6.5 10-6.5 10 6.5 10 6.5-3.6 6.5-10 6.5S2 12.5 2 12.5z" {F}/><circle cx="12" cy="12.5" r="3" fill="currentColor"/><path d="M12 2v1.6M5.2 4.2l1 1.3M18.8 4.2l-1 1.3"/>'),
 'k_don': svg('<path d="M13 5l7 7-7 7"/><path d="M4 8h6M2 12h11M4 16h6" stroke-opacity=".6"/>'),
 'k_dan': svg(f'<circle cx="12" cy="14.5" r="6.5" {F}/><path d="M10 4h4v3.5h-4zM9 7.5h6"/><path d="M8.8 13.8A3.4 3.4 0 0 1 12 10.8" stroke-opacity=".6"/>'),
 's_ho': svg(f'<path d="M12 2.5 20 5.5v6c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10v-6z" {F}/><path d="M8.5 9.5h7M8.5 12.5h2.6M12.9 12.5h2.6M8.5 15.5h7"/>'),
 's_pha': svg(f'<rect x="5" y="2.5" width="14" height="19" rx="2" {F}/><path d="M12.5 2.5 10 8l3.5 3-3 4.5 1.5 6"/>'),
 's_an': svg(f'<path d="M10 2.5h4l1 5H9z"/><rect x="5" y="7.5" width="14" height="5.5" rx="1" {F}/><path d="M4 21l5-5 3 3 2.5-2.5L20 21z" fill="currentColor" fill-opacity=".35"/>'),
 's_khon': svg(f'<circle cx="12" cy="12" r="8.5" {F}/><path d="M12 3.5v17M3.5 12h17M6 6l12 12M18 6 6 18" stroke-opacity=".55"/><circle cx="12" cy="12" r="3"/>'),
 'c_auto': svg(f'<circle cx="12" cy="12" r="8.5"/><path d="M15.8 8.2 13.6 13.6 8.2 15.8l2.2-5.4z" {F}/><circle cx="12" cy="12" r="1" fill="currentColor"/>'),
 'c_split': svg('<path d="M12 21v-7M12 14 5 6M12 14l7-8M12 14V3"/><path d="M5 6v4M5 6h4M19 6v4M19 6h-4M9.5 5.5 12 3l2.5 2.5"/>'),
 'c_mass': svg('<path d="M4 4l5.5 5.5M20 4l-5.5 5.5M12 21v-5.5"/><path d="M9.5 9.5H6M9.5 9.5V6M14.5 9.5H18M14.5 9.5V6M9.5 18 12 15.5l2.5 2.5"/><circle cx="12" cy="12" r="1.8" fill="currentColor"/>'),
}
COL = {'k_yu':'#a07a12','k_loi':'#e0602a','k_van':'#0f6b55','k_yan':'#7c4dcc','k_don':'#1f7fd6','k_dan':'#1e9b4f',
       's_ho':'#1f7fd6','s_pha':'#c9901a','s_an':'#a5672a','s_khon':'#e5483d'}

bad = []
def swap(eid, html):
    global H
    m = re.search(r'id="' + eid + r'"[^>]*>', html)
    if not m:
        bad.append('no id ' + eid); return html
    i = m.end()
    g = re.compile(r'<span class="g"( style="[^"]*")?>(.)</span>').search(html, i)
    if not g or g.start() - i > 120:
        bad.append('no glyph ' + eid); return html
    ch = g.group(2)
    col = COL.get(eid)
    st = f' style="color:{col}"' if col else ''
    new = f'<span class="g ico"{st} title="{ch}">{ICON[eid]}</span><span class="hz">{ch}</span>'
    return html[:g.start()] + new + html[g.end():]

# ô Khốn Long Trận chưa có id: gắn id để thay
H = H.replace('<div class="slot lock"><span class="g">困</span>', '<div class="slot lock" id="s_khon"><span class="g">困</span>', 1)
for k in ICON:
    H = swap(k, H)

CSS = """
/* ===== bản 6: biểu tượng chiêu thức ===== */
.ico{display:flex;align-items:center;justify-content:center;line-height:0}
.ico .ic{width:30px;height:30px;display:block}
.sk.big .ico .ic{width:54px;height:54px}
#k_yan .ico .ic{width:30px;height:30px}
#k_don .ico .ic,#k_dan .ico .ic{width:25px;height:25px}
.slot .ico .ic{width:26px;height:26px}
.cbt .ico .ic{width:19px;height:19px}
.hz{position:absolute;font-family:'Ma Shan Zheng',serif;line-height:1;pointer-events:none;opacity:.55}
.sk .hz{left:50%;bottom:5px;transform:translateX(-50%);font-size:11px;color:#0f6b55}
.sk.big .hz{bottom:12px;font-size:15px;color:#fff;opacity:.8}
#k_don .hz,#k_dan .hz{display:none}
.slot .hz{left:6px;top:4px;font-size:11px;color:#0f6b55}
.cbt .hz{display:none}
.slot{height:80px}
#hint{bottom:16px!important;left:50%!important;right:auto!important;transform:translateX(-50%);white-space:nowrap;padding:7px 16px;border-radius:999px;background:rgba(10,32,26,.86);color:#e8fff6!important;text-shadow:none!important;font-weight:600;font-size:12px;transition:none!important;z-index:5;opacity:0!important}
#stage #hint.show{opacity:1!important;transition:opacity .4s!important}
.cbt{height:50px;line-height:1.15;text-align:center;padding:0 2px}
#tag{bottom:auto!important;top:158px;left:14px;padding:3px 9px;border-radius:999px;background:rgba(255,255,255,.75);color:#2c5248!important}
#stage.bossOn #camcap{top:232px}
#stage.bossOn #warn{top:278px}
"""
H = H.replace('\n</style>', CSS + '</style>', 1)
H = H.replace('Bản thử nghiệm 5 · màu sáng hiện đại', 'Bản thử nghiệm 6 · biểu tượng chiêu thức')
H = H.replace("'Bản thử nghiệm 5 · nhân vật đang tự đánh'", "'Bản thử nghiệm 6 · nhân vật đang tự đánh'")
H = H.replace('<span class="lb">Vạn Kiếm Quy Tông</span>', '<span class="lb">Vạn Kiếm</span>', 1)
H = H.replace("if(boss.active&&!boss.dead){$('bBoss')", "$('stage').classList.toggle('bossOn',$('boss').style.display==='block');if(boss.active&&!boss.dead){$('bBoss')", 1)
H = H.replace("const ls=wrapTxt(T.sub,tw-30).slice(0,2);", "let ls=wrapTxt(T.sub,tw-30);if(ls.length>2){ls=ls.slice(0,2);ls[1]=ls[1].replace(/\\s*\\S*$/,'')+'…';}", 1)
H = H.replace("$('stage').classList.toggle('bossOn'", "$('hint').classList.toggle('show',stratShown);$('stage').classList.toggle('bossOn'", 1)
if bad:
    print('ICON MISMATCH', bad); sys.exit(1)
open(SRC, 'w', encoding='utf-8').write(H)
print('icons ok', len(H))
