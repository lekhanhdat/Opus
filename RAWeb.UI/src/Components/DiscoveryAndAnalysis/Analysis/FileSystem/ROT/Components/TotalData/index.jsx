import { useEffect, useMemo, useState } from "react";

import { FileSystemBasicDataRequester } from "../../../../requests/FileSystem";
import { FileSystemTotalMutableData } from "../../../Inactive/Components";
import { NumberUtil } from "../../../../Utils";

function ROTTotalData({ queryParameter, onQuery }) {
    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
    });

    const [progressFileTotalSize, setProgressFileTotalSize] = useState(0);

    const totalData = useMemo(() => {
        return [
            {
                text: RMResx.RM_FA_GoggleDrive_ROT_SummaryTab_ROTSizeTitle,
                value: totalDataInfo.fileTotalSize,
            },
            {
                text: RMResx.RM_FA_GoggleDrive_ROT_SummaryTab_ROTCountTitle,
                value: totalDataInfo.fileSumCount,
            },
        ];
    }, [totalDataInfo]);

    useEffect(() => {
        const handler = async () => {
            const res =
                await FileSystemBasicDataRequester.getSummaryStatisticalDataInfo();
            setProgressFileTotalSize(res.fileTotalSize);
        };
        handler();
    }, [queryParameter]);

    useEffect(() => {
        const handler = async () => {
            const res = await onQuery(queryParameter);
            setTotalDataInfo(res);
        };
        handler();
    }, [queryParameter]);

    const getSizeRate = () => {
        if (progressFileTotalSize === 0) {
            return "0%";
        }

        let rate = (
            totalDataInfo.fileTotalSize /
            progressFileTotalSize /
            1.0
        ).toFixed(2);

        if (totalDataInfo.fileTotalSize > 0 && rate === "0.00") {
            return "1%";
        }

        return Number.parseInt(rate.replace(".", "")) + "%";
    };

    return (
        <div className="reco-mutable-data">
            <div className="reco-rot-mutable-data">
                <FileSystemTotalMutableData data={totalData} />
            </div>
            <div
                className="reco-storage-style"
                tabIndex="0"
                data-tooltip="ifneed"
            >
                <$g.I18NProvider msg={RMResx.RM_FA_ROT_StorageMsg}>
                    <span className="reco-storage-text">{getSizeRate()}</span>
                    <span className="reco-storage-text">
                        {NumberUtil.internaltionalCounting(progressFileTotalSize)}
                    </span>
                </$g.I18NProvider>
            </div>
        </div>
    );
}

export default ROTTotalData;
