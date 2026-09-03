import { useEffect, useState } from "react";

import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import GoogleDriveSiteMap from "../../Components/SiteMap/GoogleDrive";
import { GoogleDriveJobManagerRequester } from "../../requests/GoogleDrive";
import InactiveSummaryV3 from './Summary/SummaryV3'
import InactiveOptimizationV3 from "./Optimization/OptimizationV3";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const Inactive = () => {
    const [jobInfo, setJobInfo] = useState(null);

    const [activeTab, setActiveTab] = useState(ActionTab.Summary);

    const [selectedOrganizationId, setSelectedOrganizationId] = useState();

    useEffect(() => {
        const handler = async () => {
            const responseJobInfo = await GoogleDriveJobManagerRequester.getLatest();
            setJobInfo(responseJobInfo);
        };
        handler();
    }, []);

    return (
        <div id="raInactive">
            <GoogleDriveSiteMap
                URL={[SiteMapLinks.FA_Inactive]}
                onChange={setSelectedOrganizationId}
            />
            <div>
                {jobInfo === null ? (
                    <div></div>
                ) : (
                    <R.Tabcontrol
                        maxWidth={"none"}
                        destroy={true}
                        onChange={setActiveTab}
                        active={activeTab}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_FA_Inactive_SummaryTab}
                            aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                        >
                            <InactiveSummaryV3
                                key={selectedOrganizationId + "google_drive_inactive_summary"}
                                organizationId={selectedOrganizationId}
                                jobInfo={jobInfo}
                            />
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_FA_Inactive_OptimizationTab}
                            aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
                        >
                            <InactiveOptimizationV3
                                key={selectedOrganizationId + "_inactive_optimization"}
                                organizationId={selectedOrganizationId}
                                jobInfo={jobInfo}
                            />
                        </R.TabPanel>
                    </R.Tabcontrol>
                )}
            </div>
        </div>
    );
};

export default Inactive;
