import React, { useState } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import Connection from "./Connection";
import ConnectionGroup from "./ConnectionGroup";
import "./index.less";

const AzureFileShareConfigureConnection = () => {

    const [activeTab, setActiveTab] = useState(0);

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_AF, SiteMapLinks.BCM_AzFileConnGroup]} />
            <div className="reco-az-config-conn">
                <R.Tabcontrol
                    flex
                    onChange={(index) => setActiveTab(index)}
                    active={activeTab}
                    destroy={true}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FS_Register_Tab_ConnectionGroup}
                        aria-label={RMResx.RM_FS_Register_Tab_ConnectionGroup}
                    >
                        <ConnectionGroup />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FS_Register_Tab_Connections}
                        aria-label={RMResx.RM_FS_Register_Tab_Connections}
                    >
                        <Connection />
                    </R.TabPanel>
                </R.Tabcontrol>
            </div>
        </div>
    );
};

export default AzureFileShareConfigureConnection;