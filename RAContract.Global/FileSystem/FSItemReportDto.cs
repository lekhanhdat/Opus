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
using System.Text;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using ProtoBuf;

namespace AvePoint.RA.Contract.FileSystem
{
    [ProtoContract]
    public class FSItemReportDto
    {
        [ProtoMember(1)]
        public Guid ItemId { get; set; }

        [ProtoMember(2)]
        public string ObjectName { get; set; }

        [ProtoMember(3)]
        public string OriginalFullPath { get; set; }

        [ProtoMember(4)]
        public string Type { get; set; }

        [ProtoMember(5)]
        public long FinishTime { get; set; }

        [ProtoMember(6)]
        public long FileSize { get; set; } // if needed

        [ProtoMember(7)]
        public JobDetailsStatus Status { get; set; }

        [ProtoMember(8)]
        public string ErrorMessage { get; set; }

        //[ProtoMember(9)]
        //public string SASURI { get; set; }
        // ...... Add other properties
    }

    [ProtoContract]
    public class FSBatchReportDto
    {
        [ProtoMember(1)]
        public string MessageId { get; set; }

        [ProtoMember(2)]
        public JobDetailsStatus BatchStatus { get; set; }

        [ProtoMember(3)]
        public int TotalItems { get; set; }

        [ProtoMember(4)]
        public int ProcessedItems { get; set; }

        [ProtoMember(5)]
        public string SASURI { get; set; }

        [ProtoMember(6)]
        public long BatchSize { get; set; } // if needed

        [ProtoMember(7)]
        public List<FSItemReportDto> Records { get; set; } = new List<FSItemReportDto>();

        [ProtoMember(8)]
        public string ErrorMessage { get; set; }
    }
}
