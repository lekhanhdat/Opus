import { useEffect, useState } from "react";
import _ from "lodash";

import { FileSystemBasicDataRequester } from "../../../../requests/FileSystem";
import ImmutableDataCard from "../../../../Components/ImmutableDataCard";

import "./index.less";

function TotalSummary({ queryParameter }) {
    const [dataInfo, setDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
        maxFileAge: 0,
    });

    useEffect(() => {
        const handler = async () => {
            const res =
                await FileSystemBasicDataRequester.getSummaryStatisticalDataInfo();
            setDataInfo(res);
        };
        handler();
    }, []);

    return (
        <section id="reco-google-total-summary">
            <div className="title">
                <span tabIndex="0">
                    {RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_SummaryTitle}
                </span>
            </div>
            <div className="cards">
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={
                            RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_TotalSize
                        }
                        value={dataInfo.fileTotalSize}
                        unit={RMResx.RM_DSB_Unit_GB}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={
                            RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_FileCount
                        }
                        value={dataInfo.fileSumCount}
                        unit={""}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={
                            RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_MaxFileAge
                        }
                        value={dataInfo.maxFileAge}
                        unit={RMResx.RM_JS_RDM_CreateRule_Unit_Months}
                    />
                </div>
            </div>
        </section>
    );
}

export default TotalSummary;
