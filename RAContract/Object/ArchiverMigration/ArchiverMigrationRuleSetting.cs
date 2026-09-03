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
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Object.ArchiverMigration
{
    public class ArchiverMigrationRuleSetting
    {
        public string Id { set; get; }
        public string SiteGroupName { set; get; }
        public string SiteUrl { get; set; }
        public NodeLevel Level { get; set; }
        public string NodeId { get; set; }
        public string ParentId { get; set; }
        public string WebId { get; set; }
        public string ListId { get; set; }
        public long SettingTime { get; set; }
        public bool IsEnableSuperUserDecrypt { get; set; }
        public bool IsIncludeManagedMetadataService { get; set; }
        public bool IsIncludeWorkflowDefinition { get; set; }
        public bool IsEnableRemoveRetentionLabel { get; set; }
        public int EnableArchiverManagement { get; set; }
        public string Url { get; set; }
        public int ContentSourceType { get; set; }

        public List<string> RuleIdList { get; set; }
        public bool IsScan { get; set; }
        public AMArchiverScheduleDto Schedule { get; set; }


        #region Need set in Opus. 
        // Get by SiteGroupName
        public Guid SiteGroupId { get; set; }
        // Get by site url
        public Guid SiteId { get; set; }
        #endregion
    }

    public class AMArchiverScheduleDto
    {
        public string Id { get; set; }
        public bool NoSchedule { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public long NextTime { get; set; }
        public string TimeZoneId { get; set; }
        public bool IsDaylightSaving { get; set; }
        public int EndType { get; set; }
        public int OccurrencesTotal { get; set; }
        public int Occurrences { get; set; }
        public int Interval { get; set; }
        public int IntervalType { get; set; }
        public int JobCategory { get; set; }
        public string ProfileId { get; set; }
        public string Extentions { get; set; }
    }
}
