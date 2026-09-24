const fs = require('fs');
const path = require('path');

class FileManagerHybrid {
    constructor(baseDir = './data-hybrid') {
        this.baseDir = baseDir;
        if (!fs.existsSync(baseDir)) {
            fs.mkdirSync(baseDir, { recursive: true });
        }
    }

    _path(filename) {
        return path.join(this.baseDir, filename);
    }

    _result(promise, callback) {
        if (callback) {
            promise.then(data => callback(null, data), err => callback(err));
        } else {
            return promise;
        }
    }

    createFile(filename, content, callback) {
        const p = fs.promises .writeFile(this._path(filename), content, 'utf8') .then(() => this._path(filename));
        return this._result(p, callback);
    }

    readFile(filename, callback) {
        const p = fs.promises.readFile(this._path(filename), 'utf8');
        return this._result(p, callback);
    }

    listFiles(callback) {
        const p = (async () => {
            const files = await fs.promises.readdir(this.baseDir);
            const stats = await Promise.all(files.map(async name => {const s = await fs.promises.stat(this._path(name));
                    return { name, isFile: s.isFile() };
                })
            );
            return stats.filter(x => x.isFile).map(x => x.name);
        })();

        return this._result(p, callback);
    }
    deleteFile(filename, callback) {
        const p = fs.promises.unlink(this._path(filename));
        return this._result(p, callback);
    }
}

module.exports = FileManagerHybrid;