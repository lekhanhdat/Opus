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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common.Global;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using RAFileSystem.SharePoint.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using static AvePoint.RA.SharePoint.Common.CAMLHelper.CAML.Types;

namespace AvePoint.RA.SharePoint.Common
{
    public class SPCommonUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SPCommonUtility));
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
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Config\\DesignLists\\DesignLists.config";
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
        public static List<RMRelatedItemInfo> GetRelatedProperties(string recordsRelatedValue)
        {
            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
            if (!string.IsNullOrEmpty(recordsRelatedValue))
            {
                var sourceUrlValue = recordsRelatedValue;
                XmlDocument xmlDoc = new XmlDocument();
                sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                xmlDoc.LoadXml(sourceUrlValue);
                if (xmlDoc.GetElementsByTagName("a").Count > 0)
                {
                    foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                    {
                        XmlElement element = ele as XmlElement;
                        var relatedObjString = element.GetAttribute("rel");
                        relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                        RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                        var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                        string url = relatedItemUrl;
                        relatedObj.url = relatedItemUrl;
                        relatedObj.url = url;
                        infos.Add(relatedObj);
                    }
                }
                else if (xmlDoc.GetElementsByTagName("RMRelatedItemInfo").Count > 0)
                {
                    infos = GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(sourceUrlValue);
                }
            }
            return infos;
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
            logger.Info("query items xml {0}", xml.LogBase64());
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
            logger.Info("query folders xml {0}", xml.LogBase64());
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
                    ScopeTypes ScopeType = queryInfo.ScopeType;

                    while (endIndex > queryInfo.StartIndex)
                    {
                        //清理每次的ID Query 条件, 重新设置ID查询范围
                        cm.QueryGroup.Conditions.RemoveAll(g => g.Query.Field == SPColumnConstants.SP_ID);
                        startIndex = endIndex - rowLimit > queryInfo.StartIndex ? endIndex - rowLimit : queryInfo.StartIndex;
                        logger.Debug($"query list index: {list.RootFolder.ServerRelativeUrl.LogBase64()}, query by id from {startIndex} to {endIndex}.");
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
                        logger.Debug($"query list xml:{queryXml.LogBase64()}");
                        //query.LoadAllItems = false;
                        List<RMDiscoverItem> items = new List<RMDiscoverItem>();
                        var tempItems = list.GetItemsForRecords(query);
                        foreach (var item in tempItems)
                        {
                            items.Add(new RMDiscoverItem(item, null));
                        }

                        total += callbackFun(items);
                        logger.Debug($"list result:{list.RootFolder.ServerRelativeUrl.LogBase64()}, item count:{items.Count}");

                        endIndex = startIndex != queryInfo.StartIndex ? startIndex : queryInfo.StartIndex;

                    }
                }
                
            }

            return total;


        }

        private void DoAction<TSource>(IEnumerable<TSource> items, Action<TSource> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }


        public static void DisableDenyAddAndCustomizePages(IAveSiteProperties siteProperties, string siteUrl)
        {
            //#region remove code for change deny add adn customize pages
            try
            {
                logger.Info("site:{0} DenyAddAndCustomizePages is {1}", siteUrl.LogBase64(), siteProperties.DenyAddAndCustomizePages);
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
                siteProperties.DenyAddAndCustomizePages = AveDenyAddAndCustomizePagesStatus.Enabled;
                //update failed, reset status for reuse;
                logger.Warn("Site: {0} Disable DenyAddAndCustomizePages error {1}", siteUrl.LogBase64(), e.ToString());
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
            logger.Info($"GetLastItemQueryXml:{result.LogBase64()}");
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
            logger.Info($"GetLastFileQueryXml:{result.LogBase64()}");
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

    }

    public class SharePointSettingUtility
    {
        //private ISPSettingTreeService spTreeService = null;
        //private ISharePointSettingDao spSettingsDao = null;

        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(SharePointSettingUtility));
        public SharePointSettingUtility()
        {
            //spTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
            //spSettingsDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
        }

        //public List<RMSharePointSetting> GetAllPhysicalSiteSettings()
        //{
        //    return spSettingsDao.GetAllPhysicalSiteSettings();
        //}
        //public RMSharePointSetting GetPhysicalSiteSetting(Guid gourpId, Guid siteId)//TO DO ylgu debug the scope id to get physical setting
        //{
        //    var setting = spSettingsDao.GetSettingInfoByScope(gourpId, siteId, siteId);
        //    if (setting != null && setting.IsEnableHoldPhyical)
        //    {
        //        return setting;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public List<RMSPTreeNode> GetRegisteredSPSites()
        {
            List<RMSPTreeNode> returnList = new List<RMSPTreeNode>();
            //List<RMSPTreeNode> registeredSites = spTreeService.LoadFarm();
            //var defaultSites = spTreeService.Browse(registeredSites[0]);
            //foreach (var defaultSite in defaultSites)
            //{
            //    returnList.AddRange(spTreeService.Browse(defaultSite));
            //}
            return returnList;
        }

        //public string GetMedataColumn(Guid scopeId)
        //{
        //    return spSettingsDao.GetMedataColumn(scopeId);
        //}

        //public string GetMedataColumn(List<Guid> ids)
        //{
        //    string res = string.Empty;

        //    foreach (var id in ids)
        //    {
        //        if (id != null && !Guid.Equals(Guid.Empty, id))
        //        {
        //            res = spSettingsDao.GetMedataColumn(id);
        //            if (!string.IsNullOrEmpty(res))
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    return res;
        //}

        #region remove
        //public AveBPOSAccountInfo GetBPOSInfo(RMSPTreeNode site)
        //{
        //    if (site.BposInfo == null || site.BposInfo.UserAccountInfo == null)
        //    {
        //        throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site.FullPath));
        //    }
        //    FipsModeUtil.InitControlCryptoMode();
        //    CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
        //    //var key = CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(site.BposInfo.UserAccountInfo.Password));
        //    //string password = "1qaz2wsxE";
        //    string password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(site.BposInfo.UserAccountInfo.Password));
        //    return new AveBPOSAccountInfo()
        //    {
        //        Domain = site.BposInfo.UserAccountInfo.Domain,
        //        UserName = site.BposInfo.UserAccountInfo.Username,
        //        Password = password
        //    };
        //}

        //public AveBPOSAccountInfo GetBPOSInfo(NodeItem site)
        //{
        //    if (site.BposInfo == null || site.BposInfo.UserAccountInfo == null)
        //    {
        //        throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site.FullPath));
        //    }
        //    FipsModeUtil.InitControlCryptoMode();
        //    CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
        //    //var key = CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(site.BposInfo.UserAccountInfo.Password));
        //    string password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(site.BposInfo.UserAccountInfo.Password));
        //    //string password = "1qaz2wsxE";
        //    return new AveBPOSAccountInfo()
        //    {
        //        Domain = site.BposInfo.UserAccountInfo.Domain,
        //        UserName = site.BposInfo.UserAccountInfo.Username,
        //        Password = password
        //    };
        //}

        //public AveBPOSAccountInfo GetBPOSInfo(RemoteSiteCollection site)
        //{
        //    if (string.IsNullOrEmpty(site.username) || string.IsNullOrEmpty(site.password))
        //    {
        //        throw new Exception(string.Format("Get AveBPOSAccountInfo Failed By RemoteSiteCollection, Site Url: {0}.", site.url));
        //    }
        //    FipsModeUtil.InitControlCryptoMode();
        //    CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
        //    //var key = CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(site.password));
        //    string password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(site.password));
        //    //string password = "1qaz2wsxE";
        //    return new AveBPOSAccountInfo()
        //    {
        //        Domain = site.domain,
        //        UserName = site.username,
        //        Password = password
        //    };
        //}
        #endregion

        public AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection GetRemoteSiteCollection(string id)
        {
            RemoteSiteCollection site = null;
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    //site = new DAOAPIClient().GetRemoteSiteCollectionById(id);
                    //site = RABrowserClient.GetRemoteSiteCollectionById(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("GetRemoteSiteCollection Failed,siteid is {0}, message:{1}", id, ex.ToString()));
            }
            return site;
        }

        //public AveBPOSAccountInfo GetBPOSInfo(NodeItem site)
        //{
        //    if (site.BposInfo == null || site.BposInfo.UserAccountInfo == null)
        //    {
        //        throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site.FullPath));
        //    }
        //    FipsModeUtil.InitControlCryptoMode();
        //    var key = CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(site.BposInfo.UserAccountInfo.Password));
        //    return new AveBPOSAccountInfo()
        //    {
        //        Domain = site.BposInfo.UserAccountInfo.Domain,
        //        UserName = site.BposInfo.UserAccountInfo.Username,
        //        Password = Encoding.UTF8.GetString(key)
        //    };
        //}
    }

    public class ReportServiceUtility
    {
        //private IRMReportService reportService = null;

        public ReportServiceUtility()
        {
            //reportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
        }

        //public RMSPTreeNode GetSPFarmTreeNode(string profileId)
        //{
        //    List<RMSPTreeNode> results = new List<RMSPTreeNode>();
        //    RMProfileDto profile = reportService.GetProfileByIdForReportJob(profileId);
        //    return reportService.GetFarmSPTreeNode(profile.Extension2);
        //}
    }
   

    public class TermUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(TermUtility));

        //private IAveTermStore termStore = null;
        //private IAveTermSet termSet = null;
        //private ITermDao TermDao = null;
        public TermUtility(IAveList list, string columnName)
        {
            try
            {
                //IAveTaxonomyField taxonomyField = list.Fields[columnName] as IAveTaxonomyField;
                //IAveTaxonomySession session = list.ParentWeb.Site.AveSPTaxonomySession;
                //int LCID = 0;
                //termStore = AveTaxonomyFieldUtility.GetTermStore(taxonomyField, session, ref LCID);
                //if (termStore == null)
                //{
                //    throw new Exception("Get term store failed.");
                //}
                //if (!taxonomyField.TermSetId.Equals(Guid.Empty))
                //{
                //    termSet = termStore.GetTermSet(taxonomyField.TermSetId);
                //}
                //else
                //{
                //    throw new Exception("Taxonomy field term set id is null");
                //}
                //TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
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
