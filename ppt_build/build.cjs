const pptxgen = require("./pptxgen.cjs.js");

// ---------- palette ----------
const INK   = "2E3322"; // dark warm green-brown text
const INK2  = "6B6A50"; // muted
const CREAM = "F7F1E1"; // page background
const PANEL = "FCF8EE"; // card background
const FOREST= "4A6535"; // primary green
const FORESTD="38492A"; // dark green
const MOSS  = "8FAE55"; // bright moss
const SAGE  = "C2D0AE"; // soft sage
const AMBER = "E0A23A"; // accent / coins
const BROWN = "8C6239"; // wood/leather
const SKY   = "8FB6D4"; // weather blue
const RUST  = "C2663B"; // enemy / warning
const WHITE = "FFFFFF";

const KF = "Malgun Gothic";       // Korean-safe body/header
const SF = "Georgia";             // serif accent for english/numbers

const IMG = (n)=>"img/"+n;
const mkShadow = ()=>({ type:"outer", color:"3A3A2A", blur:7, offset:3, angle:90, opacity:0.18 });
const mkSoft   = ()=>({ type:"outer", color:"3A3A2A", blur:9, offset:4, angle:90, opacity:0.22 });

const p = new pptxgen();
p.layout = "LAYOUT_WIDE"; // 13.3 x 7.5
p.author = "김지해";
p.title  = "SpiritStack 프로젝트 발표";

const W = 13.33, H = 7.5, M = 0.62;

// ---------- helpers ----------
function footer(slide, n){
  slide.addText([
    {text:"SpiritStack", options:{bold:true, color:FOREST}},
    {text:"  ·  카드 스태킹 생존 경영 게임", options:{color:INK2}}
  ], {x:M, y:H-0.5, w:7, h:0.3, fontSize:9, fontFace:KF, align:"left", valign:"middle", margin:0});
  slide.addShape(p.shapes.OVAL, {x:W-1.1, y:H-0.46, w:0.22, h:0.22, fill:{color:MOSS}, line:{type:"none"}});
  slide.addText(String(n), {x:W-1.1, y:H-0.47, w:0.22, h:0.24, fontSize:10, bold:true, color:WHITE, fontFace:SF, align:"center", valign:"middle", margin:0});
}

// content slide header: number chip + kicker + title
function header(slide, num, kicker, title){
  slide.addShape(p.shapes.ROUNDED_RECTANGLE, {x:M, y:0.5, w:0.62, h:0.62, rectRadius:0.12, fill:{color:FOREST}, line:{type:"none"}, shadow:mkShadow()});
  slide.addText(String(num).padStart(2,"0"), {x:M, y:0.5, w:0.62, h:0.62, fontSize:20, bold:true, color:WHITE, fontFace:SF, align:"center", valign:"middle", margin:0});
  slide.addText(kicker, {x:M+0.82, y:0.5, w:11, h:0.26, fontSize:11.5, bold:true, color:MOSS, fontFace:KF, charSpacing:2, align:"left", valign:"middle", margin:0});
  slide.addText(title, {x:M+0.8, y:0.72, w:11.6, h:0.5, fontSize:27, bold:true, color:INK, fontFace:KF, align:"left", valign:"middle", margin:0});
}

// soft rounded panel
function panel(slide, x,y,w,h, fill=PANEL, soft=false){
  slide.addShape(p.shapes.ROUNDED_RECTANGLE, {x,y,w,h, rectRadius:0.12, fill:{color:fill}, line:{color:SAGE, width:1}, shadow: soft?mkSoft():mkShadow()});
}

// ============================================================
// SLIDE 1 — TITLE
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:FORESTD};
  s.addImage({path:IMG("Title_background.png"), x:0, y:0, w:W, h:H, sizing:{type:"cover", w:W, h:H}});
  // soft dark panel bottom for legibility
  s.addShape(p.shapes.RECTANGLE, {x:0, y:4.55, w:W, h:H-4.55, fill:{color:FORESTD, transparency:18}, line:{type:"none"}});
  // top tag pill
  s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:M, y:0.55, w:3.0, h:0.5, rectRadius:0.25, fill:{color:WHITE, transparency:12}, line:{type:"none"}});
  s.addText("게임 프로젝트 발표", {x:M, y:0.55, w:3.0, h:0.5, fontSize:13, bold:true, color:FORESTD, fontFace:KF, align:"center", valign:"middle", margin:0});
  // wordmark
  s.addText("SpiritStack", {x:M, y:4.7, w:11, h:1.3, fontSize:66, bold:true, italic:true, color:WHITE, fontFace:SF, align:"left", valign:"middle", margin:0,
    shadow:{type:"outer", color:"1C2415", blur:8, offset:3, angle:90, opacity:0.5}});
  s.addText("카드를 쌓아 마을을 키우는 생존 경영 시뮬레이션", {x:M+0.06, y:5.95, w:11.5, h:0.5, fontSize:21, color:"F3E8CE", fontFace:KF, align:"left", valign:"middle", margin:0});
  s.addText([
    {text:"발표자  ", options:{color:MOSS, bold:true}},
    {text:"김지해", options:{color:WHITE}},
    {text:"      |      Build 0.3.0      |      2026. 06", options:{color:"E7DEC4"}}
  ], {x:M+0.06, y:6.65, w:11.5, h:0.4, fontSize:14, fontFace:KF, align:"left", valign:"middle", margin:0});
})();

