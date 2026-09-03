import { useEffect, useState } from "react";
import _ from "lodash";

import { GoogleDriveBasicDataRequester } from "../../../../requests/GoogleDrive";
import ImmutableDataCard from "../../../../Components/ImmutableDataCard";

import "./index.less";

function TotalSummary({ queryParameter }) {
    const [dataInfo, setDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
        maxFileAge: 0,
    });

    const [organizationId, setOrganizationId] = useState();

    useEffect(() => {
        const { organizationId } = queryParameter;
        if (!_.isNil(organizationId) && !_.isEmpty(organizationId)) {
            setOrganizationId(organizationId);
        }
    }, [queryParameter]);

    useEffect(() => {
        const handler = async () => {
            if (_.isNil(organizationId)) {
                return;
            }

            const res = await GoogleDriveBasicDataRequester.getSummaryStatisticalDataInfo(
                organizationId
            );
            setDataInfo(res);
        };
        handler();
    }, [organizationId]);

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
                        name={RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_TotalSize}
                        value={dataInfo.fileTotalSize}
                        unit={RMResx.RM_DSB_Unit_GB}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_FileCount}
                        value={dataInfo.fileSumCount}
                        unit={""}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_MaxFileAge}
                        value={dataInfo.maxFileAge}
                        unit={RMResx.RM_JS_RDM_CreateRule_Unit_Months}
                    />
                </div>
            </div>
        </section>
    );
}

export default TotalSummary;
