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
    using Microsoft.Graph;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    public partial class MicrosoftGraphAPIService
    {

        public string ResourceUrl { get { return this.resourceUrl; } }
        private string resourceUrl = string.Empty;
        private Func<string> refreshAccessToken;//此方法需要能自动刷新token
        private bool useBeta;
        private GraphServiceClient graphServiceClient;
        public IRetryable RetryController { get; set; }

        //public MicrosoftGraphAPIService(string resourceUrl, string accessToken)
        //{
        //    this.resourceUrl = resourceUrl;
        //    this.accessToken = accessToken;
        //}

        public MicrosoftGraphAPIService(string resourceUrl, Func<string> refreshToken, IGLogger logger = null, bool useBeta = false)
        {
            Logger.Init(logger);
            this.resourceUrl = resourceUrl.TrimEnd('/');
            this.refreshAccessToken = refreshToken;
            this.graphServiceClient = new GraphServiceClient(HttpClientHelper.SdkClient, new AzureAuthenticationProvider(refreshToken),$"{this.resourceUrl}/v1.0");
            this.useBeta = useBeta;
        }
        /// <summary>
        ///1.Once batch request include no more than 20 sub-requests. 2.Total sub-requests can over 20.
        /// </summary>
        /// <param name="maxItemsCount"></param>
        /// <returns></returns>
        public IBatchRequestCollection CreateBatchRequestObj(Int32 eachBatchSize, bool useBeta = false)
        {
            return new GraphAPIBatchRequstCollection(eachBatchSize, BatchRequest, useBeta);
        }
        /// <summary>
        ///1.Once batch request include no more than 20 sub-requests. 2.Maximum number of sub-requests is 20.
        /// </summary>
        /// <returns></returns>
        public IBatchRequestCollection CreateBatchRequestObj(bool useBeta = false)
        {
            return new SimpleBatchRequestCollection(BatchRequest, useBeta);
        }
        /// <summary>
        /// The count of "RequestItem" in the "BatchRequestObj" must be no more than 20.
        /// </summary>
        /// <param name="batchRequestObj"></param>
        /// <returns></returns>
        internal List<ResponseItem> BatchRequest(BatchRequestObj batchRequestObj, bool useBeta = false)
        {
            var bRequest = new GraphBatchRequest(this.resourceUrl, this.refreshAccessToken, batchRequestObj, useBeta, RetryController);
            var bResponsesObj = bRequest.GetApiResult();
            return bResponsesObj.Responses.ToList();
        }

        public GraphUser GetUser(string upnOrId, string[] selectProperties = null)
        {
            var request = new GetUser(this.resourceUrl, this.refreshAccessToken, upnOrId, RetryController);
            if (selectProperties != null && selectProperties.Length > 0)
            {
                request.QueryParameters.Select(selectProperties);
            }
            return request.GetApiResult();
        }

        public GraphUser GetUser(string upnOrId, bool isIncludeDetail)
        {
            var requet = new GetUser(this.resourceUrl, this.refreshAccessToken, upnOrId, RetryController);
            if (isIncludeDetail)
            {
                requet.QueryParameters.Select(SelectProperties_UserDetailForDefinedGroup);
            }
            return requet.GetApiResult();
        }

        public GraphUser FindUser(string mail)
        {
            var request = new ListUser(this.resourceUrl, this.refreshAccessToken, RetryController);
            request.QueryParameters.Filter($"mail eq '{mail}'");
            return request.GetApiResult().FirstOrDefault();
        }

        public GraphUser GetUserBlock(string upnOrId)
        {
            var request = new GetUser(this.resourceUrl, this.refreshAccessToken, upnOrId, RetryController);
            request.QueryParameters.Select("accountEnabled");
            return request.GetApiResult();
        }

        public GraphUser Me
        {
            get
            {
                //todo:qlluo:add lazy
                return new Me(this.resourceUrl, this.refreshAccessToken, RetryController).GetApiResult();
            }
        }

        public IEnumerable<SubscribedSkus> GetSubscribedSkus()
        {
            return new GetSubscribedSkus(resourceUrl, refreshAccessToken, RetryController)
                .GetApiResult()
                ?.Value;
        }

        public IEnumerable<LicenseDetails> GetLicenseDetails(string userId)
        {
            return new GetLicenseDetails(resourceUrl, refreshAccessToken, RetryController, userId)
                .GetApiResult()
                ?.Value;
        }

        public Byte[] GetUserPhoto(string userId)
        {
            return new GetUserPhoto(resourceUrl, refreshAccessToken, RetryController, userId)
                .GetApiResult();
        }

        public IEnumerable<Group> GetOwnedObjects(string userId)
        {
            var result = new List<Group>();
            var listGroupsObj = new GetOwnedObjects(resourceUrl, refreshAccessToken, RetryController, userId).GetApiResult();
            result = GetNextGroupsObj(result, listGroupsObj);
            return result;
        }

        public IEnumerable<Group> GetMemberOf(string userId)
        {
            var result = new List<Group>();
            var listGroupsObj = new GetMemberOf(resourceUrl, refreshAccessToken, RetryController, userId).GetApiResult();
            result = GetNextGroupsObj(result, listGroupsObj);
            return result;
        }

        public List<Group> GetNextGroupsObj(List<Group> result, ListGroupsObj listGroupsObj)
        {
            if (listGroupsObj.Value != null)
            {
                result = listGroupsObj.Value.ToList().UnionBy(result, i => i.Id).ToList();
            }
            if (string.IsNullOrEmpty(listGroupsObj.OdataNextLink))
            {
                return result;
            }
            return GetNextGroupsObj(result, new GetNextGroupsObj(refreshAccessToken, RetryController, listGroupsObj.OdataNextLink).GetApiResult());
        }

        public DirectoryObject RestoreDeletedItem(string objectId)
        {
            return new RestoreDeletedItem(resourceUrl, refreshAccessToken, objectId, RetryController).GetApiResult();
        }
    }
    class AccessTokenProvider : IAccessTokenProvider
    {
        protected Func<string> RefreshAccessToken { get; set; }
        public AllowedHostsValidator AllowedHostsValidator => null;
        public AccessTokenProvider(Func<string> refreshAccessToken)
        {
            RefreshAccessToken = refreshAccessToken;
        }
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(RefreshAccessToken.Invoke());
        }
    }
    class AzureAuthenticationProvider : BaseBearerTokenAuthenticationProvider
    {
        public AzureAuthenticationProvider(Func<string> refreshAccessToken)
            :base(new AccessTokenProvider(refreshAccessToken))
        {
        }
    }
}