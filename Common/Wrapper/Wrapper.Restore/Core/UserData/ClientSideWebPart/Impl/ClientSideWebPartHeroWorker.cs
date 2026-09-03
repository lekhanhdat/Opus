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
namespace AvePoint.Wrapper.Restore.Core
{
    using System.Collections.Generic;
    using System;
    using GCommon;
    using Newtonsoft.Json.Linq;

    class ClientSideWebPartHeroWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "c4bd7b2f-7b6e-4599-8485-16504575f590";

        protected override AveLogger logger { get { return AveLogger.GetInstance(typeof(ClientSideWebPartHeroWorker)); } }

        protected override List<ClientSideWebpartProperty> PotentialPropertiesAndTypes
        {
            get
            {
                return new List<ClientSideWebpartProperty>()
                {
                    new ClientSideWebpartProperty("content", ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                            new ClientSideWebpartProperty("previewImage",  ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                                new ClientSideWebpartProperty("siteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
                                new ClientSideWebpartProperty("webId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site),
                                new ClientSideWebpartProperty("listId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.List),
                                new ClientSideWebpartProperty("id", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Item),
                            })
                        })
                };
            }
        }

        protected override void HandleServerProcessedContent(AveClientSideWebPart webpart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            base.HandleServerProcessedContent(webpart, document, lastPost, ref needPost);
            HandleCustomMetadata(webpart, document, lastPost, ref needPost);
        }

        protected void HandleCustomMetadata(AveClientSideWebPart webpart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            JObject serverProcessedContent = webpart.ServerProcessedContent;
            if (serverProcessedContent == null || (serverProcessedContent.Count <= 0))
            {
                return;
            }
            string propertyName = "customMetadata";
            if (serverProcessedContent.ContainsKey(propertyName))
            {
                JProperty jProperty = serverProcessedContent.Property(propertyName);
                try
                {
                    JToken value = jProperty.Value;
                    JToken result;
                    if (value is JArray)
                    {
                        JArray valueArry = value as JArray;
                        for (int i = 0; i < valueArry.Count; i++)
                        {
                            var oneValue = valueArry[i];
                            bool updated = HandlePreviewImage(oneValue.ToString(), document, lastPost, ref needPost, out result);
                            if (!updated && !lastPost)
                            {
                                logger.Warn($"Server processed content needs a post action due to {propertyName}.");
                                needPost = true;
                                break;
                            }
                            valueArry[i] = result;
                        }
                        jProperty.Value = valueArry;
                    }
                    if (!HandlePreviewImage(value.ToString(), document, lastPost, ref needPost, out result) && !lastPost)
                    {
                        logger.Warn($"Server processed content needs a post action due to {propertyName}.");
                        needPost = true;
                    }
                    jProperty.Value = result;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Can not handle server processed content due to property [{jProperty.Name}], message:{ex.ToString()}");
                }
            }

            //Set ServerProcessedContent
            JObject propertiesToJObject = new JObject();
            propertiesToJObject["serverProcessedContent"] = serverProcessedContent;
            webpart.PropertiesJson = propertiesToJObject.ToString();
        }

        protected bool HandlePreviewImage(string inputJson, AveSPDoc document, bool lastPost, ref bool needPost, out JToken result)
        {
            bool updated = false;
            if (string.IsNullOrEmpty(inputJson))
            {
                result = default(JToken);
                updated = true;
                return updated;
            }
            JObject valueObject = JObject.Parse(inputJson);
            if (valueObject.Count <= 0)
            {
                result = default(JToken);
                updated = true;
                return updated;
            }
            foreach (var tempProperty in valueObject.Properties())
            {
                if (!tempProperty.Name.EndsWith(".previewImage.url", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var properties = new List<ClientSideWebpartProperty>()
                {
                    new ClientSideWebpartProperty("siteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
                    new ClientSideWebpartProperty("webId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site),
                    new ClientSideWebpartProperty("listId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.List),
                    new ClientSideWebpartProperty("uniqueId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Item),
                };
                JToken enumerableValue = tempProperty.Value;
                if (enumerableValue is JArray)
                {
                    JArray arry = enumerableValue as JArray;
                    for (int i = 0; i < arry.Count; i++)
                    {
                        JObject oneObject = arry[i] as JObject;
                        string updateObject = HandlePropertiesByType(oneObject.ToString(), document, lastPost, properties, ref needPost);
                        arry[i] = Newtonsoft.Json.Linq.JObject.Parse(updateObject);
                    }
                    tempProperty.Value = arry;
                    updated = true;
                    continue;
                }
                var propertyObj = enumerableValue as JObject;
                //string wrapJProperty = propertyValue.StartsWith("{") && propertyValue.EndsWith("}") ? propertyValue : string.Concat("{", allProperties.Property(name).Value, "}");
                string updatedValue = HandlePropertiesByType(propertyObj.ToString(), document, lastPost, properties, ref needPost);
                tempProperty.Value = Newtonsoft.Json.Linq.JObject.Parse(updatedValue);
                updated = true;
            }
            result = valueObject;
            return updated;
        }
    }
}
