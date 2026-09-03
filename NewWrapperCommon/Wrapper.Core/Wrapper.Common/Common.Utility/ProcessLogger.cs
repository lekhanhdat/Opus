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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class ProgressLogger
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ProgressLogger));

        private const int Limited = 1024 * 1024 * 1024;
        private bool needLog;
        private long total;
        private long totalInMB;
        private int lastProcess;

        public ProgressLogger(long total)
        {
            this.total = total;
            needLog = this.total >= Limited;
            totalInMB = this.total / (1024 * 1024);
            log.Debug("Total length: {0}B({1}MB) ", this.total, this.totalInMB > 0 ? this.totalInMB.ToString() : "<1");
        }

        public void LogOne(long current)
        {
            if (!needLog) return;
            int process = (int)((double)current * 100 / total);
            if ((process - lastProcess) / 10 > 0)
            {
                lastProcess = process;
                log.Debug("Total length: {0}MB, current length: {1}MB, process: {2}%", totalInMB, current / (1024 * 1024), process);
            }
        }
    }
}
