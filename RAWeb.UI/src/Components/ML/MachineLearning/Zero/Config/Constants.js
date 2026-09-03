const AutoApplyStatusType = {
    None: 0,
    NotAutoApply: "true",
    AutoApply: "false",
};

const AutoApplyStatus = new Map([
    [AutoApplyStatusType.NotAutoApply, RMResx.RM_JS_Common_Yes],
    [AutoApplyStatusType.AutoApply, RMResx.RM_JS_Common_No],
]);

const TermFilterColumnType = {
    AutoApply: 2,
};

const UsageLimitType = {
    AIRecommendation: 1,
    ZeroShot: 2,
}

export { AutoApplyStatusType, AutoApplyStatus, TermFilterColumnType, UsageLimitType };
