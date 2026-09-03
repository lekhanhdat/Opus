import { useEffect, useMemo, useState } from "react";
import _ from "lodash";

import { GoogleDriveBasicDataRequester, GoogleDriveInactiveDataRequester } from "../../../../requests/GoogleDrive";
import PieChartWithProgress from "../../../../Components/PieChartWithProgress";
import { GoogleDriveTotalMutableData } from '../index'
import { NumberUtil } from "../../../../Utils";

function TotalData({ queryParameter }) {
    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
    });

    const [progressFileTotalSize, setProgressFileTotalSize] = useState(0);

    const totalData = useMemo(() => {
        return [
            {
                text: RMResx.RM_FA_GoogleDrive_TableColumn_TotalSize,
                value: totalDataInfo.fileTotalSize,
            },
            {
                text: RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_FileCount,
                value: totalDataInfo.fileSumCount,
            },
        ];
    }, [totalDataInfo]);

    useEffect(() => {
        const handler = async () => {
            const { organizationId } = queryParameter;
            if (organizationId) {
                const res =
                    await GoogleDriveBasicDataRequester.getSummaryStatisticalDataInfo(
                        organizationId
                    );
                setProgressFileTotalSize(res.fileTotalSize);
            }
        };
        handler();
    }, [queryParameter]);

    useEffect(() => {
        const handler = async () => {
            const res = await GoogleDriveInactiveDataRequester.queryAggregateInfo(
                queryParameter
            );
            setTotalDataInfo(res);
        };
        handler();
    }, [queryParameter]);

    const handleActivePieChart = () => {
        let value = 0;
        if (progressFileTotalSize === 0) {
            value = progressFileTotalSize;
        } else if (Number.parseInt((totalDataInfo.fileTotalSize / progressFileTotalSize / 1.0).toFixed(2).replace(".", "")) === 0 && totalDataInfo.fileTotalSize !== 0) {
            value = 1;
        } else {
            value = Number.parseInt((totalDataInfo.fileTotalSize / progressFileTotalSize / 1.0).toFixed(2).replace(".", ""));
        }
        return value;
    }

    return (
        <>
            <div className="reco-mutable-data">
                <GoogleDriveTotalMutableData data={totalData} />
            </div>
            <div className="reco-percentage-chart">
                <PieChartWithProgress
                    total={NumberUtil.internaltionalCounting(progressFileTotalSize)}
                    active={handleActivePieChart()}
                    name={RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_PieChartTitle}
                    unit={RMResx.RM_JS_RDM_CreateRule_Unit_GB}
                />
            </div>
        </>
    );
}

export default TotalData;
