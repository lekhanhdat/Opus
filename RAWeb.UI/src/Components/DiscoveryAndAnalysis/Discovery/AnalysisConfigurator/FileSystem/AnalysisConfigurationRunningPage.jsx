import { useState, useEffect } from "react";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { DiscoveryJobStatus } from "../../../Analysis/Constants";
import RouterUrls from "../../../../../Constants/RouterUrls";
import { DiscoveryDataSource } from "../Constants";

const AnalysisConfigurationRunningPage = ({ history }) => {
    const [discoveredSpeed, setDiscoveredSpeed] = useState(0);

    const [discoveredProcess, setDiscoveredProcess] = useState(0);

    const [connectionSpeed, setConnectionSpeed] = useState(0);

    const [connectionProcess, setConnectionProcess] = useState(0);

    useEffect(() => {
        const fetchData = async () => {
            $$.loading(true);
            const { siteProgressInfo, status } = await fetchUtility({
                url: "/api/RMDiscoveryFSJobManagementApi/GetLatest",
                method: "Get",
            });

            let discoveredSpeed = "0/0";
            let discoveredProcess = "0%";
            let connectionSpeed = "0/0";
            let connectionProcess = "0%";

            if (siteProgressInfo.needProcessCount) {
                const completedCount = siteProgressInfo.succeedCount + siteProgressInfo.failedCount;
                discoveredSpeed = `${siteProgressInfo.discoveredCount}/${siteProgressInfo.needProcessCount}`;
                discoveredProcess =
                Number.parseInt(
                    (
                        siteProgressInfo.discoveredCount /
                        siteProgressInfo.needProcessCount
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
                connectionSpeed = `${completedCount}/${siteProgressInfo.needProcessCount}`
                connectionProcess =
                Number.parseInt(
                    (
                        completedCount /
                        siteProgressInfo.needProcessCount
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";

                if (discoveredProcess === "100%") {
                    discoveredProcess = "99%";
                }

                if (connectionProcess === "100%") {
                    connectionProcess = "99%";
                }
            }        

            setDiscoveredSpeed(discoveredSpeed);
            setDiscoveredProcess(discoveredProcess);
            setConnectionSpeed(connectionSpeed);
            setConnectionProcess(connectionProcess);

            $$.loading(false);

            if (
                status === DiscoveryJobStatus.Finished ||
                status === DiscoveryJobStatus.Failed ||
                status === DiscoveryJobStatus.Exception
            ) {
                history.push({
                    pathname: RouterUrls.FA_Discovery_Finish,
                    search: `?dataSource=${DiscoveryDataSource.FileSystem}`,
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
                            {RMResx.RM_FA_FS_Discovery_JobPage_Des02}
                        </div>
                        <div className="reco-start-state">
                            <div tabIndex="0">
                                <span className="reco-start-state-discover">
                                    {RMResx.RM_FA_Discovery_JobPage_DiscoveredConnectionsSpeed}
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
                                    {RMResx.RM_FA_Discovery_JobPage_ConnectionSpeed}
                                </span>
                                <span className="reco-start-state-speed">
                                    {connectionSpeed}
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
                                    {connectionProcess}
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
