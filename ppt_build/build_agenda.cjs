const pptxgen = require("./pptxgen.cjs.js");

const INK="2E3322", INK2="6B6A50", CREAM="F7F1E1", PANEL="FCF8EE",
      FOREST="4A6535", MOSS="8FAE55", SAGE="C2D0AE", BROWN="8C6239", WHITE="FFFFFF";
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

const s=p.addSlide();
s.background={color:CREAM};
s.addText("CONTENTS",{x:M,y:0.7,w:6,h:0.3,fontSize:12,bold:true,color:MOSS,fontFace:SF,charSpacing:4,margin:0});
s.addText("발표 순서",{x:M,y:0.98,w:7,h:0.8,fontSize:38,bold:true,color:INK,fontFace:KF,margin:0});

const items=[
  ["01","레퍼런스 게임 소개","Stacklands"],
  ["02","SpiritStack 게임 소개","핵심 게임 루프"],
  ["03","구현 범위","Scope"],
  ["04","개발 일정 요약","약 3.5주 개발 과정"],
  ["05","차별점 ① 날씨 시스템","Weather System"],
  ["06","차별점 ② 주민 속성 시스템","Villager Attributes"],
  ["07","차별점 ③ 세계수 드롭 시스템","World Tree Drop"],
  ["08","차별점 ④ 레시피별 이펙트","Recipe Effects"],
  ["09","핵심 구조 · 설계","Architecture"],
  ["10","핵심 구현 기술 2가지","Key Tech"],
  ["11","진행 중 변경된 사항","Refactoring"],
  ["12","아쉬웠던 점 & 향후 과제","Retrospective · Future"],
];
const perCol=Math.ceil(items.length/2);
const colW=5.7, x0=M, x1=6.95, rowH=0.83, y0=1.78, bh=0.72;
items.forEach((it,i)=>{
  const col=Math.floor(i/perCol);
  const row=i%perCol;
  const x=col===0?x0:x1;
  const y=y0+row*rowH;
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y,w:colW,h:bh,rectRadius:0.1,fill:{color:PANEL},line:{color:SAGE,width:1},shadow:mkShadow()});
  s.addShape(p.shapes.OVAL,{x:x+0.22,y:y+0.13,w:0.46,h:0.46,fill:{color:col===0?FOREST:BROWN},line:{type:"none"}});
  s.addText(it[0],{x:x+0.22,y:y+0.13,w:0.46,h:0.46,fontSize:14,bold:true,color:WHITE,fontFace:SF,align:"center",valign:"middle",margin:0});
  s.addText(it[1],{x:x+0.86,y:y+0.09,w:colW-1.0,h:0.36,fontSize:15,bold:true,color:INK,fontFace:KF,align:"left",valign:"middle",margin:0});
  s.addText(it[2],{x:x+0.86,y:y+0.43,w:colW-1.0,h:0.26,fontSize:10,color:INK2,fontFace:SF,align:"left",valign:"middle",margin:0});
});
footer(s,2);

p.writeFile({fileName:"C:/Users/KJH/AppData/Local/Temp/_spirit_agenda12.pptx"}).then(f=>console.log("WROTE",f));
