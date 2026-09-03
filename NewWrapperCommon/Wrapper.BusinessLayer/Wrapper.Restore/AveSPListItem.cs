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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Xml;
using System.Collections;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Contract.CodeReview;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Common.Office;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Common.Office;
using System.Security.Principal;
using System.Threading;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveSPListItem : AveSPItem, AvePoint.Wrapper.Restore.IAveSPListItem
    {
        protected AveListItemInfo mListItemInfo
        {
            get
            {
                return mBaseItemInfo as AveListItemInfo;
            }
            set
            {
                this.mBaseItemInfo = value;
            }
        }

        public AveListItemInfo ListItemInfo
        {
            get
            {
                return mBaseItemInfo as AveListItemInfo;
            }
        }

        public List<AveListTitleMappingInfo> ListTitleMappingInfo { get; set; }
        public string TagUrl
        {
            get
            {
                string fileUrl = string.Empty;
                string webUrl = mAveSPList.ParentWeb.SPWeb.Url;
                string webRelativeUrl = mAveSPList.ParentWeb.SPWeb.ServerRelativeUrl;
                if (!string.IsNullOrEmpty(mAveSPList.SPList.DefaultDisplayFormUrl))
                {
                    if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
                    {
                        fileUrl = mAveSPList.SPList.DefaultDisplayFormUrl + "?ID=" + this.RowId;
                    }
                    else
                    {
                        fileUrl = webUrl.TrimEnd('/') + "/" + mAveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(webRelativeUrl.TrimStart('/').Length).TrimStart('/') + "?ID=" + this.RowId;
                    }
                }
                else if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Meetings && this.SPListItem.ID != 0)
                {
                    //qlluo: meeting series下只有一个aspx页(movetodt.aspx)可以显示, 因此去掉查询Hidden File逻辑, 直接Hard Code。
                    //if (this.mParentFolder.SPFolder.HiddenFiles != null)
                    //{
                    //    if (this.mParentFolder.SPFolder.HiddenFiles.Count > 1 && webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase) > 0)
                    //    {
                    //        fileUrl = webUrl.Substring(0, webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase)) + this.mParentFolder.ServerRelativeUrl + "/" + this.mParentFolder.SPFolder.HiddenFiles[0].Name + "?ID=" + this.SPListItem.ID;
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
                else if ((int)mAveSPList.SPList.BaseTemplate == 550 && mListItemInfo.OriginalRowId != 0)
                {
                    //mysite下social list只有此url可以显示
                    //http://sp13workflow15:21367/personal/domainuser001/Social/FollowedContent.aspx?id=3
                    fileUrl = string.Format("{0}{1}?ID={2}",
                        webUrl.TrimEnd('/'),
                        "/Social/FollowedContent.aspx",
                        mListItemInfo.OriginalRowId);
                }
                return fileUrl;
            }
        }

        #region Obsolete field&property
        [Obsolete("already inherit from AveSPItem, will remove later")]
        public AveSPItem AveSPItem
        {
            get { return this; }
        }
        [Obsolete("no use now, will remove later")]
        public bool NeedChangeItemId
        {
            get { return mListItemInfo.NeedChangeItemId; }
            set { mListItemInfo.NeedChangeItemId = value; }
        }
        [Obsolete("no use now, will remove later")]
        private AveItemSecurity mItemSecurity;
        [Obsolete("no use now, will remove later")]
        public AveObjectSecurity Security
        {
            get
            {
                if (mItemSecurity == null)
                {
                    mItemSecurity = new AveItemSecurity(this);
                }
                return mItemSecurity;
            }
        }
        [Obsolete("no use now, will remove later")]
        private string mSrcUrl;
        [Obsolete("no use now, will remove later")]
        private string mUrl;
        [Obsolete("no use now, will remove later")]
        private long mSize;
        [Obsolete("no use now, will remove later")]
        public string SrcUrl
        {
            get
            {
                return mSrcUrl;
            }
        }
        [Obsolete("no use now, will remove later")]
        public string Url
        {
            get
            {
                return mUrl;
            }
        }
        [Obsolete("no use now, will remove later")]
        public long Size
        {
            get
            {
                return mSize;
            }
        }
        [Obsolete("no use now, will remove later")]
        public bool? ConflictWithDocument
        {
            get
            {
                if (mListItemInfo.RestoringItem == null)
                {
                    return null;
                }
                if (mListItemInfo.RestoringItem.OverwriteAllVersion)
                {
                    return true;
                }
                if (!mListItemInfo.RestoringItem.ConflictWithDocument)
                {
                    return false;
                }
                //if (RestoreOption.mAveItemRestoreOption.DELETE_ITEM)
                //{
                //    return true;
                //}
                return !IsNewCreated;
            }
        }
        #endregion
        public AveSPListItem()
        {

        }
        public AveSPListItem(AveSPFolder aveFolder, string name)
            : base(AveItemType.ListItem, aveFolder, name)
        {
            //mAveSPItem = new AveSPItem(mListItemInfo, AveItemType.ListItem, aveFolder, aveFolder.QueryService, name);
            mQueryService = aveFolder.QueryService;
            mAveParentSite = aveFolder.ParentSite;
            mAveSPList = aveFolder.ParentList;
            mParentFolder = aveFolder;

            mListItemInfo.ParentWebRelativeUrl = mAveSPList.ParentWeb.SPWeb.ServerRelativeUrl;//mAveSPList.ParentWeb.ServerRelativeUrl;
            mListItemInfo.ParentListTitle = mAveSPList.SPList.Title;//mAveSPList.Name;
            //mListItemInfo.IsNewCreated = aveFolder.IsNewCreated;//doc-67167

            mListItemInfo.ListContainsTodayFomula = mAveSPList.containsTODAY;
        }

        /// <summary>
        /// 主要给Replicator使用，因为Replicator知道目的端的ItemId
        /// </summary>
        /// <param name="aveFolder"></param>
        /// <param name="name"></param>
        /// <param name="rowId"></param>
        public AveSPListItem(AveSPFolder aveFolder, string name, int rowId)
            : this(aveFolder, name)
        {
            mListItemInfo.RowId = rowId;
        }

        public string ResetAvailableName()
        {
            try
            {

                int index = mListItemInfo.Name.LastIndexOf("_.000", StringComparison.Ordinal);
                int itemId = Convert.ToInt32(mListItemInfo.Name.Substring(0, index));
                if (!mQueryService.CheckItemIdAvailable(mAveParentSite.SPSite.ID, mAveSPList.SPList.ID, itemId))
                {
                    int id = GetNextAvailableId();
                    mListItemInfo.Name = id.ToString() + "_.000";
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("ResetAvailableName Error.\n error message:{0}", e));
                //mLog.Warn("ResetAvailableName Error: " + e.ToString());
            }
            return mListItemInfo.Name;
        }

        public string ResetAvailableName(DateTime modified)
        {
            try
            {
                try
                {
                    string listItemTempId = GetListItemIdByName(this.mListItemInfo.Name);
                    if (!String.IsNullOrEmpty(listItemTempId))
                    {
                        var listItemTemp = this.ParentList.SPList.GetItemById(Convert.ToInt32(listItemTempId));
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, string.Format("Item is not Exist.\n error message:{0}", e));
                    return this.mListItemInfo.Name;
                }
                DateTime dt = GetListItemModifiedTime();
                if ((modified == dt) && RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME)
                {
                    mListItemInfo.NeedChangeItemId = false;
                }
                else if (dt == DateTime.MinValue)
                {
                    mListItemInfo.NeedChangeItemId = true;
                }
                else
                {
                    int id = GetNextAvailableId();
                    mListItemInfo.Name = id.ToString() + "_.000";
                    mListItemInfo.NeedChangeItemId = false;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("ResetAvailableName Error.\n error message:{0}", e));
                //mLog.Warn("ResetAvailableName Error: " + e.ToString());
            }
            return mListItemInfo.Name;
        }

        public bool NeedAppendNewVersion(DateTime modified)
        {
            bool needAppendNewVersion = false;
            try
            {
                DateTime dt = DateTime.MinValue;
                dt = GetListItemModifiedTime();
                if (!dt.Equals(DateTime.MinValue) && (!RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME || dt != modified))
                {
                    needAppendNewVersion = true;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("Set NeedAppendNewVersion Error.\n error message:{0}", e));
            }
            return needAppendNewVersion;
        }

        /// <summary>
        /// 判断listitem是否存在，包括在回收站，目前只用于append
        /// </summary>
        /// <returns></returns>
        private DateTime GetListItemModifiedTime()
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                int originalId = Convert.ToInt32(mListItemInfo.Name.Substring(0, mListItemInfo.Name.Length - "_.000".Length));
                if (mAveParentSite != null && mAveParentSite.QueryService != null && mAveSPList != null)
                {
                    dt = mAveParentSite.QueryService.CheckItemIdAvailableAndGetModifiedTimeForAppend(mAveParentSite.SPSite.ID, mAveSPList.SPList.ID, originalId);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetListItemFailed, e);
            }
            return dt;
        }

        private int GetNextAvailableId()
        {
            return mQueryService.GetNextAvailableId(mAveParentSite.SPSite.ID, mAveSPList.SPList.ID);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Test ")]/// <summary>
        /// 在该方法中处理AveSPListItem需要单独处理的DocData相关设置，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的DocData处理在AveSPItem对应的ProcessPreDocDataCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreDocDataCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreDocDataCondition(allDocData);
            if (allDocData.ContainsKey("DoclibRowId"))
            {
                object memberInfo = ParentList.ParentWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(mListItemInfo.OriginalRowId);
                if (this.ParentList.SPList.BaseTemplate == AveListTemplateType.UserInformation && memberInfo != null)  //different language setting
                {
                    mListItemInfo.OriginalRowId = ((AveSPMemberInfo)memberInfo).NewId;
                    mListItemInfo.Name = mListItemInfo.OriginalRowId + "_.000";
                    mListItemInfo.OriginalRowId = mListItemInfo.OriginalRowId;
                }
            }
            else
            {
                throw new AveWarningException(AveInternalResourceKey.Wrapper_Exception_Restore_NoRowIdForItem, mListItemInfo.Name);
            }
        }

        /// <summary>
        /// 在该方法中处理AveSPListItem需要单独处理的UserData相关设置，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的UserData处理在AveSPItem对应的ProcessPreUserDataCondtion中进行设置
        /// </summary>
        /// <param name="allUserData"></param>
        [Obsolete("Use ProcessPreUserAndJunctionDataCondition() instead")]
        internal override void ProcessPreUserDataCondition(Dictionary<string, object> allUserData)
        {
            base.ProcessPreUserDataCondition(allUserData);
            if (allUserData.ContainsKey("V4ConfirmedNote") && allUserData["V4ConfirmedNote"] != null)
            {
                string[] tmpdata = allUserData["V4ConfirmedNote"].ToString().Split(';');
                string userId = string.Empty;
                try
                {
                    userId = this.mAveSPList.SPList.ParentWeb.SiteUsers[tmpdata[1].TrimStart('#')].ID.ToString();
                    allUserData["V4ConfirmedNote"] = allUserData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                    allUserData["Confirmed"] = allUserData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Cannot find confirm user.\n error message:{0}", e));
                    //mLog.Warn("can not find confirm user ");
                    userId = this.mAveSPList.SPList.ParentWeb.Site.Owner.ID.ToString();
                    allUserData["V4ConfirmedNote"] = allUserData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                    allUserData["Confirmed"] = allUserData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                }
            }
            if (allUserData.ContainsKey("#tp_GUID") && (mListItemInfo.tp_Guid == null || mListItemInfo.tp_Guid == Guid.Empty))
            {
                mListItemInfo.tp_Guid = new Guid(allUserData["#tp_GUID"].ToString());
            }
        }

        internal override void ProcessPreUserAndJunctionDataCondition(Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {
            base.ProcessPreUserAndJunctionDataCondition(allUserData, junctionData);
            if (allUserData.ContainsKey("V4ConfirmedNote") && allUserData["V4ConfirmedNote"] != null)
            {
                string[] tmpdata = allUserData["V4ConfirmedNote"].ToString().Split(';');
                string userId = string.Empty;
                try
                {
                    userId = this.mAveSPList.SPList.ParentWeb.SiteUsers[tmpdata[1].TrimStart('#')].ID.ToString();
                    allUserData["V4ConfirmedNote"] = allUserData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                    allUserData["Confirmed"] = allUserData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Cannot find confirm user.\n error message:{0}", e));
                    //mLog.Warn("can not find confirm user ");
                    userId = this.mAveSPList.SPList.ParentWeb.Site.Owner.ID.ToString();
                    allUserData["V4ConfirmedNote"] = allUserData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                    allUserData["Confirmed"] = allUserData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                }
            }
            if (allUserData.ContainsKey("#tp_GUID") && (mListItemInfo.tp_Guid == null || mListItemInfo.tp_Guid == Guid.Empty))
            {
                mListItemInfo.tp_Guid = new Guid(allUserData["#tp_GUID"].ToString());
            }
        }

        /// <summary>
        /// 在该方法中处理AveSPListItem需要单独处理的Setting设置(和allDocData，allUserData无关)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的setting设置在AveSPItem对应的ProcessPreSettingCondition中进行设置
        /// </summary>
        internal override void ProcessPreSettingCondition()
        {
            base.ProcessPreSettingCondition();
            RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mParentFolder.RestoringItem.Init(mListItemInfo.Name, CheckRestoreOption(IsNewCreated || mAveSPList.IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            mListItemInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
            mListItemInfo.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME = RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME;
            mListItemInfo.RestoringItem = mParentFolder.RestoringItem;
            mListItemInfo.SettingInfo.CheckConflictByUniqueId = RestoreOption.mAveItemRestoreOption.CheckConflictByUniqueId;
            if (RestoreOption.mAveItemRestoreOption.MOVE_SOURCE_ITEM_TO_FOLDER)
            {
                //一定发生了冲突
                var value = Guid.Empty;
                if (mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromItemGuidForReplicatorConflict(mListItemInfo.tp_Guid, out value))
                {
                    mListItemInfo.tp_Guid = value;
                }
                else
                {
                    Guid guid = Guid.NewGuid();
                    mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddItemGuidMapping(mListItemInfo.tp_Guid, guid);
                    mListItemInfo.tp_Guid = guid;
                }
            }
            if (mAveSPList.IsTaxonomyList)
            {
                //return AveRestoreResult.Omit;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            mListItemInfo.SettingInfo.OverWriteByModifiedTime = CheckRestoreOption(AveRestoreMode.OverWriteByModifiedTime);
        }

        /// <summary>
        /// 在该方法中处理AveSPListItem需要单独处理的MetaInfo(包括UnVersionedMetaInfo)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的MetaInfo设置在AveSPItem对应的ProcessPreMetaInfoCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreMetaInfoCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreMetaInfoCondition(allDocData);
        }

        internal void ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<AveListTitleMappingInfo> listTitleMappings)
        {
            this.SetRestoreOption(mRestoreOption);
            ProcessPreDocDataCondition(allDocData);
            ProcessPreSettingCondition();
            ProcessDifferentLists(allDocData, allUserData);
            ProcessPreUserDataCondition(allUserData);
        }

        internal void ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {
            this.SetRestoreOption(mRestoreOption);
            ProcessPreDocDataCondition(allDocData);
            ProcessPreSettingCondition();
            ProcessDifferentLists(allDocData, allUserData);
            ProcessPreUserAndJunctionDataCondition(allUserData, junctionData);
        }

        private void ProcessDifferentLists(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            IAveList spList = mParentFolder.ParentList.SPList;
            AveSPSite parentSite = mParentFolder.ParentList.ParentWeb.ParentSite;
            if (spList.BaseTemplate == AveListTemplateType.Meetings)
            {
                ProcessMeetings(userData, parentSite);
            }
            else if (spList.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                ProcessDiscussionBoard(data, userData);
            }
            //add by adrian
            else if (spList.BaseTemplate == AveListTemplateType.ThemeCatalog)
            {
                //ProcessThemeCatalog(userData, parentSite);
            }
            else if (spList.BaseTemplate == AveListTemplateType.MicroFeed)
            {
                ProcessMicroFeed(userData, spList, parentSite);
            }
            else if ((int)spList.BaseTemplate == 550)
            {
                ProcessSocialList(data,userData, spList, parentSite);
            }
            else if ((int)spList.BaseTemplate == 160)
            {
                ProcessAccessRequests(data, userData);
            }
            else if (spList.Title.Equals("Content Organizer Rules", StringComparison.OrdinalIgnoreCase))
            {
                if (userData.ContainsKey("RoutingTargetPath") && this.ListTitleMappingInfo !=null)
                {
                    string sourceUrl = userData["RoutingTargetPath"].ToString();
                    string webUrl = AveUrlUtility.GetServerRelativeUrl(ParentList.ParentWeb.WebInfo.Url);

                    Dictionary<string, string> titleMappingValueInfo = new Dictionary<string, string>();
                    foreach (AveListTitleMappingInfo titleMapping in this.ListTitleMappingInfo)
                    {
                        foreach (AveListTitleMappingValueInfo titleMappingValue in titleMapping.ListTitleMappingValueInfo)
                        {
                            titleMappingValueInfo.Add(webUrl + '/' + titleMappingValue.SourceName, webUrl + '/' + titleMappingValue.DestinationName);
                        }
                    }

                    userData["RoutingTargetPath"] = AveReplaceProcessor.UrlReplace(sourceUrl, titleMappingValueInfo, new ReplaceOption(true, true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                }
            }
        }
        private void ProcessAccessRequests(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            //在此函数中，会检测所请求的List或者Item是否存在，若不存在则抛出异常，不必再在AveItem中对Url进行检查。
            if (userData.ContainsKey("#tp_isARLListItemTerminated") && userData["#tp_isARLListItemTerminated"] != null && (bool)userData["#tp_isARLListItemTerminated"])
            {
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            //string[] repFields = new string[] { "RequestedListItemId", "RequestedListId", "InheritingRequestedWebId"/*在微软API中该属性直接赋值为Empty*/, "RequestedWebId", "RequestedObjectUrl", "RequestedObjectTitle", "RequestedByUserId", "RequestedForUserId", "PermissionLevelRequested" };
            IAveWeb web = SPWeb;
            if (userData.ContainsKey("RequestedObjectUrl") && userData["RequestedObjectUrl"] != null)
            {
                string relativeUrl = (string)userData["RequestedObjectUrl"];
                relativeUrl = AveReplaceProcessor.UrlReplace(relativeUrl, mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                string targetUrl = relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? relativeUrl : this.SPWeb.Site.MakeFullUrl(relativeUrl);
                userData["RequestedObjectUrl"] = (string)targetUrl;
                if (userData.ContainsKey("RequestedListItemId") && userData["RequestedListItemId"] != null && (Guid)userData["RequestedListItemId"] != Guid.Empty)
                {
                    IAveListItem targetItem = null;
                    targetItem = web.GetListItem(relativeUrl);
                    if (targetItem == null)
                    {
                        log.Warn("An error occurred while restore a listItem in the AccessRequest List.Can't find the associated item,item url:{0}.Please check whether or not the item is in the web.", relativeUrl);
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                    userData["RequestedListItemId"] = targetItem.UniqueId;
                    userData["RequestedListId"] = targetItem.ParentList.ID;
                    userData["RequestedWebId"] = web.ID;
                }
                else if (userData.ContainsKey("RequestedListId") && userData["RequestedListId"] != null && (Guid)userData["RequestedListId"] != Guid.Empty)
                {
                    IAveList tempList;
                    try
                    {
                        tempList = web.GetList(targetUrl);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while restore a listItem in the AccessRequest List.Can't find the associated list,list url:{0}.Please check whether or not the list is in the web.Error:{1}", targetUrl, e);
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                    userData["RequestedListId"] = tempList.ID;
                    userData["RequestedWebId"] = web.ID;
                }
                else
                {
                    userData["RequestedWebId"] = web.ID;
                }
            }
            if (userData.ContainsKey("PermissionLevelRequested") && userData.ContainsKey("PermissionType") && (userData["PermissionType"] as string) == "SharePoint Group")
            {
                int groupId = (int)userData["PermissionLevelRequested"];
                groupId = mAveParentSite.SPMembers.FindMemberId(groupId);
                if (groupId <= 0)
                {
                    groupId = web.Groups[0].ID;
                }
                userData["PermissionLevelRequested"] = (int)groupId;
            }

            int reqBy = 0;
            int reqFor = 0;
            if (userData.ContainsKey("RequestedByUserId") && userData["RequestedByUserId"] != null)
            {
                reqBy = (int)userData["RequestedByUserId"];
                reqBy = mAveParentSite.SPMembers.FindMemberId(reqBy);
            }
            if (userData.ContainsKey("RequestedForUserId") && userData["RequestedForUserId"] != null)
            {
                reqFor = (int)userData["RequestedForUserId"];
                reqFor = mAveParentSite.SPMembers.FindMemberId(reqFor);
            }
            if (reqBy <= 0 || reqFor <= 0)
            {
                throw new AveRestoreException(AveRestoreResult.Failed, AveInternalResourceKey.Wrapper_Exception_Restore_CanNotGetRequestUserInSourceSite, (int)userData["RequestedByUserId"], reqFor == 0 ? reqFor : (int)userData["RequestedForUserId"]);
            }
            else
            {
                userData["RequestedByUserId"] = (int)reqBy;
                userData["RequestedForUserId"] = (int)reqFor;
            }
        }


        private void ProcessMicroFeed(Dictionary<string, object> userData, IAveList list, AveSPSite parentSite)
        {
            string sourceListUrl = parentSite.MappingManager.SiteMappingManager.SourceSiteInfo.Url.TrimEnd('/');
            string destListUrl = parentSite.SPSite.Url.TrimEnd('/');
            //replace url in ContentData
            if (userData.ContainsKey("ContentData"))
            {
                string contentData = (string)userData["ContentData"];
                if (!string.IsNullOrEmpty(contentData) && contentData.IndexOf(sourceListUrl, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    userData["ContentData"] = contentData.Replace(sourceListUrl, destListUrl);
                }
            }
            //replace RootPostID
            if (userData.ContainsKey("RootPostID"))
            {
                int rootPostID = (int)userData["RootPostID"];
                int newId = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mListItemInfo.ListId, rootPostID);
                if (newId > 0)
                {
                    userData["RootPostID"] = newId;
                }
            }
            //replace RootPostOwnerID
            if (userData.ContainsKey("RootPostOwnerID"))
            {
                string rootPostOwnerID = (string)userData["RootPostOwnerID"];
                string newID = rootPostOwnerID;
                if (!string.IsNullOrEmpty(rootPostOwnerID) && rootPostOwnerID.StartsWith("8.", StringComparison.OrdinalIgnoreCase))
                {
                    string[] strArray = rootPostOwnerID.Split(new char[] { '.' });
                    if (strArray.Length == 5)
                    {
                        Guid siteId = new Guid(strArray[1]);
                        Guid webId = new Guid(strArray[2]);
                        Guid uniqueId = new Guid(strArray[3]);
                        if (siteId != parentSite.SPSite.ID)
                        {
                            newID = newID.Replace(strArray[1], parentSite.SPSite.ID.ToString("N"));
                        }
                        if (parentSite.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(webId))
                        {
                            newID = newID.Replace(strArray[2], parentSite.MappingManager.SiteMappingManager.WebIDMapping[webId].ToString("N"));
                        }
                        userData["RootPostOwnerID"] = newID;
                    }
                }
                if (userData.ContainsKey("RefRoot"))
                {
                    mAveSPList.PostMicroFeedItem.Add(mListItemInfo.OriginalRowId);
                }
                if (userData.ContainsKey("RefReply"))
                {
                    mAveSPList.PostMicroFeedItem.Add(mListItemInfo.OriginalRowId);
                }
            }
        }

        /// <summary>
        /// 处理Social list下的item信息的方法
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="list"></param>
        /// <param name="parentSite"></param>
        private void ProcessSocialList(Dictionary<string, object> docData,Dictionary<string, object> userData, IAveList list, AveSPSite parentSite)
        {
            switch (parentSite.ObjectModelFactory.ContextKind)
            {
                case AveContextKind.ClientObjectModel:
                    {//365目前没有API还原follow的逻辑，需要加到缓存里，在site post action处理其中的webid等相关信息的替换
                        parentSite.AddPostUpdateSocialItem(list.ParentWeb.ID, list.ID, mListItemInfo.OriginalRowId);
                        break;
                    }
                case AveContextKind.Server19ObjectModel:
                case AveContextKind.Server16ObjectModel:
                case AveContextKind.Server13ObjectModel:
                    {
                        //对于local 13，保持merge poc的逻辑,wrapper内部不做替换
                        if (docData != null && docData.ContainsKey("DirName"))
                        {
                            string dirName = (string)docData["DirName"];
                            if (!string.IsNullOrEmpty(dirName))
                            {
                                if (dirName.EndsWith("Social/Private/FollowedSites", StringComparison.OrdinalIgnoreCase))
                                {
                                    AveRestoreResult result = RestoreSocialListItem(userData, AveOSocialActorType.Site);
                                    throw new AveRestoreException(result,"");
                                }
                                else if (dirName.EndsWith("Social/Private/FollowedDocuments", StringComparison.OrdinalIgnoreCase))
                                {
                                    AveRestoreResult result = RestoreSocialListItem(userData, AveOSocialActorType.Document);
                                    throw new AveRestoreException(result, "");
                                }
                            }
                        }
                        break;
                    }
                default:
                    break;
            }        
        }

        private void ProcessThemeCatalog(Dictionary<string, object> userData, AveSPSite parentSite)
        {
            if (userData.ContainsKey("MasterPageUrl"))
            {
                userData["MasterPageUrl"] = AveReplaceProcessor.UrlReplace(userData["MasterPageUrl"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("MasterPageUrl#2"))
            {
                userData["MasterPageUrl#2"] = AveReplaceProcessor.UrlReplace(userData["MasterPageUrl#2"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("ThemeUrl#2"))
            {
                userData["ThemeUrl#2"] = AveReplaceProcessor.UrlReplace(userData["ThemeUrl#2"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("ThemeUrl"))
            {
                userData["ThemeUrl"] = AveReplaceProcessor.UrlReplace(userData["ThemeUrl"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("ImageUrl"))
            {
                userData["ImageUrl"] = AveReplaceProcessor.UrlReplace(userData["ImageUrl"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("ImageUrl#2"))
            {
                userData["ImageUrl#2"] = AveReplaceProcessor.UrlReplace(userData["ImageUrl#2"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("FontSchemeUrl"))
            {
                userData["FontSchemeUrl"] = AveReplaceProcessor.UrlReplace(userData["FontSchemeUrl"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("FontSchemeUrl#2"))
            {
                userData["FontSchemeUrl#2"] = AveReplaceProcessor.UrlReplace(userData["FontSchemeUrl#2"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
        }

        private void ProcessDiscussionBoard(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            int newId = 0;
            if (userData.ContainsKey("#ThreadIndexParentId") && (int)userData["#ThreadIndexParentId"] > 0)
            {
                int parentId = (int)userData["#ThreadIndexParentId"];
                newId = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mListItemInfo.ListId, parentId);
            }
            try
            {
                int parentFolderId = 0;
                if (mParentFolder.SPFolder.Item != null)
                {
                    parentFolderId = mParentFolder.SPFolder.Item.ID;
                }
                if (userData.ContainsKey("ParentFolderId"))
                {
                    userData["ParentFolderId"] = parentFolderId;
                }
                if (newId == parentFolderId || newId <= 0)
                {
                    data["DiscussionTopic"] = parentFolderId;
                }
                else
                {
                    data["ParentThreadId"] = newId;
                }
                if (userData.ContainsKey("#tp_ThreadIndex") && userData.ContainsKey("#tp_GUID") && !userData.ContainsKey("MessageId"))
                {//对于sp07-10，07端userdata中不包含MessageId值，导致在判断冲突的时候没有走到reply特有的冲突判断逻辑中，在此将此属性根据该item的guid给补上，保证走到正确的逻辑中
                    userData["MessageId"] = "<" + userData["#tp_GUID"].ToString().Replace("-", "") + "@SharePoint>";
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserDataFromDiscussionError, e.ToString());
            }
        }

        private void ProcessMeetings(Dictionary<string, object> userData, AveSPSite parentSite)
        {
            if (userData.ContainsKey("Organizer"))
            {
                int principalId = (int)userData["Organizer"];
                mListItemInfo.Extension.PrincipalId = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(principalId);
            }
            if (userData.ContainsKey("EventUrl") && userData.ContainsKey("EventUrl#2"))
            {
                mListItemInfo.Extension.FieldUrlValue = AveReplaceProcessor.UrlReplace(userData["EventUrl"].ToString(), parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
            }
            if (userData.ContainsKey("EventUID"))
            {
                if (userData.ContainsKey("EventType") && (userData["EventType"].ToString().Equals("1") || userData["EventType"].ToString().Equals("0")))
                {
                    if (userData.ContainsKey("EventUrl"))
                    {
                        string sourceUrl = userData["EventUrl"].ToString();
                        mListItemInfo.Extension.DestUrl = AveReplaceProcessor.UrlReplace(sourceUrl, parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl);
                    }
                }
            }
            if(userData.ContainsKey("EventType"))
            {
                var eventType = (int)userData["EventType"];
                if (eventType == 1 && userData.ContainsKey("Duration"))
                {
                    ParentList.mMeetingSeriesDuration = (int)userData["Duration"];
                }
                else if (eventType == 2 || eventType == 3)
                {
                    if (!userData.ContainsKey("Duration") || (int)userData["Duration"] == -1)
                    {
                        userData["Duration"] = ParentList.mMeetingSeriesDuration;
                    }
                }
            }
        }

        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> allDataJunction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListItem.RestoreSelf"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;

                #region Only for WebDatabse System List


                if (mAveSPList.SPList.IsACCSRVSystemList())
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks && allUserData.ContainsKey("#tp_WorkflowInstanceID"))
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                if (allUserData == null || allUserData.Count == 0)
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                #endregion
                try
                {
                    ProcessPreCondition(allDocData, allUserData, allDataJunction);
                    ProcessVerifyItem();
                    mAveSPList.ListItemSerializer.SetReport(report);
                    if (this.ParentList.firstTime && this.ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                        this.ParentList.firstTime = false;
                    }
                    mAveSPList.ListItemSerializer.SetObjectData(mListItemInfo);
                    //mAveSPList.SPList.RestoreListItem(mListItemInfo, data, userData);
                }
                catch (AveSecurityTrimingException)
                {
                    result = AveRestoreResult.Failed;
                    throw;
                }
                catch (AveRestoreException ex)
                {
                    result = ex.Result;
                }
                finally
                {
                    //当ListItem还原失败抛出异常时，不应该影响list的setting
                    if (this.ListItemInfo.SettingInfo.LIST_SETTING_CHANGED)
                    {
                        ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                    }
                    if (this.ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                    }
                }
                ProcessPostCondition(result, allDocData, allUserData);
                return result;

            }

        }

        /// <summary>
        /// Personal Site的Social List中的Item与Follow功能相关，不能按照正常还原Item的逻辑进行还原，
        /// 需要通过Follow的Site（或Document）的Url使用相关API在目的端重新进行Follow，让SharePoint自动生成相应的Item
        /// </summary>
        /// <param name="allUserData"></param>
        /// <param name="actorType"></param>
        /// <returns></returns>
        private AveRestoreResult RestoreSocialListItem(Dictionary<string, object> allUserData, AveOSocialActorType actorType)
        {
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            try
            {
                object followedUrl;
                if (allUserData != null && allUserData.TryGetValue("Url", out followedUrl))
                {
                    SetThreadCurrentPrincipal();
                    followedUrl = AveReplaceProcessor.UrlReplace((string)followedUrl, mAveParentSite.MappingManager.SiteMappingManager.SiteUrlMapping,
                                             new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    allUserData["Url"] = followedUrl;
                    AveSocialActorInfo socialActorInfo = new AveSocialActorInfo()
                    {
                        ActorType = actorType,
                        ContentUri = new Uri((string)followedUrl),
                    };
                    mAveParentSite.SiteOwnerSocialFollowing.Restore(socialActorInfo);
                    var pinBehavior = mAveParentSite.BusinessBehavior.GetSocialFollowPinStateBehavior(this.mAveParentSite.SiteOwnerUserProfile.UserProfile, allUserData);
                    pinBehavior.Run();
                    return AveRestoreResult.Normal;
                }
                else
                {
                    log.Warn("Can not find the url of the social followed site or document.");
                    return AveRestoreResult.Failed;
                }
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while restoring the social list item, {0}.", ex);
                throw;
            }
            finally
            {
                Thread.CurrentPrincipal = currentPrincipal;
            }
        }
        
        private void SetThreadCurrentPrincipal()
        {
            string siteOwnerLoginName = mAveParentSite.SPSite.Owner.LoginName;
            log.Debug("Site owner login name: {0}.", siteOwnerLoginName);
            string prefix = "i:0#.w|";
            if (siteOwnerLoginName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                siteOwnerLoginName = siteOwnerLoginName.Substring(prefix.Length);
            }
            var identity = new GenericIdentity(siteOwnerLoginName);
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }


        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            return RestoreSelf(allDocData, allUserData, new List<Dictionary<string, object>>());
        }

        internal void ProcessPostCondition(AveRestoreResult result, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            //if (this.ParentList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
            //{
            //    if (this.ParentSite.MappingManager.SiteMappingManager.NeedDeletedUsers.ContainsKey(this.RowId))
            //    {
            //        this.ParentSite.MappingManager.SiteMappingManager.NeedDeletedUsers[this.RowId] = DateTime.Parse(allUserData["Modified"].ToString());
            //    }
            //}
            if (mListItemInfo.SettingInfo.LIST_SETTING_CHANGED)
            {
                ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
            }
            if (result > 0)
            {
                mAveSPList.AveFields.ResetNotUpdateLookupFieldValue(mListItemInfo.RowId);
                mAveSPList.AveFields.ResetNintexFormDataFieldValue(mListItemInfo.RowId);
                mAveSPList.AveFields.ResetNotUpdateUrlFieldValue(mListItemInfo.RowId);
                this.ResetRelatedItemsFieldValue(mListItemInfo.RowId);
                this.AddItemMapping(mListItemInfo.OriginalRowId);
                //放在Server层处理，在添加完ListItem之后就会更新
                //if (SPListItem != null)
                //{
                //    mParentFolder.RestoringItem.ReSetItemName(SPListItem.ID.ToString() + "_.000");
                //}
            }

            #region ProcessHold

            Hashtable lockMetaInfo = new Hashtable();
            Dictionary<string, string> fileHoldValue = new Dictionary<string, string>();

            try
            {
                if ((allUserData.ContainsKey("_vti_ItemHoldRecordStatus")) && (!string.Equals(allUserData["_vti_ItemHoldRecordStatus"].ToString(), "0", StringComparison.OrdinalIgnoreCase)) && allDocData.ContainsKey("MetaInfo"))
                {
                    var dataMetaInfo = (byte[])allDocData["MetaInfo"];
                    var itemHoldRecord = this.GetHoldRecord(lockMetaInfo, dataMetaInfo, allUserData);
                    if (itemHoldRecord != null)
                    {
                        mAveSPList.ParentWeb.ParentSite.AddUnRestoreItemHoldRecordInfo(mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID, SPListItem.ID, itemHoldRecord);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while getting hold and declared record information. Error Message:{0} ", ex);
            }
            #endregion
        }

        /*
         * 这个方法以后需要去掉，对于ListMapping的情况，需要联系CM重新考虑和实现 
         */
        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<AveListTitleMappingInfo> listTitleMappings)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListItem.RestoreSelf"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;

                #region Only for WebDatabse System List


                if (mAveSPList.SPList.IsACCSRVSystemList())
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks && allUserData.ContainsKey("#tp_WorkflowInstanceID"))
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }
                if (allUserData == null || allUserData.Count == 0)
                {
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                #endregion
                try
                {
                    ProcessPreCondition(allDocData, allUserData, listTitleMappings);
                    ProcessVerifyItem();
                    if (this.ParentList.firstTime && this.ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                        this.ParentList.firstTime = false;
                    }
                    mAveSPList.ListItemSerializer.SetReport(report);
                    mAveSPList.ListItemSerializer.SetObjectData(mListItemInfo);
                    //mAveSPList.SPList.RestoreListItem(mListItemInfo, data, userData);
                }
                catch (AveSecurityTrimingException)
                {
                    result = AveRestoreResult.Failed;
                    throw;
                }
                catch (AveRestoreException ex)
                {
                    result = ex.Result;
                }
                finally
                {
                    //当ListItem还原失败抛出异常时，不应该影响list的setting
                    if (this.ListItemInfo.SettingInfo.LIST_SETTING_CHANGED)
                    {
                        ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                    }
                    if (this.ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                    }
                }
                ProcessPostCondition(result, allDocData, allUserData);
                return result;

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="forceRestore">if false, only restore user in mappings</param>
        public bool RestoreUserInfo(Dictionary<string, object> userData, bool forceRestore)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListItemUserInfo"))
            {
                if (!forceRestore && !NeedRestoreUserInfoItem(userData))
                {
                    return false;
                }
                if (!userData.ContainsKey("Name"))
                {
                    return false;
                }
                if (userData.ContainsKey("#tp_IsCurrent"))
                {
                    if (!Convert.ToBoolean(userData["#tp_IsCurrent"]))
                    {
                        return false;
                    }
                }
                int newId = -1;
                int originalRowId = -1;
                string name = userData["Name"].ToString();
                userData.Remove("Name");
                if(name.Equals("SHAREPOINT\\system",StringComparison.OrdinalIgnoreCase)&& userData.ContainsKey("Title"))
                {
                    userData.Remove("Title");
                }
                if (userData.ContainsKey("#tp_ID") && Int32.TryParse(userData["#tp_ID"].ToString(), out originalRowId))
                {
                    var pricipal = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMember(originalRowId, true);
                    if (pricipal != null)
                    {
                        newId = pricipal.ID;
                        var restoredName = pricipal.LoginName;
                        try
                        {
                            this.SPListItem = mAveSPList.SPList.GetItemById(newId);
                            this.InitBySPListItem(this.SPListItem);
                            Dictionary<string, object> fieldData = mParentFolder.ParentList.AveFields.GetFieldValues(string.Empty, this.RowId, 512, userData, true);
                            if (forceRestore || mAveSPList.ParentWeb.ParentSite.SPMembers.AllGroups.Contains(name.ToLower(CultureInfo.InvariantCulture)) || name.Equals(restoredName, StringComparison.OrdinalIgnoreCase))
                            {
                                this.UpdateFields(fieldData, mListItemInfo);
                            }
                            else
                            {
                                //ADO-144425:replicator需要比对userinformation listItem的modifyTime属性,所以当mapping前后name不同时，只更新Modified属性。
                                object modifyTimeField;
                                if (fieldData.TryGetValue("Modified", out modifyTimeField))
                                {
                                    Dictionary<string, object> modifyTimeFieldData = new Dictionary<string, object>();
                                    modifyTimeFieldData["Modified"] = modifyTimeField;
                                    this.UpdateFields(modifyTimeFieldData, mListItemInfo);
                                }
                            }
                            if (userData.ContainsKey("IsActive") && SPListItem["IsActive"] != null)
                            {
                                if (!SPListItem["IsActive"].ToString().Equals(userData["IsActive"].ToString(), StringComparison.OrdinalIgnoreCase))
                                {
                                    SPListItem["IsActive"] = userData["IsActive"];
                                    SPListItem.SystemUpdate(false);
                                    log.Debug("Restore job has changed the user information column: IsActive, user name:{0}, IsActive: {1}", name, userData["IsActive"]);
                                }
                            }
                        }
                        catch (Exception ep)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Restore user item failed. User Name:{0}, User ID:{1}, Reason:{2}", name, newId, ep.ToString()));
                            return false;
                        }
                        if (userData.ContainsKey("#tp_ID") && Int32.TryParse(userData["#tp_ID"].ToString(), out originalRowId))
                        {
                            this.AddItemMapping(originalRowId);
                        }
                        
                        return true;
                    }
                }
                log.Log(AveLogLevel.INFO, "cannot get user item, username:{0}, original id:{1}", name, originalRowId);
                return false;

            }

        }

        public bool RestoreUserInfo(Dictionary<string, object> userData)
        {
            return RestoreUserInfo(userData, true);
        }

        private bool NeedRestoreUserInfoItem(Dictionary<string, object> userData)
        {
            if (userData.ContainsKey("#tp_ID"))
            {
                Object obj = this.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping((int)userData["#tp_ID"]);
                //obj==null没有在Mapping中，(obj as AveSPMemberInfo) == null在Mapping中但是没有还原过。
                if (obj == null || (obj as AveSPMemberInfo) == null || (!this.ParentSite.SPMembers.DefaultOption.NeedDeleteUser && (bool)userData["Deleted"]))
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateFields(Dictionary<string, object> fieldData, AveBaseItemInfo info)
        {
            mAveItem.SetReport(report);
            mAveItem.UpdateFields(fieldData, info);
        }

        public bool IsWorkflowTask(Dictionary<string, object> userData)
        {
            bool isWorkflowInstance = false;
            try
            {
                if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks)
                {
                    if (userData != null && userData.ContainsKey("#tp_ContentTypeId"))
                    {//0x01080100C9C9515DE4E24001905074F980F93160
                        byte[] id = userData["#tp_ContentTypeId"] as byte[];
                        string contentTypeId = AveConvert.ConvertByteToContentTypeId(id).ToString();

                        if ((!string.IsNullOrEmpty(contentTypeId)) && contentTypeId.StartsWith("0x010801", StringComparison.OrdinalIgnoreCase))
                        {
                            isWorkflowInstance = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex.ToString());
            }
            return isWorkflowInstance;
        }

        #region Obsolete method
        [Obsolete("no use now, will remove later")]
        public bool DestinationExist()
        {
            bool isExist = false;
            try
            {
                int originalId = Convert.ToInt32(mListItemInfo.Name.Substring(0, mListItemInfo.Name.Length - "_.000".Length));
                isExist = this.mAveSPList.SPList.CheckItemIsExist(originalId);
            }
            catch (Exception e)
            {
                log.Warn("Check the item{0} exist in destination with exception:{1}", mListItemInfo.Name, e.ToString());
            }
            return isExist;
        }
        [Obsolete("no use now, will remove later")]
        public IAveListItem GetCurrentSPListItem(Dictionary<string, object> data)
        {
            IAveListItem current = null;
            try
            {
                int originalRowid = 0;
                if (data.ContainsKey("DoclibRowId"))
                {
                    originalRowid = Convert.ToInt32(data["DoclibRowId"]);
                }
                object memberInfo = ParentList.ParentWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(originalRowid);
                if (this.ParentList.SPList.Title == "User Information List" && memberInfo != null)  //different language setting
                {
                    originalRowid = ((AveSPMemberInfo)memberInfo).NewId;
                }
                int tempRowId = originalRowid;
                tempRowId = mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mAveSPList.SPList.ID, tempRowId, tempRowId);
                current = mAveSPList.SPList.GetItemById(tempRowId);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetCurrentSPListItemFailed, ex);
            }
            return current;
        }
        [Obsolete("no use now, will remove later")]
        private IAveListItem GetListItem(string name, int id)
        {
            try
            {
                return mAveSPList.SPList.GetItemById(id);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetListItemFailed, e);
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Restore ListItem
        /// 
        /// 这个是新加的接口,外围请暂时不要调用
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="spListItemRestoreOption"></param>
        /// <returns></returns>
        //public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPListItemRestoreOption spListItemRestoreOption)
        //{
        //    if (restoreStream == null)
        //    {
        //        throw new ArgumentNullException("restoreStream");
        //    }

        //    if (spListItemRestoreOption == null)
        //    {
        //        throw new ArgumentNullException("spListItemRestoreOption");
        //    }

        //    var restoreReport = new SPFileRestoreReport();

        //    using (
        //        WrapperStopwatch.CreateInstance(spListItemRestoreOption.IncludePerformanceDetails,
        //                                        restoreReport.UpdateTimeUsage))
        //    {
        //        var listItemRestoreDto = new AveListItemRestoreHelper.AveSPListItemRestoreDto()
        //        {
        //            SPListItem = this,
        //            RestoreStream = restoreStream,
        //            SPListItemRestoreOption = spListItemRestoreOption,
        //        };

        //        AveMetadata metadata = null;

        //        while ((metadata = restoreStream.ReadMetadata()) != null)
        //        {
        //            listItemRestoreDto.Metadata = metadata;
        //            restoreReport.Add(metadata.MetadataType, AveListItemRestoreHelper.HandleMetadata(listItemRestoreDto));
        //        }
        //    }

        //    return restoreReport;
        //}

        /// <summary>
        /// Restore Document Metadata Dto
        /// </summary>
        /// <param name="fileRestoreOption"></param>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreStream"></param>
        /// <returns></returns>
        //internal MetadataRestoreDetails RestoreListItemMetadataDto(IAveRestoreStream restoreStream, SPListItemRestoreOption listITemRestoreOption, SPListItemMetadataDto listItemMetadataDto)
        //{
        //    var restoreDetails = new MetadataRestoreDetails();

        //    listITemRestoreOption.ToAveRestoreOption(this.mRestoreOption);
        //    //this.SetStream(restoreStream);
        //    this.mAveParentSite.RestoreUser(listItemMetadataDto.UserCache);
        //    this.mAveParentSite.RestoreGroup(listItemMetadataDto.GroupCache);
        //    this.mAveParentSite.RestoreMetadataInfo(listItemMetadataDto.MetadataInfo);
        //    this.VerifyItemMetadataDependency(listItemMetadataDto, listITemRestoreOption);
        //    this.RestoreListItem(listItemMetadataDto);
        //    this.RestoreLookupFieldGuidValue(listItemMetadataDto.ItemTPGUIDofLookupValue);

        //    // RestoreUser，RestoreGroup的reports
        //    restoreDetails.AnalyzeReport(this.mAveParentSite.SPMembers.GetReport());
        //    // RestoreMetadataInfo的reports
        //    restoreDetails.AnalyzeReport(this.mAveParentSite.MetadataService.GetReport());
        //    // RestoreListItem，RestoreLookupFieldGuidValue的report
        //    restoreDetails.AnalyzeReport(this.GetReport());
        //    return restoreDetails;
        //}

        /// <summary>
        /// Restore List Item
        /// </summary>
        /// <param name="listItemMetadataDto"></param>
        private void RestoreListItem(SPListItemMetadataDto listItemMetadataDto)
        {
            this.RestoreSelf(listItemMetadataDto.DocInfo_Old, listItemMetadataDto.UserDataInfo);
        }

        public IAveListItem Item
        {
            get { return mAveItem.ListItem; }
        }
    }

    //public class AveSPListItemV1 : AveSPListItem, ISPListItemImport
    //{
    //    private bool alertRestored = false;

    //    public AveSPListItemV1(AveSPFolder aveFolder, string name)
    //        : base(aveFolder, name) { }
    //    public AveSPListItemV1(AveSPFolder aveFolder, string name, int rowId)
    //        : base(aveFolder, name, rowId) { }

    //    /// <summary>
    //    /// Restore文件
    //    /// 
    //    /// 这个是新加的接口,外围暂时请不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spListItemRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPListItemRestoreOption spListItemRestoreOption)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }
    //        if (spListItemRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spListItemRestoreOption");
    //        }

    //        SPFileRestoreReport restoreReport = new SPFileRestoreReport();
    //        #region User Information List & Archiver MicroFeed item restore
    //        var specialAction = GetActionForSpecialList(spListItemRestoreOption.ArchiverRestoreMicroFeed);
    //        if (specialAction != null)
    //        {
    //            using (WrapperStopwatch.CreateInstance(spListItemRestoreOption.IncludePerformanceDetails, restoreReport.UpdateTimeUsage))
    //            {
    //                specialAction(restoreStream);
    //            }
    //            return restoreReport;
    //        }
    //        #endregion
    //        using (WrapperStopwatch.CreateInstance(spListItemRestoreOption.IncludePerformanceDetails, restoreReport.UpdateTimeUsage))
    //        {
    //            this.PreRestore(restoreStream, spListItemRestoreOption.FilterUserInfo, spListItemRestoreOption.FilterGroupInfo);
    //            AveMetadata metadata = null;

    //            while ((metadata = restoreStream.ReadMetadata()) != null)
    //            {
    //                var action = GetAction(metadata.MetadataType);
    //                if (action != null)
    //                {
    //                    var metadataRestoreReport = new MetadataRestoreReport(metadata.MetadataType);
    //                    using (WrapperStopwatch.CreateInstance(spListItemRestoreOption.IncludePerformanceDetails, metadataRestoreReport.AddTimeUsage))
    //                    {
    //                        action(restoreStream, spListItemRestoreOption, metadata, metadataRestoreReport);
    //                    }
    //                    restoreReport.Add(metadata.MetadataType, metadataRestoreReport);
    //                }
    //                else
    //                {
    //                    WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "TODO:{0}", metadata.MetadataType.ToString());
    //                }
    //            }
    //        }
    //        return restoreReport;
    //    }

    //    private Action<IAveRestoreStream> GetActionForSpecialList(bool isArchiverJob)
    //    {
    //        Action<IAveRestoreStream> action = null;

    //        switch (this.ParentList.SPList.BaseTemplate)
    //        {
    //            case AveListTemplateType.UserInformation:
    //                action = RestoreUserInfo;
    //                break;
    //            case AveListTemplateType.MicroFeed:
    //                if (isArchiverJob)
    //                {
    //                    action = RestorePostForArchiver;
    //                }
    //                break;
    //        }
    //        return action;
    //    }

    //    private Action<IAveRestoreStream, SPListItemRestoreOption, AveMetadata, MetadataRestoreReport> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPListItemRestoreOption, AveMetadata, MetadataRestoreReport> action = null;
    //        switch (metadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //            case AveMetadataType.ItemMetadataDto:
    //                action = RestoreListItemMetadataDto;
    //                break;
    //            case AveMetadataType.RoleAssignment:
    //                action = RestoreRoleAssignments;
    //                break;
    //            case AveMetadataType.RoleAssignmentsDto:
    //                action = RestoreRoleAssignmentsDto;
    //                break;
    //            //case AveMetadataType.RoleAssignmentInheritStatus:
    //            //    break;
    //            case AveMetadataType.AlertsDto:
    //                action = RestoreAlertsDto;
    //                break;
    //            case AveMetadataType.DocImmedSubscriptions:
    //            case AveMetadataType.DocSchedSubscriptions:
    //                action = RestoreItemAlert;
    //                break;
    //            case AveMetadataType.SocialTag:
    //                action = RestoreSocialTag;
    //                break;
    //            case AveMetadataType.SocialComment:
    //                action = RestoreSocialComment;
    //                break;
    //            case AveMetadataType.DocumentTagging:
    //                action = RestoreDocumentTag;
    //                break;
    //            case AveMetadataType.WorkflowInstance:
    //                action = RestoreWorkflowInstance;
    //                break;
    //            case AveMetadataType.WorkflowSchedule:
    //                action = RestoreWorkflowSchedule;
    //                break;
    //        }
    //        return action;
    //    }

    //    private void RestoreRoleAssignmentsDto(IAveRestoreStream restoreStream, SPListItemRestoreOption option, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        var roleAssignments = metadata.GetMetadata<AvePoint.Wrapper.Core.SPBackupDto.SPRoleAssignmentsDto>();

    //        if (option.RoleAssignmentsRestoreOption != null)
    //        {
    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments.RoleAssignmentInfos = option.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments.RoleAssignmentInfos);
    //                }

    //                if (option.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = !roleAssignments.IsInherit;
    //                }

    //                security.ParentSite.RestoreUser(roleAssignments.UserCache);
    //                security.ParentSite.RestoreGroup(roleAssignments.GroupCache);

    //                security.RestoreRoleAssignments(roleAssignments.RoleAssignmentInfos, option.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void EnsureItem(int rowId)
    //    {
    //        if (this.SPListItem == null && rowId > 0)
    //        {
    //            this.SPListItem = this.ParentList.SPList.GetItemById(rowId);
    //        }
    //    }

    //    private SPListItemMetadataDto GetSPListItemMetadataDto(IAveRestoreStream stream, AveMetadata metadata)
    //    {
    //        SPListItemMetadataDto itemDto = null;
    //        switch (metadata.MetadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //                itemDto = new SPListItemMetadataDto
    //                            {
    //                                DocInfo_Old = metadata.GetMetadata<Dictionary<string, object>>(),
    //                                MetadataInfo = stream.GetMetadataObj<List<AveTermStoreInfo>>(AveMetadataType.MetadataService),
    //                                UserDataInfo = stream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData),
    //                                DocDataJunction = stream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction),
    //                                ItemTPGUIDofLookupValue = stream.GetMetadataObj<Dictionary<string, string>>(AveMetadataType.LookupFieldGuidValue),
    //                                ItemUIVersionNums = stream.GetMetadataObj<List<int>>(AveMetadataType.DocVersions)
    //                            };
    //                break;
    //            case AveMetadataType.ItemMetadataDto:
    //                itemDto = metadata.GetMetadata<SPListItemMetadataDto>();
    //                break;
    //            default:
    //                WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "Invalid MetadataType to get SPListItemMetadataDto. MetadataType:{0}", metadata.MetadataType.ToString());
    //                break;
    //        }
    //        return itemDto;
    //    }

    //    /// <summary>
    //    /// 处理备份数据，如果外围有特殊处理，也在此执行
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="restoreOption"></param>
    //    /// <returns>是否成功处理</returns>
    //    private bool ProcessSourceMetadataInfo(SPListItemMetadataDto metaDto, SPListItemRestoreOption restoreOption)
    //    {
    //        if (metaDto == null)
    //        {
    //            throw new ArgumentNullException("metaDto");
    //        }
    //        if (restoreOption == null)
    //        {
    //            throw new ArgumentNullException("restoreOption");
    //        }
    //        else if (restoreOption.MetadataRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("MetadataRestoreOption");
    //        }

    //        this.mListItemInfo.VerifyItemMMSColumnValue = restoreOption.MetadataRestoreOption.VerifyDependency;
    //        this.mListItemInfo.KeepDefaultValue = restoreOption.MetadataRestoreOption.KeepColumnDefaultValue;
    //        this.mListItemInfo.KeepDestItemRowId = restoreOption.MetadataRestoreOption.KeepUniqueIdAndRowId;

    //        return AveDelegateExecutor.SafeExecuteFunc(restoreOption.ProcessListItemMetadataDto, metaDto);

    //    }

    //    private void RestoreListItemMetadataDto(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        var metadataDto = GetSPListItemMetadataDto(restoreStream, metadata);
    //        if (this.IsWorkflowTask(metadataDto.UserDataInfo))
    //        {
    //            WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, "Skip to restore workflow task list item");
    //        }
    //        var restoreAction = ProcessConflictCheck(restoreOption, metadataDto);
    //        ProcessConflictResultAction(restoreOption, restoreAction, metadataDto);
    //        //if (TryGetListItem(restoreOption, metadataDto))
    //        //{
    //        //    HandleConflictItem(restoreOption);
    //        //}
    //        ProcessSourceMetadataInfo(metadataDto, restoreOption);
    //        restoreReport.Details = this.RestoreListItemMetadataDto(restoreStream, restoreOption, metadataDto);
    //    }

    //    //private bool TryGetListItem(SPListItemRestoreOption option, SPListItemMetadataDto metadataDto)
    //    //{
    //    //    if (option.ConflictCheckOption == SPItemConflictCheckOption.None)
    //    //    {
    //    //        //reasonMsg = "No need to check conflict.";
    //    //        return this.Item != null;
    //    //    }
    //    //    else if (option.ConflictCheckOption == SPItemConflictCheckOption.CheckExist)
    //    //    {
    //    //        //获取IAveListItem 对象，如果不存在，则抛出异常
    //    //    }

    //    //    //正常的CheckConflict 逻辑

    //    //    return true;
    //    //}

    //    //private void HandleConflictItem(SPListItemRestoreOption option)
    //    //{
    //    //    SPItemRestoreAction action = GetItemRestoreAction(option);

    //    //    switch (action)
    //    //    {
    //    //        case SPItemRestoreAction.Skip:
    //    //            //TODO Log
    //    //            //OmitException
    //    //            throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Skip");
    //    //        case SPItemRestoreAction.Default:
    //    //            break;
    //    //    }
    //    //}

    //    //private SPItemRestoreAction GetItemRestoreAction(SPListItemRestoreOption option)
    //    //{
    //    //    SPItemRestoreAction action = SPItemRestoreAction.Skip;

    //    //    switch (option.ConflictHandleOption)
    //    //    {
    //    //        case SPItemConflictHandleOption.Skip:
    //    //            action = SPItemRestoreAction.Skip;
    //    //            break;
    //    //        case SPItemConflictHandleOption.Custom:
    //    //            if (option.ConflictHandleFunc == null)
    //    //            {
    //    //                //TODO Log;
    //    //                action = SPItemRestoreAction.Skip;
    //    //            }
    //    //            else
    //    //            {
    //    //                action = option.ConflictHandleFunc(this.SPListItem);
    //    //            }
    //    //            break;
    //    //        case SPItemConflictHandleOption.Overwrite:
    //    //            //TODO 删除的逻辑需要处理一下

    //    //            action = SPItemRestoreAction.Overwrite;
    //    //            break;
    //    //        default:
    //    //            action = SPItemRestoreAction.Default;
    //    //            break;
    //    //    }

    //    //    return action;
    //    //}

    //    /// <summary>
    //    /// conflict的选项
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="sourceData"></param>
    //    /// <returns></returns>
    //    private SPItemRestoreAction ProcessConflictCheck(SPListItemRestoreOption option, SPListItemMetadataDto sourceData)
    //    {
    //        if (option.ConflictOption == null)
    //        {
    //            throw new ArgumentNullException("ConflictOption");
    //        }

    //        var restoreAction = option.ConflictOption.NonConflictAction;

    //        if (option.ConflictOption.CheckOptions != null && option.ConflictOption.CheckOptions.Count > 0)
    //        {
    //            foreach (var item in option.ConflictOption.CheckOptions)
    //            {
    //                var conflict = ProcessConflictCheckOption(item, sourceData);

    //                if (option.ConflictOption.CustomConflictResultHandler != null)
    //                {
    //                    var processResult = option.ConflictOption.CustomConflictResultHandler(item, conflict);

    //                    if (!processResult.Item1)
    //                    {
    //                        restoreAction = processResult.Item2;
    //                        break;
    //                    }
    //                }
    //                else if (conflict)
    //                {
    //                    restoreAction = option.ConflictOption.ConflictAction;
    //                    break;
    //                }
    //            }
    //        }
    //        else if (option.ConflictOption.CustomConflictHandler != null)
    //        {
    //            restoreAction = option.ConflictOption.CustomConflictHandler(this.SPListItem);
    //        }

    //        return restoreAction;
    //    }

    //    private void ProcessConflictResultAction(SPListItemRestoreOption option, SPItemRestoreAction action, SPListItemMetadataDto metaDto)
    //    {
    //        switch (action)
    //        {
    //            case SPItemRestoreAction.Skip:
    //                //TODO Log
    //                //OmitException
    //                throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Skip");
    //            case SPItemRestoreAction.NewVersion:
    //                ParentFolder.RestoringItem.ResetNewItemValues(true, mBaseItemInfo.Name, mBaseItemInfo.Name);
    //                ProcessAppendNewVersion(metaDto, option);
    //                break;
    //            case SPItemRestoreAction.Default:
    //                ParentFolder.RestoringItem.ResetNewItemValues(true, mBaseItemInfo.Name, mBaseItemInfo.Name);
    //                break;
    //        }
    //    }

    //    /// <summary>
    //    /// 在目的端Version基础上，根据原端增长Version。
    //    /// </summary>
    //    /// <param name="metaDto"></param>
    //    private void ProcessAppendNewVersion(SPListItemMetadataDto metaDto, SPListItemRestoreOption option)
    //    {
    //        if (option.ProcessListItemMetadataDto == null && Item != null)
    //        {
    //            if (metaDto.DocInfo_Old != null && metaDto.DocInfo_Old.ContainsKey("UIVersion"))
    //            {
    //                int destVersion = (int)Item[AveBuiltInFieldId._UIVersion];
    //                metaDto.DocInfo_Old["UIVersion"] = destVersion + 512;
    //            }
    //            else
    //            {
    //                throw new ArgumentException("Can't find version info from metadata");
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Built-in check的机制
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="sourceData"></param>
    //    /// <returns></returns>
    //    protected bool ProcessConflictCheckOption(SPItemConflictCheckOption option, SPItemMetadataDto sourceData)
    //    {
    //        var conflict = false;

    //        switch (option)
    //        {
    //            case SPItemConflictCheckOption.CheckExist:
    //                conflict = this.SPListItem != null;
    //                break;
    //            case SPItemConflictCheckOption.CheckModifiedTime:
    //                {
    //                    var item = this.SPListItem;
    //                    if (item != null)
    //                    {
    //                        conflict = !item[AveBuiltInFieldId.Modified].Equals(((DateTime)sourceData.UserDataInfo["#tp_Modified"]).ToUniversalTime());
    //                    }
    //                    else
    //                    {
    //                        conflict = true;
    //                    }
    //                }
    //                break;
    //            case SPItemConflictCheckOption.CheckNewChanged:
    //                {
    //                    var item = this.SPListItem;
    //                    if (item != null)
    //                    {
    //                        conflict = ((DateTime)item[AveBuiltInFieldId.Modified]) < ((DateTime)(sourceData.UserDataInfo["#tp_Modified"])).ToUniversalTime();
    //                    }
    //                }
    //                break;
    //            case SPItemConflictCheckOption.CheckVersionNumber:
    //                {
    //                    var item = this.Item;
    //                    if (item != null)
    //                    {
    //                        conflict = (int)item["_UIVersion"] > ((int)(sourceData.UserDataInfo["#tp_UIVersion"]));
    //                    }
    //                }
    //                break;
    //            default:
    //                break;
    //        }

    //        return conflict;
    //    }

    //    private void RestoreRoleAssignments(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RoleAssignmentsRestoreOption != null)
    //        {
    //            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
    //            //EnsureItem(restoreOption.TargetItemRowId);
    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments = restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
    //                }

    //                security.RestoreRoleAssignments(roleAssignments, restoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());
    //                //security.SourceHasUniqueRoleAssignment = this.HasUniqueRoleAssignments;
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private Tuple<List<Dictionary<string, object>>, List<Dictionary<string, object>>> GetAllAlertsInfo(IAveRestoreStream restoreStream, AveMetadata metadata)
    //    {
    //        List<Dictionary<string, object>> immAlerts = new List<Dictionary<string, object>>();
    //        List<Dictionary<string, object>> schedAlerts = new List<Dictionary<string, object>>();
    //        try
    //        {
    //            if (metadata.MetadataType == AveMetadataType.DocImmedSubscriptions)
    //            {
    //                immAlerts = metadata.GetMetadata<List<Dictionary<string, object>>>();
    //                schedAlerts = restoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocSchedSubscriptions);
    //            }
    //            else if (metadata.MetadataType == AveMetadataType.DocSchedSubscriptions)
    //            {
    //                schedAlerts = metadata.GetMetadata<List<Dictionary<string, object>>>();
    //                immAlerts = restoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocImmedSubscriptions);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "Failed to get alert info. Error:{0}", ex.ToString());
    //        }
    //        return new Tuple<List<Dictionary<string, object>>, List<Dictionary<string, object>>>(immAlerts, schedAlerts);
    //    }

    //    private void RestoreAlertsDto(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        var alerts = metadata.GetMetadata<SPAlertsDto>();
    //        bool restoreImmed = alerts.ImmedSubscriptions != null && alerts.ImmedSubscriptions.Count > 0;
    //        bool restoreSched = alerts.SchedSubscriptions != null && alerts.SchedSubscriptions.Count > 0;
    //        if (restoreImmed || restoreSched)
    //        {
    //            this.ParentSite.RestoreUser(alerts.UserCache);
    //            using (AveSPAlert alert = AveSPAlert.CreateInstance(this))
    //            {
    //                if (restoreImmed)
    //                {
    //                    alert.RestoreAlerts(alerts.ImmedSubscriptions, false);
    //                }
    //                if (restoreSched)
    //                {
    //                    alert.RestoreAlerts(alerts.SchedSubscriptions, true);
    //                }
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreItemAlert(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (alertRestored)
    //        {
    //            return;
    //        }

    //        var alertInfos = GetAllAlertsInfo(restoreStream, metadata);
    //        if (alertInfos.Item1.Count == 0 && alertInfos.Item2.Count == 0)
    //        {
    //            return;
    //        }
    //        using (AveSPAlert alert = AveSPAlert.CreateInstance(this))
    //        {
    //            alert.RestoreAlerts(alertInfos.Item1, false);
    //            alert.RestoreAlerts(alertInfos.Item2, true);
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(alert.GetReport());
    //        }
    //        alertRestored = true;
    //    }

    //    private void RestoreSocialTag(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        using (AveSPSocialTag socialTags = new AveSPSocialTag(this.TagUrl, this.mAveParentSite))
    //        {
    //            socialTags.Restore(metadata.GetMetadata<List<AveSocialTagInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(socialTags.GetReport());
    //        }
    //    }

    //    private void RestoreSocialComment(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        using (AveSPSocialComment socialComment = new AveSPSocialComment(this.TagUrl, this.mAveParentSite))
    //        {
    //            socialComment.Restore(metadata.GetMetadata<List<AveSocialCommentInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(socialComment.GetReport());
    //        }
    //    }

    //    private void RestoreDocumentTag(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        using (AveDocumentTagging docTag = new AveDocumentTagging(this.TagUrl, this.mAveParentSite))
    //        {
    //            docTag.Restore(metadata.GetMetadata<List<AveDocumentTaggingInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(docTag.GetReport());
    //        }
    //    }

    //    private void RestoreWorkflowInstance(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.WorkflowRestoreOption.InstanceRestoreOption.NeedCheckRestoreOption && !this.CheckRestoreOption(this.IsNewCreated, AveRestoreMode.OverWrite))
    //        {
    //            return;
    //        }
    //        //for replicator
    //        //EnsureItem(restoreOption.TargetItemRowId);
    //        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //        WFConflictResolution wfResolution = WFConflictResolution.Instance;
    //        wfResolution.InstanceOption = WFInstanceConflictResolutionOption.OverwriteByModifiedTime;
    //        foreach (var unit in wfInfo)
    //        {
    //            wfResolution.RestoreInstanceData(unit, this);
    //        }
    //        if (wfInfo.Count > 0)//由于对象不一致，导致在还原workflow instance时list.update（UpdateListSettings）出错，现在增加list的reload操作，重新获取一下list对象
    //        {
    //            this.ParentFolder.ParentList.ReloadList();
    //        }
    //    }

    //    private void RestoreWorkflowSchedule(IAveRestoreStream restoreStream, SPListItemRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (this.CheckRestoreOption(this.IsNewCreated, AveRestoreMode.OverWrite))
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            IWFConflictResolution wfResolution = WFConflictResolution.Instance;
    //            foreach (var unit in wfInfo)
    //            {
    //                wfResolution.RestoreScheduleData(unit, this.SPListItem);
    //            }
    //        }
    //    }

    //    private void RestoreUserInfo(IAveRestoreStream stream)
    //    {
    //        Dictionary<string, object> userData = stream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData);
    //        if (userData.Count == 0)
    //        {
    //            return;
    //        }
    //        //user information list item 对应的user已经删除，还原反而会对目的端的user产生影响，所以过滤掉不必还原
    //        if (userData.ContainsKey("Deleted") && userData["Deleted"].ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
    //        {
    //            this.RestoreUserInfo(userData);
    //        }
    //    }

    //    private void RestorePostForArchiver(IAveRestoreStream stream)
    //    {
    //        AveMetadata feedMetadata = stream.TryReadMetadata(AveMetadataType.SingleSocialFeed);
    //        if (feedMetadata != null)
    //        {
    //            List<AveSocialFeedInfo> feeds = new List<AveSocialFeedInfo>();
    //            AveSocialFeedInfo archiverFeed = feedMetadata.GetMetadata<AveSocialFeedInfo>();
    //            feeds.Add(archiverFeed);
    //            if (this.ParentWeb.SPWeb.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
    //            {
    //                AveSPUserProfile userProfile = new AveSPUserProfile(this.ParentSite, false);
    //                AveUserProfileInfo profileInfo = new AveUserProfileInfo();
    //                profileInfo.LoginName = this.ParentSite.SourceSiteInfo.OwnerLogin;
    //                profileInfo.Feeds = feeds;
    //                userProfile.RestoreForArchiver(profileInfo);
    //            }
    //            else
    //            {
    //                using (AveSPSocialFeed socialFeed = new AveSPSocialFeed(this.ParentWeb))
    //                {
    //                    socialFeed.RestoreForArchiver(feeds);
    //                }
    //            }
    //        }
    //    }
    //}
    /// <summary>
    /// Document Restore Healper
    /// </summary>
    //static class AveListItemRestoreHelper
    //{
    //    /// <summary>
    //    /// 临时使用
    //    /// </summary>
    //    internal sealed class AveSPListItemRestoreDto
    //    {
    //        public AveSPListItem SPListItem;
    //        public IAveRestoreStream RestoreStream;// { get;set}
    //        public SPListItemRestoreOption SPListItemRestoreOption;
    //        public AveMetadata Metadata;
    //    }

    //    private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AveDocumentRestoreHelper));

    //    private static readonly Dictionary<AveMetadataType, RestoreAction<AveSPListItemRestoreDto, MetadataRestoreReport>> restoreActions = new Dictionary<AveMetadataType, RestoreAction<AveSPListItemRestoreDto, MetadataRestoreReport>>
    //            {
    //                {AveMetadataType.DocProperty, RestoreListItemProperty},
    //                {AveMetadataType.RoleAssignment, RestoreRoleAssignments},
    //                {AveMetadataType.RoleAssignmentInheritStatus, RestoreInheritance},
    //                {AveMetadataType.GroupCache, RestoreGroupCache},
    //                {AveMetadataType.UserCache, RestoreUserCache},
    //                {AveMetadataType.DocumentTagging, RestoreDocumentTagging},
    //                {AveMetadataType.DocImmedSubscriptions, RestoreDocImmedSubscriptions},
    //                {AveMetadataType.DocSchedSubscriptions, RestoreDocSchedSubscriptions},
    //                {AveMetadataType.WorkflowInstance, RestoreWorkflowInstance},
    //                {AveMetadataType.WorkflowSchedule, RestoreWorkflowSchedule},
    //                {AveMetadataType.SocialTag, RestoreSocialTag},
    //                {AveMetadataType.SocialComment, RestoreSocialComment},
    //                {AveMetadataType.ItemMetadataDto, RestoreListItemMetadataDto},
    //                {AveMetadataType.RoleAssignmentsDto, RestoreRoleAssignmentsDto},
    //                {AveMetadataType.AlertsDto, RestoreAlertsDto},
    //                {AveMetadataType.SocialDto, RestoreSocialDto},
    //                {AveMetadataType.WorkflowDto, RestoreWorkflowDto}
    //            };

    //    private static MetadataRestoreReport RestoreWorkflowDto(AveSPListItemRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowDto(
    //        //    restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPListItem.SPListItem,
    //        //    restoreDto.SPListItem.ParentList, restoreDto.SPListItemRestoreOption.WorkflowRestoreOption);
    //    }

    //    private static MetadataRestoreReport RestoreWorkflowSchedule(AveSPListItemRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowSchedule(
    //        //        restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPListItem.SPListItem);
    //    }

    //    private static MetadataRestoreReport RestoreWorkflowInstance(AveSPListItemRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowInstance(
    //        //        restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPListItem.SPListItem,
    //        //        restoreDto.SPListItem.ParentList, restoreDto.SPListItemRestoreOption.WorkflowRestoreOption);
    //    }

    //    private static MetadataRestoreReport RestoreSocialDto(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialoDto(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPListItem.ParentSite, restoreDto.Metadata, restoreDto.SPListItem.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreAlertsDto(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveAlertRestoreHelper.RestoreAlertDto(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.SPListItem.ParentSite, restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreSocialComment(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialComment(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPListItem.ParentSite, restoreDto.Metadata, restoreDto.SPListItem.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreSocialTag(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialTag(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPListItem.ParentSite, restoreDto.Metadata, restoreDto.SPListItem.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreDocSchedSubscriptions(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveAlertRestoreHelper.RestoreDocSchedSubscriptions(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreDocImmedSubscriptions(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveAlertRestoreHelper.RestoreDocImmedSubscriptions(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreDocumentTagging(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreDocumentTag(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPListItem.ParentSite, restoreDto.Metadata, restoreDto.SPListItem.TagUrl);
    //    }


    //    /// <summary>
    //    /// Restore Group Cache
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreGroupCache(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSecurityRestoreHelper.RestoreGroupCache(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                          restoreDto.SPListItem.ParentSite, restoreDto.Metadata);
    //    }

    //    /// <summary>
    //    /// Restore User Cache
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreUserCache(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return AveSecurityRestoreHelper.RestoreUserCache(restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                         restoreDto.SPListItem.ParentSite, restoreDto.Metadata);
    //    }

    //    /// <summary>
    //    /// Restore Inheritance
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreInheritance(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveSecurityRestoreHelper.RestoreInheritance(
    //                    restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPListItemRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore role assignments
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreRoleAssignments(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveSecurityRestoreHelper.RestoreRoleAssignments(
    //                    restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPListItemRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore Role Assignments
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreRoleAssignmentsDto(AveSPListItemRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPListItem))
    //        {
    //            return AveSecurityRestoreHelper.RestoreRoleAssignmentsDto(
    //                    restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPListItemRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore ItemList Property
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreListItemProperty(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.ItemMetadataDto,
    //                                                       restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                       () =>
    //                                                       {
    //                                                           var listItemtMetadataDto = new SPListItemMetadataDto()
    //                                                           {
    //                                                               //UserCache = restoreDto.RestoreStream.GetMetadataObj<AveUserList>(AveMetadataType.UserCache),
    //                                                               //GroupCache = restoreDto.RestoreStream.GetMetadataObj<AveGroupList>(AveMetadataType.GroupCache),
    //                                                               DocInfo_Old = restoreDto.Metadata.GetMetadata<Dictionary<string, object>>(),
    //                                                               MetadataInfo = restoreDto.RestoreStream.GetMetadataObj<List<AveTermStoreInfo>>(AveMetadataType.MetadataService),
    //                                                               UserDataInfo = restoreDto.RestoreStream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData),
    //                                                               DocDataJunction = restoreDto.RestoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction),
    //                                                               ItemTPGUIDofLookupValue = restoreDto.RestoreStream.GetMetadataObj<Dictionary<string, string>>(AveMetadataType.LookupFieldGuidValue)
    //                                                           };

    //                                                           return restoreDto.SPListItem.RestoreListItemMetadataDto(restoreDto.RestoreStream, restoreDto.SPListItemRestoreOption, listItemtMetadataDto);
    //                                                       });
    //    }

    //    /// <summary>
    //    /// Restore Doc Metadata
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreListItemMetadataDto(AveSPListItemRestoreDto restoreDto)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.ItemMetadataDto,
    //                                                   restoreDto.SPListItemRestoreOption.IncludePerformanceDetails,
    //                                                   () =>
    //                                                   {
    //                                                       var documentMetadataDto =
    //                                                           restoreDto.Metadata
    //                                                                     .GetMetadata<SPListItemMetadataDto>();//SPDocumentMetadataDto

    //                                                       return
    //                                                           restoreDto.SPListItem.RestoreListItemMetadataDto(
    //                                                               restoreDto.RestoreStream,
    //                                                               restoreDto.SPListItemRestoreOption,
    //                                                               documentMetadataDto);
    //                                                   });
    //    }

    //    /// <summary>
    //    /// Handle Metadata
    //    /// </summary>
    //    /// <param name="docRestoreDto"></param>
    //    /// <returns></returns>
    //    internal static MetadataRestoreReport HandleMetadata(AveSPListItemRestoreDto listItemRestoreDto)
    //    {
    //        RestoreAction<AveSPListItemRestoreDto, MetadataRestoreReport> restoreAction = null;

    //        if (restoreActions.TryGetValue(listItemRestoreDto.Metadata.MetadataType, out restoreAction))
    //        {
    //            return restoreAction(listItemRestoreDto);
    //        }
    //        else
    //        {
    //            logger.Error("Cannot handle this type:{0}", listItemRestoreDto.Metadata.MetadataType);
    //            //TODO 以后需要处理这个
    //        }

    //        return null;
    //    }
    //}
}