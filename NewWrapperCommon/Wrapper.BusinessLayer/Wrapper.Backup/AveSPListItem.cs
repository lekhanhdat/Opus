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



using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Common.Office;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPListItem : AvePoint.Wrapper.Backup.IAveSPListItem, ISPItemExport
    {
        private Guid mId;
        private string mName;
        private AveSPFolder mParentFolder;
        private IAveBackupStream mSender;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPItem mAveSPItem;
        private AveSPList mAveList;

        private int mVersion;
        private DateTime mBiggestVersionModified;

        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl = null)
            : this(aveFolder, name, id, rowId, version, serverRelativeUrl, DateTime.MinValue)
        {
        }

        public AveSPListItem(ISPListExport backupList, IAveListItem listItem)
            : this(new AveSPFolder((AveSPList)backupList, listItem.ParentList.RootFolder), listItem.Name, listItem.UniqueId, listItem.ID, listItem.Versions[listItem.Versions.Count - 1].VersionId)
        {
        }

        // add by adrian for 07 item backup 07item 备份userdata时，需要 serverRelativeUrl
        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl, DateTime currentVersionModified)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.Constructor"))
            {
                mParentFolder = aveFolder;
                mSender = aveFolder.Sender;
                mQueryService = aveFolder.QueryService;
                mAveList = aveFolder.AveList;
                mVersion = version;
                mId = id;
                mName = name;
                mBiggestVersionModified = currentVersionModified;
                Init(rowId, serverRelativeUrl);
            }
        }

        // add by adrian for 07 item backup 07item 备份userdata时，需要 serverRelative
        private void Init(int rowId, string serverRelativeUrl)
        {
            mAveSPItem = new AveSPItem(mId, rowId, mVersion, serverRelativeUrl, AveItemType.ListItem, mParentFolder.Id,
                mAveList.ParentWeb.ParentSite.SPSite.ID, mAveList,
                mSender, mQueryService, mAveList.Fields, mAveList.SolutionStatus, mParentFolder.SPFolder);
            //mAveSPItem.ParentId = mParentFolder.Id;
        }

        public string ExportDocInfo()
        {
            string xml = string.Empty;
            Dictionary<string, object> docInfo = mAveSPItem.GetListItemInfo();
            if (docInfo != null)
            {
                xml = AveConvert.ConvertAveObjToAveXml(AveMetadataType.DocProperty.ToString(), docInfo);
            }
            return xml;
        }

        public AveSPItem AveSPItem
        {
            get
            {
                return mAveSPItem;
            }
        }

        public AveSPSite AveSPSite
        {
            get
            {
                return mParentFolder.AveList.ParentWeb.ParentSite;
            }
        }

        public AveSPWeb AveSPWeb
        {
            get
            {
                return mParentFolder.AveList.ParentWeb;
            }
        }

        public Dictionary<string, string> GetMetaInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.GetMetaInfo"))
            {
                return mAveSPItem.GetMetaInfo();
            }
        }

        public string Url
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.TagUrl"))
                {
                    //  s...d/a.aspx
                    string fileUrl = string.Empty;
                    string webUrl = AveSPWeb.SPWeb.Url;
                    string webRelativeUrl = AveSPWeb.SPWeb.ServerRelativeUrl;
                    if (!string.IsNullOrEmpty(mAveList.SPList.DefaultDisplayFormUrl))
                    {
                        if (mAveList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
                        {
                            fileUrl = mAveList.SPList.DefaultDisplayFormUrl + "?ID=" + mAveSPItem.RowId;
                        }
                        else
                        {
                            fileUrl = webUrl.TrimEnd('/') + "/" + mAveList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(webRelativeUrl.TrimStart('/').Length).TrimStart('/') + "?ID=" + mAveSPItem.RowId;
                        }
                    }
                    else if (mAveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.Meetings && mAveSPItem.Item.ListItem.ID != 0)
                    {
                        //qlluo: meeting series下只有一个aspx页(movetodt.aspx)可以显示, 因此去掉查询Hidden File逻辑, 直接Hard Code。
                        //if (this.mParentFolder.SPFolder.HiddenFiles != null)
                        //{
                        //    if (this.mParentFolder.SPFolder.HiddenFiles.Count > 1 && webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase) > 0)
                        //    {
                        //        fileUrl = webUrl.Substring(0, webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase)) + this.mParentFolder.ServerRelativeUrl + "/" + this.mParentFolder.SPFolder.HiddenFiles[0].Name + "?ID=" + mAveSPItem.Item.ListItem.ID;
                        //    }
                        //}
                        //http://oliversp2013/Meeting1/Lists/Meeting%20Series/movetodt.aspx?id=1
                        if (webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            fileUrl = string.Format("{0}{1}/{2}?ID={3}",
                                webUrl.Substring(0, webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase)),     // "http://oliversp2013"
                                this.mParentFolder.ServerRelativeUrl,                                                            // "/meeting1/Lists/Meeting Series"
                                "MoveToDT.aspx".ToLowerInvariant(),
                                this.SPListItem.ID);
                        }
                    }
                    else if ((int)mAveSPItem.AveSPList.SPList.BaseTemplate == 550 && this.SPListItem.ID != 0)
                    {
                        //mysite下social list只有此url可以显示
                        //http://sp13workflow15:21367/personal/domainuser001/Social/FollowedContent.aspx?id=3
                        fileUrl = string.Format("{0}{1}?ID={2}",
                            webUrl.TrimEnd('/'),
                            "/Social/FollowedContent.aspx",
                            this.SPListItem.ID);
                    }
                    return fileUrl;
                }
            }
        }

        #region IAveSPListItem Members

        IAveSPItem IAveSPListItem.AveSPItem
        {
            get { return mAveSPItem; }
        }

        IAveSPSite IAveSPListItem.AveSPSite
        {
            get { return mParentFolder.AveList.ParentWeb.ParentSite; }
        }

        IAveSPWeb IAveSPListItem.AveSPWeb
        {
            get { return mParentFolder.AveList.ParentWeb; }
        }

        public IAveListItem SPListItem
        {
            get { return AveSPItem.SPListItem; }
        }

        public bool IsWorkflowTaskItem
        {
            get
            {
                if (mAveList.SPList != null && ((int)mAveList.SPList.BaseTemplate == 107 || (int)mAveList.SPList.BaseTemplate == 171))
                {
                    if (mQueryService != null)
                    {
                        return AveSPUtility.IsWorkflowTaskItem(mQueryService.GetItemContentTypeId(mAveList.SPList.ParentWeb.Site.ID, mParentFolder.Id, mAveSPItem.Id, mAveSPItem.Version));
                    }
                    else if (mAveSPItem.SPListItem != null)
                    {
                        return AveSPUtility.IsWorkflowTaskItem(mAveSPItem.SPListItem.ContentTypeId.ToString());
                    }
                }
                return false;
            }
        }

        public void ExportDocInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportDocInfo"))
            {
                var docInfo = GetDocInfo();
                if (docInfo != null)
                {
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
        }

        /// <summary>
        /// Get Doc Info
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDocInfo()
        {
            Dictionary<string, object> docInfo = mAveSPItem.GetListItemInfo();
            if (docInfo != null)
            {
                if (mBiggestVersionModified != DateTime.MinValue)
                {
                    docInfo["BiggestVersionModified"] = mBiggestVersionModified;
                }
            }
            return docInfo;
        }

        public void ExportAlerts(IAveBackupStream output, bool includeUsers = true, bool onlyUnAvaiableUser = false)
        {
            if (includeUsers)
            {
                this.AveSPItem.CacheUserFromAlert(this);
                if (onlyUnAvaiableUser)
                {
                    this.AveSPItem.ExportUnavailableUserInCache(output);
                }
                else
                {
                    this.AveSPItem.ExportUserCache(output);
                }
            }
            AveSPAlert alerts = AveSPAlert.CreateInstance(this);
            alerts.Export(output);
        }

        public void ExportSocialTags(IAveBackupStream output)
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var tag = new AveSPSocialTag(this.Url, this.AveSPSite);
                    tag.Export(output);
                }
            }
        }

        public void ExportSocialComments(IAveBackupStream output)
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var comment = new AveSPSocialComment(this.Url, this.AveSPSite);
                    comment.Export(output);
                }
            }
        }

        //no use
        private void ExportSingleSocialFeed(IAveBackupStream output)
        {
            if (this.AveSPSite.SPContextKind.IsServerMode13Upper() || this.AveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.AveSPWeb.SPWeb.Url, this.AveSPSite);
                    int feedID = Convert.ToInt32(SPListItem["ID"]);
                    object singleFeed = AveSPWeb.MicroFeedCache[feedID];
                    feed.ExportSingleFeed(output, singleFeed);
                }
            }
        }

        //no use
        private void ExportSocialThread(IAveBackupStream output)
        {
            if (mAveList.SocialThreadCache == null)
            {
                return;
            }
            if (this.AveSPSite.SPContextKind.IsServerMode13Upper() || this.AveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.AveSPWeb.SPWeb.Url, this.AveSPSite);
                    int feedID = Convert.ToInt32(SPListItem["ID"]);
                    object socialThread = new object();
                    bool IsRootPost = mAveList.SocialThreadCache.TryGetValue(feedID, out socialThread);
                    if (IsRootPost)
                    {
                        feed.ExportSingleFeed(output, socialThread);
                    }

                }
            }
        }

        public void ExportSingleSocialFeedForArchiver(IAveBackupStream output, object singleFeed)
        {
            if (this.AveSPSite.SPContextKind.IsServerMode13Upper()|| this.AveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.AveSPWeb.SPWeb.Url, this.AveSPSite);
                    feed.ExportSingleFeed(output, singleFeed);
                }
            }
        }

        public void ExportToExcel()
        {
            if (mParentFolder.AveList.NeedExportExcel && mParentFolder.AveList.SPList != null && !mParentFolder.AveList.SPList.Hidden)
            {
                mAveSPItem.ExportDataToExcel(this.mParentFolder.ServerRelativeUrl.Substring(this.mParentFolder.ServerRelativeUrl.IndexOf(this.AveSPWeb.ScopeString.ToString(), StringComparison.OrdinalIgnoreCase)));
            }
        }

        public List<AveAlertInfo> GetAlerts()
        {
            AveSPAlert alerts = AveSPAlert.CreateInstance(this);
            return alerts.GetAlertInfos();
        }

        public List<AveSocialTagInfo> GetSocialTags()
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var tag = new AveSPSocialTag(this.Url, this.AveSPSite);
                    return tag.GetSocialTags();
                }
            }
            return null;
        }

        public List<AveSocialCommentInfo> GetSocialComments()
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var comment = new AveSPSocialComment(this.Url, this.AveSPSite);
                    return comment.GetSocialComments();
                }
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Export Metadata for document
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="backupOption"></param>
        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            var metadata = new SPListItemMetadataDto();

            metadata.DocInfo_Old = GetDocInfo();
            var userData = mAveSPItem.GetUserDataInfoWithDependence(backupOption);
            metadata.UserDataInfo = userData.Item1;
            metadata.MetadataInfo = userData.Item2;

            metadata.DocDataJunction = mAveSPItem.GetUserDataJunctionCache(true);

            if (backupOption != null && backupOption.BackupItemTPGUIDofLookupValue)
            {
                metadata.ItemTPGUIDofLookupValue = mAveSPItem.GetLookupFieldGuidValue();
            }

            if (backupOption != null)
            {
                if (backupOption.IncludeUser)
                {
                    metadata.UserCache = mAveSPItem.GetUserCache(false);
                }
                if (backupOption.IncludeGroup)
                {
                    metadata.GroupCache = mAveSPItem.GetGroupCache();
                }
            }

            stream.WriteMetadata(AveMetadataType.ItemMetadataDto, metadata);
        }

        /// <summary>
        /// Export Role Assignments
        /// </summary>
        /// <param name="stream"></param>
        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            ExportRoleAssignments(stream, new SPRoleAssignmentsBakupOption()
            {
                IncludeUsers = true,
                IncludeGroups = true,
                IncludeInheritedRoleAssignments = false,
            });
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            if (backupOption == null)
            {
                throw new ArgumentNullException("backupOption");
            }
            mAveSPItem.ExportRoleAssignments(stream, backupOption.IncludeUsers, backupOption.IncludeGroups);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            var alert = AveSPAlert.CreateInstance(this);
            var alertsDto = alert.GetAlertsDto();

            if (alertsDto != null)
            {
                stream.WriteMetadata(AveMetadataType.AlertsDto, alertsDto);
            }
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            mAveSPItem.ExportSocialInfos(stream, Url);
        }


        //add for PRItemRestore to backup social info.
        public Dictionary<int, object> ConvertItemstoThreadsInfo(List<IAveListItem> items)
        {
            Dictionary<int, object> result = new Dictionary<int, object>();
            List<AveSocialFeedInfo> feedInfos = ConvertItemToPostInfo(items);
            foreach (AveSocialFeedInfo info in feedInfos)
            {
                int id = 0;
                if (int.TryParse(info.Id, out id))
                {
                    result.Add(id, info);
                }
            }
            return result;
        }

        //add for PRItemRestore to backup social info.
        private List<AveSocialFeedInfo> ConvertItemToPostInfo(List<IAveListItem> items)
        {
            List<AveSocialFeedInfo> feedInfos = new List<AveSocialFeedInfo>();
            List<AveSocialFeedPostInfoForPR> infoList = new List<AveSocialFeedPostInfoForPR>();
            foreach (IAveListItem item in items)
            {
                AveSocialFeedPostInfoForPR infoForPR = new AveSocialFeedPostInfoForPR();
                infoForPR.Info = new AveSocialFeedPostInfo();


                infoForPR.Info.Attributes = (AveOSocialPostAttributes)Enum.Parse(typeof(AveOSocialPostAttributes), item["Attributes"].ToString());
                //info.AuthorIndex = item.Author.ID;
                infoForPR.Info.CreatedTime = DateTime.Parse(item["Created"].ToString()).ToUniversalTime();
                infoForPR.Info.Id = item.ID.ToString();

                if (item["LikedBy"] != null && !string.IsNullOrEmpty(item["LikedBy"].ToString()))
                {
                    foreach (string name in Regex.Split(item["LikedBy"].ToString(), ";#"))
                    {
                        int temp = 0;
                        if (int.TryParse(name, out temp))
                        {
                            infoForPR.Info.Likers.Add(this.AveSPWeb.SPWeb.SiteUsers.GetByID(temp).NoPrefixLoginName);
                        }
                    }
                }


                infoForPR.Info.ModifiedTime = DateTime.Parse(item["Modified"].ToString()).ToUniversalTime();
                //info.Overlays
                if (item["ContentType"].ToString().Equals("Post", StringComparison.OrdinalIgnoreCase))
                {
                    infoForPR.Info.PostType = AveOSocialPostType.Root;
                }
                else if (item["ContentType"].ToString().Equals("Reply", StringComparison.OrdinalIgnoreCase))
                {
                    infoForPR.Info.PostType = AveOSocialPostType.Reply;
                }
                if (item["MediaLinkURI"] != null)
                {
                    infoForPR.Info.PreferredImageUri = new Uri(item["MediaLinkURI"].ToString());
                }

                if (item["PostSource"] != null && item["PostSourceUri"] != null)
                {
                    infoForPR.Info.Source = new AveSocialLink();
                    infoForPR.Info.Source.Text = item["PostSource"].ToString();
                    infoForPR.Info.Source.Uri = new Uri(item["Content"].ToString());
                }


                if (item["Content"] != null && item["ContentData"] != null)
                {
                    infoForPR.Info.Text = ChangeSocialFeedInfoContentForPR(item["Content"].ToString(), item["ContentData"].ToString());
                }
                else if (item["Content"] != null)
                {
                    infoForPR.Info.Text = item["Content"].ToString();
                }

                
                infoForPR.Info.AuthorIndex = 0;
                infoForPR.ReplyCount = int.Parse(item["ReplyCount"].ToString());
                infoForPR.RootPostId = item["RootPostID"].ToString();

                IAveUser user = this.AveSPWeb.SPWeb.SiteUsers.First(n => n.NoPrefixLoginName.Equals(item["PostAuthor"].ToString(), StringComparison.OrdinalIgnoreCase));
                infoForPR.ActorInfo = new AveSocialActorInfo();
                infoForPR.ActorInfo.ActorType = AveOSocialActorType.User;
                infoForPR.ActorInfo.AccountName = user.NoPrefixLoginName;
                infoForPR.ActorInfo.CanFollow = true;
                //infoForPR.ActorInfo.ContentUri = user.
                infoForPR.ActorInfo.EmailAddress = user.Email;
                infoForPR.ActorInfo.Id = user.ID.ToString();
                //infoForPR.ActorInfo.ImageUri = user.ima
                infoForPR.ActorInfo.Name = user.Name;
                //infoForPR.ActorInfo.Title = user.Name;
                infoForPR.ActorInfo.Status = AveOSocialStatusCode.OK;

                infoList.Add(infoForPR);
            }

            foreach (AveSocialFeedPostInfoForPR post in infoList)
            {
                AveSocialFeedInfo feedinfo = new AveSocialFeedInfo();
                if (post.RootPostId.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    feedinfo.Id = post.Info.Id;
                    feedinfo.RootPost = post.Info;
                    feedinfo.TotalReplyCount = post.ReplyCount;
                    feedinfo.OwnerIndex = post.Info.AuthorIndex;
                    feedinfo.Actors = new AveSocialActorInfo[] { post.ActorInfo };
                    feedInfos.Add(feedinfo);
                }
                else
                {
                    feedInfos.First(n => n.Id.Equals(post.RootPostId, StringComparison.OrdinalIgnoreCase)).Replies.Add(post.Info);
                }
            }
            return feedInfos;
        }

        //Change the @ and # to what the restore need.[only for PR]
        private string ChangeSocialFeedInfoContentForPR(string content, string contentData)
        {
            string result = content;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(contentData);
            XmlNamespaceManager xmlNSManager = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNSManager.AddNamespace("", "http://Microsoft/Office/Server/SPMicroFeedContentDataCollection");
            xmlNSManager.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");
            xmlNSManager.AddNamespace("d2p1", "http://Microsoft/Office/Server/MicroFeed");
            XmlNodeList nodes = xmlDoc.SelectNodes("//d2p1:CD/d2p1:h", xmlNSManager);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (result.Contains(node.InnerText))
                    {
                        switch (node.ParentNode.SelectSingleNode("d2p1:e", xmlNSManager).InnerText)
                        {
                            case "User":
                                result = result.Replace(node.InnerText, node.ParentNode.SelectSingleNode("d2p1:i", xmlNSManager).InnerText);
                                break;
                            case "Tag":
                                result = result.Replace(node.InnerText, node.ParentNode.SelectSingleNode("d2p1:b", xmlNSManager).InnerText);
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            
            return result;
        }


        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    class AveSocialFeedPostInfoForPR
    {
        public AveSocialFeedPostInfo Info;
        public int ReplyCount;
        public AveSocialActorInfo ActorInfo;
        public string RootPostId;
    }
}