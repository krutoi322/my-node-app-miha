const FileManagerHybrid = require('./fileOperationsHybrid');

const fm = new FileManagerHybrid('./data-hybrid');

async function main() {
    console.log('гибридный подход\n');

    console.log('стиль промисов');
    await fm.createFile('a.txt', 'привет из промиса');
    const text = await fm.readFile('a.txt');
    console.log('a.txt:', text);

    console.log('\n стиль колбэков');
    fm.createFile('b.txt', 'привет из колбэка', (err, filePath) => {
        if (err) return console.error('ошибка', err.message);
        console.log('  Создан:', filePath);

        fm.listFiles((err, files) => {
            if (err) return console.error('ошибка', err.message);
            console.log('Файлы:', files);

            fm.readFile('нет такого.txt', (err) => {
                console.log('\n обработка ошибок');
                if (err) console.log('ошибка найдена', err.code);

                (async () => {
                    for (const f of files) await fm.deleteFile(f);
                    console.log('\n файлы удалены');
                })();
            });
        });
    });
}

main().catch(console.error);