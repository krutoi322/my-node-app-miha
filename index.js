const http = require('http');
const EventEmitter = require('events');
const fs = require('fs');
const { setupLogger } = require('./logger');
class AppServer extends EventEmitter {
    start(port) {
        this.server = http.createServer((req, res) => {
            setTimeout(() => {
                this.emit('request:received', req.url, req.method);
            }, 2000);


            res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
            res.write('<h1>Семенюк Михаил</h1>');
            res.write('478<br>');
            res.end(`Число PI: ${PI()}`);
        });

        this.server.listen(port, () => {
            this.emit('server:started', port);
        });
    }
    stop() {
        setTimeout(() => {
            this.server.close(() => {
                this.emit('server:closed');
            });
        }, 20000);
    }
}

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
    const server = new AppServer();
    setupLogger(server);
    server.once('server:started', (port) => {
        console.log(`Сервер запущен на порту: ${port}`);
    });
    server.once('server:closed', () => {
        console.log("сервер остановлен");
        process.exit(0); 
    });
    server.on('request:received', (url, method) => {
        console.log(`${url} ${method}`);
        console.log("Hello from Event-Driven Server");
    });
    server.start(8090)
    server.stop()