// ============================================================
// SLIDE 2 — AGENDA
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  s.addText("CONTENTS", {x:M, y:0.7, w:6, h:0.3, fontSize:12, bold:true, color:MOSS, fontFace:SF, charSpacing:4, margin:0});
  s.addText("발표 순서", {x:M, y:0.98, w:7, h:0.8, fontSize:38, bold:true, color:INK, fontFace:KF, margin:0});

  const items = [
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
  ];
  const perCol = Math.ceil(items.length/2);
  const colW = 5.7, x0=M, x1=6.95, rowH=0.83, y0=1.78, bh=0.72;
  items.forEach((it,i)=>{
    const col = Math.floor(i/perCol);
    const row = i % perCol;
    const x = col===0? x0 : x1;
    const y = y0 + row*rowH;
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:colW, h:bh, rectRadius:0.1, fill:{color:PANEL}, line:{color:SAGE, width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.OVAL, {x:x+0.22, y:y+0.13, w:0.46, h:0.46, fill:{color:col===0?FOREST:BROWN}, line:{type:"none"}});
    s.addText(it[0], {x:x+0.22, y:y+0.13, w:0.46, h:0.46, fontSize:14, bold:true, color:WHITE, fontFace:SF, align:"center", valign:"middle", margin:0});
    s.addText(it[1], {x:x+0.86, y:y+0.09, w:colW-1.0, h:0.36, fontSize:15, bold:true, color:INK, fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(it[2], {x:x+0.86, y:y+0.43, w:colW-1.0, h:0.26, fontSize:10, color:INK2, fontFace:SF, align:"left", valign:"middle", margin:0});
  });
  footer(s,2);
})();

// ============================================================
// SLIDE 3 — REFERENCE GAME : Stacklands
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,1,"REFERENCE GAME","레퍼런스 게임 — Stacklands");

  // left : game intro panel
  panel(s, M, 1.55, 6.35, 5.2);
  s.addText([
    {text:"Stacklands", options:{bold:true, fontSize:22, color:FOREST, breakLine:true}},
    {text:"Sokpop Collective · 2022 · 카드 스태킹 마을 건설 / 생존", options:{fontSize:11.5, color:INK2, italic:true}},
  ], {x:M+0.38, y:1.82, w:5.7, h:0.85, fontFace:KF, align:"left", valign:"top", margin:0, paraSpaceAfter:4});

  s.addText([
    {text:"카드를 쌓아 상호작용", options:{bold:true, breakLine:true}},
    {text:"주민을 자원·건물에 올려 채집·제작·전투를 자동화", options:{color:INK2, breakLine:true, fontSize:12.5, paraSpaceAfter:9}},
    {text:"수집과 발견 중심의 확장", options:{bold:true, breakLine:true}},
    {text:"200여 종 카드 · 13종 카드팩(요리·농사·건설) · 퀘스트", options:{color:INK2, breakLine:true, fontSize:12.5, paraSpaceAfter:9}},
    {text:"문(Moon) 주기 생존 · 로그라이트", options:{bold:true, breakLine:true}},
    {text:"주기마다 식량 소비, 적과 자동 전투로 버티는 생존 루프", options:{color:INK2, fontSize:12.5}},
  ], {x:M+0.38, y:2.78, w:5.7, h:2.55, fontSize:14, fontFace:KF, bullet:{characterCode:"2022", indent:15}, align:"left", valign:"top", margin:0});

  // stat strip
  const stats3 = [["95%","압도적 긍정 평가"],["200+","수집 카드"],["13종","카드팩"]];
  let sx=M+0.38;
  stats3.forEach((st)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:sx, y:5.5, w:1.85, h:1.0, rectRadius:0.1, fill:{color:CREAM}, line:{color:SAGE,width:1}});
    s.addText(st[0], {x:sx, y:5.6, w:1.85, h:0.5, fontSize:23, bold:true, color:AMBER, fontFace:SF, align:"center", valign:"middle", margin:0});
    s.addText(st[1], {x:sx, y:6.12, w:1.85, h:0.32, fontSize:10.5, color:INK2, fontFace:KF, align:"center", valign:"middle", margin:0});
    sx += 1.97;
  });

  // right : fun factors & why-reference
  panel(s, 7.25, 1.55, W-M-7.25, 5.2, FORESTD, true);
  const rx=7.25, rw=W-M-7.25;
  s.addText("이 게임의 재미 · 레퍼런스로 삼은 이유", {x:rx, y:1.78, w:rw, h:0.4, fontSize:14.5, bold:true, color:"F3EBD6", fontFace:KF, align:"center", margin:0});
  const fun = [
    ["villager.png","편안한 몰입감","복잡한 조작 없이 카드를 쌓는 명상적이고 느긋한 플레이"],
    ["cardPack_01.png","수집·조합의 재미","수많은 카드와 숨은 조합을 발견하는 성취감"],
    ["card_house.png","단순한 규칙, 깊은 성장","쉬운 진입 + 마을이 커지는 의미 있는 성장 루프"],
    ["card_campfire.png","따뜻한 핸드드로잉 감성","귀엽고 아늑한 아트가 주는 편안한 분위기"],
  ];
  let fy=2.32;
  fun.forEach((f)=>{
    s.addShape(p.shapes.OVAL, {x:rx+0.32, y:fy+0.05, w:0.78, h:0.78, fill:{color:PANEL}, line:{type:"none"}});
    s.addImage({path:IMG(f[0]), x:rx+0.42, y:fy+0.13, w:0.58, h:0.62, sizing:{type:"contain",w:0.58,h:0.62}});
    s.addText(f[1], {x:rx+1.25, y:fy, w:rw-1.45, h:0.4, fontSize:14.5, bold:true, color:WHITE, fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(f[2], {x:rx+1.25, y:fy+0.4, w:rw-1.5, h:0.45, fontSize:11, color:"D9E0C8", fontFace:KF, align:"left", valign:"top", margin:0});
    fy += 1.0;
  });
  s.addText("→ ‘단순하지만 깊은’ 카드 스태킹 루프와 따뜻한 감성에 매력을 느껴 SpiritStack 의 모티브로 삼았다", {x:rx+0.3, y:6.28, w:rw-0.6, h:0.4, fontSize:11.5, italic:true, bold:true, color:AMBER, fontFace:KF, align:"center", valign:"middle", margin:0});

  footer(s,3);
})();

