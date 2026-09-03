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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ReportRelateSettingManagers
{
    public class SharePointOnlineReportRelateSettingManager : IReportRelateSettingManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnlineReportRelateSettingManager));

        private static readonly ISharePointSettingDao SharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private static readonly Dictionary<string, ReportRelateSettingModel> ReportRelateSettingCache = new Dictionary<string, ReportRelateSettingModel>();

        private static readonly Dictionary<int, ManualApprovalSettingModel> SettingCache = new Dictionary<int, ManualApprovalSettingModel>();

        //private static Guid PrevReportSiteCollectionId = Guid.Empty;

        public SourceFlag Flag => SourceFlag.SharePoint;

        public async Task<ManualApprovalSettingModel> GetReportRelateSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo)
        {
            //if(PrevReportSiteCollectionId != manualApprovalReportInfo.RegistedSiteId)
            //{
            //    ReportRelateSettingCache.Clear();
            //}
            //PrevReportSiteCollectionId = manualApprovalReportInfo.RegistedSiteId;
            return new InnerReportRelateSettingManager(manualApprovalReportInfo).GetReportRelateSetting();
        }

        class InnerReportRelateSettingManager
        {
            private bool NeedSelectParentLevelNode { get; set; } = false;

            private ManualExportReportInfo ManualApprovalReportInfo { get; set; }

            private ReportRelateSettingModel PrevSettingModel { get; set; } = null;

            private ReportRelateSettingModel CurrentRelateSettingModel { get; set; } = null;

            private IAveSite Site { get; set; }

            public InnerReportRelateSettingManager(ManualExportReportInfo manualApprovalReportInfo)
            {
                ManualApprovalReportInfo = manualApprovalReportInfo;
            }

            public ManualApprovalSettingModel GetReportRelateSetting()
            {

                if(ManualApprovalReportInfo.ObjectLevel == RMReportObjectLevel.List)
                {
                    ManualApprovalReportInfo.ListID = ManualApprovalReportInfo.NodeID;
                }

                var settingKey = ReportRelateSettingModel.GenerateKey(ManualApprovalReportInfo);
                Logger.Info($"The current manual approval report setting key: [{settingKey}]");

                if (TryGetReportRelateSettingFromCache(settingKey, out var settingId))
                {
                    if (settingId == -1)
                    {
                        return new ManualApprovalSettingModel();
                    }
                    return SettingCache[settingId];
                }

                if (TryGetFolderOrItemNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }

                if (TryGetListNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }

                if (TryGetSiteNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }

                if (TryGetSiteCollectionNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }

                if (TryGetGroupNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }

                return new ManualApprovalSettingModel();
            }

            private bool TryGetFolderOrItemNodeSetting(out int settingId)
            {
                settingId = -1;

                if (ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Folder && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Item)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(ManualApprovalReportInfo.ServerRelativeUrl) && !ManualApprovalReportInfo.ServerRelativeUrl.StartsWith("/"))
                {
                    ManualApprovalReportInfo.ServerRelativeUrl = "/" + ManualApprovalReportInfo.ServerRelativeUrl;
                }

                var folderRelativeUrl = ManualApprovalReportInfo.ServerRelativeUrl.Contains("\\") ?
                    ManualApprovalReportInfo.ServerRelativeUrl.Substring(0, ManualApprovalReportInfo.ServerRelativeUrl.IndexOf("\\")) :
                    ManualApprovalReportInfo.ServerRelativeUrl;

                var site = GetSite();
                var list = site.OpenWeb(ManualApprovalReportInfo.WebID).GetList(ManualApprovalReportInfo.ListID);
                var folder = list.GetFolder(folderRelativeUrl);
                var folderRootId = list.RootFolder.UniqueId;

                if(!folder.Exists)
                {
                    Logger.Info($"Can't load folder for item: [{ManualApprovalReportInfo.NodeID}].");
                    return false;
                }

                while (folder.UniqueId != Guid.Empty && folder.UniqueId != folderRootId)
                {
                    var relateSetting = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, ManualApprovalReportInfo.WebID, ManualApprovalReportInfo.ListID, folder.ServerRelativeUrl);

                    if (TryGetReportRelateSettingFromDB(relateSetting, item => item.ScopeId == folder.UniqueId && !item.IsRemoved, out settingId))
                    {
                        return true;
                    }

                    folder = folder.ParentFolder;
                }

                var rootFolderRelateSetting = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, ManualApprovalReportInfo.WebID, ManualApprovalReportInfo.ListID, folder.ServerRelativeUrl);
                if(TryGetReportRelateSettingFromDB(rootFolderRelateSetting, item => false, out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;
                return false;
            }

            private bool TryGetListNodeSetting(out int settingId)
            {
                settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.List)
                {
                    return false;
                }

                if (!SharePointDaoMappingManager.TryGetRecordGroupId(ManualApprovalReportInfo, out var recordGroupId) ||
                    !SharePointDaoMappingManager.TryGetRecordSiteCollectionId(ManualApprovalReportInfo, out var recordSiteCollectionId))
                {
                    return false;
                }

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, ManualApprovalReportInfo.WebID, ManualApprovalReportInfo.ListID);

                if (TryGetReportRelateSettingFromDB(settingModel,
                    item => item.SiteGroupId == recordGroupId && item.SiteId == recordSiteCollectionId && item.WebId == ManualApprovalReportInfo.WebID && item.ScopeId == ManualApprovalReportInfo.ListID && !item.IsRemoved,
                    out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;
                return false;
            }

            private bool TryGetSiteNodeSetting(out int settingId)
            {
                settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Site)
                {
                    return false;
                }

                if (!SharePointDaoMappingManager.TryGetRecordGroupId(ManualApprovalReportInfo, out var recordGroupId) ||
                    !SharePointDaoMappingManager.TryGetRecordSiteCollectionId(ManualApprovalReportInfo, out var recordSiteCollectionId))
                {
                    return false;
                }

                var site = GetSite();
                var web = site.OpenWeb(ManualApprovalReportInfo.WebID);
                var rootWebId = site.RootWeb.ID;

                while (web.ID != Guid.Empty && web.ID != rootWebId)
                {
                    var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, web.ID);
                    if (TryGetReportRelateSettingFromDB(settingModel,
                        item => item.SiteGroupId == recordGroupId && item.SiteId == recordSiteCollectionId && item.ScopeId == web.ID,
                        out settingId))
                    {
                        return true;
                    }

                    web = web.ParentWeb;
                }

                var rootSettingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, rootWebId);
                if (TryGetReportRelateSettingFromDB(rootSettingModel,
                        item => item.SiteGroupId == recordGroupId && item.SiteId == recordSiteCollectionId && item.ScopeId == rootWebId && !item.IsRemoved,
                        out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;
                return false;
            }

            private bool TryGetSiteCollectionNodeSetting(out int settingId)
            {

                settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.SiteCollection)
                {
                    return false;
                }

                if (!SharePointDaoMappingManager.TryGetRecordGroupId(ManualApprovalReportInfo, out var recordGroupId) ||
                    !SharePointDaoMappingManager.TryGetRecordSiteCollectionId(ManualApprovalReportInfo, out var recordSiteCollectionId))
                {
                    return false;
                }

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId);

                if (TryGetReportRelateSettingFromDB(settingModel, item => item.SiteGroupId == recordGroupId && item.ScopeId == recordSiteCollectionId && !item.IsRemoved, out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;

                return false;
            }

            private bool TryGetGroupNodeSetting(out int settingId)
            {
                settingId = -1;

                if (!SharePointDaoMappingManager.TryGetRecordGroupId(ManualApprovalReportInfo, out var recordGroupId))
                {
                    return false;
                }

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID);

                if (TryGetReportRelateSettingFromDB(settingModel, item => item.ScopeId == recordGroupId && !item.IsRemoved, out settingId))
                {
                    return true;
                }

                settingModel.SetRoot();
                ReportRelateSettingCache[settingModel.Id] = settingModel;

                return false;
            }

            private bool TryGetReportRelateSettingFromDB(ReportRelateSettingModel settingModel, Expression<Func<RMSharePointSetting, bool>> findSettingCondition, out int settingId)
            {

                PrevSettingModel = CurrentRelateSettingModel;
                CurrentRelateSettingModel = settingModel;

                if (PrevSettingModel != null)
                {
                    PrevSettingModel.SetParentId(CurrentRelateSettingModel.Id);
                    ReportRelateSettingCache[PrevSettingModel.Id] = PrevSettingModel;
                    Logger.Info($"Successful add report relate setting to cache. Key: [{PrevSettingModel.Id}], Parent id: [{CurrentRelateSettingModel.Id}].");
                }

                if(TryGetReportRelateSettingFromCache(CurrentRelateSettingModel.Id, out settingId))
                {
                    return true;
                }

                var settingInfo = SharePointSettingDao.Find(findSettingCondition);
                if (settingInfo != null)
                {
                    AddSettingInfoToCache(settingInfo);
                    settingId = settingInfo.Id;
                    CurrentRelateSettingModel.SetSettingId(settingInfo.Id);
                    ReportRelateSettingCache[CurrentRelateSettingModel.Id] = CurrentRelateSettingModel;

                    Logger.Info($"Successful get report relate setting info: [{settingInfo.Id}], and add to cache. Key: [{CurrentRelateSettingModel.Id}].");
                    return true;
                }

                return false;
            }

            private bool TryGetReportRelateSettingFromCache(string key, out int settingId)
            {
                settingId = -1;

                while (ReportRelateSettingCache.TryGetValue(key, out var relateSettingModel))
                {
                    if (relateSettingModel.HasSetting)
                    {
                        settingId = relateSettingModel.SettingId;
                        return true;
                    }

                    if (relateSettingModel.IsRoot)
                    {
                        return true;
                    }

                    if (!relateSettingModel.HasParent)
                    {
                        break;
                    }

                    key = relateSettingModel.ParentId;
                }

                return false;
            }

            private IAveSite GetSite()
            {
                if (Site == null)
                {
                    if (!SharePointDaoMappingManager.TryGetRecordSiteCollection(ManualApprovalReportInfo, out var recordSiteCollection))
                    {
                        return null;
                    }

                    var factory = MultiAppUtil.CreateAveObjectModelFactory(ManualApprovalReportInfo.SiteUrl, PoolUserUtil.GetAveBPOSAccountInfo(recordSiteCollection.Bpos, ManualApprovalReportInfo.SiteUrl), AveContextKind.ClientObjectModel);
                    Site = factory.CreateSite();
                }

                return Site;
            }

            private void AddSettingInfoToCache(RMSharePointSetting settingInfo)
            {

                if (SettingCache.ContainsKey(settingInfo.Id))
                {
                    return;
                }

                var manualApprovalSettingInfo = new ManualApprovalSettingModel
                {
                    SettingId = settingInfo.Id,
                    IsSendEmialToOwner = settingInfo.EMailToRecordOwner,
                    ManualApprovalType = settingInfo.ApprovalType
                };
                if (settingInfo.ApprovalType == ApprovalType.ApprovalProcess)
                {
                    manualApprovalSettingInfo.WorkflowId = settingInfo.WorkflowReferenceId;
                }
                else if (settingInfo.ApprovalType == ApprovalType.RecordOwners)
                {
                    manualApprovalSettingInfo.Owners = SharePointSettingDao.GetReocrdOwnersBySettingId(settingInfo.Id);
                }

                SettingCache[settingInfo.Id] = manualApprovalSettingInfo;
                Logger.Info($"Successful add manual approval setting info: [{settingInfo.Id}] to cache.");
            }
        }
    }
}
