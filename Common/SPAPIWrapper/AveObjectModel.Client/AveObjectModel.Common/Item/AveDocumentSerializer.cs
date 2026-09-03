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
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource;
using System.IO;
using System.Collections;
using AvePoint.GCommon.Contract.ContentManager.Object;
using Util.MIP;
using Microsoft365.SharePoint.Cache.Restore;
using System.Xml.Linq;

namespace AvePoint.ObjectModel.Common
{
    class AveDocumentSerializer : IAveDocumentSerializer
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveDocumentSerializer));

        private IAveRequest mRequest;
        private AveWeb mWeb;
        private AveList mList;
        private AveFolder mParentFolder;

        public AveDocumentSerializer(AveFolder parentFolder, AveList list, AveWeb web, IAveRequest request)
        {
            mRequest = request;
            mParentFolder = parentFolder;
            mList = list;
            mWeb = web;
        }

        public AveRestoreResult SetObjectData(AveDocumentInfo info, System.IO.Stream content, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<IAveListItem> holdItems, System.Collections.Hashtable HTMetaInfo)
        {
            return SetObjectDataWithRequest(info, content, allDocData, allUserData, holdItems, HTMetaInfo, null);
        }

        public AveRestoreResult SetObjectDataWithRequest(AveDocumentInfo info, System.IO.Stream content, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<IAveListItem> holdItems, System.Collections.Hashtable HTMetaInfo, IAveRequest aveRequest)
        {
            info.DocData = AveList.AssembleBaseItemInfo(info, this.mList);
            this.AssembleDocumentInfo(info.DocData, info, allDocData, allUserData);
            info.FieldsInfo.Fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields, info.FieldsInfo.MultilookupFields, mList != null ? (int)mList.BaseTemplate : -1);
            if (!info.FieldsInfo.Fields.ContainsKey("Modified"))
            {
                info.FieldsInfo.Fields.Add("Modified", info.DTimeLastModified);
            }
            if (!info.FieldsInfo.Fields.ContainsKey("Created"))
            {
                info.FieldsInfo.Fields.Add("Created", info.DTimeCreated);
            }
            //SAAS-37788 process the 'News link' title reference url.
            ProcessNewsWebpartReferenceFieldValues(info);
            ProcessLinkContentTypeReferenceFieldValues(info);
            if (mList != null) //File in the web system folder does not has parent list
            {
                mList.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping);
                if (mList.NeedSetNullFields == null)
                {
                    mList.NeedSetNullFields = mList.SetNeedSetNullFields(info.KeepDefaultValue, info.FieldsInfo.Fields, allUserData);
                }
                else
                {
                    mList.NeedSetNullFields = mList.NeedSetNullFields.Union(mList.SetNeedSetNullFields(info.KeepDefaultValue, info.FieldsInfo.Fields, allUserData)).ToList();
                }
                info.FieldsInfo.Fields.Add("NeedSetNullFields", mList.NeedSetNullFields);
                if (info.KeepDefaultValue && mList.BaseType == AveBaseType.DocumentLibrary)
                {
                    foreach (var field in allUserData["#DefaultValues"] as Dictionary<string, object>)
                    {
                        if (!info.FieldsInfo.Fields.ContainsKey(field.Key))
                        {
                            Dictionary<string, object> values = field.Value as Dictionary<string, object>;
                            string folderPath = System.Web.HttpUtility.UrlDecode(info.ParentFolderRelativeUrl);
                            string key = values.Keys.Where(k => folderPath.Contains(k)).OrderByDescending(k => k.Length).FirstOrDefault();
                            if (!string.IsNullOrEmpty(key))
                                info.FieldsInfo.Fields[field.Key] = values[key];
                        }
                    }
                }
            }
            if (info.IsView)
            {
                this.AssembleViewInfo(info);
            }
            if (mList != null && mList.BaseTemplate == AveListTemplateType.XMLForm && info.Name.EndsWith(".xml"))
            {
                content = FixBrokenLinks(info, content);
            }

            DocumentRestoreInfo parentInfo = SetParentInfo(info);
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            if (aveRequest == null)
            {
                restoreResult = mRequest.RestoreDocument(info, content, parentInfo);
            }
            else
            {
                restoreResult = aveRequest.RestoreDocument(info, content, parentInfo);
            }

            bool isNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;
            bool mOverWrite = info.DocData.ContainsKey("DeleteItem") ? Convert.ToBoolean(info.DocData["DeleteItem"]) : false;
            string mFileRelativeUrl = ((string)info.DocData["FolderUrl"]).TrimEnd('/') + "/" + ((string)info.DocData["Title"]);
            if (mOverWrite && !isNewCreated)
            {
                ItemRestoreCache.AddOverWriteFailItem(mList.ID.ToString(), mFileRelativeUrl);
            }
            if (restoreResult.ContainsKey("File"))
            {
                Dictionary<string, object> properties = restoreResult["File"] as Dictionary<string, object>;
                AveFile newFile = new AveFile(mRequest, mWeb, mList, mParentFolder, properties);
                info.AveItem.File = newFile;

                info.AveItem.ListItem = (newFile.Item != null && mList != null && newFile.Item.ID > 0) ? mList.GetItemById(newFile.Item.ID) : newFile.Item;
                //获得file的docid
                if (properties.ContainsKey("UniqueId"))
                {
                    info.GUID = (Guid)properties["UniqueId"];
                }
                //记录原端与目的端 Document UniqueId 的Mapping
                if (properties.ContainsKey("UniqueId") && info.OrignialID != Guid.Empty)
                {
                    info.MappingManager.SiteMappingManager.AddDocumentUniqueIdMapping(info.OrignialID, (Guid)properties["UniqueId"]);
                }
            }
            if (restoreResult.ContainsKey("SkippedByLastModifiedTime") && Convert.ToBoolean(restoreResult["SkippedByLastModifiedTime"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedItemByLastModifiedTime;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByLastModifiedTime.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByDeclaredDocument") && Convert.ToBoolean(restoreResult["SkippedByDeclaredDocument"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedByDeclaredDocument;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedByDeclaredDocument.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByHasUniqueValue") && Convert.ToBoolean(restoreResult["SkippedByHasUniqueValue"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedItemByHasUniqueValue;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByHasUniqueValue.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByIsPersonalView") && Convert.ToBoolean(restoreResult["SkippedByIsPersonalView"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedByIsPersonalView;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedByIsPersonalView.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedByCannotEditItem") && Convert.ToBoolean(restoreResult["SkippedByCannotEditItem"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedByCannotEditItem;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedByCannotEditItem.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("IsSkipped") && Convert.ToBoolean(restoreResult["IsSkipped"]))
            {
                mLogger.Warn("skip restore the document due to it has no change.");
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = "RM_RS_SkippedItemByIsSameItemWithSkipConflictResolution";
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByIsSameItem.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("SkippedItemByTargetGtSourceVersion") && Convert.ToBoolean(restoreResult["SkippedItemByTargetGtSourceVersion"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedItemByTargetGtSourceVersion;
                info.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByTargetGtSourceVersion.ToString();
                info.RestoringItem.ConfictType = ConfictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            if (restoreResult.ContainsKey("Exception"))
            {
                mLogger.Error("restore document {0} failed, due to:{1}", info.ServerRelativeUrl, restoreResult["Exception"]);
                //return AveRestoreResult.Failed;这个状态外围都已经不处理了，所以改成throw；
                throw new Exception(restoreResult["ExceptionMessage"].ToString());
            }

            if (parentInfo.ParentListInfo != null && parentInfo.ParentListInfo.isListVersionSettingChanged)
            {
                info.SettingInfo.LIST_SETTING_CHANGED = true;
                mList.EnableVersioning = parentInfo.ParentListInfo.EnableVersioning;
                mList.EnableMinorVersions = parentInfo.ParentListInfo.EnableMinorVersions;
            }
            if (restoreResult.ContainsKey("RowId"))
            {
                info.RowId = (int)restoreResult["RowId"];
            }
            if (restoreResult.ContainsKey("OldUniqueId"))
            {
                info.OldUniqueId = (Guid)restoreResult["OldUniqueId"];
            }
            else
            {
                info.OldUniqueId = info.GUID;
            }
            if (info.IsView)
            {
                Dictionary<Guid, Guid> viewIdMapping = restoreResult["ViewIdsMapping"] as Dictionary<Guid, Guid>;
                foreach (KeyValuePair<Guid, Guid> viewIdKV in viewIdMapping)
                {
                    lock (info.MappingManager.SiteMappingManager.ViewGuidMapping)
                    {
                        info.MappingManager.SiteMappingManager.ViewGuidMapping[viewIdKV.Key] = viewIdKV.Value;
                        mLogger.Info($"Add View Id mapping {viewIdKV.Key} -> {viewIdKV.Value}");
                    }
                    info.AveView.Views[viewIdKV.Value] = viewIdKV.Key;
                }
                info.IsNewCreatedView = isNewCreated;
                AveList list = mWeb.Lists.TryGetList(info.ParentListTitle) as AveList;
                if (list != null)
                {
                    mLogger.Info("[SAAS-30604]Find list by Title {0} while post restore view successfully.", list.Title);
                }
                if (list == null && info.ParentListId != Guid.Empty)
                {
                    list = mWeb.Lists.GetById(info.ParentListId) as AveList;
                    if (list != null)
                    {
                        mLogger.Info("[SAAS-30604]Find list by listId {0} while post restore view successfully.ListTitle:{1},ListTitleInViewCacheInfo:{2}", list.ID, list.Title, info.ParentListTitle);
                    }
                }
                if (list == null)
                {
                    throw new ArgumentException("Parent list not found while genrate list view info cache.ListTitle:{0},ListId:{1}");
                }
                if (restoreResult.ContainsKey("View"))
                {
                    AveViewCollection views = list.Views as AveViewCollection;
                    AveView view = new AveView(list, views, mRequest, restoreResult["View"] as Dictionary<string, object>);
                    views.ListData.Add(view);
                    info.AveItem.View = view;
                }
                else if (info.AveItem.File != null)
                {
                    foreach (AveView view in list.Views)
                    {
                        if (info.AveItem.File.ServerRelativeUrl.EndsWith(view.Url, StringComparison.OrdinalIgnoreCase))
                        {
                            info.AveItem.View = view;
                            break;
                        }
                    }
                }
            }
            else
            {
                info.IsNewCreatedDoc = isNewCreated;
            }
            info.RestoringItem.IsNewItem = isNewCreated;
            if (!info.RestoringItem.IsNewItem)
            {
                info.RestoringItem.ConfictType = ConfictType.Document;
            }
            return AveRestoreResult.Normal;
        }

        private void ProcessLinkContentTypeReferenceFieldValues(AveDocumentInfo info)
        {
            try
            {
                if (info == null || !WrapperConfiguration.WrapperConfigurationForBPOS.UseTargetReferenceOfLinkContentTypeItem)
                {
                    return;
                }
                if (info != null && info.FieldsInfo != null && info.FieldsInfo.Fields != null && info.FieldsInfo.Fields.Any())
                {
                    //field key: _OriginalSourceUrl
                    ProcessShortCutFieldValues(info);
                }
                else
                {
                    throw new AveWrapperInvalidDataException();
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occur when process link contentType reference field values, error:{0}", e);
            }
        }

        private void ProcessShortCutFieldValues(AveDocumentInfo info)
        {
            SetFieldValueToNull(info, "_ShortcutUrl");
            SetFieldValueToNull(info, "_ShortcutSiteId");
            SetFieldValueToNull(info, "_ShortcutWebId");
            SetFieldValueToNull(info, "_ShortcutUniqueId");
        }

        private void SetFieldValueToNull(AveDocumentInfo info, string fieldValueName)
        {
            var fields = info.FieldsInfo.Fields;
            if (fields.ContainsKey(fieldValueName))
            {
                var shortcutSourceObj = fields[fieldValueName];
                mLogger.Info(string.Format("Set field value to null, {0}:{1}", fieldValueName, shortcutSourceObj));
                if (shortcutSourceObj != null)
                {
                    fields[fieldValueName] = null;
                }
            }
        }

        /// <summary>
        /// News webpart下news link的title所引用的url，如果是当前document,需要替换当前documents的一些field values
        /// 例如：
        /// 当前目的端document url: https://m365x475714.sharepoint.com/sites/test35812/SitePages/AllItems.aspx
        /// 它的field value,引用的是源端的某个item的url: _OriginalSourceUrl:https://m365x475714.sharepoint.com/sites/test1/Lists/PublishedFeed/AllItems.aspx
        /// </summary>
        /// <param name="fields"></param>
        private void ProcessNewsWebpartReferenceFieldValues(AveDocumentInfo info)
        {
            try
            {
                //mLogger.Info("Start to process News Webpart Reference Field Values...");
                if (info != null && info.FieldsInfo != null && info.FieldsInfo.Fields != null && info.FieldsInfo.Fields.Any())
                {
                    //field key: _OriginalSourceUrl
                    ProcessOriginalSourceUrl(info);
                }
                else
                {
                    throw new AveWrapperInvalidDataException();
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occur when Process News Webpart Reference Field Values, error:{0}", e);
            }
            //finally
            //{
            //    mLogger.Info("End to process News Webpart Reference Field Values...");
            //}
        }

        private void ProcessOriginalSourceUrl(AveDocumentInfo info)
        {
            var fields = info.FieldsInfo.Fields;
            if (fields.ContainsKey("_OriginalSourceUrl"))
            {
                var originalSourceUrlObj = fields["_OriginalSourceUrl"];
                mLogger.Info(string.Format("Start to process OriginalSourceUrl:{0}", originalSourceUrlObj));
                if (originalSourceUrlObj != null && originalSourceUrlObj is string)
                {
                    //通过mapping找到目的端的url
                    string originalSourceUrl = originalSourceUrlObj as string;
                    var oldUrl = originalSourceUrl.StartsWith("//") ? originalSourceUrl.Substring(1) : originalSourceUrl;
                    string desServerRelativeUrl = AveReplaceProcessor.UrlReplace(oldUrl, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                    fields["_OriginalSourceUrl"] = desServerRelativeUrl;
                }
                mLogger.Info(string.Format("End to process OriginalSourceUrl:{0}", fields["_OriginalSourceUrl"]));
            }
        }

        private DocumentRestoreInfo SetParentInfo(AveDocumentInfo info, params object[] args)
        {
            DocumentRestoreInfo parentInfo = new DocumentRestoreInfo()
            {
                ParentWebInfo = new RestoreWebInfo(mWeb),
                ParentFolderInfo = new RestoreFolderInfo(mParentFolder)
            };

            if (mList != null)
            {
                parentInfo.ParentListInfo = new RestoreListInfo(mList);
                if (info.IsView)
                {
                    parentInfo.ParentListInfo.ListFieldInternalNames = mList.Fields.GetInternalNamesBySchema();
                }
            }

            return parentInfo;
        }

        private System.IO.Stream FixBrokenLinks(AveDocumentInfo info, System.IO.Stream content)
        {
            XmlDocument infoPathDocument = new XmlDocument();
            infoPathDocument.PreserveWhitespace = true;
            infoPathDocument.Load(content);
            XmlNode node = infoPathDocument.SelectSingleNode("/processing-instruction(\"mso-infoPathSolution\")");
            if (node != null)
            {
                XmlProcessingInstruction pi = (XmlProcessingInstruction)node; //here we should to keep case sensitivity of the string, because SharePoint does
                string oldValue = pi.Value;
                string[] keys = oldValue.Split(new char[] { '"' });
                foreach (string key in keys)
                {
                    if (key.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        string url = key;
                        //因为update field时，会同步更新Content中的内容，所以下面的内容有问题，需要判断下是否是更新过的目的端的Url
                        if (url.StartsWith(info.MappingManager.SiteMappingManager.DestSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
                        {
                            //nothing...
                        }
                        else if (!url.StartsWith(info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            string hostheader = AveReplaceProcessor.GetHostHeader(info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl);
                            string zoneUrl = AveReplaceProcessor.GetHostHeader(url);
                            url = url.Replace(zoneUrl, hostheader);
                        }
                        url = AveReplaceProcessor.UrlReplace(url, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        if (!string.IsNullOrEmpty(key) && !key.Equals(url, StringComparison.OrdinalIgnoreCase))
                        {
                            pi.Value = oldValue.Replace(key, url);
                        }
                    }
                }
            }
            System.IO.MemoryStream fixedContent = new System.IO.MemoryStream();
            infoPathDocument.Save(fixedContent);
            fixedContent.Position = 0;
            return fixedContent;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property.")]
        private void AssembleDocumentInfo(Dictionary<string, object> docData, AveDocumentInfo docInfo, Dictionary<string, object> alldocData, Dictionary<string, object> allUserData)
        {
            docData["IsView"] = docInfo.IsView;
            bool isOriginalCheckOut = false;
            if (alldocData.ContainsKey("IsCheckOut"))
            {
                isOriginalCheckOut = Convert.ToBoolean(alldocData["IsCheckOut"]);
            }
            else
            {
                isOriginalCheckOut = docInfo.IsCheckOut;
            }
            docData["IsOriginalCheckOut"] = docInfo.IsOrignialCheckOut || isOriginalCheckOut;
            docData["CheckoutUserId"] = docInfo.CheckoutUserId;
            docData["CheckInComment"] = docInfo.CheckinComment.Contains(";#") ? docInfo.CheckinComment.Substring(docInfo.CheckinComment.IndexOf(";#") + 2) : docInfo.CheckinComment;
            docData["SolutionId"] = docInfo.SolutionId;
            if (this.mWeb.AllProperties != null)
            {
                docData["ParentWebAllProperties"] = this.mWeb.AllProperties;
            }
            if (alldocData.ContainsKey("LeafName"))
            {
                docData["LeafName"] = alldocData["LeafName"];
            }
            if (allUserData.ContainsKey("#SolutionStatus"))
            {
                docData["SolutionStatus"] = allUserData["#SolutionStatus"];
            }
            if (allUserData.ContainsKey("#tp_GUID"))
            {
                docData["GUID"] = allUserData["#tp_GUID"];
            }
            if (alldocData.ContainsKey("SetupPath"))
            {
                docData["SetupPath"] = alldocData["SetupPath"];
            }
            if (alldocData.ContainsKey("IsFormPage"))
            {
                docData["IsFormPage"] = alldocData["IsFormPage"];
            }
            if (alldocData.ContainsKey("IsCurrentVersion"))
            {
                docData["IsCurrentVersion"] = alldocData["IsCurrentVersion"];
            }
            if (docInfo.IsView)
            {
                docData["IsViewPage"] = true;
            }
            AveItem aveItem = docInfo.AveItem as AveItem;
            if (docInfo.IsGhostPage)
            {
                docData["IsGhostedPage"] = true;
                docData["SetupPath"] = docInfo.SetupPath;
                docData["GhostPageOption"] = docInfo.GhostPageOption;
                if ((docInfo.OriginalRowId < 0 || aveItem.List == null || aveItem.List != null
                    && (aveItem.List.BaseTemplate == AveListTemplateType.ListTemplateCatalog
                    || aveItem.List.BaseTemplate == AveListTemplateType.WebTemplateCatalog
                    || aveItem.List.BaseTemplate == AveListTemplateType.SolutionCatalog
                    || aveItem.List.BaseTemplate == AveListTemplateType.ThemeCatalog
                    || aveItem.List.BaseTemplate == AveListTemplateType.WebPartCatalog
                    || aveItem.List.BaseTemplate == AveListTemplateType.MasterPageCatalog)))
                {

                    Dictionary<string, string> metaInfo = new Dictionary<string, string>();
                    if (docInfo.MetaInfoDic != null)
                    {
                        string[] needRestore = new string[] { "ipfs_listform", "ipfs_streamhash" };
                        foreach (string key in needRestore)
                        {
                            if (docInfo.MetaInfoDic.ContainsKey(key))
                            {
                                metaInfo[key] = docInfo.MetaInfoDic[key];
                            }
                        }
                    }
                    docData["Properties"] = metaInfo;
                }
            }
            docData["AveWebObject"] = this.mWeb;
            if (alldocData.ContainsKey("ComplianceTag"))
            {
                docData["ComplianceTag"] = alldocData["ComplianceTag"];
            }
            if (alldocData.ContainsKey("CustomizedPageStatus"))
            {
                docData["CustomizedPageStatus"] = alldocData["CustomizedPageStatus"];
            }
            PreRestoreWebParts(docInfo.WebParts, docInfo, docData);
        }

        private void PreRestoreWebParts(List<AveWebPartBaseInfo> webParts, AveDocumentInfo docInfo, Dictionary<string, object> docData)
        {
            if (docInfo.WebParts != null && docInfo.WebParts.Count > 0)
            {
                docData["WebParts"] = docInfo.WebParts;
                docData["WebPartRestoreCache"] = docInfo.WebPartCache;
            }
        }

        private void AssembleViewInfo(AveDocumentInfo info)
        {
            List<Dictionary<string, object>> viewInfos = new List<Dictionary<string, object>>();
            foreach (AveViewInfo view in info.AveView.Vinfos)
            {
                Dictionary<string, object> viewInfo = new Dictionary<string, object>();
                viewInfo["Id"] = view.Id;
                viewInfo["BaseViewId"] = view.BaseViewId;
                viewInfo["Title"] = view.Title;
                if (view.IsDefaultView.HasValue)
                {
                    viewInfo["SetAsDefaultView"] = view.IsDefaultView;
                }
                viewInfo["Scope"] = view.Scope;
                viewInfo["PersonalView"] = view.IsPersonal;
                viewInfo["LeafName"] = view.LeafName;
                viewInfo["UserID"] = view.UserID;
                viewInfo["ViewType"] = (int)view.ViewType;
                viewInfo["Hidden"] = view.Hidden;
                viewInfo["RowLimit"] = view.RowLimit;
                viewInfo["MobileView"] = view.IsMobileView;
                viewInfo["MobileDefaultView"] = view.IsDefaultMobileView;
                viewInfo["ViewData"] = view.ViewData;
                viewInfo["ContentTypeId"] = view.ContentTypeId;
                if (!string.IsNullOrEmpty(view.ListViewXml) && view.ListViewXml.StartsWith("<View"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(view.ListViewXml);
                    var viewNodeList = doc.SelectNodes("/View");
                    foreach (XmlNode viewNode in viewNodeList)
                    {
                        XmlAttribute attr = viewNode.Attributes["Url"];
                        if (attr != null)
                        {
                            string newUrl = AveReplaceProcessor.UrlReplace(attr.Value, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                            attr.Value = newUrl;
                            viewInfo["ListViewXml"] = doc.InnerXml;
                        }
                    }
                }
                else
                {
                    viewInfo["ListViewXml"] = view.ListViewXml;
                }
                viewInfos.Add(viewInfo);
            }
            info.DocData["ViewInformation"] = viewInfos;
        }
        }
}
