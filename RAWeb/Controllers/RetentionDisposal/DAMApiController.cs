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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecordManager.Controllers.DisposalActivity
{
    [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMPermissionExtensionMasks.TeamsEndUser, PermissionJoinType.Any, preferred: false)]
    public class DAMApiController : BaseApiController
    {
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        private IGlobalSettingService _GlobalSettingService;
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService(ref _GlobalSettingService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private IRMJobService _RMJobService;
        private IRMJobService RMJobService => PlatformWindsorManager.GetService(ref _RMJobService);
        private IRMSharePointSettingsService _RMSPSettingsService;
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);

        private IRMTeamsSettingsService _teamsSettingsService;
        private IRMTeamsSettingsService RMTeamsSettingsService => PlatformWindsorManager.GetService(ref _teamsSettingsService);

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser,RMPermissionExtensionMasks.TeamsEndUser , RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser | RMSOPermissionMasks.TeamsEndUser, AvePoint.RA.DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public string GetSPDesignLists()
        {
            //获取配置文件中填写的design lists(前台SPTree需要过滤这部分节点)
            var lists = RMSPSettingsService.GetDesignLists();
            return JsonConvert.SerializeObject(lists);
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser, AvePoint.RA.DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public string GetSPTreeInitData()
        {
            var farmNode = SPSettingTreeService.LoadFarm()[0];
            if (farmNode == null || string.IsNullOrEmpty(farmNode.Id))
            {
                Logger.Warn("Farm node is null.Please refresh page.");
            }
            else
            {
                if (farmNode.Children != null)
                {
                    //删除Children属性，避免以后convert to SPTree时出现死循环
                    farmNode.Children = null;
                }
            }
            return JsonConvert.SerializeObject(farmNode);
        }

        [HttpPost]
        [ValidSPTreeParameterFilter]
        public async Task<string> Browse([FromBody] RMSPTreeNode node)
        {
            string result = string.Empty;
            string spObjectId = string.Empty;
            try
            {
                //获取子节点
                List<RMSPTreeNode> children = new List<RMSPTreeNode>();
                spObjectId = node.SPObjectId;
                //var curRMNode = SPTreeCacheUtil.GetNodeById(spObjectId, RAModule.Common);
                if (string.IsNullOrEmpty(node.Id))
                {
                    var farmNodes = SPSettingTreeService.LoadFarm();
                    children = await SPSettingTreeService.BrowseAsync(farmNodes[0], true);
                }
                else
                {
                    children = await SPSettingTreeService.BrowseAsync(node, true);
                }
                //这个值前台目前是写死的，所以这个IF逻辑是一定会走的
                if (node.NeedLoadSchedule)
                {
                    await RMSPSettingsService.LoadScheduleAsync(children);
                }

                RMSPSettingsService.CheckIsContainScheduleForOwnAndChildNodes(children);

                List<RMSPTreeNode> foregroundNodes = new List<RMSPTreeNode>();

                foreach (var child in children)
                {
                    if (child.BposInfo != null && child.BposInfo.UserAccountInfo != null)
                    {
                        var accountInfo = child.BposInfo.UserAccountInfo;
                        accountInfo.Domain = string.Empty;
                        accountInfo.Username = string.Empty;
                        accountInfo.AppId = string.Empty;
                        accountInfo.AppClientId = string.Empty;
                        accountInfo.AppCertSecret = string.Empty;
                        //accountInfo.AppCertContent = string.Empty;
                        accountInfo.AppCertSecretContent = string.Empty;
                    }
                    
                    if (!child.Hidden)
                    {
                        if (child.Children != null)
                        {
                            //删除Children属性，避免以后convert to SPTree时出现死循环
                            child.Children = null;
                        }
                        //缓存节点
                        //SPTreeCacheUtil.CacheNode(child, RAModule.Common);
                        ////删除不必要属性，减少序列化以及通信的size
                        //RMSPTreeNode tempChild = child.Clone();
                        //tempChild.ParentId = tempChild.Parent.Id;
                        //tempChild.Parent = null;
                        //tempChild.Children = null;
                        //foregroundNodes.Add(tempChild);
                        if (child.Parent != null)
                        {
                            child.ParentId = child.Parent.Id;
                            child.Parent = null;
                        }
                        foregroundNodes.Add(child);
                    }
                }

                result = JsonConvert.SerializeObject(foregroundNodes);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe node.NodeSPObjectId:[{0}] Error:{1}", spObjectId, e.ToString());
                throw;
            }
            return result;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidSPTreeParameterFilter]
        public async Task<RAReturnMessage> RunJob([FromBody] string node)
        {
            RMSPTreeNode selectedNode = null;
            try
            {
                selectedNode = SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(node);
                //var selectedNode = SPTreeCacheUtil.GetNodeById(spObjectId, RAModule.Common);
                Logger.Info("Run job Node FullPath:[{0}]", selectedNode?.FullPath);
                if (TenantService.IsCSDTenant())
                {
                    return RMJobService.RunDeclaredOnly(selectedNode, JobRunBy.Control);
                }
                else if (TenantService.IsNewOpusTenant())
                {
                    return RMSPSettingsService.RunRecordsDisposalJob(selectedNode, JobRunBy.Control);
                }
                else
                {
                    return await RMJobService.RunNowAsync(selectedNode, JobRunBy.Control);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run job. NodeSPObjectId:[{0}] Error:{1}", selectedNode?.SPObjectId, e.ToString());
                throw;

            }
        }

        [HttpPost]
        //[ValidSPTreeParameterFilter]
        public async Task<RAReturnMessage> RunTeamsJob([FromBody] string node)
        {
            RMSPTreeNode selectedNode = null;
            try
            {
                selectedNode = SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(node);
                //var selectedNode = SPTreeCacheUtil.GetNodeById(spObjectId, RAModule.Common);
                Logger.Info("Run job Node FullPath:[{0}]", selectedNode?.FullPath);
                if (TenantService.IsNewOpusTenant())
                {
                    return RMTeamsSettingsService.RunRecordsDisposalJob(selectedNode, JobRunBy.Control);
                }
                else
                {
                    return new RAReturnMessage() {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_JS_Common_NoPermissionLicense"),
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run job. NodeSPObjectId:[{0}] Error:{1}", selectedNode?.SPObjectId, e.ToString());
                throw;

            }
        }


        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public ValidationMessage ValidateDAConnectionSetting()
        {
            return GlobalSettingService.CheckDocAveConnectionSetting();
        }

        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser, RMSOPermissionMasks.ContentRepositoyEnduser)]
        public async Task<ResultType> ValidateCommonSettings()
        {
            ResultType result = ResultType.Nothing;
            try
            {
                //var tempDAConConfigured = mSPSettingTreeService.ValidateDocAveConnectionSetting();
                var tempStorageSettingConfigured = await SPSettingTreeService.ValidateGlobalStorageSettingAsync();
               
                if (!tempStorageSettingConfigured)
                {
                    result = ResultType.NoGlobalStorageSetting;
                }
                else
                {
                    result = ResultType.AllCorrect;
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when validate DocAve connection and global storage setting.Error:{0}"
                    ,e.ToString());
                throw;
            }
            return result;
        }

        [HttpPost]
        public Task<string> SaveDisposalJobScheduleSetting([FromBody]string schedule)
        {
            var Scheduleinfo = SerializerHelper.DeserializeByJsonConvert<ScheduleInfo>(schedule);
            Scheduleinfo.Id = Guid.NewGuid().ToString();
            return ScheduleService.CreateScheduleServiceAsync(Scheduleinfo);
        }

        [HttpPost]
        public Task<string> UpdateScheduleService([FromBody]string schedule)
        {
            var Scheduleinfo = SerializerHelper.DeserializeByJsonConvert<ScheduleInfo>(schedule);
            return ScheduleService.UpdateScheduleServiceAsync(Scheduleinfo);
        }

        [HttpPost]
        public Task<ScheduleInfo> GetScheduleByProfileId([FromBody] string profileId)
        {
            return ScheduleService.GetScheduleByProfileIdAsync(profileId);
        }


        [HttpPost]
        public void DeleteScheduleService([FromBody]string Id)
        {
            ScheduleService.DeleteScheduleService(Id);
        }

        public enum ResultType
        {
            //DocAve Connection和Global Storage Setting都设置了
            AllCorrect = 0,
            NoDocAveConnection=1,
            NoGlobalStorageSetting=2,
            //DocAve Connection和Global Storage Setting都没设置
            Nothing=3
        }
    }
}
