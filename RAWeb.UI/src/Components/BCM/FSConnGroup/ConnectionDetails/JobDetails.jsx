import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { bindEvents, LicenseHelper } from "../../../../Utilities/CommonUtil";
import JMDetailList from "../../../JM/JMDetailList";
import JMTable from "../../../JM/JMTable";
import { JobDetailTemplate } from "../../../JM/JMTableTemplate";
import { JobDetailFilterForm } from "../../../JM/JobMonitorFilterForm";
import {
    FSAgentJobI18N,
    FSAgentJobTypes,
    FSJobDetailsCells,
    FSJobDetailsColumns,
    FSJobDetailsColumnsWidth,
    FSJobDetailSearchKeys,
    FSJobDetailStatusI18N,
    FSJobStatusI18N
} from "./Constants";
import "./index.less";

class FSJobDetails extends R.Component {
    constructor(props) {
        super(props);
        if (props.location.state) {
            this.JobId = props.location.state.jobId;
            this.JobType = props.location.state.jobType;
        }
        this.filterData = {
            JobID: this.JobId,
            JobType: this.JobType,
            SearchValue: '',
            SearcheKeys: FSJobDetailSearchKeys[this.JobType],
            PageSize: 15,
            CurrentPage: 1,
            StatusFilters: [],
            EntityTypeFilters: [],
            ActionTabFilters: [],
            ArchiverActionFilters: [],
        }
        this.state = {
            jobsPagerIndex: 0,
            jobsPagerSize: 15,
            tabIndex: 0,
            jobsCount: 0,
            summaryModel: {},
            soSummaryModel: {},
            showFilterPanel: false,
            items: [],
            columns: this.getDetailColumns(),
            filterData: this.filterData
        }
        bindEvents(this, "handleSelectedIndexChanged", 'onSearchStart', 'jqPageChange');
    }

    componentInit() {
        this.getJobDetail();
    }

    convertStatusStr(statusCode) {
        return FSJobDetailStatusI18N[statusCode];
    }

    convertCommentString(comment) {
        if (comment && ((comment.indexOf("0x80070005") + comment.indexOf("E_ACCESSDENIED")) > -1)) {
            return RMResx.RM_JM_Details_Failed_AccessDenied;
        }
        return comment;
    }

    getJobDetail() {
        if (this.JobId) {
            $$.loading(true);
            let option = {
                url: "/api/JMApi/GetJobSummary",
                method: "POST",
                data: this.JobId
            };
            fetchUtility(option).then((data) => {
                if (data) {
                    this.setState({
                        summaryModel: data,
                    }, () => {
                        if (data.JobType == FSAgentJobTypes.FSArchiverRestore) {
                            this.getRestoreJobSummaryDetails();
                        }
                    });
                }
                $$.loading(false);
            }).catch((_e) => {
                $$.loading(false);
            });
        }
    }

    getRestoreJobSummaryDetails = () => {
        $$.loading(true);
        const option = {
            url: "/api/JMApi/GetRestoreJobSummaryDetails",
            method: "POST",
            data: this.JobId
        };
        fetchUtility(option).then((data) => {
            if (data) {
                this.setState({
                    soSummaryModel: data,
                });
            }
            $$.loading(false);
        }).catch((_e) => {
            $$.loading(false);
        });
    }

