import MD5 from "./Md5";

export function HashFnv32a(str, seed = 0x811c9dc5) {
    var i, l,
        hval = (seed === undefined) ? 0x811c9dc5 : seed;

    for (i = 0, l = str.length; i < l; i++) {
        hval ^= str.charCodeAt(i);
        hval += (hval << 1) + (hval << 4) + (hval << 7) + (hval << 8) + (hval << 24);
    }

    return hval >>> 0;
}

export function HashMd5(str) {
    return MD5(str);
}