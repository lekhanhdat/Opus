import React, { useEffect, useState } from "react";
import "./index.less";

import PieChartLink from "../Components/PieChartLink/index";
import EmptyContent from "../Components/EmptyContent/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";

const Colors = [
    "#41AEE8",
    "#24BCA4",
    "#FC7B00",
];

const RequestOption = {
    url: "/api/Dashboard/GetMyLoanPhysicalRequest"
};

const QueryParams = [
    "/Root/PRM/MyRequest?status=0&type=1&value=0",
    "/Root/PRM/MyRequest?status=1&type=1&value=0",
    "/Root/PRM/MyRequest?status=2&type=1&value=0",
];

const MyLoanPhysicalRequest = () => {

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
        <div className="reco-dashboard-my-lpr-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_MyLoanPhysicalRecordsRequest_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-my-lpr-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={"/Root/PRM/MyRequest?source=0"}/>
                </div>
                <div className="reco-dashboard-my-lpr-keyvalues">
                    {
                        datas.map((data, index) => <ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link={QueryParams[index]}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default MyLoanPhysicalRequest;