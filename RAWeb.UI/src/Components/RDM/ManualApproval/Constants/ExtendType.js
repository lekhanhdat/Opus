export const ExtendType = {
    None: 0,
    After3Month: 1,
    After6Month: 2,
    After1Year: 3,
    Custom: 4,
    Month: 5,
    Year:6,
    After1Month: 7, 
};

export const ExtendTypeI18Ns = new Map([
    [ExtendType.After3Month, RMResx.RM_MA_EntendDisposalTime_3M],
    [ExtendType.After6Month, RMResx.RM_MA_EntendDisposalTime_6M],
    [ExtendType.After1Year, RMResx.RM_MA_EntendDisposalTime_1Y],
    [ExtendType.Custom, RMResx.RM_MA_EntendDisposalTime_Custom],
    [ExtendType.Month, RMResx.RM_MA_EntendDisposalTime_Month],
    [ExtendType.Year, RMResx.RM_MA_EntendDisposalTime_Year],
    [ExtendType.After1Month, RMResx.RM_MA_EntendDisposalTime_1Month],
]);
