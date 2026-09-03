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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Workflow;
using RAGoogle.Common;

namespace AvePoint.RA.Service.Services.Google;

public class GControlUpdateTaskAssignee : IGControlUpdateTaskAssignee
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(GControlUpdateTaskAssignee));
    
    private readonly IGControlTaskAssigneeService _taskAssigneeService;
    private readonly IGControlPlatformTaskAssigneeService _gControlPlatformTaskAssigneeService;
    private readonly RMWorkflowProcessor _workflowProcessor;
    private readonly IExplorerDao _explorerDao = new ExplorerDao(true);
    private readonly IAccountDao _accountDao;
    private readonly ILnkUserGroupDao _lnkUserGroupDao;

    private readonly IPeoplePickerService _peoplePickerService;
    
    private Dictionary<string, List<string>> _groupUserMapping = [];


    public GControlUpdateTaskAssignee(IGControlTaskAssigneeService taskAssigneeService, 
        IGControlPlatformTaskAssigneeService gControlPlatformTaskAssigneeService,
        IAccountDao accountDao,
        ILnkUserGroupDao lnkUserGroupDao)
    {
        _taskAssigneeService = taskAssigneeService;
        _gControlPlatformTaskAssigneeService = gControlPlatformTaskAssigneeService;
        _workflowProcessor = new();
        _peoplePickerService = new PeoplePickerService();
        _accountDao = accountDao;
        _lnkUserGroupDao = lnkUserGroupDao;
    }

    private async Task UpdateTaskAssignees(List<GControlWorkflowDto> allTaskAssignees)
    {
        var (needToAddTaskAssignees, needToDeleteTaskAssignees) = await GetNeededAddAndDeleteAssignee(allTaskAssignees);
        var assigneeNotExistInAnyNextStep = needToDeleteTaskAssignees.Except(needToAddTaskAssignees);
        var reallyNeedToDeleteTaskAssignees = GetRealDeleteAssignee(assigneeNotExistInAnyNextStep);

        await UpdateMyHubTaskReviewers(needToAddTaskAssignees.ToList(), reallyNeedToDeleteTaskAssignees.ToList());

        await _taskAssigneeService.BatchDeleteAsync(allTaskAssignees.Select(item => item.Id));
    }

    private IEnumerable<string> GetRealDeleteAssignee(IEnumerable<string> assigneeNotExistInAnyNextStep)
    {
        HashSet<string> reallyNeedToDeleteTaskAssignees = [];
        foreach (var assignee in assigneeNotExistInAnyNextStep)
        {
            var existInOtherPendingTask = _explorerDao.Exist(record => record.GControlCurrentApproverId == assignee && record.GControlManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove && record.RecordStatus == (int)RMRecordStatus.ManualPreSync);
            if (!existInOtherPendingTask)
            {
                reallyNeedToDeleteTaskAssignees.Add(assignee);
            }
        }

        return reallyNeedToDeleteTaskAssignees;
    }
    
    private IEnumerable<string> GetRealDeleteAssignee(IEnumerable<RMAccount> userShouldBeRemoved)
    {
        HashSet<string> reallyNeedToDeleteTaskAssignees = [];
        foreach (var assignee in userShouldBeRemoved)
        {
            var existInOtherPendingTask = _explorerDao.Exist(record => (record.GControlCurrentApproverId == assignee.AADId || Enumerable.Contains(record.GControlManualReviewers, assignee.Id)) && record.GControlManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove && record.RecordStatus == (int)RMRecordStatus.ManualPreSync);
            if (!existInOtherPendingTask)
            {
                reallyNeedToDeleteTaskAssignees.Add(assignee.AADId);
            }
        }

        return reallyNeedToDeleteTaskAssignees;
    }

    private async Task<(HashSet<string>, HashSet<string>)> GetNeededAddAndDeleteAssignee(List<GControlWorkflowDto> allTaskAssignees)
    {
        HashSet<string> needToAddTaskAssignees = [];
        HashSet<string> needToDeleteTaskAssignees = [];
            
        foreach (var taskAssignee in allTaskAssignees)
        {
            try
            {
                var workflowInstance = await _workflowProcessor.LoadFromGControlAsync(taskAssignee.WorkflowId);
                var currentStep = workflowInstance.LoadStep(taskAssignee.StageId);
                var reviewer = (await currentStep.GetReviewersAsync(Guid.Empty))[0];
                if (taskAssignee.Status == ApprovalProcessStatus.Pending)
                {
                    needToAddTaskAssignees.Add(reviewer.UserId);
                    await AddUsersUnderGroupIfGroupType(reviewer, needToAddTaskAssignees);
                    continue;
                }
                needToDeleteTaskAssignees.Add(reviewer.UserId);
                await AddUsersUnderGroupIfGroupType(reviewer, needToDeleteTaskAssignees);
                var nextStep = taskAssignee.Status == ApprovalProcessStatus.Approved ? currentStep.Approve() : currentStep.Reject();

                if(nextStep.IsEnd || taskAssignee.Status == ApprovalProcessStatus.Rejected)
                {
                    continue;
                }
                
                var nextReviewer = (await nextStep.GetReviewersAsync(Guid.Empty))[0];
                await AddUsersUnderGroupIfGroupType(nextReviewer, needToAddTaskAssignees);
                needToAddTaskAssignees.Add(nextReviewer.UserId);
                    
            }
            catch (Exception ex)
            {
                _logger.Error($"Error workflow processor for WorkflowId: {taskAssignee.WorkflowId}, StageId: {taskAssignee.StageId}, Status: {taskAssignee.Status}", ex);
            }
                     
        }

        return (needToAddTaskAssignees, needToDeleteTaskAssignees);
    }

    /// <summary>
    /// Current the group user cannot login to Opus to get group parent
    /// So in here, we check if user is group type, we will get all users under this group from PeoplePickerService
    /// and compare with DB to get existing users, and add them to tempList and add LnkUserGroup mapping to DB for future use
    /// </summary>
    /// <param name="reviewer"></param>
    /// <param name="tempList"></param>
    private async Task AddUsersUnderGroupIfGroupType(ReviewerUser reviewer, HashSet<string> tempList)
    {
        if (reviewer.InviteType != RMActiveDirectoryObjectType.Group)
        {
            return;
        }
        var currentUserId = reviewer.UserId;
        if (!_groupUserMapping.TryGetValue(currentUserId, out var usersInGroup))
        {
            var googleGroupUserIds = await _peoplePickerService.GetGroupUserIdsAsync(currentUserId);
            var dbGroupUsers= await _accountDao.GetExistGoogleUserIdsAsync(googleGroupUserIds);
            usersInGroup = dbGroupUsers.Select(user => user.Item1).ToList();
            await _lnkUserGroupDao.AddUsersInGroupAsync(dbGroupUsers.Select(user => user.Item2), currentUserId);
            _groupUserMapping.TryAdd(currentUserId, usersInGroup);
        }
        foreach (var user in usersInGroup)
        {
            tempList.Add(user);
        }
    }
    
    private async Task<(List<RMAccount>, List<RMAccount>)> GetNeededAddAndDeleteReassignAssignee(List<GControlWorkflowDto> allTaskReviewers)
    {
        List<RMAccount> needToAddTaskAssignees = [];
        List<RMAccount> needToDeleteTaskAssignees = [];
        var allUsers = await _accountDao.GetUserWithRemovedByIds(allTaskReviewers.Select(r => r.ManualReviewerId).Distinct().ToList());

        foreach (var reviewer in allTaskReviewers)
        {
            try
            {
                var user = allUsers.FirstOrDefault(u => u.Id == reviewer.ManualReviewerId);
                if(user != null && !string.IsNullOrEmpty(user.AADId))
                {
                    switch (reviewer.Status)
                    {
                        case ApprovalProcessStatus.AddMapping:
                            await AddUsersToList(needToAddTaskAssignees, user);
                            break;
                        case ApprovalProcessStatus.RemoveMapping:
                            await AddUsersToList(needToDeleteTaskAssignees, user);
                            break;
                    }
                }   
            }catch(Exception ex)
            {
                _logger.Error($"Error getting reassigned reviewer for ReviewerId: {reviewer.ManualReviewerId}, Status: {reviewer.Status}", ex);
            }
        }
        
        return (needToAddTaskAssignees.DistinctBy(user => user.Id).ToList(), needToDeleteTaskAssignees.DistinctBy(user => user.Id).ToList());
    }
    
    private async Task AddUsersToList(List<RMAccount> list, RMAccount user)
    {
        list.Add(user);
        if(user.ObjectType == RMActiveDirectoryObjectType.Group)
        {
            HashSet<string> tempGroupUserIds = [];
            await AddUsersUnderGroupIfGroupType(new ReviewerUser()
            {
                UserId = user.AADId,
                InviteType = RMActiveDirectoryObjectType.Group
            }, tempGroupUserIds);
            foreach (var groupUserId in tempGroupUserIds)
            {
                var existedUsers = await _accountDao.GetUserByAADIdAsync(groupUserId);
                if(existedUsers != null)
                {
                    list.Add(existedUsers);
                }
            }
        }
    }
    
    private async Task UpdateTaskReviewers(List<GControlWorkflowDto> allTaskReviewers)
    {
        var (needToAddTaskAssignees, needToDeleteTaskAssignees) = await GetNeededAddAndDeleteReassignAssignee(allTaskReviewers);
        var assigneeNotExistInAnyNextStep = needToDeleteTaskAssignees.ExceptBy(needToAddTaskAssignees.Select(assignee => assignee.Id), a => a.Id);
        var reallyNeedToDeleteTaskAssignees = GetRealDeleteAssignee(assigneeNotExistInAnyNextStep);
        
        await UpdateMyHubTaskReviewers(needToAddTaskAssignees.Select(assignee => assignee.AADId).ToList(), reallyNeedToDeleteTaskAssignees.ToList());
        
        await _taskAssigneeService.BatchDeleteAsync(allTaskReviewers.Select(item => item.Id));
    }

    private async Task UpdateMyHubTaskReviewers(List<string> needToAddTaskAssignees, List<string> needToDeleteTaskAssignees)
    {
        var currentTaskAssignees = await _gControlPlatformTaskAssigneeService.GetCurrentPlatformTaskAssignees();
        var taskAssigneesToDelete = currentTaskAssignees.Where(a => needToDeleteTaskAssignees.Contains(a.AssigneeId)).ToList();
        await _gControlPlatformTaskAssigneeService.DeletePlatformTaskAssignees(taskAssigneesToDelete.Select(item => item.Id));
        var userIdsToAdd = needToAddTaskAssignees.Except(currentTaskAssignees.Select(a => a.AssigneeId)).ToList();
        await _gControlPlatformTaskAssigneeService.AddPlatformTaskAssigneesAsync(userIdsToAdd);
    }

    public async Task<bool> IsSucceedAddedPendingTaskAssignee()
    {
        try
        {
            var allTaskAssignees = await _taskAssigneeService.GetAllPendingTaskAssigneeAsync();
            _logger.Info($"Updating pending task assignees user count: {allTaskAssignees.Count}");
            if (allTaskAssignees.Count == 0)
            {
                return true;
            }

            await UpdateTaskAssignees(allTaskAssignees);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Error occurred while updating pending task assignees.", ex);
            return false;
        }
    }

    public async Task<bool> IsSucceedAddedApprovalTaskAssignee()
    {
        try
        {
            var allTaskAssignees = await _taskAssigneeService.GetAllApprovalTaskAssigneeAsync();
            _logger.Info($"Updating approval task assignees user count: {allTaskAssignees.Count}");
            if (allTaskAssignees.Count == 0)
            {
                return true;
            }

            await UpdateTaskAssignees(allTaskAssignees);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Error occurred while updating approval task assignees.", ex);
            return false;
        }
    }
    
    public async Task<bool> IsSucceedAddedTaskReviewer()
    {
        try
        {
            var allTaskAssignees = await _taskAssigneeService.GetAllPendingReviewersAsync();
            _logger.Info($"Updating task reviewers count: {allTaskAssignees.Count}");
            if (allTaskAssignees.Count == 0)
            {
                return true;
            }

            await UpdateTaskReviewers(allTaskAssignees);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Error occurred while updating task reviewers.", ex);
            return false;
        }
    }
}