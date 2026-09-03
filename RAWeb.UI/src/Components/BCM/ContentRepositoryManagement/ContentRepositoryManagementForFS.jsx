import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import ManualApprovalSettingComponent from "./ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import { isShowActionByDC, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import CRMFSTree from "../../Common/Tree/Instances/FSTree/CRMFSTree";
import FileSystemDocumentTerm from "./DocumentTermSetting/Context/FileSystemDocumentTerm";
import FSManualApprovalSetting from "./ManualApprovalSetting/Context/FSManualApprovalSetting";
import FSScheduleSetting from "./ScheduleSetting/Context/FSScheduleSetting";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import CheckAgentAvailable from "./Common/CommonMessageBar/CheckAgentAvailable";
import ImportSettingPanel from "./ImportSettingPanel";
import { checkPermission } from "../../../Utilities/permissionManager";
import ClassificationSettingPanel, { ClassificationSettingType } from "./ClassificationSetting/ClassificationSettingPanel";
import { Messagebox } from "../../Common/Messagebox";
import UniqueIdSettingCommon from "./UniqueIdSetting/UniqueIdSettingCommon";
import FSUniqueIdSetting from "./UniqueIdSetting/Context/FSUniqueIdSetting";
import RouterUrls from "../../../Constants/RouterUrls";
import ConnectionSettings from "../FSConnGroup/Components/ConnectionSettings";
import ViewPermissionPanel from "../FSConnGroup/Components/ViewPermissionPanel";
import GeneralManagementComponent from "./GeneralManagement/GeneralManagementComponent";
import FileSystemGeneralManagement from "./GeneralManagement/Context/FileSystemGeneralManagement";
import FSDriveInformation from "./DriveInformationCard/DriveInformation";
import { NodeLevel } from "../../../Constants/DAEnums";
import FSClassCode from "./ClassCodeSetting/ClassCodePolicyPanel";
import AuditTrailPanel from "./AuditTrail/AuditTrailPanel";
import { ClassCodeSelectorPanel } from "./ClassCodeSetting/ClassCodeSelectorPanel";
import DownloadRccReportPanel from "./DownloadRccReport/DownloadRccReportPanel";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2
};

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();

