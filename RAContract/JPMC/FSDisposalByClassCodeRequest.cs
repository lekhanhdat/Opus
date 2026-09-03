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
using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.JPMC
{
    internal sealed class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(List<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
                return token.ToObject<List<T>>(serializer);

            return new List<T> { token.ToObject<T>(serializer) };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value);
    }
    [DataContract(IsReference = true)]
    [JsonObject]
    public class FSDisposalByClassCodeRequest
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid ConnectionGroupID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid NodeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        [JsonConverter(typeof(SingleOrArrayConverter<Guid>))]
        public List<Guid> TermID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Level { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Name { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMFSTreeNode Parent { get; set; }
    }
}