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

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnOrder
    {
        [DataMember]
        public string PropName { get; set; }

        [DataMember]
        public bool AscOrder { get; set; }

        public string OrderType
        {
            get
            {
                return AscOrder ? "asc" : "desc";
            }
            set
            {
                if (value != null)
                {
                    AscOrder = value.Equals("asc", StringComparison.Ordinal);
                }
            }
        }

        /// <summary>
        /// Order < 0 stand for desc
        /// Order > 0 stand for asc
        /// </summary>
        public int Order
        {
            get { return AscOrder ? 1 : -1; }
            set
            {
                if (value != 0)
                {
                    AscOrder = value > 1;
    }
            }
        }


    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchJobDto : BaseJobParamDto
    {
        public SearchJobDto()
        {
            base.JobMonitorCommandType = JobMonitorCommandType.GetJobValues;
        }
        /// <summary>
        /// Job JobDateRangeType
        /// Schedule ScheduleDateRangeType
        /// </summary>
        [DataMember]
        public int RangeType { get; set; }
        [DataMember]
        public long FromTime { get; set; }
        [DataMember]
        public long ToTime { get; set; }

        [DataMember]
        public long CalendarFromTime { get; set; }
        [DataMember]
        public long CalendarToTime { get; set; }

        [DataMember]
        public int Start { get; set; }
        [DataMember]
        public int Length { get; set; }
        [DataMember]
        public Dictionary<string, List<string>> Filter { get; set; }

        [DataMember]
        public string CustomSearch { get; set; }

        [DataMember]
        public List<string> CustomSearchProperties { get; set; }

        [DataMember]
        public List<ColumnOrder> OrderList { get; set; }
        /// <summary>
        /// 区分是CalendarView 还是 ListView.(Calendar无分页)
        /// </summary>
        [DataMember]
        public ViewType ViewType { get; set; }

        [DataMember]
        public String[] PlanGroupIDs { get; set; }

        [DataMember]
        public List<string> SelectionKeys { get; set; }

        [DataMember]
        public List<string> DynamicFilterKeys { get; set; }
    }
}
