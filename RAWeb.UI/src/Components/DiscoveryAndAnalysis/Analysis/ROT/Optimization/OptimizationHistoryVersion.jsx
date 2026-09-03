import { useEffect, useState, useRef } from "react";
import WithoutModifiedDate from "../../Components/WithoutModifiedDate";
import "./index.less";
import _ from "lodash";
import DiscoveryDataView from "../../Components/DiscoveryDataView";
import { DiscoveryNodeViewMode, DiscoveryQueryDataType, DiscoveryTotalDataType } from "../../Constants";
import ROTCategories from "../Components/Category";
import { RotDataRequester } from "../../requests";
import { CalculateUtil } from "../../Utils";
import DataOptimizePanel from "../../Components/DataOptimizePanel";
import { checkPermission } from "../../../../../Utilities/permissionManager";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";

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
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Redundant,
                internalName: "redundant",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Obsolete,
                internalName: "obsolete",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "oSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Trivial,
                internalName: "trivial",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "tSaving",
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
                isPlaceholder: true,
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
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Redundant,
                internalName: "redundant",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Obsolete,
                internalName: "obsolete",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "oSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Trivial,
                internalName: "trivial",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "tSaving",
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
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Redundant,
                internalName: "redundant",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Obsolete,
                internalName: "obsolete",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "oSaving",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Trivial,
                internalName: "trivial",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "tSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
]);

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Rot,
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
        pageSize: 5
    },
    rotRuleQueryParameter:{ 
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

const ROTOptimizationHistoryVersion = ({ o365TenantId }) => {

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

    const getTableColumns = () => {
        return buildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await RotDataRequester.queryOptimizationNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateRotOptimizationNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await RotDataRequester.queryOptimizationNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateRotOptimizationNodeTotalAggregateInfo(res);
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
        <div className="reco-rot-optimization-container">
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
                    <span tabIndex="0">{RMResx.RM_FA_ROT_OptimizationTab_ROTDataTitle}</span>
                </section>
                <section className="reco-basic-data">
                    <div className="reco-rot-category">
                        <WithoutModifiedDate
                            title={RMResx.RM_FA_Inactive_ModifiedTitle}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                        />
                    </div>
                    <ROTCategories
                        queryParameter={queryParameter}
                        onChange={setQueryParameter}
                        o365TenantId = {o365TenantId}
                        isOptimizePanel={false}
                    />
                </section>
            </div>
            <div className="reco-data">
                <section className="reco-node-data">
                    <DiscoveryDataView
                        title={RMResx.RM_FA_ROT_OptimizationTab_YearlySavingTitle}
                        getColumns={getTableColumns}
                        queryParameter={queryParameter}
                        onChange={setQueryParameter}
                        queryNodeDataInfo={queryNodeDataInfo}       
                        queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}       
                        hasSearchbox
                    />
                </section>
            </div>
            <DataOptimizePanel ref={optimizePanelRef} viewMode={queryParameter.nodeQueryParameter.viewMode} />
        </div>
    );
};

export default ROTOptimizationHistoryVersion;