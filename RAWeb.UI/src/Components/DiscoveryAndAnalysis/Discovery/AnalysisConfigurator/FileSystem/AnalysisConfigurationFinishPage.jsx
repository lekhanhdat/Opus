import { useState, useRef, useEffect } from "react";

import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import RouterUrls from "../../../../../Constants/RouterUrls";
import { DiscoveryJobStatus, DiscoveryDataSource } from "../Constants";
import { DiscoveryJobType } from "../../../Analysis/Constants";
import { getRequestVerificationToken } from "../../../../../Utilities/CommonUtil";

const AnalysisConfigurationFinishPage = ({ history }) => {
    const [jobInfo, setJobInfo] = useState({});

    const formRef = useRef();

    useEffect(() => {
        const fetchJobStatusInfo = async () => {
            const jobStatusInfo = await fetchUtility({
                url: "/api/RMDiscoveryFSJobManagementApi/GetLatest",
                method: "Get",
            });

            setJobInfo(jobStatusInfo);
        };

        fetchJobStatusInfo();
    }, []);

    const onDiscoveryAgain = () => {
        history.push({
            pathname: RouterUrls.FA_Discovery_Configuration,
            search: `?dataSource=${DiscoveryDataSource.FileSystem}`,
            state: jobInfo,
        });
    };

    const onDownloadJobReport = (e) => {
        e.preventDefault();
        formRef.current.submit();
    };

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
                                        href={`/Root/FileAnalysis/InactiveOptimization?dataSource=${DiscoveryDataSource.FileSystem}`}
                                    >
                                        {RMResx.RM_FA_Inactive}
                                    </a>
                                    <a
                                        className="ra-link-a"
                                        href={`/Root/FileAnalysis/ROTOptimization?dataSource=${DiscoveryDataSource.FileSystem}`}
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
                                        RMResx.RM_FA_FS_Discovery_ExceptionPage_Des
                                    }
                                >
                                    <a
                                        className="ra-link-a"
                                        href={`/Root/FileAnalysis/InactiveOptimization?dataSource=${DiscoveryDataSource.FileSystem}`}
                                    >
                                        {RMResx.RM_FA_Inactive}
                                    </a>
                                    <a
                                        className="ra-link-a"
                                        href={`/Root/FileAnalysis/ROTOptimization?dataSource=${DiscoveryDataSource.FileSystem}`}
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
                    <div className="flex justify-center align-center gap-s">
                        <R.Button
                            id="raDiscoveryAgainBtn"
                            primary={
                                jobInfo.status === DiscoveryJobStatus.Failed &&
                                    jobInfo.jobType ===
                                        DiscoveryJobType.Newly
                                    ? false
                                    : true
                            }
                            classify={
                                jobInfo.status === DiscoveryJobStatus.Failed &&
                                    jobInfo.jobType ===
                                        DiscoveryJobType.Newly
                                    ? "default"
                                    : "theme"
                            }
                            text={RMResx.RM_FA_Discovery_SuccessPage_AgainBtn}
                            onClick={onDiscoveryAgain}
                        />
                    </div>
                </div>
            </div>
            <section style={{ display: "none" }}>
                <form
                    ref={formRef}
                    action="/api/RMDiscoveryFSConfigurationApi/DownloadDiscoveryJobReport"
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