// ============================================================
// SLIDE 4 — OUR GAME : SpiritStack intro / core loop
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,2,"OUR GAME","SpiritStack — 게임 소개");

  panel(s, M, 1.55, 12.1, 1.5);
  s.addImage({path:IMG("villager.png"), x:M+0.3, y:1.78, w:1.05, h:1.05, sizing:{type:"contain",w:1.05,h:1.05}});
  s.addText([
    {text:"주민 카드를 중심으로 자원을 모으고, 제작하고, 마을을 지켜내는 생존 경영 게임", options:{bold:true, fontSize:16, color:INK, breakLine:true}},
    {text:"카드를 쌓아 작업을 지시하고 · 하루가 지날 때마다 식량으로 주민을 먹이며 · 적의 습격과 변화하는 날씨를 버텨낸다", options:{fontSize:12.5, color:INK2}},
  ], {x:M+1.55, y:1.62, w:10.3, h:1.36, fontFace:KF, valign:"middle", align:"left", margin:0, paraSpaceAfter:6});

  // core loop row
  s.addText("핵심 게임 루프", {x:M, y:3.32, w:6, h:0.4, fontSize:15, bold:true, color:FOREST, fontFace:KF, margin:0});
  const steps = [
    ["villager.png","카드 스택","주민 + 자원/도구 카드를\n겹쳐 작업 시작"],
    ["card_campfire.png","채집 · 제작","레시피에 따라 자원 채집,\n음식 · 도구 생산"],
    ["berry-bush.png","하루 경과","2분 = 하루, 매일 주민이\n식량을 소비"],
    ["enemy_01.png","적 웨이브","포탈에서 적 출현,\n주민과 자동 전투"],
    ["coin.png","경제 순환","카드 판매 → 코인 →\n카드팩 구매로 확장"],
  ];
  const bw=2.16, gap=0.21, y=3.85, bh=2.65;
  let x=M;
  steps.forEach((st,i)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:bw, h:bh, rectRadius:0.12, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.OVAL, {x:x+bw/2-0.7, y:y+0.22, w:1.4, h:1.4, fill:{color:CREAM}, line:{color:SAGE,width:1}});
    s.addImage({path:IMG(st[0]), x:x+bw/2-0.52, y:y+0.4, w:1.04, h:1.04, sizing:{type:"contain",w:1.04,h:1.04}});
    s.addText(st[1], {x:x, y:y+1.66, w:bw, h:0.36, fontSize:14.5, bold:true, color:INK, fontFace:KF, align:"center", valign:"middle", margin:0});
    s.addText(st[2], {x:x+0.1, y:y+2.0, w:bw-0.2, h:0.6, fontSize:10.5, color:INK2, fontFace:KF, align:"center", valign:"top", margin:0});
    if(i<steps.length-1){
      s.addText("→", {x:x+bw-0.04, y:y+0.55, w:gap+0.08, h:0.8, fontSize:20, bold:true, color:MOSS, fontFace:SF, align:"center", valign:"middle", margin:0});
    }
    x += bw+gap;
  });
  s.addText("매일 반복되는 사이클 — 식량·전투·날씨를 관리하며 더 오래 생존하는 것이 목표", {x:M, y:6.62, w:12.1, h:0.34, fontSize:12, italic:true, color:BROWN, fontFace:KF, align:"center", margin:0});
  footer(s,4);
})();

// ============================================================
// SLIDE 5 — SCOPE (3x3 grid)
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,3,"SCOPE","구현 범위");

  const feats = [
    ["villager.png","카드 스택 & 자동 스택","드래그·겹침, 인접 동종 카드 자동 정렬"],
    ["card_campfire.png","레시피 · 제작 시스템","재료 조합 → 음식·도구·건물 생산"],
    ["card_tree.png","채집 & 랜덤 보상","자원 채집, 결과 확률형 드랍"],
    ["berry-bush.png","하루 사이클 & 식량","2분=하루, 식량 공급·아사 처리"],
    ["cardPack_01.png","카드팩 시스템","스타터팩 · 가중치 확률 랜덤팩"],
    ["coin.png","상점 & 코인 경제","구매/판매 거점, 코인 주머니"],
    ["enemy_01.png","적 웨이브 & 전투","포탈 출현, 자동 전투·드랍"],
    ["icon_weather_sunny.png","날씨 시스템","룰렛 기반 4종 날씨 효과"],
    ["card_storage.png","세이브 / 로드","전체 게임 상태 저장·복원"],
  ];
  const cols=3, gx=0.3, gy=0.22, gw=(12.1-(cols-1)*gx)/cols, gh=1.55, x0=M, y0=1.7;
  feats.forEach((f,i)=>{
    const c=i%cols, r=Math.floor(i/cols);
    const x=x0+c*(gw+gx), y=y0+r*(gh+gy);
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:gw, h:gh, rectRadius:0.1, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:x+0.22, y:y+0.32, w:0.95, h:0.95, rectRadius:0.1, fill:{color:CREAM}, line:{color:SAGE,width:1}});
    s.addImage({path:IMG(f[0]), x:x+0.32, y:y+0.42, w:0.75, h:0.75, sizing:{type:"contain",w:0.75,h:0.75}});
    s.addText(f[1], {x:x+1.32, y:y+0.28, w:gw-1.5, h:0.45, fontSize:14.5, bold:true, color:INK, fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(f[2], {x:x+1.32, y:y+0.74, w:gw-1.5, h:0.6, fontSize:11, color:INK2, fontFace:KF, align:"left", valign:"top", margin:0});
  });
  footer(s,5);
})();

