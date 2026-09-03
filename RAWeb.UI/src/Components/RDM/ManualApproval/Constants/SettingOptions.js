export const EscalateSettingType = {
    None: 0,
    WorkflowNextStep: 1,
    ReassignSpecificUsers: 2,
    NoAction: 3,
};

export const EscalateSettingTypeI18NS = new Map([
    [EscalateSettingType.WorkflowNextStep, RMResx.RM_MA_Setting_Escalation_Workflow],
    [EscalateSettingType.ReassignSpecificUsers, RMResx.RM_MA_Setting_Escalation_Reassign],
    [EscalateSettingType.NoAction, RMResx.RM_MA_Setting_Escalation_NoAction],
]);

export const IntervalType = {
    None: 0,
    Days: 1,
    Weeks: 2,
};

export const IntervalTypeI18Ns = new Map([
    [IntervalType.Days, RMResx.RM_JS_ScheduleSetting_Days],
    [IntervalType.Weeks, RMResx.RM_JS_ScheduleSetting_Weeks],
]);

export const EndType = {
    None: 0,
    NoEnd: 1,
    EndOccurrences: 2,
};

export const EndTypeI18Ns = new Map([
    [EndType.NoEnd, RMResx.RM_JS_ScheduleSetting_NoEndDate],
    [EndType.EndOccurrences, RMResx.RM_JS_ScheduleSetting_EndAfter],
]);

export const NotificationType = {
    None: 0,
    Interval: 1,
    Advanced: 2,
};