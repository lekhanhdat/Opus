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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPerformance
    {
        #region ==Net==
        [DataMember]
        public long TotalBytesReceived { get; set; }

        [DataMember]
        public long TotalReadTime { get; set; }

        [DataMember]
        public long TotalBytesSent { get; set; }

        [DataMember]
        public long TotalWriteTime { get; set; }
        #endregion 

        #region ==IO==
        [DataMember]
        public long TotalWriteBytes { get; set; }

        [DataMember]
        public long TotalWriteTicks { get; set; }

        [DataMember]
        public long TotalReadBytes { get; set; }

        [DataMember]
        public long TotalReadTicks { get; set; }

        //the information of physical device used in this subjob
        [DataMember]
        public List<string> PhysicalDeviceDescriptions { get; set; }
        #endregion

        #region ==Transfer==
        [DataMember]
        public Int64 DataTransferIn { get; set; }

        [DataMember]
        public Int64 DataTransferOut { get; set; } 
        #endregion

        public override string ToString()
        {
            StringBuilder buidler = new StringBuilder();
            buidler.Append("[TotalBytesReceived]:" + TotalBytesReceived + ",");
            buidler.Append("[TotalBytesSent]:" + TotalBytesSent + ",");
            buidler.Append("[TotalReadTime]:" + TotalReadTime + ",");
            buidler.Append("[TotalWriteTime]:" + TotalWriteTime + ",");
            buidler.Append("[TotalWriteBytes]:" + TotalWriteBytes + ",");
            buidler.Append("[TotalReadBytes]:" + TotalReadBytes + ",");
            buidler.Append("[TotalWriteTicks]:" + TotalWriteTicks + ",");
            buidler.Append("[TotalReadTicks]:" + TotalReadTicks);
            return buidler.ToString();
        }
    }
}
