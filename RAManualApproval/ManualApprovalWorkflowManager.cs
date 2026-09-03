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
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client.WorkflowServices;
using PnP.Core.Model.Security;
using RAManualApproval.Comparers;
using RAManualApproval.ReportRelateSettingManagers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval
{
    public class ManualApprovalWorkflowManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalWorkflowManager));

        private static readonly IManualProcessManagementService ManualProcessManagementService = PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private static readonly IManualApprovalService ManualApprovalService = PlatformWindsorManager.GetService<IManualApprovalService>();

        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();
        
        private static readonly IRMWorkflowSiteOwnersDao WorkflowSiteOwnersDao = PlatformWindsorManager.GetService<IRMWorkflowSiteOwnersDao>();

        private static readonly IAccountWrapperService AccountWrapperService = PlatformWindsorManager.GetService<IAccountWrapperService>();

        private static readonly HashSet<string> SyncedSiteOwnerWorkflows = new HashSet<string>();

        private static readonly HashSet<string> NotFoundSiteOwnerWorkflows = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, WorkflowDefinitionDto> Workflows = new ConcurrentDictionary<string, WorkflowDefinitionDto>();

        private static readonly ConcurrentDictionary<string, List<AccountDto>> WorkflowOwners = new ConcurrentDictionary<string, List<AccountDto>>();

        private static readonly Dictionary<Guid, List<AADAccount>> SiteOwners = new Dictionary<Guid, List<AADAccount>>();

        private static readonly Dictionary<string, List<AADAccount>> SharePointGroups = new Dictionary<string, List<AADAccount>>();

        private static readonly HashSet<string> SyncedSPGroupWorkflows = new HashSet<string>();

        public static WorkflowDefinitionDto Get(string workflowRefernceId)
        {
            if(!Workflows.TryGetValue(workflowRefernceId, out var workflow))
            {
                workflow = ManualProcessManagementService.GetWorkflow(Guid.Parse(workflowRefernceId));
                if(!Workflows.TryAdd(workflowRefernceId, workflow))
                {
                    Logger.Warn($"Add workflow: [{workflowRefernceId}] to cache failed");
                }
            }
            return workflow;
        }
        public static WorkflowDefinitionDto Load(string workflowDefinitionId)
        {
            if (!Workflows.TryGetValue(workflowDefinitionId, out var workflow))
            {
                workflow = ManualProcessManagementService.LoadProcess(Guid.Parse(workflowDefinitionId));
                if (!Workflows.TryAdd(workflowDefinitionId, workflow))
                {
                    Logger.Warn($"Add workflow: [{workflowDefinitionId}] to cache failed");
                }
            }
            return workflow;
        }
        public static bool CheckWorkflowHasStepUseSiteOwnerReviewer(string workflowId)
        {
            var definition = Get(workflowId);
            var node = definition.Content.WorkflowNodes.FirstOrDefault(item => item.ReviewerType == WorkflowReviewerType.SiteOwners);
            return node != null;
        }

        public static async Task<List<AccountDto>> GetOwnersAsync(string workflowRefernceId, Guid siteId)
        {
            if(!WorkflowOwners.TryGetValue(workflowRefernceId + siteId.ToString(), out var workflowOwner))
            {
                var workflowDefinition = Get(workflowRefernceId);
                workflowOwner = await ManualApprovalService.GetUserIdsForManualJobAsync(workflowDefinition, siteId);
                if (!WorkflowOwners.TryAdd(workflowRefernceId + siteId.ToString(), workflowOwner))
                {
                    Logger.Warn($"The workflow: [{workflowRefernceId}] add owners to cache failed");
                }

                ManualApprovalOwnerManager.AddOwnersCache(workflowOwner);
            }

            return workflowOwner;
        }

        public static async System.Threading.Tasks.Task SyncSiteOwnerAsync(string workflowRefernceId, ManualExportReportInfo reportInfo, Guid siteId)
        {

            if (NotFoundSiteOwnerWorkflows.Contains($"{workflowRefernceId}=Ave={siteId}"))
            {
                throw new Exception("RM_MA_NotFound_SiteOwner");
            }

            if(SyncedSiteOwnerWorkflows.Contains($"{workflowRefernceId}=Ave={siteId}"))
            {
                Logger.Info($"The site: [{siteId}] used workflow: [{workflowRefernceId}] is synced site owners.");
                return;
            }

            // 处理相同 Site, 但使用的 workflow 是不同的情况。
            if(SiteOwners.TryGetValue(siteId, out var existOwners))
            {
                Logger.Info($"The site: [{siteId}] already exist cache.");
                await SyncOwnersToWorkflowSiteOwnersAsync(existOwners, siteId, workflowRefernceId);
                return;
            }

            if(!SharePointDaoMappingManager.TryGetRecordSiteCollection(reportInfo, out var recordSiteCollection))
            {
                Logger.Warn($"Can't find site collection in record by id: [{siteId}].");
                return;
            }

            var aadAccounts = GetSiteOwners(recordSiteCollection, reportInfo.SiteUrl);
            if(aadAccounts.Count == 0)
            {
                Logger.Info($"Non't find site: [{siteId}] owners.");
                NotFoundSiteOwnerWorkflows.Add($"{workflowRefernceId}=Ave={siteId}");
                throw new Exception("RM_MA_NotFound_SiteOwner");
            }

            await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, aadAccounts, recordSiteCollection.TenantId);

            await SyncOwnersToWorkflowSiteOwnersAsync(aadAccounts, siteId, workflowRefernceId);

            SyncedSiteOwnerWorkflows.Add($"{workflowRefernceId}=Ave={siteId}");
            SiteOwners.Add(siteId, aadAccounts);
        }

        public static async Task SyncSharePointGroupAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step)
        {
            if (!SharePointDaoMappingManager.TryGetRecordSiteCollection(reportInfo, out var recordSiteCollection))
            {
                Logger.Warn($"Can't find site collection in record by id: [{siteId}].");
                return;
            }

            var sPGroupsInWorkflow = step.GetAllSharePointGroupNameAndIsAssignSiteOwners();
            foreach (var group in sPGroupsInWorkflow)
            {
                if (SharePointGroups.TryGetValue(group.Key, out var accounts))
                {
                    Logger.Info($"The group: [{group.Key}] already exist cache.");
                    await SyncUserInSPGroupToWorkflowSiteOwnersAsync(accounts, siteId, workflowId, group.Key);
                    continue;
                }
                if (SyncedSiteOwnerWorkflows.Contains($"{workflowId}=Ave={siteId}={group.Key}"))
                {
                    Logger.Info($"The site: [{siteId}] used workflow: [{workflowId}] is synced SP group [{group.Key}].");
                    continue;
                }
                Logger.Info($"Start get group information with group name is {group.Key}, isAssignSiteOwnersChecked is {group.Value}");

                var syncAccounts = GetAccountInSPGroup(recordSiteCollection, group.Key, group.Value);
                if (syncAccounts == null)
                {
                    Logger.Info($"Don't have the account to sync the opus is current group");
                    throw new Exception("RM_MA_NotFound_SpecifiedGroup");
                }
                else
                {
                    if(syncAccounts.Count == 0)
                    {
                        Logger.Info($"Don't user under the current SharePointGroup {group.Key}");
                        throw new Exception("RM_MA_NotFoundUserUnderGroup_SpecifiedGroup");
                    }

                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, syncAccounts, recordSiteCollection.TenantId);
                    await SyncUserInSPGroupToWorkflowSiteOwnersAsync(syncAccounts, siteId, workflowId, group.Key);
                    SharePointGroups.Add(group.Key, syncAccounts);
                    SyncedSPGroupWorkflows.Add($"{workflowId}=Ave={siteId}={group.Key}");
                }
            }
        }

        public static async System.Threading.Tasks.Task SyncUserInSPGroupToWorkflowSiteOwnersAsync(List<AADAccount> accounts, Guid siteId, string workflowReferenceId, string groupName)
        {
            var workflowId = Get(workflowReferenceId).Id;
            var existSiteOwners = await WorkflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowId.ToString() && item.SiteId == siteId && item.IsSPGroup && item.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
            var siteOwners = accounts.ConvertAll(item => new RMWorkflowSiteOwner
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = workflowId.ToString(),
                SiteId = siteId,
                OwnerType = item.InviteType == AccountType.Group ? RMActiveDirectoryObjectType.Group : RMActiveDirectoryObjectType.User,
                OwnerId = item.AccountId,
                IsSPGroup = true,
                GroupName = groupName
            });

            var needAdded = siteOwners.Except(existSiteOwners, new WorkflowSiteOwnerComparer());
            var needDeleted = existSiteOwners.Except(siteOwners, new WorkflowSiteOwnerComparer());
            WorkflowSiteOwnersDao.BatchAdd(needAdded.ToList());
            WorkflowSiteOwnersDao.BatchDelete(needDeleted.ToList());
        }

        private static List<AADAccount>? GetAccountInSPGroup(RemoteSiteCollection recordSiteCollection, string groupName, bool isAssignSiteOwnersChecked)
        {
            string office365TenantId = recordSiteCollection.TenantId;
            var factory = MultiAppUtil.CreateAveObjectModelFactory(recordSiteCollection.url, CommonPoolUserUtil.GetAveBPOSAccountInfo(recordSiteCollection.Bpos, recordSiteCollection.url), AveContextKind.ClientObjectModel);
            var site = factory.CreateSite();
            Logger.Info($"The site: [{recordSiteCollection.id}] template: [{site.RootWeb.Template}].");

            var specifiedGroup = site?.RootWeb?.Groups?.Where(g => g != null && g.Name.EqualIgnoreCase(groupName)).FirstOrDefault() ?? null;
            if (specifiedGroup != null)
            {
                Logger.Info($"Start convert user in specified group");
                return ConvertToAADAccount(specifiedGroup.Users.ToList(), office365TenantId);
            }

            if (isAssignSiteOwnersChecked && site != null)
            {
                Logger.Info($"Start convert site owner user");
                return GetSiteOwners(recordSiteCollection, site, office365TenantId);
            }
            return null;
        }

        private static List<AADAccount> GetSiteOwners(RemoteSiteCollection recordSiteCollection, IAveSite site, string office365TenantId)
        {
            if (site.RootWeb.Template.StartsWith("SPSPERS#"))
            {
                var owner = site.Owner;
                return ConvertToAADAccount(new List<IAveUser> { owner }, office365TenantId);
            }

            if (site.RootWeb.Template != "GROUP#0")
            {
                var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                var users = ownerGroup.Users.ToList();
                return ConvertToAADAccount(users, office365TenantId);
            }

            var administrator = site.RootWeb.AssociatedOwnerGroup.Users.First(item => item.PrincipalType == AvePrincipalType.SecurityGroup && item.IsSiteAdmin);

            var aadId = administrator.LoginName.Split('|').Last().Split('_').First();
            var result = AccountWrapperService.GetTeamSiteGroupOwners(TenantLocalValue.LogonGroupId, aadId, office365TenantId);
            result.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Mail))
                {
                    item.Mail = item.UserPrincipalName;
                }
            });
            return result;
        }

        private static List<AADAccount> GetSiteOwners(RemoteSiteCollection recordSiteCollection, string siteUrl)
        {
            var office365TenantId = recordSiteCollection.TenantId;
            var factory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, PoolUserUtil.GetAveBPOSAccountInfo(recordSiteCollection.Bpos, siteUrl), AveContextKind.ClientObjectModel);
            var site = factory.CreateSite();
            Logger.Info($"The site: [{recordSiteCollection.id}] tempalte: [{site.RootWeb.Template}].");

            if(site.RootWeb.Template.StartsWith("SPSPERS#"))
            {
                var owner = site.Owner;
                return ConvertToAADAccount(new List<IAveUser> { owner }, office365TenantId);
            }

            if(site.RootWeb.Template != "GROUP#0")
            {
                var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                var users = ownerGroup.Users.ToList();
                return ConvertToAADAccount(users, office365TenantId);
            }

            var administrator = site.RootWeb.AssociatedOwnerGroup.Users.First(item => item.PrincipalType == AvePrincipalType.SecurityGroup && item.IsSiteAdmin);
            
            var aadId = administrator.LoginName.Split('|').Last().Split('_').First();
            var result = AccountWrapperService.GetTeamSiteGroupOwners(TenantLocalValue.LogonGroupId, aadId, office365TenantId);
            result.ForEach(item =>
            {
                if(string.IsNullOrEmpty(item.Mail))
                {
                    item.Mail = item.UserPrincipalName;
                }
            });
            return result;
        }

        private static async System.Threading.Tasks.Task SyncOwnersToWorkflowSiteOwnersAsync(List<AADAccount> accounts, Guid siteId, string workflowRefernceId)
        {
            var workflowId = Get(workflowRefernceId).Id;
            var existSiteOwners = await WorkflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowId.ToString() && item.SiteId == siteId && !item.IsSPGroup);
            var siteOwners = accounts.ConvertAll(item => new RMWorkflowSiteOwner
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = workflowId.ToString(),
                SiteId = siteId,
                OwnerType = item.InviteType == AccountType.Group ? RMActiveDirectoryObjectType.Group : RMActiveDirectoryObjectType.User,
                OwnerId = item.AccountId
            });

            var needAdded = siteOwners.Except(existSiteOwners, new WorkflowSiteOwnerComparer());
            var needDeleted = existSiteOwners.Except(siteOwners, new WorkflowSiteOwnerComparer());
            WorkflowSiteOwnersDao.BatchAdd(needAdded.ToList());
            WorkflowSiteOwnersDao.BatchDelete(needDeleted.ToList());
        }

        private static List<AADAccount> ConvertToAADAccount(List<IAveUser> users, string office365TenantId)
        {
            var accounts = users.Where(item => item.ID != 1073741823).ToList();
            if(accounts.Count == 0)
            {
                Logger.Warn($"Can't find site owners.");
                return new List<AADAccount>();
            }

            var result = new List<AADAccount>();
            var owners = accounts.Where(item => !item.IsDomainGroup).ToList();
            owners.ForEach(owner =>
            {
                if(string.IsNullOrEmpty(owner.Email))
                {
                    owner.Email = owner.LoginName.Split('|').Last();
                }
            });
            var userEmails = owners.Select(item => item.Email).ToList();
            var userResult = AccountWrapperService.GetAccountsByUserEmials(TenantLocalValue.LogonGroupId, userEmails, office365TenantId);
            userResult.ForEach(item =>
            {
                item.InviteType = AccountType.User;
                if (string.IsNullOrEmpty(item.Mail))
                {
                    item.Mail = item.UserPrincipalName;
                }
            });
            result.AddRange(userResult);

            var domainGroups = accounts.Where(item => item.IsDomainGroup).ToList();
            var domainGroupAadIds = domainGroups.Select(item => item.LoginName.Split('|').Last()).ToList();
            var groupResult = AccountWrapperService.GetGroupsByAadIds(TenantLocalValue.LogonGroupId, domainGroupAadIds, office365TenantId);
            groupResult.ForEach(item =>
            {
                item.InviteType = AccountType.Group;
                if(string.IsNullOrEmpty(item.Mail))
                {
                    item.Mail = item.DisplayName;
                }
            });
            result.AddRange(groupResult);

            return result;
        }
    }
}
