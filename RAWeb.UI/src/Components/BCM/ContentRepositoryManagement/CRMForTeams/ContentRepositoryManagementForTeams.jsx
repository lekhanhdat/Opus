import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { NodeLevel } from "../../../../Constants/DAEnums";
import CRMCommonUtil, { OperationState, RAMessageType, SplitterSize } from "../Common/CRMCommonUtil";
import DocumentTermSettingComponent from "../DocumentTermSetting/DocumentTermSettingComponent";
import ColumnSettingComponent from '../ColumnSetting/ColumnSettingComponent';
import ManualApprovalSettingComponent from "../ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "../ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import GeneralManagementComponent from "../GeneralManagement/GeneralManagementComponent";
import ImportSettingPanel from "../ImportSettingPanel";
import "../../../../Less/BCM/ContentRepositoryManagement/common.less";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import ValidateMessageBar from "../Common/CommonMessageBar/ValidateMessageBar";
import CheckRemoteNodeMessageBar from "../Common/CommonMessageBar/CheckRemoteNodeMessageBar";
import UniqueIdSettingCommon from "../UniqueIdSetting/UniqueIdSettingCommon";
import { addTelemetryRecord } from "../../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../../Constants/Constants";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { TabIndex } from "../CRMForSPO";
import ConvertStubPanel from "../ConvertStubSetting/ConvertStubPanel";
import RouterUrls from "../../../../Constants/RouterUrls";
import TeamsGeneralManagement from "../GeneralManagement/Context/TeamsGeneralManagement";
import TeamsColumnSetting from "../ColumnSetting/Context/TeamsColumnSetting";
import TeamsDocumentTerm from "../DocumentTermSetting/Context/TeamsDocumentTerm";
import TeamsManualApprovalSetting from "../ManualApprovalSetting/Context/TeamsManualApprovalSetting";
import TeamsScheduleSetting from "../ScheduleSetting/Context/TeamsScheduleSetting";
import TeamsUniqueIdSetting from "../UniqueIdSetting/Context/TeamsUniqueIdSetting";
import CRMTeamsTree from "../../../Common/Tree/Instances/TeamsTree/CRMTeamsTree";
import ContainerTermSettingComponent from "../ContainerTermSetting/ContainerTermSettingComponent";
import TeamsContainerSetting from "../ContainerTermSetting/Context/TeamsContainerSetting";
import CustomMetadataPanel from "../CustomMetadataSetting/CustomMetadataPanel";
import { ExportSettingEnumType, getExportSettingTypes, getTeamsSearchPlaceholder, TeamsTreeBrowseType } from "../../Constants";
import MigrateDeclaredRecordsPanel from "../MigrateDeclaredRecordsSetting/MigrateDeclaredRecordsPanel";
import { pagerModes } from "../../../Common/Tree/Components/Constants";
import { CRMTeamsGroupEntireTree } from "../../../Common/Tree/Instances/TeamsTree";

