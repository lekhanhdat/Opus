import { useState, useEffect } from "react";

import GoogleDriveSiteMap from "../../Components/SiteMap/GoogleDrive";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { GoogleDriveJobManagerRequester } from "../../requests/GoogleDrive";
import { GoogleDriveROTSummaryV3 } from "./Summary";
import { GoogleDriveROTOptimizationV3 } from "./Optimization";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const ROT = () => {
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
        <div id="raROT">
            <GoogleDriveSiteMap
                URL={[SiteMapLinks.FA_ROT]}
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
                            tab={RMResx.RM_FA_ROT_SummaryTab}
                            aria-label={RMResx.RM_FA_ROT_SummaryTab}
                        >
                            <GoogleDriveROTSummaryV3
                                key={selectedOrganizationId + "_inactive_summary"}
                                organizationId={selectedOrganizationId}
                                jobInfo={jobInfo}
                            />
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_FA_ROT_OptimizationTab}
                            aria-label={RMResx.RM_FA_ROT_OptimizationTab}
                        >
                            <GoogleDriveROTOptimizationV3
                                key={selectedOrganizationId + "_rot_optimization"}
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

export default ROT;
