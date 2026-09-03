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





namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using System.Linq;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditDataQueryInfo
    {
        public String SiteUrl { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// site节点对应的某月的数据表
        /// </summary>
        [DataMember]
        public string AuditDataTable { get; set; }

        /// <summary>
        /// site数据的DB
        /// </summary>
        [DataMember]
        public string DatabaseID { get; set; }

        [DataMember]
        public List<FilterCondition> Filters { get; set; }
        [DataMember]
        public UrlFilterCondition UrlFilter { get; set; }

        /// <summary>
        /// 0 是all, only include
        /// </summary>
        [DataMember]
        public int Actions { get; set; }

        [DataMember]
        public AuditorItemType ItemType { get; set; }

        public override string ToString()
        {
            return String.Format("AuditDataQueryInfo[start {0}, end {1}, AuditDataTable {2}, Actions {3}]",
                StartTime, EndTime, AuditDataTable, Actions);
        }
    }

    /// <summary>
    /// 值与Auditor数据ItemType值一一对应
    /// </summary>
    public enum AuditorItemType
    {
        All = -1,
        Document = 1,
        ListItem = 3,
        List = 4,
        Folder = 5,
        Web = 6,
        Site = 7
    }
}
