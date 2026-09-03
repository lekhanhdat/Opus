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
    public class CustomizeConnectorApiResponse: BaseConnectorApiResponse
    {  
        public List<CustomizeConnectorApiFailedItem> failedItems { get; set; } = new List<CustomizeConnectorApiFailedItem>();
        public static CustomizeConnectorApiResponse SomeDataOperationFailed(List<CustomizeConnectorApiFailedItem> failedItems)
        {
            return new CustomizeConnectorApiResponse
            {
                statusCode = CustomizeConnectorApiResponseStatusCode.SomeDataOperationFailed,
                message = I18NEntity.GetString("RM_Connector_Validate_ItemFailed"),
                failedItems = failedItems
            };
        }

        public static CustomizeConnectorApiResponse ExistOperationFailedData(List<CustomizeConnectorApiFailedItem> failedItems)
        {
            return new CustomizeConnectorApiResponse
            {
                statusCode = CustomizeConnectorApiResponseStatusCode.ExistOperationFailedData,
                message = I18NEntity.GetString("RM_Connector_Validate_ItemFailed"),
                failedItems = failedItems
            };
        }
    }

    public class CustomizeConnectorApiFailedItem
    {
        public object Item { get; set; }

        public string Message { get; set; }
    }
}
