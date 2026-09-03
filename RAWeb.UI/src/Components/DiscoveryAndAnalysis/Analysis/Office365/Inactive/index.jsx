import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
// import { AvaWidget } from "@gui/chat-dialog";
import { setAvaExternalActionRequest } from "../../../../../Redux/slices/avaDialogSlice";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import {
    InactiveSummaryHistoryVersion,
    InactiveSummaryV3,
    InactiveSummaryV4,
} from "./Summary";
import {
    InactiveOptimizationHistoryVersion,
    InactiveOptimizationV3,
    InactiveOptimizationV4,
} from "./Optimization";
import SiteMap from "../../Components/SiteMap";
import { JobMangerRequester } from "../../requests";
import { DiscoveryJobVersion } from "../../Constants";
import { OpusExternalRequestType, ExternalRequestProductType, RoleType } from '../../../../../Constants/Constants'
import { checkPermission } from "../../../../../Utilities/permissionManager";
import RouterUrls from "../../../../../Constants/RouterUrls";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const createGuidId = () => {
    return globalThis.crypto ? globalThis.crypto.randomUUID() : Math.random().toString(36).slice(2);
};

const normalizeTenantId = (payload) => {
    if (typeof payload === "string") return payload;
    if (!payload || typeof payload !== "object") return "";
    return payload.tenantId || payload.id || payload.value || "";
};

const Inactive = () => {
    const dispatch = useDispatch();

    const [jobInfo, setJobInfo] = useState(null);

    const [activeTab, setActiveTab] = useState(ActionTab.Summary);

    const [selectedO365TenantId, setSelectedO365TenantId] = useState();

    const [isWidgetExpanded, setIsWidgetExpanded] = useState(true);

    const [showAvaWidget, setShowAvaWidget] = useState(false);

    const [shouldHidePlanChatInputBox, setShouldHidePlanChatInputBox] = useState(false);

    useEffect(() => {
        const fetchJobInfo = async () => {
            const responseJobInfo = await JobMangerRequester.getLatest();
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

    const handleSiteMapChange = (payload) => {
        setSelectedO365TenantId(normalizeTenantId(payload));
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

    const InactiveView = (jobInfo) => {
        if (jobInfo == null) {
            return <div></div>;
        } else if (jobInfo.version == DiscoveryJobVersion.V4 || jobInfo.version == DiscoveryJobVersion.V5) {
            return (
                <R.Tabcontrol
                    maxWidth={"none"}
                    destroy={true}
                    onChange={(index) => setActiveTab(index)}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_SummaryTab}
                        aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                    >
                        <InactiveSummaryV4
                            key={selectedO365TenantId + "_inactive_summary"}
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_OptimizationTab}
                        aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
                    >
                        <InactiveOptimizationV4
                            key={
                                selectedO365TenantId + "_inactive_optimization"
                            }
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        } else if (jobInfo.version == DiscoveryJobVersion.V3) {
            return (
                <R.Tabcontrol
                    maxWidth={"none"}
                    destroy={true}
                    onChange={(index) => setActiveTab(index)}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_SummaryTab}
                        aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                    >
                        <InactiveSummaryV3
                            key={selectedO365TenantId + "_inactive_summary"}
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_OptimizationTab}
                        aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
                    >
                        <InactiveOptimizationV3
                            key={
                                selectedO365TenantId + "_inactive_optimization"
                            }
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        } else {
            return (
                <R.Tabcontrol
                    maxWidth={"none"}
                    destroy={true}
                    onChange={(index) => setActiveTab(index)}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_SummaryTab}
                        aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                    >
                        <InactiveSummaryHistoryVersion
                            key={selectedO365TenantId + "_inactive_summary"}
                            o365TenantId={selectedO365TenantId}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_OptimizationTab}
                        aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
                    >
                        <InactiveOptimizationHistoryVersion
                            key={
                                selectedO365TenantId + "_inactive_optimization"
                            }
                            o365TenantId={selectedO365TenantId}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        }
    };

    return (
        <div id="raInactive">
            <SiteMap
                URL={[SiteMapLinks.FA_Inactive]}
                onChange={setSelectedO365TenantId}
            />
            <div className="reco-ava-box">
                {renderAvaWidget()}
            </div>
            <div>
                {InactiveView(jobInfo)}
            </div>
        </div>
    );
};

export default Inactive;
