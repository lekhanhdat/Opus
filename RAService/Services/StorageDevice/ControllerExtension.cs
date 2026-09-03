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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security;
using System;
using System.Security.Cryptography;
using System.Text;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.RA.Web.Controllers.PhysicalDevice
{
    public class ControllerExtension
    {
    }
    public class Crypto
    {
        public static string WrapKeyToBase64String(string password)
        {
            var p = Encoding.UTF8.GetBytes(password);
            return CspCommunicationWrapper.WrapKeyToBase64String(p);
        }

        public static SecureString UnWrapKeyToSecureString(string password)
        {
            return CspCommunicationWrapper.UnWrapKeyToSecureString(password);
        }

        public static string WrapKey(string password)
        {
            var p = Encoding.UTF8.GetBytes(password);
            return CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(p);
        }

        public static string UnWrapKey(string password)
        {
            var result = CspCrossPlatformExchangeWrapper.UnWrapKey(password);
            return Encoding.UTF8.GetString(result, 0, result.Length);
        }

        /// <summary>
        /// 使用DataContractJsonSerializer 来序列化对象为Json
        /// </summary>
        /// <param name="type"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Convert2Json(Type type, object o)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(type).WriteObject(ms, o);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
