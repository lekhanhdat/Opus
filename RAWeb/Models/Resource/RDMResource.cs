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
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Models.Resource
{
    public class RDMResource : BaseResource
    {
        public override List<ResourceItem> Get()
        {
            return new List<ResourceItem>()
            {
                    new ResourceItem()
                {
                    Key = ResourceKeys.RDM_RuleManagement,
                    Value = ResourceKeys.RDM_RuleManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                    SOPermission = RMSOPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_RuleManagementOld,
                    Value = ResourceKeys.RDM_RuleManagementOld.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                    SOPermission = RMSOPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_CreateRule,
                    Value = ResourceKeys.RDM_CreateRule.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                    SOPermission = RMSOPermissionMasks.RuleManagementEnduser,

                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_EditRule,
                    Value = ResourceKeys.RDM_EditRule.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                    SOPermission = RMSOPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_ManualApprovalReview,
                    Value = ResourceKeys.RDM_ManualApprovalReview.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ManualReviewEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_ManualApprovalReviews,
                    Value = ResourceKeys.RDM_ManualApprovalReviews.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ManualReviewEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_MAProcessesManagement,
                    Value = ResourceKeys.RDM_MAProcessesManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_ViewWorkFlow,
                    Value = ResourceKeys.RDM_ViewWorkFlow.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_CreateWorkFlow,
                    Value = ResourceKeys.RDM_CreateWorkFlow.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.RuleManagementEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RDM_ApprovalSetting,
                    Value = ResourceKeys.RDM_ApprovalSetting.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.ManualApprovalSettingEndUser,
                },
            };
        }
    }
}