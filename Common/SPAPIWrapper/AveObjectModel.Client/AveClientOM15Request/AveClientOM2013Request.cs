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




using AveClientRequest.Common;
using AvePoint.Common.Portal;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.ObjectModel.PSI;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Resource.Client;
using CamlBuilder;
using Cloud.Sdk.CloudInsights;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.ApplicationPages.ClientPickerQuery;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.CompliancePolicy;
//using Microsoft.Azure.ActiveDirectory.Client.Framework;
using Microsoft.SharePoint.Client.DocumentManagement;
using Microsoft.SharePoint.Client.DocumentSet;
using Microsoft.SharePoint.Client.RecordsRepository;
using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client.UserProfiles;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using Microsoft.SharePoint.Client.WorkflowServices;
using Microsoft365.Authentication;
using Microsoft365.SharePoint.CSOM.Extension;
using Microsoft365.SharePoint.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PnP.Framework.ALM;
using PnP.Framework.Enums;
using PnP.Framework.Utilities.REST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Util.MIP;
using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
using ClientFile = Microsoft.SharePoint.Client.File;
using ClientFolder = Microsoft.SharePoint.Client.Folder;
using RE = System.Text.RegularExpressions;
using SPChangeType = Microsoft.SharePoint.Client.ChangeType;

namespace AvePoint.ObjectModel.ClientOM
{
    internal delegate ClientObjectData GetObjectData(ClientObject clientObject);

    public static class AveClientContentExtension
    {
        private static readonly PropertyInfo stringvalue;
        static AveClientContentExtension()
        {
            var type = typeof(ClientContext);
            stringvalue = type.GetProperty("ObjectPaths", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        public static Dictionary<long, ObjectPath> GetObjectPaths(this ClientContext context)
        {
            return stringvalue.GetValue(context) as Dictionary<long, ObjectPath>;
        }
    }

    public partial class AveClientOM2013Request : IAveRequest, IDisposable
    {
        #region AveClientOM2013Request
        private const int RowIdStep = 2000;
        private const int CACHE_TIME_OUT = 5 * 60 * 1000;
        private const int LARGE_FILE_BLOCK_SIZE = 50 * 1024 * 1024;//50M
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOM2013Request));
        private string mWebUrl;
        private string mWebAppName;
        public static List<string> SpecialFileList = new List<string>() { ".master", ".evtx", ".cs", ".xoml", ".rules", ".aspx", ".wsp", ".js", ".css", ".html", ".htm" };
        private static Dictionary<string, int> systempListsToSkipDeletion;
        private static Dictionary<string, List<string>> listsActivatedByFeatureToSkipDeletion;     //these lists are created by activating features and can not be deleted
        private ITokenProvider tokenProvider;
        private string mServerVersion;
        private string mInternalServerVersion;
        private string mSiteRelativeUrl;
        private uint maxItemsPerThrottledOperation;
        private AveBPOSAccountInfo mUserAccountInfo;
        //private AuthenticationMode mAuthMode;
        private ClientContext mFormDigestContext;
        private AveWebServiceRequest mWebServiceRequest;
        private AvePSIRequest mPSIRequest;
        private IAveHttpWebRequestCommon mRequestCommon;
        private int CompatibilityLevel = 15;
        /// <summary>
        /// 主要load非root folder的unique id属性
        /// </summary>
        private static bool mIsLoadDFolderId = false;
        private const int ListViewThreshold = 5000;
        private readonly static object mLockObj = new object();
        private readonly static object mCacheLockObj = new object();
        private readonly static object mlatUtilityLockObj = new object();
        private IReportService reportService;
        private DateTime latMgtApiEnableTime;
        private string lastSiteUrl = string.Empty;
        private LastAccessTimeSqliteDBUtility lastAccessTimeSqliteDBUtility;
        private AveNintexAPIProcessor nintexAPIProcessor;
        private readonly object tokenLockObj = new object();
        private readonly static object declareLockObj = new object();
        private Guid mTenantId = Guid.Empty;
        private string mTenantGroupId = string.Empty;
        //ensure formdigestvaue only be fetched once
        private MIPService mMIPService;

        static AveClientOM2013Request()
        {
            InitScriptTypeMap();
            InitSkipDeletionLists();
        }

        private static void InitScriptTypeMap()
        {
            try
            {
                string TaxonomyAssemblyPath = typeof(TaxonomyField).Assembly.Location;
                Type type = typeof(ClientRuntimeContext).Assembly.GetType("Microsoft.SharePoint.Client.ScriptTypeMap");
                AvePoint.Common.Invoker.CallStaticMethod(type, "EnsureInited");
                object value = type.GetField("s_scriptTypeFactories", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                mLogger.Info("IScriptTypeFactory count: {0}", (value as List<IScriptTypeFactory>).Count);
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to init script type map, error info: {0}", e.ToString());
            }
        }

        private static void InitSkipDeletionLists()
        {
            systempListsToSkipDeletion = new Dictionary<string, int>
            {
                { "OData__x005f_catalogs_x002f_appdata", 125 },
                { "OData__x005f_catalogs_x002f_design", 124 },
                { "UserInfo", 112 },
                { "OData__x005f_catalogs_x002f_lt", 114 },
                { "OData__x005f_catalogs_x002f_masterpage", 116 },
                { "OData__x005f_catalogs_x002f_solutions", 121 },
                { "OData__x005f_catalogs_x002f_theme", 123 },
                { "OData__x005f_catalogs_x002f_wp", 113 },
                { "Style_x0020_Library", 101 }
            };
            listsActivatedByFeatureToSkipDeletion = new Dictionary<string, List<string>>();
            listsActivatedByFeatureToSkipDeletion["00bfea71-de22-43b2-a848-c05709900100"] = new List<string>()
            {
                "BadgesList", "Cache_x0020_Profiles", "Reports_x0020_List", "RoutingRules", "ContentTypeSyncLogList",
                "ContentTypeAppLogList", "DeviceChannels", "HoldsList", "Long_x0020_Running_x0020_Operation_x0020_Status",
                "Notification_x0020_Pages", "Quick_x0020_Deploy_x0020_Items", "Relationships_x0020_List",
                "ReusableContent", "PackageListList", "PublishedLinks", "TaxonomyHiddenListList",
                "Translation_x0020_Status", "Variation_x0020_Labels"
            };
            listsActivatedByFeatureToSkipDeletion["00bfea71-a83e-497e-9ba0-7a5c597d0107"] = new List<string>()
            {
                "WorkflowTasks","SiteCollectionDocuments","Style_x0020_Library","Translation_x0020_Packages",
                "DropOffLibrary","FormServerTemplates","HoldReports"
            };
            listsActivatedByFeatureToSkipDeletion["4bcccd62-dcaf-46dc-a7d4-e38277ef33f4"] = new List<string>()
            {
                "SiteCollectionImages","PublishingImages"
            };
            listsActivatedByFeatureToSkipDeletion["d32700c7-9ec5-45e6-9c89-ea703efca1df"] = new List<string>() { "CategoriesList" };
            listsActivatedByFeatureToSkipDeletion["947afd14-0ea1-46c6-be97-dea1bf6f5bae"] = new List<string>() { "MembersList" };
            listsActivatedByFeatureToSkipDeletion["3016e6bf-cfe2-4b9d-bfd0-41a1d1d62ab8"] = new List<string>() { "AnnouncementTilesList" };
            listsActivatedByFeatureToSkipDeletion["a0e5a010-1329-49d4-9e09-f280cdbed37d"] = new List<string>() { "IWConvertedForms" };
            listsActivatedByFeatureToSkipDeletion["00bfea71-6a49-43fa-b535-d15c05500108"] = new List<string>() { "Community_x0020_DiscussionList" };
            listsActivatedByFeatureToSkipDeletion["ea23650b-0340-4708-b465-441a41c37af7"] = new List<string>() { "PublishedFeedList" };
            listsActivatedByFeatureToSkipDeletion["22a9ef51-737b-4ff2-9346-694633fe4416"] = new List<string>() { "Pages" };
            listsActivatedByFeatureToSkipDeletion["c6a92dbf-6441-4b8b-882f-8d97cb12c83a"] = new List<string>() { "AbuseReportsList" };
        }

        private AveClientObjectsCache mCurrentList;

        internal string WebAppName
        {
            get
            {
                if (mWebAppName == null)
                {
                    int indexOfSlash = mWebUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mWebUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mWebUrl.Substring(0, mWebUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }

        public ITokenProvider TokenProvider
        {
            get
            {
                return tokenProvider;
            }
            set
            {
                tokenProvider = value;
            }
        }

        public string Url
        {
            get
            {
                return this.mWebUrl;
            }
        }

        public AveRequestKind Kind
        {
            get
            {
                return AveRequestKind.ClientObjectModel;
            }
        }

        public string SPVersion
        {
            get
            {
                return mServerVersion;
            }
        }

        public string OriginalUrl { get; set; }

        public AveBPOSAccountInfo BposInfo
        {
            get
            {
                return mUserAccountInfo;
            }
        }
        public IAveWebServiceRequestOnline WebServiceRequestOnline
        {
            get
            {
                return mWebServiceRequest;
            }
        }

        public AveClientOM2013Request(string url, AveBPOSAccountInfo userAccountInfo)
        {
            this.tokenProvider = userAccountInfo.Convert2TokenProvider();
            mWebUrl = url;
            mUserAccountInfo = userAccountInfo;
            try
            {
                mTenantId = new Guid(userAccountInfo.TenantId);
            }
            catch (Exception e)
            {
                mLogger.Error($"error occured when AveClientOM2013Request,error:{e}");
            }

            try
            {
                mTenantGroupId = userAccountInfo.TenantGroupId;
            }
            catch (Exception e)
            {
                mLogger.Error($"error occured when AveClientOM2013Request2,error:{e}");
            }
            //mAuthMode = (AuthenticationMode)mode;
            //mServerVersion = serverVersion;
            mInternalServerVersion = "15";
            mFormDigestContext = InitClientObject(url);
            mWebServiceRequest = new AveWebServiceRequest(url, userAccountInfo, tokenProvider);
            mPSIRequest = new AvePSIRequest(url, tokenProvider);
            nintexAPIProcessor = new AveNintexAPIProcessor(url, tokenProvider, Nintex.O365API.APIMethod.HTTP);

            this.mCurrentList = AveClientObjectsCache.NewCache;

        }
        
        public AveClientOM2013Request(string url, AveBPOSAccountInfo userAccountInfo, bool isSpoExportService)
        {
            this.tokenProvider = userAccountInfo.Convert2TokenProvider();
            mWebUrl = url;
            mUserAccountInfo = userAccountInfo;
            try
            {
                mTenantId = new Guid(userAccountInfo.TenantId);
            }
            catch (Exception e)
            {
                mLogger.Error($"error occured when AveClientOM2013Request,error:{e}");
            }

            try
            {
                mTenantGroupId = userAccountInfo.TenantGroupId;
            }
            catch (Exception e)
            {
                mLogger.Error($"error occured when AveClientOM2013Request1,error:{e}");
            }
            //mAuthMode = (AuthenticationMode)mode;
            //mServerVersion = serverVersion;
            mInternalServerVersion = "15";
            mFormDigestContext = InitClientObject(url);
        }

        public void InitMIPService(string office365TenantId, string workingUser, Util.MIP.Cloud cloudLocation)
        {
            mLogger.Info("Init mip service.");
            MIPServiceImp.Instance.Init(office365TenantId, workingUser, cloudLocation, GetToken);
            mMIPService = MIPServiceImp.Instance.GetService();
        }
        private AveClientContext InitDeleteFileVersionClientObject()
        {
            var context = new AveClientContext(mWebUrl, mTenantId.ToString(), ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            context.RequestTimeout = 3600000 * 24; //24 hours
            SetContextInfo(context);
            return context;
        }
        private string GetToken(string scope)
        {
            return tokenProvider.GetToken(new Uri(scope));
        }

        private AveClientContext InitClientObject(string url)
        {
            var context = new AveClientContext(url, mTenantId.ToString(), ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;//ten miniutes
            SetContextInfo(context);
            return context;
        }
        /// <summary>
        /// Can not be used to Restore.
        /// </summary>
        /// <returns></returns>
        private AveClientContextForDiscover CreateDiscoverContext()
        {
            return InitDiscoverClientContext(mWebUrl);
        }

        private AveClientContextForDiscover InitDiscoverClientContext(string url)
        {
            var context = new AveClientContextForDiscover(url, mTenantId.ToString(), ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            SetContextInfo(context);
            context.RefreshToken(() =>
            {
                context.ResetContext(TokenProvider);
            });
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;//ten miniutes
            return context;
        }

        private (Guid tenantId, string defaultAppId) GetTenantIdAndDefaultAppIdFunc()
        {
            return (mTenantId, mUserAccountInfo?.ClientId);
        }

        /// <summary>
        /// 经过了解，DAO不支持跑job过程中切换user.
        /// </summary>
        /// <param name="request"></param>
        private void ChangeTokenProvider(WebRequest request)
        {
            if (!AveAppProfileUtility.HasInit(mTenantId))
            {
                mLogger.Warn("Multiple app profile is not enabled for tenant:{0}", mTenantId);
                return;
            }

            lock (tokenLockObj)
            {
                try
                {
                    AveAppProfileUtility.SetBlockStatus(mTenantId,1800);
                    var bposInfo = AveAppProfileUtility.ChangeAppProfile(mTenantId);
                    if (bposInfo != null)
                    {
                        mLogger.Info("current app profile id:{0} tenant id:{1}", bposInfo.UserAccountInfo?.AppClientId, mTenantId);
                        var appProfileProvider = bposInfo.ConvertToAveBPOSAccountInfo().Convert2TokenProvider();
                        request.SetTokenProvider(this.mWebUrl, appProfileProvider);
                        if (request is ReliableHttpWebRequest webRequest)
                        {
                            webRequest.SetRefreshTokenProvider(mWebUrl, appProfileProvider);
                            mLogger.Info(@$"change app form {webRequest.CurrentAppId} to {bposInfo.UserAccountInfo?.AppClientId}");
                            webRequest.LastAppId = webRequest.CurrentAppId;
                            webRequest.CurrentAppId = bposInfo.UserAccountInfo?.AppClientId;
                        }
                        mLogger.Info("change token for request finished");
                    }
                    else
                    {
                        mLogger.Info("bposInfo is null");
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("change app profile failed, error:{0}", e);
                }
            }
        }

        //private ITokenProvider GetBestTokenProvider()
        //{
        //    if (AveAppProfileUtility.HasInit(mTenantId))
        //    {
        //        lock (tokenLockObj)
        //        {
        //            try
        //            {
        //                var bposInfo = AveAppProfileUtility.ChangeAppProfile(mTenantId);
        //                if (bposInfo != null)
        //                {
        //                    mLogger.Info("Get best app profile, client id:{0} tenant id:{1}", bposInfo.UserAccountInfo?.AppClientId, mTenantId);
        //                    return bposInfo.ConvertToAveBPOSAccountInfo()?.Convert2TokenProvider();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                mLogger.Warn("An error occurred while getting best app profile, error:{0}", e.ToString());
        //            }
        //        }
        //    }
        //    return null;
        //}

        private void SetContextInfo(ClientContext context)
        {
            //var tempToken = GetBestTokenProvider();
            //if (tempToken != null)
            //{
            //    tokenProvider = tempToken;
            //}
            //if (tokenProvider.TokenType == TokenType.Bearer)
            //{
            //    context.FormDigestHandlingEnabled = false;
            //}
            //else
            //{
                context.SetFormDigest();
            //}
            context.SetTokenProvider(tokenProvider);
        }

        private AveClientContext CreateContext()
        {
            return CreateContext(mWebUrl);
        }

        private ClientContext CreateSimpleContext()
        {
            var context = new ClientContext(mWebUrl);
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;//ten miniutes
            SetContextInfo(context);
            return context;
        }

        private AveClientContext CreateContext(string weburl)
        {
            AveClientContext context = InitClientObject(weburl);
            return context;
        }

        private AveRetryClientContext CreateRetryContext()
        {
            return InitRetryClientContext(mWebUrl);
        }

        private AveRetryClientContext CreateRetryContext(string url)
        {
            return InitRetryClientContext(url);
        }

        private AveRetryClientContext InitRetryClientContext(string url)
        {
            var context = new AveRetryClientContext(url, mTenantId.ToString(), ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            //set timeout to 20 mins
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout * 2;
            SetContextInfo(context);
            context.RefreshToken(() =>
            {
                context.ResetContext(tokenProvider);
            });
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;
            return context;
        }

        public void Dispose()
        {
            DisposeCache();
        }

        public void DisposeCache()
        {
            lastSiteUrl = "";
            if (this.mCurrentList != null)
            {
                this.mCurrentList.Dispose();
            }
            if (this.lastAccessTimeSqliteDBUtility != null)
            {
                this.lastAccessTimeSqliteDBUtility.Dispose();
            }
        }

        #endregion AveClientOM2013Request

        #region Get

        public virtual Dictionary<string, AveListItemConflictBaseInfo> GetItemsForConflict(string webServerRelativeUrl, Guid siteId, Guid webId, string listName, Guid listId, string[] camlQueryNode)
        {
            using (AveClientContext context = CreateContext())
            {
                bool isLoadItemProperty = false;
                Dictionary<string, AveListItemConflictBaseInfo> listItemsCollection = new Dictionary<string, AveListItemConflictBaseInfo>(StringComparer.OrdinalIgnoreCase);
                bool loadAllItems = true;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = camlQueryNode[3];
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    camlQuery.FolderServerRelativeUrl = camlQueryNode[4];
                }
                ListItemCollectionPosition listItemCollectionPosition = null;
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    listItemCollectionPosition = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlQuery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }
                var objectPaths = new Dictionary<long, ObjectPath>(context.GetObjectPaths());
                do
                {
                    camlQuery.ListItemCollectionPosition = listItemCollectionPosition;
                    ListItemCollection items = list.GetItems(camlQuery);
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition);
                        }
                        using (ehScope.StartCatch())
                        {
                            context.Load(items);
                            if (isLoadItemProperty)
                            {
                                context.Load(items, its => its.ListItemCollectionPosition,
                                                    its => its.Include(t => t.HasUniqueRoleAssignments));//SAAS-6084 DisplayName not support discussion board
                            }
                            else
                            {
                                context.Load(items, its => its.ListItemCollectionPosition);
                            }
                        }
                    }

                    context.ExecuteQuery();
                    if (ehScope.HasException)
                    {
                        mLogger.Warn("load item failed due to: {0}", ehScope.ErrorMessage);
                    }
                    foreach (ListItem item in items)
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        GetItemConflictDic(itemProperties, item);
                        AveListItemConflictBaseInfo itemInfo = new AveListItemConflictBaseInfo(this, webServerRelativeUrl, itemProperties);
                        listItemsCollection[itemInfo.ServerRelativeUrl] = itemInfo;
                    }
                    listItemCollectionPosition = items.ListItemCollectionPosition;
                    var tempCache = context.GetObjectPaths();
                    tempCache.Clear();
                    foreach (var path in objectPaths)
                    {
                        tempCache[path.Key] = path.Value;
                    }
                }
                while (listItemCollectionPosition != null && loadAllItems);
                return listItemsCollection;
            }
        }

        public virtual IEnumerable<List<AveListItemConflictBaseInfo>> GetItemsForConflictByBatch(string webServerRelativeUrl, Guid siteId, Guid webId, string listName, Guid listId, string[] camlQueryNode, int batchSize)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = camlQueryNode[3];
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    camlQuery.FolderServerRelativeUrl = camlQueryNode[4];
                }
                ListItemCollectionPosition listItemCollectionPosition = null;
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    listItemCollectionPosition = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlQuery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }
                //var objectPaths = new Dictionary<long, ObjectPath>(context.GetObjectPaths());
                do
                {
                    camlQuery.ListItemCollectionPosition = listItemCollectionPosition;
                    ListItemCollection items = list.GetItems(camlQuery);
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition);
                        }
                        using (ehScope.StartCatch())
                        {
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition);
                        }
                    }

                    context.ExecuteQuery();
                    if (ehScope.HasException)
                    {
                        mLogger.Warn("GetItemsForConflictByBatch load item failed due to: {0}", ehScope.ErrorMessage);
                    }

                    List<AveListItemConflictBaseInfo> batch = new List<AveListItemConflictBaseInfo>(batchSize);
                    foreach (ListItem item in items)
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        GetItemConflictDic(itemProperties, item);
                        AveListItemConflictBaseInfo itemInfo = new AveListItemConflictBaseInfo(this, webServerRelativeUrl, itemProperties);
                        batch.Add(itemInfo);
                    }

                    yield return batch;

                    listItemCollectionPosition = items.ListItemCollectionPosition;
                    //var tempCache = context.GetObjectPaths();
                    //tempCache.Clear();
                    //foreach (var path in objectPaths)
                    //{
                    //    tempCache[path.Key] = path.Value;
                    //}
                }
                while (listItemCollectionPosition != null);
            }
        }

        public void GetItemConflictDic(Dictionary<string, object> itemProperties, ListItem item)
        {
            if (item.FieldValues.Count > 0)
            {
                //item properties
                foreach (KeyValuePair<string, object> pair in item.FieldValues)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    if (pair.Value.GetType().FullName == "System.String")
                    {
                        if (string.IsNullOrEmpty(pair.Value.ToString()))
                        {
                            continue;
                        }
                    }
                    switch (pair.Key)
                    {
                        case "FileRef":
                            itemProperties["ServerRelativeUrl"] = pair.Value.ToString();
                            break;
                        case "Created":
                            itemProperties["TimeCreated"] = AssignTimeKind((DateTime)pair.Value);
                            break;
                        case "ParentUniqueId":
                            itemProperties["ParentUniqueId"] = pair.Value;
                            break;
                        case "Modified":
                            itemProperties["TimeLastModified"] = AssignTimeKind((DateTime)pair.Value);
                            break;
                        case "FSObjType":
                            itemProperties["FileSystemObjectType"] = int.Parse(pair.Value.ToString());
                            break;
                        case "ID":
                            itemProperties["ID"] = pair.Value;
                            break;
                        case "_Level":
                            itemProperties["LEVEL"] = byte.Parse(pair.Value.ToString());
                            break;
                        case "GUID":
                        case "UniqueId":
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        case "Author":
                        case "Editor":
                            itemProperties[pair.Key] = (pair.Value as FieldUserValue)?.LookupId;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        protected DateTime AssignTimeKind(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local) : dateTime;
        }

        public DateTime QueryLastAccessTime(Guid itemId, string folderServerRelativeUrl, DateTime modifiedTime, bool isCompatibleByModifiedTime = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.QueryLastAccessTime"))
            {
                string siteUrl = WebAppName.TrimEnd('/') + mSiteRelativeUrl;
                DateTime itemLastAccessTime = DateTime.MinValue;
                string cloudInsightsApiUrl = GCommonRoleConfiguration.PortalCloudInsightsApiURL;
                try
                {
                    if (reportService == null)
                    {
#if DEBUG
                        cloudInsightsApiUrl = "https://graph.sharepointguild.com/cloudinsights";
#endif
                        reportService = AvePoint.GCommon.Utility.AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(cloudInsightsApiUrl, mTenantGroupId).ReportService;
                    }
                    lock (mlatUtilityLockObj)
                    {
                        if (!string.Equals(lastSiteUrl, siteUrl))
                        {
                            latMgtApiEnableTime = new DateTime(PortalUtil.Execute(() => reportService.GetMgtApiEnableTimeByTenantId(mTenantId.ToString())));
                            mLogger.Info($"reportService CheckPrerequisites documentLatCheckResult:{latMgtApiEnableTime}.o365TenantId:{mTenantId}.tenantGroupId:{mTenantGroupId}.");
                            if (WrapperConfiguration.EnableDownloadLATData)
                            {
                                mLogger.Info("enable download lat is true");
                                //new interfacce
                                var scLAT = (PortalUtil.Execute(() => reportService.GetDocumentLastAccessTime(new List<string>() { siteUrl }))).FirstOrDefault();
                                mLogger.Info($"reportService CheckPrerequisites SASUrlIsEmpty:{string.IsNullOrEmpty(scLAT?.StorageSasUrl)}.TableName:{scLAT?.TableName}.SASFileName:{scLAT?.FileName}.");
                                LastAccessTimeSqliteDBUtility.ClearInstance();
                                lastAccessTimeSqliteDBUtility = LastAccessTimeSqliteDBUtility.GetInstance(mTenantGroupId, scLAT?.StorageSasUrl, scLAT?.TableName);
                            }
                            lastSiteUrl = siteUrl;
                        }
                    }
                    if (latMgtApiEnableTime != DateTime.MinValue)
                    {
                        if (WrapperConfiguration.EnableDownloadLATData)
                        {
                            itemLastAccessTime = GetItemLastAccessTimeV2(itemId, modifiedTime, isCompatibleByModifiedTime);
                        }
                        else
                        {
                            itemLastAccessTime = GetItemLastAccessTimeV1(itemId, siteUrl, folderServerRelativeUrl, modifiedTime, isCompatibleByModifiedTime);
                        }
                    }
                    else
                    {
                        //modify for SAAS-23181,由于DateTime这种声明方式不允许将time赋值为空，所以将itemLastAccessTime赋一个默认值。
                        mLogger.Warn("The site do not meet the conditions.The file {0} has no lastAccessTime.", itemId);
                        throw new Exception("The site do not meet the conditions.");
                        //itemLastAccessTime = new DateTime(1900, 01, 01, 00, 00, 00);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error("Query Last Access Time has a error, error is:{0}", ex.ToString());
                    throw new Exception("The site do not meet the conditions.");
                }
                return itemLastAccessTime;
            }
        }

        private DateTime GetItemLastAccessTimeV1(Guid itemId, string siteUrl, string folderServerRelativeUrl, DateTime modifiedTime, bool isCompatibleByModifiedTime = false)
        {
            DateTime itemLastAccessTime;
            lock (mCacheLockObj)
            {   //Lock防止多线程重复初始化LAT缓存
                if (!this.mCurrentList.FoldersToSubItemLastAccessTime.ContainsKey(folderServerRelativeUrl))
                {
                    mLogger.Info("process FoldersToSubItemLastAccessTime:{0}", folderServerRelativeUrl);
                    if (!this.mCurrentList.FoldersToSubItemUniqueIds.ContainsKey(folderServerRelativeUrl))
                    {
                        mLogger.Info("Current item:{0} doesn't exist in FoldersToSubItemUniqueIds.Url:{1}.", itemId, folderServerRelativeUrl);
                        List<string> itemIds = new List<string>();
                        itemIds.Add(itemId.ToString());
                        //Dictionary<string, DateTime> lastAccessTimes = documentAccessService.GetLastAccessedTime(siteUrl, itemIds);
                        Dictionary<string, long> lastAccessTimes = PortalUtil.Execute(() => reportService.GetDocumentLat(new Cloud.Sdk.Data.CloudInsights.DocumentLatModel() { SiteUrl = siteUrl, ItemIds = itemIds }));
                        if (lastAccessTimes.ContainsKey(itemId.ToString()))
                        {
                            itemLastAccessTime = new DateTime(lastAccessTimes[itemId.ToString()]);
                            return itemLastAccessTime;
                        }
                        itemLastAccessTime = modifiedTime > latMgtApiEnableTime ? modifiedTime : latMgtApiEnableTime;
                        return itemLastAccessTime;
                    }

                    List<string> itemUniqueIds = this.mCurrentList.FoldersToSubItemUniqueIds[folderServerRelativeUrl].ToList<string>();
                    mLogger.Info("Current folder:{0} items count is:{1}.", folderServerRelativeUrl, itemUniqueIds.Count);
                    if (itemUniqueIds.Count <= 1000)
                    {
                        //this.mCurrentList.FoldersToSubItemLastAccessTime[folderServerRelativeUrl] = documentAccessService.GetLastAccessedTime(siteUrl, itemUniqueIds);
                        this.mCurrentList.FoldersToSubItemLastAccessTime[folderServerRelativeUrl] = PortalUtil.Execute(() => reportService.GetDocumentLat(new Cloud.Sdk.Data.CloudInsights.DocumentLatModel() { SiteUrl = siteUrl, ItemIds = itemUniqueIds }));
                    }
                    else
                    {
                        int index = 0;
                        int count = 1000;//每次取得最大个数是1000
                        this.mCurrentList.FoldersToSubItemLastAccessTime[folderServerRelativeUrl] = new Dictionary<string, long>();
                        do
                        {
                            List<string> tempItemUniqueIds = itemUniqueIds.GetRange(index, count);
                            //Dictionary<string, DateTime> tempItemLats = new Dictionary<string, DateTime>(count);
                            //tempItemLats = documentAccessService.GetLastAccessedTime(siteUrl, tempItemUniqueIds);
                            Dictionary<string, long> tempItemLats = new Dictionary<string, long>(count);
                            tempItemLats = PortalUtil.Execute(() => reportService.GetDocumentLat(new Cloud.Sdk.Data.CloudInsights.DocumentLatModel() { SiteUrl = siteUrl, ItemIds = tempItemUniqueIds }));
                            foreach (KeyValuePair<string, long> tempItemLat in tempItemLats)
                            {
                                this.mCurrentList.FoldersToSubItemLastAccessTime[folderServerRelativeUrl].Add(tempItemLat.Key, tempItemLat.Value);
                            }
                            index += count;
                            count = (itemUniqueIds.Count - index) > count ? count : (itemUniqueIds.Count - index);
                        } while (count > 0);
                    }
                }
            }
            Dictionary<string, long> itemLat = this.mCurrentList.FoldersToSubItemLastAccessTime[folderServerRelativeUrl];
            if (itemLat.ContainsKey(itemId.ToString()))
            {
                itemLastAccessTime = new DateTime(itemLat[itemId.ToString()]);
                return itemLastAccessTime;
            }
            if (isCompatibleByModifiedTime)
            {
                mLogger.Info("Because compatibility, directly use modifiedTime, itemid {0}  {1}", itemId.ToString(), modifiedTime.ToString());
                return modifiedTime;
            }
            itemLastAccessTime = modifiedTime > latMgtApiEnableTime ? modifiedTime : latMgtApiEnableTime;
            return itemLastAccessTime;
        }

        private DateTime GetItemLastAccessTimeV2(Guid itemId, DateTime modifiedTime, bool isCompatibleByModifiedTime = false)
        {
            DateTime itemLastAccessTime = DateTime.MinValue;
            Stopwatch watch = Stopwatch.StartNew();
            lastAccessTimeSqliteDBUtility.ExecuteQueryWithAction(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    itemLastAccessTime = lastAccessTimeSqliteDBUtility.SelectItemLastAccessedTimeFromSqliteDB(command, itemId);
                }
            });
            watch.Stop();   
            if (itemLastAccessTime != DateTime.MinValue)
            {
                mLogger.Info($"[QueryLastAccessTime]Use itemLastAccessTime, itemid {itemId}.LATTime:{itemLastAccessTime}.QueryTime:{watch.Elapsed}.");
                return itemLastAccessTime;
            }

            if (isCompatibleByModifiedTime)
            {
                mLogger.Info($"[QueryLastAccessTime]Because compatibility, directly use modifiedTime, itemid {itemId}.modifiedTime:{modifiedTime}.QueryTime:{watch.Elapsed}.");
                return modifiedTime;
            }

            itemLastAccessTime = modifiedTime > latMgtApiEnableTime ? modifiedTime : latMgtApiEnableTime;
            if (modifiedTime > latMgtApiEnableTime)
            {
                mLogger.Info($"[QueryLastAccessTime]Use modifiedTime, itemid {itemId}.modifiedTime:{modifiedTime}.QueryTime:{watch.Elapsed}.");
            }
            else
            {
                mLogger.Info($"[QueryLastAccessTime]Use latMgtApiEnableTime, itemid {itemId}.latMgtApiEnableTime:{latMgtApiEnableTime}.QueryTime:{watch.Elapsed}.");
            }
            return itemLastAccessTime;
        }

        public DateTime QueryLastAccessTime(string sitecollectionURL, DateTime? modifiedTime = null, bool isCompatibleByModifiedTime = false)
        {
            DateTime siteCollectionLastAccessTime;
            try
            {
                if (reportService == null)
                {
                    //string tenantId = System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("TenantGroupId").ToString();
                    reportService = AvePoint.GCommon.Utility.AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, mTenantGroupId).ReportService;
                }
                if (!string.Equals(lastSiteUrl, sitecollectionURL))
                {
                    latMgtApiEnableTime = new DateTime(PortalUtil.Execute(() => reportService.GetMgtApiEnableTimeByTenantId(mTenantId.ToString()))); 
                    mLogger.Info($"reportService CheckPrerequisites documentLatCheckResult:{latMgtApiEnableTime}.o365TenantId:{mTenantId}.tenantGroupId:{mTenantGroupId}.");
                    lastSiteUrl = sitecollectionURL;
                }
                if (latMgtApiEnableTime != DateTime.MinValue)
                {
                    siteCollectionLastAccessTime = new DateTime(PortalUtil.Execute(() => reportService.GetSiteCollectionLat(sitecollectionURL)));
                    if (siteCollectionLastAccessTime == DateTime.MinValue)
                    {
                        if (isCompatibleByModifiedTime)
                        {
                            mLogger.Warn("The site lastaccesstime has no value, because compatibility, directly use modifiedTime");
                            return modifiedTime.Value;
                        }
                        mLogger.Warn("The site lastaccesstime has no value .The sitecollection {0} has no lastAccessTime.", sitecollectionURL);
                        throw new Exception("The site do not meet the conditions.");
                    }
                    else
                    {
                        mLogger.Info("The sitecollection {0} has lastAccessTime {1}.", sitecollectionURL, siteCollectionLastAccessTime.ToString());
                    }
                }
                else
                {
                    //modify for SAAS-23181,由于DateTime这种声明方式不允许将time赋值为空，所以将itemLastAccessTime赋一个默认值。
                    mLogger.Warn("The site do not meet the conditions.The sitecollection {0} has no lastAccessTime.", sitecollectionURL);
                    throw new Exception("The site do not meet the conditions.");
                    //itemLastAccessTime = new DateTime(1900, 01, 01, 00, 00, 00);
                }
                return siteCollectionLastAccessTime;
            }
            catch (Exception ex)
            {
                mLogger.Error("Query Last Access Time has a error, error is:{0}", ex.ToString());
                throw new Exception("The site do not meet the conditions.");
            }
        }

        public bool ExistSCTermGroup()
        {
            try
            {
                using (var context = CreateRetryContext())
                {
                    TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(context);
                    TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                    TermGroup termGroup = termStore.GetSiteCollectionGroup(context.Site, false);
                    context.Load(termGroup);
                    context.ExecuteQuery();
                    return termGroup?.ServerObjectIsNull == false;
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"Fail check sc term group exist, ex:{e}");
                throw;
            }
        }

        public void UpdateSCTermGroupName(string name)
        {
            try
            {
                using (var context = CreateRetryContext())
                {
                    TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(context);
                    TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                    TermGroup termGroup = termStore.GetSiteCollectionGroup(context.Site, true);
                    termGroup.Name = name;
                    context.Load(termGroup);
                    context.ExecuteQuery();
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"Fail update sc term group name, ex:{e}");
                throw;
            }
        }

        public void DeleteSCTermGroup()
        {
            try
            {
                using (var context = CreateRetryContext())
                {
                    TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(context);
                    TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                    TermGroup termGroup = termStore.GetSiteCollectionGroup(context.Site, false);
                    context.Load(termGroup);
                    context.ExecuteQuery();

                    if (termGroup?.ServerObjectIsNull != false)
                    {
                        mLogger.Error($"sc term group is null");
                        return;
                    }

                    var termSets = termGroup.TermSets;
                    context.Load(termSets);
                    context.ExecuteQuery();
                    foreach (var termSet in termSets)
                    {
                        termSet.DeleteObject();
                    }

                    termGroup.DeleteObject();
                    termStore.CommitAll();
                    context.ExecuteQuery();
                }
            }
            catch(Exception e)
            {
                mLogger.Error($"Fail delete sc term group, ex:{e}");
                throw;
            }
        }

        public Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadListItemIDUrlCache(string webServerRelativeUrl, Guid listId)
        {
            mLogger.Info("LoadListItemIDUrlCache.WebUrl:{0},ListId:{1}", webServerRelativeUrl, listId);
            //Folder,File
            Dictionary<string, int> urlCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            //Folder/File/Item
            Dictionary<int, Guid> rowIdCache = new Dictionary<int, Guid>();
            using (var context = CreateRetryContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                context.Load(list, l => l.ItemCount, l => l.BaseType, l => l.BaseTemplate, l => l.Title);
                context.ExecuteQuery();

                int index = 0;
                int totalCount = list.ItemCount;
                int objectType = list.BaseType == BaseType.DocumentLibrary ? 2 : 1;
                ListItemCollection listItems = null;

                var performanceTimer = Stopwatch.StartNew();
                var timer = Stopwatch.StartNew();
                mLogger.Info("LoadListItemIDUrlCache start.List {0},ItemCount:{1},ListTemplate:{2}.", list.Title, list.ItemCount, list.BaseTemplate);
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    int lastIndex = index;
                    const string query = @"<View Scope=""RecursiveAll"">
                            <Query>
                                <Where>
                                    <And>
                                        <Gt>
                                            <FieldRef Name=""ID""/>
                                            <Value Type=""Integer"">{0}</Value>
                                        </Gt>
                                        <Leq>
                                            <FieldRef Name=""ID""/>
                                            <Value Type=""Integer"">{1}</Value>
                                        </Leq>
                                    </And>
                                </Where>
                            </Query>
                            <RowLimit>{2}</RowLimit>
                            </View>";

                    camlQuery.ViewXml = string.Format(query, index, index + RowIdStep, RowIdStep);
                    if (list.BaseTemplate == (int)ListTemplateType.UserInformation)
                    {
                        listItems = list.GetItems(CamlQuery.CreateAllItemsQuery());
                    }
                    else
                    {
                        listItems = list.GetItems(camlQuery);
            }
                    context.Load(listItems,
                        items => items.ListItemCollectionPosition,
                           items => items.Include(
                               item => item["UniqueId"],
                               item => item.Id,
                               item => item["FileLeafRef"],
                               item => item["FileDirRef"],
                               item => item.FileSystemObjectType));
                    context.ExecuteQuery();

            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                Guid uniqueId = new Guid(item["UniqueId"].ToString());
                int rowId = item.Id;
                string fileListRelativeUrl = (item["FileDirRef"] + "/" + item["FileLeafRef"]).Substring(webServerRelativeUrl.Length + 1);
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    rowIdCache[rowId] = uniqueId;
                    urlCache[fileListRelativeUrl] = rowId;

                }
                else if (list.BaseType == BaseType.GenericList)
                {
                    rowIdCache[rowId] = uniqueId;
                    if (item.FileSystemObjectType == FileSystemObjectType.Folder)
                    {
                        urlCache[fileListRelativeUrl] = rowId;
                    }
                }
                        index = index < listItems[i].Id ? listItems[i].Id : index;
            }
                    index = lastIndex + RowIdStep < index ? index : lastIndex + RowIdStep;
                    totalCount -= listItems.Count;

                    timer.Stop();

                    if (timer.ElapsedMilliseconds > CACHE_TIME_OUT)
                    {
                        mLogger.Info("LoadListItemIDUrlCache query longer than expected.Time:{0},CurrentRangeStartRowId:{1},Progess:{2}/{3}",
                            timer.Elapsed, timer.Elapsed, lastIndex, list.ItemCount - totalCount, list.ItemCount);
        }
                    timer.Restart();
                }
                while (totalCount > 0);
                performanceTimer.Stop();
                mLogger.Info("LoadListItemIDUrlCache finished.List {0},ItemCount:{1},Time Cost:{2},UrlCache count:{3},RowIdCache count:{4}",
                    list.Title, list.ItemCount, performanceTimer.Elapsed, urlCache.Count, rowIdCache.Count);
            }
            return new Tuple<Dictionary<string, int>, Dictionary<int, Guid>>(urlCache, rowIdCache);
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Site, MethodType.HttpRequest)]
        public bool GetSiteRssSetting()
        {
            return mRequestCommon.GetSiteRssSetting();
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.App, MethodType.CSOM)]
        public Dictionary<string, object> LoadAndInstallApp(string webServerRelativeUrl, Stream stream)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                AppInstance appInstance = web.LoadAndInstallApp(stream);
                context.Load(appInstance);
                context.ExecuteQuery();
                Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                CopyProperty(appInstanceProperties, appInstance);
                if (!string.IsNullOrEmpty(appInstance.AppWebFullUrl))
                {
                    appInstanceProperties["AppWebFullUrl"] = new Uri(appInstance.AppWebFullUrl);
                }
                Uri startPage = null;
                if (Uri.TryCreate(appInstance.StartPage, UriKind.RelativeOrAbsolute, out startPage))
                {
                    appInstanceProperties["StartPage"] = startPage;
                }
                return appInstanceProperties;
            }
        }

        [ClientOMRequest(ReadWrite.Write, MethodLevel.Tenant, MethodType.CSOM)]
        public void DeleteSite(string siteUrl)
        {
            //SAAS-13067 不用try语句，如果删除Site失败，直接抛出异常，提示job 有异常。
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                tenant.RemoveSite(siteUrl);
                context.ExecuteQuery();
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public ItemIdMapping GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, string listTitle, Guid listId)
        {
            const int CACHE_TIME_OUT = 10 * 60 * 1000;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            ItemIdMapping idMapping = new ItemIdMapping();
            idMapping.HasAttachment = false;

            idMapping.IdMapping = new Dictionary<string, int>();
            idMapping.AppendItemMapping = [];

            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);//.GetByTitle(listTitle);
                    context.Load(list, l => l.ItemCount, l => l.BaseTemplate);
                    context.ExecuteQuery();

                    if (list.BaseTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        return GetListItemGuidAndRowIdMappingsInSmallList(webServerRelativeUrl, rootFolderServerRelativeUrl, listTitle, listId);
                    }

                    int index = 0;
                    int totalCount = list.ItemCount;
                    do
                    {
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.ViewXml = string.Format(
                            "<View Scope='RecursiveAll'>" +
                            "<Query><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
                            "<ViewFields><FieldRef Name='GUID' /><FieldRef Name='ID' /><FieldRef Name='Attachments'/><FieldRef Name='FileDirRef' /></ViewField>" +
                            "<RowLimit>{2}</RowLimit>" +
                            "</View>", index, index + 50000, 5000);
                        int lastIndex = index;
                        camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(rootFolderServerRelativeUrl);

                        ListItemCollection items = list.GetItems(camlQuery);
                        context.Load(items, its => its
                            .Include(it => it.Id, 
                                it => it["GUID"], 
                                it => it["FileDirRef"], 
                                it => it["Attachments"], 
                                it => it.Properties
                                ));
                        context.ExecuteQuery();
                        foreach (ListItem item in items)
                        {
                            idMapping.HasAttachment = Convert.ToBoolean(item.FieldValues["Attachments"]) ? true : idMapping.HasAttachment;
                            string mappingKey = item.FieldValues["GUID"].ToString() + item.FieldValues["FileDirRef"].ToString().Substring(rootFolderServerRelativeUrl.Length);
                            idMapping.IdMapping[mappingKey] = item.Id;
                            index = index < item.Id ? item.Id : index;

                            try
                            {
                                if (item.Properties?.FieldValues?.TryGetValue("AppendGUID", out var appendGuid) == true
                                    && (!idMapping.AppendItemMapping.TryGetValue(appendGuid.ToString(), out var tempItemId) || tempItemId < item.Id))
                                {
                                    idMapping.AppendItemMapping[appendGuid.ToString()] = item.Id;
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Error("Failed to get list item AppendGUID due to: {0}", e.ToString());
                            }
                        }
                        index = lastIndex + 5000 < index ? index : lastIndex + 5000;
                        totalCount -= items.Count;
                        if (items.Count > 0)
                        {
                            timer.Reset();
                            timer.Start();
                        }
                        if (timer.ElapsedMilliseconds > CACHE_TIME_OUT)
                        {
                            mLogger.Warn("Timeout when caching item GUID and rowId mappings under list : {0}", rootFolderServerRelativeUrl);
                            break;
                        }
                    }
                    while (totalCount > 0);
                    timer.Stop();
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to get list item Guid and RowId mappings due to: {0}", e.ToString());
            }

            return idMapping;
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public ItemIdMapping GetListItemGuidAndRowIdMappingsInSmallList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, string listTitle, Guid listId)
        {
            ItemIdMapping idMapping = new ItemIdMapping();
            idMapping.HasAttachment = false;
            idMapping.IdMapping = new Dictionary<string, int>();
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(rootFolderServerRelativeUrl);
                    ListItemCollection items = list.GetItems(camlQuery);
                    context.Load(items, its => its.Include(it => it.Id, it => it["GUID"], it => it["FileDirRef"], it => it["Attachments"]));
                    context.ExecuteQuery();
                    foreach (ListItem item in items)
                    {
                        idMapping.HasAttachment = Convert.ToBoolean(item.FieldValues["Attachments"]) ? true : idMapping.HasAttachment;
                        string mappingKey = item.FieldValues["GUID"].ToString() + item.FieldValues["FileDirRef"].ToString().Substring(rootFolderServerRelativeUrl.Length);
                        idMapping.IdMapping[mappingKey] = item.Id;
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to get list item Guid and RowId mappings due to: {0}", e.ToString());
            }

            return idMapping;
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        {
            return GetItemsProperties(webServerRelativeUrl, listName, listId, camlQueryNode, true);
        }
        public Dictionary<string, object> GetItemsForRecords(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode, bool resetItemsIdCache = true)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType, l => l.ItemCount);
                context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                context.ExecuteQuery();
                CamlQuery camlquery = new CamlQuery();
                camlquery.ViewXml = camlQueryNode[3];
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    //SetCamlQueryFolderUrl(camlquery, camlQueryNode[4]);
                    camlquery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(camlQueryNode[4]);
                    mLogger.Debug("Get items for records, folder: {0}", camlQueryNode[4]);
                }
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    ListItemCollectionPosition licp = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                    camlquery.ListItemCollectionPosition = licp;
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlquery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }

                ListItemCollection items = list.GetItems(camlquery);
                ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                    using (scope.StartTry())
                    {
                        context.Load(items);
                        context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName));
                    }
                    using (scope.StartCatch())
                    {
                        context.Load(items);
                        context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments));
                    }
                }
                context.ExecuteQuery();
                List<string> itemIdList = new List<string>();
                List<IDictionary<string, object>> itemList = new List<IDictionary<string, object>>();
                foreach (ListItem item in items)
                {
                    Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                    GetItemDic(itemProperties, item);
                    itemList.Add(itemProperties);
                    if (itemProperties.ContainsKey("UniqueId"))
                    {
                        itemIdList.Add(itemProperties["UniqueId"].ToString());
                    }
                }
                itemsProperties[AveObjectModelConstant.ChildrenProperties] = itemList;
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    mLogger.Info("Cache items ID count {0}, in folder:{1},resetItemsIdCache? {2}", itemIdList.Count, camlQueryNode[4], resetItemsIdCache);
                    if (resetItemsIdCache)
                    {
                        //本次query了多少Item， 就缓存多少Item的UniqueID到CurrentList中， 以方便后期如果用到LastAcessTime时批量获取
                            this.mCurrentList.FoldersToSubItemUniqueIds = new Dictionary<string, IList<string>>() { { camlQueryNode[4], itemIdList } };
                        //同时清空上一次Query的缓存
                        this.mCurrentList.FoldersToSubItemLastAccessTime = new System.Collections.Concurrent.ConcurrentDictionary<string, Dictionary<string, long>>();
                    }
                    else
                    {
                        if(mCurrentList.FoldersToSubItemUniqueIds == null || mCurrentList.FoldersToSubItemUniqueIds.Count == 0 || !mCurrentList.FoldersToSubItemUniqueIds.ContainsKey(camlQueryNode[4]))
                        {
                            this.mCurrentList.FoldersToSubItemUniqueIds = new Dictionary<string, IList<string>>() { { camlQueryNode[4], itemIdList } };
                            this.mCurrentList.FoldersToSubItemLastAccessTime = new System.Collections.Concurrent.ConcurrentDictionary<string, Dictionary<string, long>>();
                        }
                        else
                        {
                            List<string> cachedItemId = mCurrentList.FoldersToSubItemUniqueIds[camlQueryNode[4]] as List<string>;
                            cachedItemId.AddRange(itemIdList);
                        }
                    }
                    mLogger.Info("Cached items Id total count {0}", mCurrentList.FoldersToSubItemUniqueIds.ContainsKey(camlQueryNode[4]) ? mCurrentList.FoldersToSubItemUniqueIds[camlQueryNode[4]].Count : 0);
                }
                if (items.ListItemCollectionPosition != null)
                {
                    itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                }
                else
                {
                    itemsProperties["PageInfo"] = null;
                }

                return itemsProperties;
            }
        }
        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetItemsLightly(string webServerRelativeUrl, string listName, Guid listId, string[] loadFieldInternalNames)
        {
            return GetItemsPropertiesLightly(webServerRelativeUrl, listName, listId, loadFieldInternalNames);
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetItemsByIdSelectedFields(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        {
            return GetItemsProperties(webServerRelativeUrl, listName, listId, camlQueryNode, false);
        }
        private Dictionary<string, object> GetItemsPropertiesLightly(string webServerRelativeUrl, string listName, Guid listId, string[] loadFieldInternalNames)
        {
            using (var context = CreateDiscoverContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseTemplate);
                context.ExecuteQuery();
                //SAAS-38545 Convert loadFieldInternalNames to camlQuery
                AveCamlQuery aveCamlQuery = CreateAveCamlQuery(loadFieldInternalNames, list.BaseTemplate == (int)AveListTemplateType.ExternalList);
                CamlQuery camlQuery;
                bool loadAllItems;
                ListItemCollectionPosition licp;
                ConvertToCamlQuery(aveCamlQuery, out camlQuery, out loadAllItems, out licp);
                var itemList = new List<IDictionary<string, object>>();
                do
                {
                    camlQuery.ListItemCollectionPosition = licp;
                    ListItemCollection items = list.GetItems(camlQuery);
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    using (ehScope.StartScope())
                    {
                        context.Load(items, its => its.ListItemCollectionPosition,
                                            its => its.Include(t => t.Id));
                        //SAAS-38545 Load Specific field value
                        foreach (var loadFieldInternalName in loadFieldInternalNames)
                        {
                            context.Load(items, its => its.Include(t => t.Id, t => t[loadFieldInternalName]));
                        }
                    }
                    context.ExecuteQuery();
                    if (ehScope.HasException)
                    {
                        mLogger.Warn("load item failed due to: {0}", ehScope.ErrorMessage);
                    }
                    if (items != null)
                    {
                        mLogger.Info($"[SAAS-38322]Success to find items lightly in list:{listId}, count:{items.Count()},  ItemList:{itemList.Count()}");
                        foreach (ListItem item in items)
                        {
                            Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                            GetItemDic(itemProperties, item);
                            itemList.Add(itemProperties);
                        }
                        licp = items.ListItemCollectionPosition;
                        if (items.ListItemCollectionPosition != null)
                        {
                            itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                        }
                        else
                        {
                            itemsProperties["PageInfo"] = null;
                        }
                    }
                }
                while (licp != null && loadAllItems);
                itemsProperties.AddChildren(itemList);
                return itemsProperties;
            }
        }

        private static void ConvertToCamlQuery(AveCamlQuery aveCamlQuery, out CamlQuery camlQuery, out bool loadAllItems, out ListItemCollectionPosition licp)
        {
            loadAllItems = true;
            camlQuery = new CamlQuery();
            var camlQueryNode = aveCamlQuery.ToStringArray();

            camlQuery.ViewXml = camlQueryNode[3];
            if (!string.IsNullOrEmpty(camlQueryNode[4]))
            {
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(camlQueryNode[4]);
            }
            licp = null;
            if (!string.IsNullOrEmpty(camlQueryNode[5]))
            {
                licp = new ListItemCollectionPosition
                {
                    PagingInfo = camlQueryNode[5]
                };
            }
            if (!string.IsNullOrEmpty(camlQueryNode[6]))
            {
                camlQuery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
            }
            if (!string.IsNullOrEmpty(camlQueryNode[7]))
            {
                loadAllItems = Convert.ToBoolean(camlQueryNode[7]);
            }
        }

        private static AveCamlQuery CreateAveCamlQuery(string[] loadFieldInternalNames, bool isExternalList = false)
        {
            AveCamlQuery aveCamlQuery = AveCamlQuery.CreateAllItemsQuery(5000, loadFieldInternalNames);
            if (isExternalList)
            {
                aveCamlQuery.ViewXml = null;
                aveCamlQuery.QueryXml = null;
                aveCamlQuery.QueryOptionXml = null;
                aveCamlQuery.ViewFieldsXml = null;
                aveCamlQuery.FolderServerRelativeUrl = null;
            }
            aveCamlQuery.DatesInUtc = true;
            return aveCamlQuery;
        }
        private Dictionary<string, object> GetItemsProperties(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode, bool isLoadItemProperty)
        {
            using (var context = CreateDiscoverContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = camlQueryNode[3];
                bool loadAllItems = true;
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(camlQueryNode[4]);
                }
                ListItemCollectionPosition licp = null;
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    licp = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlQuery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }
                if (!string.IsNullOrEmpty(camlQueryNode[7]))
                {
                    loadAllItems = Convert.ToBoolean(camlQueryNode[7]);
                }
                var itemList = new List<IDictionary<string, object>>();
                do
                {
                    camlQuery.ListItemCollectionPosition = licp;
                    ListItemCollection items = list.GetItems(camlQuery);
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition,
                                                its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName));
                        }
                        using (ehScope.StartCatch())
                        {
                            context.Load(items);
                            if (isLoadItemProperty)
                            {
                                context.Load(items, its => its.ListItemCollectionPosition,
                                                    its => its.Include(t => t.HasUniqueRoleAssignments));//SAAS-6084 DisplayName not support discussion board
                            }
                            else
                            {
                                context.Load(items, its => its.ListItemCollectionPosition);
                            }
                        }
                    }

                    context.ExecuteQuery();
                    if (ehScope.HasException)
                    {
                        mLogger.Warn("load item failed due to: {0}", ehScope.ErrorMessage);
                    }
                    foreach (ListItem item in items)
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        GetItemDic(itemProperties, item);
                        itemList.Add(itemProperties);
                    }
                    licp = items.ListItemCollectionPosition;
                    if (items.ListItemCollectionPosition != null)
                    {
                        itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        itemsProperties["PageInfo"] = null;
                    }
                }
                while (licp != null && loadAllItems);
                itemsProperties.AddChildren(itemList);
                return itemsProperties;
            }
        }

        /*public Dictionary<int, List<int>> GetUniquePermissionItemsIDInEachFolder(string webUrl, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webUrl);
                List list = web.Lists.GetById(listId);
                Dictionary<int, List<int>> items = new Dictionary<int, List<int>>();
                SPOCaFolder folder = list.LoadAllItemIdsForCa(4000, null);
                AnalyzeSPFolder(folder, items);
                return items;
            }
        }*/

        /*private void AnalyzeSPFolder(SPOCaFolder folder, Dictionary<int, List<int>> items)
        {
            List<int> uniqueItems = new List<int>();
            if (folder.Items != null)
            {
                foreach (var item in folder.Items)
                {
                    if (item.HasUniqueRoleAssignments)
                    {
                        uniqueItems.Add(item.Id);
                    }
                }
                if (uniqueItems.Count > 0)
                {
                    items.Add(folder.Id, uniqueItems);
                }
            }
            if (folder.SubFolders != null)
            {
                foreach (var subFolder in folder.SubFolders)
                {
                    AnalyzeSPFolder(subFolder, items);
                }
            }
        }*/

        /*private void FilterSPFolder(SPOCaFolder folder, Dictionary<int, KeyValuePair<int, List<int>>> items, int parentId = 0)
        {
            if (FolderIncludeUniqueItemsOrFolders(folder))
            {
                KeyValuePair<int, List<int>> uniqueItems = new KeyValuePair<int, List<int>>(parentId,new List<int>());
                items.Add(folder.Id, uniqueItems);
                if (folder.Items != null)
                {
                    foreach (var item in folder.Items)
                    {
                        if (item.HasUniqueRoleAssignments)
                        {
                            uniqueItems.Value.Add(item.Id);
                        }
                    }
                    if (uniqueItems.Value.Count > 0)
                    {
                        items[folder.Id] = uniqueItems;
                    }
                }
            }

            if (folder.SubFolders != null)
            {
                foreach (var subFolder in folder.SubFolders)
                {
                    FilterSPFolder(subFolder, items, folder.Id);
                }
            }
        }*/

        /*private bool FolderIncludeUniqueItemsOrFolders(SPOCaFolder folder)
        {
            if (folder.HasUniqueRoleAssignments)
            {
                return true;
            }
            if (folder.Items != null)
            {
                foreach (var item in folder.Items)
                {
                    if (item.HasUniqueRoleAssignments)
                    {
                        return true;
                    }
                }
            }
            if (folder.SubFolders != null)
            {
                foreach (var subFolder in folder.SubFolders)
                {
                    bool result = FolderIncludeUniqueItemsOrFolders(subFolder);
                    if (result)
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

        /*public Dictionary<int, KeyValuePair<int, List<int>>> GetFoldersIncludeUniquePermissionSubItemsOrFolders(string webUrl, Guid listId)
        {
            int defaultRowLimit = 4000;
            int rowlimit = defaultRowLimit;
            do
            {
                try
                {
                    mLogger.Info("Excute LoadAllItemIdsForCa with count: {0}", rowlimit);
                    using (var context = CreateDiscoverContext())
                    {
                        Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                        Web web = context.Site.OpenWeb(webUrl);
                        List list = web.Lists.GetById(listId);
                        Dictionary<int, KeyValuePair<int, List<int>>> items = new Dictionary<int, KeyValuePair<int, List<int>>>();
                        SPOCaFolder folder = list.LoadAllItemIdsForCa(rowlimit, null);
                        FilterSPFolder(folder, items);
                        return items;
                    }
                }
                catch (ServerException ex)
                {
                    rowlimit /= 2;
                    if (ExceptionHandleUtil.HandleBatchExecuteException(ex, ref defaultRowLimit, ref rowlimit) || rowlimit == 0)
                    {
                        throw;
                    }
                }
            }
            while (true);
        }*/

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId)
        {
            ListItem item = null;
            Dictionary<string, object> itemPro = new Dictionary<string, object>();

            if (itemId.Equals(default(int)))
            {
                if (uniqueId == Guid.Empty)
                {
                    throw new NullReferenceException("Item id is null");
                }
                else
                {
                    return GetItemByUniqueId(webServerRelativeUrl, listId, uniqueId);
                }
            }
            else if (this.mCurrentList.ListTitle.Equals(listName, StringComparison.OrdinalIgnoreCase) && mCurrentList.Items.ContainsKey(itemId)
                && mCurrentList.ListId == listId)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CurrentItem.GetListItem.GetItemFromCache"))
                return this.mCurrentList.Items[itemId];
            }
            else
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    item = list.GetItemById(itemId);

                    //SAAS-14246 avoid code blocks after restore
                    ConditionalScope isDocumentLibraryScope = new ConditionalScope(context, () => list.BaseType == BaseType.DocumentLibrary, true);
                    ExceptionHandlingScope ehScopeIfTrue = new ExceptionHandlingScope(context);
                    ExceptionHandlingScope ehScopeIfFalse = new ExceptionHandlingScope(context);
                    using (isDocumentLibraryScope.StartScope())
                    {
                        using (isDocumentLibraryScope.StartIfTrue())
                        {
                            using (ehScopeIfTrue.StartScope())
                            {
                                using (ehScopeIfTrue.StartTry())
                                {
                                    context.Load(item);
                                    context.Load(item, i => i.File.CustomizedPageStatus, i => i.HasUniqueRoleAssignments, i => i.DisplayName);
                                }
                                using (ehScopeIfTrue.StartCatch())
                                {
                                    context.Load(item);
                                    context.Load(item, i => i.File.CustomizedPageStatus, i => i.HasUniqueRoleAssignments);
                                }
                            }
                        }
                        using (isDocumentLibraryScope.StartIfFalse())
                        {
                            using (ehScopeIfFalse.StartScope())
                            {
                                using (ehScopeIfFalse.StartTry())
                                {
                                    context.Load(item);
                                    context.Load(item, i => i.HasUniqueRoleAssignments, i => i.DisplayName);
                                }
                                using (ehScopeIfFalse.StartCatch())
                                {
                                    context.Load(item);
                                    context.Load(item, i => i.HasUniqueRoleAssignments);//SAAS-6084 DisplayName not support discussion board
                                }
                            }
                        }
                    }
                    context.ExecuteQuery();

                    if (ehScopeIfTrue.HasException || ehScopeIfFalse.HasException)
                    {
                        string errorMessage = ehScopeIfTrue.HasException ? ehScopeIfTrue.ErrorMessage : ehScopeIfFalse.ErrorMessage;
                        mLogger.Warn("load item failed due to: {0}", errorMessage);
                    }
                    if (item.File.ServerObjectIsNull.HasValue && !item.File.ServerObjectIsNull.Value)
                    {
                        itemPro["CustomizedPageStatus"] = (int)item.File.CustomizedPageStatus;
                    }
                }
            }
            GetItemDic(itemPro, item);
            return itemPro;

        }

        private Dictionary<string, object> GetItemByUniqueId(string webServerRelativeUrl, Guid listId, Guid itemId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = GetListItemByUniqueId(context, list, itemId);
                if (item == null)
                {
                    throw new FileNotFoundException("Can't find ListItem by specific uniqueId");
                }
                GetItemDic(itemProp, item);
                return itemProp;
            }
        }

        public Dictionary<string, object> GetFileByPath(string webServerRelativeUrl, string filePath)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);

                ListItem item = null;
                var retryHelper = new AveTaskRetryHelper(3, true);
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    item = web.GetFileByServerRelativeUrl(filePath).ListItemAllFields;
                    context.Load(item);
                    context.ExecuteQuery();
                });
                if (item == null)
                {
                    throw new FileNotFoundException("Can't find ListItem by specific uniqueId");
                }
                GetItemDic(itemProp, item);
                return itemProp;
            }
        }

        public Dictionary<string, object> GetItemById(Guid webId, Guid listId, int itemId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);

                ListItem item = null;
                var retryHelper = new AveTaskRetryHelper(3, true);
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    item = list.GetItemById(itemId);
                    context.Load(item);
                    context.ExecuteQuery();
                });
                if (item == null)
                {
                    throw new FileNotFoundException("Can't find ListItem by specific uniqueId");
                }
                GetItemDic(itemProp, item);
                return itemProp;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                var forms = new List<IDictionary<string, object>>();
                returnInfo.AddChildren(forms);
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    context.Load(list.Forms);
                    context.ExecuteQuery();
                    foreach (Form form in list.Forms)
                    {
                        Dictionary<string, object> formPro = new Dictionary<string, object>();
                        //formPro["ID"] = form.Id;   some form doesn't have id
                        formPro["Url"] = form.ServerRelativeUrl;
                        formPro["TemplateName"] = form.FormType.ToString();
                        formPro["FormType"] = (int)form.FormType;
                        forms.Add(formPro);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("failed to load list form. list: {0}, web: {1}, cause: {2}", listName, webServerRelativeUrl, e.ToString());
                }
                return returnInfo;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Workflow, MethodType.CSOM | MethodType.HttpRequest)]
        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                var workflows = new List<IDictionary<string, object>>();
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCollection wfa = null;
                switch (workflowSource)
                {
                    case "list.workflow":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        wfa = list.WorkflowAssociations;
                        break;
                    case "contentType.workflow":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        wfa = contentType.WorkflowAssociations;
                        break;
                    case "web.workflow":
                        web = context.Site.OpenWeb(webServerRelativeUrl);
                        wfa = web.WorkflowAssociations;
                        break;
                    default:
                        break;
                }
                context.Load(wfa);
                context.ExecuteQuery();
                ArgumentCheck.CheckNotNull(wfa);
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflow in wfa)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    workflows.Add(workflowPro);
                }
                try
                {
                    if (workflows.Count > 0)
                    {
                        Dictionary<string, object> webRequestProperties = mRequestCommon.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
                        if (webRequestProperties.Count > 0)
                        {
                            foreach (var workflowProp in workflows)
                            {
                                workflowProp["RunningInstances"] = webRequestProperties[workflowProp["Name"].ToString()];
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Info(e.ToString());
                }
                returnInfo.AddChildren(workflows);
                return returnInfo;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId)
        {
            string webUrl = AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/');
            using (AveClientContext context = CreateContext(webUrl))
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                var views = new List<IDictionary<string, object>>();
                Web web = context.Web;
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields));
                context.ExecuteQuery();
                int max = 10;
                int count = 0;
                Dictionary<Guid, ClientFile> viewFiles = new Dictionary<Guid, ClientFile>();
                Dictionary<Guid, Dictionary<string, object>> viewPropderties = new Dictionary<Guid, Dictionary<string, object>>();
                Dictionary<Guid, string> viewInfos = new Dictionary<Guid, string>();
                foreach (View view in list.Views)
                {
                    viewInfos.Add(view.Id, view.ServerRelativeUrl);
                    Dictionary<string, object> viewPro = new Dictionary<string, object>();
                    AssembleViewProperties(viewPro, view, webServerRelativeUrl);
                    viewPropderties[view.Id] = viewPro;
                    ClientFile viewFile;
                    ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                    using (exceptionScope.StartScope())
                    {
                        using (exceptionScope.StartTry())
                        {
                            viewFile = context.Web.GetFileByServerRelativePath(view.ServerRelativePath);
                            context.Load(viewFile, v => v.ETag);
                        }
                        using (exceptionScope.StartCatch())
                        {
                            //viewFile出现异常
                        }
                    }
                    viewFiles[view.Id] = viewFile;
                    count++;
                    if (count >= max)
                    {
                        context.ExecuteQuery();
                        count = 0;
                    }
                }
                if (count > 0)
                {
                    context.ExecuteQuery();
                }
                foreach (var viewFile in viewFiles)
                {
                    if (!viewFile.Value.IsPropertyAvailable("ETag"))
                    {
                        mLogger.Warn("An error occurred while getting viewFile. view url:{0}.", viewInfos[viewFile.Key]);
                    }
                    else
                    {
                        string guid = GetIdsFromEtag(viewFile.Value.ETag)[0];
                        Dictionary<string, object> tempViewProperty = viewPropderties[viewFile.Key];
                        tempViewProperty["PageUrlID"] = new Guid(guid);
                        views.Add(tempViewProperty);
                    }
                }
                viewFiles.Clear();
                viewPropderties.Clear();
                returnInfo.AddChildren(views);
                return returnInfo;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        public Dictionary<string, object> GetLists(string webServerRelativeUrl, List<string> supportedResourceCultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var allLists = new List<List>();
                ListCollectionPosition listCollectionPosition = null;
                var exceptionList = new List<Dictionary<string, object>>();
                try
                {
                    do
                    {
                        var listCollection = web.GetLists(new GetListsParameters
                        {
                            RowLimit = 2000,
                            ListCollectionPosition = listCollectionPosition,
                        });
                        context.Load(
                            listCollection, 
                            ll => ll.ListCollectionPosition,
                            ls => ls.IncludeWithDefaultProperties(
                                l => l.ValidationFormula,
                                l => l.ValidationMessage,
                                l => l.OnQuickLaunch,
                                l => l.IsSiteAssetsLibrary,
                                l => l.HasUniqueRoleAssignments,
                                l => l.DataSource,
                                l => l.Id,
                                l => l.Hidden,
                                l => l.ItemCount,
                                l => l.EnableAttachments,
                                l => l.EnableVersioning,
                                l => l.DefaultDisplayFormUrl,//SAAS-964
                                l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                                l => l.DefaultContentApprovalWorkflowId,
                                l => l.RootFolder,
                                l => l.LastItemModifiedDate,
                                l => l.RootFolder.Properties,
                                l => l.EnableAssignToEmail,
                                l => l.Author));
                        context.ExecuteQuery();
                        foreach (var list in listCollection)
                        {
                            allLists.Add(list);
                        }
                        listCollectionPosition = listCollection.ListCollectionPosition;
                    } while (listCollectionPosition != null);
                }
                catch (ServerUnauthorizedAccessException ex)
                {
                    mLogger.Debug($"ServerUnauthorizedAccessException occurred, retry GetLists. {ex.Message}");

                    context.Load(
                        web.Lists,
                        ls => ls.Include(
                            l => l.BaseType,
                            l => l.BaseTemplate,
                            l => l.ValidationFormula,
                            l => l.ValidationMessage,
                            //l => l.OnQuickLaunch,
                            l => l.IsSiteAssetsLibrary,
                            l => l.HasUniqueRoleAssignments,
                            l => l.DataSource,
                            l => l.Id,
                            l => l.Hidden,
                            l => l.ItemCount,
                            l => l.EnableAttachments,
                            l => l.EnableVersioning,
                            //l => l.DefaultDisplayFormUrl,//SAAS-964
                            l => l.LastItemModifiedDate,
                            //l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                            l => l.DefaultContentApprovalWorkflowId,
                            l => l.EnableAssignToEmail,
                            l => l.Author
                        ).Where(l => l.BaseTemplate == (int)AveListTemplateType.UserInformation));
                    context.ExecuteQuery();
                    allLists = new List<List>(web.Lists);

                    listCollectionPosition = null;
                    do
                    {
                        var listCollection = web.GetLists(new GetListsParameters
                        {
                            RowLimit = 2000,
                            ListCollectionPosition = listCollectionPosition,
                        });
                        context.Load(
                        listCollection,
                        ls => ls.ListCollectionPosition,
                        ls => ls.IncludeWithDefaultProperties(
                            l => l.ValidationFormula,
                            l => l.ValidationMessage,
                            l => l.OnQuickLaunch,
                            l => l.IsSiteAssetsLibrary,
                            l => l.HasUniqueRoleAssignments,
                            l => l.DataSource,
                            l => l.Id,
                            l => l.Hidden,
                            l => l.ItemCount,
                            l => l.EnableAttachments,
                            l => l.EnableVersioning,
                            l => l.DefaultDisplayFormUrl,//SAAS-964
                            l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                            l => l.DefaultContentApprovalWorkflowId,
                            l => l.RootFolder,
                            l => l.LastItemModifiedDate,
                            l => l.RootFolder.Properties,
                            l => l.EnableAssignToEmail,
                            l => l.Author
                        ).Where(l => l.BaseTemplate != (int)AveListTemplateType.UserInformation));
                        context.ExecuteQuery();

                        allLists.AddRange(listCollection);

                        listCollectionPosition = listCollection.ListCollectionPosition;
                    } while (listCollectionPosition != null);
                    
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Batch get list property failed, switch to one by one method. Error: {0}", ex);
                    var exceptionHandler = new ExceptionHandlingScope(context);
                    using (exceptionHandler.StartScope())
                    {
                        using (exceptionHandler.StartTry())
                        {
                            context.Load(
                                web.Lists, 
                                ls => ls.IncludeWithDefaultProperties(
                                    l => l.ValidationFormula,
                                    l => l.ValidationMessage,
                                    l => l.OnQuickLaunch,
                                    l => l.IsSiteAssetsLibrary,
                                    l => l.HasUniqueRoleAssignments,
                                    l => l.DataSource,
                                    l => l.Id,
                                    l => l.Hidden,
                                    l => l.ItemCount,
                                    l => l.EnableAttachments,
                                    l => l.EnableVersioning,
                                    l => l.DefaultDisplayFormUrl,//SAAS-964
                                    l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                                    l => l.DefaultContentApprovalWorkflowId,
                                    l => l.RootFolder,
                                    l => l.LastItemModifiedDate,
                                    l => l.RootFolder.Properties,
                                    l => l.EnableAssignToEmail,
                                    l => l.Author));
                        }
                        using (exceptionHandler.StartCatch())
                        {
                            context.Load(web.Lists, ls => ls.Include(l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate));
                        }
                    }
                    context.ExecuteQuery();

                    if (exceptionHandler.HasException)
                    {
                        mLogger.Warn("Get lists by web url:{0}, failed:{1}, ServerStackTrace:{2}, ServerErrorDetails:{3}",
                            webServerRelativeUrl,
                            exceptionHandler.ErrorMessage,
                            exceptionHandler.ServerStackTrace,
                            exceptionHandler.ServerErrorDetails);
                        //context.Load(web.Lists, ls => ls.IncludeWithDefaultProperties());
                        //context.ExecuteQuery();
                        var lists = new List<List>(web.Lists);
                        List item = null;
                        foreach (var tempList in lists)
                        {
                            item = tempList;
                            try
                            {
                                var listExceptionHandler = new ExceptionHandlingScope(context);
                                using (listExceptionHandler.StartScope())
                                {
                                    using (listExceptionHandler.StartTry())
                                    {
                                        context.Load(
                                            item, 
                                            l => l.ValidationFormula,
                                            l => l.ValidationMessage,
                                            l => l.OnQuickLaunch,
                                            l => l.IsSiteAssetsLibrary,
                                            l => l.HasUniqueRoleAssignments,
                                            l => l.DataSource,
                                            l => l.Id,
                                            l => l.Hidden,
                                            l => l.ItemCount,
                                            l => l.EnableAttachments,
                                            l => l.EnableVersioning,
                                            l => l.DefaultDisplayFormUrl,//SAAS-964
                                            l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                                            l => l.DefaultContentApprovalWorkflowId,
                                            l => l.RootFolder,
                                            l => l.LastItemModifiedDate,
                                            l => l.RootFolder.Properties,
                                            l => l.EnableAssignToEmail,
                                            l => l.Author);
                                    }
                                    using (listExceptionHandler.StartCatch())
                                    {
                                        context.Load(
                                            item,
                                            l => l.ValidationFormula,
                                            l => l.ValidationMessage,
                                            l => l.OnQuickLaunch,
                                            l => l.IsSiteAssetsLibrary,
                                            l => l.HasUniqueRoleAssignments,
                                            l => l.DataSource,
                                            l => l.Id,
                                            l => l.Hidden,
                                            l => l.ItemCount,
                                            l => l.EnableAttachments,
                                            l => l.EnableVersioning,
                                            l => l.DefaultDisplayFormUrl,//SAAS-964
                                            l => l.LastItemModifiedDate,
                                            l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                                            l => l.DefaultContentApprovalWorkflowId,
                                            l => l.EnableAssignToEmail,
                                            l => l.Author);
                                    }
                                }
                                mLogger.Info("Get list:{0} by web url:{1}", item.Title, webServerRelativeUrl);
                                context.ExecuteQuery();
                                allLists.Add(item);
                                if (listExceptionHandler.HasException)
                                {
                                    mLogger.Warn("Get list:{0} by web url:{1}, failed:{2}, ServerStackTrace:{3}, ServerErrorDetails:{4}",
                                                    item.Title,
                                                    webServerRelativeUrl,
                                                    exceptionHandler.ErrorMessage,
                                                    exceptionHandler.ServerStackTrace,
                                                    exceptionHandler.ServerErrorDetails);
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Error($"Failed retrieve list id: {item.Id}. list title:{item.Title}. in web {webServerRelativeUrl}, Error: {e}");

                                var dic = new Dictionary<string, object>();
                                dic["Id"] = item.Id;
                                dic["Title"] = item.Title;
                                dic["BaseType"] = item.BaseType;
                                dic["Exception"] = e;

                                var rootFolderProp = new Dictionary<string, object>();
                                rootFolderProp["Exists"] = false;
                                dic["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;

                                exceptionList.Add(dic);
                            }
                        }
                    }
                    else
                    {
                        allLists.AddRange(web.Lists);
                    }
                }

                // 获取list title Resource和 description Resource
                if (supportedResourceCultureNames != null && supportedResourceCultureNames.Count > 0)
                {
                    try
                    {
                        foreach (List list in allLists)
                        {
                            foreach (var languageName in supportedResourceCultureNames)
                            {
                                list.TitleResource.GetValueForUICulture(languageName);
                                list.DescriptionResource.GetValueForUICulture(languageName);
                            }
                        }
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Error while query lists user resource, web url:{0}, lists count:{1}, error:{2}", webServerRelativeUrl, web.Lists.Count, ex);
                    }
                }

                var listsProp = new List<IDictionary<string, object>>(exceptionList);
                foreach (List l in allLists)
                {
                    mLogger.Info($"Getting list {(l.IsPropertyAvailable("Title") ? l.Title : $"{l.BaseTemplate}|{l.Id}")}");
                    Dictionary<string, object> listProperties = new Dictionary<string, object>();
                    CopyProperty(listProperties, l);
                    CopyUserResourceProperty(listProperties, l);
                    long flag = 0;
                    if (l.EnableVersioning)
                        flag |= 0x0000000000000080;
                    if (!l.EnableAttachments)
                        flag |= 0x0000000000000008;
                    listProperties["Flag"] = flag;    //Can not get this property.
                    //external list
                    if (l.DataSource != null && l.BaseTemplate == (int)AveListTemplateType.ExternalList)
                    {
                        Dictionary<string, object> listDataSource = new Dictionary<string, object>();
                        listDataSource.Add(AveBDCProperties.LobSystemInstance, l.DataSource.Properties[AveBDCProperties.LobSystemInstance]);
                        listDataSource.Add(AveBDCProperties.EntityNamespace, l.DataSource.Properties[AveBDCProperties.EntityNamespace]);
                        listDataSource.Add(AveBDCProperties.Entity, l.DataSource.Properties[AveBDCProperties.Entity]);
                        listDataSource.Add(AveBDCProperties.SpecificFinder, l.DataSource.Properties[AveBDCProperties.SpecificFinder]);
                        listProperties.Add("DataSource" + AveObjectModelConstant.ObjectPropertySuffix, listDataSource);
                        listProperties.Remove("DataSource");
                        //Always, itemCount value is zero in external list,
                        //listProperties.Remove("ItemCount");
                    }
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    try
                    {
                        if (!l.IsObjectPropertyInstantiated("RootFolder"))
                        {
                            context.Load(l, list => list.RootFolder, list => list.RootFolder.Properties);
                            context.ExecuteQuery();
                        }
                        AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, l.RootFolder);
                        rootFolderProp["Exists"] = true;
                    }
                    catch (Exception ex)
                    {
                        if (l.BaseTemplate == (int)AveListTemplateType.UserInformation)
                        {
                            mLogger.Debug($"Failed to assemble root folder properties of list: {l.Id} Error: {ex}");
                            rootFolderProp["Url"] = "_catalogs/users";
                            rootFolderProp["ServerRelativeUrl"] = $"{webServerRelativeUrl.TrimEnd('/')}/_catalogs/users";
                            rootFolderProp["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = $"{webServerRelativeUrl.TrimEnd('/')}/_catalogs";
                            rootFolderProp["Exists"] = true;
                        }
                        else
                        {
                            mLogger.Warn($"Failed to assemble root folder properties of list: {l.Id} Error: {ex}");
                            rootFolderProp["Exists"] = false;
                        }
                    }
                    try
                    {
                        if (l.Author != null && l.Author.LoginName != null)
                        {
                            listProperties["Author" + AveObjectModelConstant.ObjectPropertySuffix] = l.Author.LoginName;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn($"Failed to assemble author properties of list: {l.Id} Error: {ex}");
                    }
                    listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    listsProp.Add(listProperties);
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                returnInfo.AddChildren(listsProp);
                return returnInfo;
            }
        }

        private void AssemblRootFolderProperties(string webServerRelativeUrl, Dictionary<string, object> folderProperties, Folder rootFolder)
        {
            CopyProperty(folderProperties, rootFolder);
            string url = TrimFolderUrl(webServerRelativeUrl, rootFolder.ServerRelativeUrl);
            folderProperties["Url"] = url;
            int length = url.LastIndexOf('/');
            string parentFolderUrl = length == -1 ? webServerRelativeUrl : webServerRelativeUrl.TrimEnd('/') + "/" + url.Substring(0, length);
            folderProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderUrl;
            folderProperties["Exists"] = true;
            if (WrapperConfiguration.WrapperConfigurationForBPOS.LoadRootFolderUniqueId && rootFolder.Properties.FieldValues.ContainsKey("vti_etag") && rootFolder.Properties["vti_etag"] != null)    //SAAS-11986 获取rootFolder的真正的UniqueId.
            {
                string tagString = rootFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                Guid uniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                folderProperties["UniqueId"] = uniqueId;
            }
            folderProperties["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable(rootFolder.Properties.FieldValues);
        }

        private static String TrimFolderUrl(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            string url = string.Empty;
            if (folderServerRelativeUrl.TrimEnd('/').Equals(webServerRelativeUrl.TrimEnd('/')))
            {
                url = string.Empty;
            }
            else
            {
                url = folderServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            return url;
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        public Dictionary<string, object> GetListByTitle(Guid webId, string listTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                context.Load(web, w => w.ServerRelativeUrl);
                context.ExecuteQuery();
                List list = web.Lists.GetByTitle(listTitle);
                context.Load(list, l => l.RootFolder,
                    l => l.RootFolder.Properties,
                    l => l.Id,
                    l => l.Title,
                    l => l.BaseType,
                    l => l.Hidden,
                    l => l.EnableVersioning,
                    l => l.EnableAttachments);
                context.ExecuteQuery();
                Dictionary<string, object> listProperties = new Dictionary<string, object>();
                CopyProperty(listProperties, list);
                long flag = 0;
                if (list.EnableVersioning)
                    flag |= 0x0000000000000080;
                if (!list.EnableAttachments)
                    flag |= 0x0000000000000008;
                listProperties["Flag"] = flag;    //Can not get this property.
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                try
                {
                    AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, list.RootFolder);
                }
                catch (Exception e)
                {
                    mLogger.Warn("List's root folder is not available. {0}" + list.Title, e);
                }
                rootFolderProp["Exists"] = true;
                listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                listProperties["WebId"] = webId;
                listProperties["RootFolderUrl"] = rootFolderProp["ServerRelativeUrl"];
                if (rootFolderProp.ContainsKey("UniqueId"))
                {
                    listProperties["RootFolderId"] = rootFolderProp["UniqueId"];
                }
                return listProperties;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        public string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                //if (string.IsNullOrEmpty(ParentWebUrl) || Guid.Equals(Id, Guid.Empty))
                //    RefreshContext();
                Web web = context.Site.OpenWeb(ParentWebUrl);
                List list = web.Lists.GetById(Id);
                context.Load(list, l => l.SchemaXml);
                try
                {
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Cannot get schemal xml. Web: {0}, Id: {1}, Title: {2} \n {3}", ParentWebUrl, Id.ToString(), listTitle, ex.ToString());
                }
                return list.SchemaXml;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        public Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                List list = web.Lists.GetById(listId);
                this.LoadList(context, list);
                Dictionary<string, object> listProp = new Dictionary<string, object>();
                CopyProperty(listProp, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, list.RootFolder);
                listProp["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProp;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        public string GetListTitle(Guid siteId, Guid webId, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, li => li.Title);
                context.ExecuteQuery();
                return list.Title;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.List, MethodType.CSOM)]
        private Dictionary<string, object> GetList(string webServerRelativeUrl, string title)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                List list = web.Lists.GetByTitle(title);
                this.LoadList(context, list);
                Dictionary<string, object> listProp = new Dictionary<string, object>();
                CopyProperty(listProp, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, list.RootFolder);
                listProp["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProp;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Site, MethodType.CSOM)]
        public Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webTemplatesProperties = new Dictionary<string, object>();
                WebTemplateCollection templates = context.Site.GetWebTemplates(lcid, 0);
                context.Load(templates);
                context.ExecuteQuery();
                var templateList = new List<IDictionary<string, object>>();
                foreach (WebTemplate template in templates)
                {
                    Dictionary<string, object> templateProperties = new Dictionary<string, object>();
                    CopyProperty(templateProperties, template);
                    templateList.Add(templateProperties);
                }
                webTemplatesProperties.AddChildren(templateList);
                return webTemplatesProperties;
            }
        }



        [ClientOMRequest(ReadWrite.Read, MethodLevel.ContentType, MethodType.CSOM)]
        public Dictionary<string, object> GetContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, string contentTypeId)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                context.ExecuteQuery();
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(newProp, contentType);
                return newProp;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.ContentType, MethodType.CSOM)]
        public List<string> GetContentTypeResourceFiles(string webServerRelativeUrl, string serverRelativeUrl, Dictionary<string, List<string>> resourceFilesIndex)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);

                var index = serverRelativeUrl.IndexOf("_cts");

                if (index >= 0)
                {
                    var folderUrl = webServerRelativeUrl.TrimEnd('/') + "/_cts";

                    var folder = web.GetFolderByServerRelativeUrl(folderUrl);

                    context.Load(folder.Folders,
                            a => a.Include(
                                b => b.Files.Include(c => c.TimeCreated, c => c.TimeLastModified, c => c.ServerRelativeUrl),
                                b => b.ServerRelativeUrl));
                    context.ExecuteQuery();

                    foreach (var subFolder in folder.Folders)
                    {
                        if (subFolder.ServerObjectIsNull == true)
                        {

                        }
                        else if (subFolder.Files.Count == 0)
                        {
                            resourceFilesIndex[subFolder.ServerRelativeUrl] = null;
                        }
                        else
                        {
                            resourceFilesIndex[subFolder.ServerRelativeUrl] = subFolder.Files.Select(a => a.ServerRelativeUrl).ToList();
                        }
                    }
                }
                else
                {
                    var folder = web.GetFolderByServerRelativeUrl(serverRelativeUrl);
                    context.Load(folder, a => a.Exists);

                    var conditionScope = new ConditionalScope(context, () => folder.Exists);

                    using (conditionScope.StartScope())
                    {
                        using (conditionScope.StartIfTrue())
                        {
                            context.Load(folder.Files, a => a.Include(c => c.TimeCreated, c => c.TimeLastModified, c => c.ServerRelativeUrl));
                        }
                    }

                    context.ExecuteQuery();

                    if (!folder.Exists && folder.Files.Count > 0)
                    {
                        resourceFilesIndex[serverRelativeUrl] = folder.Files.Select(a => a.ServerRelativeUrl).ToList();
                    }
                    else
                    {
                        resourceFilesIndex[serverRelativeUrl] = null;
                    }
                }
            }

            List<string> files = null;

            resourceFilesIndex.TryGetValue(serverRelativeUrl, out files);

            return files;
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Web, MethodType.CSOM)]
        public Dictionary<string, object> GetSubWebs(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(context.Site, s => s.Url, s => s.ServerRelativeUrl);
                context.Load(context.Site.RootWeb, w => w.Id);
                LoadSubSites(context, web);
                context.ExecuteQuery();
                Dictionary<string, object> subWebs = new Dictionary<string, object>();
                var subWebList = new List<IDictionary<string, object>>();
                foreach (Web subWeb in web.Webs)
                {
                    Dictionary<string, object> subWebProperties = new Dictionary<string, object>();
                    subWebProperties = GetWebProperties(context, subWeb, context.Site.Url, context.Site.ServerRelativeUrl, true);
                    subWebList.Add(subWebProperties);
                }
                subWebs.AddChildren(subWebList);
                return subWebs;
            }
        }

        private static void LoadSubSites(ClientContext context, Web web)
        {
            context.Load(web.Webs, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                w => w.RootFolder,
                                                                                                w => w.AllProperties,
                                                                                                w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch,
                                                                                                w => w.AllowDesignerForCurrentUser, w => w.HasUniqueRoleAssignments,
                                                                                                w => w.RequestAccessEmail,
                                                                                                w => w.UseAccessRequestDefault,
                                                                                                w => w.MembersCanShare,
                                                                                                w => w.AccessRequestSiteDescription,
                                                                                                w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType,
                                                                                                w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType,
                                                                                                w => w.SupportedUILanguageIds,
                                                                                                w => w.NoCrawl, w => w.ExcludeFromOfflineClient, w => w.AllowAutomaticASPXPageIndexing, w => w.SiteLogoDescription
                                                                                                ).Where(tempWeb => tempWeb.AppInstanceId == Guid.Empty));
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.ContentType, MethodType.CSOM)]
        public Dictionary<string, object> GetFile(string webServerRelativeUrl, Guid id)
        {
            return GetFile(webServerRelativeUrl, null, null, id);
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.ContentType, MethodType.CSOM)]
        public Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName)
        {
            return GetFile(webServerRelativeUrl, serverRelativeUrl, listName, Guid.Empty);
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.ContentType, MethodType.CSOM)]
        public Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName, Guid id)
        {
            ClientFile file = null;
            bool serverRelativeUrlVaild = true;
            Dictionary<string, object> fileProperties = new Dictionary<string, object>();
            ClientResult<DateTime> timeResult = null;
            if (serverRelativeUrl != null && this.mCurrentList.Files.ContainsKey(serverRelativeUrl))
            {
                fileProperties = this.mCurrentList.Files[serverRelativeUrl];
                fileProperties["Exists"] = true;
                return fileProperties;
            }
            else
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    if (serverRelativeUrl != null)
                    {
                        file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
                    }
                    else
                    {
                        file = web.GetFileById(id);
                    }
                    ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                    ExceptionHandlingScope excepScopeTrue = new ExceptionHandlingScope(context);
                    ExceptionHandlingScope excepScopeFalse = new ExceptionHandlingScope(context);
                    using (fileExistScope.StartScope())
                    {
                        using (fileExistScope.StartIfTrue())
                        {
                            if (serverRelativeUrl == null || !string.IsNullOrEmpty(listName))
                            {
                                context.Load(web.RegionalSettings);
                                SafeLoadFile(context, file, excepScopeTrue, excepScopeFalse);
                            }
                            else
                            {
                                context.Load(file);
                            }
                        }
                    }

                    try
                    {
                        context.ExecuteQuery();
                        if (excepScopeTrue.HasException || excepScopeFalse.HasException)
                        {
                            string errorMessage = excepScopeTrue.HasException ? excepScopeTrue.ErrorMessage : excepScopeFalse.ErrorMessage;
                            mLogger.Warn("Get File CheckedOutByUser Or Author Error, FileUrl:{0}, id:{1} , Error Message:{2}", serverRelativeUrl, id, errorMessage);
                        }
                        fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                        serverRelativeUrlVaild = Convert.ToBoolean(fileProperties["Exists"]);

                        if (file.ListItemAllFields.ServerObjectIsNull.HasValue && !file.ListItemAllFields.ServerObjectIsNull.Value)
                        {
                            //SAAS-27786
                            timeResult = web.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)file.ListItemAllFields["Modified"]);
                            context.ExecuteQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("An error occurred while getting file with url:{0} and id:{1} .Message:{2}.", serverRelativeUrl, id, ex.ToString());
                        fileProperties["Exists"] = false;
                        serverRelativeUrlVaild = false;
                    }
                }
            }

            if (!serverRelativeUrlVaild)
            {
                if (serverRelativeUrl != null)
                {
                    //Assemble file necessary properties for restore
                    fileProperties["Name"] = serverRelativeUrl.Substring(serverRelativeUrl.LastIndexOf('/') + 1);
                }
            }
            else
            {
                fileProperties["ListName"] = listName;

                AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
                // 使用UTC时间给modified 赋值                
                if (fileProperties.ContainsKey("Item" + AveObjectModelConstant.ObjectPropertySuffix)
                    && timeResult?.Value != null)
                {
                    Dictionary<string, object> itemProp = fileProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    if (itemProp.ContainsKey("FieldValues"))
                    {
                        (itemProp["FieldValues"] as Dictionary<string, object>)["Modified"] = timeResult.Value;
                    }
                }
            }
            return fileProperties;
        }

        private void SafeLoadFile(ClientContext context, ClientFile file, ExceptionHandlingScope excepScopeTrue, ExceptionHandlingScope excepScopeFalse)
        {
            ConditionalScope isListItem = new ConditionalScope(context, () => file.ListItemAllFields.ServerObjectIsNull.Value);
            using (isListItem.StartScope())
            {
                using (isListItem.StartIfTrue())
                {
                    using (excepScopeTrue.StartScope())
                    {
                        using (excepScopeTrue.StartTry())
                        {
                            context.Load(file, f => f.CheckedOutByUser, f => f.Author, f => f.ModifiedBy);
                            context.Load(file);
                        }
                        using (excepScopeTrue.StartCatch())
                        {
                            context.Load(file);
                        }
                    }
                }
                using (isListItem.StartIfFalse())
                {
                    using (excepScopeFalse.StartScope())
                    {
                        using (excepScopeFalse.StartTry())
                        {
                            context.Load(file, f => f.CheckedOutByUser, f => f.Author, f => f.ModifiedBy);
                            context.Load(file);
                            context.Load(file, f => f.ListItemAllFields);
                        }
                        using (excepScopeFalse.StartCatch())
                        {
                            context.Load(file);
                            context.Load(file, f => f.ListItemAllFields);
                        }
                    }
                }
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM)]
        public Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileVersionsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        //context.Load(file, f => f.Versions.IncludeWithDefaultProperties(v => v.CreatedBy));
                        context.Load(file.Versions, a => a.Include(
                                                                    b => b.Length,
                                                                    b => b.CheckInComment,
                                                                    b => b.Created,
                                                                    b => b.CreatedBy,
                                                                    b => b.ID,
                                                                    b => b.IsCurrentVersion,
                                                                    b => b.Url,
                                                                    b => b.VersionLabel));
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(file, f => f.Versions);
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Get FileVersions CreatedBy Error, FileUrl:{0} , Error Message:{1}", fileServerRelativeUrl, excepScope.ErrorMessage);
                }
                var versionList = new List<IDictionary<string, object>>();
                foreach (FileVersion fileVersion in file.Versions)
                {
                    Dictionary<string, object> versionProperties = new Dictionary<string, object>();
                    CopyProperty(versionProperties, fileVersion);
                    string createdby = "CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix;
                    if (!fileVersion.CreatedBy.ServerObjectIsNull.Value)
                    {
                        versionProperties[createdby] = fileVersion.CreatedBy.LoginName;
                    }
                    versionProperties["ServerRelativeUrl"] = webServerRelativeUrl.TrimEnd('/') + "/" + fileVersion.Url.TrimStart('/');
                    versionList.Add(versionProperties);
                }
                fileVersionsProperties.AddChildren(versionList);
                return fileVersionsProperties;
            }
        }

        [ClientOMRequest(ReadWrite.Read, MethodLevel.Item, MethodType.CSOM | MethodType.Rest | MethodType.WebService)]
        public Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source, Guid uniqueId, bool isSpecialList = false)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            if (SpecialFileList.Contains(System.IO.Path.GetExtension(fileServerRelativeUrl)) || isSpecialList)
            {
                return GetFileBinary(webServerRelativeUrl, fileServerRelativeUrl);
            }

            Stream stream = null;
            try
            {
                stream = GetFileStreamByRestApi(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl), fileServerRelativeUrl, uniqueId, WrapperConfiguration.OpenBinaryOptions);
            }
            catch (Exception e)
            {
                try
                {
                    mLogger.Error("Get file throught RestAPI failed. File:{0} Web:{1} Error:{2}", fileServerRelativeUrl, webServerRelativeUrl, e);
                    if (tokenProvider.TokenType != TokenType.Bearer)
                    {
                        string tempWebServerRelativeUrl = string.Empty;
                        if (source.Equals("File", StringComparison.OrdinalIgnoreCase))
                        {
                            string filePath = fileServerRelativeUrl;
                            if (!fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                filePath = AveUrlUtility.CombineUrl(webServerRelativeUrl, fileServerRelativeUrl);
                            }
                            if (CompatibilityLevel == 15)
                            {
                                tempWebServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/15/download.aspx?SourceUrl=");
                            }
                            else
                            {
                                tempWebServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/download.aspx?SourceUrl=");
                            }
                        }
                        AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(6, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                                       new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                                       new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                                       new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
                        retryHelper.ExecuteWithRetryMechanism(() =>
                        {
                            stream = mWebServiceRequest.GetFileStream(tempWebServerRelativeUrl, fileServerRelativeUrl, source);
                        });
                    }
                    else
                    {
                        stream = GetFileBinary(webServerRelativeUrl, fileServerRelativeUrl);
                    }
                }
                catch (Exception exception)
                {
                    mLogger.Error("Get file throught WebService failed. File:{0} Web:{1} Error:{2}", fileServerRelativeUrl, webServerRelativeUrl, exception);
                    stream = GetFileBinary(webServerRelativeUrl, fileServerRelativeUrl);
                }
            }
            return stream;
        }

        private Stream GetFileStreamByRestApi(string webUrl, string fileServerRelativeUrl, Guid uniqueId, AveOpenBinaryOptions option)
        {
            //此处需要将server url进行编码，并且保证空格的编码为“%20"
            if (fileServerRelativeUrl.Contains("'"))
            {
                fileServerRelativeUrl = fileServerRelativeUrl.Replace("'", "''");
            }

            string siteUrl = webUrl.TrimEnd('/');
            string request = string.Empty;
            if (uniqueId != null && uniqueId != Guid.Empty)
            {
                request = ToRestUri(siteUrl, uniqueId, ConvertOpenBinaryOption(option)).ToString();
            }
            else
            {
                request = ToRestUri(siteUrl, fileServerRelativeUrl, ConvertOpenBinaryOption(option)).ToString();
            }

            //string methodCmd = string.Empty;
            ////优先使用uniqueId去获取file，防止url过长导致获取失败
            //if (uniqueId != Guid.Empty && uniqueId != null)
            //{
            //    methodCmd = string.Format("GetFileById('{0}')", uniqueId);
            //}
            //else
            //{
            //    methodCmd = string.Format("GetFileByServerRelativePath(decodedUrl='{0}')", Uri.EscapeDataString(fileServerRelativeUrl));
            //}
            ////string methodCmd = string.Format("GetFileByServerRelativePath(decodedUrl='{0}')", Uri.EscapeDataString(fileServerRelativeUrl));

            //string request = string.Format("{0}/_api/Web/{1}/$value", webUrl.TrimEnd('/'), methodCmd);
            mLogger.Info("get file stream by restapi request (|{0}|0|{1}|): {2}", SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl, itemGuid: uniqueId), uniqueId, request);
            Stream stream = null;
            int retryInterval = IsOneNoteFile(fileServerRelativeUrl) ? 1 : 3;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(retryInterval, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "RApiFS");
            });
            return stream;
        }

        private Stream GetFileVersionStreamByRestApi(string webUrl, string fileServerRelativeUrl, int uiVersion, Guid uniqueId, AveOpenBinaryOptions option)
        {
            if (fileServerRelativeUrl.Contains("'"))
            {
                fileServerRelativeUrl = fileServerRelativeUrl.Replace("'", "''");
            }

            string request = string.Empty;
            string siteUrl = webUrl.TrimEnd('/');
            if (uniqueId != null && uniqueId != Guid.Empty)
            {
                request = ToRestUri(siteUrl, uniqueId, uiVersion, ConvertOpenBinaryOption(option)).ToString();
            }
            else
            {
                request = ToRestUri(siteUrl, fileServerRelativeUrl, uiVersion, ConvertOpenBinaryOption(option)).ToString();
            }

            //string methodCmd = string.Empty;
            ////优先使用id去获取file，防止url过长导致获取文件失败
            //if (uniqueId != null && uniqueId != Guid.Empty)
            //{
            //    methodCmd = string.Format("GetFileById('{0}')", uniqueId);
            //}
            //else
            //{
            //    methodCmd = string.Format("GetFileByServerRelativePath(decodedUrl='{0}')", Uri.EscapeDataString(fileServerRelativeUrl));
            //}
            //string versionCmd = string.Format("versions({0})", uiVersion);
            //string request = string.Format("{0}/_api/Web/{1}/{2}/$value", webUrl.TrimEnd('/'), methodCmd, versionCmd);
            mLogger.Info("get file version stream by restapi request (|{0}|{1}|{2}|): {3}", SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl, itemGuid: uniqueId), uiVersion, uniqueId, request);
            Stream stream = null;
            int retryInterval = IsOneNoteFile(fileServerRelativeUrl) ? 1 : 3;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(retryInterval, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", ""));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "FileVersionContentFS");
            });
            return stream;
        }


        private Uri ToRestUri(string siteUrl, Guid fileId, SPOpenBinaryOptions options)
        {
            return new Uri($"{siteUrl}/_api/web/getfilebyid('{fileId}')/OpenBinaryStreamWithOptions({(int)options})");
        }
        private Uri ToRestUri(string siteUrl, Guid fileId, int version, SPOpenBinaryOptions options)
        {
            return new Uri($"{siteUrl}/_api/web/getfilebyid('{fileId}')/versions({version})/OpenBinaryStreamWithOptions({(int)options})");
        }

        private Uri ToRestUri(string siteUrl, string fileServerRelativeUrl, SPOpenBinaryOptions options)
        {
            return new Uri($"{siteUrl}/_api/web/GetFileByServerRelativePath(decodedUrl='{fileServerRelativeUrl}')/OpenBinaryStreamWithOptions({(int)options})");
        }
        private Uri ToRestUri(string siteUrl, string fileServerRelativeUrl, int version, SPOpenBinaryOptions options)
        {
            return new Uri($"{siteUrl}/_api/web/GetFileByServerRelativePath(decodedUrl='{fileServerRelativeUrl}')/versions({version})/OpenBinaryStreamWithOptions({(int)options})");
        }

        private SPOpenBinaryOptions ConvertOpenBinaryOption(AveOpenBinaryOptions option)
        {
            switch (option)
            {
                case AveOpenBinaryOptions.Unprotected:
                    return SPOpenBinaryOptions.Unprotected;
                case AveOpenBinaryOptions.SkipVirusScan:
                    return SPOpenBinaryOptions.SkipVirusScan;
                case AveOpenBinaryOptions.MinimizeProcessing:
                    return SPOpenBinaryOptions.MinimizeProcessing;
                case AveOpenBinaryOptions.GetAsZipWithAltStreamsIfAvailable:
                    return SPOpenBinaryOptions.GetAsZipWithAltStreamsIfAvailable;
                case AveOpenBinaryOptions.ForceAVScanIfPreviouslyUnscanned:
                    return SPOpenBinaryOptions.ForceAVScanIfPreviouslyUnscanned;
                case AveOpenBinaryOptions.GetAsZipStreamBundleFriendly:
                    return SPOpenBinaryOptions.GetAsZipStreamBundleFriendly;
                case AveOpenBinaryOptions.SkipDisallowInfectedFileDownloadCheck:
                    return SPOpenBinaryOptions.SkipDisallowInfectedFileDownloadCheck;
                case AveOpenBinaryOptions.None:
                    return SPOpenBinaryOptions.None;
            }
            throw new NotSupportedException($"Not support option:{option.ToString()}");
        }

        private Stream GetContentStream(string cmd, string internalName)
        {
            ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(cmd, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);

            webRequest.SetTokenProvider(cmd, tokenProvider, false);
            return webRequest.GetResponsStreamEx(internalName);
        }

        public void ShareLinkByRestApi(int linkKind, string loginName, bool isDomainGroup, string parentWebUrl, Guid listId, int itemId, string roleValue = "")
        {
            string restUrl = linkKind != (int)AveSharingLinkKind.Flexible ? "{0}/_api/web/Lists(@a1)/GetItemById(@a2)/ShareLink?@a1='{1}'&@a2='{2}'" : "{0}/_api/web/Lists(@a1)/GetItemById(@a2)/ShareObject?@a1='{1}'&@a2='{2}'";
            string url = string.Format(restUrl, this.WebAppName + parentWebUrl, listId, itemId);
            string entityType = isDomainGroup ? "SecGroup" : "User";
            //string body = "{\"request\":{\"createLink\":true,\"settings\":{\"linkKind\":" + linkKind.ToString() + ",\"expiration\":null},\"peoplePickerInput\":\"[{\\\"Key\\\":\\\"" + loginName + "\\\",\\\"Description\\\":\\\"\\\",\\\"DisplayText\\\":\\\"\\\",\\\"EntityType\\\":\\\"" + entityType + "\\\",\\\"ProviderDisplayName\\\":\\\"Tenant\\\",\\\"ProviderName\\\":\\\"Tenant\\\",\\\"IsResolved\\\":true,\\\"EntityData\\\":{\\\"IsAltSecIdPresent\\\":\\\"False\\\",\\\"Title\\\":\\\"\\\",\\\"Email\\\":\\\"\\\",\\\"MobilePhone\\\":\\\"\\\",\\\"ObjectId\\\":\\\"\\\",\\\"Department\\\":\\\"\\\"}}]\",\"emailData\":{\"body\":\"\"}}}";
            string body = GetSharingLinkRequestBody(linkKind, loginName, entityType, roleValue);
            mLogger.Info($"Share link, {url},{body}");
            ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(url, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            webRequest.RefreshDigestInfo(url, tokenProvider);
            webRequest.SetTokenProvider(this.WebAppName + parentWebUrl, tokenProvider);

            var buffer = Encoding.UTF8.GetBytes(body);
            webRequest.ContentLength = buffer.Length;
            webRequest.Method = "POST";
            webRequest.Accept = "application/json;odata=verbose";
            webRequest.ContentType = "application/json;odata=verbose";

            Stream inputBody = webRequest.GetRequestStream();
            inputBody.Write(buffer, 0, buffer.Length);
            using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
            {
                if (result != null)
                {
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        mLogger.Error(string.Format("ShareLink Faild. Url:{0}, Body{1}, {2}", url, body, result.StatusCode.ToString()));
                        throw new WebException(string.Format("ShareLink Faild. Url:{0}, Body{1}, {2}", url, body, result.StatusCode.ToString()));
                    }
                }
                else
                {
                    mLogger.Error(string.Format("ShareLink Faild. Url:{0}, Body{1}.", url, body));
                    throw new WebException(string.Format("ShareLink Faild. Url:{0}, Body{1}.", url, body));
                }
            }
        }

        private string GetSharingLinkRequestBody(int linkKind, string loginName, string entityType, string roleValue)
        {
            string body = "";
            switch (linkKind)
            {
                case (int)AveSharingLinkKind.OrganizationEdit:
                case (int)AveSharingLinkKind.OrganizationView:
                    body = "{\"request\":{\"createLink\":true,\"settings\":{\"linkKind\":" + linkKind.ToString() + ",\"expiration\":null},\"peoplePickerInput\":\"[{\\\"Key\\\":\\\"" + loginName + "\\\",\\\"Description\\\":\\\"\\\",\\\"DisplayText\\\":\\\"\\\",\\\"EntityType\\\":\\\"" + entityType + "\\\",\\\"ProviderDisplayName\\\":\\\"Tenant\\\",\\\"ProviderName\\\":\\\"Tenant\\\",\\\"IsResolved\\\":true,\\\"EntityData\\\":{\\\"IsAltSecIdPresent\\\":\\\"False\\\",\\\"Title\\\":\\\"\\\",\\\"Email\\\":\\\"\\\",\\\"MobilePhone\\\":\\\"\\\",\\\"ObjectId\\\":\\\"\\\",\\\"Department\\\":\\\"\\\"}}]\",\"emailData\":{\"body\":\"\"}}}";
                    break;
                case (int)AveSharingLinkKind.Flexible:
                    JObject bodyObject = new JObject();
                    bodyObject["emailBody"] = null;
                    bodyObject["includeAnonymousLinkInEmail"] = new JValue(false);
                    bodyObject["propagateAcl"] = new JValue(true);
                    bodyObject["sendEmail"] = new JValue(true);
                    bodyObject["useSimplifiedRoles"] = new JValue(true);

                    //TODO: need accurate permissions
                    bodyObject["roleValue"] = new JValue(roleValue);
                    //"role:1073741827";//edit
                    //roleValue = role:1073741826 view

                    JObject peopleInput = new JObject();
                    peopleInput["key"] = new JValue(loginName);
                    peopleInput["Description"] = new JValue("");
                    peopleInput["DisplayText"] = new JValue("");
                    peopleInput["EntityType"] = new JValue(entityType);
                    peopleInput["ProviderDisplayName"] = new JValue("Tenant");
                    peopleInput["ProviderName"] = new JValue("Tenant");
                    peopleInput["IsResolved"] = new JValue(true);
                    peopleInput["EntityData"] = JObject.Parse("{\"IsAltSecIdPresent\":\"False\",\"Title\":\"\",\"Email\":\"\",\"MobilePhone\":\"\",\"ObjectId\":\"\",\"Department\":\"\"}");

                    bodyObject["peoplePickerInput"] = new JArray(peopleInput).ToString();
                    body = bodyObject.ToString();
                    break;
                case (int)AveSharingLinkKind.AnonymousEdit:
                case (int)AveSharingLinkKind.AnonymousView:
                    body = "{\"request\":{\"createLink\":true,\"settings\":{\"linkKind\":" + linkKind.ToString() + ",\"expiration\":null},\"peoplePickerInput\":\"[{\\\"Key\\\":\\\"" + loginName + "\\\",\\\"Description\\\":\\\"\\\",\\\"DisplayText\\\":\\\"\\\",\\\"EntityType\\\":\\\"" + entityType + "\\\",\\\"ProviderDisplayName\\\":\\\"Tenant\\\",\\\"ProviderName\\\":\\\"Tenant\\\",\\\"IsResolved\\\":true,\\\"EntityData\\\":{\\\"IsAltSecIdPresent\\\":\\\"False\\\",\\\"Title\\\":\\\"\\\",\\\"Email\\\":\\\"\\\",\\\"MobilePhone\\\":\\\"\\\",\\\"ObjectId\\\":\\\"\\\",\\\"Department\\\":\\\"\\\"}}]\",\"emailData\":{\"body\":\"\"}}}";
                    break;
                case (int)AveSharingLinkKind.Direct:
                case (int)AveSharingLinkKind.Uninitialized:
                    break;
            }
            return body;
        }

        public byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options, Guid uniqueId)
        {
            using (Stream stream = this.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, "File", uniqueId))
            {
                byte[] buffer = new byte[stream.Length];
                int len = 0;
                int position = 0;
                int count = stream.Length > 32768 ? 32768 : (int)stream.Length;
                while ((len = stream.Read(buffer, position, count)) != 0)
                {
                    position += len;
                    if (stream.Length - position < count)
                    {
                        count = (int)stream.Length - position;
                    }
                }
                return buffer;
            }
        }


        private Stream GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            bool needDisposeContext = true;
            AveClientContext context = CreateContext();
            try
            {
                //SAAS-22941 在OpenBinaryStream的时候偶尔出现以下异常，添加retry。
                //The specified program requires a newer version of Windows. (Exception from HRESULT: 0x8007047E)
                //Attempt to reuse a disposed CobaltStream.（无法避免国际化问题）
                ClientResult<Stream> content = null;
                AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x8007047E"), new KeyValuePair<string, string>("ServerException", "Attempt to reuse a disposed CobaltStream"));
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                    //content = file.OpenBinaryStream();
                    content = file.OpenBinaryStreamWithOptions(ConvertOpenBinaryOption(WrapperConfiguration.OpenBinaryOptions));
                    context.ExecuteQuery();
                });

                if(content?.Value.Length > AveSPDataStreamReader.USE_SPDATA_STREAM_READER_LIMIT)
                {
                    needDisposeContext = false;
                    return new AveSPDataStreamReader(content?.Value, content.Value.Length, context);
                }
                else
                {
                    //binary copy is required, cause ClientResult<Stream> can't be used after context is disposed
                    Stream binary = new AveCoordinatedStream("CApiFS");
                    AveIOHelper.Copy(content?.Value, binary);
                    try
                    {
                        content.Value.Dispose();
                    }
                    /*review-qlluo*/
                    catch (Exception e)
                    {
                        mLogger.Warn("Failed to dispose Client API file stream: {0}", e);
                    }
                    binary.Position = 0;
                    return binary;
                }
            }
            finally
            {
                if(needDisposeContext)
                {
                    context?.Dispose();
                }
            }
        }

        public Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo culture, Dictionary<string, string> needLoadFields, bool force)
        {
            var itemVersions = GetItemVersionsViaCSOM(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, culture, needLoadFields, force);

            //#if DEBUG
            //            if (tokenProvider.TokenType != TokenType.Bearer)
            //            {
            //                var itemVersionsByWebService = GetItemVersionsViaWebService(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, culture, needLoadFields, force);

            //                Compare("ItemVersions", itemVersions, itemVersionsByWebService);
            //            }
            //#endif

            return itemVersions;
        }

#if DEBUG
        private void Compare(string key, Dictionary<string, object> source, Dictionary<string, object> destination)
        {
            if (source != null && destination != null)
            {
                if (source.Count != destination.Count)
                {
                    mLogger.Error("Key:{0}, source keys:{1}, destination keys:{2}", key,
                        string.Join(", ", source.Keys.Where(a => !destination.ContainsKey(a))),
                        string.Join(", ", destination.Keys.Where(a => !source.ContainsKey(a))));
                }

                foreach (KeyValuePair<string, object> keyValue in source)
                {
                    object targetValue;
                    if (destination.TryGetValue(keyValue.Key, out targetValue))
                    {
                        Compare(keyValue.Key, keyValue.Value, targetValue);
                    }
                }
            }
            else
            {
                mLogger.Error("Key:{0}, source keys:{1}, destination keys:{2}", key, source, destination);
            }
        }

        private void Compare(string key, List<Dictionary<string, object>> source, List<Dictionary<string, object>> destination)
        {
            if (source != null && destination != null)
            {
                if (source.Count != destination.Count)
                {
                    mLogger.Error("Key:{0}, source keys:{1}, destination keys:{2}", key, source.Count, destination.Count);
                }

                for (var index = 0; index < source.Count && index < destination.Count; index++)
                {
                    Compare(key + "_" + index, source[index], destination[index]);
                }
            }
            else
            {
                mLogger.Error("Key:{0}, source keys:{1}, destination keys:{2}", key, source, destination);
            }
        }

        private void Compare(string key, Object sourceObj, Object destinationObj)
        {
            if (!object.Equals(sourceObj, destinationObj))
            {
                if (sourceObj == null || destinationObj == null)
                {
                    mLogger.Error("Key:{0}, source:{1}, destination:{2}", key, sourceObj, destinationObj);
                }
                else
                {
                    var sourceType = sourceObj.GetType();

                    if (sourceType == typeof(string) && string.Compare((string)sourceObj, (string)destinationObj, StringComparison.Ordinal) == 0)
                    {

                    }
                    else if (sourceType.IsValueType || sourceType.IsEnum)
                    {
                        mLogger.Error("Key:{0}, source:{1}, destination:{2}", key, sourceObj, destinationObj);
                    }
                    else if (sourceType == typeof(List<Dictionary<string, object>>))
                    {
                        Compare(key, (List<Dictionary<string, object>>)sourceObj, (List<Dictionary<string, object>>)destinationObj);
                    }
                    else if (sourceType == typeof(Dictionary<string, object>))
                    {
                        Compare(key, (Dictionary<string, object>)sourceObj, (Dictionary<string, object>)destinationObj);
                    }
                    else
                    {
                        mLogger.Error("Key:{0}, source:{1}, destination:{2} -- unsupported type", key, sourceObj, destinationObj);
                    }
                }
            }
        }
#endif 

        private Dictionary<string, object> GetItemVersionsViaCSOM(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo culture, Dictionary<string, string> needLoadFields, bool force)
        {
            Dictionary<string, object> itemVersions = new Dictionary<string, object>();

            using (var context = CreateRetryContext(AveUrlUtility.GetServerUrl(mWebUrl) + webRelativeUrl.TrimStart('/')))
            {
                List list = context.Web.Lists.GetById(new Guid(listId));
                ListItem item = list.GetItemById(itemId);
                var listItemVersions = item.Versions;
                // Set the backup version count as WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount * 5 in case the versions changed during the time gap between discovery and backup
                int backupVersionCount = WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount * 5;
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(listItemVersions, v => v.IncludeWithDefaultProperties(
                        a => a.CreatedBy,
                        a => a.FileVersion.CheckInComment,
                        a => a.FileVersion.Created).Take(backupVersionCount));
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(listItemVersions, v => v.IncludeWithDefaultProperties(
                        a => a.FileVersion.CheckInComment,
                        a => a.FileVersion.Created).Take(backupVersionCount));
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Failed GetItemVersionsViaCSOM.listID:{0}.listRelativeUrl:{1}.itemId:{2}.Message:{3}.", listId, listRelativeUrl, itemId, excepScope.ErrorMessage);
                }

                if (listItemVersions.Count > 0)
                {
                    var versions = new List<IDictionary<string, object>>();
                    itemVersions.AddChildren(versions);
                    foreach (ListItemVersion listItemVersion in listItemVersions)
                    {
                        Dictionary<string, object> listItemVersionData = new Dictionary<string, object>();
                        Dictionary<string, object> listItemVersionFieldValue = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> fieldValue in listItemVersion.FieldValues)
                        {
                            if (needLoadFields.ContainsKey(fieldValue.Key) ||
                                fieldValue.Key.Equals("Editor", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("_CheckinComment", StringComparison.OrdinalIgnoreCase))
                            {
                                AssembleItemProperties(listItemVersionFieldValue, fieldValue.Value, fieldValue.Key);
                            }
                        }


                        ///这么做的原因是因为List Item Version的Field Values取出来的check in comment是所有version都有，
                        ///可能是通过current version赋值的，所以使用下面这种方式。
                        string checkinComment = null;

                        if (listItemVersion.IsObjectPropertyInstantiated("FileVersion") && listItemVersion.FileVersion.IsPropertyAvailable("CheckInComment"))
                        {
                            checkinComment = listItemVersion.FileVersion.CheckInComment;
                        }
                        else
                        {
                            object checkinCommentObj;
                            if (listItemVersion.FieldValues.TryGetValue("_CheckinComment", out checkinCommentObj))
                            {
                                checkinComment = checkinCommentObj as string;
                            }
                        }

                        if (checkinComment != null)
                        {
                            listItemVersionFieldValue["_CheckinComment"] = checkinComment;
                            listItemVersionData["_CheckinComment"] = checkinComment;
                        }

                        /* Old comment
                        //这个是list item version的created时间，不是界面上显示的那个created，界面上显示是这个文件第一次被创建的时间。
                        //listItemVersionFieldValue["Created"] = listItemVersion.Created;
                        */
                        /*New Comment
                        //Created 在创建item的时候能顺带更新这个属性，页面上显示的item Created是第一个version的Created
                        //Created_x0020_Date 不可更新，SharePoint记录的当前时间。
                        */
                        listItemVersionFieldValue["Created"] = listItemVersion.Created;
                        //object createdObj;
                        //if (listItemVersion.FieldValues.TryGetValue("Created_x0020_Date", out createdObj))
                        //{
                        //    //format : Created_x0020_Date=2017-10-11T02:46:15Z
                        //    //format is u.
                        //    listItemVersionFieldValue["Created"] = DateTime.Parse(createdObj as string, new CultureInfo("en-US", false)).ToUniversalTime();
                        //}
                        //else
                        //{
                        //    listItemVersionFieldValue["Created"] = listItemVersion.Created;
                        //}

                        listItemVersionData.Add("FieldValues", listItemVersionFieldValue);

                        //listItemVersionData["Created"] = listItemVersion.Created;
                        listItemVersionData["Modified"] = listItemVersion.FieldValues["Modified"];
                        listItemVersionData["Editor"] = listItemVersionFieldValue["Editor"];
                        listItemVersionData["VersionId"] = listItemVersion.VersionId;
                        listItemVersionData["VersionLabel"] = listItemVersion.VersionLabel;

                        listItemVersionData["Level"] = byte.Parse(listItemVersion.FieldValues["_Level"].ToString());
                        listItemVersionData["IsCurrentVersion"] = listItemVersion.IsCurrentVersion;
                        listItemVersionData["Url"] = listItemVersion.FieldValues["FileRef"];

                        object length;
                        if (listItemVersion.FieldValues.TryGetValue("File_x0020_Size", out length))
                        {
                            listItemVersionData["Length"] = length;
                        }
                        listItemVersionData["ModerationStatus"] = listItemVersion.FieldValues["_ModerationStatus"];
                        if (!listItemVersion.IsPropertyAvailable("CreatedBy") || listItemVersion.CreatedBy.ServerObjectIsNull == true)
                        {
                            mLogger.Info("Current listItemVersion CreatedBy not Available or ServerObjectIsNull.listID:{0}.listRelativeUrl:{1}.itemId:{2}.", listId, listRelativeUrl, itemId);
                            listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = null;
                        }
                        else
                        {
                            listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = listItemVersion.CreatedBy.LoginName;
                        }
                        versions.Add(listItemVersionData);
                    }
                }
                else
                {
                    itemVersions["HasVersion"] = false;
                }
            }

            return itemVersions;
        }

        public Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource)
        {
            var users = GetUsersViaCSOM(webRelativeUrl, groupName, userColSource);

            //#if DEBUG
            //            if (tokenProvider.TokenType != TokenType.Bearer)
            //            {
            //                var usersByWebService = mWebServiceRequest.GetUsers(webRelativeUrl, groupName, userColSource);

            //                Compare("users", users, usersByWebService);
            //            }
            //#endif
            return users;
        }

        public Dictionary<string, object> GetUsersViaCSOM(string webRelativeUrl, string groupName, string userColSource)
        {
            switch (userColSource)
            {
                case "web.users":
                    return GetUsers(webRelativeUrl);
                case "web.allUsers":
                case "web.siteUsers":
                    return GetSiteUsers(webRelativeUrl);
                case "group.users":
                    return GetGroupUsers(webRelativeUrl, groupName);
                default:
                    throw new Exception("unsupported source:" + userColSource);
            }
        }

        private Dictionary<string, object> GetSiteUsers(string webRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web.SiteUsers, a => a.IncludeWithDefaultProperties());
                context.ExecuteQuery();

                return ConvertUserCollection(web.SiteUsers);
            }
        }

        private Dictionary<string, object> ConvertUserCollection(UserCollection users)
        {
            var userCollectionProperties = new Dictionary<string, object>();
            var userPropertiesList = new List<IDictionary<string, object>>();
            userCollectionProperties.AddChildren(userPropertiesList);

            foreach (var user in users)
            {
                userPropertiesList.Add(ConvertUser(user));
            }

            return userCollectionProperties;
        }

        private Dictionary<string, object> ConvertUser(User user)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            CopyProperty(userProperties, user);

            if (user.IsPropertyAvailable("UserId") && user.UserId != null)
            {
                if ((!string.IsNullOrEmpty(user.UserId.NameId)) && user.UserId.NameId.StartsWith("S-", StringComparison.OrdinalIgnoreCase))
                {
                    userProperties.Add("SID", user.UserId.NameId);
                }
            }

            if (user.PrincipalType != PrincipalType.User)
            {
                userProperties.Add("IsDomainGroup", true);
            }

            userProperties.Remove("Title");
            userProperties.Add("Name", user.Title);

            return userProperties;
        }

        private Dictionary<string, object> GetUsers(string webRelativeUrl)
        {
            var userCollectionProperties = new Dictionary<string, object>();
            var userPropertiesList = new List<IDictionary<string, object>>();
            userCollectionProperties.AddChildren(userPropertiesList);

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web, a => a.HasUniqueRoleAssignments,
                    a => a.RoleAssignments.Include(r => r.Member),
                    a => a.RoleAssignments.Groups.Include(g => g.Users.IncludeWithDefaultProperties()));
                context.ExecuteQuery();

                if (web.HasUniqueRoleAssignments)
                {
                    var userId = new HashSet<int>();
                    foreach (var role in web.RoleAssignments)
                    {
                        var user = role.Member as User;

                        if (user != null)
                        {
                            userId.Add(user.Id);
                            userPropertiesList.Add(ConvertUser(user));
                        }
                    }

                    foreach (var group in web.RoleAssignments.Groups)
                    {
                        foreach (var user in group.Users)
                        {
                            if (!userId.Contains(user.Id))
                            {
                                userId.Add(user.Id);
                                userPropertiesList.Add(ConvertUser(user));
                            }
                        }
                    }
                }
            }

            return userCollectionProperties;
        }

        private Dictionary<string, object> GetGroupUsers(string webRelativeUrl, string groupName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                var group = web.SiteGroups.GetByName(groupName);
                context.Load(group.Users, a => a.IncludeWithDefaultProperties());
                context.ExecuteQuery();

                return ConvertUserCollection(group.Users);
            }
        }

        public Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                string UrlPrefix = string.Empty;
                Dictionary<string, object> attachmentCollection = new Dictionary<string, object>();
                var attachmentPropertiesList = new List<IDictionary<string, object>>();
                try
                {
                    Web web = context.Site.OpenWeb(webRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    ListItem item = list.GetItemById(itemId);
                    AttachmentCollection attachments = item.AttachmentFiles;
                    context.Load(attachments);
                    context.ExecuteQuery();
                    foreach (Attachment attachment in attachments)
                    {
                        Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                        AssembleAttachmentProperties(attachment, attachmentProperties);
                        attachmentPropertiesList.Add(attachmentProperties);
                    }
                    if (attachments.Count > 0)
                    {
                        UrlPrefix = attachments[0].ServerRelativeUrl;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("failed to get attachments due to: {0}", e.ToString());
                }
                attachmentCollection.AddChildren(attachmentPropertiesList);
                UrlPrefix = UrlPrefix.Substring(0, UrlPrefix.LastIndexOf('/') + 1);
                attachmentCollection.Add("UrlPrefix", UrlPrefix);
                return attachmentCollection;
            }
        }
        internal static void AssembleAttachmentProperties(Attachment attachment, Dictionary<string, object> attachmentProperties)
        {
            CopyProperty(attachmentProperties, attachment);
        }

        public Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            if (groupColSource.Equals("web.siteGroups", StringComparison.OrdinalIgnoreCase))
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webRelativeUrl);
                    context.Load(web.SiteGroups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                    context.ExecuteQuery();
                    Dictionary<string, object> groups = new Dictionary<string, object>();
                    var groupList = new List<IDictionary<string, object>>();
                    foreach (Group group in web.SiteGroups)
                    {
                        Dictionary<string, object> groupProp = GetGroupProperties(context, group, true);
                        groupList.Add(groupProp);
                    }
                    groups.AddChildren(groupList);
                    return groups;
                }
            }
            else if (groupColSource.Equals("web.groups", StringComparison.OrdinalIgnoreCase))
            {
                var groups = GetWebGroups(webRelativeUrl);
                //#if DEBUG
                //                if (tokenProvider.TokenType != TokenType.Bearer)
                //                {
                //                    var groupsByWebService = mWebServiceRequest.GetGroups(webRelativeUrl, groupColSource, loginName);

                //                    Compare(groupColSource, groups, groupsByWebService);
                //                }
                //#endif

                return groups;
            }
            else if (groupColSource.Equals("user.groups", StringComparison.OrdinalIgnoreCase))
            {
                var groups = GetUserGroups(webRelativeUrl, loginName);
                //#if DEBUG
                //                if (tokenProvider.TokenType != TokenType.Bearer)
                //                {
                //                    var groupsByWebService = mWebServiceRequest.GetGroups(webRelativeUrl, groupColSource, loginName);

                //                    Compare(groupColSource, groups, groupsByWebService);
                //                }
                //#endif

                return groups;
            }

            throw new System.NotImplementedException(groupColSource);
        }

        private Dictionary<string, object> GetWebGroups(string webRelativeUrl)
        {
            Dictionary<string, object> groups = new Dictionary<string, object>();
            var groupList = new List<IDictionary<string, object>>();
            groups.AddChildren(groupList);

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web, a => a.HasUniqueRoleAssignments,
                    a => a.RoleAssignments.Groups.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                context.ExecuteQuery();

                if (web.HasUniqueRoleAssignments)
                {
                    foreach (Group group in web.RoleAssignments.Groups)
                    {
                        Dictionary<string, object> groupProp = GetGroupProperties(context, group, true);
                        groupList.Add(groupProp);
                    }
                }
            }

            return groups;
        }

        private Dictionary<string, object> GetUserGroups(string webRelativeUrl, string loginname)
        {
            Dictionary<string, object> groups = new Dictionary<string, object>();
            var groupList = new List<IDictionary<string, object>>();
            groups.AddChildren(groupList);

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);

                var user = web.SiteUsers.GetByLoginName(loginname);
                context.Load(user.Groups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                context.ExecuteQuery();

                foreach (Group group in user.Groups)
                {
                    Dictionary<string, object> groupProp = GetGroupProperties(context, group, true);
                    groupList.Add(groupProp);
                }
            }

            return groups;
        }



        public Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        {
            Dictionary<string, object> files = new Dictionary<string, object>();
            var fileList = new List<IDictionary<string, object>>();
            Folder folder = null;
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(folder);
                    context.Load(folder.ParentFolder, f => f.ServerRelativeUrl);
                    if (string.IsNullOrEmpty(listName))
                    {
                        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.CheckedOutByUser, file => file.Author, file => file.ModifiedBy));
                            }
                            using (excepScope.StartCatch())
                            {
                                context.Load(folder, f => f.Files);
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope.HasException)
                        {
                            mLogger.Warn("Get Files CheckedOutByUser Or Author Error, FolderUrl:{0} , Error Message:{1}", folderServerRelativeUrl, excepScope.ErrorMessage);
                        }
                    }
                    else
                    {
                        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.ListItemAllFields, file => file.ListItemAllFields.HasUniqueRoleAssignments, file => file.CheckedOutByUser, file => file.Author, file => file.ModifiedBy));
                            }
                            using (excepScope.StartCatch())
                            {
                                context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.ListItemAllFields, file => file.ListItemAllFields.HasUniqueRoleAssignments));
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope.HasException)
                        {
                            mLogger.Warn("Get Files CheckedOutByUser Or Author Error, FolderUrl:{0} , Error Message:{1}", folderServerRelativeUrl, excepScope.ErrorMessage);
                        }
                    }
                    foreach (ClientFile file in folder.Files)
                    {
                        Dictionary<string, object> fileProp = new Dictionary<string, object>();
                        fileProp["Exists"] = true;
                        fileProp["ListName"] = listName;
                        AssembleFileProperties(fileProp, file, webServerRelativeUrl, file.ListItemAllFields);
                        fileProp["Versions"] = new List<Dictionary<string, object>>(); // need to fill it later
                        fileList.Add(fileProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn(string.Format("get files failed, parent folder url: {0}", folderServerRelativeUrl), e);
                }
                files.AddChildren(fileList);
                return files;
            }
        }
        public Dictionary<string, object> GetListTemplates(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> listTemplates = new Dictionary<string, object>();
                var listTemplateList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.ListTemplates);
                context.ExecuteQuery();
                foreach (ListTemplate listTemplate in web.ListTemplates)
                {
                    Dictionary<string, object> listTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(listTemplateProperties, listTemplate);
                    listTemplateProperties["Type"] = listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateProperties["Type_Client"] = (int)listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateList.Add(listTemplateProperties);
                }
                listTemplates.AddChildren(listTemplateList);
                return listTemplates;
            }
        }
        public Dictionary<string, object> GetAvailableFields(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.AvailableFields);
                context.ExecuteQuery();
                Dictionary<string, object> availableFieldsProperties = new Dictionary<string, object>();
                var availableFieldList = new List<IDictionary<string, object>>();
                foreach (Field field in web.AvailableFields)
                {
                    Dictionary<string, object> availableFieldProperties = new Dictionary<string, object>();
                    CopyProperty(availableFieldProperties, field);
                    availableFieldList.Add(availableFieldProperties);
                }
                availableFieldsProperties.AddChildren(availableFieldList);
                return availableFieldsProperties;
            }
        }
        public Dictionary<string, object> GetAvailableContentTypes(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                var availableContentTypes = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.AvailableContentTypes);
                context.ExecuteQuery();
                foreach (ContentType c in web.AvailableContentTypes)
                {
                    Dictionary<string, object> availableContentTypeProperties = new Dictionary<string, object>();
                    CopyProperty(availableContentTypeProperties, c);
                    availableContentTypeProperties["Id"] = c.Id.ToString();
                    availableContentTypes.Add(availableContentTypeProperties);
                }
                returnInfo.AddChildren(availableContentTypes);
                web = null;
                return returnInfo;
            }
        }

        public Dictionary<string, object> GetSiteGroupsWithUsers(string webRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                mLogger.Info("Start get group include members");
                context.Load(web.SiteGroups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType, g => g.IsHiddenInUI, g => g.Users.IncludeWithDefaultProperties()));
                context.ExecuteQuery();
                mLogger.Info("Finish get group include members");
                Dictionary<string, object> groups = new Dictionary<string, object>();
                var groupList = new List<IDictionary<string, object>>();
                foreach (Group group in web.SiteGroups)
                {
                    Dictionary<string, object> groupProp = GetGroupProperties(context, group, true, true);
                    groupList.Add(groupProp);
                }
                groups.AddChildren(groupList);
                return groups;
            }
        }

        public Dictionary<string, object> GetSiteGroups(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.SiteGroups);
                context.ExecuteQuery();
                Dictionary<string, object> siteGroups = new Dictionary<string, object>();
                var siteGroupList = new List<IDictionary<string, object>>();
                foreach (Group siteGroup in web.SiteGroups)
                {
                    Dictionary<string, object> siteGroupProperties = GetGroupProperties(context, siteGroup, false);
                    siteGroupList.Add(siteGroupProperties);
                }
                siteGroups.AddChildren(siteGroupList);
                return siteGroups;
            }
        }
        public Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                User ensureUser = web.EnsureUser(loginName);
                context.Load(ensureUser);
                context.ExecuteQuery();
                Dictionary<string, object> ensureUserProperties = new Dictionary<string, object>();
                CopyProperty(ensureUserProperties, ensureUser);
                ensureUserProperties["Name"] = ensureUser.Title;
                return ensureUserProperties;
            }
        }
        public Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.GetCatalog(typeCatalog);
                this.LoadList(context, list);
                Dictionary<string, object> listProperties = new Dictionary<string, object>();
                CopyProperty(listProperties, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                CopyProperty(rootFolderProp, list.RootFolder);
                listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProperties;
            }
        }
        public Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebTemplateCollection webTemplateCollection = web.GetAvailableWebTemplates(lcid, doIncludeCrossLanguage);
                context.Load(webTemplateCollection);
                context.ExecuteQuery();
                Dictionary<string, object> webTemplates = new Dictionary<string, object>();
                var webTemplateList = new List<IDictionary<string, object>>();
                foreach (WebTemplate webTemplate in webTemplateCollection)
                {
                    Dictionary<string, object> webTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(webTemplateProperties, webTemplate);
                    webTemplateList.Add(webTemplateProperties);
                }
                webTemplates.AddChildren(webTemplateList);
                return webTemplates;
            }
        }

        public Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentColProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        List list = web.Lists.GetById(listId);
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        List list1 = web.Lists.GetById(listId);
                        ListItem listItem = list1.GetItemById(itemId);
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;

                }
                context.Load(roleAssignmentCol, i => i.Include(r => r.PrincipalId, r => r.RoleDefinitionBindings.Include(b => b.Id)));
                //context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(r => r.RoleDefinitionBindings, r => r.Member));
                context.ExecuteQuery();
                AssembleRoleAssignmetsProperites(roleAssignmentColProperties, roleAssignmentCol);
                return roleAssignmentColProperties;
            }
        }
        public Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl)
        {
            string webUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl; // SAAS-26366 想获取当前web的definition 必须使用当前web 的context 
            using (AveClientContext context = CreateContext(webUrl))
            {
                Dictionary<string, object> roleDefinitionColProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web.RoleDefinitions);
                context.ExecuteQuery();
                AssembleRoleDefinitionsProperties(roleDefinitionColProperties, webServerRelativeUrl, web.RoleDefinitions);
                return roleDefinitionColProperties;
            }
        }

        public Dictionary<string, object> GetUserSolutions()
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
                List solutionGallery = context.Site.GetCatalog((int)AveListTemplateType.SolutionCatalog);
                ListItemCollection solutionItems = solutionGallery.GetItems(CamlQuery.CreateAllItemsQuery());
                context.Load(solutionItems);
                context.ExecuteQuery();
                var solutionList = new List<IDictionary<string, object>>();
                foreach (var tempItem in solutionItems)
                {
                    try
                    {
                        var itemProperties = new Dictionary<string, object>();
                        itemProperties = AssembleSolutionProperties(tempItem);
                        solutionList.Add(itemProperties);
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("get solution item failed. error message:{0}", e.ToString());
                    }
                }
                solutionProperties.AddChildren(solutionList);
                return solutionProperties;
            }
        }

        //public Dictionary<string, object> OperateOnSolution(string operation, int id)
        //{
        //    Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
        //    if (tokenProvider.TokenType != TokenType.Bearer)
        //    {
        //        AveWebServiceRequest.OperateOnSolution(operation, mWebUrl, AveUrlUtility.GetServerRelativeUrl(mWebUrl), id, tokenProvider);
        //    }
        //    using (AveClientContext context = CreateContext())
        //    {
        //        List solutionGallery = context.Site.GetCatalog(0x79);
        //        ListItem solutionItem = solutionGallery.GetItemById(id);
        //        context.Load(solutionItem);
        //        context.ExecuteQuery();
        //        solutionProperties = AssembleSolutionProperties(solutionItem);
        //        return solutionProperties;
        //    }
        //}

        public Dictionary<string, object> LoadSolution(int id)
        {
            Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                List solutionGallery = context.Site.GetCatalog(0x79);
                ListItem solutionItem = solutionGallery.GetItemById(id);
                context.Load(solutionItem);
                context.ExecuteQuery();
                solutionProperties = AssembleSolutionProperties(solutionItem);
                return solutionProperties;
            }
        }

        private Dictionary<string, object> AssembleSolutionProperties(ListItem solutionItem)
        {
            Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
            GetItemDic(solutionProperties, solutionItem);
            Dictionary<string, object> fieldValues = solutionProperties["FieldValues"] as Dictionary<string, object>;
            string[] needAssembleProperties = { "SolutionId", "Status", "HasAssemblies", "Name", "Signature" };
            foreach (var fieldName in needAssembleProperties)
            {
                if (solutionItem.FieldValues.ContainsKey(fieldName) && solutionItem[fieldName] != null)
                {
                    if (fieldName == "Status")
                    {
                        int statusValue = 0;
                        int.TryParse((solutionItem[fieldName] as FieldLookupValue).LookupValue, out statusValue);
                        if (statusValue > 0)
                        {
                            solutionProperties[fieldName] = statusValue;
                        }
                        //solutionProperties[fieldName] = int.Parse((solutionItem[fieldName] as FieldLookupValue).LookupValue);
                        continue;
                    }
                    solutionProperties[fieldName] = solutionItem[fieldName];
                }
            }
            return solutionProperties;
        }

        public Dictionary<string, object> GetWebApplication()
        {
            return null;
        }
        public virtual Dictionary<string, object> GetAlerts(string webServerRelativeUrl)
        {
            return null;
            //var alertPropertiesList = new List<IDictionary<string, object>>();
            //using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            //{
            //    Web web = context.Web;
            //    context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl));
            //    context.ExecuteQuery();
            //    foreach (var alert in web.Alerts)
            //    {
            //        LoadAlertSpecialProperty(context, alert);
            //    }
            //    if (context.HasPendingRequest)
            //    {
            //        context.ExecuteQuery();
            //    }
            //    foreach (var alert in web.Alerts)
            //    {
            //        Dictionary<string, object> alertProperties = LoadAlertProprty(alert);
            //        alertPropertiesList.Add(alertProperties);
            //    }
            //}
            //var result = new Dictionary<string, object>();
            //result.AddChildren(alertPropertiesList);
            //return result;
        }

        public Dictionary<string, object> GetAlertsV2(string webServerRelativeUrl)
        {
            var alertPropertiesList = new List<IDictionary<string, object>>();
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl, alert => alert.ID));
                context.ExecuteQuery();
                foreach (var alert in web.Alerts)
                {
                    LoadAlertSpecialProperty(context, alert);
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
                foreach (var alert in web.Alerts)
                {
                    Dictionary<string, object> alertProperties = LoadAlertProprty(alert);
                    alertPropertiesList.Add(alertProperties);
                }
            }
            var result = new Dictionary<string, object>();
            result.AddChildren(alertPropertiesList);
            return result;
        }

        private void LoadAlertSpecialProperty(AveClientContext context, Alert alert)
        {
            if (alert.AlertType == AlertType.Item)
            {
                context.Load(alert, al => al.ItemID);
            }
            if (alert.AlertFrequency != AlertFrequency.Immediate)
            {
                context.Load(alert, al => al.AlertTime);
            }
        }

        private Dictionary<string, object> LoadAlertProprty(Alert alert)
        {
            Dictionary<string, object> alertProperties = new Dictionary<string, object>();
            CopyProperty(alertProperties, alert);

            #region Reset Properties 
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var property in alert.Properties)
            {
                properties.Add(property.Key, property.Value);
            }
            alertProperties.Add("Properties" + AveObjectModelConstant.ObjectPropertySuffix, properties);
            alertProperties.Remove("Properties");
            #endregion
            return alertProperties;
        }

        public void DisableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds)
        {
            if (disableAlertIds == null || disableAlertIds.Count == 0)
            {
                mLogger.Info($"DisableAlert.disableAlertIds is null.WebURL:{webServerRelativeUrl}.");
            }
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl));
                context.ExecuteQuery();
                mLogger.Info($"DisableAlert.Web:{webServerRelativeUrl}.Alerts Count:{web.Alerts.Count}.");
                bool needUpdateAlert = false;
                foreach (var alert in web.Alerts)
                {
                    if (disableAlertIds.Contains(alert.ID))
                    {
                        if (alert.Status == AlertStatus.On)
                        {
                            mLogger.Info($"DisableAlert.alert is On.WebURL:{webServerRelativeUrl}.AlertName:{alert.Title}.AlertId:{alert.ID}.AlertListID:{alert.ListID}.AlertListURL:{alert.ListUrl}.");
                            needUpdateAlert = true;
                            alert.Status = AlertStatus.Off;
                            alert.UpdateAlert();
                        }
                        else
                        {
                            mLogger.Info($"DisableAlert.alert is Off.WebURL:{webServerRelativeUrl}.AlertName:{alert.Title}.AlertId:{alert.ID}.AlertListID:{alert.ListID}.AlertListURL:{alert.ListUrl}.");
                        }
                    }
                }
                if (needUpdateAlert)
                {
                    context.ExecuteQuery();
                    mLogger.Info($"DisableAlert success.");
                }
            } 
        }

        public void EnableAlert(string webServerRelativeUrl, List<Guid> enableAlertIds)
        {
            if (enableAlertIds == null || enableAlertIds.Count == 0)
            {
                mLogger.Info($"EnableAlert.enableAlertIds is null.WebURL:{webServerRelativeUrl}.");
            }
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl));
                context.ExecuteQuery();
                mLogger.Info($"EnableAlert.Web:{webServerRelativeUrl}.Alerts Count:{web.Alerts.Count}.");
                bool needUpdateAlert = false;
                foreach (var alert in web.Alerts)
                {
                    if (enableAlertIds.Contains(alert.ID))
                    {
                        if (alert.Status == AlertStatus.Off)
                        {
                            mLogger.Info($"EnableAlert.alert is Off.WebURL:{webServerRelativeUrl}.AlertName:{alert.Title}.AlertId:{alert.ID}.AlertListID:{alert.ListID}.AlertListURL:{alert.ListUrl}.");
                            needUpdateAlert = true;
                            alert.Status = AlertStatus.On;
                            alert.UpdateAlert();
                        }
                        else
                        {
                            mLogger.Info($"EnableAlert.alert is On.WebURL:{webServerRelativeUrl}.AlertName:{alert.Title}.AlertId:{alert.ID}.AlertListID:{alert.ListID}.AlertListURL:{alert.ListUrl}.");
                        }
                    }
                }
                if (needUpdateAlert)
                {
                    context.ExecuteQuery();
                    mLogger.Info($"EnableAlert success.");
                }
            }
        }

        public Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, List<string> supportedResourceCultureNames)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> Fields = new List<Dictionary<string, object>>();
                ContentTypeCollection contentTypes = this.GetContentTypesWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                context.ExecuteQuery();
                if (supportedResourceCultureNames != null && supportedResourceCultureNames.Count > 0)
                {
                    try
                    {
                        foreach (var languageName in supportedResourceCultureNames)
                        {
                            foreach (var ct in contentTypes)
                            {
                                ct.NameResource.GetValueForUICulture(languageName);
                                ct.DescriptionResource.GetValueForUICulture(languageName);
                            }
                        }
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Error while query ContentType user resource, web url:{0}, contentType source:{1}, list id:{2}, error:{3}", webServerRelativeUrl, contentTypeSource, listId, ex);
                    }
                }
                AssembleContentTypesProperties(contentTypeProperties, contentTypes);
                return contentTypeProperties;
            }
        }

        public Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp, List<string> supportedResourceCultureNames)
        {
            using (var context = CreateRetryContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                Dictionary<string, object> fieldsProperties = new Dictionary<string, object>();
                var fieldList = new List<IDictionary<string, object>>();
                Web web = context.Web;
                FieldCollection fieldCollection = null;

                switch (fieldSource)
                {
                    case "web.fields":
                        fieldCollection = web.Fields;
                        break;
                    case "web.availableFields":
                        fieldCollection = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fieldCollection = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listTitle, listId, contentTypeSource, id);
                        fieldCollection = contentType.Fields;
                        break;
                    default:
                        break;
                }
                ArgumentCheck.CheckNotNull(fieldCollection);
                context.Load(fieldCollection);
                context.ExecuteQuery();
                fieldsProperties["SchemaXml"] = fieldCollection?.SchemaXml;
                try
                {
                    if (WrapperConfiguration.EnableUseWorkingLanguage)
                    {
                        mLogger.Info("SchemaXml:" + fieldCollection?.SchemaXml);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(fieldCollection?.SchemaXml);
                        bool change = false;
                        foreach (var node in doc.DocumentElement.ChildNodes)
                        {
                            if (node is XmlElement)
                            {
                                var element = node as XmlElement;
                                Guid id = new Guid(element.GetAttribute("ID"));
                                string title = element.GetAttribute("DisplayName");
                                var field = fieldCollection.FirstOrDefault(t => t.Id == id);
                                if (field != null)
                                {
                                    if (!string.Equals(title, field.Title))
                                    {
                                        element.SetAttribute("DisplayName", field.Title);
                                        change = true;
                                    }
                                }
                            }
                        }
                        if (change)
                        {
                            fieldsProperties["SchemaXml"] = doc.DocumentElement.OuterXml;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Correct field title in xml failed.Error:{0}", e);
                }

                if (supportedResourceCultureNames != null && supportedResourceCultureNames.Count > 0)
                {
                    try
                    {
                        foreach (var languageName in supportedResourceCultureNames)
                        {
                            if (fieldCollection != null)
                            {
                                foreach (Field field in fieldCollection)
                                {
                                    field.TitleResource.GetValueForUICulture(languageName);
                                    field.DescriptionResource.GetValueForUICulture(languageName);
                                }
                            }
                        }
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Error while query fields user resource, web url:{0}, field source:{1}, list id:{2}, error:{3}", webServerRelativeUrl, fieldSource, listId, ex);
                    }
                }
                if (fieldCollection != null)
                {
                    foreach (Field field in fieldCollection)
                    {
                        Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                        AssembleSingleFieldProperties(fieldProperties, field);
                        fieldList.Add(fieldProperties);
                    }
                }
                fieldsProperties.AddChildren(fieldList);
                return fieldsProperties;
            }
        }

        public Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource)
        {
            using (AveClientContext context = CreateContext())
            {
                ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listTitle, listId, contentTypeSource, contentTypeId);
                //context.Load(contentType, c => c.FieldLinks);
                context.ExecuteQuery();
                FieldLinkCollection fieldLinks = contentType.FieldLinks;
                Dictionary<string, object> fieldLinksProp = new Dictionary<string, object>();
                var fieldLinksList = new List<IDictionary<string, object>>();
                foreach (FieldLink fl in fieldLinks)
                {
                    Dictionary<string, object> fieldLinkProp = new Dictionary<string, object>();
                    CopyProperty(fieldLinkProp, fl);
                    fieldLinkProp["DisplayName"] = fl.Name;
                    fieldLinksList.Add(fieldLinkProp);
                }
                fieldLinksProp.AddChildren(fieldLinksList);
                return fieldLinksProp;
            }
        }

        public Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> features = new Dictionary<string, object>();
                var featuresList = new List<IDictionary<string, object>>();
                FeatureCollection featureCollection = null;
                switch (featuresSource)
                {
                    case "site.features":
                        context.Load(context.Site, f => f.Features);
                        context.ExecuteQuery();
                        featureCollection = context.Site.Features;
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(serverRelativeUrl);
                        context.Load(web, f => f.Features);
                        context.ExecuteQuery();
                        featureCollection = web.Features;
                        break;
                    default:
                        break;
                }
                ArgumentCheck.CheckNotNull(featureCollection);
                foreach (Feature f in featureCollection)
                {
                    Dictionary<string, object> featurePropteries = new Dictionary<string, object>();
                    featurePropteries = ObjectToDicValue(f, typeof(Feature));
                    featuresList.Add(featurePropteries);
                }
                features.AddChildren(featuresList);
                return features;
            }
        }

        public Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, Guid listId, string listTitle, string eventReceiverDefSource)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiversInfo = new Dictionary<string, object>();
                var lists = new List<IDictionary<string, object>>();
                EventReceiverDefinitionCollection eventReceivers;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                if (string.Equals(eventReceiverDefSource, "list.eventReceivers"))
                {
                    List list = web.Lists.GetById(listId);
                    eventReceivers = list.EventReceivers;
                }
                else
                {
                    eventReceivers = web.EventReceivers;
                }
                context.Load(eventReceivers);
                context.ExecuteQuery();
                foreach (EventReceiverDefinition eventReceiver in eventReceivers)
                {
                    Dictionary<string, object> eventReceiverInfo = new Dictionary<string, object>();
                    eventReceiverInfo["Assembly"] = eventReceiver.ReceiverAssembly;
                    eventReceiverInfo["Class"] = eventReceiver.ReceiverClass;
                    eventReceiverInfo["Name"] = eventReceiver.ReceiverName;
                    eventReceiverInfo["Id"] = eventReceiver.ReceiverId;
                    eventReceiverInfo["Type"] = eventReceiver.EventType;
                    lists.Add(eventReceiverInfo);
                }
                eventReceiversInfo.AddChildren(lists);
                return eventReceiversInfo;
            }
        }
        public Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                Dictionary<string, object> navigationProperties = new Dictionary<string, object>();
                var navigationList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                NavigationNodeCollection navigationNodeCol = null;
                switch (navigationNodeSource)
                {
                    case "topNavigationBar":
                        navigationNodeCol = web.Navigation.TopNavigationBar;
                        break;
                    case "quickLaunch":
                        navigationNodeCol = web.Navigation.QuickLaunch;
                        break;
                    case "children":
                        NavigationNode navNode = new NavigationNode(context, navProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                        navigationNodeCol = navNode.Children;
                        break;
                    default:
                        break;
                }
                context.Load(navigationNodeCol);
                context.ExecuteQuery();
                ArgumentCheck.CheckNotNull(navigationNodeCol);
                foreach (NavigationNode navigation in navigationNodeCol)
                {
                    Dictionary<string, object> navigationProperty = new Dictionary<string, object>();
                    CopyProperty(navigationProperty, navigation);
                    navigationProperty["Id" + AveObjectModelConstant.ObjectPropertySuffix] = navigation.Path;
                    if (!string.IsNullOrEmpty(navigation.Url))
                    {
                        if (navigation.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                            || navigation.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                            || navigation.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            navigationProperty["IsExternal"] = !navigation.Url.StartsWith(this.WebAppName.TrimEnd('/') + "/" + this.mSiteRelativeUrl.TrimStart('/') + "/", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            navigationProperty["IsExternal"] = false;
                        }
                    }
                    navigationList.Add(navigationProperty);
                }
                navigationProperties.AddChildren(navigationList);
                return navigationProperties;
            }
        }

        public Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope)
        {
            //#if DEBUG
            //            var limitedWebPartManager = GetLimitedWebPartManagerViaCSOM(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
            //            if (tokenProvider.TokenType != TokenType.Bearer)
            //            {
            //                var limitedWebPartManagerByWebService = GetLimitedWebPartManagerViaWebService(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);

            //                Compare("Limited Web Part Manager", limitedWebPartManager, limitedWebPartManagerByWebService);
            //            }
            //            return limitedWebPartManager;

            //#else
            if (tokenProvider.TokenType != TokenType.Bearer)
            {
                return GetLimitedWebPartManagerWithServiceAccount(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
                //GetLimitedWebPartManagerViaWebService(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
            }
            else
            {
                return GetLimitedWebPartManagerViaCSOM(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
            }
            //#endif
        }

        /// <summary>
        /// SAAS-38064
        /// Use CSOM First，then if the webpart's difinitionxml's properties contains the special property(SAAS-31866,special key:InplaceSearchEnabled) 
        /// which need to use web service, it will use web service to get this property value and replace csom result
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="personalizationScope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetLimitedWebPartManagerWithServiceAccount(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope)
        {
            Dictionary<string, object> webpartManagerProperties = new Dictionary<string, object>();
            Dictionary<string, object> webparts = new Dictionary<string, object>();
            webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] = webparts;
            var webpartLists = new List<IDictionary<string, object>>();
            webparts.AddChildren(webpartLists);

            using (AveClientContext context = CreateContext(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl)))
            {
                Web web = context.Web;
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));

                var listItemExceptionScope = new ExceptionHandlingScope(context);
                using (listItemExceptionScope.StartScope())
                {
                    using (listItemExceptionScope.StartTry())
                    {
                        context.Load(file, f => f.ListItemAllFields);
                    }
                    using (listItemExceptionScope.StartCatch())
                    {
                    }
                }

                LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl,
                            wpd => wpd.WebPart.Properties));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl));
                    }
                }
                context.ExecuteQuery();

                //if (listItemExceptionScope.HasException)
                //{
                //    mLogger.Warn("get item for proterties failed,due to {0}", listItemExceptionScope.ErrorMessage);
                //}

                if (exceptionScope.HasException)
                {
                    mLogger.Warn("get webpart proterties failed,due to {0}", exceptionScope.ErrorMessage);
                }

                Dictionary<Guid, ClientResult<string>> webPartSchemaXml = new Dictionary<Guid, ClientResult<string>>();

                Dictionary<string, WebPartDefinition> webPartDefinitionMapping = null;
                ///Storage Key --> WebPart Id
                Dictionary<Guid, string> webPartIdMapping = null;

                object webpartControlIdContent;
                if (file.IsObjectPropertyInstantiated("ListItemAllFields") && (file.ListItemAllFields.FieldValues.TryGetValue("WikiField", out webpartControlIdContent) || file.ListItemAllFields.FieldValues.TryGetValue("PublishingPageContent", out webpartControlIdContent)))
                {
                    if (webpartControlIdContent != null)
                    {
                        webPartDefinitionMapping = new Dictionary<string, WebPartDefinition>(StringComparer.OrdinalIgnoreCase);
                        const string GUIDRegex = @"([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\})";
                        var guids = new RE.Regex(GUIDRegex).Matches(webpartControlIdContent as string);

                        foreach (RE.Match id in guids)
                        {
                            webPartDefinitionMapping[id.Value] = null;
                        }

                        //属于优化部分，可以这么做
                        //if (webPartDefinitionMapping.Count == 1 && limitedWebPartManager.WebParts.Count == 1)
                        //{
                        //    webPartIdMapping = new Dictionary<Guid, string>();
                        //    webPartIdMapping[limitedWebPartManager.WebParts[0].Id] = webPartDefinitionMapping.Keys.First();
                        //    webPartDefinitionMapping = null;
                        //}
                        //else
                        {
                            foreach (var id in webPartDefinitionMapping.Keys.ToArray())
                            {
                                var getByControlIdExceptionScope = new ExceptionHandlingScope(context);
                                using (getByControlIdExceptionScope.StartScope())
                                {
                                    using (getByControlIdExceptionScope.StartTry())
                                    {
                                        var webPart = limitedWebPartManager.WebParts.GetByControlId(string.Concat("g_", id.Replace('-', '_')));
                                        webPartDefinitionMapping[id] = webPart;
                                        context.Load(webPart, w => w.Id);
                                    }
                                    using (getByControlIdExceptionScope.StartCatch())
                                    {

                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var webPart in limitedWebPartManager.WebParts)
                {
                    if (webPart.WebPart.ExportMode != WebPartExportMode.All)
                    {
                        //webPart.WebPart.ExportMode = WebPartExportMode.All;这种方法不好使的原因是两个获取的对象不是一个
                        //通过查看server code，只要export mode不等于None，就可以export，也不需要save，这个目的是为了在运行过程中修改。
                        limitedWebPartManager.WebParts.GetById(webPart.Id).WebPart.ExportMode = WebPartExportMode.All;
                    }

                    var csomDefinition = limitedWebPartManager.ExportWebPart(webPart.Id);
                    //DataFormat:  <nodeFullPath , <propertyNameAndValue, nodeValue>> 
                    //For Example:<'/webParts/webPart/data/properties/property' , <'name=InplaceSearchEnabled','True'>>
                    //Dictionary<string, Dictionary<string, object>> needReplaceNodeValues = new Dictionary<string, Dictionary<string, object>>();
                    //if (NeedToReplaceWebpartXmlByWebService(csomDefinition, out needReplaceNodeValues))
                    //{
                    //    string webServiceDefinition = GetWebPartSchemaXmlWithWebService(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope, webPart.Id);

                    //    ReplaceProperties(csomDefinition,webServiceDefinition, needReplaceNodeValues);
                    //}
                    webPartSchemaXml[webPart.Id] = csomDefinition;
                }

                context.ExecuteQuery();

                if (webPartDefinitionMapping != null && webPartDefinitionMapping.Count > 0)
                {
                    webPartIdMapping = new Dictionary<Guid, string>();
                    foreach (var keyValue in webPartDefinitionMapping)
                    {
                        if (keyValue.Value.IsPropertyAvailable("Id"))
                        {
                            webPartIdMapping[keyValue.Value.Id] = keyValue.Key;
                        }
                    }
                }

                Dictionary<Guid, string> needToReplaceWebpartInfos;
                List<IDictionary<string, object>> webpartPropertiesWithWebService = TryGetWebPartSchemaWithWebServiceIfNeedReplaceProperties(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope, webPartSchemaXml, out needToReplaceWebpartInfos);

                foreach (WebPartDefinition webPart in limitedWebPartManager.WebParts)
                {
                    Dictionary<string, object> webPartDict = new Dictionary<string, object>();
                    CopyProperty(webPartDict, webPart);
                    CopyProperty(webPartDict, webPart.WebPart);

                    webPartDict["ID"] = webPart.Id.ToString("D");
                    webPartDict.Remove("Id");
                    webPartDict.Remove("ZoneId");

                    webPartDict["ZoneID"] = webPart.ZoneId;
                    webPartDict["PartOrder"] = webPart.WebPart.ZoneIndex;

                    if (webpartPropertiesWithWebService != null &&
                        webpartPropertiesWithWebService.Any() &&
                        needToReplaceWebpartInfos != null &&
                        needToReplaceWebpartInfos.ContainsKey(webPart.Id)
                        )
                    {
                        AnalyzeWebPartV1(webPart, webPartSchemaXml[webPart.Id].Value, webPartDict, webPartIdMapping, needToReplaceWebpartInfos[webPart.Id]);
                    }
                    else
                    {
                        AnalyzeWebPart(webPart, webPartSchemaXml[webPart.Id].Value, webPartDict, webPartIdMapping);
                    }

                    webpartLists.Add(webPartDict);
                }
            }
            return webpartManagerProperties;
        }

        private void AnalyzeWebPartV1(WebPartDefinition webPart, string webPartDefinition, Dictionary<string, object> webPartDict, Dictionary<Guid, string> webPartIdMapping, string webPartSchemaWithWebService)
        {
            if (string.IsNullOrEmpty(webPartDefinition))
            {
                return;
            }

            var document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.LoadXml(webPartDefinition);

            var webPartExtractor = WebPartExtractorFactory.Create(document);

            var typeFullName = webPartExtractor.TypeFullName;

            if (!string.IsNullOrEmpty(typeFullName))
            {
                if (typeFullName.IndexOf(',') < 0)
                {
                    webPartDict["RealWebPartType"] = typeFullName;
                }
                else
                {
                    webPartDict["RealWebPartType"] = typeFullName.Substring(0, typeFullName.IndexOf(','));
                }
                //webPartDict["WebPartIdProperty"] = WebPartTypeIdUtility.GenerateId(typeFullName);
            }

            string webPartIdProperty = null;
            if (webPartIdMapping != null && webPartIdMapping.TryGetValue(webPart.Id, out webPartIdProperty))
            {
                webPartDict["WebPartIdProperty"] = webPartIdProperty;
            }

            if (!document.DocumentElement.HasAttribute("ID"))
            {
                document.DocumentElement.SetAttribute("ID", webPart.Id.ToString("D"));
            }

            if (webPartExtractor is V3WebPartPropertyExtractor && webPart.WebPart.IsObjectPropertyInstantiated("Properties"))
            {
                foreach (var keyValue in webPart.WebPart.Properties.FieldValues)
                {
                    if (keyValue.Value != null && (!webPartExtractor.ContainsProperty(keyValue.Key)))
                    {
                        webPartExtractor.AddProperty(true, keyValue.Key, keyValue.Value);
                    }
                }
            }

            var listId = webPartExtractor.GetProperty("ListName");

            if (!AveTypeHelper.IsGuid(listId))
            {
                listId = webPartExtractor.GetProperty("ListId");
                if (AveTypeHelper.IsGuid(listId))
                {
                    webPartDict["ListId"] = new Guid(listId);
                }
            }
            else
            {
                webPartDict["ListId"] = new Guid(listId);
            }

            var isIncluded = webPartExtractor.GetBoolProperty("IsIncluded");

            webPartDict["IsIncluded"] = isIncluded == null ? !webPart.WebPart.IsClosed : isIncluded.Value;

            if (webPartExtractor is V3WebPartPropertyExtractor)
            {
                webPartExtractor.AddProperty(false, "IsIncluded", webPartDict["IsIncluded"].ToString());
            }
            else
            {
                //对于V2格式的
                if (!string.IsNullOrEmpty(webPartIdProperty))
                {
                    webPartExtractor.AddProperty(false, "ID", string.Concat("g_", webPartIdProperty.Replace('-', '_')));
                }
            }


            object webPartIdPropertyObj;
            if (webPartDict.TryGetValue("WebPartIdProperty", out webPartIdPropertyObj))
            {
                webPartExtractor.AddProperty(false, "WebPartIdProperty", webPartIdPropertyObj);
            }

            if (!webPartExtractor.ContainsProperty("ZoneID"))
            {
                webPartExtractor.AddProperty(false, "ZoneID", webPartDict["ZoneID"]);
            }

            if (!webPartExtractor.ContainsProperty("PartOrder"))
            {
                webPartExtractor.AddProperty(false, "PartOrder", webPartDict["PartOrder"]);
            }

            ProcessIfNeedToReplacePropertiesInSchema(document, webPartSchemaWithWebService);
            webPartDict["DefinitionXml"] = document.OuterXml;
        }

        private void ProcessIfNeedToReplacePropertiesInSchema(XmlDocument document, string webPartSchemaWithWebService)
        {
            try
            {
                if (document != null && !string.IsNullOrWhiteSpace(webPartSchemaWithWebService))
                {
                    var webServiceDocument = LoadWithXmlDocument(webPartSchemaWithWebService);
                    var webPartExtractorSource = WebPartExtractorFactory.Create(webServiceDocument);
                    var webPartExtractorTarget = WebPartExtractorFactory.Create(document);

                    CompareAndReplace(webPartExtractorTarget, webPartExtractorSource, "InplaceSearchEnabled", true);
                    mLogger.Info($"Process IfNeedToReplacePropertiesInSchema success, replace result:{document.OuterXml}");
                }
                else
                {
                    throw new ArgumentNullException($"csom document object:{document==null} or web service schemaxml is null");
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occured when ProcessIfNeedToReplacePropertiesInSchema, error:{e.Message}, StackTrace:{e.StackTrace}. WebService Schema: {webPartSchemaWithWebService},");
            }
        }

        /// <summary>
        /// Set source property value to the target
        /// </summary>
        /// <param name="webPartExtractorTarget"></param>
        /// <param name="webPartExtractorSource"></param>
        /// <param name="propertyName"></param>
        /// <param name="isPropertiesNode"></param>
        private static void CompareAndReplace(IWebPartPropertyExtractor webPartExtractorTarget, IWebPartPropertyExtractor webPartExtractorSource, string propertyName, bool isPropertiesNode = true)
        {
            if (webPartExtractorTarget == null || webPartExtractorSource == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }
            var inplaceSearchEnabledSource = webPartExtractorSource.GetProperty(propertyName);
            var inplaceSearchEnabledTarget = webPartExtractorTarget.GetProperty(propertyName);
            if (!string.IsNullOrWhiteSpace(inplaceSearchEnabledSource)
                && !string.IsNullOrWhiteSpace(inplaceSearchEnabledTarget)
                && !string.Equals(inplaceSearchEnabledSource, inplaceSearchEnabledTarget, StringComparison.OrdinalIgnoreCase))
            {
                webPartExtractorTarget.RemoveAndAddNewProperty(isPropertiesNode, propertyName, inplaceSearchEnabledSource);
                mLogger.Info($"[SAAS-38064]Replace webpart schemaxml property:{propertyName} success and will use webService, csom result:{inplaceSearchEnabledTarget}, webService result:{inplaceSearchEnabledSource}");
            }
        }

        /// <summary>
        /// If csom web part results contains the special property key, we will use webservice to replace this key value
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="personalizationScope"></param>
        /// <param name="webPartSchemaXml"></param>
        /// <returns></returns>
        private List<IDictionary<string, object>> TryGetWebPartSchemaWithWebServiceIfNeedReplaceProperties(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, Dictionary<Guid, ClientResult<string>> webPartSchemaXml, out Dictionary<Guid, string> needToReplaceWebpartInfos)
        {
            List<IDictionary<string, object>> webpartPropertiesWithWebService = null;
            List<Guid> needToReplaceWebpartIds = new List<Guid>();
            needToReplaceWebpartInfos = null;
            bool needExecuteWebService = false;
            try
            {
                AddToWebpartIdsIfContainsKey(needToReplaceWebpartIds, webPartSchemaXml, "InplaceSearchEnabled", ref needExecuteWebService);

                if (needExecuteWebService && needToReplaceWebpartIds.Any())
                {
                    mLogger.Info($"This file:{fileServerRelativeUrl}-{personalizationScope} webparts contain special property which need to use web service to replace property node value.");
                    var webpartManagerPropertiesWithWebService = mWebServiceRequest.GetLimitedWebPartManager(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
                    var webpartObjectValue = webpartManagerPropertiesWithWebService["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    webpartPropertiesWithWebService = webpartObjectValue.GetChildren();
                    if (needToReplaceWebpartIds != null && needToReplaceWebpartIds.Any())
                    {
                        needToReplaceWebpartInfos = new Dictionary<Guid, string>();
                        foreach (var webPartId in needToReplaceWebpartIds)
                        {
                            var schemaXml = GetSchemaXmlByWebpartId(webpartPropertiesWithWebService, webPartId);
                            needToReplaceWebpartInfos.Add(webPartId, schemaXml);
                            mLogger.Info($"This file:{fileServerRelativeUrl}-{personalizationScope}'s webpart:{webPartId.ToString()} contains special property. SchemaXml:{schemaXml}");
                        }
                    }
                }
                else
                {
                    mLogger.Info($"Don't need to use web service to access this file:{fileServerRelativeUrl}-{personalizationScope} webparts.");
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when GetWebPartPropertiesWithWebServiceIfNeed, error:{0}", e);
            }
            return webpartPropertiesWithWebService;
        }

        /// <summary>
        /// Add web part id which contains special property
        /// </summary>
        /// <param name="needToReplaceWebpartIds"></param>
        /// <param name="webPartSchemaXml"></param>
        /// <param name="propertyName">Input which you want to add replace webpart collection</param>
        /// <param name="needExecuteWebService"></param>
        private static void AddToWebpartIdsIfContainsKey(List<Guid> needToReplaceWebpartIds, Dictionary<Guid, ClientResult<string>> webPartSchemaXml, string propertyName, ref bool needExecuteWebService)
        {
            if (webPartSchemaXml.Any(it => it.Value.Value.Contains(propertyName)))
            {
                foreach (var wp in webPartSchemaXml)
                {
                    string definition = wp.Value.Value;
                    XmlDocument webServiceDocument = LoadWithXmlDocument(definition);
                    if (webServiceDocument != default(XmlDocument))
                    {
                        var webPartExtractorSource = WebPartExtractorFactory.Create(webServiceDocument);
                        var isExists = !string.IsNullOrEmpty(webPartExtractorSource.GetProperty(propertyName));
                        if (isExists)
                        {
                            needExecuteWebService = true;
                            needToReplaceWebpartIds.Add(wp.Key);
                        }
                    }
                }
            }
        }

        private static XmlDocument LoadWithXmlDocument(string definition)
        {
            try
            {
                var document = new XmlDocument();
                document.PreserveWhitespace = true;
                document.LoadXml(definition);
                return document;
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when LoadWithXmlDocument, error:{0}", e);
            }
            return default(XmlDocument);
        }

        private static string GetSchemaXmlByWebpartId(List<IDictionary<string, object>> webpartProperties, Guid inputWebPartId, string schemaXmlPropertyName= "DefinitionXml")
        {
            try
            {
                if (webpartProperties == null || !webpartProperties.Any() || inputWebPartId == Guid.Empty)
                {
                    return "";
                }

                foreach (var webpartProperty in webpartProperties)
                {
                    var webpartIdWithWebService = webpartProperty.ContainsKey("ID") ? new Guid(webpartProperty["ID"].ToString()) : Guid.Empty;
                    if (Guid.Equals(inputWebPartId, webpartIdWithWebService))
                    {
                        return webpartProperty[schemaXmlPropertyName] as string;
                    }
                }
            }
            catch (Exception e)
            {
                string wpId = inputWebPartId == default(Guid) ? "" : inputWebPartId.ToString();
                mLogger.Error($"An error occured when GetSchemaXmlByWebpartId:{wpId}, error:{e.Message}, stackTrace:{e.StackTrace}");
            }
            return string.Empty;
        }

        /// <summary>
        /// WebPartId2(WebPartIdProperty)这个属性，普通情况下，无法获取到，目前能获取的唯一方式是wiki field valu和PublishingPageContent(ArticlePage) field value，其他的普通page无法获取，暂时不支持。
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="personalizationScope"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetLimitedWebPartManagerViaCSOM(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope)
        {
            Dictionary<string, object> webpartManagerProperties = new Dictionary<string, object>();
            Dictionary<string, object> webparts = new Dictionary<string, object>();
            webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] = webparts;
            var webpartLists = new List<IDictionary<string, object>>();
            webparts.AddChildren(webpartLists);

            using (AveClientContext context = CreateContext(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl)))
            {
                Web web = context.Web;
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));

                var listItemExceptionScope = new ExceptionHandlingScope(context);
                using (listItemExceptionScope.StartScope())
                {
                    using (listItemExceptionScope.StartTry())
                    {
                        context.Load(file, f => f.ListItemAllFields);
                    }
                    using (listItemExceptionScope.StartCatch())
                    {
                    }
                }

                LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl,
                            wpd => wpd.WebPart.Properties));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl));
                    }
                }
                context.ExecuteQuery();

                //if (listItemExceptionScope.HasException)
                //{
                //    mLogger.Warn("get item for proterties failed,due to {0}", listItemExceptionScope.ErrorMessage);
                //}

                if (exceptionScope.HasException)
                {
                    mLogger.Warn("get webpart proterties failed,due to {0}", exceptionScope.ErrorMessage);
                }

                Dictionary<Guid, ClientResult<string>> webPartSchemaXml = new Dictionary<Guid, ClientResult<string>>();

                Dictionary<string, WebPartDefinition> webPartDefinitionMapping = null;
                ///Storage Key --> WebPart Id
                Dictionary<Guid, string> webPartIdMapping = null;

                object webpartControlIdContent;
                if (file.IsObjectPropertyInstantiated("ListItemAllFields") && (file.ListItemAllFields.FieldValues.TryGetValue("WikiField", out webpartControlIdContent) || file.ListItemAllFields.FieldValues.TryGetValue("PublishingPageContent", out webpartControlIdContent)))
                {
                    if (webpartControlIdContent != null)
                    {
                        webPartDefinitionMapping = new Dictionary<string, WebPartDefinition>(StringComparer.OrdinalIgnoreCase);
                        const string GUIDRegex = @"([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\})";
                        var guids = new RE.Regex(GUIDRegex).Matches(webpartControlIdContent as string);

                        foreach (RE.Match id in guids)
                        {
                            webPartDefinitionMapping[id.Value] = null;
                        }

                        //属于优化部分，可以这么做
                        //if (webPartDefinitionMapping.Count == 1 && limitedWebPartManager.WebParts.Count == 1)
                        //{
                        //    webPartIdMapping = new Dictionary<Guid, string>();
                        //    webPartIdMapping[limitedWebPartManager.WebParts[0].Id] = webPartDefinitionMapping.Keys.First();
                        //    webPartDefinitionMapping = null;
                        //}
                        //else
                        {
                            foreach (var id in webPartDefinitionMapping.Keys.ToArray())
                            {
                                var getByControlIdExceptionScope = new ExceptionHandlingScope(context);
                                using (getByControlIdExceptionScope.StartScope())
                                {
                                    using (getByControlIdExceptionScope.StartTry())
                                    {
                                        var webPart = limitedWebPartManager.WebParts.GetByControlId(string.Concat("g_", id.Replace('-', '_')));
                                        webPartDefinitionMapping[id] = webPart;
                                        context.Load(webPart, w => w.Id);
                                    }
                                    using (getByControlIdExceptionScope.StartCatch())
                                    {

                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var webPart in limitedWebPartManager.WebParts)
                {
                    if (webPart.WebPart.ExportMode != WebPartExportMode.All)
                    {
                        //webPart.WebPart.ExportMode = WebPartExportMode.All;这种方法不好使的原因是两个获取的对象不是一个
                        //通过查看server code，只要export mode不等于None，就可以export，也不需要save，这个目的是为了在运行过程中修改。
                        limitedWebPartManager.WebParts.GetById(webPart.Id).WebPart.ExportMode = WebPartExportMode.All;
                    }

                    var definition = limitedWebPartManager.ExportWebPart(webPart.Id);

                    webPartSchemaXml[webPart.Id] = definition;
                }

                context.ExecuteQuery();


                if (webPartDefinitionMapping != null && webPartDefinitionMapping.Count > 0)
                {
                    webPartIdMapping = new Dictionary<Guid, string>();
                    foreach (var keyValue in webPartDefinitionMapping)
                    {
                        if (keyValue.Value.IsPropertyAvailable("Id"))
                        {
                            webPartIdMapping[keyValue.Value.Id] = keyValue.Key;
                        }
                    }
                }

                foreach (WebPartDefinition webPart in limitedWebPartManager.WebParts)
                {
                    Dictionary<string, object> webPartDict = new Dictionary<string, object>();
                    CopyProperty(webPartDict, webPart);
                    CopyProperty(webPartDict, webPart.WebPart);

                    webPartDict["ID"] = webPart.Id.ToString("D");
                    webPartDict.Remove("Id");
                    webPartDict.Remove("ZoneId");

                    webPartDict["ZoneID"] = webPart.ZoneId;
                    webPartDict["PartOrder"] = webPart.WebPart.ZoneIndex;

                    AnalyzeWebPart(webPart, webPartSchemaXml[webPart.Id].Value, webPartDict, webPartIdMapping);

                    webpartLists.Add(webPartDict);
                }
            }
            return webpartManagerProperties;
        }

        private void AnalyzeWebPart(WebPartDefinition webPart, string webPartDefinition, Dictionary<string, object> webPartDict, Dictionary<Guid, string> webPartIdMapping)
        {
            if (string.IsNullOrEmpty(webPartDefinition))
            {
                return;
            }

            var document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.LoadXml(webPartDefinition);

            var webPartExtractor = WebPartExtractorFactory.Create(document);

            var typeFullName = webPartExtractor.TypeFullName;

            if (!string.IsNullOrEmpty(typeFullName))
            {
                if (typeFullName.IndexOf(',') < 0)
                {
                    webPartDict["RealWebPartType"] = typeFullName;
                }
                else
                {
                    webPartDict["RealWebPartType"] = typeFullName.Substring(0, typeFullName.IndexOf(','));
                }
                //webPartDict["WebPartIdProperty"] = WebPartTypeIdUtility.GenerateId(typeFullName);
            }

            string webPartIdProperty = null;
            if (webPartIdMapping != null && webPartIdMapping.TryGetValue(webPart.Id, out webPartIdProperty))
            {
                webPartDict["WebPartIdProperty"] = webPartIdProperty;
            }

            if (!document.DocumentElement.HasAttribute("ID"))
            {
                document.DocumentElement.SetAttribute("ID", webPart.Id.ToString("D"));
            }

            if (webPartExtractor is V3WebPartPropertyExtractor && webPart.WebPart.IsObjectPropertyInstantiated("Properties"))
            {
                foreach (var keyValue in webPart.WebPart.Properties.FieldValues)
                {
                    if (keyValue.Value != null && (!webPartExtractor.ContainsProperty(keyValue.Key)))
                    {
                        webPartExtractor.AddProperty(true, keyValue.Key, keyValue.Value);
                    }
                }
            }

            var listId = webPartExtractor.GetProperty("ListName");

            if (!AveTypeHelper.IsGuid(listId))
            {
                listId = webPartExtractor.GetProperty("ListId");
                if (AveTypeHelper.IsGuid(listId))
                {
                    webPartDict["ListId"] = new Guid(listId);
                }
            }
            else
            {
                webPartDict["ListId"] = new Guid(listId);
            }

            var isIncluded = webPartExtractor.GetBoolProperty("IsIncluded");

            webPartDict["IsIncluded"] = isIncluded == null ? !webPart.WebPart.IsClosed : isIncluded.Value;

            if (webPartExtractor is V3WebPartPropertyExtractor)
            {
                webPartExtractor.AddProperty(false, "IsIncluded", webPartDict["IsIncluded"].ToString());
            }
            else
            {
                //对于V2格式的
                if (!string.IsNullOrEmpty(webPartIdProperty))
                {
                    webPartExtractor.AddProperty(false, "ID", string.Concat("g_", webPartIdProperty.Replace('-', '_')));
                }
            }


            object webPartIdPropertyObj;
            if (webPartDict.TryGetValue("WebPartIdProperty", out webPartIdPropertyObj))
            {
                webPartExtractor.AddProperty(false, "WebPartIdProperty", webPartIdPropertyObj);
            }

            if (!webPartExtractor.ContainsProperty("ZoneID"))
            {
                webPartExtractor.AddProperty(false, "ZoneID", webPartDict["ZoneID"]);
            }

            if (!webPartExtractor.ContainsProperty("PartOrder"))
            {
                webPartExtractor.AddProperty(false, "PartOrder", webPartDict["PartOrder"]);
            }

            webPartDict["DefinitionXml"] = document.OuterXml;

        }

        //[Obsolete]
        public Dictionary<string, object> GetLimitedWebPartManagerViaWebService(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope)
        {
            //Dictionary<Guid, object> webPartZoneAndPartOrders = new Dictionary<Guid, object>();
            Dictionary<string, object> webpartManagerProperties = mWebServiceRequest.GetLimitedWebPartManager(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope);
            if (webpartManagerProperties.Count == 0)
            {
                return webpartManagerProperties;
            }
            Dictionary<Guid, object> webPartProperties = new Dictionary<Guid, object>();
            int defaultPartOrder = 0;
            using (AveClientContext context = CreateContext(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl)))
            {
                Web web = context.Web;
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(wpd => wpd.WebPart.ZoneIndex, wpd => wpd.ZoneId, wpd => wpd.Id, wpd => wpd.WebPart.Properties));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(wpd => wpd.WebPart.ZoneIndex, wpd => wpd.ZoneId, wpd => wpd.Id));
                    }
                }
                context.ExecuteQuery();
                if (exceptionScope.HasException)
                {
                    mLogger.Warn("get webpart proterties failed,due to {0}", exceptionScope.ErrorMessage);
                }
                foreach (WebPartDefinition webPart in limitedWebPartManager.WebParts)
                {
                    Dictionary<string, object> needReplacePros = new Dictionary<string, object>();
                    needReplacePros["ZoneID"] = webPart.ZoneId;
                    needReplacePros["PartOrder"] = webPart.WebPart.ZoneIndex;
                    if (webPart.WebPart.IsObjectPropertyInstantiated("Properties"))
                    {
                        needReplacePros["Properties"] = webPart.WebPart.Properties.FieldValues;
                    }
                    defaultPartOrder = webPart.WebPart.ZoneIndex > defaultPartOrder ? webPart.WebPart.ZoneIndex : defaultPartOrder;
                    webPartProperties[webPart.Id] = needReplacePros;
                }
            }
            if (webpartManagerProperties.ContainsKey("WebParts" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                Dictionary<string, object> webPartColProperties = webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                var webpartPropertiesList = webPartColProperties.GetChildren();
                AppendWebPartProperties(webpartPropertiesList, webPartProperties, "wpz", defaultPartOrder);
            }
            return webpartManagerProperties;
        }

        /// <summary>
        /// Old:add zoneid and partorder to cache. New: add zoneid, partorder and properties
        /// </summary>
        /// <param name="webpartPropertiesList"></param>
        /// <param name="needReplaceProperties"></param>
        private void AppendWebPartProperties(List<IDictionary<string, object>> webpartPropertiesList, Dictionary<Guid, object> needReplaceProperties, string defaultZoneID, int defaultPartOrder)
        {
            IDictionary<string, object> webpartProperties = null;
            for (int i = webpartPropertiesList.Count - 1; i >= 0; i--)
            {
                webpartProperties = webpartPropertiesList[i];
                if (!webpartProperties.ContainsKey("ID") || !webpartProperties.ContainsKey("DefinitionXml"))
                {
                    mLogger.Warn("get webpart id and definitionxml failed.");
                    continue;
                }
                Guid webpartId = new Guid(webpartProperties["ID"].ToString());
                Dictionary<string, object> needReplaceProperty;
                if (needReplaceProperties.ContainsKey(webpartId))
                {
                    needReplaceProperty = needReplaceProperties[webpartId] as Dictionary<string, object>;
                }
                else
                {
                    webpartPropertiesList.RemoveAt(i);
                    continue;
                    //needReplaceProperty = new Dictionary<string, object>();
                    //needReplaceProperty["ZoneID"] = defaultZoneID;
                    //needReplaceProperty["PartOrder"] = ++defaultPartOrder;
                    //mLogger.Warn("get webpart zoneid and partorder failed.webpart definitionxml:{0}", webpartProperties["DefinitionXml"].ToString());
                }
                webpartProperties["ZoneID"] = needReplaceProperty["ZoneID"];
                webpartProperties["PartOrder"] = needReplaceProperty["PartOrder"];
                webpartProperties["ZoneIndex"] = needReplaceProperty["PartOrder"];
                XDocument xdocDefinition = XDocument.Parse(webpartProperties["DefinitionXml"].ToString());
                XNamespace ns = V3WebPartPropertyExtractor.WebPartV3NameSpace; //http://schemas.microsoft.com/WebPart/v3;
                XElement webpart = xdocDefinition.Root;
                foreach (KeyValuePair<string, object> properties in needReplaceProperty)
                {
                    if (properties.Key.Equals("Properties", StringComparison.OrdinalIgnoreCase))
                    {
                        Dictionary<string, object> propertiesDic = properties.Value as Dictionary<string, object>;
                        XElement xeleParent = webpart.Descendants(ns + "properties").FirstOrDefault();
                        foreach (KeyValuePair<string, object> propertyPair in propertiesDic)
                        {
                            if (xeleParent == null || propertyPair.Value == null ||
                                xeleParent.Elements().Where(e => e.Attribute("name") != null && e.Attribute("name").Value.Equals(propertyPair.Key, StringComparison.OrdinalIgnoreCase)).Count() > 0)
                            {
                                continue;
                            }
                            else
                            {
                                xeleParent.Add(new XElement(ns + "property", new XAttribute("name", propertyPair.Key), new XAttribute("type", propertyPair.Value.GetType().Name), propertyPair.Value));
                            }
                        }
                    }
                    else
                    {
                        XElement xele = webpart.Descendants().Where(e => e.Attribute("name") != null && e.Attribute("name").Value.Equals(properties.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (xele == null)
                        {
                            xele = new XElement(properties.Key);
                            webpart.Add(xele);
                        }
                        xele.Value = properties.Value.ToString();
                    }
                }
                webpartProperties["DefinitionXml"] = xdocDefinition.ToString();
                //XmlDocument definitionXml = new XmlDocument();
                //definitionXml.LoadXml(webpartProperties["DefinitionXml"].ToString());
                //XmlNode webpart = definitionXml.DocumentElement;
                //foreach (KeyValuePair<string, object> properties in needReplaceProperty)
                //{
                //    XmlNode node = webpart.SelectSingleNode(".//*[name()='" + properties.Key + "']");
                //    if (node == null)
                //    {
                //        XmlElement tempElement = definitionXml.CreateElement(properties.Key);
                //        node = webpart.AppendChild(tempElement);
                //    }
                //    node.InnerText = properties.Value.ToString();
                //}
                //webpartProperties["DefinitionXml"] = definitionXml.OuterXml;
            }
        }

        public AveBasePermissions GetUserEffectivePermissions(string level, string Url, Guid id, string userName, int itemId = 0)
        {
            using (AveClientContext context = CreateContext())
            {
                AveBasePermissions basePermissions = AveBasePermissions.EmptyMask;
                ClientResult<BasePermissions> basePerms = null;
                Web web = context.Site.OpenWeb(Url);
                switch (level)
                {
                    case "Site":
                    case "Web":
                        basePerms = web.GetUserEffectivePermissions(userName);
                        break;
                    case "List":
                        List list = web.Lists.GetById(id);
                        basePerms = list.GetUserEffectivePermissions(userName);
                        break;
                    case "Item":
                        List lists = web.Lists.GetById(id);
                        ListItem item = lists.GetItemById(itemId);
                        basePerms = item.GetUserEffectivePermissions(userName);
                        break;
                }
                context.ExecuteQuery();

                foreach (int perm in Enum.GetValues(typeof(PermissionKind)))
                {
                    if (basePerms != null && basePerms.Value.Has((PermissionKind)Enum.ToObject(typeof(PermissionKind), perm)))
                    {
                        string permissionStr = Enum.GetName(typeof(PermissionKind), perm);
                        basePermissions = (AveBasePermissions)Enum.Parse(typeof(AveBasePermissions), permissionStr) | basePermissions;
                    }
                }

                return basePermissions;
            }
        }

        public Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId, Guid uniqueId)
        {
            try
            {
                return GetFileVersionStreamByRestApi(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl), fileServerRelativeUrl, versionId, uniqueId, WrapperConfiguration.OpenBinaryOptions);
            }
            catch (Exception e)
            {
                try
                {
                    if (mWebServiceRequest.IsAvaliable)
                    {
                        return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
                    }
                }
                /*review-qlluo*/
                catch (Exception e1)
                {
                    mLogger.Warn("get file version stream by WebService failed. error message:{0}", e1.ToString());
                }

                return GetFileVersionStreamByCsomApi(webServerRelativeUrl, fileServerRelativeUrl, versionId);
            }
        }

        private bool IsOneNoteFile(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) && fileName.EndsWith(".one", StringComparison.OrdinalIgnoreCase);
        }

        private Stream GetFileVersionStreamByCsomApi(string webServerRelativeUrl, string fileServerRelativeUrl, int versionId)
        {
            bool needDisposeContext = true;
            AveClientContext context = CreateContext();
            try
            {
                ClientResult<Stream> content = null;
                AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x8007047E"), new KeyValuePair<string, string>("ServerException", "Attempt to reuse a disposed CobaltStream"));
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                    FileVersion version = file.Versions.GetById(versionId);
                    content = version.OpenBinaryStreamWithOptions(SPOpenBinaryOptions.Unprotected);
                    context.ExecuteQuery();
                });

                if(content?.Value.Length > AveSPDataStreamReader.USE_SPDATA_STREAM_READER_LIMIT)
                {
                    needDisposeContext = false;
                    return new AveSPDataStreamReader(content.Value, content.Value.Length, context);
                }
                else
                {
                    //binary copy is required, cause ClientResult<Stream> can't be used after context is disposed
                    //MemoryStream binary = new MemoryStream((int)content.Value.Length);
                    Stream binary = new AveCoordinatedStream("CApiFVS");
                    AveIOHelper.Copy(content.Value, binary);
                    binary.Position = 0;
                    return binary;
                }
            }
            finally
            {
                if (needDisposeContext)
                {
                    context?.Dispose();
                }
            }
        }

        public Dictionary<string, object> GetUserProfileManager()
        {
            return new Dictionary<string, object>();
        }

        public Dictionary<string, object> GetAudienceManager()
        {
            throw new NotImplementedException();
        }

        public Guid GetListId(Guid webId, string listTitle)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    List list = web.Lists.GetByTitle(listTitle);
                    context.Load(list, l => l.Id);
                    context.ExecuteQuery();
                    return list.Id;
                }
            }
            catch (Exception ex)//when lookup list havn't been restored, there is going to be an exception
            {
                mLogger.Warn("Can't Get list:{0} Id.Error Message:{1}", listTitle, ex.Message);
                return Guid.Empty;
            }
        }

        public IList<Dictionary<string, object>> GetManagedThemes()
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> themes = new Dictionary<string, object>();
                List<Dictionary<string, object>> themeList = new List<Dictionary<string, object>>();
                Web web = context.Site.RootWeb;
                List list = web.GetCatalog((int)ListTemplateType.ThemeCatalog);
                FileCollection files = list.RootFolder.Files;
                context.Load(files);
                context.ExecuteQuery();
                foreach (Microsoft.SharePoint.Client.File file in files)
                {
                    Dictionary<string, object> fileProp = new Dictionary<string, object>();
                    CopyProperty(fileProp, file);
                    themeList.Add(fileProp);
                }
                return themeList;
            }
        }

        public Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public string GetApplicationPath(string serverRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Dictionary<string, object>> ResolvePrincipals(string webServerRelativeUrl, List<string> searchNames, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff = true)
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> resolvedPrincipals = new Dictionary<string, Dictionary<string, object>>();
                using (AveClientContext context = CreateContext())
                {
                    Dictionary<string, ClientResult<string>> userInfoResults = new Dictionary<string, ClientResult<string>>();
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    for (int i = 0; i < searchNames.Count; i++)
                    {
                        string searchName = searchNames[i];
                        userInfoResults.Add(searchName, SearchUsers(context, searchName, scopes, sources));
                        //数字不易过大，一次处理的太多会导致SharePoint抛timeout异常
                        if (i >= 20 && i % 20 == 0)
                        {
                            context.ExecuteQuery();
                        }
                    }
                    if (context.HasPendingRequest)
                    {
                        context.ExecuteQuery();
                    }
                    foreach (string searchName in searchNames)
                    {
                        Dictionary<string, object> resolvedPrincipal = AssemblePrincipalInfo(searchName, userInfoResults[searchName].Value);
                        resolvedPrincipals.Add(searchName, resolvedPrincipal);
                    }
                    return resolvedPrincipals;
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to resolve principals, error defail : {0}", e.ToString());
                return null;
            }
        }

        private Dictionary<string, object> AssemblePrincipalInfo(string searchName, string userInfos)
        {
            if (!string.IsNullOrEmpty(userInfos))
            {
                if (searchName.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase) || searchName.Equals("authenticated users", StringComparison.OrdinalIgnoreCase))
                {
                    return ResolveAuthenticatedUsersFromJson(userInfos);
                }
                //foreach (Dictionary<string, object> userInfo in ResolveUsersFromJson(userInfos))
                //{
                //    return userInfo;
                //}
                return ResolveUsersFromJson(userInfos)?.FirstOrDefault();
            }
            return null;
        }

        public Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string searchName, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff = true)
        {
            using (AveClientContext context = CreateContext())
            {
                ClientResult<string> UserInfos = ResolveUser(context, searchName, scopes, sources); //SearchUsers(context, searchName, scopes, sources);
                context.ExecuteQuery();
                string result = "[" + UserInfos.Value + "]";
                return AssemblePrincipalInfo(searchName, result);
            }
        }

        public Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount)
        {
            List<string> loginNames = new List<string>();
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> principalInfos = new Dictionary<string, object>();
                List<Dictionary<string, object>> infoList = new List<Dictionary<string, object>>();
                ClientResult<string> userInfos = SearchUsers(context, input, scopes, sources, maxCount);
                context.ExecuteQuery();
                if (!string.IsNullOrEmpty(userInfos.Value))
                {
                    if (input.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase) || input.Equals("authenticated users", StringComparison.OrdinalIgnoreCase))
                    {
                        Dictionary<string, object> userInfo = ResolveAuthenticatedUsersFromJson(userInfos.Value);
                        infoList.Add(userInfo);
                        if (userInfo.ContainsKey("LoginName"))
                        {
                            loginNames.Add(userInfo["LoginName"].ToString());
                        }
                    }
                    foreach (Dictionary<string, object> userInfo in ResolveUsersFromJson(userInfos.Value))
                    {
                        infoList.Add(userInfo);
                        if (userInfo.ContainsKey("LoginName"))
                        {
                            loginNames.Add(userInfo["LoginName"].ToString());
                        }
                    }
                }
                principalInfos.Add("Principals", infoList);
                return principalInfos;
            }
        }

        private Dictionary<string, object> AssembleUserProperties(Dictionary<string, object> originalData)
        {
            if (!originalData.ContainsKey("ProviderName"))//not exist user and group
            {
                return null;
            }
            if (originalData.ContainsKey("EntityData"))
            {
                if (originalData["EntityData"] is Dictionary<string, object> && (originalData["EntityData"] as Dictionary<string, object>).ContainsKey("PrincipalType"))
                {
                    if ((originalData["EntityData"] as Dictionary<string, object>)["PrincipalType"].ToString().Contains("UNVALIDATED"))
                    {
                        return null;
                    }
                }
                else if (originalData.ContainsKey("IsResolved") && !Convert.ToBoolean(originalData["IsResolved"]))
                {
                    return null;
                }
            }//not exist email address
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            if (originalData.ContainsKey("Key"))
            {
                infoDic["LoginName"] = originalData["Key"].ToString();
            }
            infoDic["DisplayName"] = originalData.ContainsKey("DisplayText") ? originalData["DisplayText"] : string.Empty;
            if (originalData.ContainsKey("EntityType"))
            {
                if (originalData["EntityType"].ToString().Equals("User"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.User);
                }
                else if (originalData["EntityType"].ToString().Equals("FormsRole"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                }
                else if (originalData["EntityType"].ToString().Equals("SecGroup"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                }
                infoDic["PrincipalId"] = int.MinValue;
            }
            if (originalData.ContainsKey("EntityData"))
            {
                Dictionary<string, object> ed = new Dictionary<string, object>();
                if (originalData["EntityData"] is Dictionary<string, object>)
                {
                    ed = originalData["EntityData"] as Dictionary<string, object>;
                }
                else if (originalData["EntityData"] is JObject)
                {
                    ed = JsonConvert.DeserializeObject<Dictionary<string, object>>(originalData["EntityData"].ToString());
                }
                if (ed.ContainsKey("PrincipalType") && !infoDic.ContainsKey("PrincipalType"))
                {
                    switch (ed["PrincipalType"].ToString())
                    {
                        case "SecurityGroup":
                            infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                            break;
                        case "SharePointGroup":
                            infoDic.Add("PrincipalType", AvePrincipalType.SharePointGroup);
                            break;
                        case "DistributionList":
                            infoDic.Add("PrincipalType", AvePrincipalType.DistributionList);
                            break;
                        case "User":
                            infoDic.Add("PrincipalType", AvePrincipalType.User);
                            break;
                        default:
                            infoDic.Add("PrincipalType", AvePrincipalType.None);
                            break;
                    }
                }
                if (ed.ContainsKey("Title"))
                {
                    infoDic.Add("JobTitle", ed["Title"]);
                }
                if (ed.ContainsKey("MobilePhone"))
                {
                    infoDic.Add("Mobile", ed["MobilePhone"]);
                }
                if (ed.ContainsKey("Email"))
                {
                    infoDic["Email"] = ed["Email"];
                }
                if (ed.ContainsKey("Department"))
                {
                    infoDic.Add("Department", ed["Department"]);
                }
            }
            return infoDic;
        }
        private Dictionary<string, object> ResolveAuthenticatedUsersFromJson(string jsonData)
        {
            jsonData = jsonData.TrimStart('[').TrimEnd(']');
            Dictionary<string, object> userProp = new Dictionary<string, object>();
            Dictionary<string, object> jsonObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            if (jsonObj != null)
            {
                userProp = AssembleUserProperties(jsonObj);
            }
            return userProp;
        }

        private List<Dictionary<string, object>> ResolveUsersFromJson(string jsonData)
        {
            List<Dictionary<string, object>> infoList = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> jsonObj = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonData);
            if (jsonObj != null)
            {
                foreach (Dictionary<string, object> obj in jsonObj)
                {
                    Dictionary<string, object> userProp = new Dictionary<string, object>();
                    if (obj.ContainsKey("MultipleMatches") && (obj["MultipleMatches"] as ArrayList) != null && (obj["MultipleMatches"] as ArrayList).Count > 0)
                    {
                        Array multipleObj = (obj["MultipleMatches"] as ArrayList).ToArray(typeof(Dictionary<string, object>));
                        foreach (Dictionary<string, object> dir in multipleObj)
                        {
                            userProp = AssembleUserProperties(dir);
                            if (userProp != null)
                            {
                                infoList.Add(userProp);
                            }
                        }
                        return infoList;
                    }
                    userProp = AssembleUserProperties(obj);
                    if (userProp != null)
                    {
                        infoList.Add(userProp);
                    }
                }
            }
            return infoList;
        }

        private ClientResult<string> SearchUsers(ClientContext context, string input, int scopes, int sources)
        {
            return SearchUsers(context, input, scopes, sources, 30);
        }

        private ClientResult<string> SearchUsers(ClientContext context, string input, int scopes, int sources, int maxCount)
        {
            ClientPeoplePickerQueryParameters searchParams = new ClientPeoplePickerQueryParameters()
            {
                AllowEmailAddresses = true,
                AllowMultipleEntities = true,
                QueryString = input,
                Required = true,
                PrincipalType = (PrincipalType)scopes,
                PrincipalSource = (PrincipalSource)sources,
                MaximumEntitySuggestions = maxCount
            };
            if (input.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase) || input.Equals("authenticated users", StringComparison.OrdinalIgnoreCase))
            {
                return ClientPeoplePickerWebServiceInterface.ClientPeoplePickerResolveUser(context, searchParams);
            }
            return ClientPeoplePickerWebServiceInterface.ClientPeoplePickerSearchUser(context, searchParams);
        }

        private ClientResult<string> ResolveUser(ClientContext context, string input, int scopes, int sources)
        {
            ClientPeoplePickerQueryParameters searchParams = new ClientPeoplePickerQueryParameters()
            {
                AllowEmailAddresses = true,
                AllowMultipleEntities = true,
                QueryString = input,
                Required = true,
                PrincipalType = (PrincipalType)scopes,
                PrincipalSource = (PrincipalSource)sources,
                MaximumEntitySuggestions = 30
            };
            return ClientPeoplePickerWebServiceInterface.ClientPeoplePickerResolveUser(context, searchParams);
        }

        public Dictionary<string, object> GetListsProperties(string webServerRelativeUrl, Dictionary<string, object> listsProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web.Lists, webLists => webLists.Include(l => l.Title, l => l.RootFolder, l => l.AllowContentTypes, l => l.ContentTypesEnabled));
                context.ExecuteQuery();
                var lists = listsProp.GetChildren();
                int i = 0;
                foreach (List list in web.Lists)
                {
                    var listProperties = lists[i];
                    i++;
                    if (listProperties.ContainsKey("Title") && list.Title.Equals(listProperties["Title"].ToString()))
                    {
                        Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                        CopyProperty(rootFolderProp, list.RootFolder);
                        rootFolderProp["Url"] = list.RootFolder.ServerRelativeUrl.Substring(webServerRelativeUrl.Length + 1);
                        rootFolderProp["Exists"] = true;
                        listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                        listProperties["AllowContentTypes"] = list.AllowContentTypes;
                        listProperties["ContentTypesEnabled"] = list.ContentTypesEnabled;
                    }
                }
                return listsProp;
            }
        }
        public Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                FieldCollection fields = null;
                if (listId != Guid.Empty)
                {
                    List list = web.Lists.GetById(listId);
                    fields = list.Fields;
                }
                else
                {
                    fields = web.Fields;
                }
                try
                {
                    context.Load(fields, fs => fs.IncludeWithDefaultProperties().Where(f => f.InternalName == "TaxCatchAll"));
                    context.ExecuteQuery();
                    Dictionary<string, object> taxonomyCatchAllFieldProperties = new Dictionary<string, object>();
                    AssembleSingleFieldProperties(taxonomyCatchAllFieldProperties, fields[0]);
                    return taxonomyCatchAllFieldProperties;
                }
                catch (Exception e)
                {
                    mLogger.Warn("When creating this TaxonomyField, sharepoint didn't create taxonomy catch all field. Error Message:{0}", e.ToString());
                    return null;
                }
            }
        }

        public Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> relatedfields = new Dictionary<string, object>();
                var relatedFieldPropertiesList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                RelatedFieldCollection fieldCollection = list.GetRelatedFields();
                context.Load(fieldCollection);
                context.ExecuteQuery();
                foreach (RelatedField field in fieldCollection)
                {
                    Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                    CopyProperty(fieldProperties, field);
                    relatedFieldPropertiesList.Add(fieldProperties);
                }
                relatedfields.AddChildren(relatedFieldPropertiesList);
                return relatedfields;
            }
        }
        //public Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            return mRequestCommon.GetSitePortal(siteUrl);
        }
        public List<string> GetSiteEnabledHelpCollections()
        {
            return mRequestCommon.GetSiteEnabledHelpCollections();
        }
        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListRated(webServerRelativeUrl, listId);
        }
        public string GetListExperience(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListExperience(webServerRelativeUrl, listId);
        }
        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return mRequestCommon.GetMetadataNavigationSettings(webServerRelativeUrl, listId, listTitle);
        }

        public IDictionary<string, object> TakeOverCheckOut(string webServerRelativeUrl, Guid listId, string fileServerRelativeUrl)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                var fileCollection = list.GetCheckedOutFiles();
                var file = fileCollection.GetByPath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.TakeOverCheckOut();
                context.Load(file);
                context.ExecuteQuery();
                return new Dictionary<string, object>
                        {
                            { "CheckedOutById",file.CheckedOutById },
                            { "ServerRelativePath",file.ServerRelativePath.DecodedUrl }
                        };
            }
        }

        public IList<IDictionary<string, object>> GetListCheckOutFilesWithCSOM(string webServerRelativeUrl, Guid listId)
        {
            var properties = new List<IDictionary<string, object>>();
            using (var context = CreateRetryContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                var fileCollection = list.GetCheckedOutFiles();
                context.Load(fileCollection, fs => fs.Include(f => f.CheckedOutById, f => f.ServerRelativePath));
                context.ExecuteQuery();
                foreach (var file in fileCollection)
                {
                    string path = file.ServerRelativePath.DecodedUrl;
                    int checkOutBy = file.CheckedOutById;
                    properties.Add(
                        new Dictionary<string, object>
                        {
                            { "CheckedOutById",checkOutBy },
                            { "ServerRelativePath",path }
                        });
                }
            }
            return properties;
        }

        public List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            int localedId = 1033;
            bool isTime24 = false;
            if (tokenProvider.TokenType != TokenType.Bearer)
            {
                using (AveClientContext context = CreateContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        context.Load(web);
                        RegionalSettings regionalSettings = web.RegionalSettings;
                        context.Load(regionalSettings, r => r.LocaleId, r => r.Time24);
                        context.ExecuteQuery();
                        localedId = Convert.ToInt32(regionalSettings.LocaleId);
                        isTime24 = regionalSettings.Time24;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Get Web RegionalSettings Failed, Error message:{0}", e.ToString());
                    }
                }
            }
            return mRequestCommon.GetListCheckedOutFiles(webServerRelativeUrl, listId, localedId, isTime24);
        }
        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return mRequestCommon.GetMetadataListFieldSettings(webServerRelativeUrl, "", listId);
        }
        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            mRequestCommon.UpdateMetadataListFieldSettings(webServerRelativeUrl, listId, updateProperties);
        }
        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListVersionLimited(webServerRelativeUrl, listId);
        }
        public Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetPerLocationViewSettings(webServerRelativeUrl, listId);
        }
        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListRssProperties(webServerRelativeUrl, listId);
        }
        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            mRequestCommon.UpdateListRssSetting(webServerRelativeUrl, listId, updateProp);
        }
        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mRequestCommon.GetPublishedContentTypes();
        }
        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListGeneralProperties(webServerRelativeUrl, listId);
        }
        public Dictionary<string, object> GetListEditViewSettingProperties(string webServerRelativeUrl, String listTitle, Guid listId, Guid viewId)
        {
            return new Dictionary<string, object>();
        }
        public Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId)
        {
            return new Dictionary<string, object>();
        }
        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            //this.mWebServiceRequest.mSiteTrimObj
            return this.mRequestCommon.GetListAdvancedSettingProperties(webServerRelativeUrl, listId, null);
        }
        public Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> settings = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(settings, list.InformationRightsManagementSettings);
                return settings;
            }
        }
        public Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> setting = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                list.InformationRightsManagementSettings.Reset();
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(setting, list.InformationRightsManagementSettings);
                return setting;
            }
        }
        public Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> setting = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                AveObjectCopy.UpdateObjectBasicProperties(updateProperties, list.InformationRightsManagementSettings);
                list.InformationRightsManagementSettings.Update();
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(setting, list.InformationRightsManagementSettings);
                return setting;
            }
        }
        public void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.ApplyTheme(colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
                context.ExecuteQuery();
            }
        }

        public void ApplyWebTemplate(string webServerRelativeUrl, string webTemplate, uint lcid)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebTemplateCollection templatecollections = web.GetAvailableWebTemplates(lcid, false);
                WebTemplate template = templatecollections.GetByName(webTemplate);
                context.Load(template);
                context.ExecuteQuery();
                if (template.ServerObjectIsNull != null && !template.ServerObjectIsNull.Value)
                {
                    web.ApplyWebTemplate(template.Name);
                    context.ExecuteQuery();
                }
            }
        }

        public void ApplyWebTemplate(string webServerRelativeUrl, string webTemplateName)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.ApplyWebTemplate(webTemplateName);
                context.ExecuteQuery();
            }
        }

        public List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            return new List<Dictionary<string, object>>();
        }
        public List<Dictionary<string, object>> GetKeyWords()
        {
            return mRequestCommon.GetKeyWords();
        }
        public Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> listTemplates = new Dictionary<string, object>();
                var listTemplateList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListTemplateCollection templates = context.Site.GetCustomListTemplates(web);
                context.Load(templates);
                context.ExecuteQuery();
                foreach (ListTemplate listTemplate in templates)
                {
                    Dictionary<string, object> listTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(listTemplateProperties, listTemplate);
                    //listTemplateProperties["AveBaseType"] = listTemplateProperties["BaseType"];   //ListTemplateProperties中没有"AveBaseType"这个属性
                    //listTemplateProperties["AveListTemplateType"] = listTemplateProperties["ListTemplateTypeKind"]; //ListTemplateProperties中没有"AveListTemplateType"这个属性
                    listTemplateProperties["Type"] = listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateProperties["Type_Client"] = (int)listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateList.Add(listTemplateProperties);
                }
                listTemplates.AddChildren(listTemplateList);
                return listTemplates;
            }
        }
        public Dictionary<string, object> GetAllFeatureDefinitions(string url, int lcid, string featuresSource)
        {
            return mRequestCommon.GetAllFeatureDefinitions(url, lcid, featuresSource);
        }

        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return mRequestCommon.GetDefaultRegionalSetting(webServerRelativeUrl, lcid);
        }
        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl)
        {
            return mRequestCommon.GetThemeUrlForWeb(webServerRelativeUrl);
        }
        public Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            return new Dictionary<string, object>();
        }
        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            return mRequestCommon.GetMasterPageProperties(webServerRelativeUrl);
        }
        public Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl)
        {
            Dictionary<string, object> themeProp = new Dictionary<string, object>();
            Stream themeStream = null;
            try
            {
                themeStream = GetFileBinary(string.Empty, fileServerRelativeUrl);//this.GetFileStream(string.Empty, fileServerRelativeUrl, string.Empty);
            }
            catch (Exception e)
            {
                mLogger.Debug(AveClientOMRequestResource.OpenThmxThemeError, fileServerRelativeUrl, e.ToString());
            }
            if (themeStream != null)
            {
                using (themeStream)
                {
                    Package package = Package.Open(themeStream, FileMode.Open, FileAccess.Read);
                    PackagePart part = package.GetPart(new Uri("/theme/theme/themeManager.xml", UriKind.Relative));
                    PackagePart themePart = null;
                    if (string.Compare(part.ContentType, "application/vnd.openxmlformats-officedocument.themeManager+xml", StringComparison.Ordinal) == 0)
                    {
                        //foreach (PackageRelationship relationship in part.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme"))
                        //{
                        //    if (relationship.TargetMode != TargetMode.Internal)
                        //    {
                        //        throw new Exception("open theme failed");
                        //    }
                        //    themePart = package.GetPart(PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri));
                        //    break;
                        //}
                        var relationship = part.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme").Where(r => r.TargetMode != TargetMode.Internal).FirstOrDefault();
                        themePart = package.GetPart(PackUriHelper.ResolvePartUri(relationship?.SourceUri, relationship?.TargetUri));
                    }
                    if (themePart != null)
                    {
                        try
                        {
                            XmlDocument themeDocument = new XmlDocument();
                            using (Stream stream = themePart.GetStream())
                            {
                                themeDocument.Load(stream);
                            }
                            if (themeDocument.ChildNodes.Count > 1)
                            {
                                XmlNode node = themeDocument.ChildNodes[1];
                                string name = node.Attributes["name"].Value;
                                themeProp["Name"] = name;
                                themeProp["ServerRelativeUrl"] = fileServerRelativeUrl;
                                this.GetThemeProperties(node.FirstChild, themeProp);
                            }
                        }
                        /*review-qlluo*/
                        catch (Exception e)
                        {
                            throw new Exception(string.Format("open theme failed.ErrorMessage:{0}.", e.ToString()));
                        }
                    }
                }
            }
            return themeProp;
        }

        private void GetThemeProperties(XmlNode parent, Dictionary<string, object> themeProp)
        {
            if (parent != null)
            {
                foreach (XmlNode node in parent.ChildNodes)
                {
                    if (node.Name.Equals("a:clrScheme"))
                    {
                        foreach (XmlNode color in node.ChildNodes)
                        {
                            #region color
                            switch (color.Name)
                            {
                                case "a:dk1":
                                    themeProp["DarkColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:lt1":
                                    themeProp["LightColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:dk2":
                                    themeProp["DarkColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:lt2":
                                    themeProp["LightColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent1":
                                    themeProp["AccentColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent2":
                                    themeProp["AccentColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent3":
                                    themeProp["AccentColor3"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent4":
                                    themeProp["AccentColor4"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent5":
                                    themeProp["AccentColor5"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent6":
                                    themeProp["AccentColor6"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:hlink":
                                    themeProp["HyperlinkColor"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:folHlink":
                                    themeProp["FollowedHyperlinkColor"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }
                    else if (node.Name.Equals("a:fontScheme"))
                    {
                        foreach (XmlNode font in node.ChildNodes)
                        {
                            #region font
                            switch (font.Name)
                            {
                                case "a:majorFont":
                                    themeProp["MajorFont"] = font.FirstChild.Attributes["typeface"].Value;
                                    break;
                                case "a:minorFont":
                                    themeProp["MinorFont"] = font.FirstChild.Attributes["typeface"].Value;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }
                }
            }
        }

        public void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            using (ClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                SiteProperties siteProperties = null;
                siteProperties = tenant.GetSitePropertiesByUrl(siteUrl, true);
                siteProperties.Retrieve(
                    SitePropertiesPropertyNames.Template,
                    SitePropertiesPropertyNames.StorageWarningLevel,
                    SitePropertiesPropertyNames.StorageMaximumLevel,
                    SitePropertiesPropertyNames.UserCodeMaximumLevel,
                    SitePropertiesPropertyNames.UserCodeWarningLevel);
                context.ExecuteQuery();
                double rate = 0;
                if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0")) //for my site
                {
                    rate = siteProperties.StorageWarningLevel * 1.0 / siteProperties.StorageMaximumLevel * 1.0;
                    siteProperties.StorageWarningLevel = Convert.ToInt64(storageQuota * Math.Round(rate, 2));
                }
                siteProperties.StorageMaximumLevel = storageQuota;
                if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0"))
                {
                    rate = siteProperties.UserCodeMaximumLevel.Equals(0) ? 0.0 : siteProperties.UserCodeWarningLevel * 1.0 / siteProperties.UserCodeMaximumLevel * 1.0;
                    siteProperties.UserCodeWarningLevel = Convert.ToInt64(serverResourceQuota * Math.Round(rate, 2));
                }
                siteProperties.UserCodeMaximumLevel = serverResourceQuota;
                siteProperties.Update();
                context.ExecuteQuery();
            }
        }

        public object GetClientContext()
        {
            return CreateSimpleContext();
        }

        #endregion

        #region  Add
        public Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                ListItemCreationInformation itemCrtInfo = new ListItemCreationInformation();
                itemCrtInfo.FolderUrl = folderUrl;
                itemCrtInfo.UnderlyingObjectType = (FileSystemObjectType)underlyingObjectType;
                itemCrtInfo.LeafName = leafName;
                ListItem item = list.AddItem(itemCrtInfo);
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    foreach (KeyValuePair<string, object> pair in itemProperties["ChangedFieldValues"] as Dictionary<string, object>)
                    {
                        item[pair.Key] = pair.Value;
                    }
                }
                string updateMethod = itemProperties[AveObjectModelConstant.UpdateMethodName] as string;
                switch (updateMethod)
                {
                    case "Update":
                        item.Update();
                        context.Load(item);
                        context.ExecuteQuery();
                        GetItemDic(returnInfo, item);
                        break;
                    default:
                        break;
                }
                return returnInfo;
            }
        }

        public Dictionary<string, object> AddItemUsingPath(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                ListItemCreationInformationUsingPath itemCrtInfo = new ListItemCreationInformationUsingPath();
                itemCrtInfo.FolderPath = ResourcePath.FromDecodedUrl(folderUrl);
                itemCrtInfo.UnderlyingObjectType = (FileSystemObjectType)underlyingObjectType;
                itemCrtInfo.LeafName = ResourcePath.FromDecodedUrl(leafName);
                ListItem item = list.AddItemUsingPath(itemCrtInfo);
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    foreach (KeyValuePair<string, object> pair in itemProperties["ChangedFieldValues"] as Dictionary<string, object>)
                    {
                        item[pair.Key] = pair.Value;
                    }
                }
                string updateMethod = itemProperties[AveObjectModelConstant.UpdateMethodName] as string;
                switch (updateMethod)
                {
                    case "Update":
                        item.Update();
                        context.Load(item);
                        context.ExecuteQuery();
                        GetItemDic(returnInfo, item);
                        break;
                    default:
                        break;
                }
                return returnInfo;
            }
        }

        public Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        {
            Dictionary<string, object> groupProperties = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                switch (groupSource)
                {
                    case "web.siteGroups":
                        GroupCreationInformation gcinfo = new GroupCreationInformation();
                        gcinfo.Title = groupName;
                        gcinfo.Description = description;
                        Group group = context.Site.RootWeb.SiteGroups.Add(gcinfo);
                        ExceptionHandlingScope exceptionScope = null;
                        if (!string.IsNullOrEmpty(ownerName))
                        {
                            exceptionScope = new ExceptionHandlingScope(context);
                            using (exceptionScope.StartScope())
                            {
                                using (exceptionScope.StartTry())
                                {
                                    if (string.IsNullOrEmpty(ownerType) || "user".Equals(ownerType, StringComparison.OrdinalIgnoreCase))
                                    {
                                        group.Owner = context.Site.RootWeb.SiteUsers.GetByLoginName(ownerName);
                                    }
                                    else
                                    {
                                        group.Owner = context.Site.RootWeb.SiteGroups.GetByName(ownerName);
                                    }
                                    group.Update();
                                }
                                using (exceptionScope.StartCatch())
                                {

                                }
                            }
                        }
                        ExceptionHandlingScope defaultUserException = null;
                        if (!string.IsNullOrEmpty(defaultUserName))
                        {
                            defaultUserException = new ExceptionHandlingScope(context);
                            using (defaultUserException.StartScope())
                            {
                                using (defaultUserException.StartTry())
                                {
                                    group.Users.Add(new UserCreationInformation() { LoginName = defaultUserName });
                                }
                                using (defaultUserException.StartCatch())
                                {
                                }
                            }
                        }

                        context.Load(group);
                        ConditionalScope cs = new ConditionalScope(context, () => group.Owner.ServerObjectIsNull.Value);
                        using (cs.StartScope())
                        {
                            using (cs.StartIfFalse())
                            {
                                context.Load(group, g => g.Owner.Id, g => g.Owner.PrincipalType);
                            }
                        }
                        context.ExecuteQuery();

                        if (exceptionScope != null && exceptionScope.HasException)
                        {
                            mLogger.Warn("Restore Group owner with owner name:{0}, type:{1} for group:{2} failed:{3}", ownerName, ownerType, groupName, exceptionScope.ExtractException());
                        }

                        if (defaultUserException != null && defaultUserException.HasException)
                        {
                            mLogger.Warn("Restore Group default user with name:{0} for group:{1} failed:{2}", defaultUserName, groupName, defaultUserException.ExtractException());
                        }

                        groupProperties.Add("Id", group.Id);
                        groupProperties.Add("Name", group.Title);
                        groupProperties.Add("Title", group.Title);
                        groupProperties.Add("LoginName", group.LoginName);
                        groupProperties.Add("Description", group.Description);
                        if (!group.Owner.ServerObjectIsNull.Value)
                        {
                            groupProperties.Add("OwnerId", group.Owner.Id);
                            groupProperties.Add("OwnerIsUser", group.Owner.PrincipalType == PrincipalType.User);
                        }
                        //SAAS-8191 load出新建的group的四个属性，并添加到groupProperties。
                        groupProperties.Add("AllowMembersEditMembership", group.AllowMembersEditMembership);
                        groupProperties.Add("AllowRequestToJoinLeave", group.AllowRequestToJoinLeave);
                        groupProperties.Add("AutoAcceptRequestToJoinLeave", group.AutoAcceptRequestToJoinLeave);
                        groupProperties.Add("OnlyAllowMembersViewMembership", group.OnlyAllowMembersViewMembership);
                        break;
                    case "web.groups":
                        throw new Exception("You cannot add a group directly to the Groups collection.  You can add a group to the SiteGroups collection.");
                }
            }
            return groupProperties;
        }

        public Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere)
        {
            for (int count = 0; count < WrapperConfiguration.WrapperConfigurationForBPOS.AddWebRetryCount; count++)
            {
                try
                {
                    return RetryAddWeb(parentWebRelativeUrl, webUrl, description, language, title, useSamePermissionsAsParentSite, webTemplate, bConvertIfThere);
                }
                /*review-qlluo*/
                catch (ServerException ex)
                {
                    //retry when met server exception, if there's any special error code, handle it later
                    if (count == WrapperConfiguration.WrapperConfigurationForBPOS.AddWebRetryCount - 1)
                    {
                        throw;
                    }
                    Thread.Sleep(count * 15 * 1000);
                    mLogger.Warn("retry add web,retry times:{0},error message:{1}", count, ex.Message);
                }
            }
            throw new Exception(string.Format("retry add web failed,retry times:{0}", 5));
        }

        private Dictionary<string, object> RetryAddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere)
        {
            using (AveClientContext context = CreateContext())
            {
                context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpCreateWebRequestTimeout;
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                //if (CompatibilityLevel >= 15)
                {
                    WebCreationInformation wci = new WebCreationInformation();
                    wci.Url = webUrl.Trim(' ');
                    wci.Title = title;
                    wci.Description = description;
                    wci.Language = (int)language;
                    wci.UseSamePermissionsAsParentSite = useSamePermissionsAsParentSite;
                    wci.WebTemplate = webTemplate;
                    Web parentWeb = context.Site.OpenWeb(parentWebRelativeUrl);
                    Web newWeb = parentWeb.Webs.Add(wci);
                    try
                    {
                        context.ExecuteQuery();
                        webProperties = GetWebProperties(context, newWeb, context.Url, mSiteRelativeUrl, false);
                    }
                    /*review-qlluo*/
                    catch (WebException e)
                    {
                        if (WebExceptionStatus.Timeout == e.Status)
                        {
                            string webRelativeUrl = parentWebRelativeUrl.TrimEnd('/') + "/" + webUrl;
                            webProperties = this.GetWeb(webRelativeUrl);
                            if (webProperties.ContainsKey("Exists") && Convert.ToBoolean(webProperties["Exists"]))
                            {
                                return webProperties;
                            }
                        }
                        throw;
                    }
                }
                //else
                //{
                //    if (string.IsNullOrEmpty(parentWebRelativeUrl))
                //    {
                //        Web parentWeb = context.Site.OpenWeb(parentWebRelativeUrl);
                //        context.Load(parentWeb, w => w.ServerRelativeUrl);
                //        context.ExecuteQuery();
                //        parentWebRelativeUrl = parentWeb.ServerRelativeUrl;
                //    }
                //    mWebServiceRequest.AddWeb(parentWebRelativeUrl, webUrl, description, language, title, useSamePermissionsAsParentSite, webTemplate, bConvertIfThere);
                //    Web newWeb = context.Site.OpenWeb(string.Format("{0}/{1}", parentWebRelativeUrl.TrimEnd('/'), webUrl));
                //    webProperties = GetWebProperties(context, newWeb, context.Url, mSiteRelativeUrl, false);
                //}
                return webProperties;
            }
        }

        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int itemId, int eventType, int frequency, bool isSendEmail)
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int eventType, int frequency, bool isSendEmail)
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, int webTemplateType, string featureId = null)
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                try
                {

                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    ListCreationInformation newList = new ListCreationInformation();
                    newList.Description = description;
                    newList.Title = title;
                    newList.TemplateType = webTemplateType;
                    if (!string.IsNullOrEmpty(featureId))
                    {
                        newList.TemplateFeatureId = new Guid(featureId);
                    }
                    List list = web.Lists.Add(newList);
                    TryLoadList(context, web, ref list, title);
                    CopyProperty(prop, list);
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                    prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    return prop;
                }
                /*review-qlluo*/
                catch (ServerException serverException)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, templateType:{3}, serverErrorCode:{4}, exception:{5}",
                        webServerRelativeUrl, title, description, webTemplateType, serverException.ServerErrorCode, serverException);
                    string listUrl = title;
                    switch (serverException.ServerErrorCode)
                    {
                        case -2130575300:  //SPUniqueListInstanceException
                        case -2130575342:   //SPListExistException
                        case -2147024809: //name is invalid
                            Web web = context.Site.OpenWeb(webServerRelativeUrl);
                            context.Load(web.Lists, ls => ls.Include(l => l.RootFolder.ServerRelativeUrl));
                            context.ExecuteQuery();
                            foreach (List reloadList in web.Lists)
                            {
                                if (reloadList.RootFolder.ServerRelativeUrl.EndsWith(listUrl, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    List newLoadList = web.GetList(reloadList.RootFolder.ServerRelativeUrl);
                                    this.LoadList(context, newLoadList);
                                    CopyProperty(prop, newLoadList);
                                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                                    AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, newLoadList.RootFolder);
                                    prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                                    return prop;
                                }
                            }
                            break;
                        case -2130575282://his site has exceeded its maximum file storage limit. 不需要做转换。
                            throw;
                        default:
                            break;
                    }
                    throw ExceptionHandleUtil.ConvertServerException(serverException);
                }
                /*review-qlluo*/
                catch (Exception ex)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, templateType:{3}, exception:{4}",
                       webServerRelativeUrl, title, description, webTemplateType, ex);
                    throw;
                }
                return null;
            }
        }
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions)
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    ListCreationInformation newList = new ListCreationInformation();
                    newList.Description = description;
                    newList.Title = title;
                    newList.Url = url;
                    newList.TemplateFeatureId = new Guid(featureId);
                    newList.TemplateType = templateType;
                    newList.DocumentTemplateType = string.IsNullOrEmpty(docTemplateType) ? 0 : Convert.ToInt32(docTemplateType);
                    newList.QuickLaunchOption = (QuickLaunchOptions)quickLaunchOptions;
                    List list = web.Lists.Add(newList);
                    TryLoadList(context, web, ref list, title);
                    AveObjectCopy.GetObjectBasicProperties(prop, list);
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                    prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    return prop;
                }
                /*review-qlluo*/
                catch (ServerException serverException)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, url:{3}, featureId:{4}, templateType:{5}, document template type:{6}, quick launch:{7},serverErrorCode:{8}, exception:{9}",
                        webServerRelativeUrl, title, description, url, featureId, templateType, docTemplateType, quickLaunchOptions, serverException.ServerErrorCode, serverException);
                    string listUrl = string.IsNullOrEmpty(url) ? title : url;
                    switch (serverException.ServerErrorCode)
                    {
                        case -2130575300:  //SPUniqueListInstanceException
                        case -2130575342:   //SPListExistException
                        case -2147024809: //name is invalid
                            //Web web = context.Site.OpenWeb(webServerRelativeUrl);
                            //context.Load(web.Lists, ls => ls.Include(l => l.RootFolder.ServerRelativeUrl,l=>l.Title));
                            //context.ExecuteQuery();
                            //foreach (List reloadList in web.Lists)
                            //{
                            //    if (reloadList.RootFolder.ServerRelativeUrl.EndsWith(listUrl, StringComparison.InvariantCultureIgnoreCase)
                            //        &&string.Equals(reloadList.Title,title,StringComparison.InvariantCultureIgnoreCase))
                            //    {
                            //        List newLoadList = web.GetList(reloadList.RootFolder.ServerRelativeUrl);
                            //        this.LoadList(context, newLoadList);
                            //        AveObjectCopy.GetObjectBasicProperties(prop, newLoadList);
                            //        Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                            //        AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, newLoadList.RootFolder);
                            //        prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                            //        return prop;
                            //    }
                            //}
                            break;
                        case -2130575282://his site has exceeded its maximum file storage limit. 不需要做转换。
                            throw;
                        default:
                            break;
                    }
                    throw ExceptionHandleUtil.ConvertServerException(serverException);
                }
                /*review-qlluo*/
                catch (Exception ex)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, url:{3}, featureId:{4}, templateType:{5}, document template type:{6}, quick launch:{7}, exception:{8}",
                        webServerRelativeUrl, title, description, url, featureId, templateType, docTemplateType, quickLaunchOptions, ex);
                    throw;
                }
            }
        }

        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource)
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    ListCreationInformation newList = new ListCreationInformation();
                    newList.Title = title;
                    newList.Description = description;
                    newList.Url = url;
                    newList.DataSourceProperties.Add(AveBDCProperties.LobSystemInstance, dataSource[AveBDCProperties.LobSystemInstance] as string);
                    newList.DataSourceProperties.Add(AveBDCProperties.EntityNamespace, dataSource[AveBDCProperties.EntityNamespace] as string);
                    newList.DataSourceProperties.Add(AveBDCProperties.Entity, dataSource[AveBDCProperties.Entity] as string);
                    newList.DataSourceProperties.Add(AveBDCProperties.SpecificFinder, dataSource[AveBDCProperties.SpecificFinder] as string);
                    List list = web.Lists.Add(newList);
                    TryLoadList(context, web, ref list, title);
                    CopyProperty(prop, list);
                    if (list.DataSource != null && list.BaseTemplate == (int)AveListTemplateType.ExternalList)
                    {
                        Dictionary<string, object> listDataSource = new Dictionary<string, object>();
                        listDataSource.Add(AveBDCProperties.LobSystemInstance, list.DataSource.Properties[AveBDCProperties.LobSystemInstance]);
                        listDataSource.Add(AveBDCProperties.EntityNamespace, list.DataSource.Properties[AveBDCProperties.EntityNamespace]);
                        listDataSource.Add(AveBDCProperties.Entity, list.DataSource.Properties[AveBDCProperties.Entity]);
                        listDataSource.Add(AveBDCProperties.SpecificFinder, list.DataSource.Properties[AveBDCProperties.SpecificFinder]);
                        prop.Add("DataSource" + AveObjectModelConstant.ObjectPropertySuffix, listDataSource);
                        prop.Remove("DataSource");
                        //ItemCount == 0
                        //prop.Remove("ItemCount");
                    }
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                    prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    return prop;
                }
                /*review-qlluo*/
                catch (ServerException serverException)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, url:{3}, serverErrorcode:{4} ,exception:{5}",
                        webServerRelativeUrl, title, description, url, serverException.ServerErrorCode, serverException);
                    string listUrl = string.IsNullOrEmpty(url) ? title : url;
                    switch (serverException.ServerErrorCode)
                    {
                        case -2130575300:  //SPUniqueListInstanceException
                        case -2130575342:   //SPListExistException
                        case -2147024809: //name is invalid
                            Web web = context.Site.OpenWeb(webServerRelativeUrl);
                            context.Load(web.Lists, ls => ls.Include(l => l.RootFolder.ServerRelativeUrl));
                            context.ExecuteQuery();
                            foreach (List reloadList in web.Lists)
                            {
                                if (reloadList.RootFolder.ServerRelativeUrl.EndsWith(listUrl, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    List newLoadList = web.GetList(reloadList.RootFolder.ServerRelativeUrl);
                                    this.LoadList(context, newLoadList);
                                    CopyProperty(prop, newLoadList);
                                    if (newLoadList.DataSource != null && newLoadList.BaseTemplate == (int)AveListTemplateType.ExternalList)
                                    {
                                        Dictionary<string, object> listDataSource = new Dictionary<string, object>();
                                        listDataSource.Add(AveBDCProperties.LobSystemInstance, newLoadList.DataSource.Properties[AveBDCProperties.LobSystemInstance]);
                                        listDataSource.Add(AveBDCProperties.EntityNamespace, newLoadList.DataSource.Properties[AveBDCProperties.EntityNamespace]);
                                        listDataSource.Add(AveBDCProperties.Entity, newLoadList.DataSource.Properties[AveBDCProperties.Entity]);
                                        listDataSource.Add(AveBDCProperties.SpecificFinder, newLoadList.DataSource.Properties[AveBDCProperties.SpecificFinder]);
                                        prop.Add("DataSource" + AveObjectModelConstant.ObjectPropertySuffix, listDataSource);
                                        prop.Remove("DataSource");
                                        //ItemCount == 0
                                        //prop.Remove("ItemCount");
                                    }
                                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                                    AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, newLoadList.RootFolder);
                                    prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                                    return prop;
                                }
                            }
                            break;
                        case -2130575282://his site has exceeded its maximum file storage limit. 不需要做转换。
                            throw;
                        default:
                            break;
                    }
                    throw ExceptionHandleUtil.ConvertServerException(serverException);
                }
                /*review-qlluo*/
                catch (Exception ex)
                {
                    mLogger.Error("Add list with web server relative url:{0}, title:{1}, description:{2}, url:{3}, exception:{4}",
                        webServerRelativeUrl, title, description, url, ex);
                    throw;
                }
            }
            return null;
        }
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            this.mRequestCommon.AddList(webServerRelativeUrl, title, description, listTemplate);
            return this.GetList(webServerRelativeUrl, title);
        }

        private void TryLoadList(AveClientContext context, Web web, ref List list, string title)
        {
            try
            {
                this.LoadList(context, list);
            }
            /*review-qlluo*/
            catch (Exception e)
            {
                if (e is ServerException && e.Message.ToUpper().Contains("HRESULT: 0X8107140D"))
                {
                    list = web.Lists.GetByTitle(title);
                    this.LoadList(context, list);
                }
                else
                {
                    throw;
                }
            }
        }

        public Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> newRoleDefinitionProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleDefinitionCreationInformation rdci = new RoleDefinitionCreationInformation();
                AveObjectCopy.UpdateObjectBasicPropertiesWithEscape(roleDefinitionProperties, rdci, new string[] { "BasePermissions" });
                rdci.BasePermissions = ConvertULongToBasePerm((ulong)roleDefinitionProperties["BasePermissions"]);
                RoleDefinition roleDefinition = web.RoleDefinitions.Add(rdci);
                context.Load(roleDefinition);
                context.ExecuteQuery();
                CopyProperty(newRoleDefinitionProperties, roleDefinition);
                newRoleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
                return newRoleDefinitionProperties;
            }
        }

        public Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {

                Dictionary<string, object> newRoleAssignmentProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Principal principal = null;
                RoleDefinitionBindingCollection roleDefinitionBindingCol = null;
                RoleAssignment roleAssignment = null;
                if (GetRoleAssignment(context.Site, web, roleAssignmentProperties, out principal, out roleDefinitionBindingCol))
                {
                    switch (roleAssignmentsSource)
                    {
                        case "web.roleAssignments":
                            roleAssignment = web.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                        case "list.roleAssignments":
                            List list = web.Lists.GetById(listId);
                            roleAssignment = list.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                        case "item.roleAssignments":
                            List list1 = web.Lists.GetById(listId);
                            ListItem listItem = list1.GetItemById(itemId);
                            roleAssignment = listItem.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                    }
                    ArgumentCheck.CheckNotNull(roleAssignment);
                    context.Load(roleAssignment);
                    context.Load(roleAssignment?.RoleDefinitionBindings);
                    context.Load(roleAssignment, r => r.Member);
                    context.ExecuteQuery();
                    AssembleRoleAssignmetProperites(newRoleAssignmentProperties, roleAssignment);
                }
                return newRoleAssignmentProperties;
            }
        }

        public Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, int itemId, string leafName, byte[] attachment)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webRelativeUrl);

                var list = web.Lists.GetByTitle(listName);

                var listItem = list.GetItemById(itemId);

                using (var memoryStream = new MemoryStream(attachment))
                {
                    var attach = listItem.AttachmentFiles.AddUsingPath(ResourcePath.FromDecodedUrl(leafName), memoryStream);
                    listItem.SystemUpdate();
                    context.Load(attach, a => a.ServerRelativeUrl);
                    context.ExecuteQuery();
                    fileProperties.Add("FileName", leafName);
                    fileProperties.Add("ServerRelativeUrl", attach.ServerRelativeUrl);
                }

                return fileProperties;
            }
        }

        public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile newFile = null;
                string fileType = Path.GetExtension(urlOfFile);
                //因为save binary direct方法不支持app token，是直接http request，所以需要屏蔽，以后尽量试用Add方法。
                if (SpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase))
                {
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    //support special char ("#%")
                    FileCollectionAddParameters fileParameters = new FileCollectionAddParameters();
                    fileParameters.Overwrite = overwrite;
                    Stream fileStream = new MemoryStream(file);
                    ResourcePath filePath = ResourcePath.FromDecodedUrl(urlOfFile);
                    newFile = folder.Files.AddUsingPathV1(filePath, fileParameters, fileStream);
                }
                else
                {
                    //context.ExecuteQuery();
                    using (MemoryStream stream = new MemoryStream(file))
                    {
                        if (!urlOfFile.StartsWith("/") && !urlOfFile.StartsWith(folderServerRelativeUrl))
                        {
                            urlOfFile = folderServerRelativeUrl + "/" + urlOfFile;
                        }
                        //Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, urlOfFile, stream, true);
                        AddFileByRestApi(webServerRelativeUrl, urlOfFile, stream, true);
                    }
                    newFile = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(urlOfFile));
                }

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Get AddFile CheckedOutByUser Error, newFileUrl:{0} , Error Message:{1}", urlOfFile, excepScope.ErrorMessage);
                }
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }
        public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            string currentWebUrl = AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/');
            using (AveClientContext context = CreateContext(currentWebUrl))
            {
                string serverRelativeUrl = string.Empty;
                if (urlOfFile.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile.Substring(WebAppName.Length);
                }
                else if (urlOfFile.StartsWith(folderServerRelativeUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile;
                }
                else
                {
                    serverRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                }

                if (file.Length <= LARGE_FILE_BLOCK_SIZE)
                {
                    //Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, serverRelativeUrl, file, overwrite);
                    AddFileByRestApi(webServerRelativeUrl, serverRelativeUrl, file, overwrite);
                }
                else
                {
                    //context.RequestTimeout = 60 * 60 * 1000;
                    ClientFile uploadFile = null;
                    ClientResult<long> bytesUploaded = null;
                    Folder parentFolder = context.Web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    using (BinaryReader br = new BinaryReader(file))
                    {
                        Guid uploadId = Guid.NewGuid();
                        byte[] buffer = new byte[LARGE_FILE_BLOCK_SIZE];
                        long fileoffset = 0;//fileoffset is the pointer where the next slice will be added
                        long totalBytesRead = 0;
                        int bytesRead;
                        bool first = true;

                        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            if (first)
                            {
                                using (MemoryStream emptyContent = new MemoryStream())
                                {
                                    // Add an empty file. support special charaters("#%")
                                    FileCollectionAddParameters fileParameters = new FileCollectionAddParameters();
                                    fileParameters.Overwrite = true;
                                    ResourcePath filePath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                                    uploadFile = parentFolder.Files.AddUsingPathV1(filePath, fileParameters, emptyContent);
                                    using (MemoryStream s = new MemoryStream(buffer))
                                    {
                                        bytesUploaded = uploadFile.StartUpload(uploadId, s);
                                        context.ExecuteQuery();
                                        fileoffset = bytesUploaded.Value;
                                    }
                                }
                                first = false;
                            }
                            else
                            {
                                uploadFile = context.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
                                if (totalBytesRead < file.Length)
                                {
                                    using (MemoryStream s = new MemoryStream(buffer))
                                    {
                                        bytesUploaded = uploadFile.ContinueUpload(uploadId, fileoffset, s);
                                        context.ExecuteQuery();
                                        fileoffset = bytesUploaded.Value;
                                    }
                                }
                                else//the last slice of data
                                {
                                    byte[] lastBuffer = new byte[bytesRead];
                                    Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);// Copy to a new buffer that has the correct size
                                    using (MemoryStream s = new MemoryStream(lastBuffer))
                                    {
                                        uploadFile.FinishUpload(uploadId, fileoffset, s);
                                        context.ExecuteQuery();
                                    }
                                }
                            }
                        }
                    }
                }
                return this.GetFile(webServerRelativeUrl, serverRelativeUrl, null);
            }
        }

        private void AddParentFolderToCache(ClientContext context, List list, Folder folder, Dictionary<string, object> existFolders, List<Dictionary<string, object>> changeFolderCache)
        {
            if (folder == null)
            {
                return;
            }
            context.Load(folder);
            context.Load(folder.ListItemAllFields, i => i.Id);
            context.ExecuteQuery();
            string folderUrl = folder.ServerRelativeUrl;
            if (folderUrl.Equals(list.RootFolder.ServerRelativeUrl) || existFolders.ContainsKey(folderUrl))
            {
                return;
            }
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            CopyProperty(folderProperties, folder);
            folderProperties["ChangeType"] = AvePoint.Wrapper.Common.ChangeType.None;
            folderProperties["LeafName"] = folder.Name;
            folderProperties["Versions"] = new List<Dictionary<string, object>>();
            folderProperties["DoclibRowId"] = folder.ListItemAllFields.Id;
            //folderProperties["DocID"] = folder.ListItemAllFields.FieldValues.ContainsKey("UniqueId") ? folder.ListItemAllFields.FieldValues["UniqueId"] : Guid.Empty;//获得folder的docid
            folderProperties["DocID"] = folder.UniqueId;
            folderProperties["FullUrl"] = folder.ServerRelativeUrl;
            existFolders[folderUrl] = folderProperties;
            changeFolderCache.Add(folderProperties);
            AddParentFolderToCache(context, list, folder.ParentFolder, existFolders, changeFolderCache);
        }

        public enum SaveBinaryCheckMode
        {
            ETag,
            Overwrite
        }

        public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                ClientFile newFile = folder.Files.AddTemplateFile(urlOfFile, (TemplateFileType)templateFileType);

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Get AddFile CheckedOutByUser Error, newFileUrl:{0} , Error Message:{1}", urlOfFile, excepScope.ErrorMessage);
                }
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }
        public Dictionary<string, object> AddFolder(string webServerRelativeUrl, string folderServerRelativeUrl, string strUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder parentFolder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                FolderCollectionAddParameters folderAddParam = new FolderCollectionAddParameters();
                folderAddParam.Overwrite = true;
                Folder newFolder = parentFolder.Folders.AddUsingPath(ResourcePath.FromDecodedUrl(strUrl), folderAddParam);
                context.Load(newFolder);
                context.ExecuteQuery();
                Dictionary<string, object> newFolderPro = new Dictionary<string, object>();
                CopyProperty(newFolderPro, newFolder);
                newFolderPro["Exists"] = true;
                newFolderPro["Url"] = strUrl;
                return newFolderPro;
            }
        }

        public Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ViewCollection views = list.Views;
                string[] viewFields = new string[strCollViewFields.Count];
                strCollViewFields.CopyTo(viewFields, 0);
                ViewCreationInformation newViewCrtInfo = new ViewCreationInformation();
                newViewCrtInfo.Paged = bPaged;
                newViewCrtInfo.PersonalView = bPersonalView;
                newViewCrtInfo.Query = strQuery;
                newViewCrtInfo.RowLimit = iRowLimit;
                newViewCrtInfo.SetAsDefaultView = bMakeViewDefault;
                newViewCrtInfo.Title = strViewName;
                newViewCrtInfo.ViewFields = viewFields;
                newViewCrtInfo.ViewTypeKind = (ViewType)type;
                View newView = views.Add(newViewCrtInfo);
                context.Load(newView);
                context.Load(newView, v => v.ViewFields);
                context.ExecuteQuery();
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                AssembleViewProperties(returnInfo, newView, webServerRelativeUrl);
                return returnInfo;
            }
        }
        public void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.Add(field);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            try
            {
                return AddFeatureByRestApi(WebAppName, webServerRelativeUrl, featureId, force, featuresSource);
            }
            catch (WebException e)
            {
                mLogger.Warn("Add feature {0} failed with rest api,will try to active it by CSOM API.Error:{1}", featureId, e);
            }
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> featureProperties = new Dictionary<string, object>();
                Feature newFeature = null;
                switch (featuresSource)
                {
                    case "site.features":
                        newFeature = context.Site.Features.Add(featureId, force, (FeatureDefinitionScope)scope);
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        newFeature = web.Features.Add(featureId, force, (FeatureDefinitionScope)scope);
                        break;
                }
                context.Load(newFeature);//否則下面方法用到會拋異常。SAAS-615
                context.ExecuteQuery();
                AssembleFeatureProperties(featureProperties, newFeature);
                return featureProperties;
            }
        }
        public Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                ContentType newCont = null;
                if (newContentTypeProperties.ContainsKey("IsNew"))
                {
                    Dictionary<string, object> parentContentDic = newContentTypeProperties["ParentContentType" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    string parentContentTypeWebServerRelativeUrl = parentContentDic[AveObjectModelConstant.WebServerRelativeUrl] as string;
                    string parentContentTypeListName = parentContentDic[AveObjectModelConstant.ListTitle] as string;
                    Guid parentContentTypeListId = (Guid)parentContentDic[AveObjectModelConstant.ListId];
                    string parentContentTypeId = parentContentDic["ContentTypeId"] as string;
                    string parentContentTypeSource = parentContentDic["ContentTypeSource"] as string;
                    ContentType parentContentType = this.GetContentTypeWithoutFields(context, parentContentTypeWebServerRelativeUrl, parentContentTypeListName, parentContentTypeListId, parentContentTypeSource, parentContentTypeId);
                    //context.Load(parentContentType);
                    //context.ExecuteQuery();
                    ContentTypeCollection cts = this.GetContentTypesWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                    ContentTypeCreationInformation createInfo = new ContentTypeCreationInformation();
                    if (newContentTypeProperties.ContainsKey("ContentTypeId"))
                    {
                        createInfo.Id = newContentTypeProperties["ContentTypeId"] as string;
                    }
                    else
                    {
                        createInfo.ParentContentType = parentContentType;
                    }
                    createInfo.Name = newContentTypeProperties["Name"] as string;
                    //createInfo.Description = newContentTypeProperties["Description"] as string;
                    newCont = cts.Add(createInfo);
                    context.Load(newCont);
                    context.Load(newCont, ct => ct.SchemaXml, ct => ct.WorkflowAssociations);
                    context.Load(newCont.Parent);
                    context.ExecuteQuery();
                }
                else
                {
                    string existContentTypeWebServerRelativeUrl = newContentTypeProperties[AveObjectModelConstant.WebServerRelativeUrl] as string;
                    string existContentTypeListName = newContentTypeProperties[AveObjectModelConstant.ListTitle] as string;
                    Guid parentContentTypeListId = (Guid)newContentTypeProperties[AveObjectModelConstant.ListId];
                    string existContentTypeId = newContentTypeProperties["ContentTypeId"] as string;
                    string existContentTypeSource = newContentTypeProperties["ContentTypeSource"] as string;
                    ContentType existContentType = this.GetContentTypeWithoutFields(context, existContentTypeWebServerRelativeUrl, existContentTypeListName, parentContentTypeListId, existContentTypeSource, existContentTypeId);
                    //context.Load(existContentType);
                    //context.ExecuteQuery();
                    ContentTypeCollection cts = this.GetContentTypesWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                    newCont = cts.AddExistingContentType(existContentType);
                    context.Load(newCont);
                    context.Load(newCont, ct => ct.SchemaXml, ct => ct.WorkflowAssociations);
                    context.Load(newCont.Parent);
                    context.ExecuteQuery();
                }
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(newProp, newCont);
                return newProp;
            }
        }

        public Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                context.Load(workflowServicesManager);
                context.ExecuteQuery();
                CopyProperty(returnInfo, workflowServicesManager);
                return returnInfo;
            }
        }

        public Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionService workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();
                WorkflowSubscriptionCollection workflowSubscriptionColl = workflowSubscriptionService.EnumerateSubscriptionsByList(listId);
                context.Load(workflowSubscriptionColl);
                context.ExecuteQuery();

                var subscrips = new List<IDictionary<string, object>>();
                foreach (WorkflowSubscription workflow in workflowSubscriptionColl)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    subscrips.Add(workflowPro);
                }
                returnInfo.AddChildren(subscrips);

                return returnInfo;
            }
        }

        public Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId)
        {
            var returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionCollection workflowSubscriptionColl = workflowServicesManager.GetWorkflowSubscriptionService().EnumerateSubscriptionsByEventSource(webId);
                context.Load(workflowSubscriptionColl);
                context.ExecuteQuery();
                var subscrips = new List<IDictionary<string, object>>();
                foreach (WorkflowSubscription workflow in workflowSubscriptionColl)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    subscrips.Add(workflowPro);
                }
                returnInfo.AddChildren(subscrips);
                return returnInfo;
            }
        }

        public Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowDeploymentService workflowDeploymentService = new WorkflowServicesManager(context, web).GetWorkflowDeploymentService();
                WorkflowDefinitionCollection workflowDefinitions = workflowDeploymentService.EnumerateDefinitions(publishedOnly);
                context.Load(workflowDefinitions);
                context.ExecuteQuery();

                var definitions = new List<IDictionary<string, object>>();
                foreach (WorkflowDefinition definition in workflowDefinitions)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, definition);
                    definitions.Add(workflowPro);
                }
                returnInfo.AddChildren(definitions);
                return returnInfo;
            }
        }

        public Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowDeploymentService workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
                WorkflowDefinition workflowDefinition = workflowDeploymentService.GetDefinition(definitionId);
                context.Load(workflowDefinition);
                context.ExecuteQuery();
                CopyProperty(returnInfo, workflowDefinition);
                return returnInfo;
            }
        }

        public Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition)
        {
            //Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowDeploymentService workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
                WorkflowDefinition workflowDefinition = new WorkflowDefinition(context);
                workflowDefinition.AssociationUrl = definition.AssociationUrl;
                workflowDefinition.Description = definition.Description;
                workflowDefinition.DisplayName = definition.DisplayName;
                workflowDefinition.DraftVersion = definition.DraftVersion;
                workflowDefinition.FormField = definition.FormField;
                workflowDefinition.InitiationUrl = definition.InitiationUrl;
                if (definition.Properties != null)
                {
                    foreach (KeyValuePair<string, string> kv in definition.Properties)
                    {
                        workflowDefinition.SetProperty(kv.Key, kv.Value);
                    }
                }
                workflowDefinition.RequiresAssociationForm = definition.RequiresAssociationForm;
                workflowDefinition.RequiresInitiationForm = definition.RequiresInitiationForm;
                workflowDefinition.RestrictToScope = definition.RestrictToScope;
                workflowDefinition.RestrictToType = definition.RestrictToType;
                workflowDefinition.Xaml = definition.Xaml;
                if (!definition.Id.Equals(Guid.Empty))
                {
                    workflowDefinition.Id = definition.Id;
                }
                ClientResult<Guid> res = workflowDeploymentService.SaveDefinition(workflowDefinition);

                context.Load(workflowDefinition);
                context.ExecuteQuery();
                definition.Id = workflowDefinition.Id;

                return res.Value;
            }
        }

        public void PublishDefinition(string webServerRelativeUrl, Guid definitionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowDeploymentService workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
                workflowDeploymentService.PublishDefinition(definitionId);
                context.ExecuteQuery();
            }
        }

        public Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionService workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();
                WorkflowSubscription workflowSubscription = new WorkflowSubscription(context);
                workflowSubscription.DefinitionId = subscription.DefinitionId;
                //workflowSubscription.Enabled = subscription.Enabled;
                workflowSubscription.EventSourceId = subscription.EventSourceId;
                workflowSubscription.Id = subscription.Id;
                workflowSubscription.Name = subscription.Name;
                workflowSubscription.Enabled = subscription.Enabled;
                //workflowSubscription.SetProperty("HistoryListId", subscription.GetProperty("HistoryListId"));
                //workflowSubscription.SetProperty("TaskListId", subscription.GetProperty("TaskListId"));
                if (subscription.PropertyDefinitions != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in subscription.PropertyDefinitions)
                    {
                        workflowSubscription.SetProperty(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                workflowSubscription.StatusFieldName = subscription.StatusFieldName;
                string eventTypeStr = string.Empty;
                List<string> eventTypes = new List<string>();
                foreach (string eventType in subscription.EventTypes)
                {
                    if (!eventTypes.Contains(eventType))
                    {
                        eventTypeStr += eventType + "#;";
                        eventTypes.Add(eventType);
                    }
                }
                workflowSubscription.EventTypes = eventTypes.ToArray();
                ClientResult<Guid> res = listId != Guid.Empty ? workflowSubscriptionService.PublishSubscriptionForList(workflowSubscription, listId) :
                                                                workflowSubscriptionService.PublishSubscription(workflowSubscription);
                context.ExecuteQuery();
                return res.Value;
            }
        }

        public Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionService workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();
                WorkflowSubscription workflowSubscription = workflowSubscriptionService.GetSubscription(subscriptionId);
                context.Load(workflowSubscription);
                context.ExecuteQuery();
                CopyProperty(returnInfo, workflowSubscription);
                return returnInfo;
            }
        }

        public Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, Guid listId, string listTitle, string eventReceiverDefSource, int receiverType, string assembly, string className, string name)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiverInfo = new Dictionary<string, object>();
                try
                {
                    EventReceiverDefinitionCollection eventReceivers;
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = null;
                    switch (eventReceiverDefSource)
                    {
                        case "web.eventReceivers":
                            eventReceivers = web.EventReceivers; break;
                        case "list.eventReceivers":
                            list = web.Lists.GetById(listId);
                            eventReceivers = list.EventReceivers; break;
                        default:
                            eventReceivers = web.EventReceivers; break;
                    }
                    EventReceiverDefinitionCreationInformation eventReceiverCreateInfo = new EventReceiverDefinitionCreationInformation();
                    eventReceiverCreateInfo.EventType = (EventReceiverType)receiverType;
                    eventReceiverCreateInfo.ReceiverAssembly = assembly;
                    eventReceiverCreateInfo.ReceiverClass = className;
                    eventReceiverCreateInfo.ReceiverName = name;
                    EventReceiverDefinition eventReceiver = eventReceivers.Add(eventReceiverCreateInfo);
                    if (list != null)
                    {
                        list.Update();
                    }
                    else
                    {
                        web.Update();
                    }
                    context.Load(eventReceiver);
                    context.ExecuteQuery();
                    eventReceiverInfo["Assembly"] = eventReceiver.ReceiverAssembly;
                    eventReceiverInfo["Class"] = eventReceiver.ReceiverClass;
                    eventReceiverInfo["Name"] = eventReceiver.ReceiverName;
                    eventReceiverInfo["Id"] = eventReceiver.ReceiverId;
                    eventReceiverInfo["Type"] = eventReceiver.EventType;
                }
                catch (Exception e)
                {
                    mLogger.Warn(e.ToString());
                }
                return eventReceiverInfo;
            }
        }
        public Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource)
        {
            ClientContext context = CreateContext();
            if (parentNodeProperties != null && parentNodeProperties.ContainsKey("ClientContext"))
            {
                context = parentNodeProperties["ClientContext"] as ClientContext;
            }

            Dictionary<string, object> newNavigationNodeProperties = new Dictionary<string, object>();
            Web web = context.Site.OpenWeb(webRelativeUrl);
            NavigationNode newNavigationNode = null;
            NavigationNodeCollection navigationNodeCollection = null;
            NavigationNodeCreationInformation createInfo = new NavigationNodeCreationInformation();
            createInfo.AsLastNode = newNodeProperties.ContainsKey("AsLastNode") ? (bool)newNodeProperties["AsLastNode"] : false;
            createInfo.Title = newNodeProperties.ContainsKey("Title") ? (string)newNodeProperties["Title"] : null;
            createInfo.Url = newNodeProperties.ContainsKey("Url") ? (string)newNodeProperties["Url"] : null;
            createInfo.IsExternal = newNodeProperties.ContainsKey("IsExternal") ? (bool)newNodeProperties["IsExternal"] : false;

            if (newNodeProperties.ContainsKey("PreviousNode"))
            {
                Dictionary<string, object> location = newNodeProperties["PreviousNode"] as Dictionary<string, object>;
                if (location.ContainsKey("Id" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    createInfo.PreviousNode = new NavigationNode(context, location["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                }
            }
            switch (navigationSource)
            {
                case "children":
                    NavigationNode parentNavigationNode = new NavigationNode(context, parentNodeProperties?["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                    navigationNodeCollection = parentNavigationNode.Children;
                    break;
                case "topNavigationBar":
                    navigationNodeCollection = web.Navigation.TopNavigationBar;
                    break;
                case "quickLaunch":
                    navigationNodeCollection = web.Navigation.QuickLaunch;
                    break;
                case "searchNav":
                    navigationNodeCollection = web.Navigation.GetNodeById(0x410).Children;
                    break;
            }
            //mClientContext.Load(navigationNodeCollection);
            ArgumentCheck.CheckNotNull(navigationNodeCollection);
            newNavigationNode = navigationNodeCollection?.Add(createInfo);
            context.Load(newNavigationNode);
            context.ExecuteQuery();

            CopyProperty(newNavigationNodeProperties, newNavigationNode);
            newNavigationNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = newNavigationNode?.Path;
            newNavigationNodeProperties["ClientContext"] = context;
            return newNavigationNodeProperties;
        }

        public Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            try
            {
                //SAAS-26015，在还原到subsite时，context需要传入完整的siteUrl，否则创建出的field title格式不对
                string webUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
                using (AveClientContext context = CreateContext(webUrl))
                {
                    Web web = context.Web;
                    Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                    Field field = null;
                    FieldCollection fields = null;
                    switch (fieldSource)
                    {
                        case "list.fields":
                            List list = web.Lists.GetById(listId);
                            fields = list.Fields;
                            break;
                        case "web.fields":
                            fields = web.Fields;
                            break;
                        //case "web.availablefields":
                        //    field = web.AvailableFields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                        //    break;
                        //case "contenttype.fields":
                        //    string id = contentTypeProp["Id"] as string;
                        //    string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        //    ContentType contentType = GetContentTypeWithoutFields(webServerRelativeUrl, listName, contentTypeSource, id);
                        //    field = contentType.Fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                        //    break;
                        default:
                            break;
                    }
                    if (fields != null)
                    {
                        field = fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                        // the default load can't get the right type.
                        context.Load(fields, fs => fs.IncludeWithDefaultProperties().Where(f => f.InternalName == field.InternalName));
                        context.ExecuteQuery();
                        AssembleSingleFieldProperties(fieldProperties, fields[0]);
                        field = fields[0];
                        if (field is TaxonomyField)
                        {
                            TaxonomyField taxField = field as TaxonomyField;
                            context.Load(fields, fs => fs.IncludeWithDefaultProperties().Where(f => f.Id == taxField.TextField));
                            context.ExecuteQuery();
                            Dictionary<string, object> textFieldProperties = new Dictionary<string, object>();
                            AssembleSingleFieldProperties(textFieldProperties, fields[0]);
                            fieldProperties["TextField" + AveObjectModelConstant.ObjectPropertySuffix] = textFieldProperties;
                        }
                    }
                    return fieldProperties;
                }
            }
            /*review-qlluo*/
            catch (Exception ex)
            {
                mLogger.Error("add field with xml:{0}, web server relative url:{1}, list name:{2}, list id:{3}, add to default view:{4}, op:{5}, fieldSource:{6}, exception:{7}",
                    fieldXml, webServerRelativeUrl, listName, listId, addToDefaultView, op, fieldSource, ex);
                throw;
            }
        }

        private User AddGroupUser(ClientContext context, string groupName, string userName, string userLoginName, string userEmail, int userId)
        {
            User user;
            if (userId > 0)
            {
                mLogger.Info("Add user {0} to group {1} by user id.", userId, groupName);
                var tempUser = context.Web.SiteUsers.GetById(userId);
                user = context.Web.SiteGroups.GetByName(groupName).Users.AddUser(tempUser);
            }
            else
            {
                mLogger.Info("Add user {0}|{1}|{2} to group {3} by user info.", userName, userLoginName, userEmail, groupName);
                var userCreationInfo = new UserCreationInformation()
                {
                    Email = userEmail,
                    LoginName = userLoginName,
                    Title = userName,
                };
                user = context.Web.SiteGroups.GetByName(groupName).Users.Add(userCreationInfo);
            }
            return user;
        }

        public Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            string userName = userProp["Name"] as string;
            string userLoginName = userProp["LoginName"] as string;
            string userEmail = userProp["Email"] as string;
            string userNotes = userProp["Notes"] as string;
            int userId = (int)userProp["ID"];
            var userCreationInfo = new UserCreationInformation()
            {
                Email = userEmail,
                LoginName = userLoginName,
                Title = userName,
            };

            using (var context = CreateContext())
            {
                User user = null;

                switch (source)
                {
                    case "group.users":
                        user = AddGroupUser(context, groupName, userName, userLoginName, userEmail, userId);
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteUsers":
                        user = context.Web.SiteUsers.Add(userCreationInfo);
                        break;
                    case "web.siteAdministrators":
                        user = context.Web.SiteUsers.Add(userCreationInfo);
                        user.IsSiteAdmin = true;
                        user.Update();
                        break;
                    default:
                        break;
                }

                if (user != null)
                {
                    context.Load(user);
                    context.ExecuteQuery();

                    return ConvertUser(user);
                }
            }
            return new Dictionary<string, object>();
        }

        public bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            try
            {
                string adminSiteUrl = string.IsNullOrEmpty(tenantAdminSiteUrl) ? AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, siteCollectionUrl) : tenantAdminSiteUrl;
                using (AveClientContext context = InitClientObject(adminSiteUrl))     //mTokenProvider should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    tenant.SetSiteAdmin(siteCollectionUrl, username, true);
                    context.ExecuteQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to add user to site collection administrators, site collection url : {0}, username : {1}, error message : {2}", siteCollectionUrl, username, e.ToString());
                return false;
            }
        }

        public Dictionary<string, object> AddUserProfile(string accountName)
        {
            throw new NotImplementedException();
        }
        public void AddPersonalSite(string accountName, int lcid)
        {
            throw new NotImplementedException();
        }
        public void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId)
        {
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return mRequestCommon.AddKeyWord(term, startDate, localId, calendarType);
        }
        public string AddSynonm(string term, string synTerm, string terms)
        {
            return mRequestCommon.AddSynonm(term, synTerm, terms);
        }
        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return mRequestCommon.AddBestBet(term, bestBetUrlList, bestBetProp, action);
        }

        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            mRequestCommon.AddSitePolicy(policySchema, siteUrl);
        }

        public void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                Web web = site.RootWeb;
                context.ExecuteQuery();
                //上传solution
                string fileUrl = webServerRelativeUrl.TrimEnd('/') + "/_catalogs/solutions/" + solutionName;
                using (FileStream fileStream = new FileStream(solutionPath, FileMode.Open, FileAccess.Read))
                {
                    //ClientFile.SaveBinaryDirect(context, fileUrl, fileStream, true);
                    AddFileByRestApi(webServerRelativeUrl, fileUrl, fileStream, true);
                }
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileUrl));
                context.Load(file.ListItemAllFields, item => item.Id);
                context.ExecuteQuery();
                //查找solution  激活solution
                //if (tokenProvider.TokenType == Office365.Api.TokenType.Bearer)
                //{
                InstallDesignPackage(new AveDesignPackageInfo(), fileUrl);
                //}
                //else
                //{
                //    AveWebServiceRequest.OperateOnSolution("ACT", mWebUrl, AveUrlUtility.GetServerRelativeUrl(mWebUrl), file.ListItemAllFields.Id, tokenProvider);
                //}
                file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileUrl));
                context.Load(file, f => f.ListItemAllFields);
                context.Load(context.Site.Features, fs => fs.Include(f => f.DefinitionId));
                context.ExecuteQuery();
                Dictionary<string, object> solutionPropiesDir = file.ListItemAllFields.FieldValues;
                //激活solution  同时要激活对应的feature
                if (solutionPropiesDir.ContainsKey("Status") && int.Parse((solutionPropiesDir["Status"] as FieldLookupValue).LookupValue) == 1)
                {
                    bool ativeFeature = false;
                    Guid newActiveSolutionId = solutionPropiesDir.ContainsKey("Status") ? (Guid)solutionPropiesDir["SolutionId"] : new Guid();
                    foreach (AveSolutionFeature feature in packageFeatures)
                    {
                        if (packageSolutionId == newActiveSolutionId && feature.Scope == AveFeatureScope.Site)
                        {
                            if (!site.Features.Select(f => f.DefinitionId == feature.FeatureId).Any())
                            {
                                site.Features.Add(feature.FeatureId, false, FeatureDefinitionScope.Site);
                                ativeFeature = true;
                            }
                        }
                    }
                    if (ativeFeature)
                    {
                        context.ExecuteQuery();
                    }
                }
                //应用激活solution生成的WebTemplate
                web.ApplyWebTemplate(webTemplateName);
                context.ExecuteQuery();
            }
        }
        /// <summary>
        /// Create site collection for sharepoint online
        /// </summary>
        /// <returns>string.Empty if site collection is successfully created, otherwise, error message</returns>
        public Dictionary<string, object> AddSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            Dictionary<string, object> newSiteProperty = new Dictionary<string, object>();
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Tenant tenant = new Tenant(context);
                    var siteCreationProperty = new SiteCreationProperties()
                    {
                        CompatibilityLevel = compatibilityLevel,
                        Lcid = lcid,
                        Owner = owner,
                        Template = template ?? "", //if template is null, use "" otherwise, create site collection would throw unknown error
                        TimeZoneId = timeZoneId,
                        Title = title,
                        Url = url,
                        StorageMaximumLevel = storageQuota,
                        UserCodeMaximumLevel = resourceQuota,
                        UserCodeWarningLevel = Math.Floor(resourceQuota * 0.85),
                        StorageWarningLevel = (long)Math.Floor(storageQuota * 0.85)
                    };
                    mLogger.Info("CreateSiteCollection_SiteCreationProperties.CompatibilityLevel:{0},Lcid:{1},Owner:{2},Template:{3},TimeZoneId:{4},Title:{5},Url:{6},StorageMaximumLevel:{7},UserCodeMaximumLevel:{8},UserCodeWarningLevel:{9},StorageWarningLevel:{10}",
                        siteCreationProperty.CompatibilityLevel,
                        siteCreationProperty.Lcid,
                        siteCreationProperty.Owner,
                        siteCreationProperty.Template,
                        siteCreationProperty.TimeZoneId,
                        siteCreationProperty.Title,
                        siteCreationProperty.Url,
                        siteCreationProperty.StorageMaximumLevel,
                        siteCreationProperty.UserCodeMaximumLevel,
                        siteCreationProperty.UserCodeWarningLevel,
                        siteCreationProperty.StorageWarningLevel);
                    SpoOperation ope = tenant.CreateSite(siteCreationProperty);
                    context.Load(ope);
                    context.ExecuteQuery();
                    mLogger.Info("Begin wait for creating site collection complete {0}", url);
                    WaitForSpoOperationComplete(context, ope);
                    mLogger.Info("Finish wait for creating site collection complete {0}", url);
                    var siteProperties = tenant.GetSitePropertiesByUrl(System.Web.HttpUtility.UrlPathEncode(url), false);
                    siteProperties.RetrieveSiteProperties();
                    context.ExecuteQuery();
                    if (siteProperties.Template.Equals("BLANKINTERNETCONTAINER#0", StringComparison.OrdinalIgnoreCase))
                    {
                        siteProperties.Template = "BLANKINTERNET#0";
                    }
                    CopyProperty(newSiteProperty, siteProperties);
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create site collection, url : {0}, error message : {1}", url, e.ToString());
                newSiteProperty["ErrorMessage"] = e is ServerException ? "ServerException" + e.Message : e.Message; ;
            }

            return newSiteProperty;
        }

        #endregion

        #region  Update
        public Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties, bool isCustomScriptDisabled)
        {
            Dictionary<string, object> webProp = new Dictionary<string, object>();
            if (isCustomScriptDisabled)
            {
                webProp = UpdateWebAndGetPropertiesWhileCustomScriptDisabled(webServerRelativeUrl, webProperties);
            }
            else
            {
                webProp = UpdateWebAndGetProperties(webServerRelativeUrl, webProperties);
            }
            //if (webProperties.ContainsKey("SiteLogoUrl") || webProperties.ContainsKey("SiteLogoDescription") || webProperties.ContainsKey("Name"))
            //{
            //    if (webProperties.ContainsKey("SiteLogoUrl"))
            //    {
            //        webProp["SiteLogoUrl"] = webProperties["SiteLogoUrl"];
            //    }
            //    if (webProperties.ContainsKey("SiteLogoDescription"))
            //    {
            //        webProp["SiteLogoDescription"] = webProperties["SiteLogoDescription"];
            //    }
            //    if (webProperties.ContainsKey("Name"))
            //    {
            //        webProp["Name"] = webProperties["Name"];
            //    }
            //    this.mRequestCommon.UpdateWebLogo(webServerRelativeUrl, webProperties);
            //}
            //此处会更新site setting中的seach and offline availability，如果client api支持此setting后，需要更换此方法。
            if (webProperties.ContainsKey("NoCrawl") && webProperties.ContainsKey("ASPXPageIndexMode") && webProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                mRequestCommon.UpdateWebSearchAndOfflineAvailability(webServerRelativeUrl, webProperties);
                webProp["NoCrawl"] = webProperties["NoCrawl"];
                //由于NoCrawl这个属性是用Web Service还原的，而又因为在AllProperties里面也有一个这样的字段，并且没有更新到webPro里面，所以加到里面。
                //注：在web.NoCrawl里面这个属性是bool类型，而在AllProperties里面这个字段是String类型。
                Dictionary<string, object> tempProp = new Dictionary<string, object>();
                if (webProp.ContainsKey("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    tempProp = webProp["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    if (tempProp.ContainsKey("NoCrawl"))
                    {
                        tempProp["NoCrawl"] = webProperties["NoCrawl"].ToString();
                    }
                }
                webProp["ASPXPageIndexMode"] = webProperties["ASPXPageIndexMode"];
                webProp["ExcludeFromOfflineClient"] = webProperties["ExcludeFromOfflineClient"];
            }
            if (webProperties.ContainsKey("RegionalSettingsChangedProperties"))
            {
                Dictionary<string, object> regionalProp = webProperties["RegionalSettingsChangedProperties"] as Dictionary<string, object>;
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                UpdateRegionalSettings(webServerRelativeUrl, regionalProp);
                newProp = this.GetWebRegionalSetting(webServerRelativeUrl);
                webProp["RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix] = newProp;
            }
            return webProp;
        }
        /// <summary>
        /// 用于更新Group team site的web
        /// </summary>
        private Dictionary<string, object> UpdateWebAndGetPropertiesWhileCustomScriptDisabled(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                bool changed = UpdateWebAccessRequestSetting(context, web, webProperties) > 0;
                AveObjectCopy.UpdateObjectBasicProperties(webProperties, web);

                Dictionary<string, object> webPro = new Dictionary<string, object>();
                if (Convert.ToInt32(webProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || changed)
                {
                    web.Update();
                    webPro = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
                return webPro;
            }
        }

        private int UpdateWebAccessRequestSetting(ClientContext context, Web web, Dictionary<string, object> webProperties)
        {

            int updateCount = 0;
            var UseAccessRequestDefault = webProperties.SafeGetAndRemoveProperty<bool>(WebPropertyNames.UseAccessRequestDefault);
            var RequestAccessEmail = webProperties.SafeGetAndRemoveProperty(WebPropertyNames.RequestAccessEmail);
            var AccessRequestSiteDescription = webProperties.SafeGetAndRemoveProperty(WebPropertyNames.AccessRequestSiteDescription);
            var MembersCanShare = webProperties.SafeGetAndRemoveProperty<bool>(WebPropertyNames.MembersCanShare);
            bool needExecuteQuery = false;
            if (UseAccessRequestDefault.HasValue)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.SetUseAccessRequestDefaultAndUpdate(UseAccessRequestDefault.Value);
                    needExecuteQuery = true;
                }
                //updateCount++;
            }

            if (AccessRequestSiteDescription != null)
            {
                web.SetAccessRequestSiteDescriptionAndUpdate(AccessRequestSiteDescription);
                needExecuteQuery = true;
                //updateCount++;
            }
            //SAAS-35456
            if (needExecuteQuery)
            {
                context.ExecuteQuery();
                mLogger.Info($"UpdateWebAccessRequestSetting_NeedExecuteQuery:[{needExecuteQuery}]");
            }
            
            if (RequestAccessEmail != null)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.RequestAccessEmail = RequestAccessEmail;
                }
                updateCount++;
            }
            if (MembersCanShare.HasValue)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.MembersCanShare = MembersCanShare.Value;
                }
                updateCount++;
            }
            
            return updateCount;
        }

        private Dictionary<string, object> UpdateWebAndGetProperties(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string tempWebUrl = WebAppName.Trim('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                bool changed = false;
                changed = UpdateWebAccessRequestSetting(context, web, webProperties) > 0;
                AveObjectCopy.UpdateObjectBasicProperties(webProperties, web);
                if (webProperties.ContainsKey("UseShared"))
                {
                    web.Navigation.UseShared = Convert.ToBoolean(webProperties["UseShared"]);
                    changed = true;
                }
                if (webProperties.ContainsKey("AllPropertiesDictionary"))
                {
                    Dictionary<string, object> allPropertiesDic = webProperties["AllPropertiesDictionary"] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> pair in allPropertiesDic)
                    {
                        web.AllProperties[pair.Key] = pair.Value;
                    }
                    changed = true;
                }
                if (webProperties.ContainsKey("AssociatedMemberGroup"))
                {
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            Group associatedMemberGroup = web.SiteGroups.GetById((int)webProperties["AssociatedMemberGroup"]);
                            web.AssociatedMemberGroup = associatedMemberGroup;
                            changed = true;
                        }
                        using (scope.StartCatch())
                        { }
                    }
                }
                if (webProperties.ContainsKey("AssociatedOwnerGroup"))
                {
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            Group associatedOwnerGroup = web.SiteGroups.GetById((int)webProperties["AssociatedOwnerGroup"]);
                            web.AssociatedOwnerGroup = associatedOwnerGroup;
                            changed = true;
                        }
                        using (scope.StartCatch())
                        { }
                    }
                }
                if (webProperties.ContainsKey("AssociatedVisitorGroup"))
                {
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            Group associatedVisitorGroup = web.SiteGroups.GetById((int)webProperties["AssociatedVisitorGroup"]);
                            web.AssociatedVisitorGroup = associatedVisitorGroup;
                            changed = true;
                        }
                        using (scope.StartCatch())
                        { }
                    }
                }
                Dictionary<string, object> webPro = new Dictionary<string, object>();
                if (Convert.ToInt32(webProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || changed)
                {
                    web.Update();
                    webPro = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
                return webPro;
            }
        }
        public Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> needAddProperties = new Dictionary<string, object>();
            if (siteProperties.ContainsKey("PortalUrl") || siteProperties.ContainsKey("PortalName"))//SAAS-1299
            {
                Dictionary<string, object> portalProperties = new Dictionary<string, object>();
                if (siteProperties.ContainsKey("PortalName"))
                {
                    portalProperties.Add("PortalName", siteProperties["PortalName"]);
                    needAddProperties.Add("PortalName", siteProperties["PortalName"]);
                    siteProperties.Remove("PortalName");
                }
                if (siteProperties.ContainsKey("PortalUrl"))
                {
                    portalProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    needAddProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    siteProperties.Remove("PortalUrl");
                }
                mRequestCommon.UpdateSitePortal(portalProperties);
            }
            string[] variationsKeys = new string[] { "EnableAutoSpawnPropertyName", "AutoSpawnStopAfterDeletePropertyName", "UpdateWebPartsPropertyName", "SendNotificationEmailPropertyName" };
            if (variationsKeys.Any(key => siteProperties.ContainsKey(key)))
            {
                UpdateVariationsSettings(siteProperties, variationsKeys, ref needAddProperties);
            }
            if (siteProperties.ContainsKey("SyndicationEnabled"))
            {
                mRequestCommon.UpdateSiteRssSetting(Convert.ToBoolean(siteProperties["SyndicationEnabled"]));
                needAddProperties.Add("SyndicationEnabled", siteProperties["SyndicationEnabled"]);
                siteProperties.Remove("SyndicationEnabled");
            }
            if (siteProperties.Count > 0)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties = UpdateSiteProeprties(siteProperties);//原来的updateUser改成UpdateUserProperties
                foreach (string key in needAddProperties.Keys)
                {
                    if (!properties.ContainsKey(key))
                    {
                        properties.Add(key, needAddProperties[key]);
                    }
                }
                return properties;
            }
            return needAddProperties;
        }

        private void UpdateVariationsSettings(Dictionary<string, object> siteProperties, string[] variationsKeys, ref Dictionary<string, object> needAddProperties)
        {
            var relationshipsListIdKey = "_VarRelationshipsListId";
            object listId;

            if (siteProperties.TryGetValue(relationshipsListIdKey, out listId))
            {
                siteProperties.Remove(relationshipsListIdKey);

                try
                {
                    using (var context = CreateContext())
                    {
                        var rootFolder = context.Web.Lists.GetById((Guid)listId).RootFolder;

                        foreach (var key in variationsKeys)
                        {
                            if (siteProperties.ContainsKey(key))
                            {
                                rootFolder.Properties[key] = siteProperties[key].ToString();
                                needAddProperties[key] = siteProperties[key];
                                siteProperties.Remove(key);
                            }
                        }

                        rootFolder.Update();
                        context.ExecuteQuery();
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error("Error while updating site collection:{0} variations settings, relationships list Id:{1}, error:{2}", this.mWebUrl, listId, ex);
                }
            }
        }
        public Dictionary<string, object> UpdateSiteProeprties(Dictionary<string, object> siteProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperty = new Dictionary<string, object>();
                Site site = context.Site;
                AveObjectCopy.UpdateObjectBasicProperties(siteProperties, site);
                if (siteProperties.ContainsKey("Owner"))
                {
                    int userId = (int)siteProperties["Owner"];
                    User user = site.RootWeb.SiteUsers.GetById(userId);
                    site.Owner = user;
                }
                site.RefreshLoad();
                site.RetrieveSite();
                context.ExecuteQuery();
                siteProperty = GetSite();
                return siteProperty;
            }
        }
        public Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            int type = listProperties.ContainsKey("ListType") ? (int)listProperties["ListType"] : -1;
            listProperties.Remove("ListType");
            Dictionary<string, object> properties = null;
            if (type == (int)AveListTemplateType.Survey && tokenProvider.TokenType != TokenType.Bearer)
            {
                properties = mWebServiceRequest.UpdateList(webServerRelativeUrl, listName, listId, listProperties);
            }
            Dictionary<string, object> advancedSettingProp = new Dictionary<string, object>();
            Dictionary<string, object> generalSettings = new Dictionary<string, object>();
            if (CompatibilityLevel >= 15)
            {
                SetAdvancedSetting2013(advancedSettingProp, listProperties);
                SetGeneralSetting2013(generalSettings, listProperties);
            }
            else
            {
                SetAdvancedSetting2010(advancedSettingProp, listProperties);
                SetGeneralSetting2010(generalSettings, listProperties);
            }
            string tempWebUrl = WebAppName.Trim('/') + webServerRelativeUrl;
            using (AveClientContext context = CreateContext(tempWebUrl))
            {
                //code: "list.DocumentTemplateUrl = string.Empty;" works fine in server mode, we should make it work in client mode
                context.ValidateOnClient = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);

                bool needUpdate = false;
                needUpdate |= UpdateUserResourceForUICulture(list.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, listProperties);
                needUpdate |= UpdateUserResourceForUICulture(list.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, listProperties);
                needUpdate |= UpdateListVersionSetting(listProperties, context, list);


                AveObjectCopy.UpdateObjectBasicProperties(listProperties, list);
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                if ((int)(listProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || needUpdate)
                {
                    list.Update();
                    this.LoadList(context, list);
                    AveObjectCopy.GetObjectBasicProperties(newProp, list);
                    CopyUserResourceProperty(newProp, list);
                }
                if (advancedSettingProp.Count > 0)
                {
                    mRequestCommon.UpdateListAdvancedSetting(webServerRelativeUrl, listId, advancedSettingProp);
                }
                if (generalSettings.Count > 0)
                {
                    mRequestCommon.UpdateListGeneralSetting(webServerRelativeUrl, listId, generalSettings);
                }
                return properties != null ? properties : newProp;
            }
        }

        private static bool UpdateListVersionSetting(Dictionary<string, object> listProperties, AveClientContext context, List list)
        {
            const string EnableVersioning = "EnableVersioning";
            const string EnableMinorVersions = "EnableMinorVersions";
            const string EnableModeration = "EnableModeration";
            const string MajorVersionLimit = "MajorVersionLimit";
            const string MajorWithMinorVersionsLimit = "MajorWithMinorVersionsLimit";

            bool? enableVersioning = listProperties.SafeGetAndRemoveProperty<bool>(EnableVersioning);
            bool? enableMinorVersions = listProperties.SafeGetAndRemoveProperty<bool>(EnableMinorVersions);
            bool? enableModeration = listProperties.SafeGetAndRemoveProperty<bool>(EnableModeration);
            int? majorVersionLimit = listProperties.SafeGetAndRemoveProperty<int>(MajorVersionLimit);
            int? majorWithMinorVersionsLimit = listProperties.SafeGetAndRemoveProperty<int>(MajorWithMinorVersionsLimit);

            if (!(enableVersioning.HasValue ||
                enableMinorVersions.HasValue ||
                enableModeration.HasValue ||
                majorVersionLimit.HasValue ||
                majorWithMinorVersionsLimit.HasValue
                ))
            {
                mLogger.Warn("No version setting found in list change properties.");
                return false;
            }
            mLogger.Info("UpdateListVersionSetting:EnalbeVersioning:{0},EnableMinorVersion:{1},EnableModeration:{2},MajorVersionLimit:{3},MajorAndMinorVersionLimit:{4}",
                enableVersioning, enableMinorVersions, enableModeration, majorVersionLimit, majorWithMinorVersionsLimit);
            try
            {
                ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                ExceptionHandlingScope catchScope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    using (scope.StartTry())
                    {
                        SetVersionSetting(list, enableVersioning, enableMinorVersions, enableModeration, majorVersionLimit, majorWithMinorVersionsLimit);
                        list.Update();
                    }
                    using (scope.StartCatch())
                    {
                        using (catchScope.StartScope())
                        {
                            using (catchScope.StartTry())
                            {
                                SetVersionSetting(list, enableVersioning, enableMinorVersions, enableModeration, majorVersionLimit, null);
                                list.Update();
                            }
                            using (catchScope.StartCatch())
                            {
                                SetVersionSetting(list, enableVersioning, enableMinorVersions, enableModeration, null, null);
                                list.Update();
                            }
                        }
                        list.Update();
                    }
                }
                context.ExecuteQuery();

                if (scope.HasException)
                {
                    mLogger.Warn("Update list version setting with exception first time, version limited setting for minor version will is not updated.Error:{0}", scope.ErrorMessage);
                }
                if (catchScope.HasException)
                {
                    mLogger.Warn("Update list version setting with exception first time, version limited setting will is not updated.Error:{0}", catchScope.ErrorMessage);
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while trying to update list version setting.Error:{0}", e);
                return false;
            }
        }

        private static void SetVersionSetting(List list, bool? enableVersioning, bool? enableMinorVersions, bool? enableModeration, int? majorVersionLimit, int? majorWithMinorVersionsLimit)
        {
            if (enableVersioning.HasValue)
            {
                list.EnableVersioning = enableVersioning.Value;
            }
            if (enableMinorVersions.HasValue)
            {
                list.EnableMinorVersions = enableMinorVersions.Value;
            }
            if (enableModeration.HasValue)
            {
                list.EnableModeration = enableModeration.Value;
            }
            if (majorVersionLimit.HasValue)
            {
                list.MajorVersionLimit = majorVersionLimit.Value;
            }
            if (majorWithMinorVersionsLimit.HasValue)
            {
                list.MajorWithMinorVersionsLimit = majorWithMinorVersionsLimit.Value;
            }
        }

        private void SetAdvancedSetting2013(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnableManagedIndexes"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ManagedIndexesSection$ctl02$AllowManagedIndex"] = (bool)listProperties["EnableManagedIndexes"] ? "RadManagedIndexesNo" : "RadManagedIndexesYes";
                listProperties.Remove("EnableManagedIndexes");
            }
            if (listProperties.ContainsKey("DisableGridEditing"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl02$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                listProperties.Remove("DisableGridEditing");
            }
            if (listProperties.ContainsKey("NavigateForFormsPages"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl03$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                listProperties.Remove("NavigateForFormsPages");
            }
            if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl02$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                listProperties.Remove("IsSiteAssetsLibrary");
            }
            if (listProperties.ContainsKey("DefaultItemOpenUseListSetting"))
            {
                if (listProperties.ContainsKey("DefaultItemOpen"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                    listProperties.Remove("DefaultItemOpen");
                }
                else
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (bool)listProperties["DefaultItemOpenUseListSetting"] ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenServerSetting";
                }
                listProperties.Remove("DefaultItemOpenUseListSetting");
            }
            else if (listProperties.ContainsKey("DefaultItemOpen"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                listProperties.Remove("DefaultItemOpen");
            }

            if (listProperties.ContainsKey("SendToLocationName"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                listProperties.Remove("SendToLocationName");
            }
            if (listProperties.ContainsKey("SendToLocationUrl"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl02$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                listProperties.Remove("SendToLocationUrl");
            }
        }
        private void SetAdvancedSetting2010(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnableAssignToEmail"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$TasksIssuesEmailSettingsSection$ctl00$EnableAssignToEmail"] = (bool)listProperties["EnableAssignToEmail"] ? "RadEnableAssigntoEmailYes" : "RadEnableAssigntoEmailNo";
                listProperties.Remove("EnableAssigntoEmail");
            }
            if (listProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowSyncSection$ctl01$AllowSync"] = (bool)listProperties["ExcludeFromOfflineClient"] ? "RadAllowSyncNo" : "RadAllowSyncYes";
                listProperties.Remove("ExcludeFromOfflineClient");
            }
            if (listProperties.ContainsKey("DisableGridEditing"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl01$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                listProperties.Remove("DisableGridEditing");
            }
            if (listProperties.ContainsKey("NavigateForFormsPages"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl02$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                listProperties.Remove("NavigateForFormsPages");
            }
            if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl01$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                listProperties.Remove("IsSiteAssetsLibrary");
            }
            if (listProperties.ContainsKey("EnableAttachments"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentsSection$ctl01$DisableAttachments"] = (bool)listProperties["EnableAttachments"] ? "RadAttachmentsEnabled" : "RadAttachmentsDisabled";
                listProperties.Remove("EnableAttachments");
            }
            if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                listProperties.Remove("DefaultItemOpenUseListSetting");
            }
            else if (listProperties.ContainsKey("DefaultItemOpen"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                listProperties.Remove("DefaultItemOpen");
            }
            if (listProperties.ContainsKey("SendToLocationName"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl00$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                listProperties.Remove("SendToLocationName");
            }
            if (listProperties.ContainsKey("SendToLocationUrl"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                listProperties.Remove("SendToLocationUrl");
            }
            if (listProperties.ContainsKey("ReadSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl08$ReadSecurity"] = listProperties["ReadSecurity"];
                listProperties.Remove("ReadSecurity");
            }
            if (listProperties.ContainsKey("WriteSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$WriteSecurity"] = listProperties["WriteSecurity"];
                listProperties.Remove("WriteSecurity");
            }
        }
        private void SetGeneralSetting2013(Dictionary<string, object> generalSettings, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnablePeopleSelector"))
            {
                generalSettings["ctl00$PlaceHolderMain$EventSection$ctl01$enablePeopleSelector"] = (bool)listProperties["EnablePeopleSelector"] ? "RadEnablePeopleSelectorYes" : "RadEnablePeopleSelectorNo";
                listProperties.Remove("EnablePeopleSelector");
            }
        }
        private void SetGeneralSetting2010(Dictionary<string, object> generalSettings, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnablePeopleSelector"))
            {
                generalSettings["ctl00$PlaceHolderMain$EventSection$ctl00$enablePeopleSelector"] = (bool)listProperties["EnablePeopleSelector"] ? "RadEnablePeopleSelectorYes" : "RadEnablePeopleSelectorNo";
                listProperties.Remove("EnablePeopleSelector");
            }
        }
        public Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, string folderServerRelativeUrl, Dictionary<string, object> folderProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                AveObjectCopy.UpdateObjectBasicProperties(folderProperties, folder);
                if (folderProperties.ContainsKey("UniqueContentTypeOrder"))
                {
                    List<string> uniqueContentTypeOrder = folderProperties["UniqueContentTypeOrder"] as List<string>;
                    List<ContentTypeId> contentTypeOlders = new List<ContentTypeId>();
                    foreach (string ContentTypeId in uniqueContentTypeOrder)
                    {
                        ContentTypeId cti = new ContentTypeId();
                        AveAssemblyUtility.SetFieldValue(cti, "m_stringValue", ContentTypeId);
                        contentTypeOlders.Add(cti);
                    }
                    folder.UniqueContentTypeOrder = contentTypeOlders;
                }
                if (folderProperties.ContainsKey("FolderChangeProperties") && folderProperties["FolderChangeProperties"] is Dictionary<string, object>)
                {
                    SetFolderPropertyValues(folder, folderProperties["FolderChangeProperties"] as Dictionary<string, object>);
                }
                folder.Update();
                Dictionary<string, object> newProp = this.GetFolder(context, webServerRelativeUrl, listName, folderServerRelativeUrl);
                return newProp;
            }
        }

        private void SetFolderPropertyValues(ClientFolder folder, Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, object> tempPair in properties)
            {
                folder.Properties[tempPair.Key] = tempPair.Value;
            }
        }

        public AveStorageMetrics GetFolderStorageMetrics(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                context.Load(folder, f => f.StorageMetrics);
                context.ExecuteQuery();
                return new AveStorageMetrics(folder.StorageMetrics.LastModified, folder.StorageMetrics.TotalFileCount, folder.StorageMetrics.TotalFileStreamSize, folder.StorageMetrics.TotalSize);
            }
        }

        public Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, Dictionary<string, object> viewProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                View view = this.FindView(webServerRelativeUrl, listName, listId, viewId, context);
                if (view != null)
                {
                    if (viewProperties.ContainsKey("DeleteAllFields"))
                    {
                        view.ViewFields.RemoveAll();
                    }
                    if (viewProperties.ContainsKey("AddViewFields"))
                    {
                        List<string> addViewFieldList = viewProperties["AddViewFields"] as List<string>;
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        foreach (string fieldName in addViewFieldList)
                        {
                            if (list.Fields.GetByInternalNameOrTitle(fieldName) != null)
                            {
                                view.ViewFields.Add(fieldName);
                            }
                        }
                    }
                    if (viewProperties.ContainsKey("DeleteViewFields"))
                    {
                        List<string> deleteViewFieldList = viewProperties["DeleteViewFields"] as List<string>;
                        foreach (string fieldName in deleteViewFieldList)
                        {
                            for (int i = 0; i < view.ViewFields.Count; ++i)
                            {
                                if (fieldName.Equals(view.ViewFields[i]))
                                {
                                    view.ViewFields.Remove(fieldName);
                                    break;
                                }
                            }
                        }
                    }
                    if (viewProperties.ContainsKey("MoveFieldTo"))
                    {
                        List<Dictionary<string, object>> moveFieldList = viewProperties["MoveFieldTo"] as List<Dictionary<string, object>>;
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        foreach (var moveField in moveFieldList)
                        {
                            view.ViewFields.MoveFieldTo((string)moveField["fieldName"], (int)moveField["index"]);
                        }
                    }
                    AveObjectCopy.UpdateObjectBasicProperties(viewProperties, view);
                    view.Update();
                    context.Load(view);
                    context.Load(view, v => v.ViewFields);
                    context.ExecuteQuery();
                    Dictionary<string, object> viewProp = new Dictionary<string, object>();
                    AssembleViewProperties(viewProp, view, webServerRelativeUrl);
                    return viewProp;
                    //return null;
                }
                else
                {
                    return null;
                }
            }
        }
        public void SystemUpdateItemForRecords(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties, bool isFolder = false)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> kv in itemFieldValues)
                    {
                        item[kv.Key] = kv.Value;
                    }
                }
                if (isFolder)
                {
                    context.ExecutingWebRequest += ClearUserAgent;
                }
                item.SystemUpdate();//for records.
                context.Load(item);
                context.ExecuteQuery();
            }
        }

        public void SystemUpdateForProps(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                foreach (var (key, value) in itemProperties)
                {
                    item.Properties[key] = value;
                }
                item.SystemUpdate();
                context.Load(item);
                context.ExecuteQuery();
            }
        }

        private void ClearUserAgent(object sender, WebRequestEventArgs e)
        {
            var request = (e.WebRequestExecutor as AvePoint.ObjectModel.ClientOM.AveWebRequestExecutor)?.Request;
            if(request != null)
            {
                request.UserAgent = null;
            }
        }
        public Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                bool needRefreshContext = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                if (itemProperties.ContainsKey("Ave_ModerationInformation"))
                {
                    Dictionary<string, object> moderationChangedProp = itemProperties["Ave_ModerationInformation"] as Dictionary<string, object>;
                    if (moderationChangedProp.ContainsKey("Comment"))
                    {
                        item["_ModerationComments"] = moderationChangedProp["Comment"] as string;
                        needRefreshContext = true;
                    }
                    else if (moderationChangedProp.ContainsKey("Status"))
                    {
                        item["_ModerationStatus"] = (int)moderationChangedProp["Status"];
                        needRefreshContext = true;
                    }
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
                    string updateMethod = itemProperties[AveObjectModelConstant.UpdateMethodName] as string;
                    switch (updateMethod)
                    {
                        case "Update":
                            foreach (KeyValuePair<string, object> kv in itemFieldValues)
                            {
                                item[kv.Key] = kv.Value;
                            }
                            item.Update();
                            context.Load(item);
                            context.ExecuteQuery();
                            needRefreshContext = false;
                            break;
                        case "SystemUpdate":

                            item = InternUpdate(list, itemId, itemProperties);
                            context.Load(item);
                            context.ExecuteQuery();
                            needRefreshContext = false;
                            break;
                        default:
                            break;
                    }
                }
                if (needRefreshContext)
                {
                    CreateContext();
                    context.Load(item);
                    context.ExecuteQuery();
                }
                GetItemDic(returnInfo, item);
                return returnInfo;
            }
        }
        public Dictionary<string, object> UpdateAudit(int compatibilityLevel, Dictionary<string, object> needUpdateProperties)
        {
            //if (compatibilityLevel == 14)
            //{
            //    return mWebServiceRequest.UpdateAudit(compatibilityLevel, needUpdateProperties);
            //}
            using (AveClientContext context = CreateContext())
            {
                var site = context.Site;
                var audit = site.Audit;
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateProperties, audit);
                audit.Update();
                context.ExecuteQuery();
                return needUpdateProperties;
            }
        }
        public Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Group group = null;
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        group = web.SiteGroups.GetById(id);
                        StringBuilder sb = new StringBuilder();
                        foreach (KeyValuePair<string, object> kv in groupProperties)
                        {
                            if (kv.Value != null)
                            {
                                sb.AppendLine(kv.Key + ":" + kv.Value.ToString());
                            }
                            else
                            {
                                sb.AppendLine(kv.Key + ":" + "is null");
                            }
                        }
                        mLogger.Info("Need update properties.{0}", sb);

                        AveObjectCopy.UpdateObjectBasicProperties(groupProperties, group);

                        if (groupProperties.ContainsKey("OwnerId") && groupProperties.ContainsKey("OwnerType"))
                        {
                            if (groupProperties["OwnerType"].ToString().Equals("group"))
                            {
                                Group ownerGroup = web.SiteGroups.GetById((int)groupProperties["OwnerId"]);
                                group.Owner = ownerGroup;
                            }
                            else if (groupProperties.ContainsKey("OwnerLoginName"))
                            {
                                User user = web.EnsureUser(groupProperties["OwnerLoginName"].ToString());
                                group.Owner = user;
                            }
                        }
                        mLogger.Info("Update properties");
                        group.Update();
                        context.Load(group);
                        //context.ExecuteQuery();
                        mLogger.Info("Update properties finished");
                    }
                    using (excepScope.StartCatch())
                    {
                        if (excepScope.HasException)
                        {
                            mLogger.Info("Update properties error.ServerErrorCode:{0},Error Message:{1},ServerStackTrace:{2},", excepScope.ServerErrorCode, excepScope.ErrorMessage, excepScope.ServerStackTrace);
                            if (groupProperties.ContainsKey("OnlyAllowMembersViewMembership") && group.OnlyAllowMembersViewMembership != (bool)groupProperties["OnlyAllowMembersViewMembership"])
                            {
                                group.OnlyAllowMembersViewMembership = (bool)groupProperties["OnlyAllowMembersViewMembership"];
                                group.Update();
                            }
                        }
                        context.Load(group);
                    }
                }

                Dictionary<string, object> groupPro = new Dictionary<string, object>();
                groupPro = GetGroupProperties(context, group, false);

                return groupPro;
            }
        }
        public Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties)
        {
            ClientContext context = CreateContext();
            if (navigationNodeProperties != null && navigationNodeProperties.ContainsKey("ClientContext"))
            {
                context = navigationNodeProperties["ClientContext"] as ClientContext;
            }

            Dictionary<string, object> NavigationProperties = new Dictionary<string, object>();
            ArgumentCheck.CheckNotNull(navigationNodeProperties);
            NavigationNode navigationNode = new NavigationNode(context, navigationNodeProperties?["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
            AveObjectCopy.UpdateObjectBasicProperties(needUpdateProperties, navigationNode);
            navigationNode.Update();
            context.Load(navigationNode);
            context.ExecuteQuery();

            CopyProperty(NavigationProperties, navigationNode);
            NavigationProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = navigationNode.Path;
            NavigationProperties["ClientContext"] = context;
            return NavigationProperties;
        }
        public Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, web, roleAssignmentProperties);
                        break;
                    case "list.roleAssignments":
                        List list = web.Lists.GetById(listId);
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, list, roleAssignmentProperties);
                        break;
                    case "item.roleAssignments":
                        List list1 = web.Lists.GetById(listId);
                        ListItem listItem = list1.GetItemById(itemId);
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, listItem, roleAssignmentProperties);
                        break;
                }
                return roleAssignmentProperties;
            }
        }
        public Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleDefinitionProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleDefinition roleDefinition = web.RoleDefinitions.GetById(id);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateRoledefinitionProperties, roleDefinition);//, new string[] { "BasePermissions" });
                if (needUpdateRoledefinitionProperties.ContainsKey("BasePermissions"))
                {
                    roleDefinition.BasePermissions = ConvertULongToBasePerm((ulong)needUpdateRoledefinitionProperties["BasePermissions"]);
                }
                roleDefinition.Update();
                context.Load(roleDefinition);
                context.ExecuteQuery();
                AssembleRoleDefinitionProperties(roleDefinitionProperties, webServerRelativeUrl, roleDefinition);
                return roleDefinitionProperties;
            }
        }
        public Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties)
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames)
        {
            //mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties, supportedResourceCultureNames);
            var ctProperties = InternalUpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties, supportedResourceCultureNames);

            string schemaXmlWithRT = null;
            object schemaXml = null;
            if (ctProperties.TryGetValue("SchemaXmlWithResourceTokens", out schemaXml))
            {
                schemaXmlWithRT = schemaXml as string;
            }

            var newProperties = UpdateContentType(webServerRelativeUrl, listName, contentTypeId, schemaXmlWithRT, updateChildren, needUpdateContentTypeProperties, supportedResourceCultureNames);

            if (newProperties != null && newProperties.Count > 0)
            {
                return newProperties;
            }

            return ctProperties;
        }

        public Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, bool isReadOnly, List<string> supportedResourceCultureNames)
        {
            bool changedReadOnly = false;
            if (isReadOnly)
            {
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                contentTypeProperties["ReadOnly"] = false;
                InternalUpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, contentTypeProperties, supportedResourceCultureNames);
                changedReadOnly = true;
            }
            //SAAS-9309 update seal contentType with client api first
            Dictionary<string, object> ctProperties = InternalUpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties, supportedResourceCultureNames);
            Dictionary<string, object> newProperties = null;
            string schemaXmlWithRT = null;
            object schemaXml = null;
            if (ctProperties.TryGetValue("SchemaXmlWithResourceTokens", out schemaXml))
            {
                schemaXmlWithRT = schemaXml as string;
            }
            //Documentset viewid xmldocument 
            bool isDocumentSet = AveSPDocumentSet.IsDocumentSet(contentTypeId);
            //SAAS-37124
            bool hasInformationManagementPolicy = CtHasInformationManagementPolicy(needUpdateContentTypeProperties);
            mLogger.Info("Update documentset contenttype xmldocument.ContentTypeId:{0},IsDocumentSet:{1},TokenType:{2},HasInformationManagementPolicy:{3}", contentTypeId, isDocumentSet, tokenProvider.TokenType, hasInformationManagementPolicy);
            //if ((isDocumentSet || hasInformationManagementPolicy) && tokenProvider.TokenType != TokenType.Bearer)
            //{

            //    if (!string.IsNullOrEmpty(listName))
            //    {
            //        mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties, supportedResourceCultureNames);
            //    }
            //    else
            //    {
            //        mWebServiceRequest.UpdateContentType(webServerRelativeUrl, contentTypeId, needUpdateContentTypeProperties); //SAAS-9171 增加web下的ContentType 的更新
            //    }
            //}
            //else
            //{
                newProperties = UpdateContentType(webServerRelativeUrl, listName, contentTypeId, schemaXmlWithRT, updateChildren, needUpdateContentTypeProperties, supportedResourceCultureNames);
            //}
            if (newProperties != null && newProperties.Count > 0)
            {
                return newProperties;
            }
            return ctProperties;
        }

        /// <summary>
        /// check content type has Information Management Policy Setting
        /// </summary>
        /// <param name="needUpdateContentTypeProperties"></param>
        /// <returns></returns>
        private bool CtHasInformationManagementPolicy(Dictionary<string, object> needUpdateContentTypeProperties)
        {
            var doesContainsKey =
                needUpdateContentTypeProperties != null &&
                needUpdateContentTypeProperties.Any() &&
                needUpdateContentTypeProperties.ContainsKey("AddedDocuments") &&
                needUpdateContentTypeProperties["AddedDocuments"] != null &&
                needUpdateContentTypeProperties["AddedDocuments"] is Dictionary<string, string>;
            if (doesContainsKey)
            {
                var dic = needUpdateContentTypeProperties["AddedDocuments"] as Dictionary<string, string>;
                if (dic.ContainsKey("office.server.policy"))
                {
                    return true;
                }
            }
            return false;
        }

        /// only support xmldocuments and other properties, future support the field link and all settings TODO_LONG
        private Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, string contentTypeId, string schemaXml, bool updateChildren, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames)
        {
            Dictionary<string, object> ctProperties = new Dictionary<string, object>();

            if (needUpdateContentTypeProperties.ContainsKey("NewDocumentControl") ||
                needUpdateContentTypeProperties.ContainsKey("RequireClientRenderingOnNew") ||
                needUpdateContentTypeProperties.ContainsKey("DeletedDocuments") ||
                needUpdateContentTypeProperties.ContainsKey("AddedDocuments"))
            {

                if (string.IsNullOrEmpty(schemaXml))
                {
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        ContentType contentType = null;
                        if (string.IsNullOrEmpty(listName))
                        {
                            contentType = web.ContentTypes.GetById(contentTypeId);
                        }
                        else
                        {
                            contentType = web.Lists.GetByTitle(listName).ContentTypes.GetById(contentTypeId);
                        }

                        context.Load(contentType, c => c.SchemaXml, c => c.SchemaXmlWithResourceTokens);
                        context.ExecuteQuery();
                        if (!string.IsNullOrEmpty(contentType.SchemaXmlWithResourceTokens))
                        {
                            schemaXml = contentType.SchemaXmlWithResourceTokens;
                        }
                        else
                        {
                            schemaXml = contentType.SchemaXml;
                        }
                    }
                }

                var document = new XmlDocument();
                document.LoadXml(schemaXml);

                var changed = false;

                object keyValue;

                if (needUpdateContentTypeProperties.TryGetValue("NewDocumentControl", out keyValue))
                {
                    document.DocumentElement.SetAttribute("NewDocumentControl", keyValue != null ? keyValue.ToString() : string.Empty);
                    changed = true;
                }

                if (needUpdateContentTypeProperties.TryGetValue("RequireClientRenderingOnNew", out keyValue))
                {
                    document.DocumentElement.SetAttribute("RequireClientRenderingOnNew", keyValue != null ? keyValue.ToString() : "false");
                    changed = true;
                }

                var xmlDocuments = document.SelectSingleNode("/ContentType/XmlDocuments");

                if (needUpdateContentTypeProperties.TryGetValue("DeletedDocuments", out keyValue))
                {
                    if (xmlDocuments != null)
                    {
                        var list = keyValue as List<string>;
                        if (list != null)
                        {
                            List<XmlNode> deletedNodes = new List<XmlNode>();
                            foreach (XmlNode node in xmlDocuments.ChildNodes)
                            {
                                var namespaceUri = node.Attributes["NamespaceURI"].Value;
                                if (list.Contains(namespaceUri))
                                {
                                    deletedNodes.Add(node);
                                }
                            }

                            foreach (var node in deletedNodes)
                            {
                                changed = true;
                                xmlDocuments.RemoveChild(node);
                            }
                        }
                    }
                }

                if (needUpdateContentTypeProperties.TryGetValue("AddedDocuments", out keyValue))
                {
                    var list = keyValue as Dictionary<string, string>;

                    if (list != null && list.Count > 0)
                    {
                        Dictionary<string, XmlNode> nodeMapping = null;
                        if (xmlDocuments == null)
                        {
                            xmlDocuments = document.CreateElement("XmlDocuments");
                            document.DocumentElement.AppendChild(xmlDocuments);
                        }
                        else if (xmlDocuments.ChildNodes.Count > 0)
                        {
                            nodeMapping = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
                            foreach (XmlNode node in xmlDocuments.ChildNodes)
                            {
                                nodeMapping[node.Attributes["NamespaceURI"].Value] = node;
                            }
                        }

                        foreach (var item in list)
                        {
                            XmlNode node;
                            if (nodeMapping != null && nodeMapping.TryGetValue(item.Key, out node))
                            {
                                xmlDocuments.RemoveChild(node);
                            }

                            var xmlDocument = document.CreateElement("XmlDocument");
                            xmlDocument.SetAttribute("NamespaceURI", item.Key);
                            xmlDocument.InnerXml = item.Value;
                            xmlDocuments.AppendChild(xmlDocument);
                        }

                        changed = true;
                    }
                }

                if (changed)
                {
                    schemaXml = document.OuterXml;

                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        ContentType contentType = null;
                        if (string.IsNullOrEmpty(listName))
                        {
                            contentType = web.ContentTypes.GetById(contentTypeId);
                        }
                        else
                        {
                            contentType = web.Lists.GetByTitle(listName).ContentTypes.GetById(contentTypeId);
                        }

                        contentType.SchemaXmlWithResourceTokens = schemaXml;
                        contentType.Update(updateChildren);
                        context.Load(contentType);
                        context.Load(contentType, c => c.Parent);
                        context.Load(contentType, c => c.SchemaXml);
                        context.Load(contentType, c => c.SchemaXmlWithResourceTokens);
                        context.Load(contentType, c => c.WorkflowAssociations);

                        #region load culture info
                        if (supportedResourceCultureNames != null && supportedResourceCultureNames.Count > 0)
                        {
                            try
                            {
                                foreach (var languageName in supportedResourceCultureNames)
                                {
                                    contentType.NameResource.GetValueForUICulture(languageName);
                                    contentType.DescriptionResource.GetValueForUICulture(languageName);
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Error("Error while query ContentType user resource, web url:{0}, contentType id:{1}, list name:{2}, error:{3}", webServerRelativeUrl, contentTypeId, listName, e);
                            }
                        }
                        #endregion

                        context.ExecuteQuery();
                        AssembleSingleContentTypeProperties(ctProperties, contentType);
                    }
                }
            }

            return ctProperties;
        }

        private Dictionary<string, object> InternalUpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                FieldCollection fields = null;
                Field field = null;
                bool changed = false;
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                //SAAS-6766 update contentType documentTemplate exception(like Asp Net Master Page) 
                string documentTemplate = string.Empty;
                if (needUpdateContentTypeProperties.ContainsKey("DocumentTemplate"))
                {
                    documentTemplate = needUpdateContentTypeProperties["DocumentTemplate"] as string;
                    needUpdateContentTypeProperties.Remove("DocumentTemplate");
                }
                ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    using (scope.StartTry())
                    {
                        if (!string.IsNullOrEmpty(documentTemplate))
                        {
                            contentType.DocumentTemplate = documentTemplate;
                            contentType.Update(updateChildren);
                            changed = true;
                        }
                    }
                    using (scope.StartCatch())
                    {
                    }
                }


                bool hasUserResource = UpdateUserResourceForUICulture(contentType.NameResource, AveUserResourceConstants.NAME_RESOUCE, needUpdateContentTypeProperties, false);
                hasUserResource |= UpdateUserResourceForUICulture(contentType.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, needUpdateContentTypeProperties, false);

                AveObjectCopy.UpdateObjectBasicProperties(needUpdateContentTypeProperties, contentType);

                int objectPathCount = 0;
                //Dictionary<Field, Dictionary<string, object>> hiddenTextFieldProperties = new Dictionary<Field, Dictionary<string, object>>();
                Dictionary<string, Field> hiddenTextFieldCaches = new Dictionary<string, Field>();
                Dictionary<string, Dictionary<string, object>> hiddenTextFieldPropertyCaches = new Dictionary<string, Dictionary<string, object>>();
                List<string> taxonomyFieldIds = new List<string>();
                List<Dictionary<string, object>> addfieldlinks = null;
                List list = null;
                if (needUpdateContentTypeProperties.ContainsKey("AddFieldLink"))
                {
                    addfieldlinks = needUpdateContentTypeProperties["AddFieldLink"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> fieldLinkProp in addfieldlinks)
                    {
                        //这俩个id是TaxKeywordTaxHTField，Taxonomy Catch All Column,  related to  Enterprise Keywords这个field  直接skip.
                        if (fieldLinkProp["FieldId"].ToString().ToLower() == "1390a86a-23da-45f0-8efe-ef36edadfb39" ||
                            fieldLinkProp["FieldId"].ToString().ToLower() == "f3b0adf9-c1a2-4b02-920d-943fba4b3611")
                        {
                            continue;
                        }
                        bool isNew = fieldLinkProp.ContainsKey("IsNew") ? (bool)fieldLinkProp["IsNew"] : false;
                        if (isNew)
                        {
                            switch (fieldLinkProp["fieldSource"].ToString())
                            {
                                case "web.fields":
                                    fields = web.Fields;
                                    break;
                                case "web.availableFields":
                                    fields = web.AvailableFields;
                                    break;
                                case "list.fields":
                                    list = web.Lists.GetById(listId);
                                    fields = list.Fields;
                                    break;
                                default:
                                    break;
                            }
                            string fieldId = fieldLinkProp["FieldId"].ToString();
                            field = fields?.GetById(new Guid(fieldId));
                            //判断internalName为guid的，暂不处理。
                            if (!string.IsNullOrEmpty(fieldLinkProp["FieldInternalName"] as string))
                            {
                                string tempName = fieldLinkProp["FieldInternalName"].ToString();
                                if ((tempName[0] >= 'g') && (tempName[0] <= 'p'))
                                {
                                    char ch = tempName[0];
                                    tempName = new System.Text.RegularExpressions.Regex(ch.ToString()).Replace(tempName, ((char)(tempName[0] - '7')).ToString(), 1);
                                }
                                if (Wrapper.Common.AveSPUtility.IsGuid(tempName))
                                {
                                    hiddenTextFieldCaches.Add(fieldId, field);
                                    hiddenTextFieldPropertyCaches.Add(fieldId, fieldLinkProp);
                                    continue;
                                }
                            }
                            if (string.Compare(fieldLinkProp["FieldTypeAsString"] as string, "TaxonomyFieldType") == 0 ||
                                 string.Compare(fieldLinkProp["FieldTypeAsString"] as string, "TaxonomyFieldTypeMulti") == 0 ||
                                !fieldLinkProp.ContainsKey("FieldTypeAsString"))
                            {
                                string tempId = (new Guid(fieldId)).ToString("N");
                                if ((tempId[0] >= '0') && (tempId[0] <= '9'))
                                {
                                    char ch = tempId[0];
                                    tempId = new System.Text.RegularExpressions.Regex(ch.ToString()).Replace(tempId, ((char)(tempId[0] + '7')).ToString(), 1);
                                }
                                taxonomyFieldIds.Add(tempId);
                            }
                        }
                        else
                        {
                            ContentType newContentType = GetContentTypeWithoutFields(context, AveUrlUtility.GetServerRelativeUrl(fieldLinkProp["site"].ToString()), fieldLinkProp["ParentList"] == null ? null : fieldLinkProp["ParentList"].ToString(), fieldLinkProp.ContainsKey("ParentListId") ? Guid.Empty : (Guid)fieldLinkProp["ParentListId"], fieldLinkProp["contentTypeSource"].ToString(), fieldLinkProp["Id"].ToString());
                            context.Load(newContentType, c => c.FieldLinks, c => c.Fields);
                            field = newContentType.Fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        objectPathCount++;
                        AddContentTypeFieldLink(contentType, field, fieldLinkProp);
                        changed = true;
                        if (objectPathCount >= 50) //otherwise 'use too many resources' exception will be throwed
                        {
                            contentType.Update(updateChildren);
                            context.ExecuteQuery();
                            objectPathCount = 0;
                        }
                    }
                }
                foreach (KeyValuePair<string, Dictionary<string, object>> taxField in hiddenTextFieldPropertyCaches)
                {
                    if (taxonomyFieldIds.Contains(taxField.Value["FieldInternalName"].ToString()))
                    {
                        continue;
                    }
                    AddContentTypeFieldLink(contentType, hiddenTextFieldCaches[taxField.Key], taxField.Value);
                }

                ExceptionHandlingScope deleteFieldLinkScope = null;
                if (needUpdateContentTypeProperties.ContainsKey("DeleteFieldLink"))
                {
                    contentType.Update(updateChildren);
                    context.ExecuteQuery();
                    deleteFieldLinkScope = new ExceptionHandlingScope(context);
                    using (deleteFieldLinkScope.StartScope())
                    {
                        using (deleteFieldLinkScope.StartTry())
                        {
                            foreach (Guid fieldId in needUpdateContentTypeProperties["DeleteFieldLink"] as List<Guid>)
                            {
                                foreach (FieldLink fieldLink in contentType.FieldLinks)
                                {
                                    if (fieldLink.Id == fieldId)
                                    {
                                        fieldLink.DeleteObject();
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                        }
                        using (deleteFieldLinkScope.StartCatch())
                        {

                        }
                    }
                }
                if (needUpdateContentTypeProperties.ContainsKey("UpdateFieldLinks"))
                {
                    Dictionary<Guid, Dictionary<string, object>> fieldLinks = needUpdateContentTypeProperties["UpdateFieldLinks"] as Dictionary<Guid, Dictionary<string, object>>;
                    foreach (KeyValuePair<Guid, Dictionary<string, object>> fieldlinkInterator in fieldLinks)
                    {
                        FieldLink fieldLink = contentType.FieldLinks.GetById(fieldlinkInterator.Key);
                        if (fieldlinkInterator.Value.ContainsKey("Hidden"))
                        {
                            fieldLink.Hidden = Convert.ToBoolean(fieldlinkInterator.Value["Hidden"]);
                            changed = true;
                        }
                        if (fieldlinkInterator.Value.ContainsKey("Required"))
                        {
                            fieldLink.Required = Convert.ToBoolean(fieldlinkInterator.Value["Required"]);
                            changed = true;
                        }
                        if (fieldlinkInterator.Value.ContainsKey("ReadOnly"))
                        {
                            fieldLink.ReadOnly = Convert.ToBoolean(fieldlinkInterator.Value["ReadOnly"]);
                            changed = true;
                        }
                    }
                }

                string[] internalNames = null;
                if (needUpdateContentTypeProperties.ContainsKey("Reorder"))
                {
                    internalNames = (needUpdateContentTypeProperties["Reorder"] as List<string>).ToArray();
                    changed = true;
                }

                int propertiesCount = Convert.ToInt32(needUpdateContentTypeProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]);
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                if (changed || propertiesCount > 0 || hasUserResource)
                {
                    if (internalNames != null)
                    {
                        contentType.FieldLinks.Reorder(internalNames);
                    }

                    contentType.Update(updateChildren);
                    context.Load(contentType);
                    context.Load(contentType, c => c.Parent);
                    context.Load(contentType, c => c.SchemaXml);
                    context.Load(contentType, c => c.SchemaXmlWithResourceTokens);
                    context.Load(contentType, c => c.WorkflowAssociations);

                    #region load culture info
                    if (supportedResourceCultureNames != null && supportedResourceCultureNames.Count > 0)
                    {
                        try
                        {
                            foreach (var languageName in supportedResourceCultureNames)
                            {
                                contentType.NameResource.GetValueForUICulture(languageName);
                                contentType.DescriptionResource.GetValueForUICulture(languageName);
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("Error while query ContentType user resource, web url:{0}, contentType source:{1}, list id:{2}, error:{3}", webServerRelativeUrl, contentTypeSource, listId, e);
                        }
                    }
                    #endregion

                    context.ExecuteQuery();

                    this.AssembleSingleContentTypeProperties(newProp, contentType);
                }
                if (scope.HasException)
                {
                    mLogger.Warn("update contentType's documentTemplate failed. contentType Id:{0}  {1}", contentTypeId, scope.ErrorMessage);
                }

                if (deleteFieldLinkScope != null && deleteFieldLinkScope.HasException)
                {
                    mLogger.Error("Delete field link of content type:{0} failed:{1}", contentTypeId, deleteFieldLinkScope.ExtractException());
                }
                KeepSystemBuiltInColumnReadonlyProperty(list, addfieldlinks);
                return newProp;
            }
        }

        //由于client api无法keep fieldlink的readonly属性，所以如果源端的contenttype上带着下面4列的fieldlink并且hidden是false，readonly是true，转到目的端后由于readonly没有keep住，将导致editform,viewform,newform显示有问题
        private void KeepSystemBuiltInColumnReadonlyProperty(List list, List<Dictionary<string, object>> fieldLinks)
        {
            if (list != null && fieldLinks != null)
            {
                try
                {
                    HashSet<string> builtinColumns = new HashSet<string>();
                    //author,editor,created,modified,uiversion,versionstring
                    builtinColumns.Add("7841bf41-43d0-4434-9f50-a673baef7631");
                    builtinColumns.Add("28cf69c5-fa48-462a-b5cd-27b6f9d2bd5f");
                    builtinColumns.Add("d31655d1-1d5b-4511-95a1-7a09e9b75bf2");
                    builtinColumns.Add("8c06beca-0777-48f7-91c7-6da68bc07b69");
                    builtinColumns.Add("1df5e554-ec7e-46a6-901d-d85a3881cb18");
                    builtinColumns.Add("dce8262a-3ae9-45aa-aab4-83bd75fb738a");
                    bool changed = false;
                    foreach (Dictionary<string, object> fieldLinkProp in fieldLinks)
                    {
                        if (builtinColumns.Contains(fieldLinkProp["FieldId"].ToString().ToLower()))
                        {
                            Field field = list.Fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                            field.ReadOnlyField = true;
                            field.Update();
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        list.Context.ExecuteQuery();
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("failed to keep system builtin column due to: {0}", e.ToString());
                }
            }
        }

        public void AddContentTypeFieldLink(ContentType contentType, Field field, Dictionary<string, object> fieldLinkProp)
        {
            FieldLinkCreationInformation Info = new FieldLinkCreationInformation();
            Info.Field = field;
            contentType.FieldLinks.Add(Info);
            int fieldLinksCount = contentType.FieldLinks.Count;
            FieldLink fieldLink = contentType.FieldLinks[fieldLinksCount - 1];
            if (fieldLinkProp.ContainsKey("Hidden"))
            {
                fieldLink.Hidden = bool.Parse(fieldLinkProp["Hidden"].ToString());
            }
            if (fieldLinkProp.ContainsKey("Required"))
            {
                fieldLink.Required = bool.Parse(fieldLinkProp["Required"].ToString());
            }
        }

        public Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Dictionary<string, object> eventReceiverProperties = new Dictionary<string, object>();
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    EventReceiverDefinition eventReceiverDefinition = null;
                    switch (eventReceiverDefSource)
                    {
                        case "web.eventReceivers":
                            eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                            break;
                        case "list.eventReceivers":
                            List list = web.Lists.GetById(listId);
                            eventReceiverDefinition = list.EventReceivers.GetById(eventReceiverDefId);
                            break;
                        default:
                            eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                            break;
                    }

                    if (eventReceiverDefinition != null)
                    {
                        AveObjectCopy.UpdateObjectBasicProperties(needUpdateEventReceiverProperties, eventReceiverDefinition);
                        eventReceiverDefinition.Update();
                        context.Load(eventReceiverDefinition);
                        context.ExecuteQuery();
                        AveObjectCopy.GetObjectBasicProperties(eventReceiverProperties, eventReceiverDefinition);
                    }
                    return eventReceiverProperties;
                }
            }
            /*review-qlluo*/
            catch (Exception e)
            {
                mLogger.Error("update event receiver with web url:{0}, list url:{1}, list title:{2}, listId:{3}, source:{4}, event receiver Id:{5} failed:{6}",
                    webServerRelativeUrl, listServerRealtiveUrl, listTitle, listId, eventReceiverDefSource, eventReceiverDefId, e);
                throw;
            }
        }

        public Dictionary<string, object> UpdatePropertyBag(string webServerRelativeUrl, string propertyBagSource, Guid alertId, Dictionary<string, object> needUpdateProperties)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        web.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        List list = web.Lists.GetById(listId);
                        list.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        List list1 = web.Lists.GetById(listId);
                        ListItem listItem = list1.GetItemById(itemId);
                        listItem.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;
                }
                context.Load(roleAssignmentCol, i => i.Include(r => r.PrincipalId, r => r.RoleDefinitionBindings.Include(b => b.Id)));
                //context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(role => role.RoleDefinitionBindings, role => role.Member));
                context.ExecuteQuery();
                AssembleRoleAssignmetsProperites(roleAssignmentsProperties, roleAssignmentCol);
                return roleAssignmentsProperties;
            }
        }

        public Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        web.ResetRoleInheritance();
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        List list = web.Lists.GetById(listId);
                        list.ResetRoleInheritance();
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        List list1 = web.Lists.GetById(listId);
                        ListItem listItem = list1.GetItemById(itemId);
                        listItem.ResetRoleInheritance();
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;
                }
                context.Load(roleAssignmentCol, i => i.Include(r => r.PrincipalId, r => r.RoleDefinitionBindings.Include(b => b.Id)));
                //context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(r => r.RoleDefinitionBindings, r => r.Member));
                context.ExecuteQuery();
                AssembleRoleAssignmetsProperites(roleAssignmentsProperties, roleAssignmentCol);
                return roleAssignmentsProperties;
            }
        }

        public Dictionary<string, object> BreakRoleDefinitionInheritance(string webServerRelativeUrl, bool copyRoleDefinitions, bool keepRoleAssignments)
        {
            throw new NotImplementedException();
        }
        public void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field, int index)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.MoveFieldTo(field, index);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                context.Load(file, f => f.ListItemAllFields.Id);
                context.ExecuteQuery();
                ListItem listItem = file.ListItemAllFields.ParentList.GetItemById(file.ListItemAllFields.Id);
                listItem["_ModerationStatus"] = (int)AveModerationStatusType.Approved;
                listItem["_ModerationComments"] = comment;
                listItem.Update();
                context.Load(listItem);
                context.ExecuteQuery();
            }
        }
        public void CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.CheckIn(comment, (CheckinType)checkinType);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public void CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.CheckOut();
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.CopyTo(strNewUrl, bOverWrite);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.MoveTo(strNewUrl, (MoveOperations)flags);
                context.Load(file);
                context.ExecuteQuery();
            }
        }

        public void MoveToKeepEditor(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, string editor, DateTime modified, int flags)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));

                ResourcePath targetPath = ResourcePath.FromDecodedUrl(strNewUrl);
                file.MoveToUsingPath(targetPath, (MoveOperations)flags); // RetainEditorAndModifiedOnMove flag not working with rename
                
                context.Load(file, f => f.ListItemAllFields);
                context.ExecuteQuery();

                file.ListItemAllFields["Editor"] = editor;
                file.ListItemAllFields["Modified"] = modified;
                file.ListItemAllFields.UpdateOverwriteVersion();

                context.ExecuteQuery();
            }
        }

        public void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file)
        {
            //using (AveClientContext context = CreateContext())
            //{
            //    Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, fileServerRelativeUrl, file, true);
            //}
            AddFileByRestApi(webServerRelativeUrl, fileServerRelativeUrl, file, true);
        }
        public void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file)
        {
            using (AveClientContext context = CreateContext())
            {
                FileSaveBinaryInformation fileInfo = new FileSaveBinaryInformation();
                fileInfo.Content = file;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File targetFile = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                targetFile.SaveBinary(fileInfo);
                context.Load(targetFile);
                context.ExecuteQuery();
            }
        }
        public void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.UndoCheckOut();
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.UnPublish(comment);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Publish(comment);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            if (prop.ContainsKey("ChangedMetaInfo"))
            {
                Dictionary<string, object> changedMetaInfo = prop["ChangedMetaInfo"] as Dictionary<string, object>;
                mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, changedMetaInfo);
            }
            return null;
        }
        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            if (moveMethodName.Equals("MoveToCollection"))
            {//SAAS-611
                this.mRequestCommon.MoveNavigationNodeToCollection(webServerRelativeUrl, navigationNodeProperties);
            }
            else
            {
                this.mRequestCommon.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
            }
        }
        public Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties, string fieldSchema)
        {
            if (string.IsNullOrEmpty(fieldSchema))
            {
                return UpdateField(webServerRelativeUrl, listName, listId, internalName, fieldSource, contentTypeProp, fieldProperties);
            }
            else
            {
                if (fieldProperties != null && fieldProperties.Count > 0)
                {
                    var fieldPropertiesWithoutSomeKey = fieldProperties.Where(keyValue => (!keyValue.Key.Equals("fieldSource") &&
                    !keyValue.Key.Equals("ObjectPath") &&
                    !keyValue.Key.Equals("FieldType"))).ToArray();

                    if (fieldPropertiesWithoutSomeKey != null && fieldPropertiesWithoutSomeKey.Length > 0)
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(fieldSchema);
                        var fieldNode = doc.DocumentElement;
                        foreach (KeyValuePair<string, object> fieldProperty in fieldPropertiesWithoutSomeKey)
                        {
                            if (fieldProperty.Value != null)
                            {
                                if (fieldProperty.Value is bool)
                                {
                                    //the bool value need to be upper case
                                    fieldNode.SetAttribute(fieldProperty.Key, (bool)fieldProperty.Value ? "TRUE" : "FALSE");
                                }
                                else
                                {
                                    fieldNode.SetAttribute(fieldProperty.Key, fieldProperty.Value.ToString());
                                }
                            }
                            else
                            {
                                fieldNode.SetAttribute(fieldProperty.Key, string.Empty);
                            }
                        }

                        fieldSchema = doc.DocumentElement.OuterXml;
                    }
                }

                Dictionary<string, object> fieldProp = new Dictionary<string, object>();

                using (var context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
                {
                    Field field = null;
                    switch (fieldSource)
                    {
                        case "list.fields":
                            field = context.Web.Lists.GetByTitle(listName).Fields.GetByInternalNameOrTitle(internalName);
                            break;
                        case "web.fields":
                            field = context.Web.Fields.GetByInternalNameOrTitle(internalName);
                            break;
                        default:
                            throw new NotSupportedException(string.Format("invalid field source: {0}", fieldSource));
                    }
                    field.SchemaXml = fieldSchema;
                    field.Update();
                    context.Load(field);
                    context.ExecuteQuery();
                    AssembleSingleFieldProperties(fieldProp, field);

                    return fieldProp;
                }

                //return this.mWebServiceRequest.UpdateField(webServerRelativeUrl, listName, listId, internalName, fieldSource, contentTypeProp, fieldProperties, fieldSchema);
            }
        }


        private Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties)
        {
            //if (fieldProperties.ContainsKey("ClientContext"))
            //{
            //    context = fieldProperties["ClientContext"] as ClientContext;
            //}
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Web;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                ArgumentCheck.CheckNotNull(fields);
                Guid fieldId = GetFieldIdFromIdentity(fieldProperties["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields?.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;

                object lookupListObj;
                if (fieldProperties.TryGetValue("LookupList", out lookupListObj))
                {
                    var lookupField = field as FieldLookup;

                    if (lookupField != null)
                    {
                        context.Load(lookupField);
                        context.ExecuteQuery();
                        //var xml = ;

                        var xElement = XElement.Parse(lookupField.SchemaXml, LoadOptions.PreserveWhitespace);

                        xElement.SetAttributeValue("List", lookupListObj.ToString());


                        //var listIdRegex = new System.Text.RegularExpressions.Regex("List=\"([^\"]*)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ECMAScript);
                        //xml = listIdRegex.Replace(xml, String.Format("List=\"{0}\"", lookupListObj));

                        fieldProperties.Remove("LookupList");

                        object lookupWebObj;

                        if (fieldProperties.TryGetValue("LookupWebId", out lookupWebObj))
                        {
                            Guid lookupWebId = (Guid)lookupWebObj;
                            //var webIdRegex = new System.Text.RegularExpressions.Regex("WebId=\"([^\"]*)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ECMAScript);
                            //xml = webIdRegex.Replace(xml, String.Format("WebId=\"{0}\"", lookupWebId.ToString("D")));
                            xElement.SetAttributeValue("WebId", lookupWebId.ToString("D"));

                            fieldProperties.Remove("LookupWebId");
                        }

                        lookupField.SchemaXml = xElement.ToString();
                        lookupField.Update();
                        context.Load(lookupField);
                        context.ExecuteQuery();
                        AssembleSingleFieldProperties(fieldProp, field);
                    }
                }
                bool needReload = false;
                object fieldType;
                if (fieldProperties.TryGetValue("Type", out fieldType) && fieldType != null)
                {
                    fieldProperties["FieldTypeKind"] = fieldType;
                    needReload = true;
                }
                bool needUpdate = false;
                needUpdate |= UpdateUserResourceForUICulture(field.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, fieldProperties);
                needUpdate |= UpdateUserResourceForUICulture(field.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, fieldProperties);

                AveObjectCopy.UpdateObjectBasicProperties(fieldProperties, field);
                if ((int)(fieldProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || needUpdate)
                {
                    field.Update();
                    if (needReload)
                    {
                        field = fields?.GetById(fieldId);
                    }
                    context.Load(field?.TypedObject);
                    context.ExecuteQuery();
                    AssembleSingleFieldProperties(fieldProp, field?.TypedObject as Field);
                }
                return fieldProp;
            }
        }

        private bool UpdateUserResourceForUICulture(UserResource userResource, string resourceName, IDictionary<string, object> fieldProperties, bool needLoad = true)
        {
            bool changed = false;
            object resourceProperties;
            if (fieldProperties.TryGetValue(resourceName, out resourceProperties))
            {
                fieldProperties.Remove(resourceName);

                var changedProperties = resourceProperties as IDictionary<string, string>;
                if (changedProperties != null)
                {
                    SetAndGetUserResourceValueForUICulture(userResource, changedProperties, needLoad);
                    changed = true;
                }
            }

            return changed;
        }

        private void SetAndGetUserResourceValueForUICulture(UserResource userResource, IDictionary<string, string> keyValues, bool needLoad)
        {
            foreach (var item in keyValues)
            {
                mLogger.Info("Set user resource,Key:{0},Value:{1}", item.Key, item.Value);
                userResource.SetValueForUICulture(item.Key, item.Value);
                if (needLoad)
                {
                    userResource.GetValueForUICulture(item.Key);
                }
            }
        }

        public Dictionary<string, object> UpdateTermStore(Guid guid, Dictionary<string, object> needUpdateProperties)
        {
            Dictionary<string, object> TermStoreProp = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore termStore = session.TermStores.GetById(guid);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateProperties, termStore);
                context.Load(termStore);
                context.ExecuteQuery();
                AveObjectCopy.GetObjectBasicProperties(TermStoreProp, termStore);

                Dictionary<string, object> GroupsProperties = new Dictionary<string, object>();
                Dictionary<string, object> GroupList = new Dictionary<string, object>();
                if (needUpdateProperties.ContainsKey("AddGroup"))
                {
                    List<string> groupNames = needUpdateProperties["AddGroup"] as List<string>;
                    foreach (string groupName in groupNames)
                    {
                        Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                        TermGroup group = termStore.CreateGroup(groupName, Guid.NewGuid());
                        context.Load(group);
                        context.ExecuteQuery();
                        AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                        GroupList.Add(groupName, groupProperties);
                    }
                }
                if (needUpdateProperties.ContainsKey("UpdateGroups"))
                {
                    Dictionary<string, object> needUpdateGroups = needUpdateProperties["UpdateGroups"] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> group in needUpdateGroups)
                    {
                        TermGroup updateGroup = termStore.Groups.GetByName(group.Key);
                        Dictionary<string, object> needUpdateGroupProperties = group.Value as Dictionary<string, object>;
                        AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_OUT_RANGE_INDEX);
                        retryHelper.ExecuteWithRetryMechanism(() =>
                        {
                            AveObjectCopy.UpdateObjectBasicProperties(needUpdateGroupProperties, updateGroup);
                            updateGroup.TermStore.CommitAll();
                            context.Load(updateGroup);
                            context.Load(updateGroup.TermSets);
                            context.ExecuteQuery();
                        });
                        Dictionary<string, object> groupProp = UpdateTermGroup(context, updateGroup, termStore.DefaultLanguage, needUpdateGroupProperties);
                        GroupList[group.Key] = groupProp;
                    }
                }
                GroupsProperties["Group"] = GroupList;
                TermStoreProp.Add(termStore.Id.ToString(), GroupsProperties);
            }
            return TermStoreProp;
        }
        public Dictionary<string, object> UpdateUserProfileProperties(string userProfilePropertyName, Dictionary<string, object> dictionary)
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                //if (fieldProperties.ContainsKey("ClientContext"))
                //{
                //    context = fieldProperties["ClientContext"] as ClientContext;
                //}

                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Web;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProperties["ObjectPath"].ToString());
                ArgumentCheck.CheckNotNull(fields);
                ObjectPath path = new ObjectPathMethod(context, fields?.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;
                AveObjectCopy.UpdateObjectBasicProperties(fieldProperties, field);
                if ((int)(fieldProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    //Readonly field can be update if the ReadOnlyField property is true.
                    //field.ReadOnlyField = false;
                    //field.Update();
                    //field.ReadOnlyField = true;
                    field.Update();
                    context.Load(field);
                    context.ExecuteQuery();
                    CopyProperty(fieldProp, field);
                    fieldProp["Type"] = field.FieldTypeKind;
                    fieldProp["ObjectPath"] = GetObjectPathString(field.Path);
                    fieldProp["FieldType"] = field.GetType();
                }
                return fieldProp;
            }
        }

        public Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            using (var context = CreateContext())
            {
                var user = context.Web.SiteUsers.GetByLoginName(loginName);
                bool changed = false;
                foreach (KeyValuePair<string, object> pair in userProp)
                {
                    switch (pair.Key)
                    {
                        case "Email":
                            user.Email = userProp["Email"] as string;
                            changed = true;
                            break;
                        case "Name":
                            user.Title = userProp["Name"] as string;
                            changed = true;
                            break;
                        case "Notes":
                            //user. = userProp["Notes"] as string;
                            //need to update the list item field, no need to keep for this.
                            break;
                        case "IsSiteAdmin":
                            user.IsSiteAdmin = Convert.ToBoolean(pair.Value);
                            changed = true;
                            break;
                        default:
                            break;
                    }
                }

                if (changed)
                {
                    user.Update();
                }

                context.Load(user);
                context.ExecuteQuery();
                return ConvertUser(user);
            }
        }

        /*private void UpdateUserSiteAdmin(string webServerRelativeUrl, string loginName, bool isSiteAdmin)
        {
            using (ClientContext cc = CreateContext())
            {
                User user = cc.Site.RootWeb.SiteUsers.GetByLoginName(loginName);
                user.IsSiteAdmin = isSiteAdmin;
                user.Update();
                cc.ExecuteQuery();
            }
        }*/

        public void UpdateUserProfileDetails(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public void UpdateUserProfileMemberships(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public void UpdateUserProfileColleages(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public void UpdateUserProfileTags(string accountName, string xml)
        {
            throw new NotImplementedException();
        }

        public void SetThemeUrlForWeb(string webServerRelativeUrl, string themeUrl)
        {

        }

        public void ApplyTo(string webServerRelativeUrl, bool shareGenerated, string name)
        {

        }

        public Dictionary<string, object> UpdatePublishingWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            throw new NotImplementedException();
        }
        public void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }
        public void UpdateSpecialProperty(Dictionary<string, object> specialProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                AveObjectCopy.UpdateObjectBasicProperties(specialProp, site);
                context.ExecuteQuery();
            }
        }
        public void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {

        }
        public void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {

        }
        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            mRequestCommon.UpdateSiteRssSetting(syndicationEnabled);
        }
        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return mRequestCommon.UpdateKeyWord(term, localId, calendarType, keyWordProp);
        }
        public void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.Navigation.UseShared = useShared;
                context.ExecuteQuery();
            }
        }
        private void UpdateRegionalSettings(string webServerRelativeUrl, Dictionary<string, object> properties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //context.Load(web.RegionalSettings, rg => rg.TimeZone, rg => rg.TimeZones);
                if (properties.ContainsKey("TimeZoneChangedProperties"))
                    web.RegionalSettings.TimeZone = web.RegionalSettings.TimeZones.GetById((Convert.ToInt32((properties["TimeZoneChangedProperties"] as Dictionary<string, object>)["ID"])));
                //if (RSProperties.ContainsKey("Local"))
                //    web.RegionalSettings.LocaleId = Convert.ToUInt32(RSProperties["Local"]);
                //saas-23724,在regional setting里无法对locale进行修改，原因为上面那段把locale属性的修改注释掉了，properties中的属性应为localeId，所以更改为如下写法。
                if (properties.ContainsKey("LocaleId") || properties.ContainsKey("Local"))
                    web.RegionalSettings.LocaleId = properties.ContainsKey("LocaleId") ? Convert.ToUInt32(properties["LocaleId"]) : Convert.ToUInt32(properties["Local"]);
                if (properties.ContainsKey("Collation"))
                    web.RegionalSettings.Collation = Convert.ToInt16(properties["Collation"]);
                if (properties.ContainsKey("CalendarType"))
                    web.RegionalSettings.CalendarType = Convert.ToInt16(properties["CalendarType"]);
                if (properties.ContainsKey("ShowWeeks"))
                    web.RegionalSettings.ShowWeeks = Convert.ToBoolean(properties["ShowWeeks"]);
                if (properties.ContainsKey("AlternateCalendarType"))
                    web.RegionalSettings.AlternateCalendarType = Convert.ToInt16(properties["AlternateCalendarType"]);
                if (properties.ContainsKey("WorkDays"))
                    web.RegionalSettings.WorkDays = Convert.ToInt16(properties["WorkDays"]);
                if (properties.ContainsKey("FirstDayOfWeek"))
                    web.RegionalSettings.FirstDayOfWeek = Convert.ToUInt32(properties["FirstDayOfWeek"]);
                if (properties.ContainsKey("FirstWeekOfYear"))
                    web.RegionalSettings.FirstWeekOfYear = Convert.ToInt16(properties["FirstWeekOfYear"]);
                if (properties.ContainsKey("WorkDayStartHour"))
                    web.RegionalSettings.WorkDayStartHour = Convert.ToInt16(properties["WorkDayStartHour"]);
                if (properties.ContainsKey("WorkDayEndHour"))
                    web.RegionalSettings.WorkDayEndHour = Convert.ToInt16(properties["WorkDayEndHour"]);
                if (properties.ContainsKey("Time24"))
                    web.RegionalSettings.Time24 = Convert.ToBoolean(properties["Time24"]);
                if (properties.ContainsKey("AdjustHijriDays"))
                    web.RegionalSettings.AdjustHijriDays = Convert.ToInt16(properties["AdjustHijriDays"]);

                web.RegionalSettings.Update();
                context.ExecuteQuery();
            }
        }

        #endregion

        #region Delete
        public void DeleteSite()
        {
            throw new NotImplementedException();
        }
        public void DeleteFeature(string webServerRelativeUrl, Guid featureId, bool force, string featureSource)
        {
            using (AveClientContext context = CreateContext())
            {
                FeatureCollection featureCollection = null;
                switch (featureSource)
                {
                    case "site.features":
                        context.Load(context.Site, f => f.Features);
                        featureCollection = context.Site.Features;
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        featureCollection = web.Features;
                        break;
                    default:
                        break;
                }
                featureCollection?.Remove(featureId, force);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            using (AveClientContext context = CreateContext())
            {
                context.Site.RecycleBin.GetById(id).DeleteObject();
                context.ExecuteQuery();
            }
        }
        public void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(workflow.ParentWeb.ServerRelativeUrl);
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation needDeleteWF = null;
                switch (source)
                {
                    case "web.workflow":
                        needDeleteWF = web.WorkflowAssociations.GetById(workflow.ID);
                        break;
                    case "list.workflow":
                        if (workflow.ParentList != null)
                        {
                            List list = web.Lists.GetById(workflow.ParentList.ID);
                            needDeleteWF = list.WorkflowAssociations.GetById(workflow.ID);
                        }
                        break;
                    case "contentType.workflow":
                        ContentType contentType = null;
                        if (workflow.ParentList != null)
                        {
                            contentType = web.Lists.GetById(workflow.ParentList.ID).ContentTypes.GetById(workflow.ContentTypeId.ToString());
                            needDeleteWF = contentType.WorkflowAssociations.GetById(workflow.ID);
                        }
                        else
                        {
                            contentType = web.ContentTypes.GetById(workflow.ContentTypeId.ToString());
                            needDeleteWF = contentType.WorkflowAssociations.GetById(workflow.ID);
                        }
                        break;
                    default:
                        break;
                }
                try
                {
                    if (needDeleteWF != null)
                    {
                        needDeleteWF.DeleteObject();
                        context.ExecuteQuery();
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Delete workflow failed.Workflow Name:{0},Message:{1}", workflow.Name, ex.ToString());
                }
            }
        }
        public void DeleteWeb(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientObjectList<AppInstance> appInstances = AppCatalog.GetAppInstances(context, web);
                context.Load(appInstances);
                context.Load(web.Lists, ls => ls.Where(l => l.ItemCount >= ListViewThreshold).Include(l => l.Title, l => l.BaseType, l => l.RootFolder.ServerRelativeUrl, l => l.ItemCount));
                context.ExecuteQuery();
                if (appInstances.Count > 0)
                {
                    UninstallApps(context, web, appInstances);
                }
                DeleteLargeLists(context, web, webServerRelativeUrl, web.Lists);
                web.DeleteObject();
                context.ExecuteQuery();
            }
        }

        private void DeleteLargeLists(ClientContext context, Web web, string webServerRelativeUrl, ListCollection lists)
        {
            foreach (List list in lists.ToList<List>())
            {
                try
                {
                    DeleteList(webServerRelativeUrl, list.Title, list.Id, list.BaseTemplate, list.EntityTypeName, list.TemplateFeatureId.ToString());
                    //DeleteLargeList(context, web, list, list.RootFolder.ServerRelativeUrl, webServerRelativeUrl, list.BaseType == BaseType.DocumentLibrary);
                }
                catch (Exception e)
                {
                    mLogger.Error("failed to delete large list: {0} due to:{1}", list.Title, e.ToString());
                }
            }
        }

        private void UninstallApps(ClientContext context, Web web, ClientObjectList<AppInstance> appInstances)
        {
            foreach (AppInstance appInstance in appInstances)
            {
                appInstance.Uninstall();
            }
            context.ExecuteQuery();
            int sleepTime = 2;
            ClientObjectList<AppInstance> retryAppInstances = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            do
            {
                retryAppInstances = AppCatalog.GetAppInstances(context, web);
                context.Load(retryAppInstances, aps => aps.Include(a => a.Id));
                context.ExecuteQuery();
                if (retryAppInstances.Count > 0)
                {
                    if (watch.ElapsedMilliseconds > WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout)
                    {
                        throw new TimeoutException("time out when uninstalling app");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(sleepTime * 1000);
                        sleepTime++;
                    }
                }
            }
            while (retryAppInstances.Count > 0);
        }

        public void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId)
        {
            using (AveClientContext context = CreateContext())
            {
                View view = this.FindView(webServerRelativeUrl, listName, listId, viewId, context);
                if (view != null)
                {
                    view.DeleteObject();
                    context.ExecuteQuery();
                }
            }
        }

        public bool DeleteList(string webServerRelativeUrl, string listName, Guid listId, int baseTemplate, string entityTypeName, string templateFeatureId, bool recycle = false)
        {
            if ((templateFeatureId.IsNotNullOrEmpty() && listsActivatedByFeatureToSkipDeletion.ContainsKey(templateFeatureId) && listsActivatedByFeatureToSkipDeletion[templateFeatureId].Contains(entityTypeName))
                || entityTypeName.IsNotNullOrEmpty() && (systempListsToSkipDeletion.ContainsKey(entityTypeName) && systempListsToSkipDeletion[entityTypeName] == baseTemplate))
            {
                return false;
            }
            bool retried = false;
            while (true)
            {
                try
                {
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        context.Load(list, l => l.ItemCount, l => l.Title, l => l.BaseType);
                        context.Load(list, l => l.RootFolder.ServerRelativeUrl);
                        context.ExecuteQuery();
                        string folderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                        if (list.ItemCount > 5000)
                        {
                            //DeleteLargeList(context, web, list, folderServerRelativeUrl, webServerRelativeUrl, list.BaseType == BaseType.DocumentLibrary);

                            var folderStructure = GetListFolderStructure(web.Id, listId);
                            var watch = Stopwatch.StartNew();
                            var deletedCount = 0;
                            try
                            {
                                DeleteItems(list, folderStructure, ref deletedCount);
                            }
                            finally
                            {
                                watch.Stop();
                                mLogger.Info($"Item deletion costs {watch.Elapsed}, Url: {list.RootFolder.ServerRelativeUrl}, Item count: {list.ItemCount}, DeletedCount: {deletedCount}");
                            }
                            folderStructure?.Dispose();
                        }
                        if (recycle)
                        {
                            list.Recycle();
                        }
                        else
                        {
                            list.DeleteObject();
                        }
                        context.ExecuteQuery();
                    }
                    return true;
                }
                catch (WebException e)
                {
                    if (e.Message.ToLower().Contains("the operation has timed out") && !retried)
                    {
                        retried = true;
                        continue;
                    }

                    throw e;
                }
                catch (ServerException se)
                {
                    if (se.ServerErrorCode != -2146233088)      //list does not exist
                    {
                        throw se;
                    }
                }
            }
        }

        private void DeleteItems(List list, SPOFolder folder, ref int count)
        {
            if (folder.Items != null && folder.Items.Count > 0)
            {
                var itemsId = folder.Items.Select(i => i.Id);
                foreach (var id in itemsId)
                {
                    list.GetItemById(id).DeleteObject();
                    if (++count % AveCamlQuery.QUERY_VALUES_LIMITE_FILE == 0)
                    {
                        try
                        {
                            list.Context.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn($"Error occurred while deleting items in folder:{folder.Name}. Error:{ex}");
                        }
                    }
                }
            }
            if (folder.SubFolders != null && folder.SubFolders.Count > 0)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    DeleteItems(list, subfolder, ref count);

                    list.GetItemById(subfolder.Id).DeleteObject();
                    if (++count % AveCamlQuery.QUERY_VALUES_LIMITE_FILE == 0)
                    {
                        try
                        {
                            list.Context.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn($"Error occurred while deleting sub folders in folder:{folder.Name}. Error:{ex}");
                        }
                    }
                }
            }
            if (list.Context.HasPendingRequest)
            {
                try
                {
                    list.Context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Warn($"Error occurred while deleting objects in folder:{folder.Name}. Error:{ex}");
                }
            }
        }

        public void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            List<string> foldersToSkip = new List<string>() { "Lists/PublishedFeed" };     //these folder could not be deleted
            foreach (string folderToSkip in foldersToSkip)
            {
                if (folderServerRelativeUrl.Contains(folderToSkip))
                {
                    return;
                }
            }

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                folder.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public void DeleteItem(string webServerRelativeUrl, string listUrl, string listTile, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                item.DeleteObject();
                context.ExecuteQuery();
            }
        }

        /// <summary>
        /// Twenty data are deleted at a time.
        /// </summary>
        public void DeleteItemsByRowIds(string webUrl, Guid listId, Dictionary<int,long> rowIdsWithModifiedTime, Dictionary<int, long> rowIdsWithTimeLastModified)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webUrl);
                List list = web.Lists.GetById(listId);
                CamlQuery query = new CamlQuery();
                StringBuilder values = new StringBuilder();
                foreach (var id in rowIdsWithModifiedTime.Keys)
                {
                    values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                }
                query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);
                var items = list.GetItems(query);
                context.Load(items);
                context.ExecuteQuery();
                List<ListItem> itemCollections = new List<ListItem>();
                foreach (var item in items)
                {
                    if (CheckItemHasModifiedAfterBackup(item, rowIdsWithModifiedTime[item.Id], rowIdsWithTimeLastModified[item.Id]))
                    {
                        throw new Exception("StorageOptimization_DeleteItemSkip_Modified");
                    }
                    itemCollections.Add(item);
                }
                itemCollections.ForEach(item =>
                {
                    item.DeleteObject();
                });
                context.ExecuteQuery();
            }
        }
        private bool CheckItemHasModifiedAfterBackup(ListItem item, long archiverModifiedTime, long archiverTimeLastModified)
        {
            try
            {
                DateTime modifiedTime = (DateTime)item.FieldValues["Modified"];
                long modifiedTimeToleranceTicks = TimeSpan.FromSeconds(5).Ticks;

                if (archiverTimeLastModified > 0 && archiverTimeLastModified + modifiedTimeToleranceTicks < modifiedTime.Ticks)
                {
                    mLogger.Error($"Success repeat time statistic error result to unable leave stub, modifiedTime:{modifiedTime.Ticks}, archiverModifiedTime:{archiverModifiedTime}, timeLastModified:{archiverTimeLastModified}");
                }

                if (archiverModifiedTime <= 0)
                {
                    mLogger.Error($"archiverModifiedTime is 0, modifiedTime:{modifiedTime.Ticks}, archiverModifiedTime:{archiverModifiedTime}, timeLastModified:{archiverTimeLastModified}");
                    return true;
                }
                else if (archiverModifiedTime > 0 && archiverModifiedTime + modifiedTimeToleranceTicks < modifiedTime.Ticks)
                {
                    mLogger.Warn($"stub current doc has modifed,can not deleted it,archiver modified time:{archiverModifiedTime},modfied time:{modifiedTime.Ticks}, timeLastModified:{archiverTimeLastModified}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"stub CheckItemHasModifiedAfterBackup failed, error:{ex}");
                return true;
            }
        }

        /// <summary>
        /// Twenty data are deleted at a time.
        /// </summary>
        public void DeleteItemsByRowIds(string webUrl, Guid listId, List<int> rowIds)
        {
            using AveClientContext context = CreateContext();
            Web web = context.Site.OpenWeb(webUrl);
            List list = web.Lists.GetById(listId);
            CamlQuery query = new CamlQuery();
            StringBuilder values = new StringBuilder();
            foreach (var id in rowIds)
            {
                values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
            }
            query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);
            var items = list.GetItems(query);
            context.Load(items);
            context.ExecuteQuery();
            List<ListItem> itemCollections = [.. items];
            itemCollections.ForEach(item =>
            {
                item.DeleteObject();
            });
            context.ExecuteQuery();
        }

        public void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTile, Guid listId, int itemId, int versionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list);
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                context.ExecuteQuery();
                string listId1 = list.Id.ToString();
                string fileName = item.FieldValues["FileRef"].ToString();
                //string op = "Delete";
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileName));   //SAAS-10871 通过API来删除Item Version
                context.Load(file);
                FileVersion version = file.Versions.GetById(versionId);
                version.DeleteObject();
                context.ExecuteQuery();
                //OperateOnVersion(webServerRelativeUrl, WebAppName, mObj, listUrl, itemId, versionId, listId1, fileName, op);
            }
        }

        public void OperateOnVersion(string webServerRelativeUrl, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            if (tokenProvider.TokenType != TokenType.Bearer)
            {
                string mLayouts;
                if (CompatibilityLevel == 15)
                {
                    mLayouts = "/_layouts/15";
                }
                else
                {
                    mLayouts = "/_layouts";
                }
                string url = webAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/') + mLayouts + "/Versions.aspx?";
                AveHttpWebRequestCommon.OperateOnVersion(url, webAppName, tokenProvider, listUrl, itemId, versionId, listId, fileName, op);
            }
        }

        public void DeleteHistoryVersions(string webServerRelativeUrl, Guid listId, int itemId, IEnumerable<int> versionIds)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                var item = list.GetItemById(itemId);
                Dictionary<int, ExceptionHandlingScope> scopes = new Dictionary<int, ExceptionHandlingScope>();

                foreach (var versionId in versionIds)
                {
                    var exceptionHandler = new ExceptionHandlingScope(context);
                    using (exceptionHandler.StartScope())
                    {
                        using (exceptionHandler.StartTry())
                        {
                            item.Versions.GetById(versionId).DeleteObject();
                        }
                        using (exceptionHandler.StartCatch())
                        { }
                    }
                    scopes[versionId] = exceptionHandler;
                }

                context.ExecuteQuery();

                foreach (var scope in scopes)
                {
                    if (scope.Value.HasException)
                    {
                        mLogger.Warn("Delete Version:{0} for item:{1} under list:{2} with web:{3} failed:{4}", scope.Key, itemId, listId, webServerRelativeUrl, scope.Value.ExtractException());
                    }
                }
            }
        }

        //public void DeleteListItemVersions(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, string listId, string fileName, string op)
        //{
        //    string mLayouts = "/_layouts/15";
        //    string url = webAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/') + mLayouts + "/Versions.aspx?";
        //    AveHttpWebRequestCommon.DeleteListItemVersions(url, webAppName, tokenProvider, listUrl, itemId, listId, fileName, op);
        //}
        public void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Versions.DeleteByID(id);
                context.ExecuteQuery();
            }
        }
        public void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = InitDeleteFileVersionClientObject())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Versions.DeleteAll();
                context.ExecuteQuery();
            }
        }
        public void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Versions.DeleteByLabel(versionLabel);
                context.ExecuteQuery();
            }
        }
        public List<int> DeleteFileVersionSpecificNumber(string webServerRelativeUrl, string fileServerRelativeUrl, List<int> id)
        {
            List<int> failedVersionIds = new List<int>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                int versionCount = 0;
                List<int> currentBatchVersionIds = new List<int>();
                StringBuilder builder = new StringBuilder();
                foreach (int mid in id)
                {
                    try
                    {
                        builder.Append(mid + ";");
                        currentBatchVersionIds.Add(mid);
                        versionCount++;
                        file.Versions.DeleteByID(mid);
                        if (versionCount >= 50)
                        {
                            context.ExecuteQuery();
                            mLogger.Info($"DeleteFileVersionSpecificNumber success.Versions:{builder.ToString()}.");
                            versionCount = 0;
                            currentBatchVersionIds.Clear();
                            builder.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        failedVersionIds.AddRange(currentBatchVersionIds);
                        mLogger.Warn($"DeleteFileVersionSpecificNumber failed.Versions:{builder.ToString()}.Message:{ex}.");
                        versionCount = 0;
                        currentBatchVersionIds.Clear();
                        builder.Clear();
                    }
                }
                if (versionCount > 0)
                {
                    try
                    {
                        context.ExecuteQuery();
                        mLogger.Info($"DeleteFileVersionSpecificNumber success.Versions:{builder.ToString()}.");
                    }
                    catch (Exception ex)
                    {
                        failedVersionIds.AddRange(currentBatchVersionIds);
                        mLogger.Warn($"DeleteFileVersionSpecificNumber final failed.Versions:{builder.ToString()}.Message:{ex}.");
                    }
                }
            }
            return failedVersionIds;
        }
        public void DeleteGroup(string webServerRelativeUrl, int id)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Group group = web.SiteGroups.GetById(id);
                web.SiteGroups.Remove(group);
                context.ExecuteQuery();
            }
        }
        public void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                mLogger.Info($"Begin DeleteRoleAssignment.listServerRelativeUrl:{listServerRelativeUrl}.listTitle:{listTitle}.listId:{listId}.itemId:{itemId}.principalId:{principalId}.source:{source}.");
                switch (source)
                {
                    case "web.roleAssignments":
                        web.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                    case "list.roleAssignments":
                        List list = web.Lists.GetById(listId);
                        list.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                    case "item.roleAssignments":
                        List _list = web.Lists.GetById(listId);
                        ListItem item = _list.GetItemById(itemId);
                        item.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                }
                context.ExecuteQuery();
            }
        }
        public void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName)
        {
            using (AveClientContext context = CreateContext())
            {
                mLogger.Info($"Begin DeleteRoleDefinition.webServerRelativeUrl:{webServerRelativeUrl}.roleDefintionName:{roleDefintionName}.");
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.RoleDefinitions.GetByName(roleDefintionName).DeleteObject();
                context.ExecuteQuery();
            }
        }

        public void DeleteAttachmentNow(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string leafName)
        {
            using (AveClientContext context = CreateContext())   //SAAS-11014 通过API来删除Item下的Attachment
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                Attachment attachment = item.AttachmentFiles.GetByFileName(leafName);
                attachment.DeleteObject();
                context.ExecuteQuery();
            }
            //mWebServiceRequest.DeleteAttachmentNow(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, itemId, leafName);
        }

        public void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.Remove(fieldName);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.RemoveAll();
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }

        public void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiverProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                EventReceiverDefinition eventReceiverDefinition = null;
                switch (eventReceiverDefSource)
                {
                    case "web.eventReceivers":
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    case "list.eventReceivers":
                        List list = web.Lists.GetById(listId);
                        eventReceiverDefinition = list.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    default:
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                }

                eventReceiverDefinition.DeleteObject();
                context.ExecuteQuery();
            }
        }
        public void DeleteNavigationNode(string webServerRelativeUrl, IDictionary<string, object> parentNodeProperties, IDictionary<string, object> deleteNodeProperties)
        {
            ClientContext context = CreateContext();
            if (deleteNodeProperties != null && deleteNodeProperties.ContainsKey("ClientContext"))
            {
                context = deleteNodeProperties["ClientContext"] as ClientContext;
            }
            NavigationNode deleteNavigationNode = new NavigationNode(context, deleteNodeProperties?["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
            context.Load(deleteNavigationNode);
            deleteNavigationNode.DeleteObject();
            context.ExecuteQuery();
        }
        public void DeleteField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Field field = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        field = list.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "web.fields":
                        field = web.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "web.availableFields":
                        field = web.AvailableFields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["Id"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        field = contentType.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    default:
                        break;
                }
                field?.DeleteObject();
                context.ExecuteQuery();
            }
        }
        public void DeleteUserSolution(Guid solutionId)
        {
            throw new NotImplementedException();
        }
        public void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            using (AveClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (source)
                {
                    case "group.users":
                        web.SiteGroups.GetByName(groupName).Users.RemoveByLoginName(loginName);
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        web.SiteUsers.RemoveByLoginName(loginName);
                        break;
                    default:
                        break;
                }
                context.ExecuteQuery();
            }
        }

        public void DeleteUsers(string webServerRelativeUrl, string source, string groupName, List<string> loginNames)
        {
            using (AveClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (source)
                {
                    case "group.users":
                        foreach (string loginName in loginNames)
                        {
                            web.SiteGroups.GetByName(groupName).Users.RemoveByLoginName(loginName);
                        }
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        foreach (string loginName in loginNames)
                        {
                            web.SiteUsers.RemoveByLoginName(loginName);
                        }
                        break;
                    default:
                        break;
                }
                context.ExecuteQuery();
            }
        }

        public void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles)
        {
        }
        public bool DeleteContextType(string contentTypeId, string webServerRelativeUrl, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                ContentTypeCollection contentTypes;
                if (listId != Guid.Empty)
                {
                    var list = web.Lists.GetById(listId);
                    contentTypes = list.ContentTypes;
                }
                else
                {
                    contentTypes = web.ContentTypes;
                }
                var contentType = contentTypes.GetById(contentTypeId);
                contentType.DeleteObject();
                context.ExecuteQuery();
                return true;
            }
        }

        #endregion

        #region Restore
        public virtual void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            using (AveClientContext context = CreateContext())
            {
                context.Site.RecycleBin.GetById(id).Restore();
                context.ExecuteQuery();
            }
        }

        public virtual void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Versions.RestoreByLabel(versionLabel);
                context.ExecuteQuery();
            }
        }

        public void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, List<AveWebPartBaseInfo> webpartBaseInfoList, AveWebPartCache mapping, bool post)
        {
            using (ClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                using (AveWebPartRestore webpartRestore = new AveWebPartRestore(this, webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, post, context, mapping, tokenProvider))
                {
                    webpartRestore.RestoreWebParts(webpartBaseInfoList);
                }
            }
        }

        public Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            return RestoreListItem(data, userData, null);
        }

        public Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Dictionary<string, object> uniqueValues)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveListItemRestore listItemRestore = new AveListItemRestore(this, site, context))
                {
                    return listItemRestore.RestoreListItem(data, userData, uniqueValues);
                }
            }
        }

        public Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveFolderRestore folderRestore = new AveFolderRestore(this, site, context, tokenProvider))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }

        public Dictionary<string, object> RestoreDocument(AveDocumentInfo docInfo, Stream fileStream, DocumentRestoreInfo parentInfo)
        {
            using (ClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + docInfo.ParentWebRelativeUrl.TrimStart('/')))
            {
                Site site = context.Site;
                using (AveDocumentRestore documentRestore = new AveDocumentRestore(this, site, context, mServerVersion))
                {
                    return documentRestore.RestoreDocument(docInfo, fileStream, parentInfo);
                }
            }
        }

        public Dictionary<string, object> RestoreAttachment(string parentWebFullUrl, Dictionary<string, object> data, Stream fileStream)
        {
            using (AveClientContext context = CreateContext(parentWebFullUrl))
            {
                using (AveAttachmentRestore attachmentRestore = new AveAttachmentRestore(this, context, tokenProvider))
                {
                    return attachmentRestore.RestoreAttachment(data, fileStream);
                }
            }
        }



        public List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);

                //if (tokenProvider.TokenType == TokenType.Bearer)
                {
                    switch (featuresSource)
                    {
                        case "web.features":
                            return RestoreWebFeatures(context, webServerRelativeUrl, force, scope, featureInfoList);
                        case "site.features":
                            return RestoreSiteFeatures(context, webServerRelativeUrl, force, scope, featureInfoList);
                        default:
                            throw new NotImplementedException(string.Format("The scope:{0} is not supported", featuresSource));
                    }
                }

               // return mRequestCommon.RestoreFeatures(webServerRelativeUrl, force, scope, featuresSource, featureInfoList, context, web);
            }
        }

        public List<Dictionary<string, object>> RestoreSiteFeatures(ClientContext context, string webServerRelativeUrl, bool force, int scope, List<Dictionary<string, object>> featureInfoList)
        {
            FeatureCollection collection = context.Site.Features;
            FeatureDefinitionScope featureDefScope = FeatureDefinitionScope.Site;

            context.Load(collection, f => f.Include(a => a.DefinitionId));
            context.ExecuteQuery();

            HashSet<Guid> activedFeatures = new HashSet<Guid>();
            foreach (var featureDef in collection)
            {
                activedFeatures.Add(featureDef.DefinitionId);
            }

            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = RestoreFeature(context, collection, featureId, force, scope, featureDefScope, activedFeatures);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add Feature to {0}:{1} failed.Error Message:{2}", featureDefScope, webServerRelativeUrl, ex);
                }
            }
            return featuresProperties;
        }

        public List<Dictionary<string, object>> RestoreWebFeatures(ClientContext context, string webServerRelativeUrl, bool force, int scope, List<Dictionary<string, object>> featureInfoList)
        {
            FeatureCollection siteFeatures = context.Site.Features;
            //SAAS-32681  目的端context对象的web url(该url是top level site的url),并不一定是webServerRelativeUrl
            Web currentWeb = context.Site.OpenWeb(webServerRelativeUrl);
            context.Load(currentWeb);
            context.ExecuteQuery();

            FeatureCollection collection = currentWeb.Features;
            FeatureDefinitionScope featureDefScope = FeatureDefinitionScope.Web;

            context.Load(siteFeatures, f => f.Include(a => a.DefinitionId));
            context.Load(collection, f => f.Include(a => a.DefinitionId));
            context.ExecuteQuery();

            HashSet<Guid> activedFeatures = new HashSet<Guid>();
            foreach (var featureDef in collection)
            {
                activedFeatures.Add(featureDef.DefinitionId);
            }

            HashSet<Guid> siteActivedFeatures = new HashSet<Guid>();
            foreach (var featureDef in siteFeatures)
            {
                siteActivedFeatures.Add(featureDef.DefinitionId);
            }

            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        object featureSourceObj;
                        if (featureInfo.TryGetValue("FeatureSource", out featureSourceObj) && featureSourceObj != null)
                        {
                            if ("site.features".Equals(featureSourceObj.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                RestoreFeature(context, siteFeatures, id, force, scope, FeatureDefinitionScope.Site, siteActivedFeatures);
                            }
                            else
                            {
                                RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                            }
                        }
                        else
                        {
                            RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                        }
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = RestoreFeature(context, collection, featureId, force, scope, featureDefScope, activedFeatures);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add Feature to {0}:{1} failed.Error Message:{2}", featureDefScope, webServerRelativeUrl, ex);
                }
            }
            return featuresProperties;
        }

        private Dictionary<string, object> RestoreFeature(ClientContext context, FeatureCollection featureCollection, Guid featureId, bool force, int scope, FeatureDefinitionScope featureDefinitionScope, HashSet<Guid> activedFeatures)
        {
            Dictionary<string, object> featureProp = null;
            if (!activedFeatures.Contains(featureId))
            {
                int times = 2;
                ///测试过程中发现，对于sanbox solution，scope就是对应的scope，而SPO built 的feature都是farm的scope，但是备份的时候无法得知具体的scope，
                ///所以这里先使用farm进行尝试，然后再使用对应的scope。
                ///按照PNP的guide，可能需要等待一段时间来获取feature是否active上，比如publishing feature，这个先看看测试结果是否需要添加monitoring
                var defaultScope = FeatureDefinitionScope.Farm;
                featureProp = new Dictionary<string, object>();
                while (times > 0)
                {
                    times--;
                    try
                    {
                        var feature = featureCollection.Add(featureId, force, defaultScope);

                        context.ExecuteQuery();
                        bool actived=CheckIsFeatureActive(featureCollection, featureId, 10, 5);
                        mLogger.Info("Restore Feature id:[{0}],force:[{1}],defaultScope:[{2}] succeed,FinalCheckState:{3}", featureId, force, defaultScope.ToString(), actived);
                        activedFeatures.Add(featureId);
                        featureProp["DefinitionId"] = featureId;
                        Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
                        featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
                        break;
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("failed to activate feature: {0} -> {1} due to: {2}", featureId, defaultScope, e);

                        defaultScope = featureDefinitionScope;
                    }
                }
            }

            return featureProp;
        }

        private static bool CheckIsFeatureActive(FeatureCollection features,Guid featureId,int retryAttempts = 10,int pollingIntervalSeconds=5)
        {
            bool result = false;
            int retryCount = 0;
            while (retryAttempts > retryCount)
            {
                if (IsFeatureActiveInternal(features, featureId, noRetry: true))
                {
                    retryCount = retryAttempts;
                    result = true;
                    mLogger.Info("Feature {0} is Actived", featureId);
                }
                else
                {
                    retryCount++;
                    mLogger.Info("Feature {0} is not Actived", featureId);
                }
                Thread.Sleep(TimeSpan.FromSeconds((double)pollingIntervalSeconds));
            }
            return result;
        }

        internal static void ClearObjectData(ClientObject clientObject)
        {
            ((ClientObjectData)typeof(ClientObject).GetProperty("ObjectData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(clientObject, new object[0])).MethodReturnObjects.Clear();
        }

        private static bool IsFeatureActiveInternal(FeatureCollection features, Guid featureID, bool noRetry = false)
        {
            bool featureIsActive = false;
            ClearObjectData(features);
            features.Context.Load(features);
            features.Context.ExecuteQuery();
            Feature byId = features.GetById(featureID);
            features.Context.Load(byId, b => b.DefinitionId);
            features.Context.ExecuteQuery();
            if (byId != null && byId.IsPropertyAvailable("DefinitionId") && !byId.ServerObjectIsNull.Value && byId.DefinitionId.Equals(featureID))
            {
                featureIsActive = true;
            }
            return featureIsActive;
        }


        public void RestoreSolutionStatus(string webServerRelativeUrl, IList<AveSolutionInfo> sandboxSolutions)
        {
            using (ClientContext context = CreateContext())
            {
                IList<Guid> activitedSolutions = GetActivitedSolutions(context.Site.RootWeb, context);

                AveSolutionInfo readySolution = GetReadyToBeActivitedSoltuion(sandboxSolutions, activitedSolutions);
                while (readySolution != null)
                {
                    try
                    {
                        mLogger.Info("Start to active solution:{0} under url:{1}, web server relative url:{2}, rowId:{3}", readySolution.Id, context.Url, webServerRelativeUrl, readySolution.RowId);
                        string name = Path.GetFileName(webServerRelativeUrl);
                        InstallDesignPackage(name, webServerRelativeUrl);
                        //AveWebServiceRequest.OperateOnSolution("ACT", context.Url, webServerRelativeUrl, readySolution.RowId, tokenProvider);
                        activitedSolutions.Add(readySolution.Id);
                        WaitUntilSolutionActivated(context, readySolution.RowId);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("faield to active solution due to: {0}", e.ToString());
                    }
                    sandboxSolutions.Remove(readySolution);
                    readySolution = GetReadyToBeActivitedSoltuion(sandboxSolutions, activitedSolutions);
                }
            }
        }

        private void WaitUntilSolutionActivated(ClientContext context, int rowid)
        {
            List solutionList = context.Site.RootWeb.GetCatalog((int)AveListTemplateType.SolutionCatalog);
            ListItem item = solutionList.GetItemById(rowid);

            bool status = false;
            int i = 0;
            do
            {
                try
                {
                    context.Load(item);
                    context.ExecuteQuery();
                    FieldLookupValue lookupValue = item.FieldValues["Status"] as FieldLookupValue;
                    status = lookupValue.LookupValue == "1";
                }
                /*review-qlluo*/
                catch (Exception e)
                {
                    mLogger.Error("failed to check solution status due to: {0}", e.ToString());
                }
                if (i++ > 20 || status)
                {
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
            while (true);
        }

        private IList<Guid> GetActivitedSolutions(Web rootweb, ClientContext context)
        {
            IList<Guid> activitedSolutions = new List<Guid>();

            List solutionGallery = rootweb.GetCatalog((int)AveListTemplateType.SolutionCatalog);
            CamlQuery activitedSolutionQuery = new CamlQuery();
            activitedSolutionQuery.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><ViewFields><FieldRef Name=\"SolutionId\"/></ViewFields><Query><Where><Eq><FieldRef Name=\"Status\"/><Value Type=\"String\">1;#1</Value></Eq></Where></Query></View>");
            ListItemCollection activitedItems = solutionGallery.GetItems(activitedSolutionQuery);
            context.Load(activitedItems);
            context.ExecuteQuery();
            foreach (ListItem activitedItem in activitedItems)
            {
                activitedSolutions.Add((Guid)activitedItem["SolutionId"]);
            }
            return activitedSolutions;
        }

        private AveSolutionInfo GetReadyToBeActivitedSoltuion(IList<AveSolutionInfo> sandboxSolutions, IList<Guid> activitedSolutions)
        {
            foreach (AveSolutionInfo solutionInfo in sandboxSolutions)
            {
                if (solutionInfo.Dependencies.Count == 0
                    || solutionInfo.Dependencies.All((d) => activitedSolutions.Contains(d)))
                {
                    return solutionInfo;
                }
            }
            return null;
        }

        class WebNavigationProperties
        {
            public bool AddNewPagesToNavigation { get; set; }
            public bool CreateFriendlyUrlsForNewPages { get; set; }
        }

        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties, AveNavigationInfoList navigationList)
        {
            return mRequestCommon.RestoreNavigation(webServerRelativeUrl, nodes, webAllProperties, navigationList);
        }
        public bool RestoreSearchNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            return mRequestCommon.RestoreSearchNavigation(webServerRelativeUrl, nodes, webAllProperties);
        }
        public void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            try
            {
                #region Mew Logic Description
                /* 
                * #0 if themedCssFolderUrl is null or empty, need to change the theme to default
                * #1 review the restore condition, themedtitle is assigned for 13 style(webservice which is for 10 style should be obsoluted), restore theme should have only one implentment
                * #2 retrieve the themed file(color,font and image) by URL(url has been replaced when RestoreWebProperty)
                * #3 if file not exist in #2, add file to _catalogs/theme or Site Assets Library(in case DenyAddAndCustomizePages is enabled), if site assets doesn't exist, skip
                * #4 remove file added in both _catalogs/them or site assets after applied theme 
                *  
                * */
                #endregion
                RestoreWebMasterPageUrl(webServerRelativeUrl, webSettingInfo); //Restore maste page url at first.
                if (webSettingInfo.ModernThemeInfo != null && webSettingInfo.ModernThemeInfo.IsAvailable && webSettingInfo.ModernThemeInfo.Value != null)
                {
                    AveModernThemeInfo modernThemeInfo = webSettingInfo.ModernThemeInfo.Value;
                    if (string.IsNullOrEmpty(modernThemeInfo.ThemedCssFolderUrl)) // source web with default theme, revert the destination web theme to default.
                    {
                        using (ClientContext context = CreateContext())
                        {
                            Web web = context.Site.OpenWeb(webServerRelativeUrl);
                            context.Load(web, w => w.ThemedCssFolderUrl);
                            web.ThemedCssFolderUrl = null;
                            web.Update();
                            context.ExecuteQuery();
                            mLogger.Info("Change the theme from {0} to default. webServerRelativeUrl:{1}", web.ThemedCssFolderUrl, webServerRelativeUrl);
                            ResetCurrentItemOfDesignCatalog(context, web, webSettingInfo.ThemedColorUrl?.Value, webSettingInfo.ThemedFontUrl?.Value, webSettingInfo.ThemedImageUrl?.Value);
                        }
                    }
                    else
                    {
                        ApplyModernTheme(webServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, modernThemeInfo);
                    }
                }
                //Keep the old logic for compatible with old data
                else if (webSettingInfo.ThemedTitle != null && webSettingInfo.ThemedTitle.IsAvailable)
                {
                    using (ClientContext context = CreateContext())
                    {
                        string themeColorURL = null;
                        string themeFontURL = null;
                        string themeImageURL = null;
                        if (webSettingInfo.ThemedColorUrl != null && webSettingInfo.ThemedColorUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedColorUrl.Value))
                        {
                            themeColorURL = webSettingInfo.ThemedColorUrl.Value;
                        }
                        if (webSettingInfo.ThemedFontUrl != null && webSettingInfo.ThemedFontUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedFontUrl.Value))
                        {
                            themeFontURL = webSettingInfo.ThemedFontUrl.Value;
                        }
                        if (webSettingInfo.ThemedImageUrl != null && webSettingInfo.ThemedImageUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedImageUrl.Value))
                        {
                            // 是否还原Themed Image File，以及还原是否成功
                            bool? addThemedImage = null;
                            if (webSettingInfo.ThemedImageContent != null && webSettingInfo.ThemedImageContent.IsAvailable && webSettingInfo.ThemedImageContent.Value != null)
                            {
                                // "/sites/test/_catalogs/theme/Themed/108FDA81/fb58a5f4-e2aa-41dd-9a5e-62962c8ba068bkimage-4CB8FF1B.themedjpg?ctag"
                                var tempImageUrl = webSettingInfo.ThemedImageUrl.Value;
                                try
                                {
                                    var index = tempImageUrl.IndexOf('?');
                                    if (index > 0)
                                    {
                                        tempImageUrl = tempImageUrl.Substring(0, index);
                                    }
                                    index = tempImageUrl.LastIndexOf('/');
                                    var imageName = tempImageUrl.Substring(index).TrimStart('/');

                                    // 获取ThemeGallery下的Themed folder
                                    var themedLibrary = context.Site.RootWeb.GetCatalog((int)AveListTemplateType.ThemeCatalog);
                                    var themedFolder = themedLibrary.RootFolder.Folders.GetByPath(ResourcePath.FromDecodedUrl("Themed"));
                                    //context.Load(themedFolder, f => f.ServerRelativeUrl);
                                    //context.ExecuteQuery();
                                    const string themedForderUrl = "/_catalogs/theme/Themed";

                                    // 获取Themed folder下的“108FDA81” folder
                                    var startIndex = tempImageUrl.IndexOf(themedForderUrl) + themedForderUrl.Length;
                                    var folderName = tempImageUrl.Substring(startIndex, index - startIndex).TrimStart('/');
                                    //var folderName = tempImageUrl.Substring(themedFolder.ServerRelativeUrl.Length, index - themedFolder.ServerRelativeUrl.Length).TrimStart('/');
                                    // 如果folder存在会抛异常, 用ConditionalScope方式没有创建出folder，改用ExceptionHandlingScope
                                    Folder folder = null;
                                    var exceptionScope = new ExceptionHandlingScope(context);
                                    using (exceptionScope.StartScope())
                                    {
                                        using (exceptionScope.StartTry())
                                        {
                                            themedFolder.AddSubFolderUsingPath(ResourcePath.FromDecodedUrl(folderName));
                                        }

                                        using (exceptionScope.StartFinally())
                                        {
                                            folder = themedFolder.Folders.GetByPath(ResourcePath.FromDecodedUrl(folderName));
                                        }
                                    }

                                    // 上传Themed Image File
                                    using (var stream = new MemoryStream(webSettingInfo.ThemedImageContent.Value))
                                    {
                                        var imageFile = folder.Files.AddUsingPath(ResourcePath.FromDecodedUrl(imageName), new FileCollectionAddParameters() { Overwrite = true }, stream);
                                        context.ExecuteQuery();
                                    }
                                    addThemedImage = true;
                                }
                                catch (Exception ex)
                                {
                                    mLogger.Warn("Add theme image file:{0} faild:{1}", webSettingInfo.ThemedImageUrl.Value, ex);
                                    addThemedImage = false;
                                }
                            }
                            try
                            {
                                if (addThemedImage == null)
                                {
                                    Web rootWeb = context.Site.RootWeb;
                                    Microsoft.SharePoint.Client.File imageFile = rootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(webSettingInfo.ThemedImageUrl.Value));
                                    ConditionalScope conditionScope = new ConditionalScope(context, () => imageFile.Exists, true);
                                    using (conditionScope.StartScope())
                                    {
                                        context.Load(imageFile);
                                    }
                                    context.ExecuteQuery();
                                    if (conditionScope.TestResult.Value)
                                    {
                                        themeImageURL = webSettingInfo.ThemedImageUrl.Value;
                                    }
                                }
                                else if (addThemedImage == true)
                                {
                                    themeImageURL = webSettingInfo.ThemedImageUrl.Value;
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Warn("Get theme image file failed,file:{0},error:{1}", webSettingInfo.ThemedImageUrl.Value, e.ToString());
                            }
                        }
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        try
                        {
                            mLogger.Info("apply theme for web:{0} with color:{1}, font:{2}, image:{3}", webServerRelativeUrl, themeColorURL, themeFontURL, themeImageURL);
                            web.ApplyTheme(themeColorURL, themeFontURL, themeImageURL, true);
                            context.ExecuteQuery();
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn(e.ToString());
                        }
                        ResetCurrentItemOfDesignCatalog(context, web, themeColorURL, themeFontURL, themeImageURL);
                    }
                }
                //[Obsolete] For 10 style theme
                //else
                //{
                //    //TODO_LONG need to know the restore condition
                //    mWebServiceRequest.RestoreTheme(webServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
                //}
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occurried while restore theme with the legacy logic. Ex:{0}", ex);
            }
        }

        #region Private methods of Restore Theme
        private void RestoreWebMasterPageUrl(string webServerRelativeUrl, AveWebSettingInfo webSettingInfo)
        {
            bool needUpdate = false;
            try
            {
                using (ClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    if (webSettingInfo.MasterUrl != null && webSettingInfo.MasterUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.MasterUrl.Value))
                    {
                        mLogger.Info("update web:{0} with master url:{1}", webServerRelativeUrl, webSettingInfo.MasterUrl.Value);

                        var rootWeb = context.Site.RootWeb;
                        var masterFile = rootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(webSettingInfo.MasterUrl.Value));
                        var conditionScope = new ConditionalScope(context, () => masterFile.Exists, true);
                        using (conditionScope.StartScope())
                        {
                            using (conditionScope.StartIfTrue())
                            {
                                web.MasterUrl = webSettingInfo.MasterUrl.Value;
                                web.Update();
                                needUpdate = true;
                            }
                        }
                    }
                    if (webSettingInfo.CustomMasterUrl != null && webSettingInfo.CustomMasterUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.CustomMasterUrl.Value))
                    {
                        mLogger.Info("update web:{0} with custom master url:{1}", webServerRelativeUrl, webSettingInfo.CustomMasterUrl.Value);
                        var rootWeb = context.Site.RootWeb;
                        var masterFile = rootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(webSettingInfo.CustomMasterUrl.Value));
                        var conditionScope = new ConditionalScope(context, () => masterFile.Exists, true);
                        using (conditionScope.StartScope())
                        {
                            using (conditionScope.StartIfTrue())
                            {
                                web.CustomMasterUrl = webSettingInfo.CustomMasterUrl.Value;
                                web.Update();
                                needUpdate = true;
                            }
                        }
                    }
                    if (needUpdate)
                    {
                        context.ExecuteQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("An error occurred while updating web mastepageUrl. Web:{0}, Error:{1}", webServerRelativeUrl, ex);
            }
        }

        /// <summary>
        ///  Update the current item properties in the _catalogs/design list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="web"></param>
        /// <param name="themeColorURL">it must be /_catalogs/theme/15/palette0xx.spcolor, otherwise, the current itme would not show in the site setting-> change look and feel page.</param>
        /// <param name="themeFontURL">nullable</param>
        /// <param name="themeImageURL">nullable, retrieve image url from the themedcssfolder </param>
        /// <param name="needRetrieveThemedFiles"></param>
        private void ResetCurrentItemOfDesignCatalog(ClientContext context, Web web, string themeColorURL, string themeFontURL, string themeImageURL, bool needRetrieveThemedFiles=false)
        {
            try
            {
                List list = web.GetCatalog((int)ListTemplateType.DesignCatalog);
                if (needRetrieveThemedFiles)
                {
                    context.Load(web, w => w.ThemedCssFolderUrl);
                    context.ExecuteQuery();
                    ClientFolder themedCssFolder = context.Site.RootWeb.GetFolderByServerRelativeUrl(web.ThemedCssFolderUrl);
                    context.Load(themedCssFolder, fo => fo.Files.Include(f => f.Exists, f => f.ServerRelativeUrl));
                    context.ExecuteQuery();
                    var themedImageFile = themedCssFolder.Files.FirstOrDefault(f => f.ServerRelativeUrl.Substring(f.ServerRelativeUrl.LastIndexOf('.')).StartsWith(".themed", StringComparison.OrdinalIgnoreCase));
                    if (themedImageFile != null && themedImageFile.Exists)
                    {
                        themeImageURL = themedImageFile.ServerRelativeUrl;
                    }
                }
                CamlQuery camlQuery = new CamlQuery();
                //ADO-51026
                camlQuery.ViewXml = "<View>" +
                                   "<Query><Where>" +
                                   "<Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq>" +
                                   "</Where></Query>" +
                               "</View>";
                camlQuery.DatesInUtc = true;
                ListItemCollection items = list.GetItems(camlQuery);
                context.Load(items, its => its.Include(it => it.DisplayName));
                context.ExecuteQuery();
                if (items.Count == 1)
                {
                    //item["MasterPageUrl"] = "";
                    items[0]["ThemeUrl"] = themeColorURL;
                    items[0]["FontSchemeUrl"] = themeFontURL;
                    items[0]["ImageUrl"] = themeImageURL;
                    items[0].Update();
                    context.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn($"An error occurred while ResetCurrentItemOfDesignCatalog, error:{ex.ToString()}");
            }
        }

        /// <summary>
        /// Actually this method can apply both classical and modern experience themes.  
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="modernThemeInfo"></param>
        private void ApplyModernTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, AveModernThemeInfo modernThemeInfo)
        {
            try
            {
                var content = FormatOutput.Process(modernThemeInfo);
                mLogger.Info("ModernThemeInfo:{0}", content);
                Tuple<string, string, string> themedFileUrls = null;
                List tempThemedList = null;
                // Folder Struct _catalogs/theme/guid or siteassets/guid
                string themedSubfolderName = Guid.NewGuid().ToString("N");
                using (ClientContext context = CreateContext())
                {
                    try
                    {
                        tempThemedList = context.Site.RootWeb.GetCatalog((int)AveListTemplateType.ThemeCatalog);
                        themedFileUrls = RetrievehemedFiles(context, modernThemeInfo, tempThemedList, themedSubfolderName);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("An error occurred while uploading themed related files to _catalogs/theme, error:{0}", ex);
                        try
                        {
                            //Handle root site (relative Url is "/")
                            string siteAssetsUrl = "/" + (string.IsNullOrEmpty(siteServerRelativeUrl) ? webServerRelativeUrl + "/SiteAssets" : siteServerRelativeUrl + "/SiteAssets").TrimStart('/');
                            tempThemedList = context.Site.RootWeb.GetList(siteAssetsUrl);
                            themedFileUrls = RetrievehemedFiles(context, modernThemeInfo, tempThemedList, themedSubfolderName);
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("An error occurred while uploading themed related files to Site Assets, webServerRelativeUrl:{0}, error:{1}", webServerRelativeUrl, e);
                        }
                    }
                    if (themedFileUrls != null && !string.IsNullOrEmpty(themedFileUrls.Item1))
                    {
                        mLogger.Info("apply theme for web:{0} with color:{1}, font:{2}, image:{3}", webServerRelativeUrl, themedFileUrls.Item1, themedFileUrls.Item2, themedFileUrls.Item3);
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        web.ApplyTheme(themedFileUrls.Item1, themedFileUrls.Item2, themedFileUrls.Item3, true);
                        DeleteTempThemedFolder(context, tempThemedList, themedSubfolderName, themedFileUrls);
                        context.ExecuteQuery();
                        ResetCurrentItemOfDesignCatalog(context, web, webSettingInfo.ThemedColorUrl.Value, webSettingInfo.ThemedFontUrl?.Value, webSettingInfo.ThemedImageUrl?.Value, true);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occurred while restoring modern theme. webServerRelativeUrl:{0}, error:{1}", webServerRelativeUrl, ex);
            }
        }

        /// <summary>
        /// Retrieve the themed file urls if files don't exist upload the themed file to the target list, (currently the taget list are _catalogs/theme > siteUrl/siteassets )
        /// </summary>
        /// <param name="context"></param>
        /// <param name="modernThemeInfo"></param>
        /// <param name="targetList"></param>
        /// <returns>the server relative url of the 3 themed related files</returns>
        private Tuple<string, string, string> RetrievehemedFiles(ClientContext context, AveModernThemeInfo modernThemeInfo, List targetList, string themedSubfolderName)
        {
            #region Declare private variables
            ClientFolder folder = null;
            string themedColorFileName = null;
            string themedFontFileName = null;
            string themedImgFileName = null;
            Stream themedColorContent = null;
            Stream themedFontContent = null;
            Stream themedImageContent = null;
            Tuple<ClientFile, ClientFile> themedColorFiles = null;
            Tuple<ClientFile, ClientFile> themedFontFiles = null;
            Tuple<ClientFile, ClientFile> themedImgFiles = null;
            string themedColorFileUrl = null;
            string themedFontFileUrl = null;
            string themedImgFileUrl = null;
            #endregion
            try
            {
                InitThemedFileInfo(modernThemeInfo.ThemedColorUrl, modernThemeInfo.ThemedColorContent, ref themedColorFileName, ref themedColorContent);
                InitThemedFileInfo(modernThemeInfo.ThemedFontUrl, modernThemeInfo.ThemedFontContent, ref themedFontFileName, ref themedFontContent);
                InitThemedFileInfo(modernThemeInfo.ThemedImageUrl, modernThemeInfo.ThemedImageContent, ref themedImgFileName, ref themedImageContent);
                var eScope = new ExceptionHandlingScope(context);
                using (eScope.StartScope())
                {
                    using (eScope.StartTry())
                    {
                        targetList.RootFolder.AddSubFolderUsingPath(ResourcePath.FromDecodedUrl(themedSubfolderName));
                    }
                    using (eScope.StartFinally())
                    {
                        folder = targetList.RootFolder.Folders.GetByPath(ResourcePath.FromDecodedUrl(themedSubfolderName));
                        themedColorFiles = CreateFileIfNeeded(context, folder, modernThemeInfo.ThemedColorUrl, themedColorFileName, themedColorContent);
                        themedFontFiles = CreateFileIfNeeded(context, folder, modernThemeInfo.ThemedFontUrl, themedFontFileName, themedFontContent);
                        themedImgFiles = CreateFileIfNeeded(context, folder, modernThemeInfo.ThemedImageUrl, themedImgFileName, themedImageContent);
                    }
                }
                context.ExecuteQuery();
                themedColorFileUrl = GetFileUrl(themedColorFiles);
                themedFontFileUrl = GetFileUrl(themedFontFiles);
                themedImgFileUrl = GetFileUrl(themedImgFiles);
            }
            finally
            {
                #region Dispose Stream content
                if (themedColorContent != null)
                {
                    themedColorContent.Dispose();
                }
                if (themedFontContent != null)
                {
                    themedFontContent.Dispose();
                }
                if (themedImageContent != null)
                {
                    themedImageContent.Dispose();
                }
                #endregion
            }
            return new Tuple<string, string, string>(themedColorFileUrl, themedFontFileUrl, themedImgFileUrl);
        }

        private void InitThemedFileInfo(string fileUrl, byte[] fileBytes, ref string fileName, ref Stream fileContent)
        {
            if (!string.IsNullOrEmpty(fileUrl))
            {
                fileName = GetThemedFileName(fileUrl);
                if (fileBytes != null)
                {
                    fileContent = new MemoryStream(fileBytes);
                }
            }
        }

        private string GetThemedFileName(string tempFileUrl)
        {
            var index = tempFileUrl.IndexOf('?');
            if (index > 0)
            {
                tempFileUrl = tempFileUrl.Substring(0, index);
            }
            index = tempFileUrl.LastIndexOf('/');
            return tempFileUrl.Substring(index).TrimStart('/');
        }
        private Tuple<ClientFile, ClientFile> CreateFileIfNeeded(ClientContext context, Folder folder, string fileRelativeUrl, string fileName, Stream fileStream)
        {
            if (string.IsNullOrEmpty(fileRelativeUrl) || string.IsNullOrEmpty(fileName) || fileStream == null)
            {
                return null;
            }
            ClientFile tmpfile = null;
            ClientFile newCreateFile = null;
            MemoryStream copiedStream = new MemoryStream();
            fileStream.CopyTo(copiedStream);
            copiedStream.Position = 0;
            var eScope = new ExceptionHandlingScope(context);
            using (eScope.StartScope())
            {
                using (eScope.StartTry())
                {
                    tmpfile = context.Site.RootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileRelativeUrl)); //This may throw exception, if cannot get file, add the file in catch clause
                    var conditionScope = new ConditionalScope(context, () => tmpfile.Exists == true, true);
                    using (conditionScope.StartScope())
                    {
                        using (conditionScope.StartIfTrue())
                        {
                            context.Load(tmpfile, f => f.ServerRelativeUrl);
                        }
                        using (conditionScope.StartIfFalse())
                        {
                            newCreateFile = folder.Files.AddUsingPath(ResourcePath.FromDecodedUrl(fileName), new FileCollectionAddParameters() { Overwrite = true }, fileStream);
                            context.Load(newCreateFile, f => f.ServerRelativeUrl);
                        }
                    }
                }
                using (eScope.StartCatch())
                {
                    newCreateFile = folder.Files.AddUsingPath(ResourcePath.FromDecodedUrl(fileName), new FileCollectionAddParameters() { Overwrite = true }, copiedStream);
                    context.Load(newCreateFile, f => f.ServerRelativeUrl);
                }
                return new Tuple<ClientFile, ClientFile>(tmpfile, newCreateFile);
            }
        }

        private string GetFileUrl(Tuple<ClientFile, ClientFile> fileResult)
        {
            string outPutUrl = null;
            if (fileResult != null)
            {
                try
                {
                    outPutUrl = fileResult.Item1.ServerRelativeUrl;
                }
                catch
                {
                    try
                    {
                        outPutUrl = fileResult.Item2.ServerRelativeUrl;
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug($"Get file url {e.ToString()}");
                    }
                }
            }
            return outPutUrl;
        }

        private void DeleteTempThemedFolder(ClientContext context, List targetList, string themedFolderName, Tuple<string, string, string> themedFileUrls)
        {
            var escope = new ExceptionHandlingScope(context);
            using (escope.StartScope())
            {
                using (escope.StartTry())
                {
                    var themedFolder = targetList.RootFolder.Folders.GetByPath(ResourcePath.FromDecodedUrl(themedFolderName));
                    themedFolder.DeleteObject();
                }
                using (escope.StartCatch())
                {
                    if (!string.IsNullOrEmpty(themedFileUrls.Item1))
                    {
                        var tmpColorFile = context.Site.RootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(themedFileUrls.Item1));
                        tmpColorFile.DeleteObject();
                    }
                    if (!string.IsNullOrEmpty(themedFileUrls.Item2))
                    {
                        var tmpFontFile = context.Site.RootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(themedFileUrls.Item2));
                        tmpFontFile.DeleteObject();
                    }
                    if (!string.IsNullOrEmpty(themedFileUrls.Item3))
                    {
                        var tmpImgFile = context.Site.RootWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(themedFileUrls.Item3));
                        tmpImgFile.DeleteObject();
                    }
                    var themedFolder = targetList.RootFolder.Folders.GetByPath(ResourcePath.FromDecodedUrl(themedFolderName));
                    themedFolder.DeleteObject();
                }
            }
        }
        #endregion

        public Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo)
        {
            throw new NotImplementedException();
        }
        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            mRequestCommon.RestoreMasterPage(webServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
        }
        #endregion

        #region Recycle

        public void RecycleFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                file.Versions.RecycleByID(id);
                context.ExecuteQuery();
            }
        }
        public void RecycleFileVersionByIdList(string webServerRelativeUrl, string fileServerRelativeUrl, List<int> ids)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                foreach (var id in ids)
                {
                    file.Versions.RecycleByID(id);
                }
                context.ExecuteQuery();
            }
        }
        public Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTile, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                item.Recycle();
                context.ExecuteQuery();
                return (Guid)item.FieldValues["GUID"];
            }
        }

        public Guid RecycleList(string webRelativeUrl, string listTitle, Guid listid)
        {
            return Guid.Empty;
        }

        public Guid RecycleList(string webServerRelativeUrl, string listName, Guid listId, int baseTemplate, string entityTypeName, string templateFeatureId)
        {
            if ((listsActivatedByFeatureToSkipDeletion.ContainsKey(templateFeatureId) && listsActivatedByFeatureToSkipDeletion[templateFeatureId].Contains(entityTypeName))
                || (systempListsToSkipDeletion.ContainsKey(entityTypeName) && systempListsToSkipDeletion[entityTypeName] == baseTemplate))
            {
                return Guid.Empty;
            }
            bool retried = false;
            while (true)
            {
                try
                {
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        context.Load(list, l => l.ItemCount, l => l.Title, l => l.BaseType, l => l.Id);
                        context.Load(list, l => l.RootFolder.ServerRelativeUrl);
                        context.ExecuteQuery();
                        string folderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                        if (list.ItemCount > 5000)
                        {
                            DeleteList(webServerRelativeUrl, list.Title, list.Id, list.BaseTemplate, list.EntityTypeName, list.TemplateFeatureId.ToString(), true);
                            //DeleteLargeList(context, web, list, folderServerRelativeUrl, webServerRelativeUrl, list.BaseType == BaseType.DocumentLibrary);
                        }
                        else
                        {
                            list.Recycle();
                            context.ExecuteQuery();
                        }
                    }
                    break;
                }
                catch (WebException e)
                {
                    if (e.Message.ToLower().Contains("the operation has timed out") && !retried)
                    {
                        retried = true;
                        continue;
                    }

                    mLogger.Warn("Failed to recycle list, error detail : {0}", e.ToString());
                    return Guid.Empty;
                }
                catch (ServerException se)
                {
                    mLogger.Warn("Failed to recycle list, error detail : {0}", se.ToString());
                    if (se.ServerErrorCode != -2146233088)      //list does not exist
                    {
                        return Guid.Empty;
                    }
                }
            }
            return listId;
        }

        public Guid RecycleFolder(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            List<string> foldersToSkip = new List<string>() { "Lists/PublishedFeed" };     //these folder could not be deleted
            foreach (string folderToSkip in foldersToSkip)
            {
                if (folderServerRelativeUrl.Contains(folderToSkip))
                {
                    return Guid.Empty;
                }
            }

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                ClientResult<Guid> result = folder.Recycle();
                context.ExecuteQuery();
                return result.Value;
            }
        }
        #endregion

        #region private method
        private void LoadWeb(Web web, ClientContext context)
        {
            ConditionalScope parentWebCondition = new ConditionalScope(context, () => web.ParentWeb == null);
            using (parentWebCondition.StartScope())
            {
                using (parentWebCondition.StartIfFalse())
                {
                    context.Load(web, w => w.ParentWeb.Id, w => w.ParentWeb.ServerRelativeUrl);
                }
            }
            ConditionalScope memberGroupCondition = new ConditionalScope(context, () => web.AssociatedMemberGroup == null);
            using (memberGroupCondition.StartScope())
            {
                using (memberGroupCondition.StartIfFalse())
                {
                    context.Load(web, w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.AllowMembersEditMembership, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType);
                }
            }
            ConditionalScope ownerGroupCondition = new ConditionalScope(context, () => web.AssociatedOwnerGroup == null);
            using (ownerGroupCondition.StartScope())
            {
                using (ownerGroupCondition.StartIfFalse())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.AllowMembersEditMembership, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType);
                }
            }
            ConditionalScope visitorGroupCondition = new ConditionalScope(context, () => web.AssociatedVisitorGroup == null);
            using (visitorGroupCondition.StartScope())
            {
                using (visitorGroupCondition.StartIfFalse())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup, w => w.AssociatedVisitorGroup.AllowMembersEditMembership, w => w.AssociatedVisitorGroup.Owner.Id, w => w.AssociatedVisitorGroup.Owner.PrincipalType);
                }
            }
            context.Load(web);
            context.Load(web, w => w.CurrentUser);
            context.Load(web, w => w.RootFolder);
            context.Load(web, w => w.AllProperties);
            context.Load(web, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch);
            context.Load(web, w => w.AllowDesignerForCurrentUser, w => w.HasUniqueRoleAssignments);
            context.Load(web, w => w.RequestAccessEmail);
            context.Load(web, w => w.UseAccessRequestDefault);
            context.Load(web, w => w.MembersCanShare);
            context.Load(web, w => w.AccessRequestSiteDescription);
            context.Load(web, w => w.SupportedUILanguageIds, w => w.NoCrawl, w => w.ExcludeFromOfflineClient, w => w.AllowAutomaticASPXPageIndexing, w => w.SiteLogoDescription);
            context.Load(web, w => w.ThemedCssFolderUrl);
            LoadUserResource(context.Site.RootWeb.TitleResource);
            LoadUserResource(context.Site.RootWeb.DescriptionResource);
        }

        private Dictionary<string, object> ObjectToDicValue(object Object, Type type)
        {
            Dictionary<string, object> DicProperties = new Dictionary<string, object>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.Name == "DefinitionId")
                {
                    DicProperties[property.Name] = property.GetGetMethod().Invoke(Object, null);
                }
            }
            return DicProperties;
        }
        public static ListItem InternUpdate(List list, int itemid, Dictionary<string, object> itemProperties)
        {
            Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;

            bool minorversionEnabled = itemProperties.ContainsKey("EnableMinorVersions") ? Convert.ToBoolean(itemProperties["EnableMinorVersions"]) : false;
            if (minorversionEnabled && itemFieldValues.ContainsKey("_ModerationStatus"))
            {//keep modified and editor when minor version is enabled. moderation status will not be supported
                int moderationstatus = (int)itemFieldValues["_ModerationStatus"];
                itemFieldValues.Remove("_ModerationStatus");
            }

            string itemTitle = itemFieldValues.ContainsKey("FileLeafRef") ? itemFieldValues["FileLeafRef"] as string : string.Empty;
            itemFieldValues.Remove("FileLeafRef");
            ListItem tempListItem = new ListItem(list.Context, new ObjectPathMethod(list.Context, list.Path, "GetItemById", new object[] { itemid }));

            bool changed = AveListItemRestore.SetFieldValues(ref tempListItem, itemFieldValues, false, false);
            if (changed)
            {
                IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
                values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = itemTitle });
                values = tempListItem.ValidateUpdateListItem(values, true, "", true, true, string.Empty);
            }

            return tempListItem;
        }
        private void WebGetSubwebs(ClientContext context, Web rootWeb, List<IDictionary<string, object>> webList, string siteUrl, string siteServerRelativeUrl)
        {
            LoadSubSites(context, rootWeb);
            context.ExecuteQuery();
            foreach (Web web in rootWeb.Webs)
            {
                //if (IsApplicationWeb(web))
                //{
                //    continue;
                //}
                Dictionary<string, object> dicWeb = new Dictionary<string, object>();
                dicWeb = GetWebProperties(context, web, siteUrl, siteServerRelativeUrl, true);
                webList.Add(dicWeb);
                WebGetSubwebs(context, web, webList, siteUrl, siteServerRelativeUrl);
            }
        }

        private List<IDictionary<string, object>> NavigationNodeCollectionToList(NavigationNodeCollection nodes, Dictionary<string, object> nodesProp)// Dictionary<string, string> nodeTypes, Dictionary<string, string> navigationUrls)
        {
            var returnPropeties = new List<IDictionary<string, object>>();
            foreach (NavigationNode node in nodes)
            {
                Dictionary<string, object> nodeDic = new Dictionary<string, object>();
                CopyProperty(nodeDic, node);

                var childNodeList = new List<IDictionary<string, object>>();
                GetNavigationNodeChild(node, childNodeList, nodesProp);
                Dictionary<string, object> childNodesProperties = new Dictionary<string, object>();
                childNodesProperties.AddChildren(childNodeList);
                nodeDic["Children" + AveObjectModelConstant.ObjectPropertySuffix] = childNodesProperties;
                nodeDic["Id" + AveObjectModelConstant.ObjectPropertySuffix] = node.Path;
                nodeDic["ClientContext"] = node.Context;
                nodeDic["IsExternal"] = node.IsExternal;
                if (nodesProp != null)
                {
                    foreach (KeyValuePair<string, object> pair in nodesProp)
                    {
                        //Navigation的pair.Key获取的是用逗号分隔的两个参数，所以需要逗号加nodeid比较。
                        //SearchNavigation的pair.Key获取的是一个参数，所以不需要加逗号比较。
                        if (pair.Key.Contains("," + node.Id.ToString()) || pair.Key.Equals(node.Id.ToString()))
                        {
                            Dictionary<string, object> nodeProp = pair.Value as Dictionary<string, object>;
                            nodeDic["Url"] = nodeProp["NodeUrl"].ToString();
                            nodeDic["NodeType"] = nodeProp["NodeType"].ToString();
                            nodeDic["Description"] = nodeProp["Description"].ToString();
                            nodeDic["Audience"] = nodeProp["Audience"].ToString();
                            //SharePoint API doesn't work. Use the property in HTML.
                            nodeDic["IsVisible"] = nodeProp["IsVisible"];
                            nodeDic["Target"] = nodeProp["Target"];
                            break;
                        }
                    }
                }
                returnPropeties.Add(nodeDic);
            }
            return returnPropeties;
        }
        private void GetNavigationNodeChild(NavigationNode node, List<IDictionary<string, object>> dic, Dictionary<string, object> nodesProp)// Dictionary<string, string> nodeTypes, Dictionary<string, string> navigationUrls)
        {
            node.Context.Load(node.Children);
            node.Context.ExecuteQuery();
            foreach (NavigationNode childNode in node.Children)
            {
                Dictionary<string, object> nodeDic = new Dictionary<string, object>();
                //AveObjectCopy.GetObjectBasicProperties(nodeDic, childNode);
                var childNodeList = new List<IDictionary<string, object>>();
                GetNavigationNodeChild(childNode, childNodeList, nodesProp);
                Dictionary<string, object> childNodesProperties = new Dictionary<string, object>();
                childNodesProperties.AddChildren(childNodeList);
                CopyProperty(nodeDic, childNode);
                nodeDic["Children" + AveObjectModelConstant.ObjectPropertySuffix] = childNodesProperties;
                nodeDic["Id" + AveObjectModelConstant.ObjectPropertySuffix] = childNode.Path;
                nodeDic["ClientContext"] = childNode.Context;
                if (!string.IsNullOrEmpty(childNode.Url))
                {
                    if (childNode.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        childNode.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        childNode.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    {
                        nodeDic["IsExternal"] = !childNode.Url.StartsWith(this.WebAppName, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        nodeDic["IsExternal"] = false;
                    }
                }
                else
                {
                    nodeDic["IsExternal"] = true;
                }
                if (nodesProp != null)
                {
                    foreach (KeyValuePair<string, object> pair in nodesProp)
                    {
                        if (pair.Key.Contains("," + childNode.Id.ToString()))
                        {
                            Dictionary<string, object> nodeProp = pair.Value as Dictionary<string, object>;
                            nodeDic["Url"] = nodeProp["NodeUrl"].ToString();
                            nodeDic["NodeType"] = nodeProp["NodeType"].ToString();
                            nodeDic["Description"] = nodeProp["Description"].ToString();
                            nodeDic["Audience"] = nodeProp["Audience"].ToString();
                            //SharePoint API doesn't work. Use the property in HTML.
                            nodeDic["IsVisible"] = nodeProp["IsVisible"];
                            break;
                        }
                    }
                }
                dic.Add(nodeDic);
            }
        }
        private Microsoft.SharePoint.Client.View FindView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, ClientContext context)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            List list = web.Lists.GetById(listId);
            View view = null;
            try
            {
                view = list.Views.GetById(viewId);
                context.Load(view);
                context.Load(view, v => v.ViewFields);
                context.ExecuteQuery();
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while finding view.Message:{0}.", ex.ToString());
                view = null;
            }
            return view;
        }

        public static void AppendFieldValues(IDictionary<string, object> fieldValues, IDictionary<string, object> fieldValuesToAppend)
        {
            if (fieldValues.Count > 0)
            {
                foreach (var kv in fieldValuesToAppend)
                {
                    if (!fieldValues.ContainsKey(kv.Key))
                    {
                        fieldValues.Add(kv.Key, kv.Value);
                    }
                }
            }
            else
            {
                fieldValues = fieldValuesToAppend;
            }
        }
        public static void GetItemDic(IDictionary<string, object> itemProperties, ListItem item)
        {
            //require object has been initialized
            CopyProperty(itemProperties, item);
            if (item.FieldValues.Count > 0)
            {
                IDictionary<string, object> fieldValues = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> fieldValue in item.FieldValues)
                {
                    AssembleItemProperties(fieldValues, fieldValue.Value, fieldValue.Key);
                }
                if (itemProperties.ContainsKey("FieldValues"))
                {
                    AppendFieldValues(itemProperties["FieldValues"] as IDictionary<string, object>, fieldValues);
                }
                else
                {
                    itemProperties["FieldValues"] = fieldValues;
                }
                if (fieldValues.ContainsKey("MyEditor") && !fieldValues.ContainsKey("Editor"))
                {
                    fieldValues["Editor"] = fieldValues["MyEditor"];
                }
                if (fieldValues.ContainsKey("MyAuthor") && !fieldValues.ContainsKey("Author"))
                {
                    fieldValues["Author"] = fieldValues["MyAuthor"];
                }
                //item properties
                foreach (KeyValuePair<string, object> pair in item.FieldValues)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    if (pair.Value.GetType().FullName == "System.String")
                    {
                        if (string.IsNullOrEmpty(pair.Value.ToString()))
                        {
                            continue;
                        }
                    }
                    switch (pair.Key)
                    {
                        case "FileRef":
                            itemProperties["ServerRelativeUrl"] = pair.Value.ToString();
                            itemProperties["FullUrl"] = pair.Value.ToString();
                            break;
                        case "File_x0020_Size":
                            itemProperties["Length"] = long.Parse(pair.Value.ToString());
                            break;
                        case "Title":
                            itemProperties["Title"] = pair.Value.ToString();
                            break;
                        case "Created":
                            itemProperties["TimeCreated"] = pair.Value;
                            break;
                        case "Modified":
                            itemProperties["TimeLastModified"] = pair.Value;
                            break;
                        case "FSObjType":
                            itemProperties["FileSystemObjectType"] = int.Parse(pair.Value.ToString());
                            break;
                        case "ID":
                            itemProperties["Id"] = pair.Value;
                            itemProperties["ID"] = pair.Value;
                            break;
                        case "_UIVersionString":
                            itemProperties["UIVersionString"] = pair.Value.ToString();
                            break;
                        case "_UIVersion":
                            itemProperties["UIVersion"] = int.Parse(pair.Value.ToString());
                            break;
                        case "_Level":
                            itemProperties["Level"] = byte.Parse(pair.Value.ToString());
                            break;
                        case "ContentTypeId":
                            itemProperties["ContentTypeId"] = pair.Value.ToString();
                            break;
                        case "Attachments":
                            itemProperties["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = pair.Value.ToString();
                            break;
                        case "Url":
                            break;
                        case "FileLeafRef":
                            if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
                            {
                                if ((pair.Value as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                                {
                                    itemProperties["Name"] = item.FieldValues["Title"];
                                    if (itemProperties["Name"] == null)
                                    {
                                        itemProperties["Name"] = "";
                                    }
                                }
                                else
                                {
                                    itemProperties["Name"] = pair.Value.ToString();
                                }
                            }
                            else
                            {
                                itemProperties["Name"] = pair.Value.ToString();
                            }
                            itemProperties["LeafName"] = pair.Value.ToString();
                            break;
                        case "UniqueId":
                            itemProperties["DocId"] = pair.Value;
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        case "GUID":
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        case "_IsCurrentVersion":
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        case "FileDirRef":
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        case "ParentUniqueId":
                            itemProperties["ParentID"] = Guid.Parse(pair.Value.ToString());
                            break;
                        default:
                            //itemProperties[pair.Key] = pair.Value;
                            break;
                    }
                }

            }
        }

        private ContentTypeCollection GetContentTypesWithoutFields(ClientContext context, string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            ContentTypeCollection contentTypes = null;
            switch (contentTypeSource)
            {
                case "web.availableContentTypes":
                    contentTypes = web.AvailableContentTypes;
                    break;
                case "web.contentTypes":
                    contentTypes = web.ContentTypes;
                    break;
                case "list.contentTypes":
                    List list = web.Lists.GetById(listId);
                    contentTypes = list.ContentTypes;
                    break;
                default:
                    break;
            }
            context.Load(contentTypes, cts => cts.IncludeWithDefaultProperties(ct => ct.Id, ct => ct.Parent.Id, ct => ct.SchemaXml, ct => ct.WorkflowAssociations));//cts => cts.IncludeWithDefaultProperties(ct => ct.Fields, ct => ct.FieldLinks));
            return contentTypes;
        }
 
        private ContentType GetContentTypeWithoutFields(ClientContext context, string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, string contentTypeId)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            ContentTypeCollection cts = null;
            ContentType contentType = null;
            switch (contentTypeSource)
            {
                case "web.availableContentTypes":
                    cts = web.AvailableContentTypes;
                    contentType = cts.GetById(contentTypeId);
                    break;
                case "web.contentTypes":
                    cts = web.ContentTypes;
                    contentType = cts.GetById(contentTypeId);
                    break;
                case "list.contentTypes":
                    List list = web.Lists.GetById(listId);
                    contentType = list.ContentTypes.GetById(contentTypeId);
                    break;
                default:
                    break;
            }
            context.Load(contentType, c => c.Id, c => c.SchemaXml, c => c.FieldLinks, c => c.WorkflowAssociations);
            return contentType;
        }

        private static void LoadUserResource(UserResource userResource)
        {
            if (AveUserResourceExtension.SupportedResourceCultureNames != null
                && AveUserResourceExtension.SupportedResourceCultureNames.Count > 0)
            {
                foreach (var languageName in AveUserResourceExtension.SupportedResourceCultureNames)
                {
                    userResource.GetValueForUICulture(languageName);
                }
            }
        }

        private Dictionary<string, object> GetWebProperties(ClientContext context, Web web, string siteUrl, string siteServerRelativeUrl, bool webLoaded)
        {
            try
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                if (!webLoaded)
                {
                    context.Load(context.Site.RootWeb);
                    LoadWeb(web, context);
                    LoadUserResource(web.TitleResource);
                    LoadUserResource(web.DescriptionResource);
                    context.ExecuteQuery();
                }
                CopyProperty(webProperties, web);
                CopyUserResourceProperty(webProperties, web);
                webProperties["Exists"] = true;
                webProperties["IsAppWeb"] = web.AppInstanceId != Guid.Empty;
                webProperties["CurrentUser" + AveObjectModelConstant.ObjectPropertySuffix] = web.CurrentUser.LoginName;
                //webProperties.Add("IsPublish", false);

                webProperties["SupportedUILanguageIds"] = web.SupportedUILanguageIds;
                webProperties[WebPropertyNames.MembersCanShare] = web.MembersCanShare;
                webProperties[WebPropertyNames.AccessRequestSiteDescription] = web.AccessRequestSiteDescription;
                webProperties[WebPropertyNames.RequestAccessEmail] = web.RequestAccessEmail;
                webProperties[WebPropertyNames.UseAccessRequestDefault] = web.UseAccessRequestDefault;

                webProperties["Url"] = web.Url;
                bool IsRootWeb = true;
                string Name = string.Empty;
                string parentWebServerRelativeUrl = string.Empty;
                Guid parentWebId = Guid.Empty;
                if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    IsRootWeb = false;//isRootWeb
                    int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/') + 1;
                    Name = web.ServerRelativeUrl.Substring(lastSlashIndex);
                    parentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex - 1);
                    parentWebServerRelativeUrl = string.IsNullOrEmpty(parentWebServerRelativeUrl) ? "/" : parentWebServerRelativeUrl;
                    if (web.ParentWeb.IsPropertyAvailable("Id"))
                    {
                        parentWebId = web.ParentWeb.Id;
                    }
                }
                webProperties["IsRootWeb"] = IsRootWeb;
                // The value of HasUniqueRoleDefinitions in RootWeb is true.
                webProperties["HasUniqueRoleDefinitions"] = IsRootWeb;
                // Add RootWeb Id
                webProperties["FirstUniqueRoleDefinitionWeb" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.RootWeb.Id;
                webProperties["Name"] = Name;
                webProperties["ParentWeb" + AveObjectModelConstant.ObjectPropertySuffix] = parentWebServerRelativeUrl;
                webProperties["ParentWebId" + AveObjectModelConstant.ObjectPropertySuffix] = parentWebId;
                string webTemplate = string.Empty;
                //对web的template和configuration进行赋值
                //string configuration = AveWebServiceRequest.GetWebTemplateConfiguration(this.WebAppName, web.ServerRelativeUrl, this.mObj);
                //string[] datas = configuration.Split('#');
                //if (datas.Length == 2)
                //{
                //    webProperties["WebTemplate"] = datas[0];
                //    webProperties["Configuration"] = short.Parse(datas[1]);
                //}
                webProperties["WebTemplate"] = web.WebTemplate;
                webProperties["Configuration"] = web.Configuration;
                webProperties["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] = web.AllProperties.FieldValues;
                Dictionary<string, object> AssociatedMemberGroupProperties = GetGroupProperties(context, web.AssociatedMemberGroup, true);
                Dictionary<string, object> AssociatedOwnerGroupProperties = GetGroupProperties(context, web.AssociatedOwnerGroup, true);
                Dictionary<string, object> AssociatedVisitorGroupProperties = GetGroupProperties(context, web.AssociatedVisitorGroup, true);
                webProperties["AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedMemberGroupProperties;
                webProperties["AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedOwnerGroupProperties;
                webProperties["AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedVisitorGroupProperties;
                //webProperties["SiteLogoDescription"] = GetWebLogoProperties(web.ServerRelativeUrl)["SiteLogoDescription"]; //SAAS-20894 在获取Web Properties的时候直接获取Logo Description，没有Site Assets的可以直接创建出来
                mRequestCommon.GetWebSearchAndOfflineAvailability(web.ServerRelativeUrl, webProperties, tokenProvider);
                //#if DEBUG
                //            if (webProperties.ContainsKey("NoCrawl") == false || webProperties.ContainsKey("ExcludeFromOfflineClient") == false)
                //            {
                //                mLogger.Error("Missing NoCrawl...");
                //            }
                //#endif
                return webProperties;
            }
            catch (Microsoft.SharePoint.Client.ServerException mse)
            {
                mLogger.Info($"GetSite failed with ServerException.Message:{mse.Message}." +
                    $"ServerErrorCode:{mse.ServerErrorCode}." +
                    $"ServerErrorDetails:{mse.ServerErrorDetails}." +
                    $"ServerErrorTraceCorrelationId:{mse.ServerErrorTraceCorrelationId}." +
                    $"ServerErrorTypeName:{mse.ServerErrorTypeName}." +
                    $"ServerErrorValue:{mse.ServerErrorValue}." +
                    $"ServerStackTrace:{mse.ServerStackTrace}." +
                    $"Source:{mse.Source}." +
                    $"StackTrace:{mse.StackTrace}.");
                throw;
            }
        }
        private void AssembleRoleAssignmetsProperites(Dictionary<string, object> roleAssignmentsProperties, RoleAssignmentCollection roleAssignmentCollection)
        {
            CopyProperty(roleAssignmentsProperties, roleAssignmentCollection);
            var roleAssignmentPropertiesList = new List<IDictionary<string, object>>(roleAssignmentCollection.Count);
            foreach (RoleAssignment roleAssignment in roleAssignmentCollection)
            {
                Dictionary<string, object> roleAssignemntProperties = new Dictionary<string, object>();
                AssembleRoleAssignmetProperites(roleAssignemntProperties, roleAssignment);
                roleAssignmentPropertiesList.Add(roleAssignemntProperties);
            }
            roleAssignmentsProperties.AddChildren(roleAssignmentPropertiesList);
        }
        private void AssembleRoleAssignmetProperites(Dictionary<string, object> roleAssignemntProperties, RoleAssignment roleAssignment)
        {
            CopyProperty(roleAssignemntProperties, roleAssignment);
            //Principal member = roleAssignment.Member;
            //roleAssignemntProperties.Add("MemberLoginName", member.LoginName);
            //roleAssignemntProperties.Add("MemberType", member.PrincipalType == PrincipalType.User ? "User" : "Group");
            //if (member.PrincipalType == PrincipalType.User)
            //{
            //    roleAssignemntProperties.Add("MemberType", "User");
            //}
            //else if (member.PrincipalType == PrincipalType.SecurityGroup)
            //{
            //    roleAssignemntProperties.Add("MemberType", "SecurityGroup");
            //}
            //else
            //{
            //    roleAssignemntProperties.Add("MemberType", "Group");
            //}
            //Dictionary<string, object> roleDefinitionBindingCollectionProperties = new Dictionary<string, object>();
            //AssembleRoleDefinitionBindingsProperties(roleDefinitionBindingCollectionProperties, roleAssignment.RoleDefinitionBindings);
            var bindings = new List<int>();
            foreach (var item in roleAssignment.RoleDefinitionBindings)
            {
                bindings.Add(item.Id);
            }

            roleAssignemntProperties.Add("RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix, bindings);
        }

        private void AssembleRoleDefinitionsProperties(Dictionary<string, object> roleDefinitionsProperties, string webServerRelativeUrl, RoleDefinitionCollection roleDefinitionCollection)
        {
            var roleDefinitionPropertiesList = new List<IDictionary<string, object>>();
            foreach (RoleDefinition roleDefinition in roleDefinitionCollection)
            {
                Dictionary<string, object> roleDefinitionProperties = new Dictionary<string, object>();
                CopyProperty(roleDefinitionProperties, roleDefinition);
                roleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
                roleDefinitionProperties[AveObjectModelConstant.WebServerRelativeUrl] = webServerRelativeUrl;
                roleDefinitionProperties["Type"] = (int)roleDefinition.RoleTypeKind;
                roleDefinitionPropertiesList.Add(roleDefinitionProperties);
            }
            roleDefinitionsProperties.AddChildren(roleDefinitionPropertiesList);
        }
        private void AssembleRoleDefinitionProperties(Dictionary<string, object> roleDefinitionProperties, string webServerRelativeUrl, RoleDefinition roleDefinition)
        {
            CopyProperty(roleDefinitionProperties, roleDefinition);
            roleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
            roleDefinitionProperties[AveObjectModelConstant.WebServerRelativeUrl] = webServerRelativeUrl;
        }

        public static void AssembleBasicFileProperties(Dictionary<string, object> fileProperties, ClientFile file, string webServerRelativeUrl)
        {
            string url = string.Empty;
            url = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            fileProperties["Url"] = url;
            string parentFolderServerRelativeUrl = file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf('/'));
            if (string.IsNullOrEmpty(parentFolderServerRelativeUrl))
            {
                parentFolderServerRelativeUrl = "/";
            }
            fileProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderServerRelativeUrl;
            //if (file.IsObjectPropertyInstantiated("CheckedOutByUser") && file.CheckedOutByUser.IsPropertyAvailable("LoginName"))
            //{
            //    string checkedUserName = file.CheckedOutByUser.LoginName;
            //    fileProperties["CheckedOutByUser" + AveObjectModelConstant.ObjectPropertySuffix] = checkedUserName;
            //}
            if (file.IsObjectPropertyInstantiated("Author") && file.Author.IsPropertyAvailable("LoginName"))
            {
                string checkedUserName = file.Author.LoginName;
                fileProperties["Author" + AveObjectModelConstant.ObjectPropertySuffix] = checkedUserName;
            }
            if (file.IsObjectPropertyInstantiated("ModifiedBy") && file.ModifiedBy.IsPropertyAvailable("LoginName"))
            {
                fileProperties["ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix] = file.ModifiedBy.LoginName;
            }
            CopyProperty(fileProperties, file);
            fileProperties["CustomizedPageStatus"] = (int)file.CustomizedPageStatus;
        }
        public static void AssembleFileProperties(Dictionary<string, object> fileProperties, ClientFile file, string webServerRelativeUrl, ListItem item)
        {
            string url = string.Empty;
            url = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            fileProperties["Url"] = url;
            string parentFolderServerRelativeUrl = file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf('/'));
            if (string.IsNullOrEmpty(parentFolderServerRelativeUrl))
            {
                parentFolderServerRelativeUrl = "/";
            }
            fileProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderServerRelativeUrl;

            if (item != null && item.FieldValues.Count > 0)
            {
                Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                GetItemDic(itemProperties, item);
                fileProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                fileProperties["UniqueId"] = itemProperties["UniqueId"];
                if (itemProperties.ContainsKey("Length"))
                {
                    fileProperties["Length"] = itemProperties["Length"];
                }
            }
            else
            {
                try
                {
                    string[] ids = GetIdsFromEtag(file.ETag);
                    string uniqueId = ids[0];
                    if (!string.IsNullOrEmpty(uniqueId))
                    {
                        fileProperties["UniqueId"] = new Guid(uniqueId);
                    }
                    string id = ids[1];
                    if (!string.IsNullOrEmpty(id))
                    {
                        fileProperties["DocLibRowId"] = Convert.ToInt32(id);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Assemble file:{0} property Id failed.Error Message:{1}", file.ServerRelativeUrl, ex.ToString());
                }
            }
            //if (file.IsObjectPropertyInstantiated("CheckedOutByUser") && file.CheckedOutByUser.IsPropertyAvailable("LoginName"))
            //{
            //    string checkedUserName = file.CheckedOutByUser.LoginName;
            //    fileProperties["CheckedOutByUser" + AveObjectModelConstant.ObjectPropertySuffix] = checkedUserName;
            //}
            if (file.IsObjectPropertyInstantiated("Author") && file.Author.IsPropertyAvailable("LoginName"))
            {
                string checkedUserName = file.Author.LoginName;
                fileProperties["Author" + AveObjectModelConstant.ObjectPropertySuffix] = checkedUserName;
            }
            if (file.IsObjectPropertyInstantiated("ModifiedBy") && file.ModifiedBy.IsPropertyAvailable("LoginName"))
            {
                fileProperties["ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix] = file.ModifiedBy.LoginName;
            }
            CopyProperty(fileProperties, file);
            fileProperties["CustomizedPageStatus"] = (int)file.CustomizedPageStatus;
        }
        public static void AssembleFolderProperties(string webServerRelativeUrl, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> folderProp)
        {
            string Url = string.Empty;
            if (folder.ServerRelativeUrl.TrimEnd('/').Equals(webServerRelativeUrl.TrimEnd('/')))
            {
                Url = string.Empty;
            }
            else
            {
                Url = folder.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            CopyProperty(folderProp, folder);
            if ((mIsLoadDFolderId || WrapperConfiguration.WrapperConfigurationForBPOS.LoadRootFolderUniqueId) && folder.Properties.FieldValues.ContainsKey("vti_etag") && folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                Guid uniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                folderProp["UniqueId"] = uniqueId;
            }
            folderProp["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
            if (folder.Properties.FieldValues != null && folder.Properties.FieldValues.Count > 0)
            {
                Hashtable hashtable = new Hashtable();
                foreach (KeyValuePair<string, object> pair in folder.Properties.FieldValues)
                {
                    hashtable[pair.Key] = pair.Value;
                }
                folderProp["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = hashtable;
            }
            folderProp["Url"] = Url;
            if (webServerRelativeUrl.TrimEnd('/').Equals(folderServerRelativeUrl.TrimEnd('/')))
            {
                folderProp["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = null;
            }
            else
            {
                folderProp["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = AveUrlUtility.GetParentUrl(folderServerRelativeUrl);
            }
        }


        private static void AssembleItemProperties(IDictionary<string, object> props, object fieldValue, string fieldName)
        {
            if (fieldValue != null && !AveTypeHelper.IsBasicType(fieldValue))
            {
                if (fieldValue is FieldUserValue[])
                {
                    StringBuilder fieldUserValue = new StringBuilder();
                    foreach (FieldUserValue userValue in fieldValue as FieldUserValue[])
                    {
                        GetFieldLookupValue(fieldUserValue, userValue.LookupId, userValue.LookupValue);
                    }
                    if (fieldUserValue.Length > 0)
                    {
                        fieldValue = fieldUserValue.ToString(0, fieldUserValue.Length - 2);
                    }
                    else
                    {
                        fieldValue = fieldUserValue.ToString();
                    }
                }
                else if (fieldValue is FieldLookupValue[])
                {
                    StringBuilder fieldLookupValue = new StringBuilder();
                    foreach (FieldLookupValue lookupValue in fieldValue as FieldLookupValue[])
                    {
                        GetFieldLookupValue(fieldLookupValue, lookupValue.LookupId, lookupValue.LookupValue);
                    }
                    if (fieldLookupValue.Length > 0)
                    {
                        fieldValue = fieldLookupValue.ToString(0, fieldLookupValue.Length - 2);
                    }
                    else
                    {
                        fieldValue = fieldLookupValue.ToString();
                    }
                }
                else if (fieldValue is FieldUserValue)
                {
                    StringBuilder fieldUserValue = new StringBuilder();
                    FieldUserValue userValue = (fieldValue as FieldUserValue);
                    GetFieldLookupValue(fieldUserValue, userValue.LookupId, userValue.LookupValue);
                    fieldValue = fieldUserValue.ToString(0, fieldUserValue.Length - 2);
                }
                else if (fieldValue is FieldLookupValue)
                {
                    StringBuilder fieldLookupValue = new StringBuilder();
                    FieldLookupValue lookupValue = (fieldValue as FieldLookupValue);
                    GetFieldLookupValue(fieldLookupValue, lookupValue.LookupId, lookupValue.LookupValue);
                    fieldValue = fieldLookupValue.ToString(0, fieldLookupValue.Length - 2);
                }
                else if (fieldValue is FieldUrlValue)
                {
                    FieldUrlValue urlValue = (fieldValue as FieldUrlValue);
                    StringBuilder fieldUrlValue = new StringBuilder(urlValue.Url);
                    fieldUrlValue.Append(", ");
                    fieldUrlValue.Append(urlValue.Description);
                    fieldValue = fieldUrlValue.ToString();
                }
                else if (fieldValue is TaxonomyFieldValueCollection)
                {
                    fieldValue = TaxonomyFieldValueCollectionToString(fieldValue as TaxonomyFieldValueCollection);
                }
                else if (fieldValue is TaxonomyFieldValue)
                {
                    fieldValue = TaxnonmyFieldToString(fieldValue as TaxonomyFieldValue);
                }
                else if (fieldValue is FieldRatingScaleQuestionAnswer[])
                {
                    FieldRatingScaleQuestionAnswer[] answers = fieldValue as FieldRatingScaleQuestionAnswer[];
                    StringBuilder answersbuilder = new StringBuilder();
                    for (int i = 0; i < answers.Length; i++)
                    {
                        FieldRatingScaleQuestionAnswer answer = answers[i];
                        answersbuilder.Append(answer.Question);
                        answersbuilder.Append(string.Format(";#{0}#", answer.Answer));
                    }
                    fieldValue = answersbuilder.ToString();
                }
                else if (fieldValue is FieldGeolocationValue)
                {
                    FieldGeolocationValue geolocationValue = fieldValue as FieldGeolocationValue;
                    fieldValue = string.Format("Point ({0} {1} {2} {3})", geolocationValue.Longitude, geolocationValue.Latitude, geolocationValue.Altitude, geolocationValue.Measure);
                }
                else if (fieldValue is string[])
                {
                    //do nothing.
                }
                else
                {
                    fieldValue = fieldValue.ToString();
                }
            }
            //else if (fieldName.Equals("QuickAddGroups"))
            //{
            //    fieldValue = GetQuickAddGroupsProp(fieldValue as string[]);
            //}
            props[fieldName] = fieldValue;
        }

        public static string TaxnonmyFieldToString(TaxonomyFieldValue tfValue)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(tfValue.Label) || !string.IsNullOrEmpty(tfValue.TermGuid))
            {
                builder.Append(tfValue.Label);
                builder.Append("|");
                builder.Append(tfValue.TermGuid);
            }
            return builder.ToString();
        }

        public static string TaxonomyFieldValueCollectionToString(TaxonomyFieldValueCollection tfValueCollection)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (TaxonomyFieldValue value2 in tfValueCollection)
            {
                if (value2 == null)
                {
                    continue;
                }
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(';');
                }
                builder.Append(TaxnonmyFieldToString(value2));
            }
            return builder.ToString();
        }

        /*private static string GetQuickAddGroupsProp(string[] QuickAddGroups)
        {
            if (QuickAddGroups != null)
            {
                StringBuilder Groups = new StringBuilder();
                foreach (string setting in QuickAddGroups)
                {
                    Groups.Append(";#" + setting);
                }
                Groups.Append(";#");
                return Groups.ToString();
            }
            return string.Empty;
        }*/

        private static void GetFieldLookupValue(StringBuilder builder, int lookupId, string lookupValue)
        {
            builder.Append(lookupId);
            builder.Append(";#");
            builder.Append(lookupValue);
            builder.Append(";#");
        }

        public static void AssembleDiscoverItemProperties(IDictionary<string, object> listItemProperty, ListItem listItem)
        {
            CopyProperty(listItemProperty, listItem);
            listItemProperty["DocID"] = listItem.FieldValues["UniqueId"];
            if (listItem.FieldValues.ContainsKey("FileLeafRef"))
            {
                listItemProperty["LeafName"] = listItem.FieldValues["FileLeafRef"];
            }
            else
            {
                listItemProperty["LeafName"] = string.Empty;
            }
            ;
            listItemProperty["ID"] = listItem.FieldValues["ID"];
            if (listItem.FieldValues.ContainsKey("GUID"))
            {
                listItemProperty["tp_GUID"] = listItem.FieldValues["GUID"];
                listItemProperty["GUID"] = listItem.FieldValues["GUID"];
            }
            else
            {
                listItemProperty["tp_GUID"] = Guid.Empty;
                listItemProperty["GUID"] = Guid.Empty;
            }
            if (listItem.FieldValues.ContainsKey("File_x0020_Size") && listItem.FieldValues["File_x0020_Size"].ToString() != string.Empty)
            {
                listItemProperty["HasStream"] = 1;
            }
            else
            {
                listItemProperty["HasStream"] = 0;
            }

            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = listItem.FieldValues["FileRef"];
            listItemProperty["DirName"] = listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            listItemProperty["Level"] = Convert.ToByte(listItem.FieldValues["_Level"]);
            listItemProperty["UIVersion"] = listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            listItemProperty["Hidden"] = (listItemProperty["ID"] == null) ? true : false;
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = listItem.FieldValues["_IsCurrentVersion"];
        }

        public static void AssembleDiscoverWebProperties(Dictionary<string, object> webProperty, Web web, string siteServerRelativeUrl)
        {
            webProperty["WebID"] = web.Id;
            webProperty["Title"] = web.Title;
            webProperty["FullUrl"] = web.ServerRelativeUrl;
            webProperty["SubWebs"] = new Dictionary<Guid, object>();
            string name = string.Empty;
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl))
            {
                name = web.ServerRelativeUrl.Substring(web.ServerRelativeUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
            }
            else
            {
                name = ".";
            }
            webProperty["Name"] = name;
            webProperty["AppInstanceId"] = web.AppInstanceId;
        }

        public static void AssembleViewFileProperties(Dictionary<string, object> listItemProperty, ClientFile file)
        {
            CopyProperty(listItemProperty, file);
            if (!string.IsNullOrEmpty(file.ETag))
            {
                int index = file.ETag.IndexOf(',');
                string guid = file.ETag.Substring(1, index - 1);
                string id = file.ETag.Substring(index + 1, file.ETag.Length - index - 2);
                listItemProperty["GUID"] = new Guid(guid);
                listItemProperty["Id"] = default(int);//Convert.ToInt32(id);
                listItemProperty["ID"] = default(int);
            }
            listItemProperty["LeafName"] = file.Name;
            if (listItemProperty.ContainsKey("GUID"))
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = listItemProperty["GUID"];
            }
            else
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = Guid.Empty;
            }
            listItemProperty["HasStream"] = 1;
            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = file.ServerRelativeUrl;//listItem.FieldValues["FileRef"];
            listItemProperty["DirName"] = file.ServerRelativeUrl.Contains('/') ? file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf('/')) : file.Name;//listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            if (!listItemProperty.ContainsKey("FileDirRef"))
            {
                listItemProperty["FileDirRef"] = listItemProperty["DirName"];
            }
            listItemProperty["Level"] = Convert.ToByte((int)file.Level);//listItem.FieldValues["_Level"]);
            listItemProperty["UIVersion"] = file.UIVersion;//listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = file.TimeLastModified;//listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte(1);//Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)file.Tag; //(int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            if (listItemProperty.ContainsKey("Id") && listItemProperty["Id"] != null)
            {
                listItemProperty["Hidden"] = true;
            }
            else
            {
                listItemProperty["Hidden"] = false;
            }
            listItemProperty["ObjType"] = 2;
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = true;//listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = true;
        }

        public static void AssembleViewFolderProperties(Dictionary<string, object> listItemProperty, Folder folder)
        {
            CopyProperty(listItemProperty, folder);
            //if (!string.IsNullOrEmpty(file.ETag))
            //{
            //    int index = file.ETag.IndexOf(',');
            //    string guid = file.ETag.Substring(1, index - 1);
            //    string id = file.ETag.Substring(index + 1, file.ETag.Length - index - 2);
            //    listItemProperty["Guid"] = new Guid(guid);
            //    listItemProperty["ID"] = Convert.ToInt32(id);
            //}
            listItemProperty["GUID"] = Guid.Empty;
            listItemProperty["Id"] = 0;
            listItemProperty["ID"] = 0;
            listItemProperty["LeafName"] = folder.Name;
            if (listItemProperty.ContainsKey("GUID"))
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = listItemProperty["GUID"];
            }
            else
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = Guid.Empty;
            }
            listItemProperty["HasStream"] = 0;
            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = folder.ServerRelativeUrl;//listItem.FieldValues["FileRef"];
            listItemProperty["DirName"] = folder.ServerRelativeUrl.Contains('/') ? folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.LastIndexOf('/')) : folder.Name;//listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            if (!listItemProperty.ContainsKey("FileDirRef"))
            {
                listItemProperty["FileDirRef"] = listItemProperty["DirName"];
            }
            listItemProperty["Level"] = Convert.ToByte(1);//listItem.FieldValues["_Level"]);
            //代码中统一为“UIVersion”
            listItemProperty["UIVersion"] = 512;//listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = DateTime.MinValue;//listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte(1);//Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)folder.Tag; //(int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            if (listItemProperty.ContainsKey("ID") && listItemProperty["ID"] != null)
            {
                listItemProperty["Hidden"] = true;
            }
            else
            {
                listItemProperty["Hidden"] = false;
            }
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = true;//listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = true;
        }

        private void AssembleContentTypesProperties(Dictionary<string, object> contentTypesProperties, ContentTypeCollection contentTypeCol)
        {
            var contentTypePropertiesList = new List<IDictionary<string, object>>(contentTypeCol.Count);
            var flags = false;
            StringBuilder regularContentTypes = new StringBuilder("ContentTypes:\r\n");
            foreach (ContentType contentType in contentTypeCol)
            {
                if (contentType.ServerObjectIsNull.HasValue && contentType.ServerObjectIsNull.Value)
                {
                    flags = true;
                    continue;
                }
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(contentTypeProperties, contentType);
                contentTypePropertiesList.Add(contentTypeProperties);
                regularContentTypes.Append(string.Format("{0} \t", contentType.Name));
            }
            contentTypesProperties.AddChildren(contentTypePropertiesList);
            if (flags)
            {
                mLogger.Info("get properties successful contentTypes:{0}", regularContentTypes.ToString());
            }
        }
        private void AssembleSingleContentTypeProperties(Dictionary<string, object> contentTypeProperties, ContentType contentType)
        {
            CopyProperty(contentTypeProperties, contentType);
            CopyUserResourceProperty(contentTypeProperties, contentType);
            contentTypeProperties.Remove("Id");
            contentTypeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = contentType.Id.ToString();
            //try
            {
                if (contentType.Parent != null && (!(contentType.Parent.ServerObjectIsNull.HasValue && contentType.Parent.ServerObjectIsNull.Value)))
                {
                    contentTypeProperties["ParentId"] = contentType.Parent.Id.ToString();
                }
                else
                {
                    //contentTypeProperties["ParentId"] = "0x01";//找不到默认都设置为item
                    mLogger.Warn("This content type:{0} with Id {1} does not have parent.", contentType.Name, contentType.Id.ToString());
                    throw new Exception(string.Format("This content type:{0} with Id {1} does not have parent.", contentType.Name, contentType.Id.ToString()));
                }
            }
            //catch(Exception e)
            //{
            //    mLogger.Warn("This content type:{0} with Id {1} does not have parent, exception:{2}", contentType.Name, contentType.Id.ToString(), e);
            //}

            Dictionary<string, object> wfAssociations = new Dictionary<string, object>(1);

            if (contentType.WorkflowAssociations.Count > 0)
            {
                var workflowAssociations = new List<IDictionary<string, object>>();
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflow in contentType.WorkflowAssociations)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    workflowPro["ContentTypeIdString"] = contentType.Id.ToString();
                    workflowAssociations.Add(workflowPro);
                }

                wfAssociations.AddChildren(workflowAssociations);
            }
            else
            {
                wfAssociations.AddChildren(new List<IDictionary<string, object>>(0));
            }
            contentTypeProperties["SPOWorkflowAssociations"] = wfAssociations;

        }
        public static void CopyProperty(IDictionary<string, object> proDic, ClientObject Obj)
        {
            ClientObjectData objData = Obj.GetObjectData();
            Dictionary<string, object> clientObjData = objData.Properties;
            foreach (KeyValuePair<string, object> propertyInfo in clientObjData)
            {
                object obj = propertyInfo.Value;
                if (obj == null)
                {
                    proDic[propertyInfo.Key] = null;
                }
                else
                {
                    Type proType = obj.GetType();
                    if (proType.IsEnum)
                    {
                        proDic[propertyInfo.Key] = AveTypeHelper.CastEnumValue((obj));
                    }
                    else
                    {
                        proDic[propertyInfo.Key] = obj;
                    }
                }
            }
        }

        public static void CopyUserResourceProperty(Dictionary<string, object> proDic, ClientObject Obj)
        {
            ClientObjectData objData = Obj.GetObjectData();
            Dictionary<string, object> clientObjData = objData.ClientObjectProperties;
            foreach (KeyValuePair<string, object> propertyInfo in clientObjData)
            {
                var obj = propertyInfo.Value;
                if (obj != null)
                {
                    Type proType = obj.GetType();

                    if (string.Equals("Microsoft.SharePoint.Client.UserResource", proType.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        var resourceData = ((ClientObject)obj).GetObjectData();
                        object value;
                        if (resourceData.MethodReturnObjects.TryGetValue("GetValueForUICulture", out value))
                        {
                            var result = value as Dictionary<string, ClientResult<string>>;
                            if (result != null)
                            {
                                proDic[propertyInfo.Key] = result.ToDictionary(p => p.Key, p => p.Value.Value, StringComparer.OrdinalIgnoreCase);
                            }
                        }
                    }
                }
            }
        }

        private void AssembleSingleFieldProperties(Dictionary<string, object> fieldProperties, Field field)
        {
            try
            {
                CopyProperty(fieldProperties, field);
                CopyUserResourceProperty(fieldProperties, field);
                //these properties can't get from client api, so get it from schemal
                XmlDocument doc = new XmlDocument();
                doc.InnerXml = field.SchemaXml;

                GetNormalFieldProperties(field, doc, fieldProperties);
                if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                {
                    GetCustomization(doc, fieldProperties);
                }
                if (WrapperConfiguration.EnableUseWorkingLanguage)
                {
                    object titleObj = "";
                    if (fieldProperties.TryGetValue("Title", out titleObj) && fieldProperties.ContainsKey("SchemaXml"))
                    {
                        string title = (string)titleObj;
                        string titleInSchema = doc.DocumentElement.GetAttribute("DisplayName");
                        if (!string.Equals(title, titleInSchema, StringComparison.OrdinalIgnoreCase))
                        {
                            doc.DocumentElement.SetAttribute("DisplayName", title);
                            fieldProperties["SchemaXml"] = doc.InnerXml;
                            // mLogger.Info("NewColumnSchema:{0}",doc.InnerXml);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Assemble Single Field Properties Error. FieldSchemaXml : {0} , Error : {1}", field.SchemaXml, e.ToString());
            }
        }

        private void GetNormalFieldProperties(Field field, XmlDocument doc, Dictionary<string, object> fieldProperties)
        {
            XmlElement fElement = doc.FirstChild as XmlElement;
            GetFieldBoolProperties(fElement, fieldProperties);
            string attributeValue = fElement.GetAttribute("FriendlyDisplayFormat");
            if ("DateTime".Equals(field.TypeAsString, StringComparison.Ordinal) && string.IsNullOrEmpty(attributeValue))
            {
                object friendlyFormat = null;
                if (fieldProperties.TryGetValue("FriendlyDisplayFormat", out friendlyFormat))
                {
                    int ff = Convert.ToInt32(friendlyFormat.ToString());
                    string fformat = ((AveDateTimeFieldFriendlyFormatType)ff).ToString();
                    fElement.SetAttribute("FriendlyDisplayFormat", fformat);
                    //fElement.SetAttribute("Indexed", "TRUE");
                    fieldProperties["SchemaXml"] = doc.InnerXml;
                }
            }
            attributeValue = fElement.GetAttribute("RowOrdinal");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RowOrdinal"] = Convert.ToInt32(attributeValue);
            }
            attributeValue = fElement.GetAttribute("AggregationFunction");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["AggregationFunction"] = fElement.GetAttribute("AggregationFunction");
            }

            fieldProperties["ColName"] = fElement.GetAttribute("ColName");

            attributeValue = GetSingleNodeValue(fElement, "DefaultFormula", false);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DefaultFormula"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("DisplaySize");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DisplaySize"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("IMEMode");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["IMEMode"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("JumpToField");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToField"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PIAttribute");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PIAttribute"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PITarget");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PITarget"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PrimaryPIAttribute");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PrimaryPIAttribute"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PrimaryPITarget");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PrimaryPITarget"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("RelatedField");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RelatedField"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("SchemaXmlWithResourceTokens");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["SchemaXmlWithResourceTokens"] = attributeValue;
            }
            attributeValue = GetSingleNodeValue(fElement, "Translations", true);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["TranslationXml"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("SourceID");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["SourceId"] = attributeValue;
            }
            fieldProperties["Type"] = (int)field.FieldTypeKind;
            fieldProperties["ObjectPath"] = GetObjectPathString(field.Path);
            fieldProperties["FieldType"] = field.GetType();
            fieldProperties["RealType"] = field.GetType().Name;
            //fieldProperties["ClientContext"] = mClientContext;

            attributeValue = fElement.GetAttribute("DifferencingLimit");
            if (string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DifferencingLimit"] = 0x5dc;
            }
            else
            {
                fieldProperties["DifferencingLimit"] = Convert.ToInt32(attributeValue);
            }
            attributeValue = fElement.GetAttribute("JumpToNo");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToNo"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("JumpToYes");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToYes"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("RichTextMode");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RichTextMode"] = (AveRichTextMode)Enum.Parse(typeof(AveRichTextMode), attributeValue, true);
            }

            attributeValue = fElement.GetAttribute("Node");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["XPath"] = attributeValue;
            }
            FieldMultiChoice fieldMC = field as FieldMultiChoice;
            if (fieldMC != null && fieldMC.Choices != null)
            {
                StringCollection sc = new StringCollection();
                sc.AddRange(fieldMC.Choices);
                fieldProperties["Choices"] = sc;
            }
        }

        private void GetFieldBoolProperties(XmlElement fElement, Dictionary<string, object> fieldProperties)
        {
            //cache 中保存的key与sharepoint online中获取的key一致。
            List<string> boolProperties = new List<string>() { "AllowDeletion", "Indexed", "NoCrawl", "Reorderable", "ShowInDisplayForm", "ShowInEditForm", "ShowInListSettings" ,
                                                                  "ShowInNewForm","ShowInVersionHistory","ShowInViewForms","PrependId","UnlimitedLengthInDocumentLibrary","IsolateStyles",
                                                                  "RichText"};
            string attributeValue = string.Empty;
            bool fieldPropertyValue;
            foreach (string boolProperty in boolProperties)
            {
                attributeValue = fElement.GetAttribute(boolProperty);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    if (!Boolean.TryParse(attributeValue, out fieldPropertyValue))
                    {
                        mLogger.Warn("can't covert column's property:{0} to bool,column schemaxml:{1}", boolProperty, fElement.OuterXml);
                    }
                    fieldProperties[boolProperty] = fieldPropertyValue;
                }
            }
            //cache中保存的key与sharepoint中保存的获取的key不一致。
            attributeValue = fElement.GetAttribute("LinkToItemAllowed");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                if (attributeValue == "Prohibited")
                {
                    fieldProperties["LinkToItemAllowed"] = false;
                }
                else if (attributeValue == "Required")
                {
                    fieldProperties["LinkToItemAllowed"] = true;
                }
            }
            if (fieldProperties.ContainsKey("LinkToItemAllowed"))
            {
                fieldProperties["LinkToItem"] = fieldProperties["LinkToItemAllowed"];
            }
            else
            {
                attributeValue = fElement.GetAttribute("LinkToItem");
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    if (!Boolean.TryParse(attributeValue, out fieldPropertyValue))
                    {
                        mLogger.Warn("can't covert column's property:{0} to bool,column schemaxml:{1}", "LinkToItem", fElement.OuterXml);
                    }
                    fieldProperties["LinkToItem"] = fieldPropertyValue;
                }
            }
            attributeValue = fElement.GetAttribute("Percentage");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                if (!Boolean.TryParse(attributeValue, out fieldPropertyValue))
                {
                    mLogger.Warn("can't covert column's property:{0} to bool,column schemaxml:{1}", "Percentage", fElement.OuterXml);
                }
                fieldProperties["ShowAsPercentage"] = fieldPropertyValue;
            }
        }

        private void GetCustomization(XmlDocument doc, Dictionary<string, object> fieldProperties)
        {
            foreach (XmlNode childNode in doc.FirstChild.ChildNodes)
            {
                if(childNode.NodeType != XmlNodeType.Element)
                {
                    //在SPO Manual Choose Taxonomy Column Value生成的Field xml中 Customization同级出现了Comment
                    continue;
                }
                XmlElement customElement = childNode as XmlElement;
                if (customElement.Name.Equals("Customization"))
                {
                    foreach (XmlElement element in customElement.ChildNodes)
                    {
                        if (element.Name.Equals("ArrayOfProperty"))
                        {
                            foreach (XmlElement propertyElement in element.ChildNodes)
                            {
                                try
                                {
                                    if (propertyElement.Name.Equals("Property"))
                                    {
                                        string name = null;
                                        object value = null;
                                        XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement nameElement = (XmlElement)elements[0];
                                            name = nameElement.InnerText;
                                        }
                                        elements = propertyElement.GetElementsByTagName("Value");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement valueElement = (XmlElement)elements[0];
                                            string text = valueElement.InnerText;
                                            string type = valueElement.GetAttribute("p4:type");
                                            type = type.Substring(type.IndexOf(":") + 1);
                                            ArgumentCheck.CheckNotNull(name);
                                            if (name.Equals("TextField") || name.Equals("SspId") || name.Equals("GroupId") || name.Equals("TermSetId") || name.Equals("AnchorId"))
                                            {
                                                type = "guid";
                                                string tValue = valueElement.InnerText;
                                                if (tValue.Contains('|'))
                                                {
                                                    string[] temp = tValue.ToString().Split('|');
                                                    if (temp.Length == 2)
                                                    {
                                                        fieldProperties.Add(name, valueElement.InnerText);
                                                        valueElement.InnerText = temp[0];
                                                        continue;
                                                    }
                                                }
                                            }
                                            switch (type)
                                            {
                                                case "datetime":
                                                    value = Convert.ToDateTime(valueElement.InnerText);
                                                    break;
                                                case "boolean":
                                                    value = Convert.ToBoolean(valueElement.InnerText);
                                                    break;
                                                case "guid":
                                                    value = new Guid(valueElement.InnerText);
                                                    break;
                                                case "int32":
                                                case "int":
                                                    value = Convert.ToInt32(valueElement.InnerText);
                                                    break;
                                                case "double":
                                                    value = Convert.ToDouble(valueElement.InnerText);
                                                    break;
                                                default:
                                                    value = valueElement.InnerText;
                                                    break;
                                            }
                                        }
                                        if (!String.IsNullOrEmpty(name) && !fieldProperties.ContainsKey(name))
                                        {
                                            fieldProperties.Add(name, value);
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    mLogger.Debug($"Get Customization {e.ToString()}");
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private string GetSingleNodeValue(XmlElement xmlElement, string nodeName, bool outXml)
        {
            XmlNode node = xmlElement.SelectSingleNode(nodeName);
            if (node != null)
            {
                if (outXml)
                {
                    return node.OuterXml;
                }
                else
                {
                    return node.InnerText;
                }
            }
            return string.Empty;
        }

        private void AssembleFeatureProperties(Dictionary<string, object> featureProperties, Feature feature)
        {
            featureProperties["DefinitionId"] = feature.DefinitionId;
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProperties["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
        }





        public static void AssembleViewProperties(Dictionary<string, object> viewProperties, View view, string webServerRelativeUrl)
        {
            CopyProperty(viewProperties, view);
            viewProperties.Remove("ContentTypeId");
            viewProperties["ContentTypeId" + AveObjectModelConstant.ObjectPropertySuffix] = view.ContentTypeId.ToString();
            viewProperties["Query"] = view.ViewQuery;
            viewProperties["Type"] = view.ViewType;
            viewProperties["Url"] = view.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            //viewFields
            Dictionary<string, object> viewFields = new Dictionary<string, object>();
            List<string> viewFieldsList = new List<string>();
            for (int i = 0; i < view.ViewFields.Count; ++i)
            {
                viewFieldsList.Add(view.ViewFields[i]);
            }
            viewFields[AveObjectModelConstant.ChildrenProperties] = viewFieldsList;
            viewFields["SchemaXml"] = view.ViewFields.SchemaXml;
            viewProperties["ViewFields" + AveObjectModelConstant.ObjectPropertySuffix] = viewFields;
        }

        public static void AssembleDiscoverViewProperties(Dictionary<string, object> viewProperties, View view, ClientFile file)
        {
            string etag = file.ETag;
            string id = etag.Substring(etag.IndexOf("{", StringComparison.OrdinalIgnoreCase) + 1, etag.IndexOf("}", StringComparison.OrdinalIgnoreCase) - etag.IndexOf("{", StringComparison.OrdinalIgnoreCase) - 1);
            Guid docId = new Guid(id);
            viewProperties.Add("PageUrlID", docId);
            viewProperties.Add("DocID", docId);
            viewProperties.Add("LeafName", file.Name);
            viewProperties.Add("ID", (file.ListItemAllFields.FieldValues.Count == 0) ? (int?)null : file.ListItemAllFields.Id);
            viewProperties.Add("Uiversion", file.UIVersion);
            viewProperties.Add("TimeLastModified", file.TimeLastModified);
            viewProperties.Add("Level", Convert.ToByte(file.Level));
            string dirName = view.ServerRelativeUrl.Substring(0, view.ServerRelativeUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).TrimStart('/');
            viewProperties.Add("ViewID", view.Id);
            viewProperties.Add("ViewType", 0);    //Can not get this property 
            viewProperties.Add("IsPersonalView", view.PersonalView);
            viewProperties.Add("BaseViewId", Convert.ToByte(view.BaseViewId));
            viewProperties.Add("ViewTitle", view.Title);
            viewProperties.Add("ViewUserID", (int?)null);//Can not get this property 
            viewProperties.Add("DirName", dirName);
            viewProperties.Add("DocFlags", (int?)null);   //Can not get this property
            viewProperties.Add("Type", Convert.ToByte(0));
            viewProperties.Add("Size", 0);     //Can not get this property
            viewProperties.Add("ParentID", Guid.Empty);    //Can not get this property
            viewProperties.Add("FullUrl", view.ServerRelativeUrl);
        }

        private Principal GetPrincipalByLoginName(Web web, Dictionary<string, object> properties)
        {
            string loginName = properties["MemberLoginName"] as string;
            string memeberType = properties["MemberType"] as string;
            Principal member = null;
            switch (memeberType)
            {
                case "Group":
                    int groupId = (int)properties["MemberId"];
                    member = web.SiteGroups.GetById(groupId);
                    break;
                default:
                    member = web.EnsureUser(loginName);
                    break;
            }
            return member;
        }
        private Dictionary<string, object> GetGroupProperties(ClientContext context, Group group, bool IsLoaded, bool isNeedLoadGroupUsers = false)
        {
            Dictionary<string, object> siteGroupProperties = new Dictionary<string, object>();
            if (!IsLoaded)
            {
                try
                {
                    context.Load(group, g => g.Owner.Id, g => g.Owner.PrincipalType);
                    context.Load(group, g => g.AllowMembersEditMembership);
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Warn("Load GroupProperties Error, Error Message: {0}", e.ToString());
                    return siteGroupProperties;
                }
            }
            CopyProperty(siteGroupProperties, group);
            if (group.ServerObjectIsNull.HasValue && !group.ServerObjectIsNull.Value)
            {
                siteGroupProperties["Name"] = group.Title;
                siteGroupProperties["AllowMembersEditMembership"] = group.AllowMembersEditMembership;
                siteGroupProperties["OwnerId"] = group.Owner.Id;
                siteGroupProperties["OwnerType"] = group.Owner.PrincipalType.ToString();

                if (isNeedLoadGroupUsers)
                {
                    siteGroupProperties["IsHiddenInUI"] = group.IsHiddenInUI;
                    //siteGroupProperties["Users"] = ConvertUserCollection(group.Users);
                    //Dictionary<string, object> users = new Dictionary<string, object>();
                    //List<Dictionary<string, object>> userList = new List<Dictionary<string, object>>();
                    List<string> users = new List<string>();
                    foreach (User user in group.Users)
                    {
                        //Dictionary<string, object> userProperties = new Dictionary<string, object>();
                        //AveObjectCopy.GetObjectBasicProperties(userProperties, user);
                        //userProperties.Add("Name", user.Title);
                        //userList.Add(userProperties);
                        users.Add(user.LoginName);
                    }
                    //users.Add("ChildrenProperties", userList);
                    siteGroupProperties["Users" + AveObjectModelConstant.ObjectPropertySuffix] = users;
                }
                siteGroupProperties["Exists"] = true;
            }
            else
            {
                siteGroupProperties["Exists"] = false;
            }
            return siteGroupProperties;
        }
        //roleassignment may come from web or list or listitem
        private bool GetRoleAssignment(Site site, Web pricipalBelongedWeb, Dictionary<string, object> properties, out Principal principal, out RoleDefinitionBindingCollection roleDefinitionBindingCol)
        {
            bool rdbcUpdated = false;
            bool isNewCreated = (bool)properties[AveObjectModelConstant.IsNewCreated];
            Principal member = GetPrincipalByLoginName(pricipalBelongedWeb, properties);
            RoleAssignment roleAssignment = null;
            if (isNewCreated)
            {
                principal = member;
                roleDefinitionBindingCol = new RoleDefinitionBindingCollection(site.Context);
                foreach (var roleDefinitionId in properties["RoleDefinitionBindingCollection"] as List<int>)
                {
                    if (roleDefinitionId != AveConstants.LIMIT_ACCESS_ROLE_ID)
                    {
                        RoleDefinition roleDef = pricipalBelongedWeb.RoleDefinitions.GetById(roleDefinitionId);
                        roleDefinitionBindingCol.Add(roleDef);
                        rdbcUpdated = true;
                    }
                }
            }
            else
            {
                string webServerRelativeUrl = properties[AveObjectModelConstant.WebServerRelativeUrl] as string;
                Web web = site.OpenWeb(webServerRelativeUrl);

                RoleAssignmentCollection roleAssignmentCol = null;
                if (properties.ContainsKey(AveObjectModelConstant.ListId))
                {
                    Guid listId = (Guid)properties[AveObjectModelConstant.ListId];
                    List list = web.Lists.GetById(listId);
                    if (properties.ContainsKey(AveObjectModelConstant.ItemId))
                    {
                        int itemId = (int)properties[AveObjectModelConstant.ItemId];
                        ListItem listItem = list.GetItemById(itemId);
                        roleAssignmentCol = listItem.RoleAssignments;
                    }
                    else
                    {
                        roleAssignmentCol = list.RoleAssignments;
                    }
                }
                else
                {
                    roleAssignmentCol = web.RoleAssignments;
                }
                roleAssignment = roleAssignmentCol.GetByPrincipal(member);
                principal = roleAssignment.Member;
                roleDefinitionBindingCol = roleAssignment.RoleDefinitionBindings;
                rdbcUpdated = true;
            }
            return rdbcUpdated;
        }
        internal static BasePermissions ConvertULongToBasePerm(ulong aveBasePerm)
        {
            BasePermissions basePerm = new BasePermissions();
            ulong permValue = (ulong)aveBasePerm;
            AveReflectionUtility.SetFieldValue("m_high", basePerm, (uint)(permValue >> 32));
            AveReflectionUtility.SetFieldValue("m_low", basePerm, (uint)aveBasePerm);
            return basePerm;
        }
        internal static ulong ConvertBasePermToULong(BasePermissions basePerm)
        {
            if (basePerm == null)
            {
                return 0;
            }
            uint high = (uint)AveReflectionUtility.GetFieldValue("m_high", basePerm);
            uint low = (uint)AveReflectionUtility.GetFieldValue("m_low", basePerm);
            return ((ulong)high << 32) | low;
        }
        internal void ConvertToChangeObject(ChangeCollection changeCollection, IDictionary<string, object> changeCache)
        {
            Dictionary<Guid, object> changedSiteCache = changeCache["ChangedSiteCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedWebCache = changeCache["ChangedWebCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedListCache = changeCache["ChangedListCache"] as Dictionary<Guid, object>;
            Dictionary<string, object> changedItemsCache = changeCache["ChangedItemsCache"] as Dictionary<string, object>;
            Dictionary<Guid, object> changedFolderCache = changedItemsCache["ChangedFolderCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedFileCache = changedItemsCache["ChangedFileCache"] as Dictionary<Guid, object>;
            Dictionary<string, object> changedItemCache = changedItemsCache["ChangedItemCache"] as Dictionary<string, object>;
            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                Dictionary<string, object> tempProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                AveChangeType preChangeType = AveChangeType.None;
                SPChangeType currentChangeType = (SPChangeType)objectProperties["ChangeType"];
                objectProperties["SPChangeType"] = currentChangeType.ToString();
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeItem":
                        Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
                        Guid itemlistId = new Guid(objectProperties["ListId"].ToString());
                        tempProperties = ConvertToChangeItem(changeObject, currentChangeType, itemlistId, objectProperties, changedItemCache);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = itemWebId;
                        if (!changedWebCache.ContainsKey(itemWebId))
                        {
                            changedWebCache[itemWebId] = tempProperties;
                        }

                        tempProperties["ListId"] = itemlistId;
                        if (!changedListCache.ContainsKey(itemlistId))
                        {
                            changedListCache[itemlistId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeFile":
                    case "Microsoft.SharePoint.Client.ChangeFolder":
                        bool isFile = changeObject.GetType().ToString().Equals("Microsoft.SharePoint.Client.ChangeFile");
                        Guid folderWebId = new Guid(objectProperties["WebId"].ToString());
                        Dictionary<Guid, object> changeFileOrFolderCache = isFile ? changedFileCache : changedFolderCache;
                        tempProperties = ConvertToChangeFileOrFolder(changeObject, currentChangeType, true, objectProperties, changeFileOrFolderCache);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = folderWebId;
                        if (!changedWebCache.ContainsKey(folderWebId))
                        {
                            changedWebCache[folderWebId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeList":
                        tempProperties = ConvertToChangeList(changeObject, currentChangeType, preChangeType, objectProperties, changedListCache);
                        Guid parentWebId = new Guid(objectProperties["WebId"].ToString());
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = parentWebId;
                        if (!changedWebCache.ContainsKey(parentWebId))
                        {
                            changedWebCache[parentWebId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeView":
                        ConvertToChangeView(changeObject, currentChangeType, preChangeType, objectProperties, changedFileCache, changedListCache, changedWebCache, changedSiteCache);
                        break;
                    case "Microsoft.SharePoint.Client.ChangeWeb":
                        tempProperties = ConvertToChangeWeb(changeObject, currentChangeType, preChangeType, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        break;
                    //case "Microsoft.SharePoint.Client.ChangeGroup":
                    //case "Microsoft.SharePoint.Client.ChangeUser":
                    case "Microsoft.SharePoint.Client.ChangeField":
                    case "Microsoft.SharePoint.Client.ChangeAlert":
                    case "Microsoft.SharePoint.Client.ChangeContentType":
                        tempProperties = ConvertToChangeWebFAC(changeObject, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        break;
                    default:
                        objectProperties["ChangeObjectType"] = ChangeObjectType.Site;
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId) && objectProperties.Count > 0)
                        {
                            changedSiteCache[changeObject.SiteId] = objectProperties;
                        }
                        break;
                }
            }
        }

        internal IDictionary<string, object> GetChangeItemObject(ChangeCollection changeCollection)
        {
            Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                SPChangeType currentChangeType = (SPChangeType)objectProperties["ChangeType"];
                objectProperties["SPChangeType"] = currentChangeType.ToString();
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeItem":
                        Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
                        Guid itemlistId = new Guid(objectProperties["ListId"].ToString());
                        ConvertToChangeItem(changeObject, currentChangeType, itemlistId, objectProperties, changedItemCache);
                        break;
                    default:
                        break;
                }
            }
            return changedItemCache;
        }

        internal void ConvertToContainerChangeObject(ChangeCollection changeCollection, IDictionary<string, object> changeCache)
        {
            Dictionary<Guid, object> changedSiteCache = changeCache["ChangedSiteCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedWebCache = changeCache["ChangedWebCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedListCache = changeCache["ChangedListCache"] as Dictionary<Guid, object>;
            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                Dictionary<string, object> tempProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                AveChangeType preChangeType = AveChangeType.None;
                SPChangeType currentChangeType = (SPChangeType)objectProperties["ChangeType"];
                objectProperties["SPChangeType"] = currentChangeType.ToString();
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeItem":
                        Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
                        Guid itemlistId = new Guid(objectProperties["ListId"].ToString());
                        tempProperties = ConvertToChangeItem(changeObject, currentChangeType, itemlistId, objectProperties, null);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = new Dictionary<string, object>(tempProperties);
                        }
                        tempProperties["WebId"] = itemWebId;
                        if (!changedWebCache.ContainsKey(itemWebId))
                        {
                            changedWebCache[itemWebId] = new Dictionary<string, object>(tempProperties);
                        }

                        tempProperties["ListId"] = itemlistId;
                        if (!changedListCache.ContainsKey(itemlistId))
                        {
                            changedListCache[itemlistId] = new Dictionary<string, object>(tempProperties);
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeFile":
                    case "Microsoft.SharePoint.Client.ChangeFolder":
                        bool isFile = changeObject.GetType().ToString().Equals("Microsoft.SharePoint.Client.ChangeFile");
                        Guid folderWebId = new Guid(objectProperties["WebId"].ToString());
                        //Dictionary<Guid, object> changeFileOrFolderCache = isFile ? changedFileCache : changedFolderCache;
                        tempProperties = ConvertToChangeFileOrFolder(changeObject, currentChangeType, true, objectProperties, null);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = new Dictionary<string, object>(tempProperties);
                        }
                        tempProperties["WebId"] = folderWebId;
                        if (!changedWebCache.ContainsKey(folderWebId))
                        {
                            changedWebCache[folderWebId] = new Dictionary<string, object>(tempProperties);
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeList":
                        tempProperties = ConvertToChangeList(changeObject, currentChangeType, preChangeType, objectProperties, changedListCache);
                        Guid parentWebId = new Guid(objectProperties["WebId"].ToString());
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = new Dictionary<string, object>(tempProperties);
                        }
                        tempProperties["WebId"] = parentWebId;
                        if (!changedWebCache.ContainsKey(parentWebId))
                        {
                            changedWebCache[parentWebId] = new Dictionary<string, object>(tempProperties);
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeView":
                        ConvertToChangeView(changeObject, currentChangeType, preChangeType, objectProperties, null, changedListCache, changedWebCache, changedSiteCache);
                        break;
                    case "Microsoft.SharePoint.Client.ChangeWeb":
                        tempProperties = ConvertToChangeWeb(changeObject, currentChangeType, preChangeType, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = new Dictionary<string, object>(tempProperties);
                        }
                        break;
                    //case "Microsoft.SharePoint.Client.ChangeGroup":
                    //case "Microsoft.SharePoint.Client.ChangeUser":
                    case "Microsoft.SharePoint.Client.ChangeField":
                    case "Microsoft.SharePoint.Client.ChangeAlert":
                    case "Microsoft.SharePoint.Client.ChangeContentType":
                        tempProperties = ConvertToChangeWebFAC(changeObject, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = new Dictionary<string, object>(tempProperties);
                        }
                        break;
                    default:
                        objectProperties["ChangeObjectType"] = ChangeObjectType.Site;
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId) && objectProperties.Count > 0)
                        {
                            changedSiteCache[changeObject.SiteId] = objectProperties;
                        }
                        break;
                }
            }
        }

        private Dictionary<string, object> ConvertToChangeItem(Change changeObject, SPChangeType currentChangeType, Guid listId, IDictionary<string, object> objectProperties, IDictionary<string, object> changedItemCache)
        {
            bool isRenamed = false;
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Item;
            if (currentChangeType == SPChangeType.DeleteObject)
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Delete;
            }
            else if (currentChangeType == SPChangeType.Rename)
            {
                isRenamed = true;
                objectProperties["ChangeType"] = (int)AveChangeType.Add;
            }
            else if (currentChangeType == SPChangeType.Add)
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Add;
            }
            else if (currentChangeType == SPChangeType.Restore) //SAAS-7203
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Restore;
            }
            else
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
            }
            int itemId = (int)(objectProperties["ItemId"]);
            objectProperties["IsRenamed"] = isRenamed;
            if (changedItemCache != null)
            {
                changedItemCache[listId.ToString() + ";" + itemId.ToString()] = objectProperties;
            }
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.Edit;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        private Dictionary<string, object> ConvertToChangeFileOrFolder(Change changeObject, SPChangeType currentChangeType, bool isFile, IDictionary<string, object> objectProperties, IDictionary<Guid, object> changedFileOrFolderCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = isFile ? ChangeObjectType.File : ChangeObjectType.Folder;
            if (currentChangeType == SPChangeType.DeleteObject)
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Delete;
            }
            else if (currentChangeType == SPChangeType.Add)
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Add;
            }
            else
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
            }
            Guid uniqueId = new Guid(objectProperties["UniqueId"].ToString());
            if (changedFileOrFolderCache != null && !changedFileOrFolderCache.ContainsKey(uniqueId))
            {
                changedFileOrFolderCache[uniqueId] = objectProperties;
            }
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.Edit;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        private Dictionary<string, object> ConvertToChangeList(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedListCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.List;
            Guid listId = new Guid(objectProperties["ListId"].ToString());
            if (!changedListCache.ContainsKey(listId))
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
                changedListCache[listId] = objectProperties;
            }
            Dictionary<string, object> listObj = changedListCache[listId] as Dictionary<string, object>;
            preChangeType = (AveChangeType)listObj["ChangeType"];
            currentChangeType = changeObject.ChangeType;
            if (preChangeType == AveChangeType.Add ||
                preChangeType == AveChangeType.Restore)
            {
                if (currentChangeType == SPChangeType.DeleteObject)
                {
                    listObj["ChangeType"] = (int)AveChangeType.Delete;
                    if (!listObj.ContainsKey("ChangeTypeBeforeDelete"))
                    {
                        listObj["ChangeTypeBeforeDelete"] = (int)AveChangeType.Add;
                    }
                }
                //otherwise not change.
            }
            else //"None or Edit", change to "Edit or Delete".
            {
                if (preChangeType == AveChangeType.Delete &&
                    currentChangeType == SPChangeType.Restore)
                {
                    listObj["ChangeType"] = listObj["ChangeTypeBeforeDelete"];
                    if (preChangeType == AveChangeType.None)
                    {
                        changedListCache.Remove(listId);
                    }
                }
                else if (currentChangeType == SPChangeType.DeleteObject)
                {
                    listObj["ChangeTypeBeforeDelete"] = (int)preChangeType;
                    listObj["ChangeType"] = (int)AveChangeType.Delete;
                }
                else if (currentChangeType == SPChangeType.Add)
                {
                    listObj["ChangeType"] = (int)AveChangeType.Add;
                }
                else
                {
                    listObj["ChangeType"] = (int)AveChangeType.Edit;
                }
            }
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.Edit;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        private Dictionary<string, object> ConvertToChangeWeb(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedWebCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Web;
            Guid tempWebId = new Guid(objectProperties["WebId"].ToString());
            if (!changedWebCache.ContainsKey(tempWebId))
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
                changedWebCache[tempWebId] = objectProperties;
            }
            Dictionary<string, object> webObj = changedWebCache[tempWebId] as Dictionary<string, object>;
            preChangeType = (AveChangeType)webObj["ChangeType"];
            currentChangeType = changeObject.ChangeType;
            if (currentChangeType == SPChangeType.Navigation)
            {
                webObj["NavigationChanged"] = true;
            }
            if (preChangeType == AveChangeType.Add)
            {
                if (currentChangeType == SPChangeType.DeleteObject)
                {
                    webObj["ChangeType"] = (int)AveChangeType.Delete;
                }
            }
            else if (currentChangeType == SPChangeType.DeleteObject)
            {
                webObj["FullUrl"] = "";
                webObj["ChangeType"] = (int)AveChangeType.Delete;
            }
            else if (currentChangeType == SPChangeType.Add)
            {
                webObj["ChangeType"] = (int)AveChangeType.Add;
            }
            else //"None or Edit", change to "Edit or Delete".
            {
                webObj["ChangeType"] = (int)AveChangeType.Edit;
            }
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.Edit;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        /// <summary>
        /// 对于有改动的web level的field alert contenttype，
        /// 如果对应的web没有改动，那么该web的ChangeType为Edit；
        /// 如果对应的web已经有ChangeType了，保持不变。
        /// </summary>
        /// <param name="changeObject"></param>
        /// <param name="objectProperties"></param>
        /// <param name="changedWebCache"></param>
        /// <returns></returns>
        protected Dictionary<string, object> ConvertToChangeWebFAC(Change changeObject, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedWebCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Web;
            Guid tempWebId = new Guid(objectProperties["WebId"].ToString());
            if (!changedWebCache.ContainsKey(tempWebId))
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
                changedWebCache[tempWebId] = objectProperties;
            }
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.Edit;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }

        protected Dictionary<string, object> ConvertToChangeView(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedFileCache, Dictionary<Guid, object> changedListCache, Dictionary<Guid, object> changedWebCache, Dictionary<Guid, object> changedSiteCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.View;
            objectProperties["ChangeType"] = (int)AveChangeType.None;
            Guid listId = new Guid(objectProperties["ListId"].ToString());

            Dictionary<string, object> listObj = null;
            Object tempObj = null;

            if (changedListCache.TryGetValue(listId, out tempObj))
            {
                listObj = (Dictionary<string, object>)tempObj;
            }
            else
            {
                listObj = new Dictionary<string, object>(objectProperties);
                changedListCache[listId] = listObj;

                Guid webId = new Guid(objectProperties["WebId"].ToString());

                if (!changedWebCache.ContainsKey(webId))
                {
                    changedWebCache[webId] = listObj;

                    if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                    {
                        changedSiteCache[changeObject.SiteId] = listObj;
                    }
                }
            }

            Guid viewId = new Guid(objectProperties["ViewId"].ToString());
            Dictionary<string, object> viewObj = null;
            if (changedFileCache != null && changedFileCache.TryGetValue(viewId, out tempObj))
            {
                viewObj = (Dictionary<string, object>)tempObj;
                preChangeType = (AveChangeType)viewObj["ChangeType"];
                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        viewObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                        if (preChangeType != AveChangeType.Add)
                        {
                            viewObj["ChangeType"] = (int)AveChangeType.Edit;
                        }
                        break;
                    case SPChangeType.DeleteObject:
                        viewObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Restore:
                        viewObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                viewObj = objectProperties;
                if (changedFileCache != null)
                {
                    changedFileCache[viewId] = viewObj;
                }
                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        viewObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                        viewObj["ChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.DeleteObject:
                        viewObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Restore:
                        viewObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }

            return null;
        }
        private void UpdateRoleAssignment(Web web, Dictionary<string, object> roleAssignmentProperties, int principalId, SecurableObject securableObject, Dictionary<string, object> newRoleAssignmentProperties)
        {
            RoleAssignment roleAssignment = securableObject.RoleAssignments.GetByPrincipalId(principalId);
            web.Context.Load(roleAssignment);
            web.Context.Load(roleAssignment, role => role.Member);
            web.Context.Load(roleAssignment.RoleDefinitionBindings);
            web.Context.ExecuteQuery();
            UpdateRoleDefinitionBindingCollection(web, roleAssignment.RoleDefinitionBindings, roleAssignmentProperties);
            roleAssignment.Update();
            web.Context.Load(roleAssignment);
            web.Context.Load(roleAssignment, role => role.Member, role => role.RoleDefinitionBindings);
            web.Context.ExecuteQuery();
            AssembleRoleAssignmetProperites(newRoleAssignmentProperties, roleAssignment);
        }
        private void UpdateRoleDefinitionBindingCollection(Web web, RoleDefinitionBindingCollection roleDefinitionBindingCol, Dictionary<string, object> roleAssignmentProperties)
        {
            HashSet<int> containedRoleDefintionIdSet = new HashSet<int>();
            List<RoleDefinition> shouldDeletedRoleDefinitionList = new List<RoleDefinition>();
            List<int> shouldAddedRoleDefinitionIdList = new List<int>();
            List<int> shouldContainedRoleDefinitonIdSet = roleAssignmentProperties["RoleDefinitionBindingCollection"] as List<int>;
            foreach (RoleDefinition roleDefinition in roleDefinitionBindingCol)
            {
                if (shouldContainedRoleDefinitonIdSet.Contains(roleDefinition.Id))
                {
                    containedRoleDefintionIdSet.Add(roleDefinition.Id);
                }
                else
                {
                    shouldDeletedRoleDefinitionList.Add(roleDefinition);
                }
            }
            foreach (var roleDefinitionId in shouldContainedRoleDefinitonIdSet)
            {
                if (!containedRoleDefintionIdSet.Contains(roleDefinitionId))
                {
                    shouldAddedRoleDefinitionIdList.Add(roleDefinitionId);
                }
            }
            foreach (RoleDefinition roleDefinition in shouldDeletedRoleDefinitionList)
            {
                roleDefinitionBindingCol.Remove(roleDefinition);
            }
            foreach (var roleDefinitionId in shouldAddedRoleDefinitionIdList)
            {
                roleDefinitionBindingCol.Add(web.RoleDefinitions.GetById(roleDefinitionId));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="etagStr"></param>
        /// <returns>string[0]: UniqueId; string[1]: DocLibRowId</returns>
        private static string[] GetIdsFromEtag(string etagStr)
        {
            string UniqueId = string.Empty;
            string DocLibRowId = string.Empty;
            int startIndex1 = etagStr.IndexOf("{", StringComparison.OrdinalIgnoreCase);
            int startIndex2 = etagStr.IndexOf(",", StringComparison.OrdinalIgnoreCase) + 1;
            if (startIndex1 >= 0)
            {
                int endIndex1 = etagStr.IndexOf('}', startIndex1);
                if (endIndex1 > startIndex1)
                {
                    UniqueId = etagStr.Substring(startIndex1, endIndex1 - startIndex1 + 1);
                }
            }
            if (startIndex2 >= 0)
            {
                int endIndex2 = etagStr.IndexOf('"', startIndex2);
                if (endIndex2 > startIndex2)
                {
                    DocLibRowId = etagStr.Substring(startIndex2, endIndex2 - startIndex2);
                }
            }
            return new string[] { UniqueId, DocLibRowId };
        }
        private void LoadList(ClientContext context, List list)
        {
            context.Load(list);
            context.Load(list, l => l.ValidationFormula,
                                      l => l.ValidationMessage,
                                      l => l.OnQuickLaunch,
                                      //l => l.SchemaXml,
                                      l => l.RootFolder,
                                      l => l.RootFolder.Properties,
                                      l => l.IsSiteAssetsLibrary,
                                      l => l.HasUniqueRoleAssignments,
                                      l => l.DataSource,
                                      l => l.Id,
                                      l => l.Hidden,
                                      l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614
                                      l => l.DefaultDisplayFormUrl,//SAAS-964
                                      l => l.ItemCount,
                                      l => l.EnableAssignToEmail//SAAS-35092 
                                      );   
            context.ExecuteQuery();
        }

        public bool DisableListVersion(string webRelativeUrl, string listTitle, Guid listId, bool changed, bool enableVersioning)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);
                if (!changed)
                {
                    if (enableVersioning)
                    {
                        list.EnableVersioning = false;
                        list.Update();
                        context.ExecuteQuery();
                        return true;
                    }
                    return false;
                }
                else
                {
                    list.EnableVersioning = true;
                    list.Update();
                    context.ExecuteQuery();
                    return false;
                }
            }
        }





        #region metadata

        private void AssembleTermProperties(Term term, Dictionary<string, object> termProperties)
        {
            AveObjectCopy.GetObjectBasicProperties(termProperties, term);
            if (termProperties.ContainsKey("MergedTermIds"))
            {
                termProperties.Remove("MergedTermIds");
            }
        }

        private bool ExecuteTermMethod(Term term, Dictionary<string, object> needUpdateTermProperties)
        {
            bool needCommit = false;
            if (needUpdateTermProperties.ContainsKey("SetDescription"))
            {
                List<string> parms = needUpdateTermProperties["SetDescription"] as List<string>;
                term.SetDescription(parms[0], Convert.ToInt32(parms[1]));
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("CreateLabel"))
            {
                List<List<string>> parmLists = needUpdateTermProperties["CreateLabel"] as List<List<string>>;
                foreach (List<string> parms in parmLists)
                {
                    term.CreateLabel(parms[0], Convert.ToInt32(parms[1]), Convert.ToBoolean(parms[2]));
                }
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("Deprecate"))
            {
                bool doDeprecate = Convert.ToBoolean(needUpdateTermProperties["Deprecate"]);
                term.Deprecate(doDeprecate);
                needUpdateTermProperties.Remove("Deprecate");
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("ChangedCustomProperties"))
            {
                Dictionary<string, string> customProperties = needUpdateTermProperties["ChangedCustomProperties"] as Dictionary<string, string>;
                foreach (KeyValuePair<string, string> customProperty in customProperties)
                {
                    term.SetCustomProperty(customProperty.Key, customProperty.Value);
                }
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("ChangedLocalCustomProperties"))
            {
                Dictionary<string, string> customProperties = needUpdateTermProperties["ChangedLocalCustomProperties"] as Dictionary<string, string>;
                foreach (KeyValuePair<string, string> customProperty in customProperties)
                {
                    term.SetLocalCustomProperty(customProperty.Key, customProperty.Value);
                }
                needCommit = true;
            }

            return needCommit;
        }

        private Dictionary<string, object> UpdateTermGroup(ClientContext context, TermGroup updateGroup, int language, Dictionary<string, object> needUpdateGroupProperties)
        {
            Dictionary<string, object> GroupProp = new Dictionary<string, object>();
            Dictionary<string, object> TermSetsList = new Dictionary<string, object>();
            if (needUpdateGroupProperties.ContainsKey("DeleteGroup"))
            {
                if (updateGroup.TermSets.Count > 0)
                {
                    foreach (TermSet set in updateGroup.TermSets)
                    {
                        set.DeleteObject();
                    }
                }
                updateGroup.DeleteObject();
                context.ExecuteQuery();
            }
            else
            {
                AveObjectCopy.GetObjectBasicProperties(GroupProp, updateGroup);
                if (needUpdateGroupProperties.ContainsKey("AddTermSet"))
                {
                    List<Dictionary<string, object>> termSetNames = needUpdateGroupProperties["AddTermSet"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> termsetProp in termSetNames)
                    {
                        Dictionary<string, object> newTermSetProperties = new Dictionary<string, object>();
                        TermSet termset1 = null;
                        TermSet termset2 = null;
                        //if the id is being token, then create a term with new id. we can't use conditionscope here, because termstore.getterm(guid id) can't get term which exists in other sitecollection term group.
                        ExceptionHandlingScope isIdTaken = new ExceptionHandlingScope(context);
                        using (isIdTaken.StartScope())
                        {
                            using (isIdTaken.StartTry())
                            {
                                termset1 = updateGroup.CreateTermSet(termsetProp["Name"].ToString(), (Guid)termsetProp["Id"], language);
                                context.Load(termset1);
                                updateGroup.TermStore.CommitAll();
                            }
                            using (isIdTaken.StartCatch())
                            {
                                termset2 = updateGroup.CreateTermSet(termsetProp["Name"].ToString(), Guid.NewGuid(), language);
                                context.Load(termset2);
                                updateGroup.TermStore.CommitAll();
                            }
                        }
                        context.ExecuteQuery();
                        TermSet termset = isIdTaken.HasException ? termset2 : termset1;
                        CopyProperty(newTermSetProperties, termset);
                        TermSetsList.Add(termset.Name, newTermSetProperties);
                    }
                }
                if (needUpdateGroupProperties.ContainsKey("UpdateTermSets"))
                {
                    Dictionary<string, object> needUpdateTermSets = needUpdateGroupProperties["UpdateTermSets"] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> termSetProperties in needUpdateTermSets)
                    {
                        TermSet termSet = updateGroup.TermSets.GetByName(termSetProperties.Key);
                        Dictionary<string, object> needUpdateTermSetProperties = termSetProperties.Value as Dictionary<string, object>;
                        Dictionary<string, object> TermSetProp = UpdateTermSet(context, termSet, needUpdateTermSetProperties, language);
                        TermSetsList[termSetProperties.Key] = TermSetProp;
                    }
                }
            }
            GroupProp.Add("TermSet", TermSetsList);
            return GroupProp;
        }

        private Dictionary<string, object> UpdateTermSet(ClientContext context, TermSet termSet, Dictionary<string, object> needUpdateTermSetProperties, int language)
        {
            Dictionary<string, object> TermSetProp = new Dictionary<string, object>();
            Dictionary<string, object> TermsList = new Dictionary<string, object>();
            if (needUpdateTermSetProperties.ContainsKey("DeleteTermSet"))
            {
                termSet.DeleteObject();
                context.ExecuteQuery();
            }
            else
            {
                AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_OUT_RANGE_INDEX); // 在3151环境下出现loadtermset失败的情况影响整个MMS的还原添加 retry逻辑保证term  set的还原   by zma
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    AveObjectCopy.UpdateObjectBasicProperties(needUpdateTermSetProperties, termSet);
                    bool needCommitAll = false;
                    if (needUpdateTermSetProperties.ContainsKey("ChangedCustomProperties"))
                    {
                        Dictionary<string, string> customProperties = needUpdateTermSetProperties["ChangedCustomProperties"] as Dictionary<string, string>;
                        foreach (KeyValuePair<string, string> kv in customProperties)
                        {
                            termSet.SetCustomProperty(kv.Key, kv.Value);
                        }
                        needCommitAll = true;
                    }
                    if (needUpdateTermSetProperties.ContainsKey("AddStakeholder"))
                    {
                        List<string> stakeHolders = needUpdateTermSetProperties["AddStakeholder"] as List<string>;
                        foreach (string userName in stakeHolders)
                        {
                            termSet.AddStakeholder(userName);
                        }
                        needCommitAll = true;
                    }
                    else if (Convert.ToInt32(needUpdateTermSetProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                    {
                        needCommitAll = true;
                    }
                    if (needCommitAll)
                    {
                        termSet.TermStore.CommitAll();
                    }
                    context.Load(termSet);
                    context.ExecuteQuery();
                });
                CopyProperty(TermSetProp, termSet);

                Dictionary<Guid, Guid> tempTermIdMapping = new Dictionary<Guid, Guid>();
                if (needUpdateTermSetProperties.ContainsKey("AddTerm"))
                {
                    Dictionary<Guid, List<string>> termNames = needUpdateTermSetProperties["AddTerm"] as Dictionary<Guid, List<string>>;
                    foreach (KeyValuePair<Guid, List<string>> termName in termNames)
                    {
                        Dictionary<string, object> termProperties = new Dictionary<string, object>();
                        Term term1 = null;
                        Term term2 = null;
                        //if the id is being token, then create a term with new id. we can't use conditionscope here, because termstore.getterm(guid id) can't get term which exists in other sitecollection term group.
                        ExceptionHandlingScope isIdTaken = new ExceptionHandlingScope(context);
                        using (isIdTaken.StartScope())
                        {
                            using (isIdTaken.StartTry())
                            {
                                term1 = termSet.CreateTerm(termName.Value[0], Convert.ToInt32(termName.Value[1]), termName.Key);
                                termSet.TermStore.CommitAll();
                                context.Load(term1);
                            }
                            using (isIdTaken.StartCatch())
                            {
                                term2 = termSet.CreateTerm(termName.Value[0], Convert.ToInt32(termName.Value[1]), Guid.NewGuid());
                                termSet.TermStore.CommitAll();
                                context.Load(term2);
                            }
                        }
                        context.ExecuteQuery();
                        Term term = isIdTaken.HasException ? term2 : term1;
                        if (isIdTaken.HasException)
                        {
                            tempTermIdMapping.Add(termName.Key, term.Id);
                        }
                        AssembleTermProperties(term, termProperties);
                        TermsList.Add(term.Id.ToString(), termProperties);
                    }
                }
                if (needUpdateTermSetProperties.ContainsKey("ReuseTerm"))
                {
                    Dictionary<Guid, bool> reuseTerms = needUpdateTermSetProperties["ReuseTerm"] as Dictionary<Guid, bool>;
                    if (reuseTerms != null)
                    {
                        foreach (KeyValuePair<Guid, bool> pair in reuseTerms)
                        {
                            Dictionary<string, object> termProperties = new Dictionary<string, object>();
                            Term sourceTerm = null;
                            Term term = null;
                            try
                            {
                                sourceTerm = termSet.TermStore.GetTerm(pair.Key);
                                term = termSet.ReuseTerm(sourceTerm, pair.Value);
                                context.Load(term);
                                context.ExecuteQuery();
                            }
                            catch (ServerException e)
                            {
                                context.Load(sourceTerm, t => t.Name, t => t.Id);
                                context.ExecuteQuery();
                                term = termSet.CreateTerm(sourceTerm?.Name, language, Guid.NewGuid());
                                context.Load(term);
                                context.ExecuteQuery();
                                tempTermIdMapping.Add(sourceTerm.Id, term.Id);
                                mLogger.Warn("An error occur when reused term: {0}.create a new term instead.due to {1}.", sourceTerm.Name, e.Message.ToString());
                            }
                            AssembleTermProperties(term, termProperties);
                            TermsList.Add(pair.Key.ToString(), termProperties);
                        }
                    }
                }
                if (needUpdateTermSetProperties.ContainsKey("PinTerm"))
                {
                    Dictionary<Guid, bool> pinTerms = needUpdateTermSetProperties["PinTerm"] as Dictionary<Guid, bool>;
                    if (pinTerms != null)
                    {
                        foreach (KeyValuePair<Guid, bool> pair in pinTerms)
                        {
                            Dictionary<string, object> termProperties = new Dictionary<string, object>();
                            Term sourceTerm = termSet.TermStore.GetTerm(pair.Key);
                            Term term = termSet.ReuseTermWithPinning(sourceTerm);
                            context.Load(term);
                            context.ExecuteQuery();
                            AssembleTermProperties(term, termProperties);
                            TermsList.Add(pair.Key.ToString(), termProperties);
                        }
                    }
                }
                if (needUpdateTermSetProperties.ContainsKey("UpdateTerms"))
                {
                    Dictionary<string, object> needUpdateTerms = needUpdateTermSetProperties["UpdateTerms"] as Dictionary<string, object>;
                    if (needUpdateTerms != null)
                    {
                        foreach (KeyValuePair<string, object> termProperties in needUpdateTerms)
                        {
                            try
                            {
                                Guid originalTermId = new Guid(termProperties.Key);
                                Guid termId = tempTermIdMapping.ContainsKey(originalTermId) ? tempTermIdMapping[originalTermId] : originalTermId;
                                Term term = termSet.GetTerm(termId);
                                Dictionary<string, object> needUpdateTermProperties = termProperties.Value as Dictionary<string, object>;
                                UpdateTerm(context, term, needUpdateTermProperties, TermsList, language);
                            }
                            catch (ServerException e)
                            {
                                mLogger.Warn("An error occur when restore term.due to {0}.", e.ToString());
                            }
                        }
                    }
                }
            }
            TermSetProp.Add("Term", TermsList);
            return TermSetProp;
        }

        private void UpdateTerm(ClientContext context, Term term, Dictionary<string, object> needUpdateTermProperties, Dictionary<string, object> TermsList, int language)
        {
            if (needUpdateTermProperties.ContainsKey("DeleteTerm"))
            {
                term.DeleteObject();
                context.ExecuteQuery();
            }
            else
            {
                Dictionary<string, object> termProp = new Dictionary<string, object>();
                AveObjectCopy.UpdateObjectBasicPropertiesWithEscape(needUpdateTermProperties, term, new string[] { "Name" });
                if (ExecuteTermMethod(term, needUpdateTermProperties)
                    || (Convert.ToInt32(needUpdateTermProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0))
                {
                    term.TermStore.CommitAll();
                }
                context.Load(term);
                context.ExecuteQuery();
                AveObjectCopy.GetObjectBasicProperties(termProp, term);
                TermsList[term.Id.ToString()] = termProp;

                if (needUpdateTermProperties.ContainsKey("AddTerm"))
                {
                    //Dictionary<string, object> subTermsList = new Dictionary<string, object>();
                    Dictionary<Guid, List<string>> termNames = needUpdateTermProperties["AddTerm"] as Dictionary<Guid, List<string>>;
                    foreach (KeyValuePair<Guid, List<string>> termName in termNames)
                    {
                        Dictionary<string, object> termProperties = new Dictionary<string, object>();
                        Term term1 = null;
                        Term term2 = null;
                        ExceptionHandlingScope isIdTaken = new ExceptionHandlingScope(context);
                        using (isIdTaken.StartScope())
                        {
                            using (isIdTaken.StartTry())
                            {
                                term1 = term.CreateTerm(termName.Value[0], Convert.ToInt32(termName.Value[1]), termName.Key);
                                context.Load(term1);
                            }
                            using (isIdTaken.StartCatch())
                            {
                                term2 = term.CreateTerm(termName.Value[0], Convert.ToInt32(termName.Value[1]), Guid.NewGuid());
                                context.Load(term2);
                            }
                        }
                        context.ExecuteQuery();
                        Term subTerm = isIdTaken.HasException ? term2 : term1;
                        AssembleTermProperties(subTerm, termProperties);
                        //subTermsList.Add(subTerm.Id.ToString(), termProperties);
                        TermsList[subTerm.Id.ToString()] = termProperties;
                    }
                }
                if (needUpdateTermProperties.ContainsKey("ReuseTerm"))
                {
                    //Dictionary<string, object> subTermsList = new Dictionary<string, object>();
                    Dictionary<Guid, bool> termNames = needUpdateTermProperties["ReuseTerm"] as Dictionary<Guid, bool>;
                    foreach (KeyValuePair<Guid, bool> pair in termNames)
                    {
                        Dictionary<string, object> termProperties = new Dictionary<string, object>();
                        Term subTerm = null;
                        Term sourceTerm = null;
                        try
                        {
                            sourceTerm = term.TermStore.GetTerm(pair.Key);
                            subTerm = term.ReuseTerm(sourceTerm, pair.Value);
                            context.Load(subTerm);
                            context.ExecuteQuery();
                        }
                        catch (ServerException e)
                        {
                            context.Load(sourceTerm, t => t.Name, t => t.Id);
                            context.ExecuteQuery();
                            subTerm = term.CreateTerm(sourceTerm?.Name, language, Guid.NewGuid());
                            context.Load(subTerm);
                            context.ExecuteQuery();
                            mLogger.Warn("An error occur when reused term: {0}.create a new term instead.due to {1}.", sourceTerm.Name, e.Message.ToString());
                        }
                        AssembleTermProperties(subTerm, termProperties);
                        //subTermsList.Add(subTerm.Id.ToString(), termProperties);
                        TermsList[subTerm.Id.ToString()] = termProperties;
                    }
                }
                if (needUpdateTermProperties.ContainsKey("PinTerm"))
                {
                    //Dictionary<string, object> subTermsList = new Dictionary<string, object>();
                    Dictionary<Guid, bool> termNames = needUpdateTermProperties["PinTerm"] as Dictionary<Guid, bool>;
                    foreach (KeyValuePair<Guid, bool> pair in termNames)
                    {
                        Dictionary<string, object> termProperties = new Dictionary<string, object>();
                        Term sourceTerm = term.TermStore.GetTerm(pair.Key);
                        Term subTerm = term.ReuseTermWithPinning(sourceTerm);
                        context.Load(subTerm);
                        context.ExecuteQuery();
                        AssembleTermProperties(subTerm, termProperties);
                        //subTermsList.Add(subTerm.Id.ToString(), termProperties);
                        TermsList[subTerm.Id.ToString()] = termProperties;
                    }
                }
                if (needUpdateTermProperties.ContainsKey("ReassignSourceTerm"))
                {
                    bool reAssignSourceTerm = Convert.ToBoolean(needUpdateTermProperties["ReassignSourceTerm"]);
                    if (reAssignSourceTerm)
                    {
                        Dictionary<string, object> termProperties = new Dictionary<string, object>();
                        term.SourceTerm.ReassignSourceTerm(term);
                        context.ExecuteQuery();
                        AssembleTermProperties(term, termProperties);
                        TermsList[term.Id.ToString()] = termProperties;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region set
        public void SetAuditLogTrimming(int compatibilityLevel, Dictionary<string, object> parameters)
        {
            using (AveClientContext context = CreateContext())
            {
                string isEnable = (string)parameters["TrimAuditLog"];
                if (isEnable.Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase))
                {
                    context.Site.TrimAuditLog = true;
                    context.Site.AuditLogTrimmingRetention = int.Parse(parameters["TrimRetention"].ToString());
                }
                else
                {
                    context.Site.TrimAuditLog = false;
                }
                context.ExecuteQuery();
            }
        }
        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            mRequestCommon.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }

        public bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp)
        {
            return mRequestCommon.SetListRateSetting(webServerRelativeUrl, listUrl, listId, enableRating, isLikesExp);
        }
        public void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            mRequestCommon.SetMetadataNavigationSettings(webServerRelativeUrl, listTitle, listId, updateProperties);
        }
        public void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {

        }
        public Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            return new Dictionary<string, object>();
        }
        public Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            return new Dictionary<string, object>();
        }
        #endregion

        #region webpart

        public void CloseWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                LimitedWebPartManager manager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
                WebPartDefinition webpart = manager.WebParts.GetById(webpartId);
                webpart.CloseWebPart();
                webpart.SaveWebPartChanges();
                context.ExecuteQuery();
            }
        }

        public void DeleteWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                LimitedWebPartManager manager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
                WebPartDefinition webpart = manager.WebParts.GetById(webpartId);
                webpart.DeleteWebPart();
                context.ExecuteQuery();
            }
        }

        public Dictionary<string, object> ImportAndAddWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, string webPartXml, string zoneId, int zoneIndex)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                LimitedWebPartManager manager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
                WebPartDefinition webpart = manager.ImportWebPart(webPartXml);
                WebPartDefinition webpartAdded = manager.AddWebPart(webpart.WebPart, zoneId, zoneIndex);
                context.Load(webpartAdded);
                context.ExecuteQuery();
                WebPart webPartInner = webpartAdded.WebPart;
                context.Load(webPartInner);
                context.ExecuteQuery();

                Dictionary<string, object> webPartProperties = new Dictionary<string, object>();
                CopyProperty(webPartProperties, webPartInner);
                webPartProperties["ID"] = webpartAdded.Id.ToString();
                webPartProperties["DefinitionXml"] = webPartXml;
                return webPartProperties;
            }
        }

        #endregion

        public void Dispose(bool KeepRequest)
        {
            if (!KeepRequest)
            {
                this.Dispose();
            }
            else
            {
                this.DisposeCache();
            }
        }

        internal delegate string GetObjectIdentity();
        internal delegate long GetPathId();
        internal delegate void SetObjectPathId(long id);

        internal object GetObjectPathString(ObjectPath path)
        {
            string identity = string.Empty;
            long id = 0;
            Assembly assembly = typeof(ObjectPath).Assembly;
            Type objectPathIdentity = assembly.GetType("Microsoft.SharePoint.Client.ObjectPathIdentity", false, true);
            if (path.GetType().Equals(objectPathIdentity))
            {
                BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
                    | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
                MethodInfo getIdentityMethod = objectPathIdentity.GetProperty("Identity", flags).GetGetMethod(true);
                GetObjectIdentity getIdentity = Delegate.CreateDelegate(typeof(GetObjectIdentity), path, getIdentityMethod) as GetObjectIdentity;
                identity = getIdentity();
                MethodInfo getIdMethod = objectPathIdentity.GetProperty("Id", flags).GetGetMethod(true);
                GetPathId getId = Delegate.CreateDelegate(typeof(GetPathId), path, getIdMethod) as GetPathId;
                id = getId();
                if (string.IsNullOrEmpty(identity) || id <= 0)
                {
                    return string.Empty;
                }
                return identity + "#" + id.ToString();
            }
            return path;
        }

        //internal ObjectPath GetObjectPathByIdentity(object objectPath)
        //{
        //    object path = null;
        //    if ((objectPath as ObjectPath) != null)
        //    {
        //        return objectPath as ObjectPath;
        //    }
        //    if (string.IsNullOrEmpty(objectPath.ToString()))
        //    {
        //        return null;
        //    }
        //    string[] splitStrings = objectPath.ToString().Split(new char[] { '#' });
        //    if (string.IsNullOrEmpty(splitStrings[0]) || string.IsNullOrEmpty(splitStrings[1]))
        //    {
        //        return null;
        //    }
        //    using (AveClientContext context = CreateContext())
        //    {
        //        Assembly assembly = typeof(ClientRuntimeContext).Assembly;
        //        Type objectPathIdentity = assembly.GetType("Microsoft.SharePoint.Client.ObjectPathIdentity", false);
        //        Type[] types = new Type[] { typeof(ClientRuntimeContext), typeof(string) };
        //        ConstructorInfo constructor = objectPathIdentity.GetConstructor(types);
        //        object[] paramaters = new object[] { context, splitStrings[0] };
        //        path = constructor.Invoke(paramaters);
        //        BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
        //            | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
        //        MethodInfo setPathId = typeof(ObjectPath).GetProperty("Id", flags).GetSetMethod(true);
        //        SetObjectPathId setId = Delegate.CreateDelegate(typeof(SetObjectPathId), path, setPathId.Name) as SetObjectPathId;
        //        setId(Convert.ToInt64(splitStrings[1]));
        //        return path as ObjectPath;
        //    }
        //}

        internal Guid GetFieldIdFromIdentity(string identity)
        {
            Guid fieldId = Guid.Empty;
            if (!string.IsNullOrEmpty(identity))
            {
                int startIndex = identity.IndexOf(":field:", StringComparison.OrdinalIgnoreCase) + 7;
                int endIndex = identity.IndexOf("#");
                if (startIndex >= endIndex)
                {
                    return fieldId;
                }
                string id = identity.Substring(startIndex, endIndex - startIndex);
                fieldId = new Guid(id);
            }
            return fieldId;
        }

        protected bool CheckWikiPage(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                bool isWikiPage = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                ExceptionHandlingScope itemIsNullCondition = new ExceptionHandlingScope(context);
                using (itemIsNullCondition.StartScope())
                {
                    using (itemIsNullCondition.StartTry())
                    {
                        context.Load(file);
                        context.Load(file.ListItemAllFields);
                    }
                    using (itemIsNullCondition.StartCatch())
                    {
                        context.Load(file);
                    }
                }
                context.ExecuteQuery();
                if (file.IsObjectPropertyInstantiated("ListItemAllFields") && file.ListItemAllFields.IsPropertyAvailable("Id"))
                {
                    if (file.ListItemAllFields.FieldValues.Count > 0 &&
                        file.ListItemAllFields.FieldValues.ContainsKey("WikiField") && file.ListItemAllFields.FieldValues["WikiField"] != null)
                    {
                        isWikiPage = true;
                    }
                }
                return isWikiPage;
            }
        }


        public Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                var workflowTemplates = new List<IDictionary<string, object>>();
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplateCollection wfTemplates = null;
                switch (workflowSource)
                {
                    case "web.workflowTemplates":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        wfTemplates = web.WorkflowTemplates;
                        break;
                    default:
                        break;
                }
                ArgumentCheck.CheckNotNull(wfTemplates);
                context.Load(wfTemplates);
                context.ExecuteQuery();
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template in wfTemplates)
                {
                    Dictionary<string, object> workflowtmplate = new Dictionary<string, object>();
                    CopyProperty(workflowtmplate, template);
                    workflowTemplates.Add(workflowtmplate);
                }
                returnInfo.AddChildren(workflowTemplates);
                return returnInfo;
            }
        }

        public Dictionary<string, object> GetPages(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> pages = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItemCollectionPosition position = null;
                CamlQuery camlQuery = new CamlQuery();
                StringBuilder queryXml = new StringBuilder();
                queryXml.Append("<View Scope='RecursiveAll'><ViewFields>");
                queryXml.Append("<FieldRef Name='FileDirRef'/>");
                queryXml.Append("<FieldRef Name='FileLeafRef'/>");
                queryXml.Append("<FieldRef Name='File_x0020_Type'/>");
                queryXml.Append("</ViewFields><RowLimit>");
                queryXml.Append(this.maxItemsPerThrottledOperation);
                queryXml.Append("</RowLimit></View>");
                camlQuery.ViewXml = queryXml.ToString();
                var pagePropertiesList = new List<IDictionary<string, object>>();
                pages.AddChildren(pagePropertiesList);
                do
                {
                    camlQuery.ListItemCollectionPosition = position;
                    ListItemCollection listItems = list.GetItems(camlQuery);
                    //"==" is ok here, SharePoint will lower case the file extension automaticlly
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                            items => items.Include(item => item["File_x0020_Type"], item => item["FileDirRef"], item => item["FileLeafRef"])
                                            .Where(item => ((string)item["File_x0020_Type"]) == "aspx"));
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        Dictionary<string, object> fieldValues = new Dictionary<string, object>();
                        fieldValues["FileDirRef"] = item["FileDirRef"];
                        fieldValues["FileLeafRef"] = item["FileLeafRef"];
                        itemProperty["FieldValues"] = fieldValues;
                        pagePropertiesList.Add(itemProperty);
                    }
                    position = listItems.ListItemCollectionPosition;
                }
                while (position != null);

                return pages;
            }
        }

        public string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            return mWebServiceRequest.AssociateWorkflowMarkup(webServerRelativeUrl, configUrl, configVersion);
        }

        public void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            mWebServiceRequest.BrowserEnableUserFormTemplate(formTemplateUrl);
        }

        public Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostlistId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);
                context.ExecuteQuery();

                List taskListCM = web.Lists.GetById(asso.TaskListId);
                context.Load(taskListCM);
                context.ExecuteQuery();

                List historyListCM = web.Lists.GetById(asso.HistoryListId);
                context.Load(historyListCM);
                context.ExecuteQuery();

                List hostListCM = web.Lists.GetById(hostlistId);
                context.Load(hostListCM);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.TaskList = taskListCM;
                createInfo.HistoryList = historyListCM;
                createInfo.Template = template;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = hostListCM.WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                //assoNew.HistoryListTitle = asso.HistoryListTitle;
                //assoNew.TaskListTitle = asso.TaskListTitle;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);

                return returnInfo;
            }
        }

        public Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostlistId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);
                context.ExecuteQuery();

                List taskListCM = web.Lists.GetById(asso.TaskListId);
                context.Load(taskListCM);
                context.ExecuteQuery();

                List historyListCM = web.Lists.GetById(asso.HistoryListId);
                context.Load(historyListCM);
                context.ExecuteQuery();

                List hostListCM = web.Lists.GetById(hostlistId);
                context.Load(hostListCM);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.TaskList = taskListCM;
                createInfo.HistoryList = historyListCM;
                createInfo.Template = template;
                createInfo.ContentTypeAssociationHistoryListName = historyListCM.Title;
                createInfo.ContentTypeAssociationTaskListName = taskListCM.Title;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = hostListCM.ContentTypes.GetById(ctId.ToString()).WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                //assoNew.HistoryListTitle = asso.HistoryListTitle;
                //assoNew.TaskListTitle = asso.TaskListTitle;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);

                return returnInfo;
            }
        }

        public Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid hostlistId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);
                context.ExecuteQuery();

                List taskListCM = web.Lists.GetById(asso.TaskListId);
                context.Load(taskListCM);
                context.ExecuteQuery();

                List historyListCM = web.Lists.GetById(asso.HistoryListId);
                context.Load(historyListCM);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.Template = template;
                createInfo.HistoryList = historyListCM;
                createInfo.TaskList = taskListCM;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = web.WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                assoNew.Update();

                context.Load(assoNew);
                try
                {
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Value cannot be null"))
                    {
                        //local站点执行update时会抛异常，当数据可以更新
                    }
                    else
                    {
                        mLogger.Debug("An error occurred while create WebAssociation.Message:{0}.", e.ToString());
                        throw;
                    }
                }
                CopyProperty(returnInfo, assoNew);
                return returnInfo;
            }
        }

        public Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.Template = template;
                createInfo.ContentTypeAssociationHistoryListName = asso.HistoryListTitle;
                createInfo.ContentTypeAssociationTaskListName = asso.TaskListTitle;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = web.ContentTypes.GetById(ctId.ToString()).WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);

                return returnInfo;
            }
        }

        public void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listId, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCollection workflowAssociations = null;
                switch (workflowSource)
                {
                    case "web.workflows":
                        workflowAssociations = web.WorkflowAssociations;
                        break;
                    case "list.workflows":
                        List list = web.Lists.GetById(listId);
                        workflowAssociations = list.WorkflowAssociations;
                        break;
                    case "contentType.workflows":
                        ContentType contentType = null;
                        if (!string.IsNullOrEmpty(listName))
                        {
                            contentType = web.Lists.GetById(listId).ContentTypes.GetById(ctId);
                        }
                        else
                        {
                            contentType = web.ContentTypes.GetById(ctId);
                        }
                        workflowAssociations = contentType.WorkflowAssociations;
                        break;
                    default:
                        break;
                }
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflowAsso = workflowAssociations?.GetById(workflowAssociationId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateWorkflowProperties, workflowAsso);
                workflowAsso?.Update();
                context.ExecuteQuery();

            }
        }

        public Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo, List<AveAppMetadata> avaliableTenantApp, List<AveAppMetadata> avaliableSiteApp)
        {
            AveAppRestore appRestore = new AveAppRestore(this, webServerRelativeUrl, avaliableTenantApp, avaliableSiteApp);
            bool isNewCreate = appRestore.RestoreApp(appInfo, restoreInfo);
            Dictionary<string, object> appsProp = GetAppsByProductId(webServerRelativeUrl, appInfo.ProductId);
            appsProp[AveObjectModelConstant.IsNewCreated] = isNewCreate;
            return appsProp;
        }

        public virtual Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinsh)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                var appInstance = web.GetAppInstanceById(instanceId);
                var result = appInstance.Uninstall();
                context.ExecuteQuery();
                if (waitUninstallFinsh)
                {
                    WaitUntilUninstallFinish(context, web, productId);
                }
                return result.Value;
            }
        }

        public Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type)
        {
            Dictionary<string, string> customProperties = new Dictionary<string, string>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermSetItem item = null;
                if (type == AveTermSetItemType.TermSet)
                {
                    item = store.GetTermSet(termSetId) as TermSetItem;
                }
                else
                {
                    item = store.GetTermInTermSet(termSetId, termId) as TermSetItem;
                }
                item.SetCustomProperty(name, value);
                context.Load(item, it => it.CustomProperties);
                context.ExecuteQuery();
                foreach (KeyValuePair<string, string> pair in item.CustomProperties)
                {
                    customProperties[pair.Key] = pair.Value;
                }
            }
            return customProperties;
        }
        public Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value)
        {
            Dictionary<string, string> localCustomProperties = new Dictionary<string, string>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                Term term = store.GetTermInTermSet(termSetId, termId);
                term.SetLocalCustomProperty(name, value);
                context.Load(term, t => t.LocalCustomProperties);
                context.ExecuteQuery();
                foreach (KeyValuePair<string, string> pair in term.LocalCustomProperties)
                {
                    localCustomProperties[pair.Key] = pair.Value;
                }
            }
            return localCustomProperties;
        }


        ///TODO_LONG need to search
        public void SetFormForList(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            mWebServiceRequest.SetFormForList(webServerRelativeUrl, lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
        }

        public void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId)
        {
            this.mRequestCommon.ResetPersonalizationState(webServerRelativeUrl, fileServerRelativeUrl, webpartId);
        }


        public IList<AveEventReceiver> RemoveAllEventReceivers(string webServerRelativeUrl, Guid listId)
        {
            IList<AveEventReceiver> eventReciverList = new List<AveEventReceiver>();
            List<string> ignoreEventReceiverClass = new List<string>();
            //ignoreEventReceiverClass.Add("Microsoft.SharePoint.Portal.CommunityEventReceiver");
            ignoreEventReceiverClass.Add("Microsoft.Office.RecordsManagement.Internal.HoldEventReceiver");
            ignoreEventReceiverClass.Add("Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver");
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                EventReceiverDefinitionCollection erCollection = list.EventReceivers;
                context.Load(erCollection);
                context.ExecuteQuery();

                IList<EventReceiverDefinition> willDeleteEventReceivers = new List<EventReceiverDefinition>(erCollection.Count);
                foreach (EventReceiverDefinition erDefinition in erCollection)
                {
                    if (ignoreEventReceiverClass.Contains(erDefinition.ReceiverClass))
                    {
                        continue;
                    }
                    AveEventReceiver eventReceiver = new AveEventReceiver()
                    {
                        EventType = (AveEventReceiverType)erDefinition.EventType,
                        ReceiverAssembly = erDefinition.ReceiverAssembly,
                        ReceiverClass = erDefinition.ReceiverClass,
                        ReceiverId = erDefinition.ReceiverId,
                        ReceiverName = erDefinition.ReceiverName,
                        ReceiverUrl = erDefinition.ReceiverUrl,
                        SequenceNumber = erDefinition.SequenceNumber,
                        Synchronization = (AveEventReceiverSynchronization)erDefinition.Synchronization
                    };
                    eventReciverList.Add(eventReceiver);
                    willDeleteEventReceivers.Add(erDefinition);
                }

                foreach (EventReceiverDefinition erDefinition in willDeleteEventReceivers)
                {
                    erDefinition.DeleteObject();
                }
                context.ExecuteQuery();
            }
            return eventReciverList;
        }

        public void AddEventReceivers(string webServerRelativeUrl, Guid listId, IList<AveEventReceiver> eventReceivers)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                EventReceiverDefinitionCollection erCollection = list.EventReceivers;

                foreach (AveEventReceiver eventReceiver in eventReceivers)
                {
                    EventReceiverDefinitionCreationInformation erdcInfo = new EventReceiverDefinitionCreationInformation()
                    {
                        EventType = (EventReceiverType)eventReceiver.EventType,
                        ReceiverAssembly = eventReceiver.ReceiverAssembly,
                        ReceiverClass = eventReceiver.ReceiverClass,
                        ReceiverName = eventReceiver.ReceiverName,
                        ReceiverUrl = eventReceiver.ReceiverUrl,
                        SequenceNumber = eventReceiver.SequenceNumber,
                        Synchronization = (EventReceiverSynchronization)eventReceiver.Synchronization
                    };
                    erCollection.Add(erdcInfo);
                }

                context.ExecuteQuery();
            }
        }

        public Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                Folder parentFolder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderUrl));
                ContentType contentType = list.ContentTypes.GetById(contentTypeId.ToString());
                context.Load(contentType, c => c.Id);
                context.ExecuteQuery();
                ClientResult<string> result = DocumentSet.Create(context, parentFolder, name, contentType.Id);
                context.ExecuteQuery();
                string documentSetRelativeUrl = result.Value;
                Dictionary<string, object> folderInfo = this.GetFolder(webServerRelativeUrl, listName, AveUrlUtility.GetServerRelativeUrl(documentSetRelativeUrl));
                return folderInfo;
            }
        }

        /// <summary>
        /// 还原Document Set Versions
        /// </summary>
        public void RestoreDocumentsetVersions(string webRelativeUrl, Guid listId, int itemId, IOrderedEnumerable<XmlElement> versions)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem folder = list.GetItemById(itemId);
                context.Load(web.CurrentUser);
                context.Load(folder);
                context.Load(folder.ContentType.FieldLinks);
                context.Load(folder.Folder.Properties);
                context.Load(folder.Folder.Files, fs => fs.Include(f => f.ListItemAllFields));
                context.ExecuteQuery();

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root;
                XmlElement items;
                XmlElement snapshots;
                List<string> oldItemsGuid;
                int lastLabel = 1;
                CheckFirstVersion(xmlDoc, folder, out root, out items, out snapshots, out oldItemsGuid);
                //将目的端的version信息置为空，全部使用远端version信息
                snapshots.InnerText = "";
                DocumentSetVersionItems(folder, root, items, oldItemsGuid);
                foreach (XmlElement item in versions)
                {
                    DocumentSetVersionSnapshot(web, folder, root, snapshots, item);
                    if (lastLabel < int.Parse(item.GetAttribute("Label")))
                    {
                        lastLabel = int.Parse(item.GetAttribute("Label"));
                    }
                }

                root.Attributes["NextSnapshotNumber"].Value = (lastLabel + 1).ToString();

                folder.Folder.Properties["snapshots"] = xmlDoc.OuterXml;
                folder.Folder.Update();
                context.ExecuteQuery();
            }
        }

        private void DocumentSetVersionSnapshot(Web web, ListItem folder, XmlElement root, XmlElement snapshots, XmlElement item)
        {

            XmlElement snapshot = root.OwnerDocument.CreateElement("Snapshot");
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes["Label"] = item.GetAttribute("Label");
            attributes["Major"] = item.GetAttribute("Major");
            attributes["Created"] = item.GetAttribute("Created");
            attributes["By"] = web.CurrentUser.LoginName;
            AppendAttributes(snapshot, attributes);
            XmlElement comments = root.OwnerDocument.CreateElement("Comments");
            comments.InnerText = item.GetElementsByTagName("Comments")[0].InnerText;
            snapshot.AppendChild(comments);
            DocumentSetVersionFields(folder, snapshot);
            DocumentSetVersionSnapshotItems(folder, snapshot, bool.Parse(item.GetAttribute("Major")));
            snapshots.InnerXml = snapshot.OuterXml + snapshots.InnerXml;
            root.AppendChild(snapshots);
        }

        public void AddDocumentsetVersion(string webRelativeUrl, Guid listId, string listTitle, int itemId, bool isMajor, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);
                ListItem folder = list.GetItemById(itemId);
                context.Load(web.CurrentUser);
                context.Load(folder);
                context.Load(folder.ContentType.FieldLinks);
                context.Load(folder.Folder.Properties);
                context.Load(folder.Folder.Files, fs => fs.Include(f => f.ListItemAllFields));
                context.ExecuteQuery();

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root;
                XmlElement items;
                XmlElement snapshots;
                List<string> oldItemsGuid;
                CheckFirstVersion(xmlDoc, folder, out root, out items, out snapshots, out oldItemsGuid);
                DocumentSetVersionItems(folder, root, items, oldItemsGuid);
                DocumentSetVersionSnapshot(web, folder, root, snapshots, isMajor, comment);
                root.Attributes["NextSnapshotNumber"].Value = (int.Parse(root.Attributes["NextSnapshotNumber"].Value) + 1).ToString();

                folder.Folder.Properties["snapshots"] = xmlDoc.OuterXml;
                folder.Folder.Update();
                context.ExecuteQuery();
            }
        }

        protected void CheckFirstVersion(XmlDocument xmlDoc, ListItem folder, out XmlElement root, out XmlElement items, out XmlElement snapshots, out List<string> oldItemsGuid)
        {
            oldItemsGuid = new List<string>();
            if (folder.Folder.Properties.FieldValues.ContainsKey("snapshots"))
            {
                string originalXml = folder.Folder.Properties["snapshots"].ToString();
                xmlDoc.LoadXml(originalXml);
                root = xmlDoc.SelectSingleNode("//SnapshotCollection") as XmlElement;
                items = xmlDoc.SelectSingleNode("//Items") as XmlElement;
                snapshots = xmlDoc.SelectSingleNode("//Snapshots") as XmlElement;
                XmlNodeList oldItems = xmlDoc.SelectNodes("//Items/Item");
                foreach (XmlNode node in oldItems)
                {
                    oldItemsGuid.Add(node.Attributes["Guid"].Value);
                }
            }
            //First Version
            else
            {
                root = xmlDoc.CreateElement("SnapshotCollection");
                items = xmlDoc.CreateElement("Items");
                root.AppendChild(items);
                snapshots = xmlDoc.CreateElement("Snapshots");
                root.AppendChild(snapshots);
                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes["NextSnapshotNumber"] = "1";
                attributes["NextInternalId"] = "1";
                AppendAttributes(root, attributes);
                xmlDoc.AppendChild(root);
            }
        }

        protected void DocumentSetVersionItems(ListItem folder, XmlElement root, XmlElement items, List<string> oldItemsGuid)
        {
            int fileId = int.Parse(root.Attributes["NextInternalId"].Value);
            foreach (Microsoft.SharePoint.Client.File file in folder.Folder.Files)
            {
                if (oldItemsGuid.Count > 0 && oldItemsGuid.Contains(file.ListItemAllFields.FieldValues["UniqueId"].ToString()))
                {
                    continue;
                }
                XmlElement item = root.OwnerDocument.CreateElement("Item");
                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes["Id"] = fileId.ToString();
                attributes["Guid"] = file.ListItemAllFields.FieldValues["UniqueId"].ToString();
                attributes["Url"] = file.ListItemAllFields.FieldValues["FileLeafRef"].ToString();
                attributes["LinkToDoc"] = Boolean.FalseString;
                AppendAttributes(item, attributes);
                items.AppendChild(item);
                fileId++;
            }
            root.ReplaceChild(items, items);
            root.Attributes["NextInternalId"].Value = fileId.ToString();
        }

        private void AppendAttributes(XmlNode node, Dictionary<string, string> attributes)
        {
            if (attributes == null || attributes.Count <= 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> tempAttribute in attributes)
            {
                XmlAttribute attribute = node.OwnerDocument.CreateAttribute(tempAttribute.Key);
                attribute.Value = tempAttribute.Value;
                node.Attributes.Append(attribute);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DateTime format string. ")]
        protected void DocumentSetVersionSnapshot(Web web, ListItem folder, XmlElement root, XmlElement snapshots, bool isMajor, string comment)
        {
            XmlElement snapshot = root.OwnerDocument.CreateElement("Snapshot");
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes["Label"] = root.Attributes["NextSnapshotNumber"].Value.ToString();
            attributes["Major"] = isMajor.ToString();
            attributes["Created"] = DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");
            attributes["By"] = web.CurrentUser.LoginName;
            AppendAttributes(snapshot, attributes);
            XmlElement comments = root.OwnerDocument.CreateElement("Comments");
            comments.InnerText = comment;
            snapshot.AppendChild(comments);
            DocumentSetVersionFields(folder, snapshot);
            DocumentSetVersionSnapshotItems(folder, snapshot, isMajor);
            snapshots.InnerXml = snapshot.OuterXml + snapshots.InnerXml;
            root.AppendChild(snapshots);
        }

        protected void DocumentSetVersionFields(ListItem folder, XmlElement snapshot)
        {
            List<string> needSkipFields = new List<string>() { "ContentType", "ItemChildCount", "FolderChildCount" };
            Dictionary<string, Guid> fieldsInfo = new Dictionary<string, Guid>();
            XmlElement fields = snapshot.OwnerDocument.CreateElement("Fields");
            foreach (FieldLink fieldlink in folder.ContentType.FieldLinks)
            {
                if (!needSkipFields.Contains(fieldlink.Name))
                {
                    fieldsInfo[fieldlink.Name] = fieldlink.Id;
                }
            }
            //DocumentSet默认的contenttype中自带的field
            if (fieldsInfo.ContainsKey("Title"))
            {
                DocumentSetVersionField(fields, folder, "Title", fieldsInfo);
                fieldsInfo.Remove("Title");
            }
            if (fieldsInfo.ContainsKey("DocumentSetDescription"))
            {
                DocumentSetVersionField(fields, folder, "DocumentSetDescription", fieldsInfo);
                fieldsInfo.Remove("DocumentSetDescription");
            }
            if (fieldsInfo.ContainsKey("FileLeafRef"))
            {
                DocumentSetVersionField(fields, folder, "FileLeafRef", fieldsInfo);
                fieldsInfo.Remove("FileLeafRef");
            }
            //如果有额外添加的Field存在
            if (fieldsInfo.Count > 0)
            {
                foreach (KeyValuePair<string, Guid> pair in fieldsInfo)
                {
                    DocumentSetVersionField(fields, folder, pair.Key, fieldsInfo);
                }
            }
            snapshot.AppendChild(fields);
        }

        protected void DocumentSetVersionField(XmlElement fields, ListItem folder, string fieldName, Dictionary<string, Guid> fieldsInfo)
        {
            XmlElement field = fields.OwnerDocument.CreateElement("Field");
            XmlAttribute id = fields.OwnerDocument.CreateAttribute("Id");
            id.Value = fieldsInfo[fieldName].ToString();
            field.Attributes.Append(id);
            if (folder.FieldValues.ContainsKey(fieldName) && folder[fieldName] != null)
            {
                field.InnerText = folder["Title"].ToString();
            }
            else
            {
                field.InnerText = string.Empty;
            }
            fields.AppendChild(field);
            //fieldsInfo.Remove(fieldName);
        }

        protected void DocumentSetVersionSnapshotItems(ListItem folder, XmlElement snapshot, bool isMajor)
        {
            XmlElement snapshotItems = snapshot.OwnerDocument.CreateElement("SnapshotItems");
            if (!isMajor)
            {
                foreach (Microsoft.SharePoint.Client.File f in folder.Folder.Files)
                {
                    XmlElement snapshotItem = snapshot.OwnerDocument.CreateElement("SnapshotItem");
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    XmlNode itemNode = snapshot.OwnerDocument.SelectSingleNode("//Items/Item[@Guid=\'" + f.ListItemAllFields["UniqueId"].ToString() + "\']");
                    if (itemNode != null)
                    {
                        attributes["Id"] = itemNode.Attributes["Id"].Value;
                    }
                    attributes["Version"] = f.ListItemAllFields["_UIVersionString"].ToString();
                    AppendAttributes(snapshotItem, attributes);

                    snapshotItems.AppendChild(snapshotItem);
                }
            }
            snapshot.AppendChild(snapshotItems);
        }

        public void RemoveItemCache(int itemId)
        {
            lock (mLockObj)
            {
                this.mCurrentList.Items.Remove(itemId);
            }
        }

        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            mRequestCommon.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }

        /// <summary>
        /// GetSitePropertiesByUrl
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var siteProp = tenant.GetSitePropertiesByUrl(siteUrl, true);
                //context.Load(siteProp);
                siteProp.RetrieveSiteProperties();
                context.ExecuteQuery();
                Dictionary<string, object> sitePropDic = new Dictionary<string, object>();
                CopyProperty(sitePropDic, siteProp);
                return sitePropDic;
            };
        }
        /// <summary>
        /// UpdateSiteBasicPropertiesByUrl
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="dic"></param>
        public void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> dic)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var siteProp = tenant.GetSitePropertiesByUrl(siteUrl, true);
                AveObjectCopy.UpdateObjectBasicProperties(dic, siteProp);
                siteProp.Update();
                context.ExecuteQuery();
            };
        }

        public Dictionary<string, object> GetDeletedSitePropertiesByUrl(string siteUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var deleteSiteProp = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                context.Load(deleteSiteProp);
                context.ExecuteQuery();
                Dictionary<string, object> sitePropDic = new Dictionary<string, object>();
                CopyProperty(sitePropDic, deleteSiteProp);
                return sitePropDic;
            }
        }

        /*
         * copy logic of tenant.SiteExistsAnywhere
         */
        public SiteExistence SiteExistsAnywhere(string siteUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                try
                {
                    // CHANGED: Modified in order to support non privilege users
                    // Get the site name
                    var properties = tenant.GetSitePropertiesByUrl(siteUrl, false);
                    tenant.Context.Load(properties);
                    tenant.Context.ExecuteQueryRetry();
                    // Will cause an exception if site URL is not there. Not optimal, but the way it works.
                    return SiteExistence.Yes;
                }
                catch (Exception ex)
                {
                    if (IsCannotGetSiteException(ex) || IsUnableToAccessSiteException(ex))
                    {
                        if (IsUnableToAccessSiteException(ex))
                        {
                            //Let's retry to see if this site collection was recycled
                            try
                            {
                                var deletedProperties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                                tenant.Context.Load(deletedProperties);
                                tenant.Context.ExecuteQueryRetry();
                                if (deletedProperties.Status.Equals("Recycled", StringComparison.OrdinalIgnoreCase))
                                {
                                    return SiteExistence.Recycled;
                                }
                                else
                                {
                                    return SiteExistence.No;
                                }
                            }
                            catch
                            {
                                return SiteExistence.No;
                            }
                        }
                        else
                        {
                            return SiteExistence.No;
                        }
                    }
                    else
                    {
                        return SiteExistence.Yes;
                    }
                }
            }
        }

        private static bool IsCannotGetSiteException(Exception ex)
        {
            if (ex is ServerException)
            {
                if (((ServerException)ex).ServerErrorCode == -1 && ((ServerException)ex).ServerErrorTypeName.Equals("Microsoft.Online.SharePoint.Common.SpoNoSiteException", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static bool IsUnableToAccessSiteException(Exception ex)
        {
            if (ex is ServerException)
            {
                if (
                     (((ServerException)ex).ServerErrorCode == -2147024809 && ((ServerException)ex).ServerErrorTypeName.Equals("System.ArgumentException", StringComparison.InvariantCultureIgnoreCase)) ||
                     (((ServerException)ex).ServerErrorCode == -1 && ((ServerException)ex).ServerErrorTypeName.Equals("Microsoft.Online.SharePoint.Common.SpoNoSiteException", StringComparison.InvariantCultureIgnoreCase))
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Dictionary<string, object> GetObjectSharingInformationByUrl(string objectUrl, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformationByUrl(context, objectUrl, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetWebSharingInformation(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests)
        {
            using (AveClientContext context = CreateContext())
            {
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetWebSharingInformation(context, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetWebSharingInformation2(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation(context, context.Web, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetWebSharingInformation3(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation2(context, context.Web, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetListSharingInformation(Guid listGuid, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                List list = context.Web.Lists.GetById(listGuid);
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation(context, list, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetListSharingInformation2(Guid listGuid, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                List list = context.Web.Lists.GetById(listGuid);
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation2(context, list, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetListItemSharingInformation(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests)
        {
            using (AveClientContext context = CreateContext())
            {
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetListItemSharingInformation(context, listID, itemID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetListItemSharingInformation2(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                List list = context.Web.Lists.GetById(listID);
                ListItem item = list.GetItemById(itemID);
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation(context, item, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }
        public Dictionary<string, object> GetListItemSharingInformation3(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels)
        {
            using (AveClientContext context = CreateContext())
            {
                List list = context.Web.Lists.GetById(listID);
                ListItem item = list.GetItemById(itemID);
                ObjectSharingInformation shareInfo = ObjectSharingInformation.GetObjectSharingInformation2(context, item, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
                var dic = GetObjectSharingInformation(shareInfo, context);
                return dic;
            }
        }

        #region GetObjectSharingInformation
        private Dictionary<string, object> GetObjectSharingInformation(ObjectSharingInformation shareInfo, AveClientContext context)
        {
            context.Load(shareInfo);
            var shareUsers = shareInfo.GetSharedWithUsers();
            context.Load(shareUsers, s => s.IncludeWithDefaultProperties(u => u.User, u => u.Principal, u => u.User.Groups));
            context.ExecuteQuery();
            var dic = new Dictionary<string, object>();
            dic["AnonymousEditLink"] = shareInfo.AnonymousEditLink;
            dic["AnonymousViewLink"] = shareInfo.AnonymousViewLink;
            dic["CanBeShared"] = shareInfo.CanBeShared;
            dic["CanBeUnshared"] = shareInfo.CanBeUnshared;
            dic["CanManagePermissions"] = shareInfo.CanManagePermissions;
            dic["HasPendingAccessRequests"] = shareInfo.HasPendingAccessRequests;
            dic["HasPermissionLevels"] = shareInfo.HasPermissionLevels;
            dic["IsSharedWithCurrentUser"] = shareInfo.IsSharedWithCurrentUser;
            dic["IsSharedWithGuest"] = shareInfo.IsSharedWithGuest;
            dic["IsSharedWithMany"] = shareInfo.IsSharedWithMany;
            dic["IsSharedWithSecurityGroup"] = shareInfo.IsSharedWithSecurityGroup;
            dic["PendingAccessRequestsLink"] = shareInfo.PendingAccessRequestsLink;
            var shareUsersList = GetSharedWithUsers(shareUsers, context);
            dic["SharedWithUsers"] = shareUsersList;
            dic["SharingLinks"] = GetSharingLinks(shareInfo.SharingLinks);

            return dic;
        }

        private List<Dictionary<string, object>> GetSharingLinks(IEnumerable<SharingLinkInfo> sharingLinks)
        {
            var sharingLinkList = new List<Dictionary<string, object>>();
            foreach (var sharingLink in sharingLinks)
            {
                Dictionary<string, object> dicLink = new Dictionary<string, object>();
                dicLink["IsDefault"] = sharingLink.IsDefault;
                dicLink["ShareId"] = sharingLink.ShareId;
                dicLink["RestrictedShareMembership"] = sharingLink.RestrictedShareMembership;
                dicLink["RequiresPassword"] = sharingLink.RequiresPassword;
                dicLink["PasswordLastModified"] = sharingLink.PasswordLastModified;
                dicLink["SharingLinkKind"] = sharingLink.LinkKind.ToString();
                dicLink["LimitUseToApplication"] = sharingLink.LimitUseToApplication;
                dicLink["LastModified"] = sharingLink.LastModified;
                dicLink["IsUnhealthy"] = sharingLink.IsUnhealthy;
                dicLink["IsReviewLink"] = sharingLink.IsReviewLink;
                dicLink["IsFormsLink"] = sharingLink.IsFormsLink;
                dicLink["IsEditLink"] = sharingLink.IsEditLink;
                dicLink["TypeId"] = sharingLink.TypeId;
                dicLink["IsCreateOnlyLink"] = sharingLink.IsCreateOnlyLink;
                dicLink["IsActive"] = sharingLink.IsActive;
                dicLink["HasExternalGuestInvitees"] = sharingLink.HasExternalGuestInvitees;
                dicLink["Expiration"] = sharingLink.Expiration;
                dicLink["Description"] = sharingLink.Description;
                dicLink["Created"] = sharingLink.Created;
                dicLink["BlocksDownload"] = sharingLink.BlocksDownload;
                dicLink["ApplicationId"] = sharingLink.ApplicationId;
                dicLink["AllowsAnonymousAccess"] = sharingLink.AllowsAnonymousAccess;
                dicLink["Url"] = sharingLink.Url;
                sharingLinkList.Add(dicLink);
            }
            return sharingLinkList;
        }

        private List<Dictionary<string, object>> GetSharedWithUsers(ClientObjectList<ObjectSharingInformationUser> shareUsers, AveClientContext context)
        {
            var shareUsersList = new List<Dictionary<string, object>>();
            foreach (var user in shareUsers)
            {
                Dictionary<string, object> dicUser = new Dictionary<string, object>();
                dicUser["CustomRoleNames"] = user.CustomRoleNames;
                dicUser["Department"] = user.Department;
                dicUser["Email"] = user.Email;
                dicUser["HasEditPermission"] = user.HasEditPermission;
                dicUser["HasViewPermission"] = user.HasViewPermission;
                dicUser["Id"] = user.Id;
                dicUser["IsDomainGroup"] = user.IsDomainGroup;
                dicUser["IsSiteAdmin"] = user.IsSiteAdmin;
                dicUser["JobTitle"] = user.JobTitle;
                dicUser["LoginName"] = user.LoginName;
                dicUser["Name"] = user.Name;
                dicUser["Picture"] = user.Picture;

                Dictionary<string, object> princInfo = GetPrincipalInfo(user.Principal);
                dicUser["Principal"] = princInfo;

                dicUser["SipAddress"] = user.SipAddress;

                User userInfo = user.User;
                Dictionary<string, object> userInfoDic = new Dictionary<string, object>();
                userInfoDic["Email"] = userInfo.Email;
                userInfoDic["IsShareByEmailGuestUser"] = userInfo.IsShareByEmailGuestUser;

                var groupsList = GetGroupsInfo(userInfo.Groups, context);
                userInfoDic["Groups"] = groupsList;
                userInfoDic["IsSiteAdmin"] = userInfo.IsSiteAdmin;
                userInfoDic["UserId"] = userInfo.UserId;

                dicUser["User"] = userInfoDic;
                shareUsersList.Add(dicUser);
            }
            return shareUsersList;
        }
        private Dictionary<string, object> GetPrincipalInfo(Principal princ)
        {
            var princInfo = new Dictionary<string, object>();
            princInfo["Id"] = princ.Id;
            princInfo["IsHiddenInUI"] = princ.IsHiddenInUI;
            princInfo["LoginName"] = princ.LoginName;
            princInfo["Title"] = princ.Title;
            princInfo["PrincipalType"] = princ.PrincipalType;
            return princInfo;
        }

        private List<Dictionary<string, object>> GetGroupsInfo(GroupCollection groups, AveClientContext context)
        {
            context.Load(groups, s => s.IncludeWithDefaultProperties(g => g.CanCurrentUserEditMembership, g => g.CanCurrentUserManageGroup, g => g.CanCurrentUserViewMembership));
            context.ExecuteQuery();
            var groupsList = new List<Dictionary<string, object>>();
            foreach (var group in groups)
            {
                var groupDic = new Dictionary<string, object>();
                groupDic["AllowMembersEditMembership"] = group.AllowMembersEditMembership;
                groupDic["AllowRequestToJoinLeave"] = group.AllowRequestToJoinLeave;
                groupDic["AutoAcceptRequestToJoinLeave"] = group.AutoAcceptRequestToJoinLeave;
                groupDic["CanCurrentUserEditMembership"] = group.CanCurrentUserEditMembership;
                groupDic["CanCurrentUserManageGroup"] = group.CanCurrentUserManageGroup;
                groupDic["CanCurrentUserViewMembership"] = group.CanCurrentUserViewMembership;
                groupDic["Description"] = group.Description;
                groupDic["OnlyAllowMembersViewMembership"] = group.OnlyAllowMembersViewMembership;
                groupDic["Owner"] = group.Owner;
                groupDic["OwnerTitle"] = group.OwnerTitle;
                groupDic["RequestToJoinLeaveEmailSetting"] = group.RequestToJoinLeaveEmailSetting;
                groupsList.Add(groupDic);
            }
            return groupsList;
        }
        #endregion

        public int CanCurrentUserShare(string docId)
        {
            using (AveClientContext context = CreateContext())
            {
                ClientResult<UserSharingCapabilities> clientResult = ObjectSharingInformation.CanCurrentUserShare(context, docId);
                context.ExecuteQuery();
                return (int)clientResult.Value;
            }
        }
        public int CanCurrentUserShareRemote(string docId)
        {
            using (AveClientContext context = CreateContext())
            {
                ClientResult<UserSharingCapabilities> clientResult = ObjectSharingInformation.CanCurrentUserShareRemote(context, docId);
                context.ExecuteQuery();
                return (int)clientResult.Value;
            }
        }
        public Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIDs, string loginName)
        {
            Dictionary<string, object> newPersonalSiteProperty = new Dictionary<string, object>();
            DateTime endTime = DateTime.Now.AddMinutes(30);  //设置时间为30分钟，如果超出时间则停止等待。
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    ProfileLoader profileLoader = ProfileLoader.GetProfileLoader(context);
                    PeopleManager peopleManager = new PeopleManager(context);
                    ClientResult<string> result = null;
                    profileLoader.CreatePersonalSiteEnqueueBulk(emailIDs);
                    context.ExecuteQuery();
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        do
                        {
                            System.Threading.Thread.Sleep(10000);
                            if (DateTime.Now > endTime)
                            {
                                throw new Exception("Create Site Collection timeout.");
                            }
                            result = peopleManager.GetUserProfilePropertyFor(loginName, "SPS-PersonalSiteInstantiationState");
                            context.ExecuteQuery();
                        } while (!result.Value.Equals(((int)PersonalSiteInstantiationState.Created).ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create Personal Site,  error message : {0}", e.ToString());
                newPersonalSiteProperty["ErrorMessage"] = e is ServerException ? "ServerException" + e.Message : e.Message; ;
            }
            return newPersonalSiteProperty;
        }
        public Dictionary<Guid, string> GetListTitleResource(string webServerRelativeUrl, string cultureName)
        {
            Dictionary<Guid, string> titleResources = new Dictionary<Guid, string>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web.Lists, a => a.Include(l => l.Id, l => l.Title));
                context.ExecuteQuery();
                Dictionary<Guid, ClientResult<string>> resources = new Dictionary<Guid, ClientResult<string>>();

                foreach (var list in web.Lists)
                {
                    resources[list.Id] = list.TitleResource.GetValueForUICulture(cultureName);
                }
                context.ExecuteQuery();

                foreach (KeyValuePair<Guid, ClientResult<string>> keyValue in resources)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value.Value))
                    {
                        titleResources[keyValue.Key] = keyValue.Value.Value;
                    }
                    else
                    {
                        mLogger.Warn("The title resource for list:{0} with culture:{1} under web:{2} is empty.", keyValue.Key, cultureName, webServerRelativeUrl);
                    }
                }
            }

            return titleResources;
        }

        public string GetListTitleResource(string webServerRelativeUrl, Guid id, string cultureName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(id);
                var titleResource = list.TitleResource.GetValueForUICulture(cultureName);
                context.ExecuteQuery();

                return titleResource.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="cultureName"></param>
        /// <param name="resourceName">AveUserResourceConstants.TITLE_RESOUCE or AveUserResourceConstants.DESCRIPTION_RESOUCE</param>
        /// <returns></returns>
        public string GetWebUserResource(string webServerRelativeUrl, string cultureName, string resourceName)
        {
            ClientResult<string> value;
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        value = web.TitleResource.GetValueForUICulture(cultureName);
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        value = web.DescriptionResource.GetValueForUICulture(cultureName);
                        break;
                    default:
                        throw new Exception(string.Format("resource name is invalid.{0}", resourceName));
                }
                context.ExecuteQuery();
                return value.Value;
            }
        }

        public void SetListTitleResource(string webServerRelativeUrl, Guid id, Dictionary<string, string> changedTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(id);
                foreach (KeyValuePair<string, string> keyValue in changedTitle)
                {
                    list.TitleResource.SetValueForUICulture(keyValue.Key, keyValue.Value);
                }
                context.ExecuteQuery();
            }
        }

        public void SetWebUserResource(string webServerRelativeUrl, string resourceName, Dictionary<string, string> changedTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                UserResource userResource;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        userResource = web.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        userResource = web.DescriptionResource;
                        break;
                    default:
                        throw new Exception(string.Format("resource name is invalid.{0}", resourceName));
                }
                foreach (KeyValuePair<string, string> keyValue in changedTitle)
                {
                    userResource.SetValueForUICulture(keyValue.Key, keyValue.Value);
                }
                web.Update();
                context.ExecuteQuery();
            }
        }

        public string GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, string cultureName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(id);
                var userResource = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        userResource = list.TitleResource.GetValueForUICulture(cultureName);
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        userResource = list.DescriptionResource.GetValueForUICulture(cultureName);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }

                return userResource.Value;
            }
        }

        #region

        public void GetSubItemsFromFolder(ClientContext context, Web web, List list, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> parentFolderProp,uint maxItemsPerThrottledOperation, ref string pageInfo)
        {
            List<Dictionary<string, object>> items = parentFolderProp["Items"] as List<Dictionary<string, object>>;
            string rootFolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            int subfolderCount = folder.Properties.FieldValues.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folder.Properties.FieldValues["vti_foldersubfolderitemcount"]) : 0;
            int totalItemCount = folder.ItemCount - subfolderCount;//list.RootFolder.ItemCount;
            mLogger.Info("start query list items from folder,items and folders count:{0},items count:{1},folder ServerRelativeUrl:{2}", folder.ItemCount, totalItemCount, folderServerRelativeUrl);
            this.SwitchListContext(list, folderServerRelativeUrl);
            bool firstPage = string.IsNullOrEmpty(pageInfo) ? true : false;
            bool queryException = false;
            if (totalItemCount > 0)
            {
                IList<int> subItemIds;
                if (this.mCurrentList.FoldersToSubItemIds.ContainsKey(folderServerRelativeUrl))
                {
                    subItemIds = this.mCurrentList.FoldersToSubItemIds[folderServerRelativeUrl];
                }
                else
                {
                    subItemIds = new List<int>(totalItemCount);
                    this.mCurrentList.FoldersToSubItemIds[folderServerRelativeUrl] = subItemIds;
                }
                List<Dictionary<string, object>> listItems = new List<Dictionary<string, object>>();

                if (totalItemCount <= 5000 && firstPage)
                {
                    try
                    {
                        listItems = CacheItems(context, list, folderServerRelativeUrl, subItemIds);
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Get all list items failed without paging, will try paging later, error:{0}", e.ToString());
                        queryException = true;
                    }
                }
                if (totalItemCount > 5000 || !firstPage || queryException)
                {
                    if (this.mCurrentList.FolderPageInfo.StartIndex == 0)
                    {
                        this.mCurrentList.FolderPageInfo.SurplusCount = totalItemCount;
                        int folderId = folder.ListItemAllFields.ServerObjectIsNull.Value ? 0 : folder.ListItemAllFields.Id;
                        this.mCurrentList.FolderPageInfo.StartIndex = this.GetStartItemId(context, list, folderServerRelativeUrl, folderId);   //获取当前folder下的第一个Item和最后一个item的id来作为分页查询的结束条件
                        this.mCurrentList.FolderPageInfo.EndIndex = this.GetLastItemId(context, list, folderServerRelativeUrl);
                    }
                    if (this.mCurrentList.FolderPageInfo.StartIndex != 0
                        && this.mCurrentList.FolderPageInfo.StartIndex <= this.mCurrentList.FolderPageInfo.EndIndex)
                    {
                        listItems = CacheItemsInPage(context, list, folderServerRelativeUrl, subItemIds, ref pageInfo);
                    }
                }
                //else
                //{
                //    listItems = CacheItems(context, list, folderServerRelativeUrl, subItemIds);
                //}
                foreach (Dictionary<string, object> item in listItems)
                {
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance && ItemHasVersion(list, item))
                    {
                        item["Versions"] = new List<Dictionary<string, object>>();
                    }
                    item["Attachments"] = new List<Dictionary<string, object>>();
                    item["RbsId"] = null;
                    if (list.BaseType != BaseType.DocumentLibrary)
                    {
                        GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl);
                    }
                    items.Add(item);
                }
            }
            //GetSystemFoldersAndFiles(context, folders, items, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl);
            //Add to Query View Item by Client API
            //AddViewItems(context, list, folderServerRelativeUrl, items, folders);
            if (firstPage && folder.ListItemAllFields.ServerObjectIsNull.Value)
            {
                GetSystemFiles(context, items, list, folder, folderServerRelativeUrl,maxItemsPerThrottledOperation);
                AddViewItems(context, list, folderServerRelativeUrl, items,maxItemsPerThrottledOperation);
            }
        }

        private List<Dictionary<string, object>> CacheItemsInPage(ClientContext context, List list, string folderUrl, IList<int> subItemIds, ref string pageInfo)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            //SwitchListContext(list);
            int startIndex = this.mCurrentList.FolderPageInfo.StartIndex;
            int lastIndex = this.mCurrentList.FolderPageInfo.EndIndex;
            if (!string.IsNullOrEmpty(pageInfo))
            {
                try
                {
                    string[] pageInfos = pageInfo.Split('&');
                    Dictionary<string, string> pageInfoDic = pageInfos.ToDictionary(v => v.Split('=')[0], v => v.Split('=')[1]);//SAAS-14378 将pageInfo中的信息转化成Dictionary，然后取出p_ID的值。
                    startIndex = pageInfoDic.ContainsKey("p_ID") ? Convert.ToInt32(pageInfoDic["p_ID"]) + 1 : startIndex;
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Analyse pageinfo index failed.Error Message:{0}", ex.ToString());
                }
            }
            int endIndex = startIndex + this.mCurrentList.FolderPageInfo.QueryRange;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
            //"<View Scope=\"RecursiveAll\">" +
            "<View Scope=\"FilesOnly\"><Query><Where><And><Geq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Geq><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query><RowLimit>{2}</RowLimit></View>",
            startIndex, endIndex, RowIdStep);
            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);
            ListItemCollection listItems = null;
            ListItemCollection listItems2 = null;
            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
            using (ehScope.StartScope())
            {
                using (ehScope.StartTry())
                {
                    listItems = list.GetItems(camlQuery);
                    if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        context.Load(listItems, items => items.ListItemCollectionPosition,
                            items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                            item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus).Where(item => (string)item["FSObjType"] == "0"));
                    }
                    else
                    {
                        context.Load(listItems, items => items.ListItemCollectionPosition,
                            items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                    }
                }
                using (ehScope.StartCatch())
                {
                    endIndex = startIndex + 3999;
                    camlQuery.ViewXml = string.Format(
                        "<View Scope=\"FilesOnly\"><Query><Where><And><Geq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Geq><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query><RowLimit>{2}</RowLimit></View>",
                        startIndex, endIndex, RowIdStep);
                    listItems2 = list.GetItems(camlQuery);
                    if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        context.Load(listItems2, items => items.ListItemCollectionPosition,
                            items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus, item => item.File.TimeLastModified));
                    }
                    else
                    {
                        context.Load(listItems2, items => items.ListItemCollectionPosition,
                            items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                    }
                }
            }

            context.ExecuteQuery();
            if (ehScope.HasException)
            {
                mLogger.Warn("load item row id failed during cacheitemsinpage. folderUrl:{0}. ErrorMessage: {1},startindex:{2},endindex:{3}", folderUrl, ehScope.ErrorMessage, startIndex.ToString(), endIndex.ToString());
                this.mCurrentList.FolderPageInfo.QueryRange = 3999;
                listItems = listItems2;
            }

            int currentItemId = endIndex;
            this.mCurrentList.FolderPageInfo.SurplusCount -= listItems.Count;
            for (int i = 0; i < listItems.Count; i++)
            {
                Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                currentItemId = listItems[i].Id;
                GetItemDic(itemProperty, listItems[i]);
                itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;

                //if (listItems[i].FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(listItems[i]["FileRef"] as string) && filesMap.ContainsKey(listItems[i]["FileRef"] as string))
                //{
                //    string fileRelativeUrl = listItems[i]["FileRef"] as string;
                //    ClientFile file = filesMap[fileRelativeUrl];
                //    itemProperty["ServerRelativeUrl"] = fileRelativeUrl;
                //    Dictionary<string, object> fileProperty = new Dictionary<string, object>();
                //    //AssembleFileProperties(fileProperty, file, webServerRelativeUrl, listItems[i]);
                //    AssembleBasicFileProperties(fileProperty, file, webServerRelativeUrl);
                //    itemProperty["File" + AveObjectModelConstant.ObjectPropertySuffix] = fileProperty;
                //    //this.mCurrentList.Files[fileRelativeUrl] = fileProperty;
                //    //filesets.Add(fileRelativeUrl);
                //}
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    itemProperty["CustomizedPageStatus"] = (int)listItems[i].File.CustomizedPageStatus;
                    itemProperty["ObjType"] = 2;
                }
                else
                {
                    itemProperty["ObjType"] = 1;
                }

                this.mCurrentList.Items[currentItemId] = itemProperty;
                subItemIds.Add(currentItemId);
                results.Add(itemProperty);
            }

            if (listItems.ListItemCollectionPosition != null)
            {
                pageInfo = listItems.ListItemCollectionPosition.PagingInfo;
            }
            else if (!(this.mCurrentList.FolderPageInfo.SurplusCount == 0 || currentItemId >= lastIndex))
            {
                pageInfo = string.Format("Paged=TRUE&p_ID={0}", currentItemId);  //SAAS-14755 如果两个item id之间间隔比较大，pageinfo有可能是null，但是其实后面还有Item
            }
            else
            {
                pageInfo = null;
            }

            #region 防止SurplusCount计数不准无限query

            if (listItems.Count > 0)
            {
                this.mCurrentList.FolderPageInfo.QueryTimer.Reset();
            }
            else
            {
                if (!this.mCurrentList.FolderPageInfo.QueryTimer.IsRunning)
                {
                    this.mCurrentList.FolderPageInfo.QueryTimer.Start();
                }
                else if (this.mCurrentList.FolderPageInfo.QueryTimer.ElapsedMilliseconds > CACHE_TIME_OUT)
                {
                    mLogger.Warn("Timeout when caching items under list : {0}", folderUrl);
                    pageInfo = null;
                }
            }
            #endregion

            mLogger.Info("load all items under list:{0} under folder:{1} from {2} to {3}, count:{4}", list.Title, folderUrl, startIndex, endIndex, listItems.Count);
            EnsureParentThreadId(list, results);   //原来listItem集合是一次全部获取，现在改成只获取当前folder下的item，对于嵌套的discusstionboard可能有问题，需要验证一下

            return results;
        }

        private List<Dictionary<string, object>> CacheItems(ClientContext context, List list, string folderUrl, IList<int> subItemIds)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            //SwitchListContext(list);

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View Scope=\"FilesOnly\"></View>";
            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);

            ListItemCollection listItems = list.GetItems(camlQuery);
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                context.Load(listItems, items => items.ListItemCollectionPosition,
                    items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                    item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus).Where(item => (string)item["FSObjType"] == "0"));
            }
            else
            {
                context.Load(listItems, items => items.ListItemCollectionPosition,
                    items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
            }
            context.ExecuteQuery();

            for (int i = 0; i < listItems.Count; i++)
            {
                Dictionary<string, object> itemProperty = new Dictionary<string, object>();

                GetItemDic(itemProperty, listItems[i]);
                itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;

                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    if (!listItems[i].File.ServerObjectIsNull.Value)
                    {
                        itemProperty["CustomizedPageStatus"] = (int)listItems[i].File.CustomizedPageStatus;
                    }
                    itemProperty["ObjType"] = 2;
                }
                else
                {
                    itemProperty["ObjType"] = 1;
                }

                this.mCurrentList.Items[listItems[i].Id] = itemProperty;
                subItemIds.Add(listItems[i].Id);
                results.Add(itemProperty);
            }
            mLogger.Info("load all items under folder:{0}, count {1}", folderUrl, listItems.Count);
            EnsureParentThreadId(list, results);   //原来listItem集合是一次全部获取，现在改成只获取当前folder下的item，对于嵌套的discusstionboard可能有问题，需要验证一下

            return results;
        }

        private int GetLastItemId(ClientContext context, List list, string folderServerRelativeUrl)
        {
            //ID 是index，查询应该很快, 只查询当前folder下的最后一个item的id
            try
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                "<View Scope=\"RecursiveAll\">" +
                "<Query><OrderBy><FieldRef Name=\"ID\" Ascending=\"FALSE\"/></OrderBy></Query>" +
                "<RowLimit>1</RowLimit></View>";
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(list.RootFolder.ServerRelativeUrl);
                ListItemCollection items = list.GetItems(camlQuery);
                context.Load(items, its => its.Include(it => it.Id));
                context.ExecuteQuery();
                if (items != null && items.Count > 0)
                {
                    ListItem lastItem = items[0];
                    return lastItem.Id;
                }
            }
            /*review-qlluo*/
            catch (Exception e)
            {
                mLogger.Warn("get last index failed. folderUrl:{0}, error message:{1}", folderServerRelativeUrl, e.Message);
            }
            return int.MaxValue;
        }


        private int GetStartItemId(ClientContext context, List list, string folderServerRelativeUrl, int folderId)
        {
            //ID 是index，查询应该很快, 只查询当前folder下的最后一个item的id
            try
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                    "<View Scope=\"FilesOnly\">" +
                    "<Query></Query>" +
                    "<RowLimit>1</RowLimit></View>";
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderServerRelativeUrl);
                ListItemCollection items = null;
                ListItemCollection tempItems = null;

                ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                using (ehScope.StartScope())
                {
                    using (ehScope.StartTry())
                    {
                        items = list.GetItems(camlQuery);
                        context.Load(items, its => its.Include(it => it.Id));
                    }
                    using (ehScope.StartCatch())
                    {
                        camlQuery.ViewXml =
                            "<View Scope=\"RecursiveAll\">" +
                            "<Query></Query>" +
                            "<RowLimit>1</RowLimit></View>";
                        tempItems = list.GetItems(camlQuery);
                        context.Load(tempItems, its => its.Include(it => it.Id));
                    }
                }
                context.ExecuteQuery();
                if (ehScope.HasException)
                {
                    items = tempItems;
                    mLogger.Info("load item row id failed during GetStartItemId. folderUrl:{0}. ErrorMessage: {1}", folderServerRelativeUrl, ehScope.ErrorMessage);
                }
                if (items != null && items.Count > 0)
                {
                    ListItem startItem = items[0];
                    return startItem.Id;
                }
            }
            /*review-qlluo*/
            catch (Exception e)
            {
                mLogger.Warn("get start index failed. folderUrl:{0}, error message:{1}", folderServerRelativeUrl, e.Message);
            }
            return folderId + 1;
        }

        public void GetSubFoldersFromFolder(ClientContext context, Web web, List list, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> parentFolder,uint maxItemsPerThrottledOperation=5000)
        {
            List<Dictionary<string, object>> items = parentFolder["Items"] as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> folders = parentFolder["Folders"] as List<Dictionary<string, object>>;
            string rootFolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            int totalItemCount = folder.ItemCount;//list.RootFolder.ItemCount;            
            int subfolderCount = folder.Properties.FieldValues.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folder.Properties.FieldValues["vti_foldersubfolderitemcount"]) : 0;
            this.SwitchListContext(list);
            IList<int> subItemIds = new List<int>(totalItemCount);
            //this.mCurrentList.FoldersToSubItemIds[folderServerRelativeUrl] = subItemIds;
            if (totalItemCount > 0 && subfolderCount > 0)
            {
                //Query Folder                                            
                List<Dictionary<string, object>> listItems = GetFoldersByCamlIncludeRequestedFields(context, list, web.ServerRelativeUrl, folderServerRelativeUrl, subfolderCount, subItemIds);

                foreach (Dictionary<string, object> item in listItems)
                {
                    item["Items"] = new List<Dictionary<string, object>>();
                    item["Folders"] = new List<Dictionary<string, object>>();
                    item["Attachments"] = new List<Dictionary<string, object>>();
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance && ItemHasVersion(list, item))
                    {
                        item["Versions"] = new List<Dictionary<string, object>>();
                    }
                    item["ItemId"] = item["Id"];
                    item["Hidden"] = (item["Id"] == null) ? true : false;
                    GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl);
                    folders.Add(item);
                }
            }
            //GetSystemFoldersAndFiles(context, folders, items, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl);
            //Add to Query View Item by Client API
            //AddViewItems(context, list, folderServerRelativeUrl, items, folders);
            if (folder.ListItemAllFields.ServerObjectIsNull.Value)
            {
                GetSystemFolders(context, folders, folder, maxItemsPerThrottledOperation);
                AddViewFolders(context, list, folderServerRelativeUrl, folders, maxItemsPerThrottledOperation);
            }
        }

        private void AddViewItems(ClientContext context, List list, string folderServerRelativeUrl, List<Dictionary<string, object>> items, uint maxItemsPerThrottledOperation)
        {
            bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
            
            if (!isRootFolder && !isForms)
            {
                return;
            }
			if (!WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView)
            {
                mLogger.Warn($"Views will not be included as IncludeListView is false.CurrentFolder:{folderServerRelativeUrl}");
                return;
            }
            //library forms and list root folder ,should load view files
            if ((list.BaseType == BaseType.DocumentLibrary && isForms)
                || (list.BaseType != BaseType.DocumentLibrary && isRootFolder))
            {
                var viewFiles = LoadViewFiles(context, list, folderServerRelativeUrl, maxItemsPerThrottledOperation);
                if (viewFiles != null && viewFiles.Count > 0)
                {
                    foreach (var viewFile in viewFiles)
                    {
                        this.mCurrentList.Files[viewFile.Key] = viewFile.Value;
                        items.Add(viewFile.Value);
                    }
                }
            }
        }

        private Dictionary<string,Dictionary<string,object>> LoadViewFiles(ClientContext context, List list, string folderServerRelativeUrl, uint maxItemsPerThrottledOperation)
        {
            Dictionary<string, Dictionary<string, object>> viewFileProperties = new Dictionary<string, Dictionary<string, object>>();
            List<ClientFile> viewFiles = null;
            Folder folder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
            context.Load(folder, f => f.ItemCount, f => f.ServerRelativeUrl);
            context.ExecuteQuery();
            try
            {
                if (folder.ItemCount < maxItemsPerThrottledOperation)
                {
                    context.Load(folder.Files);
                    context.ExecuteQuery();
                    viewFiles = folder.Files.ToList();
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn($"Discover system folder view files failed, will try will views instead.Folder:{folderServerRelativeUrl},ItemCount:{folder.ItemCount},maxItemsPerThrottledOperation:{maxItemsPerThrottledOperation},Error:{ex}");
            }
            if (viewFiles != null)
            {
                foreach (ClientFile viewFile in viewFiles)
                {
                    Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                    AssembleViewFileProperties(itemProperty, viewFile);
                    itemProperty["ObjType"] = 2;//set default value to 2.
                    itemProperty["IsSystemFile"] = true;
                    viewFileProperties[viewFile.ServerRelativeUrl]=itemProperty;
                }
            }
            else
            {
                context.Load(list.Views);
                context.ExecuteQuery();
                foreach (View view in list.Views)
                {
                    context.Load(list.Views);
                    if (!string.IsNullOrEmpty(view.ServerRelativeUrl) && view.ServerRelativeUrl.StartsWith(folderServerRelativeUrl.TrimEnd('/') + '/'))
                    {
                        ClientFile viewFile = list.ParentWeb.GetFileByServerRelativePath(view.ServerRelativePath);
                        context.Load(viewFile);
                        context.ExecuteQuery();
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        AssembleViewFileProperties(itemProperty, viewFile);
                        itemProperty["ObjType"] = 2;//set default value to 2.
                        itemProperty["IsSystemFile"] = true;
                        viewFileProperties[viewFile.ServerRelativeUrl]=itemProperty;
                    }
                }
            }
            return viewFileProperties;
        }

        private void AddViewFolders(ClientContext context, List list, string folderServerRelativeUrl, List<Dictionary<string, object>> folders,uint maxItemsPerThrottledOperation)
        {
            bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
            if (!isRootFolder && !isForms)
            {
                return;
            }
            if (list.BaseType == BaseType.DocumentLibrary && isRootFolder)
            {
                try
                {
                    Folder formsFolder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl + "/Forms"));
                    context.Load(formsFolder);
                    context.ExecuteQuery();
                    Dictionary<string, object> itemPro = new Dictionary<string, object>();
                    itemPro["Items"] = new List<Dictionary<string, object>>();
                    itemPro["Folders"] = new List<Dictionary<string, object>>();
                    itemPro["Attachments"] = new List<Dictionary<string, object>>();
                    itemPro["Versions"] = new List<Dictionary<string, object>>();
                    AssembleViewFolderProperties(itemPro, formsFolder);
                    itemPro["IsSystemFile"] = true;
                    itemPro["ObjType"] = 4;  //Folder
                    itemPro["ItemId"] = itemPro["Id"];
                    //this.mCurrentList.Folders[formsFolder.ServerRelativeUrl] = itemPro;
                    folders.Add(itemPro);
                }
                catch (Exception e)
                {
                    mLogger.Debug($"Add View folder {e.ToString()}");
                }
            }
        }

        private void GetSystemFiles(ClientContext context, List<Dictionary<string, object>> items, List list, Folder folder, string folderServerRelativeUrl,uint maxItemsPerThrottledOperation)
        {
            if (folder.ListItemAllFields.ServerObjectIsNull.Value)
            {
                bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
                if (isRootFolder || isForms)
                {
                    return;
                }
                if (folder.ItemCount >= maxItemsPerThrottledOperation)
                {
                    return;
                }
                try
                {
                    context.Load(folder.Files);
                    context.ExecuteQuery();

                    if (folder.ItemCount == 0 && folder.Files.Count > 0)
                    {
                        foreach (Microsoft.SharePoint.Client.File file in folder.Files)
                        {
                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            AssembleViewFileProperties(itemProperty, file);
                            itemProperty["ObjType"] = 2;//set default value to 2.
                            itemProperty["IsSystemFile"] = true;
                            items.Add(itemProperty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn($"Load system files under folder {folderServerRelativeUrl} failed.Error:{ex}");
                }
            }
        }

        private void GetSystemFolders(ClientContext context, List<Dictionary<string, object>> folders, Folder folder,uint maxItemsPerThrottledOperation)
        {
            try
            {
                if (folder.ListItemAllFields.ServerObjectIsNull.Value)
                {
                    if (!folder.Folders.AreItemsAvailable)
                    {
                        if (folder.ItemCount > maxItemsPerThrottledOperation)
                        {
                            mLogger.Warn("Item count under folder {folder.ServerRelativeUrl} has reach max items per throttled operation.Discover system folders under this folder will be skipped.");
                            return;
                        }
                        context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ListItemAllFields, f => f.Properties).Where(f => f.ListItemAllFields.ServerObjectIsNull.Value));
                        context.ExecuteQuery();
                    }
                    foreach (var subFolder in folder.Folders)
                    {
                        if (subFolder.ListItemAllFields.ServerObjectIsNull.Value && !subFolder.Name.Equals("Attachments", StringComparison.OrdinalIgnoreCase) && !subFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            itemProperty["ObjType"] = 4;
                            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = subFolder.ListItemAllFields.FieldValues.ContainsKey("Attachments") ? subFolder.ListItemAllFields.FieldValues["Attachments"] : false;
                            itemProperty["FullUrl"] = subFolder.ServerRelativeUrl;
                            itemProperty["ServerRelativeUrl"] = subFolder.ServerRelativeUrl;
                            itemProperty["LeafName"] = subFolder.Name;
                            itemProperty["Items"] = new List<Dictionary<string, object>>();
                            itemProperty["Folders"] = new List<Dictionary<string, object>>();
                            itemProperty["Attachments"] = new List<Dictionary<string, object>>();
                            itemProperty["ItemId"] = itemProperty["Id"] = null;
                            itemProperty["Hidden"] = true; //(itemProperty["Id"] == null) ? true : false;
                            itemProperty["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
                            itemProperty["IsSystemFile"] = true;
                            if (subFolder.Properties.FieldValues != null && subFolder.Properties.FieldValues.Count > 0)
                            {
                                Hashtable hashtable = new Hashtable();
                                foreach (KeyValuePair<string, object> pair in subFolder.Properties.FieldValues)
                                {
                                    hashtable[pair.Key] = pair.Value;
                                }
                                itemProperty["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = hashtable;
                            }
                            folders.Add(itemProperty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn($"Discover system folder sub folders failed.Error:{ex}");
            }
        }

        #endregion

        public void ShareObject(string webUrl, string url, string peoplePickerInput, string roleValue, int groupId, bool propagateAcl, bool sendEmail, bool includeAnonymousLinkInEmail, string emailSubject, string emailBody, bool useSimplifiedRoles)
        {
            using (AveClientContext context = CreateContext(webUrl))
            {
                SharingResult sharingResult = Web.ShareObject(context, url, peoplePickerInput, roleValue, groupId, propagateAcl, sendEmail, includeAnonymousLinkInEmail, emailSubject, emailBody, useSimplifiedRoles);
                context.Load(sharingResult);
                context.ExecuteQuery();
                if (!string.IsNullOrEmpty(sharingResult.ErrorMessage))
                {
                    mLogger.Warn("Grant permission {0} failed, User Info: {1}, Error message: {2}", roleValue, peoplePickerInput, sharingResult.ErrorMessage);
                }
            }
        }

        public string CreateAnonymousLinkWithExpiration(string webUrl, string fileFullPath, bool isEditLink, long expirationTicks)
        {
            using (AveClientContext context = CreateContext(webUrl))
            {
                string expirationString = null;
                if (expirationTicks != default(long))
                {
                    ClientResult<DateTime> timeResult = context.Web.RegionalSettings.TimeZone.UTCToLocalTime(new DateTime(expirationTicks, DateTimeKind.Utc));
                    context.ExecuteQuery();
                    expirationString = timeResult.Value.ToString("yyyyMMddTHHmmssZ");
                }
                ClientResult<string> result = Web.CreateAnonymousLinkWithExpiration(context, fileFullPath, isEditLink, expirationString);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public string CreateOrganizationSharingLink(string webUrl, string fileFullPath, bool isEditLink)
        {
            using (AveClientContext context = CreateContext(webUrl))
            {
                ClientResult<string> result = Web.CreateOrganizationSharingLink(context, fileFullPath, isEditLink);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public void FolderMoveTo(string webServerRelativeUrl, string folderServerRelativeUrl, string desServerRelativeUrl)
        {
            //List<string> foldersToSkip = new List<string>() { "Lists/PublishedFeed" };     //these folder could not be deleted
            //foreach (string folderToSkip in foldersToSkip)
            //{
            //    if (folderServerRelativeUrl.Contains(folderToSkip))
            //    {
            //        return;
            //    }
            //}

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                folder.MoveTo(desServerRelativeUrl);
                context.ExecuteQuery();
            }
        }

        public void DeclareItemsByRowIds(string webUrl, Guid listId, List<int> rowIds)
        {
            using (AveClientContext context = CreateContext())
            {
                lock (declareLockObj)
                {
                    AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_REQUIRES_WINDOW_VERSION, AveSPErrorCode.TP_E_MD_VERSION_CONFLICT);
                    retryHelper.ExecuteWithRetryMechanism(() =>
                    {
                        Web web = context.Site.OpenWeb(webUrl);
                        List list = web.Lists.GetById(listId);
                        CamlQuery query = new CamlQuery();
                        StringBuilder values = new StringBuilder();
                        foreach (var id in rowIds)
                        {
                            values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                        }
                        query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);
                        var items = list.GetItems(query);
                        context.Load(items);
                        context.ExecuteQuery();
                        try
                        {
                            foreach (var item in items)
                            {
                                Records.DeclareItemAsRecord(context, item);
                            }
                            context.ExecuteQuery();
                        }
                        catch (ServerException e)
                        {
                            if (e.ServerErrorCode == AveSPErrorCode.COR_E_APPLICATION)
                            {
                                mLogger.Error("DeclareItemsByRowIds Failed, webServerRelativeUrl:{0}, listId:{1}, message:{2}.", webUrl, listId, e);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    });
                }
            }
        }

        public void DeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                lock (declareLockObj)
                {
                    //archiver针对同一list的不同item，使用多线程declare操作时，可能会因为同时update list或者添加feature出现serverException的异常:Save Conflict.
                    //Your changes conflict with those made concurrently by another user. If you want your changes to be applied, click Back in your Web browser, 
                    //refresh the page, and resubmit your changes.所以增加此retry操作
                    AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_REQUIRES_WINDOW_VERSION, AveSPErrorCode.TP_E_MD_VERSION_CONFLICT);
                    retryHelper.ExecuteWithRetryMechanism(() =>
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        List list = web.Lists.GetById(listId);
                        ListItem item = list.GetItemById(itemId);
                        try
                        {
                            Records.DeclareItemAsRecord(context, item);
                            context.ExecuteQuery();
                        }
                        /*review-qlluo*/
                        catch (ServerException e)
                        {
                            if (e.ServerErrorCode == AveSPErrorCode.COR_E_APPLICATION)
                            {
                                mLogger.Error("This item has been declared a record, webServerRelativeUrl:{0}, listId:{1}, itemId:{2}, message:{3}", webServerRelativeUrl, listId, itemId, e);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    });
                }
            }
        }

        public void UndeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    mLogger.Info($"real start to UndeclareItemAsRecord,itemid:{itemId}");
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    ListItem item = list.GetItemById(itemId);
                    Records.UndeclareItemAsRecord(context, item);
                    context.ExecuteQuery();
                    mLogger.Info($"finish to UndeclareItemAsRecord,itemid:{itemId}");

                }
                catch (Exception e)
                {
                    mLogger.Warn("undeclare item as record failed. error message:{0}", e.ToString());
                }
            }
        }

        public void AddSupportedUILanguage(string webServerRelativeUrl, List<int> supportedUILanguageIds)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.SupportedUILanguageIds, w => w.Language);
                context.ExecuteQuery();

                if (supportedUILanguageIds == null || supportedUILanguageIds.Count == 0)
                {

                    return;
                }

                if (supportedUILanguageIds.Count == 1)
                {
                    var alternateLanguages = web.SupportedUILanguageIds.ToList();
                    //first is default language
                    alternateLanguages.RemoveAt(0);
                    //source don't enable multi language,destination should disable as well
                    alternateLanguages.ForEach(t => web.RemoveSupportedUILanguage(t));
                    web.IsMultilingual = false;
                    web.Update();
                    context.ExecuteQuery();
                    //return;
                }
                else
                {
                    web.IsMultilingual = true;
                    List<int> needRemovedLanguage = web.SupportedUILanguageIds.Except(supportedUILanguageIds).ToList();
                    foreach (var language in supportedUILanguageIds)
                    {
                        if (!web.SupportedUILanguageIds.Contains(language))
                        {
                            web.AddSupportedUILanguage(language);
                        }
                    }
                    foreach (var language in needRemovedLanguage)
                    {
                        web.RemoveSupportedUILanguage(language);
                    }
                    web.Update();
                    context.ExecuteQuery();
                }


                #region old
                //supportedUILanguageIds.RemoveAt(0);//将源端主语言移出，不添加为目的端的多选语言SAAS-22594

                //List<int> add = supportedUILanguageIds.Except(web.SupportedUILanguageIds).ToList();
                //List<int> del = web.SupportedUILanguageIds.Except(supportedUILanguageIds).ToList();

                ////将目的端主语言移出，不删除目的端的主语言
                //for (int i = 0; i < del.Count; i++)
                //{
                //    if (del[i] == web.Language)
                //    {
                //        del.RemoveAt(i);
                //        break;
                //    }
                //}

                //bool needUpdate = false;
                //if (add.Count > 0)
                //{
                //    web.IsMultilingual = true;
                //    needUpdate = true;
                //}
                //foreach (int lcid in add)
                //    web.AddSupportedUILanguage(lcid);

                //foreach (int lcid in del)
                //{
                //    web.RemoveSupportedUILanguage(lcid);
                //}

                //if (del.Count > 0 && add.Count == 0)
                //{
                //    needUpdate = true;
                //    if (web.SupportedUILanguageIds.Count() - del.Count == 1)
                //        web.IsMultilingual = false;
                //}
                //if (needUpdate)
                //{
                //    web.Update();
                //    context.ExecuteQuery();
                //}
                #endregion old
            }
        }


        public string GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string listName, string ctSource, string resourceName, string contentTypeId, string cultureName)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ContentTypeCollection contentTypes = null;
                switch (ctSource)
                {
                    case "web.availableContentTypes":
                        contentTypes = web.AvailableContentTypes;
                        break;
                    case "web.contentTypes":
                        contentTypes = web.ContentTypes;
                        break;
                    case "list.contentTypes":
                        List list = web.Lists.GetById(listId);
                        contentTypes = list.ContentTypes;
                        break;
                    default:
                        break;
                };
                ObjectPath path = new ObjectPathMethod(context, contentTypes?.Path, "GetById", new object[] { contentTypeId });
                ContentType ct = new ContentType(context, path);
                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.NAME_RESOUCE:
                        result = ct.NameResource.GetValueForUICulture(cultureName);
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        result = ct.DescriptionResource.GetValueForUICulture(cultureName);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }

                return result.Value;
            }
        }

        public string GetFieldUserResource(string webServerRelativeUrl, Guid listId, string listName, string fieldSource, string resourceName, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties, string cultureName)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                Web web = context.Web;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProperties["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields?.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;

                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        result = field.TitleResource.GetValueForUICulture(cultureName);
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        result = field.DescriptionResource.GetValueForUICulture(cultureName);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }

                return result.Value;
            }
        }

        public void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields)
        {
            mRequestCommon.ReorderListFields(webServerRelativeUrl, listId, mappedSourceFields);
        }

        public void PublishNintexWorkflow(string webUrl, string workflowId, string workflowRestrictToScope)
        {
            nintexAPIProcessor.PublishNintexWorkflow(webUrl, workflowId, workflowRestrictToScope);
        }

        public void RemoveSiteCollection(string siteUrl)
        {
            DeleteSiteCore(siteUrl, true);
        }

        public void DeleteSiteCollectionImmediately(string siteUrl)
        {
            DeleteSiteCore(siteUrl, false);
        }

        public void RemoveDeletedSiteCollection(string siteUrl)
        {
            RemoveDeletedSiteCore(siteUrl);
        }

        private void DeleteSiteCore(string url, bool deleteToRecybleBin)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var result = tenant.RemoveSite(url);
                context.ExecuteQuery();
                WaitForSpoOperationComplete(context, result);
            }
            if (!deleteToRecybleBin)
            {
                RemoveDeletedSiteCore(url);
            }
        }

        private void RemoveDeletedSiteCore(string url)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var result = tenant.RemoveDeletedSite(url);
                WaitForSpoOperationComplete(context, result);
            }
        }

        /// <summary>
        /// 默认等1个月，等同于无限等
        /// </summary>
        /// <param name="context"></param>
        /// <param name="operation"></param>
        /// <param name="maxTimes"></param>
        /// <param name="intervalSeconds"></param>
        private void WaitForSpoOperationComplete(AveClientContext context, SpoOperation operation, int maxTimes = 864000, int intervalSeconds = 3)
        {
            int times = 0;
            while (true)
            {
                try
                {
                    context.Load(operation);
                    context.ExecuteQuery();
                    mLogger.Debug("[CheckingSpoOperationCompleteStatus]IsComplete: {0}", operation.IsComplete);
                    if (operation.IsComplete)
                    {
                        break;
                    }
                    Thread.Sleep(intervalSeconds * 1000);
                    times++;

                }
                catch (Exception e)
                {
                    mLogger.Error("Check operation complete status failed.{0}", e);
                    return;
                }
                if (times >= maxTimes)
                {
                    mLogger.Warn("Operation did not complete within {0} seconds, continue doing next step.");
                    break;
                }
            }
        }

        private static AveContentTypeSource GetContentTypeSourceScope(IDictionary<string, object> props)
        {
            object sourceString;
            if (props != null & props.TryGetValue("ContentTypeSource", out sourceString))
            {
                string ctSourceString = sourceString as string;
                switch (ctSourceString)
                {
                    case "web.availableContentTypes":
                        return AveContentTypeSource.WebAvaliableContentTypes;
                    case "web.contentTypes":
                        return AveContentTypeSource.WebContentTypes;
                    case "list.contentTypes":
                        return AveContentTypeSource.ListContentTypes;
                }
            }
            throw new ArgumentException("Invalid contentTypeSource");
        }

        private static string GetContentTypeId(IDictionary<string, object> props)
        {
            object sourceString;
            if (props != null & props.TryGetValue("ContentTypeId", out sourceString))
            {
                return sourceString as string;
            }
            throw new ArgumentException("Invalid ContentTypeId");
        }

        private static Field GetContentTypeField(AveClientContext context, Web web, Guid listId,
            IDictionary<string, object> contentTypeProps,
            Guid fieldId, AveFieldSource fieldSource)
        {
            var ctId = GetContentTypeId(contentTypeProps);
            if (fieldSource == AveFieldSource.ListContentTypeFields)
            {
                var list = web.Lists.GetById(listId);
                var ct = list.ContentTypes.GetById(ctId);
                var field = ct.Fields.GetById(fieldId);
                context.Load(list);
                context.Load(ct);
                context.Load(field);
                context.ExecuteQuery();
                return field;
            }
            else if (fieldSource == AveFieldSource.WebContentTypeFields)
            {
                switch (GetContentTypeSourceScope(contentTypeProps))
                {
                    case AveContentTypeSource.WebContentTypes:
                        var ct = web.ContentTypes.GetById(ctId);
                        return ct.Fields.GetById(fieldId);
                    case AveContentTypeSource.WebAvaliableContentTypes:
                        ct = web.AvailableContentTypes.GetById(ctId);
                        return ct.Fields.GetById(fieldId);
                    default:
                        throw new ArgumentException("invalid ct source.");
                }
            }
            throw new ArgumentException("invalid field source.");
        }

        private void SetShowInForm(string webServerRelativeUrl, AveFieldSource source, Guid listId,
          IDictionary<string, object> contentTypeProps, Guid fieldId, bool value, Action<Field, bool> setMethod)
        {
            using (
                AveClientContext context =
                    CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl.TrimStart('/')))
            {
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
                Field field = null;
                List list;
                ContentType ct;
                var web = context.Web;
                switch (source)
                {
                    case AveFieldSource.WebFields:
                        field = web.Fields.GetById(fieldId);
                        break;
                    case AveFieldSource.WebAvaliableFields:
                        field = web.AvailableFields.GetById(fieldId);
                        break;
                    case AveFieldSource.WebContentTypeFields:


                        field = GetContentTypeField(context, web, listId, contentTypeProps, fieldId, source);
                        break;
                    case AveFieldSource.ListFields:
                        list = web.Lists.GetById(listId);
                        field = list.Fields.GetById(fieldId);
                        break;
                    case AveFieldSource.ListContentTypeFields:
                        field = GetContentTypeField(context, web, listId, contentTypeProps, fieldId, source);
                        break;
                    default:
                        throw new ArgumentException("Invalid field source");
                }
                setMethod(field, value);
                //field.Update();
                //context.Load(field.TypedObject);
                context.ExecuteQuery();
            }
        }


        public void SetShowInDisplayForm(string webServerRelativeUrl, AveFieldSource source, Guid listId,
            IDictionary<string, object> contentTypeProps, Guid fieldId, bool value)
        {
            Action<Field, bool> method = (Field f, bool va) =>
             {
                 f.SetShowInDisplayForm(va);
                 mLogger.Info("Set field SetShowInDisplayForm to {0}", va);
             };
            SetShowInForm(webServerRelativeUrl, source, listId, contentTypeProps, fieldId, value, method);
        }

        public void SetShowInEditForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value)
        {
            Action<Field, bool> method = (Field f, bool va) =>
            {
                f.SetShowInEditForm(va);
                mLogger.Info("Set field SetShowInEditForm to {0}", va);
            };
            SetShowInForm(webServerRelativeUrl, source, listId, contentTypeProps, fieldId, value, method);
        }

        public void SetShowInNewForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value)
        {
            Action<Field, bool> method = (Field f, bool va) =>
            {
                f.SetShowInNewForm(va);
                mLogger.Info("Set field SetShowInNewForm to {0}", va);
            };
            SetShowInForm(webServerRelativeUrl, source, listId, contentTypeProps, fieldId, value, method);
        }



        public void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.SaveNintexForm(formXml, webUrl, listId, contentTypeId);
        }

        public void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.PublishNintexForm(webUrl, listId, contentTypeId);
        }

        public Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            return nintexAPIProcessor.ExportNintexForm(webUrl, listId, contentTypeId);
        }

        public List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl)
        {
            return mRequestCommon.GetListsIdContainItemsWithUniquePermissions(webUrl);
        }

        public List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib)
        {
            return mRequestCommon.GetItemsIdWithUniquePermissions(webServerRelativeUrl, webUrl, listId, isDocLib);
        }

        public void UpdateTenantProperties(Dictionary<string, object> props)
        {
            using (AveClientContext context = CreateContext(mWebUrl))
            {
                var tenant = new Tenant(context);
                AveObjectCopy.UpdateObjectBasicProperties(props, tenant);
                context.ExecuteQuery();
            }
        }

        public bool GetRequestAccessEnable(string webUrl)
        {
            //return mRequestCommon.GetRequestAccessEnable(webUrl);
            bool result = false;
            using (AveClientContext context = CreateContext(webUrl))
            {
                context.Load(context.Web, w => w.UseAccessRequestDefault, w => w.RequestAccessEmail);
                context.ExecuteQuery();
                result = context.Web.UseAccessRequestDefault || !string.IsNullOrWhiteSpace(context.Web.RequestAccessEmail);
            }
            return result;
        }

        public bool SetRequestAccessEnable(string webUrl, bool value)
        {
            //return mRequestCommon.SetRequestAccessEnable(webUrl, value);
            bool result = false;
            using (AveClientContext context = CreateContext(webUrl))
            {
                context.Web.SetUseAccessRequestDefaultAndUpdate(value);
                context.ExecuteQuery();
            }
            return result;
        }

        public bool GetAccessRequestApprover(string webUrl)
        {
            //return mRequestCommon.GetAccessRequestApprover(webUrl);
            bool result = false;
            using (AveClientContext context = CreateContext(webUrl))
            {
                context.Load(context.Web, w => w.UseAccessRequestDefault, w => w.RequestAccessEmail);
                context.ExecuteQuery();
                result = context.Web.UseAccessRequestDefault && string.IsNullOrWhiteSpace(context.Web.RequestAccessEmail);
            }
            return result;
        }

        public void SetAccessRequestApprover(string webUrl, bool value, string email)
        {
            //mRequestCommon.SetAccessRequestApprover(webUrl, value, email);
            using (AveClientContext context = CreateContext(webUrl))
            {
                context.Web.SetUseAccessRequestDefaultAndUpdate(value);
                context.Web.RequestAccessEmail = string.IsNullOrWhiteSpace(email) ? string.Empty : email;
                context.ExecuteQuery();
            }
        }

        public void SetComplianceTagOnBulkItems(string webUrl,Guid webID, Guid listID, List<int> itemIds, string complianceTagValue)
         {
            using var context = CreateContext(webUrl);
            var web = context.Site.OpenWebById(webID);
            var list = web.Lists.GetById(listID);
            context.Load(list);
            context.Load(list, l => l.RootFolder.ServerRelativeUrl);
            context.ExecuteQuery();
            SPPolicyStoreProxy.SetComplianceTagOnBulkItems(context, itemIds, list.RootFolder.ServerRelativeUrl, complianceTagValue);
            context.ExecuteQuery();
        }

        public void SetComplianceTagOnBulkItems(ClientContext context, string listUrl, List<int> itemIds, string complianceTagValue)
        {
            SPPolicyStoreProxy.SetComplianceTagOnBulkItems(context, itemIds, listUrl, complianceTagValue);
            context.ExecuteQuery();
        }

        public void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webID);
                var list = web.Lists.GetById(listID);
                var item = list.GetItemById(rowID);
                item.SetComplianceTag(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock, false);

                //bool blockDel = (complianceSettingInfo.ComplianceSettingFlag & 1) != 0;
                //bool blockEdit = (complianceSettingInfo.ComplianceSettingFlag & 4) != 0;
                //bool changed = (complianceSettingInfo.ComplianceSettingFlag & 2) != 0;
                //item.SetComplianceTagWithMetaInfo(complianceSettingInfo.ComplianceTag, blockDel, blockEdit, complianceSettingInfo.ComplianceWrittenDate, complianceSettingInfo.ComplianceUserLoginName, false);
                //item.SetComplianceTagWithExplicitMetasUpdate(complianceTag, complianceSettingFlags, complianceWrittenDate, string.Empty);
                context.Load(item);
                context.Load(item, i => i.ComplianceInfo);
                context.ExecuteQuery();
                //return AssembleComplianceTagInfo(item);
            }
        }

        public void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock, bool unlockedAsDefault)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webID);
                var list = web.Lists.GetById(listID);
                var item = list.GetItemById(rowID);
                item.SetComplianceTag(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock, unlockedAsDefault);
                //bool blockDel = (complianceSettingInfo.ComplianceSettingFlag & 1) != 0;
                //bool blockEdit = (complianceSettingInfo.ComplianceSettingFlag & 4) != 0;
                //bool changed = (complianceSettingInfo.ComplianceSettingFlag & 2) != 0;
                //item.SetComplianceTagWithMetaInfo(complianceSettingInfo.ComplianceTag, blockDel, blockEdit, complianceSettingInfo.ComplianceWrittenDate, complianceSettingInfo.ComplianceUserLoginName, false);
                //item.SetComplianceTagWithExplicitMetasUpdate(complianceTag, complianceSettingFlags, complianceWrittenDate, string.Empty);
                context.Load(item);
                context.Load(item, i => i.ComplianceInfo);
                context.ExecuteQuery();
                //return AssembleComplianceTagInfo(item);
            }
        }

        public void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool blockDel, bool blockEdit, DateTime complianceWrittenDate, string userEmail, bool isTagSuperLock)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webID);
                var list = web.Lists.GetById(listID);
                var item = list.GetItemById(rowID);
                item.SetComplianceTagWithMetaInfo(complianceTag, blockDel, blockEdit, complianceWrittenDate, userEmail, isTagSuperLock, false);
                context.Load(item);
                context.Load(item, i => i.ComplianceInfo);
                context.ExecuteQuery();
            }
        }

        #region compliance setting (Apply label to items in this list or library)
        public List<AveComplianceTagInfo> GetAvailableTagsForSite(string siteUrl)
        {
            using (ClientContext context = CreateContext(siteUrl))
            {
                List<AveComplianceTagInfo> AvailableTags = new List<AveComplianceTagInfo>();
                var availableComplianceTags = SPPolicyStoreProxy.GetAvailableTagsForSite(context, siteUrl);
                context.ExecuteQuery();
                foreach(var complianceTag in availableComplianceTags)
                {
                    var info = new AveComplianceTagInfo();
                    info.AcceptMessagesOnlyFromSendersOrMembers = complianceTag.AcceptMessagesOnlyFromSendersOrMembers;
                    info.AccessType = complianceTag.AccessType;
                    info.AllowAccessFromUnmanagedDevice = complianceTag.AllowAccessFromUnmanagedDevice;
                    info.AutoDelete = complianceTag.AutoDelete;
                    info.BlockDelete = complianceTag.BlockDelete;
                    info.BlockEdit = complianceTag.BlockEdit;
                    info.ContainsSiteLabel = complianceTag.ContainsSiteLabel;
                    info.DisplayName = complianceTag.DisplayName;
                    info.EncryptionRMSTemplateId = complianceTag.EncryptionRMSTemplateId;
                    info.HasRetentionAction = complianceTag.HasRetentionAction;
                    info.IsEventTag = complianceTag.IsEventTag;
                    info.Notes = complianceTag.Notes;
                    info.RequireSenderAuthenticationEnabled = complianceTag.RequireSenderAuthenticationEnabled;
                    info.ReviewerEmail = complianceTag.ReviewerEmail;
                    info.SharingCapabilities = complianceTag.SharingCapabilities;
                    info.SuperLock = complianceTag.SuperLock;
                    info.TagDuration = complianceTag.TagDuration;
                    info.TagId = complianceTag.TagId;
                    info.TagName = complianceTag.TagName;
                    info.TagRetentionBasedOn = complianceTag.TagRetentionBasedOn;
                    info.UnlockedAsDefault = complianceTag.UnlockedAsDefault;
                    AvailableTags.Add(info);
                }
                return AvailableTags;
            }
        }

        public AveComplianceTagInfo GetListComplianceTag(string webUrl, string listUrl)
        {
            using (ClientContext context = CreateContext(webUrl))
            {
                AveComplianceTagInfo info = null;
                var complianceTag = SPPolicyStoreProxy.GetListComplianceTag(context, listUrl);
                context.ExecuteQuery();
                if (complianceTag.Value != null)
                {
                    info = new AveComplianceTagInfo();
                    info.AcceptMessagesOnlyFromSendersOrMembers = complianceTag.Value.AcceptMessagesOnlyFromSendersOrMembers;
                    info.AccessType = complianceTag.Value.AccessType;
                    info.AllowAccessFromUnmanagedDevice = complianceTag.Value.AllowAccessFromUnmanagedDevice;
                    info.AutoDelete = complianceTag.Value.AutoDelete;
                    info.BlockDelete = complianceTag.Value.BlockDelete;
                    info.BlockEdit = complianceTag.Value.BlockEdit;
                    info.ContainsSiteLabel = complianceTag.Value.ContainsSiteLabel;
                    info.DisplayName = complianceTag.Value.DisplayName;
                    info.EncryptionRMSTemplateId = complianceTag.Value.EncryptionRMSTemplateId;
                    info.HasRetentionAction = complianceTag.Value.HasRetentionAction;
                    info.IsEventTag = complianceTag.Value.IsEventTag;
                    info.Notes = complianceTag.Value.Notes;
                    info.RequireSenderAuthenticationEnabled = complianceTag.Value.RequireSenderAuthenticationEnabled;
                    info.ReviewerEmail = complianceTag.Value.ReviewerEmail;
                    info.SharingCapabilities = complianceTag.Value.SharingCapabilities;
                    info.SuperLock = complianceTag.Value.SuperLock;
                    info.TagDuration = complianceTag.Value.TagDuration;
                    info.TagId = complianceTag.Value.TagId;
                    info.TagName = complianceTag.Value.TagName;
                    info.TagRetentionBasedOn = complianceTag.Value.TagRetentionBasedOn;
                }
                return info;
            }
        }

        public void SetListComplianceTag(string webUrl, string listUrl, AveComplianceTagInfo info)
        {
            using (ClientContext context = CreateContext(webUrl))
            {
                //if (!string.IsNullOrEmpty(info.TagName)) Support Apply Label to None
                {
                    SPPolicyStoreProxy.SetListComplianceTag(context, listUrl, info.TagName, info.BlockDelete, info.BlockEdit, false);
                    context.ExecuteQuery();
                }

            }
        }

        #endregion 

       /* private string GetO365DomainName(string siteUrl)
        {
            try
            {
                var uri = new Uri(siteUrl);
                var hostName = uri.Host;
                hostName = hostName.Replace("-my.sharepoint.", ".sharepoint.");
                var domainName = hostName.Substring(0, hostName.IndexOf("."));
                if (hostName.EndsWith(".com"))
                {
                    return $"{domainName}.onmicrosoft.com";
                }
                else if (hostName.EndsWith(".us"))
                {
                    return $"{domainName}.onmicrosoft.us";
                }
                else if (hostName.EndsWith(".cn"))
                {
                    return $"{domainName}.partner.onmschina.cn";
                }
                else
                {
                    throw new Exception($"Invalid site Url {siteUrl}");
                }
            }
            catch
            {
                throw new Exception($"Invalid site Url {siteUrl}");
            }
        }*/
        public void AddFileByRestApi(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            string webFullUrl = this.WebAppName + parentWebUrl;
            FileRestProcessor.AddFileByRestApi(mFormDigestContext, tokenProvider, webFullUrl, Guid.Empty, fileServerRelativeUrl, body, isOverwrite);
        }
        
        public void AddFileByRestApiWithContext(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            using (AveClientContext context = CreateContext())
            {
                string webFullUrl = this.WebAppName + parentWebUrl;
                FileRestProcessor.AddFileByRestApi(context, tokenProvider, webFullUrl, Guid.Empty, fileServerRelativeUrl, body, isOverwrite);
            }
        }

        public void AddFile(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            using AveClientContext context = CreateContext();
            var web = context.Site.OpenWeb(parentWebUrl);
            var parentFolder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
            DocumentContentProcessor.AddDocument(
                context,
                tokenProvider,
                parentWebUrl,
                parentFolder,
                fileServerRelativeUrl,
                body,
                isOverwrite);
            var file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
            context.Load(file, f => f.Level);
            ConditionalScope conditionalScope = new ConditionalScope(context, () => file.Level == FileLevel.Checkout, true);
            using (conditionalScope.StartScope())
            {
                using (conditionalScope.StartIfTrue())
                {
                    file.CheckIn("", CheckinType.MajorCheckIn);
                }
            }
            context.ExecuteQueryRetry();
        }
        #region MetaDefaults
        //need debug for records.
        public string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string fieldName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.RootFolder);
                context.ExecuteQuery();
                Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");
                context.Load(formsFolder, f => f.Files);
                context.ExecuteQuery();

                var clientLocationBasedDefaultsFile =
                    formsFolder.Files.FirstOrDefault(
                        f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

                if (clientLocationBasedDefaultsFile != null)
                {
                    string defaultValues = ReadFileContent(clientLocationBasedDefaultsFile);
                    var defaultsXmlDoc = new XmlDocument();
                    try
                    {
                        defaultsXmlDoc.LoadXml(defaultValues);
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("xml have special character {0}", e.ToString());
                        var fci = new FileCreationInformation();
                        if (defaultValues.Contains("&"))
                        {
                            var replaceXml = defaultValues.Replace("&", "%26");
                            fci.Content = Encoding.UTF8.GetBytes(replaceXml);//Encoding.UTF8.GetBytes(defaultsXmlDoc.OuterXml);
                            fci.Url = "client_LocationBasedDefaults.html";
                            fci.Overwrite = true;
                            var metaDataFile = formsFolder.Files.Add(fci);

                            context.Load(metaDataFile);
                            context.ExecuteQuery();
                            defaultsXmlDoc.LoadXml(replaceXml);
                            mLogger.Info("Replace & to modify the xml");
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    XmlNode xmlNode = SelectSingleFieldDefaultNode(defaultsXmlDoc, folderUrl, fieldName);
                    string existFolderDefaultValue = string.Empty;
                    if (xmlNode != null)
                    {
                        existFolderDefaultValue = xmlNode.InnerText;
                    }
                    else
                    {
                        existFolderDefaultValue = "";
                    }
                    return existFolderDefaultValue;
                }

            }
            return string.Empty;
        }

        //need debug records....
        public bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderPath, string fieldName)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    context.Load(list, l => l.RootFolder);
                    context.ExecuteQuery();
                    Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");
                    var defaultValues = string.Empty;
                    context.Load(formsFolder, f => f.Files);
                    context.ExecuteQuery();

                    var clientLocationBasedDefaultsFile =
                        formsFolder.Files.FirstOrDefault(
                            f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

                    if (clientLocationBasedDefaultsFile != null)
                    {
                        defaultValues = ReadFileContent(clientLocationBasedDefaultsFile);
                    }
                    if (!string.IsNullOrEmpty(defaultValues))
                    {
                        defaultValues = defaultValues.EncodeAmpersandInHref();
                        mLogger.Warn("'/forms/client_LocationBasedDefaults.html' is not exist.");
                        var defaultsXmlDoc = new XmlDocument();
                        defaultsXmlDoc.LoadXml(defaultValues);
                        defaultsXmlDoc = RemoveFieldDefault(defaultsXmlDoc, folderPath, fieldName);
                        var fci = new FileCreationInformation();
                        fci.Content = Encoding.UTF8.GetBytes(defaultsXmlDoc.OuterXml);
                        fci.Url = "client_LocationBasedDefaults.html";
                        fci.Overwrite = true;
                        var metaDataFile = formsFolder.Files.Add(fci);

                        context.Load(metaDataFile);
                        context.ExecuteQuery();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn("Remove folder default value failed {0}", e.ToString());
                return false;
            }
        }

        public bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderPath, string fieldName, string value)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    MetadataDefaults metadataDefaults = new MetadataDefaults(context, list);
                    Folder folder = web.GetFolderByServerRelativeUrl(folderPath);
                    context.Load(folder);
                    metadataDefaults.SetFieldDefault(folder, fieldName, value);
                    metadataDefaults.Update();
                    context.ExecuteQuery();
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn("Set folder default value failed {0}", e.ToString());
                return false;
            }
        }

        public bool SetTaxonomyFieldValue(string webServerRelativeUrl, Guid listId, int itemId, string fieldName, string termId, string termName)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    var item = list.GetItemById(itemId);
                    var field = list.Fields.GetByInternalNameOrTitle(fieldName);
                    var listTaxField = context.CastTo<TaxonomyField>(field);
                    TaxonomyFieldValue taxValue = new TaxonomyFieldValue();
                    taxValue.TermGuid = termId;
                    taxValue.Label = termName;
                    listTaxField.SetFieldValueByValue(item, taxValue);

                    item.Update();
                    context.ExecuteQuery();
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn($"Set taxonomy field value failed {e}");
                return false;
            }
        }

        #endregion
        #region private xml method for handle folder default value change or remove (For Opus.)
        private XmlNode SelectSingleFieldDefaultNode(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']/DefaultValue[@FieldName='{1}']", new object[]
            {
        Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false),
        fieldName
            }));
        }
        private XmlDocument RemoveFieldDefault(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            XmlNode xmlNode = this.SelectSingleFolderNode(defaultsXml, folderPath);
            if (xmlNode == null)
            {
                return defaultsXml;
            }
            XmlNode xmlNode2 = xmlNode.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "./DefaultValue[@FieldName='{0}']", new object[]
            {
        fieldName
            }));
            if (xmlNode2 == null)
            {
                return defaultsXml;
            }
            xmlNode.RemoveChild(xmlNode2);
            if (!xmlNode.HasChildNodes)
            {
                defaultsXml.DocumentElement.RemoveChild(xmlNode);
            }
            return defaultsXml;
        }
        private string ReadFileContent(ClientFile file)
        {
            ClientResult<System.IO.Stream> stream = file.OpenBinaryStream();
            file.Context.ExecuteQuery();

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream.Value, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        private XmlNode SelectSingleFolderNode(XmlDocument defaultsXml, string folderPath)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']", new object[]
            {
        Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false)
            }));
        }
        #endregion

        public void RestoreSharingLink(AveSharingLinkInfo shareLinkInfo, IEnumerable<IAvePrincipal> principals, string parentWebServerRelativeUrl, Guid listId, int itemId)
        {
            try
            {
                string url = string.Format("{0}/_api/web/Lists(@a1)/GetItemById(@a2)/ShareLink?@a1='{1}'&@a2='{2}'", this.WebAppName + parentWebServerRelativeUrl, System.Web.HttpUtility.UrlEncode(listId.ToString("B").ToUpper()), itemId);
                string body = GetSharingLinkPostBody(shareLinkInfo, principals);
                mLogger.Info($"Share link, {url},{body}");
                ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(url, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
                webRequest.RefreshDigestInfo(url, TokenProvider);
                webRequest.SetTokenProvider(this.WebAppName + parentWebServerRelativeUrl, TokenProvider);
                var buffer = Encoding.UTF8.GetBytes(body);
                webRequest.ContentLength = buffer.Length;
                webRequest.Method = "POST";
                webRequest.Accept = "application/json;odata=verbose";
                webRequest.ContentType = "application/json;odata=verbose";

                Stream inputBody = webRequest.GetRequestStream();
                inputBody.Write(buffer, 0, buffer.Length);
                using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (result != null)
                    {
                        if (result.StatusCode != HttpStatusCode.OK)
                        {
                            mLogger.Error($"ShareLink Faild. Url:{url}, Body{body}, {result.StatusCode}");
                            throw new WebException($"ShareLink Faild. Url:{url}, Body{body}, {result.StatusCode}");
                        }
                    }
                    else
                    {
                        mLogger.Error($"ShareLink Faild. Url:{url}, Body{body}");
                        throw new WebException($"ShareLink Faild. Url:{url}, Body{body}");
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"An error occured while restoring sharing link.parentWebUrl:{parentWebServerRelativeUrl}, listID:{listId}, itemID:{itemId}, ex:{ex}.");
            }
        }

        public void LockRecordItem(string parentWebServerRelativeUrl, string listUrl, string itemId)
        {
            try
            {
                string url = string.Format("{0}/_api/SP.CompliancePolicy.SPPolicyStoreProxy.LockRecordItem()", this.WebAppName + parentWebServerRelativeUrl);
                string body = GetLockRecordItemPostBody(itemId,listUrl);
                mLogger.Info($"lock record item, {url},{body}");
                ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(url, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
                webRequest.RefreshDigestInfo(url, TokenProvider);
                webRequest.SetTokenProvider(this.WebAppName + parentWebServerRelativeUrl, TokenProvider);
                var buffer = Encoding.UTF8.GetBytes(body);
                webRequest.ContentLength = buffer.Length;
                webRequest.Method = "POST";
                webRequest.Accept = "application/json;odata=verbose";
                webRequest.ContentType = "application/json;odata=verbose";

                Stream inputBody = webRequest.GetRequestStream();
                inputBody.Write(buffer, 0, buffer.Length);
                using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (result != null)
                    {
                        if (result.StatusCode != HttpStatusCode.OK)
                        {
                            mLogger.Error($"lock record item. Url:{url}, Body{body}, {result.StatusCode}");
                            throw new WebException($"lock record item. Url:{url}, Body{body}, {result.StatusCode}");
                        }
                    }
                    else
                    {
                        mLogger.Error($"lock record item. Url:{url}, Body{body}");
                        throw new WebException($"lock record item. Url:{url}, Body{body}");
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"An error occured while lock record item.parentWebUrl:{parentWebServerRelativeUrl}, listUrl:{listUrl}, itemID:{itemId}, ex:{ex}.");
            }
        }

        public void UnlockRecordItem(string parentWebServerRelativeUrl, string listUrl, string itemId)
        {
            try
            {
                string url = string.Format("{0}/_api/SP.CompliancePolicy.SPPolicyStoreProxy.UnlockRecordItem()", this.WebAppName + parentWebServerRelativeUrl);
                string body = GetLockRecordItemPostBody(itemId, listUrl);
                mLogger.Info($"unlock record item, {url},{body}");
                ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(url, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
                webRequest.RefreshDigestInfo(url, TokenProvider);
                webRequest.SetTokenProvider(this.WebAppName + parentWebServerRelativeUrl, TokenProvider);
                var buffer = Encoding.UTF8.GetBytes(body);
                webRequest.ContentLength = buffer.Length;
                webRequest.Method = "POST";
                webRequest.Accept = "application/json;odata=verbose";
                webRequest.ContentType = "application/json;odata=verbose";

                Stream inputBody = webRequest.GetRequestStream();
                inputBody.Write(buffer, 0, buffer.Length);
                using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (result != null)
                    {
                        if (result.StatusCode != HttpStatusCode.OK)
                        {
                            mLogger.Error($"unlock record item. Url:{url}, Body{body}, {result.StatusCode}");
                            throw new WebException($"unlock record item. Url:{url}, Body{body}, {result.StatusCode}");
                        }
                    }
                    else
                    {
                        mLogger.Error($"unlock record item. Url:{url}, Body{body}");
                        throw new WebException($"unlock record item. Url:{url}, Body{body}");
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"An error occured while unlock record item.parentWebUrl:{parentWebServerRelativeUrl}, listUrl:{listUrl}, itemID:{itemId}, ex:{ex}.");
            }
        }

        

        public ListItemComplianceInfo GetListItemComplianceInfo(Guid webID, Guid listID, int rowID)
        {
            using (AveClientContext context = CreateContext())
            {
                var web = context.Site.OpenWebById(webID);
                var list = web.Lists.GetById(listID);
                var item = list.GetItemById(rowID);
                context.Load(item);
                context.Load(item, i => i.ComplianceInfo);
                context.ExecuteQuery();
                return item.ComplianceInfo;
            }
        }

        public ListItemComplianceInfo GetListItemComplianceInfo(ClientContext context, ListItem item)
        {
            context.Load(item, i => i.ComplianceInfo);
            context.ExecuteQuery();
            return item.ComplianceInfo;
        }

        protected string GetSharingLinkPostBody(AveSharingLinkInfo shareLinkInfo, IEnumerable<IAvePrincipal> principals)
        {
            string body = "";
            JObject bodyObject = new JObject();
            JObject request = new JObject();
            request["createLink"] = new JValue(true);
            JObject settings = new JObject();
            settings["linkKind"] = new JValue(shareLinkInfo.LinkKind);
            settings["allowAnonymousAccess"] = new JValue(shareLinkInfo.AllowsAnonymousAccess);
            settings["expiration"] = DateTime.TryParse(shareLinkInfo.Expiration, out DateTime expiration) ?
                expiration.ToString("yyyyMMdd'T'HHmmss") + "+0000" : null;
            settings["scope"] = new JValue(shareLinkInfo.Scope);
            if (shareLinkInfo.LinkKind == 6)
            {
                int role = 0;
                if (shareLinkInfo.BlocksDownload)
                {
                    role = 7;
                }
                else if (shareLinkInfo.IsReviewLink)
                {
                    role = 6;
                }
                else
                {
                    role = shareLinkInfo.IsEditLink ? 2 : 1;
                }
                settings["role"] = new JValue(role);
            }

            settings["restrictShareMembership"] = new JValue(shareLinkInfo.RestrictedShareMembership);
            settings["updatePassword"] = new JValue(false);
            settings["password"] = "";
            if (shareLinkInfo.AllowsAnonymousAccess)
            {
                settings["trackLinkUsers"] = new JValue(false);
            }

            JObject emailData = new JObject();
            emailData["body"] = "";
            emailData["subject"] = "";
            string principalStr = "";
            foreach (var p in principals)
            {
                principalStr += "{\"Key\":\"" + p.LoginName + "\",\"DisplayText\":\"\",\"IsResolved\":true,\"Description\":\"\",\"EntityType\":\"" + (p.PrincipalType == AvePrincipalType.User ? "User" : "SecGroup") + "\",\"EntityData\":{\"IsAltSecIdPresent\":\"False\",\"Title\":\"\",\"Email\":\"\",\"MobilePhone\":\"\",\"ObjectId\":\"\",\"Department\":\"\"},\"MultipleMatches\":[],\"ProviderName\":\"Tenant\",\"ProviderDisplayName\":\"Tenant\"},";
            }
            request["settings"] = settings;
            // request["emailData"] = emailData;
            if (principals.Count() > 0)
            {
                request["peoplePickerInput"] = string.Format("[{0}]", principalStr.TrimEnd(','));
            }
            bodyObject["request"] = request;
            body = bodyObject.ToString();
            return body;
        }

        protected string GetLockRecordItemPostBody(string itemId, string listUrl)
        {
            string body = "";
            JObject bodyObject = new JObject();
            bodyObject["itemId"] = itemId;
            bodyObject["listUrl"] = listUrl;
            body = bodyObject.ToString();
            return body;
        }

        public AveDictionary<Guid, AveSharingLinkInfo> GetListItemSharingLinks(string parentWebUrl, Guid listId, int itemId)
        {
            AveDictionary<Guid, AveSharingLinkInfo> SharingLinks = new AveDictionary<Guid, AveSharingLinkInfo>();
            try
            {
                string RESTURL = "{0}/_api/web/Lists(@a1)/GetItemById(@a2)/GetSharingInformation?@a1='{1}'&@a2='{2}'&$Expand=permissionsInformation";
                string url = string.Format(RESTURL, parentWebUrl, listId, itemId);

                ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(url, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
                webRequest.RefreshDigestInfo(url, TokenProvider);
                webRequest.SetTokenProvider(parentWebUrl, TokenProvider);

                webRequest.Method = "POST";
                webRequest.Accept = "application/json;odata=verbose";
                webRequest.ContentType = "application/json;odata=verbose";
                webRequest.ContentLength = 0;

                WebResponse webResponse = webRequest.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();
                    JObject jobj = JObject.Parse(response);
                    JArray jarr = (JArray)jobj["d"]["permissionsInformation"]["links"]["results"];
                    foreach (JObject j in jarr)
                    {
                        try
                        {
                            var link = j["linkDetails"];
                            AveSharingLinkInfo linkinfo = new AveSharingLinkInfo();
                            linkinfo.ShareId = new Guid(link["ShareId"].ToString());
                            linkinfo.LinkKind = Convert.ToInt32(link["LinkKind"]);
                            linkinfo.Expiration = link["Expiration"].ToString();
                            linkinfo.AllowsAnonymousAccess = Convert.ToBoolean(link["AllowsAnonymousAccess"]);
                            linkinfo.RestrictedShareMembership = Convert.ToBoolean(link["RestrictedShareMembership"]);
                            linkinfo.BlocksDownload = Convert.ToBoolean(link["BlocksDownload"]);
                            linkinfo.IsEditLink = Convert.ToBoolean(link["IsEditLink"]);
                            linkinfo.IsReviewLink = Convert.ToBoolean(link["IsReviewLink"]);
                            linkinfo.Scope = Convert.ToInt32(link["Scope"]);
                            linkinfo.RequiresPassword = Convert.ToBoolean(link["RequiresPassword"]);
                            mLogger.Info($"Success to get one sharing link info, ShareId:{linkinfo.ShareId}, linkKind:{linkinfo.LinkKind}");
                            SharingLinks.Add(linkinfo.ShareId, linkinfo);
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("An error occured while get one sharing link. parentWebUrl:{0}, listID:{1}, itemID:{2}, ex:{3}, response:{4}.", parentWebUrl, listId, itemId, e.ToString(), response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occured while getting sharing link.parentWebUrl:{0}, listID:{1}, itemID:{2}, ex:{3}.", parentWebUrl, listId, itemId, ex.ToString());
            }
            mLogger.Info($"Retrieve {SharingLinks.Count} sharing links for the web: {parentWebUrl}, list: {listId}, item: {itemId}");
            return SharingLinks;
        }

        public async Task<LabelOperationResponse> RemoveSensitiveLabelAsync(FileInfo srcFile, FileInfo dstFile)
        {
            LabelOperationContext context = new LabelOperationContext();
            context.OutputFileInfo = dstFile;
            context.InputFileInfo = srcFile;
            //需要设置此参数，测试时发现不设置此参数会导致文件去除保护，但是label 没有被remove
            context.OnlyHandleCustomProtection = true;
            context.OperationType = LabelOperationType.RemoveLabel;
            var response = await mMIPService.OperateLabelAsync(context);
            return response;
        }

        public Dictionary<string, (Guid UniqueId, Guid ListId)> GetStubNodesByBatchPath(List<string> serverRelativeUrls)
        {
            var result = new Dictionary<string, (Guid UniqueId, Guid ListId)>(StringComparer.OrdinalIgnoreCase);
            if (serverRelativeUrls == null || serverRelativeUrls.Count == 0) return result;

            var scopeList = new List<Tuple<string, ExceptionHandlingScope, Microsoft.SharePoint.Client.File>>();

            using (AveClientContext ctx = CreateContext())
            {
                foreach (var url in serverRelativeUrls)
                {
                    var scope = new ExceptionHandlingScope(ctx);
                    var file = ctx.Web.GetFileByUrl(url);

                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            ctx.Load(file
                                        ,f => f.Exists
                                        ,f => f.UniqueId
                                        ,f => f.ListId
                                        //,f => f.WebId
                                     );
                        }
                        using (scope.StartCatch())
                        {
                            // Catch 404
                        }
                    }
                    scopeList.Add(Tuple.Create(url, scope, file));
                }

                try
                {
                    ctx.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Batch ExecuteQuery failed: {ex.Message}");
                }
            }
            
            foreach (var item in scopeList)
            {
                try
                {
                    if (item.Item2.HasException)
                    {
                        mLogger.Error($"BatchCheckFilesExist: Error when checking file {item.Item1}, Error Code: {item.Item2.ServerErrorCode}, Exception: {item.Item2.ErrorMessage}");
                        continue;
                    }

                    if (item.Item3.Exists)
                    {
                        if (!result.ContainsKey(item.Item1))
                        {
                            result[item.Item1] = (item.Item3.UniqueId, item.Item3.ListId);
                        }
                    }
                }
                catch (Exception e)
                {
                    // if throw error when accessing Exist, consider file not exist
                    mLogger.Error("BatchCheckFilesExist: Error when processing file {0}. Exception: {1}", item.Item1, e);
                }
            }

            return result;
        }

        public Dictionary<string, object> GetItemsByUniqueIds(string webServerRelativeUrl, string listName, Guid listId, Guid[] uniqueIds)
        {
            using (var context = CreateDiscoverContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                var itemList = new List<IDictionary<string, object>>();

                if (uniqueIds == null || uniqueIds.Length == 0)
                {
                    itemsProperties["PageInfo"] = null;
                    itemsProperties.AddChildren(itemList);
                    return itemsProperties;
                }

                var batchDefinitions = new List<Tuple<Guid, ExceptionHandlingScope, ListItem>>();

                foreach (var id in uniqueIds)
                {
                    var scope = new ExceptionHandlingScope(context);

                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            var item = list.GetItemByUniqueId(id);

                            context.Load(item);

                            batchDefinitions.Add(new Tuple<Guid, ExceptionHandlingScope, ListItem>(id, scope, item));
                        }
                        using (scope.StartCatch())
                        {
                            // Catch 404
                        }
                    }
                }

                try
                {
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Batch load items failed. E: {0}", ex.Message);
                }

                foreach (var def in batchDefinitions)
                {
                    var scope = def.Item2;
                    var item = def.Item3;

                    if (!scope.HasException && !item.ServerObjectIsNull.GetValueOrDefault())
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();

                        GetItemDic(itemProperties, item);

                        itemList.Add(itemProperties);
                    }
                    else if (scope.HasException)
                    {
                        mLogger.Warn($"Failed to load item {def.Item1} in batch. Error: {scope.ErrorMessage}");
                    }
                }

                itemsProperties["PageInfo"] = null;
                itemsProperties.AddChildren(itemList);

                return itemsProperties;
            }
        }
    }
}



