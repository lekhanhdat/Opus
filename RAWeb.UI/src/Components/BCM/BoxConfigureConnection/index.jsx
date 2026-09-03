import "./index.less";
import { useEffect, useState } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import Connection from "./Connection";
import ConnectionGroup from "./ConnectionGroup";

const BoxConfigureConnection = () => {

    const [activeTab, setActiveTab] = useState(0);

    useEffect(() => {
        if (RM.Url.getParam(window.location.href, "code")) {
            setActiveTab(1);
        }
    }, [RM.Url.getParam(window.location.href, "code")])

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_Box, SiteMapLinks.BCM_BoxConnGroup]} />
            <div className="reco-box-config-conn">
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

export default BoxConfigureConnection;