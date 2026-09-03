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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Portal.WebControls;
using Microsoft.SharePoint.WebPartPages;
using SPDisposeCheck;
using UIWebPart = System.Web.UI.WebControls.WebParts.WebPart;
using Microsoft.SharePoint.Publishing.WebControls;
using Microsoft.SharePoint.Taxonomy;

namespace AvePoint.ObjectModel.ServerSE
{
    public class AveLimitedWebPartManager : IAveLimitedWebPartManager
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SPLimitedWebPartManager mSharedManager;
        //一个mPersonalManager对应一个user，reload需要使用
        private SPUser mPersonalManagerUser;
        //配合PersonalManager使用，初始化PersonalManager的时候，需要使用到mCheckoutWeb，以及初始化mPersonalManagerUser
        private SPWeb mCheckoutWeb;
        private SPLimitedWebPartManager mPersonalManager;
        //备份WebPartExtensionInfo的时候使用，仅在备份使用
        private SPLimitedWebPartManager webPartExtensionManager;
        private AveLimitedWebPartCollection mWebParts;
        private AveWeb mWeb;
        private AveSite mSite;
        private readonly bool hasFullControlPermission;
        private AveFile mFile;
        private XmlNode mWebPartTypes;
        private XmlNode mWebPartTypesInGallery;
        private string mFileServerRelativeUrl;
        private Dictionary<SPListTemplateType, string> mValidBaseViewIDCollection = new Dictionary<SPListTemplateType, string>();
        private AveWebPartCache mCache;
        private IReport mReport = null;
        private List<SPWebPartConnection> allConnections = new List<SPWebPartConnection>();

        public List<Tuple<Guid, bool>> NeedResaveListViewWebpart = new List<Tuple<Guid, bool>>();

        private SPWebPartManager mWebPartManager = null;
        internal SPWebPartManager WebPartManager
        {
            get
            {
                if (mWebPartManager == null)
                {
                    mWebPartManager = AveAssemblyUtility.GetPropertyValue(GetWebPartManager(true), "WebPartManager") as SPWebPartManager;
                }
                return mWebPartManager;
            }
        }

        internal bool HasFullControlPermission
        {
            get { return this.hasFullControlPermission; }
        }

        public bool NeedReloadList { get; private set; }

        public AveFile File
        {
            get { return mFile; }
            set { mFile = value; }
        }

