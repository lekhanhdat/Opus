import { useEffect, useState } from "react";

import EmptyContent from "../../Components/EmptyContent";
import {
    ArchiverDataUnit,
    ArchiverDataUnitName,
} from "../../SOAdminView/config";

import "./index.less";

const DefaultTotalInfo = {
    TotalSize: "",
    ArchiverDataUnit: ArchiverDataUnit.Unknown,
};

function DeletedData({ archivedRetentionData }) {
    const [noData, setNoData] = useState(true);
    const [data, setData] = useState({
        totalArchived: DefaultTotalInfo,
        fileNumber: DefaultTotalInfo,
        runJobDate: "",
    });

    useEffect(() => {
        if (archivedRetentionData) {
            setData({
                totalArchived: {
                    TotalSize: archivedRetentionData.TotalSize,
                    ArchiverDataUnit: archivedRetentionData.TotalSizeDataUnit,
                },
                fileNumber: {
                    TotalSize: archivedRetentionData.TotalNumber,
                    ArchiverDataUnit: archivedRetentionData.TotalNumberDataUnit,
                },
                runJobDate: archivedRetentionData.DeleteTime,
            });
            setNoData(false);
        }
    }, [archivedRetentionData]);

    return (
        <div id="reco-dashboard-deleted-data">
            <div style={{ justifyContent: "space-between" }} className="flex">
                <div className="reco-dashboard-cards-title" tabIndex="0">
                    {RMResx.RM_DSB_Retention_Title}
                </div>
            </div>
            <div className="reco-dashboard-size-cards">
                <div className="reco-dashboard-size-card">
                    <div
                        className="reco-dashboard-size-card-title"
                        style={{ lineHeight: "34px" }}
                        tabIndex="0"
                    >
                        {RMResx.RM_DSB_Retention_Title_Archived}
                    </div>
                    <EmptyContent isEmpty={noData}>
                        <div
                            className="reco-dashboard-size-card-data"
                            tabIndex="0"
                        >
                            <span className="reco-dashboard-size-number">
                                {data.totalArchived.TotalSize || "0"}
                            </span>
                            <span className="reco-dashboard-size-unit">
                                {" "}
                                {
                                    ArchiverDataUnitName[
                                        data.totalArchived.ArchiverDataUnit
                                    ]
                                }
                            </span>
                        </div>
                    </EmptyContent>
                </div>
                <div className="reco-dashboard-size-card">
                    <div
                        className="reco-dashboard-size-card-title"
                        style={{ lineHeight: "34px" }}
                        tabIndex="0"
                    >
                        {RMResx.RM_DSB_Retention_Title_ArchivedFiles}
                    </div>
                    <EmptyContent isEmpty={noData}>
                        <div
                            className="reco-dashboard-size-card-data"
                            tabIndex="0"
                        >
                            <span className="reco-dashboard-size-number">
                                {data.fileNumber.TotalSize || "0"}
                            </span>
                            <span className="reco-dashboard-size-unit">
                                {" "}
                                {
                                    ArchiverDataUnitName[
                                        data.fileNumber.ArchiverDataUnit
                                    ]
                                }
                            </span>
                        </div>
                    </EmptyContent>
                </div>
                <div className="reco-dashboard-size-card">
                    <div
                        className="reco-dashboard-size-card-title"
                        style={{ lineHeight: "34px" }}
                        tabIndex="0"
                    >
                        {RMResx.RM_DSB_Retention_Title_DeletedDate}
                    </div>
                    <EmptyContent isEmpty={noData}>
                        <div
                            className="reco-dashboard-size-card-data"
                            tabIndex="0"
                        >
                            <span className="reco-dashboard-size-number">
                                {data.runJobDate}
                            </span>
                        </div>
                    </EmptyContent>
                </div>
            </div>
        </div>
    );
}

export default DeletedData;
