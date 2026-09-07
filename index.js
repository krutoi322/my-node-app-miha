const http = require('http');

function PI() {
    const s = 10n ** 25n;
    function atan(x) {
        x = BigInt(x);
        let sum = 0n;
        let term = s / x;
        let n = 1n;
        let sign = 1n;

        while (term > 0n) {
            sum += sign * term / n;
            term /= x * x;
            n += 2n;
            sign = -sign;
        }
        return sum;
    }
    const p = 16n * atan(5) - 4n * atan(239);
    return `${p / s}.${(p % s).toString().padStart(25, '0').slice(0, 17)}`;
}
const server = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
    res.end(`<h1>Семенюк Михаил</h1><p>Группа: 478</p><p>Число π: ${PI()}</p>`);
});
const PORT = 3000;
server.listen(PORT, () => {
    console.log(`Сервер запущен на http://localhost:${PORT}`);
});