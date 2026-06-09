import { execSync } from 'child_process';
import { existsSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));
const frontendDir = resolve(__dirname, '..');

const browserCandidates = [
  'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
  'C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe',
  `${process.env.LOCALAPPDATA || ''}\\Google\\Chrome\\Application\\chrome.exe`,
  'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe',
  'C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe',
];

if (!process.env.CHROME_BIN) {
  const found = browserCandidates.find((p) => existsSync(p));
  if (found) {
    process.env.CHROME_BIN = found;
  }
}

const ngArgs = process.argv.slice(2).join(' ');
const cmd = `ng test ${ngArgs}`;

try {
  execSync(cmd, { cwd: frontendDir, stdio: 'inherit', shell: true });
} catch {
  process.exit(1);
}
