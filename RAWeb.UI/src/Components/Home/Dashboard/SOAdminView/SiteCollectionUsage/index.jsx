import "./index.less";
import "../../SOAdminView/index.less";
import React, { useEffect, useState } from "react";
import Template from "./Template";
import { SiteCollectionRequestOption } from "../config";
import RouterUrls from "../../../../../Constants/RouterUrls";
import { EnvironmentHelper, LicenseHelper } from "../../../../../Utilities/CommonUtil";

const isNewOpusAccount = LicenseHelper.EnableRecordsArchiver();
const is21VEnv = LicenseHelper.Is21VEnv();
const isGccEnv = EnvironmentHelper.IsGovAzureEnv;

const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Size,
        width: [150],
        resizeable: true,
    },
    {
        header: <div className="flex align-center">
            {RMResx.RM_DSB_Column_Deleted_Size}
            <$g.Popover>{RMResx.RM_DSB_Column_Deleted_Size_Note}</$g.Popover>
        </div>,
        width: [150],
        resizeable: true,
    },
];

const NewTableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [180],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_External_Archived_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_External_Archived_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_Destroyed_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_Destroyed_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_M365_Archived_Size}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_M365_Archived_Size_Note}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    }
];

const isSupportedNewTableColumn = isNewOpusAccount && !is21VEnv && !isGccEnv;
const tableColumns = isSupportedNewTableColumn ? NewTableColumns : TableColumns;

const SiteCollectionTop = () => {

    const [siteCollection, setSiteCollection] = useState([]);
    const [loadPlaceholder, setLoadPlaceholder] = useState(false);

    useEffect(() => {
        loadAllSiteCollection();
    }, []);

    const loadAllSiteCollection = async () => {
        let allSiteCollection = await fetchUtility(SiteCollectionRequestOption);
        setLoadPlaceholder(true);
        setSiteCollection(allSiteCollection);
    };

    return <div>
        <div className="reco-dashboard-cards-title-layout reco-dashboard-cards-title">
            <div tabIndex="0">{RMResx.RM_DSB_Title_SiteCollection}</div>
            <div>
                <a className="highlight" tabIndex="0" href={RouterUrls.RC_StorageOptimizationReportManagement}>{RMResx.RM_DSB_Title_ViewAll}</a>
            </div>
        </div>
        <div>
            <R.Table
                id="DSBTable"
                height={["auto","460px"]}
                columns={tableColumns}
                rowTemplate={Template}
                items={siteCollection}
                flexible={true}
            />
        </div>
        {!loadPlaceholder && <div style={{ height: "500px" }}></div>}
    </div>;
};

export default SiteCollectionTop;