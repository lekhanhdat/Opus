import {
    forwardRef,
    useEffect,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";

import SearchBox from "../../../Search/SearchBox";
import { AddScopesTableColumns } from "../../../Config/TableColumnsConfig";
import AddScopeTemplate from "../../../RowTemplate/AddScopeTemplate";
import ContinuationTokenPager from "../../../../../Common/ContinuationTokenPager";
import { RAMessageType } from "../../../Config/Constains";
import { showToast } from "../../../../../../Utilities/CommonUtil";

function AddTrainingScope({ doAction }, ref) {
    const [showAddScopePanel, setShowAddScopePanel] = useState(false);
    const [searchValue, setSearchValue] = useState("");
    const [items, setItems] = useState([]);
    const [selectedItems, setSelectedItems] = useState([]);
    const [termPaths, setTermPaths] = useState({});
    const [pageInfo, setPageInfo] = useState({
        currentPageSize: 10,
        totalCount: 0,
    });
    const [continuationToken, setContinuationToken] = useState("");
    const [sortInfo, setSortInfo] = useState({
        sortBy: "",
        isAscending: false,
    });

    const tokenPagerRef = useRef();

    useImperativeHandle(ref, () => ({
        openPanel: () => {
            setSelectedItems([]);
            setShowAddScopePanel(true);
            loadUsageTrainingData(pageInfo.currentPageSize, "");
        },
    }));

    useEffect(() => {
        loadUsageTrainingData(pageInfo.currentPageSize, searchValue, selectedItems);
    }, [sortInfo.sortBy, sortInfo.isAscending]);

    const loadUsageTrainingData = async (pageSize, searchValue, selectedItems = [], continuationToken) => {
        const requestOption = {
            url: "/api/TrainingScopeApi/LoadUsageScope",
            method: "POST",
            data: {
                SearchValue: searchValue,
                PageIndex: continuationToken || "",
                PageSize: pageSize,
                SortBy: sortInfo.sortBy,
                IsAscending: sortInfo.isAscending,
            },
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (_.isUndefined(continuationToken)) {
            tokenPagerRef.current?.reset();
        }
        if (res) {
            const selectedIds = new Set(selectedItems.map((item) => item.Id));
            setItems(res.TrainingScopes.map((item) => ({
                ...item,
                checked: selectedIds.has(item.Id),
            })));
            if (res.PageIndex) {
                setContinuationToken(res.PageIndex);
            }
            setPageInfo((prev) => ({ ...prev, totalCount: res.TotalCount }));
        }
    };

    const setTermFullPath = (args) => {
        const termId = args.rowData.TermId;
        const option = {
            url: `/api/TermManagementApi/GetTermWithPath/?termId=${termId}`,
            method: "GET",
        };
        const newItems = [...items];
        if (termPaths[termId]) {
            newItems[args.rowIndex].IsShowTermFullPath = true;
            newItems[args.rowIndex].TermFullPath = termPaths[termId];
            setItems(newItems);
        } else {
            fetchUtility(option).then((res) => {
                const data = JSON.parse(res);
                newItems[args.rowIndex].IsShowTermFullPath = true;
                newItems[args.rowIndex].TermFullPath = data.FullPath;
                setTermPaths((prev) => ({
                    ...prev,
                    [termId]: data.FullPath,
                }));
                setItems(newItems);
            }).catch((e) => {
            });
        }
    }

    // For table
    const onSearch = (value) => {
        setSearchValue(value);
        loadUsageTrainingData(pageInfo.currentPageSize, value);
    };

    const onSelectItems = (list) => {
        if (!list.length) {
            setSelectedItems([]);
            return;
        }
        setSelectedItems((prev) => {
            const existingIds = new Set(prev.map(item => item.Id));
            const newItems = list.filter(item => !existingIds.has(item.Id));
            return [...prev, ...newItems];
        });
    };

    const onTableSort = (args) => {
        setSortInfo({
            sortBy: args.column.valuePath,
            isAscending: args.status === "asc",
        });
    };

    const onRowEvent = (args) => {
        switch (args.type) {
            case 'showTermFullPath':
                setTermFullPath(args);
                break;
            default:
                break;
        }
    }

    const onPageChange = (continuationToken, pageSize) => {
        setContinuationToken(continuationToken);
        setPageInfo((prev) => ({
            ...prev,
            currentPageSize: pageSize,
        }));
        loadUsageTrainingData(pageSize, searchValue, selectedItems, continuationToken);
    };

    // For panel
    const onCloseAddScopePanel = () => {
        setShowAddScopePanel(false);
        setSearchValue("");
    };

    const onSaveAddScope = async () => {
        const requestOption = {
            url: "/api/TrainingScopeApi/AddTrainingScopeManually",
            method: "POST",
            data: selectedItems,
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            if (res.MessageType === RAMessageType.Successful) {
                showToast.success(RMResx.RM_ML_Add_Scope_Success);
                onCloseAddScopePanel();
                doAction()
            } else {
                showToast.error(res.ErrorMessage);
            }
        }
    };

    return (
        <div>
            <R.Panel
                id="raMtAddTrainingScopePanel"
                header={RMResx.RM_ML_TrainingScope_AddBtn}
                size={664}
                status={{ show: showAddScopePanel }}
                onHide={onCloseAddScopePanel}
                destroy={true}
            >
                <div id="raMlAddScopeTable" className="flex flex-column">
                    <div className="margin-bottom-l">
                        <SearchBox
                            onSearch={onSearch}
                            placeholder={RMResx.RM_ML_TS_Search_Placeholder}
                            width={"100%"}
                        />
                    </div>
                    <R.Table
                        id="raMTAddScopeTable"
                        columns={AddScopesTableColumns}
                        rowTemplate={AddScopeTemplate}
                        items={items}
                        checkable
                        flexible
                        onCheckByItems={false}
                        doSort={onTableSort}
                        onCheck={onSelectItems}
                        onRowEvent={onRowEvent}
                    />
                    <div className="ra-main-footer">
                        <ContinuationTokenPager
                            ref={tokenPagerRef}
                            totalCount={pageInfo.totalCount}
                            shownCount={items.length}
                            showPagerCounter
                            showPagerSize
                            continuationToken={continuationToken}
                            pagerSizeOptions={[5, 10, 15, 50]}
                            onChange={onPageChange}
                        />
                    </div>
                </div>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={onCloseAddScopePanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        disabled={!selectedItems.length}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={onSaveAddScope}
                    />
                </>
            </R.Panel>
        </div>
    );
}

export default forwardRef(AddTrainingScope);
