import React, { useCallback, useEffect, useState } from "react";

import Template from "./Template";
import { TeamsGroupsRequestOption } from "../config";
import RouterUrls from "../../../../../Constants/RouterUrls";

import "../../SOAdminView/index.less";
import "../SiteCollectionUsage/index.less";

const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_TeamsAndGroups,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Teams_TotalArchivedSize,
        width: [150],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_Teams_TotalSize}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_Teams_TotalSizeNote}
                </$g.Popover>
            </div>
        ),
        width: [150],
        resizeable: true,
    },
];

const TeamsGroupsTop = () => {
    const [teamsGroupsData, setTeamsGroupsData] = useState([]);
    const [loadPlaceholder, setLoadPlaceholder] = useState(false);

    useEffect(() => {
        loadAllSiteCollection();
    }, []);

    const loadAllSiteCollection = useCallback(async () => {
        const allTeamsGroups = await fetchUtility(TeamsGroupsRequestOption);
        setLoadPlaceholder(true);
        setTeamsGroupsData(allTeamsGroups || []);
    }, []);

    return (
        <div>
            <div className="reco-dashboard-cards-title-layout reco-dashboard-cards-title">
                <div tabIndex="0">{RMResx.RM_DSB_Title_TeamsGroups}</div>
                <div>
                    <a
                        className="highlight"
                        tabIndex="0"
                        href={`${RouterUrls.RC_StorageOptimizationReportManagement}?tab=1`}
                    >
                        {RMResx.RM_DSB_Title_ViewAll}
                    </a>
                </div>
            </div>
            <div>
                <R.Table
                    id="DSBTableTeamsGroups"
                    height={["auto", "460px"]}
                    columns={TableColumns}
                    rowTemplate={Template}
                    items={teamsGroupsData}
                    flexible={true}
                />
            </div>
            {!loadPlaceholder && <div style={{ height: "500px" }}></div>}
        </div>
    );
};

export default TeamsGroupsTop;
