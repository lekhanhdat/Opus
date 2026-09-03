import { ArraySimpleConditionComponent } from "../ConditionComponents/ArrayConditionComponents";
import { BooleanSimpleConditionComponent } from "../ConditionComponents/BooleanConditionComponents";
import { DateTimeOnlyOneConditionComponent, DateTimeUnitConditionComponent } from "../ConditionComponents/DateTimeConditionComponents";
import { DuplicateFieldConditionComponent } from "../ConditionComponents/DuplicateConditionComponents";
import { NumberSimpleConditionComponent, NumberSizeUnitConditionComponent } from "../ConditionComponents/NumberConditionComponents";
import { ExtraTextSimpleConditionComponent, TextOnlyConditionComponent, TextSimpleConditionComponent } from "../ConditionComponents/TextConditionComponents";

const ConditionCategory = {
    None: 0,
    Text: 1,
    Number: 2,
    Date: 3,
    DateTime: 4,
    Array: 5,
    BooleanLogic: 6,
    FileSize: 7,
    Duplicate: 8,
    Version: 9,
    TextExtraInput: 10,
    NumberExtraInput: 11,
    BooleanExtraInput: 12,
    DateTimeExtraInput: 13,
}

const TextConditionType = {
    None: 0,
    Contains: 1,
    DoesNotContain: 2,
    Equals: 3,
    DoesNotEqual: 4,
}

const TextExtraInputConditionType = {
    None: 0,
    Contains: 1,
    DoesNotContain: 2,
    Matches: 3,
    DoesNotMatch: 4,
    Equals: 5,
    DoesNotEqual: 6,
}

const TextConditionI18n = new Map([
    [TextConditionType.Contains, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains],
    [TextConditionType.DoesNotContain, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains],
    [TextConditionType.Equals, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals],
    [TextConditionType.DoesNotEqual, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot],
]);

const TextExtraInputConditionI18n = new Map([
    [TextExtraInputConditionType.Contains, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains],
    [TextExtraInputConditionType.DoesNotContain, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains],
    [TextExtraInputConditionType.Matches, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths],
    [TextExtraInputConditionType.DoesNotMatch, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath],
    [TextExtraInputConditionType.Equals, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals],
    [TextExtraInputConditionType.DoesNotEqual, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot],
]);

const TextConditionComponentInfo = new Map([
    [TextConditionType.Contains, {
        layout: "reco-condition-layout-c3",
        component: TextSimpleConditionComponent
    }],
    [TextConditionType.DoesNotContain, {
        layout: "reco-condition-layout-c3",
        component: TextSimpleConditionComponent
    }],
    [TextConditionType.Equals, {
        layout: "reco-condition-layout-c3",
        component: TextSimpleConditionComponent
    }],
    [TextConditionType.DoesNotEqual, {
        layout: "reco-condition-layout-c3",
        component: TextSimpleConditionComponent
    }],
])

