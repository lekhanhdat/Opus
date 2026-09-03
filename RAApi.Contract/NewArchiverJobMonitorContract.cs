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

using System.Collections.Generic;
using System.Runtime.Serialization;
namespace AvePoint.Api.Contract.Job
{
    #region JMJobSummary
    public class JMJobSummary
    {
        public string JobId { get; set; }
        public string ProfileName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string JobRunBy { get; set; }
        public NewArchiverJobStatus Status { get; set; }
        public string Scope { get; set; }
        public string Comment { get; set; }
        public RMJobSummaryInfos DisposalSummary { get; set; }
        public string ProgressSCStr { get; set; }
        public string ProgressFileCountStr { get; set; }
    }
    public class RMJobSummaryInfos
    {
        public string JobId { get; set; }
        public List<RMJobSummaryItem> SummaryItem { get; set; }
    }
    public class RMJobSummaryItem
    {
        public string Title { get; set; }

        public List<RMJobSummaryRow> SummaryRow { get; set; }

    }
    public class RMJobSummaryRow
    {
        //public RMSummaryRowType Type { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

    }

    #endregion

    #region JMJobDetails
    public class JMDetailsResult
    {
        public bool IsDeleted { get; set; }

        public int TotalNumber { get; set; }

        public bool Success { get; set; } = true;

        public IEnumerable<JMSOJobDetails> Details { get; set; }

    }
    public class JMJobDetails
    {
        public JobDetailsStatus Status { get; set; }
        public string Comment { get; set; }
    }
    public class JMSOJobDetails : JMJobDetails
    {
        public int ActionTab { get; set; }
        public string Level { get; set; }
        public string SourceLocation { get; set; }
        public string DestinationLocation { get; set; }
        public string Size { get; set; }
        public string SizeStr { get; set; }
        public string RuleName { get; set; }
        public long FinishTime { get; set; }
        public string FinishTimeStr { get; set; }
        public string Action { get; set; }
        public long FileSize { get; set; }
    }
    #endregion

    #region JMSOSummaryDetails
    public class JMSOSummaryDetails : JMJobDetails
    {
        public List<ActionStatistics> ActionStatistics { get; set; }
    }
    public class ActionStatistics
    {
        public int ActionTab { get; set; }
        public NewArchiverJobStatus Status { get; set; }
        public ObjectStatistic SuccessfulObj { get; set; }
        public ObjectStatistic FailedObj { get; set; }
        public ObjectStatistic SkippedObj { get; set; }
        public long Size { get; set; }
        public string SizeStr { get; set; }
        public long DeleteSize { get; set; }
        public string DeleteSizeStr { get; set; }
        public ActionStatistics()
        {
            SuccessfulObj = new ObjectStatistic();
            FailedObj = new ObjectStatistic();
            SkippedObj = new ObjectStatistic();
        }
    }
    public class ObjectStatistic
    {
        public long TotleCount { get; set; }
        public long SiteCollectionCount { get; set; }
        public long SiteCount { get; set; }
        public long ListCount { get; set; }
        public long FolderCount { get; set; }
        public long ItemCount { get; set; }
        public long ExceptionCount { get; set; }

        // For Box content source
        public long BoxTotalCount { get; set; }
        public long ConnectionCount { get; set; }
        public long UserCount { get; set; }
        public long FileCount { get; set; }
    }
    public enum JobDetailsStatus
    {
        None = -1,
        Successful = 0,
        Failed = 1,
        Skipped = 2,
        Pending = 3,
        Exception = 4,
    }

    [DataContract]
    public class JMDetailsQuery
    {
        [DataMember]
        public string JobID { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string[] SearcheKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public JobDetailsStatus[] StatusFilters { get; set; }
        [DataMember]
        public int[] EntityTypeFilters { get; set; }
        [DataMember]
        public ActionTab[] ActionTabFilters { get; set; }
    }

    #endregion

    #region Job Status

    public class JMJobInfo
    {
        public string Id { get; set; }
        public string JobId { get; set; }
        public string TaskName { get; set; }
        public string JobType { get; set; }
        public int JobTypeCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public NewArchiverJobStatus Status { get; set; }
        public int Progress { get; set; }
        public int ProfileId { get; set; }
        public string UserName { get; set; }
        public string LastUpdateTime { get; set; }
        public int NodeType { get; set; }
        public string Comment { get; set; }
        public int? MigrationJobStatus { get; set; }
        public string AdditionalInformation { get; set; }
        public string Joblocation { get; set; }
        public string SiteUrl { get; set; }

    }
    #endregion

    #region Job Enum

    public enum NewArchiverJobStatus
    {
        None = -1,
        Wait = 0,
        InProgress = 1,
        Finished = 2,
        Failed = 3,
        FinishWithException = 4,
        Stopped = 5,
        Skipped = 6,
        Stopping = 7,
        Calculating = 8,
        //Waiting for external resources
        Pending = 9,
    }
    public enum ActionTab
    {
        //actions 0 - 29
        None = -1,
        Scan = 0,
        Export = 1,
        Backup = 2,
        Action = 3,
        //settings 30 - 50
        DOJobSettings = 30,
    }
    #endregion
}
