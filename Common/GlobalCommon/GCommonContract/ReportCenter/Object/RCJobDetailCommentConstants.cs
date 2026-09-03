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

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    public class RCJobDetailCommentConstants
    {
        private static string GetString(string enStr)
        {
            return enStr;
        }

        /// <summary>
        /// Access is denied.
        /// </summary>
        public static string AccessDenied
        {
            get
            {
                return GetString("Access is denied.");
            }
        }

        /// <summary>
        /// An error occured at {0}.{0}为节点url，title
        /// </summary>
        public static string CommonMessage1
        {
            get
            {
                return GetString("An error occured at {0}.");
            }
        }

        /// <summary>
        /// Get {0} failed.{0}为对应Setting名
        /// </summary>
        public static string CommonMessage2
        {
            get
            {
                return GetString("Get {0} failed.");
            }
        }
    }
}