const TextExtraInputConditionComponentInfo = new Map([
    [TextExtraInputConditionType.Contains, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
    [TextExtraInputConditionType.DoesNotContain, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
    [TextExtraInputConditionType.Matches, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
    [TextExtraInputConditionType.DoesNotMatch, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
    [TextExtraInputConditionType.Equals, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
    [TextExtraInputConditionType.DoesNotEqual, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: TextOnlyConditionComponent
    }],
])

const NumberConditionType = {
    None: 0,
    LessThanEquals: 1,
    GreaterThanEquals: 2,
    LessThan: 3,
    GreaterThan: 4,
    Equals: 5
}

const NumberExtraInputConditionType = {
    None: 0,
    LessThanEquals: 1,
    GreaterThanEquals: 2,
}

const NumberConditionI18n = new Map([
    [NumberConditionType.LessThanEquals, "<="],
    [NumberConditionType.GreaterThanEquals, ">="],
    [NumberConditionType.LessThan, "<"],
    [NumberConditionType.GreaterThan, ">"],
    [NumberConditionType.Equals, "="],
]);

const NumberExtraInputConditionI18n = new Map([
    [NumberExtraInputConditionType.LessThanEquals, "<="],
    [NumberExtraInputConditionType.GreaterThanEquals, ">="],
]);

const NumberConditionComponentInfo = new Map([
    [NumberConditionType.LessThanEquals, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [NumberConditionType.GreaterThanEquals, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [NumberConditionType.LessThan, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [NumberConditionType.GreaterThan, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [NumberConditionType.Equals, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }]
])

const NumberExtraInputConditionComponentInfo = new Map([
    [NumberExtraInputConditionType.LessThanEquals, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: NumberSimpleConditionComponent
    }],
    [NumberExtraInputConditionType.GreaterThanEquals, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: NumberSimpleConditionComponent
    }],
])

const DateTimeConditionType = {
    None: 0,
    Before: 1,
    OlderThan: 2,
    FromTo: 3,
}

const DateTimeExtraInputConditionType = {
    None: 0,
    Before: 1,
    OlderThan: 2,
}

const DateTimeConditionI18n = new Map([
    [DateTimeConditionType.Before, RMResx.RM_JS_RDM_CreateRule_DateOption_Before],
    [DateTimeConditionType.OlderThan, RMResx.RM_JS_RDM_CreateRule_DateOption_Older],
    [DateTimeConditionType.FromTo, RMResx.RM_JS_RDM_CreateRule_DateOption_FromTo],
]);

const DateTimeExtraInputConditionI18n = new Map([
    [DateTimeExtraInputConditionType.Before, RMResx.RM_JS_RDM_CreateRule_DateOption_Before],
    [DateTimeExtraInputConditionType.OlderThan, RMResx.RM_JS_RDM_CreateRule_DateOption_Older],
]);

const DateTimeConditionComponentInfo = new Map([
    [DateTimeConditionType.Before, {
        layout: "reco-condition-layout-c3",
        component: DateTimeOnlyOneConditionComponent
    }],
    [DateTimeConditionType.OlderThan, {
        layout: "reco-condition-layout-c4",
        component: DateTimeUnitConditionComponent
    }],
    [DateTimeConditionType.FromTo, {
        layout: "reco-condition-layout-c3",
        component: DateTimeOnlyOneConditionComponent
    }]
])

const DateTimeExtraInputConditionComponentInfo = new Map([
    [DateTimeExtraInputConditionType.Before, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: DateTimeOnlyOneConditionComponent
    }],
    [DateTimeExtraInputConditionType.OlderThan, {
        layout: "reco-condition-layout-c5-1",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: DateTimeUnitConditionComponent
    }],
])

const ArrayConditionType = {
    None: 0,
    In: 1,
    NotIn: 2,
    TextMatchIn : 3,
    TextNotMatchIn : 4,
}

const ArrayConditionI18n = new Map([
    [ArrayConditionType.In, RMResx.RM_FA_Discovery_RuleCondition_In],
    [ArrayConditionType.NotIn, RMResx.RM_FA_Discovery_RuleCondition_NotIn],
    [ArrayConditionType.TextMatchIn, RMResx.RM_FA_Discovery_RuleCondition_TextMatchIn],
    [ArrayConditionType.TextNotMatchIn, RMResx.RM_FA_Discovery_RuleCondition_TextNotMatchIn],
]);

const ArrayConditionComponentInfo = new Map([
    [ArrayConditionType.In, {
        layout: "reco-condition-layout-c2-r2-lc1",
        component: ArraySimpleConditionComponent
    }],
    [ArrayConditionType.NotIn, {
        layout: "reco-condition-layout-c2-r2-lc1",
        component: ArraySimpleConditionComponent
    }],
    [ArrayConditionType.TextMatchIn, {
        layout: "reco-condition-layout-c2-r2-lc1",
        component: ArraySimpleConditionComponent
    }],
    [ArrayConditionType.TextNotMatchIn, {
        layout: "reco-condition-layout-c2-r2-lc1",
        component: ArraySimpleConditionComponent
    }],
]);

const BooleanConditionType = {
    None: 0,
    IsEmpty: 1
}

const BooleanExtraInputConditionType = {
    None: 0,
    Equals: 1,
    DoesNotEqual: 2,
}

const BooleanConditionI18n = new Map([
    [BooleanConditionType.IsEmpty, RMResx.RM_FA_Discovery_RuleCondition_IsEmpty],
]);

const BooleanExtraInputConditionI18n = new Map([
    [BooleanExtraInputConditionType.Equals, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals],
    [BooleanExtraInputConditionType.DoesNotEqual, RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot],
]);

const BooleanConditionComponentInfo = new Map([
    [BooleanConditionType.IsEmpty, {
        layout: "reco-condition-layout-c3",
        component: BooleanSimpleConditionComponent
    }]
])

const BooleanExtraInputConditionComponentInfo = new Map([
    [BooleanExtraInputConditionType.Equals, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: BooleanSimpleConditionComponent
    }],
    [BooleanExtraInputConditionType.DoesNotEqual, {
        layout: "reco-condition-layout-c5",
        extraComponent: ExtraTextSimpleConditionComponent,
        component: BooleanSimpleConditionComponent
    }]
])

const FileSizeConditionType = {
    None: 0,
    LessThanEquals: 1,
    GreaterThanEquals: 2,
    LessThan: 3,
    GreaterThan: 4,
    Equals: 5
}

const FileSizeConditionI18n = new Map([
    [FileSizeConditionType.LessThanEquals, "<="],
    [FileSizeConditionType.GreaterThanEquals, ">="],
    [FileSizeConditionType.LessThan, "<"],
    [FileSizeConditionType.GreaterThan, ">"],
    [FileSizeConditionType.Equals, "="],
]);

const FileSizeConditionComponentInfo = new Map([
    [FileSizeConditionType.LessThanEquals, {
        layout: "reco-condition-layout-c4",
        component: NumberSizeUnitConditionComponent
    }],
    [FileSizeConditionType.GreaterThanEquals, {
        layout: "reco-condition-layout-c4",
        component: NumberSizeUnitConditionComponent
    }],
    [FileSizeConditionType.LessThan, {
        layout: "reco-condition-layout-c4",
        component: NumberSizeUnitConditionComponent
    }],
    [FileSizeConditionType.GreaterThan, {
        layout: "reco-condition-layout-c4",
        component: NumberSizeUnitConditionComponent
    }],
    [FileSizeConditionType.Equals, {
        layout: "reco-condition-layout-c4",
        component: NumberSizeUnitConditionComponent
    }]
])

const DuplicateConditionType = {
    None: 0,
    InField: 1,
}

const DuplicateConditionI18n = new Map([
    [DuplicateConditionType.InField, RMResx.RM_FA_Discovery_RuleCondition_In],
]);

const DuplicateConditionComponentInfo = new Map([
    [DuplicateConditionType.InField, {
        layout: "reco-condition-layout-c3",
        component: DuplicateFieldConditionComponent
    }],
])

const VersionConditionType = {
    None: 0,
    MajorAndMinor: 1,
    MajorAndNoMinor: 2,
    MinorVersionOfEachMajor: 3,
    MinorVersionsOfLatestMajor: 4
}

const VersionConditionI18n = new Map([
    [VersionConditionType.MajorAndMinor, RMResx.RM_JS_RDM_CreateRule_KeepVersion_MajorAndMinor],
    [VersionConditionType.MajorAndNoMinor, RMResx.RM_JS_RDM_CreateRule_KeepVersion_MajorNoMinor],
    [VersionConditionType.MinorVersionOfEachMajor, RMResx.RM_JS_RDM_CreateRule_KeepVersion_MinorEachMajor],
    [VersionConditionType.MinorVersionsOfLatestMajor, RMResx.RM_JS_RDM_CreateRule_KeepVersion_MinorLatestMajor],
]);

const VersionConditionComponentInfo = new Map([
    [VersionConditionType.MajorAndMinor, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [VersionConditionType.MajorAndNoMinor, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [VersionConditionType.MinorVersionOfEachMajor, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }],
    [VersionConditionType.MinorVersionsOfLatestMajor, {
        layout: "reco-condition-layout-c3",
        component: NumberSimpleConditionComponent
    }]
])

const ConditionType = {
    TextConditionType,
    TextExtraInputConditionType,
    NumberConditionType,
    NumberExtraInputConditionType,
    DateTimeConditionType,
    DateTimeExtraInputConditionType,
    ArrayConditionType,
    BooleanConditionType,
    BooleanExtraInputConditionType,
    FileSizeConditionType,
    DuplicateConditionType,
    VersionConditionType,
}

const ConditionI18n = {
    TextConditionI18n,
    TextExtraInputConditionI18n,
    NumberConditionI18n,
    NumberExtraInputConditionI18n,
    DateTimeConditionI18n,
    DateTimeExtraInputConditionI18n,
    ArrayConditionI18n,
    BooleanConditionI18n,
    BooleanExtraInputConditionI18n,
    FileSizeConditionI18n,
    DuplicateConditionI18n,
    VersionConditionI18n,
}

const ConditionComponentInfo = {
    TextConditionComponentInfo,
    TextExtraInputConditionComponentInfo,
    NumberConditionComponentInfo,
    NumberExtraInputConditionComponentInfo,
    DateTimeConditionComponentInfo,
    DateTimeExtraInputConditionComponentInfo,
    ArrayConditionComponentInfo,
    BooleanConditionComponentInfo,
    BooleanExtraInputConditionComponentInfo,
    FileSizeConditionComponentInfo,
    DuplicateConditionComponentInfo,
    VersionConditionComponentInfo
}

const ConditionCategoryInfo = new Map([
    [ConditionCategory.Text, {
        type: TextConditionType,
        i18n: TextConditionI18n,
        componentInfo: TextConditionComponentInfo
    }],
    [ConditionCategory.TextExtraInput, {
        type: TextExtraInputConditionType,
        i18n: TextExtraInputConditionI18n,
        componentInfo: TextExtraInputConditionComponentInfo
    }],
    [ConditionCategory.Number, {
        type: NumberConditionType,
        i18n: NumberConditionI18n,
        componentInfo: NumberConditionComponentInfo
    }],
    [ConditionCategory.NumberExtraInput, {
        type: NumberExtraInputConditionType,
        i18n: NumberExtraInputConditionI18n,
        componentInfo: NumberExtraInputConditionComponentInfo
    }],
    [ConditionCategory.DateTime, {
        type: DateTimeConditionType,
        i18n: DateTimeConditionI18n,
        componentInfo: DateTimeConditionComponentInfo
    }],
    [ConditionCategory.DateTimeExtraInput, {
        type: DateTimeExtraInputConditionType,
        i18n: DateTimeExtraInputConditionI18n,
        componentInfo: DateTimeExtraInputConditionComponentInfo
    }],
    [ConditionCategory.Array, {
        type: ArrayConditionType,
        i18n: ArrayConditionI18n,
        componentInfo: ArrayConditionComponentInfo
    }],
    [ConditionCategory.BooleanLogic, {
        type: BooleanConditionType,
        i18n: BooleanConditionI18n,
        componentInfo: BooleanConditionComponentInfo
    }],
    [ConditionCategory.BooleanExtraInput, {
        type: BooleanExtraInputConditionType,
        i18n: BooleanExtraInputConditionI18n,
        componentInfo: BooleanExtraInputConditionComponentInfo
    }],
    [ConditionCategory.FileSize, {
        type: FileSizeConditionType,
        i18n: FileSizeConditionI18n,
        componentInfo: FileSizeConditionComponentInfo
    }],
    [ConditionCategory.Duplicate, {
        type: DuplicateConditionType,
        i18n: DuplicateConditionI18n,
        componentInfo: DuplicateConditionComponentInfo
    }],
    [ConditionCategory.Version, {
        type: VersionConditionType,
        i18n: VersionConditionI18n,
        componentInfo: VersionConditionComponentInfo
    }],
])

export {
    ConditionCategory,
    ConditionType,
    ConditionI18n,
    ConditionComponentInfo,
    ConditionCategoryInfo
}