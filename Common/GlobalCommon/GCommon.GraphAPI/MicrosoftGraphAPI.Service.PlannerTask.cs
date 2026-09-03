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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.GraphAPI
{
    public partial class MicrosoftGraphAPIService
    {
        #region Task
        public List<GraphPlannerTask> ListPlannerTasksByPlanId(string planId)
        {
            var lpTask = new ListPlannerTasks(this.resourceUrl, this.refreshAccessToken, planId, this.RetryController);
            return (List<GraphPlannerTask>)lpTask.GetApiResult();
        }
        public ListPlannerTaskCommentsObj ListPlannerTaskComments(string groupId, string conversationId)
        {
            var gbDetails = new ListPlannerTaskComments(this.resourceUrl, this.refreshAccessToken, groupId, conversationId, this.RetryController);
            return gbDetails.GetApiResult();
        }
        public GraphPlannerTask GetTaskByTaskId(string taskId)
        {
            var gTask = new GetPlannerTask(this.resourceUrl, this.refreshAccessToken, taskId, this.RetryController);
            return gTask.GetApiResult();
        }
        public GraphPlannerTask GetNewTaskIdById(string taskId)
        {
            var gpTaskId = new GetPlannerTaskId(this.resourceUrl, this.refreshAccessToken, taskId, this.RetryController);
            return gpTaskId.GetApiResult();
        }
        public GraphPlannerTaskDetails GetTaskDetailsByTaskId(string taskId)
        {
            var gtDetails = new GetPlannerTaskDetails(this.resourceUrl, this.refreshAccessToken, taskId, this.RetryController);
            return gtDetails.GetApiResult();
        }
        public GraphPlannerTaskDetails GetNewTaskDetailsIdById(string taskId)
        {
            var gtDetails = new GetPlannerTaskDetails(this.resourceUrl, this.refreshAccessToken, taskId, this.RetryController);
            return gtDetails.GetApiResult();
        }
        public GraphPlannerTask CreatePlannerTask(CreatePlannerTaskObj createTaskObj)
        {
            var cpTask = new CreatePlannerTask(this.resourceUrl, this.refreshAccessToken, createTaskObj, this.RetryController);
            return cpTask.GetApiResult();
        }
        public bool UpdatePlannerTask(UpdatePlannerTaskObj upTaskObj, string taskId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var upTask = new UpdatePlannerTask(this.resourceUrl, this.refreshAccessToken, taskId, requestHeaders, upTaskObj, this.RetryController);
            return upTask.GetApiResult();
        }
        public bool UpdatePlannerTaskDetails(UpdatePlannerTaskDetailsObj upTaskDetailsObj, string taskId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var upTaskDetails = new UpdatePlannerTaskDetails(this.resourceUrl, this.refreshAccessToken, taskId, requestHeaders, upTaskDetailsObj, this.RetryController);
            return upTaskDetails.GetApiResult();
        }
        #endregion

        #region ConversationThread
        public string CreateConversationThread(string groupId, CreateConversationThreadObj createConversationThreadObj)
        {
            var ccThread = new CreateConversationThread(this.resourceUrl, this.refreshAccessToken, groupId, createConversationThreadObj, this.RetryController);
            var cctObj = ccThread.GetApiResult();
            return cctObj.Id;
        }
        public GetConversationThreadObj GetConversationThread(string groupId, string conversationId)
        {
            var gbDetails = new GetConversationThread(this.resourceUrl, this.refreshAccessToken, groupId, conversationId, this.RetryController);
            return gbDetails.GetApiResult();
        }

        public IList<GetConversationThreadObj> ListRecentCreatedConversationThread(string groupId, int count)
        {
            var list = new ListConversationThread(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            list.QueryParameters.OrderBy("lastDeliveredDateTime desc");
            list.QueryParameters.Top(count);
            return list.GetApiResult();
        }

        public IList<GetConversationObj> ListConversationsBySearch(string groupId, string search)
        {
            var list = new ListConversations(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            list.QueryParameters.Search($"\"{search.Replace("\"", "\\\"")}\"");
            list.QueryParameters.OrderBy("lastDeliveredDateTime desc");
            return list.GetApiResult();
        }

        public IList<GetConversationThreadObj> ListThreadsOfConversation(string groupId, string conversationId)
        {
            var list = new ListThreadOfConversation(this.resourceUrl, this.refreshAccessToken, groupId, conversationId, this.RetryController);
            return list.GetApiResult();
        }

        #endregion
    }
}