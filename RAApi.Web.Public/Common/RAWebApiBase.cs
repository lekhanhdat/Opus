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

using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Common
{
    //TODO fpwang AddFilter
    [Route("api/[controller]/[action]")]
    public class RAWebApiBase : ControllerBase
    {
        public RAWebApiBase() 
        {
           
        }

        protected Task<TResponse> RouteMultiGeoApiActionByConnectionIdAsync<TRequest, TResponse>(string partitionKeyId,
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> mainFunc)
        {
            return RAMultiGeoClient.RouteToDataCenterByConnectionIdAsync(
                partitionKeyId,
                requestBody,
                mainFunc,
                operationType);
        }

        protected Task<TResponse> RouteMultiGeoApiActionByConnectionIdAsync<TRequest, TResponse>(string partitionKeyId,
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> mainFunc,
        Func<MultiGeoErrorType, TResponse> createRejectedResponse)
        {
            return RAMultiGeoClient.RouteToDataCenterByConnectionIdAsync(
                partitionKeyId,
                operationType,
                requestBody,
                mainFunc,
                createRejectedResponse);
        }

        protected Task<TResponse> RouteMultiGeoApiActionByConnectionIdAsync<TResponse>(string partitionKeyId,
        MultiGeoOperationType operationType,
        Func<Task<TResponse>> mainFunc)
        {
            return RAMultiGeoClient.RouteToDataCenterByConnectionIdAsync(
                partitionKeyId,
                mainFunc,
                operationType);
        }

        protected Task<TResponse> SeparateRouteMultiGeoApiActionByPartitionKeysAsync<TRequest, TResponse>(string[] partitionKeys, MultiGeoOperationType operationType, TRequest request,
            Func<TRequest, Dictionary<string, IEnumerable<string>>, Dictionary<string, TRequest>> funcSeparateRequest,
            Func<TRequest, Task<TResponse>> mainFunc, Func<IEnumerable<TResponse>, TResponse> summaryResponeFunc)
        {
            return RAMultiGeoClient.SeperateRouteToDataCenterByPartitionKeysAsync(
                partitionKeys,
                operationType,
                request,
                funcSeparateRequest,
                mainFunc,
                summaryResponeFunc);
        }

        protected Task<TResponse> RouteMultiGeoApiActionAsync<TRequest, TResponse>(
           TRequest requestBody,
           MultiGeoOperationType operationType,
           Func<TRequest, Task<TResponse>> localAction,
           Func<TRequest, TResponse, Task> prepareReplicaRequest,
           Func<string, TResponse> createRejectedResponse)
        {
            return RAMultiGeoClient.RouteApiActionAsync(
                requestBody,
                operationType,
                localAction,
                prepareReplicaRequest,
                createRejectedResponse);
        }

        protected Task<TResponse> RouteMultiGeoApiActionAsync<TRequest, TResponse>(
            TRequest requestBody,
            MultiGeoOperationType operationType,
            Func<TRequest, Task<TResponse>> localAction,
            Func<string, TResponse> createRejectedResponse)
        {
            return RAMultiGeoClient.RouteApiActionAsync(
                requestBody,
                operationType,
                localAction,
                createRejectedResponse);
        }

        protected Task<TResponse> RouteMainDCApiActionAsync<TRequest, TResponse>(
            TRequest requestBody,
            MultiGeoOperationType mainDCOperationType,
            MultiGeoOperationType otherDCOperationType,
            Func<TRequest, Task<TResponse>> localAction)
        {
            return RAMultiGeoClient.PostCommonDataToMainDcAsync<TRequest, TResponse>(
                requestBody, mainDCOperationType, otherDCOperationType, localAction);
        }
    }
}
