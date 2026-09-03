import { useEffect, useState, useRef } from "react";
import "./index.less";
import _ from "lodash";
import WithoutModifiedDate from "../../../Components/WithoutModifiedDate";
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import {
    DiscoveryNodeViewMode,
    DiscoveryQueryDataType,
    DiscoveryTotalDataType,
} from "../../../Constants";
import SizeRangAndCategory from "../../../Components/SizeRangAndCategory";
import TotalData from "../Components/TotalData";
import { BasicDataRequester, InactiveDataRequester } from "../../../requests";
import { CalculateUtil } from "../../../Utils";
import DataOptimizePanel from "../../../Components/DataOptimizePanel";
import { checkPermission } from "../../../../../../Utilities/permissionManager";
import { LicenseHelper } from "../../../../../../Utilities/CommonUtil";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_Container,
                internalName: "name",
                isLink: true,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: `${RMResx.RM_FA_TableColumn_Saving} ${RMResx.RM_FA_TableColumn_Saving_Unit_Monthly}`,
                internalName: "saving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.Site,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isAggregateField: true,
                isPlaceholder: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: `${RMResx.RM_FA_TableColumn_Saving} ${RMResx.RM_FA_TableColumn_Saving_Unit_Monthly}`,
                internalName: "saving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.SiteInContainer,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isPlaceholder: true,
                isAggregateField: true,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: `${RMResx.RM_FA_TableColumn_Saving} ${RMResx.RM_FA_TableColumn_Saving_Unit_Monthly}`,
                internalName: "saving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
]);

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Inactive,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: DiscoveryNodeViewMode.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5,
    },
    rotRuleQueryParameter: {
        ruleCategories : [
            {
                ruleCategory : 2,
                ruleIds : [],
                checked: true,
            },
            {
                ruleCategory : 3,
                ruleIds : [],
                checked: true,
            },
            {
                ruleCategory : 4,
                ruleIds : [],
                checked: true,
            },
        ]
    },
    fileExtensionQueryParameter: {},
    needCalculateTotalDataTypes: [
        DiscoveryTotalDataType.SizeAndCount,
        DiscoveryTotalDataType.Sites,
    ],
    nodeChangeNeedRerender: true,
};

const isShowButton = checkPermission("Archiver_Discovery_Optimization_RunJob", RM.UserResources);

const InactiveOptimizationHistoryVersion = ({ o365TenantId }) => {

    const optimizePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    useEffect(() => {
        if(_.isNil(o365TenantId)) {
            return;
        }
        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.o365TenantId = o365TenantId;
        setQueryParameter(clonedQueryParameter);
    }, [o365TenantId]);

    const getTableColumns = async () => {
        const ruleColumns = await BasicDataRequester.getInactiveTableColumns();
        ruleColumns.forEach((a) => {
            a.displayName += " (GB)"; 
        });
        const clonedBuildInColumns = _.cloneDeep(buildInColumns);
        if (!_.isNil(ruleColumns)) {
            clonedBuildInColumns.forEach((i) =>
                ruleColumns.forEach((j) => {
                    j.width = 200;
                    j.isAggregateField = true;
                    i.push(j);
                })
            );
        }
        return clonedBuildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await InactiveDataRequester.queryOptimizationNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateInactivesNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await InactiveDataRequester.queryOptimizationNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateInactivesNodeTotalAggregateInfo(res);
    };

    const onDataOptimizeClick = async () => {
        $$.loading(true);
        const jobStatusInfo = await fetchUtility({
            url: "/api/RMDiscoveryOffice365JobManagementApi/GetLatest",
            method: "Get",
        });
        $$.loading(false);
        optimizePanelRef.current.onShow(queryParameter, o365TenantId, jobStatusInfo);
    };

    const checkIsSelectedData = (query) => {
        if (query.containerIds.length > 0 || query.siteIds.length > 0) {
            return true;
        } else {
            return false;
        }
    };

    return (
        <div className="reco-inactive-optimization-container">
            {LicenseHelper.EnableRecordsArchiver() && isShowButton && checkIsSelectedData(queryParameter.nodeQueryParameter) && <div>
                <R.Button
                    id="raDataOptimizeBtn"
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_FA_DataOptimize_OptimizePanelBtn}
                    onClick={onDataOptimizeClick}
                />
            </div>}
            <div className="reco-data">
                <section className="reco-title">
                    <span tabIndex="0">
                        {
                            RMResx.RM_FA_Inactive_OptimizationTab_InactiveDataTitle
                        }
                    </span>
                </section>
                <section className="reco-basic-data">
                    <div className="reco-modified-date">
                        <div className="margin-bottom-l">
                            <WithoutModifiedDate
                                title={RMResx.RM_FA_Inactive_ModifiedTitle}
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                            />
                        </div>
                        <SizeRangAndCategory
                            queryParameter={queryParameter}
                            o365TenantId={o365TenantId}
                            onChange={setQueryParameter}
                        />
                    </div>
                    <TotalData
                        tab={ActionTab.Optimization}
                        queryParameter={queryParameter}
                    />
                </section>
            </div>
            <div className="reco-data">
                <section>
                    <DiscoveryDataView
                        title={
                            RMResx.RM_FA_Inactive_OptimizationTab_InactiveOptimizationTitle
                        }
                        getColumns={getTableColumns}
                        queryNodeDataInfo={queryNodeDataInfo}
                        queryParameter={queryParameter}
                        onChange={setQueryParameter}
                        queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
                        hasSearchbox
                    />
                </section>
            </div>
            <DataOptimizePanel ref={optimizePanelRef} viewMode={queryParameter.nodeQueryParameter.viewMode} />
        </div>
    );
};

export default InactiveOptimizationHistoryVersion;
