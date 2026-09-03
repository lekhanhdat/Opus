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
using System.Text;
using System.Threading;
using System.Xml;
namespace AvePoint.ObjectModel.ClientOM
{
    public class AveListItemRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveListItemRestore));
        /// <summary>
        /// 多个client list item同时删除，其中随机一个或几个会出报出item不存在的server exception，先将执行这个操作的execute锁住，串行执行
        /// </summary>
        protected static object lockObj = new object();

        protected Site mSite;
        protected Web mParentWeb;
        protected List mParentList;
        protected string mParentFolderRelativeUrl;
        protected int mRowId;
        protected int mDestRowId;
        protected Guid mGuid;
        protected int mModerationStatus;
        protected bool mIsNewCreated;
        protected int mOrginalVersion;
        protected byte mOriginalLevel;
        protected bool mOverWrite;
        protected int mDraftOwnerId;
        protected int mCheckoutUserId;
        protected ClientContext mContext;
        protected object mObj;
        protected string mTitle;
        protected int mListTemplate;
        protected bool mEnableModeration;
        protected bool mEnableVersioning;
        protected bool mMOVE_ITEM_TO_CONFLICT_FOLDER;
        protected bool mMOVE_SOURCE_TO_CONFLICT_FOLDER;
        protected bool mSKIP_IF_SAME_MODIFIEDTIME;
        protected bool mOverwriteByLastModifiedTime;
        protected bool mIsCurrentMethodRetried = false;
        protected AveRestoreOption mRestoreOption;
        protected AveClientOMRequest mRequest;
        protected IAveWeb mAveWebCache;
        /// <summary>
        /// 为unittest提供构造方法
        /// </summary>
        public AveListItemRestore() { }

        public AveListItemRestore(AveClientOMRequest request, Site site, AveClientContext conText, object obj)
        {
            mRequest = request;
            mSite = site;
            mContext = conText;
            mObj = obj;
        }

        public AveListItemRestore(AveClientOMRequest request, Site site, Web web, List list, int rowId, int moderationStatus, ClientContext context, object obj)
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of variable")]
        protected virtual void PrepareRestoreContext(Dictionary<string, object> data, Dictionary<string, object> userData)
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
            mListTemplate = data.ContainsKey("ListTemplate") ? (int)data["ListTemplate"] : -1;
            mEnableModeration = data.ContainsKey("ListEnableModeration") ? Convert.ToBoolean(data["ListEnableModeration"]) : false;
            mEnableVersioning = data.ContainsKey("ListEnableVersioning") ? Convert.ToBoolean(data["ListEnableVersioning"]) : false;
            mMOVE_ITEM_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_ITEM_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_ITEM_TO_CONFLICT_FOLDER"]) : false;
            mMOVE_SOURCE_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_SOURCE_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_SOURCE_TO_CONFLICT_FOLDER"]) : false;
            mOverwriteByLastModifiedTime = data.ContainsKey("OverwriteByLastModifiedTime") ? Convert.ToBoolean(data["OverwriteByLastModifiedTime"]) : false;
            mRestoreOption = data.ContainsKey("RestoreOption") ? (AveRestoreOption)Enum.Parse(typeof(AveRestoreOption), data["RestoreOption"].ToString()) : AveRestoreOption.Default;
            mSKIP_IF_SAME_MODIFIEDTIME = data.ContainsKey("SKIP_IF_SAME_MODIFIEDTIME") ? Convert.ToBoolean(data["SKIP_IF_SAME_MODIFIEDTIME"]) : false;
            //mContext.Load(mParentWeb);
            mContext.Load(mParentList);
        }

        protected bool TryGetListId(Dictionary<string, object> data, out Guid listId)
        {
            listId = Guid.Empty;
            object idObj = null;
            if (data != null && data.TryGetValue("ListId", out idObj) && idObj != null)
            {
                try
                {
                    listId = new Guid(idObj as string);
                    return true;
                }
                catch (Exception e)
                {
                    mLogger.Debug("An error occurred while trying to get list id.Error:{0}", e);
                }
            }
            return false;
        }

        internal virtual Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
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
                        //ADO-177075 10模拟多线程还原systemfile时可能会修改version setting，导致item version涨不上去
                        lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                        {//mEnableVersioning是在还原item之前获取的version状态
                            if (mEnableVersioning == true && mParentList.EnableVersioning == false)
                            {
                                mParentList.EnableVersioning = true;
                                mParentList.Update();
                                mContext.ExecuteQuery();
                            }
                        }
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
                restoreResult["Exception"] = e.Message + "--" + e.StackTrace;
                restoreResult["ExceptionMessage"] = e.Message;
                restoreResult["RestoreStatus"] = false;
            }

            return restoreResult;
        }

        protected virtual void UnlockFile(ListItem item)
        {
            try
            {
                if (CheckFileLockStatus(item))
                {
                    if (mAveWebCache != null)
                    {
                        this.mRequest.DeclareOrUndeclareItem(item.Id, this.mParentList.Id, mAveWebCache.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to unlock file. {0}", ex);
            }
        }

        protected virtual bool CheckFileLockStatus(ListItem listItem)
        {
            if (!listItem.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus"))
            {
                return false;
            }
            bool locked = false;
            try
            {
                object status = listItem["_vti_ItemHoldRecordStatus"];
                int value;
                if (status != null && int.TryParse(status.ToString(), out value))
                {
                    locked = IsLocked(value);
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "Failed to check item lock status. Error:{0}", ex.ToString());
            }

            return locked;
        }

        protected bool IsLocked(int value)
        {
            //bool isOnHold = ((long)value & 4096L) != 0L;
            bool isRecord = ((long)value & 16L) != 0L;
            return isRecord;
            //return isOnHold || isRecord;
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

        internal virtual ListItem AddListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            ListItem listItem = null;
            ListItem parentItem = null;//记录需要keep一写data如modified time的parent item
            Dictionary<string, object> needKeepData = new Dictionary<string, object>();// DiscussionBoard存放parent item需要keep的data, survey存放response需要keep的data
            switch ((ListTemplateType)mListTemplate)
            {
                case ListTemplateType.DiscussionBoard:
                    listItem = AddDiscussionTopic(data, userData, ref parentItem, ref needKeepData);
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
                    listItem = mParentList.AddItem(creationInformation);
                    break;
            }

            if (mGuid != Guid.Empty && mListTemplate != (int)ListTemplateType.Survey)
            {
                listItem["GUID"] = mGuid;
            }

            if (mOrginalVersion == 512)
            {
                RestoreItemFields(ref listItem, userData, ListItemUpdateMethodKind.None);

                if (mEnableModeration)
                {
                    listItem["_ModerationStatus"] = mModerationStatus;
                }
            }

            listItem.Update();
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
            finally
            {
                mIsCurrentMethodRetried = false;
            }
            if (mListTemplate == (int)AveListTemplateType.Posts && userData.ContainsKey("Body") && listItem.Id != mRowId)
            {
                object newBodyValue = ReplaceItemIdInBodyHtml(userData["Body"].ToString(), listItem.Id, mRowId);
                Dictionary<string, object> updateData = new Dictionary<string, object>();
                updateData["Body"] = newBodyValue;
                if (userData.ContainsKey("Modified"))
                {
                    updateData["Modified"] = userData["Modified"];
                }
                UpdateListItem(ref listItem, updateData, ListItemUpdateMethodKind.SystemUpdate, false);
            }

            //keep discussion folder的一些属性如modified time等
            if ((ListTemplateType)mListTemplate == ListTemplateType.DiscussionBoard)
            {
                if (parentItem != null && needKeepData.Count > 0)
                {
                    RestoreItemFields(ref parentItem, needKeepData, ListItemUpdateMethodKind.Update);
                }
            }
            else if ((ListTemplateType)mListTemplate == ListTemplateType.Survey)
            {
                if (listItem != null && needKeepData.Count > 0)
                {
                    UpdateListItem(ref listItem, needKeepData, ListItemUpdateMethodKind.SystemUpdate, false);
                }
            }
            mIsNewCreated = true;

            return listItem;
        }


        private void HandleDeclareItem(ListItem listItem, Dictionary<string, object> userData)
        {
            if (userData.ContainsKey("_vti_ItemDeclaredRecord")) //Item declare records.
            {
                listItem["_vti_ItemDeclaredRecord"] = userData["_vti_ItemDeclaredRecord"];
                if (mEnableModeration)
                {
                    listItem["_ModerationStatus"] = mModerationStatus;
                }
                listItem.Update();
            }
        }

        private object ReplaceItemIdInBodyHtml(String fieldValue, int newId, int originalId)
        {
            XmlDocument fieldDoc = new XmlDocument();
            try
            {
                fieldDoc.InnerXml = "<ReplaceXmlLinks>" + fieldValue + "</ReplaceXmlLinks>";
                foreach (XmlNode node in fieldDoc.GetElementsByTagName("a"))
                {
                    String hrefValue = node.Attributes["href"].Value;
                    node.Attributes["href"].Value = hrefValue.Replace("Attachments/" + originalId, "Attachments/" + newId);
                }
                foreach (XmlNode node in fieldDoc.GetElementsByTagName("img"))
                {
                    String srcValue = node.Attributes["src"].Value;
                    node.Attributes["src"].Value = srcValue.Replace("Attachments/" + originalId, "Attachments/" + newId);
                }
            }
            catch (Exception e)
            {
                mLogger.Log(AveLogLevel.WARN, "Error while replace item id in body html.{0}", e);
            }
            return fieldDoc.FirstChild.InnerXml;
        }


        protected virtual ListItem AddDiscussionTopic(Dictionary<string, object> data, Dictionary<string, object> userData, ref ListItem parentItem, ref Dictionary<string, object> parentNeedKeepData)
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
            mContext.Load(item, it => it.HasUniqueRoleAssignments);
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
                    keepData["_ModerationStatus"] = item["_ModerationStatus"];
                }
            }
            parentNeedKeepData = keepData;
            return Utility.CreateNewDiscussionReply(mContext, item);
        }

        protected virtual ListItem AddMettingUser(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderRelativeUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = mParentList.AddItem(creationInformation);
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

        protected virtual ListItem AddMettings(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderRelativeUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = mParentList.AddItem(creationInformation);

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

        protected virtual ListItem AddSurveyRespond(Dictionary<string, object> data, Dictionary<string, object> userData, Dictionary<string, object> needKeepData)
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderRelativeUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.File;
            ListItem listItem = AddItemByAPI(mParentList, creationInformation);

            if (mGuid != Guid.Empty)
            {
                needKeepData.Add("GUID", mGuid);
                object colValue = null;
                if (userData.TryGetValue("Modified", out colValue))
                {
                    needKeepData.Add("Modified", colValue);
                }
                if (userData.TryGetValue("Created", out colValue))
                {
                    needKeepData.Add("Created", colValue);
                }
                if (userData.TryGetValue("Editor", out colValue))
                {
                    needKeepData.Add("Editor", colValue);
                }
                if (userData.TryGetValue("Author", out colValue))
                {
                    needKeepData.Add("Author", colValue);
                }
            }
            return listItem;
        }
        internal virtual void UpdateListItemForFolder(ref ListItem listItem, Dictionary<string, object> userData, ListItemUpdateMethodKind updateMethodKind, bool change)
        {
            UpdateListItem(ref listItem, userData, updateMethodKind, change);
        }

        internal virtual void UpdateListItem(ref ListItem listItem, Dictionary<string, object> userData, ListItemUpdateMethodKind updateMethodKind, bool change)
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

                if (mParentList.EnableModeration && listItem["_ModerationStatus"] != null
                    && (mParentList.EnableMinorVersions || (int)listItem["_ModerationStatus"] != mModerationStatus))
                {
                    if (mParentList.BaseType != BaseType.DocumentLibrary)
                    {
                        listItem["Editor"] = userData.ContainsKey("Editor") ? userData["Editor"] : "1073741823";
                        //listItem["Modified"] = userData.ContainsKey("Modified") ? userData["Modified"] : DateTime.Now;
                        if (userData.ContainsKey("Modified") && userData["Modified"] != null)
                        {
                            DateTime modified = new DateTime(((DateTime)userData["Modified"]).Ticks, DateTimeKind.Utc);
                            listItem["Modified"] = modified;
                        }
                        else
                        {
                            listItem["Modified"] = DateTime.Now;
                        }
                        listItem["_ModerationStatus"] = mModerationStatus;
                        listItem.Update();
                        mContext.ExecuteQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Update list item failed.Error Message:{0}.", ex.ToString());
                listItem = mParentList.GetItemById(listItem.Id);
            }

        }

        internal virtual ListItem UpdateToSpecificVersion(ListItem listItem, int originalVersion, bool deleteCurrentVersion, Dictionary<string, object> userData)
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
                UpdateListItem(ref listItem, userData, ListItemUpdateMethodKind.Update, change);
            }
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listform is a part of url")]
        protected virtual void RestoreItemFields(ref ListItem item, Dictionary<string, object> fields, ListItemUpdateMethodKind updateMethodKind)
        {
            if (item != null)
            {
                try
                {
                    //if (fields.ContainsKey("Author"))
                    //{
                    //    fields.Remove("Author");
                    //}
                    //if (fields.ContainsKey("Editor"))
                    //{
                    //    fields.Remove("Editor");
                    //}
                    int ratings = -1;
                    if (fields.ContainsKey("CurrentUserRatings"))
                    {
                        ratings = Convert.ToInt32(fields["CurrentUserRatings"]);
                        fields.Remove("CurrentUserRatings");
                    }
                    if (string.Compare(mContext.ServerVersion.ToString(), "15.", StringComparison.OrdinalIgnoreCase) < 0 && ratings != -1)
                    {
                        using (AveWebServiceRequest webServiceRequest = new AveWebServiceRequest(mRequest.Url, mRequest.mUserAccountInfo, mObj, mContext.ServerVersion.ToString()))
                        {
                            string itemUrl = mParentList.ParentWebUrl.TrimEnd('/') + @"/_layouts/listform.aspx?PageType=4&amp;ListId=" + mParentList.Id.ToString("B") + "amp;ID=" + item.Id;
                            mContext.Load(mSite, site => site.Id);
                            mContext.Load(mParentWeb, web => web.Id);
                            mContext.ExecuteQuery();
                            webServiceRequest.SetListItemRatings(itemUrl, mTitle, ratings, mSite.Id, mParentWeb.Id);
                        }
                    }
                    switch (updateMethodKind)
                    {
                        case ListItemUpdateMethodKind.Update:
                            bool changed = SetFieldValues(item, fields);
                            if (changed)
                            {
                                item.Update();
                                mContext.Load(item);
                                mContext.Load(item, it => it.HasUniqueRoleAssignments);
                                mContext.ExecuteQuery();
                            }
                            break;
                        case ListItemUpdateMethodKind.SystemUpdate:
                            CustomSystemUpdate(ref item, fields);
                            break;
                        default:
                            SetFieldValues(item, fields);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error("{0}:Restore Item fields failed.Error Message:{1}.", updateMethodKind, ex);
                    mContext.Load(item);
                    mContext.Load(item, it => it.HasUniqueRoleAssignments);
                    mContext.ExecuteQuery();
                }
            }
        }

        private void CustomSystemUpdate(ref ListItem item, Dictionary<string, object> fields)
        {
            Dictionary<string, object> itemProp = new Dictionary<string, object>();
            itemProp["ChangedFieldValues"] = fields;
            itemProp["EnableVersioning"] = mParentList.EnableVersioning;
            itemProp["EnableMinorVersions"] = mParentList.EnableMinorVersions;
            itemProp["EnableModeration"] = mParentList.EnableModeration;
            itemProp["FileSystemObjectType"] = (int)item.FileSystemObjectType;
            if (mParentList.EnableMinorVersions)
            {
                itemProp["IsCurrentMinorVersion"] = true;
            }
            if (mParentList.EnableModeration && (int)item["_ModerationStatus"] == 0)
            {
                itemProp["IsApproved"] = true;
            }
            if (mParentList.EnableMinorVersions && item.FileSystemObjectType == FileSystemObjectType.Folder
                && ((int)item["_UIVersion"] % 512 != 0 || (int)item["_UIVersion"] % 512 == 0 && !mParentList.EnableModeration))
            {
                itemProp["EnableVersioning"] = false;
            }
            if (fields.ContainsKey("CheckInComment"))
            {
                itemProp["CheckInComment"] = fields["CheckInComment"];
                fields.Remove("CheckInComment");
            }
            if (fields.ContainsKey("IsOriginalCheckOut"))
            {
                itemProp["IsOriginalCheckOut"] = fields["IsOriginalCheckOut"];
                fields.Remove("IsOriginalCheckOut");
            }
            if (!fields.ContainsKey("FileLeafRef"))
            {
                if (item.FieldValues.ContainsKey("FileLeafRef"))
                {
                    fields["FileLeafRef"] = item.FieldValues["FileLeafRef"];
                    //ADO-197498 更新FileLeafRef 时需要更新Title
                    if (!fields.ContainsKey("Title") && item.FieldValues.ContainsKey("Title"))
                    {
                        fields["Title"] = item.FieldValues["Title"];
                    }
                }
                else if (item.FieldValues.ContainsKey("Title"))
                {
                    fields["FileLeafRef"] = item.FieldValues["Title"];
                }
            }
            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(mContext);
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                item = mRequest.InternUpdate(mParentList, item, itemProp, excepScope);
                mContext.ExecuteQuery();
            }
            if (excepScope.HasException)
            {
                mLogger.Warn(string.Format("restore item fields failed. Reason:{0}", excepScope.ErrorMessage));
            }
        }

        /// <summary>
        /// ADO-144796: For One Drive Document Restore to keep ModifiedBy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        internal static bool SetModifiedBy(ListItem item, Dictionary<string, object> fields)
        {
            bool changed = false;
            if (fields.ContainsKey("Modified_x0020_By"))
            {
                // need update these field to keep data.
                var modified = (DateTime)fields["Modified"];
                if (modified.Kind == DateTimeKind.Unspecified)
                {
                    modified = DateTime.SpecifyKind(modified, DateTimeKind.Utc);
                }
                item["Modified"] = modified;
                item["Modified_x0020_By"] = fields["Modified_x0020_By"];
                item["Editor"] = fields["Editor"];
                changed = true;
            }
            return changed;
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
            if (fields.ContainsKey("DescendantLikesCount") && fields.ContainsKey("LikesCount"))
            {
                //添加一个Likes的reply的时候，会根据相应的数量自己添加。
                fields["DescendantLikesCount"] = double.Parse(fields["DescendantLikesCount"].ToString()) - double.Parse(fields["LikesCount"].ToString());
            }

            foreach (KeyValuePair<string, object> field in fields)
            {
                try
                {
                    #region ADO-151413. 365更新此属性，Update出错。过滤掉。
                    //ADO-167882 ShortestThreadIndexIdLookup,DiscussionTitleLookup这两个field只有07有，365更新此属性会抛错。
                    if (field.Key.Equals("_HasCopyDestinations", StringComparison.OrdinalIgnoreCase)
                        || field.Key.Equals("ShortestThreadIndexIdLookup", StringComparison.OrdinalIgnoreCase)
                        || field.Key.Equals("DiscussionTitleLookup", StringComparison.OrdinalIgnoreCase)
                        || field.Key.Equals("CheckoutUser", StringComparison.OrdinalIgnoreCase))//这个属性会导致更新失败，并且会涨version，使后面version 不能还原，暂时先不还原此field，并且目前365 对check out的属性基本都不支持
                    {
                        continue;
                    }
                    #endregion
                    if (field.Key.Equals("Properties"))   // The field has been change before in a special method. HandleMetaInfoField
                    {
                        changed = true;
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
                    //else if (field.Key.Equals("WikiField"))
                    //{
                    //    continue;
                    //}
                    else if (field.Value is DateTime)
                    {
                        var dateTime = (DateTime)field.Value;
                        if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                        item[field.Key] = dateTime;
                    }
                    else if (field.Key.Equals("ContentType"))
                    {
                        item["ContentTypeId"] = field.Value;
                    }
                    else if (field.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                    {
                        FieldUrlValue urlValue = new FieldUrlValue();
                        urlValue.Description = field.Value.ToString();
                        string realFieldName = field.Key.Replace("#2", "");
                        urlValue.Url = fields.ContainsKey(realFieldName) ? fields[realFieldName].ToString() : "";
                        item[realFieldName] = urlValue;
                    }
                    else if (field.Key.Equals("Attachments"))
                    {
                        continue;
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

        private static void RestoreTaxonomyFields(ListItem listItem, List<Dictionary<string, object>> taxonomyFields)
        {
            foreach (Dictionary<string, object> taxonomyField in taxonomyFields)
            {
                string internalName = taxonomyField["FieldName"].ToString();
                string taxField = taxonomyField["TextField"].ToString();
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
                    listItem[internalName] = internalName.Equals("TaxKeyword") ? ";#" : values.ToString();
                    listItem[taxField] = values.ToString();
                }
                else
                {
                    string text = taxonomyField["Text"].ToString();
                    listItem[internalName] = text;
                    listItem[taxField] = text;
                }
            }
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

        internal virtual bool IsItemExist(List list, int listTemplate, Guid tpGuid, int rowId, int destRowId, ref ListItem listItem)
        {
            try
            {
                //if (listTemplate != (int)ListTemplateType.Survey)
                //{
                CamlQuery camelQueyr = new CamlQuery();
                camelQueyr.ViewXml =
                       string.Format("<View Scope=\"RecursiveAll\"><Query><Where><And><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                       mGuid, mParentFolderRelativeUrl);//通过guid找item必须有其parent信息，因为同一个list不同folder下可以有相同Guid的item
                ListItemCollection listItems = list.GetItems(camelQueyr);
                mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                mContext.ExecuteQuery();
                if (listItems == null || listItems.Count <= 0)
                {
                    listItem = null;
                    return false;
                }
                if (destRowId != -1)
                {
                    for (int i = 0; i < listItems.Count; i++)
                    {
                        if (listItems[i].Id == destRowId)
                        {
                            listItem = listItems[i];
                            return true;
                        }
                    }
                    listItem = listItems[0];
                    return true;
                }
                else
                {
                    listItem = listItems[0];
                    return true;
                }
                //}
                //else
                //{
                //    CamlQuery camelQuery = new CamlQuery();
                //    camelQuery.ViewXml =
                //           string.Format("<View Scope=\"RecursiveAll\"><Query><Where><And><Eq><FieldRef Name=\"AveServeyID\"/><Value Type=\"Integer\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                //           rowId, mParentFolderRelativeUrl);//通过guid找item必须有其parent信息，因为同一个list不同folder下可以有相同Guid的item
                //    ListItemCollection listItems = list.GetItems(camelQuery);
                //    mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                //    mContext.ExecuteQuery();
                //    bool exist = listItems != null && listItems.Count > 0;
                //    listItem = exist ? listItems[0] : null;
                //    return exist;
                //}
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
                    //mContext.Load(mParentWeb);
                    mContext.Load(mParentList);
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

        public virtual void Dispose()
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
        protected virtual void MoveToConflictFolder()
        {
            try
            {
                ListItem item = null;//获取item，并获取list setting属性；
                bool itemExist = IsItemExist(mParentList, mListTemplate, mGuid, mRowId, mDestRowId, ref item);
                if (!mParentList.ServerTemplateCanCreateFolders)
                {
                    return;
                }
                if (!mParentList.EnableFolderCreation)
                {
                    mParentList.EnableFolderCreation = true;
                    mParentList.Update();
                }
                if (!itemExist)
                {
                    return;
                }
                Microsoft.SharePoint.Client.File file = null;
                if (item.FieldValues.ContainsKey("FileRef"))
                {
                    try
                    {
                        file = mParentWeb.GetFileByServerRelativeUrl(item["FileRef"].ToString());
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
                Folder mParentFolder = mParentWeb.GetFolderByServerRelativeUrl(mParentFolderRelativeUrl);
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
                Microsoft.SharePoint.Client.Folder folder = mParentWeb.GetFolderByServerRelativeUrl(conflictFolderUrl);
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
                        ListItem listItem = mParentList.AddItem(creationInformation);
                        listItem["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        listItem.Update();
                        folder = mParentWeb.GetFolderByServerRelativeUrl(conflictFolderUrl);
                        mContext.Load(listItem);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                    else
                    {
                        if (mParentList != null && mParentList.BaseTemplate == 2100)
                        {
                            string webApp = AveUrlUtility.GetServerUrl(mContext.Url);
                            AveWebServiceRequest.AddSlideFolder(webApp, mAveWebCache.ServerRelativeUrl, mParentList.Title, mParentFolder.ServerRelativeUrl, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME, mObj);
                            folder = mParentWeb.GetFolderByServerRelativeUrl(conflictFolderUrl);
                        }
                        else
                        {
                            folder = mParentFolder.Folders.Add(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        }
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                }
                #endregion
                string moveFileName = AveSPUtility.GetConflictNewName(item["FileLeafRef"].ToString(), file.TimeLastModified);
                string moveFileTitle = AveSPUtility.GetConflictNewName(item["Title"].ToString(), file.TimeLastModified);
                string moveFileUrl = conflictFolderUrl + "/" + moveFileName;
                file.MoveTo(moveFileUrl, MoveOperations.None);
                mContext.Load(file);
                mContext.ExecuteQuery();
                needKeepFields.Add("Title", moveFileTitle);
                //file为该方法内部变量，此处这么写没有问题，如果以后在UpdateListItem之后再次调用listItem对象，请调用fileListItem而不是file.ListItemAllFields
                var fileListItem = file.ListItemAllFields;
                UpdateListItem(ref fileListItem, needKeepFields, ListItemUpdateMethodKind.SystemUpdate, false);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move item:{0} to conflict folder failed,error:{1}.", (mParentFolderRelativeUrl + "/" + mTitle), ex.ToString());
            }
        }

        protected virtual ListItem AddItemByAPI(List list, ListItemCreationInformation creationInformation)
        {
            return mParentList.AddItem(creationInformation);
        }
    }
}
