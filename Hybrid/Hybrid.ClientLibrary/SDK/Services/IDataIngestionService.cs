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
using AvePoint.Hybrid.ClientCore;
using AvePoint.RA.Contract.DataIngestion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface IDataIngestionService
    {
        [Api(Url = "api/dataingestion/sendmessage", HttpMethod = "POST")]
        Task<RMDataIngestionMessageSendReceipt> SendMessage(RMDataIngestionMessageDto message);
        
        [Api(Url = "api/dataingestion/notifyanalyzedfinish", HttpMethod = "POST")]
        Task<RMDataIngestionMessageSendReceipt> NotifyAnalyzedFinish(RMDataIngestionMessageDto message);

        [Api(Url = "api/dataingestion/generateblobreference", HttpMethod = "POST")]
        Task<RMDataIngestionBlobReference> GenerateBlobReference(RMDataIngestionBlobNamingContext blobNamingContext);

        [Api(Url = "api/dataingestion/generateblobsasuri", HttpMethod = "GET")]
        Task<string> GenerateBlobSasUri(RMDataIngestionType ingestionType, string blobName);

        [Api(Url = "api/dataingestion/getingestionexecutionresults", HttpMethod = "GET")]
        Task<RMDataIngestionExecutionResult> GetIngestionExecutionResult(string uniqueId, string messageId);

        [Api(Url = "api/dataingestion/deleteblob", HttpMethod = "POST")]
        Task<bool> DeleteBlobByName(RMDataIngestionBlobDto blobDto);
    }
}
