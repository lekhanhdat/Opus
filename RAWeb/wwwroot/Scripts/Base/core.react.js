/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
var RM = window.RM;
if (!RM) {
    RM = window.RM = {};
}
RM.Constant = {
    passwordPlaceholder: "_★◇●☆◆○_"
};

RM.Ajax = {
    ErrorType: {
        Logout: "RecordManager.Logout",
        NoPermissionAccess: "RecordManager.NoPermissionAccess"
    },
    sendRequest: function(url, callback, data, isDefaultType, isSync, errorCallback) {
        var options = {
            url: url,
            success: function(result) {
                callback(result);
            },
            error: function(e) {
                var error = JSON.parse(e.responseText);
                switch (error.MessageType) {
                    case RM.Ajax.ErrorType.Logout:
                        errorCallback = function() {
                            location.href = '/Passport/ForceLogout';
                        };
                        break;
                    case RM.Ajax.ErrorType.NoPermissionAccess:
                        errorCallback = function() {
                            location.href = '/Passport';
                        };
                        break;
                }
                if (errorCallback) {
                    errorCallback();
                }
                //$.alert(error.Message, null, errorCallback, error.Solution);
                RM.hideLoadCover();
            }
        };
        if (data) options.data = data;
        if (isDefaultType) {
            if (isDefaultType == true) {
                options.contentType = 'application/x-www-form-urlencoded';
            } else if (isDefaultType.toLowerCase() == 'get') {
                options.type = 'get';
                options.contentType = 'application/json';
            } else {
                options.type = isDefaultType;
            }
        }
        if (isSync) options.async = false;
        this.ajaxCustom(options);
    },
    ajaxCustom: function(option) {
        var ajaxParam = {
            type: 'POST',
            contentType: 'application/json;charset=utf-8',
            data: null,
            dataType: 'json',
            success: null,
            error: null
            //url: null,
        };
        $.extend(ajaxParam, option);
        if (option.data && ajaxParam.contentType == 'application/json;charset=utf-8' && (ajaxParam.type.toLowerCase() == 'post' || ajaxParam.type.toLowerCase() == 'delete')) {
            ajaxParam.data = JSON.stringify(option.data);
        }
        $.ajax(ajaxParam);
    }
};

