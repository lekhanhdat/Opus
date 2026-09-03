import { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
// import { AvaWidget } from "@gui/chat-dialog";
import { setAvaExternalActionRequest } from "../../../../../Redux/slices/avaDialogSlice";
import SiteMap from "../../Components/SiteMap";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { ROTSummaryHistoryVersion, ROTSummaryV3 } from "./Summary";
import { ROTOptimizationHistoryVersion, ROTOptimizationV3 } from "./Optimization";
import JobManagerRequester from "../../requests/JobMangerRequester";
import { DiscoveryJobVersion } from "../../Constants";
import { OpusExternalRequestType, ExternalRequestProductType, RoleType } from '../../../../../Constants/Constants'
import { checkPermission } from "../../../../../Utilities/permissionManager";
import RouterUrls from "../../../../../Constants/RouterUrls";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const ROT = () => {
    const dispatch = useDispatch();
    const [isWidgetExpanded, setIsWidgetExpanded] = useState(true);

    const [jobInfo, setJobInfo] = useState(null);

    const [activeTab, setActiveTab] = useState(ActionTab.Summary);

    const [selectedO365TenantId, setSelectedO365TenantId] = useState();

    const [showAvaWidget, setShowAvaWidget] = useState(false);

    const [shouldHidePlanChatInputBox, setShouldHidePlanChatInputBox] = useState(false);

    useEffect(() => {
        const fetchJobInfo = async () => {
            const responseJobInfo = await JobManagerRequester.getLatest();
            setJobInfo(responseJobInfo);
        };
        fetchJobInfo();
    }, []);

    useEffect(() => {
        fetchUtility({
            url: "/api/RMDiscoveryPlanProfileApi/EnableAIMessage",
            method: "POST"
        })
        .then(res => {
            setShowAvaWidget(res === true);
        })
        .catch(err => console.error(err));
    }, [selectedO365TenantId]);

    useEffect(() => {
        fetchUtility({
            url: `/api/RMDiscoveryPlanProfileApi/GetPlanChatDisplayConfiguration`,
            method: "GET"
        })
        .then(shouldDisplayChat => {
            setShouldHidePlanChatInputBox(shouldDisplayChat !== true);
        })
        .catch(err => {
            console.error(err)
        });
    }, []);

    const createGuidId = () => {
        return globalThis.crypto ? globalThis.crypto.randomUUID() : Math.random().toString(36).slice(2);
    };

    const triggerExternalAction = (type, profileGroupId, shouldHidePlanChatInputBox) => {
        const requestData = { type };

        if (profileGroupId != null) {
            requestData.profileGroupId = profileGroupId;
        }

        if (shouldHidePlanChatInputBox != null) {
            requestData.shouldHideChatInputBox = shouldHidePlanChatInputBox;
        }

        const nextRequest = {
            id: createGuidId(),
            productType: ExternalRequestProductType.Opus,
            data: requestData
        };

        dispatch(setAvaExternalActionRequest(nextRequest));
    };

    const renderAvaWidget = () => {
        if (!checkPermission(
            RouterUrls.FA_Plan_Profile,
            RM.UserResources
        )) return null;
        
        if (!showAvaWidget) return null;
        if (!(!RM.gData.diableChatBot && RM.gData.chatBotApiURL) || RM.RoleType != RoleType.SupAdmin) return null;

        const profileGroupId = selectedO365TenantId || undefined;
        
        return (
            <div className="margin-bottom-m">
                {/* <AvaWidget
                    layout="vertical"
                    showMore={true}
                    onToggle={() => setIsWidgetExpanded(!isWidgetExpanded)}
                >
                    <AvaWidget.GroupAction
                        title={RMResx.RM_AVA_Title}
                        description={RMResx.RM_AVA_Description}
                    >
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.AskYourData)}>
                            {RMResx.RM_AVA_AskYourData_Button}
                        </AvaWidget.Button>
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.BuildPlanOpus, profileGroupId, shouldHidePlanChatInputBox)}>
                            {RMResx.RM_AVA_BuildPlan_Button}
                        </AvaWidget.Button>
                    </AvaWidget.GroupAction>
                </AvaWidget> */}
            </div>
        );
    };

    return (
        <div id="raROT">
            <SiteMap
                URL={[SiteMapLinks.FA_ROT]}
                onChange={setSelectedO365TenantId}
            />
            <div className="reco-ava-box">
                {renderAvaWidget()}
            </div>
            <div>
                {jobInfo === null ? (
                    <div></div>
                ) : jobInfo.version == DiscoveryJobVersion.V3 || jobInfo.version == DiscoveryJobVersion.V4 || jobInfo.version == DiscoveryJobVersion.V5 ? (
                    <R.Tabcontrol
                        flex
                        destroy={true}
                        onChange={(index) => setActiveTab(index)}
                        active={activeTab}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_FA_ROT_SummaryTab}
                            aria-label={RMResx.RM_FA_ROT_SummaryTab}
                        >
                            <ROTSummaryV3
                                key={selectedO365TenantId + "_rot_summary"}
                                o365TenantId={selectedO365TenantId}
                                jobInfo={jobInfo}
                            />
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_FA_ROT_OptimizationTab}
                            aria-label={RMResx.RM_FA_ROT_OptimizationTab}
                        >
                            <ROTOptimizationV3
                                key={selectedO365TenantId + "_rot_optimization"}
                                o365TenantId={selectedO365TenantId}
                                jobInfo={jobInfo}
                            />
                        </R.TabPanel>
                    </R.Tabcontrol>
                ) : (
                    <R.Tabcontrol
                        flex
                        destroy={true}
                        onChange={(index) => setActiveTab(index)}
                        active={activeTab}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_FA_ROT_SummaryTab}
                            aria-label={RMResx.RM_FA_ROT_SummaryTab}
                        >
                            <ROTSummaryHistoryVersion
                                key={selectedO365TenantId + "_rot_summary"}
                                o365TenantId={selectedO365TenantId}
                            />
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_FA_ROT_OptimizationTab}
                            aria-label={RMResx.RM_FA_ROT_OptimizationTab}
                        >
                            <ROTOptimizationHistoryVersion
                                key={selectedO365TenantId + "_rot_optimization"}
                                o365TenantId={selectedO365TenantId}
                            />
                        </R.TabPanel>
                    </R.Tabcontrol>
                )}
            </div>
        </div>
    );
};

export default ROT;
