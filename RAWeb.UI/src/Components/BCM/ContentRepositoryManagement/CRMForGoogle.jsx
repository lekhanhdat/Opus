import { useMemo, useRef, useState } from "react";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import  CRMCommonUtil, {
    RAMessageType,
    SplitterSize,
} from "./Common/CRMCommonUtil";
import { SourceFlags } from "../../../Constants/Constants";
import { IconStatus } from "../../Common/TreeComponents/Constants";
import { showToast } from "../../../Utilities/CommonUtil";
import GoogleScheduleSetting from "./ScheduleSetting/Context/GoogleScheduleSetting";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import { useStableCallback } from "../../Common/Hooks";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
// import { checkPermission } from "../../../Utilities/permissionManager";
// import UniqueIdSettingCommon from "./UniqueIdSetting/UniqueIdSettingCommon";
// import GoogleUniqueIdSetting from "./UniqueIdSetting/Context/GoogleUniqueIdSetting";
import GoogleGeneralManagement from "./GeneralManagement/Context/GoogleGeneralManagement";
import CRMGoogleTree from "../../Common/Tree/Instances/GoogleTree/CRMGoogleTree";
import GoogleDocumentLabelSettingComponent from "./DocumentTermSetting/GoogleDocumentLabelSetting/GoogleDocumentLabelSettingComponent";
import GoogleDocumentLabel from "./DocumentTermSetting/Context/GoogleDocumentLabel";
import GoogleGeneralManagementComponent from "./GeneralManagement/GoogleGeneralManagementSetting/GoogleGeneralManagementComponent";
import { EnableRecordManagementSetting } from "./GeneralManagement/GoogleGeneralManagementSetting/GoogleGeneralManagementPanel";
import ValidateMessageBar from "./Common/CommonMessageBar/ValidateMessageBar";

const menuBtnItemsInMore = [];

const RunApplySettingMethod = {
    SelectedScope: 1,
    AllScope: 2
};

