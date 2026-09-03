/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
(function ($) {
    $.fn.extend({
        RMLoading: function (options) {
            var that = this;
            if (options == "show") {
                that.showLoading();
                return;
            }
            if (options == "hide") {
                that.hideLoading();
                return;
            }

            var $self = $(this);
            var deafualt = {
                zIndex: 999,
                opacity: 0.5,
                overlayColor: "#666666"
            };
            var parameter = $.extend({}, deafualt, options);
            if ($self.loading) {
                $self.loading(parameter);
                $self.children(".aui-loading-content").css("display", "none");
            }
        },
        showLoading: function () {
            var $self = $(this);
            if ($self.loading) {
                $self.loading("show");
                $self.children(".aui-loading-overlay").focus();
            }
        },
        hideLoading: function () {
            var $self = $(this);
            if ($self.loading) {
                $self.loading("hide");
            }
        },
    });
})(jQuery);
(function ($) {
    $.fn.extend({
        rmTopMessageBar: function (options) {
            var self = $(this);
            self.empty();
            var deafualt = {
                type: 'info',
                content: "",
                margin: [0, 0, 0, 0],
                showClose:true
            };
            var parameter = $.extend({}, deafualt, options);
            var temp = $('<div class="ra-topMessageBar-icon"></div><div class="ra-topMessageBar-message" tabindex="0"></div><div class="ra-topMessageBar-close-border"><div class="ra-topMessageBar-close" tabindex="0"  role="button" aria-label="' + RMResx.RM_JS_Common_Close +'"></div></div>');
            self.append(temp);
            var self_margin = "";
            parameter.margin.map(
                function (item, i) {
                    self_margin += (item + "px" + " ");
                });
            self.css('position', 'relative').css('min-height', '28px').css('margin', self_margin);
            if (self.css('display') == 'none') {
                self.toggle();
            }
            if (!parameter.showClose) {
                self.find(".ra-topMessageBar-close-border").css('display','none');
            }
            if (parameter.type == 'info') {
                self.find(".ra-topMessageBar-icon").css('background-position', '-16px 0');
                self.css('background-color', '#ecf9ff').css('border', '1px solid #54abe8');
            }
            if (parameter.type == 'error') {
                self.find(".ra-topMessageBar-icon").css('background-position', '0 0');
                self.css('background-color', '#ffecec').css('border', '1px solid #ff6f6f');
            }
            if (parameter.type == 'warning') {
                self.find(".ra-topMessageBar-icon").css('background-position', '-48px 0');
                self.css('background-color', '#fff2e2').css('border', '1px solid #ff9a48');
            }
            if (parameter.type == 'success') {
                self.find(".ra-topMessageBar-icon").css('background-position', '-32px 0');
                self.css('background-color', '#f0ffe7').css('border', '1px solid #77cc50');
            }
            self.find(".ra-topMessageBar-message").html(parameter.content);
            if (self.height()>30) {
                self.find(".ra-topMessageBar-message").css('padding', '5px 30');
            }
            self.find(".ra-topMessageBar-close-border").click(function () {
                self.toggle();
                self.empty();
                if (parameter.closeEvent) {
                    parameter.closeEvent();
                }
            }).on("keydown", function (e) {
                if (e.keyCode == 13) {
                    self.toggle();
                    self.empty();
                    if (parameter.closeEvent) {
                        parameter.closeEvent();
                    }
                }
            });
        }
    });
})(jQuery);