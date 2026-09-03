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
using AvePoint.RA.Common.Email;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google.GControlPlatform;

namespace AvePoint.RA.RACommonUtility.Workflow
{
    public class RMWorkflowProcessor
    {

        private readonly SemaphoreSlim _locker = new(1);

        private readonly Dictionary<Guid, RMWorkflowInstance> _workflowInstanceCache;

        private readonly RMWorkflowCacheContext _cacheContext;
        
        private readonly IGControlPlatformApprovalProcessService _gControlPlatformApprovalProcessService;

        public RMWorkflowProcessor()
        {
            _workflowInstanceCache = new();
            _cacheContext = new RMWorkflowCacheContext();
            _gControlPlatformApprovalProcessService =
                PlatformWindsorManager.GetService<IGControlPlatformApprovalProcessService>();
        }

        public async Task<RMWorkflowInstance> LoadAsync(Guid id)
        {
            if (!_workflowInstanceCache.ContainsKey(id))
            {
                await _locker.WaitAsync();
                try
                {
                    if (!_workflowInstanceCache.ContainsKey(id))
                    {
                        _workflowInstanceCache.Add(id, new RMWorkflowInstance(id, _cacheContext));
                    }
                }
                finally
                {
                    _locker.Release();
                }
            }

            return _workflowInstanceCache[id];
        }
        
        public async Task<RMWorkflowInstance> LoadFromGControlAsync(Guid id)
        {
            if (!_workflowInstanceCache.ContainsKey(id))
            {
                await _locker.WaitAsync();
                try
                {
                    if (!_workflowInstanceCache.ContainsKey(id))
                    {
                        var gControlWorkFlow = await 
                            _gControlPlatformApprovalProcessService.GetPlatformApprovalProcess(id);
                        _workflowInstanceCache.Add(id, new RMWorkflowInstance(gControlWorkFlow, _cacheContext));
                    }
                }
                finally
                {
                    _locker.Release();
                }
            }

            return _workflowInstanceCache[id];
        }
    }

    public class RMWorkflowInstance
    {

        private readonly IRMWorkflowDefinitionDao _workflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private readonly WorkflowDefinitionDto _definition;

        private readonly RMWorkflowCacheContext _cacheContext;

        public RMWorkflowInstance(Guid id, RMWorkflowCacheContext cacheContext)
        {
            var definition = _workflowDefinitionDao.LoadAsync(id).GetAwaiter().GetResult() ??
                throw new Exception($"Workflow [{id}] not exists.");

            _definition = new()
            {
                Id = definition.Id,
                Name = definition.Name,
                ReferenceId = definition.ReferenceId,
                Content = JsonConvert.DeserializeObject<RMWorkflowContentDto>(definition.ContentStr),
                XamlStr = definition.XamlStr
            };

            _cacheContext = cacheContext;
        }
        
        public RMWorkflowInstance(WorkflowDefinitionDto definition, RMWorkflowCacheContext cacheContext)
        {

            _definition = new()
            {
                Id = definition.Id,
                Name = definition.Name,
                ReferenceId = definition.ReferenceId,
                Content = definition.Content,
                XamlStr = definition.XamlStr
            };

            _cacheContext = cacheContext;
        }

        public RMWorkflowStep Start()
        {
            return new RMWorkflowStep(_definition.Id, _definition.Content.WorkflowNodes, _cacheContext);
        }

        public RMWorkflowStep LoadStep(Guid id)
        {
            return new RMWorkflowStep(_definition.Id, _definition.Content.WorkflowNodes, id, _cacheContext);
        }

        public bool HasStepUsedSiteOwnerApprovalMode()
        {
            return _definition.Content.WorkflowNodes.Any(item => item.ReviewerType == WorkflowReviewerType.SiteOwners);
        }

        public bool HasStepUsedInfomationOwnerApprovalMode()
        {
            return _definition.Content.WorkflowNodes.Any(item => item.ReviewerType == WorkflowReviewerType.InformationOwner);
        }

        public bool HasStepUsedSharePointGroupApprovalMode()
        {
            return _definition.Content.WorkflowNodes.Any(w => w.ReviewerType == WorkflowReviewerType.SharePointGroup);
        }

        public Dictionary<string, bool> GetAllSharePointGroupNameAndIsAssignSiteOwners()
        {
            return _definition.Content.WorkflowNodes
                .Where(w => w.ReviewerType == WorkflowReviewerType.SharePointGroup && !string.IsNullOrWhiteSpace(w.GroupName))
                .GroupBy(w => w.GroupName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Any(x => x.IsAssignSiteOwnersChecked)
                );
        }
    }

