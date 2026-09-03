/*Covered by AvePoint copyright and license agreement*/
"use strict";

(function (window, document, $, $$) {

    //保存当前的$$.SVG对应的值
    //var _SVG = $$.SVG;

    //与对象相关的公共方法
    var objectUtill;
    //与jQuery相关的公共方法
    var jQueryUtil;
    //SVG对应的局部变量

    var svgUtil;
    /**
     * Prometheus SVG控件的基础类。其中包括一些公共方法。
     * @class SVG
     * @static
     */
    svgUtil = function () {
    };
    objectUtill = {
        //SVG-related namespace URLS
        _XMLNS: "http://www.w3.org/2000/svg",

        //defined the xlink namespace that SVG uses
        _XLINK: "http://www.w3.org/1999/xlink",

        //log: function (msg) {
        //    if ("console" in window) {
        //        window.console.log(msg);
        //    }
        //},
        /**
         * 创建元素节点
         * @method create
         * @param {String} svg 要创建元素的名称
         * @param {Object} attr 元素的属性。
         * @return {Object} 元素的属性及属性值
         */
        create: function (svg, attr) {
            if (!this.isIE8()) {
                svg = document.createElementNS(this._XMLNS, svg);
                if (svg.nodeName == "svg") {
                    svg.setAttribute("xmlns", this._XMLNS);
                    svg.setAttribute("xmlns:xlink", this._XLINK);
                    svg.setAttribute("version", "1.1");
                }
                if (attr) {
                    $(svg).svgAttr(attr);
                }
                return svg;
            }
            else {
                var vml;
                switch (svg) {
                    case 'g':
                    case 'svg':
                        vml = document.createElement('div');
                        vml.style.width = 0;
                        vml.style.height = 0;
                        vml.style.position = 'relative';
                        break;
                    case 'div':
                        vml = document.createElement('div');
                        break;
                    default:
                        vml = document.createElement('v:' + svg);
                        break;
                }
                if (attr) {
                    $(vml).svgAttr(attr);
                }
                return vml;
            }
        },
        /**
         * 创建svg 元素
         * @method svg
         * @param {String} svg 元素的id
         * @param {Object} attr 元素的属性及属性值
         * @return {Object} 返回包含attr的svg元素
         */
        svg: function (id, attr) {
            var svg = this.create("svg", attr);
            $(svg).svgAttr({
                id: id ? id : null
            });
            return svg;
        },
        /**
         * 创建g 元素
         * @method g
         * @param {Object} attr  元素的属性。
         * @return {Object} 返回包含attr的g元素。
         */
        g: function (attr) {
            return this.create("g", attr);
        },
        /**
         * 创建line 元素
         * @method line
         * @param {Number}  x1  起点x坐标。
         * @param {Number} y1 起点y坐标。
         * @param {Number} x2  终点x坐标。
         * @param {Number} y2 终点 y坐标。
         * @return {Object} 返回line元素。
         */
        line: function (x1, y1, x2, y2) {
            if (!this.isIE8()) {
                return this.create("line", {
                    x1: x1,
                    y1: y1,
                    x2: x2,
                    y2: y2,
                    /*strokeWidth: 1, */
                    stroke: "#000000",
                    shapeRendering: "crispEdges"
                });
            }
            else {
                var vml = this.create("line");
                vml.strokecolor = "#000000";
                //vml.strokeweight = '1px';
                vml.style.left = x1 + 'px';
                vml.style.top = y1 + 'px';
                vml.from = '0,0';
                vml.to = (x2 - x1) + 'px,' + (y2 - y1) + 'px';
                return vml;
            }
        },
        /**
         * 创建rect元素
         * @method rect
         * @param {Number} x 矩形左上角的坐标(用户坐标系)的x值。
         * @param {Number} y 矩形左上角的坐标(用户坐标系)的y值。
         * @param {Number} width 矩形宽度。
         * @param {Number} height 矩形的高度。
         * @param {Number} rx 实现圆角效果时，圆角沿x轴的半径。
         * @param {Number} ry 实现圆角效果时，圆角沿y轴的半径
         */
        rect: function (x, y, width, height, rx, ry) {
            if (!this.isIE8()) {
                return this.create("rect", {
                    x: x,
                    y: y,
                    rx: rx ? rx : 0,
                    ry: ry ? ry : 0,
                    width: width,
                    height: height
                });
            }
            else {
                var vml = this.create('rect');
                vml.style.left = x + 'px';
                vml.style.top = y + 'px';
                vml.style.width = width + 'px';
                vml.style.height = height + 'px';
                vml.style.padding = 0;
                vml.filled = 'f';
                vml.stroked = 'f';
                return vml;
            }
        },
        /**
         * 创建circle元素
         * @method circle
         * @param {Number} cx 圆心坐标x值。
         * @param {Number} cy 圆心坐标y值。
         * @param {Number} r 圆半径。
         */
        circle: function (cx, cy, r) {
            if (!this.isIE8()) {
                return this.create("circle", {
                    cx: cx,
                    cy: cy,
                    r: r
                });
            }
            else {
                var path = 'AE0 0 ' + r + ' ' + r + ' 0 23592960';
                var shape = this.svgPath(path);
                $(shape).svgAttr({
                    a: cx,
                    b: cy
                });
                return shape;
            }
        },
        /**
         * 创建 path 元素
         * @method path
         * @param {String} d 一系列绘制指令和绘制参数(点)组合成。
         */
        svgPath: function (d) {
            if (!this.isIE8()) {
                return this.create("path", {
                    d: d
                });
            }
            else {
                var shape = this.create('shape');
                shape.style.width = '100px';
                shape.style.height = '100px';
                shape.coordsize = '100,100';
                shape.path = d;
                shape.stroked = 'f';
                return shape;
            }
        },
        /**
         * 创建text 元素。
         * @method text
         * @param {Object} text 元素的content。
         * @param {Number} x 文本坐标x值。
         * @param {Number} y 文本坐标y值。
         * @param {String} anchor 文本显示的方向 。
         * @param {Number} fontSize 文本的字体大小。
         */
        text: function (text, x, y, anchor, fontSize) {// start, middle end
            if (!this.isIE8()) {
                var t = this.create("text", {
                    x: x,
                    y: y,
                    textAnchor: anchor ? anchor : "start",
                    stroke: "none",
                    fontFamily: 'Verdana'
                });
                t.textContent = text;
                $(t).css({
                    fontSize: fontSize ? fontSize : 12
                });
                return t;
            }
            else {
                var div = this.create('div');
                div.innerText = text;
                $(div).css({
                    position: 'absolute',
                    left: x + 'px',
                    top: (y - (fontSize || 12)) + 'px',
                    color: '#222',
                    fontFamily: "Verdana",
                    fontSize: fontSize ? fontSize + 'px' : '12px',
                    padding: 0,
                    //if (!wrap) {
                    whiteSpace: 'nowrap',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis'
                    //}
                });
                anchor = anchor || 'start';
                switch (anchor) {
                    case 'start':
                        anchor = 'left';
                        break;
                    case 'end':
                        anchor = 'right';
                        break;
                    case 'middle':
                        anchor = 'center';
                        break;
                    default:
                        anchor = 'left';
                        break;
                }
                $(div).css('textAlign', anchor);
                return div;
            }
        },

        isIE8: function () {
            return $.support.leadingWhitespace === false;
        },

        //TextTrimming
        trimmingSvgText: function (text, maxWidth) {
            if (!$$.isIE8()) {
                var index = 1,
                    str = text.textContent,
                    width = text.getComputedTextLength(),
                    length = str.length;
                if (width > maxWidth) {
                    while (index <= length) {
                        var subStr = str.substr(0, index);
                        text.textContent = subStr + "...";
                        width = text.getComputedTextLength();
                        if (width > maxWidth) {
                            index--;
                            text.textContent = str.substr(0, index) + "...";
                            return true;
                        }
                        index++;
                    }
                }
            } else {
                $(text).svgAttr({
                    width: maxWidth,
                    wrap: false
                });
            }

            return false;
        },
        trimmingText: function (text, maxWidth) {
            if (!$$.isIE8()) {
                var index = 1,
                    str = text.textContent,
                    width = text.getComputedTextLength(),
                    length = str.length;
                if (width > maxWidth) {
                    while (index <= length) {
                        var subStr = str.substr(0, index);
                        text.textContent = subStr;
                        width = text.getComputedTextLength();
                        if (width > maxWidth) {
                            index--;
                            return [str.substr(0, index), str.substr(index, length)];
                        }
                        index++;
                    }
                }
            }
            return [str, ''];
        }
    };

    $.extend(svgUtil, objectUtill);
    jQueryUtil = {
        /**
         * 将jQuery属性转化成svg属性
         * @method svgAttr 
         * @param attr jQuery属性
         */
        svgAttr: function (attr) {
            var key, value, node;
            if (!$$.isIE8()) {
                if (typeof attr == "string") {
                    if (attr == "translate") {
                        var transform = this.attr("transform");
                        if (typeof transform == "undefined") {
                            return {
                                x: 0,
                                y: 0
                            };
                        } else {
                            transform = transform.replace(/translate/, "");
                            // 得到数值
                            transform = transform.substring(1, transform.length - 1);
                            // 去掉括号
                            return {
                                x: transform.split(" ")[0],
                                y: transform.split(" ")[1]
                            };
                        }
                    } else {
                        if (attr != "viewBox") {
                            return this.attr(attr.replace(/([A-Z])/g, "-$1").toLowerCase());
                        } else {
                            var vb = this.get(0).getAttributeNS(null, attr);
                            if (vb) {
                                vb = vb.split(" ");
                                return {
                                    x: vb[0],
                                    y: vb[1],
                                    width: vb[2],
                                    height: vb[3]
                                };
                            } else {
                                return {
                                    x: 0,
                                    y: 0,
                                    width: 0,
                                    height: 0
                                };
                            }
                        }
                    }
                } else if (typeof attr == "object") {
                    for (key in attr) {
                        value = String(attr[key]);
                        node = this.get(0);
                        switch (key) {
                            case "viewBox":
                                node.setAttributeNS(null, key, value);
                                break;
                            case "fontSize":
                                $(node).css('fontSize', value);
                                break;
                            default:
                                node.setAttributeNS(null, key.replace(/([A-Z])/g, "-$1").toLowerCase(), value);
                                break;
                        }
                    }
                }
            } else {
                if (typeof attr == "object") {
                    for (key in attr) {
                        value = attr[key];
                        node = this.get(0);
                        var isDiv = node.nodeName == 'DIV';
                        switch (key) {
                            case "x":
                                node.style.left = value + 'px';
                                break;
                            case "y":
                                node.style.top = value + 'px';
                                break;
                            case "stroke":
                                node.stroked = (value == 'transparent' || value == 'none') ? 'f' : 't';
                                node.strokecolor = value;
                                break;
                            case "strokeWidth":
                                node.stroked = value == 0 ? 'f' : 't';
                                node.strokeweight = value + 'px';
                                break;
                            case "fill":
                                if (isDiv) {
                                    $(node).css('color', value == 'currentcolor' ? '' : value);
                                    //node.style.color = value;
                                } else {
                                    node.filled = (value == 'transparent' || value == 'none') ? 'f' : 't';
                                    node.fillcolor = value;
                                }
                                break;
                            case "strokeDasharray":
                                var stroke = $$.create('stroke');
                                stroke.dashstyle = value;
                                node.appendChild(stroke);
                                break;
                            case "fillOpacity":
                                var fill = $$.create('fill');;
                                fill.opacity = value;
                                node.appendChild(fill);
                                break;
                            case "wrap":
                                //node.style.whiteSpace = value ? 'normal' : 'nowrap'
                                node.style.overflow = value ? 'visible' : 'hidden';
                                node.style.textOverflow = value ? 'clip' : 'ellipsis';
                                break;
                            case "points":
                            case "coordsize":
                                //key = key.replace(/([A-Z])/g, "-$1").toLowerCase();
                                node.setAttribute(key, value);
                                break;
                            case "class":
                                node.className = value;
                                break;
                            case "transform":
                            case "style":
                                break;
                            case "width":
                            case "height":
                            case "display":
                            case "cursor":
                            case "fontFamily":
                            case "overflow":
                            case "textAlign":
                            case "position":
                            case "whiteSpace":
                            default:
                                $(node).css(key, value);
                                break;
                        }
                        //this.get(0).setAttribute(key, value);
                    }
                }
            }
            return null;
        },
        /**
         *
         */
        svgFill: function (color) {
            if (!$$.isIE8()) {
                this.get(0).setAttribute("fill", color);
            }
            else {
                this.get(0).filled = (color == 'transparent' || color == 'none') ? 'f' : 't';
                this.get(0).fillcolor = color;
            }
        },
        /**
         * 设置svg元素的translate坐标属性
         * @method svgTranslate
         * @param {Number} x x轴水平向右坐标
         * @param {NUmber} y y轴竖直向下坐标
         */
        svgTranslate: function (x, y) {
            if (!$$.isIE8()) {
                var transform = this.attr("transform");
                if (!transform) {
                    if (!x) { x = 0; }
                    if (!y) { y = 0; }
                    transform = "translate(" + x + "," + y + ")";
                }
                else if (transform.indexOf("translate") < 0) {
                    if (!x) { x = 0; }
                    if (!y) { y = 0; }
                    transform += " translate(" + x + "," + y + ")";
                }
                else {
                    x = (x != 0 && !x) ? "$1" : x;
                    y = (y != 0 && !y) ? "$2" : "," + y;
                    transform = transform.replace(/translate\((-?\d+(?:\.\d+)?)((?:\s+|,)?-?\d+(?:\.\d+)?)?\)/ig, "translate(" + x + y + ")");
                }
                this.attr("transform", transform);
                //this.setRotate(null, x, y);
            }
            else {
                if (x && !isNaN(x)) { this.get(0).style.left = x + 'px'; }
                if (y && !isNaN(y)) { this.get(0).style.top = y + 'px'; }
            }
        },
        /**
         * 设置svg元素的scale缩放属性
         * @method setScale
         * @param {Number} s 元素缩放比例
         */
        setScale: function (x, y) {
            if (!$$.isIE8()) {
                var transform = this.attr("transform");
                if (!transform) {
                    if (!x) { x = 1; }
                    if (!y) { y = 1; }
                    transform = "scale(" + x + "," + y + ")";
                }
                else if (transform.indexOf("scale") < 0) {
                    if (!x) { x = 1; }
                    if (!y) { y = 1; }
                    transform += "scale(" + x + "," + y + ")";
                }
                else {
                    x = (!x) ? "$1" : x;
                    y = (!y) ? "$2" : "," + y;
                    transform = transform.replace(/scale\((-?\d+(?:\.\d+)?)((?:\s+|,)?-?\d+(?:\.\d+)?)?\)/ig, "scale(" + x + y + ")");
                }
                this.attr("transform", transform);
            }
            else {
                if (!x) { x = 1; }
                if (!y) { y = 1; }
                var getLength = function (length, s) {
                    if (length && length.length > 2) {
                        length = length.substr(0, length.length - 2);
                        if (!isNaN(length)) {
                            return (parseFloat(length) / s);
                        }
                    }
                    return 0;
                }
                var node = this.get(0);
                var cx = getLength(node.style.width, x),
                    cy = getLength(node.style.height, y);
                node.coordsize = cx + ',' + cy;
            }
        },
        /**
         * 设置svg元素的rotate旋转属性
         * @method setRotate
         * @param {Number} r 元素旋转角度
         * @param {Number} x 元素旋转x轴坐标（仅svg元素有效）
         * @param {NUmber} y 元素旋转y轴坐标（仅svg元素有效）
         */
        setRotate: function (r, x, y) {
            if (!$$.isIE8()) {
                var transform = this.attr("transform");
                if (!transform) {
                    if (!r) { r = 0; }
                    if (!x) { x = 0; }
                    if (!y) { y = 0; }
                    transform = "rotate(" + r + " " + x + "," + y + ")";
                }
                else if (transform.indexOf("rotate") < 0) {
                    if (!r) { r = 0; }
                    if (!x) { x = 0; }
                    if (!y) { y = 0; }
                    transform += " rotate(" + r + " " + x + "," + y + ")";
                }
                else {
                    r = (!r) ? "$1" : r;
                    x = (!x) ? "$2" : x;
                    y = (!y) ? "$3" : y;
                    transform = transform.replace(/rotate\((-?\d+(?:\.\d+)?)(?:\s(-?\d+(?:\.\d+)?)(?:\s+|,)(-?\d+(?:\.\d+)?))?\)/ig, "rotate(" + r + " " + x + "," + y + ")");
                }
                this.attr("transform", transform);
            }
            else {
                if (r && !isNaN(r)) {
                    this.get(0).style.rotation = r + 'deg';
                }
            }
        }
    }

    $.fn.extend(jQueryUtil);
    $.extend($$, svgUtil);

})(window, window.document, jQuery, AUI);


