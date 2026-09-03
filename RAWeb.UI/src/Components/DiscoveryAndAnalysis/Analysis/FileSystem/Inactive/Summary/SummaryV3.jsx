import { useState } from "react";
import _ from "lodash";

import {
    DiscoveryNodeViewModeForFS,
    DiscoveryQueryDataType,
    DiscoveryTotalDataType,
} from "../../../Constants";
import {
    FileSystemBasicDataRequester,
    FileSystemInactiveDataRequester,
} from "../../../requests/FileSystem";
import {
    FileSystemTotalSummary,
    FileSystemWithoutModifiedDate,
    FileSystemTotalData,
} from "../Components";
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import SizeRangeChart from "../../../Components/SizeRangeChart";
import FileTypeChart from "../../../Components/FileTypeChart";

import "./index.less";
import { CalculateUtil } from "../../../Utils";

const buildInColumns = new Map([
    [
        DiscoveryNodeViewModeForFS.Container,
        [
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_Container,
                internalName: "name",
                isLink: false,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_ROT_SummaryTab_Collections,
                internalName: "connectionCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewModeForFS.Connection,
        [
            {
                displayName: RMResx.RM_FA_FileSystem_ROT_SummaryTab_Collections,
                internalName: "name",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewModeForFS.ConnectionInContainer,
        [
            {
                displayName: RMResx.RM_FA_FileSystem_ROT_SummaryTab_Collections,
                internalName: "name",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_FileSystem_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_Rate,
                internalName: "rate",
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
        viewMode: DiscoveryNodeViewModeForFS.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5,
    },
    rotRuleQueryParameter: {},
    fileExtensionQueryParameter: {},
    needCalculateTotalDataTypes: [
        DiscoveryTotalDataType.SizeAndCount,
        DiscoveryTotalDataType.Sites,
    ],
    nodeChangeNeedRerender: true,
};

function InactiveSummaryV3({ jobInfo }) {
    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    const getTableColumns = async () => {
        const ruleColumns =
            await FileSystemBasicDataRequester.getInactiveTableColumns();
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
        const res = await FileSystemInactiveDataRequester.querySummaryNodesData(
            queryParameter
        );
        res.items = await CalculateUtil.CalculateFileSystemInactivesNodesData(
            res.items
        );
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res =
            await FileSystemInactiveDataRequester.querySummaryNodeTotalAggregateInfo(
                queryParameter
            );
        return await CalculateUtil.CalculateFileSystemInactivesNodeTotalAggregateInfo(
            res
        );
    };

    return (
        <>
            <div className="reco-inactive-summary-container">
                {/* Summary */}
                <div>
                    <FileSystemTotalSummary queryParameter={queryParameter} />
                </div>

                {/* Inactive data */}
                <div className="reco-data">
                    <section className="reco-title">
                        <span tabIndex="0">
                            {
                                RMResx.RM_FA_FileSystem_Inactive_SummaryTab_InactiveDataTitle
                            }
                        </span>
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-basic-data">
                        <div className="reco-modified-date">
                            <FileSystemWithoutModifiedDate
                                title={
                                    RMResx.RM_FA_FileSystem_Inactive_ModifiedTitle
                                }
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                            />
                        </div>
                        <FileSystemTotalData queryParameter={queryParameter} />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-node-data">
                        <DiscoveryDataView
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
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-chart-data">
                        <div>
                            <div className="reco-chart-title">
                                {
                                    RMResx.RM_FA_FileSystem_Inactive_SummaryTab_DocumentSizeTitle
                                }
                            </div>
                            <div className="reco-column-chart">
                                <SizeRangeChart
                                    id={
                                        "file_system_inactive_summary_size_range"
                                    }
                                    height={300}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryData={
                                        FileSystemInactiveDataRequester.querySizeRanges
                                    }
                                />
                            </div>
                        </div>
                        <div>
                            <div className="reco-chart-title">
                                {
                                    RMResx.RM_FA_FileSystem_Inactive_SummaryTab_FileTypeTitle
                                }
                            </div>
                            <div className="reco-treemap-chart">
                                <FileTypeChart
                                    id={
                                        "file_system_inactive_summary_file_type"
                                    }
                                    height={300}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryData={
                                        FileSystemInactiveDataRequester.queryFileExtensions
                                    }
                                />
                            </div>
                        </div>
                    </section>
                </div>
            </div>
        </>
    );
}

export default InactiveSummaryV3;
