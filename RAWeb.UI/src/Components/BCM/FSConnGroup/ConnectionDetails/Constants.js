const manageColumnCacheName = "FSConnectionDetailsManageColumnCheckedIds";
const manageColumnConnectionCacheName = "FSConnectionManageColumnCheckedIds";

const filterCacheNamePrefix = "FSConnectionDetailsFilter";
const filterConnectionCacheNamePrefix = "FSConnectionFilter";

const cacheFilterDataType = {
    path: "ConnectionPath",
    jobType: "JobType",
    status: "Status",
    connectionGroup: "ConnectionGroupName",
    startTime: "StartTime",
    endTime: "EndTime",
    jobRunBy: "JobRunBy",
    modifiedTime: "LastModifiedTime",
    lastSyncTime: "LastSyncTime",
    groupName: "GroupName",
};

const FSJobStatusCode = {
    Wait: 0,
    InProgress: 1,
    Finished: 2,
    Failed: 3,
    FinishWithException: 4,
    Stopped: 5,
    Skipped: 6,
    Stopping: 7,
    Calculating: 8,
    Pending: 9
};

const FSJobStatusI18N = {
    [FSJobStatusCode.Wait]: RMResx.RM_JS_JM_Status_Wait,
    [FSJobStatusCode.InProgress]: RMResx.RM_JS_JM_Status_InProgerss,
    [FSJobStatusCode.Finished]: RMResx.RM_JS_JM_Status_Finished,
    [FSJobStatusCode.Failed]: RMResx.RM_JS_JM_Status_Failed,
    [FSJobStatusCode.FinishWithException]: RMResx.RM_JS_JM_Status_FinishWithException,
    [FSJobStatusCode.Stopped]: RMResx.RM_JS_JM_Status_Stopped,
    [FSJobStatusCode.Skipped]: RMResx.RM_JS_JM_Status_Skipped,
    [FSJobStatusCode.Stopping]: RMResx.RM_JS_JM_Status_Stopping,
    [FSJobStatusCode.Pending]: RMResx.RM_JS_JM_Status_Pending
};

const FSJobDetailStatusI18N = {
    0: RMResx.RM_JS_JMD_Status_Successful,
    1: RMResx.RM_JS_JMD_Status_Failed,
    2: RMResx.RM_JS_JMD_Status_Skipped,
    4: RMResx.RM_JS_JMD_Status_Exception
};

const FSAgentJobTypes = {
    FSDataSynchronization: 5000,
    FSDataSynchronizationSchedule: 5001,
    FSDisposal: 5002,
    FSDisposalSchedule: 5003,
    FSItemsFilesDueDisposal: 5004,
    FSCreateAndDestroyedFileReport: 5006,
    FSArchiverRestore: 8059,
    FSRetain: 8060,
    FSRetainSimulate: 8064,
    DiscoveryAnalysisFileSystemV1: 10019,
    DiscoveryFileSystemV1: 10018, // not supported job yet
    ApplyClassCode: 1027,
    FSDisposalByClassCode: 5202,
};

const FSJobDetailSearchKeys = {
    [FSAgentJobTypes.FSDataSynchronization]: ["ObjectName"],
    [FSAgentJobTypes.FSDataSynchronizationSchedule]: ["ObjectName"],
    [FSAgentJobTypes.FSDisposal]: ["ObjectName"],
    [FSAgentJobTypes.FSDisposalSchedule]: ["ObjectName"],
    [FSAgentJobTypes.FSCreateAndDestroyedFileReport]: ["Title"],
    [FSAgentJobTypes.FSItemsFilesDueDisposal]: ["TitleOrName"],
    [FSAgentJobTypes.FSArchiverRestore]: ["SourceLocation"],
    [FSAgentJobTypes.FSRetain]: ["SourceLocation", "SiteCollectionURL", "JobId"],
    [FSAgentJobTypes.FSRetainSimulate]: ["SourceLocation", "SiteCollectionURL", "JobId"],
    [FSAgentJobTypes.DiscoveryAnalysisFileSystemV1]: ["ConnectionName"],
    [FSAgentJobTypes.ApplyClassCode]: ["TitleOrName"],
    [FSAgentJobTypes.FSDisposalByClassCode]: ["ObjectName"],
};

