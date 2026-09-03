import DiscoveryAndAnalysisNavigation from "../../Navigation";
import { useState } from "react";
import { DiscoveryDataSource } from "./Constants";
import { Office365AnalysisConfigurationInitializationPage } from "./Office365";
import { SalesforceAnalysisConfigurationInitializationPage } from "./Salesforce";
import { GoogleDriveAnalysisConfigurationInitializationPage } from "./GoogleDrive";
import { FSAnalysisConfigurationInitializationPage } from "./FileSystem";

const AnalysisConfigurationInitializationPage = ({ history }) => {
    const [selectedDataSource, setSelectedDataSource] = useState(
        DiscoveryDataSource.None
    );

    const onDataSourceChange = (dataSource) => {
        setSelectedDataSource(dataSource);
    };

    return (
        <>
            <DiscoveryAndAnalysisNavigation
                onChange={onDataSourceChange}
                dataSources={[
                    DiscoveryDataSource.Office365,
                    DiscoveryDataSource.Salesforce,
                    DiscoveryDataSource.Google,
                    DiscoveryDataSource.FileSystem,
                ]}
            />
            {selectedDataSource === DiscoveryDataSource.Office365 && (
                <Office365AnalysisConfigurationInitializationPage
                    history={history}
                />
            )}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceAnalysisConfigurationInitializationPage
                    history={history}
                />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && (
                <GoogleDriveAnalysisConfigurationInitializationPage
                    history={history}
                />
            )}
            {selectedDataSource === DiscoveryDataSource.FileSystem && (
                <FSAnalysisConfigurationInitializationPage
                    history={history}
                />
            )}
        </>
    );
};

export default AnalysisConfigurationInitializationPage;
