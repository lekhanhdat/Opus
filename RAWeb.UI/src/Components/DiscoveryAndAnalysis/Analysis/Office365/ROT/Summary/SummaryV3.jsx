import "./index.less";
import _ from "lodash";
import TotalSummary from "../../../Components/TotalSumamry";
import WithoutModifiedDate from "../../../Components/WithoutModifiedDate";
import { useEffect, useMemo, useRef, useState } from "react";
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import FileTypeChart from "../../../Components/FileTypeChart";
import {
    DiscoveryJobStatus,
    DiscoveryNodeViewMode,
    DiscoveryQueryDataType,
    DiscoverySizeRangeQueryMode,
    DiscoveryTotalDataType,
    TotalSummaryCard,
} from "../../../Constants";
import ROTTotalData from "../Components/TotalData";
import TreeChart from "../Components/OrgTree";
import { BasicDataRequester, RotDataRequester } from "../../../requests";
import { CalculateUtil, MappingContainerName } from "../../../Utils";
import OptimizationProfileCreateOrEditPanel from "../Optimization/OptimizationProfileCreateOrEditPanel";
import ProfileRequester from "../../../requests/ProfileRequester";
import DuplicatedFileCleanupPanel from "./DuplicatedFileCleanupPanel";
import { LicenseHelper } from "../../../../../../Utilities/CommonUtil";

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_Container,
                internalName: "name",
                isLink: false,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
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
        pageSize: 5,
    },
    rotRuleQueryParameter: {
        ruleCategories: [
            {
                ruleCategory: 2,
                ruleIds: [],
                checked: true,
            },
            {
                ruleCategory: 3,
                ruleIds: [],
                checked: true,
            },
            {
                ruleCategory: 4,
                ruleIds: [],
                checked: true,
            },
        ],
    },
    fileExtensionQueryParameter: {},
    needCalculateTotalDataTypes: [
        DiscoveryTotalDataType.SizeAndCount,
        DiscoveryTotalDataType.Sites,
    ],
    nodeChangeNeedRerender: true,
};

// Only use HasDiscoveryLicenseOnly for this case
const isNewLogicAccount = LicenseHelper.HasDiscoveryLicenseOnly() || LicenseHelper.EnableRecordsArchiver();

