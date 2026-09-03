import SiteMapLinks from "../../../Constants/SiteMapLinks";
import HSFilter from "./HSFilter/HSFilter";
import HSActions from "./HSActions";
import Table from "./HSTable/Table";
import PhyObjectManagePermission from '../../PRM/RecordsExplorer/Components/PhyObjectManagePermission';
import { RoleType, TelemetryEventType, TelemetryModule } from '../../../Constants/Constants';
import { PhysicalDefaultColumnIDs, PhysicalDefaultArray, SourceFlags } from "../../../Constants/Constants";
import { BuildColumnIds, ToSearchComponentDispatchType, MsgComponentType, GlobalSearchColumns } from './Constants';
import "../../../Less/PRM/hybridSearch.less";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import RouterUrls from "../../../Constants/RouterUrls";
import { checkPermission } from "../../../Utilities/permissionManager";
import { EnvironmentHelper, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import { SourceFlag } from "../Constants";


const SetPermissionMethod = {
    None: 0,
    SelectedNodes: 1,
    SearchResult: 2
};

export default class HybridSearch extends R.Component {
    idAttr = true;
    componentCreate() {
        this.defautTableColumns = RM.deepcopy(this.initColumns());
        this.state = {
            showBtnGroups: false,
            isShowPermissionPanel: false,
            setMethod: SetPermissionMethod.None,
            showTip: false,
            tipType: "success",
            tipMsg: "",
            isContainsPhySource: false,
            manageColumnsLists: this.defautTableColumns,
            tableColumnsLists: this.defautTableColumns,
            customColums: [],
            pager: {
                pageIndex: 0,
                pageSize: 10,
                shownCount: 10,
                hasNext: false,
            },
            selectItemsCount: 0,
            totalCount: 0,
            isCurrentViewChecked: true,
            isExportToBrowser: true,
            globalSearchExportDto: [],
            isPhysicalEndUser: false,
            isHideReclassifyBtnByApiSetting: false,
        };
        this.hsTableId = "hsTable";
        this.hsActionId = "hsAction";
        this.hsFilterId = "raHSFilter";
        this.isPhysicalEnduser = RM.RoleType == RoleType.StandardUser;
        this.isStandardReviewUser = RM.RoleType == RoleType.StandardReviewUser;
        this.jumpFromphysical = RM.Url.getParam(window.location.href, "source") == -1;
        this.jumpFromDSBParam = RM.Url.getParam(window.location.href, "source");
        this.isShowAll = RM.Url.getParam(window.location.href, "showAll");
        this.isSelectedOneSource = false;
        this.isPhysicalAdmin = RM.gData.isPhysicalAdmin;

        this.tableSelectItems = [];
        this.cachePageBrowserState = [];
        this.currentPage = {
            pageIndex: 0,
            pageSize: 10,
        };
        this.pageSizeOptions = [
            { key: 5, value: 5 },
            { key: 10, value: 10,  checked: true },
            { key: 15, value: 15 },
            { key: 50, value: 50 }
        ];
        this.filterOption = {};
    }

    componentInit() {
        this.setDataByPermission();
        addTelemetryRecord(TelemetryModule.GlobalSearch, TelemetryEventType.ContentPageLoaded);
        this.initIsEndUser();
        this.fetchIsHideReclassifyBtnSetting();
    }

    managePermissionPanelButtons = () => {
        return <>
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Cancel}
                onClick={() => {
                    this.setState({ isShowPermissionPanel: false });
                }}
            />
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Save}
                primary={true}
                classify="theme"
                onClick={this.onSavePermission.bind(this)}
            />
        </>
    }

    setDataByPermission() {
        $$.loading(true);
        let url = `/api/RecordsExplorerApi/GetAvaliableSourceFlagsFromDb`;
        let option = {
            url: url,
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if (EnvironmentHelper.IsGCPEnvironment) {
                res = res.filter(item => item.Value !== SourceFlag.FileSystem && item.Value !== SourceFlag.SharePointOnPrem);
            }

            let avaliableSourceIds = res.map((item)=>{ return item.Value; });
            this.isContainsPhySource = avaliableSourceIds.includes(SourceFlags.Phy); //当前权限是否包含physical
            this.defautTableColumns = this.initColumns();
            this.isOneSourcePermission = res.length == 1;
            this.dispatch(this.hsFilterId, ToSearchComponentDispatchType.SourceType, res);
            this.setColumnsList();
            $$.loading(false);
        });
    }

     initIsEndUser() {
        let option = {
            url: "/api/Dashboard/IsPhysicalEndUser",
            method: "POST"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            this.setState({
                isPhysicalEndUser: res
            });
        });
        $$.loading(false);
    }

    fetchIsHideReclassifyBtnSetting() {
        let option = {
            url: "/api/ManualApproval/IsHideReclassifyBtnInManualApproval",
            method: "GET"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            this.setState({
                isHideReclassifyBtnByApiSetting: !!res
            });
        });
        $$.loading(false);
    }

    showMessageTip(type, msg, msgComponentType) {
        if(msgComponentType == MsgComponentType.MsgBar){
            if (type) {
                let tipOption = { showTip: true, tipType: type, tipMsg: msg };
                this.setState(tipOption);
            } else {
                this.hideMessageTip();
            }
        }else{
            let option = { content: msg, classify: type,};
            $$.toast(option);
        }
    }

    hideMessageTip = () => {
        this.setState({
            showTip: false
        });
    }

    onFilterOrSearch = (data, echoTableColumns, echoSortColumnInfo, isOfflineSearch) => {
        //1.echoTableColumns为undefined代表不需要重置table中的column。
        //2.echoTableColumns为null代表重置table中的column为buildIn column。
        //3.echoTableColumns为其他就按控件返回的数据显示。
        if (echoTableColumns !== undefined) {
            this.manageTableColumns(echoTableColumns);
        }
        if(echoSortColumnInfo){
            this.orderAsc = echoSortColumnInfo.OrderAsc;
            this.orderColumn = echoSortColumnInfo.Column;
        }else{
            this.orderColumn = null;
        }
        this.searchOption = data;
        let needAddTelemetry = echoTableColumns === undefined;
        if(!isOfflineSearch){
            this.loadData(false, needAddTelemetry);
        }
    }

    onOfflineSearch = (profileId, jobId, hasRunningJob) =>{
        this.profileId = profileId;
        this.jobId = jobId;
        this.hasRunningJob = hasRunningJob;
        this.loadOfflineSearchData();
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    initColumns() {
        const showRecordsLabel = !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver();
        let columns = RM.deepcopy(GlobalSearchColumns);

        if (!showRecordsLabel) { 
            columns = columns.filter((column) => column.id !== BuildColumnIds.LockedByRecordLabel);
        }

        if (!this.isContainsPhySource) {
            columns.splice(-2);
        }
        return columns;
    }

    loadData(isNotResetPager, needAddTelemetry) {
        $$.loading(true);
        //isNotResetPager true 不回到第一页；
        if (!isNotResetPager) {
            this.tableSelectItems = [];
        }
        let pagePager = {};
        pagePager.PageSize = this.currentPage.pageSize;
        pagePager.PageIndex = isNotResetPager ? this.currentPage.pageIndex : 0;
        if (pagePager.PageIndex != 0) {
            if (this.currentPage.pageIndex < this.state.pager.pageIndex) {
                pagePager.currentBrowserState = this.cachePageBrowserState[this.currentPage.pageIndex - 1];
            } else {
                pagePager.currentBrowserState = this.state.pager.currentBrowserState;
            }
        } else {
            this.cachePageBrowserState = [];
        }
        let orderColumn = {
            OrderAsc: this.orderAsc,
            Column: this.orderColumn
        };
        let requestParam = {
            PagingInfo: {
                PageIndex: pagePager.currentBrowserState || "",
                PageSize: pagePager.PageSize
            },
            QueryOption: {
                OrderColumn: this.orderColumn ? orderColumn : null,
                Values: this.searchOption
            }
        };
        let url = `/api/PhysicalRecordApi/QueryDataListV3`;
        let option = {
            url: url,
            method: "POST",
            data: requestParam
        };
        if (this.searchOption == null) {
            let tableDefautData = "{\"CanConvert2BasicSearch\":true,\"Datas\":[],\"PagingInfo\":{\"PageIndex\":null,\"PageSize\":10,\"Total\":0,\"HasNextPage\":false}}";
            this.loadDataCallback(tableDefautData, pagePager, isNotResetPager, true);
            this.isSelectedOneSource = false;
            this.isOfflineSearch = false;
        } else {
            fetchUtility(option).then((res) => {
                this.isOfflineSearch = false;
                this.addTelemetryRecordBySearch(needAddTelemetry, res);
                this.loadDataCallback(res, pagePager, isNotResetPager, false);
            }).catch((e) => {
                showToast.error(RMResx.RM_HS_Criteria_View_Msg_ValidOtherError);
            }).finally(() => $$.loading(false));
        }
    }

    loadOfflineSearchData(isNotResetPager, needAddTelemetry){
        $$.loading(true);
        if (!isNotResetPager) {
            this.tableSelectItems = [];
        }
        if(this.jobId){
            let orderColumn = {
                OrderAsc: this.orderAsc,
                Column: this.orderColumn
            };
            let requestParam = {
                PagingInfo: {
                    PageIndex: isNotResetPager ? this.currentPage.pageIndex : 0,
                    PageSize: this.currentPage.pageSize,
                },
                OrderColumn: this.orderColumn ? orderColumn : null,
                ProfileId: this.profileId,
                JobId: this.jobId 
            };
            let url = `/api/RecordsExplorerApi/QueryOfflineSearchData`;
            let option = {
                url: url,
                method: "POST",
                data: requestParam
            };
            fetchUtility(option).then((res) => {
                this.isOfflineSearch = true;
                this.addTelemetryRecordBySearch(needAddTelemetry, res);
                this.loadDataCallback(JSON.stringify(res), null , isNotResetPager, false);
            }).catch((e) => {
                $$.loading(false);
                showToast.error(RMResx.RM_HS_Criteria_View_Msg_ValidOtherError);
            });
        }else{
            this.isOfflineSearch = true;
            let tableDefautData = "{\"CanConvert2BasicSearch\":false,\"Datas\":[],\"PagingInfo\":{\"PageIndex\":0,\"PageSize\":10,\"Total\":0,\"HasNextPage\":false}}";
            this.loadDataCallback(tableDefautData, null, isNotResetPager, false);
        }
    }

    addTelemetryRecordBySearch(needAddTelemetry, res){
        let sendDate = new Date().getTime();
        let receiveDate = new Date().getTime();
        let responseTime = receiveDate - sendDate;
        if (needAddTelemetry) {
            let data = JSON.parse(res);
            let hasNext = data.PagingInfo.HasNextPage;
            addTelemetryRecord(TelemetryModule.GlobalSearch, TelemetryEventType.Search, [hasNext, this.searchOption, responseTime]);
        }
    }

    loadDataCallback(res, pagePager, isNotResetPager, noSearchConditions) {
        $$.loading(false);
        let data = JSON.parse(res);
        this.isSelectedOneSource = data.CanDoGlobalAction;
        this.canDoPhysicalBulkUpdate = data.CanDoPhysicalBulkUpdate;
        let hasNext = data.PagingInfo.HasNextPage;
        let pager = {};
        if(pagePager){
            //normal search
            let currentBrowserState = data.PagingInfo.PageIndex || "";
            if (pagePager.PageIndex >= this.state.pager.pageIndex || pagePager.PageIndex == 0) {
                if (this.cachePageBrowserState.indexOf(currentBrowserState) == -1) {
                    if (currentBrowserState) {
                        this.cachePageBrowserState.push(currentBrowserState);
                    }
                }
            }
            pager = {
                pageIndex: pagePager.PageIndex,
                pageSize: pagePager.PageSize,
                shownCount: data.Datas.length,
                hasNext: hasNext,
                currentBrowserState: currentBrowserState
            };
        }else{
            //offline search
            pager = {
                pageIndex: data.PagingInfo.PageIndex * 1,
                pageSize: data.PagingInfo.PageSize,
                shownCount: data.Datas.length,
                hasNext: data.PagingInfo.Total - data.PagingInfo.PageSize * (data.PagingInfo.PageIndex * 1 + 1) > 0,
            };
        }
        this.cellData = data.Datas;
        this.dispatch(this.hsFilterId, ToSearchComponentDispatchType.DisableBackBaseBtn, !data.CanConvert2BasicSearch);
        this.setState({
            pager: pager,
            totalCount: data.PagingInfo.Total,
            selectItemsCount: this.tableSelectItems.length
        }, () => {
            let otherParams = {
                noSearchConditions: noSearchConditions,
                isOneSourcePermission: this.isOneSourcePermission,
                isHasNotOfflineJob: this.isOfflineSearch && !this.jobId,
                isHasRunningJob: this.hasRunningJob
            };
            this.dispatch(this.hsTableId, data, this.state.tableColumnsLists, !isNotResetPager, otherParams);

            this.dispatch(this.hsActionId, "onPageDataLoaded", data.Datas || [], this.searchOption || [], pager.pageIndex);
        });
    }

    loadTableData(isNotResetPager){
        this.isOfflineSearch ? this.loadOfflineSearchData(isNotResetPager) : this.loadData(isNotResetPager);
    }

    onClickCell = (data) => {
        this.dispatch(this.hsActionId, "showDetails", data);
    }

    onCheckChanged = (selectedItems, isSelectAll) => {
        this.tableSelectItems = selectedItems;
        this.setState({selectItemsCount: isSelectAll ? this.state.totalCount : selectedItems.length });
        this.dispatch(this.hsActionId, "onCheckChanged", isSelectAll, selectedItems, this.searchOption, this.isSelectedOneSource, this.canDoPhysicalBulkUpdate);
    }

    setColumnsList() {
        if (this.isContainsPhySource) {
            let param = {
                url: '/api/TemplateManagementApi/LoadAllColumns',
                method: "post",
                data: {
                    LoadAll: true,
                }
            };
            fetchUtility(param).then((res) => {
                let customColums = RM.deepcopy(res);
                //由于接口的返回值有不能删除的defaut column数据，与defaut column重复，因此删除。
                customColums.splice(customColums.findIndex(item =>
                    item.UniqueId === PhysicalDefaultColumnIDs.NameOrTitle
                ), 1);
                customColums.splice(customColums.findIndex(item =>
                    item.UniqueId === PhysicalDefaultColumnIDs.Classification
                ), 1);
                customColums.splice(customColums.findIndex(item =>
                    item.UniqueId === PhysicalDefaultColumnIDs.LoanedBy
                ), 1);
                let defautColumns = this.defautTableColumns;
                this.setManageColumnsList(customColums, defautColumns);
                this.setTableColumnsList(customColums, defautColumns);
                this.setState({ customColums: customColums });
            });
        } else {
            this.setManageColumnsList([], this.defautTableColumns);
        }
        this.setState({isContainsPhySource: this.isContainsPhySource});
    }

    setManageColumnsList(customColums, defautColumns) {
        //defaut column
        let defautManageColums = RM.deepcopy(defautColumns);
        for (let defautColumn of defautManageColums) {
            defautColumn.name = defautColumn.header;
            defautColumn.title = RMResx.RM_PRM_BarcodeTemp_AreaF_BuildInColumn;
            defautColumn.isChecked = true;
        }
        //custom column
        let customManageColums = RM.deepcopy(customColums);
        //当前用户没有physical的权限，不对customColums进行处理。
        if (customColums && customColums.length > 0) {
            for (let customColumn of customManageColums) {
                let isBuildColumn = PhysicalDefaultArray.includes(customColumn.NameHash);
                customColumn.name = RMResx[customColumn.ColumnName] || customColumn.ColumnName;
                customColumn.isChecked = false;
                if (isBuildColumn) {
                    customColumn.title = RMResx.RM_PRM_BarcodeTemp_AreaF_BuildInColumn;
                } else {
                    customColumn.title = customColumn.Templates.map((item) => { return item.Name; }).toString();
                }
            }
        }
        this.setState({
            manageColumnsLists: [...defautManageColums, ...customManageColums],
        });
    }

    setTableColumnsList(customColums, defautColumns) {
        let customTableColums = RM.deepcopy(customColums);
        let defautTableColums = RM.deepcopy(defautColumns);
        customTableColums = customTableColums.map((item) => {
            return {
                header: RMResx[item.ColumnName] || item.ColumnName,
                width: [200],
                resizeable: true,
                sortable: !!item.AllowSort,
                visible: false,
                NameHash: item.NameHash,
                id: item.NameHash,
                valuePath: {
                    Id: item.UniqueId,
                    Type: item.ColumnType
                }
            };
        });
        this.setState({
            tableColumnsLists: [...defautTableColums, ...customTableColums],
        });
    }

    onSelectManageColumnChanged = (args) => {
        let selectedManageColumnsIds = args.newValue.map((item) => { return item.NameHash; });
        this.manageTableColumns(selectedManageColumnsIds);
    }

    manageTableColumns(selectedColumnsIds) {
        //传入的参数为null时，默认显示build in column；
        let buildColumnIds = [];
        for (let key in RM.deepcopy(BuildColumnIds)) {
            buildColumnIds.push(RM.deepcopy(BuildColumnIds)[key]);
        }
        buildColumnIds.splice(-2);
        let selectedColumnIdsArr = selectedColumnsIds || buildColumnIds;
        for (let item of this.state.tableColumnsLists) {
            if (item.header) {
                item.visible = selectedColumnIdsArr.includes(item.NameHash) || selectedColumnIdsArr.includes(item.OldUniqueId);
            }
        }
        for (let item of this.state.manageColumnsLists) {
            if (item.NameHash) {
                item.isChecked = selectedColumnIdsArr.includes(item.NameHash) || selectedColumnIdsArr.includes(item.OldUniqueId);
            }
        }
        this.setState({
            tableColumnsLists: RM.deepcopy(this.state.tableColumnsLists),
            manageColumnsLists: RM.deepcopy(this.state.manageColumnsLists),
        }, () => {
            this.dispatch(this.hsTableId, null, this.state.tableColumnsLists, null);
            this.dispatch(this.hsFilterId, ToSearchComponentDispatchType.TransSelectedTableIds, selectedColumnIdsArr);
        });
    }

    showManagePermissionPanel = (setMethod) => {
        if (setMethod == SetPermissionMethod.SelectedNodes) {
            if (this.tableSelectItems.length == 0) {
                this.showMessagebox();
                return;
            }
        }
        this.setState({
            isShowPermissionPanel: true,
            setMethod: setMethod
        });
    }

    hideManagePermissionPanel = () => {
        this.setState({
            isShowPermissionPanel: false,
        });
    }

    showMessagebox = () => {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_PRM_GS_Permission_NoSelectNodes,
            buttons: [{ text: RMResx.RM_JS_Common_OK, onClick: this.hideMessageBox }]
        };
        $$.messagedialog(true, this.args);
    }

    managePermission = (returnData) => {
        let reqParam = {
            QueryDto: null,
            NodeIds: null,
            Accounts: returnData.userList,
            UserConflictOption: returnData.selConflictType
        };
        let orderColumn = {
            OrderAsc: this.orderAsc,
            Column: this.orderColumn
        };
        switch (this.state.setMethod) {
            case SetPermissionMethod.SelectedNodes:
                if (this.tableSelectItems && this.tableSelectItems.length > 0) {
                    reqParam.NodeIds = this.tableSelectItems.map(o => { return o.Id; });
                }
                break;
            case SetPermissionMethod.SearchResult:
                if (this.filterOption.SearchOption) {
                    this.filterOption.SearchOption.Key = this.searchKey;
                }
                reqParam.QueryV3Dto = {
                    QueryOption: {
                        OrderColumn: this.orderColumn ? orderColumn : null,
                        Values: this.searchOption
                    }
                };
                break;
            default:
                break;
        }

        let url = `/api/PhysicalRecordApi/RunJobForGlobalSearch`;
        let option = {
            url: url,
            method: "POST",
            data: reqParam
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == "0") {
                if (result.Extension) {
                    if (this.isPhysicalEnduser || this.isStandardReviewUser) {
                        this.showMessageTip("success", RMResx.RM_PRM_PRE_Msg_EndUserSetPermissionSuccessful);
                    } else {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                }
                this.hideManagePermissionPanel();
            }
            else {
                this.showMessageTip("error", result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSavePermission() {
        let callback = (data, success) => {
            this.managePermission(data);
        };
        this.dispatch('raPhyObjectManagePermission', 'onSave', callback);
        return false;
    }

    hideMessageBox = () => {
        $$.messagedialog(false);
    }

    onPagerSizeChanged = (args) => {
        this.currentPage.pageSize = args.newValue.value;
        this.dispatch(this.hsActionId, "SET_LIMIT_COUNT", this.currentPage.pageSize);
        this.loadTableData();
    }

    onPageChange = (index, size, callback) => {
        this.currentPage.pageIndex = index;
        this.currentPage.pageSize = size;
        this.loadTableData(true);
        if (callback) {
            callback(true);
        }
    }

    onSort = (isAsc, sortColumn) => {
        this.orderAsc = isAsc;
        this.orderColumn = sortColumn;
        this.dispatch(this.hsFilterId, ToSearchComponentDispatchType.SortColumn, {
            OrderAsc: isAsc,
            Column: sortColumn
        });
        this.currentPage.pageIndex = 0;
        this.loadTableData();
    }

    onSelectResult = () =>{
        this.setState({selectItemsCount: this.state.totalCount});
    }

    getSelectedItems = () => {
        return this.tableSelectItems;
    }

    routeToHoldManagement = () =>{
        this.routerTo(RouterUrls.BCM_ManageHold);
        // let source = RM.Url.getParam(window.location.href, "source");
        // if (this.isPhysicalAdmin) {
        //     this.routerTo(RouterUrls.PRM_ManageHold + "/?source=" + source, "all");
        // } else {
        //     this.routerTo(RouterUrls.PRM_ManageHold + "/?source=" + source);
        // }
    }

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    renderSiteMap() {
        // let source = RM.Url.getParam(window.location.href, "source");
        // if (source == 4) {
        //     return <$g.SiteMap data={[SiteMapLinks.PRM_Search]}></$g.SiteMap>;
        // } else {
        //     return <$g.SiteMap data={[SiteMapLinks.PRM_Search]}>
        //         {
        //            <div className="flex ra-flex-justify-end">
        //                 <R.Button
        //                     classify="theme"
        //                     title={RMResx.RM_JS_BCM_Explorer_Button_ManageHold}
        //                     text={RMResx.RM_JS_BCM_Explorer_Button_ManageHold}
        //                     onClick={this.routeToHoldManagement} />
        //             </div>
        //         }
        //     </$g.SiteMap>;
        // }
        return <$g.SiteMap data={[SiteMapLinks.PRM_Search]}>
            {
                checkPermission(RouterUrls.BCM_ManageHold, RM.UserResources) && <div className="ra-flex-justify-end">
                    <R.Button
                        primary={true}
                        classify="theme"
                        title={RMResx.RM_JS_BCM_Explorer_Button_ManageHold}
                        text={RMResx.RM_JS_BCM_Explorer_Button_ManageHold}
                        onClick={this.routeToHoldManagement} />
                </div>
            }
        </$g.SiteMap>;
    }

    renderMessagebar() {
        return <div className="margin-bottom-l">
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                status={{ show: this.state.showTip }}
                onClose={this.hideMessageTip}
            />
        </div>;
    }

    renderPermissionBtns() {
        if (this.state.showBtnGroups) {
            return <div id="permissionBtnGroup">
                <R.ButtonGroup
                    icon="fia-manage-access-control"
                    type="bald"
                    height={200}
                    text={RMResx.RM_PRM_GS_Permission_Btn_Default}
                >
                    <R.Button
                        key={1}
                        text={RMResx.RM_PRM_GS_Permission_Btn_SelectedNodes}
                        title={RMResx.RM_PRM_GS_Permission_Btn_SelectedNodes}
                        onClick={this.showManagePermissionPanel.bind(this, SetPermissionMethod.SelectedNodes)} />
                    <R.Button
                        key={2}
                        text={RMResx.RM_PRM_GS_Permission_Btn_SearchResult}
                        title={RMResx.RM_PRM_GS_Permission_Btn_SearchResult}
                        onClick={this.showManagePermissionPanel.bind(this, SetPermissionMethod.SearchResult)} />
                </R.ButtonGroup>
            </div>;
        }
    }

    renderManagePermissionPanel() {
        let panelTitle = this.state.setMethod == SetPermissionMethod.SelectedNodes ? RMResx.RM_PRM_GS_Permission_Btn_SelectedNodes : RMResx.RM_PRM_GS_Permission_Btn_SearchResult;
        return <R.Panel
            id="raPhyManagePermissionPanel"
            header={panelTitle}
            size={600}
            status={{ show: this.state.isShowPermissionPanel }}
            onHide={this.hideManagePermissionPanel}
            destroy={true}
        >
            <div>
                <PhyObjectManagePermission
                    id='raPhyObjectManagePermission'
                    data={[]}
                    globalSearch={true}
                ></PhyObjectManagePermission>
            </div>
            {this.managePermissionPanelButtons()}
        </R.Panel>;
    }

    renderManageColumnBtns() {
        return <div className="hs-manage-column">
            <R.Multicombobox
                checkedField="isChecked"
                disabledField="disabled"
                textField="name"
                valueField="NameHash"
                tooltipField="name"
                items={this.state.manageColumnsLists}
                onChange={this.onSelectManageColumnChanged}
                triggerBySource={true}
                clearable={true}
                customTrigger={true}
            >
                <div>
                    <R.Button icon="fia-manage-column" className="hs-manage-column-btn" text={RMResx.RM_JS_JM_CustomColumns} tooltip={RMResx.RM_JS_JM_CustomColumns} />
                    <R.Button type="bald" icon="fia-triangle-down" className="hs-btn-fia-triangle-down" tooltip={RMResx.RM_JS_JM_CustomColumns} />
                </div>
            </R.Multicombobox>
        </div>;
    }

    getExportInfo = () => {
        return {
            orderColumns: this.orderColumn,
            orderAsc: this.orderAsc,
            totalCount: this.state.totalCount,
            tableColumns: this.state.tableColumnsLists,
        };
    }

    renderTableBar() {
        let selectedItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.selectItemsCount, this.state.totalCount);
        return <div className='ra-main-navbar'>
            <div>
                <HSActions
                    id={this.hsActionId}
                    getSelectedItems={this.getSelectedItems}
                    loadData={this.loadTableData.bind(this)}
                    routerTo={this.routerTo.bind(this)}
                    showMessageTip={this.showMessageTip.bind(this)}
                    getExportInfo={this.getExportInfo}
                    isPhysicalEndUser={this.state.isPhysicalEndUser}
                    isHideReclassifyBtnByApiSetting={this.state.isHideReclassifyBtnByApiSetting}
                />
            </div>
            <div className="ra-main-selected-counter">{selectedItemsCount}</div>
        </div>;
    }

    renderHSTable() {
        return <Table
            id={this.hsTableId}
            columns={this.state.tableColumnsLists}
            isContainsPhySource={this.state.isContainsPhySource}
            cellClick={this.onClickCell}
            customColums={this.state.customColums}
            onCheckChanged={this.onCheckChanged}
            onSort={this.onSort}
        >
            {this.renderHSPager()}
        </Table>;
    }

    renderHSPager() {
        let pager = this.state.pager;
        pager.hasNext = pager.pageSize * (pager.pageIndex * 1 + 1) < this.state.totalCount;
        return <div className="flex ra-flex-align-center">
            <div className="ra-pager-section ra-pager-size">
                <div className="ra-pager-section margin-right-s" tabIndex="0">
                    {RMResx.RM_Common_ShowRows}
                </div>
                <R.Combobox
                    width="60px"
                    height={22}
                    searchable={false}
                    compact
                    textField='value'
                    valueField='value'
                    checkedField='checked'
                    items={this.pageSizeOptions}
                    onChange={this.onPagerSizeChanged}
                />
            </div>
            <div className='inline-block'>
                <$g.SimplePager
                    pagerIndex={pager.pageIndex}
                    pagerSize={pager.pageSize}
                    shownCount={pager.shownCount}
                    hasNext={pager.hasNext}
                    onChange={this.onPageChange}
                ></$g.SimplePager>
            </div>
        </div>;
    }

    renderHSFilter() {
        return <HSFilter
            id={this.hsFilterId}
            isExpireReturnDateSearch={this.jumpFromphysical}
            onSearch={this.onFilterOrSearch}
            onOfflineSearch={this.onOfflineSearch}
            onShowMsg={this.showMessageTip.bind(this)}
            jumpParam={this.jumpFromDSBParam}
            isShowAll={this.isShowAll}
        >
            {this.renderManageColumnBtns()}
        </HSFilter>;
    }

    render() {
        return <div id="raHybridSearch">
            {this.renderSiteMap()}
            {this.renderMessagebar()}
            <div className="ra-page-container">
                {this.renderHSFilter()}
                {this.renderTableBar()}
                {this.renderHSTable()}
                {this.renderManagePermissionPanel()}
            </div>
        </div>;
    }
}