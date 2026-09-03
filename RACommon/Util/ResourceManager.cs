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
namespace AvePoint.RA.Common.Util
{
    using Contract.CodeView;
    using GCommon;
    using GCommon.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    [RACodeReview("Allen Yin", comment: "consider是否会有自动重新load资源文件的需求")]
    public class ResourceManager
    {
        private static readonly Dictionary<int, ResourceCacheObjct> resxCache = new Dictionary<int, ResourceCacheObjct>();
        private static readonly int defaultLCID = 0;
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(ResourceManager));

        static ResourceManager()
        {
            string dirPath = Path.Combine(WebUtil.GetInstallPath(), "App_GlobalResources");
            if (Directory.Exists(dirPath))
            {
                var filePaths = Directory.EnumerateFiles(dirPath);
                var reg = new Regex(@"RMWeb\.((?<lang>[^\.]+)\.)?resx", RegexOptions.IgnoreCase);
                foreach (string filePath in filePaths)
                {
                    var matchs = reg.Matches(filePath.Substring(filePath.LastIndexOf('\\') + 1));
                    if (matchs.Count > 0)
                    {
                        int lcid = defaultLCID;
                        var group = matchs[0].Groups["lang"];
                        if (group.Success)
                        {
                            try
                            {
                                lcid = new CultureInfo(group.Value).LCID;
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        Dictionary<String, String> resx = null;
                        string jsresx = null;
                        if (TryGetResources(filePath, out jsresx, out resx))
                        {
                            var lastmodifiedTime = File.GetLastWriteTime(filePath);
                            string version = HashCodeHelper.ToMD5HashCode(lcid + lastmodifiedTime.ToString(DateTimeFormatInfo.InvariantInfo));
                            resxCache[lcid] = new ResourceCacheObjct(version, jsresx, resx);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取请求Resource Script的Url
        /// </summary>
        public static String GetJsResourceUrl()
        {
            return string.Format("/RMWeb/JsResx?version={0}", ResourceManager.GetResourceVersion(Thread.CurrentThread.CurrentUICulture.LCID));
        }

        public static String GetString(string key)
        {
            return GetString(Thread.CurrentThread.CurrentUICulture.LCID, key);
        }

        public static String GetResourceScript()
        {
            return GetResourceScript(Thread.CurrentThread.CurrentUICulture.LCID);
        }

        private static Dictionary<String, String> GetResource(int lcid)
        {
            ResourceCacheObjct rco = null;
            if (resxCache.TryGetValue(lcid, out rco))
            {
                return rco.Resx;
            }
            else if (lcid != defaultLCID && resxCache.TryGetValue(defaultLCID, out rco))
            {
                return rco.Resx;
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        private static String GetResourceScript(int lcid)
        {
            ResourceCacheObjct rco = null;
            if (resxCache.TryGetValue(lcid, out rco))
            {
                return rco.JSResx;
            }
            else if (lcid != defaultLCID && resxCache.TryGetValue(defaultLCID, out rco))
            {
                return rco.JSResx;
            }
            else
            {
                return "var RMResx={}";
            }
        }

        private static string GetResourceVersion(int lcid)
        {
            ResourceCacheObjct rco = null;
            if (resxCache.TryGetValue(lcid, out rco))
            {
                return rco.Version;
            }
            else if (lcid != defaultLCID && resxCache.TryGetValue(defaultLCID, out rco))
            {
                return rco.Version;
            }
            else
            {
                return "";
            }
        }

        private static String GetString(int lcid, string key)
        {
            var resources = GetResource(lcid);
            string tempRes = null;
            if (resources.TryGetValue(key, out tempRes))
            {
                return tempRes;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 此方法从一个指定的物理资源文件中load资源，生成资源dictionary以及js资源类，
        /// JS 词条必须以RM_JS_开头
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="jsress"></param>
        /// <param name="ress"></param>
        /// <returns></returns>
        private static bool TryGetResources(String filePath, out String jsress, out Dictionary<String, String> ress)
        {
            bool result = true;
            ress = new Dictionary<String, String>();
            var jssb = new StringBuilder(50000);
            jssb.AppendLine("var RMResx={");
            try
            {
                using (var resourceReader = new ResXResourceReader(filePath))
                {
                    string val;
                    string key;
                    var id = resourceReader.GetEnumerator();
                    while (id.MoveNext())
                    {
                        val = id.Value.ToString();
                        key = id.Key.ToString();
                        ress[key] = val;
                        if (key.StartsWith("RM_JS_"))
                        {
                            jssb.AppendLine(key + ": " + "'" + val.Replace("\'", "\\\'") + "',");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("load resource error: file path:[{0}] , exception: {1}", filePath, ex.ToString()));
                result = false;
            }
            jssb.AppendLine("};");
            jsress = jssb.ToString();
            return result;
        }
    }

    [RACodeReview("Allen Yin")]
    public class ResourceCacheObjct
    {
        public string Version;
        public string JSResx = "";
        public Dictionary<String, String> Resx;

        public ResourceCacheObjct(string version, string jsresx, Dictionary<String, String> resx)
        {
            this.Version = version;
            this.Resx = resx;
            this.JSResx = jsresx;
        }
    }
}
