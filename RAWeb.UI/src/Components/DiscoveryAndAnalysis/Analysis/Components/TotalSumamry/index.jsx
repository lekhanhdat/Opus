import { useEffect, useState } from "react";
import ImmutableDataCard from "../ImmutableDataCard";
import _ from "lodash";
import { BasicDataRequester } from "../../requests";
import "./index.less";
import { TotalSummaryCard } from "../../Constants";

const TotalSummary = ({
    queryParameter,
    enableDispalyDuplicateFileTotalSize = false,
    cardActions
}) => {
    const [dataInfo, setDataInfo] = useState({
        fileTotalSize: 0,
        fileSumCount: 0,
        maxFileAge: 0,
        totalVersionSize: 0,
        phlVolume: 0,
        duplicateFileTotalSize: -1,
    });

    const [o365TenantId, setO365TenantId] = useState();

    useEffect(() => {
        const tenantId = queryParameter.o365TenantId;
        if (!_.isNil(tenantId) && !_.isEmpty(tenantId)) {
            setO365TenantId(tenantId);
        }
    }, [queryParameter]);

    useEffect(() => {
        const handler = async () => {
            if (_.isNil(o365TenantId)) {
                return;
            }

            const res = await BasicDataRequester.getSummaryStatisticalDataInfo(
                o365TenantId
            );
            setDataInfo(res);
        };
        handler();
    }, [o365TenantId]);

    return (
        <section className="reco-total-summary">
            <div className="title">
                <span tabIndex="0">
                    {RMResx.RM_FA_Inactive_SummaryTab_SummaryTitle}
                </span>
            </div>
            <div className="cards">
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_Inactive_SummaryTab_TotalSize}
                        value={dataInfo.fileTotalSize}
                        unit={RMResx.RM_DSB_Unit_GB}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_Inactive_SummaryTab_FileCount}
                        value={dataInfo.fileSumCount}
                        unit={""}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_Inactive_SummaryTab_MaxFileAge}
                        value={dataInfo.maxFileAge}
                        unit={RMResx.RM_JS_RDM_CreateRule_Unit_Months}
                    />
                </div>
                <div tabIndex="0">
                    <ImmutableDataCard
                        name={RMResx.RM_FA_Inactive_SummaryTab_TotalVersionSize}
                        value={dataInfo.totalVersionSize}
                        unit={RMResx.RM_DSB_Unit_GB}
                    />
                </div>
                <div tabIndex="0">
                    {enableDispalyDuplicateFileTotalSize &&
                    dataInfo.duplicateFileTotalSize > -1 ? (
                        <ImmutableDataCard
                            name={RMResx.RM_DA_Analysis_TotalSummamy_DuplicateDataSize}
                            value={dataInfo.duplicateFileTotalSize}
                            unit={RMResx.RM_DSB_Unit_GB}
                            cardAction={cardActions?.[TotalSummaryCard.DuplicateDataSize]}
                        />
                    ) : (
                        <ImmutableDataCard
                            name={(
                                <div className="flex align-center">
                                    <span className="ra-webkit-box-ellipsis" data-tooltip="ifneed">{RMResx.RM_FA_Inactive_SummaryTab_PHL}</span>
                                    <$g.Popover style={{ marginTop: 2, marginBottom: 0 }}>{RMResx.RM_FA_Inactive_SummaryTab_PHL_Tips}</$g.Popover>
                                </div>
                            )}
                            value={dataInfo.phlVolume}
                            unit={RMResx.RM_DSB_Unit_GB}
                        />
                    )}
                </div>
            </div>
        </section>
    );
};

export default TotalSummary;
