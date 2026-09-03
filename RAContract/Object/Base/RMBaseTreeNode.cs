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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object.Base
{
    [DataContract(IsReference = true)]
    [JsonObject]
    public class RMBaseTreeNode<T> where T : RMBaseTreeNode<T>
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Id { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Level { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Name { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string OrphanNameSuffix { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DisplayName { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Title { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int NodeType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool Hidden { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool? Loaded { get; set; }

        /// <summary>
        /// IncludeNew为-1代表当前节点没有Include New的逻辑，为0代表不是Include New，为1代表是Include New
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int IncludeNew { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool Expanded { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ParentId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ParentName { set; get; }

        /// <summary>
        /// CheckNumber为1代表当前节点是Checked状态，为0代表UnChecked状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int CheckNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public IconStatus IconStatus { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PageIndex { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PageSize { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ChildrenCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public T Parent { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<T> Children { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public PredictionModeType PredictionModeType { set; get; }

    }
}
