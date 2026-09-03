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
using System.Text;
using System.Threading.Tasks;

namespace  AvePoint.Hybrid.Utility
{
    public class DBTableHelper
    {
        public const string EVENT_JOB_PREFIX = "RECEM";
        public const string SCAN_JOB_PREFIX = "RECSCAN";
        public const string RENEW_WEBHOOK_JOB_PREFIX = "RECRENEW";
        public const string REGISTER_WEBHOOK_JOB_PREFIX = "RECREG";
        public const string NOTIFICATION_WEBHOOK_JOB_PREFIX = "RECNTF";

        public const string SQL_TABLE_JOB_PREFIX = "CSD_Job";
        public const string SQL_TABLE_EVENT_MESSAGE_PREFIX = "CSD_ItemEventMessage";
        public const string SQL_TABLE_EVENT_MESSAGE_PREFIX_RETIRED = "Retired-";

        private const int RANDOM_MIN = 100000;
        private const int RANDOM_MAX = 1000000;


        public static string GenerateJobId(string jobPrefix)
        {
            DateTime now = DateTime.UtcNow;
            Random r = new Random();
            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details: 未被使用
            * Ignore Reason: random用于生成jobid，不涉及安全问题 
            */
            string id = jobPrefix + now.ToString("yyyyMMddHHmmss") + r.Next(RANDOM_MIN, RANDOM_MAX);
            return id;
        }

        public static string GetJobDayTime(string jobId)
        {
            string dayTime = "";
            if (jobId.StartsWith(RENEW_WEBHOOK_JOB_PREFIX))
            {
                dayTime = jobId.Substring(RENEW_WEBHOOK_JOB_PREFIX.Length, 8);
            }
            else if (jobId.StartsWith(SCAN_JOB_PREFIX))
            {
                dayTime = jobId.Substring(7, 8);
            }
            else if (jobId.StartsWith(REGISTER_WEBHOOK_JOB_PREFIX))
            {
                dayTime = jobId.Substring(REGISTER_WEBHOOK_JOB_PREFIX.Length, 8);
            }
            else if (jobId.StartsWith(NOTIFICATION_WEBHOOK_JOB_PREFIX))
            {
                dayTime = jobId.Substring(NOTIFICATION_WEBHOOK_JOB_PREFIX.Length, 8);
            }
            else if (jobId.StartsWith(EVENT_JOB_PREFIX))
            {
                dayTime = jobId.Substring(5, 8);
            }

            return dayTime;
        }

        public static string GetJobTableByTime(DateTime time)
        {
            string name = SQL_TABLE_JOB_PREFIX + time.ToString("yyyyMMdd");
            return name;
        }

        public static string JobBlobPath(string jobId)
        {

            string path = GetJobDayTime(jobId) + @"\" + jobId + ".Context";
            return path;
        }

        public static string JobTable(string jobId)
        {
            string path = SQL_TABLE_JOB_PREFIX + GetJobDayTime(jobId);
            return path;
        }

        public static string GetEventMessageTableByTime(DateTime time)
        {
            string name = SQL_TABLE_EVENT_MESSAGE_PREFIX + time.ToString("yyyyMMdd");
            return name;
        }

        public static string GetTableNameByJobId(string jobId)
        {
            string tableName = null;
            if (jobId.StartsWith(EVENT_JOB_PREFIX))
            {
                tableName = SQL_TABLE_EVENT_MESSAGE_PREFIX + jobId.Substring(EVENT_JOB_PREFIX.Length, 8);
            }
            else if (jobId.StartsWith(SCAN_JOB_PREFIX))
            {
                tableName = SQL_TABLE_JOB_PREFIX + jobId.Substring(SCAN_JOB_PREFIX.Length, 8);
            }

            return tableName;
        }

    }
}
