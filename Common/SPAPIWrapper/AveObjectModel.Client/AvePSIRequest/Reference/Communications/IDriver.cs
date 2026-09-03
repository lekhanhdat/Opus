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
    using Microsoft.Office.Project.Server.Schema;
    using System;
    using System.ServiceModel;

    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Driver/", Name = "Driver")]
    public interface IDriver
    {
        // Token: 0x060000CD RID: 205
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/CreateDriver", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/CreateDriverResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void CreateDriver(DriverDataSet dsDriver);

        // Token: 0x060000CE RID: 206
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadDriverList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadDriverListResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        DriverDataSet ReadDriverList();

        // Token: 0x060000CF RID: 207
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadDriver", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadDriverResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        DriverDataSet ReadDriver(Guid driverUid);

        // Token: 0x060000D0 RID: 208
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/DeleteDrivers", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/DeleteDriversResponse")]
        void DeleteDrivers(Guid[] driverUids);

        // Token: 0x060000D1 RID: 209
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/UpdateDriver", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/UpdateDriverResponse")]
        DriverDataSet UpdateDriver(DriverDataSet dsDriver);

        // Token: 0x060000D2 RID: 210
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadPrioritization", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadPrioritizationResponse")]
        DriverPrioritizationDataSet ReadPrioritization(Guid prioritizationUid);

        // Token: 0x060000D3 RID: 211
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/CreatePrioritization", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/CreatePrioritizationResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        DriverPrioritizationDataSet CreatePrioritization(DriverPrioritizationDataSet driverPrioritizationDataSet);

        // Token: 0x060000D4 RID: 212
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/DeletePrioritizations", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/DeletePrioritizationsResponse")]
        void DeletePrioritizations(Guid[] prioritizationUids);

        // Token: 0x060000D5 RID: 213
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/UpdatePrioritization", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/UpdatePrioritizationResponse")]
        DriverPrioritizationDataSet UpdatePrioritization(DriverPrioritizationDataSet driverPrioritizationDataSet);

        // Token: 0x060000D6 RID: 214
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadPrioritizationList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Driver/ReadPrioritizationListResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        DriverPrioritizationDataSet ReadPrioritizationList();
    }
}
