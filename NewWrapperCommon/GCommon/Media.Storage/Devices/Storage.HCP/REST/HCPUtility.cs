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

namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Cryptography;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.Media.Storage.Cloud.Common;
    using System.Globalization;
    #endregion

    class HCPUtility : CommonUtility
    {
        public static string GenerateCookie(HCPOpenParameter openParams)
        {
            if (!string.IsNullOrEmpty(openParams.UserName) && !string.IsNullOrEmpty(openParams.Password))
            {
                return HCPConsts.KEY_COOKIE_VAL_PREFIX + "=" + Base64(openParams.UserName) + ":" + GetMD5Hash(SecretUtil.DescryptPassword(openParams.Password));
            }
            return string.Empty;
        }

        //public static string GetMD5Hash(string input)
        //{
        //    IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
        //    byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        //    StringBuilder sBuilder = new StringBuilder();

        //    // 遍历字节数组，将每一个字节转换为十六进制字符串后，追加到StringBuilder实例的结尾
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));
        //    }

        //    // 返回一个十六进制字符串
        //    return sBuilder.ToString();

        //}

        //private static string Base64(string strVal)
        //{
        //    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strVal));
        //}

        public static string GetMetadataXML(Dictionary<string, string> metaInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" ?>");
            sb.Append("<DOCAVE".ToLower(CultureInfo.InvariantCulture) + "_metaInfo" + "S>".ToLower(CultureInfo.InvariantCulture));
            foreach (KeyValuePair<string, string> kvp in metaInfos)
            {
                sb.Append("<" + kvp.Key + ">" + kvp.Value + "</" + kvp.Key + ">");
            }
            sb.Append("</DOCAVE".ToLower(CultureInfo.InvariantCulture) + "_metaInfo" + "S>".ToLower(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        public static Dictionary<string, string> CombinDictionary(Dictionary<string, string> destinationDic, Dictionary<string, string> sourceDic)
        {
            foreach (KeyValuePair<string, string> entry in sourceDic)
            {
                if (!destinationDic.ContainsKey(entry.Key))
                {
                    destinationDic[entry.Key] = entry.Value;
                }
            }
            return destinationDic;
        }

    }
}
