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
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Contract.AveLicense.Detail;
using AvePoint.GCommon.Utility.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Utility
{
    public class AveCustomerInfoHelper
    {
        //static AveLogger logger = AveLogger.GetInstance(typeof(AveCustomerInfoHelper));

        private static CustomerInfo _customerInfo { get; set; }

        public static CustomerInfo GetCustomerInfoForLog()
        {
            var result = new CustomerInfo();
            //try
            //{
            if (!string.IsNullOrEmpty(LicenseWrapper.AccountNumber))
            {
                result.AccountNumber = EncryptSensitiveInfo(LicenseWrapper.AccountNumber);
            }
            //}
            //catch (Exception e)
            //{
            //    logger.Error("GetCustomerInfoForLog error: {0}", e);
            //}
            return result;
        }

        private static String EncryptSensitiveInfo(String sensitiveInfo)
        {
            String result = sensitiveInfo;
            if (!string.IsNullOrEmpty(sensitiveInfo))
            {
                var psBinary = CryptoUtil.ConvertStringToBytes(sensitiveInfo);
                result = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(psBinary);
            }
            return ValueEncode(result);
        }

        private static string DecryptSensitiveInfo(string sensitiveInfo)
        {
            string result = sensitiveInfo;
            if (!string.IsNullOrEmpty(sensitiveInfo))
            {
                result = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(ValueDecode(sensitiveInfo)));
            }
            return result;
        }

        private static string ValueEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
            //return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D");
        }

        private static string ValueDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%").Replace("%5e", "^");
            //return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%");
        }
    }

    public class CustomerInfo
    {
        public string AccountNumber { get; set; } 
        
    }
}
