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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SummaryRowType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Schedule = 1,
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobSummaryRow
    {
        [DataMember]
        public SummaryRowType Type { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }

        public virtual JobSummaryRow Clone()
        {
            JobSummaryRow clone = new JobSummaryRow();
            clone.Key = this.Key;
            clone.Value = this.Value;
            clone.Type = this.Type;
            return clone;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleSummaryRow : JobSummaryRow
    {
        [DataMember]
        public string EndTime { get; set; }

        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Interval { get; set; }

        [DataMember]
        public string Option { get; set; }

        public override JobSummaryRow Clone()
        {
            return this.MemberwiseClone() as ScheduleSummaryRow;
}
    }
}