const ROTSummaryV3 = ({ o365TenantId, jobInfo }) => {
    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    const [rotEnable, setRotEnable] = useState(defaultQueryParameter);

    const duplicatedFileCleanupPanelRef = useRef(null);

    const TOTAL_SUMMARY_CARD_ACTIONS = useMemo(() => {
        const cardActions = {
            [TotalSummaryCard.DuplicateDataSize]: {
                canShow: isNewLogicAccount,
                icon: "fia-eraser",
                iconTooltip: RMResx.RM_DA_Summary_CleanUp,
                onClick: () => duplicatedFileCleanupPanelRef.current.show()
            }
        };

        return cardActions;
    }, []);

    useEffect(() => {
        if (_.isNil(o365TenantId)) {
            return;
        }
        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.o365TenantId = o365TenantId;
        setQueryParameter(clonedQueryParameter);
    }, [o365TenantId]);

    useEffect(() => {
        const fetchRotEnable = async () => {
            const rotEnable = await BasicDataRequester.getRotEnable();
            setRotEnable(rotEnable);
        };
        fetchRotEnable();
    }, []);

    const getTableColumns = () => {
        return buildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await RotDataRequester.querySummaryNodesDataV3(
            queryParameter
        );
        let items = MappingContainerName(res.items);
        res.items = await CalculateUtil.CalculateRotSummaryNodesData(items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await RotDataRequester.querySummaryNodeTotalAggregateInfoV3(
            queryParameter
        );
        return await CalculateUtil.CalculateRotSummaryNodeTotalAggregateInfo(
            res
        );
    };

    const queryRotDataOfTree = async (queryParameter) => {
        return await RotDataRequester.queryTreeRuleInfosV3(queryParameter);
    };

    const queryAggregateInfo = async (queryParameter) => {
        return await RotDataRequester.queryAggregateInfoV3(queryParameter);
    };

    const renderNoRotCard = () => {
        return (
            <div className="reco-discovery-tree-empty">
                <span className="reco-discovery-tree-empty-icon fia-book-b">
                    <span className="path1"></span>
                    <span className="path2"></span>
                </span>
                <span className="reco-discovery-tree-empty-text" tabIndex="0">
                    {RMResx.RM_FA_ROT_NoItem}
                </span>
            </div>
        );
    };

    const onProfileCreate = async () => {
        const profileInfoes = await ProfileRequester.getRotProfileInfoes(
            o365TenantId
        );
        const selectRuleIds =
            queryParameter.rotRuleQueryParameter.ruleCategories
                .map((item) => item.ruleIds)
                .flat();
        profilePanelRef.current.onAdd(
            {
                o365TenantId: o365TenantId,
                sizeRange: -1,
                ruleIds: selectRuleIds,
                sizeRangeQueryMode:
                    DiscoverySizeRangeQueryMode.GenerateThanEqual,
                greaterThanEqualWithoutInDate:
                    queryParameter.withoutDateQueryParameter.from,
                lessThanEqualWithoutInDate:
                    queryParameter.withoutDateQueryParameter.to,
                fileExtensionIds: _.isNil(
                    queryParameter.fileExtensionQueryParameter.fileExtensions
                )
                    ? []
                    : queryParameter.fileExtensionQueryParameter.fileExtensions,
                sortBy: "FileTotalSize",
            },
            profileInfoes
        );
    };

    return (
        <>
            <div className="reco-rot-summary-container">
                <div>
                    <TotalSummary
                        queryParameter={queryParameter}
                        enableDispalyDuplicateFileTotalSize={true}
                        cardActions={TOTAL_SUMMARY_CARD_ACTIONS}
                    />
                </div>
                <div className="reco-data">
                    <section className="reco-title">
                        {RMResx.RM_FA_ROT_SummaryTab_ROTDataTitle}
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-basic-data">
                        <div className="reco-modified-date">
                            <WithoutModifiedDate
                                title={RMResx.RM_FA_Inactive_ModifiedTitle}
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                            />
                        </div>
                        <ROTTotalData
                            queryParameter={queryParameter}
                            onQuery={queryAggregateInfo}
                        />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-node-data">
                        <div className="reco-node-data-tree">
                            <span className="reco-node-data-tree-title">
                                {RMResx.RM_FA_ROT_Classification}
                            </span>
                            {(rotEnable && !_.isNil(o365TenantId)) ? (
                                <TreeChart
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    onQuery={queryRotDataOfTree}
                                />
                            ) : (
                                renderNoRotCard()
                            )}
                        </div>
                        <div>
                            <div>
                                <DiscoveryDataView
                                    title={
                                        RMResx.RM_FA_ROT_SummaryTab_SiteCollections
                                    }
                                    getColumns={getTableColumns}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryNodeDataInfo={queryNodeDataInfo}
                                    queryNodeTotalAggregateInfo={
                                        queryNodeTotalAggregateInfo
                                    }
                                    disabledViewSwitcher={true}
                                    hasSearchbox
                                />
                            </div>
                            <div>
                                <div className="reco-chart-title margin-top-m">
                                    {
                                        RMResx.RM_FA_ROT_SummaryTab_SizeByTypeTitle
                                    }
                                </div>
                                <div className="reco-treemap-chart">
                                    <FileTypeChart
                                        id={"rot_summary_file_type"}
                                        height={300}
                                        queryParameter={queryParameter}
                                        onChange={setQueryParameter}
                                        queryData={
                                            RotDataRequester.queryFileExtensionsV3
                                        }
                                    />
                                </div>
                            </div>
                        </div>
                    </section>
                </div>
            </div>
            {(jobInfo.status === DiscoveryJobStatus.Finished ||
                jobInfo.status == DiscoveryJobStatus.Failed ||
                jobInfo.status == DiscoveryJobStatus.Exception) && jobInfo.enableRot && (
                <div className="reco-discovery-summary-profile-bar">
                    <div className="reco-discovery-split-line"></div>
                    <div className="reco-discovery-summary-profile-btn">
                        <R.Button
                            id="raSaveAsProfileBtn"
                            text={RMResx.RM_DA_Summary_SaveProfile}
                            primary={true}
                            classify="theme"
                            onClick={onProfileCreate}
                        />
                    </div>
                    <OptimizationProfileCreateOrEditPanel
                        o365TenantId={o365TenantId}
                        ref={profilePanelRef}
                    />
                </div>
            )}

            {/* Enable for new logic account */}
            {isNewLogicAccount && (
                <DuplicatedFileCleanupPanel
                    ref={duplicatedFileCleanupPanelRef}
                    o365TenantId={o365TenantId}
                />
            )}
        </>
    );
};

export default ROTSummaryV3;