//barchart
(function ($$, $) {
    function barChart(settings, style, element) {
        this.container = $(element).css(style.containerCss);
        this.stage = null;
        this.leftArrowImg = null;
        this.rightArrowImg = null;
        this.config = new chartConfig();
        this.pager = new chartPagerInfo(settings, style, this);
        this.xAxis = new chartXAxisInfo(settings, style, this);
        this.tooltip = new chartTooltip(settings, style, this);
        var that = this;

        this.group = {
            top: $$.g(), grid: $$.g(), barLabel: $$.g(), yBarLabel: $$.g(),
            bar: $$.g(), barOut: $$.g(), pageArrow: $$.g(), barG: $$.g(), clipBarG: $$.g(), clipG: $$.g()
        };

        function prepareData() {
            if (settings.stacked && settings.series && settings.series.length > 0) {
                for (var i = 0; i < settings.series.length; i++) {
                    settings.series[i].stackedData = [];
                    for (var j = 0; j < settings.yAxis.length; j++) {
                        var stackedItem = {};
                        stackedItem.sum = 0;
                        stackedItem.stacks = [];
                        for (var m = 0; m < settings.series[i].stacks.length; m++) {
                            var stackedItemContent = {};
                            var item = settings.series[i].data[j];
                            var stack = settings.series[i].stacks[m];
                            stackedItemContent.data = item[stack.property];
                            stackedItem.sum += stackedItemContent.data;
                            stackedItemContent.name = stack.name;
                            stackedItemContent.style = stack.style;
                            stackedItemContent.styleHighLight = stack.styleHighLight;
                            stackedItem.stacks.push(stackedItemContent);
                        }
                        settings.series[i].stackedData.push(stackedItem);
                    }
                }
            }
        }
        function initElement() {
            if ($$.isIE8()) {
                initVmlCss();
            }
            that.config.chartId = "aui-bar-chart-" + new Date().getTime();
            that.stage = $$.svg(that.config.chartId, { width: settings.width, height: settings.height });
            that.container.append(that.stage);
            that.config.left = settings.left || that.config.left;
            that.config.chartWidth = settings.width - that.config.left - that.config.right;
            that.config.chartHeight = settings.height - that.config.bottom - that.config.paddingTop;
            that.pager.init();
            that.xAxis.init();
        }
        //初始化vml namespaces、css for IE8
        function initVmlCss() {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
        function drawAxis() {
            var y = that.config.paddingTop + that.config.chartHeight,
                isTop = settings.xAxis.position && settings.xAxis.position.toLowerCase() == 'top',
                top = !isTop ? that.config.paddingTop : that.config.paddingTop - 5,
                x = that.config.left + that.config.chartWidth,
                line;
            line = $$.line(that.config.left, top, that.config.left, y);
            $(line).svgAttr(style.hLine);
            that.group.grid.appendChild(line);

            top = !isTop ? y : that.config.paddingTop - 5;
            var title = settings.xAxis.title;
            var text = $$.text(title, x, top + 3, "start");
            $(text).css(style.titleCss);
            that.group.grid.appendChild(text);
            var width = !$$.isIE8() ? text.getComputedTextLength() : text.offsetWidth;
            x = that.config.left + that.config.chartWidth - width;
            $(text).svgAttr({ x: x });

            line = $$.line(that.config.left, top, x - 5, top);
            $(line).svgAttr(style.hLine);
            that.group.grid.appendChild(line);
        }
        function drawtopArea() {
            if (settings.hiddenTop) {
                return;
            }
            var rect;
            var barStyle;
            var color;
            var left = 30;
            var top = 5;
            var txt, text;
            for (var i = 0; i < settings.series.length; i++) {
                if (!settings.stacked) {
                    barStyle = settings.series[i].style || style.bar;
                    color = barStyle.fill || style.bar.fill;
                    rect = $$.rect(left, top, 10, 10);
                    $(rect).svgAttr({ fill: color });
                    that.group.top.appendChild(rect);
                    left += 10;
                    left += 5;

                    txt = settings.series[i].name;
                    text = $$.text(txt, left, top + 10, "start");
                    $(text).css(style.titleCss);
                    that.group.top.appendChild(text);
                    left += !$$.isIE8() ? text.getComputedTextLength() : text.offsetWidth;
                    left += 30;
                }
                else {
                    for (var j = 0; j < settings.series[i].stacks.length; j++) {
                        barStyle = settings.series[i].stacks[j].style || style.bar;
                        color = barStyle.fill || style.bar.fill;
                        rect = $$.rect(left, top, 10, 10);
                        $(rect).svgAttr({ fill: color });
                        that.group.top.appendChild(rect);
                        left += 10;
                        left += 5;

                        txt = settings.series[i].stacks[j].name;
                        text = $$.text(txt, left, top + 10, "start");
                        $(text).css(style.titleCss);
                        that.group.top.appendChild(text);
                        left += !$$.isIE8() ? text.getComputedTextLength() : text.offsetWidth;
                        left += 30;
                    }
                }
            }
        }
        function drawClipG() {
            var clipId = that.config.chartId + "-clip",
                y = that.config.paddingTop - 5,
                width = that.config.left + that.config.chartWidth + 15,
                height = that.config.chartHeight - 5;
            if (!$$.isIE8()) {
                var clipPath = $$.create("clipPath");
                clipPath.setAttribute("id", clipId);
                var rect = $$.rect(0, y, width, height, 0, 0);
                clipPath.appendChild(rect);
                that.group.clipG.appendChild(clipPath);
                that.group.clipBarG.setAttribute("clip-path", "url(#" + clipId + ")");
                $(that.group.barG).svgTranslate(null, -settings.gapHeight / 2);
            }
            else {
                $(that.group.clipBarG).svgAttr({
                    y: y,
                    width: width,
                    height: height,
                    overflow: 'hidden'
                });
                $(that.group.barG).svgTranslate(null, -y - settings.gapHeight / 2);
            }
            $(that.group.clipBarG).svgTranslate(0, 0);
        }
        function drawBarsAndXLabels() {
            if (that.pager.pageCount > 1) {
                that.pager.refreshPageStatus();
            }
            var data = that.pager.getPagedData();
            if (!data || data.length == 0) {
                return;
            }
            var max = that.xAxis.getXAxisInfo().max;
            var count = data[0].length;
            var barCountInOneGroup = settings.series.length;
            var columnWidth = settings.gapHeight + settings.barHeight;
            var barGroupWidth = settings.barHeight;
            var groupMargin = settings.gapHeight;
            var barWidth = barGroupWidth;
            var barMargin = 0;
            if (barCountInOneGroup > 1) {
                barWidth = Math.floor(barGroupWidth * 0.97 / barCountInOneGroup);
                barMargin = (barGroupWidth - barWidth * barCountInOneGroup) / (barCountInOneGroup - 1);
            }
            for (var i = 0; i < count; i++) {
                for (var j = 0; j < data.length; j++) {
                    var item = data[j][i];
                    if (item > 0) {
                        var a = that.config.chartWidth / max;
                        var y = that.config.paddingTop + (columnWidth * i + groupMargin) + j * (barWidth + barMargin);
                        drawBar(that.config.left, y, a, barWidth, j, i, item);
                    }
                }
                var xLabelX = that.config.left - 5;;
                var xLabelY = that.config.paddingTop + (columnWidth * i + groupMargin);
                var xLabelText = settings.yAxis[i];
                drawYLabel(xLabelX, xLabelY, xLabelText, that.config.left - 6, barGroupWidth);
            }
        }
        function drawBar(x, y, a, height, groupIndex, barIndex, item) {
            if (settings.stacked) {
                drawMultiBars(x, y, a, height, groupIndex, barIndex);
            } else {
                drawSingleBar(x, y, a, height, groupIndex, barIndex, item);
            }
        }
        function drawSingleBar(x, y, a, height, groupIndex, barIndex, item) {
            var width = a * item;
            if (item > 0 && width < 2) {
                width = 2;
            }
            var rect = $$.rect(x - that.config.left, y, width, height, 0, 0);
            var barStyle = settings.series[groupIndex].style || style.bar;
            $(rect).svgAttr(barStyle);
            rect.setAttribute("groupIndex", groupIndex);
            rect.setAttribute("barIndex", barIndex);
            $(rect).bind('mousemove', rect, function (e) {
                that.eventHandler.bar_mousemove(e);
            }).bind('mouseover', rect, function (e) {
                that.eventHandler.bar_mouseover(e);
            }).bind('mouseleave', rect, function (e) {
                that.eventHandler.bar_mouseout(e);
            });

            rect.onclick = that.eventHandler.bar_click;
            that.group.bar.appendChild(rect);

            drawBarLabel(x + width, y - 8, height, item, groupIndex, barIndex);
        }
        function drawMultiBars(x, y, a, height, groupIndex, barIndex) {
            var g = $$.g();
            var stackedData = settings.series[groupIndex].stackedData[barIndex];
            var stacks = stackedData.stacks;
            var left = x;
            var labelRight = 0;
            for (var i = 0; i < stacks.length; i++) {
                var data = stacks[i].data;
                var barWidth = Math.round(a * data);
                if (data > 0 && barWidth < 2) {
                    barWidth = 2;
                }
                var rect = $$.rect(left - that.config.left, y, barWidth, height);
                $(rect).svgAttr(stacks[i].style || style.bar);
                rect.setAttribute("groupIndex", groupIndex);
                rect.setAttribute("barIndex", barIndex);
                rect.setAttribute("stackIndex", i);
                $(rect).bind('mousemove', rect, function (e) {
                    that.eventHandler.bar_mousemove(e);
                }).bind('mouseover', rect, function (e) {
                    that.eventHandler.bar_mouseover(e);
                }).bind('mouseleave', rect, function (e) {
                    that.eventHandler.bar_mouseout(e);
                });
                rect.onclick = that.eventHandler.bar_click;
                g.appendChild(rect);

                left += barWidth;
                if (data > 0) {
                    var text = drawBarLabel(left, y - 8, height, data, groupIndex, barIndex, i);
                    var width = $$.isIE8() ? $(text).width() : text.getComputedTextLength();
                    var labelLeft = left - width / 2;
                    if (i == 0 && labelLeft < that.config.left) {
                        $(text).svgTranslate(that.config.left - labelLeft);
                        labelRight = left + width / 2 + that.config.left - labelLeft;
                    } else if (i > 0 && labelRight && labelRight > 0 && labelLeft <= labelRight) {
                        $(text).remove();
                    } else {
                        labelRight = left + width / 2;
                    }
                }
            }
            that.group.bar.appendChild(g);;
        }
        function drawBarLabel(x, y, height, content, groupIndex, barIndex, stackIndex) {
            var anchor = 'middle';
            if (settings.valuePosition == 'right') {
                x += 5;
                y += 3 + height / 2 + 4;
                anchor = 'start';
            }
            if (settings.valueTemplate && settings.valueTemplate != $.noop) {
                var data = {
                    label: settings.yAxis[barIndex],
                    data: settings.series[groupIndex].data[barIndex],
                    group: settings.series[groupIndex].name
                };
                if (settings.stacked) {
                    var stack = settings.series[groupIndex].stackedData[barIndex].stacks[stackIndex];
                    data.stackName = stack.name;
                    data.stackData = stack.data;
                }
                content = settings.valueTemplate(data);
            }
            var text = $$.text(content, x, y, anchor);
            $(text).svgAttr(style.barLabel);
            $(text).css(style.barLabelCss);
            that.group.barLabel.appendChild(text);
            return text;
        }
        function drawYLabel(x, y, content, maxWidth, barHeight) {
            var text = $$.text(content, x, y, "end");
            $(text).svgAttr(style.barLabel);
            $(text).css(style.barLabelCss);
            text.setAttribute("fullText", content);
            $(text).bind('mousemove', text, function (e) {
                that.eventHandler.bar_mousemove(e);
            }).bind('mouseover', text, function (e) {
                that.eventHandler.bar_mouseover(e);
            }).bind('mouseleave', text, function (e) {
                that.eventHandler.bar_mouseout(e);
            });
            that.group.yBarLabel.appendChild(text);
            if (!$$.isIE8()) {
                var width = text.getComputedTextLength();
                //需要做换行处理
                //if (width > maxWidth) {
                //    var createtspan = function (value) {
                //        var tspan = $$.create('tspan', {
                //            x: x,
                //            dy: tspans.length == 0 ? 0 : 12
                //        });
                //        tspan.textContent = value;
                //        tspans.push(tspan);
                //    };
                //    var list = content.split(' ');
                //    content = '';
                //    var c = '';
                //    var tspans = [];
                //    for (var i = 0; i < list.length; i++) {
                //        content += list[i];
                //        text.textContent = content;
                //        width = text.getComputedTextLength();
                //        if (width > maxWidth) {
                //            if (c != '') {
                //                createtspan(c);
                //            }
                //            content = list[i];
                //            text.textContent = content;
                //            width = text.getComputedTextLength();
                //            if (width > maxWidth) {
                //                var index = 1;
                //                while (index <= content.length) {
                //                    var subStr = content.substr(0, index);
                //                    text.textContent = subStr;
                //                    width = text.getComputedTextLength();
                //                    if (width > maxWidth) {
                //                        index--;
                //                        c = content.substr(0, index);
                //                        createtspan(c);
                //                        content = content.substr(index);
                //                        index = 0;
                //                    }
                //                    index++;
                //                }
                //            }
                //            c = content;
                //            content += ' ';
                //        }
                //        else {
                //            c = content;
                //            content += ' ';
                //        }
                //        if (i == list.length - 1) {
                //            createtspan(c);
                //        }
                //    }
                //    if (tspans.length > 3) {
                //        tspans = tspans.slice(0, 3);
                //        var span = tspans[2];
                //        var c = span.textContent;
                //        text.textContent = c + '...';
                //        width = text.getComputedTextLength();
                //        while (width > maxWidth && c.length > 1) {
                //            c = c.substring(0, c.length - 1);
                //            text.textContent = c + '...';
                //            width = text.getComputedTextLength();
                //        }
                //        span.textContent = c + '...';
                //    }
                //    if (tspans.length > 0) {
                //        text.textContent = null;
                //        $(text).append(tspans);
                //        y -= (tspans.length - 1) * 11 / 2;
                //    }
                //}
                if (width > maxWidth - 13) {
                    var index = content.length - 1;
                    var c = '';
                    while (index <= content.length) {
                        var subStr = content.substr(0, index);
                        text.textContent = subStr.trim();
                        width = text.getComputedTextLength();
                        if (width > maxWidth - 13) {
                            index--;
                            c = content.substr(0, index);
                            //text.textContent = c;
                            //content = content.substr(index);
                            text.textContent = c;
                        } else {
                            text.textContent = text.textContent + '...';
                            break;
                        }
                    }
                }

                y += (barHeight + 8) / 2;
                $(text).svgAttr({ y: y });
            }
            else {
                $(text).svgAttr({
                    x: 3,
                    width: maxWidth,
                    whiteSpace: 'normal',
                    wordWrap: 'break-word'
                });
                y += (barHeight - text.offsetHeight) / 2;
                $(text).svgAttr({ y: y });
            }
        }
        //function clearChart() {
        //    that.container.find("div[name]").remove();
        //    $(that.group.barLabel).empty();
        //    $(that.group.barOut).empty();
        //}
        function firstRender() {
            if ($$.isIE8()) {
                that.group.bar = $$.create('group', {
                    width: 100,
                    height: 100,
                    x: 0,
                    y: 0,
                    coordsize: '100,100'
                });
            }
            $(that.group.bar).svgTranslate(that.config.left, 0);

            if (settings.startAnimate == true || (settings.startAnimate == null && settings.animate && settings.animate.startAnimate)) {
                $(that.group.bar).svgAttr({
                    display: 'none'
                });
                if (settings.animate && settings.animate.showBarLabelAfterAnimate) {
                    $(that.group.barLabel).svgAttr({
                        display: 'none'
                    });
                }
            }

            //that.group.top.setAttribute("class", "chart-top-lines");
            that.group.grid.setAttribute("class", "chart-grid-lines");
            that.group.barOut.setAttribute("class", "chart-bars-out");
            that.group.bar.setAttribute("class", "chart-bars");
            that.group.barLabel.setAttribute("class", "chart-bar-label");
            that.group.yBarLabel.setAttribute("class", "chart-ybar-label");
            that.group.pageArrow.setAttribute("class", "chart-page-arrow");
            $(that.group.pageArrow).svgAttr({
                position: 'static'
            });
            that.group.barG.setAttribute("class", "chart-barG");
            that.group.clipBarG.setAttribute("class", "chart-clipBarG");
            that.group.clipG.setAttribute("class", "chart-clipG");
            //that.stage.appendChild(that.group.top);
            that.stage.appendChild(that.group.grid);
            that.group.barG.appendChild(that.group.barOut);
            that.group.barOut.appendChild(that.group.bar);
            that.group.barG.appendChild(that.group.barLabel);
            that.group.barG.appendChild(that.group.yBarLabel);
            that.group.clipBarG.appendChild(that.group.barG);
            that.stage.appendChild(that.group.clipBarG);
            that.stage.appendChild(that.group.pageArrow);
            that.stage.appendChild(that.group.clipG);
        }
        function afterRender() {
            if (settings.startAnimate == true || (settings.startAnimate == null && settings.animate && settings.animate.startAnimate)) {
                setTimeout(beginAnimation, 1);
            }
        }
        function beginAnimation() {
            var barOut = that.group.barOut,
                group = barOut.firstChild;//group.bar
            //$(group).setScale(0.01, 1);
            $(group).svgAttr({
                display: 'block'
            });
            if (!Object.prototype.hasOwnProperty.call(barOut, "scale")) {
                Object.defineProperty(barOut, "scale", {
                    set: function (val) {
                        if ($$.isIE8()) {
                            //$(group).setScale(val, 1);
                            $(group).svgAttr({
                                width: Math.max(100 * val, 1)
                            });
                        }
                        else {
                            $(group).setScale(val, 1);
                        }
                    }
                });
            }
            $(barOut).animate({ scale: 1 }, 900, function () {
                $(that.group.barLabel).svgAttr({
                    display: 'block'
                });
            });
        }

        that.init = function () {
            prepareData();
            initElement();
            firstRender();
            drawtopArea();
            drawAxis();
            drawClipG();
            drawBarsAndXLabels();
            afterRender();
            that.tooltip.initTooltip();
        };

        this.getData = function (series, index) {
            var value = 0;
            if (settings.stacked) {
                if (settings.series[series].stackedData.length > index) {
                    value = settings.series[series].stackedData[index].sum;
                }
            } else if (settings.property) {
                if (settings.series[series].data.length > index) {
                    value = settings.series[series].data[index][settings.property];
                }
            } else {
                if (settings.series[series].data.length > index) {
                    value = settings.series[series].data[index];
                }
            }
            return parseInt(value);
        };

        this.eventHandler = {
            bar_click: function () {
                if (settings.clickCallback) {
                    var barIndex = this.getAttribute("barIndex");
                    var groupIndex = this.getAttribute("groupIndex");
                    var data = {
                        label: settings.yAxis[barIndex],
                        data: settings.series[groupIndex].data[barIndex]
                    };
                    if (settings.stacked) {
                        var stackIndex = this.getAttribute("stackIndex");
                        var stack = settings.series[groupIndex].stackedData[barIndex].stacks[stackIndex];
                        data.stackName = stack.name;
                        data.stackData = stack.data;
                    }
                    settings.clickCallback(data);
                }
            },
            bar_mouseover: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (!isLabel) {
                    $(rect).svgAttr({ cursor: "pointer" });
                }
                if (!isLabel) {
                    var barIndex = rect.getAttribute("barIndex");
                    var groupIndex = rect.getAttribute("groupIndex");
                    var data = {
                        label: settings.yAxis[barIndex],
                        data: settings.series[groupIndex].data[barIndex],
                        group: settings.series[groupIndex].name
                    };
                    var barStyle = settings.series[groupIndex].styleHighLight || style.barHighLight;
                    if (settings.stacked) {
                        var stackIndex = rect.getAttribute("stackIndex");
                        var stack = settings.series[groupIndex].stackedData[barIndex].stacks[stackIndex];
                        barStyle = stack.styleHighLight || style.barHighLight;
                        data.stackName = stack.name;
                        data.stackData = stack.data;
                    }
                    $(rect).svgAttr(barStyle);
                } else {
                    data = rect.getAttribute("fullText");
                }
                //var e = window.event || arguments[0];
                that.tooltip.createTooltip(e, data, isLabel);
            },
            bar_mousemove: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (that.tooltip.showTooltip) {
                    //var e = window.event || arguments[0];
                    that.tooltip.moveTooltip(e, isLabel);
                }
            },
            bar_mouseout: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (!isLabel) {
                    var groupIndex = rect.getAttribute("groupIndex");
                    var barStyle = settings.series[groupIndex].style || style.bar;
                    if (settings.stacked) {
                        var stackIndex = rect.getAttribute("stackIndex");
                        barStyle = settings.series[groupIndex].stacks[stackIndex].style || style.bar;
                    }
                    $(rect).svgAttr(barStyle);
                }
                that.tooltip.removeTooltip();
            },
            clickLeftArrow: function () {
                that.pager.pageIndex--;
                that.pager.refreshPageStatus();
                if (that.pager.pageIndex >= 0) {
                    var limit = -that.pager.pageIndex * that.config.chartHeight - settings.gapHeight / 2;
                    if ($$.isIE8()) {
                        limit -= that.config.paddingTop - 5;
                    }
                    $(that.group.barG).animate({ 'translateY': limit }, 900);
                }
            },
            clickRightArrow: function () {
                that.pager.pageIndex++;
                that.pager.refreshPageStatus();
                if (that.pager.pageIndex < that.pager.pageCount) {
                    var limit = -that.pager.pageIndex * that.config.chartHeight - settings.gapHeight / 2;
                    if ($$.isIE8()) {
                        limit -= that.config.paddingTop - 5;
                    }
                    $(that.group.barG).animate({ 'translateY': limit }, 900);
                }
            }
        };
    }

    function chartConfig() {
        this.chartId = '';
        this.paddingTop = 35;
        this.left = 120;
        this.right = 10;
        this.bottom = 20;
        this.chartWidth = 0;
        this.chartHeight = 0;
    }

    function chartTooltip(settings, style, chart) {
        this.showTooltip = false;
        this.tooltipElement = null;
        this.tooltipElement_Label = null;
        this.isLabel = false;
        this.initTooltip = function () {
            this.tooltipElement = $('<span></span>');
            if (settings.tooltip && typeof settings.tooltip == 'object' && settings.tooltip.toolTipStyle != null) {
                if (typeof settings.tooltip.toolTipStyle == "string") {
                    this.tooltipElement.addClass(settings.tooltip.toolTipStyle);
                } else if (typeof settings.tooltip.toolTipStyle == "object") {
                    this.tooltipElement.css(settings.tooltip.toolTipStyle);
                }
            } else {
                this.tooltipElement.css(style.tooltipCss_Black);
            }
            this.tooltipElement.css({
                visibility: "hidden",
                position: "absolute"
            });
            chart.container.append(this.tooltipElement);

            this.tooltipElement_Label = $('<span></span>');
            this.tooltipElement_Label.css(style.lableTooltipCss);
            this.tooltipElement_Label.css("visibility", "hidden");
            chart.container.append(this.tooltipElement_Label);
        };
        this.createTooltip = function (e, data, isLabel) {
            this.showTooltip = true;
            this.isLabel = isLabel;
            var tip = isLabel ? this.tooltipElement_Label : this.tooltipElement;
            tip.empty();
            var content = '';
            if (this.isLabel) {
                content = data;
            } else {
                var template = settings.tooltip;
                if (settings.tooltip && typeof settings.tooltip == 'object' && settings.tooltip.toolTipTemplate) {
                    template = settings.tooltip.toolTipTemplate;
                }
                if (template && template != $.noop) {
                    content = template(data);
                }
                else {
                    //tooltip default value
                    content = data.label + ": ";
                    if (settings.stacked) {
                        content += data.stackData;
                    } else if (settings.property) {
                        content += data.data[settings.property];
                    } else {
                        content += data.data;
                    }
                }
            }
            tip.append(content);
            tip.css("visibility", "visible");
            this.moveTooltip(e, isLabel);
        };
        this.moveTooltip = function (e, isLabel) {
            var mouseX, mouseY;
            if (e.pageX) {
                var offset = chart.container.offset();
                var offsetX = offset.left;
                var offsetY = offset.top;
                mouseX = e.pageX - offsetX;
                mouseY = e.pageY - offsetY;
            }
            else {
                mouseX = e.offsetX;
                mouseY = e.offsetY;
            }
            var tip = isLabel ? this.tooltipElement_Label : this.tooltipElement;
            var mX = mouseX + 15,
                mY = mouseY,
                tipWidth = tip.width() + 23,
                tipHeight = tip.height() + 16;
            var maxX = settings.width - tipWidth - 3,
                maxY = settings.height - tipHeight - 3;
            if (mX > maxX) {
                mX = mouseX - tipWidth - 8;
            }
            if (mY > maxY) {
                mY = mouseY - tipHeight + 20;
            }
            tip.css({
                left: mX + 'px',
                top: mY + 'px'
            });
        };
        this.removeTooltip = function () {
            //this.tooltipElement.empty();
            this.tooltipElement.css("visibility", "hidden");
            //this.tooltipElement_Label.empty();
            this.tooltipElement_Label.css("visibility", "hidden");
            this.showTooltip = false;
        };
    }

    function chartPagerInfo(settings, style, chart) {
        var that = this,
            config = chart.config,
            i;
        this.pageIndex = 0;
        this.pageCount = 1;
        this.pageSize = 0;
        this.getPagedData = function () {
            var count = settings.yAxis ? settings.yAxis.length : 0;
            var data = [];
            for (i = 0; i < count; i++) {
                if (i == count) break;
                for (var j = 0; j < settings.series.length; j++) {
                    if (!data[j]) data[j] = [];
                    data[j].push(chart.getData(j, i));
                }
            }
            if (count > 0 && data.length == 0) {
                data[0] = [];
                for (i = 0; i < count; i++) {
                    data[0].push(0);
                }
            }
            return data;
        };
        this.refreshPageStatus = function () {
            chart.leftArrowImg.style.display = "block";
            chart.rightArrowImg.style.display = "block";
            if (this.pageIndex == 0) {
                chart.leftArrowImg.style.display = "none";
            }
            if (this.pageIndex == this.pageCount - 1) {
                chart.rightArrowImg.style.display = "none";
            }
        };
        function drawPageArrow() {
            if (that.pageCount <= 1) return;
            var x = config.left;
            var leftY = config.paddingTop - 5;
            var rightY = config.paddingTop + config.chartHeight + 5;

            var left, leftPath, right, rightPath;
            if (!$$.isIE8()) {
                leftPath = "M-4 2 L0 -2 4 2 M-9,0 A9,9 0,1,1 -9,0.01Z";
                rightPath = "M4 -2 L0 2 -4 -2 M-9,0 A9,9 0,1,1 -9,0.01Z";
            }
            else {
                leftPath = 'M-4 2 L0 -2 4 2E AL0 0 9 9 0 23592960';
                rightPath = 'M4 -2 L0 2 -4 -2E AL0 0 9 9 0 23592960';
            }
            left = chart.leftArrowImg = $$.svgPath(leftPath);
            $(left).svgAttr(style.pageArrowPath);
            $(left).svgTranslate(x, leftY);

            right = chart.rightArrowImg = $$.svgPath(rightPath);
            $(right).svgAttr(style.pageArrowPath);
            $(right).svgTranslate(x, rightY);

            chart.group.pageArrow.appendChild(left);
            chart.group.pageArrow.appendChild(right);
            left.onclick = chart.eventHandler.clickLeftArrow;
            right.onclick = chart.eventHandler.clickRightArrow;

            //给chart.group.barG添加translateY get\set属性，用来上下翻页滚动动画
            if (!Object.prototype.hasOwnProperty.call(chart.group.barG, "translateY")) {
                Object.defineProperty(chart.group.barG, "translateY", {
                    get: function () {
                        return this._translateX;
                    },
                    set: function (val) {
                        $(chart.group.barG).svgTranslate(null, val);
                        this._translateX = val;
                    }
                });
            }
            if (!$$.isIE8()) {
                chart.group.barG.translateY = -settings.gapHeight / 2;
            } else {
                chart.group.barG.translateY = -settings.gapHeight / 2 - config.paddingTop + 5;
            }
        };
        this.init = function () {
            this.pageIndex = 0;
            this.pageSize = 0;
            this.pageCount = 1;
            if (!settings.yAxis || settings.yAxis.length == 0) {
                return;
            }
            var count = settings.yAxis.length;
            this.pageSize = count;

            var chartHeight = config.chartHeight;
            var barHeight = Math.floor(chartHeight / count);
            var minBarHeight = settings.gapHeight + settings.barHeight;
            if (barHeight < minBarHeight) {
                this.pageSize = parseInt(chartHeight / minBarHeight, 10);
                settings.gapHeight += (chartHeight - minBarHeight * this.pageSize) / this.pageSize;

                this.pageCount = Math.ceil(count / this.pageSize);
            }
            if (this.pageCount > 1) {
                drawPageArrow();
            }
        };
    }

    function chartXAxisInfo(settings, style, chart) {
        var max = 0;
        var min = 0;
        var pixelPerTick = 72;
        var interval = 0;
        function calMaxValue() {
            var count = settings.series.length;
            for (var i = 0; i < count; i++) {
                for (var j = 0; j < settings.yAxis.length; j++) {
                    var data = chart.getData(i, j);
                    if (data > max) max = data;
                }
            }
        };
        function calInterval() {
            if (max == min) {
                if (max == 0) max = 1;
                else {
                    max = Math.ceil(max * 1.2);
                }
                interval = max;
                return;
            }
            var multiples = [1, 2, 2.5, 5, 10];
            var height = chart.config.chartHeight;
            interval = (max - min) * pixelPerTick / (height || 1);
            var magnitude = Math.pow(10, Math.floor(Math.log(interval) / Math.LN10));
            var normalized = interval / magnitude;
            for (var i = 0; i < multiples.length; i++) {
                interval = multiples[i];
                if (normalized <= (multiples[i] + (multiples[i + 1] || multiples[i])) / 2) {
                    break;
                }
            }
            interval *= magnitude;
            if (interval < 1) interval = 1;
            var total = 0;
            while (total <= max * 1.1) {
                total += interval;
            }
            max = total;
        };
        this.getXAxisInfo = function () {
            return {
                max: max,
                min: min,
                interval: interval
            };
        };
        this.init = function () {
            calMaxValue();
            calInterval();
        };
    }

    $.widget("aui.barchart", {
        options: {
            width: 700,
            height: 400,
            left: 120,
            barHeight: 20,
            gapHeight: 20,
            tooltip: {
                toolTipTemplate: $.noop,
                toolTipStyle: null
            },
            valueTemplate: $.noop,
            valuePosition: 'top',
            yAxis: [],
            xAxis: {
                title: "Values",
                position: "bottom"
            },
            stacked: false,
            property: "",
            series: [],
            clickCallback: $.noop,
            hiddenTop: false,
            animate: {
                startAnimate: true,
                showBarLabelAfterAnimate: false
            }
        },

        //Build-in
        _create: function () {
            var chart = new barChart(this.options, this._style, this.element);
            chart.init();
        },
        _setOption: function (key, value) {
            this.options[key] = value;
            this.destroy();
            this._create();
        },
        destroy: function () {
            this.element.empty();
        },
        /**
         * 重画条形图。
         * @method draw
         * @param {Object} options 根据该配置信息重画条形图。
         */
        draw: function (options) {
            var self = this;
            if (options) {
                if (options.series) {
                    self.options.series = null;
                }
                if (options.xAxis) {
                    self.options.xAxis = null;
                }
                if (options.yAxis) {
                    self.options.yAxis = null;
                }
                //保持原来的部分属性
                $.extend(true, self.options, options);
            }
            self.destroy();
            self._create();
        },

        _style: {
            containerCss: {
                overflow: "hidden",
                position: "relative"
            },
            hLine: {
                //opacity: 1,
                fill: "none",
                stroke: "#999999",
                strokeWidth: 1
            },
            barLabel: {
                fill: "#222"
            },
            barLabelCss: {
                color: "#222",
                fontFamily: 'Verdana',
                fontSize: '13px',
                cursor: "default"
            },
            bar: {
                fill: "#3a6506"
            },
            barHighLight: {
                fill: "#558536"
            },
            //tooltip默认样式
            tooltipCss_Black: {
                position: 'absolute',
                //maxWidth: '160px',
                backgroundColor: '#333333',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#fff',
                //whiteSpace: 'nowrap',
                lineHeight: '18px',
                padding: '8px 15px 8px 8px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)'
            },
            lableTooltipCss: {
                position: "absolute",
                fontFamily: 'Verdana',
                fontSize: 12,
                backgroundColor: '#fff',
                border: '#999 1px solid',
                color: '#333333',
                padding: '2px 4px'
            },
            pageArrowPath: {
                fill: "#ffffff",
                stroke: "#999999",
                strokeWidth: 1
            },
            titleCss: {
                fontFamily: 'Verdana',
                fontSize: '12px',
                color: '#222'
            }
        }
    });

})(AUI, jQuery);


