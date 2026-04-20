const esbuild = require('./node_modules/esbuild');
const fs = require('fs');
const path = 'src/app/pages/NurseVerification.jsx';
const src = fs.readFileSync(path, 'utf8');
try {
    esbuild.transformSync(src, { loader: 'jsx', sourcefile: path });
    console.log('parsed ok');
} catch (e) {
    console.error(e.message);
    if (e.errors) console.error(JSON.stringify(e.errors, null, 2));
    process.exit(1);
}