    public class RMWorkflowStep
    {

        private readonly List<RMWorkflowStepNode> _steps;

        private readonly RMWorkflowStepNode _current;

        private readonly RMWorkflowCacheContext _cacheContext;

        private readonly Guid _definitionId;

        public Guid Id => _current.Id;

        public bool IsEnd => _current.NodeType == WorkflowNodeType.NotDestroy || _current.NodeType == WorkflowNodeType.Destroy;

        public RMWorkflowStepUsedEmailTemplateMode UsedEmailTemplateMode => _current.UsedEmailTemplateMode;

        public Guid UsedEmailTemplateId => UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Default ? 
            RMEmailTemplateId.MANUAL_APPROVAL : _current.UsedEmailTemplateId;

        public List<CustomIntervalSetting> CustomIntervalSettings => _current.CustomIntervalSetting;

        public RMWorkflowStep(Guid definitionId, List<RMWorkflowStepNode> steps, RMWorkflowCacheContext cacheContext)
        {
            _definitionId = definitionId;
            _steps = steps;
            var startStep = steps.First(item => item.NodeType == WorkflowNodeType.Start);
            _current = steps.Where(a => startStep.ChildrenIds.Contains(a.Id)).First(c => c.NodeType == WorkflowNodeType.BeginDisposalReview || c.NodeType == WorkflowNodeType.DisposalReview);
            _cacheContext = cacheContext;
        }

        public RMWorkflowStep(Guid definitionId, List<RMWorkflowStepNode> steps, Guid id, RMWorkflowCacheContext cacheContext)
        {
            _definitionId = definitionId;
            _steps = steps;
            _current = steps.First(item => item.Id == id);
            _cacheContext = cacheContext;
        }

        public RMWorkflowStep Approve()
        {
            var childIds = _current.ChildrenIds;
            var step = _steps.Where(item => childIds.Contains(item.Id)).First(item => item.NodeType == WorkflowNodeType.DisposalReview || item.NodeType == WorkflowNodeType.Destroy);
            return new RMWorkflowStep(_definitionId, _steps, step.Id, _cacheContext);
        }

        public RMWorkflowStep Reject()
        {
            var childIds = _current.ChildrenIds;
            var step = _steps.Where(item => childIds.Contains(item.Id)).First(item => item.NodeType == WorkflowNodeType.DisposalReview || item.NodeType == WorkflowNodeType.NotDestroy);
            return new RMWorkflowStep(_definitionId, _steps, step.Id, _cacheContext);
        }

        public Task<List<ReviewerUser>> GetReviewersAsync(Guid siteId)
        {
            return _cacheContext.GetWorkflowReviewAsync(_current, _definitionId, siteId);
        }

        public RMWorkflowStep GetLastStep()
        {
            var step = _steps.LastOrDefault(item => item.NodeType == WorkflowNodeType.DisposalReview)
                ?? _steps.LastOrDefault(item => item.NodeType == WorkflowNodeType.BeginDisposalReview);

            if (step == null)
            {
                throw new InvalidOperationException("Workflow does not contain a review step.");
            }

            return new RMWorkflowStep(_definitionId, _steps, step.Id, _cacheContext);
        }
        
        public Dictionary<string, bool> GetAllSharePointGroupNameAndIsAssignSiteOwners()
        {
            return _steps
                .Where(w => w.ReviewerType == WorkflowReviewerType.SharePointGroup && !string.IsNullOrWhiteSpace(w.GroupName))
                .GroupBy(w => w.GroupName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Any(x => x.IsAssignSiteOwnersChecked)
                );
        }
    }

    public class RMWorkflowCacheContext
    {
        private readonly IRMWorkflowSiteOwnersDao _workflowSiteOwnersDao = PlatformWindsorManager.GetService<IRMWorkflowSiteOwnersDao>();
        private readonly IRMWorkflowInformationOwnersDao _workflowInformationOwnersDao = PlatformWindsorManager.GetService<IRMWorkflowInformationOwnersDao>();

        private readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private readonly Dictionary<string, List<string>> _siteOwnerCache = new();

        private readonly Dictionary<string, ReviewerUser> _userCache = new();

        private readonly Dictionary<string, List<string>> _spGroupCache = new();
        private readonly Dictionary<string, List<string>> _fsGroupCache = new();

        private readonly SemaphoreSlim _locker = new(1);

