import { useEffect, useState } from "react";
import _ from "lodash";

import { FilterOptions } from "../Constants";

const filterOption = FilterOptions.CustomColumnYesOrNo;

const initialState = [
    {
        name: RMResx.RM_JS_Common_Yes,
        value: 1,
        checked: false,
    },
    {
        name: RMResx.RM_JS_Common_No,
        value: 2,
        checked: false,
    },
];

function CustomColumnYesOrNoFilter(props) {
    const {
        title,
        customColumnId,
        filterDefinitions,
        onFilterChange,
        onRemoveFilterChange,
    } = props;

    const [items, setItems] = useState(initialState);

    useEffect(() => {
        if (!filterDefinitions.has(customColumnId)) {
            setItems(initialState);
            return;
        }
        const objValue = filterDefinitions.get(customColumnId);
        const value = objValue.Value;
        const clonedItems = _.cloneDeep(items);
        clonedItems[0].checked = value;
        clonedItems[1].checked = !value;
        setItems(clonedItems);
    }, [filterDefinitions]);

    const onChange = (args) => {
        const newValue = args.newValue;
        if (!newValue) {
            onRemoveFilterChange(customColumnId);
            return;
        }

        const newItems = _.cloneDeep(items);
        newItems.forEach((item) => {
            item.checked = item.value === newValue.value;
        });
        setItems(newItems);
        const obj = {
            FilterOption: filterOption,
            Value: newValue.value === 1 ? true : false, // Yes is true, no is false
            CustomColumnId: customColumnId,
        };
        onFilterChange(obj);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title" tabIndex="0">
                {title}
            </div>
            <R.Combobox
                id={`raCustomColumnCbx-${customColumnId}`}
                items={items}
                width="100%"
                searchable={false}
                textField="name"
                valueField="value"
                checkedField="checked"
                tooltipField="name"
                clearable
                onChange={onChange}
            />
        </div>
    );
}

export default CustomColumnYesOrNoFilter;
