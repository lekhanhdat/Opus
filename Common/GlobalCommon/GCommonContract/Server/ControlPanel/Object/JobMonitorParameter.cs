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




using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract]
    public class JobMonitorParameter
    {
        [DataMember]
        public int Start {get; set;}
        [DataMember]
        public int Length {get; set;}
        [DataMember]
        public int Type {get; set;}
        [DataMember]
        public string OrderBy { get; set;}
        [DataMember]
        public int OrderDirection { get; set; }
        [DataMember]
        public string ConfigName { get; set; }
        [DataMember]
        public int DispType { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public Dictionary<string, List<object>> Filter {get; set;}

        private List<ColumnOrder> orderList;

        [DataMember]
        public string PropName { get; set; }

        [DataMember]
        public List<ColumnOrder> OrderList
        {
            get
            {
                if (orderList == null)
                {
                    orderList = new List<ColumnOrder>();
                }
                return orderList;
            }
            private set
            {
                orderList = value;
            }
        }

        private List<JobParameter> jobs = new List<JobParameter>();

        [DataMember]
        public List<JobParameter> Jobs
        {
            get { return this.jobs; }
            set { this.jobs = value; }
        }
    }
}
