/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
var RM = {};
RM.Constant = {
    passwordPlaceholder: "_★◇●☆◆○_"
};
RM.createBreadCrumb = function(data) {
    $$page("rm.breadcrumb", {
        _viewModel: {
            breadcrumbs: JSON.parse(data),
            keys: ["text", "href"]
        },
        _create: function() {
            $("#rmMapLink").breadcrumb(this._viewModel);
            $activeMapLink = $("#rmMapLink .aui-breadcrumb-active>div");
            $activeMapLink.css("outline", "");
        }
    });
};
RM.Session = {
    timeout: 60 * 1000,
    timer: null,
    checkSessionResult: {
        Success: 1,
        SessionTimeout: 2,
        ForcedLogout: 3
    },
    init: function () {
        $("#rmSessionTimeOut").messagebox({
            width: 600,
            contentMaxheight: 120,
            type: "w",
            buttons: {
                "OK": function(e, args) {
                    $(this).messagebox("hide");
                    //clearTimeout(RM.Session.timer);
                    RM.Session.refreshPage();
                }
            },
            parameters: {
                "OK": {}
            },
            theme: {
                "OK": ""
            }
        });
        $(".aui-messagebox .aui-messagebox-title").attr("tabindex", "0");
        RM.Session.bindEvent();
        //check is session timeout while page loaded
        this.checkSession();
    },
    bindEvent: function() {
        $("#rmSessionTimeOut").parents(".aui-messagebox").find(".aui-icon-searchbox-close").on("click", function() {
            $("#rmSessionTimeOut").messagebox("hide");
            //clearTimeout(RM.Session.timer);
            RM.Session.refreshPage();
        });
    },
    setSessionTimer: function(t) {
        if (t) {
            RM.Session.timeout = t;
        }
        RM.Session.timer = setTimeout(RM.Session.checkSession, RM.Session.timeout);
    },
    resetSessionTimer: function() {
        clearTimeout(RM.Session.timer);
        RM.Session.setSessionTimer(RM.Session.timeout);
    },
    checkSession: function () {
        $.ajax({
            url: "/Account/CheckSession",
            async: true,
            cache: false,
            success: function (data) {
                if (data == RM.Session.checkSessionResult.Success) {
                    RM.Session.resetSessionTimer();
                }
                else {
                    var msg = data == RM.Session.checkSessionResult.ForcedLogout ? RMResx.RM_JS_Login_ForcedLogout_Warn : RMResx.RM_JS_Login_SessionTimeOut_Warn;
                    $("#rmSesionTImeOutWarn").text(msg);
                    $('#rmSessionTimeOut').messagebox("show");
                }
            },
            error: function(data) {
                console.log("Check session failed.");
                //$('#rmSessionTimeOut').messagebox("show");
            }
        });
    },
    refreshPage: function() {
        window.location.href = "/Account/Logon?redirectUrl=" + encodeURIComponent(window.location.href);
    }
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
                RM.hideAllLoadCover();
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


if (typeof Object.assign != 'function') {
    Object.assign = function(target) {
        'use strict';
        if (target == null) {
            throw new TypeError('Cannot convert undefined or null to object');
        }

        target = Object(target);
        for (var index = 1; index < arguments.length; index++) {
            var source = arguments[index];
            if (source != null) {
                for (var key in source) {
                    if (Object.prototype.hasOwnProperty.call(source, key)) {
                        target[key] = source[key];
                    }
                }
            }
        }
        return target;
    };
}

RM.TimeUtil = {
    globalAuiFormat: "YYYY-MMMM-DD HH:mm:ss",
    globalTimezoneInfo: null,
    localTimezoneOffset: new Date().getTimezoneOffset() * 60 * 1000,
    globalTimezoneOffset: 0,
    init: function() {
        this.globalTimezoneInfo = this.getTimezoneInfo(RM.TimeSettingModel.TimeZoneId, RM.TimeSettingModel.isSetDayLight);
        this.globalTimezoneOffset = this.getTimezoneOffset(this.globalTimezoneInfo);
        this.globalAuiFormat = this.getAuiFormat(RM.TimeSettingModel.DateFormat, RM.TimeSettingModel.TimeFormat);
    },
    convertTime: function(ticks) {
        var d = this.convertTiksToTime(ticks);
        var h = d.getHours();
        var t = 'AM';
        if (h == 0) {
            h = 12;
        } else if (h == 12) {
            t = 'PM';
        } else if (h > 12) {
            t = 'PM';
            h -= 12;
        }
        var m = d.getMinutes();
        m = m < 10 ? '0' + m : m;
        var month = d.getMonth() + 1;
        var dateStr = month + '/' + d.getDate() + '/' + d.getFullYear() + ' ' + h + ':' + m + ' ' + t;
        return dateStr;
    },
    convertTiksToTime: function(ticks) {
        var tick1970 = 621355968000000000;
        var timeStemp = (ticks - tick1970) / 10000;
        return new Date(timeStemp);
    },
    getCommonDateStr: function(date) {
        if (date) {
            return date.getFullYear() + '/' + (date.getMonth() + 1) + '/' + date.getDate() + ' ' + date.getHours() + ':' + date.getMinutes();
        } else {
            return date;
        }

    },
    getDateByFormat: function(date, fmt) {
        if (!fmt) return date;
        var o = {
            "M+": date.getMonth() + 1,
            "d+": date.getDate(),
            "h+": date.getHours() % 12 == 0 ? 12 : date.getHours() % 12,
            "H+": date.getHours(),
            "m+": date.getMinutes(),
            "s+": date.getSeconds(),
            "q+": Math.floor((date.getMonth() + 3) / 3),
            "S": date.getMilliseconds()
        };
        if (/(y+)/.test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.length));
        }

        for (var k in o) {
            if (k != "M+") {
                if (new RegExp("(" + k + ")").test(fmt)) {
                    fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
                }
            }
        }

        if (/(M+)/.test(fmt)) {
            if (RegExp.$1.length >= 3) {
                if (RegExp.$1.length == 4) {
                    fmt = fmt.replace(RegExp.$1, ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'][date.getMonth()]);
                } else {
                    fmt = fmt.replace(RegExp.$1, ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'][date.getMonth()]);
                }
            } else {
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o["M+"]) : (("00" + o["M+"]).substr(("" + o["M+"]).length)));
            }
        }

        return fmt;
    },
    formatDateTime: function(time, f) {
        time = new Date(time);
        var format = f || dateTimeFormat,
            data = {
                D: time.getDate(),
                d: time.getDay(),
                M: time.getMonth(),
                Y: time.getFullYear(),
                h: time.getHours(),
                m: time.getMinutes(),
                s: time.getSeconds()
            };

        if (/(H+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.h > 9 ? "" : RegExp.$1.length === 1 ? "" : "0") + data.h);
        }

        if (/(h+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.h > 12 ? -12 : data.h > 9 ? 0 : RegExp.$1.length === 1 ? 0 : "0") + data.h + "");
        }

        if (/(m+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.m > 9 ? "" : RegExp.$1.length === 1 ? "" : "0") + data.m);
        }

        if (/(s+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.s > 9 ? "" : RegExp.$1.length === 1 ? "" : "0") + data.s);
        }

        if (/(YY+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.Y + "").substr(4 - RegExp.$1.length));
        }

        if (/(D+)/.test(format)) {
            format = format.replace(RegExp.$1, (data.D > 9 ? "" : RegExp.$1.length === 1 ? "" : "0") + data.D);
        }

        if (/(MMM+)/.test(format)) {
            //number month for 1-12
            format = format.replace(RegExp.$1, (RegExp.$1.length === 3 || data.M > 8 ? "" : "0") + (data.M + 1));
        }

        if (/(M+)/.test(format)) {
            //string month for 0-11
            format = format.replace(RegExp.$1, (RegExp.$1.length === 1 ? ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'][data.M] : ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'][data.M]));
        }

        if (/(dddd+)/.test(format)) {
            //string
            //format = format.replace(RegExp.$1, i18n["days"][data.d]);
        }

        if (/(TT+)/.test(format)) {
            //string
            format = format.replace(RegExp.$1, data.h > 12 ? "PM" : "AM");
        }

        return format;
    },

    getAuiFormat: function(dateFormat, timeFormat) {
        if (dateFormat) {
            switch (dateFormat) {
                case "yyyy-MM-dd":
                    dateFormat = "YYYY-MMMM-DD";
                    break;
                case "M-d-yyyy":
                    dateFormat = "MMM-D-YYYY";
                    break;
                case "M-d-yy":
                    dateFormat = "MMM-D-YY";
                    break;
                case "MM-dd-yy":
                    dateFormat = "MMMM-DD-YY";
                    break;
                case "d-MMMM-yy":
                    dateFormat = "D-MM-YY";
                    break;
                case "MMMM d,yyyy":
                    dateFormat = "MM D,YYYY";
                    break;
                case "d-MMM-yyyy":
                    dateFormat = "D-M-YYYY";
                    break;
                case "dd-MM-yyyy":
                    dateFormat = "DD-MMMM-YYYY";
                    break;
            }
        } else {
            dateFormat = "";
        }
        if (timeFormat) {
            switch (timeFormat) {
                case "HH:mm:ss":
                    timeFormat = "HH:mm:ss";
                    break;
                case "h:mm:ss tt":
                    timeFormat = "h:mm:ss TT";
                    break;
            }
        } else {
            timeFormat = "";
        }

        return dateFormat + " " + timeFormat;
    },
    getTimezoneOffset: function(timezone) {
        return timezone.offsetHours * 60 * 60 * 1000 + timezone.offsetMinutes * 60 * 1000;
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
    getGlobalAuiFormat: function () {
        return this.globalAuiFormat;
    },
    getGlobalTimezoneInfo: function () {
        return this.globalTimezoneInfo;
    },
    ticksToDate: function(utcTicks, timezone) {
        let targetTimezoneOffset = !timezone ? this.globalTimezoneOffset : this.getTimezoneOffset(timezone);
        if (!timezone) {
            timezone = this.globalTimezoneInfo;
        }
        var tick1970 = 621355968000000000;
        var timeStamp = (utcTicks - tick1970) / 10000; //local
        timeStamp = timeStamp + this.localTimezoneOffset + targetTimezoneOffset;
        return new Date(timeStamp);
    },
    dateToString: function(date, timezone) {
        if (date) {
            let timezoneName = (!timezone ? this.globalTimezoneInfo : timezone).displayName;
            return this.formatDateTime(date, this.globalAuiFormat) + " " + timezoneName;
        } else {
            return "";
        }
    },
    ticksToString: function(utcTicks, timezone) {
        if (!timezone) {
            timezone = this.globalTimezoneInfo;
        }
        let date = this.ticksToDate(utcTicks, timezone);
        if (date) {
            return this.formatDateTime(date, this.globalAuiFormat) + " " + timezone.displayName;
        } else {
            return "";
        }
    }
};

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

