import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RequestTable from "./Components/Table";
import RequestDetailForm from "./Components/RequestDetailForm";
import PhyObjectForm from '../Common/PhyObjectForm';
import PhyObjectDetail from '../Common/PhyObjectDetail';
import RequestAction from "./Components/RequestAction";
import '../../../Less/PRM/Request/MyRequest.less';
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import { ActionTypeMode, RequestStatusMode, RequestTypeMode, PhysicalRequestFilterColumn,PhysicalRequestType,PhysicalRequestStatus } from "../Constants";
import { PhysicalDefaultColumnIDs } from "../../../Constants/Constants";
import { NodeType } from "../../../Constants/DAEnums";
import { MyRequestFilterForm } from "../../PRM/Request/Components/MyRequestFilterForm";
import LoanedRecordsTableTemplate from './Components/LoanedRecords/RowTemplate'

const PageI18N = {
    OperateLimit: RMResx.RM_PRM_Request_Operate_Limit
};

const ColumnHeadNames = {
    RequestID: RMResx.RM_PRM_MyRequest_RequestId,
    ItemName: RMResx.RM_PRM_MyRequest_ItemName,
    UniqueId: RMResx.RM_PRM_RequestManagement_UniqueId,
    RequestType: RMResx.RM_PRM_MyRequest_RequestType,
    Status: RMResx.RM_PRM_RequestManagement_Status,
    RequestBy: RMResx.RM_PRM_MyRequest_RequestBy,
    CreatedTime: RMResx.RM_PRM_PRE_Column_CreatedTime,
};

const ApprovalPanelType = {
    ApprovalPanel: 1,
    ReviewApprovalPanel: 2
};

const SkipShowLoanedDataConfirm = {
    Yes: true,
    No: false
};

const ReportObjectLevel = {
    PhyBox: 9300,
    PhyFolder: 9400,
};

const RequestType = {
    LoanRequest: 0,
    CreationRequest: 1,
    MovementRequest: 2,
};

const RealTimeJobStatus = {
    Running: 3
};

const RealTimeJobMessageType = {
    FailedWithEx: -1,
    Successful: 0,
    Failed: 1,
    Exception: 2,
    Skipped: 3,
    Confirmation: 4
};

