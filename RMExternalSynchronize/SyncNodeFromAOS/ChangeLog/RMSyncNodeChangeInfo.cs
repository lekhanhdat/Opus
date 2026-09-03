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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public abstract class RMSyncNodeChangeInfoBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("ai")]
        public string AosId { get; set; }
        [JsonProperty("ci")]
        public string ContainerId { get; set; }

        [JsonProperty("cn")]
        public string ContainerName { get; set; }
        [JsonProperty("nl")]
        public NodeLevel NodeLevel { get; set; }

        [JsonProperty("ct")]
        public RMSyncNodeChangeType ChangeType { get; set; }
        [JsonProperty("ic")]
        public bool IsContainer { get; set; }
        [JsonProperty("cs")]
        public SourceFlag ContentSource { get; set; }
    }
    public class RMSyncNodeChangeInfo : RMSyncNodeChangeInfoBase
    {
        [JsonProperty("burl")]
        public string BeforeUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("oti")]
        public string O365TenantId { get; set; }

        [JsonIgnore]
        public Guid RealId { get; set; }

        [JsonIgnore]
        public string MoveSourceContainerId { get; set; }
    }
}
