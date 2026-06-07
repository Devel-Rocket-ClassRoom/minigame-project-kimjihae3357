const fs=require('fs');
function pngSize(b){ if(b.readUInt32BE(0)!==0x89504e47) return null; return [b.readUInt32BE(16), b.readUInt32BE(20)]; }
for(const f of fs.readdirSync('img').sort()){
  const b=fs.readFileSync('img/'+f);
  const s=pngSize(b);
  console.log(f.padEnd(26), s?`${s[0]}x${s[1]}  (r=${(s[0]/s[1]).toFixed(2)})`:'?');
}
