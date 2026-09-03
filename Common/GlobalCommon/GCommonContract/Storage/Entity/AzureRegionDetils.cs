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

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    public class AzureRegionDetils
    {
        [JsonProperty(propertyName: "changeNumber")]
        public int ChangeNumber { get; set; }
        [JsonProperty(propertyName: "cloud")]
        public string Cloud { get; set; }
        [JsonProperty(propertyName: "values")]
        public AzureRegionDetil[] Values { get; set; }
    }

    public class AzureRegionDetil
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
        [JsonProperty(propertyName: "id")]
        public string Id { get; set; }
        [JsonProperty(propertyName: "properties")]
        public AzureRegionProperties Properties { get; set; }

    }

    public class AzureRegionProperties
    {
        [JsonProperty(propertyName: "changeNumber")]
        public int ChangeNumber { get; set; }
        [JsonProperty(propertyName: "region")]
        public string Region { get; set; }
        [JsonProperty(propertyName: "regionId")]
        public int RegionId { get; set; }
        [JsonProperty(propertyName: "platform")]
        public string Platform { get; set; }
        [JsonProperty(propertyName: "systemService")]
        public string SystemService { get; set; }
        [JsonProperty(propertyName: "addressPrefixes")]
        public string[] AddressPrefixes { get; set; }
        [JsonProperty(propertyName: "networkFeatures")]
        public string[] NetworkFeatures { get; set; } 
    }
}
