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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using static AvePoint.RA.SharePoint.Common.CAMLHelper.CAML.Types;

namespace AvePoint.RA.SharePoint.Common
{
    public class SPCommonUtility
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SPCommonUtility));

        public static RMSPTreeNode DeserializeTreeNodeInfo(string nodeInfo)
        {
            if (string.IsNullOrWhiteSpace(nodeInfo))
            {
                return null;
            }

            return SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeInfo);
        }

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
        

        public static long ConfigItemsByQueryInfo(SPQueryInfo queryInfo, RMReportExtension reportExt, Func<List<RMDiscoverItem>, int> callbackFun)
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
                    ScopeTypes ScopeType = queryInfo.ScopeType;

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
                            if (!NeedProcessItem(item, reportExt, cm))
                            {
                                continue;
                            }
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

        public static bool NeedProcessItem(IAveListItem aveItem, RMReportExtension reportExt, CAMLManager cm)
        {
            if (reportExt != null && reportExt is RMDueDisposalReportListExtension listExt)
            {
                if (listExt.CanUnclassificationQuery && cm.IsUnclassificationQuery && listExt.IsCreatedIndexed)
                {
                    //var itemCreated = aveItem.GetItemFieldValue(SPColumnConstants.SP_Created);

                    var TimeCreated = aveItem.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0;

                    if (TimeCreated < listExt.TimePoint.Ticks)
                    {
                        return true;
                    }
                    return false;
                }
            }

            return true;
        }

        // check any rule criterias is indexed column by supported criterias
        public static bool FilterIndexedIncludeCriteria(IEnumerable<string> indexedColumns, IEnumerable<int> criterias)
        {
            foreach (var criteria in criterias)
            {
                switch ((ArchiverFilterRuleType)criteria)
                {
                    case ArchiverFilterRuleType.Size:
                    case ArchiverFilterRuleType.DocumentSize:
                        if (indexedColumns.Contains(SPColumnConstants.File_Size)) return true;
                        break;
                    case ArchiverFilterRuleType.ModifiedTime:
                        if (indexedColumns.Contains(SPColumnConstants.Modified)) return true;
                        break;
                    case ArchiverFilterRuleType.CreatedTime:
                        if (indexedColumns.Contains(SPColumnConstants.SP_Created)) return true;
                        break;
                    case ArchiverFilterRuleType.CreatedBy:
                        if (indexedColumns.Contains(SPColumnConstants.Author)) return true;
                        break;
                    case ArchiverFilterRuleType.ModifiedBy:
                        if (indexedColumns.Contains(SPColumnConstants.Editor)) return true;
                        break;
                    case ArchiverFilterRuleType.ContentType:
                        if (indexedColumns.Contains(SPColumnConstants.SP_ContentType)) return true;
                        break;
                    case ArchiverFilterRuleType.Title:
                        if (indexedColumns.Contains(SPColumnConstants.SP_Title)) return true;
                        break;
                    case ArchiverFilterRuleType.Name:
                        if (indexedColumns.Contains(SPColumnConstants.SP_NAME)) return true;
                        break;
                    default:
                        logger.Info($"This criteria might not be indexable. Criteria: {criteria}");
                        break;
                }
            }
            return false;
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

        public static int GetFirstItemFolderId(IAveList list, IAveFolder folder)
        {
            string firstItemQueryXml = GetFirstItemQueryXml();
            int firstItemId = InnerGetOneItemIdByQuery(list, folder, firstItemQueryXml);

            string fileQueryXml = GetFirstFileQueryXml();//include file and item
            int maxFileId = InnerGetOneItemIdByQuery(list, folder, fileQueryXml);
            return Math.Min(firstItemId, maxFileId);
        }

        private static string GetFirstItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='TRUE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetFirstItemQueryXml:{result}");
            return result;
        }

        private static string GetFirstFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='TRUE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetFirstFileQueryXml:{result}");
            return result;
        }

        public static int GetLastItemFolderId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetOneItemIdByQuery(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetOneItemIdByQuery(list, folder, fileQueryXml);
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

        private static int InnerGetOneItemIdByQuery(IAveList list, IAveFolder folder, string queryXml)
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
                if (id != null && !Guid.Equals(Guid.Empty, id))
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

        public AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection GetRemoteSiteCollection(string id)
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
}
