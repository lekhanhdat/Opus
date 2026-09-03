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

    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Resource/", Name = "Resource")]
    public interface IResource
    {
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadResourceAuthorization", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadResourceAuthorizationResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        ResourceAuthorizationDataSet ReadResourceAuthorization(Guid resourceUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/SetResourceAuthorization", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/SetResourceAuthorizationResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void SetResourceAuthorization(ResourceAuthorizationDataSet users);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadDelegations", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadDelegationsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        UserDelegationDataSet ReadDelegations(UserDelegationConsts.DelegationFilter filter, Guid resUid);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/UpdateDelegations", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/UpdateDelegationsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void UpdateDelegations(UserDelegationDataSet ds);

        // Token: 0x060002CA RID: 714
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CreateDelegations", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CreateDelegationsResponse")]
        void CreateDelegations(UserDelegationDataSet ds);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CreateResources", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CreateResourcesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        ResourceDataSet CreateResources(ResourceDataSet rds, bool validateOnly, bool autoCheckIn);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/UpdateResources", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/UpdateResourcesResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        ResourceDataSet UpdateResources(ResourceDataSet rds, bool validateOnly, bool autoCheckIn);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/DeleteResources", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/DeleteResourcesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void DeleteResources(Guid[] arrayRes, string deletionComment);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CheckOutResources", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CheckOutResourcesResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void CheckOutResources(Guid[] array);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CheckInResources", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/CheckInResourcesResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void CheckInResources(Guid[] array, bool force);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadResource", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Resource/ReadResourceResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        ResourceDataSet ReadResource(Guid resourceUid);
    }
}
