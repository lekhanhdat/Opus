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
using AvePoint.Wrapper.Restore;

namespace AvePoint.ObjectModel.Common
{
    class AveListItemSerializer : IAveListItemSerializer, IDisposable
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveListItemSerializer));
        private IReport mReport;
        public IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private IList<string> mRequiredFields;
        private Dictionary<Guid, FieldBackInfo> mNeedBackupField;
        private bool mFieldChanged = false;

        public AveListItemSerializer(AveSite site, AveWeb web, AveList list)
        {
            this.mSite = site;
            this.mWeb = web;
            this.mList = list;
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public AveRestoreResult SetObjectData(AveListItemInfo info)
        {
            //if ((int)mList.BaseTemplate == AveCommunitiesConstants.MembersList_TemplateType) //community site member list下item不还原
            //{
            //    info.RestoringItem.NeedSkipped = true;
            //    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            //}

            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, this.mList);
            docData["ListTemplate"] = (int)mList.BaseTemplate;
            docData["ListEnableModeration"] = mList.EnableModeration;
            docData["ListEnableVersioning"] = mList.EnableVersioning;
            docData["AveWebObject"] = this.mWeb;
            bool IsSurveyListItemGUIDExist = false;
            Guid tempGUID = new Guid();
            if (mList.BaseType == AveBaseType.Survey)
            {
                if (!docData["GUID"].Equals(Guid.Empty))
                {
                    tempGUID = (Guid)docData["GUID"];
                    IsSurveyListItemGUIDExist = true;
                }
                docData["GUID"] = info.DocData["Id"];
            }

            EnsureDestRowIdInDocData(info, docData);
            if (mList.BaseType == AveBaseType.Survey && IsSurveyListItemGUIDExist && (int)docData["DestRowId"] == -1)
            {
                //为replicator two way添加：如果surveylist的GUID不为空且取不到DestRowId，再用GUID遍历目的端的UniqueId，把目的端ID赋给DestRowId
                for (int i = 0; i < mList.Items.Count; i++)
                {
                    if (mList.Items[i].UniqueId == tempGUID)
                    {
                        docData["DestRowId"] = mList.Items[i].ID;
                    }
                }
            }
            if (mList != null && mList.IsVariationLabelsList())
            {
                var labelName = info.UserData.ContainsKey("Title") ? info.UserData["Title"].ToString() : null;
                var isSource = info.UserData.ContainsKey("Is_x0020_Source") ? (bool)info.UserData["Is_x0020_Source"] : false;
                (info.AveItem as AveItem).CheckConflictStateForVariationLabels(info.RestoringItem, labelName, isSource);
            }
            if (mList != null && mList.IsRelationshipsList())
            {
                //此处使用替换过的column url
                var objectID = info.FieldsInfo.Fields.ContainsKey("ObjectID") ? ((AveFieldValueInfo)info.FieldsInfo.Fields["ObjectID"]).ColValue.ToString() : null;
                (info.AveItem as AveItem).CheckConflictStateForRelationshipsList(info.RestoringItem, objectID);
            }
            if (mList.IsVariationLabelsList() || mList.IsRelationshipsList())
            {
                if (info.RestoringItem.ConflictRowId > 0)
                    docData["DestRowId"] = info.RestoringItem.ConflictRowId;
            }

            info.FieldsInfo.Fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields);

            if (mList.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                if (info.DocData.ContainsKey("DiscussionTopic"))
                {
                    docData["DiscussionTopic"] = info.DocData["DiscussionTopic"];
                }
                if (info.DocData.ContainsKey("ParentThreadId"))
                {
                    docData["ParentThreadId"] = info.DocData["ParentThreadId"];
                }
            }

            if (mList.BaseTemplate == AveListTemplateType.Meetings)
            {
                mList.AssemblyMeetingItemInfo(info, info.UserData, docData);
            }
            if (mWeb.Site.APIType != AveAPIType.BPOS_S || string.Compare(mWeb.Site.SPVersion, "15.", StringComparison.OrdinalIgnoreCase) > 0)
            {
                mList.SetTaxonomyField(info, -1, info.IsForceAddTerm, info.FieldsInfo.TermIdMapping, info.FieldsInfo.MergedTermIdMapping);
            }
            info.FieldsInfo.Fields.Add("NeedSetNullFields", info.NeedSetNullFields);

            if (docData.ContainsKey("ListId") && docData["ListId"] != null)
            {
                var oldId = new Guid(docData["ListId"] as string);
                Guid value = Guid.Empty;
                if (info.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldId, out value))
                {
                    docData["ListId"] = value.ToString();
                }
                else
                {
                    docData["ListId"] = oldId.ToString();
                }
            }
            docData["IsExceedListViewLookupThreshold"] = mList.IsExceedListViewLookupThreshold;

            #region For Nintex Form
            if (info.FieldsInfo.Fields.ContainsKey("FormData")
                && !string.IsNullOrEmpty(info.FieldsInfo.Fields["FormData"] == null ? "" : info.FieldsInfo.Fields["FormData"].ToString())
                && mList.ParentWeb.Site.IsOnlineSite
                && mList.Fields.ContainsField("NFFormData"))
            {
                string formDataValue = info.FieldsInfo.Fields["FormData"].ToString();
                info.FieldsInfo.Fields.Remove("FormData");
                info.FieldsInfo.Fields.Add("NFFormData", formDataValue);
            }
            #endregion

            Dictionary<string, object> restoreResult = mList.Request.RestoreListItem(docData, info.FieldsInfo.Fields, AddItemMapping);
            // 在还原ListItem的Version的时候会开启List的EnableVersioning，需要给AveList上的EnableVersioning属性同步。
            if (restoreResult.ContainsKey("ListEnableVersioning"))
            {
                var enableVersioning = Convert.ToBoolean(restoreResult["ListEnableVersioning"]);
                if (mList.EnableVersioning != enableVersioning)
                {
                    mList.EnableVersioning = enableVersioning;
                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                }
            }
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;

            if (!(Boolean)restoreResult["RestoreStatus"])
            {
                mLogger.Error("Restore list item {0} failed, due to:{1}.", info.ServerRelativeUrl, restoreResult["Exception"]);
                //此处以后需要修改,修改异常类型会影响外围调用，66暂不修改，目前直接抛出system exception,此处error message无法国际化
                throw new Exception(restoreResult["ExceptionMessage"].ToString());
                //throw new AveRestoreException(AveRestoreResult.Failed, restoreResult["Exception"] as string);
            }
            if ((restoreResult.ContainsKey("SkippedByLastModifiedTime") && Convert.ToBoolean(restoreResult["SkippedByLastModifiedTime"]))
                || (info.RestoreOption == AveRestoreMode.Default && !info.IsNewCreated && info.OriginalVersion >= info.Version))//ADO-135831 skip之后没有给外围返回skip result状态。
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.ConflictType = ConflictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            AveListItem item = new AveListItem(mList.Request, mList.ParentWeb, mList, restoreResult["Item"] as Dictionary<string, object>, false);
            info.AveItem.ListItem = item;
            info.Version = item.UIVersion;
            info.RowId = item.ID;
            info.RestoringItem.IsNewItem = info.IsNewCreated;
            if (!info.RestoringItem.IsNewItem)
            {
                info.RestoringItem.ConflictType = ConflictType.Document;
            }
            else
            {
                info.RestoringItem.ReSetItemName(info.RowId + "_.000");
                AddItemMapping(info, docData);
            }
            return AveRestoreResult.Normal;
        }

        private void EnsureDestRowIdInDocData(AveBaseItemInfo info, IDictionary<string, object> docData)
        {
            if (mList == null || docData == null | info == null)
            {
                return;
            }

            var uniqueIdKey = (docData.ContainsKey(AveFieldNameCollection.Id_Field) ? new Guid(docData[AveFieldNameCollection.Id_Field].ToString()) : Guid.Empty) + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            var guidKey = (docData.ContainsKey(AveFieldNameCollection.Guid_Field) ? new Guid(docData[AveFieldNameCollection.Guid_Field].ToString()) : Guid.Empty) + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            var destRowIdFound = false;

            if (mList.ListItemGuidAndRowIdMappings != null && mList.ListItemGuidAndRowIdMappings.ContainsKey(guidKey))
            {
                docData["DestRowId"] = mList.ListItemGuidAndRowIdMappings[guidKey];
                destRowIdFound = true;
            }
            //for HSM IB. use unique id to find row id
            if (!destRowIdFound && info.SettingInfo.CheckConflictByUniqueId)
            {
                if (mList.ListItemUniqueIdAndRowIdMappings != null && mList.ListItemUniqueIdAndRowIdMappings.ContainsKey(uniqueIdKey))
                {
                    docData["DestRowId"] = mList.ListItemUniqueIdAndRowIdMappings[uniqueIdKey];
                }
            }
        }

        private void AddItemMapping(Guid uniqueId, Guid tpGuid, int rowId, IDictionary<string, object> docData)
        {
            var uniqueIdKey = uniqueId + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            var guidKey = tpGuid + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            if (!uniqueIdKey.StartsWith(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)
                && mList.ListItemUniqueIdAndRowIdMappings != null)
            {
                mList.ListItemUniqueIdAndRowIdMappings[uniqueIdKey] = rowId;
            }
            if (mList.ListItemGuidAndRowIdMappings != null)
            {
                mList.ListItemGuidAndRowIdMappings[guidKey] = rowId;
            }
        }

        private void AddItemMapping(AveBaseItemInfo info, IDictionary<string, object> docData)
        {
            var uniqueIdKey = (docData.ContainsKey(AveFieldNameCollection.Id_Field) ? new Guid(docData[AveFieldNameCollection.Id_Field].ToString()) : Guid.Empty) + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            var guidKey = (docData.ContainsKey(AveFieldNameCollection.Guid_Field) ? new Guid(docData[AveFieldNameCollection.Guid_Field].ToString()) : Guid.Empty) + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            if (info.SettingInfo.CheckConflictByUniqueId)
            {
                if (!uniqueIdKey.StartsWith(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)
                    && mList.ListItemUniqueIdAndRowIdMappings != null)
                {
                    mList.ListItemUniqueIdAndRowIdMappings[uniqueIdKey] = info.RowId;
                }
            }
            if (mList.ListItemGuidAndRowIdMappings != null)
            {
                mList.ListItemGuidAndRowIdMappings[guidKey] = info.RowId;
            }
        }

        /// <summary>
        /// when we set the readonly of modified field as true, the fieldlink will change automaticlly, so we need to revert it after all the items are restored in this list
        /// </summary>
        private void BackupFieldLinks()
        {
            if (mSite.Request.Kind != AveRequestKind.Extension && mList != null)
            {
                try
                {
                    mNeedBackupField = new Dictionary<Guid, FieldBackInfo>();
                    Guid[] needBackupFieldGuids = new Guid[] { AveBuiltInFieldId.Editor, AveBuiltInFieldId.Modified };
                    foreach (Guid fieldId in needBackupFieldGuids)
                    {
                        AveField field = mList.Fields[fieldId] as AveField;
                        if (field != null)
                        {
                            FieldBackInfo info = new FieldBackInfo();
                            info.ReadOnly = field.ReadOnlyField;
                            info.Hidden = field.Hidden;
                            mNeedBackupField[fieldId] = info;
                            if (fieldId.Equals(AveBuiltInFieldId.Modified) && field.ReadOnlyField)
                            {
                                field.ReadOnlyField = false;
                                field.Update();
                            }
                            mFieldChanged = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Failed to set the readonly attribute of modified field as false, list title: {0}, error message:{1}, stack trace:{2}.", mList.Title, e.Message, e.StackTrace);
                }
            }
        }

        private void RevertFieldLinks()
        {
            if (mSite.Request.Kind != AveRequestKind.Extension && mList != null && mFieldChanged)
            {
                try
                {
                    foreach (KeyValuePair<Guid, FieldBackInfo> kv in mNeedBackupField)
                    {
                        AveField field = mList.Fields[kv.Key] as AveField;
                        field.ReadOnlyField = kv.Value.ReadOnly;
                        field.Hidden = kv.Value.Hidden;
                        field.Update();
                    }
                    mFieldChanged = false;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Failed to revert the readonly attribute of modified field, list title: {0}, error message:{1}, stack trace:{2}.", mList.Title, e.Message, e.StackTrace);
                }
            }
        }

        private void BackupRequiredFields()
        {
            if (mSite.Request.Kind != AveRequestKind.Extension && mList != null)
            {
                mRequiredFields = new List<string>();
                foreach (IAveField field in mList.Fields)
                {
                    if (field.Required && field.ID != AveBuiltInFieldId.FileLeafRef && field.ID != AveBuiltInFieldId.Title && !field.Hidden)
                    {
                        field.Required = false;
                        field.Update();
                        mRequiredFields.Add(field.InternalName);
                    }
                }
            }
        }

        private void RevertRequiredFields()
        {
            if (mRequiredFields != null && mRequiredFields.Count > 0)
            {
                IAveFieldCollection fields = mList.Fields;
                try
                {
                    foreach (string fieldInternalName in mRequiredFields)
                    {
                        IAveField field = fields.GetFieldByInternalName(fieldInternalName);
                        field.Required = true;
                        field.Update();
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Can't revert the required properties, due to: {0}.", e.ToString());
                }
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Custom Field Property")]
        public void BeforeSetObjectData()
        {
            //BackupFieldLinks();
            //BackupRequiredFields();
            if (this.mList != null && mList.BaseTemplate == AveListTemplateType.Survey && !mList.Fields.Contains(new Guid("c80e4553-d104-45d4-929b-28f7aae3a1c7")))
            {
                string newSchemaXml = "<Field ID=\"{c80e4553-d104-45d4-929b-28f7aae3a1c7}\" ColName=\"tp_GUID\" RowOrdinal=\"0\" ReadOnly=\"TRUE\" Hidden=\"TRUE\" Type=\"Guid\" Name=\"GUID\" DisplayName=\"GUID\" SourceID=\"http://schemas.microsoft.com/sharepoint/v3\" StaticName=\"GUID\" FromBaseType=\"TRUE\" />";
                mList.Fields.AddFieldAsXml(newSchemaXml, false, AveAddFieldOptions.DefaultValue);
            }
        }

        public void AfterSetObjectData()
        {
            //RevertFieldLinks();
            //RevertRequiredFields();
        }

        public void Dispose()
        {

        }
    }
    struct FieldBackInfo
    {
        public bool ReadOnly;
        public bool Hidden;
    }
}
