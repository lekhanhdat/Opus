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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using Microsoft.SharePoint.Utilities;
using System.Collections;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.SPService;
using AvePoint.GCommon;
using System.Linq;
using AvePoint.ObjectModel.Server19.Office;

namespace AvePoint.ObjectModel.Server19
{
    class AveListItem : AveSecurableObject, IAveListItem, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveListItem));
        private SPListItem mListItem;
        private AveFile mFile;
        private AveAttachmentCollection mAttachments;
        private AveListItemCollection mItems;
        private Dictionary<string, object> mFieldValues;
        private AveFolder mFolder;
        private AveFieldCollection mFields;
        private AveModerationInformation mModerationInformation;
        private AveListItemVersionCollection mVersions;
        private AveSite mSite;
        private AveList mParentList;
        private AveFieldStringValues m_FieldValuesAsHtml;
        private AveFieldStringValues m_FieldValuesAsText;
        private AveFieldStringValues m_FieldValuesForEdit;
        private AveAudit mAudit;
        private AveContentTypeId mContentTypeId;
        private AveLinkCollection backwardLinks;
        private AveLinkCollection forwardLinks;

        public AveListItem(AveList parentList, SPListItem listItem)
            : base(listItem)
        {
            mListItem = listItem;
            mParentList = parentList;
            mSite = mParentList.ParentWeb.Site as AveSite;
        }

        public AveListItem(AveListItemCollection items, SPListItem listItem)
            : base(listItem)
        {
            mItems = items;
            mListItem = listItem;
            mParentList = mItems.List as AveList;
            mSite = items.List.ParentWeb.Site as AveSite;
        }

        #region IAveListItem Members

        public IAveContentType ContentType
        {
            get
            {
                SPContentType contentType = mListItem.ContentType;
                if (contentType == null)
                {
                    return null;
                }
                return new AveContentType(this.Web.ContentTypes as AveContentTypeCollection, contentType);
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get
            {
                if (mContentTypeId == null)
                {
                    mContentTypeId = new AveContentTypeId(mListItem.ContentTypeId);
                }
                return mContentTypeId;
            }
        }

        public IAveWorkflowCollection WorkFlows
        {
            get { return new AveWorkflowCollection(this,mListItem.Workflows); }
        }

        public string DisplayName
        {
            get
            {
                return mListItem.DisplayName;
            }
        }

        public AveBasePermissions EffectiveBasePermissions
        {
            get { return (AveBasePermissions)mListItem.EffectiveBasePermissions; }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                if (mFieldValues == null)
                {
                    mFieldValues = new Dictionary<string, object>();
                    foreach (SPField spField in mListItem.Fields)
                    {
                        mFieldValues.Add(spField.InternalName, mListItem[spField.Id]);
                    }
                }
                return mFieldValues;
            }
        }

        public IAveFieldStringValues FieldValuesAsHtml
        {
            get
            {
                if (m_FieldValuesAsHtml == null)
                {
                    m_FieldValuesAsHtml = new AveFieldStringValues(mListItem, AveFieldValuesType.FieldValueAsHtml);
                }
                return m_FieldValuesAsHtml;
            }
        }

        public IAveFieldStringValues FieldValuesAsText
        {
            get
            {
                if (m_FieldValuesAsText == null)
                {
                    m_FieldValuesAsText = new AveFieldStringValues(mListItem, AveFieldValuesType.FieldValueAsText);
                }
                return m_FieldValuesAsText;
            }
        }

        public IAveFieldStringValues FieldValuesForEdit
        {
            get
            {
                if (m_FieldValuesForEdit == null)
                {
                    m_FieldValuesForEdit = new AveFieldStringValues(mListItem, AveFieldValuesType.FieldValueForEdit);
                }
                return m_FieldValuesForEdit;
            }
        }

        public IAveFile File
        {
            get
            {
                if (mFile == null)
                {
                    SPFile file = mListItem.File;
                    if (file != null)
                    {
                        mFile = new AveFile(this.Web as AveWeb, file);
                    }
                }
                return mFile;
            }
        }

        public AveFileSystemObjectType FileSystemObjectType
        {
            get { return (AveFileSystemObjectType)mListItem.FileSystemObjectType; }
        }

        public IAveAttachmentCollection Attachments
        {
            get
            {
                if (mAttachments == null || mAttachments.IsDirty)
                {
                    mAttachments = new AveAttachmentCollection(mListItem.Attachments, this);//此集合再加完附件后需要重新获取一下
                }
                return mAttachments;
            }
        }

        public int ID
        {
            get { return mListItem.ID; }
        }

        public IAveList ParentList
        {
            get
            {
                return mParentList;
            }
        }

        public object this[string fieldName]
        {
            get
            {
                var fieldValue = mListItem[fieldName];
                if (fieldValue == null || (AveTypeHelper.IsBasicType(fieldValue.GetType()) && !fieldValue.GetType().IsEnum))
                {
                    return fieldValue;
                }
                return AveServerAssemblyInit.CreateElement(typeof(object), fieldValue);
            }
            set
            {
                if (value != null)
                {
                    //此处需要对两种特殊的FieldValue进行特殊处理一下，不能直接赋值
                    mListItem[fieldName] = SetRealFieldValueToListItem(value);
                }
                else
                {
                    mListItem[fieldName] = null;
                }
            }
        }

        public void Update()
        {
            mListItem.Update();
        }

        public void UpdateOverwriteVersion()
        {
            mListItem.UpdateOverwriteVersion();
        }

        public Guid UniqueId
        {
            get { return mListItem.UniqueId; }
        }

        public IAveFolder Folder
        {
            get
            {
                if (mFolder == null)
                {
                    SPFolder folder = mListItem.Folder;
                    if (folder != null)
                    {
                        mFolder = new AveFolder(this.Web as AveWeb, folder);
                    }
                }
                return mFolder;
            }
        }

        public IAveWeb Web
        {
            get
            {
                return mParentList.Lists.Web;
            }
        }

        public IAveModerationInformation ModerationInformation
        {
            get
            {
                if (mModerationInformation == null)
                {
                    SPModerationInformation moderationInformation = mListItem.ModerationInformation;
                    if (moderationInformation != null)
                    {
                        mModerationInformation = new AveModerationInformation(moderationInformation);
                    }
                }
                return mModerationInformation;
            }
        }

        public string Name
        {
            get { return mListItem.Name; }
        }

        public void SystemUpdate(bool incrementListItemVersion)
        {
            mListItem.SystemUpdate(incrementListItemVersion);
        }

        public void SystemUpdate()
        {
            mListItem.SystemUpdate();
        }

        public void Delete()
        {
            mListItem.Delete();
        }

        internal void RemoveItemWorkflowInstance()
        {
            try
            {
                List<IAveWorkflow> workflows = this.Web.Site.WorkflowManager.GetItemWorkflows(this);
                if (workflows != null && workflows.Count > 0)
                {
                    foreach (IAveWorkflow workflow in workflows)
                    {
                        this.Web.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while do remove workflow from list item. Exception: {0}", ex.ToString());
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (mFields == null)
                {
                    SPFieldCollection fields = mListItem.Fields;
                    if (fields != null)
                    {
                        mFields = new AveFieldCollection(this.Web as AveWeb, fields);
                    }
                }
                return mFields;
            }
        }

        public AveFileLevel Level
        {
            get { return (AveFileLevel)mListItem.Level; }
        }

        public Hashtable Properties
        {
            get { return mListItem.Properties; }
        }

        public object this[Guid fieldId]
        {
            get
            {
                var fieldValue = mListItem[fieldId];
                if (fieldValue == null || (AveTypeHelper.IsBasicType(fieldValue.GetType()) && !fieldValue.GetType().IsEnum))
                {
                    return fieldValue;
                }
                return AveServerAssemblyInit.CreateElement(typeof(object), fieldValue);
            }
            set
            {
                if (value != null)
                {
                    //此处需要对两种特殊的FieldValue进行特殊处理一下，不能直接赋值
                    mListItem[fieldId] = SetRealFieldValueToListItem(value);
                }
                else
                {
                    mListItem[fieldId] = null;
                }
            }
        }

        private object GetTaxonomyFieldValue(object value)
        {
            return (value as AveTaxonomyFieldValue).TaxonomyFieldValue;
        }

        private object GetTaxonomyFieldValueCollection(object value)
        {
            return (value as AveTaxonomyFieldValueCollection).TaxonomyFieldValueCollection;
        }

        private object SetRealFieldValueToListItem(object value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItem.SetRealFieldValueToListItem"))
            {

                object realFieldValue = value;
                switch (value.GetType().Name)
                {
                    case "AveTaxonomyFieldValue":
                        if (AveEnv.IsMoss)
                        {
                            realFieldValue = GetTaxonomyFieldValue(value);
                        }
                        break;
                    case "AveTaxonomyFieldValueCollection":
                        if (AveEnv.IsMoss)
                        {
                            realFieldValue = GetTaxonomyFieldValueCollection(value);
                        }
                        break;
                    case "AveFieldLookupValue":
                        realFieldValue = ((AveFieldLookupValue)value).FieldLookupValue;
                        break;
                    case "AveFieldLookupValueCollection":
                        realFieldValue = ((AveFieldLookupValueCollection)value).FieldLookupValues;
                        break;
                    case "AveFieldUserValue":
                        realFieldValue = ((AveFieldUserValue)value).FieldUserValue;
                        break;
                    case "AveFieldUserValueCollection":
                        realFieldValue = ((AveFieldUserValueCollection)value).FieldUserValueCollection;
                        break;
                    case "AveFieldUrlValue":
                        realFieldValue = ((AveFieldUrlValue)value).FieldUrlValue;
                        break;
                    default:
                        break;
                }
                return realFieldValue;

            }

        }

        public string Url
        {
            get { return mListItem.Url; }
        }

        public string Title
        {
            get { return mListItem.Title; }
        }

        public Guid Recycle()
        {
            return mListItem.Recycle();
        }

        public IAveListItemVersionCollection Versions
        {
            get
            {
                if (mVersions == null)
                {
                    mVersions = new AveListItemVersionCollection(this, mListItem.Versions);
                }
                return mVersions;
            }
        }

        public string Xml
        {
            get { return mListItem.Xml; }
        }

        public IAveAudit Audit
        {
            get
            {
                if (mAudit == null)
                {
                    mAudit = new AveAudit(mListItem.Audit);
                }
                return mAudit;
            }
        }

        #endregion

        #region IAveSecurableObject Members

        //public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        //{
        //    mListItem.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
        //}

        //public void BreakRoleInheritance(bool copyRoleAssignments)
        //{
        //    mListItem.BreakRoleInheritance(copyRoleAssignments);
        //}

        //public void ResetRoleInheritance()
        //{
        //    mListItem.ResetRoleInheritance();
        //}

        //public bool HasUniqueRoleAssignments
        //{
        //    get { return mListItem.HasUniqueRoleAssignments; }
        //}

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                return new AveSecurableObjectImpl(this.Web as AveWeb, AveAssemblyUtility.GetPropertyValue(mListItem, "SecurableObjectImpl"));
            }
        }

        #endregion

        internal SPListItem ListItem
        {
            get
            {
                return mListItem;
            }
        }

        #region IAveListItem Members

        public void UpdateInternal(Type[] argsTypes, object[] args)
        {
            AveAssemblyUtility.InvokeMethod(mListItem, mListItem.GetType(), "UpdateInternal", argsTypes, args);
        }

        public void SetValue(Type[] argsTypes, object[] args)
        {
            AveAssemblyUtility.InvokeMethod(mListItem, mListItem.GetType(), "SetValue", argsTypes, args);
        }

        public int GetTpIdByTpGuid(Guid tp_guid, Guid listId)
        {
            return mSite.QueryService.GetTpIdByTpGuid(mSite.ID, tp_guid, listId);
        }

        public Guid GetTPGuid()
        {
            return mSite.QueryService.GetListItemGuid(mSite.ID, mListItem.ParentList.ID, mListItem.ID);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mFields != null)
            {
                mFields.Dispose();
                mFields = null;
            }
        }

        #endregion

        #region add for Micro Feed Archiver

        public IAveOSocialFeedManager GetMicroFeedManager()
        {
            IAveOSocialFeedManager feedManager = null;
            try
            {
                AveServiceContext context = new AveServiceContext();
                IAveServiceContext serviceContext = context.GetContext(mSite);
                AveOUserProfileManager upm = new AveOUserProfileManager(serviceContext);
                IAveOUserProfile up = upm.GetUserProfile(mListItem["PostAuthor"].ToString()); 
                feedManager = new AveOSocialFeedManager(up, serviceContext);
                return feedManager;
            }
            catch (Exception ex)
            {
                logger.Warn("Get micro feed manager error. {0}", ex.ToString());
                throw;
            }
        }

        public Dictionary<int, List<int>> GetMicroFeedReplyID()
        {
            Dictionary<int, List<int>> microFeedReplyIDCache = new Dictionary<int, List<int>>();
            try
            {
                int postId = -1;
                IAveOSocialFeedManager feedManager = GetMicroFeedManager();
                AveOSocialFeedOptions option = new AveOSocialFeedOptions();
                option.MaxThreadCount = int.MaxValue;
                option.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                option.NewerThan = DateTime.MinValue;
                option.OlderThan = DateTime.Now.ToUniversalTime().AddDays(1);
                IAveOSocialFeed feed;
                do
                {
                    feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                    foreach (IAveOSocialThread thread in feed.Threads)
                    {
                        if (thread.Attributes != null && thread.Attributes != AveOSocialThreadAttributes.None)
                        {
                            IAveOSocialThread fullThread = feedManager.GetFullThread(thread.RootPost.Id);
                            List<int> replyIDs = new List<int>();
                            postId = Convert.ToInt32(fullThread.RootPost.Id.Split('.')[7]);
                            foreach (IAveOSocialPost reply in fullThread.Replies)
                            {
                                replyIDs.Add(Convert.ToInt32(reply.Id.Split('.')[7]));
                            }
                            if (!microFeedReplyIDCache.ContainsKey(postId))
                            {
                                microFeedReplyIDCache.Add(postId, replyIDs);
                            }
                        }
                    }
                    if (feed.Threads.Count() != 0)
                    {
                        option.OlderThan = feed.Threads.Last().RootPost.CreatedTime;
                    }
                }
                while (feed != null && feed.Threads.Count() != 0);
                return microFeedReplyIDCache;
            }
            catch (Exception ex)
            {
                logger.Warn("Get micro feed reply id cache error. {0}", ex.ToString());
                throw;
            }
        }

        public Dictionary<int, List<string>> GetMicroFeedLiker()
        {
            Dictionary<int, List<string>> microFeedLikerCache = new Dictionary<int, List<string>>();
            try
            {
                int postId = -1;
                IAveOSocialFeedManager feedManager = GetMicroFeedManager();
                AveOSocialFeedOptions option = new AveOSocialFeedOptions();
                option.MaxThreadCount = int.MaxValue;
                option.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                option.NewerThan = DateTime.MinValue;
                option.OlderThan = DateTime.Now.ToUniversalTime().AddDays(1);
                IAveOSocialFeed feed;
                do
                {
                    feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                    foreach (IAveOSocialThread thread in feed.Threads)
                    {
                        if (thread.Attributes != null && thread.Attributes != AveOSocialThreadAttributes.None)
                        {
                            IAveOSocialThread fullThread = feedManager.GetFullThread(thread.RootPost.Id);
                            IAveOSocialActor[] actors = fullThread.Actors;
                            List<string> FeedLikers = new List<string>();
                            postId = Convert.ToInt32(fullThread.RootPost.Id.Split('.')[7]);
                            if (fullThread.RootPost.LikerInfo.IncludesCurrentUser)
                            {
                                FeedLikers.Add(mListItem["PostAuthor"].ToString());
                            }
                            foreach (int index in fullThread.RootPost.LikerInfo.Indexes)
                            {
                                FeedLikers.Add(actors[index].AccountName);
                            }
                            foreach (IAveOSocialPost reply in fullThread.Replies)
                            {
                                if (reply.LikerInfo.IncludesCurrentUser)
                                {
                                    FeedLikers.Add(mListItem["PostAuthor"].ToString());
                                }
                                foreach (int index in reply.LikerInfo.Indexes)
                                {
                                    FeedLikers.Add(actors[index].AccountName);
                                }
                            }
                            if (!microFeedLikerCache.ContainsKey(postId))
                            {
                                microFeedLikerCache.Add(postId, FeedLikers);
                            }
                        }
                        if (feed.Threads.Count() != 0)
                        {
                            option.OlderThan = feed.Threads.Last().RootPost.CreatedTime;
                        }
                    }
                }
                while (feed != null && feed.Threads.Count() != 0);
                return microFeedLikerCache;
            }
            catch (Exception ex)
            {
                logger.Warn("Get micro feed like cache error. {0}", ex.ToString());
                throw;
            }
        }

        public void GetMicroFeedMentionAndTag(ref Dictionary<int, List<string>> microFeedMentionCache, ref Dictionary<int, List<string>> microFeedMentionDisPlayCache, ref Dictionary<int, List<string>> microFeedTagCache)
        {
            try
            {
                int postId = -1;
                IAveOSocialFeedManager feedManager = GetMicroFeedManager();
                AveOSocialFeedOptions option = new AveOSocialFeedOptions();
                option.MaxThreadCount = int.MaxValue;
                option.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                option.NewerThan = DateTime.MinValue;
                option.OlderThan = DateTime.Now.ToUniversalTime().AddDays(1);
                IAveOSocialFeed feed;
                do
                {
                    feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                    foreach (IAveOSocialThread thread in feed.Threads)
                    {
                        if (thread.Attributes != null && thread.Attributes != AveOSocialThreadAttributes.None)
                        {
                            IAveOSocialThread fullThread = feedManager.GetFullThread(thread.RootPost.Id);
                            IAveOSocialActor[] actors = fullThread.Actors;
                            List<string> FeedMentions = new List<string>();
                            List<string> FeedMentionsDisPlay = new List<string>();
                            List<string> FeedTags = new List<string>();
                            postId = Convert.ToInt32(fullThread.RootPost.Id.Split('.')[7]);
                            if (fullThread.RootPost.Overlays != null)
                            {
                                foreach (IAveOSocialDataOverlay overlay in fullThread.RootPost.Overlays)
                                {
                                    IAveOSocialActor actor = actors[overlay.ActorIndexes[0]];
                                    if (actor.ActorType == AveOSocialActorType.User)
                                    {
                                        FeedMentions.Add(actor.AccountName);
                                        FeedMentionsDisPlay.Add(actor.Name);
                                    }
                                    else if (actor.ActorType == AveOSocialActorType.Tag)
                                    {
                                        FeedTags.Add(actor.Name);
                                    }
                                }
                            }
                            foreach (IAveOSocialPost reply in fullThread.Replies)
                            {
                                if (reply.Overlays != null)
                                {
                                    foreach (IAveOSocialDataOverlay overlay in reply.Overlays)
                                    {
                                        IAveOSocialActor actor = actors[overlay.ActorIndexes[0]];
                                        if (actor.ActorType == AveOSocialActorType.User)
                                        {
                                            FeedMentions.Add(actor.AccountName);
                                            FeedMentionsDisPlay.Add(actor.Name);
                                        }
                                        else if (actor.ActorType == AveOSocialActorType.Tag)
                                        {
                                            FeedTags.Add(actor.Name);
                                        }
                                    }
                                }
                            }
                            if (!microFeedMentionCache.ContainsKey(postId))
                            {
                                microFeedMentionCache.Add(postId, FeedMentions);
                            }
                            if (!microFeedMentionDisPlayCache.ContainsKey(postId))
                            {
                                microFeedMentionDisPlayCache.Add(postId, FeedMentionsDisPlay);
                            }
                            if (!microFeedTagCache.ContainsKey(postId))
                            {
                                microFeedTagCache.Add(postId, FeedTags);
                            }
                        }
                    }
                    if (feed.Threads.Count() != 0)
                    {
                        option.OlderThan = feed.Threads.Last().RootPost.CreatedTime;
                    }
                }
                while (feed != null && feed.Threads.Count() != 0);
            }
            catch (Exception ex)
            {
                logger.Warn("Get micro feed mention and tag cache error. {0}", ex.ToString());
                throw;
            }
        }

        public Dictionary<int, string> GetMicroFeedPostID()
        {
            Dictionary<int, string> microFeedPostIDCache = new Dictionary<int, string>();
            try
            {
                int postId = -1;
                IAveOSocialFeedManager feedManager = GetMicroFeedManager();
                AveOSocialFeedOptions option = new AveOSocialFeedOptions();
                option.MaxThreadCount = int.MaxValue;
                option.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                option.NewerThan = DateTime.MinValue;
                option.OlderThan = DateTime.Now.ToUniversalTime().AddDays(1);
                IAveOSocialFeed feed;
                do
                {
                    feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                    foreach (IAveOSocialThread thread in feed.Threads)
                    {
                        if (thread.Attributes != null && thread.Attributes != AveOSocialThreadAttributes.None)
                        {
                            postId = Convert.ToInt32(thread.RootPost.Id.Split('.')[7]);
                            if (!microFeedPostIDCache.ContainsKey(postId))
                            {
                                microFeedPostIDCache.Add(postId, thread.RootPost.Id);
                            }
                        }
                    }
                    if (feed.Threads.Count() != 0)
                    {
                        option.OlderThan = feed.Threads.Last().RootPost.CreatedTime;
                    }
                }
                while (feed != null && feed.Threads.Count() != 0);
                return microFeedPostIDCache;
            }
            catch (Exception ex)
            {
                logger.Warn("Get micro feed post id cache error. {0}", ex.ToString());
                throw;
            }
        }

        public void DeletePost(bool needReloadFeedManager = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItem.DeletePost"))
            {
                string id = string.Empty;
                string postId = string.Empty;
                try
                {
                    //get social feed
                    AveServiceContext context = new AveServiceContext();
                    IAveServiceContext serviceContext = context.GetContext(mSite);
                    AveOUserProfileManager upm = new AveOUserProfileManager(serviceContext);
                    IAveOUserProfile up = upm.GetUserProfile(mListItem["PostAuthor"].ToString());
                    AveOSocialFeedManager feedManager = new AveOSocialFeedManager(up, serviceContext);
                    AveOSocialFeedOptions option = new AveOSocialFeedOptions();
                    option.MaxThreadCount = int.MaxValue;
                    option.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                    option.NewerThan = DateTime.MinValue;
                    option.OlderThan = DateTime.Now.ToUniversalTime().AddDays(1);
                    IAveOSocialFeed feed;
                    do
                    {
                        feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                        List<IAveOSocialThread> feedThreads = new List<IAveOSocialThread>(feed.Threads);
                        //thread.RootPost.Id.Split('.') [0]-[5] RootPostOwnerID,[6] ID,[7] PostID
                        IEnumerable<IAveOSocialThread> threads = from t in feedThreads where t.RootPost.Id.Split('.')[7].Equals(mListItem["ID"].ToString()) select t;
                        foreach (IAveOSocialThread thread in threads)
                        {
                            IAveOSocialThread fullThread = feedManager.GetFullThread(thread.RootPost.Id);
                            //delete replies
                            foreach (IAveOSocialPost reply in fullThread.Replies)
                            {
                                feedManager.DeletePost(reply.Id);
                            }
                            //normal site does not need reload Feed Manager before delete root post
                            if (!needReloadFeedManager)
                            {
                                //delete normal site root post
                                postId = fullThread.RootPost.Id;
                                feedManager.DeletePost(postId);
                            }
                        }
                        //my site needs reload Feed Manager before delete root post
                        if (needReloadFeedManager)
                        {
                            feedManager = new AveOSocialFeedManager(up, serviceContext);
                            feed = feedManager.GetFeedFor(mParentList.ParentWeb.Url, option);
                            //delete my site root post
                            foreach (IAveOSocialThread thread in threads)
                            {
                                postId = thread.RootPost.Id;
                                feedManager.DeletePost(postId);
                            }
                        }
                        if (feed.Threads.Count() != 0)
                        {
                            option.OlderThan = feed.Threads.Last().RootPost.CreatedTime;
                        }
                    }
                    while (feed != null && feed.Threads.Count() != 0);
                }
                catch (Exception ex)
                {
                    logger.Warn("Delete post error. {0}", ex.ToString());
                    throw;
                }
            }
        }

        #endregion

        public string IconOverlay
        {
            get
            {
                return mListItem.IconOverlay;
            }
            set
            {
                mListItem.IconOverlay = value;
            }
        }

        public bool MissingRequiredFields
        {
            get { return mListItem.MissingRequiredFields; }
        }

        public AveFileSystemObjectType SortType
        {
            get
            {
                return (AveFileSystemObjectType)mListItem.SortType;
            }
            set
            {
                mListItem.SortType = (SPFileSystemObjectType)value;
            }
        }


        public bool HasPublishedVersion
        {
            get { return mListItem.HasPublishedVersion; }
        }
        public IAveLinkCollection BackwardLinks
        {
            get
            {
                if (backwardLinks == null)
                {
                    backwardLinks = new AveLinkCollection(mListItem.BackwardLinks);
                }
                return backwardLinks;
            }
        }
        public IAveLinkCollection ForwardLinks
        {
            get
            {
                if (forwardLinks == null)
                {
                    forwardLinks = new AveLinkCollection(mListItem.ForwardLinks);
                }
                return forwardLinks;
            }
        }

        public IAveListItemComplianceInfo ComplianceTagInfo
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public void ReplaceLink(string oldUrl, string newUrl)
        {
            mListItem.ReplaceLink(oldUrl, newUrl);
        }

        public void SetComplianceTag(AveItemComplianceTagInfo info)
        {
            throw new NotSupportedException();
        }

        public void SystemUpdateForRecords()
        {
            throw new NotImplementedException();
        }

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            throw new NotImplementedException();
        }
    }
}