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
using AvePoint.GA.WebAPI;
using AvePoint.GA.WebAPI.Models;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using Cloud.Sdk.Data.Aos;
using Cloud.Sdk.Data.Aos.Tenant;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using RemoteNodeType = Cloud.Sdk.Data.Aos.Tenant.RemoteNodeType;
using AvePoint.RA.Web.Extentions.Util;
using System.Threading.Tasks;

namespace AvePoint.RA.Web
{
    [ServiceContract(Namespace = "http://www.avepoint.com/")]
    public interface IRecordsService
    {
        [OperationContract]
        int ApplyRecordsSettings(string requestId, string token);
        [OperationContract]
        Task<int> AddSiteCollectionOrTeamSiteCustomSettingAsync(string requestId, string token, string sitecollectionURL, string RootTermPath, string DefaultTermPath, string applyToExistDocuments = "false", string overWriteExist = "false", string ApplySettingNow = "false");
        [OperationContract]
        Task<int> AddSiteCollectionCustomSettingAsync(string requestId, string token, string sitecollectionURL, string RootTermPath, string DefaultTermPath);

    }


    /// <summary>
    /// Summary description for RecordsService
    /// </summary>
    //[WebService(Namespace = "http://www.avepoint.com/")]
    //[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    //[System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class RecordsService : IRecordsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RecordsService));

        private static readonly IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static readonly ISyncRemoteNodeRedisService RemoteNodeCacheService = PlatformWindsorManager.GetService<ISyncRemoteNodeRedisService>();

        private static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private IDocAveSharePointSiteService mSharePointSiteService { get; set; }
        public IDocAveSharePointSiteService SharePointSiteService
        {
            get
            {
                if (mSharePointSiteService == null)
                {
                    mSharePointSiteService = PlatformWindsorManager.GetService(typeof(IDocAveSharePointSiteService)) as IDocAveSharePointSiteService;
                    return mSharePointSiteService;
                }
                else
                {
                    return mSharePointSiteService;
                }
            }
        }
        // public IDocAveSharePointSiteService SharePointSiteService { set; get; }
        //public string HelloWorld()
        //{
        //    return "Hello World";
        //}

        public int ApplyRecordsSettings(string requestId, string token)
        {
            try
            {
                logger.Info("Request Id {0}", requestId);
                if (!ValidateToken())
                {
                    throw new Exception("UnKnown or Invalid Token");
                }
                SharePointSiteService.ApplyAllSharePointSettingJob();
                return 0;
            }
            catch (Exception e)
            {
                logger.Error("Apply Records Setting Through Web Api failed {0}", e.ToString());
            }
            return 1;
        }

