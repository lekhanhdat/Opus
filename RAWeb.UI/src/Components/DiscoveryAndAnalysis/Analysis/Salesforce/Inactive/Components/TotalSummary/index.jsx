import { useEffect, useState } from "react";
import _ from "lodash";

import { SFBasicDataRequester } from "../../../../requests";
import { UnitConvertsionUtil } from "../../../../Utils";
import { SalesforceImmutableDataCard } from "../index";

import "./index.less";

const TotalSummary = () => {
    const [dataInfo, setDataInfo] = useState({
        BiggestObjectByDataSize: "",
        BiggestObjectByFileSize: "",
        BiggestObjectByRecordCount: "",
        DataStorageUsage: 0,
        DataTotalSize: 0,
        FileStorageUsage: 0,
        FileTotalSize: 0,
        ObjectTotalCount: 0,
        OldestRecords: 0,
        RecordsTotalCount: 0,
    });

    useEffect(() => {
        const fetchData = async () => {
            const res = await SFBasicDataRequester.getSummaryStatisticalDataInfo();
            setDataInfo(res);
        };
        fetchData();
    }, []);

    const summaryObjects = [
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_TotalObject,
            value: dataInfo.ObjectTotalCount,
            unit: "",
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_RecordsCount,
            value: dataInfo.RecordsTotalCount,
            unit: "",
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_MaxObject,
            value: dataInfo.BiggestObjectByRecordCount,
            unit: "",
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_OldestRecords,
            value: dataInfo.OldestRecords,
            unit: RMResx.RM_JS_RDM_CreateRule_Unit_Months,
        },
    ]
    
    const summaryData = [
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_DataSize,
            value: UnitConvertsionUtil.DynamicConvert(dataInfo.DataTotalSize, 2),
            unit: UnitConvertsionUtil.GetUnitI18N(UnitConvertsionUtil.GetUnit(dataInfo.DataTotalSize)),
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_StorageUsage,
            value: dataInfo.DataStorageUsage + "%",
            unit: "",
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_BiggestObject,
            value: dataInfo.BiggestObjectByDataSize,
            unit: "",
        },

        /** 
        ** Hidden in this release, It'll be implemented later 
        ** Hide the "Yearly cost" card
        */
        // {
        //     name: RMResx.RM_FA_SF_Inactive_SummaryTab_YearlyCost,
        //     value: "",
        //     unit: RMResx.RM_FA_SF_Inactive_SummaryTab_ComingSoon,
        // },
    ]
    
    const summaryFiles = [
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_FileSize,
            value: UnitConvertsionUtil.DynamicConvert(dataInfo.FileTotalSize, 2),
            unit: UnitConvertsionUtil.GetUnitI18N(UnitConvertsionUtil.GetUnit(dataInfo.FileTotalSize)),
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_StorageUsage,
            value: dataInfo.FileStorageUsage + "%",
            unit: "",
        },
        {
            name: RMResx.RM_FA_SF_Inactive_SummaryTab_BiggestObject,
            value: dataInfo.BiggestObjectByFileSize,
            unit: "",
        },

        /** 
        ** Hidden in this release, It'll be implemented later 
        ** Hide the "Yearly cost" card
        */
        // {
        //     name: RMResx.RM_FA_SF_Inactive_SummaryTab_YearlyCost,
        //     value: "",
        //     unit: RMResx.RM_FA_SF_Inactive_SummaryTab_ComingSoon,
        // },
    ]

    return (
        <section className="reco-sf-total-summary">
            <div className="title">
                <span tabIndex="0">
                    {RMResx.RM_FA_SF_Inactive_SummaryTab_SummaryTitle}
                </span>
            </div>
            <div className="summary-section">
                <span tabIndex="0" className="label">
                    {RMResx.RM_FA_SF_Inactive_SummaryTab_ObjectSection}
                </span>
                <div className="cards">
                    {summaryObjects.map((item, index) => (
                        <div key={index} tabIndex="0">
                            <SalesforceImmutableDataCard
                                name={item.name}
                                value={item.value}
                                unit={item.unit}
                            />
                        </div>
                    ))}
                </div>
            </div>
            <div className="summary-section">
                <span tabIndex="0" className="label">
                    {RMResx.RM_FA_SF_Inactive_SummaryTab_DataSection}
                </span>
                <div className="cards">
                    {summaryData.map((item, index) => (
                        <div key={index} tabIndex="0">
                            <SalesforceImmutableDataCard
                                name={item.name}
                                value={item.value}
                                unit={item.unit}
                            />
                        </div>
                    ))}
                </div>
            </div>
            <div className="summary-section">
                <span tabIndex="0" className="label">
                    {RMResx.RM_FA_SF_Inactive_SummaryTab_FileSection}
                </span>
                <div className="cards">
                    {summaryFiles.map((item, index) => (
                        <div key={index} tabIndex="0">
                            <SalesforceImmutableDataCard
                                name={item.name}
                                value={item.value}
                                unit={item.unit}
                            />
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default TotalSummary;
