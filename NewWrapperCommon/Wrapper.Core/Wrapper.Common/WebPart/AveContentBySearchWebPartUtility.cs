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
using System.Web.Script.Serialization;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveContentBySearchWebPartUtility
    {
        public static string UpdateDataProviderJsonProperty(string oldValue, AveWebPartCache cache)
        {
            var dataProviderJson = oldValue;
            if (!string.IsNullOrEmpty(dataProviderJson))
            {
                var changed = false;
                var js = new JavaScriptSerializer();
                var values = js.Deserialize<Dictionary<string, object>>(dataProviderJson);
                var url = string.Empty;
                var replacedUrl = string.Empty;
                object obj;
                if (values.TryGetValue("Properties", out obj))
                {
                    var props = obj as Dictionary<string, object>;
                    if (props != null && props.Count > 0 && props.TryGetValue("Scope", out obj))
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

                if (changed)
                {
                    obj = null;
                    if (values.TryGetValue("PropertiesJson", out obj))
                    {
                        if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        {
                            var propertiesJson = obj.ToString();
                            propertiesJson = propertiesJson.Replace(url, replacedUrl);
                            values["PropertiesJson"] = propertiesJson;
                        }
                    }
                    dataProviderJson = js.Serialize(values);
                }
            }
            return dataProviderJson;
        }
    }
}
