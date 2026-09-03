import React, { useEffect, useState } from "react";
import "./index.less";

import { IconLabelValueLink } from "../Components/LabelValueLink";
import EmptyContent from "../Components/EmptyContent/index";

const RequestOption = {
    url: "/api/Dashboard/GetSourcesSettingCount"
};

const JumpLinks = [
    "/Root/BCM/ContentSourcesForSharePointOnline",
    "/Root/BCM/ContentSourcesForFileSystem",
    "/Root/BCM/ContentSourcesForExchangeOnline",
    "/Root/BCM/ContentSourcesForPhysicalRecords",
    "/Root/BCM/ContentSourcesForSharePointOnPremises",
    "/Root/BCM/ContentSourcesForOneDriveforBusiness",   
    "/Root/BCM/ContentSourcesForAzureFiles",
    "/Root/BCM/ContentSourcesForBox",
    "/Root/BCM/ContentSourcesForGoogle",
    "", // Salesforce
    "/Root/BCM/ContentSourcesForTeams",
];

const SourceUniqueSettings = () => {

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
        <div className="reco-dashboard-sus-wrapper reco-dashboard-card">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_UniqueSetting_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-sus-keyvalues">
                    {
                        datas.map((data, index) => <IconLabelValueLink key={data.name + index} sourceFlag={data.sourceFlag} label={data.name} value={data.value} hasBgcColor={(index % 2 === 0)} link={JumpLinks[data.sourceFlag - 1]}/>)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default SourceUniqueSettings;