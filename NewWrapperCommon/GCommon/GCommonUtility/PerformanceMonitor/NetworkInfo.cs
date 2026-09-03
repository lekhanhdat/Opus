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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public class NetworkInfo
    {
        public String AdapterName { get; internal set; }
        public String AdapterDescription { get; internal set; }
        public Int64 Speed { get; internal set; }

        public Int64 SentSpeed { get; internal set; }
        public Int64 ReceivedSpeed { get; internal set; }
        public Int64 TotalSpeed { get; internal set; }
        public Int32 NetworkUsage { get { return this.Speed > 0 ? Convert.ToInt32((this.TotalSpeed * 100.0) / this.Speed) : 0; } }
    }
}