        public async Task<List<ReviewerUser>> GetWorkflowReviewAsync(RMWorkflowStepNode step, Guid workflowDefinitionId, Guid siteId)
        {
            if (step.ReviewerType == WorkflowReviewerType.RecordUsers)
            {
                return step.Reviewers;
            }

            if(step.ReviewerType == WorkflowReviewerType.SharePointGroup)
            {
                var groupName = step.GroupName.Trim();
                var spGroupKey = $"{workflowDefinitionId}=AVE={siteId}=AVE={groupName}";
                if (!_spGroupCache.ContainsKey(spGroupKey))
                {
                    await _locker.WaitAsync();
                    try
                    {
                        if (!_spGroupCache.ContainsKey(spGroupKey))
                        {
                            var owners = await _workflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowDefinitionId.ToString() && item.SiteId == siteId && item.IsSPGroup && item.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                            var ownerIds = owners.Select(item => item.OwnerId).ToList();

                            var needQueryOwnerIds = ownerIds.Where(item => !_userCache.ContainsKey(item)).ToList();
                            var users = _accountDao.GetUserWithRemovedByUserIds(needQueryOwnerIds);
                            foreach (var user in users)
                            {
                                if (!_userCache.ContainsKey(user.UserId))
                                {
                                    _userCache.Add(user.UserId, new ReviewerUser
                                    {
                                        RMUserId = user.Id,
                                        UserId = user.UserId,
                                        UserPrincipalName = user.UserPrincipalName,
                                        DisplayName = user.DisplayName,
                                        GivenName = user.FirstName,
                                        SurName = user.LastName
                                    });
                                }
                            }

                            _spGroupCache.Add(spGroupKey, ownerIds);
                        }
                    }
                    finally
                    {
                        _locker.Release();
                    }
                }

                return _spGroupCache[spGroupKey].ConvertAll(item => _userCache[item]);
            }

            if (step.ReviewerType == WorkflowReviewerType.InformationOwner)
            {
                var fsGroupKey = $"{workflowDefinitionId}=AVE={siteId}";
                if (!_fsGroupCache.ContainsKey(fsGroupKey))
                {
                    await _locker.WaitAsync();
                    try
                    {
                        if (!_fsGroupCache.ContainsKey(fsGroupKey))
                        {
                            var owners = await _workflowInformationOwnersDao.FindListAsync(item => item.DefinitionId == workflowDefinitionId.ToString() && item.ConnectionId == siteId);
                            var ownerIds = owners.Select(item => item.OwnerId).ToList();

                            var needQueryOwnerIds = ownerIds.Where(item => !_userCache.ContainsKey(item)).ToList();
                            var users = _accountDao.GetUserWithRemovedByUserIds(needQueryOwnerIds);
                            foreach (var user in users)
                            {
                                if (!_userCache.ContainsKey(user.UserId))
                                {
                                    _userCache.Add(user.UserId, new ReviewerUser
                                    {
                                        RMUserId = user.Id,
                                        UserId = user.UserId,
                                        UserPrincipalName = user.UserPrincipalName,
                                        DisplayName = user.DisplayName,
                                        GivenName = user.FirstName,
                                        SurName = user.LastName
                                    });
                                }
                            }

                            _fsGroupCache.Add(fsGroupKey, ownerIds);
                        }
                    }
                    finally
                    {
                        _locker.Release();
                    }
                }

                return _fsGroupCache[fsGroupKey].ConvertAll(item => _userCache[item]);
            }

            var key = workflowDefinitionId.ToString() + "=AVE=" + siteId.ToString();
            if (!_siteOwnerCache.ContainsKey(key))
            {
                await _locker.WaitAsync();
                try
                {
                    if (!_siteOwnerCache.ContainsKey(key))
                    {

                        var owners = await _workflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowDefinitionId.ToString() && item.SiteId == siteId && !item.IsSPGroup);
                        var ownerIds = owners.Select(item => item.OwnerId).ToList();

                        var needQueryOwnerIds = ownerIds.Where(item => !_userCache.ContainsKey(item)).ToList();
                        var users = _accountDao.GetUserWithRemovedByUserIds(needQueryOwnerIds);
                        foreach (var user in users)
                        {
                            if (!_userCache.ContainsKey(user.UserId))
                            {
                                _userCache.Add(user.UserId, new ReviewerUser
                                {
                                    RMUserId = user.Id,
                                    UserId = user.UserId,
                                    UserPrincipalName = user.UserPrincipalName,
                                    DisplayName = user.DisplayName,
                                    GivenName = user.FirstName,
                                    SurName = user.LastName
                                });
                            }
                        }

                        _siteOwnerCache.Add(key, ownerIds);
                    }
                }
                finally
                {
                    _locker.Release();
                }
            }

            return _siteOwnerCache[key].ConvertAll(item => _userCache[item]);
        }
    }
}