// ============================================================
// SLIDE 6 — TIMELINE
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,4,"TIMELINE","개발 일정 요약");
  s.addText("2026. 05. 14  ~  06. 07   ·   약 3.5주 · 101 커밋", {x:M+0.8, y:1.2, w:11, h:0.3, fontSize:12.5, italic:true, color:BROWN, fontFace:KF, margin:0});

  const phases = [
    ["WEEK 1","05.14 ~ 05.19","기반 구축", FOREST, ["프로젝트 셋팅 · 폴더 구조","카드 UI 출력","카메라 줌·이동","PlayerInput 입력 시스템"]],
    ["WEEK 2","05.20 ~ 05.25","코어 게임플레이", MOSS, ["채집 · 레시피 조합","인접 카드 자동 스택","스타터 카드팩","하루 사이클 · 식량 · 게임오버"]],
    ["WEEK 3","05.26 ~ 06.01","시스템 확장", AMBER, ["상점 · 코인 경제","카드팩 확률 시스템","적 시스템 · 포탈 · 전투","날씨 시스템 (4종)"]],
    ["WEEK 4","06.02 ~ 06.07","완성도 · 저장", BROWN, ["타이틀 씬 · 카드 제약/최대수","세이브 / 로드","창고 → 최대 카드 수 증가","이펙트 · 사운드 · UI 보강"]],
  ];
  const pw=2.93, gap=0.13, y=1.75, ph=4.85; let x=M;
  phases.forEach((ph_,i)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:pw, h:ph, rectRadius:0.12, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    // header band
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:pw, h:1.18, rectRadius:0.12, fill:{color:ph_[3]}, line:{type:"none"}});
    s.addShape(p.shapes.RECTANGLE, {x:x, y:y+0.7, w:pw, h:0.48, fill:{color:ph_[3]}, line:{type:"none"}});
    s.addText(ph_[0], {x:x+0.2, y:y+0.16, w:pw-0.4, h:0.3, fontSize:13, bold:true, color:WHITE, fontFace:SF, charSpacing:2, margin:0});
    s.addText(ph_[1], {x:x+0.2, y:y+0.44, w:pw-0.4, h:0.28, fontSize:11, color:"F3EBD6", fontFace:SF, margin:0});
    s.addText(ph_[2], {x:x+0.2, y:y+0.74, w:pw-0.4, h:0.38, fontSize:15, bold:true, color:WHITE, fontFace:KF, valign:"middle", margin:0});
    s.addText(ph_[4].map((t,j)=>({text:t, options:{bullet:{characterCode:"2022", indent:14}, breakLine:true, paraSpaceAfter:9}})),
      {x:x+0.24, y:y+1.42, w:pw-0.46, h:ph-1.6, fontSize:12, color:INK, fontFace:KF, align:"left", valign:"top", margin:0});
    x += pw+gap;
  });
  footer(s,6);
})();

// ============================================================
// SLIDE 7 — DIFFERENTIATOR 1 : WEATHER
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:"EAF1F6"}; // cool tint to signal highlight
  // highlight ribbon kicker
  header(s,5,"DIFFERENTIATOR ①  ·  레퍼런스와 다른 점","날씨 시스템");

  // roulette visual
  panel(s, M, 1.65, 4.0, 5.1, PANEL, true);
  s.addImage({path:IMG("Roulette.png"), x:M+0.65, y:2.0, w:2.7, h:2.7, sizing:{type:"contain",w:2.7,h:2.7}});
  s.addText([
    {text:"7일마다 룰렛 추첨", options:{bold:true, fontSize:15, color:FOREST, breakLine:true}},
    {text:"날씨를 무작위로 결정하고\n3일 동안 효과가 지속된다", options:{fontSize:12.5, color:INK2}},
  ], {x:M+0.2, y:4.95, w:3.6, h:1.6, fontFace:KF, align:"center", valign:"top", margin:0, paraSpaceAfter:4});

  // 2x2 weather effects
  const wx=5.0, ww=(W-M-wx-0.0), cardW=(ww-0.3)/2, cardH=2.4, gap=0.3;
  const weat = [
    ["icon_weather_sunny.png","맑음 (Sunny)","채집 결과 30% 확률로 2배","FBE7B0","B7841F"],
    ["icon_weather_rain.png","비 (Rain)","채집 속도 1.5배 가속","CFE3F2","3E6B8C"],
    ["icon_weather_snow.png","눈 (Snow)","매일 카드 2장 빙결 → 사용 불가","E2ECF4","5E7C92"],
    ["icon_weather_storm.png","폭풍 (Storm)","카드 흔들림, 매일 1장씩 날아감","D8D6E6","5B5577"],
  ];
  weat.forEach((wd,i)=>{
    const c=i%2, r=Math.floor(i/2);
    const x=wx+c*(cardW+gap), y=1.65+r*(cardH+0.3);
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:cardW, h:cardH, rectRadius:0.12, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.OVAL, {x:x+0.28, y:y+0.42, w:1.5, h:1.5, fill:{color:wd[3]}, line:{type:"none"}});
    s.addImage({path:IMG(wd[0]), x:x+0.5, y:y+0.64, w:1.06, h:1.06, sizing:{type:"contain",w:1.06,h:1.06}});
    s.addText(wd[1], {x:x+1.95, y:y+0.42, w:cardW-2.1, h:0.5, fontSize:17, bold:true, color:wd[4], fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(wd[2], {x:x+1.95, y:y+0.98, w:cardW-2.15, h:1.1, fontSize:13, color:INK, fontFace:KF, align:"left", valign:"top", margin:0});
  });
  footer(s,7);
})();

