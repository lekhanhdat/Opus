import React, {useEffect, useRef, useState } from "react";
import { LocalStorage } from "../../../Utilities/CommonUtil";
import { UnderReviewTable } from "./Tables/index";
import { UnderReviewTableColumns } from "./Tables/Config/index";
import UnderReviewFilterPanel from "./FilterPanels/UnderReviewFilterPanel";
import { FilterOptions, ApprovalStatus, CacheKeys, Source, ManualTab, NodeType} from "./Constants/index";
import { useBuildFilterDefinitionFromUrl, useDidUpdateEffect } from "./Hooks/index";
import Utility from "./Utility";

import _ from "lodash";
import Paginate from "./Paginate";
import UnderReviewActions from "./Actions/UnderReviewActions";
import ManageColumns from "./Common/ManageColumns";
import ApprovalCommentSettingPanel from "./Panels/ApprovalCommentSettingPanel";
import { ApprovalCommentOptions } from "./Constants/ConfigOptions";
import { RoleType } from "../../../Constants/Constants";
import { StayManualReviewOption as StayManualReviewType } from "./Constants/ApprovalStatus";
import { checkPermission } from "../../../Utilities/permissionManager";
import { ApprovalSettingModule } from "./Constants/Module";

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
        ManualApprovalTab : ManualTab.UnderReview,
    };

    if (!_.isNil(sortDefinition)) {
        queryDefintion.OrderBy = sortDefinition.orderBy;
        queryDefintion.IsDesc = sortDefinition.isDesc;
        queryDefintion.CustomColumnId = sortDefinition.customColumnId;
    }

    return {
        url: "/api/ManualApproval/UnderReviewQuery",
        data: queryDefintion
    };
};

