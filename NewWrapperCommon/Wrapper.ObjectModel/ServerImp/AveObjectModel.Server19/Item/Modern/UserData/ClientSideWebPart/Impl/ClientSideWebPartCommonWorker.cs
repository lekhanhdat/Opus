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
namespace AvePoint.ObjectModel.Server19
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Newtonsoft.Json.Linq;
    using OfficeDevPnP.Core.Pages;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Collections;
    using Microsoft.SharePoint;
    using AngleSharp.Html.Parser;
    using AngleSharp.Html.Dom;

    public delegate void HtmlProperitesDataSetter(ClientSideWebPart sender, string htmlPropertiesData);

    abstract class ClientSideWebPartCommonWorker
    {

        protected static HtmlProperitesDataSetter HtmlDataSetter;

        protected static AveLogger logger = AveLogger.GetInstance(typeof(ClientSideWebPartCommonWorker));

        protected abstract List<ClientSideWebpartProperty> PotentialPropertiesAndTypes { get; }

        protected abstract List<ClientSideWebpartProperty> PotentialHtmlPropertiesAndTypes { get; }

        protected virtual List<String> ServerProcessedContentProperties { get { return new List<string>() { "imageSources", "links" }; } }

        protected virtual string UserLoginPrefix { get { return ""; } }

        static ClientSideWebPartCommonWorker()
        {
            HtmlDataSetter = GetFieldSetter(typeof(ClientSideWebPart), "htmlPropertiesData");
        }

        private static HtmlProperitesDataSetter GetFieldSetter(Type type, string field)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField;
            FieldInfo htmlDataField = type.GetField(field, flag);
            return htmlDataField.SetValue;
        }

        #region--------------------------------------VirtualMethods--------------------------------------
        public virtual bool Process(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost)
        {
            var requirePostAction = false;
            logger.Info($"begin to handle webpart for document:{document.Name}, IsPostAction:{lastPost}");
            ProcessProperties(webPart, document, mapping, lastPost, ref requirePostAction);
            ProcessHtmlPropertiesData(webPart, document, mapping, lastPost, ref requirePostAction);
            logger.Info($"finish to handle webpart for document:{document.Name}, Need post action:{requirePostAction}");
            return !requirePostAction;
        }

        protected void ProcessProperties(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref bool needPost)
        {
            var propertiesJson = HandlePropertiesByType(webPart.PropertiesJson, document, mapping, lastPost, PotentialPropertiesAndTypes, ref needPost);
            HandleServerProcessedContent(webPart, document, mapping, lastPost, ref needPost);
            webPart.PropertiesJson = propertiesJson;

            HandlePropertiesJsonExtended(webPart, document, mapping, lastPost, ref needPost);
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
        protected virtual void HandleServerProcessedContent(ClientSideWebPart webpart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref bool needPost)
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
                        JToken value = jProperty.Value;
                        JToken result;
                        if (value is JArray)
                        {
                            JArray valueArry = value as JArray;
                            for (int i = 0; i < valueArry.Count; i++)
                            {
                                var oneValue = valueArry[i];
                                bool updated = HandleAmbiguousProperty(oneValue.ToString(), document, mapping, lastPost, ref needPost, out result);
                                if (!updated && !lastPost)
                                {
                                    logger.Warn($"Server processed content needs a post action due to {propertyName}.");
                                    needPost = true;
                                    break;
                                }
                                valueArry[i] = result;
                            }
                            jProperty.Value = valueArry;
                            continue;
                        }
                        if (!HandleAmbiguousProperty(value.ToString(), document, mapping, lastPost, ref needPost, out result) && !lastPost)
                        {
                            logger.Warn($"Server processed content needs a post action due to {propertyName}.");
                            needPost = true;
                            break;
                        }
                        jProperty.Value = result;
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

        protected virtual string HandlePropertiesByType(string propertiesJson, IAveFile document, AveSiteMappingManager mapping, bool lastPost, List<ClientSideWebpartProperty> potentialPropertiesAndTypes, ref bool needPost)
        {
            //bool updated = false;
            IEnumerator<ClientSideWebpartProperty> enumerator = potentialPropertiesAndTypes.GetEnumerator();
            Newtonsoft.Json.Linq.JObject allProperties = Newtonsoft.Json.Linq.JObject.Parse(propertiesJson);
            while (enumerator.MoveNext())
            {
                var property = enumerator.Current;
                try
                {
                    string name = property.Name;
                    if (!allProperties.ContainsKey(name))
                    {
                        continue;
                    }
                    if (property.IsEnumerable)
                    {
                        JToken enumerableValue = allProperties.Property(name).Value;
                        if (enumerableValue is JArray)
                        {
                            JArray arry = enumerableValue as JArray;
                            for (int i = 0; i < arry.Count; i++)
                            {
                                JObject oneObject = arry[i] as JObject;
                                string updateObject = HandlePropertiesByType(oneObject.ToString(), document, mapping, lastPost, property.ChildProperties, ref needPost);
                                arry[i] = Newtonsoft.Json.Linq.JObject.Parse(updateObject);
                            }
                            allProperties[name] = arry;
                            continue;
                        }
                        var propertyObj = enumerableValue as JObject;
                        //string wrapJProperty = propertyValue.StartsWith("{") && propertyValue.EndsWith("}") ? propertyValue : string.Concat("{", allProperties.Property(name).Value, "}");
                        string updatedValue = HandlePropertiesByType(propertyObj.ToString(), document, mapping, lastPost, property.ChildProperties, ref needPost);
                        allProperties.Property(name).Value = Newtonsoft.Json.Linq.JObject.Parse(updatedValue);
                        continue;
                    }
                    JProperty jProperty = allProperties.Property(name);
                    if (!HandleOnePropertyByType(jProperty, document, mapping, lastPost, property, ref needPost))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to replace property:{property.Name}, message:{ex.ToString()}");
                }
            }

            return allProperties.ToString();
        }

        protected virtual bool ReplacePropertyByType(IAveSite site, JProperty property, ClientSideWebpartPropertyTypes propertyType, ClientSideWebpartPropertyScopes scope, out JToken result)
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
                    string mappedLogin = originalLogin.Trim(true, false, UserLoginPrefix);
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
        
        /// <summary>
        /// Handle display html properties
        /// </summary>
        /// <param name="webPart">Client side WebPart</param>
        /// <param name="document">Client side page document</param>
        /// <param name="lastPost">Post action</param>
        /// <returns></returns>
        protected void ProcessHtmlPropertiesData(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref bool needPost)
        {
            string updatedHtmlPropertiesData;
            if (!needPost || lastPost)
            {
                HandleHtmlPropertiesData(webPart, document, mapping, lastPost, out updatedHtmlPropertiesData);
                HandleHtmlPropertiesDataExtended(webPart, document, mapping, lastPost, ref updatedHtmlPropertiesData);
                HtmlDataSetter(webPart, updatedHtmlPropertiesData);
            }
        }

        protected bool HandleHtmlPropertiesData(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, out string updatedPropertiesData)
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
                            needPost = !ReplaceUrl(ref mappedValue, document.Web.Site);
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
        
        /// <summary>
        /// Extension for ProcessProperties, need to be override.
        /// </summary>
        /// <param name="webPart"></param>
        /// <param name="document"></param>
        /// <param name="lastPost"></param>
        /// <param name="needPost"></param>
        protected virtual void HandlePropertiesJsonExtended(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref bool needPost)
        {
        }

        /// <summary>
        /// Extension for ProcessHtmlPropertiesData, need to be override.
        /// </summary>
        /// <param name="webPart"></param>
        /// <param name="document"></param>
        /// <param name="lastPost"></param>
        /// <param name="updatedHtmlPropertiesData"></param>
        /// <returns></returns>
        protected virtual bool HandleHtmlPropertiesDataExtended(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref string updatedHtmlPropertiesData)
        {
            return false;
        }
        #endregion
        
        private bool HandleOnePropertyByType(JProperty jProperty, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ClientSideWebpartProperty property, ref bool needPost)
        {
            bool updated = false;

            if (jProperty == null || jProperty.Value == null)
            {
                updated = true;
                return updated;
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
                        var oneObject = oneValue as JObject;
                        var oneProperty = oneObject.Property(property.Name);
                        if (!ReplacePropertyByType(document.Web.Site, oneProperty, property.PropertyType, property.Scope, out result) && !lastPost)
                        {
                            //break once a property needs post action.
                            needPost = true;
                            logger.Warn($"Webpart needs to be postponed due to property {property.Name}");
                            return updated;
                        }
                        valueList[i] = result;
                    }
                }
                jProperty.Value = valueList;
                updated = true;
                return updated;
            }
            if (!ReplacePropertyByType(document.Web.Site, jProperty, property.PropertyType, property.Scope, out result) && !lastPost)
            {
                //break once a property needs post action.
                needPost = true;
                logger.Warn($"Webpart needs to be postponed due to property {property.Name}");
                return updated;
            }
            jProperty.Value = result;
            updated = true;
            return updated;
        }

        private bool HandleAmbiguousProperty(string inputJson, IAveFile document, AveSiteMappingManager mapping, bool lastPost, ref bool needPost, out JToken result)
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
                if (!tempProperty.Name.EndsWith("Url", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var value = tempProperty.Value.ToString();
                updated = ReplaceUrl(ref value, document.Web.Site);
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
        protected virtual bool ReplaceUrl(ref string sourceUrl, IAveSite site)
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

        protected string ReplaceUrl(string sourceUrl, IAveSite site)
        {
            return AveReplaceProcessor.UrlReplace(sourceUrl,
                            this.mapping.SiteManagedMappings,
                            new ReplaceOption(true, true),
                            this.sourceSitInfo, site.ServerRelativeUrl);
        }

        private bool ReplaceQueryString(ref string url, IAveSite site)
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

        private bool IsExternalUrl(string url, IAveSite site)
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return !(url.StartsWith(site.Url, StringComparison.OrdinalIgnoreCase) || url.StartsWith(this.sourceSitInfo.Url, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual bool ReplaceGUID(Guid input, IAveSite site, ClientSideWebpartPropertyScopes scope, out Guid result)
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

        private Dictionary<Guid, Guid> GetGUIDMappingByScope(IAveSite site, ClientSideWebpartPropertyScopes scope)
        {
            switch (scope)
            {
                case ClientSideWebpartPropertyScopes.SiteCollection:
                    //SourceSiteInfo is the backup site info, SourceSiteId is from the site header
                    Guid sourceSiteId = this.sourceSitInfo.Id;
                    if (sourceSiteId != Guid.Empty && sourceSiteId != site.ID)
                    {
                        return new Dictionary<Guid, Guid>() {
                            { sourceSiteId, site.ID },
                            { site.ID, site.ID }
                        };
                    }
                    return new Dictionary<Guid, Guid>() { { site.ID, site.ID } };//Not Supported at the moment
                case ClientSideWebpartPropertyScopes.Site:
                    return this.mapping.WebIDMapping;
                case ClientSideWebpartPropertyScopes.List:
                    return this.mapping.ListIdMapping;
                case ClientSideWebpartPropertyScopes.Folder:
                    return new Dictionary<Guid, Guid>();//Not Supported at the moment
                case ClientSideWebpartPropertyScopes.Item:
                    return this.mapping.DocumentUniqueIdMapping;
                case ClientSideWebpartPropertyScopes.Invalid:
                default:
                    throw new NotSupportedException("Mapping scope is not supported.");
            }
        }
        #endregion

        private AveSiteMappingManager mapping;
        private AveSiteInfo sourceSitInfo;
        public void SetMappingAndSourceInfo(AveSiteMappingManager mapping, AveSiteInfo sourceSitInfo)
        {
            this.mapping = mapping;
            this.sourceSitInfo = sourceSitInfo;
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
        Item //including document, listitem, attachment
    }
}
