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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.Tenant
{
    public class RMGraphTenantSubscribedSku
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("accountId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid AccountId { get; set; }

        [JsonProperty("accountName", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountName { get; set; }

        [JsonProperty("skuId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid SkuId { get; set; }

        [JsonProperty("skuPartNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string SkuPartNumber { get; set; }

        [JsonProperty("prepaidUnits", NullValueHandling = NullValueHandling.Ignore)]
        public RMGraphTenantLicenseUnitsDetail PrepaidUnits { get; set; } = new();
    }

    public class RMGraphTenantLicenseUnitsDetail
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public int Enabled { get; set; } = 0;

        [JsonProperty("suspended", NullValueHandling = NullValueHandling.Ignore)]
        public int Suspended { get; set; } = 0;

        [JsonProperty("warning", NullValueHandling = NullValueHandling.Ignore)]
        public int Warning { get; set; } = 0;
    }
}
