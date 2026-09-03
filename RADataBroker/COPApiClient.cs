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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Message;
using Cloud.Sdk.Data.Cop.Common;
using Cloud.Sdk.Data.Cop.DataDeletion;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CopApiUnitTest
{
    public class COPAPIClient
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(COPAPIClient));
        public static COPReturnMessage DataDeletion(COPReturnMessage message)
        {
            ArgumentCheck.NotNull(message, nameof(message));          
            var apiResult = AosApiUtility.CopClient.DataDeletionService.UpdateDeletionStatus(message?.RecordId, 
                new UpdateDeletionStatusModel() 
                {
                    RecordId = message.RecordId,
                    Product = message.Product,
                    Status = (DataDeletionStatus)message.Status
                }).Result;
            if (apiResult?.Type == Cloud.Sdk.Data.Cop.CopApiResultType.Failed)
            {
                logger.Error($"send cop api failed: {message?.RecordId}, {apiResult?.Message}");
            }
            return new COPReturnMessage() 
            {
                Type = (MessageType)apiResult?.Type,
                Message = apiResult?.Message
            };
        }

        public static async Task<List<ToBeDeletedCustomersResult>> GetToBeDeletedCustomers(DeletionType deletionType, ProductType productType, string dataCenter)
        {
            var parameters = new ToBeDeletedCustomersPara()
            {
                DeletionType = deletionType,
                Product = productType,
                DataCenter = dataCenter,
            };
            var result = await AosApiUtility.CopClient.DataDeletionService.GetCustomersToBeDeleted(parameters);
            return result;
        }

    }
}
