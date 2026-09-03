using System;
using System.Collections.Generic;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal static class MultiGeoOperationRegistry
{
    public static IReadOnlyDictionary<string, MultiGeoOperationDescriptor> Create()
    {
        var registry = new Dictionary<string, MultiGeoOperationDescriptor>(StringComparer.OrdinalIgnoreCase);

        RegisterAgentManagementOperations(registry);
        RegisterBusinessClassificationOperations(registry);
        RegisterConnectionRegisterOperations(registry);
        RegisterControlPanelOperations(registry);
        RegisterCheckCOPDeletionOperations(registry);
        RegisterMultiGeoSettingOperations(registry);
        RegisterRuleManagementOperations(registry);
        RegisterJobOperations(registry);
        RegisterTermManagementOperations(registry);
        RegisterTenantManagementOperations(registry);
        RegisterMyHubOperations(registry);
        RegisterManualApprovalOperations(registry);
        RegisterSingalROperations(registry);
        RegisterJpmcAgentManagementOperations(registry);
        RegisterJpmcJobTriggerOperations(registry);

        return registry;
    }

    private static void RegisterJpmcJobTriggerOperations(Dictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.OtherDCJpmcTriggerJobPauseDisposalProcess, "api/v1/job-trigger/OtherPauseDisposalProcess");
        RegisterOperation(registry, MultiGeoOperationType.MainDCJpmcTriggerJobPauseDisposalProcess, "api/v1/job-trigger/MainPauseDisposalProcess");
        RegisterOperation(registry, MultiGeoOperationType.MainDCJpmcTriggerJobResumeDisposalProcess, "api/v1/job-trigger/MainResumeDisposalProcess");
        RegisterOperation(registry, MultiGeoOperationType.OtherDCJpmcTriggerJobResumeDisposalProcess, "api/v1/job-trigger/OtherResumeDisposalProcess");
    }

    private static void RegisterJpmcAgentManagementOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtCreateAgentAsync, "api/v1/agent-mgmt/CreateAgent");
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtUpdateAgentAsync, "api/v1/agent-mgmt/UpdateAgent");
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtDeleteAgentAsync, "api/v1/agent-mgmt/DeleteAgent");
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtDisableAgentAsync, "api/v1/agent-mgmt/DisableAgent");
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtEnableAgentAsync, "api/v1/agent-mgmt/EnableAgent");
        RegisterOperation(registry, MultiGeoOperationType.JpmcAgentMgmtUpdateAgentJobLimit, "api/v1/agent-mgmt/UpdateAgentJobLimit");
    }

    private static void RegisterManualApprovalOperations(Dictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalUnderReviewQuery, "api/ManualApprovalApi/UnderReviewQuery");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalApprove, "api/ManualApprovalApi/Approve");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalReject, "api/ManualApprovalApi/Reject");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalRunFolderViewActionJob, "api/ManualApprovalApi/RunFolderViewActionJob");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalRunBulkActionJob, "api/ManualApprovalApi/RunBulkActionJob");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalQueryFolderPath, "api/ManualApprovalApi/QueryFolderPath");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalGetRealTimeJobStatusInfo, "api/ManualApprovalApi/GetRealTimeJobStatusInfo");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalDoAction, "api/ManualApprovalApi/DoAction");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalGetApprovalCommentOption, "api/ManualApprovalApi/GetApprovalCommentOption");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalGetSettingInfo, "api/ManualApprovalApi/GetSettingInfo");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalPause, "api/ManualApprovalApi/Pause");
        RegisterOperation(registry, MultiGeoOperationType.ManualApprovalResume, "api/ManualApprovalApi/Resume");
        RegisterOperation(registry, MultiGeoOperationType.SaveApprovalSettingInfo, "api/ManualApprovalApi/SaveApprovalSettingInfo");
    }

    private static void RegisterMyHubOperations(Dictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryDrivesVolume, "api/MyHubApi/QueryDrivesVolume");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetNodeIdByConnectionId, "api/MyHubApi/GetNodeIdByConnectionId");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryMyhubTreeFolder, "api/MyHubApi/QueryMyhubTreeFolder");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryMyhubRootTreeFolderItems, "api/MyHubApi/QueryMyhubRootTreeFolderItems");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryDetailTable, "api/MyHubApi/QueryDetailTable");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryFolderAndItems, "api/MyHubApi/QueryFolderAndItems");
        RegisterOperation(registry, MultiGeoOperationType.MyhubReadClassCodeName, "api/MyHubApi/ReadClassifyDataByPartitionKeyIds");
        RegisterOperation(registry, MultiGeoOperationType.MyHubClassifyUpdate, "api/MyHubApi/ClassifyUpdate");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryClassifyInfo, "api/MyHubApi/QueryClassifyInfo");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetPendingDisposalVolume, "api/MyHubApi/GetPendingDisposalVolume");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetPendingDisposalVolumeDisc, "api/MyHubApi/GetPendingDisposalVolumeDisc");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetFSDashboardData, "api/MyHubApi/GetFSDashboardData");
        RegisterOperation(registry, MultiGeoOperationType.MyHubRunFSDashboardDataSyncJob, "api/MyHubApi/RunFSDashboardDataSyncJob");
        RegisterOperation(registry, MultiGeoOperationType.MyHubUpdateConnectionRecordOwners, "api/MyHubApi/UpdateConnectionRecordOwners");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetPendingDisposalFolderFilter, "api/MyHubApi/GetPendingDisposalFolderFilter");
        RegisterOperation(registry, MultiGeoOperationType.MyhubGetParameterBeforeUnderReviewQuery, "api/MyHubApi/GetParameterBeforeUnderReviewQuery");
        RegisterOperation(registry, MultiGeoOperationType.MyHubPauseOrResume, "api/MyHubApi/PauseOrResume");
        RegisterOperation(registry, MultiGeoOperationType.MyHubLoadRCCInfosById, "api/MyHubApi/LoadRCCInfosById");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGenerateRCCReport, "api/MyHubApi/GenerateRCCReport");
        RegisterOperation(registry, MultiGeoOperationType.MyHubLoadDisposalReportData, "api/MyHubApi/LoadDisposalReportData");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGenerateDisposalHistoryReport, "api/MyHubApi/GenerateDisposalHistoryReport");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryDriveSettings, "api/MyHubApi/QueryDriveSettings");
        RegisterOperation(registry, MultiGeoOperationType.MyHubDownloadReportContentMyhub, "api/MyHubApi/DownloadReportContentMyhub");
        RegisterOperation(registry, MultiGeoOperationType.MyHubQueryAuditTrial, "api/MyHubApi/QueryAuditTrial");
        RegisterOperation(registry, MultiGeoOperationType.MyHubGetFolderStatistics, "api/MyHubApi/GetFolderStatistics");
        RegisterOperation(registry, MultiGeoOperationType.MyHubCheckJobExists, "api/MyHubApi/CheckJobExists");
        RegisterOperation(registry, MultiGeoOperationType.MyHubDeleteReportContent, "api/MyHubApi/DeleteReportContent");
    }

    private static void RegisterTenantManagementOperations(Dictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.InitTenant, "api/initialization/InitTenant");
        RegisterOperation(registry, MultiGeoOperationType.IsInitTenant, "api/initialization/IsTenantInitialized");
        RegisterOperation(registry, MultiGeoOperationType.RunSyncCommonDataOtherDCJob, "api/CommonDataResourceApi/RunSyncCommonDataOtherDCJob");
    }

    private static void RegisterAgentManagementOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.CreateAgent, "api/CPAgentMgmtApi/CreateAgent");
        RegisterOperation(registry, MultiGeoOperationType.UpdateAgent, "api/CPAgentMgmtApi/UpdateAgent");
        RegisterOperation(registry, MultiGeoOperationType.DeleteAgent, "api/CPAgentMgmtApi/DeleteAgent");
        RegisterOperation(registry, MultiGeoOperationType.DisableAgent, "api/CPAgentMgmtApi/DisableAgent");
        RegisterOperation(registry, MultiGeoOperationType.EnableAgent, "api/CPAgentMgmtApi/EnableAgent");
        RegisterOperation(registry, MultiGeoOperationType.UpgradeCloudAgent, "api/CPAgentMgmtApi/UpgradeCloudAgent");
        RegisterOperation(registry, MultiGeoOperationType.CreateCertificate, "api/CPAgentMgmtApi/CreateCertificate");
        RegisterOperation(registry, MultiGeoOperationType.SaveClientId, "api/CPAgentMgmtApi/SaveClientId");
        RegisterOperation(registry, MultiGeoOperationType.UpdateAgentRuntimeStatus, "api/CPAgentMgmtApi/SyncAgentRuntimeStatus");
        RegisterOperation(registry, MultiGeoOperationType.SyncAgentStatusAfterUpgrade, "api/CPAgentMgmtApi/SyncAgentStatusAfterUpgrade");
        RegisterOperation(registry, MultiGeoOperationType.CheckRunningAgent, "api/CPAgentMgmtApi/HasAgentsRunningJobs");
    }

    private static void RegisterBusinessClassificationOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.UpdateUniqueIdSetting, "api/BCMAdminSettingApi/UpdateUniqueIdSetting");
    }

    private static void RegisterConnectionRegisterOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.SaveConnectionGroup, "api/ConnectionRegisterApi/SaveConnectionGroup");
        RegisterOperation(registry, MultiGeoOperationType.SaveConnectionGroups, "api/ConnectionRegisterApi/SaveConnectionGroups");
        RegisterOperation(registry, MultiGeoOperationType.SaveConnection, "api/ConnectionRegisterApi/SaveConnection");
        RegisterOperation(registry, MultiGeoOperationType.SaveConnections, "api/ConnectionRegisterApi/SaveConnections");
        RegisterOperation(registry, MultiGeoOperationType.ValidateConnections, "api/ConnectionRegisterApi/ValidateConnections");
        RegisterOperation(registry, MultiGeoOperationType.DeleteConnection, "api/ConnectionRegisterApi/DeleteConnection");
        RegisterOperation(registry, MultiGeoOperationType.DeleteGroup, "api/ConnectionRegisterApi/DeleteGroup");
        RegisterOperation(registry, MultiGeoOperationType.UpdateLastSyncTimeFSConnection, "api/ConnectionRegisterApi/UpdateLastSyncTimeFSConnection");
    }

    private static void RegisterControlPanelOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.SaveManualProcess, "api/CpApi/SaveManualProcess");
        RegisterOperation(registry, MultiGeoOperationType.DeleteManualProcess, "api/CpApi/DeleteManualProcess");
        RegisterOperation(registry, MultiGeoOperationType.CreateEmailTemplate, "api/CpApi/CreateEmailTemplate");
        RegisterOperation(registry, MultiGeoOperationType.EditEmailTemplate, "api/CpApi/EditEamilTemplate");
        RegisterOperation(registry, MultiGeoOperationType.DeleteEmailTemplate, "api/CpApi/DeleteEmailTemplate");
        RegisterOperation(registry, MultiGeoOperationType.SyncUsers, "api/CpApi/SyncUsers");
        RegisterOperation(registry, MultiGeoOperationType.UploadImages, "api/CpApi/UploadImage");
        RegisterOperation(registry, MultiGeoOperationType.SyncUsersToMainDC, "api/CpApi/SyncCommonDataUsersInfo");

    }

    private static void RegisterCheckCOPDeletionOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.PrepareCheckCOPDeletion, "api/CopApi/PrepareCheckCOPDeletion");
    }

    private static void RegisterMultiGeoSettingOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.SaveMultiGeoSettings, "api/MultiGeoSettingApi/SaveMultiGeoSettings");
    }

    private static void RegisterRuleManagementOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.CreateRule, "api/RuleApi/CreateRule");
        RegisterOperation(registry, MultiGeoOperationType.EditRule, "api/RuleApi/EditRule");
        RegisterOperation(registry, MultiGeoOperationType.DeleteRules, "api/RuleApi/DeleteRules");
        RegisterOperation(registry, MultiGeoOperationType.CreateRuleContainer, "api/RuleApi/SaveRuleContainer");
        RegisterOperation(registry, MultiGeoOperationType.EditRuleContainer, "api/RuleApi/SaveRuleContainer");
        RegisterOperation(registry, MultiGeoOperationType.DeleteRuleContainer, "api/RuleApi/DeleteRuleContainer");
    }

    private static void RegisterTermManagementOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.CreateTerm, "api/TermManagementApi/CreateTerm", "AddTerm");
        RegisterOperation(registry, MultiGeoOperationType.RenameTerm, "api/TermManagementApi/RenameTerm");
        RegisterOperation(registry, MultiGeoOperationType.RenameTermGroup, "api/TermManagementApi/RenameTermGroup");
        RegisterOperation(registry, MultiGeoOperationType.RenameTermSet, "api/TermManagementApi/RenameTermSet");
        RegisterOperation(registry, MultiGeoOperationType.DeprecateTerm, "api/TermManagementApi/ApplyDeprecateTerm");
        RegisterOperation(registry, MultiGeoOperationType.EnableTerm, "api/TermManagementApi/ApplyEnableTerm");
        RegisterOperation(registry, MultiGeoOperationType.DeleteTerm, "api/TermManagementApi/ApplyDeleteTerm");
        RegisterOperation(registry, MultiGeoOperationType.DeleteRootTerms, "api/TermManagementApi/ApplyDeleteRootTerms");
        RegisterOperation(registry, MultiGeoOperationType.DeleteTermGroup, "api/TermManagementApi/DeleteTermGroup");
        RegisterOperation(registry, MultiGeoOperationType.InheritSettingToParent, "api/TermManagementApi/InheritSettingToParent");
        RegisterOperation(registry, MultiGeoOperationType.CreateTermGroup, "api/TermManagementApi/CreateTermGroup", "AddTermGroup");
        RegisterOperation(registry, MultiGeoOperationType.CreateTermSet, "api/TermManagementApi/CreateTermSet", "AddTermSet");
        RegisterOperation(registry, MultiGeoOperationType.SaveTermSettings, "api/TermManagementApi/SaveTermSettings");
        RegisterOperation(registry, MultiGeoOperationType.SaveTermSet, "api/TermManagementApi/SaveTermSet");
        RegisterOperation(registry, MultiGeoOperationType.SaveTermGroup, "api/TermManagementApi/SaveTermGroup");
    }

    private static void RegisterJobOperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.ImportTermAndRule, "api/TermManagementApi/ImportData", true, "ImportData");
    }

    private static void RegisterSingalROperations(IDictionary<string, MultiGeoOperationDescriptor> registry)
    {
        RegisterOperation(registry, MultiGeoOperationType.SignalRGetAgent, "api/SignalRApi/GetAgents");
    }

    private static void RegisterOperation(
        IDictionary<string, MultiGeoOperationDescriptor> registry,
        MultiGeoOperationType operationType,
        string replicaApiPath,
        params string[] aliases)
    {
        RegisterOperation(registry, operationType, replicaApiPath, false, aliases);
    }

    private static void RegisterOperation(
        IDictionary<string, MultiGeoOperationDescriptor> registry,
        MultiGeoOperationType operationType,
        string replicaApiPath,
        bool isJobAction,
        params string[] aliases)
    {
        var descriptor = new MultiGeoOperationDescriptor(operationType, replicaApiPath, isJobAction);
        registry[operationType.ToString()] = descriptor;

        foreach (var alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            registry[alias] = descriptor;
        }
    }
}