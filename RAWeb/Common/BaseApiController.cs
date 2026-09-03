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


using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common
{
    [RMApiAuthorize]
    public class BaseApiController : ControllerBase
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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

        public FileStreamResult GetValidatedFile(Stream stream, string contentType, string fileName)
        {
            if ((SecurityUtils.IsValidFileName(fileName)))
            {
                return File(stream, contentType, fileName);
            }
            throw new Exception("Invalid file name");
        }
    }
}