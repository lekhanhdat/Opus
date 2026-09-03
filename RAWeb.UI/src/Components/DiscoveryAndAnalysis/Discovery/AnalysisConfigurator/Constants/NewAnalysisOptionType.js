const NewAnalysisOptionType = {
    None: 0,
    New: 1,
    Append: 2
};

const NewAnalysisOptionTypeI18ns = new Map([
    [NewAnalysisOptionType.New, RMResx.RM_FA_Discovery_NewlyPanel_NewOpt],
    [NewAnalysisOptionType.Append, RMResx.RM_FA_Discovery_NewlyPanel_AppendOpt],
]);

export {
    NewAnalysisOptionType,
    NewAnalysisOptionTypeI18ns
};