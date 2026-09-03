import React, { useRef, useState } from "react";
import { FilterOptions } from "./Constants/index";
import { useDidUpdateEffect } from "./Hooks/index";
import { HistoryTableCloumns } from "./Tables/Config/index";
import Utility from "./Utility";

import _ from "lodash";
import Paginate from "./Paginate";
import HistoryFilterPanel from "./FilterPanels/HistoryFilterPanel";
import HistorActionPanel from "./Panels/HistoryExportActionPanel";
import HistoryTable from "./Tables/HistoryTable";
import useBuildFilterDefinitionFromUrl from "./Hooks/BuildFilterDefinitionFromUrl";
import ManageColumns from "./Common/ManageColumns";
import { showToast } from "../../../Utilities/CommonUtil";
import { RoleType } from "../ManualApproval/Constants/RoleType";
import { ExportType } from "../ManualApproval/Constants/index";

const ExportSetting = {
    LatestExportType: ExportType.After3Month,
    CustomDate:{
        StartDateTime:RM.TimeUtil.getCommonDateStr(new Date(0)),
        EndDateTime:RM.TimeUtil.getCommonDateStr(new Date(0))
    }
};

const formatIrregularDate = (date) => {
    if(date){
        if(RM.TimeUtil.getGlobalAuiFormat().includes("dd-MM-yyyy")){
            let timeArr = date.split("-");
            let newTimeArr = [timeArr[1], timeArr[0], timeArr[2]];
            return newTimeArr.join("-");
        }
    }
    return date;
};

const BuildQueryRequestOptions = (itemCount, pageIndex, pageSize, filterDefinitions, searchFilterDefinition, sortDefinition) => {

    const clonedFilterDefinitions = _.cloneDeep(filterDefinitions);
    if (!_.isNil(searchFilterDefinition)) {
        clonedFilterDefinitions.push(searchFilterDefinition);
    }


    if (clonedFilterDefinitions.some(item => item.FilterOption === FilterOptions.RuleDisposalClass)) {
        let RuleDisposalClassObject = clonedFilterDefinitions.find(item => item.FilterOption === FilterOptions.RuleDisposalClass);
        if (RuleDisposalClassObject) {
          let RuleDisposalClassValue = JSON.parse(RuleDisposalClassObject.Value);
          if (RuleDisposalClassValue.includes("None")) {
            RuleDisposalClassValue.push("");
            RuleDisposalClassObject.Value = JSON.stringify(RuleDisposalClassValue);
          }
        }
      }
      
   
    const queryDefintion = {
        pageIndex: pageIndex,
        pageSize: pageSize,
        needCalculationCount: itemCount === 0,
        filters: clonedFilterDefinitions,
    };

    if (!_.isNil(sortDefinition)) {
        queryDefintion.OrderBy = sortDefinition.orderBy;
        queryDefintion.IsDesc = sortDefinition.isDesc;
    }

    return queryDefintion;
};

const History = ({ filterAvailableOptions }) => {

    const querierRef = useRef(new HistoryQuerier([]));

    const itemCacheRef = useRef(new Map());

    const [showPanel, setShowPanel] = useState(false);

    const [showActionPanel, setShowActionPanel] = useState(false);

    const [filterDefinitions, setFilterDefinitions] = useState([]);

    const [filterDefinitionsCache, setFilterDefinitionsCache] = useState([]);

    const [searchFilterDefinition, setSearchFilterDefinition] = useState(null);

    const [sortDefinitioin, setSortDefinition] = useState(null);

    const [items, setItems] = useState([]);

    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState(10);

    const [itemCount, setItemCount] = useState(0);

    const [settingModel, setSettingModel] = useState(ExportSetting);

    const [manageColumns, setManageColumns] = useState(Utility.checkAllColumns(HistoryTableCloumns, "history"));

    const [isFiltered, setIsFiltered] = useState(false);

    useBuildFilterDefinitionFromUrl(async (filterDefinition) => {
        $$.loading(true);
        const requestOption = {
            url: "/API/ManualApproval/HistoryAzureTableQuery",
        };
        const res = await fetchUtility(requestOption);
        querierRef.current = new HistoryQuerier(res);
        setFilterDefinitions(filterDefinition);
        setFilterDefinitionsCache(filterDefinition);
        $$.loading(false);
    });

    useDidUpdateEffect(async () => {
        $$.loading(true);
        setPageIndex(1);
        itemCacheRef.current.clear();
        const requestOption = BuildQueryRequestOptions(0, 1, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        const result = querierRef.current.query(requestOption);
        onQueryEnd(result, 1, true);
        onCheckFiltered(filterDefinitions);
    }, [filterDefinitions, searchFilterDefinition, sortDefinitioin]);

    const onQueryEnd = (result, pageIndex, needUpdateCount = false) => {
        setItems(result.items);
        if (needUpdateCount) {
            setItemCount(result.count);
        }
        itemCacheRef.current.set(pageIndex, result.items);
        $$.loading(false);
    };

    const onFilter = (value) => {
        setFilterDefinitions(value);
        setFilterDefinitionsCache(value);
        setShowPanel(false);
    };

    const onHide = () => {
        setShowPanel(false);
        setShowActionPanel(false);
    };

    const onFilterButtonClick = () => {
        setFilterDefinitionsCache([...filterDefinitionsCache]);
        setShowPanel(true);
    };

    const onChangeExportSetting = (value) => {
        const clonedSettingModel = _.cloneDeep(settingModel);
        clonedSettingModel.LatestExportType = value.LatestExportType;
        clonedSettingModel.CustomDate = value.CustomDate;
        setSettingModel(clonedSettingModel);
    };

    const onExportButtonClick = async () => {
        setShowActionPanel(true);
    };

    const onExport = async (value) => {
        const requestOption = {
            url: "/api/ManualApproval/RunExportHistoryDataJob",
            data: value,
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        if (result.MessageType == "0") {
            if (RM.RoleType != RoleType.StandardUser) {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            } else {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_EndUser_JobStart}>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            }
        } else {
            showToast.error(result.ErrorMessage);
        }
        setShowActionPanel(false);
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
            isDesc: args.isDesc
        };
        setSortDefinition(value);
    };

    const onPageSizeChange = (pageSize) => {
        $$.loading(true);
        itemCacheRef.current.clear();
        setPageSize(pageSize);
        setPageIndex(1);
        const requestOption = BuildQueryRequestOptions(0, 1, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        const result = querierRef.current.query(requestOption);
        onQueryEnd(result, 1);
    };

    const onPageIndexChange = (pageIndex) => {
        $$.loading(true);
        setPageIndex(pageIndex);
        const requestOption = BuildQueryRequestOptions(itemCount, pageIndex, pageSize, filterDefinitions, searchFilterDefinition, sortDefinitioin);
        const result = querierRef.current.query(requestOption);
        onQueryEnd(result, pageIndex);
    };

    const managedColumnsChanged = (columns) => {
        let checkedColumns = columns.filter(item => item.visible);
        Utility.checkAllColumns(checkedColumns, "history");
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
        <div className="reco-manual-history">
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
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_MA_ExportToHistory}
                    type="button"
                    tooltip={RMResx.RM_MA_ExportToHistory}
                    onClick={onExportButtonClick}
                />
                <div className="reco-manual-review-actions">
                    <div></div>
                    <div className="reco-manual-review-actions-desc">
                        {
                            RMResx.RM_Common_TotalCount.format(itemCount)
                        }
                        <$g.Popover>{RMResx.RM_JS_MA_ManualHistory_Tips}</$g.Popover>
                    </div>
                </div>
            </section>
            <div id="downloadDiv" style={{ display: "none" }} />
            <HistoryTable
                items={items}
                onSort={onSort}
                columns={manageColumns}
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
            <HistoryFilterPanel
                show={showPanel}
                onFilter={onFilter}
                onHide={onHide}
                defaultFilterDefinitions={filterDefinitionsCache}
                filterAvailableOptions={filterAvailableOptions}
            />
            <HistorActionPanel
                show={showActionPanel}
                onHide={onHide}
                onChange={onChangeExportSetting}
                onSave={onExport}
                Setting={settingModel}
            />
        </div>
    );

};

