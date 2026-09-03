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
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using AvePoint.GCommon;
using System.Xml;
using System.Collections.Specialized;
using AvePoint.ObjectModel.WebService;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveWebPartRestore : IDisposable
    {
        protected static AveLogger Logger = AveLogger.GetInstance(typeof(AveWebPartRestore));
        protected ClientContext mContext;
        protected Web mWeb;
        protected List mList;
        protected File mPage;
        protected ListItem mFileItem;
        protected LimitedWebPartManager mLimitedWebPartManager;
        protected ExceptionHandlingScope mExcepScope;
        protected AveListMemento mListMemento;
        protected AveWebPartCache mMapping;
        protected int mScope;
        protected bool mClearAll;
        protected string mWebServerRelativeUrl;
        protected string mListTitle;
        protected Guid mListId;
        protected string mFileServerRelativeUrl;
        private string mFileUrl;
        protected Dictionary<string, string> mWebpartIdMapping = new Dictionary<string, string>();
        //Replace view fields时使用，记录目的端存在的fields
        protected List<string> mExsitFields = new List<string>();
        protected IReport mReport;

        public Dictionary<string, string> WebPartIdMapping
        {
            get { return mWebpartIdMapping; }
        }
        protected string FileUrl
        {
            get
            {
                if (string.IsNullOrEmpty(mFileUrl))
                {
                    string webAppUrl = AveUrlUtility.GetServerUrl(mContext.Url);
                    mFileUrl = webAppUrl + mFileServerRelativeUrl;
                }
                return mFileUrl;
            }
        }

        protected IAveWeb mCachedWeb;
        protected object mObj;
        public AveWebPartRestore(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, bool clearAll, ClientContext context, AveWebPartCache mapping, IAveWeb web, IReport report, object obj)
        {
            mContext = context;
            mMapping = mapping;
            mWebServerRelativeUrl = webServerRelativeUrl;
            mListTitle = listTitle;
            mListId = listId;
            mScope = scope;
            mClearAll = clearAll;
            mFileServerRelativeUrl = fileServerRelativeUrl;
            mCachedWeb = web;
            mObj = obj;
            mReport = report;
        }

        public AveWebPartRestore(ClientContext context, IAveWeb cachedWeb, Web web, List list, File page, LimitedWebPartManager limitedWebPartManager, ListItem item, AveWebPartCache mapping, IReport report, object obj)
        {
            mContext = context;
            mCachedWeb = cachedWeb;
            mScope = 1;
            mWeb = web;
            mList = list;
            mListId = list == null ? Guid.Empty : list.Id;
            mPage = page;
            mFileItem = item;
            mLimitedWebPartManager = limitedWebPartManager;
            mMapping = mapping;
            mFileServerRelativeUrl = mPage.ServerRelativeUrl;
            mClearAll = true;
            mObj = obj;
            mReport = report;
        }

        protected void LoadPageLimitedWebPartManager()
        {
            mLimitedWebPartManager = mPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            mContext.Load(mLimitedWebPartManager);
            mContext.Load(mLimitedWebPartManager, manager => manager.WebParts);
        }
        protected void LoadList()
        {
            mContext.Load(mList);
            mContext.Load(mList, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
        }
        public List<AveWebPartBaseInfo> GetNeedRestoreWebParts(System.Collections.IList webParts, bool needCheckDelete)
        {
            if (webParts == null)
            {
                return null;
            }
            List<AveWebPartBaseInfo> restoreWebParts = new List<AveWebPartBaseInfo>();
            XmlDocument webpartDoc = new XmlDocument();
            foreach (AveWebPartBaseInfo webpartInfo in webParts)
            {
                if (!string.IsNullOrEmpty(webpartInfo.DefinitionXml))
                {
                    //ADO-205610 support backup data from cloud backup
                    if (webpartInfo.DefinitionXml.StartsWith("<?xml version=\"1.0\" encoding=\"utf-16\"?>"))
                    {
                        webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                    }
                    webpartDoc.LoadXml(webpartInfo.DefinitionXml);
                    bool needPostRestore = this.UpdateWebPartDefinitionXml(webpartInfo, webpartDoc);
                    var webpartAssembly = string.Empty;
                    var webpartType = string.Empty;
                    if (!needCheckDelete && needPostRestore)//当前已经是Post action，并且needPostRestore == true。 加Failed Report。
                    {
                        var message = string.Format("Failed to restore web part because of missing web part data. File Url: {0}, WebPartId: {1}, WebPartTypeId: {2}.", mFileServerRelativeUrl, webpartInfo.ID, webpartInfo.WebPartTypeId);
                        Logger.Warn(message);
                        mReport.AddDetail(new AveWrapperWebpartReportDto(webpartInfo.DisplayName, webpartInfo.DisplayName, webpartInfo,
                        webpartInfo.Assembly, webpartInfo.Class, AveStatus.Failed, message));
                    }
                    else if (needCheckDelete && needPostRestore && webpartInfo.IsCurrentVersion)
                    {
                        AddUnRestoreWebPartInfo(mCachedWeb.ID, webpartInfo.ListId, mFileServerRelativeUrl, webpartInfo);
                    }
                    else
                    {
                        webpartInfo.DefinitionXml = webpartDoc.OuterXml;
                        restoreWebParts.Add(webpartInfo);
                    }
                }
            }
            return restoreWebParts;
        }
        private void GetWebpartAssemblyInfo(string webPartTypeString, out string webpartAssembly, out string webpartType)
        {
            webpartAssembly = string.Empty;
            webpartType = string.Empty;
            var index = webPartTypeString.IndexOf(',');
            if (index > 0)
            {
                webpartType = webPartTypeString.Substring(0, index);
                webpartAssembly = webPartTypeString.Substring(index + 1, webPartTypeString.Length - webpartType.Length - 1);
            }
        }
        protected bool UpdateWebPartDefinitionXml(AveWebPartBaseInfo webpartInfo, XmlDocument webpartDoc)
        {
            try
            {
                //替换webpart中的一些需要替换的信息，暂时只替换了一些url
                XmlNode webPartNode = webpartDoc.FirstChild;
                if ((webPartNode as XmlElement) == null)
                {
                    foreach (XmlNode node in webpartDoc.ChildNodes)
                    {
                        if (node is XmlElement)
                        {
                            webPartNode = node;
                        }
                    }
                }
                AveWebPartPropertyUpdater webPartPropertyUpdater = AveClientWebPartUrlHandlerFactory.GenerateWebPartUrlHanlder(webpartInfo.WebPartTypeId, mCachedWeb, webPartNode, mMapping);
                return webPartPropertyUpdater.UpdateWebPartProperty(webpartInfo, webpartDoc);
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred while update WebPart definition xml.Message:{0}.", ex.ToString());
                return false;
            }
        }

        internal protected void AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, AveWebPartBaseInfo info)
        {
            this.mMapping.SiteMappingManager.AddUnRestoreWebPartInfo(webId, listId, file, info);
        }

        public virtual void RestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            EnsureContext();

            RealRestoreWebParts(webpartBaseInfoList);
        }

        protected void EnsureContext()
        {
            mWeb = mContext.Site.OpenWeb(mWebServerRelativeUrl);
            mPage = mWeb.GetFileByServerRelativeUrl(mFileServerRelativeUrl);
            LoadPageLimitedWebPartManager();
            mExcepScope = new ExceptionHandlingScope(mContext);
            if (!string.IsNullOrEmpty(mListTitle) || (mListId != null && mListId != Guid.Empty))
            {
                TryLoadPageWithItem();
            }
            else
            {
                mContext.Load(mPage);
            }
            mContext.Load(mWeb, w => w.Id, w => w.ServerRelativeUrl);
            mContext.ExecuteQuery();
            if (mList != null && !mExcepScope.HasException && IsItemNotNull(mPage.ListItemAllFields))
            {
                mFileItem = mPage.ListItemAllFields;
                mListMemento = new AveListMemento(mList);
            }
        }

        protected void TryLoadPageWithItem()
        {
            if (mListId != Guid.Empty)
            {
                mList = mWeb.Lists.GetById(mListId);
            }
            else
            {
                mList = mWeb.Lists.GetByTitle(mListTitle);
            }
            LoadList();
            using (mExcepScope.StartScope())
            {
                using (mExcepScope.StartTry())
                {
                    mContext.Load(mPage);
                    mContext.Load(mPage.ListItemAllFields);
                }
                using (mExcepScope.StartCatch())
                {
                    mContext.Load(mPage);
                }
            }
        }


        protected void RealRestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                if (mListMemento != null)
                {
                    mListMemento.DisableVersionSettings();
                }
                InternalRestoreWebParts(webpartBaseInfoList);
                if (mListMemento != null)
                {
                    mListMemento.RevertVersionSettings();
                }
                mContext.ExecuteQuery();
            }
        }

        internal protected void InternalRestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            if (webpartBaseInfoList == null)
            {
                return;
            }
            View view;
            WebPartDefinition webPartDef;
            DeleteAllWebParts(out view, out webPartDef);
            if (webpartBaseInfoList.Count > 0)
            {
                foreach (var webpartInfo in webpartBaseInfoList)
                {
                    InternalRestoreWebPart(webpartInfo, view, webPartDef);
                }
                ReplaceWebPartIdInWikiContent();
            }
            mContext.ExecuteQuery();
            foreach (var webpartInfo in webpartBaseInfoList)
            {
                string oldWebPartId = webpartInfo.ID.ToString();
                if (webpartInfo.WebPartIdProperty != null)
                {
                    oldWebPartId = webpartInfo.WebPartIdProperty.TrimStart('g').TrimStart('_').Replace("_", "-");
                }
                string newWebPartId;
                if (WebPartIdMapping.TryGetValue(oldWebPartId, out newWebPartId) && !string.IsNullOrEmpty("newWebPartId"))
                {
                    PostUpdateWebPart(webpartInfo, newWebPartId);
                }
            }
        }

        protected virtual void PostUpdateWebPart(AveWebPartBaseInfo webpartInfo, string newWebPartId)
        {
            var updater = AveWebPartPostUpdater.CreateInstance(new Guid(newWebPartId), webpartInfo, mCachedWeb, mFileServerRelativeUrl, mObj);
            updater.PostUpdate();
        }

        protected void InternalRestoreWebPart(AveWebPartBaseInfo webpartInfo, View view, WebPartDefinition viewWebPart)
        {
            try
            {
                if (mList != null && webpartInfo.IsViewBuildInWebPart)
                {
                    UpdateViewWebPart(webpartInfo, view, viewWebPart, mList);
                    mContext.ExecuteQuery();
                }
                //else if (webpartInfo.SolutionId != Guid.Empty)
                //{
                //    AddWebPartWithWebService(webpartInfo);
                //}
                else
                {
                    ImportWebPart(webpartInfo);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Restore WebPart failed,WebpartId:{0}, due to:{1}", webpartInfo.ID, e);

                var message = string.Format("Restore WebPart failed,WebpartId:{0}, due to:{1}", webpartInfo.ID, e.Message);
                mReport.AddDetail(new AveWrapperWebpartReportDto(webpartInfo.DisplayName, webpartInfo.DisplayName, webpartInfo,
                    webpartInfo.Assembly, webpartInfo.Class, AveStatus.Failed, message));
            }
        }

        protected void AddWebPartWithWebService(AveWebPartBaseInfo webpartInfo)
        {
            string webAppUrl = AveUrlUtility.GetServerUrl(mContext.Url);
            string webUrl = webAppUrl + mCachedWeb.ServerRelativeUrl;
            string pageUrl = webAppUrl + mFileServerRelativeUrl;
            Guid webPartNewId = GetWebPartIdByWebservice(webpartInfo, webUrl, pageUrl);
            if (webpartInfo.WebPartIdProperty != null)
            {
                string webpartOldId = webpartInfo.WebPartIdProperty.TrimStart(new char[] { 'g' }).TrimStart(new char[] { '_' }).Replace("_", "-");
                mWebpartIdMapping[webpartOldId] = webPartNewId.ToString();
            }
            else
            {
                string webpartOldId = webpartInfo.ID.ToString();
                mWebpartIdMapping[webpartOldId] = webPartNewId.ToString();
            }
        }

        protected virtual Guid GetWebPartIdByWebservice(AveWebPartBaseInfo webpartInfo, string webUrl, string pageUrl)
        {
            return AveWebServiceRequest.AddWebPartWithWebService(webUrl, pageUrl, mObj, webpartInfo);
        }

        private string GetValidDefinationXmlForSP2010(AveWebPartBaseInfo webpartInfo, string originalDefinationXml)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(originalDefinationXml);
                List<XmlElement> nodeToRemove = new List<XmlElement>();
                foreach (XmlElement propertyEle in doc.DocumentElement.GetElementsByTagName("property"))
                {
                    if (propertyEle.HasAttribute("name"))
                    {
                        switch (propertyEle.GetAttribute("name"))
                        {
                            case "ZoneID":
                                {
                                    if (string.IsNullOrEmpty(webpartInfo.ZoneID))
                                    {
                                        webpartInfo.ZoneID = propertyEle.InnerText;
                                    }
                                    nodeToRemove.Add(propertyEle);
                                    break;
                                }
                            case "WebPartIdProperty":
                                {
                                    if (string.IsNullOrEmpty(webpartInfo.WebPartIdProperty))
                                    {
                                        webpartInfo.WebPartIdProperty = propertyEle.InnerText;
                                    }
                                    nodeToRemove.Add(propertyEle);
                                    break;
                                }
                            case "IsIncluded":
                                {
                                    try
                                    {
                                        webpartInfo.IsIncluded = Boolean.Parse(propertyEle.InnerText);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Info("Does not have IsIncluded property element. Exception: {0}", ex.ToString());
                                    }
                                    nodeToRemove.Add(propertyEle);
                                    break;
                                }
                            case "PartOrder":
                                {
                                    try
                                    {
                                        webpartInfo.PartOrder = Convert.ToInt32(propertyEle.InnerText);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Info("Does not have PartOrder property element. Exception: {0}", ex.ToString());
                                    }
                                    nodeToRemove.Add(propertyEle);
                                    break;
                                }
                            case "ID":
                                {
                                    if (Guid.Empty.Equals(webpartInfo.ID))
                                    {
                                        webpartInfo.ID = new Guid(propertyEle.InnerText);
                                    }
                                    nodeToRemove.Add(propertyEle);
                                    break;
                                }
                        }
                    }
                }
                foreach (var eleToRemove in nodeToRemove)
                {
                    eleToRemove.ParentNode.RemoveChild(eleToRemove);
                }
                return doc.OuterXml;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to remove invalid properties in DefinitionXml for SP2010. Exception: {0}", ex.ToString());
                return originalDefinationXml;
            }
        }

        protected void ImportWebPart(AveWebPartBaseInfo webpartInfo)
        {
            var importDefinationXml = webpartInfo.DefinitionXml;
            if (mContext.ServerVersion.Major == 14)
            {
                importDefinationXml = GetValidDefinationXmlForSP2010(webpartInfo, webpartInfo.DefinitionXml);
            }
            WebPartDefinition webpartDef = mLimitedWebPartManager.ImportWebPart(importDefinationXml);
            webpartDef = mLimitedWebPartManager.AddWebPart(webpartDef.WebPart, webpartInfo.ZoneID, int.MaxValue - 370);
            UpdateListViewWebPart(webpartDef, webpartInfo);
            string webpartNewId = new Guid(webpartDef.Id.ToString("D").TrimStart(new char[] { 'g' }).Replace("_", "")).ToString();
            if (webpartInfo.WebPartIdProperty != null)
            {
                string webpartOldId = webpartInfo.WebPartIdProperty.TrimStart(new char[] { 'g' }).TrimStart(new char[] { '_' }).Replace("_", "-");
                mWebpartIdMapping[webpartOldId] = webpartNewId;
            }
            else
            {
                string webpartOldId = webpartInfo.ID.ToString();
                mWebpartIdMapping[webpartOldId] = webpartNewId;
            }
            if (!webpartInfo.IsIncluded)
            {
                webpartDef.CloseWebPart();
            }
            webpartDef.MoveWebPartTo(webpartInfo.ZoneID, webpartInfo.PartOrder);
            webpartDef.SaveWebPartChanges();
            mContext.ExecuteQuery();
        }

        protected void DeleteAllWebParts(out View view, out WebPartDefinition viewWebPart)
        {
            view = null;
            viewWebPart = null;
            try
            {
                if (mList != null)
                {
                    foreach (View iView in mList.Views)
                    {
                        if (mPage.ServerRelativeUrl.EndsWith(iView.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            view = iView;
                            break;
                        }
                    }
                }
                if (mClearAll)
                {
                    List<WebPartDefinition> needDeleteWebParts = new List<WebPartDefinition>(mLimitedWebPartManager.WebParts.Count);
                    foreach (WebPartDefinition webpartDef in mLimitedWebPartManager.WebParts)
                    {
                        if (!this.mMapping.SiteMappingManager.ViewGuidMappingContainsValue(webpartDef.Id) ||
                            view == null || webpartDef.Id != view.Id)
                        {
                            needDeleteWebParts.Add(webpartDef);
                        }
                        else if (view != null && view.Id.Equals(webpartDef.Id))
                        {
                            viewWebPart = webpartDef;
                        }
                    }
                    int deleteWebPartCount = 0;
                    foreach (WebPartDefinition webpartDef in needDeleteWebParts)
                    {
                        webpartDef.DeleteWebPart();
                        deleteWebPartCount++;
                        if (deleteWebPartCount == 10)
                        {
                            mContext.ExecuteQuery();
                            deleteWebPartCount = 0;
                        }
                    }
                    mContext.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred while deleting webParts.Message:{0}.", ex.ToString());
                mLimitedWebPartManager = mPage.GetLimitedWebPartManager((PersonalizationScope)mScope);
                mContext.Load(mLimitedWebPartManager, manager => manager.WebParts);
                mContext.ExecuteQuery();
                DeleteRestWebParts();
            }
        }

        protected void DeleteRestWebParts()
        {
            List<WebPartDefinition> restDeleteWebParts = new List<WebPartDefinition>(mLimitedWebPartManager.WebParts.Count);
            foreach (WebPartDefinition webpartDef in mLimitedWebPartManager.WebParts)
            {
                restDeleteWebParts.Add(webpartDef);
            }
            if (restDeleteWebParts.Count > 1) //ADO-18967 Blog Archives WebPart删除后无法添加，故在此不做删除
            {
                for (int i = 1; i < restDeleteWebParts.Count; i++)
                {
                    restDeleteWebParts[i].DeleteWebPart();
                }
                mContext.ExecuteQuery();
            }
        }

        private bool LoadAndFindListField(string internalName, List list)
        {
            if (!mExsitFields.Contains(internalName))
            {
                try
                {
                    Field field = list.Fields.GetByInternalNameOrTitle(internalName);
                    mContext.Load(field);
                    mContext.ExecuteQuery();
                    mExsitFields.Add(internalName);
                }
                catch (Exception e)
                {
                    Logger.Warn("Can not find field in destination while replace view schemaXml, list Title:{0}, field name:{1}, error:{2}.", list.Title, internalName, e);
                    return false;
                }
            }
            return true;
        }

        private void ReplaceOrderFields(XmlDocument xDoc, IAveFieldMapping fieldMapping, List list)
        {
            XmlElement node = xDoc.SelectSingleNode("//OrderBy") as XmlElement;
            if (node != null)
            {
                XmlNodeList nodes = node.GetElementsByTagName("FieldRef");
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Attributes["Name"] != null)
                    {
                        string fieldName = nodes[i].Attributes["Name"].Value;
                        string mappingName = fieldMapping != null ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : String.Empty;
                        if (!string.IsNullOrEmpty(mappingName))
                        {
                            nodes[i].Attributes["Name"].Value = mappingName;
                        }
                        else if (!LoadAndFindListField(fieldName, list))
                        {
                            nodes[i].ParentNode.RemoveChild(nodes[i]);
                        }
                    }
                }
            }
        }

        private void ReplaceFilterFields(XmlDocument xDoc, IAveFieldMapping fieldMapping, List list)
        {
            XmlElement node = xDoc.SelectSingleNode("//Where") as XmlElement;
            if (node != null)
            {
                XmlNodeList nodes = node.GetElementsByTagName("FieldRef");
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Attributes["Name"] != null)
                    {
                        string fieldName = nodes[i].Attributes["Name"].Value;
                        string mappingName = fieldMapping != null ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : string.Empty;
                        if (!string.IsNullOrEmpty(mappingName))
                        {
                            nodes[i].Attributes["Name"].Value = mappingName;
                        }
                        else if (!LoadAndFindListField(fieldName, list))
                        {
                            #region 此处逻辑是将view filter中不存在的field从Where语句中移除，并使剩下的语句成立，否则view页会显示出错
                            XmlNode nodeA = nodes[i].ParentNode.ParentNode; // 当前or/and节点
                            if (nodeA.Name.Equals("Where", StringComparison.OrdinalIgnoreCase))
                            {
                                nodeA.RemoveChild(nodes[i].ParentNode);
                            }
                            else
                            {
                                XmlNode nodeB = nodes[i].ParentNode.ParentNode.ParentNode;  //Parent or/and节点
                                //field在目的端找不到，移除当前条件节点
                                nodeA.RemoveChild(nodes[i].ParentNode);
                                //将当前or/and节点下的剩余条件移到父or/and节点下
                                var childs = nodeA.ChildNodes;
                                for (int j = 0; j < childs.Count; j++)
                                {
                                    nodeB.AppendChild(childs[j]);
                                }
                                //将当前or/and节点移除
                                nodeB.RemoveChild(nodeA);
                                if (i < nodes.Count)
                                {
                                    //节点个数顺序发生变化，需递归重新遍历
                                    ReplaceFilterFields(xDoc, fieldMapping, list);
                                }
                                break;
                            }
                            #endregion
                        }
                    }
                }
            }
        }

        private string ReplaceQueryFields(string query, List list)
        {
            IAveFieldMapping fieldMapping;
            if (!mMapping.SiteMappingManager.TryGetValueFromListFieldsMapping(list.Id, out fieldMapping))
            {
                return query;
            }
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml("<Query>" + query + "</Query>");
                ReplaceOrderFields(xDoc, fieldMapping, list);
                ReplaceFilterFields(xDoc, fieldMapping, list);
                return xDoc.FirstChild.InnerXml;
            }
            catch (Exception e)
            {
                Logger.Warn("An error occurred while Replace view query fileds for the web part. Error: {0}", e);
                return query;
            }
        }

        protected void UpdateViewWebPart(AveWebPartBaseInfo webPartInfo, View view, WebPartDefinition viewWebPart, List list)
        {
            if (view == null)
            {
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webPartInfo.DefinitionXml);
            XmlNode defNode = doc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
            if (defNode == null)
            {
                defNode = doc.SelectSingleNode("//*[name() = 'ListViewXml']");
            }
            XmlDocument xmlDefNode = new XmlDocument();
            xmlDefNode.LoadXml(defNode.InnerText);
            AveXmlView xmlView = new AveXmlView(xmlDefNode.FirstChild);
            TranslateFieldName(xmlView, list.Id);
            if (!string.Equals(xmlView.Title, view.Title, StringComparison.OrdinalIgnoreCase))
            {
                view.Title = xmlView.Title;
            }
            if (xmlView.Aggregations != null && !string.Equals(xmlView.Aggregations, view.Aggregations, StringComparison.OrdinalIgnoreCase))
            {
                view.Aggregations = xmlView.Aggregations;
                view.AggregationsStatus = xmlView.AggregationsStatus;
            }
            if (!string.IsNullOrEmpty(xmlView.Query))
            {
                view.ViewQuery = ReplaceQueryFields(xmlView.Query, list);
            }
            if (xmlView.DefaultView != view.DefaultView)
            {
                view.DefaultView = xmlView.DefaultView;
            }
            if (xmlView.MobileDefaultView != view.MobileDefaultView)
            {
                view.MobileDefaultView = xmlView.MobileDefaultView;
            }
            if (xmlView.MobileView != view.MobileView)
            {
                view.MobileView = xmlView.MobileView;
            }
            if (xmlView.Paged != view.Paged)
            {
                view.Paged = xmlView.Paged;//true;
            }
            if (xmlView.RowLimit != view.RowLimit)
            {
                view.RowLimit = xmlView.RowLimit;
            }
            if (!string.IsNullOrEmpty(xmlView.Formats))
            {
                view.Formats = xmlView.Formats;
            }
            if (!string.IsNullOrEmpty(xmlView.ViewData))
            {
                view.ViewData = xmlView.ViewData;
            }
            if (!string.IsNullOrEmpty(xmlView.Joins))
            {
                view.ViewJoins = xmlView.Joins;
            }
            if (!string.Equals(xmlView.Toolbar, view.Toolbar, StringComparison.OrdinalIgnoreCase))
            {
                view.Toolbar = xmlView.Toolbar;
            }
            if (!string.IsNullOrEmpty(xmlView.Scope) && !string.Equals(xmlView.Scope, view.Scope.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                view.Scope = (ViewScope)Enum.Parse(typeof(ViewScope), xmlView.Scope, true);
            }

            ViewFieldCollection viewFields = view.ViewFields;
            XmlDocument viewFieldsDoc = new XmlDocument();
            viewFieldsDoc.LoadXml("<Fields>" + viewFields.SchemaXml + "</Fields>");
            List<string> needDeletedViewFields = new List<string>();
            List<string> exsitViewFields = new List<string>();
            foreach (XmlNode viewField in viewFieldsDoc.FirstChild.ChildNodes)
            {
                if (viewField.Attributes["Name"] != null)
                {
                    if (!xmlView.ViewFields.Contains(viewField.Attributes["Name"].Value))
                    {
                        needDeletedViewFields.Add(viewField.Attributes["Name"].Value);
                    }
                    else
                    {
                        exsitViewFields.Add(viewField.Attributes["Name"].Value);
                    }
                }
            }
            //Merge view fields for SharePoint 2013 Discussion Board.
            if (this.mList == null ||
                (this.mList.BaseTemplate != (int)AveListTemplateType.DiscussionBoard &&
                 this.mList.BaseTemplate != (int)AveListTemplateType.PictureLibrary) ||
                mCachedWeb.Site.CompatibilityLevel != 15)
            {
                foreach (string vf in needDeletedViewFields)
                {
                    viewFields.Remove(vf);
                }
            }
            ReorderViewFields(xmlView.ViewFields, viewFields, exsitViewFields);

            if (!string.IsNullOrEmpty(xmlView.CalendarSettings))
            {
                UpdateCalendarSettings(view, xmlView);
            }
            view.Update();
            UpdateWebPartProperties(viewWebPart, doc);
            viewWebPart.MoveWebPartTo(webPartInfo.ZoneID, webPartInfo.PartOrder);
            viewWebPart.SaveWebPartChanges();
        }
        protected virtual void UpdateCalendarSettings(View view, AveXmlView xmlView)
        {

        }

        protected void TranslateFieldName(AveXmlView view, Guid listId)
        {
            IAveFieldMapping fieldMapping;
            if (mList == null || !mMapping.SiteMappingManager.TryGetValueFromListFieldsMapping(listId, out fieldMapping))
            {
                return;
            }
            StringCollection fieldNames = new StringCollection();
            foreach (string fieldName in view.ViewFields)
            {
                string mappedFieldName = !string.IsNullOrEmpty(fieldMapping.GetMappingRestoredFieldInternalName(fieldName)) ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : fieldName;
                fieldNames.Add(mappedFieldName);
            }
            view.ViewFields = fieldNames;
        }

        protected void ReorderViewFields(StringCollection viewfields, ViewFieldCollection spviewfields, List<string> exsitViewFields)
        {
            for (int i = 0; i < viewfields.Count; i++)
            {
                if (exsitViewFields.Contains(viewfields[i]))
                {
                    spviewfields.MoveFieldTo(viewfields[i], i);
                    exsitViewFields.Remove(viewfields[i]);
                }
                else
                {
                    spviewfields.Add(viewfields[i]);
                    spviewfields.MoveFieldTo(viewfields[i], i);
                }
            }
        }

        protected void UpdateListViewWebPart(WebPartDefinition webpart, AveWebPartBaseInfo webpartInfo)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webpartInfo.DefinitionXml);
            XmlNode defNode = doc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
            if (defNode == null)
            {
                defNode = doc.SelectSingleNode("//*[name() = 'ListViewXml']");
            }
            XmlNode listIdNode = doc.SelectSingleNode(".//*[@name = 'ListId']");
            if (listIdNode == null)
            {
                listIdNode = doc.SelectSingleNode("//*[name() = 'ListId']");
            }
            if (listIdNode == null)
            {
                listIdNode = doc.SelectSingleNode("//*[name() = 'ListName']");
            }
            UpdateWebPartProperties(webpart, doc);
            if (defNode != null && listIdNode != null && new Guid(listIdNode.InnerText) != Guid.Empty)
            {
                List list = mWeb.Lists.GetById(new Guid(listIdNode.InnerText));
                mContext.Load(webpart);
                mContext.Load(list, l => l.Id, l => l.Title, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
                mContext.ExecuteQuery();
                View needUpdateView = GetViewByWebPartId(webpart.Id, list.Views);
                if (needUpdateView == null) //07 migration reload list views.
                {
                    mContext.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
                    mContext.ExecuteQuery();
                    needUpdateView = GetViewByWebPartId(webpart.Id, list.Views);
                }
                if (needUpdateView != null)
                {
                    UpdateViewWebPart(webpartInfo, needUpdateView, webpart, list);
                }
            }
            else
            {
                mContext.Load(webpart);
                mContext.ExecuteQuery();
            }
            CheckWebPartTitle(webpart, doc);
        }

        protected View GetViewByWebPartId(Guid webPartId, ViewCollection views)
        {
            foreach (View view in views)
            {
                if (webPartId == view.Id)
                {
                    return view;
                }
            }
            return null;
        }

        public virtual void CheckWebPartTitle(WebPartDefinition webPart, XmlDocument definitionXmlDoc) { }
        public virtual void UpdateWebPartProperties(WebPartDefinition webpart, XmlDocument doc) { }

        protected void ReplaceWebPartIdInWikiContent()
        {
            if (mList != null && mFileItem != null)
            {
                string fieldName = string.Empty;
                if (mList.BaseTemplate == (int)ListTemplateType.WebPageLibrary &&
                    mFileItem.FieldValues.ContainsKey("WikiField") &&
                    !string.IsNullOrEmpty(mFileItem["WikiField"] as string))
                {
                    fieldName = "WikiField";
                }
                else if (mList.BaseTemplate == 850 &&
                        mFileItem.FieldValues.ContainsKey("PublishingPageContent") &&
                        !string.IsNullOrEmpty(mFileItem["PublishingPageContent"] as string)) //Office 365 Site中的用于存放publishing page的特殊类型List，添加publishing page时，系统默认生成。
                {
                    fieldName = "PublishingPageContent";
                }
                ReplaceItemField(fieldName);
            }
        }

        protected virtual void ReplaceItemField(string FieldName)
        {
            if (string.IsNullOrEmpty(FieldName))
            {
                return;
            }
            StringBuilder sb = new StringBuilder(mFileItem[FieldName] as string);
            foreach (KeyValuePair<string, string> webpartId in mWebpartIdMapping)
            {
                sb.Replace(webpartId.Key, webpartId.Value);
            }
            mFileItem[FieldName] = sb.ToString();
            DateTime modified = (DateTime)mFileItem["Modified"];
            mFileItem["Modified"] = modified;
            CheckOutType pageCheckoutType = mPage.CheckOutType;
            if (mListMemento != null && mListMemento.EnableMinorVersions && pageCheckoutType == CheckOutType.None)
            {
                mPage.CheckOut();
            }
            mFileItem.Update();
            if (mListMemento != null && mListMemento.EnableMinorVersions && pageCheckoutType == CheckOutType.None)
            {
                mPage.CheckIn(mPage.CheckInComment, CheckinType.OverwriteCheckIn);
            }
        }

        protected bool IsItemNotNull(ListItem item)
        {
            return item.ServerObjectIsNull.HasValue && !item.ServerObjectIsNull.Value;
        }

        public void Dispose()
        {
        }
    }
}
