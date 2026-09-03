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
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.CustomizeConnector.Model.Api
{
    public class BaseConnectorApiV1Response
    {
    }

    public class SuccessResponse : BaseConnectorApiV1Response
    {
        public CustomizeConnectorApiResponseStatusCode statusCode { get; set; }
        public string message { get; set; }

        public static BaseConnectorApiV1Response OK()
        {
            return new SuccessResponse
            {
                statusCode = CustomizeConnectorApiResponseStatusCode.OK
            };
        }

        public static BaseConnectorApiV1Response Created()
        {
            return new SuccessResponse
            {
                statusCode = CustomizeConnectorApiResponseStatusCode.Created
            };
        }

        public static BaseConnectorApiV1Response NoContent()
        {
            return new SuccessResponse
            {
                statusCode = CustomizeConnectorApiResponseStatusCode.NoContent
            };
        }
    }

    public class ErrorResponse : BaseConnectorApiV1Response
    {
        public Error error { get; set; }

        public static BaseConnectorApiV1Response BadRequest(string message)
        {
            return new ErrorResponse
            {
                error = new Error()
                {
                    statusCode = CustomizeConnectorApiResponseStatusCode.BadRequest,
                    message = message
                }
            };
        }

        public static BaseConnectorApiV1Response InternalServerError(string message)
        {
            return new ErrorResponse
            {
                error = new Error()
                {
                    statusCode = CustomizeConnectorApiResponseStatusCode.InternalServerError,
                    message = message
                }
            };
        }
    }

    public class ExceptionResponse : BaseConnectorApiV1Response
    {
        public Error error { get; set; }
        public List<CustomizeConnectorApiFailedItem> failedItems { get; set; } = new List<CustomizeConnectorApiFailedItem>();
        public static BaseConnectorApiV1Response ExistOperationFailedData(List<CustomizeConnectorApiFailedItem> failedItems)
        {
            return new ExceptionResponse
            {
                error = new Error
                {
                    statusCode = CustomizeConnectorApiResponseStatusCode.ExistOperationFailedData,
                    message = I18NEntity.GetString("RM_Connector_Validate_ItemFailed"),
                },
                failedItems = failedItems
            };
        }
    }

    public class QueryResponse : BaseConnectorApiV1Response
    {
        public string startIndex { get; set; }
        public CustomizeConnectorApiResponseStatusCode statusCode { get; set; }
        public List<ExpandoObject> queriedItems { get; set; } = new List<ExpandoObject>();
        public static BaseConnectorApiV1Response QueryResult(List<ExpandoObject> queriedItems, string pageIndex)
        {
            return new QueryResponse
            {
                statusCode = queriedItems.Count == 0 ? CustomizeConnectorApiResponseStatusCode.NoContent : CustomizeConnectorApiResponseStatusCode.OK,
                startIndex = pageIndex,
                queriedItems = queriedItems
            };
        }
    }

    public class Error
    {
        public CustomizeConnectorApiResponseStatusCode statusCode { get; set; }
        public string message { get; set; }
    }
}
