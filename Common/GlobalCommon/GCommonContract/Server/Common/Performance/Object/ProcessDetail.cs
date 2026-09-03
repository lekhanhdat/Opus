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




namespace AvePoint.GCommon.Contract.Server.Common.Performance
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProcessDetails
    {
        [DataMember]
        public String IOOtherBytes { set; get; }
        [DataMember]
        public String IOWriteBytes { set; get; }
        [DataMember]
        public String IOReadBytes { set; get; }
        [DataMember]
        public String IOOther { set; get; }
        [DataMember]
        public String IOWrites { set; get; }
        [DataMember]
        public String IOReads { set; get; }
        [DataMember]
        public String GDIObjects { set; get; }
        [DataMember]
        public String UserObjects { set; get; }
        [DataMember]
        public String Threads { set; get; }
        [DataMember]
        public String Handles { set; get; }
        [DataMember]
        public String BasePri { set; get; }
        [DataMember]
        public String NPPool { set; get; }
        [DataMember]
        public String PagedPool { set; get; }
        [DataMember]
        public String VMSize { set; get; }
        [DataMember]
        public String PFDelta { set; get; }
        [DataMember]
        public String PageFaults { set; get; }
        [DataMember]
        public String MemDelta { set; get; }
        [DataMember]
        public String PeakMemUsage { set; get; }
        [DataMember]
        public String MemUsage { set; get; }
        [DataMember]
        public String CPUTime { set; get; }
        [DataMember]
        public String CPU { set; get; }
        [DataMember]
        public String SessionId { set; get; }
        [DataMember]
        public String UserName { set; get; }
        [DataMember]
        public String PID { set; get; }
        [DataMember]
        public String ImageName { set; get; }
    }
}
