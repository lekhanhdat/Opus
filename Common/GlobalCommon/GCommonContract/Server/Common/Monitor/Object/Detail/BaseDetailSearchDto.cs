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



using System.Runtime.Serialization;
using AvePoint.Common.Module.JobMonitor.Entities;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Job.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    /// <summary>
    /// Job detail search object.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseDetailSearchDto
    {
        [DataMember]
        public int Skip { get; set; }

        [DataMember]
        public int Take { get; set; }

        [DataMember]
        public string CommonSearch { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public TimeZoneType ZoneType { get; set; }

        [DataMember]
        public JobReportDetailStatus[] States { get; set; }

        [DataMember]
        public JobReportDetailEntityType[] EntityTypes { get; set; }


        public virtual void Assemble(BaseDetailSearchDto source)
        {
            this.Skip = source.Skip;
            this.Take = source.Take;
            this.CommonSearch = source.CommonSearch;
            this.EntityTypes = source.EntityTypes;
            this.States = source.States;
            TimeZoneId = source.TimeZoneId;
            ZoneType = source.ZoneType;
        }

        public BaseDetailSearchDto Clone()
        {
            return new BaseDetailSearchDto()
            {
                CommonSearch = this.CommonSearch,
                EntityTypes = this.EntityTypes != null ? this.EntityTypes.Clone() as JobReportDetailEntityType[] : null,
                Skip = this.Skip,
                States = this.States != null ? this.States.Clone() as JobReportDetailStatus[] : null,
                Take = this.Take,
                TimeZoneId = TimeZoneId,
                ZoneType = ZoneType
            };
        }
    }
}
