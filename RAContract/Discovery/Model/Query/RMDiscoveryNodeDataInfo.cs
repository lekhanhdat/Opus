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
using AvePoint.RA.Contract.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Query
{
    public class RMDiscoveryNodeDataInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public List<Dictionary<string, object>> Items { get; set; } = new();

        public RMDiscoveryNodeDataInfo ApplyI18NForDefaultNodeName()
        {
            if (this.Items == null) return this;
            this.Items.ForEach(item =>
            {
                string nodeName = item.TryGetValue("name", out object nameObj) ? nameObj as string : string.Empty;
                switch (nodeName)
                {
                    case RMConstants.DEFAULT_GOOGLE_USER_GROUP:
                        item["name"] = I18N.Core.I18NEntity.GetString("RM_GoogleUser_Default_Container");
                        break;
                    case RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP:
                        item["name"] = I18N.Core.I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
                        break;
                    default:
                        break;
                }
            });
            return this;
        }
    }
    public class RMDiscoveryNodeDataSizeInfo
    {
        [JsonProperty("siteUrl")]
        public string SiteUrl { get; set; }
        [JsonProperty("siteId")]
        public string SiteId { get; set; }

        [JsonProperty("archiveSize")]
        public long ArchiveSize { get; set; }
        [JsonProperty("destroySize")]
        public long DestroySize { get; set; }
    }
}
