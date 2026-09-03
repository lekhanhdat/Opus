using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using RAMultiGeo.Interface;

namespace RAMultiGeo.Implement
{
    public class RMTermGroupsHandleDataSQL : IHandleDataSQL
    {
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await TermGroupDao.MultiGeoInsertTermGroupTableAsync(data.OfType<RMTermGroup>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await TermGroupDao.MultiGeoDeleteAllTermGroupAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermGroupDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMTermSetsHandleDataSQL : IHandleDataSQL
    {
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await TermSetDao.MultiGeoInsertTermSetTableAsync(data.OfType<RMTermSet>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await TermSetDao.MultiGeoDeleteAllTermSetAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermSetDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMTermSetMembershipsHandleDataSQL : IHandleDataSQL
    {
        private ITermSetMembershipDao TermSetMembershipDao => PlatformWindsorManager.GetService<ITermSetMembershipDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await TermSetMembershipDao.MultiGeoInsertTermSetMembershipTableAsync(data.OfType<RMTermSetMembership>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await TermSetMembershipDao.MultiGeoDeleteAllTermSetMembershipAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermSetMembershipDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMTermRuleAssociationsHandleDataSQL : IHandleDataSQL
    {
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        public Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return TermRuleAssociationDao.MultiGeoInsertTermRuleAssociationTableAsync(data.OfType<RMTermRuleAssociation>());
        }

        public Task<long> DeleteAllDataAsync()
        {
            return TermRuleAssociationDao.MultiGeoDeleteAllTermRuleAssociationAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermRuleAssociationDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMTermsHandleDataSQL : IHandleDataSQL
    {
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await TermDao.MultiGeoInsertTermTableAsync(data.OfType<RMTerm>());
        }

        public Task<long> DeleteAllDataAsync()
        {
            return TermDao.MultiGeoDeleteAllTermAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMWorkflowDefinitionsHandleDataSQL : IHandleDataSQL
    {
        private IRMWorkflowDefinitionDao WorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await WorkflowDefinitionDao.MultiGeoInsertWorkflowDefinitionTableAsync(data.OfType<RMWorkflowDefinition>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await WorkflowDefinitionDao.MultiGeoDeleteAllWorkflowDefinitionAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await WorkflowDefinitionDao.LoadWorkflowDefinitionsByPager(pageIndex, pageSize);
        }
    }

    public class RMWorkflowStepsHandleDataSQL : IHandleDataSQL
    {
        private readonly IRMWorkflowStepDao WorkflowStepDao = new RMWorkflowStepDao();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await WorkflowStepDao.MultiGeoInsertWorkflowStepTableAsync(data.OfType<RMWorkflowStep>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await WorkflowStepDao.MultiGeoDeleteAllWorkflowStepAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await WorkflowStepDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMWorkflowStepConfigurationsHandleDataSQL : IHandleDataSQL
    {
        private IRMWorkflowDefinitionDao WorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await WorkflowDefinitionDao.MultiGeoInsertWorkflowStepConfigurationTableAsync(data.OfType<RMWorkflowStepConfiguration>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await WorkflowDefinitionDao.MultiGeoDeleteAllWorkflowStepConfigurationAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await WorkflowDefinitionDao.LoadWorkflowStepConfigurationByPager(pageIndex, pageSize);
        }
    }

    public class RMRuleContainersHandleDataSQL : IHandleDataSQL
    {
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await RMRuleDao.MultiGeoInsertRuleContainerTableAsync(data.OfType<RMRuleContainer>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await RMRuleDao.MultiGeoDeleteAllRuleContainerAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await RMRuleDao.LoadRuleContainerByPager(pageIndex, pageSize);
        }
    }

    public class RMRulesHandleDataSQL : IHandleDataSQL
    {
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await RMRuleDao.MultiGeoInsertRuleTableAsync(data.OfType<RMRule>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await RMRuleDao.MultiGeoDeleteAllRuleAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await RMRuleDao.LoadRulesByPager(pageIndex, pageSize);
        }
    }

    public class RMRuleContainerMembershipsHandleDataSQL : IHandleDataSQL
    {
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await RMRuleDao.MultiGeoInsertRuleContainerMembershipTableAsync(data.OfType<RMRuleContainerMembership>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await RMRuleDao.MultiGeoDeleteAllRuleContainerMembershipAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await RMRuleDao.LoadRuleContainerMembershipByPager(pageIndex, pageSize);
        }
    }

    #region Schedules
    //public class RMSchedulesHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMScheduleDao ScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await ScheduleDao.MultiGeoInsertScheduleTableAsync(data.OfType<RMSchedule>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await ScheduleDao.MultiGeoDeleteAllScheduleAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await ScheduleDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region email template
    public class RMEmailTemplatesHandleDataSQL : IHandleDataSQL
    {
        private IRMEamilTemplateDao EmailTemplateDao => PlatformWindsorManager.GetService<IRMEamilTemplateDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await EmailTemplateDao.MultiGeoInsertEmailTemplateTableAsync(data.OfType<RMEmailTemplate>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await EmailTemplateDao.MultiGeoDeleteAllEmailTemplateAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await EmailTemplateDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #endregion
    #region Profile
    //public class RMProfilesHandleDataSQL : IHandleDataSQL
    //{
    //    private IProfileDao ProfileDao => PlatformWindsorManager.GetService<IProfileDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await ProfileDao.MultiGeoInsertProfileTableAsync(data.OfType<RMProfile>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await ProfileDao.MultiGeoDeleteAllProfileAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await ProfileDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion

    public class RMAgentsHandleDataSQL : IHandleDataSQL
    {
        private IRMAgentDao AgentDao => PlatformWindsorManager.GetService<IRMAgentDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await AgentDao.MultiGeoInsertAgentTableAsync(data.OfType<RMAgent>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await AgentDao.MultiGeoDeleteAllAgentAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await AgentDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMKeyValuesHandleDataSQL : IHandleDataSQL
    {
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await KeyValueDao.MultiGeoInsertKeyValueTableAsync(data.OfType<RMKeyValue>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await KeyValueDao.MultiGeoDeleteAllKeyValueAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await KeyValueDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class FSConnectionGroupWithAgentMembershipsHandleDataSQL : IHandleDataSQL
    {
        private IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMembershipDao => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await FSConnectionGroupWithAgentMembershipDao.MultiGeoInsertFSConnectionGroupWithAgentMembershipTableAsync(data.OfType<FSConnectionGroupWithAgentMembership>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await FSConnectionGroupWithAgentMembershipDao.MultiGeoDeleteAllFSConnectionGroupWithAgentMembershipAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await FSConnectionGroupWithAgentMembershipDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMCertificatesHandleDataSQL : IHandleDataSQL
    {
        private IRMCertificateDao CertificateDao => PlatformWindsorManager.GetService<IRMCertificateDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await CertificateDao.MultiGeoInsertCertificateTableAsync(data.OfType<RMCertificate>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await CertificateDao.MultiGeoDeleteAllCertificateAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await CertificateDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #region SettingProfiles
    //public class SettingProfilesHandleDataSQL : IHandleDataSQL
    //{
    //    private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await SettingProfileDao.MultiGeoInsertSettingProfileTableAsync(data.OfType<SettingProfiles>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await SettingProfileDao.MultiGeoDeleteAllSettingProfileAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await SettingProfileDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region GlobalStorageSettings
    //public class RMCPGlobalStorageSettingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await GlobalStorageSettingDao.MultiGeoInsertGlobalStorageSettingTableAsync(data.OfType<RMCPGlobalStorageSetting>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await GlobalStorageSettingDao.MultiGeoDeleteAllGlobalStorageSettingAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await GlobalStorageSettingDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region StorageDeviceInfo
    //public class RMStorageDeviceInfoesHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMStorageDeviceInfoDao StorageDeviceInfoDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await StorageDeviceInfoDao.MultiGeoInsertStorageDeviceInfoTableAsync(data.OfType<RMStorageDeviceInfo>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await StorageDeviceInfoDao.MultiGeoDeleteAllStorageDeviceInfoAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await StorageDeviceInfoDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region CPExportSettings
    //public class RMCPExportSettingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IExportSettingsDao CPExportSettingDao => PlatformWindsorManager.GetService<IExportSettingsDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await CPExportSettingDao.MultiGeoInsertExportSettingsTableAsync(data.OfType<RMCPExportSetting>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await CPExportSettingDao.MultiGeoDeleteAllExportSettingsAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await CPExportSettingDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region PersonalSettings
    //public class RMPersonalSettingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IPersonalSettingDao PersonalSettingsDao => PlatformWindsorManager.GetService<IPersonalSettingDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await PersonalSettingsDao.MultiGeoInsertPersonalSettingTableAsync(data.OfType<RMPersonalSetting>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await PersonalSettingsDao.MultiGeoDeleteAllPersonalSettingAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await PersonalSettingsDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    public class RMLnkUserGroupsHandleDataSQL : IHandleDataSQL
    {
        private ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await LnkUserGroupDao.MultiGeoInsertLnkUserGroupTableAsync(data.OfType<RMLnkUserGroup>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await LnkUserGroupDao.MultiGeoDeleteAllLnkUserGroupTableAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await LnkUserGroupDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMAccountsHandleDataSQL : IHandleDataSQL
    {
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await AccountDao.MultiGeoInsertAccountTableAsync(data.OfType<RMAccount>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await AccountDao.MultiGeoDeleteAllAccountAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await AccountDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #region SecurityGroupMemberships
    //public class RMSecurityGroupMembershipsHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMSecurityGroupMembershipDao SecurityGroupMembershipDao => PlatformWindsorManager.GetService<IRMSecurityGroupMembershipDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await SecurityGroupMembershipDao.MultiGeoInsertSecurityGroupMembershipTableAsync(data.OfType<RMSecurityGroupMembership>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await SecurityGroupMembershipDao.MultiGeoDeleteAllSecurityGroupMembershipAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await SecurityGroupMembershipDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion

    public class RMLnkUserRolesHandleDataSQL : IHandleDataSQL
    {
        private ILnkUserRoleDao LnkUserRoleDao => PlatformWindsorManager.GetService<ILnkUserRoleDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await LnkUserRoleDao.MultiGeoInsertLnkUserRoleTableAsync(data.OfType<RMLnkUserRole>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await LnkUserRoleDao.MultiGeoDeleteAllLnkUserRoleAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await LnkUserRoleDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #region Role
    //public class RMRolesHandleDataSQL : IHandleDataSQL
    //{
    //    private IRoleDao RoleDao => PlatformWindsorManager.GetService<IRoleDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await RoleDao.MultiGeoInsertRoleTableAsync(data.OfType<RMRole>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await RoleDao.MultiGeoDeleteAllRoleAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await RoleDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    public class RMRecordOwnersHandleDataSQL : IHandleDataSQL
    {
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await RecordOwnerDao.MultiGeoInsertRecordOwnerTableAsync(data.OfType<RMRecordOwner>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await RecordOwnerDao.MultiGeoDeleteAllRecordOwnerAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await RecordOwnerDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #region SecurityGroup
    //public class RMSecurityGroupsHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await SecurityGroupDao.MultiGeoInsertSecurityGroupTableAsync(data.OfType<RMSecurityGroup>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await SecurityGroupDao.MultiGeoDeleteAllSecurityGroupAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await SecurityGroupDao.LoadSecurityGroupByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region ScopeRoleAssignment
    //public class RMScopeRoleAssignmentsHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await ScopeRoleAssignmentDao.MultiGeoInsertScopeRoleAssignmentTableAsync(data.OfType<RMScopeRoleAssignment>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await ScopeRoleAssignmentDao.MultiGeoDeleteAllScopeRoleAssignmentAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await ScopeRoleAssignmentDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region RMSecurityGroupTermMappings
    //public class RMSecurityGroupTermMappingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await SecurityGroupDao.MultiGeoInsertSecurityGroupTermMappingTableAsync(data.OfType<RMSecurityGroupTermMapping>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await SecurityGroupDao.MultiGeoDeleteAllSecurityGroupTermMappingAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await SecurityGroupDao.LoadSecurityGroupTermMappingByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    #region RMSecurityGroupRuleMappings
    //public class RMSecurityGroupRuleMappingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await SecurityGroupDao.MultiGeoInsertSecurityGroupRuleMappingTableAsync(data.OfType<RMSecurityGroupRuleMapping>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await SecurityGroupDao.MultiGeoDeleteAllSecurityGroupRuleMappingAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await SecurityGroupDao.LoadSecurityGroupRuleMappingByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    public class RMNodeFlagsHandleDataSQL : IHandleDataSQL
    {
        private IRMNodeFlagDao NodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await NodeFlagDao.MultiGeoInsertNodeFlagTableAsync(data.OfType<RMNodeFlag>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await NodeFlagDao.MultiGeoDeleteAllNodeFlagAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await NodeFlagDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMUniqueIdSettingsHandleDataSQL : IHandleDataSQL
    {
        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await UniqueIdSettingDao.MultiGeoInsertUniqueIdSettingTableAsync(data.OfType<RMUniqueIdSetting>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await UniqueIdSettingDao.MultiGeoDeleteAllUniqueIdSettingAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await UniqueIdSettingDao.LoadByPager(pageIndex, pageSize);
        }
    }
    #region FSSettings
    //public class RMFileSystemSettingsHandleDataSQL : IHandleDataSQL
    //{
    //    private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();

    //    public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
    //    {
    //        return await FileSystemSettingDao.MultiGeoInsertFileSystemSettingTableAsync(data.OfType<RMFileSystemSetting>());
    //    }

    //    public async Task<long> DeleteAllDataAsync()
    //    {
    //        return await FileSystemSettingDao.MultiGeoDeleteAllFileSystemSettingAsync();
    //    }

    //    public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
    //    {
    //        return await FileSystemSettingDao.LoadByPager(pageIndex, pageSize);
    //    }
    //}
    #endregion
    public class FSConnectionGroupsHandleDataSQL : IHandleDataSQL
    {
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await FSConnectionGroupDao.MultiGeoInsertFSConnectionGroupTableAsync(data.OfType<FSConnectionGroup>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await FSConnectionGroupDao.MultiGeoDeleteAllFSConnectionGroupAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await FSConnectionGroupDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class FSConnectionsHandleDataSQL : IHandleDataSQL
    {
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await FSConnectionDao.MultiGeoInsertFSConnectionTableAsync(data.OfType<FSConnection>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await FSConnectionDao.MultiGeoDeleteAllFSConnectionAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await FSConnectionDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMFSConnectionAndOwnerRelationshipsHandleDataSQL : IHandleDataSQL
    {
        private IRMFSConnectionAndOwnerRelationshipDao RMFSConnectionAndOwnerRelationshipDao = new RMFSConnectionAndOwnerRelationshipDao();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await RMFSConnectionAndOwnerRelationshipDao.MultiGeoInsertFSConnectionAndOwnerRelationshipTableAsync(data.OfType<RMFSConnectionAndOwnerRelationship>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await RMFSConnectionAndOwnerRelationshipDao.MultiGeoDeleteAllFSConnectionAndOwnerRelationshipAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await RMFSConnectionAndOwnerRelationshipDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMMiscProfilesHandleDataSQL : IHandleDataSQL
    {
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await MiscProfileDao.MultiGeoInsertMiscProfileTableAsync(data.OfType<RMMiscProfile>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await MiscProfileDao.MultiGeoDeleteAllMiscProfileAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await MiscProfileDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMChangeClassificationsHandleDataSQL : IHandleDataSQL
    {
        private IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await ChangeClassificationDao.MultiGeoInsertChangeClassificationTableAsync(data.OfType<RMChangeClassification>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await ChangeClassificationDao.MultiGeoDeleteAllChangeClassificationAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await ChangeClassificationDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMGoogleLabelInfoesHandleDataSQL : IHandleDataSQL
    {
        private IRMGoogleLabelInfoDao GoogleLabelInfoDao => PlatformWindsorManager.GetService<IRMGoogleLabelInfoDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await GoogleLabelInfoDao.MultiGeoInsertGoogleLabelInfoTableAsync(data.OfType<RMGoogleLabelInfo>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await GoogleLabelInfoDao.MultiGeoDeleteAllGoogleLabelInfoAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await GoogleLabelInfoDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMMLTermsHandleDataSQL : IHandleDataSQL
    {
        private IRMMLTermDao MLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await MLTermDao.MultiGeoInsertMLTermTableAsync(data.OfType<RMMLTerm>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await MLTermDao.MultiGeoDeleteAllMLTermAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await MLTermDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMTermGroupMembershipsHandleDataSQL : IHandleDataSQL
    {
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await TermGroupMembershipDao.MultiGeoInsertTermGroupMembershipTableAsync(data.OfType<RMTermGroupMembership>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await TermGroupMembershipDao.MultiGeoDeleteAllTermGroupMembershipAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await TermGroupMembershipDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMEXOLabelsHandleDataSQL : IHandleDataSQL
    {
        private IRMEXOLabelDao EXOLabelDao => PlatformWindsorManager.GetService<IRMEXOLabelDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await EXOLabelDao.MultiGeoInsertEXOLabelTableAsync(data.OfType<RMEXOLabel>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await EXOLabelDao.MultiGeoDeleteAllEXOLabelAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await EXOLabelDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class MultiGeoSettingInfoesHandleDataSQL : IHandleDataSQL
    {

        private IMultiGeoSettingDao MultiGeoSettingInfoDao => PlatformWindsorManager.GetService<IMultiGeoSettingDao>();
        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await MultiGeoSettingInfoDao.MultiGeoInsertMultiGeoSettingTableAsync(data.OfType<MultiGeoSettingInfo>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await MultiGeoSettingInfoDao.MultiGeoDeleteAllMultiGeoSettingAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await MultiGeoSettingInfoDao.LoadByPager(pageIndex, pageSize);
        }
    }

    public class RMFunctionSettingsHandleDataSQL : IHandleDataSQL
    {
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        public async Task<long> BatchInsertDataAsync(IEnumerable<object> data)
        {
            return await FunctionSettingDao.MultiGeoInsertFunctionSettingTableAsync(data.OfType<RMFunctionSetting>());
        }

        public async Task<long> DeleteAllDataAsync()
        {
            return await FunctionSettingDao.MultiGeoDeleteAllFunctionSettingAsync();
        }

        public async Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize)
        {
            return await FunctionSettingDao.LoadByPager(pageIndex, pageSize);
        }
    }
}
