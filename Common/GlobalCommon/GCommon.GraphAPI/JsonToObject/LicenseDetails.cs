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

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using Newtonsoft.Json;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ListLicenseDetails : EntityBase
    {
        [JsonProperty("value")]
        public LicenseDetails[] Value { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LicenseDetails : EntityBase
    {
        /// <summary>
        /// Gets or sets service plans.
        /// Information about the service plans assigned with the license. Read-only, Not nullable
        /// </summary>
        [JsonProperty("servicePlans")]
        public ServicePlanInfo[] ServicePlans { get; set; }

        /// <summary>
        /// Gets or sets sku id.
        /// Unique identifier (GUID) for the service SKU. Equal to the skuId property on the related SubscribedSku object. Read-only
        /// </summary>
        [JsonProperty("skuId")]
        public Guid? SkuId { get; set; }

        /// <summary>
        /// Gets or sets sku part number.
        /// Unique SKU display name. Equal to the skuPartNumber on the related SubscribedSku object; for example: 'AAD_Premium'. Read-only
        /// </summary>
        [JsonProperty("skuPartNumber")]
        public string SkuPartNumber { get; set; }
    }
}