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
using Job.ModernManagement.Report;
using System;
using System.Collections.Concurrent;

namespace ExchangeCommonWrapper
{
    public class M365ObjectBackupSummary
    {
        public DateTime StartTime { get; set; }
        public ConcurrentDictionary<string, long> SuccessDic;
        public ConcurrentDictionary<string, long> FailedDic;
        public ConcurrentDictionary<string, long> SkippedDic;
        public ConcurrentDictionary<string, long> WarningDic;

        public M365ObjectBackupSummary()
        {
            SuccessDic = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            FailedDic = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            SkippedDic = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            WarningDic = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            StartTime = DateTime.UtcNow;
        }

        public void Add(ReportStatus status, string key, long value)
        {
            var dic = status switch
            {
                ReportStatus.Success => SuccessDic,
                ReportStatus.Failed => FailedDic,
                ReportStatus.Skipped => SkippedDic,
                ReportStatus.Warn => WarningDic,
                _ => null,
            };

            if (null != dic)
                Add(dic, key, value);

            void Add(ConcurrentDictionary<string, long> dic, string key, long value)
            {
                dic.AddOrUpdate(key, value, (k, v) => v + value);
            }
        }
    }
}
