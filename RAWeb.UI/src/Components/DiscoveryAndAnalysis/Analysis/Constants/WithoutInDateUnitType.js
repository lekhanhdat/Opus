const WithoutInDateUnitType = {
    None: 0,
    Month: 1,
    Year: 2,
};

const WithoutInDateUnitI18ns = new Map([
    [WithoutInDateUnitType.Month, RMResx.RM_JS_RDM_CreateRule_Unit_Months],
    [WithoutInDateUnitType.Year, RMResx.RM_JS_RDM_CreateRule_Unit_Years],
]);

const WithoutInDateUnitConstants = {
    type: WithoutInDateUnitType,
    i18n: WithoutInDateUnitI18ns
};

export default WithoutInDateUnitConstants;