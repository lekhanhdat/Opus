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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.RA.Contract.Object.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    /// <summary>
    /// Exchange online tree node for GUI 
    /// </summary>
    [DataContract(IsReference = true)]
    [JsonObject]
    public class RMSampleEXOTreeNode : RMBaseTreeNode<RMSampleEXOTreeNode>
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string GroupName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public MailboxType MailboxType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string InternalFolderPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SiteCollectionUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Sender { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long SendDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DisplayTo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Email { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Category { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool HasAttachment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int OffSet { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SubFolderCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMSampleEXOTreeNode Parent { set { base.Parent = value; } get { return base.Parent; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMSampleEXOTreeNode> Children { set { base.Children = value; } get { return base.Children; } }

        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMSampleEXOTreeNode Clone()
        {
            return this.MemberwiseClone() as RMSampleEXOTreeNode;
        }
    }
}
