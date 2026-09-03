import "./index.less";
import React from "react";
import PropTypes from "prop-types";
import ArchivedSize from "./ArchivedSize";
import SiteCollectionTop from "./SiteCollectionUsage";
import TeamsGroupsTop from "./TeamsGroupsUsage";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";

const SOAdminView = ({isRunSODashboardJob}) => {
    return <div className="reco-dashboard-soadmin-view-wrapper">
        <div className="reco-dashboard-so-layout-wrapper">
            <section className="reco-dashboard-tip">
                <R.Messagebar
                    message={RMResx.RM_DSB_SOTips}
                    classify="info"
                    hasClose={false}
                    status={{ show: !isRunSODashboardJob }} />
            </section>
            <section className="reco-dashboard-cards">
                <ArchivedSize />
            </section>
            <section className="reco-dashboard-cards">
                <SiteCollectionTop />
            </section>
            {LicenseHelper.HasUpgradeTeams() && (
                <section className="reco-dashboard-cards">
                    <TeamsGroupsTop />
                </section>
            )}
        </div>
    </div>;
};

SOAdminView.propTypes = {
    isRunSODashboardJob: PropTypes.bool
};

export default SOAdminView;