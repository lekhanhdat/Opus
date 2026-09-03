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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;


namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveSPListItem : RestoreableObject,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPListItem));

        private AveSPList mAveSPList;
        private AveSPFolder mParentFolder;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPItem mAveSPItem;
        private AveItemSecurity mItemSecurity;
        private string mSrcUrl;
        private string mUrl;
        private long mSize;
        private AveSPSite mAveParentSite;
        private AveListItemInfo mListItemInfo = new AveListItemInfo();
        public AveSPSite ParentSite { get { return mAveParentSite; } }
        public AveSPList ParentList
        {
            get { return mAveSPList; }
        }

        public IAveListItem SPListItem
        {
            get { return mAveSPItem.SPListItem; }
        }

        public AveSPItem AveSPItem
        {
            get { return mAveSPItem; }
        }

        public string Name
        {
            get { return mListItemInfo.Name; }
        }

        public bool IsNewCreated
        {
            get { return mListItemInfo.IsNewCreated; }
        }

        public bool NeedChangeItemId
        {
            get { return mListItemInfo.NeedChangeItemId; }
            set { mListItemInfo.NeedChangeItemId = value; }
        }

        public AveObjectSecurity Security
        {
            get
            {
                if (mItemSecurity == null)
                {
                    mItemSecurity = new AveItemSecurity(mAveSPItem);
                }
                return mItemSecurity;
            }
        }

        public string SrcUrl
        {
            get
            {
                return mSrcUrl;
            }
        }

        public string Url
        {
            get
            {
                return mUrl;
            }
        }

        public long Size
        {
            get
            {
                return mSize;
            }
        }

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

        public string OwnerLoginName
        {
            get
            {
                return mAveSPItem.OwnerLoginName;
            }
        }

        public Guid OldUniqueId
        {
            get
            {
                return mListItemInfo.OldUniqueId;
            }
        }

        public AveSPListItem(AveSPFolder aveFolder, string name)
        {
            aveFolder.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
            mParentFolder = aveFolder;
            mAveSPList = aveFolder.ParentList;
            mListItemInfo.KeepDefaultValue = mAveSPList.ParentWeb.ParentSite.KeepDefaultValue;
            mListItemInfo.VerifyItemMMSColumnValue = mAveSPList.ParentWeb.ParentSite.VerifyItemMMSColumnValue;
            mListItemInfo.SiteId = mAveSPList.ParentWeb.ParentSite.SPSite.ID;
            mListItemInfo.ParentId = aveFolder.Id;
            mListItemInfo.Name = name;
            mListItemInfo.ParentWebRelativeUrl = mAveSPList.ParentWeb.SPWeb.ServerRelativeUrl;//mAveSPList.ParentWeb.ServerRelativeUrl;
            mListItemInfo.ParentListTitle = mAveSPList.SPList.Title;//mAveSPList.Name;
            mListItemInfo.ParentListId = mAveSPList.SPList.ID;
            mListItemInfo.ParentFolderRelativeUrl = aveFolder.SPFolder.ServerRelativeUrl;//aveFolder.ServerRelativeUrl;            
            int pos = mListItemInfo.Name.IndexOf(':');
            if (pos >= 0)
            {
                mListItemInfo.Name = mListItemInfo.Name.Substring(0, pos);
            }
            mQueryService = aveFolder.QueryService;
            mAveParentSite = mParentFolder.ParentSite;
            mAveSPItem = new AveSPItem(mListItemInfo, AveItemType.ListItem, mParentFolder, mQueryService);
            //mListItemInfo.IsNewCreated = aveFolder.IsNewCreated;//doc-67167

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
                int id = GetNextAvailableId();
                mListItemInfo.Name = id.ToString() + "_.000";
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
                ///ADO-42988 zj check item存在 包括回收站
                DateTime dt = DateTime.MinValue;
                dt = GetListItemModifiedTime();
                if (!dt.Equals(DateTime.MinValue) && (!RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME || dt != modified))
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
                    dt = mAveParentSite.QueryService.CheckItemIdAvailableAndGetModifiedTimeForAppend(mAveSPList.SPList.ID, originalId);
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
            return mQueryService.GetNextAvailableId(mAveSPList.SPList.ID);
        }

        public void ResetName(string newName)
        {
            mListItemInfo.Name = newName;
        }

        /*private IAveListItem GetListItem(string name, int id)
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
        }*/

        public AveRestoreResult ProcessPreCondition(Dictionary<string, object> data, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction)
        {
            mAveSPItem.SetRestoreOption(mRestoreOption);
            mAveSPItem.ProcessPreCondition(data, userData);
            RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mParentFolder.RestoringItem.Init(mListItemInfo.Name, CheckRestoreOption(IsNewCreated || mAveSPList.IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            mListItemInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
            mListItemInfo.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME = RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME;
            mListItemInfo.RestoringItem = mParentFolder.RestoringItem;
            if (data.ContainsKey("DoclibRowId"))
            {
                object memberInfo = ParentList.ParentWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(mListItemInfo.OriginalRowId);
                if (this.ParentList.SPList.BaseTemplate == AveListTemplateType.UserInformation && memberInfo != null)  //different language setting
                {
                    mListItemInfo.OriginalRowId = ((AveSPMemberInfo)memberInfo).NewId;
                    mListItemInfo.Name = mListItemInfo.OriginalRowId + "_.000";
                    //mListItemInfo.OriginalRowId = mListItemInfo.OriginalRowId;
                }
                if (userData.ContainsKey("V4ConfirmedNote") && userData["V4ConfirmedNote"] != null)
                {
                    string[] tmpdata = userData["V4ConfirmedNote"].ToString().Split(';');
                    string userId = string.Empty;
                    try
                    {
                        userId = this.mAveSPList.SPList.ParentWeb.SiteUsers[tmpdata[1].TrimStart('#')].ID.ToString();
                        userData["V4ConfirmedNote"] = userData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                        userData["Confirmed"] = userData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Cannot find confirm user.\n error message:{0}", e));
                        //mLog.Warn("can not find confirm user ");
                        userId = this.mAveSPList.SPList.ParentWeb.Site.Owner.ID.ToString();
                        userData["V4ConfirmedNote"] = userData["V4ConfirmedNote"].ToString().Replace(tmpdata[0] + ";", userId + ";");
                        userData["Confirmed"] = userData["Confirmed"].ToString().Replace("#" + tmpdata[0], "#" + userId);
                    }
                }
                if (userData.ContainsKey("#tp_GUID"))
                {
                    mListItemInfo.tp_Guid = new Guid(userData["#tp_GUID"].ToString());
                }

                if (data.ContainsKey("HasUniqueRoleAssignments"))
                {
                    mListItemInfo.HasUniqueRoleAssignments = (bool)data["HasUniqueRoleAssignments"];
                    data.Remove("HasUniqueRoleAssignments");
                }

                if (RestoreOption.mAveItemRestoreOption.MOVE_SOURCE_ITEM_TO_FOLDER)
                {
                    //一定发生了冲突
                    if (mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict.ContainsKey(mListItemInfo.tp_Guid))
                    {
                        mListItemInfo.tp_Guid = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict[mListItemInfo.tp_Guid];
                    }
                    else
                    {
                        Guid guid = Guid.NewGuid();
                        mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict[mListItemInfo.tp_Guid] = guid;
                        mListItemInfo.tp_Guid = guid;
                    }
                }
            }
            else
            {
                throw new AveWarningException("The list item '{0}' does not have row id.", mListItemInfo.Name);
            }
            if (mAveSPList.IsTaxonomyList)
            {
                //return AveRestoreResult.Omit;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            mListItemInfo.SettingInfo.OverWriteByModifiedTime = CheckRestoreOption(AveRestoreMode.OverWriteByModifiedTime);
            ProcessDifferentLists(data, userData);
            //由于现在无法获取还原Item的Version， 在前台逻辑中统一取出最全的FiledsValues
            //mListItemInfo.FieldsInfo.Fields = mParentFolder.ParentList.AveFields.GetFieldValues(string.Empty, mListItemInfo.OriginalRowId, mListItemInfo.OriginalVersion, userData, true);
            Dictionary<string, object> fields;
            Dictionary<string, object> uniqueValues;
            //获取该ListItem的显示name
            string title = userData.ContainsKey("Title") ? userData["Title"].ToString() : mListItemInfo.Name;
            //SAAS-20934 设置Same type的column mapping，设置的ListItem目的端column value未应用上
            mParentFolder.ParentList.AveFields.GetFieldValues(title, mListItemInfo.OriginalRowId, mListItemInfo.OriginalVersion, userData, true, out fields, out uniqueValues);
            mListItemInfo.FieldsInfo.Fields = fields;
            mListItemInfo.FieldsInfo.UniqueValueFields = uniqueValues;
            mListItemInfo.FieldsInfo.MultilookupFields = mAveSPItem.GetDataJunction(dataJunction);
            //在这儿通过FieldMapping尝试得到TaxonomyFields TermIdMapping
            mAveSPItem.GetTaxonomyTermIdMapping(mListItemInfo.FieldsInfo.Fields, mListItemInfo);
            //mListItemInfo.NeedSetNullFields = mParentFolder.ParentList.SetNeedSetNullFields(mListItemInfo.FieldsInfo.Fields);
            mListItemInfo.SourceSiteInfo = mParentFolder.ParentSite.SourceSiteInfo;
            mListItemInfo.ParentSiteServerRelativeUrl = mParentFolder.ParentSite.ServerRelativeUrl;
            if (mAveSPList.IsCommunitySiteDiscussionList)
            {
                mListItemInfo.IsInCommunityDiscussion = true;
            }
            return AveRestoreResult.Normal;
        }

        private void ProcessDifferentLists(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            IAveList spList = mParentFolder.ParentList.SPList;
            AveSPSite parentSite = mParentFolder.ParentList.ParentWeb.ParentSite;
            if (spList.BaseTemplate == AveListTemplateType.Meetings)
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
            }
            else if (spList.BaseTemplate == AveListTemplateType.DiscussionBoard)
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
                        userData["ParentItemID"] = parentFolderId;
                    }
                    else
                    {
                        data["ParentThreadId"] = newId;
                        userData["ParentItemID"] = newId;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserDataFromDiscussionError, e.ToString());
                }
            }
        }


        public AveRestoreResult RestoreSelf(Dictionary<string, object> data, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction = null)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListItem.RestoreSelf"))
            {
#endif
                AveRestoreResult result = AveRestoreResult.Normal;

                #region Only for WebDatabse System List


                if (mAveSPList.SPList.IsACCSRVSystemList())
                {
                    log.Warn("skip the listitem due to it's parent list is ACCSRV");
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks && userData.ContainsKey("#tp_WorkflowInstanceID"))
                {
                    log.Warn("skip the listitem due to it's parent list is Task,and current item is workflow task item.");
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }
                if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.AccessRequest)
                {
                    log.Warn(WrapperRestoreReportResource.Wrapper_SkippedAccessRequestListItem);
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    mParentFolder.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedAccessRequestListItem.ToString();
                    mParentFolder.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedAccessRequestListItem;
                    return result;
                }
                if (userData == null || userData.Count == 0)
                {
                    log.Warn("skip the listitem due to no userdata");
                    mParentFolder.RestoringItem.NeedSkipped = true;
                    return result;
                }

                #endregion
                try
                {
                    ProcessPreCondition(data, userData, dataJunction);
                    if (mListItemInfo.VerifyItemMMSColumnValue)
                    {
                        if (this.ParentSite.MetadataService == null)
                        {
                            this.ParentSite.MetadataService = new AveMetadataService(this.ParentSite);
                        }
                        //保证item的MetadataColumn的term能够存在或还原成功，才能允许继续restore item
                        if (this.ParentList.SPList != null && mListItemInfo.FieldsInfo.TaxonomyFieldsInMapping != null && mListItemInfo.FieldsInfo.TermIdMapping != null && !this.ParentSite.MetadataService.VerifyMetadataColumnValue(mListItemInfo, this.ParentList.SPList, mListItemInfo.FieldsInfo.TaxonomyFieldsInMapping, mListItemInfo.FieldsInfo.TermIdMapping, mAveParentSite.ObjectModelFactory))
                        {
                            log.Log(AveLogLevel.WARN, string.Format("VerifyMetadataColumnValue failed, shouldn't restore listItem:{0}", mListItemInfo.Name));
                            throw new AveVerifyItemMetadataValueNotFoundException("Verify item metadata column value failed");
                        }
                    }

                    result = mAveSPList.ListItemSerializer.SetObjectData(mListItemInfo);
                    mAveSPItem.CacheMutiLookupValue();
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
                if (result > 0 || result == AveRestoreResult.SkipTheSameItem)//SAAS-13436 还原blogSite中保证post与comment不错位，保证即使是post被skip也要做mapping。在post action中keep lookupfield value
                {
                    mAveSPList.AveFields.ResetNotUpdateLookupFieldValue(mListItemInfo.RowId);
                    mAveSPList.AveFields.ResetNintexFormDataFieldValue(mListItemInfo.RowId);
                    List<string> needPreserveMappingListNames = new List<string>() { "Project Policy Item List" };
                    bool needPreserveMapping = needPreserveMappingListNames.Contains(mAveSPList.SPList.Title);
                    mAveSPItem.AddItemMapping(mListItemInfo.OriginalRowId, needPreserveMapping);
                    if (SPListItem != null)
                    {
                        mParentFolder.RestoringItem.ReSetItemName(SPListItem.ID.ToString() + "_.000");
                    }

                    #region ProcessHold

                    Hashtable lockMetaInfo = new Hashtable();
                    Dictionary<string, string> fileHoldValue = new Dictionary<string, string>();
                    try
                    {
                        if ((userData.ContainsKey("_vti_ItemHoldRecordStatus")) && (!string.Equals(userData["_vti_ItemHoldRecordStatus"].ToString(), "0", StringComparison.OrdinalIgnoreCase)) && data.ContainsKey("MetaInfo"))
                        {
                            var dataMetaInfo = (byte[])data["MetaInfo"];
                            var itemHoldRecord = mAveSPItem.GetHoldRecord(lockMetaInfo, dataMetaInfo, userData);
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
                return result;
#if PerformanceLog
            }
#endif
        }
        public void RestoreUserInfo(Dictionary<string, object> userData)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListItemUserInfo"))
            {
#endif
                string name = null;
                if (userData.ContainsKey("Name"))
                {
                    name = userData["Name"].ToString();
                    userData.Remove("Name");
                }
                else
                {
                    return;
                }
                if (userData.ContainsKey("#tp_IsCurrent"))
                {
                    if (!Convert.ToBoolean(userData["#tp_IsCurrent"]))
                    {
                        return;
                    }
                }

                int id = mAveSPList.ParentWeb.GetUserIdByName(name);
                if (id <= 0)
                {
                    int originalRowId = -1;
                    if (userData.ContainsKey("#tp_ID") && Int32.TryParse(userData["#tp_ID"].ToString(), out originalRowId))
                    {
                        var pricipal = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMember(originalRowId, false);
                        if (pricipal != null)
                        {
                            id = pricipal.ID;
                        }
                    }
                }
                if (id > 0)
                {
                    try
                    {
                        mAveSPItem.SPListItem = mAveSPList.SPList.GetItemById(id);
                        mAveSPItem.InitBySPListItem(mAveSPItem.SPListItem);
                        string mappingName = mAveSPList.ParentSite.SPMembers.GetMappingUserLogin(name);
                        if (mAveSPList.ParentWeb.ParentSite.SPMembers.AllGroups.Contains(name.ToLower()) || name.Equals(mappingName, StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> fieldData = mParentFolder.ParentList.AveFields.GetFieldValues(string.Empty, mAveSPItem.RowId, 512, userData, true);
                            mAveSPItem.UpdateFields(fieldData, mListItemInfo);
                        }
                    }
                    catch (Exception ep)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Restore user item failed. User Name:{0}, User ID:{1}, Reason:{2}", name, id, ep.ToString()));
                        return;
                    }
                    int originalRowId = -1;
                    if (userData.ContainsKey("#tp_ID") && Int32.TryParse(userData["#tp_ID"].ToString(), out originalRowId))
                    {
                        mAveSPItem.AddItemMapping(originalRowId);
                    }
                }
                else
                {
                    Dictionary<string, object> fieldData = mParentFolder.ParentList.AveFields.GetFieldValues(string.Empty, -1, 512, userData, true);
                    mAveSPList.ParentWeb.ParentSite.MappingManager.WebMappingManager.PostUserInfo[name] = fieldData;
                }
#if PerformanceLog
            }
#endif
        }
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
                if (mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(mAveSPList.SPList.ID)
                    && mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mAveSPList.SPList.ID].ContainsKey(tempRowId))
                {
                    tempRowId = mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mAveSPList.SPList.ID][tempRowId];
                }
                else if (data.ContainsKey("GUID"))
                {
                    //SAAS-11351 通过GUID来获得相应的ListItem
                    string key = data["GUID"].ToString();
                    tempRowId = mAveSPList.SPList.ListItemGuidAndRowIdMappings.ContainsKey(key) ? mAveSPList.SPList.ListItemGuidAndRowIdMappings[key] : -1;
                }
                if (tempRowId > 0)
                {
                    current = mAveSPList.SPList.GetItemById(tempRowId);
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetCurrentSPListItemFailed, ex);
            }
            return current;
        }

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
                        fileUrl = mAveSPList.SPList.DefaultDisplayFormUrl + "?ID=" + mAveSPItem.RowId;
                    }
                    else
                    {
                        fileUrl = webUrl.TrimEnd('/') + "/" + mAveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(webRelativeUrl.TrimStart('/').Length).TrimStart('/') + "?ID=" + mAveSPItem.RowId;
                    }
                }
                else if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Meetings && mAveSPItem.SPListItem.ID != 0)
                {
                    if (this.mParentFolder.SPFolder.HiddenFiles != null)
                    {
                        if (this.mParentFolder.SPFolder.HiddenFiles.Count > 1 && webUrl.LastIndexOf(webRelativeUrl) > 0)
                        {
                            fileUrl = webUrl.Substring(0, webUrl.LastIndexOf(webRelativeUrl)) + this.mParentFolder.ServerRelativeUrl + "/" + this.mParentFolder.SPFolder.HiddenFiles[0].Name + "?ID=" + mAveSPItem.SPListItem.ID;
                        }
                    }
                }
                return fileUrl;
            }
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

        public void Dispose()
        {
            mAveSPItem?.Dispose();
        }
    }
}