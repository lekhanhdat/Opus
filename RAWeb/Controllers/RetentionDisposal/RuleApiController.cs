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

using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Contract.RMWeb.Rule;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Web.Common.Filters.RuleApiFilter;
using AvePoint.Common.Portal;
using AvePoint.RA.Common.Aos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System.Threading.Tasks;
using Cloud.Sdk.CloudInsights;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Services;

namespace AvePoint.RA.Web.Controllers.RetentionDisposal
{
    
    public class RuleApiController : BaseApiController
    {
        private RALogger logger = RALogger.GetInstance(typeof(RuleApiController));
        private IRuleManagerService _RuleService;
        private IRuleManagerService RuleService => PlatformWindsorManager.GetService(ref _RuleService);

        private IGlobalSettingService _GlobalSettingService;
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService(ref _GlobalSettingService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);
        private IRuleContainerService _RuleContainerService;
        private IRuleContainerService RuleContainerService => PlatformWindsorManager.GetService(ref _RuleContainerService);
      

        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);

        public Guid RuleContainerRoot = new Guid("01f4b11e-12b0-4c08-8309-9f53064281d4");
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMCacheManager RMCacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();

        /// <summary>
        /// 此处考虑到rule的数量级不大，采用前台分页,按modified时间倒序排列，取得信息还直接用于View Detail
        /// </summary>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public async Task<string> GetRuleDatas()
        {
            List<RMRuleInfos> rules = new List<RMRuleInfos>();
            try
            {
                using (PerformanceScope scope = new PerformanceScope("Outer get all rules"))
                {
                    //rules = RuleService.GetRuleInfosFromDA();
                    var allRuleContainers = await RuleContainerService.GetAllRuleContainersAsync();
                    rules = await RuleService.GetSimpleRulesFromDBAsync(allRuleContainers.Select(c => c.ContainerId).ToList());
                    rules = rules.Where(o => o.ModelType == RuleModel.Records || o.ModelType == RuleModel.None).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return JsonConvert.SerializeObject(rules);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.RuleManagementEnduser)]
        [ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType.RuleParameter)]
        public async Task<string> GetSearchRuleDatas([FromBody] RuleParameter ruleParameter)
        {
            List<RMRuleInfos> rules = new List<RMRuleInfos>();
            try
            {
                using (PerformanceScope scope = new PerformanceScope("Outer get all rules"))
                {
                    rules = await RuleService.GetSearchRuleFromDBAsync(ruleParameter);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return JsonConvert.SerializeObject(rules);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public bool CheckIsCSDTenant()
        {
            return TenantService.IsCSDTenant();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public string GetRulesFromDB()
        {
            List<RuleDto> rules = RuleService.GetBaseRulesFromDB();
            //rules = rules.OrderByDescending(rule => DateTime.Parse(rule.Modified)).ToList();
            return JsonConvert.SerializeObject(rules);
        }

        /// <summary>
        /// edit rule时先取rule用于回显
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        [ValidateRuleIdPermissionFilter]
        public async Task<RAReturnMessage> GetRuleByID([FromBody] string ruleId)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                Logger.Info("get rule by id normal,id:{0}", ruleId);
                RMRuleInfos rule = await RuleService.LoadRuleAsync(ruleId);
                result.Extension = JsonConvert.SerializeObject(rule);
            }
            catch (Exception ex)
            {
                var exMsg = $"get rule by id faild,id:{ruleId},msg:{ex.Message}";
                Logger.Info(exMsg);
                result.MessageType = RAMessageType.Exception;
                result.ErrorMessage = exMsg;
            }
            return result;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        [ValidCreateRuleParameterFilter]
        [ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType.RMRuleInfos)]
        public async Task<string> CreateRule([FromBody] RMRuleInfos ruleInfo)
        {
            return await RouteMultiGeoApiActionAsync(
                ruleInfo,
                MultiGeoOperationType.CreateRule,
                async request =>
                {
                    try
                    {
                        RAReturnMessage result = await RuleService.CreateRuleInDAAsync(request);
                        if (result.MessageType == RAMessageType.Failed)
                        {
                            Logger.Error("create rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, request.RuleName);
                            if (TenantService.IsNewOpusTenant())
                            {
                                return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), I18NEntity.GetString(result.ErrorMessage));
                            }

                            return string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), I18NEntity.GetString(result.ErrorMessage));
                        }

                        await RuleService.BuildManualAprovalJobScheduleForCreateRule(request);
                        Logger.Info("create rule success,RuleName:{0}.", request.RuleName);
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("an error occurred while create rule(c),rule name:{1},ERROR:{0}", ex.ToString(), request.RuleName);
                        return string.Format(I18NEntity.GetString("RM_JS_RDM_CreateRule_MessageInfo_Faild"), I18NEntity.GetString(ex.Message));
                    }
                },
                _ => I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage"));
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        [ValidCreateRuleParameterFilter]
        [ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType.RMRuleInfos)]
        public async Task<string> EditRule([FromBody] RMRuleInfos ruleInfo)
        {
            return await RouteMultiGeoApiActionAsync(
                ruleInfo,
                MultiGeoOperationType.EditRule,
                async request =>
                {
                    try
                    {
                        RAReturnMessage result = await RuleService.ModifyRuleInDAAsync(request);
                        if (result.MessageType == RAMessageType.Failed)
                        {
                            Logger.Info("edit rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, request.RuleName);
                            if (TenantService.IsNewOpusTenant())
                            {
                                return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), I18NEntity.GetString(result.ErrorMessage));
                            }

                            return string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), I18NEntity.GetString(result.ErrorMessage));
                        }

                        await RuleService.BuildManualAprovalJobScheduleForEditRule(request);
                        Logger.Info("edit rule success.RuleName:{0},RuleId:{1}", request.RuleName, request.RuleId);
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("an error occurred while edit rule(c),rule name:{1}, ERROR:{0}", ex.ToString(), request.RuleName);
                        return string.Format(I18NEntity.GetString("RM_JS_RDM_EditRule_MessageInfo_Faild"), I18NEntity.GetString(ex.Message));
                    }
                },
                _ => I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage"));
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        [ValidateRuleIdsPermissionFilter]
        public async Task<string> DeleteRules([FromBody] List<string> ruleIds)
        {
            return await RouteMultiGeoApiActionAsync(
                ruleIds,
                MultiGeoOperationType.DeleteRules,
                async request =>
                {
                    try
                    {
                        Logger.Info("begin to delete rule(s).");
                        RAReturnMessage result = await RuleService.DeleteRulesAsync(request);
                        if (result.MessageType == RAMessageType.Failed)
                        {
                            Logger.Info("delete rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, request?.FirstOrDefault());
                            return result.ErrorMessage;
                        }

                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("an error occurred while delete rules,ERROR:{0}", ex.ToString());
                        throw;
                    }
                },
                _ => I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage"));
        }

        /// <summary>
        /// 删除rule的时候需要提示关联的term有哪些，用到此方法
        /// </summary>
        /// <param name="ruleInfos"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public string GetAssociateTerms([FromBody] List<RMRuleTermInfos> ruleInfos)
        {
            try
            {
                var terms = RuleService.GetRuleTermInfos(ruleInfos);
                return JsonConvert.SerializeObject(terms);
            }
            catch (Exception ex)
            {
                Logger.Error("an error occurred while get terms info,ERROR:{0}", ex.ToString());
                throw ex;
            }
        }
        //[RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        //public ValidationMessage validateDocAveConnAndGlobalStorageSetting()
        //{
        //    return GlobalSettingService.CheckDocAveConnectionGlobalStorageSetting();
        //}

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public ValidationMessage validateExportSetting(int sourceFlag)
        {
            return GlobalSettingService.CheckExportSetting(ValidationType.ExportSetting, sourceFlag);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public ValidationMessage validateNaaExportSetting(int sourceFlag)
        {
            return GlobalSettingService.CheckExportSetting(ValidationType.NNAExportSetting, sourceFlag);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public ValidationMessage validateNaraExportSetting(int sourceFlag)
        {
            return GlobalSettingService.CheckExportSetting(ValidationType.NARAExportSetting, sourceFlag);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public string GetAllWorkflows()
        {
            var workflows = ManualProcessManagementService.GetAllSimpleProcesses();
            return JsonConvert.SerializeObject(workflows);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetChildrenByDB([FromBody] RuleContainerQuery query)
        {
            List<RuleContainerDto> result = new List<RuleContainerDto>();
            (var subNodes,var count) = await RuleContainerService.GetRuleContainersAsync(query);
            var rootNode = new RuleContainerDto()
            {
                NodeType = RMNodeLevel.RuleContainerRoot,
                ContainerId = RuleContainerRoot,
                Name = I18NEntity.GetString("RM_RDM_RuleContainerRoot"),
                RuleContainerList = subNodes,
                TotalCount = count
            };
            result.Add(rootNode);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]

        [ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType.RuleContainerDto)]
        public async Task<string> SaveRuleContainer([FromBody] RuleContainerDto dto)
        {
            if (dto == null)
            {
                return string.Empty;
            }

            dto.IsCreateOperation = dto.ContainerId == Guid.Empty;
            if (dto.IsCreateOperation)
            {
                dto.ContainerId = Guid.NewGuid();
            }

            if (dto.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID)
            {
                return "Can Not Edit/Delete Default Container";
            }

            if (dto.Name == I18NEntity.GetString("RM_RDM_DefaultRuleContainer"))
            {
                return "Can Not Create Default Container";
            }

            return await RouteMultiGeoApiActionAsync(
                dto,
                dto.IsCreateOperation ? MultiGeoOperationType.CreateRuleContainer : MultiGeoOperationType.EditRuleContainer,
                request =>
                {
                    try
                    {
                        RuleContainerDto result = request.IsCreateOperation
                            ? RuleContainerService.CreateRuleContainer(request)
                            : RuleContainerService.EditRuleContainer(request);

                        return Task.FromResult(result != null ? JsonConvert.SerializeObject(result) : string.Empty);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"{e}");
                        return Task.FromResult(string.Empty);
                    }
                },
                _ => "-2");
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        [ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType.ContainerId)]
        public async Task<RAReturnMessage> DeleteRuleContainer([FromBody] Guid containerId)
        {
            return await RouteMultiGeoApiActionAsync(
                containerId,
                MultiGeoOperationType.DeleteRuleContainer,
                request =>
                {
                    try
                    {
                        if (request == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID)
                        {
                            return Task.FromResult(new RAReturnMessage
                            {
                                MessageType = RAMessageType.Failed,
                                ErrorMessage = "Can Not Edit/Delete Default Container"
                            });
                        }

                        return Task.FromResult(RuleContainerService.DeleteRuleContainer(request));
                    }
                    catch (Exception e)
                    {
                        logger.Error($"{e}");
                        return Task.FromResult(new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = string.Empty
                        });
                    }
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.FailedWithEx,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetAllRuleContainers()
        {
            try
            {
                return JsonConvert.SerializeObject(await RuleContainerService.GetAllRuleContainersAsync());
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public string GetRuleContainersByTermId(int termId)
        {
            try
            {
                return JsonConvert.SerializeObject(RuleContainerService.GetRuleContainersByTermId(termId));
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public string GetRuleContainersForLabel()
        {
            try
            {
                return JsonConvert.SerializeObject(RuleContainerService.GetRuleContainersForLabel());
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public string GetRuleContainersByContainerId(string containerId, int sourceFlag)
        {
            try
            {
                return JsonConvert.SerializeObject(RuleContainerService.GetRuleContainersByContainerId(containerId, sourceFlag));
            }
            catch (Exception ex)
            {
                logger.Error($"An error while GetRuleContainersByContainerId, message:{ex}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetCanCopyRulesByTermId(int termId,int moduleType)
        {
            try
            {
                return JsonConvert.SerializeObject(await RuleService.GetCanCopyRulesByTermIdAsync(termId, moduleType));
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetCanCopyRulesForDisableClassification(int moduleType)
        {
            try
            {
                return JsonConvert.SerializeObject(await RuleService.GetCanCopyRulesForDisableClassificationAsync(moduleType));
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public string CheckContainerCrossSecurityGroup(string oldContainerId, string newContainerId, string ruleId)
        {
            try
            {
                return JsonConvert.SerializeObject(RuleContainerService.CheckContainerCrossSecurityGroup(oldContainerId, newContainerId, ruleId));
            }
            catch (Exception e)
            {
                logger.Error($"{e}");
                return string.Empty;
            }
        }
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public bool HaveRecenter()
        {
            return !string.IsNullOrEmpty(RMAosApiClient.GetRECENTERServiceUrl(TenantLocalValue.LogonGroupId));
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public bool CheckHaveRecenter()
        {
            try
            {
                return HaveRecenter();
            }
            catch (Exception e)
            {
                logger.Error($"Error Check recenter, {e}");
                return false;
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser)]
        public bool CheckIsNestleCustomize()
        {
            return RMKeyValueDao.GetIsNestleCustomize();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public async Task<string> GetLATEnableTime()
        {
            return await RMCacheManager.Cache.TryGetAsync("GET_LAT_ENABLE_TIME", GetLATEnableTimeAsync, TimeSpan.FromMinutes(15));
        }

        private async Task<string> GetLATEnableTimeAsync()
        {
            try
            {
                string cloudInsightsApiUrl = GCommon.Utility.Cloud.GCommonRoleConfiguration.PortalCloudInsightsApiURL;
#if DEBUG
                cloudInsightsApiUrl = "https://graph.sharepointguild.com/cloudinsights";
#endif
                ISettingsService settingsService = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(cloudInsightsApiUrl, TenantLocalValue.LogonGroupId.ToString()).SettingsService;
                var collectionSetting = await settingsService.GetCollectionSetting();
                if (collectionSetting == null)
                {
                    logger.Info($"[GetLATEnableTime] Not enable collection");
                    return string.Empty;
                }
                var collectionEnabletime = await settingsService.GetLatEnableTime();//create rule enable collection time
                if (collectionEnabletime == DateTime.MinValue)
                {
                    logger.Info("[GetLATEnableTime] There may be old account that don't need to be collected separately");
                    var featureEnabletime = await settingsService.GetEnableTime();//AOS enable feature time
                    logger.Info($"[GetLATEnableTime] Enable feature in AOS time is: {featureEnabletime}");
                    if (featureEnabletime < new DateTime(2023, 10, 8))
                    {
                        collectionEnabletime = featureEnabletime;
                        logger.Info($"[GetLATEnableTime] Is directly open the collection");
                    }
                }
                else
                {
                    logger.Info($"[GetLATEnableTime] Enable collection time is: {collectionEnabletime}");
                }

                if (collectionEnabletime == DateTime.MinValue)
                {
                    return string.Empty;
                }
                else
                {
                    var datetimeFormat = I18NEntity.GetString("RM_RDM_CR_LastAccessTime_DateFormat");
                    var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
                    return GeneralSettingService.ConvertTiksToDateTime(gls, collectionEnabletime.Ticks, true).DataTime.ToString(datetimeFormat);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"GetLATEnableTime error, {e}");
                return string.Empty;
            }
        }

        [HttpGet]
        public async Task<string> GetRecordLabel()
        {
            var setting = await GeneralSettingService.GetGeneralSettingAsync();
            if (setting != null)
            {
                return setting.RecordsLabel;
            }
            return string.Empty;
        }
    }
}
