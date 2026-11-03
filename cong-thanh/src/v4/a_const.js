const VW=1280,VH=720,WW=6400,WH=5800;
const C={x:3700,y:2950};
const WALL_R=1650,WALL_T=40,IN_R=700,IN_T=32,BAR_R=1800,RING_R=1150,RING_W=50,AV_W=80,FLAG_R=110;
const G_OUT=[-Math.PI/2,0,Math.PI/2,Math.PI],G_IN=[0,Math.PI/2,Math.PI];
const RINGS=[{R:WALL_R,T:WALL_T,half:100/WALL_R,gates:G_OUT},{R:IN_R,T:IN_T,half:90/IN_R,gates:G_IN}];
const CAMP={x:C.x-2750,y:C.y};
const QUAN=20,Z_FIGHT=0.88,Z_FLY=0.74,Z0=0.218,TIME=900,VULN_T=9,SEC_H=Math.PI/4,INTRO=8.6;
const ELEM={kim:{n:'Kim',g:'金',col:'236,226,190'},moc:{n:'Mộc',g:'木',col:'127,209,139'},thuy:{n:'Thủy',g:'水',col:'111,183,255'},hoa:{n:'Hỏa',g:'火',col:'255,138,74'},tho:{n:'Thổ',g:'土',col:'214,170,106'},pha:{n:'Phá',g:'破',col:'255,215,120'}};
// bốn hướng, mỗi hướng một cung kết giới do một trận nhãn ẩn nuôi
const DIRS=[{k:'W',n:'Tây',a:Math.PI},{k:'N',n:'Bắc',a:-Math.PI/2},{k:'E',n:'Đông',a:0},{k:'S',n:'Nam',a:Math.PI/2}];
const EYE_DEF=[
 {el:'kim',weak:'hoa',a:194,r:2230,seals:2,hp:6500,place:'Loạn Thạch Cương',tool:'<span class="g">雷</span> Lôi Hỏa Phù (2) hoặc Liệt Diễm của Viêm Dương',toolT:'Hỏa pháp: Lôi Hỏa Phù, Liệt Diễm',voff:-8},
 {el:'thuy',weak:'tho',a:-104,r:2200,seals:2,hp:5000,place:'Đầm Sen',tool:'<span class="g">鎮</span> Trấn Sơn Ấn (G) hoặc Thổ pháp của Thạch Nham',toolT:'Thổ pháp: Trấn Sơn Ấn',voff:9},
 {el:'hoa',weak:'thuy',a:14,r:2200,seals:2,hp:5000,place:'Hỏa Diệm Cốc',tool:'Huyền Băng Tiễn của Thủy Nguyệt, hoặc <span class="g">破</span> Phá Trận Phù (F)',toolT:'Thủy pháp: Huyền Băng Tiễn',voff:-9},
 {el:'moc',weak:'kim',a:104,r:2200,seals:3,hp:6000,place:'Hắc Lâm',tool:'<span class="g">御</span> Ngự Kiếm (1)',toolT:'Kim pháp: Ngự Kiếm',voff:-9},
];
const EYE_POS=EYE_DEF.map(d=>({x:C.x+Math.cos(deg(d.a))*d.r,y:C.y+Math.sin(deg(d.a))*d.r}));

