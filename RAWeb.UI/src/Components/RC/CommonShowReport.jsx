import { bindEvents, getMulticomboboxAllItems, getRequestVerificationToken, showToast } from "../../Utilities/CommonUtil";
import "../../Less/RC/commonShowReport.less";
import {
    DueDisposalReportTemplate,
    CreationAndDestructionReportTemplate,
    TermUsageReportTemplate,
    AvailiableSpaceReportTemplate,
    ActionAuditReportTemplate,
    RestoreReportTemplate,
    ArchivedSitesTemplate,
} from "./ShowReportTemplate";
import {JobType } from "./../../Constants/Constants";
import {
    ReportType,
    ObjectLevel,
    Status,
    TermStatus,
    ManualApproval,
    ExportTypeValue,
    ObjectLevelsForSP,
    CreateAndDesObjectLevelsForSP,
    ObjectLevelsForExo,
    ObjectLevelsForPhy,
    CreationObjectLevelForPhysical,
    ObjectLevelsForFS,
    ObjectLevelsForLSP,
    ObjectLevelsForRestoreReport,
    JobTypeMaxRange,
    AuditEventType
} from "./Constants";
import ActionAuditShowReportFilter from "./ActionAuditReport/ShowReportFilter";
import { Messagebox } from "../Common/Messagebox";
import RuleUtil from "../../Utilities/RuleUtil";
import { ApprovalStatus, ApprovalStatusI18Ns, InProgressApprovalStatus} from "../MT/MachineLearningReview/Constants";

