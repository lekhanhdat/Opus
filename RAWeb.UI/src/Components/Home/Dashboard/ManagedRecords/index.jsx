import React, { useEffect, useState } from "react";
import "./index.less";

import PieChartLink from "../Components/PieChartLink/index";
import { ColorLabelValueLink } from "../Components/LabelValueLink/index";
import { ColorLabelValue } from "../Components/LabelValue/index";
import EmptyContent from "../Components/EmptyContent/index";

const Colors = [
    "#24BCA4",
    "#8361b2",
    "#F7A100"    
];

const RequestOption = {
    url: "/api/Dashboard/GetManagedRecordsCount"
};

const QueryParams = [
    "/Root/BCM/HybridSearch?source=7&showAll=true",
    "/Root/BCM/HybridSearch?source=8&showAll=true",
];

const TotalLink = "/Root/BCM/HybridSearch";

const ManagedRecords = () => {

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
        <div className="reco-dashboard-managed-records-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_ManagedRecords_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-managed-records-piechart">
                    <PieChartLink colors={Colors} datas={datas} link={TotalLink}/>
                </div>
                <div className="reco-dashboard-managed-records-keyvalues">
                    {
                        datas.map((data, index) => 
                            QueryParams[index]
                                ?<ColorLabelValueLink key={data.name + index} color={Colors[index]} label={data.name} value={data.value} link ={QueryParams[index]}/>
                                :<ColorLabelValue key={data.name + index} color={Colors[index]} label={data.name} value={data.value}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default ManagedRecords;