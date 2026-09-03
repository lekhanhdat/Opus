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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.FilterPolicy;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class QueryMessage : EDBaseMessage
    {
        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public string KeyWord { get; set; } //搜索关键字
        [DataMember]
        public int ResultType { get; set; }
        [DataMember]
        public List<EDFilterPolicy> EDFilters { get; set; }

        /// <summary>
        /// 是否超出了查询字符串的限制.
        /// </summary>
        [DataMember]
        public bool OutOfSize { get; set; }

        /// <summary>
        /// 不用了
        /// </summary>
        [DataMember]
        public List<FilterPolicy> Filters { get; set; }

        [DataMember]
        public HoldFileRelevantResults HoldFileRelevant { get; set; }

        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public int PlanCategory { get; set; }
        [DataMember]
        public int JobType { get; set; }

        #region off line 不用
        [DataMember]
        public PageAction Action { get; set; }

        //除了第一次search 每次必须发 
        [DataMember]
        public List<QueryInfo> QueryInfoList { get; set; }
        [DataMember]
        public int StartPage { get; set; }
        //数据丢的时候 再找数据的时候用到
        [DataMember]
        public List<PageInfo> PageInfoList { get; set; }
        [DataMember]
        public int CountPerPage { get; set; }
        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public bool IsFirstPage { get; set; }
        #endregion

        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

//        [DataMember]
//        public EDExportLocationDto SearchResultLocation { get; set; }
        [DataMember]
        public PhysicalDeviceDto SearchResultLocation { get; set; }

        [DataMember]
        public CplDBSettingsDto DBSettings { get; set; }

        /// <summary>
        /// Gui与Service通过此FarmName获得可用的Agent
        /// </summary>
        [DataMember]
        public string FarmName { get; set; }
    }
}
