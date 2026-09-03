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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.FullTextIndexSearch.FilterPolicy;

namespace AvePoint.GCommon.Contract.FullTextIndexSearch
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexQuery
    {

        [DataMember]
        public string Keyword { get; set; }

        /// <summary>
        /// 动作源
        /// </summary>
        [DataMember]
        public ActionSource ActionSource { get; set; }

        /// <summary>
        /// 排序名称,为实装.
        /// </summary>
        [DataMember]
        public string SortName { get; set; }

        [DataMember]
        public SortColumn SortColumn { get; set; }

        /// <summary>
        /// 完整的Tree,从Farm Level开始.
        /// </summary>
        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        /// <summary>
        /// 排序正序还是倒序.
        /// </summary>
        [DataMember]
        public bool Reverse { get; set; }

        /// <summary>
        /// 页码.
        /// </summary>
        [DataMember]
        public int TempOffSet { get; set; }

        [DataMember]
        public int OffSet 
        {
            get
            {
                return this.TempOffSet;
            }
            set 
            {
                TempOffSet = value - 1 > 0 ? value - 1 : 0;
            }
        }

        [DataMember]
        public int Length { get; set; }

        /// <summary>
        /// Filter Policy 条件.
        /// </summary>
        [DataMember]
        public List<FullTextIndexFilterPolicy> Policys { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionSource
    {
        [EnumMember]
        ARCHIVER,
        [EnumMember]
        EDISCOVERY,
        [EnumMember]
        FSARCHIVER
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SortColumn
    {
        [EnumMember]
        NONE,
        [EnumMember]
        NAME,
        [EnumMember]
        VERSION_NAME,
        [EnumMember]
        SIZE,
        [EnumMember]
        LOCATION,
        [EnumMember]
        CREATED_BY,
        [EnumMember]
        LAST_MODIFIED_TIME,
    }
}
