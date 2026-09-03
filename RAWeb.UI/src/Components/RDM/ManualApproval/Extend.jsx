import React, { useEffect, useRef, useState } from "react";
import { FilterOptions, Source, ManualTab} from "./Constants/index";
import { useDidUpdateEffect } from "./Hooks/index";
import { ExtendTableColumns } from "./Tables/Config/index";
import Utility from "./Utility";

import _ from "lodash";
import Paginate from "./Paginate";
import ExtendFilterPanel from "./FilterPanels/ExtendFilterPanel";
import ExtendTable from "./Tables/ExtendTable";
import ExtendActions from "./Actions/ExtendActions";
import ManageColumns from "./Common/ManageColumns";

const BuildQueryRequestOptions = (continuation, pageSize, filterDefinitions, searchFilterDefinition, sortDefinition ,isSpecialReview,isClearAll) => {

    const clonedFilterDefinitions = _.cloneDeep(filterDefinitions);
    if(clonedFilterDefinitions.length == 0 && isSpecialReview && !isClearAll)
    {
        var arr = 
        {
            FilterOption: FilterOptions.Source,
            Value: JSON.stringify([Source.OneDrive])
        };
        clonedFilterDefinitions.push(arr);
        filterDefinitions.push(arr);
    }

    if (!_.isNil(searchFilterDefinition)) {
        clonedFilterDefinitions.push(searchFilterDefinition);
    }

    const queryDefintion = {
        Continuation: continuation,
        PageSize: pageSize,
        NeedCalculationCount: _.isNil(continuation),
        Filters: clonedFilterDefinitions,
        ManualApprovalTab : ManualTab.Extend,
    };

    if (!_.isNil(sortDefinition)) {
        queryDefintion.OrderBy = sortDefinition.orderBy;
        queryDefintion.IsDesc = sortDefinition.isDesc;
        queryDefintion.CustomColumnId = sortDefinition.customColumnId;
    }

    return {
        url: "/api/ManualApproval/ExtendQuery",
        data: queryDefintion
    };
};

