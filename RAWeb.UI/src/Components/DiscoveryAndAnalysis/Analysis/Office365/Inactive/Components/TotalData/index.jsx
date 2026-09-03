import { useEffect, useState } from "react";
import { InactiveDataRequester, BasicDataRequester } from "../../../../requests";
import TotalMutableData from "../TotalMutableData";
import PieChartWithProgress from "../../../../Components/PieChartWithProgress";
import { NumberUtil } from "../../../../Utils";

const TotalData = ({ queryParameter, tab }) => {

    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
    });

    const [progressFileTotalSize, setProgressFileTotalSize] = useState(0);

    useEffect(() => {
        const fetchSummaryStatisticalDataInfo = async () => {
            const res = await BasicDataRequester.getSummaryStatisticalDataInfo(queryParameter.o365TenantId);
            setProgressFileTotalSize(res.fileTotalSize);
        };
        fetchSummaryStatisticalDataInfo();
    }, [queryParameter]);

    useEffect(() => {
        const fetchAggregateInfo = async () => {
            const res = await InactiveDataRequester.queryAggregateInfo(queryParameter);
            setTotalDataInfo(res);
        };
        fetchAggregateInfo();
    }, [queryParameter]);

    const getTotalData = (tab) => {
        return [
            {
                text: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeTitle,
                value: totalDataInfo.fileTotalSize,
            },
            {
                text: RMResx.RM_FA_Inactive_OptimizationTab_FileCountsTitle,
                value: totalDataInfo.fileSumCount,
            },
        ];
    };

    return (
        <>
            <div className="reco-mutable-data">
                <TotalMutableData
                    data={getTotalData(tab)}
                />
            </div>
            <div className="reco-percentage-chart">
                <PieChartWithProgress
                    total={NumberUtil.internaltionalCounting(progressFileTotalSize)}
                    active={progressFileTotalSize === 0 ? progressFileTotalSize : (
                        Number.parseInt((totalDataInfo.fileTotalSize / progressFileTotalSize / 1.0).toFixed(2).replace('.', '')) === 0 && totalDataInfo.fileTotalSize !== 0 ? 
                            1 : Number.parseInt((totalDataInfo.fileTotalSize / progressFileTotalSize / 1.0).toFixed(2).replace('.', '')))
                    }
                    name={RMResx.RM_FA_Inactive_SummaryTab_PieChartTitle}
                    unit={RMResx.RM_JS_RDM_CreateRule_Unit_GB}
                />
            </div>
        </>
    );
};

export default TotalData;
