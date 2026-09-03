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
    public class ListSubscribedSkus : EntityBase
    {
        [JsonProperty("value")]
        public SubscribedSkus[] Value { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SubscribedSkus : EntityBase
    {
        /// <summary>
        /// Gets or sets capability status.
        /// Possible values are: Enabled, Warning, Suspended, Deleted, LockedOut. The capabilityStatus is Enabled if the prepaidUnits property has at least 1 unit that is enabled, and LockedOut if the customer cancelled their subscription.
        /// </summary>
        [JsonProperty("capabilityStatus")]
        public string CapabilityStatus { get; set; }

        /// <summary>
        /// Gets or sets consumed units.
        /// The number of licenses that have been assigned.
        /// </summary>
        [JsonProperty("consumedUnits")]
        public int? ConsumedUnits { get; set; }

        /// <summary>
        /// Gets or sets sku id.
        /// The unique identifier (GUID) for the service SKU.
        /// </summary>
        [JsonProperty("skuId")]
        public Guid? SkuId { get; set; }

        /// <summary>
        /// Gets or sets sku part number.
        /// The SKU part number; for example: 'AAD_PREMIUM' or 'RMSBASIC'. To get a list of commercial subscriptions that an organization has acquired, see List subscribedSkus.
        /// </summary>
        [JsonProperty("skuPartNumber")]
        public string SkuPartNumber { get; set; }

        /// <summary>
        /// Gets or sets applies to.
        /// For example, 'User' or 'Company'.
        /// </summary>
        [JsonProperty("appliesTo")]
        public string AppliesTo { get; set; }

        /// <summary>
        /// Gets or sets prepaid units.
        /// Information about the number and status of prepaid licenses.
        /// </summary>
        [JsonProperty("prepaidUnits")]
        public LicenseUnitsDetail PrepaidUnits { get; set; }

        /// <summary>
        /// Gets or sets service plans.
        /// Information about the service plans that are available with the SKU. Not nullable
        /// </summary>
        [JsonProperty("servicePlans")]
        public ServicePlanInfo[] ServicePlans { get; set; }
    }
}