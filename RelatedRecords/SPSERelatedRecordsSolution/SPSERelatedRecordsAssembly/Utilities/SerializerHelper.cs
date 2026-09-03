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

namespace AvePoint.Opus.RelatedRecords.Utilities
{
    public class SerializerHelper
    {

        public static string SerializeByJsonSerializer(Object data, bool serializeWithReference = false)
        {
            var settings = GetJsonSerializerSettings(serializeWithReference);
            return JsonConvert.SerializeObject(data, settings);
        }

        public static T DeserializeByJsonSerializer<T>(string data, bool serializeWithReference = false)
        {
            var settings = GetJsonSerializerSettings(serializeWithReference);
            return JsonConvert.DeserializeObject<T>(data, settings);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(bool serializeWithReference)
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = serializeWithReference ? Newtonsoft.Json.ReferenceLoopHandling.Serialize : Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = serializeWithReference ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }
    }
}
