import React, { useEffect, useRef, useState } from "react";
import { FilterOptions , ManualTab, Source} from "./Constants/index";
import { useDidUpdateEffect } from "./Hooks/index";
import { WaitDisposalTableColumns } from "./Tables/Config/index";
import Utility from "./Utility";

import _ from "lodash";
import Paginate from "./Paginate";
import WaitDisposalFilterPanel from "./FilterPanels/WaitDisposalFilterPanel";
import WaitDisposalActions from "./Actions/WaitDisposalActions";
import WaitDisposalTable from "./Tables/WaitDisposalTable";
import ManageColumns from "./Common/ManageColumns";


const BuildQueryRequestOptions = (continuation, pageSize, filterDefinitions, searchFilterDefinition, sortDefinition,isSpecialReview,isClearAll) => {

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
        ManualApprovalTab : ManualTab.WaitDisposal,
    };

    if (!_.isNil(sortDefinition)) {
        queryDefintion.OrderBy = sortDefinition.orderBy;
        queryDefintion.IsDesc = sortDefinition.isDesc;
        queryDefintion.CustomColumnId = sortDefinition.customColumnId;
    }

    return {
        url: "/api/ManualApproval/WaitDisposalQuery",
        data: queryDefintion
    };
};

const WaitDisposal = ({ filterAvailableOptions, customColumns }) => {

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

    const [manageColumns, setManageColumns] = useState(Utility.checkAllColumns(WaitDisposalTableColumns, "wait-disposal"));

    const [isCheckedAll, setIsCheckedAll] = useState(false);
    
    let [isClearAll, setIsClearAll] = useState(false);

    const [unCheckedItems, setUnCheckedItems] = useState([]);

    const [queryDefintionForJob, setqueryDefintionForJob] = useState({});

    //是否是admin  用于判断Filter内的Folder Path
    const [SpecialReviewDefinitions, setSpecialReviewDefinitions] = useState(false);
    const [SpecialReviewOnlyOneLocationDefinitions, setSpecialReviewOnlyOneLocationDefinitions] = useState(false);
    const realTimeIsAdminRef = useRef(false); 

    const [approvalCommentQuickReasons, setApprovalCommentQuickReasons] = useState([]);

    const [needCustomButton, setNeedCustomButton] = useState(false);

    const [customButtonNames, setCustomButtonNames] = useState([]);

    const [isFiltered, setIsFiltered] = useState(false);

    useEffect(() => {
        if (customColumns.length) {
            setManageColumns((prev) => Utility.checkAllColumns([...prev, ...customColumns], "wait-disposal"));
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
            const requestOption = BuildQueryRequestOptions(null, pageSize, filterDefinitionsCache, null, null, EnableFolderPath && !realTimeIsAdminRef.current, false);
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
            onQueryEnd(result, 1, false, true);
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
        const requestOption = BuildQueryRequestOptions(null, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin, SpecialReviewDefinitions ,isClearAll);
        setqueryDefintionForJob(requestOption.data);
        const result = await fetchUtility(requestOption);
        setIsCheckedAll(false);
        setCheckedItems([]);
        onQueryEnd(result, 1, false, true);
        onCheckFiltered(filterDefinitions);
    }, [filterDefinitions, searchFilterDefinition, sortDefinitioin, reloadRefreshKey]);

    const onQueryEnd = (result, pageIndex, isSelectedAll, needUpdateCount = false) => {

        result.items.forEach(item => {
            item.checked = isSelectedAll && !Utility.getItemIds(unCheckedItems).includes(item.id);
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
        onQueryEnd(result, 1, isCheckedAll);
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
        onQueryEnd(result, pageIndex, isCheckedAll);
    };

    const onChangeChecked = () => {

        const willCheckedItems = [];
        const willUnCheckedItems = [];

        itemCacheRef.current.forEach(value => {
            value.forEach(i => {
                if (i.checked) {
                    willCheckedItems.push(i);
                    return;
                }
                i.checked = false;
                willUnCheckedItems.push(i);
            });
        });

        setCheckedItems(willCheckedItems);
        
        if(willUnCheckedItems.length === itemCount){
            setIsCheckedAll(false);
            setUnCheckedItems([]);
            setCheckedItems([]);
            return;
        }

        if(isCheckedAll){
            setUnCheckedItems(willUnCheckedItems);
        }
    };

    const managedColumnsChanged = (columns) => {
        let checkedColumns = columns.filter(item => item.visible);
        Utility.checkAllColumns(checkedColumns, "wait-disposal");
        setManageColumns(RM.deepcopy(columns));
    };

    const onCheckedSelectedAll = (isSelectedAll) => {
        itemCacheRef.current.forEach((value) => {
            value.forEach((item) => {
                item.checked = isSelectedAll;
            });
        });

        const clonedItems = [...items];
        clonedItems.forEach((item) => {
            item.checked = isSelectedAll;
        });

        setItems(clonedItems);
        setIsCheckedAll(isSelectedAll);
        if (!isSelectedAll) {
            setCheckedItems([]);
            setUnCheckedItems([]);
        }
    };

    const onKeyUpCheckedSelectedAll = (e, isSelectedAll) => {
        if (e.keyCode !== 13) {
            return;
        }
        e.stopPropagation();
        onCheckedSelectedAll(isSelectedAll);
    };

    const setCheckedOption = async () =>{
        const res = await fetchUtility({url: "/api/ManualApproval/GetApprovalCommentOption"});
        setApprovalCommentQuickReasons(res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo);
        setNeedCustomButton(res.modifyButtonName.manualApprovalModifyButton.enableModifyButtonName);
        setCustomButtonNames(res.modifyButtonName.manualApprovalModifyButton.modifiedButtonNames);
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
        <div className="reco-manual-wait-disposal">
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
                <WaitDisposalActions
                    checkedItems={checkedItems}
                    unCheckedItems={unCheckedItems}
                    itemCount={itemCount}
                    limitItemsCount={5000}
                    onReload={onReload}
                    isCheckedAll={isCheckedAll}
                    queryDefintion={queryDefintionForJob}
                    NeedCustomButton={needCustomButton}
                    CustomButtonNames={customButtonNames}
                />
            </section>
            <WaitDisposalTable
                items={items}
                onSort={onSort}
                onChangeChecked={onChangeChecked}
                onReload={onReload}
                columns={manageColumns}
                onFilter={onFilter}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                approvalCommentQuickReasons = {approvalCommentQuickReasons}
                SpeciallEnableReviewOnlyOneLocationDefinitions = {SpecialReviewOnlyOneLocationDefinitions}
                customButtonNames={customButtonNames}
                needCustomButton={needCustomButton}
                customColumns={customColumns}
            />
            <section className="reco-manual-review-footer">
                <div className="reco-manual-review-selected-all">
                    {itemCount > 0 &&
                        (!isCheckedAll ? (
                            <a
                                className="reco-manual-review-link"
                                tabIndex="0"
                                role="button"
                                onClick={(e) => onCheckedSelectedAll(true)}
                                onKeyUp={(e) =>
                                    onKeyUpCheckedSelectedAll(e, true)
                                }
                            >
                                {RMResx.RM_MA_SelectAllTasks}
                            </a>
                        ) : (
                            <>
                                {
                                    unCheckedItems.length > 0 ?
                                        <span
                                            className="reco-manual-review-link-desc"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_MA_TasksDeSelected}
                                        </span>    
                                        :
                                        <span
                                            className="reco-manual-review-link-desc"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_MA_TasksSelected}
                                        </span>                                                                                          
                                }
                                <a
                                    className="reco-manual-review-link"
                                    tabIndex="0"
                                    role="button"
                                    onClick={(e) => onCheckedSelectedAll(false)}
                                    onKeyUp={(e) =>
                                        onKeyUpCheckedSelectedAll(e, false)
                                    }
                                >
                                    {
                                        RMResx.RM_PRM_PRE_GlobalSearch_AllResultClear
                                    }
                                </a>
                            </>
                        ))}
                </div>
                <Paginate
                    hasNextPage={(pageIndex * pageSize < itemCount)}
                    currentPageCount={items.length}
                    onPageIndexChange={onPageIndexChange}
                    onPageSizeChange={onPageSizeChange}
                    pageIndex={pageIndex}
                />
            </section>
            <WaitDisposalFilterPanel
                show={showPanel}
                onFilter={onFilter}
                onHide={onHide}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                approvalCommentQuickReasons = {approvalCommentQuickReasons}
                SpeciallEnableReviewOnlyOneLocationDefinitions = {SpecialReviewOnlyOneLocationDefinitions}
            />
        </div>
    );

};

export default WaitDisposal;