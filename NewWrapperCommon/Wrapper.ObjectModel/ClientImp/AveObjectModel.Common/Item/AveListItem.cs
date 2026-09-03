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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/02/29", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    class AveListItem : AveSecurableObject, IAveListItem
    {
        private IAveWeb mParentWeb;
        private bool mIsNewCreated;
        public static Dictionary<string, string> ChangeMapping = new Dictionary<string, string>();
        public static List<string> ModifyMapping = new List<string>();
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
            ModifyMapping.Add("Size");
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
            ModifyMapping.Add("WorkflowInstanceID");
        }

        public AveListItem(IAveRequest request, IAveWeb parentWeb, IAveList parentList, Dictionary<string, object> itemProperties, bool newCreated)
            : base(request)
        {
            mIsNewCreated = newCreated;
            mParentWeb = parentWeb;
            mRequest = request;
            if (itemProperties != null)
            {
                itemProperties["Web"] = parentWeb;
                if (parentList != null)
                {
                    itemProperties["ParentList"] = parentList;
                    itemProperties["Fields"] = parentList.Fields;
                }
                base.DataCache.AddPropertyies(itemProperties);
            }
        }

        internal override void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties)
        {
            roleAssignmentProperties[AveObjectModelConstant.WebServerRelativeUrl] = mParentWeb.ServerRelativeUrl;
            roleAssignmentProperties[AveObjectModelConstant.ListTitle] = this.ParentList.Title;
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
                    base.DataCache.PropertiesCache["ContentType"] = contentType;
                }
                return base.DataCache.GetProperty<IAveContentType>("ContentType");
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get { return ContentType.ID; }
        }

        public string DisplayName
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayName");
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
                return base.DataCache.GetProperty<IAveFieldCollection>("Fields");
            }
        }

        public IAveWorkflowCollection WorkFlows
        {
            get { throw new NotImplementedException(); }
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
                        fileProperties["Item"] = this;
                        file = new AveFile(mRequest, mParentWeb as AveWeb, this.ParentList as AveList, null, fileProperties);
                    }
                    else
                    {
                        file = mParentWeb.GetFile(this.Url);
                    }
                    base.DataCache.PropertiesCache["File"] = file;
                    return file;
                }
                return base.DataCache.GetProperty<IAveFile>("File");
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
                if (base.DataCache.IsPropertyNotLoaded("Attachments"))
                {
                    Dictionary<string, object> attachmentColProperties = mRequest.GetAttachments(mParentWeb.ServerRelativeUrl, ParentList.Title, this.ID);
                    AveAttachmentCollection attachmentCollection = new AveAttachmentCollection(this, mRequest, attachmentColProperties);
                    base.DataCache.PropertiesCache["Attachments"] = attachmentCollection;
                    return attachmentCollection;
                }
                return base.DataCache.GetProperty<IAveAttachmentCollection>("Attachments");
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
                //if (this.FieldValues.ContainsKey("MetaInfo") && this.FieldValues["MetaInfo"] != null)
                //{
                if (this.DataCache.IsPropertyNotLoaded("Properties"))
                {
                    //        Hashtable MetaInfoTable = new MetaInfoHandler(this.FieldValues["MetaInfo"].ToString()).ToHashtable();
                    base.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable(new Hashtable(), SetChangeProperty);
                }
                else
                {
                    if (!(base.DataCache.PropertiesCache["Properties"] is AveCustomHashtable))
                    {
                        base.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable((base.DataCache.PropertiesCache["Properties"] as Hashtable), SetChangeProperty);
                    }
                }
                //}
                return base.DataCache.PropertiesCache["Properties"] as Hashtable;
            }
        }

        private void SetChangeProperty(object key, object value)
        {
            if (!this.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
            {
                this.DataCache.ChangedProperties["ChangedFieldValues"] = new Dictionary<string, object>();
            }
            Dictionary<string, object> changedProperties = this.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>;
            if (!changedProperties.ContainsKey("Properties"))
            {
                changedProperties.Add("Properties", new Hashtable());
            }
            (changedProperties["Properties"] as Hashtable).Add(key, value);
        }


        public int UIVersion
        {
            get
            {
                return Convert.ToInt32(this["_UIVersion"]);
            }
        }

        public object this[Guid fieldId]
        {
            get
            {
                IAveField field = this.Fields[fieldId];
                return GetFieldValueByInternalName(field);
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
                IAveField field = this.Fields.GetField(fieldName);
                return GetFieldValueByInternalName(field);
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

        private object GetFieldValueByInternalName(IAveField field)
        {
            string fieldName = field.InternalName;
            if (base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
            {
                Dictionary<string, object> fieldValues = base.DataCache.GetProperty<Dictionary<string, object>>("ChangedFieldValues");
                if (fieldValues.ContainsKey(fieldName))
                {
                    return fieldValues[fieldName];
                }
            }
            if (base.DataCache.IsPropertyAvailable("FieldValues"))
            {
                Dictionary<string, object> oldFieldValues = base.DataCache.PropertiesCache["FieldValues"] as Dictionary<string, object>;
                if (oldFieldValues.ContainsKey(fieldName))
                {
                    object obj = oldFieldValues[fieldName];
                    if (field is IAveTaxonomyField && obj is Dictionary<string, object>)
                    {
                        var value = GetMMSColumnValue(obj);
                        return value ?? obj;
                    }
                    return obj;
                }
                else
                {
                    if(field is IAveTaxonomyField)//caml query can not get simulation 365 taxonomy column value. retry get with text field.
                    {
                        var textField = ParentList.Fields.GetById((field as IAveTaxonomyField).TextField);
                        if(textField != null && oldFieldValues.ContainsKey(textField.InternalName))
                        {
                            return oldFieldValues[textField.InternalName];
                        }
                    }
                }
            }
            return null;
        }

        private string GetMMSColumnValue(object obj)
        {
            Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
            if (dictionary.ContainsKey("_ObjectType_"))
            {
                if (dictionary["_ObjectType_"].ToString() == "SP.Taxonomy.TaxonomyFieldValue")
                {
                    return dictionary["Label"].ToString() + "|" + dictionary["TermGuid"].ToString();
                }
                else if (dictionary["_ObjectType_"].ToString() == "SP.Taxonomy.TaxonomyFieldValueCollection")
                {
                    Object[] values = dictionary["_Child_Items_"] as Object[];
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (object value in values)
                    {
                        if (value == null)
                        {
                            continue;
                        }
                        Dictionary<string, object> dic = value as Dictionary<string, object>;
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.Append(';');
                        }
                        builder.Append(dic["Label"]);
                        builder.Append("|");
                        builder.Append(dic["TermGuid"]);
                    }
                    return builder.ToString();
                }
            }
            return null;
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
                    base.DataCache.PropertiesCache["Url"] = url;
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
                        folderProperties["Item"] = this;
                        folder = new AveFolder(mRequest, mParentWeb, ParentList, null, folderProperties);
                    }
                    else
                    {
                        folder = mParentWeb.GetFolder(this.mParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + this.Url);
                    }
                    base.DataCache.PropertiesCache["Folder"] = folder;
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
                        base.DataCache.PropertiesCache["ModerationInformation"] = new AveModerationInformation(mRequest, this);
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

        private bool HasVersion
        {
            get
            {
                return this.ParentList.BaseTemplate != AveListTemplateType.UserInformation && (this.ParentList.BaseType == AveBaseType.DocumentLibrary || Convert.ToInt32(this["UIVersion"]) > 512);
            }
        }

        public IAveListItemVersionCollection Versions
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Versions" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> listItemVersionColProperties = null;
                    if ((base.DataCache.PropertiesCache.ContainsKey("HasVersion") && !Convert.ToBoolean(base.DataCache.PropertiesCache["HasVersion"])) || !WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        listItemVersionColProperties = this.ConvertCurrentVersionProperties(this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues"), (this.ParentList as AveList).NeedLoadFields);
                    }
                    else
                    {
                        CultureInfo cultureInfo = new CultureInfo(Convert.ToInt32(mParentWeb.RegionalSettings.LocaleId));
                        listItemVersionColProperties = mRequest.GetItemVersions(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.ID.ToString(), this.ID, this.ServerRelativeUrl, cultureInfo, (this.ParentList as AveList).NeedLoadFields);
                        if (listItemVersionColProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(listItemVersionColProperties["HasVersion"]))
                        {
                            listItemVersionColProperties = this.ConvertCurrentVersionProperties(this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues"), (this.ParentList as AveList).NeedLoadFields);
                        }
                        else if (this.ParentList.BaseTemplate == AveListTemplateType.DiscussionBoard)
                        {
                            EnsureParentThreadIndex(listItemVersionColProperties);
                        }
                    }
                    AveListItemVersionCollection listItemVerCol = new AveListItemVersionCollection(this, mRequest, listItemVersionColProperties);
                    base.DataCache.PropertiesCache["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = listItemVerCol;
                    return listItemVerCol;
                }
                return base.DataCache.GetProperty<IAveListItemVersionCollection>("Versions" + AveObjectModelConstant.ObjectPropertySuffix);
            }
        }

        private void EnsureParentThreadIndex(Dictionary<string, object> listItemVersionColProperties)
        {
            try
            {
                object value;
                if (this.DataCache.GetProperty<Dictionary<string, object>>("FieldValues").TryGetValue("#ThreadIndexParentId", out value))
                {
                    List<Dictionary<string, object>> versions = listItemVersionColProperties["ChildrenProperties"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> tempVersion in versions)
                    {
                        Dictionary<string, object> fieldValues = tempVersion["FieldValues"] as Dictionary<string, object>;
                        fieldValues["#ThreadIndexParentId"] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Debug(string.Format("Can not get parent thread index id for discussion board.Message:{0}", ex));
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

        public void SystemUpdate()
        {
            this.UpdateOperation("SystemUpdate");
        }
        public void SystemUpdateForRecords()
        {
            this.UpdateOperationForRecords();
        }
        public void Delete()
        {
            mRequest.DeleteItem(mParentWeb.ServerRelativeUrl, ParentList.DefaultViewUrl, ParentList.Title, ParentList.ID, this.ID);
            //(ParentList.Items as AveListItemCollection).ListData.Remove(this);
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
                    base.DataCache.PropertiesCache["RoleAssignments"] = roleAssignments;
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }
        #endregion

        #region private method
        private void UpdateOperation(string updateMethod)
        {
            bool isMinVersion = false;
            Dictionary<string, object> newProp = null;
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                if (updateMethod.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    //no need to change;
                }
                else if (updateMethod.StartsWith("SystemUpdate", StringComparison.OrdinalIgnoreCase) && updateMethod.Length > "SystemUpdate".Length)
                {
                    updateMethod = "SystemUpdateAPI";
                }
                else
                {
                    updateMethod = "SystemUpdate";
                }
                try
                {
                    if (this.FieldValues.ContainsKey("_UIVersion"))
                    {
                        int uiVersison = (int)this.FieldValues["_UIVersion"];
                        isMinVersion = uiVersison % 512 != 0;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Check is minversion {0}:{1}", this.ID, ex.ToString());
                }
                base.DataCache.AddChangedProperty(AveObjectModelConstant.UpdateMethodName, updateMethod);
                base.DataCache.AddChangedProperty("EnableVersioning", ParentList.EnableVersioning);
                base.DataCache.AddChangedProperty("EnableMinorVersions", ParentList.EnableMinorVersions);
                base.DataCache.AddChangedProperty("EnableModeration", ParentList.EnableModeration);
                base.DataCache.AddChangedProperty("IsCurrentMinorVersion", isMinVersion);
                base.DataCache.AddChangedProperty("IsApproved", this.ModerationInformation == null ? false : this.ModerationInformation.Status == AveModerationStatusType.Approved);
                base.DataCache.AddChangedProperty("FileSystemObjectType", (int)this.FileSystemObjectType);
                base.DataCache.AddChangedProperty("IsCurrentCheckOut", this.FieldValues != null && this.FieldValues.ContainsKey("_Level") ? this.FieldValues["_Level"].Equals(255) : false);


                if (base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
                {
                    ConvertFieldValuesToString(base.DataCache.ChangedProperties);
                    Dictionary<string, object> fieldValues = base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>;

                    if ("SystemUpdate".Equals(updateMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!fieldValues.ContainsKey("FileLeafRef"))
                        {
                            if (!string.IsNullOrEmpty(this["FileLeafRef"] as string))
                            {
                                fieldValues["FileLeafRef"] = this["FileLeafRef"];
                            }
                            else if (fieldValues.ContainsKey("Title"))
                            {
                                fieldValues["FileLeafRef"] = fieldValues["Title"];
                            }
                            else
                            {
                                fieldValues["FileLeafRef"] = this.Title;
                            }
                        }
                    }

                    if (this["Modified"] != null)
                    {
                        if (this["Modified"] is DateTime)//对于非DateTime类型不进行处理 ,因为无法判断Kind
                        {
                            DateTime modified = (DateTime)this["Modified"];
                            //通过Web.GetFile 取出的item 的Modified是Local Time,在更新时需要转为UTC时间
                            //否则在web时区与系统时区不同的情况下,SystemUpdate会导致Modified 改变
                            //这是因为更新时传入的是Local时间 而Client API只识别UTC 时间 因此将Local 时间识别为UTC时间导致的。
                            //因此需要加特殊处理
                            if (modified.Kind == DateTimeKind.Local)
                            {
                                var timeZoneInfo = AveTimeZoneUtility.ToTimeZoneInfo(this.mParentWeb.RegionalSettings.TimeZone);
                                modified = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(modified, DateTimeKind.Unspecified), timeZoneInfo);
                            }
                            fieldValues["Modified"] = modified;
                        }
                        else
                        {
                            fieldValues["Modified"] = this["Modified"];
                        }
                    }
                    if (this.ParentList.BaseType != AveBaseType.DocumentLibrary)
                    {
                        if (this["_ModerationStatus"] != null)
                        {
                            fieldValues["_ModerationStatus"] = this["_ModerationStatus"];
                        }
                        if (this["Editor"] != null)
                        {
                            fieldValues["Editor"] = this["Editor"];
                        }
                    }
                }

                if (mIsNewCreated == true)
                {
                    base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                    newProp = this.mRequest.AddItem(
                        this.mParentWeb.ServerRelativeUrl,
                        this.ParentList.Title,
                        this.ParentList.ID,
                        base.DataCache.GetProperty<string>("folderUrl"),
                        base.DataCache.GetProperty<int>("parentId"),
                        base.DataCache.GetProperty<int>("FileSystemObjectType"),
                        base.DataCache.GetProperty<string>("leafName"),
                        base.DataCache.ChangedProperties,
                        base.DataCache.GetProperty<bool>("isDiscussion"));
                    mIsNewCreated = false;
                }
                else
                {
                    newProp = this.mRequest.UpdateItem(this.mParentWeb.ServerRelativeUrl, this.ParentList.Title, this.ParentList.ID, this.ID, base.DataCache.ChangedProperties);
                }
            }
            else if (mIsNewCreated == true) //ADO-58621 有些list没有对应属性更新(Title)所以changedProperties是空的
            {
                base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                newProp = this.mRequest.AddItem(
                    this.mParentWeb.ServerRelativeUrl,
                    this.ParentList.Title,
                    this.ParentList.ID,
                    base.DataCache.GetProperty<string>("folderUrl"),
                    base.DataCache.GetProperty<int>("parentId"),
                    base.DataCache.GetProperty<int>("FileSystemObjectType"),
                    base.DataCache.GetProperty<string>("leafName"),
                    base.DataCache.ChangedProperties,
                    base.DataCache.GetProperty<bool>("isDiscussion"));
                mIsNewCreated = false;
            }
            base.DataCache.UpdateProperties(newProp);
        }
        private void UpdateOperationForRecords()
        {
            Dictionary<string, object> newProp = null;
            bool isMinVersion = false;
            string updateMethod = "SystemUpdateForRecords";
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                //if (updateMethod.Equals("Update", StringComparison.OrdinalIgnoreCase))
                //{
                //    //no need to change;
                //}
                //else if (updateMethod.StartsWith("SystemUpdate", StringComparison.OrdinalIgnoreCase) && updateMethod.Length > "SystemUpdate".Length)
                //{
                //    updateMethod = "SystemUpdateAPI";
                //}
                //else
                //{
                //    updateMethod = "SystemUpdate";
                //}
                try
                {
                    if (this.FieldValues.ContainsKey("_UIVersion"))
                    {
                        int uiVersison = (int)this.FieldValues["_UIVersion"];
                        isMinVersion = uiVersison % 512 != 0;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Info("Check is minversion {0}:{1}", this.ID, ex.ToString());
                    isMinVersion = false;
                }

                base.DataCache.AddChangedProperty(AveObjectModelConstant.UpdateMethodName, updateMethod);
                base.DataCache.AddChangedProperty("EnableVersioning", ParentList.EnableVersioning);
                base.DataCache.AddChangedProperty("EnableMinorVersions", ParentList.EnableMinorVersions);
                base.DataCache.AddChangedProperty("EnableModeration", ParentList.EnableModeration);
                base.DataCache.AddChangedProperty("IsCurrentMinorVersion", isMinVersion);//this.File == null ? false : this.File.MinorVersion != 0);
                base.DataCache.AddChangedProperty("IsApproved", this.ModerationInformation == null ? false : this.ModerationInformation.Status == AveModerationStatusType.Approved);
                base.DataCache.AddChangedProperty("FileSystemObjectType", (int)this.FileSystemObjectType);
                base.DataCache.AddChangedProperty("IsCurrentCheckOut", this.FieldValues != null && this.FieldValues.ContainsKey("_Level") ? this.FieldValues["_Level"].Equals(255) : false);


                if (base.DataCache.ChangedProperties.ContainsKey("ChangedFieldValues"))
                {
                    ConvertFieldValuesToString(base.DataCache.ChangedProperties);
                    Dictionary<string, object> fieldValues = base.DataCache.ChangedProperties["ChangedFieldValues"] as Dictionary<string, object>;

                    if ("SystemUpdate".Equals(updateMethod, StringComparison.OrdinalIgnoreCase) || "SystemUpdateForRecords".Equals(updateMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!fieldValues.ContainsKey("FileLeafRef"))
                        {
                            if (!string.IsNullOrEmpty(this["FileLeafRef"] as string))
                            {
                                fieldValues["FileLeafRef"] = this["FileLeafRef"];
                            }
                            else if (fieldValues.ContainsKey("Title"))
                            {
                                fieldValues["FileLeafRef"] = fieldValues["Title"];
                            }
                            else
                            {
                                fieldValues["FileLeafRef"] = this.Title;
                            }
                        }
                    }

                    if (this["Modified"] != null)
                    {
                        if (this["Modified"] is DateTime)//对于非DateTime类型不进行处理 ,因为无法判断Kind
                        {
                            DateTime modified = (DateTime)this["Modified"];
                            //通过Web.GetFile 取出的item 的Modified是Local Time,在更新时需要转为UTC时间
                            //否则在web时区与系统时区不同的情况下,SystemUpdate会导致Modified 改变
                            //这是因为更新时传入的是Local时间 而Client API只识别UTC 时间 因此将Local 时间识别为UTC时间导致的。
                            //因此需要加特殊处理
                            if (modified.Kind == DateTimeKind.Local)
                            {
                                var timeZoneInfo = AveTimeZoneUtility.ToTimeZoneInfo(this.mParentWeb.RegionalSettings.TimeZone);
                                modified = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(modified, DateTimeKind.Unspecified), timeZoneInfo);
                                //modified = this.mParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(modified);
                            }
                            fieldValues["Modified"] = modified;
                        }
                        else
                        {
                            fieldValues["Modified"] = this["Modified"];
                        }
                    }
                    if (this.ParentList.BaseType != AveBaseType.DocumentLibrary)
                    {
                        if (this["_ModerationStatus"] != null)
                        {
                            fieldValues["_ModerationStatus"] = this["_ModerationStatus"];
                        }
                        if (this["Editor"] != null)
                        {
                            fieldValues["Editor"] = this["Editor"];
                        }
                    }
                }

                if (mIsNewCreated == true)
                {
                    base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                    newProp = this.mRequest.AddItem(
                        this.mParentWeb.ServerRelativeUrl,
                        this.ParentList.Title,
                        this.ParentList.ID,
                        base.DataCache.GetProperty<string>("folderUrl"),
                        base.DataCache.GetProperty<int>("parentId"),
                        base.DataCache.GetProperty<int>("FileSystemObjectType"),
                        base.DataCache.GetProperty<string>("leafName"),
                        base.DataCache.ChangedProperties,
                        base.DataCache.GetProperty<bool>("isDiscussion"));
                    mIsNewCreated = false;
                }
                else
                {
                    newProp = this.mRequest.UpdateItem(this.mParentWeb.ServerRelativeUrl, this.ParentList.Title, this.ParentList.ID, this.ID, base.DataCache.ChangedProperties);
                }
            }
            else if (mIsNewCreated == true) //ADO-58621 有些list没有对应属性更新(Title)所以changedProperties是空的
            {
                base.DataCache.ChangedProperties[AveObjectModelConstant.UpdateMethodName] = "Update";
                newProp = this.mRequest.AddItem(
                    this.mParentWeb.ServerRelativeUrl,
                    this.ParentList.Title,
                    this.ParentList.ID,
                    base.DataCache.GetProperty<string>("folderUrl"),
                    base.DataCache.GetProperty<int>("parentId"),
                    base.DataCache.GetProperty<int>("FileSystemObjectType"),
                    base.DataCache.GetProperty<string>("leafName"),
                    base.DataCache.ChangedProperties,
                    base.DataCache.GetProperty<bool>("isDiscussion"));
                mIsNewCreated = false;
            }
            base.DataCache.UpdateProperties(newProp);
        }

        internal void ConvertFieldValuesToString(Dictionary<string, object> fieldValues)
        {
            var changedField = fieldValues["ChangedFieldValues"] as Dictionary<string, object>;

            if (changedField.ContainsKey("DeleteAttachment"))
            {
                changedField.Remove("DeleteAttachment");
            }

            Dictionary<string, object> fields = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> fieldValue in changedField)
            {
                if (fieldValue.Key.Equals("Properties"))
                {
                    fields[fieldValue.Key] = fieldValue.Value;
                    continue;
                }
                object value = fieldValue.Value;
                if (value is AveFieldValueInfo)
                {
                    value = (fieldValue.Value as AveFieldValueInfo).ColValue;
                }
                else if (value is DateTime)
                {
                    fields[fieldValue.Key] = value;
                    continue;
                }
                fields[fieldValue.Key] = value == null ? string.Empty : value.ToString();
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
            if (this.FieldValues.ContainsKey("MetaInfo"))
            {
                string str = this.FieldValues["MetaInfo"].ToString();
                docInfo["MetaInfo"] = AveCompressedUtility.GetTCompressedBytes(str);
                Dictionary<string, string> dicMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(str);
                if (dicMetaInfo.ContainsKey("vti_setuppath"))
                {
                    docInfo["SetupPath"] = dicMetaInfo["vti_setuppath"];
                    docInfo["HasStream"] = 0;
                }
                else { docInfo["HasStream"] = 1; }
                baseItemInfo.HasStream = (int)docInfo["HasStream"] == 1 ? true : false;
            }
            AveFile file = this.File as AveFile;
            if (file != null)
            {
                if (file.UIVersion == baseItemInfo.Version && file.CheckOutType != AveCheckOutType.None)
                {
                    isCheckOut = true;
                }
                docInfo["LeafName"] = file.Name;
                docInfo["CustomizedPageStatus"] = (int)file.CustomizedPageStatus;
            }
            docInfo["IsCheckOut"] = isCheckOut;
            if (isCheckOut)//ADO-181371 Online 不支持备份Checkout 的文件，因此，如果数据是Checkout 状态的，那么Checkout user 肯定是当前user
            {
                docInfo["CheckoutUserId"] = this.mParentWeb.CurrentUser.ID;
            }
            docInfo["HasUniqueRoleAssignments"] = this.HasUniqueRoleAssignments;
            baseItemInfo.ScopeUrl = (base.DataCache.GetProperty<string>("FileDirRef") + "/" + base.DataCache.GetProperty<string>("FileLeafRef")).TrimStart('/');
        }

        public bool HasUniqueRoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("HasUniqueRoleAssignments"))
                {
                    base.DataCache.PropertiesCache["HasUniqueRoleAssignments"] = false;
                }
                return base.DataCache.GetProperty<bool>("HasUniqueRoleAssignments");
            }
        }

        /// <summary>
        /// 获取item的基本column信息
        /// </summary>
        /// <param name="baseItemInfo"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listform is a part of url")]
        internal Dictionary<string, object> GetUserData(AveBaseItemInfo baseItemInfo, ref List<Dictionary<string, object>> userDataJunction)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            IAveListItemVersion itemVersion = this.Versions.GetVersionFromID(baseItemInfo.Version);
            Dictionary<string, string> taxonomyTextFieldValueCache = new Dictionary<string, string>();
            if (itemVersion != null)
            {
                foreach (IAveField field in itemVersion.Fields)
                {
                    try
                    {
                        if (itemVersion[field.InternalName] == null || string.IsNullOrEmpty(itemVersion[field.InternalName].ToString()))//避免value值为string.Empty的情况；tostring（）之前先判空；
                        {
                            continue;
                        }
                        if (field is IAveFieldLookup)
                        {
                            //this column is caculated on the fly, shouldn't backup it
                            if ((field as IAveFieldLookup).CountRelated)
                            {
                                continue;
                            }
                            if ((field as IAveFieldLookup).AllowMultipleValues)
                            {
                                try
                                {
                                    if (field is IAveTaxonomyField)
                                    {
                                        var taxonomyField = field as IAveTaxonomyField;
                                        var textField = itemVersion.Fields.GetById(taxonomyField.TextField);
                                        var fieldValue = itemVersion[taxonomyField.InternalName];
                                        if (fieldValue is object[])
                                        {
                                            var array = fieldValue as object[];
                                            StringBuilder textBuilder = new StringBuilder();
                                            for (int k = 0; k < array.Length; k++)
                                            {
                                                textBuilder.AppendFormat("{0};", array[k]);
                                            }
                                            var textValue = textBuilder.ToString().TrimEnd(';');
                                            taxonomyTextFieldValueCache[textField.InternalName] = textValue;
                                            mLogger.Debug("Add taxonomy field to cache.Key:{0},{1},Value:{2}", taxonomyField.InternalName, textField.InternalName, textValue);
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    mLogger.Warn("Analysis taxonomy field multiply value failed,Name: {0}.Error:{1}", field.InternalName, err);
                                }
                                this.AddUserDataJunction(baseItemInfo.Version, field.ID, itemVersion[field.InternalName], ref userDataJunction);
                            }
                            else
                            {
                                if (field.InternalName.Equals("_CheckinComment"))
                                {
                                    string tempValue = itemVersion[field.InternalName].ToString();
                                    this.AddUserData(field.InternalName, userData, tempValue);
                                }
                                else if (itemVersion[field.InternalName] != null)
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
                                        try
                                        {
                                            if (field is IAveTaxonomyField)
                                            {
                                                var taxonomyField = field as IAveTaxonomyField;
                                                var textField = itemVersion.Fields.GetById(taxonomyField.TextField);
                                                var fieldValue = itemVersion[taxonomyField.InternalName];
                                                if (fieldValue is object[])
                                                {
                                                    var array = fieldValue as object[];
                                                    StringBuilder textBuilder = new StringBuilder();
                                                    for (int k = 0; k < array.Length; k++)
                                                    {
                                                        textBuilder.AppendFormat("{0};", array[k]);
                                                    }
                                                    var textValue = textBuilder.ToString().TrimEnd(';');
                                                    taxonomyTextFieldValueCache[textField.InternalName] = textValue;
                                                    mLogger.Debug("Add taxonomy field to cache.Key:{0},{1},Value:{2}", taxonomyField.InternalName, textField.InternalName, textValue);
                                                }
                                                else if (fieldValue is String)
                                                {
                                                    var textValue = fieldValue.ToString();
                                                    taxonomyTextFieldValueCache[textField.InternalName] = textValue;
                                                    mLogger.Debug("Add Single taxonomy field to cache.Key:{0},{1},Value:{2}", taxonomyField.InternalName, textField.InternalName, textValue);
                                                }
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            mLogger.Warn("Analysis taxonomy field value failed,Name: {0}.Error:{1}", field.InternalName, err);
                                        }
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
                            if (itemVersion[field.InternalName] != null)
                            {
                                string tempValue = itemVersion[field.InternalName].ToString();
                                if (tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase) > 0)
                                {
                                    string url = tempValue.Substring(0, tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase));
                                    string description = tempValue.Substring(tempValue.IndexOf(", ", StringComparison.OrdinalIgnoreCase) + 2);
                                    this.AddUserData(field.InternalName, userData, url);
                                    this.AddUserData(field.InternalName + "#2", userData, description);
                                }
                            }
                        }
                        else if (field.Type == AveFieldType.ContentTypeId)
                        {
                            if (itemVersion[field.InternalName] != null)
                            {
                                userData.Add("#tp_" + field.InternalName, new AveContentTypeId(itemVersion[field.InternalName].ToString()).ToByteArray());
                            }
                        }
                        else
                        {
                            if (field.ID == AveBuiltInFieldId.MessageId)
                            {
                                continue;
                            }
                            if (itemVersion[field.InternalName] != null)
                            {
                                this.AddUserData(field.InternalName, userData, itemVersion[field.InternalName]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveObjectModel_CommonResource.GetUserDataError, field.Title, this.Url, e.ToString());
                        //mLog.Warn("fieldName:{0}, fieldValue:{1}", field.Title, itemVersion[field.Title].ToString());
                    }

                }
                if (this.ParentList.BaseTemplate == AveListTemplateType.DiscussionBoard)
                {
                    if (itemVersion["#ThreadIndexParentId"] != null)
                    {
                        userData["#ThreadIndexParentId"] = itemVersion["#ThreadIndexParentId"];
                    }
                    if (itemVersion.ListItem.FieldValues.ContainsKey("LikesCount") && itemVersion.ListItem["LikesCount"] != null)
                    {
                        userData["LikesCount"] = itemVersion.ListItem["LikesCount"];
                        //userData["LikedBy"] = itemVersion.ListItem["LikedBy"]; //item是folder的时候，目的端存在Like源端不存在，则要用源端的给更新一下  
                    }
                }
            }
            if (this.ParentList.Fields.Contains(AveFieldId.AverageRatings) && this.ParentList.Fields.Contains(AveFieldId.RatingsCount) && string.Compare(this.mParentWeb.Site.SPVersion, "15.", StringComparison.OrdinalIgnoreCase) < 0)
            {
                string itemUrl = string.Empty;
                int ratings = -1;
                if (ParentList.BaseType == AveBaseType.DocumentLibrary)
                {
                    itemUrl = mParentWeb.Url.Substring(0, mParentWeb.Url.IndexOf(mParentWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)) + ServerRelativeUrl;
                }
                else
                {
                    itemUrl = mParentWeb.Url.TrimEnd('/') + @"/_layouts/listform.aspx?PageType=4&amp;ListId=" + ParentList.ID.ToString("B") + "&amp;ID=" + this.ID;
                }
                ratings = mRequest.GetListItemRatings(itemUrl);
                if (ratings != -1)
                {
                    userData["CurrentUserRatings"] = ratings;
                }
            }
            if (this.ParentList.BaseTemplate == AveListTemplateType.SolutionCatalog && userData.ContainsKey("SolutionId"))
            {
                Guid solutionId = new Guid(userData["SolutionId"].ToString());
                userData["#SolutionStatus"] = baseItemInfo.MappingManager.SiteMappingManager.SolutionStatus.ContainsKey(solutionId) ?
                                              baseItemInfo.MappingManager.SiteMappingManager.SolutionStatus[solutionId] : 0;
            }
            if (baseItemInfo.IsCurrentVersion && this.ParentList.BaseTemplate == AveListTemplateType.Links && itemVersion.Fields.ContainsField("Order"))
            {
                try
                {
                    var orderValue = this["Order"];
                    if (orderValue != null)
                    {
                        userData["Order"] = orderValue;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Failed to get the Order column value for the item, exception: {0}", e);
                }
            }
            if (ParentList != null && ParentList.IsRelationshipsList())
            {
                AssemblyVariationLabelName(userData);
            }
            foreach (string fieldName in taxonomyTextFieldValueCache.Keys)
            {
                if (userData.ContainsKey(fieldName))
                {
                    mLogger.Debug("Update field {0} value from {1} to {2}", fieldName, userData[fieldName], taxonomyTextFieldValueCache[fieldName]);
                    userData[fieldName] = taxonomyTextFieldValueCache[fieldName];
                }
            }
            return userData;
        }
        /// <summary>
        /// 在备份Relationships List Item的时候通过Label Unique Id反找到Label Name
        /// </summary>
        /// <param name="userData"></param>
        private void AssemblyVariationLabelName(Dictionary<string, object> userData)
        {
            if (userData.ContainsKey("Label"))
            {
                var labelId = (Guid)userData["Label"];
                if (labelId != Guid.Empty)
                {
                    var labelName = (Web.Site as AveSite).GetVariationLabelName(labelId);
                    userData["Label"] = string.Format("{0};{1}", labelId.ToString(), labelName);
                }
            }
        }
        private void AddUserData(string fieldName, Dictionary<string, object> userData, object value)
        {
            lock (ChangeMapping)
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
            try
            {
                Dictionary<string, object> tempJunctionData;
                string[] multiValues = value.ToString().Split(new string[] { ";#" }, StringSplitOptions.None);
                for (int i = 0; i < multiValues.Length; i = i + 2)
                {
                    int outValue;
                    bool result = int.TryParse(multiValues[i], out outValue);
                    if (result)
                    {
                        tempJunctionData = new Dictionary<string, object>();
                        tempJunctionData["tp_FieldId"] = fieldId;
                        tempJunctionData["tp_Id"] = outValue;
                        tempJunctionData["tp_UIVersion"] = itemVersion;
                        tempJunctionData["DisplayValue"] = multiValues[i + 1];
                        userDataJunction.Add(tempJunctionData);
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Can not analyze the data junction.FieldId:{0},Value:{1},ErrorMessage:{2}", fieldId.ToString(), value.ToString(), e.ToString());
            }
            //foreach (string tempValue in multiValues)
            //{
            //    int outValue;
            //    bool result = int.TryParse(tempValue, out outValue);
            //    if (result)
            //    {
            //        tempJunctionData = new Dictionary<string, object>();
            //        tempJunctionData["tp_FieldId"] = fieldId;
            //        tempJunctionData["tp_Id"] = outValue;
            //        tempJunctionData["tp_UIVersion"] = itemVersion;
            //        userDataJunction.Add(tempJunctionData);
            //    }
            //}
        }

        private Dictionary<string, object> ConvertCurrentVersionProperties(Dictionary<string, object> itemCurrentVersionFieldValues, Dictionary<string, string> needLoadFields)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
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
            if (itemCurrentVersionFieldValues.ContainsKey("AverageRating"))
            {
                itemVersionFieldValues["AverageRating"] = itemCurrentVersionFieldValues["AverageRating"];
            }
            if (itemCurrentVersionFieldValues.ContainsKey("_Level"))
            {
                listItemVersionProperties["Level"] = byte.Parse(itemCurrentVersionFieldValues["_Level"].ToString());
            }
            if (itemCurrentVersionFieldValues.ContainsKey("#ThreadIndexParentId"))
            {
                itemVersionFieldValues["#ThreadIndexParentId"] = itemCurrentVersionFieldValues["#ThreadIndexParentId"];
            }
            if (itemCurrentVersionFieldValues.ContainsKey("InstanceID") && itemCurrentVersionFieldValues["InstanceID"] != null)
            {
                int result = -1;
                if (Int32.TryParse(itemCurrentVersionFieldValues["InstanceID"].ToString(), out result))
                {
                    if (result != -1)
                    {
                        listItemVersionProperties["#tp_InstanceID"] = result;
                        itemVersionFieldValues["InstanceID"] = result;
                    }
                }
            }
            return listItemVersionsProperties;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "SendTo", Justification = "SharePoint Property")]
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
                case "WorkflowStatus":
                case "OutcomeChoice"://2013 new field, taskoutcome column
                case "GridChoice"://Rating Scale
                case "ThreadIndex":
                case "Link":
                    return svalue.ToString();
                case "Calculated":
                case "CallTo":
                case "SendTo":
                case "Lookup":
                case "LookupMulti":
                case "User":
                case "UserMulti":
                case "SummaryLinks":
                case "TaxonomyFieldType":
                case "TaxonomyFieldTypeMulti":
                case "MediaFieldType":
                    return svalue;
                case "DateTime":
                case "PublishingScheduleStartDateFieldType":
                case "PublishingScheduleEndDateFieldType":
                    return (DateTime)svalue;
                case "AverageRating":
                case "Currency":
                case "Number":
                case "RatingCount":
                case "Likes":
                    return double.Parse(svalue.ToString());
                case "Guid":
                    return new Guid(svalue.ToString());
                case "AllDayEvent":
                case "Boolean":
                case "Recurrence":
                case "CrossProjectLink"://calendar中event关联workspace 对应的column为该类型
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
                    break;
                case "Computed":
                    break;
                default:
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
                        if (!folderServerRelativeUrl.StartsWith(mParentWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
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


        public void DeletePost(bool needReloadFeedManager = false)
        {
            throw new NotImplementedException();
        }

        public IAveOSocialFeedManager GetMicroFeedManager()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<int>> GetMicroFeedReplyID()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<string>> GetMicroFeedLiker()
        {
            throw new NotImplementedException();
        }

        public void GetMicroFeedMentionAndTag(ref Dictionary<int, List<string>> microFeedMentionCache, ref Dictionary<int, List<string>> microFeedMentionDisPlayCache, ref Dictionary<int, List<string>> microFeedTagCache)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, string> GetMicroFeedPostID()
        {
            throw new NotImplementedException();
        }


        public string IconOverlay { get; set; }

        public bool MissingRequiredFields
        {
            get { return false; }
        }

        public AveFileSystemObjectType SortType { get; set; }


        public bool HasPublishedVersion
        {
            get { throw new NotImplementedException(); }
        }
        public IAveLinkCollection BackwardLinks
        {
            get { throw new NotImplementedException(); }
        }

        public IAveLinkCollection ForwardLinks
        {
            get { throw new NotImplementedException(); }
        }

        public IAveListItemComplianceInfo ComplianceTagInfo
        {
            get
            {
                if (this.ParentList.ParentWeb.Site.IsOnlineSite)
                {
                    try
                    {
                        if (!base.DataCache.IsPropertyAvailable("ComplianceTagInfo" + AveObjectModelConstant.ObjectPropertySuffix))
                        {
                            var complianceInfo = (mRequest ).GetListItemComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID);
                            if (complianceInfo.Count > 0)
                            {
                                DataCache.AddPropertyies(complianceInfo);
                            }
                        }
                        if (base.DataCache.IsPropertyAvailable("ComplianceTagInfo" + AveObjectModelConstant.ObjectPropertySuffix))
                        {
                            var properties = (Dictionary<string, object>)base.DataCache.PropertiesCache["ComplianceTagInfo" + AveObjectModelConstant.ObjectPropertySuffix];
                            return new AveListItemComplianceInfo(properties["ComplianceTag"].ToString(),
                                (bool)properties["TagPolicyHold"],
                                (bool)properties["TagPolicyRecord"],
                                (bool)properties["TagPolicyEventBased"],
                                (DateTime)properties["ComplianceWrittenDate"],
                                (int)properties["ComplianceFlags"],
                                (int)properties["ComplianceTagUserId"]);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to get item compliance info. Web id: {0}, list id: {1}, row id:{2}. Exception: {3}", this.Web.ID, this.ParentList.ID, this.ID, e);
                    }
                }
                return null;
            }
        }

        public void ReplaceLink(string oldUrl, string newUrl)
        {
            throw new NotImplementedException();
        }

        public void SetComplianceTag(AveItemComplianceTagInfo info)
        {
            try
            {
                if (mRequest is IAveRequest)
                {
                    var newProperties = (mRequest ).SetComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID, info);
                    DataCache.AddPropertyies(newProperties);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set the compliance tag for item. WebID: {0}, listID: {1}, rowID:{2}, exception: {3}", ParentList.ParentWeb.ID, ParentList.ID, ID, ex));
            }
        }

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            try
            {
                if (mRequest is IAveRequest)
                {
                    mRequest.SetComplianceTag(this.ParentList.ParentWeb.ID, this.ParentList.ID, this.ID, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set the compliance tag for item. WebID: {0}, listID: {1}, rowID:{2}, exception: {3}", ParentList.ParentWeb.ID, ParentList.ID, ID, ex));
            }
            
        }
    }
}