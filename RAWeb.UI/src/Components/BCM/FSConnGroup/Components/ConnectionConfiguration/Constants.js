export const AccessConnectionType = {
    All: 0,
    Specify: 1,
};

export const DCConnectionType = {
    None: 0,
    Default: 1,
    Specify: 2,
};

export const SourceType = {
    FileSystem: 1,
    SharePointOnPremise: 2
};

export const AgentServiceStatus = {
    NotInstalled: 0,
    InActive: 1,
    Active: 2,
    Deleted: 3,
    Disabled: 4,
    Mismatched: 5,
    ActiveException: 6
};

export const AgentSErviceStatusI18Ns = new Map([
    [AgentServiceStatus.NotInstalled, RMResx.RM_CP_Agent_Column_Status_NotInstalled],
    [AgentServiceStatus.InActive, RMResx.RM_CP_Agent_Column_Status_InActive],
    [AgentServiceStatus.Active, RMResx.RM_CP_Agent_Column_Status_Active],
    [AgentServiceStatus.Disabled, RMResx.RM_CP_Agent_Column_Status_Disabled],
    [AgentServiceStatus.Mismatched, RMResx.RM_CP_Agent_Column_Status_Mismatched],
    [AgentServiceStatus.ActiveException, RMResx.RM_CP_Agent_Column_Status_ActiveException],
]);

export const AgentServiceStatusIcon = new Map([
    [AgentServiceStatus.NotInstalled, "reco-conn-cfg-not-installed-img"],
    [AgentServiceStatus.InActive, "reco-conn-cfg-inActive-img"],
    [AgentServiceStatus.Active, "reco-conn-cfg-active-img"],
    [AgentServiceStatus.Disabled, "reco-conn-cfg-disabled-img"],
    [AgentServiceStatus.Mismatched, "fia-mismatched"],
    [AgentServiceStatus.ActiveException, "fia-error"],
]);

export const SourceTypeI18N = new Map([
    [SourceType.SharePointOnPremise, RMResx.RM_JS_SPS_TabLabel_SPLocal],
    [SourceType.FileSystem, RMResx.RM_JS_SPS_TabLabel_FS],
]);