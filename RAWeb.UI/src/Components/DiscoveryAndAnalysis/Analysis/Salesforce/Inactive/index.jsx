import React, { useEffect, useState } from "react";

import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import SFSiteMap from "../../Components/SiteMap/Salesforce";
import SFJobManagerRequester from "../../requests/Salesforce/SFJobManagerRequester";
import InactiveSummaryV3 from "./Summary/SummaryV3";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const Inactive = () => {
    const [jobInfo, setJobInfo] = useState(null);

    const [activeTab, setActiveTab] = useState(ActionTab.Summary);

    useEffect(() => {
        const fetchJobInfo = async () => {
            const responseJobInfo = await SFJobManagerRequester.getLatest();
            setJobInfo(responseJobInfo);
        };

        fetchJobInfo();
    }, []);

    return (
        <div id="raInactive">
            <SFSiteMap URL={[SiteMapLinks.FA_Inactive]} />
            <div>
                {jobInfo === null ? (
                    <div></div>
                ) : <R.Tabcontrol
                        maxWidth={"none"}
                        destroy={true}
                        onChange={setActiveTab}
                        active={activeTab}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_FA_Inactive_SummaryTab}
                            aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                        >
                            <InactiveSummaryV3 key={"sfTenantId" + "_inactive_summary"} />
                        </R.TabPanel>
                    </R.Tabcontrol>}
            </div>
        </div>
    );
};

export default Inactive;
