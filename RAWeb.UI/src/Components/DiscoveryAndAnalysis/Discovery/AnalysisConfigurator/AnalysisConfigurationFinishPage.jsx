import DiscoveryAndAnalysisNavigation from "../../Navigation";
import { useState } from "react";
import { DiscoveryDataSource } from "./Constants";
import { Office365AnalysisConfigurationFinishPage } from "./Office365";
import { SalesforceAnalysisConfigurationFinishPage } from "./Salesforce";
import { GoogleDriveAnalysisConfigurationFinishPage } from "./GoogleDrive";
import { FSAnalysisConfigurationFinishPage } from "./FileSystem";
import RouterUrls from "../../../../Constants/RouterUrls";

const AnalysisConfigurationFinishPage = ({ history }) => {
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
                <Office365AnalysisConfigurationFinishPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceAnalysisConfigurationFinishPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && (
                <GoogleDriveAnalysisConfigurationFinishPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.FileSystem && (
                <FSAnalysisConfigurationFinishPage history={history} />
            )}
        </>
    );
};

export default AnalysisConfigurationFinishPage;