RM.showLoadCover = function() {
    $$page.rm_loadcover("show");
};
RM.hideLoadCover = function() {
    $$page.rm_loadcover("hide");
};
RM.hideAllLoadCover = function() {
    $$page.rm_loadcover("hideAll");
};

RM.showBodyCover = function() {
    $("#rmLoadCover").show();
};

RM.hideBodyCover = function() {
    $("#rmLoadCover").hide();
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

RM.RAMessageType = {
    Success: 0,
    Failed: 1
};

RM.RAFailedType = {
    None: 0,
    NameExisting: 1,
    RunningJobExist: 2,
    NoIndexDevice: 3,
    NoDBSetting: 4,
    NoIndexDeviceAndDBSetting: 5,
    NoLocation: 6,
    LicenseExpired: 7,
    ScheduleServiceFailed: 8,
    DefaultTermIsOrphaned: 9,
    DisableRecordsManagement: 10,
    BreakFolderNode: 11,
    PhysicalMoveHasHoldConflict: 12,
    DeleteUsingSuite: 13,
    DeleteUningTemplate: 14,
    SoftDeleted: 15,
    UpdateFailed: 16,
    HasRunningWorkflowInstance: 17,
    UniqueIdSettingIsEmpty: 18,
    NotAvailableAgent: 19
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
//$$page instance
$$page("rm.loadcover", {
    _shownTimes: 0,
    $panel: null,
    _create: function() {
        this._initMembers()
            ._initWidget();
    },
    _initMembers: function() {
        this.$panel = $("body");
        return this;
    },
    _initWidget: function() {
        if (this.$panel.loading) {
            this.$panel.loading();
            $(".aui-loading-content").css("display", "none");
        }
    },
    show: function() {
        if (this.$panel.loading) {
            this.$panel.loading("show");
            this._shownTimes++;
        }
    },
    hide: function() {
        if (this.$panel.loading) {
            this._shownTimes--;
            if (this._shownTimes == 0) {
                this.$panel.loading("hide");
            } else if (this._shownTimes < 0) {
                this._shownTimes = 0;
            }
        }
    },
    hideAll: function() {
        //for hold 
        if (this.$panel.loading) {
            this._shownTimes = 0;
            this.$panel.loading("hide");
        }
    }
});
$$page("rm.Nav.Event", {
    isPhysicalEnduser: false,
    _create: function() {
        //this._getNavs();
    },
    _initMembers: function() {

    },
    _getNavs: function() {
        var self = this;
        RM.Ajax.sendRequest('/api/HomeApi/GetModules', function(dataObj) {

            self._init(dataObj);
            self._bindEvent();
        });
    },
    _init: function(data) {
        var self = this;
        var navs = self.renderNav(data);
        $(".rm-nav-lv1").empty();
        $(".rm-nav-lv1").append(navs);

        var monitors = self.renderJobMonitorAndControlPanel(data);
        $(".rm-nav-lv1").append(monitors);

    },
    renderNav: function(navData) {
        var self = this;
        if (!self.isPhysicalEnduser) {
            return navData.map(function(item, index) {
                return $("<li class='rm-nav-lv1Tab' key=" + index + "tabIndex='0'>" +
                    "<div class='rm-nav-lv1TabName'>" + item.title + "</div>" +
                    "<ul class='rm-nav-lv2'>" +
                    self.renderSubNav(item.links) +
                    "</ul>" +
                    "</li>");

                // return $(`<li class='rm-nav-lv1Tab' key=${index} tabIndex='0'>
                //     <div class='rm-nav-lv1TabName'>${item.title} </div>
                //     <ul class='rm-nav-lv2'>
                //         ${self.renderSubNav(item.links)}
                //     </ul>
                //     </li>`);
            });
        } else {
            if (navData && navData.length > 0) {
                return navData[0].links.map(function(sublink, index) {
                    return $("<li class='rm-nav-lv1Tab' key=" + index + "tabIndex='0'>" +
                        "<div class='rm-nav-lv1TabName'>" +
                        "<a href=" + sublink.href + " target=" + sublink.target + ">" +
                        sublink.text +
                        "</a>" +
                        "</div>" +
                        "</li>"
                    );
                    // return $(
                    //     `<li class='rm-nav-lv1Tab' key=${index} tabIndex='0'>
                    //         <div class='rm-nav-lv1TabName'>
                    //             <a href=${sublink.href} target=${sublink.target}>
                    //             ${sublink.text}
                    //             </a>
                    //         </div>
                    //     </li>`
                    // );
                });
            }
        }
    },
    renderSubNav: function(subNavs) {
        let subNav = "";
        subNavs.map(function(subLink, index) {
            subNav += "<li class='rm-nav-lv2Tab' key=" + index + ">" +
                "<a href=" + subLink.href + " target=" + subLink.target + ">" + subLink.text + "</a>" +
                "</li>";
            // `<li class='rm-nav-lv2Tab' key=${index}>
            //     <a href=${subLink.href} target=${subLink.target}>${subLink.text}</a>
            // </li>`;

        });
        return subNav;
    },

    /**
     * 是否显示JobMonitor和Control菜单
     * @param {*} isAdmin 是否是管理员
     */
    renderJobMonitorAndControlPanel: function(isAdmin) {
        if (isAdmin) {
            let cp = '';
            if (checkPermission("CP", RM.UserResources)) {
                cp = "<div class='rm-nav-cp' tabIndex='0'>" +
                    "<span class='rmRibbonRow-right-groupImg rm-nav-cpIcon'></span>" +
                    "<span class='rm-nva-cplable'>" + RMResx.RM_CP_ControlPanel + "</span>" +
                    "</div>";
                // `<div class='rm-nav-cp' tabIndex='0'>
                //     <span class='rmRibbonRow-right-groupImg rm-nav-cpIcon'></span>
                //     <span class='rm-nva-cplable'>${RMResx.RM_CP_ControlPanel}</span>
                // </div>`;
            }
            return $("<li class='rm-nav-jmcp'>" +
                "<div class='rm-nav-jm' tabIndex='0'>" +
                "<span class='rmRibbonRow-right-groupImg rm-nav-jmIcon'></span>" +
                "<span class='rm-nva-jmlable'>" + RMResx.RM_JS_JM_Title + "</span>" +
                "</div>" +
                cp +
                "</li>"
            );
            //return `<li class='rm-nav-jmcp'>
            //     <div class='rm-nav-jm' tabIndex='0'>
            //         <span class='rmRibbonRow-right-groupImg rm-nav-jmIcon'></span>
            //         <span class='rm-nva-jmlable'>${RMResx.RM_JS_JM_Title}</span>
            //     </div>
            //     ${cp}
            // </li>`;
        }
    },
    _bindEvent: function() {
        var menuDelay = null;
        var delayTime = 150;
        $("body").on({
                mouseenter: function(e) {
                    var self = $(this);
                    if (menuDelay != null) {
                        clearTimeout(menuDelay);
                    }
                    $(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                    self.addClass("rmRibbonRow-right-item-hover");
                    $("#rmNav_icon").addClass("rmNav_icon_hover");
                },
                mouseleave: function(e) {
                    var self = $(this);
                    menuDelay = setTimeout(function() {
                        var $focusEl = $(document.activeElement);
                        if ($focusEl.attr("Id") == "rmNav_group" || $focusEl.closest("#rmNav_group").length == 1) {} else {
                            self.removeClass("rmRibbonRow-right-item-hover");
                            $("#rmNav_icon").removeClass("rmNav_icon_hover");
                            $("#rmNav_downIcon").removeClass("rmNav_downIcon_hover");
                            $(".rm-nav-lv1").hide();
                            $(".rm-nav-lv2").hide();
                        }
                    }, 400);
                },
                blur: function(e) {
                    var self = $(this);
                    menuDelay = setTimeout(function() {
                        var $focusEl = $(document.activeElement);
                        if ($focusEl.attr("Id") == "rmNav_group" || $focusEl.closest("#rmNav_group").length == 1) {

                        } else {
                            self.removeClass("rmRibbonRow-right-item-hover");
                            $("#rmNav_icon").removeClass("rmNav_icon_hover");
                            $("#rmNav_downIcon").removeClass("rmNav_downIcon_hover");
                            $(".rm-nav-lv1").hide();
                            $(".rm-nav-lv2").hide();
                        }
                    }, 400);
                },
                click: function(e) {
                    if (menuDelay != null) {
                        clearTimeout(menuDelay);
                    }
                    if (e.target == this || e.target == $("#rmNav_icon")[0] || e.target == $(".rm-nav-name")[0] || e.target == $("#rmNav_downIcon")[0]) {
                        $(".rm-nav-lv1").slideToggle(10);
                        $("#rmNav_downIcon").toggleClass("rmNav_downIcon_hover");
                    }
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(".rm-nav-lv1").show();
                        //$(".rm-nav-lv1Tab:eq(0)").focus();
                        e.stopPropagation();
                    } else if (e.keyCode == 40) {
                        $(".rm-nav-lv1Tab:eq(0)").focus();
                    }
                }
            },
            "#rmNav_group"
        );
        $("body").on({
                click: function(e) {
                    window.location.href = "/Root/Home";
                },
                mouseenter: function(e) {
                    var self = $(this);
                    self.addClass("rmRibbonRow-right-item-hover");
                    $("#rmHome_icon").addClass("rmHome-icon-hover");
                },
                mouseleave: function(e) {
                    var self = $(this);
                    self.removeClass("rmRibbonRow-right-item-hover");
                    $("#rmHome_icon").removeClass("rmHome-icon-hover");
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                        e.stopPropagation();
                    }
                }
            },
            "#rmHome_group"
        );
        var lv2Delay = null;
        var lv2LeaveDelay = null;
        $("body").on({
                mouseenter: function(e) {
                    var nav1Tab = $(this);
                    clearTimeout(lv2LeaveDelay);
                    lv2Delay = setTimeout(function() {
                        nav1Tab.addClass("rm-nav-lv1Hover");
                        nav1Tab.siblings(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                        $(".rm-nav-lv2").hide();
                        var nav2 = nav1Tab.find(".rm-nav-lv2");
                        nav2.show();
                    }, delayTime);
                },
                mouseleave: function(e) {
                    var nav1Tab = $(this);
                    clearTimeout(lv2Delay);
                    lv2LeaveDelay = setTimeout(function() {
                        nav1Tab.removeClass("rm-nav-lv1Hover");
                        var nav2 = nav1Tab.find(".rm-nav-lv2");
                        nav2.hide();
                    }, delayTime);
                },
                keydown: function(e) {
                    var keyCode = e.which || event.keyCode || e.keyCode;
                    if (keyCode == 13) {
                        //$(this).mouseenter();
                        $(this).find(".rm-nav-lv2").show();
                        $(this).find(".rm-nav-lv2").focus();
                        e.stopPropagation();
                    } else if (keyCode == 9) {
                        $(".rm-nav-lv2").hide();
                        var nav1Tab = $(this);
                        nav1Tab.addClass("rm-nav-lv1Hover");
                        nav1Tab.siblings(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                        $(".rm-nav-lv2").hide();
                        var nav2 = nav1Tab.find(".rm-nav-lv2");
                        nav2.show();
                    }
                },
                focus: function() {
                    var nav1Tab = $(this);
                    nav1Tab.addClass("rm-nav-lv1Hover");
                    nav1Tab.siblings(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                    $(this).find(".rm-nav-lv2").show();
                }
            },
            ".rm-nav-lv1Tab"
        );
        var jmdelay = null;
        var jmleavedelay = null;
        $("body").on({
                mousedown: function(e) {
                    $(".rm-nav-jmIcon").addClass("rm-nav-jmIcon-hover");
                },
                click: function(e) {
                    window.location.href = "/Root/JM/Index";
                },
                mouseenter: function(e) {
                    var self = $(this);
                    clearTimeout(jmleavedelay);
                    jmdelay = setTimeout(function() {
                        $(".rm-nav-lv2").hide();
                        $(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                        self.addClass("rm-nav-jm-hover");
                    }, delayTime);
                },
                mouseleave: function(e) {
                    clearTimeout(jmdelay);
                    jmleavedelay = setTimeout(function() {
                        $(".rm-nav-jm").removeClass("rm-nav-jm-hover");
                    }, delayTime);
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                        e.stopPropagation();
                    } else if (e.keyCode == 9) {
                        $(".rm-nav-lv2").hide();
                    }
                },
                focus: function() {
                    $(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                    $(this).addClass("rm-nav-jm-hover");
                },
                blur: function() {
                    $(this).removeClass("rm-nav-jm-hover");
                }
            },
            ".rm-nav-jm"
        );
        var cpdelay = null;
        var cpleavedelay = null;
        $("body").on({
                mousedown: function(e) {
                    $(".rm-nav-cpIcon").addClass("rm-nav-cpIcon-hover");
                },
                mouseenter: function(e) {
                    var self = $(this);
                    clearTimeout(cpdelay);
                    cpdelay = setTimeout(function() {
                        $(".rm-nav-lv2").hide();
                        $(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                        self.addClass("rm-nav-cp-hover");
                    }, delayTime);
                },
                mouseleave: function(e) {
                    clearTimeout(cpdelay);
                    cpleavedelay = setTimeout(function() {
                        $(".rm-nav-cp").removeClass("rm-nav-cp-hover");
                    }, delayTime);
                },
                click: function(e) {
                    window.location.href = "/Root/CP/Index";
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                        e.stopPropagation();
                    } else if (e.keyCode == 9 && !e.shiftKey) {
                        $(".rm-nav-lv2").hide();
                        $("#rmNav_group").focus();
                    } else if (e.shiftKey && e.keyCode == 9) {
                        $(".rm-nav-jm").focus();
                    }
                },
                focus: function() {
                    $(this).addClass("rm-nav-cp-hover");
                },
                blur: function() {
                    $(this).removeClass("rm-nav-cp-hover");
                }
            },
            ".rm-nav-cp"
        );
        $(".rm-nav-lv2Tab a").on("keydown", function(e) {
            var $parent = $(this).parent(".rm-nav-lv2Tab");
            if (e.keyCode == 9 && !e.shiftKey) {
                if ($parent.index() == $parent.closest(".rm-nav-lv2").find("li").length - 1) {
                    $(".rm-nav-lv2").hide();
                    if ($parent.closest(".rm-nav-lv1Tab").index() == $(".rm-nav-lv1Tab").length - 1) {
                        $(".rm-nav-jm").focus();
                        $(".rm-nav-lv2").hide();
                    } else {
                        $(this).closest(".rm-nav-lv1Tab").next().focus();
                        $(".rm-nav-lv2").hide();
                    }
                } else {
                    $parent.next().find("a").focus();
                }
                e.stopPropagation();
                e.preventDefault();
            } else if (e.shiftKey && e.keyCode == 9) {
                $parent.prev().find("a").focus();
                e.stopPropagation();
                e.preventDefault();
            }
        });
        $(".rm-nav-lv1").on("keydown", function(e) {
            if (e.keyCode == 27) {
                $("#rmNav_icon").removeClass("rmNav_icon_hover");
                $("#rmNav_downIcon").removeClass("rmNav_downIcon_hover");
                $(".rm-nav-lv1Tab").removeClass("rm-nav-lv1Hover");
                $(".rm-nav-cp").removeClass("rm-nav-cp-hover");
                $(".rm-nav-lv1").hide();
                $(".rm-nav-lv2").hide();
            }
        });
    },
    hideMenu: function() {
        $(".rm-nav-lv1").hide();
    }
});
$$page("rm.Change.Font", {
    _minSize: 20,
    _maxSize: 28,
    _callbackFuncs: [],
    _prepare: function() {
        var initSize = RM.Cookie.UserInfo.getFontSize();
        $("html")[0].style.fontSize = initSize;
    },
    _create: function() {
        this._init()._bindEvent();
    },
    _init: function() {
        var initSize = RM.Cookie.UserInfo.getFontSize();
        if (parseFloat(initSize) == this._minSize) {
            $("#font_shrink").removeClass("font-shrink-hover");
        }
        if (parseFloat(initSize) == this._maxSize) {
            $("#font_expend").removeClass("font-expend-hover");
        }
        return this;
    },
    _bindEvent: function() {
        var self = this;
        $("body").on({
                click: function(e) {
                    var size = $("html")[0].style.fontSize;
                    var textFontSize = parseFloat(size);
                    if (textFontSize < self._maxSize) {
                        textFontSize += 2;
                        $("html").css("font-size", textFontSize + "px");
                        if (textFontSize == self._maxSize) {
                            $("#font_expend").removeClass("font-expend-hover");
                        }
                        $("#font_shrink").addClass("font-shrink-hover");
                        self.changHandlers(self, textFontSize);
                    }
                    // for IE repaints
                    if (document.body.currentStyle) {
                        $("a:not(.aui-treeview-item-li-a), span, div:not(.aui-icon-tree-hide), p, h2, h3").css('visibility', 'visible');
                    }
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                    }
                }
            },
            "#font_expend"
        );
        $("body").on({
                click: function(e) {
                    var size = $("html")[0].style.fontSize;
                    var textFontSize = parseFloat(size);
                    if (textFontSize > self._minSize) {
                        textFontSize -= 2;
                        $("html").css("font-size", textFontSize + "px");
                        if (textFontSize == self._minSize) {
                            $("#font_shrink").removeClass("font-shrink-hover");
                        }
                        $("#font_expend").addClass("font-expend-hover");
                        self.changHandlers(self, textFontSize);
                    }
                    // for IE repaints
                    if (document.body.currentStyle) {
                        $("a:not(.aui-treeview-item-li-a), span, div:not(.aui-icon-tree-hide), p, h2, h3").css('visibility', 'visible');
                    }
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                    }
                }
            },
            "#font_shrink"
        );
    },
    changHandlers: function(self, textFontSize) {
        RM.Cookie.UserInfo.setFontSize(textFontSize + "px");
        var top = $("#rmNav_group").height() + 12;
        $(".rm-nav-lv1").css("top", top + "px");
        var len = self._callbackFuncs.length;
        for (var i = 0; i < len; i++) {
            try {
                self._callbackFuncs[i](textFontSize);
            } catch (e) {}
        }
    },
    setBaseFontChangeCallBack: function(func) {
        this._callbackFuncs.push(func);
    }
});
$$page("rm.User.Event", {
    _create: function() {
        this._bindEvent();
    },
    _bindEvent: function() {
        var hide = null;
        $("body").on({
                click: function(e) {
                    if (e.target == this || e.target == $("#rmUserManager_Icon")[0] || e.target == $("#rmUserManager_Arrow")[0] || e.target == $("#rmUserManager_Name")[0]) {
                        $("#rm_helpContext").removeClass("rm-help-clic-style");
                        $("#rmUserManager_Content").toggleClass("rmUserManager-Click-Style");
                        $("#rmUserManager_DropDown").slideToggle(10);
                        $("#rmUserManager_Arrow").toggleClass("rmUserManager_Arrow_Down");
                        $("#rmHelp_DropDownList").hide(1);
                    }
                },
                mouseenter: function(e) {
                    if (hide != null) {
                        clearTimeout(hide);
                    }
                },
                mouseleave: function(e) {
                    hide = setTimeout(function() {
                        if ($(document.activeElement).closest("#rmUserManager_Content").length == 0) {
                            $("#rmUserManager_Content").removeClass("rmUserManager-Click-Style");
                            $("#rmUserManager_DropDown").hide(1);
                            $("#rmUserManager_Arrow").removeClass("rmUserManager_Arrow_Down");
                        }
                    }, 200);
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                    }
                }
            },
            "#rmUserManager_Content"
        );
        $("#rmUserManager_Logout").on("keydown", function(e) {
            if (e.keyCode == 9) {
                $("#rmUserManager_DropDown").hide();
            }
        });
    }
});
$$page("rm.Help.Event", {
    _create: function() {
        this._bindEvent();
    },
    _bindEvent: function() {
        var hide = null;
        $("#rmHelp_Abort_Title,.ra-help-about-popup,.ra-help-about-placeholder").on("click", function() {
            $(".ra-help-about-popup,.ra-help-about-placeholder").toggleClass("show");
        });
        $("body").on({
                click: function(e) {
                    if (e.target == this || e.target == $("#rmHelp")[0]) {
                        $("#rmUserManager_Content").removeClass("rmUserManager-Click-Style");
                        $("#rm_helpContext").toggleClass("rm-help-clic-style");
                        $("#rmHelp_DropDownList").slideToggle(10);
                        $("#rmUserManager_DropDown").hide(1);
                    }
                },
                mouseenter: function(e) {
                    if (hide != null) {
                        clearTimeout(hide);
                    }
                },
                mouseleave: function(e) {
                    hide = setTimeout(function() {
                        if ($(document.activeElement).closest("#rm_helpContext").length == 0) {
                            $("#rm_helpContext").removeClass("rm-help-clic-style");
                            $("#rmHelp_DropDownList").hide(1);
                        }
                    }, 200);
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                    }
                }
            },
            "#rm_helpContext"
        );
        $("#rmHelp_Abort_Title").on("keydown", function(e) {
            if (e.keyCode == 9) {
                setTimeout(function() {
                    $("#rm_helpContext").removeClass("rm-help-clic-style");
                    $("#rmHelp_DropDownList").hide();
                }, 10);
            }
        });
    }
});

$$page("rm.Notification.Event", {
    _create: function() {
        this._bindEvent();
    },
    _bindEvent: function() {
        var hide = null;
        var self = this;
        $("body").on({
                click: function(e) {
                    $(".notification_suitbar_container").toggle();
                    $(".notification_suitbar").toggle(50);
                    $(".rmSuitBar-notification-Img").toggleClass("rmSuitBar-notification_selected-Img");
                    $(".rmSuitBar-notification-Img").removeClass("rmSuitBar-notification-alert-Img");
                    $(".rmSuitBar-notification-Img").parent().toggleClass("rm_notification_selected");
                },
                //mouseenter: function (e) {
                //    if (hide != null) {
                //        clearTimeout(hide);
                //    }
                //},
                mouseleave: function(e) {
                    self.bindCheckBoxClickHideNotification();
                },
                keydown: function(e) {
                    if (e.keyCode == 13) {
                        $(this).click();
                    }
                    if (e.keyCode == 9) {
                        if ($(".notification_suitbar_container").is(":visible")) {
                            e.preventDefault();
                            $(".head-title span").focus();
                        }
                    }
                }
            },
            "#rm_notification"
        );
        $(".notification_suitbar .close_btn").on("click", function() {
            $(".notification_suitbar").toggle(50);
            $(".notification_suitbar_container").hide();
            $(".rmSuitBar-notification-Img").toggleClass("rmSuitBar-notification_selected-Img");
            $(".rmSuitBar-notification-Img").parent().toggleClass("rm_notification_selected");
        }).on("keydown", function(e) {
            if (e.keyCode == 13) {
                $(this).click();
                $("#rm_notification").focus();
            }
        });

        $(".notification_suitbar .notification_title .action-buttons .dismiss-all").on("click", function() {
            $(".notification_suitbar .notification_body .jobNoti").hide();
            $(".notification_suitbar .notification_title .action-buttons").hide();
            $(".notification_body_empty").show();
        }).on("keydown", function(e) {
            if (e.keyCode == 13) {
                $(this).click();
            }
        });
        $("body").click(function(e) {
            if ($(e.target).closest("#rm_notification").length == 0 && $(e.target).closest(".notification_suitbar").length == 0) {
                $(".notification_suitbar").hide(50);
                $(".notification_suitbar_container").hide();
                $(".rmSuitBar-notification-Img").removeClass("rmSuitBar-notification_selected-Img");
                $(".rmSuitBar-notification-Img").parent().removeClass("rm_notification_selected");
            }
        });
    },
    bindCheckBoxClickHideNotification: function() {
        var checkBoxClick = function() {
            $(".notification_suitbar").hide(50);
            $(".notification_suitbar_container").hide();
            $(".rmSuitBar-notification-Img").removeClass("rmSuitBar-notification_selected-Img");
            $(".rmSuitBar-notification-Img").parent().removeClass("rm_notification_selected");
        };
        $(".aui-checkbox").click(checkBoxClick);
    }
});
RM.IsIE = function() {
    if (!!window.ActiveXObject || "ActiveXObject" in window)
        return true;
    else
        return false;
};

RM.IsEdge = function() {
    var userAgent = navigator.userAgent;
    var isEdge = userAgent.indexOf("Edge") > -1;
    return isEdge;
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

        }
    },
    remove: function() {
        RM.Cookie.remove(this.Status, "/");
    },
    CreateSuccess: "createSuccess",
    EditSuccess: "editSuccess",
    Status: "commSts",
};

