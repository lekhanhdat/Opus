import ContainerStorageOptimization from "./ContainerStorageOptimization";
import SiteOptimizationTable from "./SiteOptimizationTable";
import TotalStatistics from "./TotalStatistics";
import "./index.less";

const Progress = ({o365TenantId}) => {
    return (
        <div className="reco-analysis-progress">
            <div className="reco-title" tabIndex="0">{RMResx.RM_FA_Inactive_SummaryTab_SummaryTitle}</div>
            <div className="reco-summary">
                <TotalStatistics o365TenantId={o365TenantId}/>
                <ContainerStorageOptimization o365TenantId={o365TenantId}/>
            </div>
            <div className="reco-title" tabIndex="0">{RMResx.RM_FA_Progress_SiteOptimization}</div>
            <div className="reco-optimization">
                <SiteOptimizationTable o365TenantId={o365TenantId}/>
            </div>
        </div>
    )
};

export default Progress;