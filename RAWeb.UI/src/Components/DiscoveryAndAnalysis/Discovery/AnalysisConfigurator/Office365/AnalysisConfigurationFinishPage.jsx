import { useState, useRef, useEffect } from "react";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import RouterUrls from "../../../../../Constants/RouterUrls";
import { ConfigurationRequester } from "../../../Analysis/requests";
import {
    DiscoveryJobStatus,
    DiscoveryDataSource
} from "../Constants";
import {AnalysisConfigurationPanelComponent} from "./Component";
import AnalysisConfigurationExclusionListPanel from "./Component/AnalysisConfigurationExclusionListPanel";
import { getRequestVerificationToken, LicenseHelper, showToast } from "../../../../../Utilities/CommonUtil";
import { DiscoveryJobType, DiscoveryJobVersion } from "../../../Analysis/Constants";
import { LicenseType } from "../../../../../Constants/Constants";
import { Messagebox } from "../../../../Common/Messagebox";

const AnalysisConfigurationFinishPage = ({ history }) => {
    const panelRef = useRef();

    const formRef = useRef();

    const [jobInfo, setJobInfo] = useState({});
    const [showExclusionListPanel, setShowExclusionListPanel] = useState(false);
    const isEnableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
    useEffect(() => {
        const fetchData = async () => {
            const jobStatusInfo = await fetchUtility({
                url: "/api/RMDiscoveryOffice365JobManagementApi/GetLatest",
                method: "Get",
            });

            setJobInfo(jobStatusInfo);
        };

        fetchData();
    }, []);

    const onDiscoveryAgain = () => {
        if (
            (jobInfo.status === DiscoveryJobStatus.Failed &&
                jobInfo.jobType === DiscoveryJobType.Newly) ||
            RM.gData.licenseType === LicenseType.Trial
        ) {
            history.push({
                pathname: RouterUrls.FA_Discovery_Configuration,
                search: `?dataSource=${DiscoveryDataSource.Office365}`,
                state: jobInfo,
            });
            return;
        }
        panelRef.current.onShow(jobInfo, history);
    };

    const onDiscoveryRetry = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_EnsureDiscovery}</span>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.messagedialog(false);
                        const res =
                            await ConfigurationRequester.retryFailedAnalysisJob();
                        if (res.MessageType !== 0) {
                            showToast.error(res.ErrorMessage);
                            return false;
                        }
                        history.push({
                            pathname: RouterUrls.FA_Discovery_RunJob,
                            search: `?dataSource=${DiscoveryDataSource.Office365}`
                        });
                        return true;
                    },
                },
            ],
        });
    };

    const onGenerateExportData = () => {
        const content = (() => {
            return (
                <div className="flex flex-column gap-xs">
                    <span>{RMResx.RM_JS_Common_ExportMsg}</span>
                    <span>{RMResx.RM_FA_Discovery_GenerateExportData_WarningMess}</span>
                </div>
            )
        })();
        Messagebox({ content: content, actionFun: handleGenerateData });
    }

    const handleGenerateData = async () => {
        const requestOption = {
            url: "/api/RMDiscoveryOffice365ExportRowDataApi/ExportRowDataJob",
            method: "GET",
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType === 0) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>); 
        } else {
            showToast.error(result.ErrorMessage);
        }  
    }

    const onDownloadJobReport = (e) => {
        e.preventDefault();
        formRef.current.submit();
    };

    const onShowExclusionListPanel = () => {
        setShowExclusionListPanel(true);
    };

    const onHideExclusionListPanel = () => {
        setShowExclusionListPanel(false);
    };

    const shouldShowExclusionList = RM.gData.licenseType !== LicenseType.Trial && isEnableRecordsArchiver && (jobInfo.version === DiscoveryJobVersion.V4 || jobInfo.version === DiscoveryJobVersion.V5);

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.FA_Discovery]} />
            <div className="reco-success-configurator">
                <div className="reco-success-content">
                    {jobInfo.status === DiscoveryJobStatus.Finished && (
                        <div>
                            <div className="margin-bottom-l">
                                <img src={`${RM.gData.resCdnURL}/cloud%20records/success.svg`} />
                            </div>
                            <div className="reco-success-title" tabIndex="0">
                                {RMResx.RM_FA_Discovery_SuccessPage_Title}
                            </div>
                            <div className="reco-success-des" tabIndex="0">
                                <$g.I18NProvider
                                    msg={RMResx.RM_FA_Discovery_SuccessPage_Des}
                                >
                                    <a
                                        className="ra-link-a"
                                        href="/Root/FileAnalysis/InactiveOptimization"
                                    >
                                        {RMResx.RM_FA_Inactive}
                                    </a>

                                    <a
                                        className="ra-link-a"
                                        href="/Root/FileAnalysis/ROTOptimization"
                                    >
                                        {RMResx.RM_FA_ROT}
                                    </a>
                                </$g.I18NProvider>
                            </div>
                            <div className="reco-success-time" tabIndex="0">
                                <span>
                                    {RMResx.RM_FA_Discovery_SuccessPage_LastTime +
                                        " "}
                                </span>
                                <a
                                    tabIndex="0"
                                    onKeyDown={(e) => {
                                        if (e.key == "Enter") {
                                            onDownloadJobReport(e);
                                        }
                                    }}
                                    onClick={onDownloadJobReport}
                                    className="ra-link-a"
                                >
                                    {jobInfo.endTime}
                                </a>
                            </div>
                        </div>
                    )}
                    {jobInfo.status === DiscoveryJobStatus.Failed && (
                        <div>
                            <div className="margin-bottom-l">
                                <img src={`${RM.gData.resCdnURL}/cloud%20records/failed.svg`} />
                            </div>
                            <div className="reco-success-title" tabIndex="0">
                                {RMResx.RM_FA_Discovery_SuccessPage_FailedTitle}
                            </div>
                            <div className="reco-success-des" tabIndex="0">
                                {RMResx.RM_FA_Discovery_SuccessPage_FailedDes}
                            </div>
                            <div className="reco-success-time" tabIndex="0">
                                <span>
                                    {RMResx.RM_FA_Discovery_SuccessPage_LastTime +
                                        " "}
                                </span>
                                <a
                                    tabIndex="0"
                                    onKeyDown={(e) => {
                                        if (e.key == "Enter") {
                                            onDownloadJobReport(e);
                                        }
                                    }}
                                    onClick={onDownloadJobReport}
                                    className="ra-link-a"
                                >
                                    {jobInfo.endTime}
                                </a>
                            </div>
                        </div>
                    )}
                    {jobInfo.status === DiscoveryJobStatus.Exception && (
                        <div>
                            <div className="margin-bottom-l">
                                <img src={`${RM.gData.resCdnURL}/cloud%20records/exception.svg`} />
                            </div>
                            <div className="reco-success-title" tabIndex="0">
                                {RMResx.RM_FA_Discovery_ExceptionPage_Title}
                            </div>
                            <div className="reco-success-des" tabIndex="0">
                                <$g.I18NProvider
                                    msg={
                                        RMResx.RM_FA_Discovery_ExceptionPage_Des
                                    }
                                >
                                    <a
                                        className="ra-link-a"
                                        href="/Root/FileAnalysis/InactiveOptimization"
                                    >
                                        {RMResx.RM_FA_Inactive}
                                    </a>
                                    <a
                                        className="ra-link-a"
                                        href="/Root/FileAnalysis/ROTOptimization"
                                    >
                                        {RMResx.RM_FA_ROT}
                                    </a>
                                </$g.I18NProvider>
                            </div>
                            <div className="reco-success-time" tabIndex="0">
                                <span>
                                    {RMResx.RM_FA_Discovery_SuccessPage_LastTime +
                                        " "}
                                </span>
                                <a
                                    tabIndex="0"
                                    onKeyDown={(e) => {
                                        if (e.key == "Enter") {
                                            onDownloadJobReport(e);
                                        }
                                    }}
                                    onClick={onDownloadJobReport}
                                    className="ra-link-a"
                                >
                                    {jobInfo.endTime}
                                </a>
                            </div>
                        </div>
                    )}
                    {shouldShowExclusionList && <div className="margin-bottom-m">
                        <R.Button
                            type="link"
                            classify="default"
                            text={RMResx.RM_FA_Discovery_ExclusionList}
                            onClick={onShowExclusionListPanel}
                        />
                    </div>}
                    <div className="flex justify-center align-center gap-s">
                        <R.Button
                            id="raDiscoveryAgainBtn"
                            primary={
                                (jobInfo.status === DiscoveryJobStatus.Failed &&
                                    jobInfo.jobType ===
                                        DiscoveryJobType.Newly) ||
                                jobInfo.status === DiscoveryJobStatus.Exception
                                    ? false
                                    : true
                            }
                            classify={
                                (jobInfo.status === DiscoveryJobStatus.Failed &&
                                    jobInfo.jobType ===
                                        DiscoveryJobType.Newly) ||
                                jobInfo.status === DiscoveryJobStatus.Exception
                                    ? "default"
                                    : "theme"
                            }
                            text={RMResx.RM_FA_Discovery_SuccessPage_AgainBtn}
                            onClick={onDiscoveryAgain}
                        />
                        {(jobInfo.status === DiscoveryJobStatus.Exception ||
                            (jobInfo.status === DiscoveryJobStatus.Failed &&
                                jobInfo.jobType !==
                                    DiscoveryJobType.Newly)) && (
                            <R.Button
                                id="raDiscoveryRetryBtn"
                                text={RMResx.RM_FA_Discovery_Rescan_Btn}
                                onClick={onDiscoveryRetry}
                            />
                        )}
                        {LicenseHelper.HasDiscoveryExportRowData() && 
                        (jobInfo.status === DiscoveryJobStatus.Finished 
                        || jobInfo.status === DiscoveryJobStatus.Exception) &&
                            <R.Button 
                                id="raDiscoveryExportRawDataBtn"
                                classify="default"
                                text={RMResx.RM_FA_Discovery_GenerateExportData}
                                onClick={onGenerateExportData}
                            />
                        }
                    </div>
                    <AnalysisConfigurationPanelComponent ref={panelRef}/>
                    <AnalysisConfigurationExclusionListPanel
                        show={showExclusionListPanel}
                        onClose={onHideExclusionListPanel}
                    />
                </div>
            </div>
            <section style={{ display: "none" }}>
                <form
                    ref={formRef}
                    action="/api/RMDiscoveryOffice365ConfigurationApi/DownloadDiscoveryJobReport"
                    method="post"
                >
                    <input
                        name="RequestVerificationToken"
                        type="hidden"
                        value={getRequestVerificationToken()}
                        readOnly
                    />
                </form>
            </section>
        </div>
    );
};

export default AnalysisConfigurationFinishPage;
