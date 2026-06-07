const fs=require('fs');
(async()=>{
  const url='https://cdn.jsdelivr.net/npm/jszip@3.10.1/dist/jszip.min.js';
  const r=await fetch(url);
  if(!r.ok){console.error('HTTP',r.status);process.exit(1);}
  const buf=Buffer.from(await r.arrayBuffer());
  fs.writeFileSync('node_modules/jszip/jszip.min.js',buf);
  fs.writeFileSync('node_modules/jszip/package.json',JSON.stringify({name:'jszip',version:'3.10.1',main:'jszip.min.js'},null,2));
  console.log('jszip bytes:',buf.length);
})();
