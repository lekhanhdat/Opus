import { useState } from "react";
import ImmutableDataCard from "../../../Components/ImmutableDataCard";
import MutableDataCard from "../../../Components/MutableDataCard";
import { useEffect } from "react";
import ProgressRequester from "../../../requests/ProgressRequester";

const TotalStatistics = ({o365TenantId}) => {

    const [totalStatisticsInfo, setTotalStatisticsInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
        nextOptimizableFileTotalSize: 0,
        nextOptimizableVersionTotalSize: 0,
        archived: 0,
        deleted: 0
    });

    useEffect(() => {
        const fetchData = async () => {
            if (_.isNil(o365TenantId)) {
                return;
            }
            const info = await ProgressRequester.getSummaryOptimizedInfo(o365TenantId);
            setTotalStatisticsInfo(info);
        };

        fetchData();
    }, [o365TenantId]);

    return (
        <div className="reco-progress-total-statistics">
            <ImmutableDataCard
                name={RMResx.RM_JS_JMD_Summary_DataSize}
                value={totalStatisticsInfo.fileTotalSize}
                unit={RMResx.RM_DSB_Unit_GB}
            />
            <ImmutableDataCard
                name={RMResx.RM_FA_Inactive_SummaryTab_FileCount}
                value={totalStatisticsInfo.fileSumCount}
                unit={""}
            />
            <ImmutableDataCard
                name={RMResx.RM_FA_Progress_SummaryTab_NextFile}
                value={totalStatisticsInfo.nextOptimizableFileTotalSize}
                unit={RMResx.RM_DSB_Unit_GB}
            />
            <ImmutableDataCard
                name={RMResx.RM_FA_Progress_SummaryTab_NextVersion}
                value={totalStatisticsInfo.nextOptimizableVersionTotalSize}
                unit={RMResx.RM_DSB_Unit_GB}
                tooltip={RMResx.RM_FA_Progress_SummaryTab_Version_Tooltip}
            />
            <ImmutableDataCard
                name={RMResx.RM_FA_Progress_SummaryTab_Archived}
                value={totalStatisticsInfo.archived}
                unit={RMResx.RM_DSB_Unit_GB}
            />
            <ImmutableDataCard
                name={RMResx.RM_FA_Progress_SummaryTab_Deleted}
                value={totalStatisticsInfo.deleted}
                unit={RMResx.RM_DSB_Unit_GB}
            />
        </div>
    );
};

export default TotalStatistics;