import React, { useEffect, useState } from "react";
import "./index.less";
import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#41AEE8",
    "#24BCA4",
    "#FC7B00",
];

const RequestOption = {
    url: "/api/Dashboard/GetMyCreationPhysicalRequest"
};

const QueryParams = [
    "/Root/PRM/MyRequest?status=0&type=1&value=1",
    "/Root/PRM/MyRequest?status=1&type=1&value=1",
    "/Root/PRM/MyRequest?status=2&type=1&value=1",
];

const MyCreationPhysicalRequest = () => {

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
        <div className="reco-dashboard-my-cpr-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_MyCreationPhyiscalRecordsRequest_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-my-cpr-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={"/Root/PRM/MyRequest?source=1"}/>
                </div>
                <div className="reco-dashboard-my-cpr-keyvalues">
                    {
                        datas.map((data, index) => <ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link={QueryParams[index]}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default MyCreationPhysicalRequest;