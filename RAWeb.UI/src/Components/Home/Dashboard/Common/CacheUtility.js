const CacheSymbol = Symbol("cache");

const InstanceSymbol = Symbol("instance");

export default class CacheUtility {

    constructor() {
        this[CacheSymbol] = new Map();
    }

    static get Instance() {
        if(this[InstanceSymbol] === undefined) {
            this[InstanceSymbol] = new CacheUtility();
        }
        return this[InstanceSymbol];
    }

    has(key) {
        return this[CacheSymbol].has(key);
    }

    get(key) {
        if(!this.has(key)) {
            return undefined;
        }

        return this[CacheSymbol].get(key);
    }

    set(key, value) {
        this[CacheSymbol].set(key, value);
    }

    remove(key) {
        this[CacheSymbol].delete(key);
    }

    clear() {
        this[CacheSymbol].clear();
    }
}