import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";

import ManagedRecords from "../ManagedRecords/index";
import RecordsStatus from "../RecordsStatus/index";
import ManagedTerms from "../ManagedTerms/index";
import SourceActiveRecords from "../SourceActiveRecords/index";
import SourceUniqueSettings from "../SourceUniqueSettings/index";
import MostUsedTerms from "../MostUsedTerms/index";
import MostSiteCollections from "../MostSiteCollections/index";
import MostUserRecords from "../MostUserRecords/index";
import RecordCountByStatus from "../RecordCountByStatus/index";
import PhysicalRequest from "../PhysicalRequest/index";
import DisposalApproval from "../DisposalApproval/index";
import { SourceFlag } from "../Common/Constants";

// const IsAdminRequestOption = {
//     url: "/api/Dashboard/IsAdmin"
// };

const SourceFlagsRequestOption = {
    url: "/api/Dashboard/GetSourceFlags"
};

const AdminView = ({isAdmin}) => {

    // const [isAdmin, setIsAdmin] = useState(false);

    const [hasPermissionSourceFlags, setHasPermissionSourceFlags] = useState([]);

    useEffect(() => {
        // const isAdmin = await fetchUtility(IsAdminRequestOption);
        // setIsAdmin(isAdmin);        
        getHasPermissionSourceFlags();
    }, []);
    
    const getHasPermissionSourceFlags = async () => {
        const sourceFlags = await fetchUtility(SourceFlagsRequestOption);
        if(sourceFlags.length > 0) {
            sourceFlags[0].checked = true;
        }
        setHasPermissionSourceFlags(sourceFlags);
    };

    const hasPhysicalPermission = hasPermissionSourceFlags.some(item => item.value == SourceFlag.Physical);

    const layoutWrapper = () => {
        if (hasPhysicalPermission) {
            return "reco-dashboard-managed-cards-4wrapper";
        }
        return "reco-dashboard-managed-cards-3wrapper";
    }

    return (
        <div className="reco-dashboard-admin-view-wrapper">
            <div className="reco-dashboard-layout-wrapper">
                <section className={layoutWrapper()}>
                    <div className="reco-dashboard-managed-card">
                        <ManagedRecords />
                    </div>
                    <div className="reco-dashboard-managed-card">
                        <RecordsStatus />
                    </div>
                    <div className="reco-dashboard-managed-card">
                        <ManagedTerms />
                    </div>
                    {
                        hasPhysicalPermission &&
                        <div className="reco-dashboard-managed-card">
                            <PhysicalRequest />
                        </div>
                    }
                </section>
                <section className="reco-dashboard-source-cards">
                    <div className="reco-dashboard-source-card">
                        <SourceActiveRecords />
                    </div>
                    <div className="reco-dashboard-source-card">
                        <SourceUniqueSettings />
                    </div>
                    <div className="reco-dashboard-source-card">
                        <DisposalApproval />
                    </div>
                </section>
                <section className={isAdmin ? "reco-dashboard-graphic-cards" : ""}>
                    <div className="reco-dashboard-graphic-left">
                        {
                            isAdmin &&
                            <div className="reco-dashboard-line-chart-card">
                                <RecordCountByStatus sourceFlags={hasPermissionSourceFlags}/>
                            </div>
                        }
                        <div className="reco-dashboard-progress-cards">
                            <div className="reco-dashboard-progress-card">
                                <MostUsedTerms sourceFlags={hasPermissionSourceFlags}/>
                            </div>
                            <div className="reco-dashboard-progress-card">
                                <MostSiteCollections sourceFlags={hasPermissionSourceFlags}/>
                            </div>
                        </div>
                    </div>
                    {
                        isAdmin &&
                        <div className="reco-dashboard-user-record-card">
                            <MostUserRecords sourceFlags={hasPermissionSourceFlags}/>
                        </div>
                    }
                </section>
            </div>
        </div>
    );
};

AdminView.propTypes = {
    isAdmin: PropTypes.bool
};

export default AdminView;