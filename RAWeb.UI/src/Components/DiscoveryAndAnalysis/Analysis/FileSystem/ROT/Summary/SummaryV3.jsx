import { useEffect, useState } from "react";

import {
    FileSystemTotalSummary,
    FileSystemWithoutModifiedDate,
} from "../../Inactive/Components";
import { FileSystemTreeChart, FileSystemROTTotalData } from "../Components";
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import FileTypeChart from "../../../Components/FileTypeChart";
import {
    DiscoveryNodeViewModeForFS,
    DiscoveryQueryDataType,
    DiscoveryTotalDataType,
} from "../../../Constants";
import {
    FileSystemBasicDataRequester,
    FileSystemRotDataRequester,
} from "../../../requests/FileSystem";
import { CalculateUtil } from "../../../Utils";

import "./index.less";

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
                displayName:
                    RMResx.RM_FA_ROT_FileSystem_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewModeForFS.Connection,
        [
            {
                displayName: RMResx.RM_FA_FileSystem_TableColumn_Collection,
                internalName: "name",
                width: 350,
            },
            {
                displayName:
                    RMResx.RM_FA_ROT_FileSystem_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewModeForFS.ConnectionInContainer,
        [
            {
                displayName:
                    RMResx.RM_FA_FileSystem_TableColumn_Collection,
                internalName: "name",
                width: 350,
            },
            {
                displayName:
                    RMResx.RM_FA_ROT_FileSystem_TableColumn_ROTTotalSize,
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
        viewMode: DiscoveryNodeViewModeForFS.Container,
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

function ROTSummaryV3({ jobInfo }) {
    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    const [rotEnable, setRotEnable] = useState(false);

    useEffect(() => {
        const handler = async () => {
            const rotEnable = await FileSystemBasicDataRequester.getRotEnable();
            setRotEnable(rotEnable);
        };
        handler();
    }, []);

    const getTableColumns = () => {
        return buildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await FileSystemRotDataRequester.querySummaryNodesDataV3(
            queryParameter
        );
        res.items = await CalculateUtil.CalculateRotSummaryNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res =
            await FileSystemRotDataRequester.querySummaryNodeTotalAggregateInfoV3(
                queryParameter
            );
        return await CalculateUtil.CalculateRotSummaryNodeTotalAggregateInfo(
            res
        );
    };

    const queryRotDataOfTree = async (queryParameter) => {
        return await FileSystemRotDataRequester.queryTreeRuleInfosV3(
            queryParameter
        );
    };

    const queryAggregateInfo = async (queryParameter) => {
        return await FileSystemRotDataRequester.queryAggregateInfoV3(
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

    return (
        <>
            <div className="reco-rot-summary-container">
                <div>
                    <FileSystemTotalSummary queryParameter={queryParameter} />
                </div>
                <div className="reco-data">
                    <section className="reco-title">
                        {RMResx.RM_FA_FileSystem_ROT_SummaryTab_ROTDataTitle}
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
                        <FileSystemROTTotalData
                            queryParameter={queryParameter}
                            onQuery={queryAggregateInfo}
                        />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-node-data">
                        <div className="reco-node-data-tree">
                            <span className="reco-node-data-tree-title">
                                {RMResx.RM_FA_FileSystem_ROT_Classification}
                            </span>
                            {rotEnable ? (
                                <FileSystemTreeChart
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
                                        RMResx.RM_FA_FileSystem_ROT_SummaryTab_Collections
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
                                        RMResx.RM_FA_FileSystem_ROT_SummaryTab_SizeByTypeTitle
                                    }
                                </div>
                                <div className="reco-treemap-chart">
                                    <FileTypeChart
                                        id={"rot_summary_file_type"}
                                        height={300}
                                        queryParameter={queryParameter}
                                        onChange={setQueryParameter}
                                        queryData={
                                            FileSystemRotDataRequester.queryFileExtensionsV3
                                        }
                                    />
                                </div>
                            </div>
                        </div>
                    </section>
                </div>
            </div>
        </>
    );
}

export default ROTSummaryV3;
