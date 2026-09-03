import React, { useCallback, useEffect, useRef, useState } from "react";
import _ from "lodash";
import "../../../Less/RC/soReport.less";
import { DefaultPager, RAMessageType, ReportType, ReportTitle } from "./config";
import { EnvironmentHelper, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import ReportTable from "./ReportTable";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import { TimeRange } from "./TimeRange";
import ArchivedSitesForTeams from './Teams/ArchivedSites';
import ArchivedSitesForGoogle from './Google/ArchivedSites';
import { formatLocalDay } from "../../../Utilities/DateUtil";

const isNewOpusAccount = LicenseHelper.EnableRecordsArchiver();
const is21VEnv = LicenseHelper.Is21VEnv();
const isGccEnv = EnvironmentHelper.IsGovAzureEnv;

const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Size,
        width: [150],
        resizeable: true,
    },
    {
        header: <div className="flex align-center">
            {RMResx.RM_DSB_Column_Deleted_Size}
            <$g.Popover>{RMResx.RM_DSB_Column_Deleted_Size_Note}</$g.Popover>
        </div>,
        width: [150],
        resizeable: true,
    },
];

const NewTableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [180],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_External_Archived_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_External_Archived_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_Destroyed_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_Destroyed_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_M365_Archived_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_M365_Archived_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    }
];

const GetExportRequestOption = (info, reportType) => {
    if (reportType === ReportType.DedupData) {
        const startTime = new Date(info.StartTime);
        const endTime = new Date(info.EndTime);
        return {
            url: "/api/Dashboard/RunArchiverDeduplicationReportJob",
            data: {
                DedupFrom: formatLocalDay(startTime, "00:00:00"),
                DedupTo: formatLocalDay(endTime, "23:59:59"),
            }
        };
    }
    return {
        url: "/api/Dashboard/RunExportArchiverSiteInfoJob",
        data: info
    }
};

const DefaultExportReportInfo = {
    ReportType: ReportType.None,
    SiteInfos: null,
};

const todayDate = RM.TimeUtil.getTodayStartEndTime();

const ExportSetting = {
    TimeRange: TimeRange.All,
    StartTime: todayDate.start,
    EndTime: todayDate.end,
};

const objectLevelItems = [
    {
        name: RMResx.RM_MA_DocumentAndItem,
        value: ReportType.AllItem,
        checked: true,
    },
    {
        name: RMResx.RM_MA_SubSite,
        value: ReportType.SubSite,
        checked: false,
    },
];

const googleObjectLevelItems = [
    {
        name: RMResx.RM_MA_Document,
        value: ReportType.AllGoogleDriveItems,
        checked: true,
    }
];

const runSOJobRequestOption = {
    url: "/api/Dashboard/IsRunSODashboardJob"
};

