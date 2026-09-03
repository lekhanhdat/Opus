import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { NodeLevel } from "../../../../Constants/DAEnums";
import SPCRMTree from "../../../Common/Tree/Instances/SPTree/CRMSPTree";
import SPSiteCollectionTree from "../../../Common/Tree/Instances/SPTree/CRMSPSiteCollectionTree";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "../Common/CRMCommonUtil";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import "../../../../Less/BCM/ContentRepositoryManagement/common.less";
import { LicenseHelper, ServiceHelper, showToast } from "../../../../Utilities/CommonUtil";
import ValidateMessageBar from "../Common/CommonMessageBar/ValidateMessageBar";
import CheckRemoteNodeMessageBar from "../Common/CommonMessageBar/CheckRemoteNodeMessageBar";
import { addTelemetryRecord } from "../../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule, BrowseTreeNodeSourceType } from "../../../../Constants/Constants";
import ArchiveSettingComponent from "../ArchiveSetting/ArchiveSettingComponent";
import SPOArchiveSetting from "../ArchiveSetting/Context/SPOArchiveSetting";
import { TabIndex } from "../CRMForSPO";
import ScheduleSettingComponent from "../ScheduleSetting/ScheduleSettingComponent";
import ArchiveSPOScheduleSetting from "../ScheduleSetting/Context/ArchiveSPOScheduleSetting";
import GeneralSettingComponent from "../ArchiveGeneralSetting/GeneralSettingComponent";
import SPOGeneralSetting from "../ArchiveGeneralSetting/Context/SPOGeneralSetting";
import ImportSCPanel from "../ImportSCSetting/ImportSCPanel";
import ConvertStubPanel from "../ConvertStubSetting/ConvertStubPanel";
import RouterUrls from "../../../../Constants/RouterUrls";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { ExportSettingEnumType, getExportSettingTypes, getSOSearchPlaceholder, SPOTreeBrowseType } from "../../Constants";
import MigrateDeclaredRecordsPanel from "../MigrateDeclaredRecordsSetting/MigrateDeclaredRecordsPanel";
import { pagerModes } from "../../../Common/Tree/Components/Constants";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2,
    ParentDisable: 3
};

const RunJobButtonType = {
    ArchiverJobBtn: 1,
    ScanJobBtn: 2,
};