class HistoryQuerier {

    constructor(items) {
        this.items = items;
        this.filters = new Map([
            [FilterOptions.CollectionTime, this.collectionTimeFilter],
            [FilterOptions.ActionTime, this.actionTimeFilter],
            [FilterOptions.Source, this.sourceFilter],
            [FilterOptions.ApprovalStatus, this.approvalStatusFilter],
            [FilterOptions.RuleName, this.ruleNameFilter],
            [FilterOptions.RuleDisposalClass, this.ruleDisposalClassFilter],
            [FilterOptions.ApprovedBy, this.approvedByFilter],
            [FilterOptions.LeafName, this.leafNameFilter],
            [FilterOptions.ModifiedTime, this.modifiedTimeFilter],
        ]);
    }

    collectionTimeFilter(value, items) {
        const startDateTime = new Date(value.StartTime);
        const endDateTime = new Date(value.EndTime);
        return items.filter(item => new Date(item.collectionTime) >= startDateTime && new Date(item.collectionTime) <= endDateTime);
    }

    modifiedTimeFilter(value, items) {
        const startDateTime = new Date(value.StartTime);
        const endDateTime = new Date(value.EndTime);
        return items.filter(item => new Date(item.modifiedTime) >= startDateTime && new Date(item.modifiedTime) <= endDateTime);
    }

    actionTimeFilter(value, items) {
        const startDateTime = new Date(value.StartTime);
        const endDateTime = new Date(value.EndTime);
        return items.filter(item => 
            new Date(formatIrregularDate(item.actionTime).replaceAll("-", "/")) >= startDateTime && 
            new Date(formatIrregularDate(item.actionTime).replaceAll("-", "/")) <= endDateTime
        );
    }

    sourceFilter(value, items) {
        return items.filter(item => value.some(i => i === item.sourceFlag));
    }

    approvalStatusFilter(value, items) {
        return items.filter(item => value.some(i => i === item.internalApprovedStatus));
    }

    ruleNameFilter(value, items) {
        return items.filter(item => value.some(i => i === item.ruleName));
    }

    ruleDisposalClassFilter(value, items) {
        return items.filter(item => value.some(i => i === item.ruleDisposalClass));
    }

    approvedByFilter(value, items) {
        return items.filter(item => value.some(i => i === item.approvedByUserId));
    }

    leafNameFilter(value, items) {
        const lowerValue = value.toLowerCase();
        return items.filter(item => !_.isNil(item.leafName) && item.leafName.trim() !== '' && item.leafName.toLowerCase().includes(lowerValue));
    }

    query(queryDefintion) {
        let filters = queryDefintion.filters;
        let items = this.items;
        for (let filter of filters) {
            const filterAction = this.filters.get(filter.FilterOption);
            let value = filter.Value;
            if(filter.FilterOption !== FilterOptions.LeafName) {
                value = JSON.parse(filter.Value);
            }
            items = filterAction(value, items);
        }

        const pagedItems = items.slice((queryDefintion.pageIndex - 1) * queryDefintion.pageSize, queryDefintion.pageIndex * queryDefintion.pageSize);
        return {
            items: pagedItems,
            count: items.length,
        };
    }
}

export default History;