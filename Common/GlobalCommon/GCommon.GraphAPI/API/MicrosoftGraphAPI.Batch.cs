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
    using System.Net;
    using System.Threading;

    class GraphBatchRequest : PostRequest<BatchRequestObj, BatchResponseObj>
    {
        protected override string RequestUrl
        {
            get
            {
                return $"{(this.UseBetaAPI ? this.apiUrlBeta : this.apiUrlV1)}/$batch";
            }
        }

        public bool UseBetaAPI { get; set; }

        public GraphBatchRequest(string baseUrl, Func<string> getToken, BatchRequestObj batchRequestObj, IRetryable retryable)
            : this(baseUrl, getToken, batchRequestObj, false, retryable)
        {
        }

        public GraphBatchRequest(string baseUrl, Func<string> getToken, BatchRequestObj batchRequestObj, bool useBetaAPI, IRetryable retryable)
            : base(baseUrl, getToken, batchRequestObj, retryable)
        {
            UseBetaAPI = useBetaAPI;
        }
    }

    public interface IBatchRequestCollection
    {
        void Add(RequestItem requestItem);
        List<ResponseItem> SentRequest();
        Int32 NextIndex { get; }
        Int32 maxBatchSize { get; }
    }

    public class GraphAPIBatchRequstCollection : IBatchRequestCollection
    {
        public Int32 maxBatchSize => 20;
        private Int32 eachBatchSize;
        private List<BatchRequestObj> batchRequests;
        private List<RequestItem> tempRequestItems;
        private Func<BatchRequestObj, bool, List<ResponseItem>> batchRequest;
        private readonly bool useBetaApi;

        public Int32 NextIndex
        {
            get { return tempRequestItems.Count; }
        }
        GraphAPIBatchRequstCollection(Int32 eachBatchSize)
        {
            int absMaxCount = Math.Abs(eachBatchSize);
            this.eachBatchSize = absMaxCount > maxBatchSize ? maxBatchSize : absMaxCount;
            tempRequestItems = new List<RequestItem>(this.eachBatchSize);
            batchRequests = new List<BatchRequestObj>();
        }

        public GraphAPIBatchRequstCollection(Int32 eachBatchSize, Func<BatchRequestObj, bool, List<ResponseItem>> batchRequest, bool useBetaApi = false) : this(eachBatchSize)
        {
            this.batchRequest = batchRequest;
            this.useBetaApi = useBetaApi;
        }

        /// <summary>
        /// 每段 BatchRequestObj 中第一条 requestItem 不应该使用 DependsOn ，如果使用了也将被去除。
        /// </summary>
        /// <param name="requestItem"></param>
        public void Add(RequestItem requestItem)
        {
            if (NeedChangeNextCollection())
            {
                AddRequestToBatchList();
            }
            this.tempRequestItems.Add(requestItem);
        }
        public List<BatchRequestObj> GetBatchRequestList()
        {
            if (tempRequestItems.Any())
            {
                AddRequestToBatchList();
            }
            foreach (BatchRequestObj batchObj in batchRequests)
            {// 去除每个单独 BatchRequestObj 中第一条tempRequestItems 的 DependsOn 值，解决循环 Add() 时 DependsOn 不存在的 Id。
                if (null != batchObj.Requests && batchObj.Requests.Any()) batchObj.Requests[0].DependsOn = null;
            }
            return batchRequests;
        }
        public List<ResponseItem> SentRequest()
        {
            List<ResponseItem> responseItems = new List<ResponseItem>();
            foreach (var batchRequestObj in GetBatchRequestList())
            {
                responseItems.AddRange(batchRequest(batchRequestObj, useBetaApi));
            }
            return responseItems;
        }
        private void AddRequestToBatchList()
        {
            var requst = new BatchRequestObj() { Requests = tempRequestItems.ToArray() };
            batchRequests.Add(requst);
            this.tempRequestItems = new List<RequestItem>(this.eachBatchSize);
        }
        private bool NeedChangeNextCollection()
        {
            return this.NextIndex >= this.eachBatchSize;
        }
    }

    public class SimpleBatchRequestCollection : IBatchRequestCollection
    {
        public Int32 maxBatchSize => 20;
        private BatchRequestObj batchRequestObj;
        private Func<BatchRequestObj, bool, List<ResponseItem>> BatchRequest;
        private List<RequestItem> tempRequestItems;
        private readonly bool useBetaApi;

        public Int32 NextIndex
        {
            get { return tempRequestItems.Count; }
        }
        public SimpleBatchRequestCollection(Func<BatchRequestObj, bool, List<ResponseItem>> BatchRequest, bool useBetaApi = false)
        {
            this.BatchRequest = BatchRequest;
            batchRequestObj = new BatchRequestObj();
            tempRequestItems = new List<RequestItem>(20);
            this.useBetaApi = useBetaApi;
        }

        public void Add(RequestItem requestItem)
        {
            if (NextIndex < maxBatchSize)
            {
                this.tempRequestItems.Add(requestItem);
            }
            else
            {
                throw new ArgumentException("Maximum subrequests count in once batch request, pleace use overload method, if you want.");
            }
        }
        public void AddRange(List<RequestItem> requestItems)
        {
            if (NextIndex + requestItems.Count <= maxBatchSize)
            {
                this.tempRequestItems.AddRange(requestItems);
            }
            else
            {
                throw new ArgumentException("Maximum subrequests count in once batch request, pleace use overload method, if you want.");
            }
        }
        public List<ResponseItem> SentRequest()
        {
            batchRequestObj.Requests = tempRequestItems.ToArray();
            return BatchRequest(batchRequestObj, useBetaApi);
        }

        public void Clear()
        {
            tempRequestItems.Clear();
        }
    }

    public static class BatchRequestWithRetry
    {
        public static List<ResponseItem> Execute(IBatchRequestCollection request, int retryInterval = 3000, int retryCount = 5)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    var result = request.SentRequest();
                    var error = result.FirstOrDefault(r => !r.IsSuccessStatusCode);
                    if (error != null)
                    {
                        exceptions.Add(new BatchRequestException($"Batch request [{retry + 1}] error, status: {error.Status}, body: {error.Body}.", error.Status));
                        if (error.Status == HttpStatusCode.Unauthorized
                            || error.Status == HttpStatusCode.Forbidden
                            || error.Status == HttpStatusCode.NotFound)
                        {
                            break;
                        }
                        Thread.Sleep(retryInterval);
                        continue;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }

    [Serializable]
    public class BatchRequestException : Exception
    {
        public BatchRequestException(string message, HttpStatusCode httpStatusCode) : base(message) => HttpStatusCode = httpStatusCode;

        protected BatchRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public HttpStatusCode HttpStatusCode { get; private set; }
    }
}