$$page("rm.VPAT.Event", {
    _create: function() {
        this._init().skip_navigation();
    },
    _init: function() {
        $(".skip_navigation_words").messagebox({
            //title: $("#msg").attr("tip"),
            width: 600,
            contentMaxheight: 160,
            type: "w",
            buttons: {
                "Yes": function(e, args) {
                    $(this).messagebox("hide");
                    if ($(".aui-breadcrumb-active").length > 0) {
                        $(".aui-breadcrumb-active div").focus();
                    } else {
                        $(".rm-home-module-icon-bcm").next(".rm-home-moudule-title").focus();
                    }
                },
                "No": function(e, args) {
                    $(this).messagebox("hide");
                    $("#font_shrink").focus();
                }
            },
            parameters: {
                "Yes": {},
                "No": {}
            },
            theme: {
                "Yes": "blue"
            }
        });
        $(".aui-messagebox-title").attr("tabindex", "0");
        return this;
    },
    skip_navigation: function() {
        $(".skip_navigation").on("focus", function() {
            $(".skip_navigation_words").messagebox("show");
            setTimeout(function() {
                $(".aui-messagebox-title").focus();
            }, 250);
        });
    }
});

$$page("rm.notification", {
    _create: function() {
        this._init();
    },
    _init: function() {
        $(".notification_menu .close_btn").click(function() {
            $(".notification_menu").hide();
            $(".notification_menu .jobMsg").remove();
        }).on("keydown", function(e) {
            if (e.keyCode == 13) {
                $(this).click();
            }
        });
        return this;
    },
});
RM.IsIE = function() {
    if (!!window.ActiveXObject || "ActiveXObject" in window)
        return true;
    else
        return false;
};

