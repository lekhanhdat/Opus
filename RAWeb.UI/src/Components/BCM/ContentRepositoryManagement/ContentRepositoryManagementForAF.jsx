import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import ValidateMessageBar from "./Common/CommonMessageBar/ValidateMessageBar";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import AzureFilesDocumentTerm from "./DocumentTermSetting/Context/AzureFilesDocumentTerm";
import { SingleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";
import { showToast } from "../../../Utilities/CommonUtil";
import { IconStatus } from "../../Common/TreeComponents/Constants";

export default class ContentRepositoryManagementForAF extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            showRightSetting: false,
            headerName: "",
            nodeLevel: "",
            isCustomSetting: false,
        };
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.menuBtnItems = [
            { isStatic: true, name: RMResx.RM_AF_Register_PageTitle_Link, id: "raCrmAzFRouteToConnGroupBtn", onClick: () => { this.props.history.push({ pathname: RouterUrls.BCM_AzFileShareConfigureConnection }); } }
        ];
        this.inheritButton = { name: RMResx["RM_SPS_InheritGlobalSettings"], id: "raCrmInheritParentBtn", icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.syncDataButton = { name: RMResx["RM_JS_SPS_CollectNow"], id:"raCrmRunDataSyncBtn", icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.menuBtnItemsInMore = [];
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/AzureFileSettingApi/InheritParentSetting",
            method: "Post",
            data: this.settingNode.SelectedNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result == "Sucess") {
                this.refreshNodeSettings(null, false);
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
            url: "/api/AzureFileSettingApi/RunCollectionJob",
            method: "Post",
            data: this.currNode
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
            content: RMResx.RM_JS_BCM_NAFSS_EnsureSync,
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

    refreshNodeSettings = (args, isBreakInheritance = true) => {
        try {
            this.currNode.iconStatus = isBreakInheritance ? IconStatus.Break : IconStatus.Inhert;
            this.refCRMTree.onUpdateNodeInfo(this.currNode);
        } catch (error) {
            console.log(error);
        }
        this.loadNodeSettings(args);
    };

    onTreeChanged = (nodeItem) => {
        if (nodeItem == null) {
            this.onNodeRefresh();
            return;
        }
        this.currNode = nodeItem;
        this.loadNodeSettings();
    }

    onNodeRefresh = () => {
        this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
        this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
    }

    loadNodeSettings(reload) {
        let nodeItem = this.currNode;
        $$.loading(true);
        let option = {
            url: "/api/AzureFileSettingApi/LoadAzureFileNodeSetting",
            method: "Post",
            data: nodeItem
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let settingNode = result;
                this.settingNode = settingNode;
                settingNode.SelectedNode.parent.id = nodeItem.parent.id;
                settingNode.SelectedNode.parent = nodeItem.parent;

                this.setState({
                    showRightSetting: true,
                    headerName: settingNode.SelectedNode.displayName,
                    nodeLevel: settingNode.SelectedNode.level,
                    isCustomSetting: settingNode.IsCustomSetting,
                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
                });

                // try {
                //     nodeItem.iconStatus = settingNode.SelectedNode.iconStatus;
                //     this.refCRMTree.onUpdateNodeInfo(nodeItem);
                // } catch (error) {
                //     console.log(error);
                // }

                let menuButtons = [...this.menuBtnItems];
                if (settingNode.IsCustomSetting) {
                    menuButtons.push(this.inheritButton);
                }
                let enableRunJob = !CRMCommonUtil.guidIsEmpty(settingNode.TermSetId);
                if (enableRunJob) {
                    menuButtons.push(this.syncDataButton);
                }
                menuButtons.push(...this.menuBtnItemsInMore);
                this.refTopButtons.updateButtons(menuButtons);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    checkMissingConfig = () => {
        if (CRMCommonUtil.guidIsEmpty(this.settingNode.TermSetId) && !CRMCommonUtil.isAzureFileGroup(this.settingNode.SelectedNode)) {
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

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_AF]} />
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
                            <div className="ra-splitter-tree">
                                <SingleChoiceSourceTree
                                    ref={r => this.refCRMTree = r}
                                    sourceFlag={SourceFlags.AzureFile}
                                    onSelected={this.onTreeChanged}
                                ></SingleChoiceSourceTree>
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
                            
                            {this.state.showRightSetting && <div>
                                <DocumentTermSettingComponent
                                    context={AzureFilesDocumentTerm.getContext()}
                                    id={this.documentTermSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    // disabled={this.checkEditIsDisabled()}
                                    checkMissingConfig={this.checkMissingConfig}
                                    sourceFlag={SourceFlags.AzureFile}
                                ></DocumentTermSettingComponent>
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
            </section>
        </div>;
    }
}