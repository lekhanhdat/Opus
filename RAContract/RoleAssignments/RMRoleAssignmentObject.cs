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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RoleAssignments
{
    public enum RMRoleType
    {
        None = -1,
        StandardUser = 0,
        ApplicationAdmin = 1,
        DeligatedAdmin = 2,
        ReviewUser = 3,
        StandardReviewUser = 4, //StandardUser + ReviewUser
        ManageHoldUser = 5
    }

    public enum RMPermissionType
    {
        All,
        Reviewer,
        Common,
    }
    /// <summary>
    /// View 1,Edit 2, Create4 ,Delete 8 , totol F.
    /// Build-In Permission for check.
    /// Need sub permission mask when add new datasource.
    /// Current support it ,replace to acl string when too many permmasks?..
    /// </summary>
    [Flags]
    [LinkedToProduct(PaidForProduct.OpusIL, PaidForProduct.OpusGoogle)]
    public enum RMPermissionMasks : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "Access", Description = "common module access")]
        CommonModuleAccess = 0x1,

        [Display(GroupName = "Physical", Name = "Manager", Description = "manage all pyhsical data")]
        PhysicalAdmin = 0xF0,

        [Display(GroupName = "Physical", Name = "Enduser", Description = "Manager Physical data by permission")]
        PhysicalEndUser = 0x10,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "OneDrive", Description = "can access one drive")]
        OneDriveAdmin = 0xF00,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "OneDrive", Description = "can access one drive")]
        OneDriveEnduser = 0x100,

        [LinkedToFeature(PaidForModule.FileSystem)]
        [Display(GroupName = "Features", Name = "FileSystem", Description = "can access filesystem")]
        FSAdmin = 0xF000,

        [LinkedToFeature(PaidForModule.FileSystem)]
        [Display(GroupName = "Features", Name = "FileSystem", Description = "can access filesystem")]
        FSEnduser = 0x1000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "SharePoint", Description = "can access sharepoint")]
        SPOAdmin = 0xF0000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "SharePoint", Description = "can access sharepoint")]
        SPOEnduser = 0x10000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "Exchange", Description = "can access exchange")]
        EXOAdmin = 0xF00000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "Exchange", Description = "can access exchange")]
        EXOEnduser = 0x100000,

        [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
        ControlPanelAdmin = 0xF000000,

        [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
        ControlPanelEnduser = 0x1000000,

        [Display(GroupName = "Features", Name = "TermManagement", Description = "can access TermManagement")]
        TermManagementAdmin = 0xF0000000,

        [Display(GroupName = "Features", Name = "TermManagement", Description = "can access TermManagement")]
        TermManagementEnduser = 0x10000000,

        [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
        ContentRepositoyAdmin = 0xF00000000,

        [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
        ContentRepositoyEnduser = 0x100000000,

        [Display(GroupName = "Features", Name = "RecordExplorer", Description = "can access RecordExplorer")]
        EletricRecordExplorerAdmin = 0xF000000000,

        [Display(GroupName = "Features", Name = "RecordExplorerEndUser", Description = "can access RecordExplorer")]
        EletricRecordExplorerEnduser = 0x1000000000,

        [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
        RuleManagementAdmin = 0xF0000000000,

        [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
        RuleManagementEnduser = 0x10000000000,

        [Display(GroupName = "Features", Name = "ReportCenter", Description = "can access ReportCenter")]
        ReportCenterAdmin = 0xF00000000000,

        [Display(GroupName = "Features", Name = "ReportCenter", Description = "can access ReportCenter")]
        ReportCenterEnduser = 0x100000000000,

        [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
        JobMonitorAdmin = 0xF000000000000,

        [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
        JobMonitorEnduser = 0x1000000000000,

        [Display(GroupName = "Features", Name = "ManualReview", Description = "can access ManualReviewer")]
        ManualReviewAdmin = 0xF0000000000000,

        [Display(GroupName = "Features", Name = "ManualReview Enduser", Description = "can access ManualReviewer")]
        ManualReviewEnduser = 0x10000000000000,

        [LinkedToFeature(PaidForModule.SharePointOnPrem)]
        [Display(GroupName = "Features", Name = "SharePoint OnPrem", Description = "can access SharePoint OnPrem")]
        SPOnPremAdmin = 0xF00000000000000,

        [LinkedToFeature(PaidForModule.SharePointOnPrem)]
        [Display(GroupName = "Features", Name = "SharePoint OnPrem", Description = "can access SharePoint OnPrem")]
        SPOnPremEnduser = 0x100000000000000,

        [Display(GroupName = "Features", Name = "ManageHold", Description = "can access Manage Hold")]
        ManageHold = 0x1000000000000000,
        //[LinkedToFeature(PaidForModule.Box)]
        //[Display(GroupName = "Features", Name = "Box", Description = "can access Box")]
        //BoxAdmin = 0xF000000000000000,

        //[LinkedToFeature(PaidForModule.Box)]
        //[Display(GroupName = "Features", Name = "Box", Description = "can access Box")]
        //BoxEnduser = 0x1000000000000000,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,

    }

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusIL, PaidForProduct.OpusGoogle)]
    public enum RMSubPermissionMasks : long
    {
        [Display(GroupName = "Features", Name = "None", Description = "No physical management permission")]
        None = 0x0,

        [Display(GroupName = "PhysicalEndUser", Name = "SetAccessControl", Description = "Can set phyiscal access control ")]
        PhysicalAccessControl = 0x1,//1

        [Display(GroupName = "PhysicalEndUser", Name = "FolderCreationRequest", Description = "Can new folder creation request")]
        PhysicalFolderCreationRequest = 0x2,//10

        [Display(GroupName = "PhysicalEndUser", Name = "FolderLoanRequest", Description = "Can new folder loan request")]
        PhysicalFolderLoanRequest = 0x4,//100

        [Display(GroupName = "PhysicalEndUser", Name = "BoxCreationRequest", Description = "Can new box creation request")]
        PhysicalBoxCreationRequest = 0x8,//1000

        [Display(GroupName = "PhysicalEndUser", Name = "PhysicalFolderLoanReturn", Description = "Can return loaned folder")]
        PhysicalFolderLoanReturn = 0x10,//1 0000

        [Display(GroupName = "PhysicalEndUser", Name = "MoveRequest", Description = "Can new movement request")]
        PhysicalMoveRequest = 0x20,
    }

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusIL, PaidForProduct.OpusGoogle)]
    public enum RMPermissionExtensionMasks : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [LinkedToFeature(PaidForModule.AzureFiles)]
        [Display(GroupName = "Features", Name = "AzureFiles", Description = "can access azure files")]
        AzureFSAdmin = 0xF0,

        [LinkedToFeature(PaidForModule.AzureFiles)]
        [Display(GroupName = "Features", Name = "AzureFiles", Description = "can access azure files")]
        AzureFSEndUser = 0x10,

        [LinkedToFeature(PaidForModule.Box)]
        [Display(GroupName = "Features", Name = "Box", Description = "can access box")]
        BoxAdmin = 0xF00,

        [LinkedToFeature(PaidForModule.Box)]
        [Display(GroupName = "Features", Name = "Box", Description = "can access box")]
        BoxEndUser = 0x100,

        [LinkedToFeature(PaidForModule.Google)]
        [Display(GroupName = "Features", Name = "Google", Description = "can access Google")]
        GoogleAdmin = 0xF00000,

        [LinkedToFeature(PaidForModule.Google)]
        [Display(GroupName = "Features", Name = "Google", Description = "can access Google")]
        GoogleEndUser = 0x100000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "Teams", Description = "can access Teams")]
        TeamsAdmin = 0xF000000,

        [LinkedToFeature(PaidForModule.Office365)]
        [Display(GroupName = "Features", Name = "Teams", Description = "can access Teams")]
        TeamsEndUser = 0x1000000,

        [Display(GroupName = "Features", Name = "ManageHold", Description = "can access Manage Hold")]
        ManageHoldAdmin = 0xF0000000,

        [Display(GroupName = "Features", Name = "ManageHold", Description = "can access Manage Hold")]
        ManageHoldEndUser = 0x10000000,

        [Display(GroupName = "Features", Name = "ManualApproval", Description = "can access Manual Approval Setting")]
        ManualApprovalSettingAdmin = 0xF00000000,

        [Display(GroupName = "Features", Name = "ManualApproval", Description = "can access Manual Approval Setting")]
        ManualApprovalSettingEndUser = 0x100000000,
        //[LinkedToFeature(PaidForModule.Archiver)]
        //[Display(GroupName = "Features", Name = "Archiver", Description = "can access archiver")]
        //ArchiverAdmin = 0xF00,

        //[LinkedToFeature(PaidForModule.Archiver)]
        //[Display(GroupName = "Features", Name = "Recenter", Description = "can access recenter")]
        //Recenter = 0x100,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access all extension permission.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }
    [Flags]
    [LinkedToProduct(PaidForProduct.OpusIL, PaidForProduct.OpusGoogle,PaidForProduct.OpusSO)]
    public enum RMReportPermissionMasks : int
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0,

        //[Display(GroupName = "Features", Name = "ContentDueForAction", Description = "can access ContentDueForAction")]
        //ContentDueForActionAdmin = 0xF000000000,

        [Display(GroupName = "Features", Name = "ContentDueForAction", Description = "can access ContentDueForAction")]
        ContentDueForActionEnduser = 1,

        //[Display(GroupName = "Features", Name = "TermUsage", Description = "can access TermUsage")]
        //TermUsageAdmin = 0xF0000000000,

        [Display(GroupName = "Features", Name = "TermUsage", Description = "can access TermUsage")]
        TermUsageEnduser = 2,

        //[Display(GroupName = "Features", Name = "RuleUsage", Description = "can access RuleUsage")]
        //RuleUsageAdmin = 0xF00000000000,

        [Display(GroupName = "Features", Name = "RuleUsage", Description = "can access RuleUsage")]
        RuleUsageEnduser = 4,

        //[Display(GroupName = "Features", Name = "CreationAndDestruction", Description = "can access CreationAndDestruction")]
        //CreationAndDestructionAdmin = 0xF000000000000,

        [Display(GroupName = "Features", Name = "CreationAndDestruction", Description = "can access CreationAndDestruction")]
        CreationAndDestructionEnduser = 8,

        //[Display(GroupName = "Features", Name = "ActionAudit", Description = "can access ActionAudit")]
        //ActionAuditAdmin = 0xF0000000000000,

        [Display(GroupName = "Features", Name = "ActionAudit", Description = "can access ActionAudit")]
        ActionAuditEnduser = 16,

        //[Display(GroupName = "Features", Name = "RestoredData", Description = "can access RestoredData")]
        //RestoredDataAdmin = 0xF00000000000000,

        [Display(GroupName = "Features", Name = "RestoredData", Description = "can access RestoredData")]
        RestoredDataEnduser = 32,

        [Display(GroupName = "Features", Name = "AvailableSpace", Description = "can access RestoredData")]
        AvailableSpaceEndUser = 64,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access all extension permission.")]
        AccessAll = 127,
    }
    [Flags]
    public enum RMRoleUpgradeType : long
    {
        None = 0,
        UpgradePhysicalAction = 1,
        UpgradeGooglePermission = 2,
        UpgradeSubGooglePermission = 3,
        //next upgrade role 2,4,8 
    }

    //[Flags]
    //[LinkedToProduct(PaidForProduct.OpusSO)]
    //public enum RMSOPermissionMasks : long
    //{
    //    [LinkedToFeature(PaidForModule.Archiver)]
    //    [Display(GroupName = "Features", Name = "none", Description = "none access")]
    //    None = 0,

    //    [LinkedToFeature(PaidForModule.Archiver)]
    //    [Display(GroupName = "Features", Name = "Access", Description = "has archiver admin")]
    //    ArchiverAdmin = 0x1,
    //}

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusSO)]
    public enum RMSOPermissionMasks : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "Access", Description = "common module access")]
        CommonModuleAccess = 0x1,

        [Display(GroupName = "Features", Name = "OneDrive", Description = "can access one drive")]
        OneDriveAdmin = 0xF0,

        [Display(GroupName = "Features", Name = "OneDrive", Description = "can access one drive")]
        OneDriveEnduser = 0x10,

        [Display(GroupName = "Features", Name = "SharePoint", Description = "can access sharepoint")]
        SPOAdmin = 0xF00,

        [Display(GroupName = "Features", Name = "SharePoint", Description = "can access sharepoint")]
        SPOEnduser = 0x100,

        [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
        RuleManagementAdmin = 0xF000,

        [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
        RuleManagementEnduser = 0x1000,

        [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
        ControlPanelAdmin = 0xF0000,

        [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
        ControlPanelEnduser = 0x10000,

        [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
        JobMonitorAdmin = 0xF00000,

        [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
        JobMonitorEnduser = 0x100000,

        [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
        ContentRepositoyAdmin = 0xF000000,

        [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
        ContentRepositoyEnduser = 0x1000000,

        [Display(GroupName = "Features", Name = "RestoreCenter", Description = "Can search restore data")]
        RestoreCenterSearch = 0x10000000,               //0001 0000 0000 0000 0000 0000 0000 0000

        [Display(GroupName = "Features", Name = "RestoreCenter", Description = "Can search and expport restore data")]
        RestoreCenterExport = 0x30000000,               //0111 0000 0000 0000 0000 0000 0000 0000

        [Display(GroupName = "Features", Name = "RestoreCenter", Description = "Can all actions for restore data")]
        RestoreCenterFullControl = 0xF0000000,          //1111 0000 0000 0000 0000 0000 0000 0000

        [Display(GroupName = "Features", Name = "Teams", Description = "can access Teams")]
        TeamsAdmin = 0xF00000000,

        [Display(GroupName = "Features", Name = "Teams", Description = "can access Teams")]
        TeamsEndUser = 0x100000000,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access so every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusDiscovery)]
    public enum RMDiscoveryPermissionMasks : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "DiscoveryAndAnalysis", Description = "can access discovery and analysis")]
        DiscoveryAndAnalysis = 0xF,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access so every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }
    
    [Flags]
    [LinkedToProduct(PaidForProduct.OpusSalesforceDiscovery)]
    public enum RMDiscoverySalesforcePermissionMask : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "DiscoveryAndAnalysisForSalesforce", Description = "can access salesforce discovery and analysis")]
        DiscoveryAndAnalysis = 0xF,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access so every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusGoogleWorkspaceDiscovery)]
    public enum RMDiscoveryGoogleROTPermissionMask : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "DiscoveryAndAnalysisForGoogleROT", Description = "can access google ROT discovery and analysis")]
        DiscoveryAndAnalysis = 0xF,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access so every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }

    [Flags]
    [LinkedToProduct(PaidForProduct.OpusFileSystemDiscovery)]
    public enum RMDiscoveryFileSystemPermissionMask : long
    {
        [Display(GroupName = "Features", Name = "none", Description = "none access")]
        None = 0x0,

        [Display(GroupName = "Features", Name = "DiscoveryAndAnalysisForFileSystem", Description = "can access file system discovery and analysis")]
        DiscoveryAndAnalysis = 0xF,

        [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access every feature.")]
        AccessAll = 0x7FFFFFFFFFFFFFFF,
    }

    //[Flags]
    //[LinkedToProduct(PaidForProduct.OpusGoogle)]
    //public enum RMGooglePermissionMasks : long
    //{
    //    [Display(GroupName = "Features", Name = "none", Description = "none access")]
    //    None = 0x0,

    //    [Display(GroupName = "Features", Name = "Access", Description = "common module access")]
    //    CommonModuleAccess = 0x1,

    //    [LinkedToFeature(PaidForModule.Google)]
    //    [Display(GroupName = "Features", Name = "Google", Description = "can access Google")]
    //    GoogleAdmin = 0xF0,

    //    [LinkedToFeature(PaidForModule.Google)]
    //    [Display(GroupName = "Features", Name = "Google", Description = "can access Google")]
    //    GoogleEndUser = 0x10,

    //    [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
    //    RuleManagementAdmin = 0xF00,

    //    [Display(GroupName = "Features", Name = "RuleManagement", Description = "can access RuleManagement")]
    //    RuleManagementEnduser = 0x10,

    //    [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
    //    ControlPanelAdmin = 0xF000,

    //    [Display(GroupName = "Features", Name = "ControlPanel", Description = "can access ControlPanel")]
    //    ControlPanelEnduser = 0x1000,

    //    [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
    //    JobMonitorAdmin = 0xF0000,

    //    [Display(GroupName = "Features", Name = "JobMonitor", Description = "can access JobMonitor")]
    //    JobMonitorEnduser = 0x10000,

    //    [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
    //    ContentRepositoyAdmin = 0xF00000,

    //    [Display(GroupName = "Features", Name = "ContentRepositoy", Description = "can access ContentRepositoy")]
    //    ContentRepositoyEnduser = 0x100000,

    //    [Display(GroupName = "Features", Name = "ManualReview", Description = "can access ManualReviewer")]
    //    ManualReviewAdmin = 0xF000000,

    //    [Display(GroupName = "Features", Name = "ManualReview Enduser", Description = "can access ManualReviewer")]
    //    ManualReviewEnduser = 0x1000000,

    //    [Display(GroupName = "Features", Name = "TermManagement", Description = "can access TermManagement")]
    //    TermManagementAdmin = 0xF0000000,

    //    [Display(GroupName = "Features", Name = "TermManagement", Description = "can access TermManagement")]
    //    TermManagementEnduser = 0x10000000,

    //    [Display(GroupName = "Features", Name = "ReportCenter", Description = "can access ReportCenter")]
    //    ReportCenterAdmin = 0xF00000000,

    //    [Display(GroupName = "Features", Name = "ReportCenter", Description = "can access ReportCenter")]
    //    ReportCenterEnduser = 0x100000000,

    //    [Display(GroupName = "Features", Name = "RecordExplorer", Description = "can access RecordExplorer")]
    //    EletricRecordExplorerAdmin = 0xF000000000,

    //    [Display(GroupName = "Features", Name = "RecordExplorerEndUser", Description = "can access RecordExplorer")]
    //    EletricRecordExplorerEnduser = 0x1000000000,

    //    [Display(GroupName = "Physical", Name = "Manager", Description = "manage all pyhsical data")]
    //    PhysicalAdmin = 0xF0000000000,

    //    [Display(GroupName = "Physical", Name = "Enduser", Description = "Manager Physical data by permission")]
    //    PhysicalEndUser = 0x10000000000,

    //    [Display(GroupName = "SecurityGroup", Name = "AccessAll", Description = "This allows the user to access so every feature.")]
    //    AccessAll = 0x7FFFFFFFFFFFFFFF,
    //}

    //[Flags]
    //[LinkedToProduct(PaidForProduct.OpusGoogle)]
    //public enum RMSubGooglePermissionMasks : long
    //{
    //    [Display(GroupName = "Features", Name = "None", Description = "No physical management permission")]
    //    None = 0x0,

    //    [Display(GroupName = "PhysicalEndUser", Name = "SetAccessControl", Description = "Can set phyiscal access control ")]
    //    PhysicalAccessControl = 0x1,

    //    [Display(GroupName = "PhysicalEndUser", Name = "FolderCreationRequest", Description = "Can new folder creation request")]
    //    PhysicalFolderCreationRequest = 0x2,

    //    [Display(GroupName = "PhysicalEndUser", Name = "FolderLoanRequest", Description = "Can new folder loan request")]
    //    PhysicalFolderLoanRequest = 0x4,

    //    [Display(GroupName = "PhysicalEndUser", Name = "BoxCreationRequest", Description = "Can new box creation request")]
    //    PhysicalBoxCreationRequest = 0x8,

    //    [Display(GroupName = "PhysicalEndUser", Name = "PhysicalFolderLoanReturn", Description = "Can return loaned folder")]
    //    PhysicalFolderLoanReturn = 0x10,

    //}
}
