import SiteMapLinks from "../../../Constants/SiteMapLinks";
import GroupTable from "./Components/Table";
import SecurityGroupSettings from "./SecurityGroupSettings";
import { SourceTypeI18N, PanelDisplayMode, DefaultSecurityGroup, SecurityGroupValidateType, PermissionSettingType } from "../../../Constants/Constants";
import { LicenseHelper } from "../../../Utilities/CommonUtil";
import "../../../Less/CP/accountManagement.less";


export default class GroupManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            userList: [],
            groupItems: [],
            groupPanelTitle: '',
            groupPanelHeader: <span className="ra-panel-header">{RMResx.RM_CP_AM_Panel_Group_Header}</span>,
            isShowGroupSettingsPanel: { show: false },
            columns: this.initTableColumns(),
            spContainerItems: this.props.spContainerItems,
            exoContainerItems: this.props.exoContainerItems,
            oneDriveContainerItems: this.props.oneDriveContainerItems,
            teamsContainerItems: this.props.teamsContainerItems,
            physicalLocationItems: this.props.physicalLocationItems,
            validateGroupDialogData: [],
            validateGroupDialogShow: false,
            validateGroupType: 0,
            pagerIndex: 0,
            pagerSize: 10,
            shownCount: 0,
            totalCount: 0
        };
        this.groupId = -1;
        this.groupTableId = "groupTable";
        this.groupSettingsComponentId="raSecurityGroupSettings";
        this.searchedItems = [];
        this.showPanelMode = PanelDisplayMode.Create;
        this.groupPanelTitle = "";
        this.bind([
            "handlePageChange",
            "onCreateGroupClick",
            "cellOperate",
            "onSaveClick",
            // "showMessageTip",
            "showMsgToast",
            "hideMessageTip",
            "onDeleteGroupClick",
            "onTabChanged",
            "onSort",
            "sortGroup",
            "onPageChange",
            "onSearch",
            "cellClick",
            "onSaveBefore",
        ]);
    }

    componentInit() {

    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        this.setState({
            spContainerItems: nextProps.spContainerItems.slice(),
            exoContainerItems: nextProps.exoContainerItems.slice(),
            oneDriveContainerItems: nextProps.oneDriveContainerItems.slice(),
            teamsContainerItems: nextProps.teamsContainerItems.slice(),
            physicalLocationItems: nextProps.physicalLocationItems.slice(),
        });
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "init":
                this.initGroupData(args[0]);
                break;
        }
    }

    initTableColumns() {
        let commonColumn = [
            {
                headerTemplate: RMResx.RM_CP_AM_Table_Column_GroupName,
                width: [250],
                sortable: true,
                valuePath: "Name",
                isResizable: true,
            },
            {
                header: RMResx.RM_CP_AM_Table_Column_ScopeName,
                width: [300],
                isResizable: true,
            },
            {
                header: RMResx.RM_CP_AM_TermPermission_Title,
                width:[250],
                isResizable: true,
                visible: LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense()
            },
            {
                header: RMResx.RM_CP_AM_RulePermission_RuleTitle,
                width:[250],
                isResizable: true,
            },
            {
                header: RMResx.RM_CP_AM_Report,
                width:[300],
                isResizable: true,
            },
        ];

        if (!LicenseHelper.HasOpusSOLicenseOnly()) {
            commonColumn.push(
                {
                    header: RMResx.RM_CP_AM_ManageHolds,
                    width:[300],
                    isResizable: true,
                },
                {
                    header: RMResx.RM_CP_AM_ManageApprovalSettings,
                    width:[300],
                    isResizable: true,
                }
            );
        }

        return commonColumn;
    }

    initPanelButtons = () => {
        if (this.showPanelMode == PanelDisplayMode.View) {
            return (
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Close}
                    onClick={() => {
                        this.setState({ isShowGroupSettingsPanel: { show: false } });
                    }}
                />
            )
        } else {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={() => {
                            this.setState({ isShowGroupSettingsPanel: { show: false } });
                        }}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSaveBefore}
                    />
                </>
            )
        }
    }

    loadGroups() {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/LoadGroups",
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let groups = JSON.parse(result);
            this.initGroupScopeTypeNames(groups);
            this.setState({
                groupItems: groups
            }, ()=>{
                this.onPageChange(0, this.state.pagerSize);
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }
    
    initGroupData(data)
    {
        if(data.length > 0)
        {
            let groups = data;
            this.initGroupScopeTypeNames(groups);
            this.setState({groupItems: groups}, ()=>{
                this.onPageChange(0, this.state.pagerSize);
            });
        }else{
            this.loadGroups();
        }
    }

    initGroupScopeTypeNames(groups)
    {
        groups.map(g=>{
            let [names, types] = [[], g.ContainsSourceType];
            if(types && types.length > 0)
            {
                types.map(t=>{
                    names.push(SourceTypeI18N[t]);
                });
            }
            g.SourceTypesName = names.join("; ");                
        });
    }

    onCreateGroupClick(id) {
        this.showPanelMode = PanelDisplayMode.Create;
        this.groupPanelTitle = RMResx.RM_CP_AM_NewGroup_Title;
        this.groupId = -1;
        this.showGroupSettingsPanel();
    }

    onEditGroupClick(id) {
        this.showPanelMode = PanelDisplayMode.Edit;
        this.groupPanelTitle = RMResx.RM_CP_AM_EditGroup_Title;
        this.groupId = id;
        this.showGroupSettingsPanel();
    }

    onViewGroupClick(id) {
        this.showPanelMode = PanelDisplayMode.View;
        this.groupPanelTitle = RMResx.RM_CP_AM_ViewGroup_Title;
        this.groupId = id;
        this.showGroupSettingsPanel();
    }

    showGroupSettingsPanel() {
        this.setState({
            groupPanelTitle: this.groupPanelTitle,
            isShowGroupSettingsPanel: { show: true }
        }, () => {
            this.dispatch(this.groupSettingsComponentId, "init", this.showPanelMode);
        });
    }

    onDeleteGroupClick(id) {
        let args = {
            // classify: "warn",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_CP_AM_Confirm_DelGroup_Msg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                        $$.messagedialog(false);
                        $$.loading(true);
                        let option = {
                            url: `/api/CPApi/DeleteGroup`,
                            method: "post",
                            data: id,
                        };
                        fetchUtility(option)
                            .then((result) => {
                                $$.loading(false);
                                if (result) {
                                    this.loadGroups();
                                    // this.showMessageTip(
                                    //     "success",
                                    //     RMResx.RM_CP_AM_Success_DelGroup_Msg
                                    // );
                                    this.showMsgToast( RMResx.RM_CP_AM_Success_DelGroup_Msg,"success",true);
                                }
                            })
                            .catch((e) => {
                                $$.loading(false);
                                // this.showMessageTip(
                                //     "error",
                                //     RMResx.RM_CP_AM_Failed_DelGroup_Msg
                                // );
                                this.showMsgToast(RMResx.RM_CP_AM_Failed_DelGroup_Msg,"error",true);
                            });
                    },
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    cellOperate(args, selectedOption) {
        switch (selectedOption.index) {
            case 1: 
                this.showPanelMode = PanelDisplayMode.Edit;
                this.onEditGroupClick(args.Id);
                break;
            case 2:
                this.onDeleteGroupClick(args.Id);
                break;
            case 3:
                this.showPanelMode = PanelDisplayMode.View;
                this.onViewGroupClick(args.Id);
                break;
        }
    }

    cellClick(id) {
        if(DefaultSecurityGroup.BuiltInAdmin == id)
        {
            this.onViewGroupClick(id);
        } else {
            this.onEditGroupClick(id);
        }
    }
    
    onSort(isAsc, sortColName) {
        let items = RM.deepcopy(this.state.groupItems);
        let sortedItems = this.sortGroup(items, sortColName, isAsc);
        this.setState({
            groupItems: sortedItems
        },() => {
            this.onPageChange(0, this.state.pagerSize);
        });
    }

    sortGroup(items, sortColName, isAsc)
    {
        let [defaultAdminGroupName, defaultEndUserGroupName, defaultReviewUserGroupName, defaultHoldManagerGroupName] = [
            "RM_CP_AM_DefaultGroup_Admin_Title",
            "RM_CP_AM_DefaultGroup_EndUser_Title",
            "RM_CP_AM_DefaultGroup_ReviewUser_Title",
            "RM_CP_AM_DefaultGroup_Hold_Title"
        ];
        return items.sort((a, b) => {
            //Built-in admin first
            if (a.Name == defaultAdminGroupName) {
                return -1;
            }
            if (b.Name == defaultAdminGroupName) {
                return 1;
            }
            //Built-in enduser second
            if (a.Name == defaultEndUserGroupName) {
                return -1;
            }
            if (b.Name == defaultEndUserGroupName) {
                return 1;
            }
            //Built-in reviewuser third
            if (a.Name == defaultReviewUserGroupName) {
                return -1;
            }
            if (b.Name == defaultReviewUserGroupName) {
                return 1;
            }
            //Built-in Hold Manager fourth
            if (a.Name == defaultHoldManagerGroupName) {
                return -1;
            }
            if (b.Name == defaultHoldManagerGroupName) {
                return 1;
            }
            //other
            if (isAsc) {
                return a[sortColName] < b[sortColName] ? -1 : 1;
            } else {
                return b[sortColName] < a[sortColName] ? -1 : 1;
            }
        });
    }

    onSaveBefore() {
        let callback = (data, success) => {
            this.ValidateGroupMessageBox(data);
        };
        this.dispatch(this.groupSettingsComponentId, "onSave", callback);
        return false;
    }

    ValidateGroupMessageBox(data, validateType)
    {
        let reqGroup = {
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Users: data.Users,
            DataSourceScopeInfo: data.DataSourceScopeInfo,
            TermTreeNodeInfo: data.TermTreeNodeInfo,
            SetTermPermissionMethod: data.SetTermPermissionMethod,
            RuleTreeNodeInfo: data.RuleTreeNodeInfo,
            SetRulePermissionMethod: data.SetRulePermissionMethod,
            IsEnableTrim: data.IsEnableTrim,
            IsUseReportingPermissionControl: data.IsUseReportingPermissionControl,
            ReportingPermission: data.ReportingPermission,
            IsEnableManageHold: data.IsEnableManageHold,
            IsEnableApprovalSetting: data.IsEnableApprovalSetting,
            SecurityGroupControlType: data.SecurityGroupControlType,
            FunctionSubPermission: data.FunctionSubPermission,
        };
        let reqParam = { ValidateGroup: reqGroup, ValidateType: validateType ? validateType : 1 };

        if (data.SecurityGroupControlType === PermissionSettingType.DataScope) {
            let option = {
                url: "/api/CPApi/ValidateGroup",
                method: "POST",
                data: reqParam
            };
            $$.loading(true);
            fetchUtility(option)
                .then((result) => {
                    $$.loading(false);
                    if (result) {
                        if (result.MessageType == "1") {
                            this.processMessage(result, reqParam, data);
                        } else {
                            this.saveSecurityGroup(data);
                        }
                    }
                }
                ).catch((e) => {
                    $$.loading(false);
                });
        } else {
            this.saveSecurityGroup(data);
        }
    }

    processMessage(result, reqParam, data) {
        let sourceContainerConflictData = result.Extsion1[SecurityGroupValidateType.SourceContainerConflict];
        let termConflictData = result.Extsion1[SecurityGroupValidateType.TermConflict];
        let ruleConflictData = result.Extsion1[SecurityGroupValidateType.RuleConflict];
        let termAssociationRuleMissingData = result.Extsion1[SecurityGroupValidateType.TermAssociationRuleMissing];
        let ruleAssociationTermMissing = result.Extsion1[SecurityGroupValidateType.RuleAssociationTermMissing];
        let ruleAssociationNodeMissing = result.Extsion1[SecurityGroupValidateType.RuleAssociationNodeMissing];
        if (sourceContainerConflictData) {
            this.setState({
                validateGroupDialogData: sourceContainerConflictData,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.SourceContainerConflict
            });
        } else if (termConflictData) {
            this.setState({
                validateGroupDialogData: termConflictData,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.TermConflict
            });
        } else if (ruleConflictData) {
            this.setState({
                validateGroupDialogData: ruleConflictData,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.RuleConflict
            });
        } else if (termAssociationRuleMissingData) {
            this.setState({
                validateGroupDialogData: termAssociationRuleMissingData,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.TermAssociationRuleMissing
            });
        } else if (ruleAssociationTermMissing) {
            this.setState({
                validateGroupDialogData: ruleAssociationTermMissing,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.RuleAssociationTermMissing
            });
        } else if (ruleAssociationNodeMissing) {
            this.setState({
                validateGroupDialogData: ruleAssociationNodeMissing,
                validateGroupDialogShow: true,
                validateGroupType: SecurityGroupValidateType.RuleAssociationNodeMissing
            });
        }
    }

    validateGroupDialogContent = () => {
        let contentMsg = "";
        let expanderTitle = "";
        let itemArray = [];
        this.state.validateGroupDialogData.forEach((element) => {
            element.ConflictItems.map(item => {
                let existItem = itemArray.find(k => k.id == item.ItemId);
                if (!existItem) {
                    itemArray.push({ "id": item.ItemId, "name": item.ItemFullPath });
                }
            });
        });

        if (this.state.validateGroupType == SecurityGroupValidateType.SourceContainerConflict) {
            contentMsg = RMResx.RM_CP_AM_ContentSource;
            expanderTitle = RMResx.RM_CP_AM_Permission_ContentSource;
        } else if (this.state.validateGroupType == SecurityGroupValidateType.TermConflict) {
            contentMsg = RMResx.RM_CP_AM_TermScope;
            expanderTitle = RMResx.RM_CP_AM_Permission_TermScope;
        } else if (this.state.validateGroupType == SecurityGroupValidateType.RuleConflict) {
            contentMsg = RMResx.RM_CP_AM_RuleContainer;
            expanderTitle = RMResx.RM_CP_AM_Permission_RuleContainer;
        } else if (this.state.validateGroupType == SecurityGroupValidateType.TermAssociationRuleMissing) {
            contentMsg = RMResx.RM_CP_AM_TermAndRuleMapping;
            expanderTitle = RMResx.RM_CP_AM_Permission_RuleName;
        } else if (this.state.validateGroupType == SecurityGroupValidateType.RuleAssociationTermMissing) {
            contentMsg = RMResx.RM_CP_AM_RuleAndTermMapping;
            expanderTitle = RMResx.RM_CP_AM_Permission_TermName;
        } else if (this.state.validateGroupType == SecurityGroupValidateType.RuleAssociationNodeMissing)
        {
            contentMsg = RMResx.RM_CP_AM_RuleAndSiteMapping;
            expanderTitle = RMResx.RM_CP_AM_Permission_SourceNode;
        }

        return <div id="validateGroupDialog">
            <div className="dialog-content">
                <div className="dialog-content-left">
                    <span className="dialog-circle">
                        <span className="dialog-icon fia-status-warning"></span>
                    </span>
                </div>
                <div className="dialog-content-right">{contentMsg}</div>
            </div>
            <R.Expander status={{ show: true }} mini={true} onShow={this.expanderShown.bind(this)} title={expanderTitle}>
                <div className="dialog-expander-list">
                    {itemArray.map(e => {
                        return <div key={e.id} className="dialog-expander-list-div" data-tooltip="diffneed" aria-label={e.name}>{e.name}</div>;
                    })}
                </div>
            </R.Expander>
        </div>;
    }

    onCloseValidateGroupDialog = () => {
        this.setState({ validateGroupDialogShow: false });
    }

    expanderShown() {
        this.setState({});
    }

    saveSecurityGroup(data)
    {
        let [isEdit, reqUrl, successMsg, failedMsg] = [data.Id > -1, "/api/CPApi/CreateGroup", RMResx.RM_CP_AM_Success_NewGroup_Msg, RMResx.RM_CP_AM_Failed_NewGroup_Msg];
        if(isEdit)
        {
            reqUrl = "/api/CPApi/EditGroup";
            successMsg = RMResx.RM_CP_AM_Success_EditGroup_Msg;
            failedMsg = RMResx.RM_CP_AM_Failed_EditGroup_Msg;
        }
        let reqParam = {
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Users: data.Users,
            DataSourceScopeInfo: data.DataSourceScopeInfo,
            TermTreeNodeInfo: data.TermTreeNodeInfo,
            SetTermPermissionMethod: data.SetTermPermissionMethod,
            RuleTreeNodeInfo: data.RuleTreeNodeInfo,
            SetRulePermissionMethod: data.SetRulePermissionMethod,
            IsEnableTrim: data.IsEnableTrim,
            IsUseReportingPermissionControl: data.IsUseReportingPermissionControl,
            ReportingPermission: data.ReportingPermission,
            IsEnableManageHold: data.IsEnableManageHold,
            IsEnableApprovalSetting: data.IsEnableApprovalSetting,
            SecurityGroupControlType: data.SecurityGroupControlType,
            FunctionSubPermission: data.FunctionSubPermission,
        };
        
        // return;
        let option = {
            url: reqUrl,
            method: "POST",
            data: reqParam
        };
        $$.loading(true);
        fetchUtility(option)
            .then((result) => {
                $$.loading(false);
                if (result) {
                    if (result.MessageType == "0") {
                        this.setState(
                            { isShowGroupSettingsPanel: { show: false } },
                            () => {
                                // this.showMessageTip("success", successMsg);
                                this.showMsgToast(successMsg,"success",true);
                                this.loadGroups();
                            }
                        );
                    } else {
                        if (result.FaildType == 1) {
                            failedMsg =
                                RMResx.RM_CP_AM_Group_SameName_Error_Msg;
                        }
                        this.dispatch(
                            this.groupSettingsComponentId,
                            "failedSave",
                            result.ErrorMessage || failedMsg
                        );
                    }
                }
            }
            ).catch((e) => {
                $$.loading(false);
            });
    }

    onPageChange(pageIndex, pageSize, callback) {
        let items = this.searchKey? this.searchedItems : RM.deepcopy(this.state.groupItems);
        let currentPageItems = JSON.parse(JSON.stringify(items.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        this.dispatch(this.groupTableId, currentPageItems, this.state.columns);
        this.setState({
            pagerIndex: pageIndex,
            pagerSize: pageSize,
            shownCount: currentPageItems.length,
            totalCount: items.length
        });
        if (callback) {
            callback(true);
        }
    }

    onSearch = (args) => {
        let key = $.trim(args);
        if(key){
            this.searchKey = key;
            let allItems = RM.deepcopy(this.state.groupItems);
            this.searchedItems = allItems.filter(o => this.wrapperI18N(o.Name).toLowerCase().indexOf(key.toLowerCase()) > -1);
            this.onPageChange(0, this.state.pagerSize);
        } else {
            this.searchKey = "";
            this.onPageChange(0, this.state.pagerSize);
        }
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }
    showMsgToast (content, type){
        let option = {
            content: content,
            classify: type
        };
        $$.toast(option);
    }

    hideMessageTip() {
        this.setState({
            showTip: false,
        });
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    renderSiteMap() {
        return <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_AccountManagement]}/>;
    }

    renderHeaderFilter() {
        return (
            <div className="ra-main-header">
                <div className="navbar-search">
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                        width={380}
                    />
                </div>
               
            </div>
        );
    }
    renderNavBarAction(){
        return(
            <div className="ra-main-navbar">
                <div className="navbar-actions">
                    {
                        <R.Button
                            id="raCpAmSecurityGroupCreateBtn"
                            text={RMResx.RM_CP_AM_NewGroup_Title}
                            primary={true}
                            classify="theme"
                            onClick={this.onCreateGroupClick}
                        />
                    }
                </div>
            </div>
        );
    }
    renderCreateGroupPanel() {
        return <R.Panel
            id="raSecurityGroupCreate"
            header={this.state.groupPanelTitle}
            size={670}
            status={this.state.isShowGroupSettingsPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span>{this.state.groupPanelHeader}</span>
            </div>
            <div>
                <SecurityGroupSettings
                    id={this.groupSettingsComponentId}
                    groupId={this.groupId}
                    groupItems={this.state.groupItems}
                    spContainerItems={this.state.spContainerItems}
                    exoContainerItems={this.state.exoContainerItems}
                    oneDriveContainerItems={this.state.oneDriveContainerItems}
                    teamsContainerItems={this.state.teamsContainerItems}
                    physicalLocationItems={this.state.physicalLocationItems}
                >
                </SecurityGroupSettings>
            </div>
            {this.initPanelButtons()}
        </R.Panel>;
    }

    renderPager() {
        return (
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={this.state.totalCount}
                    pagerIndex={this.state.pagerIndex}
                    pagerSize={this.state.pagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onPageChange}
                />
            </div>
            // </div>
        );
    }

    renderValidateGroupDialog() {
        return <R.Dialog
            id="validateGroupDialog"
            header={RMResx.RM_CP_AM_Permission_WarningDialogTitle}
            width={464}
            status={{ show: this.state.validateGroupDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCloseValidateGroupDialog}
            destroy={true}
        >
            {this.validateGroupDialogContent()}
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onCloseValidateGroupDialog} />
        </R.Dialog>;
    }

    render() {
        return (
            <div id="raGroupManagement">
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                    onClose={this.hideMessageTip}
                />
                <div id="rm_control">{this.renderHeaderFilter()}</div>
                {this.renderNavBarAction()}
                <div className="ra-main-table">
                    <GroupTable
                        id="groupTable"
                        cellOperate={this.cellOperate}
                        cellClick={this.cellClick}
                        onSort={this.onSort}
                    />
                </div>
                {this.renderPager()}
                {this.renderCreateGroupPanel()}
                {this.renderValidateGroupDialog()}
            </div>
        );
    }
}