Date.prototype.deepCopy = function () {
    return new Date(this.valueOf());
};
export default class MyRequest extends R.Component {
    idAttr = true;
    componentCreate() {
        bindEvents(
            this,
            "onCheckChanged",
            "setBtnState",
            "renderActionBar",
            "cellClick",
            "cellOperate",
            "reviewApproval",
            "onShowFilter",
            "onHideFilterPanel",
            "onShowApprovalActionPanel",
            "onShowRejectActionPanel",
            "onApproval",
            "onReject",
            "onCloseRequestDetail",
            "reviewReject",
            "onShowFileDetail",
            "onCloseFileDetail",
            "onEditRequestFile",
            "onSaveRequestFile",
            "onApprovalResend",
            "onCancelResendApproval",
            "onFilter",
            "handleChangeRequestBy",
            "handleChangeRequestType",
            "setPanelHeader",
            "onShowCancelActionMessageBox"
        );

        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            pagerIndex: 0,
            pagerSize: 10,
            pagerTotal: 0,
            shownCount: 0,
            batchActionDisable: true,
            formTemplateName: "",
            showRequestDetail: { show: false },
            actionPanelStatus: { show: false },
            showFileDetail: { show: false },
            showEditFilePanel: { show: false },
            actionPanelTitle: "",
            fileDetailParam: {
                status: RequestStatusMode.WaitingForAproval,
                isRequest: false,
                requestId: null,
                id: null,
                nodeType: NodeType.PhyFile // file
            },
            requestActionParam:{},
            requestDetailParam: {},
            timeRange: {
                start: null,
                end: null,
            },
            startTime:new Date("2000/1/1"),
            endTime:new Date("2999/1/1"),
            requestTypeItems:[],
            requestByItems: [],
            createdTimeItems:[],
            approvalStatusItems:[],
            filterOption: [],
            filterOptionsInfo:this.getJumpFromDSBFilterOptions(),
            filters:this.getJumpFromDSBFilters(),
            showFilterPanel: false,
            requestId: 0,
            showApprovalLoanedDialog: false,
            loanedDataList: [],
            allPhysicalFileInfos: [],
            activeTab: 0,
            approvalPanelType : ApprovalPanelType.ApprovalPanel,
        };
        this.actionType = ActionTypeMode.None;
        this.selectdItems = [];
        this.cacheItems = [];
        this.operateLimitCount = 15;
        this.isAdmin = RM.gData.isPhysicalAdmin;
        this.requestDetailEditable = false;
        this.tableColumns = this.getColumnInfo();
        this.searchKey = "";
        this.requestDetailPanelId = "raRequestDetail";
        this.fileEditFormId = "fileEditFormId";
        this.actoinPanelId = "raRequestActionPanel";
        this.jumpId = RM.Url.getParam(window.location.href, "source");
    }
    
    componentInit() {
        this.initFilterRequsetBy();
        this.initFilterRequestType();
        this.iniFilterApprovalStatus();
        this.loadData(this.state.pagerIndex, this.state.pagerSize);
    }

    addCacheItem(item){
        let isExits = this.cacheItems.find( r=> r.RequestId == item.RequestId);
        if(item.RequestId != ''){
            if(isExits == undefined){
                item.isChecked = false;
                this.cacheItems.push(item);
            }else{
                this.updateItemCheckedStatus(item);
            }
        }
    }

    clearCacheItems(){
        this.cacheItems = [];
    }

    updateItemCheckedStatus(item){
        let cacheItem = this.cacheItems.find( r=> r.RequestId == item.RequestId);
        if(cacheItem !== undefined){
            item.isChecked = cacheItem.isChecked;
        }
    }

    updateCacheItemsStatus(rowItems){
        if(rowItems && rowItems.length > 0){
            this.cacheItems.forEach((item, key)=>{
                let rowItem = rowItems.find(t => t.RequestId == item.RequestId);
                if(rowItem !== undefined){
                    item.isChecked = rowItem.isChecked;   
                }
            });
        }
    }

    getJumpFromDSBFilters(){
        let jumpId = RM.Url.getParam(window.location.href, "source");
        let status = RM.Url.getParam(window.location.href, "status");
        let value = RM.Url.getParam(window.location.href, "value");
        let type =  RM.Url.getParam(window.location.href, "type");
        let filter = [];
        if(status !== "" && value !== "" && type !== ""){
            filter = [{Column:parseInt(type),ColumnValues:[parseInt(value)]},{Column:2,ColumnValues:[parseInt(status)]}];
        }
        switch(jumpId){
            case "0":
                filter = [{Column:1,ColumnValues:[0]},{Column:2,ColumnValues:[0,1,2]}];
                break;
            case "1":
                filter = [{Column:1,ColumnValues:[1]},{Column:2,ColumnValues:[0,1,2]}];
                break;
            case "2":
                filter = [{Column:2,ColumnValues:[0]}];
                break;
            case "3":
                filter = [{Column:2,ColumnValues:[1]}];
                break;
            case "4":
                filter = [{Column:2,ColumnValues:[2]}];
                break;
            case "5":
                filter = [{Column:1,ColumnValues:[0]},{Column:2,ColumnValues:[0]}];
                break;
            case "6":
                filter = [{ Column: 2, ColumnValues: [0, 1, 2] }];
                break;
            case "7":
                filter = [{Column:1,ColumnValues:[2]},{Column:2,ColumnValues:[0,1,2]}];
                break;
        }
        return filter;
    }
    
    getJumpFromDSBFilterOptions(){
        let jumpId = RM.Url.getParam(window.location.href, "source");
        let status = RM.Url.getParam(window.location.href, "status");
        let value = RM.Url.getParam(window.location.href, "value");
        let type =  RM.Url.getParam(window.location.href, "type");
        let filterOptions = {};
        
        if(jumpId && (jumpId == 6)){
            let Status = [
                {id:1,name:"Approved",checked:true,tooltip:"",disabled:false},
                {id:2,name:"Rejected",checked:true,tooltip:"",disabled:false},
                {id:3,name:"Canceled",checked:false,tooltip:"",disabled:false},
                {id:0,name:"Waiting for Approval",checked:true,tooltip:"",disabled:false}
            ];  
            filterOptions.Status = Status;
            return filterOptions;
        }

        if(jumpId && (jumpId == 0 || jumpId == 1 || jumpId == 7)){
            let Type = [
                {id:1,checked: 1 == jumpId,disabled:false,name: RMResx.RM_PRM_RequestType_Creation,tooltip:""},
                {id:0,checked: 0 == jumpId,disabled:false,name: RMResx.RM_PRM_RequestType_Loan,tooltip:""},
                {id:2,checked: 7 == jumpId,disabled:false,name: RMResx.RM_PRM_PRE_MovementRequest,tooltip:""}
            ];
            let Status = [
                {id:1,name:"Approved",checked:true,tooltip:"",disabled:false},
                {id:2,name:"Rejected",checked:true,tooltip:"",disabled:false},
                {id:3,name:"Canceled",checked:false,tooltip:"",disabled:false},
                {id:0,name:"Waiting for Approval",checked:true,tooltip:"",disabled:false}
            ];
            filterOptions.Type = Type;   
            filterOptions.Status = Status;
        }else if(jumpId && (jumpId == 2 || jumpId == 3 || jumpId ==4)){
            let Status = [
                {id:1,name:"Approved",checked:3 == jumpId,tooltip:"",disabled:false},
                {id:2,name:"Rejected",checked:4 == jumpId,tooltip:"",disabled:false},
                {id:3,name:"Canceled",checked:false,tooltip:"",disabled:false},
                {id:0,name:"Waiting for Approval",checked:2 == jumpId,tooltip:"",disabled:false}
            ];
            filterOptions.Status = Status;
        }else if(jumpId && jumpId == 5){
            let Type = [
                {id:1,checked: false,disabled:false,name: RMResx.RM_PRM_RequestType_Creation,tooltip:""},
                {id:0,checked: true,disabled:false,name: RMResx.RM_PRM_RequestType_Loan,tooltip:""}
            ];
            let Status = [
                {id:1,name:"Approved",checked:true,tooltip:"",disabled:false},
                {id:2,name:"Rejected",checked:false,tooltip:"",disabled:false},
                {id:3,name:"Canceled",checked:false,tooltip:"",disabled:false},
                {id:0,name:"Waiting for Approval",checked:false,tooltip:"",disabled:false}
            ];
            filterOptions.Type = Type; 
            filterOptions.Status = Status;
        }else if(status && type){
            let Type = [
                {id:1,checked: value == 1,disabled:false,name: RMResx.RM_PRM_RequestType_Creation,tooltip:""},
                {id:0,checked: value == 0,disabled:false,name: RMResx.RM_PRM_RequestType_Loan,tooltip:""},
                {id:2,checked: value == 2,disabled:false,name: RMResx.RM_PRM_PRE_MovementRequest,tooltip:""}
            ];
            let Status = [
                {id:1,name:"Approved",checked:status == 1,tooltip:"",disabled:false},
                {id:2,name:"Rejected",checked:status == 2,tooltip:"",disabled:false},
                {id:3,name:"Canceled",checked:false, tooltip:"",disabled:false},
                {id:0,name:"Waiting for Approval",checked:status == 0,tooltip:"",disabled:false}
            ];
            filterOptions.Type = Type; 
            filterOptions.Status = Status;
        }
        return filterOptions; 
    }

    getDefaultStartTime() {
        let status = RM.Url.getParam(window.location.href, "status");
        let value = RM.Url.getParam(window.location.href, "value");
        let type =  RM.Url.getParam(window.location.href, "type");
        let nonStartTime = false;
        if(status !="" || value != "" || type !="" || this.jumpId !=""){
            nonStartTime = true;
        }
        let startTime = new Date();
        startTime.setMonth(new Date().getMonth() - 1);
        return nonStartTime?new Date("2000/1/1"):startTime;
    }

    getSelectedItems(){
        return this.cacheItems.filter(t=> t.isChecked);
    }

    loadData = (pagerIndex, pagerSize, callback) => {
        $$.loading(true);
        let requestParam = {
            PageIndex: pagerIndex + 1,
            PageSize: pagerSize,
            SearchText: this.searchKey,
            Filters: this.state.filters,
            StartTime:RM.TimeUtil.getCommonDateStr(this.state.startTime),
            EndTime:RM.TimeUtil.getCommonDateStr(this.state.endTime),
        };
        let urlData = "/api/PhysicalRequestApi/Query";
        let option = {
            url: urlData,
            method: "POST",
            data: requestParam
        };
        fetchUtility(option).then((res) => {
            let data = res; //JSON.parse(res);
            $$.loading(false);
            if (data.HasError == false) {
                data.RequestList.map(d =>{
                    this.addCacheItem(d);
                });

                this.dispatch("requestTable", data.RequestList, this.tableColumns, this.isAdmin);
                this.setState({
                    pagerIndex: pagerIndex,
                    pagerSize: pagerSize,
                    pagerTotal: data.TotalCount,
                    shownCount: data.RequestList.length,
                    batchActionDisable: true,
                });
                this.setBtnState(this.cacheItems.filter(t=> t.isChecked));

                if(callback) { callback(true); }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    };

    initFilterRequsetBy() {
        $$.loading(true);
        let option = {
            url: "/api/PhysicalRequestApi/GetFilterInfo",
            method:"POST" 
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let data = JSON.parse(res);
            let AllUsers = data[PhysicalRequestFilterColumn.RequestBy];
            let RequestByItems = [];
            
            for (let key in AllUsers){
                if(AllUsers.hasOwnProperty(key)){
                    let item = {};
                    item.id = AllUsers[key].UserId;
                    item.name = AllUsers[key].DisplayName;
                    item.tooltip = AllUsers[key].UserPrincipalName;
                    item.checked = true;
                    RequestByItems.push(item);             
                }
            }
            this.setState({
                requestByItems:RequestByItems.sort(this.sortFun('name'))
            });
        });
    }

    initFilterRequestType(){
        let requestTypeItems = [];
        let data = PhysicalRequestType;
        for(let key in data){
            if (data.hasOwnProperty(key)) {
                let item = {};
                item.id = Number(key);
                item.name = data[key].name;
                item.checked = true;
                requestTypeItems.push(item); 
            }
        }
        requestTypeItems = requestTypeItems.sort(this.sortFun('name'));
        this.setState({requestTypeItems: requestTypeItems});
    }

    iniFilterApprovalStatus(){
        let approvalStatusItems = [];
        let data = PhysicalRequestStatus;
        for(let key in data){
            if (data.hasOwnProperty(key)) {
                let item = {};
                item.id = Number(key);
                item.name = data[key].name;
                item.checked = true;
                approvalStatusItems.push(item);
            }
        }
        approvalStatusItems = approvalStatusItems.sort(this.sortFun('name'));
        this.setState({approvalStatusItems:approvalStatusItems});
    }

    sortFun(property) {
        return function (item1, item2) {
            var value1 = item1[property];
            var value2 = item2[property];
            if (value1 < value2) {
                return -1;
            } else if (value1 > value2) {
                return 1;
            } else {
                return 0;
            }
        };
    }

    onFilter() {
        let callback = (filterOptionsInfo,timeRange) => {
            this.state.filters = [];
            let startTime = new Date("2000/1/1");
            let endTime = new Date("2999/1/1");
            if (!Object.values(timeRange).every((value) => !value)) { // Mean that start and end must be not null
                startTime = timeRange.start;
                endTime = timeRange.end;
            }
            for (let key in filterOptionsInfo) {
                let filterParam = { Column: PhysicalRequestFilterColumn[key], ColumnValues: [] };          
                let filterOptions = filterOptionsInfo[key];
                let filterOptionValues = filterOptions.filter((item) => { return item.checked; }).map((option) => { return option.id; });
                for (let value of filterOptionValues) {                    
                    filterParam.ColumnValues.push(value);
                }
                if (filterOptionValues.length != filterOptions.length && filterOptionValues.length != 0) {
                    this.state.filters.push(filterParam);
                }
            }
            this.state.timeRange.start = timeRange.start;
            this.state.timeRange.end = timeRange.end;
            this.setState({ filterOptionsInfo: filterOptionsInfo, timeRange: timeRange, startTime, endTime }, () => {
                this.loadData(0 , this.state.pagerSize);
            });
            this.clearCacheItems();
        };
        this.dispatch("mrFilterForm", callback);
        this.setState({ showFilterPanel: false });
    }

    getColumnInfo() {
        let commonColumn = [{
            header: ColumnHeadNames.RequestID,
            width: 150,
            resizeable: true,
        }, {
            header: ColumnHeadNames.ItemName,
            width: 300,
            resizeable: true,
            visible: true,
        }, {
            header: ColumnHeadNames.UniqueId,
            resizeable: true,
            width: [250,300]
        }, {
            header: ColumnHeadNames.RequestType,
            resizeable: true,
            width: 200
        },
        {
            header:ColumnHeadNames.CreatedTime,
            resizeable:true,
            width: 300
        },
        {
            header: ColumnHeadNames.Status,
            resizeable: true,
            width: 200
        }];
        return this.isAdmin ? [...commonColumn, ...this.getEnduserColumn()] : commonColumn;
    }

    getEnduserColumn() {
        return [{
            header: ColumnHeadNames.RequestBy,
            resizeable: true,
            width: 260,
        }];
    }

    getFileDetailButtons() {
        let showEdit = this.isAdmin && this.state.fileDetailParam.isRequest && this.state.fileDetailParam.status == RequestStatusMode.WaitingForAproval;
        if (showEdit) {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={this.onCloseFileDetail}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Edit}
                        primary={true}
                        classify="theme"
                        onClick={this.onEditRequestFile}
                    />
                </>
            )
        } else {
            return (
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Close}
                    primary={true}
                    classify="theme"
                    onClick={this.onCloseFileDetail}
                />
            )
        }
    }

    onShowFilter() {
        this.setState({ 
            showFilterPanel: true 
        });
    }

    onHideFilterPanel() {
        this.setState({ showFilterPanel: false });
    }

    onApprovalResend() {
        $$.messagedialog(false);
        this.ignoreReturnDateExpired = true;
        this.onApproval(SkipShowLoanedDataConfirm.Yes);
    }

    onCancelResendApproval () {
        $$.messagedialog(false);
        this.setState({ actionPanelStatus: { show: false } });
        this.clearCacheItems();
        this.loadData(this.state.pagerIndex, this.state.pagerSize);
    }

    onShowApprovalLoanedMessageBox(loanedDataList) {
        this.setState({ showApprovalLoanedDialog: true, loanedDataList : loanedDataList });
    }

    onApproval(skipShowLoanedDataConfirm) {
        let callback = (actionData, showErrorMsgInsidePanelCallBack) => {
            let validateFailed = false;
            if (actionData.RequestTypeMode == RequestTypeMode.Loan && actionData.Items.length == 1) {
                if (!validateFailed && (actionData.OnBehalf == null || actionData.OnBehalf.length == 0)) {
                    validateFailed = true;
                }
            } 
            if (validateFailed) {
                return false;
            }

            if (!skipShowLoanedDataConfirm) {
                let hasLoanedData = false;
                let loanedDataList = RM.deepcopy(this.state.loanedDataList);
                actionData.Items.forEach(item => {
                    if (item.HoldBy) {
                        hasLoanedData = true;
                        loanedDataList.push({
                            requestId: item.RequestId,
                            uniqueId: item.UniqueId,
                            requestedBy: item.HoldBy,
                        });
                    }
                    if(item.PhysicalFileInfos && item.PhysicalFileInfos.length > 0){
                        item.PhysicalFileInfos.forEach((file, index) => {
                            if (file.HoldBy) {
                                hasLoanedData = true;
                                loanedDataList.push({
                                    requestId: item.RequestId,
                                    uniqueId: file.UniqueId,
                                    requestedBy: file.HoldBy,
                                });
                            }
                        })
                    }
                });

                if (loanedDataList.length > 0) {
                    this.onShowApprovalLoanedMessageBox(loanedDataList, ApprovalPanelType.ApprovalPanel);
                    return false;
                }
            }

            let approvaldto = {
                Requests: []
            };
            var requests = [];
            for (let index = 0; index < actionData.Items.length; index++) {
                const element = actionData.Items[index];
                let disposalClass = {
                    HoldCategory: 1,
                };
                if (actionData.RequestTypeMode == RequestTypeMode.Loan) {
                    if (actionData.Items.length == 1) {
                        disposalClass.EndTimeStr = !actionData.ReturnDate.DateTimeObj ? "" : RM.TimeUtil.getCommonDateStr(new Date(actionData.ReturnDate.DateTimeObj));
                        disposalClass.TimeZoneId = actionData.ReturnDate.TimeZoneId;
                        disposalClass.IsDaylightSavingTime = actionData.ReturnDate.AutoAdjustClock;
                        disposalClass.ReviewComment = actionData.Comment;
                        requests.push({
                            Id: element.ItemId,
                            Title: element.Title,
                            Type: RequestTypeMode.Loan,
                            HoldUserId: actionData.OnBehalf[0].UserId,
                            HoldUserDisplay: actionData.OnBehalf[0].DisplayName,
                            DisposalClass: disposalClass,
                            GroupRequestId : element.GroupRequestId
                        });
                    }else{
                        disposalClass.ReviewComment = actionData.Comment;
                        requests.push({
                            Id: element.ItemId,
                            Title: element.Title,
                            Titles: element.Titles,
                            RequestId: element.RequestId,
                            RecordId: element.RecordId,
                            RecordIds: element.RecordIds,
                            Type: RequestTypeMode.Loan,
                            DisposalClass: disposalClass,
                            GroupRequestId : element.GroupRequestId
                        });
                    }
                } else if (Number(actionData.RequestTypeMode) === Number(RequestType.MovementRequest)) {
                    const sendEmailFlag = actionData.MoveDto
                        ? actionData.MoveDto.IsSendEmailToDestinationRM
                        : undefined;

                    requests.push({
                        Id: element.ItemId,
                        Title: element.Title || null,
                        Type: RequestType.MovementRequest,
                        DisposalClass: {
                            ReviewComment: actionData.Comment
                        },
                        MoveDto: this.normalizeMoveDto(element.MoveDto, element, sendEmailFlag)
                    });
                } else {
                    requests.push({
                        Id: element.ItemId,
                        Title: element.Title,
                        Type: RequestTypeMode.Creation,
                        DisposalClass: {
                            ReviewComment: actionData.Comment
                        }
                    });
                }
            }
            approvaldto.Requests = requests;
            if (this.ignoreReturnDateExpired) {
                approvaldto.IgnoreReturnDateExpired = true;
                approvaldto.ResendIdList = this.resendIdList;
            }
            this.approvalRequest(approvaldto,
                (result) => {
                    this.ignoreReturnDateExpired = false;
                    $$.loading(false);
                    this.setState({ showApprovalLoanedDialog: false })
                    if (result.NeedConfirmIgnoreReturnDate) {
                        this.resendIdList = result.FailedIdList;
                        let messageboxArgs = {
                            // classify: "warn",
                            width: "550px",
                            hideActions: false,
                            title: RMResx.RM_JS_Common_Confirmation,
                            content: RMResx.RM_PRM_PRE_ReturnDateIgnoredConfirmMessage,
                            buttons: [
                                {
                                    text: RMResx.RM_JS_Common_Cancel, 
                                    onClick: this.onCancelResendApproval
                                },
                                {
                                    text: RMResx.RM_JS_Common_OK,
                                    primary: true,
                                    classify: "theme",
                                    onClick: this.onApprovalResend
                                }]
                        };
                        $$.messagedialog(true, messageboxArgs);

                        return result;
                    }
                    if (result.StartLoanBoxJob) {
                        showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    if (result.HasError) {
                        if (Number(result.FailedType) === 2) {
                            this.onLoanConfirming(result.ErrorMsg);
                        } else {
                            if (result.ErrorMsg == RMResx.RM_JS_Common_AUI_Datepicker_Earlier) {
                                showErrorMsgInsidePanelCallBack(result.ErrorMsg);
                            } else {
                                this.dispatch(this.actoinPanelId, 'showMsgTip', { type: "error", msg: result.ErrorMsg || RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed });
                            }
                            if (result.FailedIdList != null && result.FailedIdList.length != requests.length) {
                                //Contains a successful request
                                this.clearCacheItems();
                                this.loadData(this.state.pagerIndex, this.state.pagerSize);
                            }
                            return result;
                        }
                    }
                    this.clearCacheItems();
                    this.setState({ actionPanelStatus: { show: false } });
                    this.loadData(this.state.pagerIndex, this.state.pagerSize);
                    return result;
                    //$$.loading(false);
                });
        };
        this.dispatch(this.actoinPanelId, 'onSave', callback);
        return false;//keep panel
    }

    onBeforeApproval(panelType) {
        let actionParam = this.state.requestActionParam.Items;
        let detailParam = this.state.requestDetailParam;
        let physicalId = [];
        let containsPhyBoxLoan = false;
        this.setState({approvalPanelType : panelType, loanedDataList : []});
        if (panelType == ApprovalPanelType.ApprovalPanel) {
            actionParam.forEach(item => {
                if (item.RequestType == RequestType.LoanRequest) {
                    if(item.PhysicalFileInfos && item.PhysicalFileInfos.length > 0){
                        item.PhysicalFileInfos.forEach(file => {
                            if(file.NodeType == ReportObjectLevel.PhyBox){
                                containsPhyBoxLoan = true;
                                physicalId.push(file.Id)
                            }
                        } );
                    }
                    else {
                        if(item.NodeType == ReportObjectLevel.PhyBox){
                            containsPhyBoxLoan = true;
                            physicalId.push(item.PhysicalId);
                        }
                    }
                }
            });
        } else if (panelType == ApprovalPanelType.ReviewApprovalPanel) {
            if (detailParam.Type == RequestType.LoanRequest) {
                if(detailParam.PhysicalFileInfos && detailParam.PhysicalFileInfos.length > 0){
                    detailParam.PhysicalFileInfos.forEach(file =>{
                        if(file.NodeType == ReportObjectLevel.PhyBox){
                            containsPhyBoxLoan = true;
                            physicalId.push(file.Id)
                        }
                    } );
                }else{
                    if(detailParam.PhysicalFileInfo.NodeType == ReportObjectLevel.PhyBox){
                        containsPhyBoxLoan = true;
                        physicalId.push(detailParam.PhysicalFileInfo.Id)
                    }
                }
            }
        }
        if (containsPhyBoxLoan) {
            let option = {
                url: "/api/PhysicalRequestApi/GetLoanFolderByBoxIds",
                method: "post",
                data: physicalId
            };
            fetchUtility(option).then((res) => {
                let phyData = res;
                if (phyData.length == 0) {
                    if (panelType == ApprovalPanelType.ApprovalPanel) {
                        this.onApproval(SkipShowLoanedDataConfirm.No);
                    } else if (panelType == ApprovalPanelType.ReviewApprovalPanel) {
                        this.reviewApproval(SkipShowLoanedDataConfirm.No);
                    }
                } else {
                    let loanedDataList = [];
                    phyData.map((data) => {
                        let holdByUser = "";
                        if (data.MetaInfo && data.MetaInfo[PhysicalDefaultColumnIDs.LoanedBy]) 
                        {
                            let loanInfo = JSON.parse(data.MetaInfo[PhysicalDefaultColumnIDs.LoanedBy]);
                            holdByUser = loanInfo[0].DisplayName;
                        }
                        loanedDataList.push({
                            requestId: data.RequestId,
                            uniqueId: data.UniqueId,
                            requestedBy: holdByUser,
                        });
                    });
                    this.setState({loanedDataList : loanedDataList}, () => {
                        if (panelType == ApprovalPanelType.ApprovalPanel) {
                                $$.messagedialog(false);
                                this.onApproval(SkipShowLoanedDataConfirm.No);
                        } else if (panelType == ApprovalPanelType.ReviewApprovalPanel) {
                            $$.messagedialog(false);
                            this.reviewApproval(SkipShowLoanedDataConfirm.No);
                        }
                    })
                }
            }).catch((e) => {

            });
        } 
        else {
            if (panelType == ApprovalPanelType.ApprovalPanel) {
                this.onApproval(SkipShowLoanedDataConfirm.No);
            } else if (panelType == ApprovalPanelType.ReviewApprovalPanel) {
                this.reviewApproval(SkipShowLoanedDataConfirm.No);
            }
        }
        return false;//keep panel
    }

    formatMovementPayload(dto) {
        if (!dto || !dto.Requests || dto.Requests.length === 0) {
            return dto;
        }

        let payload = RM.deepcopy(dto);
        const globalEmailFlag = typeof payload.IsSendEmailToDestinationRM === "boolean"
            ? payload.IsSendEmailToDestinationRM
            : undefined;

        if (Object.prototype.hasOwnProperty.call(payload, "IsSendEmailToDestinationRM")) {
            delete payload.IsSendEmailToDestinationRM;
        }
            
        payload.Requests = payload.Requests.map((requestItem) => {
            if (!this.isMovementRequestType(requestItem.Type)) {
                return requestItem;
            }
            return {
                ...requestItem,
                MoveDto: this.normalizeMoveDto(requestItem.MoveDto, requestItem, globalEmailFlag)
            };
        });

        return payload;
    }

    isMovementRequestType(type) {
        return Number(type) === Number(RequestType.MovementRequest);
    }

    hasMovementRequest(payload) {
        if (!payload || !payload.Requests || payload.Requests.length === 0) {
            return false;
        }
        return payload.Requests.some((r) => this.isMovementRequestType(r.Type));
    }

    buildMovementMoveDto(element) {
        return this.normalizeMoveDto(element && element.MoveDto ? element.MoveDto : {});
    }

    extractSourcePhyRecordIdsFromRequest(requestItem) {
        const safeItem = requestItem || {};
        const safeMoveDto = safeItem.MoveDto || {};
        const fromMoveDto = safeMoveDto.SourcePhyRecordIds;

        if (Array.isArray(fromMoveDto) && fromMoveDto.length > 0) {
            return fromMoveDto.filter(Boolean);
        }

        if (Array.isArray(safeItem.RecordIds) && safeItem.RecordIds.length > 0) {
            return safeItem.RecordIds.filter(Boolean);
        }

        if (typeof safeItem.RecordIds === "string" && safeItem.RecordIds.trim() !== "") {
            return safeItem.RecordIds.split(",").map((id) => id.trim()).filter(Boolean);
        }

        if (safeItem.RecordId) {
            return [safeItem.RecordId];
        }

        return [];
    }

    normalizeMoveDto(moveDto, requestItem, overrideEmailFlag) {
        const sourceIds = this.extractSourcePhyRecordIdsFromRequest({
            ...(requestItem || {}),
            MoveDto: moveDto || {}
        });

        const resolvedEmailFlag = typeof overrideEmailFlag === "boolean"
            ? overrideEmailFlag
            : !!(moveDto && moveDto.IsSendEmailToDestinationRM);

        return {
            SourcePhyRecordIds: sourceIds,
            LocationId: moveDto && moveDto.LocationId ? moveDto.LocationId : "",
            BoxId: moveDto && moveDto.BoxId ? moveDto.BoxId : "",
            FolderId: moveDto && moveDto.FolderId ? moveDto.FolderId : "",
            NameConflictOption: Number((moveDto && moveDto.NameConflictOption) || 1),
            DestinationPath: moveDto && moveDto.DestinationPath ? moveDto.DestinationPath : "",
            FromModule: moveDto ? moveDto.FromModule : undefined,
            IsSendEmailToDestinationRM: resolvedEmailFlag
        };
    }

    parseCheckItemOnLoanResult(result) {
        if (typeof result === "boolean") {
            return result;
        }
        if (result && typeof result.Data === "boolean") {
            return result.Data;
        }
        if (result && typeof result.Result === "boolean") {
            return result.Result;
        }
        return false;
    }

    showMovementLoanConfirmDialog(payload, callbackFun) {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_PRM_PRE_Msg_MovementFailed_Title,
            content: RMResx.RM_PRM_PRE_Msg_MovementFailed,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                        this.executeApproveRequest(payload, callbackFun);
                    }
                }
            ]
        });
    }

    checkItemOnLoanThenApprove(payload, callbackFun) {
        $$.loading(true);
        let option = {
            url: "/api/PhysicalRequestApi/CheckItemOnLoan",
            method: "POST",
            data: payload
        };

        fetchUtility(option).then((res) => {
            $$.loading(false);
            let hasLoanedItem = this.parseCheckItemOnLoanResult(res);
            if (hasLoanedItem) {
                this.showMovementLoanConfirmDialog(payload, callbackFun);
                return;
            }
            this.executeApproveRequest(payload, callbackFun);
        }).catch(() => {
            $$.loading(false);
        });
    }

    handleMovementApproveResult(result) {
        if (!result || !result.MoveResult) {
            return;
        }

        let moveResult = result.MoveResult;
        let jobId = moveResult.JobId;

        if (!jobId) {
            return;
        }

        if (moveResult.IsStartJob === true) {
            showToast.success(RMResx.RM_PRM_PRE_Msg_Movement_Notification);
        }

        this.fetchRealTimeJobStatus(jobId);
    }

    parseRealTimeJobStatusResponse(response) {
        if (!response) {
            return null;
        }

        if (typeof response === "string") {
            try {
                return JSON.parse(response);
            } catch (error) {
                return null;
            }
        }

        return response;
    }

    fetchRealTimeJobStatus(jobId) {
        const poll = () => {
            let option = {
                url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                method: "GET"
            };

            fetchUtility(option).then((res) => {
                const statusResult = this.parseRealTimeJobStatusResponse(res);

                if (!statusResult) {
                    showToast.error(`^Failed to get real-time job status. JobId: ${jobId}`);
                    return;
                }

                if (Number(statusResult.Status) === RealTimeJobStatus.Running) {
                    setTimeout(poll, 1000);
                    return;
                }

                this.notifyRealTimeJobStatusResult(statusResult, jobId);
            }).catch(() => {
                showToast.error(`^Failed to get real-time job status. JobId: ${jobId}`);
            });
        };

        poll();
    }

    notifyRealTimeJobStatusResult(result, jobId) {
        if (!result) {
            showToast.error(`^Failed to get real-time job status. JobId: ${jobId}`);
            return;
        }

        const messageType = Number(result.MessageType);
        if (messageType === RealTimeJobMessageType.Successful) {
            showToast.success(`Real-time job status loaded successfully. JobId: ${jobId}`);
            return;
        }

        showToast.error(result.ErrorMessage || result.ErrorMsg || `^Failed to get real-time job status. JobId: ${jobId}`);
    }

    executeApproveRequest(payload, callbackFun) {
        $$.loading(true);
        let option = {
            url: `/api/PhysicalRequestApi/Approve`,
            method: "POST",
            data: payload
        };
        fetchUtility(option).then((result) => {
            this.handleMovementApproveResult(result);
            callbackFun(result);
        }).catch(() => {
            $$.loading(false);
        });
    }

    approvalRequest(approvaldto, callbackFun){
        let payload = this.formatMovementPayload(approvaldto);
        if (this.hasMovementRequest(payload)) {
            this.checkItemOnLoanThenApprove(payload, callbackFun);
            return;
        }
        this.executeApproveRequest(payload, callbackFun);
    }

    rejectRequest(rejectDto, callbackFun){
        $$.loading(true);
        let url = `/api/PhysicalRequestApi/Reject`;
        let option = {
            url: url,
            method: "POST",
            data: rejectDto
        };
        fetchUtility(option)
            .then(callbackFun)
            .catch((e) => {

            });
    }

    onReject() {
        let callback = (actionData) => {
            let rejectDto = {
                Requests: []
            };
            var requests = [];
            for (let index = 0; index < actionData.Items.length; index++) {
                const element = actionData.Items[index];
                requests.push({
                    Id: element.ItemId,
                    Title: element.Title,
                    DisposalClass: {
                        ReviewComment: actionData.Comment
                    }
                });
            }
            rejectDto.Requests = requests;
            this.rejectRequest(rejectDto,
                (result) => {
                    $$.loading(false);
                    if (result.HasError) {
                        this.dispatch(this.actoinPanelId, 'showMsgTip', { type: "error", msg: result.ErrorMsg || RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed });
                        if (result.FailedIdList != null && result.FailedIdList.length != requests.length) {
                            //Contains a successful request
                            this.clearCacheItems();
                            this.loadData(this.state.pagerIndex, this.state.pagerSize);
                        }
                        return result;
                    }
                    this.clearCacheItems();
                    this.setState({ actionPanelStatus: { show: false } });
                    this.loadData(this.state.pagerIndex, this.state.pagerSize);
                });
        };
        this.dispatch(this.actoinPanelId, 'onSave', callback);
        return false;
    }

    onShowApprovalActionPanel() {
        this.actionType = ActionTypeMode.Approval;
        this.approvalOrRejectInit(this.getSelectedItems(), true);
    }

    onShowRejectActionPanel() {
        this.actionType = ActionTypeMode.Reject;
        this.approvalOrRejectInit(this.getSelectedItems(), false);
    }

    onShowCancelActionMessageBox(actionData) {
        let cancelMsgContent = <div>
            <div>{RMResx.RM_RC_Request_Action_Msg_ConfirmCancel}</div>
        </div>;
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>{cancelMsgContent}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false, this.args);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onCancelRequestMsgSureClick.bind(this, actionData)
                },  
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onCancelRequestMsgSureClick(actionData) {
        $$.messagedialog(false, this.args);
        $$.loading(true);
        let cancelRequestDto = {
            Requests: []
        };
        var requests = [];
        
        for (let index = 0; index < actionData.length; index++) {
            const element = actionData[index];
            requests.push({
                Id: element.Id,
                Title: element.Title,
            });
        }
        cancelRequestDto.Requests = requests;
        let url = `/api/PhysicalRequestApi/CancelRequest`;
        let option = {
            url: url,
            method: "POST",
            data: cancelRequestDto
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.clearCacheItems();
            this.loadData(this.state.pagerIndex, this.state.pagerSize);
        })
            .catch((e) => {
                $$.loading(false);
            });
    }

    reviewApproval(skipShowLoanedDataConfirm){
        let callback = (actionData) => {
            if (!skipShowLoanedDataConfirm) {
                let hasLoanedData = false;
                let loanRequestHoldUserMissing = false;
                let loanedDataList = RM.deepcopy(this.state.loanedDataList);;
                actionData.Requests.forEach(item => {
                    if (item.Type == RequestTypeMode.Loan && (item.HoldUserDisplay == null && item.HoldUserId == null)) {
                        loanRequestHoldUserMissing = true;
                    }
                    item.PhysicalFileInfos && item.PhysicalFileInfos.forEach((physicalFileInfo) => {
                        if (physicalFileInfo.HoldBy) {
                            hasLoanedData = true;
                            loanedDataList.push({
                                requestId: item.RequestId,
                                uniqueId: physicalFileInfo.UniqueId,
                                requestedBy: physicalFileInfo.HoldBy,
                            });
                        }
                    });
                });
                if (loanRequestHoldUserMissing) {
                    return false;
                }

                if (loanedDataList.length > 0) {
                    this.onShowApprovalLoanedMessageBox(loanedDataList, ApprovalPanelType.ReviewApprovalPanel);
                    return false;
                }
            }

            this.approvalRequest(actionData,
                (result) => {
                    $$.loading(false);
                    this.setState({ showApprovalLoanedDialog: false })
                    if (result.StartLoanBoxJob) {
                        showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    if (result.HasError) {
                        if (Number(result.FailedType) === 2) {
                            this.onLoanConfirming(result.ErrorMsg);
                        } else {
                            this.dispatch(this.requestDetailPanelId, 'showMsgTip', { type: "error", msg: result.ErrorMsg || RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed });
                            return result;
                        }
                    }
                    this.clearCacheItems();
                    this.setState({ showRequestDetail: { show: false } });
                    this.loadData(this.state.pagerIndex, this.state.pagerSize);    
                });
        };
        this.dispatch(this.requestDetailPanelId, "onSave", callback);
        this.setState({ batchActionDisable : true});
        return false;
    }

    reviewReject(){
        let callback = (actionData) => {
            let loanRequestHoldUserMissing = false;
            actionData.Requests.forEach(item => {
                if (item.Type == RequestTypeMode.Loan && (item.HoldUserDisplay == null && item.HoldUserId == null)) {
                    loanRequestHoldUserMissing = true;
                }
            });
            
            if (loanRequestHoldUserMissing) {
                return false;
            }
            this.rejectRequest(actionData,
                (result) => {
                    $$.loading(false);
                    if (result.HasError) {
                        this.dispatch(this.requestDetailPanelId, 'showMsgTip', { type: "error", msg: result.ErrorMsg || RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed });
                        return result;
                    }
                    this.clearCacheItems();
                    this.setState({ showRequestDetail: { show: false } });
                    this.loadData(this.state.pagerIndex, this.state.pagerSize);
                    
                    
                });
        };
        this.dispatch(this.requestDetailPanelId, "onSave", callback);
        return false;
    }

    onSearch = (args) => {
        this.searchKey = args;
        if($.trim(this.searchKey)){
            this.clearCacheItems();
        }
        this.loadData(0, this.state.pagerSize);
    }

    onStopSearch = (args) => {
        this.searchKey = "";
        this.loadData(0, this.state.pagerSize);
    }

    onCloseRequestDetail() {
        this.setState({
            showRequestDetail: { show: false },
        });
    }

    onCloseFileDetail() {
        this.setState({
            showFileDetail: { show: false },
        });
    }

    onEditRequestFile() {
        let formData = {
            formType: 4,
            NodeType: this.state.fileDetailParam.nodeType,
            Id: this.state.fileDetailParam.id,
        };
        this.setState({
            showEditFilePanel: {show: true},
            fileData: formData,
            requestId : this.state.fileDetailParam.requestId,
        });
    }

    onShowRequestDetail(data) {
        //only edit waiting for approval file
        this.requestDetailEditable = this.isAdmin && data.Status == RequestStatusMode.WaitingForAproval;
        this.setState({
            showRequestDetail: { show: true },
            requestDetailParam: data
        });
        //this.dispatch(this.requestDetailPanelId, "init", data);
    }

    onShowFileDetail(reqDto, isRequest = true, selectedIndex = 0) {
        let recId, nodeType = "";
        let currentPhysicalFileInfos = [reqDto.PhysicalFileInfo];
        if(reqDto.PhysicalFileInfos && reqDto.PhysicalFileInfos.length > 0){
            currentPhysicalFileInfos = [...reqDto.PhysicalFileInfos];
        }
        this.setState({allPhysicalFileInfos : currentPhysicalFileInfos, activeTab : selectedIndex});

        if(currentPhysicalFileInfos) {
            recId = currentPhysicalFileInfos[selectedIndex].Id;
            if(currentPhysicalFileInfos[selectedIndex].NodeType){
                nodeType = currentPhysicalFileInfos[selectedIndex].NodeType;
            }else{
                //?????, undefined?folder
                nodeType = 9400;
            }
            
        }
        this.setState({
            showFileDetail: {show: true},
            fileDetailParam: {
                status: reqDto.Status,
                isRequest: isRequest,
                requestId: reqDto.Id,
                id: recId,
                nodeType: nodeType
            }
        });
    }

    onSaveRequestFile() {
        $$.loading(true);
        this.dispatch(this.fileEditFormId, 'onSave', (success, data) => {
            if (success) {
                this.setState({ showEditFilePanel: { show: false } });
                this.onCloseFileDetail();
                this.onCloseRequestDetail();
            }
            $$.loading(false);
        });
    }

    selectedPageChanged = (args) => {
        //console.log(args)
    }

    onCheckChanged(items) {
        let currentPageItems = items.slice();
        this.updateCacheItemsStatus(currentPageItems);
        this.setBtnState(this.cacheItems.filter(t=>t.isChecked));
    }

    cellOperate(args, tableSelectedOption) {
        switch (tableSelectedOption.index) {
            case 1: //approval
                this.actionType = ActionTypeMode.Approval;
                this.approvalOrRejectInit([args], true);
                break;
            case 2: //reject
                this.actionType = ActionTypeMode.Reject;
                this.approvalOrRejectInit([args], false);
                break;
            case 3: //cancel
                this.onShowCancelActionMessageBox([args]);
        }
    }

    cellClick(data, action, selectedIndex) {
        switch (action) {
            case 1: //request detail
                this.onShowRequestDetail(data);
                break;
            case 2: //file detail
                this.onShowFileDetail(data, false, selectedIndex);
                break;
        }
    }

    //reset button status
    setBtnState(items) {
        let batchActionDisable = false;
        if (items.length > 0) {
            let firstItemType = items[0].Type;
            for (let index = 0; index < items.length; index++) {
                if (items[index].Status != 0) {
                    this.setState({
                        batchActionDisable: true,
                    });
                    return;
                }
                if (firstItemType != items[index].Type) {
                    this.setState({
                        batchActionDisable: true,
                    });
                    return;
                }
            }
        }else{
            batchActionDisable = true;
        }
        this.setState({
            batchActionDisable: batchActionDisable,
        });
    }

    approvalOrRejectInit(selectedItems, isApprval) {
        if(selectedItems.length <= this.operateLimitCount)
        {
            let actionData = {
                Items: selectedItems.map((item) => {
                    let holdBy = item.PhysicalFileInfo ? item.PhysicalFileInfo.HoldBy : "";
                    let nodeType = item.PhysicalFileInfo ? item.PhysicalFileInfo.NodeType : "";
                    let physicalId = item.PhysicalFileInfo ? item.PhysicalFileInfo.Id : "";
                    return {
                        ItemId: item.Id,
                        Title: item.Title,
                        Titles : item.Titles,
                        RequestId: item.RequestId,
                        RecordId: item.RecordId,
                        RecordIds: item.RecordIds,
                        HoldBy: holdBy,
                        RequestType: item.Type,
                        NodeType: nodeType,
                        PhysicalId: physicalId,
                        PhysicalFileInfos: item.PhysicalFileInfos,
                        GroupRequestId : item.GroupRequestId,
                        MoveDto: item.MoveDto
                    };
                }),
                ActionType: isApprval ? ActionTypeMode.Approval : ActionTypeMode.Reject
            };
            if (selectedItems.length > 0) {
                actionData.RequestTypeMode = selectedItems[0].Type;
            }
            if (selectedItems.length == 1) {
                let item = selectedItems[0];
                if (item.Type == RequestTypeMode.Loan) {
                    // let returnDate = item.DisposalClass.EndTime == 0 ? null : RM.TimeUtil.ticksToDate(item.DisposalClass.EndTime);
                    if(item.DisposalClass.EndTime == 0){
                        actionData.ReturnDate = { DateTimeObj: null };
                    }else{
                        actionData.ReturnDate = { DateTimeObj: new Date(item.DisposalClass.EndTimeStr),
                            TimeZoneId: item.DisposalClass.TimeZoneId,
                            IsDaylightSavingTime: item.DisposalClass.IsDaylightSavingTime
                        };
                    }
                    if (item.HoldUserId != null || item.HoldUserDisplay != null) {
                        actionData.OnBehalf = [{
                            UserId: item.HoldUserId,
                            DisplayName: item.HoldUserDisplay,
                            UserPrincipalName: "",
                            Checked: true
                        }];
                    }else{
                        actionData.OnBehalf = [];//Creation Request
                    }
                }
            }else{
                actionData.ReturnDate = { DateTimeObj: null };
                actionData.OnBehalf = [];
            }
            // this.dispatch(this.actoinPanelId, 'init', actionData);
            this.setState({ 
                requestActionParam: actionData ,
                actionPanelStatus: { show: true },
                actionPanelTitle: isApprval ? RMResx.RM_RC_Request_Action_ApprovalRequest : RMResx.RM_RC_Request_Action_RejectRequest
            });
        }else{
            this.showMessagebox();
        }
    }

    showMessagebox = ()=> {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: PageI18N.OperateLimit.format(this.operateLimitCount),
            buttons: [{ text: RMResx.RM_JS_Common_OK, onClick: this.hideMessageBox }]
        };
        $$.messagedialog(true, this.args);
    }

    hideMessageBox = ()=>{
        $$.messagedialog(false);
    }

    setPanelHeader(templateName) {
        this.setState({
            formTemplateName: templateName,
        });
    }

    onLoanConfirming = (errorMsg) => {
        $$.messagedialog(true, {
            width: "550px",
            title: RMResx.RM_RC_Request_Action_LoanedConfirmTitle,
            content: errorMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                }
            ]
        });
    }

    getActionPanelButtons = () => {
        if (this.actionType == ActionTypeMode.Approval) {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={() => this.setState({ actionPanelStatus: { show: false } })}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_RC_Request_Action_ApprovalRequest}
                        primary={true}
                        classify="theme"
                        onClick={this.onBeforeApproval.bind(this, ApprovalPanelType.ApprovalPanel)}
                    />
                </>
            )
        } else if (this.actionType == ActionTypeMode.Reject) {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={() => this.setState({ actionPanelStatus: { show: false } })}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_RC_Request_Action_RejectRequest}
                        primary={true}
                        classify="theme"
                        onClick={this.onReject}
                    />
                </>
            )
        }
    }

    getDetailPanelButtons = () => {
        if (this.requestDetailEditable) {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCloseRequestDetail}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_RC_Request_Action_ApprovalRequest}
                        primary={true}
                        classify="theme"
                        onClick={this.onBeforeApproval.bind(this, ApprovalPanelType.ReviewApprovalPanel)}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_RC_Request_Action_RejectRequest}
                        primary={true}
                        classify="theme"
                        onClick={this.reviewReject}
                    />
                </>
            )
        } else {
            return (
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Close}
                    primary={true}
                    classify="theme"
                    onClick={this.onCloseRequestDetail}
                />
            )
        }
    }

    renderActionPanel() {
        return <R.Panel
            id="actoinPanel"
            header={this.state.actionPanelTitle}
            status={this.state.actionPanelStatus}
            destroy={true}
            size={600}
        >
            <div className="ra-panel-content">
                <RequestAction
                    id={this.actoinPanelId}
                    data={this.state.requestActionParam}
                ></RequestAction>
            </div>
            {this.getActionPanelButtons()}
        </R.Panel>;
    }

    renderApprovalLoanedDialog = () => {
        const builtInColumns = [
            {
                header: RMResx.RM_Request_Loaned_UniqueID,
                width: 200,
            },
            {
                header: RMResx.RM_Request_Loaned_RequestedBy,
                width: 200,
            },
            {
                header: RMResx.RM_Request_Loaned_RequestID,
                width: 200,
            },
        ];
        return (
            <R.Dialog
                id="raAddWhitelist"
                header={RMResx.RM_RC_Request_Action_ApprovalRequest}
                width={680}
                // height={346}
                status={{ show: this.state.showApprovalLoanedDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={() => this.setState({ showApprovalLoanedDialog: false })}
            >
                <div>
                    <p style={{ margin: 0 }}>
                       {RMResx.RM_Request_Loaned_Message}
                    </p>
                    <div className="margin-top-m">
                        <R.Table
                            id="raApprovalLoannedTable"
                            rowTemplate={LoanedRecordsTableTemplate}
                            items={this.state.loanedDataList}
                            columns={builtInColumns}
                        />
                    </div>
                </div>
                <R.Button
                    slot="buttons"
                    classify="blank"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => this.setState({ showApprovalLoanedDialog: false })}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_Request_Loaned_Continue}
                    onClick={() => this.state.approvalPanelType == ApprovalPanelType.ApprovalPanel ? this.onApproval(SkipShowLoanedDataConfirm.Yes) : this.reviewApproval(SkipShowLoanedDataConfirm.Yes)}
                />
            </R.Dialog>
        );
    }

    renderRequestDetail() {
        return <R.Panel
            id="raRequestDetailPanel"
            header={RMResx.RM_PRM_MyRequest_RequestDetail}
            status={this.state.showRequestDetail}
            destroy={true}
            size={600}
        >
            <div className="ra-panel-content">
                <RequestDetailForm
                    id={this.requestDetailPanelId}
                    // onSave={(data) => {
                    //     this.detailFormData = data;
                    // }}
                    data={this.state.requestDetailParam}
                    onShowFileDetail={this.onShowFileDetail}
                />
            </div>
            {this.getDetailPanelButtons()}
        </R.Panel>;
    }

    renderButton() {
        if (this.isAdmin) {
            return <div className='navbar-actions'>
                {!this.state.batchActionDisable && 
                <R.Button
                    primary={true} classify="theme" type="button" text={RMResx.RM_RC_Request_Action_ApprovalRequest} tooltip={RMResx.RM_RC_Request_Action_ApprovalRequest}
                    disabled={this.state.batchActionDisable} onClick={this.onShowApprovalActionPanel} />
                }
                {!this.state.batchActionDisable && 
                <R.Button
                    type="button" icon="fia-close" text = {RMResx.RM_RC_Request_Action_RejectRequest} tooltip={RMResx.RM_RC_Request_Action_RejectRequest}
                    disabled={this.state.batchActionDisable} onClick={this.onShowRejectActionPanel} />                     
                }
            </div>; 
        }else{
            return <div className='navbar-actions'>
                {!this.state.batchActionDisable && <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_RC_Request_Action_CancelRequest}
                    disabled={this.state.batchActionDisable}
                    onClick={() => {
                        this.onShowCancelActionMessageBox(this.getSelectedItems());
                    }} />}
            </div>;
        }
    }

    getFileDetailParam(id){
        return {
            status: this.state.fileDetailParam.Status,
            isRequest: false,
            requestId: this.state.fileDetailParam.Id,
            id: id,
            nodeType: this.state.fileDetailParam.nodeType
        }
    }

    setActiveTab(index){
        this.setState({activeTab : index})
    }

    renderFileDetail() {
        let onlyOneItem = this.state.allPhysicalFileInfos.length === 1;
        let multItems = this.state.allPhysicalFileInfos.length > 1;
        return <R.Panel
            id="raFileDetailPanel"
            header={RMResx.RM_PRM_RequestManagement_CreationFileDetail}
            status={this.state.showFileDetail}
            destroy={true}
            size={600}
        >
            <div id="raRequestFileDetail">
                {
                    onlyOneItem && 
                    <PhyObjectDetail
                        data={this.state.fileDetailParam}
                        isRequest={true}
                    ></PhyObjectDetail>
                }
                {   
                    multItems &&                     
                    <R.Tabcontrol
                        flex
                        onChange={(index) => this.setActiveTab(index)}
                        active={this.state.activeTab}
                    >   
                    {
                        this.state.allPhysicalFileInfos.map((item, index) => (
                        <R.TabPanel
                            key={item.UniqueId}
                            tab={item.UniqueId}
                            aria-label={item.UniqueId}
                        > {
                            index === this.state.activeTab && 
                            <PhyObjectDetail
                                data={this.getFileDetailParam(item.Id)}
                                isRequest={true}
                            ></PhyObjectDetail>
                        }
                        </R.TabPanel>
                    ))}
                    </R.Tabcontrol>
                    
                }
            </div>
            {this.getFileDetailButtons()}
        </R.Panel>;
    }

    getEditFileTitle() {
        let nodeType = this.state.fileDetailParam.nodeType;
        switch (nodeType) {
            case NodeType.PhyFile:
                return RMResx.RM_PRM_PRE_PanelTitle_EditFile;
            case NodeType.PhyBox:
                return RMResx.RM_PRM_PRE_PanelTitle_EditBox;
            case NodeType.PhyRecord:
                return RMResx.RM_PRM_PRE_PanelTitle_EditRecord;
            default:
                return RMResx.RM_PRM_PRE_PanelTitle_EditFile;
        }
    }

    renderFileEditPanel() {
        return <R.Panel
            id="fileEditPanel"
            header={this.getEditFileTitle()}
            size={600}
            status={this.state.showEditFilePanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="phy-head">{RMResx.RM_JS_BCM_Explorer_Template}</span>
                <span className="phy-head margin-xs">{this.state.formTemplateName}</span>
            </div>
            <div className="ra-panel-content">
                <PhyObjectForm
                    id={this.fileEditFormId}
                    data={this.state.fileData}
                    requestId={this.state.requestId}
                    setPanelTitle={this.setPanelHeader}
                    loadData={() => this.loadData(this.state.pagerIndex, this.state.pagerSize)}
                >
                </PhyObjectForm>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_BCM_Explorer_Button_Back} onClick={() => { this.setState({ showEditFilePanel: { show: false } }); }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveRequestFile.bind(this)} />
            </>
        </R.Panel>;
    }

    renderNavBar() {
        return (
            <div className="ra-main-header">
                <div className="navbar-search">
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={(args) => (args || "").trim() === "" ? this.onStopSearch(args) : this.onSearch(args)}
                        width = {380}
                    />
                </div>
                <div className="flex">
                    <R.Button
                        className="theme"
                        text={RMResx.RM_Common_Filter}
                        type="button"
                        icon="fia-filter"
                        tooltip={RMResx.RM_PRM_PRE_Filter}
                        onClick={this.onShowFilter}
                    />
                </div>
            </div>
        );
    }

    renderActionBar() {
        return (
            <div className="ra-main-navbar">
                <div className="navbar-left">{this.renderButton()}</div>
                <div className="navbar-right"></div>
            </div>
        );
    }

    renderFilterPanel() {
        return (
            <R.Panel
                header={RMResx.RM_Common_Filter}
                size={660}
                status={{ show: this.state.showFilterPanel }}
                onHide={this.onHideFilterPanel}
                destroy={true}
            >
                <MyRequestFilterForm
                    id="mrFilterForm"
                    requestTypeItems={this.state.requestTypeItems}
                    requestByItems={this.state.requestByItems}
                    approvalStatusItems = {this.state.approvalStatusItems}
                    filterOptionsInfo={this.state.filterOptionsInfo}
                    timeRange = {this.state.timeRange}
                    jumpId = {this.jumpId}
                    isAdmin = {this.isAdmin}
                >
                </MyRequestFilterForm>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onHideFilterPanel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
                </>
            </R.Panel>
        );
    }
    
    render() {

        // let sitemapLink = this.isAdmin
        //     ? SiteMapLinks.PRM_RequestManagement
        //     : SiteMapLinks.PRM_MyRequest;
        let sitemapLink = SiteMapLinks.PRM_RequestForReview;
        return (
            <div id="raMyRequestContainer">
                <$g.SiteMap data={[sitemapLink]} />

                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                />
                <div className="ra-page-main">
                    {this.renderNavBar()}
                    {this.renderActionBar()}
                    <div className="ra-main-table">
                        <RequestTable
                            id="requestTable"
                            onCheckChanged={this.onCheckChanged}
                            cellOperate={this.cellOperate}
                            cellClick={this.cellClick}
                        />
                        {/* </div> */}
                    </div>
                    <div className={"ra-main-footer"}>
                        <$g.Pager
                            itemsCount={this.state.pagerTotal}
                            pagerIndex={this.state.pagerIndex}
                            pagerSize={this.state.pagerSize}
                            showPagerSize={true}
                            showPagerCounter={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.loadData}
                        />
                    </div>
                </div>
                {this.renderFilterPanel()}
                {this.renderRequestDetail()}
                {this.renderFileDetail()}
                {this.renderFileEditPanel()}
                {this.renderActionPanel()}
                {this.renderApprovalLoanedDialog()}
            </div>
        );
    }
}