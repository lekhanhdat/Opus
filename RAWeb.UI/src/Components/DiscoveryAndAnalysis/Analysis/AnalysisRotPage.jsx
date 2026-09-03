import { useState } from "react";
import { DiscoveryDataSource } from "../Discovery/AnalysisConfigurator/Constants";
import DiscoveryAndAnalysisNavigation from "../Navigation";
import { Office365ROTPage } from "./Office365";
import { GoogleDriveROTPage } from "./GoogleDrive";
import { FileSystemROTPage } from "./FileSystem";

const AnalysisRotPage = ({history}) => {

    const [selectedDataSource, setSelectedDataSource] = useState(DiscoveryDataSource.None);

    const onDataSourceChange = (dataSource) => {
        setSelectedDataSource(dataSource);
    };

    return (
        <>
        <DiscoveryAndAnalysisNavigation onChange={onDataSourceChange} dataSources={[DiscoveryDataSource.Office365, DiscoveryDataSource.Google, DiscoveryDataSource.FileSystem]}/>
        {
            selectedDataSource === DiscoveryDataSource.Office365 && <Office365ROTPage/>
        }
        {selectedDataSource === DiscoveryDataSource.Google && <GoogleDriveROTPage />}
        {selectedDataSource === DiscoveryDataSource.FileSystem && <FileSystemROTPage />}
        </>
    )
};

export default AnalysisRotPage;