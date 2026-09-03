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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.JobMonitor;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Job.ModernManagement.Report;
    using M365GroupTeam;
    using RAArchiverCommon.TeamsController;
    using AvePerformanceScope = AvePoint.Wrapper.Common.AvePerformanceScope;

    public abstract class BaseRestoreHelperBatch : IRestoreHelperBatch
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(BaseRestoreHelperBatch));

        #region ---protected service---
        protected MicrosoftTeamsAPIBase TeamsService => M365APIService.TeamsService;
        protected MicrosoftTeamsAPIBase TeamsServiceForDelegate => M365APIService.TeamsServiceForDelegate;
        protected ExchangePlannerService PlannerService => M365APIService.PlannerService;
        protected ExchangePlannerService PlannerServiceForDelegate => M365APIService.PlannerServiceForDelegate;
        //public MicrosoftTeamsAPIBase TeamsMembershipService => M365APIService.TeamsService;
        public MicrosoftTeamsAPIBase TeamsMembershipService { get; set; }

        public IReportCenter Report;
        public RestoreConfig Config { get; set; }
        public M365APIService M365APIService { get; set; }
        public AuthorizationManager AuthorizationManager => M365APIService.authorizationManager;
        #endregion

        public ExchangeFileHeader FileHeader { get; set; }

        public ReportDto ReportDto { get; set; }

        protected bool IsTeamsRestoreOutOfPlace;

        protected static I18NParameterCollector I18NDataCollector { get; set; }

        #region ---SharedProperties---
        private TeamsCacheMapping _teamsCache = new TeamsCacheMapping();
        protected List<string> _TeamsChannels
        {
            get => _teamsCache.TeamsChannels;
            set => _teamsCache.TeamsChannels = value;
        }
        protected List<string> _NewlyPlannerPlanIds
        {
            get => _teamsCache.NewlyPlannerPlanIds;
            set => _teamsCache.NewlyPlannerPlanIds = value;
        }
        protected List<TeamChannel> _ExistedChannels
        {
            get => _teamsCache.ExistedChannels;
            set => _teamsCache.ExistedChannels = value;
        }
        protected List<TeamMember> _ExistedTeamUsers
        {
            get => _teamsCache.ExistedTeamUsers;
            set => _teamsCache.ExistedTeamUsers = value;
        }
        protected ChannelCache _CurrentChannel
        {
            get => _teamsCache.CurrentChannel;
            set => _teamsCache.CurrentChannel = value;
        }

        protected MicrosoftTeamsEntity _SourceMSTeamsEntity
        {
            get => _teamsCache.SourceMSTeamsEntity;
            set => _teamsCache.SourceMSTeamsEntity = value;
        }
        protected SpecialCustomersAdapter _SpecialTeamAdapter
        {
            get => _teamsCache.SpecialTeamAdapter;
            set => _teamsCache.SpecialTeamAdapter = value;
        }
        protected bool _SiteNotFound
        {
            get => _teamsCache.SiteNotFound;
            set => _teamsCache.SiteNotFound = value;
        }
        protected bool _IsNewlyCreatedGroupSite
        {
            get => _teamsCache.IsNewlyCreatedTeams;
            set => _teamsCache.IsNewlyCreatedTeams = value;
        }
        
        protected string _GroupId
        {
            get => _teamsCache.GroupId;
            set => _teamsCache.GroupId = value;
        }
        protected string _TeamIntenalId
        {
            get => _teamsCache.TeamIntenalId;
            set => _teamsCache.TeamIntenalId = value;
        }
        protected string _SourceTeamIntenalId
        {
            get => _teamsCache.SourceTeamIntenalId;
            set => _teamsCache.SourceTeamIntenalId = value;
        }
        protected string _GroupSiteUrl
        {
            get => _teamsCache.GroupSiteUrl;
            set => _teamsCache.GroupSiteUrl = value;
        }
        protected string _GroupSiteFilesUrl
        {
            get => _teamsCache.GroupSiteFilesUrl;
            set => _teamsCache.GroupSiteFilesUrl = value;
        }
        protected string _GeneralCannelName
        {
            get => _teamsCache.GeneralCannelName;
            set => _teamsCache.GeneralCannelName = value;
        }

        protected Dictionary<string, string> EntityIdDic
        {
            get => _teamsCache.EntityIdDic;
            set => _teamsCache.EntityIdDic = value;
        }
        protected Dictionary<string, string> SiteUrlDic
        {
            get => _teamsCache.SiteUrlDic;
            set => _teamsCache.SiteUrlDic = value;
        }
        protected Dictionary<string, string> BucketIdDic
        {
            get => _teamsCache.BucketIdDic;
            set => _teamsCache.BucketIdDic = value;
        }
        /// 记录 bucket 旧 id 新 id 对应关系的字典
        protected List<Bucket> _UnmatchBuckets
        {
            get => _teamsCache.UnmatchBuckets;
            set => _teamsCache.UnmatchBuckets = value;
        }
        /// 匹配不到 id 的 buckets
        protected Dictionary<string, Office365PlannerBucketProperties> _NeedUpdatePlanBuckets
        {
            get => _teamsCache.NeedUpdatePlanBuckets;
            set => _teamsCache.NeedUpdatePlanBuckets = value;
        }
        protected List<PlannerTabUpdateObj> _PlannerTabs
        {
            get => _teamsCache.PlannerTabs;
            set => _teamsCache.PlannerTabs = value;
        }

        protected List<FileTabUpdateObj> _FileTabs
        {
            get => _teamsCache.FileTabs;
            set => _teamsCache.FileTabs = value;
        }

        protected Dictionary<string, string> _AllTasks
        {
            get => _teamsCache.AllTasks;
            set => _teamsCache.AllTasks = value;
        }
        
        protected Dictionary<string, List<ConversationMember>> _ConversationMembers
        {
            get => _teamsCache.ConversationMembers;
            set => _teamsCache.ConversationMembers = value;
        }
        protected Dictionary<string, GroupAdditionalPropertiesV2?> _GroupAdditionalProperties
        {
            get => _teamsCache.GroupAdditionalProperties;
            set => _teamsCache.GroupAdditionalProperties = value;
        }

        public bool UseMigrationMode
        {
            get => _teamsCache.UseMigrationMode;
            set => _teamsCache.UseMigrationMode = value;
        }

        protected long OldestMessageDate
        {
            get => _teamsCache.OldestMessageDate;
            set => _teamsCache.OldestMessageDate = value;
        }

        protected List<string> MigrationChannelIds
        {
            get => _teamsCache.MigrationChannelIds;
            set => _teamsCache.MigrationChannelIds = value;
        }

        #endregion

        public BaseRestoreHelperBatch()
        {

        }

        public BaseRestoreHelperBatch(BaseRestoreHelperBatch baseHelper)
        {
            this.M365APIService = baseHelper.M365APIService;
            this.Config = baseHelper.Config;
            this.Report = baseHelper.Report;
            this._teamsCache = baseHelper._teamsCache;
            this.IsTeamsRestoreOutOfPlace = Config.JobType == (int)JobType.TeamsOutPlaceRestore;
        }

        protected virtual void InitReport(MetadataEntity baseEntity, string sourceUrlPath)
        {
            ReportDto = new ReportDto
            {
                Name = baseEntity.Title,
                Title = baseEntity.Title,
                Status = ReportStatus.Success,
                Option = RestoreOption.NewCreated.GetEnumDescription(),
                EntityType = JobReportDetailEntityType.Objects,
                Size = baseEntity.Size,
                Path = GetTargetPath(baseEntity.DisplayPath),
                SourcePath = sourceUrlPath ?? baseEntity.DisplayPath
            };
            string GetTargetPath(string sourceUrlPath)
            {
                var sPath = sourceUrlPath;
                if (string.IsNullOrEmpty(sPath) || string.IsNullOrEmpty(RestoreConfig.CurrentRestoreMailbox)) return sPath;
                var index = sPath.IndexOf('\\');
                var sName = sPath.Substring(0, index == -1 ? sPath.Length : index);
                return sPath.Replace(sName, RestoreConfig.CurrentRestoreMailbox);
            }
        }

        protected virtual void InitReport()
        {
            ReportDto = new ReportDto
            {
                EntityType = JobReportDetailEntityType.Objects,
                Name = FileHeader.Name,
                Status = ReportStatus.Success,
                Option = RestoreOption.NewCreated.GetEnumDescription(),
                Title = FileHeader.Name,
                SourcePath = FileHeader.Name,
                Path = FileHeader.Name,
                Size = RestoreConstants.FolderSize
            };
        }
        protected virtual bool NeedRestore() => true;

        public virtual void Restore(IEnumerable<ExchangeDataBlockForBatch> dataBlockList)
        {
            logger.Info("Start to real restore the data.");
            if (NeedRestore())
            {
                using (AvePerformanceScope pc = new AvePerformanceScope($"{this.GetType().Name}.RealRestore"))
                {
                    this.FileHeader = dataBlockList.First().FileHeader;
                    RealRestore(dataBlockList);
                }
            }
            logger.Info("End to real restore the data.");
        }

        protected abstract void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataBlockList);

        public void Dispose()
        {
            ClearConversationMembers();
            TryCompleteMigration();
        }

        public void ClearConversationMembers()
        {
            if (Config.RestoreConversationType == RestoreConversationType.Html || _ConversationMembers.Count == 0) return;

            //var authObj = AuthorizationManager.GetDelegateAppAuthObject(RestoreConfig.CurrentRestoreMailbox, DelegateAppCloudBackupModuleType.Channel);
            //var delegateAppAuthObj = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj);
            //TeamsMembershipService = delegateAppAuthObj.AuthObject.AuthType switch
            //{
            //    AuthObjectType.PasswordAccessToken => delegateAppAuthObj,
            //    AuthObjectType.AccessToken or _ => TeamsService
            //};

            TeamsMembershipService = TeamsServiceForDelegate.AuthObject.AuthType switch
            {
                AuthObjectType.PasswordAccessToken => TeamsServiceForDelegate,
                AuthObjectType.AccessToken or _ => TeamsService
            };

            var deletedTeamIds = new List<string>();
            foreach (var member in _ConversationMembers)
            {
                var info = member.Key.Split(ExchangeConstants.PathParser);
                var teamId = info[0];
                member.Value.ForEach(m =>
                {
                    if (string.IsNullOrEmpty(m.Id) || !m.NeedDelete) return;

                    try
                    {
                        if (info.Length == 1)
                        {
                            logger.Info("Remove member: {0} from team: {1}, id: {2}.", m.Email, teamId, m.Id);
                            TeamsMembershipService.RemoveTeamMember(teamId, m.Id);
                            deletedTeamIds.Add(teamId);
                        }
                        else if (!deletedTeamIds.Contains(teamId))
                        {
                            var channelId = info[1];
                            logger.Info("Remove member: {0} from private channel: {1}/{2}, id: {3}.", m.Email, teamId, channelId, m.Id);
                            TeamsMembershipService.RemoveChannelMember(teamId, channelId, m.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Remove member failed, {0}.", ex);
                    }
                });
            }
            _ConversationMembers.Clear();
        }


        // should be called after all standard channels are processed restore
        // todo: may need to change the channel restore order if support Migration later if the order is not correct (all standard channels should be processd sequentially, without any others in between)
        protected void TryCompleteMigration(TeamChannel nextChannel = null)
        {
            if (UseMigrationMode && (nextChannel is null || !nextChannel.IsStandardChannel()))
            {
                try
                {
                    foreach (var channelId in MigrationChannelIds)
                    {
                        TeamsService.CompleteChannelMigration(_GroupId, channelId);
                    }
                    TeamsService.CompleteTeamsMigration(_GroupId);
                    TeamsService.UpdateTeamsSetting(_GroupId, _SourceMSTeamsEntity);
                    TeamsService.AddTeamMembersAndOwners(_GroupId, _SourceMSTeamsEntity);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error complete migration, ex: {ex}");
                }
                finally
                {
                    UseMigrationMode = false;
                    MigrationChannelIds.Clear();
                }
            }
        }

        public void PostAction()
        {
            logger.Info("Start executing PostAction");

            if (!_FileTabs.IsNullOrEmpty())
            {
                logger.Info("Start updating FileTab");
                UpdateFileTab();
                logger.Info("End updating FileTab");
            }

            if (!_GroupAdditionalProperties.IsNullOrEmpty())
            {
                logger.Info($"Start updating MemberShip for Group: {_GroupId}");
                UpdateMembershipType();
                logger.Info("End updating MemberShip");
            }

            if (Config.IsMicrosoftTeams && _SourceMSTeamsEntity.IsArchived.HasValue && _SourceMSTeamsEntity.IsArchived.Value)
            {
                logger.Info($"Start updating Teams archive status for Group: {_GroupId}, IsChannelSiteReadOnly:{TeamsRestoreState.IsChannelSiteReadOnly}");
                UpdateTeamsArchiveStatus(TeamsRestoreState.IsChannelSiteReadOnly);
                logger.Info("End updating Teams archive status");
            }

            logger.Info("End executing PostAction");
        }

        public void UpdateFileTab()
        {
            foreach (var fileTab in _FileTabs)
            {
                try
                {
                    if (TryGetMappedEntityId(fileTab.EntityId, null, out var newEntityId))
                    {

                        logger.Info($@"Update channel tab configuration in UpdateFileTab: 
                                        Channel:           {fileTab.ChannelId},
                                        Tab:               {fileTab.RestoreTab.ChannelTab.DisplayName},
                                        EntityId:          {fileTab.EntityId}->{newEntityId},
                                        Configuration:     {System.Text.Json.JsonSerializer.Serialize(fileTab.RestoreTab.Configuration)}");

                        fileTab.RestoreTab.Configuration.EntityId = newEntityId.ToString().ToUpperInvariant();
                        M365APIService.TeamsService.UpdateChannelTabConfig(_GroupId, fileTab.ChannelId, fileTab.TabId, fileTab.RestoreTab);
                    }
                    else
                    {
                        logger.Info($@"Cannot update channel tab configuration in UpdateFileTab: 
                                        Channel:           {fileTab.ChannelId},
                                        Tab:               {fileTab.RestoreTab.ChannelTab.DisplayName},
                                        EntityId:          {fileTab.EntityId}->{newEntityId},
                                        Configuration:     {System.Text.Json.JsonSerializer.Serialize(fileTab.RestoreTab.Configuration)}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to update team tabs configuration, error: {ex}");
                }

            }

            bool TryGetMappedEntityId(string entityId, AveMappingManager mapping, out Guid value)
            {
                if (Guid.TryParse(entityId, out Guid oldId))
                {
                    if (WrapperConfiguration.ChannelTabEntityIdMapping.TryGetValue(oldId, out value))
                    {
                        return value != Guid.Empty && value != oldId;
                    }
                }
                value = Guid.Empty;
                return false;
            }
        }

        public void UpdateMembershipType()
        {
            try
            {
                if (!_GroupAdditionalProperties.TryGetValue("Archived", out var archivedProps) || archivedProps == null)
                {
                    logger.Warn($"Invalid archivedProps: {archivedProps?.ToMembershipString()} or groupId: {_GroupId}. Skip updating membership info");
                    return;
                }

                if (!archivedProps.IsMembershipDynamic)
                {
                    // archived assigned => exist dynamic => skip/restore ?
                    logger.Info($"archivedProps does not use dynamic membership type. Skip updating membership info");
                    return;
                }

                if (!_GroupAdditionalProperties.TryGetValue("Exist", out var existProps))
                {
                    logger.Info($"There is no existProps. Update membership info: {archivedProps.ToMembershipString()}");
                    M365APIService.GroupService.UpdateGroupMembershipType(_GroupId, archivedProps);
                    return;
                }
                else if (existProps == null)
                {
                    logger.Warn($"group already existed but conflict resolution is skip. Skip updating membership info");
                    return;
                }
                else
                {
                    if (!IsMembershipChange(archivedProps, existProps))
                    {
                        logger.Info($"Membership info is not change. Skip updating membership info");
                        return;
                    }

                    logger.Info($"Need update membership info. \nExist membership info: {existProps.ToMembershipString()}.\nArchived membership info: {archivedProps.ToMembershipString()}");

                    M365APIService.GroupService.UpdateGroupMembershipType(_GroupId, archivedProps);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to update group membership type for group id: {_GroupId}, error: {e}");
            }
        }

        public static bool IsMembershipChange(GroupAdditionalPropertiesV2 oldProps, GroupAdditionalPropertiesV2 newProps)
        {
            return oldProps.IsMembershipDynamic != newProps.IsMembershipDynamic
                || (oldProps.IsMembershipDynamic == true &&
                    (!string.Equals(oldProps.MembershipRule, newProps.MembershipRule, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(oldProps.MembershipRuleProcessingState, newProps.MembershipRuleProcessingState, StringComparison.OrdinalIgnoreCase))
                   );
        }

        public bool UpdateTeamsArchiveStatus(bool makeSiteReadOnly)
        {
            try
            {
                var isArchivedSuccess = false;
                var result = M365APIService.TeamsServiceForDelegate.ArchiveTeam(_GroupId, makeSiteReadOnly);
                if (result)
                {
                    var teamsSetting = M365APIService.TeamsService.GetTeamSettings(_GroupId);
                    if (teamsSetting != null)
                    {
                        isArchivedSuccess = teamsSetting.IsArchived.HasValue && teamsSetting.IsArchived.Value;
                    }
                }

                if (isArchivedSuccess)
                {
                    logger.Info($"Successfully updated Teams archive status for group id: {_GroupId}, makeSiteReadOnly: {makeSiteReadOnly}");
                    return true;
                }

                //Todo?: need add loop check for the status until it is archived or reach the max retry count ?

                logger.Info($"Failed to update Teams archive status for group id: {_GroupId}, makeSiteReadOnly: {makeSiteReadOnly}");
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to update Teams archive status for group id: {_GroupId}, error: {e}");
                return false;
            }
        }

        public bool UpdateTeamsUnarchiveStatus()
        {
            try
            {
                var isUnarchivedSuccess = false;
                var result = M365APIService.TeamsServiceForDelegate.UnarchiveTeam(_GroupId);
                if (result)
                {
                    var teamsSetting = M365APIService.TeamsService.GetTeamSettings(_GroupId);
                    if (teamsSetting != null)
                    {
                        isUnarchivedSuccess = teamsSetting.IsArchived.HasValue && !teamsSetting.IsArchived.Value;
                    }
                }
                if (isUnarchivedSuccess)
                {
                    logger.Info($"Successfully updated Teams unarchive status for group id: {_GroupId}");
                    return true;
                }
                logger.Info($"Failed to update Teams unarchive status for group id: {_GroupId}");
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to update Teams unarchive status for group id: {_GroupId}, error: {e}");
                return false;
            }
        }

        public bool IsTeamsArchived()
        {
            try
            {
                var teamsSetting = M365APIService.TeamsService.GetTeamSettings(_GroupId);
                if (teamsSetting != null)
                {
                    return teamsSetting.IsArchived.HasValue && teamsSetting.IsArchived.Value;
                }
                logger.Info($"Failed to get Teams settings for group id: {_GroupId}");
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to get Teams settings for group id: {_GroupId}, error: {e}");
                return false;
            }
        }
    }
}