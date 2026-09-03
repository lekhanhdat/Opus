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


using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2013ListItemRestore : AveListItemRestore, IDisposable
    {
        protected static AveLogger mLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private Site mSite;
        //private Web mParentWeb;
        //private List mParentList;
        //private string mParentFolderRelativeUrl;
        //private int mRowId;
        //private int mDestRowId;
        //private Guid mGuid;
        //private int mModerationStatus;
        //private bool mIsNewCreated;
        //private int mOrginalVersion;
        //private byte mOriginalLevel;
        //private bool mOverWrite;
        //private int mDraftOwnerId;
        //private int mCheckoutUserId;
        //private ClientContext mContext;
        //private object mObj;
        //private string mTitle;
        private string mName;
        private int mMemberID;
        //private int mListTemplate;
        //private bool mEnableModeration;
        //private bool mEnableVersioning;
        //private bool mMOVE_ITEM_TO_CONFLICT_FOLDER;
        //private bool mSKIP_IF_SAME_MODIFIEDTIME;
        //private bool mOverwriteByLastModifiedTime;
        //private AveRestoreOption mRestoreOption;
        //private AveClientOM2013Request mRequest;

        /// <summary>
        /// 为unittest提供构造方法
        /// </summary>
        public Ave2013ListItemRestore() { }

        public Ave2013ListItemRestore(AveClientOMRequest request, Site site, ClientContext conText, object obj)
        {
            mRequest = request;
            mSite = site;
            mContext = conText;
            mObj = obj;
        }

        public Ave2013ListItemRestore(AveClientOMRequest request, Site site, Web web, List list, int rowId, int moderationStatus, ClientContext context, object obj)
        {
            mRequest = request;
            mSite = site;
            mContext = context;
            mParentWeb = web;
            mParentList = list;
            mRowId = rowId;
            mModerationStatus = moderationStatus;
            mObj = obj;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of Keys")]
        protected override void PrepareRestoreContext(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            mParentFolderRelativeUrl = data["FolderUrl"] as string;
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mAveWebCache = data.ContainsKey("AveWebObject") ? (IAveWeb)data["AveWebObject"] : null;
            mParentList = null;
            Guid listId;
            if (TryGetListId(data, out listId))
            {
                if (!listId.Equals(Guid.Empty))
                {
                    mParentList = mParentWeb.Lists.GetById(listId);
                }
            }
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mDestRowId = data.ContainsKey("DestRowId") ? (int)data["DestRowId"] : -1;
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
            if (mListTemplate == AveCommunitiesConstants.MembersList_TemplateType && userData != null && userData.ContainsKey("Member") && userData["Member"] is String)
            {
                if (!int.TryParse(userData["Member"] as string, out mMemberID))
                {
                    mMemberID = -1;
                }
            }
            mListTemplate = data.ContainsKey("ListTemplate") ? (int)data["ListTemplate"] : -1;
            mEnableModeration = data.ContainsKey("ListEnableModeration") ? Convert.ToBoolean(data["ListEnableModeration"]) : false;
            mEnableVersioning = data.ContainsKey("ListEnableVersioning") ? Convert.ToBoolean(data["ListEnableVersioning"]) : false;
            mMOVE_ITEM_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_ITEM_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_ITEM_TO_CONFLICT_FOLDER"]) : false;
            mMOVE_SOURCE_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_SOURCE_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_SOURCE_TO_CONFLICT_FOLDER"]) : false;
            mOverwriteByLastModifiedTime = data.ContainsKey("OverwriteByLastModifiedTime") ? Convert.ToBoolean(data["OverwriteByLastModifiedTime"]) : false;
            mRestoreOption = data.ContainsKey("RestoreOption") ? (AveRestoreOption)Enum.Parse(typeof(AveRestoreOption), data["RestoreOption"].ToString()) : AveRestoreOption.Default;
            mSKIP_IF_SAME_MODIFIEDTIME = data.ContainsKey("SKIP_IF_SAME_MODIFIEDTIME") ? Convert.ToBoolean(data["SKIP_IF_SAME_MODIFIEDTIME"]) : false;
            LoadContextInfo();
        }
        private void LoadContextInfo()
        {
            //mContext.Load(mParentWeb);
            mContext.Load(mParentList);
            mContext.Load(mParentList, list => list.DefaultViewUrl,
                                       list => list.ContentTypes.Include(ct => ct.StringId));
            mContext.ExecuteQuery();
        }

        internal override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            try
            {
                PrepareRestoreContext(data, userData);
                ListItem listItem = null;
                //处理conflict folder
                if (mMOVE_ITEM_TO_CONFLICT_FOLDER)
                {
                    MoveToConflictFolder();
                }
                bool deleteCurrentVersion = false;
                bool itemExist = false;
                object fieldDisplayName;
                if (data.TryGetValue("MatchItemFieldDisplayName", out fieldDisplayName))
                {
                    var field = this.mParentList.Fields.GetByTitle(fieldDisplayName.ToString());
                    mContext.Load(field);
                    mContext.ExecuteQuery();
                    listItem = GetItemByFieldValue(field.InternalName, field.TypeAsString, userData[field.InternalName]);
                    if (listItem != null)
                    {
                        itemExist = true;
                    }
                }
                else
                {
                    itemExist = IsItemExist(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref listItem);
                }
                if (itemExist && CheckFileLockStatus(listItem))
                {
                    if (!this.mOverWrite)
                    {
                        throw new RestoreResultException(RestoreResult.SkipConflict, "Skip restoring item because it is locked in destination");
                    }
                    //文件Unlock之后重新Load文件的信息
                    UnlockFile(listItem);
                    itemExist = IsItemExist(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref listItem);
                }

                if (itemExist && mOverwriteByLastModifiedTime && data.ContainsKey("BiggestVersionModified")
                    && (DateTime)data["BiggestVersionModified"] <= (DateTime)listItem.FieldValues["Modified"])
                {
                    //Overwrite item by LastModifiedTime.
                    restoreResult["SkippedByLastModifiedTime"] = true;
                    restoreResult["RestoreStatus"] = true;
                    return restoreResult;
                }
                if (itemExist && mRestoreOption.Equals(AveRestoreOption.Append) && !mIsNewCreated &&
                    (!mSKIP_IF_SAME_MODIFIEDTIME || (DateTime)data["BiggestVersionModified"] != (DateTime)listItem.FieldValues["Modified"]))
                {
                    //Need append a new item.
                    listItem = null;
                }
                if (listItem != null && mOverWrite)
                {
                    listItem.DeleteObject();
                    listItem = null;
                }

                if (listItem == null)
                {
                    listItem = AddListItem(data, userData, AddItemMapping);

                    deleteCurrentVersion = true;
                }
                if (mOverWrite || mIsNewCreated)
                {
                    if (mOrginalVersion == Convert.ToInt32(listItem["_UIVersion"]))
                    {
                        if (!mIsNewCreated)
                        {
                            UpdateListItem(ref listItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                        }
                    }
                    else
                    {
                        listItem = UpdateToSpecificVersion(listItem, mOrginalVersion, deleteCurrentVersion, userData);
                        restoreResult["ListEnableVersioning"] = mParentList.EnableVersioning;
                    }
                }
                restoreResult["IsNewCreated"] = !mIsNewCreated ? data["IsNewCreated"] : true;
                restoreResult["RestoreStatus"] = true;
                if (mMOVE_SOURCE_TO_CONFLICT_FOLDER)//destination Win的情况的转移。
                {
                    MoveToConflictFolder();
                }
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                mRequest.GetItemDic(itemProp, listItem);
                restoreResult["Item"] = itemProp;
            }
            catch (Exception e)
            {
                restoreResult["Exception"] = e.ToString();//e.Message + "--" + e.StackTrace;
                restoreResult["ExceptionMessage"] = e.Message;
                restoreResult["RestoreStatus"] = false;
            }

            return restoreResult;
        }

        private ListItem GetItemByFieldValue(string fieldInternalName, string fieldType, object fieldValue)
        {
            string fieldValueStr = fieldValue.ToString();
            CamlQuery query = new CamlQuery();
            query.ViewXml = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='{1}'>{2}</Value></Eq></Where>", fieldInternalName, fieldType, fieldValueStr);
            ListItem item = null;
            var items = mParentList.GetItems(query);
            mContext.Load(items);
            mContext.ExecuteQuery();
            if (items != null && items.Count > 0)
            {
                item = items[0];
            }
            return item;
        }

        private void PreRestoreFields(Dictionary<string, object> userData, ref Dictionary<string, object> keepData)
        {
            if (mAveWebCache.WebTemplate.Equals(AveCommunitiesConstants.CommunityTemplateName)
                && this.mParentList != null && this.mParentList.BaseTemplate == (int)ListTemplateType.DiscussionBoard
                && mAveWebCache.Configuration == 0
                && mOrginalVersion == 512
                && userData.ContainsKey("Author") && userData.ContainsKey("Editor"))//Community site's discussion list add new item before restoring author and editor.
            {
                keepData["NewItemProperties"] = GetNeedPostFields(userData, new string[] { "Author", "Editor" });
            }

            else if (mParentList.BaseTemplate == (int)ListTemplateType.TasksWithTimelineAndHierarchy
                     && userData.ContainsKey("Author") && userData.ContainsKey("Editor"))
            {
                foreach (ContentType ct in mParentList.ContentTypes)
                {
                    if (ct.StringId.StartsWith("0x0107", StringComparison.OrdinalIgnoreCase))//tasks list with message content type need post restore author and editor.
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
                    int sourceRowId = Convert.ToInt32(parentItem["BestAnswerId"]);
                    if (mRowId == sourceRowId)
                    {
                        parentItem["BestAnswerId"] = listItem.Id;
                        parentItem.Update();
                    }
                    if (needKeepData.Count > 0)
                    {
                        if (needKeepData.ContainsKey("NewItemProperties"))
                        {
                            Dictionary<string, object> postProperties = needKeepData["NewItemProperties"] as Dictionary<string, object>;
                            UpdateListItem(ref listItem, postProperties, ListItemUpdateMethodKind.SystemUpdate, false);
                            needKeepData.Remove("NewItemProperties");
                        }
                        UpdateListItem(ref parentItem, needKeepData, ListItemUpdateMethodKind.SystemUpdate, false);
                    }
                }
            }
            else if (needKeepData.Count > 0)
            {
                UpdateListItem(ref listItem, needKeepData, ListItemUpdateMethodKind.SystemUpdate, false);
            }
        }
        internal override ListItem AddListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            ListItem listItem = null;
            ListItem parentItem = null;//记录需要keep一写data如modified time的parent item
            Dictionary<string, object> needKeepData = new Dictionary<string, object>();//存放parent item需要keep的data
            switch ((ListTemplateType)mListTemplate)
            {
                case ListTemplateType.DiscussionBoard:
                    if (userData.ContainsKey("ContentType") && (userData["ContentType"].ToString().StartsWith("0x012002") || userData["ContentType"].ToString().StartsWith("0x0107")))
                    {
                        listItem = AddDiscussionTopic(data, userData, ref parentItem, ref needKeepData);
                    }
                    else
                    {
                        mLogger.Debug("There are other content type in discussionboard.");
                        ListItemCreationInformation creationInformationspe = new ListItemCreationInformation();
                        creationInformationspe.FolderUrl = mParentFolderRelativeUrl;
                        creationInformationspe.UnderlyingObjectType = FileSystemObjectType.File;
                        listItem = mParentList.AddItem(creationInformationspe);
                    }
                    break;
                case ListTemplateType.MeetingUser:
                    listItem = AddMettingUser(data, userData);
                    break;
                case ListTemplateType.Meetings:
                    listItem = AddMettings(data, userData);
                    break;
                case ListTemplateType.Survey:
                    listItem = AddSurveyRespond(data, userData, needKeepData);
                    break;
                default:
                    ListItemCreationInformation creationInformation = new ListItemCreationInformation();
                    creationInformation.FolderUrl = mParentFolderRelativeUrl;
                    creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
                    listItem = AddItemByAPI(mParentList, creationInformation);
                    break;
            }

            // set GUID这个column要和create在一次update前进行，否则在update的时候模拟会抛出异常，真实365会update不上GUID的值。
            if (mGuid != Guid.Empty && mListTemplate != (int)ListTemplateType.Survey)
            {
                listItem["GUID"] = mGuid;
            }
            object isExceedListViewLookupThreshold = null;
            //ADO-180781 attendees list走此逻辑会报Attempted to use an object that has ceased to exist.的错。
            if ((ListTemplateType)mListTemplate != ListTemplateType.MeetingUser)
            {
                if (data.TryGetValue("IsExceedListViewLookupThreshold", out isExceedListViewLookupThreshold))
                {
                    // 当lookup column的数量超过了limitation的时候，先update和excute让listItem创建出来，然后在set column value。            
                    if ((bool)isExceedListViewLookupThreshold || (ListTemplateType)mListTemplate == ListTemplateType.Meetings)
                    {
                        listItem.Update();
                        mContext.Load(listItem);
                        mContext.ExecuteQuery();
                    }
                }
            }

            if (mOrginalVersion == 512)
            {
                PreRestoreFields(userData, ref needKeepData);
                RestoreItemFields(ref listItem, userData, ListItemUpdateMethodKind.None);

                if (mEnableModeration)
                {
                    listItem["_ModerationStatus"] = mModerationStatus;
                }
            }
            if (isExceedListViewLookupThreshold != null && (bool)isExceedListViewLookupThreshold)
            {
                SystemUpdateListItem(listItem);
            }
            else
            {
                listItem.Update();
            }
            mContext.Load(listItem);
            mContext.Load(listItem, it => it.HasUniqueRoleAssignments);
            HandleDeclareItem(listItem, userData);
            try
            {
                lock (lockObj)
                {
                    mContext.ExecuteQuery();
                }
            }
            catch (WebException ex)
            {
                mLogger.Debug("Can not add item[{0}]. Message:{1}", mGuid, ex.ToString());
                int interval = WrapperConfiguration.BPOS_S.ClientRequestRetryInterval;
                if (!AveExceptionHelper.IsConnectionException(ex) && !AveExceptionHelper.IsHTTP429Error(ex, ref interval) || mIsCurrentMethodRetried)
                {
                    return listItem;
                }
                Thread.Sleep(interval);
                mIsCurrentMethodRetried = true;
                mLogger.Debug("Retry Method:{0}", "AddListItem");
                return AveEventHelper.Retry(delegate { return AddListItem(data, userData, AddItemMapping); });
            }
            catch (ServerException ex)
            {
                mLogger.Debug("add list item:{0}\\{1}, ServerErrorCode:{2}, ServerErrorDetails:{3}, ServerErrorTraceCorrelationId:{4}, ServerErrorTypeName:{5}, ServerErrorValue:{6}, ServerStackTrace:{7}, Source:{8}, StackTrace:{9}, Message:{10}",
                    mParentFolderRelativeUrl,
                    mRowId,
                    ex.ServerErrorCode,
                    ex.ServerErrorDetails,
                    ex.ServerErrorTraceCorrelationId,
                    ex.ServerErrorTypeName,
                    ex.ServerErrorValue,
                    ex.ServerStackTrace,
                    ex.Source,
                    ex.StackTrace,
                    ex.Message);

                if (ex.ServerErrorCode == AveSPErrorCode.TP_E_CANCELLED_BY_EVENT_HANDLER)
                {
                    mLogger.Debug("Update ListItem in Project List of Project Web Database failed. error message:{0}", ex.Message);
                    int sleepTime = WrapperConfiguration.BPOS_S.SleepTime;
                    System.Threading.Thread.Sleep(sleepTime);
                    if (mGuid != Guid.Empty && mListTemplate != (int)ListTemplateType.Survey)
                    {
                        listItem["GUID"] = mGuid;
                    }

                    if (mOrginalVersion == 512)
                    {
                        PreRestoreFields(userData, ref needKeepData);
                        RestoreItemFields(ref listItem, userData, ListItemUpdateMethodKind.None);

                        if (mEnableModeration)
                        {
                            listItem["_ModerationStatus"] = mModerationStatus;
                        }
                    }
                    listItem.Update();
                    mContext.Load(listItem);
                    mContext.Load(listItem, it => it.HasUniqueRoleAssignments);
                    mContext.ExecuteQuery();
                }
                else
                {
                    if (AddItemMapping != null)//ADO-214121 发现虽然update field value失败了 但是item依然创建出来了，这导致guid的mapping数据不完全，因此导致item的每一个version都作为一个新的item add到了目的端
                    {
                        var listItemProperties = GetListItemByGuid(mGuid);
                        if (listItemProperties != null && listItemProperties.ContainsKey("UniqueId") && listItemProperties.ContainsKey("GUID") && listItemProperties.ContainsKey("ID"))
                        {
                            AddItemMapping((Guid)listItemProperties["UniqueId"], (Guid)listItemProperties["GUID"], (int)listItemProperties["ID"], data);
                        }
                    }

                    throw;
                }
            }
            finally
            {
                mIsCurrentMethodRetried = false;
            }
            if (AddItemMapping != null && mListTemplate != (int)ListTemplateType.Survey)
            {
                AddItemMapping((Guid)listItem["UniqueId"], (Guid)listItem["GUID"], listItem.Id, data);
            }

            PostRestoreFields(parentItem, listItem, needKeepData);
            mIsNewCreated = true;

            return listItem;
        }
        public Dictionary<string, object> GetListItemByGuid(Guid tp_Guid)
        {
            try
            {
                var query = new CamlQuery
                {
                    ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tp_Guid)
                };
                this.mRequest.SetCamlQueryFolderUrl(query, mParentFolderRelativeUrl);

                if (mParentList.ItemCount + 1 < this.mRequest.MaxItemsPerThrottledOperation)
                {
                    ListItemCollection listItems = mParentList.GetItems(query);
                    mContext.Load(listItems, items => items.IncludeWithDefaultProperties());
                    mContext.ExecuteQuery();
                    if (listItems != null && listItems.Count > 0)
                    {
                        var listItemProperty = new Dictionary<string, object>();
                        listItemProperty["UniqueId"] = listItems[0]["UniqueId"];
                        listItemProperty["GUID"] = listItems[0]["GUID"];
                        listItemProperty["ID"] = listItems[0].Id;
                        return listItemProperty;
                    }
                }
                else
                {
                    mContext.Load(mParentWeb, w => w.ServerRelativeUrl);
                    mContext.Load(mParentList.RootFolder, f => f.ServerRelativeUrl);
                    mContext.ExecuteQuery();

                    var filesMap = new Dictionary<string, ClientFile>();
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(mContext);
                    List<Dictionary<string, object>> itemList = new List<Dictionary<string, object>>();

                    this.mRequest.QueryItemsByQueryStringForLargeList(mContext, mParentList, mParentWeb.ServerRelativeUrl, mParentFolderRelativeUrl, scope, filesMap, itemList, query);
                    if (itemList.Count > 0)
                    {
                        return itemList[0];
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while get list item by guid. tp_Guid: {0}, error: {1}", tp_Guid, e);
            }
            return null;
        }

        protected void SystemUpdateListItem(ListItem listItem)
        {
            Dictionary<string, object> itemProp = new Dictionary<string, object>();
            //添加一个ChangeFieldValue, 让底层走Update
            itemProp["ChangedFieldValues"] = new Dictionary<string, object> { { "ContentTypeId", listItem["ContentTypeId"] } };
            itemProp["EnableVersioning"] = mParentList.EnableVersioning;
            itemProp["EnableMinorVersions"] = mParentList.EnableMinorVersions;
            itemProp["EnableModeration"] = mParentList.EnableModeration;
            itemProp["FileSystemObjectType"] = (int)listItem.FileSystemObjectType;
            if (mParentList.EnableMinorVersions)
            {
                itemProp["IsCurrentMinorVersion"] = true;
            }
            if (mParentList.EnableModeration && (int)listItem["_ModerationStatus"] == 0)
            {
                itemProp["IsApproved"] = true;
            }
            if (mParentList.EnableMinorVersions && listItem.FileSystemObjectType == FileSystemObjectType.Folder
                && ((int)listItem["_UIVersion"] % 512 != 0 || (int)listItem["_UIVersion"] % 512 == 0 && !mParentList.EnableModeration))
            {
                itemProp["EnableVersioning"] = false;
            }
            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(mContext);
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                listItem = mRequest.InternUpdate(mParentList, listItem, itemProp, excepScope);
                mContext.ExecuteQuery();
            }
            if (excepScope.HasException)
            {
                mLogger.Warn(string.Format("System update list item failed. Reason:{0}", excepScope.ErrorMessage));
            }
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

        protected override ListItem AddDiscussionTopic(Dictionary<string, object> data, Dictionary<string, object> userData, ref ListItem parentItem, ref Dictionary<string, object> parentNeedKeepData)
        {
            ListItem item = null;
            if (data.ContainsKey("ParentThreadId"))
            {
                try
                {
                    ListItem repliedItem = mParentList.GetItemById(Convert.ToInt32(data["ParentThreadId"]));
                    mContext.Load(repliedItem);
                    mContext.ExecuteQuery();
                    item = mParentList.GetItemById(Convert.ToInt32(repliedItem["ParentFolderId"]));
                }
                catch (Exception ex)
                {
                    mLogger.Debug("An error occurred while getting the parent folder of reply, {0}.", ex.ToString());
                }
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
                object value;
                if (item.FieldValues.TryGetValue("DiscussionLastUpdated", out value))
                {
                    keepData["DiscussionLastUpdated"] = value;
                }
                if (item.FieldValues.TryGetValue("Modified", out value))
                {
                    keepData["Modified"] = value;
                }
                //if (item.FieldValues.ContainsKey("MyEditor"))
                //{
                //    keepData["MyEditor"] = item["MyEditor"];
                //}
                if (item.FieldValues.TryGetValue("Created", out value))
                {
                    keepData["Created"] = value;
                }
                if (item.FieldValues.TryGetValue("Editor", out value))
                {
                    keepData["Editor"] = value;
                }
                if (item.FieldValues.TryGetValue("Author", out value))
                {
                    keepData["Author"] = value;
                }
            }
            //ADO-111048, reply item does not have column: LastReplyBy. Setting it as null will throw exception, remove it.
            if (userData.ContainsKey("NeedSetNullFields") && userData["NeedSetNullFields"] != null)
            {
                (userData["NeedSetNullFields"] as List<string>).Remove("LastReplyBy");
                (userData["NeedSetNullFields"] as List<string>).Remove("RatedBy");
                (userData["NeedSetNullFields"] as List<string>).Remove("HashTags");
            }
            parentNeedKeepData = keepData;
            return Utility.CreateNewDiscussionReply(mContext, item);
        }

        protected override ListItem AddMettingUser(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderRelativeUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = AddItemByAPI(mParentList, creationInformation);
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

        protected override ListItem AddMettings(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderRelativeUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = AddItemByAPI(mParentList, creationInformation);

            if (data.ContainsKey("Title"))
            {
                listItem["Title"] = data["Title"];
            }
            int eventType = 0;
            if (data.ContainsKey("EventType"))
            {
                eventType = (int)data["EventType"];
                int IsDetached = data.ContainsKey("IsDetached") ? (int)data["IsDetached"] : -1;
                string mEventUID = data.ContainsKey("EventUID") ? data["EventUID"].ToString() : null;
                //update will fail if we don't assign recurrenceID field when eventtype is 0,please see ADO-5026 for more detail
                if (eventType == 0)
                {
                    //之前针对ADO-5026的修改没有考虑在Calendar创建的Item不勾选Recurrence的情况，导致不勾选Recurrence的item关联的workspcae下的Item的EventType还原不正确，
                    //根据不勾选Recurrence的Item的特性添加后半部分的判断。
                    //if (IsDetached == 0 && mEventUID != null)
                    //{
                    //    listItem["EventType"] = eventType;
                    //}
                    //else
                    //{
                    //    listItem["EventType"] = 2;
                    //    listItem["RecurrenceID"] = data["EventDate"];
                    //}
                    listItem["EventType"] = 2;
                    listItem["RecurrenceID"] = data["EventDate"];
                }
                else
                {
                    listItem["EventType"] = eventType;
                }
            }
            DateTime eventDate = DateTime.MinValue;
            int duration = -1;
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
                eventDate = new DateTime(((DateTime)data["EventDate"]).Ticks, DateTimeKind.Utc);
                listItem["EventDate"] = eventDate;
            }
            if (data.ContainsKey("Duration"))
            {
                duration = (int)data["Duration"];
                listItem["Duration"] = duration;
            }
            if (data.ContainsKey("EndDate"))
            {
                listItem["EndDate"] = new DateTime(((DateTime)data["EndDate"]).Ticks, DateTimeKind.Utc);
            }
            else if (eventType == 3 && eventDate != DateTime.MinValue && duration >= 0)
            {
                TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                var date = eventDate.Add(tsDuration);
                listItem["EndDate"] = date;
                userData["EndDate"] = date;
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
                //DOC-67486，在此处设置listItem["IsOrphaned"]=true或者不设置该值，都会导致listItem.Update抛出异常
                //所以在此处设置listItem["IsOrphaned"] = false，如果是true在之后更新field的时候会更新正确。
                listItem["IsOrphaned"] = false;
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
                userData.Remove("EventUrl");
                userData.Remove("EventUrl#2");
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


        internal override void UpdateListItem(ref ListItem listItem, Dictionary<string, object> userData, ListItemUpdateMethodKind updateMethodKind, bool change)
        {
            RestoreItemFields(ref listItem, userData, updateMethodKind);

            if (change)
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mParentList.Id.ToString();
                string fileName = listItem["FileRef"].ToString();
                string op = "TakeOffline";
                mRequest.OperateOnVersion(mAveWebCache.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, listItem.Id, (int)listItem["_UIVersion"], listId, fileName, op);
            }
            try
            {

                if (WrapperConfiguration.BPOS_S.KeepModeration)
                {
                    if (mParentList.EnableModeration && listItem["_ModerationStatus"] != null && (int)listItem["_ModerationStatus"] != mModerationStatus)
                    {
                        if (mParentList.BaseType == BaseType.DiscussionBoard)
                        {
                            listItem["Editor"] = userData.ContainsKey("Editor") ? userData["Editor"] : "1073741823";
                            listItem["Modified"] = userData.ContainsKey("Modified") ? userData["Modified"] : DateTime.Now;
                            listItem["_ModerationStatus"] = mModerationStatus;
                            listItem.Update();
                            mContext.ExecuteQuery();
                        }
                        else
                        {
                            mLogger.Info("XLUOTEST:KeepModeration and updated ");
                            string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                            Dictionary<string, object> modifiedData = new Dictionary<string, object>();
                            modifiedData["ModerationStatus"] = mModerationStatus;
                            modifiedData["Modified"] = userData.ContainsKey("Modified") ? userData["Modified"] : null;
                            UpdateListItemByWebService(listItem, webAppName, modifiedData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Update list item failed.Error Message:{0}", ex.ToString());
                listItem = mParentList.GetItemById(listItem.Id);
            }

            //if (mListTemplate == (int)ListTemplateType.Tasks)
            //{
            //    if (!userData.ContainsKey("StartDate") && listItem.ParentList.Fields.GetByInternalNameOrTitle("StartDate") != null)
            //    {
            //        listItem["StartDate"] = null;
            //        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(mContext);
            //        lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            //        {
            //            mRequest.InternUpdate(mParentList, listItem.Id, userData, excepScope);
            //        }
            //    }
            //}
            //because of ADO-72716, comment out the code.

        }

        protected virtual void UpdateListItemByWebService(ListItem listItem, string webAppName, Dictionary<string, object> modifiedData)
        {
            AveWebServiceRequest.UpdateListItems(webAppName, mAveWebCache.ServerRelativeUrl, mParentList.Title, listItem.Id, listItem["FileRef"].ToString(), mObj, modifiedData);
        }

        private static void RestoreTaxonomyFields(ListItem listItem, List<Dictionary<string, object>> taxonomyFields)
        {
            foreach (Dictionary<string, object> taxonomyField in taxonomyFields)
            {
                string internalName = taxonomyField["FieldName"].ToString();
                bool allowMultipleValues = Convert.ToBoolean(taxonomyField["AllowMultipleValues"].ToString());
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
                    listItem[internalName] = values.ToString();
                }
                else
                {
                    string text = taxonomyField["Text"].ToString();
                    listItem[internalName] = text;
                }
            }
        }

        internal override ListItem UpdateToSpecificVersion(ListItem listItem, int originalVersion, bool deleteCurrentVersion, Dictionary<string, object> userData)
        {
            List<int> skipVersionLabels = new List<int>();

            int destVersion = (int)listItem["_UIVersion"];

            if (deleteCurrentVersion)
            {
                skipVersionLabels.Add(destVersion);
            }

            bool change = false;
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                if (!mEnableVersioning)
                {
                    mParentList.EnableVersioning = true;
                    mParentList.Update();
                }

                if ((int)listItem["_ModerationStatus"] == 1)
                {
                    listItem["_ModerationStatus"] = 0;
                    listItem.Update();
                    change = true;
                }

                while (originalVersion - destVersion > 512)
                {
                    listItem.Update();
                    destVersion += 512;
                    skipVersionLabels.Add(destVersion);
                }
            }

            UpdateListItem(ref listItem, userData, ListItemUpdateMethodKind.Update, change);

            if (skipVersionLabels.Count > 0)
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mParentList.Id.ToString();
                string fileName = listItem.FieldValues["FileRef"].ToString();
                string op = "Delete";
                for (int i = 0; i < skipVersionLabels.Count; i++)
                {
                    mRequest.OperateOnVersion(mAveWebCache.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, listItem.Id, skipVersionLabels[i], listId, fileName, op);
                }
            }

            return listItem;
        }

        internal static bool SetFieldValues(ListItem item, Dictionary<string, object> fields)
        {
            bool changed = false;
            if (fields.ContainsKey("NeedSetNullFields"))
            {
                List<string> setToNullFields = fields["NeedSetNullFields"] as List<string>;
                SetFieldValueToNull(item, setToNullFields);
                fields.Remove("NeedSetNullFields");
            }

            if (fields.ContainsKey("TaxonomyFields"))
            {
                RestoreTaxonomyFields(item, fields["TaxonomyFields"] as List<Dictionary<string, object>>);
                fields.Remove("TaxonomyFields");
            }

            foreach (KeyValuePair<string, object> field in fields)
            {
                try
                {
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
                    //else if (field.Key.Equals("WikiField"))
                    //{
                    //    continue;
                    //}
                    else if (field.Value is DateTime)
                    {
                        item[field.Key] = new DateTime(((DateTime)field.Value).Ticks, DateTimeKind.Utc);
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
                    mLogger.Debug(AveClientOMRequestResource.SetFieldValue, field.Key, ex);
                }
            }
            return changed;
        }

        private static void SetFieldValueToNull(ListItem item, List<string> fields)
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
                    mLogger.Debug(AveClientOMRequestResource.SetFieldValueToNullError, e);
                }
            }
        }

        // SAAS-1478
        private bool CheckItemExistenceForSpecialListTemplateType(List list, int template, ref ListItem listItem)
        {
            ListItemCollection listItems = null;
            CamlQuery camlQuery = new CamlQuery();
            if (template == 880 && mMemberID != -1)
            {
                camlQuery.ViewXml =
                       string.Format("<View><Query><Where><Eq><FieldRef Name={0} LookupId=\"True\"/><Value Type=\"Lookup\">{1}</Value></Eq></Where></Query></View>",
                       AveCommunitiesConstants.MemberFieldName, mMemberID);
            }
            else if (template == (int)ListTemplateType.DesignCatalog)
            {
                camlQuery.ViewXml =
                           string.Format("<View Scope=\"RecursiveAll\"><Query><Where><And><Eq><FieldRef Name=\"Name\"/><Value Type=\"Text\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                           mName, mParentFolderRelativeUrl);//find item by name in composed look list 
            }
            listItems = list.GetItems(camlQuery);
            mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
            mContext.ExecuteQuery();
            bool exist = listItems != null && listItems.Count > 0;
            listItem = exist ? listItems[0] : null;
            return listItem != null;
        }

        internal override bool IsItemExist(List list, int listTemplate, Guid tpGuid, int rowId, int destRowId, ref ListItem listItem)
        {
            try
            {
                if (listTemplate == 880 || listTemplate == (int)ListTemplateType.DesignCatalog)
                {
                    return CheckItemExistenceForSpecialListTemplateType(list, listTemplate, ref listItem);
                }
                if (destRowId <= 0)
                {
                    if (mContext.HasPendingRequest)
                    {
                        mContext.ExecuteQuery();
                    }
                    listItem = null;
                    return false;
                }
                else
                {
                    listItem = list.GetItemById(destRowId);
                    mContext.Load(listItem);
                    mContext.Load(listItem, item => item.HasUniqueRoleAssignments);
                    mContext.ExecuteQuery();
                    if (listItem != null && listItem.FileSystemObjectType == FileSystemObjectType.Folder)
                    {
                        return base.IsItemExist(list, listTemplate, tpGuid, rowId, destRowId, ref listItem);
                    }
                    return listItem != null;
                }
            }
            catch (WebException e)
            {
                mLogger.Info("Can not find item[{0}].Error Message:{1}", tpGuid, e.ToString());
                int interval = WrapperConfiguration.BPOS_S.ClientRequestRetryInterval;
                if (!AveExceptionHelper.IsConnectionException(e) && !AveExceptionHelper.IsHTTP429Error(e, ref interval) || mIsCurrentMethodRetried)
                {
                    return false;
                }
                Thread.Sleep(interval);
                mIsCurrentMethodRetried = true;
                mLogger.Debug("Retry Method:{0}", "IsItemExist");
                return AveEventHelper.Retry<List, int, Guid, int, int, ListItem, bool>(delegate (List tempList, int tempListTemplate, Guid temptpGuid, int tempRowId, int tempDestRowId, ref ListItem tempListItem)
                {
                    LoadContextInfo();
                    return IsItemExist(tempList, tempListTemplate, temptpGuid, tempRowId, tempDestRowId, ref tempListItem);
                }, list, listTemplate, tpGuid, rowId, destRowId, ref listItem);
            }
            catch (Exception ex)
            {
                mLogger.Info("Item:{0} is not exist.Error Message:{1}", tpGuid, ex.ToString());
                listItem = null;
                return false;
            }
            finally
            {
                mIsCurrentMethodRetried = false;
            }
        }

        public override void Dispose()
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
        protected override void MoveToConflictFolder()
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
                bool itemExist = IsItemExist(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref item);
                if (!itemExist)
                {
                    return;
                }
                Microsoft.SharePoint.Client.File file = null;
                if (item.FieldValues.ContainsKey("FileRef"))
                {
                    try
                    {
                        file = GetFileByAPI(item["FileRef"].ToString());
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
                Folder mParentFolder = GetFolderByAPI(mParentFolderRelativeUrl);
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
                Microsoft.SharePoint.Client.Folder folder = GetFolderByAPI(conflictFolderUrl);
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
                    if (mParentList != null && mParentList.BaseType != BaseType.DocumentLibrary)
                    {
                        ListItemCreationInformation creationInformation = new ListItemCreationInformation();
                        creationInformation.FolderUrl = mParentFolder.ServerRelativeUrl;
                        creationInformation.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInformation.LeafName = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        ListItem listItem = AddItemByAPI(mParentList, creationInformation);
                        listItem["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        listItem.Update();
                        folder = GetFolderByAPI(conflictFolderUrl);
                        mContext.Load(listItem);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                    else
                    {
                        if (mParentList != null && mParentList.BaseTemplate == 2100)
                        {
                            string webApp = AveUrlUtility.GetServerUrl(mContext.Url);
                            AddSlideFolderByWebService(mParentFolder, webApp);
                            folder = GetFolderByAPI(conflictFolderUrl);
                        }
                        else
                        {
                            folder = AddFolderByAPI(mParentFolder.Folders, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        }
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                }
                #endregion
                string moveFileName = AveSPUtility.GetConflictNewName(item["FileLeafRef"].ToString(), file.TimeLastModified);
                string moveFileTitle = AveSPUtility.GetConflictNewName(item["Title"].ToString(), file.TimeLastModified);
                string moveFileUrl = conflictFolderUrl + "/" + moveFileName;
                MoveToByAPI(file, moveFileUrl, MoveOperations.None);
                mContext.Load(file);
                mContext.ExecuteQuery();
                needKeepFields.Add("Title", moveFileTitle);
                //file为该方法内部变量，此处这么写没有问题，如果以后在UpdateListItem之后再次调用listItem对象，请调用fileListItem而不是file.ListItemAllFields
                var fileListItem = file.ListItemAllFields;
                UpdateListItem(ref fileListItem, needKeepFields, ListItemUpdateMethodKind.SystemUpdate, false);
                mDestRowId = -1;
            }
            catch (Exception ex)
            {
                mLogger.Error("Move item:{0} to Conflict folder failed,error:{1}", (mParentFolderRelativeUrl + "/" + mTitle), ex.ToString());
            }
        }

        protected virtual void AddSlideFolderByWebService(Folder mParentFolder, string webApp)
        {
            AveWebServiceRequest.AddSlideFolder(webApp, mAveWebCache.ServerRelativeUrl, mParentList.Title, mParentFolder.ServerRelativeUrl, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME, mObj);
        }

        protected virtual Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            return folders.Add(url);
        }

        protected virtual Folder GetFolderByAPI(string url)
        {
            return mParentWeb.GetFolderByServerRelativeUrl(url);
        }
        protected virtual File GetFileByAPI(string url)
        {
            return mParentWeb.GetFileByServerRelativeUrl(url);
        }

        protected virtual void MoveToByAPI(File file, string url, MoveOperations option)
        {
            file.MoveTo(url, option);
        }

        // version14的client API中，Web没有Url属性，从目前构造函数的使用来看，还原document的时候mAveWebCache是空，需要使用Web的Url，14中没有该属性，所以在这里重载。
        protected override void UnlockFile(ListItem item)
        {
            try
            {
                if (CheckFileLockStatus(item))
                {
                    if (mAveWebCache != null)
                    {
                        this.mRequest.DeclareOrUndeclareItem(item.Id, this.mParentList.Id, mAveWebCache.Url);
                    }
                    else
                    {
                        mContext.Load(mParentWeb, web => web.Url);
                        mContext.ExecuteQuery();
                        this.mRequest.DeclareOrUndeclareItem(item.Id, this.mParentList.Id, mParentWeb.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to unlock file. {0}", ex);
            }
        }
    }
}