const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2,
    ParentDisable: 3
};
const RunApplySettingMethod = {
    UpdatedScope: 1,
    AllScope: 2,
    Auto: 3,
    SelectedScope: 4
};
export default class ContentRepositoryManagementForTeams extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            showTip: false,
            tipType: 'success',
            tipMsg: '',
            items: [],
            searchKey: "",
            isShowColumnSettingsPanel: { show: false },
            isShowImportSettingsPanel: { show: false },
            isShowConvertStubPanel: { show: false },
            isShowMigrateDeclaredRecordsPanel: { show: false },
            isShowCustomMetadataPanel: { show: false },
            showRightSetting: false,
            enableClassification: "",
            enableSyncData: "",
            headerName: "",
            nodeLevel: "",
            initUniqueIdPanel: false,
            initImportPanel: false,
            initConvertStubPanel: false,
            initMigrateDeclaredRecordsPanel: false,
            initCustomMetadataPanel: false,
            initExportSettingDialog: false,
            skipActionInMessageBox: false,
            isEnableSuperUserDecryptInMessageBox: false,
            isCustomSetting: false,
            isUsingExistColumnName: false,
            setDocLevelTermForExistColumn: false,
            isCSDTenant: false,
            lastAccessTimeCollection: "",
            isShowExportSettingDialog: { show: false },
            exportSettingTypes: getExportSettingTypes(ExportSettingEnumType.CustomSetting, SourceFlags.Teams),
            selectedExportSettingType: ExportSettingEnumType.CustomSetting,
            browseType: TeamsTreeBrowseType.Container,
            shouldRenderSiteCollectionTree: false,
        };
        this.columnSettingComponent = "columnSettingComponent";
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.containerTermSettingComponent = "containerTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [
            {
                isGroup: true, id: "raCrmApplyGlobalSettingsBtnGroup", name: RMResx["RM_SPS_ApplyGlobalSettings"], buttons: [
                    { id: "raCrmApplyUpdatedScopeBtn", name: RMResx["RM_JS_SPS_ ApplyUpdatedScope"], onClick: this.applySettingMessageBox.bind(this, RunApplySettingMethod.UpdatedScope) },
                    { id: "raCrmApplyAllScopeBtn", name: RMResx["RM_JS_SPS_ ApplyAllScope"], onClick: this.applySettingMessageBox.bind(this, RunApplySettingMethod.AllScope) },
                ]
            }
        ];
        this.inheritButton = { id: "raCrmInheritParentBtn", name: RMResx["RM_SPS_InheritGlobalSettings"], icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { id: "raCrmRunRuleActionBtn", name: RMResx["RM_JS_SPS_DisposalNow"], icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.syncDataButton = { id: "raCrmRunDataSyncBtn", name: RMResx["RM_JS_SPS_CollectNow"], icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.convertStubButton = { id: "raCrmConvertStubBtn", name: RMResx.RM_JS_SP_ConvertStub_Btn, icon: "fia-convert-stub", onClick: this.showConvertStubClick.bind(this) };
        this.migrateDeclaredRecordsButton = { id: "raCrmMigrateDeclaredRecordsButtonBtn", name: RMResx.RM_JS_SP_MigrateDeclaredRecords_Btn, icon: "fia-change", onClick: this.showMigrateDeclaredRecordsClick.bind(this) };
        this.menuBtnItemsInMore = [];
        if (checkPermission("BCM_ContentRepositoryManagement_UniqueId", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id: "raCrmUniqueIdSettingBtn", name: RMResx.RM_JS_SP_UniqueIdSetting_Btn, icon: "fia-uniqueid", onClick: this.showUniqueIdSettingsClick.bind(this) });
        }
        if (checkPermission("BCM_ContentRepositoryManagement_Import", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id: "raCrmImportSettingsBtn", name: RMResx.RM_JS_SP_ImportSetting_Btn, icon: "fia-import", onClick: this.showImportSettingsClick.bind(this) });
        }
        if (checkPermission("BCM_ContentRepositoryManagement_Export", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id: "raCrmExportSettingsBtn", name: RMResx.RM_JS_SP_ExportSetting_Btn, icon: "fia-export-settings", onClick: this.handleExport });
        }
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && LicenseHelper.EnableRecordsArchiver()) {
            this.menuBtnItemsInMore.push({ id: "raCrmCustomMetadataBtn", name: RMResx.RM_JS_SP_CustomMetadata_Btn, icon: "fia-export-settings", onClick: this.showCustomMetadataClick });
        }
        this.showSkipAction = true;
        this.showUseDecrypt = RM.gData.enableRecordsArchiver;
        this.browseTypeOptions = [
            {
                name: RMResx.RM_PRM_PRE_ContainerTitle,
                value: TeamsTreeBrowseType.Container,
            },
            {
                name: RMResx.RM_PRM_PRE_GroupMailboxTitle,
                value: TeamsTreeBrowseType.Office365GroupEntire,
            }
        ];
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
        this.setState({
            initUniqueIdPanel: true,
            initImportPanel: true,
            initConvertStubPanel: true,
            initMigrateDeclaredRecordsPanel: true,
            initCustomMetadataPanel: true,
            initExportSettingDialog: true,
        });
        this.checkIsCSDTenant();
        this.loadLastAccessTimeCollectionData();
    }

    checkIsCSDTenant() {
        $$.loading(true);
        let urlData = "/api/RuleApi/CheckIsCSDTenant";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            this.setState({ isCSDTenant: res });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    applySettingDoAction = (runApplySettingMethod, hasRunningJob) => {
        $$.messagedialog(false);
        $$.loading(true);
        let url = runApplySettingMethod == RunApplySettingMethod.SelectedScope ? "/api/TeamsSettingApi/ApplySettingsOnSelectedNode" : "/api/TeamsSettingApi/ApplySettings";
        let data = runApplySettingMethod == RunApplySettingMethod.SelectedScope ? this.settingNode : { FromTimerJobPage: false, RunJobMethod: runApplySettingMethod };
        let option = {
            url: url,
            method: "Post",
            data: data
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                let showExtensionMessage = false;
                let extensionMessage = "";
                if (hasRunningJob) {
                    showToast.withTitle({
                        id: `toast-source-${SourceFlags.Teams}-apply-setting-is-running`,
                        title: <div className="font-semibold">{RMResx.RM_JS_SPS_RunApplySettingJobSucceedWithConflictJob_Title}</div>,
                        content: (
                            <$g.I18NProvider slot="content" msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceedWithConflictJob}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        ),
                        type: "success",
                    });
                } else if (runApplySettingMethod == RunApplySettingMethod.UpdatedScope) {
                    let settingsCount = parseInt(resultData.Extension, 10);
                    if (settingsCount > 0) {
                        showExtensionMessage = true;
                        extensionMessage = resultData.Extsion1;
                        let content = <div>
                            {showExtensionMessage && <div>{extensionMessage}
                            </div>}
                            <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        </div>;
                        showToast.success(content);
                    } else {
                        showToast.warn(resultData.Extsion1);
                    }
                } else if (runApplySettingMethod == RunApplySettingMethod.AllScope) {
                    let content = <div>
                        <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>
                    </div>;
                    showToast.success(content);
                } else if (runApplySettingMethod == RunApplySettingMethod.SelectedScope) {
                    let content = <div>
                        <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>
                    </div>;
                    showToast.success(content);
                }
            } else {
                let errorMsg = CRMCommonUtil.commonHandelErrorMessage(result);
                if (errorMsg != null && errorMsg != "") {
                    showToast.error(errorMsg);
                } else {
                    showToast.error(RMResx.RM_JS_TeamsGroups_FailedRunAppplyJobMsg);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.ApplySettings, [SourceFlags.Teams, runApplySettingMethod]);
    }

    getRunningJobConflictMessage = async (defaultResult) => {
        const option = {
            url: "/api/TeamsSettingApi/CheckRunningTeamsSettingJob",
            method: "GET",
        }
        const hasRunningJob = await fetchUtility(option);
        if (!!hasRunningJob) {
            return {
                hasRunningJob: hasRunningJob,
                classify: "",
                messageTitle: RMResx.RM_JS_SPS_ApplySettingJobConflictDetected_Title,
                messageContent: <div tabIndex={0}>{RMResx.RM_JS_SPS_ApplySettingJobConflictDetected}</div>,
            };
        }

        return defaultResult;
    }

    getIntelligentPermissionMessage = async (defaultResult) => {
        const option = {
            url: "/api/TrainingScopeApi/GetTrainingModelStatus",
            method: "Post",
            data: this.settingNode
        };
        const status = await fetchUtility(option);
        if (status != OperationState.Running) {
            return this.getRunningJobConflictMessage(defaultResult);
        }

        return {
            ...defaultResult,
            classify: "warn",
            messageContent: (
                <div>
                    <div>{RMResx.RM_MachineLearning_ModelIsTrainingWarn}</div>
                    <div>{RMResx.RM_JS_BCM_NTEAMS_EnsureApply}</div>
                </div>
            ),
        };
    }

    getApplySettingMessageBoxContent = async () => {
        const defaultResult = {
            hasRunningJob: false,
            classify: "",
            messageTitle: RMResx.RM_JS_Common_Confirmation,
            messageContent: RMResx.RM_JS_BCM_NTEAMS_EnsureApply,
        };

        if (RM.gData.hasIntelligentPermission) {
            return this.getIntelligentPermissionMessage(defaultResult);
        }

        if (LicenseHelper.EnableRecordsArchiver()) {
            return this.getRunningJobConflictMessage(defaultResult);
        }

        return defaultResult;
    }

    applySettingMessageBox = async (scopeType) => {
        const { hasRunningJob, classify, messageTitle, messageContent } = await this.getApplySettingMessageBoxContent();
        let args = {
            classify: classify,
            width: "550px",
            hideActions: false,
            title: messageTitle,
            content: messageContent,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmApplySettingDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.applySettingDoAction(scopeType, hasRunningJob);
                    }
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/TeamsSettingApi/InheritParentSettings",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                this.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.FaildType == 10 || resultData.FaildType == 11) {
                    this.refreshNodeSettings();
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    inheritParentMessageBox = () => {
        let args = {
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

    runDataSyncDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/TeamsSettingApi/RunCollectionJob",
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

    loadLastAccessTimeCollectionData = () => {
        $$.loading(true);
        let option = {
            url: "/api/RuleApi/GetLATEnableTime",
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            this.setState({
                lastAccessTimeCollection: res
            })
        }).finally(() => $$.loading(false));
    }

    runDataSyncMessageBox() {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NTEAMS_EnsureSync,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunDataSyncDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runDataSyncDoAction.bind(this)
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    runRuleActionDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        this.settingNode.SkipRemoveContentAndDestroyAction = this.state.skipActionInMessageBox;
        this.settingNode.IsEnableSuperUserDecrypt = this.state.isEnableSuperUserDecryptInMessageBox;
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: "/api/DAMApi/RunTeamsJob",
            method: "Post",
            data: JSON.stringify(this.settingNode)
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
                } else if (result.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_FTPExportLocationNotSupported) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_FTPExportLocationNotSupported);
                } else {
                    showToast.error(result.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.Teams]);
    }

    runRuleActionMessageBox() {
        this.setState({ skipActionInMessageBox: false });
        this.setState({ isEnableSuperUserDecryptInMessageBox: false });
        let args = {
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
                {this.showUseDecrypt && <div className="ra-inline-middle margin-bottom-m">
                    <R.Checkbox
                        text={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                        title={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                        checked={this.state.isEnableSuperUserDecryptInMessageBox}
                        onChange={this.isEnableSuperUserDecryptInMessageBoxChanged.bind(this)}
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

    isEnableSuperUserDecryptInMessageBoxChanged(args) {
        this.setState({ isEnableSuperUserDecryptInMessageBox: args });
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
            url: "/api/TeamsSettingApi/LoadSampleNodeSettings",
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
                settingNode.DisplayName = nodeItem.DisplayName;
                if (settingNode.IconStatus == 0) {
                    settingNode.EnableRecordManagement = EnableRecordManagementSetting.Enable;
                }
                let text = settingNode.Name;
                if (text == "." && settingNode.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(settingNode.Title);
                }
                if (this.currNode.TeamName) {
                    text = "(" + this.currNode.TeamName + ")" + this.currNode.Name;
                }
                if (this.state.isCSDTenant) {
                    if (settingNode.ColumnName != null && settingNode.ColumnName != "") {
                        settingNode.ColumnName = "CSD Class";
                    }
                }
                this.setState({
                    showRightSetting: true,
                    headerName: text,
                    nodeLevel: settingNode.Level,
                    enableClassification: settingNode.EnableRecordManagement,
                    enableSyncData: settingNode.IsSyncData,
                    isCustomSetting: settingNode.IsCustomSetting,
                    isUsingExistColumnName: settingNode.IsUsingExistColumnName,
                    setDocLevelTermForExistColumn: settingNode.SetDocLevelTermForExistColumn,

                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
                    this.dispatch(this.columnSettingComponent, 'columnData', settingNode);
                    this.dispatch(this.containerTermSettingComponent, 'containerData', settingNode);
                    this.dispatch(this.manualApprovalSettingComponent, 'manualApprovalData', settingNode);
                    this.dispatch(this.scheduleSettingComponent, 'scheduleData', settingNode);
                    this.refGeneralManagementComponent && this.refGeneralManagementComponent.initData(settingNode);
                });
                let updateProps = { IconStatus: settingNode.IconStatus };
                if (nodeItem.IconStatus == 0 && settingNode.IconStatus == 2 || reload) {
                    this.refCRMTree.refreshSelectedNode(updateProps, true);
                } else {
                    this.refCRMTree.refreshSelectedNode(updateProps);
                }

                let buttons = [...this.menuBtnItems[0].buttons];
                if (this.state.isCustomSetting || (CRMCommonUtil.isGroup(this.settingNode) && settingNode.IconStatus != 0)) {
                    buttons.unshift({ id: "raCrmApplySelectedScopeBtn", name: RMResx.RM_JS_SPS_ApplySelectedScope, onClick: this.applySettingMessageBox.bind(this, RunApplySettingMethod.SelectedScope) });
                }
                let menuButtons = [...this.menuBtnItems];
                menuButtons[0] = {
                    isGroup: true, id: "raCrmApplyGlobalSettingsBtnGroup", name: RMResx["RM_SPS_ApplyGlobalSettings"], buttons: buttons
                };
                if (settingNode.IsCustomSetting) {
                    menuButtons.push(this.inheritButton);
                }
                let enableRunJob = settingNode.EnableRecordManagement == EnableRecordManagementSetting.Enable && ((settingNode.ColumnName != null && settingNode.ColumnName != "") || settingNode.IsUsingExistColumnName);
                if (enableRunJob) {
                    menuButtons.push(this.ruleActionButton);
                }
                if (enableRunJob && settingNode.IsSyncData && (CRMCommonUtil.isGroup(settingNode) || CRMCommonUtil.isTeams(settingNode))) {
                    menuButtons.push(this.syncDataButton);
                }

                let menuButtonsInMore = [...this.menuBtnItemsInMore];
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && RM.gData.enableRecordsArchiver && (CRMCommonUtil.isGroup(settingNode) || CRMCommonUtil.isTeams(settingNode) || CRMCommonUtil.isSiteCollection(settingNode))) {
                    menuButtonsInMore.push(this.convertStubButton);
                }
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver() && CRMCommonUtil.isGroup(settingNode)) {
                    menuButtonsInMore.push(this.migrateDeclaredRecordsButton);
                }
                menuButtons.push(...menuButtonsInMore);
                this.refTopButtons.updateButtons(menuButtons);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleApplySetting = () => {
        this.refreshSelectedNode();
    }

    onSearch = (args) => {
        this.setState({
            searchKey: args,
            shouldRenderSiteCollectionTree: this.state.browseType === TeamsTreeBrowseType.Office365GroupEntire && args != "",
        });
    }

    onBrowseTypeChange = (args) => {
        const browseType = args && args.newValue ? args.newValue.value : TeamsTreeBrowseType.Office365GroupEntire;
        
        if (this.searchBoxRef) {
            this.searchBoxRef.clear?.();
        }

        this.setState({
            browseType,
            searchKey: "",
        });
    }

    getPagerMode = () => {
        if (this.state.browseType === TeamsTreeBrowseType.Container) {
            return pagerModes.normal;
        }
        return pagerModes.loadMore;
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    checkMissingConfig = () => {
        if (!this.settingNode.ColumnName && !this.settingNode.IsUsingExistColumnName) {
            let args = {
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SPS_ColumnSettingMissing,
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

    showUniqueIdSettingsClick() {
        this.refUniqueIdComponent.showUniqueIdSettingsPanel();
    }

    showImportSettingsClick() {
        this.setState({ isShowImportSettingsPanel: { show: true } });
    }

    showConvertStubClick() {
        this.setState({ isShowConvertStubPanel: { show: true } });
    }

    showMigrateDeclaredRecordsClick() {
        this.setState({ isShowMigrateDeclaredRecordsPanel: { show: true } });
    }

    handleExport = () => {
        this.setState({
            isShowExportSettingDialog: { show: true },
        });
    }

    handleHideExportSettingDialog = () => {
        this.setState({
            isShowExportSettingDialog: { show: false },
        });
    }

    getCustomIndexMetadatas = async () => {
        $$.loading(true);
        const option = {
            url: "/api/SPSettingApi/GetCustomIndexMetadatasBySourceFlag",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    // Get manage metadata list
    getCustomMetadataColumns = async () => {
        $$.loading(true);
        const option = {
            url: "/api/SPSettingApi/GetCustomMetadataColumns",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    getInUsedCustomMetadataColumns = async () => {
        $$.loading(true);
        const option = {
            url: "/api/SPSettingApi/GetInUsedCustomMetadataColumns",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    showCustomMetadataClick = async () => {
        this.setState({ isShowCustomMetadataPanel: { show: true } });
        const [indexData, manageData, inUsedColumnData] = await Promise.all([this.getCustomIndexMetadatas(), this.getCustomMetadataColumns(), this.getInUsedCustomMetadataColumns()]);
        this.dispatch("customMetadataPanel", "isOpenPanel", { indexData, manageData, inUsedColumnData });
    }

    onExportTeamsSetting = () => {
        const option = {
            url: "/api/BCMAdminTeamsSettingApi/ExportTeamsSetting",
            method: "POST",
            data: this.state.selectedExportSettingType,
        };
        $$.loading(true);

        fetchUtility(option).then((result) => {
            if (result.MessageType === RAMessageType.Failed) {
                showToast.error(result.ErrorMessage);
            }
            else {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                        <a className="ra-link-a" href="/Root/DC/Download">
                            {RMResx.RM_JS_DC_Title}
                        </a>
                    </$g.I18NProvider>
                );
                this.handleHideExportSettingDialog();
            }
        }).finally(() => $$.loading(false));
    }

    saveImportSettings = (e) => {
        this.dispatch("importSettingPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowImportSettingsPanel: { show: false } });
            }
        });
        return false;
    }

    cancelImportSettings = () => {
        this.setState({ isShowImportSettingsPanel: { show: false } });
    }

    saveConvertStub = (e) => {
        this.dispatch("convertStubPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowConvertStubPanel: { show: false } });
            }
        });
        return false;
    }

    cancelConvertStub = () => {
        this.setState({ isShowConvertStubPanel: { show: false } });
    }
    
    saveMigrateDeclaredRecords = (e) => {
        this.dispatch("migrateDeclaredRecordsPanel", 'onValidate', (isValid) => {
            if (isValid) {
                $$.messagedialog(true, {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: (
                        <div tabIndex={0}>
                            {RMResx.RM_JS_SP_MigrateDeclaredRecords_ConfirmMsg}
                        </div>
                    ),
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
                            onClick: this.onSaveMigrateDeclaredRecords.bind(this),
                        },
                    ],
                });
            }
        });
        return false;
    }

    onSaveMigrateDeclaredRecords = () => {
        this.dispatch("migrateDeclaredRecordsPanel", 'onSave', (success, data) => {
            if (success) {
                $$.messagedialog(false);
                this.setState({ isShowMigrateDeclaredRecordsPanel: { show: false } });
            }
        });
        return false;
    }

    cancelMigrateDeclaredRecords = () => {
        this.setState({ isShowMigrateDeclaredRecordsPanel: { show: false } });
    }

    saveCustomMetadata = (e) => {
        this.dispatch("customMetadataPanel", "save");
    }

    cancelCustomMetadata = () => {
        this.setState({ isShowCustomMetadataPanel: { show: false } });
    }

    handleChangeExportSettingOption = (newValue) => {
        this.setState({
            selectedExportSettingType: newValue,
            exportSettingTypes: getExportSettingTypes(newValue, SourceFlags.Teams),
        });
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
                source={SourceFlags.Teams}
                downloadTemplateUrl="/api/BCMAdminTeamsSettingApi/DownloadTeamsTemplate"
                saveSettingUrl="/api/BCMAdminTeamsSettingApi/ImportTeamsSetting"
            ></ImportSettingPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelImportSettings} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveImportSettings} />
            </>
        </R.Panel>;
    }

    renderConvertStubPanel() {
        return <R.Panel
            header={RMResx.RM_JS_SP_ConvertStub_Btn}
            size={670}
            status={this.state.isShowConvertStubPanel}
            destroy={true}
        >
            <ConvertStubPanel
                id="convertStubPanel"
                source={SourceFlags.Teams}
                treeData={this.settingNode}
            ></ConvertStubPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelConvertStub} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_TimerJob_Run} onClick={this.saveConvertStub} />
            </>
        </R.Panel>;
    }

    renderMigrateDeclaredRecordsPanel() {
        return (
            <R.Panel
                header={RMResx.RM_JS_SP_MigrateDeclaredRecords_Btn}
                size={670}
                status={this.state.isShowMigrateDeclaredRecordsPanel}
                destroy={true}
                onClose={this.cancelMigrateDeclaredRecords}
            >
                <MigrateDeclaredRecordsPanel
                    id="migrateDeclaredRecordsPanel"
                    source={SourceFlags.SP}
                    treeData={this.settingNode}
                ></MigrateDeclaredRecordsPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelMigrateDeclaredRecords} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_SP_MigrateDeclaredRecords_MigrateBtn} onClick={this.saveMigrateDeclaredRecords} />
                </>
            </R.Panel>
        );
    }

    renderCustomMetadataPanel() {
        return (
            <R.Panel
                header={RMResx.RM_JS_SP_CustomMetadata_Btn}
                size={670}
                status={this.state.isShowCustomMetadataPanel}
                destroy={true}
            >
                <CustomMetadataPanel id="customMetadataPanel" getCustomMetadataColumns={this.getCustomMetadataColumns} onClose={this.cancelCustomMetadata} />
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelCustomMetadata} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveCustomMetadata} />
                </>
            </R.Panel>
        );
    }

    renderExportSettingDialog = () => {
        return (
            <R.Dialog
                id="exportSettingDialog"
                header={RMResx.RM_JS_SP_ExportSetting_Title}
                width={550}
                status={this.state.isShowExportSettingDialog}
                struct={{ foot: true }}
                onHide={this.handleHideExportSettingDialog}
                destroy={true}
            >
                <div id="export-setting-dialog">
                    <div tabIndex={0} className="font-semibold">
                        {RMResx.RM_JS_SP_ExportSetting_SelectOption}
                    </div>
                    <div style={{ marginTop: 12 }}>
                        <R.Radio.Group
                            id="export-setting-radio"
                            block
                            name="export-setting"
                            items={this.state.exportSettingTypes}
                            onChange={this.handleChangeExportSettingOption}
                        />
                    </div>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleHideExportSettingDialog} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_SP_ExportSetting_ExportBtn} onClick={this.onExportTeamsSetting} />
                </>
            </R.Dialog>
        );
    }

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    renderCRMTeamsTree() {
        const shouldRenderGroupEntireTree = this.state.shouldRenderSiteCollectionTree;

        if (!shouldRenderGroupEntireTree) {
            return <CRMTeamsTree
                ref={r => this.refCRMTree = r}
                treeSource={SourceFlags.Teams}
                searchKey={this.state.searchKey}
                browseType={this.state.browseType}
                mode={TabIndex.Records}
                pagerMode={this.getPagerMode()}
                onSelectedNodeChanged={this.onTreeChanged}
                onNodeRefresh={this.onNodeRefresh}
            ></CRMTeamsTree>;
        } else {
            return <CRMTeamsGroupEntireTree 
                ref={r => this.refCRMTree = r}
                treeSource={SourceFlags.Teams}
                searchKey={this.state.searchKey}
                browseType={this.state.browseType}
                mode={TabIndex.Records}
                pagerMode={this.getPagerMode()}
                onSelectedNodeChanged={this.onTreeChanged}
                onNodeRefresh={this.onNodeRefresh}
            />;
        }
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_Teams]} />
                <ValidateMessageBar />
                <div className="margin-top-m"><CheckRemoteNodeMessageBar treeSource={SourceFlags.Teams} /></div>
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                    showCount={4}
                ></TopButtonsComponent>
            </section>
            <section className="crm-content">
                <div className="ra-crm-splitter-container">
                    <R.Splitter minAsize={SplitterSize.minAsize} minBsize={SplitterSize.minBsize} defaultAsize={SplitterSize.defaultAsize}>
                        <div className="ra-splitter-left">
                            <div className="ra-splitter-header-leftHight">
                                <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_LeftTitle}</div>
                                {this.props.tabControl && <div className="ra-splitter-header-tabcontrol">{this.props.tabControl}</div>}
                            </div>
                            <div className="ra-splitter-searchbox">
                                <div className="flex">
                                    <R.Combobox
                                        width={160}
                                        height={34}
                                        items={this.browseTypeOptions.map((item) => ({
                                            ...item,
                                            checked: item.value === this.state.browseType,
                                        }))}
                                        textField="name"
                                        valueField="value"
                                        checkedField="checked"
                                        searchable={false}
                                        linkMode={false}
                                        excludeChecked={false}
                                        onChange={this.onBrowseTypeChange}
                                    />
                                    <R.Searchbox
                                        ref={r => this.searchBoxRef = r}
                                        width={252}
                                        height={34}
                                        placeholder={getTeamsSearchPlaceholder(this.state.browseType)}
                                        disabled={false}
                                        onSearch={this.onSearch}
                                    />
                                </div>
                            </div>
                            <div className="ra-splitter-tree">
                                {this.renderCRMTeamsTree()}
                            </div>
                        </div>

                        <div className="ra-splitter-right">
                            <div style={{ fontSize: 0 }}>
                                <div style={{ width: "calc(100% - 156px)", display: "inline-block" }}>
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

                            {this.settingNode && this.state.showRightSetting && <div>
                                <GeneralManagementComponent
                                    context={TeamsGeneralManagement.getContext()}
                                    id="generalManagementComponent"
                                    ref={r => this.refGeneralManagementComponent = r}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    sourceFlag={SourceFlags.Teams}
                                ></GeneralManagementComponent>
                            </div>}

                            {this.state.showRightSetting && this.state.enableClassification == EnableRecordManagementSetting.Enable && <div>
                                <ColumnSettingComponent
                                    context={TeamsColumnSetting.getContext()}
                                    isCSDTenant={this.state.isCSDTenant}
                                    id={this.columnSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    sourceFlag={SourceFlags.Teams}
                                ></ColumnSettingComponent>
                                {((!this.state.isUsingExistColumnName) || (this.state.isUsingExistColumnName && this.state.setDocLevelTermForExistColumn)) &&
                                    <DocumentTermSettingComponent
                                        context={TeamsDocumentTerm.getContext()}
                                        id={this.documentTermSettingComponent}
                                        refreshNodeSettings={this.refreshNodeSettings}
                                        checkMissingConfig={this.checkMissingConfig}
                                        sourceFlag={SourceFlags.Teams}
                                        isCSDTenant={this.state.isCSDTenant}
                                        lastAccessTimeCollection={this.state.lastAccessTimeCollection}
                                    ></DocumentTermSettingComponent>
                                }
                                {!CRMCommonUtil.isFolder(this.settingNode) && <ContainerTermSettingComponent
                                    context={TeamsContainerSetting.getContext()}
                                    id={this.containerTermSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ContainerTermSettingComponent>}
                                <ManualApprovalSettingComponent
                                    context={TeamsManualApprovalSetting.getContext()}
                                    id={this.manualApprovalSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ManualApprovalSettingComponent>
                                <ScheduleSettingComponent
                                    context={TeamsScheduleSetting.getContext()}
                                    id={this.scheduleSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ScheduleSettingComponent>
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
                {this.state.initUniqueIdPanel &&
                    <UniqueIdSettingCommon
                        id="uniqueId"
                        ref={r => this.refUniqueIdComponent = r}
                        supportCustomColumn={true}
                        context={TeamsUniqueIdSetting.getContext()}
                        sourceFlag={SourceFlags.Teams}
                    />}
                {this.state.initImportPanel && this.renderImportSettingPanel()}
                {this.state.initConvertStubPanel && this.renderConvertStubPanel()}
                {this.state.initMigrateDeclaredRecordsPanel && this.renderMigrateDeclaredRecordsPanel()}
                {this.state.initCustomMetadataPanel && this.renderCustomMetadataPanel()}
                {this.state.initExportSettingDialog && this.renderExportSettingDialog()}
            </section>
        </div>
    }
}