const TYPES={
 hero:{name:'Ngao Thiên',team:0,hp:3600,atk:0,range:0,spd:165,cd:0,r:14,kind:'hero'},
 viem:{name:'Viêm Dương',team:0,hp:3000,atk:44,range:230,spd:150,cd:1.1,r:14,kind:'champ'},
 thach:{name:'Thạch Nham',team:0,hp:4200,atk:56,range:30,spd:140,cd:1.0,r:15,kind:'champ'},
 nguyet:{name:'Thủy Nguyệt',team:0,hp:2900,atk:42,range:240,spd:152,cd:1.0,r:14,kind:'champ'},
 thanh:{name:'Thanh Mộc',team:0,hp:3300,atk:40,range:32,spd:150,cd:0.95,r:14,kind:'champ'},
 kiem:{name:'Kiếm Tu Vệ',team:0,hp:130,atk:15,range:22,spd:74,cd:0.9,r:9,kind:'melee',body:'#2d6a8a',trim:'#8fd0ef'},
 long:{name:'Long Nhân Chiến Sĩ',team:0,hp:250,atk:22,range:24,spd:64,cd:1.1,r:12,kind:'melee',body:'#1f5670',trim:'#d9b46a'},
 phu:{name:'Phù Sư',team:0,hp:90,atk:14,range:210,spd:66,cd:1.4,r:9,kind:'caster',body:'#3b7d8f',trim:'#efe4cf'},
 loan:{name:'Thanh Loan Kỵ',team:0,hp:180,atk:19,range:24,spd:128,cd:0.8,r:13,kind:'cav',body:'#2f8a76',trim:'#a8f0d8'},
 quy:{name:'Huyền Quy Công Thành',team:0,hp:1800,atk:240,range:40,spd:36,cd:2.4,r:22,kind:'siege',body:'#2d5a5a',trim:'#6fc3a8'},
 lang:{name:'Hắc Lang Vương',team:1,hp:4600,atk:68,range:30,spd:152,cd:1.0,r:15,kind:'champ'},
 thi:{name:'Âm Thi Lão Quỷ',team:1,hp:4200,atk:46,range:220,spd:134,cd:1.2,r:14,kind:'champ'},
 bo:{name:'Bổ Thiên Khách',team:1,hp:4400,atk:58,range:30,spd:148,cd:1.0,r:15,kind:'champ'},
 ta:{name:'Tà Cực Ma Nữ',team:1,hp:4000,atk:54,range:200,spd:158,cd:1.0,r:14,kind:'champ'},
 no:{name:'Huyết Nô',team:1,hp:115,atk:16,range:22,spd:72,cd:0.95,r:9,kind:'melee',body:'#7a2420',trim:'#ff8c84'},
 cung:{name:'Huyết Cung Thủ',team:1,hp:85,atk:12,range:240,spd:62,cd:1.7,r:9,kind:'archer',body:'#5a1f22',trim:'#e5604d'},
 tuong:{name:'Ma Tướng',team:1,hp:460,atk:34,range:26,spd:68,cd:1.2,r:13,kind:'melee',body:'#2a1214',trim:'#d9534f'},
 boss:{name:'Huyết Sát Phó Tướng',team:1,hp:9000,atk:110,range:40,spd:84,cd:1.5,r:24,kind:'boss'},
};
// năm tướng mỗi phe: hệ ngũ hành, tông môn, chiêu chính, dáng vẽ
const CH_DEF={
 hero:{el:'kim',g:'劍',sect:'Vạn Kiếm Tông',sk:'Ngự Kiếm'},
 viem:{el:'hoa',g:'炎',sect:'Hạo Nhiên Tông',sk:'Liệt Diễm Phù',tk:'fire',cd:7.5,robe:'#8a3a22',cape:'#4a1a12',trim:'#ffb070',w:'staff',hat:'crown'},
 thach:{el:'tho',g:'岩',sect:'Vô Cực Trận Tông',sk:'Trấn Sơn Ấn',tk:'an',cd:7,robe:'#6a5434',cape:'#3a2c1c',trim:'#e0b878',w:'shield',hat:'bun'},
 nguyet:{el:'thuy',g:'月',sect:'Vạn Pháp Tông',sk:'Huyền Băng Tiễn',tk:'ice',cd:6.5,robe:'#2a4f7a',cape:'#18304e',trim:'#9fd0ff',w:'orb',hat:'long'},
 thanh:{el:'moc',g:'龍',sect:'Vạn Thú Tông · Long tộc',sk:'Đằng La Triền',tk:'vine',cd:7,robe:'#2f6a44',cape:'#1c4029',trim:'#9fe0b0',w:'spear',hat:'horns'},
 lang:{el:'kim',g:'狼',sect:'Vạn Ma Tông',sk:'Huyết Lang Trảo',post:0,col:'230,90,70',robe:'#2a2224',cape:'#120e10',trim:'#c8c0b8',w:'claw',hat:'ears',skin:'#6a5a52'},
 thi:{el:'thuy',g:'屍',sect:'Quỷ Dị Môn',sk:'Triệu Thi',post:1,col:'120,220,140',robe:'#26302a',cape:'#101810',trim:'#8fe0a0',w:'orbG',hat:'hood',skin:'#9aa890'},
 bo:{el:'tho',g:'補',sect:'Bổ Thiên Các',sk:'Bổ Thiên Trận',post:2,col:'232,200,112',robe:'#3a2e1e',cape:'#1c140a',trim:'#e8c870',w:'staff',hat:'halo',skin:'#c9b6a0'},
 ta:{el:'hoa',g:'邪',sect:'Tà Cực Tông',sk:'Tà Cực Ma Âm',post:3,col:'224,110,210',robe:'#3a1838',cape:'#1e0a1c',trim:'#e080d0',w:'blade',hat:'long',skin:'#d8c4c0'},
 boss:{el:'hoa',g:'煞',sect:'Trấn thủ · Kết Đan'},
};
const WP=[null,{x:860,y:1750},null,{x:860,y:4150}];
const SPLIT0={hero:3,viem:0,thach:1,nguyet:1,thanh:0};
