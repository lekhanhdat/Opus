/*Covered by AvePoint copyright and license agreement*/
var DayOfWeekOffset = {
    1: 6,
    2: 5,
    3: 4,
    4: 3,
    5: 2,
    6: 1,
    0: 0
}
var DayOfMonthOffset = function (data, year) {
    var temp = {
        1: 31,
        3: 31,
        5: 31,
        7: 31,
        8: 31,
        10: 31,
        12: 31,
        2: -1,
        4: 30,
        6: 30,
        9: 30,
        11: 30
    }
    if (temp[data] == -1) {
        if ((year % 100 != 0 && year % 4 == 0)
            || (year % 400 == 0)) {
            return 29;
        } else {
            return 28;
        }
    }
    return temp[data];
}

var DateRange = {
    
    lst12m: 0,
    lst10w: 1,
    lst10d: 2,
    custom: 3,
}

$$page("rm.board.viewboard", {
    $assigneePieChart: null,
    $cdwlineChart: null,
    $locationBarChart: null,
    $termusageBarChart: null,
    $dateSelectDate: null,
    _BoardData: null,
    _chartData: null,
    _curLineDatas: null,
    _lineChartlabels: null,
    _lineChartsources: null,
    _curViewByMode: 0,
    _isFirstLoad: true,
    _lineMaxCount: 0,
    _curVolumeSource: 1,
    _boardTotalDto: null,
    _prepare: function () {
        
        //$("#rm_dsb_innverContainer").width(1250);
        $("#rm_dsb_container").show();
        $("#rm_dsb_collectionTime").show();
        //RM.Ajax.sendRequest('/api/BoardApi/NeedRunNewJob', function (result) {
        //    if (result) {
        //        var data = JSON.parse(result);
        //        var a = document.createElement("a");
        //        a.setAttribute("href", "/BCM/ContentRepositoryManagement");
        //        a.className = "a_class";
        //        $(a).text(RMResx.RM_SPS_SharePointSettings);//RM_SPS_SharePointSettings
        //        var content = RMResx.RM_JS_DSB_NeedRunNewJob.format(a.outerHTML);
        //        if (data) {
        //            //var content = 'The data in dashboard is collected by Explorer sync job. Please go to <a href="/BCM/ContentRepositoryManagement" class="a_class">Content Repository Management</a> and click "Sync Data to Explorer”.'
        //            $("#messageBar").rmTopMessageBar({
        //                type: 'error',
        //                content: content,
        //                margin: [0, 30, 10, 30],
        //                showClose: true,
        //            });
        //        }
        //    }
        //    RM.hideLoadCover();
        //}, null, 'Get');



        //RM.Ajax.sendRequest('/api/CPApi/LoadDashboardSetting', function (result) {
        //    if (result) {
        //        var data = JSON.parse(result);
        //        if (data && data.isActive) {
        //            $("#rm_dsb_container").show();
        //            $("#rm_dsb_collectionTime").show();
        //        } else {
        //            $("#rm_dsb_not_set_time").show();
        //        }
        //    }
        //    RM.hideLoadCover();
        //}, null, 'Get');
    },
    _create: function () {
        this._initMembers()._initCombox()._getData();
    },
    _bindEvents: function () {
        var self = this;

        return self;
    },
    _initMembers: function () {
        var self = this;
        self.$assigneePieChart = $(".rm-dsb-waiting-chart");
        self.$cdwlineChart = $(".rm-dsb-linechart-body");
        self.$volPieChart = $(".rm-dsb-volumn-chart");
        self.$locationBarChart = $(".rm-dsb-location-body");
        self.$termusageBarChart = $(".rm-dsb-termusage-body");
        self.$dateSelectDate = $("#rm-dsb-linechart-selectDate");
        self.$dateSelectSrc = $("#rm-dsb-linechart-selectSrc");
        self.$usageSelectSrc = $("#rm-dsb-usage-selectSrc");
        self.$locationSelectSrc = $("#rm-dsb-location-selectSrc");
        self.TelemetryModule = {
            None: 0,
            HomePage: 1,
            ContentRepositoryManagement: 2,
            RecordsExplorer: 3,
            PhysicalRecordsExplorer: 4,
            ReportCenter: 5,
            GlobalSearch: 6
        };
        self.TelemetryEventType = {
            None: 0,
            HomepageLoaded: 1,
            ContentPageLoaded: 2,
            ApplySettings: 3,
            RunEnforceRuleActions: 4,
            Search: 5,
            Filter: 6,
            DashboardLoaded: 7,
            CreateContentDueProfile: 8,
            CreateCreationAndDestructionProfile: 9,
            ViewAuditReport: 10
        };
        return self;
    },
    _initWidget: function () {
        var self = this;
        self._initAssignee()._initLineChart()._initBarChart();
        return self;
    },
    _initCombox: function () {
        var self = this;
        var sources = [{ name: RMResx.RM_JS_DSB_Last12Month, ViewBy: "month" }, { name: RMResx.RM_JS_DSB_Last10Weeks, ViewBy: "week" }, { name: RMResx.RM_JS_DSB_Last10Days, ViewBy: "day" }, ];
        self.$dateSelectDate.combobox({
            itemsSource: sources,
            itemTemplateId: "test-template",
            dataTextField: "name",
            dataValueField: "ViewBy",
            popupWidth: 160,
            selectedIndex: 0,
            selectedValue: { name: "Last 12 months", ViewBy: "month" },
            selectionChanged: function (e, args) {
                if (args.oldValue.index < 0 || self._isFirstLoad) {
                    return;
                }
                RM.Cookie.set("RM_ViewByMode", args.newValue.index + "", 7, "/");
                self._curViewByMode = args.newValue.index;
                var mode = {
                    Range: args.newValue.index,
                    SourceFlag: self._curVolumeSource,
                    //StartTime: self._$rangeDate.rangepicker("option", "selectedStartDate"),
                    //EndTime: self._$rangeDate.rangepicker("option", "selectedEndDate")
                }
               
                self._getLineData(mode);
            }
        });
        var sources = [{ name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP" }, { name: RMResx.RM_JS_SPS_TabLabel_FS, ViewBy: "FS" }, { name: RMResx.RM_JS_Common_ReportType_Exchange, ViewBy: "Exchange" }, { name: RMResx.RM_JS_SPS_TabLabel_Physical, ViewBy: "Physical" }, { name: RMResx.RM_JS_SPS_TabLabel_SPLocal, ViewBy: "SPLocal" }, { name: RMResx.RM_JS_SPS_TabLabel_OneDrive, ViewBy: "OneDrive" } ];
        self.$dateSelectSrc.combobox({
            itemsSource: sources,
            itemTemplateId: "test-template",
            dataTextField: "name",
            dataValueField: "ViewBy",
            popupWidth: 160,
            width:190,
            selectedIndex: 0,
            selectedValue: { name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP" },
            selectionChanged: function (e, args) {
                if (args.oldValue.index < 0 || self._isFirstLoad) {
                    return;
                }
                self._curVolumeSource = args.newValue.index + 1;
                var mode = {
                    Range: self._curViewByMode,
                    SourceFlag: self._curVolumeSource,
                }

                self._getLineData(mode);
            }
        });

        var sources = [{ name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP" }, { name: RMResx.RM_JS_SPS_TabLabel_FS, ViewBy: "FS" }, { name: RMResx.RM_JS_Common_ReportType_Exchange, ViewBy: "Exchange" }, { name: RMResx.RM_JS_SPS_TabLabel_Physical, ViewBy: "Physical" }, { name: RMResx.RM_JS_SPS_TabLabel_SPLocal, ViewBy: "SPLocal" }, { name: RMResx.RM_JS_SPS_TabLabel_OneDrive, ViewBy: "OneDrive" }];
        self.$usageSelectSrc.combobox({
            itemsSource: sources,
            itemTemplateId: "test-template",
            dataTextField: "name",
            dataValueField: "ViewBy",
            popupWidth: 160,
            width: 190,
            selectedIndex: 0,
            selectedValue: { name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP" },
            selectionChanged: function (e, args) {
                if (args.oldValue.index < 0 || self._isFirstLoad) {
                    return;
                }
                self._curUsageSource = args.newValue.index + 1;
                var mode = {
                    SourceFlag: self._curUsageSource,
                }

                self._getTermUsageBarData(mode, true);
            }
        });

        var locationSource = [{ name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP", SourceFlag: 1 }, { name: RMResx.RM_JS_SPS_TabLabel_SPLocal, ViewBy: "SPLocal", SourceFlag: 5 }, { name: RMResx.RM_JS_SPS_TabLabel_OneDrive, ViewBy: "OneDrive", SourceFlag: 6 }]
        self.$locationSelectSrc.combobox({
            itemsSource: locationSource,
            itemTemplateId: "test-template",
            dataTextField: "name",
            dataValueField: "ViewBy",
            popupWidth: 160,
            width: 190,
            selectedIndex: 0,
            selectedValue: { name: RMResx.RM_JS_Common_ReportType_SharePoint, ViewBy: "SP" },
            selectionChanged: function (e, args) {
                if (args.oldValue.index < 0 || self._isFirstLoad) {
                    return;
                }
                var mode = {
                    SourceFlag: args.newValue.item.SourceFlag,
                }
                self._getSiteCollectionUsageBarData(mode, true);
            }
        });
        return self;
    },
    _getData: function () {
        var self = this;
        var viewBy = RM.Cookie.get("RM_ViewByMode");
        var range = 0;
        var mode = { LineChartPageMode: { Range: 0, PageIndex: 1, SourceFlag:1 } };
        if (viewBy) {
            range = parseInt(viewBy, 10);
            mode.LineChartPageMode.Range = range;
            
            setTimeout(function () {
                self.$dateSelectDate.combobox("selectedIndex", range);
                self._isFirstLoad = false;
            }, 10);

        } else {
            self._isFirstLoad = false;
        }
        RM.showLoadCover();

        RM.Ajax.sendRequest('/api/BoardApi/GetAllDataInfo', function (result) {
            RM.hideLoadCover();
            if (result) {
                var pData = JSON.parse(result);
                self._BoardData = pData;
                self._chartData = pData.LineChartInfo;
                self.__boardTotalDto = pData.TotalInfo;
                self._curViewByMode = range;
                self._initWidget();
                self._initDataCount();
                self.setChartTextTabIndex();
                self._addTelemetryRecord([pData.TotalInfo.CreatedTotal, pData.SourcePie.Sources]);
            }

            RM.hideLoadCover();
        }, mode, 'Post');
       
       
    },
    _addTelemetryRecord: function (args) {
        var self = this;
        var telemetryDto = {
            Module: self.TelemetryModule.ReportCenter,
            EventType: self.TelemetryEventType.DashboardLoaded,
            Args: args
        };
        RM.Ajax.sendRequest('/api/HomeApi/AddTelemetryRecord', function (result) { }, telemetryDto,'Post');
    },
    _timeMode: function () {
        var self = this;
        var viewBy = "day";
        if (self._curViewByMode == DateRange.lst12m)
        {
            viewBy = "month";
        }
        if (self._curViewByMode == DateRange.lst10w)
        {
            viewBy = "week";
        }
        return viewBy;

    },
    _getLineData: function (mode) {
        var self = this;
        //RM.showLoadCover();
        $$page.rm_dashboard_loadcover("show");
        RM.Ajax.sendRequest('/api/BoardApi/GetLineChartInfo', function (result) {
            if (result) {
                self._chartData = result;
                self._initLineChart();
               // self._initDataCount();
            }

            $$page.rm_dashboard_loadcover("hide");
        }, mode, 'Post');

    },
    __drawLineChart: function () {
        var self = this;
        self.$cdwlineChart.linechart({
            width: 1245, 
            height: 400, 
            leftAxisWidth: 50, 
            bottomAxisHeight: 80,
            series: self._lineChartsources, 
            xAxis: self._lineChartlabels, 
            hasShade: false,
            yAxis: {
                title: "",
                min: 0, 
                max: self._lineMaxCount,
                interval: Math.floor(self._lineMaxCount / 5)
            },
            pointStyle: 'circle-writeborder', 
            tooltip: {
                toolTipTemplate: function (dto) {
                    return $('<div>' + 'Date: ' + dto.data.name + '</br>' + 'Count: ' + dto.data.value + '</div>');
                },
                isToolTipCollapsedAfterClick: true, 
                useDefaultStyle: true, 
                toolTipStyle: null 
            },
            onPointClick: function (dto) { 
                //alert(dto.data.name + ':' + dto.data.value);
            },
            topArea: { hasTopArea: false }, 
            pager: { 
                usePager: true, 
                pageSize: 12, 
                endWithLastPoint: false 
            }
        });
        
        self.$cdwlineChart.linechart("draw");
    },
    _setLineData: function () {
        var self = this;
        
        self._lineChartlabels = [];
        self._lineChartsources = [];
        
        if (self._chartData == null || self._chartData.length == 0
            || self._chartData.ChartInfos == null || self._chartData.ChartInfos.length == 0) {
            return;
        }
        self._lineMaxCount = 0;
        var lineColor = ["#70ad47", "#f7941d", "#448ccb"];
        var curLinDatas = self._chartData.ChartInfos;
        for (var i = 0; i < curLinDatas.length; i++) {
            var srcs = [];
            var line = curLinDatas[i];
            var lineData = self._viewModeChange(line.Nodes, self._timeMode(), self);

            for (var j = 0 ; j < lineData.length ; j++) {
                srcs.push({ value: lineData[j].valueCount, name: lineData[j].LabelStr, val: lineData[j].LabelVal });
                if (i == 0) { self._lineChartlabels.push(lineData[j].LabelStr); }
            }
            self._lineChartsources.push({
                itemsSource: srcs,
                lineColor: lineColor[i],
                lineStrokeWidth: 2,
                pointRadius: 6,
                isVirtual: false,
                lineName: null
            });
        }


    },
    _initDataCount: function () {
        var self = this;

        var data = self.__boardTotalDto;
        $(".rm-dsb-waitingRecord").children(".rm-dsb-top-count").text(data.WaitingTotal);
        $(".rm-dsb-createdRecord").children(".rm-dsb-top-count").text(data.CreatedTotal);
        $(".rm-dsb-destroyedRecord").children(".rm-dsb-top-count").text(data.DestroyTotal);
        if (data.LastJobTime) {
            $("#rm_dsb_time_last").text(data.LastJobTime);
        }
        //if (data.NextJobTime) {
        //    $("#rm_dsb_time_next").text(data.NextJobTime);
        //}
        return self;
    },
    _viewModeChange: function (datas, mode, self) {
        
        if (datas == null || datas.length == 0) {
            return;
        }
        if (mode == "day") {
            self._lineMaxCount = self._chartData.MaxNum;
            return datas;
        }
        var newDatas = [];
        if (mode == "week") {
            
            var j = DayOfWeekOffset[datas[0].dateOfWeek];
            self._calcuWeekCount(datas, newDatas, 0, j, self);
        }
        if (mode == "month") {
            self._calcuMonthCount(datas, newDatas);
        }
        for (var i = 0; i < newDatas.length; i++) {
            self._lineMaxCount = self._lineMaxCount > newDatas[i].valueCount ? self._lineMaxCount : newDatas[i].valueCount;
        }
        return newDatas;
    },
    _calcuWeekCount: function (datas, newDatas, i, j, self, isEnd) {
        var count = 0; var tempEnd = false;
        j = (j >= datas.length - 1 ? datas.length - 1 : j);
        for (var si = i; si <= j; si++) {
            count += datas[si].valueCount
        }
        newDatas.push({
            LabelStr: datas[i].LabelStr + "~" + datas[j].LabelStr,
            valueCount: count
        });
        if (j >= datas.length - 1) {
            return;
        }
        var ni = (j + 1 >= datas.length - 1 ? datas.length - 1 : j + 1);
        var nj = (ni + 6 >= datas.length - 1 ? datas.length - 1 : ni + 6);
        if (nj == datas.length - 1) {
            tempEnd = true;
        }
        self._calcuWeekCount(datas, newDatas, ni, nj, self, tempEnd);

    },
    _calcuMonthCount: function (datas, newMonDatas) {
        if (datas == null || datas.length == 0) {
            return;
        }
        var current = DayOfMonthOffset(datas[0].month, datas[0].year);
        var count = 0;
        var curIndex = 0;
        for (var i = 0; i < datas.length; i++) {
            count += datas[i].valueCount;
            if (datas[i].day == current || i == datas.length - 1) {
                newMonDatas.push({
                    LabelStr: datas[curIndex].LabelStr + "~" + datas[i].LabelStr,
                    valueCount: count
                });
                current = i + 1 >= datas.length - 1
                    ? DayOfMonthOffset(datas[datas.length - 1].month, datas[datas.length - 1].year)
                    : DayOfMonthOffset(datas[i + 1].month, datas[i + 1].year);
                count = 0; curIndex = i + 1;
            }
        }
    },
    _initAssignee: function () {
        var self = this;
        
        var data = self._BoardData.AssigneesDto;
        self.$assigneePieChart.piechart({
            width: 400, 
            height: 470, 
            r: 90,
            innerR: 57.5, 
            moveR: 10,
            datas: data.Assignees, 
            onPieClick: function (data) { 
                console.log("value: " + data.data);
            },
            title: function (data) { },
            tooltip: { 
                toolTipTemplate: function (data) { 
                    return $('<div style="word-break: break-all; word-wrap: break-word;"><b>'
                            + RM.Encoding.htmlEncode(data.name) + ': </b>' + data.data + '</div>');
                }
            },
            innerText: {
                show: true, 
            },
            brokenLine: {
                show: false, 
            },
            labelArea: {
                show: true, 
                position: 'bottom', 
                useTemplate: true, 
                showValue: false, 
                width: undefined,
                template: function (data) {
                    var name = RM.Encoding.htmlEncode(data.name);
                    return $('<div class="rm-dsb-assginee" ><div class="rm-dsb-assginee-title" tabindex="0" title="' + name + '">' + name + '</div><div tabindex="0" class="rm-dsb-assginee-count" title="' + data.data + '">' + data.data + '</div></div>');
                }
            }
        });

        self.$volPieChart.piechart({
            width: 615,
            height: 470,
            r: 90,
            innerR: 57.5,
            moveR: 10,
            datas: self._BoardData.SourcePie.Sources,
            onPieClick: function (data) {
                console.log("value: " + data.data);
            },
            title: function (data) { },
            tooltip: {
                toolTipTemplate: function (data) {
                    return $('<div><b>' + RM.Encoding.htmlEncode(data.name) + ': </b>' + data.data + '</div>');
                }
            },
            innerText: {
                show: true,
            },
            brokenLine: {
                show: false,
            },
            labelArea: {
                show: true,
                position: 'bottom',
                useTemplate: true,
                showValue: false,
                width: undefined,
                template: function (data) {
                    var name = RM.Encoding.htmlEncode(data.name);
                    return $('<div class="rm-dsb-assginee" ><div class="rm-dsb-assginee-title" tabindex="0" title="' + name + '">' + name + '</div><div tabindex="0" class="rm-dsb-assginee-count" title="' + data.data + '">' + data.data + '</div></div>');
                }
            }
        });
        return self;
    },
    generateLine: function (datas, lineColor) {
        var items = [];
        for (var i = 0; i < datas.length; i++) {
            items.push({
                value: datas[i],
                name: ''
            });
        }
        return {
            itemsSource: items,
            lineColor: lineColor,
            lineStrokeWidth: 2,
            pointRadius: 6,
            isVirtual: false,
            lineName: null
        };
    },
    _initLineChart: function () {

        var self = this

        self._setLineData();
        self.__drawLineChart();
        self._initDataCount();

        return self;
    },
    _initBarChart: function () {
        var self = this;
        var mode = { SourceFlag: 1 };
        RM.Ajax.sendRequest('/api/BoardApi/GetSiteCollectionInfo', function (result) {
            if (result) {
                self.createBarChartCommon(self.$locationBarChart, result, "#7eb559", false);
            }
            self.setChartTextTabIndex();
            RM.hideLoadCover();
        }, mode, 'Post');
        self._getTermUsageBarData(mode, false);

        return self;
    },

    _getTermUsageBarData: function (mode, startAnimate) {
        var self = this;
        RM.Ajax.sendRequest('/api/BoardApi/GetTermUsageInfo', function (result) {
            if (result) {
                self.createBarChartCommon(self.$termusageBarChart, result, "#267abf",startAnimate);
            }
            self.setChartTextTabIndex();
            RM.hideLoadCover();
        }, mode, 'Post');
    },

    _getSiteCollectionUsageBarData: function (mode, startAnimate) {
        var self = this;
        RM.Ajax.sendRequest('/api/BoardApi/GetSiteCollectionInfo', function (result) {
            if (result) {
                self.createBarChartCommon(self.$locationBarChart, result, "#7eb559", startAnimate);
            }
            self.setChartTextTabIndex();
            RM.hideLoadCover();
        }, mode, 'Post');
    },

    createBarChartCommon: function ($obj, result, color, sAnimate) {
        var rData = JSON.parse(result)
        var yAxisData = [];
        var seriseContentData = [];
        if (rData.length == 0) {
            if ($obj.barchart) {
                $obj.hide();
            }
            return;
        } else {
            if ($obj.barchart) {
                $obj.show();
            }
           
        }
        for (var i = 0; i < rData.length; i++) {
            var title = RM.Encoding.htmlEncode(rData[i].Title);
            yAxisData.push(title);
            seriseContentData.push({ content: rData[i].Content, tooltip: rData[i].TooltipValue })
        }



        $obj.barchart({
            width: 610,
            height: 460,
            left: 70,
            barHeight: 30,
            gapHeight: 10,
            valuePosition: 'right',
            tooltip: {
                toolTipTemplate: function (item) {
                    return [item.data.tooltip].join("");
                },
                toolTipStyle: "rm-chart-tooltip"
            },
            yAxis: yAxisData,
            xAxis: {
                title: "",
                position: 'top'
            },
            clickCallback: function (data) {
                //alert(JSON.stringify(data));
            },
            hiddenTop: true,
            property: "content",
            series: [
                {
                    name: "",
                    style: {
                        fill: color
                    },
                    styleHighLight: {
                        fill: color
                    },
                    data: seriseContentData
                },
            ],
            animate: {
                //startAnimate: true,
                //showBarLabelAfterAnimate: true
                startAnimate: RM.IsEdge() ? false : true,
                showBarLabelAfterAnimate: false
            }
        });
        setTimeout(function () {
            console.log($(".chart-bars").attr("transform"));
            $(".chart-bars").attr("transform", "translate(70,0)scale(1,1)");
        }, 100);
    },
    setChartTextTabIndex: function () {
        $("svg text").attr("tabindex", "0");
    },
   
});

$$page("rm.dashboard.loadcover", {
    _shownTimes: 0,
    $panel: null,
    _create: function () {
        this._initMembers()
            ._initWidget();
    },
    _initMembers: function () {
        this.$panel = $(".rm-dsb-linechart-container");
        return this;
    },
    _initWidget: function () {
        if (this.$panel.loading) {
            this.$panel.loading();
            $(".aui-loading-content").css("display", "none");
        }
    },
    show: function () {
        if (this.$panel.loading) {
            this.$panel.loading("show");
            this._shownTimes++;
        }
    },
    hide: function () {
        if (this.$panel.loading) {
            this._shownTimes--;
            if (this._shownTimes == 0) {
                this.$panel.loading("hide");
            } else if (this._shownTimes < 0) {
                this._shownTimes = 0;
            }
        }
    }
});