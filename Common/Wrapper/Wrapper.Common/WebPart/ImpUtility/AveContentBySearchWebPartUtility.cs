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

using System;
using System.Collections.Generic;
using System.Xml;
using AvePoint.GCommon;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace AvePoint.Wrapper.Common
{
    public class AveContentBySearchWebPartUtility
    {
        private static bool UpdateQueryTemplate(string oldValue, AveWebPartCache cache, out string newValue)
        {
            bool needPost = false;
            newValue = oldValue;
            Dictionary<string, string> pathMap = new Dictionary<string, string>();
            Dictionary<string, string> listIDMap = new Dictionary<string, string>();
           
            needPost |= GenerateListIdMapping(oldValue, cache, listIDMap);
            if (!needPost)
            {
                needPost |= GeneratePathMapping(oldValue, cache, pathMap);
            }
            if (!needPost)
            {
                var mapping = new Dictionary<string, string>();
                foreach (var key in pathMap.Keys)
                {
                    if (!mapping.ContainsKey(key))
                    {
                        mapping.Add(key, pathMap[key]);
                    }
                }
                foreach (var key in listIDMap.Keys)
                {
                    if (!mapping.ContainsKey(key))
                    {
                        mapping.Add(key, listIDMap[key]);
                    }
                }
                List<string> keys = new List<string> {  };
                foreach (string key in mapping.Keys)
                {
                    keys.Add(key);
                }
                keys.Sort();
                keys.Reverse();
                for (int k = 0; k < keys.Count; k++)
                {
                    string key = keys[k];
                    newValue = Regex.Replace(newValue, key, mapping[key]);
                }
            }
            return needPost;
        }

        private static bool GenerateListIdMapping(string oldValue, AveWebPartCache cache, Dictionary<string, string> mapping)
        {
            bool needPost = false;
            string RegexString = @"ListID[:|=|<|>|>=|<=|..][\S]*";
            var matches = Regex.Matches(oldValue, RegexString);
            foreach (Match match in matches)
            {
                if (match.Captures.Count > 0)
                {
                    for (int k = 0; k < match.Captures.Count; k++)
                    {
                        string value = match.Captures[k].Value.Substring(6).TrimStart(':', '=', '>', '<', ',');
                        needPost |= GenerateListIdMappingAction(cache, mapping, value);
                    }
                }
            }
            return needPost;
        }

        private static bool GenerateListIdMappingAction(AveWebPartCache cache, Dictionary<string, string> mapping, string value)
        {
            bool needPost = false;
            string srcId = value;
            srcId = srcId.ToString().Trim('"');
            if (!string.IsNullOrEmpty(srcId))
            {
                Guid destId;
                if (cache.ListIdMapping.TryGetValue(new Guid(srcId), out destId))
                {
                    if(!mapping.ContainsKey(srcId))
                    mapping.Add(srcId, destId.ToString());
                }
                else
                {
                    needPost = true;
                }
            }
            return needPost;
        }

        private static bool GeneratePathMapping(string oldValue, AveWebPartCache cache,Dictionary<string,string> mapping)
        {
            bool needPost = false;
            string RegexString = @"(?i)Path[:|=|<|>|>=|<=|..][\S]*";
            var matches = Regex.Matches(oldValue, RegexString);
            foreach (Match match in matches)
            {
                
                if (match.Captures.Count > 0)
                {
                    for (int k = 0; k < match.Captures.Count; k++)
                    {
                        string value = match.Captures[k].Value.Substring(4).TrimStart(':', '=', '>', '<', ',', '"');
                        needPost|= GeneratePathMappingAction(cache, mapping, value);
                    }
                }
            }
            return needPost;
        }

        private static bool GeneratePathMappingAction(AveWebPartCache cache, Dictionary<string, string> mapping, string value)
        {
            bool needPost = false;
            string url = value;
            if (url.StartsWith(cache.SourceSiteInfo.Url))
            {
                url = url.ToString().Trim('"');
                if (!string.IsNullOrEmpty(url))
                {
                    var replacedUrl = AveReplaceProcessor.UrlReplace(url, cache.FullUrlMapping, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                    
                    if (string.Equals(url, replacedUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        needPost = true;
                    }
                    else
                    {
                        if (!mapping.ContainsKey(url))
                        {
                            mapping.Add(url, replacedUrl);
                        }
                    }
                }
            }

            return needPost;
        }

        public static string UpdateDataProviderJsonProperty(string oldValue, AveWebPartCache cache,out bool needPostAction)
        {
            needPostAction = false;
            var dataProviderJson = oldValue;
            if (!string.IsNullOrEmpty(dataProviderJson))
            {
                var changed = false;
                //var js = new JavaScriptSerializer();
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataProviderJson);
                var url = string.Empty;
                var replacedUrl = string.Empty;
                object obj;
                if (values.TryGetValue("Properties", out obj))
                {
                    var props = obj as Dictionary<string, object>;
                    if (props != null && props.Count > 0)
                    {
                        if (props.TryGetValue("Scope", out obj))
                        {
                            if (obj != null)
                            {
                                // "\"Url\"" format,need trim
                                url = obj.ToString().Trim('"');
                                if (!string.IsNullOrEmpty(url))
                                {
                                    replacedUrl = AveReplaceProcessor.UrlReplace(url, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                                    if (!string.Equals(url, replacedUrl, StringComparison.OrdinalIgnoreCase))
                                    {
                                        props["Scope"] = string.Format("\"{0}\"", replacedUrl);
                                        values["Properties"] = props;
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (values.TryGetValue("QueryTemplate", out obj))
                {
                    if (obj != null)
                    {
                        changed = true;
                        string template = obj.ToString();
                        string newTemplate = null;
                        if (UpdateQueryTemplate(template, cache, out newTemplate))
                        {
                            values["QueryTemplate"] = newTemplate;
                            //needPostAction
                            needPostAction = true;
                        }
                        else
                        {
                            values["QueryTemplate"] = newTemplate;
                        }
                    }
                }

                if (changed)
                {
                    obj = null;
                    if (values.TryGetValue("PropertiesJson", out obj)
                        &&!string.IsNullOrEmpty(url)
                        &&!string.IsNullOrEmpty(replacedUrl))
                    {
                        if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        {
                            var propertiesJson = obj.ToString();
                            propertiesJson = propertiesJson.Replace(url, replacedUrl);
                            values["PropertiesJson"] = propertiesJson;
                        }
                    }
                    dataProviderJson = JsonConvert.SerializeObject(values);
                }
            }
            return dataProviderJson;
        }
    }
}
