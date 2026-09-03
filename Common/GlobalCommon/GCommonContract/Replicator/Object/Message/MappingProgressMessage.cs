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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingProgressMessage
    {
        [DataMember]
        public double TotalPercent { get; set; }

        [DataMember]
        public bool IsRetry { get; set; }

        [DataMember]
        public DateTime RetryTime { get; set; }

        [DataMember]
        public int MaxRetryNumber { get; set; }

        [DataMember]
        public int RetryCount { get; set; }

        [DataMember]
        public string SrcConnection { get; set; }

        [DataMember]
        public string DestConnection { get; set; }

        [DataMember]
        public int ItemSend { get; set; }

        [DataMember]
        public int ItemReceived { get; set; }

        [DataMember]
        public long TotalTransSize { get; set; }

        [DataMember]
        public int DestItemSend { get; set; }

        [DataMember]
        public int DestItemReceived { get; set; }

        [DataMember]
        public int ItemFailed { get; set; }

        [DataMember]
        public int ItemConflicted { get; set; }

        [DataMember]
        public int ItemRetried { get; set; }

        [DataMember]
        public string ItemPath { get; set; }

    }
}
