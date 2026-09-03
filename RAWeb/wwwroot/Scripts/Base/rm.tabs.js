/*Covered by AvePoint copyright and license agreement*/
(function ($) {
    $.fn.extend({
        rm_tabs: function (options) {
            var defaultOptions = {
                isCheckBox: true,
                selectedIndex: 0,
                dataSource: [],
                check: $.noop(),
                uncheck: $.noop()
            };
            var opts = $.extend({}, defaultOptions, options);
            var element = $(this);
            var obj = {
                elementId: element.attr("id"),
                name: "",
                $tabContainer: "",
                styleClassName: {
                    tabNormal: "rm-tabs-normal",
                    tabSelected: "rm-tabs-current",
                    checkedBox: "rm-tabs-checkbox-check",
                    uncheckedBox: "rm-tabs-checkbox-uncheck",
                    normalCheckBoxDiv: "rm-tabs-checkbox-box",
                    disabledCheckBoxDiv: "rm-tabs-checkbox-box-disabled"
                },
                _curTabItem: null,
                _panelIds: [],
                _initTabWidget: function () {
                    var self = this;
                    self.$tabContainer = $("<div/>").attr("id", self.elementId + "_tab_container");
                    self._initTabItems();
                    element.append(self.$tabContainer);
                    self.name = "$rm_tabs_" + self.elementId;
                },
                _initTabItems: function () {
                    var self = this,
                        indexs = [];
                    if ($.isArray(opts.dataSource) && opts.dataSource.length > 0) {
                        opts.dataSource.forEach(function (item, index) {
                            var $item = self._createTabItem(item, index);
                            if ($item.attr("data-visiable") == 1) {
                                $item.css("display", "inline-block");
                                indexs.push(index);
                            } else {
                                $item.css("display", "none");
                            }
                            self._panelIds.push(item.panelId);
                            self.$tabContainer.append($item);
                        });
                    }
                    if (indexs.length == 1) {
                        self._setTabSelected(indexs[0]);
                        self._setCheckBoxSelected(indexs[0]);
                    }
                },
                _createTabItem: function (item, index) {
                    var self = this,
                        $tabItem = $("<div/>").addClass(self.styleClassName.tabNormal).attr("tabindex", 0).attr("data-panel", item.panelId);

                    if (opts.selectedIndex == index) {
                        $tabItem.addClass(self.styleClassName.tabSelected);
                    }
                    if (opts.isCheckBox) {
                        var $checkBoxDiv = $("<div class='rm-tabs-checkbox-box rm-tabs-checkbox-box-base' role='checkbox' tabindex='0'><div class='rm-tabs-checkbox-check-box'><img src='../../aui/image/checkbox.png' class='" + self.styleClassName.uncheckedBox + "'><div/></div>");
                        $checkBoxDiv.on("click", self, self._event.checkBoxClick).on("keydown", self, self._event.checkBoxKeyDown);
                        $tabItem.append($checkBoxDiv).on("click", self, self._event.tabClick);
                    }
                    $tabItem.append(item.title).attr("data-value", item.value).attr("data-visiable", item.visiable ? 1 : 0);
                    return $tabItem;
                },
                _event: {
                    checkBoxClick: function (e) {
                        var self = e.data,
                            $img = $(this).find("img");
                        if ($img.closest("." + self.styleClassName.disabledCheckBoxDiv).length > 0) {
                            e.preventDefault();
                            return false;
                        }
                        $img.toggleClass(self.styleClassName.checkedBox);
                        $img.toggleClass(self.styleClassName.uncheckedBox);

                        
                        if ($img.hasClass(self.styleClassName.checkedBox)) {
                            var pIndex = $(e.target).closest("." + self.styleClassName.tabNormal).index();
                            self._setTabSelected(pIndex);
                            var $parentTab = $img.closest("." + self.styleClassName.tabSelected);
                            var panelId = $parentTab.attr("data-panel");
                            self._showPanel(panelId);
                            opts.check(e);

                        } else {
                            opts.uncheck(e);
                        }
                        e.stopPropagation();
                    },
                    checkBoxKeyDown: function (e) {
                        var self = e.data;
                        if (e.keyCode == 32) {
                            $(e.target).trigger("click");
                        }
                    },
                    tabClick: function (e) {
                        var self = e.data;
                        if ($(e.target).hasClass(self.styleClassName.tabSelected) || $(e.target).find("." + self.styleClassName.disabledCheckBoxDiv).length > 0) {
                            return true;
                        }
                        $(e.target).addClass(self.styleClassName.tabSelected).siblings().removeClass(self.styleClassName.tabSelected);

                        var panelId = $(e.target).attr("data-panel");
                        self._showPanel(panelId);
                        e.stopPropagation();
                    }
                },
                _setCheckBoxSelected: function (index) {
                    var self = this,
                        $tabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal).eq(index);
                    var $checkBox = $tabItem.find("." + self.styleClassName.uncheckedBox);
                    $checkBox.removeClass(self.styleClassName.uncheckedBox).addClass(self.styleClassName.checkedBox);
                    self._curTabItem = $tabItem;
                },
                _setTabSelected: function (index) {
                    var self = this,
                       $tabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal).eq(index);
                    $tabItem.addClass(self.styleClassName.tabSelected)
                            .siblings().removeClass(self.styleClassName.tabSelected);
                    self._curTabItem = $tabItem;
                },
                _showPanel: function (panelId) {
                    var self = this;
                    self._panelIds.forEach(function (id) {
                        if (id == panelId) {
                            $("#" + panelId).show();
                        } else {
                            $("#" + id).hide();
                        }
                    });
                },
                _showPanelByCurTabItem:function () {
                    var self = this;
                    if (self._curTabItem) {
                        var panelId = self._curTabItem.attr("data-panel");
                        self._showPanel(panelId);
                    }
                },
                _showTabByIndex: function (index) {
                    var self = this;
                    var $curTabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal);
                    $curTabItem.eq(index).attr("data-visiable", 1).show();

                },
                _hideTabByIndex: function (index) {
                    var self = this;
                    var $curTabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal);
                    $curTabItem.eq(index).attr("data-visiable", 0).hide();
                },
                _reset: function () {
                    var self = this;
                    self._resetCheckBoxStatus();
                },
                _resetCheckBoxStatus: function () {
                    var self = this;
                    //$tabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal).eq(index);
                    var $checkBox = self.$tabContainer.find("." + self.styleClassName.checkedBox);
                    $checkBox.removeClass(self.styleClassName.checkedBox).addClass(self.styleClassName.uncheckedBox);
                },
                //_setCheckBoxSelectedStatus: function (index, status) {
                //    var self = this,
                //        indexs = [];
                //    if (arguments.length == 0) {
                //        return;
                //    }
                //    if (arguments.length > 1) {
                //        if (typeof arguments[0] === "Array") {
                //            indexs = arguments[0];
                //            indexs.forEach(function (idx) {
                //                var $tabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal).eq(idx);
                //                var $checkBox = $tabItem.find("." + self.styleClassName.uncheckedBox);
                //                $checkBox.removeClass(self.styleClassName.uncheckedBox).addClass(self.styleClassName.checkedBox);
                //            });
                //        } else {
                //            var $tabItem = self.$tabContainer.find("." + self.styleClassName.tabNormal).eq(idx);
                //            var $checkBox = $tabItem.find("." + self.styleClassName.uncheckedBox);
                //            $checkBox.removeClass(self.styleClassName.uncheckedBox).addClass(self.styleClassName.checkedBox);
                //        }
                //    } else {

                //    }
                //},
                //_selectAllCheckBox: function () {
                //    var self = this;
                //    var $checkBox = self.$tabContainer.find("." + self.styleClassName.checkedBox);
                //    $checkBox.removeClass(self.styleClassName.checkedBox).addClass(self.styleClassName.uncheckedBox);
                //},
                //_noSelectAllCheckBox: function () {
                //    var self = this;
                //    var $checkBox = self.$tabContainer.find("." + self.styleClassName.checkedBox);
                //    $checkBox.removeClass(self.styleClassName.checkedBox).addClass(self.styleClassName.uncheckedBox);
                //},
                _getSelectedValues: function () {
                    var self = this,
                        results = [],
                        $checkBoxs = self.$tabContainer.find("." + self.styleClassName.checkedBox);
                    $checkBoxs.each(function () {
                        var $tabItem = $(this).closest("." + self.styleClassName.tabNormal);
                        if ($tabItem.attr("data-visiable") == 1) {
                            results.push($tabItem.attr("data-value"));
                        }
                    });
                    return results;
                },
                _enableAllCheckBox: function (status) {
                    var self = this;
                    var $enabledDivs = self.$tabContainer.find("." + self.styleClassName.normalCheckBoxDiv);
                    var $disabledDivs = self.$tabContainer.find("." + self.styleClassName.disabledCheckBoxDiv);
                    if (status == true) {
                        $disabledDivs.removeClass(self.styleClassName.disabledCheckBoxDiv).addClass(self.styleClassName.normalCheckBoxDiv);
                        $disabledDivs.attr("tabindex", "0");
                        self.$tabContainer.find("." + self.styleClassName.tabNormal).attr("tabindex", "0");
                    } else {
                        $enabledDivs.addClass(self.styleClassName.disabledCheckBoxDiv).removeClass(self.styleClassName.normalCheckBoxDiv);
                        $enabledDivs.attr("tabindex", "-1");
                        self.$tabContainer.find("." + self.styleClassName.tabNormal).attr("tabindex", "-1");
                    }
                }
            };
            obj._initTabWidget();
            window[obj.name] = {
                ShowTab: function (idx) {
                    obj._showTabByIndex(idx);
                },
                HideTab: function (idx) {
                    obj._hideTabByIndex(idx);
                },
                SetTabSelected: function (idx) {
                    obj._setTabSelected(idx);
                    obj._showPanelByCurTabItem();
                },
                GetSelectedValues: function () {
                    return obj._getSelectedValues();
                },
                SetCheckBoxSelected: function (idx) {
                    obj._setCheckBoxSelected(idx);
                },
                Reset: function () {
                    obj._reset();
                },
                EnableAllCheckBox: function (status) {
                    obj._enableAllCheckBox(status);
                }
            };
        }
    });
})(jQuery);