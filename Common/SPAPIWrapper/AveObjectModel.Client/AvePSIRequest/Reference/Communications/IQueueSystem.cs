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
namespace Microsoft.Office.Project.Server.Interfaces
{
    using Microsoft.Office.Project.Server.Library;
    using Microsoft.Office.Project.Server.Schema;
    using System;
    using System.ServiceModel;

    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/", Name = "QueueSystem")]
    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    public interface IQueueSystem
    {
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/GetJobCompletionState", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/GetJobCompletionStateResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        QueueConstants.JobState GetJobCompletionState(Guid jobUID, out string errorString);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/GetJobWaitTime", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/GetJobWaitTimeResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        int GetJobWaitTime(Guid jobID);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/ReadJobStatusSimple", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/ReadJobStatusSimpleResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        QueueStatusDataSet ReadJobStatusSimple(Guid[] jobUIDs, bool includeWaitTime);

        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/UnblockCorrelation", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/UnblockCorrelationResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        void UnblockCorrelation(Guid correlationGUID);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/RetryJob", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/QueueSystem/RetryJobResponse")]
        void RetryJob(Guid JobGUID);
    }
}