const isMultiGeoMainDC = isShowActionByDC();
export default class ContentRepositoryManagementForFS extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.saveConnectionUrl = "/api/ConnectionRegisterApi/SaveConnection";
        this.connectionSettingsPanelId = "ra-connection-settings-panel";
        this.state = {
            showTip: false,
            tipType: 'success',
            tipMsg: '',
            items: [],
            searchKey: "",
            showRightSetting: false,
            enableClassification: "",
            enableSyncData: "",
            headerName: "",
            nodeLevel: "",
            initUniqueIdPanel: false,
            skipActionInMessageBox: false,
            isCustomSetting: false,
            isActive: true,
            isShowImportSettingsPanel: { show: false },
            initImportPanel:false,
            isShowClassificationSettingsPanel: { show: false },
            initClassificationPanel:false,
            classificationData: ClassificationSettingType.FileLevel,
            isShowViewPermissionPanel: { show: false },
            isShowEditConnectionPanel: { show: false },
            isShowApplyClassCodePanel: { show: false },
            connection: {},
            driveInformationComponentKey: "driveInformationComponent",
            isShowAuditTrailPanel: { show: false },
            isShowDownloadRCCReportPanel: { show: false },
        };
        this.generalManagementComponent = "generalManagementComponent";
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.containerTermSettingComponent = "containerTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [
            { isStatic: true, name: RMResx.RM_FS_Register_PageTitle_Link, id:"raCrmRouteToConnGroupBtn", onClick: () => { this.props.history.push({ pathname: "/Root/BCM/FSConnGroup" }); } }
        ];
        this.inheritButton = { name: RMResx["RM_SPS_InheritGlobalSettings"], id:"raCrmInheritParentBtn", icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { name: RMResx["RM_JS_SPS_DisposalNow"], id:"raCrmRunRuleActionBtn", icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.syncDataButton = { name: RMResx["RM_JS_SPS_CollectNow"], id:"raCrmRunDataSyncBtn", icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.runEnforceRuleActionButtonGroup = [
            {
                classify: "default",
                id: "raCrmEnforceRuleActionBtnGroup",
                isGroup: true,
                name: RMResx["RM_JS_SPS_DisposalNow"],
                icon: "fia-run",
                buttons: [
                    {
                        id: "raCrmRunRuleOnSelectedNodeActionBtn",
                        name: RMResx.RM_JS_FS_DisposalOnSelectedNode,
                        onClick: this.runRuleActionMessageBox.bind(this)
                    },
                    { 
                        id:"raCrmRunRuleOnSpecialClassActionBtn", 
                        name: RMResx.RM_JS_FS_DisposalOnSpecificClassCode, 
                        onClick: this.runEnforceRuleOnSpecificClassCode.bind(this)
                    }
                ]
            }
        ];
        this.menuBtnItemsInMore = [];
        if (checkPermission("BCM_ContentRepositoryManagement_UniqueId", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id:"raCrmUniqueIdSettingBtn", name: RMResx.RM_JS_SP_UniqueIdSetting_Btn, icon: "fia-uniqueid", onClick: this.showUniqueIdSettingsClick.bind(this) });
        }
        if (checkPermission("BCM_ContentRepositoryManagement_Classification", RM.UserResources) && !isEnableJPMCFeature) {
            this.menuBtnItemsInMore.push({ name: RMResx.RM_JS_FS_ClassificationSetting_Btn,  id:"raCrmClassificationSettingsBtn", icon: "fia-classification-settings", onClick: this.showClassificationSettingsClick.bind(this) });
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("BCM_ContentRepositoryManagement_Import", RM.UserResources) && !isEnableJPMCFeature) {
            this.menuBtnItemsInMore.push({ name: RMResx.RM_JS_SP_ImportSetting_Btn,  id:"raCrmImportSettingsBtn", icon: "fia-import", onClick: this.showImportSettingsClick.bind(this) });
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("BCM_ContentRepositoryManagement_Export", RM.UserResources) && !isEnableJPMCFeature) {
            this.menuBtnItemsInMore.push({ name: RMResx.RM_JS_SP_ExportSetting_Btn, id:"raCrmExportSettingsBtn", icon: "fia-export-settings", onClick: this.onExport.bind(this) });
        }
        this.viewPermissionButton = {
            name: RMResx.RM_JS_FS_ViewPermission_Btn,
            icon: "fia-permission",
            id: "raCrmViewPermissionSettingsBtn",
            onClick: this.showViewPermissionPanel.bind(this),
        };
        this.viewAuditTrailButton = {
            icon: "fia-eye",
            name: RMResx.RM_FS_AuditTrail,
            id: "raCrmAuditTrailBtn",
            onClick: this.showAuditTrailPanel.bind(this),
        }
        this.downloadRccReportButton = {
            name: RMResx.RM_FS_DownloadRCCReport,
            id: "raDownloadRCCReportBtn",
            icon: "fia-download",
            onClick: this.showDownloadRCCReportPanel.bind(this)
        }
        this.monitoringButton = { name: RMResx.RM_JS_FS_Monitoring, id: "raCrmMonitoringBtn", icon: "fia-job-monitor", onClick: this.onRedirectToMonitoring.bind(this) };
        this.applyClassClassCodeButton = { name: RMResx.RM_FS_ClassCodePolicy_ApplyClassCode, id: "raApplyClassCodeBtn", icon: "fia-apply", onClick: this.onShowClassCodePanel.bind(this) };
        this.showSkipAction = false;
        this.checkAgentUrl = "/api/FSSettingApi/CheckHasAvailableAgent";
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
        this.setState({ initUniqueIdPanel: true, initImportPanel: true, initClassificationPanel: true });
        this.initClassificationSetting();
    }

    initClassificationSetting = () => {
        let option = {
            url: "/API/FSSettingApi/GetClassificationLevel",
            method: "Post",
        };
        fetchUtility(option).then((res) => {
            if (res) {
                this.setState({ classificationData: res });
            }
        }).catch((e) => {
        });
    }

    getClassificationData = () => {
        return this.state.classificationData;
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/FSSettingApi/InheritFSParentSetting",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result == "Sucess") {
                this.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else if (result == "Failed") {
                showToast.error(RMResx.RM_JS_SPS_SaveSettingsFailed);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }
    
    inheritParentMessageBox = () => {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_EnsureInherit,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmInheritParentDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.inheritParentDoAction.bind(this)
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    runDataSyncDoAction(){
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/FSSettingApi/RunFSCollectionJob",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.ErrorMessage != "") {
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    runDataSyncMessageBox(){
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NFSS_EnsureSync,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raRunDataSyncDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick:  this.runDataSyncDoAction.bind(this)
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    runRuleActionDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        this.settingNode.SkipRemoveContentAndDestroyAction = this.state.skipActionInMessageBox;
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: "/api/FSSettingApi/RunFSDisposalJob",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (result.MessageType == RAMessageType.Failed) {
                if (result.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_NoExportLocation) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_NoExportLocation);
                }else{
                    showToast.error(result.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.FS]);
    }

    runRuleActionMessageBox(){
        // Connection Group cannot determine whether child connections have applied class codes, so skip validation and allow the enforce rule job to run.
        // if(isEnableJPMCFeature && !this.settingNode.ClassCode) {
        //     this.confirmNoClassCodeAppliedDialog();
        //     return;
        // }
        this.setState({ skipActionInMessageBox: false });
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m">{RMResx.RM_JS_BCM_NSPS_EnsureRun}</div>
                {this.showSkipAction && <div className="ra-inline-middle margin-bottom-m">
                    <R.Checkbox
                        text={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                        title={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                        checked={this.state.skipActionInMessageBox}
                        onChange={this.skipActionInMessageBoxChanged.bind(this)}
                    />
                </div>}
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunRuleActionDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runRuleActionDoAction.bind(this)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    skipActionInMessageBoxChanged(args) {
        this.setState({ skipActionInMessageBox: args });
    }

    runEnforceRuleOnSpecificClassCode() {
        this.setState({ isShowClassCodeSelectorPanel: { show: true } });
    }

    onTreeChanged = (nodeItem) => {
        this.currNode = nodeItem;
        this.loadNodeSettings();
    }

    loadNodeSettings(reload) {
        let nodeItem = Object.assign({}, this.currNode, { Children: null, ChildrenIds: null });
        let currentItem = nodeItem;
        while (currentItem.Parent) {
            currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
            currentItem = currentItem.Parent;
        } 
        $$.loading(true);
        let option = {
            url: "/api/FSSettingApi/LoadFSNodeSetting",
            method: "Post",
            data: nodeItem
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let settingNode = JSON.parse(result);
                this.settingNode = settingNode;
                settingNode.ParentId = nodeItem.ParentId;
                settingNode.Parent = nodeItem.Parent;
                if(settingNode.IconStatus == 0) {
                    settingNode.EnableRecordManagement = EnableRecordManagementSetting.Enable;
                }
                this.setState({
                    showRightSetting: true,
                    headerName: settingNode.Name,
                    nodeLevel: settingNode.Level,
                    isCustomSetting: settingNode.IsCustomSetting,
                    isActive: settingNode.IsActive,
                    enableClassification: settingNode.EnableRecordManagement,
                    driveInformationComponentKey: `driveInformationComponent_${Date.now()}`
                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
                    this.dispatch(this.manualApprovalSettingComponent, 'manualApprovalData', settingNode);
                    this.dispatch(this.scheduleSettingComponent, 'scheduleData', settingNode);
                    this.refGeneralManagementComponent && this.refGeneralManagementComponent.initData(settingNode);
                });
                let updateProps = { IconStatus: settingNode.IconStatus, IsCustomSetting: settingNode.IsCustomSetting };
                if (nodeItem.IconStatus == 0 && settingNode.IconStatus == 2 || reload) {
                    this.refCRMTree.refreshSelectedNode(updateProps, true, settingNode.IsActive);
                } else {
                    this.refCRMTree.refreshSelectedNode(updateProps, false, settingNode.IsActive);
                }
                let menuButtons = [...this.menuBtnItems];
                let moreButtons = [...this.menuBtnItemsInMore];
                if (settingNode.IsCustomSetting) {
                    menuButtons.push(this.inheritButton);
                }
                const isRecordManagementEnabled = !isEnableJPMCFeature || settingNode.EnableRecordManagement == EnableRecordManagementSetting.Enable;
                let enableRunJob = !CRMCommonUtil.guidIsEmpty(settingNode.TermSetId) && settingNode.IsActive;
                if (isEnableJPMCFeature && enableRunJob) {
                    if (isRecordManagementEnabled) {
                        if (settingNode.Level === NodeLevel.WebApplication || settingNode.Level === NodeLevel.SiteCollection) { 
                            menuButtons.push(...this.runEnforceRuleActionButtonGroup);
                        } else {
                            menuButtons.push(this.ruleActionButton);
                        }
                    }
                    menuButtons.push(this.syncDataButton);
                }
                if (!isEnableJPMCFeature && enableRunJob) {
                    if (isRecordManagementEnabled) {
                        menuButtons.push(this.ruleActionButton);
                    }
                    menuButtons.push(this.syncDataButton);
                }
                if (isEnableJPMCFeature && settingNode.Level) {
                    if(settingNode.Level === NodeLevel.SiteCollection) {
                        moreButtons.push(this.viewPermissionButton);
                    }
                    moreButtons.push(this.viewAuditTrailButton);
                }

                const hasValidTermSet = !CRMCommonUtil.guidIsEmpty(settingNode.TermSetId);
                const isRecordManagementEnable = settingNode.EnableRecordManagement === EnableRecordManagementSetting.Enable;
                if (isEnableJPMCFeature
                    && hasValidTermSet
                    && (settingNode.Level === 2 || settingNode.Level === 100)
                    && isRecordManagementEnable
                ) {
                    const classificationIndex = moreButtons.findIndex(btn => btn.id === "raCrmClassificationSettingsBtn");
                    const insertIndex = classificationIndex >= 0 ? classificationIndex + 1 : 1;
                    moreButtons.splice(insertIndex, 0, this.applyClassClassCodeButton);
                }
                if (isEnableJPMCFeature && settingNode.Level !== NodeLevel.WebApplication) {
                    moreButtons.push(this.monitoringButton);
                    if(settingNode.IsActive && isRecordManagementEnable) {
                        moreButtons.push(this.downloadRccReportButton);
                    }
                }
                menuButtons.push(...moreButtons);
                this.refTopButtons.updateButtons(menuButtons);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    checkMissingConfig = () => {
        if (CRMCommonUtil.guidIsEmpty(this.settingNode.TermSetId) && !CRMCommonUtil.isGroup(this.settingNode)) {
            let args = {
                // classify: "warn",
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_FSS_GroupSettingMissing,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => {
                            $$.messagedialog(false);
                        }
                    }]
            };
            $$.messagedialog(true, args);
            return true;
        } else {
            return false;
        }
    }

    onSearch = (args) => {
        this.setState({ searchKey: args });
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    onActiveClick = (isActive, treeData) =>{
        treeData.IsActive = isActive;
        treeData.IsAllowUserDownloadRCCReport = false;
        const clonedTreeData = RM.deepcopy(treeData);
        clonedTreeData.Parent = null;
        clonedTreeData.Children = null;
        let option = {
            url: "/api/FSSettingApi/FSActiveSetting",
            method: "Post",
            data: clonedTreeData
        };
        fetchUtility(option).then((result) => {
            if (result == "Sucess") {
                this.refCRMTree.refreshSelectedNode({ IconStatus: treeData.IconStatus, IsCustomSetting: treeData.IsCustomSetting}, isActive, isActive);
                this.loadNodeSettings();
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    checkEditIsDisabled(){
        if (CRMCommonUtil.isFSFolder(this.settingNode)) {
            return !this.state.isActive;
        }
        return false;
    }

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    showUniqueIdSettingsClick() {
        this.refUniqueIdComponent.showUniqueIdSettingsPanel();
    }

    showImportSettingsClick() {
        this.setState({ isShowImportSettingsPanel: { show: true } });
    }

    onExport() {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.showExportSettingsClick });
    }

    showExportSettingsClick() {
        const option = {
            url: "/api/BCMAdminSettingApi/ExportFSSetting",
            method: "POST",
            data: {}
        };
        $$.loading(true);

        fetchUtility(option).then((result) => {
            if (result) {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                        <a className="ra-link-a" href="/Root/DC/Download">
                            {RMResx.RM_JS_DC_Title}
                        </a>
                    </$g.I18NProvider>
                )
            }
        }).finally(() => $$.loading(false));
    }


    saveImportSettings = (e) => {
        this.dispatch("importSettingPanel",'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowImportSettingsPanel: { show: false } });
            }
        });
        return false;
    }

    cancelImportSettings = () => {
        this.setState({ isShowImportSettingsPanel: { show: false } });
    }

    showClassificationSettingsClick() {
        this.setState({ isShowClassificationSettingsPanel: { show: true } });
    }

    saveClassificationSettings = (e) => {
        this.dispatch("classificationSettingsPanel",'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowClassificationSettingsPanel: { show: false } });
            }
        });
        return false;
    }

    runEnforceRuleOnSpecificClassCodeJob = () => {
        this.runSpecificClassCodeConfirmPopup();
    }

    showViewPermissionPanel = () => {
        $$.loading(true);
        let option = {
            url: `/api/FSSettingApi/GetConnectionPermissions?connectionId=${this.settingNode.Id}`,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            if (res.MessageType === 0) {
                let connection = JSON.parse(res.Extension);
                this.setState({ 
                    isShowViewPermissionPanel: { show: true },
                    connection: connection || {},
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showAuditTrailPanel = () => {
        this.setState({ isShowAuditTrailPanel: { show: true } },
            () => this.dispatch('auditTrailPanelId', 'initData', this.settingNode)
        );
    }

    onDownloadRCCReport = () => {
        this.dispatch('downloadRCCReportPanelId', 'onDownload', this.settingNode, () => this.setState({ isShowDownloadRCCReportPanel: { show: false } }));
    }

    onCloseDownloadRCCReportPanel = () => {
        this.setState({ isShowDownloadRCCReportPanel: { show: false } });
    }

    showDownloadRCCReportPanel = () => {
        this.setState({ isShowDownloadRCCReportPanel: { show: true } });
    }

    onCloseAuditTrail = () => {
        this.setState({ isShowAuditTrailPanel: { show: false } });
    }

    cancelClassificationSettings = () => {
        this.setState({ isShowClassificationSettingsPanel: { show: false } });
    }

    onClosePanelViewPermission = () => {
        this.setState({ isShowViewPermissionPanel: { show: false } });
    };
    
    onCancelEditConnection = () => {
        this.setState({ isShowEditConnectionPanel: { show: false } });
    };

    onShowClassCodePanel = () => {
        this.setState({ isShowApplyClassCodePanel: { show: true } });
    }

    onCloseClassCodePanel = () => {
        this.setState({ isShowApplyClassCodePanel: { show: false } });
    }

    onRedirectToMonitoring = () => { 
        this.props.history.push({
            pathname: RouterUrls.BCM_FSConnection_JobMonitor,
            state: this.settingNode
        });
    }

    onEditViewPermission = () => {
        let callback = () => {
        };
        this.setState({ isShowViewPermissionPanel: { show: false }, isShowEditConnectionPanel: { show: true } }, () => {
            this.dispatch(this.connectionSettingsPanelId, 'onSaveInit', callback, [], [], this.state.connection);
        });
    };

    onSaveClassCodePolicy = (isRunJobNow = false) => { 
        this.dispatch('classCodePolicyPanelId', 'onSave', this.loadNodeSettings.bind(this), isRunJobNow);
    };

    handleSortConnection = (field, sortedData) => {
        this.setState(prev => ({ connection: { ...prev.connection, [field]: sortedData }}));
    }

    onEditConnection = () => {
        let callback = (item, showMessageFunc) => {
            if (item.Name.trim().length > 255) {
                showMessageFunc(RMResx.RM_JS_Common_Msg_CannotExceed255);
                return;
            }
            $$.loading(true);
            let option = {
                url: this.saveConnectionUrl,
                method: "POST",
                data: item,
            };
            fetchUtility(option).then((res) => {
                let returnMessage = JSON.parse(res);
                if (returnMessage.MessageType === 1) {
                    showMessageFunc(returnMessage.ErrorMessage);
                } else {
                    this.setState({ isShowEditConnectionPanel: { show: false } });
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch(this.connectionSettingsPanelId, 'onSave', callback);
        return false;
    }
    
    renderImportSettingPanel() {
        return <R.Panel
            header={RMResx.RM_JS_SP_ImportSetting_Btn}
            size={670}
            status={this.state.isShowImportSettingsPanel}
            destroy={true}
        >
            <ImportSettingPanel
                id="importSettingPanel"
                source={SourceFlags.FS}
                downloadTemplateUrl="/api/BCMAdminSettingApi/DownloadFSTemplate"
                saveSettingUrl="/api/BCMAdminSettingApi/ImportFSSetting"
            ></ImportSettingPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelImportSettings} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveImportSettings} />
            </>
        </R.Panel>;
    }

    renderViewPermissionPanel() {
        return (
            <R.Panel
                header={RMResx.RM_JS_FS_ViewPermission_Btn}
                size={670}
                status={this.state.isShowViewPermissionPanel}
                destroy={true}
            >
                <ViewPermissionPanel
                    connection={this.state.connection}
                    onSort={this.handleSortConnection}
                ></ViewPermissionPanel>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={this.onClosePanelViewPermission}
                    />
                    {isMultiGeoMainDC && <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Edit}
                        onClick={this.onEditViewPermission}
                    />}
                </>
            </R.Panel>
        );
    }

    renderAuditTrailPanel() {
        return (
            <R.Panel
                header={RMResx.RM_FS_AuditTrail}
                size={670}
                status={this.state.isShowAuditTrailPanel}
                destroy={true}
            >
                <AuditTrailPanel id="auditTrailPanelId" />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Close}
                    onClick={this.onCloseAuditTrail}
                />
            </R.Panel>
        )
    }

    renderDownloadRccReportPanel() {
        return (
            <R.Panel
                header={RMResx.RM_FS_DownloadRCCReport}
                size={670}
                status={this.state.isShowDownloadRCCReportPanel}
                destroy={true}
            >
                <DownloadRccReportPanel id="downloadRCCReportPanelId" />
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={this.onCloseDownloadRCCReportPanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_FS_DownloadRCCReport_Btn}
                        onClick={this.onDownloadRCCReport}
                    />
                </>
            </R.Panel>
        )
    }

    confirmApplyCodeNow = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_FS_ClassCodePolicy_EnsureApply,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        this.setState({ isShowApplyClassCodePanel: { show: false } });
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => this.onSaveClassCodePolicy(true),
                },
            ],
        });
    }

    confirmNoClassCodeAppliedDialog = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_FS_ClassCode_NodeMissing,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
            ],
        });
    }

    renderApplyClassCodePanel() { 
        return (
            <R.Panel
                header={RMResx.RM_FS_ClassCodePolicy_ApplyClassCode}
                size={670}
                status={this.state.isShowApplyClassCodePanel}
                destroy={true}
            >
                <FSClassCode
                    id="classCodePolicyPanelId"
                    data={this.settingNode}
                    closePanel={this.onCloseClassCodePanel}
                />

                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCloseClassCodePanel}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_FS_ClassCode_ApplyButton}
                    onClick={this.dispatch.bind(this, 'classCodePolicyPanelId', 'onValidate', this.confirmApplyCodeNow)}
                />
            </R.Panel>
        );
    }

    renderEditConnectionPanel() {
        return (
            <R.Panel
                header={RMResx.RM_FS_EditPermissions_Title}
                size={670}
                status={this.state.isShowEditConnectionPanel}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="panel-description-header">{RMResx.RM_FS_EditPermission_SubTitle}</span>
                </div>
                <div>
                    <ConnectionSettings
                        id={this.connectionSettingsPanelId}
                        isEditPermission={true}
                    >
                    </ConnectionSettings>
                </div>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancelEditConnection}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onEditConnection}
                    />
                </>
            </R.Panel>
        );
    };

    renderClassificationSettingsPanel() {
        return <R.Panel
            header={RMResx.RM_JS_FS_ClassificationSetting_Btn}
            size={670}
            status={this.state.isShowClassificationSettingsPanel}
            destroy={true}
        >
            <ClassificationSettingPanel
                id="classificationSettingsPanel"
                source={SourceFlags.FS}
                initClassificationData={this.initClassificationSetting}
            >
            </ClassificationSettingPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelClassificationSettings} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveClassificationSettings} />
            </>
        </R.Panel>;
    }

    renderClassCodeSelectorPanel() { 
        return (
            <R.Panel
                header={RMResx.RM_JS_FS_DisposalOnSpecificClassCode}
                size={670}
                status={this.state.isShowClassCodeSelectorPanel}
                destroy={true}
            >
                <ClassCodeSelectorPanel id="classCodeSelectorPanelId" selectedNode={this.settingNode} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => this.setState({ isShowClassCodeSelectorPanel: false })} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_FS_Run} onClick={this.runEnforceRuleOnSpecificClassCodeJob} />
            </R.Panel>
        )
    }

    runSpecificClassCodeConfirmPopup = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_FS_RunEnforceRule_Confirm,
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
                        this.dispatch('classCodeSelectorPanelId', 'runJob', this.settingNode, () => {
                            this.setState({ isShowClassCodeSelectorPanel: { show: false } });
                        });
                    },
                },
            ],
        });
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_FS]} />
                <CheckAgentAvailable url={this.checkAgentUrl} />
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                ></TopButtonsComponent>
            </section>
            <section className="crm-content">
                <div className="ra-crm-splitter-container">
                    <R.Splitter minAsize={SplitterSize.minAsize} minBsize={SplitterSize.minBsize} defaultAsize={SplitterSize.defaultAsize}>
                        <div className="ra-splitter-left">
                            <div className="ra-splitter-header-left">
                                <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_LeftTitle}</div>
                            </div>
                            {( isEnableJPMCFeature &&
                                <div className="ra-splitter-searchbox">
                                    <R.Searchbox
                                        width={380}
                                        height={34}
                                        placeholder={RMResx.RM_FS_JPMC_SearchPlaceholder}
                                        disabled={false}
                                        onSearch={this.onSearch}
                                    />
                                </div>
                            )}
                            <div className="ra-splitter-tree">
                                <CRMFSTree
                                    ref={r => this.refCRMTree = r}
                                    searchKey={this.state.searchKey}
                                    onSelectedNodeChanged={this.onTreeChanged}
                                    onActiveClick={this.onActiveClick}
                                    onNodeRefresh={this.onNodeRefresh}
                                ></CRMFSTree>
                            </div>
                        </div>

                        <div className="ra-splitter-right">
                            <div style={{ fontSize: 0 }}>
                                <div style={{ width: this.state.isCustomSetting ? "calc(100% - 156px)" : "calc(100% - 24px)", display: "inline-block" }}>
                                    <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_RightTitle}</div>
                                    <div className="ra-splitter-header-name" data-tooltip="diffneed" aria-label={this.state.headerName}>
                                        {this.state.headerName != "" && <span className="fia-folder ra-splitter-folder"></span>}
                                        <span tabIndex="0" style={{ flex: 1 }} className="ra-ellipsis">{this.state.headerName}</span>
                                    </div>
                                </div>
                                {this.state.isCustomSetting && <div className="ra-splitter-unique-container" tabIndex="0" aria-label={RMResx.RM_JS_SPS_HasOwnSettingMessage}>
                                    <div
                                        id="showUniqueBtn"
                                        className="inline-block"
                                        style={{ lineHeight: "26px", marginRight: "8px" }}
                                    >
                                        <span className="fia-asterisk ra-splitter-unique-icon"></span>
                                        <span>{RMResx.RM_JS_SPS_Unique}</span>
                                    </div>
                                    <R.Popup of={'#showUniqueBtn'} arrow={true} triggerEvent="hover:300" position="right">
                                        <div>
                                            <div style={{ margin: "16px", width: "280px", fontSize: "14px" }}>
                                                <span>{RMResx.RM_JS_SPS_HasOwnSettingMessage}</span>
                                            </div>
                                        </div>
                                    </R.Popup>
                                </div>}
                            </div>
                            {!this.state.showRightSetting && <div className="ra-splitter-description" tabIndex="0">
                                <span>{RMResx.RM_JS_SPS_Tips}</span>
                            </div>}

                            {
                                this.state.showRightSetting && isEnableJPMCFeature
                                && (
                                    <div className="ra-divider-top padding-l">
                                        <FSDriveInformation selectedNode={this.settingNode} key={this.state.driveInformationComponentKey} />
                                    </div>
                                )
                            }
                            {
                                this.state.showRightSetting && (
                                    <div>
                                        {isEnableJPMCFeature && (
                                            <GeneralManagementComponent
                                                context={FileSystemGeneralManagement.getContext()}
                                                id={this.generalManagementComponent}
                                                ref={r => this.refGeneralManagementComponent = r}
                                                refreshNodeSettings={this.refreshNodeSettings}
                                                disabled={this.checkEditIsDisabled()}
                                                checkMissingConfig={this.checkMissingConfig}
                                                sourceFlag={SourceFlags.FS}
                                            ></GeneralManagementComponent>
                                        )}
                                        {(!isEnableJPMCFeature || this.state.enableClassification == EnableRecordManagementSetting.Enable) && (
                                            <>
                                                <DocumentTermSettingComponent
                                                    context={FileSystemDocumentTerm.getContext()}
                                                    id={this.documentTermSettingComponent}
                                                    refreshNodeSettings={this.refreshNodeSettings}
                                                    disabled={this.checkEditIsDisabled()}
                                                    checkMissingConfig={this.checkMissingConfig}
                                                    sourceFlag={SourceFlags.FS}
                                                    getClassificationData={this.getClassificationData}
                                                ></DocumentTermSettingComponent>
                                                <ManualApprovalSettingComponent
                                                    context={FSManualApprovalSetting.getContext()}
                                                    id={this.manualApprovalSettingComponent}
                                                    refreshNodeSettings={this.refreshNodeSettings}
                                                    disabled={this.checkEditIsDisabled()}
                                                    checkMissingConfig={this.checkMissingConfig}
                                                ></ManualApprovalSettingComponent>
                                                <ScheduleSettingComponent
                                                    context={FSScheduleSetting.getContext()}
                                                    id={this.scheduleSettingComponent}
                                                    refreshNodeSettings={this.refreshNodeSettings}
                                                    disabled={this.checkEditIsDisabled()}
                                                    checkMissingConfig={this.checkMissingConfig}
                                                ></ScheduleSettingComponent>
                                            </>
                                        )}
                                    </div>
                                )
                            }
                        </div>
                    </R.Splitter>
                </div>
                {this.state.initUniqueIdPanel &&
                    <UniqueIdSettingCommon
                        id="uniqueId"
                        ref={r => this.refUniqueIdComponent = r}
                        supportCustomColumn={false}
                        context={FSUniqueIdSetting.getContext()}
                        sourceFlag={SourceFlags.FS}
                    />}
                {this.state.initImportPanel && this.renderImportSettingPanel()}
                {this.state.initClassificationPanel && this.renderClassificationSettingsPanel()}
                {this.renderEditConnectionPanel()}
                {this.renderViewPermissionPanel()}
                {this.renderApplyClassCodePanel()}
                {this.renderAuditTrailPanel()}
                {this.renderClassCodeSelectorPanel()}
                {this.renderDownloadRccReportPanel()}
            </section>
        </div>;
    }
}
