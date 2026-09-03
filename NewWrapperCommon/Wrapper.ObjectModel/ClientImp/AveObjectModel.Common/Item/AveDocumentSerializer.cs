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
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace AvePoint.ObjectModel.Common
{
    class AveDocumentSerializer : IAveDocumentSerializer, IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveDocumentSerializer));
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
        public void SetReport(IReport report)
        {
            mReport = report;
        }

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

        public AveRestoreResult SetObjectData(AveDocumentInfo info, IAveRestoreStream receiver, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<IAveListItem> holdItems, System.Collections.Hashtable HTMetaInfo)
        {
            info.DocData = AveList.AssembleBaseItemInfo(info, this.mList);
            this.AssembleDocumentInfo(info.DocData, info, allDocData, allUserData);
            HandleDocumentIdService(info.FieldsInfo.Fields);
            
            info.FieldsInfo.Fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields);
            if (!info.FieldsInfo.Fields.ContainsKey("Modified"))
            {
                info.FieldsInfo.Fields.Add("Modified", info.DTimeLastModified);
            }
            if (!info.FieldsInfo.Fields.ContainsKey("CurrentUserRatings") && allUserData.ContainsKey("CurrentUserRatings"))
            {
                info.FieldsInfo.Fields.Add("CurrentUserRatings", allUserData["CurrentUserRatings"]);
            }
            if (mList != null) //File in the web system folder does not has parent list
            {
                if (mWeb.Site.APIType != AveAPIType.BPOS_S || string.Compare(mWeb.Site.SPVersion, "15.", StringComparison.OrdinalIgnoreCase) > 0)//ADO-82366
                {
                    mList.SetTaxonomyField(info, -1, info.IsForceAddTerm, info.FieldsInfo.TermIdMapping, info.FieldsInfo.MergedTermIdMapping);
                }
                info.FieldsInfo.Fields.Add("NeedSetNullFields", info.NeedSetNullFields);
            }
            if (info.IsView)
            {
                this.AssembleViewInfo(info.DocData, info.AveView.Vinfos);
            }
            Stream content = new AveSPFileStream(receiver);
            // 在ReplaceWebPartContent方法中，已经处理了Infopath相关URL的替换。
            /*if (mList != null && mList.BaseTemplate == AveListTemplateType.XMLForm && info.Name.EndsWith(".xml",StringComparison.OrdinalIgnoreCase))
            {
                content = FixBrokenLinks(info, content);
            }*/
            if ((mList == null || mList.BaseTemplate != AveListTemplateType.XMLForm) && info.Name.EndsWith(".Xml", StringComparison.OrdinalIgnoreCase))
            {
                if (!info.ParentLibraryIsMasterPageGallery)
                {
                    content = FixXmlUrl(info, content);
                }
                else
                {
                    mLogger.Debug("The ParentList is Master Page Gallery, and needn't to execute the method 'FixXmlUrl'");
                }
            }
            else if (info.Name.EndsWith(".rdl", StringComparison.OrdinalIgnoreCase))//暂时保留, 减少构造ContentTypeId的次数, 以后添加其他类型时考虑去掉该判断
            {
                object ctIdObj;
                if (info.FieldsInfo.Fields.TryGetValue("ContentType", out ctIdObj) && ctIdObj != null)
                {
                    switch (ReportServiceUtil.GetReportFileType(info.Name, new AveContentTypeId(ctIdObj.ToString())))
                    {
                        case ReportFileType.RDL:
                            content = FixRdlUrl(info, content);
                            break;
                        default:
                            break;
                    }
                }
            }

            info.ServerRelativeUrl = info.ParentFolderRelativeUrl.TrimEnd('/') + "/" + info.Name;
            AveContentReplacer contentReplacer = new AveContentReplacer(info.AveItem.Folder.ParentWeb.Site, content, info);
            content = contentReplacer.ReplaceWebPartContent();

            #region For Nintex Form
            if (info.FieldsInfo.Fields.ContainsKey("FormData")
                && !string.IsNullOrEmpty(info.FieldsInfo.Fields["FormData"] == null ? "" : info.FieldsInfo.Fields["FormData"].ToString())
                && mList.ParentWeb.Site.IsOnlineSite)
            {
                string formDataValue = info.FieldsInfo.Fields["FormData"].ToString();
                info.FieldsInfo.Fields.Remove("FormData");
                info.FieldsInfo.Fields.Add("NFFormData", formDataValue);
            }
            #endregion

            Dictionary<string, object> restoreResult = mRequest.RestoreDocument(info, content, mReport);

            if (restoreResult.ContainsKey("File"))
            {
                Dictionary<string, object> properties = restoreResult["File"] as Dictionary<string, object>;
                AveFile newFile = new AveFile(mRequest, mWeb, mList, mParentFolder, properties);
                info.AveItem.File = newFile;
                info.AveItem.ListItem = newFile.Item;
                info.Version = newFile.UIVersion;
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

            if (restoreResult.ContainsKey("ListVersionSetting"))
            {
                info.SettingInfo.LIST_SETTING_CHANGED = true;
                mList.DataCache.AddPropertyies(restoreResult["ListVersionSetting"] as Dictionary<string, object>);
            }
            if (restoreResult.ContainsKey("RowId"))
            {
                info.RowId = (int)restoreResult["RowId"];
            }
            bool isNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;
            if (restoreResult.ContainsKey("Exception"))
            {
                mLogger.Error("Restore document {0} failed, due to:{1}.", info.ServerRelativeUrl, restoreResult["Exception"]);
                //return AveRestoreResult.Failed;这个状态外围都已经不处理了，所以改成throw；
                throw new Exception(restoreResult["ExceptionMessage"].ToString());
            }

            if (info.IsView)
            {
                AveList list = mWeb.Lists[info.ParentListTitle] as AveList;
                if (restoreResult.ContainsKey("View"))
                {
                    AveViewCollection views = list.Views as AveViewCollection;
                    AveView view = new AveView(list, views, mRequest, restoreResult["View"] as Dictionary<string, object>);
                    views.ListData.Add(view);
                    info.AveItem.View = view;
                }
                else if (info.AveItem.File != null && !string.IsNullOrEmpty(info.AveItem.File.ServerRelativeUrl))
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
                info.IsNewCreated = isNewCreated;
            }
            info.RestoringItem.IsNewItem = isNewCreated;
            info.RestoringItem.OverwriteAllVersion = restoreResult.ContainsKey("OverwriteAllVersion") ? (bool)restoreResult["OverwriteAllVersion"] : false;
            bool conflictWithDocument = restoreResult.ContainsKey("ConflictWithDocument") ? (bool)restoreResult["ConflictWithDocument"] : false;
            if (conflictWithDocument)
            {
                info.RestoringItem.ConflictType = ConflictType.Document;
            }
            NeedThrowSkipException(restoreResult, info);
            return AveRestoreResult.Normal;
        }
        /// <summary>
        /// Check if restore document Id when the DocumentId Service is actived.
        /// </summary>
        private void HandleDocumentIdService(Dictionary<string, object> fields)
        {
            if (!WrapperConfiguration.KeepDocumentIdValue &&
                this.mWeb.Site.Features[new Guid("B50E3104-6812-424f-A011-CC90E6327318")] != null)
            {
                AveList.RemoveDocumentId(fields);
            }
        }

        private void NeedThrowSkipException(Dictionary<string, object> restoreResult, AveDocumentInfo info)
        {
            if (restoreResult.ContainsKey("SkipViewItem") && Convert.ToBoolean(restoreResult["SkipViewItem"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.ConflictType = ConflictType.Document;
                string message = restoreResult.ContainsKey("SkipViewMessage") ? restoreResult["SkipViewMessage"].ToString() : AveRestoreResult.SkipTheSameItem.ToString();
                mLogger.Log(AveLogLevel.WARN, message);
                throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, message);
            }
            if (restoreResult.ContainsKey("SkippedByLastModifiedTime") && Convert.ToBoolean(restoreResult["SkippedByLastModifiedTime"]))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.ConflictType = ConflictType.Document;
                string message = restoreResult.ContainsKey("RestoreMessage") ? restoreResult["RestoreMessage"].ToString() : AveRestoreResult.SkipTheSameItem.ToString();
                mLogger.Log(AveLogLevel.WARN, message);
                throw new AveRestoreException(AveRestoreResult.Omit, message);
            }
            if (restoreResult.ContainsKey("SkipTopicFile") || restoreResult.ContainsKey("SkipConflict"))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.ConflictType = ConflictType.Document;
                string message = restoreResult.ContainsKey("RestoreMessage") ? restoreResult["RestoreMessage"].ToString() : AveRestoreResult.SkipTheSameItem.ToString();
                mLogger.Log(AveLogLevel.WARN, message);
                throw new AveRestoreException(AveRestoreResult.Omit, message);
            }
        }

        private Stream FixRdlUrl(AveDocumentInfo info, Stream content)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveReportingService.ReplaceReportStream"))
            {

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(content);
                XmlNodeList dsrNodeList = xmlDoc.GetElementsByTagName("DataSourceReference");
                if (dsrNodeList.Count > 0)
                {
                    for (int i = 0; i < dsrNodeList.Count; i++)
                    {
                        dsrNodeList[i].InnerText = AveReplaceProcessor.UrlReplace(dsrNodeList[i].InnerText, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.SourceSiteInfo, info.ParentSiteServerRelativeUrl);
                    }
                }
                XmlNodeList nodelist = xmlDoc.DocumentElement.GetElementsByTagName("rd:ReportServerUrl");
                if (nodelist.Count > 0)
                {
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        nodelist[i].InnerText = AveReplaceProcessor.UrlReplace(nodelist[i].InnerText, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.SourceSiteInfo, info.ParentSiteServerRelativeUrl);
                    }
                }
                Stream newStream = new MemoryStream();
                xmlDoc.Save(newStream);
                newStream.Position = 0;
                return newStream;
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while fixing rdl file url, file url: {0}, file name: {1}, error: {2}", info.Url, info.Name, e);
            }
            return content;

            }

        }

        private Stream FixXmlUrl(AveDocumentInfo info, Stream content)
        {
            if (content.Length <= 0)
            {
                return content;
            }
            byte[] originalData = new byte[content.Length];
            content.Read(originalData, 0, (int)content.Length);
            MemoryStream fixedContent;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                if (info.Name.Equals("RTE2ToolbarExtension.xml", StringComparison.OrdinalIgnoreCase))
                {
                    xmlDoc.LoadXml(originalData.ToString());
                }
                else
                {
                    xmlDoc.LoadXml(Encoding.UTF8.GetString(originalData));
                }
                //xmlDoc.LoadXml(Encoding.UTF8.GetString(originalData));
                xmlDoc.InnerXml = AveReplaceProcessor.ReplaceUrlInXml(xmlDoc.InnerXml, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), info.SourceSiteInfo, mWeb.Site.Url);
                fixedContent = new MemoryStream();
                xmlDoc.Save(fixedContent);
                fixedContent.Position = 0;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Replace xml file failed.Message:{0}", ex.ToString());
                fixedContent = new MemoryStream(originalData);
            }
            return fixedContent;
        }

        private Stream FixBrokenLinks(AveDocumentInfo info, Stream content)
        {
            if (content.Length <= 0)
            {
                return content;
            }
            XmlDocument infoPathDocument = new XmlDocument();
            infoPathDocument.PreserveWhitespace = true;
            infoPathDocument.Load(content);
            XmlNode node = infoPathDocument.SelectSingleNode("/processing-instruction(\"mso-infoPathSolution\")");
            if (node == null)
            {
                return content;
            }
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
                    else if (info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl != null &&
                             !url.StartsWith(info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl, StringComparison.OrdinalIgnoreCase))
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
            //ADO-133973:如果content不作任何修改，返回原content的话会导致position变化，导致不能read，所以需要重新save XmlDocument
            MemoryStream fixedContent = new MemoryStream();
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

        private void PreRestoreWebParts(List<AveWebPartBaseInfo> webParts, AveDocumentInfo docInfo, Dictionary<string, object> docData)
        {
            if (docInfo.WebParts != null && docInfo.WebParts.Count > 0)
            {
                docData["WebParts"] = docInfo.WebParts;
                docData["WebPartRestoreCache"] = docInfo.WebPartCache;
            }
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
            if (allUserData.ContainsKey("ContentType"))
            {
                docData["ContentType"] = allUserData["ContentType"];
            }
            else if (allUserData.ContainsKey("#tp_ContentTypeId"))
            {
                docData["ContentType"] = new AveContentTypeId((byte[])allUserData["#tp_ContentTypeId"]).ToString();
            }
            docData["IsOriginalCheckOut"] = docInfo.IsOrignialCheckOut || isOriginalCheckOut;
            docData["CheckoutUserId"] = docInfo.CheckoutUserId;
            docData["CheckInComment"] = docInfo.CheckinComment;
            docData["SolutionId"] = docInfo.SolutionId;
            if (this.mWeb.AllProperties != null && this.mWeb.AllProperties.ContainsKey("_reportinggallerytemplateid"))
            {
                docData["_reportinggallerytemplateid"] = this.mWeb.AllProperties["_reportinggallerytemplateid"];
            }
            if (allUserData.ContainsKey("#SolutionStatus"))
            {
                docData["SolutionStatus"] = allUserData["#SolutionStatus"];
            }
            if (allUserData.ContainsKey("#tp_GUID"))
            {
                docData["GUID"] = allUserData["#tp_GUID"];
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
            if (docInfo.OriginalRowId <= 0)
            {
                if (docInfo.MetaInfoDic != null && docInfo.MetaInfoDic.ContainsKey("ContentTypeId"))
                {
                    Dictionary<string, string> properties = null;
                    if (docData.ContainsKey("Properties"))
                    {
                        properties = docData["Properties"] as Dictionary<string, string>;
                    }
                    if(properties == null)
                    {
                        properties = new Dictionary<string, string>();
                    }
                    properties["ContentTypeId"] = docInfo.MetaInfoDic["ContentTypeId"];
                    docData["Properties"] = properties;
                }
            }
            docData["AveWebObject"] = this.mWeb;
            PreRestoreWebParts(docInfo.WebParts, docInfo, docData);
        }

        private void AssembleViewInfo(Dictionary<string, object> docData, List<AveViewInfo> views)
        {
            List<Dictionary<string, object>> viewInfos = new List<Dictionary<string, object>>();
            foreach (AveViewInfo view in views)
            {
                Dictionary<string, object> viewInfo = new Dictionary<string, object>();
                viewInfo["Id"] = view.Id;
                viewInfo["BaseViewId"] = view.BaseViewId;
                viewInfo["Title"] = view.Title;
                if (view.IsDefaultView.HasValue)
                {
                    viewInfo["SetAsDefaultView"] = view.IsDefaultView;
                }
                viewInfo["PersonalView"] = view.IsPersonal;
                viewInfo["LeafName"] = view.LeafName;
                viewInfo["UserID"] = view.UserID;
                viewInfo["ViewType"] = (int)view.ViewType;
                viewInfos.Add(viewInfo);
            }
            docData["ViewInformation"] = viewInfos;
        }

        public void Dispose()
        {
            if(mReport !=null)
            {
                mReport.Dispose();
            }
        }
    }
}
