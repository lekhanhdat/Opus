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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace AvePoint.RA.Contract.Box
{
    [DataContract]
    public class BoxTreeNode : SourceTreeNode
    {
        [IgnoreDataMember]
        public override SourceFlag Flag => SourceFlag.Box;

        [DataMember]
        [JsonProperty(PropertyName = "ownerId")]
        public string OwnerId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "parent")]
        public BoxTreeNode Parent { get; set; }

        [IgnoreDataMember]
        [JsonProperty(PropertyName = "hasParent")]
        public bool HasParent => this.Parent != null;

        [DataMember]
        [JsonProperty(PropertyName = "connectionId")]
        public string ConnectionId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "startJobNodeLevel")]
        public RMNodeLevel StartJobNodeLevel { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "pageIndex")]
        public int PageIndex { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "childrenIds")]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "children")]
        public List<BoxTreeNode> Children { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "childrenCount")]
        public int ChildrenCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "checkNumber")]
        public int CheckNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "name")]
        public string Name { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "expanded")]
        public bool Expanded { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "isProcessApprovalDatasOnly")]
        public bool IsProcessApprovalDatasOnly { get; set; }
    }

}
