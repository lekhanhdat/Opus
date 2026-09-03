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
    using System;
    using System.Collections.Generic;

    public abstract class PatchRequest<TArg> : MicrosoftGraphApiBase<bool>
    {
        protected TArg patchObj;
        public PatchRequest(string baseUrl, Func<string> getToken, TArg patchObj, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.patchObj = patchObj;
        }


        public override bool GetApiResult()
        {
            Patch(this.patchObj);
            return true;
        }
    }

    public class UpdateGroup : PatchRequest<Group>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/Groups/{groupId}";//use beta api, v1.0 api not work for teams

        public UpdateGroup(string baseUrl, Func<string> getToken, Group group, IRetryable retryable) : base(baseUrl, getToken, group, retryable)
        {
            if (string.IsNullOrEmpty(group.Id)) throw new ArgumentNullException(nameof(group.Id));
            this.groupId = group.Id;
        }

        private string groupId;

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
            nameof(Group.PreferredDataLocation)
        };
    }
    class UpdateGroupExtraSettings : PatchRequest<GroupExtraInfo>
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/Groups/{groupId}";//use beta api, v1.0 api not work for teams

        public UpdateGroupExtraSettings(string baseUrl, Func<string> getToken, GroupExtraInfo info, IRetryable retryable) : base(baseUrl, getToken, info, retryable)
        {
            if (string.IsNullOrEmpty(info.Id)) throw new ArgumentNullException(nameof(info.Id));
            this.groupId = info.Id;
        }

        private string groupId;

        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(GroupExtraInfo.AllowExternalSenders),
            nameof(GroupExtraInfo.AutoSubscribeNewMembers),
            nameof(GroupExtraInfo.HideFromAddressLists),
            nameof(GroupExtraInfo.HideFromOutlookClients),
        };
    }

    public class UpdatePlannerPlan : PatchRequest<CreatePlannerPlanObj>
    {
        public string PlanId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}";
            }
        }
        public UpdatePlannerPlan(string baseUrl, Func<string> getToken, string planId, Dictionary<string, string> requestHeaders, CreatePlannerPlanObj updateObj, IRetryable retryable)
            : base(baseUrl, getToken, updateObj, retryable)
        {
            this.PlanId = planId;
            this.RequestHeader = requestHeaders;
        }
    }

    public class UpdatePlannerPlanDetails : PatchRequest<UpdatePlannerPlanDetailsObj>
    {
        public string PlanId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/plans/{this.PlanId}/details";
            }
        }
        public UpdatePlannerPlanDetails(string baseUrl, Func<string> getToken, string planId, Dictionary<string, string> requestHeaders, UpdatePlannerPlanDetailsObj updateObj, IRetryable retryable)
            : base(baseUrl, getToken, updateObj, retryable)
        {
            this.PlanId = planId;
            this.RequestHeader = requestHeaders;
        }
    }

    public class UpdatePlannerBucket : PatchRequest<CreatePlannerBucketObj>
    {
        public string BucketId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/Buckets/{this.BucketId}";
            }
        }
        public UpdatePlannerBucket(string baseUrl, Func<string> getToken, string bucket, Dictionary<string, string> requestHeaders, CreatePlannerBucketObj updateObj, IRetryable retryable)
            : base(baseUrl, getToken, updateObj, retryable)
        {
            this.BucketId = bucket;
            this.RequestHeader = requestHeaders;
        }
    }

    public class UpdatePlannerTask : PatchRequest<UpdatePlannerTaskObj>
    {
        public string TaskId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}";
            }
        }
        public UpdatePlannerTask(string baseUrl, Func<string> getToken, string TaskId, Dictionary<string, string> requestHeaders, UpdatePlannerTaskObj updateObj, IRetryable retryable)
            : base(baseUrl, getToken, updateObj, retryable)
        {
            this.TaskId = TaskId;
            this.RequestHeader = requestHeaders;
        }
    }




    public class UpdatePlannerTaskDetails : PatchRequest<UpdatePlannerTaskDetailsObj>
    {
        public string TaskId { get; private set; }
        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/planner/tasks/{this.TaskId}/details";
            }
        }
        public UpdatePlannerTaskDetails(string baseUrl, Func<string> getToken, string TaskId, Dictionary<string, string> requestHeaders, UpdatePlannerTaskDetailsObj updateObj, IRetryable retryable)
            : base(baseUrl, getToken, updateObj, retryable)
        {
            this.TaskId = TaskId;
            this.RequestHeader = requestHeaders;
        }
    }

}