String.prototype.endsWith = function(suffix) {
    return this.indexOf(suffix, this.length - suffix.length) !== -1;
};

Date.prototype.add = function(milliseconds) {
    var m = this.getTime() + milliseconds;
    return new Date(m);
};
Date.prototype.addSeconds = function(second) {
    return this.add(second * 1000);
};
Date.prototype.addMinutes = function(minute) {
    return this.addSeconds(minute * 60);
};
Date.prototype.addHours = function(hour) {
    return this.addMinutes(60 * hour);
};

Date.prototype.addDays = function(day) {
    return this.addHours(day * 24);
};

Date.isLeepYear = function(year) {
    return (year % 4 == 0 && year % 100 != 0);
};

Date.daysInMonth = function(year, month) {
    if (month == 2) {
        if (year % 4 == 0 && year % 100 != 0)
            return 29;
        else
            return 28;
    } else if ((month <= 7 && month % 2 == 1) || (month > 7 && month % 2 == 0))
        return 31;
    else
        return 30;
};

Date.prototype.addMonth = function() {
    var m = this.getMonth();
    if (m == 11) return new Date(this.getFullYear() + 1, 1, this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());

    var daysInNextMonth = Date.daysInMonth(this.getFullYear(), this.getMonth() + 1);
    var day = this.getDate();
    if (day > daysInNextMonth) {
        day = daysInNextMonth;
    }
    return new Date(this.getFullYear(), this.getMonth() + 1, day, this.getHours(), this.getMinutes(), this.getSeconds());
};

