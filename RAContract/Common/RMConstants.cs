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
namespace AvePoint.RA.Contract.Common
{
    public class RMConstants
    {

        public const string DEFAULT_SPSITES_GROUP = "Default_ SharePoint Sites_ Group";
        public const string DEFAULT_SKYDRIVEPROS_GROUP = "Default OneDrive for Business Group";
        public const string DEFAULT_O365_GROUP = "Default Office 365 Group Group";
        public const string DEFAULT_O365_SITES_GROUP = "Default Office 365 Group Sites Group";
        public const string DEFAULT_O365_GROUPS_GROUP = "Default_ O365_ Groups_ Group";
        public const string DEFAULT_MAILBOX_GROUP = "Default_ Mailbox_ Group";
        public const string DefaultProjectOnlineGroup = "Default_ProjectOnline_Sites_Group";
        public const string DefaultPrivateChannelSitesGroup = "Default Private Channel Sites Container";
        public const string DEFAULT_GOOGLE_USER_GROUP = "Default_ GoogleUser_ Group";
        public const string DEFAULT_GOOGLE_SHARED_DRIVE_GROUP = "Default_ Google_ SharedDrive_ Group";

        public const string DEFAULT_AGENT_GROUP = "Default_ Agent_ Group";

        public const string GUIDE_LINK_RELATED_APP = "https://cdn.avepoint.com/assets/webhelp/avepoint-opus/index.htm";
        public const string DEFAULT_PHYSICAL_DEVICE = "Default Physical Device";
        public const string DEFAULT_STORAGE_POLICY = "Default Storage Policy";
        public const int STORAGE_NEW_DATA_TYPE = 0;
        public const int STORAGE_DEVICE_DATA_ONLINE = 0;
        public const int STORAGE_OLD_DATA_TYPE = 1;
        public const string PASSWORD_RETURN_VALUE = "A!v@E#$p";
        public const string STUBFILENAMEMAPPING = "[StorageOptimization.Gui_9FE3A6A6-DB1B-478A-9C84-3793B070A958]";
        public const string STUBFILEPATHMAPPING = "[StorageOptimization.Gui_FB4CF4C0-AA67-43A7-9C37-97719E9B97A3]";
        public const string STUBARCHIVEDTIMEMAPPING = "[StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B]";
        public const string STUBRULENAMEMAPPING = "[StorageOptimization.Gui_AE414513-8007-44BC-98B9-8E6B1212C257]";
        public const string STUBRESTORELINKMAPPING = "[RM_AR_CP_Stub_Panel_RestoreLink]";
        public const string STUBCONTENT = "RM_AR_CP_Stub_Panel_StubContent";
        public const string STUBRETENTIONPERIOD = "RM_Audit_Stub_RetentionPeriod";
        public const string STUBEXTERNALLINKMAPPING = "|";
        public const string UNKNOW = "unknown";
        public const string DefaultPrivateChannelSitesGroupId = "41cfe969-e07b-45cb-a7d0-b022f967e929";

        public const string RETENTIONTYPE_EVENT = "Event";
        public const string RETENTIONTYPE_FLAT = "Flat";
        public const string ImportArchiveDataFolderName = "HSMBackup";

        public const string O365ArchiveStatus = "fullyArchived";

        public const int PreviewRestoreMaxSelectedObjectCount = 10;

        public const int PreviewRestorePerMinuteLimit = 5;

        public static string GetImportArchiveDataFolderName(string traceId, string folderName = ImportArchiveDataFolderName)
        {
            var normalizedFolderName = string.IsNullOrWhiteSpace(folderName)
                ? ImportArchiveDataFolderName
                : folderName.Trim().TrimEnd('/', '\\');

            return string.IsNullOrWhiteSpace(traceId)
                ? normalizedFolderName
                : string.Concat(normalizedFolderName, "/", traceId);
        }
    }
}
