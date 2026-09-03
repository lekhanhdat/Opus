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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Box.Model
{
    [DataContract]
    public class BoxConnectionViewModel
    {
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("description")]
        public string Description { get; set; }
        [DataMember]
        [JsonProperty("authenticationType")]
        public BoxAuthenticationType AuthenticationType { get; set; }
        [DataMember]
        [JsonProperty("enterpriseId")]
        public string EnterpriseId { get; set; }
        [DataMember]
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [DataMember]
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
        [DataMember]
        [JsonProperty("jsonFileName")]
        public string JsonFileName { get; set; }
        [DataMember]
        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }
        [DataMember]
        [JsonProperty("created")]
        public string Created { get; set; }
        [DataMember]
        [JsonProperty("modified")]
        public string Modified { get; set; }
        [DataMember]
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        [DataMember]
        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }
        [DataMember]
        [JsonProperty("connectionGroupId")]
        public Guid ConnectionGroupId { get; set; }
    }
}
