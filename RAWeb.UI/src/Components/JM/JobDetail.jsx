import * as JMConstants from "./JMConstants";
import RouterUrls from "../../Constants/RouterUrls";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { bindEvents, showToast } from "../../Utilities/CommonUtil";
import JMDetailList from './JMDetailList';
import { JobDetailTemplate, JobDetailTermTemplate, JobProgressTemplate, SubJobDetailTemplate } from "./JMTableTemplate";
import { JobDetailFilterForm } from "./JobMonitorFilterForm";
import { LicenseHelper } from "../../Utilities/CommonUtil";
import JMTable from "./JMTable";
import { UnitConvertsionUtil } from "../DiscoveryAndAnalysis/Analysis/Utils";
import { DataSizeType, DataSizeTypeI18ns } from "../DiscoveryAndAnalysis/Analysis/Constants";
import { Sourceflags } from "../Home/Dashboard/RetentionAndDestroyView/Constants";
import { RestoreType } from "../ArchiveRC/Constants";
import { MessageType } from "../CP/CPConstants";
import { JMProgressFilterForm } from "./JMProgressFilterForm";

const isShowProgressTabTab = new Set([
    JMConstants.JobType.RMArchiverBackup,
    JMConstants.JobType.SpecifySitesArchiverBackup,
    JMConstants.JobType.SpecifyTeamsArchiverBackup,
    JMConstants.JobType.TeamsArchiverBackup,
    JMConstants.JobType.RecordsDisposal,
    JMConstants.JobType.OneDriveRecordsDisposal,
    JMConstants.JobType.TeamsRecordsDisposal
]);

const teamsArchiverBackupJobTypes = new Set([
    JMConstants.JobType.TeamsArchiverBackup,
    JMConstants.JobType.SpecifyTeamsArchiverBackup,
    JMConstants.JobType.TeamsRecordsDisposal
]);

const sitesArchiverBackupJobTypes = new Set([
    JMConstants.JobType.RMArchiverBackup,
    JMConstants.JobType.SpecifySitesArchiverBackup
]);

const archiverOptimizationJobTypes = new Set([
    JMConstants.JobType.RMEndUserArchiverBackup,
    JMConstants.JobType.DiscoverOptimization,
    JMConstants.JobType.ArchiverByHSMXml,
    JMConstants.JobType.CleanUpDuplicateDatas
]);

export default class Jobdetail extends R.Component {
    constructor(props) {
        super(props);
        if (props.location.state) {
            this.Jobid = props.location.state.id;
            this.JobType = props.location.state.type;
            this.DisposalId = props.location.state.DisposalId;
            this.isSkipMergeDetailsJob = props.location.state.isUnmergedJob ?? false;
        }
        this.filterData = this.getDefaultFilterData();
        this.progressFilterData = this.getProgressFilterDefaultData();
        this.state = {
            jobsChecked: [],
            jobsCount: 0,              //分页数据总数
            jobsPagerIndex: 0,         //分页每页的条数
            jobsPagerSize: 15,         //分页每页条数
            termsAllData: [],
            termsCount: 0,             //分页数据总数
            termsPagerIndex: 0,        //分页每页的条数
            termsPagerSize: 15,        //分页每页条数
            summaryModel: {},
            jobSettingModel: {},
            soSummaryModel: {},
            isShowDetailTab: true,
            index: 0,
            showFilterPanel: false,
            columns: this.getDetailColumns(),
            items: [],
            filterOptions: { EntityTypeFilters: [], StatusColumns: [] },
            filterData: this.filterData,
            progressFilterData: this.progressFilterData,
            // Sub-job related state
            soSubJobSummary: {},
            subJobView: this.isSkipMergeDetailsJob ? JMConstants.SubJobTabType.SubJobSummary : null,
            subJobSummaryData: [],
            subJobSummaryCount: 0,
            subJobPagerIndex: 0,
            subJobPagerSize: 15,
            selectedSubJob: null,
            selectedSubJobStatusFilter: null,
            subJobDetailSummaryModel: null,
            showProgressFilterPanel: false,
            jobProgressColumns: this.getProgressColumns(),
            progressStatisticsData: {},
            isShowProgressTab: false,
        };
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        bindEvents(this, "handleSelectedIndexChanged", "onTermsPageChange", 'onSearchStart', 'jqPageChange');
    }

    routerTo(routerUrl, params) {
        this.props.history.push({
            pathname: routerUrl,
            query: params
        });
    }

    componentInit() {
        //是否是DisposalJob
        this.getJobDetail();
        this.handleShowSupportProgressTab();
        if (this.JobType == JMConstants.JobType.OneDriveTermUsageReport || this.JobType == JMConstants.JobType.BCSTermUsageReport || this.JobType == JMConstants.JobType.RetiredTermReport
            || this.JobType == JMConstants.JobType.OrphanedTermReport || this.JobType == JMConstants.JobType.EnforceRetention
            || this.JobType == JMConstants.JobType.EXOTermUsageReport || this.JobType == JMConstants.JobType.EXORetiredTermUsageReport
            || this.JobType == JMConstants.JobType.EXOOrphanedTermUsageReport || this.JobType == JMConstants.JobType.PhysicalTermUsageReport
            || this.JobType == JMConstants.JobType.PhysicalOrphanedTermUsageReport || this.JobType == JMConstants.JobType.PhysicalRetiredTermUsageReport
            || this.JobType == JMConstants.JobType.FSBCSTermUsageReport || this.JobType == JMConstants.JobType.FSOrphanedTermReport || this.JobType == JMConstants.JobType.FSRetiredTermReport
            || this.JobType == JMConstants.JobType.SPOnPremBCSTermUsageReport || this.JobType == JMConstants.JobType.SPOnPremOrphanedTermReport || this.JobType == JMConstants.JobType.SPOnPremRetiredTermReport
            || this.JobType == JMConstants.JobType.BoxBCSTermUsageReport || this.JobType == JMConstants.JobType.BoxOrphanedTermUsageReport || this.JobType == JMConstants.JobType.BoxRetiredTermUsageReport
            || this.JobType == JMConstants.JobType.GoogleBCSTermUsageReport || this.JobType == JMConstants.JobType.GoogleOrphanedTermUsageReport || this.JobType == JMConstants.JobType.GoogleRetiredTermUsageReport
            || this.JobType == JMConstants.JobType.TeamsBCSTermUsageReport || this.JobType == JMConstants.JobType.TeamsOrphanedTermUsageReport || this.JobType == JMConstants.JobType.TeamsRetiredTermUsageReport
            || this.JobType == JMConstants.JobType.TermUsageReport
        ) {
            this.getTermSelection();
        }
    }

    handleShowSupportProgressTab() {
        if(isShowProgressTabTab.has(this.JobType) && this.enableRecordsArchiver && this.isSkipMergeDetailsJob) {
            this.setState({ isShowProgressTab: true });
        }
    }

    convertStatusStr(statusCode) {
        return JMConstants.StatusValue[statusCode];
    }

    convertActionTabStr(tabCode) {
        return JMConstants.ActionTabValue[tabCode];
    }

    convertCommentString(comment) {
        if (comment && ((comment.indexOf("0x80070005") + comment.indexOf("E_ACCESSDENIED")) > -1)) {
            return RMResx.RM_JM_Details_Failed_AccessDenied;
        }
        return comment;
    }

    convertFileSize(fileSize) {
        const convertedFileSize = UnitConvertsionUtil.DynamicConvertForJobDetail(fileSize);

        if (Number(convertedFileSize) == 0) {
            return `0${RMResx.RM_FS_JobReportSizeUnitBytes}`
        }
        return `${convertedFileSize} ${DataSizeTypeI18ns.get(UnitConvertsionUtil.GetUnitForJobDetail(fileSize))}`;
    }


