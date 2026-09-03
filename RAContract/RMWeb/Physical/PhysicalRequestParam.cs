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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    [DataContract]
    public class PhysicalRequestParam
    {
        #region Query Param
        [DataMember]
        public int PageIndex { set; get; }
        [DataMember]
        public int PageSize { set; get; }
        [DataMember]
        public string SearchText { set; get; }
        [DataMember]
        public string SortBy { set; get; }

        ///后台以固定的Key Search,  不需要前台定制Key,  暂时去掉此属性
        //public List<string> SearcheKeys { get; set; }
        [DataMember] 
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? EndTime { get; set; }
        [DataMember]
        public List<PhysicalRequestFilter> Filters { set; get; }
        #endregion
        /// <summary>
        /// Approval And reject param
        /// </summary>
        [DataMember]
        public List<PhysicalRequestDto> Requests { set; get; }
        [DataMember]
        public bool IgnoreReturnDateExpired { set; get; }
        [DataMember]
        public List<int> ResendIdList { set; get; }
    }
    [DataContract]
    public class PhysicalRequestFilter
    {
        [DataMember]
        public PhysicalRequestFilterColumn Column { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }

    public enum PhysicalRequestFilterColumn
    {
        None = 0,
        Type,
        Status, 
        RequestBy
    }
}
