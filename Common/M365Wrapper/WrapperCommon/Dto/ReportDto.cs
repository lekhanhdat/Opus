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

namespace ExchangeCommonWrapper
{
    using System;
    using System.ComponentModel;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using Job.ModernManagement.Report;

    public class ReportDto : JobReportDto
    {
        public ReportDto()
        {
            StartTime = DateTime.UtcNow.Ticks;
        }
        public ReportDto(string title, char type, long size, string path, string Message, ReportStatus status = ReportStatus.Success, JobReportDetailEntityType entityType = JobReportDetailEntityType.Objects)
        {
            Title = title;
            Status = status;
            Type = type;
            Size = size;
            Path = path;
            EntityType = entityType;
            Name = title;
            //Option = RestoreOption.NewCreated.GetEnumDescription();
            SourcePath = path;
            ErrorMessage = status == ReportStatus.Failed ? Message : string.Empty;
        }

        public DateTime FinishTime { get; set; }

        public bool IsFailed => Status == ReportStatus.Failed;

        public bool IsSuccess => Status == ReportStatus.Success;

        public bool IsSkipped => Status == ReportStatus.Skipped;

        public bool IsFiltered => Status == ReportStatus.Filtered;

        public bool IsWarning => Status == ReportStatus.Warn;

        public int? FailedCount { get; set; }
        public string MainLabel { get; set; }
    }

    public enum RestoreOption
    {
        [Description("Skipped")]
        Skipped,
        [Description("New Created")]
        NewCreated,
        [Description("Overwritten")]
        Overwritten,
        [Description("Appended")]
        Appended,
        [Description("Replaced")]
        Replaced,
        [Description("Updated")]
        Updated
    }
}