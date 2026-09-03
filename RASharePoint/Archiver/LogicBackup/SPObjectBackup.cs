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
using System.Linq;
using System.Text;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon.Contract.CodeReview;
using System.Text.RegularExpressions;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAExportCommon;
using AvePoint.RA.SharePoint.Extension;
using Microsoft.SharePoint.Client;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/7/11",
    "ruiheng.liu@AvePoint.com",
    "Yanlong.Gu@AvePoint.com",
    new string[]
    {
        CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
        CodeReviewConstants.CHECK_LIST_ID_EH_1,
        CodeReviewConstants.CHECK_LIST_ID_EH_2,
        CodeReviewConstants.CHECK_LIST_ID_DB_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_10,
        CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
    },
    "ADO-36739",
    false
    )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2012/8/7",
      "ruiheng.liu@AvePoint.com",
      "yanlong.gu@AvePoint.com",
      new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
      "ADO-44684",
      false
      )]
    internal abstract class SPObjectBackup : IDisposable
    {
        /// <summary>
        /// Real backup method.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        /// <exception cref=""></exception>
        public abstract System.Threading.Tasks.Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobid, int ruleLevel, string mediaName, BackupInfoSender fileSender);

        public abstract System.Threading.Tasks.Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobid, int ruleLevel, string mediaName, BackupInfoSender fileSender);

        public IBackwardDependencyNodeCache<CacheNode> CacheSPObjs { get; set; }

        protected AveLogger mLog { get; set; }

        //protected static AveBackupOMFactory mFactory = AveBackupOMFactory.CreateBackupOMFactory();

        //protected static AveObjectModelFactory mFactory = new AveObjectModelFactory();

        //public BackupInfoSender AveSender { get; set; }

        public ScheduleConfiguration Configuration { get; set; }

        //public VaultBefArcInfo VaultBeforeArcInfo { get; set; }

        public SPObjectBackup VaultExport { get; set; }

        //add  this for life cycle rule,目前仅为了获取Size,如果以后想要获取更多属性，可以封成对象.
        public long BackupSize { get; set; }
        public VaultBefArcInfo VaultBeforeArcInfo { get; set; }
        public Dictionary<int, object> MicroFeedCache = new Dictionary<int, object>();

        public HSMConnector HSMConnectorInstance { get; set; }

        private string ConvertProperyString(string propertyString)
        {
            if (!string.IsNullOrEmpty(propertyString))
            {
                Regex reg = new Regex(@"<\s*(\w+)\s*[^>]*>([^<>]*)</\1>", RegexOptions.IgnoreCase);
                MatchEvaluator evaluator = new MatchEvaluator(GetGroup);
                while (reg.IsMatch(propertyString))
                {
                    propertyString = reg.Replace(propertyString, evaluator);
                }
                propertyString = propertyString.Replace("\r\n", "");
                propertyString = propertyString.Replace("<br />", "");
                propertyString = propertyString.Replace("<", "&lt;").Replace(">", "&gt;");
                if (propertyString.StartsWith(";#", StringComparison.OrdinalIgnoreCase) && propertyString.EndsWith(";#", StringComparison.OrdinalIgnoreCase))
                {
                    propertyString = propertyString.Substring(2, propertyString.Length - 4).Replace(";#", ";");
                }
            }
            return propertyString;
        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName)
        {
            try
            {
                if (Configuration.SharePointRetentionLabel == null)
                {
                    Configuration.InitRetentionLabelCollections(site);
                }
                if (Configuration.SharePointRetentionLabel.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                }
                return false;
            }
            catch(Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                throw;
            }
        }

        protected bool GetComplianceTagIfEnableRemove(IAveListItem listItem, out ListItemComplianceInfo complianceInfo)
        {
            try
            {
                complianceInfo = null;
                string retentionLabel = listItem.GetComplianceTagName();
                if (!string.IsNullOrWhiteSpace(retentionLabel) && 
                    (WrapperConfiguration.EnableRemoveRetentionLabel || 
                    (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                {
                    var nowComploanceInfo = listItem.GetComplianceInfo(false);
                    complianceInfo = new ListItemComplianceInfo()
                    {
                        ComplianceTag = nowComploanceInfo.ComplianceTag,
                        TagPolicyHold = nowComploanceInfo.TagPolicyHold,
                        TagPolicyEventBased = nowComploanceInfo.TagPolicyEventBased,
                        TagPolicyRecord = nowComploanceInfo.TagPolicyRecord
                    };
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Error($"fail get complianceTag, item:{listItem.Url},Exception:{e}");
                throw;
            }
        }

        protected void DeleteComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
            {
                DeleteComplianceTag(listItem, complianceInfo);
            }
        }

        protected void DeleteComplianceTag(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
                {
                    if (!complianceInfo.TagPolicyRecord && complianceInfo.TagPolicyHold && IsRecordTypeComplianceTag(listItem.Web.Site, complianceInfo.ComplianceTag))
                    {
                        listItem.LockRecordItem();
                    }
                    listItem.SetComplianceTagOnBulkItems("");
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail delete retention label,error message:{ex.Message},error:{ex}");
            }

        }

        protected bool SetComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
            {
                return SetComplianceTag(listItem, complianceInfo);
            }
            return false;
        }

        protected bool SetComplianceTag(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
            {
                try
                {
                    listItem.SetComplianceTagOnBulkItems(complianceInfo.ComplianceTag);
                    if (Configuration.SharePointRetentionLabel == null)
                    {
                        Configuration.InitRetentionLabelCollections(listItem.Web.Site);
                    }
                    if (Configuration.SharePointRetentionLabel.TryGetValue(complianceInfo.ComplianceTag, out AveComplianceTagInfo aveComplianceTagInfo))
                    {
                        if (aveComplianceTagInfo.UnlockedAsDefault && complianceInfo.TagPolicyHold && complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(listItem.Web.Site, complianceInfo.ComplianceTag))
                        {
                            listItem.LockRecordItem();
                        }
                    }
                    else
                    {
                        mLog.Warn($"can not get compliance init lock status, compliane name :{complianceInfo.ComplianceTag}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    mLog.Error($"Fail set retention label,label:{complianceInfo.ComplianceTag},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
            return false;
        }



        private static string GetGroup(Match m)
        {
            if (m.Groups[1].Value.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                return m.Groups[2].Value + " ";
            }
            return m.Groups[2].Value;
        }

        protected void SetItemAttributes(AveSPItem item, Dictionary<string, object> userData, string displayName, ArchiveApproveReport itemEntity, FileAtrributeInfo info, List<TagInfoCollection> tagInfos = null)
        {
            info.ExtraTitle = displayName;
            SetFullTextIndexAttributes(item, info);
            Char Delimiter = (Char)0x12;
            if (item != null && item.SPListItem.ParentList.BaseTemplate == AveListTemplateType.MicroFeed && userData != null)
            {
                string title = userData.ElementAt(0).Key.ToString();
                string microBlogType = userData.ElementAt(1).Key.ToString();
                string postAuthor = userData.ElementAt(2).Key.ToString();
                string definitionId = userData.ElementAt(3).Key.ToString();
                string rootPostID = userData.ElementAt(4).Key.ToString();
                string replyCount = userData.ElementAt(7).Key.ToString();
                string searchContent = userData.ElementAt(12).Key.ToString();
                string createdTime = userData.ElementAt(34).Key.ToString();
                string itemID = userData.ElementAt(32).Key.ToString();

                info.AddProperty(title + Delimiter.ToString() + userData[title]);
                string blogType = (MicroBlogType)userData[microBlogType] == MicroBlogType.Post ? "Post" : "Reply";
                info.AddProperty(microBlogType + Delimiter.ToString() + blogType);
                info.AddProperty(postAuthor + Delimiter.ToString() + userData[postAuthor]);
                info.AddProperty(definitionId + Delimiter.ToString() + userData[definitionId]);
                info.AddProperty(rootPostID + Delimiter.ToString() + userData[rootPostID]);
                info.AddProperty(replyCount + Delimiter.ToString() + userData[replyCount]);
                info.AddProperty(searchContent + Delimiter.ToString() + (userData[searchContent] != null ? userData[searchContent] : ""));

                if ((MicroBlogType)userData[microBlogType] == MicroBlogType.Post)
                {
                    info.PostId = userData[itemID].ToString();
                }
                else
                {
                    if (userData.ContainsKey(rootPostID))
                    {
                        info.PostId = userData[rootPostID].ToString();
                    }
                }
                if (userData.ContainsKey(createdTime))
                {
                    info.NewsFeedCreatedTime = ((DateTime)userData[createdTime]).Ticks;
                }
            }
            info.AddProperty("TimeZoneID" + Delimiter.ToString() + Configuration.TimeZone);
            if (tagInfos != null)
            {
                foreach (TagInfoCollection tagInfo in tagInfos)
                {
                    string keyWord = tagInfo.Key;
                    if (tagInfo.Key.Equals("ArchiveTime", StringComparison.OrdinalIgnoreCase))
                    {
                        keyWord = "Archived";
                    }
                    if (tagInfo.Key.Equals("ArchiveBy", StringComparison.OrdinalIgnoreCase))
                    {
                        keyWord = "Archived By";
                    }
                    info.AddProperty(AveConverter.ReplaceSpecialChar(XmlConvert.DecodeName(keyWord)) + Delimiter.ToString() + tagInfo.Value.ToString());
                }
            }
            //info.AddProperty(AveConverter.ReplaceSpecialChar("Title") + Delimiter.ToString() + userData["Title"].ToString());
            //userData.Remove("Title");
            if ((item == null || item.SPListItem.ParentList.BaseTemplate != AveListTemplateType.MicroFeed) && userData != null)
            {
                foreach (string name in userData.Keys)
                {
                    //MetaInfo 是wrapper后添加的column，对应value为一些column的name和content的字符串转换成byte数组，可以不用显示到Attribute中去
                    if (userData[name] != null && !(name.Equals("MetaInfo", StringComparison.OrdinalIgnoreCase) && userData[name].ToString().Equals("System.Byte[]", StringComparison.OrdinalIgnoreCase)))
                    {
                        info.AddProperty(AveConverter.ReplaceSpecialChar(name) + Delimiter.ToString() + ConvertProperyString(userData[name].ToString()));
                    }
                    else if (userData[name] == null || userData[name].ToString() == string.Empty)
                    {
                        info.AddProperty(AveConverter.ReplaceSpecialChar(name) + Delimiter.ToString() + string.Empty);
                    }
                }
            }
        }

        private static void SetFullTextIndexAttributes(AveSPItem item, FileAtrributeInfo info)
        {
            //TODO

            if (item != null)
            {
                var fullTextIndex = item.GetFullTextIndex(FullTextIndexLevel.IncludeAllVisiableColumns);
                info.AddFullTextProperty("Created", fullTextIndex.Created.ToString());
                info.AddFullTextProperty("Modified", fullTextIndex.Modified.ToString());
                info.AddFullTextProperty("Size", fullTextIndex.Size.ToString());
                info.AddFullTextProperty("ContentTypeName", fullTextIndex.ContentTypeName);
                if (fullTextIndex.ColumnValues != null)
                {
                    foreach (KeyValuePair<string, object> keyValue in fullTextIndex.ColumnValues)
                    {
                        if (!info.ContainFullTextAttribute(keyValue.Key))
                        {
                            if (keyValue.Value != null)
                            {
                                info.AddFullTextProperty(keyValue.Key, keyValue.Value.ToString());
                            }
                        }
                    }
                }

            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //if (VaultBeforeArcInfo != null)
            //{
            //    VaultBeforeArcInfo.Dispose();
            //}
        }

        #endregion
    }

}