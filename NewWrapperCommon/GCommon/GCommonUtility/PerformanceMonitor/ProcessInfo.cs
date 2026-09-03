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
    #endregion

    internal class ProcessInfo
    {
        public String IOOtherBytes { set; get; }

        public String IOWriteBytes { set; get; }

        public String IOReadBytes { set; get; }

        public String IOOther { set; get; }

        public String IOWrites { set; get; }

        public String IOReads { set; get; }

        public String GDIObjects { set; get; }

        public String UserObjects { set; get; }

        public String Threads { set; get; }

        public String Handles { set; get; }

        public String BasePri { set; get; }

        public String NPPool { set; get; }

        public String PagedPool { set; get; }

        public String VMSize { set; get; }

        public String PFDelta { set; get; }

        public String PageFaults { set; get; }

        public String MemDelta { set; get; }

        public String PeakMemUsage { set; get; }

        public String MemUsage { set; get; }

        public String CPUTime { set; get; }

        public String CPU { set; get; }

        public String SessionId { set; get; }

        public String UserName { set; get; }

        public String PID { set; get; }

        public String ImageName { set; get; }
    }
}
