import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { bindEvents, getRequestVerificationToken, LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import '../../../../Less/PRM/ManageHold.less';
import PhyHoldForm from "./PhyHoldForm";
import PhyHoldDetailForm from "./PhyHoldDetailForm";
import { Messagebox } from "../../../Common/Messagebox";
import StringUtil from "../../../../Utilities/StringUtil";
import { checkPermission } from "../../../../Utilities/permissionManager";
import WorkspaceHoldForm from "./WorkspaceForm";
import { DataSourceType } from "../Constants";

const ColumnHeadNames = {
    Title: RMResx.RM_JS_RDM_Explorer_HoldType,
    CreateTime: RMResx.RM_JS_RDM_Explorer_CreateTime,
    Duration: RMResx.RM_JS_RDM_Explorer_HoldSetting,
    Comment: RMResx.RM_JS_JM_Comment
};
let limitCount = 10;
export default class PhyManageHold extends R.Component {
    idAttr = true;
    componentCreate() {
        this.checkSPOLicense = checkPermission("Source_SP", RM.UserResources);
        this.checkODLicense = checkPermission("Source_OneDrive", RM.UserResources);
        this.checkTeamsLicense = LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources);
        this.checkEXOLicense = checkPermission("Source_EXO", RM.UserResources);
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            disabled: false,
            pagerIndex: 0,
            pagerSize: 10,
            itemsCount: 0,
            shownCount: 0,
            batchSelection: false,
            singleSelection: false,
            hasRelatedRecord: false,
            selectedItemsCount: 0,

            items: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
            tableColumns: this.getColumnInfo(),
            workspaceColumns: this.getWorkspaceColumnInfo(),
            phyHoldDetailFormData: {},
            detailFormPanelTitle: "",
            showDetailPanel: { show: false },
            phyHoldFormData: {},
            showHoldPanel: { show: false },
            formPanelTitle: RMResx.RM_PRM_PRE_EditHold,

            removeDialogShow: false,
            releaseDialogShow: false,

            showRemoveExpender: { show: true },
            dialogTitle: { header: RMResx.RM_PRM_PRE_RemoveHold, title: RMResx.RM_PRM_PRE_Dialog_RemoveTheHolds },
            contextManageHoldTemp: {},
            activeTab: 0,
            dataSourceList: this.getDataSourceOptions(),
        };
        this.isPhysicalRecords = this.props.location.state == 'phy';
        this.isAll = this.props.location.state == 'all';
        this.holdCategory = 0;
        this.profileType = -1;
        this.holdFormId = "phyHoldFormId";
        this.workspaceFormId = "workspaceFormId";
        this.holdDetailFormId = "phyHoldDetailFormId";
        this.showImportPanel = { show: false };

        this.searchedItems = [];
        this.cachedAllItems = [];
        this.selectdItems = [];
        this.operateLimitCount = 15;
        this.isAdmin = RM.gData.isPhysicalAdmin;
        this.isContextOperate = false;  //delete from context list
        this.operationType = 2; //2 delete, 3 release
        //this.tableColumns = this.getColumnInfo();
        this.isSelectAll = false;
        this.searchKey = "";
        bindEvents(this, "onCheckChange", "setBtnState", "renderSearchbox", "cellClick", "cellOperate", "onPageChange",
            "onSelectAllChanged", "onSaveFormHold", "getSelectedItems", "onEditHold", "onNewHold", "onDoRemove", "onDoRelease", "onDialogConfirm",
            "hideMessageTip", "renderRemoveHoldDialog", "onDeleteHold", "onCancelRemove", "onDoExport", "handleExportBtn", "renderImportPanel",
            "renderImportFromTemplate", "handleDownloadTemplate", "onShowImportPanel", "onHideImportPanel", "handleHoldImport");
    }

    componentInit() {
        this.loadData();
    }

    getDataSourceOptions() {
        let options = [
            {
                name: RMResx.RM_JS_RDM_SPO,
                value: DataSourceType.SPO,
                checked: false,
                isShow: this.checkSPOLicense && !LicenseHelper.HasOpusGoogleLicenseOnly(),
            },
            {
                name: RMResx.RM_JS_RDM_Teams,
                value: DataSourceType.Teams,
                checked: false,
                isShow: this.checkTeamsLicense && !LicenseHelper.HasOpusGoogleLicenseOnly(),
            },
            {
                name: RMResx.RM_JS_RDM_EXO,
                value: DataSourceType.EXO,
                checked: false,
                isShow: this.checkEXOLicense,
            },
            {
                name: RMResx.RM_JS_RDM_OD,
                value: DataSourceType.OD,
                checked: false,
                isShow: this.checkODLicense,
            }
        ];
    
        let filteredOptions = options.filter(item => item.isShow);
        if (filteredOptions.length > 0) {
            filteredOptions[0].checked = true;
        }
    
        return filteredOptions;
    }

    getHoldApiUrl() {
        if (RM.RoleType === 5) {
            return "/api/RecordsExplorerApi/GetAssignedHolds";
        }
        if (this.state.activeTab === 1) {
            return "/api/RecordsExplorerApi/GetWorkspaceHoldsByPageSize";
        }
        return "/api/RecordsExplorerApi/GetAllHolds";
    }

    getSelectedItems() {
        if (this.searchKey == "") {
            return this.cachedAllItems.filter(t => t.isChecked);
        } else {
            return this.searchedItems.filter(t => t.isChecked);
        }
    }

    loadData = (callback) => {
        $$.loading(true);
        let option = {
            url: this.getHoldApiUrl(),
            method: "Get"
        }; 
    fetchUtility(option).then((res) => {
            let data = res;
            $$.loading(false);
            if (data != null) {
                let index = this.state.pagerIndex;
                let size = this.state.pagerSize;
                this.cachedAllItems = data;
                let firstPageData = data.slice(index * size, (index + 1) * size);
                this.setState({
                    itemsCount: data.length,
                    items: firstPageData,
                });
                this.updateSelectAll(false);
                this.setBtnState([]);
                if (this.searchKey) {
                    let searchArgs = { value: this.searchKey };
                    this.onSearch(searchArgs);
                }
                if (callback) { callback(true); }
            }
        }).catch((e) => {
            $$.loading(false);
        });

    }

    getColumnInfo() {
        let commonColumn = [{
            headerTemplate: <R.Checkbox
                checked={this.isSelectAll}
                disabled={false}
                onChange={this.onSelectAllChanged.bind(this)} />,
            width: 60,
            visible: true,
        }, {
            header: ColumnHeadNames.Title,
            width: [250],
            resizeable: true,
        }, {
            header: ColumnHeadNames.CreateTime,
            width: [250],
            resizeable: true,
            visible: true,
        }, {
            header: ColumnHeadNames.Duration,
            resizeable: true,
            width: [250],
        }, {
            header: ColumnHeadNames.Comment,
            resizeable: true,
            width: [300]
        }];
        return commonColumn;
    }

    getWorkspaceColumnInfo() {
        let commonColumn = [{
            headerTemplate: <R.Checkbox
                checked={this.isSelectAll}
                disabled={false}
                onChange={this.onSelectAllChanged.bind(this)} />,
            width: 60,
            visible: true,
        }, {
            header: RMResx.RM_JS_RDM_Workspace_URL,
            width: [250],
            resizeable: true,
        }, {
            header: RMResx.RM_JS_Common_Type,
            width: [250],
            resizeable: true,
            visible: true,
        }, {
            header: RMResx.RM_JS_RDM_Hold,
            resizeable: true,
            width: [250],
        }, {
            header: RMResx.RM_JS_RDM_HoldBy,
            resizeable: true,
            width: [300]
        }];
        return commonColumn;
    }

    onSearch = (args) => {
        this.searchKey = args;
        this.cachedAllItems.forEach((value, index) => { value.isChecked = false; });
        this.updateSelectAll(false);
        let matchItems = this.cachedAllItems.filter((value, index) => { return this.state.activeTab === 1 ? value.WorkplaceUrl.match(this.searchKey) != null : value.Name.match(this.searchKey) != null});
        this.searchedItems = matchItems;
        this.selectdItems = [];
        this.setBtnState([]);
        this.onPageChange(0, this.state.pagerSize);
    }

    onStopSearch = (args) => {
        this.searchKey = "";
        this.searchedItems = [];
        this.updateSelectAll(false);
        this.cachedAllItems.forEach((value, index) => { value.isChecked = false; });
        this.selectdItems = [];
        this.setBtnState([]);
        this.onPageChange(0, this.state.pagerSize);
    }
    onPageChange(index, size, callback) {
        let allItem = [];
        if (this.searchKey == "") {
            allItem = this.cachedAllItems;
        } else {
            allItem = this.searchedItems;
        }
        let currentPageItems = allItem.slice(index * size, (index + 1) * size);
        let totalCount = allItem.length;
        let isSelectAll = false;
        if (currentPageItems && currentPageItems.length > 0) {
            isSelectAll = currentPageItems.every(r => r.isChecked);
            if (isSelectAll) {
                this.updateSelectAll(true);
            } else {
                let isAllUnchecked = currentPageItems.every(r => !r.isChecked);
                this.updateSelectAll(isAllUnchecked ? false : 'mixed');
            }
        } else {
            this.updateSelectAll(false);
        }
        this.setState({ items: currentPageItems, itemsCount: totalCount, pagerSize: size, pagerIndex: index });
        if (callback) {
            callback(true);
        }
    }

    onSelectAllChanged(checked) {
        let viewItems = this.searchKey == "" ? this.cachedAllItems : this.searchedItems;
        let currentPagerItemIds = this.state.items.map((item) => { return item.Id; });
        //this.selectdItems = checked ? viewItems : [];
        viewItems.forEach(item => {
            if (currentPagerItemIds.includes(item.Id)) {
                item.isChecked = checked;
            }
        });
        this.selectdItems = viewItems.filter((item) => { return item.isChecked; });
        this.setState({ items: this.state.items.slice() });
        this.updateSelectAll(checked);
        this.setBtnState(this.selectdItems);
    }

    onCheckChange(rowData) {
        let viewItems = this.searchKey == "" ? this.cachedAllItems : this.searchedItems;
        let checkItems = viewItems.filter(t => t.isChecked);
        let allChecked = false;
        this.selectdItems = checkItems;
        if (rowData.isChecked) {
            allChecked = this.state.items.every(item => item.isChecked);
            this.updateSelectAll(allChecked ? true : 'mixed');
        } else {
            let allUnchecked = this.state.items.every(item => !item.isChecked);
            this.updateSelectAll(allUnchecked ? false : 'mixed');
        }
        this.setBtnState(checkItems);
    }

    updateSelectAll(checked) {
        this.isSelectAll = checked;
        if (this.state.activeTab === 0) {
            let columns = this.state.tableColumns;
            columns[0] = {
                ...columns[0],
                headerTemplate: (
                    <R.Checkbox
                        checked={this.isSelectAll}
                        onChange={this.onSelectAllChanged}
                    />
                )
            };
            this.setState({
                tableColumns: columns.slice()
            });
        } else {
            let columns = this.state.workspaceColumns;
            columns[0] = {
                ...columns[0],
                headerTemplate: (
                    <R.Checkbox
                        checked={this.isSelectAll}
                        onChange={this.onSelectAllChanged}
                    />
                )
            };
            this.setState({
                workspaceColumns: columns.slice()
            });
        }
    }

    cellClick(data, action) {
        this.setState({
            phyHoldDetailFormData: data,
            detailFormPanelTitle: this.state.activeTab === 0 ? RMResx.RM_JS_BCM_Explorer_Hold_ViewDetail.format(data.Name) : "View details",
            showDetailPanel: { show: true },
        });
    }

    //reset button status
    setBtnState(items) {
        let selectOne = false;
        let selectMany = false;
        if (items.length == 1) {
            selectOne = true;
        } else if (items.length > 1) {
            selectMany = true;
        }
        let hasRelated = items.some((item, index) => (item.hasRelated));
        this.setState({
            batchSelection: selectMany,
            singleSelection: selectOne,
            hasRelatedRecord: hasRelated,
            selectedItemsCount: items.length
        });
    }

    showMessagebox = () => {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: PageI18N.OperateLimit.format(this.operateLimitCount),
            buttons: [{ text: "OK", onClick: this.hideMessageBox }]
        };
        $$.messagedialog(true, this.args);
    }

    hideMessageBox = () => {
        $$.messagedialog(false);
    }
    onNewHold() {
        this.setState({
            formPanelTitle: this.state.activeTab === 0 ? RMResx.RM_JS_RDM_Hold_Create : RMResx.RM_JS_Common_Create
        });
        this.openForm("new");
    }
    onEditHold() {
        this.setState({ formPanelTitle: this.state.activeTab === 0 ? RMResx.RM_PRM_PRE_EditHold : RMResx.RM_PRM_PRE_EditWorkspace });
        this.openForm("edit", this.selectdItems[0]);
    }
    onDeleteHold() {
        this.hideMessageBox();
        this.operationType = 2;
        this.isContextOperate = false;
        this.setState({ removeDialogShow: true, dialogTitle: { header: this.state.activeTab === 0 ? RMResx.RM_PRM_PRE_DeleteHold : RMResx.RM_JS_RDM_Workspace_Delete, title: this.state.activeTab === 0 ? RMResx.RM_JS_PRM_Hold_DeleteConfirm : RMResx.RM_JS_RDM_Workspace_DeleteConfirm } });
    }
    onReleaseHold() {
        this.hideMessageBox();
        this.operationType = 3;
        this.isContextOperate = false;
        this.setState({ removeDialogShow: true, dialogTitle: { header: RMResx.RM_JS_RDM_Hold_CancelHoldTitle, title: RMResx.RM_JS_PRM_Hold_ReleaseConfirm } });
    }
    onExportHold() {
        this.hideMessageBox();
        this.operationType = 4;
        this.isContextOperate = false;
        this.setState({ removeDialogShow: true, dialogTitle: { header: RMResx.RM_JS_RDM_Hold_ExportHoldTitle, title: RMResx.RM_JS_PRM_Hold_ExportConfirm } });
    }
    onExtendHold() {
        this.setState({ formPanelTitle: RMResx.RM_JS_RDM_Hold_SusPendHoldTitle });
        this.openForm("extend", this.selectdItems);
    }
    handleUpload(args) {
        const isSucceed = args.isSucceed;
        $$.log(isSucceed ? 'uploadSuccess:' : 'uploadError', args);
        if (isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            this.files = args.files[0];
        }
    }

    handleDelete(args) {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            this.files = null;
        }
    }
    handleHoldImport() {
        this.setState({ showImportPanel: { show: true } });
    }
    cellOperate(args, tableSelectedOption) {
        this.hideMessageBox();
        switch (tableSelectedOption.index) {
            case 1:  //Edit
                this.setState({ formPanelTitle: this.state.activeTab === 0 ? RMResx.RM_PRM_PRE_EditHold : RMResx.RM_PRM_PRE_EditWorkspace});
                this.openForm("edit", args);
                break;
            case 2:  //Delete
                this.operationType = 2;
                this.isContextOperate = true;
                this.setState({ removeDialogShow: true, dialogTitle: { header:  this.state.activeTab === 0 ? RMResx.RM_PRM_PRE_DeleteHold : RMResx.RM_JS_RDM_Workspace_Delete, title: this.state.activeTab === 0 ? RMResx.RM_JS_PRM_Hold_DeleteConfirm : RMResx.RM_JS_RDM_Workspace_DeleteConfirm }, contextManageHoldTemp: args });
                break;
            case 3: //remove
                this.operationType = 3;
                this.isContextOperate = true;
                this.setState({ removeDialogShow: true, dialogTitle: { header: RMResx.RM_JS_RDM_Hold_CancelHoldTitle, title: RMResx.RM_JS_PRM_Hold_ReleaseConfirm }, contextManageHoldTemp: args });
                break;
            case 4: //extend
                this.setState({ formPanelTitle: RMResx.RM_JS_RDM_Hold_SusPendHoldTitle });
                this.openForm("extend", [args]);
                break;
            case 5: //export
                this.operationType = 4;
                this.isContextOperate = true;
                this.setState({ removeDialogShow: true, dialogTitle: { header: RMResx.RM_JS_RDM_Hold_ExportHoldTitle, title: RMResx.RM_JS_PRM_Hold_ExportConfirm }, contextManageHoldTemp: args });
                break;
        }
    }

    onRowEvent = (args, selectedOption) => {
        let rowIndex = args.rowIndex,
            rowData = args.rowData;
        switch (args.type) {
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);  //button in row header
                break;
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            case 'checked':
                this.onCheckChange(rowData);  //check box status change in one row
                break;
            default:
                break;
        }
    }
    openForm(formType, data) {
        this.hideMessageBox();
        switch(formType){
            case "new":
                this.profileType = -1;
                break;
            case "edit":
                this.profileType = data.ProfileType;
                break;
        }

        this.dialogFormType = formType;
        let formData = {
            formType: formType,
            holdItem: data,
            holdCategory: this.holdCategory,
            profileType: this.profileType
        };
        this.setState({
            showHoldPanel: { show: true },
            phyHoldFormData: formData
        });
    }
    onSaveFormHold() {
        const formId =
            this.state.activeTab === 0 ? this.holdFormId : this.workspaceFormId;
            $$.loading(true);
                this.dispatch(formId, 'onSave', (success, data) => {
                    if (success) {
                        if (this.dialogFormType == 'extend') {
                            showToast.success(RMResx.RM_JS_PRM_Hold_ExtendSucess);
                        } else {
                            showToast.success(this.state.activeTab === 0 ? RMResx.RM_JS_PRM_Hold_CreateOrEditSucess : RMResx.RM_JS_RDM_Workspace_SaveSuccess);
                        }
                        this.setState({ showHoldPanel: { show: false } });
                        this.loadData();
                    }
                    if(!success){
                        if (data) {
                            this.state.activeTab === 1 && showToast.error(data.ErrorMessage)
                        } else {
                            $$.loading(false);
                        }
                    }
            $$.loading(false);
            });
            return false;
    }

    onDialogConfirm() {
        if (this.operationType == 2) {
            this.onDoRemove();
        }
        else if (this.operationType == 3)
        {
            this.onDoRelease();
        }
        else if (this.operationType == 4)
        {
            this.onDoExport();
        }
    }
    onDoRemove() {
        let postData = [];
        if (this.isContextOperate) {
            postData.push(this.state.contextManageHoldTemp.Id);
        } else {
            this.selectdItems.map((item, index) => {
                postData.push(item.Id);
            });
        }
        const url = this.state.activeTab === 0 ? "/api/RecordsExplorerApi/DeleteHoldAndSetting" : "/api/RecordsExplorerApi/DeleteWorkspaceHolds"
        let option = {
            url: url,
            method: "POST",
            data: postData
        };
        fetchUtility(option).then((result) => {
            this.onCancelRemove();
            if (result == "") {
                showToast.success(this.state.activeTab === 0 ? RMResx.RM_JS_PRM_Hold_DeleteSucess : RMResx.RM_JS_RDM_Workspace_DeleteSuccess);
                this.loadData();
            } else {
                showToast.error(result);
            }
        }).catch((e) => {
            this.onCancelRemove();
            showToast.error(e);
        });
    }
    onCancelRemove() {
        this.setState({ removeDialogShow: false });
    }
    onDoRelease() {
        let postData = [];
        if (this.isContextOperate) {
            postData.push(this.state.contextManageHoldTemp.Id);
        } else {
            this.selectdItems.map((item, index) => {
                postData.push(item.Id);
            });
        }
        let option = {
            url: "/api/RecordsExplorerApi/CancelHolds",
            method: "POST",
            data: postData
        };
        fetchUtility(option).then((result) => {
            this.onCancelRemove();
            if (result == "") {
                showToast.success(RMResx.RM_JS_PRM_Hold_ReleaseSucess);
                this.loadData();
            } else {
                showToast.error(result);
            }
        }).catch((e) => {
            this.onCancelRemove();
            showToast.error(e);
        });
    }

    getExportIds() {
        let postData = [];
        if (this.isContextOperate) {
            postData.push(this.state.contextManageHoldTemp.Id);
        } else {
            this.selectdItems.map((item) => {
                postData.push(item.Id);
            });
        }
        return postData;
    }

    exportLimit = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_HS_Export_DataLimit.format(limitCount),
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
        };
        $$.messagedialog(true, args);
    }


    handleExportBtn() {
        let exportIds = this.getExportIds();
        if (exportIds.length > limitCount) {
            this.exportLimit();
            return;
        }
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.onDoExport });
    }
    handleDownloadTemplate = (e) => {
        console.log("handleDownloadTemplate")
        let downloadTemplate = StringUtil.newGuid();
        var $downloadStatusKey = $("#importDownloadFlag");
        $downloadStatusKey.val(downloadTemplate);

        $("#ra-form-download")
            .attr("action", this.state.activeTab === 0 ? "/api/RecordsExplorerApi/DownloadTemplate" : "/api/RecordsExplorerApi/DownloadWorkspaceHoldTemplate")
            .submit();
    }
    handleImportSaveClick = () => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append(this.state.activeTab === 0 ? 'holdImportFile' : "workspaceHoldImportFile", this.files.file, this.files.fileName);
        const url = this.state.activeTab === 1 ? "/api/RecordsExplorerApi/ImportWorkspaceHoldData" : "/api/RecordsExplorerApi/ImportData"
        fetch(url, {
            method: 'POST',
            body: formData,
        })
            .then(function (data) {
                $$.loading(false);
                if (data) {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                }
            });
        this.setState({ showImportPanel: { show: false } });
    }
    handleImportCancelClick = () => {
        this.setState({ showImportPanel: { show: false } });
    }
    renderImportPanel() {
        return <R.Panel
            header={RMResx.RM_TM_ImportDialogTitle}
            size={670}
            status={this.state.showImportPanel}
            destroy={true}
            onShow={this.onShowImportPanel}
            onHide={this.onHideImportPanel}
        >
            {this.renderImportFromTemplate()}
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleImportCancelClick} />
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleImportSaveClick} />
        </R.Panel>;
    }
    renderImportFromTemplate() {
        const requestVerificationToken = getRequestVerificationToken();
        return (
            <div id="importSettingPanel" className='flex flex-column gap-s margin-top-m'>
                <div className='margin-block-s'>
                    <R.Validation>
                        <div ref={r => this.allValidation = r}>
                            <div className="tm-import-download">
                                <form id="ra-form-download" method="POST" action="">
                                    <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                                    <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                                </form>
                                <span className="tm-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
                            </div>
                            <div>
                                <div className="tm-import-title" tabIndex="0">
                                    <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                                </div>
                                <div>
                                    <R.Validation
                                        element="Uploader"
                                        require={RMResx.RM_JS_BCM_NoImportFile}>
                                        <R.Uploader
                                            ref={this.uploaderRef}
                                            files={this.state.files}
                                            fileTypes={["CSV"]}
                                            onUpload={this.handleUpload.bind(this)}
                                            onDelete={this.handleDelete.bind(this)}
                                            multiple={false}
                                            maxSize="10MB"
                                            showMaxSize={true}
                                            showTypes
                                        />
                                    </R.Validation>
                                </div>
                            </div>
                        </div>
                    </R.Validation>
                </div>
            </div>
        )
    }
    onDoExport = async () => {
        let postData = this.getExportIds();
        if (postData.length > limitCount) {
            this.exportLimit();
            return;
        }
        this.onCancelRemove();
        let requestOption = {
            url: "/api/RecordsExplorerApi/DownLoadReportJob",
            method: "POST",
            data: postData
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType === 0) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>);
        } else {
            showToast.error(result.ErrorMessage);
        }
    }
    expanderShown() {
        this.setState({});
    }
    renderRemoveHoldDialog() {
        return <R.Dialog
            id="removeConfirmDialog"
            header={this.state.dialogTitle.header}
            width={650}
            height={400}
            status={{ show: this.state.removeDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelRemove}
            destroy={true}
        >
            <div id="removeHoldDialog_body" className="phyhold-expander">
                <R.Expander status={this.state.showRemoveExpender} title={this.state.dialogTitle.title} onShow={this.expanderShown.bind(this)}>
                    <div className="phyhold-expander-list">
                        {!this.isContextOperate &&
                            this.selectdItems.map((item, index) => {
                                return this.state.activeTab === 1 ? (<div key={"item" + index} className="workspace-item text-overflow" data-tooltip="ifneed" data-tooltip-wrap="force" tabIndex="0">{item.WorkplaceUrl}</div>) : (<div key={"item" + index} className="phyhold-expander-item" tabIndex="0">{item.Name}</div>);
                            })
                        }
                        {this.isContextOperate &&
                            this.state.activeTab === 0 ? (<div key="onlyOne" className="phyhold-expander-item" tabIndex="0">{this.state.contextManageHoldTemp.Name}</div>) : (<div key="onlyOne" className="workspace-item text-overflow" data-tooltip="ifneed" data-tooltip-wrap="force" tabIndex="0">{this.state.contextManageHoldTemp.WorkplaceUrl}</div>)
                        }
                    </div>
                </R.Expander>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_No} onClick={this.onCancelRemove} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Yes} onClick={this.onDialogConfirm} />
            </>
        </R.Dialog>;
    }

    renderFormPanel() {
        return <R.Panel
            id="formPanel"
            header={this.state.formPanelTitle}
            size={600}
            status={this.state.showHoldPanel}
            destroy={true}
        >
            <div id="raHoldOperatePanel" className="ra-panel-content">
                {this.state.activeTab === 0 && <PhyHoldForm
                    id={this.holdFormId}
                    data={this.state.phyHoldFormData}
                >
                </PhyHoldForm>}
                {this.state.activeTab === 1 && <WorkspaceHoldForm
                    id={this.workspaceFormId}
                    data={this.state.phyHoldFormData}
                    dataSourceList={this.state.dataSourceList}/>
            }
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showHoldPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveFormHold} />
            </>
        </R.Panel>;
    }
    renderDetailFormPanel() {
        return <R.Panel
            id="formDetailPanel"
            header={RM.Encoding.htmlEncode(this.state.detailFormPanelTitle)}
            size={800}
            status={this.state.showDetailPanel}
            destroy={true}
        >
            <div id="raHoldOperatePanelDetail" className="ra-panel-content">
                {this.state.activeTab === 0 ? <PhyHoldDetailForm
                    id={this.holdDetailFormId}
                    data={this.state.phyHoldDetailFormData}
                >
                </PhyHoldDetailForm> :  <$g.DetailList labelWidth={150}>
                    <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_Workspace_Type}
                                value={this.state.phyHoldDetailFormData.SourceType}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_Workspace_URL}
                                value={this.state.phyHoldDetailFormData.WorkplaceUrl}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_OnHold}
                                value={this.state.phyHoldDetailFormData.IsHold ? RMResx.RM_JS_RDM_OnHold_Yes : RMResx.RM_JS_RDM_OnHold_No}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_HoldTitle}
                                value={this.state.phyHoldDetailFormData.HoldTitle}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_HoldBy}
                                value={this.state.phyHoldDetailFormData.HoldBy}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_JS_RDM_HoldExpiration}
                                value={this.state.phyHoldDetailFormData.ReleaseTime}
                            />
                        </$g.DetailRow>
                    </$g.DetailList>
                }
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={() => {
                this.setState({ showDetailPanel: { show: false } });
            }} />
        </R.Panel>;
    }
    renderButton() {
        return <div className='navbar-actions'>
            {
                this.state.singleSelection &&
                <React.Fragment>
                    <R.Button
                        id="raManageHoldEditBtn"
                        icon="fia-edit" text={RMResx.RM_JS_Common_Edit}
                        disabled={this.state.batchActionDisable} onClick={this.onEditHold} />
                    <R.Button
                        id="raManageHoldDeleteBtn"
                        icon="fia-delete" text={RMResx.RM_JS_BCM_Explorer_Button_Delete}
                        disabled={this.state.batchActionDisable} onClick={this.onDeleteHold.bind(this)} />
                    {
                        this.state.hasRelatedRecord && <R.Button
                            id="raManageHoldExtendBtn"
                            icon="fia-gear" text={RMResx.RM_JS_BCM_Explorer_Button_Suspend}
                            onClick={this.onExtendHold.bind(this)} />
                    }
                    {
                        this.state.hasRelatedRecord && <R.Button id="raManageHoldRemoveBtn" text={RMResx.RM_JS_BCM_Explorer_Button_Cancel} icon="fia-remove" onClick={this.onReleaseHold.bind(this)} />
                    }
                    {this.state.activeTab === 0 && <R.Button id="raManageHoldExportBtn" text={RMResx.RM_JS_BCM_Explorer_Button_Export} icon="fia-export-settings" onClick={this.handleExportBtn.bind(this)} />}
                </React.Fragment>
            }
            {
                this.state.batchSelection &&
                <React.Fragment>
                    {
                        !this.state.batchActionDisable && <R.Button
                            id="raManageHoldDeleteBtn"
                            icon="fia-delete" text={RMResx.RM_JS_BCM_Explorer_Button_Delete}
                            onClick={this.onDeleteHold.bind(this)} />
                    }
                    {
                        this.state.hasRelatedRecord && <R.Button
                            id="raManageHoldExtendBtn"
                            icon="fia-gear" text={RMResx.RM_JS_BCM_Explorer_Button_Suspend}
                            onClick={this.onExtendHold.bind(this)} />
                    }
                    {
                        this.state.hasRelatedRecord && <R.Button id="raManageHoldRemoveBtn" text={RMResx.RM_JS_BCM_Explorer_Button_Cancel} icon="fia-remove" onClick={this.onReleaseHold.bind(this)} />
                    }
                    {this.state.activeTab === 0 && <R.Button id="raManageHoldExportBtn" text={RMResx.RM_JS_BCM_Explorer_Button_Export} icon="fia-export-settings" onClick={this.handleExportBtn.bind(this)} />}
                </React.Fragment>
            }
            {
                !this.state.singleSelection && !this.state.batchSelection &&
                <R.Button text={this.state.activeTab === 0 ? RMResx.RM_PRM_PRE_New : RMResx.RM_JS_Common_Create} id="raManageHoldNewBtn" primary={true} classify="theme" onClick={this.onNewHold.bind(this)} />
            }
            {
                !this.state.singleSelection && !this.state.batchSelection &&
                <R.Button icon="fia-import" text={RMResx.RM_PRM_PRE_Hold_Records_Import} id="raManageHoldRecordsImportBtn" onClick={this.handleHoldImport.bind(this)} />
            }
        </div>;
    }

    renderSearchbox() {
        return <div className='ra-main-header'>
            <R.Searchbox
                placeholder={RMResx.RM_JS_TM_SearchTxt}
                width={380}
                disabled={false}
                onSearch={(args) => (args || "").trim() === "" ? this.onStopSearch(args) : this.onSearch(args)}
            />
        </div>;
    }

    renderActionsBar() {
        let selectedItemsCounter = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.selectedItemsCount, this.state.itemsCount);
        return <div className="ra-main-navbar">
            {this.renderButton()}
            <div className="ra-main-selected-counter">{selectedItemsCounter}</div>
        </div>;
    }

    renderSiteMap() {
        let manageHoldParentRoute = this.isPhysicalRecords ? SiteMapLinks.PRM_RecordsExplorer : SiteMapLinks.PRM_HybridSearch;
        return <$g.SiteMap data={[manageHoldParentRoute, SiteMapLinks.PRM_ManageHold]} />;
    }

    renderTable(){
        return<div className="ra-main-table">
            <R.Table
                id="table1"
                disabled={false}
                rootData={this.state.rootData}
                columns={this.state.tableColumns}
                rowTemplate={PhyHoldTableRowTemplate}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
            />
        </div>;
    }

    renderWorkspaceTable(){
        return<div className="ra-main-table">
            <R.Table
                id="table2"
                disabled={false}
                rootData={{
                    ...this.state.rootData,
                    dataSourceList: this.state.dataSourceList
                }}
                columns={this.state.workspaceColumns}
                rowTemplate={WorkspaceTableRowTemplate}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
            />
        </div>;
    }

    renderFooter(){
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.itemsCount}
                pagerIndex={this.state.pagerIndex}
                pagerSize={this.state.pagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPageChange} />
        </div>;
    }

    resetTabState() {
        this.searchKey = "";
        this.searchedItems = [];
        this.selectdItems = [];
        this.cachedAllItems = [];
        this.isSelectAll = false;
        this.updateSelectAll(false);
        this.setState({
            pagerIndex: 0,
            itemsCount: 0,
            items: [],
            selectedItemsCount: 0,
            batchSelection: false,
            singleSelection: false,
            hasRelatedRecord: false,
        });
    }

    render() {
        // let manageHoldParentRoute = this.isPhysicalRecords ? SiteMapLinks.PRM_RecordsExplorer : SiteMapLinks.BCM_RecordsExplorer;
        return (
            <div id="raManageHoldContainer">
                {/* <$g.SiteMap data={[manageHoldParentRoute, SiteMapLinks.PRM_ManageHold]}/> */}
                {this.renderSiteMap()}

                 <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                    onClose={this.hideMessageTip}
                />
                {LicenseHelper.EnableRecordsArchiver() ? (
                    <R.Tabcontrol
                        flex
                        destroy={true}
                        active={this.state.activeTab}
                        onChange={(index) => {
                            this.resetTabState();
                            this.setState(
                                { activeTab: index },
                                () => this.loadData()
                            );
                        }}
                    >
                        <R.TabPanel tab={RMResx.RM_JS_RDM_Holds} aria-label={RMResx.RM_JS_RDM_Holds}>
                            <div className="ra-page-container margin-top-l">
                                {this.renderSearchbox()}
                                {this.renderActionsBar()}
                                {this.renderTable()}
                                {this.renderFooter()}
                            </div>
                        </R.TabPanel>   

                        <R.TabPanel tab={RMResx.RM_JS_RDM_Workspace} aria-label={RMResx.RM_JS_RDM_Workspace}>
                            <div className="ra-page-container margin-top-l">
                                {this.renderSearchbox()}
                                {this.renderActionsBar()}
                                {this.renderWorkspaceTable()}
                                {this.renderFooter()}
                            </div>
                        </R.TabPanel>
                    </R.Tabcontrol>
                ) : (
                    <div className="ra-page-container">
                        {this.renderSearchbox()}
                        {this.renderActionsBar()}
                        {this.renderTable()}
                        {this.renderFooter()}
                    </div>
                )}
                
                {this.renderImportPanel()}
                {this.renderFormPanel()}
                {this.renderRemoveHoldDialog()}
                {this.renderDetailFormPanel()}
            </div>
        );
    }
}
const intervalType = {
    0: RMResx.RM_JS_ScheduleSetting_Days,
    1: RMResx.RM_JS_ScheduleSetting_Weeks,
    2: RMResx.RM_JS_RDM_Explorer_Months,
    3: RMResx.RM_JS_RDM_Explorer_Years
};
let groupItems = [
    { displayName: RMResx.RM_JS_Common_Edit, index: 1 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Delete, index: 2 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Suspend, index: 4 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Cancel, index: 3 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Export, index: 5 },
];
let groupWorkspaceItems = [
    { displayName: RMResx.RM_JS_Common_Edit, index: 1 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Delete, index: 2 },
];
class PhyHoldTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "onCellClick", "onSelectChange");
    }

    onSelectChange(item) {
        this.dispatch('cellOperate', item);
    }

    onCheckChange = (value) => {
        this.props.rowData.isChecked = value;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick() {
        this.dispatch('cellClick');
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }


    getActionBtns(rowData) {
        return <React.Fragment>
            {
                groupItems.map((item, key) => (
                    ((item.index == 3 || item.index == 4) ? rowData.hasRelated : true)&& <R.Button
                        key={key}
                        onClick={this.onSelectChange.bind(this, item)}
                        text={item.displayName} />
                ))
            }
        </React.Fragment>;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let timeZone = RM.TimeUtil.getTimezoneInfo(rowData.TimeZoneId, rowData.IsDayLightSaving);
        let duration = rowData.Type == 0 ?
            rowData.Number + " " + intervalType[rowData.Unit]
            : RM.TimeUtil.dateToStringSimplifyTimeZone(new Date(rowData.CalenderTime), timeZone);
        return (
            <Row action={this.getActionBtns(rowData)}>
                <Cell>
                    <R.Checkbox
                        id={"raPRMManageholdTableChk" + this.props.index}
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Name}>
                        <a className="ra-main-cell-link" onClick={this.onCellClick.bind(this)} tabIndex='0' onKeyDown={this.onKeyDown}>
                            {rowData.Name}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreateTime}>
                        {rowData.CreateTime}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={duration}>
                        {duration}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Description}>
                        {rowData.Description}
                    </div>
                </Cell>
            </Row>
        );
    }
}

class WorkspaceTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "onCellClick", "onSelectChange");
    }

    onSelectChange(item) {
        this.dispatch("cellOperate", item);
    }

    onCheckChange = (value) => {
        this.props.rowData.isChecked = value;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick() {
        this.dispatch("cellClick");
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    getActionBtns() {
        return <React.Fragment>
            {
                groupWorkspaceItems.map((item, key) => (
                    <R.Button
                        key={key}
                        onClick={this.onSelectChange.bind(this, item)}
                        text={item.displayName} />
                ))
            }
        </React.Fragment>;
    }


    render(Row, Cell) {
        let rowData = this.props.rowData;
        const sourceType = this.props.rootData?.dataSourceList?.find(item => item.value === rowData.SourceType)?.name;
       return (
            <Row action={this.getActionBtns()}>
                <Cell>
                    <R.Checkbox
                        id={"raPRMManageholdTableChk" + this.props.index}
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.WorkplaceUrl}
                    >
                        <a
                            className="ra-main-cell-link"
                            onClick={this.onCellClick.bind(this)}
                            tabIndex="0"
                            onKeyDown={this.onKeyDown}
                        >
                            {rowData.WorkplaceUrl}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={sourceType}
                    >
                        {sourceType}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.HoldTitle}
                    >
                        {rowData.HoldTitle}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.HoldBy}
                    >
                        {rowData.HoldBy}
                    </div>
                </Cell>
            </Row>
        );
    }
}