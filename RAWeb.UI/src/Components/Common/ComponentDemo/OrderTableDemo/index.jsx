import React, { useState } from "react";
import { OrderTable } from "../../OrderTable";

export const OrderTableDemo = () => {
    const [columns] = useState([
        {
            key: "column1",
            name: "Display name",
            width: 10,
            minWidth: 150,
            fieldName: "field1",
        },
        { key: "column2", name: "Mapped key", width: 250, minWidth: 150, fieldName: "field2" },
        {
            key: "column3",
            name: "",
            width: 80,
            minWidth: 150,
            onRender: (item) => (
                <>
                    <button
                        onClick={() => {
                            setItems((pre) =>
                                pre.map((i) => {
                                    if (i.id === item.id) {
                                        return { ...i, field1: "edited" };
                                    }
                                    return i;
                                })
                            );
                        }}
                    >
                        Edit
                    </button>
                    <button
                        onClick={() =>
                            setItems((pre) =>
                                pre.filter((i) => i.id !== item.id)
                            )
                        }
                    >
                        Delete
                    </button>
                </>
            ),
        },
    ]);

    const addedRowItem = {
        id: Math.random().toString(36).substring(2, 9),
        field1: "test1",
        field2: Math.random().toString(36).substring(2, 9),
    };

    const [items, setItems] = useState([
        {
            id: Math.random().toString(36).substring(2, 9),
            field1: "Series Number",
            field2: Math.random().toString(36).substring(2, 9),
        },
        {
            id: Math.random().toString(36).substring(2, 9),
            field1: "Box Barcode Number",
            field2: Math.random().toString(36).substring(2, 9),
        },
        {
            id: Math.random().toString(36).substring(2, 9),
            field1: "Box Barcode a",
            field2: Math.random().toString(36).substring(2, 9),
        },
        {
            id: Math.random().toString(36).substring(2, 9),
            field1: "Box Barcode b",
            field2: Math.random().toString(36).substring(2, 9),
        },
    ]);

    const handleOrderChange = (newItems) => {
        setItems([...newItems]);
    };

    const handleAddRow = () => {
        setItems((pre) => [...pre, addedRowItem]);
    };

    // const handleGetDataClick = () => {
    //    console.log(items);
    // }

    return (
        <div style={{ background: "#fff", padding: 20 }}>
            <OrderTable
                columns={columns}
                items={items}
                onOrderChange={handleOrderChange}
                onAddRow={handleAddRow}
            />
            {/* <button onClick={handleGetDataClick}>{"Get Data"}</button> */}
        </div>
    );
};