const Extend = ({ filterAvailableOptions, customColumns }) => {

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

    const [manageColumns, setManageColumns] = useState(Utility.checkAllColumns(ExtendTableColumns, "extend"));

    const [isFiltered, setIsFiltered] = useState(false);

    const [approvalCommentQuickReasons, setApprovalCommentQuickReasons] = useState([]);
    
    let [isClearAll, setIsClearAll] = useState(false);

	const [SpecialReviewDefinitions, setSpecialReviewDefinitions] = useState(false);

    const [SpecialReviewOnlyOneLocationDefinitions, setSpecialReviewOnlyOneLocationDefinitions] = useState(false);

    const realTimeIsAdminRef = useRef(false);

    const [queryDefintionForJob, setqueryDefintionForJob] = useState({});

    useEffect(() => {
        if (customColumns.length) {
            setManageColumns((prev) => Utility.checkAllColumns([...prev, ...customColumns], "extend"));
        }
    }, [customColumns]);

    useEffect(() => {
        const fetchData = async () => {
            const isAdmin = await fetchUtility({ url: "/api/Dashboard/IsAdmin" });
            realTimeIsAdminRef.current = isAdmin;
            const EnableFolderPath = await fetchUtility({ url: "/api/ManualApproval/EnableFolderPath" });
            const OnlyOneLocation = await fetchUtility({ url: "/api/ManualApproval/EnableFolderPathOnlyOneLocation" });
            setSpecialReviewDefinitions(EnableFolderPath && !realTimeIsAdminRef.current);
            setSpecialReviewOnlyOneLocationDefinitions(OnlyOneLocation);
            setCheckedOption();
            $$.loading(true);
            const requestOption = BuildQueryRequestOptions(null, pageSize, [], null, null, EnableFolderPath && !realTimeIsAdminRef.current, false);
            setqueryDefintionForJob(requestOption.data);
            if (EnableFolderPath && !realTimeIsAdminRef.current) {
                var arr = {
                    FilterOption: FilterOptions.Source,
                    Value: JSON.stringify([Source.OneDrive])
                };
                filterDefinitions.push(arr);
                setFilterDefinitions(filterDefinitions);
            }
            const result = await fetchUtility(requestOption);
            onQueryEnd(result, 1, true);
        };

        fetchData();
    }, []);

    useDidUpdateEffect(async () => {
        $$.loading(true);
        setPageIndex(1);
        itemCacheRef.current.clear();
         if(SpecialReviewDefinitions)
        {
            const OnlyOneLocation = await  fetchUtility({  url: "/api/ManualApproval/EnableFolderPathOnlyOneLocation"});  
            setSpecialReviewOnlyOneLocationDefinitions(OnlyOneLocation);
        }
        if (!filterDefinitions.some(item => item.FilterOption === FilterOptions.Source))
        {
            isClearAll = true;
            if(  filterDefinitions.some(item => item.FilterOption === FilterOptions.FolderPath))
            {
                var arr = 
                {
                    FilterOption: FilterOptions.Source,
                    Value: JSON.stringify([Source.OneDrive])
                };
                filterDefinitions.push(arr);

            }
        }
        const requestOption = BuildQueryRequestOptions(null, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin,SpecialReviewDefinitions ,isClearAll);
        setqueryDefintionForJob(requestOption.data);
        const result = await fetchUtility(requestOption);
        setCheckedItems([]);
        onQueryEnd(result, 1, true);
        onCheckFiltered(filterDefinitions);
    }, [filterDefinitions, searchFilterDefinition, sortDefinitioin, reloadRefreshKey]);

    const onQueryEnd = (result, pageIndex, needUpdateCount = false) => {

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

    const setCheckedOption = async () =>{
        const res = await fetchUtility({url: "/api/ManualApproval/GetApprovalCommentOption"});
        setApprovalCommentQuickReasons(res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo);
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
                if (i.checked ) {  //&& i.internalApprovedStatus !== ApprovalStatus.WorkflowComplete
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
        Utility.checkAllColumns(checkedColumns, "extend");
        setManageColumns(RM.deepcopy(columns));
    };

    const onCheckFiltered = (filterCache) => {
        if (!_.isNil(filterCache) && filterCache.length > 0) {
            if (SpecialReviewDefinitions) {
                let sourcevalue = JSON.parse(filterCache[0].Value);
                if (filterCache.length === 1 && filterCache[0].FilterOption === FilterOptions.Source && sourcevalue[0] === Source.OneDrive) {
                    setIsFiltered(false);
                } else {
                    setIsFiltered(true);
                }
            } else {
                setIsFiltered(true);
            }
        } else {
            setIsFiltered(false);
        }
    };

    return (
        <div className="reco-manual-extend">
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
                <ExtendActions
                    checkedItems={checkedItems}
                    itemCount={itemCount}
                    onReload={onReload}
                    limitItemsCount={5000}
                    queryDefintion={queryDefintionForJob}
                />
            </section>
            <ExtendTable
                items={items}
                columns={manageColumns}
                onSort={onSort}
                onChangeChecked={onChangeChecked}
                onReload={onReload}
                onFilter={onFilter}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                approvalCommentQuickReasons= {approvalCommentQuickReasons}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                SpeciallEnableReviewOnlyOneLocationDefinitions ={SpecialReviewOnlyOneLocationDefinitions}
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
            <ExtendFilterPanel
                show={showPanel}
                onFilter={onFilter}
                onHide={onHide}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                approvalCommentQuickReasons= {approvalCommentQuickReasons}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                SpeciallEnableReviewOnlyOneLocationDefinitions ={SpecialReviewOnlyOneLocationDefinitions}
            />
        </div>
    );

};

export default Extend;