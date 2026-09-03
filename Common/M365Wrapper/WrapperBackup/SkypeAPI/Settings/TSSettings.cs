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

namespace ExchangeUtility.Graph.SkypeAPI.Settings
{
    using AvePoint.Common;
    using AvePoint.GCommon.Utility;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;

    public class TSSettings : ISingleton
    {
        const string Setting_JS_URL = "https://teams.microsoft.com/package/scripts/settings.js ";
        const string Setting_Name_TS_OVERRIDE_SETTINGS = "window.TS_OVERRIDE_SETTINGS";
        const string Setting_Name_TS_ADDITIONAL_SETTINGS = "window.TS_ADDITIONAL_SETTINGS";
        private readonly string jsonSettings;
        private TSSettings()
        {
            using (var client = new HttpClient())
            {
                this.jsonSettings = client.GetStringAsync(Setting_JS_URL).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            Init();
        }

        public static TSSettings Instance { get { return Singleton<TSSettings>.SingletonInstance; } }

        private void Init()
        {
            var settings = ToKeyValuePair(this.jsonSettings);
            InitOverridSettings(settings);
            InitAdditionalSettings(settings);
        }

        private void InitAdditionalSettings(Dictionary<string, string> settings)
        {
            if (settings.TryGetValue(Setting_Name_TS_ADDITIONAL_SETTINGS, out string additionalSettings))
            {
                //var jObj = JObject.Parse(additionalSettings);
            }
        }

        private void InitOverridSettings(Dictionary<string, string> settings)
        {
            if (settings.TryGetValue(Setting_Name_TS_OVERRIDE_SETTINGS, out string overrideSettings))
            {
                var jObj = JObject.Parse(overrideSettings);
                this.SkypeAuthEndpointAddressV2 = jObj["authConstants"]["skypeAuthEndpointAddressV2"].ToString();

            }
        }

        private static Dictionary<string, string> ToKeyValuePair(string setting)
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StringReader(setting))
            {
                string line;
                while ((line = reader.ReadLine())!= null)
                {
                    var index = line.IndexOf('=');
                    if (index > 0)
                    {
                        var name = line.Substring(0, index).Trim().TrimEnd(';');
                        var value = line.Substring(index + 1).Trim().TrimEnd(';');
                        dic.Add(name, value);
                    }
                }
            }
            return dic;
        }

        //https://teams.microsoft.com/api/authsvc/v1.0/authz
        public string SkypeAuthEndpointAddressV2 { get; private set; }
    }
}