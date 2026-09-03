import React, { useEffect, useState } from "react";
import "./index.less";

import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#41AEE8",
    "#FC7B00",
    "#24BCA4",
];

const JumpLinks = [
    "/Root/RDM/ManualApprovalReview?filter=All",
    "/Root/RDM/ManualApprovalReview?tab=1",
];

const TotalLink = "/Root/RDM/ManualApprovalReview?filter=All";

const RecordsStatus = () => {
    const [datas, setDatas] = useState([]);

    const requestOption = {
        url: "/api/Dashboard/GetManualApprovalStatus",
    };

    useEffect(() => {
        const fetchData = async () => {
            const result = await fetchUtility(requestOption);
            setDatas(result);
        };

        fetchData();
    }, []);

    const checkDataIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    return (
        <div className="reco-dashboard-records-status-wrapper reco-dashboard-card">
            <div className="reco-dashboard-records-status-top-section">
                <div 
                    className="ra-flex-1 reco-dashboard-card-title" 
                    tabIndex="0" 
                    data-tooltip="ifneed" 
                    aria-label={RMResx.RM_DSB_Status_Title}
                >
                    {RMResx.RM_DSB_Status_Title}
                </div>
                <div>
                    <a className="highlight" tabIndex="0" href={"/Root/RDM/ManualApprovalReview?tab=4"}>{RMResx.RM_DSB_History_Link}</a>
                </div>
            </div>
            <EmptyContent isEmpty={checkDataIsEmpty()}>
                <div className="reco-dashboard-records-status-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={TotalLink} />
                </div>
                <div className="reco-dashboard-records-status-keyvalues">
                    {
                        datas.map((data, index) => <ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link={JumpLinks[index]} />)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default RecordsStatus;