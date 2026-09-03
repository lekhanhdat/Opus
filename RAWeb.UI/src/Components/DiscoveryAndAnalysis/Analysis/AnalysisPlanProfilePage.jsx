import { useState } from "react";
import { DiscoveryDataSource } from "../Discovery/AnalysisConfigurator/Constants";
import DiscoveryAndAnalysisNavigation from "../Navigation";
import { Office365PlanProfilePage } from "./Office365";

const AnalysisPlanProfilePage = ({history}) => {

    const [selectedDataSource, setSelectedDataSource] = useState(DiscoveryDataSource.None);

    const onDataSourceChange = (dataSource) => {
        setSelectedDataSource(dataSource);
    };

    return (
        <>
        <DiscoveryAndAnalysisNavigation onChange={onDataSourceChange} dataSources={[DiscoveryDataSource.Office365]}/>
        {
            selectedDataSource === DiscoveryDataSource.Office365 && <Office365PlanProfilePage/>
        }
        </>
    )
};

export default AnalysisPlanProfilePage;