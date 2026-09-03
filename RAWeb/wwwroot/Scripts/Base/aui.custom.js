/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
/*aui slider*/
(function ($) {
    var extensionMethods = {
        pips: function (settings) {
            options = {
                showPips: true,
            };
            $.extend(options, settings);
            this.element.addClass("ra-slider-pips").removeClass("ui-corner-all").find(".ui-slider-pip").remove().addClass("ui-slider-range").addClass("ui-widget-header").addClass("ui-slider-range-min");
            this.element.find("a").removeClass("ui-corner-all");
            var pips = this.options.max - this.options.min;
            var minNum = this.options.min;
            for (i = minNum; i <= pips + minNum ; i++) {
                var s = $('<span class="ra-slider-pip"><span class="ra-slider-line"></span><span class="ra-slider-number">' + i + '</span></span>');
                if (options.showPips == true) {
                    s.addClass('ra-slider-pip-show');
                } else {
                    s.addClass('ra-slider-pip-hide');
                }
                if (this.options.orientation == "horizontal")
                    s.css({
                        left: '' + (100 / pips) * (i - minNum) + '%'
                    });
                else
                    s.css({
                        top: '' + (100 / pips) * (i - minNum) + '%'
                    });
                this.element.append(s);
            }
        }
    };
    $.extend(true, $['ui']['slider'].prototype, extensionMethods);
})(jQuery);

