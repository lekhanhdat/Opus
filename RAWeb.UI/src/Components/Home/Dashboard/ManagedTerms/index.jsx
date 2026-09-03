import React, { useEffect, useState } from "react";
import "./index.less";

import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#24BCA4",
    "#F7A100"
];

const JumpLink = "/Root/BCM/TermManagement";

const ManagedTerms = () => {

    const [datas, setDatas] = useState([]);

    const requestOption = {
        url: "/api/Dashboard/GetTermApplyRuleUsages"
    };

    useEffect(() => {
        const fetchData = async () => {
            const responseData = await fetchUtility(requestOption);
            setDatas(responseData);
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    return (
        <div className="reco-dashbaord-managed-terms-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_Term_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-managed-terms-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={JumpLink}/>
                </div>
                <div className="reco-dashboard-managed-terms-keyvalues">
                    {
                        datas.map((data, index) => <ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link={JumpLink}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default ManagedTerms;