//columnchart
(function ($$, $) {
    function columnChart(settings, style, element) {
        this.container = $(element).css(style.containerCss);
        this.stage = null;
        this.leftArrowImg = null;
        this.rightArrowImg = null;
        this.config = new chartConfig();
        this.pager = new chartPagerInfo(settings, style, this);
        this.yAxis = new chartYAxisInfo(settings, style, this);
        this.tooltip = new chartTooltip(settings, style, this);
        var that = this;

        this.group = {
            top: $$.g(), grid: $$.g(), yLabel: $$.g(), barLabel: $$.g(),
            bar: $$.g(), barOut: $$.g(), pageArrow: $$.g(), barG: $$.g(), clipBarG: $$.g(), clipG: $$.g()
        };

        function prepareData() {
            if (settings.stacked && settings.series && settings.series.length > 0) {
                for (var i = 0; i < settings.series.length; i++) {
                    settings.series[i].stackedData = [];
                    for (var j = 0; j < settings.xAxis.length; j++) {
                        var stackedItem = {};
                        stackedItem.sum = 0;
                        stackedItem.stacks = [];
                        for (var m = 0; m < settings.series[i].stacks.length; m++) {
                            var stackedItemContent = {};
                            var item = settings.series[i].data[j];
                            var stack = settings.series[i].stacks[m];
                            stackedItemContent.data = item[stack.property];
                            stackedItem.sum += stackedItemContent.data;
                            stackedItemContent.name = stack.name;
                            stackedItemContent.style = stack.style;
                            stackedItemContent.styleHighLight = stack.styleHighLight;
                            stackedItem.stacks.push(stackedItemContent);
                        }
                        settings.series[i].stackedData.push(stackedItem);
                    }
                }
            }
        }
        function initElement() {
            if ($$.isIE8()) {
                initVmlCss();
            }
            that.config.chartId = "aui-column-chart-" + new Date().getTime();

            that.config.width = settings.width == 'auto' ? $(element).width() : settings.width;

            that.stage = $$.svg(that.config.chartId, { width: that.config.width, height: settings.height });
            that.container.append(that.stage);
            that.config.left = settings.left || that.config.left;
            that.config.chartWidth = that.config.width - that.config.left - that.config.right;
            that.config.chartHeight = settings.height - that.config.bottom - that.config.paddingTop;
            that.config.chartHeight2 = that.config.chartHeight - 100;
            that.pager.init();
            that.yAxis.init();
        }

        //初始化vml namespaces、css for IE8
        function initVmlCss() {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
        function drawtopArea() {
            if (settings.hiddenTop) {
                return;
            }
            var rect;
            var barStyle;
            var color;
            var max = 0;
            var top = 5;
            var txt, text;
            for (var i = 0; i < settings.series.length; i++) {
                if (!settings.stacked) {
                    barStyle = settings.series[i].style || style.bar;
                    color = barStyle.fill || style.bar.fill;
                    rect = $$.rect(0, top, 6, 6);
                    $(rect).svgAttr({ fill: color });
                    that.group.top.appendChild(rect);

                    txt = settings.series[i].name;
                    text = $$.text(txt, 11, top + 7, "start");
                    $(text).css(style.titleCss);
                    that.group.top.appendChild(text);

                    var w = !$$.isIE8() ? text.getComputedTextLength() : text.offsetWidth;
                    max = Math.max(max, w);
                    top += 20;
                }
                else {
                    for (var j = 0; j < settings.series[i].stacks.length; j++) {
                        barStyle = settings.series[i].stacks[j].style || style.bar;
                        color = barStyle.fill || style.bar.fill;
                        rect = $$.rect(0, top, 6, 6);
                        $(rect).svgAttr({ fill: color });
                        that.group.top.appendChild(rect);

                        txt = settings.series[i].stacks[j].name;
                        text = $$.text(txt, 11, top + 7, "start");
                        $(text).css(style.titleCss);
                        that.group.top.appendChild(text);

                        var w = !$$.isIE8() ? text.getComputedTextLength() : text.offsetWidth;
                        max = Math.max(max, w);
                        top += 20;
                    }
                }
            }
            $(that.group.top).svgTranslate(that.config.width - max - 27, 0);
        }
        function drawTitle() {
            var text;
            if (settings.yAxis.title) {
                text = $$.text(settings.yAxis.title, that.config.left, that.config.paddingTop - 20, "middle");
                $(text).svgAttr(style.yTitle);
                that.group.grid.appendChild(text);

                if (!$$.isIE8()) {
                    if (text.getComputedTextLength() / 2 > that.config.left - 12) {
                        $(text).svgAttr({
                            x: 10,
                            textAnchor: "start"
                        });
                    }
                }
                else {
                    $(text).svgAttr({
                        x: 10,
                        width: that.config.left - 12,
                        wrap: true
                    });
                    if (text.offsetWidth > that.config.left - 12) {
                        $(text).svgAttr({
                            width: 'auto',
                            textAlign: 'left'
                        });
                    }
                }
            }
            //x轴title
            if (settings.xAxisTitle) {
                var top = that.config.chartHeight + that.config.paddingTop + 5;
                if (that.pager.pageCount > 1) {
                    top += 17;
                }
                text = $$.text(settings.xAxisTitle, that.config.chartWidth + that.config.left + 5, top, "start");
                $(text).svgAttr(style.yTitle);
                that.group.grid.appendChild(text);
            }
        }
        function drawClipG() {
            var clipId = that.config.chartId + "-clip";
            var x = that.config.left,
                y = 0,
                width = that.config.chartWidth,
                height = that.config.chartHeight + that.config.bottom;
            if (!$$.isIE8()) {
                var clipPath = $$.create("clipPath");
                clipPath.setAttribute("id", clipId);
                var rect = $$.rect(x, y, width, height, 0, 0);
                clipPath.appendChild(rect);
                that.group.clipG.appendChild(clipPath);
                that.group.clipBarG.setAttribute("clip-path", "url(#" + clipId + ")");

                $(that.group.barG).svgTranslate(-settings.gapWidth / 2, -that.config.paddingTop);
            }
            else {
                $(that.group.clipBarG).svgAttr({
                    x: x,
                    width: width,
                    height: height,
                    overflow: 'hidden'
                });
                $(that.group.barG).svgTranslate(-x - settings.gapWidth / 2, -that.config.paddingTop);
            }
            $(that.group.clipBarG).svgTranslate(0, that.config.paddingTop);
        }
        function drawHLinesAndYLabels() {
            var pageInfo = that.yAxis.getYAxisInfo();
            var start = pageInfo.min;
            var end = pageInfo.max;
            var interval = pageInfo.interval;
            var count = Math.ceil((end - start) / (interval)) || 1;
            var cHeight = that.config.chartHeight - that.config.chartTopGap;
            var dH = Math.floor(cHeight / count);
            var last = cHeight % count;
            var y = that.config.paddingTop + last + that.config.chartTopGap;
            var line;
            //horizontal
            for (var i = 0; i < count + 1; i++) {
                if (i == count) {
                    line = $$.line(that.config.left - 4, y, that.config.left + that.config.chartWidth, y);
                    $(line).svgAttr(style.hLine);
                    that.group.grid.appendChild(line);
                } else {
                    line = $$.line(that.config.left - 4, y, that.config.left, y);
                    $(line).svgAttr(style.hLine);
                    //横向虚线
                    //var line = $$.line(that.config.left - 4, y + 0.5, that.config.left + that.config.chartWidth, y + 0.5);
                    //$(line).svgAttr({
                    //    strokeDasharray: "1,1",
                    //    stroke: style.hLine.stroke,
                    //    strokeWidth: 1
                    //});
                    that.group.grid.appendChild(line);
                }
                var txt = start + interval * (count - i);
                settings.yAxis.suffix ? txt += settings.yAxis.suffix : '';
                var text = $$.text(txt, that.config.left - 7, y + 5, "end");
                $(text).svgAttr(style.LabelStyle);
                that.group.yLabel.appendChild(text);
                y += dH;
            }
            that.config.baseY = y - dH;
            //vertical
            line = $$.line(that.config.left, that.config.paddingTop - 5, that.config.left, that.config.baseY);
            $(line).svgAttr(style.hLine);
            that.group.grid.appendChild(line);
        }
        function drawBarsAndXLabels() {
            if (that.pager.pageCount > 1) {
                that.pager.refreshPageStatus();
            }
            var data = that.pager.getPagedData();
            if (!data || data.length == 0) {
                return;
            }
            var max = that.yAxis.getYAxisInfo().maxx;
            var count = data[0].length;
            var barCountInOneGroup = settings.series.length;
            var columnWidth = settings.gapWidth + settings.barWidth;
            var barGroupWidth = settings.barWidth;
            var groupMargin = settings.gapWidth - 5;
            var barWidth = barGroupWidth;
            var barMargin = 0;
            if (barCountInOneGroup > 1) {
                barWidth = Math.floor(barGroupWidth * 0.97 / barCountInOneGroup);
                barMargin = (barGroupWidth - barWidth * barCountInOneGroup) / (barCountInOneGroup - 1) + 20;
            }
            for (var i = 0; i < count; i++) {
                for (var j = 0; j < data.length; j++) {
                    var item = data[j][i];
                    if (item > 0) {
                        var a = that.config.chartHeight / max;
                        var x = that.config.left + columnWidth * i + groupMargin + j * (barWidth + barMargin);
                        drawBar(x, barWidth, a, item, j, i);
                    }
                }
                var xLabelX = that.config.left + columnWidth * i + groupMargin + (barWidth * data.length + barMargin * (data.length - 1)) / 2;
                var xLabelY = that.config.baseY + 20;
                var xLabelText = settings.xAxis[i];
                drawXLabel(xLabelX, xLabelY, xLabelText, columnWidth);
            }
        }
        function drawBar(x, width, a, item, groupIndex, barIndex) {
            var y;
            if (settings.stacked) {
                y = drawMultiBars(x, width, a, item, groupIndex, barIndex);
            } else {
                y = drawSingleBar(x, width, a, item, groupIndex, barIndex);
            }
            y = that.config.chartHeight - y + that.config.paddingTop - 5;
            drawBarLabel(x, width, y - 5, item, groupIndex, barIndex);
        }
        function drawSingleBar(x, width, a, item, groupIndex, barIndex) {
            var y = a * item;
            if (item > 0 && y < 2) {
                y = 2;
            }
            var rect = $$.rect(x, -y, width, y, 0, 0);
            var colors = settings.series[groupIndex].colors;
            var barStyle = (colors && colors[barIndex]) ? { fill: colors[barIndex] } : (settings.series[groupIndex].style || style.bar);
            $(rect).svgAttr(barStyle);
            rect.setAttribute("groupIndex", groupIndex);
            rect.setAttribute("barIndex", barIndex);
            $(rect).bind('mousemove', rect, function (e) {
                that.eventHandler.bar_mousemove(e);
            }).bind('mouseover', rect, function (e) {
                that.eventHandler.bar_mouseover(e);
            }).bind('mouseleave', rect, function (e) {
                that.eventHandler.bar_mouseout(e);
            });
            rect.onclick = that.eventHandler.bar_click;
            that.group.bar.appendChild(rect);
            return y;
        }
        function drawMultiBars(x, width, a, item, groupIndex, barIndex) {
            var g = $$.g();
            var stackedData = settings.series[groupIndex].stackedData[barIndex];
            var stacks = stackedData.stacks;
            var y = 0;
            for (var i = 0; i < stacks.length; i++) {
                var data = stacks[i].data;
                var barHeight = Math.round(data * a);
                if (data > 0 && barHeight < 2) {
                    barHeight = 2;
                }
                var rect = $$.rect(x, y, width, barHeight);
                $(rect).svgAttr(stacks[i].style || style.bar);
                rect.setAttribute("groupIndex", groupIndex);
                rect.setAttribute("barIndex", barIndex);
                rect.setAttribute("stackIndex", i);
                $(rect).bind('mousemove', rect, function (e) {
                    that.eventHandler.bar_mousemove(e);
                }).bind('mouseover', rect, function (e) {
                    that.eventHandler.bar_mouseover(e);
                }).bind('mouseleave', rect, function (e) {
                    that.eventHandler.bar_mouseout(e);
                });
                rect.onclick = that.eventHandler.bar_click;
                g.appendChild(rect);
                y += barHeight;
            }
            $(g).svgTranslate(0, -y);
            that.group.bar.appendChild(g);
            return y;
        }
        function drawBarLabel(x, barWidth, y, content, groupIndex, barIndex) {
            if (settings.valueTemplate && settings.valueTemplate != $.noop) {
                var data = {
                    label: settings.yAxis[barIndex],
                    data: settings.series[groupIndex].data[barIndex],
                    group: settings.series[groupIndex].name
                };
                content = settings.valueTemplate(data);
            }

            var text = $$.text(content, x + barWidth / 2, y, "middle");
            if ($$.isIE8()) {
                $(text).svgAttr({
                    x: x + 3 / 2,
                    width: barWidth - 3,
                    wrap: false
                });
            }
            $(text).svgAttr(style.barLabelCss);
            that.group.barLabel.appendChild(text);
            if ($.isArray(content)) {
                var tspans = [],
                    createTspan = function (t, y, isLight) {
                        var tspan = $$.create('tspan', {
                            x: x + barWidth / 2,
                            dy: y || 0,
                            textAnchor: "middle",
                            fill: isLight ? '#888' : '#222',
                            fontStyle: isLight ? 'italic' : 'normal',
                        });
                        tspan.textContent = t;
                        return tspan;
                    };
                for (var i = 0; i < content.length; i++) {
                    var ty = tspans.length > 0 ? 16 : 0,
                        tspan = createTspan(content[i], ty, i != 0);
                    tspans.push(tspan);
                }
                $(text).svgAttr({ y: y - (content.length - 1) * 12 });
                text.textContent = null;
                $(text).append(tspans);
            }

            //var maxWidth = barWidth + 20,
            //    createTspan = function (t, y) {
            //        var tspan = $$.create('tspan', {
            //            x: x + barWidth / 2,
            //            dy: y || 0,
            //            textAnchor: "middle"
            //        });
            //        tspan.textContent = t;
            //        return tspan;
            //    },
            //    trimming = $$.trimmingText(text, maxWidth);
            //if (trimming[1].length > 0) {
            //    var tspans = [], yy = 0;
            //    while (trimming[1].length > 0) {
            //        var ty = tspans.length > 0 ? 12 : 0,
            //            tspan = createTspan(trimming[0], ty);
            //        tspans.push(tspan);
            //        yy += 12;
            //        text.textContent = trimming[1];
            //        trimming = $$.trimmingText(text, maxWidth);
            //    }
            //    var ty = tspans.length > 0 ? 12 : 0,
            //        tspan = createTspan(trimming[0], ty);
            //    tspans.push(tspan);
            //    $(text).svgAttr({ y: y - yy });
            //    text.textContent = null;
            //    $(text).append(tspans);
            //}
        }
        function drawXLabel(x, y, content, columnWidth) {
            var maxWidth = columnWidth - 3,
                text = $$.text(content, x, y, "middle");
            $(text).svgAttr(style.LabelStyle);
            that.group.barLabel.appendChild(text);

            $$.trimmingSvgText(text, maxWidth);
            if ($$.isIE8()) {
                $(text).svgAttr({
                    x: x - maxWidth / 2
                });
            }

            text.setAttribute("fullText", content);
            $(text).bind('mousemove', text, function (e) {
                that.eventHandler.bar_mousemove(e);
            }).bind('mouseover', text, function (e) {
                that.eventHandler.bar_mouseover(e);
            }).bind('mouseleave', text, function (e) {
                that.eventHandler.bar_mouseout(e);
            });
        }

        function firstRender() {
            if ($$.isIE8()) {
                that.group.bar = $$.create('group', {
                    width: 100,
                    height: 100,
                    x: 0,
                    y: 0,
                    coordsize: '100,100'
                });
            }
            if (settings.animate && settings.animate.showBarLabelAfterAnimate) {
                $(that.group.barLabel).svgAttr({
                    display: 'none'
                });
            }
            $(that.group.bar).svgTranslate(0, that.config.chartHeight + that.config.paddingTop);

            //that.group.top.setAttribute("class", "chart-top-lines");
            that.group.grid.setAttribute("class", "chart-grid-lines");
            that.group.yLabel.setAttribute("class", "chart-y-labels");
            that.group.barOut.setAttribute("class", "chart-bars-out");
            that.group.bar.setAttribute("class", "chart-bars");
            that.group.barLabel.setAttribute("class", "chart-bar-label");
            that.group.pageArrow.setAttribute("class", "chart-page-arrow");
            $(that.group.pageArrow).svgAttr({
                position: 'static'
            });
            that.group.barG.setAttribute("class", "chart-barG");
            that.group.clipBarG.setAttribute("class", "chart-clipBarG");
            that.group.clipG.setAttribute("class", "chart-clipG");
            //that.stage.appendChild(that.group.top);
            that.stage.appendChild(that.group.grid);
            that.stage.appendChild(that.group.yLabel);
            that.group.barG.appendChild(that.group.barOut);
            that.group.barOut.appendChild(that.group.bar);
            that.group.barG.appendChild(that.group.barLabel);
            that.group.clipBarG.appendChild(that.group.barG);
            that.stage.appendChild(that.group.clipBarG);
            that.stage.appendChild(that.group.pageArrow);
            that.stage.appendChild(that.group.clipG);

            if (settings.startAnimate == true || (settings.startAnimate == null && settings.animate && settings.animate.startAnimate)) {
                setTimeout(beginAnimation, 0);
            }
        }
        function beginAnimation() {
            $(that.group.bar).svgAttr({
                display: 'none'
            });
            var barOut = that.group.barOut,
                group = barOut.firstChild;//group.bar
            //$(group).setScale(1, 0.01);
            $(group).svgAttr({
                display: 'block'
            });
            if (!Object.prototype.hasOwnProperty.call(barOut, "scale")) {
                Object.defineProperty(barOut, "scale", {
                    set: function (val) {
                        if ($$.isIE8()) {
                            //$(group).setScale(1, val);
                            $(group).svgAttr({
                                height: Math.max(100 * val, 1)
                            });
                        }
                        else {
                            $(group).setScale(1, val);
                        }
                    }
                });
            }
            $(barOut).animate({ scale: 1 }, 900, function () {
                $(that.group.barLabel).svgAttr({
                    display: 'block'
                });
            });
        }

        //unfinish todo
        that.resize = function (width) {
            var barOut = that.group.barOut,
                group = barOut.firstChild;

            var scale = width / that.config.width;
            that.config.width = width;
            $(that.stage).svgAttr({
                width: width
            });
            $(group).setScale(scale, 1);
        };

        that.init = function () {
            prepareData();
            initElement();
            firstRender();
            drawtopArea();
            drawTitle();
            drawClipG();
            drawHLinesAndYLabels();
            drawBarsAndXLabels();
            that.tooltip.initTooltip();
        };

        this.getData = function (series, index) {
            var value = 0;
            if (settings.stacked) {
                if (settings.series[series].stackedData.length > index) {
                    value = settings.series[series].stackedData[index].sum;
                }
            } else if (settings.property) {
                if (settings.series[series].data.length > index) {
                    value = settings.series[series].data[index][settings.property];
                }
            } else {
                if (settings.series[series].data.length > index) {
                    value = settings.series[series].data[index];
                }
            }
            return parseFloat(value);
        };

        this.eventHandler = {
            bar_click: function () {
                if (settings.clickCallback) {
                    var barIndex = this.getAttribute("barIndex");
                    var groupIndex = this.getAttribute("groupIndex");
                    var data = {
                        label: settings.xAxis[barIndex],
                        data: settings.series[groupIndex].data[barIndex]
                    };
                    if (settings.stacked) {
                        var stackIndex = this.getAttribute("stackIndex");
                        var stack = settings.series[groupIndex].stackedData[barIndex].stacks[stackIndex];
                        data.stackName = stack.name;
                        data.stackData = stack.data;
                    }
                    settings.clickCallback(data);
                }
            },
            bar_mouseover: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (!isLabel) {
                    $(rect).svgAttr({ cursor: "pointer" });
                }
                if (!isLabel) {
                    var barIndex = rect.getAttribute("barIndex");
                    var groupIndex = rect.getAttribute("groupIndex");
                    var data = {
                        label: settings.xAxis[barIndex],
                        data: settings.series[groupIndex].data[barIndex],
                        group: settings.series[groupIndex].name
                    };
                    var colors = settings.series[groupIndex].highLightColors;
                    var barStyle = (colors && colors[barIndex]) ? { fill: colors[barIndex] } : (settings.series[groupIndex].styleHighLight || style.barHighLight);
                    if (settings.stacked) {
                        var stackIndex = rect.getAttribute("stackIndex");
                        var stack = settings.series[groupIndex].stackedData[barIndex].stacks[stackIndex];
                        barStyle = stack.styleHighLight || style.barHighLight;
                        data.stackName = stack.name;
                        data.stackData = stack.data;
                    }
                    $(rect).svgAttr(barStyle);
                } else {
                    data = rect.getAttribute("fullText");
                }
                //var e = window.event || arguments[0];
                that.tooltip.createTooltip(e, data, isLabel);
            },
            bar_mousemove: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (that.tooltip.showTooltip) {
                    //var e = window.event || arguments[0];
                    that.tooltip.moveTooltip(e, isLabel);
                }
            },
            bar_mouseout: function (e) {
                var rect = e.data;
                var isLabel = rect.nodeName == "text" || rect.nodeName == "DIV";
                if (!isLabel) {
                    var barIndex = rect.getAttribute("barIndex");
                    var groupIndex = rect.getAttribute("groupIndex");
                    var colors = settings.series[groupIndex].colors;
                    var barStyle = (colors && colors[barIndex]) ? { fill: colors[barIndex] } : (settings.series[groupIndex].style || style.bar);
                    if (settings.stacked) {
                        var stackIndex = rect.getAttribute("stackIndex");
                        barStyle = settings.series[groupIndex].stacks[stackIndex].style || style.bar;
                    }
                    $(rect).svgAttr(barStyle);
                }
                that.tooltip.removeTooltip();
            },
            clickLeftArrow: function () {
                that.pager.pageIndex--;
                that.pager.refreshPageStatus();
                if (that.pager.pageIndex >= 0) {
                    var limit = -that.pager.pageIndex * (settings.gapWidth + settings.barWidth) * that.pager.pageSize - settings.gapWidth / 2;
                    if ($$.isIE8()) {
                        limit -= that.config.left;
                    }
                    $(that.group.barG).animate({ 'translateX': limit }, 900);
                }
            },
            clickRightArrow: function () {
                that.pager.pageIndex++;
                that.pager.refreshPageStatus();
                if (that.pager.pageIndex < that.pager.pageCount) {
                    var limit = -that.pager.pageIndex * (settings.gapWidth + settings.barWidth) * that.pager.pageSize - settings.gapWidth / 2;
                    if ($$.isIE8()) {
                        limit -= that.config.left;
                    }
                    $(that.group.barG).animate({ 'translateX': limit }, 900);
                }
            }
        };
    }

    function chartConfig() {
        this.chartId = '';
        this.paddingTop = 40;
        this.left = 50;
        this.right = 60;
        this.bottom = 30;
        this.chartWidth = 0;
        this.chartHeight = 0;
        this.baseY = 0;
        this.chartTopGap = 36;
    }

    function chartTooltip(settings, style, chart) {
        this.showTooltip = false;
        this.tooltipElement = null;
        this.tooltipElement_Label = null;
        this.isLabel = false;
        this.initTooltip = function () {
            this.tooltipElement = $('<span></span>');
            if (settings.tooltip && typeof settings.tooltip == 'object' && settings.tooltip.toolTipStyle != null) {
                if (typeof settings.tooltip.toolTipStyle == "string") {
                    this.tooltipElement.addClass(settings.tooltip.toolTipStyle);
                } else if (typeof settings.tooltip.toolTipStyle == "object") {
                    this.tooltipElement.css(settings.tooltip.toolTipStyle);
                }
            } else {
                this.tooltipElement.css(style.tooltipCss_Black);
            }
            this.tooltipElement.css({
                visibility: "hidden",
                position: "absolute"
            });
            chart.container.append(this.tooltipElement);

            this.tooltipElement_Label = $('<span></span>');
            this.tooltipElement_Label.css(style.lableTooltipCss);
            this.tooltipElement_Label.css("visibility", "hidden");
            chart.container.append(this.tooltipElement_Label);
        };
        this.createTooltip = function (e, data, isLabel) {
            this.showTooltip = true;
            this.isLabel = isLabel;
            var tip = isLabel ? this.tooltipElement_Label : this.tooltipElement;
            tip.empty();

            var content = '';
            if (this.isLabel) {
                content = data;
            } else {
                var template = settings.tooltip;
                if (settings.tooltip && typeof settings.tooltip == 'object' && settings.tooltip.toolTipTemplate) {
                    template = settings.tooltip.toolTipTemplate;
                }
                if (template && template != $.noop) {
                    content = template(data);
                }
                else {
                    //tooltip default value
                    content = data.label + ": ";
                    if (settings.stacked) {
                        content += data.stackData;
                    } else if (settings.property) {
                        content += data.data[settings.property];
                    } else {
                        content += data.data;
                    }
                }
            }
            tip.append(content);
            tip.css("visibility", "visible");
            this.moveTooltip(e, isLabel);
        };
        this.moveTooltip = function (e, isLabel) {

            var mouseX, mouseY;
            if (e.pageX) {
                var offset = chart.container.offset();
                var offsetX = offset.left;
                var offsetY = offset.top;
                mouseX = e.pageX - offsetX;
                mouseY = e.pageY - offsetY;
            }
            else {
                mouseX = e.offsetX;
                mouseY = e.offsetY;
            }
            var tip = isLabel ? this.tooltipElement_Label : this.tooltipElement;
            tip.css({ left: 0, top: 0, right: 'initial' });
            var mX = mouseX + 15,
                mY = mouseY,
                mR = 0,
                tipWidth = tip.width() + 23,
                tipHeight = tip.height() + 16;
            var maxX = chart.config.width - tipWidth - 3,
                maxY = settings.height - tipHeight - 3;
            if (maxX <= 0) {
                mR = 10;
            }
            else if (mX > maxX) {
                mX = mouseX - tipWidth - 8;
            }
            if (mY > maxY) {
                mY = mouseY - tipHeight + 20;
            }
            tip.css({
                left: mX + 'px',
                top: mY + 'px',
                right: mR ? mR + 'px' : 'initial'
            });
        };
        this.removeTooltip = function () {
            //this.tooltipElement.empty();
            this.tooltipElement.css("visibility", "hidden");
            //this.tooltipElement_Label.empty();
            this.tooltipElement_Label.css("visibility", "hidden");
            this.showTooltip = false;
        };
    }

    function chartPagerInfo(settings, style, chart) {
        var config = chart.config;
        var that = this;
        this.pageIndex = 0;
        this.pageCount = 1;
        this.pageSize = 0;
        var i;
        this.getPagedData = function () {
            var count = settings.xAxis ? settings.xAxis.length : 0;
            var data = [];
            for (i = 0; i < count; i++) {
                if (i == count) break;
                for (var j = 0; j < settings.series.length; j++) {
                    if (!data[j]) data[j] = [];
                    data[j].push(chart.getData(j, i));
                }
            }
            if (count > 0 && data.length == 0) {
                data[0] = [];
                for (i = 0; i < count; i++) {
                    data[0].push(0);
                }
            }
            return data;
        };
        this.refreshPageStatus = function () {
            chart.leftArrowImg.style.display = "block";
            chart.rightArrowImg.style.display = "block";
            if (this.pageIndex == 0) {
                chart.leftArrowImg.style.display = "none";
            }
            if (this.pageIndex == this.pageCount - 1) {
                chart.rightArrowImg.style.display = "none";
            }
        };
        function drawPageArrow() {
            if (that.pageCount <= 1) return;
            var y = config.chartHeight + config.paddingTop;
            var leftX = config.left - 11;
            var rightX = config.left + config.chartWidth + 9;

            var left, leftPath, right, rightPath;
            if (!$$.isIE8()) {
                leftPath = "M2 4 L-2 0 2 -4 M-9,0 A9,9 0,1,1 -9,0.01Z";
                rightPath = "M-2,-4 L2,0 -2,4 M-9,0 A9,9 0,1,1 -9,0.01Z";
            }
            else {
                leftPath = 'M2 4 L-2 0 2 -4E AL0 0 9 9 0 23592960';
                rightPath = 'M-2 -4 L2 0 -2 4E AL0 0 9 9 0 23592960';
            }
            left = chart.leftArrowImg = $$.svgPath(leftPath);
            $(left).svgAttr(style.pageArrowPath);
            $(left).svgTranslate(leftX, y);

            right = chart.rightArrowImg = $$.svgPath(rightPath);
            $(right).svgAttr(style.pageArrowPath);
            $(right).svgTranslate(rightX, y);

            chart.group.pageArrow.appendChild(left);
            chart.group.pageArrow.appendChild(right);
            left.onclick = chart.eventHandler.clickLeftArrow;
            right.onclick = chart.eventHandler.clickRightArrow;
            //给chart.group.barG添加translateX get\set属性，用来左右翻页滚动动画
            if (!Object.prototype.hasOwnProperty.call(chart.group.barG, "translateX")) {
                Object.defineProperty(chart.group.barG, "translateX", {
                    get: function () {
                        return this._translateX;
                    },
                    set: function (val) {
                        $(chart.group.barG).svgTranslate(val, null);
                        this._translateX = val;
                    }
                });
            }
            if (!$$.isIE8()) {
                chart.group.barG.translateX = -settings.gapWidth / 2;
            } else {
                chart.group.barG.translateX = -settings.gapWidth / 2 - config.left;
            }
        };
        this.init = function () {
            this.pageIndex = 0;
            this.pageSize = 0;
            this.pageCount = 1;
            if (!settings.xAxis || settings.xAxis.length == 0) {
                return;
            }
            var count = settings.xAxis.length;
            this.pageSize = count;

            var chartWidth = config.chartWidth;
            var barWidth = Math.floor(chartWidth / count);
            var minBarWidth = settings.gapWidth + settings.barWidth;
            if (barWidth < minBarWidth) {
                var size = this.pageSize = parseInt(chartWidth / minBarWidth, 10);
                //settings.gapWidth = chartWidth / size;
                settings.gapWidth += (chartWidth - minBarWidth * size) / size;
                if (settings.pageSize && settings.pageSize > 0) {
                    this.pageSize = Math.min(parseInt(settings.pageSize, 10), size);
                }
                this.pageCount = Math.ceil((count - size) / this.pageSize) + 1;
            }
            if (this.pageCount > 1) {
                drawPageArrow();
            }
        };
    }

    function chartYAxisInfo(settings, style, chart) {
        var max = 0;
        var min = 0;
        var pixelPerTick = 72;
        var interval = 0;
        function calMaxValue() {
            if (settings.yAxis.max > 0) {
                max = settings.yAxis.max;
                return;
            }
            var count = settings.series.length;
            for (var i = 0; i < count; i++) {
                for (var j = 0; j < settings.xAxis.length; j++) {
                    var data = chart.getData(i, j);
                    if (data > max) max = data;
                }
            }
        };
        function calInterval() {
            if (settings.yAxis.interval > 0) {
                interval = settings.yAxis.interval;
                return;
            }
            if (max == min) {
                if (max == 0) max = 1;
                else {
                    max = Math.ceil(max * 1.2);
                }
                interval = max;
                return;
            }
            var multiples = [1, 2, 2.5, 5, 10];
            var height = chart.config.chartHeight;
            interval = (max - min) * pixelPerTick / (height || 1);
            var magnitude = Math.pow(10, Math.floor(Math.log(interval) / Math.LN10));
            var normalized = interval / magnitude;
            for (var i = 0; i < multiples.length; i++) {
                interval = multiples[i];
                if (normalized <= (multiples[i] + (multiples[i + 1] || multiples[i])) / 2) {
                    break;
                }
            }
            interval *= magnitude;
            //if (interval < 1) interval = 1;
            if (Math.floor(interval) < interval) {
                interval = Math.floor(interval) + 1;
            }
            var total = 0;
            while (total < max) {
                total += interval;
            }
            max = total;
        };
        this.getYAxisInfo = function () {
            var gap = (max * (chart.config.chartTopGap / chart.config.chartHeight));
            return {
                maxx: max + gap,
                max: max,
                min: min,
                interval: interval
            };
        };
        this.init = function () {
            calMaxValue();
            calInterval();
        };
    }

    $.widget("aui.columnchart", {
        options: {
            width: 700,
            height: 400,
            left: 50,
            barWidth: 50,
            gapWidth: 20,
            tooltip: {
                toolTipTemplate: $.noop,
                toolTipStyle: null
            },
            valueTemplate: $.noop,
            xAxis: [],
            xAxisTitle: "Values",
            yAxis: {
                title: "Values",
                min: 0,
                max: 0,
                interval: 0,
                suffix: ''
            },
            stacked: false,
            property: "",
            //pageSize: undefined,
            series: [],
            clickCallback: $.noop,
            hiddenTop: false,
            animate: {
                startAnimate: true,
                showBarLabelAfterAnimate: false
            }
        },

        _chart: null,

        _create: function () {
            this._chart = new columnChart(this.options, this._style, this.element);
            this._chart.init();
        },
        destroy: function () {
            this.element.empty();
        },
        _setOption: function (key, value) {
            this.options[key] = value;
            this.destroy();
            this._create();
        },
        /**
         * 重画柱图。
         * @method draw
         * @param {Object} options 根据该配置信息重画柱图。
         */
        draw: function (options) {
            var self = this;
            if (options) {
                if (options.series) {
                    self.options.series = null;
                }
                if (options.xAxis) {
                    self.options.xAxis = null;
                }
                if (options.yAxis) {
                    self.options.yAxis = null;
                }
                //保持原来的部分属性
                $.extend(true, self.options, options);
            }
            self.destroy();
            self._create();
        },

        resize: function (width) {
            this._chart.resize(width);
        },

        _style: {
            containerCss: {
                overflow: "hidden",
                position: "relative"
            },
            yTitle: {
                fontFamily: 'Verdana',
                fill: "#222",
                fontSize: "12px;"
            },
            hLine: {
                fill: "none",
                stroke: "#999999",
                strokeWidth: 1
            },
            LabelStyle: {
                fill: "#222",
                fontFamily: 'Verdana',
                fontSize: "12px",
                cursor: "default"
            },
            barLabelCss: {
                fill: "#222",
                fontFamily: 'Verdana',
                //fontWeight: 'bold',
                fontSize: '12px'
            },
            bar: {
                fill: "#3a6506"
            },
            barHighLight: {
                fill: "#558536"
            },
            //tooltip默认样式
            tooltipCss_Black: {
                position: 'absolute',
                maxWidth: '160px',
                backgroundColor: '#333333',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#fff',
                //whiteSpace: 'nowrap',
                lineHeight: '18px',
                padding: '8px 15px 8px 8px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)'
            },
            lableTooltipCss: {
                position: "absolute",
                fontFamily: 'Verdana',
                fontSize: 12,
                backgroundColor: '#fff',
                border: '#999 1px solid',
                color: '#333333',
                padding: '2px 4px'
            },
            pageArrowPath: {
                fill: "#ffffff",
                stroke: "#999999",
                strokeWidth: 1
            },
            titleCss: {
                fontFamily: 'Verdana',
                fontSize: '12px',
                color: '#222'
            }
        }
    });

})(AUI, jQuery);


