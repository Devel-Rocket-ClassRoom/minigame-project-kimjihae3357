const pptxgen = require("./pptxgen.cjs.js");

const INK="2E3322", INK2="6B6A50", CREAM="F7F1E1", PANEL="FCF8EE",
      FOREST="4A6535", MOSS="8FAE55", SAGE="C2D0AE", AMBER="E0A23A",
      BROWN="8C6239", WHITE="FFFFFF";
const KF="Malgun Gothic", SF="Georgia";
const mkShadow=()=>({type:"outer",color:"3A3A2A",blur:7,offset:3,angle:90,opacity:0.18});

const p=new pptxgen();
p.layout="LAYOUT_WIDE";
const W=13.33,H=7.5,M=0.62;

function footer(slide,n){
  slide.addText([
    {text:"SpiritStack", options:{bold:true, color:FOREST}},
    {text:"  ·  카드 스태킹 생존 경영 게임", options:{color:INK2}}
  ],{x:M,y:H-0.5,w:7,h:0.3,fontSize:9,fontFace:KF,align:"left",valign:"middle",margin:0});
  slide.addShape(p.shapes.OVAL,{x:W-1.1,y:H-0.46,w:0.22,h:0.22,fill:{color:MOSS},line:{type:"none"}});
  slide.addText(String(n),{x:W-1.1,y:H-0.47,w:0.22,h:0.24,fontSize:10,bold:true,color:WHITE,fontFace:SF,align:"center",valign:"middle",margin:0});
}
function header(slide,num,kicker,title){
  slide.addShape(p.shapes.ROUNDED_RECTANGLE,{x:M,y:0.5,w:0.62,h:0.62,rectRadius:0.12,fill:{color:FOREST},line:{type:"none"},shadow:mkShadow()});
  slide.addText(String(num).padStart(2,"0"),{x:M,y:0.5,w:0.62,h:0.62,fontSize:20,bold:true,color:WHITE,fontFace:SF,align:"center",valign:"middle",margin:0});
  slide.addText(kicker,{x:M+0.82,y:0.5,w:11,h:0.26,fontSize:11.5,bold:true,color:MOSS,fontFace:KF,charSpacing:2,align:"left",valign:"middle",margin:0});
  slide.addText(title,{x:M+0.8,y:0.72,w:11.6,h:0.5,fontSize:27,bold:true,color:INK,fontFace:KF,align:"left",valign:"middle",margin:0});
}

const s=p.addSlide();
s.background={color:CREAM};
header(s,4,"TIMELINE","개발 일정 요약");
s.addText("2026. 05. 18  ~  06. 08   ·   약 3주 개발 (3단계)",{x:M+0.8,y:1.2,w:11,h:0.3,fontSize:12.5,italic:true,color:BROWN,fontFace:KF,margin:0});

const phases=[
  ["WEEK 1","05.18 ~ 05.24","코어 시스템 구축", FOREST, [
    "카드 데이터 구조 · PlayerInput 입력",
    "카메라 줌 · 이동 제어",
    "레시피 조합 · 채집 · 자동 스택",
    "스타터 카드팩 · 하루 사이클 · 식량/게임오버",
  ]],
  ["WEEK 2","05.25 ~ 05.31","시스템 확장", AMBER, [
    "상점 · 코인 경제",
    "카드팩 확률 시스템",
    "적 시스템 · 포탈 · 전투 · 드랍",
    "날씨 시스템 (4종) · URP 이펙트",
  ]],
  ["WEEK 3","06.01 ~ 06.08","콘텐츠 · 완성도", BROWN, [
    "타이틀 씬 · 시간 슬라이더 · 날씨 아이콘",
    "카드 제약 · 최대 개수 · 창고 확장",
    "세이브 / 로드 · 채집 랜덤 보상",
    "이펙트 · 사운드 · UI 보강 · 빌드 0.3.0",
  ]],
];
const pw=3.80, gap=0.35, y=1.75, ph=4.85; let x=M;
phases.forEach((ph_)=>{
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y,w:pw,h:ph,rectRadius:0.12,fill:{color:PANEL},line:{color:SAGE,width:1},shadow:mkShadow()});
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y,w:pw,h:1.18,rectRadius:0.12,fill:{color:ph_[3]},line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE,{x:x,y:y+0.7,w:pw,h:0.48,fill:{color:ph_[3]},line:{type:"none"}});
  s.addText(ph_[0],{x:x+0.24,y:y+0.16,w:pw-0.48,h:0.3,fontSize:14,bold:true,color:WHITE,fontFace:SF,charSpacing:2,margin:0});
  s.addText(ph_[1],{x:x+0.24,y:y+0.46,w:pw-0.48,h:0.28,fontSize:11.5,color:"F3EBD6",fontFace:SF,margin:0});
  s.addText(ph_[2],{x:x+0.24,y:y+0.76,w:pw-0.48,h:0.38,fontSize:16,bold:true,color:WHITE,fontFace:KF,valign:"middle",margin:0});
  s.addText(ph_[4].map((t)=>({text:t, options:{bullet:{characterCode:"2022", indent:15}, breakLine:true, paraSpaceAfter:14}})),
    {x:x+0.28,y:y+1.45,w:pw-0.52,h:ph-1.65,fontSize:13.5,color:INK,fontFace:KF,align:"left",valign:"top",margin:0});
  x += pw+gap;
});
footer(s,6);

p.writeFile({fileName:"C:/Users/KJH/AppData/Local/Temp/_spirit_timeline3.pptx"}).then(f=>console.log("WROTE",f));
