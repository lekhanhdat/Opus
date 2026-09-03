import React, { useEffect, useMemo, useState } from "react";
import PropTypes from "prop-types";
import { SampleTable } from "../SampleTable";
import { OrderDropdown } from "./orderDropdown";
import "./index.less";

export const OrderTable = ({ columns, items, onOrderChange, onAddRow }) => {
    const getOrderItems = (newItems) => {
        return newItems.map((item, index) => ({
            ...item,
            orderNumber: index,
        }));
    };

    const [tableItems, setTableItems] = useState(getOrderItems(items));

    const orderColumn = useMemo(
        () => ({
            key: "orderNumber",
            name: RMResx.RM_ES_CompliantExport_ChildTable_OrderColumn,
            width: 10,
            minWidth: 80,
            onRender: (item) => {
                return (
                    <OrderDropdown
                        order={item.orderNumber + 1}
                        maxOrder={tableItems.length}
                        onChange={(oldOrder, newOrder) => {
                            handleOrderChange(oldOrder, newOrder);
                        }}
                    />
                );
            },
        }),
        [tableItems]
    );

    useEffect(() => {
        setTableItems(getOrderItems(items));
    }, [items]);

    const handleKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    const handleOrderChange = (oldOrder, newOrder) => {
        const newItems = [...tableItems];
        [newItems[oldOrder - 1], newItems[newOrder - 1]] = [
            newItems[newOrder - 1],
            newItems[oldOrder - 1],
        ];

        const orderedItems = getOrderItems(newItems);
        setTableItems(orderedItems);
        onOrderChange?.(orderedItems);
    };

    return (
        <div className="opus-common-order-table">
            <SampleTable
                columns={[orderColumn, ...columns]}
                items={tableItems}
                flexible
            />
            <div tabIndex={0} className="opus-common-order-table-add-row flex justify-center align-center" onKeyDown={handleKeyDown} onClick={onAddRow}>
                <div className="fia-plus"></div>
                <div>
                    {RMResx.RM_ES_CompliantExport_AddMappingTitle}
                </div>
                <div className="fia-triangle-down"></div>
            </div>
        </div>
    );
};

OrderTable.propTypes = {
    columns: PropTypes.arrayOf(
        PropTypes.shape({
            key: PropTypes.string.isRequired,
            name: PropTypes.string.isRequired,
            width: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
                .isRequired,
            fieldName: PropTypes.string,
            onRender: PropTypes.func,
        })
    ).isRequired,
    items: PropTypes.arrayOf(PropTypes.object).isRequired,
    onOrderChange: PropTypes.func,
    onAddRow: PropTypes.func.isRequired,
};