//facechart
(function ($$, $) {
    "use strict";
    $.widget("aui.facechart", {
        options: {
            width: 260,
            height: 150,
            min: 0,
            max: 0,
            value: 0,
            suffix: undefined,
            showfaces: true,
            showTopValue: false
        },

        _style: {
            faceStyle: { stroke: '#AAAAAA', fill: '#FFF' }
        },

        _params: function () {
            return {
                _constant: {
                    BARANDLINE_WIDTH: 260,// 线图默认整体宽度
                    BARANDLINE_HEIGHT: 150,// 线图默认整体高度
                    BARANDLINE_BOTTOMAXIS_HEIGHT: 30,// 线图默认下轴空间
                },
                _seriesMember: {
                    chartWidth: undefined,
                    chartHeight: undefined,
                    bottomHeight: undefined,
                    R: undefined,
                    innerR: undefined,
                    width: undefined
                },
                _template: {
                    root: undefined,
                    panelG: undefined,
                    pointer: undefined
                }
            };
        },

        _create: function () {
            var self = this;
            var parm = self._params();
            $.extend(self, parm);
            self._seriesMember.uuid = $$.generateUUIDByControlName("facechart");
            self._initLayout(true);
        },

        _setOption: function (key, value) {
            var self = this,
                options = self.options;
            if (key == 'value') {
                options[key] = value;
                self.changeValue(value);
            }
        },

        destroy: function () {
            this.element.off();
            this.element.remove();
        },

        changeValue: function (value) {
            var self = this,
                options = self.options,
                temp = self._template,
                pointer = temp.pointer,
                valueText = temp.valueText;
            if (pointer) {
                var angle = 0;
                if (options.max > options.min && value >= options.min && value <= options.max) {
                    angle = (value - options.min) / (options.max - options.min) * 180;
                }
                $(pointer).animate({ rotateAngle: angle }, 800);
                //this.attr("transform", '{rotate(-' + angle + ')}');
                //$(pointer).setRotate(-angle, 0, 0);
            }
            if (valueText) {
                value += (options.suffix || '');
                if (!$$.isIE8()) {
                    valueText.textContent = value;
                } else {
                    valueText.innerText = value;
                }
                //$(valueText).svgAttr({ textContent: value });
            }
        },

        //初始化调整控件的位置
        _initLayout: function () {
            var self = this,
                element = self.element,
                options = self.options,
                con = self._constant,
                sm = self._seriesMember,
                temp = self._template,
                chartWidth = sm.chartWidth = con.BARANDLINE_WIDTH,
                chartHeight = sm.chartHeight = con.BARANDLINE_HEIGHT,
                optionWidth = options.width || con.BARANDLINE_WIDTH,
                optionHeight = options.height || con.BARANDLINE_HEIGHT,
                bottomHeight = sm.bottomHeight = con.BARANDLINE_BOTTOMAXIS_HEIGHT,
                uuid = sm.uuid;

            element.css({
                overflow: 'hidden',
                position: 'relative'
            });

            if (!$$.isIE8()) {
                var defs = '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.1" ' +
                    'width="' + optionWidth + '" height="' + optionHeight + '" id="' + uuid + '">' +
                    '<defs><linearGradient id="defs1" x1="0%" y1="0%" x2="100%" y2="0%">' +
                    '<stop offset="0%" stop-color="#C92F31"/>' +
                    '<stop offset="50%" stop-color="#FFC934"/>' +
                    '<stop offset="100%" stop-color="#92D460"/>' +
                    '</linearGradient>' +
                    '<linearGradient id="defs2" x1="0%" y1="0%" x2="100%" y2="0%">' +
                    '<stop offset="0%" stop-color="#D75200"/>' +
                    '<stop offset="100%" stop-color="#FEA132"/>' +
                    '</linearGradient></defs></svg>';
                temp.root = $(defs);

            } else {
                self._initVmlCss();
                temp.root = $$.svg(uuid, {
                    width: optionWidth,
                    height: optionHeight
                });
            }
            element.append(temp.root);

            var scaleX = sm.scaleX = optionWidth / chartWidth;
            var scaleY = sm.scaleY = optionHeight / chartHeight;

            if (!$$.isIE8()) {
                temp.panelG = temp.vmlDiv = $$.g();
                $(temp.panelG).svgTranslate(optionWidth / 2, optionHeight - bottomHeight * scaleY);
                $(temp.panelG).setScale(scaleX, scaleY);
                $(temp.root).append(temp.panelG);
            } else {
                var div = temp.vmlDiv = $$.g();
                $(div).svgTranslate(optionWidth / 2, optionHeight - bottomHeight * scaleY);
                temp.panelG = $$.create('group', {
                    width: '100px',
                    height: '100px',
                    coordsize: (100 / scaleX) + ',' + (100 / scaleY)
                });
                $(div).append(temp.panelG);
                $(temp.root).append(div);
            }

            self._drawPie();
            self._drawPath();
            self._drawPointer();
            self._drawCircleText();
            if (options.showfaces) {
                self._drawFace();
            }
        },

        _drawPie: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                chartWidth = sm.chartWidth,
                width, innerR, mR;

            width = sm.width = 8;
            mR = sm.R = (chartWidth - 80) / 2;
            innerR = sm.innerR = mR - width;

            var y1 = 0,
                y2 = 5,
                x1 = mR - width,
                x2 = mR,
                x3 = -mR,
                x4 = -mR + width, d;
            if (!$$.isIE8()) {
                d = "M " + x1 + "," + y2 +
                    " L " + x2 + "," + y2 +
                    " L " + x2 + "," + y1 +
                    " A " + mR + "," + mR +
                    " 0 0 0 " +
                    x3 + "," + y1 +
                    " L " + x3 + "," + y2 +
                    " L " + x4 + "," + y2 +
                    " L " + x4 + "," + y1 +
                    " A " + innerR + "," + innerR +
                    " 0 0 1 " +
                    x1 + "," + y1 +
                    " L " + x1 + "," + y2 +
                    " Z ";
            }
            else {
                x1 = Math.round(x1);
                x2 = Math.round(x2);
                x3 = Math.round(x3);
                x4 = Math.round(x4);
                var b = 180 * Math.pow(2, 16);
                b = Math.round(b);
                d = "M " + x1 + "," + y2 +
                    " L " + x2 + "," + y2 +
                    " L " + x2 + "," + y1 +
                   " AE 0,0 " +
                   mR + "," + mR +
                   " 0," + b +
                    " L " + x3 + "," + y2 +
                    " L " + x4 + "," + y2 +
                    " L " + x4 + "," + y1 +
                   " AE 0,0 " +
                   innerR + "," + innerR +
                   " " + b + ",-" + b +
                   " E ";
            }
            var piepath = $$.svgPath(d);
            if ($$.isIE8()) {
                $(piepath).svgFill('#92D460');
                $(piepath).html('<v:fill type="gradient" colors="50% #FFC934,100% #C92F31" angle="90"/>');
            } else {
                $(piepath).svgFill('url(#defs1)');
            }

            $(piepath).svgAttr({
                strokeWidth: 0
            });
            $(temp.panelG).append(piepath);

        },

        _drawPath: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                innerR = sm.innerR, d;
            if (!$$.isIE8()) {
                d = 'M-' + innerR + ',5' +
                 ' Q-28,20 -25,5' +
                 ' L-25,0' +
                 ' A 25,25  0 1 1 25,0' +
                 ' L25,5' +
                 ' Q28,20 ' + innerR + ',5';
            } else {
                var b = 180 * Math.pow(2, 16);
                b = Math.round(b);
                d = 'M-25,5' +
                ' qb-28,20 -' + innerR + ',5 r 0 0 e' +
                ' M-25,5' + ' L-25,0' +
                ' AE 0,0 25,25 0' + b + ',-' + b +
                ' L25,5' +
                ' qb28,20 ' + innerR + ',5 r 0 0 e';
            }
            var path = $$.svgPath(d);
            $(path).svgAttr({ stroke: '#DFDFDF', fill: '#FFF' });
            $(temp.panelG).append(path);
        },

        _drawPointer: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template;
            var w1 = 8,
                w2 = 4,
                mR = sm.R - (options.showTopValue ? 30 : 20),
                angle = 0;
            if (options.max > options.min && options.value >= options.min && options.value <= options.max) {
                angle = (options.value - options.min) / (options.max - options.min) * 180;
            }
            var d;
            if (!$$.isIE8()) {
                d = ' M0,' + w1 / 2 +
                       ' L' + mR + ',' + w2 / 2 +
                       ' A ' + (w2 - 2) + ',' + (w2 - 2) +
                       ' 0 0 0 ' +
                       mR + ',-' + w2 / 2 +
                       ' L0,-' + w1 / 2 +
                       ' Z';
            } else {
                var b = 30 * Math.pow(2, 16),
                r = mR - w2;
                b = Math.round(b);
                r = Math.round(r);
                d = ' M0,' + w1 / 2 +
                     ' L' + mR + ',' + w2 / 2 +
                     ' AE ' + r + ',0 ' +
                     w2 + "," + w2 +
                     ' -' + b + ',' + (b * 2) +
                     ' L0,-' + w1 / 2 +
                     ' L0,' + w1 / 2 + ' E';
            }
            var pointer = $$.svgPath(d);
            if ($$.isIE8()) {
                $(pointer).svgAttr({
                    fill: '#D75200',
                    width: '1px',
                    height: '1px',
                    coordsize: '1,1'
                });
                $(pointer).html('<v:fill type="gradient" colors="100% #FEA132" angle="90"/>');
            } else {
                $(pointer).svgFill('url(#defs2)');
            }
            $(temp.panelG).append(pointer);

            temp.pointer = pointer;
            if (!Object.prototype.hasOwnProperty.call(pointer, 'rotateAngle')) {
                Object.defineProperty(pointer, 'rotateAngle', {
                    get: function () {
                        return parseInt(this._rotateAngle, 10);
                    },
                    set: function (val) {
                        var rotate = parseFloat(val).toFixed(2);
                        if (!$$.isIE8()) {
                            this.setAttribute('transform', 'rotate(-' + rotate + ')');
                        } else {
                            this.style.rotation = '-' + rotate + 'deg';
                        }
                        this._rotateAngle = rotate;
                    }
                });
            }

            setTimeout(function () {
                $(pointer).animate({ rotateAngle: angle }, 800);
            }, 1);
        },

        _drawCircleText: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template,
                width = sm.width,
                mR = sm.R;
            var range1 = $$.circle(0, 0, 15);
            $(range1).svgAttr({
                strokeWidth: 1,
                stroke: '#D1D1D1',
                fill: '#FBFBFB'
            });

            var range2 = $$.circle(0, 0, 7);
            $(range2).svgAttr({
                strokeWidth: 1,
                stroke: '#D1D1D1',
                fill: '#DFE0E2'
            });
            $(temp.panelG).append(range1);
            $(temp.panelG).append(range2);

            var line, lines = [], angle = 0;
            var count = 50;// options.max - options.min;
            for (var i = 0; i < count; i++) {
                var d = 'M ' + (mR - 15) + ',0 L ' + (mR - 10) + ',0';
                line = $$.svgPath(d);
                $(line).setRotate(-angle, 0, 0);
                $(line).svgAttr({
                    stroke: '#DFDFDF'
                });
                if ($$.isIE8()) {
                    $(line).svgAttr({
                        width: '1px',
                        height: '1px',
                        coordsize: '1,1'
                    });
                }
                lines.push(line);
                angle += 180 / count;
            }
            $(temp.panelG).append(lines);

            var leftText, rightText, middleText;
            var suffix = options.suffix || '',
                maxValue = options.max + suffix,
                minValue = options.min + suffix,
                value = options.value + suffix;
            if (!$$.isIE8()) {
                leftText = $$.text(maxValue, -mR + width / 2, 20, 'middle', 12);
                rightText = $$.text(minValue, mR - width / 2, 20, 'middle', 12);
                if (options.showTopValue) {
                    middleText = temp.valueText = $$.text(value, 0, -sm.innerR + 18, 'middle', 12);
                } else {
                    middleText = temp.valueText = $$.text(value, 0, -35, 'middle', 18);
                }
            } else {
                leftText = $$.text(maxValue, (-mR + width / 2) * sm.scaleX - 50, 20 * sm.scaleY, 'middle', 12 * sm.scaleX);
                $(leftText).svgAttr({ width: 100 });
                rightText = $$.text(minValue, (mR - width / 2) * sm.scaleX - 50, 20 * sm.scaleY, 'middle', 12 * sm.scaleX);
                $(rightText).svgAttr({ width: 100 });
                if (options.showTopValue) {
                    middleText = temp.valueText = $$.text(value, -50, (-sm.innerR + 20) * sm.scaleY, 'middle', 12 * sm.scaleX);
                } else {
                    middleText = temp.valueText = $$.text(value, -50, -35 * sm.scaleY, 'middle', 18 * sm.scaleX);
                }
                $(middleText).svgAttr({ width: 100 });
            }
            $(temp.vmlDiv).append(leftText);
            $(temp.vmlDiv).append(rightText);
            $(temp.vmlDiv).append(middleText);
        },

        _drawFace: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                mR = sm.R;
            var r = 10,
                angle, x, y,
                d, face, faces, g, filled = false;
            for (var i = 0; i < 5; i++) {
                g = $$.g();
                if ($$.isIE8()) {
                    g = $$.create('group', {
                        width: '1px',
                        height: '1px',
                        coordsize: '1,1'
                    });
                }
                angle = i * Math.PI / 4;
                x = (mR + 15) * Math.cos(angle);
                y = (mR + 15) * Math.sin(angle);
                if (i == 0 || i == 4) { y += 5; }
                $(g).svgTranslate(x, -y);
                faces = [];
                //圆脸
                if (!$$.isIE8()) {
                    d = "M " + (r + 1) + "," + 0 +
                        " A " + r + "," + r +
                        " 0 1 1 " +
                        (r + 1) + ",-" + 0.001 +
                        " Z ";
                } else {
                    d = 'AE0,0 ' + r + ',' + r + ' 0,23592960 E';
                }
                face = $$.svgPath(d);
                $(face).svgAttr(self._style.faceStyle);
                faces.push(face);

                //双眼
                if (!$$.isIE8()) {
                    d = "M-4.5,-3 A1.5,1.5 0 1 1 -4.5,-2 Z" +
                        "M4,-3 A1.5,1.5 0 1 1 4,-2 Z";
                } else {
                    d = 'AE-4,-2 2,2 0,23592960 ' +
                        'M 5,-2 AE5,-2 2,2 0,23592960 E';
                }
                face = $$.svgPath(d);
                $(face).svgFill(self._style.faceStyle.stroke);
                faces.push(face);

                //嘴\眉毛
                d = null;
                filled = false;
                switch (i) {
                    case 0:
                        if (!$$.isIE8()) {
                            d = "M-4,4 C-3,9 5,9 6,4 L-4,4 ";
                        } else {
                            d = "M-4,4 c-3,9 4,9 5,4 r 0 0 e L-4,4 ";
                        }
                        filled = true;
                        break;
                    case 1:
                        if (!$$.isIE8()) {
                            d = "M-4,4 C-3,6 5,6 6,4";
                        } else {
                            d = "M-5,4 c-4,6 4,6 5,4 r 0 0 e";
                        }
                        break;
                    case 2:
                        if (!$$.isIE8()) {
                            d = "M-3,4 L5,4 ";
                        } else {
                            d = "M-4,4 L4,4 ";
                        }
                        break;
                    case 3:
                        if (!$$.isIE8()) {
                            d = "M-4,4 C-3,2 5,2 6,4";
                        } else {
                            d = "M-5,4 c-4,2 4,2 5,4 r 0 0 e";
                        }
                        break;
                    case 4:
                        if (!$$.isIE8()) {
                            d = "M-4,4 C-3,2 5,2 6,4" +
                                "M-0,-4 L-3,-7" +
                                "M2,-4 L5,-7";
                        } else {
                            d = "M-5,4 c-4,2 4,2 5,4 r 0 0 e" +
                                "M-2,-4 L-5,-7" +
                                "M2,-4 L5,-7";
                        }
                        break;
                    default:
                        break;
                }
                if (d) {
                    face = $$.svgPath(d);
                    if (filled) {
                        $(face).svgFill(self._style.faceStyle.stroke);
                    } else {
                        $(face).svgAttr(self._style.faceStyle);
                    }
                    faces.push(face);
                }
                $(g).append(faces);
                $(temp.panelG).append(g);
            }
        },

        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
    });
})(AUI, jQuery);


//halfpiechart
(function ($$, $) {
    "use strict";
    $.widget("aui.halfpiechart", {
        options: {
            width: 260,
            height: 150,
            min: 0,
            max: 0,
            value: 0,
            r: undefined,
            innerR: undefined,
            pieStyle: {
                strokeWidth: 1,
                stroke: 'red',
                fill: 'red'
            },
            backgroundStyle: {
                strokeWidth: 1,
                stroke: '#DBD9D9',
                fill: '#E9E7E7'
            },
            valueTextSize: 25,
            minmaxTextSize: 12
        },

        _style: {
        },

        _params: function () {
            return {
                _constant: {
                    DEFAULT_WIDTH: 260,// 默认整体宽度
                    DEFAULT_HEIGHT: 150,// 默认整体高度
                    DEFAULT_BOTTOMAXIS_HEIGHT: 30,// 默认下轴空间
                    DEFAULT_R_WIDTH: 10// 默认环形宽度
                },
                _seriesMember: {
                    bottomHeight: undefined,
                    R: undefined,
                    r: undefined,
                    width: undefined,
                    uuid: undefined
                },
                _template: {
                    root: undefined,
                    panelGG: undefined,
                    panelG: undefined,
                    clipG: undefined,
                    contentG: undefined,
                    valuePath: undefined
                }
            };
        },

        _create: function () {
            var self = this;
            var parm = self._params();
            $.extend(self, parm);
            self._seriesMember.uuid = $$.generateUUIDByControlName("halfpiechart");
            self._initLayout(true);
        },

        _setOption: function (key, value) {
            var self = this,
                options = self.options;
            if (key == 'value') {
                options[key] = value;
                self.changeValue(value);
            }
        },

        destroy: function () {
            this.element.off();
            this.element.remove();
        },

        changeValue: function (value) {
            this._changeValue(value);
        },

        //初始化调整控件的位置
        _initLayout: function () {
            var self = this,
                element = self.element,
                options = self.options,
                con = self._constant,
                sm = self._seriesMember,
                temp = self._template,
                width = options.width || con.DEFAULT_WIDTH,
                height = options.height || con.DEFAULT_HEIGHT,
                bottomHeight = sm.bottomHeight = con.DEFAULT_BOTTOMAXIS_HEIGHT,
                uuid = sm.uuid;

            element.css({
                overflow: 'hidden',
                position: 'relative'
            });

            self._initVmlCss();
            temp.root = $$.svg(uuid, {
                width: width,
                height: height
            });
            element.append(temp.root);

            var pieBottomHeight = sm.pieBottomHeight = height - bottomHeight;
            temp.panelGG = $$.g();
            temp.contentG = $$.g();
            temp.panelG = $$.g();
            $(temp.panelG).svgTranslate(width / 2, pieBottomHeight);
            $(temp.contentG).svgTranslate(width / 2, pieBottomHeight);
            $(temp.panelGG).append(temp.panelG);
            $(temp.root).append(temp.contentG);
            $(temp.root).append(temp.panelGG);
            temp.clipG = $$.g();
            $(temp.root).append(temp.clipG);

            self._setClipPath();
            self._drawPie();
            self._drawCircleText();
        },

        _setClipPath: function () {
            var self = this,
                temp = self._template,
                options = self.options,
                sm = self._seriesMember;
            if (!$$.isIE8()) {
                var uuid = sm.uuid,
                    clipId = uuid + "_Clip",
                    clipPath = $$.create("clipPath");
                clipPath.setAttribute("id", clipId);
                var points = '0 0 ' + options.width + ' 0 '
                        + options.width + ' ' + sm.pieBottomHeight + ' '
                        + '0 ' + sm.pieBottomHeight;
                var polyline = $$.create("polyline", {
                    points: points
                });
                $(temp.clipG).append(clipPath);
                $(clipPath).append(polyline);
                temp.panelGG.setAttribute("clip-path", "url(#" + clipId + ")");
            }
            else {
                $(temp.panelGG).svgAttr({
                    width: options.width,
                    height: sm.pieBottomHeight,
                    overflow: 'hidden'
                });
            }
        },

        _drawPie: function () {
            var self = this,
                options = self.options,
                temp = self._template;

            var piepath = self._drawPieByRangle(1);
            $(piepath).svgAttr(options.backgroundStyle);
            $(temp.panelG).append(piepath);

            var angle = 0;
            if (options.max > options.min && options.value >= options.min && options.value <= options.max) {
                angle = (options.value - options.min) / (options.max - options.min);
            }
            var valuePath = temp.valuePath = self._drawPieByRangle(1);
            if ($$.isIE8()) {
                $(valuePath).svgAttr({
                    width: 1,
                    height: 1,
                    coordsize: '1,1'
                });
            }
            if (!Object.prototype.hasOwnProperty.call(piepath, "rotateAngle")) {
                Object.defineProperty(valuePath, "rotateAngle", {
                    get: function () {
                        return parseInt(this._rotateAngle, 10);
                    },
                    set: function (val) {
                        var value = parseFloat(val).toFixed(2);
                        if (!$$.isIE8()) {
                            this.setAttribute('transform', 'rotate(' + value + ')');
                        } else {
                            this.style.rotation = value + 'deg';
                        }
                        this._rotateAngle = value;
                    }
                });
            }
            $(valuePath).prop({
                rotateAngle: -180
            });
            $(valuePath).animate({ rotateAngle: angle * 180 - 180 }, 800);
            $(valuePath).svgAttr(options.pieStyle);
            $(temp.panelG).append(valuePath);
        },

        _changeValue: function (value) {
            var self = this,
                options = self.options,
                temp = self._template,
                valuePath = temp.valuePath,
                valueText = temp.valueText;
            if (valuePath) {
                var angle = 0;
                if (options.max > options.min && value >= options.min && value <= options.max) {
                    angle = (value - options.min) / (options.max - options.min);
                }
                $(valuePath).animate({ rotateAngle: angle * 180 - 180 }, 800);
            }
            if (valueText) {
                if (!$$.isIE8()) {
                    valueText.textContent = value;
                } else {
                    valueText.innerText = value;
                }
            }
        },

        _drawPieByRangle: function (angle) {
            var self = this,
                options = self.options,
                con = self._constant,
                sm = self._seriesMember,
                chartWidth = options.width,
                width, innerR, mR;

            mR = sm.R = options.r || Math.min(sm.pieBottomHeight - 20, (chartWidth - 20) / 2);
            innerR = sm.innerR = options.innerR || mR - con.DEFAULT_R_WIDTH;
            width = sm.width = mR - innerR;

            angle = angle * Math.PI;
            var y1 = -innerR * Math.sin(angle),
                x1 = -innerR * Math.cos(angle),
                y2 = -mR * Math.sin(angle),
                x2 = -mR * Math.cos(angle),
                y = 0,
                x3 = -mR,
                x4 = -innerR, d;
            if (!$$.isIE8()) {
                d = "M " + x1 + "," + y1 +
                    " L " + x2 + "," + y2 +
                    " A " + mR + "," + mR +
                    " 0 0 0 " +
                    x3 + "," + y +
                    " L " + x3 + "," + y +
                    " L " + x4 + "," + y +
                    " A " + innerR + "," + innerR +
                    " 0 0 1 " +
                    x1 + "," + y1 +
                    " Z ";
            }
            else {
                y1 = Math.round(y1);
                x1 = Math.round(x1);
                y2 = Math.round(y2);
                x2 = Math.round(x2);
                x3 = Math.round(x3);
                x4 = Math.round(x4);
                var b = (angle * 180 / Math.PI) * Math.pow(2, 16),
                    a = 180 * Math.pow(2, 16);
                a = Math.round(a);
                b = Math.round(b);
                d = "M " + x1 + "," + y1 +
                    " L " + x2 + "," + y2 +
                   " AE 0,0 " +
                   mR + "," + mR +
                   " " + (a - b) + "," + b +
                    " L " + x3 + "," + y +
                    " L " + x4 + "," + y +
                   " AE 0,0 " +
                   innerR + "," + innerR +
                   " " + a + ",-" + b +
                   " E ";
            }
            return $$.svgPath(d);
        },

        _drawCircleText: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template,
                width = sm.width,
                mR = sm.R;
            var leftText, rightText, middleText;
            leftText = $$.text(options.min, -mR + width / 2, 20, 'middle', options.minmaxTextSize);
            rightText = $$.text(options.max, mR - width / 2, 20, 'middle', options.minmaxTextSize);
            middleText = temp.valueText = $$.text(options.value, 0, -10, 'middle', options.valueTextSize);
            if ($$.isIE8()) {
                $(leftText).svgAttr({
                    x: (-mR + width / 2) - 50,
                    width: 100
                });
                $(rightText).svgAttr({
                    x: mR - width / 2 - 50,
                    width: 100
                });
                $(middleText).svgAttr({
                    x: -50,
                    width: 100
                });
            }
            $(temp.contentG).append(leftText);
            $(temp.contentG).append(rightText);
            $(temp.contentG).append(middleText);
        },

        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
    });
})(AUI, jQuery);