        public string GetSiteCollectionURL(string requestID)
        {
            logger.Info("Get URL Info from GA Request {0}", requestID);
            InitFromRequest();
            var requestService = GaoApi.Create<IRequestService>();
            var request = requestService.Get(new Guid(requestID));
            logger.Info($"{request.GetType().ToString()}");
            APIRequestProvSite siteCollectionRequest = request as APIRequestProvSite;
            if (siteCollectionRequest != null)
            {
                logger.Info("Get URL Info from GA Request URL {0}", siteCollectionRequest.Url.ToString());
                return siteCollectionRequest.Url.ToString();
            }
            else
            {
                logger.Info("get group object from GA requst ");
                APIRequestCreateGroup groupSiteUrlRequest = request as APIRequestCreateGroup;
                if (groupSiteUrlRequest != null)
                {
                    logger.Info($"group request email object {groupSiteUrlRequest.GroupEmail} : {groupSiteUrlRequest.GroupName} :{groupSiteUrlRequest.GroupId}");
                    return groupSiteUrlRequest.GroupEmail;
                }
                else
                {
                    logger.Info("group object is null");
                }
            }
            return null;
        }
        private static void InitFromRequest()
        {
            string text = HttpContextExtensions.CurrentHttpContext().Request.Headers.GetHeaderValue("X_GovernanceAutomation_Access_Token");
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("X_GovernanceAutomation_Access_Token is not found in request header.");
            }
            Region region = Region.EastUS;
            string text2 = HttpContextExtensions.CurrentHttpContext().Request.Headers.GetHeaderValue("X_GovernanceAutomation_Region");
            int num;
            if (!string.IsNullOrEmpty(text2) && int.TryParse(text2, out num))
            {
                region = (Region)num;
            }
            GaoApi.Init(region, text, false);
        }
        public bool ValidateToken()
        {
            try
            {
                InitFromRequest();
                var logonService = GaoApi.Create<ISecurityService>();
                CurrentUserModel account = logonService.GetTokenAccount();
                AosAuthentication aosAuthentication = new AosAuthentication();
                var credential = new AOSCredential()
                {
                    UserId = account.UserId,
                    UserName = account.Upn,
                    TenantGroupId = account.Tenant,
                };
                RMIdentity identity = aosAuthentication.AuthenticateCredential(credential);
                if (string.IsNullOrEmpty(identity.Name) || string.IsNullOrEmpty(identity.TenantGroupId))
                {
                    throw new Exception("Validate Account failed");
                }
                TenantLocalValue.LogonUserEmail = identity.Name;
                TenantLocalValue.LogonGroupId = identity.TenantGroupId;
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn("WebAPI Validate Account failed {0}", ex.ToString());
                return false;
            }
        }

        public async Task<int> AddSiteCollectionOrTeamSiteCustomSettingAsync(string requestId, string token, string sitecollectionURL, string RootTermPath, string DefaultTermPath, string applyToExistDocuments = "false", string overWriteExist = "false", string ApplySettingNow = "false")
        {
            try
            {
                logger.Info($"GAO Request Id {requestId},{sitecollectionURL}:{RootTermPath}{DefaultTermPath}{applyToExistDocuments}{overWriteExist}{ApplySettingNow}");
                bool applyToExist = false;
                bool overWiteExist = false;
                bool runNow = false;
                Boolean.TryParse(applyToExistDocuments, out applyToExist);
                Boolean.TryParse(overWriteExist, out overWiteExist);
                Boolean.TryParse(ApplySettingNow, out runNow);
                if (!ValidateToken())
                {
                    throw new Exception("UnKnown or Invalid Token");
                }
#if DEBUG
                //TenantLocalValue.LogonUserEmail = "admintest@M365x96735542.onmicrosoft.com";
                //TenantLocalValue.LogonGroupId = "5c8375d4-db90-4ce6-9f42-c0b47eef0e4c";
#endif
                #region set custom setting first

                if (string.IsNullOrEmpty(sitecollectionURL))
                {
                    sitecollectionURL = GetSiteCollectionURL(requestId);
                }

                RemoteSiteCollection site = RABrowserClient.GetRemoteSiteCollectionByUrl(sitecollectionURL);

                if (site == null)
                {
                    logger.Info($"get it from AOS again {sitecollectionURL}");
                    //sleep 30s or Retry after 30s
                    try
                    {
                        var remoteNodes = RMAosApiClient.GetRemoteNodeBySiteUrl(TenantLocalValue.LogonGroupId, sitecollectionURL);
                        if (remoteNodes.Count > 0)
                        {
                            logger.Info($"{remoteNodes.FirstOrDefault()?.Url} {remoteNodes.FirstOrDefault()?.Id} is in Aos container.");
                        }
                        else
                        {
                            logger.Info("Retry get remote node from Aos ");
                            Thread.Sleep(30 * 1000);
                            remoteNodes = RMAosApiClient.GetRemoteNodeBySiteUrl(TenantLocalValue.LogonGroupId, sitecollectionURL);
                        }
                        if (remoteNodes.Count > 0)
                        {
                            SyncAosRemoteNodes(remoteNodes);
                            sitecollectionURL = remoteNodes.FirstOrDefault()?.Url;
                            logger.Info($"init by site collection url again {sitecollectionURL} ");
                            site = RABrowserClient.GetRemoteSiteCollectionByUrl(sitecollectionURL);
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Info($" init node from AOS failed {e}");
                    }

                }
                if (site == null)
                {
                    logger.Info($"Can't init site object {sitecollectionURL}");
                    return 305;
                }
                if (string.Equals(sitecollectionURL, site.url, StringComparison.OrdinalIgnoreCase))
                {
                    //参数是真正的SiteCollection
                    logger.Debug("url from model is a Site Collection");
                }
                else
                {
                    logger.Debug("url from model is not a Site Collection");
                    //其它级别
                }

                RemoteWebApplication remoteSiteGroup = RABrowserClient.GetWebApplicationById(site.parentId);
                if (DefaultTermPath != null || RootTermPath != null)
                {
                    int status = await SharePointSiteService.SetRMSharePointSettingAsync(remoteSiteGroup, site, DefaultTermPath, RootTermPath, applyToExist, overWiteExist);
                    if (status == 0 && runNow)
                    {
                        logger.Info($"Apply setting now {sitecollectionURL}");
                        int runJobStatus = SharePointSiteService.ApplySharePointSettingJobOnNode(site);
                        logger.Info($"{sitecollectionURL} job status {runJobStatus}");
                        return runJobStatus;
                    }
                    else
                    {
                        logger.Info($"Api result status {status} {sitecollectionURL}");
                        return status;
                    }
                }
                else
                {
                    logger.Info("default term and root term is both null.");
                    return 1;
                }

                #endregion
            }
            catch (Exception e)
            {
                logger.Error("Apply Records Setting Through Web Api failed {0}", e.ToString());
            }
            return 1;
        }

        public async Task<int> AddSiteCollectionCustomSettingAsync(string requestId, string token, string sitecollectionURL, string RootTermPath, string DefaultTermPath)
        {
            try
            {
                logger.Info("Request Id {0} ", requestId);
                if (!ValidateToken())
                {
                    throw new Exception("UnKnown or Invalid Token");
                }
                #region set custom setting first
                //DAOAPIClientV1 test = new DAOAPIClientV1();
                if (string.IsNullOrEmpty(sitecollectionURL))
                {
                    sitecollectionURL = GetSiteCollectionURL(requestId);
                }
                //RemoteSiteCollection site = test.GetRemoteSiteCollectionByUrl(sitecollectionURL);
                RemoteSiteCollection site = RABrowserClient.GetRemoteSiteCollectionByUrl(sitecollectionURL);

                if (site == null)
                {
                    return 305;     //注册成功  但是url改变了, 无法获取详细信息
                }
                if (string.Equals(sitecollectionURL, site.url, StringComparison.OrdinalIgnoreCase))
                {
                    //参数是真正的SiteCollection
                    logger.Debug("url from model is a Site Collection");
                }
                else
                {
                    logger.Debug("url from model is not a Site Collection");
                    //其它级别
                }
                logger.Debug("Finish auto register site to DocAve. start to apply classification");
                //RemoteWebApplication remoteSiteGroup = test.GetWebApplicationById(site.parentId);
                RemoteWebApplication remoteSiteGroup = RABrowserClient.GetWebApplicationById(site.parentId);
                if (DefaultTermPath != null || RootTermPath != null)
                {
                    return await SharePointSiteService.SetRMSharePointSettingAsync(remoteSiteGroup, site, DefaultTermPath, RootTermPath);
                }
                else
                {
                    logger.Info("default term and root term is both null.");
                    return 1;
                }

                #endregion
            }
            catch (Exception e)
            {
                logger.Error("Apply Records Setting Through Web Api failed {0}", e.ToString());
            }
            return 1;
        }
        private void SyncAosRemoteNodes(List<RemoteNode> aosRemoteNodes)
        {
            try
            {
                var localRemoteNodes = aosRemoteNodes.ConvertAll(ConvertAosRemoteNode);
                var redisCache = localRemoteNodes.ToDictionary(item => item.url.ToLower(), item => new SyncRemoteNodePara
                {
                    NodeName = item.url,
                    ParentId = item.parentId,
                    AppType = item.AppType,
                    AuthType = item.AuthType,
                    ServiceAccountId = item.ServiceAccountId,
                    TenantId = item.TenantId,
                    ScanSource = item.ScanSource,
                    TeamId = item.TeamId,
                    NodeLevel = ConvertSiteCollectionNodeLevel(item.NodeType, item.ChannelType),
                });
                RemoteNodeCacheService.AddNodesToCache(TenantLocalValue.LogonGroupId, redisCache, () =>
                {
                    RemoteNodeService.SyncRemoteSiteCollections(localRemoteNodes);
                });
                logger.Info($"Successful sync aos remote node: [{string.Join(", ", localRemoteNodes.Select(item => item.id))}] to local.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while sync aos remote ndoes. Error: {e}");
            }
        }

        private RemoteSiteCollection ConvertAosRemoteNode(RemoteNode aosRemoteNode)
        {
            if (string.IsNullOrEmpty(aosRemoteNode.ParentId))
            {
                throw new Exception($"The aos remote node: [{aosRemoteNode.Id}] not relate group.");
            }

            var groupNode = RemoteNodeDao.GetGroupByAosIdAndNodeLevel(aosRemoteNode.ParentId, (int)ConvertGroupNodeLevel(aosRemoteNode.NodeType));
            if (groupNode == null)
            {
                throw new Exception($"Can't find local remote group node by: [{aosRemoteNode.ParentId} - {aosRemoteNode.NodeType}].");
            }

            return new RemoteSiteCollection
            {
                id = aosRemoteNode.Id,
                ObjectId = aosRemoteNode.ObjectId,
                Name = aosRemoteNode.Name,
                parentName = groupNode.NodeName,
                parentId = groupNode.NodeId,
                CreateTime = DateTime.UtcNow.Ticks,
                domain = aosRemoteNode.DomainName,
                ChannelType = (TeamsChannelType)aosRemoteNode.ChannelType,
                SiteCollectionType = (aosRemoteNode.O365GroupType == O365GroupType.TeamsGroup) ? AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Teams : (AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType)(int)aosRemoteNode.SiteCollectionType,
                state = SiteCollectionState.AccessAll,
                SPVersion = aosRemoteNode.SPVersion,
                TemplateName = aosRemoteNode.TemplateName,
                TemplateTitle = aosRemoteNode.TemplateTitle,
                url = aosRemoteNode.Url,
                username = aosRemoteNode.UserName,
                password = string.Empty,
                AdminUrl = aosRemoteNode.AdminUrl,
                TenantId = string.IsNullOrEmpty(aosRemoteNode.TenantId) ? string.Empty : aosRemoteNode.TenantId,
                AuthType = (GCommon.Contract.CentralAdmin.Object.BposConnectionType)aosRemoteNode.ConnectionType,
                AppType = ConvertAosAppProfileType(aosRemoteNode.AppProfileType), // AOS AppToken方式Scan才有意义
                ScanSource = RemoteNodeScanSource.AOS,
                ServiceAccountId = GetServiceAccountId(aosRemoteNode),
                TeamId = aosRemoteNode.ExternalId ?? string.Empty
            };
        }

        private AvePoint.GCommon.Contract.CentralAdmin.Object.AppType ConvertAosAppProfileType(IdentityProviderType providerType)
        {
            var appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365;
            switch (providerType)
            {
                case IdentityProviderType.SharePointOnline:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365;
                    break;
                case IdentityProviderType.SharePoint:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.SharePoint;
                    break;
                case IdentityProviderType.Exchange:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Exchange;
                    break;
                case IdentityProviderType.CustomAzureApp:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp;
                    break;
            }
            return appType;
        }

        private string GetServiceAccountId(RemoteNode aosRemoteNode)
        {
            var serviceAccountId = string.Empty;
            var authType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)aosRemoteNode.ConnectionType;
            switch (authType)
            {
                case AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount:
                    serviceAccountId = HashCodeHelper.ToMD5HashCode(aosRemoteNode.UserName.ToLowerInvariant());
                    break;
                case AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken:
                    if (string.IsNullOrEmpty(aosRemoteNode.UserName))
                    { // AppProfile
                        serviceAccountId = string.Empty;
                    }
                    else
                    { // AppProfile + MFA
                        serviceAccountId = HashCodeHelper.ToMD5HashCode(aosRemoteNode.UserName.ToLowerInvariant());
                    }
                    break;
                case AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("AuthType is {0} and out of range.", authType.ToString());
            }
            return serviceAccountId;
        }

        private NodeLevel ConvertGroupNodeLevel(RemoteNodeType nodeType)
        {
            switch (nodeType)
            {
                case RemoteNodeType.SiteCollection:
                    return NodeLevel.WebApplication;
                case RemoteNodeType.OneDrive:
                    return NodeLevel.SkyDriveProGroup;
                case RemoteNodeType.Office365GroupSites:
                case RemoteNodeType.Office365Group:
                    return NodeLevel.O365GroupSitesGroup;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync.");
            }
        }

        private NodeLevel ConvertSiteCollectionNodeLevel(RemoveNodeType nodeType, TeamsChannelType channelType)
        {
            switch (nodeType)
            {
                case RemoveNodeType.SiteCollection:
                    return NodeLevel.SiteCollection;
                case RemoveNodeType.SkyDrivePro:
                    return NodeLevel.SkyDrivePro;
                case RemoveNodeType.O365GroupSites:
                    return NodeLevel.O365GroupSites;
                case RemoveNodeType.PrivateChannel:
                    return channelType == TeamsChannelType.Private? NodeLevel.PrivateChannel: NodeLevel.SharedChannel;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync.");
            }
        }

        //public int MarkPhysicalLocation(string requestId, string token, string sitecollectionURL)
        //{
        //    logger.Info("Request Id {0}, token {1}", requestId, token);
        //    if (!ValidateToken(token))
        //    {
        //        throw new Exception("UnKnown or Invalid Token");
        //    }
        //    if (string.IsNullOrEmpty(sitecollectionURL))
        //    {
        //        sitecollectionURL = GetSiteCollectionURL(requestId);
        //    }
        //    //if (string.IsNullOrEmpty(fullUrl))
        //    //{
        //    //    fullUrl = sitecollectionURL;
        //    //}
        //    logger.Debug("register physical location {0} ", sitecollectionURL);
        //    //加参 Site Collection Url, 用于取用户密码, 使用ClientAPI
        //    DAOAPIClientV1 test = new DAOAPIClientV1();
        //    RemoteSiteCollection site = test.GetRemoteSiteCollectionByUrl(sitecollectionURL);
        //    if (site == null)
        //    {
        //        logger.Warn("no remote site collection match the site collection url in DA. return 102");
        //        return 102;
        //    }
        //    RemoteWebApplication remoteSiteGroup = test.GetWebApplicationById(site.parentId);
        //    return SharePointSiteService.MarkPhysicalLocation(remoteSiteGroup, site, string.Empty);
        //}
    }
}
