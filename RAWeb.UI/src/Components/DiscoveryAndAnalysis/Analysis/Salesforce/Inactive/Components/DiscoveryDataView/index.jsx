import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
import _ from "lodash";

import { SimplePager } from "../../../../../../Common/Pager";
import DataTable from './DataTable'
import { SFDiscoveryNodeViewMode } from "../../../../Constants";

import "./index.less";

const buildInColumns = new Map([
    [
        SFDiscoveryNodeViewMode.Data,
        [
            {
                displayName: RMResx.RM_FA_SF_TableColumn_Object,
                internalName: "displayName",
                isLink: true,
            },
        ],
    ],
    [
        SFDiscoveryNodeViewMode.File,
        [
            {
                displayName: RMResx.RM_FA_SF_TableColumn_Object,
                internalName: "displayName",
                isLink: true,
            },
        ],
    ],
    [],
]);

const SFDiscoveryDataView = ({
    id,
    getColumns,
    queryNodeDataInfo,
    queryNodeTotalAggregateInfo,
    title,
    queryParameter,
    onChange,
    showPagination = true,
}, ref) => {
    const [columns, setColumns] = useState(buildInColumns);

    const [pageInfo, setPageInfo] = useState({
        pageIndex: 0,
        pageSize: 5,
        hasNext: true,
    });

    const [items, setItems] = useState([]);

    const [totalAggregateInfo, setTotalAggregateInfo] = useState({});

    const [totalNodeCount, setTotalNodeCount] = useState(0);

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
            setColumns(columns);
        },
        refreshTableIndex: ()=>{
            checkedNodeIdsRef.current = new Map();
            checkedNodeInfosRef.current = new Map();
            pageInfoRef.current = {
                pageIndex: 0,
                pageSize: 5,
                hasNext: true
            };
            setPageInfo({
                pageIndex: 0,
                pageSize: 5,
                hasNext: true,
            });
        }
    }));

    useEffect(() => {
        const fetchColumns = async () => {
            const columns = await getColumns();
            setColumns(columns);
        };
        fetchColumns();
    }, []);

    useEffect(() => {
        const fetchData = async () => {
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

            await queryItems(
                clonedQueryParameter,
                pageInfoRef.current.pageIndex,
                pageInfoRef.current.pageSize
            );
        };
        fetchData();
    }, [queryParameter]);

    // const onSort = async (columnInfo) => {
    //     const clonedQueryParameter = _.cloneDeep(queryParameter);
    //     clonedQueryParameter.nodeQueryParameter.isDesc = columnInfo.state === -1;
    //     pageInfoRef.current.pageIndex = 0;
    //     await queryItems(
    //         clonedQueryParameter,
    //         0,
    //         pageInfoRef.current.pageSize
    //     );
    // }

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
        // const checkedNodeInfos = [...checkedNodeInfosRef.current.values()].flatMap(
        //     (item) => item
        // );

        const clonedQueryParameter = _.cloneDeep(queryParameter);
        // clonedQueryParameter.nodeQueryParameter.checkedItems = checkedNodeInfos;
        clonedQueryParameter.nodeQueryParameter.objectIds = checkedNodeIds;
        onChange(clonedQueryParameter);
    };

    const onPageChange = async (pageIndex) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
        clonedQueryParameter.nodeQueryParameter.objectIds = [];
        pageInfoRef.current.pageIndex = pageIndex;
        checkedNodeIdsRef.current = new Map(); // remove previous checkedIds when change page
        await queryItems(
            clonedQueryParameter,
            pageIndex,
            pageInfoRef.current.pageSize
        );
    };

    const queryItems = async (queryParameter, pageIndex, pageSize) => {
        const clonedQueryParameter = _.cloneDeep(queryParameter);
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
        <div className={`reco-discovery-data-table ${showPagination ? "" : "margin-bottom-m"}`}>
            <div className="reco-discovery-table-title">
                {title}
            </div>
            <div className="reco-discovery-scroll-table">
                <div>
                    <DataTable
                        id={id}
                        columns={columns.get(
                            queryParameter.nodeQueryParameter.viewMode
                        )}
                        items={items}
                        onChangeChecked={onChangeChecked}
                        // onSort={onSort}
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
                    { showPagination && ( 
                        <SimplePager
                            pagerIndex={pageInfo.pageIndex}
                            pagerSize={pageInfo.pageSize}
                            shownCount={items.length}
                            hasNext={pageInfo.hasNext}
                            onChange={onPageChange}
                        />
                    )}
                </div>
            </div>
        </div>
    );
};

export default forwardRef(SFDiscoveryDataView);
