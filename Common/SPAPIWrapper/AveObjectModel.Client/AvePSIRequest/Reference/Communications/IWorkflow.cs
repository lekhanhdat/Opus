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
    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/", Name = "Workflow")]
    public interface IWorkflow
    {
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/ReadEnterpriseProjectType", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/ReadEnterpriseProjectTypeResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        WorkflowDataSet ReadEnterpriseProjectType(Guid enterpriseProjectTypeUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/UpdateEnterpriseProjectType", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/UpdateEnterpriseProjectTypeResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        WorkflowDataSet UpdateEnterpriseProjectType(WorkflowDataSet workflowDS);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/ReadEnterpriseProjectTypeList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Workflow/ReadEnterpriseProjectTypeListResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        WorkflowDataSet ReadEnterpriseProjectTypeList();
    }
}