//linechart
(function ($$, $) {
    "use strict";
    $.widget("aui.linechart", {
        options: {
            width: 600,
            height: 350,
            leftAxisWidth: 50,
            bottomAxisHeight: 30,
            series: [],
            xAxis: [],
            yAxis: {
                title: "Values",
                min: 0,
                max: 0,
                interval: 0,
                yAxisList: []
            },
            tooltip: {
                toolTipTemplate: $.noop,
                isToolTipCollapsedAfterClick: true,
                useDefaultStyle: true,
                toolTipStyle: null
            },
            onPointClick: $.noop,
            hasShade: false,
            isMultiPointSytle: false,
            pointStyle: 'ring',
            multiPointSytleList: ['ring', 'circle', 'X-type', 'triangle', 'Rhombus'], //isMultiPointSytle=true时，多样循环显示的样式集合，类型参考_drawPointStyle方法
            backgroundMetadatas: [],
            chartTitle: undefined,
            showAverage: false,
            averageFormat: "Average: {0}",
            minGapWidth: 80,
            isNoFirstGap: false,
            showLine: {
                showX: true,
                showY: true,
                showXline: true,
                showYline: true
            },
            topArea: {
                hasTopArea: true,
                weeklySeries: [],
                weeklyxAxis: [],
                monthlySeries: [],
                monthlyxAxis: [],
                onTopAreaClick: $.noop,
            },
            pager: {
                usePager: false,
                pageSize: 0,
                endWithLastPoint: false
            }
        },

        _style: {
            //label css
            labelCss: {
                fontFamily: "Verdana",
                fill: "#222",
                fontSize: '12px',
                cursor: "default"
            },
            //线图箭头及网格线样式
            lineCss: {
                stroke: "#999999",
                strokeWidth: 1
            },
            //线图边框样式
            outlineCss: {
                stroke: "#999999",
                strokeWidth: 1
            },
            //label tooltip样式
            TooltipStyle_Label: {
                position: 'absolute',
                fontFamily: 'Verdana',
                fontSize: 12,
                backgroundColor: '#fff',
                border: '#999 1px solid',
                color: '#222',
                padding: '2px 4px'
            },
            TooltipStyle_White: {
                position: 'absolute',
                fontFamily: 'Verdana',
                fontSize: 12,
                backgroundColor: '#fff',
                border: '#999 1px solid',
                color: '#333333',
                padding: '2px 1px 2px 1px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)'
            },
            //tooltip默认样式
            TooltipStyle_Black: {
                position: 'absolute',
                maxWidth: '160px',
                backgroundColor: '#333333',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#fff',
                //whiteSpace: 'nowrap',
                lineHeight: '18px',
                padding: '8px 15px 8px 8px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)'
            },
            //average 平均线样式
            averageLineStyle: {
                stroke: "#FF9900",
                strokeDasharray: "1,1",
                strokeWidth: 1
            }
        },

        _create: function () {
            var self = this;
            var params = {
                _constant: {
                    BARANDLINE_HEIGHT: 350,// 线图默认整体高度
                    BARANDLINE_WIDTH: 600,// 线图默认整体宽度
                    BARANDLINE_LEFTAXIS_WIDTH: 50,// 线图默认左侧轴空间
                    BARANDLINE_BOTTOMAXIS_HEIGHT: 30,// 线图默认下轴空间
                    BARANDLINE_RIGHTAXIS_WIDTH: 30,// 线图右侧轴空间
                    BARANDLINE_TOPAXIS_HEIGHT: 70,// 线图上侧轴空间
                    BARANDLINE_TOPAXIS_HEIGHT_NOTOPAVE: 35,// 线图上侧轴空间(options.hasTopArea = false)
                    ColorStorage: ["#997700", "#CC00FF", "#FF6000", "#0099AA", "#00AA00", "#0011BB"],// 线图默认线的颜色容器
                    BackgroundColorStorage: ["#DDFFDD", "#FFE3CC", "#FFDDDD"],// 线图背景色的颜色容器
                    DefaultMinGap: 1,
                    DEFAULT_LOAD_SIZE: 10
                },
                _seriesMember: {
                    chartWidth: undefined,
                    chartHeight: undefined,
                    leftWidth: undefined,
                    bottomHeight: undefined,
                    rightWidth: undefined,
                    topHeight: undefined,
                    seriesWidth: undefined,
                    seriesHeight: undefined
                },
                _template: {
                    root: undefined,
                    panelG: undefined,
                    arrowG: undefined,
                    contentG: undefined,
                    lineG: undefined,
                    labelG: undefined,
                    topG: undefined,
                    scrollG: undefined,
                    scrollG2: undefined,
                    clipG: undefined,
                    //toolTipG: undefined,
                    leftCircle: undefined,
                    rightCircle: undefined,
                    toolTipDiv: undefined,
                    foucsPoint: undefined
                },
                _parameter: {
                    lableGap: 0,
                    pageSize: 0,
                    pageCount: 0,
                    pageIndex: 0,
                    uuid: undefined,
                    labelList: undefined,
                    scales: undefined,
                    metadatePX: undefined,
                    series: undefined,
                    moveTopPoint: undefined,
                    timer: undefined,
                    isBackground: false,
                    hasScroll: false,
                    isScrollPage: false,
                    selectedArea: "daily",
                    loadSize: 0,
                    scale: 1,
                    labelGLeft: 0
                }
            };
            $.extend(self, params);
            self._parameter.uuid = $$.generateUUIDByControlName("linechart");
            self._initLayout(true);
        },

        _setOption: function (key, value) {
            var self = this,
                options = self.options;
            options[key] = value;
        },

        /**
         * 画线图。
         * @method draw
         * @param {Object} options 若没值，则根据原来的配置信息画线图；若有值，则根据新的配置信息重画线图。
         * @example
         $(element).linechart("draw"，{});
         */
        draw: function (options) {
            var self = this;
            //保持原来的部分属性
            if (options) {
                if (options.series) {
                    self.options.series = null;
                }
                if (options.xAxis) {
                    self.options.xAxis = null;
                }
                if (options.backgroundMetadatas) {
                    self.options.backgroundMetadatas = null;
                }
                $.extend(true, self.options, options);
            }
            self._draw();
        },

        /**
         * 清除线图内容，只保留外边框。
         * @method clear
         * @example
         $(element).linechart("clear");
         */
        clear: function () {
            var self = this;
            self._clear();
            self._initLayout(false);
        },

        destroy: function () {
            var self = this;
            self._clear();
        },

        /**
         * 根据options.width与参数width的比例动态缩放线图比例，在window.onresize中线图高宽动态改变时调用。
         * @method resize
         * @param {Number} width 缩放到的宽度。
         * @example
         $(element).linechart("resize"，400);
         */
        resize: function (width) {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                parameter = self._parameter;

            var scale = parameter.scale = sm.chartWidth / width;
            $(temp.root).svgAttr({
                width: sm.chartWidth / scale,
                height: sm.chartHeight / scale
            });

            self.element.width(sm.chartWidth / scale + 5);
            self.element.height(sm.chartHeight / scale + 5);

            $(temp.root).find('line[stroke-dasharray]').css({
                strokeDasharray: scale + ',' + scale
            });
            $(temp.root).find('line[stroke-width]').css({
                strokeWidth: scale
            });
            var fontSize = scale <= 1 ? 12 : Math.round(12 / scale);
            $(temp.root).find('text').css({
                fontSize: fontSize
            });

            $(temp.toolTipDiv).css({
                //fontSize: 5
            });

        },

        _draw: function () {
            var self = this;
            self._clear();
            self._initLayout(true);
            self._setGapPage();
            self._initLabels();
            self._initScales();
            self._generateYScaleAxis();
            var isInclinedLabel = self._generateXScaleAxis();
            self._setClipPath(isInclinedLabel);
            self._setPointList();
            self._drawLine();
            self._drawAverage();
            self._setLineAnimate();
            self._initTip();
        },

        _clear: function (keepTop) {
            var self = this,
                temp = self._template,
                parameter = self._parameter;
            if (temp.contentG) {
                $(temp.contentG).stop(true, false);
            }
            if (temp.startAnimateE) {
                $(temp.startAnimateE).stop(true, false);
            }
            temp.contentG = undefined;
            temp.startAnimateE = undefined;

            if (!keepTop) {
                if (temp.root) {
                    $(temp.root).remove();
                }
                parameter.selectedArea = 'daily';
            }
            else {
                $(temp.arrowG).remove();
                $(temp.scrollG).remove();
                $(temp.clipG).remove();
            }
            if (temp.toolTipDiv) {
                temp.toolTipDiv.remove();
                temp.toolTipDiv = undefined;
            }
            $(self.element).find('span').remove();
            parameter.series = undefined;
            parameter.labelList = undefined;
            parameter.scales = undefined;
            parameter.lableGap = 0;
            parameter.pageSize = 0;
            parameter.pageCount = 0;
            parameter.pageIndex = 0;
            parameter.loadSize = 0;
        },

        //初始化调整线图的位置
        _initLayout: function (drawTop) {
            var self = this,
                element = self.element,
                options = self.options,
                con = self._constant,
                sm = self._seriesMember,
                temp = self._template,
                parameter = self._parameter,
                chartWidth = sm.chartWidth = options.width || con.BARANDLINE_WIDTH,
                chartHeight = sm.chartHeight = options.height || con.BARANDLINE_HEIGHT,
                leftWidth = sm.leftWidth = options.leftAxisWidth || con.BARANDLINE_LEFTAXIS_WIDTH,
                bottomHeight = sm.bottomHeight = options.bottomAxisHeight || con.BARANDLINE_BOTTOMAXIS_HEIGHT,
                rightWidth = sm.rightWidth = con.BARANDLINE_RIGHTAXIS_WIDTH,
                topHeight = sm.topHeight = options.topArea.hasTopArea ? con.BARANDLINE_TOPAXIS_HEIGHT : con.BARANDLINE_TOPAXIS_HEIGHT_NOTOPAVE,
                uuid = parameter.uuid;

            sm.seriesWidth = chartWidth - leftWidth - rightWidth;
            sm.seriesHeight = chartHeight - bottomHeight - topHeight;
            element.css({ position: 'relative' });
            if (!options.showAverage) {
                element.css({ overflow: 'hidden' });
            }
            element.width(chartWidth + 5);
            element.height(chartHeight + 5);

            self._initVmlCss();

            temp.root = $$.svg(uuid, {
                width: chartWidth,
                height: chartHeight
            });
            element.append(temp.root);

            // set root view box
            var scaleX = chartWidth,
                scaleY = chartHeight,
                viewBox = 0 + " " + 0 + " " + scaleX + " " + scaleY;
            $(temp.root).svgAttr({
                viewBox: viewBox
            });

            temp.panelG = $$.g();
            $(temp.panelG).svgTranslate(sm.leftWidth, sm.chartHeight - sm.bottomHeight);
            $(temp.root).append(temp.panelG);

            self._drawRect();
            if (drawTop && options.topArea.hasTopArea) {
                self._drawTopG();
            }
        },

        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        },

        //画线图外边框
        _drawRect: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                items = [], line;
            var zero = self._style.outlineCss.strokeWidth == 1 ? 0.5 : 0;
            if (self.options.showLine.showX) {
                line = $$.line(-10, zero, sm.seriesWidth + 10, zero);
                $(line).svgAttr(self._style.outlineCss);
                items.push(line);
            }
            if (self.options.showLine.showY) {
                line = $$.line(zero, 0, zero, -sm.seriesHeight - 5);
                $(line).svgAttr(self._style.outlineCss);
                items.push(line);
            }
            $(temp.panelG).append(items);
        },

        //初始化svg元素
        _setGapPage: function () {
            var self = this,
                con = self._constant,
                sm = self._seriesMember,
                temp = self._template,
                parameter = self._parameter,
                count = parameter.pageSize = self._getOptionsxAxis().length;
            parameter.lableGap = Math.max(0, self.options.minGapWidth);
            parameter.hasScroll = false;
            parameter.pageCount = 0;
            parameter.pageIndex = 0;

            if (self.options.pager && self.options.pager.usePager) {
                var pager = self.options.pager;
                if ($.isNumeric(pager.pageSize)) {
                    parameter.pageSize = pager.pageSize <= 0 ? count : Math.min(count, pager.pageSize);
                    parameter.hasScroll = parameter.pageSize < count;
                }
                else if ($.isArray(pager.pageSize) && pager.pageSize.length > 0) {
                    var list = pager.pageSize,
                        sizeLength = list.length;
                    if (sizeLength == 1) {
                        parameter.pageSize = list[0] <= 0 ? count : Math.min(count, list[0]);
                        parameter.hasScroll = parameter.pageSize < count;
                    } else {
                        parameter.isScrollPage = true;
                        parameter.hasScroll = true;
                        parameter.pageSizeList = [];
                        var pIndex = 0, maxSize = list[0];
                        for (var i = 0; i < sizeLength; i++) {
                            var size = list[i];
                            size = Math.min(list[i], maxSize);
                            pIndex += size;
                            if (pIndex >= count) {
                                if (i == 0) {
                                    parameter.isScrollPage = false;
                                    parameter.hasScroll = false;
                                } else {
                                    if (self.options.pager.endWithLastPoint) {
                                        size = size - (pIndex - count);
                                    }
                                    parameter.pageSizeList.push(size);
                                }
                                break;
                            }
                            else if (i == sizeLength - 1) {
                                i--;
                            }
                            parameter.pageSizeList.push(size);
                        }
                    }
                }
            }
            else if (count > 0) {
                if (count * parameter.lableGap > sm.seriesWidth) {
                    parameter.hasScroll = true;
                    parameter.pageSize = parseInt(sm.seriesWidth / parameter.lableGap, 10);
                } else {
                    parameter.hasScroll = false;
                }
            }
            if (parameter.isScrollPage) {
                parameter.pageSize = parameter.pageSizeList[0];
                parameter.lableGap = sm.seriesWidth / parameter.pageSize;
                parameter.pageCount = parameter.pageSizeList.length - 1;
            } else {
                if (parameter.hasScroll) {
                    var size = parameter.pageSize;
                    parameter.lableGap = sm.seriesWidth / size;
                    parameter.pageCount = (count % size) == 0 ? parseInt(count / size, 10) - 1 : parseInt(count / size, 10);
                } else {
                    var fristGap = self.options.isNoFirstGap ? 0 : parameter.lableGap / 2;
                    if (!self.options.isNoFirstGap) {
                        parameter.lableGap = (sm.seriesWidth) / count;
                    } else {
                        parameter.lableGap = (sm.seriesWidth - fristGap - 10) / (count - 1);
                    }
                }
            }
            parameter.loadSize = 0;
            if (parameter.hasScroll) {
                parameter.loadSize = Math.max(con.DEFAULT_LOAD_SIZE, parameter.pageSize + 2);
                parameter.labelGLeft = 0;
            } else {
                parameter.labelGLeft = Math.max(0, sm.leftWidth - 10);
            }

            var left = parameter.lableGap - (!parameter.hasScroll && self.options.isNoFirstGap ? 0 : parameter.lableGap / 2),
                left2 = parameter.lableGap * 3 / 4;

            temp.arrowG = $$.g();
            temp.scrollG = $$.g();
            temp.scrollG2 = $$.g();
            temp.contentG = $$.g();
            temp.lineOutG = $$.g();
            temp.lineG = $$.g();
            temp.labelG = $$.g();
            temp.clipG = $$.g();
            $(temp.arrowG).svgTranslate(sm.leftWidth, sm.chartHeight - sm.bottomHeight);
            $(temp.scrollG).svgTranslate(sm.leftWidth, sm.topHeight - 10);
            $(temp.scrollG2).svgTranslate(-left, sm.seriesHeight + 10);
            $(temp.lineOutG).svgTranslate(left2, -sm.seriesHeight - 10);
            $(temp.lineG).svgTranslate(-left2, sm.seriesHeight + 10);
            $(temp.labelG).svgTranslate(-parameter.labelGLeft, 0);
            $(temp.root).append(temp.arrowG);
            $(temp.root).append(temp.scrollG);
            $(temp.scrollG).append(temp.scrollG2);
            $(temp.scrollG2).append(temp.contentG);
            $(temp.contentG).append(temp.labelG);
            $(temp.contentG).append(temp.lineOutG);
            $(temp.lineOutG).append(temp.lineG);
            $(temp.root).append(temp.clipG);

            if (parameter.hasScroll) {
                self._drawArrows();
            }
        },

        //画线图上方title date
        _drawTopG: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template,
                parameter = self._parameter,
                lines = [],
                texts = [],
                rects = [],
                line, rect,
                left = sm.chartWidth - 10;

            var fun = function (x, width, text, isLeft) {
                line = $$.line(x, 0.5, x + width, 0.5);
                lines.push(line);
                if (text) {
                    var textNode,
                        fontSize = self._style.labelCss.fontSize;
                    fontSize = isLeft ? 20 : fontSize ? parseInt(fontSize, 10) : 12;
                    if (!$$.isIE8()) {
                        textNode = $$.text(text, isLeft ? 10 : x + width / 2, -6, isLeft ? "start" : "middle", fontSize);
                        texts.push(textNode);
                        rect = $$.rect(x, -20, width, 20);
                        $(rect).svgFill("transparent");
                        rects.push(rect);
                    }
                    else {
                        textNode = $$.text(text, x, isLeft ? -5 : -8, "middle", fontSize);
                        $(textNode).svgAttr({
                            width: isLeft ? (width - 5) : width,
                            height: 21,
                            textAlign: isLeft ? 'left' : 'center'
                        });
                        //mark
                        textNode.style.lineHeight = '21px';
                        textNode.style.paddingBottom = '2px';
                        textNode.style.paddingLeft = isLeft ? '5px' : 0;
                        rects.push(textNode);
                    }
                    var labelCss = {};
                    $.extend(labelCss, self._style.labelCss);
                    labelCss.fontSize = undefined;
                    $(textNode).svgAttr(labelCss);
                }
            };
            fun(left, 5, null);
            left -= 85;
            fun(left, 80, 'Monthly');
            left -= 85;
            fun(left, 80, 'Weekly');
            left -= 55;
            fun(left, 50, 'Daily');
            left -= 5;
            fun(5, left - 5, options.chartTitle || "Title", true);

            var selectedIndex = 3, i;
            for (i = 0; i < lines.length; i++) {
                $(lines[i]).svgAttr({
                    stroke: i == selectedIndex ? "red" : "#003E7F",
                    strokeWidth: 2.5
                });
            }

            for (i = 0; i < rects.length - 1; i++) {
                $(rects[i]).svgAttr({
                    cursor: "pointer",
                    stroke: "none"
                });
                $(rects[i]).bind("click", i, function (event) {
                    var index = event.data;
                    for (var j = 0; j < lines.length; j++) {
                        $(lines[j]).svgAttr({
                            stroke: j == index + 1 ? "red" : "#003E7F"
                        });
                    }
                    switch (index) {
                        case 0:
                            parameter.selectedArea = "monthly";
                            break;
                        case 1:
                            parameter.selectedArea = "weekly";
                            break;
                        case 2:
                        default:
                            parameter.selectedArea = "daily";
                            break;
                    }
                    self._reDraw();
                    if (options.topArea.hasTopArea && options.topArea.onTopAreaClick) {
                        options.topArea.onTopAreaClick(parameter.selectedArea);
                    }
                });
            }

            temp.topG = $$.g();
            $(temp.topG).svgTranslate(0, 30);
            $(temp.root).append(temp.topG);
            $(temp.topG).append(lines);
            $(temp.topG).append(texts);
            $(temp.topG).append(rects);
        },

        //当topArea.hasTopArea=true时，切换右上侧时间时，根据对应的数据源重新draw
        _reDraw: function () {
            var self = this;
            self._clear(true);
            self._setGapPage();
            self._initLabels();
            self._initScales();
            self._generateYScaleAxis();
            var isInclinedLabel = self._generateXScaleAxis();
            self._setClipPath(isInclinedLabel);
            self._setPointList();
            self._drawLine();
            self._drawAverage();
            self._setLineAnimate();
            self._initTip();
        },

        //设置ClipPath，用于左右翻页滚动
        _setClipPath: function (isInclinedLabel) {
            var self = this,
                parameter = self._parameter,
                temp = self._template,
                sm = self._seriesMember;
            if (!parameter.hasScroll) {
                return;
            }
            if (!$$.isIE8()) {
                var uuid = self._parameter.uuid,
                    clipId = uuid + "_Clip",
                    clipPath = $$.create("clipPath");
                clipPath.setAttribute("id", clipId);
                var points = '0 0 ' + sm.seriesWidth + ' 0 '
                        + sm.seriesWidth + ' ' + (sm.seriesHeight + sm.bottomHeight + 10) + ' ';
                if (false) {// &&isInclinedLabel) {
                    points += '0 ' + (sm.seriesHeight + sm.bottomHeight + 10) + ' '
                       + '0 ' + (sm.seriesHeight + 20) + ' '
                       + '0 ' + (sm.seriesHeight + 20);
                }
                else {
                    points += '0 ' + (sm.seriesHeight + sm.bottomHeight + 10);
                }
                var polyline = $$.create("polyline", {
                    points: points
                });
                $(temp.clipG).append(clipPath);
                $(clipPath).append(polyline);
                temp.scrollG.setAttribute("clip-path", "url(#" + clipId + ")");
            }
            else {
                $(temp.scrollG).svgAttr({
                    width: sm.seriesWidth,
                    height: sm.chartHeight,
                    overflow: 'hidden'
                });
            }
        },

        //画左右翻页箭头
        _drawArrows: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                leftPath, rightPath;
            if (!$$.isIE8()) {
                leftPath = "M2 4 L-2 0 2 -4 M-9,0 A9,9 0,1,1 -9,0.01Z";
                rightPath = "M-2,-4 L2,0 -2,4 M-9,0 A9,9 0,1,1 -9,0.01Z";
            }
            else {
                leftPath = 'M2 4 L-2 0 2 -4E AL0 0 9 9 0 23592960';
                rightPath = 'M-2 -4 L2 0 -2 4E AL0 0 9 9 0 23592960';
            }
            var left = temp.leftCircle = $$.svgPath(leftPath);
            $(left).svgAttr(self._style.lineCss);
            $(left).svgAttr({
                fill: 'white',
            });
            $(left).svgTranslate(-18, 0);
            $(left).bind("click", function () {
                self._leftArrowsScroll();
            });

            var right = temp.rightCircle = $$.svgPath(rightPath);
            $(right).svgAttr(self._style.lineCss);
            $(right).svgAttr({
                fill: 'white',
            });
            $(right).svgTranslate(sm.seriesWidth + 8, 0);
            $(right).bind("click", function () {
                self._rightArrowsScroll();
            });

            $(temp.arrowG).append(left);
            $(temp.arrowG).append(right);
            self._enableScroll();

            //给temp.contentG添加translateX get\set属性，用来左右翻页滚动动画
            if (!Object.prototype.hasOwnProperty.call(temp.contentG, "translateX")) {
                Object.defineProperty(temp.contentG, "translateX", {
                    get: function () {
                        return this._translateX;
                    },
                    set: function (val) {
                        $(temp.contentG).svgTranslate(val, null);
                        this._translateX = val;
                    }
                });
            }
        },

        //向左翻页滚动
        _leftArrowsScroll: function () {
            var self = this,
                parameter = self._parameter;
            if (parameter.pageIndex <= 0) {
                return;
            }
            parameter.pageIndex--;

            var limit = 0;
            if (!parameter.isScrollPage) {
                limit = -parameter.pageIndex * self._seriesMember.seriesWidth;
            } else {
                for (var i = 0; i < parameter.pageIndex ; i++) {
                    limit -= parameter.pageSizeList[i + 1] * (self._seriesMember.seriesWidth / parameter.pageSize);
                }
            }
            $(self._template.contentG).animate({ 'translateX': limit }, 900);
            self._enableScroll();
        },

        //向右翻页滚动
        _rightArrowsScroll: function () {
            var self = this,
               parameter = self._parameter;
            if (parameter.pageIndex >= parameter.pageCount) {
                return;
            }
            parameter.pageIndex++;
            self._loadNextDatas();

            var limit = 0;
            if (!parameter.isScrollPage) {
                limit = -parameter.pageIndex * self._seriesMember.seriesWidth;
            } else {
                for (var i = 0; i < parameter.pageIndex ; i++) {
                    limit -= parameter.pageSizeList[i + 1] * (self._seriesMember.seriesWidth / parameter.pageSize);
                }
            }
            $(self._template.contentG).animate({ 'translateX': limit }, 900);
            self._enableScroll();
        },

        _loadNextDatas: function () {
            var self = this,
                parameter = self._parameter;
            var max = (parameter.pageIndex + 2) * parameter.pageSize;
            var index = parameter.labelList.length;
            if (max > index) {
                self._initLabels(true);
                self._generateXScaleAxis(index);
                self._setPointList(true);
                self._drawLine(index);
            }
        },

        //控制左右翻页箭头灰显
        _enableScroll: function () {
            var self = this,
                temp = self._template,
                parameter = self._parameter;
            if (temp.leftCircle && temp.rightCircle) {
                if (parameter.pageCount == 0) {
                    temp.leftCircle.style.display = temp.rightCircle.style.display = "none";
                    //$(temp.leftCircle).svgFill("#ccc");
                    //$(temp.rightCircle).svgFill("#ccc");
                } else {
                    if (parameter.pageIndex <= 0) {
                        temp.leftCircle.style.display = "none";
                        //$(temp.leftCircle).svgFill("#ccc");
                    } else {
                        temp.leftCircle.style.display = "block";
                        //$(temp.leftCircle).svgFill("white");
                    }
                    if (parameter.pageIndex >= parameter.pageCount) {
                        temp.rightCircle.style.display = "none";
                        //$(temp.rightCircle).svgFill("#ccc");
                    } else {
                        temp.rightCircle.style.display = "block";
                        //$(temp.rightCircle).svgFill("white");
                    }
                }
            }
        },

        //初始化labelList
        _initLabels: function (load) {
            var self = this,
                labels = self._getOptionsxAxis(),
                parameter = self._parameter,
                length;
            if (!load) {
                parameter.labelList = [];
            }
            var index = parameter.labelList.length;
            if (labels && labels.length > index) {
                length = labels.length;
                if (length > index && parameter.loadSize) {
                    length = Math.min(length, index + parameter.loadSize);
                }
                for (var i = index; i < length; i++) {
                    var item = labels[i];
                    parameter.labelList.push({
                        Str: item,
                        X: (i + 1) * parameter.lableGap + parameter.labelGLeft
                    });
                }
            }
        },

        //初始化Scales
        _initScales: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                parameter = self._parameter,
                scales = parameter.scales = [],
                scaleCount, i,
                backgroundMetadatas = options.backgroundMetadatas;
            parameter.isBackground = backgroundMetadatas && backgroundMetadatas.length > 0;
            if (parameter.isBackground) {
                scaleCount = backgroundMetadatas.length;
                for (i = 0; i < scaleCount; i++) {
                    var item = backgroundMetadatas[i];
                    scales.push({
                        Value: item,
                        Y: undefined,
                        Str: item
                    });
                }
                parameter.metadatePX = (sm.seriesHeight - 30) / (scales[scales.length - 1].Value - scales[0].Value);
            }
            else {
                var max = options.yAxis.max || self._getMaxValue(),
                    min = options.yAxis.min || 0,
                    discrepancy = max - min,
                    interval;
                if (discrepancy <= 0.0) {
                    discrepancy = 1.0;
                }
                interval = options.yAxis.interval ? options.yAxis.interval : discrepancy;
                scaleCount = Math.ceil(discrepancy / interval) + 1;
                for (i = 0; i < scaleCount; i++) {
                    var value = (10 * interval * i) / 10 + min;
                    scales.push({
                        Value: value,
                        Y: undefined,
                        Str: value
                    });
                }
                parameter.metadatePX = sm.seriesHeight / (scales[scales.length - 1].Value - scales[0].Value);
            }
            scaleCount = scales.length;
            for (i = 0; i < scaleCount; i++) {
                scales[i].Y = self._getYByValue(scales[i].Value);
            }
        },

        //获取数据源中的平均值
        _getAverageValue: function () {
            var self = this,
                series = self._getOptionsSeries(),
                max = 0;
            if (series && series.length > 0) {
                var length = series.length;
                for (var i = 0; i < length; i++) {
                    var dto = series[i];
                    if (dto && dto.itemsSource && dto.itemsSource.length > 0) {
                        var item = dto.itemsSource,
                            itemLength = item.length;
                        for (var j = 0; j < itemLength; j++) {
                            max = Math.max(max, item[j].value);
                        }
                    }
                }
            }
            return max;
        },

        //获取数据源中的最大值
        _getMaxValue: function () {
            var self = this,
                series = self._getOptionsSeries(),
                max = 0;
            if (series && series.length > 0) {
                var length = series.length;
                for (var i = 0; i < length; i++) {
                    var dto = series[i];
                    if (dto && dto.itemsSource && dto.itemsSource.length > 0) {
                        var item = dto.itemsSource,
                            itemLength = item.length;
                        for (var j = 0; j < itemLength; j++) {
                            max = Math.max(max, item[j].value);
                        }
                    }
                }
            }
            return max;
        },

        //生成横向网格与y轴labels
        _generateYScaleAxis: function () {
            var self = this,
                options = self.options,
                parameter = self._parameter,
                temp = self._template,
                sm = self._seriesMember,
                scales = parameter.scales;
            if (!scales || scales.length == 0) {
                return;
            }

            var yAxisList = options.yAxis.yAxisList,
                hasYAxisList = yAxisList && yAxisList.length > 0;
            var items = [],
                texts = [],
                text, i;

            for (i = 0; i < scales.length; i++) {
                var item = scales[i],
                    y = item.Y;

                if (!parameter.isBackground && i != 0 && options.showLine.showXline) {
                    //draw 横向网格
                    var line = $$.line(-5, y + 0.5, sm.seriesWidth, y + 0.5);
                    $(line).svgAttr(self._style.lineCss);
                    $(line).svgAttr({
                        strokeDasharray: "1,1",
                        strokeWidth: 1
                    });
                    items.push(line);
                }
                else if (parameter.isBackground && i != 0) {
                    var preItem = scales[i - 1],
                        rect = $$.rect(1, y, sm.seriesWidth - 2, (i != 1) ? preItem.Y - y - 2 : preItem.Y - y - 1);
                    $(rect).svgFill(self._getBackgroundColor(i - 1));
                    items.push(rect);
                }

                //draw y轴labels
                if (!hasYAxisList && (i != 0 || !parameter.hasScroll)) {
                    text = self._createLabelSvg(item.Str, -10, y + 4, "end");
                    texts.push(text);
                }
            }

            $(temp.arrowG).append(items);
            if (hasYAxisList) {
                for (var i = 0; i < yAxisList.length; i++) {
                    var item = yAxisList[i];
                    if (item.value >= 0) {
                        var y = self._getYByValue(item.value);
                        text = self._createLabelSvg(item.value, -10, y - 2, "end");
                        texts.push(text);

                        text = self._createLabelSvg(item.content, -10, y + 10, "end");
                        texts.push(text);
                    }
                }
            }

            if (texts.length > 0) {
                $(temp.arrowG).append(texts);

                for (i = 0; i < texts.length; i++) {
                    if (!$$.isIE8()) {
                        self._setTrimmingLabel(texts[i], sm.leftWidth - 10);
                    }
                    else {
                        $(texts[i]).svgAttr({
                            x: -sm.leftWidth + 5,
                            width: sm.leftWidth - 10,
                            wrap: false
                        });
                    }
                }
            }

            if (options.yAxis.title) {
                var yTitle = options.yAxis.title;
                text = self._createLabelSvg(yTitle, -2, -sm.seriesHeight - 15, 'end', true);
                $(temp.arrowG).append(text);

                if (!$$.isIE8()) {
                    if (text.getComputedTextLength() > sm.leftWidth - 12) {
                        $(text).svgAttr({
                            x: -sm.leftWidth + 10,
                            textAnchor: "start"
                        });
                    }
                }
                else {
                    $(text).svgAttr({
                        x: -sm.leftWidth + 10,
                        width: sm.leftWidth - 12,
                        wrap: true
                    });
                    if (text.offsetWidth > sm.leftWidth - 12) {
                        $(text).svgAttr({
                            width: 'auto',
                            textAlign: 'left'
                        });
                    }
                }
            }

            //x轴title
            //if (options.yAxis.xTitle) {
            //    var xTitle = options.yAxis.xTitle,
            //		x = -sm.seriesHeight - 15;
            //    var text = self._createLabelSvg(xTitle, sm.chartWidth - sm.leftWidth, 15, 'end', true);
            //    $(temp.arrowG).append(text);
            //    if ($$.isIE8()) {
            //        $(text).svgAttr({
            //            x: -sm.leftWidth + 10,
            //            width: sm.leftWidth - 12,
            //            wrap: true
            //        });
            //    }
            //}
        },

        //生成纵向网格与x轴labels
        _generateXScaleAxis: function (from) {
            var self = this,
                parameter = self._parameter,
                temp = self._template,
                labelList = parameter.labelList,
                length = labelList.length;
            if (length == 0) {
                return false;
            }

            var sm = self._seriesMember,
                lines = [],
                texts = [];

            from = from || 0;
            for (var i = from; i < length; i++) {
                var item = labelList[i],
                    x = item.X;
                //draw 纵向网格
                if (!parameter.isBackground && item.Str != null && item.Str != '' && self.options.showLine.showYline) {
                    var line = $$.line(x + 0.5, 0, x + 0.5, -sm.seriesHeight - 5);
                    $(line).svgAttr(self._style.lineCss);
                    $(line).svgAttr({
                        strokeDasharray: "1,1",
                        strokeWidth: 1
                    });
                    lines.push(line);
                }

                //draw x轴labels
                var text;
                if (!$$.isIE8()) {
                    text = self._createLabelSvg(item.Str, 0, 0);
                }
                else {
                    text = self._createLabelSvg(item.Str, x - (parameter.lableGap - 3) / 2, 20, 'middle');
                    $(text).svgAttr({
                        width: parameter.lableGap - 3,
                        wrap: false
                    });
                }
                texts.push(text);
            };
            $(temp.labelG).append(lines);
            $(temp.labelG).append(texts);
            if (!$$.isIE8()) {
                return self._setXLableDisplay(labelList.slice(from, length), texts);
            }
            return false;
        },

        //创建label text svg元素
        _createLabelSvg: function (value, x, y, anchor, isTitle) {
            var self = this,
                text = $$.text(value, x, y, anchor, 13);
            $(text).svgAttr(self._style.labelCss);
            if (!isTitle) {
                $(text).bind("mouseover", value, function (event) {
                    self._showTip(event.data, event, true);
                })
                    .bind("mousemove", function (event) {
                        self._setTipLocation(event, true);
                    })
                    .bind("mouseout", function () {
                        self._hidenTip();
                    });
            }
            return text;
        },

        //处理x轴labels横向/斜向显示
        _setXLableDisplay: function (labelList, texts) {
            var self = this,
                parameter = self._parameter,
                sm = self._seriesMember,
                length = labelList.length,
                isInclinedLabel = false,
                xGap = parameter.lableGap - 3,
                yGap = (sm.bottomHeight - 15) / Math.sin(Math.PI / 4),
                i, text;

            //暂时去掉斜向显示label的功能
            for (i = 0; i < length; i++) {
                text = texts[i];
                var width = text.getComputedTextLength();
                if (width > xGap) {
                    isInclinedLabel = yGap > xGap;
                    break;
                }
            }

            for (i = 0; i < length; i++) {
                var x = labelList[i].X;
                text = texts[i];
                if (isInclinedLabel) {
                    $(text).svgAttr({
                        textAnchor: "end"
                    });
                    $(text).svgTranslate(x, 15);
                    $(text).setRotate(-45, 0, 0);
                    var gap = (x - parameter.lableGap / 2 - 5) / Math.sin(Math.PI / 4);
                    gap = Math.min(yGap, gap);
                    self._setTrimmingLabel(text, gap);
                }
                else {
                    $(text).svgAttr({
                        textAnchor: "middle"
                    });
                    $(text).svgTranslate(x, 20);
                    self._setTrimmingLabel(text, xGap);
                }
            }
            return isInclinedLabel;
        },

        //TextTrimming
        _setTrimmingLabel: function (text, gap) {
            var index = 1,
                str = text.textContent,
                width = text.getComputedTextLength(),
                length = str.length;
            if (width > gap) {
                while (index <= length) {
                    var subStr = str.substr(0, index);
                    text.textContent = subStr + "...";
                    width = text.getComputedTextLength();
                    if (width > gap) {
                        index--;
                        text.textContent = str.substr(0, index) + "...";
                        return true;
                    }
                    index++;
                }
            }
            return false;
        },

        //设置数据源
        _setPointList: function (load) {
            var self = this,
                oseries = self._getOptionsSeries(),
                parameter = self._parameter;
            if (oseries && oseries.length > 0) {
                var series = parameter.series = oseries;
                var labelList = parameter.labelList,
                    labelListLength = labelList.length;
                if (series.length > 0) {
                    var length = series.length;
                    for (var i = 0; i < length; i++) {
                        var dto = series[i];
                        if (dto && dto.itemsSource && dto.itemsSource.length > 0) {
                            dto.lineColor = dto.lineColor || self._getLineColor(i);
                            dto.lineStrokeWidth = dto.lineStrokeWidth || 2;
                            if (!dto.pointRadius || dto.pointRadius <= 0) {
                                dto.pointRadius = 5;
                            }
                            dto.isVirtual = dto.isVirtual || false;

                            var item = dto.itemsSource,
                                itemLength = item.length;
                            if (!load) {
                                dto.PointList = [];
                            }
                            var index = dto.PointList.length;
                            if (itemLength > index && parameter.loadSize) {
                                itemLength = Math.min(itemLength, index + parameter.loadSize);
                            }
                            for (var j = index; j < itemLength; j++) {
                                if (labelListLength > j) {
                                    var mPoint = self._getPointContext(item[j], labelList[j], i, j);
                                    if (!mPoint) {
                                        continue;
                                    }
                                    mPoint.parentDto = dto;
                                    dto.PointList.push(mPoint);
                                }
                            }
                        }
                    }
                }
                return;
            }
        },

        //生成PointContext对象
        _getPointContext: function (item, label, i, j) {
            var self = this,
                options = self.options,
                parameter = self._parameter,
                series = parameter.series,
                length = series.length,
                pointDto,
                scales = parameter.scales,
                maxScale = scales[scales.length - 1].Value,
                minScale = scales[0].Value,
                x = label.X - parameter.labelGLeft;

            if (!item || item.value == undefined || item.value == null
                || isNaN(item.value) || item.value < minScale) {
                return null;
            }
            var realValue = Math.min(item.value, maxScale),
                y = self._getYByValue(realValue);
            pointDto = {
                data: item,
                Value: realValue,
                label: label.Str,
                EllipsePoint: { X: x, Y: y },
                CurrentPoint: { X: x, Y: y },
                isTopPoint: true,
                CoincidentPoints: [],
                IsMouseOver: false,
                IsMoving: false,
                ToolTipDiv: undefined,
                parentDto: undefined,
                svgDom: undefined,
                xline: undefined,
                yline: undefined
            };
            for (var k = 0; k < length; k++) {
                var list = series[k].PointList,
                    point;
                if (list && list.length > j) {
                    point = list[j];
                    if (point.EllipsePoint.X == pointDto.EllipsePoint.X && point.EllipsePoint.Y == pointDto.EllipsePoint.Y) {
                        point.isTopPoint = false;
                        pointDto.isTopPoint = true;
                        pointDto.CoincidentPoints.push(point);
                        point.CoincidentPoints.push(pointDto);
                    }
                }
            };
            var tip;
            if (options.tooltip.toolTipTemplate) {
                tip = options.tooltip.toolTipTemplate(pointDto);
            }
            if (tip == null) {
                var title = options.yAxis.title || '';
                tip = $('<span><b>' + item.value + '</b> ' + title + '</br>' + pointDto.label + '</span>');
            }
            pointDto.ToolTipDiv = $(tip);
            return pointDto;
        },

        //画线
        _drawLine: function (index) {
            var self = this,
                options = self.options,
                parameter = self._parameter,
                temp = self._template,
                series = parameter.series;
            if (series && series.length > 0) {
                var length = series.length,
                    pointList = [],
                    lineList = [];
                index = index || 0;
                index = Math.max(0, index - 1);
                for (var i = 0; i < length; i++) {
                    var dto = series[i];
                    if (dto && dto.PointList && dto.PointList.length > 0) {
                        var list = dto.PointList,
                            listLength = list.length,
                            color = dto.lineColor,
                            points = "", polyLine, polyFill, totalCount = 0, totalValue = 0;

                        for (var j = index; j < listLength; j++) {
                            var pointDto = list[j],
                                x = pointDto.EllipsePoint.X,
                                y = pointDto.EllipsePoint.Y;
                            totalCount += y;
                            totalValue += pointDto.Value;
                            if (j == index) {
                                if (index == 0) {
                                    if (options.hasShade) { points += x; }
                                    else { points += (x + 1); }
                                }
                                else { points += (x - 1); }
                            } else if (j == listLength - 1) {
                                if (options.hasShade) {
                                    points += x;
                                } else {
                                    points += (x - 1);
                                }
                            } else {
                                points += x;
                            }
                            points += " " + y + " ";

                            var pointSvg = self._drawPoint(pointDto, i);
                            pointList.push(pointSvg);

                            var xline = $$.line(parameter.lableGap / 2, y + 0.5, x, y + 0.5);
                            $(xline).svgAttr({
                                stroke: "#FF9900",
                                strokeDasharray: "1,1",
                                display: "none",
                                strokeWidth: 1
                            });
                            pointDto.xline = xline;
                            lineList.push(xline);

                            var yline = $$.line(x + 0.5, 0, x + 0.5, y);
                            $(yline).svgAttr({
                                stroke: "#FF9900",
                                strokeDasharray: "1,1",
                                display: "none",
                                strokeWidth: 1
                            });
                            pointDto.yline = yline;
                            lineList.push(yline);
                        }

                        polyLine = $$.create("polyline", {
                            points: points
                        });
                        $(polyLine).svgAttr({
                            stroke: color,
                            strokeWidth: dto.lineStrokeWidth
                        });
                        if (dto.isVirtual) {
                            $(polyLine).svgAttr({
                                strokeDasharray: "5,2",
                            });
                        }
                        $(polyLine).svgFill("transparent");

                        if (options.hasShade) {
                            //设置线图内部背景色，目前的应用是RC的CPU Usage功能
                            points += list[listLength - 1].EllipsePoint.X + " " + 0 + " ";
                            if (index == 0) {
                                points += list[index].EllipsePoint.X - 1 + " " + 0 + " ";
                            } else {
                                points += list[index].EllipsePoint.X + " " + 0 + " ";
                            }
                            polyFill = $$.create("polyline", {
                                points: points
                            });
                            $(polyFill).svgAttr({
                                fillOpacity: 0.5,
                                stroke: "transparent",
                                strokeWidth: 0,
                                fill: color
                            });
                            lineList.push(polyFill);
                        }
                        if (dto.lineName && listLength >= 2) {
                            self._drawLineName(dto);
                        }
                        lineList.push(polyLine);
                    }
                }
                if (lineList.length > 0) {
                    $(temp.lineG).append(lineList);
                }
                if (pointList.length > 0) {
                    $(temp.lineG).append(pointList);
                }
            }
        },

        _drawAverage: function () {
            var self = this,
                options = self.options,
                parameter = self._parameter,
                temp = self._template,
                series = parameter.series;
            if (options.showAverage && series && series.length > 0) {
                for (var i = 0; i < series.length; i++) {
                    var dto = series[i];
                    if (dto && dto.itemsSource && dto.itemsSource.length > 0) {
                        var item = dto.itemsSource,
                            itemLength = item.length,
                            totalValue = 0;
                        for (var j = 0; j < itemLength; j++) {
                            totalValue += item[j].value;
                        }
                        //平均线
                        var average = parseFloat((totalValue / itemLength).toFixed(2));
                        var averageHeight = self._getYByValue(average);
                        var averageLine = $$.line(-5, averageHeight, self._seriesMember.seriesWidth, averageHeight);
                        $(averageLine).svgAttr(self._style.averageLineStyle);
                        $(temp.arrowG).append(averageLine);

                        average = options.averageFormat.format(average);
                        if (parameter.hasScroll) {
                            averageHeight = Math.min(-18, averageHeight);
                        }
                        //var x3 = self._seriesMember.seriesWidth + 3;
                        //var averageText = $$.text(average, x3, averageHeight + 4, "center");
                        //$(averageText).svgAttr(self._style.labelCss);
                        //$(temp.arrowG).append(averageText);
                        var x3 = self._seriesMember.leftWidth + self._seriesMember.seriesWidth + 8;
                        if (averageHeight >= -8) {
                            x3 += 10;
                        }
                        averageHeight = self._seriesMember.seriesHeight + self._seriesMember.topHeight + averageHeight - 12;
                        var averageText = $('<span></span>');
                        averageText.text(average);
                        averageText.css(self._style.labelCss);
                        averageText.css({
                            position: 'absolute',
                            whiteSpace: 'nowrap',
                            left: x3 + 'px',
                            top: (averageHeight + 4) + 'px'
                        });
                        self.element.append(averageText);
                    }
                }
            }
        },

        //画点
        _drawPoint: function (pointDto, index) {
            var self = this,
                options = self.options,
                x = pointDto.EllipsePoint.X,
                y = pointDto.EllipsePoint.Y,
                r = pointDto.parentDto.pointRadius,
                color = pointDto.parentDto.lineColor,
                pointD, point;

            var pointStyleList = self.options.multiPointSytleList;
            if (options.isMultiPointSytle && pointStyleList && pointStyleList.length > 0) {
                pointD = self._drawPointStyle(pointStyleList[index % pointStyleList.length], r, color);
            }
            else {
                pointD = self._drawPointStyle(options.pointStyle || 'ring', r, color);
            }
            point = pointD.point;
            $(point).svgTranslate(x, y);
            $(point).svgAttr({ cursor: 'pointer' });
            pointDto.svgDom = point;
            pointDto.isWriteBorder = pointD.isWriteBorder;
            $(point).bind("mouseover", pointDto, function (event) { self._onPointMousenter(event); });
            return point;
        },

        _drawPointStyle: function (style, r, color) {
            style = style || 'ring';
            style = style.toLowerCase();
            //如果以writeborder或2结尾(不去分大小写)，则添加白边样式。
            var isWriteBorder = style.indexOf('writeborder') > 0 || style.indexOf('2') > 0;
            if (isWriteBorder && (style.indexOf('ring') == 0 || style.indexOf('circle') == 0)) {
                r--;
                style = 'circle_writeborder';
            }
            if (style.indexOf('ring') == 0) { //空心圆 默认
                var point = $$.circle(0, 0, r - 1);
                $(point).svgAttr({
                    stroke: color,
                    strokeWidth: 2
                });
                $(point).svgFill('white');
            }
            else if (style.indexOf('circle') == 0) { //实心圆
                point = $$.circle(0, 0, r);
                $(point).svgFill(color);
            }
            else if (style.indexOf('x-type') == 0) { //X型
                var h = Math.ceil(r / 4),
                    a = (r + h) * Math.sin(Math.PI / 4),
                    b = (r - h) * Math.sin(Math.PI / 4),
                    c = h / Math.sin(Math.PI / 4);
                a = Math.ceil(a);
                b = Math.ceil(b);
                c = Math.ceil(c);
                var list = [[a, b], [b, a], [0, c], [-b, a], [-a, b], [-c, 0],
                      [-a, -b], [-b, -a], [0, -c], [b, -a], [a, -b], [c, 0], [a, b]];
                var typePath = "M";
                for (var i = 0; i < list.length; i++) {
                    typePath += list[i][0] + ',' + list[i][1];
                    if (i == 0) {
                        typePath += ' L';
                    }
                    else if (i == a.length - 1) {
                        typePath += '';
                    }
                    else {
                        typePath += ' ';
                    }
                }
                point = $$.svgPath(typePath);
                $(point).svgFill(color);
            }
            else if (style.indexOf('triangle') == 0) { //三角形
                var rx = (r * Math.sin(Math.PI / 3)),
                    ry = (r * Math.sin(Math.PI / 6));
                rx = Math.ceil(rx);
                ry = Math.ceil(ry);
                var trianglePath = 'M' + 0 + "," + (-r) + " L" + rx + "," + ry + " " + (-rx) + "," + ry + ' ' + 0 + "," + (-r) + '';
                point = $$.svgPath(trianglePath);
                $(point).svgFill(color);
            }
            else if (style.indexOf('rhombus') == 0) { //菱形
                var rhombusPath = 'M' + (-r) + ',0 L0,' + r + ' ' + r + ',0 0,' + (-r) + ' ' + (-r) + ',0';
                point = $$.svgPath(rhombusPath);
                $(point).svgFill(color);
            }
            else if (style.indexOf('rect') == 0) { //正方形
                r = r * 3 / 4;
                r = Math.ceil(r);
                var rectPath = 'M' + (-r) + ',' + r + ' L' + r + ',' + r + ' ' + r + ',' + (-r) + ' ' + (-r) + ',' + (-r) + ' ' + (-r) + ',' + r;
                point = $$.svgPath(rectPath);
                $(point).svgFill(color);
            }

            if (point && isWriteBorder) {
                $(point).svgAttr({
                    stroke: '#ffffff',
                    strokeWidth: 1.5
                });
            }
            return {
                point: point,
                isWriteBorder: isWriteBorder
            };
        },

        //画线的Name
        _drawLineName: function (dto) {
            var self = this,
                r = dto.pointRadius,
                temp = self._template,
                texts = [];
            if (dto.lineName && dto.PointList.length > 1) {
                var item = dto.PointList[1],
                    x = item.EllipsePoint.X,
                    y = item.EllipsePoint.Y - r - 5,
                    text;
                text = $$.text(dto.lineName, x, y, "center");
                $(text).svgAttr(self._style.labelCss);
                texts.push(text);
            }
            $(temp.lineG).append(texts);
        },

        //加载线图动画
        _setLineAnimate: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                parameter = self._parameter,
                uuid = self._parameter.uuid,
                clipId = uuid + "_LineGClip";
            if (!$$.isIE8()) {
                var clipPath = $$.create("clipPath");
                clipPath.setAttribute("id", clipId);
                var rect = temp.startAnimateE = $$.rect(0, 0, 0, sm.chartHeight + 40);
                $(rect).svgTranslate(parameter.lableGap / 4, -sm.seriesHeight - 20);
                $(temp.clipG).append(clipPath);
                $(clipPath).append(rect);
                temp.lineG.setAttribute("clip-path", "url(#" + clipId + ")");

                if (!rect.hasOwnProperty("Rectwidth")) {
                    Object.defineProperty(rect, "Rectwidth", {
                        set: function (val) {
                            rect.setAttribute('width', val);
                        }
                    });
                }
                $(rect).animate({ 'Rectwidth': sm.seriesWidth + 10 }, 900, function () {
                    temp.lineG.setAttribute("clip-path", "");
                    $(clipPath).remove();
                });
            }
            else {
                temp.startAnimateE = temp.lineOutG;
                $(temp.lineOutG).svgAttr({
                    height: sm.chartHeight,
                    overflow: 'hidden'
                });

                if (!Object.prototype.hasOwnProperty.call(temp.lineOutG, "Rectwidth")) {
                    Object.defineProperty(temp.lineOutG, "Rectwidth", {
                        set: function (val) {
                            $(temp.lineOutG).svgAttr({
                                width: val
                            });
                        }
                    });
                }
                $(temp.lineOutG).animate({ 'Rectwidth': sm.seriesWidth + 10 }, 900, function () {
                    $(temp.lineOutG).svgAttr({
                        height: 0,
                        overflow: 'visible'
                    });
                });
            }
        },

        _onPointMousenter: function (event) {
            var self = this,
                dto = event.data,
                temp = self._template,
                point = dto;
            if (dto.IsMoving) {
                self._clearTimer();
            }
            else {
                self._resetMovePoint();
            }
            point = dto;
            if (!dto.IsMoving && dto.CoincidentPoints && dto.CoincidentPoints.length > 0) {
                if (!dto.isTopPoint) {
                    for (var i = 0; i < dto.CoincidentPoints.length; i++) {
                        if (dto.CoincidentPoints[i].isTopPoint) {
                            point = dto.CoincidentPoints[i];
                            break;
                        }
                    }
                }
                self._movePoint(point);
            }
            if (temp.foucsPoint) {
                temp.foucsPoint.remove();
                temp.foucsPoint = undefined;
            }
            if (!point.isWriteBorder) {
                temp.foucsPoint = self._clonePoint(point.svgDom);
                temp.foucsPoint.setScale(1.5, 1.5);
            } else {
                var point1 = self._clonePoint(point.svgDom);
                var point2 = self._clonePoint(point.svgDom);
                var point3 = self._clonePoint(point.svgDom);
                var g = $$.g();
                point1.setScale(1.5, 1.5);
                point3.setScale(1.5, 1.5);
                point1.svgAttr({
                    //r: (r - 1) * 1.5,
                    strokeWidth: 0,
                    fill: 'white'
                });
                point3.svgAttr({
                    //r: (r - 1) * 1.5,
                    stroke: dto.parentDto.lineColor,
                    strokeWidth: 1,
                    fill: 'transparent'
                });
                $(g).append(point1);
                $(g).append(point2);
                $(g).append(point3);
                temp.foucsPoint = $(g);
            }

            temp.foucsPoint.bind("mousemove", point, function (e) { self._onPointMousemove(e); })
                .bind("mouseout", point, function (e) { self._onPointMouseleave(e); })
                .bind("click", point, function (e) { self._onPointClick(e); });
            temp.foucsPoint.svgAttr({ cursor: 'pointer' });
            $(temp.lineG).append(temp.foucsPoint);

            $(point.xline).svgAttr({
                display: "block"
            });
            $(point.yline).svgAttr({
                display: "block"
            });
            self._showTip(point.ToolTipDiv, event);
        },

        _clonePoint: function (element) {
            if (!$$.isIE8()) {
                return $(element).clone();
            } else {
                var clone = $$.create("shape");
                clone.style.left = element.style.left;
                clone.style.top = element.style.top;
                clone.style.width = element.style.width;
                clone.style.height = element.style.height;
                clone.path = element.path;
                clone.filled = element.filled;
                clone.fillcolor = element.fillcolor;
                clone.stroked = element.stroked;
                clone.strokeweight = element.strokeweight;
                clone.strokecolor = element.strokecolor;
                clone.coordsize = element.coordsize;
                clone.style.rotation = element.style.rotation;
                return $(clone);
            }
        },

        _onPointMousemove: function (event) {
            var self = this;
            self._setTipLocation(event);
        },

        _onPointMouseleave: function (event) {
            var self = this,
                parameter = self._parameter,
                temp = self._template,
                dto = event.data;
            if (temp.foucsPoint) {
                temp.foucsPoint.remove();
                temp.foucsPoint = undefined;
            }
            $(dto.xline).svgAttr({
                display: 'none'
            });
            $(dto.yline).svgAttr({
                display: 'none'
            });
            self._hidenTip();
            self._clearTimer();
            if (dto.IsMoving && dto.CoincidentPoints && dto.CoincidentPoints.length > 0) {
                parameter.timer = window.setTimeout(function () {
                    window.clearTimeout(parameter.timer);
                    parameter.timer = undefined;
                    self._resetMovePoint();
                }, 300);
            }
        },

        _onPointClick: function (event) {
            var self = this,
                options = self.options,
                dto = event.data;
            if (options.onPointClick) {
                if (options.tooltip.isToolTipCollapsedAfterClick) {
                    self._hidenTip();
                }
                options.onPointClick(dto);
            }
        },

        //当多点重合时，mousemove多点显示
        _movePoint: function (pointDto) {
            var self = this,
                sm = self._seriesMember,
                parameter = self._parameter,
                points = pointDto.CoincidentPoints,
                length = points.length;
            pointDto.IsMoving = true;
            parameter.moveTopPoint = pointDto;
            var index = 0;
            for (var i = 0; i < length; i++) {
                var movePoint = points[i],
                    pr = pointDto.parentDto.pointRadius;

                var p = self._getMovePoint(movePoint.EllipsePoint, pr, index);
                var pageWidth = parameter.pageIndex * sm.seriesWidth;
                while (p.y > sm.bottomHeight - pr || p.y < -sm.seriesHeight
                    || p.x - pageWidth < sm.leftWidth + pr || p.x - pageWidth > sm.leftWidth + sm.seriesWidth - pr - 3) {
                    index++;
                    p = self._getMovePoint(movePoint.EllipsePoint, pr, index);
                }
                movePoint.CurrentPoint.X = p.x;
                movePoint.CurrentPoint.Y = p.y;
                $(movePoint.svgDom).svgTranslate(p.x, p.y);
                movePoint.IsMoving = true;
                index++;
            }
        },

        _getMovePoint: function (point, pr, index) {
            var r = pr * 3 * (parseInt(index / 8, 10) + 1),
                r2 = 0,
                x = point.X, y = point.Y,
                length = 8;
            if (index < 8) {

            } else {
                length = 16;
                index -= 8;
                r = pr * 3 * (parseInt(index / 16, 10) + 2);
                r2 = r - pr * 3;
            }
            switch (index % length) {
                case 0:
                    x -= r;
                    break;
                case 1:
                    x += r;
                    break;
                case 2:
                    y -= r;
                    break;
                case 3:
                    y += r;
                    break;
                case 4:
                    x += r;
                    y -= r;
                    break;
                case 5:
                    x -= r;
                    y -= r;
                    break;
                case 6:
                    x += r;
                    y += r;
                    break;
                case 7:
                    x -= r;
                    y += r;
                    break;

                case 8:
                    x -= r2;
                    y -= r;
                    break;
                case 9:
                    x += r2;
                    y -= r;
                    break;
                case 10:
                    x -= r;
                    y -= r2;
                    break;
                case 11:
                    x += r;
                    y -= r2;
                    break;
                case 12:
                    x -= r;
                    y += r2;
                    break;
                case 13:
                    x += r;
                    y += r2;
                    break;
                case 14:
                    x -= r2;
                    y += r;
                    break;
                case 15:
                    x += r2;
                    y += r;
                    break;
            }
            return {
                x: x,
                y: y
            };
        },

        _resetMovePoint: function () {
            var self = this,
                movePoint = self._parameter.moveTopPoint;
            if (movePoint && movePoint.IsMoving && movePoint.CoincidentPoints && movePoint.CoincidentPoints.length > 0) {
                var points = movePoint.CoincidentPoints,
                    length = points.length;
                for (var i = 0; i < length; i++) {
                    var point = points[i],
                        x = point.EllipsePoint.X,
                        y = point.EllipsePoint.Y;
                    $(point.svgDom).svgTranslate(x, y);
                    point.CurrentPoint.X = x;
                    point.CurrentPoint.Y = y;
                    point.IsMoving = false;
                }
                movePoint.IsMoving = false;
            }
            self._parameter.moveTopPoint = undefined;
        },

        _clearTimer: function () {
            var self = this,
                parameter = self._parameter;
            if (parameter.timer) {
                window.clearTimeout(parameter.timer);
                parameter.timer = undefined;
            }
        },

        //init tooltip
        _initTip: function () {
            var self = this,
                temp = self._template;
            temp.toolTipDiv = $('<span></span>');

            if (self.options.tooltip.toolTipStyle != null) {
                var style = self.options.tooltip.toolTipStyle;
                if (typeof style == "string") {
                    temp.toolTipDiv.addClass(style);
                } else if (typeof style == "object") {
                    temp.toolTipDiv.css(style);
                }
            }
            else if (self.options.tooltip.useDefaultStyle) {
                temp.toolTipDiv.css(self._style.TooltipStyle_Black);
            } else {
                temp.toolTipDiv.css(self._style.TooltipStyle_White);
            }
            temp.toolTipDiv.css({
                visibility: "hidden",
                position: "absolute"
            });
            self.element.append(temp.toolTipDiv);

            temp.toolTipLabelDiv = $('<span></span>');
            temp.toolTipLabelDiv.css(self._style.TooltipStyle_Label);
            temp.toolTipLabelDiv.css("visibility", "hidden");
            self.element.append(temp.toolTipLabelDiv);
        },

        //show tooltip
        _showTip: function (tip, e, isLabel) {
            var self = this,
                temp = self._template,
                toolTip = isLabel ? temp.toolTipLabelDiv : temp.toolTipDiv;
            toolTip.empty();
            toolTip.append(tip);
            toolTip.css("visibility", "visible");
            self._setTipLocation(e, isLabel);
        },

        //set tooltip location
        _setTipLocation: function (e, isLabel) {
            var self = this,
                temp = self._template,
                sm = self._seriesMember,
                parameter = self._parameter,
                tip = isLabel ? temp.toolTipLabelDiv : temp.toolTipDiv;
            var offsetX = $(self.element).offset().left;
            var offsetY = $(self.element).offset().top;
            var mouseX, mouseY;
            if (e.pageX) {
                mouseX = e.pageX - offsetX;
                mouseY = e.pageY - offsetY;
            }
            else {
                mouseX = e.offsetX;
                mouseY = e.offsetY;
            }
            var mX = mouseX + 15,
                mY = mouseY,
                tipWidth = tip.width() + 23,
                tipHeight = tip.height() + 16;
            var maxX = sm.chartWidth / parameter.scale - tipWidth - 3,
                maxY = sm.chartHeight / parameter.scale - tipHeight - 3;
            if (mX > maxX) {
                mX = mouseX - tipWidth - 8;
            }
            if (mY > maxY) {
                mY = mouseY - tipHeight + 20;
            }
            tip.css({
                left: mX + 'px',
                top: mY + 'px'
            });
        },

        //hiden tooltip
        _hidenTip: function () {
            var self = this;
            //self._template.toolTipDiv.empty();
            self._template.toolTipDiv.css("visibility", "hidden");
            //self._template.toolTipLabelDiv.empty();
            self._template.toolTipLabelDiv.css("visibility", "hidden");
        },

        //get 当前选择时间的Series
        _getOptionsSeries: function () {
            var self = this,
                options = self.options,
                parameter = self._parameter;
            if (parameter.selectedArea == 'weekly' && options.topArea.hasTopArea && options.topArea.weeklySeries) {
                return options.topArea.weeklySeries;
            }
            else if (parameter.selectedArea == 'monthly' && options.topArea.hasTopArea && options.topArea.monthlySeries) {
                return options.topArea.monthlySeries;
            }
            else {
                return options.series;
            }
        },

        //get 当前选择时间的xAxis
        _getOptionsxAxis: function () {
            var self = this,
                options = self.options,
                parameter = self._parameter;
            if (parameter.selectedArea == 'weekly' && options.topArea.hasTopArea && options.topArea.weeklyxAxis) {
                return options.topArea.weeklyxAxis || [];
            }
            else if (parameter.selectedArea == 'monthly' && options.topArea.hasTopArea && options.topArea.monthlyxAxis) {
                return options.topArea.monthlyxAxis || [];
            }
            else {
                return options.xAxis || [];
            }
        },

        _getLineColor: function (index) {
            var list = this._constant.ColorStorage;
            return this._constant.ColorStorage[index % list.length];
        },

        _getBackgroundColor: function (index) {
            var list = this._constant.BackgroundColorStorage;
            return list[index % list.length];
        },

        _getYByValue: function (value) {
            var self = this,
               parameter = self._parameter;
            return -(value - parameter.scales[0].Value) * parameter.metadatePX;
        }
    });
})(AUI, jQuery);


