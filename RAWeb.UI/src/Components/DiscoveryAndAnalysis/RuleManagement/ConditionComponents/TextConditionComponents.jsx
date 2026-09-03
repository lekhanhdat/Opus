
const TextSimpleConditionComponent = ({value, onChange}) => {
    return (
        <>
            <div>
                <R.Input
                    value={value}
                    placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue}
                    type="text"
                    width={"100%"}
                    onChange={(value) => onChange(value)}
                />
            </div>
        </>
    );
};

TextSimpleConditionComponent.validate = (value) => {
    if(_.isNil(value) || _.isEmpty(value)) {
        return {
            isValidated: false,
            errorMessages: [
                RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]
            ]
        };
    }

    return {
        isValidated: true
    };
};

TextSimpleConditionComponent.getDisplayText = (value) => {
    value = JSON.parse(value);
    return value.text;
}

TextSimpleConditionComponent.defaultValue = "";


const TextOnlyConditionComponent = ({value, onChange}) => {
    return (
        <>
            <div>
                <R.Input
                    value={value}
                    placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue}
                    type="text"
                    width={"100%"}
                    onChange={(value) => onChange(value)}
                />
            </div>
        </>
    );
};

TextOnlyConditionComponent.validate = (value) => {
    if(_.isNil(value) || _.isEmpty(value)) {
        return {
            isValidated: false,
            errorMessages: [
                RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]
            ]
        };
    }

    return {
        isValidated: true
    };
};

TextOnlyConditionComponent.getDisplayText = (value) => {
    return value;
}

TextOnlyConditionComponent.defaultValue = "";


const ExtraTextSimpleConditionComponent = ({value, onChange}) => {
    return (
        <>
            <div>
                <R.Input
                    value={value}
                    placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue}
                    type="text"
                    width={"100%"}
                    onChange={(value) => onChange(value)}
                />
            </div>
        </>
    );
};

ExtraTextSimpleConditionComponent.validate = (value) => {
    if(_.isNil(value) || _.isEmpty(value)) {
        return {
            isValidated: false,
            errorMessages: [
                RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]
            ]
        };
    }

    return {
        isValidated: true
    };
};

ExtraTextSimpleConditionComponent.getDisplayText = (value) => {
    return value;
}

ExtraTextSimpleConditionComponent.extraDefaultValue = "";

export {
    TextSimpleConditionComponent,
    TextOnlyConditionComponent,
    ExtraTextSimpleConditionComponent
};