import { useEffect, useState } from "react";
import { DiscoveryJobStatus, DiscoveryDataSource } from "../Constants";
import RouterUrls from "../../../../../Constants/RouterUrls";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";

const AnalysisConfigurationInitializationPage = ({ history }) => {
    const [jobInfos, setJobInfos] = useState({});

    useEffect(() => {
        const fetchJobStatusInfo = async () => {
            $$.loading(true);
            const jobStatusInfo = await fetchUtility({
                url: "/api/RMDiscoverySalesforceJobManagementApi/GetLatest",
                method: "Get",
            });
            setJobInfos(jobStatusInfo);
            switch (jobStatusInfo.status) {
                case DiscoveryJobStatus.Preparing:
                case DiscoveryJobStatus.Pending:
                case DiscoveryJobStatus.Running:
                    history.push({
                        pathname: RouterUrls.FA_Discovery_RunJob,
                        search: `?dataSource=${DiscoveryDataSource.Salesforce}`
                    });
                    break;
                case DiscoveryJobStatus.Finished:
                case DiscoveryJobStatus.Failed:
                case DiscoveryJobStatus.Exception:
                    history.push({
                        pathname: RouterUrls.FA_Discovery_Finish,
                        search: `?dataSource=${DiscoveryDataSource.Salesforce}`
                    });
                    break;
                default:
                    history.push({
                        pathname: RouterUrls.FA_Discovery,
                        search: `?dataSource=${DiscoveryDataSource.Salesforce}`
                    });
                    break;
            }
            $$.loading(false);
        };

        fetchJobStatusInfo();
    }, []);

    const onStart = () => {
        history.push({
            pathname: RouterUrls.FA_Discovery_Configuration,
            search: `?dataSource=${DiscoveryDataSource.Salesforce}`,
            state: jobInfos,
        });
    };

    return (
        <div id="raDiscovery">
            <$g.SiteMap data={[SiteMapLinks.FA_Discovery]} />

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
                                {RMResx.RM_FA_Salesforce_Discovery_HomeDes02}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Salesforce_Discovery_HomeDes03}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Salesforce_Discovery_HomeDes04}
                            </div>
                            <div className="reco-discovery-text3">
                                {RMResx.RM_FA_Salesforce_Discovery_HomeDes05}
                            </div>
                        </div>
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
        </div>
    );
};
export default AnalysisConfigurationInitializationPage;
