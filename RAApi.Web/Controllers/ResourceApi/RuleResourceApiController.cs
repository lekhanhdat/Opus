using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Controllers.GoogleOne;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/RuleApi/[action]")]
    [ApiController]
    public class RuleResourceApiController : RAWebApiBase
    {
        private readonly IRALogger _logger = new RALogger(typeof(RuleResourceApiController));

        private IRuleContainerService RuleContainerService => PlatformWindsorManager.GetService<IRuleContainerService>();
        private IRuleManagerService RuleService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();


        [HttpPost]
        public string SaveRuleContainer([FromBody] RuleContainerDto dto)
        {
            try
            {
                ValidateReplicaRuleContainerRequest(dto);

                if (dto.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID)
                {
                    return "Can Not Edit/Delete Default Container";
                }
                if (dto.Name == I18NEntity.GetString("RM_RDM_DefaultRuleContainer"))
                {
                    return "Can Not Create Default Container";
                }
                RuleContainerDto result = null;
                if (dto.IsCreateOperation)
                {
                    result = RuleContainerService.CreateRuleContainer(dto);
                }
                else
                {
                    result = RuleContainerService.EditRuleContainer(dto);
                }
                return result != null ? JsonConvert.SerializeObject(result) : string.Empty;
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while saving rule container. Error: {0}", e.ToString());
                throw;
            }
        }

        [HttpPost]
        public string DeleteRuleContainer([FromBody] Guid containerId)
        {
            try
            {
                if (containerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID)
                {
                    return "Can Not Edit/Delete Default Container";
                }
                return JsonConvert.SerializeObject(RuleContainerService.DeleteRuleContainer(containerId));
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while deleting rule container. ContainerId: {0}, Error: {1}", containerId, e.ToString());
                throw;
            }
        }

        [HttpPost]
        public async Task<string> CreateRule([FromBody] RMRuleInfos ruleInfo)
        {
            ValidateReplicaCreateRuleRequest(ruleInfo);

            try
            {
                RAReturnMessage result = await RuleService.CreateRuleInDAAsync(ruleInfo);
                if (result.MessageType == RAMessageType.Failed)
                {
                    _logger.Error("create rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, ruleInfo.RuleName);
                    if (TenantService.IsNewOpusTenant())
                    {
                        return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), I18NEntity.GetString(result.ErrorMessage));
                    }
                    else
                    {
                        return string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), I18NEntity.GetString(result.ErrorMessage));
                    }
                }
                else
                {
                    await RuleService.BuildManualAprovalJobScheduleForCreateRule(ruleInfo);
                }
                _logger.Info("create rule success,RuleName:{0}.", ruleInfo.RuleName);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error("an error occurred while create rule(c),rule name:{1},ERROR:{0}", ex.ToString(), ruleInfo.RuleName);
                throw;
            }
        }

        [HttpPost]
        public async Task<string> EditRule([FromBody] RMRuleInfos ruleInfo)
        {
            try
            {
                RAReturnMessage result = await RuleService.ModifyRuleInDAAsync(ruleInfo);
                if (result.MessageType == RAMessageType.Failed)
                {
                    _logger.Info("edit rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, ruleInfo.RuleName);
                    if (TenantService.IsNewOpusTenant())
                    {
                        return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), I18NEntity.GetString(result.ErrorMessage));
                    }
                    else
                    {
                        return string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), I18NEntity.GetString(result.ErrorMessage));
                    }
                }
                else
                {
                    //enable manual approval, create manula approval job schedule
                    await RuleService.BuildManualAprovalJobScheduleForEditRule(ruleInfo);

                }
                _logger.Info("edit rule success.RuleName:{0},RuleId:{1}", ruleInfo.RuleName, ruleInfo.RuleId);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error("an error occurred while edit rule(c),rule name:{1}, ERROR:{0}", ex.ToString(), ruleInfo.RuleName);
                throw;
            }
        }

        [HttpPost]
        public async Task<string> DeleteRules(List<string> ruleIds)
        {
            try
            {
                RAReturnMessage result = await RuleService.DeleteRulesAsync(ruleIds);
                if (result.MessageType == RAMessageType.Failed)
                {
                    return result.ErrorMessage;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while deleting rules. Error: {0}", ex.ToString());
                throw;
            }
        }

        private static void ValidateReplicaRuleContainerRequest(RuleContainerDto dto)
        {
            if (dto == null)
            {
                throw new InvalidOperationException("Rule container payload is required for replica requests.");
            }

            if (dto.IsCreateOperation && dto.ContainerId == Guid.Empty)
            {
                throw new InvalidOperationException("Rule container id is required for replica create requests.");
            }
        }

        private static void ValidateReplicaCreateRuleRequest(RMRuleInfos ruleInfo)
        {
            if (ruleInfo == null || string.IsNullOrWhiteSpace(ruleInfo.RuleId))
            {
                throw new InvalidOperationException("Rule id is required for replica create requests.");
            }
        }


    }
}
