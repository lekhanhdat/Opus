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




namespace AvePoint.Media.Core.Index
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using AvePoint.GCommon;
    using Merged18NResources.MediaCoreIndex;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public static class IndexDatabasePropertiesManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 调用前需要确保volume\fileName文件存在，否则会抛出异常
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="fileName"></param>
        /// <param name="cacheSystem"></param>
        /// <returns></returns>
        public static Dictionary<IndexDatabaseProperties, String> ParseDBProperties(string volume, string fileName, IXSystem cacheSystem)
        {
            var dic = new Dictionary<IndexDatabaseProperties, String>();
            var info = XConvert.FromNames(volume, fileName);
            using (XStream cacheStream = cacheSystem.OpenStream(info, FileMode.Open))
            {
                var lineForLog = string.Empty;
                var sb = new StringBuilder();
                try
                {
                    while (true)
                    {
                        int b = cacheStream.ReadByte();
                        if (b == -1) { break; }
                        else { sb.Append((char)b); }
                    }
                    var xEle = XElement.Parse(sb.ToString());
                    if (xEle.Element("LastModifyTime") != null)
                    {
                        dic.AddOrReplace(IndexDatabaseProperties.LastModifyTime, xEle.Element("LastModifyTime").Value);
                    }
                    if (xEle.Element("LastAccessTime") != null)
                    {
                        dic.AddOrReplace(IndexDatabaseProperties.LastAccessTime, xEle.Element("LastAccessTime").Value);
                    }
                }

                catch (Exception ex)
                {
                    logger.Error(MediaCoreIndexResource.IndexDatabasePropertiesManagerParseDBPropertiesError, lineForLog, ex.ToString());
                }
            }
            return dic;
        }

        /// <summary>
        /// 将keyVals值存储到db properties文件中，修改方式采用添加或者更新
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="fileName"></param>
        /// <param name="cacheSystem"></param>
        /// <param name="keyVals"></param>
        public static void SaveDBProperties(string volume, string fileName, IXSystem cacheSystem, Dictionary<IndexDatabaseProperties, string> keyVals)
        {
            var dic = new Dictionary<IndexDatabaseProperties, string>();
            var info = XConvert.FromNames(volume, fileName);
            var fileExist = cacheSystem.FileExists(info);
            var xEle = new XElement("DBProperties");
            if (fileExist)
            {
                dic = ParseDBProperties(volume, fileName, cacheSystem);
            }
            foreach (KeyValuePair<IndexDatabaseProperties, string> pair in keyVals)
            {
                dic[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<IndexDatabaseProperties, string> pair in dic)
            {
                xEle.Add(new XElement(pair.Key.ToString(), pair.Value));
            }
            using (XStream cacheStream = cacheSystem.OpenStream(info, FileMode.Create))
            {
                try
                {
                    cacheStream.Write(Encoding.UTF8.GetBytes(xEle.ToString()), 0, Encoding.UTF8.GetByteCount(xEle.ToString()));
                }
                catch (Exception ex)
                {
                    logger.Error(MediaCoreIndexResource.IndexDatabasePropertiesManagerSaveDBPropertiesError, ex.ToString());
                }
                cacheStream.Flush();
            }
        }
    }
}
