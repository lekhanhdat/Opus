import React, { useEffect, useState } from "react";
import "./index.less";

import { IconLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const SourceActiveRecords = () => {

    const [datas, setDatas] = useState([]);

    const requestOption = {
        url: "/api/Dashboard/GetSourcesActiveCount"
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
        <div className="reco-dashboard-sar-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_Source_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-sar-keyvalues">
                    {
                        datas.map((data, index) => <IconLabelValueLink key={data.name + index} sourceFlag={data.sourceFlag} label={data.name} value={data.value} hasBgcColor={(index % 2 === 0)} link={"/Root/BCM/HybridSearch?source=" + data.sourceFlag +"&showAll=false"}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default SourceActiveRecords;