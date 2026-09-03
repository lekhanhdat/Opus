const AnalyseMethod = {
    None: 0,
    Document: 1,
    Version: 2,
    DuplicateDocument: 3,
    GoogleDocument: 4,
    FSDocument: 5,
    AVADocument: 6
};

const AnalyseMethodI18ns = new Map([
    [AnalyseMethod.Document, RMResx.RM_FA_Discovery_RuleMethod_Document],
    [AnalyseMethod.Version, RMResx.RM_FA_Discovery_RuleMethod_Version],
    [AnalyseMethod.DuplicateDocument, RMResx.RM_FA_Discovery_RuleMethod_Duplicate],
    [AnalyseMethod.GoogleDocument, RMResx.RM_FA_Discovery_RuleMethod_GoogleDocument],
    [AnalyseMethod.FSDocument, RMResx.RM_FA_Discovery_RuleMethod_FSDocument],
]);

const AnalyseMethodConstants = {
    type: AnalyseMethod,
    i18n: AnalyseMethodI18ns
};

export default AnalyseMethodConstants;