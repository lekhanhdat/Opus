import React, { useEffect, useRef, useState } from "react";
import { FilterOptions, ApprovalStatus, ManualTab } from "./Constants/index";
import { useDidUpdateEffect } from "./Hooks/index";
import { RelatedRecordsTableColumns } from "./Tables/Config/index";
import Utility from "./Utility";

import _ from "lodash";
import Paginate from "./Paginate";
import RelatedRecordsFilterPanel from "./FilterPanels/RelatedRecordsFilterPanel";
import RelatedRecordsTable from "./Tables/RelatedRecordsTable";
import RelatedRecordsActions from "./Actions/RelatedRecordsActions";
import ManageColumns from "./Common/ManageColumns";

const BuildQueryRequestOptions = (continuation, pageSize, filterDefinitions, searchFilterDefinition, sortDefinition) => {

    const clonedFilterDefinitions = _.cloneDeep(filterDefinitions);
    if (!_.isNil(searchFilterDefinition)) {
        clonedFilterDefinitions.push(searchFilterDefinition);
    }

    const queryDefintion = {
        Continuation: continuation,
        PageSize: pageSize,
        NeedCalculationCount: _.isNil(continuation),
        Filters: clonedFilterDefinitions,
        ManualApprovalTab : ManualTab.RelatedRecords,
    };

    if (!_.isNil(sortDefinition)) {
        queryDefintion.OrderBy = sortDefinition.orderBy;
        queryDefintion.IsDesc = sortDefinition.isDesc;
        queryDefintion.CustomColumnId = sortDefinition.customColumnId;
    }

    return {
        url: "/api/ManualApproval/RelatedRecordsQuery",
        data: queryDefintion
    };
};

