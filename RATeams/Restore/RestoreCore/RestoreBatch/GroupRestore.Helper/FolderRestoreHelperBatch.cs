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
    
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.GCommon.Utility;
    using AvePoint.Metadata;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Job.ModernManagement.Report;
    using Office365GroupBackup;
    using Polly;
    using RAArchiverCommon;
    using RAArchiverCommon.TeamsController;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class FolderRestoreHelperBatch : BaseRestoreHelperBatch
    {
        public FolderRestoreHelperBatch(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {

        }

        protected override void InitReport(MetadataEntity baseEntity, String sourceUrlDic)
        {
            base.InitReport(baseEntity, sourceUrlDic);
            ReportDto.Type = ReportNodeHeader.Channel;
        }

        protected override bool NeedRestore() => !string.IsNullOrEmpty(RestoreConfig.CurrentRestoreMailbox);

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataBlockCollection)
        {
            
            CleanChannelCache();
            var restoreData = dataBlockCollection.First().RestoreData;
            var entity = restoreData.Metadata;
            var teamChannel = GetTeamChannelMetadata(restoreData, entity);

            TryCompleteMigration(teamChannel);

            var sourceUrlPath = dataBlockCollection.First().RestoreData.SourceUrlPath;
            try
            {
                InitReport(entity, sourceUrlPath);

                logger.Info($"Start to restore {ReportDto.Type}, name:{ReportDto.Name}, path: {ReportDto.Path}, size:{ReportDto.Size}");

                if (teamChannel.IsInComing)
                {
                    var teamMail = teamChannel.AdditionalData.TryGetValue("HostTeamMail", out var hostTeamMail) ? hostTeamMail.ToString() : String.Empty;
                    var message = $"This is a shared channel from another team, please restore from Team ({teamMail}).";
                    ReportDto.ErrorMessage = ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.PowerBI.SkipRestoreIncomingChannel_B8815F3E-16EF-447D-9FDA-9305D1EF947E", teamMail);
                    ReportDto.Status = ReportStatus.Skipped;
                    logger.Info(message);
                }
                else
                {
                    RestoreChannel(entity, teamChannel);
                }
                RestoreConfig.FileNames = new HashSet<string>();
            }
            catch (Exception ex)
            {
                ReportDto.ErrorMessage = ex.Message;
                ReportDto.Status = ReportStatus.Failed;
                logger.Error($"Failed to restore the channel, title:{entity.Title}, error:{ex}.");
            }
            finally
            {
                Report.AddRestoreReport(ReportDto);
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
            }
        }
        private void CleanChannelCache()
        {
            _CurrentChannel = new ChannelCache();
        }

        private TeamChannel GetTeamChannelMetadata(ExchangeRestoreDataForBatch restoreData, MetadataEntity entity)
        {
            var metadata = restoreData.MetadataLists.FirstOrDefault(m => m.MetadataType == AveMetadataType.TeamsChannel);
            if (metadata == null)
            {
                var backupChannelInfos = _SourceMSTeamsEntity.TeamChannels.ToDictionary(itemArg => itemArg.Id, itemArg => itemArg);
                return GenerateRestoreChannel(entity, backupChannelInfos);
            }
            return SerializerHelper.DeserializeByDataContractSerializer<TeamChannel>(metadata.GetMetadata<string>());
        }

        private void RestoreChannel(MetadataEntity entity, TeamChannel teamChannel)
        {
            string targetChannelId = entity.Id;
            string teamAddress = entity.DisplayPath.Substring(0, entity.DisplayPath.IndexOf("\\"));
            var isNewCreateChannel = false;
            _CurrentChannel.DisplayName = entity.Title;
            _CurrentChannel.MembershipType = teamChannel.MembershipType;
            logger.Info("Start to restore Team Channel. SourceTeamName: [{0}]. ChannelName: [{1}]. ", teamAddress, entity.Title);
            if (!string.IsNullOrEmpty(_GroupId))
            {
                try
                {
                    var existedChannelBasicInfo = _ExistedChannels.Distinct(new ChannelComparer()).ToDictionary(itemArg => itemArg.DisplayName, itemArg => itemArg, StringComparer.OrdinalIgnoreCase);
                    var existedChannelInfos = _ExistedChannels.ToDictionary(itemArg => itemArg.Id, itemArg => itemArg);
                    if (_SiteNotFound) throw new Exception("Agent.Teams.SiteNotFound_152A5656-8624-4179-86C7-8684C2B1B5F0");
                    if (!_TeamsChannels.Contains((string)teamChannel.DisplayName)) _TeamsChannels.Add((string)teamChannel.DisplayName);

                    if (this.Config.RestoreType == EORestoreType.InPlace)
                    {
                        //treat current's and backed up team's primary channel as having the same id
                        if (teamChannel.Id.Equals(_SourceTeamIntenalId) && !teamChannel.Id.Equals(_TeamIntenalId))
                        {
                            targetChannelId = entity.Id = teamChannel.Id = _TeamIntenalId;
                        }
                        if (existedChannelInfos.ContainsKey((string)teamChannel.Id))
                        {
                            RestoreIdExistedChannel(teamAddress, existedChannelBasicInfo.Keys.ToList(), existedChannelInfos, (TeamChannel)teamChannel);
                        }
                        else
                        {
                            try
                            {
                                targetChannelId = RestoreIdNotExistedChannel(existedChannelBasicInfo, (TeamChannel)teamChannel, teamAddress);
                                isNewCreateChannel = true;
                            }
                            catch (Exception ex) when (ex.Message.Contains("Channel name already existed, please use other name"))
                            {
                                logger.Warn("Create channel failed. {0}", ex);
                                throw new Exception("Agent.Teams.ChannelInDeleted_F162DDAB-9DF0-4967-BFF3-C12D71B35DAA");
                            }
                            catch (Exception ex) when (ex.Message.Contains("Channel DisplayName already exists, please use another name."))
                            {
                                logger.Warn("Create channel failed DisplayName already exists. {0}", ex);
                                if (!existedChannelBasicInfo.TryGetValue(_CurrentChannel.DisplayName, out _))
                                {
                                    logger.Warn($"The site is hard deleted but channel is still in recycle bin of the Teams: {_GroupSiteUrl}. Channel name: {_CurrentChannel.DisplayName}");
                                    throw new Exception("RM_JM_Details_Teams_ChannelInTeamsRecycleBin_Error");
                                }
                                throw;
                            }
                        }
                        if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge && ReportDto.Status == ReportStatus.Skipped)
                        {
                            logger.Error($"Change the job report to success for overwrite restore job, title:{entity.Title}");
                            ReportDto.Status = ReportStatus.Success;
                            ReportDto.Option = RestoreOption.Overwritten.GetEnumDescription();
                            ReportDto.ErrorMessage = "";
                        }
                    }
                    else if (this.Config.RestoreType == EORestoreType.OutOfPlace)
                    {
                        if (!existedChannelBasicInfo.ContainsKey(entity.Title))
                        {//Create Channel and Restore all Tab
                            targetChannelId = NewChannel(teamChannel, RestoreConfig.CurrentRestoreMailbox);
                        }
                        else
                        {
                            logger.Warn("Membership status of channels with the same name is [{0} --> {1}]", (object)teamChannel.MembershipType, existedChannelBasicInfo[(string)teamChannel.DisplayName].MembershipType);
                            if (!string.IsNullOrEmpty((string)teamChannel.MembershipType) && !existedChannelBasicInfo[(string)teamChannel.DisplayName].MembershipType.Equals((string)teamChannel.MembershipType))
                            {
                                throw new Exception("Agent.Teams.ChannelTypeConflict_0EBAAFEE-B1A5-46FF-AABE-57521B12860E");
                            }
                            var channelId = existedChannelBasicInfo[entity.Title].Id;
                            targetChannelId = channelId;
                            if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
                            {//just restore tab
                                logger.Info("Start to update channel tabs. ");
                                var existedChannelTabs = TeamsService.GetChannelTabs(_GroupId, channelId);
                                var noOrderIndexTabs = teamChannel.ChannelTabs.FindAll(ct => string.IsNullOrEmpty(ct.SortOrderIndex));
                                var hasOrderIndexTabs = teamChannel.ChannelTabs.FindAll(ct => !string.IsNullOrEmpty(ct.SortOrderIndex));
                                SortChannelTab(hasOrderIndexTabs);
                                var existedTabDic = existedChannelTabs.ToDictionary(cT => cT.Id, cT => cT);
                                hasOrderIndexTabs.ForEach(cT => RestoreExistedChannelTabs(channelId, existedChannelTabs, existedTabDic, cT));
                                noOrderIndexTabs.ForEach(cT => RestoreExistedChannelTabs(channelId, existedChannelTabs, existedTabDic, cT));
                                logger.Info("Success to update channel tabs. ");
                            }
                            else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Skip)
                            {//skip, not restore tab
                                logger.Info("The container conflict is skip, no need to update channel. ");
                            }
                        }
                    }
                } 
                catch(Exception e)
                {
                    logger.Error("Restore Teams Channel with error {0}. ChannelName: {1}", e, entity.Title);
                    throw;
                }
                finally
                {
                    SetCurrentChannelFilesUrl(targetChannelId, entity.Title);
                }
            }
            _CurrentChannel.Id = targetChannelId;
            var isNewCreate = _CurrentChannel.IsStandardChannel() ? _IsNewlyCreatedGroupSite : isNewCreateChannel;
            TeamsRestoreState.AddAllowRestoreChannelSite(_CurrentChannel.FilesFolderUrl, isNewCreate);
            logger.Info("Finish to restore Team Channel. ChannelName: [{0}]. isNewCreateChannel: [{1}]", entity.Title, isNewCreateChannel);
        }

        private void SetCurrentChannelFilesUrl(string channelId, string channelName = null)
        {
            if (string.IsNullOrEmpty(channelName)) channelName = channelId;
            logger.Info("Start get channel[{0}] files url", channelName);
            try
            {
                Action action = delegate { _CurrentChannel.FilesFolderUrl = TeamsService.GetChannelFilesUrl(_GroupId, channelId); };
                var policyBuilder = Policy.Handle(delegate (Exception ex)
                {
                    if (ex.Message.Contains("Folder location for this channel is not ready yet, please try again later")) return true;
                    if (ex.Message.Contains("Sharepoint folder not found")) return true;
                    return false;
                });
                Func<int, TimeSpan> sleepDurationProvider = (int i) => { return TimeSpan.FromSeconds(20); };
                Action<Exception, TimeSpan, int, Context> onRetry = delegate (Exception ex, TimeSpan timeSpan, int retryTimes, Context context)
                { logger.Warn($"An exception occurred while getting the file folder url. Details: {ex.Message}, Start {retryTimes}th retry."); };
                var retryPolicy = policyBuilder.WaitAndRetry(5, sleepDurationProvider, onRetry);
                //var wrap = retryPolicy.Wrap(Policy.Timeout(TimeSpan.FromMinutes(5), Polly.Timeout.TimeoutStrategy.Optimistic));
                //wrap.Execute(action);
                retryPolicy.Execute(action);
            }
            catch (Exception ex)
            {
                logger.Error("Get channel files url failed. Error :{0}", ex.ToString());
                if (ex.Message.Contains("Site isn't available.") || ex.Message.Contains("Attempting to create site : False"))
                {
                    logger.Warn($"The site is hard deleted but channel is still in recycle bin of the Teams: {_GroupSiteUrl}. Channel name: {_CurrentChannel.DisplayName}");
                    throw new Exception("RM_JM_Details_Teams_ChannelSitePermanentlyDeleted_Error");
                }
                _CurrentChannel.FilesFolderUrl = string.Empty;
            }
        }

        private static TeamChannel GenerateRestoreChannel(MetadataEntity entity, Dictionary<string, TeamChannel> backupChannelInfos)
        {
            var channelExist = backupChannelInfos.ContainsKey(entity.Id);
            return new TeamChannel()
            {
                Id = entity.Id,
                DisplayName = entity.Title,
                Description = channelExist ? backupChannelInfos[entity.Id].Description : string.Empty,
                ChannelTabs = channelExist ? backupChannelInfos[entity.Id].ChannelTabs : null,
                MembershipType = channelExist ? backupChannelInfos[entity.Id].MembershipType : null,
                ChannelMembers = channelExist ? backupChannelInfos[entity.Id].ChannelMembers : null,
            };
        }

        private void RestoreIdExistedChannel(string restoreEntityTitle, List<string> existedChannelNames, Dictionary<string, TeamChannel> existedChannelInfos, TeamChannel tempChannel)
        {
            logger.Info("The same ID Team Channel exists in the team. ChannelName: {0}. ChannelId: {1}. TeamName: {2}", tempChannel.DisplayName, tempChannel.Id, restoreEntityTitle);
            if (NeedUpdateIdExistedChannel(existedChannelNames, existedChannelInfos, tempChannel))
            {
                try
                {
                    logger.Info("Update team channel display name and description. ");
                    TeamsService.SetTeamChannel(_GroupId, tempChannel.Id, tempChannel.DisplayName, tempChannel.Description);
                }
                catch (GraphAPIException ex)
                {
                    if (!ex.Message.Equals("General channel cannot be patched.", StringComparison.OrdinalIgnoreCase)) throw;
                    logger.Info($"Channel can not be updated : {ex.Message}");
                }
                ReportDto.Option = RestoreOption.Updated.GetEnumDescription();
            }
            else
            {
                ReportDto.Status = ReportStatus.Skipped;
                ReportDto.Option = RestoreOption.Skipped.GetEnumDescription();
                ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.Teams.ChannelExist_F125D124-6A2A-4C57-8364-F9964F0CA07C", existedChannelInfos[tempChannel.Id].DisplayName);
            }
            if (null != tempChannel.ChannelMembers)
            {
                RestoreExistedChannelMembers(_GroupId, tempChannel.Id, tempChannel.ChannelMembers);
            }
            if (tempChannel.ChannelTabs == null) logger.Info("The backup channel tabs is null. Please check the backup job. ");
            else UpdateChannelTabs(tempChannel.Id, tempChannel);
        }
        private bool NeedUpdateIdExistedChannel(List<string> existedChannelNames, Dictionary<string, TeamChannel> existedChannelInfos, TeamChannel tempChannel)
        {
            //Inplace restore and old team is deleted, have to rename new created general channel to backed up primary channel's name
            if (tempChannel.Id.Equals(_TeamIntenalId, StringComparison.OrdinalIgnoreCase) && _IsNewlyCreatedGroupSite && !tempChannel.DisplayName.Equals("general", StringComparison.OrdinalIgnoreCase)) return true;
            //考虑 channel 不能同名的问题
            if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge)
            {
                if(tempChannel.DisplayName.Equals("general", StringComparison.OrdinalIgnoreCase))
                {
                    _CurrentChannel.DisplayName = existedChannelInfos[tempChannel.Id].DisplayName;
                    return false;
                }
                bool descriptionChanged = existedChannelInfos[tempChannel.Id].Description != tempChannel.Description;//existedChanne 的 description 很可能为 null 
                bool nameChanged = !existedChannelInfos[tempChannel.Id].DisplayName.Equals(tempChannel.DisplayName, StringComparison.OrdinalIgnoreCase);
                bool hasSameNameChannel = existedChannelNames.Contains(tempChannel.DisplayName);
                bool needUpdateName = nameChanged && !hasSameNameChannel;
                if (needUpdateName) return true;
                tempChannel.DisplayName = null;//反序列化时会忽略空值，这里是为了避免发生同名错误。
                return descriptionChanged;
            }
            else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Skip)
            {
                _CurrentChannel.DisplayName = existedChannelInfos[tempChannel.Id].DisplayName;

            }
            return false;
        }
        private void UpdateChannelTabs(string channelId, TeamChannel tempChannel)
        {
            try
            {
                logger.Info("Start to update channel tabs. ");
                var existedChannelTabs = TeamsService.GetChannelTabs(_GroupId, channelId);
                var noOrderIndexTabs = tempChannel.ChannelTabs.FindAll(ct => string.IsNullOrEmpty(ct.SortOrderIndex));
                var hasOrderIndexTabs = tempChannel.ChannelTabs.FindAll(ct => !string.IsNullOrEmpty(ct.SortOrderIndex));
                SortChannelTab(hasOrderIndexTabs);
                if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge || _IsNewlyCreatedGroupSite)
                {
                    var existedTabDic = existedChannelTabs.ToDictionary(cT => cT.Id, cT => cT);
                    hasOrderIndexTabs.ForEach(cT => RestoreExistedChannelTabs(channelId, existedChannelTabs, existedTabDic, cT));
                    noOrderIndexTabs.ForEach(cT => RestoreExistedChannelTabs(channelId, existedChannelTabs, existedTabDic, cT));
                }
                #region for conflict replace
                //else if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Overwrite) //for replace
                //{
                //    try
                //    {
                //        existedChannelTabs.ForEach(cT => exchangeMicrosoftTeams.DeleteChannelTab(GroupId, channelId, cT.Id));
                //        tempChannel.ChannelTabs.ForEach(cT => exchangeMicrosoftTeams.AddChannelTab(GroupId, channelId, cT));
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Error("Failed to update channel tabs. ChannelName: {0}. Reason: {1}.", tempChannel.DisplayName, ex.ToString());
                //    }
                //}
                #endregion
                logger.Info("Success to update channel tabs. ");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to update channel tabs. Reason: {0}.", ex.ToString());
            }
        }

        private void RestoreExistedChannelTabs(string channelId, List<ChannelTab> existedChannelTabs, Dictionary<string, ChannelTab> existedTabDic, ChannelTab tempTab)
        {
            logger.Info("Start to restore tab. TabName:{0}. TabId: {1}. TeamsAppId:{2}. TabConfig:{3}. ", tempTab.DisplayName, tempTab.Id, tempTab.TeamsAppId, tempTab.Configuration);
            try
            {
                if (existedTabDic.ContainsKey(tempTab.Id))
                {
                    logger.Info("The tab is judged existed by tab id. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);

                    var restoreTab = TabFactory.CreateTabConfig(tempTab, EntityIdDic, RestoreConfig.TenantIdMap, SiteUrlDic);

                    if (IsExistedTabNeedUpdate(existedTabDic[tempTab.Id], restoreTab))
                    {
                        logger.Info("Dest tab info. TabName:{0}. TabId: {1}. TeamsAppId:{2}. TabConfig:{3}. ", existedTabDic[tempTab.Id].DisplayName, existedTabDic[tempTab.Id].Id, existedTabDic[tempTab.Id].TeamsAppId, existedTabDic[tempTab.Id].Configuration);
                        logger.Info("The tab needs to update. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);
                        
                        TeamsService.UpdateChannelTab(_GroupId, channelId, restoreTab);
                        RecordPlannerTab(channelId, tempTab.Id, tempTab, restoreTab);
                        logger.Info("Success to update tab. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);
                    }
                }
                else
                {
                    AddNewTab(channelId, existedChannelTabs, tempTab);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to add or update channel tab. TabName: {0}. TabId: {1}. Reason: {2}.", tempTab.DisplayName, tempTab.Id, ex.ToString());
            }
            logger.Info("Success to restore channel tab. TabName:{0}. TabId: {1}. TeamsAppId:{2}. ", tempTab.DisplayName, tempTab.Id, tempTab.TeamsAppId);
        }

        private void AddNewTab(string channelId, List<ChannelTab> existedChannelTabs, ChannelTab tempTab)
        {
            logger.Info("The tab is not existed by tab id. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);
            if (!IsTabExisted(existedChannelTabs, tempTab))
            {
                logger.Info("The tab needs to add. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);
                tempTab.DisplayName = GetTruncateTabName(tempTab.DisplayName);

                var restoreTab = TabFactory.CreateTabConfig(tempTab, EntityIdDic, RestoreConfig.TenantIdMap, SiteUrlDic);
                var tabId = TeamsService.AddChannelTab(_GroupId, channelId, restoreTab);
                RecordPlannerTab(channelId, tabId, tempTab, restoreTab);
                logger.Info("Success to add tab. TabName:{0}. TabId: {1}.", tempTab.DisplayName, tempTab.Id);
            }
        }

        private void RecordPlannerTab(string channelId, string tabId, ChannelTab tempTab, RestoreTab restoreTab)
        {
            if (tempTab.TeamsAppId.Equals(BuiltInTabTeamAppsId.Planner, StringComparison.OrdinalIgnoreCase))
            {
                var planTabConfig = TabFactory.CreateTabConfig(tempTab, new Dictionary<string, string>(), RestoreConfig.TenantIdMap).Configuration;
                //var planTabConfig = new Tab() { TeamsAppId = tempTab.TeamsAppId, Configuration = tempTab.Configuration }.KnownConfiguration;
                if (!string.IsNullOrEmpty(planTabConfig.EntityId)) _PlannerTabs.Add(new PlannerTabUpdateObj() { ChannelId = channelId, TabId = tabId, PlannerId = planTabConfig.EntityId, ChannelTab = tempTab, });
            }
            else if (restoreTab is FileRestoreTab)
            {
                var entityId = restoreTab.Configuration.EntityId;
                if (!string.IsNullOrEmpty(restoreTab.Configuration.EntityId))
                {
                    _FileTabs.Add(new FileTabUpdateObj() { ChannelId = channelId, TabId = tabId, EntityId = restoreTab.Configuration.EntityId, RestoreTab = restoreTab, });
                    if (Guid.TryParse(entityId, out Guid oldId))
                    {
                        AvePoint.Wrapper.Common.WrapperConfiguration.ChannelTabEntityIdMapping[oldId] = Guid.Empty;
                        logger.Info($"Cache entity id to WrapperConfiguration.ChannelTabEntityIdMapping. id:{oldId}");
                    }
                }
            }
        }

        private string GetTruncateTabName(string tabName)
        {
            if (string.IsNullOrEmpty(tabName)) return tabName;
            if (tabName.Length > 128)
            {
                logger.Warn("The tab name is too long, truncate it to 128 characters. TabName: {0}.", tabName);
                return tabName.Substring(0, 128);
            }
            return tabName;
        }

        private bool IsTabExisted(List<ChannelTab> existedChannelTabs, ChannelTab tempTab)
        {
            bool isExisted = false;
            foreach (var channelTab in existedChannelTabs)
            {
                if (channelTab.DisplayName.Equals(tempTab.DisplayName, StringComparison.OrdinalIgnoreCase) && channelTab.TeamsAppId.Equals(tempTab.TeamsAppId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info("The same tab info: TabId: {0}, Displayname: {1}, AppId: {2}. ", channelTab.Id, channelTab.DisplayName, channelTab.TeamsAppId);
                    isExisted = true;
                    break;
                }
            }
            return isExisted;
        }

        private bool IsExistedTabNeedUpdate(ChannelTab destTab, RestoreTab sourTab)
        {
            if (!destTab.DisplayName.Equals(sourTab.ChannelTab.DisplayName, StringComparison.OrdinalIgnoreCase)) return true;
            var sourConfig = sourTab.Configuration;
            if (sourConfig == null) return false;
            var destConfig = TabFactory.CreateTabConfig(destTab);
            if (destConfig == null) return true;
            return sourTab.ChannelTab switch
            {
                { TeamsAppId: var teamsAppId } when BuiltInTabTeamAppsId.Planner.EqualsIgnoreCase(teamsAppId)
                                                    => !sourConfig.EntityId.EqualsIgnoreCase(destConfig.EntityId),
                { TeamsAppId: var teamsAppId } when BuiltInTabTeamAppsId.DocumentLibrary.EqualsIgnoreCase(teamsAppId)
                                                    => !sourConfig.ContentUrl.EqualsIgnoreCase(destConfig.ContentUrl),
                { TeamsAppId: var teamsAppId } when BuiltInTabTeamAppsId.IsOfficeTab(teamsAppId)
                                                    => !sourConfig.ContentUrl.EqualsIgnoreCase(destConfig.ContentUrl) ||
                                                       !sourConfig.EntityId.EqualsIgnoreCase(destConfig.EntityId),
                _ => false
            };
        }

        private ConfigurationBase GetRestoreTab(ChannelTab tempTab)
        {

            ConfigurationBase tabConfig = null;
            var tempConfig = TabFactory.CreateTabConfig(tempTab, EntityIdDic, RestoreConfig.TenantIdMap, SiteUrlDic).Configuration;
            if (tempConfig != null)
                tabConfig = new ConfigurationBase()
                {
                    EntityId = !string.IsNullOrEmpty(tempConfig.EntityId) ? tempConfig.EntityId : string.Empty,
                    ContentUrl = !string.IsNullOrEmpty(tempConfig.ContentUrl) ? tempConfig.ContentUrl : string.Empty,
                };
            return tabConfig;
        }

        private string RestoreIdNotExistedChannel(Dictionary<string, TeamChannel> existedChannelBasicInfo, TeamChannel tempChannel, string teamAddress)
        {
            var targetChannelId = string.Empty;
            if (!existedChannelBasicInfo.TryGetValue(tempChannel.DisplayName, out TeamChannel existChannel))
            {
                targetChannelId = NewChannel(tempChannel, teamAddress);
                if (!string.IsNullOrEmpty(targetChannelId) && null != tempChannel.ChannelMembers)
                {
                    RestoreChannelMembers(_GroupId, targetChannelId, tempChannel.ChannelMembers);
                }
            }
            else
            {
                targetChannelId = existChannel.Id;
                logger.Warn("Membership status of channels with the same name is [{0} --> {1}], the channel id is [{2}]", tempChannel.MembershipType, existedChannelBasicInfo[tempChannel.DisplayName].MembershipType, targetChannelId);
                if (!string.IsNullOrEmpty(tempChannel.MembershipType) && !existedChannelBasicInfo[tempChannel.DisplayName].MembershipType.Equals(tempChannel.MembershipType))
                {
                    throw new Exception("Agent.Teams.ChannelTypeConflict_0EBAAFEE-B1A5-46FF-AABE-57521B12860E");
                }
                if (this.Config.ContainerConflictResolution == EOConflictResolutionType.Merge
                    && !existChannel.DisplayName.Equals("General", StringComparison.OrdinalIgnoreCase)
                    && existChannel.Description != tempChannel.Description)
                {
                    try
                    {
                        tempChannel.DisplayName = null;
                        logger.Info("Update team channel description.");
                        TeamsService.SetTeamChannel(_GroupId, existChannel.Id, tempChannel.DisplayName, tempChannel.Description);
                    }
                    catch (GraphAPIException ex)
                    {
                        if (!ex.Message.Equals("General channel cannot be patched.", StringComparison.OrdinalIgnoreCase)) throw;
                        logger.Info($"Channel can not be updated : {ex.Message}");
                    }
                    ReportDto.Option = RestoreOption.Updated.GetEnumDescription();
                }
                else
                {
                    ReportDto.Status = ReportStatus.Skipped;
                    ReportDto.Option = RestoreOption.Skipped.GetEnumDescription();
                    ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.Teams.ChannelExist_F125D124-6A2A-4C57-8364-F9964F0CA07C", tempChannel.DisplayName);
                    logger.Info("Channel with the same display name exists in the team, no need to create new. ChannelName: {0}. TeamName: {1}", tempChannel.DisplayName, teamAddress);
                }
                //InitChannelSite(TeamIntenalId, targetChannelId);
                if (!string.IsNullOrEmpty(targetChannelId) && null != tempChannel.ChannelMembers)
                {
                    RestoreExistedChannelMembers(_GroupId, targetChannelId, tempChannel.ChannelMembers);
                }
                if (tempChannel.ChannelTabs == null) logger.Info("The backup channel tabs is null. Please check the backup job. ");
                else UpdateChannelTabs(existChannel.Id, tempChannel);
            }
            return targetChannelId;
        }

        #region Create Channel
        private string NewChannel(TeamChannel tempChannel, string teamAddress)
        {
            logger.Info("Create Team Channel. ChannelName: {0}, ChannelId: {1}, MembershipType: {2}, TeamName: {3}.", tempChannel.DisplayName, tempChannel.Id, tempChannel.MembershipType, teamAddress);
            string channelId = string.Empty;
            if (tempChannel.IsPrivateChannel())
            {
                channelId = CreatePrivateChannel(tempChannel);
            }
            else if (tempChannel.IsSharedChannel())
            {
                try
                {
                    channelId = CreateSharedChannel(tempChannel);
                }
                catch (GraphAPIException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.BadRequest && e.Message.Contains("UnknownError", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Agent.Teams.CreateChannelUnknownError_ECF3C523-DBD4-4D33-8AED-E35605005A5F");
                }
            }
            else
            {
                channelId = TeamsService.CreateTeamChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, UseMigrationMode ? new DateTime(OldestMessageDate).ToUniversalTime().ToString("O") : null);
                if (UseMigrationMode)
                {
                    MigrationChannelIds.Add(channelId);
                }
            }
            ReportDto.Status = ReportStatus.Success;
            if (!string.IsNullOrEmpty(channelId)) NewChannelTabs(tempChannel, channelId);
            else logger.Error("The channelId is null, can't restore the channel tabs. ChannelName: {0}. TeamName: {1}", tempChannel.DisplayName, teamAddress);
            return channelId;
        }
        private string CreatePrivateChannel(TeamChannel tempChannel)
        {
            //var authObjSA = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
            var authObjSA = M365APIService.graphApplicationAuthObj;
            
            if (authObjSA.AuthType == AuthObjectType.AccessToken)
            {
                if (null == tempChannel.ChannelMembers) throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                if (null == _ExistedTeamUsers)
                {
                    foreach (var user in tempChannel.ChannelMembers.Where(user => user.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            logger.Info("Select user[{0}], id [{1}], trying to create private channel.", user.Email, user.UserId);
                            return TeamsService.CreatePrivateChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, user.UserId);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("is not part of team") || ex.Message.Contains("no members specified when calling from an Application"))
                            {
                                logger.Warn("Failed to create private channel using user[{0}]. Reason：{1}", user.Email, ex.ToString());
                                continue;
                            }
                            throw;
                        }
                    }
                    throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                }
                else
                {
                    var channelOwnerId = string.Empty;
                    var existUserIds = _ExistedTeamUsers.Select(user => user.UserId).ToList();
                    foreach (var user in tempChannel.ChannelMembers.Where(user => user.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase)))
                    {
                        if (existUserIds.Contains(user.UserId))
                        {
                            channelOwnerId = user.UserId;
                            logger.Info("Match to user[{0}], id [{1}], start to create private channel.", user.Email, user.UserId);
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(channelOwnerId)) throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                    return TeamsService.CreatePrivateChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, channelOwnerId);
                }
            }
            else
            {   //service account create private channel 会指定自己为channel owner
                logger.Info("Start create private channel[{0}] with {1}.", tempChannel.DisplayName, authObjSA.UserName);
                using (var exchangeTeamPreferredSA = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObjSA))
                {
                    logger.Info("Current certification type: {0}, Scope of use : Create private channel.", authObjSA.AuthType);
                    var newId = exchangeTeamPreferredSA.CreatePrivateChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description);
                    InitChannelSite(_TeamIntenalId, newId);
                    return newId;
                }
            }
        }

        private string CreateSharedChannel(TeamChannel tempChannel)
        {
            //var authObjSA = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
            var authObjSA = M365APIService.graphApplicationAuthObj;
            
            if (authObjSA.AuthType == AuthObjectType.AccessToken)
            {
                if (null == tempChannel.ChannelMembers) throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                if (null == _ExistedTeamUsers)
                {
                    foreach (var user in tempChannel.ChannelMembers.Where(user => user.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            logger.Info("Select user[{0}], id [{1}], trying to create shared channel.", user.Email, user.UserId);
                            return TeamsService.CreateSharedChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, user.UserId);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("is not part of team") || ex.Message.Contains("no members specified when calling from an Application"))
                            {
                                logger.Warn("Failed to create shared channel using user[{0}]. Reason：{1}", user.Email, ex.ToString());
                                continue;
                            }
                            throw;
                        }
                    }
                    throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                }
                else
                {
                    var channelOwnerId = string.Empty;
                    var existUserIds = _ExistedTeamUsers.Where(u => u.RoleType == TeamMemberRoleType.Owner).Select(user => user.UserId).ToList();
                    foreach (var user in tempChannel.ChannelMembers.Where(user => user.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase)))
                    {
                        if (existUserIds.Contains(user.UserId))
                        {
                            channelOwnerId = user.UserId;
                            logger.Info("Match to user[{0}], id [{1}], start to create shared channel.", user.Email, user.UserId);
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(channelOwnerId)) throw new Exception("Agent.Teams.NoAvailableOwner_55AEF688-FC22-4720-A4E5-DEA3F6BE1CE1");
                    return TeamsService.CreateSharedChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, channelOwnerId);
                }
            }
            else
            {
                logger.Info("Start create shared channel[{0}] with {1}.", tempChannel.DisplayName, authObjSA.UserName);
                using (var exchangeTeamPreferredSA = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObjSA))
                {
                    logger.Info("Current certification type: {0}, Scope of use : Create shared channel.", authObjSA.AuthType);
                    var newId = exchangeTeamPreferredSA.CreateSharedChannel(_GroupId, tempChannel.DisplayName, tempChannel.Description, GetSAUserId(exchangeTeamPreferredSA));
                    //InitChannelSite(TeamIntenalId, newId);//Invalid for shared channels.
                    return newId;
                }
            }
        }

        private string GetSAUserId(MicrosoftTeamsAPIBase exchangeTeamPreferredSA)
        {
            try
            {
                var userName = (exchangeTeamPreferredSA.AuthObject as ServiceAccout2AppTokenAuthObject).UserName;
                if (userName.IsNotNullOrEmpty() && GlobalCache.UserIdMap.TryGetValue(userName, out var Id))
                    return Id;
                var me = exchangeTeamPreferredSA.GetMe();
                GlobalCache.UserIdMap[me.UserPrincipalName] = me.Id;
                return me.Id;
            }
            catch (Exception ex)
            {
                logger.Info($"Failed to get Service account user id.");
            }
            return null;
        }

        #endregion

        #region InitChannelSite
        public bool InitChannelSite(string internalId, string channelId)
        {
            var authObjSA = AuthorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
            if (!string.IsNullOrEmpty(_TeamIntenalId))
            {
                var service = InitSkypService(authObjSA);
                if (null != service)
                {
                    logger.Info("Start to init channel site.");
                    return RetryForInitChannelSite(() => { service.InitPrivateChannelSite(internalId, channelId); });
                }
            }
            return false;
        }
        private ExchangeUtility.Graph.SkypeAPI.TeamsSkypeService InitSkypService(AuthObject authObject)
        {
            var authSA = authObject as AOSTokenAuthObjectV2;
            if (null == authSA) return null;
            try
            {
                var service = new ExchangeUtility.Graph.SkypeAPI.TeamsSkypeService();
                service.LoginWithSkypeTokenOnly(authSA.TenantId, authSA.UserName, authSA.TokenProvider);

                return service;
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot log in to the Teams Skype service. Reason :{0}", ex.ToString());
                return null;
            }
        }

        private bool RetryForInitChannelSite(Action action)
        {
            bool isSuccessed = false;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int cursor = 0;
            //正常sa 自动初始化需要10秒左右，代码初始化需要 4.8秒 ~ 5.8 秒；
            //正常app only 无法自动初始化，代码初始化需要 5分6秒 ~ 5分10秒；
            Int32[] waitTimeArray = new Int32[] { 6, 10, 10, 35, 60, 120, 60, 10, 20, 30, };
            do
            {
                try
                {
                    action();
                    isSuccessed = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (cursor < waitTimeArray.Length)
                    {
                        Thread.Sleep(waitTimeArray[cursor] * 1000);
                    }
                    else
                    {
                        logger.Warn("Failed to init private channel site. Reason: {0}", ex.Message);
                        break;
                    }
                }
            }
            while (cursor++ < waitTimeArray.Length);
            watch.Stop();
            logger.Info("Initialization result {0}, cursor position:{1}, Toltal time is :{2}.", isSuccessed, cursor, watch.ElapsedMilliseconds);
            return isSuccessed;
        }
        #endregion
        private void NewChannelTabs(TeamChannel tempChannel, string channelId)
        {
            try
            {
                if (tempChannel.ChannelTabs == null)
                {
                    logger.Info("The backup channel tabs is null. Please check the backup job. ");
                }
                else
                {
                    logger.Info("Start to add channel tabs. TabCount: {0}. ", tempChannel.ChannelTabs.Count);
                    var existedChannelTabs = TeamsService.GetChannelTabs(_GroupId, channelId);
                    logger.Info("Existed channel tab count: {0}. ", existedChannelTabs.Count);
                    var noOrderIndexTabs = tempChannel.ChannelTabs.FindAll(ct => string.IsNullOrEmpty(ct.SortOrderIndex));
                    var hasOrderIndexTabs = tempChannel.ChannelTabs.FindAll(ct => !string.IsNullOrEmpty(ct.SortOrderIndex));
                    SortChannelTab(hasOrderIndexTabs);
                    hasOrderIndexTabs.ForEach(cT => AddNewTab(channelId, existedChannelTabs, cT));
                    noOrderIndexTabs.ForEach(cT => AddNewTab(channelId, existedChannelTabs, cT));
                    //hasOrderIndexTabs.ForEach(cT => RecordPlannerTab(channelId, exchangeMicrosoftTeams.AddChannelTab(GroupId, channelId, TabFactory.CreateTabConfig(cT, EntityIdDic, RestoreConfig.TenantIdMap, SiteUrlDic)), cT));
                    //noOrderIndexTabs.ForEach(cT => RecordPlannerTab(channelId, exchangeMicrosoftTeams.AddChannelTab(GroupId, channelId, TabFactory.CreateTabConfig(cT, EntityIdDic, RestoreConfig.TenantIdMap, SiteUrlDic)), cT));
                    logger.Info("Success to add channel tabs. ");
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to new channel tabs. Reason: {0}.", ex.ToString());
            }
        }

        private static void SortChannelTab(List<ChannelTab> hasOrderIndexTabs)
        {
            try
            {
                hasOrderIndexTabs.ForEach(cT => logger.Info("TabInfo. Name: {0}. Id: {1}. SordId: {2}. AppId: {3}.", cT.DisplayName, cT.Id, cT.SortOrderIndex, cT.TeamsAppId));
                hasOrderIndexTabs.Sort((cT1, cT2) => Convert.ToDouble(cT1.SortOrderIndex).CompareTo(Convert.ToDouble(cT2.SortOrderIndex)));
            }
            catch (Exception ex)
            {
                logger.Info("An error occurred while to sort the channel tabs. Reason: {0}. ", ex.ToString());
            }
        }
        private void RestoreExistedChannelMembers(string teamId, string channelId, List<ExchangeCommonWrapper.ChannelMember> channelMembers)
        {
            //The roles for that user. Must be "owner" or empty. Guest users must always have role "guest" and cannot change.
            if (this.Config.ContainerConflictResolution != EOConflictResolutionType.Merge) return;
            try
            {
                var existedMember = TeamsService.ListChannelMembers(teamId, channelId).ToDictionary(user => user.UserId);
                var listToUpdate = new List<ExchangeCommonWrapper.ChannelMember>();
                var listToAdd = new List<ExchangeCommonWrapper.ChannelMember>();
                foreach (var user in channelMembers)
                {
                    if (existedMember.TryGetValue(user.UserId, out ExchangeCommonWrapper.ChannelMember eUser))
                    {
                        if (user.Roles.Any() != eUser.Roles.Any())
                        {
                            user.Id = eUser.Id;// user.Id 有值时需要重新赋值为 "" 或新的 member id； member id 是由 channelId 和 userId组成的。
                            listToUpdate.Add(user);
                        }
                    }
                    else
                    {
                        listToAdd.Add(user);
                    }
                }
                RestoreChannelMembers(teamId, channelId, listToAdd);
                if (!listToUpdate.Any()) return;
                logger.Info("Start to update member role.");
                foreach (var member in listToUpdate.Where(user => user.Roles.Any()))
                {//member=>owner
                    try
                    {
                        TeamsService.UpdateChannelMemberRoles(teamId, channelId, member);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while adding owner role to {0}. Reason: {1}", member.Email, ex.ToString());
                    }
                }
                //foreach (var member in listToUpdate.Where(user => !user.Roles.Any()))
                //{//owner=>member
                //    try
                //    {
                //        exchangeMicrosoftTeams.UpdateChannelMemberRoles(teamId, channelId, member);
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Warn("An error occurred while removing owner role from {0}. Reason: {1}", member.Email, ex.ToString());
                //    }
                //}

            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while restoring private channel members. Reason: {0}", ex.ToString());
            }

        }
        private void RestoreChannelMembers(string teamId, string channelId, List<ExchangeCommonWrapper.ChannelMember> channelMembers)
        {
            foreach (var member in channelMembers)
            {
                try
                {
                    TeamsService.AddChannelMember(teamId, channelId, member);
                }
                catch (Exception ex)
                {
                    logger.Warn($"Filed to add {member.DisplayName} to channel. Reason: {ex.Message}");
                }
            }
        }

    }

    public class ChannelComparer : IEqualityComparer<TeamChannel>
    {
        public bool Equals(TeamChannel x, TeamChannel y)
        {
            if (x == null || y == null) return false;
            return x.DisplayName.Equals(y.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(TeamChannel obj)
        {
            return obj.DisplayName.GetHashCode();
        }
    }
}