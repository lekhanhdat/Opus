/*Covered by AvePoint copyright and license agreement*/
//jQuery extends
{
    var _parseJson = $.parseJSON;
    $.parseJSON = function (json) {
        if (typeof (json) == 'string') {
            return _parseJson(json);
        }
        return json;
    }
}

//String prototype extends
{
    String.prototype.endsWith = function (suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
    String.prototype.format = function () {
        var e = arguments
            , n = $.isArray(e[0]);
        return this.replace(/\{(\d+)\}/g, function (t, o) {
            return n ? e[0][o] : e[o]
        })
    }
}
//Date prototype extends
{
    Date.prototype.add = function (milliseconds) {
        var m = this.getTime() + milliseconds;
        return new Date(m);
    };
    Date.prototype.addSeconds = function (second) {
        return this.add(second * 1000);
    };
    Date.prototype.addMinutes = function (minute) {
        return this.addSeconds(minute * 60);
    };
    Date.prototype.addHours = function (hour) {
        return this.addMinutes(60 * hour);
    };
    Date.prototype.addDays = function (day) {
        return this.addHours(day * 24);
    };
    Date.isLeepYear = function (year) {
        return (year % 4 == 0 && year % 100 != 0)
    };
    Date.daysInMonth = function (year, month) {
        if (month == 2) {
            if (year % 4 == 0 && year % 100 != 0)
                return 29;
            else
                return 28;
        }
        else if ((month <= 7 && month % 2 == 1) || (month > 7 && month % 2 == 0))
            return 31;
        else
            return 30;
    };
    Date.prototype.addMonth = function () {
        var m = this.getMonth();
        if (m == 11) return new Date(this.getFullYear() + 1, 1, this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());

        var daysInNextMonth = Date.daysInMonth(this.getFullYear(), this.getMonth() + 1);
        var day = this.getDate();
        if (day > daysInNextMonth) {
            day = daysInNextMonth;
        }
        return new Date(this.getFullYear(), this.getMonth() + 1, day, this.getHours(), this.getMinutes(), this.getSeconds());
    };
    Date.prototype.subMonth = function () {
        var m = this.getMonth();
        if (m == 0) return new Date(this.getFullYear() - 1, 12, this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());
        var day = this.getDate();
        var daysInPreviousMonth = Date.daysInMonth(this.getFullYear(), this.getMonth());
        if (day > daysInPreviousMonth) {
            day = daysInPreviousMonth;
        }
        return new Date(this.getFullYear(), this.getMonth() - 1, day, this.getHours(), this.getMinutes(), this.getSeconds());
    };
    Date.prototype.addMonths = function (addMonth) {
        var result = false;
        if (addMonth > 0) {
            while (addMonth > 0) {
                result = this.addMonth();
                addMonth--;
            }
        } else if (addMonth < 0) {
            while (addMonth < 0) {
                result = this.subMonth();
                addMonth++;
            }
        } else {
            result = this;
        }
        return result;
    };
    Date.prototype.addYears = function (year) {
        return new Date(this.getFullYear() + year, this.getMonth(), this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());
    };
}