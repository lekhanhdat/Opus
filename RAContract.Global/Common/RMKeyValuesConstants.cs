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
    public static class RMKeyValuesConstants
    {
        public const string ArchiverBackupOutputStreamLevel = "ArchiverBackupOutputStreamLevel";    // 0: FileLevel, 4096: DataBlockLevel, Default is DataBlockLevel
        public const string RecordsBackupOutputStreamLevel = "RecordsBackupOutputStreamLevel";  // 0: FileLevel, 4096: DataBlockLevel, Default is FileLevel

        public const string ArchiverMigrationPreRunSRNJobPeriodInMinutes = "ArchiverMigrationPreRunSRNJobPeriodInMinutes";
        public const string PreviewFeature = "PreviewFeature";

        public const string EnableDeleteRestoredDataFeature = "ENABLE_DELETE_RESTORED_DATA_FEATURE";

        public const string AISmartTermMaxCount = "AI_SmartTerm_MaxCount";
        public const string EnableApplySettingAlwaysScanAll = "ContentCource_Enable_AlwaysScanAllOption";

        public const string RestoreExactSearchSiteConfig = "RestoreExactSearchSiteConfig";
        public const string DiscoveryShowPlanChat = "DISCOVERY_SHOW_PLAN_CHAT";


        public const string SUPER_PRIORITY_JOB_QUEUE_NAME = "SUPER_PRIORITY_JOB_QUEUE_NAME";

        public const string ENABLE_SUPER_PRIORITY_JOB_QUEUE = "EnableSuperPriorityJobQueue";
    }
}