    getJobDetailItems(cellInfo) {
        let cellValues = [];
        let cellKeys = FSJobDetailsCells[this.JobType];
        if (cellKeys) {
            for (let cellKey of cellKeys) {
                let cellValue = cellInfo[cellKey];
                switch (cellKey) {
                    case "Status":
                        cellValue = this.convertStatusStr(cellInfo[cellKey]);
                        break;
                    case "Comment":
                        cellValue = this.convertCommentString(
                            cellInfo[cellKey]
                        );
                        break;
                    case "SourceFlag":
                        if (this.JobType === FSAgentJobTypes.FSRetainSimulate) {
                            cellValue = RMResx.RM_JS_SPS_TabLabel_FS;
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

    getDetailColumns() {
        let needTransformI18nKeyOrToolTipList = new Set(["Comment", "SiteCollectionURL",  "Url", "TitleOrName"
            , "ObjectName", "DestinationUrl", "Location", "ObjectLevel", "Title", "BackupSourceURL", "URL"
            , "Type", "Size", "FinishTime", "Action", "FullPath", "SourceLocation", "DestinationLocation"
            , "JobId", "FileName", "ConnectionName", "RetentionSetting"]);
        let columnHeader = FSJobDetailsColumns[this.JobType];
        let column = [];
        for (let key in columnHeader) {
            if (columnHeader.hasOwnProperty(key)) {
                let columnName = '';
                let isShowTip = false;
                if (needTransformI18nKeyOrToolTipList.has(columnHeader[key])) {
                    switch (columnHeader[key]) {
                        case "ObjectLevel":
                            columnName = RMResx.RM_JS_RC_ReportColumn_ObjectLevel;
                            break;
                        case "DestinationURL":
                        case "DestinationLocation":
                            columnName = RMResx.RM_JS_JMD_Grid_DestinationUrl;
                            break;
                        case "BackupSourceURL":
                            columnName = RMResx.RM_JS_JMD_Grid_BackupSourceURL;
                            break;
                        case "Location":
                            columnName = RMResx.RM_JS_RC_ReportColumn_LocationPath;
                            break;
                        case "FullPath":
                            columnName = RMResx.RM_JS_JMD_Grid_SourceURL;
                            break;
                        case "Url":
                            columnName = RMResx["RM_JS_JMD_Grid_" + columnHeader[key]];
                            break;
                        case "JobId":
                            columnName = RMResx.RM_JS_JM_JobID;
                            break;
                        case "FileName":
                            columnName = RMResx.RM_JS_DC_FileName;
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
                            width: [FSJobDetailsColumnsWidth[this.JobType][key] * 1280],
                            resizeable: true,
                            showTip: isShowTip
                        });
                }
            }
        }
        return column;
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

    jqPageChange(pagerIndex, pagerSize, callback) {
        this.filterData.PageSize = pagerSize;
        this.filterData.CurrentPage = pagerIndex + 1;

        this.setState({ jobsPagerIndex: pagerIndex, jobsPagerSize: pagerSize });
        this.getDetailFromServer(false);
        callback(true);
    }

    handleSelectedIndexChanged(newIndex) {
        if (newIndex === 1) {
            this.getDetailFromServer(true);
        }
        this.setState({ tabIndex: newIndex });
    }

    onSearchStart(args) {
        this.filterData.SearchValue = args;
        this.getDetailFromServer(true);
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
            this.setState({
                filterData: this.filterData
            });
            this.getDetailFromServer(true);
        }
        this.setState({ showFilterPanel: false });
    }

    getSOJobSummaryContent(countObject) {
        let content = <></>;
        if (this.JobType === FSAgentJobTypes.FSArchiverRestore) {
            content = (
                <>
                    <span className="fs-so-summary-count">{countObject.TotleCount} </span>
                    <$g.I18NProvider msg={RMResx.RM_JM_FS_SOSummary_NumberContent}>
                        <span>{countObject.ItemCount}</span>
                    </$g.I18NProvider>
                </>
            );
        }
        return content;
    }

    renderNormalSummary() {
        if (this.state.summaryModel.JobType) {
            const jobType = this.state.summaryModel.JobType;
            const jobSummary = this.state.summaryModel;
            const isNewOpus = LicenseHelper.EnableRecordsArchiver();
            let normalSummary = [
                {
                    name: RMResx.RM_JS_JMD_Summary_JobType,
                    value: FSAgentJobI18N[jobType]
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_JobID,
                    value: jobSummary.JobId
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_StartTime,
                    value: jobSummary.StartTime,
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_EndTime,
                    value: jobSummary.EndTime,
                },
                {
                    name: RMResx.RM_JS_JM_JobRunBy,
                    value: jobSummary.JobRunBy,
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Status,
                    value: FSJobStatusI18N[jobSummary.Status],
                },
                {
                    name: RMResx.RM_JM_JS_Location,
                    value: jobSummary.Scope,
                    hidden: !(isNewOpus && (
                        jobType == FSAgentJobTypes.FSDisposal
                        || jobType == FSAgentJobTypes.FSDisposalSchedule
                        || jobType == FSAgentJobTypes.FSArchiverRestore
                    )) 
                },
                {
                    name: RMResx.RM_JS_JMD_Summary_Comment,
                    value: jobSummary.Comment
                }
            ];
            normalSummary = normalSummary.filter((item) => { return !item.hidden; });
            return (
                <React.Fragment>
                    <JMDetailList
                        textField={"name"}
                        valueField={"value"}
                        title={RMResx.RM_JS_JMD_GeneralSetting}
                        data={normalSummary}
                    />
                </React.Fragment>
            );
        }
    }

    renderSOJobSummary() {
        const soJobSummaryDetails = this.state.soSummaryModel;
        const jobType = this.state.summaryModel.JobType;

        if (
            soJobSummaryDetails
            && soJobSummaryDetails.ActionStatistics
            && jobType === FSAgentJobTypes.FSArchiverRestore
        ) {
            return soJobSummaryDetails.ActionStatistics.map((item, index) => {
                const title = RMResx.RM_JM_SOSummary_ArchivingTitle;
                const soJobSummary = [
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
                        name: RMResx.RM_JM_SOSummary_Column_Total_Archived_Size,
                        value: item?.SizeStr || "",
                    },
                    {
                        name: RMResx.RM_JS_JMD_Summary_Status,
                        value: FSJobStatusI18N[item.Status],
                    }
                ];

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

    renderFilterPanel() {
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
                isShowTabsFilter={false}
                isShowDiscoveryPreScanFilter={false}
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
                placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                disabled={false}
                width="380"
                onSearch={this.onSearchStart}
            />
            <R.Button icon="fia-filter" text={RMResx.RM_Common_Filter} onClick={this.openFilterPanel} />
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

    render() { 
        return (
            <>
                <$g.SiteMap
                    data={[
                        SiteMapLinks.BCM_ContentRepositoryManagement_FS,
                        SiteMapLinks.BCM_FSConnGroup,
                        SiteMapLinks.BCM_FSConnection_JobMonitor,
                        SiteMapLinks.BCM_FSConnection_JobDetails,
                    ]}
                />
                <div id="fsJobDetails">
                    <div id="raJobDetailModule">
                        <R.Tabcontrol
                            active={this.state.tabIndex}
                            onChange={this.handleSelectedIndexChanged}
                        >
                            <R.TabPanel
                                tab={RMResx.RM_JS_JMD_Tab_Summary}>
                                <div id="_summary">
                                    {this.renderNormalSummary()}
                                    {this.renderSOJobSummary()}
                                </div>
                            </R.TabPanel>
                            <R.TabPanel tab={RMResx.RM_JS_JMD_Tab_Details}>
                                <div >
                                    <div className="ra-page-container">
                                        {this.renderJMHeader()}
                                        {this.renderJobDetailTable()}
                                        {this.renderFooter()}
                                    </div>
                                </div>
                            </R.TabPanel>
                        </R.Tabcontrol>
                        {this.renderFilterPanel()}
                    </div>
                </div>
            </>
        )
    }
}

export default FSJobDetails;