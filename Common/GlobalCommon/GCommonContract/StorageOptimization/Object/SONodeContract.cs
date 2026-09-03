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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(IsReference = true)]
    public abstract class SONodeContract<T>
    {
        /// <summary>
        /// 节点ItemId
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        [DataMember]
        public string NodeName { get; set; }

        /// <summary>
        /// 节点显示名称
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// 节点级别
        /// </summary>
        [DataMember]
        public NodeLevel NodeLevel { get; set; }

        /// <summary>
        /// 父节点ID
        /// </summary>
        [DataMember]
        public string ParentNodeId { get; set; }

        /// <summary>
        /// 父节点Url
        /// </summary>
        [DataMember]
        public string ParentNodeName { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        [DataMember]
        public T ParentNode { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        [DataMember]
        public List<T> Children { get; set; }

        /// <summary>
        /// FarmId
        /// </summary>
        [DataMember]
        public string FarmId { get; set; }

        /// <summary>
        /// FarmName
        /// </summary>
        [DataMember]
        public string FarmName { get; set; }

        /// <summary>
        /// SharePoint Version
        /// </summary>
        [DataMember]
        public int SPVersion { get; set; }

        /// <summary>
        /// SharePoint tree in manager database id
        /// </summary>
        [DataMember]
        public string ManagerTreeId { get; set; }

        /// <summary>
        /// SPType, 区分是不是Remote Farm节点, 不存数据库
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SPType SPType { set; get; }

        /// <summary>
        /// 记录Bpos节点的信息,不存数据库
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public BposInfo BposInfo { set; get; }
       
    }

}
