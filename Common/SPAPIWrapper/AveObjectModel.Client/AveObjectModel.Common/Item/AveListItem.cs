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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Net;
using System.Xml;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.RA.Common.Global;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.News.DataModel;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/02/29", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    class AveListItem : AveSecurableObject, IAveListItem
    {
        private IAveWeb mParentWeb;
        private bool mIsNewCreated;
        private bool mIsUsingPath;
        public static Dictionary<string, string> ChangeMapping = new Dictionary<string, string>();
        public static List<string> ModifyMapping = new List<string>();
        private readonly object mlockObj = new object();
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveListItem));

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special URL")]
        static AveListItem()
        {
            ChangeMapping.Add("Order", "#tp_ItemOrder");
            ChangeMapping.Add("Attachments", "#tp_HasAttachment");

            ModifyMapping.Add("ID");
            ModifyMapping.Add("RowOrdinal");
            ModifyMapping.Add("Version");
            ModifyMapping.Add("_ModerationStatus");
            ModifyMapping.Add("IsCurrent");
            ModifyMapping.Add("GUID");
            ModifyMapping.Add("File_x0020_Size");
            ModifyMapping.Add("WorkflowVersion");
            ModifyMapping.Add("ContentTypeId");
            ModifyMapping.Add("_Level");
            ModifyMapping.Add("_IsCurrentVersion");
            ModifyMapping.Add("_UIVersion");
            ModifyMapping.Add("CalculatedVersion");
            ModifyMapping.Add("DraftOwnerId");
            ModifyMapping.Add("FileLeafRef");
            ModifyMapping.Add("_CheckinComment");
            ModifyMapping.Add("_UIVersionString");
            ModifyMapping.Add("CheckedOutUserId");
            ModifyMapping.Add("VirusStatus");
            ModifyMapping.Add("CheckedOutTitle");
            ModifyMapping.Add("owshiddenversion");
            ModifyMapping.Add("ParentVersionString");
            ModifyMapping.Add("ParentLeafName");
            ModifyMapping.Add("SyncClientId");
            ModifyMapping.Add("SortBehavior");
            ModifyMapping.Add("PermMask");
            ModifyMapping.Add("DocIcon");
        }

        public AveListItem(IAveRequest request, IAveWeb parentWeb, IAveList parentList, IDictionary<string, object> itemProperties, bool newCreated)
            : base(request)
        {
            base.DataCache = new AveClientThreadSafeObjectData();
            mIsNewCreated = newCreated;
            mParentWeb = parentWeb;
            mRequest = request;
            if (itemProperties != null)
            {
                lock (itemProperties)
                {
                    itemProperties["Web"] = parentWeb;
                    if (parentList != null)
                    {
                        itemProperties["ParentList"] = parentList;
                    }
                    base.DataCache.AddPropertyies(itemProperties);
                    object versionsObject;
                    if (itemProperties.TryGetValue("Versions" + AveObjectModelConstant.ObjectPropertySuffix, out versionsObject))
                    {
                        var versionsProperties = versionsObject as Dictionary<string, object>;
                        if (versionsProperties != null)
                        {
                            object versionData;
                            if (versionsProperties.TryGetValue(AveObjectModelConstant.ChildrenProperties, out versionData))
                            {
                                AveListItemVersionCollection listItemVerCol = new AveListItemVersionCollection(this, mRequest, versionsProperties);
                                DataCache.AddProperty("Versions" + AveObjectModelConstant.ObjectPropertySuffix, listItemVerCol);
                                //base.DataCache.PropertiesCache["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = listItemVerCol;
                            }
                            else if (versionsProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(versionsProperties["HasVersion"])
                                && itemProperties.ContainsKey("FieldValues"))//Some system file have no versions, use have no "FieldValues" to check them temporary to avoid backup failed
                            {
                                versionsProperties = this.ConvertCurrentVersionProperties(itemProperties["FieldValues"] as Dictionary<string, object>, (this.ParentList as AveList).NeedLoadFields);
                                DataCache.AddProperty("Versions" + AveObjectModelConstant.ObjectPropertySuffix, new AveListItemVersionCollection(this, mRequest, versionsProperties));
                                //base.DataCache.PropertiesCache["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = new AveListItemVersionCollection(this, mRequest, versionsProperties);
                            }
                            if (this.ParentList.BaseTemplate == AveListTemplateType.DiscussionBoard)
                            {
                                EnsureParentThreadIndex(versionsProperties);
                            }
                        }
                    }
                }
            }
        }

        public AveListItem(IAveRequest request, IAveWeb parentWeb, IAveList parentList, Dictionary<string, object> itemProperties, bool newCreated, bool usingPath)
            : this(request, parentWeb, parentList, itemProperties, newCreated)
        {
            mIsUsingPath = usingPath;
        }

        internal override void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties)
        {
            roleAssignmentProperties[AveObjectModelConstant.WebServerRelativeUrl] = mParentWeb.ServerRelativeUrl;
            roleAssignmentProperties[AveObjectModelConstant.ListTitle] = this.ParentList.Title;
            roleAssignmentProperties[AveObjectModelConstant.ListId] = this.ParentList.ID;
            roleAssignmentProperties[AveObjectModelConstant.ItemId] = this.ID;
        }

        internal override Dictionary<string, object> AddRoleAssignment(Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.AddRoleAssignment(mParentWeb.ServerRelativeUrl, this.ParentList.DefaultViewUrl, this.ParentList.Title, this.ParentList.ID, this.ID, roleAssignmentProperties, "item.roleAssignments");
        }

        internal override Dictionary<string, object> UpdateRoleAssignment(int principalId, Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.UpdateRoleAssignment(mParentWeb.ServerRelativeUrl, this.ParentList.DefaultViewUrl, this.ParentList.Title, this.ParentList.ID, this.ID, principalId, roleAssignmentProperties, "item.roleAssignments");
        }

        #region IAveListItem Members

        public IAveContentType ContentType
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ContentType"))
                {
                    string contentTypeId = base.DataCache.GetProperty<string>("ContentTypeId");
                    AveContentType contentType = ParentList.ContentTypes[new AveContentTypeId(contentTypeId)] as AveContentType;
                    base.DataCache.AddProperty("ContentType", contentType);
                }
                return base.DataCache.GetProperty<IAveContentType>("ContentType");
            }
        }

        public string DisplayName
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayName");
            }
        }

        public bool CommentsDisabled
        {
            get
            {
                var value = base.DataCache.GetProperty<bool>("CommentsDisabled");
                mLogger.Info($"SAAS-38248, Get CommentsDisabled:{value}");
                return value;
            }
        }

        public AveCommentsDisabledScope CommentsDisabledScope
        {
            get
            {

                var value = base.DataCache.GetProperty<AveCommentsDisabledScope>("CommentsDisabledScope");
                mLogger.Info($"SAAS-38248, Get CommentsDisabledScope:{value}");
                return value;
            }
        }

        public AveBasePermissions EffectiveBasePermissions
        {
            get
            {
                return base.DataCache.GetProperty<AveBasePermissions>("EffectiveBasePermissions");
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                return ParentList.Fields;
            }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
            }
        }

        public IAveFieldStringValues FieldValuesAsHtml
        {
            get
            {
                return base.DataCache.GetProperty<IAveFieldStringValues>("FieldValuesAsHtml");
            }
        }

        public IAveFieldStringValues FieldValuesAsText
        {
            get
            {
                return base.DataCache.GetProperty<IAveFieldStringValues>("FieldValuesAsText");
            }
        }

        public IAveFieldStringValues FieldValuesForEdit
        {
            get
            {
                return base.DataCache.GetProperty<IAveFieldStringValues>("FieldValuesForEdit");
            }
        }

        public IAveFile File
        {
            get
            {
                if (this.ParentList.BaseType == AveBaseType.DocumentLibrary && this.FileSystemObjectType == AveFileSystemObjectType.File && base.DataCache.IsPropertyNotLoaded("File"))
                {
                    Dictionary<string, object> fileProperties = base.DataCache.GetProperty<Dictionary<string, object>>("File" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveFile file = null;
                    if (fileProperties != null)
                    {
                        lock (fileProperties)
                        {
                            fileProperties["Item"] = this;
                            file = new AveFile(mRequest, mParentWeb as AveWeb, this.ParentList as AveList, null, fileProperties);
                        }
                    }
                    else
                    {
                        file = mParentWeb.GetFile(this.Url);
                    }
                    base.DataCache.AddProperty("File", file);
                    return file;
                }
                return base.DataCache.GetProperty<IAveFile>("File");
            }
        }

        public IAveFile BackupFile
        {
            get
            {
                if (this.ParentList.BaseType == AveBaseType.DocumentLibrary && this.FileSystemObjectType == AveFileSystemObjectType.File && base.DataCache.IsPropertyNotLoaded("BackupFile"))
                {
                    IAveFile file = new AveFile(mRequest, mParentWeb as AveWeb, this.ParentList as AveList, null, base.DataCache.GetPropertyCache());
                    base.DataCache.AddProperty("BackupFile", file);
                    return file;
                }
                return base.DataCache.GetProperty<IAveFile>("BackupFile");
            }
        }

        public AveFileSystemObjectType FileSystemObjectType
        {
            get
            {
                return base.DataCache.GetProperty<AveFileSystemObjectType>("FileSystemObjectType");
            }
        }

        public IAveAttachmentCollection Attachments
        {
            get
            {
                //如果包含attachments，有可能是discover的attachment对象，这个对象需要去掉，重新获取attachment
                if (base.DataCache.IsPropertyNotLoaded("Attachments"))
                {
                    Dictionary<string, object> attachmentColProperties = mRequest.GetAttachments(mParentWeb.ServerRelativeUrl, ParentList.Title, ParentList.ID, this.ID);
                    AveAttachmentCollection attachmentCollection = new AveAttachmentCollection(this, mRequest, attachmentColProperties);
                    base.DataCache.AddProperty("Attachments", attachmentCollection);
                    return attachmentCollection;
                }
                else
                {
                    object attachmentObject = base.DataCache.GetProperty<object>("Attachments");
                    if (attachmentObject is AveAttachmentCollection)
                    {
                        return base.DataCache.GetProperty<IAveAttachmentCollection>("Attachments");
                    }
                    else
                    {
                        base.DataCache.RemoveProperty("Attachments");
                        return this.Attachments;
                    }
                }
            }
        }

        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("Id");
            }
        }

        public AveFileLevel Level
        {
            get
            {
                return base.DataCache.GetProperty<AveFileLevel>("Level");
            }
        }

        public IAveList ParentList
        {
            get
            {
                return base.DataCache.GetProperty<IAveList>("ParentList");
            }
        }

        public Hashtable Properties
        {
            get
            {
                if (this.FieldValues.ContainsKey("MetaInfo") && this.FieldValues["MetaInfo"] != null)
                {
                    if (this.DataCache.IsPropertyNotLoaded("Properties"))
                    {
                        Hashtable MetaInfoTable = new MetaInfoHandler(this.FieldValues["MetaInfo"].ToString()).ToHashtable();
                        base.DataCache.AddProperty("Properties", new AveCustomHashtable(MetaInfoTable, SetChangeProperty));
                    }
                }
                return base.DataCache.GetPropertyWithoutChange<Hashtable>("Properties");
            }
        }

        public object this[Guid fieldId]
        {
            get
            {
                string fieldName = this.Fields[fieldId].InternalName;
                if (base.DataCache.IsPropertyAvailable("ChangedFieldValues"))
                {
                    Dictionary<string, object> fieldValues = base.DataCache.GetProperty<Dictionary<string, object>>("ChangedFieldValues");
                    if (fieldValues.ContainsKey(fieldName))
                    {
                        return fieldValues[fieldName];
                    }
                }
                Dictionary<string, object> oldFieldValues = base.DataCache.GetPropertyWithoutChange<Dictionary<string, object>>("FieldValues");
                if (oldFieldValues.ContainsKey(fieldName))
                {
                    return oldFieldValues[fieldName];
                }
                return null;
            }
            set
            {
                if (!base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
                {
                    base.DataCache.ChangedProperties["ChangedFieldValues"] = new Dictionary<string, object>();
                }
                (base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>)[this.Fields[fieldId].InternalName] = value;
            }
        }

        public object this[string fieldName]
        {
            get  //check changed fieldvalue first
            {
                string fieldInternalName = this.Fields.GetField(fieldName).InternalName;
                if (base.DataCache.IsPropertyAvailable("ChangedFieldValues"))
                {
                    Dictionary<string, object> fieldValues = base.DataCache.GetProperty<Dictionary<string, object>>("ChangedFieldValues");
                    if (fieldValues.ContainsKey(fieldInternalName))
                    {
                        return fieldValues[fieldInternalName];
                    }
                }
                if (base.DataCache.IsPropertyAvailable("FieldValues"))
                {
                    Dictionary<string, object> oldFieldValues = base.DataCache.GetPropertyWithoutChange<Dictionary<string, object>>("FieldValues");
                    if (oldFieldValues.ContainsKey(fieldInternalName))
                    {
                        return oldFieldValues[fieldInternalName];
                    }
                }
                return null;
            }
            set
            {
                string fieldInternalName = this.Fields.GetField(fieldName).InternalName;
                if (!base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
                {
                    base.DataCache.ChangedProperties["ChangedFieldValues"] = new Dictionary<string, object>();
                }
                (base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>)[fieldInternalName] = value;
            }
        }

        public Guid UniqueId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("UniqueId");
            }
        }

        public string Url
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Url") && base.DataCache.IsPropertyAvailable("ServerRelativeUrl"))
                {
                    string str = base.DataCache.GetProperty<string>("ServerRelativeUrl");
                    string webServerRelativeUrl = this.mParentWeb.ServerRelativeUrl.TrimEnd('/');
                    string url = str.Substring(webServerRelativeUrl.Length + 1);
                    base.DataCache.AddProperty("Url", url);
                    return url;
                }
                return base.DataCache.GetProperty<string>("Url");
            }
        }
        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }
        public IAveFolder Folder
        {
            get
            {
                if (this.FileSystemObjectType == AveFileSystemObjectType.Folder && base.DataCache.IsPropertyNotLoaded("Folder"))
                {
                    Dictionary<string, object> folderProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Folder" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveFolder folder = null;
                    if (folderProperties != null)
                    {
                        lock (folderProperties)
                        {
                            folderProperties["Item"] = this;
                            folder = new AveFolder(mRequest, mParentWeb, ParentList, null, folderProperties);
                        }
                    }
                    else
                    {
                        folder = mParentWeb.GetFolder(this.Url);
                    }
                    base.DataCache.AddProperty("Folder", folder);
                    return folder;
                }
                return base.DataCache.GetProperty<IAveFolder>("Folder");
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
        }

        public IAveWeb Web
        {
            get
            {
                return base.DataCache.GetProperty<IAveWeb>("Web");
            }
        }

        public IAveModerationInformation ModerationInformation
        {
            get
            {
                if (!this.ParentList.EnableModeration)
                {
                    return null;
                }
                else
                {
                    if (base.DataCache.IsPropertyNotLoaded("ModerationInformation"))
                    {
                        base.DataCache.AddProperty("ModerationInformation", new AveModerationInformation(mRequest, this));
                    }
                    return base.DataCache.GetProperty<IAveModerationInformation>("ModerationInformation");
                }
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }



        public IAveListItemVersionCollection Versions
        {
            get
            {
                lock (mlockObj)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Versions" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        Dictionary<string, object> listItemVersionColProperties = GetItemVersionProperties();
                        AveListItemVersionCollection listItemVerCol = new AveListItemVersionCollection(this, mRequest, listItemVersionColProperties);
                        base.DataCache.AddProperty("Versions" + AveObjectModelConstant.ObjectPropertySuffix, listItemVerCol);
                        return listItemVerCol;
                    }
                    return base.DataCache.GetProperty<IAveListItemVersionCollection>("Versions" + AveObjectModelConstant.ObjectPropertySuffix);
                }
            }
        }

        /// <summary>
        /// get version properties,folder 的history version为了提高效率放弃获取。
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetItemVersionProperties()
        {
            Dictionary<string, object> listItemVersionColProperties = null;
            AveList list = this.ParentList as AveList;
            bool isFolder = false, needGetVersions = true;
            if (this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues").ContainsKey("FSObjType"))
            {
                isFolder = Convert.ToInt32(this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues")["FSObjType"]) == 1;
            }
            bool hasVersion;
            if ((base.DataCache.TryGetProperty<bool>("HasVersion", out hasVersion) && !hasVersion)
                || !WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance || isFolder //folder online 不keep version 所以放弃获取folder history version信息
                || AveListTemplateType.UserInformation == ParentList.BaseTemplate)  //user information list下的item version不需要备份,且在getversion时容易出现Attempted to perform an unauthorized operation的异常SAAS-26354
            {
                if (list.IsExceedListViewLookupThreshold)
                {
                    if (this.ID > 0 && this.UniqueId != Guid.Empty)
                    {
                        if (AveListTemplateType.UserInformation == ParentList.BaseTemplate)
                        {
                            if (!this.DataCache.IsPropertyAvailable("FieldValues"))
                            {
                                Dictionary<string, object> itemFieldValues = mRequest.GetItemById(mParentWeb.ID, list.ID, this.ID);
                                //Dictionary<string, object> itemFieldValues = mRequest.GetItem(mParentWeb.ServerRelativeUrl, list.Title, list.ID, this.ID, this.UniqueId);
                                this.DataCache.AddPropertyies(itemFieldValues);
                            }
                            needGetVersions = false;
                        }
                        else
                        {
                            needGetVersions = true;
                        }
                    }
                    else
                    {
                        needGetVersions = true;
                    }
                }
                else
                {
                    needGetVersions = false;
                }
            }
            if (!needGetVersions)
            {
                listItemVersionColProperties = this.ConvertCurrentVersionProperties(this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues"), (this.ParentList as AveList).NeedLoadFields);
            }
            else
            {
                CultureInfo cultureInfo = new CultureInfo(Convert.ToInt32(mParentWeb.RegionalSettings.LocaleId));
                listItemVersionColProperties = mRequest.GetItemVersions(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.ID.ToString(), this.ID, this.ServerRelativeUrl, cultureInfo, list.NeedLoadFields, list.IsExceedListViewLookupThreshold);
                if (listItemVersionColProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(listItemVersionColProperties["HasVersion"]))
                {
                    listItemVersionColProperties = this.ConvertCurrentVersionProperties(this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues"), (this.ParentList as AveList).NeedLoadFields);
                }
                else if (this.ParentList.BaseTemplate == AveListTemplateType.DiscussionBoard)
                {
                    EnsureParentThreadIndex(listItemVersionColProperties);
                }
            }
            return listItemVersionColProperties;
        }

        private void EnsureParentThreadIndex(Dictionary<string, object> listItemVersionColProperties)
        {
            try
            {
                object value;
                if (this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues").TryGetValue("#ThreadIndexParentId", out value))
                {
                    var versions = listItemVersionColProperties.GetChildren();
                    foreach (var tempVersion in versions)
                    {
                        var fieldValues = tempVersion["FieldValues"] as IDictionary<string, object>;
                        fieldValues["#ThreadIndexParentId"] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Debug(string.Format("Can not get parent thread index id for discussion board.Message:{0}", ex.ToString()));
            }
        }

        public string Xml
        {
            get
            {
                return base.DataCache.GetProperty<string>("Xml");
            }
        }

        public Guid Recycle()
        {
            return mRequest.RecycleItem(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID);
        }

        public void SystemUpdate(bool incrementListItemVersion)
        {
            this.UpdateOperation("SystemUpdate" + incrementListItemVersion.ToString());
        }
        public void SystemUpdateForRecords()
        {
            mRequest.SystemUpdateItemForRecords(this.mParentWeb.ServerRelativeUrl, this.ParentList.Title, this.ParentList.ID, this.ID,
                base.DataCache.ChangedProperties, this.FileSystemObjectType == AveFileSystemObjectType.Folder);
        }

        public void SystemUpdate()
        {
            this.UpdateOperation("SystemUpdate");
        }

        public void SystemUpdateForProps(Dictionary<string, object> itemProperties)
        {
            mRequest.SystemUpdateForProps(this.mParentWeb.ServerRelativeUrl, this.ParentList.Title, this.ParentList.ID, this.ID, itemProperties);
        }

        public void Delete()
        {
            mRequest.DeleteItem(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID);
        }

        public ListItemComplianceInfo GetComplianceInfo(bool useCache = false)
        {
            lock (mlockObj)
            {
                if (!useCache || base.DataCache.IsPropertyNotLoaded("ComplianceInfo"))
                {
                    ListItemComplianceInfo complianceInfo = mRequest.GetListItemComplianceInfo(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID);
                    base.DataCache.AddProperty("ComplianceInfo", complianceInfo);
                    return complianceInfo;
                }
                return base.DataCache.GetProperty<ListItemComplianceInfo>("ComplianceInfo");
            }
        }

        public void LockRecordItem()
        {
            mRequest.LockRecordItem(Web.ServerRelativeUrl, this.ParentList.RootFolder.ServerRelativeUrl, this.ID.ToString());
        }

        public void UnlockRecordItem()
        {
            mRequest.UnlockRecordItem(Web.ServerRelativeUrl, this.ParentList.RootFolder.ServerRelativeUrl, this.ID.ToString());
        }

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            mRequest.SetComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
            //DataCache.AddPropertyies(newProperties);
        }

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock, bool unlockedAsDefault)
        {
            mRequest.SetComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock, unlockedAsDefault);
            //DataCache.AddPropertyies(newProperties);
        }

        public void SetComplianceTag(string complianceTag, bool blockDel, bool blockEdit, DateTime complianceWrittenTime = default(DateTime), string userEmail = default(string), bool isTagSuperLock = false)
        {
            mRequest.SetComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID, complianceTag, blockDel, blockEdit, complianceWrittenTime, userEmail, isTagSuperLock);
        }

        public void SetComplianceTagOnBulkItems(string complianceTagValue)
        {
            mRequest.SetComplianceTagOnBulkItems(this.ParentList.ParentWeb.Url, this.ParentList.ParentWeb.ID, this.ParentList.ID, new List<int>() { this.ID }, complianceTagValue); 
        }

        public void Update()
        {
            this.UpdateOperation("Update");
        }

        public void UpdateOverwriteVersion()
        {
            this.UpdateOperation("UpdateOverwriteVersion");
        }

        public void InternalUpdate()
        {
            this.UpdateOperation("InternalUpdate");
        }

        public void UpdateInternal(Type[] argsTypes, object[] args)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Type[] argsTypes, object[] args)
        {
            throw new NotImplementedException();
        }

        public int GetTpIdByTpGuid(Guid tp_guid, Guid listId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IAveSecurableObject Members

        protected override IAveRoleAssignmentCollection InternalBreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.BreakRoleInheritance(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID, copyRoleAssignments, clearSubscopes, "item.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, this.mParentWeb.Site as AveSite, this.mParentWeb as AveWeb, this.ParentList as AveList, this.ID, "item.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        protected override IAveRoleAssignmentCollection InternalResetRoleInheritance()
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.ResetRoleInheritance(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID, "item.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, this.mParentWeb.Site as AveSite, this.mParentWeb as AveWeb, this.ParentList as AveList, this.ID, "item.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        public override void RemoveRoleAssignment(int principalId)
        {
            if (this.RoleAssignments.GetByPrincipalId(principalId) != null)
            {
                mRequest.DeleteRoleAssignment(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID, principalId, "item.roleAssignments");
            }
        }

        public override IAveRoleAssignmentCollection RoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleAssignments"))
                {
                    Dictionary<string, object> roleAssignmentsProperties = mRequest.GetRoleAssignments(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID, "item.roleAssignments");
                    AveRoleAssignmentCollection roleAssignments = new AveRoleAssignmentCollection(this, mRequest, this.mParentWeb.Site as AveSite, this.mParentWeb as AveWeb, this.ParentList as AveList, this.ID, "item.roleAssignments", roleAssignmentsProperties);
                    base.DataCache.AddProperty("RoleAssignments", roleAssignments);
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }
        #endregion

        #region private method
        private void UpdateOperation(string updateMethod)
        {
            Dictionary<string, object> newProp = null;
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                base.DataCache.AddChangedProperty(AveObjectModelConstant.UpdateMethodName, updateMethod);
                base.DataCache.AddChangedProperty("EnableVersioning", ParentList.EnableVersioning);
                base.DataCache.AddChangedProperty("EnableMinorVersions", ParentList.EnableMinorVersions);
                base.DataCache.AddChangedProperty("EnableModeration", ParentList.EnableModeration);
                base.DataCache.AddChangedProperty("IsCurrentMinorVersion", this.File == null ? false : this.File.MinorVersion != 0);
                base.DataCache.AddChangedProperty("IsApproved", this.ModerationInformation == null ? false : this.ModerationInformation.Status == AveModerationStatusType.Approved);
                base.DataCache.AddChangedProperty("FileSystemObjectType", (int)this.FileSystemObjectType);
                if (base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
                {
                    //base.DataCache.ChangedProperties["ChangedFieldValues"] = AveList.ConvertFieldValuesToString(base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>);
                    ConvertFieldValuesToString(base.DataCache.ChangedProperties);
                    Dictionary<string, object> fieldValues = base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>;
                    if ("SystemUpdate".Equals(updateMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        fieldValues["FileLeafRef"] = this["FileLeafRef"];
                    }
                    if (!fieldValues.ContainsKey("Modified") && this["Modified"] != null)
                    {
                        fieldValues["Modified"] = this["Modified"];
                    }
                    if (!fieldValues.ContainsKey("Editor") && this["Editor"] != null)
                    {
                        fieldValues["Editor"] = this["Editor"];
                    }
                    if (this.ParentList.BaseType != AveBaseType.DocumentLibrary)
                    {
                        if (this["_ModerationStatus"] != null)
                        {
                            fieldValues["_ModerationStatus"] = this["_ModerationStatus"];
                        }
                    }
                }
                if (mIsNewCreated == true)
                {
                    base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                    if (!mIsUsingPath)
                    {
                        newProp = this.mRequest.AddItem(
                            this.mParentWeb.ServerRelativeUrl,
                            this.ParentList.Title,
                            this.ParentList.ID,
                            base.DataCache.GetProperty<string>("folderUrl"),
                            base.DataCache.GetProperty<int>("FileSystemObjectType"),
                            base.DataCache.GetProperty<string>("leafName"),
                            base.DataCache.ChangedProperties);
                    }
                    else
                    {
                        newProp = this.mRequest.AddItemUsingPath(
                            this.mParentWeb.ServerRelativeUrl,
                            this.ParentList.Title,
                            this.ParentList.ID,
                            base.DataCache.GetProperty<string>("folderUrl"),
                            base.DataCache.GetProperty<int>("FileSystemObjectType"),
                            base.DataCache.GetProperty<string>("leafName"),
                            base.DataCache.ChangedProperties);
                    }
                }
                else
                {
                    newProp = this.mRequest.UpdateItem(this.mParentWeb.ServerRelativeUrl, this.ParentList.Title, this.ParentList.ID, this.ID, base.DataCache.ChangedProperties);
                }
            }
            else if (mIsNewCreated == true)
            {
                base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                if (!mIsUsingPath)
                {
                    newProp = this.mRequest.AddItem(
                        this.mParentWeb.ServerRelativeUrl,
                        this.ParentList.Title,
                        this.ParentList.ID,
                        base.DataCache.GetProperty<string>("folderUrl"),
                        base.DataCache.GetProperty<int>("FileSystemObjectType"),
                        base.DataCache.GetProperty<string>("leafName"),
                        base.DataCache.ChangedProperties);
                }
                else
                {
                    newProp = this.mRequest.AddItemUsingPath(
                        this.mParentWeb.ServerRelativeUrl,
                        this.ParentList.Title,
                        this.ParentList.ID,
                        base.DataCache.GetProperty<string>("folderUrl"),
                        base.DataCache.GetProperty<int>("FileSystemObjectType"),
                        base.DataCache.GetProperty<string>("leafName"),
                        base.DataCache.ChangedProperties);
                }
            }
            base.DataCache.UpdateProperties(newProp);
        }

        internal void ConvertFieldValuesToString(Dictionary<string, object> fieldValues)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> fieldValue in fieldValues["ChangedFieldValues"] as Dictionary<string, object>)
            {
                object value = fieldValue.Value;
                if (fieldValue.Value is AveFieldValueInfo)
                {
                    value = (fieldValue.Value as AveFieldValueInfo).ColValue;
                }
                if (value != null)
                {
                    fields[fieldValue.Key] = value.ToString();
                }
                else
                {
                    fields[fieldValue.Key] = string.Empty;
                }
            }
            fieldValues["ChangedFieldValues"] = fields;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        internal void GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> docInfo)
        {
            IAveListItemVersion itemVersion = this.Versions.GetVersionFromID(baseItemInfo.Version);
            docInfo["Id"] = baseItemInfo.GUID;
            docInfo["UIVersion"] = baseItemInfo.Version;
            docInfo["DoclibRowId"] = baseItemInfo.RowId;
            bool isCheckOut = false;
            if (itemVersion != null)
            {
                docInfo["IsCurrentVersion"] = itemVersion.IsCurrentVersion;
                baseItemInfo.IsCurrentVersion = itemVersion.IsCurrentVersion;
                docInfo["Level"] = (byte)itemVersion.Level;
                //获得version的created和modified time
                if (itemVersion["Created"] != null)
                {
                    docInfo["TimeCreated"] = (DateTime)itemVersion["Created"];
                }
                if (itemVersion["Modified"] != null)
                {
                    docInfo["TimeLastModified"] = (DateTime)itemVersion["Modified"];
                }
                isCheckOut = itemVersion.Level == AveFileLevel.Checkout;
            }
            else
            {
                mLogger.Warn("item version is missing, version count: {0}, expected version: {1}.", this.Versions.Count, baseItemInfo.Version);
            }
            if (this.FieldValues.ContainsKey("MetaInfo"))
            {
                string str = this.FieldValues["MetaInfo"].ToString();
                //docInfo["MetaInfo"] = AveCompressedUtility.GetTCompressedBytes(str);
                Dictionary<string, string> dicMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(str);
                docInfo["MetaInfo"] = AveCompressedUtility.GetTCompressedBytes(str);
                if (dicMetaInfo.ContainsKey("vti_setuppath"))
                {
                    docInfo["SetupPath"] = dicMetaInfo["vti_setuppath"];
                }
                if (dicMetaInfo.ContainsKey("docset_LastRefresh"))
                {
                    docInfo["docset_LastRefresh"] = dicMetaInfo["docset_LastRefresh"];
                }
                if (dicMetaInfo.ContainsKey("snapshots"))
                {
                    docInfo["snapshots"] = dicMetaInfo["snapshots"];
                }
                if (dicMetaInfo.ContainsKey("vti_contenttypeorder"))
                {
                    docInfo["vti_contenttypeorder"] = dicMetaInfo["vti_contenttypeorder"];
                }
                docInfo["HasStream"] = 1;
                baseItemInfo.HasStream = Convert.ToInt32(docInfo["HasStream"]) == 1 ? true : false;
            }
            //AveFile file = this.File as AveFile;
            //if (file != null)
            //{
            //    //Hashtable properties = file.Properties;
            //    //if (properties != null && properties.ContainsKey("vti_setuppath"))
            //    //{
            //    //    docInfo["SetupPath"] = properties["vti_setuppath"].ToString();
            //    //    docInfo["HasStream"] = 0;
            //    //}
            //    //else
            //    //{
            //    //    docInfo["HasStream"] = 1;
            //    //}
            //    //baseItemInfo.HasStream = (int)docInfo["HasStream"] == 1 ? true : false;
            //    if (file.UIVersion == baseItemInfo.Version)
            //    {
            //        isCheckOut = file.Level == AveFileLevel.Checkout;
            //    }
            //    docInfo["LeafName"] = file.Name;
            //}
            //else
            //{
            docInfo["LeafName"] = this.Name;
            //}
            if (this.ParentList.BaseType == AveBaseType.DocumentLibrary && this.FileSystemObjectType == AveFileSystemObjectType.File)
            {
                if ((int)this.FieldValues["_UIVersion"] == baseItemInfo.Version && this.FieldValues.ContainsKey("_Level"))
                {
                    isCheckOut = (AveFileLevel)byte.Parse(this.FieldValues["_Level"].ToString()) == AveFileLevel.Checkout;
                }
                if (this.DataCache.IsPropertyAvailable("CustomizedPageStatus"))
                {
                    docInfo["CustomizedPageStatus"] = this.DataCache.GetPropertyWithoutChange("CustomizedPageStatus");
                }
            }
            //only arcvhie retention label when archive item lastest ver
            if (FieldValues != null && itemVersion != null && FieldValues.ContainsKey(SPColumnConstants.SP_ComplianceTag) && this.Versions.GetLastVersion().VersionId == itemVersion.VersionId)
            {
                docInfo["ComplianceTag"] = FieldValues[SPColumnConstants.SP_ComplianceTag];
            }

            docInfo["IsCheckOut"] = isCheckOut;
            docInfo["HasUniqueRoleAssignments"] = this.HasUniqueRoleAssignments;
            if (baseItemInfo.ItemType == AveItemType.Folder && baseItemInfo.ParentId != null && !Guid.Empty.Equals(baseItemInfo.ParentId))
            {
                docInfo["ParentId"] = baseItemInfo.ParentId;
            }
            //if (this.File != null && this.File.Exists)
            //{
            //    docInfo["CustomizedPageStatus"] = (int)this.File.CustomizedPageStatus;
            //}
            baseItemInfo.ScopeUrl = (base.DataCache.GetProperty<string>("FileDirRef") + "/" + base.DataCache.GetProperty<string>("FileLeafRef")).TrimStart('/');

            if (this.FieldValues.ContainsKey("_IpLabelId"))
            {
                var slId = this.FieldValues["_IpLabelId"]?.ToString();
                if (!string.IsNullOrEmpty(slId))
                {
                    docInfo["_IpLabelId"] = slId;
                }
            }
        }

        public bool HasUniqueRoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("HasUniqueRoleAssignments"))
                {
                    base.DataCache.AddProperty("HasUniqueRoleAssignments", false);
                }
                return base.DataCache.GetProperty<bool>("HasUniqueRoleAssignments");
            }
        }

        public bool IsDocumentIDField(IAveField field)
        {
            if (field.ID == new Guid("{AE3E2A36-125D-45d3-9051-744B513536A6}")
                || field.ID == new Guid("{3B63724F-3418-461f-868B-7706F69B029C}")
                || field.ID == new Guid("{C010D384-479C-494f-968C-C413DBE3DE29}"))
            {
                return true;
            }
            return false;
        }

        private bool IsFieldInvalid(IAveField field)
        {
            IAveTaxonomyField tField = field as IAveTaxonomyField;
            if (tField != null)
            {
                return tField.TermSetId == Guid.Empty && tField.AnchorId == Guid.Empty;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取item的基本column信息
        /// </summary>
        /// <param name="baseItemInfo"></param>
        /// <returns></returns>
        internal Dictionary<string, object> GetUserData(AveBaseItemInfo baseItemInfo, ref List<Dictionary<string, object>> userDataJunction)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            IAveListItemVersion itemVersion = this.Versions.GetVersionFromID(baseItemInfo.Version);
            if (itemVersion != null)
            {
                foreach (IAveField field in itemVersion.Fields)
                {
                    try
                    {
                        var internalName = field.InternalName;
                        object internalNameValue = null;

                        if (IsFieldInvalid(field)) //IsDocumentIDField(field) || 
                        {
                            continue;
                        }
                        if (AveList.ItemBuildInField.Contains(internalName))
                        {
                            this.AddUserData(internalName, userData, this[internalName]);
                            continue;
                        }

                        internalNameValue = itemVersion[internalName];

                        if (internalNameValue == null || string.IsNullOrEmpty(internalNameValue.ToString()))//避免value值为string.Empty的情况；tostring（）之前先判空；
                        {
                            continue;
                        }
                        if (field is IAveFieldLookup)
                        {
                            if ((field as IAveFieldLookup).CountRelated)
                            {
                                continue;
                            }
                            if ((field as IAveFieldLookup).AllowMultipleValues)
                            {
                                this.AddUserDataJunction(baseItemInfo.Version, field.ID, internalNameValue, ref userDataJunction);
                            }
                            else
                            {
                                if (internalName.Equals("_CheckinComment"))
                                {
                                    string tempValue = internalNameValue.ToString();
                                    this.AddUserData(internalName, userData, tempValue);
                                    mLogger.Info("{0} version:{1} checkincomment:{2}", this.Url, itemVersion.VersionId, tempValue);
                                }
                                else if (internalNameValue != null)
                                {
                                    string tempValue = itemVersion[field.InternalName].ToString();
                                    if (tempValue.IndexOf(';') > 0 &&
                                        (string.Equals(field.TypeAsString, "User", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(field.TypeAsString, "UserMulti", StringComparison.OrdinalIgnoreCase)))//对于AvePoint Meeting里面的某些Field，type 时UserMulti但是AllowMultipleValue是false，暂时按照单值来处理
                                    {
                                        tempValue = tempValue.Substring(0, tempValue.IndexOf(';'));
                                        int value = 0;
                                        bool result = int.TryParse(tempValue, out value);
                                        if (result)
                                        {
                                            this.AddUserData(field.InternalName, userData, value);
                                        }
                                    }
                                    else if (field.TypeAsString.Equals("TaxonomyFieldType"))
                                    {
                                        this.AddUserData(field.InternalName, userData, tempValue);
                                    }
                                    else
                                    {//Restore端对于Id;Value格式的value也能处理
                                        int op = tempValue.IndexOf(";#", StringComparison.OrdinalIgnoreCase);
                                        tempValue = tempValue.Remove(op + 1, 1);
                                        this.AddUserData(field.InternalName, userData, tempValue);
                                    }
                                }
                            }
                        }
                        else if (field.TypeAsString == "TaxonomyFieldTypeMulti")
                        {
                            continue;
                        }
                        else if (field is IAveFieldUrl)
                        {
                            if (internalNameValue != null)
                            {
                                string tempValue = internalNameValue.ToString();
                                string url;
                                string description;
                                if (tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase) > 0)
                                {
                                    url = tempValue.Substring(0, tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase));
                                    description = tempValue.Substring(tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase) + 2);
                                }
                                else
                                {
                                    url = tempValue;
                                    description = url;
                                }
                                this.AddUserData(internalName, userData, url);
                                this.AddUserData(internalName + "#2", userData, description);
                            }
                        }
                        else if (field.Type == AveFieldType.ContentTypeId)
                        {
                            if (internalNameValue != null)
                            {
                                userData.Add("#tp_" + internalName, new AveContentTypeId(internalNameValue.ToString()).ToByteArray());
                            }
                        }
                        else
                        {
                            if (field.ID == AveBuiltInFieldId.MessageId)
                            {
                                continue;
                            }
                            if (internalNameValue != null)
                            {
                                this.AddUserData(internalName, userData, internalNameValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveObjectModel_CommonResource.GetUserDataError, field.Title, this.Url, e.ToString());
                        //mLog.Warn("fieldName:{0}, fieldValue:{1}", field.Title, itemVersion[field.Title].ToString());
                    }
                }
                if (itemVersion.IsCurrentVersion)
                {
                    object timeCreated;
                    if (DataCache.IsPropertyAvailable("TimeCreated"))
                    {
                        userData["Created"] = DataCache.GetProperty<DateTime>("TimeCreated");
                    }
                    else if (FieldValues.TryGetValue("Created", out timeCreated))
                    {
                        userData["Created"] = timeCreated;
                    }
                }
                if (this.ParentList != null &&
                    this.ParentList.BaseTemplate == AveListTemplateType.SolutionCatalog &&
                    userData.ContainsKey("SolutionId"))
                {
                    userData["#SolutionStatus"] = userData.ContainsKey("Status") ? userData["Status"] : 0;
                    userData.Remove("Status");
                }
                if (this.ParentList.BaseTemplate == AveListTemplateType.DiscussionBoard && itemVersion["#ThreadIndexParentId"] != null)
                {
                    userData["#ThreadIndexParentId"] = itemVersion["#ThreadIndexParentId"];
                }
                RemoveUnvalidTaxonomyValues(userData, itemVersion);
            }
            if (this.ParentList.BaseTemplate == AveListTemplateType.Meetings && this["InstanceID"] != null)
            {
                userData["#tp_InstanceID"] = this["InstanceID"];
            }
            return userData;
        }

        private void RemoveUnvalidTaxonomyValues(Dictionary<string, object> columnValues, IAveListItemVersion itemVersion)
        {
            try
            {
                foreach (IAveField field in itemVersion.Fields)
                {
                    try
                    {
                        IAveTaxonomyField taxonomyField = field as IAveTaxonomyField;
                        if (taxonomyField != null)
                        {
                            Guid textFieldId = taxonomyField.TextField;
                            if (textFieldId != Guid.Empty)
                            {
                                IAveField textField = null;
                                try
                                {
                                    textField = itemVersion.Fields.FirstOrDefault(t => t.ID == textFieldId);
                                }
                                catch (Exception e)
                                {
                                    mLogger.Warn("Get text field by id {0} failed.Error:{1}", textFieldId, e);
                                }
                                //handle the text field value if the text field exist
                                if (textField != null)
                                {
                                    //没有值则把textfield的值也置成空，如果有值则把text field的值置成taxonomyfield的值，防止出现textfield和taxonomyfield值不一致的case
                                    if (string.IsNullOrEmpty(itemVersion[field.InternalName] as string))
                                    {
                                        if (columnValues.ContainsKey(textField.InternalName))
                                        {
                                            columnValues.Remove(textField.InternalName);
                                        }
                                    }
                                    else
                                    {
                                        columnValues[textField.InternalName] = itemVersion[field.InternalName];
                                    }
                                }
                                else
                                {
                                    mLogger.Warn("Text field with Id {0} not found for taxonomy field {1}[{2}].", textFieldId, taxonomyField.ID, taxonomyField.InternalName);
                                }

                            }
                            else
                            {
                                mLogger.Warn("Text field with Id {0} not found for taxonomy field {1}[{2}].", textFieldId, taxonomyField.ID, taxonomyField.InternalName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("RemoveUnvalidTaxonomyValues for item field failed.Iteminfo:{0},{1}.FieldInfo:[{2};{3}],Error:{4}", itemVersion.Url, itemVersion.VersionId, field.ID, field.Title, e);
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("RemoveUnvalidTaxonomyValues for item failed.Iteminfo:{0},{1}.Error:{2}", itemVersion.Url, itemVersion.VersionId, e);
            }
        }

        private void AddUserData(string fieldName, Dictionary<string, object> userData, object value)
        {
            if (ChangeMapping.ContainsKey(fieldName))
            {
                userData.Add(ChangeMapping[fieldName], value);
            }
            else if (ModifyMapping.Contains(fieldName))
            {
                userData.Add("#tp_" + fieldName.TrimStart('_'), value);
            }
            else
            {
                userData.Add(fieldName, value);
            }
        }

        private void AddUserDataJunction(int itemVersion, Guid fieldId, object value, ref List<Dictionary<string, object>> userDataJunction)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return;
            }
            if (userDataJunction == null)
            {
                userDataJunction = new List<Dictionary<string, object>>();
            }
            Dictionary<string, object> tempJunctionData;
            AveFieldLookupValueCollection multiValues = new AveFieldLookupValueCollection(value.ToString());
            foreach (AveFieldLookupValue tempValue in multiValues)
            {
                tempJunctionData = new Dictionary<string, object>();
                tempJunctionData["tp_FieldId"] = fieldId;
                tempJunctionData["tp_Id"] = tempValue.LookupId;
                tempJunctionData["tp_UIVersion"] = itemVersion;
                //获取对应column value。还原lookup column时使用value进行item的对应查找
                tempJunctionData["tp_Value"] = tempValue.LookupValue;
                userDataJunction.Add(tempJunctionData);
            }
        }

        private Dictionary<string, object> ConvertCurrentVersionProperties(Dictionary<string, object> itemCurrentVersionFieldValues, Dictionary<string, string> needLoadFields)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            var itemVersionPropertiesList = new List<IDictionary<string, object>>();
            listItemVersionsProperties.AddChildren(itemVersionPropertiesList);
            Dictionary<string, object> listItemVersionProperties = new Dictionary<string, object>(); ;
            itemVersionPropertiesList.Add(listItemVersionProperties);
            Dictionary<string, object> itemVersionFieldValues = new Dictionary<string, object>();
            listItemVersionProperties["FieldValues"] = itemVersionFieldValues;

            Dictionary<string, string> fieldNameMapping = new Dictionary<string, string>()
                { { "_UIVersion","VersionId" }, { "_UIVersionString","VersionLabel" },
                { "_Level","Level" },{ "_IsCurrentVersion","IsCurrentVersion" },
                { "FileRef","Url" },{ "File_x0020_Size","Length" },
                { "_ModerationStatus","ModerationStatus" },{ "Created_x0020_By","CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix }};

            foreach (KeyValuePair<string, object> kv in itemCurrentVersionFieldValues)
            {
                if (kv.Value != null && needLoadFields.ContainsKey(kv.Key))
                {
                    string key = fieldNameMapping.ContainsKey(kv.Key) ? fieldNameMapping[kv.Key] : kv.Key;
                    object value = GetValueFromType(needLoadFields[kv.Key], kv.Value);
                    listItemVersionProperties[key] = value;
                    itemVersionFieldValues[kv.Key] = value;
                }
            }
            if (itemCurrentVersionFieldValues.ContainsKey("Modified"))
            {
                object value = GetValueFromType("DateTime", itemCurrentVersionFieldValues["Modified"]);
                listItemVersionProperties["Modified"] = value;
                itemVersionFieldValues["Modified"] = value;
            }
            //获得editor属性
            if (itemCurrentVersionFieldValues.ContainsKey("Editor"))
            {
                object value = GetValueFromType("User", itemCurrentVersionFieldValues["Editor"]);
                listItemVersionProperties["Editor"] = value;
                itemVersionFieldValues["Editor"] = value;
            }

            if (itemCurrentVersionFieldValues.ContainsKey("_Level"))
            {
                listItemVersionProperties["Level"] = byte.Parse(itemCurrentVersionFieldValues["_Level"].ToString());
            }
            if (itemCurrentVersionFieldValues.ContainsKey("#ThreadIndexParentId"))
            {
                itemVersionFieldValues["#ThreadIndexParentId"] = itemCurrentVersionFieldValues["#ThreadIndexParentId"];
            }
            return listItemVersionsProperties;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private object GetValueFromType(string vtype, object svalue)
        {
            switch (vtype)
            {
                case "BusinessData":
                case "Choice":
                case "ContactInfo":
                case "ContentTypeId":
                case "FreeBusy":
                case "HTML":
                case "Note":
                case "Overbook":
                case "Text":
                case "URL":
                case "Whereabout":
                case "Image":
                case "ThreadIndex":
                case "Link":
                case "MediaFieldType":
                case "OutcomeChoice":
                    return svalue.ToString();
                case "Calculated":
                case "CallTo":
                case "SendTo":
                case "Lookup":
                case "LookupMulti":
                case "User":
                case "UserMulti":
                case "GridChoice": //for survey list rating scale field
                    return svalue;
                case "DateTime":
                    return DateTime.Parse(svalue.ToString(), CultureInfo.CreateSpecificCulture("en-US"));
                case "AverageRating":
                case "Currency":
                case "Number":
                case "RatingCount":
                    return double.Parse(svalue.ToString());
                case "Guid":
                    return new Guid(svalue.ToString());
                case "AllDayEvent":
                case "Boolean":
                case "Recurrence":
                case "CrossProjectLink"://item if relative workspace
                    return bool.Parse(svalue.ToString());
                case "Counter":
                case "Integer":
                case "ModStat":
                    return int.Parse(svalue.ToString());
                case "MultiChoice":
                    string[] multiV = svalue as string[];
                    if (multiV != null && multiV.Length > 0)
                    {
                        if (multiV.Length == 1)
                        {
                            return multiV[0];
                        }
                        else
                        {
                            StringBuilder stb = new StringBuilder(";#");
                            foreach (string v in multiV)
                            {
                                stb.Append(v + ";#");
                            }
                            return stb.ToString();
                        }
                    }
                    else if (svalue != null)
                    {
                        return svalue.ToString();
                    }
                    break;
                case "Computed":
                    break;
                default:
                    if (svalue != null)
                    {
                        return svalue;
                    }
                    break;
            }
            return null;
        }
        #endregion

        public Guid GetTPGuid(Guid parentId)
        {
            if (this.ParentList.BaseTemplate != AveListTemplateType.Survey)
            {
                return (Guid)this["GUID"];
            }
            return Guid.Empty;
        }


        public Guid GetTPGuid()
        {
            return GetTPGuid(Guid.Empty);
        }

        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                if (this.HasUniqueRoleAssignments)
                {
                    return new AveSecurableObjectImpl(this.UniqueId, this.RoleAssignments);
                }
                else
                {
                    if (this.Folder != null && this.Folder.ParentFolder != null)
                    {
                        if (this.Folder.ParentFolder.Item != null)
                        {
                            return this.Folder.ParentFolder.Item.SecurableObjectImpl;
                        }
                        else
                        {
                            return this.ParentList.SecurableObjectImpl;
                        }
                    }
                    else if (this.File != null && this.File.ParentFolder != null)
                    {
                        if (this.File.ParentFolder.Item != null)
                        {
                            return this.File.ParentFolder.Item.SecurableObjectImpl;
                        }
                        else
                        {
                            return this.ParentList.SecurableObjectImpl;
                        }
                    }
                    else
                    {
                        int index = this.Url.LastIndexOf('/');
                        string folderServerRelativeUrl = this.Url.Substring(0, index);
                        if (!folderServerRelativeUrl.StartsWith(mParentWeb.ServerRelativeUrl))
                        {
                            folderServerRelativeUrl = mParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + folderServerRelativeUrl.Trim('/');
                        }
                        IAveFolder parentFolder = mParentWeb.GetFolder(folderServerRelativeUrl);
                        if (parentFolder.Item != null)
                        {
                            return parentFolder.Item.SecurableObjectImpl;
                        }
                        else
                        {
                            return this.ParentList.SecurableObjectImpl;
                        }
                    }
                }
            }
        }
        public AveDictionary<Guid, AveSharingLinkInfo> SharingLinks
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SharingLinks"))
                {
                    AveDictionary<Guid, AveSharingLinkInfo> sharingLinks = mRequest.GetListItemSharingLinks(mParentWeb.Url, ParentList.ID, this.ID);
                    base.DataCache.AddProperty("SharingLinks", sharingLinks);
                    return sharingLinks;
                }
                return base.DataCache.GetProperty<AveDictionary<Guid, AveSharingLinkInfo>>("SharingLinks");
            }
        }
        private void SetChangeProperty(object key, object value)
        {
            if (key == null)
            {
                return;
            }
            if (!this.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
            {
                this.DataCache.ChangedProperties["ChangedFieldValues"] = new Dictionary<string, object>();
            }
            Dictionary<string, object> changedProperties = this.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>;
            if (!changedProperties.ContainsKey("ChangeMetaInfo"))
            {
                changedProperties.Add("ChangeMetaInfo", new Hashtable());
                changedProperties.Add("MetaInfo", this.FieldValues["MetaInfo"]);
            }
            (changedProperties["ChangeMetaInfo"] as Hashtable).Add(key, value);
        }


        public DateTime GetLastAccessTime(Guid id, string folderServerRelativeUrl, DateTime modified, bool isCompatibleByModifiedTime = false)
        {
            return mRequest.QueryLastAccessTime(id, folderServerRelativeUrl, modified, isCompatibleByModifiedTime);
        }
        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author"))
                {
                    string filesUserString = FieldValues["Author"].ToString();
                    var userId = filesUserString.Substring(0, filesUserString.IndexOf(";#"));
                    int id = 0;
                    if (int.TryParse(userId, out id))
                    {
                        AveUser author = this.Web.SiteUsers.GetByID(id) as AveUser;
                        base.DataCache.AddProperty("Author", author);
                    }
                    else
                    {
                        base.DataCache.AddProperty("Author", null);
                    }
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }

        public IAveUser ModifiedBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ModifiedBy"))
                {
                    string filesUserString = FieldValues["Editor"].ToString();
                    var userId = filesUserString.Substring(0, filesUserString.IndexOf(";#"));
                    int id = 0;
                    if (int.TryParse(userId, out id))
                    {
                        AveUser editor = this.Web.SiteUsers.GetByID(id) as AveUser;
                        base.DataCache.AddProperty("ModifiedBy", editor);
                    }
                    else
                    {
                        base.DataCache.AddProperty("ModifiedBy", null);
                    }
                }
                return base.DataCache.GetProperty<IAveUser>("ModifiedBy");
            }
        }
    }
}