//linefacechart
(function ($$, $) {
    "use strict";
    $.widget("aui.linefacechart", {
        options: {
            width: 330,
            height: 70,
            leftValue: 0,
            rightValue: 0,
            maxValue: 0,
            value: 0,
            suffix: undefined
        },

        _style: {
            faceStyle: { stroke: '#808080', fill: '#FFF', strokeWidth: 1 },
            textBackground: '#eeeeee',
            lineColors: ['#62bf60', '#f7dd2f', '#ff3939']
        },

        _params: function () {
            return {
                _constant: {
                    CHART_WIDTH: 330, //控件整体默认宽度
                    CHART_HEIGHT: 70, //控件整体默认高度
                    LINE_HEIGHT: 10, //横条默认高度
                    LINE_MARGE: 20 //横条两侧间距
                },
                _seriesMember: {
                    chartWidth: undefined,
                    chartHeight: undefined,
                    scaleX: undefined,
                    scaleY: undefined,
                    lineHeight: undefined,
                    left: undefined,
                    valuePosition: undefined
                },
                _template: {
                    root: undefined,
                    panelG: undefined,
                    leftText: undefined,
                    leftRect: undefined,
                    rightText: undefined,
                    rightRect: undefined,
                    valueG: undefined,
                    valueFace: undefined
                }
            };
        },

        _create: function () {
            var self = this;
            var parm = self._params();
            $.extend(self, parm);
            self._seriesMember.uuid = $$.generateUUIDByControlName("linefacechart");
            self._initLayout(true);
        },

        _setOption: function (key, value) {
            var self = this,
                options = self.options;
            switch (key) {
                case 'value':
                    options[key] = value;
                    self._drawValueElement();
                    break;
                case 'leftValue':
                    options[key] = value;
                    self._changeLeftRightText();
                    self._drawValueElement();
                    break;
                case 'rightValue':
                    options[key] = value;
                    self._changeLeftRightText();
                    self._drawValueElement();
                case 'maxValue':
                    options[key] = value;
                    //self._changeLeftRightText();
                    self._drawValueElement();
                    break;
                default:
                    break;
            }
        },

        destroy: function () {
            this.element.off();
            this.element.remove();
        },

        changeValue: function (value) {
            var self = this,
                options = self.options;
            var leftrightChange = false,
                valueChange = false;
            //if (left != null && !isNaN(left)) {
            //    options['leftValue'] = left;
            //    leftrightChange = true;
            //}
            //if (right != null && !isNaN(right)) {
            //    options['rightValue'] = right;
            //    leftrightChange = true;
            //}
            //if (leftrightChange) {
            //    self._changeLeftRightText();
            //    valueChange = true;
            //}
            if (value != null && !isNaN(value)) {
                options['value'] = value;
                valueChange = true;
            }
            if (valueChange) {
                self._drawValueElement();
            }
        },

        //初始化调整控件的位置
        _initLayout: function () {
            var self = this,
                element = self.element,
                options = self.options,
                con = self._constant,
                sm = self._seriesMember,
                temp = self._template,
                chartWidth = sm.chartWidth = options.width || con.CHART_WIDTH,
                chartHeight = sm.chartHeight = options.height || con.CHART_HEIGHT,
                uuid = sm.uuid;

            sm.lineHeight = con.LINE_HEIGHT;
            sm.left = con.LINE_MARGE;

            element.css({
                overflow: 'hidden',
                position: 'relative'
            });

            if (!$$.isIE8()) {
                var defs = '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.1" ' +
                    'width="' + chartWidth + '" height="' + chartHeight + '" id="' + uuid + '">' +
                    '<defs><linearGradient id="defs1" x1="0%" y1="0%" x2="100%" y2="0%">' +
                    '<stop offset="0%" stop-color="' + self._style.lineColors[0] + '"/>' +
                    '<stop offset="50%" stop-color="' + self._style.lineColors[1] + '"/>' +
                    '<stop offset="100%" stop-color="' + self._style.lineColors[2] + '"/>' +
                    '</linearGradient></defs></svg>';
                temp.root = $(defs);

            } else {
                self._initVmlCss();
                temp.root = $$.svg(uuid, {
                    width: chartWidth,
                    height: chartHeight
                });
            }
            element.append(temp.root);
            temp.panelG = $$.g();
            $(temp.root).append(temp.panelG);

            //optionWidth = options.width || con.CHART_WIDTH,
            //optionHeight = options.height || con.CHART_HEIGHT,
            //var scaleX = sm.scaleX = optionWidth / chartWidth;
            //var scaleY = sm.scaleY = optionHeight / chartHeight;
            //if (!$$.isIE8()) {
            //    temp.panelG = temp.vmlDiv = $$.g();
            //    $(temp.panelG).svgTranslate(optionWidth / 2, optionHeight - bottomHeight * scaleY);
            //    $(temp.panelG).setScale(scaleX, scaleY);
            //    $(temp.root).append(temp.panelG);
            //} else {
            //    var div = temp.vmlDiv = $$.g();
            //    $(div).svgTranslate(optionWidth / 2, optionHeight - bottomHeight * scaleY);
            //    temp.panelG = $$.g();
            //    temp.panelG = $$.create('group', {
            //    width: '100px',
            //    height: '100px',
            //    coordsize: (100 / scaleX) + ',' + (100 / scaleY)
            //    });
            //    $(div).append(temp.panelG);
            //    $(temp.root).append(div);
            //}

            self._drawLine();
            self._drawLeftRight();
        },

        _drawLine: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                chartWidth = sm.chartWidth;

            var x1 = sm.left,
                x2 = sm.chartHeight / 2 - sm.lineHeight / 2,
                y1 = chartWidth - sm.left * 2,
                y2 = sm.lineHeight;
            x1 = Math.round(x1);
            x2 = Math.round(x2);
            y1 = Math.round(y1);
            y2 = Math.round(y2);
            var piepath = $$.rect(x1, x2, y1, y2);
            if ($$.isIE8()) {
                $(piepath).svgFill(self._style.lineColors[2]);
                $(piepath).html('<v:fill type="gradient" colors="50% ' + self._style.lineColors[1] + ',100% ' + self._style.lineColors[0] + '" angle="90"/>');
            } else {
                $(piepath).svgFill('url(#defs1)');
            }

            $(piepath).svgAttr({
                strokeWidth: 0
            });
            $(temp.panelG).append(piepath);
        },

        _drawLeftRight: function () {
            var self = this,
                sm = self._seriesMember,
                temp = self._template,
                lineWidth = sm.chartWidth - sm.left * 2,
                leftText, rightText, leftRect, rightRect, leftArrow, rightArrow,
                leftFace, rightFace;

            var y1 = sm.chartHeight / 2 - sm.lineHeight / 2,
                y2 = sm.chartHeight / 2 + sm.lineHeight / 2 + 7,
                x1 = sm.left + lineWidth / 3,
                x2 = x1 + lineWidth / 3;

            //left\right face
            leftFace = self._drawFace(0, sm.left + 8, y1 - 13);
            rightFace = self._drawFace(2, sm.left + lineWidth - 10, y1 - 13);
            $(temp.panelG).append(leftFace);
            $(temp.panelG).append(rightFace);

            //left rect\text
            leftArrow = self._drawArraw(x1, y2 - 4, 8, 4, self._style.textBackground);
            leftRect = temp.leftRect = $$.rect(x1 - 25, y2, 50, 20);
            $(leftRect).svgFill(self._style.textBackground);
            leftText = temp.leftText = $$.text('', x1, y2 + 14, 'middle', 12);
            $(temp.panelG).append(leftArrow);
            $(temp.panelG).append(leftRect);
            $(temp.panelG).append(leftText);
            if ($$.isIE8()) {
                $(leftText).svgAttr({ y: y2 + 4 });
            }

            //right rect\text
            rightArrow = self._drawArraw(x2, y2 - 4, 8, 4, self._style.textBackground);
            rightRect = temp.rightRect = $$.rect(x2 - 25, y2, 50, 20);
            $(rightRect).svgFill(self._style.textBackground);
            rightText = temp.rightText = $$.text('', x2, y2 + 14, 'middle', 12);
            $(temp.panelG).append(rightArrow);
            $(temp.panelG).append(rightRect);
            $(temp.panelG).append(rightText);
            if ($$.isIE8()) {
                $(rightText).svgAttr({ y: y2 + 4 });
            }

            self._changeLeftRightText();
            self._drawValueElement();
        },

        _changeLeftRightText: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template;
            var suffix = options.suffix || '',
                lineWidth = sm.chartWidth - sm.left * 2,
                x1 = sm.left + lineWidth / 3,
                x2 = x1 + lineWidth / 3;
            //set rect\text width
            if (!$$.isIE8()) {
                temp.leftText.textContent = options.leftValue + suffix;
                temp.rightText.textContent = options.rightValue + suffix;
            }
            else {
                temp.leftText.innerText = options.leftValue + suffix;
                temp.rightText.innerText = options.rightValue + suffix;
            }
            var lW = !$$.isIE8() ? temp.leftText.getComputedTextLength() + 4 : $(temp.leftText).width() + 4,
                rW = !$$.isIE8() ? temp.rightText.getComputedTextLength() + 4 : $(temp.rightText).width() + 4;
            lW = Math.max(50, lW);
            rW = Math.max(50, rW);
            $(temp.leftRect).svgAttr({ x: x1 - lW / 2, width: lW });
            $(temp.rightRect).svgAttr({ x: x2 - rW / 2, width: rW });
            if ($$.isIE8()) {
                $(temp.leftText).svgAttr({ x: x1 - lW / 2, width: lW });
                $(temp.rightText).svgAttr({ x: x2 - rW / 2, width: rW });
            }

        },

        //value valueG
        _drawValueElement: function () {
            var self = this,
                options = self.options,
                sm = self._seriesMember,
                temp = self._template,
                lineWidth = sm.chartWidth - sm.left * 2,
                //a = lineWidth / (3 * (options.rightValue - options.leftValue)),
                //x = sm.left + lineWidth / 3 + (options.value - options.leftValue) * a,
                y1 = sm.chartHeight / 2 - sm.lineHeight / 2,
                y2 = sm.chartHeight / 2 + sm.lineHeight / 2 + 7,
                left = options.leftValue,
                right = options.rightValue,
                value = options.value,
                max = options.maxValue;
            //x = Math.max(sm.left, x);
            //x = Math.min(sm.left + lineWidth, x);
            var a, x, position;
            a = lineWidth / 3;
            if (value <= 0) {
                position = 0;
                x = 0;
            }
            else if (value < left) {
                position = 0;
                x = a * value / left;
            } else if (value <= right) {
                position = 1;
                x = a + a * (value - left) / (right - left);
            } else if (value < max) {
                position = 2;
                x = a * 2 + a * (value - right) / (max - right);
            } else {
                position = 2;
                x = a * 3;
            }
            x += sm.left;

            var isCreate = !temp.valueG;
            if (isCreate) {
                var valueArrow = self._drawArraw(0.5, y2 - 4, 7, 4, self._style.faceStyle.stroke);
                var valueLine = $$.line(0.5, y1 - 3, 0.5, y2 - 5);
                $(valueLine).svgAttr(self._style.faceStyle);
                temp.valueG = $$.g();
                $(temp.valueG).append(valueArrow);
                $(temp.valueG).append(valueLine);
                $(temp.panelG).append(temp.valueG);
            }

            if (isCreate || sm.valuePosition != position) {
                sm.valuePosition = position;
                if (temp.valueFace) { temp.valueFace.remove(); }
                var g = self._drawFace(position, 0, y1 - 13);
                temp.valueFace = $(g);
                $(temp.valueG).append(g);
            }

            if (!Object.prototype.hasOwnProperty.call(temp.valueG, 'moveX')) {
                Object.defineProperty(temp.valueG, 'moveX', {
                    get: function () {
                        return parseInt(this._moveX, 10);
                    },
                    set: function (val) {
                        var intVal = parseInt(val, 10);// parseFloat(val).toFixed(2);
                        if (!$$.isIE8()) {
                            this.setAttribute('transform', 'translate(' + intVal + ',0)');
                        } else {
                            this.style.left = intVal;
                        }
                        this._moveX = intVal;
                    }
                });
            }
            if (isCreate) {
                temp.valueG.moveX = x;
                //temp.valueG.svgTranslate(x, 0);
            }
            else {
                $(temp.valueG).stop();
                $(temp.valueG).animate({ moveX: x }, 800);
            }
        },

        _drawFace: function (index, x, y) {
            var self = this,
                r = 8,
                g = $$.g(),
                faces = [];
            if (!$$.isIE8()) {
                r = 7.5;
                x = x - 1;
                y = Math.round(y);
                y += 0.5;
            }
            $(g).svgTranslate(x, y);
            //圆脸
            var d, face;
            if (!$$.isIE8()) {
                d = "M " + (r + 1) + "," + 0 +
                    " A " + r + "," + (r) +
                    " 0 1 1 " +
                    (r + 1) + ",-" + 0.001 +
                    " Z ";
            } else {
                d = 'AE0,0 ' + r + ',' + r + ' 0,23592960 E';
            }
            face = $$.svgPath(d);
            $(face).svgAttr(self._style.faceStyle);
            faces.push(face);

            //双眼\嘴
            var d1, d2;
            switch (index) {
                case 0:
                    if (!$$.isIE8()) {
                        d1 = "M-3.5,-0.5 C-3,-2.5 -1,-2.5 -0.5,-0.5" + " M2.5,-0.5 C3,-2.5 5,-2.5 5.5,-0.5";
                        d2 = "M-2,2.5 C-1,5.5 3,5.5 4,2.5";
                    } else {
                        d1 = "M-5,-1 c-4,-3 -3,-3 -2,-1 r 0 0 e" + " M2,-1 c3,-3 4,-3 5,-1 r 0 0 e";
                        d2 = "M-3,3 c-2,5 2,5 3,3 r 0 0 e";
                    }
                    break;
                case 1:
                    if (!$$.isIE8()) {
                        d1 = "M-4,-2 L-0.5,-2 " + "M2,-2 L5.5,-2";
                        d2 = "M-1.5,3 L3.5,3 ";
                    } else {
                        d1 = "M-5,-2 L-2,-2 " + "M2,-2 L5,-2";
                        d2 = "M-2,3 L2,3 ";
                    }
                    break;
                case 2:
                    if (!$$.isIE8()) {
                        d1 = "M-4,-1 C-1,-1 -1,-1 -0.5,-3.5" + " M2.5,-3.5 C2,-1 3.5,-1 6,-1";
                        d2 = "M-2,4.5 C-1,2.5 3,2.5 4,4.5";
                    } else {
                        d1 = "M-5,-1 C-3,-1 -2,-1 -2,-4 r 0 0 e" + " M2,-4 C2,-1 3,-1 5,-1 r 0 0 e";
                        d2 = "M-3,4 C-2,2 2,2 3,4 r 0 0 e";
                    }
                    break;
                default:
                    break;
            }
            if (d1) {
                var eye = $$.svgPath(d1);
                $(eye).svgAttr(self._style.faceStyle);
                faces.push(eye);
            }
            if (d2) {
                var mouth = $$.svgPath(d2);
                $(mouth).svgAttr(self._style.faceStyle);
                faces.push(mouth);
            }
            $(g).append(faces);

            var sw = 0;
            switch (index) {
                case 0:
                    sw = 0.8;
                    break;
                case 1:
                    sw = 1;
                    break;
                case 2:
                    sw = 1.1;
                    break;
            }
            //$(face).svgAttr({ strokeWidth: sw });
            //$(eye).svgAttr({ strokeWidth: sw });
            //$(mouth).svgAttr({ strokeWidth: sw });
            return g;
        },

        _drawArraw: function (x, y, width, height, color) {
            width = width / 2;
            width = Math.round(width);
            if ($$.isIE8()) { height += 1; }
            var d = 'M -' + width + ',' + height + ' L ' + width + ',' + height + ' L0,0';
            var path = $$.svgPath(d);
            $(path).svgFill(color);
            $(path).svgTranslate(x, y);
            return path;
        },

        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
    });
})(AUI, jQuery);


