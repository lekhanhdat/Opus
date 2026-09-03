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


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AvePoint.GCommon.Utility.Cryptography;

//namespace AutoInstallationCommon.Utility.Handler
//{
//   public class EncryptHelper
//    {
//        public string EncryptPasswordHandle(string password)
//        {
//            string encryptedPassword = ConfigurationProtectionUtil.ProtectWithBase64(Encoding.UTF8.GetBytes(password));
//            return encryptedPassword;
//        }

//        public string DecryptPasswordHandle(string encryptedPassword)
//        {
//            byte[] decryptedPasswordArray = ConfigurationProtectionUtil.UnProtectWithBase64(encryptedPassword);
//            string decryptedPassword = Encoding.UTF8.GetString(decryptedPasswordArray);
//            return decryptedPassword;
//        }

//        public string DecryptBuiltInDBPassword(string encryptedPassword)
//        {
//            byte[] decryptedPasswordArray = CspCrossPlatformExchangeWrapper.UnWrapKey(encryptedPassword);
//            string decryptedPassword = Encoding.UTF8.GetString(decryptedPasswordArray);
//            return decryptedPassword;
//        }

//        public string EncryptBuiltInDBPassword(string password)
//        {
//            string encryptedPassword =
//                CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(password));
//            return encryptedPassword;
//        }
//    }
//}

