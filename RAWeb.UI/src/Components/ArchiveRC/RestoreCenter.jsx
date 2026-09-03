import _ from "lodash";
import "../../Less/ArchiveRC/archiveRestoreCenter.less";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import TopButtonsComponent from "../Common/Util/TopButtonsComponent";
import RestoreCenterTable from "./RestoreCenterTable";
import RestorePanel from "./RestorePanel";
import Paginate from "./Paginate";
import { createRef } from "react";
import SiteMapping from "./SiteMapping";
import Whitelist from "./Whitelist";
import { EnvironmentHelper, LicenseHelper, setCheckedStatusByValue, showToast } from "../../Utilities/CommonUtil";
import { checkPermission } from "../../Utilities/permissionManager";
import RouterUrls from "../../Constants/RouterUrls";
import ViewStatistics from "./Statistics";
import { ActiveTab, DataSourceType, LevelType, ObjectLevelItems, RestoreType, SearchMode, StatisticJobStatus, StatisticMessageType, TeamsObjectLevelItems } from "./Constants";
import { LicenseType, RestoreCenterType } from "../../Constants/Constants";
import { sortRestoreCenterUrlList, sortUrlByArchiveTime } from "./Utils";
import { MessageType } from "../CP/CPConstants";
import AdvanceRestorePanel from "./AdvanceRestorePanel";

const longMaxValue = 2147483647;

const SoftDeleteType = {
    None: 0,
    Yes: 1,
    No: 2,
    All: 3,
}

export default class RestoreCenter extends R.Component {
    constructor(props) {
        super(props);
        this.defaultShowActions = {
            showExportBtn: false,
            showRestoreBtn: false,
            showViewStaticBtn: false,
        };
        this.checkM365License = checkPermission("Source_Teams", RM.UserResources) || checkPermission("Source_SP", RM.UserResources) || checkPermission("Source_OneDrive", RM.UserResources) || checkPermission("Archiver_RestoreCenter_Search", RM.UserResources);
        this.checkFSLicense = (checkPermission("Source_FS", RM.UserResources) || checkPermission("Archiver_RestoreCenter_Search", RM.UserResources)) && (RM.gData.licenseType != LicenseType.Trial) && (!LicenseHelper.HasOpusSOLicenseOnly()) && !EnvironmentHelper.IsGCPEnvironment;
        this.checkTeamsLicense = LicenseHelper.HasUpgradeTeams() && (checkPermission("Source_Teams", RM.UserResources) || checkPermission("Archiver_RestoreCenter_Search", RM.UserResources));
        this.checkGoogleLicense = LicenseHelper.HasOpusGoogleLicense() && (checkPermission("Source_Google", RM.UserResources) || checkPermission("Archiver_RestoreCenter_Search", RM.UserResources));
        this.exactSearchSite = false;
        this.ediscoverExactSearchSite = false;
        this.state = {
            activeTab: ActiveTab.Search,
            itemsChecked: [],
            itemsCount: 0,
            totalCount: 0,
            itemsPagerIndex: 1,
            itemsPagerSize: 10,
            itemsPagerHasNext: false,
            itemsPagerContinuationToken: null,
            itemsPagerCategoryId: longMaxValue,
            items: [],
            allColumns: this.getColumns(),
            showRestorePanel: { show: false },
            showDeleteSCDialog: { show: false },
            showSiteMappingPanel: { show: false },
            showStatisticsPanel: { show: false },
            showWhitelistPanel: { show: false },
            deleteSCConfirmValue: "",
            disableDeleteSCBtn: true,
            statisticInfo: null,
            statisticJobErrorMsg: "",
            showActions: this.defaultShowActions,
            urlList: [],
            urlSelected: {},
            urlInputClassify: "",
            levelItems: this.getLevelItems(),
            levelSelected: this.getSelectedLevelType(),
            softDeleteItems: this.getSoftDeleteItems(),
            softDeleteSelected: SoftDeleteType.All,
            documentName: "",
            documentContent: "",
            createdBy: "",
            modifiedBy: "",
            mainJobId: "",
            createdTimeInfo: {},
            modifiedTimeInfo: {},
            archivedTimeInfo: {},
            archivedTimeInfoForSearchTab: {},
            searchLevel: -1,
            enabledEDiscovery: false,
            simpleSearchValue: "",
            isAdvancedSearch: false,
            permissionType: RestoreCenterType.None,
            dataSourceList: this.getDataSourceOptions(),
            dataSourceFlag: this.checkDataSourceFlag(),
            rerenderControl: 2,
            isSelectedAll: false,
            searchContract: null,
            isShowSelectedAll: false,
            cachedSelectedAllItems: [],
            isSCBlackListForEdiscovery: false,
            showLinkForSC: false,
            siteCollectionAllData: [],
            searchingLevel: null,
            searchAllDate: [],
            isEnableDeleteArchivedSiteCollection: false,
            showAdvanceRestorePanel: { show: false },
        };
        this.filterData = this.getDefaultPager();
        this.cachedFilterData = this.getDefaultPager();
        this.siteLatestArchivedDateCacheRef = createRef();
        this.siteLatestArchivedDateCacheRef.current = new Map();
        this.restoreCenterTableRef = createRef();
        this.cachedPages = new Set();
    }

    componentInit() {
        this.isOnlySupportExactSearchSite();
        this.isEnableDeleteArchivedSiteCollection();
    }

    checkDataSourceFlag = () => {
        let flag = DataSourceType.None;
        if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
            if (this.checkFSLicense && LicenseHelper.HasFileSystemLicense()) {
                return DataSourceType.FS;
            }
            return DataSourceType.Google;
        }
        