RM.TimeUtil = (function() {
    let globalAuiFormat = "YYYY-MMMM-DD HH:mm:ss";
    let globalTimezoneInfo = null;
    let localTimezoneOffset = new Date().getTimezoneOffset() * 60 * 1000;
    let globalTimezoneOffset = 0;

    return {
        init: function() {
            globalAuiFormat = getAuiFormat(RM.TimeSettingModel.DateFormat, RM.TimeSettingModel.TimeFormat);
            globalTimezoneInfo = this.getTimezoneInfo(RM.TimeSettingModel.TimeZoneId, RM.TimeSettingModel.isSetDayLight);
            globalTimezoneOffset = getTimezoneOffset(globalTimezoneInfo);
        },
        toISOString: function (date) {
            if (date) {
                //'2022-07-28T01:38:38'
                return $$.date.format(date, "yyyy-MM-ddTHH:mm:ss");
            } else {
                return date;
            }
        },
        getDateStr: function(date) {
            return date.getFullYear() + '/' + (date.getMonth() + 1) + '/' + date.getDate();
        },
        getTodayStartEndTime(){
            let startTime = new Date(new Date(this.getDateStr(new Date())).getTime());
            let endTime = new Date(new Date(this.getDateStr(new Date())).getTime()+24*60*60*1000-1);
            return {start: startTime, end: endTime};
        },
        getCommonDateStr: function(date) {
            if (date) {
                return date.getFullYear() + '/' + (date.getMonth() + 1) + '/' + date.getDate() + ' ' + date.getHours() + ':' + date.getMinutes();
            } else {
                return date;
            }
        },
        getTimezoneInfo: function(Id, isDayLightSaving) {
            for (var i = 0; i < RM.TimeSettingModel.TimeZoneInfo.length; i++) {
                var timezone = RM.TimeSettingModel.TimeZoneInfo[i];
                if (timezone.id == Id) {
                    timezone = Object.assign({}, RM.TimeSettingModel.TimeZoneInfo[i]);
                    if (timezone.supportsDaylightSavingTime) {
                        timezone.autoAdjustClock = !!isDayLightSaving;
                    }
                    return timezone;
                }
            }
            return this.getGlobalTimezoneInfo();
        },
        getGlobalAuiFormat: function() {
            return globalAuiFormat;
        },
        getGlobalTimezoneInfo: function() {
            return globalTimezoneInfo;
        },
        dateToString: function(date, timezone, isSimplifyTimeZone) {
            if (date) {
                let timezoneInfo = (!timezone ? globalTimezoneInfo : timezone);
                if (isSimplifyTimeZone) {
                    return $$.date.format(date, globalAuiFormat) + " " + timezoneInfo.simplifyDisplayName;
                }
                return $$.date.format(date, globalAuiFormat) + " " + timezoneInfo.displayName;
            } else {
                return "";
            }
        },
        dateToStringSimplifyTimeZone: function(date, timezone) {
            return this.dateToString(date, timezone, true);
        },
        ticksToDate: function(utcTicks, timezone) {
            let targetTimezoneOffset = !timezone ? globalTimezoneOffset : getTimezoneOffset(timezone);
            if (!timezone) {
                timezone = globalTimezoneInfo;
            }
            var tick1970 = 621355968000000000;
            var tickOneHour = 60 * 60 * 1000;
            var timeStamp = (utcTicks - tick1970) / 10000; //local
            timeStamp = timeStamp + localTimezoneOffset + targetTimezoneOffset;
            if(timezone && timezone.supportsDaylightSavingTime && !timezone.autoAdjustClock){
                timeStamp = timeStamp - tickOneHour;
            }
            return new Date(timeStamp);
        },
        ticksToString: function (utcTicks, timezone, isSimplifyTimeZone) {
            if (!utcTicks) { return ""; }
            if (!timezone) {
                timezone = globalTimezoneInfo;
            }
            let date = this.ticksToDate(utcTicks, timezone);
            if (date) {
                if (isSimplifyTimeZone) {
                    return $$.date.format(date, RM.TimeUtil.getGlobalAuiFormat()) + " " + timezone.simplifyDisplayName;
                }
                return $$.date.format(date, RM.TimeUtil.getGlobalAuiFormat()) + " " + timezone.displayName;
            } else {
                return "";
            }
        },
        ticksToStringSimplifyTimeZone: function(utcTicks, timezone) {
            return this.ticksToString(utcTicks, timezone, true);
        }
    };

    function getAuiFormat(dateFormat, timeFormat) {
        dateFormat = dateFormat || "";
        timeFormat = timeFormat || "";
        // if (dateFormat) {
        //     switch (dateFormat) {
        //         case "yyyy-MM-dd":
        //             dateFormat = "YYYY-MMMM-DD";
        //             break;
        //         case "M-d-yyyy":
        //             dateFormat = "MMM-D-YYYY";
        //             break;
        //         case "M-d-yy":
        //             dateFormat = "MMM-D-YY";
        //             break;
        //         case "MM-dd-yy":
        //             dateFormat = "MMMM-DD-YY";
        //             break;
        //         case "d-MMMM-yy":
        //             dateFormat = "D-MM-YY";
        //             break;
        //         case "MMMM d,yyyy":
        //             dateFormat = "MM D,YYYY";
        //             break;
        //         case "d-MMM-yyyy":
        //             dateFormat = "D-M-YYYY";
        //             break;
        //         case "dd-MM-yyyy":
        //             dateFormat = "DD-MMMM-YYYY";
        //             break;
        //     }
        // } else {
        //     dateFormat = "";
        // }
        // if (timeFormat) {
        //     switch (timeFormat) {
        //         case "HH:mm:ss":
        //             timeFormat = "HH:mm:ss";
        //             break;
        //         case "h:mm:ss tt":
        //             timeFormat = "h:mm:ss TT";
        //             break;
        //     }
        // } else {
        //     timeFormat = "";
        // }
        return dateFormat + " " + timeFormat;
    }

    function getTimezoneOffset(timezone) {
        return timezone.offsetHours * 60 * 60 * 1000 + timezone.offsetMinutes * 60 * 1000;
    }
})();


RM.Cookie = {
    get: function(name) {
        var content = document.cookie;
        if (content.length > 0) {
            var begin = content.indexOf(name + "=");
            if (begin != -1) {
                begin += name.length + 1;
                var end = content.indexOf(";", begin);
                if (end == -1) {
                    end = content.length;
                }
                return content.substring(begin, end);
            }
        }
        return '';
    },
    set: function(name, value, expires, path) {
        if (!name || name.length == 0 || !value) {
            return false;
        }
        var content = name + "=" + value;
        if (expires) {
            var d = new Date();
            d.setTime(d.getTime() + (expires * 24 * 60 * 60 * 1000));
            content += "; expires=" + d.toGMTString();
        }
        if (path) {
            content += "; path=" + path;
        }
        document.cookie = content;
        return true;
    },
    remove: function(name, path) {
        return this.set(name, "removed", -1, path);
    },
    debugMode: function(isDebug) {
        if (isDebug) {
            this.set("RM_IsDebug", "true", 365, "/");
        } else {
            this.set("RM_IsDebug", "false", 365, "/");
        }
    },
    UserInfo: {
        _uiObj: null,
        _init: function() {
            var uiStr = RM.Cookie.get("RM_UserInfo");
            if (uiStr.length > 0) {
                this._uiObj = JSON.parse(decodeURI(uiStr));
            } else {
                this._uiObj = {
                    loginName: "",
                    fontSize: "20px"
                };
            }
        },
        _save: function() {
            if (this._uiObj) {
                RM.Cookie.set("RM_UserInfo", encodeURI(JSON.stringify(this._uiObj)), 365, "/");
            }
        },
        getLoginName: function() {
            if (!this._uiObj) {
                this._init();
            }
            return this._uiObj.loginName;
        },
        getFontSize: function() {
            if (!this._uiObj) {
                this._init();
            }
            return this._uiObj.fontSize;
        },
        setLoginName: function(name) {
            if (!this._uiObj) {
                this._init();
            }
            this._uiObj.loginName = name;
            this._save();
        },
        setFontSize: function(size) {
            if (!this._uiObj) {
                this._init();
            }
            this._uiObj.fontSize = size;
            this._save();
        },
    },
};