const SOReportManagement = () => {

    const refTopButtons = useRef(null);

    const refReportTable = useRef(null);

    const refArchivedSitesForTeams = useRef(null);

    const refArchivedSitesForGoogle = useRef(null);

    const refSitesCheckedCache = useRef([]);

    const refSearchKey = useRef('');

    const [totalCount, setTotalCount] = useState(0);

    const [filterData, setFilterData] = useState(_.cloneDeep(DefaultPager));

    const [sitesChecked, setSitesChecked] = useState([]);

    const [showExportDialog, setShowExportDialog] = useState(false);

    const [isEnableDedup, setIsEnableDedup] = useState(false);

    const [selectSetting, setSelectSetting] = useState(ExportSetting);

    const [startTime, setStartTime] = useState(todayDate.start);

    const [endTime, setEndTime] = useState(todayDate.end);
    
    const [exportReportInfo, setExportReportInfo] = useState(DefaultExportReportInfo);

    const [exportReportInfoForTeams, setExportReportInfoForTeams] = useState(
        DefaultExportReportInfo
    );

    const [exportReportInfoForGoogle, setExportReportInfoForGoogle] = useState(
        DefaultExportReportInfo
    );

    const [isRunSODashboardJob, setIsRunSODashboardJob] = useState(false);

    const [activeTab, setActiveTab] = useState(0);

    const [sourceOptions, setSourceOptions] = useState([]);

    useEffect(() => {
        const url = new URL(window.location.href);
        const activeTab = url.searchParams.get('tab');

        const defaultSourceOptions = getReportTypeOptions().filter((item) => item.isShow);

        if (activeTab) {
            if (!LicenseHelper.HasUpgradeTeams()) {
                window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            } else {
                setActiveTab(Number(activeTab));
                setSourceOptions(defaultSourceOptions.map(item => ({ ...item, checked: item.value === Number(activeTab) })));
            }
        }else{
            setSourceOptions(defaultSourceOptions);
        }
    }, []);

    useEffect(() => {
        if (activeTab === 1) {
            if (LicenseHelper.HasUpgradeTeams()) {
                refArchivedSitesForTeams.current.loadAllTeamsGroups(true, DefaultPager);
            } else {
                refArchivedSitesForGoogle.current.loadAllGoogleData(true, DefaultPager);
            }
        } else if (activeTab === 2) {
            refArchivedSitesForGoogle.current.loadAllGoogleData(true, DefaultPager);
        } else {
            if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
                refArchivedSitesForGoogle.current.loadAllGoogleData(true, DefaultPager);
            } else {
                loadAllSiteCollection(true, DefaultPager);
            }
        }
        checkIsEnableDedup();
        checkSODashboardJobStatus();
    }, [activeTab]);

    const getReportTypeOptions = () => [
        {
            name: RMResx.RM_MA_SharePointSites_Tab,
            value: 0,
            isShow: true,
            component: renderSpOrOD(),
            checked: true,
        },
        {
            name: RMResx.RM_MA_TeamsGroup_Tab,
            value: 1,
            isShow: LicenseHelper.HasUpgradeTeams(),
            component: renderTeams(),
        },
        {
            name: RMResx.RM_MA_Google_Tab,
            value: 2,
            isShow: LicenseHelper.HasOpusGoogleLicense(),
            component: renderGoogle(),
        },
    ];

    const loadAllSiteCollection = async (isResetPagerIndex, paramFilterData) => {
        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/GetArchiverSiteInfoByPager",
            data: paramFilterData || filterData
        };
        const getData = await fetchUtility(requestOption);
        $$.loading(false);
        if (getData) {
            setTotalCount(getData.Count);
            refReportTable.current.setTableInfo({ items: getData.ArchiverSiteSizeInfos, isReset: isResetPagerIndex, });
        }
    };
    
    const checkIsEnableDedup = async () => {
        const requestOption = {
            url: '/api/RetentionApi/IsEnableDeduplication',
            method: 'GET',
        };
        const result = await fetchUtility(requestOption);
        if (result) {
            setIsEnableDedup(true);
            let hasSelectedSites = sitesChecked && sitesChecked.length > 0;
            let showButtons = getShowActions(!hasSelectedSites, !hasSelectedSites, hasSelectedSites);
            refTopButtons.current.updateButtons(showButtons);
        }
    };

    const checkSODashboardJobStatus = useCallback(async () => {
        const isRunSOJob = await fetchUtility(runSOJobRequestOption);
        setIsRunSODashboardJob(isRunSOJob);
    }, []);

    const getShowActions = (showExportSitesBtn, showExportDedupDataBtn, showExportItemBtn) => {
        let buttonsInfo = [
            { isStatic: true, name: RMResx.RM_AR_Report_ExportAllSites, onClick: () => { onExportBtn(ReportType.SiteCollection); }, isShow: showExportSitesBtn },
            { name: RMResx.RM_AR_Report_ExportDedupData, icon: "fia-export-settings", onClick: () => { onExportBtn(ReportType.DedupData); }, isShow: showExportDedupDataBtn },
            { name: RMResx.RM_AR_Report_ExportItem, icon: "fia-export-settings", onClick: () => { onExportBtn(ReportType.AllItemsOrSubSite); }, isShow: showExportItemBtn },
        ];
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    };

    const onExportBtn = (reportType) => {
        setShowExportDialog(true);
        const clonedExportReportInfo = _.cloneDeep(exportReportInfo);
        clonedExportReportInfo.ReportType = reportType;
        if (reportType === ReportType.AllItemsOrSubSite) {
            clonedExportReportInfo.SiteInfos = refSitesCheckedCache.current;
        }
        setExportReportInfo(clonedExportReportInfo);
    };

    const onObjectLevelChanged = (args) => {
        if ((activeTab === 1 && !LicenseHelper.HasUpgradeTeams()) || activeTab === 2 || LicenseHelper.HasOpusGoogleLicenseOnly()) { 
            return onGoogleObjectLevelChanged(args);
        }
        const clonedExportReportInfo = _.cloneDeep(exportReportInfo);
        if (clonedExportReportInfo.ReportType === ReportType.AllItemsOrSubSite) {
            clonedExportReportInfo.ReportType = args.newValue.value;
        }
        setExportReportInfo(clonedExportReportInfo);
    };

    const onGoogleObjectLevelChanged = (args) => {
        const clonedExportReportInfo = _.cloneDeep(exportReportInfoForGoogle);
        clonedExportReportInfo.ReportType = args.newValue.value;
        setExportReportInfoForGoogle(clonedExportReportInfo);
    };

    const onChangeTimeRange = (args) => {
        const argsValue = Number(args);
        const clonedSetting = _.cloneDeep(selectSetting);
        clonedSetting.TimeRange = argsValue;
        
        if (argsValue === TimeRange.Custom) {
            clonedSetting.StartTime = new Date(startTime).toISOString();
            clonedSetting.EndTime = new Date(endTime).toISOString();
        } else {
            clonedSetting.StartTime = new Date(0).toISOString();
            clonedSetting.EndTime = new Date(0).toISOString();
        }
        setSelectSetting(clonedSetting);
    }

    const onChangeRangePicker = (args) => {
        if (_.isNil(args.newValue)) {
            setStartTime(todayDate.start);
            setEndTime(todayDate.end);
        }

        const newStartTimeValue = args.newValue.start;
        const newEndTimeValue = args.newValue.end;
        const clonedSetting = _.cloneDeep(selectSetting);
        clonedSetting.StartTime = new Date(newStartTimeValue).toISOString(),
        clonedSetting.EndTime = new Date(newEndTimeValue).toISOString(),

        setStartTime(newStartTimeValue);
        setEndTime(newEndTimeValue);
        setSelectSetting(clonedSetting);
    }

    const onHideExportDialog = () => {
        setSelectSetting(ExportSetting);
        setShowExportDialog(false);
    }

    const onExportDoAction = async () => {
        if (exportReportInfo.ReportType === ReportType.AllItemsOrSubSite) {
            exportReportInfo.ReportType = ReportType.AllItem;
        }
        exportReportInfo.TimeRange = selectSetting.TimeRange;
        exportReportInfo.StartTime = selectSetting.StartTime;
        exportReportInfo.EndTime = selectSetting.EndTime;
        $$.loading(true);
        const requestOption = GetExportRequestOption(exportReportInfo, exportReportInfo.ReportType);
        const exportState = await fetchUtility(requestOption);
        if (exportState.MessageType === RAMessageType.Successful) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>);
        } else {
            showToast.error(exportState.ErrorMessage);
        }
        $$.loading(false);
        setShowExportDialog(false);
    };

    const onSelectChange = (items) => {
        let itemsSelected = items.length > 0;
        refSitesCheckedCache.current = items;
        setSitesChecked(items);

        let showButtons = getShowActions(!itemsSelected, isEnableDedup && !itemsSelected, itemsSelected);
        refTopButtons.current.updateButtons(showButtons);
    };
    
    const onPageChange = (currentPageIndex, currentPageSize, callback) => {
        const cloneFilterData = _.cloneDeep(filterData);
        cloneFilterData.PageIndex = currentPageIndex;
        cloneFilterData.PageSize = currentPageSize;
        cloneFilterData.SearchKey = refSearchKey.current;
        setFilterData(cloneFilterData);
        callback?.(true);
        if (activeTab === 1) {
            if (LicenseHelper.HasUpgradeTeams()) {
                refArchivedSitesForTeams.current.loadAllTeamsGroups(false, cloneFilterData);
            } else {
                refArchivedSitesForGoogle.current.loadAllGoogleData(false, cloneFilterData);
            }
        } else if (activeTab === 2) {
            refArchivedSitesForGoogle.current.loadAllGoogleData(false, cloneFilterData);
        } else {
            if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
                refArchivedSitesForGoogle.current.loadAllGoogleData(false, cloneFilterData);
            } else {
                loadAllSiteCollection(false, cloneFilterData);
            }
        }
    };

    const onSearch = (args) => {
        let key = args.trim();
        if (key) {
            refSearchKey.current = key;
            onPageChange(0, filterData.PageSize);
        }
    }
    
    const onStopSearch = () => {
        refSearchKey.current = '';
        onPageChange(0, filterData.PageSize);
    }

    const renderSearchBox = () => {
        return (
            <div className="ra-main-header">
                <div className="navbar-search">
                    <R.Searchbox
                        key={activeTab}
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={(args) => (args || "").trim() === "" ? onStopSearch() : onSearch(args)}
                        width={380}
                        height={34}
                    />
                </div>
            </div>
        )
    }

    const renderNavBar = () => {
        let selectCount = RMResx.RM_Common_SelectTableItemsCounter.format(sitesChecked.length, totalCount);
        return <div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={refTopButtons}
                    data={{ menuBtnItems: getShowActions(true, isEnableDedup, false) }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className="ra-main-selected-counter">{selectCount}</div>
        </div>;
    };

    const renderTable = () => {
        const isSupportedNewTableColumn = isNewOpusAccount && !is21VEnv && !isGccEnv;
        const tableColumns = isSupportedNewTableColumn ? NewTableColumns : TableColumns;
        return <div className="ra-main-table">
            <ReportTable
                id="raReportTable"
                ref={refReportTable}
                columns={tableColumns}
                uniqueKey={"SiteId"}
                checkable={true}
                onChange={onSelectChange}
            />
        </div>;
    };

    const renderFooter = () => {
        return <div className="ra-main-footer">
            <$g.Pager
                key={activeTab}
                itemsCount={totalCount}
                pagerIndex={filterData.PageIndex}
                pagerSize={filterData.PageSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={onPageChange} />
        </div>;
    };

    const renderCustomerDateRange = () => {
        return <div className={`margin-top-8 ${exportReportInfo.ReportType !== ReportType.DedupData ? "range-picker" : ""}`}>
            <R.Rangepicker
                selectedDate={(_.isNil(startTime) || _.isNil(endTime)) ? null : {
                    start: startTime,
                    end: endTime,
                }}
                data-part="vtWidget"
                width={374}
                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                onChange={onChangeRangePicker}
                enableDates={{end : new Date()}}
            />
        </div>
    }

    const renderExportDialog = () => {
        const showObjectLevel = 
            exportReportInfo.ReportType === ReportType.AllItemsOrSubSite
            || exportReportInfo.ReportType === ReportType.AllItem
            || exportReportInfo.ReportType === ReportType.SubSite
            || exportReportInfoForGoogle.ReportType === ReportType.AllGoogleDriveItems;
        let reportType, items;
        if (activeTab === 1) {
            reportType = LicenseHelper.HasUpgradeTeams() ? exportReportInfoForTeams.ReportType : exportReportInfoForGoogle.ReportType;
            items = LicenseHelper.HasUpgradeTeams() ? objectLevelItems : googleObjectLevelItems;
        } else if (activeTab === 2) {
            reportType = exportReportInfoForGoogle.ReportType;
            items = googleObjectLevelItems;
        } else {
            reportType = LicenseHelper.HasOpusGoogleLicenseOnly() ? exportReportInfoForGoogle.ReportType : exportReportInfo.ReportType;
            items = LicenseHelper.HasOpusGoogleLicenseOnly() ? googleObjectLevelItems : objectLevelItems;
        }

        return (
            <R.Dialog
                id="exportArchiveSite"
                header={ReportTitle[reportType]}
                width={464}
                status={{ show: showExportDialog }}
                struct={{ foot: true }}
                destroy={true}
                onHide={onHideExportDialog}
                buttons={[
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        disabled: false,
                        onClick: onHideExportDialog,
                    },
                    {
                        text: RMResx.RM_MA_Export,
                        primary: true,
                        classify: "theme",
                        disabled: false,
                        onClick: () => {
                            if (activeTab === 1) {
                                if (LicenseHelper.HasUpgradeTeams()) {
                                    return refArchivedSitesForTeams.current.onExportDoActionForTeams();
                                }
                                return refArchivedSitesForGoogle.current.onExportDoActionForGoogle();
                            }
                            if (activeTab === 2) {
                                return refArchivedSitesForGoogle.current.onExportDoActionForGoogle();
                            }

                            if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
                                return refArchivedSitesForGoogle.current.onExportDoActionForGoogle();
                            }
                            return onExportDoAction();
                        },
                    },
                ]}
            >
                <div id="export-site-dialog">
                    {showObjectLevel && <div className="objectlevel-bottom">
                        <div id="ariaObjectLevel" className="dialog-title">
                            {RMResx.RM_MA_ExportObjectLevel}
                        </div>
                        <div className="margin-top-s">
                            <R.Combobox
                                id="objectLevelCombo"
                                width="100%"
                                items={items}
                                textField="name"
                                valueField="value"
                                searchable={false}
                                onChange={onObjectLevelChanged}
                                aria="#ariaObjectLevel"
                            />
                        </div>
                    </div>}
                    <h4 className="dialog-title">
                        {RMResx.RM_MA_SelectEntendDisposalTime}
                    </h4>
                    {exportReportInfo.ReportType === ReportType.DedupData && renderCustomerDateRange()}
                    {exportReportInfo.ReportType !== ReportType.DedupData && <div>
                        <div className="margin-top-8">
                            <R.Radio
                                name="location-radio"
                                text={RMResx.RM_MA_HistoryExport_All}
                                checked={selectSetting.TimeRange === TimeRange.All}
                                value="1"
                                onChange={onChangeTimeRange}
                            />
                        </div>
                        <div className="margin-top-8">
                            <R.Radio
                                name="location-radio"
                                text={RMResx.RM_MA_HistoryExport_TimeRange_Custom}
                                checked={selectSetting.TimeRange === TimeRange.Custom}
                                value="2"
                                onChange={onChangeTimeRange}
                            />
                            {selectSetting.TimeRange === TimeRange.Custom && renderCustomerDateRange()}
                        </div>
                    </div>}
                </div>
            </R.Dialog>
        )
    }


    const renderSpOrOD = () => {
        return <div>
            <div className="ra-page-container">
                {isNewOpusAccount && renderSearchBox()}
                {renderNavBar()}
                {renderTable()}
                {renderFooter()}
            </div>
        </div>
    }

    const renderTeams = () => {
        return (
            <div>
                <div className="ra-page-container">
                    <ArchivedSitesForTeams
                        ref={refArchivedSitesForTeams}
                        filterData={filterData}
                        selectSetting={selectSetting}
                        exportReportInfoForTeams={exportReportInfoForTeams}
                        setExportReportInfoForTeams={
                            setExportReportInfoForTeams
                        }
                        renderSearchBox={renderSearchBox}
                        renderFooter={renderFooter}
                        setShowExportDialog={setShowExportDialog}
                        setTotalCount={setTotalCount}
                    />
                </div>
            </div>
        );
    };

    const renderSourceSelector = () => {
        return  <div className="report-type-selector">
            <R.Combobox
                width={252}
                items={sourceOptions}
                textField="name"
                valueField="value"
                checkedField="checked"
                searchable={false}
                onChange={(args) => {
                    setActiveTab(args.newValue.value);
                    setFilterData(_.cloneDeep(DefaultPager));
                    refSearchKey.current = '';
                }}
            />
        </div>
    }

    const renderGoogle = () => {
        return <div>
            <div className="ra-page-container">
                <ArchivedSitesForGoogle
                    ref={refArchivedSitesForGoogle}
                    filterData={filterData}
                    selectSetting={selectSetting}
                    exportReportInfoForGoogle={exportReportInfoForGoogle}
                    setExportReportInfoForGoogle={setExportReportInfoForGoogle}
                    renderSearchBox={renderSearchBox}
                    renderFooter={renderFooter}
                    setShowExportDialog={setShowExportDialog}
                    setTotalCount={setTotalCount}
                    totalCount={totalCount}
                />
            </div>
        </div>
    }
    
    const renderContent = () => {
        if (LicenseHelper.HasOpusSOLicense() && (LicenseHelper.HasUpgradeTeams() || LicenseHelper.HasOpusGoogleLicense())) {
            const activeReport = sourceOptions.find((item) => item.value === activeTab)?.component;

            return (
                <div className="raSOReport-content">
                    {renderSourceSelector()}
                    {activeReport}
                </div>
            );
        }

        if (LicenseHelper.HasOpusGoogleLicenseOnly() || (!LicenseHelper.HasOpusSOLicense() && LicenseHelper.HasOpusGoogleLicense())) {
            return (
                <div>
                    <div className="ra-page-container">
                        <ArchivedSitesForGoogle
                            ref={refArchivedSitesForGoogle}
                            filterData={filterData}
                            selectSetting={selectSetting}
                            exportReportInfoForGoogle={exportReportInfoForGoogle}
                            setExportReportInfoForGoogle={setExportReportInfoForGoogle}
                            renderSearchBox={renderSearchBox}
                            renderFooter={renderFooter}
                            setShowExportDialog={setShowExportDialog}
                            setTotalCount={setTotalCount}
                            totalCount={totalCount}
                        />
                    </div>
                </div>
            )
        }

        return (
            <div>
                <div className="ra-page-container">
                    {isNewOpusAccount && renderSearchBox()}
                    {renderNavBar()}
                    {renderTable()}
                    {renderFooter()}
                </div>
            </div>
        );
    }

    return <div id="raSOReport">
        {LicenseHelper.HasOpusGoogleLicenseOnly() ? null : 
            <div className="margin-bottom-m" hidden={isRunSODashboardJob}>
                <R.Messagebar
                    message={RMResx.RM_RC_SOTips}
                    classify="info"
                    hasClose={false}
                    status={{ show: !isRunSODashboardJob }}
                />
            </div>
        }
        {renderContent()}
        {renderExportDialog()}
    </div>;
};

export default SOReportManagement;