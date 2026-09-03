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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Service.Services.TemplateManagement;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ReportRelateSettingManagers
{
    public class SharePointOnPremiseReportRelateSettingManager : IReportRelateSettingManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnPremiseReportRelateSettingManager));

        private static readonly ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao = PlatformWindsorManager.GetService<ISharePointOnPremiseSettingDao>();

        private static readonly Dictionary<string, ReportRelateSettingModel> ReportRelateSettingCache = new Dictionary<string, ReportRelateSettingModel>();

        private static readonly Dictionary<int, ManualApprovalSettingModel> SettingCache = new Dictionary<int, ManualApprovalSettingModel>();

        public SourceFlag Flag => SourceFlag.SharePointOnPrem;

        public Task<ManualApprovalSettingModel> GetReportRelateSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo)
        {
            return new InnerReportRelateSettingManager(manualApprovalReportInfo).GetReportRelateSettingAsync();
        }

        class InnerReportRelateSettingManager
        {
            private bool NeedSelectParentLevelNode { get; set; } = false;

            private ManualExportReportInfo ManualApprovalReportInfo { get; set; }

            private ReportRelateSettingModel PrevSettingModel { get; set; } = null;

            private ReportRelateSettingModel CurrentRelateSettingModel { get; set; } = null;

            public InnerReportRelateSettingManager(ManualExportReportInfo manualApprovalReportInfo)
            {
                ManualApprovalReportInfo = manualApprovalReportInfo;
            }

            public async Task<ManualApprovalSettingModel> GetReportRelateSettingAsync()
            {

                var settingKey = ReportRelateSettingModel.GenerateKeyForOnpremise(ManualApprovalReportInfo);
                Logger.Info($"The current manual approval report setting key: [{settingKey}]");

                if (TryGetReportRelateSettingFromCache(settingKey, out var settingId))
                {
                    if (settingId == -1)
                    {
                        return new ManualApprovalSettingModel();
                    }
                    return SettingCache[settingId];
                }
                (var hasSetting, settingId) = await TryGetFolderOrItemNodeSettingAsync();
                if (hasSetting)
                {
                    return SettingCache[settingId];
                }

                if (TryGetListNodeSetting(out settingId))
                {
                    return SettingCache[settingId];
                }
                (hasSetting, settingId) = await TryGetSiteNodeSettingAsync();
                if (hasSetting)
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

            private async Task<(bool,int)> TryGetFolderOrItemNodeSettingAsync()
            {
                int settingId = -1;

                if (ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Folder && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Item)
                {
                    return (false, settingId);
                }

                var folderFullPath = ManualApprovalReportInfo.Path.Substring(0, ManualApprovalReportInfo.Path.LastIndexOf("/"));
                
                var parentFolderIds = await SharePointOnPremClient.GetRootParentIdsFromFolderAsync(ManualApprovalReportInfo.SiteUrl, ManualApprovalReportInfo.WebID.ToString(), ManualApprovalReportInfo.ListID.ToString(), folderFullPath);

                foreach(var folderId in parentFolderIds)
                {
                    var relateSetting = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, ManualApprovalReportInfo.WebID, ManualApprovalReportInfo.ListID, folderId);
                    if (TryGetReportRelateSettingFromDB(relateSetting, item => item.ScopeId == new Guid(folderId) && !item.IsRemoved, out settingId))
                    {
                        return (true, settingId);
                    };
                }

                NeedSelectParentLevelNode = true;
                return (false, settingId);
            }

            private bool TryGetListNodeSetting(out int settingId)
            {
                settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.List)
                {
                    return false;
                }

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, ManualApprovalReportInfo.WebID, ManualApprovalReportInfo.ListID);

                if (TryGetReportRelateSettingFromDB(settingModel, item => item.ScopeId == ManualApprovalReportInfo.ListID && !item.IsRemoved, out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;
                return false;
            }

            private async Task<(bool,int)> TryGetSiteNodeSettingAsync()
            {
                int settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.Site)
                {
                    return (false, settingId);
                }

                var parentWebIds = await SharePointOnPremClient.GetRootParentIdsFromWebAsync(ManualApprovalReportInfo.SiteUrl, ManualApprovalReportInfo.WebID.ToString());
                foreach(var parentWebId in parentWebIds)
                {
                    var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId, new Guid(parentWebId));
                    if(TryGetReportRelateSettingFromDB(settingModel, item => item.ScopeId == new Guid(parentWebId) && !item.IsRemoved, out settingId))
                    {
                        return (true, settingId);
                    }
                }

                NeedSelectParentLevelNode = true;
                return (false, settingId);
            }

            private bool TryGetSiteCollectionNodeSetting(out int settingId)
            {

                settingId = -1;

                if (!NeedSelectParentLevelNode && ManualApprovalReportInfo.ObjectLevel != RMReportObjectLevel.SiteCollection)
                {
                    return false;
                }

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID, ManualApprovalReportInfo.RegistedSiteId);

                if (TryGetReportRelateSettingFromDB(settingModel, item => item.ScopeId == ManualApprovalReportInfo.RegistedSiteId && !item.IsRemoved, out settingId))
                {
                    return true;
                }

                NeedSelectParentLevelNode = true;

                return false;
            }

            private bool TryGetGroupNodeSetting(out int settingId)
            {
                settingId = -1;

                var settingModel = ReportRelateSettingModel.GenerateModel(ManualApprovalReportInfo.SiteGroupID);

                if (TryGetReportRelateSettingFromDB(settingModel, item => item.ScopeId == ManualApprovalReportInfo.SiteGroupID && !item.IsRemoved, out settingId))
                {
                    return true;
                }

                settingModel.SetRoot();
                ReportRelateSettingCache[settingModel.Id] = settingModel;

                return false;
            }

            private bool TryGetReportRelateSettingFromDB(ReportRelateSettingModel settingModel, Expression<Func<RMSharePointOnPremiseSetting, bool>> findSettingCondition, out int settingId)
            {
                settingId = -1;

                PrevSettingModel = CurrentRelateSettingModel;
                CurrentRelateSettingModel = settingModel;

                if (PrevSettingModel != null)
                {
                    PrevSettingModel.SetParentId(CurrentRelateSettingModel.Id);
                    ReportRelateSettingCache[PrevSettingModel.Id] = PrevSettingModel;
                    Logger.Info($"Successful add report relate setting to cache. Key: [{PrevSettingModel.Id}], Parent id: [{CurrentRelateSettingModel.Id}].");
                }

                if(TryGetReportRelateSettingFromCache(settingModel.Id, out settingId))
                {
                    return true;
                }

                var settingInfo = SharePointOnPremiseSettingDao.Find(findSettingCondition);
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

            private void AddSettingInfoToCache(RMSharePointOnPremiseSetting settingInfo)
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
                    manualApprovalSettingInfo.Owners = SharePointOnPremiseSettingDao.GetReocrdOwnersBySettingId(settingInfo.Id);
                }

                SettingCache[settingInfo.Id] = manualApprovalSettingInfo;
                Logger.Info($"Successful add manual approval setting info: [{settingInfo.Id}] to cache.");
            }
        }
    }
}
