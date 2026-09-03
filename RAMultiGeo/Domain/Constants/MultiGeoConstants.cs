using AvePoint.RA.Contract.Multi_Geo.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAMultiGeo.Domain.Constants
{
    public class MultiGeoConstants
    {
        public const MultiGeoCommonSyncTable AllSyncTable = 
            MultiGeoCommonSyncTable.RMTermGroups | MultiGeoCommonSyncTable.RMTermSets | MultiGeoCommonSyncTable.RMTermSetMemberships | MultiGeoCommonSyncTable.RMTermRuleAssociations | MultiGeoCommonSyncTable.RMTerms
            | MultiGeoCommonSyncTable.RMWorkflowDefinitions | MultiGeoCommonSyncTable.RMWorkflowSteps | MultiGeoCommonSyncTable.RMWorkflowStepConfigurations
            | MultiGeoCommonSyncTable.RMRuleContainers | MultiGeoCommonSyncTable.RMRules | MultiGeoCommonSyncTable.RMRuleContainerMemberships
            | MultiGeoCommonSyncTable.RMAgents | MultiGeoCommonSyncTable.RMKeyValues
            | MultiGeoCommonSyncTable.FSConnectionGroupWithAgentMemberships | MultiGeoCommonSyncTable.RMCertificates
            | MultiGeoCommonSyncTable.RMLnkUserGroups | MultiGeoCommonSyncTable.RMAccounts | MultiGeoCommonSyncTable.RMLnkUserRoles
            | MultiGeoCommonSyncTable.RMRecordOwners 
            | MultiGeoCommonSyncTable.RMUniqueIdSettings  | MultiGeoCommonSyncTable.FSConnectionGroups | MultiGeoCommonSyncTable.FSConnections
            | MultiGeoCommonSyncTable.RMFSConnectionAndOwnerRelationships | MultiGeoCommonSyncTable.RMMiscProfiles | MultiGeoCommonSyncTable.RMChangeClassifications
            | MultiGeoCommonSyncTable.RMGoogleLabelInfoes | MultiGeoCommonSyncTable.RMMLTerms | MultiGeoCommonSyncTable.RMTermGroupMemberships | MultiGeoCommonSyncTable.RMEXOLabels | MultiGeoCommonSyncTable.MultiGeoSettingInfoes
            | MultiGeoCommonSyncTable.RMEmailTemplates | MultiGeoCommonSyncTable.RMFunctionSettings;
    }
}
