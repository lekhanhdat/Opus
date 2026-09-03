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

    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Security/", Name = "Security")]
    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    public interface ISecurity
    {
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadTemplate", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadTemplateResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        SecurityTemplatesDataSet ReadTemplate(Guid templateUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadTemplateList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadTemplateListResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        SecurityTemplatesDataSet ReadTemplateList();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetTemplates", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetTemplatesResponse")]
        void SetTemplates(SecurityTemplatesDataSet templates);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateTemplates", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateTemplatesResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void CreateTemplates(SecurityTemplatesDataSet templates);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteTemplates", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteTemplatesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        void DeleteTemplates(Guid[] templateUids);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateCategories", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateCategoriesResponse")]
        void CreateCategories(SecurityCategoriesDataSet categories);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetCategories", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetCategoriesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        void SetCategories(SecurityCategoriesDataSet categories);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadCategory", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadCategoryResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        SecurityCategoriesDataSet ReadCategory(Guid categoryUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadCategoryList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadCategoryListResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        SecurityCategoriesDataSet ReadCategoryList();

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetGroups", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/SetGroupsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void SetGroups(SecurityGroupsDataSet group);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateGroups", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateGroupsResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void CreateGroups(SecurityGroupsDataSet groups);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadGroup", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadGroupResponse")]
        SecurityGroupsDataSet ReadGroup(Guid groupUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadGroupList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadGroupListResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        SecurityGroupsDataSet ReadGroupList();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteGroups", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteGroupsResponse")]
        void DeleteGroups(Guid[] groupUids);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateProjectCategories", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/CreateProjectCategoriesResponse")]
        void CreateProjectCategories(SecurityProjectCategoriesDataSet categories);

        // Token: 0x060002E8 RID: 744
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/UpdateProjectCategories", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/UpdateProjectCategoriesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        void UpdateProjectCategories(SecurityProjectCategoriesDataSet categories);

        // Token: 0x060002E9 RID: 745
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteProjectCategories", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/DeleteProjectCategoriesResponse")]
        void DeleteProjectCategories(Guid[] projUids);

        // Token: 0x060002EA RID: 746
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadProjectCategory", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Security/ReadProjectCategoryResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        SecurityProjectCategoriesDataSet ReadProjectCategory(Guid projUid);
    }
}
