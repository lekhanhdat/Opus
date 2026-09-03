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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Archiver
{
    public class JobExecutionProcessStatictics
    {

        public ScanSummary ScanSummary { get; set; }

        //key : rule id ,value summary
        public IDictionary<String, RuleSummary> RuleSummaryDic { get; set; } = new Dictionary<String, RuleSummary>();
        //key : rule id ,value summary
        public IDictionary<String,ArchiveSummary> ArchiveSummaryDic { get; set; } = new Dictionary<String, ArchiveSummary>();
        //key : rule id ,value summary
        public IDictionary<String, DeleteAndStubSummary> DeleteAndStubSummaryDic { get; set; } = new Dictionary<String, DeleteAndStubSummary>();

    }

    public class RuleSummary
    {
        public String RuleId { get; set; }

        public String RuleName { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Rule Rule { get; set; }

        public Int64 MatchRuleFileCount { get; set; }

        public Int64 MatchRuleFileSize { get; set; }

        public Double MatchRuleFileGBSize { get; set; }

        public bool AlreadyCheckRuleRegionInfo { get; set; }
    }

    public class ScanSummary
    {
        public String JobNode { get; set; }
        public String NodeLevel { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public Int64 SiteCollectionFileCount { get; set; }
        public Int64 SiteCollectionSize { get; set; }
        public Double SiteCollectionGBSize { get; set; }
        public Int64 ScanMatchRuleFileCount { get; set; }
        public Int64 ScanMatchRuleFileSize { get;set; }
        public Double ScanMatchRuleFileGBSize { get; set; }

        public Int64 ScanMatchOthersFileCount { get; set; }
        public Int64 ScanMatchOthersFileSize { get; set; }
        public Double ScanMatchOthersFileGBSize { get; set; }
    }

    public class ArchiveSummary
    {
        public Int64 ArchivedFileCount { get; set; }

        public Int64 ArchivedFileSize { get;set; }

        public Double ArchivedFileGBSize { get; set; }

        public DateTime ArchiveStartTime { get; set; } = DateTime.MinValue;

        public DateTime ArchiveEndTime { get; set; } = DateTime.MinValue;
    }

    public class DeleteAndStubSummary
    {
        public long DeletedFileCount { get; set; }

        public long StubedFileCount { get; set; }

        public DateTime DeleteAndStubStartTime { get; set; } = DateTime.MinValue;

        public DateTime DeleteAndStubEndTime { get; set; } = DateTime.MinValue;
    }

    public class MainJobExecutionProcessStatictics
    {
        public string MainJobId { get; set; }
        public string O365TenantId { get; set; }
        public int UserSeats { get; set; }
        public int MaxRunSubJobCount { get; set; }
        public DateTime JobMonitorStartTime { get; set; }
        public DateTime FirstSubJobStartTime { get; set; }
        public DateTime LastSubJobEndTime { get; set; }
        public long ArchivedFileCount { get; set; }
        public long ArchivedFileSize { get; set; }
        public double ArchivedFileGBSize { 
            get => ArchivedFileSize / (double)(1024 * 1024 * 1024); 
            set { } 
        }
        public int SubJobCount { get; set; }
        public int CalCulatedSubJobCount { get; set; }
        public double ArchvieFileSpeedPerHour 
        {
            get
            {
                TimeSpan span = LastSubJobEndTime - FirstSubJobStartTime;
                double totalHours = span.TotalHours == 0 ? -1 : span.TotalHours;
                return ArchivedFileCount / totalHours;
            }
            set { } 
        }
        public double ArchiveSizeSpeedPerHour
        {
            get
            {
                TimeSpan span = LastSubJobEndTime - FirstSubJobStartTime;
                double totalHours = span.TotalHours == 0 ? -1 : span.TotalHours;
                return ArchivedFileSize / totalHours;
            }
            set { }
        }
    }



}
