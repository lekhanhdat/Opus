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
    using System.Linq;

    public partial class MicrosoftGraphAPIService
    {
        #region Plan

        public List<GraphPlannerPlan> ListAllPlansByGroupID(string groupId)
        {
            var lpPlan = new ListPlannerPlan(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            return lpPlan.GetApiResult().ToList();
        }
        public GraphPlannerPlanDetails GetPlanDetailsByPlanId(string planId)
        {
            var gpDetails = new GetPlannerPlanDetails(this.resourceUrl, this.refreshAccessToken, planId, this.RetryController);
            return gpDetails.GetApiResult();
        }
        public GraphPlannerPlanDetails GetNewPlanDetailsIdByPlanId(string planId)
        {
            var gpDetails = new GetPlannerPlanDetailsId(this.resourceUrl, this.refreshAccessToken, planId, this.RetryController);
            return gpDetails.GetApiResult();
        }
        public GraphPlannerPlan GetPlanByPlanId(string planId)
        {
            var gPlan = new GetPlannerPlan(this.resourceUrl, this.refreshAccessToken, planId, this.RetryController);
            return gPlan.GetApiResult();
        }

        public void DeletePlanByPlanId(string planId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var gPlan = new DeletePlannerPlan(this.resourceUrl, this.refreshAccessToken, planId, requestHeaders, this.RetryController);
            gPlan.GetApiResult();
        }
        public GraphPlannerPlan CreatePlannerPlan(CreatePlannerPlanObj createPlanObj)
        {
            var cpPlan = new CreatePlannerPlan(this.resourceUrl, this.refreshAccessToken, createPlanObj, this.RetryController);
            return cpPlan.GetApiResult();
        }
        public bool UpdatePlannerPlan(CreatePlannerPlanObj upPlanObj, string planId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var request = new UpdatePlannerPlan(this.resourceUrl, this.refreshAccessToken, planId, requestHeaders, upPlanObj, this.RetryController);
            return request.GetApiResult();
        }
        public bool UpdatePlannerPlanDetails(UpdatePlannerPlanDetailsObj upPlanDetailsObj, string planId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var upPlanDetails = new UpdatePlannerPlanDetails(this.resourceUrl, this.refreshAccessToken, planId, requestHeaders, upPlanDetailsObj, this.RetryController);
            return upPlanDetails.GetApiResult();
        }
        #endregion

        #region Bucket
        public List<GraphPlannerBucket> ListPlannerBucketsByPlanId(string planId)
        {
            var lpBuckets = new ListPlannerBuckets(this.resourceUrl, this.refreshAccessToken, planId, this.RetryController);
            return (List<GraphPlannerBucket>)lpBuckets.GetApiResult();
        }
        public GraphPlannerBucket GetBucketByBucketId(string bucketId)
        {
            var gbDetails = new GetPlannerBucket(this.resourceUrl, this.refreshAccessToken, bucketId, this.RetryController);
            return gbDetails.GetApiResult();
        }
        public GraphPlannerBucket CreatePlannerBucket(CreatePlannerBucketObj createBucketObj)
        {
            var cpBucket = new CreatePlannerBucket(this.resourceUrl, this.refreshAccessToken, createBucketObj, this.RetryController);
            return cpBucket.GetApiResult();
        }
        public bool UpdatePlannerBucket(CreatePlannerBucketObj updateBucketObj, string bucketId, string odataEtag)
        {
            var requestHeaders = new Dictionary<string, string>() { { "If-Match", odataEtag } };
            var request = new UpdatePlannerBucket(this.resourceUrl, this.refreshAccessToken, bucketId, requestHeaders, updateBucketObj, this.RetryController);
            return request.GetApiResult();
        }
        #endregion
    }
}