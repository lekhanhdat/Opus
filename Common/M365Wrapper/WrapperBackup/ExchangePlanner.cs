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

namespace ExchangeUtility.Graph
{
    #region namespace

    using AvePoint.GCommon.GraphAPI;
    using AvePoint.RA.CommonUtil;
    using ExchangeCommonWrapper;
    using M365.Wrapper.Backup.Auth.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;

    #endregion

    //public static class ExchangePlannerFacotry
    //{
    //    public static ExchangePlannerService CreateOffice365Planner(AuthObject authObj)
    //    {
    //        switch (authObj.AuthType)
    //        {
    //            case AuthObjectType.PasswordAccessToken:
    //                return new ExchangePlannerWithPasswordAppToken(authObj as IAppTokenAuthObject);
    //            case AuthObjectType.AccessToken:
    //            default:
    //                throw new ArgumentException(string.Format("Unsupport AuthType: {0}", authObj.AuthType));
    //        }
    //    }
    //}

    public abstract class ExchangePlannerService
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(ExchangePlannerService));
        protected MicrosoftGraphAPIService msGraphAPIService = null;

        public abstract bool CreateConversationThread(CreateConversationThreadInfo createInfo, out (string ConversationThreadId, string MessageId) result);

        public abstract IBatchRequestCollection BuildBatchRequestObj(AddPlannerTaskCommentsInfo addCommnetsInfo);

        protected ExchangePlannerService(IAppTokenAuthObject authObj)
        {
            msGraphAPIService = new MicrosoftGraphAPIService(authObj.ResourceUrl, authObj.GetAccessToken, new GraphLogger());
            msGraphAPIService.RetryController = new GraphAPIRetry();
            logger.Info("Create Graph API Service Success: {0}", msGraphAPIService != null);
        }

        public GraphUser GetMe()
        {
            try
            {
                return msGraphAPIService.Me;
            }
            catch { return null; }
        }

        public string GetGroupIdByAddress(string o365GroupMailBox)
        {
            var groupId = string.Empty;
            var retryTime = 0;
            while (string.IsNullOrEmpty(groupId) && retryTime <= 60)
            {
                groupId = msGraphAPIService.GetGroupInfoByAddress(o365GroupMailBox)?.Id;
                retryTime++;
                logger.Info("Get group id, Retry time: {0}", retryTime);
                if (string.IsNullOrEmpty(groupId)) Thread.Sleep(1000);
            }
            return groupId;
        }

        public Group GetGroupInfoByAddress(string o365GroupMailBox)
        {
            var group = msGraphAPIService.GetGroupInfoByAddress(o365GroupMailBox);
            logger.Info("Get group info by address, GroupName: {0}, GroupId: {1}", group?.DisplayName, group?.Id);
            return group;
        }

        public List<Office365PlanBasicProperties> ListAllPlansByGroupID(string groupId)
        {
            logger.Info("Begin to list all plan by groupid. id: {0}", groupId);
            return msGraphAPIService
                .ListAllPlansByGroupID(groupId)
                .Select(v => v.ToM())
                .ToList();
        }

        public Office365PlanDetailsProperties GetPlanDetailsByPlanId(string planId)
        {
            var planDetails = msGraphAPIService.GetPlanDetailsByPlanId(planId);
            return planDetails?.ToM();
        }

        public NewIdDto GetNewPlanDetailsIdByPlanId(string planId)
        {
            try
            {
                var planDetails = msGraphAPIService.GetNewPlanDetailsIdByPlanId(planId);
                return new NewIdDto() { NewId = planDetails.Id, OdataEtag = planDetails.OdataEtag };
            }
            catch (Exception e)
            {
                logger.Warn($"The plan etag was not got. error message : {e.Message}");
            }
            return new NewIdDto();
        }

        public List<Office365PlannerTaskBasicProperties> ListAllTaskByPlanID(string planId)
        {
            var taskDtoList = new List<Office365PlannerTaskBasicProperties>();
            var lPTValue = msGraphAPIService.ListPlannerTasksByPlanId(planId);
            taskDtoList.AddRange(lPTValue.Select(v => v.ToM()));
            return taskDtoList;
        }

        /// <summary>
        /// 1.将取到的 GraphPlannerTask list 先排序，再转换。
        /// 2.反序排序，create 操作后的对象可以保持相对正序。
        /// 3.交换排序方法参数中 T1，T2 的位置，实现正、反序切换。 
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public List<Office365PlannerTaskBasicProperties> GetAllTasks(string planId)
            => msGraphAPIService
                .ListPlannerTasksByPlanId(planId)
                .OrderBy(o => o.BucketTaskBoardFormat.OrderHint, new CompareStringOrdinal())
                .Select(v => v.ToM())
                .ToList();

        public Office365PlannerTaskDetailsProperties GetTaskDetails(string taskId)
        {
            var taskDetails = msGraphAPIService.GetTaskDetailsByTaskId(taskId);
            return taskDetails?.ToM();
        }

        public List<Office365PlannerBucketProperties> ListAllBucketsByPlanID(string planId)
        {
            var bucketDtoList = new List<Office365PlannerBucketProperties>();
            var lPBValue = msGraphAPIService.ListPlannerBucketsByPlanId(planId);
            bucketDtoList.AddRange(lPBValue.Select(v => v.ToM()));
            return bucketDtoList;
        }

        public Office365PlannerTaskBucketProperties GetTaskBucketByBucketId(string bucketId)
        {
            try
            {
                var bucketInfo = msGraphAPIService.GetBucketByBucketId(bucketId);
                return bucketInfo?.ToM2();
            }
            catch (Exception ex)
            {
                logger.Warn("The task bucket information was not got, error message : {0}.", ex);
            }
            return null;
        }

        public Office365PlannerTaskCommentProperties GetPlannerTaskComments(string groupId, string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return EmptyObject();
            }

            try
            {
                var taskCommentsObj = msGraphAPIService.ListPlannerTaskComments(groupId, conversationId);
                return new Office365PlannerTaskCommentProperties()
                {
                    Topic = taskCommentsObj.Topic,
                    ConversationLastDeliveredDateTime = taskCommentsObj.LastDeliveredDateTime,
                    Comments = taskCommentsObj.Posts.Select(taskComment => RecordAndConvertTaskComment(taskComment)).Where(coment => coment != null).ToList(),
                };
            }
            catch (GraphAPIException ex) when (ex.Error.Code == "ErrorItemNotFound"
                || ex.Error.Code == "ErrorNonExistentMailbox"
                || ex.Error.Code == "ErrorMailboxMoveInProgress"
                || ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                return EmptyObject();
            }

            Office365PlannerTaskCommentProperties EmptyObject() => new Office365PlannerTaskCommentProperties()
            {
                ConversationLastDeliveredDateTime = string.Empty,
                Comments = new List<TaskComment>()
            };
        }

        private TaskComment RecordAndConvertTaskComment(GraphTaskComment taskComment)
        {
            if (taskComment.Body == null)
            {
                logger.Warn("Comment with id [{0}] has no body", taskComment.Id);
                return null;
            }
            if (taskComment.Sender == null)
            {
                logger.Warn("Comment with id [{0}] has no sender", taskComment.Id);
            }
            return taskComment.ToM();
        }

        public String CreatePlannerPlan(Office365PlanBasicProperties planBasicProperties, String groupId)
        {
            var cpPlanObj = planBasicProperties.ToCreateObj(groupId);
            var plannerPlanObj = msGraphAPIService.CreatePlannerPlan(cpPlanObj);
            return plannerPlanObj.Id;
        }

        public void DeletePlannerPlan(String planId, string odataEtag)
        {
             msGraphAPIService.DeletePlanByPlanId(planId, odataEtag);
        }

        public String GetPlannerPlanId(string planId)
        {
            try
            {
                return msGraphAPIService.GetPlanByPlanId(planId).Id;
            }
            catch (Exception e)
            {
                logger.Warn($"The plan id was not got. error message : {e.Message}");
            }
            return string.Empty;
        }

        public String GetGroupSiteUrl(string groupId)
        {
            var groupSite = msGraphAPIService.GetGroupSiteByGroupId(groupId);
            return groupSite.WebUrl;
        }
        public bool UpdatePlannerPlan(NewIdDto planDto, String title)
        {
            var updateObj = new CreatePlannerPlanObj() { Title = title };
            return msGraphAPIService.UpdatePlannerPlan(updateObj, planDto.NewId, planDto.OdataEtag);
        }
        public bool UpdatePlanDetails(Office365PlanDetailsProperties planDetailsProperties, NewIdDto newPlanDto)
        {
            var updatePlanDetailsObj = planDetailsProperties.ToUpdateObj();
            return msGraphAPIService.UpdatePlannerPlanDetails(updatePlanDetailsObj, newPlanDto.NewId, newPlanDto.OdataEtag);
        }
        public bool UpdatePlannerBucket(NewIdDto bucketDto, String name)
        {
            var updateObj = new CreatePlannerBucketObj() { Name = name };
            return msGraphAPIService.UpdatePlannerBucket(updateObj, bucketDto.NewId, bucketDto.OdataEtag);
        }
        public string CreatePlannerBucket(Office365PlannerTaskBucketProperties taskBucketProperties, String planId, String orderHint = " !")
        {
            var cpBucketObj = taskBucketProperties.ToCreateObj(planId, orderHint);
            var gpbObj = msGraphAPIService.CreatePlannerBucket(cpBucketObj);
            return gpbObj.Id;
        }

        public NewIdDto CreatePlannerTask(Office365PlannerTaskBasicProperties taskBasicProperties, String planId, String bucketId)
        {
            var createTaskResult = new NewIdDto();
            var cpTaskObj = taskBasicProperties.ToCreateObj(planId, bucketId);
            var plannerTaskObj = msGraphAPIService.CreatePlannerTask(cpTaskObj);
            if (plannerTaskObj != null)
            {
                createTaskResult.NewId = plannerTaskObj.Id;
                createTaskResult.OdataEtag = plannerTaskObj.OdataEtag;
            }
            return createTaskResult;
        }


        public bool CheckConversationReady(string groupId, string conversationThreadId)
        {
            int[] waitTimeArray = new int[] { 1, 2, 7, 5, 15, 10, 10, 10 };
            int index = 0;
            int sumTime = 0;
            while (!CheckConversationThreadExist(groupId, conversationThreadId))
            {
                if (index >= waitTimeArray.Length) return false;
                sumTime += waitTimeArray[index];
                Thread.Sleep(waitTimeArray[index] * 1000);
                logger.Warn("Sum wait time: {0}s", sumTime);
                ++index;
            }
            logger.Info("conversation is ready");
            return true;
        }
        public bool CheckConversationThreadExist(String groupId, String conversationThreadId)
        {
            if (String.IsNullOrWhiteSpace(conversationThreadId)) return false;
            try
            {
                var result = msGraphAPIService.GetConversationThread(groupId, conversationThreadId);
                return true;
            }
            catch (GraphAPIException Ex)
            {
                return false;
            }
        }
        public bool UpdateTask(UpdatePlannerTaskObj updateTaskObj, NewIdDto newTaskDto)
        {
            return msGraphAPIService.UpdatePlannerTask(updateTaskObj, newTaskDto.NewId, newTaskDto.OdataEtag);
        }

        public bool UpdateTaskDetails(UpdatePlannerTaskDetailsObj updateTaskDetailsObj, NewIdDto newTaskDto)
        {
            return msGraphAPIService.UpdatePlannerTaskDetails(updateTaskDetailsObj, newTaskDto.NewId, newTaskDto.OdataEtag);
        }

        public bool BatchAddPlannerTaskComments(AddPlannerTaskCommentsInfo addCommnetsInfo)
        {
            var taskComments = addCommnetsInfo.TaskComments;
            if (null == taskComments || !taskComments.Any()) { return true; }
            var batchRequestObj = BuildBatchRequestObj(addCommnetsInfo);
            var responseItems = batchRequestObj.SentRequest();
            foreach (var responseItem in responseItems)
            {
                if (!responseItem.IsSuccessStatusCode)
                {
                    //CreateConversationThread(string groupId) 与BatchAddPlannerTaskComments（...）间需要间隔一段时间
                    logger.Error("Some task comment failed to add because the new conversationThread {0} in the group {1} is not ready \r\n{2}", addCommnetsInfo.ConversationThreadId, addCommnetsInfo.GroupId, responseItem.Body.ToString());
                    return false;
                }
            }
            return true;
        }

        public List<(string Id, string UserPrincipalName, string DisplayName)> BatchGetAssignedUsers(List<TaskAssignment> taskAssignments)
        {
            var assignedUsers = new List<(string, string, string)>();
            if (!taskAssignments.Any()) return assignedUsers;

            var batchRequestObj = msGraphAPIService.CreateBatchRequestObj(20);
            foreach (var taskAssignmentItem in taskAssignments)
            {
                var requestItem = new RequestItem()
                {
                    Id = taskAssignmentItem.AssignmentId,
                    Url = $"users/{taskAssignmentItem.AssignmentId}?$select=userPrincipalName,displayName",
                    Method = "GET",
                };
                batchRequestObj.Add(requestItem);
            }

            List<ResponseItem> responseItems;
            try
            {
                responseItems = batchRequestObj.SentRequest();
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to get task AssignedUsers, so try aging. Reason:{0}", ex.ToString());
                responseItems = batchRequestObj.SentRequest();
            }

            responseItems.ForEach(responseItem =>
            {
                if (responseItem.IsSuccessStatusCode)
                {
                    var tempUserObj = responseItem.ToObject<GraphUser>();
                    assignedUsers.Add((responseItem.Id, tempUserObj.UserPrincipalName, tempUserObj.DisplayName));
                }
                else
                {
                    logger.Warn("An error occurred while querying assigned user information,the assigned user ID: {0}; Error: {1}", responseItem.Id, responseItem.Body.ToString());
                }
            });

            return assignedUsers;
        }
        /// <summary>
        /// Iteam1 : TaskDetails
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public Tuple<Office365PlannerTaskDetailsProperties> BatchGetTaskInfo(string taskId)
        {
            var batchRequestObj = msGraphAPIService.CreateBatchRequestObj();
            batchRequestObj.Add(
                new RequestItem()
                {
                    Id = "TaskDetails",
                    Method = "GET",
                    Url = $"planner/tasks/{taskId}/details",
                });
            var responses = batchRequestObj.SentRequest();
            if (responses.First().IsSuccessStatusCode)
            {
                return Tuple.Create(responses[0].ToObject<GraphPlannerTaskDetails>().ToM());
            }
            else
            {
                throw responses[0].ToGraphAPIExceptionObj();
            }
        }

        public Dictionary<string, ResponseItem> BatchSelectInfo(string newTaskId, string groupId)
        {
            SimpleBatchRequestCollection batchRequestObj = msGraphAPIService.CreateBatchRequestObj() as SimpleBatchRequestCollection;
            var requestItems = new List<RequestItem>()
            {
                new BatchItem_GetGroupSite(SimpleItemId.GetGroupSite,groupId,"?$select=webUrl"),
                new BatchItem_GetTask(SimpleItemId.GetTask,newTaskId,"?$select=id,assignments"),
                new BatchItem_GetTaskDetails(SimpleItemId.GetTaskDetails,newTaskId,"?$select=id,checklist,references"),
            };
            var retryableKeys = new List<string> { SimpleItemId.GetTask, SimpleItemId.GetTaskDetails };
            batchRequestObj.AddRange(requestItems);
            logger.Info("Batch request start...");
            Dictionary<string, ResponseItem> responseItemDic = new Dictionary<string, ResponseItem>(StringComparer.OrdinalIgnoreCase);
            var responseItems = batchRequestObj.SentRequest();
            if (responseItems.All(r => r.IsSuccessStatusCode)) return responseItems.ToDictionary(key => key.Id);
            var retryTimes = 0;
            do
            {
                var interval = 3000;
                responseItems.ForEach(response =>
                {
                    responseItemDic[response.Id] = response;//更新结果
                    if (response.IsSuccessStatusCode || !retryableKeys.Contains(response.Id))
                    {
                        requestItems.RemoveAll(request => request.Id.Equals(response.Id));//剔除不需要retry的子请求
                    }
                    else if ((Int32)response.Status == 429)
                    {
                        interval = 180000;
                    }
                });
                if (requestItems.Count == 0) return responseItemDic;
                OutputBatchResult(responseItems, retryTimes + 1);
                Thread.Sleep(interval);
                batchRequestObj.Clear();
                batchRequestObj.AddRange(requestItems);
                responseItems = batchRequestObj.SentRequest();
            }
            while (++retryTimes <= 5);
            return responseItemDic;
        }
        public void OutputBatchResult(List<ResponseItem> responses, int retryTimes)
        {
            var sb = new StringBuilder("Errors occurred in BatchSelectInfo, Requests: ");
            responses.ForEach(r => sb.Append($"{r.Id}: [{r.Status}], "));
            sb.Append($"start {retryTimes}th retry.");
            logger.Warn(sb.ToString());
        }
        public List<ResponseItem> BatchGetUsers(List<String> userNames)
        {
            userNames = userNames.Distinct().ToList();
            var batchRequestObj = msGraphAPIService.CreateBatchRequestObj(20);
            foreach (var userName in userNames)
            {
                batchRequestObj.Add(new RequestItem()
                {
                    Id = userName,
                    Method = "GET",
                    Url = $"/users/{ODataSpecialCharactersConverter.ConvertToS(userName)}?$select=id"
                });
            }
            return batchRequestObj.SentRequest();
        }
    }

    public class NewIdDto
    {
        public string NewId { get; set; }

        public string OdataEtag { get; set; }

        public override string ToString()
        {
            return string.Format("NewId: {0}, OdataEtag: {1}", NewId, OdataEtag);
        }
    }

    public class Bucket
    {
        public string OId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public string OrderHint
        {
            get;
            set;
        }
        public string NId
        {
            get;
            set;
        }

        public bool CanGetByName
        {
            get;
            set;
        }
    }

    public class CreateConversationThreadInfo
    {
        public string GroupId { get; set; }

        public string GroupMail { get; set; }

        public Office365PlannerTaskEntity TaskProperties { get; set; }
    }

    public class AddPlannerTaskCommentsInfo
    {
        public List<TaskComment> TaskComments { get; set; }

        public string GroupId { get; set; }

        public string GroupMail { get; set; }

        public string ConversationThreadId { get; set; }

        public string MessageId { get; set; }
    }
}