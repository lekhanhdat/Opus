import { useEffect, useMemo, useState } from "react";
import SFTotalMutableData from "../../Salesforce/SFTotalMutableData";
import { SFInactiveDataRequester } from "../../../../requests";

const SFTotalData = ({ queryParameter }) => {
    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        dataTotalSize: 0,
        recordsTotalCount:0
    });

    useEffect(() => {
        const fetchData = async () => {
            const res = await SFInactiveDataRequester.queryAggregateInfo(queryParameter);
            setTotalDataInfo(res);
        };
        fetchData();
    }, [queryParameter]);

    const totalData = useMemo(() => {
        return [
            {
                text: RMResx.RM_FA_SF_Inactive_SummaryTab_RecordsCount,
                value: totalDataInfo.recordsTotalCount,
            },
            {
                text: RMResx.RM_FA_SF_Inactive_OptimizationTab_DataSizeTitle,
                value: totalDataInfo.dataTotalSize,
            },
            {
                text: RMResx.RM_FA_SF_Inactive_OptimizationTab_FileSizeTitle,
                value: totalDataInfo.fileTotalSize,
            },
        ];
    }, [totalDataInfo])

    return (
        <div className="sf-reco-mutable-data">
            <SFTotalMutableData data={totalData} />
            {/* <div>
                <$g.I18NProvider msg={RMResx.RM_FA_SF_Inactive_SummaryTab_InactiveRangeDesc}>
                    <span className="emphasize-text">9%</span>
                    <span className="emphasize-text">2288</span>
                </$g.I18NProvider>
            </div> */}
        </div>
    );
};

export default SFTotalData;
