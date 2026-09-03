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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Models.Resource
{
    public class ReportCenterResource: BaseResource
    {
        public override List<ResourceItem> Get()
        {
            var forwardToDAORC = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter");
            return new List<ResourceItem>() 
            {
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_Dashboard,
                    Value = ResourceKeys.RC_Dashboard.ToUrl(),
                    Permission = RMPermissionMasks.ReportCenterAdmin,
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_DueDisposalReport_Management,
                    Value = ResourceKeys.RC_DueDisposalReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_DueDisposalReport_Profile,
                    Value = ResourceKeys.RC_DueDisposalReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_DueDisposalReport_ShowReport,
                    Value = ResourceKeys.RC_DueDisposalReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_DueDisposalReport_ViewDetail,
                    Value = ResourceKeys.RC_DueDisposalReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TimeFrameFileReport_Management,
                    Value = ResourceKeys.RC_TimeFrameFileReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.CreationAndDestructionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TimeFrameFileReport_Profile,
                    Value = ResourceKeys.RC_TimeFrameFileReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.CreationAndDestructionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TimeFrameFileReport_ShowReport,
                    Value = ResourceKeys.RC_TimeFrameFileReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.CreationAndDestructionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TimeFrameFileReport_ViewDetail,
                    Value = ResourceKeys.RC_TimeFrameFileReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.CreationAndDestructionEnduser
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_TermUsageReport_Management,
                    Value = ResourceKeys.RC_TermUsageReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TermUsageReport_Profile,
                    Value = ResourceKeys.RC_TermUsageReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TermUsageReport_ShowReport,
                    Value = ResourceKeys.RC_TermUsageReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TermUsageReport_ViewDetail,
                    Value = ResourceKeys.RC_TermUsageReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_AvailableSpaceReport_Management,
                    Value = ResourceKeys.RC_AvailableSpaceReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                    ReportPermission = RMReportPermissionMasks.AvailableSpaceEndUser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_AvailableSpaceReport_Profile,
                    Value = ResourceKeys.RC_AvailableSpaceReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                    ReportPermission = RMReportPermissionMasks.AvailableSpaceEndUser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_AvailableSpaceReport_ShowReport,
                    Value = ResourceKeys.RC_AvailableSpaceReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                    ReportPermission = RMReportPermissionMasks.AvailableSpaceEndUser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_AvailableSpaceReport_ViewDetail,
                    Value = ResourceKeys.RC_AvailableSpaceReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                    ReportPermission = RMReportPermissionMasks.AvailableSpaceEndUser
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_RuleUsageReport_Management,
                    Value = ResourceKeys.RC_RuleUsageReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.RuleUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_DueDisposalReport_Create,
                    Value = ResourceKeys.RC_DueDisposalReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_DueDisposalReport_Edit,
                    Value = ResourceKeys.RC_DueDisposalReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.ContentDueForActionEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_CreateAndDestryoedReport_Create,
                    Value = ResourceKeys.RC_CreateAndDestryoedReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_CreateAndDestryoedReport_Edit,
                    Value = ResourceKeys.RC_CreateAndDestryoedReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TermUsageReport_Create,
                    Value = ResourceKeys.RC_TermUsageReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_TermUsageReport_Edit,
                    Value = ResourceKeys.RC_TermUsageReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    ReportPermission = RMReportPermissionMasks.TermUsageEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_AuditReport_Management,
                    Value = ResourceKeys.RC_AuditReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionReport_Management,
                    Value = forwardToDAORC,
                    Permission = RMPermissionMasks.ReportCenterAdmin,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Management,
                    Value = ResourceKeys.RC_ActionAuditReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ReportCenterEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Profile,
                    Value = ResourceKeys.RC_ActionAuditReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_ShowReport,
                    Value = ResourceKeys.RC_ActionAuditReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_ViewDetail,
                    Value = ResourceKeys.RC_ActionAuditReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Create,
                    Value = ResourceKeys.RC_ActionAuditReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Edit,
                    Value = ResourceKeys.RC_ActionAuditReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser,
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Management,
                    Value = ResourceKeys.RC_ActionAuditReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Profile,
                    Value = ResourceKeys.RC_ActionAuditReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_ShowReport,
                    Value = ResourceKeys.RC_ActionAuditReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_ViewDetail,
                    Value = ResourceKeys.RC_ActionAuditReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Create,
                    Value = ResourceKeys.RC_ActionAuditReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ActionAuditReport_Edit,
                    Value = ResourceKeys.RC_ActionAuditReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    ReportPermission = RMReportPermissionMasks.ActionAuditEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_StorageOptimizationReport_Management,
                    Value = ResourceKeys.RC_StorageOptimizationReport_Management.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_StorageOptimizationReport_Profile,
                    Value = ResourceKeys.RC_StorageOptimizationReport_Profile.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_StorageOptimizationReport_ShowReport,
                    Value = ResourceKeys.RC_StorageOptimizationReport_ShowReport.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin,
                },
                new ResourceItem()//
                {
                    Key = ResourceKeys.RC_RestoreReport_Management,
                    Value = ResourceKeys.RC_RestoreReport_Management.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Profile,
                    Value = ResourceKeys.RC_RestoreReport_Profile.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_ShowReport,
                    Value = ResourceKeys.RC_RestoreReport_ShowReport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_ViewDetail,
                    Value = ResourceKeys.RC_RestoreReport_ViewDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Create,
                    Value = ResourceKeys.RC_RestoreReport_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Edit,
                    Value = ResourceKeys.RC_RestoreReport_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_ExportSiteMetricsReport_Generate,
                    Value = ResourceKeys.RC_ExportSiteMetricsReport_Generate.ToString(),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                },
                #region for only google
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Management,
                    Value = ResourceKeys.RC_RestoreReport_Management.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Profile,
                    Value = ResourceKeys.RC_RestoreReport_Profile.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_ShowReport,
                    Value = ResourceKeys.RC_RestoreReport_ShowReport.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_ViewDetail,
                    Value = ResourceKeys.RC_RestoreReport_ViewDetail.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Create,
                    Value = ResourceKeys.RC_RestoreReport_Create.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.RC_RestoreReport_Edit,
                    Value = ResourceKeys.RC_RestoreReport_Edit.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
                    ReportPermission = RMReportPermissionMasks.RestoredDataEnduser,
                },
                #endregion
            };
        }
    }
}