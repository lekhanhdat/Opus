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

using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.Wrapper.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;  // 添加这个for JObject
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;  // 添加这个for File, Path, Directory
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using SiteExistence = Microsoft.SharePoint.Client.SiteExistence;

namespace AvePoint.RA.RACommonUtility.Common
{
    public static class MemoryCacheUtility
    {
        private static readonly LocalCache _CommonCache = new LocalCache(TimeSpan.FromDays(30));

        public static T Get<T>(string key, TimeSpan expTime, Func<T> valueFactory) where T : class
        {
            return _CommonCache.Get<T>(key, valueFactory, expTime);
        }

        public static T Get<T>(string key, string customerId, TimeSpan expTime, Func<T> valueFactory) where T : class
        {
            return _CommonCache.Get<T>($"{key}_{customerId}", valueFactory, expTime);
        }
    }

    public class SpCommonUtility
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SpCommonUtility));
        private static List<string> mDesignLists = null;
        private static List<string> designLists
        {
            get
            {
                mDesignLists = mDesignLists ?? GetDesignLists();
                return mDesignLists;
            }
        }

        public static bool CheckIsDesignList(string listInfo)
        {
            return designLists.Contains(listInfo);
        }

        private static List<string> GetDesignLists()
        {
            List<string> results = new List<string>();
            try
            {
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Config/DesignLists/DesignLists.config";
                XmlDocument doc = new XmlDocument();
                doc.Load(configFilePath);
                foreach (var node in doc.GetElementsByTagName("List"))
                {
                    XmlElement xe = (XmlElement)node;
                    results.Add(xe.GetAttribute("url") + xe.GetAttribute("serverTemplate"));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get Design Lists config file error {0}", ex.ToString());
            }
            return results;
        }
        
        public static List<IAveFolder> GetFoldersItems(IAveList list, IAveFolder folder, out List<IAveListItem> allItems, int rowLimit = 5000)
        {
            allItems = new List<IAveListItem>();
            List<IAveListItem> tempAllItems = null;
            CAMLManager cmForFolders = new CAMLManager();
            cmForFolders.RowLimit = rowLimit;
            cmForFolders.ScopeType = Types.ScopeTypes.RecursiveAll;
            AveCamlQuery queryForFolders = new AveCamlQuery();
            string xml = cmForFolders.GetFullCAML();
            if (folder != null)
            {
                queryForFolders.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            }
            queryForFolders.ViewXml = xml;
            logger.Info("query items xml {0}", xml);
            List<IAveListItem> allFolders = null;
            List<IAveListItem> tempFolders = null;
           
            IAveListItemCollection items = list.GetItems(queryForFolders);
            allFolders = items.Where(f => f["FSObjType"].ToString() == "1").ToList();
            allItems = items.Where(f => f["FSObjType"].ToString() == "0").ToList();
            IAveListItemCollectionPosition position = items.ListItemCollectionPosition;

            while (position != null)
            {
                queryForFolders.ListItemCollectionPosition.PagingInfo = position.PagingInfo;
                IAveListItemCollection tempItems = list.GetItems(queryForFolders);
                position = tempItems.ListItemCollectionPosition;
                tempFolders = tempItems.Where(f => (string)f["FSObjType"] == "1").ToList();
                tempAllItems = tempItems.Where(f => (string)f["FSObjType"] == "0").ToList();
                allFolders.AddRange(tempFolders);
                allItems.AddRange(tempAllItems);
            }

            List<IAveFolder> discoverFolders = allFolders.Select(i => i.Folder).ToList();
            discoverFolders.Insert(0, list.RootFolder);

            return discoverFolders;
        }

        public static List<IAveFolder> GetAllFolders(IAveList list, int rowLimit = 5000)
        {
            if (list.ItemCount <= rowLimit)
            {
                var returnFolders = list.Folders.Select(i => i.Folder).ToList();
                returnFolders.Insert(0, list.RootFolder);
                return returnFolders;
            }

            CAMLManager cmForFolders = new CAMLManager();
            cmForFolders.RowLimit = rowLimit;
            cmForFolders.ScopeType = Types.ScopeTypes.RecursiveAll;

            AveCamlQuery queryForFolders = new AveCamlQuery();
            string xml = cmForFolders.GetFullCAML();
            queryForFolders.ViewXml = xml;
            logger.Info("query folders xml {0}", xml);
            List<IAveListItem> allFolders = null;
            List<IAveListItem> tempFolders = null;
            IAveListItemCollection folders = list.GetItems(queryForFolders);
            allFolders = folders.Where(f => f["FSObjType"].ToString() == "1").ToList();

            IAveListItemCollectionPosition position = folders.ListItemCollectionPosition;

            while (position != null)
            {
                queryForFolders.ListItemCollectionPosition.PagingInfo = position.PagingInfo;
                IAveListItemCollection tempItems = list.GetItems(queryForFolders);
                position = tempItems.ListItemCollectionPosition;
                tempFolders = tempItems.Where(f => (string)f["FSObjType"] == "1").ToList();

                allFolders.AddRange(tempFolders);
            }

            List<IAveFolder> discoverFolders = allFolders.Select(i => i.Folder).ToList();
            discoverFolders.Insert(0, list.RootFolder);

            return discoverFolders;
        }
        

        public static long ConfigItemsByQueryInfo(SPQueryInfo queryInfo, Func<List<RMDiscoverItem>, int> callbackFun)
        {
            IAveList list = null;
            IAveFolder folder = null;
            long total = 0;
            if (queryInfo.Valid())
            {
                list = queryInfo.List;
                folder = queryInfo.CurrentFolder;
                if (queryInfo.MaxItemId > 0)
                {
                    var startIndex = queryInfo.StartIndex;
                    var endIndex = queryInfo.MaxItemId;
                    var rowLimit = queryInfo.RowLimit;
                    var cm = queryInfo.CAML;
                    Types.ScopeTypes ScopeType = queryInfo.ScopeType;

                    while (endIndex > queryInfo.StartIndex)
                    {
                        //清理每次的ID Query 条件, 重新设置ID查询范围
                        cm.QueryGroup.Conditions.RemoveAll(g => g.Query.Field == SPColumnConstants.SP_ID);
                        startIndex = endIndex - rowLimit > queryInfo.StartIndex ? endIndex - rowLimit : queryInfo.StartIndex;
                        logger.Debug($"query list index: {list.RootFolder.ServerRelativeUrl}, query by id from {startIndex} to {endIndex}.");
                        AveCamlQuery query = new AveCamlQuery();

                        cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                        cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                        cm.ScopeType = ScopeType;
                        if (!string.IsNullOrEmpty(queryInfo.ServerRelativeUrl))
                        {
                            query.FolderServerRelativeUrl = queryInfo.ServerRelativeUrl;
                        }
                        cm.RowLimit = rowLimit;
                        string queryXml = cm.GetFullCAML();
                        query.ViewXml = queryXml;
                        logger.Debug($"query list xml:{queryXml}");
                        query.LoadAllItems = false;
                        List<RMDiscoverItem> items = new List<RMDiscoverItem>();
                        IAveListItemCollection tempItems;
                        using (PerformanceScope scope = new PerformanceScope("List.GetItemsForRecordsByCAMLs", addToStatistics: true))
                        {
                            tempItems = list.GetItemsForRecords(query);
                        }
                        foreach (var item in tempItems)
                        {
                            items.Add(new RMDiscoverItem(item, null));
                        }

                        total += callbackFun(items);
                        logger.Debug($"list result:{list.RootFolder.ServerRelativeUrl}, item count:{items.Count}");

                        endIndex = startIndex != queryInfo.StartIndex ? startIndex : queryInfo.StartIndex;

                    }
                }
                
            }

            return total;


        }




        public static void DisableDenyAddAndCustomizePages(IAveSiteProperties siteProperties, string siteUrl)
        {
            //#region remove code for change deny add adn customize pages
            try
            {
                if (siteProperties == null)
                {
                    logger.Warn($"Can't init deny add and cutomize pages setting,site url {siteUrl}");
                    logger.Warn($"May be using the Sites.Selected permission, we cannot grant the custom pages permissions. Try as has permissions first.");
                    return;
                }
                logger.Info("site:{0} DenyAddAndCustomizePages is {1}", siteUrl, siteProperties.DenyAddAndCustomizePages);
                if (siteProperties.DenyAddAndCustomizePages == AveDenyAddAndCustomizePagesStatus.Enabled)
                {
                    logger.Info("Need set DenyAddAndCustomizePages to Disabled");
                    siteProperties.DenyAddAndCustomizePages = AveDenyAddAndCustomizePagesStatus.Disabled;
                    siteProperties.Update();
                    logger.Info("set DenyAddAndCustomizePages to Disabled success");
                }
            }
            catch (Exception e)
            {
                if (siteProperties != null)
                {
                    siteProperties.DenyAddAndCustomizePages = AveDenyAddAndCustomizePagesStatus.Enabled;
                }
                //update failed, reset status for reuse;
                logger.Warn("Site: {0} Disable DenyAddAndCustomizePages error {1}", siteUrl, e.ToString());
                throw new DenyAddAndCustomizePagesEnableExcetion("DenyAddAndCustomizePages is Enable");
            }
            //#endregion

            //logger.Info("site:{0} DenyAddAndCustomizePages is {1}", siteUrl, siteProperties.DenyAddAndCustomizePages);
            //if (siteProperties.DenyAddAndCustomizePages == AveDenyAddAndCustomizePagesStatus.Enabled)
            //{
            //    logger.Error("site:{0} DenyAddAndCustomizePages is {1}", siteUrl, siteProperties.DenyAddAndCustomizePages);
            //    throw new DenyAddAndCustomizePagesEnableExcetion("DenyAddAndCustomizePages is Enable");
            //}
        }

        public static int GetLastItemFolderId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        private static string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result}");
            return result;
        }

        private static string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result}");
            return result;
        }

        private static int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }

        public static string GetSiteLevelExistColumnStaticName(IAveSite site, string columnName)
        {
            var collection = site.RootWeb.Fields;
            var tempField = collection.Where(f => f.Title == columnName).FirstOrDefault();
            tempField ??= collection.Where(f => f.InternalName == columnName).FirstOrDefault();
            if (tempField == null)
            {
                logger.Warn($"[GetSiteLevelExistColumnStaticName] Can not get column by name.");
                return columnName;
            }
            else
            {
                logger.Info($"[GetSiteLevelExistColumnStaticName] Configuration ColumnName:{columnName}, Title:{tempField.Title}, InternalName: {tempField.InternalName}, StaticName: {tempField.StaticName}");
                return tempField.StaticName;
            }
        }

        public static bool IsTeamChannelFolder(IAveListItem folderItem, bool addLog = true)
        {
            if (folderItem.FileSystemObjectType != AveFileSystemObjectType.Folder)
            {
                return false;
            }

            if (folderItem.Properties != null && folderItem.Properties.Count > 0)
            {
                if (folderItem.Properties.ContainsKey("vti_teamchannelurl") && folderItem.Properties["vti_teamchannelurl"] is string teamChannelUrl && !teamChannelUrl.IsNullOrEmpty())
                {
                    if (addLog) logger.Info($"Folder {folderItem.Url} is channel folder. teamChannelUrl: {teamChannelUrl}");
                    return true;
                }

                if (folderItem.Properties.ContainsKey("vti_progid") && folderItem.Properties["vti_progid"] is string propProgid && propProgid.Equals("Team.Channel", StringComparison.OrdinalIgnoreCase))
                {
                    if (addLog) logger.Info($"Folder {folderItem.Url} is channel folder. vti_progid: {propProgid}");
                    return true;
                }
            }

            if (folderItem.FieldValues != null && folderItem.FieldValues.Count > 0)
            {
                if (folderItem.FieldValues.TryGetValue("ProgId", out var progid) && "Team.Channel".Equals(progid?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (addLog) logger.Info($"Folder {folderItem.Url} is channel folder. ProgId: {progid}");
                    return true;
                }

                if (folderItem.FieldValues.TryGetValue("HTML_x0020_File_x0020_Type", out var htmlFT) && "Team.Channel".Equals(htmlFT?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (addLog) logger.Info($"Folder {folderItem.Url} is channel folder. html FileType: {htmlFT}");
                    return true;
                }
            }

            return false;
        }
    }
    public static class SPCommonUtilityExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SPCommonUtilityExtension));
        public static IAveTaxonomyField GetRecordTaxonomyField(this IAveFieldCollection fields, string columnName, bool isIncluedDefaultName = false)
        {
            IAveTaxonomyField aveTaxonomyField = null;
            List<IAveField> aveFields = new();
            aveFields.AddRange(fields.AsQueryable().Where(f => f.Title.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToList());
            aveFields.AddRange(fields.AsQueryable().Where(f => f.InternalName.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToList());
            aveFields.AddRange(fields.AsQueryable().Where(f => f.StaticName.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToList());
            if (isIncluedDefaultName)
            {
                aveFields.AddRange(fields.AsQueryable().Where(f => f.InternalName.Equals("RevIMBCS", StringComparison.OrdinalIgnoreCase)).ToList());
            }
            foreach (var field in aveFields)
            {
                aveTaxonomyField = field as IAveTaxonomyField;
                if (aveTaxonomyField != null)
                {
                    break;
                }
            }
            if (aveTaxonomyField != null)
            {
                logger.Info($"[GetRecordTaxonomyField] Configuration ColumnName:{columnName}, Title:{aveTaxonomyField.Title}, InternalName: {aveTaxonomyField.InternalName}, StaticName: {aveTaxonomyField.StaticName}");
            }
            else
            {
                logger.Warn($"[GetRecordTaxonomyField] Can not get column by name.");
            }
            return aveTaxonomyField;
        }

        public static IAveTenant CreateTenantCompatibleGeo(this AveObjectModelFactory mfactory, AveBPOSAccountInfo bposInfo, string siteUrl)
        {
            var adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, siteUrl);

            IAveTenant aveTenant = null;
            GCommon.Utility.TransientFault.AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
            {
                aveTenant = mfactory.CreateTenant(adminUrl);
            });
            var geoLocationInfo = aveTenant?.GetTenantGeoLocationinfo();
            if (geoLocationInfo != null && geoLocationInfo.Count > 1)
            {
                foreach (var location in geoLocationInfo)
                {
                    if (siteUrl.StartsWith(location.RootSiteUrl) || siteUrl.StartsWith(location.MySiteHostUrl))
                    {
                        adminUrl = location.TenantAdminUrl;
                        logger.Info($"GetTenantGeoLocationinfo.O365 Admin New Url is : {adminUrl}.SiteUrl:{siteUrl}.");
                        GCommon.Utility.TransientFault.AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
                        {
                            aveTenant = mfactory.CreateTenant(adminUrl);
                        });
                    }
                }
            }
            return aveTenant;
        }
    }

    public class SharePointSettingUtility
    {
        private ISPSettingTreeService spTreeService = null;
        private ISharePointSettingDao spSettingsDao = null;
        private ITeamsSettingDao teamSettingsDao = null;
        private IOneDriveSettingDao oneDriveSettingsDao = null;

        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(SharePointSettingUtility));
        public SharePointSettingUtility()
        {
            spTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
            spSettingsDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
            oneDriveSettingsDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
            teamSettingsDao = (ITeamsSettingDao)PlatformWindsorManager.GetService(typeof(ITeamsSettingDao));
        }

        public List<RMSharePointSetting> GetAllPhysicalSiteSettings()
        {
            return spSettingsDao.GetAllPhysicalSiteSettings();
        }
        public RMSharePointSetting GetPhysicalSiteSetting(Guid gourpId, Guid siteId)//TO DO ylgu debug the scope id to get physical setting
        {
            var setting = spSettingsDao.GetSettingInfoByScope(gourpId, siteId, siteId);
            if (setting != null && setting.IsEnableHoldPhyical)
            {
                return setting;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<RMSPTreeNode>> GetRegisteredSPSitesAsync()
        {
            List<RMSPTreeNode> returnList = new List<RMSPTreeNode>();
            List<RMSPTreeNode> registeredSites = spTreeService.LoadFarm();
            var defaultSites = await spTreeService.BrowseAsync(registeredSites[0]);
            foreach (var defaultSite in defaultSites)
            {
                returnList.AddRange(await spTreeService.BrowseAsync(defaultSite));
            }
            return returnList;
        }

        public string GetMedataColumn(Guid scopeId)
        {
            return spSettingsDao.GetMedataColumn(scopeId);
        }
        public string GetTeamsMedataColumn(Guid scopeId)
        {
            return teamSettingsDao.GetMedataColumn(scopeId);
        }

        public string GetOneDriveMetadataColumn(Guid scopeId)
        { 
            return oneDriveSettingsDao.GetMetadataColumn(scopeId);
        }

        public string GetMedataColumn(List<Guid> ids)
        {
            string res = string.Empty;

            foreach (var id in ids)
            {
                if (!Guid.Equals(Guid.Empty, id))
                {
                    res = spSettingsDao.GetMedataColumn(id);
                    if (!string.IsNullOrEmpty(res))
                    {
                        break;
                    }
                }
            }

            return res;
        }

        public RemoteSiteCollection GetRemoteSiteCollection(string id)
        {
            RemoteSiteCollection site = null;
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    //site = new DAOAPIClientV1().GetRemoteSiteCollectionById(id);
                    site = RABrowserClient.GetRemoteSiteCollectionById(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("GetRemoteSiteCollection Failed,siteid is {0}, message:{1}", id, ex.ToString()));
            }
            return site;
        }

        public AveBPOSAccountInfo GetBPOSInfo(NodeItem site)
        {
            if (site.BposInfo == null || site.BposInfo.UserAccountInfo == null)
            {
                throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site.FullPath));
            }
            FipsModeUtil.InitControlCryptoMode();
            return new AveBPOSAccountInfo()
            {
                Domain = site.BposInfo.UserAccountInfo.Domain,
                UserName = site.BposInfo.UserAccountInfo.Username,
                Password = site.BposInfo.UserAccountInfo.Password.ToSecureString()
            };
        }
    }

    public class ReportServiceUtility
    {
        private IRMReportService reportService = null;

        public ReportServiceUtility()
        {
            reportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
        }

        public RMSPTreeNode GetSPFarmTreeNode(string profileId)
        {
            List<RMSPTreeNode> results = new List<RMSPTreeNode>();
            RMProfileDto profile = reportService.GetProfileByIdForReportJob(profileId);
            return reportService.GetFarmSPTreeNode(profile.Extension2);
        }
    }
   

    public class TermUtility
    {
        //private static RALogger logger = RALogger.GetInstance(typeof(TermUtility));

        private IAveTermStore termStore = null;
        private IAveTermSet termSet = null;
        private ITermDao TermDao = null;
        public TermUtility(IAveList list, string columnName)
        {
            try
            {
                IAveTaxonomyField taxonomyField = list.Fields[columnName] as IAveTaxonomyField;
                IAveTaxonomySession session = list.ParentWeb.Site.AveSPTaxonomySession;
                int LCID = 0;
                termStore = AveTaxonomyFieldUtility.GetTermStore(taxonomyField, session, ref LCID);
                if (termStore == null)
                {
                    throw new Exception("Get term store failed.");
                }
                if (!taxonomyField.TermSetId.Equals(Guid.Empty))
                {
                    termSet = termStore.GetTermSet(taxonomyField.TermSetId);
                }
                else
                {
                    throw new Exception("Taxonomy field term set id is null");
                }
                TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Can not find column,web url:[{0}],list title:[{1}],column name:[{2}],error:{3}", list.ParentWebUrl, list.Title, columnName, e.ToString()));
            }
        }
        //public TermUtility(IAveSite site)
        //{
        //    termStore = site.AveSPTaxonomySession.TermStores[0];
        //    TermDao = new TermDao();
        //}
        //public string GetTermPathByID(Guid termId)
        //{
        //    try
        //    {
        //        IAveTerm endTerm = termStore.GetTerm(termId);
        //        return endTerm.PathOfTerm.Replace(';', '/');
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("get term path error,Get term path from Records, {0}:{1}", termId, ex.ToString());
        //        return TermDao.GetTermFullPathForDestroyReport(termId);
        //    }
        //}

        //public string GetTermPath(Guid termId)
        //{
        //    try
        //    {
        //        IAveTerm endTerm = termStore.GetTerm(termSet.ID, termId);
        //        return endTerm.PathOfTerm.Replace(';', '/');
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("get term path error,Get term path from Records, {0}:{1}", termId, ex.ToString());
        //        return TermDao.GetTermFullPathForDestroyReport(termId);
        //    }
        //}
    }
    public class CommonUtilityForSpecialTenant
    {
        private static IRMGlobalKeyValueDao keyValueDao = PlatformWindsorManager.GetService<IRMGlobalKeyValueDao>();
        private const string DEPLOY_OTHER_DC_TENANT_INFO = "DEPLOY_OTHER_DC_TENANT_INFO";
        private static RALogger logger = RALogger.GetInstance(typeof(CommonUtilityForSpecialTenant));
        public static string GetStorageConnectionStringFromConfigFile(StorageStringType storageStringType)
        {
            try
            {
                var checkResult = CheckCurrentTenantIsDeploySpecialDC();
                if (checkResult.Item1)
                {
                    logger.Info($"curret TenantLocalValue.LogonGroupId is :{TenantLocalValue.LogonGroupId?.ToString()},it need use special storage");
                    var configs = GetSpecialRegionConfigs(checkResult.Item2);
                    if (storageStringType == StorageStringType.DefaultStorage)
                    {
                        return configs[RMStorageSetting.SPECIAL_REGIONS_RESOURCES_DEFAULT_STORAGE_CONNECTION_STRING];
                    }
                    else if (storageStringType == StorageStringType.SharedStorage)
                    {
                        return configs[RMStorageSetting.SPECIAL_REGIONS_RESOURCES_SHARED_STORAGE_CONNECTION_STRING];
                    }
                    else
                    {
                        logger.Error("not exist any storage from config when get SpecialStorage");
                        return string.Empty;
                    }
                }
                else
                {
                    return OriginalStorageString(storageStringType);
                }
            }
            catch (Exception e)
            {
                logger.Error($"something error when IsCurrentTenantNeedUseSpecialStorage,error:{e}");
                throw;
            }
        }
        public static string GetJobQueueNameFromConfigFile()
        {
            var checkResult = CheckCurrentTenantIsDeploySpecialDC();
            if (checkResult.Item1)
            {
                logger.Info($"curret TenantLocalValue.LogonGroupId is :{TenantLocalValue.LogonGroupId?.ToString()},it need use specail jobqueue");
                var configs = GetSpecialRegionConfigs(checkResult.Item2);
                return configs[RMStorageSetting.SPECIAL_REGIONS_RESOURCES_JOB_QUEUE_NAME];
            }
            else
            {
                return RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.JOB_QUEUE_NAME];
            }
        }
        private static Tuple<bool, TheSpecialDCKey> CheckCurrentTenantIsDeploySpecialDC()
        {
            try
            {
                // 使用修复后的方法检查是否存在特殊区域配置
                var specialConfigs = GetSpecialRegionConfigsUsingConfiguration();
                if (specialConfigs == null || specialConfigs.Count == 0)
                {
                    logger.Info($"this tenant dc not config special connection string,no need to check it,tenantid:{TenantLocalValue.LogonGroupId?.ToString()}");
                    return new Tuple<bool, TheSpecialDCKey>(false, TheSpecialDCKey.SOUTHAFRICA);
                }
                var key = $"{DEPLOY_OTHER_DC_TENANT_INFO}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.DeployOtherDCTenantInfo}";
                var entity = keyValueDao.GetValueByKey(key);
                logger.Info($"curret TenantLocalValue.LogonGroupId is :{TenantLocalValue.LogonGroupId?.ToString()},GlobalKeyValue:{entity?.Value}");
                if (entity != null && entity.Value.Contains(TenantLocalValue.LogonGroupId?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var tenantInfoes = SerializerHelper.DeserializeByJsonSerializer<List<SpecialTenantInfo>>(entity.Value);
                    return new Tuple<bool, TheSpecialDCKey>(true, tenantInfoes.Where(a => a.TenantId.EqualIgnoreCase(TenantLocalValue.LogonGroupId?.ToString())).Select(a => a.DataSource).FirstOrDefault());
                }
                return new Tuple<bool, TheSpecialDCKey>(false, TheSpecialDCKey.SOUTHAFRICA);
            }
            catch (Exception e)
            {
                logger.Error($"error occued when CheckCurrentTenantIsDeploySpecialDC curret TenantLocalValue.LogonGroupId is :{TenantLocalValue.LogonGroupId?.ToString()}, Exception: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 修复版本的特殊区域配置获取方法，直接使用Microsoft.Extensions.Configuration
        /// </summary>
        private static Dictionary<string, string> GetSpecialRegionConfigs(TheSpecialDCKey dcKey)
        {
            try
            {
                // 直接使用Microsoft.Extensions.Configuration来读取复杂JSON对象
                string regionKey = dcKey.ToString();
                logger.Info($"Attempting to get special region config for: {regionKey}");

                var specialConfigs = GetSpecialRegionConfigsUsingConfiguration();

                if (specialConfigs?.ContainsKey(regionKey) == true)
                {
                    logger.Info($"Successfully loaded special config for region: {regionKey}");
                    return specialConfigs[regionKey];
                }
                else
                {
                    logger.Warn($"Region {regionKey} not found in special configs, attempting file fallback");
                    return GetSpecialRegionConfigFromFileDirect(dcKey);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting special region configs: {ex.Message}", ex);

                // 如果主方法失败，尝试备用方法
                try
                {
                    logger.Info("Attempting fallback configuration loading...");
                    return GetSpecialRegionConfigFromFileDirect(dcKey);
                }
                catch (Exception fallbackEx)
                {
                    logger.Error($"Fallback configuration loading also failed: {fallbackEx.Message}", fallbackEx);
                    return new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// 使用Microsoft.Extensions.Configuration读取特殊区域配置
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> GetSpecialRegionConfigsUsingConfiguration()
        {
            try
            {
                // 尝试多个可能的配置文件位置
                string[] possibleConfigPaths = {
                    SecurityUtils.SafeCombinePath(AppContext.BaseDirectory, "appsettings.json"),
                    SecurityUtils.SafeCombinePath(AppContext.BaseDirectory, "appsettings.Production.json")
                };

                foreach (var configPath in possibleConfigPaths)
                {
                    if (File.Exists(configPath))
                    {
                        try
                        {
                            logger.Info($"Attempting to load config from: {configPath}");

                            var builder = new ConfigurationBuilder()
                                .AddJsonFile(configPath, optional: false, reloadOnChange: false);

                            var configuration = builder.Build();
                            var specialSection = configuration.GetSection("SPECIAL_REGIONS_RESOURCES");

                            if (specialSection.Exists())
                            {
                                var result = new Dictionary<string, Dictionary<string, string>>();

                                foreach (var regionSection in specialSection.GetChildren())
                                {
                                    var regionConfig = new Dictionary<string, string>();
                                    foreach (var configItem in regionSection.GetChildren())
                                    {
                                        regionConfig[configItem.Key] = configItem.Value;
                                    }
                                    result[regionSection.Key] = regionConfig;
                                }

                                logger.Info($"Successfully loaded SPECIAL_REGIONS_RESOURCES from {configPath}");
                                return result;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Failed to load config from {configPath}: {ex.Message}");
                            continue;
                        }
                    }
                }

                logger.Warn("No valid configuration file found with SPECIAL_REGIONS_RESOURCES");
                return null;
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetSpecialRegionConfigsUsingConfiguration: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 直接从文件读取特定区域配置的备用方法
        /// </summary>
        private static Dictionary<string, string> GetSpecialRegionConfigFromFileDirect(TheSpecialDCKey dcKey)
        {
            try
            {
                string[] possibleConfigPaths = {
                    SecurityUtils.SafeCombinePath(AppContext.BaseDirectory, "appsettings.json"),
                    SecurityUtils.SafeCombinePath(AppContext.BaseDirectory, "appsettings.Production.json")
                };

                string regionKey = dcKey.ToString();

                foreach (var configPath in possibleConfigPaths)
                {
                    if (File.Exists(configPath))
                    {
                        var json = File.ReadAllText(configPath);
                        var config = JObject.Parse(json);

                        if (config.ContainsKey("SPECIAL_REGIONS_RESOURCES"))
                        {
                            var specialRegions = config["SPECIAL_REGIONS_RESOURCES"] as JObject;
                            if (specialRegions?.ContainsKey(regionKey) == true)
                            {
                                var regionConfig = specialRegions[regionKey] as JObject;
                                if (regionConfig != null)
                                {
                                    var result = new Dictionary<string, string>();
                                    foreach (var prop in regionConfig.Properties())
                                    {
                                        result[prop.Name] = prop.Value?.ToString();
                                    }
                                    logger.Info($"Successfully loaded region config for {regionKey} from {configPath}");
                                    return result;
                                }
                            }
                        }
                    }
                }

                logger.Warn($"No configuration found for region {regionKey}");
                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                logger.Error($"Error in direct file config loading: {ex.Message}", ex);
                return new Dictionary<string, string>();
            }
        }
        private static string OriginalStorageString(StorageStringType storageStringType)
        {
            if (storageStringType == StorageStringType.DefaultStorage)
            {
                return RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.DEFAULT_STORAGE_CONNECTION_STRING];
            }
            else if (storageStringType == StorageStringType.SharedStorage)
            {
                return RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONNECTION_STRING];
            }
            else
            {
                logger.Error("not exist any storage from config");
                return string.Empty;
            }
        }

        public enum StorageStringType
        {
            DefaultStorage,
            SharedStorage
        }
    }
    public class SpecialTenantInfo
    {
        public string TenantId { get; set; }

        public TheSpecialDCKey DataSource { get; set; }
    }

    public class DownloadCenterUtility
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(DownloadCenterUtility));
        private static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        private static bool? _forceNeedSasUri;
        private static bool _needSasUri;
        /// <summary>
        /// Used for testing
        /// </summary>
        public static bool ForceNeedSasUri
        {
            get
            {
                if (_forceNeedSasUri.HasValue)
                    return _forceNeedSasUri.Value;

                _forceNeedSasUri = _keyValueDao.TryGetBoolValue("ForceNeedSasUri", out var result) && result;

                if (_forceNeedSasUri.Value) _logger.Info("using ForceNeedSasUri");

                return _forceNeedSasUri.Value;
            }
            set
            {
                _forceNeedSasUri = value;
                _logger.Info($"ForceNeedSasUri set to {value}");
            }
        }

        private static string _sharedCS;
        public static string SharedCS
        {
            get
            {
                if (!string.IsNullOrEmpty(_sharedCS))
                    return _sharedCS;

                _sharedCS = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);

                if (string.IsNullOrEmpty(_sharedCS)) // if cannot get the shareCS, process further is meaningless, throw
                {
                    _logger.Error("Cannot get the shared storage connection string, pls check the config file");
                    throw new Exception("RM_HS_Criteria_View_Msg_ValidOtherError");
                }

                return _sharedCS;
            }
        }
        private static string _blobName;

        public static string UploadStorageForDownloadCenter(string blobName, string filePath)
        {
            (_blobName, _needSasUri) = RAStorageUtil.UploadStorageForDownloadCenter(blobName, filePath, ForceNeedSasUri, SharedCS);
            return _blobName;
        }
        public static string UploadStorageForDownloadCenter(string blobName, Stream fileStream)
        {
            (_blobName, _needSasUri) = RAStorageUtil.UploadStorageForDownloadCenter(blobName, fileStream, ForceNeedSasUri, SharedCS);
            return _blobName;
        }

        // UploadStorageForDownloadCenter must be called before this method
        public static async Task<string> GenerateSasUri()
        {
            if (!ForceNeedSasUri && !_needSasUri)
            {
                _logger.Info("No need to Create File SAS");
                return string.Empty;
            }
            if (string.IsNullOrEmpty(_blobName))
            {
                _logger.Warn("blobName not found to GenerateSasUri");
                return string.Empty;
            }

            _logger.Info($"Start GenerateSasUri for {_blobName}");
            var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
            AzureBlobStorage azureBlobStorage = new(SharedCS, containerName);
            if (await azureBlobStorage.CheckBlobExistAsync(_blobName))
            {
                var sasUri = Util.MSAzure.StorageUtil.GenerateSasUriForRead(SharedCS, containerName, _blobName, TimeSpan.FromDays(7));
                _logger.Info("Finish Create File SAS");
                return sasUri;
            }
            else
            {
                throw new Exception($"Can not find blob, blobName:{_blobName}.");
            }
        }
    }

    public class SPOExportUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SPOExportUtility));

        //private static string SiteUrl { get; set; }
        //private static string WebRelativePath { get; set; }
        //private static string ListRelativePath { get; set; }

        private static IAveList aveList = null;
        public static bool IsExportToSPODocumentLibrary = false;

        public static (bool isValid, string listFullPath) ValidateWebUrl(IAveSite mSite, string webUrl, string libRelativePath, AveBPOSAccountInfo bposInfo, string siteId, bool needCacheList = false)
        {
            try
            {
                using IAveWeb web = webUrl.Equals(mSite.Url, StringComparison.OrdinalIgnoreCase)
                    ? mSite.RootWeb
                    : mSite.OpenWeb(webUrl.Substring(mSite.Url.Length).Trim('/'));
                if (web == null || !web.Exists)
                {
                    logger.Info($"Cannot connect to web: {webUrl}. WebIsExist: {web?.Exists}");
                    return (false, null);
                }

                IAveList list = null;
                if (libRelativePath.Contains("#/"))
                {
                    list = web.GetListFromUrl(libRelativePath.Substring(libRelativePath.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                }
                else
                {
                    list = web.GetList(libRelativePath);
                }

                if (list == null || !(list?.RootFolder?.Exists == true))
                {
                    logger.Info($"Cannot connect to list: {libRelativePath}. ListIsExist: {list?.RootFolder?.Exists}");
                    return (false, null);
                }

                var listTemplate = Convert.ToInt32(list.BaseTemplate);
                //判断是不是library
                if (listTemplate == 101 || listTemplate == 1302 || listTemplate == 700)
                {
                    logger.Info("This is a library, List Template is [{0}], List path is {1}", listTemplate, libRelativePath);
                    if (needCacheList)
                    {
                        //SiteUrl = mSite.Url;
                        //WebRelativePath = web.ServerRelativeUrl;
                        //ListRelativePath = list.RootFolder.ServerRelativeUrl;
                        aveList = list;
                        IsExportToSPODocumentLibrary = true;
                    }
                    return (true, GetListFullUrl(list));
                }

                logger.Info("This is not a library, List Template is [{0}], List path is {1}", listTemplate, libRelativePath);
            }
            catch (Exception ex)
            {
                logger.Error("Failed validating location url for ra, [{0}],error message:{1}", libRelativePath, ex.Message);
            }
            return (false, null);
        }


        public static bool UploadToSPODocumentLibrary(string filePath)
        {
            if (aveList == null)
            {
                logger.Warn("Document library is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                logger.Warn($"Local file not found: {filePath}");
                return false;
            }
            var listFullUrl = GetListFullUrl(aveList);

            try
            {
                var fileRelativePath = aveList.RootFolder.ServerRelativeUrl + "/" + Path.GetFileName(filePath);
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var aveFile = aveList.AddItem(fileRelativePath, fs, true);
                if (aveFile != null && aveFile.File.Exists)
                {
                    logger.Info($"Finish uploading the report to document library: {listFullUrl}, File Url: {aveFile.Url}");
                    return true;
                }
                else
                {
                    logger.Warn($"Failed uploading the report to document library: {listFullUrl}");
                }
            }
            catch (IOException ioEx)
            {
                logger.Error($"IO error while reading file {filePath}. Ex: {ioEx}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                logger.Error($"Access denied reading file {filePath}. Ex: {uaEx}");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed uploading the report for to document library: {listFullUrl}, Ex: {ex}");
            }

            return false;
        }

        public static string GetListFullUrl(IAveList current)
        {
            return current.ParentWeb.Url + "/" + current.RootFolder.Url;
        }
    }

    public class StorageDeviceUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(StorageDeviceUtility));

        // Validate access point for azure storage type only.
        public static bool ValidateAzureAccessPoint(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                logger.Warn("Access point is null.");
                return false;
            }
            //if (s.Length > 255)
            //{
            //    logger.Warn("Access point is too long.");
            //    return false;
            //}
            List<char> specialChars = new List<char>() { '/', '*', '?', '<', '>', '\"', '|' };
            foreach (char c in specialChars)
            {
                if (s.EndsWith(c))
                {
                    logger.Warn($"Access point ends with invalid character: {c}");
                    return false;
                }
            }
            return true;
        }
    }

    public class SiteStateTransitionScopeUtility : IDisposable
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(SiteStateTransitionScopeUtility));
        private readonly string siteCollectionUrl;
        private AveObjectModelFactory aveObjectModelFactory;
        private readonly SiteState targetState;
        private SiteState? originalState;
        private bool hasAttemptedConversion;
        private bool hasChanged;
        private bool disposed;
        private static readonly MemoryCache DedicatedTenantCache = new MemoryCache("SiteStateTenantCache");

        private readonly bool _isRestoreJob = false;

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private AveObjectModelFactory EnsureFactory
        {
            get
            {
                if (this.aveObjectModelFactory == null)
                {
                    logger.Info($"AveObjectModelFactory is null, creating a new instance for site collection: {siteCollectionUrl}");
                    aveObjectModelFactory = !_isRestoreJob ? CreateAveObjectModelFactory() : CreateAveObjectModelFactoryForRestore();
                }
                return this.aveObjectModelFactory;
            }
        }

        public SiteStateTransitionScopeUtility(string siteCollectionUrl, AveObjectModelFactory aveObjectModelFactory, SiteState targetState) : this(siteCollectionUrl, aveObjectModelFactory, targetState, false) { }

        /// <summary>
        /// It's used when only provide site collection url and target state, will create AveObjectModelFactory by itself if need.
        /// </summary>
        public SiteStateTransitionScopeUtility(string siteCollectionUrl, SiteState targetState, bool needConvertState) : this(siteCollectionUrl, null, targetState, needConvertState) { }

        public SiteStateTransitionScopeUtility(string siteCollectionUrl, SiteState targetState, bool needConvertState, bool isRestoreJob) : this(siteCollectionUrl, null, targetState, needConvertState, isRestoreJob)
        {
        }

        public SiteStateTransitionScopeUtility(string siteCollectionUrl, AveObjectModelFactory aveObjectModelFactory, SiteState targetState, bool needConvertState, bool isRestoreJob = false)
        {
            _isRestoreJob = isRestoreJob;
            if (string.IsNullOrWhiteSpace(siteCollectionUrl))
            {
                throw new ArgumentException("Site collection url is required.", nameof(siteCollectionUrl));
            }
            this.siteCollectionUrl = siteCollectionUrl;
            this.targetState = targetState;
            // can create it later
            this.aveObjectModelFactory = aveObjectModelFactory; // ?? throw new ArgumentNullException(nameof(aveObjectModelFactory));
            
            if (needConvertState)
            {
                TryConvertToTargetStatus();
            }
        }

        public AveObjectModelFactory CreateAveObjectModelFactory()
        {
            try
            {
                //AveBPOSAccountInfo bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
                var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteCollectionUrl);
                if (remoteSiteCollection == null)
                {
                    logger.Warn($"Cannot find {siteCollectionUrl} in the RemoteSiteCollection table. so skip remove stub.");
                    throw new ArgumentException("Site collection is required.", nameof(remoteSiteCollection));
                }
                AveBPOSAccountInfo bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                return MultiAppUtil.CreateAveObjectModelFactory(siteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialize SiteStateTransitionScopeUtility for site:{siteCollectionUrl}, state: {targetState} ,ex:{e}");
                throw new Exception("RM_AR_Restore_SiteLocked_ErrorMessage", e);
            }
        }

        public AveObjectModelFactory CreateAveObjectModelFactoryForRestore()
        {
            try
            {
                var o365TenantId = _archiverSiteMasterIndexDao.GetO365TenantIdBySiteCollectionAsync(siteCollectionUrl).ExecuteAsyncTask();
                if (string.IsNullOrEmpty(o365TenantId))
                {
                    o365TenantId = RABrowserClient.GetRemoteSiteCollectionByUrl(siteCollectionUrl).TenantId;
                }
                var remoteSiteCollection = new RemoteSiteCollection()
                {
                    url = siteCollectionUrl,
                    TenantId = o365TenantId,
                    AdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(null, siteCollectionUrl),
                };
                AveBPOSAccountInfo bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                return MultiAppUtil.CreateAveObjectModelFactory(siteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialize SiteStateTransitionScopeUtility for site: {siteCollectionUrl}, state: {targetState},ex: {e}");
                throw new AveSkipLockSiteException("RM_AR_Restore_SiteLocked_ErrorMessage", e);
            }
        }

        public bool TryConvertToTargetStatus()
        {
            if (hasAttemptedConversion)
            {
                return false;
            }
            hasAttemptedConversion = true;

            try
            {
                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    return false;
                }

                if (!TryParseSiteState(siteProps.LockState, out SiteState currentState))
                {
                    logger.Info($"Unable to parse site lock state:{siteProps.LockState}.");
                    return false;
                }

                originalState ??= currentState;
                if ((int)currentState >= (int)targetState)
                {
                    logger.Info($"No need to change site lock state for site: {siteCollectionUrl}. Current state: {currentState}, Target state: {targetState}.");
                    return true;
                }

                siteProps.LockState = targetState.ToString();
                siteProps.Update();
                hasChanged = true;
                logger.Info($"Successfully changed site lock state for site: {siteCollectionUrl} from {currentState} to {targetState}.");
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to try change site lock state for site:{siteCollectionUrl} ,ex:{e}");
                if (ShouldRethrowForException(e, "converting site lock state"))
                {
                    throw new Exception("RM_AR_Restore_SiteLocked_ErrorMessage", e);
                }
                return false;
            }
        }

        public bool VerifyCurrentSiteState(SiteState targetSiteState)
        {
            try
            {
                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    logger.Warn($"Cannot get site properties for site: {siteCollectionUrl}. Unable to verify current lock state.");
                    return false;
                }
                if (!TryParseSiteState(siteProps.LockState, out SiteState currentState))
                {
                    logger.Warn($"Cannot parse site lock state: {siteProps.LockState}. Unable to verify current lock state.");
                    return false;
                }
                if (currentState == targetSiteState)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to verify site lock state for site: {siteCollectionUrl}, ex:{e}");
            }
            return false;
        }

        public bool IsTeamPrivateChannelSite()
        {
            try
            {
                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    logger.Warn($"Cannot get site properties for site: {siteCollectionUrl}. Unable to verify if it's a private channel site.");
                    return false;
                }
                return AveSPWebTemplate.IsTeamPrivateChannelSite(siteProps.Template);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to check if site is a private channel for site: {siteCollectionUrl}, ex: {e}");
                return false;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            if (!hasChanged || !originalState.HasValue)
            {
                return;
            }

            try
            {
                if (!TryGetSiteProperties(out IAveSiteProperties siteProps))
                {
                    logger.Warn($"Cannot get site properties for site: {siteCollectionUrl}. Unable to restore original lock state.");
                    return;
                }

                if (!TryParseSiteState(siteProps.LockState, out SiteState currentState))
                {
                    logger.Info($"Unable to parse site lock state:{siteProps.LockState}.");
                    return;
                }

                if (currentState == originalState.Value)
                {
                    return;
                }

                siteProps.LockState = originalState.Value.ToString();
                siteProps.Update();
                logger.Info($"Successfully restore site lock state for site: {siteCollectionUrl} to {originalState.Value}.");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to restore site lock state for site:{siteCollectionUrl} ,ex:{e}");
                if (ShouldRethrowForException(e, "restoring site lock state"))
                {
                    throw new Exception("RM_AR_Restore_SiteLocked_ErrorMessage", e);
                }
            }
        }

        private bool TryGetSiteProperties(out IAveSiteProperties siteProps)
        {
            siteProps = null;
            if (string.IsNullOrWhiteSpace(siteCollectionUrl))
            {
                return false;
            }

            string adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(EnsureFactory.AccountInfo, siteCollectionUrl);
            //logger.Info($"O365 Admin Url is : {adminUrl}");

            try
            {
                IAveTenant aveTenant = GetOrCreateCachedTenant(adminUrl);

                siteProps = aveTenant.GetSitePropertiesByUrl(siteCollectionUrl);
                return siteProps != null;
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get site properties from cached tenant for {siteCollectionUrl}. Error: {ex.Message}");
                if (!_isRestoreJob)
                {
                    DedicatedTenantCache.Remove(adminUrl);
                }
                return false;
            }
        }

        private IAveTenant GetOrCreateCachedTenant(string adminUrl)
        {
            var tenantLazy = new Lazy<IAveTenant>(() =>
            {
                logger.Info($"Creating tenant for admin url: {adminUrl}");
                var tenant = EnsureFactory.CreateTenant(adminUrl);
                if (tenant.TryGetAdminUrlForMultiGeoTenant(siteCollectionUrl, out string geoAdminUrl))
                {
                    logger.Info($"O365 Tenant is a multiple geo tenant, admin Url is : {geoAdminUrl}");
                    tenant = EnsureFactory.CreateTenant(geoAdminUrl);
                }
                return tenant;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            if (_isRestoreJob)
            {
                return tenantLazy.Value;
            }

            var policy = new CacheItemPolicy 
            {
                //SlidingExpiration = TimeSpan.FromMinutes(2)
                SlidingExpiration = TimeSpan.FromHours(1) 
            };

            var existingLazy = DedicatedTenantCache.AddOrGetExisting(adminUrl, tenantLazy, policy) as Lazy<IAveTenant>;
            var actualLazy = existingLazy ?? tenantLazy;

            return actualLazy.Value;
        }

        private static bool TryParseSiteState(string lockState, out SiteState state)
        {
            return Enum.TryParse(lockState, true, out state);
        }

        private bool ShouldRethrowForException(Exception exception, string action)
        {
            var existence = GetSiteExistence();
            logger.Info($"Site existence check result is {existence} while {action} for site:{siteCollectionUrl}.");
            if (existence == SiteExistence.No || existence == SiteExistence.Recycled)
            {
                logger.Info($"Site collection is not available while {action}. Site:{siteCollectionUrl} Error:{exception}.");
                return false;
            }
            return true;
        }


        private SiteExistence GetSiteExistence()
        {
            string adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(EnsureFactory.AccountInfo, siteCollectionUrl);
            try
            {
                var aveTenant = GetOrCreateCachedTenant(adminUrl);
                return aveTenant.SiteExistsAnywhere(siteCollectionUrl);
            }
            catch
            {
                if (!_isRestoreJob)
                {
                    DedicatedTenantCache.Remove(adminUrl);
                }
                logger.Error($"Failed to get site existence for site:{siteCollectionUrl} when accessing tenant admin url: {adminUrl}. Assuming site does not exist.");
                throw;
            }
        }
    }

    public class FSHighPerformanceUtility
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(FSHighPerformanceUtility));

        private static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private const string FSHighPerformanceKey = KeyNameCollection.EnableFileSystemHighPerformanceMode;

        public static FSHighPerformanceConfiguration LoadFSHighPerformanceConfig()
        {
            var defaultConfig = new FSHighPerformanceConfiguration();
            var setting = _keyValueDao.GetValueByKey(FSHighPerformanceKey);
            if (setting == null || string.IsNullOrWhiteSpace(setting.Value)) return defaultConfig;

            try
            {
                defaultConfig = JsonConvert.DeserializeObject<FSHighPerformanceConfiguration>(setting.Value);
                if (defaultConfig?.Setting == null)
                {
                    defaultConfig.Setting = new FSHighPerformanceSetting();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Deserialize FSHighPerformanceConfiguration failed. Exception: {e}");
            }

            return defaultConfig;
        }
        
        public static JPMCUpgradeSetting LoadFSUpgradeConfig()
        {
            var defaultConfig = new JPMCUpgradeSetting();
            var setting = _keyValueDao.GetValueByKey(KeyNameCollection.JPMCUpgradeSetting);
            if (setting == null) return null;

            try
            {
                defaultConfig = JsonConvert.DeserializeObject<JPMCUpgradeSetting>(setting.Value);
            }
            catch (Exception e)
            {
                _logger.Error($"Deserialize FSHighPerformanceConfiguration failed. Exception: {e}");
            }

            return defaultConfig;
        }

        public static bool IsEnabledJPMCFileSystemFeature()
        {
            return _keyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
        }

        public static RMDataIngestionMessageStatus GetNextMessageStatus(int currentCount)
        {
            var enabledJPMCFSFeature = _keyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if (enabledJPMCFSFeature)
            {
                _logger.Info("JPMC File System Feature is enabled, start to get FS High Performance config.");
                var config = LoadFSHighPerformanceConfig();
                if (config == null)
                {
                    throw new Exception("FS High Performance Mode is not enabled.");
                }

                RMDataIngestionMessageStatus status;

                if (currentCount >= config.Setting.MessageThreshold)
                {
                    status = RMDataIngestionMessageStatus.Waiting;
                }
                else
                {
                    status = RMDataIngestionMessageStatus.Pending;
                }
                _logger.Info($"GetNextMessageStatus: currentCount:{currentCount}, queueThreshold:{config.Setting.MessageThreshold}, status:{status}");
                return status;
            }
            else
            {
                _logger.Info("JPMC File System Feature is not enabled.");
                throw new Exception("JPMC File System Feature is not enabled");
            }

        }
    }

    public class SimulationUtility : IDisposable
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(SimulationUtility));

        private static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly static object _lock = new();

        private readonly bool _isRandomExceptionEnabled = false;
        private readonly double _randomExceptionProbability = 0.3; // Default probability of throwing an exception (30%)

        private static SimulationUtility _instance;
        public static SimulationUtility Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SimulationUtility();
                    }
                }
                return _instance;
            }
        }

        public SimulationUtility()
        {
            _isRandomExceptionEnabled = _keyValueDao.IsEnableRandomExceptionForTestingAsync().ExecuteAsyncTask();
            _randomExceptionProbability = _keyValueDao.GetRandomExceptionProbabilityAsync(defaultValue: _randomExceptionProbability).ExecuteAsyncTask();

            _logger.Info($"SimulationUtility initialized. Random exception testing enabled: {_isRandomExceptionEnabled}, Probability: {_randomExceptionProbability}");
        }

        public void Dispose()
        {
            if (_instance is not null)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Randomly throws an exception if random exception testing is enabled.
        /// Used for testing error handling and recovery scenarios.
        /// </summary>
        /// <param name="operationName">Name of the operation being tested</param>
        /// <param name="probability">Probability of throwing exception (0.0 to 1.0, default 0.3 = 30%)</param>
        public void ThrowRandomExceptionIfEnabled(string operationName, params Exception[] additionalExceptions)
        {
            if (!_isRandomExceptionEnabled)
            {
                return;
            }
            var random = new Random();
            if (random.NextDouble() < _randomExceptionProbability)
            {
                var exceptionTypes = new Action[]
                {
                    () => throw new IOException($"[TESTING] Simulated IO exception during {operationName}"),
                    () => throw new UnauthorizedAccessException($"[TESTING] Simulated access denied during {operationName}"),
                    () => throw new TimeoutException($"[TESTING] Simulated timeout during {operationName}"),
                    () => throw new InvalidOperationException($"[TESTING] Simulated invalid operation during {operationName}")
                };
                if (additionalExceptions is not null && additionalExceptions.Length > 0)
                {
                    exceptionTypes = exceptionTypes.Concat(additionalExceptions.Select(ex => new Action(() => throw ex))).ToArray();
                }
                if (exceptionTypes.Length > 0)
                {
                    var exceptionIndex = random.Next(exceptionTypes.Length);
                    _logger.Warn($"[TESTING] Throwing random exception for operation: {operationName}");
                    exceptionTypes[exceptionIndex]();
                }
            }
        }
    }
}
