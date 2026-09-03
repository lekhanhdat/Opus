import React, { useState } from "react";
import Management from "./Management";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import ArchivedSiteProfiles from "./Profile/index";

const SOReport = () => {
    const [activePageTab, setActivePageTab] = useState(0);

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.RC_StorageOptimizationReportManagement]} />
            <R.Tabcontrol
                active={activePageTab}
                onChange={(index)=>{
                    setActivePageTab(index);
                }}
            >
                <R.TabPanel tab={RMResx.RM_JS_JMD_Tab_Summary}>
                    <Management />
                </R.TabPanel>
                <R.TabPanel tab={"^^Profile"}>
                    <ArchivedSiteProfiles/>
                </R.TabPanel> 
            </R.Tabcontrol>  
        </div>
    );
};
 
export default SOReport;