//mapchart
(function ($$, $) {
    "use strict";
    $.widget("aui.mapchart", {
        options: {
            width: undefined,
            height: undefined,
            imgPath: undefined,
            series: [],
            toolTipTemplate: $.noop,
        },

        _style: {
            //圆上显示文字默认样式
            TextStyle: {
                fill: '#222',
                fontFamily: "Verdana",
                fontSize: 12
            },
            //右下角显示文字默认样式
            RangeTextStyle: {
                fill: '#222',
                fontFamily: "Verdana",
                fontSize: 12
            },
            TooltipStyle: {
                position: 'absolute',
                maxWidth: '160px',
                backgroundColor: '#333333',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#fff',
                //whiteSpace: 'nowrap',
                lineHeight: '18px',
                padding: '8px 15px 8px 8px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)',
            },
            //tooltip默认样式
            TooltipStyle_Black: {
                fontFamily: 'Verdana',
                fontSize: '12px',
                backgroundColor: '#333333',
                color: '#ffffff',
                borderRadius: '2px',
                whiteSpace: 'nowrap',
                padding: '5px 10px 5px 10px',
                boxShadow: '0 0 3px #333333',
                display: 'inline-block',
                marginLeft: '-17px',
                marginRight: '-17px'
            },
            ColorList: [
                { stroke: '#3cd878', fill: '#66c599' },
                { stroke: '#0e80cb', fill: '#5599dd' },
                { stroke: '#bd3b47', fill: '#c99196' },
                { stroke: '#fec42c', fill: '#f7db91' },
                { stroke: '#8560a8', fill: '#bb96cc' },
                { stroke: '#a0de3f', fill: '#cfeca3' },
                { stroke: '#fd9335', fill: '#eeba8c' },
                { stroke: '#dd4d79', fill: '#eb8daa' },
                { stroke: '#0e9993', fill: '#5ec2be' },
                { stroke: '#43ccff', fill: '#94e2ff' }
            ]
        },

        _create: function () {
            var self = this;
            self._initLayout();
        },

        _setOption: function (key, value) {
            var self = this,
                options = self.options;
            options[key] = value;
        },

        destroy: function () {
            this.element.empty();
        },

        _initLayout: function () {
            var self = this,
                element = self.element,
                options = self.options,
                uuid = "map_" + $$.generateUUIDByControlName();

            self._initVmlCss();
            var outDiv = self.outDiv = $('<div style="position:relative;"></div>');
            element.append(outDiv);

            var img = $('<img src="' + options.imgPath + '" />');
            outDiv.append(img);

            var svgDiv = $('<div></div>');
            svgDiv.css({
                position: 'absolute',
                top: 0
            });
            outDiv.append(svgDiv);
            var svg = $$.svg(uuid);
            self.rootSvg = $(svg);
            svgDiv.append(self.rootSvg);

            img.load(function () {
                var w = options.width || img.width(),
                    h = options.height || img.height();
                self.rootSvg.css({
                    width: w,
                    height: h
                });

                self._draw();
            });
        },

        _draw: function () {
            var self = this,
                options = self.options,
                rootSvg = self.rootSvg,
                //chartWidth = rootSvg.width(),
                chartHeight = rootSvg.height(),
                i, j,
                item;

            var length = options.series.length;
            for (i = 0; i < length; i++) {
                item = options.series[i];
                var r = Math.ceil(item.width / 2);

                var color = self._getColor(i);
                var circleStyle = {
                    fill: item.color || color.fill,
                    fillOpacity: 0.8,
                    strokeWidth: 2,
                    stroke: item.stroke || (item.color ? '#FFF' : color.stroke)
                };
                for (j = 0; j < item.points.length; j++) {
                    var point = item.points[j],
                        x = Math.ceil(point.x),
                        y = Math.ceil(point.y);
                    var circle = $$.circle(0, 0, r);
                    var textY = item.isTop ? y - r - 3 : y + 8;
                    var text = $$.text(point.value, x, textY, 'middle');
                    $(circle).svgTranslate(x, y);
                    $(circle).svgAttr(circleStyle);
                    $(text).svgAttr(self._style.TextStyle);
                    if (item.fontColor) {
                        $(text).svgAttr({ fill: item.fontColor });
                    }
                    if (item.fontSize) {
                        $(text).svgAttr({ fontSize: item.fontSize });
                    }
                    rootSvg.append(circle);
                    rootSvg.append(text);
                    if ($$.isIE8()) {
                        $(text).svgAttr({
                            x: x - $(text).width() / 2,
                            y: item.isTop ? y - r - 22 : y - 12
                        });
                    }

                    var outCircle = $$.circle(0, 0, r);
                    $(outCircle).svgTranslate(x, y);
                    $(outCircle).svgAttr({
                        fillOpacity: 0,
                        //fill: 'transparent',
                        cursor: 'pointer'
                    });
                    rootSvg.append(outCircle);

                    var tip;
                    if (options.toolTipTemplate) {
                        tip = options.toolTipTemplate(point);
                    }
                    if (tip == null || tip == undefined) {
                        tip = point.name + ': ' + point.value;
                    }

                    $(outCircle).bind("mouseover", tip, function (event) {
                        self._showTip(event.data, event);
                    }).bind("mousemove", function (event) {
                        self._setTipLocation(event);
                    }).bind("mouseout", function () {
                        self._hidenTip();
                    });
                }
            }
            var tx = 15 + 4, max = 0;
            for (i = 0; i < length; i++) {
                item = options.series[i];
                var isFirst = i % 2 == 0;
                var circle2 = $$.circle(0, 0, 4);
                var ty = isFirst ? chartHeight - 32 : chartHeight - 15;
                $(circle2).svgTranslate(tx, ty);
                var color2 = self._getColor(i);
                $(circle2).svgAttr({ fill: item.color || color2.stroke });
                rootSvg.append(circle2);

                var textLabel = $$.text(item.rangeText, tx + 9, ty + 4);
                $(textLabel).svgAttr(self._style.RangeTextStyle);
                rootSvg.append(textLabel);

                if (i != length - 1) {
                    var width = !$$.isIE8() ? textLabel.getComputedTextLength() : $(textLabel).width();
                    max = Math.max(max, width);
                    if (!isFirst) {
                        tx += 9 + max + 25 + 4;
                        max = 0;
                    }
                }
            }
            self._initTip();
        },

        _getColor: function (index) {
            var self = this,
                length = self._style.ColorList.length;
            return self._style.ColorList[index % length];
        },

        //init tooltip
        _initTip: function () {
            var self = this;
            self.toolTipDiv = $('<span></span>');
            self.toolTipDiv.css(self._style.TooltipStyle);
            self.toolTipDiv.css("visibility", "hidden");
            self.outDiv.append(self.toolTipDiv);
        },

        //show tooltip
        _showTip: function (tip, e) {
            var self = this;
            self.toolTipDiv.empty();
            self.toolTipDiv.append(tip);
            self.toolTipDiv.css("visibility", "visible");
            self._setTipLocation(e);
        },

        //set tooltip location
        _setTipLocation: function (e) {
            var self = this,
                tip = self.toolTipDiv;
            var offsetX = $(self.outDiv).offset().left;
            var offsetY = $(self.outDiv).offset().top;
            var mouseX, mouseY;
            if (e.pageX) {
                mouseX = e.pageX - offsetX;
                mouseY = e.pageY - offsetY;
            }
            else {
                mouseX = e.offsetX;
                mouseY = e.offsetY;
            }
            var chartWidth = self.rootSvg.width(),
                chartHeight = self.rootSvg.height();
            var mX = mouseX + 15,
                mY = mouseY,
                tipWidth = tip.width() + 23,
                tipHeight = tip.height() + 16;
            var maxX = chartWidth - tipWidth - 3,
                maxY = chartHeight - tipHeight - 3;
            if (mX > maxX) {
                mX = mouseX - tipWidth - 8;
            }
            if (mY > maxY) {
                mY = mouseY - tipHeight + 20;
            }
            tip.css({
                left: mX + 'px',
                top: mY + 'px'
            });
        },

        //hiden tooltip
        _hidenTip: function () {
            var self = this;
            self.toolTipDiv.css("visibility", "hidden");
            self.toolTipDiv.empty();
        },

        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        }
    });
})(AUI, jQuery);


