using AvePoint.RA.Common;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace RAMultiGeo.SyncCommonData.MainDC.DataCenterSync
{
    internal class ChangeLogReader
    {
        private readonly IRMMultiGeoApiChangeLogDao RMMultiGeoApiChangeLogDao = new RMMultiGeoApiChangeLogDao();
        private long LastSyncTime;
        private bool SyncImageEmailTemplateFailed;
        public void SetLastSyncTime(long lastSyncTime) => LastSyncTime = lastSyncTime;

        public long GetAllTableNeedSync()
        {
            var listOperationType = RMMultiGeoApiChangeLogDao.GetAllOperationTypeNeedSync(TenantLocalValue.LogonGroupId, LastSyncTime);
            long tableNeedUpdate = 0;
            MultiGeoOperationType geoOperationType = MultiGeoOperationType.None;
            foreach (var operationType in listOperationType)
            {
                if (!Enum.TryParse(operationType, out geoOperationType))
                {
                    geoOperationType = MultiGeoOperationType.None;
                }
                if (geoOperationType == MultiGeoOperationType.UploadImages)
                {
                    SyncImageEmailTemplateFailed = true;
                }
                tableNeedUpdate |= ConvertOperationTypeToSyncTable(geoOperationType);
            }
            return tableNeedUpdate;
        }

        public bool GetSyncImageEmailTemplateFailed()
        {
            return SyncImageEmailTemplateFailed;
        }

        private long ConvertOperationTypeToSyncTable(MultiGeoOperationType operationType) => operationType switch
        {
            MultiGeoOperationType.CreateAgent => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.UpdateAgent => (long)MultiGeoCommonSyncTable.RMAgents | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships,
            MultiGeoOperationType.DeleteAgent => (long)MultiGeoCommonSyncTable.RMAgents | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships,
            MultiGeoOperationType.DisableAgent => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.EnableAgent => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.UpdateUniqueIdSetting => (long)MultiGeoCommonSyncTable.RMUniqueIdSettings,
            MultiGeoOperationType.SaveConnectionGroup => (long)MultiGeoCommonSyncTable.FSConnectionGroups | (long)MultiGeoCommonSyncTable.FSConnections | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships | (long)MultiGeoCommonSyncTable.RMRecordOwners,
            MultiGeoOperationType.SaveConnection => (long)MultiGeoCommonSyncTable.FSConnections | (long)MultiGeoCommonSyncTable.RMFSConnectionAndOwnerRelationships | (long)MultiGeoCommonSyncTable.RMAccounts,
            MultiGeoOperationType.DeleteConnection => (long)MultiGeoCommonSyncTable.RMFSConnectionAndOwnerRelationships | (long)MultiGeoCommonSyncTable.FSConnections | (long)MultiGeoCommonSyncTable.RMRecordOwners,
            MultiGeoOperationType.DeleteGroup => (long)MultiGeoCommonSyncTable.FSConnectionGroups | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships | (long)MultiGeoCommonSyncTable.FSConnections,
            MultiGeoOperationType.SaveManualProcess => (long)MultiGeoCommonSyncTable.RMWorkflowDefinitions | (long)MultiGeoCommonSyncTable.RMWorkflowSteps | (long)MultiGeoCommonSyncTable.RMWorkflowStepConfigurations,
            MultiGeoOperationType.DeleteManualProcess => (long)MultiGeoCommonSyncTable.RMWorkflowDefinitions | (long)MultiGeoCommonSyncTable.RMWorkflowSteps | (long)MultiGeoCommonSyncTable.RMWorkflowStepConfigurations,
            MultiGeoOperationType.CreateRule => (long)MultiGeoCommonSyncTable.RMRules | (long)MultiGeoCommonSyncTable.RMRuleContainerMemberships  | (long)MultiGeoCommonSyncTable.RMAccounts | (long)MultiGeoCommonSyncTable.RMMiscProfiles,
            MultiGeoOperationType.EditRule => (long)MultiGeoCommonSyncTable.RMRules | (long)MultiGeoCommonSyncTable.RMRuleContainerMemberships | (long)MultiGeoCommonSyncTable.RMAccounts | (long)MultiGeoCommonSyncTable.RMMiscProfiles | (long)MultiGeoCommonSyncTable.RMChangeClassifications,
            MultiGeoOperationType.DeleteRules => (long)MultiGeoCommonSyncTable.RMRules | (long)MultiGeoCommonSyncTable.RMRuleContainerMemberships | (long)MultiGeoCommonSyncTable.RMMiscProfiles | (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMChangeClassifications,
            MultiGeoOperationType.CreateRuleContainer => (long)MultiGeoCommonSyncTable.RMRuleContainers,
            MultiGeoOperationType.EditRuleContainer => (long)MultiGeoCommonSyncTable.RMRuleContainers,
            MultiGeoOperationType.DeleteRuleContainer => (long)MultiGeoCommonSyncTable.RMRuleContainers,
            MultiGeoOperationType.CreateTerm => (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMKeyValues,
            MultiGeoOperationType.RenameTerm => (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMTermSetMemberships,
            MultiGeoOperationType.RenameTermGroup => (long)MultiGeoCommonSyncTable.RMTermGroups,
            MultiGeoOperationType.RenameTermSet => (long)MultiGeoCommonSyncTable.RMTermSets,
            MultiGeoOperationType.DeprecateTerm => (long)MultiGeoCommonSyncTable.RMTerms,
            MultiGeoOperationType.EnableTerm => (long)MultiGeoCommonSyncTable.RMTerms,
            MultiGeoOperationType.DeleteTerm => (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMGoogleLabelInfoes | (long)MultiGeoCommonSyncTable.RMKeyValues | (long)MultiGeoCommonSyncTable.RMMLTerms,
            MultiGeoOperationType.DeleteRootTerms => (long)MultiGeoCommonSyncTable.RMTermSets | (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMKeyValues | (long)MultiGeoCommonSyncTable.RMMLTerms,
            MultiGeoOperationType.DeleteTermGroup => (long)MultiGeoCommonSyncTable.RMTermGroups | (long)MultiGeoCommonSyncTable.RMTermGroupMemberships | (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMGoogleLabelInfoes | (long)MultiGeoCommonSyncTable.RMKeyValues | (long)MultiGeoCommonSyncTable.RMTermSets | (long)MultiGeoCommonSyncTable.RMMLTerms | (long)MultiGeoCommonSyncTable.RMTerms,
            MultiGeoOperationType.InheritSettingToParent => (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMChangeClassifications,
            MultiGeoOperationType.CreateTermGroup => (long)MultiGeoCommonSyncTable.RMTermGroups,
            MultiGeoOperationType.CreateTermSet => (long)MultiGeoCommonSyncTable.RMTermSets,
            MultiGeoOperationType.SaveTermSettings => (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMKeyValues | (long)MultiGeoCommonSyncTable.RMEXOLabels | (long)MultiGeoCommonSyncTable.RMChangeClassifications,
            MultiGeoOperationType.SaveTermSet => (long)MultiGeoCommonSyncTable.RMTermSets,
            MultiGeoOperationType.SaveTermGroup => (long)MultiGeoCommonSyncTable.RMTermGroups | (long)MultiGeoCommonSyncTable.RMTermGroupMemberships,
            MultiGeoOperationType.SaveMultiGeoSettings => (long)MultiGeoCommonSyncTable.MultiGeoSettingInfoes,
            MultiGeoOperationType.ImportTermAndRule => (long)MultiGeoCommonSyncTable.RMTerms | (long)MultiGeoCommonSyncTable.RMTermSetMemberships | (long)MultiGeoCommonSyncTable.RMRules | (long)MultiGeoCommonSyncTable.RMTermRuleAssociations | (long)MultiGeoCommonSyncTable.RMTermSets 
            |  (long)MultiGeoCommonSyncTable.RMTermGroups | (long)MultiGeoCommonSyncTable.RMMLTerms | (long)MultiGeoCommonSyncTable.RMRuleContainerMemberships | (long)MultiGeoCommonSyncTable.RMRuleContainers | (long)MultiGeoCommonSyncTable.RMMiscProfiles,
            MultiGeoOperationType.CreateEmailTemplate => (long)MultiGeoCommonSyncTable.RMEmailTemplates,
            MultiGeoOperationType.EditEmailTemplate => (long)MultiGeoCommonSyncTable.RMEmailTemplates,
            MultiGeoOperationType.DeleteEmailTemplate => (long)MultiGeoCommonSyncTable.RMEmailTemplates,
            MultiGeoOperationType.SaveApprovalSettingInfo => (long)MultiGeoCommonSyncTable.RMFunctionSettings | (long)MultiGeoCommonSyncTable.RMAccounts,
            MultiGeoOperationType.SyncUsers => (long)MultiGeoCommonSyncTable.RMAccounts,
            MultiGeoOperationType.JpmcAgentMgmtCreateAgentAsync => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.JpmcAgentMgmtUpdateAgentAsync => (long)MultiGeoCommonSyncTable.RMAgents | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships,
            MultiGeoOperationType.JpmcAgentMgmtDeleteAgentAsync => (long)MultiGeoCommonSyncTable.RMAgents | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships,
            MultiGeoOperationType.JpmcAgentMgmtDisableAgentAsync => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.JpmcAgentMgmtEnableAgentAsync => (long)MultiGeoCommonSyncTable.RMAgents,
            MultiGeoOperationType.JpmcAgentMgmtUpdateAgentJobLimit => (long)MultiGeoCommonSyncTable.RMKeyValues,
            MultiGeoOperationType.SaveConnectionGroups => (long)MultiGeoCommonSyncTable.FSConnectionGroups | (long)MultiGeoCommonSyncTable.FSConnections | (long)MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships | (long)MultiGeoCommonSyncTable.RMRecordOwners,
            MultiGeoOperationType.SaveConnections => (long)MultiGeoCommonSyncTable.FSConnections | (long)MultiGeoCommonSyncTable.RMFSConnectionAndOwnerRelationships | (long)MultiGeoCommonSyncTable.RMAccounts,
            MultiGeoOperationType.MyHubPauseOrResume => (long)MultiGeoCommonSyncTable.FSConnections,
            MultiGeoOperationType.OtherDCJpmcTriggerJobPauseDisposalProcess => (long)MultiGeoCommonSyncTable.FSConnections,
            MultiGeoOperationType.OtherDCJpmcTriggerJobResumeDisposalProcess => (long)MultiGeoCommonSyncTable.FSConnections,
            _ => (long)MultiGeoCommonSyncTable.None
        };
    }
}
