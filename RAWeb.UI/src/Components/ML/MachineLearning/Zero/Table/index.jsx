import { useState, useEffect } from "react";
import _ from "lodash";
import PropTypes from "prop-types";

import SelectAllButton from "../../Table/Component/SelectAllButton";
import CheckedItemsCounter from "../../Table/Component/CheckedItemsCounter";
import { modifyItemsCheckStatus } from "../../Table/Utility";

import "./index.less";

const ZeroMLTable = (props) => {
    const {
        id,
        columns,
        items,
        template,
        itemKey,
        checkable,
        actionComponent,
        pagerComponent,
        showSelectedCounts,
        totalCount,
        onSort,
        onRowEvent,
        exsitSelectAll,
        onSelect,
        isReset,
        exsitSelectAllTip,
        selectAllTip,
    } = props;

    const [cachedItems, setCachedItems] = useState({});

    const [pagerItems, setPagerItems] = useState([]);

    const [isSelectAll, setIsSelectAll] = useState(false);

    const [selectedItemsCount, setSelectedItemsCount] = useState(0);

    useEffect(() => {
        if (checkable) {
            isReset ? resetItems() : setCheckableItems();
        } else {
            setPagerItems(items);
        }
    }, [items]);

    const setCheckableItems = () => {
        let currentPagerItems = getCurrentPagerItems();
        isSelectAll ? selectAllItems() : setPagerItems(currentPagerItems);
    };

    const getCurrentPagerItems = () => {
        let currentPagerItems = _.cloneDeep(items);
        for (let item of currentPagerItems) {
            let nodeKey = item[itemKey];
            if (cachedItems[nodeKey]) {
                item.checked = cachedItems[nodeKey].checked;
            } else {
                cachedItems[nodeKey] = item;
            }
        }
        return currentPagerItems;
    };

    const onSelectAllItems = (isSelectAll) => {
        isSelectAll ? selectAllItems() : clearItems();
        setIsSelectAll(isSelectAll);
    };

    const resetItems = () => {
        let currentCachedItems = getCurrentCachedItems({}, _.cloneDeep(items));
        let currentPagerItems = _.cloneDeep(items);
        let selectedItems = [];
        onSelectTableItems(
            false,
            currentCachedItems,
            currentPagerItems,
            selectedItems
        );
    };

    const selectAllItems = () => {
        let currentCachedItems = modifyItemsCheckStatus(cachedItems, true);
        let currentPagerItems = modifyItemsCheckStatus(
            _.cloneDeep(items),
            true
        );
        let selectedItems = Object.values(currentCachedItems);
        onSelectTableItems(
            true,
            currentCachedItems,
            currentPagerItems,
            selectedItems
        );
    };

    const clearItems = () => {
        let currentCachedItems = modifyItemsCheckStatus(cachedItems, false);
        let currentPagerItems = _.cloneDeep(items);
        let selectedItems = [];
        onSelectTableItems(
            false,
            currentCachedItems,
            currentPagerItems,
            selectedItems
        );
    };

    const onSelectItems = () => {
        let currentCachedItems = getCurrentCachedItems(
            cachedItems,
            _.cloneDeep(pagerItems)
        );
        let currentPagerItems = _.cloneDeep(pagerItems);
        let selectedItems = Object.values(currentCachedItems).filter((item) => {
            return item.checked;
        });
        onSelectTableItems(
            false,
            currentCachedItems,
            currentPagerItems,
            selectedItems
        );
    };

    const getCurrentCachedItems = (currentCachedItems, currentPagerItems) => {
        for (let currentPagerItem of currentPagerItems) {
            let nodeKey = currentPagerItem[itemKey];
            let cachedItem = currentCachedItems[nodeKey];
            if (cachedItem) {
                Object.assign(cachedItem, currentPagerItem);
            } else {
                currentCachedItems[nodeKey] = currentPagerItem;
            }
        }
        return currentCachedItems;
    };

    const onSelectTableItems = (
        isSelectAll,
        currentCachedItems,
        currentPagerItems,
        selectedItems
    ) => {
        setCachedItems(currentCachedItems);
        setPagerItems(currentPagerItems);
        setSelectedItemsCount(isSelectAll ? totalCount : selectedItems.length);
        setIsSelectAll(isSelectAll);
        onSelect(selectedItems, isSelectAll);
    };

    const onTableSort = (args) => {
        onSort(args.status === "asc", args.column.valuePath);
    };

    const renderNav = () => {
        if (showSelectedCounts) {
            return (
                <CheckedItemsCounter
                    totalCount={totalCount}
                    selectedItemsCount={selectedItemsCount}
                    actionComponent={actionComponent}
                ></CheckedItemsCounter>
            );
        } else {
            return (
                <div className="ra-common-table-item">{actionComponent}</div>
            );
        }
    };

    const renderTable = () => {
        return (
            <R.Table
                id={id}
                columns={columns}
                rowTemplate={template}
                items={pagerItems}
                checkable={checkable}
                flexible={true}
                onCheckByItems={false}
                doSort={onTableSort}
                onCheck={onSelectItems}
                onRowEvent={(args) => {
                    onRowEvent(args);
                }}
            />
        );
    };

    const renderSelectAllButton = () => {
        let exsitItems = items && items.length > 0;
        if (exsitSelectAll && exsitItems) {
            return (
                <SelectAllButton
                    onSelectAll={onSelectAllItems}
                    isShowSelectAll={!isSelectAll}
                    showPopover={exsitSelectAllTip}
                    popoverContent={selectAllTip}
                />
            );
        }
    };

    const renderFooter = () => {
        if (pagerComponent) {
            return (
                <div className="ra-common-table-item">
                    <div>{renderSelectAllButton()}</div>
                    {pagerComponent}
                </div>
            );
        }
    };

    return (
        <div className="ra-common-table">
            {renderNav()}
            {renderTable()}
            {renderFooter()}
        </div>
    );
};

ZeroMLTable.propTypes = {
    id: PropTypes.string,
    itemKey: PropTypes.string,
    columns: PropTypes.array,
    items: PropTypes.array,
    checkable: PropTypes.bool,
    actionComponent: PropTypes.any,
    template: PropTypes.any,
    onSort: PropTypes.func,
    onRowEvent: PropTypes.func,
    onSelect: PropTypes.func,
    showSelectedCounts: PropTypes.bool,
    totalCount: PropTypes.any,
    exsitSelectAll: PropTypes.bool,
    pagerComponent: PropTypes.any,
    isReset: PropTypes.bool,
    exsitSelectAllTip: PropTypes.bool,
    selectAllTip: PropTypes.string,
};

ZeroMLTable.defaultProps = {
    itemKey: "Id",
    totalCount: 0,
    columns: [],
    items: [],
    checkable: false,
    actionComponent: null,
    showSelectedCounts: false,
    exsitSelectAll: false,
    isReset: true,
    exsitSelectAllTip: false,
    selectAllTip: "",
};

export default ZeroMLTable;
