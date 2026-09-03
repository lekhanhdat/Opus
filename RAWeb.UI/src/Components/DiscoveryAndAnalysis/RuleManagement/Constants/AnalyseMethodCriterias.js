import AnalyseMethodConstants from "./AnalyseMethodConstants";
import { ConditionCategory, ConditionType } from "./ConditionInfoes";
import { CriteriaConstants } from ".";

const AnalyseDocumentCriterias = [
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.Name),
        value: CriteriaConstants.document.type.Name,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentFolder),
        value: CriteriaConstants.document.type.ParentFolder,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.CreatedTime),
        value: CriteriaConstants.document.type.CreatedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ModifiedTime),
        value: CriteriaConstants.document.type.ModifiedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentType),
        value: CriteriaConstants.document.type.DocumentType,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.In,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.NotIn,
            },
            {
                category: ConditionCategory.BooleanLogic,
                type: ConditionType.BooleanConditionType.IsEmpty,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentSize),
        value: CriteriaConstants.document.type.DocumentSize,
        conditions: [
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentLibraryText),
        value: CriteriaConstants.document.type.ParentLibraryText,
        conditions: [
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Contains,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotContain,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Matches,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotMatch,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Equals,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotEqual,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentLibraryNumber),
        value: CriteriaConstants.document.type.ParentLibraryNumber,
        conditions: [
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentLibraryBoolean),
        value: CriteriaConstants.document.type.ParentLibraryBoolean,
        conditions: [
            {
                category: ConditionCategory.BooleanExtraInput,
                type: ConditionType.BooleanExtraInputConditionType.Equals,
            },
            // {
            //     category: ConditionCategory.BooleanExtraInput,
            //     type: ConditionType.BooleanExtraInputConditionType.DoesNotEqual,
            // },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentLibraryDateTime),
        value: CriteriaConstants.document.type.ParentLibraryDateTime,
        conditions: [
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.Before,
            },
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.PropertyBagText),
        value: CriteriaConstants.document.type.PropertyBagText,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Contains,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotContain,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Matches,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotMatch,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Equals,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotEqual,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.PropertyBagNumber),
        value: CriteriaConstants.document.type.PropertyBagNumber,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.PropertyBagBoolean),
        value: CriteriaConstants.document.type.PropertyBagBoolean,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.BooleanExtraInput,
                type: ConditionType.BooleanExtraInputConditionType.Equals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.PropertyBagDateTime),
        value: CriteriaConstants.document.type.PropertyBagDateTime,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.Before,
            },
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentSiteCollectionText),
        value: CriteriaConstants.document.type.ParentSiteCollectionText,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Contains,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotContain,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Matches,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotMatch,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.Equals,
            },
            {
                category: ConditionCategory.TextExtraInput,
                type: ConditionType.TextExtraInputConditionType.DoesNotEqual,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentSiteCollectionNumber),
        value: CriteriaConstants.document.type.ParentSiteCollectionNumber,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.NumberExtraInput,
                type: ConditionType.NumberExtraInputConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentSiteCollectionBoolean),
        value: CriteriaConstants.document.type.ParentSiteCollectionBoolean,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.BooleanExtraInput,
                type: ConditionType.BooleanExtraInputConditionType.Equals,
            },
            // {
            //     category: ConditionCategory.BooleanExtraInput,
            //     type: ConditionType.BooleanExtraInputConditionType.DoesNotEqual,
            // },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentSiteCollectionDateTime),
        value: CriteriaConstants.document.type.ParentSiteCollectionDateTime,
        tooltip: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNotSupported,
        conditions: [
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.Before,
            },
            {
                category: ConditionCategory.DateTimeExtraInput,
                type: ConditionType.DateTimeExtraInputConditionType.OlderThan,
            },
        ],
    },
];

const AnalyseGoogleDocumentCriterias = [
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.Name),
        value: CriteriaConstants.document.type.Name,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ParentFolder),
        value: CriteriaConstants.document.type.ParentFolder,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.CreatedTime),
        value: CriteriaConstants.document.type.CreatedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ModifiedTime),
        value: CriteriaConstants.document.type.ModifiedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentType),
        value: CriteriaConstants.document.type.DocumentType,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.In,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.NotIn,
            },
            {
                category: ConditionCategory.BooleanLogic,
                type: ConditionType.BooleanConditionType.IsEmpty,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentSize),
        value: CriteriaConstants.document.type.DocumentSize,
        conditions: [
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.GoogleLabel),
        value: CriteriaConstants.document.type.GoogleLabel,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.In,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.NotIn,
            },
            {
                category: ConditionCategory.BooleanLogic,
                type: ConditionType.BooleanConditionType.IsEmpty,
            },
        ],
    },
]

