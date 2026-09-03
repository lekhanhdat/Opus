/*Covered by AvePoint copyright and license agreement*/
// Code Review Whole File
$$page("rm.home.modules", {
    _prepare: function () {
        var sizeStr = RM.Cookie.UserInfo.getFontSize();
        sizeStr = sizeStr.substr(0, sizeStr.length - 2);
        var fsize = parseInt(sizeStr, 10);
        var mainWidth = $("body").width(),
            moduleWidth = 14 * fsize + 30 + 50,
            rowCount = Math.floor(mainWidth / moduleWidth);
        $("#rmInnerModules").width(moduleWidth * rowCount);
    },
    _create: function () {
        this._modulesResize()._bindEvents();
    },
    _bindEvents: function () {
        $(window).on('resize', this._modulesResize);
        $$page.rm_Change_Font("setBaseFontChangeCallBack", this._modulesResize);
    },
    _modulesResize: function () {
        var $main = $("body"),
            $innerModule = $("#rmInnerModules"),
            $modules = $(".rm-home-module"),
            moduleWidth = $modules.outerWidth() + 30,
            mainWidth = $main.width(),
            rowCount = Math.floor(mainWidth / moduleWidth),
            totalCount = $modules.length;
        if (rowCount > totalCount) {
            rowCount = totalCount;
        }
        var tempWidth = moduleWidth * rowCount;
        if (tempWidth > $innerModule.width()) {
            $innerModule.width(tempWidth);
        }
        //compute top(css) of the elements which class = 'rm-home-moudule-link-icon'
        var sizeStr = RM.Cookie.UserInfo.getFontSize();
        sizeStr = sizeStr.substr(0, sizeStr.length - 2);
        var fsize = parseInt(sizeStr, 10);
        $(".rm-home-moudule-link-icon").css("top", (fsize * 0.75 - 18) / 2);

        var minHeight = 260,
            tempHeight = minHeight,
            curHeight,
            preRowNum = 0,
            curRowNum = 0,
            maxIdx = $modules.length - 1;
        $modules.removeAttr("style");
        $modules.each(function (idx) {
            curHeight = $(this).height();
            curRowNum = Math.floor(idx / rowCount);
            if (curRowNum == preRowNum) {
                if (curHeight > tempHeight) {
                    tempHeight = curHeight;
                }
                if (maxIdx == idx && tempHeight > minHeight) {
                    for (var i = curRowNum * rowCount; i <= idx; i++) {
                        $modules.eq(i).height(tempHeight);
                    }
                    $modules.eq(i - 1).css("margin-right", 0);
                    tempHeight = minHeight;
                }
            }
            else {
                if (tempHeight > minHeight) {
                    for (var i = preRowNum * rowCount; i < idx; i++) {
                        $modules.eq(i).height(tempHeight);
                    }
                    $modules.eq(i - 1).css("margin-right", 0);
                }
                preRowNum = curRowNum;
                tempHeight = curHeight > minHeight ? curHeight : minHeight;
            }
        });
        if (tempWidth < $innerModule.width()) {
            $innerModule.width(tempWidth);
        }
        return this;
    },

    resize: function () {
        this._modulesResize();
    }
});