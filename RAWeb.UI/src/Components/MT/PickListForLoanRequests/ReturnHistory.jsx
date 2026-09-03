import React, { useState, useEffect, useRef } from "react";
import PickListTable from "../PickListCommon/PickListTable";
import { showToast } from "../../../Utilities/CommonUtil";
import { Messagebox } from "../../Common/Messagebox";
import ReturnHistoryFilterPanel from "./ReturnHistoryFilterPanel";

const ReturnHistory = ({
    recordListApiUrl,
    tableColumns,
    tableTemplate,
    exportUrl,
}) => {
    const [recordList, setRecordList] = useState([]);

    const [searchValue, setSearchValue] = useState("");

    const [filterOptions, setFilterOptions] = useState({});

    const [paginateInfo, setPaginateInfo] = useState({
        pageIndex: 0,
        pageSize: 10,
        shownCount: 10,
        hasNext: false,
    });

    const tableRef = useRef();

    const filterPanelRef = useRef();

    useEffect(() => {
        loadRecordList();
    }, [searchValue, filterOptions, paginateInfo.pageIndex, paginateInfo.pageSize, paginateInfo.shownCount]);

    const getRecordListParam = () => {
        return {
            SearchText: searchValue,
            FilterOptions: filterOptions,
            PageIndex: paginateInfo.pageIndex,
            PageSize:  paginateInfo.pageSize, 
        };
    };

    const loadRecordList = () => {
        $$.loading(true);
        const option = {
            url: recordListApiUrl,
            method: "Post",
            data: getRecordListParam(),
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res) {
                setRecordList(res.Datas || []);
                setPaginateInfo((prev) => ({
                    ...prev,
                    shownCount: res.Datas.length,
                    hasNext: (res.TotalCount - prev.pageSize * (prev.pageIndex * 1 + 1)) > 0,
                }));
            }
        });
    };

    const onSearch = (args) => {
        setSearchValue(args ? args : "");
    };

    const openFilterPanel = () => {
        filterPanelRef.current.openPanel(filterOptions);
    };

    const onFilter = () => {
        setFilterOptions(
            RM.deepcopy(filterPanelRef.current.getFilterOptions())
        );
    };

    const onExportBtn = () => {
        Messagebox({
            content: RMResx.RM_JS_Common_ExportMsg,
            actionFun: onExport,
        });
    };

    const onExport = () => {
        $$.loading(true);
        const param = { SearchText: searchValue, FilterOptions: filterOptions };
        const option = {
            data: param,
            url: exportUrl,
            method: "Post",
        };
        fetchUtility(option)
            .then((res) => {
                $$.loading(false);
                if (res.MessageType == 0) {
                    showToast.success(
                        <$g.I18NProvider
                            msg={RMResx.RM_MA_HistoryExport_JobStart}
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                            <a className="ra-link-a" href="/Root/DC/Download">
                                {RMResx.RM_JS_DC_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                } else {
                    showToast.success(
                        RMResx.RM_CP_AM_Certificate_OperationFailed_Tip
                    );
                }
            })
            .catch((e) => {
                $$.loading(false);
                showToast.success(
                    RMResx.RM_CP_AM_Certificate_OperationFailed_Tip
                );
            });
    };

    const onPageChange = (currentPageIndex, currentPageSize) =>{
        setPaginateInfo((prev) => ({
            ...prev,
            pageIndex: currentPageIndex,
            pageSize: currentPageSize,
        }))
    };

    const renderHeader = () => {
        return (
            <div className="ra-main-header">
                <R.Searchbox
                    id="raMtReturnHistorySearchbox"
                    placeholder={RMResx.RM_JS_RM_SearchContainerTxt}
                    onSearch={onSearch}
                    width={380}
                />
                <R.Button
                    id="raMtReturnHistoryFilterBtn"
                    icon="fia-filter"
                    text={RMResx.RM_Common_Filter}
                    onClick={openFilterPanel}
                />
            </div>
        );
    };

    const renderNavBar = () => {
        return (
            <div className="ra-main-navbar">
                <div className="ra-tm-picklist-actions flex align-center gap-s">
                    <R.Button
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_MT_PickList_ExportBtn}
                        onClick={onExportBtn}
                    />
                </div>
            </div>
        );
    };

    const renderTable = () => {
        return (
            <div className="ra-main-table">
                <PickListTable
                    ref={tableRef}
                    id="ReturnHistoryTable"
                    itemKey={"Id"}
                    items={recordList}
                    columns={tableColumns}
                    template={tableTemplate}
                    checkable={false}
                />
            </div>
        );
    };

    const renderTableFooter = () => {
        return (
            <div style={{ justifyContent: "end" }} className="ra-main-footer">
                <$g.SimplePager
                    pagerIndex={paginateInfo.pageIndex}
                    pagerSize={paginateInfo.pageSize}
                    shownCount={paginateInfo.shownCount}
                    hasNext={paginateInfo.hasNext}
                    showPagerSize={true}
                    pagerSizeOptions={[5, 10, 15, 50]}
                    onChange={onPageChange}
                />
            </div>
        );
    }

    const renderFilterPanel = () => {
        return (
            <ReturnHistoryFilterPanel
                ref={filterPanelRef}
                onFilter={onFilter}
            />
        );
    };

    return (
        <React.Fragment>
            <div className="ra-page-container">
                {renderHeader()}
                {renderNavBar()}
                {renderTable()}
                {renderTableFooter()}
                {renderFilterPanel()}
            </div>
        </React.Fragment>
    );
};

export default ReturnHistory;
