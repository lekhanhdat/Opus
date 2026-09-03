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
using System.Net.Http;

namespace AvePoint.GCommon.GraphAPI
{
    public abstract class PostRequest<TArg, TValue> : MicrosoftGraphApiBase<TValue>
    {
        public PostRequest(string baseUrl, Func<string> getToken, TArg postObj, IRetryable retryable) 
            : base(baseUrl, getToken, retryable)
        {
            this.postObj = postObj;
        }

        protected TArg postObj;

        public override TValue GetApiResult()
        {
            return Post(this.postObj);
        }
    }

    public class CreatePlannerPlan : PostRequest<CreatePlannerPlanObj, GraphPlannerPlan>
    {

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans";
            }
        }

        public CreatePlannerPlan(string baseUrl, Func<string> getToken, CreatePlannerPlanObj createPlanDto, IRetryable retryable) 
            : base(baseUrl, getToken, createPlanDto, retryable)
        {
        }
    }

    public class CreatePlannerBucket : PostRequest<CreatePlannerBucketObj, GraphPlannerBucket>
    {

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/buckets";
            }
        }

        public CreatePlannerBucket(string baseUrl, Func<string> getToken, CreatePlannerBucketObj createBucketDto, IRetryable retryable) 
            : base(baseUrl, getToken, createBucketDto, retryable)
        {
        }
    }

    public class CreatePlannerTask : PostRequest<CreatePlannerTaskObj, GraphPlannerTask>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks";
            }
        }

        public CreatePlannerTask(string baseUrl, Func<string> getToken, CreatePlannerTaskObj createTaskObj, IRetryable retryable) 
            : base(baseUrl, getToken, createTaskObj, retryable)
        {
        }

    }

    public class CreateConversationThread : PostRequest<CreateConversationThreadObj, GetConversationThreadObj>
    {
        public string GroupId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/groups/{this.GroupId}/threads";
            }
        }

        public CreateConversationThread(String baseUrl, Func<string> getToken, String groupId, CreateConversationThreadObj createConversationThreadObj, IRetryable retryable) 
            : base(baseUrl, getToken, createConversationThreadObj, retryable)
        {
            this.GroupId = groupId;
        }
    }

    public class CreateGroup : PostRequest<Group, Group>
    {
        public CreateGroup(string baseUrl, Func<string> getToken, Group postObj, IRetryable retryable) : base(baseUrl, getToken, postObj, retryable)
        {
        }
        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(Group.DisplayName),
            nameof(Group.Description),
            nameof(Group.Classification),
            nameof(Group.GroupTypes),
            nameof(Group.MembershipRule),
            nameof(Group.MembershipRuleProcessingState),
            nameof(Group.MailEnabled),
            nameof(Group.MailNickname),
            nameof(Group.SecurityEnabled),
            nameof(Group.Visibility),
            nameof(Group.PreferredDataLocation),
            nameof(Group.OwnersOdata),
            nameof(Group.MembersOdata),
        };

        protected override string RequestUrl => $"{this.apiUrlV1}/groups";
    }

    public class AddGroupOwner : AddGroupMember
    {
        //When adding members to or removing members from a team using the Microsoft Graph v1.0 endpoint, there is a delay before the membership changes are reflected in the Microsoft Teams application/website.
        //If a current team member or owner is signed in to the Microsoft Teams application/website, the change will be reflected within an hour.
        //If none of those users are signed in to the Microsoft Teams application/website, the change will not be reflected until an hour after one of them signs in.
        protected override string RequestUrl => $"{this.apiUrlV1}/groups/{this.GroupId}/owners/$ref";//beta endpoint is faster than v1

        public AddGroupOwner(string baseUrl, Func<string> getToken, string groupId, string directoryObjId, IRetryable retryable)
            : base(baseUrl, getToken, groupId, directoryObjId, retryable)
        {
        }

        public override Empty GetApiResult()
        {
            return base.GetApiResult();
        }
    }

    public class AddGroupMember : PostRequest<DirectoryObject, Empty>
    {
        //When adding members to or removing members from a team using the Microsoft Graph v1.0 endpoint, there is a delay before the membership changes are reflected in the Microsoft Teams application/website.
        //If a current team member or owner is signed in to the Microsoft Teams application/website, the change will be reflected within an hour.
        //If none of those users are signed in to the Microsoft Teams application/website, the change will not be reflected until an hour after one of them signs in.
        protected override string RequestUrl => $"{this.apiUrlV1}/groups/{this.GroupId}/members/$ref";//beta endpoint is faster than v1

        public string GroupId { get; private set; }
        public string DirectoryObjId { get; private set; }

        public AddGroupMember(string baseUrl, Func<string> getToken, string groupId, string directoryObjId, IRetryable retryable)
            : base(baseUrl, getToken, null, retryable)
        {
            this.GroupId = groupId;
            this.DirectoryObjId = directoryObjId;
        }

        public override Empty GetApiResult()
        {
            return Post(BuildDirectoryObject(this.DirectoryObjId));
        }

        private DirectoryObject BuildDirectoryObject(string id)
        {
            return new DirectoryObject()
            {
                ODataId = $"{this.apiUrlV1}/directoryObjects/{id}",
            };
        }
    }

    public class SendMail : PostRequest<SendMailObj, Empty>
    {
        public string UserIdOrUPN { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/users/{UserIdOrUPN}/sendMail";
            }
        }

        public SendMail(string baseUrl, Func<string> getToken, string userIdOrUPN, SendMailObj sendMailObj, IRetryable retryable) : base(baseUrl, getToken, sendMailObj, retryable)
        {
            this.UserIdOrUPN = userIdOrUPN;
        }
    }

    public class ReplyMailMessage : PostRequest<ReplyMessageObj, Empty>
    {
        public string UserIdOrUPN { get; private set; }

        public string MessageId { get; private set; }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/users/{UserIdOrUPN}/messages/{MessageId}/reply";
            }
        }

        public ReplyMailMessage(string baseUrl, Func<string> getToken, string userIdOrUPN, string messageId, ReplyMessageObj replyMailObj, IRetryable retryable) : base(baseUrl, getToken, replyMailObj, retryable)
        {
            this.UserIdOrUPN = userIdOrUPN;
            this.MessageId = messageId;
        }
    }

    public class RestoreDeletedItem : PostRequest<Empty, DirectoryObject>
    {
        public RestoreDeletedItem(
            string baseUrl,
            Func<string> getToken,
            string objectId,
            IRetryable retryable)
            : base(baseUrl, getToken, null, retryable) =>
            ObjectId = objectId;

        public string ObjectId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/directory/deletedItems/{ObjectId}/restore";
    }
}