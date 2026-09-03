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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CADocAveNodePolicyDto
    {
        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public string PolicyId { get; set; }

        [DataMember]
        public string ParentNodeId { get; set; }

        [DataMember]
        public string Scope { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public SPTreeNodeDto ApplyNode { get; set; }

        [DataMember]
        public string Id { get; set; }

        public CADocAveNodePolicyDto Clone()
        {
            CADocAveNodePolicyDto clone = new CADocAveNodePolicyDto();
            clone.NodeId = this.NodeId;
            clone.FarmId = this.FarmId;
            clone.Level = this.Level;
            clone.PolicyId = this.PolicyId;
            clone.ParentNodeId = this.ParentNodeId;
            clone.Scope = this.Scope;
            clone.ProfileName = this.ProfileName;
            clone.FarmName = this.FarmName;
            return clone;
        }
    }

    /// <summary>
    /// 每一个mapping所对应的ID
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CADocAveNodePolicyKey
    {
        /// <summary>
        /// apply的节点ID
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string Scope { get; set; }

        /// <summary>
        /// apply的profile id
        /// </summary>
        [DataMember]
        public string PolicyId { get; set; }

        [DataMember]
        public SPTreeNodeDto ApplyNode { get; set; }
    }


    /// <summary>
    /// 该类以farm分类储存profile和节点的mapping
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NodeAndProfileMapping
    {
        [DataMember]
        public SPTreeNodeDto FarmNode { get; set; }

        /// <summary>
        /// farm节点下的所有profileId
        /// </summary>
        [DataMember]
        public List<string> ProfileIds { get; set; }

        /// <summary>
        /// 该farm下所有apply的节点ID
        /// </summary>
        [DataMember]
        public List<string> NodeIDs { get; set; }

        [DataMember]
        public List<CADocAveNodePolicyKey> NodePolicyKeys { get; set; }

        [DataMember]
        public List<TreeNodeCollection> NodeCollections { get; set; }
    }
}
