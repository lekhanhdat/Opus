import _ from "lodash";

const SizeUnitType = {
    None: 0,
    KB: 1,
    MB: 2,
    GB: 3,
};

const SizeUnitOptions = [
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_KB,
        value: SizeUnitType.KB,
    },
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_MB,
        value: SizeUnitType.MB,
    },
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_GB,
        value: SizeUnitType.GB,
    },
];

const NumberSimpleConditionComponent = ({ value, onChange }) => {
    return (
        <div>
            <R.Input
                value={value}
                min={0}
                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue}
                type="number"
                width={"100%"}
                onChange={(value) => onChange(value)}
            />
        </div>
    );
};

NumberSimpleConditionComponent.validate = (value) => {
    if (_.isNil(value) ||  value === '') {
        return {
            isValidated: false,
            errorMessages: [RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]],
        };
    }

    if (value < 1) {
        return {
            isValidated: false,
            errorMessages: [RMResx.RM_FA_Discovery_NumberInvalid],
        };
    }

    return {
        isValidated: true,
        errorMessages: [],
    };
};

NumberSimpleConditionComponent.getDisplayText = (value) => {
    return value;
}

NumberSimpleConditionComponent.defaultValue = "";

const NumberSizeUnitConditionComponent = ({ value, onChange }) => {

    const getSizeUnitOptions = (value) => {
        var parsedValue = {};
        try{
            parsedValue = JSON.parse(value);
        }catch{

        }
        return _.cloneDeep(SizeUnitOptions).map((item) => {
            item.checked = item.value === parsedValue.unitType;
            return item;
        });
    };

    const getSizeUnit = (value) => {
        var parsedValue = {};
        try{
            parsedValue = JSON.parse(value);
        }catch{

        }
        return parsedValue.unit;
    };

    const onInnerChange = (field, newValue) => {
        const parsedValue = JSON.parse(value);
        parsedValue[field] = newValue;
        onChange(JSON.stringify(parsedValue));
    };

    return (
        <>
            <div>
                <R.Input
                    value={getSizeUnit(value)}
                    min={0}
                    placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue}
                    type="number"
                    width={"100%"}
                    onChange={(newValue) => {
                        onInnerChange("unit", newValue);
                    }}
                />
            </div>
            <div>
                <R.Combobox
                    width={"100%"}
                    popupMaxHeight={400}
                    searchable={false}
                    items={getSizeUnitOptions(value)}
                    textField="name"
                    valueField="value"
                    onChange={(args) => {
                        onInnerChange("unitType", args.newValue.value);
                    }}
                />
            </div>
        </>
    );
};

NumberSizeUnitConditionComponent.validate = (value) => {
    const parsedValue = JSON.parse(value);
    if (_.isNil(parsedValue.unit) ||  parsedValue.unit === '') {
        return {
            isValidated: false,
            errorMessages: [RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]],
        };
    }

    if (parsedValue.unit < 0) {
        return {
            isValidated: false,
            errorMessages: [
                [RMResx.RM_FA_Discovery_NumberInvalid],
            ],
        };
    }

    return {
        isValidated: true,
        errorMessages: [],
    };
};

NumberSizeUnitConditionComponent.getDisplayText = (value) => {
    value = JSON.parse(value);
    const unitTypeName = SizeUnitOptions.find(item => item.value === value.unitType).name;
    return `${value.unit} ${unitTypeName}`;
};

NumberSizeUnitConditionComponent.defaultValue = JSON.stringify({
    unitType: SizeUnitType.KB,
});

export { NumberSimpleConditionComponent, NumberSizeUnitConditionComponent };
