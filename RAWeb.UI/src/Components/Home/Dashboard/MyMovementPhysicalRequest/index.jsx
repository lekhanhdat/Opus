import React, { useEffect, useState } from "react";
import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#41AEE8", // Waiting for approval
    "#24BCA4", // Approved
    "#FC7B00", // Rejected
];

const RequestOption = {
    url: "/api/Dashboard/GetMyMovePhysicalRequest"
};

const QueryParams = [
    "/Root/PRM/MyRequest?status=0&type=1&value=2",
    "/Root/PRM/MyRequest?status=1&type=1&value=2",
    "/Root/PRM/MyRequest?status=2&type=1&value=2",
];

const MyMovementPhysicalRequest = () => {
    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const responseData = await fetchUtility(RequestOption);
            setDatas(responseData);
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        if (!datas || !Array.isArray(datas)) {
            return true;
        }
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    return (
        <div className="reco-dashboard-my-mpr-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_MyMovementPhyiscalRecordsRequest_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div>
                    <PieChartLink colors={Colors} datas={datas} link={"/Root/PRM/MyRequest?source=7"}/>
                </div>
                <div>
                    {
                        (datas || []).map((data, index) => (
                            <ColorLabelValueLink 
                                key={data.name + index} 
                                color={Colors[index]} 
                                label={data.name} 
                                value={data.value} 
                                link={QueryParams[index]}
                            />
                        ))
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default MyMovementPhysicalRequest;