// ============================================================
// SLIDE 8 — DIFFERENTIATOR 2 : VILLAGER ATTRIBUTES
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:"F1EEE6"};
  header(s,6,"DIFFERENTIATOR ②  ·  레퍼런스와 다른 점","주민 속성 시스템");

  // left : attribute villagers
  s.addText("4가지 속성 주민", {x:M, y:1.55, w:7, h:0.4, fontSize:16, bold:true, color:FOREST, fontFace:KF, margin:0});
  const vills = [
    ["villager.png","기본 (Normal)","ECE6D6"],
    ["Villager_Fire.png","불 (Fire)","FCE3D2"],
    ["Villager_Water.png","물 (Water)","D6E8F2"],
    ["Villager_Stone.png","돌 (Stone)","E4E0D6"],
  ];
  const vw=1.78, vgap=0.2; let vx=M;
  vills.forEach((v)=>{
    const y=2.0;
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:vx, y, w:vw, h:2.25, rectRadius:0.12, fill:{color:v[2]}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addImage({path:IMG(v[0]), x:vx+0.34, y:y+0.22, w:1.1, h:1.1, sizing:{type:"contain",w:1.1,h:1.1}});
    s.addText(v[1], {x:vx, y:y+1.45, w:vw, h:0.6, fontSize:13.5, bold:true, color:INK, fontFace:KF, align:"center", valign:"middle", margin:0});
    vx += vw+vgap;
  });
  s.addText("enum VillagerType { Normal, Fire, Water, Stone }  +  속성별 외형·특성", {x:M, y:4.35, w:7.7, h:0.34, fontSize:11.5, italic:true, color:BROWN, fontFace:"Consolas", margin:0});

  // baby growth note
  panel(s, M, 4.85, 7.7, 1.9);
  s.addImage({path:IMG("baby.png"), x:M+0.3, y:5.15, w:1.25, h:1.25, sizing:{type:"contain",w:1.25,h:1.25}});
  s.addText([
    {text:"아기 → 성장 시스템", options:{bold:true, fontSize:15, color:FOREST, breakLine:true}},
    {text:"아기 카드는 채집·제작에 참여하지 못하며, 3일이 지나면 성인 주민(adultData)으로 성장한다.", options:{fontSize:12.5, color:INK2}},
  ], {x:M+1.75, y:5.05, w:5.8, h:1.5, fontFace:KF, valign:"middle", align:"left", margin:0, paraSpaceAfter:5});

  // right : stat panel
  panel(s, 8.45, 1.55, 4.25, 5.2, FORESTD, true);
  s.addText("주민이 가지는 스탯", {x:8.45, y:1.8, w:4.25, h:0.4, fontSize:15, bold:true, color:"F3EBD6", fontFace:KF, align:"center", margin:0});
  const stats = [
    ["체력 (Health)","적의 공격으로 감소"],
    ["배고픔 (Hunger)","매일 증가, 0이면 사망"],
    ["공격력 (Attack)","전투 시 적에게 피해"],
    ["작업 속도 (Work Speed)","레시피 소요시간 단축"],
    ["공격 간격 (Interval)","자동 전투 공격 주기"],
  ];
  let sy=2.35;
  stats.forEach((st)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:8.7, y:sy, w:3.75, h:0.78, rectRadius:0.08, fill:{color:PANEL}, line:{type:"none"}});
    s.addText(st[0], {x:8.9, y:sy+0.08, w:3.4, h:0.34, fontSize:13, bold:true, color:INK, fontFace:KF, valign:"middle", margin:0});
    s.addText(st[1], {x:8.9, y:sy+0.4, w:3.4, h:0.3, fontSize:10.5, color:INK2, fontFace:KF, valign:"middle", margin:0});
    sy += 0.88;
  });
  footer(s,8);
})();

// ============================================================
// SLIDE 9 — DIFFERENTIATOR 3 : WORLD TREE DROP
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:"E9F1E3"}; // soft fairy-green tint
  header(s,7,"DIFFERENTIATOR ③  ·  추가 구현 콘텐츠","세계수 랜덤 드롭 시스템");

  // left : world tree card
  panel(s, M, 1.62, 3.5, 5.05);
  s.addImage({path:IMG("card_worldTree.png"), x:M+0.65, y:2.05, w:2.2, h:2.2, sizing:{type:"contain",w:2.2,h:2.2}});
  s.addText("세계수 카드", {x:M, y:4.45, w:3.5, h:0.4, fontSize:18, bold:true, color:FOREST, fontFace:KF, align:"center", margin:0});
  s.addText("필드에 랜덤으로 생성되어\n5번 채집할 수 있는 나무", {x:M+0.2, y:4.92, w:3.1, h:1.4, fontSize:12.5, color:INK2, fontFace:KF, align:"center", valign:"top", margin:0});

  // right-top : drop flow panel
  const rx=4.45, rw=W-M-rx;
  panel(s, rx, 1.62, rw, 3.05);
  s.addText("세계수 채집  →  매번 랜덤 드롭", {x:rx+0.3, y:1.76, w:rw-0.6, h:0.35, fontSize:14.5, bold:true, color:FOREST, fontFace:KF, align:"left", valign:"middle", margin:0});
  s.addText("랜덤 드롭", {x:4.95, y:2.18, w:3.15, h:0.3, fontSize:11, bold:true, color:INK2, fontFace:KF, align:"center", valign:"middle", margin:0});
  // drop card factory
  const dcard=(x,img,label,hi)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y:2.5, w:1.55, h:1.75, rectRadius:0.1, fill:{color:CREAM}, line:{color:hi?AMBER:SAGE, width:hi?2:1}, shadow:mkShadow()});
    s.addImage({path:IMG(img), x:x+0.3, y:2.62, w:0.95, h:0.95, sizing:{type:"contain",w:0.95,h:0.95}});
    s.addText(label, {x, y:3.62, w:1.55, h:0.5, fontSize:12.5, bold:true, color:INK, fontFace:KF, align:"center", valign:"middle", margin:0});
  };
  dcard(4.95,"wood.png","통나무",false);
  dcard(6.6,"card_fairyHerb.png","요정허브",false);
  s.addText("→", {x:8.2, y:2.85, w:0.85, h:0.9, fontSize:30, bold:true, color:MOSS, fontFace:SF, align:"center", valign:"middle", margin:0});
  dcard(9.05,"card_fairySoup.png","요정 스프",true);
  s.addText("요정허브로 만드는\n음식 카드", {x:10.75, y:2.78, w:1.85, h:1.2, fontSize:11.5, bold:true, color:BROWN, fontFace:KF, align:"left", valign:"middle", margin:0});

  // right-bottom : reason panel
  panel(s, rx, 4.87, rw, 1.8, FORESTD, true);
  s.addText("왜 추가했나?", {x:rx+0.32, y:5.05, w:3, h:0.4, fontSize:14.5, bold:true, color:MOSS, fontFace:KF, align:"left", valign:"middle", margin:0});
  s.addText("음식 카드의 종류가 부족했던 상황에서, 플레이어에게 새로운 음식 공급원이자 하나의 즐길 거리(콘텐츠)를 제공하고자 — 세계수 → 요정허브 → 요정 스프로 이어지는 채집·제작 경로를 추가했다.",
    {x:rx+0.32, y:5.48, w:rw-0.64, h:1.05, fontSize:13, color:"E7DEC4", fontFace:KF, align:"left", valign:"top", margin:0});

  footer(s,9);
})();

