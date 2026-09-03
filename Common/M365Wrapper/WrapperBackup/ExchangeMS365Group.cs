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

namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using M365.Wrapper.Backup.Auth.Common;
    using Microsoft365.Configuration;
    using Microsoft365.SharePoint.Extension;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;

    public abstract class Microsoft365GroupServiceBase : IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(Microsoft365GroupServiceBase));
        protected MicrosoftGraphAPIService msGraphAPIService;

        public IAuthObject AuthObject { get; private set; }

        public Microsoft365GroupServiceBase(IAuthObject authObject) => AuthObject = authObject;

        #region Backup
        public Office365GroupEntityV2 GetO365GroupDetails(string o365GroupName)
        {
            logger.Info("Get office 365 group details.[{0}]", o365GroupName);
            var groupDetailsEntity = GetO365GroupDetail(o365GroupName);
            groupDetailsEntity.GroupMemberList = GetO365GroupOwnerAndMembers(groupDetailsEntity.ExternalDirectoryObjectId);
            return groupDetailsEntity;
        }

        public Office365GroupEntityV2 GetO365GroupDetail(string o365GroupName)
        {
            var groupInfo = FindGroup(o365GroupName);
            if (null == groupInfo) throw new Exception($"No group named [{o365GroupName}] was found.");
            (groupInfo.ExtraSettings, groupInfo.GroupResources) = GetExternalInfo(groupInfo.ExternalDirectoryObjectId, false);
            return groupInfo;
        }

        public Office365GroupEntityV2 GetO365GroupDetailById(string id)
        {
            var groupInfo = msGraphAPIService.GetGroupById(id)?.ToM();
            if (groupInfo is not null)
            {
                (groupInfo.ExtraSettings, groupInfo.GroupResources) = GetExternalInfo(groupInfo.ExternalDirectoryObjectId, false);
            }
            return groupInfo;
        }

        public List<AssignedLabelsV2> GetO365GroupAssignedLabelById(string id)
        {
            var assignedLabels = msGraphAPIService.GetGroupAssignedLabelById(id)?.ConvertAll(_ => new AssignedLabelsV2
            {
                DisplayName = _.DisplayName,
                LabelId = _.LabelId
            }).ToList() ?? new List<AssignedLabelsV2>();
            return assignedLabels;
        }

        /// <summary>
        /// return group base info  or  null 
        /// </summary>
        public Office365GroupEntityV2 FindGroup(string o365GroupName)
        {
            var groupInfo = msGraphAPIService.GetGroupInfoByAddress(o365GroupName);
            if (groupInfo == null)
            {
                logger.Warn("Get group by address failed, try to get group by mail nickname. GroupName: {0}", o365GroupName);
                groupInfo = msGraphAPIService.GetGroupByMailNickName(o365GroupName);
            }
            return groupInfo?.ToM();
        }

        public GroupResourceV2[] GetGroupResourceV2(string groupId)
        {
            var result = GetSiteInfoWithRetry(groupId);
            if (result == null) return null;

            var tRSite = result[SimpleItemId.GetTenantRootSite];
            var gSite = result[SimpleItemId.GetGroupSite];
            var gDrive = result[SimpleItemId.GetGroupDrive];
            logger.Info("GetTenantRootSite [{0}]; GetGroupSite [{1}]; GetGroupDrive [{2}]", tRSite.IsSuccessStatusCode, gSite.IsSuccessStatusCode, gDrive.IsSuccessStatusCode);

            var tenantRootSiteUrl = tRSite.IsSuccessStatusCode
                ? tRSite.ToObject<GetGroupSiteObj>().WebUrl.Trim('/')
                : string.Empty;
            var siteUrl = gSite.IsSuccessStatusCode
                ? gSite.ToObject<GetGroupSiteObj>().WebUrl
                : $"{tenantRootSiteUrl}/_layouts/groupstatus.aspx?id={groupId}&target=site";
            var filesUrl = gDrive.IsSuccessStatusCode
                ? gDrive.ToObject<GetGroupDriveObj>().WebUrl
                : $"{tenantRootSiteUrl}/_layouts/groupstatus.aspx?id={groupId}&target=documents";

            return new GroupResourceV2[]
            {
                new GroupResourceV2() { Type = GroupResouceTypeV2.Site, Url = siteUrl },
                new GroupResourceV2() { Type = GroupResouceTypeV2.Files, Url = filesUrl },
            };
        }

        private IBatchRequestCollection CreateRequestSiteInfoObject(string groupId)
        {
            var batchRequestObj = msGraphAPIService.CreateBatchRequestObj();
            var getTenantRootSite = new BatchItem_GetTenantRootSite(SimpleItemId.GetTenantRootSite, "?$select=webUrl");
            var getGroupSite = new BatchItem_GetGroupSite(SimpleItemId.GetGroupSite, groupId, "?$select=webUrl");
            var getGroupDrive = new BatchItem_GetGroupDrive(SimpleItemId.GetGroupDrive, groupId, "?$select=webUrl");
            batchRequestObj.Add(getTenantRootSite);
            batchRequestObj.Add(getGroupSite);
            batchRequestObj.Add(getGroupDrive);
            return batchRequestObj;
        }

        private Dictionary<string, ResponseItem> GetSiteInfoWithRetry(string groupId)
        {
            var batchRequestObj = CreateRequestSiteInfoObject(groupId);
            var retryTimes = 0;
            do
            {
                try
                {
                    var result = batchRequestObj.SentRequest();
                    var resultDic = result.ToDictionary(key => key.Id);
                    var tRSite = resultDic[SimpleItemId.GetTenantRootSite];
                    var gSite = resultDic[SimpleItemId.GetGroupSite];
                    var gDrive = resultDic[SimpleItemId.GetGroupDrive];
                    // Add diagnostic logs for the AOSBR-19385 to show the error message
                    result.ForEach(item =>
                    {
                        if (!item.IsSuccessStatusCode)
                        {
                            logger.Warn($"{item.Id} : {item.Body?.ToString()}");
                        }
                    });

                    if ((!gSite.IsSuccessStatusCode || !gDrive.IsSuccessStatusCode) && !tRSite.IsSuccessStatusCode)
                    {
                        result.ForEach(item =>
                        {
                            if (item.IsSuccessStatusCode)
                            {
                                logger.Info($"{item.Id} : {item.Body?.ToString()}");
                            }
                        });
                    }
                    else
                    {
                        return resultDic;
                    }
                }
                catch (Exception ex)
                {
                    if (retryTimes < 5)
                    {
                        logger.Error(ex.ToString());
                    }
                    else throw;
                }
                if (retryTimes < 5)
                {
                    Thread.Sleep(60000);
                    logger.Warn("Get site info failed, start {0}th retry.", retryTimes);
                }
            }
            while (retryTimes++ < 5);
            return null;
        }

        public List<GroupMemberV2> GetO365GroupOwnerAndMembers(String groupId)
        {
            try
            {
                var owners = msGraphAPIService.ListGroupOwners(groupId).ToList();
                var members = msGraphAPIService.ListGroupMembers(groupId).ToList();
                var ownerDic = owners.ToDictionary(key => key.Id, value => value.ToGroupMember(true, false));
                members.ForEach(user =>
                {
                    if (ownerDic.ContainsKey(user.Id))
                    {
                        ownerDic[user.Id].IsMember = true;
                        logger.Info("[{0}] is owner and member.", user.UserPrincipalName);
                    }
                    else
                    {
                        ownerDic.Add(user.Id, user.ToGroupMember(false, true));
                    }
                });
                return ownerDic.Values.ToList();
            }
            catch (Exception ex)
            {
                logger.Info("Failed to get group owners/members, Reason: {0}", ex);
                if (!ex.Message.Contains("Unsupported token. Unable to initialize the authorization context."))
                {
                    throw;
                }
                return null;
            }
        }
        public List<GroupMemberV2> GetO365GroupOwnerAndMembers2(String groupId)
        {
            var owners = msGraphAPIService.GetGroupOwners(groupId);
            var members = msGraphAPIService.GetGroupMembers(groupId);
            var ownerDic = owners.ToDictionary(key => key.Id, value => (value as Microsoft.Graph.Models.User).ToGroupMember(true));
            members.ForEach(user =>
            {
                try
                {
                    ownerDic.Add(user.Id, (user as Microsoft.Graph.Models.User).ToGroupMember(false));
                }
                catch (Exception e)
                {
                    logger.Error($"The group id/members was not found.error message : {e.Message}");
                }
            });
            return ownerDic.Values.ToList();
        }

        private GroupExtraInfo GetGroupExtraSettings(string groupId, bool throwException = true)
        {
            try
            {
                return msGraphAPIService.GetGroupExtraSettings(groupId);
            }
            catch (Exception ex)
            {
                logger.Warn("Unable to get the group extra settings. Reason: {0}", ex);
                if (throwException)
                {
                    throw;
                }
            }
            return null;
        }

        private (ExtraSettings, GroupResourceV2[]) GetExternalInfo(string id, bool throwException = true) =>
            (GetGroupExtraSettings(id, throwException)?.ToM(), GetGroupResourceV2(id));

        #endregion


        #region Restore
        public TeamInfo CreateO365Group(Office365GroupEntityV2 office365GroupEntity, bool needUpdateDataLocation = false)
        {
            logger.Info("Create office 365 group.");
            string mail = string.Empty;
            string id = string.Empty;
            if (needUpdateDataLocation && !string.IsNullOrEmpty(office365GroupEntity.PreferredDataLocation))
            {
                logger.Info("The dataLocation of restoring group is: {0}.", office365GroupEntity.PreferredDataLocation);
                var groupToCreate = new Group()
                {
                    DisplayName = office365GroupEntity.DisplayName,
                    MailNickname = office365GroupEntity.SmtpAddress.Substring(0, office365GroupEntity.SmtpAddress.LastIndexOf('@')),
                    Description = office365GroupEntity.Description,
                    GroupTypes = new String[] { "Unified" },
                    MailEnabled = true,
                    SecurityEnabled = false,
                    Visibility = office365GroupEntity.AccessType.ToString(),
                    PreferredDataLocation = office365GroupEntity.PreferredDataLocation,
                };
                var group = msGraphAPIService.CreateUnifiedGroup(groupToCreate);
                mail = group.Mail;
                id = group.Id;
            }
            else
            {
                var groupToCreate = new Group()
                {
                    DisplayName = office365GroupEntity.DisplayName,
                    MailNickname = office365GroupEntity.SmtpAddress.Substring(0, office365GroupEntity.SmtpAddress.LastIndexOf('@')),
                    Description = office365GroupEntity.Description,
                    GroupTypes = new String[] { "Unified" },
                    MailEnabled = true,
                    SecurityEnabled = false,
                    Visibility = office365GroupEntity.AccessType.ToString(),
                };
                var group = msGraphAPIService.CreateUnifiedGroup(groupToCreate);
                mail = group.Mail;
                id = group.Id;
            }
            logger.Info("Create Group Address: {0}.", mail);
            string groupAddress = GetGroupAddressWithRetry(id);
            logger.Info("ReGet Group Address: {0}.", groupAddress);
            return new TeamInfo() { GroupId = id, Mail = groupAddress };
            //return group.Id;
        }

        public string GetGroupAddressWithRetry(string groupId, int retry = 5)
        {
            int delayMs = 5000;
            for (int i = 1; i <= retry; ++i)
            {
                try
                {
                    return msGraphAPIService.GetGroupInfoById(groupId).Mail;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("does not exist or one of its queried reference-property objects are not present") || i == retry) throw;
                    Thread.Sleep(delayMs);
                    delayMs *= 2;
                    logger.Warn($"Group address is not available now, retrying... Attempt: {i}, GroupId: {groupId}");
                }
            }
            return string.Empty;
        }

        public string GetGroupSiteURLByGroupId(string groupId)
        {
            return msGraphAPIService.GetGroupRootSite(groupId)?.WebUrl;
        }

        public void UpdateGroupInfo(String groupId, Office365GroupEntityV2 office365GroupEntity)
        {
            logger.Info("Start updating group infomation.");
            var groupToUpdate = new Microsoft.Graph.Models.Group()
            {
                DisplayName = office365GroupEntity.DisplayName,
                Description = office365GroupEntity.Description,
                Visibility = office365GroupEntity.AccessType.ToString(),
                PreferredDataLocation = office365GroupEntity.PreferredDataLocation
            };
            msGraphAPIService.UpdateGroup(groupId, groupToUpdate);
        }
        public void UpdateGroupSettings(String groupId, Office365GroupEntityV2 office365GroupEntity)
        {//目前app only 不能更新这部分settings
            logger.Info("Start updating group settings.");
            try
            {
                if (null != office365GroupEntity.ExtraSettings)
                { //Graph 新数据 2020/8/7
                    var settingToUpdate1 = new GroupExtraInfo()
                    {
                        HideFromAddressLists = office365GroupEntity.ExtraSettings.HideFromAddressLists,
                        HideFromOutlookClients = office365GroupEntity.ExtraSettings.HideFromOutlookClients,
                        Id = groupId,
                    };
                    var settingToUpdate2 = new GroupExtraInfo()
                    {//不支持 app only Restore
                        AllowExternalSenders = office365GroupEntity.ExtraSettings.AllowExternalSenders,
                        AutoSubscribeNewMembers = office365GroupEntity.ExtraSettings.AutoSubscribeNewMembers,
                        Id = groupId,
                    };
                    msGraphAPIService.UpdateGroupExtraSettings(settingToUpdate1);
                    msGraphAPIService.UpdateGroupExtraSettings(settingToUpdate2);

                }
                else if (null != office365GroupEntity.MailboxSettings)
                {//v2旧数据
                    var settingToUpdate = new GroupExtraInfo()
                    {
                        AllowExternalSenders = office365GroupEntity.MailboxSettings.ExternalSendersEnabled,
                        AutoSubscribeNewMembers = office365GroupEntity.MailboxSettings.AutoSubscribeNewMembers,
                        Id = groupId,
                    };
                    msGraphAPIService.UpdateGroupExtraSettings(settingToUpdate);
                }
                else
                {
                    logger.Info("Group settings is null,so skiped.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Unable to set group settings. Reason : {0}", ex.Message);
            }
        }

        public void UpdateGroupMembershipType(String groupId, GroupAdditionalPropertiesV2 additionalProperties)
        {
            var groupTypes = additionalProperties.GroupTypes.ToList();
            var groupToUpdate = new Microsoft.Graph.Models.Group
            {
                GroupTypes = groupTypes,
            };
            if (groupTypes.Contains("DynamicMembership", StringComparer.OrdinalIgnoreCase))
            {
                groupToUpdate.MembershipRule = additionalProperties.MembershipRule;
                groupToUpdate.MembershipRuleProcessingState = additionalProperties.MembershipRuleProcessingState;
            }
            msGraphAPIService.UpdateGroup(groupId, groupToUpdate);
        }

        private List<ResponseItem> BatchAddO365GroupOwnerAndMembers(String groupId, List<GroupMemberV2> users, IEnumerable<ToExportUserInfo> specifyUserList, bool isMembershipDynamic = false)
        {
            logger.Info("Start adding group owners and members.");
            var batchRequestObj = msGraphAPIService.CreateBatchRequestObj(20);
            var addMemberUrl = $"groups/{groupId}/members/$ref";
            var addOwnerUrl = $"groups/{groupId}/owners/$ref";
            var headrs = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            users.ForEach(user =>
            {
                var upn = user.UserName;
                if (!isMembershipDynamic)
                {
                    var memberItem = new RequestItem()
                    {
                        Id = user.UserName,
                        Url = addMemberUrl,
                        Method = "POST",
                        Headers = headrs,
                        Body = this.msGraphAPIService.BuildDirectoryObject(upn.Replace("#", "%23")),
                    };
                    batchRequestObj.Add(memberItem);
                }
                if (user.IsOwner)
                {
                    var ownerItem = new RequestItem()
                    {
                        Id = $"{user.UserName}(Owner)",
                        Url = addOwnerUrl,
                        Method = "POST",
                        Headers = headrs,
                        Body = this.msGraphAPIService.BuildDirectoryObject(upn),
                    };
                    batchRequestObj.Add(ownerItem);
                }
            });
            var result = batchRequestObj.SentRequest();
            LogAddFailedUser(result);

            var totalOwners = users.Count(u => u.IsOwner);
            var failedOwners = result.Count(r => !r.IsSuccessStatusCode && r.Id.Contains("(Owner)"));
            // Need fallback owner if:
            // - no owner in original list
            // - all original owners failed
            var isNeedAddOtherOwner = totalOwners == 0 || totalOwners == failedOwners;
            if (isNeedAddOtherOwner && specifyUserList.IsNotNullOrEmpty())
            {
                logger.Info("No valid owner available from original list. Trying fallback owners from specify user list.");
                foreach (var user in specifyUserList)
                {
                    try
                    {
                        this.msGraphAPIService.AddGroupOwner(groupId, user.Id);

                        logger.Info("Successfully added owner with specify user list, userId: {0}.", user.Id);
                        // If successfully add owner with specify user list, remove the failed owner result to avoid unnecessary retry attempts
                        result.RemoveAll(r => r.Id.Contains("(Owner)"));
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Failed to add owner with specify user list, userId: {0}, reason: {1}", user.Id, ex);
                    }
                }
            }
            return result;
        }
        public void AddO365GroupOwnerAndMembers(String groupId, List<GroupMemberV2> users, IEnumerable<ToExportUserInfo> specifyUserList, bool isMembershipDynamic = false)
        {
            var result = BatchAddO365GroupOwnerAndMembers(groupId, users, specifyUserList, isMembershipDynamic);
            //[AOSBR-25099] The following logic is to be compatible with old data. It should be removed later.
            var failedUsename = result.Where(r => r.Status == System.Net.HttpStatusCode.NotFound && CheckSingleQuotesEscapes(r.Id)).Select(r => r.Id);
            var failedEscapesUser = users.Where(u => failedUsename.Contains(u.UserName)).ToList();
            if (failedEscapesUser.Any())
            {
                foreach (var user in failedEscapesUser)
                {
                    user.UserName = user.UserName.Replace("''", "'");
                }
                BatchAddO365GroupOwnerAndMembers(groupId, failedEscapesUser, specifyUserList, isMembershipDynamic);
            }
        }
        private static bool CheckSingleQuotesEscapes(string usernnem)
        {
            if (!usernnem.Contains("''")) return false;
            return !usernnem.Replace("''", string.Empty).Contains("'");
        }
        private void LogAddFailedUser(List<ResponseItem> addUserResult)
        {
            var errorItemFomart = "\"{0}\":\"{1}\",";
            var fsb = new StringBuilder("Add_Failed : {");
            foreach (var resultItem in addUserResult)
            {
                if (!resultItem.IsSuccessStatusCode) fsb.AppendFormat(errorItemFomart, resultItem.Id, resultItem.Body);
            }
            fsb.Append("}");
            logger.Warn(fsb.ToString());
        }
        #endregion


        public String GetGroupUser(string o365GroupName)
        {
            logger.Info("Get office 365 group first user.[{0}]", o365GroupName);
            try
            {
                var isSuccess = TryGetO365GroupId(o365GroupName, out String groupId);
                logger.Warn("Try get office 365 group first owner : {0}. ", isSuccess);
                if (!isSuccess) return null;
                var groupOwner = msGraphAPIService.GetLicensedUser(groupId, GraphMethodExtension.FindRole.owner, thrownException: false);
                if (groupOwner != null)
                {
                    return groupOwner.UserPrincipalName;
                }
                var groupMember = msGraphAPIService.GetLicensedUser(groupId, GraphMethodExtension.FindRole.member, thrownException: false);
                if (groupMember != null)
                {
                    return groupMember.UserPrincipalName;
                }
                logger.Warn("The office 365 group maybe have none owners and members.");
                return null;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when to get office 365 group first user. Reason: [{0}]", ex.ToString());
                return null;
            }

        }

        public Boolean TryGetGroupCurtInfo(String o365GroupName, out TeamInfo info)
        {
            var group = msGraphAPIService.GetGroupInfoByAddress(o365GroupName);
            if (group == null)
            {
                logger.Warn("Get group by address failed, try to get group by mail nickname. GroupName: {0}", o365GroupName);
                group = msGraphAPIService.GetGroupByMailNickName(o365GroupName);
            }
            info = new TeamInfo() { GroupId = group?.Id, Mail = group?.Mail };
            return null != group;
        }

        public Boolean TryGetO365GroupId(String o365GroupName, out String groupId)
        {
            try
            {
                groupId = msGraphAPIService.GetGroupIdByAddress(o365GroupName);
                return !String.IsNullOrEmpty(groupId);
            }
            catch (Exception ex)
            {
                groupId = String.Empty;
                logger.Error("An error occurred when to get group ID.{0}", ex);
                return false;
            }
        }
        public Boolean ISGroupIdExist(String groupId)
        {
            try
            {
                msGraphAPIService.GetGroupInfoById(groupId);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when to check group Id exist.{0}", ex);
                return false;
            }
        }

        public void RemoveGroup(String groupId)
        {
            msGraphAPIService.DeleteGroup(groupId);
            logger.Info("Remove group success.");
        }

        public void RemoveDirectoryGroup(String groupId)
        {
            msGraphAPIService.RemoveDirectoryGroup(groupId);
            logger.Info("Remove group from directory success.");
        }

        public Boolean IsO365GroupExist(String o365GroupName)
        {
            try
            {
                var groupId = msGraphAPIService.GetGroupIdByAddress(o365GroupName);
                if (string.IsNullOrEmpty(groupId))
                {
                    logger.Warn("Get group by address failed, try to get group by mail nickname. GroupName: {0}", o365GroupName);
                    groupId = msGraphAPIService.GetGroupByMailNickName(o365GroupName)?.Id;
                }
                return !String.IsNullOrEmpty(groupId);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when to check group exist.{0}", ex);
                return false;
            }
        }

        public bool IsUserExist(string upnOrId)
        {
            try
            {
                return msGraphAPIService.GetUser(upnOrId) != null;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when to check user exist, {0}.", ex);
                return false;
            }
        }

        public string GetGroupSiteUrl(string id)
        {
            try
            {
                return msGraphAPIService.GetGroupSiteByGroupId(id).WebUrl;
            }
            catch(Exception ex)
            {
                logger.Error("An error occurred when to get group site url, {0}.", ex);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
    public class MembersDistinct : IEqualityComparer<GroupMemberV2>
    {
        public bool Equals(GroupMemberV2 T1, GroupMemberV2 T2)
        {
            return T1.UserName.Equals(T2.UserName, StringComparison.OrdinalIgnoreCase);
        }
        public int GetHashCode(GroupMemberV2 user)
        {
            return user.UserName.GetHashCode();
        }
    }
    public sealed class Microsoft365GroupServiceWithGraph : Microsoft365GroupServiceBase
    {
        public Microsoft365GroupServiceWithGraph(IAppTokenAuthObject authObj) : base(authObj)
        {
            msGraphAPIService = new MicrosoftGraphAPIService(authObj.ResourceUrl, authObj.GetAccessToken, new GraphLogger());
            msGraphAPIService.RetryController = new GraphAPIRetry();
            logger.Info("Create Graph API Service Success: {0}", msGraphAPIService != null);
        }
    }

    public static class Office365GroupConverter
    {
        public static GroupMemberV2 ToGroupMember(this GraphUser user, Boolean isOwner, Boolean isMember)
        {
            return new GroupMemberV2()
            {
                OdataType = user.OdataType,
                Id = user.Id,
                UserName = user?.UserPrincipalName ?? string.Empty,
                IsOwner = isOwner,
                IsMember = isMember,
                Name = user?.DisplayName ?? string.Empty,
            };
        }
        public static GroupMemberV2 ToGroupMember(this Microsoft.Graph.Models.User user, Boolean isOwner)
        {
            return new GroupMemberV2()
            {
                OdataType = user.OdataType,
                Id = user.Id,
                UserName = user?.UserPrincipalName ?? string.Empty,
                IsOwner = isOwner,
            };
        }

        private static GroupAccessTypeV2 ToGroupAccessTypev2(string visibility)
        {
            switch (visibility.ToUpper())
            {
                case "HIDDENMEMBERSHIP":
                    return GroupAccessTypeV2.HiddenMembership;
                case "PUBLIC":
                    return GroupAccessTypeV2.Public;
                case "PRIVATE":
                    return GroupAccessTypeV2.Private;
                default:
                    throw new Exception($"Unknown visibility type: [{visibility}]");
            }
        }

        public static Office365GroupEntityV2 ToM(this Group groupInfo)
        {
            return new Office365GroupEntityV2()
            {
                Description = groupInfo.Description,
                DisplayName = groupInfo.DisplayName,
                ExternalDirectoryObjectId = groupInfo.Id,
                SmtpAddress = groupInfo.Mail,
                AccessType = Office365GroupConverter.ToGroupAccessTypev2(groupInfo.Visibility),
                Classification = groupInfo.Classification?.ToString(),
                IsTeamsGroup = groupInfo.CreationOptions?.Contains("Team", StringComparer.OrdinalIgnoreCase) ?? false,
                IsVivaGroup = groupInfo.CreationOptions?.Contains("YammerProvisioning", StringComparer.OrdinalIgnoreCase) ?? false,
                MailboxGuid = String.Empty,
                UnifiedGroupSKU = new UnifiedGroupSKUV2()
                {
                    GroupType = groupInfo.GroupTypes.FirstOrDefault() ?? string.Empty, // it may get the DynamicMembership group type if has instead of unified
                    IsNull = !groupInfo.GroupTypes.Any(),
                },
                AdditionalProperties = new GroupAdditionalPropertiesV2()
                {
                    ExternalMemberCount = 0,
                    IsGroupMembershipHidden = groupInfo.Visibility.Equals("hiddenmembership", StringComparison.OrdinalIgnoreCase),
                    //IsMembershipDynamic = groupInfo.GroupTypes.Contains("DynamicMembership", StringComparer.OrdinalIgnoreCase),
                    GroupTypes = groupInfo.GroupTypes ?? [],
                    MembershipRule = groupInfo.MembershipRule,
                    MembershipRuleProcessingState = groupInfo.MembershipRuleProcessingState,
                    SubscriptionEnabled = false,
                },
                PreferredDataLocation = groupInfo.PreferredDataLocation,
                CreatedDateTime = groupInfo.CreatedDateTime,
            };
        }
        public static ExtraSettings ToM(this GroupExtraInfo extraInfo)
        {
            return new ExtraSettings
            {
                AllowExternalSenders = extraInfo.AllowExternalSenders ?? false,
                AutoSubscribeNewMembers = extraInfo.AutoSubscribeNewMembers ?? false,
                HideFromAddressLists = extraInfo.HideFromAddressLists ?? false,
                HideFromOutlookClients = extraInfo.HideFromOutlookClients ?? false,
            };
        }
    }
}