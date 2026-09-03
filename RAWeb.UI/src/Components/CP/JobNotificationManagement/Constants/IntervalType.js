export const IntervalType  = {
    None: 0,
    Daily: 1,
    Weekly: 2,
};


export const IntervalTypeI18N = new Map([
    [IntervalType.Daily, RMResx.RM_JS_JN_IntervalType_Daily],
    [IntervalType.Weekly, RMResx.RM_JS_JN_IntervalType_Weekly],
]);

export const WeeklyTypes = {
    Sunday : 0,
    Monday: 1,
    Tuesday: 2,
    Wednesday: 3,
    Thursday: 4,
    Friday: 5,
    Saturday: 6,
};

export const WeeklyTypesI18N = new Map([
    [WeeklyTypes.Monday, RMResx.RM_JS_JN_WeeklyType_Monday],
    [WeeklyTypes.Tuesday, RMResx.RM_JS_JN_WeeklyType_Tuesday],
    [WeeklyTypes.Wednesday, RMResx.RM_JS_JN_WeeklyType_Wednesday],
    [WeeklyTypes.Thursday, RMResx.RM_JS_JN_WeeklyType_Thursday],
    [WeeklyTypes.Friday, RMResx.RM_JS_JN_WeeklyType_Friday],
    [WeeklyTypes.Saturday, RMResx.RM_JS_JN_WeeklyType_Saturday],
    [WeeklyTypes.Sunday, RMResx.RM_JS_JN_WeeklyType_Sunday],
]);
