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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RuleManagement.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Explorer;
using static AvePoint.RA.Contract.RMWeb.CP.SecurityRuleInfo;

namespace AvePoint.RA.Service.Services.RuleManagement
{
    [Audit]
    public class RuleContainerService : RMServiceBase, IRuleContainerService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RuleContainerService));
        #region Interface
        protected IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        #endregion

        public async Task<(List<RuleContainerDto>,int)> GetRuleContainersAsync(RuleContainerQuery query)
        {
            var ruleContainers = await SecurityTrimmingHelper.GetRuleScopeAsync();
            int totalCount = RMRuleDao.GetRuleContainersCount(query.SearchKey, ruleContainers);
            var countMapping = RMRuleDao.GetRuleContainersMapping(ruleContainers);
            return (RMRuleDao.GetRuleContainersByPager(query, ruleContainers).ConvertAll(c => ConvertToDto(c, countMapping)),totalCount);
        }
        public async Task<List<RuleContainerDto>> GetAllRuleContainersAsync()
        {
            var ruleContainers = await SecurityTrimmingHelper.GetRuleScopeAsync();
            return RMRuleDao.GetAllRuleContainers(ruleContainers).ConvertAll(c => ConvertToDto(c));
        }

        public List<RuleContainerDto> GetRuleContainersByTermId(int termId)
        {
            var scopeRuleContainers = SecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termId.ToString());
            return RMRuleDao.GetAllRuleContainers(scopeRuleContainers).ConvertAll(c => ConvertToDto(c));
        }

        public List<RuleContainerDto> GetRuleContainersForLabel()
        {
            return RMRuleDao.GetAllRuleContainers().ConvertAll(c => ConvertToDto(c));
        }

        public List<RuleContainerDto> GetRuleContainersByIds(List<Guid> conteinerIds)
        {
            return RMRuleDao.GetAllRuleContainers(conteinerIds).ConvertAll(c => ConvertToDto(c));
        }

        private static RuleContainerDto ConvertToDto(RMRuleContainer ruleContainer, Dictionary<Guid, int> countMapping = null)
        {
            var result = new RuleContainerDto()
            {
                NodeType = RMNodeLevel.RuleContainer,
                ContainerId = ruleContainer.ContainerId,
                Name = I18NEntity.GetString(ruleContainer.Name),
                IsDefault = ruleContainer.IsDefault,

            };
            if (countMapping != null)
            {
                if (countMapping.TryGetValue(ruleContainer.ContainerId, out int count))
                {
                    result.SubTermCount = count;
                }
            }
            return result;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.CreateRuleContainer, BeforeHandler = typeof(RuleManagerBeforeAuditHandler), AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public RuleContainerDto CreateRuleContainer(RuleContainerDto ruleContainer)
        {
            return InnerSaveRuleContainer(ruleContainer);
        }
        
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.EditRuleContainer, BeforeHandler = typeof(RuleManagerBeforeAuditHandler), AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public RuleContainerDto EditRuleContainer(RuleContainerDto ruleContainer)
        {
            return InnerSaveRuleContainer(ruleContainer);
        }

        private RuleContainerDto InnerSaveRuleContainer(RuleContainerDto ruleContainer)
        {
            ruleContainer.Name = ruleContainer.Name.Trim();
            if (ruleContainer.Name == string.Empty)
            {
                logger.Error($"Save rule container, name is EMPTY");
                return null;
            }
            if (RMRuleDao.CheckRuleContainerNameExist(ruleContainer.Name))
            {
                logger.Error($"Save rule container, name exist {ruleContainer.Name}");
                //throw new SameNameExistException();
                return null;
            }
            if (ruleContainer.ContainerId == Guid.Empty)
            {
                ruleContainer.ContainerId = Guid.NewGuid();
            }
            return ConvertToDto(RMRuleDao.UpsertRuleContainer(new DB.Model.RMRuleContainer()
            {
                ContainerId = ruleContainer.ContainerId,
                Name = ruleContainer.Name,
                IsDefault = ruleContainer.IsDefault,
                ModifyTime = DateTime.UtcNow.Ticks
            }));
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.DeleteRuleContainer, BeforeHandler = typeof(RuleManagerBeforeAuditHandler), AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public RAReturnMessage DeleteRuleContainer(Guid containerId)
        {
            RAReturnMessage result = new RAReturnMessage();
            var allRules = RMRuleDao.GetAvailableRules(new List<Guid>() { containerId });
            if (allRules.Count > 0)
            {
                result.MessageType = RAMessageType.Failed;
                return result;
            }

            var success = RMRuleDao.DeleteRuleContainer(containerId);
            result.MessageType = success ? RAMessageType.Successful : RAMessageType.Failed;
            return result;
        }

        public string GetRuleTreeForSecurityGroup(QueryRuleObjDto queryDto)
        {
            var result = "";
            var mapped = RMSecurityGroupDao.GetMappedRuleByOtherGroups(queryDto.GroupId);
            var isExistMappedAll = mapped.Any(m => m.Level == SecurityRuleLevel.All);
            if (isExistMappedAll)
            {
                return result;
            }
            var mappedRuleContainers = mapped.Where(m => m.Level == SecurityRuleLevel.RuleContainer).Select(m => m.RuleObjId);
            var mappedRules = mapped.Where(m => m.Level == SecurityRuleLevel.Rule).Select(m => m.RuleObjId);
            switch (queryDto.ParentType) {
                case RMRuleType.Root:
                    List<SecurityRuleInfo> ruleContainerItmes = new List<SecurityRuleInfo>();
                    var allContainers = RMRuleDao.GetAllRuleContainers();
                    allContainers = allContainers.Where(c => !mappedRuleContainers.Contains(c.ContainerId)).ToList();
                    foreach (var dbRuleContainer in allContainers)
                    {
                        var ruleCon = Convert2SecurityRuleInfo(dbRuleContainer);
                        var dbRules = RMRuleDao.GetAvailableRules(new List<Guid> { dbRuleContainer.ContainerId });
                        dbRules = dbRules.Where(g => !mappedRules.Contains(g.RuleId)).ToList();
                        var rules = dbRules.ConvertAll(r => Convert2SecurityRuleInfo(r, dbRuleContainer.ContainerId));
                        if (rules.Count > 0)
                        {
                            ruleCon.SubItems = rules;
                            ruleCon.SubItemCount = rules.Count;
                            ruleCon.IsLoaded = true;
                        }
                        ruleCon.SubPerSize = 10;
                        ruleContainerItmes.Add(ruleCon);
                    }
                    result = JsonConvert.SerializeObject(new QueryRuleObjResultDto
                    {
                        TermObjItems = ruleContainerItmes
                    });
                    break;
            }
            return result;
        }

        public RAReturnMessage CheckContainerCrossSecurityGroup(string oldContainerId, string newContainerId, string ruleId)
        {
            return RMRuleDao.CheckContainerCrossSecurityGroup(new Guid(oldContainerId), new Guid(newContainerId), ruleId);
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRuleContainer ruleContainer)
        {
            return new SecurityRuleInfo
            {
                Id = ruleContainer.Id,
                UniqueId = ruleContainer.ContainerId,
                ParentId = Guid.Empty,
                Name = I18NEntity.GetString(ruleContainer.Name),
                Type = RMRuleType.RuleContainer,
            };
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRule rule, Guid parentId)
        {
            return new SecurityRuleInfo
            {
                Id = rule.Id,
                UniqueId = rule.RuleId,
                Name = rule.RuleName,
                Type = RMRuleType.Rule,
                ParentId = parentId
            };
        }

        public List<RuleContainerDto> GetRuleContainersByContainerId(string scopeContainerId, int sourceFlag)
        {
            var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string> { scopeContainerId }, (SourceFlag)sourceFlag);
            var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
            return RMRuleDao.GetAllRuleContainers(ruleContainerIds).ConvertAll(c => ConvertToDto(c));
        }
    }
}
