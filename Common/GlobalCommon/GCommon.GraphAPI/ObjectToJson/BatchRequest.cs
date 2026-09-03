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

namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class BatchRequestObj
    {
        [JsonProperty("requests")]
        public RequestItem[] Requests { get; set; }
    }
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RequestItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("dependsOn")]
        public string[] DependsOn { get; set; }

        [JsonProperty("headers")]
        public Dictionary<String, String> Headers { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }
    }
    public class BatchResult
    {
        private readonly List<ResponseItem> ResponseItems;
        public readonly bool CanRetry;
        private readonly IBatchRequestCollection BatchRequestObj;

        public BatchResult(IBatchRequestCollection batchRequestObj, bool canRetry = false)
        {
            ResponseItems = batchRequestObj.SentRequest();
            if (canRetry)
            {
                CanRetry = canRetry;
                BatchRequestObj = batchRequestObj;
            }
        }
        public BatchResult Retry()
        {
            if (CanRetry) return new BatchResult(BatchRequestObj, CanRetry);
            return null;
        }
        //public Dictionary<string, ResponseItem> ToDictionary()
        //{
        //    return ResponseItems.ToDictionary(key => key.Id);
        //}
    }
    #region BatchItems
    public static class SimpleItemId
    {
        public const String GetTask = "GET_TASK";
        public const String GetTaskDetails = "GET_TASKDETAILS";
        public const String GetConversationThread = "GET_CONVERSATION_THREAD";
        public const String GetTenantRootSite = "GET_TENANT_ROOT_SITE";
        public const String GetGroupSite = "GET_GROUP_SITE";
        public const String GetGroupDrive = "GET_GROUP_DRIVE";
    }

    public class BatchGetItem : RequestItem
    {
        public BatchGetItem(String requestId, String dependsOn = null)
        {
            Id = requestId;
            Method = "GET";
            DependsOn = String.IsNullOrEmpty(dependsOn) ? null : new string[] { dependsOn };
        }
    }
    public class BatchPatchItem : RequestItem
    {
        public BatchPatchItem(String requestId, String OdataEtag, String dependsOn = null)
        {
            Id = requestId;
            Method = "PATCH";
            DependsOn = String.IsNullOrEmpty(dependsOn) ? null : new string[] { dependsOn };
        }
    }
    public class BatchItem_GetTask : BatchGetItem
    {
        public BatchItem_GetTask(String requestId, String taskId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            Url = $"planner/tasks/{taskId}{querryString}";
        }
    }
    public class BatchItem_GetTaskDetails : BatchGetItem
    {
        public BatchItem_GetTaskDetails(String requestId, String taskId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            Url = $"planner/tasks/{taskId}/details{querryString}";
        }
    }
    public class BatchItem_GetGroupSite : BatchGetItem
    {
        public BatchItem_GetGroupSite(String requestId, String groupId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            Url = $"groups/{groupId}/sites/root{querryString}";
        }
    }
    public class BatchItem_GetConversationThread : BatchGetItem
    {
        public BatchItem_GetConversationThread(String requestId, String groupId, String conversationThreadId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            if (String.IsNullOrEmpty(conversationThreadId)) { conversationThreadId = "null"; }
            Url = $"groups/{groupId}/threads/{conversationThreadId}{querryString}";
        }
    }
    public class BatchItem_GetTenantRootSite : BatchGetItem
    {
        public BatchItem_GetTenantRootSite(String requestId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            Url = $"/sites/root{querryString}";
        }
    }
    public class BatchItem_GetGroupDrive : BatchGetItem
    {
        public BatchItem_GetGroupDrive(String requestId, String groupId, String querryString = "", String dependsOn = null) : base(requestId, dependsOn)
        {
            Url = $"groups/{groupId}/drive{querryString}";
        }
    }

    public class BatchItem_GetHostedContentsAsString : BatchGetItem
    {
        public BatchItem_GetHostedContentsAsString(string requestId, string url, string dependsOn = null) : base(requestId, dependsOn) => Url = url;
    }

    #endregion
}