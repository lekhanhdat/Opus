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
using System.Diagnostics;
using AvePoint.RA.Contract.ManualScaleIn;

namespace AvePoint.RA.AutoScale.RuleCheckers
{
    public class ProcessRuleChecker : IRMAutoScaleRuleChecker
    {
        private string mAgentServicePath;
        private string mBinParentFolderPath;
        private long mModifyValue;

        private static List<string> ProcessBlackList = new List<string>
        {
            "RecordsHotfixMaintenanceService.exe",
        };

        public long Result
        {
            get
            {
                return this.mModifyValue;
            }
        }

        public ProcessRuleChecker(string agentServicePath, string binParentFolderPath)
        {
            this.mAgentServicePath = agentServicePath;
            this.mBinParentFolderPath = binParentFolderPath;
        }

        private bool IsBlackListItem(string condition)
        {
            foreach (var item in ProcessBlackList)
            {
                if (condition.EndsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void Check(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return;
            }
            if (IsBlackListItem(condition))
            {
                return;
            }

            var value = condition.StartsWith(this.mAgentServicePath, StringComparison.OrdinalIgnoreCase)
                || condition.StartsWith(this.mBinParentFolderPath, StringComparison.OrdinalIgnoreCase);

            if (value)
            {
                this.mModifyValue = DateTime.UtcNow.Ticks;
            }
        }
    }
}
