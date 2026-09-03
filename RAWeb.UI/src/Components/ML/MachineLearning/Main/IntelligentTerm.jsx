import React, { useEffect, useState, useRef } from "react";
import _ from "lodash";
import SearchBox from "../Search/SearchBox";
import Filter from '../Search/Filter';
import Actions from "../Actions/IntelligentTermActions";
import Table from "../Table/Index";
import RowTemplate from "../RowTemplate/IntelligentTermTemplate";
import AddTermPanel from "../Actions/AddTerms/AddTermPanel";
import IntelligentTermFilterForm  from '../Filter/IntelligentTermFilterForm';
import { IntelligentTermTableColumns } from "../Config/TableColumnsConfig";

const IntelligentTerm = ({clickTrainingScope, refresh}) =>{

    const [ intelligentTerms, setIntelligentTerms ] = useState([]);

    const [ selectedItems, setSelectedItems ] = useState([]);

    const [ pagerIndex, setPagerIndex ] = useState(0);

    const [ currentPagerSize, setPagerSize ] = useState(10);

    const [ totalCount, setTotalCount ] = useState(0);

    const [ searchValue, setSearchValue] = useState("");

    const [ filterOptions, setFilterOptions] = useState([]);

    const [ sortInfo, setSortInfo ] = useState({ SortBy: "", IsAscending: false});

    const [ isReset, setIsReset] = useState(true);

    const addTermPanel = useRef();

    useEffect(()=>{
        loadTerms();
    },[searchValue, filterOptions, sortInfo]);

    const loadTerms = async(pagerIndex, pagerSize) => {
        const requestOption = {   
            url: "/api/RMMLTermApi/LoadTerms",
            data: {     
                PageSize: pagerSize || currentPagerSize,
                PageIndex: pagerIndex || 0,      
                SearchValue: searchValue,
                SortBy: sortInfo.SortBy,
                IsAscending: sortInfo.IsAscending,
                Filters: filterOptions
            }
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        let isNeedReset = _.isUndefined(pagerIndex) || pagerSize != currentPagerSize;
        if(isNeedReset){
            setPagerIndex(0);
        }
        setIsReset(isNeedReset);
        setTotalCount(result.TotalCount);
        setIntelligentTerms(result.MLTerms || []);
    };

    const onSearch = (value) =>{
        setSearchValue(value);
    };

    const onFilter = (filterOptions) => {
        setFilterOptions(filterOptions);
    };

    const onSelectItems = (items) => {
        setSelectedItems(items);
    };

    const doAction = (actionType) => {
        switch(actionType){
            case "OPEN_ADD_TERM_PANEL":
                addTermPanel.current.openAddTermPanel();
                break; 
            case "REFRESH_ACTION":
                loadTerms(); 
                refresh();
                break;
            default:
                loadTerms(); 
        }
    };

    const onPagerChange = (pagerIndex, pagerSize, callback) => {
        setPagerIndex(pagerIndex);
        setPagerSize(pagerSize);
        loadTerms(pagerIndex, pagerSize);
        callback(true);
    };

    const onSort = (isAsc, sortColumn) => {
        setSortInfo({
            SortBy: sortColumn, 
            IsAscending: isAsc
        });
    };

    const onRowEvent = (args) => {
        switch(args.type){
            case "SWITCH_AUTO_APPLY":
                loadTerms(pagerIndex, currentPagerSize);
                break;
            case "CLICK_TRAINING_SCOPE":
                clickTrainingScope(args.rowData.Id);
        }
    };

    const renderActions = () =>{
        return <Actions
            doAction={doAction}
            selectedItems={selectedItems}
        />;
    };

    return <div className="ra-page-container">
        <div className="ra-main-header">
            <SearchBox 
                onSearch={onSearch}
                placeholder={RMResx.RM_ML_AddTerm_Search_Placeholder}
            />
            <Filter 
                onFilter={onFilter} 
                FilterForm={IntelligentTermFilterForm} 
            />
        </div>
        <div className="ra-main-table-with-border">
            <Table
                checkable
                columns={IntelligentTermTableColumns}  
                items={intelligentTerms} 
                isReset={isReset}       
                template={RowTemplate}
                actionComponent={renderActions()}
                onSelect={onSelectItems}
                showSelectedCounts={true}
                totalCount={totalCount}
                onSort={onSort}
                onRowEvent={onRowEvent}
            />
        </div>
        <div className="ra-main-footer">
            <$g.Pager
                itemsCount={totalCount}
                showPagerSize={true}
                showPagerCounter={true}
                pagerIndex={pagerIndex}
                pagerSize={currentPagerSize}
                pagerSizeOptions={[5, 10, 15]}
                onChange={onPagerChange} />
        </div>
        <AddTermPanel doAction={doAction} ref={addTermPanel} placeholder={RMResx.RM_ML_Zero_AddTerm_Search_Placeholder} />
    </div>;  
};

export default IntelligentTerm;