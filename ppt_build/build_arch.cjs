const pptxgen = require("./pptxgen.cjs.js");

const INK="2E3322", INK2="6B6A50", CREAM="F7F1E1", PANEL="FCF8EE",
      FOREST="4A6535", FORESTD="38492A", MOSS="8FAE55", SAGE="C2D0AE",
      AMBER="E0A23A", BROWN="8C6239", WHITE="FFFFFF";
const KF="Malgun Gothic", SF="Georgia";
const mkShadow=()=>({type:"outer",color:"3A3A2A",blur:7,offset:3,angle:90,opacity:0.18});
const mkSoft=()=>({type:"outer",color:"3A3A2A",blur:9,offset:4,angle:90,opacity:0.22});

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
function panel(slide,x,y,w,h,fill=PANEL,soft=false){
  slide.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y,w,h,rectRadius:0.12,fill:{color:fill},line:{color:SAGE,width:1},shadow:soft?mkSoft():mkShadow()});
}

const s=p.addSlide();
s.background={color:CREAM};
header(s,9,"ARCHITECTURE","핵심 구조 · 설계");

// ---- left: 3 layers (identical to current deck) ----
const layers=[
  ["Manager 계층","Singleton",FOREST,["GameManager","DayManager","WeatherManager","RecipeManager","EnemyManager","SoundManager","SettlementManager","InputManager"]],
  ["Data 계층","ScriptableObject",AMBER,["CardData","VillagerCardData","EnemyCardData","CardRecipe","CardPackData","WeatherType"]],
  ["Card · UI · 런타임","MonoBehaviour",MOSS,["Card / VillagerCard","CardStack","ProgressTask","ResourceCardUI","UI_Ingame / RecipeBook"]],
];
let y=1.7; const lh=1.42, lw=8.7;
layers.forEach((L)=>{
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:M,y,w:lw,h:lh,rectRadius:0.1,fill:{color:PANEL},line:{color:SAGE,width:1},shadow:mkShadow()});
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:M,y,w:2.3,h:lh,rectRadius:0.1,fill:{color:L[2]},line:{type:"none"}});
  s.addShape(p.shapes.RECTANGLE,{x:M+1.8,y,w:0.5,h:lh,fill:{color:L[2]},line:{type:"none"}});
  s.addText(L[0],{x:M+0.18,y:y+0.32,w:2.0,h:0.45,fontSize:15.5,bold:true,color:WHITE,fontFace:KF,margin:0});
  s.addText(L[1],{x:M+0.18,y:y+0.78,w:2.0,h:0.32,fontSize:10.5,italic:true,color:"F3EBD6",fontFace:SF,margin:0});
  const chips=L[3]; const perRow=Math.ceil(chips.length/2);
  const areaX=M+2.5, areaW=lw-2.7;
  const cwid=(areaW-(perRow-1)*0.15)/perRow;
  chips.forEach((c,i)=>{
    const r=Math.floor(i/perRow), col=i%perRow;
    const cx=areaX+col*(cwid+0.15), cy=y+0.2+r*0.56;
    s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:cx,y:cy,w:cwid,h:0.46,rectRadius:0.08,fill:{color:CREAM},line:{color:L[2],width:1}});
    s.addText(c,{x:cx,y:cy,w:cwid,h:0.46,fontSize:10,color:INK,fontFace:"Consolas",align:"center",valign:"middle",margin:0});
  });
  y += lh+0.2;
});

// ---- right green panel ----
const px=M+lw+0.25, py=1.7, pw=W-M-(M+lw+0.25), ph=4.46, cx=px+pw/2;
panel(s, px, py, pw, ph, FORESTD, true);

// title
s.addText("카드 데이터 구조",{x:px,y:py+0.16,w:pw,h:0.3,fontSize:13.5,bold:true,color:"F3EBD6",fontFace:KF,align:"center",margin:0});

// CardData base box
const cdW=1.85, cdH=0.5, cdX=cx-cdW/2, cdY=py+0.62;
s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:cdX,y:cdY,w:cdW,h:cdH,rectRadius:0.1,fill:{color:AMBER},line:{type:"none"},shadow:mkShadow()});
s.addText("CardData",{x:cdX,y:cdY,w:cdW,h:cdH,fontSize:13,bold:true,color:"3A2E12",fontFace:"Consolas",align:"center",valign:"middle",margin:0});
s.addText("base · ScriptableObject",{x:px,y:cdY+cdH-0.02,w:pw,h:0.2,fontSize:8.5,italic:true,color:"C9D4B3",fontFace:SF,align:"center",margin:0});

// 상속 arrow label
s.addText("↓  상속",{x:px,y:cdY+cdH+0.16,w:pw,h:0.22,fontSize:10.5,bold:true,color:MOSS,fontFace:KF,align:"center",valign:"middle",margin:0});

// derived chips (2 cols x 3 rows)
const chipW=1.36, gapx=0.12, lX=px+0.18, rX=px+0.18+1.36+0.12;
const rowsY=[cdY+cdH+0.46, cdY+cdH+1.0, cdY+cdH+1.54];
const derived=[["Resource","Source"],["Coin","Heart"],["Food","Building"]];
derived.forEach((pair,r)=>{
  pair.forEach((name,c)=>{
    const x=c===0?lX:rX, yy=rowsY[r];
    s.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y:yy,w:chipW,h:0.44,rectRadius:0.08,fill:{color:CREAM},line:{color:MOSS,width:1}});
    s.addText(name,{x,y:yy,w:chipW,h:0.44,fontSize:10.5,color:INK,fontFace:"Consolas",align:"center",valign:"middle",margin:0});
  });
});

// UI 묶음 arrow label
const afterChips=rowsY[2]+0.44;
s.addText("↓  카드 UI로 묶임",{x:px,y:afterChips+0.04,w:pw,h:0.22,fontSize:10.5,bold:true,color:MOSS,fontFace:KF,align:"center",valign:"middle",margin:0});

// ResourceCardUI box
const uiY=afterChips+0.32;
s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:px+0.28,y:uiY,w:pw-0.56,h:0.46,rectRadius:0.1,fill:{color:MOSS},line:{type:"none"},shadow:mkShadow()});
s.addText("ResourceCardUI",{x:px+0.28,y:uiY,w:pw-0.56,h:0.46,fontSize:11,bold:true,color:"24320F",fontFace:"Consolas",align:"center",valign:"middle",margin:0});

// caption
s.addText("Coin·Heart·Resource·Source 를 함께 표시\n(Food·Building 등은 각자 전용 UI)",
  {x:px+0.18,y:uiY+0.5,w:pw-0.36,h:0.5,fontSize:8.5,color:"C9D4B3",fontFace:KF,align:"center",valign:"top",margin:0});

footer(s,11);

p.writeFile({fileName:"C:/Users/KJH/AppData/Local/Temp/_spirit_arch.pptx"}).then(f=>console.log("WROTE",f));
