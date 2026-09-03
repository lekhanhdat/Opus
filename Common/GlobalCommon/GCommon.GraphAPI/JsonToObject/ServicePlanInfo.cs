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
    public class ServicePlanInfo : EntityBase
    {
        /// <summary>
        /// Gets or sets appliesTo.
        /// The object the service plan can be assigned to. The possible values are:User - service plan can be assigned to individual users.Company - service plan can be assigned to the entire tenant.
        /// </summary>
        [JsonProperty("appliesTo")]
        public string AppliesTo { get; set; }

        /// <summary>
        /// Gets or sets provisioningStatus.
        /// The provisioning status of the service plan. The possible values are:Success - Service is fully provisioned.Disabled - Service has been disabled.ErrorStatus - The service plan has not been provisioned and is in an error state.PendingInput - Service is not yet provisioned; awaiting service confirmation.PendingActivation - Service is provisioned but requires explicit activation by administrator (for example, Intune_O365 service plan)PendingProvisioning - Microsoft has added a new service to the product SKU and it has not been activated in the tenant, yet.
        /// </summary>
        [JsonProperty("provisioningStatus")]
        public string ProvisioningStatus { get; set; }

        /// <summary>
        /// Gets or sets servicePlanId.
        /// The unique identifier of the service plan.
        /// </summary>
        [JsonProperty("servicePlanId")]
        public Guid? ServicePlanId { get; set; }

        /// <summary>
        /// Gets or sets servicePlanName.
        /// The name of the service plan.
        /// </summary>
        [JsonProperty("servicePlanName")]
        public string ServicePlanName { get; set; }
    }
}