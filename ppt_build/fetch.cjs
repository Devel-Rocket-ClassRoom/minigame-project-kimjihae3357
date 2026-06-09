const fs=require('fs');
const url='https://cdn.jsdelivr.net/npm/pptxgenjs@3.12.0/dist/pptxgen.cjs.js';
(async()=>{
  const r=await fetch(url);
  if(!r.ok){console.error('HTTP',r.status);process.exit(1);}
  const buf=Buffer.from(await r.arrayBuffer());
  fs.writeFileSync('pptxgen.cjs.js',buf);
  console.log('downloaded bytes:',buf.length);
})();
