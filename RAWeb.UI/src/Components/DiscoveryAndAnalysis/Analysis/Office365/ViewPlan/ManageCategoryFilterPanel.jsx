import React, { useEffect, useState } from "react";

const ManageCategoryFilterPanel = ({
    show,
    onHide,
    onApply,
    categoryItems = [],
    currentCategory,
}) => {
    const [category, setCategory] = useState(currentCategory || "all");

    useEffect(() => {
        setCategory(currentCategory || "all");
    }, [currentCategory, show]);

    const onCategoryChange = (item) => {
        if (item && item.newValue && item.newValue.value) {
            setCategory(item.newValue.value);
            return;
        }

        if (item && item.value) {
            setCategory(item.value);
            return;
        }

        setCategory(item || "all");
    };

    const onSaveFilter = () => {
        onApply({ category });
    };

    return (
        <R.Panel
            id="ra-view-plan-manage-category-filter-panel"
            header="Filter"
            size={760}
            onHide={onHide}
            onClose={onHide}
            status={{ show }}
            destroy={true}
        >
            <div className="ra-view-plan-filter-panel-body">
                <label className="ra-view-plan-filter-label">
                    Category <span>*</span>
                </label>
                <R.Combobox
                    id="ra-view-plan-filter-category"
                    textField="name"
                    valueField="value"
                    items={categoryItems}
                    value={category}
                    searchable={false}
                    width="100%"
                    onChange={onCategoryChange}
                />
            </div>

            <>
                <R.Button slot="buttons" text="Cancel" onClick={onHide} />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text="Save"
                    onClick={onSaveFilter}
                />
            </>
        </R.Panel>
    );
};

export default ManageCategoryFilterPanel;