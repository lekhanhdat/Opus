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
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Resource.Client;
using AveClientRequest.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using Microsoft.SharePoint.Client.Taxonomy;
using AvePoint.Wrapper.Resource;
using Microsoft.SharePoint.Client.RecordsRepository;
using Microsoft365.Authentication;
using AvePoint.Common.FilterEngine;
using Microsoft.SharePoint.Client.CompliancePolicy;
using Microsoft365.SharePoint.Cache.Restore;
using AvePoint.GCommon.Contract.DeploymentManager.Object;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveListItemRestore : IDisposable
    {
        private readonly static object moveConflictFolderLocker = new object();
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveListItemRestore));
        private Site mSite;
        private Web mParentWeb;
        private string mParentWebServerRelativeUrl;
        private List mParentList;
        private string mParentFolderRelativeUrl;
        private ResourcePath mParentFolderRelativePath;
        private string mListRootFolderUrl;
        private string mListDefaultViewUrl;
        private int mListTemplate;
        private Dictionary<string, object> mParentWebProperties;
        private Dictionary<string, object> mParentListProperties;
        private int mRowId;
        private int mDestRowId;
        private Guid mGuid;
        private int mModerationStatus;
        private bool mIsNewCreated;
        private int mOrginalVersion;
        private Dictionary<string, object> mOriginalUserData;
        private byte mOriginalLevel;
        private bool mOverWrite;
        private int mDraftOwnerId;
        private int mCheckoutUserId;
        private ClientContext mContext;
        private string mParentWebUrl;
        private string mTitle;
        private string mName;
        private bool mEnableModeration;
        private bool mEnableVersioning;
        private Guid mListId;
        private int mListBaseType;
        private string mListTitle;
        private bool mEnableMinorVersions;
        private bool mMOVE_ITEM_TO_CONFLICT_FOLDER;
        private bool mSKIP_IF_SAME_MODIFIEDTIME;
        private bool mOverwriteByLastModifiedTime;
        private bool mRestoreSecurityOnly;
        private bool mIsCommunityDiscussionList;
        private string mListRootFolderServerRelativeUrl;
        private AveRestoreOption mRestoreOption;
        private AveClientOM2013Request mRequest;
        private Dictionary<string, object> mUniqueValues;

        /// <summary>
        /// 为unittest提供构造方法
        /// </summary>
        public AveListItemRestore() { }

        public AveListItemRestore(AveClientOM2013Request request, Site site, ClientContext conText)
        {
            mRequest = request;
            mSite = site;
            mContext = conText;
        }

        public AveListItemRestore(AveClientOM2013Request request, Site site, Web web, List list, int rowId, int moderationStatus, ClientContext context)
        {
            mRequest = request;
            mSite = site;
            mContext = context;
            mParentWeb = web;
            mParentList = list;
            mRowId = rowId;
            mModerationStatus = moderationStatus;
        }

        public void PrepareParentProperties(Dictionary<string, object> data)
        {
            mParentWebProperties = data["ParentWebProperties"] as Dictionary<string, object>;
            mParentListProperties = data["ParentListProperties"] as Dictionary<string, object>;
            mParentWebServerRelativeUrl = mParentWebProperties["ServerRelativeUrl"] as string;
            mListTemplate = mParentListProperties.ContainsKey("ListTemplate") ? (int)mParentListProperties["ListTemplate"] : -1;
            mEnableModeration = mParentListProperties.ContainsKey("ListEnableModeration") ? Convert.ToBoolean(mParentListProperties["ListEnableModeration"]) : false;
            mEnableVersioning = mParentListProperties.ContainsKey("ListEnableVersioning") ? Convert.ToBoolean(mParentListProperties["ListEnableVersioning"]) : false;
            mEnableMinorVersions = mParentListProperties.ContainsKey("ListEnableMinorVersions") ? Convert.ToBoolean(mParentListProperties["ListEnableMinorVersions"]) : false;
        }

        protected void PrepareRestoreContext(Dictionary<string, object> data, Dictionary<string, object> userData, Dictionary<string, object> uniqueValues)
        {
            PrepareParentProperties(data);
            mParentFolderRelativeUrl = data["FolderUrl"] as string;
            mParentFolderRelativePath = ResourcePath.FromDecodedUrl(mParentFolderRelativeUrl);
            mParentWebUrl = data["WebUrl"] as string;
            mListTemplate = mParentListProperties.ContainsKey("ListTemplate") ? (int)mParentListProperties["ListTemplate"] : -1;
            mListBaseType = mParentListProperties.ContainsKey("BaseType") ? (int)mParentListProperties["BaseType"] : -1;
            mListRootFolderServerRelativeUrl = data["ListRootFolderServerRelativeUrl"] as string;
            mListRootFolderUrl = mParentListProperties.ContainsKey("ListRootFolderUrl") ? mParentListProperties["ListRootFolderUrl"] as string : string.Empty;
            mListDefaultViewUrl = mParentListProperties.ContainsKey("ListDefaultViewUrl") ? mParentListProperties["ListDefaultViewUrl"] as string : string.Empty;
            mListTitle = mParentListProperties.ContainsKey("ListTitle") ? mParentListProperties["ListTitle"] as string : string.Empty;
            mListId = mParentListProperties.ContainsKey("ListId") ? (Guid)mParentListProperties["ListId"] : Guid.Empty;
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mParentList = mParentWeb.Lists.GetById(mListId);
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mDestRowId = (int)data["DestRowId"];
            mGuid = data.ContainsKey("GUID") ? new Guid(data["GUID"].ToString()) : Guid.Empty;
            mOrginalVersion = Convert.ToInt32(data["UIVersion"]);
            mOriginalLevel = data.ContainsKey("Level") ? Convert.ToByte(data["Level"]) : (byte)0;
            mDraftOwnerId = data.ContainsKey("DraftOwnerId") ? Convert.ToInt32(data["DraftOwnerId"]) : -1;
            mModerationStatus = data.ContainsKey("_ModerationStatus") ? Convert.ToInt32(data["_ModerationStatus"]) : -1;
            mOverWrite = data.ContainsKey("DeleteItem") ? Convert.ToBoolean(data["DeleteItem"]) : false;
            mIsNewCreated = data.ContainsKey("IsNewCreated") ? Convert.ToBoolean(data["IsNewCreated"]) : false;
            mCheckoutUserId = data.ContainsKey("CheckoutUserId") ? Convert.ToInt32(data["CheckoutUserId"]) : -1;
            mTitle = data.ContainsKey("Title") ? data["Title"] as string : string.Empty;
            mName = userData != null && userData.ContainsKey("Name") ? userData["Name"] as string : string.Empty;
            mMOVE_ITEM_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_ITEM_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_ITEM_TO_CONFLICT_FOLDER"]) : false;
            mOverwriteByLastModifiedTime = data.ContainsKey("OverwriteByLastModifiedTime") ? Convert.ToBoolean(data["OverwriteByLastModifiedTime"]) : false;
            mRestoreSecurityOnly = data.ContainsKey("RestoreSecurityOnly") ? Convert.ToBoolean(data["RestoreSecurityOnly"]) : false;
            mRestoreOption = data.ContainsKey("RestoreOption") ? (AveRestoreOption)Enum.Parse(typeof(AveRestoreOption), data["RestoreOption"].ToString()) : AveRestoreOption.Default;
            mSKIP_IF_SAME_MODIFIEDTIME = data.ContainsKey("SKIP_IF_SAME_MODIFIEDTIME") ? Convert.ToBoolean(data["SKIP_IF_SAME_MODIFIEDTIME"]) : false;
            mIsCommunityDiscussionList = data.ContainsKey("IsCommunityDiscussionList") ? Convert.ToBoolean(data["IsCommunityDiscussionList"]) : false;
            mUniqueValues = uniqueValues;
            mOriginalUserData = userData;
        }

        internal Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            return RestoreListItem(data, userData, null);
        }

        private bool ResolveDeclaredDocument(ListItem listItem, Dictionary<string, object> restoreResult)
        {
            //overwrite或者appendNewVersion都需要做undeclare处理(AppendANewVersion只archiver模块用到)
            bool flag = (int)mRestoreOption == (int)AveRestoreMode.AppendANewVersion;
            if (flag || mOverWrite)
            {
                Records.UndeclareItemAsRecord(mContext, listItem);
                if (flag)
                {
                    //针对还原目的端version大于要还原的version这种情况，需要先load listItem，否则会出现serverException：Version Conflict.
                    mContext.Load(listItem);
                    mContext.ExecuteQuery();
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        internal Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Dictionary<string, object> uniqueValues = null)
        {
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            try
            {
                PrepareRestoreContext(data, userData, uniqueValues);
                if(ItemRestoreCache.IsOverWriteFailItem(mListId.ToString(), mRowId.ToString()))
                {
                    throw new Exception("RM_RS_FailOverwriteItem");
                }
                ListItem listItem = null;
                //处理conflict folder
                if (mMOVE_ITEM_TO_CONFLICT_FOLDER)
                {
                    lock (moveConflictFolderLocker)
                    {
                        MoveToConflictFolder();
                    }
                }
                bool deleteCurrentVersion = false;
                bool itemExist = false;
                if (!mMOVE_ITEM_TO_CONFLICT_FOLDER)//replicator move to conflict folder的情况下不用再check
                {
                    itemExist = NeedCheckItemExistenceForSpecialListTemplateType(mListTemplate) ?
                        CheckItemExistenceForSpecialListTemplateType(mParentList, mListTemplate, ref listItem, data, userData) :
                        IsItemExsit(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref listItem);
                }
                else
                {
                    listItem = null;
                }
                if (!itemExist)
                {
                    restoreResult["ItemExist"] = false;
                }
                if (itemExist && mOverwriteByLastModifiedTime && data.ContainsKey("BiggestVersionModified")
                    && (DateTime)data["BiggestVersionModified"] <= (DateTime)listItem.FieldValues["Modified"])
                {
                    //Overwrite item by LastModifiedTime.
                    restoreResult["SkippedByLastModifiedTime"] = true;
                    restoreResult["RestoreStatus"] = true;
                    return restoreResult;
                }

                if (itemExist && listItem.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus") && listItem.FieldValues["_vti_ItemHoldRecordStatus"] != null)
                {
                    if (listItem.FieldValues["_vti_ItemHoldRecordStatus"].ToString() == "273")
                    {
                        //返回true说明冲突处理选择skip
                        if (ResolveDeclaredDocument(listItem, restoreResult))
                        {
                            restoreResult["SkippedByDeclaredDocument"] = true;
                            restoreResult["RestoreStatus"] = true;
                            return restoreResult;
                        }
                        
                    }
                }
                //if (!itemExist && mRestoreSecurityOnly)
                //{
                //    restoreResult["SkippedByRestoreSecurityOnly"] = true;
                //    restoreResult["RestoreStatus"] = true;
                //    return restoreResult;
                //}

                if ((mRestoreOption & AveRestoreOption.Append) == AveRestoreOption.Append
                    && !mIsNewCreated && listItem != null
                    && (!mSKIP_IF_SAME_MODIFIEDTIME || (DateTime)data["BiggestVersionModified"] != (DateTime)listItem.FieldValues["Modified"]))
                {
                    //Need append a new item.
                    listItem = null;
                }
                if (listItem != null && mOverWrite)
                {
                    //community site下的category 和 member list 下的item如果已存在直接更新
                    if (!(mListTemplate == 500 || mListTemplate == 880))
                    {
                        mLogger.Debug("delete item: {0}", listItem.Id);
                        if (mParentList != null && listItem != null && listItem.FieldValues.ContainsKey("UniqueId"))
                        {
                            restoreResult["OldUniqueId"] = listItem["UniqueId"];
                        }
                        listItem.DeleteObject();
                        mContext.ExecuteQuery();
                        listItem = null;
                    }
                }
                if (listItem == null)
                {
                    listItem = AddListItem(data, userData);
                    deleteCurrentVersion = true;
                    DeleteComplianceTagIfCreateInThisJob(listItem);
                }
                if ((mOverWrite && !mRestoreSecurityOnly) || mIsNewCreated)
                {
                    if (mOrginalVersion == Convert.ToInt32(listItem["_UIVersion"]))
                    {
                        if (!mIsNewCreated || (itemExist && !deleteCurrentVersion))
                        {
                            listItem = UpdateListItem(listItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                        }
                    }
                    else
                    {
                        listItem = UpdateToSpecificVersion(listItem, mOrginalVersion, deleteCurrentVersion, userData);
                    }
                }
                SetComplianceTagIfCreateInThisJob(listItem, data);
                restoreResult["IsNewCreated"] = !mIsNewCreated ? data["IsNewCreated"] : true;
                //if append an item but destination is exist ,need skipped.
                if (listItem != null && !mOverWrite && !mIsNewCreated && !mRestoreSecurityOnly)
                {
                    //modified time相同时，不进行append操作，listitem会被skip
                    restoreResult["IsSkipped"] = true;
                }
                restoreResult["RestoreStatus"] = true;
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                AveClientOM2013Request.GetItemDic(itemProp, listItem);
                restoreResult["Item"] = itemProp;
            }
            /*review-qlluo*/catch (AveRestoreException)
            {
                //Current ListItem is skipped because duplicate values were found in the field(s) that enabled unique value
                restoreResult["SkippedByHasUniqueValue"] = true;
                restoreResult["RestoreStatus"] = true;
            }
            /*review-qlluo*/catch (Exception e)
            {
                if ((e is ServerException) && (e as ServerException).ServerErrorCode == -2130575282)
                {
                    throw;
                }
                restoreResult["Exception"] = e.Message + "--" + e.StackTrace;
                if (e != null && !string.IsNullOrEmpty(e.Message) && e.Message.Contains("0x80131904"))
                {
                    restoreResult["ExceptionMessage"] = string.Format(WrapperRestoreReportResource.Wrapper_SharePointBusyError, e.Message);
                }
                else
                {
                    restoreResult["ExceptionMessage"] = e.Message;
                }
                restoreResult["RestoreStatus"] = false;
            }

            return restoreResult;
        }

        private void DeleteComplianceTagIfCreateInThisJob(ListItem listItem)
        {
            if (ItemRestoreCache.IsNewCreateItem(mListId.ToString(), listItem.Id.ToString()) && !string.IsNullOrWhiteSpace(listItem?.ComplianceInfo?.ComplianceTag))
            {
                try
                {
                    if (!listItem.ComplianceInfo.TagPolicyRecord && listItem.ComplianceInfo.TagPolicyHold && IsRecordTypeComplianceTag(listItem.ComplianceInfo.ComplianceTag) && WasOriginallyLocked())
                    {
                        mRequest.LockRecordItem(mParentWebUrl, mListRootFolderServerRelativeUrl, listItem.Id.ToString());
                    }
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { listItem.Id }, "");
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Fail delete retention label,error message:{ex.Message},web url:{mParentWebUrl},listUrl:{mListRootFolderServerRelativeUrl},rowId:{mRowId},error:{ex}");
                }
            }
        }

        protected bool IsRecordTypeComplianceTag(string complianceTagName)
        {
            try
            {
                var sitePropertyContext = SitePropertyCache.GetInstance();
                if (sitePropertyContext.AvaliableComplianceTags == null)
                {
                    sitePropertyContext.InitAvaliableComplianceTags(mContext.Url, mContext);
                }
                var complianceTag = sitePropertyContext.AvaliableComplianceTags.FirstOrDefault(info => info.TagName == complianceTagName);
                if (complianceTag != null)
                {
                    if (complianceTag.BlockDelete && complianceTag.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLogger.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLogger.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}, ex:{ex}");
                throw;
            }
        }

        private bool WasOriginallyLocked()
        {
            if (mOriginalUserData?.TryGetValue("_vti_ItemHoldRecordStatus", out var status) != true || status == null || !int.TryParse(status.ToString(), out var value))
            {
                return false;
            }
            return ((long)value & 16L) != 0;
        }


        private void SetComplianceTagIfCreateInThisJob(ListItem listItem, Dictionary<string, object> documentInfo)
        {
            if (ItemRestoreCache.IsNewCreateItem(mListId.ToString(), listItem.Id.ToString()) && documentInfo.ContainsKey("ComplianceTag") && !string.IsNullOrWhiteSpace(documentInfo?["ComplianceTag"]?.ToString()))
            {
                try
                {
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { listItem.Id }, documentInfo["ComplianceTag"].ToString());
                    if (WasOriginallyLocked() && IsRecordTypeComplianceTag(documentInfo["ComplianceTag"].ToString()))
                    {
                        mRequest.LockRecordItem(mParentWebUrl, mListRootFolderServerRelativeUrl, listItem.Id.ToString());
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Fail set retention label,label:{documentInfo["ComplianceTag"]},web url:{mParentWebUrl}, list url:{mListRootFolderServerRelativeUrl}, row id:{listItem.Id},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
        }
        private bool IsConflictItem(ListItem tempItem)
        {
            if (mGuid.Equals(new Guid(tempItem["GUID"].ToString())))
            {
                //Trim items themselves.
                return false;
            }
            return true;
        }

        private string BuildQueryString(Dictionary<string, object> uniqueValues = null)
        {
            //SAAS-10077 当<Or>和<And>同时套用2个以上的查询条件时,构造出来的query在执行时会有异常,所以采用分层嵌套的方式来构造query.
            StringBuilder viewXmlStringBuilder = new StringBuilder();
            viewXmlStringBuilder.Append("<View><Query><Where>");
            int conditionCount = 0;
            for (int i = 1; i < uniqueValues.Count; i++)
            {
                viewXmlStringBuilder.Append("<" + CamlQueryExpression.Or.ToString() + ">");
            }
            foreach (KeyValuePair<string, object> pair in uniqueValues)
            {
                AveFieldValueInfo fieldValue = pair.Value as AveFieldValueInfo;
                if (fieldValue != null)
                {
                    string colValue = fieldValue.ColValue.ToString();
                    string value = colValue.Contains(";#") ? colValue.Substring(colValue.IndexOf(";#", StringComparison.Ordinal) + 2) : colValue;
                    if (fieldValue.FieldType == AveFieldType.Invalid)
                    {
                        fieldValue.FieldType = AveFieldType.Text;
                    }
                    string fieldRef = string.Format("<Eq><FieldRef Name='{0}'/><Value Type='{1}'>{2}</Value></Eq>", pair.Key, fieldValue.FieldType, value);
                    if (conditionCount > 0)
                    {
                        fieldRef = fieldRef + "</" + CamlQueryExpression.Or.ToString() + ">";
                    }
                    conditionCount++;
                    viewXmlStringBuilder.Append(fieldRef);
                }
            }
            viewXmlStringBuilder.Append("</Where></Query></View>");
            return viewXmlStringBuilder.ToString();
        }

        private void PreRestoreFields(Dictionary<string, object> userData, ref Dictionary<string, object> keepData)
        {
            if (mIsCommunityDiscussionList && mOrginalVersion == 512
                && userData.ContainsKey("Author")
                && userData.ContainsKey("Editor"))//Community site's discussion list add new item before restoring author and editor.
            {
                keepData["NewItemProperties"] = GetNeedPostFields(userData, new string[] { "Author", "Editor" });
            }
            else if (mListTemplate == (int)ListTemplateType.TasksWithTimelineAndHierarchy
                     && userData.ContainsKey("Author") && userData.ContainsKey("Editor"))
            {
                List<string> parentListContentTypeIds = mParentListProperties["ParentListContentTypeIds"] as List<string>;
                foreach (string ctId in parentListContentTypeIds)
                {
                    if (ctId.StartsWith("0x0107"))//tasks list with message content type need post restore author and editor.
                    {
                        keepData = GetNeedPostFields(userData, new string[] { "Author", "Editor" });
                        break;
                    }
                }
            }
        }

        private Dictionary<string, object> GetNeedPostFields(Dictionary<string, object> userData, string[] properties)
        {
            Dictionary<string, object> needPostProperties = new Dictionary<string, object>();
            foreach (string property in properties)
            {
                needPostProperties[property] = userData[property];
                userData.Remove(property);
            }
            needPostProperties["Modified"] = userData["Modified"];
            return needPostProperties;
        }

        private void PostRestoreFields(ListItem parentItem, ListItem listItem, Dictionary<string, object> needKeepData)
        {
            //keep discussion folder的一些属性如modified time等
            if ((ListTemplateType)mListTemplate == ListTemplateType.DiscussionBoard)
            {
                if (parentItem != null)
                {
                    //int sourceRowId = Convert.ToInt32(parentItem["BestAnswerId"]);
                    //if (mRowId == sourceRowId)
                    //{
                    //    parentItem["BestAnswerId"] = listItem.Id;
                    //    parentItem.Update();
                    //}
                    if (needKeepData.Count > 0)
                    {
                        if (needKeepData.ContainsKey("NewItemProperties"))
                        {
                            Dictionary<string, object> postProperties = needKeepData["NewItemProperties"] as Dictionary<string, object>;
                            UpdateListItem(listItem, postProperties, ListItemUpdateMethodKind.SystemUpdate, false);
                            needKeepData.Remove("NewItemProperties");
                        }
                        UpdateListItem(parentItem, needKeepData, ListItemUpdateMethodKind.SystemUpdate, false);
                    }
                }
            }
            else if (needKeepData.Count > 0)
            {
                UpdateListItem(listItem, needKeepData, ListItemUpdateMethodKind.SystemUpdate, false);
            }
        }

        internal ListItem AddListItem(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItem listItem = null;
            ListItem parentItem = null;//记录需要keep一写data如modified time的parent item
            Dictionary<string, object> parentNeedKeepData = new Dictionary<string, object>();//存放parent item需要keep的data
            
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                switch ((ListTemplateType)mListTemplate)
                {
                    case ListTemplateType.DiscussionBoard:
                        listItem = AddDiscussionTopic(data, userData, ref parentItem, ref parentNeedKeepData);
                        break;
                    case ListTemplateType.MeetingUser:
                        listItem = AddMettingUser(data, userData);
                        break;
                    case ListTemplateType.Meetings:
                        listItem = AddMettings(data, userData);
                        break;
                    default:
                        ListItemCreationInformationUsingPath createInfoUsingPath = new ListItemCreationInformationUsingPath();
                        createInfoUsingPath.FolderPath = mParentFolderRelativePath;
                        createInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.File;
                        listItem = mParentList.AddItemUsingPath(createInfoUsingPath);
                        break;
                }
                //SAAS-27961,当第一次还原content level 冲突为append时，无法keep住guid，增加rowID的判断，在目的端不存在这个item时，keep住Guid.
                if (mGuid != Guid.Empty && mListTemplate != (int)ListTemplateType.Survey)
                {
                    if (mRestoreOption == AveRestoreOption.Append && mRowId != -1)
                    {
                        listItem.Properties["AppendGUID"] = mGuid.ToString();
                        //listItem.SystemUpdate();
                        mLogger.Debug("set AppendGUID prop: {0}", mGuid);
                    }
                    else
                    {
                        listItem["GUID"] = mGuid;
                        mLogger.Debug("set guid field: {0}", mGuid);
                    }
                }

                if (mOrginalVersion == 512)
                {
                    PreRestoreFields(userData, ref parentNeedKeepData);
                    if (mEnableModeration)
                    {
                        userData["_ModerationStatus"] = mModerationStatus;
                    }
                    RestoreItemFields(ref listItem, userData, ListItemUpdateMethodKind.None);
                }
                else //RestoreItemFields #if ValidateUpdateListItem is called it will update listitem no need to update list item again; #else call listitem.update inside
                {
                    listItem.Update();
                }
                mContext.Load(listItem);
                mContext.Load(listItem, item => item.ComplianceInfo);
                HandleDeclareItem(listItem, userData);
                mContext.ExecuteQuery();
            });

            mLogger.Debug("new item id: {0}", listItem?.Id);
            PostRestoreFields(parentItem, listItem, parentNeedKeepData);
            mIsNewCreated = true;
            ItemRestoreCache.AddNewCreateItem(mListId.ToString(), listItem.Id.ToString());
            return listItem;
        }

        private void HandleDeclareItem(ListItem listItem, Dictionary<string, object> userData)
        {
            if (userData.ContainsKey("_vti_ItemDeclaredRecord")) //Item declare records.
            {
                listItem["_vti_ItemDeclaredRecord"] = new DateTime(((DateTime)userData["_vti_ItemDeclaredRecord"]).Ticks, DateTimeKind.Utc);
                if (mEnableModeration)
                {
                    listItem["_ModerationStatus"] = mModerationStatus;
                }
                listItem.Update();
            }
        }

        protected ListItem AddDiscussionTopic(Dictionary<string, object> data, Dictionary<string, object> userData, ref ListItem parentItem, ref Dictionary<string, object> parentNeedKeepData)
        {
            ListItem item = null;
            if (data.ContainsKey("ParentThreadId"))
            {
                item = mParentList.GetItemById(Convert.ToInt32(data["ParentThreadId"]));
            }
            else
            {
                item = mParentList.GetItemById(Convert.ToInt32(data["DiscussionTopic"]));
            }

            mContext.Load(item);
            mContext.ExecuteQuery();
            parentItem = item;
            //keep parent discussion的一些属性
            Dictionary<string, object> keepData = new Dictionary<string, object>();
            if (item != null)
            {
                if (item.FieldValues.ContainsKey("DiscussionLastUpdated"))
                {
                    keepData["DiscussionLastUpdated"] = item["DiscussionLastUpdated"];
                }
                if (item.FieldValues.ContainsKey("Modified"))
                {
                    keepData["Modified"] = item["Modified"];
                }
                if (item.FieldValues.ContainsKey("LastReplyBy"))
                {
                    keepData["LastReplyBy"] = item["LastReplyBy"];
                }
                //if (item.FieldValues.ContainsKey("MyEditor"))
                //{
                //    keepData["MyEditor"] = item["MyEditor"];
                //}
                if (item.FieldValues.ContainsKey("Created"))
                {
                    keepData["Created"] = item["Created"];
                }
                if (item.FieldValues.ContainsKey("Editor"))
                {
                    keepData["Editor"] = item["Editor"];
                }
                if (item.FieldValues.ContainsKey("Author"))
                {
                    keepData["Author"] = item["Author"];
                }
                if (item.FieldValues.ContainsKey("_ModerationStatus"))
                {
                    keepData["Parent_ModerationStatus"] = item["_ModerationStatus"];
                }
                if (item.FieldValues.ContainsKey("FileLeafRef"))
                {
                    keepData["FileLeafRef"] = item["FileLeafRef"];
                }
            }
            parentNeedKeepData = keepData;
            return Utility.CreateNewDiscussionReply(mContext, item);
        }

        protected ListItem AddMettingUser(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
            creationInfoUsingPath.FolderPath = mParentFolderRelativePath;
            creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = mParentList.AddItemUsingPath(creationInfoUsingPath);
            if (data.ContainsKey("Status"))
            {
                listItem["Status"] = data["Status"].ToString();
            }
            if (data.ContainsKey("Attendance"))
            {
                listItem["Attendance"] = data["Attendance"].ToString();
            }
            if (data.ContainsKey("Title"))
            {
                AveAssemblyUtility.SetFieldValue(listItem, "m_strNewBaseName", data["Title"].ToString());
            }
            return listItem;
        }

        protected ListItem AddMettings(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
            creationInfoUsingPath.FolderPath = mParentFolderRelativePath;
            creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = mParentList.AddItemUsingPath(creationInfoUsingPath);

            if (data.ContainsKey("Title"))
            {
                listItem["Title"] = data["Title"];
            }
            if (data.ContainsKey("EventType"))
            {
                listItem["EventType"] = (int)data["EventType"];
            }
            if (data.ContainsKey("TimeZone"))
            {
                listItem["TimeZone"] = (int)data["TimeZone"];
            }
            if (data.ContainsKey("UID"))
            {
                listItem["UID"] = (Guid)data["UID"];
            }
            if (data.ContainsKey("EventDate"))
            {
                listItem["EventDate"] = new DateTime(((DateTime)data["EventDate"]).Ticks, DateTimeKind.Utc);
            }
            if (data.ContainsKey("Duration"))
            {
                listItem["Duration"] = (int)data["Duration"];
            }
            if (data.ContainsKey("EndDate"))
            {
                listItem["EndDate"] = new DateTime(((DateTime)data["EndDate"]).Ticks, DateTimeKind.Utc);
            }
            if (data.ContainsKey("RecurrenceID"))
            {
                listItem["RecurrenceID"] = (DateTime)data["RecurrenceID"];
            }
            if (data.ContainsKey("Location"))
            {
                listItem["Location"] = data["Location"];
            }
            if (data.ContainsKey("RecurrenceData"))
            {
                listItem["RecurrenceData"] = data["RecurrenceData"];
            }
            if (data.ContainsKey("fAllDayEvent"))
            {
                listItem["fAllDayEvent"] = data["fAllDayEvent"];
            }
            if (data.ContainsKey("fRecurrence"))
            {
                listItem["fRecurrence"] = data["fRecurrence"];
            }
            if (data.ContainsKey("RRule"))
            {
                listItem["RRule"] = data["RRule"];
            }
            if (data.ContainsKey("ExRule"))
            {
                listItem["ExRule"] = data["ExRule"];
            }
            if (data.ContainsKey("SuppressUntil"))
            {
                listItem["SuppressUntil"] = data["SuppressUntil"];
            }
            if (data.ContainsKey("IsOrphaned"))
            {
                listItem["IsOrphaned"] = data["IsOrphaned"];
            }
            if (data.ContainsKey("IsException"))
            {
                listItem["IsException"] = data["IsException"];
            }
            if (data.ContainsKey("IsDetached"))
            {
                listItem["IsDetached"] = data["IsDetached"];
            }
            if (data.ContainsKey("Sequence"))
            {
                listItem["Sequence"] = data["Sequence"];
            }
            if (data.ContainsKey("DTStamp"))
            {
                listItem["DTStamp"] = new DateTime(((DateTime)data["DTStamp"]).Ticks, DateTimeKind.Utc);
            }
            if (data.ContainsKey("InstanceID"))
            {
                listItem["InstanceID"] = data["InstanceID"];
            }
            if (data.ContainsKey("EventUID"))
            {
                listItem["EventUID"] = data["EventUID"].ToString();
            }
            if (data.ContainsKey("Organizer"))
            {
                listItem["Organizer"] = data["Organizer"];
            }
            if (data.ContainsKey("EventUrl") && data.ContainsKey("EventUrl#2"))
            {
                FieldUrlValue tValue = new FieldUrlValue();
                tValue.Description = data["EventUrl#2"].ToString();
                tValue.Url = data["FieldUrlValue"].ToString();
                listItem["EventUrl"] = tValue;
                //userData.Remove("EventUrl");
                //userData.Remove("EventUrl#2");
            }
            //DOC-68017,新建的item instanceId和源端不一致，导致Attendees下的item还原过去了，但是看不到。
            //if (data.ContainsKey("#tp_InstanceID"))
            //{
            //    int sInstanceId = (int)userData["#tp_InstanceID"];
            //    int dInstanceId = (int)listItem["InstanceID"];
            //    if (sInstanceId != dInstanceId)
            //    {
            //        mSite.DBService.ChangeInstanceIdByNative(info, listItem, sInstanceId);
            //        listItem = mList.GetItemById(listItem.ID);
            //    }
            //}

            return listItem;
        }

        private ListItem UpdateCalendarEvent(ListItem listItem, Dictionary<string, object> userData)
        {
            string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
            Dictionary<string, object> modifiedData = new Dictionary<string, object>();
            modifiedData["ModerationStatus"] = mModerationStatus;
            modifiedData["Modified"] = userData.ContainsKey("Modified") ? userData["Modified"] : null;
            modifiedData["fRecurrence"] = userData["fRecurrence"];
            string recurrenceData = userData["RecurrenceData"].ToString().Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
            string[] recurrencePropertyNames = new string[] { "firstDayOfWeek", "dayFrequency", "repeatForever", "repeatInstances", "windowEnd", "weekFrequency",
                                                              "monthFrequency", "monthlyByDay", "weekdayOfMonth", "yearFrequency", "yearlyByDay" };
            foreach (string recurrencePropertyName in recurrencePropertyNames)
            {
                recurrenceData = recurrenceData.Replace(recurrencePropertyName.ToLower(), recurrencePropertyName);
            }
            modifiedData["RecurrenceData"] = recurrenceData;

            if (mRequest.TokenProvider.TokenType != TokenType.Bearer)
            {
                mRequest.WebServiceRequestOnline.UpdateListItems(webAppName, mParentWebUrl, mListTitle, listItem.Id, listItem["FileRef"].ToString(), modifiedData);

                if (listItem.FieldValues.ContainsKey("_ModerationStatus")
                     && mModerationStatus != Convert.ToInt32(listItem.FieldValues["_ModerationStatus"]))
                {
                    mContext.Load(listItem);
                    mContext.ExecuteQuery();
                }
            }
            else
            {
                listItem["_ModerationStatus"] = mModerationStatus;
                listItem["fRecurrence"] = modifiedData["fRecurrence"];
                listItem["RecurrenceData"] = modifiedData["RecurrenceData"];
                //listItem["_ModerationComments"] = 
                listItem["Modified"] = new DateTime(((DateTime)modifiedData["Modified"]).Ticks, DateTimeKind.Utc);
                listItem.Update();
                mContext.Load(listItem);
                mContext.ExecuteQuery();
            }

            return listItem;
        }

        internal ListItem UpdateListItem(ListItem listItem, Dictionary<string, object> userData, ListItemUpdateMethodKind updateMethodKind, bool change)
        {
            int parentModerationStatus = -1;
            if (userData.ContainsKey("Parent_ModerationStatus"))
            {
                parentModerationStatus = (int)userData["Parent_ModerationStatus"];
                userData.Remove("Parent_ModerationStatus");
            }
            if (mEnableModeration)
            {
                userData["_ModerationStatus"] = parentModerationStatus != -1 ? parentModerationStatus : mModerationStatus;
            }
            RestoreItemFields(ref listItem, userData, updateMethodKind);

            if (change)
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mListId.ToString();
                string fileName = listItem["FileRef"].ToString();
                string op = "TakeOffline";

                mRequest.OperateOnVersion(mParentWebUrl, webAppName, mRequest.TokenProvider, mListDefaultViewUrl, listItem.Id, (int)listItem["_UIVersion"], listId, fileName, op);
            }
            try
            {
                if (mListTemplate == (int)ListTemplateType.Events && userData.ContainsKey("fRecurrence") && userData.ContainsKey("RecurrenceData"))
                {
                    UpdateCalendarEvent(listItem, userData);
                }
            }
            /*review-qlluo*/catch (Exception ex)
            {
                mLogger.Error("Update list item failed.Error Message:{0}", ex.ToString());
                listItem = mParentList.GetItemById(listItem.Id);
            }

            return listItem;
        }

        private static bool RestoreTaxonomyFields(ListItem listItem, List<Dictionary<string, object>> taxonomyFields)
        {
            bool hasKeyword = false;
            foreach (Dictionary<string, object> taxonomyField in taxonomyFields)
            {
                string internalName = taxonomyField["FieldName"].ToString();
                if (string.Equals(internalName, "TaxKeyword", StringComparison.OrdinalIgnoreCase))
                {
                    hasKeyword = true;
                }
                bool allowMultipleValues = Convert.ToBoolean(taxonomyField["AllowMultipleValues"].ToString());
                string text = null;
                if (allowMultipleValues)
                {
                    List<string> fieldValues = taxonomyField["Text"] as List<string>;
                    StringBuilder values = new StringBuilder();
                    if (fieldValues.Count > 0)
                    {
                        values.Append(fieldValues[0]);
                        for (int i = 1; i < fieldValues.Count; i++)
                        {
                            values.Append(";").Append(fieldValues[i]);
                        }
                    }
                    text = values.ToString();
                }
                else
                {
                    text = taxonomyField["Text"].ToString();
                }
                mLogger.Info("Taxonomy field value,{0},{1}", taxonomyField["FieldName"], text);
                listItem.ParentList.Fields.GetByInternalNameOrTitle(internalName).ValidateSetValue(listItem, text);
            }
            return hasKeyword;
        }

        internal ListItem UpdateToSpecificVersion(ListItem listItem, int originalVersion, bool deleteCurrentVersion, Dictionary<string, object> userData)
        {
            List<int> skipVersionLabels = new List<int>();

            int destVersion = (int)listItem["_UIVersion"];

            if (deleteCurrentVersion)
            {
                skipVersionLabels.Add(destVersion);
            }

            bool change = false;

            if (mEnableModeration && (int)listItem["_ModerationStatus"] == 1)
            {
                listItem["_ModerationStatus"] = 0;
                listItem.Update();
                change = true;
            }

            int updateCount = 0;
            //update to current version when approve is enable otherwise update to the previous version
            while (mEnableModeration ? originalVersion > destVersion : originalVersion - destVersion > 512)
            {
                listItem.Update();
                destVersion += 512;
                if (originalVersion > destVersion)
                {
                    skipVersionLabels.Add(destVersion);
                }
                if (updateCount++ == 20)
                {
                    updateCount = 0;
                    mContext.ExecuteQuery();
                }
            }
            UpdateListItem(listItem, userData, ListItemUpdateMethodKind.Update, change);

            if (skipVersionLabels.Count > 0)
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string fileName = listItem.FieldValues["FileRef"].ToString();
                string op = string.Empty;
                //SAAS-28934 添加判断条件，删除listitem所有version的情况.
                //if (!WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                //{
                //    op = "DeleteAll";
                //    mLogger.Info("We will delete all history version.Item Id:{0}, File name:{1}.",listItem.Id,fileName);
                //    mRequest.DeleteListItemVersions(mParentWebUrl, webAppName, mRequest.TokenProvider, mListDefaultViewUrl, listItem.Id, mListId.ToString(), fileName, op);
                //}
                //else
                //{
                //    op = "Delete";
                //    for (int i = 0; i < skipVersionLabels.Count; i++)
                //    {
                //        mRequest.OperateOnVersion(mParentWebUrl, webAppName, mRequest.TokenProvider, mListDefaultViewUrl, listItem.Id, skipVersionLabels[i], mListId.ToString(), fileName, op);
                //    }
                //}
                mRequest.DeleteHistoryVersions(mParentWebUrl, mListId, listItem.Id, skipVersionLabels);
                
            }

            return listItem;
        }

        private void RestoreItemFields(ref ListItem item, Dictionary<string, object> fields, ListItemUpdateMethodKind updateMethodKind)
        {
            if (item != null)
            {
                bool isChannelFolder = false;
                try
                {
                    if (TryGetFieldValue(item, "ProgId", out string value))
                    {
                        if (value.Equals("Team.Channel"))
                        {
                            isChannelFolder = true;
                            mLogger.Warn("this folder is channel folder");
                        }
                    }
                    switch (updateMethodKind)
                    {
                        case ListItemUpdateMethodKind.Update:
                            bool changed = SetFieldValues(ref item, fields, mEnableModeration, true);
                            if (changed)
                            {
                                //item.Update();
                                mContext.Load(item);
                                mContext.ExecuteQuery();
                            }
                            break;
                        case ListItemUpdateMethodKind.SystemUpdate:
                            Dictionary<string, object> itemProp = new Dictionary<string, object>();

                            itemProp["ChangedFieldValues"] = fields;

                            item = AveClientOM2013Request.InternUpdate(mParentList, item.Id, itemProp);
                            mContext.Load(item);
                            mContext.ExecuteQuery();
                            break;
                        default:
                            SetFieldValues(ref item, fields, mEnableModeration, true);
                            break;
                    }
                }
                catch (Exception ex)
                {

                    if (isChannelFolder)
                    {
                        mLogger.Error("this folder is channel folder");
                        throw new TeamChannalFolderUpdateFailed();
                    }
                    else
                    {
                        mLogger.Error("{0}:Restore Item fields failed.Error Message:{1}", updateMethodKind, ex.ToString());
                        //Don't throw exception when folder restore fields failed.
                        //throw;
                    }
                }
            }
        }
        private bool TryGetFieldValue(ListItem item, string fieldName, out string value)
        {
            value = null;
            object objVal;
            if (item.FieldValues.TryGetValue(fieldName, out objVal))
            {
                value = objVal?.ToString();
                return true;
            }
            return false;
        }
        /// <summary>
        /// SAAS-34646 If Column name is "Folder" or "File", update column value will encounter error : Invalid Request
        /// Need to use method ValidateUpdateListItem to update the column value
        /// Column Names are case sensitive
        /// </summary>
        private static List<String> ReservedFieldNames = new List<string>() { "Folder", "File", "ComplianceInfo", "Versions", "AttachmentFiles", "IconOverlay" };

        internal static bool SetFieldValues(ref ListItem item, Dictionary<string, object> fields, bool mEnableModeration, bool needItemUpdate)
        {
            //StringBuilder sb = new StringBuilder();
            //foreach (KeyValuePair<string, object> field in fields)
            //{
            //    string value = field.Value != null ? field.Value.ToString() : "IsNull";
            //    sb.AppendLine(field.Key + ":" + value);
            //}
            //mLogger.Info("Field Values:{0}", sb);

            mLogger.Info("Field Values:{0}", FormatOutput.Process(fields.Where(field => !string.Equals(field.Key, "FileLeafRef", StringComparison.OrdinalIgnoreCase) //Do not display file name in log
                                                                                     && !string.Equals(field.Key, "Title", StringComparison.OrdinalIgnoreCase))));

            bool changed = false;
            IList<ListItemFormUpdateValue> reservedFieldvalues = new List<ListItemFormUpdateValue>();
            if (fields.ContainsKey("NeedSetNullFields"))
            {
                List<string> setToNullFields = fields["NeedSetNullFields"] as List<string>;
                foreach (string fieldName in setToNullFields)
                {
                    if (ReservedFieldNames.Contains(fieldName))
                    {
                        reservedFieldvalues.Add(new ListItemFormUpdateValue() { FieldName = fieldName, FieldValue = string.Empty });
                    }
                    else
                    {
                        item[fieldName] = null;
                    }
                }
                fields.Remove("NeedSetNullFields");
            }

            if (fields.ContainsKey("TaxonomyFields"))
            {
                mLogger.Debug("TaxonomyFields");
                bool hasKeyword = RestoreTaxonomyFields(item, fields["TaxonomyFields"] as List<Dictionary<string, object>>);
                if (mEnableModeration && hasKeyword) //SAAS-3652 item有多个version时，mModerationStatus 不是0 ，目的端List开启enterprise keywords setting,会导致taxkeyword field的值更新失败
                {
                    item["_ModerationStatus"] = 0;
                    item.Update();
                }
                fields.Remove("TaxonomyFields");
            }
            //mLogger.Info("After SetNeedNullFields and TaxonomyFields Field Values:[{0}]", FormatOutput.Process(fields));
            foreach (KeyValuePair<string, object> field in fields)
            {
                try
                {
                    if (ReservedFieldNames.Contains(field.Key))
                    {
                        reservedFieldvalues.Add(new ListItemFormUpdateValue() { FieldName = field.Key, FieldValue = field.Value as string });
                        continue;
                    }
                    if (field.Key.Equals("File_x0020_Type") && field.Value.ToString().StartsWith("arc_", StringComparison.OrdinalIgnoreCase))
                    {
                        item[field.Key] = field.Value.ToString().Substring(4);
                        continue;
                    }
                    else if (field.Key.Equals("Content_x0020_Archived"))
                    {
                        item[field.Key] = false;//for DOC-56959, If backup is archived file, restore it as real data.
                        continue;
                    }
                    else if (field.Key.Equals("_dlc_BarcodePreview"))
                    {
                        FieldUrlValue fieldUrl = new FieldUrlValue();
                        fieldUrl.Url = field.Value.ToString();
                        fieldUrl.Description = field.Value.ToString().Substring(field.Value.ToString().LastIndexOf("Barcode"));
                        item[field.Key] = fieldUrl;
                    }
                    //else if (field.Key.Equals("WikiField"))
                    //{
                    //    continue;
                    //}
                    else if (field.Value is DateTime)
                    {
                        //1/01/1900 
                        DateTime utcDateTime = new DateTime(((DateTime)field.Value).Ticks, DateTimeKind.Utc);
                        DateTime minimalDatetime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        if (utcDateTime < minimalDatetime)
                        {
                            item[field.Key] = minimalDatetime;
                        }
                        else
                        {
                            item[field.Key] = utcDateTime;
                        }
                    }
                    else if (field.Key.Equals("ContentType"))
                    {
                        item["ContentTypeId"] = field.Value;
                    }
                    else
                    {
                        item[field.Key] = field.Value;
                    }
                    changed = true;
                }
                catch (Exception ex)
                {
                    mLogger.Info(AveClientOMRequestResource.SetFieldValue, string.Empty, field.Key, ex.ToString());
                }
            }
            if (reservedFieldvalues.Count > 0)
            {
                item.ValidateUpdateListItem(reservedFieldvalues, false, "", true, true, string.Empty);
            }
            else if (needItemUpdate)
            {
                item.Update();
            }
            return changed;
        }

       /* private static void SetFieldValueToNull(Dictionary<string, object> userDataAllFields, List<string> needSetToNullFields)
        {
            if (!userDataAllFields.Any() && !needSetToNullFields.Any()) return;
            try
            {
                foreach (string needSetToNullField in needSetToNullFields)
                {
                    userDataAllFields[needSetToNullField] = null;
                    //if (userDataAllFields.Any(it => String.Equals(it.Key, needSetToNullField, StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    object value=userDataAllFields[needSetToNullField];
                    //    value = null;
                    //}
                }
            }
            catch(Exception e)
            {
                mLogger.Error("ListItemRestore SetFieldValueToNull failed,Error:[{0}] , StackTrace:[{1}]",e.Message,e.StackTrace);
            }
        }*/

        /*private static void SetFieldValueToNull(ListItem item, List<string> fields)
        {
            if (fields != null)
            {
                try
                {
                    foreach (string fieldName in fields)
                    {
                        item[fieldName] = null;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.SetFieldValueToNullError, item.DisplayName, e.ToString());
                    //mLogger.Debug("An error occured while SetFieldValueToNull. error:{0}", e.ToString());
                }
            }
        }*/

        /// <summary>
        /// 对于某些特殊的List模板，ListItem重复不能通过id来判断，否则会导致异常，通过此方法进行特殊处理
        /// </summary>
        /// <param name="checkProperties">该属性需要针对不同模板在调用处赋值</param>
        private bool CheckItemExistenceForSpecialListTemplateType(List list, int listTemplate, ref ListItem listItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCollection listItems = null;
            ListItem tempItem = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904"));
            CamlQuery camelQueyr = new CamlQuery();
            if (listTemplate == 880)     //Community Member list, SAAS-1478
            {
                camelQueyr.ViewXml =
                   string.Format("<View><Query><Where><Eq><FieldRef Name=\"Member\" LookupId=\"True\" /><Value Type=\"User\">{0}</Value></Eq></Where></Query></View>"
                                 , userData["Member"]);    //由于该List下不允许相同的Member存在，因此需要通过该属性判断是否存在
            }
            retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    listItems = list.GetItems(camelQueyr);
                    mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.ComplianceInfo));
                    mContext.ExecuteQuery();
                });
            bool exist = listItems != null && listItems.Count > 0;
            tempItem = exist ? listItems[0] : null;
            listItem = tempItem;
            return listItem != null;
        }

        private bool NeedCheckItemExistenceForSpecialListTemplateType(int listTemplate)
        {
            return listTemplate == 880;
        }

        internal bool IsItemExsit(List list, int listTemplate, Guid tpGuid, int rowId, int destRowId, ref ListItem listItem)
        {
            try
            {
                if (mUniqueValues != null && mUniqueValues.Count > 0)
                {
                    List<ListItem> items = new List<ListItem>();
                    string queryString = BuildQueryString(mUniqueValues);
                    CamlQuery query = new CamlQuery();
                    query.FolderServerRelativePath = mParentFolderRelativePath;
                    query.DatesInUtc = true;
                    query.ViewXml = queryString;
                    ListItemCollection listItems = list.GetItems(query);
                    list.Context.Load(listItems, ls => ls.IncludeWithDefaultProperties(item => item.ComplianceInfo));
                    list.Context.ExecuteQuery();
                    items.AddRange(listItems.Where(IsConflictItem));
                    if (items.Count > 0 && WrapperConfiguration.WrapperConfigurationForBPOS.UniqueFieldsResolution.RestorationOption == UniqueFieldRestorationOption.Skip)
                    {
                        throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Current ListItem is skipped because duplicate values were found in the field(s) that enabled unique value.");
                    }
                    bool exist = listItems != null && listItems.Count > 0;
                    listItem = exist ? listItems[0] : null;
                    if (exist)
                    {
                        return exist;
                    }
                }
                if (listTemplate == (int)ListTemplateType.Survey)
                {
                    CamlQuery camelQuery = new CamlQuery();
                    camelQuery.ViewXml =
                           string.Format("<View><Query><Where><And><Eq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                           rowId, mParentFolderRelativeUrl);//通过guid找item必须有其parent信息，因为同一个list不同folder下可以有相同Guid的item
                    ListItemCollection listItems = list.GetItems(camelQuery);
                    mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, item => item.ComplianceInfo));
                    mContext.ExecuteQuery();
                    bool exist = listItems != null && listItems.Count > 0;
                    listItem = exist ? listItems[0] : null;
                    return exist;
                }
                else
                {
                    ListItemCollection listItems = null;
                    ListItem tempItem = null;
                    //Load item时抛ServerException(HRESULT: 0x80131904)会导致listItem version还原出错及listItem的属性未初始化异常，所以在这里如果捕获到该异常进行Retry，SAAS-630 & SAAS-252

                    AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904"));
                    retryHelper.ExecuteWithRetryMechanism(() =>
                        {
                            if (destRowId == -1)
                            {
                                if (listTemplate == (int)ListTemplateType.DesignCatalog)
                                {
                                    CamlQuery camelQueyr = new CamlQuery();
                                    camelQueyr.DatesInUtc = true;
                                    camelQueyr.ViewXml =
                                           string.Format("<View><Query><Where><And><Eq><FieldRef Name=\"Name\"/><Value Type=\"Text\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                                           mName, mParentFolderRelativeUrl);//find item by name in composed look list 
                                    listItems = list.GetItems(camelQueyr);
                                    mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.ComplianceInfo));
                                    mContext.ExecuteQuery();
                                    bool exist = listItems != null && listItems.Count > 0;
                                    tempItem = exist ? listItems[0] : null;
                                }
                                else
                                {
                                    if (mRowId != -1)
                                    {
                                        CamlQuery camelQuery = new CamlQuery();
                                        camelQuery.ViewXml =
                                               string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Eq></Where></Query></View>",
                                               mRowId);//通过guid找item必须有其parent信息，因为同一个list不同folder下可以有相同Guid的item
                                        listItems = list.GetItems(camelQuery);
                                        mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, items => items.ComplianceInfo));
                                        mContext.ExecuteQuery();
                                        bool exist = listItems != null && listItems.Count > 0;
                                        tempItem = exist ? listItems[0] : null;
                                    }
                                    else
                                    {
                                        tempItem = null;
                                    }
                                }
                            }
                            else
                            {
                                CamlQuery camelQuery = new CamlQuery();
                                camelQuery.ViewXml =
                                               string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Eq></Where></Query></View>",
                                               destRowId);//通过guid找item必须有其parent信息，因为同一个list不同folder下可以有相同Guid的item
                                listItems = list.GetItems(camelQuery);
                                mContext.Load(listItems, 
                                    items => items.IncludeWithDefaultProperties(
                                        item => item.HasUniqueRoleAssignments, 
                                        items => items.ComplianceInfo,
                                        item => item.Properties
                                        ));
                                mContext.ExecuteQuery();
                                bool exist = listItems != null && listItems.Count > 0;
                                tempItem = exist ? listItems[0] : null;
                                //仅根据destRowId判断，不同list下相同rowIDitem会被覆盖，增加GUID判断，即可正确还原   [SAAS-23183] zma
                                if (tempItem != null)
                                {
                                    if ((mRestoreOption & AveRestoreOption.Append) == AveRestoreOption.Append 
                                    && tempItem.Properties?.FieldValues?.TryGetValue("AppendGUID", out var appendGuid) == true)
                                    {
                                        tempItem = string.Equals(appendGuid.ToString(), mGuid.ToString(), StringComparison.OrdinalIgnoreCase) ? tempItem : null;
                                    }
                                    else if (tempItem.FieldValues.ContainsKey("GUID"))
                                    {
                                        tempItem = (Guid)tempItem["GUID"] == mGuid ? tempItem : null;
                                    }
                                }
                            }
                        });
                    listItem = tempItem;
                    return listItem != null;
                }
            }
            catch (AveRestoreException ex)
            {
                mLogger.Warn("Item:{0} is should skipped.Error Message:{1}", tpGuid, ex.ToString());
                throw ex;
            }
            /*review-qlluo*/catch (Exception ex)
            {
                mLogger.Error("Item:{0} is not exist.Error Message:{1}", tpGuid, ex.ToString());
                listItem = null;
                return false;
            }
        }

        internal static void SystemUpdate(ListItem item)
        {
            Type[] argsTypes = new Type[] { typeof(bool), typeof(bool), typeof(Guid), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) };
            object[] args = new object[] { false, true, Guid.Empty, false, false, true, false, false, false };
            if ((int)item["_Level"] == (int)FileLevel.Published)
            {
                args[3] = true;
                args[4] = true;
            }
            AveAssemblyUtility.InvokeMethod(item, item.GetType(), "UpdateInternal", argsTypes, args);
        }

        public void Dispose()
        {
            if (mContext.HasPendingRequest)
            {
                AveAssemblyUtility.SetFieldValue(mContext, typeof(ClientRuntimeContext), "m_request", null);
            }
        }

        /// <summary>
        /// 处理将冲突文件添加到冲突文件夹下的逻辑。
        /// 1.先判断item是否存在，不存在直接返回。
        /// 2.先判断item所在的file是否存在，不存在直接返回。
        /// 3.之后判断conflict folder是否存在，不存在则建一个conflict folder
        /// 4.使用moveto方法将file移到conflict folder下
        /// 5.keep 一些属性如created time等
        /// </summary>
        private void MoveToConflictFolder()
        {
            try
            {
                mContext.Load(mParentList);
                mContext.ExecuteQuery();
                if (!mParentList.ServerTemplateCanCreateFolders)
                {
                    return;
                }
                if (!mParentList.EnableFolderCreation)
                {
                    mParentList.EnableFolderCreation = true;
                    mParentList.Update();
                }
                ListItem item = null;
                bool itemExist = IsItemExsit(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref item);
                if (!itemExist)
                {
                    return;
                }
                Microsoft.SharePoint.Client.File file = null;
                if (item.FieldValues.ContainsKey("FileRef"))
                {
                    try
                    {
                        file = mParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(item["FileRef"].ToString()));
                        mContext.Load(file);
                        mContext.Load(file.ListItemAllFields);
                        mContext.ExecuteQuery();
                        if (!file.Exists)
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Can not get file from item:{0},error:{1}", item["FileRef"].ToString(), e.ToString());
                        return;
                    }
                }
                else
                {
                    return;
                }
                Folder mParentFolder = mParentWeb.GetFolderByServerRelativePath(mParentFolderRelativePath);
                try
                {
                    mContext.Load(mParentFolder);
                    mContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Info("Parent folder not exist,folderUrl:{0},error:{1}", mParentFolderRelativeUrl, e.ToString());
                    mParentFolder = null;
                    return;
                }
                Dictionary<string, object> needKeepFields = new Dictionary<string, object>();
                needKeepFields.Add("Modified", item["Modified"]);
                needKeepFields.Add("Created", item["Created"]);
                needKeepFields.Add("Author", item["Author"]);
                needKeepFields.Add("Editor", item["Editor"]);
                string conflictFolderUrl = mParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                Microsoft.SharePoint.Client.Folder folder = mParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(conflictFolderUrl));
                try
                {
                    mContext.Load(folder);
                    mContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Info("Conflict folder not exist,folderUrl:{0},error:{1}", conflictFolderUrl, e.ToString());
                    folder = null;
                }
                #region --AddConflictFolder--
                if (folder == null)
                {
                    if (mParentList != null && mListBaseType != (int)BaseType.DocumentLibrary)
                    {
                        ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
                        creationInfoUsingPath.FolderPath = ResourcePath.FromDecodedUrl(mParentFolder.ServerRelativeUrl);
                        creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInfoUsingPath.LeafName = ResourcePath.FromDecodedUrl(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        ListItem listItem = mParentList.AddItemUsingPath(creationInfoUsingPath);
                        listItem["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        listItem.Update();
                        folder = mParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(conflictFolderUrl));
                        mContext.Load(listItem);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                    else
                    {
                        FolderCollectionAddParameters folderAddParam = new FolderCollectionAddParameters();
                        folderAddParam.Overwrite = mOverWrite;
                        folder = mParentFolder.Folders.AddUsingPath(ResourcePath.FromDecodedUrl(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME),folderAddParam);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                }
                #endregion
                string moveFileName = AveSPUtility.GetConflictNewName(item["FileLeafRef"].ToString(), file.TimeLastModified);
                string moveFileTitle = string.Empty;
                if (item["Title"] != null)
                {
                    moveFileTitle = AveSPUtility.GetConflictNewName(item["Title"].ToString(), file.TimeLastModified);
                }
                else
                {
                    moveFileTitle = moveFileName;
                }
                string moveFileUrl = conflictFolderUrl + "/" + moveFileName;
                file.MoveTo(moveFileUrl, MoveOperations.None);
                mContext.Load(file);
                mContext.ExecuteQuery();
                needKeepFields.Add("Title", moveFileTitle);
                UpdateListItem(file.ListItemAllFields, needKeepFields, ListItemUpdateMethodKind.SystemUpdate, false);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move item:{0} to Conflict folder failed,error:{1}", (mParentFolderRelativeUrl + "/" + mTitle), ex.ToString());
            }
        }
    }
}
