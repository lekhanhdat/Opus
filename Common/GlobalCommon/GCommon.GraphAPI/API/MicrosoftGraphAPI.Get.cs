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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.GraphAPI.JsonToObject.Teams;

namespace AvePoint.GCommon.GraphAPI
{

    public abstract class GetRequest<TValue> : MicrosoftGraphApiBase<TValue>
    {
        public GetRequest(Func<string> getToken, IRetryable retryable) : base(getToken, retryable)
        {
        }

        public GetRequest(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
        }

        public override TValue GetApiResult()
        {
            return Get();
        }
    }

    public abstract class ListRequest<TValue> : GetRequest<IList<TValue>>
    {
        public ListRequest(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
        }

        public override IList<TValue> GetApiResult()
        {
            return GetAll();
        }

        protected IList<TValue> GetAll()
        {
            var list = new List<TValue>();
            var url = this.FullUrl;
            bool morePage = false;
            do
            {
                var pageResponse = JsonDeserializer<PageResponse<TValue>>(Execute(null, url));
                list.AddRange(pageResponse.Objects);
                url = pageResponse.NextLink;
                morePage = !string.IsNullOrWhiteSpace(pageResponse.NextLink);
            } while (morePage);
            return list;
        }
    }

    public abstract class IEnumerableRequest<TValue> : GetRequest<IEnumerable<TValue>>
    {
        public IEnumerableRequest(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
        }

        public override IEnumerable<TValue> GetApiResult()
        {
            var url = FullUrl;
            bool morePage;
            do
            {
                var pageResponse = JsonDeserializer<PageResponse<TValue>>(Execute(null, url));
                foreach (var item in pageResponse.Objects)
                {
                    yield return item;
                }
                url = pageResponse.NextLink;
                morePage = !string.IsNullOrWhiteSpace(pageResponse.NextLink);
            } while (morePage);
        }
    }

