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
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.TemplateManagement;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.CustomizeConnector.Model
{
    [DataContract]
    public class CustomizeConnectorColumnInfo
    {
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("internalName")]
        public string InternalName { get; set; }
        [DataMember]
        [JsonProperty("type")]
        public ColumnType Type { get; set; }
        [DataMember]
        [JsonProperty("scope")]
        public CustomizeConnectorColumnScope Scope { get; set; }
        [DataMember]
        [JsonProperty("origin")]
        public CustomizeConnectorOrigin Origin { get; set; }
        [DataMember]
        [JsonProperty("extention")]
        public string Extention { get; set; }
        [DataMember]
        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; }
        [DataMember]
        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }
        [DataMember]
        [JsonProperty("order")]
        public int Order { get; set; }
    }
}
