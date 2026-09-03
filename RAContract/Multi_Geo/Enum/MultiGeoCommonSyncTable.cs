using System;

namespace AvePoint.RA.Contract.Multi_Geo.Enum
{
    [Flags]
    public enum MultiGeoCommonSyncTable : long
    {
        None = 0,
        RMTermGroups = 1 << 0,
        RMTermSets = 1 << 1,
        RMTermSetMemberships = 1 << 2,
        RMTermRuleAssociations = 1 << 3,
        RMTerms = 1 << 4,
        RMWorkflowDefinitions = 1 << 5,
        RMWorkflowSteps = 1 << 6,
        RMWorkflowStepConfigurations = 1 << 7,
        RMRuleContainers = 1 << 8,
        RMRules = 1 << 9,
        RMRuleContainerMemberships = 1 << 10,
        RMAgents = 1 << 12,
        RMKeyValues = 1 << 13,
        FSConnectionGroupWithAgentMemberships = 1 << 14,
        RMCertificates = 1 << 15,
        RMLnkUserGroups = 1 << 16,
        RMAccounts = 1 << 17,
        RMEmailTemplates = 1 << 18,
        RMLnkUserRoles = 1 << 19,
        RMRecordOwners = 1 << 20,
        RMUniqueIdSettings = 1 << 21,
        FSConnectionGroups = 1 << 22,
        FSConnections = 1 << 23,
        RMFSConnectionAndOwnerRelationships = 1 << 24,
        RMMiscProfiles = 1 << 25,
        RMChangeClassifications = 1 << 26,
        RMGoogleLabelInfoes = 1 << 27,
        RMMLTerms = 1 << 28,
        RMTermGroupMemberships = 1 << 29,
        RMEXOLabels = 1 << 30,
        MultiGeoSettingInfoes = 1L << 31,
        RMFunctionSettings = 1L << 32,
        AllTable = 0x7FFFFFFFFFFFFFFF
    }
}
