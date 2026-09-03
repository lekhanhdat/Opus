import { useState } from "react";
import { DiscoveryDataSource } from "../Discovery/AnalysisConfigurator/Constants";
import DiscoveryAndAnalysisNavigation from "../Navigation";
import { Office365InactivePage } from "./Office365";
import { SalesforceInactivePage } from "./Salesforce";
import { GoogleDriveInactivePage } from "./GoogleDrive";
import { FileSystemInactivePage } from "./FileSystem";

const AnalysisInactivePage = ({history}) => {

    const [selectedDataSource, setSelectedDataSource] = useState(DiscoveryDataSource.None);

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
            {selectedDataSource === DiscoveryDataSource.Office365 && <Office365InactivePage/>}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceInactivePage />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && <GoogleDriveInactivePage />}
            {selectedDataSource === DiscoveryDataSource.FileSystem && <FileSystemInactivePage />}
        </>   
    )
};

export default AnalysisInactivePage;