import { useEffect, useState } from "react";

import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import FileSystemSiteMap from "../../Components/SiteMap/FileSystem";
import { FileSystemJobManagerRequester } from "../../requests/FileSystem";
import InactiveSummaryV3 from "./Summary/SummaryV3";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const Inactive = () => {
    const [jobInfo, setJobInfo] = useState(null);

    const [activeTab, setActiveTab] = useState(ActionTab.Summary);

    useEffect(() => {
        const handler = async () => {
            const responseJobInfo =
                await FileSystemJobManagerRequester.getLatest();
            setJobInfo(responseJobInfo);
        };
        handler();
    }, []);

    return (
        <div id="raInactive">
            <FileSystemSiteMap URL={[SiteMapLinks.FA_Inactive]} />
            <div>
                {jobInfo === null ? (
                    <div></div>
                ) : (
                    <InactiveSummaryV3
                        key={"file_system_inactive_summary"}
                        jobInfo={jobInfo}
                    />
                )}
            </div>
        </div>
    );
};

export default Inactive;