        public IReport Report
        {
            get { return mReport; }
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "LimitedWebPartManager will be disposed by IAveLimitedWebPartManager")]
        public AveLimitedWebPartManager(IAveSite site, IAveWeb web, IAveFile file)
        {
            mSite = site as AveSite;
            hasFullControlPermission = mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
            mWeb = web as AveWeb;
            mFile = file as AveFile;
            mSharedManager = mFile.File.GetLimitedWebPartManager(PersonalizationScope.Shared);
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "LimitedWebPartManager will be disposed by IAveLimitedWebPartManager")]
        public AveLimitedWebPartManager(IAveWeb web, SPLimitedWebPartManager limitedWebPartManager)
        {
            mSharedManager = limitedWebPartManager;
            mWeb = web as AveWeb;
            mSite = mWeb.Site as AveSite;
            hasFullControlPermission = mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "LimitedWebPartManager will be disposed by IAveLimitedWebPartManager")]
        public AveLimitedWebPartManager(IAveWeb web, SPLimitedWebPartManager limitedWebPartManager, IAveFile file)
        {
            mSharedManager = limitedWebPartManager;
            mWeb = web as AveWeb;
            mSite = mWeb.Site as AveSite;
            hasFullControlPermission = mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
            mFile = file as AveFile;
        }

        public AveLimitedWebPartManager(IAveSite site, IAveWeb web, string fileServerRelativeUrl)
        {
            mSite = site as AveSite;
            hasFullControlPermission = mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
            mWeb = web as AveWeb;
            mFileServerRelativeUrl = fileServerRelativeUrl;
            mFile = mWeb.GetFile(fileServerRelativeUrl) as AveFile;
        }

        #region IAveLimitedWebPartManager Members

        public IAveLimitedWebPartCollection WebParts
        {
            get { return mWebParts ?? (mWebParts = new AveLimitedWebPartCollection(this, mSharedManager.WebParts)); }
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mSharedManager != null)
            {
                if (mSharedManager.Web != null)
                {
                    mSharedManager.Web.Dispose();
                }

                mSharedManager.Dispose();
                mSharedManager = null;
            }
            if (mPersonalManager != null)
            {
                if (mPersonalManager.Web != null)
                {
                    mPersonalManager.Web.Dispose();
                }
                mPersonalManager.Dispose();
                mPersonalManager = null;
            }
            if (webPartExtensionManager != null)
            {
                if (webPartExtensionManager.Web != null)
                {
                    webPartExtensionManager.Web.Dispose();
                }
                webPartExtensionManager.Dispose();
                webPartExtensionManager = null;
            }
            //不需要dispose，在AveSite中会释放对应的Web资源
            if (mCheckoutWeb != null)
            {
                //mCheckoutWeb.Dispose();
                //mCheckoutWeb = null;
                mPersonalManagerUser = null;
            }
        }

        #endregion

        #region IAveLimitedWebPartManager Members

        public void SetRestoreReport(IReport report)
        {
            this.mReport = report;
        }

        public void RestoreWebParts(IList webParts, bool clearAll)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveLimitedWebPartManager.RestoreWebParts"))
            {
                if (clearAll)
                {
                    this.ClearExsitingWebPart = clearAll;
                    DeleteAllWebParts(webParts);
                }
                //ADO-93010 在LimitedWebPartManager生成前,进行FakeSPContext，避免出现ErrorWebPart和导致SiteQuota表出现大量负值记录。
                //ADO-116971 在LimitedWebPartManager生成前,进行FakeSPContext一些特定情况下会引起进程Crash，改为在获取WebpartManager之后调用
                Web.HandleSPContext(
                    delegate ()
                    {
                        foreach (Object webPartInfoOrId in webParts)
                        {
                            string webPartIdentity = string.Empty;
                            try
                            {
                                if (webPartInfoOrId is AveWebPartBaseInfo)
                                {
                                    AveWebPartBaseInfo webPartInfo = webPartInfoOrId as AveWebPartBaseInfo;
                                    //view还原失败的时候，WebPart也过滤不还原
                                    if (webPartInfo.ID == Guid.Empty)
                                    {
                                        continue;
                                    }
                                    webPartIdentity = webPartInfo.ID.ToString();
                                    NeedReloadList |= RestoreWebPart(webPartInfo);
                                }
                                //在PostAction中Restore的WebPart，一般只是调用了一次UpdateWebPartByType的方法，
                                //但WebPartBaseInfo是null，在写对应的Updater方法的时候需要注意
                                else if (webPartInfoOrId is AveWebPartPostActionInfo)
                                {
                                    AveWebPartPostActionInfo info = webPartInfoOrId as AveWebPartPostActionInfo;
                                    webPartIdentity = info.WebPartId.ToString();
                                    NeedReloadList |= RestoreWebPart(info);
                                }
                                else
                                {
                                    logger.Log(AveLogLevel.WARN, "Error Type WebPartInfo. Type:{0}", webPartInfoOrId.GetType());
                                    continue;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, ServerAPIResource.WebpartRestoreFailed, webPartIdentity, e);
                            }
                        }
                    }
                    );
                //ADO-224768 HandleSPContext 内save webpart 对于 list view webpart的InplaceSearchEnable=true的情况不生效
                ReSaveListViewWebPartForInplaceSearchEnableProperty();
                RestoreWebPartConnections();
            }
        }
        private void ReSaveListViewWebPartForInplaceSearchEnableProperty()
        {
            logger.Debug("Start to resave list view webpart for inplace search enable property, webpart count: {0}", NeedResaveListViewWebpart.Count);
            bool isFirst = true;
            foreach (var webpartTuple in NeedResaveListViewWebpart)
            {
                try
                {
                    var webpart = isFirst ? this.ReloadWebPart(webpartTuple.Item1, webpartTuple.Item2) : this.GetWebPart(webpartTuple.Item1, webpartTuple.Item2);
                    this.SaveChanges(webpart, webpartTuple.Item2);
                    isFirst = false;
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while resave list view webpart, error: {0}", e);
                }
            }
        }

        //Restore WebPart in post action
        private bool RestoreWebPart(AveWebPartPostActionInfo info)
        {
            bool needReloadList = false;
            bool reload = false;
            System.Web.UI.WebControls.WebParts.WebPart webPart = null;
            bool shared = true;
            if (info.UserId > 0)
            {
                shared = false;
                InitPersonalManagerByUser(info.UserId);
            }
            webPart = GetWebPart(info.WebPartId, shared);

            if (webPart != null)
            {
                AveWebPart aveWebPart = new AveWebPart(this, webPart, info.UserId);
                reload = false;
                if (aveWebPart.UpdateWebPartByType(false, out reload))
                {
                    GetWebPartManager(shared).SaveChanges(webPart);
                }
                needReloadList |= reload;
            }
            else
            {
                /*添加处理替换file version中webpart属性的逻辑，
                          由于不能使用API去更新file version上的webpart，所以此处使用的逻辑为：
                          先获取file version上对于的webpart，然后将属性更新到这个webpart上，
                          将这个webpart添加到当前version上，然后使用SQL将这个webpart的属性move到file version上的webpart上，再删除该新加的webpart
                        */
                if (this.mSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Log(AveLogLevel.WARN, "Skip to restore WebPart in version because of permission issue.");
                    return false;
                }

                if (info.WebPartId != Guid.Empty)
                {
                    foreach (SPFileVersion version in mFile.File.Versions)
                    {
                        SPLimitedWebPartManager tempManager = GetWebPartManager(true, version);
                        if (tempManager == null)
                        {
                            continue;
                        }
                        foreach (var tempWebPart in tempManager.WebParts)
                        {
                            Microsoft.SharePoint.WebPartPages.WebPart spWebPart = tempWebPart as Microsoft.SharePoint.WebPartPages.WebPart;
                            if (spWebPart != null && spWebPart.StorageKey == info.WebPartId)
                            {
                                webPart = spWebPart;
                                break;
                            }
                        }
                        if (tempManager.Web != null)
                        {
                            tempManager.Web.Dispose();
                        }
                        tempManager.Dispose();
                        if (webPart != null)
                        {
                            break;
                        }
                    }
                    if (webPart == null)
                    {
                        logger.Warn("The web part not exist in file versions. File Url: {0}, Web Part ID: {1}", mFile.Url, info.WebPartId);
                        return needReloadList;
                    }
                    //reset webpart id to null avoid duplicate id issue.
                    webPart.ID = null;
                    //如果是在WikiPageLibrary下的话，default zone应该是"wpz"
                    this.AddWebPart(webPart, "Main", int.MaxValue - 370);
                    reload = false;
                    AveWebPart aveWebPart = new AveWebPart(this, webPart, -1);
                    aveWebPart.UpdateWebPartByType(false, out reload);
                    needReloadList |= reload;
                    GetWebPartManager(true).SaveChanges(webPart);
                    UpdateVersionWebPart(info.WebPartId, webPart);
                }
            }

            return needReloadList;
        }

        private bool RestoreWebPart(AveWebPartBaseInfo webPartInfo)
        {
            bool isView = this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(webPartInfo.ID);
            AveWebPart webPart = null;
            if (isView)
            {
                if (this.Cache.ViewInfo.Views[webPartInfo.ID] == Guid.Empty)
                {
                    return false;
                }
                webPart = new AveSPViewWebPart(this);
            }
            else
            {
                webPart = new AveWebPart(this);
            }
            webPart.WebPartInfo = webPartInfo;
            return webPart.RealRestore();
        }

        private void RestoreWebPartConnections()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveLimitedWebPartManager.RestoreWebPartConnection"))
            {
                try
                {
                    Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>> allConnectionsCache = this.Cache.SiteMappingManager.GetUnRestoreWebPartConnectionCache();
                    Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>> webPartList = new Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>();
                    if (allConnectionsCache.TryGetValue(this.File.ParentFolder.ParentListId, out webPartList))
                    {
                        Dictionary<string, Dictionary<object, List<string>>> webPartWeb = new Dictionary<string, Dictionary<object, List<string>>>();
                        if (webPartList != null && webPartList.Count > 0 && webPartList.TryGetValue(this.Web.ID, out webPartWeb))
                        {
                            Dictionary<object, List<string>> webPartConnection = new Dictionary<object, List<string>>();
                            if (webPartWeb != null && webPartWeb.Count > 0 && webPartWeb.TryGetValue(this.File.ServerRelativeUrl, out webPartConnection)
                                && webPartConnection != null && webPartConnection.Count > 0)
                            {

                                logger.Log(AveLogLevel.DEBUG, "Start to restore web part connections. File Url: {0}.", this.File.ServerRelativeUrl);
                                //ADO-160519 listviewwebpart updateview后需要重新reload下,否则Connection后会少viewfield
                                ReloadBeforeConnection();
                                if (System.Web.HttpContext.Current != null)
                                {
                                    System.Web.HttpContext.Current = null;
                                }

                                List<SPWebPartConnection> cons = new List<SPWebPartConnection>();
                                foreach (var temp in webPartConnection.Keys)
                                {
                                    if (temp as SPWebPartConnection != null)
                                    {
                                        cons.Add(temp as SPWebPartConnection);
                                    }
                                }
                                foreach (SPWebPartConnection con in cons)
                                {
                                    try
                                    {
                                        //mSharedManager.SPWebPartConnections.Add(con);
                                        UIWebPart provider = null;
                                        UIWebPart consumer = null;
                                        ProviderConnectionPoint providerPoint = null;
                                        ConsumerConnectionPoint consumerPoint = null;
                                        try
                                        {
                                            provider = mSharedManager.WebParts[con.ProviderID];
                                            consumer = mSharedManager.WebParts[con.ConsumerID];
                                            providerPoint = mSharedManager.GetProviderConnectionPoints(provider)[con.ProviderConnectionPointID];
                                            consumerPoint = mSharedManager.GetConsumerConnectionPoints(consumer)[con.ConsumerConnectionPointID];
                                        }
                                        catch
                                        {
                                            logger.Debug("Restore Webpart Connection failed, we should add it to post action!");
                                            this.Cache.SiteMappingManager.AddUnResotreWebPartConnectionInfo(this.Web.ID, this.File.ParentFolder.ParentListId, this.File.ServerRelativeUrl, con, con.ProviderID, con.ConsumerID);
                                            throw;
                                        }
                                        mSharedManager.SPConnectWebParts(provider, providerPoint, consumer, consumerPoint, con.Transformer);
                                        //如果从Cache中获取到了Connections，并且还原成功，从Cache中Remove掉这个Connection，免得重复还原
                                        this.Cache.SiteMappingManager.RemoveUnResotreWebPartConnectionInfoFromCache(this.Web.ID, this.File.ParentFolder.ParentListId, this.File.ServerRelativeUrl, con, con.ProviderID, con.ConsumerID);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Failed to restore web part connection. Consumer ID: {0}. Provider ID: {1}. Error: {2}", con.ConsumerID, con.ProviderID, ex);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while restoring the web part connection. Error: {0}", ex);
                }
            }
        }

        internal void AddWebPartConnections(string serializedStr)
        {
            try
            {
                ArrayList spConnections = AveWebPartUtility.DeserializeWebPartConnection(serializedStr) as ArrayList;
                if (spConnections != null && spConnections.Count == 2)
                {
                    AddWebPartConnections(spConnections[1]);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Failed to deserialize web part connection string. Error:{0}", ex);
            }
        }

        internal void AddWebPartConnections(object data)
        {
            try
            {
                object[] connectionsDetails = AveAssemblyUtility.GetFieldValue(data, "m_spDynamicConnectionState") as object[];
                if (connectionsDetails == null)
                {
                    throw new Exception("Failed to get web part connection details.");
                }
                else
                {
                    int conCount = connectionsDetails.Length / 12;
                    for (int i = 0; i < conCount; i++)
                    {
                        SPWebPartConnection connection = ConstructWebPartConnection(i, connectionsDetails);
                        if (connection != null)
                        {
                            this.Cache.SiteMappingManager.AddUnResotreWebPartConnectionInfo(this.Web.ID, this.File.ParentFolder.ParentListId, this.File.ServerRelativeUrl, connection, connection.ProviderID, connection.ConsumerID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, "An error occurred while adding web part connections. Error: {0}", ex.ToString());
            }
        }

        private SPWebPartConnection ConstructWebPartConnection(int startIndex, object[] data)
        {
            try
            {
                var tempIndex = startIndex * 12;
                SPWebPartConnection con = new SPWebPartConnection
                {
                    ID = data[tempIndex].ToString(),
                    ConsumerID = data[tempIndex + 1].ToString(),
                    ConsumerConnectionPointID = data[tempIndex + 2].ToString(),
                    ProviderID = data[tempIndex + 3].ToString(),
                    ProviderConnectionPointID = data[tempIndex + 4].ToString(),
                    CrossPageConnectionID = data[tempIndex + 7].ToString(),
                    SourcePageUrl = data[tempIndex + 8].ToString(),
                    TargetPageUrl = data[tempIndex + 9].ToString(),
                    CrossPageSchema = data[tempIndex + 10].ToString()
                };
                if ((data[tempIndex + 5] as Type) != null)
                {
                    WebPartTransformer transformer = Activator.CreateInstance(data[tempIndex + 5] as Type) as WebPartTransformer;
                    AveAssemblyUtility.InvokeMethod(transformer, "LoadConfigurationState", new object[] { data[tempIndex + 6] });
                    AveAssemblyUtility.InvokeMethod(con, "SetTransformer", new object[] { transformer });
                }

                return con;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, "Failed to construct a web part connection. Error: {0}", ex.ToString());
                return null;
            }
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        private SPLimitedWebPartManager GetWebPartManager(bool isShared, SPFileVersion fileVersion)
        {
            try
            {
                Uri pageuri = new Uri(mWeb.Web.Url.TrimEnd('/') + "/" + mFile.File.Url.TrimStart('/') + "?PageVersion=" + fileVersion.ID);

                Assembly ass = Assembly.Load(new AssemblyName("Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                Type type1 = ass.GetType("Microsoft.SharePoint.WebPartPages.PageView");
                FieldInfo[] fields = type1.GetFields();
                Type type = ass.GetType("Microsoft.SharePoint.SPWeb");
                MethodInfo method = type.GetMethod("GetLimitedWebPartManagerInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Uri), typeof(int), type1, typeof(bool), typeof(bool) }, null);
                object result = method.Invoke(mWeb.Web, new object[] { pageuri, fileVersion.ID, fields[1].GetValue(0), false, true });
                return result as Microsoft.SharePoint.WebPartPages.SPLimitedWebPartManager;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while GetWebPartManager, File Url: {0}, version: {1}, error: {2}.", mFile.File.Url, fileVersion.ID, e.ToString());
                return null;
            }
        }
        private void UpdateVersionWebPart(Guid webPartId, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            if (webPart != null)
            {
                Microsoft.SharePoint.WebPartPages.WebPart spWebPart = webPart as Microsoft.SharePoint.WebPartPages.WebPart;
                Guid webPartGuid = Guid.Empty;
                if (spWebPart != null)
                {
                    webPartGuid = spWebPart.StorageKey;
                    MoveWebPartProperty(webPartGuid, webPartId);
                }
            }
        }

        internal void MoveWebPartProperty(Guid fromWebPartId, Guid toWebPartId)
        {
            try
            {
                mSite.QueryService.MoveWebPartProperty(this.mSite.ID, this.mFile.UniqueId, fromWebPartId, toWebPartId);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while do MoveWebPartPropertyTo. FromWebpartId: {0}, ToWebpartId: {1}.", fromWebPartId, toWebPartId);
                logger.Warn("Exception: {0}", e.ToString());
            }
        }

        private void InitPersonalManagerByUser(SPUser user, SPList list = null)
        {
            try
            {
                if (mPersonalManager != null && mPersonalManagerUser != null && mPersonalManagerUser.UserToken.CompareUser(user.UserToken))
                {
                    return;
                }
                //List应该传进去check权限
                if (list == null)
                {
                    //没有ParentList
                    if (mFile.File.ParentFolder.ParentListId != Guid.Empty)
                    {
                        list = mWeb.Web.Lists[mFile.File.ParentFolder.ParentListId];
                    }
                }
                mCheckoutWeb = mSite.GetCheckoutWeb(mWeb.Web, list, ref user, Guid.Empty);
                if (mPersonalManagerUser == null || !mPersonalManagerUser.UserToken.CompareUser(user.UserToken))
                {
                    mPersonalManager = mCheckoutWeb.GetFile(mFile.File.UniqueId).GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.User);
                    mPersonalManagerUser = user;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //当开启ForceCheckout，还原第一个Version的时候，此时文件还是Checkout状态，Get文件会提示文件不存在的异常
                //这样会把PersonalWebPart还原到Agent Account上
                logger.Log(AveLogLevel.WARN, "Failed to get personal web part manager. Error:{0}", ex);
            }
        }

        private void InitPersonalManagerByUser(int userId, SPList list = null)
        {
            SPUser user = mWeb.Web.SiteUsers.GetByID(userId);
            InitPersonalManagerByUser(user, list);
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "Ignoring this error")]
        internal SPView GetPersonalView(SPList list, Guid viewId, int userId)
        {
            InitPersonalManagerByUser(userId, list);
            try
            {
                SPView view = mCheckoutWeb.Lists[list.ID].Views[viewId];
                return view;
            }
            catch (Exception ex)
            {
                logger.Debug(ServerAPIResource.PersonalViewWebPartGetError, ex);
            }
            return null;
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        public UIWebPart ReloadWebPart(string id, bool isShared)
        {
            if (String.IsNullOrEmpty(id))
            {
                return null;
            }
            Dispose();
            Reload();
            return GetWebPart(id, isShared);
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        public UIWebPart ReloadWebPart(Guid id, bool isShared)
        {
            if (id == Guid.Empty)
            {
                return null;
            }
            Dispose();
            Reload();
            return GetWebPart(id, isShared);
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        private void DeleteAllWebParts(IList webParts)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveLimitedWebPartManager.DeleteAllWebParts"))
            {
                SPLimitedWebPartManager manager = GetWebPartManager(true);
                List<System.Web.UI.WebControls.WebParts.WebPart> wpList = new List<System.Web.UI.WebControls.WebParts.WebPart>();
                foreach (System.Web.UI.WebControls.WebParts.WebPart part in manager.WebParts)
                {
                    try
                    {
                        if (Cache.ViewInfo != null)
                        {
                            XsltListViewWebPart xslPart = part as XsltListViewWebPart;
                            if (xslPart != null && Cache.ViewInfo.Views.ContainsValue(new Guid(xslPart.ViewGuid)))
                            {
                                continue;
                            }
                            if (xslPart == null)
                            {
                                ListViewWebPart viewPart = part as ListViewWebPart;
                                if (viewPart != null && Cache.ViewInfo.Views.ContainsValue(new Guid(viewPart.ViewGuid)))
                                {
                                    continue;
                                }
                            }
                        }
                        wpList.Add(part);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("List web part from a file: {0} failed: {1}", mFile.File.Url, e.ToString());
                    }
                }

                foreach (System.Web.UI.WebControls.WebParts.WebPart part in wpList)
                {
                    try
                    {
                        manager.DeleteWebPart(part);
                    }
                    catch (Exception e)
                    {
                        Guid webPartId = GetStorageKey(part, true);
                        if (webPartId == Guid.Empty)
                        {
                            continue;
                        }
                        // 使用反射内部的delete来代替使用数据库删除。当使用数据库的时候由于和api操作数据库的session不同，会导致添加到sitequota表中的数据在add webpart之后还存在负值。
                        InternalDelete(part);
                        //if (mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                        //{
                        //    DeleteWebPartByNative(mSite.ID, mFile.UniqueId, webPartId);
                        //}
                        logger.Warn("An error occurred when delete Web Part: {0} from file: {1}, error: {2}", part.ID, mFile.File.Url, e.ToString());
                    }
                }
                DeleteAllPersonalWebParts(mSite.ID, mFile.UniqueId, (int)mFile.Level, Cache.ViewInfo.Views.Values.ToList<Guid>());
            }
        }

        private void InternalDelete(System.Web.UI.WebControls.WebParts.WebPart webpart)
        {
            var method = typeof(SPWebPartManager).GetMethod("DeleteWebPartInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(WebPartManager, new object[] { webpart });
            //internalManager.DeleteWebPart(webpart);
        }

        internal string ReplaceAudienceId(string oldValue)
        {
            var audienceIDMapping = Cache.SiteMappingManager.GetAudienceIDMappingForWebPart();
            if (string.IsNullOrEmpty(oldValue) || audienceIDMapping.Count == 0)
            {
                return oldValue;
            }
            int index = oldValue.IndexOf(";;;;", StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                return oldValue;
            }
            string newValue = oldValue;
            oldValue = oldValue.Substring(0, index);
            string[] tValues = oldValue.Split(',');
            foreach (string tValue in tValues)
            {
                string value;
                if (audienceIDMapping.TryGetValue(tValue, out value))
                {
                    newValue = newValue.Replace(tValue, value);
                }
            }
            return newValue;
        }

        internal string GetValidBaseViewIdStr(SPList list)
        {
            string validBaseViewIdStr = string.Empty;
            if (mValidBaseViewIDCollection.ContainsKey(list.BaseTemplate))
            {
                validBaseViewIdStr = mValidBaseViewIDCollection[list.BaseTemplate];
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    string unCustomizedViewSchema = list.GetPropertiesXmlForUncustomizedViews();
                    if (!string.IsNullOrEmpty(unCustomizedViewSchema))
                    {
                        doc.LoadXml(unCustomizedViewSchema);
                        StringBuilder sb = new StringBuilder("|");
                        foreach (XmlNode xd in doc.DocumentElement.ChildNodes)
                        {
                            if (xd is XmlElement)
                            {
                                XmlElement viewProperties = (XmlElement)xd;
                                if (viewProperties.HasAttribute("BaseViewID"))
                                {
                                    sb.Append(viewProperties.GetAttribute("BaseViewID"));
                                }
                                sb.Append("|");
                            }
                        }
                        validBaseViewIdStr = sb.ToString();
                        mValidBaseViewIDCollection.Add(list.BaseTemplate, validBaseViewIdStr);
                    }
                    else
                    {
                        //mLog.Warn("UncustomizedViews is null..");
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BaseViewIdGetFailed, e);
                }
                finally
                {
                    doc.RemoveAll();
                }
            }
            return validBaseViewIdStr;
        }

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        private SPLimitedWebPartManager GetWebPartManager(bool isShared)
        {
            try
            {
                if (isShared)
                {
                    return mSharedManager ?? (mSharedManager = mFile.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared));
                }
                return mPersonalManager ?? (mPersonalManager = mFile.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.User));
            }
            catch (Exception spEx)//need check SPException
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.WebpartManagerGetFailed, mFileServerRelativeUrl == null ? string.Empty : mFileServerRelativeUrl, spEx);
                Reload();
                if (isShared)
                {
                    return mSharedManager ?? (mSharedManager = mFile.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared));
                }
                return mPersonalManager ?? (mPersonalManager = mFile.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.User));
            }
        }

        private void Reload()
        {
            mWeb.Web.AllowUnsafeUpdates = true;
            mSite.Site.AllowUnsafeUpdates = true;
            mFile.Reload();
        }

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "mSharedManager will be disposed in Dispose() method")]
        private void ReloadBeforeConnection()
        {
            Dispose();
            Reload();
            //ADO-80000,mSharedManager在此处可能为空，需要Reload
            if (mSharedManager == null)
            {
                mSharedManager = GetWebPartManager(true);
            }
        }

        public List<AveWebPartBaseInfo> GetWebParts(AveBaseItemInfo info)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveLimitedWebPartManager.GetWebParts"))
            {
                List<AveWebPartBaseInfo> webpartBaseInfoListFromDB = mSite.QueryService.GetWebParts(info.SiteId, info.GUID, (byte)info.Level, info.PageVersion, info.Version);
                if (webpartBaseInfoListFromDB != null && webpartBaseInfoListFromDB.Count > 0)
                {
                    foreach (AveWebPartBaseInfo webPartInfo in webpartBaseInfoListFromDB)
                    {
                        try
                        {
                            webPartInfo.ListTitle = GetListTitle(info, webPartInfo.ListId);
                            mSite.QueryService.SetWebPartPersonalization(webPartInfo, info.SiteId, info.GUID);
                            mSite.QueryService.SetWebPartLists(webPartInfo, info.SiteId, info.GUID, (byte)info.Level);
                            if (WrapperConfiguration.BackupWebPartPropertiesAsDic)
                            {
                                if (webPartInfo.AllUsersProperties != null || webPartInfo.PerUserProperties != null)
                                {
                                    int resultCode = 0;
                                    webPartInfo.DicAllUserPerUserPros = AveWebPartUtility.GetProperties(webPartInfo.AllUsersProperties, webPartInfo.PerUserProperties, out resultCode);
                                    webPartInfo.AllUsersProperties = null;
                                    webPartInfo.PerUserProperties = null;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.WebpartGetByNativeError, e);
                        }
                    }
                }
                ///下面的代码是为了备份DefinitionXml，目前Restore没有用到这个信息，通过API备份有效率问题，
                ///SetWebPartDefinitionXml这个方法完全是根据已备份的信息生成的，因此，不用暂时不备份也不用担心信息丢失。
                ///目前5.7以及5Trunk上都没有下面这些代码，还没有出过客户问题，因此暂不备份。
                //if (webpartBaseInfoListFromDB != null)
                //{
                //    SetWebPartDefinitionXml(webpartBaseInfoListFromDB);
                //}
                //backup webpart definition for BPOS restore
                if (WrapperRuntime.CurrentContext.BackupWebpartPropertiesForOffice365 && webpartBaseInfoListFromDB != null)
                {
                    List<AveWebPartBaseInfo> webpartBaseInfoListFromAPI = GetWebPartsProperties(info);
                    MergeWebPartsBaseInfo(webpartBaseInfoListFromDB, webpartBaseInfoListFromAPI);
                }
                if (webpartBaseInfoListFromDB != null && webpartBaseInfoListFromDB.Count > 0)
                {
                    GetWebPartExtensions(info, webpartBaseInfoListFromDB);
                }
                return webpartBaseInfoListFromDB;
            }
        }

        private void GetWebPartExtensions(AveBaseItemInfo baseItemInfo, List<AveWebPartBaseInfo> webPartBaseInfos)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveLimitedWebPartManager.GetWebPartExtensions"))
            {
                foreach (AveWebPartBaseInfo webPartInfo in webPartBaseInfos)
                {
                    try
                    {
                        string webPartTypeName = GetWebPartTypeName(baseItemInfo, webPartInfo, webPartInfo.WebPartTypeId);
                        AveWebPartExtensionHandler extensionHandler = AveWebPartExtensionHandler.GetWebPartExtensionHandler(webPartTypeName, this);
                        if (extensionHandler != null)
                        {
                            if (webPartExtensionManager == null)
                            {
                                try
                                {
                                    if (baseItemInfo.Version == File.UIVersion)
                                    {
                                        webPartExtensionManager = mFile.File.GetLimitedWebPartManager(PersonalizationScope.Shared);
                                    }
                                    else
                                    {
                                        SPFileVersion fileVersion = mFile.File.Versions.GetVersionFromID(baseItemInfo.Version);
                                        if (fileVersion != null)
                                        {
                                            webPartExtensionManager = fileVersion.GetLimitedWebPartManager();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log(AveLogLevel.DEBUG, "Cannot get the webpart manager while trying to get the web part extensions. File Url: {0}, version: {1}, exception: {2}", mFile.File.Url, baseItemInfo.Version, ex);
                                    break;
                                }
                            }

                            Dictionary<string, string> extensionProperties = extensionHandler.GetWebPartExtensionInfo(webPartExtensionManager.WebParts[webPartInfo.ID]);
                            if (extensionProperties.Count > 0)
                            {
                                webPartInfo.ExtensionProperties = extensionProperties;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.DEBUG, "Cannot get web part extension info. File Url: {0}. Web Part ID: {1}. Error: {2}", mFile.File.Url, webPartInfo.ID, ex);
                    }
                }
            }
        }

        private string GetWebPartTypeName(AveBaseItemInfo itemInfo, AveWebPartBaseInfo webPartInfo, Guid webPartTypeId)
        {
            string typeName = string.Empty;
            string tmpName = string.Empty;
            if (itemInfo.MappingManager.BackupMappingManager.WebPartTypeIDMapping.ContainsKey(webPartTypeId))
            {
                string[] names = itemInfo.MappingManager.BackupMappingManager.WebPartTypeIDMapping[webPartTypeId].Split('|');
                if (names.Length == 2)
                {
                    tmpName = names[1];
                }
            }
            else if (!string.IsNullOrEmpty(webPartInfo.Class))
            {
                tmpName = webPartInfo.Class;
            }

            if (!string.IsNullOrEmpty(tmpName))
            {
                string[] namespaces = tmpName.Split('.');
                if (namespaces.Length > 0)
                {
                    typeName = namespaces[namespaces.Length - 1];
                }
            }
            return typeName;
        }

        private string GetListTitle(AveBaseItemInfo info, Guid listId)
        {
            string title = string.Empty;
            try
            {
                if (info.MappingManager.BackupMappingManager.ListIdTitleMapping.ContainsKey(listId))
                    title = info.MappingManager.BackupMappingManager.ListIdTitleMapping[listId];
                else
                {
                    title = mSite.QueryService.GetListTitle(mSite.ID, listId);
                    info.MappingManager.BackupMappingManager.ListIdTitleMapping.Add(listId, title);
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.ListTitleGetFailed, listId, e);
            }

            return title;
        }
        #endregion

        #region Get WebPart DefinitionXml From DB

        public string GetViewType(int iType)
        {
            if ((iType & 0x80000) > 0)
            {
                return "Calendar";
            }
            if ((iType & 0x20000) > 0)
            {
                return "Chart";
            }
            if ((iType & 0x4000000) > 0)
            {
                return "Gantt";
            }
            if ((iType & 0x2001) >= 0x2001)
            {
                return "Recurrence";
            }
            if ((iType & 0x800) > 0)
            {
                return "Grid";
            }
            return "Html";
        }

        private bool IsCompressedXML(byte[] perUserProperties, byte[] allUserProperties)
        {
            if ((((perUserProperties == null) || (perUserProperties.Length < 2)) || ((perUserProperties[0] != 1) || (perUserProperties[1] != 5))) && (((allUserProperties == null) || (allUserProperties.Length < 2)) || ((allUserProperties[0] != 1) || (allUserProperties[1] != 5))))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region API GetWebPartDefinitionXml

        internal void MergeWebPartsBaseInfo(List<AveWebPartBaseInfo> webpartBaseInfoListFromDB, List<AveWebPartBaseInfo> webpartBaseInfoListFromAPI)
        {
            try
            {
                foreach (AveWebPartBaseInfo webpartBaseInfoFromDB in webpartBaseInfoListFromDB)
                {
                    foreach (AveWebPartBaseInfo webpartBaseInfoFromAPI in webpartBaseInfoListFromAPI)
                    {
                        if (webpartBaseInfoFromDB.ID == webpartBaseInfoFromAPI.ID)
                        {
                            webpartBaseInfoFromDB.DefinitionXml = webpartBaseInfoFromAPI.DefinitionXml;
                            webpartBaseInfoFromDB.XmlDefinition = webpartBaseInfoFromAPI.XmlDefinition;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("MergeWebPartsBaseInfo error: {0}", ex.ToString());
            }
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "This Web will be Disposed by AveWeb")]
        internal List<AveWebPartBaseInfo> GetWebPartsProperties(AveBaseItemInfo baseItemInfo)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveLimitedWebPartManager.GetWebPartsProperties"))
            {
                List<AveWebPartBaseInfo> webpartColProperties = new List<AveWebPartBaseInfo>();
                try
                {
                    if (mSharedManager == null)
                    {
                        try
                        {
                            //If the file only have a checkout version and the checkout user is not agent account.
                            if (mFile == null || mFile.File == null || !mFile.File.Exists)
                            {
                                logger.Log(AveLogLevel.WARN, "Cannot get file while exporting WebPart for O365.");
                                return webpartColProperties;
                            }
                            if (baseItemInfo.Version == mFile.File.UIVersion)
                            {
                                mSharedManager = mFile.File.GetLimitedWebPartManager(PersonalizationScope.Shared);
                            }
                            //For Checkout User
                            else if (baseItemInfo.Level == 255 && mFile.File.CheckedOutByUser != null && mFile.File.CheckedOutByUser.ID != mSite.RootWeb.CurrentUser.ID)
                            {
                                using (var checkouWeb = mSite.GetCheckoutWeb(mSite.ID, mFile.Web, mFile.ParentFolder.ParentList, mFile.CheckedOutByUser, mFile.UniqueId, true))
                                {
                                    var checkoutFile = checkouWeb.GetFile(mFile.ServerRelativeUrl) as AveFile;
                                    if (baseItemInfo.Version == checkoutFile.File.UIVersion)
                                    {
                                        mSharedManager = checkoutFile.File.GetLimitedWebPartManager(PersonalizationScope.Shared);
                                    }
                                }
                            }
                            else
                            {
                                SPFileVersion fileVersion = mFile.File.Versions.GetVersionFromID(baseItemInfo.Version);
                                if (fileVersion != null)
                                {
                                    mSharedManager = fileVersion.GetLimitedWebPartManager();
                                }
                                else
                                {
                                    logger.Warn("Can not get file version by verion id, file url: {0}, file version id: {1}", mFile.ServerRelativeUrl, baseItemInfo.Version);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Cannot get file WebPartManager. Error: {0}", e.ToString());
                            //logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e.ToString());
                        }
                    }
                    if (mSharedManager != null)
                    {
                        SPLimitedWebPartCollection limitedWebPartCol = mSharedManager.WebParts;
                        foreach (UIWebPart webpart in limitedWebPartCol)
                        {
                            AveWebPartBaseInfo webpartProperties = GetWebPartProperties(mSharedManager, webpart);
                            webpartColProperties.Add(webpartProperties);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Get webpart properties error. {0}", ex.ToString());
                }
                return webpartColProperties;
            }
        }

        //private SPFile GetCheckoutFile(SPUser checkoutUser)
        //{
        //    if (checkoutUser == null)
        //    {
        //        return null;
        //    }
        //    mCheckoutSite = new SPSite(mSite.ID, checkoutUser.UserToken);
        //    mCheckoutWeb = mCheckoutSite.OpenWeb(mWeb.ID);
        //    return mCheckoutWeb.GetFile(mFileServerRelativeUrl);
        //}

        internal AveWebPartBaseInfo GetWebPartProperties(SPLimitedWebPartManager webpartManager, UIWebPart webpart)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveLimitedWebPartManager.GetWebPartProperties"))
            {
                AveWebPartBaseInfo webpartProperties = new AveWebPartBaseInfo();
                try
                {
                    bool isWebPartOnPage;
                    var webPartDefinition = ExportWebPart(webpartManager, webpart, out isWebPartOnPage);
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(webPartDefinition);
                    XmlNode firstChild = null;
                    if (document.ChildNodes.Count == 1)
                    {
                        firstChild = document.FirstChild;
                    }
                    else if (document.ChildNodes.Count > 1)
                    {
                        firstChild = document.ChildNodes[1];
                    }
                    if (firstChild != null)
                    {
                        if (firstChild.Attributes["ID"] == null)
                        {
                            webpartProperties.ID = webpartManager.GetStorageKey(webpart);
                            XmlAttribute node = document.CreateAttribute("ID");
                            node.Value = webpartProperties.ID.ToString("D");
                            firstChild.Attributes.Append(node);
                        }
                        else
                        {
                            Guid tmpID = Guid.Empty;
                            if (Guid.TryParse(firstChild.Attributes["ID"].ToString(), out tmpID))
                            {
                                webpartProperties.ID = tmpID;
                            }
                            else
                            {
                                logger.Warn("Fail to convert the value of webPart id in export xml to guid. Value: {0}", firstChild.Attributes["ID"].ToString());
                                webpartProperties.ID = webpartManager.GetStorageKey(webpart);
                            }
                        }
                        XmlElement newChild = document.CreateElement("ZoneID");
                        if (isWebPartOnPage)
                        {
                            newChild.InnerText = webpartManager.GetZoneID(webpart);
                        }
                        XmlElement element2 = document.CreateElement("PartOrder");
                        element2.InnerText = webpart.ZoneIndex.ToString();
                        firstChild.AppendChild(newChild);
                        firstChild.AppendChild(element2);
                        XmlElement element3 = document.CreateElement("IsIncluded");
                        bool flag = !webpart.IsClosed;
                        element3.InnerText = flag.ToString().ToLower(CultureInfo.InvariantCulture);
                        firstChild.AppendChild(element3);
                        //webpartProperties.WebPartIdProperty = webpart.ID;
                        webpartProperties.DefinitionXml = firstChild.OuterXml;
                        //ADO-160937 Access Requests List需要还原该属性
                        var part = webpart as DataFormWebPart;
                        if (part != null)
                        {
                            webpartProperties.XmlDefinition = part.XmlDefinition;
                        }

                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetXmlAttError, e.ToString());
                }
                return webpartProperties;
            }
        }

        #region Export WebPart Definition

        private string ExportWebPart(SPLimitedWebPartManager limitedWpManager, UIWebPart wp, out bool isWebPartOnPage)
        {
            string webPartDefinition;
            try
            {
                isWebPartOnPage = (bool)AveAssemblyUtility.InvokeMethod(WebPartManager, "IsWebPartOnPage", new[] { typeof(UIWebPart) }, wp);

                if (wp.ExportMode != WebPartExportMode.None && isWebPartOnPage)
                {
                    webPartDefinition = ExportWebPartByAPI(limitedWpManager, wp);
                }
                else
                {
                    webPartDefinition = ExportWebPartByNativeAPI(limitedWpManager, wp);
                }
            }
            catch (Exception exception)
            {
                throw new AveWrapperBaseException(exception, AveInternalResourceKey.Wrapper_Exception_Server_ExportWebPartError, limitedWpManager.ServerRelativeUrl, exception.Message);
            }
            return webPartDefinition;
        }

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        private string ExportWebPartByAPI(SPLimitedWebPartManager limitedWebPartManager, UIWebPart webPart)
        {
            var webPartStringBuilder = new StringBuilder(0x400);
            var webPartTextWriter = new XmlTextWriter(new StringWriter(webPartStringBuilder));
            try
            {
                if (!ExportWebPartByAPIInternal(limitedWebPartManager, webPart, webPartTextWriter))
                {
                    logger.Debug(ServerAPIResource.ExportWebPartFakeContext, webPart.Title);
                    Web.HandleSPContext(() =>
                    {
                        webPartTextWriter.Flush();
                        webPartTextWriter.Close();
                        webPartStringBuilder.Clear();
                        webPartTextWriter = new XmlTextWriter(new StringWriter(webPartStringBuilder));
                        ExportWebPartByAPIInternal(limitedWebPartManager, webPart, webPartTextWriter);
                    });
                }
            }
            finally
            {
                webPartTextWriter.Flush();
                webPartTextWriter.Close();
            }
            return webPartStringBuilder.ToString();
        }

        private static bool ExportWebPartByAPIInternal(SPLimitedWebPartManager limitedWebPartManager, UIWebPart webPart, XmlTextWriter webPartWriter)
        {
            var success = true;
            try
            {
                limitedWebPartManager.ExportWebPart(webPart, webPartWriter);
            }
            catch (Exception e)
            {
                success = false;
                logger.Debug(ServerAPIResource.ExportWebPartError, e);
            }
            return success;
        }

        private static string ExportWebPartByNativeAPI(SPLimitedWebPartManager limitedWpManager, UIWebPart wp)
        {
            PropertyInfo propertyInfo = limitedWpManager.GetType().GetProperty("WebPartManager", BindingFlags.NonPublic | BindingFlags.Instance);
            Type propertyType = propertyInfo.PropertyType;
            SPWebPartManager wpManager = (SPWebPartManager)propertyInfo.GetValue(limitedWpManager, null);
            Assembly assembly = Assembly.GetAssembly(typeof(SPWeb));
            Type type = assembly.GetType("Microsoft.SharePoint.WebPartPages.SerializationTarget");
            int num = (int)propertyType.GetMethod("GetEffectiveWebPartType", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Type), type }, null).Invoke(null, new object[] { wp.GetType(), Enum.Parse(type, "Export") });
            int num2 = (int)Enum.Parse(assembly.GetType("Microsoft.SharePoint.WebPartPages.EffectiveWebPartType"), "SharePoint");
            string webPartDefinition;
            if (num2 != num)
            {
                webPartDefinition = ExportWebPartByNativeAPIInternal(wp, limitedWpManager, wpManager, ExportAspStyleWebPart);
            }
            else
            {
                webPartDefinition = ExportWebPartByNativeAPIInternal(wp, limitedWpManager, wpManager, ExportSharePointStyleWebPart);
            }
            return webPartDefinition;
        }

        private static string ExportWebPartByNativeAPIInternal(UIWebPart wp, SPLimitedWebPartManager limitedWpManager, SPWebPartManager wpManager,
            Action<UIWebPart, SPLimitedWebPartManager, SPWebPartManager, XmlTextWriter> exportAction)
        {
            var webPartStringBuilder = new StringBuilder(0x400);
            var webPartTextWriter = new XmlTextWriter(new StringWriter(webPartStringBuilder));
            exportAction(wp, limitedWpManager, wpManager, webPartTextWriter);
            return webPartStringBuilder.ToString();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private static void ExportAspStyleWebPart(UIWebPart wp, SPLimitedWebPartManager limitedWpManager, SPWebPartManager wpManager, XmlTextWriter wpWriter)
        {
            bool flag = (wp.ExportMode == WebPartExportMode.NonSensitiveData) && (limitedWpManager.Scope != PersonalizationScope.Shared);
            wpWriter.WriteStartElement("webParts");
            wpWriter.WriteStartElement("webPart");
            wpWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/WebPart/v3");
            wpWriter.WriteStartElement("metaData");
            wpWriter.WriteStartElement("type");
            Control control = (Control)wp.GetType().GetMethod("ToControl", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(wp, null);
            UserControl control2 = control as UserControl;
            if (control2 != null)
            {
                wpWriter.WriteAttributeString("src", control2.AppRelativeVirtualPath);
            }
            else
            {
                string str = (string)Assembly.GetAssembly(typeof(UIWebPart)).GetType("System.Web.UI.WebControls.WebParts.WebPartUtil").GetMethod("SerializeType", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { control.GetType() });
                wpWriter.WriteAttributeString("name", str);
            }
            wpWriter.WriteEndElement();
            wpWriter.WriteElementString("importErrorMessage", wp.ImportErrorMessage);
            wpWriter.WriteEndElement();
            wpWriter.WriteStartElement("data");

            IDictionary dictionary = (IDictionary)AveAssemblyUtility.InvokeStaticMethod(typeof(PersonalizableAttribute), "GetPersonalizablePropertyValues", new object[] { wp, PersonalizationScope.Shared, flag });

            wpWriter.WriteStartElement("properties");
            if (wp is GenericWebPart)
            {
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportIPersonalizable", new Type[] { typeof(XmlWriter), typeof(Control), typeof(Boolean) }, new object[] { wpWriter, control, flag });

                IDictionary dictionary2 = (IDictionary)AveAssemblyUtility.InvokeStaticMethod(typeof(PersonalizableAttribute), "GetPersonalizablePropertyValues", new object[] { control, PersonalizationScope.Shared, flag });
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportToWriter", new Type[] { typeof(IDictionary), typeof(XmlWriter) }, new object[] { dictionary2, wpWriter });
                wpWriter.WriteEndElement();
                wpWriter.WriteStartElement("genericWebPartProperties");
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportIPersonalizable", new Type[] { typeof(XmlWriter), typeof(Control), typeof(Boolean) }, new object[] { wpWriter, wp, flag });
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportToWriter", new Type[] { typeof(IDictionary), typeof(XmlWriter) }, new object[] { dictionary, wpWriter });

            }
            else
            {
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportIPersonalizable", new Type[] { typeof(XmlWriter), typeof(Control), typeof(Boolean) }, new object[] { wpWriter, wp, flag });
                AveAssemblyUtility.InvokeMethod(wpManager, typeof(WebPartManager), "ExportToWriter", new Type[] { typeof(IDictionary), typeof(XmlWriter) }, new object[] { dictionary, wpWriter });
            }
            wpWriter.WriteEndElement();
            wpWriter.WriteEndElement();
            wpWriter.WriteEndElement();
            wpWriter.WriteEndElement();
        }

        private static void ExportSharePointStyleWebPart(UIWebPart wp, SPLimitedWebPartManager limitedWpManager, SPWebPartManager wpManager, XmlTextWriter wpWriter)
        {
            string s = (string)wpManager.GetType().GetMethod("GetWebPartXml", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(UIWebPart), typeof(bool) }, null).Invoke(wpManager, new object[] { wp, true });
            using (StringReader reader = new StringReader(s))
            {
                wpWriter.WriteStartDocument();
                XmlTextReader reader2 = new XmlTextReader(reader);
                reader2.MoveToContent();
                while (!reader2.EOF)
                {
                    wpWriter.WriteNode(reader2, false);
                    reader2.Read();
                }
            }
        }

        #endregion Export WebPart Definition

        #endregion

        #region IAveLimitedWebPartManager Members

        public System.Web.UI.WebControls.WebParts.WebPart CreateWebPartInstance(string assemblyName, string webPartType)
        {
            object webPart = null;
            webPart = AveAssemblyUtility.CreateInstance(assemblyName, webPartType);
            return webPart as System.Web.UI.WebControls.WebParts.WebPart;
        }

        public void UpdatePropertiesByNative(Guid webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties)
        {
            mSite.QueryService.UpdateWebpartPropertiesByNative(webPartId, siteId, fileId, allUsersProperties, perUserProperties);
        }

        public void UpdatePersonalPropertiesByNative(Guid webPartId, Guid siteId, int currentUserId, byte[] perUserBytes)
        {
            mSite.QueryService.UpdatePersonalPropertiesByNative(webPartId, siteId, currentUserId, perUserBytes);
        }

        public void UpdateUserID(Guid webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal)
        {
            mSite.QueryService.UpdateWebPartUserID(webPartId, siteId, fileId, currentUserId, userId, isPersonal);
        }

        public void UpdateView(Guid webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId, string displayName)
        {
            mSite.QueryService.UpdateView(webPartId, siteId, fileId, baseViewId, view, contentTypeId, displayName);
        }

        public void InternalAddWebPart(AveWebPartBaseInfo webPartInfo, Guid siteId, string dirName, string leafName, Guid webPartId)
        {
            mSite.QueryService.InternalAddWebPart(webPartInfo, siteId, dirName, leafName, webPartId);
        }

        public void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion)
        {
            mSite.QueryService.UpdateWebPartInfo(webPartId, siteId, fileId, pageVersion, oldLevel, newLevel, isCurrentVersion, uIVersion);
            Dispose();
        }

        //public void UpdateWebPartInfo(string webPartId, Guid siteId, Guid fileId, Guid id)
        //{
        //    mSite.DBService.UpdateWebPartInfo(webPartId, siteId, fileId, id);
        //}

        public void DeleteWebPartByNative(Guid siteId, Guid docId, Guid webPartId)
        {
            mSite.QueryService.DeleteWebPartByNative(siteId, docId, webPartId);
        }

        public void DeleteAllPersonalWebParts(Guid siteId, Guid docId, int level, List<Guid> viewIds)
        {
            try
            {
                if (hasFullControlPermission)
                {
                    mSite.QueryService.DeleteAllPersonalWebParts(siteId, docId, level, viewIds);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while removing personal WebPart. Site ID: {0}, Document ID: {1}, level: {2}, exception: {3}", siteId, docId, level, ex.ToString());
            }
        }

        public AveWebPartCache Cache
        {
            get { return mCache; }
            set { mCache = value; }
        }

        #endregion

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        internal UIWebPart GetWebPart(string webPartID, bool isShared)
        {
            try
            {
                var manager = GetWebPartManager(isShared);
                return manager.WebParts.Cast<UIWebPart>().FirstOrDefault(webPart => webPart.ID.Equals(webPartID, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Cannot get Web Part. Web Part ID: {0}. Error: {1}", webPartID, ex.ToString());
                return null;
            }
        }

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        internal UIWebPart GetWebPart(Guid webPartID, bool isShared)
        {
            SPLimitedWebPartManager manager = GetWebPartManager(isShared);
            foreach (System.Web.UI.WebControls.WebParts.WebPart webPart in manager.WebParts)
            {
                Guid wId = manager.GetStorageKey(webPart);
                if (wId == webPartID)
                {
                    return webPart;
                }
            }
            return null;
        }

        [SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._160, "Ignoring this error")]
        internal Guid GetStorageKey(System.Web.UI.WebControls.WebParts.WebPart webPart, bool isShared)
        {
            try
            {
                SPLimitedWebPartManager manager = GetWebPartManager(isShared);
                return manager.GetStorageKey(webPart);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.INFO, "Cannot get the webpart storage key.", ex.ToString());
                string id = webPart.ID;
                Guid storageKey = Guid.Empty;
                if (id.StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                {
                    Guid.TryParse(id.Substring(2).Replace('_', '-'), out storageKey);
                }
                return storageKey;
            }
        }

        public void AddToNeedResetCalendarSettingsViews(Guid webId, Guid listId, Guid viewId)
        {
            this.Cache.SiteMappingManager.AddToNeedResetCalendarSettingsViews(webId, listId, viewId);
        }

        //只处理User
        public int FindMemberId(int oldUserId)
        {
            if (oldUserId == AveConstants.SYSTEM_ACCOUNT_ID)
            {
                return oldUserId;
            }
            int newId = -1;
            bool isUser = false;
            try
            {
                object member;
                if (!this.Cache.UserMapping.TryGetValue(oldUserId, out member))
                {
                    return -1;
                }

                if (member.GetType().Name.Equals("AveSPMemberInfo"))
                {
                    newId = (int)AveAssemblyUtility.GetFieldValue(member, "NewId");
                    isUser = (bool)AveAssemblyUtility.GetFieldValue(member, "IsUser");
                }
                //如果找到的不是User，认为没有找到目的端对应的user
                // -1 means this user has already been restored before and it failed.
                // So we just return null to show we cannot find this user.
                if (!isUser)
                {
                    newId = -1;
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.MemberFindError, oldUserId, e);
            }
            return newId;
        }

        #region IAveLimitedWebPartManager Members

        public void AddWebPart(IAveWebPart webPart, string zoneId, int zoneIndex)
        {
            GetWebPartManager(true).AddWebPart((webPart as AveWebPart).WebPart, zoneId, zoneIndex);
        }

        public void CloseWebPart(IAveWebPart webPart)
        {
            GetWebPartManager(true).CloseWebPart((webPart as AveWebPart).WebPart);
        }

        public void DeleteWebPart(IAveWebPart webPart)
        {
            GetWebPartManager(true).DeleteWebPart((webPart as AveWebPart).WebPart);
        }

        public void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex)
        {
            GetWebPartManager(true).MoveWebPart((webPart as AveWebPart).WebPart, zoneId, zoneIndex);
        }

        public void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex, bool isShared)
        {
            GetWebPartManager(isShared).MoveWebPart((webPart as AveWebPart).WebPart, zoneId, zoneIndex);
        }

        public void SaveChanges(IAveWebPart webPart)
        {
            GetWebPartManager(true).SaveChanges((webPart as AveWebPart).WebPart);
        }

        public void SaveChanges(IAveWebPart webPart, bool isShared)
        {
            GetWebPartManager(isShared).SaveChanges((webPart as AveWebPart).WebPart);
        }

        public void OpenWebPart(IAveWebPart webPart)
        {
            GetWebPartManager(true).SaveChanges((webPart as AveWebPart).WebPart);
        }

        public void ResetPersonalizationState(IAveWebPart webPart)
        {
            GetWebPartManager(true).ResetPersonalizationState((webPart as AveWebPart).WebPart);
        }

        #endregion

        public void AddWebPart(UIWebPart webPart, string zoneId, int zoneIndex)
        {
            GetWebPartManager(true).AddWebPart(webPart, zoneId, zoneIndex);
        }

        public void CloseWebPart(UIWebPart webPart)
        {
            GetWebPartManager(true).CloseWebPart(webPart);
        }
        public void CloseWebPart(UIWebPart webPart, bool isShared)
        {
            GetWebPartManager(isShared).CloseWebPart(webPart);
        }
        public void DeleteWebPart(UIWebPart webPart)
        {
            GetWebPartManager(true).DeleteWebPart(webPart);
        }

        public void MoveWebPart(UIWebPart webPart, string zoneId, int zoneIndex)
        {
            GetWebPartManager(true).MoveWebPart(webPart, zoneId, zoneIndex);
        }

        public void MoveWebPart(UIWebPart webPart, string zoneId, int zoneIndex, bool isShared)
        {
            try
            {
                GetWebPartManager(isShared).MoveWebPart(webPart, zoneId, zoneIndex);
            }
            catch (Exception e)
            {
                logger.Warn("Exception was thrown while moving web part. Exception: {0}", e);
            }
        }

        public void SaveChanges(UIWebPart webPart)
        {
            GetWebPartManager(true).SaveChanges(webPart);
        }

        public void SaveChanges(UIWebPart webPart, bool isShared)
        {
            GetWebPartManager(isShared).SaveChanges(webPart);
        }

        public void OpenWebPart(UIWebPart webPart)
        {
            GetWebPartManager(true).SaveChanges(webPart);
        }

        public void RestoreWebParts(List<AveWebPartBaseInfo> webparts, bool clearAll)
        {
            RestoreWebParts(webparts as IList, clearAll);
        }

        public void PostRestoreWebParts(List<AveWebPartBaseInfo> webparts)
        {
            throw new NotImplementedException();
        }

        public void UpdateWebParts(List<string> webparts)
        {
            throw new NotImplementedException();
        }

        public void ResetPersonalizationState(UIWebPart webPart)
        {
            GetWebPartManager(true).ResetPersonalizationState(webPart);
        }

        public bool ClearExsitingWebPart { get; set; }

        public IAveWebPart ImportWebPart(XmlReader reader, out string errorMessage)
        {
            return ImportWebPart(reader, true, out errorMessage);
        }

        public IAveWebPart ImportWebPart(XmlReader reader, bool isShared, out string errorMessage)
        {
            var webpart = GetWebPartManager(isShared).ImportWebPart(reader, out errorMessage);
            if (webpart != null)
            {
                return new AveWebPart(this, webpart, -1);
            }
            return null;
        }

        public void ExportWebPart(IAveWebPart webPart, XmlWriter writer)
        {
            GetWebPartManager(true).ExportWebPart((webPart as AveWebPart).WebPart, writer);
        }

        #region For SPDataAccess restore
        internal UIWebPart GetWebPart(string webPartId, bool isShared, int userId)
        {
            if (string.IsNullOrEmpty(webPartId)) return null;
            CheckPermissionAndInitPersonalManager(isShared, userId);
            return GetWebPart(webPartId, isShared);
        }

        internal UIWebPart GetWebPart(Guid webPartId, bool isShared, int userId)
        {
            if (Guid.Empty.Equals(webPartId)) return null;
            CheckPermissionAndInitPersonalManager(isShared, userId);
            return GetWebPart(webPartId, isShared);
        }

        internal void AddWebPart(UIWebPart webPart, string zoneId, int zoneIndex, bool isShared, int userId)
        {
            CheckPermissionAndInitPersonalManager(isShared, userId);
            GetWebPartManager(isShared).AddWebPart(webPart, zoneId, zoneIndex);
        }

        internal void CloseWebPart(UIWebPart webPart, bool isShared, int userId)
        {
            CheckPermissionAndInitPersonalManager(isShared, userId);
            GetWebPartManager(isShared).CloseWebPart(webPart);
        }

        internal Guid GetStorageKey(System.Web.UI.WebControls.WebParts.WebPart webPart, bool isShared, int userId)
        {
            CheckPermissionAndInitPersonalManager(isShared, userId);
            return GetStorageKey(webPart, isShared);
        }

        internal void MoveWebPart(UIWebPart webPart, string zoneId, int zoneIndex, bool isShared, int userId)
        {
            CheckPermissionAndInitPersonalManager(isShared, userId);
            MoveWebPart(webPart, zoneId, zoneIndex, isShared);
        }

        //userId对应的目的端UserId
        internal UIWebPart ReloadWebPart(string webPartId, bool isShared, int userId)
        {
            if (string.IsNullOrEmpty(webPartId)) return null;
            if (!this.hasFullControlPermission && !isShared && userId <= 0) throw new ArgumentException("Invalid user id");

            Dispose();
            Reload();
            CheckPermissionAndInitPersonalManager(isShared, userId);
            return GetWebPart(webPartId, isShared);
        }

        //userId对应的目的端UserId
        internal UIWebPart ReloadWebPart(Guid webPartId, bool isShared, int userId)
        {
            if (Guid.Empty == webPartId) return null;
            if (!this.hasFullControlPermission && !isShared && userId <= 0) throw new ArgumentException("Invalid user id");

            Dispose();
            Reload();
            CheckPermissionAndInitPersonalManager(isShared, userId);
            return GetWebPart(webPartId, isShared);
        }

        internal void SaveChanges(UIWebPart webPart, bool isShared, int userId)
        {
            CheckPermissionAndInitPersonalManager(isShared, userId);
            GetWebPartManager(isShared).SaveChanges(webPart);
        }

        private void CheckPermissionAndInitPersonalManager(bool isShared, int userId)
        {
            if (!this.hasFullControlPermission && !isShared)
            {
                if (userId <= 0) throw new ArgumentException("Invalid user id");
                InitPersonalManagerByUser(userId);
            }
        }
        #endregion

    }

    public class AveWebPartUtility
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //resultCode:1,不能解析allUsers；2，不能解析perUser
        public static Dictionary<string, object> GetProperties(byte[] allUsers, byte[] perUser, out int resultCode)
        {
            resultCode = 0;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            try
            {
                bool flag1 = false;
                bool flag2 = false;
                if (allUsers != null && allUsers[0] == 0x01 && allUsers[1] == 0x05)
                {
                    flag1 = true;
                }
                if (perUser != null && perUser[0] == 0x01 && perUser[1] == 0x05)
                {
                    flag2 = true;
                }

                if (flag1 && flag2)
                {
                    Parse0X0105(allUsers, perUser, properties);
                }
                else if (flag1 && !flag2)
                {
                    Parse0X0105(allUsers, null, properties);
                    try
                    {
                        object[] objects = Parse0xFF01(perUser);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 2;
                    }
                }
                else if (!flag1 && flag2)
                {
                    try
                    {
                        object[] objects = Parse0xFF01(allUsers);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 1;
                    }
                    Parse0X0105(null, perUser, properties);
                }
                else if (!flag1 && !flag2)
                {
                    object[] objects = null;
                    try
                    {
                        objects = Parse0xFF01(allUsers);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 1;
                    }
                    try
                    {
                        objects = Parse0xFF01(perUser);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 2;
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                resultCode = 3;
            }
            return properties;
        }

        private static Dictionary<string, object> GetApplyProperties(object[] objects, Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            ApplyProperty apply = new ApplyProperty(objects);
            Dictionary<string, object> temProperties = apply.ApplyPropertyState();
            if (temProperties != null)
            {
                foreach (string key in temProperties.Keys)
                {
                    if ((temProperties[key] as Pair) != null)
                    {
                        properties[key] = (temProperties[key] as Pair).Second;
                    }
                    else
                    {
                        properties[key] = temProperties[key];
                    }
                }
            }
            return properties;
        }

        private static object[] DeserializeByteArrayToObject(byte[] bytes)
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            object[] values = null;
            if ((bytes != null) && (bytes.Length != 0))
            {
                values = (object[])formatter.Deserialize(new MemoryStream(bytes));
            }
            return values;
        }

        private static object[] Parse0xFF01(byte[] bts)
        {
            byte[] temp1 = null;
            byte[] temp2 = null;
            if (bts != null)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    if (bts[i] == 0xff && bts[i + 1] == 0x01)
                    {
                        temp1 = new byte[i];
                        temp2 = new byte[bts.Length - i];
                        for (int j = 0; j < bts.Length; j++)
                        {
                            if (j < temp1.Length)
                            {
                                temp1[j] = bts[j];
                            }
                            else
                            {
                                temp2[j - temp1.Length] = bts[j];
                            }
                        }
                        break;
                    }
                }
                if (temp2 != null)
                {
                    return DeserializeByteArrayToObject(temp2);
                }
            }
            return null;
        }

        private static Dictionary<string, object> Parse0X0105(byte[] allUsers, byte[] perUser, Dictionary<string, object> properties)
        {
            //Assembly assem = Assembly.GetAssembly(typeof(Microsoft.SharePoint.WebPartPages.WebPart));
            //Type WebPartNameTable = assem.GetType("Microsoft.SharePoint.WebPartPages.WebPartNameTable");
            //Type CompressedXmlReader = assem.GetType("Microsoft.SharePoint.WebPartPages.CompressedXmlReader");
            //ConstructorInfo constructorInfo1 = WebPartNameTable.GetConstructors()[0];
            //object obj = constructorInfo1.Invoke(null);
            //ConstructorInfo constructorInfo2 = CompressedXmlReader.GetConstructors()[0];
            //XmlReader reader = (XmlReader)constructorInfo2.Invoke(new object[] { new XmlNamespaceManager((XmlNameTable)obj), allUsers, perUser });
            CompressedXmlReader reader = new CompressedXmlReader(new XmlNamespaceManager(new WebPartNameTable()), perUser, allUsers);
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            string propertyName = string.Empty;
            object value = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        propertyName = reader.LocalName;
                        break;
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        value = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if (!String.IsNullOrEmpty(propertyName) && propertyName != "WebPart")
                        {
                            properties[propertyName] = value;
                        }
                        propertyName = String.Empty;
                        value = String.Empty;
                        break;
                    default:
                        break;
                }
            }
            return properties;
        }

        public static string SerializeWebPartConnection(object obj)
        {
            string result = string.Empty;
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, obj);
                byte[] buffer = stream.GetBuffer();
                result = Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, "Failed to serialize web part connection. Error: {0}", ex.ToString());
            }
            finally
            {
                stream.Close();
            }
            return result;
        }

        public static object DeserializeWebPartConnection(string text)
        {
            object result = null;
            try
            {
                ObjectStateFormatter formatter = new ObjectStateFormatter();
                byte[] buffer = Convert.FromBase64String(text);
                result = formatter.Deserialize(new MemoryStream(buffer));
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, "Failed to deserialize web part connection. Error: {0}", ex.ToString());
            }
            return result;
        }

        public static System.Web.UI.WebControls.Unit ConvertStringToUnit(string value)
        {
            System.Web.UI.WebControls.UnitConverter converter = new System.Web.UI.WebControls.UnitConverter();
            return (System.Web.UI.WebControls.Unit)converter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, value);
        }
    }

    internal class ApplyProperty
    {
        private int m_index = 3;
        private object[] m_activeObjects;
        private short m_segmentType;
        private int SerializationMajorVersion;
        private int SerializationMinorVersion;
        private int m_count;
        private string[] predefinedStrings;

        public ApplyProperty(object[] array)
        {
            m_activeObjects = array;
            predefinedStrings = GetPredefinedStrings();
            if (m_activeObjects.Length > 2)
            {
                if (m_activeObjects[0] is int)
                {
                    SerializationMajorVersion = (int)m_activeObjects[0];
                }
                if (m_activeObjects[1] is int)
                {
                    SerializationMinorVersion = (int)m_activeObjects[1];
                }
            }
        }

        protected bool GetNextSegment()
        {
            if ((this.m_activeObjects != null) && (this.m_index >= this.m_activeObjects.Length))
            {
                return false;
            }
            this.m_segmentType = (this.m_activeObjects[this.m_index] is SegmentType) ? ((short)((SegmentType)this.m_activeObjects[this.m_index++])) : ((short)this.m_activeObjects[this.m_index++]);
            while (this.m_segmentType >= 5)
            {
                this.m_index += 1 + ((int)this.m_activeObjects[this.m_index]);
                if (this.m_index >= this.m_activeObjects.Length)
                {
                    return false;
                }
                this.m_segmentType = (this.m_activeObjects[this.m_index] is SegmentType) ? ((short)((SegmentType)this.m_activeObjects[this.m_index++])) : ((short)this.m_activeObjects[this.m_index++]);
            }
            return true;

        }

        protected short GetSegmentType()
        {
            return this.m_segmentType;
        }

        protected int ObjectCount()
        {
            this.m_count = (int)this.m_activeObjects[this.m_index++];
            return this.m_count;
        }

        protected string[] GetPredefinedStrings()
        {
            //Assembly assem = Assembly.GetAssembly(typeof(Microsoft.SharePoint.WebPartPages.WebPart));
            //Type xmlSchema = assem.GetType("Microsoft.SharePoint.WebPartPages.XmlSchema");
            //FieldInfo[] fields = xmlSchema.GetFields();
            //string[] predefinedStrings = new string[fields.Length];

            //for (ushort i = 0; i < fields.Length; i++)
            //{
            //    FieldInfo info = fields[i];
            //    string s = (string)info.GetValue(null);
            //    if (s != null)
            //    {
            //        predefinedStrings[i] = s;
            //    }
            //}
            //return predefinedStrings;
            return PredefinedStrings.PREDEFINEDSTRINGS;
        }

        protected string ResolveTokenizedString(int key)
        {
            if (key < predefinedStrings.Length)
            {
                return predefinedStrings[key];
            }
            return null;
        }

        protected object GetNextObject()
        {
            return this.m_activeObjects[this.m_index++];
        }

        protected void SkipSegment()
        {
            this.m_index += 1 + ((int)this.m_activeObjects[this.m_index]);
        }

        public Dictionary<string, object> ApplyPropertyState()
        {
            if (m_activeObjects.Length < 3)
            {
                return null;
            }
            Dictionary<string, object> properties = new Dictionary<string, object>();
            if ((2 == this.SerializationMajorVersion) && ((3 == this.SerializationMinorVersion) || (2 == this.SerializationMinorVersion)))
            {
                while (this.GetNextSegment())
                {
                    int num;
                    string key = string.Empty;
                    object nextObject;
                    switch (this.GetSegmentType())
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            num = this.ObjectCount();
                            while (num > 0)
                            {
                                nextObject = this.GetNextObject();
                                if (nextObject is int)
                                {
                                    key = ResolveTokenizedString((int)nextObject);
                                }
                                else
                                {
                                    key = nextObject.ToString();
                                }
                                nextObject = this.GetNextObject();
                                properties.Add(key, nextObject);
                                num -= 2;
                            }
                            continue;

                        case 4:
                            this.SkipSegment();
                            continue;
                        default:
                            break;
                    }
                    num = this.ObjectCount();
                    while (num > 0)
                    {
                        int index = (int)this.GetNextObject();
                        num--;
                    }
                    continue;
                }
            }
            return properties;
        }

        internal enum SegmentType : byte
        {
            AttachedProperties = 3,
            IPersonalizableProperties = 2,
            LinkMap = 4,
            NonPersonalizableProperties = 1,
            PersonalizableProperties = 0,
            Unknown = 5
        }
    }

    internal class CompressedXmlReader : XmlReader
    {
        // Fields
        private bool _needToPopScope;
        private const int ATTRIBUTE_NIL = -1;
        private ArrayList attributes = new ArrayList();
        private BinaryReader br;
        private int depth;
        private bool eof;
        private byte[] global;
        private int iAttribute = -1;
        private string localName;
        private WebPartNameTable nameTable;
        private string ns;
        private XmlNamespaceManager nsManager;
        private byte[] personal;
        private string text;
        private XmlNodeType type;
        private bool usePersonal;

        // Methods
        public CompressedXmlReader(XmlNamespaceManager nsManager, byte[] personal, byte[] global)
        {
            //ULS.ShipAssertTag(0x3839316d, ULSCat.msoulscat_WSS_WebParts, (personal != null) || (global != null));
            this.personal = personal;
            this.global = global;
            this.nameTable = WebPartNameTable.GlobalNameTable();
            this.nsManager = nsManager;
            this.SetBinaryReader(personal != null);
            //ULS.ShipAssertTag(0x3839316e, ULSCat.msoulscat_WSS_WebParts, this.br != null);
        }

        public override void Close()
        {
            this.br.Close();
        }

        public override string GetAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name, string ns)
        {
            foreach (WebPartXmlAttribute attribute in this.attributes)
            {
                if ((attribute.localName == name) && (attribute.ns == ns))
                {
                    return attribute.val;
                }
            }
            return null;
        }

        public override string LookupNamespace(string prefix)
        {
            return this.nsManager.LookupNamespace(this.nameTable.Get(prefix));
        }

        public override void MoveToAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToElement()
        {
            bool flag = false;
            if (this.iAttribute >= 0)
            {
                this.PopToElement();
                flag = true;
            }
            return flag;
        }

        public override bool MoveToFirstAttribute()
        {
            bool flag = false;
            this.iAttribute = -1;
            if ((this.type == XmlNodeType.Element) && (this.attributes.Count > 0))
            {
                this.depth++;
                this.type = XmlNodeType.Attribute;
                this.iAttribute = 0;
                flag = true;
            }
            return flag;
        }

        public override bool MoveToNextAttribute()
        {
            switch (this.type)
            {
                case XmlNodeType.Element:
                    return this.MoveToFirstAttribute();

                case XmlNodeType.Attribute:
                    break;

                case XmlNodeType.Text:
                    this.depth--;
                    this.type = XmlNodeType.Attribute;
                    break;

                default:
                    return false;
            }
            if ((this.iAttribute + 1) < this.attributes.Count)
            {
                this.iAttribute++;
                return true;
            }
            return false;
        }

        private XmlNodeType PeekNodeType()
        {
            //ULS.ShipAssertTag(0x3839316f, ULSCat.msoulscat_WSS_WebParts, !this.eof);
            return (XmlNodeType)this.br.PeekChar();
        }

        private void PopToElement()
        {
            switch (this.type)
            {
                case XmlNodeType.Attribute:
                    this.depth--;
                    break;

                case XmlNodeType.Text:
                    this.depth -= 2;
                    break;
            }
            this.type = XmlNodeType.Element;
        }

        public override bool Read()
        {
            if (this.eof)
            {
                return false;
            }
            if (this._needToPopScope)
            {
                this._needToPopScope = false;
                this.nsManager.PopScope();
            }
            else if (this.iAttribute >= 0)
            {
                this.PopToElement();
                this.iAttribute = -1;
                this.attributes.Clear();
            }
            XmlNodeType type = (XmlNodeType)this.br.ReadByte();
            switch (type)
            {
                case XmlNodeType.None:
                    break;

                case XmlNodeType.Element:
                    this.nsManager.PushScope();
                    this.localName = this.ReadPredefinedString();
                    this.ns = this.ReadPredefinedString();
                    if (this.ns.Length > 0)
                    {
                        this.nsManager.AddNamespace(string.Empty, this.ns);
                    }
                    this.text = null;
                    this.depth++;
                    this.ReadAttributes();
                    break;

                case XmlNodeType.Text:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.CDATA:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.EndElement:
                    this.depth--;
                    this._needToPopScope = true;
                    if (this.depth == 0)
                    {
                        this.br = null;
                        if (this.usePersonal && (this.global != null))
                        {
                            this.type = XmlNodeType.None;
                            this.SetBinaryReader(false);
                            this.MoveToContent();
                            this.Read();
                            type = this.type;
                        }
                        else
                        {
                            this.eof = true;
                        }
                    }
                    break;

                default:
                    //ULS.ShipAssertTag(0x3839317a, ULSCat.msoulscat_WSS_WebParts, false);
                    break;
            }
            this.type = type;
            return true;
        }

        private void ReadAttributes()
        {
            this.attributes.Clear();
            while (this.PeekNodeType() == XmlNodeType.Attribute)
            {
                this.br.ReadByte();
                WebPartXmlAttribute attribute = new WebPartXmlAttribute
                {
                    prefix = this.ReadPredefinedString(),
                    localName = this.ReadPredefinedString(),
                    ns = this.ReadPredefinedString()
                };
                this.text = null;
                while (this.Read() && (this.type != XmlNodeType.None))
                {
                }
                attribute.val = this.text;
                if (attribute.prefix == "xmlns")
                {
                    this.nsManager.AddNamespace(attribute.localName, attribute.val);
                }
                this.attributes.Add(attribute);
            }
            this.iAttribute = -1;
        }

        public override bool ReadAttributeValue()
        {
            bool flag = false;
            if (this.type == XmlNodeType.Attribute)
            {
                this.depth++;
                this.type = XmlNodeType.Text;
                flag = true;
            }
            return flag;
        }

        public override string ReadInnerXml()
        {
            throw new NotImplementedException();
        }

        public override string ReadOuterXml()
        {
            throw new NotImplementedException();
        }

        private string ReadPredefinedString()
        {
            return this.ReadPredefinedString(true);
        }

        private string ReadPredefinedString(bool addToNameTable)
        {
            string predefinedString = null;
            ushort us = this.br.ReadUInt16();
            if (us == 0xffff)
            {
                if (addToNameTable)
                {
                    return this.nameTable.Add(this.br.ReadString());
                }
                return this.br.ReadString();
            }
            predefinedString = this.nameTable.LookupPredefinedString(us);
            if (predefinedString == null)
            {
                switch (us)
                {
                    case 0x61:
                        return "http://schemas.microsoft.com/WebPart/v2/PivotView";

                    case 0x31:
                        return "CaptureMethod";
                }
            }
            return predefinedString;
        }

        public override string ReadString()
        {
            string str = "";
            while (this.type != XmlNodeType.EndElement)
            {
                if (this.type == XmlNodeType.Text)
                {
                    str = str + this.text;
                }
                if (!this.Read())
                {
                    return str;
                }
            }
            return str;
        }

        public override void ResolveEntity()
        {
            throw new NotImplementedException();
        }

        private void SetBinaryReader(bool usePersonal)
        {
            byte[] personal = this.personal;
            this.usePersonal = usePersonal;
            if (!usePersonal)
            {
                personal = this.global;
            }
            this.br = new BinaryReader(new MemoryStream(personal));
        }

        // Properties
        public override int AttributeCount
        {
            get
            {
                return this.attributes.Count;
            }
        }

        public override string BaseURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override int Depth
        {
            get
            {
                return this.depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.eof;
            }
        }

        public override bool HasValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        public override string this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string LocalName
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.localName;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).localName;
                }
                return null;
            }
        }

        public override string Name
        {
            get
            {
                if (this.Prefix.Length == 0)
                {
                    return this.LocalName;
                }
                return (this.Prefix + ":" + this.LocalName);
            }
        }

        public override string NamespaceURI
        {
            get
            {
                string ns = string.Empty;
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        ns = this.ns;
                        break;

                    case XmlNodeType.Attribute:
                        ns = ((WebPartXmlAttribute)this.attributes[this.iAttribute]).ns;
                        break;
                }
                if (ns.Length != 0)
                {
                    return ns;
                }
                if (this.Prefix.Length > 0)
                {
                    return this.LookupNamespace(this.Prefix);
                }
                return this.nsManager.DefaultNamespace;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.type;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.type == XmlNodeType.Attribute)
                {
                    return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).prefix;
                }
                return string.Empty;
            }
        }

        public override char QuoteChar
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ReadState ReadState
        {
            get
            {
                if (this.eof)
                {
                    return ReadState.EndOfFile;
                }
                return ReadState.Interactive;
            }
        }

        public override string Value
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.text;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;

                    case XmlNodeType.Text:
                        if (this.iAttribute < 0)
                        {
                            return this.text;
                        }
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;

                    case XmlNodeType.CDATA:
                        if (this.text.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase) && this.text.EndsWith("]]>", StringComparison.OrdinalIgnoreCase))
                        {
                            this.text = this.text.Substring(9, this.text.Length - 12);
                        }
                        return this.text;
                }
                return null;
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // Nested Types
        private class WebPartXmlAttribute
        {
            // Fields
            public string localName;
            public string ns;
            public string prefix;
            public string val;
        }
    }

    internal class WebPartNameTable : XmlNameTable
    {
        // Fields
        private static WebPartNameTable _nameTable;
        private static Hashtable _table;
        private static string[] predefinedStrings;

        // Methods
        static WebPartNameTable()
        {
            //FieldInfo[] fields = typeof(XmlSchema).GetFields();
            //ULS.ShipAssertTag(0x39676839, ULSCat.msoulscat_WSS_WebParts, fields.Length < 0xffff);
            predefinedStrings = new string[PredefinedStrings.PREDEFINEDSTRINGS.Length];
            _nameTable = new WebPartNameTable();
            _table = new Hashtable();
            for (ushort i = 0; i < PredefinedStrings.PREDEFINEDSTRINGS.Length; i = (ushort)(i + 1))
            {
                //FieldInfo info = fields[i];
                string s = PredefinedStrings.PREDEFINEDSTRINGS[i];
                if (string.Empty == s)
                {
                    s = string.Empty;
                }
                AddPredefinedString(i, s);
            }
        }

        public override string Add(string array)
        {
            string str = this.Get(array);
            if (str == null)
            {
                lock (_table)
                {
                    str = this.Get(array);
                    if (str == null)
                    {
                        _table[array] = new StringEntry(array);
                        str = array;
                    }
                }
            }
            return str;
        }

        public override string Add(char[] array, int offset, int length)
        {
            return this.Add(new string(array, offset, length));
        }

        private static void AddPredefinedString(ushort us, string s)
        {
            if (s != null)
            {
                predefinedStrings[us] = s;
                _table[s] = new StringEntry(s, us);
            }
        }

        public override string Get(string array)
        {
            StringEntry entry = (StringEntry)_table[array];
            if (entry != null)
            {
                return entry._s;
            }
            return null;
        }

        public override string Get(char[] array, int offset, int length)
        {
            return this.Get(new string(array, offset, length));
        }

        public static WebPartNameTable GlobalNameTable()
        {
            return _nameTable;
        }

        public string LookupPredefinedString(ushort us)
        {
            return predefinedStrings[us];
        }

        public static ushort LookupPredefinedStringConstant(string s)
        {
            ushort num = 0xffff;
            StringEntry entry = (StringEntry)_table[s];
            if (entry != null)
            {
                num = entry._predefinedConstant;
            }
            return num;
        }

        // Nested Types
        public class StringEntry
        {
            // Fields
            public readonly ushort _predefinedConstant;
            public readonly string _s;

            // Methods
            public StringEntry(string s)
            {
                this._s = s;
                this._predefinedConstant = 0xffff;
            }

            public StringEntry(string s, ushort predefinedConstant)
                : this(s)
            {
                this._predefinedConstant = predefinedConstant;
            }
        }
    }

    internal class PredefinedStrings
    {
        //it is thread safe, no write operations
        public static readonly string[] PREDEFINEDSTRINGS = new string[150];

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        static PredefinedStrings()
        {
            PREDEFINEDSTRINGS[0] = "http://schemas.microsoft.com/WebPart/v2";
            PREDEFINEDSTRINGS[1] = "Dir";
            PREDEFINEDSTRINGS[2] = "Description";
            PREDEFINEDSTRINGS[3] = "Encoding";
            PREDEFINEDSTRINGS[4] = "Title";
            PREDEFINEDSTRINGS[5] = "WebPart";
            PREDEFINEDSTRINGS[6] = "IsIncluded";
            PREDEFINEDSTRINGS[7] = "Zone";
            PREDEFINEDSTRINGS[8] = "ZoneID";
            PREDEFINEDSTRINGS[9] = "PartOrder";
            PREDEFINEDSTRINGS[10] = "NumberLimit";
            PREDEFINEDSTRINGS[11] = "FrameState";
            PREDEFINEDSTRINGS[12] = "Height";
            PREDEFINEDSTRINGS[13] = "Width";
            PREDEFINEDSTRINGS[14] = "Toolbar";
            PREDEFINEDSTRINGS[15] = "ContentLink";
            PREDEFINEDSTRINGS[16] = "DisplayName";
            PREDEFINEDSTRINGS[17] = "DataFields";
            PREDEFINEDSTRINGS[18] = "DataQuery";
            PREDEFINEDSTRINGS[19] = "XSLLink";
            PREDEFINEDSTRINGS[20] = "XSL";
            PREDEFINEDSTRINGS[21] = "AllowRemove";
            PREDEFINEDSTRINGS[22] = "AllowMinimize";
            PREDEFINEDSTRINGS[23] = "IsVisible";
            PREDEFINEDSTRINGS[24] = "Namespace";
            PREDEFINEDSTRINGS[25] = "ViewFlag";
            PREDEFINEDSTRINGS[26] = "DetailLink";
            PREDEFINEDSTRINGS[27] = "HelpLink";
            PREDEFINEDSTRINGS[28] = "PartStorage";
            PREDEFINEDSTRINGS[29] = "";
            PREDEFINEDSTRINGS[30] = "";
            PREDEFINEDSTRINGS[31] = "PartImageSmall";
            PREDEFINEDSTRINGS[32] = "PartImageLarge";
            PREDEFINEDSTRINGS[33] = "Assembly";
            PREDEFINEDSTRINGS[34] = "TypeName";
            PREDEFINEDSTRINGS[35] = "";
            PREDEFINEDSTRINGS[36] = "";
            PREDEFINEDSTRINGS[37] = "FrameType";
            PREDEFINEDSTRINGS[38] = "Connections";
            PREDEFINEDSTRINGS[39] = "MissingAssembly";
            PREDEFINEDSTRINGS[40] = "Name";
            PREDEFINEDSTRINGS[41] = "";
            PREDEFINEDSTRINGS[42] = "xmlns";
            PREDEFINEDSTRINGS[43] = "AllowZoneChange";
            PREDEFINEDSTRINGS[44] = "ParamBindings";
            PREDEFINEDSTRINGS[45] = "FireInitialRow";
            PREDEFINEDSTRINGS[46] = "";
            PREDEFINEDSTRINGS[47] = "ImageLink";
            PREDEFINEDSTRINGS[48] = "";
            PREDEFINEDSTRINGS[49] = "";
            PREDEFINEDSTRINGS[50] = "PostData";
            PREDEFINEDSTRINGS[51] = "Tags";
            PREDEFINEDSTRINGS[52] = "TagIndexes";
            PREDEFINEDSTRINGS[53] = "RenderTags";
            PREDEFINEDSTRINGS[54] = "RenderTagIndexes";
            PREDEFINEDSTRINGS[55] = "LastUpdated";
            PREDEFINEDSTRINGS[56] = "RefreshInterval";
            PREDEFINEDSTRINGS[57] = "LastCached";
            PREDEFINEDSTRINGS[58] = "";
            PREDEFINEDSTRINGS[59] = "Content";
            PREDEFINEDSTRINGS[60] = "ConnectionID";
            PREDEFINEDSTRINGS[61] = "http://www.w3.org/2001/XMLSchema";
            PREDEFINEDSTRINGS[62] = "http://www.w3.org/2001/XMLSchema-instance";
            PREDEFINEDSTRINGS[63] = "Normal";
            PREDEFINEDSTRINGS[64] = "Minimized";
            PREDEFINEDSTRINGS[65] = "Default";
            PREDEFINEDSTRINGS[66] = "LeftToRight";
            PREDEFINEDSTRINGS[67] = "RightToLeft";
            PREDEFINEDSTRINGS[68] = "None";
            PREDEFINEDSTRINGS[69] = "Standard";
            PREDEFINEDSTRINGS[70] = "TitleBarOnly";
            PREDEFINEDSTRINGS[71] = "true";
            PREDEFINEDSTRINGS[72] = "false";
            PREDEFINEDSTRINGS[73] = "xsi";
            PREDEFINEDSTRINGS[74] = "xsd";
            PREDEFINEDSTRINGS[75] = "NoDefaultStyle";
            PREDEFINEDSTRINGS[76] = "VerticalAlignment";
            PREDEFINEDSTRINGS[77] = "HorizontalAlignment";
            PREDEFINEDSTRINGS[78] = "BackgroundColor";
            PREDEFINEDSTRINGS[79] = "IsIncludedFilter";
            PREDEFINEDSTRINGS[80] = "XML";
            PREDEFINEDSTRINGS[81] = "XMLLink";
            PREDEFINEDSTRINGS[82] = "HeaderCaption";
            PREDEFINEDSTRINGS[83] = "HeaderTitle";
            PREDEFINEDSTRINGS[84] = "HeaderDescription";
            PREDEFINEDSTRINGS[85] = "Image";
            PREDEFINEDSTRINGS[86] = "ContentHasToken";
            PREDEFINEDSTRINGS[87] = "ExportControlledProperties";
            PREDEFINEDSTRINGS[88] = "SourceType";
            PREDEFINEDSTRINGS[89] = "Fields";
            PREDEFINEDSTRINGS[90] = "http://schemas.microsoft.com/WebPart/v2/ContentEditor";
            PREDEFINEDSTRINGS[91] = "http://schemas.microsoft.com/WebPart/v2/PageViewer";
            PREDEFINEDSTRINGS[92] = "http://schemas.microsoft.com/WebPart/v2/Image";
            PREDEFINEDSTRINGS[93] = "http://schemas.microsoft.com/WebPart/v2/Xml";
            PREDEFINEDSTRINGS[94] = "http://schemas.microsoft.com/WebPart/v2/DataView";
            PREDEFINEDSTRINGS[95] = "http://schemas.microsoft.com/WebPart/v2/ListForm";
            PREDEFINEDSTRINGS[96] = "http://schemas.microsoft.com/WebPart/v2/ListView";
            PREDEFINEDSTRINGS[97] = "";
            PREDEFINEDSTRINGS[98] = "http://schemas.microsoft.com/WebPart/v2/TitleBar";
            PREDEFINEDSTRINGS[99] = "http://schemas.microsoft.com/WebPart/v2/SimpleForm";
            PREDEFINEDSTRINGS[100] = "http://schemas.microsoft.com/WebPart/v2/Members";
            PREDEFINEDSTRINGS[101] = "CacheDataStorage";
            PREDEFINEDSTRINGS[102] = "CacheDataTimeout";
            PREDEFINEDSTRINGS[103] = "CacheXslStorage";
            PREDEFINEDSTRINGS[104] = "AlternativeText";
            PREDEFINEDSTRINGS[105] = "DataSourceBindings";
            PREDEFINEDSTRINGS[106] = "Template";
            PREDEFINEDSTRINGS[107] = "http://schemas.microsoft.com/WebPart/v3";
            PREDEFINEDSTRINGS[108] = "ID";
            PREDEFINEDSTRINGS[109] = "AttachedPropertiesShared";
            PREDEFINEDSTRINGS[110] = "AttachedPropertiesUser";
            PREDEFINEDSTRINGS[111] = "AllowConnect";
            PREDEFINEDSTRINGS[112] = "AllowEdit";
            PREDEFINEDSTRINGS[113] = "AllowHide";
            PREDEFINEDSTRINGS[114] = "HelpMode";
            PREDEFINEDSTRINGS[115] = "http://schemas.microsoft.com/WebPart/v2/UserTasks";
            PREDEFINEDSTRINGS[116] = "http://schemas.microsoft.com/WebPart/v2/UserDocs";
            PREDEFINEDSTRINGS[117] = "http://schemas.microsoft.com/WebPart/v2/Aggregation";
            PREDEFINEDSTRINGS[118] = "QuerySiteCollection";
            PREDEFINEDSTRINGS[119] = "MaxItemsShown";
            PREDEFINEDSTRINGS[120] = "QueryLastModifiedBy";
            PREDEFINEDSTRINGS[121] = "QueryCreatedBy";
            PREDEFINEDSTRINGS[122] = "QueryCheckedOutBy";
            PREDEFINEDSTRINGS[123] = "DisplayFolderColumn";
            PREDEFINEDSTRINGS[124] = "DisplayItemLinkColumn";
            PREDEFINEDSTRINGS[125] = "TitleUrl";
            PREDEFINEDSTRINGS[126] = "DisplayType";
            PREDEFINEDSTRINGS[127] = "MembershipGroupId";
            PREDEFINEDSTRINGS[128] = "AllowClose";
            PREDEFINEDSTRINGS[129] = "AuthorizationFilter";
            PREDEFINEDSTRINGS[130] = "CatalogIconImageUrl";
            PREDEFINEDSTRINGS[131] = "ChromeState";
            PREDEFINEDSTRINGS[132] = "ChromeType";
            PREDEFINEDSTRINGS[133] = "Direction";
            PREDEFINEDSTRINGS[134] = "ExportMode";
            PREDEFINEDSTRINGS[135] = "HelpUrl";
            PREDEFINEDSTRINGS[136] = "Hidden";
            PREDEFINEDSTRINGS[137] = "ImportErrorMessage";
            PREDEFINEDSTRINGS[138] = "IsClosed";
            PREDEFINEDSTRINGS[139] = "TitleIconImageUrl";
            PREDEFINEDSTRINGS[140] = "ZoneIndex";
            PREDEFINEDSTRINGS[141] = "PersonalizableProperties";
            PREDEFINEDSTRINGS[142] = "NonPersonalizableProperties";
            PREDEFINEDSTRINGS[143] = "IPersonalizableProperties";
            PREDEFINEDSTRINGS[144] = "AttachedProperties";
            PREDEFINEDSTRINGS[145] = "LinkMap";
            PREDEFINEDSTRINGS[146] = "Unknown";
            PREDEFINEDSTRINGS[147] = "ViewContentTypeId";
            PREDEFINEDSTRINGS[148] = "CssStyleSheet";
            PREDEFINEDSTRINGS[149] = "ListName";
        }
    }

    public class AveWebPartExtensionHandler
    {
        protected AveLimitedWebPartManager mManager;
        protected AveWeb mWeb;
        protected Dictionary<string, string> extensionProperties = new Dictionary<string, string>();
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected AveWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
        {
            mManager = aveManager;
            mWeb = (AveWeb)aveManager.Web;
        }

        public static AveWebPartExtensionHandler GetWebPartExtensionHandler(string name, AveLimitedWebPartManager aveManager)
        {
            if (aveManager == null || string.IsNullOrEmpty(name) || name.Equals("WebPart", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            Type subType = Type.GetType(string.Format("AvePoint.ObjectModel.ServerSE.Ave{0}ExtensionHandler", name), false, true);
            AveWebPartExtensionHandler extension = null;
            if (subType != null)
            {
                extension = subType.GetConstructor(new Type[] { typeof(AveLimitedWebPartManager) }).Invoke(new object[] { aveManager }) as AveWebPartExtensionHandler;
            }
            return extension;
        }

        public virtual Dictionary<string, string> GetWebPartExtensionInfo(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            return extensionProperties;
        }
    }

    public class AveContactFieldControlExtensionHandler : AveWebPartExtensionHandler
    {
        public AveContactFieldControlExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                ContactFieldControl contactFieldControl = webPart as ContactFieldControl;
                int userId = contactFieldControl.Contact;
                if (userId > 0)
                {
                    extensionProperties["UserId"] = userId.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting ContactFieldControl WebPart extension info. WebPart ID:{0}. Error:{1}", webPart.ID.ToString(), ex.ToString());
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
    }
    public class AveTermPropertyExtensionHandler : AveWebPartExtensionHandler
    {
        public AveTermPropertyExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager)
        { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                TermProperty termPropertyWebPart = webPart as TermProperty;
                string mmsData = GetTermPropertyMMSInfo(termPropertyWebPart);
                if (!string.IsNullOrWhiteSpace(mmsData))
                {
                    extensionProperties["TermPropertyWebPartMMSData"] = mmsData;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting TermProperty WebPart extension info. WebPart ID:{0}. Error:{1}", webPart.ID.ToString(), ex);
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
        private string GetTermPropertyMMSInfo(TermProperty termPropertyWebPart)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement rootElement = xDoc.CreateElement("TermPropertyWebPart");
            xDoc.AppendChild(rootElement);
            rootElement.SetAttribute("termStoreID", termPropertyWebPart.TermStoreID.ToString());
            rootElement.SetAttribute("termSetID", termPropertyWebPart.TermSetID.ToString());
            rootElement.SetAttribute("termID", termPropertyWebPart.TermID.ToString());

            return xDoc.OuterXml;
        }
    }
    public class AveXsltListViewWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AveXsltListViewWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            XsltListViewWebPart xsltListViewWebPart = webPart as XsltListViewWebPart;
            string indexValue = GetWebPartExtensionInfoForIndexColumn(xsltListViewWebPart.XmlDefinition, xsltListViewWebPart.ListId);
            if (!string.IsNullOrEmpty(indexValue))
            {
                extensionProperties["WithIndex"] = indexValue;
            }
            return base.GetWebPartExtensionInfo(webPart);
        }

        private string GetWebPartExtensionInfoForIndexColumn(string xmlDefinition, Guid listId)
        {
            string value = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(xmlDefinition) && xmlDefinition.IndexOf("<WithIndex ", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(xmlDefinition);
                    XmlElement node = xDoc.GetElementsByTagName("WithIndex")[0] as XmlElement;
                    string id = node.GetAttribute("ID");
                    SPList list = mWeb.Web.Lists[listId];
                    SPFieldIndex fieldIndex = list.FieldIndexes[new Guid(id)];
                    value = fieldIndex.GetField(0).ToString() + "#" + fieldIndex.GetField(1).ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, "An exception occurred while getting GetWebPartExtensionInfoForIndexColumn. List ID: {0}. Exception: {1}", listId.ToString(), ex.ToString());
            }
            return value;
        }
    }

    public class AveSPTimelineWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AveSPTimelineWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                SPTimelineWebPart spTimelineWebPart = webPart as SPTimelineWebPart;
                string listTitle = mWeb.Lists[new Guid(spTimelineWebPart.ListId)].Title;
                if (!string.IsNullOrEmpty(listTitle))
                {
                    extensionProperties["ExtensionListTitle"] = listTitle;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting  SPTimelineWebPart extension info. Web Part ID: {0}. Error: {1}", webPart.ID, ex.ToString());
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
    }

    public class AveProjectSummaryWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AveProjectSummaryWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                ProjectSummaryWebPart projectSummaryWebPart = webPart as ProjectSummaryWebPart;
                string listTitle = mWeb.Lists[projectSummaryWebPart.ListId].Title;
                if (!string.IsNullOrEmpty(listTitle))
                {
                    extensionProperties["ExtensionListTitle"] = listTitle;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting  ProjectSummaryWebPart extension info. Web Part ID: {0}. Error: {1}", webPart.ID, ex.ToString());
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
    }

    public class AveBlogLinksWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AveBlogLinksWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                BlogLinksWebPart blogLinksWebPart = webPart as BlogLinksWebPart;
                string listTitle = mWeb.Lists[blogLinksWebPart.ListId].Title;
                if (!string.IsNullOrEmpty(listTitle))
                {
                    extensionProperties["ExtensionListTitle"] = listTitle;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting  BlogLinksWebPart extension info. Web Part ID: {0}. Error: {1}", webPart.ID, ex.ToString());
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
    }

    public class AveContentByQueryWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AveContentByQueryWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        /// <summary>
        /// 如果WebPart FilterType为Taxonomy类型，将Filter相关的WebPart属性组装Xml，再在AveSPDoc中备份Terms数据。
        /// </summary>
        /// <param name="webPart"></param>
        /// <returns></returns>
        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                ContentByQueryWebPart queryWebPart = webPart as ContentByQueryWebPart;

                string mmsData = GetQueryWebPartMMSInfo(queryWebPart);

                if (!string.IsNullOrWhiteSpace(mmsData))
                {
                    extensionProperties["WebPartMMSData"] = mmsData;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting ContentByQueryWebPart extension info.Page url: {0}, web part ID:{1},error :{2}", this.mManager.File.Url, webPart.ID, ex);
            }
            return base.GetWebPartExtensionInfo(webPart);
        }

        /// <summary>
        /// 如果WebPart FilterType为Taxonomy类型，将Filter相关的WebPart属性组装Xml
        /// </summary>
        /// <param name="queryWebPart"></param>
        /// <returns></returns>
        private string GetQueryWebPartMMSInfo(ContentByQueryWebPart queryWebPart)
        {
            var xDoc = new XmlDocument();
            try
            {
                if (IsTaxonomyFilterType(queryWebPart.FilterType1))
                {
                    InitRootElement(queryWebPart, xDoc);
                    AppendFilterElement("filter1", queryWebPart.FilterField1, queryWebPart.FilterDisplayValue1, xDoc);
                }
                if (IsTaxonomyFilterType(queryWebPart.FilterType2))
                {
                    if (xDoc.DocumentElement == null)
                    {
                        InitRootElement(queryWebPart, xDoc);
                    }
                    AppendFilterElement("filter2", queryWebPart.FilterField2, queryWebPart.FilterDisplayValue2, xDoc);
                }
                if (IsTaxonomyFilterType(queryWebPart.FilterType3))
                {
                    if (xDoc.DocumentElement == null)
                    {
                        InitRootElement(queryWebPart, xDoc);
                    }
                    AppendFilterElement("filter3", queryWebPart.FilterField3, queryWebPart.FilterDisplayValue3, xDoc);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting ContentByQueryWebPart mms metadata,error {0},web part ID:{1},page url: {2}", ex, queryWebPart.ID, this.mManager.File.Url);
            }

            return xDoc.OuterXml;
        }

        private bool IsTaxonomyFilterType(string type)
        {
            if (!string.IsNullOrWhiteSpace(type) && (type.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                || type.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 初始化root element，包含FilterField关联的Web以及List数据
        /// </summary>
        /// <param name="queryWebPart"></param>
        /// <param name="xDoc"></param>
        private void InitRootElement(ContentByQueryWebPart queryWebPart, XmlDocument xDoc)
        {
            var rootElement = xDoc.CreateElement("ContentByQueryWebPart");

            if (!string.IsNullOrWhiteSpace(queryWebPart.WebUrl))
            {
                rootElement.SetAttribute("WebUrl", queryWebPart.WebUrl);
            }
            if (!string.IsNullOrWhiteSpace(queryWebPart.ListGuid))
            {
                rootElement.SetAttribute("ListGuid", queryWebPart.ListGuid);
            }
            if (!string.IsNullOrWhiteSpace(queryWebPart.ListName))
            {
                rootElement.SetAttribute("ListName", queryWebPart.ListName);
            }

            xDoc.AppendChild(rootElement);
        }

        /// <summary>
        ///在Root节点下Append一个Filter节点
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="filterField"></param>
        /// <param name="filterDisplayValue"></param>
        /// <param name="xDoc"></param>
        private void AppendFilterElement(string tagName, string filterField, string filterDisplayValue, XmlDocument xDoc)
        {

            var xElement = xDoc.CreateElement(tagName);

            xElement.SetAttribute("FilterField", filterField);
            xElement.SetAttribute("FilterDisplayValue", filterDisplayValue);

            xDoc.DocumentElement.AppendChild(xElement);

        }

        /// <summary>
        /// 向xElement添加属性
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <param name="xDoc"></param>
        /// <param name="xElement"></param>
    }


    public class AvePictureLibrarySlideshowWebPartExtensionHandler : AveWebPartExtensionHandler
    {
        public AvePictureLibrarySlideshowWebPartExtensionHandler(AveLimitedWebPartManager aveManager)
            : base(aveManager) { }

        public override Dictionary<string, string> GetWebPartExtensionInfo(UIWebPart webPart)
        {
            try
            {
                PictureLibrarySlideshowWebPart pictureLibrarySlideshowWebPart = webPart as PictureLibrarySlideshowWebPart;
                SPList picLib = mWeb.Web.Lists[pictureLibrarySlideshowWebPart.LibraryGuid];
                string title = picLib.Title;
                if (!string.IsNullOrEmpty(title))
                {
                    extensionProperties["ExtensionLibraryTitle"] = title;
                }
                title = picLib.Views[pictureLibrarySlideshowWebPart.ViewGuid].Title;
                if (!string.IsNullOrEmpty(title))
                {
                    extensionProperties["ExtensionViewTitle"] = title;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting  BlogLinksWebPart extension info. Web Part ID: {0}. Error: {1}", webPart.ID, ex.ToString());
            }
            return base.GetWebPartExtensionInfo(webPart);
        }
    }
}