const UnderReview = ({ filterAvailableOptions, settingModel, customColumns }) => {

    const itemCacheRef = useRef(new Map());

    const continuationRef = useRef(null);

    const beforeQuickReasonRef = useRef([]);
    const beforeInactiveRejectRef = useRef([]);

    const beforeCheckedQuickReasonRef = useRef(false);
    
    const beforeCheckedCustomButtonRef = useRef(false);

    const beforeModifiedButtonNameRef = useRef([]);

    const beforeCheckedAutoApprovedProcessRef = useRef(false);

    const beforeCheckedRecheckRuleRef = useRef(true);

    const beforeEnableDeleteInvalidRecordsRef = useRef(false);

    const beforeDuration = useRef(0);
    
    const [showPanel, setShowPanel] = useState(false);

    const [filterDefinitions, setFilterDefinitions] = useState([]);

    const [filterDefinitionsCache, setFilterDefinitionsCache] = useState([]);

    const [searchFilterDefinition, setSearchFilterDefinition] = useState(null);

    const [sortDefinitioin, setSortDefinition] = useState(null);

    const [items, setItems] = useState([]);

    const [canDoActionForReclassify, setCanDoActionForReclassify] = useState(false);

    const [isFSSettingClassificationFolderLevel, setIsFSSettingClassificationFolderLevel] = useState(false);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [itemCount, setItemCount] = useState(0);

    const [checkedItems, setCheckedItems] = useState([]);

    const [isCheckedAll, setIsCheckedAll] = useState(false);

    const [reloadRefreshKey, setReloadRefreshKey] = useState(Math.random());
    
    const [queryDefintionForJob, setqueryDefintionForJob] = useState({});

    const [disabledEscalate, setDisabledEscalate] = useState(true);
    
    const [manageColumns, setManageColumns] = useState(Utility.checkAllColumns(UnderReviewTableColumns, "under-review"));

    const [approvalCommentOptions,setApprovalCommentOptions] = useState(ApprovalCommentOptions);

    const [showConfigPanel, setShowConfigPanel] = useState(false);

    const [checkedCommentOption, setCheckedCommentOption] = useState(0);

    const [needQuickReason, setNeedQuickReason] = useState(false);

    const [needCustomButton, setNeedCustomButton] = useState(false);

    const [autoApprovedProcess, setAutoApprovedProcess] = useState(false);

    const [isCheckingRuleBeforeDispose, setIsCheckingRuleBeforeDispose] = useState(true);

    const [enableDeleteInvalidRecords, setEnableDeleteInvalidRecords] = useState(false);

    const [approvalCommentQuickReasons, setApprovalCommentQuickReasons] = useState([]);

    const [inactiveRejects, setInactiveRejects] = useState([]);

    const [customButtonNames, setCustomButtonNames] = useState([]);

    const [stayManualReview, setStayManualReview] = useState(StayManualReviewType.Stay);

    const [unCheckedItems, setUnCheckedItems] = useState([]);

    const [duration, setDuration] = useState(0);

    //是否是admin  用于判断Filter内的Folder Path
    const [SpecialReviewDefinitions, setSpecialReviewDefinitions] = useState(false);
    const [SpecialReviewOnlyOneLocationDefinitions, setSpecialReviewOnlyOneLocationDefinitions] = useState(false);
    const realTimeIsAdminRef = useRef(false);
    const [isFiltered, setIsFiltered] = useState(false);

    // [RECO-37027] [Hotfix] JPMC
    const [isHideReclassifyBtnByApiSetting, setIsHideReclassifyBtnByApiSetting] = useState(false);
    
    useEffect(() => {
        setCachedCheckedManageColumns();   
    }, [customColumns]);

    useEffect(() => {
        getClassificationLevel();
    }, []);

    useBuildFilterDefinitionFromUrl(async(filterDefinition, isFilterAll) => {

        const isAdmin =  await fetchUtility({ url: "/api/Dashboard/IsAdmin"});
        realTimeIsAdminRef.current = isAdmin;
        const EnableFolderPath = await  fetchUtility({  url: "/api/ManualApproval/EnableFolderPath"});   
        const OnlyOneLocation = await  fetchUtility({  url: "/api/ManualApproval/EnableFolderPathOnlyOneLocation"});  
        setSpecialReviewDefinitions( EnableFolderPath && !realTimeIsAdminRef.current );  
        setSpecialReviewOnlyOneLocationDefinitions(OnlyOneLocation);

        let cachedFilterDefinitions = LocalStorage.get(CacheKeys.URFilterData);
        let hasCache = !_.isNil(cachedFilterDefinitions) && cachedFilterDefinitions.length > 0;
        let isSelectAll =  LocalStorage.get(CacheKeys.URIsSelectedAll);
        if (isSelectAll === undefined) {
            isSelectAll = false;
        }
        if(!hasCache && EnableFolderPath && !realTimeIsAdminRef.current && !isSelectAll )
        {
            var arr = [
                {
                    FilterOption: FilterOptions.Source,
                    Value: JSON.stringify([Source.OneDrive])
                }
            ];
            LocalStorage.set(CacheKeys.URFilterData, arr);
            cachedFilterDefinitions = LocalStorage.get(CacheKeys.URFilterData);
            hasCache =  true;
        }
        if( EnableFolderPath && !realTimeIsAdminRef.current && OnlyOneLocation)
        {
            _.remove(cachedFilterDefinitions, item => item.FilterOption === FilterOptions.Workspace );
            _.remove(filterDefinition, item => item.FilterOption === FilterOptions.Workspace );
            LocalStorage.set(CacheKeys.URFilterData, cachedFilterDefinitions);
            setFilterDefinitions( cachedFilterDefinitions );
            setFilterDefinitionsCache(cachedFilterDefinitions);
        }
        checkDisabledEscalate();
        setCheckedOption();
        if((filterDefinition.length === 0 && !isFilterAll) || sessionStorage.getItem(CacheKeys.URIsFiltered)) {
            setFilterDefinitions(hasCache ? cachedFilterDefinitions : filterDefinition);
            setFilterDefinitionsCache(hasCache ? cachedFilterDefinitions : filterDefinition);
            return;
        }
        setFilterDefinitions(filterDefinition);
        setFilterDefinitionsCache(filterDefinition);
    });

    const setCachedCheckedManageColumns = () => {
        const cachedCheckedManageColIds = LocalStorage.get(CacheKeys.URCheckedManageColIds);
        if (cachedCheckedManageColIds) {
            setManageColumns(Utility.modifyItemsByIds(cachedCheckedManageColIds, _.cloneDeep([...manageColumns, ...customColumns]), "visible"));
        } else {
            setManageColumns(Utility.checkAllColumns([...manageColumns, ...customColumns], "under-review"));
        }
    };

    const setCheckedOption = async () =>{
        const res = await fetchUtility({url: "/api/ManualApproval/GetApprovalCommentOption"});
        const quickReasonInfo = res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo;
        const inactiveRejectInfo = res.commentSetting.manualApprovalQuickReasonInfo.incativeRejectBool || [];

        setApprovalCommentOptions(Utility.setCheckedOption(ApprovalCommentOptions,res.option));
        setNeedQuickReason(res.commentSetting.manualApprovalQuickReasonInfo.needQuickReason);
        setApprovalCommentQuickReasons(quickReasonInfo);
        setInactiveRejects(inactiveRejectInfo);
        setCheckedCommentOption(res.option);
        setNeedCustomButton(res.modifyButtonName.manualApprovalModifyButton.enableModifyButtonName);
        setCustomButtonNames(res.modifyButtonName.manualApprovalModifyButton.modifiedButtonNames);
        setAutoApprovedProcess(res.enableAutoApprovedProcess);
        setIsCheckingRuleBeforeDispose(res.isRecheckRule);
        setEnableDeleteInvalidRecords(!!res.enableDeleteInvalidRecords);
        setDuration(res.duration);
        setStayManualReview(res.stayManualReviewOption);
        beforeQuickReasonRef.current = res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo;
        beforeInactiveRejectRef.current = inactiveRejectInfo;
        beforeCheckedQuickReasonRef.current = res.commentSetting.manualApprovalQuickReasonInfo.needQuickReason;
        beforeCheckedCustomButtonRef.current = res.modifyButtonName.manualApprovalModifyButton.enableModifyButtonName;
        beforeModifiedButtonNameRef.current = res.modifyButtonName.manualApprovalModifyButton.modifiedButtonNames;
        beforeCheckedAutoApprovedProcessRef.current = res.enableAutoApprovedProcess;
        beforeCheckedRecheckRuleRef.current = res.isRecheckRule;
        beforeEnableDeleteInvalidRecordsRef.current = !!res.enableDeleteInvalidRecords;
        beforeDuration.current = res.duration;

        // Check if inactiveRejects state is empty array
        if (quickReasonInfo && quickReasonInfo.length && (!inactiveRejectInfo || !inactiveRejectInfo.length)) {
            const fillBoolArray = Array(quickReasonInfo.length).fill(false);
            beforeInactiveRejectRef.current = fillBoolArray;
            setInactiveRejects(fillBoolArray);
        }
    };

    const checkDisabledEscalate = async () => {
        const res = await fetchUtility({url: "/api/ManualApproval/DisabledEscalate"});
        setDisabledEscalate(res);
    };

    useDidUpdateEffect(async () => {
        $$.loading(true);
        try{
            setPageIndex(1);
            itemCacheRef.current.clear();
            if(SpecialReviewDefinitions)
            {
                const OnlyOneLocation = await  fetchUtility({  url: "/api/ManualApproval/EnableFolderPathOnlyOneLocation"});    //及时更新的问题
                if(OnlyOneLocation && !SpecialReviewOnlyOneLocationDefinitions)
                {
                    _.remove(filterDefinitions, item => item.FilterOption === FilterOptions.Workspace);
                    let cachedFilterDefinitions = LocalStorage.get(CacheKeys.URFilterData);
                    _.remove(cachedFilterDefinitions, item => item.FilterOption === FilterOptions.Workspace );
                    LocalStorage.set(CacheKeys.URFilterData, cachedFilterDefinitions);
                    setFilterDefinitions( cachedFilterDefinitions );
                    setFilterDefinitionsCache(cachedFilterDefinitions);
                }
                setSpecialReviewOnlyOneLocationDefinitions(OnlyOneLocation);
                if(!filterDefinitions.some(item => item.FilterOption === FilterOptions.Source))
                {
                 
                    if(filterDefinitions.some(item => item.FilterOption === FilterOptions.FolderPath))
                    {
                      
                        var arr = 
                        {
                            FilterOption: FilterOptions.Source,
                            Value: JSON.stringify([Source.OneDrive])
                        };
                        filterDefinitions.push(arr);
                    }
                }
            }
            
            const requestOption = BuildQueryRequestOptions(null, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
            setqueryDefintionForJob(requestOption.data);
            const result = await fetchUtility(requestOption);
            setIsCheckedAll(false);
            setCheckedItems([]);
            onQueryEnd(result, 1, false, true);
            onCheckFiltered(filterDefinitions);
        }
        catch {
            $$.loading(false);
        }
    }, [filterDefinitions, searchFilterDefinition, sortDefinitioin, reloadRefreshKey ]);

    useEffect(() => {
        const fetchIsHideReclassifyBtnSetting = async () => {
            $$.loading(true);
            const option = {
                url: "/api/ManualApproval/IsHideReclassifyBtnInManualApproval",
                method: "GET",
            };
            try {
                const isHide = await fetchUtility(option);
                setIsHideReclassifyBtnByApiSetting(isHide);
            } catch (error) {
                console.error("Failed to get IsHideReclassifyBtnInManualApproval setting", error);
                setIsHideReclassifyBtnByApiSetting(false);
            } finally {
                $$.loading(false);
            }
        };
        fetchIsHideReclassifyBtnSetting();
    }, []);

    const getClassificationLevel = () => {
        $$.loading(true);
        fetchUtility({
            url: "/API/FSSettingApi/GetClassificationLevel",
            method: "POST"
        })
            .then((res) => {
                if (res) {
                    setIsFSSettingClassificationFolderLevel(res === NodeType.FSFolder);
                }
            })
            .finally(() => $$.loading(false));
    }
    
    const onQueryEnd = (result, pageIndex, isSelectedAll, needUpdateCount = false) => {

        result.items.forEach(item => {
            item.checked = isSelectedAll && !Utility.getItemIds(unCheckedItems).includes(item.id);
            item.disabled = item.internalApprovedStatus === ApprovalStatus.WorkflowComplete;
        });

        setItems(result.items);
        setCanDoActionForReclassify(result.canDoGlobalAction || false);
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
        var hasSource =_.isNil(LocalStorage.get(CacheKeys.URFilterData));
        if(!value.length)
        {
            LocalStorage.set(CacheKeys.URIsSelectedAll, true);
        }
        if(hasSource&&  SpecialReviewDefinitions )
        {
            var arr = [
                {
                    FilterOption: FilterOptions.Source,
                    Value: JSON.stringify([Source.OneDrive])
                }
            ];
            LocalStorage.set(CacheKeys.URFilterData, arr);
        }
        else
        {
            LocalStorage.set(CacheKeys.URFilterData, value);
        }
        sessionStorage.setItem(CacheKeys.URIsFiltered, true);
        setShowPanel(false);
    };

    const onSaveConfigration = async (option) => {
        if(!needQuickReason){
            const clonedTermInfo = _.cloneDeep(beforeQuickReasonRef.current);
            const clonedInactiveRejectInfo = _.cloneDeep(beforeInactiveRejectRef.current);
            setApprovalCommentQuickReasons(clonedTermInfo);
            setInactiveRejects(clonedInactiveRejectInfo);
        }
        if(!needCustomButton){
            const clonedCustomButtonName = _.cloneDeep(beforeModifiedButtonNameRef.current);
            setCustomButtonNames(clonedCustomButtonName);
        }
        const result = await fetchUtility(option);
        if(result){
            setCheckedOption();
            setShowConfigPanel(false);
        }
        return result;
    };

    const onConfigButtonClick = () => {
        setShowConfigPanel(true);
    };

    const onHide = () => {
        setShowPanel(false);
        setShowConfigPanel(false);
        const clonedQuickReasonInfo = _.cloneDeep(beforeQuickReasonRef.current);
        const clonedInactiveRejectInfo = _.cloneDeep(beforeInactiveRejectRef.current);
        const clonedCheckedQuickReason = _.cloneDeep(beforeCheckedQuickReasonRef.current);
        const clonedCheckedCustomButtonName = _.cloneDeep(beforeCheckedCustomButtonRef.current);
        const clonedCustomButtonName = _.cloneDeep(beforeModifiedButtonNameRef.current);
        const clonedCheckedAutoApprovedProcess = _.cloneDeep(beforeCheckedAutoApprovedProcessRef.current);
        const clonedDuration = _.cloneDeep(beforeDuration.current);
        setApprovalCommentQuickReasons(clonedQuickReasonInfo);
        setInactiveRejects(clonedInactiveRejectInfo);
        setNeedQuickReason(clonedCheckedQuickReason);
        setNeedCustomButton(clonedCheckedCustomButtonName);
        setAutoApprovedProcess(clonedCheckedAutoApprovedProcess);
        setCustomButtonNames(clonedCustomButtonName);
        setDuration(clonedDuration);
        setIsCheckingRuleBeforeDispose(beforeCheckedRecheckRuleRef.current);
        setEnableDeleteInvalidRecords(beforeEnableDeleteInvalidRecordsRef.current);
    };

    const onChangeCommentTermInfo = (value) => {
        let clonedTermInfo = _.cloneDeep(approvalCommentQuickReasons);
        clonedTermInfo = value;
        setApprovalCommentQuickReasons(clonedTermInfo);
    };

    const onChangeDisableTermInfo = (value) => {
        let clonedInactiveRejectInfo = _.cloneDeep(inactiveRejects);
        clonedInactiveRejectInfo = value;
        setInactiveRejects(clonedInactiveRejectInfo);
    }

    const onChangeIsCheckTerm = (value) => {
        const clonedTermInfo = _.cloneDeep(beforeQuickReasonRef.current);
        let clonedIsCheckedTerm = _.cloneDeep(needQuickReason);
        clonedIsCheckedTerm = value;
        setNeedQuickReason(clonedIsCheckedTerm);
        setApprovalCommentQuickReasons(clonedTermInfo);
    };

    const onChangeCheckedCustom = (value) => {
        const clonedCustomButtonName = _.cloneDeep(beforeModifiedButtonNameRef.current);
        let clonedIsCustomButtonName = _.cloneDeep(needCustomButton);
        clonedIsCustomButtonName = value;
        setNeedCustomButton(clonedIsCustomButtonName);
        setCustomButtonNames(clonedCustomButtonName);
    };

    const onChangeCustomButtonName = (value) => {
        let clonedCustomButtonName = _.cloneDeep(customButtonNames);
        clonedCustomButtonName = value;
        setCustomButtonNames(clonedCustomButtonName);
    };  

    const onChangeAutoApprovedProcess = (value) => {
        let clonedAutoApprovedProcess = _.cloneDeep(autoApprovedProcess);
        clonedAutoApprovedProcess = value;
        setAutoApprovedProcess(clonedAutoApprovedProcess);
    };

    const onChangeDuration = (value) => {
        let clonedDuration = _.cloneDeep(duration);
        clonedDuration = value;
        setDuration(clonedDuration);
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
        setqueryDefintionForJob(requestOption.data);
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
        setqueryDefintionForJob(requestOption.data);
        const result = await fetchUtility(requestOption);
        onQueryEnd(result, pageIndex, isCheckedAll);
    };

    const onChangeChecked = () => {

        const willCheckedItems = [];
        const willUnCheckedItems = [];
        itemCacheRef.current.forEach(value => {
            value.forEach(i => {
                if (i.checked && i.internalApprovedStatus !== ApprovalStatus.WorkflowComplete) {
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

    const onKeyUpCheckedSelectedAll = (e, isSelectedAll) => {
        if(e.keyCode !== 13) {
            return;
        }
        e.stopPropagation();
        onCheckedSelectedAll(isSelectedAll);
    };

    const onCheckedSelectedAll = (isSelectedAll) => {
        itemCacheRef.current.forEach(value => {
            value.forEach(item => {
                item.checked = isSelectedAll;
            });
        });

        const clonedItems = [...items];
        clonedItems.forEach(item => {
            item.checked = isSelectedAll;
        });

        setItems(clonedItems);
        setIsCheckedAll(isSelectedAll);
        if (!isSelectedAll) {
            setCheckedItems([]);
            setUnCheckedItems([]);
        }
    };

    const managedColumnsChanged = (columns) => {
        let checkedColumns = columns.filter(item => item.visible);
        LocalStorage.set(CacheKeys.URCheckedManageColIds, checkedColumns.map(item => item.id));
        Utility.checkAllColumns(checkedColumns, "under-review");
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
        <div className="reco-manual-under-review">
            <section className="reco-manual-review-filter-bar">
                <R.Searchbox
                    placeholder={RMResx.RM_MA_Search_Description}
                    onSearch={onSearch}
                    width={380}
                />
                <div className="reco-manual-review-right-bar">
                    <R.Button
                        className="filtered-button"
                        primary={isFiltered}
                        classify={isFiltered ? "theme" : "default"}
                        text={isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                        type="button"
                        icon="fia-filter"
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
                    {(RM.RoleType === RoleType.SupAdmin || checkPermission("RDM_ApprovalSetting", RM.UserResources)) && (
                        <R.Button
                            className="theme"
                            text={RMResx.RM_RDM_MA_ApprovalSettings}
                            type="button"
                            icon="fia-configure"
                            tooltip={RMResx.RM_RDM_MA_ApprovalSettings}
                            onClick={onConfigButtonClick}
                        />
                    )}
                </div>
            </section>
            <section className="reco-manual-review-action-bar">
                <UnderReviewActions
                    disabledEscalate={disabledEscalate}
                    checkedItems={checkedItems}
                    unCheckedItems={unCheckedItems}
                    isCheckedAll={isCheckedAll}
                    itemCount={itemCount}
                    limitItemsCount={5000}
                    onReload={onReload}
                    settingModel={settingModel}
                    queryDefintion={queryDefintionForJob}
                    checkedCommentOption={checkedCommentOption}
                    ApprovalCommentQuickReason={approvalCommentQuickReasons}
                    InactiveRejects={inactiveRejects}
                    NeedQuickReason={needQuickReason}
                    NeedCustomButton={needCustomButton}
                    CustomButtonNames={customButtonNames}
                    canDoActionForReclassify={canDoActionForReclassify}
                    isFSSettingClassificationFolderLevel={isFSSettingClassificationFolderLevel}
                    filterDefinitions={filterDefinitions}
                    searchFilterDefinition={searchFilterDefinition}
                    isHideReclassifyBtnByApiSetting={isHideReclassifyBtnByApiSetting}
                />
            </section>
            <UnderReviewTable
                items={items}
                onSort={onSort}
                onChangeChecked={onChangeChecked}
                onReload={onReload}
                isCheckedAll={isCheckedAll}
                settingModel={settingModel}
                disabledEscalate={disabledEscalate}
                columns={manageColumns}
                checkedCommentOption={checkedCommentOption}
                ApprovalCommentQuickReason={approvalCommentQuickReasons}
                InactiveRejects = {inactiveRejects}
                NeedQuickReason={needQuickReason}
                onFilter={onFilter}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                SpeciallEnableReviewOnlyOneLocationDefinitions ={SpecialReviewOnlyOneLocationDefinitions}
                customButtonNames={customButtonNames}
                needCustomButton={needCustomButton}
                customColumns={customColumns}
                onChangeItems={(newItems) => setItems([...newItems])}
                canDoActionForReclassify={canDoActionForReclassify}
                isFSSettingClassificationFolderLevel={isFSSettingClassificationFolderLevel}
                filterDefinitions={filterDefinitions}
                searchFilterDefinition={searchFilterDefinition}
                isHideReclassifyBtnByApiSetting={isHideReclassifyBtnByApiSetting}
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
                    hasNextPage={pageIndex * pageSize < itemCount}
                    currentPageCount={items.length}
                    onPageIndexChange={onPageIndexChange}
                    onPageSizeChange={onPageSizeChange}
                    pageIndex={pageIndex}
                />
            </section>
            <UnderReviewFilterPanel
                show={showPanel}
                onFilter={onFilter}
                onHide={onHide}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
                SpecialEnableReviewDefinitions={SpecialReviewDefinitions}
                approvalCommentQuickReasons = {approvalCommentQuickReasons}
                SpeciallEnableReviewOnlyOneLocationDefinitions ={SpecialReviewOnlyOneLocationDefinitions}
                customColumns={customColumns}
            />
            <ApprovalCommentSettingPanel
                show={showConfigPanel}
                onHide={onHide}
                ApprovalComment={approvalCommentOptions}
                CheckedCommentOption={checkedCommentOption}
                ApprovalCommentQuickReason={approvalCommentQuickReasons}
                InactiveRejects={inactiveRejects}
                CustomButtons={customButtonNames}
                Duration={duration}
                NeedQuickReason={needQuickReason}
                NeedCustomButton={needCustomButton}
                AutoApprovedProcess={autoApprovedProcess}
                IsRecheckRule={isCheckingRuleBeforeDispose}
                enableDeleteInvalidRecords={enableDeleteInvalidRecords}
                StayManualReviewOption={stayManualReview}
                onSave={onSaveConfigration}
                onChange={onChangeCommentTermInfo}
                onChangeDisableTermInfo={onChangeDisableTermInfo}
                onChangeCheckedTerm={onChangeIsCheckTerm}
                onChangeCheckedCustom={onChangeCheckedCustom}
                onChangeCustomButtonName={onChangeCustomButtonName}
                onChangeAutoApprovedProcess={onChangeAutoApprovedProcess}
                onChangeDuration={onChangeDuration}
                onReload={onReload}
                onRecheckRuleSetting={setIsCheckingRuleBeforeDispose}
                onEnableDeleteInvalidRecordsSetting={setEnableDeleteInvalidRecords}
                module={ApprovalSettingModule.RecordForReview}
            />
        </div>
    );

};

export default UnderReview;