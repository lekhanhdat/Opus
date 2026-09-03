import React, { useState, useEffect } from "react";
import "./index.less";

import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#24BCA4",
    "#8361B2",
    "#E44E20"
];

const RequestOption = {
    url: "/api/Dashboard/GetPhysicalRequest"
};

const JumpLinks = [
    "/Root/PRM/MyRequest?source=1",
    "/Root/PRM/MyRequest?source=0",
    "/Root/PRM/MyRequest?source=7"
];

const TotalLink = "/Root/PRM/MyRequest?source=6";

const PhysicalRequest = () => {

    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const responseData = await fetchUtility(RequestOption);
            setDatas(responseData);
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    return (
        <div className="reco-dashboard-card reco-dashboard-pr-wrapper">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_MyPhysicalRecordsRequest_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-pr-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={TotalLink}/>
                </div>
                <div className="reco-dashboard-pr-keyvalues">
                    {
                        datas.map((data, index) => <ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link={JumpLinks[index]}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default PhysicalRequest;