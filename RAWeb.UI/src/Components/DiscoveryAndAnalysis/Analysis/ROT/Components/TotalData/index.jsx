import { useEffect, useState } from "react";
import TotalMutableData from "../../../Inactive/Components/Office365/TotalMutableData";
import { RotDataRequester, BasicDataRequester } from "../../../requests";

const ROTTotalData = ({ queryParameter, onQuery }) => {

    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
    });

    const [progressFileTotalSize, setProgressFileTotalSize] = useState(0);

    useEffect(() => {
        const fetchData = async () => {
            const res = await BasicDataRequester.getSummaryStatisticalDataInfo(queryParameter.o365TenantId);
            setProgressFileTotalSize(res.fileTotalSize);
        };
        fetchData();
    }, [queryParameter]);

    useEffect(() => {
        const fetchData = async () => {
            const res = await onQuery(queryParameter);
            setTotalDataInfo(res);
        };
        fetchData();
    }, [queryParameter]);

    const getSizeRate = () => {

        if(progressFileTotalSize === 0){
            return '0%';
        }

        let rate = (totalDataInfo.fileTotalSize / progressFileTotalSize / 1.0).toFixed(2);
        
        if(totalDataInfo.fileTotalSize > 0 && rate === "0.00"){
            return '1%';
        }

        return Number.parseInt(rate.replace('.', '')) + "%";
    };

    return (
        <>
            <div className="reco-mutable-data">
                <div className="reco-rot-mutable-data">
                    <TotalMutableData
                        data={[
                            {
                                text: RMResx.RM_FA_ROT_SummaryTab_ROTSizeTitle,
                                value: totalDataInfo.fileTotalSize,
                            },
                            {
                                text: RMResx.RM_FA_ROT_SummaryTab_ROTCountTitle,
                                value: totalDataInfo.fileSumCount,
                            },
                        ]}
                    />
                </div>
                <div className="reco-storage-style" tabIndex="0">
                    <$g.I18NProvider msg={RMResx.RM_FA_ROT_StorageMsg}>
                        <span className="reco-storage-text">
                            {getSizeRate()}
                        </span>
                        <span className="reco-storage-text">{progressFileTotalSize}</span>
                    </$g.I18NProvider>
                </div>
            </div>
        </>
    );
};

export default ROTTotalData;