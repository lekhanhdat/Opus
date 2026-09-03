import React, { useEffect, useState, useRef } from "react";
import _ from "lodash";
import SearchBox from "../../../ML/MachineLearning/Search/SearchBox";
import UnderReviewFilterPanel from '../FilterPanel/UnderReviewFilterPanel';
import Actions from "../Actions/Index";
import Table from "../../../ML/MachineLearning/Table/Index";
import RowTemplate from "../RowTemplate";
import ContinuationTokenPager from "../../../Common/ContinuationTokenPager";
import { TableColumns } from "../Config/TableColumnsConfig";
import { FilterOptions } from "../Constants/FilterOptions";

const UnderReview = ({filterAvailableOptions}) =>{

    const [ mlReviewiItems, setMlReviewiItems ] = useState([]);

    const [ selectedItems, setSelectedItems ] = useState([]);

    const [ pagerSize, setPagerSize ] = useState(10);

    const [ totalCount, setTotalCount ] = useState(0);

    const [ searchValue, setSearchValue] = useState("");

    const [ filterOptions, setFilterOptions] = useState([]);

    const [ sortInfo, setSortInfo ] =  useState({ OrderBy: "", IsDesc: false});

    const [ continuationToken, setContinuationToken ] = useState(null);

    const [ isSelectAll, setIsSelectAll] = useState(false);

    const [ isReset, setIsReset] = useState(true);

    const tokenPagerRef = useRef();

    const filterRef = useRef();

    useEffect(()=>{
        loadScope();
    },[searchValue, filterOptions, sortInfo]);


    const loadScope = async(currentContinuationToken, currentPagerSize) => {
        const requestOption = getUnderReviewQueryParam(currentPagerSize, currentContinuationToken);
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        let isNeedReset = _.isUndefined(currentContinuationToken) || pagerSize != currentPagerSize; 
        if(isNeedReset){
            tokenPagerRef.current.reset();
        }
        setIsReset(isNeedReset);
        setContinuationToken(result.continuation);
        if(requestOption.data.NeedCalculationCount){
            setTotalCount(result.count); 
        }
        setMlReviewiItems(result.items || []);
    };  

    const getUnderReviewQueryParam = (currentPagerSize, currentContinuationToken) =>{
        let requestOption = {   
            url: "/Api/MLManualApproval/UnderReviewQuery",
            data: {     
                PageSize: currentPagerSize || pagerSize,
                Continuation: currentContinuationToken || null,  
                NeedCalculationCount: _.isNil(currentContinuationToken),    
                Filters: getFilterOptions()
            }
        };
        if(sortInfo.OrderBy){
            requestOption.data = Object.assign(requestOption.data, sortInfo);
        }
        return requestOption;
    };

    const onSearch = (value) =>{
        setSearchValue(value);
    };

    const getFilterOptions = () => {
        let filterDefinition = _.cloneDeep(filterOptions);
        if(searchValue){
            const value = {
                FilterOption: FilterOptions.LeafName,
                Value: searchValue
            };
            filterDefinition.push(value);
        }
        return filterDefinition;
    };

    const onSelectItems = (items, isSelectAll) => {
        setSelectedItems(items);
        setIsSelectAll(isSelectAll);
    };

    const onPagerChange = (currentContinuationToken, pagerSize)=> {
        setPagerSize(pagerSize);
        loadScope(currentContinuationToken, pagerSize);
    };

    const onSort = (isAsc, sortColumn) => {
        setSortInfo({
            OrderBy: sortColumn, 
            IsDesc: !isAsc
        });
    };

    const renderActions = () =>{
        return <Actions
            onReload={loadScope}
            checkedItems={selectedItems}
            isCheckedAll={isSelectAll}
            queryDefintion={getFilterOptions()}
            limitItemsCount={pagerSize}
            filterOptions={filterOptions}
        />;
    };

    const renderPager = () => {
        return <ContinuationTokenPager
            ref={tokenPagerRef}
            totalCount={totalCount}
            shownCount={mlReviewiItems.length}
            showPagerSize={true}
            continuationToken={continuationToken}
            pagerSizeOptions={[5, 10, 15, 50]}
            onChange={onPagerChange}
        />;
    };

    const onFilterButtonClick = () => {
        filterRef.current.showPanel();
    };

    const onFilter = (filterOptions) => {
        setFilterOptions(filterOptions);
    };

    return <div className="ra-page-container">
        <div className="ra-main-header">
            <SearchBox 
                onSearch={onSearch}
                placeholder={RMResx.RM_MA_Search_Description}
            />
            <R.Button
                className="theme"
                text={RMResx.RM_Common_Filter}
                type="button"
                icon="fia-filter"
                tooltip={RMResx.RM_PRM_PRE_Filter}
                onClick={onFilterButtonClick}
            />
        </div>
        <div className="ra-main-table-with-border">
            <Table
                itemKey="id"
                checkable
                exsitSelectAll={true}
                isReset={isReset}
                columns={TableColumns}  
                items={mlReviewiItems}        
                template={RowTemplate}
                actionComponent={renderActions()}
                pagerComponent={renderPager()}
                onSelect={onSelectItems}
                showSelectedCounts={true}
                totalCount={totalCount}
                exsitSelectAllTip={true}
                selectAllTip={RMResx.RM_MT_MLR_Tip_SelectAll}
                onSort={onSort}
            />
        </div>
        <UnderReviewFilterPanel 
            ref={filterRef}
            filterAvailableOptions={filterAvailableOptions}
            onFilter={onFilter}
        />
    </div>;  
};

export default UnderReview;