import _ from "lodash"

class NumberUtil {

    static internaltionalCounting = (number) => {
        if(_.isNil(number)) {
            return 0;
        }
        if(_.isNumber(number)) {
            if(number === 0) return "0";
            number = _.toString(number);
        }
        //add new condition to handle the round number with decimal part, for example: 1000.00, 1000000.00
        let decimalPart = "";
        if (number.includes(".")) {
            decimalPart = number.slice(number.indexOf('.'), number.length);
            decimalPart = decimalPart.slice(0, 3);
            number = number.slice(0, number.indexOf('.'))
        }
        let numArr = number.split('');
        numArr = _.reverse(numArr);
        let textArr = [];
        for(let i = 0; i < numArr.length; i++) {
            textArr.push(numArr[i]);
            if((i + 1) % 3 === 0) {
                textArr.push(",");
            }
        }
        textArr = _.reverse(textArr);
        let res = textArr.join("");
        res = _.trimStart(res, ",");
        res = _.trimEnd(res, ",");
        return res + decimalPart;
    }

    static toPercentage = (number) => {
        if(Number.isNaN(number) || !Number.isFinite(number)) {
            return "0%";
        }
        return ((number * 100).toFixed(0) + "").replace(".", "") + "%";
    };

    static toGreaterThanZero = (number) => {
        return number > 0 ? number : 0;
    }

    static internationalCountingSF = (number) => {
        if(_.isNil(number)) {
            return 0;
        }
        if(_.isNumber(number)) {
            if(number === 0) return "0";
            number = _.toString(number);
        }
        let decimalPart = "";
        const numString = String(number);
        if (numString.includes(".")) {
            decimalPart = numString.slice(numString.indexOf('.'), numString.length);
            number = numString.slice(0, numString.indexOf('.'))
        }
        let numArr = number.split('');
        numArr = _.reverse(numArr);
        let textArr = [];
        for(let i = 0; i < numArr.length; i++) {
            textArr.push(numArr[i]);
            if((i + 1) % 3 === 0) {
                textArr.push(",");
            }
        }
        textArr = _.reverse(textArr);
        let res = textArr.join("");
        res = _.trimStart(res, ",");
        res = _.trimEnd(res, ",");
        if (!!decimalPart){
            res = res + decimalPart;
        }
        return res;
    }
};

export default NumberUtil;