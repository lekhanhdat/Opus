import "./index.less";
import _ from "lodash";
import TotalSummary from "../../Components/TotalSumamry";
import WithoutModifiedDate from "../../Components/WithoutModifiedDate";
import { useEffect, useState } from "react";
import DiscoveryDataView from "../../Components/DiscoveryDataView";
import FileTypeChart from "../../Components/FileTypeChart";
import { DiscoveryNodeViewMode, DiscoveryQueryDataType, DiscoveryTotalDataType } from "../../Constants";
import ROTTotalData from "../Components/TotalData";
import TreeChart from "../Components/OrgTree";
import { BasicDataRequester, RotDataRequester } from "../../requests";
import { CalculateUtil } from "../../Utils";

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
    ]
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

const ROTSummaryHistoryVersion = ({ o365TenantId }) => {

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    const [rotEnable, setRotEnable] = useState(defaultQueryParameter);

    useEffect(() => {
        if(_.isNil(o365TenantId)) {
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
        const res = await RotDataRequester.querySummaryNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateRotSummaryNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await RotDataRequester.querySummaryNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateRotSummaryNodeTotalAggregateInfo(res);
    };

    const queryRotDataOfTree = async (queryParameter) => {
        return await RotDataRequester.queryTreeRuleInfos(queryParameter);
    }

    const queryAggregateInfo = async (queryParameter) => {
        return await RotDataRequester.queryAggregateInfo(queryParameter);
    }

    const renderNoRotCard = () => {
        return (
            <div className="reco-discovery-tree-empty">
                <span className="reco-discovery-tree-empty-icon fia-book-b">
                    <span className="path1"></span>
                    <span className="path2"></span>
                </span>
                <span className="reco-discovery-tree-empty-text" tabIndex="0">{RMResx.RM_FA_ROT_NoItem}</span>
            </div>   
        );
    };

    return (
        <div className="reco-rot-summary-container">
            <div>
                <TotalSummary
                    queryParameter={queryParameter}
                />
            </div>
            <div className="reco-data">
                <section className="reco-title">{RMResx.RM_FA_ROT_SummaryTab_ROTDataTitle}</section>
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
                        {(rotEnable && !_.isNil(o365TenantId)) ?
                            <TreeChart
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                                onQuery={queryRotDataOfTree}
                            /> :
                            renderNoRotCard()
                        }
                    </div>
                    <div>
                        <div>
                            <DiscoveryDataView
                                title={RMResx.RM_FA_ROT_SummaryTab_SiteCollections}
                                getColumns={getTableColumns}
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                                queryNodeDataInfo={queryNodeDataInfo}
                                queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
                                hasSearchbox
                            />
                        </div>
                        <div>
                            <div className="reco-chart-title margin-top-m">
                                {RMResx.RM_FA_ROT_SummaryTab_SizeByTypeTitle}
                            </div>
                            <div className="reco-treemap-chart">
                                <FileTypeChart
                                    id={"rot_summary_file_type"}
                                    height={300}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryData={RotDataRequester.queryFileExtensions}
                                />
                            </div>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
};

export default ROTSummaryHistoryVersion;