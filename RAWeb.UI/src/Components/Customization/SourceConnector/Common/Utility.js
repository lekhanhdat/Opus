const SymbolUnicode = new Map([
    ["~", "%7E"],
    ["`", "%60"],
    ["!", "%21"],
    ["@", "%40"],
    ["#", "%23"],
    ["$", "%24"],
    ["%", "%25"],
    ["^", "%5E"],
    ["&", "%26"],
    ["*", "%2A"],
    ["(", "%28"],
    [")", "%29"],
    ["-", "%2D"],
    ["_", "%5F"],
    ["=", "%3D"],
    ["+", "%2B"],
    ["[", "%5B"],
    ["{", "%7B"],
    ["}", "%7D"],
    ["]", "%5D"],
    ["\\", "%5C"],
    ["|", "%7C"],
    [";", "%3B"],
    [":", "%3A"],
    ["\"", "%22"],
    ["'", "%27"],
    [",", "%2C"],
    ["<", "%3C"],
    [".", "%2E"],
    [">", "%3E"],
    ["/", "%2F"],
    ["?", "%3F"],
    [" ", "%20"]
]);

const UnicodeEncode = (str) => {
    let res = "";
    for (let i = 0; i < str.length; i++) {
        let partRes = "_";
        const char = str.charAt(i);
        if(SymbolUnicode.has(char)) {
            partRes += SymbolUnicode.get(char);
        } 
        else {
            const escapeChar = escape(char);
            if (escapeChar === char) {
                let j = i;
                for (j; j < str.length; j++) {
                    const enChar = str.charAt(j);
                    const enEscapeChar = escape(enChar);
                    if (enChar === enEscapeChar) {
                        partRes += enEscapeChar;
                        continue;
                    }
                    break;
                }
                i = j - 1;
            }
            else {
                partRes += escapeChar.replaceAll("%u", "x");
            }
        }        
        res += partRes;    
    }
    return res;
};

export default {
    UnicodeEncode
};