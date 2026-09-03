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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.RA.Common.Util
{
    public class XmlUtil
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(XmlUtil));
        public static string GetXmlString<T>(T obj)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xs.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static T GetXmlObject<T>(string xml)
        {
            T t;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                object obj = serializer.Deserialize(reader);
                if (obj != null)
                {
                    t = (T)obj;
                }
                else
                {
                    t = default(T);
                }
            }
            return t;
        }

        #region 转义特殊字符 
        /// <summary>
        /// 转义特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TransferSpecialCharactor(string str)
        {
            return string.IsNullOrEmpty(str) ? str :
                str.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;");
        }
        #endregion

        public static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var regex = new Regex(@"[\x00-\x08\x0B-\x0C\x0E-\x1F]");
            if (regex.IsMatch(text))
            { 
                logger.Warn($"The content is [{text}]; it contained illegal XML characters, and these illegal characters will be removed.");
                return regex.Replace(text, "");
            }
            return text;
        }
    }
}
