const path = require('path');
const fs = require('fs');
const filepath = path.join(__dirname, 'logs.txt');
const timenow = new Date().toLocaleTimeString();
const stream = fs.createWriteStream(filepath, { flags: 'a', encoding: 'utf8' });
function setupLogger(server) {
    server.once('ServerWithPortCreated', (port) => {
        stream.write(`[${timenow}] ServerWithPortCreated: localhost: ${port}\n`)
    });

    server.once('serverclosed', () => {
        console.log("сервер отключен\n")
        stream.write(`[${timenow}] serverclosed: Сервер отключен\n`)
        process.exit(0);
    });

    server.on('z', (url, method) => {
        stream.write(`[${timenow}] z : ${url} ${method}\n`)
    });
}
module.exports = { setupLogger };