import React, { useState, useEffect } from "react";
import "./index.less";

import { IconLabelValueLink } from "../Components/LabelValueLink/index";
import EmptyContent from "../Components/EmptyContent/index";

const RequestOption = {
    url: "/api/Dashboard/GetWaitingDisposalApproval"
};

// const JumpLinks = [
//     "/Root/RDM/ManualApprovalReview?filter=3&value=1",
//     "/Root/RDM/ManualApprovalReview?filter=3&value=2",
//     "/Root/RDM/ManualApprovalReview?filter=3&value=3",
//     "/Root/RDM/ManualApprovalReview?filter=3&value=4",
//     "/Root/RDM/ManualApprovalReview?filter=3&value=5",
//     "/Root/RDM/ManualApprovalReview?filter=3&value=6",
// ];

const jumpParam = [
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 1
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 2
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 3
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 4
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 5
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 6
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 7
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 8
        }
    },
    {
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 9
        }
    },
    {
        // Salesforce
        url: "",
        param: {
            filter: 3,
            value: 10
        }
    },
    {
        // Teams
        url: "/Root/RDM/ManualApprovalReview",
        param: {
            filter: 3,
            value: 11
        }
    },
];

const DisposalApproval = () => {
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
        <div className="reco-dashboard-card reco-dashboard-da-wrapper">
            <div className="reco-dashboard-card-title" tabIndex="0">
                {RMResx.RM_DSB_DisposalApproval_Title}
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-da-keyvalues">
                    {
                        datas.map((data, index) => <IconLabelValueLink 
                            key={data.name + index} 
                            sourceFlag={data.sourceFlag} 
                            label={data.name} 
                            value={data.value} 
                            hasBgcColor={(index % 2 === 0)} 
                            routeUrl={jumpParam[data.sourceFlag - 1].url}
                            routeParam={jumpParam[data.sourceFlag - 1].param}
                        />)
                    }
                </div>
            </EmptyContent>
        </div>
    );
};

export default DisposalApproval;