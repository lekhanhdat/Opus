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

namespace AvePoint.RA.Contract.Common
{
    public class DueDateUtil
    {
        public const long NextJob = -1;
        public const long Pending = -2;
        public static long None = DateTime.MinValue.Ticks;

        public static long ConvertStringDueDate2Long(string dueDateStr)
        {
            switch (dueDateStr)
            {
                case null:
                case "":
                    return DueDateUtil.None;
                case "RM_JS_JM_EndTimePending":
                case "Pending":
                    return DueDateUtil.Pending;
                case "RDM_RecordsExporer_Status_NextJob":
                case "Next Job":
                    return DueDateUtil.NextJob;
                default:
                    long dueDateLong;
                    if (long.TryParse(dueDateStr, out dueDateLong))
                    {
                        DateTime dt = new DateTime(dueDateLong);
                        return dueDateLong;
                    }
                    else
                    {
                        throw new Exception("DueDate can not convert to long...");
                    }
            }

        }
        public static string ConvertLongDueDate2String(long dueDate)
        {
            switch (dueDate)
            {
                case 0:
                    return string.Empty;
                case DueDateUtil.Pending:
                    return "RM_JS_JM_EndTimePending";
                case DueDateUtil.NextJob:
                    return "RDM_RecordsExporer_Status_NextJob";
                default:
                    return dueDate.ToString();
            }
        }
    }
}