export default class CommonShowReport extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        bindEvents(this, "exportReport", "onProfileNameChange", "onReportTimeChange", "onSearchStart",
            "onSearchStop", "onTableSort", "managedColumnChanged", "onPageChange", "onHide",
            "hideMessageTip", "onShowFilter", "onFilter", "onClearFilter", "filterColumnChanged", "actionTypeChanged");

        this.reportType = RM.Url.getParam(window.location.href, "type");
        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.jobId = RM.Url.getParam(window.location.href, "jobid");

        this.showReportApiUrl = this.props.showReportApiUrl;
        this.reportJobType = this.props.reportJobType;
        this.columnsWidth = this.props.columnsWidth;
        this.exportUrl = this.props.exportUrl;
        this.sortColumns = this.props.sortColumns;

        this.isAscending = false;
        this.filterParam = [];
        this.selectedFilterColumnLevels = [];
        this.searchValue = "";
        this.profileName = "";
        this.selectedActionTypeVal = -1;
        this.selectedFilterListObject = [];
        this.selectedFilterObjectString = AuditEventType.All;

        this.state = {
            tipStatus: false,
            tipType: "success",
            tipMsg: "",
            columns: [],
            items: [],
            showFilterPanel: false,
            filterPanelTitle: RMResx.RM_Common_Filter,
            filterData: [],
            hasNoReport: false,
            profileNameItems: [],
            reportTimeItems: [],
            pagerCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            managedColumns: this.getManagedColumns(),
            actionTypes: this.getActionTypes(),
        };
    }

    getManagedColumns() {
        if (this.props.hasOwnProperty('getMultiComboboxData')) {
            return this.props.getMultiComboboxData();
        } else {
            return [];
        }
    }

    componentInit() {
        this.setTableColumn();
        this.setTableBarItems();
        this.getFilterData();
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    getActionTypes() {
        return [
            { name: RMResx.RM_JS_JM_DueTime_All, value: -1, isChecked: true },
            { name: RMResx.RM_JS_JM_DueTime_Create, value: 0, isChecked: false },
            { name: RMResx.RM_JS_JM_DueTime_Destroy, value: 1, isChecked: false }];
    }

    getFilterData() {
        //JobTypeMaxRange每个源的最大范围集合；
        let filterData = [];
        let reportType = parseInt(this.reportType, 10);
        let isCrtAndDesReport = this.reportJobType == ReportType.CreationAndDestructionReport;
        if (reportType < JobTypeMaxRange.SP) {
            filterData = isCrtAndDesReport ? CreateAndDesObjectLevelsForSP : ObjectLevelsForSP;
        } else if (reportType < JobTypeMaxRange.EXO) {
            filterData = ObjectLevelsForExo;
        } else if (reportType < JobTypeMaxRange.PHY) {
            if(isCrtAndDesReport) {
                filterData = CreationObjectLevelForPhysical;
            }
            else {
                filterData = ObjectLevelsForPhy;
            }
        }else if(reportType < JobTypeMaxRange.FS){
            filterData = [ObjectLevelsForFS[0]];
            if (isCrtAndDesReport) {
                filterData = ObjectLevelsForFS;
            }
        } 
        else if (reportType < JobTypeMaxRange.SPOnPrem) {
            filterData = ObjectLevelsForLSP;
        }
        // Support filter for Teams
        else if (reportType > JobTypeMaxRange.Google && reportType < JobTypeMaxRange.Teams) {
            if(this.reportJobType === ReportType.SPOActionAuditReport ) return;
            filterData = isCrtAndDesReport ? [] : ObjectLevelsForSP;
        }
        else {
            if(reportType === JobTypeMaxRange.OneDrive && isCrtAndDesReport)  {
                return;
            }
        }

        if(this.reportJobType === ReportType.RestoreReport){
            filterData = ObjectLevelsForRestoreReport;
        }
        this.setState({ filterData: filterData });
    }

    showFilterBtn() {
        let isContentDueReport = this.reportJobType == ReportType.ItemDueForDisposalReport;
        let isTermUsageReport = this.reportJobType == ReportType.BCSTermUsageReport;
        let isAvailableSpaceReport = this.reportJobType == ReportType.AvailableSpaceReport;
        let isRestoreReport = this.reportJobType == ReportType.RestoreReport;
        let isArchivedSiteReport = this.reportJobType == ReportType.StorageOptimizationReport
        let isShowFilterBtn = true;
        //Available Space Show Report, restore report 没有filter
        if (isAvailableSpaceReport || isRestoreReport || isArchivedSiteReport) {
            isShowFilterBtn = false;
        }
        //Content Due Report 和 Term Usage Report show report filter中只有一个level条件。
        //如果level中只有一个选项，则不显示filter按钮。
        if (isContentDueReport || isTermUsageReport) {
            if (this.state.filterData.length < 2) {
                isShowFilterBtn = false;
            }
        }
        return isShowFilterBtn;
    }

    actionTypeChanged(args) {
        this.selectedActionTypeVal = args.newValue.value;
    }

    hideMessageTip() {
        this.setState({
            tipStatus: false
        });
    }

    setTableBarItems() {
        $$.loading(true);
        let option = {
            url: `/api/RCApi/CommonShowReport?reportType=${this.reportType}&profileId=${this.profileId}&jobId=${this.jobId}`,
            method: "GET",
        };
        fetchUtility(option).then((data) => {
            $$.loading(false);
            this.profileId = data.ProfileId;
            this.hasRanJob = data.HasRanJob;
            let profileInfo = {
                reportTimeItems: data.CollectionTimes
            };
            if(this.state.profileNameItems.length == 0){
                profileInfo.profileNameItems = data.ProfileNames;
            }
            this.setState( profileInfo, () => {
                this.setProfileNameItems();
                this.setReportTimeItems();
                this.setShowReport();
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getSelectedItem(items, selectedId) {
        for (let item of items) {
            if (item.Id == selectedId) {
                item.isChecked = true;
            } else {
                item.isChecked = false;
            }
        }
        return items;
    }

    setProfileNameItems() {
        let profileNameItems = RM.deepcopy(this.state.profileNameItems);
        for (let item of profileNameItems) {
            if (item.Id == this.profileId) {
                item.isChecked = true;
                this.profileName = item.Name;
                this.reportType = item.PType;
                this.getFilterData();
            } else {
                item.isChecked = false;
            }
        }
        this.setState({ profileNameItems: profileNameItems });
    }

    setReportTimeItems() {
        let reportTimeItems = RM.deepcopy(this.state.reportTimeItems);
        let hasNoReport = true;
        if (reportTimeItems.length > 0) {
            if (!this.jobId) {
                this.jobId = reportTimeItems[0].Value;
            }
            for (let item of reportTimeItems) {
                if (item.Value == this.jobId) {
                    item.isChecked = true;
                } else {
                    item.isChecked = false;
                }
            }
            hasNoReport = false;
        }
        this.setState({
            reportTimeItems: reportTimeItems,
            hasNoReport: hasNoReport
        });
    }

    setHasNoJobMsg() {
        if (this.profileName.indexOf(RMResx.RM_JS_RC_ProfileNameDeleted) == -1 && !this.hasRanJob) {
            showToast.warn(RMResx.RM_RC_Msg_HasNoRunJob);
        }
    }

    onProfileNameChange(args) {
        this.profileId = args.newValue.Id;
        this.profileName = args.newValue.Name;
        this.jobId = null;
        this.selectedType = args.newValue.PType;
        if (this.props.hasOwnProperty('getMultiComboboxData')) {
            let managedColumns = this.props.getMultiComboboxData(this.selectedType);
            this.setState({ managedColumns: managedColumns }, () => {
                this.setTableColumn();
            });
        } else {
            let managedColumns = [];
            this.setState({ managedColumns: managedColumns }, () => {
                this.setTableColumn();
            });
        }
        this.setTableBarItems();
    }

    onReportTimeChange(args) {
        this.jobId = args.newValue.Value;
        this.setShowReport(true);
    }

    onSearchStart(args) {
        this.searchValue = args;
        this.setShowReport(true);
    }

    onSearchStop() {
        this.searchValue = "";
        this.setShowReport(true);
    }

    managedColumnChanged(args) {
        this.setState({ managedColumns: getMulticomboboxAllItems(args.newValue, this.state.managedColumns, "id")}, () => {
            this.setTableColumn();
        });
    }

    setTableColumn() {
        let tableColumns = [];
        let managedColumns = this.state.managedColumns;
        let columnsInfo = this.props.getColumnsInfo(this.selectedType);
        for (let key in columnsInfo) {
            if (columnsInfo.hasOwnProperty(key)) {
                let columnObj = {};
                columnObj.header = columnsInfo[key];
                columnObj.width = this.columnsWidth[key];
                columnObj.resizeable = true;
                columnObj.sortable = this.sortColumns.includes(columnsInfo[key]);
                tableColumns.push(columnObj);
            }
        }
        for (let idx in managedColumns) {
            if (managedColumns.hasOwnProperty(idx)) {
                let managedColumnValue = managedColumns[idx].value;
                if (managedColumnValue == RMResx.RM_JS_RC_ReportColumn_TitleOrName
                    || managedColumnValue == RMResx.RM_JS_RC_ReportColumn_BCSTermName) {
                    tableColumns[idx].sortable = true;
                }
                if (managedColumns[idx].isChecked) {
                    tableColumns[managedColumns[idx].id].visible = true;
                } else {
                    tableColumns[managedColumns[idx].id].visible = false;
                }
            }
        }
        this.setState({ columns: tableColumns });
    }

    setShowReport(isResetPagerIndex) {
        $$.loading(true);
        let pagerIndex = 0;
        if (isResetPagerIndex) {
            this.setState({ pagerIndex: 0 });
        } else {
            pagerIndex = parseInt(this.state.pagerIndex, 10);
        }

        if (!this.jobId && this.state.reportTimeItems.length > 0) {
            this.jobId = this.state.reportTimeItems[0].Value;
        }
        let searchKeys = ["BCSTermName", "TitleOrName"];
        if (this.reportJobType == ReportType.AvailableSpaceReport) {
            searchKeys = ["Location"];
        } else if (this.reportJobType == ReportType.ItemDueForDisposalReport) {
            searchKeys.push("SiteCollectionTitle");
        }else if(this.reportJobType == ReportType.RestoreReport){
            searchKeys = ["TitleOrName"];
        }

        let filterDataObj = (this.filterDataForQuery && Object.keys(this.filterDataForQuery)) || [];
        if (filterDataObj.length == 0 || isResetPagerIndex) {
            this.filterDataForQuery = (this.refActionAuditShowReportFilter && this.refActionAuditShowReportFilter.getActionFilterData()) || {};
            filterDataObj = Object.keys(this.filterDataForQuery);
        }
        
        let newUserList = [];
        // If the userList is [], the user type is all
        if (filterDataObj.length > 0) {
            if (this.filterDataForQuery.userList.length == 0) {
                newUserList = this.filterDataForQuery.userList;
            } else {
                let userLists = this.filterDataForQuery.userList;
                for (let index = 0; index < userLists.length; index++) {
                    if (userLists[index].Checked) {
                        newUserList.push(userLists[index].Name);
                    }
                }
            }
            this.selectedFilterListObject = newUserList;
            this.selectedFilterObjectString = this.filterDataForQuery.actionTypes;
        }

        let param = {
            ReportJobType: this.reportJobType,
            SearchValue: this.searchValue,
            SearcheKeys: searchKeys,
            ProfileId: this.profileId,
            JobId: this.jobId,
            SortBy: this.sortBy,
            IsAscending: this.isAscending,
            Operation: this.selectedActionTypeVal,
            PageSize: this.state.pagerSize,
            CurrentPage: pagerIndex + 1,
            FilterLevels: this.filterParam,
            FilterListObject: newUserList,
            FilterObjectString: this.filterDataForQuery.actionTypes,
        };
        let option = {
            url: this.showReportApiUrl,
            type: "POST",
            data: param
        };
        fetchUtility(option).then((res) => {
            if (res) {
                let data = JSON.parse(res);
                let tableItems = this.getTableItems(data.Details);
                this.setState({
                    items: tableItems,
                    pagerCount: data.TotalNumber,
                });
                this.setHasNoJobMsg();
            } else {
                showToast.error(RMResx.RM_RC_Common_Msg_ShowReportError);
            }
            $$.loading(false);
            this.setState({
                showFilterPanel: false,
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getTableItems(data) {
        if (data) {
            for (let item of data) {
                if (this.reportJobType == JobType.SPOActionAuditReport) {

                } else {
                    item.ObjectLevel = ObjectLevel[item.ObjectLevel];
                    if (item.ObjectLevel == RMResx.RM_Common_ObjectLevel_PhysicalBox ||
                        item.ObjectLevel == RMResx.RM_Common_ObjectLevel_PhysicalFile ||
                        item.ObjectLevel == RMResx.RM_PRM_PRE_TableItemType_Record
                    ) {
                        item.DisposalAction = RuleUtil.parseDisposalAction(item.DisposalAction);
                    }
                    else if (
                        item.ObjectLevel == RMResx.RM_JS_Rule_ObjectLevel_FSFile ||
                        item.ObjectLevel == RMResx.RM_JS_Rule_ObjectLevel_FSFolder
                    ) {
                        item.DisposalAction = RuleUtil.parseDisposalActionForFS(item.DisposalAction);
                    }
                    else {
                        item.DisposalAction = RuleUtil.parseDisposalActionForSP(item.DisposalAction);
                    }
                    item.ManualApproval = ManualApproval[item.ManualApproval];
                    item.ExportType = this.getExportTypeValue(item.ExportType);
                    item.Status = Status[item.Status];
                    item.TermStatus = TermStatus[item.TermStatus];
                    item.EncodeUrl = this.encodeSpecialCharacters(item.Url);
                    item.isSharePoint = item.EncodeUrl && item.EncodeUrl.startsWith("http") ? true : false;

                    if(InProgressApprovalStatus[item.InternalApprovedStatus])
                    {
                        item.ApprovalStatus = "";
                    }
                    else
                    {
                        let approvalStatus = ApprovalStatusI18Ns.get(item.InternalApprovedStatus);
                        if(item.InternalApprovedStatus === ApprovalStatus.WorkflowComplete){
                            approvalStatus +=` (${ApprovalStatusI18Ns.get(item.ApprovalStatus)})`;
                        }
                        item.ApprovalStatus = approvalStatus;
                    }
                }
            }
        } else {
            data = [];
        }
        return data;
    }

    onTableSort(args) {
        if (args.column.header == RMResx.RM_JS_RC_ReportColumn_TitleOrName) {
            this.sortBy = "TitleOrName";
        }
        if (args.column.header == RMResx.RM_JS_RC_ReportColumn_BCSTermName) {
            this.sortBy = "BCSTermName";
        }
        if (args.column.header == RMResx.RM_JS_RC_ReportColumn_Size){
            this.sortBy = "Size";
        }
        if (args.column.header == RMResx.RM_JS_RC_ReportColumn_StartTime){
            this.sortBy = "StartTime";
        }
        if (args.column.header == RMResx.RM_JS_RC_ReportColumn_EndTime){
            this.sortBy = "FinishTime";
        }
        this.isAscending = args.status == "asc" ? true : false;
        this.setShowReport(true);
    }

    getExportTypeValue(exportType) {
        let result = ExportTypeValue[exportType];
        if (!result) {
            return RMResx.RM_JS_RDM_CreateRule_ExportType_None;
        }
        return result;
    }

    encodeSpecialCharacters(needEncodeStr) {
        let mapList = [{ k: "%", v: "%25" }, { k: "#", v: "%23" }];
        if (needEncodeStr && needEncodeStr.startsWith("http")) {
            mapList.forEach(function (item) {
                needEncodeStr = needEncodeStr.replaceAll(item.k, item.v);
            });
        }
        return needEncodeStr;
    }

    onPageChange(index, size, callback) {
        this.setState({
            pagerIndex: index,
            pagerSize: size
        }, () => {
            this.setShowReport();
        });
        if (callback) {
            callback(true);
        }
    }

    onShowFilter() {
        this.setState({ showFilterPanel: true, });
    }

    setActionTypes() {
        let echoActionTypes = RM.deepcopy(this.state.actionTypes);
        for (let item of echoActionTypes) {
            if (item.value == this.selectedActionTypeVal) {
                item.isChecked = true;
            } else {
                item.isChecked = false;
            }
        }
        this.setState({ actionTypes: echoActionTypes });
    }

    onFilter() {
        let echoFilterData = RM.deepcopy(this.state.filterData);
        this.filterParam = RM.deepcopy(this.selectedFilterColumnLevels);
        for (let key in echoFilterData) {
            if (this.selectedFilterColumnLevels.length == 0) {
                echoFilterData[key].isChecked = true;
            } else {
                if (this.selectedFilterColumnLevels.indexOf(echoFilterData[key].level) != -1) {
                    echoFilterData[key].isChecked = true;
                } else {
                    echoFilterData[key].isChecked = false;
                }
            }
        }
        if (this.reportJobType == ReportType.CreationAndDestructionReport) {
            this.setActionTypes();
        }
        this.setState({
            showFilterPanel: true,
            filterData: echoFilterData
        });
        this.setShowReport(true);
    }

    onClearFilter() {
        let echoFilterData = RM.deepcopy(this.state.filterData);
        for (let item of echoFilterData) {
            item.isChecked = true;
        }
        this.selectedFilterColumnLevels = [];
        this.filterParam = [];
        this.setState({
            // showFilterPanel: false,
            filterData: echoFilterData
        });
        if (this.reportJobType == ReportType.CreationAndDestructionReport) {
            this.selectedActionTypeVal = -1;
            this.setActionTypes();
        }
        if (this.reportJobType == ReportType.SPOActionAuditReport) {
            this.refActionAuditShowReportFilter.setActionItemsClear();
            this.filterDataForQuery = {};
        }

        // this.setShowReport(true);
    }

    onHide() {
        this.setState({ showFilterPanel: false });
    }

    filterColumnChanged(args) {
        this.selectedFilterColumnLevels = [];
        for (let item of args.newValue) {
            this.selectedFilterColumnLevels.push(parseInt(item.level, 10));
        }
        if (args.isSelectAll) {
            this.selectedFilterColumnLevels = [];
        }
    }

    onExportReportBtn = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.exportReport });
    }

    exportReport = async () => {
        const data = {
            ReportJobType : this.reportJobType,
            ReportJobId : this.jobId,
            ProfileName : this.profileName,
            ProfileId : this.profileId,
        };
        const requestOption = {
            url: "/api/RCApi/RunCommonExportReportJob",
            data: data,
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType !== 0){
            showToast.error(result.ErrorMessage);
            return;
        }
        if(RM.RoleType === 0){
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_EndUser_JobStart}>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>);
            return;
        }
        showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
        </$g.I18NProvider>);
    }

    renderTableBar() {
        let isShowFilterBtn = this.showFilterBtn();
        return <div className='ra-tableBar'>
            <div className='ra-tableBar-content-left'>
                <div className='ra-tableBar-searchbox'>
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                        disabled={false}
                        onSearch={(args) => (args || "").trim() === "" ? this.onSearchStop() : this.onSearchStart(args)}
                    />
                </div>
            </div>
            {
                <div className='ra-tableBar-content-right'>
                    {
                        this.state.managedColumns.length != 0 && <div className='pull-left'>
                            <R.Multicombobox
                                height={34}
                                checkedField="isChecked"
                                textField="value"
                                valueField="id"
                                hasFilter={false}
                                required={true}
                                hasSelectAll={false}
                                items={this.state.managedColumns}
                                noneText="Manage Columns"
                                onChange={this.managedColumnChanged}
                                triggerBySource={true}
                            />
                        </div>
                    }
                    {isShowFilterBtn &&
                        <div className='pull-left'>
                            <div className='ra-spliter'></div>
                            <div className='pull-left'>
                                <R.Button
                                    type="icon"
                                    tooltip={RMResx.RM_PRM_PRE_Filter}
                                    icon="fia-filter"
                                    onClick={this.onShowFilter} />
                            </div>
                        </div>
                    }
                </div>
            }
        </div>;
    }

    renderTable() {
        let tableTemplate = "";
        switch (this.reportJobType) {
            case ReportType.ItemDueForDisposalReport:
                tableTemplate = DueDisposalReportTemplate;
                break;
            case ReportType.BCSTermUsageReport:
                tableTemplate = TermUsageReportTemplate;
                break;
            case ReportType.CreationAndDestructionReport:
                tableTemplate = CreationAndDestructionReportTemplate;
                break;
            case ReportType.AvailableSpaceReport:
                tableTemplate = AvailiableSpaceReportTemplate;
                break;
            case ReportType.SPOActionAuditReport:
                tableTemplate = ActionAuditReportTemplate;
                break;
            case ReportType.RestoreReport:
                tableTemplate = RestoreReportTemplate;
                break;
            case ReportType.StorageOptimizationReport:
                tableTemplate = ArchivedSitesTemplate;
                break;
        }
        return <R.Table
            id="reco-report-show-table"
            disabled={false}
            columns={this.state.columns}
            rowTemplate={tableTemplate}
            items={this.state.items}
            doSort={this.onTableSort}
        />;
    }

    renderPager() {
        let pagerTotalCount = RMResx.RM_Common_TotalCount.format(this.state.pagerCount);
        return <div className="ra-table-pager">
            <div>{pagerTotalCount}</div>
            <$g.Pager
                itemsCount={this.state.pagerCount}
                pagerIndex={this.state.pagerIndex}
                pagerSize={this.state.pagerSize}
                showPagerSize={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPageChange} />
        </div>;
    }

    renderShowReport() {
        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle">
                <span tabIndex='0'>{RMResx.RM_JS_Common_ShowReport}</span>
            </div>
            <div>
                {this.renderTableBar()}
            </div>
            <div className="ra-form-content">
                {this.renderTable()}
                {this.renderPager()}
            </div>
        </div>;
    }

    renderFilterPanel() {
        return <R.Panel
            id="filterPanel"
            header={this.state.filterPanelTitle}
            size={400}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
            onHide={this.onHide}
        >
            <div>
                <div className="ra-flex-justify-end">
                    <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClearFilter} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
                </div>
                {
                    this.state.filterData.length > 1 &&
                    <$g.FormRow label={RMResx.RM_JS_RC_ReportColumn_ObjectLevel}>
                        <R.Multicombobox
                            height={34}
                            width={"100%"}
                            checkedField="isChecked"
                            textField="value"
                            valueField="level"
                            hasFilter={true}
                            searchable={false}
                            required={true}
                            items={this.state.filterData}
                            noneText="Manage Columns"
                            onChange={this.filterColumnChanged}
                            triggerBySource={true}
                        />
                    </$g.FormRow>
                }

                {this.reportJobType == ReportType.CreationAndDestructionReport &&
                    <$g.FormRow label={RMResx.RM_JS_RC_TimeFrame_Operation}>
                        <R.Combobox
                            searchable={false}
                            textField='name'
                            valueField='value'
                            width={"100%"}
                            height={36}
                            checkedField='isChecked'
                            items={this.state.actionTypes}
                            onChange={this.actionTypeChanged}
                        />
                    </$g.FormRow>
                }

                {this.reportJobType == ReportType.SPOActionAuditReport &&
                    <ActionAuditShowReportFilter
                        ref={r => this.refActionAuditShowReportFilter = r}
                        reportJobType={this.reportJobType}
                        jobId={this.jobId}
                        selectedUsers={this.selectedFilterListObject}
                        selectedActionTypes={this.selectedFilterObjectString}
                    />
                }
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showFilterPanel: false });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    render() {
        return (
            <div id="raRCShowReport" className="reco-report-show-wrapper">
                <section className="reco-report-show-button">
                    <R.Button primary={true} classify="theme" text={RMResx.RM_RC_Audit_AuditExp} disabled={this.state.hasNoReport}
                        onClick={this.onExportReportBtn} />
                </section>
                <section className="reco-report-show-options">
                    <div className="reco-report-show-option">
                        <div className="reco-report-show-option-title" tabIndex="0">
                            {RMResx.RM_JM_ProfileName}
                        </div>
                        <R.Combobox
                            id="raRcProfileName"
                            textField='Name'
                            valueField='Id'
                            checkedField='isChecked'
                            items={this.state.profileNameItems}
                            width='300'
                            height={34}
                            onChange={this.onProfileNameChange}
                        />
                    </div>
                    <div className="reco-report-show-option">
                        <div className="reco-report-show-option-title" tabIndex="0">
                            {RMResx.RM_JS_Common_CollectionTime}
                        </div>
                        <R.Combobox
                            id="raRcCollectionTime"
                            textField='Key'
                            valueField='Value'
                            checkedField='isChecked'
                            disabled={this.state.hasNoReport}
                            items={this.state.reportTimeItems}
                            width='300'
                            height={34}
                            onChange={this.onReportTimeChange}
                        />
                    </div>
                </section>
                <section className="reco-report-show-table">
                    <div className="reco-report-show-filter-bar">
                        <div className="reco-report-show-search">
                            <R.Searchbox
                                placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                                disabled={false}
                                onSearch={(args) => (args || "").trim() === "" ? this.onSearchStop() : this.onSearchStart(args)}
                                width={380}
                            />
                        </div>
                        <div className="reco-report-show-filters">
                            {
                                this.showFilterBtn() &&
                                <div>
                                    <R.Button
                                        type="button"
                                        primary={false}
                                        classify="default"
                                        tooltip={RMResx.RM_PRM_PRE_Filter}
                                        icon="fia-filter"
                                        text={RMResx.RM_RC_Btn_Filter}
                                        onClick={this.onShowFilter} />
                                </div>
                            }
                            {
                                this.state.managedColumns.length !== 0 &&
                                <div>
                                    <R.Multicombobox
                                        height={34}
                                        checkedField="isChecked"
                                        textField="value"
                                        valueField="id"
                                        hasFilter={false}
                                        required={true}
                                        hasSelectAll={true}
                                        items={this.state.managedColumns}
                                        noneText="Manage Columns"
                                        onChange={this.managedColumnChanged}
                                        triggerBySource={true}
                                        customTrigger={true}
                                    >
                                        <R.Button
                                            text={RMResx.RM_RC_Btn_ManageColumn}
                                            icon="fia-manage-column"
                                            primary={false}
                                            classify="default"
                                        >
                                        </R.Button>
                                    </R.Multicombobox>
                                </div>}
                        </div>
                    </div>
                    <div className="reco-report-show-data-list">
                        {this.renderTable()}
                    </div>
                    <div className="reco-report-show-footer-section">
                        <$g.Pager
                            itemsCount={this.state.pagerCount}
                            pagerIndex={this.state.pagerIndex}
                            pagerSize={this.state.pagerSize}
                            showPagerSize={true}
                            showPagerCounter={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.onPageChange} />
                    </div>
                </section>
                {this.renderFilterPanel()}
                <div id='downloadDiv' style={{ display: "none" }} />
            </div>
        );
    }
}
