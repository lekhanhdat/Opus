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
    #region  using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Name = ContractConstants.Namespace)]
    public class CpuMemoryUsage
    {
        [DataMember]
        public Int32 CpuUsage { get; set; }

        [DataMember]
        public Int64 FreeMemorySize { get; set; }

        [DataMember]
        public Int64 TotalMemorySize { get; set; }

        [DataMember]
        public String HostName { get; set; }

        public override String ToString()
        {
            return String.Format("Performance information: Cpu usage:{0}, Free memory size:{1}, Total memory size:{2}, Host name:{3}",
                this.CpuUsage,
                this.FreeMemorySize,
                this.TotalMemorySize,
                this.HostName);
        }
    }
}