const CRMForGoogleDrive = ({history}) => {
    const refTopButtons = useRef(null);

    const refCRMTree = useRef(null);

    const refDocumentLabelSetting = useRef(null);

    const refScheduleSetting = useRef(null);

    const refGeneralManagementComponent = useRef(null);

    // const refUniqueIdComponent = useRef(null);

    const currNode = useRef(null);

    const settingNode = useRef(null);

    const [isCustomSetting, setIsCustomSetting] = useState(false);

    const [headerName, setHeaderName] = useState("");

    const [showRightSetting, setShowRightSetting] = useState(false);
    
    const [enableClassification,setEnableClassification] = useState("")

    const [searchKey, setSearchKey] = useState("");

    const [availableRules, setAvailableRules] = useState([]);

    const [showTermSettings, setShowTermSettings] = useState(true);

    const inheritButton = {
        name: RMResx["RM_SPS_InheritGlobalSettings"],
        id: "raCrmInheritParentBtn",
        icon: "fia-arrow-line-up",
        onClick: () => {
            inheritParentMessageBox();
        },
    };

    const ruleActionButton = {
        name: RMResx["RM_JS_SPS_DisposalNow"],
        id: "raCrmRunDataSyncBtn",
        icon: "fia-run",
        onClick: () => {
            runRuleActionMessageBox();
        },
    };

    const applyGoogleSettingButtonGroup = [
        {
            isGroup: true, 
            id: "raCrmApplyGlobalSettingsBtnGroup", 
            name: RMResx["RM_SPS_ApplyGlobalSettings"], 
            buttons: [
                { 
                    id:"raCrmApplyAllScopeBtn", 
                    name: RMResx["RM_JS_SPS_ ApplyAllScope"], 
                    onClick: () => runApplyGoogleSettingsMessageBox(RunApplySettingMethod.AllScope)
                },
            ]
        }
    ];

    const syncDataButton = {
        name: RMResx.RM_JS_SPS_CollectNow,
        id: "raCrmRunDataSyncBtn",
        icon: "fia-sync",
        onClick: () => {
            runSyncDataMessageBox();
        },
    };


    const loadNodeSettings = async (args) => {
        $$.loading(true);
        let nodeItem = Object.assign({}, currNode.current, { Children: null, ChildrenIds: null });
        let currentItem = nodeItem;
        while (currentItem.Parent) {
            currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
            currentItem = currentItem.Parent;
        }
        const requestOption = {
            url: "/api/GoogleDriveSettingApi/LoadGoogleNodeSettings",
            data: nodeItem,
        };
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        let settingNodeResult = JSON.parse(result);

        settingNode.current = settingNodeResult;
        settingNodeResult.ParentId = nodeItem.ParentId;
        settingNodeResult.Parent = nodeItem.Parent;
        let text = settingNodeResult.DisplayName;

        setShowRightSetting(true);

        if (settingNodeResult.IconStatus == 0) {
            settingNodeResult.EnableRecordManagement = 1;
        }
        
        setHeaderName(text);
        setIsCustomSetting(settingNodeResult.IsCustomSetting);
        setShowTermSettings(!settingNodeResult.IsNullClassificationSetting);
        setEnableClassification(settingNodeResult.EnableRecordManagement)
        refDocumentLabelSetting.current?.componentReceive(
            "init",
            settingNodeResult
        );
        refScheduleSetting.current?.componentReceive(
            "scheduleData",
            settingNodeResult
        );
        refGeneralManagementComponent.current.initData(settingNodeResult);
        currNode.current.IconStatus = settingNodeResult.IconStatus;
        if (nodeItem.IconStatus == IconStatus.NoSet && settingNodeResult.IconStatus == IconStatus.Break || args) {
           refCRMTree.current.refreshSelectedNode(currNode.current, true);
        } else {
           refCRMTree.current.refreshSelectedNode(currNode.current);
        }
        
        let buttons = [...applyGoogleSettingButtonGroup[0].buttons];
        if (settingNodeResult.IsCustomSetting || CRMCommonUtil.isGoogleContainer(settingNodeResult) && settingNodeResult.IconStatus != 0) {
            buttons.unshift({ 
                id: "raCrmApplySelectedScopeBtn", 
                name: RMResx.RM_JS_SPS_ApplySelectedScope, 
                onClick: () => runApplyGoogleSettingsMessageBox(RunApplySettingMethod.SelectedScope) 
            })
        }
        let menuButtons = [];
        refTopButtons.current.updateButtons([]);

        if (!settingNodeResult.IsNullClassificationSetting) {
            menuButtons.push({ ...applyGoogleSettingButtonGroup[0], buttons: buttons })
        }

        if (settingNodeResult.IsCustomSetting) {
            menuButtons.push(inheritButton);
        }

        let enableRunJob = settingNodeResult.EnableRecordManagement == 1 && settingNodeResult.IconStatus != 0;
        if (enableRunJob) {
            menuButtons.push(ruleActionButton);
        }
        
        if (enableRunJob && settingNodeResult.IsSyncData && !settingNodeResult.IsNullClassificationSetting) {
            menuButtons.push(syncDataButton)
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
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    id: "raCrmInheritParentDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    classify: "theme",
                    onClick: inheritParentDoAction,
                    primary: true
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    const inheritParentDoAction = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/GoogleDriveSettingApi/InheritParentSettings",
            data: settingNode.current,
        };
        const request = await fetchUtility(requestOption);
        const result = JSON.parse(request);
        $$.loading(false);
        if (result.MessageType == RAMessageType.Successful) {
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
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    id: "raCrmRunRuleActionDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    classify: "theme",
                    onClick: runRuleActionDoAction,
                    primary: true
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    const runApplyGoogleSettingsMessageBox = (applySettingType) => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_EnsureApply_Google_Settings,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    id: "raCrmApplyGoogleSettingsActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    classify: "theme",
                    onClick: () => runApplyGoogleSettingsAction(applySettingType),
                    primary: true
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    const runApplyGoogleSettingsAction =  useStableCallback(async (applySettingType) => {
        $$.messagedialog(false);
        $$.loading(true);
        const url = applySettingType == RunApplySettingMethod.SelectedScope 
            ? "/api/GoogleDriveSettingApi/ApplySettingOnSelectedNode"
            : "/api/GoogleDriveSettingApi/ApplySettings";
        const data = applySettingType == RunApplySettingMethod.SelectedScope
            ? settingNode.current
            : { FromTimerJobPage: false, RunJobMethod: applySettingType }

        const requestOption = { url, data };
        const request = await fetchUtility(requestOption);
        const result = JSON.parse(request);
        $$.loading(false);
        if (result.MessageType == RAMessageType.Successful) {
            let content = (
                <$g.I18NProvider
                    msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}
                >
                    <a className="ra-link-a" href="/Root/JM/Index">
                        {RMResx.RM_JS_JM_Title}
                    </a>
                </$g.I18NProvider>
            );
            showToast.success(content);
        } else {
            showToast.error(result.ErrorMessage);
        }
    });

    const runSyncDataMessageBox = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_GoogleDriveEnsureSync,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    id: "raCrmInheritParentDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    classify: "theme",
                    onClick: runDataSyncDoAction,
                    primary: true
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    const runDataSyncDoAction =useStableCallback(async ()=> {
        $$.messagedialog(false);
        $$.loading(true);
        let requestOption = {
            url: "/api/GoogleDriveSettingApi/RunCollectionJob",
            method: "Post",
            data: settingNode.current,
        };
        const request = await fetchUtility(requestOption);
        const result = JSON.parse(request);
        $$.loading(false);
        if (result.MessageType == RAMessageType.Successful) {
            let content = (
                <$g.I18NProvider
                    msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}
                >
                    <a className="ra-link-a" href="/Root/JM/Index">
                        {RMResx.RM_JS_JM_Title}
                    </a>
                </$g.I18NProvider>
            );
            showToast.success(content);
        } else {
            showToast.error(result.ErrorMessage);
        }
    })

    const runRuleActionDoAction = useStableCallback(async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/GoogleDriveSettingApi/RecordsDisposal",
            data: settingNode.current,
        };
        const response = await fetchUtility(requestOption);
        const result = JSON.parse(response)
        $$.loading(false);
        if (result.MessageType == RAMessageType.Successful) {
            let content = (
                <$g.I18NProvider
                    msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}
                >
                    <a className="ra-link-a" href="/Root/JM/Index">
                        {RMResx.RM_JS_JM_Title}
                    </a>
                </$g.I18NProvider>
            );
            showToast.success(content);
        } else {
            showToast.error(result.ErrorMessage);
        }
    });

    const onTreeChanged = (nodeItem) => {
        currNode.current = nodeItem;
        loadNodeSettings();
        loadRules();
    };

    const loadRules = async (reload) => {
        const groupNode = CRMCommonUtil.getGoogleDriveContainerNode(currNode.current);
        $$.loading(true);
        try {
            const result = await fetchUtility({url:'/api/GoogleDriveSettingApi/GetAvailableRuleList ', method: 'post', data: groupNode.Id});
            if(result)
            {
                setAvailableRules(result);
            }
            $$.loading(false);
        } catch (error) {
            $$.loading(false);
        }
    };

    const onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            setIsCustomSetting(false);
            setHeaderName("");
            setShowRightSetting(false);
            refTopButtons.current.updateButtons([
                ...menuBtnItemsInMore,
                ...applyGoogleSettingButtonGroup,
            ]);
        }
    };

    const refreshNodeSettings = (args, isBreakInheritance = true) => {
        loadNodeSettings(args);
    };

    const checkMissingConfig = (isContainerIncluded) => {
        if (isContainerIncluded) {
            if (CRMCommonUtil.isGoogleContainer(settingNode.current) && !settingNode.current.HasContainerSetting) {
                const args = {
                    width: '550px',
                    hideActions: true,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_JS_Google_Schedule_GroupTermSettingMissing,
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
            }
        }
        if (CRMCommonUtil.isGoogleContainer(settingNode.current) || settingNode.current.IsNullClassificationSetting) {
            return false;
        }
        if (!settingNode.current.HasContainerSetting) {
            const args = {
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Google_GroupTermSettingMissing,
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
        }
        return false;
    };

    const onSearch = (args) => {
        setSearchKey(args);
    };

    return (
        <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap
                    data={[
                        SiteMapLinks.BCM_ContentRepositoryManagement_GoogleDrive,
                    ]}
                />
                <ValidateMessageBar />
                <TopButtonsComponent
                    ref={refTopButtons}
                    data={{
                        menuBtnItems: [...menuBtnItemsInMore, ...applyGoogleSettingButtonGroup],
                    }}
                ></TopButtonsComponent>
            </section>
            <section className="crm-content">
                <div className="ra-crm-splitter-container">
                    <R.Splitter
                        minAsize={SplitterSize.minAsize}
                        minBsize={SplitterSize.minBsize}
                        defaultAsize={SplitterSize.defaultAsize}
                    >
                        <div className="ra-splitter-left">
                            <div className="ra-splitter-header-left">
                                <div
                                    className="ra-splitter-header-title"
                                    tabIndex="0"
                                >
                                    {RMResx.RM_JS_SPS_LeftTitle}
                                </div>
                            </div>
                            <div className="ra-splitter-searchbox">
                                <R.Searchbox
                                    width={380}
                                    height={34}
                                    placeholder={RMResx.RM_BCM_SearchByDrive}
                                    disabled={false}
                                    onSearch={onSearch}
                                />
                            </div>
                            <div className="ra-splitter-tree">
                                <CRMGoogleTree
                                    ref={refCRMTree}
                                    treeSource={SourceFlags.Google}
                                    searchKey={searchKey}
                                    onSelectedNodeChanged={onTreeChanged}
                                    onNodeRefresh={onNodeRefresh}
                                    history={history}
                                ></CRMGoogleTree>
                            </div>
                        </div>

                        <div className="ra-splitter-right">
                            <div style={{ fontSize: 0 }}>
                                <div
                                    style={{
                                        width: isCustomSetting
                                            ? "calc(100% - 156px)"
                                            : "calc(100% - 24px)",
                                        display: "inline-block",
                                    }}
                                >
                                    <div
                                        className="ra-splitter-header-title"
                                        tabIndex="0"
                                    >
                                        {RMResx.RM_JS_SPS_RightTitle}
                                    </div>
                                    <div
                                        className="ra-splitter-header-name"
                                        data-tooltip="diffneed"
                                        aria-label={headerName}
                                    >
                                        {headerName != "" && (
                                            <span className="fia-site-collection-group ra-splitter-folder"></span>
                                        )}
                                        <span tabIndex="0">{headerName}</span>
                                    </div>
                                </div>
                                {isCustomSetting && (
                                    <div
                                        className="ra-splitter-unique-container"
                                        tabIndex="0"
                                        aria-label={
                                            RMResx.RM_JS_SPS_HasOwnSettingMessage
                                        }
                                    >
                                        <div
                                            id="showUniqueBtn"
                                            className="inline-block"
                                            style={{
                                                lineHeight: "26px",
                                                marginRight: "8px",
                                            }}
                                        >
                                            <span className="fia-asterisk ra-splitter-unique-icon"></span>
                                            <span>
                                                {RMResx.RM_JS_SPS_Unique}
                                            </span>
                                        </div>
                                        <R.Popup
                                            of={"#showUniqueBtn"}
                                            arrow={true}
                                            triggerEvent="hover:300"
                                            position="right"
                                        >
                                            <div>
                                                <div
                                                    style={{
                                                        margin: "16px",
                                                        width: "280px",
                                                        fontSize: "14px",
                                                    }}
                                                >
                                                    <span>
                                                        {
                                                            RMResx.RM_JS_SPS_HasOwnSettingMessage
                                                        }
                                                    </span>
                                                </div>
                                            </div>
                                        </R.Popup>
                                    </div>
                                )}
                            </div>
                            {!showRightSetting && (
                                <div
                                    className="ra-splitter-description"
                                    tabIndex="0"
                                >
                                    <span>{RMResx.RM_JS_SPS_Tips}</span>
                                </div>
                            )}

                            {showRightSetting && (
                                <div>
                                    <GoogleGeneralManagementComponent
                                        context={GoogleGeneralManagement.getContext()}
                                        id="generalManagementComponent"
                                        ref={refGeneralManagementComponent}
                                        refreshNodeSettings={
                                            refreshNodeSettings
                                        }
                                        checkMissingConfig={checkMissingConfig}
                                    ></GoogleGeneralManagementComponent>
                                    {enableClassification ==
                                        EnableRecordManagementSetting.Enable && (
                                        <div>
                                            <GoogleDocumentLabelSettingComponent
                                                ref={refDocumentLabelSetting}
                                                context={GoogleDocumentLabel.getContext()}
                                                id="documentLabelSettingComponent"
                                                refreshNodeSettings={
                                                    refreshNodeSettings
                                                }
                                                checkMissingConfig={
                                                    checkMissingConfig
                                                }
                                                sourceFlag={SourceFlags.Google}
                                                availableRules={availableRules}
                                                refreshRules={loadRules}
                                                showTermSettings={showTermSettings}
                                            ></GoogleDocumentLabelSettingComponent>
                                            <ScheduleSettingComponent
                                                ref={refScheduleSetting}
                                                context={GoogleScheduleSetting.getContext()}
                                                id="scheduleSettingComponent"
                                                refreshNodeSettings={
                                                    refreshNodeSettings
                                                }
                                                checkMissingConfig={ () =>
                                                    checkMissingConfig(true)
                                                }
                                            ></ScheduleSettingComponent>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    </R.Splitter>
                </div>
                {/* <UniqueIdSettingCommon
                    id="uniqueId"
                    ref={refUniqueIdComponent}
                    supportCustomColumn={true}
                    context={GoogleUniqueIdSetting.getContext()}
                /> */}
            </section>
        </div>
    );
};

export default CRMForGoogleDrive;