const AnalyseVersionCriterias = [
    {
        name: CriteriaConstants.version.i18n.get(CriteriaConstants.version.type.KeepLastVersions),
        value: CriteriaConstants.version.type.KeepLastVersions,
        conditions: [
            {
                category: ConditionCategory.Version,
                type: ConditionType.VersionConditionType.MajorAndMinor,
            },
            {
                category: ConditionCategory.Version,
                type: ConditionType.VersionConditionType.MajorAndNoMinor,
            },
            {
                category: ConditionCategory.Version,
                type: ConditionType.VersionConditionType
                    .MinorVersionOfEachMajor,
            },
            {
                category: ConditionCategory.Version,
                type: ConditionType.VersionConditionType
                    .MinorVersionsOfLatestMajor,
            },
        ],
    },
    {
        name: CriteriaConstants.version.i18n.get(CriteriaConstants.version.type.ModifiedTime),
        value: CriteriaConstants.version.type.ModifiedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.version.i18n.get(CriteriaConstants.version.type.DocumentType),
        value: CriteriaConstants.version.type.DocumentType,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.In,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.NotIn,
            },
            {
                category: ConditionCategory.BooleanLogic,
                type: ConditionType.BooleanConditionType.IsEmpty,
            },
        ],
    },
    {
        name: CriteriaConstants.version.i18n.get(CriteriaConstants.version.type.DocumentSize),
        value: CriteriaConstants.version.type.DocumentSize,
        conditions: [
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.LessThanEquals,
            },
        ],
    },
];

const AnalyseDuplicateDocumentCriterias = [
    {
        name: CriteriaConstants.duplicate.i18n.get(CriteriaConstants.duplicate.type.Duplicate),
        value: CriteriaConstants.duplicate.type.Duplicate,
        conditions: [
            {
                category: ConditionCategory.Duplicate,
                type: ConditionType.DuplicateConditionType.InField,
            },
        ],
    },
];

const AnalyseFSDocumentCriterias = [
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.Name),
        value: CriteriaConstants.document.type.Name,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentSize),
        value: CriteriaConstants.document.type.DocumentSize,
        conditions: [
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.GreaterThanEquals,
            },
            {
                category: ConditionCategory.FileSize,
                type: ConditionType.FileSizeConditionType.LessThanEquals,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ModifiedTime),
        value: CriteriaConstants.document.type.ModifiedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.CreatedTime),
        value: CriteriaConstants.document.type.CreatedTime,
        conditions: [
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.Before,
            },
            {
                category: ConditionCategory.DateTime,
                type: ConditionType.DateTimeConditionType.OlderThan,
            },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentType),
        value: CriteriaConstants.document.type.DocumentType,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.In,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.NotIn,
            },
            {
                category: ConditionCategory.BooleanLogic,
                type: ConditionType.BooleanConditionType.IsEmpty,
            },
        ],
    },
    {
        name: RMResx.RM_FA_Discovery_RuleType_FolderName,
        value: CriteriaConstants.document.type.ParentFolder,
        conditions: [
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextMatchIn,
            },
            {
                category: ConditionCategory.Array,
                type: ConditionType.ArrayConditionType.TextNotMatchIn,
            },
        ],
    },
]

const AnalyseAVADocumentCriterias = [
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.Name),
        value: CriteriaConstants.document.type.Name,
        conditions: [
            { category: ConditionCategory.Array, type: ConditionType.ArrayConditionType.TextMatchIn },
            { category: ConditionCategory.Array, type: ConditionType.ArrayConditionType.TextNotMatchIn },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.CreatedTime),
        value: CriteriaConstants.document.type.CreatedTime,
        conditions: [
            { category: ConditionCategory.DateTime, type: ConditionType.DateTimeConditionType.Before },
            { category: ConditionCategory.DateTime, type: ConditionType.DateTimeConditionType.OlderThan },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.ModifiedTime),
        value: CriteriaConstants.document.type.ModifiedTime,
        conditions: [
            { category: ConditionCategory.DateTime, type: ConditionType.DateTimeConditionType.Before },
            { category: ConditionCategory.DateTime, type: ConditionType.DateTimeConditionType.OlderThan },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentType),
        value: CriteriaConstants.document.type.DocumentType,
        conditions: [
            { category: ConditionCategory.Array, type: ConditionType.ArrayConditionType.In },
            { category: ConditionCategory.Array, type: ConditionType.ArrayConditionType.NotIn },
            { category: ConditionCategory.BooleanLogic, type: ConditionType.BooleanConditionType.IsEmpty },
        ],
    },
    {
        name: CriteriaConstants.document.i18n.get(CriteriaConstants.document.type.DocumentSize),
        value: CriteriaConstants.document.type.DocumentSize,
        conditions: [
            { category: ConditionCategory.FileSize, type: ConditionType.FileSizeConditionType.GreaterThanEquals },
            { category: ConditionCategory.FileSize, type: ConditionType.FileSizeConditionType.LessThanEquals },
        ],
    },
];

const AnalyseMethodCriterias = new Map([
    [AnalyseMethodConstants.type.Document, AnalyseDocumentCriterias],
    [AnalyseMethodConstants.type.Version, AnalyseVersionCriterias],
    [AnalyseMethodConstants.type.DuplicateDocument, AnalyseDuplicateDocumentCriterias],
    [AnalyseMethodConstants.type.GoogleDocument, AnalyseGoogleDocumentCriterias],
    [AnalyseMethodConstants.type.FSDocument, AnalyseFSDocumentCriterias],
    [AnalyseMethodConstants.type.AVADocument, AnalyseAVADocumentCriterias],
]);

export default AnalyseMethodCriterias;