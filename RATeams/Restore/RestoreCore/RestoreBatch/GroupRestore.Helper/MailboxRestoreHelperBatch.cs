/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace Office365GroupRestore
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.GCommon.Utility;
    using AvePoint.Metadata;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.DB.Dao.Extension;
    using AvePoint.Wrapper.Common;
    using DocumentFormat.OpenXml.Office2010.Excel;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using HtmlAgilityPack;
    using Job.ModernManagement.Report;
    using M365.Wrapper.Backup.Auth.Common;
    using Microsoft.Graph.Models.ODataErrors;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Utility;
    using Microsoft365.Configuration;
    using Microsoft365.Graph.Service;
    using Microsoft365.SharePoint.Extension;
    using Newtonsoft.Json.Linq;
    using Office365GroupBackup;
    using Polly;
    using RAArchiverCommon;
    using RAArchiverCommon.TeamsController;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;
    using Util.MSAzure;
    using Group = Microsoft.Graph.Models.Group;
    using Teams = Microsoft.Graph.Models.Team;
    using TeamSpecialization = Microsoft.Graph.Models.TeamSpecialization;
    using TeamVisibilityType = Microsoft.Graph.Models.TeamVisibilityType;

    public class MailboxRestoreHelperBatch : BaseRestoreHelperBatch
    {
        DiscoverMailboxEntity restoreEntity;

        private GraphService _graphService;
        private bool _hasTeamsMigratePermission;

        public IExchangeRestoreIndexService RestoreIndexService => RContainer.ExchangeRestoreIndexService;

        public MailboxRestoreHelperBatch()
        {

        }
        public MailboxRestoreHelperBatch(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {

        }

        public MailboxRestoreHelperBatch Build(IReportCenter report)
        {
            this.Report = report;
            
            return this;
        }

        public MailboxRestoreHelperBatch Build(RestoreConfig config)
        {
            this.Config = config;
            M365APIService = new M365APIService(Config.BposInfo, null);
            return this;
        }

        private string currentUser;
        
        protected override void InitReport()
        {
            base.InitReport();
            ReportDto.Type = Config.IsMicrosoftTeams ? ReportNodeHeader.Team : ReportNodeHeader.Group;
            I18NDataCollector = new I18NParameterCollector();
        }

        public void SendReport(IEnumerable<ExchangeDataBlockForBatch> dataBlockList)
        {
            FileHeader = dataBlockList.First().FileHeader; 
            InitReport();
            ReportDto.Path = RestoreConfig.CurrentRestoreMailbox;
            Report.AddRestoreReport(ReportDto);
        }

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            try
            {
                InitReport();

                logger.Info($"Start to restore {ReportDto.Type}, name:{ReportDto.Name}, path: {ReportDto.Path}, size:{ReportDto.Size}");

                var restoreData = dataCollection.First().RestoreData;

                var entityString = restoreData.MetadataLists.First()?.GetMetadata<string>();

                logger.Info("Entity String:{0}", entityString);

                if (!string.IsNullOrEmpty(entityString))
                {
                    var entity = DeserializeToEntityV2(entityString);
                    entity.IsTeamsGroup = Config.IsMicrosoftTeams;
                    restoreEntity = new DiscoverMailboxEntity(entity);
                    _GroupAdditionalProperties["Archived"] = entity.AdditionalProperties;

                    InitCurrentMailbox(entity);

                    ReportDto.Path = RestoreConfig.CurrentRestoreMailbox;

                    var authObjAPP = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox);
                    if (authObjAPP is AOSTokenAuthObjectV2 provider)
                    {
                        provider.TestConnectivity(true);
                        _hasTeamsMigratePermission = JwtUtil.GetRolesFromToken(provider.GetAccessToken())?.Contains("Teamwork.Migrate.All") ?? false;
                        _graphService = new GraphService(Endpoints.GetEndpoints(provider.CloudType).MicrosoftGraph, provider.TokenProvider);
                    }

                    #region ---AuthObj---
                    //var authObjAPP = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox);
                    //if (authObjAPP is AOSTokenAuthObjectV2 provider)
                    //    provider.TestConnectivity(true);
                    //var authObjSA = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
                    //var authObj4Plan = AuthorizationManager.GetPlannerAuth(RestoreConfig.CurrentRestoreMailbox);
                    //mS365GroupServiceWithSA = ExchangeServiceFactory.CreateMicrosoft365Group(authObjSA);
                    //I18NDataCollector.UpdateData(DynamicDataKey.UserName, authObjSA.UserName);
                    //I18NDataCollector.UpdateData(DynamicDataKey.PlannerUserName, authObj4Plan.UserName);
                    //Config.BposInfo.CustomerId = TenantLocalValue.LogonGroupId;
                    #endregion

                    GetSourceMicrosoftteams(restoreData);


                    //exchangeMicrosoftTeams = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObjAPP);
                    //logger.Info("Current certification type: {0}, Scope of use : Restore group/teams metadata.", authObjAPP.AuthType);
                    try
                    {
                        //exchangePlanner = ExchangeServiceFactory.CreateOffice365Planner(authObj4Plan);
                        //logger.Info("Current certification type: {0}, Scope of use : Restore planner data.", authObj4Plan.AuthType);
                        _NewlyPlannerPlanIds = new List<string>();
                    }
                    catch (ArgumentException ex)
                    {
                        logger.Warn("Can not use AppToken to Create ExchangePlannerService,{0}", ex);
                    }


                    _GroupSiteUrl = string.Empty;
                    _GroupSiteFilesUrl = string.Empty;
                    _SiteNotFound = false;
                    try
                    {
                        _GroupId = String.Empty;
                        _ExistedTeamUsers = null;

                        logger.Info($"ContainerConflictResolution : {Config.ContainerConflictResolution}");
                        if (this.Config.IsYammerGroup)
                        {
                            RestoreYammerGroup(entity);
                        }
                        else
                        {
                            logger.Info("Is Teams Group: {0}.", entity.IsTeamsGroup);
                            if (!entity.IsTeamsGroup)
                            {
                                RestoreMicrosoft365Group(entity);
                            }
                            else
                            {
                                if (_SpecialTeamAdapter.IsSpecialTeam) DoMapping(_SourceMSTeamsEntity.TeamMembers);
                                RestoreMicrosoftTeams(entity);
                            }
                        }
                        TeamsRestoreState.RestoreGroupSite = _GroupSiteUrl;
                    }
                    catch (Exception ex)
                    {
                        TryCompleteMigration();

                        if (ProcessException(RestoreConfig.CurrentRestoreMailbox, entity, ex))
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to restore the mailbox. error:{ex}");
                ReportDto.Status = ReportStatus.Failed;
                ReportDto.ErrorMessage = GenerateSpecifiedException(ex);
                RestoreConfig.CurrentRestoreMailbox = null;
            }
            finally
            {
                TeamsRestoreState.IsGroupSiteNewlyCreated = _IsNewlyCreatedGroupSite;
                Report.AddRestoreReport(ReportDto);
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
            }
        }

        protected void InitCurrentMailbox(Office365GroupEntityV2 entity)
        {
            _SpecialTeamAdapter = new SpecialCustomersAdapter(Config, entity.SmtpAddress);
            //BposInfos = new Dictionary<string, BposInfo>();
            if (Config.RestoreType == EORestoreType.InPlace)
            {

                RestoreConfig.CurrentRestoreMailbox = entity.SmtpAddress;

                if (_SpecialTeamAdapter.IsSpecialTeam)
                {
                    _SpecialTeamAdapter.AdaptToTeamMetadata();
                    RestoreConfig.CurrentRestoreMailbox = entity.SmtpAddress = _SpecialTeamAdapter.RegenerateTeamAddress();
                }
            }
            //if (Config.RestoreType == EORestoreType.OutOfPlace)
            //{
            //    BposInfos = RestoreConfig.OutPlaceEmailBposInfoMap;
            //    entity.SmtpAddress = RestoreConfig.CurrentRestoreMailbox = BposInfos.Keys.First();
            //}

            //BposInfos = BposInfos.ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);

            //AuthorizationManager.Init(BposInfos, 0, AuthScope.MicrosoftGraph, AuthScope.EWS, AuthScope.ExchangePS);
            AuthorizationManager.Init(
                        new Dictionary<string, BposInfo> { [RestoreConfig.CurrentRestoreMailbox] = Config.BposInfo },
                        0,
                        [AuthScope.MicrosoftGraph, AuthScope.EWS, AuthScope.ExchangePS]
                        );
        }

        public Office365GroupEntityV2 DeserializeToEntityV2(string entityString)
        {
            try
            {
                return SerializerHelper.DeserializeByDataContractSerializer<Office365GroupEntityV2>(entityString);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while deserializing to entity v2, begin to use v1, error: {0}.", ex);
                try
                {
                    var entityV1 = SerializerHelper.DeserializeByDataContractSerializer<Office365GroupEntity>(entityString);
                    return ConvertToV2(entityV1);
                }
                catch (Exception e)
                {
                    logger.Info("The entity is not Office365GroupEntity, error: {0}.", e);
                    throw new Exception("RM_JM_RestoreFaild_IndexNotExsit_ErrorMessage");
                    //return null;
                }
            }
            Office365GroupEntityV2 ConvertToV2(Office365GroupEntity entityV1)
            {
                return new Office365GroupEntityV2()
                {
                    OwnerCount = entityV1.OwnerCount,
                    Description = entityV1.Description,
                    DisplayName = entityV1.DisplayName,
                    MailboxGuid = entityV1.MailboxGuid,
                    SmtpAddress = entityV1.SmtpAddress,
                    SendToMeida = entityV1.SendToMeida,
                    IsTeamsGroup = entityV1.IsTeamsGroup,
                    Classification = entityV1.Classification,
                    AccessType = (GroupAccessTypeV2)entityV1.AccessType,
                    ExternalDirectoryObjectId = entityV1.ExternalDirectoryObjectId,
                    AdditionalProperties = new GroupAdditionalPropertiesV2()
                    {
                        ExternalMemberCount = entityV1.AdditionalProperties.ExternalMemberCount,
                        IsGroupMembershipHidden = entityV1.AdditionalProperties.IsGroupMembershipHidden,
                        //IsMembershipDynamic = entityV1.AdditionalProperties.IsMembershipDynamic,
                        MembershipRule = entityV1.AdditionalProperties.MembershipRule,
                        MembershipRuleProcessingState = entityV1.AdditionalProperties.MembershipRuleProcessingState,
                        SubscriptionEnabled = entityV1.AdditionalProperties.SubscriptionEnabled
                    },
                    MailboxSettings = new MailboxSettingsV2()
                    {
                        AlwaysSubscribeMembersToCalendarEvents = entityV1.MailboxSettings.AlwaysSubscribeMembersToCalendarEvents,
                        AutoSubscribeNewMembers = entityV1.MailboxSettings.AutoSubscribeNewMembers,
                        ExternalSendersEnabled = entityV1.MailboxSettings.ExternalSendersEnabled,
                        MailboxCultureName = entityV1.MailboxSettings.MailboxCultureName
                    },
                    UserGroupRelationship = new UserGroupRelationshipV2()
                    {
                        IsMember = entityV1.UserGroupRelationship.IsMember,
                        IsOwner = entityV1.UserGroupRelationship.IsOwner,
                        IsSubscribed = entityV1.UserGroupRelationship.IsSubscribed
                    },
                    GroupMemberList = entityV1.GroupMemberList.Select(member => new GroupMemberV2() { IsOwner = member.IsOwner, UserName = member.UserName }).ToList(),
                    GroupResources = entityV1.GroupResources.Select(groupResource => new GroupResourceV2() { Type = (GroupResouceTypeV2)groupResource.Type, Url = groupResource.Url }).ToArray(),
                    UnifiedGroupSKU = new UnifiedGroupSKUV2() { GroupType = entityV1.UnifiedGroupSKU.ToString(), IsNull = false }
                };
            }
        }

        public void UpdateGroupInformation(Office365GroupEntityV2 entity, string groupId, Boolean isExist)
        {
            var needUpdate = false;
            var isExistMembemshipDynamic = false;
            if (isExist)
            {
                if (Config.ContainerConflictResolution != EOConflictResolutionType.Merge) return;
                needUpdate = true;
                var existingGroup = M365APIService.GroupService.FindGroup(entity.SmtpAddress);
                _GroupId = existingGroup.ExternalDirectoryObjectId;
                if (existingGroup.AdditionalProperties != null)
                {
                    var membershipInfo = existingGroup.AdditionalProperties;
                    _GroupAdditionalProperties["Exist"] = membershipInfo;
                    isExistMembemshipDynamic = membershipInfo.IsMembershipDynamic;
                }
            }
            AddO365GroupOwnerAndMembers(entity.GroupMemberList, groupId, isExistMembemshipDynamic);
            if (needUpdate) M365APIService.GroupService.UpdateGroupInfo(groupId, entity);
            if (M365APIService.GroupService4ServiceAccount != null)
            {
                M365APIService.GroupService4ServiceAccount.UpdateGroupSettings(groupId, entity); //update group settings
            }
        }
        public void AddO365GroupOwnerAndMembers(List<GroupMemberV2> groupMembers, string groupId, bool isMembershipDynamic = false)
        {
            if (!groupMembers.Any()) return;
            if (Config.RestoreType == EORestoreType.OutOfPlace)
            {
                DoMapping(groupMembers);
            }
            M365APIService.GroupService.AddO365GroupOwnerAndMembers(groupId, groupMembers, Config.SpecifyUserList, isMembershipDynamic);
        }
        public void DoMapping(List<GroupMemberV2> groupMembers)
        {
            var tempDic = new Dictionary<String, GroupMemberV2>();
            try
            {
                groupMembers.ForEach(user =>
                {
                    var mUser = string.Empty;
                    if (Config.UserMapping.TryGetValue(user.UserName, out mUser)) //user mapping
                    {
                        logger.Info("User Mapping:{0} --> {1}", user.UserName, mUser);
                        user.UserName = mUser;
                    }
                    else
                    {
                        var mDomain = string.Empty;
                        var ud = user.UserName.Split('@');
                        var userName = ud[0];
                        var domain = ud[1];
                        if (Config.DomainMapping.TryGetValue(domain, out mDomain)) //domain mapping
                        {
                            mUser = string.Format("{0}@{1}", userName, mDomain);
                            logger.Info("Domain Mapping:{0} --> {1}", user, mUser);
                            user.UserName = mUser;
                        }
                    }
                    tempDic.TryAdd(user.UserName, user);
                });
                var defaultUser = Config.DefaultUser4Mapping;
                if (!String.IsNullOrWhiteSpace(defaultUser)) tempDic.TryAdd(defaultUser, new GroupMemberV2 { UserName = defaultUser, IsOwner = true });
                groupMembers = tempDic.Values.ToList();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when mapping users,ex :{0}", ex);
            }
        }
        public void DoMapping(List<TeamMember> teamMembers)
        {
            var tempDic = new Dictionary<String, TeamMember>();
            try
            {
                teamMembers.ForEach(user =>
                {
                    var mUser = string.Empty;
                    if (Config.UserMapping.TryGetValue(user.MailboxAddress, out mUser)) //user mapping
                    {
                        logger.Info("User Mapping:{0} --> {1}", user.MailboxAddress, mUser);
                        user.MailboxAddress = mUser;
                    }
                    else
                    {
                        var mDomain = string.Empty;
                        var ud = user.MailboxAddress.Split('@');
                        var userName = ud[0];
                        var domain = ud[1];
                        if (Config.DomainMapping.TryGetValue(domain, out mDomain)) //domain mapping
                        {
                            mUser = string.Format("{0}@{1}", userName, mDomain);
                            logger.Info("Domain Mapping:{0} --> {1}", user, mUser);
                            user.MailboxAddress = mUser;
                        }
                    }
                    tempDic.TryAdd(user.MailboxAddress, user);
                });
                var defaultUser = Config.DefaultUser4Mapping;
                if (!String.IsNullOrWhiteSpace(defaultUser)) tempDic.TryAdd(defaultUser, new TeamMember { MailboxAddress = defaultUser, RoleType = TeamMemberRoleType.Owner });
                teamMembers = tempDic.Values.ToList();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when mapping users,ex :{0}", ex);
            }
        }
        private bool ProcessException(string restoreEntityTitle, Office365GroupEntityV2 entity, Exception ex)
        {
            if (ex.Message.Contains("Another object with the same value for property mailNickname already exists"))
            {
                if (entity.IsTeamsGroup) throw new Exception("Agent.Teams.TeamAddressConflict_24A5CEB7-CD07-4A68-9032-87051FA1F9E0");
                if (M365APIService.GroupService.TryGetGroupCurtInfo(restoreEntityTitle, out TeamInfo groupInfo))
                {
                    logger.Info("Group:[{0}] has already exist.", restoreEntityTitle);
                    ProcessExistingGroup(restoreEntityTitle, entity, groupInfo.GroupId);
                    return false;
                }
                else throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Office365Group.SameNameUserMailbox_D8F898C3-02B2-400E-B380-29B1016C47DE", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Code: AccessDenied"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.TeamsExistButNotMember_525D7CDD-00AA-42EA-903F-76A39D746AE2", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Agent.Teams.AliasBeOccupied_19083F0C-6EF3-4701-A997-78CAC2BBCA15"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.AliasBeOccupied_19083F0C-6EF3-4701-A997-78CAC2BBCA15", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Agent.Teams.FailedGetTeamGroup_C0D9D010-8E65-435D-A6B3-2874CC3AED03"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.FailedGetTeamGroup_C0D9D010-8E65-435D-A6B3-2874CC3AED03", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Agent.Teams.FailedGetTeamSite_FE69A800-35AD-45B0-A766-4E2BDFD4F8ED"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.FailedGetTeamSite_FE69A800-35AD-45B0-A766-4E2BDFD4F8ED", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Agent.Teams.FailedUpdateTeamAddress_E5FE21A0-E10A-4E55-B38B-2E03D54975CB"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.FailedUpdateTeamAddress_E5FE21A0-E10A-4E55-B38B-2E03D54975CB", restoreEntityTitle));
            }
            else if (ex.Message.Contains("Agent.Teams.FailedUpdateTeamAddress_D9EED4BD-CD89-4D5F-8290-BBDDCA319F5B"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.FailedUpdateTeamAddress_D9EED4BD-CD89-4D5F-8290-BBDDCA319F5B", restoreEntityTitle));
            }
            else if (ex.Message.Contains("The property is missing a required prefix/suffix per your organization's Group naming requirements"))
            {
                //if (!entity.IsTeamsGroup && mS365GroupService.TryGetGroupCurtInfo(restoreEntityTitle, out TeamInfo groupInfo))
                //{
                //    logger.Info("Group:[{0}] has already exist.", restoreEntityTitle);
                //    ProcessExistingGroup(restoreEntityTitle, entity, groupInfo.GroupId);
                //    return false;
                //}
                throw new Exception("Agent.Office365Group.GroupNamingPolicy_4EAA688B-D47F-44F4-9ACA-3ACEB96B98A6");
            }
            else if (ex.Message.Contains("User Login. Teams is disabled in user licenses"))
            {
                throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Teams.NoTeamLicense_EF3C27D2-2E13-4B8F-A5E2-064F55D9CD32", I18NDataCollector.GetData(DynamicDataKey.UserName)));
            }
            return true;
        }
        private void ProcessExistingGroup(string restoreEntityTitle, Office365GroupEntityV2 entity, string groupId)
        {
            if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Skip)
            {
                _GroupAdditionalProperties["Exist"] = null;
                ReportDto.Status = ReportStatus.Skipped;
                ReportDto.Option = null;//stodo// RestoreOption.Skipped.GetEnumDescription();
                ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.Office365Group.GroupExist_F125D124-6A2A-4C57-8364-F9964F0CA07C", restoreEntityTitle);
            }
            else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
            {
                UpdateGroupInformation(entity, groupId, true);
                ReportDto.Status = ReportStatus.Success;
            }
            TeamsRestoreState.IsAllowRestoreGroupSite = true;
        }
        private void GetSourceMicrosoftteams(ExchangeRestoreDataForBatch restoreData)
        {
            foreach (var md in restoreData.MetadataLists)
            {
                if (md.MetadataType == AvePoint.Metadata.AveMetadataType.ExchangeMicrosoftTeams)
                {
                    _SourceMSTeamsEntity = SerializerHelper.DeserializeByDataContractSerializer<MicrosoftTeamsEntity>(md.GetMetadata<string>());
                }
            }
            if (_SourceMSTeamsEntity?.TeamMembers is not null && Config.IsSpecifyUser)
            {
                _SourceMSTeamsEntity.TeamMembers.AddRange(Config.SpecifyUserList.Select(u =>
                    new TeamMember { UserId = u.Id, MailboxAddress = u.UserPrincipalName, RoleType = TeamMemberRoleType.Owner }));
            }
        }
        private void RestoreMicrosoft365Group(Office365GroupEntityV2 entity)
        {
            TeamInfo groupInfo = null;
            bool needUpdateDataLocation = WhetherMeetRequireForMultiGeo(entity);
            logger.Info("Whether meet require for group multiGeo: {0}.", needUpdateDataLocation.ToString());
            if (entity.GroupResources.Length > 0 && !string.IsNullOrEmpty(entity.GroupResources[0].Url)) _GroupSiteUrl = entity.GroupResources[0].Url;
            if (needUpdateDataLocation)
            {
                groupInfo = M365APIService.GroupService.CreateO365Group(entity, needUpdateDataLocation);
                
            }
            else
            {
                groupInfo = M365APIService.GroupService.CreateO365Group(entity);
            }
            _IsNewlyCreatedGroupSite = true;
            //SameNameUserMailbox 时仍然会创建出group。例如 user mailbox：nat@M365x634190.onmicrosoft.com, created group ：nat5432@M365x634190.onmicrosoft.com 
            var alias = groupInfo.Mail.Substring(0, groupInfo.Mail.LastIndexOf('@') + 1);
            if (!RestoreConfig.CurrentRestoreMailbox.StartsWith(alias)) throw new Exception("Another object with the same value for property mailNickname already exists.");

            if (!UpdateTeamSmtpAddressV2(entity, groupInfo.Mail, groupInfo.GroupId))
            {
                logger.Error("The group new address is not the same as the current address. Failed to update group address. Group: [{0}]. ", entity.SmtpAddress);
                throw new Exception("Agent.Teams.FailedUpdateTeamAddress_E5FE21A0-E10A-4E55-B38B-2E03D54975CB");
            }
            UpdateGroupInformation(entity, groupInfo.GroupId, false);
            TeamsRestoreState.IsAllowRestoreGroupSite = true;
            _GroupId = groupInfo.GroupId;
            EnsureOriginalSiteUrl();
            ReportDto.Status = ReportStatus.Success;
            logger.Info("Create Group:[{0}] successed. SiteUrl: [{1}]", groupInfo.Mail, _GroupSiteUrl);
        }

        private void RestoreMicrosoftTeams(Office365GroupEntityV2 entity)
        {
            _TeamsChannels = new List<string>();
            SiteUrlDic = new Dictionary<string, string>();
            EntityIdDic = new Dictionary<string, string>();
            _PlannerTabs = new List<PlannerTabUpdateObj>();
            _FileTabs = new List<FileTabUpdateObj>();
            if (entity.GroupResources.Length > 0 && !string.IsNullOrEmpty(entity.GroupResources[0].Url)) _GroupSiteUrl = entity.GroupResources[0].Url;
            if (this.Config.RestoreType == EORestoreType.InPlace)
            {
                logger.Info("Current restore type: [{0}]. ", this.Config.RestoreType);
                if (M365APIService.GroupService.IsO365GroupExist(entity.SmtpAddress))
                {
                    logger.Info("TeamGroup exists in the destination. TeamName: [{0}]. ", entity.SmtpAddress);
                    var groupDetails = M365APIService.GroupService.GetO365GroupDetail(entity.SmtpAddress);
                    groupDetails.IsTeamsGroup = Config.IsMicrosoftTeams;
                    _GroupId = groupDetails?.ExternalDirectoryObjectId ?? entity.ExternalDirectoryObjectId;
                    if (TeamsService.IsTeamExist(_GroupId))
                    {
                        UpdateTeam(entity, groupDetails);
                    }
                    else
                    {
                        CreateTeam(entity);
                    }
                }
                else
                {
                    CheckShouldUseMigration();
                    CreateTeam(entity);
                    _IsNewlyCreatedGroupSite = true;
                }
            }
            else if (this.Config.RestoreType == EORestoreType.OutOfPlace)
            {
                logger.Info("Current restore type: [{0}]. ", this.Config.RestoreType);
                var groupDetails = M365APIService.GroupService.GetO365GroupDetail(RestoreConfig.CurrentRestoreMailbox);
                groupDetails.IsTeamsGroup = Config.IsMicrosoftTeams;
                _GroupId = groupDetails != null && !string.IsNullOrEmpty(groupDetails.ExternalDirectoryObjectId) ? groupDetails.ExternalDirectoryObjectId : string.Empty;
                GetTeamUrls(TeamsService, groupDetails, false);
                _ExistedChannels = TeamsService.ListTeamChanneslWithDetails(_GroupId);
                _ExistedChannels.ForEach(eChannel => eChannel.ChannelTabs.Where(cT => cT.TeamsAppId.Equals(BuiltInTabTeamAppsId.Planner, StringComparison.OrdinalIgnoreCase)).ForEach(cTab =>
                {
                    var planTabConfig = TabFactory.CreateTabConfig(cTab, new Dictionary<string, string>(), RestoreConfig.TenantIdMap).Configuration;
                    if (!string.IsNullOrEmpty(planTabConfig.EntityId)) _PlannerTabs.Add(new PlannerTabUpdateObj() { ChannelId = eChannel.Id, TabId = cTab.Id, PlannerId = planTabConfig.EntityId, ChannelTab = cTab, });
                }));

            }
            if (entity.GroupResources.Length > 0 && !string.IsNullOrEmpty(entity.GroupResources[0].Url)) SiteUrlDic.Add(entity.GroupResources[0].Url, _GroupSiteUrl);
            InitGreneralChannelName(_SourceMSTeamsEntity);
            _TeamIntenalId = TeamsService.GetTeamIntenalId(_GroupId);
            GetPrimaryChannelFilesFolder(_GroupId, _TeamIntenalId);
            //CacheTeamMembers();
            TeamsRestoreState.IsAllowRestoreGroupSite = true;
            logger.Info("Fininsh to restore team. TeamName: [{0}]. TeamSiteUrl: [{1}]. TeamSiteFilesUrl: [{2}]. ", entity.SmtpAddress, _GroupSiteUrl, _GroupSiteFilesUrl);
        }

        private void CheckShouldUseMigration()
        {
            try
            {
                if (!Config.UseImportApi || Config.RestoreConversationType != RestoreConversationType.Original) return;

                var oldMessageDate = RestoreIndexService.GetOldestMessageCreateDate();
                if (oldMessageDate == null) return;

                if (!_hasTeamsMigratePermission) return;

                UseMigrationMode = true;
                OldestMessageDate = oldMessageDate.Value;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to check whether use migration mode, error: {e}");
            }
            finally
            {
                logger.Info($"Use migration mode: {UseMigrationMode}, OldestMessageDate: {OldestMessageDate}");
            }
        }

        private void EnsureOriginalSiteUrl()
        {
            if (!string.IsNullOrEmpty(_GroupSiteUrl))
            {
                var groupSiteUrl = M365APIService.GroupService.GetGroupSiteURLByGroupId(_GroupId);
                if (!groupSiteUrl.Equals(_GroupSiteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn("The group site url is different from original one, current url: {0}, original url: {1}. So we will change the url.", groupSiteUrl, _GroupSiteUrl);
                    try
                    {
                        UpdateSiteUrl(Config.BposInfo, groupSiteUrl, _GroupSiteUrl);
                        // Ensure the site url is updated successfully before next step. Otherwise, the following steps may be affected, such as restore data to site, update channel site url, etc.
                        string currentSiteUrl = string.Empty;
                        bool isUpdated = false;
                        AvePoint.Wrapper.Common.AveTaskRetryHelper helper = new(5, true, 10000);
                        helper.AddRetryExceptionDetail(new Exception("SiteUrlNotUpdate"));
                        helper.ExecuteWithRetryMechanismV3(() =>
                        {
                            logger.Info("Checking the site url after update");
                            currentSiteUrl = this.M365APIService.GroupService.GetGroupSiteURLByGroupId(this._GroupId);
                            if (string.Equals(currentSiteUrl, this._GroupSiteUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.Info("The site url has been updated successfully. Current url: {0}.", currentSiteUrl);
                                isUpdated = true;
                            }
                            else
                            {
                                throw new Exception("SiteUrlNotUpdate");
                            }
                        });

                        if (!isUpdated)
                            logger.Warn("The site url is not updated to expected url after retries. Current url: {0}, Expected url: {1}.", currentSiteUrl, _GroupSiteUrl);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Failed to update site url, so skip it. ex: {ex}");
                    }

                    logger.Info($"Finish to ensure original site url. Current url: {groupSiteUrl}, Original url: {_GroupSiteUrl}. ");
                }
            }
        }
        private void UpdateSiteUrl(BposInfo bposInfo, string oldUrl, string newUrl)
        {
            string adminUrl = WebUtil.GetSPAdminUrl(oldUrl, string.Empty);
            bposInfo.UserAccountInfo.AdminUrl = adminUrl;
            var tokenProvider = bposInfo.ConvertToAveBPOSAccountInfo().Convert2TokenProvider();
            Uri uri = new Uri(adminUrl + "/_api/SiteRenameJobs?api-version=1.5.4");
            JObject bodyObject = new JObject();
            bodyObject["SourceSiteUrl"] = oldUrl;
            bodyObject["TargetSiteUrl"] = newUrl;
            bodyObject["TargetSiteTitle"] = null;
            bodyObject["Option"] = 0;
            bodyObject["Reserve"] = null;
            bodyObject["SkipGestures"] = null;
            bool needRetry;
            int retryTimes = 0;
            do
            {
                needRetry = false;
                var request = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(uri);
                request.SetToken(adminUrl, tokenProvider, false);
                request.Method = "POST";
                request.Accept = "application/json;odata.metadata=minimal";
                request.ContentType = "application/json;charset=UTF-8";
                var buffer = Encoding.UTF8.GetBytes(bodyObject.ToString());
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
                try
                {
                    using var result = request.GetResponse() as HttpWebResponse;
                    if (result == null ||
                        (result.StatusCode != HttpStatusCode.OK &&
                         result.StatusCode != HttpStatusCode.Created))
                    {
                        throw new WebException($"Failed to update site url. Url:{adminUrl}, Body:{bodyObject}");
                    }
                    else
                    {
                        logger.Info($"Site url updated successfully. Old Url: {oldUrl}. Original Url: {newUrl}. New Url: {bodyObject["TargetSiteUrl"]}");
                        if (!string.IsNullOrEmpty(bodyObject["TargetSiteUrl"]?.ToString()))
                        {
                            logger.Info($"UpdateSiteUrl.Site url updated successfully. Old Url: {oldUrl}. Original Url: {newUrl}. New Url: {bodyObject["TargetSiteUrl"]}");
                            _GroupSiteUrl = bodyObject["TargetSiteUrl"]!.ToString();
                            TeamsRestoreState.mappingSiteURLs[newUrl] = _GroupSiteUrl;
                        }
                    }
                }
                catch (WebException e)
                {
                    var resp = e.Response;
                    if (resp != null)
                    {
                        using var reader = new StreamReader(resp.GetResponseStream());
                        var error = reader.ReadToEnd();
                        if (error.Contains("This site address is unavailable.", StringComparison.OrdinalIgnoreCase))
                        {
                            needRetry = true;
                            retryTimes++;
                            bodyObject["TargetSiteUrl"] = bodyObject["TargetSiteUrl"] + Random.Shared.Next(0, 9).ToString();
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            } while (needRetry && retryTimes < 5);
        }

        /// <summary>
        /// 1. Only after getting PrimaryChannel filessFolder can private/shared channel filesfolder be obtained.
        /// 2. First getting the PrimaryChannel filesFolder and then creating a private/shared channel, you can immediately obtain the filesFolder for the new channel.
        /// 3. First create a private/shared channel and then obtain the PrimaryChannel filesFolder. Approximately 5 minutes after creating the channel, the filesFolder for the new channel can be obtained.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="teamIntenalId"></param>
        private void GetPrimaryChannelFilesFolder(string teamId, string teamIntenalId)
        {
            try
            {
                var url = Polly.Policy.Handle<GraphAPIException>(e => e.Message.Contains("Folder location for this channel is not ready yet, please try again later"))
                     .WaitAndRetry(3, (retryCount) => TimeSpan.FromSeconds(20))
                     .Execute(() => TeamsService.GetChannelFilesUrl(teamId, teamIntenalId));
                logger.Info($"PrimaryChannel files folder url: {url}");
            }
            catch (Exception e)
            {
                logger.Warn($"Unable to obtain PrimaryChannelFilesFolder, details: {e}");
            }
        }

        private void RestoreYammerGroup(Office365GroupEntityV2 entity)
        {
            logger.Info($"The current group is Yammer group. Address: [{entity.SmtpAddress}]");
            if (this.Config.RestoreType == EORestoreType.InPlace)
            {
                logger.Info("Current restore type: [{0}]. ", this.Config.RestoreType);
                if (M365APIService.GroupService.IsO365GroupExist(entity.SmtpAddress))
                {
                    logger.Info("Group with same address exists in the destination. Name: [{0}]. ", entity.SmtpAddress);
                    var groupDetails = M365APIService.GroupService.GetO365GroupDetail(entity.SmtpAddress);
                    groupDetails.IsTeamsGroup = Config.IsMicrosoftTeams;
                    if (groupDetails != null)
                    {
                        if (groupDetails.ExternalDirectoryObjectId.Equals(entity.ExternalDirectoryObjectId))
                        {
                            _GroupId = groupDetails.ExternalDirectoryObjectId;
                            logger.Info($"Group with same Id exists in the destination. Name: [{entity.SmtpAddress}]. Id: [{_GroupId}]. ");
                            GetTeamUrls(TeamsService, groupDetails, false);
                            if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
                            {
                                var isExistMembemshipDynamic = false;
                                if (groupDetails.AdditionalProperties != null)
                                {
                                    var membershipInfo = groupDetails.AdditionalProperties;
                                    _GroupAdditionalProperties["Exist"] = membershipInfo;
                                    isExistMembemshipDynamic = membershipInfo.IsMembershipDynamic;
                                }
                                AddO365GroupOwnerAndMembers(entity.GroupMemberList, _GroupId, isExistMembemshipDynamic);
                                logger.Info($"Update yammer group owner and member successfully. Count: [{entity.GroupMemberList.Count}]. ");
                            }
                            else
                            {
                                _GroupAdditionalProperties["Exist"] = null;
                                logger.Info($"Skip yammer group.");
                            }
                        }
                        else
                        {
                            logger.Error($"The destination group Id is not the same as the source . Address: [{entity.SmtpAddress}]. SourceId: [{entity.ExternalDirectoryObjectId}].DesId: [{groupDetails.ExternalDirectoryObjectId}]. ");
                            throw new Exception("The destination yammer group Id is not the same as the source. ");
                        }
                    }
                    else
                    {
                        logger.Error($"Failed to get group details . Id: [{entity.ExternalDirectoryObjectId}]. Address: [{entity.SmtpAddress}]. ");
                        throw new Exception("Failed to get group details. ");
                    }
                }
                else
                {
                    logger.Error($"Yammer group doesn't exist in the destination. Id: [{entity.ExternalDirectoryObjectId}]. Address: [{entity.SmtpAddress}]. ");
                    throw new Exception(ExchangeReportMessage.CreateReportMessage("Agent.Yammer.GroupNotExist_48D9F21B-1C94-445C-AE8D-7572A3353495"));
                }
            }
            else if (this.Config.RestoreType == EORestoreType.OutOfPlace)
            {

            }
            ReportDto.Status = ReportStatus.Success;
            logger.Info("Restore yammer group:[{0}] successed.", entity.SmtpAddress);
        }

        public void InitGreneralChannelName(MicrosoftTeamsEntity sourceMSTeamsEntity)
        {
            if (null != sourceMSTeamsEntity && sourceMSTeamsEntity.AdditionalData.TryGetValue("internalId", out object internalId))
            {
                _SourceTeamIntenalId = internalId.ToString();
                _GeneralCannelName = sourceMSTeamsEntity.TeamChannels?.Find(c => c.Id.Equals(_SourceTeamIntenalId))?.DisplayName ?? "General";
            }
            else
            {
                _GeneralCannelName = "General";
            }
        }

        private void UpdateTeam(Office365GroupEntityV2 entity, Office365GroupEntityV2 groupDetails)
        {
            logger.Info("Teams already exists in the destination. TeamName: [{0}]. ", entity.SmtpAddress);
            EnsureOriginalSiteUrl();
            GetTeamUrls(TeamsService, groupDetails, false);
            if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
            {
                _GroupAdditionalProperties["Exist"] = groupDetails.AdditionalProperties;
                RestoreTeamMembersAndOwners(groupDetails.AdditionalProperties.IsMembershipDynamic);
                var needUpdateDataLocation = WhetherMeetRequireForMultiGeo(entity);
                TeamsService.UpdateTeam(_GroupId, entity.DisplayName, entity.Description, string.Empty, entity.Classification, entity.AccessType.ToString(), needUpdateDataLocation ? entity.PreferredDataLocation : null, _SourceMSTeamsEntity);

                if (M365APIService.GroupService4ServiceAccount != null)
                {
                    M365APIService.GroupService4ServiceAccount.UpdateGroupSettings(_GroupId, entity);
                }
                ReportDto.Status = ReportStatus.Success;
                logger.Info("Update Teams:[{0}] successed. ", entity.SmtpAddress);
            }
            else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Skip)
            {
                logger.Info("Teams:[{0}] has already exist.", entity.SmtpAddress);
                _GroupAdditionalProperties["Exist"] = null;
                ReportDto.Status = ReportStatus.Skipped;
                ReportDto.Option = null;//stodo// RestoreOption.Skipped.GetEnumDescription();
                ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.Teams.TeamsExist_43414301-E295-4734-89FF-FE4047B63CDA", entity.SmtpAddress);
            }
            RestoreTeamExistedTeamsApps();
            _ExistedChannels = TeamsService.ListTeamChanneslWithDetails(_GroupId);
        }
        private void RestoreTeamMembersAndOwners(bool isMembershipDynamic = false)
        {
            TeamsService.AddTeamMembersAndOwners(_GroupId, _SourceMSTeamsEntity, isMembershipDynamic);
        }
        private void RestoreTeamExistedTeamsApps()
        {
            try
            {
                if (_SourceMSTeamsEntity.TeamsApps == null)
                {
                    logger.Info("The backup team apps is null. Please check the backup job. ");
                }
                else
                {
                    logger.Info("Start to restore team apps. ");
                    var existedTeamApps = TeamsService.GetTeamApps(_GroupId).Select(tA => tA.TeamsAppDefinition.TeamsAppId).ToList();
                    //CatalogTeamsApps = exchangeMicrosoftTeams.GetCataLogTeamApps().Select(cA => cA.Id).ToList();
                    if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
                    {
                        _SourceMSTeamsEntity.TeamsApps.ForEach(tA => RestoreTeamsApp(existedTeamApps, tA));
                    }
                    #region for conflict replace
                    //else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Overwrite) //for replace
                    //{
                    //    exchangeMicrosoftTeams.RemoveTeamsApps(GroupId, existedTeamApps);
                    //    SourceMSTeamsEntity.TeamsApps.ForEach(tA => RestoreTeamsApp(tA));
                    //}
                    #endregion
                    logger.Info("Success to restore team apps. ");
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to restore team apps. Reason: {0}. ", ex.ToString());
            }
        }

        private void RestoreTeamsApp(List<string> existedTeamApps, TeamsApp tempTeamsApp)
        {
            if (!existedTeamApps.Contains(tempTeamsApp.TeamsAppDefinition.TeamsAppId)) RestoreTeamsApp(tempTeamsApp);
            else logger.Info("TeamsApp exists. AppName:{0}. AppId: {1}. ", tempTeamsApp.TeamsAppDefinition.DisplayName, tempTeamsApp.TeamsAppDefinition.TeamsAppId);
        }

        private void RestoreTeamsApp(TeamsApp tempTeamsApp)
        {
            //if (CatalogTeamsApps.Contains(tempTeamsApp.TeamsAppDefinition.TeamsAppId))
            //{
            try
            {
                logger.Info("Start to restore team app. AppName:{0}. AppId: {1}. ", tempTeamsApp.TeamsAppDefinition.DisplayName, tempTeamsApp.TeamsAppDefinition.TeamsAppId);
                TeamsService.AddTeamsApp(_GroupId, tempTeamsApp);
                logger.Info("Success to restore team app. AppName:{0}. AppId: {1}. ", tempTeamsApp.TeamsAppDefinition.DisplayName, tempTeamsApp.TeamsAppDefinition.TeamsAppId);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to restore team app. AppName:{0}. AppId: {1}. Reason: {2}. ", tempTeamsApp.TeamsAppDefinition.DisplayName, tempTeamsApp.TeamsAppDefinition.TeamsAppId, ex.ToString());
            }
            //}
            //else
            //{
            //    if (!NoNeedRestoreTeamsApps.TeamsApps.Contains(tempTeamsApp.TeamsAppDefinition.TeamsAppId))
            //        logger.Error("TeamsApp can't add to the destination, please publish it first . AppName:{0}. AppId: {1}. ", tempTeamsApp.TeamsAppDefinition.DisplayName, tempTeamsApp.TeamsAppDefinition.TeamsAppId);
            //}
        }

        private void CreateTeam(Office365GroupEntityV2 entity)//, string restoreEntityTitle)
        {
            logger.Info("Start to create Teams:[{0}]. ", entity.SmtpAddress);
            string alias = entity.SmtpAddress.Substring(0, entity.SmtpAddress.LastIndexOf("@"));
            bool needUpdateDataLocation = WhetherMeetRequireForMultiGeo(entity);

            var info = UseMigrationMode ? CreateTeamMigration(entity, _SourceMSTeamsEntity) 
                    : TeamsService.CreateTeam(entity.DisplayName, entity.Description, alias, entity.Classification,
                                                         entity.AccessType.ToString(), _SourceMSTeamsEntity, currentUser, needUpdateDataLocation ? entity.PreferredDataLocation : null);
            _GroupId = info.GroupId;
            if (!UpdateTeamSmtpAddressV2(entity, info.Mail, info.GroupId))
            {
                logger.Error("The team new address is not the same as the current address. Failed to update team address. Team: [{0}]. ", entity.SmtpAddress);
                throw new Exception("Agent.Teams.FailedUpdateTeamAddress_E5FE21A0-E10A-4E55-B38B-2E03D54975CB");
            }
            Office365GroupEntityV2 tempGroupDetails = null;
            AvePoint.Wrapper.Common.AveTaskRetryHelper helper = new AvePoint.Wrapper.Common.AveTaskRetryHelper(60, true);
            helper.ExecuteWithRetryMechanism(() =>
            {
                try
                {
                    //tempGroupDetails = M365APIService.GroupService.GetO365GroupDetail(entity.SmtpAddress);
                    tempGroupDetails = M365APIService.GroupService.GetO365GroupDetailById(info.GroupId);
                    tempGroupDetails.IsTeamsGroup = Config.IsMicrosoftTeams;
                    if (tempGroupDetails == null || (tempGroupDetails != null && !string.Equals(_GroupId, tempGroupDetails.ExternalDirectoryObjectId, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new Exception("Failed to get group detail or the detail is not the new one.");
                    }
                }
                finally
                {
                    Thread.Sleep(30000);
                }
            }
            );
            if (tempGroupDetails == null || (tempGroupDetails != null && !string.Equals(_GroupId, tempGroupDetails.ExternalDirectoryObjectId, StringComparison.OrdinalIgnoreCase)))
            {
                logger.Error("Can not get the team group or the team group is not the new one. Team: [{0}]. ", entity.SmtpAddress);
                throw new Exception("Agent.Teams.FailedGetTeamGroup_C0D9D010-8E65-435D-A6B3-2874CC3AED03");
            }

            if (tempGroupDetails != null && tempGroupDetails.GroupResources.Length > 0)
            {
                // log all data Type and Url in GroupResources.
                foreach (var resource in tempGroupDetails.GroupResources)
                {
                    logger.Info("tempGroupDetails.GroupResources. Resource Type: {0} | Resource Url: {1}", resource?.Type, resource?.Url);
                }
            }

            if (M365APIService.GroupService4ServiceAccount != null)
            {
                M365APIService.GroupService4ServiceAccount.UpdateGroupSettings(_GroupId, entity);
            }
            logger.Info("Success to create Teams:[{0}]. ", entity.SmtpAddress);
            ReportDto.Status = ReportStatus.Success;
            EnsureOriginalSiteUrl();
            GetTeamUrls(TeamsService, tempGroupDetails, false);
            try
            {
                if (_SourceMSTeamsEntity.TeamsApps == null)
                {
                    logger.Info("The backup team apps is null. Please check the backup job. ");
                }
                else
                {
                    logger.Info("Start to restore team apps. ");
                    //CatalogTeamsApps = exchangeMicrosoftTeams.GetCataLogTeamApps().Select(cA => cA.Id).ToList();
                    var existedTeamApps = TeamsService.GetTeamApps(_GroupId).Select(tA => tA.TeamsAppDefinition.TeamsAppId).ToList();
                    _SourceMSTeamsEntity.TeamsApps.ForEach(tA => RestoreTeamsApp(existedTeamApps, tA));
                    logger.Info("Success to restore team apps. ");
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to restore team apps. Reason: {0}. ", ex.ToString());
            }
            _ExistedChannels = TeamsService.ListTeamChanneslWithDetails(_GroupId);
            if (UseMigrationMode)
            {
                MigrationChannelIds.AddRange(_ExistedChannels.Where(c => c.IsStandardChannel()).Select(c => c.Id));
            }
        }

        private TeamInfo CreateTeamMigration(Office365GroupEntityV2 entity, MicrosoftTeamsEntity sourceMSTeamsEntity)
        {
            string teamsId = string.Empty;
            Group? group = null;
            var teams = new Teams()
            {
                DisplayName = entity.DisplayName,
                CreatedDateTime = new DateTimeOffset(new DateTime(OldestMessageDate)),
                Description = entity.Description,
                Visibility = ConvertVisibility(entity.AccessType.ToString()),
                Classification = entity.Classification,
                AdditionalData = new Dictionary<string, object>()
                {
                    { "@microsoft.graph.teamCreationMode", "migration" }
                }
            };

            try
            {
                Policy.Handle<Exception>(ex =>
                {
                    if (ex.Message.Contains("BadRequest"))
                    {
                        teams.Classification = null;
                        return true;
                    }
                    return false;
                }).WaitAndRetry(2, retryTime => TimeSpan.FromMinutes(1))
                .Execute(() =>
                {
                    teamsId = _graphService.Teams.CreateTeamsAsync(teams).ExecuteAsyncTask();
                });

                Policy.Handle<ODataError>(e =>
                {
                    if (e.Message.Contains("does not exist"))
                    {
                        return true;
                    }
                    return false;
                }).WaitAndRetry(3, _ => TimeSpan.FromSeconds(30))
                .Execute(() =>
                {
                    group = _graphService.Groups.GetAsync(teamsId).ExecuteAsyncTask();
                });

                return new TeamInfo() { GroupId = group.Id, Mail = group.Mail };
            }
            catch (Exception e)
            {
                logger.Error($"Failed to create team in migration mode. TeamName: {entity.DisplayName}, GroupId: {teamsId}. Exception: {e}");
                if (teamsId.IsNotNullOrEmpty())
                {
                    M365APIService.GroupService.RemoveGroup(teamsId);
                }
                throw;
            }

            TeamVisibilityType ConvertVisibility(string value)
            {
                return value.ToLower() switch
                {
                    "private" => TeamVisibilityType.Private,
                    "public" => TeamVisibilityType.Public,
                    "hiddenmembership" => TeamVisibilityType.HiddenMembership,
                    _ => throw new NotSupportedException(value)
                };
            }
        }

        private bool WhetherMeetRequireForMultiGeo(Office365GroupEntityV2 entity)
        {
            try
            {
                if ((!string.IsNullOrEmpty(entity.PreferredDataLocation))
                    && (TeamsService.AuthObject is IAppTokenAuthObject auth)
                    && auth.PermissionType == TokenPermissionType.Application)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to check require for multi geo. Reason: {0}. ", ex.ToString());
                return false;
            }
        }

        public static object GetValue(Dictionary<string, object> json, string key)
        {
            if (json.TryGetValue(key, out object obj))
            {
                return obj;
            }
            return null;
        }
        public static T GetValue<T>(Dictionary<string, object> json, string key)
        {
            try
            {
                if (json.TryGetValue(key, out object obj))
                {
                    return (T)obj;
                }
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }
        private void GetTeamUrls(MicrosoftTeamsAPIBase exchangeMicrosoftTeams, Office365GroupEntityV2 tempGroupDetails, bool throwIfNotFound)
        {
            try
            {
                var groupSiteUrl = tempGroupDetails.GroupResources != null && tempGroupDetails.GroupResources.Length > 0 ? tempGroupDetails.GroupResources[0].Url : string.Empty;
                _GroupSiteUrl = GetTeamSiteUrl(exchangeMicrosoftTeams, groupSiteUrl);
                logger.Info("Team site url:[{0}]. ", _GroupSiteUrl);
                if (string.IsNullOrEmpty(_GroupSiteUrl)) throw new Exception("Agent.Teams.FailedGetTeamSite_FE69A800-35AD-45B0-A766-4E2BDFD4F8ED");
            }
            catch (Exception ex)
            {
                _SiteNotFound = true;
                logger.Error("Failed to get team site url. Team: [{0}].  Reason: {1}", tempGroupDetails.SmtpAddress, ex.ToString());
                if (throwIfNotFound) throw;
            }
            //用于获取 share document 的 name.
            try
            {
                var groupSiteFilesUrl = tempGroupDetails.GroupResources != null && tempGroupDetails.GroupResources.Length > 1 ? tempGroupDetails.GroupResources[1].Url : string.Empty;
                _GroupSiteFilesUrl = GetTeamDocLibUrl(exchangeMicrosoftTeams, groupSiteFilesUrl);
                logger.Info("GroupSiteFilesUrl:[{0}]. ", _GroupSiteFilesUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get team DocLib url. Team: [{0}].  Reason: {1}", tempGroupDetails.SmtpAddress, ex.ToString());
            }

            logger.Info("Finish to get team urls. Team site url:[{0}]. GroupSiteFilesUrl:[{1}]. ", _GroupSiteUrl, _GroupSiteFilesUrl);
        }

        private bool UpdateTeamSmtpAddressV2(Office365GroupEntityV2 entity, string newTeamAddress, string groupId)
        {
            bool isUpdateAddressSuccess = true;
            logger.Info($"New team smtp address. SmtpAddress: {newTeamAddress}.GroupId:{groupId}.");
            try
            {
                newTeamAddress = M365APIService.GroupService.GetGroupAddressWithRetry(groupId);
                logger.Info($"Refetched latest team smtp address for retry: {newTeamAddress}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to retrieve the latest team address. Details: {ex}");
            }
            if (!string.IsNullOrEmpty(newTeamAddress) && !newTeamAddress.Equals(entity.SmtpAddress, StringComparison.OrdinalIgnoreCase))
            {
                if (DaoService.RMKeyValueDao.IsDisableUpdateTeamSmtpAddress())
                {
                    logger.Info("The team new address is not the same as the current address. But the update is disabled by config. So skip updating team smtp address. Identity: {0}. SmtpAddress: {1}. ", newTeamAddress, entity.SmtpAddress);
                    return true;
                }

                logger.Info("The team new address is not the same as the current address. Need to update team smtp address. Identity: {0}. SmtpAddress: {1}. ", newTeamAddress, entity.SmtpAddress);
                var service = ExchangeServiceFactory.CreateOutlookService(
                    AuthorizationManager
                    .GetAuthObjectForExchangePS(entity.SmtpAddress)
                    //.GetAuthObjectForGraph(entity.SmtpAddress)
                    );
                isUpdateAddressSuccess = PollyRetry.HandleAsync<Exception, bool>(delegate
                {   //service.SetPFPrimarySmtpAddressAsync(_GroupId).ExecuteAsyncTask();
                    return service.SetUnifiedGroupAddressAsync(newTeamAddress, entity.SmtpAddress); }
                    , HanldException, 5, 30000).ExecuteAsyncTask();

                logger.Info("Finish to update team smtp address.");
                Thread.Sleep(30000);
            }
            return isUpdateAddressSuccess;
            bool HanldException(Exception exception)
            {
                logger.Error($"An error occurred while updating the group address. Details: {exception}");
                return true;
            }
        }

        private  string GetTeamSiteUrl(MicrosoftTeamsAPIBase exchangeMicrosoftTeams, string groupSiteUrl)
        {
            return GetTeamUrl(exchangeMicrosoftTeams, groupSiteUrl, false);
        }

        private  string GetTeamDocLibUrl(MicrosoftTeamsAPIBase exchangeMicrosoftTeams, string groupDocLibUrl)
        {
            return GetTeamUrl(exchangeMicrosoftTeams, groupDocLibUrl, true);
        }

        private string GetTeamUrl(MicrosoftTeamsAPIBase exchangeMicrosoftTeams, string groupObjectUrl, bool docLibUrl)
        {
            var bopsinfo = M365APIService.BposInfo;
            if (bopsinfo.ConnectionType == AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken
                || bopsinfo.ConnectionType == AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern)
            {
                if (string.IsNullOrEmpty(groupObjectUrl) || groupObjectUrl.ToLower().Contains("/_layouts/groupstatus.aspx?id="))//_layouts/groupstatus.aspx?id= not work for access token
                {
                    string url = string.Empty;
                    AvePoint.Wrapper.Common.AveTaskRetryHelper helper = new AvePoint.Wrapper.Common.AveTaskRetryHelper(30, true);
                    helper.ExecuteWithRetryMechanism(() =>
                    {
                        try
                        {
                            url = docLibUrl ?
                            exchangeMicrosoftTeams.GetTeamSiteDocLibUrl(_GroupId) :
                            exchangeMicrosoftTeams.GetTeamSiteUrl(_GroupId);
                        }
                        finally
                        {
                            Thread.Sleep(10000);
                        }
                    });
                    return url;
                }
                return groupObjectUrl;
            }
            else
            {
                if (groupObjectUrl.ToLower().Contains("/_layouts/groupstatus.aspx?id="))
                {
                    groupObjectUrl = groupObjectUrl.ToLower().Replace("/_layouts/groupstatus.aspx?id=", "/_layouts/15/groupstatus.aspx?id=");
                    ITokenProvider tokenProvider = null;

                    //stodo//bopsinfo.ConvertToAveBPOSAccountInfo().Convert2TokenProvider(new List<ProviderType> { ProviderType.AppProfile, ProviderType.ServiceAccount })
                    var uploader = new TeamsFileUploader(tokenProvider);
                    return uploader.GetGroupSiteUrl(groupObjectUrl);
                }
                return groupObjectUrl;
            }
        }

        private string GenerateSpecifiedException(Exception ex)
        {
            if (ex.Message.Equals(RestoreConstants.NO_SMTP_ADDRESS, StringComparison.OrdinalIgnoreCase)) return "EOBErrorMessage_NoSmtpAddress";
            if (ex.Message.Contains(RestoreConstants.MAILBOX_DATABASE_UNAVALIABLE)) return "EOBErrorMessage_DatabaseUnavaliable";
            if (ex.Message.Contains(RestoreConstants.MAILBOX_OVERDUE)) return "EOBErrorMessage_MailboxOverdue";
            if (ex.Message.Contains(RestoreConstants.SERVER_CANNOT_SERVICE_REQUEST)) return "EOBErrorMessage_ServerBusy";
            if (ex.Message.Contains(RestoreConstants.AUTODISCOVER_RETURN_ERROR)) return "EOBErrorMessage_AutoDiscoverReturnError";
            if (ex.Message.Contains(RestoreConstants.AUTODISCOVER_CANNOT_BE_LOCATED)) return "EOBErrorMessage_AutoDiscoverCannotLocate";
            if (ex.Message.Contains(RestoreConstants.ACCOUNT_UNAUTHORIZED)) return "EOBErrorMessage_Unauthorized";
            if (ex is ArgumentNullException && ex.Message.Contains("Parameter name: BposInfo")) return "Service.Common_bee74828-5326-4778-9905-6866c92196a1";
            if (ex is AggregateException) return ex.WrapAggregateErrorMessage(I18NDataCollector.GetData(DynamicDataKey.UserName));
            if (ex is GraphAPIException gEx && gEx.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                if (gEx.Message.Contains(RestoreConstants.SET_PREFERRED_DATA_LOCATION_FAILED))
                {
                    return "Agent.Office365Group.SetPreferredDataLocationFailed_B3DA1A65-523D-4F3D-9DA8-FBC8CCEB5AF4";
                }
                else
                {
                    return ExchangeReportMessage.CreateReportMessage("Agent.Teams.NotChannelMember_C65279D3-C359-61DA-3350-2FE673A979C5", I18NDataCollector.GetData(DynamicDataKey.UserName));
                }
            }

            return ex.Message;
        }
    }
}