import { useState, useEffect } from "react";

import FileSystemSiteMap from "../../Components/SiteMap/FileSystem";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { FileSystemJobManagerRequester } from "../../requests/FileSystem";
import { FileSystemROTSummaryV3 } from "./Summary";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const ROT = () => {
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
        <div id="raROT">
            <FileSystemSiteMap URL={[SiteMapLinks.FA_ROT]} />
            <div>
                {jobInfo === null ? (
                    <div></div>
                ) : (
                    <FileSystemROTSummaryV3
                        key={"file_system_inactive_summary"}
                        jobInfo={jobInfo}
                    />
                )}
            </div>
        </div>
    );
};

export default ROT;
