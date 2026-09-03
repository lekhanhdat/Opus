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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Common.DocAve;
using AvePoint.RA.Common;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.Service.API.SharePointSites
{
    public class DocAveSharePointSiteService : IDocAveSharePointSiteService
    {
        private RALogger logger = RALogger.GetInstance(typeof(DocAveSharePointSiteService));
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermSetMembershipDao TermSetMembershipDao => PlatformWindsorManager.GetService<ITermSetMembershipDao>();

        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IRMSharePointSettingsService RMSharePointSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
       

        public async Task<int> MarkPhysicalLocationAsync(RemoteWebApplication web, RemoteSiteCollection site, string url)
        {
            try
            {
                logger.Debug("Get global setting with webapp url {0}, id {1}", web.url, web.id);
                RMSharePointSetting globalSetting = SharePointSettingDao.GetGroupLevelGlobalSetting(web.url, new Guid(web.id));
                if (globalSetting != null)
                {
                    RMSPTreeNode sitecollection = this.ConvertRemoteSite2RMTreeNode(site);
                    sitecollection.TermId = Guid.Empty;
                    sitecollection.ColumnName = globalSetting.ColumnName;
                    sitecollection.Parent = ConvertRemoteWebApplication2RMTreeNode(web);
                    sitecollection.IsEnableHoldPhyical = true;
                    await RMSharePointSettingsService.AddCustomColumnAsync(new List<RMSPTreeNode>() { sitecollection });
                }
                else
                {
                    return 3;   //没有设置Global Setting
                }
            }
            catch (Exception e)
            {
                logger.Info("Mark physical error {0}", e.ToString());
                return 4;
            }
            logger.Info("Finish mark physical record location.");
            return 0;
        }

        public List<RemoteWebApplication> GetAllSiteGroup()
        {
            //IMOffice365Service Office365Service = DocAveServiceHelper.CreateServiceClient<IMOffice365Service>();
            //var client = new DAOAPIClientV1();
            //List<RemoteWebApplication> webapps = client.GetWebApplications();
            List<RemoteWebApplication> webapps = RABrowserClient.GetWebApplications();
            return webapps;
        }

        public RemoteWebApplication GetRemoteSiteGroupById(string id)
        {
            //IMOffice365Service Office365Service = DocAveServiceHelper.CreateServiceClient<IMOffice365Service>();
            //var client = new DocAveRestApiClient();
            //return client.GetRemoteWebApplicationById(id);
            //var client = new DAOAPIClientV1();
            //return client.GetWebApplicationById(id);
            return RABrowserClient.GetWebApplicationById(id);
        }

        public RemoteWebApplication GetSiteGroup(string groupName)
        {
            logger.Debug("get site group {0}", groupName);
            RemoteWebApplication result = null;
            //IMOffice365Service Office365Service = DocAveServiceHelper.CreateServiceClient<IMOffice365Service>();
            //var client = new DAOAPIClientV1();
            //List<RemoteWebApplication> webapps = client.GetWebApplications();
            List<RemoteWebApplication> webapps = RABrowserClient.GetWebApplications();
            if (webapps != null && webapps.Count > 0)
            {
                result = webapps.FirstOrDefault(a => string.Equals(a.url, groupName, StringComparison.OrdinalIgnoreCase));
            }
            ArgumentCheck.NotNull(result, nameof(result));
            logger.Debug("result : " + result.id);
            return result;
        }

        public RemoteSiteCollection CheckSiteUrlExist(string siteUrl)
        {
            logger.Debug("Validate site URL exist.");
            //IMArchiverService ArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
            //var client = new DAOAPIClientV1();
            //bool exist = client.IsRemoteSiteCollectionExistByUrl(siteUrl);
            bool exist = RABrowserClient.IsRemoteSiteCollectionExistByUrl(siteUrl);
            if (exist)
            {
                logger.Debug("result: true.");
                //RemoteSiteCollection siteCollection = client.GetRemoteSiteCollectionByUrl(siteUrl);
                RemoteSiteCollection siteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                return siteCollection;
            }
            return null;
        }

        //public RABposResult CreateRemoteSite(string siteUrl, string groupName, string userName)
        //{
        //    logger.Debug("Register remote site collection to DocAve.");
        //    IMArchiverService ArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
        //    RABposResult result = ArchiverService.RegistRemoteSiteCollection(siteUrl, groupName, userName);
        //    return result;
        //}
        public int ApplyAllSharePointSettingJob()
        {
            try
            {
                RMSharePointSettingsService.ApplySettings(JobRunBy.Control, false, RunApplySettingMethod.Auto);
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error("Apply Settings by API Error {0}", ex.ToString());
                return 501;
            }
        }
        public int ApplySharePointSettingJobOnNode(RemoteSiteCollection site)
        {
            try
            {
                RMSharePointSettingsService.ApplySettingsOnSelectedNode(this.ConvertRemoteSite2RMTreeNode(site));
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error("Apply Settings by API Error {0}", ex.ToString());
                return 501;
            }
        }

        public async Task<int> SetRMSharePointSettingAsync(RemoteWebApplication web, RemoteSiteCollection site, string defaultTermPath, string rootTermPath, bool applyToExistDocuments = false, bool overWriteExist = false)
        {
            logger.Debug("Get global setting with webapp url {0}, id {1}", web.url, web.id);
            RMSharePointSetting globalSetting = SharePointSettingDao.GetGroupLevelGlobalSetting(web.url, new Guid(web.id));
            if (globalSetting != null)
            {
                RMSPTreeNode sitecollection = this.ConvertRemoteSite2RMTreeNode(site);
                //更新Global Setting的Term, 并触发job
                bool hasDefault = false;
                bool hasScope = false;
                if (defaultTermPath != null && defaultTermPath != string.Empty)
                {
                    hasDefault = true;
                    UpdateDefaultTerm(defaultTermPath, sitecollection);
                }
                if (rootTermPath != null && rootTermPath != string.Empty)
                {
                    hasScope = true;
                    UpdateRootTerm(rootTermPath, sitecollection);
                }
                if (hasDefault || hasScope)
                {
                    if (hasDefault && !hasScope)
                    {
                        logger.Info("Only change default term path, set the custom setting scope term with global setting");
                        sitecollection.TermId = globalSetting.TermId;
                        sitecollection.TermName = globalSetting.TermName;
                        sitecollection.TermSetId = globalSetting.TermSetId;
                        sitecollection.TermSetName = globalSetting.TermSetName;
                    }
                    logger.Debug("Change happened, start to run a job to update sharepoint.");
                    sitecollection.ColumnName = globalSetting.ColumnName;
                    sitecollection.TermIdOfContainer = globalSetting.TermIdOfContainer;
                    sitecollection.TermNameOfContainer = globalSetting.TermNameOfContainer;
                    sitecollection.Parent = ConvertRemoteWebApplication2RMTreeNode(web);
                    sitecollection.EnableRecordManagement = 1;
                    sitecollection.SiteGroupId = globalSetting.SiteGroupId;
                    if (applyToExistDocuments)
                    {
                        await RMSharePointSettingsService.AddCustomColumnAsync(new List<RMSPTreeNode>() { sitecollection }, false, null, applyToExistDocuments, overWriteExist ? 1 : 2);
                    }
                    else
                    {
                        await RMSharePointSettingsService.AddCustomColumnAsync(new List<RMSPTreeNode>() { sitecollection });
                    }
                }
                else
                {
                    logger.Info("No term path changed, finish automaticly");
                }
            }
            else
            {
                return 501;   //没有设置Global Setting
            }
            return 0;
        }

        private void UpdateDefaultTerm(string defaultTermPath, RMSPTreeNode globalSetting)
        {
            string[] tNames = defaultTermPath.Split('/');
            logger.Info("Get termset by name :{0}", tNames[0]);
            RMTermSet termset = TermSetDao.GetTermSetByName(tNames[0]);
            RMTermSetMembership ship = null;
            for (int i = 1; i < tNames.Length; i++)
            {
                int parentId = ship == null ? termset.Id : ship.TermId;
                logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = TermSetMembershipDao.GetByTermNameAndParentId(parentId, tNames[i], ship == null);
                if (ship == null)
                {
                    throw new Exception($"init default term failed,term path not exist {defaultTermPath}");
                }
            }
            if (ship != null)
            {
                logger.Info("get term with path success, Name {0}, Id {1}", ship.TermName, ship.TermId);
                RMTerm term = TermDao.GetRMTermByTermId(ship.TermId);
                globalSetting.TermSetId = termset.UniqueId;
                globalSetting.TermSetName = termset.Name;
                globalSetting.DefaultTermId = term.UniqueId;
                globalSetting.DefaultTermName = term.Name;
                globalSetting.IsDefaultTermDeprecated = term.IsDeprecated;
                globalSetting.IsDefaultTermRemoved = term.IsRemoved;
                logger.Info("Ready to update default term for group level, term id {0}, name {1}, web {2}", term.UniqueId, term.Name, globalSetting.FullPath);
            }

        }
        private void UpdateRootTerm(string rootTermPath, RMSPTreeNode customSetting)
        {
            string[] tNames = rootTermPath.Split('/');
            logger.Info("Get termset by name :{0}", tNames[0]);
            RMTermSet termset = TermSetDao.GetTermSetByName(tNames[0]);
            RMTermSetMembership ship = null;
            for (int i = 1; i < tNames.Length; i++)
            {
                int parentId = ship == null ? termset.Id : ship.TermId;
                logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = TermSetMembershipDao.GetByTermNameAndParentId(parentId, tNames[i], ship == null);
                if (ship == null)
                {
                    throw new Exception($"init root term failed,term path not exist {rootTermPath}");
                }
            }
            if (ship != null)
            {
                RMTerm term = TermDao.GetRMTermByTermId(ship.TermId);
                customSetting.TermSetId = termset.UniqueId;
                customSetting.TermSetName = termset.Name;
                customSetting.TermId = term.UniqueId;
                customSetting.TermName = term.Name;
                customSetting.IsTermDeprecated = term.IsDeprecated;
                customSetting.IsTermRemoved = term.IsRemoved;
                logger.Info("Ready to update root term for site level, term id {0}, name {1}, web {2}", term.UniqueId, term.Name, customSetting.FullPath);
            }
        }
        public bool RegisteRemoteSite(string siteurl, string user, string password, RemoteWebApplication webapp)
        {
            //Check name exist
            //IMOffice365Service Office365Service = DocAveServiceHelper.CreateServiceClient<IMOffice365Service>();
            try
            {
                //Office365MessageContract message = new Office365MessageContract();
                //message.DomainName = "";
                //message.UserName = user;
                //message.SiteCollectionUrl = removeChar(siteurl);
                //Office365TestResult testResult = DocAveOnlineUtility.TestForOffice365(message, webapp, "");
                ////if (!testResult.HasAvailableAgentByStatus)//在Agent Group下没有Up的agent
                ////{
                ////    //失败, 没有可用的Agent
                ////    logger.Warn("no available agent, test o365 site {0} failed.", siteurl);
                ////    return false;
                ////}
                ////else 
                //if (testResult.SiteCollectionState == SiteCollectionState.AccessNone)
                //{
                //    logger.Warn("access denied, test o365 site {0} failed.", siteurl);
                //    //失败, 用户对这个站点没有权限
                //    return false;
                //}
                //else if (testResult.SiteCollectionState == SiteCollectionState.AccessSome)
                //{
                //    logger.Warn("no full access, test o365 site {0} failed.", siteurl);
                //    //只有部分权限, 失败
                //    return false;
                //}
                ////else if (testResult.isTypeMismatch)
                ////{
                ////    logger.Warn("type mismatched, test o365 site {0} failed.", siteurl);
                ////    //类型不匹配, 失败
                ////    return false;
                ////}

                ////IRemoteNodeService RemoteNodeService = DocAveServiceHelper.CreateServiceClient<IRemoteNodeService>();
                ////bool exist = RemoteNodeService.IsRemoteSiteCollectionExistByUrl(testResult.RealSiteCollection);
                ////var client = new DAOAPIClientV1();
                ////bool exist = client.IsRemoteSiteCollectionExistByUrl(testResult.RealSiteCollection);
                //bool exist = RABrowserClient.IsRemoteSiteCollectionExistByUrl(testResult.RealSiteCollection);
                //if (exist)
                //{
                //    logger.Warn("Site url already exists, {0}", removeChar(testResult.RealSiteCollection));
                //    return false;
                //}
                //return new DAOAPIClientV1().CreateRemoteSiteCollection(site);
                return false;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }

        }
        /*private string removeChar(string url)
        {
            url = url.Trim();
            while (url.EndsWith("/"))
            {
                int index = url.LastIndexOf("/");
                url = url.Substring(0, index);
            }
            return url;
        }*/

        private RMSPTreeNode ConvertRemoteSite2RMTreeNode(RemoteSiteCollection siteCollection)
        {
            if (siteCollection.SPVersion == null)
            {
                siteCollection.SPVersion = ((int)AveSPVersion.SharePoint2010).ToString();
            }
            RMSPTreeNode nodeDto = new RMSPTreeNode()
            {
                Id = siteCollection.id,
                SPObjectId = siteCollection.id,
                Name = siteCollection.url,
                DisplayName = siteCollection.url,
                FullPath = siteCollection.url,
                Level = (int)NodeLevel.SiteCollection,
                //Url = siteCollection.url,
                NodeType = (int)ToNodeType(siteCollection.SiteCollectionType),
                SPType = (int)SPType.BPOS,
                // FarmId = siteCollection.FarmId,   Online的没有FarmId

            };
            int spVersion = 0;
            int.TryParse(siteCollection.SPVersion, out spVersion);
            nodeDto.SPVersion = spVersion;
            //nodeDto.IsOnlineSite = siteCollection.IsOnlineSite;
            string domain = siteCollection.domain;
            string username = siteCollection.username;
            BPOSMode mode = /*siteCollection.IsResidentInLocalFarm ? BPOSMode.SecurityTrimming : */BPOSMode.Office365;
            logger.Debug("Site collection Mode: {0}.", mode);
            nodeDto.BposInfo = new BposInfo()
            {
                SiteUrl = siteCollection.url,
                UserAccountInfo = new BposUserAccountInfo()
                {
                    Domain = domain,
                    Username = username,
                    Password = siteCollection.password,
                },
                Mode = mode,
                //RealId = siteCollection.RealId,
            };
            //if (nodeDto.BposInfo.Mode == BPOSMode.SecurityTrimming)
            //{
            //    nodeDto.BposInfo.OriginalFarmId = siteCollection.IsResidentInLocalFarm ? siteCollection.FarmId : string.Empty;
            //}
            return nodeDto;
        }
        private RMSPTreeNode ConvertRemoteWebApplication2RMTreeNode(RemoteWebApplication siteCollection)
        {
            RMSPTreeNode nodeDto = new RMSPTreeNode()
            {
                Id = siteCollection.id,
                SPObjectId = siteCollection.id,
                Name = siteCollection.url,
                DisplayName = siteCollection.url,
                FullPath = siteCollection.url,
                Level = (int)NodeLevel.WebApplication,

                SPType = (int)SPType.BPOS,
            };

            return nodeDto;
        }

        private NodeType ToNodeType(SiteCollectionType type)
        {
            switch (type)
            {
                case SiteCollectionType.Normal: return NodeType.SharePointSites;
                case SiteCollectionType.Teams: return NodeType.O365TeamSites;
                case SiteCollectionType.Group: return NodeType.O365GroupSites;
                case SiteCollectionType.AdminCenter: return NodeType.AdminCenter;
                //case SiteCollectionType.OneDrive: return NodeType.OneDriveSites;
                default: return NodeType.Unused;
            }
        }

    }
}
