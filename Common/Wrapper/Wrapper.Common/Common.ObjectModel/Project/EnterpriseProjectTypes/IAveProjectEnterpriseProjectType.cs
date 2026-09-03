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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public interface IAveProjectEnterpriseProjectType
    {
        string Description { get; set; }
        Guid Id { get; }
        string ImageUrl { get; set; }
        bool IsDefault { get; set; }
        bool IsManaged { get; set; }
        string Name { get; set; }
        int Order { get; set; }
        bool PermissionSyncEnable { get; set; }
        bool TaskListSyncEnable { get; set; }
        AveEnterpriseProjectTypeSiteCreationOptions SiteCreationOption { get; set; }
        string SiteCreationURL { get; set; }
        IAveProjectDetailPageCollection ProjectDetailPages { get; }
        Guid ProjectPlanTemplateId { get; set; }
        Guid WorkflowAssociationId { get; set; }
        string WorkflowAssociationName { get; set; }

        int WorkspaceTemplateLCID { get; set; }
        string WorkspaceTemplateName { get; set; }

        void Update();
        List<AveProjectDetailPageInfo> GetDetailPages();

        void UpdateEnterpriseTypeByPSI(AveProjectEnterpriseProjectTypeInfo eptInfo);
    }

    public enum AveEnterpriseProjectTypeSiteCreationOptions
    {
        NotSpecified = 0,
        AskOnPublish = 1,
        CreateOnFirstPublish = 2,
        None = 3
    }
}
