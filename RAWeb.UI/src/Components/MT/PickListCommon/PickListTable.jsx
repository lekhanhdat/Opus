import React, { useState, useImperativeHandle, forwardRef, useRef, useEffect } from "react";

let cacheItems = {};

const PickListTable = ({ id, columns, template, height, items, onChange, itemKey, checkable }, ref) => {

    const [currentPagerItems, setCurrentPagerItems] = useState([]);

    const tableRef = useRef();

    useImperativeHandle(ref, () => ({
        setSelectedData: (isSelectAllResult) => {
            isSelectAllResult ? setSelectAllItems() : setCacheItems();
            setCurrentPagerItems(items);
        },
        clearSelectedData: () => {
            setClearAllItems();
            setCurrentPagerItems(items);
        },
    }));

    useEffect(() => {
        setCurrentPagerItems(items);
    }, [items])

    const setSelectAllItems = () =>{
        tableRef.current.selectAll(true);
        setCacheItemsOfSelectAll(true);
    };

    const setClearAllItems = () =>{
        cacheItems = {};
        tableRef.current.selectAll(false);
        setCacheItemsOfSelectAll(false);
        onChange && onChange([]);
    };

    const setCacheItemsOfSelectAll = (isSelectAllResult) =>{
        setCacheItems();
        for(let item of items){
            item.checked = isSelectAllResult;
        }
        for (let key in cacheItems) {
            cacheItems[key].checked = isSelectAllResult;
        }
    };

    const setCacheItems = () => {
        for (let item of items) {
            let nodeKey = item[itemKey];
            if (cacheItems[nodeKey]) {
                Object.assign(item, cacheItems[nodeKey]);
            } else {
                cacheItems[nodeKey] = item;
            }
        }
    };

    const onSelectItems = () => {
        for (let item of items) {
            let nodeKey = item[itemKey];
            if (cacheItems[nodeKey]) {
                Object.assign(cacheItems[nodeKey], item); 
            } else {
                cacheItems[nodeKey] = item;
            }
        }
        let selectedItems = Object.values(cacheItems).filter((item) => {
            return item.checked;
        });
        onChange && onChange(selectedItems);
    };

    return (
        <R.Table
            id={id}
            ref={tableRef}
            columns={columns}
            rowTemplate={template}
            height={height}
            items={currentPagerItems}
            checkable={checkable}
            flexible={true}
            onCheck={onSelectItems}
        />
    );
};

export default forwardRef(PickListTable);
