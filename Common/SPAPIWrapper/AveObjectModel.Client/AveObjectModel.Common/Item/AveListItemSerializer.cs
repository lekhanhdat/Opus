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
using AvePoint.Wrapper.Resource;
using Microsoft.Azure.Amqp.Framing;
using Microsoft365.SharePoint.Cache.Restore;

namespace AvePoint.ObjectModel.Common
{
    class AveListItemSerializer : IAveListItemSerializer
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveListItemSerializer));
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private Dictionary<Guid, FieldBackInfo> mNeedBackupField;
        private bool mFieldChanged = false;
        private IList<AveEventReceiver> mEventReceivers;

        private Dictionary<Guid, string> mValidationFields;
        private Dictionary<Guid, string> mFieldsDefaultValue;
        private Dictionary<Guid, AveRelationshipDeleteBehavior> mRelatedFieldBehavior = new Dictionary<Guid, AveRelationshipDeleteBehavior>();
        private Dictionary<Guid, AveRelationshipDeleteBehavior> mDestFieldBehavior = new Dictionary<Guid, AveRelationshipDeleteBehavior>();
        private bool mChangedFlag = false;

        public AveListItemSerializer(AveSite site, AveWeb web, AveList list)
        {
            this.mSite = site;
            this.mWeb = web;
            this.mList = list;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public AveRestoreResult SetObjectData(AveListItemInfo info)
        {
            if (mList.mOverWrite == null)
            {
                lock (mList.mItemRestoreLock)
                {
                    if (mList.mOverWrite == null)
                    {
                        mList.mOverWrite = info.SettingInfo.DELETE_ITEM;
                    }
                }
            }
            if (mList.mOverWrite.Value && mList.HasAttachment)
            {
                lock (mList.mItemRestoreLock)
                {
                    return RestoreListItem(info);
                }
            }
            else
            {
                return RestoreListItem(info);
            }
        }

        private AveRestoreResult RestoreListItem(AveListItemInfo info)
        {
            AveRestoreResult result = AveRestoreResult.Normal;
            mList.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping);
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, this.mList);

            #region parent web properties
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            webProperties.Add("ParentWebTemplate", mWeb.WebTemplate);
            webProperties.Add("ServerRelativeUrl", mWeb.ServerRelativeUrl);
            docData["ParentWebProperties"] = webProperties;
            #endregion

            #region parent list properties
            Dictionary<string, object> listProperties = new Dictionary<string, object>();
            listProperties.Add("ListId", mList.ID);
            listProperties.Add("ListTitle", mList.Title);
            listProperties.Add("BaseType", (int)mList.BaseType);
            listProperties.Add("ListTemplate", (int)mList.BaseTemplate);
            listProperties.Add("ListEnableModeration", mList.EnableModeration);
            listProperties.Add("ListEnableVersioning", mList.EnableVersioning);
            listProperties.Add("ListEnableMinorVersions", mList.EnableMinorVersions);
            listProperties.Add("ListBaseType", (int)mList.BaseType);
            listProperties.Add("ListRootFolderUrl", mList.RootFolder.ServerRelativeUrl);
            List<string> contentTypeIds = new List<string>();
            foreach (IAveContentType ct in mList.ContentTypes)
            {
                contentTypeIds.Add(ct.ID.ToString());
            }
            listProperties.Add("ParentListContentTypeIds", contentTypeIds);
            docData["ParentListProperties"] = listProperties;
            string key = (docData.ContainsKey("GUID") ? new Guid(docData["GUID"].ToString()) : Guid.Empty) + docData["FolderUrl"].ToString().Substring(mList.RootFolder.ServerRelativeUrl.Length);
            docData["DoclibRowId"] = mList.ListItemGuidAndRowIdMappings.ContainsKey(key) ? mList.ListItemGuidAndRowIdMappings[key] : -1;

            //docData["IsDeviceChannelsList"] = IsDeviceChannelsList();

            #endregion

            Dictionary<string, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields, info.FieldsInfo.MultilookupFields, (int)mList.BaseTemplate);
            int mappedOriginalRowId = (int)docData["DoclibRowId"]; // use mapped rowid if exist original item for append mode
            if (((AveRestoreMode)info.RestoreOption & AveRestoreMode.Append) == AveRestoreMode.Append && mappedOriginalRowId != -1)
            {
                string title = fields["Title"].ToString();
                var desRowId = int.TryParse(docData["DestRowId"].ToString(), out var value) ? value : -1;
                fields["Title"] = GetAvailableTitle(title,  mappedOriginalRowId, info.tp_Guid.ToString(), desRowId);
                mLogger.Info("Append item {0} to {1}", title, fields["Title"]);
            }
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
                docData["IsCommunityDiscussionList"] = IsCommunityDiscussionList();
            }
            if (info.DocData.ContainsKey("ComplianceTag"))
            {
                docData["ComplianceTag"] = info.DocData["ComplianceTag"];
            }
            if (mList.BaseTemplate == AveListTemplateType.Meetings)
            {
                mList.AssemblyMeetingItemInfo(info, info.UserData, docData);
            }
            if (mList.NeedSetNullFields == null)
            {
                mList.NeedSetNullFields = mList.SetNeedSetNullFields(info.KeepDefaultValue, fields);
            }
            fields.Add("NeedSetNullFields", mList.NeedSetNullFields);

            if (info.FieldsInfo.Fields.ContainsKey("TaxonomyFields"))
            {
                fields["TaxonomyFields"] = info.FieldsInfo.Fields["TaxonomyFields"];
            }
            Dictionary<string, object> restoreResult = mList.Request.RestoreListItem(docData, fields, info.FieldsInfo.UniqueValueFields);
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;
            bool mOverWrite = docData.ContainsKey("DeleteItem") ? Convert.ToBoolean(docData["DeleteItem"]) : false;
            if(mOverWrite && !info.IsNewCreated)
            {
                ItemRestoreCache.AddOverWriteFailItem(mList.ID.ToString(), /*info.OriginalRowId.ToString()*/mappedOriginalRowId.ToString());
            }
            if (restoreResult.ContainsKey("RestoreStatus") && !Convert.ToBoolean(restoreResult["RestoreStatus"]))
            {
                mLogger.Error("restore list item {0} failed, due to:{1}", info.ServerRelativeUrl, restoreResult["Exception"]);
                throw new Exception(restoreResult["ExceptionMessage"].ToString());
                //throw new AveRestoreException(AveRestoreResult.Failed, restoreResult["Exception"] as string);
            }
            if (restoreResult.ContainsKey("SkippedByLastModifiedTime") && Convert.ToBoolean(restoreResult["SkippedByLastModifiedTime"]))
            {
                mLogger.Warn("skip restore the listitem due to the conflict resolution skipby last modify time.");
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedItemByLastModifiedTime;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByLastModifiedTime.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByDeclaredDocument") && Convert.ToBoolean(restoreResult["SkippedByDeclaredDocument"]))
            {
                mLogger.Warn("skip restore the listitem due to it is DeclaredDocument.");
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedByDeclaredDocument;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedByDeclaredDocument.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByHasUniqueValue") && Convert.ToBoolean(restoreResult["SkippedByHasUniqueValue"]))
            {
                mLogger.Warn("skip restore the listitem due to the conflict unique value.");
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedItemByHasUniqueValue;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByHasUniqueValue.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("IsSkipped") && Convert.ToBoolean(restoreResult["IsSkipped"]))
            {
                result = AveRestoreResult.SkipTheSameItem;
                mLogger.Warn("skip restore the listItem due to it has no change.");
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = "RM_RS_SkippedItemByIsSameItemWithSkipConflictResolution";
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByIsSameItem.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(result, result.ToString());
            }

            AveListItem item = new AveListItem(mList.Request, mList.ParentWeb, mList, restoreResult["Item"] as Dictionary<string, object>, false);
            info.AveItem.ListItem = item;
            info.RowId = item.ID;
            info.RestoringItem.IsNewItem = info.IsNewCreated;
            if ((int)mList.BaseTemplate == 108 && info.IsInCommunityDiscussion && restoreResult.ContainsKey("ItemExist") && !Convert.ToBoolean(restoreResult["ItemExist"]))
            {
                this.mWeb.DiscussionReplyCache[item.ID] = info;
            }
            if (!info.RestoringItem.IsNewItem)
            {
                info.RestoringItem.ConfictType = ConfictType.Document;
            }
            if (restoreResult.ContainsKey("OldUniqueId"))
            {
                info.OldUniqueId = (Guid)restoreResult["OldUniqueId"];
            }
            else
            {
                info.OldUniqueId = item.UniqueId;
            }
            //return AveRestoreResult.Normal;
            return result;
        }

        private bool IsCommunityDiscussionList()
        {
            try
            {
                if (mList != null && mList.EventReceivers != null)
                {
                    foreach (IAveEventReceiverDefinition erDefinition in mList.EventReceivers)
                    {
                        if ("Microsoft.SharePoint.Portal.CommunityEventReceiver".Equals(erDefinition.Class))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("failed to get list eventreceiver due to: {0}", e.ToString());
            }
            return false;
        }

        private void RevertFieldLinks()
        {
            if (mSite.APIType == AveAPIType.BPOS_S && mList != null && mFieldChanged)
            {
                try
                {
                    foreach (KeyValuePair<Guid, FieldBackInfo> kv in mNeedBackupField)
                    {
                        AveField field = mList.Fields[kv.Key] as AveField;
                        field.ReadOnlyField = kv.Value.ReadOnly;
                        field.Hidden = kv.Value.Hidden;
                        field.Update();
                        mChangedFlag = true;
                    }
                    mFieldChanged = false;
                }
                catch (Exception e)
                {
                    mLogger.Warn("failed to revert the readonly attribute of modified field, list title: {0}, error message:{1}, stack trace:{2}", mList.Title, e.Message, e.StackTrace);
                }
            }
        }

        private void RevertEventReceivers()
        {
            try
            {
                if (mEventReceivers != null)
                {
                    (mSite.Request as IAveRequest).AddEventReceivers(mWeb.ServerRelativeUrl, mList.ID, mEventReceivers);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("failed to revert eventreceiver due to: {0}", e.ToString());
            }
        }

        private void BackupValidationFields()
        {
            if (mValidationFields != null || mList == null)
            {
                return;
            }
            mValidationFields = new Dictionary<Guid, string>();
            for (int i = mList.Fields.Count - 1; i >= 0; i--)
            {
                IAveField field = mList.Fields[i];
                if (!string.IsNullOrEmpty(field.ValidationFormula))
                {
                    mValidationFields.Add(field.ID, field.ValidationFormula);
                    field.ValidationFormula = string.Empty;
                    field.Update();
                    mChangedFlag = true;
                }
            }
        }

        private void RestoreValidationFields()
        {
            if (mValidationFields == null || mValidationFields.Count == 0)
            {
                return;
            }
            foreach (Guid fieldId in mValidationFields.Keys)
            {
                IAveField field = this.FindFieldInCollection(fieldId, mList.Fields);
                if (field != null)
                {
                    field.ValidationFormula = mValidationFields[fieldId];
                    field.Update();
                    mChangedFlag = true;
                }
            }
        }

        //如果list上的field设置了enforce unique value，并且设置了default value时，在创建item时会抛出SPDuplicateValuesFoundException异常。
        private void BackupFieldsDefaultValue()
        {
            if (mFieldsDefaultValue != null || mList == null)
            {
                return;
            }
            mFieldsDefaultValue = new Dictionary<Guid, string>();
            foreach (IAveField field in mList.Fields)
            {
                //if (field.TypeAsString.Equals("TaxonomyFieldType") || field.TypeAsString.Equals("TaxonomyFieldTypeMulti"))
                //{
                if (field.EnforceUniqueValues && !string.IsNullOrEmpty(field.DefaultValue))
                {
                    mFieldsDefaultValue.Add(field.ID, field.DefaultValue);
                }
                //}
            }
            foreach (Guid fieldId in mFieldsDefaultValue.Keys)
            {
                IAveField field = mList.Fields[fieldId];
                field.DefaultValue = string.Empty;
                field.Update();
                mChangedFlag = true;
            }
        }

        private void RestoreFieldDefaultValue()
        {
            if (mFieldsDefaultValue == null || mFieldsDefaultValue.Count == 0)
            {
                return;
            }
            foreach (Guid fieldId in mFieldsDefaultValue.Keys)
            {
                IAveField field = this.FindFieldInCollection(fieldId, mList.Fields);
                if (field != null)
                {
                    field.DefaultValue = mFieldsDefaultValue[fieldId];
                    field.Update();
                    mChangedFlag = true;
                }
            }
        }

        internal IAveField FindFieldInCollection(Guid fieldId, IAveFieldCollection collection)
        {
            IAveField field = null;
            try
            {
                if (!string.Equals(Guid.Empty, fieldId))
                {
                    field = collection[fieldId];
                }
            }/*
            catch (ArgumentException)
            { }*/
            catch (Exception e)
            {
                mLogger.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldInCollectionError, e.ToString());
            }
            return field;
        }

        private void SetRelatedFields()
        {
            try
            {
                //[DOC-70534] 需要将list的RelatedField的DeleteBehavior置为None 
                IAveRelatedFieldCollection fields = mList.GetRelatedFields();
                if (fields != null)
                {
                    foreach (IAveRelatedField relatedField in fields)
                    {
                        if (relatedField.RelationshipDeleteBehavior != AveRelationshipDeleteBehavior.None)
                        {
                            try
                            {
                                IAveList tempList = mWeb.Lists[relatedField.ListId];
                                IAveFieldLookup lookupField = tempList.Fields[relatedField.FieldId] as IAveFieldLookup;
                                if (lookupField != null)
                                {
                                    mRelatedFieldBehavior[relatedField.FieldId] = relatedField.RelationshipDeleteBehavior;
                                    lookupField.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
                                    lookupField.Update();
                                    mChangedFlag = true;
                                }
                            }
                            catch (AveSecurityTrimingException e)
                            {
                                //Contribute 权限
                                mLogger.Log(AveLogLevel.WARN, "Error occurred when set lookup field to none ." + e.ToString());
                            }
                            catch (Exception e)
                            {
                                mLogger.Log(AveLogLevel.WARN, "Error occurred when set lookup field to none ." + e.ToString());
                            }
                        }
                    }
                    foreach (IAveField field in mList.Fields)
                    {
                        try
                        {
                            if (field is IAveFieldLookup)
                            {
                                IAveFieldLookup lookupField = field as IAveFieldLookup;
                                if (lookupField.RelationshipDeleteBehavior != AveRelationshipDeleteBehavior.None)
                                {
                                    mDestFieldBehavior[field.ID] = lookupField.RelationshipDeleteBehavior;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Log(AveLogLevel.WARN, "Error occurred when set destination lookup field to none ." + e.ToString());
                        }
                    }
                    foreach (Guid fieldId in mDestFieldBehavior.Keys)
                    {
                        if (mList.Fields.Contains(fieldId))
                        {
                            IAveFieldLookup lookupField = mList.Fields[fieldId] as IAveFieldLookup;
                            lookupField.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
                            lookupField.Update();
                            mChangedFlag = true;
                        }
                    }
                }
            }
            /*catch (AveSecurityTrimingException)
            {
                //Contribute 权限
            }*/
            catch (Exception e)
            {
                mLogger.Log(AveLogLevel.DEBUG, WrapperRestoreResource.BackupListSettingFailed, e);
            }
        }

        private void RevertRelatedFields()
        {
            try
            {
                if (mList != null)
                {
                    foreach (IAveRelatedField field in mList.GetRelatedFields())
                    {
                        if (mRelatedFieldBehavior.ContainsKey(field.FieldId))
                        {
                            try
                            {
                                IAveList tempList = mWeb.Lists[field.ListId];
                                IAveFieldLookup lookupField = tempList.Fields[field.FieldId] as IAveFieldLookup;
                                if (lookupField != null)
                                {
                                    lookupField.RelationshipDeleteBehavior = mRelatedFieldBehavior[field.FieldId];
                                    lookupField.Update();
                                    mChangedFlag = true;
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                mLogger.Log(AveLogLevel.WARN, "Error occurred when restore list's related field behavior " + e.ToString());
                            }
                        }
                    }
                    foreach (Guid fieldId in mDestFieldBehavior.Keys)
                    {
                        try
                        {
                            if (mList.Fields.Contains(fieldId))
                            {
                                IAveFieldLookup lookupField = mList.Fields[fieldId] as IAveFieldLookup;
                                lookupField.RelationshipDeleteBehavior = mDestFieldBehavior[fieldId];
                                lookupField.Update();
                                mChangedFlag = true;
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            mLogger.Log(AveLogLevel.WARN, "Error occurred when set destination lookup field to none ." + e.ToString());
                        }
                    }
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLogger.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreListSettingFailed, e);
            }
        }

        public bool BeforeSetObjectData()
        {
            try
            {
                BackupValidationFields();
                BackupFieldsDefaultValue();
                SetRelatedFields();

                //DisableEventReceivers();
                //BackupFieldLinks(); Byron: comment out as this is for SP2010
                //BackupRequiredFields();
            }
            catch (Exception e)
            {
                mLogger.Warn("Exception in BeforeSetObjectData, Exception:{0}", e.ToString());
            }
            return mChangedFlag;
        }

        public bool AfterSetObjectData()
        {
            try
            {
                mChangedFlag = false;
                RestoreValidationFields();
                RestoreFieldDefaultValue();
                RevertRelatedFields();

                RevertEventReceivers();
                RevertFieldLinks();
                //RevertRequiredFields();
            }
            catch (Exception e)
            {
                mLogger.Warn("Exception in afterSetObjectData, Exception:{0}", e.ToString());
            }
            return mChangedFlag;
        }

        private string GetAvailableTitle(string sourceTitle, int itemId, string tpGuid, int destRowId)
        {
            //如果后续需要添加Item append _1的逻辑，可以修改此方法
            //int conflictId;
            //if (destRowId != - 1) 
            //{
            //    conflictId = destRowId;
            //}
            //else if (!mList.ListAppendItemMappings.TryGetValue(tpGuid, out conflictId))
            //{
            //    conflictId = itemId;
            //}

            //var conflictItem = mList.GetItemById(conflictId);
            //if (conflictItem == null)
            //{
            //    return sourceTitle;
            //}

            //if (destRowId != -1) // reuse title for versions restore
            //{
            //    return conflictItem["Title"].ToString();
            //}

            //string conflictTitle = conflictItem["Title"].ToString();
            //int index = conflictTitle.LastIndexOf("_");
            //if (index > 0)
            //{
            //    string pre = conflictTitle.Substring(0, index);
            //    int tail;
            //    if(string.Equals(pre, sourceTitle) && 
            //        Int32.TryParse(conflictTitle.Substring(index + 1), out tail))
            //    {
            //        sourceTitle = string.Format("{0}_{1}", sourceTitle, tail + 1);
            //    }
            //    else
            //    {
            //        sourceTitle += "_1"; 
            //    }
            //}
            //else
            //{
            //    sourceTitle += "_1";
            //}

            //mList.SetAppendItemTitleUsed(tpGuid);

            return sourceTitle;
        }
    }
    struct FieldBackInfo
    {
        public bool ReadOnly;
        public bool Hidden;
    }

    enum FieldLinkStatus
    {
        Required,
        Optional,
        Hidden,
    }
}
