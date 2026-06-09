const pptxgen = require("./pptxgen.cjs.js");

// palette (same as main deck)
const INK="2E3322", INK2="6B6A50", CREAM="F7F1E1", PANEL="FCF8EE",
      FOREST="4A6535", FORESTD="38492A", MOSS="8FAE55", SAGE="C2D0AE",
      AMBER="E0A23A", BROWN="8C6239", RUST="C2663B", WHITE="FFFFFF";
const KF="Malgun Gothic", SF="Georgia";
const mkShadow=()=>({type:"outer",color:"3A3A2A",blur:7,offset:3,angle:90,opacity:0.18});

const p = new pptxgen();
p.layout="LAYOUT_WIDE";
const W=13.33,H=7.5,M=0.62;

const s = p.addSlide();
s.background={color:CREAM};

// header (glyph chip instead of number, to avoid section-number conflict)
s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:M,y:0.5,w:0.62,h:0.62,rectRadius:0.12,fill:{color:FOREST},line:{type:"none"},shadow:mkShadow()});
s.addText("★",{x:M,y:0.5,w:0.62,h:0.62,fontSize:22,bold:true,color:WHITE,fontFace:SF,align:"center",valign:"middle",margin:0});
s.addText("RETROSPECTIVE · FUTURE WORK",{x:M+0.82,y:0.5,w:11,h:0.26,fontSize:11.5,bold:true,color:MOSS,fontFace:KF,charSpacing:2,align:"left",valign:"middle",margin:0});
s.addText("아쉬웠던 점 & 향후 과제",{x:M+0.8,y:0.72,w:11.6,h:0.5,fontSize:27,bold:true,color:INK,fontFace:KF,align:"left",valign:"middle",margin:0});

const PW=5.84, gap=0.3, py=1.6, ph=5.05;
const LX=M, RX=M+PW+gap;

function column(x, barColor, badgeColor, title, en, items){
  // panel
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y:py,w:PW,h:ph,rectRadius:0.12,fill:{color:PANEL},line:{color:SAGE,width:1},shadow:mkShadow()});
  // title bar
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y:py,w:PW,h:0.92,rectRadius:0.12,fill:{color:barColor},line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE,{x,y:py+0.46,w:PW,h:0.46,fill:{color:barColor},line:{type:"none"}});
  s.addText(title,{x:x+0.35,y:py+0.14,w:PW-0.7,h:0.42,fontSize:18,bold:true,color:WHITE,fontFace:KF,align:"left",valign:"middle",margin:0});
  s.addText(en,{x:x+0.35,y:py+0.55,w:PW-0.7,h:0.3,fontSize:11,italic:true,color:"F3EBD6",fontFace:SF,align:"left",valign:"middle",margin:0});
  // items
  const iy0=2.7, step=1.32;
  items.forEach((it,i)=>{
    const iy=iy0+i*step;
    s.addShape(p.shapes.OVAL,{x:x+0.32,y:iy+0.04,w:0.5,h:0.5,fill:{color:badgeColor},line:{type:"none"}});
    s.addText(String(i+1),{x:x+0.32,y:iy+0.04,w:0.5,h:0.5,fontSize:15,bold:true,color:WHITE,fontFace:SF,align:"center",valign:"middle",margin:0});
    s.addText(it[0],{x:x+1.0,y:iy,w:PW-1.25,h:0.4,fontSize:14.5,bold:true,color:INK,fontFace:KF,align:"left",valign:"middle",margin:0});
    s.addText(it[1],{x:x+1.0,y:iy+0.42,w:PW-1.3,h:0.82,fontSize:12,color:INK2,fontFace:KF,align:"left",valign:"top",margin:0});
  });
}

column(LX, RUST, RUST, "아쉬웠던 점", "Lessons Learned", [
  ["데이터 외부 연동 부재","초반에 CSV·구글 시트를 연동했다면, 협업 시 밸런스 수치 조정이 훨씬 편했을 것."],
  ["로컬라이징 미고려","텍스트를 스트링 테이블로 분리하지 않아, 다국어(로컬라이징) 확장을 초기에 고려하지 못함."],
  ["세이브 구조의 잦은 변경","세이브/로드를 후반에 추가하며 데이터 구조가 자주 바뀌어 호환성 관리가 번거로웠음."],
]);

column(RX, FOREST, MOSS, "향후 추가하고 싶은 것", "Future Work", [
  ["지역 이동 · 포탈 탐험","레퍼런스(Stacklands)에서 재밌었던 요소 — 포탈로 다른 지역에 가서 탐험하는 콘텐츠."],
  ["데이터 · 로컬라이징 파이프라인","CSV·시트 연동과 스트링 테이블을 도입해 밸런싱·다국어를 체계화 (위 아쉬운 점 개선)."],
  ["튜토리얼 · 콘텐츠 확장","신규 플레이어용 인게임 튜토리얼 정식 도입 + 카드팩·퀘스트 등 즐길 거리 확장."],
]);

// footer (brand only — no page number, to match whatever page count the edited deck has)
s.addText([
  {text:"SpiritStack", options:{bold:true, color:FOREST}},
  {text:"  ·  카드 스태킹 생존 경영 게임", options:{color:INK2}}
],{x:M,y:H-0.5,w:8,h:0.3,fontSize:9,fontFace:KF,align:"left",valign:"middle",margin:0});

p.writeFile({fileName:"C:/Users/KJH/AppData/Local/Temp/_spirit_extra_slide.pptx"}).then(f=>console.log("WROTE",f));
