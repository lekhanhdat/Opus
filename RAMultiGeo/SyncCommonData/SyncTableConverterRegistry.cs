using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.DB.Model;
using RAMultiGeo.Repositories;

namespace RAMultiGeo.SyncCommonData
{
    public static class SyncTableConverterRegistry
    {
        private static Dictionary<MultiGeoCommonSyncTable, Func<object>> _converters = new();

        static SyncTableConverterRegistry()
        {
            AddConverter(MultiGeoCommonSyncTable.RMTermGroups, () => new SQLiteSyncRepository<RMTermGroup>());
            AddConverter(MultiGeoCommonSyncTable.RMTermSets, () => new SQLiteSyncRepository<RMTermSet>());
            AddConverter(MultiGeoCommonSyncTable.RMTermSetMemberships, () => new SQLiteSyncRepository<RMTermSetMembership>());
            AddConverter(MultiGeoCommonSyncTable.RMTermRuleAssociations, () => new SQLiteSyncRepository<RMTermRuleAssociation>());
            AddConverter(MultiGeoCommonSyncTable.RMTerms, () => new SQLiteSyncRepository<RMTerm>());
            AddConverter(MultiGeoCommonSyncTable.RMWorkflowDefinitions, () => new SQLiteSyncRepository<RMWorkflowDefinition>());
            AddConverter(MultiGeoCommonSyncTable.RMWorkflowSteps, () => new SQLiteSyncRepository<RMWorkflowStep>());
            AddConverter(MultiGeoCommonSyncTable.RMWorkflowStepConfigurations, () => new SQLiteSyncRepository<RMWorkflowStepConfiguration>());
            AddConverter(MultiGeoCommonSyncTable.RMRuleContainers, () => new SQLiteSyncRepository<RMRuleContainer>());
            AddConverter(MultiGeoCommonSyncTable.RMRules, () => new SQLiteSyncRepository<RMRule>());
            AddConverter(MultiGeoCommonSyncTable.RMRuleContainerMemberships, () => new SQLiteSyncRepository<RMRuleContainerMembership>());
            AddConverter(MultiGeoCommonSyncTable.RMAgents, () => new SQLiteSyncRepository<RMAgent>());
            AddConverter(MultiGeoCommonSyncTable.RMKeyValues, () => new SQLiteSyncRepository<RMKeyValue>());
            AddConverter(MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships, () => new SQLiteSyncRepository<FSConnectionGroupWithAgentMembership>());
            AddConverter(MultiGeoCommonSyncTable.RMCertificates, () => new SQLiteSyncRepository<RMCertificate>());
            AddConverter(MultiGeoCommonSyncTable.RMLnkUserGroups, () => new SQLiteSyncRepository<RMLnkUserGroup>());
            AddConverter(MultiGeoCommonSyncTable.RMAccounts, () => new SQLiteSyncRepository<RMAccount>());
            AddConverter(MultiGeoCommonSyncTable.RMLnkUserRoles, () => new SQLiteSyncRepository<RMLnkUserRole>());
            AddConverter(MultiGeoCommonSyncTable.RMRecordOwners, () => new SQLiteSyncRepository<RMRecordOwner>());
            AddConverter(MultiGeoCommonSyncTable.RMUniqueIdSettings, () => new SQLiteSyncRepository<RMUniqueIdSetting>());
            AddConverter(MultiGeoCommonSyncTable.FSConnectionGroups, () => new SQLiteSyncRepository<FSConnectionGroup>());
            AddConverter(MultiGeoCommonSyncTable.FSConnections, () => new SQLiteSyncRepository<FSConnection>());
            AddConverter(MultiGeoCommonSyncTable.RMFSConnectionAndOwnerRelationships, () => new SQLiteSyncRepository<RMFSConnectionAndOwnerRelationship>());
            AddConverter(MultiGeoCommonSyncTable.RMMiscProfiles, () => new SQLiteSyncRepository<RMMiscProfile>());
            AddConverter(MultiGeoCommonSyncTable.RMChangeClassifications, () => new SQLiteSyncRepository<RMChangeClassification>());
            AddConverter(MultiGeoCommonSyncTable.RMGoogleLabelInfoes, () => new SQLiteSyncRepository<RMGoogleLabelInfo>());
            AddConverter(MultiGeoCommonSyncTable.RMMLTerms, () => new SQLiteSyncRepository<RMMLTerm>());
            AddConverter(MultiGeoCommonSyncTable.RMTermGroupMemberships, () => new SQLiteSyncRepository<RMTermGroupMembership>());
            AddConverter(MultiGeoCommonSyncTable.RMEXOLabels, () => new SQLiteSyncRepository<RMEXOLabel>());
            AddConverter(MultiGeoCommonSyncTable.MultiGeoSettingInfoes, () => new SQLiteSyncRepository<MultiGeoSettingInfo>());
            AddConverter(MultiGeoCommonSyncTable.RMEmailTemplates, () => new SQLiteSyncRepository<RMEmailTemplate>());
            AddConverter(MultiGeoCommonSyncTable.RMFunctionSettings, () => new SQLiteSyncRepository<RMFunctionSetting>());
        }

        private static void AddConverter(MultiGeoCommonSyncTable table, Func<object> factory)
        {
            if (!_converters.ContainsKey(table))
            {
                _converters.Add(table, factory);
            }
        }

        public static bool TryGetConverter(MultiGeoCommonSyncTable syncTable, out Func<object> converterFactory)
        {
            return _converters.TryGetValue(syncTable, out converterFactory);
        }
    }
}
