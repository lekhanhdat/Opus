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
using System.Text;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMAgentFSJMRestoreSummaryDetails : JMJobDetails
    {
        public List<AgentActionStatistics> ActionStatistics { get; set; }
    }

    public class AgentActionStatistics
    {
        public int ActionTab { get; set; }
        public JobStatus Status
        {
            get
            {
                if ((SuccessfulObj.TotleCount > 0 || SuccessfulObj.BoxTotalCount > 0 || SuccessfulObj.TeamsTotalCount > 0)
                    && (FailedObj.TotleCount > 0 || FailedObj.BoxTotalCount > 0 || FailedObj.TeamsTotalCount > 0))
                {
                    return JobStatus.FinishWithException;
                }
                else if (FailedObj.TotleCount > 0 || FailedObj.BoxTotalCount > 0 || FailedObj.TeamsTotalCount > 0)
                {
                    return JobStatus.Failed;
                }
                else if (FailedObj.ExceptionCount > 0)
                {
                    return JobStatus.FinishWithException;
                }
                else
                {
                    return JobStatus.Finished;
                }
            }
        }
        public AgentObjectStatistic SuccessfulObj { get; set; }
        public AgentObjectStatistic FailedObj { get; set; }
        public AgentObjectStatistic SkippedObj { get; set; }
        public long Size { get; set; }
        public long DeleteSize { get; set; }
        public AgentActionStatistics()
        {
            SuccessfulObj = new AgentObjectStatistic();
            FailedObj = new AgentObjectStatistic();
            SkippedObj = new AgentObjectStatistic();
        }

        public AgentActionStatistics Clone()
        {
            //DeepCopy
            var temp = this.MemberwiseClone() as AgentActionStatistics;
            temp.SuccessfulObj = SuccessfulObj.ShallowCopy();
            temp.SkippedObj = SkippedObj.ShallowCopy();
            temp.SkippedObj = SkippedObj.ShallowCopy();
            return temp;
        }
    }

    public class AgentObjectStatistic
    {
        public long TotleCount { get { return SiteCollectionCount + SiteCount + ListCount + FolderCount + ItemCount; } }
        public long SiteCollectionCount { get; set; }
        public long SiteCount { get; set; }
        public long ListCount { get; set; }
        public long FolderCount { get; set; }
        public long ItemCount { get; set; }
        public long ExceptionCount { get; set; }

        // For Box content source
        public long BoxTotalCount { get { return ConnectionCount + UserCount + FolderCount + FileCount; } }
        public long ConnectionCount { get; set; }
        public long UserCount { get; set; }
        public long FileCount { get; set; }

        // For Google content source
        public long DriveTotalCount { get { return DriveCount + FolderCount + ItemCount; } }
        public long DriveCount { get; set; }

        //For Salesforce source

        public long SObjectCount { get; set; }

        #region For Teams source

        public long TeamsTotalCount
        {
            get
            {
                return TeamsGroupCount
                     + ChannelCount
                     + ChannelConversationCount
                     + GroupMailboxCount
                     + GroupMailboxItemCount
                     + SiteCollectionCount
                     + SiteCount
                     + ListCount
                     + FolderCount
                     + ItemCount
                     + PlanCount
                     + TaskCount
                     + AttachmentCount;
            }
        }

        public long TeamsGroupCount { get; set; }
        public long ChannelCount { get; set; }
        public long ChannelConversationCount { get; set; }
        public long GroupMailboxCount { get; set; }
        public long GroupMailboxItemCount { get; set; }
        public long PlanCount { get; set; }
        public long TaskCount { get; set; }
        public long AttachmentCount { get; set; }

        #endregion

        public AgentObjectStatistic ShallowCopy()
        {
            return this.MemberwiseClone() as AgentObjectStatistic;
        }
    }
}
