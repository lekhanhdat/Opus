import React, { useState, useEffect, useRef} from "react";
import { useLocation } from 'react-router-dom';
import PickListTable from './PickListTable';
import PickListFilterPanel from './PickListFilterPanel';
import { SelectedProportionWord, showToast } from '../../../Utilities/CommonUtil';
import { PickListForLoanStatusType, PickListForDestroyStatusType } from "../../../Constants/Constants";
import { Messagebox } from "../../Common/Messagebox";

let cachePageBrowserState = [];

const PickListRequests = ({recordListApiUrl, tableColumns, tableTemplate, statusList, Actions, exportUrl, useNumericPageIndex }) =>{

    const location = useLocation();

    const [recordList, setRecordList] = useState([]);

    const [searchValue, setSearchValue] = useState("");

    const [filterOptions, setFilterOptions] = useState(location.query ? {Status: [location.query.Status]} : {});

    const [selectedItems, setSelectedItems] = useState([]);

    const [isSelectAllResult, setIsSelectAllResult] = useState(false);

    const [pageIndex, setPageIndex] = useState(0);

    const [pageSize, setPageSize] = useState(10);

    const [browserState, setBrowserState] = useState("");

    const [shownCount, setShownCount] = useState(10);

    const [hasNext, setHasNext] = useState(false);

    const [totalCount, setTotalCount] = useState(0);

    const tableRef = useRef();

    const filterPanelRef = useRef();

    useEffect (()=>{ 
        loadRecordList();
    },[searchValue, filterOptions]);

    const getRecordListParam = (pagerParam) =>{
        return {
            "PageIndex": useNumericPageIndex ? pagerParam.PageIndex : pagerParam.BrowserState,
            "PageSize":  pagerParam.PageSize , 
            "SearchText": searchValue,
            "FilterOptions": filterOptions
        };
    };

    const loadRecordList = (currentPageInfo) =>{
        $$.loading(true);
        let currentPagerParam = getPageParam(currentPageInfo);
        let option = {
            url: recordListApiUrl,
            method: "Post",
            data: getRecordListParam(currentPagerParam)
        };
        fetchUtility(option).then((res) => { 
            $$.loading(false);  
            
            let rawList = res.List || res.Datas || [];
            
            let safeList = rawList.map((item, index) => {
                if (!item.Id) {
                    item.Id = `${item.UniqueId || 'row'}_${index}`;
                }
                return item;
            });

            setRecordList(safeList);
            
            setNewPageInfo(res, currentPagerParam);
            if(currentPageInfo && !currentPagerParam.IsChangePageSize){
                tableRef.current.setSelectedData(isSelectAllResult);
            }else{
                tableRef.current.clearSelectedData();
            }
        });
    };

    const setNewPageInfo = (result, currentPagerParam) =>{
        let {TotalCount, List} = result;
        let {PageSize, PageIndex} = currentPagerParam;
        let hasNext = TotalCount - PageSize * (PageIndex * 1 + 1) > 0;
        setPageIndex(PageIndex);
        setPageSize(PageSize);
        setHasNext(hasNext);
        setShownCount(List.length);
        setTotalCount(TotalCount);
        setNewBrowserStateInfo(result);
    };

    const setNewBrowserStateInfo = (result) =>{
        let browserState = result.PageIndex || "";
        if (!cachePageBrowserState.includes(browserState)) {
            if (browserState) {
                cachePageBrowserState.push(browserState);
            }
        }
        setBrowserState(browserState);
    };

    const getPageParam = (currentPageInfo) =>{
        let currentPageSize = currentPageInfo ? currentPageInfo.pageSize : pageSize;
        let currentPageIndex = currentPageInfo ? currentPageInfo.pageIndex : 0;
        let currentBrowserState = "";
        if (currentPageIndex != 0) {
            if (currentPageIndex < pageIndex) {//向翻页
                currentBrowserState = cachePageBrowserState[currentPageIndex - 1];
            } else {
                currentBrowserState = browserState;
            }
        } else {
            cachePageBrowserState = [];
        }
        return {
            PageSize: currentPageSize, 
            BrowserState: currentBrowserState,
            PageIndex: currentPageIndex,
            IsChangePageSize: currentPageSize != pageSize
        };
    };
    
    const onSearch = (args) =>{
        setSearchValue(args ? args : "");
    }; 

    const openFilterPanel = () =>{
        filterPanelRef.current.openPanel(filterOptions);
    };

    const onFilter = () =>{
        setIsSelectAllResult(false);
        setFilterOptions(RM.deepcopy(filterPanelRef.current.getFilterOptions()));
    };

    const onSelectItems = (args) =>{
        setSelectedItems(args);
        setIsSelectAllResult(false);
    };


    const onPageChange = (currentPageIndex, currentPageSize) =>{
        loadRecordList({ pageIndex: currentPageIndex, pageSize: currentPageSize });
    };

    const onSelectAllResult = () =>{
        let currentIsSelectAllResult = !isSelectAllResult;
        if(currentIsSelectAllResult){
            tableRef.current.setSelectedData(currentIsSelectAllResult);
        }else{
            tableRef.current.clearSelectedData();
        }
        setIsSelectAllResult(currentIsSelectAllResult);
    };

    const onExportBtn = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: onExport });
    };

    const onExport = () =>{
        $$.loading(true);
        let param = { "SearchText": searchValue, "FilterOptions": filterOptions };
        let option = {
            data: param,
            url: exportUrl,
            method: "Post",
        };
        fetchUtility(option).then((res) => { 
            $$.loading(false); 
            if(res.MessageType == 0){
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            }else{
                showToast.success(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip);
            }
        }).catch((e) => {
            $$.loading(false);
            showToast.success(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip);
        });
    };

    const getAllowShowActions = () =>{
        let showActionsStatus = [
            PickListForLoanStatusType.Pendding, PickListForDestroyStatusType.Pendding
        ];
        if(isSelectAllResult){
            if(filterOptions.Status && showActionsStatus.includes(filterOptions.Status[0])){
                return true;
            }else{
                return false;
            }
        } 
        return true;
    };

    const renderHeader = () =>{
        return <div className="ra-main-header">
            <R.Searchbox 
                id="raMtPickListSearchbox"
                placeholder={RMResx.RM_JS_RM_SearchContainerTxt}
                onSearch={onSearch}
                width={380}
            />
            <R.Button  
                id="raMtPickListFilterBtn" 
                icon="fia-filter"  
                text={RMResx.RM_Common_Filter} 
                onClick={openFilterPanel} 
            />
        </div>;
    };

    const renderNavBar = () => {
        let selectItemsCount = isSelectAllResult ? totalCount : selectedItems.length;
        let allowShowActions = getAllowShowActions();
        return < div className="ra-main-navbar">
            <div className="ra-tm-picklist-actions flex align-center gap-s">
                <R.Button 
                    primary={true}
                    classify="theme" 
                    // icon="fia-export-settings" 
                    text={RMResx.RM_MT_PickList_ExportBtn} 
                    onClick={onExportBtn}
                />   
                {allowShowActions && <Actions
                    isSelectAll = {isSelectAllResult}
                    selectedItems= {selectedItems}
                    filterOptions={filterOptions}
                    searchText={searchValue}
                    limitNumberForAction={pageSize}
                    callback={loadRecordList}
                />}
            </div>
            <div className="ra-main-selected-counter">
                {SelectedProportionWord(selectItemsCount, totalCount)}
            </div>
        </div >;
    };

    const renderTable = () => {
        return <div className="ra-main-table">
            <PickListTable
                ref={tableRef}
                id="PickListTable"
                itemKey={"Id"}
                items={recordList}
                columns={tableColumns}
                template={tableTemplate}
                checkable={true}
                onChange={onSelectItems}
            />
        </div>;
    };

    const renderSelectAllBtn = () =>{
        if(recordList.length !== 0){
            let selectAllWord = isSelectAllResult 
                ? RMResx.RM_PRM_PRE_GlobalSearch_AllResultClear
                : RMResx.RM_MT_PickList_SelectAll;
            return <React.Fragment>
                {
                    isSelectAllResult && <span className="ra-main-selected-counter">
                        {RMResx.RM_PRM_PRE_GlobalSearch_ResultSelected}
                    </span>
                }
                <a  aria-label={selectAllWord} className='ra-main-italics-link margin-left-xs' 
                    tabIndex='0' role='button' onClick={onSelectAllResult}>
                    {selectAllWord}
                </a>
                { !isSelectAllResult && <$g.Popover>{RMResx.RM_MT_PickList_SelectAllTip}</$g.Popover> }
            </React.Fragment>;
        }
    };

    const renderFooter = () =>{
        return <div className="ra-main-footer">
            <div>
                {renderSelectAllBtn()}
            </div>
            <$g.SimplePager
                pagerIndex={pageIndex}
                pagerSize={pageSize}
                shownCount={shownCount}
                hasNext={hasNext}
                showPagerSize={true}
                pagerSizeOptions={[5, 10, 15, 50]}
                onChange={onPageChange}
            />
        </div>;
    };

    const renderFilterPanel = () =>{
        return <PickListFilterPanel 
            ref={filterPanelRef} 
            statusList={statusList}
            onFilter={onFilter}
        />;
    };

    return <React.Fragment>
        <div className="ra-page-container">
            {renderHeader()}
            {renderNavBar()}
            {renderTable()}
            {renderFooter()}
            {renderFilterPanel()}
        </div>
    </React.Fragment>;
};

export default PickListRequests;