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
    public abstract class Node<T>
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }

        /// <summary>
        /// Node的类型
        /// </summary>
        [DataMember] 
        public int NodeType { set; get; }
        [DataMember]
        public string ParentId { set; get; }
        [DataMember]
        public bool HasChildren { set; get; }

        /// <summary>
        /// 只是当前页显示的子节点，不一定是所有的子节点
        /// </summary>

        [DataMember] 
        public List<T> Children { get; set; }

        /// <summary>
        /// 子节点的总数
        /// </summary>
        [DataMember] 
        public int ChildrenCount { get; set; }
        [DataMember]
        public int PagerSize { set; get; }
        [DataMember]
        public int PagerIndex { set; get; }
    }
}
