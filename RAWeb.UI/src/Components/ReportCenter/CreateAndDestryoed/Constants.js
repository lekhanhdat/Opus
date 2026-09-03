export const ActionType = {
    None: 0,
    Creation: 1,
    Destruction: 2
};

export const DateFrameType = {
    CurrentWeek: 1,
    CurrentMonth: 2,
    Last3Months: 3,
    Last6Months: 4,
    Custom: 5
};

export const ActionTypeName = new Map([
    [ActionType.Creation, RMResx.RM_JS_RC_TimeFrame_Create],
    [ActionType.Destruction, RMResx.RM_JS_RC_TimeFrame_Destroyed]
]);

export const DateFrameTypeName = new Map([
    [DateFrameType.CurrentWeek, RMResx.RM_RC_Audit_Range_5D],
    [DateFrameType.CurrentMonth, RMResx.RM_RC_Audit_Range_1M],
    [DateFrameType.Last3Months, RMResx.RM_RC_Audit_Range_3M],
    [DateFrameType.Last6Months, RMResx.RM_RC_Audit_Range_6M],
    [DateFrameType.Custom, RMResx.RM_RC_Audit_Range_Custom]
]);