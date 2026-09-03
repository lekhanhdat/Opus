import * as Constants from "./Constants";
export const Regexs = [{id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains},
    {id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains},
    {id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths},
    {id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath},
    //{id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
    {id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot}
];

export const Matchs1 = [
    { id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains },
    { id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains },
    { id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths },
    { id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath },
    //{id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
    { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals },
    { id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot }
];

export const Condition = {
    id: 0,
    isColumn: false, //Archive the content when  All of these criteria are met. 第一个 input
    isText: true,  //第二个 input
    isDate1: false,
    isDate2: false,
    isMath1: true,  //content 等
    isMath2: false,  //kb，gb 等
    isValid: false,
    isDateValid: false,
    noDateValue: false,
    conditionId: 8,
    ruleTypeId: 40,
    currentTimeZone: RM.TimeUtil.getGlobalTimezoneInfo(),
    currentTimeZoneId: RM.TimeSettingModel.TimeZoneId,
    filterName: "",
    Value1: "",  //第二个input value
    valueUnit: 1,
    curLevelId: Constants.levels[0].id,
    // conditionTypes: this.exoRulTypes[6553601],
    conditionTypes: [],
    Matchs1: Matchs1,
    Matchs2: [],
    // currentType: this.exoRulTypes[6553601][0],
    currentType: {},
    currentMatch1: Regexs[0],
    currentMatch2: Regexs[0],
    CombineMode: 0,
    notNumber: false,
    currentDate1: null,
    currentDate2: null,
    isConflict: false,
    isConflictValue: false,
    RuleType: 1,
    columnNamePlaceholder: RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue,
    columnValuePlaceholder: RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue,
    sendWith: "110px"
};