        if (this.checkM365License) {
            flag = DataSourceType.M365;
        } else if (this.checkTeamsLicense) {
            flag = DataSourceType.Teams;
        } else if (this.checkFSLicense) {
            flag = DataSourceType.FS;
        } else if (this.checkGoogleLicense) {
            flag = DataSourceType.Google;
        }
        return flag;
    };

    eDiscoveryIsOnlySupportExactSearchSite = () => {
        $$.loading(true);

        let urlData = "/api/ArchiverRestore/EDiscoveryIsOnlySupportExactSearchSite";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option)
        .then((data) => {
            this.ediscoverExactSearchSite =  data || false;
            
            $$.loading(false);
        })
        .catch((e) => {
            $$.loading(false);
        });
    }

    isOnlySupportExactSearchSite = () => {
        $$.loading(true);
        if(!this.checkM365License) {
            this.exactSearchSite = false;

            this.getUrlList();
            
            $$.loading(false);
            return;
        }

        let urlData = "/api/ArchiverRestore/IsOnlySupportExactSearchSite";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option)
            .then((data) => {
                this.exactSearchSite =  data || false;

                this.getUrlList();
                
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    }

    isEnableDeleteArchivedSiteCollection = () => {
        $$.loading(true);
        const urlData = "/api/ArchiverRestore/IsEnableDeleteArchivedSiteCollection";
        const option = {
            url: urlData,
            method: "GET"
        };
        fetchUtility(option)
            .then((data) => {
                this.setState({
                    isEnableDeleteArchivedSiteCollection: !!data,
                });
            })
            .finally(() => {
                $$.loading(false);
            });
    }

    convertSiteCollections = (data) => {
       const siteCollections = data.map((item) => { 
            return{
                PathMd5AndArchiverTime: item.SiteUrl,
                ObjectName: item.SiteUrl,
                Location: item.SiteUrl,
                IsSoftDeleted: false,
                ArchivedTime: item.ArchivedTime,
                PermissionLevel: item.PermissionLevel,
                Origin: item   
            }
       })

       return siteCollections;
    }

    getUrlList = () => {
        if (this.exactSearchSite && this.state.dataSourceFlag == DataSourceType.M365) {
            this.setState({
                urlList: [],
            });
        } else {
            $$.loading(true);
            let urlData = "/api/ArchiverRestore/GetSiteCollectionsInfo";
            let option = {
                url: urlData,
                method: "POST",
                data: this.state.dataSourceFlag,
            };
            fetchUtility(option)
                .then((res) => {
                    if (res) {
                        if (res.length > 0) {
                            if (this.state.dataSourceFlag === DataSourceType.M365) {
                                const sortedUrlList = sortRestoreCenterUrlList(res, 'SiteUrl');
                                this.setState({
                                    urlList: sortedUrlList,
                                    siteCollectionAllData: _.cloneDeep(sortedUrlList),
                                });
                            } else {
                                this.setState({
                                    urlList: res,
                                    siteCollectionAllData: _.cloneDeep(res),
                                });
                            }
                        } else {
                            this.setState({
                                urlList: [],
                                siteCollectionAllData: [],
                            });
                        }
                    }
                    $$.loading(false);
                })
                .catch((e) => {
                    $$.loading(false);
                });
        }

        $$.loading(true);
        fetchUtility({
            url: "/api/ArchiverRestore/IsEnableFullTextIndexSearch",
            method: "POST"
        }).then((res => {
            if(res) {
                const key = "";
                if(!this.siteLatestArchivedDateCacheRef.current.has(key)) {
                    fetchUtility({
                        url: "/api/ArchiverRestore/GetLatestArchiverTime",
                        method: "POST",
                        data: {}
                    }).then((dateStr => {
                        const date = new Date(dateStr);
                        this.siteLatestArchivedDateCacheRef.current.set(key, {
                            start: date.addYears(-1),
                            end: new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 0, 0)
                        });
                        const archivedTimeInfo = this.siteLatestArchivedDateCacheRef.current.get(key);
                        this.setState({
                            archivedTimeInfo: archivedTimeInfo
                        });
                    }));
                }
                this.eDiscoveryIsOnlySupportExactSearchSite();
            }
            this.setState({
                enabledEDiscovery: res,
                isAdvancedSearch: !res || !checkPermission("Archiver_RestoreCenter_Discovery", RM.UserResources),
            });
        })).catch((e) => {
            console.error(e);
        }).finally(() => $$.loading(false));
    };

    getUrlListByContentSearchListlist = () => {
        if (this.ediscoverExactSearchSite) {
            this.setState({
                urlList: [],
            });
            return;
        }
        $$.loading(true);
        const urlData = this.state.isSCBlackListForEdiscovery ? "/api/ArchiverRestore/GetSiteCollectionsInfoByBlacklist" : "/api/ArchiverRestore/GetSiteCollectionsInfoByWhitelist";
        const option = {
            url: urlData,
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                this.setState({
                    urlList: res,
                });
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    };

    getColumns() {
        return [
            {
                header: RMResx.RM_AR_RC_TableCol_Name,
                width: 300,
                resizeable: true,
            },
            {
                header: RMResx.RM_AR_RC_TableCol_Location,
                width: 300,
                resizeable: true,
            },
            {
                header: RMResx.RM_AR_RC_TableCol_CreateDate,
                width: 290,
                resizeable: true,
            },
            {
                header: RMResx.RM_AR_CP_Common_ColName_ModifiedTime,
                width: 290,
                resizeable: true,
            },
            {
                header: RMResx.RM_AR_CP_Common_ColName_ArchivedTime,
                width: 290,
                resizeable: true,
            },
        ];
    }

    mapColumnData(allColumns, resizeColumn, resizeWidth) {
        let newAllColumns = [...allColumns];
        const supportedLevels = new Set([LevelType.Document, LevelType.DocumentVersion]);
        const headersToAdd = [
            RMResx.RM_AR_RC_SearchTitle_CreatedBy,
            RMResx.RM_AR_RC_SearchTitle_ModifiedBy,
            RMResx.RM_AR_RC_SearchTitle_MainJobId,
        ]

        if (this.state.activeTab === ActiveTab.Search && this.state.dataSourceFlag === DataSourceType.M365 && supportedLevels.has(this.state.levelSelected)) {
            // Avoid add duplicate columns
            const alreadyHasSupportedNewCriteriaColumns = headersToAdd.every((item) => newAllColumns.some((col) => col.header === item));

            if (!alreadyHasSupportedNewCriteriaColumns) {
                newAllColumns.splice(5, 0,
                    {
                        header: RMResx.RM_AR_RC_SearchTitle_CreatedBy,
                        width: 290,
                        resizeable: true,
                    },
                    {
                        header: RMResx.RM_AR_RC_SearchTitle_ModifiedBy,
                        width: 290,
                        resizeable: true,
                    },
                    {
                        header: RMResx.RM_AR_RC_SearchTitle_MainJobId,
                        width: 290,
                        resizeable: true,
                    },
                );
            }
        } else {
            newAllColumns = newAllColumns.filter((col) => !headersToAdd.includes(col.header));
        }

        if (resizeColumn) {
            newAllColumns = newAllColumns.map((col) => col.header === resizeColumn.header ? { ...col, width: resizeWidth } : col);
        }
        
        // All columns for enable full text index
        // || this.state.enabledEDiscovery
        if (!RM.gData.enableSoftDelete || this.state.dataSourceFlag === DataSourceType.FS || this.state.activeTab === ActiveTab.EDiscovery) {
            return newAllColumns.filter((col) => col.header !== RMResx.RM_AR_CP_Common_ColName_SoftDeleted);
        }
        
        // For enabled full text index
        const hasSoftDelete = newAllColumns.find((col) => col.header === RMResx.RM_AR_CP_Common_ColName_SoftDeleted);
        if (hasSoftDelete) {
            return newAllColumns;
        }

        let spliceStartIndex = 5; // after ArchivedTime

        if (this.state.dataSourceFlag === DataSourceType.M365 && supportedLevels.has(this.state.levelSelected)) {
            spliceStartIndex = 8; // after CreatedBy, ModifiedBy, MainJobId
        }

        newAllColumns.splice(spliceStartIndex, 0, {
            header: RMResx.RM_AR_CP_Common_ColName_SoftDeleted,
            width: 150,
            resizeable: true,
        });
        this.setState({ allColumns: newAllColumns });
        return newAllColumns;
    }

    mapRowData(searchResults) {
        const { dataSourceFlag, activeTab, levelSelected } = this.state;
        if (searchResults) {
            const supportedDataSources = new Set([DataSourceType.M365, DataSourceType.Teams, DataSourceType.Google]);
            const supportedNewCriteriasLevels = new Set([LevelType.Document, LevelType.DocumentVersion]); // RECO-33302
            return searchResults.map((result) => ({
                ...result,
                HasSoftDelete: activeTab === ActiveTab.Search ? supportedDataSources.has(dataSourceFlag) : false,
                HasNewCriteras: activeTab === ActiveTab.Search && dataSourceFlag === DataSourceType.M365 && supportedNewCriteriasLevels.has(levelSelected),
            }));
        }

        return [];
    }

    getDataSourceOptions() {
        let options = [
            {
                name: RMResx.RM_AR_RC_SearchTitle_DataSource_M365,
                value: DataSourceType.M365,
                checked: false,
                isShow: this.checkM365License && !LicenseHelper.HasOpusGoogleLicenseOnly(),
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_Teams,
                value: DataSourceType.Teams,
                checked: false,
                isShow: this.checkTeamsLicense && !LicenseHelper.HasOpusGoogleLicenseOnly(),
            },
            {
                name: RMResx.RM_AR_RC_SearchTitle_DataSource_FS,
                value: DataSourceType.FS,
                checked: false,
                isShow: this.checkFSLicense && LicenseHelper.HasFileSystemLicense(),
            },
            {
                name: RMResx.RM_AR_RC_SearchTitle_DataSource_Google,
                value: DataSourceType.Google,
                checked: false,
                isShow: this.checkGoogleLicense,
            }
        ];

        let filteredOptions = options.filter(item => item.isShow);
        if (filteredOptions.length > 0) {
            filteredOptions[0].checked = true;
        }

        return filteredOptions;
    }

    getLevelItems() {
        let flag = this.checkDataSourceFlag();
        if (flag === DataSourceType.Teams) {
            return TeamsObjectLevelItems;
        }
        return ObjectLevelItems;
    }

    getSelectedLevelType() {
        let flag = this.checkDataSourceFlag();
        if (flag === DataSourceType.Teams) {
            return LevelType.Teams;
        }
        if (flag === DataSourceType.Google) {
            return LevelType.GoogleDriveDocument;
        }
        return LevelType.Document;
    }

    getSoftDeleteItems() {
        return [
            {
                name: RMResx.RM_AR_RC_SearchTitle_SoftDeleted_All,
                value: SoftDeleteType.All,
                checked: true,
            },
            {
                name: RMResx.RM_AR_RC_SearchTitle_SoftDeleted_Yes,
                value: SoftDeleteType.Yes,
                checked: false,
            },
            {
                name: RMResx.RM_AR_RC_SearchTitle_SoftDeleted_No,
                value: SoftDeleteType.No,
                checked: false,
            },
        ]
    }

    getDefaultPager() {
        let param = {
            PageIndex: 1,
            PageSize: 10,
            TotalNumber: 0,
            ContinuationToken: null,
            CategoryId: longMaxValue,
        };
        return param;
    }

    getShowActions() {
        let { showExportBtn, showRestoreBtn, showDeleteSCBtn } = this.state.showActions;
        let buttonsInfo = [];
        let condition = false;
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) || (!checkPermission(RouterUrls.CP_Index, RM.UserResources) && this.state.permissionType === RestoreCenterType.FullControl)) {
            condition = this.state.activeTab === ActiveTab.Search && this.state.searchContract?.FilterPolicy.DataSource === DataSourceType.M365; // && this.state.searchContract?.FilterPolicy.Level !== LevelType.SiteCollection
            buttonsInfo = [
                {
                    isStatic: true,
                    name: RMResx.RM_AR_RC_ExportBtn,
                    onClick: this.onExport,
                    isShow: showExportBtn,
                },
                {
                    isStatic: !showExportBtn,
                    name: RMResx.RM_AR_RC_RestoreBtn,
                    icon: showExportBtn && "fia-restore",
                    onClick: this.onRestore,
                    isShow: showRestoreBtn,
                },
                // {
                //     name: RMResx.RM_AR_RC_ViewStatisticBtn,
                //     icon: "fia-eye",
                //     onClick: this.onViewStatistics,
                //     isShow: showViewStaticBtn,
                // },
                {
                    name: RMResx.RM_AR_RC_DeleteBtn,
                    icon: 'fia-delete',
                    onClick: this.handleDeleteSC,
                    isShow: showDeleteSCBtn,
                },
            ];
        } else if (this.state.permissionType === RestoreCenterType.SearchAndExport) {
            buttonsInfo = [
                {
                    isStatic: true,
                    name: RMResx.RM_AR_RC_ExportBtn,
                    onClick: this.onExport,
                    isShow: showExportBtn,
                },
                // {
                //     name: RMResx.RM_AR_RC_ViewStatisticBtn,
                //     icon: "fia-eye",
                //     onClick: this.onViewStatistics,
                //     isShow: showViewStaticBtn,
                // },
            ];
        }
        // else {
        //     buttonsInfo = [
        //         {
        //             isStatic: true,
        //             name: RMResx.RM_AR_RC_ViewStatisticBtn,
        //             onClick: this.onViewStatistics,
        //             isShow: showViewStaticBtn,
        //         },
        //     ];
        // }
        let showButtons = buttonsInfo.filter((item) => {
            return item.isShow;
        });
        this.setIsShowSelectedAll(condition);
        return showButtons;
    }

    getSearchValueForExport() {
        // Truthly: > 0, falsy: <= 0
        let createTime =
            Object.keys(this.state.createdTimeInfo).length > 0;
        let modifiedTime =
            Object.keys(this.state.modifiedTimeInfo).length > 0;
        let archivedTime =
            this.state.activeTab === ActiveTab.EDiscovery && Object.keys(this.state.archivedTimeInfo).length > 0;
        let searchValue;

        let searchContract = {
            SearchNode: this.state.urlSelected,
            FilterPolicy: {
                Level: this.state.levelSelected,
                FilterName: this.state.documentName,
                FilterContent: this.state.documentContent,
                FilterMetadataInfo: this.state.documentMetadata,
                CreateStartTime: createTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.createdTimeInfo.start
                    )
                    : "",
                CreateEndTime: createTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.createdTimeInfo.end
                    )
                    : "",
                ModifiedStartTime: modifiedTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.modifiedTimeInfo.start
                    )
                    : "",
                ModifiedEndTime: modifiedTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.modifiedTimeInfo.end
                    )
                    : "",
                ArchivedStartTime: archivedTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.archivedTimeInfo.start
                    )
                    : "",
                ArchivedEndTime: archivedTime
                    ? RM.TimeUtil.getCommonDateStr(
                        this.state.archivedTimeInfo.end
                    )
                    : "",
                FilterDeleteType: this.state.softDeleteSelected,
            },
        }

        // ED tab
        if (this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery) {
            if (this.state.isAdvancedSearch) {
                searchValue = {
                    SerchContract: searchContract,
                    PageIndex: this.filterData.PageIndex,
                    PageSize: this.filterData.PageSize,
                    ContinuationToken: null,
                    CategoryId: longMaxValue,
                    SearchMode: SearchMode.FullTextAdvanceSearch,
                }
            } else {
                searchValue = {
                    SearchMode: SearchMode.FullTextSimpleSearch,
                    archiverRestoreSimpleSearchQueryParameter: {
                        ContinuationToken: null,
                        CategoryId: longMaxValue,
                        PageSize: this.filterData.PageSize,
                        Keyword: this.state.simpleSearchValue,
                        ArchivedStartTime: archivedTime
                            ? RM.TimeUtil.getCommonDateStr(
                                this.state.archivedTimeInfo.start
                            )
                            : "",
                        ArchivedEndTime: archivedTime
                            ? RM.TimeUtil.getCommonDateStr(
                                this.state.archivedTimeInfo.end
                            )
                            : "",
                    },
                }
            }
        } else {
            //Search tab
            searchContract = {
                SearchNode: this.state.urlSelected,
                FilterPolicy: {
                    Level: this.state.levelSelected,
                    FilterName: this.state.documentName,
                    FilterContent: this.state.documentContent,
                    FilterMetadataInfo: this.state.documentMetadata,
                    CreateStartTime: createTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.createdTimeInfo.start
                        )
                        : "",
                    CreateEndTime: createTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.createdTimeInfo.end
                        )
                        : "",
                    ModifiedStartTime: modifiedTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.modifiedTimeInfo.start
                        )
                        : "",
                    ModifiedEndTime: modifiedTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.modifiedTimeInfo.end
                        )
                        : "",
                    ArchivedStartTime: archivedTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.archivedTimeInfo.start
                        )
                        : "",
                    ArchivedEndTime: archivedTime
                        ? RM.TimeUtil.getCommonDateStr(
                            this.state.archivedTimeInfo.end
                        )
                        : "",
                    FilterDeleteType: this.state.softDeleteSelected,
                },
            }
            searchValue = {
                SerchContract: searchContract,
                PageIndex: this.filterData.PageIndex,
                PageSize: this.filterData.PageSize,
                ContinuationToken: null,
                CategoryId: longMaxValue,
                SearchMode: SearchMode.NormalSearch,
            }
        }

        this.setState({
            searchContract,
        });

        return searchValue;
    }

    onShowSiteMappingPanel = () => {
        this.setState({
            showSiteMappingPanel: { show: true },
        });
    }

    onRestore = () => {
        this.setState({
            showRestorePanel: { show: true },
        });
    };

    handleDeleteSC = () => {
        this.setState({
            showDeleteSCDialog: { show: true },
        });
    }

    onDeleteSC = () => {
        if (!$$.verify('confirmDeleteSCValidation')) {
            return;
        }
        const option = {
            url: "/api/ArchiverRestore/RunDeleteArchivedSCJob",
            method: "POST",
            data: this.state.itemsChecked[0].Origin,
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType === MessageType.Successful) {
                    const content = (
                        <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>
                    );
                    showToast.success(content);
                    this.onCancelDeleteSC();
                    this.onSearchData(true);
                } else {
                    showToast.error(res.ErrorMessage);
                }
            })
            .finally(() => $$.loading(false));
    }

    onCancelDeleteSC = () => {
        this.setState({
            showDeleteSCDialog: { show: false },
            deleteSCConfirmValue: "",
            disableDeleteSCBtn: true,
        });
    }

    saveSimulateRestoreSettingAndRun = async () => {
        const option = {
            url: "/api/ArchiverRestore/SaveSimulateRestoreSettingAndRun",
            method: "POST",
            data: this.state.itemsChecked,
        };
        let jobId = "";
        $$.loading(true);
        const res = await fetchUtility(option);
        $$.loading(false);
        jobId = res.Extension;
        this.setState({
            showStatisticsPanel: { show: true },
            statisticInfo: null,
        });
        this.getRunningSimulateRestoreJob(jobId);
    }

    getRunningSimulateRestoreJob = (jobId) => {
        const option = {
            url: `/api/ArchiverRestore/GetSimulateRestoreJobResult?jobId=${jobId}`,
            method: "GET",
        }
        let timerCount = 0;
        const timerId = setInterval(() => {
            let isStopTimer = false;
            ++timerCount;
            fetchUtility(option).then((res) => {
                // Every 5 seconds will add 1, 120 means 10 min
                if (timerCount == 120) {
                    isStopTimer = true;
                }
    
                if (res.Extension == StatisticJobStatus.Failed || res.MessageType == StatisticMessageType.Failed) {
                    isStopTimer = true;
                    this.setState({
                        statisticJobErrorMsg: res.ErrorMessage,
                    });
                }
    
                if (res.Extension == StatisticJobStatus.Finished) {
                    const data = res.Extsion1;
                    isStopTimer = true;
                    this.setState({
                        statisticInfo: {
                            ...data.LevelCountMap,
                            totalSize: data.SizeStr,
                        },
                    });
                }
    
                if (isStopTimer) {
                    clearInterval(timerId);
                }
            });
        }, 5000);
    }

    onViewStatistics = () => {
        const option = {
            url: "/api/ArchiverRestore/HaveRunningSimulateRestoreJob",
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            const isRunning = res.Extsion1;
            if (isRunning) {
                const args = {
                    classify: "warn",
                    width: "550px",
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_AR_RC_Statistic_EnsureRunJobAgain,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false),
                        },
                        {
                            id: "rcStatisticsRunSimulate",
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: this.saveSimulateRestoreSettingAndRun,
                        },
                    ]
                }
                $$.messagedialog(true, args);
            } else {
                this.saveSimulateRestoreSettingAndRun();
            }
        })
    }

    onExport = () => {
        let args = {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_RC_ExportConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                        $$.messagedialog(false);
                        let option = {
                            url: `/api/ArchiverRestore/ExportSearchResult`,
                            method: "post",
                            data: this.getSearchValueForExport(),
                        };
                        fetchUtility(option).then((res) => {
                            if (res.MessageType === 0) {
                                const content = (
                                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                                        <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                                    </$g.I18NProvider>
                                );
                                showToast.success(content);
                            } else {
                                showToast.error(res.ErrorMessage);
                            }
                        }).catch((e) => { });
                    },
                }
            ]
        };
        $$.messagedialog(true, args);
    };

    onPagerChange = (pagerIndex, pagerSize) => {
        this.cachedFilterData.PageIndex = this.filterData.PageIndex;
        this.filterData.PageIndex = pagerIndex;
        this.filterData.PageSize = pagerSize;
        const isResetPager = pagerSize != this.state.itemsPagerSize;
        this.setState({
            itemsPagerIndex: pagerIndex,
            itemsPagerSize: pagerSize,
        });
        this.onSearchData(isResetPager);
    };

    onUrlInputValueChange = (value) => {
        this.setState({
            urlSelected: value.trim() ? { SiteUrl: value } : {},
        });
    }

    onSelUrlChanged = async (args) => {
        let archivedTimeInfo = {};
        if((this.state.levelSelected == LevelType.Document || this.state.levelSelected == LevelType.DocumentVersion) && this.state.enabledEDiscovery) {
            const key = _.isNil(args.newValue) ? "" : args.newValue.SPObjectId + "_" + args.newValue.SiteUrl;
            if(!this.siteLatestArchivedDateCacheRef.current.has(key)) {
                const res = key === "" ? await fetchUtility({
                    url: "/api/ArchiverRestore/GetLatestArchiverTime",
                    method: "POST",
                }) : await fetchUtility({
                    url: "/api/ArchiverRestore/GetSiteLatestArchiverTime",
                    method: "POST",
                    data: {
                        SiteUrl: args.newValue.SiteUrl,
                        SPObjectId: args.newValue.SPObjectId
                    }
                });

                const date = new Date(res);

                this.siteLatestArchivedDateCacheRef.current.set(key, {
                    start: date.addYears(-1),
                    end: date
                });
            }
            archivedTimeInfo = this.siteLatestArchivedDateCacheRef.current.get(key);
        }
        this.setState({
            urlSelected: _.isNil(args.newValue) ? {} : args.newValue,
            archivedTimeInfo: archivedTimeInfo,
            permissionType: _.isNil(args.newValue) ? 1 : args.newValue.PermissionLevel,
        });
    };

    selectUrlValid = () => {
        return Object.keys(this.state.urlSelected).length == 0
            ? RMResx.RM_AR_CP_Common_SelEmpty
            : true;
    };

    onDataSourceChanged = (args) => {
        let cloneLevelSelected = _.cloneDeep(this.state.levelSelected);
        let cloneLeveItems = _.cloneDeep(this.state.levelItems);
        if (args.newValue.value === DataSourceType.Teams) {
            cloneLevelSelected = LevelType.Teams;
            cloneLeveItems = TeamsObjectLevelItems;
        } else if (args.newValue.value === DataSourceType.Google) {
            cloneLevelSelected = LevelType.GoogleDriveDocument;
        } else {
            cloneLevelSelected = LevelType.Document;
            cloneLeveItems = ObjectLevelItems;
        }
        
        if (args.newValue.value !== DataSourceType.M365) {
            this.setIsShowSelectedAll(false);
            this.setState({
                isSelectedAll: false,
            });
        }

        this.setState({
            dataSourceFlag: args.newValue.value,
            items: [],
            itemsChecked: [],
            itemsCount: 0,
            totalCount: 0,
            itemsPagerIndex: 1,
            itemsPagerSize: 10,
            itemsPagerHasNext: false,
            itemsPagerContinuationToken: null,
            itemsPagerCategoryId: longMaxValue,
            urlSelected: {},
            levelItems: cloneLeveItems,
            levelSelected: cloneLevelSelected,
            documentName: "",
            documentContent: "",
            documentMetadata: "",
            createdTimeInfo: {},
            modifiedTimeInfo: {},
            archivedTimeInfo: {},
            showLinkForSC: false,
            createdBy: "",
            modifiedBy: "",
            archivedTimeInfoForSearchTab: {},
            mainJobId: "",
            searchLevel: -1,
            softDeleteSelected: SoftDeleteType.All,
            showActions: {
                showExportBtn: false,
            },
        }, () => {
            this.dispatch("RestoreCenterTable", "", {
                columns: this.state.allColumns,
                items: [],
                isReset: true,
            });
            this.getUrlList();
        });
    }

    onSelLevelChanged = (args) => {
        this.setState((prev) => ({
            levelSelected: args.newValue.value,
            documentName: "",
            createdTimeInfo: {},
            modifiedTimeInfo: {},
            createdBy: "",
            modifiedBy: "",
            archivedTimeInfoForSearchTab: {},
            mainJobId: "",
            archivedTimeInfo: this.state.enabledEDiscovery ? this.state.archivedTimeInfo : {},
            showLinkForSC: false,
            urlSelected: {},
            urlList: prev.urlList.map(({ checked, ...rest }) => rest)
        }), () => {
            this.dispatch("RestoreCenterTable", "", {
                columns: this.state.allColumns,
                items: this.mapRowData(this.state.items),
                isReset: false,
            });
        });
    };

    onSoftDeleteChanged = (args) => {
        this.setState({
            softDeleteSelected: args.newValue.value,
        });
    }

    onDocumentNameChanged = (value) => {
        this.setState({ documentName: value });
    };

    onDocumentContentChanged = (value) => {
        this.setState({ documentContent: value });
    };

    onCreatedByChanged = (value) => {
        this.setState({ createdBy: value });
    };

    onModifiedByChanged = (value) => {
        this.setState({ modifiedBy: value });
    };

    onMainJobIdChanged = (value) => {
        this.setState({ mainJobId: value });
    };

    onDocumentMetadataChanged = (value) => {
        this.setState({ documentMetadata: value });
    };

    onSelCreatedTime = (args) => {
        this.setState({ createdTimeInfo: args.newValue || {} });
    };

    onSelModifiedTime = (args) => {
        this.setState({ modifiedTimeInfo: args.newValue || {} });
    };

    onSelArchivedTime = (args) => {
        this.setState({ archivedTimeInfo: args.newValue || {}});
    }

    onSelArchivedTimeForSearchTab = (args) => {
        this.setState({ archivedTimeInfoForSearchTab: args.newValue || {}});
    }

    setIsShowSelectedAll = (condition) => {
        this.setState({
            isShowSelectedAll: condition,
        });
    }

    searchSite = async () => {
        if (this.state.urlSelected.MasterIndexId) {
            return this.state.urlSelected;
        }
        const siteUrl = this.state.urlSelected.SiteUrl;
        if (siteUrl) {
            const site = await fetchUtility({
                url: "/api/ArchiverRestore/SearchSiteCollectionInfo",
                method: "POST",
                data: siteUrl
            });
            if (site) {
                this.setState({ urlInputClassify: "", urlSelected: site, permissionType: site.PermissionLevel });
                return site;
            }
        } 
        
        if (this.state.levelSelected !== LevelType.SiteCollection) {
            this.setState({ urlInputClassify: "error" });
        }
        return null;
    }

    eDiscoverySearchSite = async () => {
        if (this.state.urlSelected.MasterIndexId) {
            return this.state.urlSelected;
        }
        const siteUrl = this.state.urlSelected.SiteUrl;
        if (siteUrl) {
            const site = await fetchUtility({
                url: "/api/ArchiverRestore/EDiscoverySearchSiteCollectionInfo",
                method: "POST",
                data: siteUrl
            });
            if (site) {
                this.setState({ urlInputClassify: "", urlSelected: site, permissionType: site.PermissionLevel });
                return site;
            }
        } 
        
        this.setState({ urlInputClassify: "error" });
        return null;
    }

    getArchivedTime = (value = 'start' | 'end') => {
        const supportedLevels = new Set([LevelType.Document, LevelType.DocumentVersion]);
        const archivedTime = this.state.activeTab === ActiveTab.Search && supportedLevels.has(this.state.levelSelected) && Object.keys(this.state.archivedTimeInfoForSearchTab).length > 0;
        if (archivedTime) {
            return RM.TimeUtil.getCommonDateStr(this.state.archivedTimeInfoForSearchTab[value]);
        }

        // For eDiscovery advance search
        const archivedTimeForEDiscovery = this.state.activeTab === ActiveTab.EDiscovery && Object.keys(this.state.archivedTimeInfo).length > 0;
        if (archivedTimeForEDiscovery) {
            return RM.TimeUtil.getCommonDateStr(this.state.archivedTimeInfo[value]);
        }
        return "";
    }

    onSearchData = async (isResetPagerIndex) => {
        // this.state.activeTab !== ActiveTab.Search && this.state.levelSelected !== LevelType.SiteCollection && 
        let disableEDiscoveryValid = this.state.levelSelected !== LevelType.SiteCollection && !this.state.enabledEDiscovery && !$$.verify(this.refSelectUrlValid.ref.current);
        let enableEDiscoveryValid = this.state.enabledEDiscovery && 
            ((this.state.activeTab === ActiveTab.Search && ((this.exactSearchSite && this.state.levelSelected === LevelType.SiteCollection) || this.state.levelSelected !== LevelType.SiteCollection) && !$$.verify(this.refSelectUrlValid.ref.current)));
        if (disableEDiscoveryValid || enableEDiscoveryValid) {
            return false;
        }
        $$.loading(true);
        
        let selectedSite = null;
        
        if(this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery && this.ediscoverExactSearchSite && this.state.isAdvancedSearch){
            selectedSite = await this.eDiscoverySearchSite();
        }
        else if(this.exactSearchSite && this.state.dataSourceFlag == DataSourceType.M365){
            selectedSite = await this.searchSite()
        }
        else{
            selectedSite = this.state.urlSelected;
        }

        if (!selectedSite && this.state.isAdvancedSearch) {
            this.setState({
                items: [],
                itemsChecked: [],
                itemsCount: 0,
                totalCount: 0,
                itemsPagerIndex: 1,
                itemsPagerHasNext: false,
                showActions: {
                    showExportBtn: false,
                },
            }, () => {
                this.dispatch("RestoreCenterTable", "", {
                    columns: this.state.allColumns,
                    items: [],
                    isReset: true,
                });
            })
            $$.loading(false);
            return false;
        }

        if (isResetPagerIndex) {
            this.filterData.PageIndex = 1;
            this.filterData.ContinuationToken = null;
            this.filterData.CategoryId = longMaxValue;
            this.setState({ itemsPagerIndex: 1 });
        }

        // Truthly > 0, falsy <= 0
        let createTime =
            Object.keys(this.state.createdTimeInfo).length > 0;
        let modifiedTime =
            Object.keys(this.state.modifiedTimeInfo).length > 0;
        let archivedTime = 
            this.state.activeTab === ActiveTab.EDiscovery && Object.keys(this.state.archivedTimeInfo).length > 0;
        let searchValue;

        const searchContract = {
            SearchNode: selectedSite,
            FilterPolicy: {
                DataSource: this.state.dataSourceFlag,
                Level: this.state.levelSelected,
                FilterName: this.state.documentName,
                FilterContent: this.state.documentContent,
                FilterMetadataInfo: this.state.documentMetadata,
                CreateStartTime: createTime
                    ? RM.TimeUtil.getCommonDateStr(
                            this.state.createdTimeInfo.start
                        )
                    : "",
                CreateEndTime: createTime
                    ? RM.TimeUtil.getCommonDateStr(
                            this.state.createdTimeInfo.end
                        )
                    : "",
                ModifiedStartTime: modifiedTime
                    ? RM.TimeUtil.getCommonDateStr(
                            this.state.modifiedTimeInfo.start
                        )
                    : "",
                ModifiedEndTime: modifiedTime
                    ? RM.TimeUtil.getCommonDateStr(
                            this.state.modifiedTimeInfo.end
                        )
                    : "",
                ArchivedStartTime: this.getArchivedTime('start'),
                ArchivedEndTime: this.getArchivedTime('end'),
                CreatedBy: this.state.createdBy,
                ModifiedBy: this.state.modifiedBy,
                MainJobId: this.state.mainJobId,
                FilterDeleteType: this.state.softDeleteSelected,
            },
        }

        if (this.state.enabledEDiscovery && !this.state.isAdvancedSearch && this.state.activeTab === ActiveTab.EDiscovery) {
            // Simple search
            searchValue = {
                ContinuationToken: this.filterData.ContinuationToken,
                CategoryId: this.filterData.CategoryId,
                PageIndex: this.filterData.PageIndex,
                PageSize: this.filterData.PageSize,
                Keyword: this.state.simpleSearchValue,
                ArchivedStartTime: archivedTime
                            ? RM.TimeUtil.getCommonDateStr(
                                  this.state.archivedTimeInfo.start
                              )
                            : "",
                ArchivedEndTime: archivedTime
                    ? RM.TimeUtil.getCommonDateStr(
                            this.state.archivedTimeInfo.end
                        )
                    : "",
            }
        } else {
            // Advanced search
            searchValue = {
                SerchContract: searchContract,
                PageIndex: this.filterData.PageIndex,
                PageSize: this.filterData.PageSize,
                ContinuationToken: this.filterData.ContinuationToken,
                CategoryId: this.filterData.CategoryId,
            }
        }

        this.setState({
            searchContract,
            searchingLevel: this.state.levelSelected
        });

        if ((this.state.levelSelected == LevelType.Document || this.state.levelSelected == LevelType.DocumentVersion) && this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery) {
            if (searchValue.Keyword) {
                this.simpleSearchQuery(searchValue, isResetPagerIndex);
            } else {
                this.eDiscoveryQuery(searchValue, isResetPagerIndex);
            }
        } else {
            if (this.state.levelSelected === LevelType.SiteCollection) {
                this.setState({
                    showLinkForSC: true,
                    searchingLevel: LevelType.SiteCollection,
                    permissionType: RestoreCenterType.FullControl
                });
                //sample search FE pager
                let siteCollectionResult = _.cloneDeep(this.state.siteCollectionAllData).filter((item) => item.PermissionLevel == RestoreCenterType.FullControl);
                siteCollectionResult = sortUrlByArchiveTime(siteCollectionResult, true);
                if(!this.exactSearchSite){
                    if(this.state.urlSelected.SiteUrl){
                        siteCollectionResult = siteCollectionResult.filter(item => this.getSiteCollectionResultByFilter(item.SiteUrl, this.state.urlSelected.SiteUrl))
                    }
                    this.setState({
                        searchAllDate: _.cloneDeep(siteCollectionResult)
                    });
                    siteCollectionResult = this.getSiteCollectionResultByPage(searchValue.PageIndex, searchValue.PageSize, siteCollectionResult);
                    this.retrySearchDataSuccessCallback(siteCollectionResult, {data: searchValue}, isResetPagerIndex);
                } else {
                    let selectedSiteResult = _.cloneDeep( selectedSite ? [selectedSite] : []).filter((item) => item.PermissionLevel == RestoreCenterType.FullControl);
                    this.setState({
                        searchAllDate: _.cloneDeep(selectedSiteResult)
                    });
                    let exactSearchSiteResult = this.convertSiteCollections(selectedSiteResult)
                    this.retrySearchDataSuccessCallback({RestoreSerchNodes: exactSearchSiteResult}, {data: searchValue}, isResetPagerIndex);  
                }       
            } else {
                searchValue.SerchContract.FilterPolicy.IsShowTotalCount = true;
                this.sqliteQuery(searchValue, isResetPagerIndex);
            }
        }
    };

    getCurrentPageItems = (items, pageSize, pageIndex) => {
        const start = (pageIndex - 1) * pageSize;
        const end = start + pageSize;
        return items.slice(start, end);
    }

    simpleSearchQuery = async (searchValue, isResetPagerIndex) => {
        let cachedAllData = window["EDISCOVERY_SIMPLE_DATA_CACHE"] || [];
        let hasMoreFromApi = window["EDISCOVERY_SIMPLE_DATA_HAS_MORE"];

        if (typeof hasMoreFromApi === "undefined") hasMoreFromApi = true;

        const tempState = this.state;
        if (isResetPagerIndex) {
            cachedAllData = [];
        }

        // If cache can fully satisfy the requested page (full page)
        const fullPageNeeded = searchValue.PageIndex * searchValue.PageSize;
        const startIndexForPage = (searchValue.PageIndex - 1) * searchValue.PageSize;

        // Serve from cache when:
        // 1) cache has at least full page, OR
        // 2) API has no more data (hasMoreFromApi === false) but cache still contains items for this page (partial last page)
        if (
            cachedAllData.length >= fullPageNeeded ||
            (!hasMoreFromApi && cachedAllData.length > startIndexForPage)
        ) {
            const slicedItems = this.getCurrentPageItems(
                cachedAllData,
                searchValue.PageSize,
                searchValue.PageIndex
            )
            // compute itemsPagerHasNext from available info:
            // - if API still has more, rely on hasMoreFromApi
            // - otherwise, check whether cache contains items beyond this page
            const itemsPagerHasNext = hasMoreFromApi
                ? true
                : cachedAllData.length > fullPageNeeded;
            this.setState({
                items: slicedItems,
                itemsPagerHasNext,
            });
            this.dispatch("RestoreCenterTable", "", {
                columns: tempState.allColumns,
                items: slicedItems,
                isReset: isResetPagerIndex,
            });
            setTimeout(() => {
                $$.loading(false);
            }, 1000);
            window["EDISCOVERY_SIMPLE_DATA_CACHE"] = cachedAllData;
            window["EDISCOVERY_SIMPLE_DATA_HAS_MORE"] = hasMoreFromApi;
            return;
        }

        try {
            const clonedSearchValue = _.cloneDeep(searchValue);
            const res = await fetchUtility({
                url: "/api/ArchiverRestore/GetEDiscoverySimpleSearchResult",
                method: "POST",
                data: clonedSearchValue
            });

            if (Array.isArray(res.RestoreSerchNodes) && res.RestoreSerchNodes.length > 0) {
                cachedAllData.push(...res.RestoreSerchNodes);
            }
            hasMoreFromApi = !!res.HasNext;
            this.filterData.ContinuationToken = res.ContinuationToken;
            this.filterData.CategoryId = res.CategoryId;
            const slicedItems = this.getCurrentPageItems(
                cachedAllData,
                searchValue.PageSize,
                searchValue.PageIndex
            );

            const itemsPagerHasNext = res.HasNext
                ? true
                : cachedAllData.length > searchValue.PageIndex * searchValue.PageSize;

            this.setState({
                items: slicedItems,
                itemsPagerHasNext,
                searchLevel: tempState.levelSelected, 
                showActions: {
                    showExportBtn: Array.isArray(res.RestoreSerchNodes) && res.RestoreSerchNodes.length > 0,
                },
            });
            this.dispatch("RestoreCenterTable", "", {
                columns: tempState.allColumns,
                items: slicedItems,
                isReset: isResetPagerIndex,
            });

            window["EDISCOVERY_SIMPLE_DATA_CACHE"] = cachedAllData;
            window["EDISCOVERY_SIMPLE_DATA_HAS_MORE"] = hasMoreFromApi;
        } catch (error) {
            console.error(`Error during simple search query: ${error}`);
        } finally {
            $$.loading(false);
        }
    }

    eDiscoveryQuery = async (searchValue, isResetPagerIndex) => {
        try{
            const tempState = this.state;

            if(isResetPagerIndex) {
                window["EDISCOVERY_DATA_CACHE"] = new Map();
            }
    
            const cacheMap = window["EDISCOVERY_DATA_CACHE"];
            if(cacheMap.has(searchValue.PageIndex)) {
                this.setState({
                    items: cacheMap.get(searchValue.PageIndex).items,
                    searchLevel: tempState.levelSelected,
                    itemsPagerHasNext: cacheMap.get(searchValue.PageIndex).hasNext,
                });
                this.dispatch("RestoreCenterTable", "", {
                    columns: tempState.allColumns,
                    items: cacheMap.get(searchValue.PageIndex).items,
                    isReset: isResetPagerIndex,
                });
                $$.loading(false);
                return;
            }            
    
            let urlData = searchValue.Keyword ? "/api/ArchiverRestore/GetEDiscoverySimpleSearchResult" : "/api/ArchiverRestore/GetAllEDiscoverySearchResult"
            const clonedSearchValue = _.cloneDeep(searchValue);

            const res = await fetchUtility({
                url: urlData,
                method: "POST",
                data: clonedSearchValue
            });
    
            cacheMap.set(searchValue.PageIndex, {items: this.mapRowData(res.RestoreSerchNodes), hasNext: res.HasNext});

            this.filterData.ContinuationToken = res.ContinuationToken;
            this.filterData.CategoryId = res.CategoryId;

            this.setState({
                items: this.mapRowData(res.RestoreSerchNodes) || [],
                searchLevel: tempState.levelSelected,
                itemsPagerHasNext: res.HasNext,
                showActions: {
                    showExportBtn: res.RestoreSerchNodes && res.RestoreSerchNodes.length > 0,
                },
            });
            this.dispatch("RestoreCenterTable", "", {
                columns: tempState.allColumns,
                items: this.mapRowData(res.RestoreSerchNodes),
                isReset: isResetPagerIndex,
            });

            window["EDISCOVERY_DATA_CACHE"] = cacheMap;

            $$.loading(false);
        }
        catch(e){
            console.error(e);
            $$.loading(false);
        }
    }

    getSiteCollectionResultByFilter(siteUrl, searchKeyword = '') {
        if (!siteUrl) {
            return;
        }

        const isFullNameMatch = searchKeyword.startsWith('"') && searchKeyword.endsWith('"');

        if (isFullNameMatch) {
            const exactMatchString = searchKeyword.substring(1, searchKeyword.length - 1);
            const escapedString = exactMatchString.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            return new RegExp(`^${escapedString}$`, 'i').test(siteUrl);
        } else {
            let regexString = searchKeyword;
            regexString = regexString.replace(/[.+^${}()|[\]\\]/g, '\\$&');

            if (searchKeyword.includes('*') || searchKeyword.includes('?')) {
                regexString = regexString.replace(/\*/g, '.*').replace(/\?/g, '.');
                return new RegExp(`^${regexString}$`, 'i').test(siteUrl);
            } else {
                return new RegExp(regexString, 'i').test(siteUrl);
            }
        }
    }

    getSiteCollectionResultByPage = (pageIndex, pageSize, allData) => {
        const cacheSiteCollectionResult = {};
        const allItems = this.convertSiteCollections(allData) || [];

        cacheSiteCollectionResult.TotalCount = allItems.length;

        const start = (pageIndex - 1) * pageSize;
        const end = start + pageSize;
        cacheSiteCollectionResult.RestoreSerchNodes = allItems.slice(start, end);
        cacheSiteCollectionResult.HasNext = end < allItems.length;

        return cacheSiteCollectionResult;
    }

    sqliteQuery = (searchValue, isResetPagerIndex) => {
        let urlData = "/api/ArchiverRestore/GetAllSerchResult";
        let option = {
            url: urlData,
            method: "POST",
            data: searchValue,
        };
        let timeoutDate = new Date();
        timeoutDate.setMinutes(timeoutDate.getMinutes() + 4);
        this.retrySearchData(option, isResetPagerIndex, 0, timeoutDate);
    }

    retrySearchData = (fetchOption, isResetPagerIndex, searchTimes, timeoutDate) => {
        searchTimes++;
        fetchUtility(fetchOption).then((res) => {
            if (res) {
                if (res.Failed && res.Message === 'WaitDownloadIndexDb') {
                    if (timeoutDate > new Date()) {
                        setTimeout(() => {
                            this.retrySearchData(fetchOption, isResetPagerIndex, searchTimes, timeoutDate);
                        }, (searchTimes / 4) * 2000);
        }
                    else {
                        $$.loading(false);
                        this.showMsgToast(RMResx.RM_JS_RestoreCenter_WaitDownloadIndexDb, "error");
    }
                    return;
                }
                else {
                    this.retrySearchDataSuccessCallback(res, fetchOption, isResetPagerIndex);
                }
            }
            $$.loading(false);
        }).catch((e) => {
            if (searchTimes < 5) {
                this.retrySearchData(fetchOption, isResetPagerIndex, searchTimes, timeoutDate);
            } else {
                $$.loading(false);
            }
        });
    }

    retrySearchDataSuccessCallback = (res, fetchOption, isResetPagerIndex) => {
        $$.loading(false);
        const tempState = this.state; 
        const newItems = this.mapRowData(res.RestoreSerchNodes) || [];
        const { PageIndex, PageSize } = fetchOption.data;

        if (PageIndex > this.cachedFilterData.PageIndex) {
            this.setState((prev) => ({
                cachedSelectedAllItems: [...prev.cachedSelectedAllItems, ...newItems]
            }));
        } else if (PageIndex < this.cachedFilterData.PageIndex) {
            const itemsToKeep = PageIndex * PageSize;
            this.setState((prev) => ({
                cachedSelectedAllItems: prev.cachedSelectedAllItems.slice(0, itemsToKeep),
            }));
        } else {
            this.setState({
                cachedSelectedAllItems: newItems,
            });
        }

        this.setState({
            items: newItems,
            itemsCount: res.TotalNumber,
            totalCount: res.TotalCount || 0,
            searchLevel: tempState.levelSelected,
            itemsPagerHasNext: res.HasNext,
            showActions: {
                showExportBtn: Array.isArray(newItems) && newItems.length > 0 && this.state.dataSourceFlag != DataSourceType.FS && this.state.dataSourceFlag != DataSourceType.Teams,
            },
        });
        this.dispatch("RestoreCenterTable", "", {
            columns: tempState.allColumns,
            items: newItems,
            isReset: isResetPagerIndex,
        });
        if (this.state.isSelectedAll) {
            if(this.state.levelSelected === LevelType.SiteCollection){
                //There is an asynchronous issue for SiteCollection 
                setTimeout(() => {
                    this.selectResult(true);
                })
            }else{
                this.selectResult(true);
            }  
        }
    }

    onSelectChange = (items) => {
        let isRestoreCenterAdmin = checkPermission("Archiver_RestoreCenter_Discovery", RM.UserResources);
        let canExportPermissonLevels = [RestoreCenterType.FullControl, RestoreCenterType.SearchAndExport];
        let isSimpleSearch = this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery && !this.state.isAdvancedSearch;
        let canExportSearchMode = this.state.searchLevel === LevelType.Document || this.state.searchLevel === LevelType.DocumentVersion || this.state.searchLevel === LevelType.GoogleDriveDocument || isSimpleSearch;
        let exportBtn = (isRestoreCenterAdmin || canExportPermissonLevels.some(p => p === this.state.permissionType)) && this.state.items && this.state.items.length > 0 && canExportSearchMode && this.state.dataSourceFlag != DataSourceType.FS;
        let restoreBtn = false;
        let viewBtn = false;
        if (items.length > 0) {
            restoreBtn = true;
        }
        if (this.state.itemsChecked.length !== items.length) {
            this.setState({
                isSelectedAll: false,
            });
        }
        const isEnableDeleteArchivedSiteCollection = this.state.isEnableDeleteArchivedSiteCollection;
        this.setState(
            {
                showActions: {
                    showExportBtn: exportBtn,
                    showRestoreBtn: restoreBtn,
                    showViewStaticBtn: viewBtn,
                    showDeleteSCBtn: checkPermission(RouterUrls.CP_Index, RM.UserResources) && isEnableDeleteArchivedSiteCollection && items.length === 1 && this.state.dataSourceFlag === DataSourceType.M365 && this.state.searchLevel === LevelType.SiteCollection,
                },
                itemsChecked: items,
            },
            () => {
                let showButtons = this.getShowActions();
                this.refTopButtons.updateButtons(showButtons);
            }
        );
    };

    onRowEvent = (args) => {
        switch (args.type) {
            case 'getSCData':
                const documentLevel = LevelType.Document;
                const spObjectId = args.rowData.Origin.SPObjectId;
                this.onSelLevelChanged({ newValue: { value: documentLevel } });
                this.setState((prev) => {
                    const urlSelected = this.exactSearchSite ? { SiteUrl: args.rowData.ObjectName } : (prev.urlList.find((item) => item.SPObjectId === spObjectId) || {})

                    return {
                        levelItems: prev.levelItems.map((item) => ({
                            ...item,
                            checked: item.value === documentLevel
                        })),
                        urlSelected,
                        urlList: prev.urlList.map((item) => ({ ...item, checked: item.SPObjectId === spObjectId })),
                    };
                }, () => {
                    this.onSearchData(true);
                });
                break;
            default:
                break;
        }
    }

    onHideSiteMappingPanel = () => {
        this.setState({
            showSiteMappingPanel: { show: false },
        });
    }

    onHideStatisticsPanel = () => {
        const onClose = () => {
            this.setState({
                showStatisticsPanel: { show: false },
                statisticInfo: null,
                statisticJobErrorMsg: "",
            });
        };

        if (!this.state.statisticJobErrorMsg && !this.state.statisticInfo) {
            const args = {
                classify: "warn",
                width: "550px",
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_AR_RC_Statistic_EnsureClosePanel,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false),
                    },
                    {
                        id: "rcStatisticsClosePanel",
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: onClose,
                    },
                ]
            }
            $$.messagedialog(true, args);
        } else {
            onClose();
        }
    }

    onHideWhitelistPanel = () => {
        this.setState({
            showWhitelistPanel: { show: false },
        });
    }

    cancelRestorePanel = () => {
        this.setState({ showRestorePanel: { show: false } });
    };

    saveRestore = (e) => {
        this.dispatch("restorePanel", "onSave", (success, data) => {
            if (success) {
                this.setState({
                    showRestorePanel: { show: false },
                });
                this.onSearchData(true);
            }
        });
        return false;
    };

    showMsgToast = (content, type) => {
        let option = {
            content : content,
            classify : type
        };
        $$.toast(option);
    }

    selectResult = async (checked) => {
        const checkedItems = this.state.items.map((item) => ({ ...item, checked }));
        this.dispatch("RestoreCenterTable", "seletedAll", {
            items: checkedItems,
        });
        await this.restoreCenterTableRef.current.selectChange(checkedItems, checked);
        this.setState({
            isSelectedAll: checked,
            itemsChecked: checked ? checkedItems : this.state.itemsChecked,
        });
    }

    onSelectResult = async () => {
        await this.selectResult(true);
        const checkedItems = this.state.cachedSelectedAllItems.map((item) => ({ ...item, checked: true }));
        await this.restoreCenterTableRef.current.setCachedItems(checkedItems);
        this.setState({
            itemsChecked: checkedItems,
        });
    }

    clearSelectedResult = () => {
        this.selectResult(false);
    }

    onSelectResultByKeyDown = (e) =>{
        if (e.keyCode == 13 || e.keyCode == 32) {
            e.target.click();
        }
    }

    renderSelectItemsInfo() {
        if (this.state.activeTab === ActiveTab.Search) {
            if (this.state.isSelectedAll) {
                return (
                    <div className="ra-main-selected-counter">
                        {RMResx.RM_Common_SelectTableItemsCounter.format(this.state.totalCount, this.state.totalCount)}
                    </div>
                );
            }

            return (
                <div className="ra-main-selected-counter">
                    {RMResx.RM_Common_SelectTableItemsCounter.format(this.state.itemsChecked.length, this.state.totalCount)}
                </div>
            );
        }

        return (
            <div className="ra-main-selected-counter">
                {RMResx.RM_AR_RC_Search_ItemChecked.format(this.state.itemsChecked.length)}
            </div>
        )
    }

    tableNavBar() {
        return (
            <div className="ra-main-navbar">
                <div className="flex">
                    <TopButtonsComponent
                        ref={(r) => (this.refTopButtons = r)}
                        data={{ menuBtnItems: this.getShowActions() }}
                        showCount={4}
                    ></TopButtonsComponent>
                </div>
                {this.renderSelectItemsInfo()}
            </div>
        );
    }

    tableContent() {
        return (
            <div className="ra-main-table">
                <RestoreCenterTable
                    ref={this.restoreCenterTableRef}
                    id="RestoreCenterTable"
                    columns={this.mapColumnData(this.state.allColumns)}
                    uniqueKey={"PathMd5AndArchiverTime"}
                    checkable={true}
                    showLink={this.state.showLinkForSC}
                    searchingLevel={this.state.searchingLevel}
                    isSelectedAll={this.state.isSelectedAll}
                    onChange={this.onSelectChange}
                    onRowEvent={this.onRowEvent}
                    onResizeColumn={(resizeColumn, resizeWidth) => {
                        const newAllColumns = this.mapColumnData(this.state.allColumns, resizeColumn, resizeWidth);
                        this.setState({ allColumns: newAllColumns });
                    }}
                />
            </div>
        );
    }

    tableFooter() {
        return (
            <div style={{ justifyContent: this.state.isShowSelectedAll ? "space-between" : "end" }} className="ra-main-footer">
                {this.state.isShowSelectedAll && (
                    <div tabIndex={0} className="flex ra-flex-align-center">
                        {!this.state.isSelectedAll && this.state.items && this.state.items.length > 0 && (
                            <a
                                className="ra-main-italics-link"
                                tabIndex="0"
                                onClick={this.onSelectResult}
                                onKeyDown={this.onSelectResultByKeyDown}
                            >
                                {RMResx.RM_AR_RC_Search_Tab_SelectedAllResult}
                            </a>
                        )}
                        {this.state.isSelectedAll && (
                            <span className="ra-main-selected-counter">
                                {RMResx.RM_AR_RC_Search_Tab_ResultSelected}
                            </span>
                        )}
                        {this.state.isSelectedAll && (
                            <a
                                className="ra-main-italics-link margin-left-xs"
                                tabIndex="0"
                                onClick={this.clearSelectedResult}
                                onKeyDown={this.onSelectResultByKeyDown}
                            >
                                {RMResx.RM_AR_RC_Search_Tab_ClearAllResult}
                            </a>
                        )}
                    </div>
                )}
                <Paginate
                    onPageIndexChange={(index) => this.onPagerChange(index, this.state.itemsPagerSize)}
                    onPageSizeChange={(pageSize) => this.onPagerChange(this.state.itemsPagerIndex, pageSize)}
                    currentPageCount={this.state.items.length}
                    hasNextPage={this.state.itemsPagerHasNext}
                    pageIndex={this.state.itemsPagerIndex}
                />
            </div>
        );
    }

    onSimpleSearch = () => {
        if (!$$.verify("searchBoxValidation")) {
            return false;
        }
        this.onSearchData(true);
    }

    onSwicthSearchType(isSetArchivedTime = true) {
        const archivedTimeInfo = this.siteLatestArchivedDateCacheRef.current.get("");
        this.setState({
            itemsChecked: [],
            itemsCount: 0,
            totalCount: 0,
            itemsPagerIndex: 1,
            itemsPagerSize: 10,
            itemsPagerHasNext: false,
            itemsPagerContinuationToken: null,
            itemsPagerCategoryId: longMaxValue,
            items: [],
            simpleSearchValue: "",
            urlSelected: {},
            levelSelected: LevelType.Document,
            documentName: "",
            documentContent: "",
            documentMetadata: "",
            createdTimeInfo: {},
            modifiedTimeInfo: {},
            createdBy: "",
            modifiedBy: "",
            archivedTimeInfoForSearchTab: {},
            mainJobId: "",
            archivedTimeInfo: isSetArchivedTime ? archivedTimeInfo : this.state.archivedTimeInfo,
            searchLevel: -1,
            softDeleteSelected: SoftDeleteType.All,
            showActions: {
                showExportBtn: false,
            },
            isSelectedAll: false,
        }, () => {
            this.dispatch("RestoreCenterTable", "", {
                columns: this.state.allColumns,
                items: [],
                isReset: true,
            });
        })
    }

    onRerenderContent = (index) => {
        let levelItems = this.getLevelItems();
        if (index === ActiveTab.EDiscovery) {
            levelItems = [
                {
                    name: RMResx["StorageOptimization.Gui_Document"],
                    value: LevelType.Document,
                    checked: true,
                },
                {
                    name: RMResx["RM_JS_Rule_ObjectLevel_DocumentVersion"],
                    value: LevelType.DocumentVersion,
                    checked: true,
                },
            ];
        } else {
            // Get all list for Search tab
            this.setState({
                dataSourceFlag: this.checkDataSourceFlag(),
            }, () => {
                this.getUrlList();
            })
        }
        this.onSwicthSearchType(false);
        this.setState({
            activeTab: index,
            levelItems,
            isAdvancedSearch: false,
            dataSourceFlag: this.checkDataSourceFlag(),
        });
    }

    isSCBlackListForEdiscovery = async () => {
        const requestOption = {
            url: "/api/ArchiverRestore/IsSCBlackListForEdiscovery",
            method: "GET",
        };
        $$.loading(true);
        fetchUtility(requestOption)
            .then((res) => {
                this.setState({
                    isSCBlackListForEdiscovery: res,
                }, () => {
                    // Get all list for eDiscovery tab
                    this.getUrlListByContentSearchListlist();
                });
            })
            .finally(() => $$.loading(false));
    }

    onChangeRestoreTabs = (index) => {
        // controls need to render again
        this.setState({
            rerenderControl: 1
        }, () => {
            this.setState({
                rerenderControl: 2,
            })
        })
        
        this.setState({ isAdvancedSearch: false }, () => {
            this.onRerenderContent(index);
        });

        this.setIsShowSelectedAll(index == ActiveTab.Search);

        if (index === ActiveTab.EDiscovery) {
            this.isSCBlackListForEdiscovery();
        }
    }

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderSiteMappingPanel() {
        return (
            <R.Panel
                id="raSiteMappingPanel"
                header={RMResx.RM_AR_RC_SiteMapping}
                size={680}
                status={this.state.showSiteMappingPanel}
                destroy={true}
                onClose={this.onHideSiteMappingPanel}
            >
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onHideSiteMappingPanel} />
                <div>
                    <SiteMapping
                        onClosePanel={this.onHideSiteMappingPanel}
                    />
                </div>
            </R.Panel>
        )
    }

    renderStatisticPanel() {
        return (
            <R.Panel
                id="raStatisticPanel"
                header={RMResx.RM_AR_RC_StatisticPanel_Header}
                size={680}
                status={this.state.showStatisticsPanel}
                destroy={true}
                onClose={this.onHideStatisticsPanel}
            >
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onHideStatisticsPanel} />
                <ViewStatistics errorMsg={this.state.statisticJobErrorMsg} statisticInfo={this.state.statisticInfo} />
            </R.Panel>
        )
    }

    renderWhitelistPanel() {
        return (
            <R.Panel
                id="raWhitelistPanel"
                header={RMResx.RM_AR_RC_Whitelist_Settings}
                size={680}
                status={this.state.showWhitelistPanel}
                destroy={true}
                onClose={this.onHideWhitelistPanel}
            >
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onHideWhitelistPanel} />
                <div>
                    <Whitelist
                        isSCBlackListForEdiscovery={this.state.isSCBlackListForEdiscovery}
                        checkIsSCBlackListForEdiscovery={this.isSCBlackListForEdiscovery}
                        getUrlListByContentSearchListlist={this.getUrlListByContentSearchListlist}
                        onClosePanel={this.onHideWhitelistPanel}
                        onReset={this.onSwicthSearchType.bind(this)}
                    />
                </div>
            </R.Panel>
        );
    }

    renderWhitelistButton = () => {
        // Enable full text index
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery) {
            return (
                <div className="flex align-center">
                    <R.Button
                        id="rcWhitelistSettings"
                        type="link"
                        classify="default"
                        icon="fia-gear"
                        text={RMResx.RM_AR_RC_Whitelist_Settings}
                        onClick={() => {
                            this.setState({
                                showWhitelistPanel: { show: true },
                            });
                        }}
                    />
                    <$g.Popover>{RMResx.RM_AR_RC_Whitelist_Settings_Desc}</$g.Popover>
                </div>
            );
        }

        return null;
    }

    renderDeleteSCDialog = () => {
        return (
            <R.Dialog
                id="raDeleteSC"
                header={RMResx.RM_AR_RC_Dialog_DeleteTitle}
                width={480}
                height={342}
                status={this.state.showDeleteSCDialog}
                struct={{ foot: true }}
                destroy
                closeable={false}
            >
                <R.Validation id="confirmDeleteSCValidation">
                    <div className="flex flex-column gap-m">
                        <div tabIndex={0}>
                            {RMResx.RM_AR_RC_Dialog_DeleteDesc}
                        </div>
                        <R.Validation
                            element="Input"
                            label={RMResx.RM_AR_RC_Dialog_Delete_ConfirmLabel}
                            require
                        >
                            <R.Input
                                id="raDeleteSCConfirmIpt"
                                width={"100%"}
                                placeholder={RMResx.RM_AR_RC_Dialog_Delete_PlaceholderIpt}
                                className="margin-top-xs"
                                value={this.state.deleteSCConfirmValue}
                                onChange={(value) => {
                                    const allowEnableDelete = new Set(["yes", "はい", "oui", "예", "是"]);
                                    this.setState({
                                        deleteSCConfirmValue: value,
                                        disableDeleteSCBtn: !allowEnableDelete.has(value?.toLowerCase()),
                                    });
                                }}
                                aria={{ ariaLabel: RMResx.RM_AR_RC_Dialog_Delete_ConfirmLabel }}
                            />
                        </R.Validation>
                    </div>
                </R.Validation>
                <R.Button
                    slot="buttons"
                    classify="blank"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancelDeleteSC}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Delete}
                    disabled={this.state.disableDeleteSCBtn}
                    onClick={this.onDeleteSC}
                />
            </R.Dialog>
        );
    }

    renderDataSource() {
        return <div>
            <div id="ariaDataSource" className="ra-rc-search-title require">
                {RMResx.RM_AR_RC_SearchTitle_DataSource}
            </div>
            <div>
                <R.Combobox
                    id="raDataSource"
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    tooltipField="tooltip"
                    width="100%"
                    items={this.state.dataSourceList}
                    onChange={this.onDataSourceChanged}
                    aria={{
                        ariaLabelledby: "ariaDataSource",
                        ariaRequired: true
                    }}
                />
            </div>
        </div>;
    }

    renderURL(isRequired = true) {
        let exactSearch = false;
        let title = RMResx.RM_AR_RC_SearchTitle_Url;
        if (this.state.activeTab === ActiveTab.Search) {
            if (this.state.dataSourceFlag === DataSourceType.Teams) {
                title = RMResx.RM_AR_RC_SearchTitle_Location;
            } else if (this.state.dataSourceFlag === DataSourceType.FS) {
                title = RMResx.RM_AR_RC_SearchTitle_Connection;
            } else if (this.state.dataSourceFlag === DataSourceType.Google) {
                title = RMResx.RM_AR_RC_SearchTitle_Google_Drive;
            } else if (this.state.levelSelected === LevelType.SiteCollection) {
                exactSearch = true;
                isRequired = this.exactSearchSite;
            } else {
                if (this.exactSearchSite) {
                    exactSearch = true;
                }
            }
        }
        else if(this.state.activeTab === ActiveTab.EDiscovery){
            if (this.ediscoverExactSearchSite) {
                    exactSearch = true;
            }
        }

        return <div>
            <div id="ariaUrl" className={`ra-rc-search-title ${isRequired ? "require" : ""}`}>
                {title}
            </div>
            <div>
                {exactSearch && <R.Input
                    id={"raSPUrl" + this.state.activeTab}
                    placeholder={title}
                    type="text"
                    value={this.state.urlSelected && this.state.urlSelected.SiteUrl || ""}
                    width="100%"
                    classify={this.state.urlInputClassify}
                    onChange={this.onUrlInputValueChange}
                />}
                {!exactSearch && <R.Combobox
                    id={"raUrl" + this.state.activeTab}
                    textField="SiteUrl"
                    valueField="MasterIndexId"
                    checkedField="checked"
                    tooltipField="tooltip"
                    width="100%"
                    items={this.state.urlList}
                    onChange={this.onSelUrlChanged}
                    aria={{
                        ariaLabelledby: "ariaUrl",
                        ariaRequired: true
                    }}
                    clearable={!isRequired}
                />}
                {
                    isRequired &&
                    <div className="margin-top-s">
                        <R.ValidationFaker
                            valid={this.selectUrlValid}
                            ref={(r) => (this.refSelectUrlValid = r)}
                        />
                    </div>
                }
            </div>
        </div>;
    }

    renderObjectLevel() {
        return <div>
            <div id="ariaLevel" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_Level}
            </div>
            <div>
                <R.Combobox
                    id="raLevel"
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    tooltipField="tooltip"
                    width="100%"
                    linkMode={false}
                    searchable={false}
                    items={this.state.levelItems}
                    onChange={this.onSelLevelChanged}
                    aria="#ariaLevel"
                />
            </div>
        </div>;
    }

    renderName() {
        return <div>
            <div id="ariaDocumentName" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_Name}
            </div>
            <div>
                <R.Input
                    id="raDocumentNameIpt"
                    type="text"
                    value={this.state.documentName}
                    onChange={this.onDocumentNameChanged}
                    aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_Name }}
                />
            </div>
        </div>;
    }

    renderCreateTime() {
        return <div>
            <div id="ariaCreatedDate" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_CreatedDate}
            </div>
            <div>
                <R.Rangepicker
                    id="raCreatedTime"
                    width="100%"
                    data-part="vtWidget"
                    clearable={true}
                    selectedDate={this.state.createdTimeInfo}
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    onChange={this.onSelCreatedTime}
                    aria="#ariaCreatedDate"
                />
            </div>
        </div>;
    }

    renderModifiedTime() {
        return <div>
            <div id="ariaModified" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_Modified}
            </div>
            <div>
                <R.Rangepicker
                    id="raModifiedTime"
                    width="100%"
                    data-part="vtWidget"
                    clearable={true}
                    selectedDate={this.state.modifiedTimeInfo}
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    onChange={this.onSelModifiedTime}
                    aria="#ariaModified"
                />
            </div>
        </div>;
    }

    renderSoftDelete() {
        return <div>
            <div id="ariaSoftDelete" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_SoftDeleted}
            </div>
            <div>
                <R.Combobox
                    id="raSoftDelete"
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    tooltipField="tooltip"
                    width="100%"
                    linkMode={false}
                    searchable={false}
                    items={setCheckedStatusByValue("value", "checked", this.state.softDeleteItems, this.state.softDeleteSelected)}
                    onChange={this.onSoftDeleteChanged}
                    aria="#ariaSoftDelete"
                />
            </div>
        </div>;
    }

    renderCreatedBy() {
        return (
            <div>
                <div id="ariaCreatedBy" className="ra-rc-search-title">
                    {RMResx.RM_AR_RC_SearchTitle_CreatedBy}
                </div>
                <div>
                    <R.Input
                        id="raCreatedByIpt"
                        type="text"
                        value={this.state.createdBy}
                        placeholder={RMResx.RM_AR_RC_SearchTitle_CreatedByPlaceholder}
                        onChange={this.onCreatedByChanged}
                        aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_CreatedBy }}
                    />
                </div>
            </div>
        );
    }

    renderModifiedBy() {
        return (
            <div>
                <div id="ariaModifiedBy" className="ra-rc-search-title">
                    {RMResx.RM_AR_RC_SearchTitle_ModifiedBy}
                </div>
                <div>
                    <R.Input
                        id="raModifiedByIpt"
                        type="text"
                        value={this.state.modifiedBy}
                        placeholder={RMResx.RM_AR_RC_SearchTitle_ModifiedByPlaceholder}
                        onChange={this.onModifiedByChanged}
                        aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_ModifiedBy }}
                    />
                </div>
            </div>
        );
    }

    renderArchivedTime() {
        return <div>
            <div id="ariaArchived" className="ra-rc-search-title">
                {RMResx["StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B"]}
            </div>
            <div>
                <R.Rangepicker
                    id="raArchivedTime"
                    width="100%"
                    data-part="vtWidget"
                    clearable={true}
                    selectedDate={this.state.archivedTimeInfo}
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    onChange={this.onSelArchivedTime}
                    aria="#ariaArchived"
                />
            </div>
        </div>;
    }

    renderArchivedTimeForSearchTab() {
        return <div>
            <div id="ariaArchived" className="ra-rc-search-title">
                {RMResx["StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B"]}
            </div>
            <div>
                <R.Rangepicker
                    id="raArchivedTime"
                    width="100%"
                    data-part="vtWidget"
                    clearable={true}
                    selectedDate={this.state.archivedTimeInfoForSearchTab}
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    onChange={this.onSelArchivedTimeForSearchTab}
                    aria="#ariaArchived"
                />
            </div>
        </div>;
    }
    
    renderMainJobId() {
        return (
            <div>
                <div id="ariaMainJobId" className="ra-rc-search-title">
                    {RMResx.RM_AR_RC_SearchTitle_MainJobId}
                </div>
                <div>
                    <R.Input
                        id="raMainJobIdIpt"
                        type="text"
                        value={this.state.mainJobId}
                        onChange={this.onMainJobIdChanged}
                        aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_MainJobId }}
                    />
                </div>
            </div>
        );
    }

    renderContent() {
        return <div>
            <div id="ariaDocumentName" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_Content}
            </div>
            <div>
                <R.Input
                    id="raDocumentContentIpt"
                    type="text"
                    value={this.state.documentContent}
                    onChange={this.onDocumentContentChanged}
                    aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_Content }}
                />
            </div>
        </div>;
    }

    renderMetadataInfo() {
        return <div>
            <div id="ariaDocumentName" className="ra-rc-search-title">
                {RMResx.RM_AR_RC_SearchTitle_Metadata}
            </div>
            <div>
                <R.Input
                    id="raDocumentMetadataIpt"
                    type="text"
                    value={this.state.documentMetadata}
                    onChange={this.onDocumentMetadataChanged}
                    aria={{ ariaLabel: RMResx.RM_AR_RC_SearchTitle_Metadata }}
                />
            </div>
        </div>;
    }

    renderSearchTabSearchContent() {
        if (this.state.rerenderControl === 1) { return; }

        let selDocumentOrVersionLevel = [LevelType.Document, LevelType.DocumentVersion, LevelType.GoogleDriveDocument].includes(this.state.levelSelected);
        let supportSoftDeleteSource = this.state.dataSourceFlag === DataSourceType.M365 || this.state.dataSourceFlag === DataSourceType.Teams || this.state.dataSourceFlag === DataSourceType.Google;
        let supportSoftDeleteLevel = selDocumentOrVersionLevel || this.state.levelSelected === LevelType.Teams || this.state.levelSelected === LevelType.Mailbox
        let showSoftDelete = RM.gData.enableSoftDelete && this.state.activeTab !== ActiveTab.EDiscovery && supportSoftDeleteSource && supportSoftDeleteLevel;
        let showCreateAndModifiedTime = (this.state.dataSourceFlag === DataSourceType.M365 && selDocumentOrVersionLevel) || this.state.dataSourceFlag === DataSourceType.FS || this.state.dataSourceFlag === DataSourceType.Google;
        let showObjectLevel = this.state.dataSourceFlag === DataSourceType.M365 || this.state.dataSourceFlag === DataSourceType.Teams;
        let showName = this.state.levelSelected != LevelType.SiteCollection && this.state.dataSourceFlag != DataSourceType.Teams;
        return <div className="ra-rc-search">
            {this.renderDataSource()}
            {showObjectLevel && this.renderObjectLevel()}
            {this.renderURL()}
            {showName && this.renderName()}
            {showCreateAndModifiedTime && this.renderCreateTime()}
            {showCreateAndModifiedTime && this.renderModifiedTime()}
            {this.state.dataSourceFlag === DataSourceType.M365 && selDocumentOrVersionLevel && (
                <>
                    {this.renderCreatedBy()}
                    {this.renderModifiedBy()}
                    {this.renderArchivedTimeForSearchTab()}
                    {this.renderMainJobId()}
                </>
            )}
            {showSoftDelete && this.renderSoftDelete()}
        </div>;
    }

    renderEDiscoveryTabSearchContent() {
        if (this.state.rerenderControl === 1) { return; }

        let selDocumentOrVersionLevel = this.state.levelSelected === LevelType.Document || this.state.levelSelected === LevelType.DocumentVersion;
        let selEDiscoveryTab = this.state.enabledEDiscovery && this.state.activeTab === ActiveTab.EDiscovery;
        return <div className="ra-rc-search">
            {this.renderURL(false)}
            {this.renderObjectLevel()}
            {this.state.levelSelected != LevelType.SiteCollection && this.renderName()}
            {selDocumentOrVersionLevel && this.renderCreateTime()}
            {selDocumentOrVersionLevel && this.renderModifiedTime()}
            {selDocumentOrVersionLevel && selEDiscoveryTab && this.renderArchivedTime()}
            {selDocumentOrVersionLevel && selEDiscoveryTab && this.renderContent()}
            {selDocumentOrVersionLevel && selEDiscoveryTab && this.renderMetadataInfo()}
        </div>;
    }

    renderAdvancedSearch() {
        let checkEDiscoveryTab = checkPermission("Archiver_RestoreCenter_Discovery", RM.UserResources) && this.state.activeTab === ActiveTab.EDiscovery && this.state.enabledEDiscovery;
        this.setState({ isAdvancedSearch: checkEDiscoveryTab });
        return (
            <>
                <div style={{ gap: 12 }} className="flex flex-column">
                    {this.renderWhitelistButton()}
                    {checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkEDiscoveryTab && (
                        <div id="raHsSwicthSearchTypeBtn" 
                            className="ra-searchtype-link margin-bottom-m" 
                            tabIndex="0"
                            data-tooltip
                            onClick={() => {
                                this.onSwicthSearchType();
                                this.setState({ isAdvancedSearch: false });
                            }}
                            onKeyDown={this.onKeyDown}>
                            {RMResx.RM_HS_BackBasicSearch}
                        </div>
                    )}
                </div>
                {checkEDiscoveryTab ? this.renderEDiscoveryTabSearchContent() : this.renderSearchTabSearchContent()}
                <div className="ra-foot-btns">
                    <R.Button
                        id="raRestoreSearchBtn"
                        primary={true}
                        classify="theme"
                        icon="fia-search"
                        text={RMResx.RM_JS_TM_SearchTxt}
                        onClick={() => {
                            this.setState({
                                isSelectedAll: false,
                                itemsChecked: [],
                                cachedSelectedAllItems: [],
                            }, () => {
                                this.onSearchData(true);
                            });
                        }}
                    />
                </div>
            </>
        );
    }

    renderRestoreSearch() {
        // this.state.enabledEDiscovery
        if (this.state.activeTab === ActiveTab.EDiscovery) {
            if (this.state.isAdvancedSearch) {
                return this.renderAdvancedSearch();
            }

            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Archiver_RestoreCenter_Discovery", RM.UserResources)) {
                // Simple search
                const searchboxWidth = window.screen.width > 1366 ? 380 : 190;
                return (
                   <div style={{ gap: 12 }} className="flex flex-column">
                        {this.renderWhitelistButton()}
                        <div className="flex align-start">
                            <div>
                                <div id="ariaSimpleSearchKeyword" className="ra-rc-search-title require">
                                    {RMResx.RM_AR_RC_SimpleSearch_Title}
                                </div>
                                <R.Validation id="searchBoxValidation">
                                    <div className="margin-right-l">
                                        <R.Validation element="Input" require>
                                            <R.Input
                                                placeholder={RMResx.RM_AR_RC_SimpleSearchBoxPlaceholder}
                                                width={searchboxWidth}
                                                value={this.state.simpleSearchValue}
                                                onChange={(value) => this.setState({ simpleSearchValue: value })}
                                                aria={{ ariaLabel: RMResx.RM_AR_RC_SimpleSearch_Title }}
                                            />
                                            <R.ValidationMessage />
                                        </R.Validation>
                                    </div>
                                </R.Validation>
                            </div>
                            <div className="flex align-end">
                                <div>
                                    <div id="ariaSimpleSearchArchivedTime" className="ra-rc-search-title">
                                        {RMResx.RM_AR_RC_SimpleSearch_ArchivedTime}
                                    </div>
                                    <div className="margin-right-l">
                                        <R.Rangepicker
                                            id="raModifiedTime"
                                            width="100%"
                                            data-part="vtWidget"
                                            clearable={true}
                                            selectedDate={
                                                this.state.archivedTimeInfo
                                            }
                                            dateTimeFormat={
                                                RM.TimeSettingModel.DateFormat
                                            }
                                            onChange={this.onSelArchivedTime}
                                            aria="#ariaSimpleSearchArchivedTime"
                                        />
                                    </div>
                                </div>
                                <div id="raHsSwicthSearchTypeBtn"
                                    style={{ marginBottom: 10 }}
                                    className="ra-searchtype-link"
                                    role="button"
                                    tabIndex="0"
                                    onKeyDown={this.onKeyDown}
                                    onClick={() => {
                                        this.onSwicthSearchType();
                                        this.setState({ isAdvancedSearch: true });
                                    }}>
                                    {RMResx.RM_HS_AdvancedSearchText}
                                </div>
                            </div>
                        </div>
                        <div className="text-end">
                            <R.Button
                                id="raRestoreSearchBtn"
                                primary={true}
                                classify="theme"
                                icon="fia-search"
                                text={RMResx.RM_JS_TM_SearchTxt}
                                onClick={this.onSimpleSearch}
                            />
                        </div>
                   </div>
                )
            }
        }

        return this.renderAdvancedSearch();
    }

    renderRestoreTable() {
        return (
            <div className="ra-section">
                {this.tableNavBar()}
                {this.tableContent()}
                {this.tableFooter()}
            </div>
        );
    }

    renderRestorePanel() {
        let isDisabledInPlace = this.state.searchLevel == LevelType.Mailbox;
        // let isDisabledOutOfPlace =
        //     this.state.searchLevel == LevelType.SiteCollection ||
        //     this.state.searchLevel == LevelType.Site ||
        //     this.state.searchLevel == LevelType.Teams;
        let isShowRemoveStub =
            this.state.searchLevel != LevelType.Item;
        let isShowVersionOption = [
            LevelType.SiteCollection,
            LevelType.Site,
            LevelType.List,
            LevelType.Folder,
            LevelType.Document,
            LevelType.GoogleDriveDocument,
            LevelType.Teams,
        ].includes(this.state.searchLevel);
        let restoreType = isDisabledInPlace ? RestoreType.OutOfPlace : RestoreType.InPlace;
        let isShowSpecifyUserOption = this.state.searchLevel == LevelType.SiteCollection || this.state.searchLevel == LevelType.Teams;
        return (
            <R.Panel
                id="raRestorePanel"
                header={RMResx.RM_AR_RC_Panel_Header}
                size={670}
                status={this.state.showRestorePanel}
                destroy={true}
            >
                <RestorePanel
                    id="restorePanel"
                    itemsChecked={this.state.itemsChecked}
                    isDisabledInPlace={isDisabledInPlace}
                    isDisabled={false}
                    isShowRemoveStub={isShowRemoveStub}
                    isShowVersionOption={isShowVersionOption}
                    dataSourceType={this.state.dataSourceFlag}
                    defaultRestoreType={restoreType}
                    isShowSpecifyUserOption={isShowSpecifyUserOption}
                    isSelectedAll={this.state.isSelectedAll}
                    searchContract={this.state.searchContract}
                    searchLevel={this.state.searchLevel}
                    onClear={this.clearSelectedResult}
                    searchAllDate={this.state.searchAllDate}
                    levelSelected={this.state.levelSelected}
                    totalItems={this.state.totalCount}
                    isSearchTab={this.state.activeTab === ActiveTab.Search}
                ></RestorePanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelRestorePanel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveRestore} />
                </>
            </R.Panel>
        );
    }

    hideAdvanceRestorePanel = () => {
        this.setState({
            showAdvanceRestorePanel: { show: false },
        });
    }

    saveAdvanceRestore = () => {
        this.dispatch('raAdvanceRestorePanel', 'onSave', () => this.setState({ showAdvanceRestorePanel: { show: false } }));
    }

    renderAdvanceRestorePanel() {
        return (
            <R.Panel
                header={RMResx.RM_RestoreCenter_AdvancedRestore}
                size={670}
                status={this.state.showAdvanceRestorePanel}
                onClose={this.hideAdvanceRestorePanel}
                destroy={true}
            >
                <AdvanceRestorePanel id="raAdvanceRestorePanel"/>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideAdvanceRestorePanel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveAdvanceRestore} />
                </>
            </R.Panel>
        )
    }

    handleAdvanceRestore = () => {
        this.setState({
            showAdvanceRestorePanel: { show: true },
        });
    }

    render() {
        return (
            <div id="rmRestoreCenter">
                <div className="ra-header">
                    <$g.SiteMap data={[SiteMapLinks.Archiver_RestoreCenter]} />
                    <div className="flex gap-s">
                        {
                            !LicenseHelper.HasOpusGoogleLicenseOnly() && (
                                <R.Button
                                    classify="default"
                                    text={RMResx.RM_RestoreCenter_AdvancedRestore}
                                    onClick={this.handleAdvanceRestore}
                                    icon="fia-gear"
                                />
                            )
                        }
                        {checkPermission(RouterUrls.CP_Index, RM.UserResources) && !LicenseHelper.HasOpusGoogleLicenseOnly() && (
                            <R.Button
                                primary
                                classify="theme"
                                text={RMResx.RM_AR_RC_SiteMapping}
                                onClick={this.onShowSiteMappingPanel}
                            />
                        )}
                    </div>
                </div>

                {/* Enable full text index */}
                {checkPermission("Archiver_RestoreCenter_Discovery", RM.UserResources) && this.state.enabledEDiscovery && !EnvironmentHelper.IsGCPEnvironment && !LicenseHelper.HasOpusGoogleLicenseOnly() && (
                    <div className="margin-bottom-s">
                        <R.Tabcontrol
                            maxWidth={"none"}
                            active={this.state.activeTab}
                            onChange={this.onChangeRestoreTabs}
                            destroy={true}
                        >
                            <R.TabPanel
                                tab={RMResx.RM_AR_RC_Search_Tab}
                                aria-label={RMResx.RM_AR_RC_Search_Tab}
                            ></R.TabPanel>
                            <R.TabPanel
                                tab={RMResx.RM_AR_RC_eDiscovery_Tab}
                                aria-label={RMResx.RM_AR_RC_eDiscovery_Tab}
                            ></R.TabPanel>
                        </R.Tabcontrol>
                    </div>
                )}

                <div className="ra-page-main">
                    <div className="ra-section">
                        {this.renderRestoreSearch()}
                    </div>
                    {this.renderRestoreTable()}
                </div>
                {this.renderSiteMappingPanel()}
                {this.renderRestorePanel()}
                {this.renderStatisticPanel()}
                {this.renderWhitelistPanel()}
                {this.renderDeleteSCDialog()}
                {this.renderAdvanceRestorePanel()}
            </div>
        );
    }
}