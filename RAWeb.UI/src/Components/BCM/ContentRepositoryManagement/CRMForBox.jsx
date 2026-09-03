import _ from "lodash";
import { useRef, useState } from "react";
import { useHistory } from "react-router";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import RouterUrls from "../../../Constants/RouterUrls";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import SingleChoiceSourceTree from "../../Common/TreeComponents/SourceTree/SingleChoiceSourceTree";
import { SourceFlags } from "../../../Constants/Constants";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import BoxDocumentTerm from "./DocumentTermSetting/Context/BoxDocumentTerm";
import { IconStatus } from "../../Common/TreeComponents/Constants";
import { showToast } from "../../../Utilities/CommonUtil";
import BoxManualApprovalSetting from "./ManualApprovalSetting/Context/BoxManualApprovalSetting";
import BoxScheduleSetting from "./ScheduleSetting/Context/BoxScheduleSetting";
import ManualApprovalSettingComponent from "./ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import { useStableCallback } from "../../Common/Hooks";

const menuBtnItemsInMore = [];

const CRMForBox = () => {

    const history = useHistory();

    const refTopButtons = useRef(null);

    const refCRMTree = useRef(null);

    const refDocumentTermSetting = useRef(null);

    const refScheduleSetting = useRef(null);

    const refManualApprovalSetting = useRef(null);

    const currNode = useRef(null);

    const settingNode = useRef(null);

    const [isCustomSetting, setIsCustomSetting] = useState(false);

    const [headerName, setHeaderName] = useState("");

    const [showRightSetting, setShowRightSetting] = useState(false);

    const menuBtnItems = [
        { isStatic: true, name: RMResx.RM_AF_Register_PageTitle_Link, id: "raCrmBoxRouteToConnGroupBtn", onClick: () => { history.push({ pathname: RouterUrls.BCM_BoxConfigureConnection }); } }
    ];

    const inheritButton = { name: RMResx["RM_SPS_InheritGlobalSettings"], id: "raCrmInheritParentBtn", icon: "fia-arrow-line-up", onClick: () => { inheritParentMessageBox(); } };

    const ruleActionButton = { name: RMResx["RM_JS_SPS_DisposalNow"], id: "raCrmRunEnforceRuleBtn", icon: "fia-run", onClick: () => { runRuleActionMessageBox(); } };

    const syncDataButton = { name: RMResx["RM_JS_SPS_CollectNow"], id: "raCrmRunDataSyncBtn", icon: "fia-sync", onClick: () => { runDataSyncMessageBox(); } };

    const loadNodeSettings = async () => {
        let nodeItem = currNode.current;
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxSetting/LoadBoxNodeSetting",
            data: nodeItem
        };
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        let settingNodeResult = result;
        settingNode.current = settingNodeResult;
        settingNodeResult.SelectedNode.parent.id = nodeItem.parent.id;
        settingNodeResult.SelectedNode.parent = nodeItem.parent;

        setShowRightSetting(true);
        setHeaderName(settingNodeResult.SelectedNode.displayName);
        setIsCustomSetting(settingNodeResult.IsCustomSetting);

        refDocumentTermSetting.current.componentReceive('init', settingNodeResult);
        refScheduleSetting.current.componentReceive('scheduleData', settingNodeResult);
        refManualApprovalSetting.current.componentReceive("manualApprovalData", settingNodeResult);

        currNode.current.iconStatus = settingNodeResult.SelectedNode.iconStatus;
        refCRMTree.current.onUpdateNodeInfo(currNode.current);

        let menuButtons = [...menuBtnItems];
        if (settingNodeResult.IsCustomSetting) {
            menuButtons.push(inheritButton);
        }
        let enableRunJob = !CRMCommonUtil.guidIsEmpty(settingNodeResult.TermSetId);
        if (enableRunJob) {
            menuButtons.push(ruleActionButton);
            menuButtons.push(syncDataButton);
        }
        menuButtons.push(...menuBtnItemsInMore);
        refTopButtons.current.updateButtons(menuButtons);
    };

    const inheritParentMessageBox = () => {
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
                    onClick: inheritParentDoAction
                },
            ]
        };
        $$.messagedialog(true, args);
    };

    const inheritParentDoAction = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxSetting/InheritParentSetting",
            data: settingNode.current.SelectedNode
        };
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.isSuccessful) {
            refreshNodeSettings(null, false);
            showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
        } else {
            showToast.error(RMResx.RM_JS_SPS_SaveSettingsFailed);
        }
    };

    const runRuleActionMessageBox = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_EnsureRun,
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
                    onClick: runRuleActionDoAction
                }
            ]
        };
        $$.messagedialog(true, args);
    };

    const runRuleActionDoAction = useStableCallback(async () => {
        $$.messagedialog(false);
        $$.loading(true);
        settingNode.current.DisposeScheduleInfo = null;
        const requestOption = {
            url: "/api/BoxSetting/RunJob",
            data: JSON.stringify(settingNode.current)
        };
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType == RAMessageType.Successful) {
            let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>;
            showToast.success(content);
        } else {
            showToast.error(result.ErrorMessage);
        }
    });

    const runDataSyncMessageBox = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NBOXS_EnsureSync,
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
                    onClick: runDataSyncDoAction
                },
            ]
        };
        $$.messagedialog(true, args);
    };

    const runDataSyncDoAction = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/BoxSetting/RunCollectionJob",
            data: currNode.current
        };
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result) {
            let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>;
            showToast.success(content);
        } else {
            showToast.error(result);
        }
    };

    const onTreeChanged = (nodeItem) => {
        if (_.isNil(nodeItem)) {
            onNodeRefresh();
            return;
        }
        currNode.current = nodeItem;
        loadNodeSettings();
    };

    const onNodeRefresh = () => {
        setIsCustomSetting(false);
        setHeaderName("");
        setShowRightSetting(false);
        refTopButtons.current.updateButtons([...menuBtnItems, ...menuBtnItemsInMore]);
    };

    const refreshNodeSettings = (args, isBreakInheritance = true) => {
        try {
            currNode.current.iconStatus = isBreakInheritance ? IconStatus.Break : IconStatus.Inhert;
            refCRMTree.current.onUpdateNodeInfo(currNode.current);
        } catch (error) {
            console.log(error);
        }
        loadNodeSettings();
    };

    const checkMissingConfig = () => {
        if (CRMCommonUtil.guidIsEmpty(settingNode.current.TermSetId) && !CRMCommonUtil.isBoxGroup(settingNode.current.SelectedNode)) {
            let args = {
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
    };

    return <div id="rmCRM">
        <section className="crm-header">
            <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_Box]} />
            <TopButtonsComponent
                ref={refTopButtons}
                data={{ menuBtnItems: [...menuBtnItems, ...menuBtnItemsInMore] }}
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
                                ref={refCRMTree}
                                sourceFlag={SourceFlags.Box}
                                onSelected={onTreeChanged}
                            ></SingleChoiceSourceTree>
                        </div>
                    </div>

                    <div className="ra-splitter-right">
                        <div style={{ fontSize: 0 }}>
                            <div style={{ width: isCustomSetting ? "calc(100% - 156px)" : "calc(100% - 24px)", display: "inline-block" }}>
                                <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_RightTitle}</div>
                                <div className="ra-splitter-header-name" data-tooltip="diffneed" aria-label={headerName}>
                                    {headerName != "" && <span className="fia-folder ra-splitter-folder"></span>}
                                    <span tabIndex="0" style={{ flex: 1 }} className="ra-ellipsis">{headerName}</span>
                                </div>
                            </div>
                            {isCustomSetting && <div className="ra-splitter-unique-container" tabIndex="0" aria-label={RMResx.RM_JS_SPS_HasOwnSettingMessage}>
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
                        {!showRightSetting && <div className="ra-splitter-description" tabIndex="0">
                            <span>{RMResx.RM_JS_SPS_Tips}</span>
                        </div>}

                        {showRightSetting && <div>
                            <DocumentTermSettingComponent
                                ref={refDocumentTermSetting}
                                context={BoxDocumentTerm.getContext()}
                                id="documentTermSettingComponent"
                                refreshNodeSettings={refreshNodeSettings}
                                checkMissingConfig={checkMissingConfig}
                                sourceFlag={SourceFlags.Box}
                            ></DocumentTermSettingComponent>
                            <ManualApprovalSettingComponent
                                ref={refManualApprovalSetting}
                                context={BoxManualApprovalSetting.getContext()}
                                id="manualApprovalSettingComponent"
                                refreshNodeSettings={refreshNodeSettings}
                                checkMissingConfig={checkMissingConfig}
                            ></ManualApprovalSettingComponent>
                            <ScheduleSettingComponent
                                ref={refScheduleSetting}
                                context={BoxScheduleSetting.getContext()}
                                id="scheduleSettingComponent"
                                refreshNodeSettings={refreshNodeSettings}
                                checkMissingConfig={checkMissingConfig}
                            ></ScheduleSettingComponent>
                        </div>}
                    </div>
                </R.Splitter>
            </div>
        </section>
    </div>;
};

export default CRMForBox;