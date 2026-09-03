import React from "react";
import "./index.less";

import MyCreationPhysicalRequest from "../MyCreationPhysicalRequest/index";
import MyLoanPhysicalRequest from "../MyLoanPhysicalRequest/index";
import DisposalApproval from "../DisposalApproval/index";
import MyMovementPhysicalRequest from "../MyMovementPhysicalRequest/index";
import { DashboardEndUserPermission } from "../Common/Constants";

const EndUserView = ({endUserPermission}) => {
    return (
        <div className="reco-dashboard-enduser-view-wrapper">
            <div className="reco-dashboard-layout-wrapper">
                { renderEndUserContent(endUserPermission) }
                { renderReviewUserContent(endUserPermission) }
                { renderEndUserContentForMovement(endUserPermission) }
            </div>
        </div>
    );
};

const renderEndUserContent = (endUserPermission) => {
    if((endUserPermission & DashboardEndUserPermission.EndUser) == DashboardEndUserPermission.EndUser)
    {
        return <>
            <div className="reco-dashboard-enduser-card">
                <MyCreationPhysicalRequest/>
            </div>
            <div className="reco-dashboard-enduser-card">
                <MyLoanPhysicalRequest/>
            </div>
        </>;
    }
};

const renderReviewUserContent = (endUserPermission) => {
    if((endUserPermission & DashboardEndUserPermission.ReviewEndUser) == DashboardEndUserPermission.ReviewEndUser)
    {
        return <>
            <div className="reco-dashboard-enduser-card">
                <DisposalApproval/>
            </div>
        </>;
    }
};

const renderEndUserContentForMovement = (endUserPermission) => {
    if((endUserPermission & DashboardEndUserPermission.EndUser) == DashboardEndUserPermission.EndUser)
    {
        return <>
            <div className="reco-dashboard-enduser-card">
                <MyMovementPhysicalRequest/>
            </div>
        </>;
    }
};

export default EndUserView;