import { useState, useRef } from "react";
import _ from "lodash";

import WithoutModifiedDate from "../../../Components/WithoutModifiedDate";
import SFTotalSummary from "../../../Components/TotalSumamry/Salesforce";
import SFTotalData from "../../Components/Salesforce/SFTotalData";
import SFDataAnalysis from "../../Components/Salesforce/SFDataAnalysis";
import SFFileAnalysis from "../../Components/Salesforce/SFFileAnalysis";
import { DiscoveryQueryDataType } from "../../../Constants";
import { SFInactiveDataRequester } from "../../../requests";

import "../index.less";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Inactive,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
};

const SFInactiveSummaryV3 = () => {
    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    const dataAnalysisRef = useRef(null)
    const fileAnalysisRef = useRef(null)

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await SFInactiveDataRequester.queryAnalysis(queryParameter);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await SFInactiveDataRequester.querySummaryNodeTotalAggregateInfo(queryParameter);
        return res;
    };

    const onWithoutModifedDateChange = (value)=>{
        setQueryParameter(value);
        dataAnalysisRef.current?.onChangeWithoutDate(value?.withoutDateQueryParameter);
        fileAnalysisRef.current?.onChangeWithoutDate(value?.withoutDateQueryParameter);
    }

    return (
        <div className="reco-inactive-summary-container">
            {/* Summary */}
            <div>
                <SFTotalSummary queryParameter={queryParameter} />
            </div>

            {/* Definition of inactive (Modified time based) */}
            <div className="reco-data">
                <section className="reco-title">
                    <span tabIndex="0">
                        {RMResx.RM_FA_SF_Inactive_SummaryTab_InactiveDataTitle}
                    </span>
                </section>
                <div className="reco-discovery-split-line"></div>
                <section className="sf-reco-basic-data">
                    <div className="reco-modified-date">
                        <WithoutModifiedDate
                            title={RMResx.RM_FA_SF_Inactive_ModifiedTitle}
                            queryParameter={queryParameter}
                            onChange={onWithoutModifedDateChange}
                        />
                    </div>
                    <SFTotalData tab={ActionTab.Summary} queryParameter={queryParameter}  />
                </section>
            </div>

            {/* Data analysis */}
            <div>
                <SFDataAnalysis
                    queryNodeDataInfo={queryNodeDataInfo}
                    queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
                    ref={dataAnalysisRef}
                />
            </div>

            {/* File analysis */}
            <div>
                <SFFileAnalysis
                    queryNodeDataInfo={queryNodeDataInfo}
                    queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
                    ref={fileAnalysisRef}
                />
            </div>
        </div>
    );
};

export default SFInactiveSummaryV3;