// ============================================================
// SLIDE 10 — DIFFERENTIATOR 4 : RECIPE EFFECTS
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:"F6EAD9"}; // warm peach tint
  header(s,8,"DIFFERENTIATOR ④  ·  레퍼런스와 다른 점","레시피별 맞춤 이펙트 연출");

  // left : explanation panel
  panel(s, M, 1.62, 4.5, 5.05, FORESTD, true);
  s.addText("작업마다 다른 ‘손맛’", {x:M, y:1.95, w:4.5, h:0.5, fontSize:18, bold:true, color:"F3EBD6", fontFace:KF, align:"center", margin:0});
  s.addText([
    {text:"카드를 스택해 작업할 때, 진행 중인 작업 종류에 맞는 전용 파티클 이펙트가 카드 위에서 재생된다.", options:{breakLine:true, fontSize:14, color:"E7DEC4", paraSpaceAfter:16}},
    {text:"구현 방식", options:{bold:true, color:MOSS, breakLine:true, fontSize:13}},
    {text:"레시피·채집 데이터마다 effectPrefab 을 지정 → ProgressTask 가 작업 중 해당 이펙트를 생성", options:{fontSize:12.5, color:"E7DEC4", breakLine:true, paraSpaceAfter:16}},
    {text:"왜 차별점인가", options:{bold:true, color:MOSS, breakLine:true, fontSize:13}},
    {text:"단순 진행 바를 넘어 — 무슨 작업인지 한눈에 보이고, 타격감과 게임의 생동감을 더한다", options:{fontSize:12.5, color:"E7DEC4"}},
  ], {x:M+0.35, y:2.55, w:3.8, h:4.0, fontFace:KF, align:"left", valign:"top", margin:0});

  // right : 2x2 effect examples
  const ex = [
    ["card_tree.png","나무 채집","도끼로 패듯 나무 조각이 사방으로 흩뿌려진다","DCE7CE","4A6535"],
    ["card_rock.png","돌 캐기","단단한 돌 조각이 튀어 오른다","E6E1D4","6B6450"],
    ["berry-bush.png","열매 채집","덤불에서 잎과 열매가 흩날린다","D8EBD0","4A6535"],
    ["card_house.png","건설 · 제작","완성을 알리는 제작 연출이 피어난다","EDE3D0","8C6239"],
  ];
  const ax=5.35, aw=W-M-ax, cw=(aw-0.3)/2, ch=2.4, gap=0.3;
  ex.forEach((e,i)=>{
    const c=i%2, r=Math.floor(i/2);
    const x=ax+c*(cw+gap), y=1.62+r*(ch+0.25);
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:cw, h:ch, rectRadius:0.12, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.OVAL, {x:x+0.26, y:y+0.4, w:1.5, h:1.5, fill:{color:e[3]}, line:{type:"none"}});
    s.addImage({path:IMG(e[0]), x:x+0.5, y:y+0.62, w:1.02, h:1.02, sizing:{type:"contain",w:1.02,h:1.02}});
    s.addText(e[1], {x:x+1.95, y:y+0.42, w:cw-2.1, h:0.5, fontSize:17, bold:true, color:e[4], fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(e[2], {x:x+1.95, y:y+0.98, w:cw-2.15, h:1.1, fontSize:13, color:INK, fontFace:KF, align:"left", valign:"top", margin:0});
  });
  footer(s,10);
})();

