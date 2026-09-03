import { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
// import { AvaWidget } from "@gui/chat-dialog";
import { setAvaExternalActionRequest } from "../../../../../Redux/slices/avaDialogSlice";
import { DiscoveryJobStatus, DiscoveryDataSource } from "../Constants";
import RouterUrls from "../../../../../Constants/RouterUrls";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import AnalysisConfigurationExclusionListPanel from "./Component/AnalysisConfigurationExclusionListPanel";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";
import { DiscoveryJobVersion } from "../../../Analysis/Constants";
import { LicenseType, OpusExternalRequestType, ExternalRequestProductType, RoleType } from "../../../../../Constants/Constants";
import { checkPermission } from "../../../../../Utilities/permissionManager";

const AnalysisConfigurationInitializationPage = ({ history }) => {
    const dispatch = useDispatch();
    const [isWidgetExpanded, setIsWidgetExpanded] = useState(true);

    const [jobInfos, setJobInfos] = useState({});
    const [showExclusionListPanel, setShowExclusionListPanel] = useState(false);

    const [showAvaWidget, setShowAvaWidget] = useState(false);

    const [shouldHidePlanChatInputBox, setShouldHidePlanChatInputBox] = useState(false);

    const isEnableRecordsArchiver = LicenseHelper.EnableRecordsArchiver()
    useEffect(() => {
        const fetchJobStatus = async () => {
            $$.loading(true);
            const jobStatusInfo = await fetchUtility({
                url: "/api/RMDiscoveryOffice365JobManagementApi/GetLatest",
                method: "Get",
            });
            setJobInfos(jobStatusInfo);
            switch (jobStatusInfo.status) {
                case DiscoveryJobStatus.Preparing:
                case DiscoveryJobStatus.Pending:
                case DiscoveryJobStatus.Running:
                    history.push({
                        pathname: RouterUrls.FA_Discovery_RunJob,
                        search: `?dataSource=${DiscoveryDataSource.Office365}`
                    });
                    break;
                case DiscoveryJobStatus.Finished:
                case DiscoveryJobStatus.Failed:
                case DiscoveryJobStatus.Exception:
                    history.push({
                        pathname: RouterUrls.FA_Discovery_Finish,
                        search: `?dataSource=${DiscoveryDataSource.Office365}`
                    });
                    break;
                default:
                    history.push({
                        pathname: RouterUrls.FA_Discovery,
                        search: `?dataSource=${DiscoveryDataSource.Office365}`
                    });
                    break;
            }
            $$.loading(false);
        };

        fetchJobStatus();
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
    }, []);

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

    const onStart = () => {
        history.push({
            pathname: RouterUrls.FA_Discovery_Configuration,
            search: `?dataSource=${DiscoveryDataSource.Office365}`,
            state: jobInfos,
        });
    };

    
    const onShowExclusionListPanel = () => {
        setShowExclusionListPanel(true);
    };

    const onHideExclusionListPanel = () => {
        setShowExclusionListPanel(false);
    };

    const shouldShowExclusionList = RM.gData.licenseType !== LicenseType.Trial && isEnableRecordsArchiver && (jobInfos.version === DiscoveryJobVersion.V4 || jobInfos.version === DiscoveryJobVersion.V5);

    const createGuidId = () => {
        return globalThis.crypto ? globalThis.crypto.randomUUID() : Math.random().toString(36).slice(2);
    };
    
    const triggerExternalAction = (type, shouldHidePlanChatInputBox) => {
        const requestData = { type };

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
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.BuildPlanOpus, shouldHidePlanChatInputBox)}>
                            {RMResx.RM_AVA_BuildPlan_Button}
                        </AvaWidget.Button>
                    </AvaWidget.GroupAction>
                </AvaWidget> */}
            </div>
        );
    };

    return (
        <div id="raDiscovery">
            <$g.SiteMap data={[SiteMapLinks.FA_Discovery]} />

            <div className="reco-ava-box">
                {renderAvaWidget()}
            </div>

            <div className="reco-discovery-configurator">
                <div className="reco-discovery-container">
                    <div className="reco-discovery-left">
                        <img src={`${RM.gData.resCdnURL}/cloud%20records/discovery.svg`} />
                    </div>
                    <div className="reco-discovery-right">
                        <div className="reco-discovery-textstyle" tabIndex="0">
                            <div className="reco-discovery-text1">
                                {RMResx.RM_FA_Discovery_HomeDes01}
                            </div>
                            <div className="reco-discovery-text2">
                                {RMResx.RM_FA_Discovery_HomeDes02}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Discovery_HomeDes03}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Discovery_HomeDes04}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Discovery_HomeDes05}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Discovery_HomeDes06}
                            </div>
                        </div>
                        {shouldShowExclusionList && <div className="margin-bottom-m">
                            <R.Button
                                type="link"
                                classify="default"
                                text={RMResx.RM_FA_Discovery_ExclusionList}
                                onClick={onShowExclusionListPanel}
                            />
                        </div>}
                        <div>
                            <R.Button
                                id="raDiscoveryBtn"
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_FA_Discovery_StartBtn}
                                onClick={onStart}
                            />
                        </div>
                    </div>
                </div>
            </div>
            <AnalysisConfigurationExclusionListPanel
                show={showExclusionListPanel}
                onClose={onHideExclusionListPanel}
            />
        </div>
    );
};
export default AnalysisConfigurationInitializationPage;
