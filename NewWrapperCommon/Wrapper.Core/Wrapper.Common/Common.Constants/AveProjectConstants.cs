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

namespace AvePoint.Wrapper.Common
{
    public class AveProjectConstants
    {
        #region WSS

        public const int MAX_WORKSPACE_NAME = 128;
        public const int MAX_WORKSPACE_SERVER_RELATIVE_URL = 260;
        public const int MAX_WORKSPACE_SERVER_RELATIVE_DIR_NAME = 256;
        public const int ValidationNoError = 0;
        public const int ValidationURIInvalid = 1;
        public const int ValidationHierarchyNotValid = 2;
        public const int ValidationWorkspaceNameIsBlank = 3;
        public const int ValidationWorkspaceAlreadyExists = 4;
        public const int ValidationWorkspaceContainsIllegalChars = 5;
        public const int ValidationWorkspaceNameCannotStartOrEndWithPeriod = 6;
        public const int ValidationWorkspaceUrlOrPartsTooLong = 7;
        public const int WssPWSTemplateNumericIdMinLimit = 6000;
        public const int WssPWSDefaultTemplateNumericId = 6215;
        public const int WssPWSTemplateNumericIdMaxLimit = 6220;
        public const int WssPWADefaultTemplateNumericId = 6221;
        public const string WssPWSDefaultTemplateName = "ProjectSite#0";
        public const string ReportCenterLink = "ProjectReportCenterName";
        public const string WSSProjectWorkspaceProjectUidPropertyName = "MSPWAPROJUID";
        public const string WssProjectWorkspacePwaUrlPropertyName = "PWAURL";
        public const string WssProjectServerSPSiteIdPropertyName = "MSPWASITEUID";
        public const string WssProjectWorkspaceDistinguishedTaskListUidPropertyName = "DistinguishedListUid";
        public const int WssHResultMiscError = -2147217873;
        public const int DuplicateRoleGroupName = -2130575293;
        public const int RoleCannotBeFound = -2146232832;
        public const int WebDoesNotExist = -2147024894;
        public const int UrlContainsIllegalChars = -2146232832;
        public const int UrlPathTooLong = -2130245272;
        public const int UserOrGroupUnknown = -2146232832;
        public const int UserDoesNotExistsOrNotUnique = -2130575276;
        public const int CannotObtainWebTemplateInfo = -2130247159;
        public const int GroupAlreadyExists = -2146232060;
        public const string PWATemplateName = "PWA";
        public const string PWSTemplateName = "PWS";
        public const string ProjectSiteTemplateName = "PROJECTSITE";
        public const string SharepointWorkerProcessGroupName = "WSS_WPG";
        public static readonly Guid HierarchyTasksListFeatureUid = new Guid("F9CE21F8-F437-4f7e-8BC6-946378C850F0");
        public static readonly Guid ReportCenterLinkUid = new Guid("4BCEF614-EE25-42F4-8B27-B081BBA821B9");
        public static readonly Guid BusinessIntelligenceCenterLinkUid = new Guid("E7FC783D-3C3D-42d6-B482-085ECE22AABF");
        public static readonly Guid WssProjectWorkspaceFeatureUid = new Guid("90014905-433F-4a06-8A61-FD153A27A2B5");
        public static readonly Guid PwaRibbonFeatureUid = new Guid("1D253548-C70D-40fd-9930-9D313BEDC359");
        public static readonly Guid PWSManagedFeatureUid = new Guid("1A2B649C-B783-433F-80F6-A2CAE4584B88");
        public static readonly Guid PWSVisibilityFeatureUid = new Guid("E7656881-9C59-49B0-B95E-37852E7A803E");

        #endregion

        #region Workflow

        public static readonly Guid ProjectWorkflow_EventSourceId = new Guid("5122D555-E672-4E5D-A7C4-8084E694A257");

        #endregion

        #region TimeLine

        public static readonly Guid ServerTimelineUID = new Guid("778AE485-6C78-4D31-99B4-B5FFFF503C17");
        public static readonly Guid ProjectCenterUID = new Guid("38DD25C2-55F1-43FB-BC7B-E3E8F10A0314");
        public static readonly Guid SchedWebPartUID = new Guid("4D890797-9076-43B6-9444-0087C146E7B2");
        
        #endregion
    }
}
