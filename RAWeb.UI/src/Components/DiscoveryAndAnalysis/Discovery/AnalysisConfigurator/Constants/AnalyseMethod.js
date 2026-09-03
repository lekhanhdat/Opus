const AnalyseMethod = {
    None: 0,
    Document: 1,
    Version: 2,
    DuplicateDocument: 3,
};

const AnalyseMethodI18ns = new Map([
    [AnalyseMethod.Document, RMResx.RM_FA_Discovery_RuleMethod_Document],
    [AnalyseMethod.Version, RMResx.RM_FA_Discovery_RuleMethod_Version],
    [AnalyseMethod.DuplicateDocument, RMResx.RM_FA_Discovery_RuleMethod_Duplicate],
]);

export {
    AnalyseMethod,
    AnalyseMethodI18ns
}