Date.prototype.subMonth = function() {
    var m = this.getMonth();
    if (m == 0) return new Date(this.getFullYear() - 1, 12, this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());
    var day = this.getDate();
    var daysInPreviousMonth = Date.daysInMonth(this.getFullYear(), this.getMonth());
    if (day > daysInPreviousMonth) {
        day = daysInPreviousMonth;
    }
    return new Date(this.getFullYear(), this.getMonth() - 1, day, this.getHours(), this.getMinutes(), this.getSeconds());
};

Date.prototype.addMonths = function(addMonth) {
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

Date.prototype.addYears = function(year) {
    return new Date(this.getFullYear() + year, this.getMonth(), this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());
};
RM.IsIE = function() {
    if (!!window.ActiveXObject || "ActiveXObject" in window)
        return true;
    else
        return false;
};
RM.SimplifyArray = function(oldArr, ojsProps, delProps) {
    if (!oldArr) {
        return [];
    }
    var newArr = [];
    //delete empty properties
    for (var i = 0; i < oldArr.length; i++) {
        var tempData = oldArr[i];
        var newData = cloneObject(tempData, ojsProps, delProps);
        newArr.push(newData);
    }
    return newArr;

    function cloneObject(obj, ojsProps, delProps) {
        var newData = {};
        for (var key in obj) {
            if (delProps && delProps.indexOf(key) > -1) {
                continue;
            }
            var val = obj[key];
            if (ojsProps && ojsProps.indexOf(key) > -1 && val) {
                newData[key] = cloneObject(val, ojsProps);
            } else if (val && val != '00000000-0000-0000-0000-000000000000') {
                newData[key] = val;
            }
        }
        return newData;
    }
};
RM.RuleRelated = {
    IconsClass: {
        SP: "spSourcePng",
        EXO: "exoSourcePng"
    },
    addIconForCriteriaItem: {
        //SPRule
        1: function(cretiaText) {
            return this.getHtml(RM.RuleRelated.IconsClass.SP, cretiaText);
        },
        //EXORule
        2: function(cretiaText) {
            return this.getHtml(RM.RuleRelated.IconsClass.EXO, cretiaText);
        },
        getHtml: function(iconClass, cretiaText) {
            return '<div class="cretia-icon-wrapper" style="padding:0;"><img alt="" class="' + iconClass + '" src="../Images/Base/action_button.png" style="position:relative;"></div>' + "<span class='cretia-text'>" + cretiaText + "</span>";
        }
    },
    appendIcon: function(ruleSource, item) {
        return this.addIconForCriteriaItem[ruleSource](item);
    },
    addIconForCriterias: function(cretias, filters, ruleSource) {
        var self = this,
            newHtmls = [];
        $.each(cretias, function(k, item) {
            newHtmls.push(self.appendIcon(ruleSource, item));
        });
        return newHtmls;
    }
};
RM.SwitchLanguage = {
    setFontFamily: function() {
        $("body").css('font-family', 'Meiryo UI');
    }
};

String.prototype.endWith = function(str) {
    var reg = new RegExp(str + "$");
    return reg.test(this);
};

String.prototype.startWith = function(str) {
    if (str == null || str == "" || this.length == 0 || str.length > this.length)
        return false;
    if (this.substr(0, str.length) == str)
        return true;
    else
        return false;
};