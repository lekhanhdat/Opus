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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RADataBroker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace AvePoint.RA.RACommonUtility.Browser
{
    public class RABrowserClient
    {
        //private static bool UseDaoApi = string.Equals("true", RMGlobalConfiguration.AppConfig[RMAppSettingKey.UseDAOBrowserTree]) 
        //    || File.Exists("c:\\ControlUseDaoApi.off");


        private RABrowserClient() { }

        private static readonly IRALogger Logger = RALogger.GetInstance(typeof(RABrowserClient));

        private static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private static readonly IRMMailboxService MailBoxService = PlatformWindsorManager.GetService<IRMMailboxService>();

        public static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();

        public static string LogonUserId { get => TenantLocalValue.LogonUserId; }

        public static string LogonUserEmail { get => TenantLocalValue.LogonGroupEmail; }

        public static string LogonGroupId { get => TenantLocalValue.LogonGroupId; }

        public static RMAccountType AccountType { get => TenantLocalValue.AccountType; }

        #region Get Remote Site Collection

        public static RemoteSiteCollection GetRemoteSiteCollectionById(string id)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetRemoteSiteCollectionById(id);
                //}
                Logger.Info("Get site collection by id: {0}.", id);
                return RemoteNodeService.GetRemoteSiteCollectionById(id);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get remote site collection. Error: {0}", e);
            }
            return null;
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionByObjectId(string id)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetRemoteSiteCollectionById(id);
                //}
                Logger.Info("Get site collection by object id: {0}.", id);
                return RemoteNodeService.GetRemoteSiteCollectionByObjectId(id);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get remote site collection. Error: {0}", e);
            }
            return null;
        }

        public static List<RemoteSiteCollection> GetRemoteSiteCollectionsByIdList(List<string> ids)
        {
            try
            {
            //    if (UseDaoApi)
            //    {
            //        Logger.Info("Use DAO api browser tree.");
            //        return new DAOAPIClientV1().GetRemoteSiteCollectionsByIdList(ids);
            //    }
                return RemoteNodeService.GetRemoteSiteCollectionByIds(ids);
            }
            catch(Exception e)
            {
                Logger.Error("An error occurred while get remote site collections. Error: {0}", e);
            }
            return new List<RemoteSiteCollection>();
        }

        public static List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsByUser(string userName = null)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetAuthorisedRemoteSiteCollectionsByUser(userName);
                //}
                return RemoteNodeService.GetAllRemoteSiteCollections();
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get authorised remote site collections by user. Error: {0}", e);
            }
            return new List<RemoteSiteCollection>();
        }

        public static bool IsRemoteSiteCollectionExistByUrl(string url)
        {
            RemoteSiteCollection result = null;
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().IsRemoteSiteCollectionExistByUrl(url);
                //}
                result = GetRemoteSiteCollectionByUrl(url);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while is remote site collection exist by url. Error: {0}", e);
            }
            return result != null;
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionByUrl(string url)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetRemoteSiteCollectionByUrl(url);
                //}
                var result = RemoteNodeService.GetRemoteSiteCollectionByUrl(url);
                if (result == null)
                {
                    Logger.Warn("There is no sitecollection, url: {0}.", url);
                    return null;
                }
                result.password = string.Empty;
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get remote site collection by url:{url}. Error: {e}.");
            }
            return null;
        }

        public static RMSPTreeNode GetRemoteTeamByTeamId(string teamId)
        {
            try
            {
                var result = RemoteNodeService.GetTeamsNodeByTeamsId(teamId);
                if (result == null)
                {
                    Logger.Warn("There is no team, id: {0}.", teamId);
                    return null;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get remote team by id:{teamId}. Error: {e}.");
            }
            return null;
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionWithBposByUrl(string url)
        {
            var site = GetRemoteSiteCollectionByUrl(url);

            var appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(new List<RemoteSiteCollection> { site }, TenantLocalValue.LogonGroupId);
            site.Bpos = new BposInfo
            {
                SiteUrl = string.Empty,
                AppType = site.AppType,
                ConnectionType = site.AuthType,
                UserAccountInfo = new BposUserAccountInfo
                {
                    Domain = site.domain,
                    Username = site.username,
                    Password = string.Empty,
                    AdminUrl = site.AdminUrl,
                    TenantId = site.TenantId
                }
            };
            site.Bpos.AddCertInfo(site, appProfileDic);

            return site;
        }

        public static bool TryGetRemoteSiteCollectionWithBposByUrl(string url, out RemoteSiteCollection site)
        {
            site = GetRemoteSiteCollectionByUrl(url);
            if(site == null)
            {
                return false;
            }

            var appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(new List<RemoteSiteCollection> { site }, TenantLocalValue.LogonGroupId);
            site.Bpos = new BposInfo
            {
                SiteUrl = string.Empty,
                AppType = site.AppType,
                ConnectionType = site.AuthType,
                UserAccountInfo = new BposUserAccountInfo
                {
                    Domain = site.domain,
                    Username = site.username,
                    Password = string.Empty,
                    AdminUrl = site.AdminUrl,
                    TenantId = site.TenantId
                }
            };
            site.Bpos.AddCertInfo(site, appProfileDic);

            return true;
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionWithBposById(string id)
        {
            var site = GetRemoteSiteCollectionById(id);

            var appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(new List<RemoteSiteCollection> { site }, TenantLocalValue.LogonGroupId);
            site.Bpos = new BposInfo
            {
                SiteUrl = string.Empty,
                AppType = site.AppType,
                ConnectionType = site.AuthType,
                UserAccountInfo = new BposUserAccountInfo
                {
                    Domain = site.domain,
                    Username = site.username,
                    Password = string.Empty,
                    AdminUrl = site.AdminUrl,
                    TenantId = site.TenantId
                }
            };
            site.Bpos.AddCertInfo(site, appProfileDic);

            return site;
        }


        #endregion

        #region Get Web Application

        public static List<RemoteWebApplication> GetWebApplications()
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetWebApplications();
                //}
                return RemoteNodeService.GetWebApplications();
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get web applications. Error: {0}", e);
            }
            return new List<RemoteWebApplication>();
        }

        public static RemoteWebApplication GetWebApplicationById(string id)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetWebApplicationById(id);
                //}
                return RemoteNodeService.GetWebApplicationById(id);
            }
            catch (Exception e)
            {
                Logger.Info("An error occurred while get web application. Error: {0}", e);
            }
            return null;
        }
        #endregion

        #region Browse
        public static GoogleDriveTreeMessage BrowseGoogle(GoogleDriveTreeMessage message)
        {
            //if (message.Node.Level != NodeLevel.Root && !CheckRemoteNodeIsInit())
            //{
            //    return new GoogleDriveTreeMessage()
            //    {
            //        NodeList = []
            //    };
            //}
            try
            {
                var result = GoogleBrowser.Browse(message);
                result.NodeList = result.NodeList.ToList();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error BrowseGoogle.Message:{ex}.");
                throw;
                //return new GoogleDriveTreeMessage()
                //{
                //    NodeList = []
                //};
            }

        }
        public static ExchangeOnlineTreeMessage BrowseExchange(ExchangeOnlineTreeMessage message)
        {
            //if (UseDaoApi)
            //{
            //    Logger.Info("Use DAO api browser tree.");
            //    if (message.Node.Level == NodeLevel.Root)
            //    {
            //        return new DAOAPIClientV1().ExchangeOnlineFarm();
            //    }
            //    return new DAOAPIClientV1().BrowseExchange(message);
            //}
            //if (message.Node.Level != NodeLevel.Root && !CheckRemoteNodeIsInit())
            //{
            //    return new ExchangeOnlineTreeMessage()
            //    {
            //        NodeList = new List<ExchangeOnlineTreeNodeDto>()
            //    };
            //}
            try
            {
                var result = ExchangeBrowser.Browse(message);
                result.NodeList = result.NodeList.Where(t => t.Type != GCommon.Contract.Tree.Object.NodeType.EOO365GroupGroup).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error BrowseExchange.Message:{ex}.");
                throw;
                //return new ExchangeOnlineTreeMessage()
                //{
                //    NodeList = new List<ExchangeOnlineTreeNodeDto>()
                //};
            }
        }

        public static SPTreeMessage BrowseSharePoint(SPTreeMessage message, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            //if (UseDaoApi)
            //{
            //    Logger.Info("Use DAO api browser tree.");
            //    if (message.Node.Level == NodeLevel.Root)
            //    {
            //        return new DAOAPIClientV1().OnlineFarm();
            //    }
            //    return new DAOAPIClientV1().Browse(message);
            //}
            //if(message.Node.Level != NodeLevel.Root && !CheckRemoteNodeIsInit())
            //{
            //    return new SPTreeMessage()
            //    {
            //        NodeList = new List<SPTreeNodeDto>()
            //    };
            //}
            try
            {
                return SharePointBrowser.Browse(message, type);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error BrowseSharePoint.Message:{ex}.");
                throw;
                //return new SPTreeMessage()
                //{
                //    NodeList = new List<SPTreeNodeDto>()
                //};
            }
        }

        private static bool CheckRemoteNodeIsInit()
        {
            return TenantService.GetTenantInitNodeState(TenantLocalValue.LogonGroupId) == Contract.Aos.Notification.RMInitNodeState.Synced;
        }
        #endregion

        #region Exchange Online

        public static BposInfo GetBPOSInfoByEXONode(ExchangeOnlineTreeNodeDto currentNode)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetBPOSInfoByEXONode(currentNode);
                //}
                return MailBoxService.GetBPOSInfoByExchangeNode(currentNode);
            }
            catch (NotSupportedException ex)
            {
                Logger.Error("An error occurred while get bpos info by exo node. Error: {0}", ex);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get bpos info by exo node. Error: {0}", e);
            }
            return null;
        }
        public static BposInfo GetBPOSInfoByTenantId(string tenantId)
        {
            try
            {
                return MailBoxService.GetBPOSInfoById(tenantId);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get bpos info by Id. Error: {0}", e);
            }
            return null;
        }
        public static ExchangeOnlineTreeNodeDto GetExchangeNodeByIdAndAddress(string id, string address)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetExchangeNodeByIdAndAddress(id, address);
                //}
                return MailBoxService.GetExchangeNodeByIdAndAddress(id, address);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get exchange node by id and address. Error: {0}", e);
            }
            return null;
        }

        public static ExchangeOnlineTreeNodeDto GetExchangeNodeById(string id)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetExchangeNodeById(id);
                //}
                var accountDto = MailBoxService.GetMailboxById(id);
                if (accountDto != null && accountDto.State == EmailAccountState.AccessAll)
                {
                    return new ExchangeOnlineTreeNodeDto
                    {
                        ID = accountDto.Id,
                        Level = accountDto.NodeLevel,
                        Name = accountDto.Email,
                        DisplayName = accountDto.Email,
                        CanChildrenBeLoaded = true,
                        EmailAddress = accountDto.Email,
                        FullPath = accountDto.Email,
                        MailboxType = accountDto.MailboxType,
                        ParentId = accountDto.ParentId,
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get exchange node by id. Error: {0}", e);
            }
            return null;
        }

        public static ExchangeOnlineTreeNodeDto GetExchangeNodeByMailBox(string email)
        {
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetExchangeNodeByMailBox(email);
                //}
                var accountDto = MailBoxService.GetMailboxesByEmailAddressName(new List<string> { email }).FirstOrDefault();
                if (accountDto != null && accountDto.State == EmailAccountState.AccessAll)
                {
                    return new ExchangeOnlineTreeNodeDto
                    {
                        ID = accountDto.Id,
                        Level = accountDto.NodeLevel,
                        Name = accountDto.Email,
                        DisplayName = accountDto.Email,
                        CanChildrenBeLoaded = true,
                        EmailAddress = accountDto.Email,
                        FullPath = accountDto.Email,
                        MailboxType = accountDto.MailboxType,
                        ParentId = accountDto.ParentId,
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while get exchange node by mail box. Error: {0}", e);
            }
            return null;
        }
        #endregion

        #region Extension

        public static RemoteSiteCollection GetSiteNode(string fullPath)
        {
            //if (UseDaoApi)
            //{
            //    Logger.Info("Use DAO api browser tree.");
            //    return new DAOAPIClientV1().GetSiteNode(fullPath);
            //}
            ThrowUtil.ThrowIfNull(fullPath, "SiteCollection Url");
            Func<RemoteSiteCollection> getObj = () =>
            {
                return GetRemoteSiteCollectionByUrl(fullPath);
            };
            return CacheService.Get(CacheNamespace.O365Site, fullPath, getObj, TimeSpan.FromHours(12));
        }

        public static RemoteSiteCollection GetSiteNode(Guid aveId)
        {
            //if (UseDaoApi)
            //{
            //    Logger.Info("Use DAO api browser tree.");
            //    return new DAOAPIClientV1().GetSiteNode(aveId);
            //}
            ThrowUtil.ThrowIfNull(aveId, "SiteCollection Id");
            Func<RemoteSiteCollection> getObj = () =>
            {
                List<string> aveIds = new List<string>();
                aveIds.Add(aveId.ToString());
                var node = GetRemoteSiteCollectionsByIdList(aveIds).FirstOrDefault();
                return node;
            };
            return CacheService.Get(CacheNamespace.O365Site, aveId.ToString(), getObj, TimeSpan.FromHours(12));
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl)
        {
            RemoteSiteCollection matchSite = null;
            try
            {
                //if (UseDaoApi)
                //{
                //    Logger.Info("Use DAO api browser tree.");
                //    return new DAOAPIClientV1().GetRemoteSiteCollectionByListUrl(listUrl);
                //}
                Stopwatch watch = new Stopwatch();
                watch.Start();
                listUrl = HttpUtility.UrlDecode(listUrl);
                var sites = GetAuthorisedRemoteSiteCollectionsByUser();
                if (sites != null && sites.Count > 0)
                {
                    matchSite = sites.OrderByDescending(a => a.url.Length).Where(s => listUrl.StartsWith(s.url, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }
                watch.Stop();
                Logger.Info("Get RemoteSiteCollection by list url, Take Milliseconds: {0} ms .", watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.Warn("Error Get RemoteSiteCollection By List Url :{1}, message:{0}", ex.Message, listUrl);
            }
            return matchSite;
        }

        public static RemoteSiteCollection GetRemoteSiteCollectionByListUrlV1(string listUrl)
        {
            RemoteSiteCollection matchSite = null;
            try
            {               
                matchSite = RemoteNodeService.GetRemoteSiteCollectionByListUrl(listUrl);
                Logger.Info("Get RemoteSiteCollection by list url.");
            }
            catch (Exception ex)
            {
                Logger.Warn("Error Get RemoteSiteCollection By List Url :{1}, message:{0}", ex.Message, listUrl);
            }
            return matchSite;
        }

        #endregion

        #region Teams

        public static SPTreeMessage BrowseTeams(SPTreeMessage message, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.Teams)
        {
            //if (UseDaoApi)
            //{
            //    Logger.Info("Use DAO api browser tree.");
            //    if (message.Node.Level == NodeLevel.Root)
            //    {
            //        return new DAOAPIClientV1().OnlineFarm();
            //    }
            //    return new DAOAPIClientV1().Browse(message);
            //}
            //if (message.Node.Level != NodeLevel.Root && !CheckRemoteNodeIsInit())
            //{
            //    return new SPTreeMessage()
            //    {
            //        NodeList = new List<SPTreeNodeDto>()
            //    };
            //}
            try
            {
                return TeamsBrowser.Browse(message, type);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error BrowseTeams.Message:{ex}.");
                throw;
                //return new SPTreeMessage()
                //{
                //    NodeList = new List<SPTreeNodeDto>()
                //};
            }
        }


        #endregion

    }
}