const FSJobDetailsCells = {
    [FSAgentJobTypes.FSDataSynchronization]: ["ObjectName", "FullPath", "Status", "AgentName", "Comment"],
    [FSAgentJobTypes.FSDataSynchronizationSchedule]: ["ObjectName", "FullPath", "Status", "AgentName", "Comment"],
    [FSAgentJobTypes.FSDisposal]: ["Type", "ObjectName", "Size", "SourceLocation", "DestinationLocation", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
    [FSAgentJobTypes.FSDisposalSchedule]: ["Type", "ObjectName", "Size", "SourceLocation", "DestinationLocation", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
    [FSAgentJobTypes.FSCreateAndDestroyedFileReport]: ["ObjectLevel", "Title", "URL", "Status", "Comment"],
    [FSAgentJobTypes.FSItemsFilesDueDisposal]:  ["TitleOrName", "Type", "Url", "Status", "Comment"],
    [FSAgentJobTypes.FSArchiverRestore]: ["SourceLocation", "SizeStr", "Status", "FinishTimeStr", "Comment"],
    [FSAgentJobTypes.FSRetain]: ["SiteUrl", "JobId", "SizeStr", "SrcStorageName", "DesStorageName", "Action", "Status", "Comment"],
    [FSAgentJobTypes.FSRetainSimulate]: ["FileName", "SiteUrl", "SourceFlag", "SizeStr", "RetentionSetting", "SrcStorageName"],
    [FSAgentJobTypes.DiscoveryAnalysisFileSystemV1]: ["ConnectionName", "Status", "Comment"],
    [FSAgentJobTypes.ApplyClassCode]: ["ObjectName", "FullPath", "Status", "Comment"],
    [FSAgentJobTypes.FSDisposalByClassCode]: ["Type", "ObjectName", "Size", "SourceLocation", "DestinationLocation", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
};

const FSJobDetailsColumns = {
    [FSAgentJobTypes.FSDataSynchronization]: ["ObjectName", "Location", "Status", "AgentName", "Comment"],
    [FSAgentJobTypes.FSDataSynchronizationSchedule]: ["ObjectName", "FullPath", "Status", "AgentName", "Comment"],
    [FSAgentJobTypes.FSDisposal]: ["Type", "ObjectName", "Size", "BackupSourceURL", "DestinationURL", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
    [FSAgentJobTypes.FSDisposalSchedule]: ["Type", "ObjectName", "Size", "BackupSourceURL", "DestinationURL", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
    [FSAgentJobTypes.FSCreateAndDestroyedFileReport]: ["ObjectLevel", "TitleOrName", "URL", "Status", "Comment"],
    [FSAgentJobTypes.FSItemsFilesDueDisposal]: ["TitleOrName", "Type", "Url", "Status", "Comment"],
    [FSAgentJobTypes.FSArchiverRestore]: ["Location", "Size", "Status", "FinishTime", "Comment"],
    [FSAgentJobTypes.FSRetain]: ["Url", "JobId", "Size", "SrcStorageName", "DesStorageName", "Action", "Status", "Comment"],
    [FSAgentJobTypes.FSRetainSimulate]: ["FileName", "URL", "ContentSource", "Size", "RetentionSetting", "Storage"],
    [FSAgentJobTypes.DiscoveryAnalysisFileSystemV1]: ["ConnectionName", "Status", "Comment"],
    [FSAgentJobTypes.ApplyClassCode]: ["ObjectName", "FullPath", "Status", "Comment"],
    [FSAgentJobTypes.FSDisposalByClassCode]: ["Type", "ObjectName", "Size", "BackupSourceURL", "DestinationURL", "FinishTime", "RuleName", "Action", "AgentName", "Status", "Comment"],
};

const FSJobDetailsColumnsWidth = {
    [FSAgentJobTypes.FSDataSynchronization]: [0.2, 0.4, 0.1, 0.1, 0.2],
    [FSAgentJobTypes.FSDataSynchronizationSchedule]: [0.2, 0.4, 0.1, 0.1, 0.2],
    [FSAgentJobTypes.FSDisposal]: [0.1, 0.1, 0.1, 0.2, 0.2, 0.1, 0.1, 0.1, 0.15, 0.1, 0.1, 0.1],
    [FSAgentJobTypes.FSDisposalSchedule]: [0.1, 0.1, 0.1, 0.2, 0.2, 0.1, 0.1, 0.1, 0.15, 0.1, 0.1, 0.1],
    [FSAgentJobTypes.FSCreateAndDestroyedFileReport]: [0.15, 0.1, 0.15, 0.2, 0.4],
    [FSAgentJobTypes.FSItemsFilesDueDisposal]: [0.15, 0.1, 0.35, 0.1, 0.3],
    [FSAgentJobTypes.FSArchiverRestore]: [0.15, 0.1, 0.1, 0.1, 0.1],
    [FSAgentJobTypes.FSRetain]: [0.15, 0.15, 0.1, 0.15, 0.2, 0.1, 0.1, 0.2],
    [FSAgentJobTypes.FSRetainSimulate]: [0.15, 0.2, 0.1, 0.1, 0.15, 0.15],
    [FSAgentJobTypes.DiscoveryAnalysisFileSystemV1]: [0.4, 0.2, 0.4],
    [FSAgentJobTypes.ApplyClassCode]: [0.15, 0.4, 0.15, 0.3],
    [FSAgentJobTypes.FSDisposalByClassCode]: [0.1, 0.1, 0.1, 0.2, 0.2, 0.1, 0.1, 0.1, 0.15, 0.1, 0.1, 0.1],
};

const getKeyByValue = (object, value) => {
    return Object.keys(object).find(key => object[key] === value);
};

const FSAgentJobI18N = {
    [FSAgentJobTypes.FSArchiverRestore]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSArchiverRestore)],
    [FSAgentJobTypes.FSDataSynchronization]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSDataSynchronization)],
    [FSAgentJobTypes.FSDataSynchronizationSchedule]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSDataSynchronizationSchedule)],
    [FSAgentJobTypes.FSDisposal]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSDisposal)],
    [FSAgentJobTypes.FSDisposalSchedule]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSDisposalSchedule)],
    [FSAgentJobTypes.FSRetain]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSRetain)],
    [FSAgentJobTypes.FSRetainSimulate]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSRetainSimulate)],
    [FSAgentJobTypes.FSCreateAndDestroyedFileReport]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSCreateAndDestroyedFileReport)],
    [FSAgentJobTypes.FSItemsFilesDueDisposal]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSItemsFilesDueDisposal)],
    [FSAgentJobTypes.DiscoveryAnalysisFileSystemV1]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.DiscoveryAnalysisFileSystemV1)],
    [FSAgentJobTypes.ApplyClassCode]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.ApplyClassCode)],
    [FSAgentJobTypes.FSDisposalByClassCode]:
        RMResx["RM_JS_JM_JobType_" + getKeyByValue(FSAgentJobTypes, FSAgentJobTypes.FSDisposalByClassCode)],
};

export {
    manageColumnCacheName,
    filterCacheNamePrefix,
    cacheFilterDataType,
    FSJobStatusCode,
    FSJobStatusI18N,
    FSJobDetailStatusI18N,
    FSAgentJobTypes,
    FSAgentJobI18N,
    FSJobDetailSearchKeys,
    FSJobDetailsCells,
    FSJobDetailsColumns,
    FSJobDetailsColumnsWidth,
    filterConnectionCacheNamePrefix,
    manageColumnConnectionCacheName,
}