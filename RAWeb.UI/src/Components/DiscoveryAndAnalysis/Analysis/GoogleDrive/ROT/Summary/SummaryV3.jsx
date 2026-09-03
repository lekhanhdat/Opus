import { useEffect, useRef, useState } from "react";

import { GoogleDriveTotalSummary, GoogleDriveWithoutModifiedDate } from "../../Inactive/Components";
import { GoogleDriveTreeChart, GoogleDriveROTTotalData } from "../Components";
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import FileTypeChart from "../../../Components/FileTypeChart";
import {
    DiscoveryNodeViewMode,
    DiscoveryQueryDataType,
    DiscoverySizeRangeQueryMode,
    DiscoveryTotalDataType,
} from "../../../Constants";
import {
    GoogleDriveBasicDataRequester,
    GoogleDriveProfileRequester,
    GoogleDriveRotDataRequester,
} from "../../../requests/GoogleDrive";
import { CalculateUtil, JobUtil } from "../../../Utils";

import "./index.less";
import OptimizationProfilePanel from "../Optimization/OptimizationProfilePanel";

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_Container,
                internalName: "name",
                isLink: false,
                width: 350,
            },
            {
                displayName:
                    RMResx.RM_FA_ROT_GoogleDrive_TableColumn_ROTTotalSize,
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
                displayName:
                    RMResx.RM_FA_GoogleDrive_TableColumn_DriveCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName:
                    RMResx.RM_FA_ROT_GoogleDrive_TableColumn_ROTTotalSize,
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
                displayName:
                    RMResx.RM_FA_GoogleDrive_TableColumn_DriveCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName:
                    RMResx.RM_FA_ROT_GoogleDrive_TableColumn_ROTTotalSize,
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

function ROTSummaryV3({ organizationId, jobInfo }) {
    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    const [rotEnable, setRotEnable] = useState(false);

    useEffect(() => {
        if (_.isNil(organizationId)) {
            return;
        }
        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.organizationId = organizationId;
        setQueryParameter(clonedQueryParameter);
    }, [organizationId]);

    useEffect(() => {
        const handler = async () => {
            const rotEnable = await GoogleDriveBasicDataRequester.getRotEnable();
            setRotEnable(rotEnable);
        };
        handler();
    }, []);

    const getTableColumns = () => {
        return buildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await GoogleDriveRotDataRequester.querySummaryNodesDataV3(
            queryParameter
        );
        res.items = await CalculateUtil.CalculateRotSummaryNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res =
            await GoogleDriveRotDataRequester.querySummaryNodeTotalAggregateInfoV3(
                queryParameter
            );
        return await CalculateUtil.CalculateRotSummaryNodeTotalAggregateInfo(
            res
        );
    };

    const queryRotDataOfTree = async (queryParameter) => {
        return await GoogleDriveRotDataRequester.queryTreeRuleInfosV3(
            queryParameter
        );
    };

    const queryAggregateInfo = async (queryParameter) => {
        return await GoogleDriveRotDataRequester.queryAggregateInfoV3(
            queryParameter
        );
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
        const profileInfoList = await GoogleDriveProfileRequester.getRotProfileInfoList(organizationId);
        const selectRuleIds = queryParameter.rotRuleQueryParameter.ruleCategories
            .map((item) => item.ruleIds)
            .flat();
        profilePanelRef.current.onAdd(
            {
                organizationId: organizationId,
                sizeRange: -1,
                ruleIds: selectRuleIds,
                sizeRangeQueryMode: DiscoverySizeRangeQueryMode.GenerateThanEqual,
                greaterThanEqualWithoutInDate: queryParameter.withoutDateQueryParameter.from,
                lessThanEqualWithoutInDate: queryParameter.withoutDateQueryParameter.to,
                fileExtensionIds: _.isNil(queryParameter.fileExtensionQueryParameter.fileExtensions)
                    ? []
                    : queryParameter.fileExtensionQueryParameter.fileExtensions,
                sortBy: "FileTotalSize",
            },
            profileInfoList
        );
    };

    return (
        <>
            <div className="reco-rot-summary-container">
                <div>
                    <GoogleDriveTotalSummary queryParameter={queryParameter} />
                </div>
                <div className="reco-data">
                    <section className="reco-title">
                        {RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_ROTDataTitle}
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-basic-data">
                        <div className="reco-modified-date">
                            <GoogleDriveWithoutModifiedDate
                                title={
                                    RMResx.RM_FA_GoogleDrive_Inactive_ModifiedTitle
                                }
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                            />
                        </div>
                        <GoogleDriveROTTotalData
                            queryParameter={queryParameter}
                            onQuery={queryAggregateInfo}
                        />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-node-data">
                        <div className="reco-node-data-tree">
                            <span className="reco-node-data-tree-title">
                                {RMResx.RM_FA_GoogleDrive_ROT_Classification}
                            </span>
                            {rotEnable && !_.isNil(organizationId) ? (
                                <GoogleDriveTreeChart
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
                                        RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_DriveCollections
                                    }
                                    getColumns={getTableColumns}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryNodeDataInfo={queryNodeDataInfo}
                                    queryNodeTotalAggregateInfo={
                                        queryNodeTotalAggregateInfo
                                    }
                                    disabledViewSwitcher
                                    hasSearchbox
                                />
                            </div>
                            <div>
                                <div className="reco-chart-title margin-top-m">
                                    {
                                        RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_SizeByTypeTitle
                                    }
                                </div>
                                <div className="reco-treemap-chart">
                                    <FileTypeChart
                                        id={"rot_summary_file_type"}
                                        height={300}
                                        queryParameter={queryParameter}
                                        onChange={setQueryParameter}
                                        queryData={
                                            GoogleDriveRotDataRequester.queryFileExtensionsV3
                                        }
                                    />
                                </div>
                            </div>
                        </div>
                    </section>
                </div>
            </div>
            {!JobUtil.isRunning(jobInfo) && jobInfo.enableRot && (
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
                    <OptimizationProfilePanel
                        organizationId={organizationId}
                        ref={profilePanelRef}
                    />
                </div>
            )}
        </>
    );
}

export default ROTSummaryV3;
