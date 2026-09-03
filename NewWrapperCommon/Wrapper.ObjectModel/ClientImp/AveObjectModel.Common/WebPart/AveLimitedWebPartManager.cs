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
using System.Xml;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.ObjectModel.Common.WebPart;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_3, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveLimitedWebPartManager : AveClientObject, IAveLimitedWebPartManager
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList m_List;
        private AveFile mFile;
        private string mFileServerRelativeUrl;
        private IAveRequest mRequest;
        private bool mLoaded = false;
        private IReport mReport;
        static private AveLogger mLogger=AveLogger.GetInstance(typeof(AveLimitedWebPartManager));

        public bool NeedReloadList { get { return false; } }

        public AveLimitedWebPartManager(AveWeb web, AveFile file, IAveRequest request, Dictionary<string, object> limitedWebPartManagerProperties)
        {
            mWeb = web;
            mFile = file;
            mRequest = request;
            base.DataCache.AddPropertyies(limitedWebPartManagerProperties);
            mFileServerRelativeUrl = file.ServerRelativeUrl;
            mLoaded = true;
        }

        public AveLimitedWebPartManager(IAveSite site, IAveWeb web, IAveFile file)
        {
            mSite = site as AveSite;
            mWeb = web as AveWeb;
            mFile = file as AveFile;
            mRequest = mSite.Request;
            mFileServerRelativeUrl = mFile.ServerRelativeUrl;
        }

        public AveLimitedWebPartManager(IAveSite site, IAveWeb web, string fileServerRelativeUrl)
        {
            mSite = site as AveSite;
            mWeb = web as AveWeb;
            mRequest = mSite.Request;
            mFileServerRelativeUrl = fileServerRelativeUrl;
        }

        private void EnsureWebPartManagerData()
        {
            if (AveUrlUtility.IsAspx(mFileServerRelativeUrl, false))
            {
                Dictionary<string, object> webpartManagerProperties = this.mRequest.GetLimitedWebPartManager(this.Web.ServerRelativeUrl, mFileServerRelativeUrl, (int)System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared, this.Web.IsAppWeb ? this.Web.Url : null);
                base.DataCache.AddPropertyies(webpartManagerProperties);
                mLoaded = true;
            }
        }

        #region IAveLimitedWebPartManager Members
        public List<AveWebPartBaseInfo> GetWebParts(AveBaseItemInfo info)
        {
            List<AveWebPartBaseInfo> webpartBaseInfoList = new List<AveWebPartBaseInfo>();
            if (!info.IsCurrentVersion || this.WebParts == null)
            {
                return webpartBaseInfoList;
            }
            foreach (AveWebPart webpart in (this.WebParts as AveLimitedWebPartCollection).WebParts)
            {
                webpart.BaseInfo.IsCurrentVersion = info.IsCurrentVersion;
                webpart.BaseInfo.PageVersion = info.Version;
                webpartBaseInfoList.Add(webpart.BaseInfo);
            }
            return webpartBaseInfoList;
        }

        public void RestoreWebParts(List<AveWebPartBaseInfo> webparts)
        {
            AveList list = mWeb.GetList(mFileServerRelativeUrl) as AveList;
            string listTitle = null;
            Guid listId = Guid.Empty;
            if (list != null)
            {
                //MarkViewBuiltinWebPart(list, webparts);
                listTitle = list.Title;
                listId = list.ID;
            }

            mRequest.RestoreWebParts(mWeb.ServerRelativeUrl, listTitle, listId, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared, webparts, Cache, false, mWeb,mReport);
        }

        // check webpart is view builtin webpart
        private void MarkViewBuiltinWebPart(AveList list, List<AveWebPartBaseInfo> webparts)
        {
            AveView webpartview = null;
            foreach (AveView view in list.Views)
            {
                if (mFile.ServerRelativeUrl.EndsWith(view.Url, StringComparison.OrdinalIgnoreCase))
                {
                    webpartview = view;
                    break;
                }
            }
            if (webpartview != null)
            {
                XmlDocument doc = new XmlDocument();
                XmlDocument viewDoc = new XmlDocument();
                foreach (AveWebPartBaseInfo webpartBaseInfo in webparts)
                {
                    doc.LoadXml(webpartBaseInfo.DefinitionXml);
                    XmlNode defNode = doc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                    if (defNode != null)
                    {
                        viewDoc.LoadXml(defNode.InnerText);
                        if (new Guid(viewDoc.DocumentElement.GetAttribute("Name")) == webpartview.ID)
                        {
                            webpartBaseInfo.IsViewBuildInWebPart = true;
                            viewDoc.DocumentElement.SetAttribute("Url", webpartview.Url);
                            defNode.InnerText = viewDoc.OuterXml;
                            break;
                        }
                    }
                    XmlNode listViewNode = doc.SelectSingleNode(".//*[@name = 'ListViewXml']");
                    if (listViewNode != null)
                    {
                        viewDoc.LoadXml(listViewNode.InnerText);
                        if (new Guid(viewDoc.DocumentElement.GetAttribute("Name")) == webpartview.ID)
                        {
                            webpartBaseInfo.IsViewBuildInWebPart = true;
                            viewDoc.DocumentElement.SetAttribute("Url", webpartview.Url);
                            listViewNode.InnerText = viewDoc.OuterXml;
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateWebParts(List<string> webparts)
        {
            throw new NotImplementedException();
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        public IAveLimitedWebPartCollection WebParts
        {
            get
            {
                if (!mLoaded)
                {
                    this.EnsureWebPartManagerData();
                }
                if (base.DataCache.IsPropertyNotLoaded("WebParts") && base.DataCache.IsPropertyAvailable("WebParts" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> webPartsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("WebParts" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveLimitedWebPartCollection limitedWebPartCollection = new AveLimitedWebPartCollection(this, mRequest, webPartsProperties,mWeb);
                    base.DataCache.PropertiesCache["WebParts"] = limitedWebPartCollection;
                    return limitedWebPartCollection;
                }
                return base.DataCache.GetProperty<IAveLimitedWebPartCollection>("WebParts");
            }
        }

        public void AddWebPart(IAveWebPart webPart, string zoneId, int zoneIndex)
        {
            throw new NotImplementedException();
        }

        public void CloseWebPart(IAveWebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void DeleteWebPart(IAveWebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex)
        {
            throw new NotImplementedException();
        }

        public void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex, bool isShared)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(IAveWebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(IAveWebPart webPart, bool isShared)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        public void AddWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex)
        {
            throw new NotImplementedException();
        }

        public void CloseWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void DeleteWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void MoveWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex)
        {
            throw new NotImplementedException();
        }

        public void MoveWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex, bool isShared)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(System.Web.UI.WebControls.WebParts.WebPart webPart, bool isShared)
        {
            throw new NotImplementedException();
        }

        public void OpenWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            throw new NotImplementedException();
        }

        public System.Web.UI.WebControls.WebParts.WebPart CreateWebPartInstance(string assemblyName, string webPartType)
        {
            throw new NotImplementedException();
        }

        #region IAveLimitedWebPartManager Members

        public void RestoreWebParts(System.Collections.IList webParts, bool clearAll)
        {
            if(NeedSkipInfoPathList())
            {
                return;
            }
            AveList list = mWeb.GetList(mFileServerRelativeUrl) as AveList;
            string listTitle = null;
            Guid listId = Guid.Empty;
            if (list != null)
            {
                //MarkViewBuiltinWebPart(list, webparts);
                listTitle = list.Title;
                listId = list.ID;
            }
            mRequest.RestoreWebParts(mWeb.ServerRelativeUrl, listTitle, listId, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared,webParts, this.Cache, clearAll, mWeb, mReport);
        }

        internal void AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, object info)
        {
            this.Cache.SiteMappingManager.AddUnRestoreWebPartInfo(webId, listId, file, info);
        }

        public AveWebPartCache Cache
        {
            internal get { return mCache; }
            set { mCache = value; }
        }

        private AveWebPartCache mCache = null;

        protected bool UpdateWebPartDefinitionXml(AveWebPartBaseInfo webpartInfo, XmlDocument webpartDoc)
        {
            try
            {
                //替换webpart中的一些需要替换的信息，暂时只替换了一些url
                AveWebPartPropertyUpdater webPartPropertyUpdater = AveClientWebPartUrlHandlerFactory.GenerateWebPartUrlHanlder(webpartInfo.WebPartTypeId, mWeb, webpartDoc.FirstChild, mCache);
                return webPartPropertyUpdater.UpdateWebPartProperty(webpartInfo, webpartDoc);
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while update WebPart definition xml.Message:{0}.", ex.ToString());
                return false;
            }
        }
        #endregion

        #region IAveLimitedWebPartManager Members


        public void OpenWebPart(IAveWebPart webPart)
        {
            throw new NotImplementedException();
        }

        #endregion


        public void RestoreWebParts(List<AveWebPartBaseInfo> webparts, bool clearAll)
        {
            if(NeedSkipInfoPathList())
            {
                return;
            }
            AveList list = mWeb.GetList(mFileServerRelativeUrl) as AveList;
            string listTitle = null;
            Guid listId = Guid.Empty;
            if (list != null)
            {
                //MarkViewBuiltinWebPart(list, webparts);
                listTitle = list.Title;
                listId = list.ID;
            }
            mRequest.RestoreWebParts(mWeb.ServerRelativeUrl, listTitle, listId, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared, webparts, this.Cache, clearAll, mWeb, mReport);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key _ipfs_infopathenabled")]
        private bool NeedSkipInfoPathList()
        {
            if(mFile == null)
            {
                mFile = this.Web.GetFile(mFileServerRelativeUrl) as AveFile;
            }

            if (mFile != null)
            {
                var parentFolder = this.mFile.ParentFolder;
                if (parentFolder != null &&
                    parentFolder.Exists &&
                    parentFolder.Properties.ContainsKey("_ipfs_infopathenabled") &&
                    ((string)parentFolder.Properties["_ipfs_infopathenabled"]).Equals("True", StringComparison.OrdinalIgnoreCase) &&
                    this.mFile.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    // ADO-128220 InfoPath sharepoint list view的WebPart不能还原出来，需要使用web service的模拟InfoPath的publish创建出来。skip view的还原。
                    //throw new AveRestoreException(AveRestoreResult.Omit, "Sharepoint InfoPath view WebPart will be created by publishing the list");
                    return true;
                }
            }
            return false;
        }

        public void PostRestoreWebParts(List<AveWebPartBaseInfo> webparts)
        {
            throw new NotImplementedException();
        }

        public void UpdatePropertiesInDatabase(string webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties)
        {
            throw new NotImplementedException();
        }

        public void UpdatePersonalPropertiesInDatabase(string webPartId, Guid siteId, int currentUserId, byte[] perUserBytes)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserID(string webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal)
        {
            throw new NotImplementedException();
        }

        public void UpdateView(string webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId)
        {
            throw new NotImplementedException();
        }

        public void UpdateWebPartInfo(string webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion)
        {
            throw new NotImplementedException();
        }

        public void DeleteWebPartByNative(Guid siteId, Guid docId, string webPartId)
        {
            throw new NotImplementedException();
        }

        public void ResetPersonalizationState(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void ResetPersonalizationState(IAveWebPart webPart)
        {
            throw new NotImplementedException();
        }

        public void SetRestoreReport(IReport report)
        {
            mReport = report;
        }

        public IAveWebPart ImportWebPart(XmlReader reader, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public IAveWebPart ImportWebPart(XmlReader reader, bool isShared, out string errorMessage)
        {
            throw new NotImplementedException();
        }


        public void ExportWebPart(IAveWebPart webPart, XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}