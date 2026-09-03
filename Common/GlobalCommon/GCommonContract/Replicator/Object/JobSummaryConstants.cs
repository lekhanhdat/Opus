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



namespace AvePoint.GCommon.Contract.Replicator.Object
{
    #region using directives

    #endregion

    public class JobSummaryConstants
    {
        public const string RealTimePrefix = "RealTime";
        public const string GranularBackupJobPrefix = "FB";

        public const string SourceFarm = "Source Farm";
        public const string DestFarm = "Destination Farm";
        public const string SourceAgentHost = "Source Agent Host";
        public const string DestAgentHost = "Destination Agent Host";

        public const string Status = "Status";
        public const string StartTime = "StartTime";
        public const string FinishTime = "FinishTime";
        public const string Progress = "Progress";
        public const string Comment = "Comment";
        public const string WebAppCount = "WebAppCount";
        public const string FailedWebAppCount = "FailedWebAppCount";
        public const string SkippedWebAppCount = "SkippedWebAppCount";
        public const string SiteCollectionCount = "SiteCollectionCount";
        public const string FailedSiteCollectionCount = "FailedSiteCollectionCount";
        public const string SkippedSiteCollectionCount = "SkippedSiteCollectionCount";
        public const string SiteCount = "SiteCount";
        public const string FailedSiteCount = "FailedSiteCount";
        public const string SkippedSiteCount = "SkippedSiteCount";
        public const string ListCount = "ListCount";
        public const string FailedListCount = "FailedListCount";
        public const string SkippedListCount = "SkippedListCount";
        public const string ItemCount = "ItemCount";
        public const string FailedItemCount = "FailedItemCount";
        public const string SkippedItemCount = "SkippedItemCount";
        public const string DataSize = "DataSize";
    }
}
