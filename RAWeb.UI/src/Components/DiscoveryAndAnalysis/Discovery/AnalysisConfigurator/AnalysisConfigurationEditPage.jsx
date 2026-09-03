import DiscoveryAndAnalysisNavigation from "../../Navigation";
import { useState } from "react";
import { DiscoveryDataSource } from "./Constants";
import { Office365AnalysisConfigurationEditPage } from "./Office365";
import { SalesforceAnalysisConfigurationEditPage } from "./Salesforce";
import { GoogleDriveAnalysisConfigurationEditPage } from "./GoogleDrive";
import { FSAnalysisConfigurationEditPage } from "./FileSystem";
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
                <Office365AnalysisConfigurationEditPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceAnalysisConfigurationEditPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && (
                <GoogleDriveAnalysisConfigurationEditPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.FileSystem && (
                <FSAnalysisConfigurationEditPage history={history} />
            )}
        </>
    );
};

export default AnalysisConfigurationEditPage;
