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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleJobStatusDto : BaseJobParamDto
    {
        [DataMember]
        public List<ScheduleStatusObj> StatusObj { get; set; }
        [DataMember]
        public SearchJobDto SearchJobDto { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleStatusObj
    {
        [DataMember]
        public string Id { get; set;}

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public ScheduleOwnerType Type { get; set; }

        [DataMember]
        public string ScheduleId { get; set; }
    }
}
