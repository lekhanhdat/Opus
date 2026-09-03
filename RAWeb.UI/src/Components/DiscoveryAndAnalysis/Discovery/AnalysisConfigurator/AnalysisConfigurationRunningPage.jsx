import DiscoveryAndAnalysisNavigation from "../../Navigation";
import { useState } from "react";
import { DiscoveryDataSource } from "./Constants";
import { Office365AnalysisConfigurationRunningPage } from "./Office365";
import { SalesforceAnalysisConfigurationRunningPage } from "./Salesforce";
import { GoogleDriveAnalysisConfigurationRunningPage } from "./GoogleDrive";
import { FSAnalysisConfigurationRunningPage } from "./FileSystem";
import RouterUrls from "../../../../Constants/RouterUrls";

const AnalysisConfigurationEditPage = ({ history }) => {
    const [selectedDataSource, setSelectedDataSource] = useState(
        DiscoveryDataSource.None
    );

    const onDataSourceChange = (dataSource) => {
        setSelectedDataSource(dataSource);
    };

    return (
        <>
            <DiscoveryAndAnalysisNavigation
                history={history}
                redirect={{ need: true, url: RouterUrls.FA_Discovery }}
                onChange={onDataSourceChange}
                dataSources={[
                    DiscoveryDataSource.Office365,
                    DiscoveryDataSource.Salesforce,
                    DiscoveryDataSource.Google,
                    DiscoveryDataSource.FileSystem,
                ]}
            />
            {selectedDataSource === DiscoveryDataSource.Office365 && (
                <Office365AnalysisConfigurationRunningPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceAnalysisConfigurationRunningPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && (
                <GoogleDriveAnalysisConfigurationRunningPage
                    history={history}
                />
            )}
            {selectedDataSource === DiscoveryDataSource.FileSystem && (
                <FSAnalysisConfigurationRunningPage
                    history={history}
                />
            )}
        </>
    );
};

export default AnalysisConfigurationEditPage;
