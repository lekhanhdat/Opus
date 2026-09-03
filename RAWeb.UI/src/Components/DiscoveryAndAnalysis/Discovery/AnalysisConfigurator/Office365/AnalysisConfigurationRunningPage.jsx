import { useState, useEffect } from "react";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { DiscoveryJobStatus } from "../../../Analysis/Constants";
import RouterUrls from "../../../../../Constants/RouterUrls";

const AnalysisConfigurationRunningPage = ({ history }) => {

    const [discoveredSpeed, setDiscoveredSpeed] = useState(0);

    const [discoveredProcess, setDiscoveredProcess] = useState(0);

    const [sitesSpeed, setSitesSpeed] = useState(0);

    const [sitesProcess, setSitesProcess] = useState(0);

    useEffect(() => {
        const fetchData = async () => {
            $$.loading(true);
            const jobStatusInfo = await fetchUtility({
                url: "/api/RMDiscoveryOffice365JobManagementApi/GetLatest",
                method: "Get",
            });

            const completedCount =
                jobStatusInfo.siteProgressInfo.succeedCount +
                jobStatusInfo.siteProgressInfo.failedCount;
            const siteSpeed =
                completedCount +
                "/" +
                jobStatusInfo.siteProgressInfo.needProcessCount;
            let siteProcess =
                Number.parseInt(
                    (
                        completedCount /
                        jobStatusInfo.siteProgressInfo.needProcessCount
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            
            if(siteProcess === "100%") {
                siteProcess = "99%";
            }

            let discoveredProcess =
                Number.parseInt(
                    (
                        jobStatusInfo.siteProgressInfo.discoveredCount /
                        jobStatusInfo.siteProgressInfo.needProcessCount
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            if(discoveredProcess === "100%") {
                discoveredProcess = "99%";
            }

            setDiscoveredSpeed(`${jobStatusInfo.siteProgressInfo.discoveredCount}/${jobStatusInfo.siteProgressInfo.needProcessCount}`);
            setDiscoveredProcess(discoveredProcess);
            setSitesSpeed(siteSpeed);
            setSitesProcess(siteProcess);

            $$.loading(false);

            if(jobStatusInfo.status === DiscoveryJobStatus.Finished || 
                jobStatusInfo.status === DiscoveryJobStatus.Failed ||
                jobStatusInfo.status === DiscoveryJobStatus.Exception){
                    history.push({
                        pathname: RouterUrls.FA_Discovery_Finish,
                    });
                }
        };

        fetchData();
    }, []);

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.FA_Discovery]} />
            <div className="reco-start-configurator">
                <section className="reco-start-title" tabIndex="0">
                    {RMResx.RM_FA_Discovery_JobPage_Title}
                </section>
                <section className="reco-start-content">
                    <div className="reco-start-content-left">
                        <div className="reco-start-contentdes" tabIndex="0">
                            {RMResx.RM_FA_Discovery_JobPage_Des01}
                        </div>
                        <div className="reco-start-contentdes" tabIndex="0">
                            {RMResx.RM_FA_Discovery_JobPage_Des02}
                        </div>
                        <div className="reco-start-state">
                            <div tabIndex="0">
                                <span className="reco-start-state-discover">
                                    {RMResx.RM_FA_Discovery_JobPage_DiscoveredSitesSpeed}
                                </span>
                                <span className="reco-start-state-speed">
                                    {discoveredSpeed}
                                </span>
                            </div>
                            <div>
                                <div className="reco-start-state-div">
                                    <span className="fia-in-progress reco-start-state-icon"></span>
                                </div>
                                <span tabIndex="0">
                                    {RMResx.RM_JS_JM_Status_InProgerss}
                                </span>
                                <span
                                    className="reco-start-state-speed"
                                    tabIndex="0"
                                >
                                    {discoveredProcess}
                                </span>
                            </div>
                        </div>
                        <div className="reco-start-state">
                            <div tabIndex="0">
                                <span className="reco-start-state-discover">
                                    {RMResx.RM_FA_Discovery_JobPage_SitesSpeed}
                                </span>
                                <span className="reco-start-state-speed">
                                    {sitesSpeed}
                                </span>
                            </div>
                            <div>
                                <div className="reco-start-state-div">
                                    <span className="fia-in-progress reco-start-state-icon"></span>
                                </div>
                                <span tabIndex="0">
                                    {RMResx.RM_JS_JM_Status_InProgerss}
                                </span>
                                <span
                                    className="reco-start-state-speed"
                                    tabIndex="0"
                                >
                                    {sitesProcess}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div className="reco-start-content-right">
                        <img src={`${RM.gData.resCdnURL}/cloud%20records/discovery.svg`} />
                    </div>
                </section>
            </div>
        </div>
    );
};

export default AnalysisConfigurationRunningPage;
