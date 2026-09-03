/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
function initLogonPage(script) {
    $$page("rm.logonPage", {
        _data: JSON.parse(script),
        _$mb: $("#rmMessageBox"),
        _prepare: function () {
            var self = this;
            self._data.provider.selectionChanged = self._onProviderChanged;
            self._createCombobox($("#rmLogOnProvider"), self._data.provider);

            if (self._data.adDomain) {
                self._data.adDomain.selectionChanged = self._onDomainChanged;
                self._createCombobox($("#rmDomains"), self._data.adDomain);
            }

            $("#rmRememberId").checkbox();
            var usr = RM.Cookie.UserInfo.getLoginName();
            if (usr && usr.length > 0) {
                $("#rmUserName").val(usr);
                $("#rmRememberId").checkbox("checked", true);
            } else {
                $("#rmUserName").val("");
            }
            $("#rmPassword").val("").css("visibility", "visible");
            $("#rmEmptyPassword").css("display", "none");

            self._resizePageContent();
        },
        _create: function () {
            this._initMessagebox()._bindEvents();
        },
        _bindEvents: function () {
            var self = this;
            $(window).on('resize', self._resizePageContent);
            $("#rmUserName").on('keydown', self, self._logOnInputKeyDown);
            $("#rmPassword").on('keydown', self, self._logOnInputKeyDown).focus(function () {
                this.type = 'password';
            });
            $("#rmLogin").click(self._logOn);
            return self;
        },
        _resizePageContent: function (e) {
            var marginTop = ($("body").height() - $("#rmLoginBody").height() - 110) / 3;
            $("#rmLoginBody").css("margin-top", marginTop);
            //for combobox popup off set position 
            if (e) {
                $(".aui-combobox-popup").css("display", "none");
                $("#rmLogOnProvider").find(".aui-combobox-input").click().click();
            }
        },
        _createCombobox: function ($ele, data) {
            var settings = {
                dataTextField: "text",
                dataValueField: "id",
                width: "328px",
                height: "33px",
                popupWidth: "328px",
            };
            if (data.selectedId) {
                settings.selectedItem = { id: data.selectedId }
            } else {
                if (!data.waterMark) {
                    settings.selectedIndex = 0;
                } else {
                    settings.waterMark = data.waterMark;
                }
            }
            if (data.selectionChanged) {
                settings.selectionChanged = data.selectionChanged;
            }
            $ele.combobox(settings);
            $ele.combobox("itemsSource", data.items);
        },
        _initMessagebox: function () {
            var self = this;
            self._$mb.messagebox({
                width: 600,
                contentMaxheight: 160,
                type: "e",
                buttons: {
                    "OK": function (e, args) {
                        $(this).messagebox("hide").text($("#rmMsgEmptyInput").val());
                        var un = $("#rmUserName"), pw = $("#rmPassword");
                        if (un.val().length == 0) {
                            un.focus();
                        }
                        else if (pw.val().length == 0) {
                            pw.focus();
                        }
                        else {
                            un.focus();
                        }
                    }
                },
                parameters: {
                    "OK": { msg: "1" },
                },
                theme: {
                    "OK": "blue"
                }
            });
            var errorMsg = self._data.errorMessage;
            if (errorMsg && errorMsg.length > 0) {
                self._$mb.text(errorMsg);
                self._$mb.messagebox("show");
            }
            return self;
        },
        _onProviderChanged: function (e, args) {
            var providerModel = args.newValue.value;
            if (providerModel == "ADIntegration") {
                $("#rmDomains").parent().css("display", "");
            }
            else {
                $("#rmDomains").parent().css("display", "none");
            }
            if (providerModel == "WindowsIntegration") {
                $("#rmUserName").attr("disabled", "disabled");
                $("#rmPassword").attr("disabled", "disabled");
            }
            else {
                $("#rmUserName").removeAttr("disabled");
                $("#rmPassword").removeAttr("disabled");
            }
            $('#AuthenticationMode').val(providerModel);
        },
        _onDomainChanged: function (e, args) {
            $('#Domain').val(args.newValue.item.text);
            $('#DomainId').val(args.newValue.item.id);
        },
        _logOn: function () {
            var providerModel = $('#rmLogOnProvider').combobox("selectedValue");
            var postUrl = '/Account/LogOn';
            if (providerModel == 'ADIntegration' || providerModel == 'LocalSystem') {
                var un = $("#rmUserName").val().trim();
                var pw = $("#rmPassword").val().trim();
                if (un.length > 0 && pw.length > 0) {
                    if ($("#rmRememberId").checkbox("checked")) {
                        RM.Cookie.UserInfo.setLoginName($("#rmUserName").val());
                    }
                    else {
                        RM.Cookie.UserInfo.setLoginName('');
                    }
                }
                else {
                    $("#rmMessageBox").messagebox("show");
                    return;
                }
            }
            else {
                postUrl = "/windows/login";
            }
            var logoForm = $('#rmLoginForm');
            var returnUrl = RM.Url.getParam(location.href, "redirecturl");
            if (returnUrl.length > 0) {
                postUrl += '?redirecturl=' + returnUrl;

            }
            $('#rmLoginForm').attr('action', postUrl).submit();
            this.disabled = true;
        },
        _logOnInputKeyDown: function (e) {
            if (e.keyCode == 13) {
                e.returnValue = false;
                e.cancel = true;
                e.data._logOn();
            }
        }
    });
}