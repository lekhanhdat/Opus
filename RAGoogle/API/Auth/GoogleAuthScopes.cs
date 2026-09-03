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
namespace RAGoogle.API
{
    public class GoogleAuthScopes
    {
        //readonly scope
        public static readonly string AdminDirectoryGroupReadonly = "https://www.googleapis.com/auth/admin.directory.group.readonly";
        public static readonly string AdminDirectoryUserReadonly = "https://www.googleapis.com/auth/admin.directory.user.readonly";
        public static readonly string AdminDirectoryDomainReadonly = "https://www.googleapis.com/auth/admin.directory.domain.readonly";

        public static readonly string ReportAuditReadOnly = "https://www.googleapis.com/auth/admin.reports.audit.readonly";
        public static readonly string AdminReportUsageReadOnly = "https://www.googleapis.com/auth/admin.reports.usage.readonly";
        //Write scope
        public static readonly string Drive = "https://www.googleapis.com/auth/drive";
        public static readonly string DriveLabelReadOnly = "https://www.googleapis.com/auth/drive.labels.readonly";
        public static readonly string DriveAdminLabelReadOnly = "https://www.googleapis.com/auth/drive.admin.labels.readonly";
        public static readonly string DriveLabel = "https://www.googleapis.com/auth/drive.labels";
        public static readonly string DriveAdminLabel = "https://www.googleapis.com/auth/drive.admin.labels";

        public static string[] DriveScopes
        {
            get { return _driveScopes; }
            set { _driveScopes = value; }
        }

        public static string[] DriveWithLabelScopes
        {
            get { return _driveWithLabelScopes; }
            set { _driveWithLabelScopes = value; }
        }

        public static string[] AdminScopes
        {
            get { return _adminScopes; }
            set { _adminScopes = value; }
        }

        public static string[] ReportScope
        {
            get { return _reportScope; }
            set { _reportScope = value; }
        }


        private static string[] _driveScopes = [
            Drive
        ];

        private static string[] _driveWithLabelScopes = [
            Drive,
            DriveLabel,
            DriveAdminLabel,
        ];

        private static string[] _adminScopes = [
            AdminDirectoryGroupReadonly,
            AdminDirectoryUserReadonly,
            AdminDirectoryDomainReadonly,
            Drive
        ];

        private static string[] _reportScope = [
            ReportAuditReadOnly,
            AdminReportUsageReadOnly
        ];
    }
}
