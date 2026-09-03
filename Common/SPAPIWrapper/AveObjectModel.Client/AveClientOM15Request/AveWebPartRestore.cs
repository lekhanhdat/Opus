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
using AveClientOM15Request;
using AvePoint.ObjectModel.Common.WebPart;
using System.Globalization;
using System.Net;
using Microsoft365.Authentication;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveWebPartRestore : IDisposable
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveWebPartRestore));
        private ClientContext mContext;
        private Web mWeb;
        private List mList;
        private DocumentRestoreInfo mDocumentParentInfo;
        private File mPage;
        private ListItem mFileItem;
        private LimitedWebPartManager mLimitedWebPartManager;
        private ConditionalScope mConditionScope;
        private AveListMemento mListMemento;
        private AveWebPartCache mMapping;
        private int mScope;
        private bool mPost;
        private string mWebServerRelativeUrl;
        private string mListTitle;
        private Guid mListId;
        private string mFileServerRelativeUrl;
        private FileLevel mLevel;
        private Dictionary<string, string> mWebpartIdMapping = new Dictionary<string, string>();
        private ITokenProvider tokenProvider;
        private IAveWeb mCachedWeb;
        private List<WebPartInfo> mBrowerFormWebparts = new List<WebPartInfo>();
        private List<WebPartDefinition> mBuiltInWebParts = new List<WebPartDefinition>();
        private bool mIsViewPage = false;
        private AveClientOM2013Request mRequest;

        private bool ListInfoIsNull
        {
            get
            {
                return mDocumentParentInfo.ParentListInfo == null;
            }
        }

        public AveWebPartRestore(AveClientOM2013Request request, string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, bool post, ClientContext context, AveWebPartCache mapping, ITokenProvider tokenProvider, bool isViewPage = false)
        {
            mRequest = request;
            mContext = context;
            mMapping = mapping;
            mWebServerRelativeUrl = webServerRelativeUrl;
            mListTitle = listTitle;
            mListId = listId;
            mScope = scope;
            mPost = post;
            mFileServerRelativeUrl = fileServerRelativeUrl;
            this.tokenProvider = tokenProvider;
            mIsViewPage = isViewPage;
        }

        public AveWebPartRestore(AveClientOM2013Request request, ClientContext context, IAveWeb cachedWeb, DocumentRestoreInfo parentInfo, List loadedList, File page, LimitedWebPartManager limitedWebPartManager, ListItem item, AveWebPartCache mapping, ITokenProvider tokenProvider, bool isViewPage)
        {
            mRequest = request;
            mContext = context;
            mCachedWeb = cachedWeb;
            mScope = 1;
            mDocumentParentInfo = parentInfo;
            mPage = page;
            mFileItem = item;
            mList = loadedList;
            mLimitedWebPartManager = limitedWebPartManager;
            mMapping = mapping;
            mWebServerRelativeUrl = parentInfo.ParentWebInfo.ServerRelativeUrl;
            mFileServerRelativeUrl = mPage.ServerRelativeUrl;
            mPost = false;
            this.tokenProvider = tokenProvider;
            mIsViewPage = isViewPage;
            InitWebAndList();
        }

        private void InitWebAndList()
        {
            mWeb = mContext.Site.OpenWeb(mWebServerRelativeUrl);
            if (mList == null && !this.ListInfoIsNull && Guid.Empty != mDocumentParentInfo.ParentListInfo.Id)
            {
                mList = mWeb.Lists.GetById(mDocumentParentInfo.ParentListInfo.Id);
            }

        }

        //private void SetCookie(object sender, WebRequestEventArgs e)
        //{
        //    AveWebRequestExecutor requestExecutor = e.WebRequestExecutor as AveWebRequestExecutor;
        //    if (requestExecutor != null)
        //    {
        //        if (mCookieContainer != null)
        //        {
        //            requestExecutor.Request.CookieContainer = mCookieContainer;
        //        }
        //        else
        //        {
        //            requestExecutor.Request.Credentials = tokenProvider as NetworkCredential;
        //        }
        //        if (mAuthMode == AuthenticationMode.Forms)
        //        {
        //            requestExecutor.Request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
        //        }
        //    }
        //}

        public void RestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            EnsureContext();

            InternalRestoreWebParts(webpartBaseInfoList, true);
        }

        private void EnsureContext()
        {
            mWeb = mContext.Site.OpenWeb(mWebServerRelativeUrl);
            mPage = mWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(mFileServerRelativeUrl));
            mLimitedWebPartManager = mPage.GetLimitedWebPartManager((PersonalizationScope)mScope);
            mConditionScope = new ConditionalScope(mContext, ()=> mPage.ListItemAllFields.ServerObjectIsNull.Value);
            if (Guid.Empty != mListId)
            {
                TryLoadPageWithItem();
            }
            else
            {
                mContext.Load(mPage);
            }
            mContext.Load(mWeb, w => w.Id, w => w.ServerRelativeUrl, w => w.Url);
            mContext.Load(mLimitedWebPartManager);
            mContext.Load(mLimitedWebPartManager, manager => manager.WebParts);
            mContext.ExecuteQuery();
            mIsViewPage = true;
            mLevel = mPage.Level;
            if (mList != null && mConditionScope.TestResult.HasValue && !mConditionScope.TestResult.Value)
            {
                mFileItem = mPage.ListItemAllFields;
                mListMemento = new AveListMemento(mContext, mList);
            }
        }

        private void TryLoadPageWithItem()
        {
            mList = mWeb.Lists.GetById(mListId);
            mContext.Load(mList);
            mContext.Load(mList, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
            using (mConditionScope.StartScope())
            {
                using (mConditionScope.StartIfTrue())
                {
                    mContext.Load(mPage);
                }
                using (mConditionScope.StartIfFalse())
                {
                    mContext.Load(mPage);
                    mContext.Load(mPage.ListItemAllFields);
                }
            }
        }

        private void RevertModerationStatus(Dictionary<string, object> needKeepProperties)
        {
            if (needKeepProperties.Count > 0)
            {
                string webServerRelativeUrl = mDocumentParentInfo.ParentWebInfo == null ? mWeb.ServerRelativeUrl : mDocumentParentInfo.ParentWebInfo.ServerRelativeUrl;
                string listTitle = this.ListInfoIsNull ? mList.Title : mDocumentParentInfo.ParentListInfo.Title;
                string webApp = AveUrlUtility.GetServerUrl(mContext.Url).Trim('/');
                mRequest.WebServiceRequestOnline.UpdateListItems(webApp, webServerRelativeUrl, listTitle, mPage.ListItemAllFields.Id, mPage.ServerRelativeUrl, needKeepProperties);
            }
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
                    if (webpartInfo.DefinitionXml.StartsWith("<?xml version=\"1.0\" encoding=\"utf-16\"?>"))
                    {
                        webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                    }
                    webpartDoc.LoadXml(webpartInfo.DefinitionXml);
                    bool needPostRestore = this.UpdateWebPartDefinitionXml(webpartInfo, webpartDoc);
                    if (needCheckDelete && needPostRestore && webpartInfo.IsCurrentVersion)
                    {
                        AddUnRestoreWebPartInfo(mCachedWeb.ID, webpartInfo.ListId, mFileServerRelativeUrl, webpartInfo);
                    }
                    else if (!needPostRestore)
                    {
                        webpartInfo.DefinitionXml = webpartDoc.OuterXml;
                        restoreWebParts.Add(webpartInfo);
                    }
                }
            }
            return restoreWebParts;
        }

        protected bool UpdateWebPartDefinitionXml(AveWebPartBaseInfo webpartInfo, XmlDocument webpartDoc)
        {
            try
            {
                //替换webpart中的一些需要替换的信息，暂时只替换了一些url
                XmlNode webPartNode = webpartDoc.FirstChild;
                if((webPartNode as XmlElement) == null)
                {
                    foreach(XmlNode node in webpartDoc.ChildNodes)
                    {
                        if(node is XmlElement)
                        {
                            webPartNode = node;
                        }
                    }
                }
                AveWebPartPropertyUpdater webPartPropertyUpdater = AveClientWebPartUrlHandlerFactory.GenerateWebPartUrlHanlder(webpartInfo.DefinitionXml, mCachedWeb, webPartNode, mMapping);
                return webPartPropertyUpdater.UpdateWebPartProperty(webpartInfo, webpartDoc);
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred while update WebPart definition xml.Message:{0}.", ex.ToString());
                return false;
            }
        }

        internal void AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, AveWebPartBaseInfo info)
        {
            lock (this.mMapping.UnRestoreWebPartCache)
            {
                if (!this.mMapping.UnRestoreWebPartCache.ContainsKey(listId))
                {
                    this.mMapping.UnRestoreWebPartCache.Add(listId, new Dictionary<Guid, Dictionary<string, List<object>>>());
                }
                if (!this.mMapping.UnRestoreWebPartCache[listId].ContainsKey(webId))
                {
                    this.mMapping.UnRestoreWebPartCache[listId].Add(webId, new Dictionary<string, List<object>>());
                }
                if (!this.mMapping.UnRestoreWebPartCache[listId][webId].ContainsKey(file))
                {
                    this.mMapping.UnRestoreWebPartCache[listId][webId].Add(file, new List<object>());
                }
                this.mMapping.UnRestoreWebPartCache[listId][webId][file].Add(info);
            }
        }

        internal void InternalRestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList, bool requireCheckout)
        {
            Dictionary<string, object> needKeepProperties = new Dictionary<string, object>();
            bool needCheckIn = false;
            if (requireCheckout && mFileItem != null && mPage.Level != FileLevel.Checkout)
            {
                mListMemento = new AveListMemento(mContext, mList, mPage);
                mListMemento.DisableVersionSettings();
                if (mList.EnableVersioning && mList.EnableMinorVersions)
                {
                    mPage.CheckOut();
                    needCheckIn = true;
                }
            }

            View view = DeleteAllWebParts();

            if (webpartBaseInfoList != null && webpartBaseInfoList.Count > 0)
            {
                foreach (AveWebPartBaseInfo webpartInfo in webpartBaseInfoList)
                {
                    if (!string.IsNullOrEmpty(webpartInfo.ZoneID))
                    {
                        InternalRestoreWebPart(webpartInfo, view);
                    }
                    else
                    {
                        Logger.Warn("zone id is missing: {0}", webpartInfo.DefinitionXml);
                    }
                }

                ReplaceWebPartIdInWikiContent();
                mContext.ExecuteQuery();
                string webUrl = mDocumentParentInfo.ParentWebInfo == null ? mWeb.Url : mDocumentParentInfo.ParentWebInfo.Url;
                string webServerRelativeUrl = mDocumentParentInfo.ParentWebInfo == null ? mWeb.ServerRelativeUrl : mDocumentParentInfo.ParentWebInfo.ServerRelativeUrl;
                foreach (WebPartInfo webPartInfo in mBrowerFormWebparts)
                {
                    mRequest.WebServiceRequestOnline.UpdateBroswerFormWebPartProperty(webUrl, webServerRelativeUrl, mFileServerRelativeUrl, webPartInfo.NewId, webPartInfo.WebpartInfo.DefinitionXml);
                }
            }
            if (needCheckIn)
            {
                mPage.CheckIn(string.Empty, CheckinType.OverwriteCheckIn);
            }
            if (mListMemento != null)
            {
                mListMemento.RevertVersionSettings();
            }
            if (mContext.HasPendingRequest)
            {
                mContext.ExecuteQuery();
            }
            RevertModerationStatus(needKeepProperties);
        }

        private void InternalRestoreWebPart(AveWebPartBaseInfo webpartInfo, View view)
        {
            try
            {
                Guid newId = Guid.Empty;
                if (mList != null && webpartInfo.IsViewBuildInWebPart)
                {
                    UpdateViewWebPart(webpartInfo, view, string.Empty);
                }
                //else if (webpartInfo.SolutionId != null && webpartInfo.SolutionId != Guid.Empty)
                //{
                //    AddWebPartWithWebService(webpartInfo);
                //    return;
                //}
                else
                {
                    newId = ImportWebPart(webpartInfo);
                }
                mContext.ExecuteQuery();
                RestoreBroswerFormWebPartProperty(webpartInfo, newId);
            }
            catch (Exception e)
            {
                //SAAS-37269
                if ((e is ServerException) && (e as ServerException).ServerErrorCode == AveSPErrorCode.TP_E_OVERQUOTA)
                {
                    throw;
                }
                Logger.Error("restore webpart failed, url:{0}, due to:{1}", mFileServerRelativeUrl, e.ToString());
            }
        }

        private Guid ImportWebPart(AveWebPartBaseInfo webpartInfo)
        {
            Logger.Info("start to import web part with definition xml:{0}", webpartInfo.DefinitionXml);

            //webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace("BaseViewID=\"12\"", "BaseViewID=\"1\"").Replace("BaseViewID=\"11\"", "BaseViewID=\"1\"");

            WebPartDefinition webpartDef = mLimitedWebPartManager.ImportWebPart(webpartInfo.DefinitionXml);
            webpartDef = mLimitedWebPartManager.AddWebPart(webpartDef.WebPart, webpartInfo.ZoneID == null ? "" : webpartInfo.ZoneID, webpartInfo.PartOrder);
            UpdateListViewWebPart(webpartDef, webpartInfo);
            return webpartDef.Id;
        }

      

        private void RestoreBroswerFormWebPartProperty(AveWebPartBaseInfo webpartInfo, Guid newId)
        {
            IWebPartPropertyExtractor wpExtractor = WebPartExtractorFactory.Create(webpartInfo.DefinitionXml);
            if (wpExtractor.TypeFullName.Contains("Microsoft.Office.InfoPath.Server.Controls.WebUI.BrowserFormWebPart"))
            {
                WebPartInfo webPartInfo = new WebPartInfo(webpartInfo, newId);
                mBrowerFormWebparts.Add(webPartInfo);
            }
        }



        private void UpdateWebPartProperties(WebPartDefinition webpartDef, AveWebPartBaseInfo webpartInfo)
        {
            string webpartNewId = new Guid(webpartDef.Id.ToString("D").TrimStart(new char[] { 'g' }).Replace("_", "")).ToString();
            if (webpartInfo.WebPartIdProperty != null)
            {
                string webpartOldId = new Guid((webpartInfo.WebPartIdProperty.TrimStart(new char[] { 'g' }).Replace("_", ""))).ToString();
                mWebpartIdMapping[webpartOldId] = webpartNewId;
            }
            else
            {
                string webpartOldId = webpartInfo.ID.ToString();
                mWebpartIdMapping[webpartOldId] = webpartNewId;
            }

            WebPartPropertyUpdater updater = WebPartPropertyUpdaterSelector.Select(webpartDef, webpartInfo);
            if (updater != null)
            {
                updater.SetMapping(mMapping);
                updater.Update();
            }

            if (!webpartInfo.IsIncluded)
            {
                webpartDef.CloseWebPart();
            }
            if (webpartInfo.PartOrder != webpartDef.WebPart.ZoneIndex)
            {
                webpartDef.MoveWebPartTo(webpartInfo.ZoneID, webpartInfo.PartOrder);
                Logger.Info("move webpart:{0} to zone:{1} part order:{2}", webpartInfo.DisplayName, webpartInfo.ZoneID, webpartInfo.PartOrder);
            }
            webpartDef.SaveWebPartChanges();
        }

        private View DeleteAllWebParts()
        {
            View view = null;
            try
            {
                if (mList != null && mIsViewPage)
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
                if (!mPost)
                {
                    List<WebPartDefinition> needDeleteWebParts = new List<WebPartDefinition>(mLimitedWebPartManager.WebParts.Count);
                    foreach (WebPartDefinition webpartDef in mLimitedWebPartManager.WebParts)
                    {
                        if (view == null || new Guid(webpartDef.Id.ToString().TrimStart(new char[] { 'g' }).Replace("_", "")) != view.Id)
                        {
                            needDeleteWebParts.Add(webpartDef);
                            continue;
                        }
                        mBuiltInWebParts.Add(webpartDef);
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
            return view;
        }

        private void DeleteRestWebParts()
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
                //mClientContext.ExecuteQuery();
            }
        }

        private void UpdateViewWebPart(AveWebPartBaseInfo webPartInfo, View view, string viewlistId)
        {
            if (view != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webPartInfo.DefinitionXml);
                XmlNode defNode = doc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                XmlNode listNameNode = doc.SelectSingleNode(".//*[@name = 'ListName']");
                if (defNode == null)
                {
                    defNode = doc.SelectSingleNode("//*[name() = 'ListViewXml']");
                    XmlDocument xmlDefNode = new XmlDocument();
                    xmlDefNode.LoadXml(defNode.InnerText);
                    AveXmlView xmlView = new AveXmlView(xmlDefNode.FirstChild);
                    TranslateFieldName(xmlView);
                    if (!string.Equals(xmlView.Title, view.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!webPartInfo.IsViewBuildInWebPart)
                        {
                            view.Title = xmlView.Title;
                        }
                    }
                    if (xmlView.Aggregations != null && !string.Equals(xmlView.Aggregations, view.Aggregations, StringComparison.OrdinalIgnoreCase))
                    {
                        view.Aggregations = xmlView.Aggregations;
                        view.AggregationsStatus = xmlView.AggregationsStatus;
                    }
                    if (!string.IsNullOrEmpty(xmlView.Query))
                    {
                        view.ViewQuery = xmlView.Query;
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
                    string listViewXml = view.ListViewXml;
                    viewFields.RemoveAll();
                    //Delete source dirty view fields
                    StringCollection finalViewFields = xmlView.ViewFields;
                    if (!ListInfoIsNull && mDocumentParentInfo.ParentListInfo.ListFieldInternalNames.Count > 0)
                    {
                        for (int i = 0; i < finalViewFields.Count;i++ )
                        {
                            if (!mDocumentParentInfo.ParentListInfo.ListFieldInternalNames.Contains(finalViewFields[i]))
                            {
                                finalViewFields.Remove(finalViewFields[i]);
                            }
                        }
                    }
                    ReorderViewFields(finalViewFields, viewFields);
                    view.ListViewXml = listViewXml;
                }
                else
                {
                    XmlDocument xmlDefNode = new XmlDocument();
                    xmlDefNode.LoadXml(defNode.InnerText);
                    XmlNode viewFieldsNode = xmlDefNode.SelectSingleNode("View/ViewFields");
                    if (viewFieldsNode != null)
                    {
                        List<XmlElement> needDeleteViewFields = new List<XmlElement>();
                        foreach (XmlElement viewField in viewFieldsNode.ChildNodes)
                        {
                            string fieldName = viewField.GetAttribute("Name");
                            string mappedFieldName = mMapping.FieldInternalNameMapping.ContainsKey(fieldName) ? mMapping.FieldInternalNameMapping[fieldName] : fieldName;
                            if (!ListInfoIsNull
                                    && mDocumentParentInfo.ParentListInfo != null
                                    && mDocumentParentInfo.ParentListInfo.Id != null
                                    && listNameNode != null
                                    && mDocumentParentInfo.ParentListInfo.Id.ToString().Equals(listNameNode.InnerText))
                            {
                                if (mDocumentParentInfo.ParentListInfo.ListFieldInternalNames != null
                                    && !mDocumentParentInfo.ParentListInfo.ListFieldInternalNames.Contains(mappedFieldName))
                                {
                                    needDeleteViewFields.Add(viewField);
                                    continue;
                                }
                            }
                            viewField.SetAttribute("Name", mappedFieldName);
                        }
                        if (needDeleteViewFields.Count > 0)
                        {
                            foreach (XmlNode node in needDeleteViewFields)
                            {
                                viewFieldsNode.RemoveChild(node);
                            }
                        }
                    }
                    //XmlNode styleIdNode = xmlDefNode.SelectSingleNode("View/ViewStyle");
                    //if (styleIdNode != null)
                    //{
                    //    styleIdNode.ParentNode.RemoveChild(styleIdNode);
                    //}
                    XmlNode viewNode = xmlDefNode.SelectSingleNode("View");
                    if (viewNode != null && !string.IsNullOrEmpty(viewlistId))
                    {
                        XmlElement viewElement = viewNode as XmlElement;
                        string sourceCTId = viewElement.GetAttribute("ContentTypeID");
                        if (!string.IsNullOrEmpty(sourceCTId))
                        {
                            Dictionary<string, string> ctIdMapping = new Dictionary<string, string>();
                            if (mMapping.ListCTIdMapping.TryGetValue(new Guid(viewlistId), out ctIdMapping))
                            {
                                string desId = null;
                                if (ctIdMapping.TryGetValue(sourceCTId, out desId))
                                {
                                    viewElement.SetAttribute("ContentTypeID", desId);
                                }
                            }
                        }
                    }
                    XmlNode withIndexNode = xmlDefNode.SelectSingleNode("View/Query/WithIndex");
                    if (withIndexNode != null)
                    {
                        XmlNode parentNode = withIndexNode.ParentNode;
                        //XmlNode tempNode = withIndexNode.Clone();
                        parentNode.RemoveChild(withIndexNode);
                        XmlNode preNode = null;
                        for (int index = withIndexNode.ChildNodes.Count - 1; index >= 0; index--)
                        {
                            var curNode = withIndexNode.ChildNodes[index];
                            if (preNode == null)
                            {
                                preNode = parentNode.AppendChild(curNode);
                            }
                            else
                            {
                                preNode = parentNode.InsertBefore(curNode, preNode);
                            }
                        }
                        //foreach (XmlNode child in withIndexNode.ChildNodes)
                        //{
                        //    parentNode.AppendChild(child);
                        //}
                    }
                    XmlNode spotLightNode = xmlDefNode.SelectSingleNode("View/SpotlightInfo");
                    if(spotLightNode != null && mMapping.ViewInfo != null)
                    {
                        foreach(var vInfo in mMapping.ViewInfo.Vinfos)
                        {
                            if (string.Equals(vInfo.Title,view.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                XmlDocument viewDoc = new XmlDocument();
                                viewDoc.LoadXml(vInfo.ListViewXml);
                                XmlNode newSpotLightNode = viewDoc.SelectSingleNode("View/SpotlightInfo");
                                spotLightNode.InnerText = newSpotLightNode.InnerText;
                            }
                        }
                    }
                    xmlDefNode.DocumentElement.SetAttribute("Name", view.Id.ToString("B").ToUpper());
                    xmlDefNode.DocumentElement.SetAttribute("Url", view.ServerRelativeUrl);
                    
                    //支持view中的TabularView属性转移 by zma
                    bool tabularView;
                    if (bool.TryParse(xmlDefNode.DocumentElement.GetAttribute("TabularView"), out tabularView))
                    {
                        view.TabularView = tabularView;
                    }
                    view.ListViewXml = xmlDefNode.DocumentElement.InnerXml;
                }
                view.Update();
                //move built-in view WebPart. Just support only one exists in the view.
                WebPartDefinition builtInViewWebPart = mBuiltInWebParts.FirstOrDefault();
                if (builtInViewWebPart != null && webPartInfo.IsViewBuildInWebPart)
                {
                    XmlNode flagNode = doc.SelectSingleNode("//*[name()='ViewFlag']");
                    if (flagNode != null && !string.IsNullOrEmpty(flagNode.InnerText))
                    {
                        int sourceFlag = int.Parse(flagNode.InnerText);
                        builtInViewWebPart.WebPart.Properties["ViewFlags"] = sourceFlag;
                    }
                    else
                    {
                        if (webPartInfo.Flags != 0)
                        {
                            builtInViewWebPart.WebPart.Properties["ViewFlags"] = webPartInfo.Flags;
                        }
                    }
                    XmlNode timelineNode = doc.SelectSingleNode(".//*[@name ='ShowTimelineIfAvailable']");
                    if (timelineNode != null && !string.IsNullOrEmpty(timelineNode.InnerText))
                    {
                        bool sourceTimeLine = bool.Parse(timelineNode.InnerText);
                        builtInViewWebPart.WebPart.Properties["ShowTimelineIfAvailable"] = sourceTimeLine;
                    }
                    builtInViewWebPart.MoveWebPartTo(webPartInfo.ZoneID, webPartInfo.PartOrder);
                    builtInViewWebPart.SaveWebPartChanges();
                    mBuiltInWebParts.Remove(builtInViewWebPart);
                }
            }
        }


        private void TranslateFieldName(AveXmlView view)
        {
            StringCollection fieldNames = new StringCollection();
            foreach (string fieldName in view.ViewFields)
            {
                string mappedFieldName = mMapping.FieldInternalNameMapping.ContainsKey(fieldName) ? mMapping.FieldInternalNameMapping[fieldName] : fieldName;
                fieldNames.Add(mappedFieldName);
            }
            view.ViewFields = fieldNames;
        }

        private void ReorderViewFields(StringCollection viewfields, ViewFieldCollection spviewfields)
        {
            for (int i = 0; i < viewfields.Count; i++)
            {
                spviewfields.Add(viewfields[i]);
                spviewfields.MoveFieldTo(viewfields[i], i);
            }
        }

        private void UpdateListViewWebPart(WebPartDefinition webpart, AveWebPartBaseInfo webpartInfo)
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
            try
            {
                if (defNode != null && listIdNode != null && new Guid(listIdNode.InnerText) != Guid.Empty)
                {
                    List list = mWeb.Lists.GetById(new Guid(listIdNode.InnerText));
                    mContext.Load(webpart);
                    mContext.Load(webpart, wp => wp.WebPart.ZoneIndex);
                    mContext.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
                    mContext.ExecuteQuery();
                    View needUpdateView = null;
                    foreach (View view in list.Views)
                    {
                        if (webpart.Id == view.Id)
                        {
                            needUpdateView = view;
                            break;
                        }
                    }
                    UpdateWebPartProperties(webpart, webpartInfo);//update view after webpart is saved
                    if (needUpdateView != null)
                    {
                        UpdateViewWebPart(webpartInfo, needUpdateView, listIdNode.InnerText);
                    }
                }
                else
                {
                    mContext.Load(webpart);
                    mContext.Load(webpart, wp => wp.WebPart.ZoneIndex);
                    mContext.ExecuteQuery();
                    UpdateWebPartProperties(webpart, webpartInfo);
                }
            }
            /*review-qlluo*/catch (Exception ex)
            {
                Logger.Warn("update list view webPart failed.due to:{0}. webPart definition xml:{1}", ex.Message, webpartInfo.DefinitionXml);
                throw;
            }
        }

        private void ReplaceWebPartIdInWikiContent()
        {
            if (mList != null && mFileItem != null)
            {
                int baseTemplate = this.ListInfoIsNull ? mList.BaseTemplate : mDocumentParentInfo.ParentListInfo.BaseTemplate;
                string fieldName = string.Empty;
                if (baseTemplate == (int)ListTemplateType.WebPageLibrary &&
                    mFileItem.FieldValues.ContainsKey("WikiField") &&
                    !string.IsNullOrEmpty(mFileItem["WikiField"] as string))
                {
                    fieldName = "WikiField";
                }
                else if (baseTemplate == 850 &&
                        mFileItem.FieldValues.ContainsKey("PublishingPageContent") &&
                        !string.IsNullOrEmpty(mFileItem["PublishingPageContent"] as string)) //Office 365 Site中的用于存放publishing page的特殊类型List，添加publishing page时，系统默认生成。
                {
                    fieldName = "PublishingPageContent";
                }
                ReplaceItemField(fieldName);
            }
            if(mList == null)
            {
                throw new ArgumentNullException();
            }
            if (mLevel == FileLevel.Published &&
                ((!this.ListInfoIsNull && mDocumentParentInfo.ParentListInfo.EnableMinorVersions) || (!this.ListInfoIsNull && mList.EnableMinorVersions)))
            {
                mPage.Publish(string.Empty);
            }
        }
        private void ReplaceItemField(string FieldName)
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
            FieldUserValue userValue = mFileItem["Editor"] as FieldUserValue;
            DateTime modified = (DateTime)mFileItem["Modified"];
            if (userValue != null)
            {
                if (!FieldName.Equals("PublishingPageContent", StringComparison.OrdinalIgnoreCase))
                {
                    mFileItem["Editor"] = userValue.LookupId;//PublishingPage page更新editor会抛异常；
                }
                mFileItem["Modified"] = modified;
            }
            CheckOutType pageCheckoutType = mPage.CheckOutType;
            mFileItem.Update();
        }
        public void Dispose()
        {
            if (mBuiltInWebParts.Count > 0)
            {
                mBuiltInWebParts.Clear();
            }
        }
    }

    public class WebPartInfo
    {
        public AveWebPartBaseInfo WebpartInfo { get; set; }
        public Guid NewId { get; set; }

        public WebPartInfo(AveWebPartBaseInfo WebpartInfo, Guid NewId)
        {
            this.WebpartInfo = WebpartInfo;
            this.NewId = NewId;
        }
    }
}
