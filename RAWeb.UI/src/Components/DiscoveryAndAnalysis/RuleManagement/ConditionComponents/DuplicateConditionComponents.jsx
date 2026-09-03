import _ from "lodash";

const DuplicateField = {
    None: 0,
    Name: 1,
    Size: 2,
    Editor: 3,
};

const DuplicateFieldOptions = [
    {
        name: RMResx.RM_FA_Discovery_DuplicateOption_Name,
        value: DuplicateField.Name,
    },
    {
        name: RMResx.RM_FA_Discovery_DuplicateOption_Size,
        value: DuplicateField.Size,
    },
    // {
    //     name: RMResx.RM_FA_Discovery_DuplicateOption_LastModifiedBy,
    //     value: DuplicateField.Editor,
    // },
];

const DuplicateFieldConditionComponent = ({ value, onChange }) => {
    const getDuplicateFieldOptions = (value) => {
        let parsedValue = [];
        try {
            parsedValue = JSON.parse(value);
        } catch {}

        if (_.isNil(parsedValue) || !_.isArray(parsedValue)) {
            return [];
        }
        return _.cloneDeep(DuplicateFieldOptions).map((item) => {
            item.checked =
                parsedValue.findIndex((field) => field === item.value) > -1;
            return item;
        });
    };

    const onInnerChange = (args) => {
        const fields = args.newValue.map((item) => item.value);
        onChange(JSON.stringify(fields));
    };

    return (
        <div>
            <R.RichCombobox
                height={34}
                width={"100%"}
                searchable={false}
                checkedField="checked"
                textField="name"
                valueField="value"
                hasFilter={true}
                required={true}
                items={getDuplicateFieldOptions(value)} //JSON.parse(value)
                noneText="Manage Columns"
                onChange={onInnerChange}
                disabled={true}
            />
        </div>
    );
};

DuplicateFieldConditionComponent.validate = (value) => {
    return {
        isValidated: true,
        errorMessages: [],
    };
};

DuplicateFieldConditionComponent.getDisplayText = (value) => {
    value = JSON.parse(value);
    const fieldNames = value.map(
        (item) =>
            DuplicateFieldOptions.find((field) => field.value === item).name
    );
    return fieldNames.join(", ");
};

DuplicateFieldConditionComponent.defaultValue = JSON.stringify([
    DuplicateField.Name,
    DuplicateField.Size,
]);

export { DuplicateFieldConditionComponent };
