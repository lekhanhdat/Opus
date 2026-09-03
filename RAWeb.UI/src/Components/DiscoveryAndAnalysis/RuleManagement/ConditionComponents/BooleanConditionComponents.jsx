const BooleanOptions = [
    {
        name: RMResx.RM_JS_Common_Yes,
        value: "true",
    },
    {
        name: RMResx.RM_JS_Common_No,
        value: "false",
    },
];

const BooleanSimpleConditionComponent = ({ value, onChange }) => {
    const getBooleanOptions = (value) => {
        return _.cloneDeep(BooleanOptions).map((item) => {
            item.checked = item.value === value;
            return item;
        });
    };

    return (
        <>
            <div>
                <R.Combobox
                    width={"100%"}
                    popupMaxHeight={400}
                    searchable={false}
                    items={getBooleanOptions(value)}
                    textField="name"
                    valueField="value"
                    onChange={(args) =>
                        onChange(args.newValue.value)
                    }
                />
            </div>
        </>
    );
};

BooleanSimpleConditionComponent.validate = (value) => {
    return {
        isValidated: true,
        errorMessages: [],
    };
};

BooleanSimpleConditionComponent.getDisplayText = (value) => {
    return BooleanOptions.find((item) => item.value === value)
        .name;
};

BooleanSimpleConditionComponent.defaultValue = "true";

export { BooleanSimpleConditionComponent };