// ============================================================
// SLIDE 11 — ARCHITECTURE
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,9,"ARCHITECTURE","핵심 구조 · 설계");

  const layers = [
    ["Manager 계층","Singleton",FOREST,["GameManager","DayManager","WeatherManager","RecipeManager","EnemyManager","SoundManager","SettlementManager","InputManager"]],
    ["Data 계층","ScriptableObject",AMBER,["CardData","VillagerCardData","EnemyCardData","CardRecipe","CardPackData","WeatherType"]],
    ["Card · UI · 런타임","MonoBehaviour",MOSS,["Card / VillagerCard","CardStack","ProgressTask","ResourceCardUI","UI_Ingame / RecipeBook"]],
  ];
  let y=1.7; const lh=1.42, lw=8.7;
  layers.forEach((L)=>{
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:M, y, w:lw, h:lh, rectRadius:0.1, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:M, y, w:2.3, h:lh, rectRadius:0.1, fill:{color:L[2]}, line:{type:"none"}});
    s.addShape(p.shapes.RECTANGLE, {x:M+1.8, y, w:0.5, h:lh, fill:{color:L[2]}, line:{type:"none"}});
    s.addText(L[0], {x:M+0.18, y:y+0.32, w:2.0, h:0.45, fontSize:15.5, bold:true, color:WHITE, fontFace:KF, margin:0});
    s.addText(L[1], {x:M+0.18, y:y+0.78, w:2.0, h:0.32, fontSize:10.5, italic:true, color:"F3EBD6", fontFace:SF, margin:0});
    // chips
    const chips=L[3]; const perRow=Math.ceil(chips.length/2);
    const areaX=M+2.5, areaW=lw-2.7;
    const cwid=(areaW-(perRow-1)*0.15)/perRow;
    chips.forEach((c,i)=>{
      const r=Math.floor(i/perRow), col=i%perRow;
      const cx=areaX+col*(cwid+0.15), cy=y+0.2+r*0.56;
      s.addShape(p.shapes.ROUNDED_RECTANGLE, {x:cx, y:cy, w:cwid, h:0.46, rectRadius:0.08, fill:{color:CREAM}, line:{color:L[2],width:1}});
      s.addText(c, {x:cx, y:cy, w:cwid, h:0.46, fontSize:10, color:INK, fontFace:"Consolas", align:"center", valign:"middle", margin:0});
    });
    y += lh+0.2;
  });

  // right note panel
  panel(s, M+lw+0.25, 1.7, W-M-(M+lw+0.25), 4.46, FORESTD, true);
  const nx=M+lw+0.25;
  s.addText("설계 포인트", {x:nx, y:1.95, w:W-M-nx, h:0.4, fontSize:15, bold:true, color:"F3EBD6", fontFace:KF, align:"center", margin:0});
  s.addText([
    {text:"이벤트 기반 결합", options:{bold:true, color:MOSS, breakLine:true}},
    {text:"OnDayChanged · OnWeatherDetermined 으로 매니저 간 느슨한 연결", options:{fontSize:11.5, color:"E7DEC4", breakLine:true, paraSpaceAfter:10}},
    {text:"코루틴 비동기 흐름", options:{bold:true, color:MOSS, breakLine:true}},
    {text:"웨이브 · 룰렛 · 세이브 복원을 코루틴으로 단계 실행", options:{fontSize:11.5, color:"E7DEC4", breakLine:true, paraSpaceAfter:10}},
    {text:"데이터 주도 콘텐츠", options:{bold:true, color:MOSS, breakLine:true}},
    {text:"카드·레시피·팩을 에셋으로 정의해 코드와 분리", options:{fontSize:11.5, color:"E7DEC4"}},
  ], {x:nx+0.3, y:2.45, w:W-M-nx-0.55, h:3.6, fontSize:13, fontFace:KF, align:"left", valign:"top", margin:0});

  footer(s,11);
})();

