import _ from "lodash";

const DateUnitType = {
    None: 0,
    Day: 1,
    Weeks: 2,
    Months: 3,
    Years: 4,
};

const DateTimeUnitOptions = [
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_Days,
        value: DateUnitType.Day,
    },
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_Weeks,
        value: DateUnitType.Weeks,
    },
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_Months,
        value: DateUnitType.Months,
    },
    {
        name: RMResx.RM_JS_RDM_CreateRule_Unit_Years,
        value: DateUnitType.Years,
    },
];

const DateTimeOnlyOneConditionComponent = ({ value, onChange }) => {
    return (
        <>
            <div>
                <R.Datepicker
                    selectedDate={value === '{}' ? null : new Date(value)}
                    data-part="vtWidget"
                    disabled={false}
                    width={"100%"}
                    dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                    hasTimePicker={true}
                    onChange={(args) =>
                        onChange(RM.TimeUtil.getCommonDateStr(
                            args.newValue
                        ),)
                    }
                />
            </div>
        </>
    );
};

DateTimeOnlyOneConditionComponent.validate = (value) => {
    if(value === '{}') {
        return {
            isValidated: false,
            errorMessages: [RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime]
        };
    }

    return {
        isValidated: true
    };
};

DateTimeOnlyOneConditionComponent.getDisplayText = (value) => {
    return value;
}

DateTimeOnlyOneConditionComponent.defaultValue = JSON.stringify({});

const DateTimeUnitConditionComponent = ({ value, onChange }) => {

    const getDateTimeUnitOptions = (value) => {
        let parsedValue = "";
        try{
            parsedValue = JSON.parse(value);
        }catch{

        }
        return _.cloneDeep(DateTimeUnitOptions).map((item) => {
            item.checked = item.value === parsedValue.unitType;
            return item;
        });        
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
                    value={value === "" ? value : JSON.parse(value).unit}
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
                    items={getDateTimeUnitOptions(value)}
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

DateTimeUnitConditionComponent.validate = (value) => {
    const parsedValue = JSON.parse(value);
    if(_.isNil(parsedValue.unit) || parsedValue.unit === '') {
        return {
            isValidated: false,
            errorMessages: [RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]]
        };
    }

    if (parsedValue.unit < 0) {
        return {
            isValidated: false,
            errorMessages: [RMResx.RM_FA_Discovery_NumberInvalid]
        };
    }

    return {
        isValidated: true
    };
};

DateTimeUnitConditionComponent.getDisplayText = (value) => {
    value = JSON.parse(value);
    const unitTypeName = DateTimeUnitOptions.find(item => item.value === value.unitType).name;
    return `${value.unit} ${unitTypeName}`;
};

DateTimeUnitConditionComponent.defaultValue = JSON.stringify({
    unitType: DateUnitType.Day
});

export { DateTimeOnlyOneConditionComponent, DateTimeUnitConditionComponent };