const RelatedRecords = ({ filterAvailableOptions, customColumns }) => {

    const itemCacheRef = useRef(new Map());

    const continuationRef = useRef(null);

    const [showPanel, setShowPanel] = useState(false);

    const [filterDefinitions, setFilterDefinitions] = useState([]);

    const [filterDefinitionsCache, setFilterDefinitionsCache] = useState([]);

    const [searchFilterDefinition, setSearchFilterDefinition] = useState(null);

    const [sortDefinitioin, setSortDefinition] = useState(null);

    const [items, setItems] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [itemCount, setItemCount] = useState(0);

    const [checkedItems, setCheckedItems] = useState([]);

    const [reloadRefreshKey, setReloadRefreshKey] = useState(Math.random());

    const [manageColumns, setManageColumns] = useState(Utility.checkAllColumns(RelatedRecordsTableColumns, "related-records"));

    const [isFiltered, setIsFiltered] = useState(false);

    const [queryDefintionForJob, setqueryDefintionForJob] = useState({});

    useEffect(() => {
        if (customColumns.length) {
            setManageColumns((prev) => Utility.checkAllColumns([...prev, ...customColumns], "related-records"));
        }
    }, [customColumns]);

    useEffect(() => {
        const fetchData = async () => {
            $$.loading(true);
            const requestOption = BuildQueryRequestOptions(null, pageSize, [], null, null);
            setqueryDefintionForJob(requestOption.data);
            const result = await fetchUtility(requestOption);
            onQueryEnd(result, 1, true);
        };

        fetchData();
    }, []);

    useDidUpdateEffect(async () => {
        $$.loading(true);
        setPageIndex(1);
        itemCacheRef.current.clear();
        const requestOption = BuildQueryRequestOptions(null, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        setqueryDefintionForJob(requestOption.data);
        const result = await fetchUtility(requestOption);
        setCheckedItems([]);
        onQueryEnd(result, 1, true);
        onCheckFiltered(filterDefinitions);
    }, [filterDefinitions, searchFilterDefinition, sortDefinitioin, reloadRefreshKey]);

    const onQueryEnd = (result, pageIndex, needUpdateCount = false) => {

        result.items.forEach(item => {
            item.disabled = item.internalApprovedStatus === ApprovalStatus.WorkflowComplete;
        });

        setItems(result.items);
        if (needUpdateCount) {
            setItemCount(result.count);
        }

        itemCacheRef.current.set(pageIndex, result.items);
        continuationRef.current = result.continuation;
        $$.loading(false);
    };

    const onFilter = (value) => {
        setFilterDefinitions(value);
        setFilterDefinitionsCache(value);
        setShowPanel(false);
    };

    const onHide = () => {
        setShowPanel(false);
    };

    const onFilterButtonClick = () => {
        setFilterDefinitionsCache([...filterDefinitionsCache]);
        setShowPanel(true);
    };

    const onSearch = (args) => {
        const searchValue = (args || "").trim();

        if (searchValue === "") {
            setSearchFilterDefinition(null);
            return;
        }

        const value = {
            FilterOption: FilterOptions.LeafName,
            Value: args
        };
        setSearchFilterDefinition(value);
    };

    const onSort = (args) => {
        const value = {
            orderBy: args.orderOption,
            isDesc: args.isDesc,
            customColumnId: args.customColumnId,
        };
        setSortDefinition(value);
    };

    const onReload = () => {
        setReloadRefreshKey(Math.random());
    };

    const onPageSizeChange = async (pageSize) => {
        $$.loading(true);
        itemCacheRef.current.clear();
        continuationRef.current = null;
        setCheckedItems([]);
        setPageSize(pageSize);
        setPageIndex(1);
        const requestOption = BuildQueryRequestOptions(continuationRef.current, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        const result = await fetchUtility(requestOption);
        onQueryEnd(result, 1);
    };

    const onPageIndexChange = async (pageIndex) => {
        $$.loading(true);
        setPageIndex(pageIndex);
        if (itemCacheRef.current.has(pageIndex)) {
            const items = itemCacheRef.current.get(pageIndex);
            setItems(items);
            $$.loading(false);
            return;
        }
        const requestOption = BuildQueryRequestOptions(continuationRef.current, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        const result = await fetchUtility(requestOption);
        onQueryEnd(result, pageIndex);
    };

    const onChangeChecked = () => {

        const willCheckedItems = [];

        itemCacheRef.current.forEach(value => {
            value.forEach(i => {
                if (i.checked && i.internalApprovedStatus !== ApprovalStatus.WorkflowComplete) {
                    willCheckedItems.push(i);
                    return;
                }
                i.checked = false;
            });
        });

        setCheckedItems(willCheckedItems);
    };

    const managedColumnsChanged = (columns) => {
        let checkedColumns = columns.filter(item => item.visible);
        Utility.checkAllColumns(checkedColumns, "related-records");
        setManageColumns(RM.deepcopy(columns));
    };

    const onCheckFiltered = (filterCache) => {
        if (!_.isNil(filterCache) && filterCache.length > 0) {
            setIsFiltered(true);
        } else {
            setIsFiltered(false);
        }
    };

    return (
        <div className="reco-manual-related-records">
            <section className="reco-manual-review-filter-bar">
                <R.Searchbox
                    placeholder={RMResx.RM_MA_Search_Description}
                    onSearch={onSearch}
                    width={380}
                />
                <div className="reco-manual-review-right-bar">
                    <R.Button
                        className="filtered-button"
                        icon="fia-filter"
                        primary={isFiltered}
                        classify={isFiltered ? "theme" : "default"}
                        text={isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                        tooltip={isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_PRM_PRE_Filter}
                        onClick={onFilterButtonClick}
                    />
                    <ManageColumns
                        columns={manageColumns}
                        textField="header"
                        valueField="header"
                        checkedField="visible"
                        onChange={managedColumnsChanged}
                    ></ManageColumns>
                </div>
                
            </section>
            <section className="reco-manual-review-action-bar">
                <RelatedRecordsActions
                    checkedItems={checkedItems}
                    itemCount={itemCount}
                    limitItemsCount={5000}
                    onReload={onReload}
                    queryDefintion={queryDefintionForJob}
                />
            </section>
            <RelatedRecordsTable
                items={items}
                columns={manageColumns}
                onSort={onSort}
                onChangeChecked={onChangeChecked}
                onReload={onReload}
                customColumns={customColumns}
            />
            <section className="reco-manual-review-footer">
                <div className="reco-manual-review-selected-all">
                </div>
                <Paginate
                    hasNextPage={(pageIndex * pageSize < itemCount)}
                    currentPageCount={items.length}
                    onPageIndexChange={onPageIndexChange}
                    onPageSizeChange={onPageSizeChange}
                    pageIndex={pageIndex}
                />
            </section>
            <RelatedRecordsFilterPanel
                show={showPanel}
                onFilter={onFilter}
                onHide={onHide}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
            />
        </div>
    );

};

export default RelatedRecords;