    getJobDetail() {
        let isShowDetailTab = true;
        if (this.Jobid) {
            $$.loading(true);
            let option = {
                url: "/api/JMApi/GetJobSummary",
                method: "POST",
                data: this.Jobid
            };
            if (this.DisposalId
                || this.JobType == JMConstants.JobType.MigrationArchiverRestore
                || this.JobType == JMConstants.JobType.MigrationArchiverRetention
                || this.JobType == JMConstants.JobType.MigrationArchiverFileLevelRetention) {
                option = {
                    url: "/api/JMApi/GetDisposalJobSummary",
                    method: "POST",
                    data: { JobID: this.Jobid, JobType: this.JobType }
                };
            }
            fetchUtility(option).then((data) => {
                if (data) {
                    if (data.JobType == JMConstants.JobType.ExportToLocation) {
                        isShowDetailTab = false;
                        this.setState({
                            isShowDetailTab: isShowDetailTab,
                        });
                    }
                    this.setState({
                        summaryModel: data,
                    }, () => {
                        if (this.JobType == JMConstants.JobType.RecordsDisposal
                            || this.JobType == JMConstants.JobType.OneDriveRecordsDisposal
                            || this.JobType == JMConstants.JobType.RMArchiverBackup
                            || this.JobType == JMConstants.JobType.RMEndUserArchiverBackup
                            || this.JobType == JMConstants.JobType.SpecifySitesArchiverBackup
                            || this.JobType == JMConstants.JobType.SOPreScan
                            || this.JobType == JMConstants.JobType.DiscoverOptimization
                            || this.JobType == JMConstants.JobType.BoxDisposal
                            || this.JobType == JMConstants.JobType.ApprovalProcessArchive
                            || this.JobType == JMConstants.JobType.DiscoveryPreScan
                            || this.JobType == JMConstants.JobType.DiscoveryPlanProOptimization
                            || this.JobType == JMConstants.JobType.DiscoveryPlanProScan
                            || this.JobType == JMConstants.JobType.ArchiverDeduplication
                            || this.JobType == JMConstants.JobType.ArchiverDeduplicationReport
                            || this.JobType == JMConstants.JobType.TeamsRecordsDisposal
                            || this.JobType == JMConstants.JobType.TeamsArchiverBackup
                            || this.JobType == JMConstants.JobType.TeamsPreScan
                            || this.JobType == JMConstants.JobType.GoogleRecordsDisposal
                            || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup
                            || this.JobType == JMConstants.JobType.EXORecordsDisposal
                            || this.JobType == JMConstants.JobType.ArchiverByHSMXml
                            || this.JobType == JMConstants.JobType.CleanUpDuplicateDatas
                        ) {
                            this.getSOSummaryModel();
                        }

                        if (data.JobType == JMConstants.JobType.ApplySharePointSettings
                            || data.JobType == JMConstants.JobType.ApplyTeamsSettings) {
                            this.getJobSetting();
                        }

                        if (data.JobType == JMConstants.JobType.TeamsArchiverRestore
                            || data.JobType == JMConstants.JobType.TeamsOutPlaceRestore
                            || data.JobType == JMConstants.JobType.ArchiverRestore
                            || data.JobType == JMConstants.JobType.FSArchiverRestore
                            || data.JobType == JMConstants.JobType.ArchiverOutPlaceRestore
                            || data.JobType == JMConstants.JobType.StubOopRestore
                            || data.JobType == JMConstants.JobType.MailBoxArchiverRestore
                            || data.JobType == JMConstants.JobType.GoogleArchiverRestore
                            || data.JobType == JMConstants.JobType.ArchiverToSpoRestore
                            || data.JobType == JMConstants.JobType.StubArchiverRestore
                            || data.JobType == JMConstants.JobType.M365InPlaceArchiverRestore
                        ) {
                            this.getRestoreJobSummaryDetails();
                        }
                    });
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    getSOSummaryModel() {
        $$.loading(true);
        let option = {
            url: "/api/JMApi/GetSOJobSummaryDetails",
            method: "POST",
            data: this.Jobid
        };
        fetchUtility(option).then((data) => {
            if (data) {
                if (this.JobType == JMConstants.JobType.TeamsArchiverBackup || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup) {
                    this.setState({
                        soSummaryModel: {
                            ...data,
                            ActionStatistics: data.ActionStatistics.sort((a, b) => a.ActionTab - b.ActionTab),
                        },
                    });
                } else {
                    this.setState({
                        soSummaryModel: data,
                    });
                }
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getSOSubJobSummary = async () => {
        $$.loading(true);
        let option = {
            url: "/api/JMApi/GetSOSubJobSummaryDetails",
            method: "POST",
            data: this.state.selectedSubJob ? this.state.selectedSubJob.SubJobID : null
        };
        try {
            const res = await fetchUtility(option);
            if (res) {
                if (this.JobType == JMConstants.JobType.TeamsArchiverBackup || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup) {
                    this.setState({
                        soSubJobSummary: {
                            ...res,
                            ActionStatistics: res.ActionStatistics.sort((a, b) => a.ActionTab - b.ActionTab),
                        },
                    });
                } else {
                    this.setState({
                        soSubJobSummary: res,
                    });
                }
            }
            $$.loading(false);
        } catch (e) {
            $$.loading(false);
        }
    }

    getRestoreJobSummaryDetails = () => {
        $$.loading(true);
        const option = {
            url: "/api/JMApi/GetRestoreJobSummaryDetails",
            method: "POST",
            data: this.Jobid
        };
        fetchUtility(option).then((data) => {
            if (data) {
                this.setState({
                    soSummaryModel: data,
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getTermSelection() {
        if (this.Jobid) {
            let urlData = "/api/JMApi/GetTermSelection" + "?id=" + this.Jobid;
            let option = {
                url: urlData,
                method: "Get"
            };
            $$.loading(true);
            fetchUtility(option).then((res) => {
                let data = JSON.parse(res);
                if (data) {
                    this.setState({
                        termsAllData: data,
                        termsCount: data.length
                    });
                    this.getTermPage(0, this.state.termsPagerSize);
                } else {
                    this.dispatch("jobDetailTerm", { columns: this.getTermColumn(), items: data, isReset: true });
                }
                $$.loading(false);
            });
        }
    }

    getJobDetailItems(cellInfo) {
        let cellValues = [];
        let cellKeys = JMConstants.JobDetailsCells[this.JobType];
        if (cellKeys) {
            for (let cellKey of cellKeys) {
                let cellValue = cellInfo[cellKey];
                switch (cellKey) {
                    case "Status":
                        cellValue = this.convertStatusStr(cellInfo[cellKey]);
                        break;
                    case "BackupSourceURL":
                        cellValue = cellInfo["SourceURL"];
                        break;
                    case "ActionTab":
                        cellValue = this.convertActionTabStr(cellInfo[cellKey]);
                        break;
                    case "Comment":
                        cellValue = this.convertCommentString(
                            cellInfo[cellKey]
                        );
                        break;
                    case "TotalSize":
                        cellValue = this.convertFileSize(cellInfo[cellKey]);
                        break;
                    case "SourceFlag":
                        if ([
                            JMConstants.JobType.ArchiverRetentionSimulate,
                            JMConstants.JobType.FSRetainSimulate,
                        ].includes(this.JobType)) {
                            cellValue = Sourceflags[cellInfo[cellKey]];
                        }
                        break;
                    case "RetentionSetting":
                        cellValue = cellKey;
                        break;
                }
                cellValues.push(cellValue);
            }
        }
        cellInfo.cellValues = cellValues;
        return cellInfo;
    }

    getM365SOJobDetailColumns() {
        return JMConstants.M365SOJobDetailColumns;
    }

    getDetailColumns() {
        let needTransformI18nKeyOrToolTipList = ["Comment", "SiteCollectionURL", "PhysicalLibraryUrl", "UniqueID"
            , "SourceURL", "Url", "Term", "TitleOrName", "Name"
            , "ObjectName", "MMSApplication", "Classification"
            , "TermName", "LocationPath", "ItemType", "SourceUrl", "DestinationUrl"
            , "PhysicalFileName", "PhysicalRecordName", "HomeLocation", "BoxName"
            , "Location", "ObjectLevel", "Title", "TermName"
            , "ApprovalStatus", "RecordOwner", "ScanItemId", "NodeId", "DestinationURL", "BackupSourceURL", "StatusStr", "URL"
            , "DetailsTab", "Type", "BackupSourceURL", "Size", "StatusStr", "FinishTime", "Action", "ActionStr", "DataCenterName","DestinationURL", "Comment"
            , "FullPath", "ColumnName", "DestinationFullPath", "SourceLocation", "DestinationLocation", "ExportLocation", "ReportName"
            , "JobId", "FileName", "TemplateName", "Term/Rule", "LabelName", "ItemName", "ProfileName", "Barcode", "DriveName"
            , "ProfileCriteria","ConnectionName", "RetentionSetting", "StorageLocation"];
        let columnHeader = JMConstants.JobDetailsColumns[this.JobType];
        let column = [];
        for (let key in columnHeader) {
            if (columnHeader.hasOwnProperty(key)) {
                let columnName = '';
                let isShowTip = false;
                if (needTransformI18nKeyOrToolTipList.indexOf(columnHeader[key]) > -1) {
                    switch (columnHeader[key]) {
                        case "ObjectLevel":
                            columnName = RMResx.RM_JS_RC_ReportColumn_ObjectLevel;
                            break;
                        case "SourceUrl":
                        case "SourceLocation":
                            columnName = RMResx.RM_JS_JMD_Grid_SourceURL;
                            break;
                        case "DestinationURL":
                        case "DestinationLocation":
                        case "DestinationUrl":
                            if ([JMConstants.JobType.ArchiverRestore, JMConstants.JobType.ArchiverOutPlaceRestore, JMConstants.JobType.ArchiverToSpoRestore].includes(this.JobType)) {
                                columnName = "^Destination URL";
                            } else {
                                columnName = RMResx.RM_JS_JMD_Grid_DestinationUrl;
                            }
                            break;
                        case "DestinationFullPath":
                            columnName = RMResx.RM_JS_JMD_Grid_DestinationUrl;
                            break;
                        case "BackupSourceURL":
                            columnName = RMResx.RM_JS_JMD_Grid_BackupSourceURL;
                            break;
                        case "StatusStr":
                            columnName = RMResx.RM_JS_JMD_Grid_Status;
                            break;
                        case "Location":
                            columnName = RMResx.RM_JS_RC_ReportColumn_LocationPath;
                            break;
                        case "StorageLocation":
                            columnName = RMResx.RM_AR_RC_Panel_Storage;
                            break;
                        case "FullPath":
                            if (this.JobType == JMConstants.JobType.PhysicalDisposal) {
                                columnName = RMResx.RM_JS_JMD_Grid_FullPathForPhysical;
                            } else {
                                columnName = RMResx.RM_JS_JMD_Grid_SourceURL;
                            }
                            break;
                        case "Url":
                            if (this.JobType == JMConstants.JobType.ManualApprovalTimer) {
                                columnName = RMResx.RM_JS_JMD_Grid_URLForManual;
                            } else {
                                columnName = RMResx["RM_JS_JMD_Grid_" + columnHeader[key]];
                            }
                            break;
                        case "JobId":
                            columnName = RMResx.RM_JS_JM_JobID;
                            break;
                        case "FileName":
                            columnName = RMResx.RM_JS_DC_FileName;
                            break;
                        case "TemplateName":
                            columnName = RMResx.RM_PRM_TM_TemplateName_Title;
                            break;
                        case "Term/Rule":
                            columnName = RMResx.RM_JS_JMD_Grid_TermOrRule;
                            break;
                        case "ProfileName":
                            columnName = RMResx.RM_JM_ProfileName;
                            break;
                        case "Barcode":
                            columnName = RMResx.RM_PRM_PRE_Column_Barcode;
                            break;
                        case "DriveName":
                            columnName = RMResx.RM_JS_JMD_Grid_GoogleDrive_Name;
                            break;
                        case "ProfileCriteria":
                            columnName = RMResx.RM_JS_JM_ProfileCriteria;
                            break;
                        case "ConnectionName":
                            columnName = RMResx.RM_JS_JMD_Grid_Connection_Name;
                            break;
                        case "RetentionSetting":
                            columnName = RMResx.RM_DSB_Retention_Column_Setting;
                            break;
                        case "Name":
                            columnName = RMResx.RM_JS_JMD_Grid_TitleOrName;
                            break;
                        case "ActionStr":
                        case "ActionName":
                            columnName = RMResx.RM_JS_JMD_Grid_Action;
                            break;
                        case "DataCenterName":
                            columnName = RMResx.RM_JS_JMD_Grid_DataCenterName;
                            break;
                        default:
                            columnName = RMResx["RM_JS_JMD_Grid_" + columnHeader[key]];
                    }
                    if (!columnName) {
                        columnName = columnHeader[key];
                    }
                    isShowTip = true;
                } else {
                    if (RMResx["RM_JS_JMD_Grid_" + columnHeader[key]]) {
                        columnName = RMResx["RM_JS_JMD_Grid_" + columnHeader[key]];
                    } else {
                        columnName = columnHeader[key];
                    }
                    isShowTip = false;
                }
                if (columnHeader[key]) {
                    column.push(
                        {
                            header: columnName,
                            width: [JMConstants.JobDetailsColumnsWidth[this.JobType][key] * 1280],
                            resizeable: true,
                            showTip: isShowTip
                        });
                }
            }
        }
        return column;
    }

    getProgressColumns() {
        return [
            {
                header: RMResx.RM_JS_JMD_Progress_SubJobID,
                width: [280],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_Status,
                width: [200],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_Scope,
                width: [350],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_StartTime,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_FinishTime,
                width: [250],
                resizeable: true,
            },
            {
                header: (
                    <>
                        {RMResx.RM_JS_JMD_Progress_ScannedObjects}
                        <$g.Popover>
                            {RMResx.RM_JS_JMD_Progress_ScannedObjects_Tooltip}
                        </$g.Popover>
                    </>
                ),
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_EstimatedScanFinishedTime,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_ExportedObjects,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_EstimatedExportFinishedTime,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_ArchivedObjects,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_ArchiveSize,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_EstimatedArchiveFinishedTime,
                width: [250],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_OtherActions,
                width: [200],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_EstimatedActionsFinishedTime,
                width: [290],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_JMD_Progress_LastUpdateTime,
                width: [250],
                resizeable: true,
            }
        ]
    }

    getTermColumn() {
        let column = [
            {
                headerTemplate: RMResx.RM_JS_JMD_Term_Term,
                width: [100],
                resizeable: true,
            }, {
                header: RMResx.RM_JS_JMD_Term_TermFullPath,
                width: [100],
                resizeable: true,
            }];
        return column;
    }

    getDefaultFilterData() {
        let filterData = {
            JobID: this.Jobid,
            JobType: this.JobType,
            SearchValue: '',
            SearcheKeys: this.isSkipMergeDetailsJob ? ["Scope"] : JMConstants.JobDetailSearchKeys[this.JobType],
            PageSize: 15,
            CurrentPage: 1,
            StatusFilters: [],
            EntityTypeFilters: [],
            ActionTabFilters: [],
            ArchiverActionFilters: [],
            SubJobStatusFilters: []
        };
        return filterData;
    }

    getProgressFilterDefaultData () {
        return {
            JobID: this.Jobid,
            JobType: this.JobType,
            PageSize: 15,
            PageNumber: 1,
            StatusFilter: [],
            SearchKeys: ["Scope"],
            SearchValue: "",
        }
    }

    onSearchStart(args) {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.filterData.SearchValue = searchValue;
            if (this.isSkipMergeDetailsJob) {
                if (this.state.subJobView === JMConstants.SubJobTabType.SubJobSummary) {
                    this.filterData.SearcheKeys = ["Scope"];
                    this.getSubJobDetailFromServer(true);
                } else {
                    this.filterData.SearcheKeys = JMConstants.JobDetailSearchKeys[this.JobType];
                    this.getSubJobDetailById(true);
                }
            } else {
                this.filterData.SearcheKeys = JMConstants.JobDetailSearchKeys[this.JobType];
                this.getDetailFromServer(true);
            }
        } else {
            this.filterData.SearchValue = "";
            if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobSummary) {
                this.getSubJobDetailFromServer(true);
            } else if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobDetails) {
                this.getSubJobDetailById(true);
            } else {
                this.getDetailFromServer(true);
            }
        }
    }

    handleSelectedIndexChanged(newIndex) {
        this.setState({
            progressFilterData: this.getProgressFilterDefaultData(),
        });
        if (this.progressSearchBoxRef) {
            this.progressSearchBoxRef.clear?.();
        }
        const isDetailTab = (this.state.isShowProgressTab && newIndex === 2) || (!this.state.isShowProgressTab && newIndex === 1);
        if (this.state.isShowProgressTab && newIndex === 1) {
            this.getProgressTabContent();
        } 
        if (isDetailTab) {
            this.filterData = this.getDefaultFilterData();
            if (this.searchBoxRef) {
                this.searchBoxRef.clear?.();
            }
            if (this.isSkipMergeDetailsJob) { 
                this.getSubJobDetailFromServer(true);
            } else {
                this.getDetailFromServer(true);
            }
        }
        this.setState({ index: newIndex, subJobView: JMConstants.SubJobTabType.SubJobSummary });
    }

    onClose() {
        if (this.DisposalId) {
            this.props.history.goBack();
        } else {
            this.routerTo("/Root/JM/Index", { id: this.Jobid });
        }
    }

    jqPageChange(pagerIndex, pagerSize, callback) {
        let filterData = this.filterData;
        filterData.PageSize = pagerSize;
        filterData.CurrentPage = pagerIndex + 1;
        this.setState({ jobsPagerIndex: pagerIndex, jobsPagerSize: pagerSize });
        if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobSummary) {
            this.getSubJobDetailFromServer(false);
        } else if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobDetails) {
            this.getSubJobDetailById(false);
        } else {
            this.getDetailFromServer(false);
        }
        callback(true);
    }

    onTermsPageChange(pagerIndex, pagerSize, callback) {
        //前台分页
        this.setState({
            termsPagerIndex: pagerIndex,
            termsPagerSize: pagerSize
        });
        this.getTermPage(pagerIndex, pagerSize);
        if (callback) {
            callback(true);
        }
    }

    getTermPage(pagerIndex, pagerSize) {
        //起始和终止分页截取
        let startIndex = pagerIndex * pagerSize;
        let endIndex = (pagerIndex + 1) * pagerSize;
        let currentSelectedItems = JSON.parse(JSON.stringify(this.state.termsAllData.slice(startIndex, endIndex)));
        this.dispatch("jobDetailTerm", { columns: this.getTermColumn(), items: currentSelectedItems, isReset: true });
    }

    // Sub-job detail: fetch detail items for a specific sub-job
    getSubJobDetailById = async (isInitPager, isCellClicked = false) => {
        if (isInitPager) {
            this.filterData.CurrentPage = 1;
            this.setState({ jobsPagerIndex: 0 });
        }
        $$.loading(true);
        if (isCellClicked) {
            this.filterData.StatusFilters = this.state.selectedSubJobStatusFilter || [];
        }
        const requestData = {
            ...this.filterData,
            JobID: this.state.selectedSubJob?.SubJobID,
        };
        const option = {
            url: "/api/JMApi/GetSubJobDetailsById",
            method: "POST",
            data: requestData
        };
        this.dispatch("jobDetailTable", { columns: this.getDetailColumns(), items: [], isReset: true });
        try {
            const res = await fetchUtility(option);
            let data = JSON.parse(res);
            if (data.Success) {
                this.setState({
                    items: data.Details || [],
                    jobsCount: data.TotalNumber,
                });
                let jobDetailInfo = [];
                if (data.Details) {
                    for (let item of data.Details) {
                        let detailItem = this.getJobDetailItems(item);
                        jobDetailInfo.push(detailItem);
                    }
                }
                this.dispatch("jobDetailTable", { columns: this.getDetailColumns(), items: jobDetailInfo, isReset: true });
            }
            $$.loading(false);
        } catch (e) {
            $$.loading(false);
        }
    }

    getProgressTabContent() {
        $$.loading(true);
        const option = {
            url: "/api/JMApi/GetJobProgress",
            method: "POST",
            data: this.state.progressFilterData
        };
        fetchUtility(option).then((res) => {
            if (res.Success) {
                this.setState({
                    items: res.Details || [],
                    jobsCount: res.TotalNumber,
                    progressStatisticsData: res.JobProgressDetails,
                });
                this.dispatch("jobProgressTable", { columns: this.state.jobProgressColumns, items: res.Details || [], isReset: true });
            } else if (res.IsDeleted) {
                $$.messagedialog(true, {
                    width: '550px',
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_JS_JM_SelectedJobIdError,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                $$.messagedialog(false);
                            }
                        },
                    ],
                });
            }
            $$.loading(false);
        }).catch(() => {
            $$.loading(false);
        });
    }

    // Handle sub-job cell click (SubJobId or count columns)
    handleSubJobCellClick = (action, rowData) => {
        let statusFilter = [];
        if (action === 'SuccessfulCountClicked') {
            statusFilter.push(0);
        } else if (action === 'FailedCountClicked') {
            statusFilter.push(1);
        } else if (action === 'SkippedCountClicked') {
            statusFilter.push(2);
        }

        this.setState({
            subJobView: JMConstants.SubJobTabType.SubJobDetails,
            selectedSubJob: rowData,
            selectedSubJobStatusFilter: statusFilter,
            subJobDetailSummaryModel: rowData,
            jobsPagerSize: 15
        }, async () => {
            this.filterData.CurrentPage = 1;
            this.filterData.SearchValue = "";
            this.filterData.SearcheKeys = JMConstants.JobDetailSearchKeys[this.JobType];
            this.setState({ jobsPagerIndex: 0 });
            await this.getSubJobDetailById(true, true);
            await this.getSOSubJobSummary();
        });
    }

    // Navigate back to sub-job summary view
    handleBackToSubJobSummary = () => {
        const clonedFilterData = RM.deepcopy(this.filterData);
        this.filterData = this.getDefaultFilterData();
        this.filterData.SubJobStatusFilters = clonedFilterData.SubJobStatusFilters;
        this.setState({
            subJobView: JMConstants.SubJobTabType.SubJobSummary,
            selectedSubJob: null,
            selectedSubJobStatusFilter: null,
            subJobDetailSummaryModel: null,
            filterData: this.filterData,
            jobsPagerSize: 15,
        }, () => {
            this.getSubJobDetailFromServer(true);
        });
    }

    getDetailFromServer(isInitPager) {
        if (isInitPager) {
            this.filterData.CurrentPage = 1;
            this.setState({
                jobsPagerIndex: 0
            });
        }
        $$.loading(true);
        let urlData = "/api/JMApi/GetJobDetails";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data.Success) {
                if (data.Details) {
                    this.setState({
                        items: data.Details,
                        jobsCount: data.TotalNumber
                    });
                }
                let jobDetailInfo = [];
                if (data.Details) {
                    for (let item of data.Details) {
                        let detailItem = this.getJobDetailItems(item);
                        jobDetailInfo.push(detailItem);
                    }
                }
                this.dispatch("jobDetailTable", { columns: this.getDetailColumns(), items: jobDetailInfo, isReset: true });
            } else if (data.IsDeleted) {
                //show message box, job has been deleted.
                $$.messagedialog(true, {
                    // classify: "warn",
                    width: '550px',
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_JS_JM_SelectedJobIdError,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                $$.messagedialog(false);
                            }
                        },
                    ],
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getSubJobDetailFromServer(isInitPager) {
        if (isInitPager) {
            this.filterData.CurrentPage = 1;
            this.setState({
                jobsPagerIndex: 0
            });
        }
        $$.loading(true);
        let urlData = "/api/JMApi/GetSubJobDetails";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data.Success) {
                this.setState({
                    subJobSummaryData: data.Details || [],
                    subJobSummaryCount: data.TotalNumber || 0,
                    subJobView: JMConstants.SubJobTabType.SubJobSummary,
                });

                this.dispatch("M365SOJobDetailTable", {
                    columns: this.getM365SOJobDetailColumns(),
                    items: data.Details || [],
                    isReset: true
                });
            } else if (data.IsDeleted) {
                //show message box, job has been deleted.
                $$.messagedialog(true, {
                    // classify: "warn",
                    width: '550px',
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_JS_JM_SelectedJobIdError,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                $$.messagedialog(false);
                            }
                        },
                    ],
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    findKey(obj, value, compare = (a, b) => a === b) {
        return Object.keys(obj).find(k => compare(obj[k], value));
    }

    openFilterPanel = () => {
        this.setState({ showFilterPanel: true }, () => {
            if (this.jdFilterFormRef && typeof this.jdFilterFormRef.resetFilterData === 'function') {
                this.jdFilterFormRef.resetFilterData(this.state.filterData);
            }
        });
    }

    hideFilterPanel = () => {
        this.setState({ showFilterPanel: false });
    }

    onFilter = () => {
        if (this.jdFilterFormRef && typeof this.jdFilterFormRef.getFilterData === 'function') {
            const filterParam = this.jdFilterFormRef.getFilterData();
            this.filterData.EntityTypeFilters = filterParam.EntityTypeFilters;
            this.filterData.StatusFilters = filterParam.StatusFilters;
            this.filterData.ActionTabFilters = filterParam.ActionTabFilters;
            this.filterData.ArchiverActionFilters = filterParam.ArchiverActionFilters;
            this.filterData.SubJobStatusFilters = filterParam.SubJobStatusFilters;
            this.setState({
                filterData: this.filterData
            });
            if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobSummary) {
                this.getSubJobDetailFromServer(true);
            } else if (this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobDetails) {
                this.getSubJobDetailById(true);
            } else {
                this.getDetailFromServer(true);
            }
        }
        this.setState({ showFilterPanel: false });
    }

    renderFilterPanel() {
        let isShowTabsFilter = this.JobType == JMConstants.JobType.RecordsDisposal
            || this.JobType == JMConstants.JobType.OneDriveRecordsDisposal
            || this.JobType == JMConstants.JobType.RMEndUserArchiverBackup
            || this.JobType == JMConstants.JobType.RMArchiverBackup
            || this.JobType == JMConstants.JobType.SpecifySitesArchiverBackup
            || this.JobType == JMConstants.JobType.DiscoverOptimization
            || this.jobType == JMConstants.JobType.BoxDisposal
            || this.JobType == JMConstants.JobType.ApprovalProcessArchive
            || this.JobType == JMConstants.JobType.TeamsRecordsDisposal
            || this.JobType == JMConstants.JobType.TeamsArchiverBackup
            || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup
            || this.JobType == JMConstants.JobType.TeamsPreScan
            || this.JobType == JMConstants.JobType.SOPreScan
            || this.JobType == JMConstants.JobType.DiscoveryPreScan
            || this.JobType == JMConstants.JobType.DiscoveryPlanProOptimization
            || this.JobType == JMConstants.JobType.DiscoveryPlanProScan
            || this.JobType == JMConstants.JobType.ArchiverByHSMXml
            || this.JobType == JMConstants.JobType.CleanUpDuplicateDatas;
        const isShowDiscoveryPreScanFilter = this.JobType == JMConstants.JobType.TeamsPreScan
            || this.JobType == JMConstants.JobType.SOPreScan
            || this.JobType == JMConstants.JobType.DiscoveryPreScan
            || this.JobType == JMConstants.JobType.DiscoveryPlanProOptimization
            || this.JobType == JMConstants.JobType.DiscoveryPlanProScan;
        
        const isSOSubJobFilter = this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobSummary;
        return <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            onHide={this.hideFilterPanel}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
        >
            <JobDetailFilterForm
                ref={(ref) => this.jdFilterFormRef = ref}
                id="jdFilterForm"
                filterOptionsInfo={this.state.filterData}
                isShowTabsFilter={isShowTabsFilter}
                isShowDiscoveryPreScanFilter={isShowDiscoveryPreScanFilter}
                isSOSubJobFilter={isSOSubJobFilter}
            ></JobDetailFilterForm>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    renderJMHeader() {
        return <div className="ra-main-header">
            <R.Searchbox
                ref={r => this.searchBoxRef = r}
                placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                disabled={false}
                width="380"
                onSearch={this.onSearchStart}
            />
            <R.Button icon="fia-filter" text={RMResx.RM_Common_Filter} onClick={this.openFilterPanel} />
        </div>;
    }

    getNoramlSummary() {
        if (this.state.summaryModel.JobType || this.state.summaryModel.JobType == 0) {
            let jobType = this.state.summaryModel.JobType;
            let summaryModel = this.state.summaryModel;
            const isNewOpus = LicenseHelper.EnableRecordsArchiver();
            const isShowProgressStatistics = this.state.summaryModel.IsNewJob;
            let noramlSummary = [
                {
                    name: RMResx.RM_JS_JMD_Summary_JobType,
                    value: RMResx["RM_JS_JM_JobType_" + this.findKey(JMConstants.JobType, jobType)]
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_JobID,
                    value: summaryModel.JobId
                },
                {
                    name: RMResx.RM_JS_JM_ProfileName,
                    value: summaryModel.ProfileName,
                    hidden: !(jobType == JMConstants.JobType.BCSTermUsageReport
                        || jobType == JMConstants.JobType.RetiredTermReport
                        || jobType == JMConstants.JobType.OrphanedTermReport
                        || jobType == JMConstants.JobType.ItemsFilesDueDisposal
                        || jobType == JMConstants.JobType.CreateAndDestroyedFileReport
                        || jobType == JMConstants.JobType.AvailableSpaceReport
                        || jobType == JMConstants.JobType.TeamsBCSTermUsageReport
                        || jobType == JMConstants.JobType.TeamsOrphanedTermUsageReport
                        || jobType == JMConstants.JobType.TeamsRetiredTermUsageReport
                        || jobType == JMConstants.JobType.TeamsItemsFilesDueDisposalReport
                        || jobType == JMConstants.JobType.TeamsCreateAndDestroyedFileReport)
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_StartTime,
                    value: summaryModel.StartTime,
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_EndTime,
                    value: summaryModel.EndTime,
                },
                {
                    name: RMResx.RM_JS_JM_JobRunBy,
                    value: summaryModel.JobRunBy,
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Status,
                    value: JMConstants.JobStatusI18N[this.state.summaryModel.Status],
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Process_Mailbox,
                    value: summaryModel.ProgressSCStr,
                    hidden: !(jobType == JMConstants.JobType.EXORecordsDisposal),
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Process_Item,
                    value: summaryModel.ProgressFileCountStr,
                    hidden: !(jobType == JMConstants.JobType.EXORecordsDisposal),
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Process_TeamsAndGroups,
                    value: summaryModel.ProgressSCStr,
                    hidden: !(teamsArchiverBackupJobTypes.has(jobType) && !isShowProgressStatistics),
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Process_Site,
                    value: summaryModel.ProgressSCStr,
                    hidden: !(
                        (sitesArchiverBackupJobTypes.has(jobType) && !isShowProgressStatistics) ||
                        archiverOptimizationJobTypes.has(jobType)
                    ),
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Process_File,
                    value: summaryModel.ProgressFileCountStr,
                    hidden: !(
                        (isShowProgressTabTab.has(jobType) && !isShowProgressStatistics) ||
                        archiverOptimizationJobTypes.has(jobType)
                    ),
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Scope,
                    value: summaryModel.Scope,
                    hidden: !(jobType == JMConstants.JobType.RecordsDisposal
                        || jobType == JMConstants.JobType.OneDriveRecordsDisposal
                        || jobType == JMConstants.JobType.RMEndUserArchiverBackup
                        || jobType == JMConstants.JobType.RMArchiverBackup
                        || jobType == JMConstants.JobType.SpecifySitesArchiverBackup
                        || jobType == JMConstants.JobType.SOPreScan
                        || jobType == JMConstants.JobType.BoxDisposal
                        || jobType == JMConstants.JobType.ApprovalProcessArchive
                        || jobType == JMConstants.JobType.TeamsRecordsDisposal
                        || jobType == JMConstants.JobType.TeamsArchiverBackup
                        || jobType == JMConstants.JobType.SpecifyTeamsArchiverBackup
                        || jobType == JMConstants.JobType.TeamsPreScan
                        )
                },
                {
                    name: RMResx.RM_JM_JS_Location,
                    value: summaryModel.Scope,
                    hidden: !(
                        isNewOpus
                        &&
                        (jobType == JMConstants.JobType.PhysicalRecordsDisposal
                        ||jobType == JMConstants.JobType.EXORecordsDisposal
                        ||jobType == JMConstants.JobType.FSDisposal
                        ||jobType == JMConstants.JobType.FSDisposalSchedule
                        ||jobType == JMConstants.JobType.SPOnPremEnforceRuleAction
                        ||jobType == JMConstants.JobType.SPOnPremEnforceRuleActionSchedule
                        ||jobType == JMConstants.JobType.BoxRecordsDisposal
                        ||jobType == JMConstants.JobType.BoxDataSynchronisation
                        ||jobType == JMConstants.JobType.ApplySharePointSettings
                        ||jobType == JMConstants.JobType.EXOApplySetting
                        ||jobType == JMConstants.JobType.EXOApplySettingSchedule
                        ||jobType == JMConstants.JobType.SPOnPremApplySetting
                        ||jobType == JMConstants.JobType.SPOnPremApplySettingSchedule
                        ||jobType == JMConstants.JobType.ArchiverRestore
                        ||jobType == JMConstants.JobType.ArchiverOutPlaceRestore
                        ||jobType == JMConstants.JobType.StubOopRestore
                        ||jobType == JMConstants.JobType.FSArchiverRestore
                        ||jobType == JMConstants.JobType.ApplyTeamsSettings
                        ||jobType == JMConstants.JobType.TeamsArchiverRestore
                        ||jobType == JMConstants.JobType.TeamsOutPlaceRestore
                        ||jobType == JMConstants.JobType.MailBoxArchiverRestore
                        ||jobType == JMConstants.JobType.GoogleArchiverRestore
                        ||jobType == JMConstants.JobType.ArchiverToSpoRestore
                        ||jobType == JMConstants.JobType.FSDisposalByClassCode
                        ||jobType == JMConstants.JobType.StubArchiverRestore
                        ||jobType == JMConstants.JobType.M365InPlaceArchiverRestore
                        )) 
                },
                {
                    name: RMResx.RM_JS_JMD_EstimatedOptimize,
                    value: this.state.summaryModel.EstimatedOptimizeDataSize,
                    hidden:  !(jobType === JMConstants.JobType.SOPreScan || jobType === JMConstants.JobType.TeamsPreScan || jobType === JMConstants.JobType.DiscoveryPreScan)
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Comment,
                    value: this.state.summaryModel.Comment
                }
            ];
            noramlSummary = noramlSummary.filter((item) => { return !item.hidden; });
            return <React.Fragment>
                <JMDetailList
                    textField={"name"}
                    valueField={"value"}
                    title={RMResx.RM_JS_JMD_GeneralSetting}
                    data={noramlSummary}>
                </JMDetailList>
            </React.Fragment>;
        }
    }

    getJobSetting() {
        $$.loading(true);
        let option = {
            url: "/api/JMApi/GetJobSetting",
            method: "POST",
            data: {
                JobId: this.Jobid,
                JobType: this.JobType
            }
        };
        fetchUtility(option).then((data) => {
            if (data) {
                this.setState({
                    jobSettingModel: {
                        ...data,
                        Settings: JSON.parse(data.Settings)
                    },
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleRerunFailedItems = () => {
        const restoreTypeMap = {
            [JMConstants.JobType.ArchiverRestore]: RestoreType.InPlace,
            [JMConstants.JobType.ArchiverOutPlaceRestore]: RestoreType.OutOfPlace,
            [JMConstants.JobType.StubOopRestore]: RestoreType.OutOfPlace,
        };

        const option = {
            url: "/api/ArchiverRestore/SaveRestoreSettingAndRun",
            method: "POST",
            data: {
                RestoreTypeSelect: restoreTypeMap[this.JobType],
                FailedJobId: this.Jobid,
            }
        };

        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                let content = <></>;
                switch (res.MessageType) {
                    case MessageType.Successful:
                        content = (
                            <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        );
                        showToast.success(content);
                        break;
                    case MessageType.Exception:
                        content = (
                            <$g.I18NProvider msg={RMResx.RM_AR_RC_Panel_RunJobException}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        );
                        showToast.success(content);
                        break;
                    default:
                        showToast.error(res.ErrorMessage);
                        break;
                }
            })
            .catch(() => {
                showToast.error(RMResx.RM_RS_SaveRestoreSettingError);
            })
            .finally(() => $$.loading(false));
    }

    getJobSettingSummary() {
        const jobSettings = this.state.jobSettingModel.Settings;
        const isShowJobSetting = this.JobType === JMConstants.JobType.ApplySharePointSettings || this.JobType === JMConstants.JobType.ApplyTeamsSettings;
        
        if (!jobSettings) {
            return null;
        }

        const data = jobSettings.map(s => ({
            value: this.getJobSettingContent(s)
        }));

        return <div className="margin-top-m">
            <R.Expander
                status={false}
                groupName="title"
                title={RMResx["RM_JS_ArchiverMigration_DataType_ArchiverRuleMapping"]}
            >
                <div>
                    <JMDetailList
                        valueField={"value"}
                        data={data}
                        isShowJobSetting={isShowJobSetting}
                    />
                </div>
            </R.Expander>
        </div>;
    }

    getJobSettingContent(setting) {
        let jobContents = [];

        switch (this.JobType) {
            case JMConstants.JobType.ApplySharePointSettings:
            case JMConstants.JobType.ApplyTeamsSettings:
                const settingKeys = Object.keys(setting);
                jobContents = settingKeys.map(key => {
                    if(key && RMResx[key]){
                        return `${RMResx[key].replace(":","")}: ${setting[key] ?? ""}`;
                    }
                });
                break;
        }

        return (
            <>
                {jobContents.map((content, i) => <p key={i}>{content}</p>)}
            </>
        )
    }

    getDedupJobSummary() {
        let statisticsData = this.state.soSummaryModel && this.state.soSummaryModel.ActionStatistics && this.state.soSummaryModel.ActionStatistics[0];
        if (!statisticsData || !statisticsData.SuccessfulObj || !statisticsData.FailedObj || !statisticsData.SkippedObj) {
            return null;
        }

        let jobSummaryData = [
            {
                name: RMResx.RM_JM_SOSummary_Column_SuccessfulNumber,
                value: statisticsData.SuccessfulObj.ItemCount
            },
            {
                name: RMResx.RM_JM_SOSummary_Column_FailedNumber,
                value: statisticsData.FailedObj.ItemCount
            },
            {
                name: RMResx.RM_JM_SOSummary_Column_SkipNumber,
                value: statisticsData.SkippedObj.ItemCount
            },
            {
                name: RMResx.RM_JM_Summary_Column_Total_Deduped_Size,
                value: statisticsData.SizeStr
            },
        ];
        
        return <div className="margin-top-m">
            <R.Expander
                status={false}
                groupName="title"
                title={RMResx.RM_JM_Summary_DedupTitle}
            >
                <div>
                    <JMDetailList
                        textField={"name"}
                        valueField={"value"}
                        data={jobSummaryData}
                    >
                    </JMDetailList>
                </div>
            </R.Expander>
        </div>;
    }

    getDedupReportJobSummary() {
        let statisticsData = this.state.soSummaryModel;
        if (!statisticsData) {
            return null;
        }

        let jobSummaryData = [
            {
                name: RMResx.RM_JM_SOSummary_Column_SuccessSitefulNumber,
                value: statisticsData.SiteCollectionCount
            },
            {
                name: RMResx.RM_JM_SOSummary_Column_FailedSiteNumber,
                value: statisticsData.FailedSiteCollectionCount
            },
            {
                name: RMResx.RM_JM_Summary_Column_Total_Deduped_Cout,
                value: statisticsData.TotalDedupFilesCount
            },
            {
                name: RMResx.RM_JM_Summary_Column_Total_Deduped_Size,
                value: statisticsData.TotalDedupFilesSizeStr
            },
        ];
        
        return <div className="margin-top-m">
            <R.Expander
                status={false}
                groupName="title"
                title={RMResx.RM_JM_Summary_DedupTitle}
            >
                <div>
                    <JMDetailList
                        textField={"name"}
                        valueField={"value"}
                        data={jobSummaryData}
                    >
                    </JMDetailList>
                </div>
            </R.Expander>
        </div>;
    }

    isHideTotalSize() {
        const hiddenJobTypes = new Set([
            JMConstants.JobType.BoxDisposal,
            JMConstants.JobType.SOPreScan,
            JMConstants.JobType.DiscoverOptimization,
            JMConstants.JobType.DiscoveryPreScan,
            JMConstants.JobType.DiscoveryPlanProOptimization,
            JMConstants.JobType.DiscoveryPlanProScan,
            JMConstants.JobType.RMArchiverBackup,
            JMConstants.JobType.RecordsDisposal,
            JMConstants.JobType.OneDriveRecordsDisposal,
            JMConstants.JobType.TeamsRecordsDisposal,
            JMConstants.JobType.TeamsPreScan,
            JMConstants.JobType.TeamsArchiverBackup
        ]);

        return hiddenJobTypes.has(this.JobType);
    }

    getSOJobSummary() {
        let soSummaryModel = this.state.soSummaryModel;
        let soSubJobSummaryModel = this.state.soSubJobSummary;
        const isSOSubJobSummary = this.isSkipMergeDetailsJob && this.state.subJobView === JMConstants.SubJobTabType.SubJobDetails;
        const isSOSubJobDataAvailable = isSOSubJobSummary && soSubJobSummaryModel && soSubJobSummaryModel.ActionStatistics;
        if ((soSummaryModel && soSummaryModel.ActionStatistics) || isSOSubJobDataAvailable) {
            const actionStatisticsData = isSOSubJobSummary ? soSubJobSummaryModel.ActionStatistics : soSummaryModel.ActionStatistics;
            return actionStatisticsData?.map((item, index) => {
                let title = "";
                let soJobSummary = [];
                if (item.ActionTab === JMConstants.ActionTab.Scan
                    || item.ActionTab === JMConstants.ActionTab.Export
                    || item.ActionTab === JMConstants.ActionTab.Archive
                    || item.ActionTab === JMConstants.ActionTab.Others
                    || item.ActionTab === JMConstants.ActionTab.Restore) {
                    soJobSummary = [
                        {
                            name: RMResx.RM_JM_SOSummary_Column_SuccessfulNumber,
                            value: this.getSOJobSummaryContent(item.SuccessfulObj),
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_FailedNumber,
                            value: this.getSOJobSummaryContent(item.FailedObj),
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_SkipNumber,
                            value: this.getSOJobSummaryContent(item.SkippedObj),
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_TotalSize,
                            value: item.SizeStr,
                            hidden: item.ActionTab === JMConstants.ActionTab.Others || item.ActionTab === JMConstants.ActionTab.Archive || this.isHideTotalSize()
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_Total_Archived_Size,
                            value: item?.SizeStr || "",
                            hidden: item.ActionTab !== JMConstants.ActionTab.Archive,
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_Total_Deletion_Size,
                            value: item?.DeleteSizeStr || "",
                            hidden: item.ActionTab !== JMConstants.ActionTab.Others,
                        },
                        {
                            name: RMResx.RM_JS_JMD_Summary_Status,
                            value: JMConstants.JobStatusI18N[item.Status],
                        },
                    ];
                } else if (item.ActionTab === JMConstants.ActionTab.DOJobSettings) {
                    if (item.ScopeSettings != null) {
                        soJobSummary.push(
                            {
                                name: RMResx.RM_FA_Inactive_DSOJobSummaryMS365DataFilterTypeTitle,
                                value: item.ScopeSettings.MS365DataTypeStr,
                            },
                            {
                                name: RMResx.RM_FA_Inactive_ModifiedTitle,
                                value: item.ScopeSettings.ModifiedTimeRangeStr,
                            },
                            {
                                name: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle,
                                value: item.ScopeSettings.SizeRangeStr,
                            },
                            {
                                name: RMResx.RM_FA_Inactive_OptimizationTab_FileCategoryTitle,
                                value: item.ScopeSettings.FileCatagorysStr,
                            }
                        );
                    }
                    if (item.DefinitionAndActionSettings != null) {
                        soJobSummary.push(
                            {
                                name: RMResx.RM_JM_DOSummary_Column_Rules,
                                value: item.DefinitionAndActionSettings
                                    .DefinitionsStr,
                            },
                            {
                                name: RMResx.RM_JM_DOSummary_Column_DocumentAction,
                                value: item.DefinitionAndActionSettings
                                    .DocumentActionStr,
                            },
                            {
                                name: RMResx.RM_JM_DOSummary_Column_DocumentVersionAction,
                                value: item.DefinitionAndActionSettings
                                    .DocumentVersionActionStr,
                            }
                        );
                    }
                } else if (item.ActionTab === JMConstants.ActionTab.Delete) {
                    soJobSummary = [
                        {
                            name: RMResx.RM_JM_SOSummary_Column_SuccessfulNumber,
                            value: this.getSOJobSummaryContent(item.SuccessfulObj),
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_FailedNumber,
                            value: this.getSOJobSummaryContent(item.FailedObj),
                        },
                        {
                            name: RMResx.RM_JM_SOSummary_Column_SkipNumber,
                            value: this.getSOJobSummaryContent(item.SkippedObj),
                        },
                        {
                            name: RMResx.RM_JS_JMD_Summary_Status,
                            value: JMConstants.JobStatusI18N[item.Status],
                        },
                    ]
                }
                soJobSummary = soJobSummary.filter((item) => { return !item.hidden; });

                switch (item.ActionTab) {
                    case JMConstants.ActionTab.Scan:
                        title = RMResx.RM_JM_SOSummary_ScanTitle;
                        break;
                    case JMConstants.ActionTab.Export:
                        title = RMResx.RM_JM_SOSummary_ExportTitle;
                        break;
                    case JMConstants.ActionTab.Archive:
                        title = RMResx.RM_JM_SOSummary_ArchivingTitle;
                        break;
                    case JMConstants.ActionTab.Others:
                        title = RMResx.RM_JM_SOSummary_OthersTitle;
                        break;
                    case JMConstants.ActionTab.Restore:
                        title = RMResx.RM_JM_SOSummary_RestoreTitle;
                        break;
                    case JMConstants.ActionTab.DOJobSettings:
                        title = RMResx.RM_JM_DOSummary_SettingTitle;
                        break;
                    case JMConstants.ActionTab.Delete: 
                        title = RMResx.RM_JM_SOSummary_DeleteTitle;
                        break;
                }
                return <div className="margin-top-m" key={index}>
                    <R.Expander
                        status={false}
                        groupName="title"
                        title={title}
                    >
                        <div>
                            <JMDetailList
                                textField={"name"}
                                valueField={"value"}
                                data={soJobSummary}
                            >
                            </JMDetailList>
                        </div>
                    </R.Expander>
                </div>;
            });
        }
    }

    getEXOJobSummary() {
        let soSummaryModel = this.state.soSummaryModel;
        
        if (soSummaryModel && soSummaryModel.ActionStatistics) {
            return soSummaryModel.ActionStatistics.map((item, index) => {
                let title = "";
                let exoJobSummary = [
                    {
                        name: RMResx.RM_JM_SOSummary_Column_SuccessfulNumber,
                        value: this.getSOJobSummaryContent(item.SuccessfulObj),
                    },
                    {
                        name: RMResx.RM_JM_SOSummary_Column_FailedNumber,
                        value: this.getSOJobSummaryContent(item.FailedObj),
                    },
                    {
                        name: RMResx.RM_JM_SOSummary_Column_SkipNumber,
                        value: this.getSOJobSummaryContent(item.SkippedObj),
                    },
                    {
                        name: RMResx.RM_JS_JMD_Summary_Status,
                        value: JMConstants.JobStatusI18N[item.Status],
                    },
                ];
                exoJobSummary = exoJobSummary.filter((item) => { return !item.hidden; });
                switch (item.ActionTab) {
                    case JMConstants.ActionTab.Scan:
                        title = RMResx.RM_JM_SOSummary_ScanTitle;
                        break;
                    case JMConstants.ActionTab.Export:
                        title = RMResx.RM_JM_SOSummary_ExportTitle;
                        break;
                    case JMConstants.ActionTab.Archive:
                        title = RMResx.RM_JM_SOSummary_ArchivingTitle;
                        break;
                    case JMConstants.ActionTab.Others:
                        title = RMResx.RM_JM_SOSummary_OthersTitle;
                        break;
                    case JMConstants.ActionTab.Restore:
                        title = RMResx.RM_JM_SOSummary_RestoreTitle;
                        break;
                    case JMConstants.ActionTab.DOJobSettings:
                        title = RMResx.RM_JM_DOSummary_SettingTitle;
                        break;
                    case JMConstants.ActionTab.Delete: 
                        title = RMResx.RM_JM_SOSummary_DeleteTitle;
                        break;
                }
                return <div className="margin-top-m" key={index}>
                    <R.Expander
                        status={false}
                        groupName="title"
                        title={title}
                    >
                        <div>
                            <JMDetailList
                                textField={"name"}
                                valueField={"value"}
                                data={exoJobSummary}
                            >
                            </JMDetailList>
                        </div>
                    </R.Expander>
                </div>;
                })
            }
        }

    getSOJobSummaryContent(countObject) {
        let content = <><span className="jm-sosummary-count">{countObject.TotleCount} </span>
            <$g.I18NProvider msg={RMResx.RM_JM_SOSummary_NumberContent}>
                <span>{countObject.SiteCollectionCount}</span>
                <span>{countObject.SiteCount}</span>
                <span>{countObject.ListCount}</span>
                <span>{countObject.FolderCount}</span>
                <span>{countObject.ItemCount}</span>
            </$g.I18NProvider></>;

        if (this.JobType == JMConstants.JobType.BoxDisposal) {
            content = <><span className="jm-sosummary-count">{countObject.BoxTotalCount} </span>
                <$g.I18NProvider msg={RMResx.RM_JM_BoxSummary_NumberContent}>
                    <span>{countObject.ConnectionCount}</span>
                    <span>{countObject.UserCount}</span>
                    <span>{countObject.FolderCount}</span>
                    <span>{countObject.FileCount}</span>
                </$g.I18NProvider></>
        }

        if (this.JobType == JMConstants.JobType.FSArchiverRestore) {
            content = (
                <>
                    <span className="jm-sosummary-count">{countObject.TotleCount} </span>
                    <$g.I18NProvider msg={RMResx.RM_JM_FS_SOSummary_NumberContent}>
                        <span>{countObject.ItemCount}</span>
                    </$g.I18NProvider>
                </>
            );
        }

        if (this.JobType == JMConstants.JobType.TeamsArchiverBackup || this.JobType == JMConstants.JobType.TeamsArchiverRestore || this.JobType == JMConstants.JobType.TeamsOutPlaceRestore || this.JobType == JMConstants.JobType.MailBoxArchiverRestore
            || this.JobType == JMConstants.JobType.TeamsPreScan || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup
        ) {
            content = (
                <>
                    <span className="jm-sosummary-count">{countObject.TeamsTotalCount} </span>
                    <$g.I18NProvider msg={RMResx.RM_JM_Teams_SOSummary_NumberContent}>
                        <span>{countObject.TeamsGroupCount}</span>
                        <span>{countObject.ChannelCount}</span>
                        <span>{countObject.PlanCount}</span>
                        <span>{countObject.TaskCount}</span>
                        <span>{countObject.ChannelConversationCount}</span>
                        <span>{countObject.GroupMailboxCount}</span>
                        <span>{countObject.GroupMailboxItemCount}</span>
                        <span>{countObject.SiteCollectionCount}</span>
                        <span>{countObject.SiteCount}</span>
                        <span>{countObject.ListCount}</span>
                        <span>{countObject.FolderCount}</span>
                        <span>{countObject.ItemCount}</span>
                    </$g.I18NProvider>
                </>
            )
        }

        if (this.JobType == JMConstants.JobType.GoogleRecordsDisposal || this.JobType == JMConstants.JobType.GoogleArchiverRestore) {
            content = (
                <>
                    <span className="jm-sosummary-count">{countObject.DriveTotalCount}</span>
                    <$g.I18NProvider msg={RMResx.RM_JM_Google_SOSummary_NumberContent}>
                        <span>{countObject.DriveCount}</span>
                        <span>{countObject.FolderCount}</span>
                        <span>{countObject.ItemCount}</span>
                    </$g.I18NProvider>
                </>
            )
        }
        if (this.JobType == JMConstants.JobType.EXORecordsDisposal) {
            content = (
                <>
                    <$g.I18NProvider msg={RMResx.RM_JM_ExchangeOnline_SOSummary_NumberContent}>
                        <span className="jm-sosummary-count">{countObject.TeamsTotalCount}</span>
                        <span>{countObject.GroupMailboxCount}</span>
                        <span>{countObject.GroupMailboxFolderCount}</span>
                        <span>{countObject.GroupMailboxItemCount}</span>
                    </$g.I18NProvider>
                </>
            )
        }
        return content;
    }

    getDisposalSummary() {
        let disposalSummary = this.state.summaryModel.DisposalSummary;
        return <div>
            {(this.state.summaryModel.JobType == JMConstants.JobType.ArchiverScan
                || this.state.summaryModel.JobType == JMConstants.JobType.ArchiverBackup
                || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverScan
                || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverBackup
                || this.state.summaryModel.JobType == JMConstants.JobType.PhysicalDisposal
                || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverRestore
                || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverRetention
                || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverFileLevelRetention
                || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverScan
                || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverBackup) &&
                disposalSummary && disposalSummary.SummaryItem.map((item, index) => {
                    return <div key={index} className="margin-bottom-m">
                        <JMDetailList
                            textField={"Key"}
                            valueField={"Value"}
                            title={item.Title}
                            data={item.SummaryRow}>
                        </JMDetailList>
                    </div>;
                })}
        </div>;
    }

    renderJobDetailTable() {
        return <div className="ra-main-table">
            <JMTable
                id="jobDetailTable"
                template={JobDetailTemplate}
                jobType={this.JobType}
            />
        </div>;
    }

    renderM365SOJobDetailTable() {
        return <div className="ra-main-table">
            <JMTable
                id="M365SOJobDetailTable"
                template={SubJobDetailTemplate}
                jobType={this.JobType}
                onCellClick={this.handleSubJobCellClick}
            />
        </div>;
    }

    renderFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.jobsCount}
                pagerIndex={this.state.jobsPagerIndex}
                pagerSize={this.state.jobsPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.jqPageChange} />
        </div>;
    }

    renderSubJobSummaryFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.subJobSummaryCount}
                pagerIndex={this.state.jobsPagerIndex}
                pagerSize={this.state.jobsPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.jqPageChange} />
        </div>;
    }

    renderJobTermTable() {
        return <div className="ra-page-container margin-top-l">
            <JMTable
                id="jobDetailTerm"
                template={JobDetailTermTemplate}
            />
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={this.state.termsCount}
                    pagerIndex={this.state.termsPagerIndex}
                    pagerSize={this.state.termsPagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onTermsPageChange} />
            </div>
        </div>;

    }

    renderDetailTabContent() {
        // For M365SOJobs, show sub-job summary or sub-job detail
        if (this.isSkipMergeDetailsJob) {
            if (this.state.subJobView === JMConstants.SubJobTabType.SubJobDetails) {
                return this.renderSubJobDetailView();
            }
            return this.renderM365SOJobDetailView();
        }

        // Default: original detail table
        return (
            <div className="ra-page-container">
                {this.renderJMHeader()}
                {this.renderJobDetailTable()}
                {this.renderFooter()}
            </div>
        );
    }

    renderM365SOJobDetailView() {
        return (
            <div className="ra-page-container">
                {this.renderJMHeader()}
                {this.renderM365SOJobDetailTable()}
                {this.renderSubJobSummaryFooter()}
            </div>
        );
    }

    renderSubJobDetailView() {
        return (
            <div>
                <div className="margin-bottom-l">
                    <R.Button
                        id="raBackBtn"
                        classify="blank"
                        icon="fia-arrow-line-left"
                        text={this.state.selectedSubJob?.Scope ?? ''}
                        onClick={this.handleBackToSubJobSummary}
                    />
                </div>
                <div className="margin-bottom-m">
                    {this.getSOJobSummary()}
                </div>
                <div className="ra-page-container">
                    {this.renderJMHeader()}
                    {this.renderJobDetailTable()}
                    {this.renderFooter()}
                </div>
            </div>
        );
    }

    onProgressSearch = (args) => {
        this.setState({
            progressFilterData: {
                ...this.state.progressFilterData,
                SearchValue: args,
                PageNumber: 1
            }
        }, () => this.getProgressTabContent());
    }

    onProgressPageChange = (pagerIndex, pagerSize, callback) => {
        this.setState({
            progressFilterData: {
                ...this.state.progressFilterData,
                PageNumber: pagerIndex + 1,
                PageSize: pagerSize
            }
        }, () => this.getProgressTabContent());
        callback(true);
    }

    openProgressFilterPanel = () => {
        this.setState({
            showProgressFilterPanel: true
        })
    }

    hideProgressFilterPanel = () => {
        this.setState({ showProgressFilterPanel: false })
    }

    onProgressFilter = () => {
        const callback = (filterData) => {
            const filterParams = filterData && filterData.map(item => item.Id);
            this.setState({
                progressFilterData: {
                    ...this.state.progressFilterData,
                    StatusFilter: filterParams && filterParams,
                    PageNumber: 1,
                },
                showProgressFilterPanel: false,
            }, () => this.getProgressTabContent());
        }
        this.dispatch("progressFilterForm", "onFilter", callback);
    }

    onRefreshProgress = () => {
        this.setState({
            progressFilterData: {
                ...this.state.progressFilterData,
                PageNumber: 1,
                StatusFilter: [],
                SearchValue: "",
            }
        }, () => this.getProgressTabContent());
    }

    renderProgressHeader() {
        return (
            <div>
                <div className="ra-main-header">
                    <R.Searchbox
                        ref={r => this.progressSearchBoxRef = r}
                        placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                        disabled={false}
                        width="380"
                        value={this.state.progressFilterData.SearchValue}
                        onSearch={this.onProgressSearch}
                    />
                    <R.Button icon="fia-filter" text={RMResx.RM_Common_Filter } onClick={this.openProgressFilterPanel} />
                </div>
                <R.Button className="ra-main-header" style={{ paddingTop: 0 }} icon="fia-refresh" text={RMResx.RM_JS_JM_Progress_Refresh_Btn } onClick={this.onRefreshProgress} />
            </div>
        );
    }

    renderProgressTable() {
        return <div className="ra-main-table">
            <JMTable
                id="jobProgressTable"
                template={JobProgressTemplate}
                jobType={this.JobType}
            />
        </div>
    }

    renderProgressStatistics() {
        const progressStatisticsData = this.state.progressStatisticsData;
        if (!progressStatisticsData || typeof progressStatisticsData !== "object") {
            return null;
        }

        let data = [
            {
                name: RMResx.RM_JS_JMD_Progress_Finished_SiteCollection,
                value: progressStatisticsData.ProcessedSites,
                hidden: teamsArchiverBackupJobTypes.has(this.JobType),
            },
            {
                name: RMResx.RM_JS_JMD_Progress_Finished_TeamsGroupsSites,
                value: progressStatisticsData.ProcessedSites,
                hidden: !teamsArchiverBackupJobTypes.has(this.JobType),
            },
            {
                name: RMResx.RM_JS_JMD_Progress_Processed_Files,
                value: progressStatisticsData.ProcessedFiles,
            },
            {
                name: RMResx.RM_JS_JMD_Progress_Archived_Files,
                value: progressStatisticsData.ProcessedSize,
            },
            {
                name: RMResx.RM_JS_JMD_Progress_Estimated_Finish_Time,
                value: progressStatisticsData.EstimatedFinishTime,
            },
            {
                name: RMResx.RM_JS_JMD_Progress_Last_Update_Time,
                value: progressStatisticsData.LastUpdateTime || "",
            },
        ];

        data = data.filter((item) => { return !item.hidden; }); 

        return <div className="margin-bottom-m">
            <R.Expander
                status={false}
                groupName="title"
                title={RMResx.RM_JS_JMD_Tab_Progress_Statistics}
            >
                <div>
                    <JMDetailList
                        textField={"name"}
                        valueField={"value"}
                        data={data}
                    />
                </div>
            </R.Expander>
        </div>
    }

    renderProgressFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.jobsCount}
                pagerIndex={this.state.progressFilterData.PageNumber - 1}
                pagerSize={this.state.progressFilterData.PageSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onProgressPageChange} />
        </div>;
    }

    renderProgressTabContent() {
        const isShowProgressStatistics = this.state.summaryModel.IsNewJob;
        return (
            <div>
                {isShowProgressStatistics && this.renderProgressStatistics()}
                <div className="ra-page-container">
                    {this.renderProgressHeader()}
                    {this.renderProgressTable()}
                    {this.renderProgressFooter()}
                </div>
            </div>
        );
    }

    renderProgressFilterPanel() {
        return <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            onHide={this.hideProgressFilterPanel}
            status={{ show: this.state.showProgressFilterPanel }}
            destroy={true}
        >
            <JMProgressFilterForm id="progressFilterForm" statusFilter={this.state.progressFilterData.StatusFilter}/>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideProgressFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onProgressFilter} />
            </>
        </R.Panel>;
    }

    render() {
        const { summaryModel } = this.state;
        let usePlanDetail = this.JobType == JMConstants.JobType.ArchiverBackup
            || this.JobType == JMConstants.JobType.ArchiverScan
            || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverScan
            || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverBackup
            || this.state.summaryModel.JobType == JMConstants.JobType.PhysicalDisposal
            || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverScan
            || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverBackup;
        const supportJobTypeForRerunFailedItems = new Set([
            JMConstants.JobType.ArchiverRestore,
            JMConstants.JobType.ArchiverOutPlaceRestore,
            JMConstants.JobType.StubOopRestore,
        ]);
        const supportStatusForRerunFailedItems = new Set([
            JMConstants.StatusCode.FinishWithException
        ]);
        const isFailedRestoreStatus = summaryModel && Object.keys(summaryModel).length > 0 && supportStatusForRerunFailedItems.has(summaryModel.Status);

        return <React.Fragment>
            <div id="rmJobDetails">
                {usePlanDetail &&
                    <$g.SiteMap data={[SiteMapLinks.JM, {
                        text: RMResx.RM_JS_JM_PlanDetails,
                        url: RouterUrls.JM_PlanDetail + '/?id=' + this.DisposalId,
                    }, SiteMapLinks.JM_DETAIL]} />
                }
                {!usePlanDetail &&
                    <$g.SiteMap data={[SiteMapLinks.JM, SiteMapLinks.JM_DETAIL]}>
                        {supportJobTypeForRerunFailedItems.has(this.JobType) && isFailedRestoreStatus && (
                            <div className="text-end">
                                <R.Button
                                    type="button"
                                    icon="fia-restart"
                                    text={RMResx.RM_JS_JMD_Rerun_FailedItems_Btn}
                                    classify="info"
                                    primary={true}
                                    onClick={this.handleRerunFailedItems}
                                />
                            </div>
                        )}
                    </$g.SiteMap>
                }
                <div id="raJobDetailModule">
                    <R.Tabcontrol
                        active={this.state.index}
                        onChange={this.handleSelectedIndexChanged}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_JS_JMD_Tab_Summary}>
                            <div id="_summary">
                                <div>
                                    {this.getDisposalSummary()}
                                </div>
                                {
                                    !(this.state.summaryModel.JobType == JMConstants.JobType.ArchiverScan
                                        || this.state.summaryModel.JobType == JMConstants.JobType.ArchiverBackup
                                        || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverScan
                                        || this.state.summaryModel.JobType == JMConstants.JobType.ExchangeArchiverBackup
                                        || this.state.summaryModel.JobType == JMConstants.JobType.PhysicalDisposal
                                        || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverRestore
                                        || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverRetention
                                        || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverFileLevelRetention
                                        || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverScan
                                        || this.state.summaryModel.JobType == JMConstants.JobType.MigrationArchiverBackup) &&
                                    this.getNoramlSummary()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.BCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.RetiredTermReport
                                        || this.JobType == JMConstants.JobType.OrphanedTermReport
                                        || this.JobType == JMConstants.JobType.EXOTermUsageReport
                                        || this.JobType == JMConstants.JobType.EXORetiredTermUsageReport
                                        || this.JobType == JMConstants.JobType.EXOOrphanedTermUsageReport
                                        || this.JobType == JMConstants.JobType.PhysicalTermUsageReport
                                        || this.JobType == JMConstants.JobType.PhysicalOrphanedTermUsageReport
                                        || this.JobType == JMConstants.JobType.PhysicalRetiredTermUsageReport
                                        || this.JobType == JMConstants.JobType.FSBCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.FSOrphanedTermReport
                                        || this.JobType == JMConstants.JobType.FSRetiredTermReport
                                        || this.JobType == JMConstants.JobType.SPOnPremBCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.SPOnPremOrphanedTermReport
                                        || this.JobType == JMConstants.JobType.SPOnPremRetiredTermReport
                                        || this.JobType == JMConstants.JobType.OneDriveTermUsageReport
                                        || this.JobType == JMConstants.JobType.BoxBCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.BoxOrphanedTermUsageReport
                                        || this.JobType == JMConstants.JobType.BoxRetiredTermUsageReport
                                        || this.JobType == JMConstants.JobType.TermUsageReport
                                        || this.JobType == JMConstants.JobType.GoogleBCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.GoogleOrphanedTermUsageReport
                                        || this.JobType == JMConstants.JobType.GoogleRetiredTermUsageReport
                                        || this.JobType == JMConstants.JobType.TeamsBCSTermUsageReport
                                        || this.JobType == JMConstants.JobType.TeamsOrphanedTermUsageReport
                                        || this.JobType == JMConstants.JobType.TeamsRetiredTermUsageReport
                                    ) && this.renderJobTermTable()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.RecordsDisposal
                                        || this.JobType == JMConstants.JobType.OneDriveRecordsDisposal
                                        || this.JobType == JMConstants.JobType.RMEndUserArchiverBackup
                                        || this.JobType == JMConstants.JobType.RMArchiverBackup
                                        || this.JobType == JMConstants.JobType.SpecifySitesArchiverBackup
                                        || this.JobType == JMConstants.JobType.SOPreScan
                                        || this.JobType == JMConstants.JobType.DiscoverOptimization
                                        || this.JobType == JMConstants.JobType.ApprovalProcessArchive
                                        || this.JobType == JMConstants.JobType.DiscoveryPreScan
                                        || this.JobType == JMConstants.JobType.DiscoveryPlanProOptimization
                                        || this.JobType == JMConstants.JobType.DiscoveryPlanProScan
                                        || this.JobType == JMConstants.JobType.TeamsRecordsDisposal
                                        || this.JobType == JMConstants.JobType.TeamsArchiverBackup
                                        || this.JobType == JMConstants.JobType.SpecifyTeamsArchiverBackup
                                        || this.JobType == JMConstants.JobType.TeamsArchiverRestore
                                        || this.JobType == JMConstants.JobType.TeamsOutPlaceRestore
                                        || this.JobType == JMConstants.JobType.ArchiverRestore
                                        || this.JobType == JMConstants.JobType.FSArchiverRestore
                                        || this.JobType == JMConstants.JobType.ArchiverOutPlaceRestore
                                        || this.JobType == JMConstants.JobType.StubOopRestore
                                        || this.JobType == JMConstants.JobType.MailBoxArchiverRestore
                                        || this.JobType == JMConstants.JobType.TeamsPreScan
                                        || this.JobType == JMConstants.JobType.GoogleRecordsDisposal
                                        || this.JobType == JMConstants.JobType.GoogleArchiverRestore
                                        || this.JobType == JMConstants.JobType.ArchiverByHSMXml
                                        || this.JobType == JMConstants.JobType.CleanUpDuplicateDatas
                                        || this.JobType == JMConstants.JobType.ArchiverToSpoRestore
                                        || this.JobType == JMConstants.JobType.StubArchiverRestore
                                        || this.JobType == JMConstants.JobType.M365InPlaceArchiverRestore
                                    ) && this.getSOJobSummary()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.ArchiverDeduplication) && this.getDedupJobSummary()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.ArchiverDeduplicationReport) && this.getDedupReportJobSummary()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.ApplySharePointSettings
                                        || this.JobType == JMConstants.JobType.ApplyTeamsSettings
                                    ) && this.getJobSettingSummary()
                                }
                                {
                                    (this.JobType == JMConstants.JobType.EXORecordsDisposal && this.getEXOJobSummary())
                                }
                            </div>
                        </R.TabPanel>
                        {
                            this.state.isShowProgressTab &&
                            <R.TabPanel tab={RMResx.RM_JS_JMD_Tab_Progress}>
                                <div>
                                    {this.renderProgressTabContent()}
                                </div>
                            </R.TabPanel>
                        }
                        {
                            this.state.isShowDetailTab && <R.TabPanel tab={RMResx.RM_JS_JMD_Tab_Details}>
                                <div>
                                    {this.renderDetailTabContent()}
                                </div>
                            </R.TabPanel>
                        }
                    </R.Tabcontrol>
                    {this.renderFilterPanel()}
                    {this.renderProgressFilterPanel()}
                </div>
            </div >
            <div className="jm-footer">
                <div className="jm-footer-btn">
                    <R.Button
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={this.onClose.bind(this)} />
                </div>
            </div>
        </React.Fragment>;
    }
}