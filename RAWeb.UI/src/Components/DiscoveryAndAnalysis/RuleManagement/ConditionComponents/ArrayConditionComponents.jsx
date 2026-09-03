import { useEffect, useState } from "react";
import _ from "lodash";

const ArraySimpleConditionComponent = ({ value, onChange }) => {

    const [items, setItems] = useState([]);

    useEffect(() => {
        var parsedValue = "";
        try{
            parsedValue = JSON.parse(value);
        }catch{

        }
        if(!_.isArray(parsedValue)) {
            return;
        }
        const clonedItems = parsedValue.map(item => ({
            name: item,
            checked: true,
            invalid: false,
            tooltip: item
        }));
        setItems(clonedItems);
    }, [value]);

    const onInnerChange = (args) => {
        const items = args.newValue.map(item => item.name);
        setTimeout(() => {
            onChange(JSON.stringify(_.toArray(new Set(items).values())));
        }, 0); 
    };

 const doMatch = (args) => {
        return args.list;
    };

    return (
        <div>
            <R.RichCombobox
                textField="name"
                valueField="id"
                silence={true}
                items={items}
                doMatch={doMatch}
                searchPlaceholder={RMResx.RM_FA_Discovery_ArrayConditionWatermark}
                onChange={onInnerChange}
            />
        </div>
    );
};

ArraySimpleConditionComponent.validate = (value) => {
    const parsedValue = JSON.parse(value);
    if (_.isNil(parsedValue) || _.isEmpty(parsedValue)) {
        return {
            isValidated: false,
            errorMessages: [RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]],
        };
    }

    return {
        isValidated: true,
        errorMessages: [],
    };
};


ArraySimpleConditionComponent.getDisplayText = (value) => {
    value = JSON.parse(value);
    if(_.isArray(value)){
        return `(${value.join(", ")})`;
    }
    return value;
};

ArraySimpleConditionComponent.defaultValue = JSON.stringify([]);

export { ArraySimpleConditionComponent };