export default class ArchiveCRMForSPO extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            showTip: false,
            tipType: 'success',
            tipMsg: '',
            items: [],
            showRightSetting: false,
            enableClassification: "",
            headerName: "",
            nodeLevel: "",
            isCustomSetting: false,
            needLoadRules: true,
            availableRules: [],
            searchKey : "", 
            isShowImportSCPanel: { show: false },
            isShowConvertStubPanel: { show: false },
            isShowMigrateDeclaredRecordsPanel: { show: false },
            isShowExportSettingDialog: { show: false },
            exportSettingTypes: getExportSettingTypes(ExportSettingEnumType.CustomSetting, SourceFlags.SP),
            selectedExportSettingType: ExportSettingEnumType.CustomSetting,
            browseType: SPOTreeBrowseType.Container,
            shouldRenderSiteCollectionTree: false,
        };
        this.archiveGeneralSettingComponent = "archiveGeneralSettingComponent";
        this.archiveSettingComponent = "archiveSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [];
        this.runSOJobButtons = {
            isGroup: true, id: "raCrmRunSOJobBtnGroup", name: RMResx.RM_AR_SPS_RunNow, buttons: [
                { id: "raCrmRunRuleActionBtn", name: RMResx.RM_AR_SPS_RunJobBtn, onClick: this.runJobMessageBox.bind(this, RunJobButtonType.ArchiverJobBtn) },
                { id: "raCrmRunImportSCBtn", name: RMResx.RM_AR_SPS_ImportSC, onClick: this.showImportSCClick.bind(this) },
            ]
        };
        this.ruleActionButton = { isStatic: true, id: "raCrmRunRuleActionBtn", name: RMResx.RM_AR_SPS_RunNow, onClick: this.runJobMessageBox.bind(this, RunJobButtonType.ArchiverJobBtn) };
        this.scanButton = { id: "raCrmScanBtn", name: RMResx.RM_AR_SPS_Scan, icon: "fia-scan", onClick: this.runJobMessageBox.bind(this, RunJobButtonType.ScanJobBtn) };
        this.inheritButton = { id: "raCrmInheritParentBtn", name: RMResx["RM_SPS_InheritGlobalSettings"], icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.inheritSubnodeButton = { id: "raCrmInheritSubnodeBtn", name: RMResx.RM_AR_SPS_InheritSubnode, icon: "fia-push-parent-settings", onClick: this.inheritSubnodeMessageBox };
        this.convertStubButton = { id: "raCrmConvertStubBtn", name: RMResx.RM_JS_SP_ConvertStub_Btn, icon: "fia-convert-stub", onClick: this.showConvertStubClick.bind(this) };
        this.migrateDeclaredRecordsButton = { id: "raCrmMigrateDeclaredRecordsButtonBtn", name: RMResx.RM_JS_SP_MigrateDeclaredRecords_Btn, icon: "fia-change", onClick: this.showMigrateDeclaredRecordsClick.bind(this) };
        this.menuBtnItemsInMore = [];
        if (checkPermission("BCM_ContentRepositoryManagement_ExportSO", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id: "raCrmExportSettingsBtn", name: RMResx.RM_JS_SP_ExportSetting_Btn, icon: "fia-export-settings", onClick: this.handleExport });
        }
        this.browseTypeOptions = [
            {
                name: RMResx.RM_PRM_PRE_ContainerTitle,
                value: SPOTreeBrowseType.Container,
            },
            {
                name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection,
                value: SPOTreeBrowseType.SiteCollection,
            }
        ];
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
    }

    onTreeChanged = (nodeItem) => {
        this.currNode = nodeItem;
        this.loadNodeSettings();
        this.loadRules();
    }

    onSearch = (args) => {
        this.setState({
            searchKey: args,
            shouldRenderSiteCollectionTree: this.state.browseType === SPOTreeBrowseType.SiteCollection && args != "",
        });
    }

    onBrowseTypeChange = (args) => {
        const browseType = args && args.newValue ? args.newValue.value : SPOTreeBrowseType.SiteCollection;
        
        if (this.searchBoxRef) {
            this.searchBoxRef.clear?.();
        }

        this.setState({
            browseType,
            searchKey: "",
        });
    }

    getPagerMode = () => {
        if (this.state.browseType === SPOTreeBrowseType.Container) {
            return pagerModes.normal;
        }
        return pagerModes.loadMore;
    }

    renderSPOTree() {
        if (!this.state.shouldRenderSiteCollectionTree) {
            return <SPCRMTree
                ref={r => this.refCRMTree = r}
                treeSource={SourceFlags.SP}
                searchKey={this.state.searchKey}
                browseType={this.state.browseType}
                mode={TabIndex.Archive}
                pagerMode={this.getPagerMode()}
                onSelectedNodeChanged={this.onTreeChanged}
                onNodeRefresh={this.onNodeRefresh}
                dataSource={BrowseTreeNodeSourceType.SharePointOnline}
            ></SPCRMTree>;
        }

        return <SPSiteCollectionTree
            ref={r => this.refCRMTree = r}
            treeSource={SourceFlags.SP}
            searchKey={this.state.searchKey}
            browseType={this.state.browseType}
            mode={TabIndex.Archive}
            pagerMode={this.getPagerMode()}
            onSelectedNodeChanged={this.onTreeChanged}
            onNodeRefresh={this.onNodeRefresh}
            dataSource={BrowseTreeNodeSourceType.SharePointOnline}
        />;
    }

    loadRules = async (reload) => {
        if (this.state.needLoadRules || reload) {
            $$.loading(true);
            try {
                const groupNode = CRMCommonUtil.getGroupNode(this.currNode);
                const result = await fetchUtility({ url: '/api/SPSettingApi/LoadArchiverRules', method: 'post', data: groupNode.Id });
                if (result) {
                    this.setState({ availableRules: result });
                }
                $$.loading(false);
            } catch (error) {
                $$.loading(false);
            }
        }
    };

    loadNodeSettings(reload) {
        let nodeItem = Object.assign({}, this.currNode, { Children: null, ChildrenIds: null });
        let currentItem = nodeItem;
        while (currentItem.Parent) {
            currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
            currentItem = currentItem.Parent;
        }
        $$.loading(true);
        let option = {
            url: "/api/SPSettingApi/LoadArchiverNodeSettings",
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
                if (settingNode.IconStatus == 0) {
                    settingNode.EnableArchiverManagement = EnableRecordManagementSetting.Enable;
                }
                let text = settingNode.Name;
                if (text == "." && settingNode.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(settingNode.Title);
                }
                if (this.currNode.TeamName) {
                    text = "(" + this.currNode.TeamName + ")" + this.currNode.Name;
                }
                this.setState({
                    showRightSetting: true,
                    headerName: text,
                    nodeLevel: settingNode.Level,
                    enableClassification: settingNode.EnableArchiverManagement,
                    isCustomSetting: settingNode.IsCustomSetting,
                }, () => {
                    this.dispatch(this.archiveGeneralSettingComponent, 'initGeneralSetting', settingNode);
                    this.dispatch(this.archiveSettingComponent, 'init', settingNode);
                    this.dispatch(this.scheduleSettingComponent, 'scheduleData', settingNode);
                });
                let updateProps = { IconStatus: settingNode.IconStatus };
                if (nodeItem.IconStatus == 0 && settingNode.IconStatus == 2 || reload) {
                    this.refCRMTree.refreshSelectedNode(updateProps, true);
                } else {
                    this.refCRMTree.refreshSelectedNode(updateProps);
                }

                let menuButtons = [...this.menuBtnItems];
                let canRunJob = settingNode.EnableArchiverManagement == EnableRecordManagementSetting.Enable && settingNode.Rules;
                let enableRunJob = canRunJob && ((CRMCommonUtil.isGroup(this.settingNode) || settingNode.IsCustomSetting));
                let isShowRunJobBtnGroup = LicenseHelper.EnableRecordsArchiver() && ServiceHelper.CanArchiverImportSC() && CRMCommonUtil.isGroup(this.settingNode) && canRunJob;
                if (isShowRunJobBtnGroup) {
                    menuButtons.push(this.runSOJobButtons);
                } else if (enableRunJob) {
                    menuButtons.push(this.ruleActionButton);
                }
                if (settingNode.IsCustomSetting) {
                    menuButtons.push(this.inheritButton);
                }
                if (settingNode.Level === NodeLevel.WebApplication || settingNode.Level === NodeLevel.SiteCollection) {
                    menuButtons.push(this.inheritSubnodeButton);
                }
                if (enableRunJob) {
                    menuButtons.push(this.scanButton);
                }

                let menuButtonsInMore = [...this.menuBtnItemsInMore];
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && (CRMCommonUtil.isGroup(this.settingNode) || CRMCommonUtil.isSiteCollection(this.settingNode))) {
                    menuButtonsInMore.push(this.convertStubButton);
                }
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver() && CRMCommonUtil.isGroup(this.settingNode)) {
                    menuButtonsInMore.push(this.migrateDeclaredRecordsButton);
                }
                menuButtons.push(...menuButtonsInMore);
                this.refTopButtons.updateButtons(menuButtons);
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

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/SPSettingApi/InheritParentArchiverSettings",
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

    inheritSubnodeMessageBox = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_SPS_InheritSubnode_Msg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmInheritSubnodeDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.inheritSubnodeDoAction.bind(this)
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    inheritSubnodeDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/SPSettingApi/InheritSubNodeToCurrentSettings",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                this.refreshNodeSettings(true);
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else {
                this.refreshNodeSettings();
                showToast.error(resultData.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    runJobMessageBox(buttonType) {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m" tabIndex="0">{buttonType === RunJobButtonType.ArchiverJobBtn ? RMResx.RM_AR_SPS_RunNow_Msg : RMResx.RM_AR_SPS_Scan_Msg}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunJobDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runJobDoAction.bind(this, buttonType)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    runJobDoAction(buttonType) {
        $$.messagedialog(false);
        $$.loading(true);
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: buttonType === RunJobButtonType.ArchiverJobBtn ? "/api/SPSettingApi/RunArchiverJob" : "/api/SPSettingApi/RunSOPreScanJob",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let result = JSON.parse(res);
            if (result.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (result.MessageType == RAMessageType.Failed) {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    showImportSCClick() {
        this.setState({ isShowImportSCPanel: { show: true } });
    }

    showConvertStubClick() {
        this.setState({ isShowConvertStubPanel: { show: true } });
    }

    showMigrateDeclaredRecordsClick() {
        this.setState({ isShowMigrateDeclaredRecordsPanel: { show: true } });
    }

    onSaveImportSC = (e) => {
        this.dispatch("importSCPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowImportSCPanel: { show: false } });
            }
        });
        return false;
    }

    onCancelImportSC = () => {
        this.setState({ isShowImportSCPanel: { show: false } });
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

    handleChangeExportSettingOption = (newValue) => {
        this.setState({
            selectedExportSettingType: newValue,
            exportSettingTypes: getExportSettingTypes(newValue, SourceFlags.SP),
        });
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

    onExportArchiveSPSetting = () => {
        const option = {
            url: "/api/BCMAdminSettingApi/ExportSPSOSetting",
            method: "POST",
            data: this.state.selectedExportSettingType
        };
        $$.loading(true);

        fetchUtility(option).then((result) => {
            if (result.MessageType === RAMessageType.Failed) {
                showToast.error(result.ErrorMessage);
            } else {
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

    renderImportSCPanel() {
        return <R.Panel
            header={RMResx.RM_AR_SPS_ImportSC}
            size={670}
            status={this.state.isShowImportSCPanel}
            destroy={true}
        >
            <ImportSCPanel
                id="importSCPanel"
                source={SourceFlags.SP}
                treeData={this.settingNode}
            ></ImportSCPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelImportSC} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_TimerJob_Run} onClick={this.onSaveImportSC} />
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
                source={SourceFlags.SP}
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
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_SP_ExportSetting_ExportBtn} onClick={this.onExportArchiveSPSetting} />
                </>
            </R.Dialog>
        );
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_SPO]} />
                {LicenseHelper.EnableRecordsArchiver() && <ValidateMessageBar />}
                <CheckRemoteNodeMessageBar />
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
                                        placeholder={getSOSearchPlaceholder(this.state.browseType)}
                                        disabled={false}
                                        onSearch={this.onSearch}
                                    />
                                </div>
                            </div>
                            <div className="ra-splitter-tree">
                                {this.renderSPOTree()}
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
                                <GeneralSettingComponent
                                    id={this.archiveGeneralSettingComponent}
                                    context={SPOGeneralSetting.getContext()}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                ></GeneralSettingComponent>
                            </div>}
                            {this.state.showRightSetting && this.state.enableClassification == EnableRecordManagementSetting.Enable && <div>
                                <ArchiveSettingComponent
                                    id={this.archiveSettingComponent}
                                    context={SPOArchiveSetting.getContext()}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    sourceFlag={SourceFlags.SP}
                                    mode={TabIndex.Archive}
                                    availableRules={this.state.availableRules}
                                    refreshRules={this.loadRules}
                                    nodeLevel={this.state.nodeLevel}
                                ></ArchiveSettingComponent>
                                <ScheduleSettingComponent
                                    id={this.scheduleSettingComponent}
                                    context={ArchiveSPOScheduleSetting.getContext()}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                ></ScheduleSettingComponent>
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
            </section>
            {this.renderImportSCPanel()}
            {this.renderConvertStubPanel()}
            {this.renderMigrateDeclaredRecordsPanel()}
            {this.renderExportSettingDialog()}
        </div>;
    }
}