RM.Url = {
    getParam: function(url, name) {
        var idx = url.indexOf('?');
        if (idx != -1) {
            var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i');
            var r = url.substring(idx + 1).match(reg);
            if (r != null) {
                return unescape(r[2]);
            }
        }
        return "";
    }
};

RM.Encoding = {
    htmlEncode: function(str) {
        if (!str) {
            return null;
        } else {
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(str));
            return div.innerHTML;
        }
    },
    htmlDecode: function(str) {
        if (!str) {
            return null;
        } else {
            var div = document.createElement('div');
            div.innerHTML = str;
            return div.innerText || div.textContent;
        }
    }
};

RM.XmlToJson = function(xml) {
    // Create the return object
    var obj = {};
    if (xml.nodeType == 1) { // element
        // do attributes
        if (xml.attributes.length > 0) {
            obj["@attributes"] = {};
            for (var j = 0; j < xml.attributes.length; j++) {
                var attribute = xml.attributes.item(j);
                obj["@attributes"][attribute.nodeName] = attribute.nodeValue;
            }
        }
    } else if (xml.nodeType == 3) { // text
        obj = xml.nodeValue;
    }

    // do children
    if (xml.hasChildNodes()) {
        for (var i = 0; i < xml.childNodes.length; i++) {
            var item = xml.childNodes.item(i);
            var nodeName = item.nodeName;
            if (typeof(obj[nodeName]) == "undefined") {
                obj[nodeName] = RM.XmlToJson(item);
            } else {
                if (typeof(obj[nodeName].push) == "undefined") {
                    var old = obj[nodeName];
                    obj[nodeName] = [];
                    obj[nodeName].push(old);
                }
                obj[nodeName].push(RM.XmlToJson(item));
            }
        }
    }
    return obj;
};

RM.IsIE = function() {
    if (!!window.ActiveXObject || "ActiveXObject" in window)
        return true;
    else
        return false;
};

RM.CommStatus = {
    get: function() {
        return RM.Cookie.get(this.Status);
    },
    save: function(status) {
        switch (status) {
            case this.CreateSuccess:
                RM.Cookie.set(this.Status, this.CreateSuccess, 1, "/");
                break;
            case this.EditSuccess:
                RM.Cookie.set(this.Status, this.EditSuccess, 1, "/");
                break;
            default:
                RM.Cookie.set(this.Status, status, 1, "/");
        }
    },
    remove: function() {
        RM.Cookie.remove(this.Status, "/");
    },
    CreateSuccess: "createSuccess",
    EditSuccess: "editSuccess",
    Status: "commSts",
};

RM.SwitchLanguage = {
    setFontFamily: function() {
        if (RM.gData.currentLanguage == 'ja' || RM.gData.currentLanguage == 'ja-JP') {
            $("body").css('font-family', 'Meiryo UI');
        }
    }
};

RM.SimplifyObject = function(obj, objProps, delProps) {
    return cloneObject(obj, objProps, delProps);

    function cloneObject(obj, objProps, delProps) {
        var isArr = Array.isArray(obj);
        var newData = isArr ? [] : {};
        for (var key in obj) {
            if (delProps && delProps.indexOf(key) > -1) {
                continue;
            }
            var val = obj[key];
            if (isArr || objProps && val && objProps.indexOf(key) > -1) {
                newData[key] = cloneObject(val, objProps, delProps);
            } else if (val && val != '00000000-0000-0000-0000-000000000000') {
                newData[key] = val;
            }
        }
        return newData;
    }
};

RM.CopyToClipboard = function(content) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(content);
    } else {
        var textarea = document.createElement('textarea');
        document.body.appendChild(textarea);
        textarea.style.position = 'fixed';
        textarea.style.clip = 'rect(0 0 0 0)';
        textarea.style.top = '10px';
        textarea.value = content;
        textarea.select();
        document.execCommand('copy', true);
        document.body.removeChild(textarea);
    }
}

RM.getSessionStorage = function(key) {
    let value = sessionStorage.getItem(key);
    return value ? JSON.parse(value) : value;
}

RM.setSessionStorage = function(key, value) {
    sessionStorage.setItem(key, value ? JSON.stringify(value) : value);
}

RM.deepcopy = function (e) {
	if (null == e || Number.isNaN(e)) return e;
	try {
		return structuredClone(e);
	} catch (t) {
		return JSON.parse(JSON.stringify(e));
	}
};