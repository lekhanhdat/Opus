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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RASourceTreeQuery.Model
{
    public abstract class SourceTreeNode
    {

        [JsonProperty(PropertyName = "flag")]
        public abstract SourceFlag Flag { get; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "realId")]
        public string RealId { get; set; }

        [JsonProperty(PropertyName = "containerId")]
        public string ContainerId { get; set; }

        [JsonProperty(PropertyName = "level")]
        public RMNodeLevel Level { get; set; }

        [JsonProperty(PropertyName = "leafName")]
        public string LeafName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "fullPath")]
        public string FullPath { get; set; }

        [JsonProperty(PropertyName = "iconStatus")]
        public IconStatus IconStatus { get; set; } = IconStatus.NoSet;
    }
}
