//columns: {container: [{displayName, internalName}], site: [{displayName, internalName}]}

import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
import DataTable from "./DataTable";
import "./index.less";
import { DiscoveryNodeViewMode } from "../../Constants";
import { SimplePager } from "../../../../Common/Pager";
import _ from "lodash";

const ViewModeI18ns = new Map([
    [DiscoveryNodeViewMode.Container, RMResx.RM_FA_ViewMode_Container],
    [DiscoveryNodeViewMode.Site, RMResx.RM_FA_ViewMode_SiteCollection],
]);

const ViewModeOptions = [
    {
        name: ViewModeI18ns.get(DiscoveryNodeViewMode.Container),
        value: DiscoveryNodeViewMode.Container,
    },
    {
        name: ViewModeI18ns.get(DiscoveryNodeViewMode.Site),
        value: DiscoveryNodeViewMode.Site,
    },
];

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_Container,
                internalName: "name",
                isLink: true,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.Site,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                isLink: false,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.SiteInContainer,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                isLink: false,
            },
        ],
    ],
]);

const DiscoveryDataView = ({
    getColumns,
    queryNodeDataInfo,
    queryNodeTotalAggregateInfo,
    title,
    queryParameter,
    onChange,
    disabledViewSwitcher,
    hasSearchbox,
}, ref) => {
    const [columns, setColumns] = useState(buildInColumns);

    const [joinedContainerName, setJoinedContainerName] = useState("");

    const [pageInfo, setPageInfo] = useState({
        pageIndex: 0,
        pageSize: 5,
        hasNext: true,
    });

    const [items, setItems] = useState([]);

    const [totalAggregateInfo, setTotalAggregateInfo] = useState({});

    const [totalNodeCount, setTotalNodeCount] = useState(0);

    const [disabledSearch, setDisabledSearch] = useState(false);

    const [searchKeyValue, setSearchKeyValue] = useState("");

    const searchBoxRef = useRef(null);

    const checkedNodeIdsRef = useRef(new Map());

    const checkedNodeInfosRef = useRef(new Map());

    const pageInfoRef = useRef({
        pageIndex: 0,
        pageSize: 5,
        hasNext: true,
    });

    useImperativeHandle(ref, () => ({
        reRenderColumns: async () => {
            const columns = await getColumns();
            checkedNodeIdsRef.current = new Map();
            checkedNodeInfosRef.current = new Map();
            pageInfoRef.current = {
                pageIndex: 0,
                pageSize: 5,
                hasNext: true
            };
            searchBoxRef.current.clear("");
            setSearchKeyValue("");
            setDisabledSearch(false);
            setColumns(columns);
        }
    }));

    useEffect(() => {
        const handler = async () => {
            const columns = await getColumns();
            setColumns(columns);
        }
        handler()
    }, []);

    useEffect(() => {
        const handler = async () => {
            const clonedQueryParameter = _.cloneDeep(queryParameter);

            const parameterCheckedNodeIds =
                clonedQueryParameter.nodeQueryParameter.checkedNodeIds;
            const checkedNodeIds = [...checkedNodeIdsRef.current.values()].flatMap(
                (item) => item
            );
            if (
                !_.isNil(parameterCheckedNodeIds) &&
                checkedNodeIds.length > 0 &&
                checkedNodeIds.length !== parameterCheckedNodeIds.length
            ) {
                return;
            }

            const aggregateInfo = await queryNodeTotalAggregateInfo(clonedQueryParameter);
            setTotalAggregateInfo(aggregateInfo);

            clonedQueryParameter.nodeQueryParameter.searchKey = searchKeyValue;
            await queryItems(
                clonedQueryParameter,
                pageInfoRef.current.pageIndex,
                pageInfoRef.current.pageSize
            );
        };
        handler();
    }, [JSON.stringify(queryParameter)]);

    const onViewModeChange = (args) => {
        const value = args.newValue.value;
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.viewMode = value;
        clonedQueryParameter.nodeQueryParameter.joinedContainerId = 0;
        clonedQueryParameter.nodeQueryParameter.containerIds = [];
        clonedQueryParameter.nodeQueryParameter.siteIds = [];
        clonedQueryParameter.nodeQueryParameter.checkedNodeIds = [];
        clonedQueryParameter.nodeQueryParameter.searchKey = "";
        searchBoxRef.current.clear("");
        checkedNodeInfosRef.current = new Map();
        checkedNodeIdsRef.current = new Map();
        pageInfoRef.current.pageIndex = 0;
        setSearchKeyValue("");
        setDisabledSearch(false);
        onChange(clonedQueryParameter);
    };

    const onJobIntoContainer = (containerInfo) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.viewMode =
            DiscoveryNodeViewMode.SiteInContainer;
        clonedQueryParameter.nodeQueryParameter.joinedContainerId =
            containerInfo[0];
        clonedQueryParameter.nodeQueryParameter.containerIds = [];
        clonedQueryParameter.nodeQueryParameter.siteIds = [];
        clonedQueryParameter.nodeQueryParameter.searchKey = "";
        pageInfoRef.current.pageIndex = 0;
        searchBoxRef.current.clear("");
        checkedNodeIdsRef.current = new Map();
        setSearchKeyValue("");
        setJoinedContainerName(containerInfo[1].value);
        setDisabledSearch(false);
        onChange(clonedQueryParameter);
    };

    const onSort = async (columnInfo) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.searchKey = searchKeyValue;
        clonedQueryParameter.nodeQueryParameter.isDesc = columnInfo.state === -1;
        pageInfoRef.current.pageIndex = 0;
        onChange(clonedQueryParameter);
        await queryItems(
            clonedQueryParameter,
            0,
            pageInfoRef.current.pageSize
        );
    }

    const onBackContainer = () => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.viewMode =
            DiscoveryNodeViewMode.Container;
        clonedQueryParameter.nodeQueryParameter.joinedContainerId = 0;
        clonedQueryParameter.nodeQueryParameter.containerIds = [];
        clonedQueryParameter.nodeQueryParameter.siteIds = [];
        clonedQueryParameter.nodeQueryParameter.searchKey = "";
        clonedQueryParameter.nodeQueryParameter.checkedNodeIds = [];
        pageInfoRef.current.pageIndex = 0;
        searchBoxRef.current.clear("");
        checkedNodeIdsRef.current = new Map();
        setSearchKeyValue("");
        setDisabledSearch(false);
        onChange(clonedQueryParameter);
    };

    const onChangeChecked = (checkedIds, checkedItems) => {
        checkedNodeIdsRef.current.set(
            pageInfoRef.current.pageIndex,
            checkedIds
        );
        const checkedNodeIds = [...checkedNodeIdsRef.current.values()].flatMap(
            (item) => item
        );

        checkedNodeInfosRef.current.set(
            pageInfoRef.current.pageIndex,
            checkedItems
        );
        const checkedNodeInfos = [...checkedNodeInfosRef.current.values()].flatMap(
            (item) => item
        );

        const clonedQueryParameter = _.cloneDeep(queryParameter);
        switch (clonedQueryParameter.nodeQueryParameter.viewMode) {
            case DiscoveryNodeViewMode.Container:
                clonedQueryParameter.nodeQueryParameter.joinedContainerId = 0;
                clonedQueryParameter.nodeQueryParameter.containerIds =
                    checkedNodeIds;
                clonedQueryParameter.nodeQueryParameter.siteIds = [];
                break;
            case DiscoveryNodeViewMode.Site:
                clonedQueryParameter.nodeQueryParameter.joinedContainerId = 0;
                clonedQueryParameter.nodeQueryParameter.containerIds = [];
                clonedQueryParameter.nodeQueryParameter.siteIds =
                    checkedNodeIds;
                break;
            case DiscoveryNodeViewMode.SiteInContainer:
                clonedQueryParameter.nodeQueryParameter.containerIds = [];
                clonedQueryParameter.nodeQueryParameter.siteIds =
                    checkedNodeIds;
                break;
            default:
                break;
        }
        clonedQueryParameter.nodeQueryParameter.checkedItems = checkedNodeInfos;
        clonedQueryParameter.nodeQueryParameter.checkedNodeIds = checkedNodeIds;
        setDisabledSearch(checkedNodeIds && checkedNodeIds.length > 0);
        onChange(clonedQueryParameter);
    };

    const onSearchKeyChange = (args) => {
        const searchKey = args ? args : "";
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.searchKey = searchKey;
        clonedQueryParameter.nodeChangeNeedRerender = true;
        pageInfoRef.current.pageIndex = 0;
        setSearchKeyValue(searchKey);
        queryItems(clonedQueryParameter, 0, 5);
    };

    const onPageChange = async (pageIndex) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.searchKey = searchKeyValue;
        pageInfoRef.current.pageIndex = pageIndex;
        await queryItems(
            clonedQueryParameter,
            pageIndex,
            pageInfoRef.current.pageSize
        );
    };

    const getViewModeOptions = (viewMode) => {
        return ViewModeOptions.map((item) => {
            item.checked = item.value === viewMode;
            return item;
        });
    };

    const queryItems = async (queryParameter, pageIndex, pageSize) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.containerIds = [];
        clonedQueryParameter.nodeQueryParameter.siteIds = [];
        clonedQueryParameter.nodeQueryParameter.checkedNodeIds = [];
        clonedQueryParameter.nodeQueryParameter.pageIndex = pageIndex;
        clonedQueryParameter.nodeQueryParameter.pageSize = pageSize;
        const dataInfo = await queryNodeDataInfo(clonedQueryParameter);
        if (pageIndex == 0) {
            setTotalNodeCount(dataInfo.count);
        }
        const checkedNodeIds = [...checkedNodeIdsRef.current.values()].flatMap(
            (item) => item
        );
        for (const item of dataInfo.items) {
            item.checked = checkedNodeIds.some((i) => i === item.id);
        }
        setItems(dataInfo.items);
        setPageInfo({
            pageIndex: pageIndex,
            pageSize: pageSize,
            hasNext: pageIndex == 0 ? dataInfo.count - (pageIndex + 1) * pageSize > 0 : totalNodeCount - (pageIndex + 1) * pageSize > 0,
        });
    };

    return (
        <div className="reco-discovery-data-table">
            <div className="reco-discovery-table-title">
                {queryParameter.nodeQueryParameter.viewMode ===
                DiscoveryNodeViewMode.SiteInContainer ? (
                    <R.Button
                        id="raBackBtn"
                        classify="blank"
                        icon="fia-arrow-line-left"
                        text={joinedContainerName}
                        onClick={onBackContainer}
                    />
                ) : (
                    title
                )}
            </div>
            <div className="reco-discovery-action-bar" hidden={!hasSearchbox}>
                {hasSearchbox && (
                    <div className="reco-discovery-searchbox">
                        <R.Searchbox
                            id="raSearchbox"
                            placeholder={RMResx.RM_FA_Searchbox_Placeholder}
                            disabled={disabledSearch}
                            onSearch={onSearchKeyChange}
                            ref={searchBoxRef}
                            width={380}
                        />
                    </div>
                )}
                <div className="reco-discovery-button-group">
                    {(disabledViewSwitcher || queryParameter.nodeQueryParameter.viewMode ===
                        DiscoveryNodeViewMode.SiteInContainer) ? (
                            <div></div>
                        ) : (
                            <div className="reco-discovery-view-mode">
                                <R.Combobox
                                    id="raViewCom"
                                    items={getViewModeOptions(
                                        queryParameter.nodeQueryParameter.viewMode
                                    )}
                                    disabled={false}
                                    textField="name"
                                    valueField="value"
                                    customTrigger={true}
                                    onChange={onViewModeChange}
                                    searchable={false}
                                >
                                    <R.Button
                                        id="raViewModeBtn"
                                        icon="fia-select-all"
                                        className="hs-manage-column-btn"
                                        text={ViewModeI18ns.get(
                                            queryParameter.nodeQueryParameter.viewMode
                                        )}
                                        tooltip={ViewModeI18ns.get(
                                            queryParameter.nodeQueryParameter.viewMode
                                        )}
                                    />
                                </R.Combobox>
                                <span className="fia-triangle-down"></span>
                            </div>
                        )}
                </div>
            </div>
            <div className="reco-discovery-scroll-table">
                <div className="reco-discovery-table">
                    <DataTable
                        columns={columns.get(
                            queryParameter.nodeQueryParameter.viewMode
                        )}
                        items={items}
                        onJoinInToContainer={onJobIntoContainer}
                        onChangeChecked={onChangeChecked}
                        onSort={onSort}
                        totalAggregateInfos={totalAggregateInfo}
                    />
                </div>
                <div className="reco-discovery-table-paging">
                    <div
                        className="reco-discovery-table-paging-total"
                        tabIndex="0"
                    >
                        {/* {RMResx.RM_FA_Table_TotalCount + totalNodeCount} */}
                    </div>
                    <SimplePager
                        pagerIndex={pageInfo.pageIndex}
                        pagerSize={pageInfo.pageSize}
                        shownCount={items.length}
                        hasNext={pageInfo.hasNext}
                        onChange={onPageChange}
                    />
                </div>
            </div>
        </div>
    );
};

export default forwardRef(DiscoveryDataView);