    /// <summary>
    /// 所有调用出处理异常
    /// </summary>
    public class ListGroups : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups";
            }
        }

        public ListGroups(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {

        }
    }

    public class GetGroupByMail : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups?$filter= mail eq '{ODataSpecialCharactersConverter.ConvertToS(this.GroupMailBox)}'";
            }
        }
        public string GroupMailBox { get; private set; }

        public GetGroupByMail(string baseUrl, Func<string> getToken, string groupMailBox, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.GroupMailBox = groupMailBox;

        }
    }

    public class GetGroupByMailNickName : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups?$filter= mailNickname eq '{ODataSpecialCharactersConverter.ConvertToS(this.MailNickName)}'";
            }
        }
        public string MailNickName { get; private set; }

        public GetGroupByMailNickName(string baseUrl, Func<string> getToken, string mailNickName, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.MailNickName = mailNickName;

        }
    }

    public class GetGroupByDisplayName : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups?$filter= displayName eq '{DisplayName}'";
            }
        }
        public string DisplayName { get; private set; }

        public GetGroupByDisplayName(string baseUrl, Func<string> getToken, string displayName, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.DisplayName = displayName;
        }
    }
    public class GetEmailByUPNName : GetRequest<GraphUserEmail>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/users/{UPNName}/mail";
            }
        }
        public string UPNName { get; private set; }

        public GetEmailByUPNName(string baseUrl, Func<string> getToken, string UPNName, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.UPNName = UPNName;
        }
    }
    public class GetGroupByGroupId : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups?$filter= id eq '{this.GroupId}'";//todo:qlluo:use get group instead, performance improve
            }
        }
        public string GroupId { get; private set; }

        public GetGroupByGroupId(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }

    public class GetGroupAssignedLabel : GetRequest<ListAssignedLabels>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}?$select=assignedLabels";//todo:qlluo:use get group instead, performance improve
            }
        }
        public string GroupId { get; private set; }

        public GetGroupAssignedLabel(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
        }
    }

    public class GetGroupVisibility : GetRequest<Group>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}?$select= visibility";
            }
        }
        public string GroupId { get; private set; }

        public GetGroupVisibility(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }

    public class ListGroupOwners : ListRequest<GraphUser>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/owners";
            }
        }
        public string GroupId { get; private set; }

        public ListGroupOwners(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }
    
    public class GetGroupExtraSettings : GetRequest<GroupExtraInfo>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}?$select=allowExternalSenders,autoSubscribeNewMembers,hideFromAddressLists,hideFromOutlookClients";
            }
        }
        public string GroupId { get; private set; }

        public GetGroupExtraSettings(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
        }
    }
    public class ListGroupUsersWithBetaApi : ListRequest<GraphUser>
    {
        protected override string RequestUrl
        {
            get
            {
                var findType = FindOwner ? "owners" : "members";
                return $"{this.apiUrlBeta}/groups/{this.GroupId}/{findType}";
            }
        }
        public string GroupId { get; private set; }
        public bool FindOwner;
        public ListGroupUsersWithBetaApi(string baseUrl, Func<string> getToken, string groupId, bool findOwner, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.FindOwner = findOwner;
        }
    }

    public class ListGroupMembers : ListRequest<GraphUser>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/members";
            }
        }
        public string GroupId { get; private set; }
        public ListGroupMembers(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }

    public class ListGroupMembersByGroupDisplayName : ListRequest<GraphUser>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups?$filter= displayName eq '{this.DisplayName}'";
            }
        }
        public string DisplayName { get; private set; }
        public ListGroupMembersByGroupDisplayName(string baseUrl, Func<string> getToken, string displayName, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.DisplayName = displayName;
        }
    }
    
        public class ListGroupUsers : ListRequest<GraphUser>
    {
        protected override string RequestUrl
        {
            get
            {
                var findType = FindOwner ? "owners" : "members";
                return $"{this.apiUrlV1}/groups/{this.GroupId}/{findType}?$select=id,displayName,mail,userPrincipalName,userType,assignedPlans";
            }
        }
        public string GroupId { get; private set; }
        public bool FindOwner;
        public ListGroupUsers(string baseUrl, Func<string> getToken, string groupId, bool findOwner, IRetryable retryable = null) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.FindOwner = findOwner;
        }
    }

    public class GetGroup : GetRequest<Group>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/groups/{this.GroupId}";

        public string GroupId { get; set; }

        public GetGroup(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
        }
    }

    public class ListGroup : ListRequest<Group>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/groups";
        public ListGroup(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable) { }
    }

    public class GetUser : GetRequest<GraphUser>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/users/{this.IdOrUserPrincipalName}";

        public string IdOrUserPrincipalName { get; set; }

        public GetUser(string baseUrl, Func<string> getToken, string idOrUserPrincipalName, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.IdOrUserPrincipalName = idOrUserPrincipalName;
        }
    }

    public class ListUser : ListRequest<GraphUser>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/users";
        public ListUser(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable) { }
    }

    public class Me : GetRequest<GraphUser>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/me";
        public Me(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable) { }
    }

    public class ListMessages : ListRequest<GetMessageObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/users/{UserId}/messages";
            }
        }

        public string UserId { get; private set; }

        public string InternetMessageId { get; private set; }

        public ListMessages(string baseUrl, Func<string> getToken, string userId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.UserId = userId;
        }
    }


    public class GetPlannerPlan : GetRequest<GraphPlannerPlan>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}";
            }
        }
        public string PlanId { get; private set; }
        public GetPlannerPlan(string baseUrl, Func<string> getToken, string planId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;

        }

    }

    public class DeletePlannerPlan : DeleteRequest
    {
        
        public string PlanId { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}";
            }
        }

        public DeletePlannerPlan(string baseUrl, Func<string> getToken, string planId, Dictionary<string, string> requestHeaders, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;
            this.RequestHeader = requestHeaders;
        }

    }

    public class ListPlannerPlan : ListRequest<GraphPlannerPlan>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/planner/plans";
            }
        }
        public string GroupId { get; private set; }
        public ListPlannerPlan(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }

    public class GetPlannerPlanDetails : GetRequest<GraphPlannerPlanDetails>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}/details";
            }
        }
        public string PlanId { get; private set; }
        public GetPlannerPlanDetails(string baseUrl, Func<string> getToken, string planId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;

        }
    }

    public class GetPlannerPlanDetailsId : GetRequest<GraphPlannerPlanDetails>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{ this.apiUrlV1}/planner/plans/{this.PlanId}/details?$select=id";
            }
        }
        public string PlanId { get; private set; }
        public GetPlannerPlanDetailsId(string baseUrl, Func<string> getToken, string planId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;
            base.httpMethod = HttpMethod.Get;

        }
    }

    public class ListPlannerTasks : ListRequest<GraphPlannerTask>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}/tasks?$expand=bucketTaskBoardFormat";
            }
        }
        public string PlanId { get; private set; }
        public ListPlannerTasks(string baseUrl, Func<string> getToken, string planId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;

        }
    }

    public class GetPlannerTask : GetRequest<GraphPlannerTask>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}";
            }
        }
        public string TaskId { get; private set; }
        public GetPlannerTask(string baseUrl, Func<string> getToken, string taskId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.TaskId = taskId;

        }
    }

    public class GetPlannerTaskId : GetRequest<GraphPlannerTask>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}?$select=id";
            }
        }
        public string TaskId { get; private set; }
        public GetPlannerTaskId(string baseUrl, Func<string> getToken, string taskId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.TaskId = taskId;

        }
    }

    public class GetPlannerTaskDetails : GetRequest<GraphPlannerTaskDetails>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}/details";
            }
        }
        public string TaskId { get; private set; }
        public GetPlannerTaskDetails(string baseUrl, Func<string> getToken, string taskId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.TaskId = taskId;

        }
    }

    public class GetPlannerTaskDetailsId : GetRequest<GraphPlannerTaskDetails>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}/details?$select=id";
            }
        }
        public string TaskId { get; private set; }
        public GetPlannerTaskDetailsId(string baseUrl, Func<string> getToken, string taskId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.TaskId = taskId;

        }
    }

    public class GetPlannerBucket : GetRequest<GraphPlannerBucket>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/buckets/{this.BucketId}";
            }
        }
        public string BucketId { get; private set; }
        public GetPlannerBucket(string baseUrl, Func<string> getToken, string bucketId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.BucketId = bucketId;

        }
    }

    public class ListPlannerTaskComments : GetRequest<ListPlannerTaskCommentsObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{GroupId}/threads/{this.ConversationId}?$select=id,topic,lastDeliveredDateTime&$expand=posts";
            }
        }
        public string GroupId { get; private set; }
        public string ConversationId { get; private set; }
        public ListPlannerTaskComments(string baseUrl, Func<string> getToken, string groupId, string conversationId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.ConversationId = String.IsNullOrEmpty(conversationId) ? "null" : conversationId;

        }
    }

    public class GetConversationThread : GetRequest<GetConversationThreadObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/threads/{this.ConversationThreadId}?$select=id";
            }
        }

        public string GroupId { get; private set; }
        public string ConversationThreadId { get; private set; }
        public GetConversationThread(string baseUrl, Func<string> getToken, string groupId, string conversationThreadId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.ConversationThreadId = String.IsNullOrWhiteSpace(conversationThreadId) ? "null" : conversationThreadId;

        }
    }

    public class ListConversationThread : ListRequest<GetConversationThreadObj>
    {
        public string GroupId { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/threads";
            }
        }

        public ListConversationThread(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
        }
    }

    public class ListConversations : ListRequest<GetConversationObj>
    {
        public string GroupId { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/conversations";
            }
        }

        public ListConversations(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
        }
    }

    public class ListThreadOfConversation : ListRequest<GetConversationThreadObj>
    {
        public string GroupId { get; private set; }

        public string conversationId { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/conversations/{this.conversationId}/threads";
            }
        }

        public ListThreadOfConversation(string baseUrl, Func<string> getToken, string groupId, string conversationId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.conversationId = conversationId;
        }
    }

    public class ListPlannerBuckets : ListRequest<GraphPlannerBucket>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}/buckets";
            }
        }
        public string PlanId { get; private set; }
        public ListPlannerBuckets(string baseUrl, Func<string> getToken, string planId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.PlanId = planId;

        }
    }

    public class GetGroupSite : GetRequest<GetGroupSiteObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/sites/root";
            }
        }
        public string GroupId { get; private set; }
        public GetGroupSite(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;

        }
    }

    public class GetUserDrive : GetRequest<DriveObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/users/{UserPrincipalName}/drive";
            }
        }
        public string UserPrincipalName { get; private set; }
        public GetUserDrive(string baseUrl, Func<string> getToken, string userPrincipalName, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.UserPrincipalName = userPrincipalName;
        }

    }
    public class GetHostedContentAsByte : GetRequest<byte[]>
    {
        protected override string RequestUrl => Url;

        public GetHostedContentAsByte(string url, Func<string> getToken, IRetryable retryable) : base(getToken, retryable) => Url = url;

        public string Url { get; private set; }

        public override byte[] GetApiResult() => ExecuteV1(null, RequestUrl);
    }

    public class GetHostedContentAsString : GetRequest<string>
    {
        protected override string RequestUrl => Url;

        public GetHostedContentAsString(string url, Func<string> getToken, IRetryable retryable) : base(getToken, retryable) => Url = url;

        public string Url { get; private set; }

        public override string GetApiResult() => Execute(null, RequestUrl);
    }

    public class GetEvent : GetRequest<Event>
    {
        protected override string RequestUrl => $"{apiUrlV1}/groups/{GroupId}/events/{EventId}";

        public string GroupId { get; private set; }

        public string EventId { get; private set; }

        public GetEvent(string baseUrl, Func<string> getToken, IRetryable retryable, string groupId, string eventId) : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            EventId = eventId;
        }
    }

    public class GetSubscribedSkus : GetRequest<ListSubscribedSkus>
    {
        protected override string RequestUrl => $"{apiUrlV1}/subscribedSkus";

        public GetSubscribedSkus(string baseUrl, Func<string> getToken, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
        }
    }

    public class GetLicenseDetails : GetRequest<ListLicenseDetails>
    {
        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/licenseDetails";

        public string UserId { get; private set; }

        public GetLicenseDetails(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable)
            => UserId = userId;
    }

    public class GetUserPhoto: GetRequest<byte[]>
    {
        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/photo/$value";

        public string UserId { get; private set; }

        public override byte[] GetApiResult() => ExecuteV1(null, RequestUrl);

        public GetUserPhoto(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable)
            => UserId = userId;
    }

    public class GetOwnedObjects : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/ownedObjects";

        public string UserId { get; private set; }

        public GetOwnedObjects(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable)
            => UserId = userId;
    }

    public class GetMemberOf : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/memberof";

        public string UserId { get; private set; }

        public GetMemberOf(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable)
            => UserId = userId;
    }

    public class GetNextGroupsObj : GetRequest<ListGroupsObj>
    {
        protected override string RequestUrl => OdataNextLink;

        public string OdataNextLink { get; private set; }

        public GetNextGroupsObj(Func<string> getToken, IRetryable retryable, string odataNextLink)
            : base(getToken, retryable)
            => OdataNextLink = odataNextLink;
    }
    public class GetODFBSite : GetRequest<GetGroupSiteObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}";
            }
        }

        public string HostName { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public GetODFBSite(string baseUrl, Func<string> getToken, string siteUrl, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            var siteUri = new Uri(siteUrl);
            this.HostName = siteUri.Authority;
            this.ServerRelativeUrl = siteUri.AbsolutePath;

        }
    }
    public class GetODFBSiteLists : ListRequest<SPListObj>
    {
        protected override string RequestUrl
        {
            get
            {
                if (string.IsNullOrEmpty(this.Select))
                {
                    return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}:/lists";

                }
                else
                {
                    return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}:/lists?Select={this.Select}";
                }
            }
        }
        public string HostName { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public string Select { get; private set; }
        public GetODFBSiteLists(string baseUrl, Func<string> getToken, string siteUrl, string select, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            var siteUri = new Uri(siteUrl);
            this.HostName = siteUri.Authority;
            this.ServerRelativeUrl = siteUri.AbsolutePath;
            this.Select = select;
        }
    }

    public class GetSPList : GetRequest<SPListObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}:/lists/{listId}";
            }
        }
        public string HostName { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public string listId { get; private set; }
        public GetSPList(string baseUrl, Func<string> getToken, string siteUrl, string id, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            var siteUri = new Uri(siteUrl);
            this.HostName = siteUri.Authority;
            this.ServerRelativeUrl = siteUri.AbsolutePath;
            this.listId = id;
        }
    }

    public class GetSPListFields : ListRequest<SPFieldObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}:/lists/{listId}/columns";
            }
        }
        public string HostName { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public string listId { get; private set; }
        public GetSPListFields(string baseUrl, Func<string> getToken, string siteUrl, string id, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            var siteUri = new Uri(siteUrl);
            this.HostName = siteUri.Authority;
            this.ServerRelativeUrl = siteUri.AbsolutePath;
            this.listId = id;
        }

    }

    public class GetDrive : GetRequest<DriveObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{this.HostName}:{this.ServerRelativeUrl}:/lists/{listId}/drive";
            }
        }
        public string HostName { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public string listId { get; private set; }
        public GetDrive(string baseUrl, Func<string> getToken, string siteUrl, string id, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            var siteUri = new Uri(siteUrl);
            this.HostName = siteUri.Authority;
            this.ServerRelativeUrl = siteUri.AbsolutePath;
            this.listId = id;
        }

    }

    public class GetODFBListItems : ListRequest<SPItemObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{this.tenantName}-my.sharepoint.com:/personal/{this.email}:/lists/{listId}/items";
            }
        }
        public string tenantName { get; private set; }
        public string email { get; private set; }
        public string listId { get; private set; }

        public GetODFBListItems(string baseUrl, Func<string> getToken, string tenantName, string email, string id, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.tenantName = tenantName;
            this.email = email;
            this.listId = id;
        }
    }

    public class GetUserRecordingDrive : GetRequest<DriveObj>
    {
        public string UserPrincipalName { get; private set; }

        protected override string RequestUrl => $"{this.apiUrlV1}/users/{UserPrincipalName}/drive/special/recordings";

        public GetUserRecordingDrive(string baseUrl, Func<string> getToken, string userPrincipalName, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.UserPrincipalName = userPrincipalName;
        }
    }

    public class GetAllDomains : ListRequest<Domain>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/domains";
        public GetAllDomains(string baseUrl, Func<string> getToken, IRetryable retryable) : base(baseUrl, getToken, retryable) { }
    }
}