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
namespace AvePoint.RA.I18N.Core
{
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
     
    public class RMResourceManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMResourceManager));
        private static readonly Dictionary<int, ResourceCacheObjct> resxCache = new Dictionary<int, ResourceCacheObjct>();
        private static readonly int defaultLCID = 0;

        static RMResourceManager()
        {
            CultureInfo[] cultures = new CultureInfo[] { new CultureInfo("en-US"), new CultureInfo("ja-JP"), new CultureInfo("zh-CN"), new CultureInfo("ko-KR"), new CultureInfo("fr-FR"), new CultureInfo("fr-CA") };
            foreach (CultureInfo culture in cultures)
            { 
                Dictionary<String, String> resx = null;
                string jsresx = null;
                int lcid = culture.LCID;
                if (TryGetResources(culture, out jsresx, out resx))
                { 
                    string version = "3.0.0.0000";
                    resxCache[lcid] = new ResourceCacheObjct(version, jsresx, resx);
                    logger.Info($"Current culture lcid is {lcid}.");
                } 
            }
            defaultLCID = cultures[0].LCID;
        }

        /// <summary>
        /// 获取请求Resource Script的Url
        /// </summary>
        public static String GetJsResourceUrl(string version = "")
        {
            //return string.Format("/RMWeb/JsResx?version={0}", GetResourceVersion(Thread.CurrentThread.CurrentUICulture.LCID));
            var resVersion = version;
            if (string.IsNullOrEmpty(resVersion))
            {
                resVersion = GetResourceVersion(Thread.CurrentThread.CurrentUICulture.LCID);
            }
            logger.Info($"Current resVersion is {resVersion}, default lcid is {defaultLCID},current culture is {Thread.CurrentThread.CurrentUICulture.LCID}, name is {Thread.CurrentThread.CurrentUICulture.Name}");
            if (Thread.CurrentThread.CurrentUICulture.LCID != defaultLCID)
            {
                return string.Format("/RMWeb/JsResx?version={0}&_d={1}", resVersion, DateTime.UtcNow.Ticks);
            }
            else
            {
                return string.Format("/RMWeb/JsResx?version={0}", resVersion);
            }
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
                logger.Info($"Current lcid is {lcid}.");
                return rco.JSResx;
            }
            else if (lcid != defaultLCID && resxCache.TryGetValue(defaultLCID, out rco))
            {
                logger.Info($"Current lcid is {lcid}.");
                return rco.JSResx;
            }
            else
            {
                logger.Info($"Current lcid is {lcid}, jsResx is var RMResx= null");
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
        private static bool TryGetResources(CultureInfo culture, out String jsress, out Dictionary<String, String> ress)
        {
            bool result = true;
            ress = new Dictionary<String, String>();
            var jssb = new StringBuilder(50000);
            jssb.AppendLine("var RMResx={");
            try
            {
                if (resxCache.TryGetValue(culture.LCID, out ResourceCacheObjct cache)) 
                {
                    jsress = cache.JSResx;
                    ress = cache.Resx;
                    return true;
                }
                string val;
                string key;
                var id = AvePoint.RA.I18N.Resources.RecordAutomation.ResourceManager.GetResourceSet(culture, true, false).GetEnumerator();
                //var id = resourceReader.GetEnumerator();
                while (id.MoveNext())
                {
                    val = id.Value.ToString();
                    key = id.Key.ToString();
                    ress[key] = val;
                    jssb.AppendLine("'" + key + "': " + "'" + val.Replace("\'", "\\\'").Replace("\r", "").Replace("\n", "") + "',");
                    //if (key.StartsWith("RM_JS_"))
                    //{
                    //    jssb.AppendLine(key + ": " + "'" + val.Replace("\'", "\\\'") + "',");
                    //}
                }
            }
            catch (Exception ex)
            { 
                result = false;
            }
            jssb.AppendLine("};");
            jsress = jssb.ToString();
            return result;
        }
    }
     
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