//piechart
(function ($$, $) {
    "use strict";
    $.widget("aui.piechart", {
        options: {
            width: undefined, //控件的宽度
            height: undefined, //控件高度
            r: undefined, //饼圆的半径
            innerR: undefined, //饼圆内圆半径
            moveR: 10, //点击饼图后，扇形移动的距离
            datas: [], //数据源集合
            brokenLine: {
                show: true, //折线注释是否可见
                showInside: false, //注释文字显示位置是否在内部
                showZeroLine: true, //值为0的折线注释是否可见
                textTemplate: $.noop, //注释文字自定义模板
                brokenLineLength: undefined, //折线的长度
                horizontalLineLength: undefined, //折线的水平长度
                labelSize: undefined //折线注释文字的字体大小
            },
            innerText: {
                show: true, //内圆文字是否可见
                showTotal: true, //内圆Total文字是否可见
                innerContent: undefined, //显示文字的内容
                totalContent: "Total", //Total文字的内容
                innerSize: undefined, //显示文字的字体大小
                totleSize: undefined //Total文字的字体大小
            },
            labelArea: {
                show: true, //注释区域是否可见
                position: 'right', //注释显示的位置
                showValue: false, //数值是否可见
                width: undefined, //注释区域宽度或高度
                useTemplate: false, //注释区域显示内容是否自定义
                template: function (data) { //设置注释区域显示自定义内容
                    return data.name;
                },
                triggerClickEvent: false //点击自定义内容时是否触发onPieClick事件
            },
            onPieClick: $.noop, //点击环或右侧注释时的callback函数
            title: function (data) { //鼠标悬停在某环上时ToolTip显示的信息。
                return data.name + ': ' + data.data;
            },
            tooltip: {
                toolTipTemplate: $.noop,
                toolTipStyle: null
            }
        },

        _style: {
            //tooltip默认样式
            TooltipStyle_Black: {
                position: 'absolute',
                maxWidth: '160px',
                backgroundColor: '#333333',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#fff',
                //whiteSpace: 'nowrap',
                lineHeight: '18px',
                padding: '8px 15px 8px 8px',
                /* older safari/Chrome browsers */
                '-webkit-opacity': 0.95,
                /* Netscape and Older than Firefox 0.9 */
                '-moz-opacity': 0.95,
                /* IE9 + etc...modern browsers */
                'opacity': 0.95,
                /* IE 4-9 */
                'filter': 'alpha(opacity=95)',
            },
            TooltipLabelStyle: {
                position: "absolute",
                fontFamily: 'Verdana',
                fontSize: 12,
                backgroundColor: '#fff',
                border: '#999 1px solid',
                color: '#222',
                padding: '2px 4px'
            },
            MiddleTextStyle1: {
                strokeWidth: 0,
                fill: '#222',
                fontSize: 20,
                fontFamily: "Verdana Light"
            },
            MiddleTextStyle2: {
                strokeWidth: 0,
                fill: '#222',
                fontSize: 14,
                fontFamily: "Verdana"
            },
            BrokenLineStyle: {
                stroke: "#999999",
                strokeWidth: 1,
                fill: "none"
            },
            BrokenLineTextStyle: {
                strokeWidth: 0,
                fill: "#222",
                fontSize: 12,
                fontFamily: "Verdana",
                //cursor: "pointer"
            },
            RightLabelTextStyle: {
                strokeWidth: 0,
                fill: "#222",
                fontSize: 12,
                fontFamily: "Verdana"
            },
            LabelDivStyle: {
                position: 'absolute',
                fontFamily: "Verdana",
                fontSize: 12,
                color: '#222',
                left: '0px',
                top: '0px'
            }
        },

        _create: function () {
            var self = this, options = self.options, element = self.element;
            var params = {
                constant: {
                    PIE_HEIGHT: 400, // 线图默认整体高度
                    PIE_WIDTH: 800, // 线图默认整体宽度
                    PIE_MOVER: 10, //点击饼图后，扇形移动的距离
                    BROKENLINELENGTH: 20, //折线的最大长度
                    HORIZONTALLINELENGTH: 30, //折线的最大水平长度
                    LABELSIZE: 12, //折线注释文字的字体默认大小
                    RIGHTRECTWIDTH: 8, //右侧注释方块的长宽
                    DEFAULT_COLORS: ['#bd3b47', '#dd4444', '#fd9335', '#fec42c', '#f2ef1c', '#a0de3f', '#3cb878',
                        '#0e9993', '#43ccff', '#00aeef', '#0e80cb', '#5793f3', '#8560a8', '#dd4d79', '#aaaaaa']
                },
                _parameter: {
                    width: 0,
                    height: 0,
                    right: 0,
                    bottom: 0,
                    r: 0,
                    innerR: 0,
                    cx: 0,
                    cy: 0,
                    moveR: 0,
                    brokenLineLength: 0,
                    horizontalLineLength: 0,
                    labelSize: 0
                }
            }
            $.extend(self, params);

            self.uuid = "piechart_" + $$.generateUUIDByControlName();
            self._parameter.width = options.width || element.width() || self.constant.PIE_WIDTH;
            self._parameter.height = options.height || element.height() || self.constant.PIE_HEIGHT;

            self.chart = $$.svg(self.uuid, {
                width: self._parameter.width,
                height: self._parameter.height
            });
            element.css({
                overflow: 'hidden',
                position: 'relative'
            });
            element.append(self.chart);
            //var scaleX = options.width * options.width / element.width();
            //var scaleY = options.height * options.height / element.height();
            //var viewBox = 0 + " " + 0 + " " + scaleX + " " + scaleY;
            //$(self.chart).svgAttr({
            //    viewBox: viewBox
            //});
            self._initVmlCss();
            self._drawLabel();
            self._initChart();
        },

        _drawLabel: function () {
            var self = this, options = self.options;
            if (!options.labelArea.show) {
                return;
            }
            if (options.labelArea.position == 'right') {
                if (options.labelArea.useTemplate && options.labelArea.template && options.labelArea.template != $.noop) {
                    self._drawRightTemplate();
                } else {
                    self._drawRight();
                }
            } else if (options.labelArea.position == 'bottom') {
                if (options.labelArea.useTemplate && options.labelArea.template && options.labelArea.template != $.noop) {
                    self._drawBottomTemplate();
                } else {
                    //self._drawBottom();
                }
            }
        },

        _drawRight: function () {
            var self = this, options = self.options, param = self._parameter, constant = self.constant;
            if (!options.labelArea.show) {
                return;
            }
            if (options.labelArea.useTemplate && options.labelArea.template && options.labelArea.template != $.noop) {
                self._drawRightTemplate();
                return;
            }

            var width = param.width;
            var height = param.height;
            var isAutoWidth = true;
            param.right = width;
            if (options.labelArea.width) {
                isAutoWidth = false;
                param.right = options.labelArea.width;
            }
            var lx = width - param.right,
                ly = 20;
            self.labels = $$.g({ 'class': "chart-labels-group" });
            $(self.chart).append(self.labels);
            $(self.labels).svgTranslate(lx, 0);

            //画右侧注释
            var labelList1 = [],
                labelList2 = [];
            var rectWidth = constant.RIGHTRECTWIDTH,
                tHeight = 12,
                tGap = 10,
                tGGap = tHeight + tGap,
                colCount = 0;

            var x = 0, i = 0, endI = 0;
            var length = options.datas.length;
            do {
                labelList1 = [];
                labelList2 = [];
                for (i = endI; ; i++) {
                    var y = ly + tGGap * (i - endI);
                    if (y > height - 20 || i == length) {
                        endI = i;
                        colCount++;
                        break;
                    }

                    var pieicon = $$.rect(x, y, rectWidth, rectWidth);
                    $(pieicon).svgAttr({
                        stroke: "#5B5B5B",
                        strokeWidth: 0
                    });
                    var color = options.datas[i].color || self._getColor(i);
                    $(pieicon).svgFill(color);
                    $(self.labels).append(pieicon);

                    var ty = y + rectWidth / 2 + 3;
                    var pielabel = $$.text(options.datas[i].name, x + rectWidth + 5, ty, "", 12);
                    $(pielabel).svgAttr(self._style.RightLabelTextStyle);
                    $(self.labels).append(pielabel);
                    labelList1.push(pielabel);

                    if (options.labelArea.showValue) {
                        var labelData = $$.text(options.datas[i].data, 0, ty, "", 12);
                        $(labelData).svgAttr(self._style.RightLabelTextStyle);
                        labelList2.push(labelData);
                        $(self.labels).append(labelData);
                    }
                }
                var twmax = 0, twmax2 = 0, twidth, text;
                for (i = 0; i < labelList1.length; i++) {
                    text = labelList1[i];
                    if (!$$.isIE8()) {
                        twidth = text.getComputedTextLength();
                    } else {
                        twidth = $(text).width();
                    }
                    twmax = Math.max(twmax, twidth);
                }
                if (options.labelArea.showValue) {
                    for (i = 0; i < labelList2.length; i++) {
                        text = labelList2[i];
                        $(text).svgAttr({ x: x + rectWidth + 5 + twmax + 10 });
                        if (!$$.isIE8()) {
                            twidth = text.getComputedTextLength();
                        } else {
                            twidth = $(text).width();
                        }
                        twmax2 = Math.max(twmax2, twidth);
                    }
                }
                x += rectWidth + 5 + twmax + twmax2 + 10 + 20;
            } while (endI < length - 1);
            if (isAutoWidth) {
                param.right = x;
                lx = width - param.right;
                $(self.labels).svgTranslate(lx, 0);
            }
            if (colCount == 1) {
                ly = (height - y - ly) / 2;
                $(self.labels).svgTranslate(null, ly);
            }
        },

        _drawRightTemplate: function () {
            var self = this, options = self.options, param = self._parameter, constant = self.constant;
            var width = param.width;
            var height = param.height;
            var isAutoWidth = true;
            param.right = width;
            if (options.labelArea.width) {
                isAutoWidth = false;
                param.right = options.labelArea.width;
            }
            var lx = width - param.right,
                ly = 20;
            self.labels = $$.g({ 'class': "chart-labels-group" });
            $(self.chart).append(self.labels);
            $(self.labels).svgTranslate(lx, 0);

            //画右侧注释
            var labelList1 = [];
            var rectWidth = constant.RIGHTRECTWIDTH,
                tGap = 10;

            var x = 0, y = ly, i = 0, endI = 0, colCount = 1;
            var length = options.datas.length;

            var mainDiv = this.labelDiv = $('<div></div>');
            mainDiv.css(self._style.LabelDivStyle);
            mainDiv.css({
                left: lx + 'px',
                right: '10px'
            });
            self.element.append(mainDiv);

            do {
                labelList1 = [];
                y = ly;
                var div = $('<div></div>');
                div.css(self._style.LabelDivStyle);
                div.css({
                    //width: '135px',
                    left: (x + rectWidth + 5) + 'px',
                    right: 0
                });
                mainDiv.append(div);
                for (i = endI; ; i++) {
                    if (i >= length) {
                        endI = i;
                        break;
                    }
                    var data = options.datas[i];

                    var template = options.labelArea.template(data);
                    var label = $('<span></span>');
                    label.css(self._style.LabelDivStyle);
                    label.css({
                        top: y + 'px'
                    });
                    if (options.labelArea.triggerClickEvent && options.onPieClick) {
                        label.bind('click', data, function (e) {
                            options.onPieClick(e.data);
                        });
                    }
                    label.append(template);
                    div.append(label);
                    var th = label.height();
                    var nextY = y + th + tGap;
                    if (nextY > height) {
                        endI = i;
                        label.remove();
                        colCount++;
                        break;
                    }
                    labelList1.push(label);

                    var recty = y + rectWidth / 2;
                    var pieicon = $$.rect(x, recty, rectWidth, rectWidth);
                    $(pieicon).svgAttr({
                        strokeWidth: 0
                    });
                    var color = data.color || self._getColor(i);
                    $(pieicon).svgFill(color);
                    $(self.labels).append(pieicon);

                    y = nextY;
                }
                var twmax = 0, twidth, text;
                for (i = 0; i < labelList1.length; i++) {
                    text = labelList1[i];
                    twidth = $(text).width();
                    twmax = Math.max(twmax, twidth);
                }
                x += rectWidth + 5 + twmax + 20;
            } while (endI < length);
            if (isAutoWidth) {
                param.right = x;
                lx = width - param.right;
                $(self.labels).svgTranslate(lx, 0);
                mainDiv.css({
                    left: lx + 'px',
                    top: 0 + 'px'
                });
            }
            if (colCount == 1) {
                ly = (height - y - ly) / 2;
                mainDiv.css({
                    top: ly + 'px'
                });
                $(self.labels).svgTranslate(null, ly);
            }
        },

        _drawBottomTemplate: function () {
            var self = this, options = self.options, param = self._parameter, constant = self.constant;
            var width = param.width;
            var height = param.height;
            var isAutoWidth = true;
            param.bottom = height;
            if (options.labelArea.width) {
                isAutoWidth = false;
                param.bottom = options.labelArea.width;
            }
            var ly = height - param.bottom;
            self.labels = $$.g({ 'class': "chart-labels-group" });
            $(self.chart).append(self.labels);
            $(self.labels).svgTranslate(0, ly);

            //画下侧注释
            var rectWidth = constant.RIGHTRECTWIDTH,
                tGap = 10;

            var y = ly, i = 0;
            var length = options.datas.length;

            var mainDiv = this.labelDiv = $('<div></div>');
            mainDiv.css(self._style.LabelDivStyle);
            mainDiv.css({
                top: ly + 'px'
            });
            self.element.append(mainDiv);

            var x1 = width / 12,
                x2 = width / 2 + width / 10,
                w1 = width / 2 - width / 6,
                w2 = width / 2 - width / 10 - 20;
            var div1 = $('<div></div>');
            div1.css(self._style.LabelDivStyle);
            div1.css({
                left: x1 + 'px',
                width: w1 + 'px'
            });
            mainDiv.append(div1);
            var div2 = $('<div></div>');
            div2.css(self._style.LabelDivStyle);
            div2.css({
                left: x2 + 'px',
                width: w2 + 'px'
            });
            mainDiv.append(div2);
            var maxH = 0;
            for (i = 0; i < length; i++) {
                var data = options.datas[i];
                var isLeft = i % 2 == 0;
                var div = isLeft ? div1 : div2;
                var x = isLeft ? x1 : x2;

                var template = options.labelArea.template(data);
                var label = $('<span></span>');
                label.css(self._style.LabelDivStyle);
                label.css({
                    top: y + 'px',
                    left: 0 + 'px'
                });
                if (options.labelArea.triggerClickEvent && options.onPieClick) {
                    label.bind('click', data, function (e) {
                        options.onPieClick(e.data);
                    });
                }
                label.append(template);
                div.append(label);
                var th = label.height();
                maxH = Math.max(maxH, th);

                var rectY = y + rectWidth / 2,
                    rectX = x - rectWidth / 2 - 10;
                var pieicon = $$.rect(rectX, rectY, rectWidth, rectWidth);
                $(pieicon).svgAttr({
                    strokeWidth: 0
                });
                var color = data.color || self._getColor(i);
                $(pieicon).svgFill(color);
                $(self.labels).append(pieicon);

                if (!isLeft) {
                    y += maxH + tGap;
                    maxH = 0;
                }
            }
            if (isAutoWidth) {
                param.bottom = y + maxH + 20;
                var ltop = height - param.bottom;
                $(self.labels).svgTranslate(0, ltop);
                mainDiv.css({
                    top: ltop + 'px'
                });
            }
        },

        _initChart: function () {
            var self = this, options = self.options, param = self._parameter, constant = self.constant;
            var total = 0;

            var width = param.width - param.right;
            var height = param.height - param.bottom;
            var moveR = param.moveR = options.moveR || constant.PIE_MOVER;
            var r = options.r || (Math.min(width - 10, height) - 100) / 2;
            var innerR = options.innerR || r * 0.7;
            var cx = (width) / 2;
            var cy = height / 2;

            var lineLength = (width - 100 - r * 2) / 4;
            var brokenLineLength = options.brokenLine.brokenLineLength || Math.min(lineLength, constant.BROKENLINELENGTH);
            var horizontalLineLength = options.brokenLine.horizontalLineLength || Math.min(lineLength, constant.HORIZONTALLINELENGTH);
            var labelSize = options.brokenLine.labelSize || constant.LABELSIZE;
            var i, notZeroNum = 0;
            for (i = 0; i < options.datas.length; i++) {
                if (options.datas[i].data > 0) {
                    notZeroNum++;
                }
                total += options.datas[i].data;
            }
            var oneNotZero = notZeroNum == 1;

            var angles = [];
            for (i = 0; i < options.datas.length; i++) {
                if (total <= 0) {
                    angles[i] = 0;
                } else {
                    angles[i] = options.datas[i].data / total * Math.PI * 2;
                }
            }
            self.series = $$.g({ 'class': "chart-series-group", cursor: 'pointer' });

            r = param.r = Math.ceil(r);
            innerR = param.innerR = Math.ceil(innerR);
            cx = param.cx = Math.ceil(cx);
            cy = param.cy = Math.ceil(cy);
            brokenLineLength = param.brokenLineLength = Math.ceil(brokenLineLength);
            horizontalLineLength = param.horizontalLineLength = Math.ceil(horizontalLineLength);
            labelSize = param.labelSize = Math.ceil(labelSize);

            $(self.series).svgTranslate(cx, cy);
            self.brokenlines = $$.g({ 'class': "chart-brokenlines-group" });
            $(self.chart).append(self.series);
            $(self.chart).append(self.brokenlines);

            //遍历饼状图的每个分片
            var startangle = 0;
            var brokenLineCoorDinate = [];
            for (i = 0; i < options.datas.length; i++) {
                var dataInfo = options.datas[i];
                var endangle = startangle + angles[i];
                var moveangle = startangle + angles[i] / 2;
                var x1 = r * Math.sin(startangle); //大弧起点X
                var y1 = -r * Math.cos(startangle); //大弧终点Y
                var x2 = r * Math.sin(endangle); //大弧终点X
                var y2 = -r * Math.cos(endangle); //大弧终点Y
                var x3 = innerR * Math.sin(startangle); //小弧终点X
                var y3 = -innerR * Math.cos(startangle); //小弧终点Y
                var x4 = innerR * Math.sin(endangle); //小弧起点X
                var y4 = -innerR * Math.cos(endangle); //小弧起点Y
                var moveX = moveR * Math.sin(moveangle);
                var moveY = -moveR * Math.cos(moveangle);
                var big = 0;
                if (endangle - startangle > Math.PI)
                    big = 1;
                var d;
                if (!$$.isIE8()) {
                    if (oneNotZero) {
                        x2 -= 0.001;
                        x4 -= 0.001;
                    }
                    d = "M " + x3 + "," + y3 + //start at circle center
                        " L " + x1 + "," + y1 + //draw line to (x1,y1)
                        " A " + r + "," + r + //draw an arc of radius r
                        " 0 " + big + " 1 " + //arc details...][poiuye=-]
                        x2 + "," + y2 + //arc goes to (x2,y2)
                        " L " + x4 + "," + y4 + //draw line to (x4,y4)
                        " A " + innerR + "," + innerR + //draw an arc of radius r
                        " 0 " + big + " 0 " + //arc details...][poiuye=-]
                        x3 + "," + y3 + //arc goes to (x3,y3)
                        " Z ";
                } else {
                    x1 = Math.round(x1);
                    y1 = Math.round(y1);
                    x2 = Math.round(x2);
                    y2 = Math.round(y2);
                    x3 = Math.round(x3);
                    y3 = Math.round(y3);
                    x4 = Math.round(x4);
                    y4 = Math.round(y4);
                    var a = (90 - (startangle * 180 / Math.PI)) * Math.pow(2, 16),
                        b = (angles[i] * 180 / Math.PI) * Math.pow(2, 16);
                    a = Math.round(a);
                    b = Math.round(b);
                    d = "M " + x3 + "," + y3 + //start at circle center
                        " L " + x1 + "," + y1 + //draw line to (x1,y1)
                        " AE 0,0 " + //draw an arc of radius r
                        r + "," + r + //draw an arc of radius r
                        " " + a + ",-" + b + //draw an arc of radius r
                        " L " + x4 + "," + y4 + //draw line to (x4,y4)
                        " AE 0,0 " + //draw an arc of radius r
                        innerR + "," + innerR + //draw an arc of radius r
                        " " + (a - b) + "," + b + //draw an arc of radius r
                        " E ";
                }
                var piepath = $$.svgPath(d);
                if ($$.isIE8()) {
                    $(piepath).svgAttr({
                        width: 1,
                        height: 1,
                        coordsize: '1,1'
                    });
                }
                $(piepath).svgAttr({
                    stroke: "white",
                    strokeWidth: 1
                });
                if (options.brokenLine.showZeroLine || dataInfo.data > 0) {
                    var lineText = total <= 0 ? 0 : Math.round(dataInfo.data * 10000 / total) / 100 + " %";
                    if (options.brokenLine.textTemplate && options.brokenLine.textTemplate != $.noop) {
                        lineText = options.brokenLine.textTemplate($.extend({}, dataInfo, { total: total }));
                    }
                    //折线的点
                    brokenLineCoorDinate.push({
                        startX: cx + r * Math.sin(moveangle),
                        startY: cy - r * Math.cos(moveangle),
                        moveangle: moveangle,
                        middleX: cx + (r + brokenLineLength) * Math.sin(moveangle),
                        middleY: cy - (r + brokenLineLength) * Math.cos(moveangle),
                        data: lineText,
                        lineColor: dataInfo.lineColor
                    });
                }
                //点击饼图移动的坐标
                piepath.movetoX = moveX;
                piepath.movetoY = moveY;

                //translateX 的 set get 方法，用于实现动画
                if (!Object.prototype.hasOwnProperty.call(piepath, "translateX")) {
                    Object.defineProperty(piepath, "translateX", {
                        get: function () {
                            return this._translateX;
                        },
                        set: function (val) {
                            $(this).svgTranslate(parseFloat(val).toFixed(2), null);
                            this._translateX = parseFloat(val).toFixed(2);
                        }
                    });
                }

                //translateY 的 set get 方法，用于实现动画
                if (!Object.prototype.hasOwnProperty.call(piepath, "translateY")) {
                    Object.defineProperty(piepath, "translateY", {
                        get: function () {
                            return this._translateY;
                        },
                        set: function (val) {
                            $(this).svgTranslate(null, parseFloat(val).toFixed(2));
                            this._translateY = parseFloat(val).toFixed(2);
                        }
                    });
                }

                //rotateAngle 的 set get 方法，用于实现动画
                if (!Object.prototype.hasOwnProperty.call(piepath, "rotateAngle")) {
                    Object.defineProperty(piepath, "rotateAngle", {
                        get: function () {
                            return parseInt(this._rotateAngle);
                        },
                        set: function (val) {
                            var angle = parseFloat(val).toFixed(2);
                            //$(this).setRotate(angle, 0, 0);
                            //不用公共类里的正则匹配方式，改用简单方式实现旋转，可以解决IE9下旋转卡顿现象
                            if (!$$.isIE8()) {
                                this.setAttribute('transform', 'rotate(' + angle + ')');
                            } else {
                                this.style.rotation = angle + 'deg';
                            }
                            this._rotateAngle = angle;
                        }
                    });
                }

                $(piepath).prop({
                    rotateAngle: -startangle / Math.PI * 180,
                });

                $(piepath).prop({
                    translateX: 0,
                    translateY: 0,
                });
                var color = dataInfo.color || self._getColor(i);
                $(piepath).svgFill(color);
                $(self.series).append(piepath);
                $(piepath).animate({ rotateAngle: 0 }, 800);

                startangle = endangle;

                var template = null;
                if (options.tooltip && options.tooltip.toolTipTemplate && options.tooltip.toolTipTemplate != $.noop) {
                    template = options.tooltip.toolTipTemplate;
                } else if (options.title) {
                    template = options.title;
                }
                var tip = template(dataInfo);
                if (tip == null || tip == undefined) {
                    tip = dataInfo.name + ': ' + dataInfo.data;
                }

                //点击pie图的事件
                $(piepath).on("click.piechart", dataInfo, function (e) {
                    if (!oneNotZero && e.data.data > 0) {
                        $(this).siblings().stop(true, true);
                        $(this).stop(true, true);
                        $(this).siblings().animate({ translateX: 0, translateY: 0 }, 500);
                        if (this.translateX == 0 && this.translateY == 0) {
                            $(this).animate({ translateX: this.movetoX, translateY: this.movetoY }, 500);
                        } else {
                            $(this).animate({ translateX: 0, translateY: 0 }, 500);
                        }
                    }
                    if (options.onPieClick) {
                        options.onPieClick(e.data);
                    }
                    //self._trigger("onPieClick", e, {
                    //    sender: this
                    //});
                }).bind('mousemove', function (e) {
                    self._setTipLocation(e);
                }).bind('mouseover', tip, function (e) {
                    self._showTip(e.data, e);
                }).bind('mouseleave', function () {
                    self._hidenTip();
                });
            }
            if (options.brokenLine.show) {
                //折线
                if (!options.brokenLine.showInside) {
                    var leftArray = [];
                    var rightArray = [];
                    for (i = 0; i < brokenLineCoorDinate.length; i++) {
                        if (brokenLineCoorDinate[i].middleX >= cx) {
                            rightArray.push(brokenLineCoorDinate[i]);
                        } else {
                            leftArray.push(brokenLineCoorDinate[i]);
                        }
                    }
                    rightArray = this._looseArray(rightArray);
                    leftArray = this._looseArray(leftArray);
                    brokenLineCoorDinate = rightArray.concat(leftArray);

                    for (i = 0; i < brokenLineCoorDinate.length; i++) {
                        var hline,
                            labelX,
                            labelMax,
                            start = "start";
                        if (brokenLineCoorDinate[i].startX < cx) {
                            hline = brokenLineCoorDinate[i].middleX - horizontalLineLength;
                            labelX = hline - 10;
                            labelMax = labelX - 10;
                            start = "end";
                        } else {
                            hline = brokenLineCoorDinate[i].middleX + param.horizontalLineLength;
                            labelX = hline + 10;
                            labelMax = param.width - param.right - labelX - 10;
                        }
                        var xx1 = Math.round(brokenLineCoorDinate[i].startX),
                            yy1 = Math.round(brokenLineCoorDinate[i].startY),
                            xx2 = Math.round(brokenLineCoorDinate[i].middleX),
                            yy2 = Math.round(brokenLineCoorDinate[i].middleY);
                        hline = Math.round(hline);
                        var brokenlinepath = "M " + xx1 + "," + yy1 + " L " + xx2 + "," + yy2 + " L" + hline + "," + yy2;
                        var brokenline = $$.svgPath(brokenlinepath);
                        $(brokenline).svgAttr(self._style.BrokenLineStyle);
                        $(self.brokenlines).append(brokenline);

                        var brokenlineLabel = $$.text(brokenLineCoorDinate[i].data, labelX, 0, start, param.labelSize);
                        $(brokenlineLabel).svgAttr(self._style.BrokenLineTextStyle);

                        $(self.brokenlines).append(brokenlineLabel);

                        $$.trimmingSvgText(brokenlineLabel, labelMax);

                        if (!$$.isIE8()) {
                            $(brokenlineLabel).svgAttr({
                                y: brokenLineCoorDinate[i].middleY + param.labelSize * 0.4,
                            });
                        } else {
                            $(brokenlineLabel).svgAttr({
                                y: brokenLineCoorDinate[i].middleY - param.labelSize / 2
                            });
                            if (start == "end") {
                                $(brokenlineLabel).svgAttr({
                                    x: labelX - brokenlineLabel.offsetWidth
                                });
                            }
                        }

                        if (brokenLineCoorDinate[i].lineColor) {
                            $(brokenline).svgAttr({ stroke: brokenLineCoorDinate[i].lineColor });
                            $(brokenlineLabel).svgAttr({ fill: brokenLineCoorDinate[i].lineColor })
                        }

                        $(brokenlineLabel).bind('mousemove', function (e) {
                            self._setTipLocation(e, true);
                        }).bind('mouseover', brokenLineCoorDinate[i].data, function (e) {
                            self._showTip(e.data, e, true);
                        }).bind('mouseleave', function () {
                            self._hidenTip();
                        });
                    }
                } else {
                    for (i = 0; i < brokenLineCoorDinate.length; i++) {
                        moveangle = brokenLineCoorDinate[i].moveangle;
                        var offset = 20;
                        var x = cx + (r - offset) * Math.sin(moveangle),
                            y = cy - (r - offset) * Math.cos(moveangle) + 5;
                        var textLabel = $$.text(brokenLineCoorDinate[i].data, x, y, 'middle', param.labelSize);
                        $(textLabel).svgAttr(self._style.BrokenLineTextStyle);
                        $(self.brokenlines).append(textLabel);

                        $$.trimmingSvgText(textLabel, param.r);

                        if ($$.isIE8()) {
                            var w = $(textLabel).width();
                            $(textLabel).svgAttr({
                                x: x - w / 2,
                                width: w
                            });
                        }

                        if (brokenLineCoorDinate[i].lineColor) {
                            $(textLabel).svgAttr({ fill: brokenLineCoorDinate[i].lineColor });
                        }
                    }
                }
            }

            if (options.innerText.show) {
                var totleText = options.innerText.innerContent;
                if (!totleText) {
                    totleText = total;
                }
                var textSize = options.innerText.innerSize;
                var style1 = {};
                $.extend(style1, self._style.MiddleTextStyle1);
                if (!textSize) {
                    textSize = style1.fontSize || Math.min(innerR - 10, 48);
                }
                style1.fontSize = textSize;
                var ty = cy;
                if (!options.innerText.showTotal) {
                    ty = cy + textSize / 2 - 5;
                }
                var countText = $$.text(totleText, cx, ty, "middle", textSize);
                $(countText).svgAttr(style1);
                $(self.chart).append(countText);

                var twidth = !$$.isIE8() ? countText.getComputedTextLength() : $(countText).width();
                if ($$.isIE8()) {
                    $(countText).svgAttr({
                        x: cx - twidth / 2,
                        width: twidth
                    });
                }

                if (options.innerText.showTotal) {
                    var totalSize = options.innerText.totleSize;
                    var style2 = {};
                    $.extend(style2, self._style.MiddleTextStyle2);
                    if (!totalSize) {
                        totalSize = style2.fontSize || textSize / 2;
                    }
                    style2.fontSize = totalSize;

                    var tx = cx;//+ twidth / 2;
                    var totalText = $$.text(options.innerText.totalContent, tx, cy + totalSize + 5, "middle", totalSize);
                    $(totalText).svgAttr(style2);
                    $(self.chart).append(totalText);

                    if ($$.isIE8()) {
                        twidth = $(totalText).width();
                        $(totalText).svgAttr({
                            x: tx - twidth,
                            width: twidth
                        });
                    }
                }
            }
            //tooltip
            self._initTip();
        },

        //init tooltip
        _initTip: function () {
            var self = this;
            self.tooltips = $('<span></span>');

            var tooltip = self.options.tooltip;
            if (tooltip && tooltip.toolTipStyle != null) {
                if (typeof tooltip.toolTipStyle == "string") {
                    self.tooltips.addClass(tooltip.toolTipStyle);
                } else if (typeof tooltip.toolTipStyle == "object") {
                    self.tooltips.css(tooltip.toolTipStyle);
                }
            } else {
                self.tooltips.css(self._style.TooltipStyle_Black);
            }

            self.tooltips.css({
                display: "none",
                position: "absolute"
            });

            self.element.append(self.tooltips);

            self.tooltips_label = $('<span></span>');
            self.tooltips_label.css(self._style.TooltipLabelStyle);
            self.tooltips_label.css("display", "none");
            self.element.append(self.tooltips_label);
        },

        //show tooltip
        _showTip: function (tip, e, isLabel) {
            var self = this;
            if (self._appsCheckIsMobile()) {
                return;
            }
            if (!isLabel) {
                self.tooltips.empty();
                self.tooltips.append(tip);
                self.tooltips.css("display", "block");
            } else {
                self.tooltips_label.empty();
                self.tooltips_label.append(tip);
                self.tooltips_label.css("display", "block");
            }
            self._setTipLocation(e, isLabel);
        },

        //set tooltip location
        _setTipLocation: function (e, isLabel) {
            var self = this, param = self._parameter,
                tip = !isLabel ? self.tooltips : self.tooltips_label;
            if (tip.css('display') == 'none') {
                return;
            }
            var mouseX;
            var mouseY;
            if (e.pageX) {
                var parentoffsetX = self.element.offset().left;
                var parentoffsetY = self.element.offset().top;
                mouseX = e.pageX - parentoffsetX;
                mouseY = e.pageY - parentoffsetY;
            } else {
                mouseX = e.offsetX;
                mouseY = e.offsetY;
            }
            var mX = mouseX + 15,
                mY = mouseY,
                tipWidth = tip.width() + 23,
                tipHeight = tip.height() + 16;
            var maxX = param.width - tipWidth - 3,
                maxY = param.height - tipHeight - 3;
            if (mX > maxX) {
                mX = mouseX - tipWidth - 8;
            }
            if (mY > maxY) {
                mY = mouseY - tipHeight + 20;
            }
            tip.css({
                left: mX + 'px',
                top: mY + 'px'
            });
        },

        //hiden tooltip
        _hidenTip: function () {
            var self = this;
            //self.tooltips.empty();
            self.tooltips.css("display", "none");
            //self.tooltips_label.empty();
            self.tooltips_label.css("display", "none");
        },

        _getColor: function (index) {
            var self = this, constant = self.constant;
            index = index % constant.DEFAULT_COLORS.length;
            return constant.DEFAULT_COLORS[index];
        },
        //初始化vml namespaces、css for IE8
        _initVmlCss: function () {
            if ($$.isIE8() && !document.namespaces.v) {
                document.namespaces.add('v', 'urn:schemas-microsoft-com:vml');
                var css = 'v\\:group, v\\:line, v\\:oval, v\\:rect, v\\:shape, v\\:polyline, v\\:fill, v\\:path, v\\:shape, v\\:stroke, v\\:textbox' +
                    '{ behavior:url(#default#VML); display: inline-block; position: absolute;} ';
                try {
                    document.createStyleSheet().cssText = css;
                } catch (e) {
                    document.styleSheets[0].cssText += css;
                }
            }
        },
        //用于将数组疏散，防止折线注释过于密集
        _looseArray: function (array) {
            var self = this, param = self._parameter, i, changeY, yLength, xLength;
            for (i = 0; i < array.length; i++) {
                if (i == 0) { continue; };
                //圆心右侧的点
                if (array[i].middleX >= param.cx && array[i - 1].middleX >= param.cx) {
                    if (array[i].middleY < array[i - 1].middleY + param.labelSize && array[i - 1].middleY + param.labelSize <= param.cy + param.r + param.brokenLineLength) {
                        changeY = array[i - 1].middleY + param.labelSize;
                        yLength = Math.abs(changeY - param.cy);
                        xLength = Math.sqrt((param.r + param.brokenLineLength) * (param.r + param.brokenLineLength) - yLength * yLength);
                        array[i].middleX = param.cx + xLength;
                        array[i].middleY = changeY;
                    }
                }
                //圆心左边的点
                if (array[i].middleX <= param.cx && array[i - 1].middleX <= param.cx) {
                    if (array[i].middleY > array[i - 1].middleY - param.labelSize && array[i - 1].middleY - param.labelSize >= param.cy - param.r - param.brokenLineLength) {
                        changeY = array[i - 1].middleY - param.labelSize;
                        yLength = Math.abs(changeY - param.cy);
                        xLength = Math.sqrt((param.r + param.brokenLineLength) * (param.r + param.brokenLineLength) - yLength * yLength);
                        array[i].middleX = param.cx - xLength;
                        array[i].middleY = changeY;
                    }
                }
            }
            //反向松散
            for (i = array.length - 1; i >= 0; i--) {
                if (i == array.length - 1) { continue; };
                //圆心右侧的点
                if (array[i].middleX >= param.cx && array[i + 1].middleX >= param.cx) {
                    if (array[i].middleY > array[i + 1].middleY - param.labelSize && array[i + 1].middleY - param.labelSize >= param.cy - param.r - param.brokenLineLength) {
                        changeY = array[i + 1].middleY - param.labelSize;
                        yLength = Math.abs(changeY - param.cy);
                        xLength = Math.sqrt((param.r + param.brokenLineLength) * (param.r + param.brokenLineLength) - yLength * yLength);
                        array[i].middleX = param.cx + xLength;
                        array[i].middleY = changeY;
                    }
                }
                //圆心左边的点
                if (array[i].middleX <= param.cx && array[i + 1].middleX <= param.cx) {
                    if (array[i].middleY < array[i + 1].middleY + param.labelSize && array[i + 1].middleY + param.labelSize <= param.cy + param.r + param.brokenLineLength) {
                        changeY = array[i + 1].middleY + param.labelSize;
                        yLength = Math.abs(changeY - param.cy);
                        xLength = Math.sqrt((param.r + param.brokenLineLength) * (param.r + param.brokenLineLength) - yLength * yLength);
                        array[i].middleX = param.cx - xLength;
                        array[i].middleY = changeY;
                    }
                }
            }
            return array;
        },
        _setOption: function (key, value) {
            this.options[key] = value;
            switch (key) {
                case "datas":
                    $(this.chart).empty();
                    if (this.tooltips) {
                        this.tooltips.remove();
                    }
                    if (this.labelDiv) {
                        this.labelDiv.remove();
                    }
                    this._drawLabel();
                    this._initChart();
            }
        },
        _appsCheckIsMobile: function () {
            var ua = navigator.userAgent;
            var ipad = ua.match(/(iPad).*OS\s([\d_]+)/),
                isIphone = ua.match(/(iPhone\sOS)\s([\d_]+)/),
                isAndroid = ua.match(/(Android)\s+([\d.]+)/),
                isMobile = ipad || isIphone || isAndroid;
            return !!isMobile;
        },
        destroy: function () {
            this.element.off();
            this.element.remove();
        }
    });
})(AUI, jQuery);