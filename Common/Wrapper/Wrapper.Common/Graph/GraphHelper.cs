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
using AvePoint.GCommon;
using AvePoint.GCommon.GraphAPI;
using AvePoint.Wrapper.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft365.Authentication;
using Microsoft365.Authentication.ServiceEndPoint;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.Wrapper.Common.Graph
{
    public static class GraphHelper
    {
        private static IAveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetGroupSiteUrlByEmail(string email, AveBPOSAccountInfo accountInfo)
        {
            log.Info($"Begin to get group site by email:{email}");
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            string resourceUrl = MicrosoftOnlineInstanceExtension.GetMsoInstance(accountInfo.AADEnvironment).AdalMsGraphServiceResource;
            var api = new MicrosoftGraphAPIService(resourceUrl, provider.GetToken, new GraphLogger());
            var group = api.GetGroupInfoByAddress(email);
            if (group == null||string.IsNullOrEmpty(group.Id))
            {
                log.Warn($"Group was not found by email:{email}");
                return null;
            }
            
            var drive = api.GetGroupSiteByGroupId(group.Id);
            if (drive == null||string.IsNullOrEmpty(drive.WebUrl))
            {
                log.Warn($"Group site was not found by group id.Email:{email},GroupId:{group.Id}");
                return null;
            }
            return drive.WebUrl;
        }

        public static string EnsureGroupSiteUrl(string groupEmail,string originalGroupSiteUrl, AveBPOSAccountInfo account)
        {
            if (string.IsNullOrEmpty(groupEmail))
            {
                log.Warn("Group email is null or empty.");
                return originalGroupSiteUrl;
            }
            var groupSiteUrl = GetGroupSiteUrlByEmail(groupEmail, account);
            if(string.IsNullOrEmpty(groupSiteUrl))
            {
                log.Warn($"The found group site url is null or empty.Email:{groupEmail}");
                return originalGroupSiteUrl;
            }
            log.Warn($"Update Group Site Url,Original Url:{originalGroupSiteUrl},new Url:{groupSiteUrl}");
            return groupSiteUrl;
        }

        public static string GetDriveWebUrl(string userPrincipalName, AveBPOSAccountInfo accountInfo)
        {
            log.Info($"Begin to get drive of user: {userPrincipalName}");
            var provider = GraphTokenProviderFactory.CreateDriveGraphProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var drive = api.GetUserDrive(userPrincipalName);
            log.Info($"WebUrl of {userPrincipalName}: {drive?.WebUrl}");
            return drive?.WebUrl;
        }

        public static string GetCurrentUserPreferedLanguage(AveBPOSAccountInfo accountInfo)
        {
            if ((accountInfo.ConnectionType == BposConnectionType.ServiceAccount || accountInfo.ConnectionType == BposConnectionType.Both)&&(!string.IsNullOrEmpty(accountInfo?.UserName)))
            {
                log.Info($"Begin to get user PreferredLanguage: {accountInfo?.UserName}");
                var provider = GraphTokenProviderFactory.CreateDriveGraphProvider(accountInfo);
                var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
                var user = api.GetUser(accountInfo.UserName);
                if (user == null)
                {
                    log.Info($"User {accountInfo?.UserName} was not found when trying to get PreferredLanguage");
                    return null;
                }
                log.Info($"User {accountInfo?.UserName} PreferredLanguage:{user?.PreferredLanguage}");
                return user.PreferredLanguage;
            }
            log.Info($"Get user PreferredLanguage for {accountInfo?.UserName} is skipped since user not valid or BposConnectionType {accountInfo?.ConnectionType} not support it.");
            return default(string);
        }

        public static RecordingDrive GetUserRecordingFolderUrl(string userEmail, AveBPOSAccountInfo account)
        {
            log.Info($"User email: {userEmail}");
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new System.ArgumentNullException(nameof(userEmail));
            }

            var provider = GraphTokenProviderFactory.CreateDriveGraphProvider(account);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger())
            {
                RetryController = new GraphAPIRetry(),
            };
            var recordingDrive = new RecordingDrive();
            try
            {
                var drive = api.GetUserRecordingDrive(userEmail);
                log.Info($"User recording folder url: {drive?.WebUrl}");
                if (drive != null && !string.IsNullOrEmpty(drive.WebUrl))
                {
                    recordingDrive.Urls.Add(System.Web.HttpUtility.UrlDecode(drive.WebUrl));
                }
            }
            catch (Exception e)
            {
                log.Warn($"Could not retrieve recording dirve, error: {e}");
                //AveWrapperI18NException ex = new AveWrapperRecordingWarnException(e);
                //if ((e is GraphAPIException graphAPIException) && string.Equals(graphAPIException.Error?.Code, "accessDenied", StringComparison.OrdinalIgnoreCase))
                //{
                //    if (account.ConnectionType == BposConnectionType.ServiceAccount)
                //    {
                //        log.Info("The recording folder does not exist in this onedrive.");
                //        ex = null;
                //    }
                //    else if (account.AppType == GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp)
                //    {
                //        //need add the internationalization later
                //        ex = new AveWrapperRecordingFailedException(
                //                WrapperReportResourceKey.Wrapper_RecordingDriveAccessDenied.ToString(),
                //                "Failed to exclude the Recordings folder.", account.ClientId);
                //    }
                //}
                //if(ex != null)
                //{
                //    recordingDrive.AddReport(userEmail, ex);
                //}
            }
            return recordingDrive;
        }

        public static IEnumerable<Channel> ListChannels(string groupId, AveBPOSAccountInfo account)
        {
            return account.ToGraphService().ListChannels(groupId);
        }

        public static IEnumerable<Tab> ListChannelTabs(string groupId, string channelId, AveBPOSAccountInfo account)
        {
            return account.ToGraphService().ListChannelTabs(groupId, channelId);
        }

        public static void UpdateChannelTab(string groupId, string channelId,TabUpdateObj tab, AveBPOSAccountInfo account)
        {
            account.ToGraphService().UpdateChannelTab(groupId, channelId, tab);
        }

        public static string GetPrivateChannelSiteUrl(string groupEmail, AveChannelInfo channeInfo, string channelSiteTitle, AveBPOSAccountInfo account)
        {
            var api = account.ToGraphService();
            var group = api.GetGroupInfoByAddress(groupEmail);
            if (group == null)
            {
                throw new System.IO.FileNotFoundException($"Team not found. Email Address: {groupEmail}");
            }

            if (channeInfo == null)
            {
                log.Info($"Group display name: {group.DisplayName}");
                var prefix = $"{group.DisplayName} - ";
                var index = channelSiteTitle.IndexOf(prefix);
                if (index == 0)
                {
                    var channelName = channelSiteTitle.Substring(prefix.Length);

                    channeInfo = new AveChannelInfo()
                    {
                        DisplayName = channelName,
                    };
                }
            }
            if (channeInfo != null)
            {
                var privateChannels = api.ListChannels(group.Id).Where(c => c.MembershipType.EqualsIgnoreCase("private"));
                log.Info($"Channels: {string.Join(",", privateChannels.Select(c => c.DisplayName))}");

                Channel channel = null;
                if (!string.IsNullOrEmpty(channeInfo.Id))
                {
                    channel = privateChannels.FirstOrDefault(c => c.Id.EqualsIgnoreCase(channeInfo.Id));
                }
                if (channel == null)
                {
                    channel = privateChannels.FirstOrDefault(c => c.DisplayName.EqualsIgnoreCase(channeInfo.DisplayName));
                }
                if (channel == null || string.IsNullOrEmpty(channel.Id))
                {
                    throw new System.IO.FileNotFoundException($"Private channel not found. ID: {channeInfo.Id}, Name: {channeInfo.DisplayName}");
                }

                var fileFolder = api.GetChannelFilesFolder(group.Id, channel.Id);
                if (fileFolder == null || string.IsNullOrEmpty(fileFolder.WebUrl))
                {
                    throw new System.IO.FileNotFoundException($"Private channel site collection not found. Private channel id: {channel.Id}");
                }

                var folderUrl = fileFolder.WebUrl.TrimEnd('/');
                log.Info($"Channel files folder url: {folderUrl}");

                var libUrl = folderUrl.Substring(0, folderUrl.LastIndexOf('/'));
                var siteCollectionUrl = libUrl.Substring(0, libUrl.LastIndexOf('/'));
                return System.Web.HttpUtility.UrlDecode(siteCollectionUrl);
            }
            return null;
        }

        //need implement exception internalization when Records need call this function.
        public static RecordingDrive GetRecordingDrive(string groupId, AveBPOSAccountInfo account, MembershipType membershipType)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            var provider = GraphTokenProviderFactory.CreateDriveGraphProvider(account);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger())
            {
                RetryController = new GraphAPIRetry(),
            };
            var recordingDrive = new RecordingDrive();
            try
            {
                var group = api.GetGroupById(groupId);
                if (group == null || group.ResourceProvisioningOptions == null || !group.ResourceProvisioningOptions.Any(ps=>ps.EqualsIgnoreCase("Team")))
                {
                    return recordingDrive;
                }
            }
            catch (GraphAPIException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return recordingDrive;
                }
                throw;
            }
            var channels = api.ListChannels(groupId);
            if (channels != null && channels.Any())
            {
                foreach (var channel in channels)
                {
                    var fileFolderUrl = string.Empty;
                    try
                    {
                        if (string.Equals(channel.MembershipType, "private", StringComparison.OrdinalIgnoreCase))
                        {
                            if (membershipType != MembershipType.Standard)
                            {
                                var fileFolder = api.GetChannelFilesFolder(groupId, channel.Id);
                                if (fileFolder == null)
                                {
                                    continue;
                                }
                                fileFolderUrl = System.Web.HttpUtility.UrlDecode(fileFolder.WebUrl);
                                var drive = api.GetRecordingDrive(groupId, fileFolder.ParentReference.DriveId);
                                if (drive != null && !string.IsNullOrEmpty(drive.WebUrl))
                                {
                                    recordingDrive.Urls.Add(System.Web.HttpUtility.UrlDecode(drive.WebUrl));
                                } 
                            }
                        }
                        else if(membershipType != MembershipType.Private)
                        {
                            var fileFolder = api.GetChannelFilesFolder(groupId, channel.Id);
                            if (fileFolder == null)
                            {
                                continue;
                            }
                            fileFolderUrl = System.Web.HttpUtility.UrlDecode(fileFolder.WebUrl);
                            if (!string.IsNullOrEmpty(fileFolderUrl))
                            {
                                recordingDrive.Urls.Add($"{fileFolderUrl.TrimEnd('/')}/{AveConstants.FOLDER_NAME_RECORDINGS}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error($"Could not retrieve recording drive for channel {channel.DisplayName}, id: {channel.Id}, error: {e}");

                        //if (!string.IsNullOrEmpty(fileFolderUrl))
                        //{
                        //    AveWrapperI18NException recordingDriveException = new AveWrapperRecordingWarnException(e);
                        //    if (e is GraphAPIException graphEx)
                        //    {
                        //        if (graphEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                        //        {
                        //            recordingDriveException = null;
                        //        }
                        //        else if (string.Equals(graphEx.Error?.Code, "accessDenied", StringComparison.OrdinalIgnoreCase) 
                        //            && account.AppType == GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp)
                        //        {
                        //            //need add the internationalization later
                        //            recordingDriveException = new AveWrapperRecordingFailedException(
                        //                WrapperReportResourceKey.Wrapper_RecordingDriveAccessDenied_Team.ToString(),
                        //                "Failed to exclude the Recordings folder.", account.ClientId);
                        //        }
                        //    }
                        //    if (recordingDriveException != null)
                        //    {
                        //        recordingDrive.AddReport(fileFolderUrl, recordingDriveException);
                        //    }
                        //}
                    }
                }
            }
            return recordingDrive;
        }

        public static IList<string> GetChannelsDriveUrl(string groupId, AveBPOSAccountInfo account, MembershipType channelType)
        {
            log.Info($"Group Id: {groupId}, Channel Type: {channelType}");
            if (string.IsNullOrEmpty(groupId))
            {
                throw new System.ArgumentNullException(nameof(groupId));
            }

            var api = account.ToGraphService();
            var channels = api.ListChannels(groupId);
            var driveUrls = new List<string>();
            if (channels != null && channels.Any())
            {
                foreach (var channel in channels.Where(t=> 
                    {
                        switch (channelType)
                        {
                            case MembershipType.All:
                                return true;
                            case MembershipType.Standard:
                                return string.Equals(t.MembershipType, "standard", StringComparison.OrdinalIgnoreCase);
                            case MembershipType.Private:
                                return string.Equals(t.MembershipType, "private", StringComparison.OrdinalIgnoreCase);
                            default:
                                throw new ArgumentException($"Unsupport channel type: {channelType}");
                        }
                    }))
                {
                    var drive = api.GetChannelFilesFolder(groupId, channel.Id);
                    if (drive != null && !string.IsNullOrEmpty(drive.WebUrl))
                    {
                        driveUrls.Add(System.Web.HttpUtility.UrlDecode(drive.WebUrl));
                    }
                }
            }
            return driveUrls;
        }

        public static AveGroupTeamInfo ListChannelsWithFileFolderUrl(AveGroupTeamInfo groupTeamInfo, AveBPOSAccountInfo account, MembershipType channelType)
        {
            if (groupTeamInfo == null)
            {
                throw new System.ArgumentNullException(nameof(groupTeamInfo));
            }

            if (groupTeamInfo.IsConnectedToTeams)
            {
                log.Info($"Start to load channels for team {groupTeamInfo.EmailAddress}({groupTeamInfo.Id})");
                var api = account.ToGraphService();
                groupTeamInfo.Channels = new List<AveChannelInfo>();
                var channels = api.ListChannels(groupTeamInfo.Id);
                if (channels != null && channels.Any())
                {
                    foreach (var channel in channels)
                    {
                        if (channelType == MembershipType.All || channel.MembershipType.EqualsIgnoreCase(channelType.ToString()))
                        {
                            try
                            {
                                AveChannelInfo channelInfo = new AveChannelInfo
                                {
                                    Id = channel.Id,
                                    DisplayName = channel.DisplayName,
                                    IsPrivateChannel = channel.MembershipType.EqualsIgnoreCase("private")
                                };
                                var drive = api.GetChannelFilesFolder(groupTeamInfo.Id, channel.Id);
                                if (drive != null && !string.IsNullOrEmpty(drive.WebUrl))
                                {
                                    var folderUrl = drive.WebUrl.TrimEnd('/');
                                    channelInfo.FileFolderUrl = folderUrl;

                                    var libUrl = folderUrl.Substring(0, folderUrl.LastIndexOf('/'));
                                    channelInfo.RelatedSiteUrl = libUrl.Substring(0, libUrl.LastIndexOf('/'));
                                }
                                groupTeamInfo.Channels.Add(channelInfo);
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Could not retrieve recording drive for channel {channel.DisplayName}, id: {channel.Id}, error: {ex}");
                            }
                        }
                    }
                }
                return groupTeamInfo;
            }
            else
            {
                log.Info($"Group {groupTeamInfo.EmailAddress}({groupTeamInfo.Id}) is not connected to Microsoft Teams, no need to list channel info.");
                return null;
            }
        }

        public static MicrosoftGraphAPIService ToGraphService(this AveBPOSAccountInfo account)
        {
            return ToGraphService(account, false);
        }
        public static MicrosoftGraphAPIService ToGraphService(this AveBPOSAccountInfo account, bool preferServiceAccount)
        {
            var provider = GraphTokenProviderFactory.CreateProvider(account);
            return new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
        }

        /// <summary>
        /// Graph API example:
        /// https://graph.microsoft.com/v1.0/sites/3a2da927-3712-4eca-b646-adcbd861d96d/lists/74abae58-a8c3-43ba-b2fd-cf21d5e16ea6/items/5/driveitem/content
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static System.IO.Stream DownloadLargeFile(string siteId, string listId, string itemId, AveBPOSAccountInfo account)
        {
            var service = account.ToGraphService();
            service.RetryController = new GraphAPIRetry();
            var tempFilePath =System.IO.Path.Combine(WrapperConfiguration.TempDirectory,$"{listId}-{itemId}");
            var result = service.GetFileContent(siteId, listId, itemId, tempFilePath);
            return result;
        }

        /// <summary>
        /// Graph API example:
        /// https://graph.microsoft.com/v1.0/sites/3a2da927-3712-4eca-b646-adcbd861d96d/lists/74abae58-a8c3-43ba-b2fd-cf21d5e16ea6/items/5/driveitem/versions/1.0/content
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="versionId"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static System.IO.Stream DownloadLargeFileVersion(string siteId, string listId, string itemId, string versionId, AveBPOSAccountInfo account)
        {
            var service = account.ToGraphService();
            service.RetryController = new GraphAPIRetry();
            var tempFilePath =System.IO.Path.Combine(WrapperConfiguration.TempDirectory, $"{listId}-{itemId}");
            var result = service.GetFileVersionContent(siteId, listId, itemId, versionId, tempFilePath);
            return result;
        }

        public static List<GraphPlannerPlan> ListGroupPlans(string groupId, AveBPOSAccountInfo account)
        {
            return account.ToGraphService(true).ListAllPlansByGroupID(groupId);
        }

        public static string GetChannelFolderFullUrl(string groupEmail, string channelId, AveBPOSAccountInfo account)
        {
            var api = account.ToGraphService();
            var group = api.GetGroupInfoByAddress(groupEmail);
            if (group == null)
            {
                throw new System.IO.FileNotFoundException($"Team not found. Email Address: {groupEmail}");
            }

            var fileFolder = api.GetChannelFilesFolder(group.Id, channelId);
            if (fileFolder == null || string.IsNullOrEmpty(fileFolder.WebUrl))
            {
                throw new System.IO.FileNotFoundException($"Channel site collection not found. Channel id: {channelId}");
            }

            var folderUrl = fileFolder.WebUrl.TrimEnd('/');
            log.Info($"Channel files folder url: {folderUrl}");
            return folderUrl;
        }

        #region Graph User/Group
        public static string GetUserDisplayName(string upnOrId, AveBPOSAccountInfo account)
        {
            try
            {
                log.Info($"Begin to get user display name by email:{upnOrId}");
                var api = account.ToGraphService();
                string[] selectProperties = { "id", "displayName" };
                var user = api.GetUser(upnOrId, selectProperties);
                return user?.DisplayName;
            }
            catch (Exception ex)
            {
                log.Warn($"An error occurred while get user display name, email: {upnOrId}, ex: {ex}");
                return string.Empty;
            }
        }

        public static IList<GraphUser> GetGroupMemberById(string id, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var users = api.ListGroupMembers(id);
            return users;
        }
        public static IList<GraphUser> GetGroupOwnersById(string id, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var users = api.ListGroupOwners(id);
            return users;
        }
        public static IList<GraphUser> GetGroupMembersByDisplayName(string displayName, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var users = api.ListGroupMembersByGroupDisplayName(displayName);
            return users;
        }

        public static string GetGroupIdByDisplayName(string displayName, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var id = api.GetGroupIdByDisplayName(displayName);
            return id;
        }
        public static string GetEmailStringByUPN(string UPNstring, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var id = api.GetEmailByUPNName(UPNstring);
            return id;
        }
        public static Group GetGroupByDisplayName(string displayName, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var group = api.GetGroupByDisplayName(displayName);
            return group;
        }

        public static IList<Domain> GetAllDomains(AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var domains = api.GetAllDomains();
            return domains;
        }

        public static GraphUser GetGraphUser(string mail, AveBPOSAccountInfo accountInfo, bool isIncludeDetail = false)
        {
            if (mail.StartsWith("i:0#.f|membership|"))
            {
                mail = mail.Substring("i:0#.f|membership|".Length);
            }
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var user = api.GetUser(mail, isIncludeDetail);
            return user;
        }

        public static GraphUser GetGraphByEmail(string mail, AveBPOSAccountInfo accountInfo)
        {
            if (mail.StartsWith("i:0#.f|membership|"))
            {
                mail = mail.Substring("i:0#.f|membership|".Length);
            }
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var user = api.FindUser(mail);
            return user;
        }

        public static GraphUser GetGroupUserBlock(string mail, AveBPOSAccountInfo accountInfo)
        {
            if (mail.StartsWith("i:0#.f|membership|"))
            {
                mail = mail.Substring("i:0#.f|membership|".Length);
            }
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var user = api.GetUserBlock(mail);
            return user;
        }

        public static Group GetGroup(string groupId, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var user = api.GetGroup(groupId);
            return user;
        }

        public static IList<Group> ListGroup(AveBPOSAccountInfo accountInfo, bool isIncludeDetail = false)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var groups = api.ListGroup(isIncludeDetail);
            return groups;
        }

        public static IList<GraphUser> ListUser(AveBPOSAccountInfo accountInfo, bool isIncludeDetail = false)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var users = api.ListUser(isIncludeDetail);
            return users;
        }

        #endregion
    }

}