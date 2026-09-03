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
using System.Linq;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMJobDetails
    {
        public JobDetailsStatus Status { get; set; }
        public string Comment { get; set; }

    }

    public class JMJobDetailsCombineUtil
    {
        private static Dictionary<JobDetailsStatus, Func<JobDetailsStatus, JobDetailsStatus>> CONVERT_METHOD_DIC = new Dictionary<JobDetailsStatus, Func<JobDetailsStatus, JobDetailsStatus>>
        {
            {JobDetailsStatus.None, CombineNoneStatus},
            {JobDetailsStatus.Successful, CombineSuccessStatus},
            {JobDetailsStatus.Failed, CombineFailedStatus},
            {JobDetailsStatus.Skipped, CombineSkippedStatus},
            {JobDetailsStatus.Pending, CombinePendingStatus},
            {JobDetailsStatus.Exception, CombineExceptionStatus},
        };

        public static JobDetailsStatus CombineJobDetailStatus(params JobDetailsStatus[] jobDetailsStatus)
        {
            if (jobDetailsStatus.Length == 0)
            {
                return JobDetailsStatus.None;
            }
            else if (jobDetailsStatus.Length == 1)
            {
                return jobDetailsStatus[0];
            }
            else
            {
                JobDetailsStatus res = jobDetailsStatus[0];
                for (int i = 1; i < jobDetailsStatus.Length; i++)
                {
                    res = CONVERT_METHOD_DIC[res](jobDetailsStatus[i]);
                }
                return res;
            }
        }

        private static JobDetailsStatus CombineSuccessStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.None:
                case JobDetailsStatus.Successful:
                case JobDetailsStatus.Skipped:
                    return JobDetailsStatus.Successful;
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Pending:
                case JobDetailsStatus.Exception:
                    return JobDetailsStatus.Exception;
                default:
                    throw new Exception("out of JobDetailsStatus enum range");
            }
        }

        private static JobDetailsStatus CombineNoneStatus(JobDetailsStatus status)
        {
            return status;
        }

        private static JobDetailsStatus CombineFailedStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.None:
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Skipped:
                    return JobDetailsStatus.Failed;
                case JobDetailsStatus.Pending:
                case JobDetailsStatus.Exception:
                case JobDetailsStatus.Successful:
                    return JobDetailsStatus.Exception;
                default:
                    throw new Exception("out of JobDetailsStatus enum range");
            }
        }

        private static JobDetailsStatus CombineSkippedStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.None:
                    return JobDetailsStatus.Skipped;
                case JobDetailsStatus.Successful:
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Skipped:
                case JobDetailsStatus.Pending:
                case JobDetailsStatus.Exception:
                    return status;
                default:
                    throw new Exception("out of JobDetailsStatus enum range");
            }
        }

        private static JobDetailsStatus CombinePendingStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.None:
                    return JobDetailsStatus.Pending;
                case JobDetailsStatus.Successful:
                    return JobDetailsStatus.Exception;
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Skipped:
                case JobDetailsStatus.Pending:
                case JobDetailsStatus.Exception:
                    return status;
                default:
                    throw new Exception("out of JobDetailsStatus enum range");
            }
        }

        private static JobDetailsStatus CombineExceptionStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.None:
                case JobDetailsStatus.Successful:
                    return JobDetailsStatus.Exception;
                case JobDetailsStatus.Failed:
                    return status;
                case JobDetailsStatus.Skipped:
                case JobDetailsStatus.Pending:
                case JobDetailsStatus.Exception:
                    return JobDetailsStatus.Exception;
                default:
                    throw new Exception("out of JobDetailsStatus enum range");
            }
        }
    }

    public class JMJobDetailsCommon : JMJobDetails
    { 
        public long FileSize { get; set; }
    }

    [DataContract]
    public enum JobDetailsStatus
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Successful = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Skipped = 2,
        [EnumMember]
        Pending = 3,
        [EnumMember]
        Exception = 4,
        [EnumMember]
        ContainerFailed = 5
    }
    [DataContract]
    public enum ActionTab
    {
        //actions 0 - 29
        [EnumMember]
        None = -1,
        [EnumMember]
        Scan = 0,
        [EnumMember]
        Export = 1,
        [EnumMember]
        Backup = 2,
        [EnumMember]
        Action = 3,
        [EnumMember]
        Restore = 4,
        [EnumMember]
        Delete = 5,
        //settings 30 - 50
        [EnumMember]
        DOJobSettings = 30,
    }
    public enum TermSyncAction
    {
        New, Update, Delete, Skipped, Pending
    }
    public enum FailedType
    {
        ConfigColumn,
        ConfigClassification,
        ConfigPhysical
    }

    public enum RestoreConflictResolution
    {
        None = 0,
        Skip = 1,
        OverWrite = 2,
        Append = 3,
    }

    public enum ProgressStatus
    {
        Pending,
        Scan,
        Export,
        Archive,
        Others,
        Finished,
        Failed,
        FinishWithException,
        Stopped,
        Skipped,
        Hang,
    }
}