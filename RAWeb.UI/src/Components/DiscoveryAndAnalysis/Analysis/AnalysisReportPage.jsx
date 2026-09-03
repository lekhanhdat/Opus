import { useState } from "react";
import { DiscoveryDataSource } from "../Discovery/AnalysisConfigurator/Constants";
import DiscoveryAndAnalysisNavigation from "../Navigation";
import { Office365ReportPage } from "./Office365";

const AnalysisReportPage = ({history}) => {

    const [selectedDataSource, setSelectedDataSource] = useState(DiscoveryDataSource.None);

    const onDataSourceChange = (dataSource) => {
        setSelectedDataSource(dataSource);
    };

    return (
        <>
        <DiscoveryAndAnalysisNavigation onChange={onDataSourceChange} dataSources={[DiscoveryDataSource.Office365]}/>
        {
            selectedDataSource === DiscoveryDataSource.Office365 && <Office365ReportPage/>
        }
        </>
    )
};

export default AnalysisReportPage;