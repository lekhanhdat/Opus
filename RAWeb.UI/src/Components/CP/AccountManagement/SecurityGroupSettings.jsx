import ScopeTable from "./Components/ScopeTable";
import ManagePhysicalPermissionTable from "./Components/ManagePhysicalPermissionTable";
import ConfigPhysicalPermissionTable from "./Components/ConfigPhysicalPermissionTable";
import ConfigScope from "./ConfigScope";
import TermPermissionSettings from "./TermPermissionSettings";
import PeoplePicker from "../../../Components/Common/PeoplePicker";
import { DefaultSecurityGroupId, SourceFlags, PanelDisplayMode, SetTermPermissionMethod, PermissionManageModule, SubPermission, PhyUserRoleType, DefaultSecurityGroup, RulePermissionMethod, RestoreCenterType, PermissionSettingType } from "../../../Constants/Constants";
import { checkPermission } from '../../../Utilities/permissionManager';
import RouterUrls from '../../../Constants/RouterUrls';
import RulePermissionSettings from "./RulePermissionSettings";
import { LicenseHelper } from '../../../Utilities/CommonUtil';
import { getPermissionReportList } from "./Constants";

export default class SecurityGroupSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.isChangedReportingPermission = true;
        this.groupId = this.props.groupId || -1;
        this.cacheItems = [];
        this.scopeColumns = this.initScopeTabColumns();
        this.permissionColumns = this.initPermissionTabColumns();
        this.managePhyPermissionColumns = this.initManagePhyPermissionColumns();
        this.phyPermissionColumns = this.initManagePhyPermissionColumns();
        this.scopeTableId = "raScopeTable";
        this.permissionTableId = "raPermissionTable";
        this.configScopeCopId = "configScopeContainers";
        this.termPermissionCopId = "termPermissionContainer";
        this.rulePermissionCopId = "rulePermissionContainer";
        this.managePhyPermissionTableId = "raManagePhyPermissionTable";
        this.configPhyPermissionCopId = "configPhyPermission";
        this.showFS = true;
        this.showPhysical = true;
        this.showSPOnPrem = true;
        this.showAzureFile = true;
        this.showBox = true;
        this.showGoogle = true;
        this.hasRecordsLicense = LicenseHelper.HasOpusILLicense();
        this.hasGoogleLicense = LicenseHelper.HasOpusGoogleLicense();
        this.showSPOD = LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusSOLicense();
        this.state = {
            isSaving: false,
            showTip: false,
            showMessageTip: this.showMessageTip,
            haveChange: false,
            groupName: '',
            groupDesc: '',
            scopeItems: [],
            phyModuleItems: this.initPhyModuleItems(),
            groupNameValidate: false,
            scopeValidate: false,
            scopeValidateMsg: "",
            showContainerPanel: { show: false },
            showPhyPermissionsPanel: { show: false },
            configScopePanelTitle: '',
            searchedUser: [],
            isPhysicalSelected: false,
            isGoogleSelected: false,
            selectedValue: "1",
            pageComponents: {
                groupNameDisabled: false,
                groupDescDisabled: false,
                groupMembersDisabled: false
            },
            nameDisabledStatus: false,
            descDisabledStatus: false,
            usersDisabledStatus: false,
            termObjItems: [],
            enableTrim: false,
            trimCheckBoxIdDisable : this.getBuiltInAdminGroupDisablesStatus(),
            showTrimButton: false,
            isBuiltInGroup: false,
            selectedPermissionSettingValue: PermissionSettingType.DataScope,
            restoreCenterValue: RestoreCenterType.None,
            restoreCenterValidate: false,
            enablePermissionReport: true,
            permissionReportList: getPermissionReportList(),
            selectedPermissionReportValue: getPermissionReportList().reduce((acc, curr) => acc + curr.value, 0),
            enableManageHoldsPermission: false,
            enableManageApprovalSettingsPermission: false,
        };
        this.bind('onCheckChanged', 'onScopeCellClick', 'onSearchUser', 'onSaveContainerClick', 'onUserRoleCheckChanged', 'onSavePermissionClick', 'onPermissionCheckChanged');
    }

    componentInit() {
        this.onCheckPermission();
        if (this.groupId > -1) {
            this.loadGroupSettings();
        }
        this.initScopeItems([]);
    }

    componentReceive(action, ...args) {
        let termSettings, ruleSettings;
        switch (action) {
            case "onSave":
                this.dispatch(this.termPermissionCopId, "save", (data) => { termSettings = data; });
                this.dispatch(this.rulePermissionCopId, "save", (data) => { ruleSettings = data; });
                this.saveGroupBefore(args[0], termSettings, ruleSettings);
                break;
            case "init":
                this.showPanelMode = args[0];
                // this.setPageComponentsStatus();
                if (this.groupId > -1) {
                    this.dispatch(this.scopeTableId, this.state.scopeItems, this.scopeColumns, this.getDisabledStatus());
                } else {
                    this.loadAssignContainerIds(ass => {
                        this.dispatch(this.scopeTableId, this.getAvailableScope(ass), this.scopeColumns, this.getDisabledStatus());
                        this.initScopeItems(ass);
                    });
                }
                break;
            case "failedSave":
                this.showMessageTip("error", args[0]);
                break;
        }
    }

    getAvailableScope(assignContainerIds) {
        return RM.deepcopy(this.state.scopeItems).map(s => {
            s.Containers = s.Containers.filter(c => {
                return assignContainerIds.indexOf(c.Id) == -1;
            });
            return s;
        });
    }

    getValidateSourceTypeFunc() {
        return {
            containsFS: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.FS);
            },
            containsPhy: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.Phy) && group.PhysicalRole == PhyUserRoleType.EndUser;
            },
            containsSPLocal: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.SPLocal);
            },
            containsAzureFile: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.AzureFile);
            },
            containsBox: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.Box);
            },
            containsGoogle: (group) => {
                return group.ContainsSourceType?.some(o => o == SourceFlags.Google);
            }
        };
    }

    getRestoreCenterItems() {
        let options = [
            // { text: RMResx.RM_JS_Common_None, value: RestoreCenterType.None },
            { text: RMResx.RM_CP_AM_SubPermission_FullControl, value: RestoreCenterType.FullControl },
            { text: RMResx.RM_CP_AM_SubPermission_SearchAndExport, value: RestoreCenterType.SearchAndExport },
            { text: RMResx.RM_CP_AM_SubPermission_SearchOnly, value: RestoreCenterType.SearchOnly },
        ];

        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.restoreCenterValue == op.value;
            return op;
        });
    }
    
    onCheckPermission() {
        const [allGroups, isEditMode] = [this.props.groupItems, this.groupId > -1];
        if(this.hasRecordsLicense || this.hasGoogleLicense)
        {
            const customGroups = allGroups.filter(item => !this.isDefaultGroup(item.Id));
            const validate = this.getValidateSourceTypeFunc();
            customGroups.forEach(g => {
                if(validate.containsFS(g)) {
                    this.showFS = false;
                }
                if(validate.containsSPLocal(g)) {
                    this.showSPOnPrem = false;
                }
                if(validate.containsAzureFile(g)) {
                    this.showAzureFile = false;
                }
                if(validate.containsBox(g)) {
                    this.showBox = false;
                }
                if(validate.containsGoogle(g)) {
                    this.showGoogle = false;
                }
            });
    
            if(isEditMode) 
            {
                const currentGroup = allGroups.find(o => o.Id == this.groupId);
                if(validate.containsFS(currentGroup)) {
                    this.showFS = true;
                }
                if(validate.containsPhy(currentGroup)) {
                    this.showPhysical = false;
                }
                if(validate.containsSPLocal(currentGroup)) {
                    this.showSPOnPrem = true;
                }
                if(validate.containsAzureFile(currentGroup)) {
                    this.showAzureFile = true;
                }
                if(validate.containsBox(currentGroup)) {
                    this.showBox = true;
                }
                if(validate.containsGoogle(currentGroup)) {
                    this.showGoogle = true;
                }
            }
        }
        
        this.setState({ selectedValue: "2" });
    }

    loadGroupSettings() {
        $$.loading(true);
        let option = {
            url: `/api/CPApi/LoadGroup?id=${this.groupId}`,
            method: "GET"
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if(result)
            {
                this.setGroupData(JSON.parse(result));
                this.cacheGroupData = {
                    ...JSON.parse(result),
                    IsEnableManageHold: false,
                    IsEnableApprovalSetting: false,
                };
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }
    loadAssignContainerIds(callback) {
        $$.loading(true);
        let option = {
            url: `/api/CPApi/LoadAssignContainerIds`,
            method: "GET"
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            callback(JSON.parse(result));
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setPageComponentsStatus()
    {
        if(this.isViewModel())
        {
            this.setComponentStatusOfViewModel();
        }

        if(this.isBuiltInEndUserGroup() || this.isBuiltInReviewUserGroup())
        {
            this.setComponentStatusOfEndUserGroup();
        }
    }

    setComponentStatusOfViewModel()
    {
        this.setState({
            pageComponents: {
                groupNameDisabled: true,
                groupDescDisabled: true,
                groupMembersDisabled: true
            }
        });
    }

    setComponentStatusOfEndUserGroup()
    {
        this.setState({
            pageComponents: {
                groupNameDisabled: true,
                groupDescDisabled: true,
                groupMembersDisabled: false
            }
        });
    }

    isViewModel() {
        return this.showPanelMode == PanelDisplayMode.View;
    }

    isBuiltInEndUserGroup() {
        return this.groupId == DefaultSecurityGroup.BuiltInEndUser;
    }

    isBuiltInAdminGroup() {
        return this.groupId == DefaultSecurityGroup.BuiltInAdmin;
    }

    isBuiltInReviewUserGroup() {
        if(this.state.isBuiltInGroup && !this.isBuiltInEndUserGroup() && !this.isBuiltInAdminGroup())
        {
            return true;
        }
        return false;
    }

    setGroupData(data, callback) {
        var items = RM.deepcopy(this.state.scopeItems);
        const permissionReportList = getPermissionReportList();
        const updatedPermissionReportList = permissionReportList.map((item) => ({
            ...item,
            checked: (data.ReportingPermission & item.value) !== 0,
        }))
        const filteredPermissionReportList = updatedPermissionReportList.filter((item) => item.checked);
        this.setUsersInfo(data.Users);
        this.setScopeInfo(data, items);
        this.dispatch(this.scopeTableId, items, this.scopeColumns, this.getDisabledStatus());
        this.setState({
            groupName: this.wrapperI18N(data.Name),
            groupDesc: this.wrapperI18N(data.Description),
            searchedUser: data.Users,
            selectedPermissionSettingValue: data.SecurityGroupControlType,
            restoreCenterValue: data.FunctionSubPermission,
            scopeItems: items,
            enableTrim: data.IsEnableTrim,
            enablePermissionReport: filteredPermissionReportList?.length > 0 && filteredPermissionReportList.length !== permissionReportList.length ? 'mixed' : data.IsUseReportingPermissionControl,
            permissionReportList: updatedPermissionReportList,
            selectedPermissionReportValue: data.ReportingPermission,
            enableManageHoldsPermission: data.IsEnableManageHold,
            enableManageApprovalSettingsPermission: data.IsEnableApprovalSetting,
            isBuiltInGroup: data.IsBuiltInGroup,
        }, () => {
            const filteredScopeItems = this.state.scopeItems.filter(o => o.isChecked);
            const isOnlyPhysicalSelected = filteredScopeItems.length === 1 && filteredScopeItems[0].SourceType === SourceFlags.Phy;
            const isSelectedEndUserValue = this.state.selectedValue == PhyUserRoleType.EndUser;
            if (isOnlyPhysicalSelected && isSelectedEndUserValue) {
                this.isChangedReportingPermission = false;
            }
            this.dispatch(this.termPermissionCopId, "edit", data.TermTreeNodeInfo);
            this.dispatch(this.rulePermissionCopId, "edit", data.RuleTreeNodeInfo);
            this.setSelectedAdmin();
            this.setPageComponentsStatus();
            if (callback) {
                callback();
            }
        });
    }

    setUsersInfo(users)
    {
        users.map((item) => { item.Checked = true; });
    }

    setScopeInfo(data, items)
    {
        let scopesInfo = data.DataSourceScopeInfo;
        let availableScopesInfo = data.AvailableDataSourceScopeInfo;
        items.map((item) => {
            var selectedItem = scopesInfo.find(o => o.DataSourceType == item.SourceType);
            item.isChecked = selectedItem !== undefined ? true : false;
            this.setContainersInfo(scopesInfo, item, availableScopesInfo);
            this.setSubPermissionInfo(selectedItem);
        });
        if(data.Id == 1)
        {
            let dataSourceItems = items.filter(o => o.SourceType == SourceFlags.SP || o.SourceType == SourceFlags.Exo || o.SourceType == SourceFlags.OneDrive || o.SourceType == SourceFlags.Teams);
            dataSourceItems.map(o=> {
                o.ContainerNames = RMResx.RM_CP_AM_AllScope_Title;
            });
        }
    }

    setContainersInfo(scopesInfo, item, availableScopesInfo)
    {
        let itemSource = item.SourceType;
        switch(itemSource)
        {
            case SourceFlags.SP:
            case SourceFlags.Exo:
            case SourceFlags.OneDrive:
            case SourceFlags.Teams:
                this.setScopeContainerInfo(scopesInfo, item, availableScopesInfo);
                break;
            case SourceFlags.Phy:
                var phyScopeInfo = scopesInfo.find(o => o.DataSourceType == SourceFlags.Phy);
                if(phyScopeInfo != undefined)
                {
                    item.SubPermission = phyScopeInfo.SubPermission;
                    this.setState({
                        isPhysicalSelected: true,
                        selectedValue: item.SubPermission
                    });
                }
                this.setScopeContainerInfo(scopesInfo, item, availableScopesInfo);
                break;
            case SourceFlags.FS:
            case SourceFlags.SPLocal:
            case SourceFlags.AzureFile:
            case SourceFlags.Box:
            case SourceFlags.Google:
                this.setState({
                    isGoogleSelected: true
                });
                break;
            default:
                break;
        }
    }

    getDisabledStatus() {
        return this.showPanelMode == PanelDisplayMode.View || this.groupId == DefaultSecurityGroup.BuiltInEndUser ? true : false;
    }

    setScopeContainerInfo(scopesInfo, item, availableScopesInfo)
    {
        let scopeInfo = scopesInfo.find(o => o.DataSourceType == item.SourceType);
        let isSPSource = item.SourceType == SourceFlags.SP;
        let isExoSource = item.SourceType == SourceFlags.Exo;
        let isOneDriveSource = item.SourceType == SourceFlags.OneDrive;
        let isTeamsSource = item.SourceType == SourceFlags.Teams;
        let isPhysicalSource = item.SourceType == SourceFlags.Phy;
        if(scopeInfo !== undefined)
        {
            let containerItems = [];
            if(isSPSource)
            {
                containerItems = RM.deepcopy(this.props.spContainerItems);
            }
            if(isExoSource)
            {
                containerItems = RM.deepcopy(this.props.exoContainerItems);
            }
            if(isOneDriveSource)
            {
                containerItems = RM.deepcopy(this.props.oneDriveContainerItems);
            }
            if (isTeamsSource) {
                containerItems = RM.deepcopy(this.props.teamsContainerItems);
            }
            if (isPhysicalSource) {
                containerItems = RM.deepcopy(this.props.physicalLocationItems);
            }
            
            if(scopeInfo.ScopeIds && scopeInfo.ScopeIds.length > 0)
            {
                var selectedContainerItems = containerItems.filter(o => scopeInfo.ScopeIds.indexOf(o.Id) > -1);
                if(selectedContainerItems && selectedContainerItems.length > 0)
                {
                    item.ContainerNames = selectedContainerItems.map((item) => { return item.Name; }).join("; ");
                }
                containerItems.map((sp) => {
                    if(scopeInfo.ScopeIds.indexOf(sp.Id) > -1)
                    {
                        sp.isChecked = true;
                    } else {
                        sp.isChecked = false;
                    }
                });
            }
            let availableContainers = [];
            if (isSPSource) {
                availableContainers = availableScopesInfo.SPContainerItems.map(c => c.Id);
            }
            if (isExoSource) {
                availableContainers = availableScopesInfo.EXOContainerItems.map(c => c.Id);
            }
            if (isOneDriveSource) {
                availableContainers = availableScopesInfo.OneDriveContainerItems.map(c => c.Id);
            }
            if (isTeamsSource) {
                availableContainers = availableScopesInfo.TeamsContainerItems.map(c => c.Id);
            }
            if (!isPhysicalSource) { 
                let selectedOrAvailableContainerItems = [];
                containerItems.map(sp => {
                    if (sp.isChecked || availableContainers.indexOf(sp.Id) > -1) {
                        selectedOrAvailableContainerItems.push(sp);
                    }
                });
                item.Containers = selectedOrAvailableContainerItems;
            } else {
                item.Containers = containerItems.filter((item) => availableScopesInfo.PhysicalLocationItems.some(c => c.Id == item.Id) || (scopeInfo.ScopeIds || []).includes(item.Id));
            }
        } else {
            if (item.SourceType == SourceFlags.SP) {
                item.Containers = availableScopesInfo.SPContainerItems;
            }
            if (item.SourceType == SourceFlags.Exo) {
                item.Containers = availableScopesInfo.EXOContainerItems;
            }
            if (item.SourceType == SourceFlags.OneDrive) {
                item.Containers = availableScopesInfo.OneDriveContainerItems;
            }
            if (item.SourceType == SourceFlags.Teams) {
                item.Containers = availableScopesInfo.TeamsContainerItems;
            }
        }
    }

    initScopeTabColumns() {
        return [
            {
                header: RMResx.RM_CP_AM_Table_Column_WorkSpaceName,
                width: 200,
                // align: 'center'
            },
            {
                header: RMResx.RM_CP_AM_Table_Column_PermissionScopeName,
                width: 270,
                // align: 'center'
            }, {
                header: "",//Action
                width: 70,
                // align: 'center'
            }];
    }

    initScopeItems(assignContainerIds)
    {
        let spAllContainers = RM.deepcopy(this.props.spContainerItems?.filter(c => assignContainerIds.indexOf(c.Id) == -1));
        let exoAllContainers = RM.deepcopy(this.props.exoContainerItems?.filter(c => assignContainerIds.indexOf(c.Id) == -1));
        let oneDriveAllContainers = RM.deepcopy(this.props.oneDriveContainerItems?.filter(c => assignContainerIds.indexOf(c.Id) == -1));
        let teamsAllContainers = RM.deepcopy(this.props.teamsContainerItems?.filter(c => assignContainerIds.indexOf(c.Id) == -1));
        let physicalLocationItems = RM.deepcopy(this.props.physicalLocationItems?.filter(c => assignContainerIds.indexOf(c.Id) == -1));
        let scopeItems = [
            //{ Id: 1, SourceType: SourceFlags.SP, Name: RMResx.RM_JS_SPS_TabLabel_SP, Containers: spAllContainers, ContainerNames: "", isChecked: false,disabled:false},
            //{ Id: 6, SourceType:SourceFlags.OneDrive, Name: RMResx.RM_JS_SPS_TabLabel_OneDrive, Containers: oneDriveAllContainers, ContainerNames: "", isChecked: false,disabled:false},
            // { Id: 2, SourceType: SourceFlags.Exo, Name: RMResx.RM_JS_SPS_TabLabel_EXO, Containers: exoAllContainers, ContainerNames: "", isChecked: false,disabled:false },
            // { Id: 3, SourceType: SourceFlags.Phy, Name: RMResx.RM_JS_SPS_TabLabel_Physical, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false ,disabled:false}
        ];
        if (this.showSPOD) {
            if (RM.gData.hasUpgradeTeams) {
                scopeItems.push({ Id: 11, SourceType: SourceFlags.Teams, Name: RMResx.RM_JS_SPS_TabLabel_Teams, Containers: teamsAllContainers, ContainerNames: "", isChecked: false, disabled: false });
            }
            scopeItems.push({ Id: 1, SourceType: SourceFlags.SP, Name: RMResx.RM_JS_SPS_TabLabel_SP, Containers: spAllContainers, ContainerNames: "", isChecked: false, disabled: false });
            scopeItems.push({ Id: 6, SourceType: SourceFlags.OneDrive, Name: RMResx.RM_JS_SPS_TabLabel_OneDrive, Containers: oneDriveAllContainers, ContainerNames: "", isChecked: false, disabled: false });
        }
        if(this.hasRecordsLicense)
        {
            scopeItems.push({ Id: 2, SourceType: SourceFlags.Exo, Name: RMResx.RM_JS_SPS_TabLabel_EXO, Containers: exoAllContainers, ContainerNames: "", isChecked: false,disabled:false });
        }
        if(this.hasRecordsLicense || this.hasGoogleLicense)
        {
            scopeItems.push({ Id: 3, SourceType: SourceFlags.Phy, Name: RMResx.RM_JS_SPS_TabLabel_Physical, Containers: physicalLocationItems, ContainerNames: "", isChecked: false ,disabled:false});
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_FS", RM.UserResources)) {
            if (this.showFS) {
                scopeItems.push({ Id: 4, SourceType: SourceFlags.FS, Name: RMResx.RM_JS_SPS_TabLabel_FS, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false ,disabled:false});
            }
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_LSP", RM.UserResources)) {
            if (this.showSPOnPrem) {
                scopeItems.push({ Id: 5, SourceType: SourceFlags.SPLocal, Name: RMResx.RM_JS_SPS_TabLabel_SPLocal, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false,disabled:false });
            }
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_AzureFile", RM.UserResources)) {
            if (this.showAzureFile) {
                scopeItems.push({ Id: 7, SourceType: SourceFlags.AzureFile, Name: RMResx.RM_JS_SPS_TabLabel_AZS, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false ,disabled:false});
            }
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Box", RM.UserResources)) {
            if (this.showBox) {
                scopeItems.push({ Id: 8, SourceType: SourceFlags.Box, Name: RMResx.RM_JS_SPS_TabLabel_Box, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false ,disabled: false });
            }
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Google", RM.UserResources)) {
            if (this.showGoogle) {
                scopeItems.push({ Id: 9, SourceType: SourceFlags.Google, Name: RMResx.RM_JS_SPS_TabLabel_GoogleDrive, Containers: [], ContainerNames: RMResx.RM_CP_AM_AllScope_Title, isChecked: false ,disabled: false });
            }
        }
        this.setState({scopeItems: scopeItems});
    }

    initPhyModuleItems()
    {
        return [
            { Id: 1, Type: PermissionManageModule.PhysicalExplorer, Name: RMResx.RM_CP_AM_Module_PhyExplorer, PermissionItems: this.initPermissionItems(PermissionManageModule.PhysicalExplorer), PermissionNames: "" }
        ];
    }

    initPermissionItems(moduleType)
    {
        let items = [];
        switch(moduleType)
        {
            case PermissionManageModule.PhysicalExplorer:
                items = [
                    { Id: 1, Type: SubPermission.SetAccessControl, Name: RMResx.RM_CP_AM_Phy_SubPermission_SetAccessControl, isChecked: false },
                    { Id: 2, Type: SubPermission.FolderCreationRequest, Name: RMResx.RM_CP_AM_Phy_SubPermission_FolderCreationRequest, isChecked: false },
                    { Id: 3, Type: SubPermission.FolderLoanRequest, Name: RMResx.RM_CP_AM_Phy_SubPermission_FolderLoanRequest, isChecked: false },
                    { Id: 4, Type: SubPermission.BoxCreationRequest, Name: RMResx.RM_CP_AM_Phy_SubPermission_BoxCreationRequest, isChecked: false },
                    { Id: 5, Type: SubPermission.FolderLoanReturn, Name: RMResx.RM_CP_AM_Phy_SubPermission_FolderLoanReturn, isChecked: false },
                    { Id: 6, Type: SubPermission.SubmitMoveRequest, Name: RMResx.RM_CP_AM_Phy_SubPermission_SubmitMoveRequest, isChecked: false },
                ];
                break;
            default:
                break;
        }
        return items;
    }

    setSubPermissionInfo(dbScopeInfo)
    {
        if(dbScopeInfo && dbScopeInfo.DataSourceType == SourceFlags.Phy && dbScopeInfo.SubPermission == 2)
        {
            let selectedPermissions = dbScopeInfo.SubPermissions;
            let moduleItems = RM.deepcopy(this.state.phyModuleItems);
            let phyExplorerModule = moduleItems.find(o => o.Type == PermissionManageModule.PhysicalExplorer);
            if(phyExplorerModule)
            {
                phyExplorerModule.PermissionItems.map(o => {
                    o.isChecked = selectedPermissions.indexOf(o.Type) > -1 ? true : false;
                });
                let selectedItems = phyExplorerModule.PermissionItems.filter(o => o.isChecked);
                phyExplorerModule.PermissionNames = selectedItems.map((item) => { return item.Name; }).join("; ");
                this.setState({
                    phyModuleItems: moduleItems
                }, () => {
                    this.dispatch(this.managePhyPermissionTableId, this.state.phyModuleItems, this.managePhyPermissionColumns, this.showPanelMode == PanelDisplayMode.View);
                });
            }
        }
    }

    initPermissionTabColumns() {
        return [{
            header: RMResx.RM_CP_AM_Table_Column_PermissionName,
            width: 240
        }];
    }

    initManagePhyPermissionColumns() {
        return [
            {
                header: "",
                width: 70
            },
            {
                header: RMResx.RM_CP_AM_ManagePermission_Module_Title,
                width: 230
            }, {
                header: RMResx.RM_CP_AM_ManagePermission_Permission_Title,
                width: 290
            }];
    }

    initPhyPermissionColumns() {
        return [{
            header: RMResx.RM_CP_AM_ManagePermission_Permission_Title,
            width: 240
        }];
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = () => {
        this.setState({
            showTip: false
        });
    }

    getScopeContainersInfo()
    {
        let scopeContainerInfo = [];
        var selectedScopes = this.state.scopeItems.filter(o => o.isChecked);
        selectedScopes.forEach((item) => {
            let selectedContainers = item.Containers.filter(o => o.isChecked);
            let scopeInfo = {
                DataSourceType: item.SourceType,
                ScopeIds: selectedContainers.map(o => { return o.Id; }),
                SubPermission: item.SourceType == SourceFlags.Phy ? this.state.selectedValue : PhyUserRoleType.None
            };
            if(item.SourceType == SourceFlags.Phy && this.state.selectedValue == PhyUserRoleType.EndUser)
            {
                scopeInfo.SubPermissions = this.getSelectedSubPermissions();
            }
            scopeContainerInfo.push(scopeInfo);
        });
        return scopeContainerInfo;
    }

    isDefaultGroup(id)
    {
        return DefaultSecurityGroupId.indexOf(id) > -1;
    }

    getBuiltInAdminGroupDisablesStatus()
    {
        return (DefaultSecurityGroup.BuiltInAdmin == this.groupId || DefaultSecurityGroup.BuiltInEndUser == this.groupId);
    }

    setComponentDisabledStatus(groupId)
    {
        let isDisabled = this.isDefaultGroup(groupId) ? true : false;
        // this.setState({
        //     nameDisabledStatus: isDisabled,
        //     descDisabledStatus: isDisabled,
        //     usersDisabledStatus: isDisabled,
        // });
    }

    onChangeGroupName = (value) => {
        this.setState({
            groupName: $.trim(value),
            groupNameValidate: false,
            haveChange: true,
        });
    }

    onChangeGroupDesc = (value) => {
        this.setState({
            groupDesc: $.trim(value),
            haveChange: true
        });
    }

    onSearchUser(args) {
        this.setState({ searchedUser: args });
    }

    onScopeCellClick = (data) => {
        this.onShowContainerPanel(data);
    }

    onModuleCellClick = (data) => {
        this.onShowModulePermissionsPanel(data);
    }

    onSaveContainerClick()
    {
        let callback = (data, success) => {
            if(data)
            {
                let scopeItems = RM.deepcopy(this.state.scopeItems);
                let scopeItem = scopeItems.find(o => o.SourceType == data.SourceType);
                if(scopeItem !== undefined)
                {
                    scopeItem.Containers = data.ContainerItems;
                    let selectedContainerItenms = data.ContainerItems.filter(o => o.isChecked);
                    if (!!selectedContainerItenms.length) {
                        this.setState({
                            scopeValidate: false,
                            scopeValidateMsg: "",
                        });
                    }
                    scopeItem.ContainerNames = selectedContainerItenms.map((item) => { return item.Name; }).join("; ");
                }
                this.setState({
                    scopeItems: scopeItems,
                    showContainerPanel: { show: false }
                },
                () => {
                    this.dispatch(this.scopeTableId, this.state.scopeItems, this.scopeColumns);
                });
            }
        };
        this.dispatch(this.configScopeCopId, 'onSave', callback);
        return false;
    }

    onSavePermissionClick()
    {
        let callback = (data, success) => {
            if(data)
            {

                let moduleItems = RM.deepcopy(this.state.phyModuleItems);
                let phyExplorerModule = moduleItems.find(o => o.Type == PermissionManageModule.PhysicalExplorer);
                if(phyExplorerModule)
                {
                    phyExplorerModule.PermissionItems = data;
                    if(data.length > 0)
                    {
                        let selectedItems = data.filter(o => o.isChecked);
                        phyExplorerModule.PermissionNames = selectedItems.map((item) => { return item.Name; }).join("; ");
                    }
                    this.setState({
                        phyModuleItems: moduleItems,
                        showPhyPermissionsPanel: { show: false }
                    },
                    () => {
                        this.dispatch(this.managePhyPermissionTableId, this.state.phyModuleItems, this.managePhyPermissionColumns);
                    });
                }
            }
        };
        this.dispatch(this.configPhyPermissionCopId, 'save', callback);
        return false;
    }

    getSelectedSubPermissions()
    {
        let selectedPermissions = [];
        let moduleItems = RM.deepcopy(this.state.phyModuleItems);
        let phyExplorerModule = moduleItems.find(o => o.Type == PermissionManageModule.PhysicalExplorer);
        if(phyExplorerModule)
        {
            let selectedItems = phyExplorerModule.PermissionItems.filter(o => o.isChecked);
            if(selectedItems.length > 0)
            {
                selectedPermissions = selectedItems.map(o => { return o.Type; });
            }
        }
        return selectedPermissions;
    }

    saveGroupBefore(callback, termSettings, ruleSettings) {
        //FileSystem: 2, Physical: 4, SharePointOnPrem: 5,
        let groupNameForFS = [];
        let groupNameForPhy = [];
        let groupNameForSPOnPrem = [];
        this.props.groupItems.map((item) => {
            if (item.Id != this.groupId && item.Id != 1 && item.Id != 2) {
                item.ContainsSourceType?.map((type) => {
                    if (type == 2) {
                        groupNameForFS.push(item.Name);
                    }
                    if (type == 4 && item.PhysicalRole == PhyUserRoleType.Admin) {
                        groupNameForPhy.push(item.Name);
                    }
                    if (type == 5) {
                        groupNameForSPOnPrem.push(item.Name);
                    }
                });
            }
        });
        let currentGroup = this.props.groupItems.find(g => g.Id == this.groupId);
        if (this.groupId > -1 && this.state.selectedPermissionSettingValue == PermissionSettingType.DataScope) {
            if (currentGroup.ContainsSourceType?.find(f => f == 2)) {
                if (groupNameForFS.length > 0) {
                    let args = {
                        width: "550px",
                        hideActions: false,
                        title: RMResx.RM_JS_Common_Confirmation,
                        content: <div tabIndex="0">
                            <$g.I18NProvider msg={RMResx.RM_CP_AM_Permission_ForFSMsg}>
                                <span>{groupNameForFS.join(", ")}</span>
                            </$g.I18NProvider>
                        </div>,
                        buttons: [
                            {
                                text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                                    $$.messagedialog(false);
                                }
                            },
                            {
                                text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                    this.checkPermissionForPhy(currentGroup, groupNameForPhy, groupNameForSPOnPrem, callback, termSettings, ruleSettings);
                                }
                            }
                        ]
                    };
                    $$.messagedialog(true, args);
                } else {
                    this.checkPermissionForPhy(currentGroup, groupNameForPhy, groupNameForSPOnPrem, callback, termSettings, ruleSettings);
                }
            } else {
                this.checkPermissionForPhy(currentGroup, groupNameForPhy, groupNameForSPOnPrem, callback, termSettings, ruleSettings);
            }
        } else {
            this.saveGroup(callback, termSettings, ruleSettings);
        }
    }

    checkPermissionForPhy(currentGroup, groupNameForPhy, groupNameForSPOnPrem, callback, termSettings, ruleSettings) {
        $$.messagedialog(false);
        this.checkPermissionForSPOnPrem(currentGroup, groupNameForSPOnPrem, callback, termSettings, ruleSettings);
    }

    checkPermissionForSPOnPrem(currentGroup, groupNameForSPOnPrem, callback, termSettings, ruleSettings) {
        $$.messagedialog(false);
        if (currentGroup.ContainsSourceType?.find(f => f == 5)) {
            if (groupNameForSPOnPrem.length > 0) {
                let args = {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: <div tabIndex="0">
                        <$g.I18NProvider msg={RMResx.RM_CP_AM_Permission_ForSPOnPremMsg}>
                            <span>{groupNameForSPOnPrem.join(", ")}</span>
                        </$g.I18NProvider>
                    </div>,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                                $$.messagedialog(false);
                            }
                        },
                        {
                            text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                                this.saveGroup(callback, termSettings, ruleSettings);
                            }
                        }
                    ]
                };
                $$.messagedialog(true, args);
            } else {
                this.saveGroup(callback, termSettings, ruleSettings);
            }
        } else {
            this.saveGroup(callback, termSettings, ruleSettings);
        }
    }

    saveGroup(callback, termSettings, ruleSettings) {
        $$.messagedialog(false);
        var groupInfo = {
            Id: this.groupId,
            Name: this.state.groupName,
            Description: this.state.groupDesc,
            Users: this.state.searchedUser,
            DataSourceScopeInfo: this.getScopeContainersInfo(),
            IsEnableTrim: this.state.enableTrim,
            IsUseReportingPermissionControl: !!this.state.enablePermissionReport,
            ReportingPermission: this.state.selectedPermissionReportValue,
            IsEnableManageHold: this.state.enableManageHoldsPermission,
            IsEnableApprovalSetting: this.state.enableManageApprovalSettingsPermission,
            SecurityGroupControlType: this.state.selectedPermissionSettingValue,
            FunctionSubPermission: this.state.restoreCenterValue,
        };

        if (this.state.selectedPermissionSettingValue == PermissionSettingType.FunctionMoudle) {
            if ($.trim(this.state.groupName).length == 0) {
                this.setState({
                    groupNameValidate: true
                });
            } else {
                if (this.state.restoreCenterValue === RestoreCenterType.None) {
                    this.setState({
                        restoreCenterValidate: true,
                        groupNameValidate: false
                    })
                } else {
                    this.setState({
                        restoreCenterValidate: false,
                        groupNameValidate: false
                    })
                    groupInfo.DataSourceScopeInfo = [];
                    groupInfo.IsEnableTrim = false;
                    groupInfo.IsEnableManageHold = false;
                    groupInfo.IsEnableApprovalSetting = false;
                    callback(groupInfo);
                }
            }
            return;
        }

        if (this.state.enableTrim) {
            this.resetTermSettingMethod(termSettings);
            this.resetRuleSettingMethod(ruleSettings);
            // if(termSettings.treeNodeInfo)
            // {
            //     if(termSettings.treeNodeInfo.IsChecked)
            //     {
            //         console.log("select all terms");
            //         //return;
            //     }
            //     termSettings.treeNodeInfo.SubTerms.map((item) => {
            //         if(item.IsChecked)
            //         {
            //             console.log(`Termgroup: ${[item.Name]} is select all`);
            //         }
            //         else {
            //             if(item.SubTerms)
            //             {
            //                 let selectedTermSets = item.SubTerms.filter(o => o.IsChecked == true);
            //                 if(selectedTermSets.length > 0)
            //                 {
            //                     var selectedNames = selectedTermSets.map((t) => {
            //                         return t.Name;
            //                     });
            //                     console.log(`Termgroup: ${[item.Name]}, termsets selected count: ${selectedTermSets.length}， termsets name: ${selectedNames.join(";")}`);
            //                 }
            //             }
            //         }
            //     });
            // }
            if(termSettings)
            {
                groupInfo.SetTermPermissionMethod = termSettings.permissionMethod;
                groupInfo.TermTreeNodeInfo = termSettings.treeNodeInfo;
            }
         
    
            if(ruleSettings.treeNodeInfo)
            {
                if(ruleSettings.treeNodeInfo.IsChecked)
                {
                    console.log("select all terms");
                    //return;
                }
                ruleSettings.treeNodeInfo.SubItems.map((item) => {
                    if(item.IsChecked)
                    {
                        console.log(`Termgroup: ${[item.Name]} is select all`);
                    }
                    else {
                        if(item.SubItems)
                        {
                            let selectedTermSets = item.SubItems.filter(o => o.IsChecked == true);
                            if(selectedTermSets.length > 0)
                            {
                                var selectedNames = selectedTermSets.map((t) => {
                                    return t.Name;
                                });
                                console.log(`Termgroup: ${[item.Name]}, termsets selected count: ${selectedTermSets.length}， termsets name: ${selectedNames.join(";")}`);
                            }
                        }
                    }
                });
            }
            groupInfo.SetRulePermissionMethod = ruleSettings.permissionMethod;
            groupInfo.RuleTreeNodeInfo = ruleSettings.treeNodeInfo;
        }

        if($.trim(this.state.groupName).length == 0)
        {
            this.setState({
                groupNameValidate: true
            });
            return;
        } else {
            this.setState({
                groupNameValidate: false
            });
        }
        // console.log(groupInfo)
        // return;
        var needCheckScope = !this.isBuiltInReviewUserGroup();
        let [invalidScope, errorMessage, groupScopesInfo] = [false, "", groupInfo.DataSourceScopeInfo];
        if(needCheckScope)
        {
            if(groupScopesInfo.length == 0)
            {
                invalidScope = true;
                errorMessage = RMResx.RM_CP_AM_NoSelectedScope_Msg;
            }else 
            {
                if (
                    !this.isValidScopeInfo(groupScopesInfo, SourceFlags.SP) ||
                    !this.isValidScopeInfo(groupScopesInfo, SourceFlags.Exo) ||
                    !this.isValidScopeInfo(groupScopesInfo, SourceFlags.OneDrive) ||
                    !this.isValidScopeInfo(groupScopesInfo, SourceFlags.Teams) ||
                    !this.isValidScopeInfo(groupScopesInfo, SourceFlags.Phy)
                    )
                {
                    invalidScope = true;
                    errorMessage = RMResx.RM_CP_AM_NoSelectContainers_Msg;
                }
            }
            if(invalidScope)
            {
                this.setState({
                    scopeValidate: true,
                    scopeValidateMsg: errorMessage
                });
                return;
            } else {
                this.setState({
                    scopeValidate: false,
                    scopeValidateMsg: ""
                });
            }
        }
        callback(groupInfo);
    }

    isValidScopeInfo(groupScopesInfo, sourceType)
    {
        if (sourceType == SourceFlags.Phy && this.state.selectedValue == PhyUserRoleType.EndUser) {
            return true;
        }
        
        let isValid = true;
        let scopeInfo = groupScopesInfo.find(o => o.DataSourceType == sourceType);
        if(scopeInfo)
        {
            if(!scopeInfo.ScopeIds || scopeInfo.ScopeIds.length == 0)
            {
                isValid = false;
            }
        }
        return isValid;
    }

    onShowContainerPanel = (data) => {
        this.setState({
            configScopePanelTitle: data.SourceType == SourceFlags.Phy ? RMResx.RM_CP_AM_EditPhyContainers_Title: RMResx.RM_CP_AM_EditContainers_Title,
            showContainerPanel: { show: true }
        }, () => {
            this.dispatch(this.configScopeCopId, "init", data);
        });
    }

    onShowModulePermissionsPanel = (data) => {
        this.setState({
            showPhyPermissionsPanel: { show: true }
        }, () => {
            this.dispatch(this.configPhyPermissionCopId, "init", data.PermissionItems, this.initPhyPermissionColumns());
        });
    }

    handleResetPermission = () => { // Used for Report, Manage Holds, Approval Settings
        this.setState({
            enablePermissionReport: false,
            permissionReportList: getPermissionReportList().map((item) => ({ ...item, checked: false })),
            selectedPermissionReportValue: 0,
            enableManageHoldsPermission: false,
            enableManageApprovalSettingsPermission: false,
        });
    }

    handleCheckPermissionReport = () => {
        this.setState({
            enablePermissionReport: true,
            permissionReportList: getPermissionReportList(),
            selectedPermissionReportValue: getPermissionReportList().reduce((acc, curr) => acc + curr.value, 0),
        });
        this.isChangedReportingPermission = true;
    }

    handleCheckPermission = (items) => {
        const filteredScopeItems = items.filter(o => o.isChecked);
        const isOnlyPhysicalSelected = filteredScopeItems.length === 1 && filteredScopeItems[0].SourceType === SourceFlags.Phy;
        const isSelectedEndUserValue = this.state.selectedValue == PhyUserRoleType.EndUser;

        if (isOnlyPhysicalSelected && isSelectedEndUserValue) {
            this.isChangedReportingPermission = false;
            this.handleResetPermission();
        } else {
            if (!this.isChangedReportingPermission) {
                this.handleCheckPermissionReport();
            }
        }
    }

    onCheckChanged(items) {
        let scopeItems = items.slice();
        this.resetScopeItemsStatus(scopeItems);

        const isPhysicalSelected = this.isPhysicalSelected(scopeItems);
        const isGoogleSelected = this.isGoogleSelected(scopeItems);
        const stateUpdate = {
            isPhysicalSelected,
            isGoogleSelected
        };

        this.handleCheckPermission(items);

        if(isPhysicalSelected)
        {
            this.setState({ isPhysicalSelected: true },()=>
            {
                if(this.state.selectedValue == PhyUserRoleType.EndUser)
                {
                    this.dispatch(this.managePhyPermissionTableId, this.state.phyModuleItems, this.managePhyPermissionColumns);
                }
            });
        } if(!isPhysicalSelected && !isGoogleSelected) {
            stateUpdate.isPhysicalSelected = false;
            stateUpdate.isGoogleSelected = false;
        }
        this.setState(stateUpdate);
        this.setSelectedAdmin();
    }

    resetScopeItemsStatus(items)
    {
        let scopeItems = RM.deepcopy(this.state.scopeItems);
        scopeItems.map(o => {
            o.isChecked = items.find(t => t.Id == o.Id && t.isChecked) !== undefined ? true : false;
        });
        const isAnyChecked = scopeItems.some(item => item.isChecked === true);
        if (isAnyChecked) {
            this.setState({
                scopeValidate: false,
                scopeValidateMsg: ""
            });
        }
        this.setState({ scopeItems: scopeItems, scopeSelectedValidate: false });
    }

    isPhysicalSelected(items)
    {
        return items.find(o => o.SourceType == SourceFlags.Phy && o.isChecked == true) !== undefined;
    }

    isGoogleSelected(items)
    {
        return items.find(o => o.SourceType == SourceFlags.Google && o.isChecked == true) !== undefined;
    }

    setPerSelectedValue = (value)=>
    {
        this.setState({ selectedValue: value });
    }

    resetTermSettingMethod(termSettings)
    {
        if(!termSettings)
        {
            return;
        }

        let treeNode = termSettings.treeNodeInfo;
        if(treeNode)
        {
            if(treeNode.IsChecked)
            {
                termSettings.permissionMethod = SetTermPermissionMethod.All;
            }
            else {
                if(treeNode.SubTerms)
                {
                    let termGroups = treeNode.SubTerms;
                    if(termGroups.some(o => o.IsChecked)) 
                    {
                        termSettings.permissionMethod = SetTermPermissionMethod.SpecifyScope;
                    }
                    else {
                        let hasTermSetChecked = false;
                        termGroups.map((t) => {
                            if(t.SubTerms && t.SubTerms.filter(o => o.IsChecked).length > 0)
                            {
                                hasTermSetChecked = true;
                                return;
                            }
                        });
                        termSettings.permissionMethod = hasTermSetChecked ? SetTermPermissionMethod.SpecifyScope : SetTermPermissionMethod.None;
                    }
                }
            }
        } else {
            termSettings.permissionMethod = SetTermPermissionMethod.None;
        }
        if(termSettings.permissionMethod != SetTermPermissionMethod.SpecifyScope)
        {
            termSettings.treeNodeInfo = null;
        }
    }

    resetRuleSettingMethod(ruleSettings)
    {
        let treeNode = ruleSettings.treeNodeInfo;
        if(treeNode)
        {
            if(treeNode.IsChecked)
            {
                ruleSettings.permissionMethod = RulePermissionMethod.All;
            }
            else {
                if(treeNode.SubItems)
                {
                    let ruleContainers = treeNode.SubItems;
                    if(ruleContainers.some(o => o.IsChecked)) 
                    {
                        ruleSettings.permissionMethod = RulePermissionMethod.SpecifyScope;
                    }
                    else {
                        let hasRuleItemChecked = false;
                        ruleContainers.map((t) => {
                            if(t.SubItems && t.SubItems.filter(o => o.IsChecked).length > 0)
                            {
                                hasRuleItemChecked = true;
                                return;
                            }
                        });
                        ruleSettings.permissionMethod = hasRuleItemChecked ? RulePermissionMethod.SpecifyScope : RulePermissionMethod.None;
                    }
                }
            }
        } else {
            ruleSettings.permissionMethod = RulePermissionMethod.None;
        }
        if(ruleSettings.permissionMethod != RulePermissionMethod.SpecifyScope)
        {
            ruleSettings.treeNodeInfo = null;
        }
    }

    onUserRoleCheckChanged = (value) => {
        let selValue = value;
        this.setState({ selectedValue: selValue }, () => {
            if(selValue == PhyUserRoleType.EndUser)
            {
                //physical end user
                this.dispatch(this.managePhyPermissionTableId, this.state.phyModuleItems, this.managePhyPermissionColumns, this.showPanelMode == PanelDisplayMode.View ? true : false);
            }
            this.handleCheckPermission(this.state.scopeItems);
            this.setSelectedAdmin();
        });
    };

    onPermissionSettingChanged = (value) => {
        if (value === PermissionSettingType.DataScope) {
            if (this.groupId > -1) {
                let callback = () => {
                    this.dispatch(this.scopeTableId, this.state.scopeItems, this.scopeColumns, this.getDisabledStatus());
                    if (this.state.selectedValue == PhyUserRoleType.EndUser) {
                        //physical end user
                        this.dispatch(this.managePhyPermissionTableId, this.state.phyModuleItems, this.managePhyPermissionColumns, this.showPanelMode == PanelDisplayMode.View ? true : false);
                    }
                }
                this.cacheGroupData.SecurityGroupControlType = PermissionSettingType.DataScope;
                this.cacheGroupData.FunctionSubPermission = 0;
                this.setGroupData(this.cacheGroupData, callback);
            } else {
                this.loadAssignContainerIds(ass => {
                    this.dispatch(this.scopeTableId, this.getAvailableScope(ass), this.scopeColumns, this.getDisabledStatus());
                });
            }
        }
        this.setState({ 
            selectedPermissionSettingValue: value,
            restoreCenterValidate: false,
        });
    };

    onRestoreCenterChanged = (args) => {
        this.setState({
            restoreCenterValue: args.newValue.value,
            restoreCenterValidate: false,
        });
    }

    onDelegateWillChange = (args) => {
        if (args){
            let msg = {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: <div tabIndex="0">{RMResx.RM_CP_AM_Permission_TermAndRuleMsg}</div>,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                            $$.messagedialog(false);
                        }
                    },
                    {
                        text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                            this.isCheckedTermAndRule(args);
                        }
                    }
                ]
            };
            $$.messagedialog(true, msg);
            return false;
        } else {
            this.isCheckedTermAndRule(args);
            return false;
        }
    }

    isCheckedTermAndRule = (checked) => {
        this.setState({ enableTrim: checked }, () => {
            if (checked) {
                this.dispatch(this.termPermissionCopId, "reload");
                this.dispatch(this.rulePermissionCopId, "reload");
            }
        });
    }

    getPermissionOptions(selectedValue) {
        let options = [];
        if (this.showPhysical) {
            options.push({ text: RMResx.RM_CP_AM_PhysicalPermission_Admin, value: "1" });
            options.push({ text: RMResx.RM_CP_AM_PhysicalPermission_EndUser, value: "2" });
            options.forEach(op => {
                op.title = op.text;
                op.checked = selectedValue == op.value;
            });
        }else{
            options.push({ text: RMResx.RM_CP_AM_PhysicalPermission_EndUser, value: "2", checked: true, title: RMResx.RM_CP_AM_PhysicalPermission_EndUser });
        }
        return options;
    }

    getPermissionSettingOptions(selectedValue) {
        let options = [
            { text: RMResx.RM_CP_AM_Permission_DataScope, value: PermissionSettingType.DataScope },
        ];
        if (LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusSOLicense()) {
            options.push({ text: RMResx.RM_CP_AM_Permission_FunctionMoudle, value: PermissionSettingType.FunctionMoudle })
        }
        options.forEach(op => {
            op.title = op.text;
            op.checked = selectedValue == op.value;
        });
        return options;
    }

    setSelectedAdmin() {
        let selectedAdmin= this.getSelectedAdmin();
        if (selectedAdmin) {
            this.setState({ showTrimButton: true });
        } else {
            this.setState({ showTrimButton: false, enableTrim: false });
        }
    }
    
    getSelectedAdmin() {
        let selectedAdmin = false;
        var selectedScopes = this.state.scopeItems.filter(o => o.isChecked);
        if (selectedScopes.length) {
            selectedAdmin = true;
        }
        // selectedScopes.map((item) => {
        //     if (item.SourceType == SourceFlags.Phy) {
        //         if (this.state.selectedValue == PhyUserRoleType.Admin) {
        //             selectedAdmin = true;
        //         }
        //     } else if (item.SourceType == SourceFlags.Google) {
        //         selectedAdmin = false;
        //     }
        //     else {
        //         selectedAdmin = true;
        //     }
        // });
        return selectedAdmin;
    }

    handleChangePermissionReport = (checked) => {
        this.setState((prev) => {
            const updatedList = prev.permissionReportList.map((item) => ({
                ...item,
                checked,
            }))

            const selectedPermissionReportValue = checked ? updatedList.reduce((acc, curr) => acc + curr.value, 0) : 0;

            return {
                enablePermissionReport: checked,
                permissionReportList: updatedList,
                selectedPermissionReportValue,
            }
        });
    }

    handleChangePermissionReportList = (checkedValues) => {
        const selectedPermissionReportValue = checkedValues.reduce(
            (acc, curr) => acc + curr,
            0
        );
        let enablePermissionReport = false;
        if (checkedValues.length > 0) {
            if (checkedValues.length === this.state.permissionReportList.length) {
                enablePermissionReport = true;
            } else {
                enablePermissionReport = 'mixed';
            }
        }
        this.setState(({
            selectedPermissionReportValue,
            enablePermissionReport,
        }));
    }

    renderContainerPanel() {
        return <R.Panel
            id="configScopePanel"
            header={this.state.configScopePanelTitle}
            size={670}
            actionType='back'
            status={this.state.showContainerPanel}
            destroy={true}
        >
            <div>
                <ConfigScope
                    id={this.configScopeCopId}
                ></ConfigScope>
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showContainerPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSaveContainerClick}
                />
            </>
        </R.Panel>;

    }

    renderPhyPermissionPanel() {
        return <R.Panel
            id="configPermissionPanel"
            header={RMResx.RM_CP_AM_EditPermission_Title}
            size={670}
            actionType='back'
            status={this.state.showPhyPermissionsPanel}
            destroy={true}
        >
            <div>
                <div className="module-desc" tabIndex={0}>{`${RMResx.RM_CP_AM_ManagePermission_Module_Title}: ${RMResx.RM_CP_AM_Module_PhyExplorer}`}</div>
                <ConfigPhysicalPermissionTable
                    id={this.configPhyPermissionCopId}
                ></ConfigPhysicalPermissionTable>
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showPhyPermissionsPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSavePermissionClick}
                />
            </>
        </R.Panel>;
    }

    renderPhysicalUserRoleContent()
    {
        return <div>
            <div className="ra-form-label" >
                <div className='input-label' tabIndex='0'>{RMResx.RM_CP_AM_Table_Column_PermissionName}</div>
            </div>
            <div className="ra-form-content">
                <R.Radio.Group
                    name="radiogroup-permission"
                    class="radiogroup"
                    items={this.getPermissionOptions(this.state.selectedValue)}
                    onChange={this.onUserRoleCheckChanged}
                    block={false}
                    disabled={this.getDisabledStatus()}
                />
            </div>

        </div>;
    }

    renderPermissionSettingRadio() {
        return <div>
            <div className="ra-form-label" >
                <div className='input-label' tabIndex='0'>{RMResx.RM_CP_AM_Table_Column_PermissionGroupName}</div>
            </div>
            <div className="ra-form-content">
                <R.Radio.Group
                    name="radiogroup-permissionsetting"
                    class="radiogroup"
                    items={this.getPermissionSettingOptions(this.state.selectedPermissionSettingValue)}
                    onChange={this.onPermissionSettingChanged}
                    block={false}
                    disabled={this.getDisabledStatus()}
                />
            </div>
        </div>;
    }

    renderPermissions()
    {
        return <>
            <div className="ra-form-label">
                <div className={this.state.pageComponents.groupNameDisabled ? 'input-label' : 'input-label require'} tabIndex='0'>{RMResx.RM_CP_AM_Table_Column_ScopeName}</div>
            </div>
            <div className="ra-form-content">
                <ScopeTable
                    id={this.scopeTableId}
                    columnInfo={this.scopeColumns}
                    selectedValue={this.state.selectedValue}
                    onCheckChanged={this.onCheckChanged}
                    cellClick={this.onScopeCellClick} />
                <$g.ValidationMsg show={this.state.scopeValidate}>
                    {this.state.scopeValidateMsg}
                </$g.ValidationMsg>
            </div>
        </>;
    }

    renderDataScopeContent() {
        const { scopeItems, selectedValue } = this.state;
        const filteredScopeItems = scopeItems.filter(o => o.isChecked);
        const isOnlyPhysicalSelected = filteredScopeItems.length === 1 && filteredScopeItems[0].SourceType === SourceFlags.Phy;
        const isSelectedEndUserValue = selectedValue == PhyUserRoleType.EndUser;

        return <div id={this.props.id}>
            {!this.isBuiltInReviewUserGroup() && this.renderPermissions()}
            {this.state.isPhysicalSelected && this.renderPhysicalUserRoleContent()}
            {this.state.isPhysicalSelected && this.state.selectedValue == PhyUserRoleType.EndUser &&
                <ManagePhysicalPermissionTable
                    id={this.managePhyPermissionTableId}
                    columnInfo={this.managePhyPermissionColumns}
                    cellClick={this.onModuleCellClick}
                />
            }

            {(this.groupId != DefaultSecurityGroup.BuiltInEndUser && this.state.showTrimButton && !(isOnlyPhysicalSelected && isSelectedEndUserValue)) && <div>
                <div className="ra-form-label form-label-delegate flex align-center">
                    <div className='input-label input-label-delegate' tabIndex='0'>{RMResx.RM_CP_AM_Permission_DelegateTitle}</div>
                    <$g.Popover>{RMResx.RM_CP_AM_Permission_DelegateTips}</$g.Popover>
                </div>
                <div id="raDelegateCheckbox" className="ra-form-content">
                    <R.Checkbox
                        id="raCpAmDelegateCheckbox"
                        text={RMResx.RM_CP_AM_Permission_EnableDelegate}
                        title={RMResx.RM_CP_AM_Permission_EnableDelegate}
                        checked={this.state.enableTrim}
                        disabled={this.state.trimCheckBoxIdDisable}
                        willChange={this.onDelegateWillChange.bind(this)}
                    />
                </div>
            </div>}
            {this.state.enableTrim && <div>
                {(this.hasRecordsLicense || this.hasGoogleLicense) &&
                    <div>
                        <div className="ra-form-label" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_CP_AM_TermPermission_Title}</div>
                        </div>
                        <div className="ra-form-content">
                            <TermPermissionSettings
                                id={this.termPermissionCopId}
                                groupId={this.groupId}
                            />
                        </div>
                    </div>}

                <div className="ra-form-label" >
                    <div className='input-label' tabIndex='0'>{RMResx.RM_CP_AM_RulePermission_RuleTitle}</div>
                </div>
                <div className="ra-form-content">
                    <RulePermissionSettings
                        id={this.rulePermissionCopId}
                        groupId={this.groupId}
                    />
                </div>
            </div>}
        </div>
    }

    renderFunctionMoudleContent() {
        return <div>
            <div className="ra-form-label" >
                <div className='input-label require' tabIndex='0'>{RMResx.RM_CP_AM_Table_Column_RestoreCenterName}</div>
            </div>
            <div className="ra-form-content">
                <R.Combobox
                    id="restoreCenterCombo"
                    width="100%"
                    searchable={false}
                    items={this.getRestoreCenterItems()}
                    textField="text"
                    valueField="value"
                    checkedField='checked'
                    onChange={this.onRestoreCenterChanged}
                />
                <$g.ValidationMsg show={this.state.restoreCenterValidate}>
                    {RMResx.RM_AR_CP_Common_SelEmpty}
                </$g.ValidationMsg>
            </div>
        </div>
    }

    renderReportContent() {
        return (
            <div className="margin-bottom-l">
                <div className="ra-form-label form-label-delegate flex align-center">
                    <div tabIndex='0' className='input-label input-label-delegate'>{RMResx.RM_CP_AM_Report}</div>
                    <$g.Popover>{RMResx.RM_CP_AM_Report_Tips}</$g.Popover>
                </div>
                <div id="raSpecificReportCheckbox">
                    <R.Checkbox
                        id="raCpAmSpecificReportCheckbox"
                        text={RMResx.RM_CP_AM_Report_Permission_SpecificReport}
                        title={RMResx.RM_CP_AM_Report_Permission_SpecificReport}
                        checked={this.state.enablePermissionReport}
                        disabled={this.state.trimCheckBoxIdDisable}
                        onChange={this.handleChangePermissionReport}
                    />
                </div>
                <div style={{ marginLeft: 26 }} className="flex flex-column gap-xs margin-top-s">
                    <R.Checkbox.Group
                        id="raCpAmSpecificReportCheckboxGroup"
                        block
                        name="checkboxgroup-reportList"
                        items={this.state.permissionReportList}
                        disabled={this.state.trimCheckBoxIdDisable}
                        onChange={this.handleChangePermissionReportList}
                    />
                </div>
            </div>
        );
    }

    renderManageHoldsContent() {
        return (
            <div className="margin-bottom-l">
                <div className="ra-form-label form-label-delegate">
                    <div tabIndex='0' className='input-label'>{RMResx.RM_CP_AM_ManageHolds}</div>
                </div>
                <div id="raManageHoldsCheckbox">
                    <R.Checkbox
                        id="raCpAmManageHoldsCheckbox"
                        text={RMResx.RM_CP_AM_ManageHolds_Option01}
                        title={RMResx.RM_CP_AM_ManageHolds_Option01}
                        checked={this.state.enableManageHoldsPermission}
                        disabled={this.state.trimCheckBoxIdDisable}
                        onChange={(checked) => this.setState({ enableManageHoldsPermission: checked })}
                    />
                </div>
            </div>
        );
    }

    renderManageApprovalSettingsContent() {
        return (
            <div className="padding-bottom-l">
                <div className="ra-form-label form-label-delegate">
                    <div tabIndex='0' className='input-label'>{RMResx.RM_CP_AM_ManageApprovalSettings}</div>
                </div>
                <div id="raManageApprovalSettingsCheckbox">
                    <R.Checkbox
                        id="raCpAmManageApprovalSettingsCheckbox"
                        text={RMResx.RM_CP_AM_ManageApprovalSettings_Option01}
                        title={RMResx.RM_CP_AM_ManageApprovalSettings_Option01}
                        checked={this.state.enableManageApprovalSettingsPermission}
                        disabled={this.state.trimCheckBoxIdDisable}
                        onChange={(checked) => this.setState({ enableManageApprovalSettingsPermission: checked })}
                    />
                </div>
            </div>
        );
    }

    render() {
        const { scopeItems, selectedValue } = this.state;
        const filteredScopeItems = scopeItems.filter(o => o.isChecked);
        const isOnlyPhysicalSelected = filteredScopeItems.length === 1 && filteredScopeItems[0].SourceType === SourceFlags.Phy;
        const isSelectedEndUserValue = selectedValue == PhyUserRoleType.EndUser;
        
        return <div>
            <div style={{ marginBottom: 12 }} hidden={!this.state.showTip}>
                <R.Messagebar
                    message={this.state.tipMsg} classify={this.state.tipType}
                    onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
            </div>
            <div className="panel-description-form">
                <div className="ra-form-label" >
                    <div className={this.state.pageComponents.groupNameDisabled ? 'input-label' : 'input-label require'}>{RMResx.RM_CP_AM_Table_Column_GroupName}</div>
                </div>
                <div className="ra-form-content">
                    <R.Input
                        name='iptSecurityGroupName'
                        type='text'
                        width={600}
                        value={this.state.groupName}
                        onChange={this.onChangeGroupName}
                        disabled={this.state.pageComponents.groupNameDisabled}
                        aria={{ ariaLabel: RMResx.RM_CP_AM_Table_Column_GroupName }}
                    />
                    <$g.ValidationMsg show={this.state.groupNameValidate}>
                        {RMResx.RM_FS_Register_NameInputValidateMessage}
                    </$g.ValidationMsg>
                </div>
                <div className="ra-form-label" >
                    <div className='input-label'>{RMResx.RM_CP_AM_Table_Column_Desc}</div>
                </div>
                <div className="ra-form-content">
                    <R.Input
                        name='iptConnGroupDesc'
                        type='textarea'
                        width={600}
                        height={88}
                        value={this.state.groupDesc}
                        onChange={this.onChangeGroupDesc}
                        disabled={this.state.pageComponents.groupDescDisabled}
                        aria={{ ariaLabel: RMResx.RM_CP_AM_Table_Column_Desc }}
                    />
                </div>

                <div className="ra-form-label" >
                    <div className='input-label' tabIndex='0'>{RMResx.RM_CP_AM_Table_Column_MembersName}</div>
                </div>
                <div className="ra-form-content">
                    <PeoplePicker
                        id="raCpAmGroupMembers"
                        width={600}
                        items={this.state.searchedUser}
                        selectionChanged={this.onSearchUser}
                        disabled={this.state.pageComponents.groupMembersDisabled}
                    />
                </div>

                {RM.gData.enableRecordsArchiver && !this.isBuiltInReviewUserGroup() && this.renderPermissionSettingRadio()}
                {this.state.selectedPermissionSettingValue === PermissionSettingType.DataScope && this.renderDataScopeContent()}
                {this.state.selectedPermissionSettingValue === PermissionSettingType.FunctionMoudle && this.renderFunctionMoudleContent()}
                {this.groupId != DefaultSecurityGroup.BuiltInEndUser &&
                    !(isOnlyPhysicalSelected && isSelectedEndUserValue) &&
                    !this.isBuiltInReviewUserGroup() &&
                    this.state.selectedPermissionSettingValue === PermissionSettingType.DataScope && (
                        <>
                            {this.renderReportContent()}
                            {!LicenseHelper.HasOpusSOLicenseOnly() && (
                                <>
                                    {this.renderManageHoldsContent()}
                                    {this.renderManageApprovalSettingsContent()}
                                </>
                            )}
                        </>
                    )}
            </div>
            {this.renderContainerPanel()}
            {this.renderPhyPermissionPanel()}
        </div>;
    }
}