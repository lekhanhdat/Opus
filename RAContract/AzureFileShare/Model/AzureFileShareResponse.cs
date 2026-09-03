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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.AzureFileShare.Model
{
    public class AzureFileShareResponse<T>
    {
        [JsonProperty("isSucceed")]
        public bool IsSucceed { get; set; }

        [JsonProperty("responseErrorType")]
        public AzureFileShareResponseErrorType ResponseErrorType { get; set; } = AzureFileShareResponseErrorType.None;

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        public static AzureFileShareResponse<T> Succeed()
        {
            return Succeed(default, string.Empty);
        }

        public static AzureFileShareResponse<T> Succeed(T result)
        {
            return Succeed(result, string.Empty);
        }

        public static AzureFileShareResponse<T> Succeed(T result, string message)
        {
            return new AzureFileShareResponse<T>
            {
                IsSucceed = true,
                ResponseMessage = message,
                Result = result,
            };
        }

        public static AzureFileShareResponse<T> Failed(AzureFileShareResponseErrorType errorType, T result)
        {
            return new AzureFileShareResponse<T>
            {
                IsSucceed = false,
                ResponseErrorType = errorType,
                ResponseMessage = string.Empty,
                Result = result
            };
        }

        public static AzureFileShareResponse<T> Failed(AzureFileShareResponseErrorType errorType, T result, string message)
        {
            return new AzureFileShareResponse<T> 
            { 
                IsSucceed = false,
                ResponseErrorType = errorType,
                ResponseMessage = message,
                Result = result
            };
        }

        public static AzureFileShareResponse<T> Generate(bool isSucceed, T result)
        {
            return new AzureFileShareResponse<T>
            {
                IsSucceed = isSucceed,
                ResponseErrorType = AzureFileShareResponseErrorType.None,
                ResponseMessage = string.Empty,
                Result = result
            };
        }

        public static AzureFileShareResponse<T> Generate(bool isSucceed, T result, AzureFileShareResponseErrorType errorType)
        {
            return new AzureFileShareResponse<T>
            {
                IsSucceed = isSucceed,
                ResponseErrorType = errorType,
                ResponseMessage = string.Empty,
                Result = result
            };
        }

        public static AzureFileShareResponse<T> Generate(bool isSucceed, T result, AzureFileShareResponseErrorType errorType, string message)
        {
            return new AzureFileShareResponse<T>
            {
                IsSucceed = isSucceed,
                ResponseErrorType = errorType,
                ResponseMessage = message,
                Result = result
            };
        }
    }
}
