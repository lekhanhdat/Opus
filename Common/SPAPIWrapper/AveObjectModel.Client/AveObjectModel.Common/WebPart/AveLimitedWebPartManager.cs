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
namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_3, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveLimitedWebPartManager : AveClientObject, IAveLimitedWebPartManager
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveFile mFile;
        private string mFileServerRelativeUrl;
        private IAveRequest mRequest;
        private bool mLoaded = false;
        static private AveLogger mLogger = AveLogger.GetInstance(typeof(AveLimitedWebPartManager));

        public AveLimitedWebPartManager(AveWeb web, AveFile file, IAveRequest request, Dictionary<string, object> limitedWebPartManagerProperties)
        {
            mWeb = web;
            mFile = file;
            mRequest = request;
            base.DataCache.AddPropertyies(limitedWebPartManagerProperties);
            mFileServerRelativeUrl = file.ServerRelativeUrl;
            mLoaded = true;
        }

        public AveLimitedWebPartManager(AveWeb web, string fileServerRelativeUrl, IAveRequest request, Dictionary<string, object> limitedWebPartManagerProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(limitedWebPartManagerProperties);
            mFileServerRelativeUrl = fileServerRelativeUrl;
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
                Dictionary<string, object> webpartManagerProperties = this.mRequest.GetLimitedWebPartManager(this.Web.ServerRelativeUrl, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared);
                base.DataCache.AddPropertyies(webpartManagerProperties);
                mLoaded = true;
            }
        }

        #region IAveLimitedWebPartManager Members
        public List<AveWebPartBaseInfo> GetWebParts(AveBaseItemInfo info)
        {
            List<AveWebPartBaseInfo> webpartBaseInfoList = new List<AveWebPartBaseInfo>();
            if (this.WebParts != null)
            {
                foreach (AveWebPart webpart in this.WebParts)//(this.WebParts as AveLimitedWebPartCollection).WebParts)
                {
                    webpart.BaseInfo.IsCurrentVersion = info.IsCurrentVersion;
                    webpart.BaseInfo.PageVersion = info.Version;
                    webpartBaseInfoList.Add(webpart.BaseInfo);
                }
            }
            return webpartBaseInfoList;
        }

        //public void RestoreWebParts(List<AveWebPartBaseInfo> webparts)
        //{
        //    AveList list = mWeb.GetList(mFileServerRelativeUrl) as AveList;
        //    string listTitle = null;
        //    Guid listId = Guid.Empty;
        //    if (list != null)
        //    {
        //        //MarkViewBuiltinWebPart(list, webparts);
        //        listTitle = list.Title;
        //        listId = list.ID;
        //    }

        //    mRequest.RestoreWebParts(mWeb.ServerRelativeUrl, listTitle, listId, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared, webparts, Cache, false);
        //}

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
                    AveLimitedWebPartCollection limitedWebPartCollection = new AveLimitedWebPartCollection(this, mRequest, webPartsProperties, mWeb);
                    base.DataCache.AddProperty("WebParts",limitedWebPartCollection);
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
            mRequest.CloseWebPart(mWeb.ServerRelativeUrl, mFileServerRelativeUrl, new Guid(webPart.ID));
        }

        public void DeleteWebPart(IAveWebPart webPart)
        {
            mRequest.DeleteWebPart(mWeb.ServerRelativeUrl, mFileServerRelativeUrl, new Guid(webPart.ID));
            (this.WebParts as AveLimitedWebPartCollection).ListData.Remove(webPart);
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
            //throw new NotImplementedException();
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

        //public void AddWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex)
        //{
        //    throw new NotImplementedException();
        //}

        //public void CloseWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        //{
        //    throw new NotImplementedException();
        //}

        //public void DeleteWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        //{
        //    throw new NotImplementedException();
        //}

        //public void MoveWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex)
        //{
        //    throw new NotImplementedException();
        //}

        //public void MoveWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart, string zoneId, int zoneIndex, bool isShared)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SaveChanges(System.Web.UI.WebControls.WebParts.WebPart webPart)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SaveChanges(System.Web.UI.WebControls.WebParts.WebPart webPart, bool isShared)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OpenWebPart(System.Web.UI.WebControls.WebParts.WebPart webPart)
        //{
        //    throw new NotImplementedException();
        //}

        //public System.Web.UI.WebControls.WebParts.WebPart CreateWebPartInstance(string assemblyName, string webPartType)
        //{
        //    throw new NotImplementedException();
        //}

        #region IAveLimitedWebPartManager Members

        public void RestoreWebParts(System.Collections.IList webParts, bool isCurrentVersion)
        {
            List<AveWebPartBaseInfo> postActionRestoredWebParts = new List<AveWebPartBaseInfo>();
            List<AveWebPartBaseInfo> restoreWebParts = new List<AveWebPartBaseInfo>();
            XmlDocument webpartDoc = new XmlDocument();
            foreach (AveWebPartBaseInfo webpartInfo in webParts)
            {
                if (!string.IsNullOrEmpty(webpartInfo.DefinitionXml))
                {
                    webpartDoc.LoadXml(webpartInfo.DefinitionXml);
                    bool needPostRestore = this.UpdateWebPartDefinitionXml(webpartInfo, webpartDoc);
                    if (isCurrentVersion && needPostRestore)
                    {
                        AddUnRestoreWebPartInfo(mWeb.ID, webpartInfo.ListId, mFileServerRelativeUrl, webpartInfo);
                    }
                    else if (!needPostRestore)
                    {
                        webpartInfo.DefinitionXml = webpartDoc.OuterXml;
                        restoreWebParts.Add(webpartInfo);
                    }
                }
            }
            //IAveLimitedWebPartManager webpartManager = mParentSite.ObjectModelFactory.CreateLimitedWebPartManager(mParentSite.SPSite, mAveDoc.SPFile.Web, mAveDoc.SPFile);
            //if (postActionRestoredWebParts.Count > 0)
            //{
            //    mParentSite.WebPartPageMapping.Add(webpartManager, postActionRestoredWebParts);
            //}
            RestoreWebParts(restoreWebParts, !isCurrentVersion);
        }

        internal void AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, object info)
        {
            lock (this.Cache.UnRestoreWebPartCache)
            {
                if (!this.Cache.UnRestoreWebPartCache.ContainsKey(listId))
                {
                    this.Cache.UnRestoreWebPartCache.Add(listId, new Dictionary<Guid, Dictionary<string, List<object>>>());
                }
                if (!this.Cache.UnRestoreWebPartCache[listId].ContainsKey(webId))
                {
                    this.Cache.UnRestoreWebPartCache[listId].Add(webId, new Dictionary<string, List<object>>());
                }
                if (!this.Cache.UnRestoreWebPartCache[listId][webId].ContainsKey(file))
                {
                    this.Cache.UnRestoreWebPartCache[listId][webId].Add(file, new List<object>());
                }
                this.Cache.UnRestoreWebPartCache[listId][webId][file].Add(info);
            }
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
                AveWebPartPropertyUpdater webPartPropertyUpdater = AveClientWebPartUrlHandlerFactory.GenerateWebPartUrlHanlder(webpartInfo.DefinitionXml, mWeb, webpartDoc.FirstChild, mCache);
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


        public void RestoreWebParts(List<AveWebPartBaseInfo> webparts, bool post)
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
            mRequest.RestoreWebParts(mWeb.ServerRelativeUrl, listTitle, listId, mFileServerRelativeUrl, (int)AvePersonalizationScope.Shared, webparts, this.Cache, post);
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

        //public void ResetPersonalizationState(System.Web.UI.WebControls.WebParts.WebPart webPart)
        //{
        //    throw new NotImplementedException();
        //}

        public void ResetPersonalizationState(IAveWebPart webPart)
        {
            IAveRequest request = mRequest as IAveRequest;
            if (request != null)
            {
                string id = !string.IsNullOrEmpty(webPart.ID) ? webPart.ID : webPart.WebPartIdProperty;
                id = id.StartsWith("g_") ? id.TrimStart(new char[]{'g','_'}).Replace('_','-') : id;
                request.ResetPersonalizationState(mWeb.ServerRelativeUrl, mFileServerRelativeUrl, new Guid(id));
            }
        }

        public void SetRestoreReport(IReport report)
        { }

        public IAveWebPart ImportAndAddWebPart(string webPartXml, string zoneId, int zoneIndex)
        {
            var webPartProperties = mRequest.ImportAndAddWebPart(mWeb.ServerRelativeUrl, mFileServerRelativeUrl, webPartXml, zoneId, zoneIndex);
            AveWebPart webPart = new AveWebPart(mRequest, mWeb, webPartProperties);
            return webPart;
        }
    }
}