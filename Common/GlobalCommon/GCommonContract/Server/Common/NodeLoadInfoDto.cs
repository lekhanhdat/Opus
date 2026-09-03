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




namespace AvePoint.GCommon.Contract.Server.Common
{
    #region == using directives ==
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NodeLoadInfoDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long GatherTime { get; set; }

        [DataMember]
        public long CpuUsage { get; set; }

        [DataMember]
        public long NetworkSendSpeed { get; set; }

        [DataMember]
        public long NetworkReceiveSpeed { get; set; }

        [DataMember]
        public long NetworkBandWidth { get; set; }

        [DataMember]
        public long IoUsage { get; set; }

        //[DataMember]
        //public long RuntimeTotalMemery { get; set; }

        //[DataMember]
        //public long RuntimeUsedMemery { get; set; }

        [DataMember]
        public long SystemTotalMemery { get; set; }

        [DataMember]
        public long SystemFreeMemery { get; set; }

        [DataMember]
        public long CpuNumber { get; set; }

        [DataMember]
        public long CpuHz { get; set; }

    }
}
