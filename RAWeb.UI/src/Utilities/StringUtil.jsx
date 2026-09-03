//string publice method
export default {
    stringFormat: function () {
        if (arguments.length == 0)
            return null;
        let str = arguments[0];
        for (let i = 1; i < arguments.length; i++) {
            let re = new RegExp("\\{" + (i - 1) + "\\}", "gm");
            str = str.replace(re, arguments[i]);
        }
        return str;
    },
    newGuid: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    },
    unique: function (items, attribute) {
        const res = new Map();
        return items.filter((item) => !res.has(item[attribute]) && res.set(item[attribute], 1));
    },
    trimEndColon: function (string) {
        return string.replace(/:$/gi, "");
    },
    trimEndFullStop: function (string) {
        return string.replace(/.$/gi, "");
    },
    toI18N: function(str) {
        return RMResx[str] || str;
    }
};