// ============================================================
// SLIDE 12 — KEY TECH (2)
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,10,"KEY TECHNOLOGY","핵심 구현 기술 2가지");

  const cardW=5.95, cardH=4.95, y=1.7;
  // tech 1
  let x=M;
  panel(s, x, y, cardW, cardH, PANEL, true);
  s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:cardW, h:1.15, rectRadius:0.12, fill:{color:FOREST}, line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE, {x:x, y:y+0.6, w:cardW, h:0.55, fill:{color:FOREST}, line:{type:"none"}});
  s.addText("01", {x:x+0.3, y:y+0.2, w:0.9, h:0.75, fontSize:30, bold:true, color:"9DBE6E", fontFace:SF, valign:"middle", margin:0});
  s.addText([
    {text:"ScriptableObject 데이터 기반 설계", options:{bold:true, fontSize:17, color:WHITE, breakLine:true}},
    {text:"Data-Driven Design", options:{fontSize:11, italic:true, color:"E7DEC4"}},
  ], {x:x+1.25, y:y+0.18, w:cardW-1.4, h:0.85, fontFace:KF, valign:"middle", margin:0, paraSpaceAfter:2});
  s.addText([
    {text:"게임 내용을 ‘데이터 파일’로 분리", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  카드·레시피·날씨 같은 요소를 코드가 아닌 데이터로 관리한다.", options:{breakLine:true, color:INK2, fontSize:13, paraSpaceAfter:13}},
    {text:"수정이 빠르다", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  코드를 건드리지 않고 데이터만 바꿔 수치·확률을 바로 조정 → 밸런싱이 쉽다.", options:{breakLine:true, color:INK2, fontSize:13, paraSpaceAfter:13}},
    {text:"추가·재사용이 쉽다", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  새 카드를 넣어도 기존 코드를 거의 고칠 필요가 없다.", options:{color:INK2, fontSize:13}},
  ], {x:x+0.4, y:y+1.5, w:cardW-0.8, h:3.2, fontSize:14.5, fontFace:KF, align:"left", valign:"top", margin:0, color:INK});

  // tech 2
  x=M+cardW+0.3;
  panel(s, x, y, cardW, cardH, PANEL, true);
  s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:cardW, h:1.15, rectRadius:0.12, fill:{color:BROWN}, line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE, {x:x, y:y+0.6, w:cardW, h:0.55, fill:{color:BROWN}, line:{type:"none"}});
  s.addText("02", {x:x+0.3, y:y+0.2, w:0.9, h:0.75, fontSize:30, bold:true, color:"E0BE93", fontFace:SF, valign:"middle", margin:0});
  s.addText([
    {text:"카드 고유 스택 관리 시스템", options:{bold:true, fontSize:17, color:WHITE, breakLine:true}},
    {text:"Card Stacking System", options:{fontSize:11, italic:true, color:"F3EBD6"}},
  ], {x:x+1.25, y:y+0.18, w:cardW-1.4, h:0.85, fontFace:KF, valign:"middle", margin:0, paraSpaceAfter:2});
  s.addText([
    {text:"카드 더미를 한 묶음으로 관리", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  쌓인 카드들을 자동으로 가지런히 정렬해 준다.", options:{breakLine:true, color:INK2, fontSize:13, paraSpaceAfter:13}},
    {text:"집고 놓으면 알아서 정리", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  카드를 내려놓으면 가까운 더미에 저절로 합쳐지고, 같은 카드는 자동으로 쌓인다.", options:{breakLine:true, color:INK2, fontSize:13, paraSpaceAfter:13}},
    {text:"스택에 규칙이 있다", options:{bullet:{characterCode:"2713", indent:18}, bold:true}},
    {text:"  주민은 항상 맨 위에 놓이고, 적·얼린 카드는 합쳐지지 않는다.", options:{color:INK2, fontSize:13}},
  ], {x:x+0.4, y:y+1.5, w:cardW-0.8, h:3.2, fontSize:14.5, fontFace:KF, align:"left", valign:"top", margin:0, color:INK});

  footer(s,12);
})();

// ============================================================
// SLIDE 13 — CHANGES DURING DEVELOPMENT
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:CREAM};
  header(s,11,"REFACTORING","진행 중 변경된 구조 · 구현");

  const changes = [
    ["입력 시스템 전환","기존 InputManager → Unity PlayerInput System 으로 교체해 입력 처리를 표준화"],
    ["전체 코드 구조 개편","스타터 카드팩 구현 중 카드/데이터 구조를 재정비 (커밋: ‘코드 구조개편’)"],
    ["채집물 구조 변경","적 드랍 아이템 도입과 함께 채집 결과물 처리 구조를 재설계"],
    ["채집 결과 랜덤화","고정 보상 → 확률 기반 랜덤 보상 시스템으로 변경"],
    ["카드 최대 개수 제약","카드 최대 수 도입 + 창고(Storage) 카드 보유 시 상한 증가 규칙 추가"],
    ["렌더링·저장 보강","URP 도입으로 이펙트 강화, 후반 세이브 시스템 추가에 맞춰 매니저 지속 보강"],
  ];
  const cols=2, gx=0.3, gy=0.25, gw=(12.1-gx)/cols, gh=1.5, x0=M, y0=1.75;
  changes.forEach((c,i)=>{
    const col=i%cols, r=Math.floor(i/cols);
    const x=x0+col*(gw+gx), y=y0+r*(gh+gy);
    s.addShape(p.shapes.ROUNDED_RECTANGLE, {x, y, w:gw, h:gh, rectRadius:0.1, fill:{color:PANEL}, line:{color:SAGE,width:1}, shadow:mkShadow()});
    s.addShape(p.shapes.OVAL, {x:x+0.28, y:y+0.5, w:0.5, h:0.5, fill:{color:RUST}, line:{type:"none"}});
    s.addText("↻", {x:x+0.28, y:y+0.48, w:0.5, h:0.5, fontSize:18, bold:true, color:WHITE, fontFace:SF, align:"center", valign:"middle", margin:0});
    s.addText(c[0], {x:x+0.95, y:y+0.22, w:gw-1.1, h:0.42, fontSize:15.5, bold:true, color:INK, fontFace:KF, align:"left", valign:"middle", margin:0});
    s.addText(c[1], {x:x+0.95, y:y+0.66, w:gw-1.15, h:0.7, fontSize:12, color:INK2, fontFace:KF, align:"left", valign:"top", margin:0});
  });
  footer(s,13);
})();

// ============================================================
// SLIDE 14 — CLOSING
// ============================================================
(()=>{
  const s = p.addSlide();
  s.background = {color:FORESTD};
  s.addImage({path:IMG("Title_background.png"), x:0, y:0, w:W, h:H, sizing:{type:"cover", w:W, h:H}});
  s.addShape(p.shapes.RECTANGLE, {x:0, y:0, w:W, h:H, fill:{color:FORESTD, transparency:35}, line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE, {x:0, y:2.55, w:W, h:2.45, fill:{color:FORESTD, transparency:20}, line:{type:"none"}});
  s.addText("감사합니다", {x:0, y:2.7, w:W, h:1.2, fontSize:56, bold:true, color:WHITE, fontFace:KF, align:"center", valign:"middle", margin:0,
    shadow:{type:"outer", color:"1C2415", blur:8, offset:3, angle:90, opacity:0.5}});
  s.addText([
    {text:"SpiritStack", options:{bold:true, italic:true, color:"F3EBD6", fontFace:SF}},
    {text:"   ·   카드 스태킹 생존 경영 게임   ·   Build 0.3.0", options:{color:"E7DEC4"}}
  ], {x:0, y:4.0, w:W, h:0.5, fontSize:18, fontFace:KF, align:"center", valign:"middle", margin:0});
  s.addText("발표자  김지해", {x:0, y:4.55, w:W, h:0.4, fontSize:14, color:MOSS, bold:true, fontFace:KF, align:"center", valign:"middle", margin:0});
})();

p.writeFile({fileName:"SpiritStack_발표자료_v2.pptx"}).then(f=>console.log("WROTE", f));
