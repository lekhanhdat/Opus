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
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Text;
    using System.Collections;
    using System.Web;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    public delegate void HtmlProperitesDataSetter(AveClientSideWebPart sender, string htmlPropertiesData);

    abstract class ClientSideWebPartCommonWorker
    {
        protected abstract AveLogger logger { get; }//AveLogger.GetInstance(typeof(ClientSideWebPartCommonWorker));

        internal delegate void PropertiesJsonExtendedEvent(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, ref bool needPost);
        internal delegate bool HtmlPropertiesDataExtendedEvent(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, ref string updatedHtmlPropertiesData);

        protected static HtmlProperitesDataSetter HtmlDataSetter;

        //Extended event for individual derived workers
        protected event PropertiesJsonExtendedEvent HandlePropertiesJsonExtended;
        protected event HtmlPropertiesDataExtendedEvent HandleHtmlPropertiesDataExtended;

        protected virtual List<ClientSideWebpartProperty> PotentialPropertiesAndTypes { get { return new List<ClientSideWebpartProperty>(); } }
        protected virtual List<ClientSideWebpartProperty> PotentialHtmlPropertiesAndTypes { get { return new List<ClientSideWebpartProperty>(); } }

        protected virtual List<String> ServerProcessedContentProperties { get { return new List<string>() { "imageSources", "links" }; } }

        protected virtual string UserLoginPrefix { get { return ""; } }

        static ClientSideWebPartCommonWorker()
        {
            HtmlDataSetter = GetFieldSetter(typeof(AveClientSideWebPart), "htmlPropertiesData");
        }

        private static HtmlProperitesDataSetter GetFieldSetter(Type type, string field)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField;
            FieldInfo htmlDataField = type.GetField(field, flag);
            return htmlDataField.SetValue;
        }

        #region--------------------------------------VirtualMethods--------------------------------------
        public virtual bool Process(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost)
        {
            var requirePostAction = false;
            try
            {
                logger.Info($"begin to handle webpart for document, WebpartId:{webPart.WebPartId}, IsPostAction:{lastPost}");
                ProcessProperties(webPart, document, lastPost, ref requirePostAction);
                ProcessHtmlPropertiesData(webPart, document, lastPost, ref requirePostAction);
                logger.Info($"finish to handle webpart for document, WebpartId:{webPart.WebPartId}, Need post action:{requirePostAction}");

            }
            catch (Exception ex)
            {
                requirePostAction = true;
                logger.Warn($"Failed to handle webpart for document, WebpartId:{webPart.WebPartId}, Message:{ex.ToDetailedString()}");
            }
            return !requirePostAction;
        }

        protected void ProcessProperties(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            var propertiesJson = HandlePropertiesByType(webPart.PropertiesJson, document, lastPost, PotentialPropertiesAndTypes, ref needPost);
            HandleServerProcessedContent(webPart, document, lastPost, ref needPost);
            webPart.PropertiesJson = propertiesJson;
            HandlePropertiesJsonExtended?.Invoke(webPart, document, lastPost, ref needPost);
        }

        /// <summary>
        /// Handle display html properties
        /// </summary>
        /// <param name="webPart">Client side WebPart</param>
        /// <param name="document">Client side page document</param>
        /// <param name="lastPost">Post action</param>
        /// <returns></returns>
        protected void ProcessHtmlPropertiesData(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            string updatedHtmlPropertiesData;
            if (!needPost || lastPost)
            {
                HandleHtmlPropertiesData(webPart, document, lastPost, out updatedHtmlPropertiesData);
                HandleHtmlPropertiesDataExtended?.Invoke(webPart, document, lastPost, ref updatedHtmlPropertiesData);
                HtmlDataSetter(webPart, updatedHtmlPropertiesData);
            }
        }

        protected virtual string HandlePropertiesByType(string propertiesJson, AveSPDoc document, bool lastPost, List<ClientSideWebpartProperty> potentialPropertiesAndTypes, ref bool needPost)
        {
            bool postponed = needPost;
            JObject allProperties = JObject.Parse(propertiesJson);

            Action<JProperty, List<ClientSideWebpartProperty>> enumerableHandler = (tempJProperty, tempCSWPPList) =>
            {
                if (postponed && !lastPost)
                {
                    return;
                }
                HandleSingleJProperty(tempJProperty, (input) =>
                {
                    JObject value = input as JObject;
                    if (input is JProperty)
                    {
                        value = ((JProperty)input).Value as JObject;
                    }
                    string updated = HandlePropertiesByType(value.ToString(), document, lastPost, tempCSWPPList, ref postponed);
                    return Newtonsoft.Json.Linq.JObject.Parse(updated);
                });
            };
            Action<JProperty, ClientSideWebpartProperty> singleHandler = (tempJProperty, tempCSWPP) =>
            {
                if (postponed && !lastPost)
                {
                    return;
                }
                HandleOnePropertyByType(tempJProperty, document, lastPost, tempCSWPP, ref postponed);
            };

            HandleProperties4JObject(allProperties, potentialPropertiesAndTypes, enumerableHandler, singleHandler);

            needPost = postponed;

            return allProperties.ToString();
        }

        /// <summary>
        /// Update client side WebPart ServerProcessedContent property, which will change PropertiesJson, need to change it back.
        /// </summary>
        /// <param name="webpart"></param>
        /// <param name="propertiesJson"></param>
        /// <param name="document"></param>
        /// <param name="lastPost"></param>
        /// <param name="needPost"></param>
        /// <returns></returns>
        protected virtual void HandleServerProcessedContent(AveClientSideWebPart webpart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            JObject serverProcessedContent = webpart.ServerProcessedContent;
            if (serverProcessedContent == null || (serverProcessedContent.Count <= 0))
            {
                return;
            }

            foreach (string propertyName in ServerProcessedContentProperties)
            {
                if (serverProcessedContent.ContainsKey(propertyName))
                {
                    JProperty jProperty = serverProcessedContent.Property(propertyName);
                    try
                    {
                        JToken result;
                        bool postponed = needPost;
                        Func<JToken, JToken> handler = (input) =>
                        {
                            JObject temp = input as JObject;
                            if (input is JProperty)
                            {
                                temp = ((JProperty)input).Value as JObject;
                            }
                            if (postponed && !lastPost)
                            {
                                return temp;
                            }
                            if (!HandleAmbiguousProperty(temp.ToString(), document, lastPost, ref postponed, out result))
                            {
                                logger.Warn($"Server processed content needs a post action due to {propertyName}.");
                                return temp;
                            }
                            return result;
                        };
                        HandleSingleJProperty(jProperty, handler);
                        needPost = postponed;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Can not handle server processed content due to property [{jProperty.Name}], message:{ex.ToString()}");
                    }
                }
            }

            //Set ServerProcessedContent
            JObject propertiesToJObject = new JObject();
            propertiesToJObject["serverProcessedContent"] = serverProcessedContent;
            webpart.PropertiesJson = propertiesToJObject.ToString();
        }
        
        protected bool HandleHtmlPropertiesData(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, out string updatedPropertiesData)
        {
            bool needPost = false;

            var parserOptions = new HtmlParserOptions() { IsEmbedded = true };
            Dictionary<string, string> linkTags = new Dictionary<string, string> { { "img", "src" }, { "a", "href" } };
            using (IHtmlDocument html = new HtmlParser(parserOptions).ParseDocument(webPart.HtmlPropertiesData))
            {
                foreach (var pair in linkTags)
                {
                    var images = html.GetElementsByTagName(pair.Key);
                    if (images.Length <= 0)
                    {
                        continue;
                    }
                    foreach (var image in images)
                    {
                        var tempElement = image;
                        string src = tempElement.GetAttribute(pair.Value);
                        if (!string.IsNullOrEmpty(src))
                        {
                            string mappedValue = src;
                            needPost = !ReplaceUrl(ref mappedValue, document.ParentSite);
                            if (!needPost || lastPost)
                            {
                                tempElement.SetAttribute(pair.Value, mappedValue);
                            }
                        }
                    }
                }
                updatedPropertiesData = html.DocumentElement.OuterHtml;
            }

            return needPost;
        }

        protected virtual bool ReplacePropertyByType(AveSPSite site, JProperty property, ClientSideWebpartPropertyTypes propertyType, ClientSideWebpartPropertyScopes scope, out JToken result)
        {
            bool updated = false;
            object value = null;
            JToken original = property.Value;
            switch (propertyType)
            {
                case ClientSideWebpartPropertyTypes.Guid:
                    Guid guid = default(Guid);
                    updated = ReplaceGUID(original.ToObject<Guid>(), site, scope, out guid);
                    value = guid;
                    break;
                case ClientSideWebpartPropertyTypes.Url:
                    string url = original.ToObject<string>();
                    updated = ReplaceUrl(ref url, site);
                    value = url;
                    break;
                case ClientSideWebpartPropertyTypes.UserUPN:
                case ClientSideWebpartPropertyTypes.UserLoginName:
                    string originalLogin = original.ToObject<string>();
                    //Only custom user mapping will take effect
                    string mappedLogin = site.SPMembers.GetMappingUserLogin(originalLogin.Trim(true, false, UserLoginPrefix));
                    updated = !string.IsNullOrEmpty(mappedLogin);
                    value = propertyType == ClientSideWebpartPropertyTypes.UserLoginName ? string.Concat(UserLoginPrefix, mappedLogin) : mappedLogin;
                    break;
                case ClientSideWebpartPropertyTypes.Invalid:
                default:
                    throw new NotSupportedException("Type is not supported.");
            }
            result = new JValue(value);
            return updated;
        }
        #endregion

        protected bool HandleOnePropertyByType(JProperty jProperty, AveSPDoc document, bool lastPost, ClientSideWebpartProperty property, ref bool needPost)
        {
            bool updated = false;

            if (jProperty == null || jProperty.Value == null || string.IsNullOrEmpty(jProperty.Value.ToString()))
            {
                updated = true;
                return updated;
            }

            JToken result;
            bool postponed = needPost;
            Func<JToken, JToken> handler = (input) =>
            {
                JProperty temp = input as JProperty;
                if (input is JObject)
                {
                    temp = ((JObject)input).Property(property.Name);
                }
                if (postponed && !lastPost)
                {
                    return temp.Value;
                }
                if (!(updated = ReplacePropertyByType(document.ParentSite, temp, property.PropertyType, property.Scope, out result)))
                {
                    logger.Warn($"Server processed content needs a post action due to {property.Name}.");
                    postponed = true;
                    return temp.Value;
                }
                return result;
            };
            HandleSingleJProperty(jProperty, handler);
            needPost = postponed;

            return updated;
        }

        protected bool HandleAmbiguousProperty(string inputJson, AveSPDoc document, bool lastPost, ref bool needPost, out JToken result)
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
                if (!tempProperty.Name.EndsWith("Url", StringComparison.OrdinalIgnoreCase) &&
                    !tempProperty.Name.EndsWith("Link", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var value = tempProperty.Value.ToString();
                updated = ReplaceUrl(ref value, document.ParentSite);
                if (!updated && !lastPost)
                {
                    logger.Warn($"Property [{tempProperty.Name}] needs a post action.");
                    needPost = true;
                    break;
                }
                tempProperty.Value = new JValue(value);
                updated = true;
            }
            result = valueObject;
            return updated;
        }

        #region--------------------------------------ReplaceMethods--------------------------------------
        protected virtual bool ReplaceUrl(ref string sourceUrl, AveSPSite site)
        {
            if (IsExternalUrl(sourceUrl, site))
            {
                //External url not supported.
                return true;
            }
            sourceUrl = ReplaceUrl(sourceUrl, site);
            bool updated = ReplaceQueryString(ref sourceUrl, site);
            return updated;
        }

        protected string ReplaceUrl(string sourceUrl, AveSPSite site)
        {
            return AveReplaceProcessor.UrlReplace(sourceUrl,
                            site.MappingManager.SiteMappingManager.SiteManagedMappings,
                            new ReplaceOption(true, true),
                            site.SourceSiteInfo, site.ServerRelativeUrl);
        }

        private bool ReplaceQueryString(ref string url, AveSPSite site)
        {
            bool updated = false;

            Uri temp = new Uri(url, UriKind.RelativeOrAbsolute);
            if (!temp.IsAbsoluteUri)
            {
                string fakeHost = "http://www.fakehost.com";
                string fakeUrl = string.Concat(fakeHost, url);
                temp = new Uri(fakeUrl);
            }
            UrlQueryString query = new UrlQueryString(temp.Query);
            if (query.Queries.Count <= 0)
            {
                updated = true;
                return updated;
            }
            foreach (ClientSideWebpartProperty property in PotentialHtmlPropertiesAndTypes)
            {
                if (query.Keys.Contains(property.Name))
                {
                    JToken result = null;
                    if (!ReplacePropertyByType(site, query.Queries.Property(property.Name), property.PropertyType, property.Scope, out result))
                    {
                        logger.Warn($"Can not update url query string due to query parameter:{property.Name}");
                        updated = false;
                        break;
                    }
                    query[property.Name] = result.ToObject<string>();
                    updated = true;
                }
            }
            url = string.Concat(temp.LocalPath, query.ToString());

            return updated;
        }

        private bool IsExternalUrl(string url, AveSPSite site)
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return !(url.StartsWith(site.SiteUrl, StringComparison.OrdinalIgnoreCase) || url.StartsWith(site.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual bool ReplaceGUID(Guid input, AveSPSite site, ClientSideWebpartPropertyScopes scope, out Guid result)
        {
            bool updated = false;
            result = input;
            var mapping = GetGUIDMappingByScope(site, scope);
            if (mapping.ContainsKey(input))
            {
                updated = true;
                result = AveReplaceProcessor.GuidReplace(input, mapping);
            }
            return updated;
        }

        private Dictionary<Guid, Guid> GetGUIDMappingByScope(AveSPSite site, ClientSideWebpartPropertyScopes scope)
        {
            switch (scope)
            {
                case ClientSideWebpartPropertyScopes.SiteCollection:
                    var mapping = new Dictionary<Guid, Guid>();
                    //SourceSiteInfo is the backup site info, SourceSiteId is from the site header
                    Guid sourceSiteId = site.SourceSiteInfo.UniqueId != Guid.Empty ? site.SourceSiteInfo.UniqueId : site.SourceHeaderSiteId;
                    if (sourceSiteId != Guid.Empty)
                    {
                        mapping[sourceSiteId] = site.SPSite.ID;
                    }
                    mapping[site.SPSite.ID] = site.SPSite.ID;
                    return mapping;//Not Supported at the moment
                case ClientSideWebpartPropertyScopes.Site:
                    return site.MappingManager.SiteMappingManager.WebIDMapping;
                case ClientSideWebpartPropertyScopes.List:
                    return site.MappingManager.SiteMappingManager.ListIdMapping;
                case ClientSideWebpartPropertyScopes.Folder:
                    return new Dictionary<Guid, Guid>();//Not Supported at the moment
                case ClientSideWebpartPropertyScopes.Item:
                    return site.MappingManager.SiteMappingManager.DocumentUniqueIdMapping;
                case ClientSideWebpartPropertyScopes.View:
                    return site.MappingManager.SiteMappingManager.ViewGuidMapping;
                case ClientSideWebpartPropertyScopes.Invalid:
                default:
                    throw new NotSupportedException("Mapping scope is not supported.");
            }
        }
        #endregion

        protected void HandleSingleJProperty(JProperty jProperty, Func<JToken, JToken> handler)
        {
            if (jProperty == null || jProperty.Value == null || string.IsNullOrEmpty(jProperty.Value.ToString()))
            {
                return;
            }
            JToken result;
            if (jProperty.Value is JArray)
            {
                var valueList = jProperty.Value as JArray;
                for (int i = 0; i < valueList.Count; i++)
                {
                    var oneValue = valueList[i];
                    if (oneValue is JObject)
                    {
                        var oneObject = (JObject)oneValue;
                        result = handler(oneObject);
                        valueList[i] = result;
                    }
                }
                jProperty.Value = valueList;
                return;
            }
            result = handler(jProperty);
            jProperty.Value = result;
        }

        protected void HandleProperties4JObject(JObject jObject, List<ClientSideWebpartProperty> properties, Action<JProperty, List<ClientSideWebpartProperty>> HandleEnumerableProperty, Action<JProperty,ClientSideWebpartProperty> HandleSingleProperty)
        {
            IEnumerator<ClientSideWebpartProperty> enumerator = properties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var property = enumerator.Current;
                try
                {
                    string name = property.Name;
                    if (!jObject.ContainsKey(name))
                    {
                        continue;
                    }
                    JProperty temp = jObject.Property(name);
                    if (property.IsEnumerable)
                    {
                        HandleEnumerableProperty(temp, property.ChildProperties);
                        continue;
                    }
                    HandleSingleProperty(temp, property);
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to replace property:{property.Name}, message:{ex.ToString()}");
                }
            }
        }
    }

    class UrlQueryString
    {
        private const char prefix = '?';
        private const char seperator = '&';
        private const char pair = '=';

        private string original;

        Dictionary<string, string> queryPairs = new Dictionary<string, string>();

        public UrlQueryString(string queryString)
        {
            this.original = queryString;
            Initialize(this.original);
        }

        private void Initialize(string query)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(query.TrimStart(new char[] { prefix })))
            {
                return;
            }

            string decoded = HttpUtility.UrlDecode(query);

            StringBuilder builder = new StringBuilder();
            string key = null;
            string value = null;
            int index = 0;
            foreach (char temp in decoded)
            {
                if (temp == '?' && index == 0)
                {
                    continue;
                }
                if (temp == '&')
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new InvalidCastException($"Invalid url query string. query:{query}");
                    }

                    value = builder.ToString();
                    builder.Clear();
                    queryPairs[key] = value;
                    key = null;
                    value = null;
                    continue;
                }
                if (temp == '=')
                {
                    key = builder.ToString();
                    builder.Clear();
                    continue;
                }
                builder.Append(temp);
                index++;
            }

            if (builder.Length > 0)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidCastException($"Invalid url query string. query:{query}");
                }
                value = builder.ToString();
                builder.Clear();
                queryPairs[key] = value;
                builder.Clear();
            }
        }

        public override string ToString()
        {
            if (queryPairs.Count <= 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(prefix);
            int counter = 1;
            foreach (string key in queryPairs.Keys)
            {
                builder.Append(key);
                builder.Append(pair);
                builder.Append(HttpUtility.UrlEncode(queryPairs[key]));
                if (counter++ < queryPairs.Count)
                {
                    builder.Append(seperator);
                }
            }
            return builder.ToString();
        }

        public string this[string key]
        {
            get
            {
                if (queryPairs.ContainsKey(key))
                {
                    return queryPairs[key];
                }
                return string.Empty;
            }
            set
            {
                queryPairs[key] = value;
            }
        }

        public JObject Queries
        {
            get
            {
                return JObject.FromObject(queryPairs);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return queryPairs.Keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return queryPairs.Values;
            }
        }

    }

    struct ClientSideWebpartProperty : IEnumerable<ClientSideWebpartProperty>
    {
        public string Name;
        public ClientSideWebpartPropertyTypes PropertyType;
        public string TypeOfString;
        public ClientSideWebpartPropertyScopes Scope;
        public bool IsEnumerable;
        public List<ClientSideWebpartProperty> ChildProperties;

        public ClientSideWebpartProperty(string name, ClientSideWebpartPropertyTypes type, ClientSideWebpartPropertyScopes scope = default(ClientSideWebpartPropertyScopes), bool isEnumerable = false)
        {
            Name = name;
            PropertyType = type;
            TypeOfString = PropertyType.ToString();
            this.Scope = scope;
            IsEnumerable = isEnumerable;
            ChildProperties = new List<ClientSideWebpartProperty>();
        }
        public ClientSideWebpartProperty(string name, ClientSideWebpartPropertyTypes type, List<ClientSideWebpartProperty> children, ClientSideWebpartPropertyScopes scope = default(ClientSideWebpartPropertyScopes))
        {
            Name = name;
            PropertyType = type;
            TypeOfString = PropertyType.ToString();
            this.Scope = scope;
            IsEnumerable = true;
            ChildProperties = children;
        }

        public IEnumerator<ClientSideWebpartProperty> GetEnumerator()
        {
            return ChildProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public enum ClientSideWebpartPropertyTypes
    {
        Invalid,
        Text,
        Url,
        Guid,
        UserLoginName,
        UserUPN,
    }

    public enum ClientSideWebpartPropertyScopes
    {
        Invalid,
        SiteCollection,
        Site,
        List,
        Folder,
        Item, //including document, listitem, attachment
        View,
    }
}
