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
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Tree.Base
{
    [DataContract]
    public abstract class ProfileNode<T> : Node<T>
    {
        /// <summary>
        /// Node是否展开
        /// </summary>
        [DataMember]
        public bool Expanded { get; set; }

        /// <summary>
        /// Node是否已经展开过
        /// 正常情况下，Expanded=true时，Loaded也必然=true
        /// </summary>
        [DataMember] 
        public bool Loaded { get; set; }

        /// <summary>
        /// Node的选中状态
        /// 为null时，标识不支持选中 或者 有部分SubNode是Checked状态
        /// </summary>
        [DataMember] 
        public bool? Checked { get; set; }

        /// <summary>
        /// 只有Tree支持多选时，才可能支持IncludeNew
        /// IncludeNew为null代表当前节点没有Include New的逻辑
        /// </summary>
        [DataMember] 
        public bool? IncludeNew { get; set; }

        /// <summary>
        /// 支持IncludeNew时，才有用
        /// </summary>
        [DataMember] 
        public bool? SelectAllBefore { get; set; }

        /// <summary>
        /// 保存 不在当前页 但是满足以下任意条件 的子节点：
        /// 1、Expanded==true
        /// 2、IncludeNew==true 
        /// 3、子节点的（Recursive All）子节点的（Expanded==true 或 Checked==true 或 IncludeNew==true）
        /// </summary>
        [DataMember] 
        public Dictionary<string, T> OtherChildren { get; set; }

        /// <summary>
        /// 存放子节点的ID,及其Index和是否Checked
        /// {ChildID: [Index, Checked]} => {"40": [11, 1]} => 表示：ID是"40"的Child，是Checked，Index是11
        /// {ChildID: [Index]} => {"41": [12]} => 表示：ID是"41"的Child，是Unchecked，Index是12
        /// value：List<int> 的Count>1时，表示Checked，Count=0表示Unchecked
        /// </summary>
        [DataMember] 
        public Dictionary<string, List<int>> ChildStates { get; set; }
    }
}
