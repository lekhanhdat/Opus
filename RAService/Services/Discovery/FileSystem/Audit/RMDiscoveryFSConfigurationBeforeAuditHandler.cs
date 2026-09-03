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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Audit 
{
    public class RMDiscoveryFSConfigurationBeforeAuditHandler : IAsyncAuditBeforeHandler
    {
        private readonly IRMDiscoveryConfigurationDao _configInfoDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryFSNodeDao _nodeDao = new RMDiscoveryFSNodeDao();

        private readonly IRMDiscoveryFSRuleInfoDao _ruleInfoDao = new RMDiscoveryFSRuleInfoDao();

        private static readonly IRMTenantDiscoveryDBInfoDao s_tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        private bool IsInitTenantDiscoveryDB => s_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult();

        private bool IsInitDiscoveryFSDB => RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync().GetAwaiter().GetResult();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            if (action == AuditAction.SaveDiscoveryConfiguration)
            {
                var newConfiguration = args[0] as RMDiscoveryFSConfigurationInfo;
                if (!IsInitTenantDiscoveryDB || !IsInitDiscoveryFSDB)
                {
                    await CollectScopeConfig(auditInfo, new() { ScopeType = RMDiscoveryFSScopeType.None }, newConfiguration.ScopeInfo);
                    await CollectROTConfig(auditInfo, new(), newConfiguration.RotDefinition);
                }
                else
                {
                    var oldScopeInfo = await _configInfoDao.GetAsync<RMDiscoveryFSScopeInfo>(RMDiscoveryConfigurationType.FileSystemNewlyScope);
                    var oldRotInfo = await _configInfoDao.GetAsync<RMDiscoveryFSRotDefinition>(RMDiscoveryConfigurationType.FileSystemROTDefinition);
                    await CollectScopeConfig(auditInfo, oldScopeInfo, newConfiguration.ScopeInfo);
                    await CollectROTConfig(auditInfo, oldRotInfo, newConfiguration.RotDefinition);
                }
            }
            return auditInfo;
        }

        public async Task CollectScopeConfig(RMAuditInfo auditInfo, RMDiscoveryFSScopeInfo oldScopeInfo, RMDiscoveryFSScopeInfo scopeInfo)
        {
            var scopeAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ScopeType",
                OldValue = oldScopeInfo.ScopeType.ToString(),
                NewValue = scopeInfo.ScopeType.ToString()
            };
            auditInfo.ModifyContent.Add(scopeAudit);

            if (oldScopeInfo.ScopeType == RMDiscoveryFSScopeType.All ||
                scopeInfo.ScopeType == RMDiscoveryFSScopeType.All)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeAll",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryFSScopeType.All)
                {
                    var oldContainerInfo = _nodeDao.LoadAllGroupsWithoutConnection();
                    IdAudit.OldValue = string.Join(";\n ", GetConnectionGroupNames(oldContainerInfo));
                }

                if (scopeInfo.ScopeType == RMDiscoveryFSScopeType.All)
                {
                    var containerInfo = _nodeDao.LoadAllGroupsWithoutConnection();
                    IdAudit.NewValue = string.Join(";\n ", GetConnectionGroupNames(containerInfo));
                }
                auditInfo.ModifyContent.Add(IdAudit);
            }

            if (oldScopeInfo.ScopeType == RMDiscoveryFSScopeType.Specify ||
                scopeInfo.ScopeType == RMDiscoveryFSScopeType.Specify)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeSpecify",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryFSScopeType.Specify)
                {
                    var oldGroupInfoes = await _nodeDao.GetConnectionGroupsByIds(oldScopeInfo.SpecifyContainerIds);
                    IdAudit.OldValue = string.Join(";\n ", GetConnectionGroupNames(oldGroupInfoes));
                }

                if (scopeInfo.ScopeType == RMDiscoveryFSScopeType.Specify)
                {
                    var groupInfoes = await _nodeDao.GetConnectionGroupsByIds(scopeInfo.SpecifyContainerIds);
                    IdAudit.NewValue = string.Join(";\n ", GetConnectionGroupNames(groupInfoes));
                }
                auditInfo.ModifyContent.Add(IdAudit);
            }

        }

        public async Task CollectROTConfig(RMAuditInfo auditInfo, RMDiscoveryFSRotDefinition oldRotDefinitionInfo, RMDiscoveryFSRotDefinition rotDefinitionInfo)
        {
            var rotConfigAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_RotEnable"
            };
            var rotRedundantRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_RedundantRule",
            };
            var rotObsoleteRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ObsoleteRule",
            };
            var rotTrivialRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_TrivialRule",
            };

            rotConfigAudit.NewValue = rotDefinitionInfo.Enable.ToString();

            if (IsInitTenantDiscoveryDB && IsInitDiscoveryFSDB)
            {
                var oldRotRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);
                rotConfigAudit.OldValue = oldRotDefinitionInfo.Enable.ToString();
                if (oldRotDefinitionInfo.Enable)
                {
                    rotRedundantRuleAudit.OldValue = string.Join(";\n ",
                        oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Redundant && rule.IsEnable)
                            .Select(rule => rule.Name));
                    rotObsoleteRuleAudit.OldValue = string.Join(";\n ",
                        oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Obsolete && rule.IsEnable)
                            .Select(rule => rule.Name));
                    rotTrivialRuleAudit.OldValue = string.Join(";\n ",
                        oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Trivial && rule.IsEnable)
                            .Select(rule => rule.Name));
                }
            }

            if (rotDefinitionInfo.Enable)
            {
                rotRedundantRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.RedundantRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
                rotObsoleteRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.ObsoleteRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
                rotTrivialRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.TrivialRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
            }

            auditInfo.ModifyContent.Add(rotConfigAudit);
            auditInfo.ModifyContent.Add(rotRedundantRuleAudit);
            auditInfo.ModifyContent.Add(rotObsoleteRuleAudit);
            auditInfo.ModifyContent.Add(rotTrivialRuleAudit);
        }

        private List<string> GetConnectionGroupNames(List<FSConnectionGroup> groups)
        {
            return groups.Select(c => c.Name).ToList();
        }
    }
}
