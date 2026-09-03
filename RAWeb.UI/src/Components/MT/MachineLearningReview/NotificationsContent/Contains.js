const StatusEnum = {
    InProgress: 0,
    Failed: 1,
    Completed: 2,
};

const ProgressStatus = {
    [StatusEnum.InProgress]: RMResx.RM_JS_Notifi_Status_Running,
    [StatusEnum.Failed]: RMResx.RM_JS_Notifi_Status_Failed,
    [StatusEnum.Completed]: RMResx.RM_JS_Notifi_Status_Competed,
};

const ProgressStatusIcon = {
    [StatusEnum.InProgress]: "fia-in-progress ra-info-color",
    [StatusEnum.Failed]: "fia-status-error ra-error-color",
    [StatusEnum.Completed]: "fia-checkbox-device ra-success-color",
};

export {StatusEnum, ProgressStatus, ProgressStatusIcon};
