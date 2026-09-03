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
using System.Text;
using System.Security;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public static class ConfigurationProtectionUtil
    {


        public static byte[] UnWrapAndProtect(byte[] userData)
        {

            return AveProtectedData.Protect(CspCommunicationWrapper.UnWrapKey(userData));

        }

        public static byte[] UnWrapAndProtect(string userData)
        {

            return AveProtectedData.Protect(CspCommunicationWrapper.UnWrapKey(userData));

        }

        public static string UnWrapAndProtectWithBase64(byte[] userData)
        {

            return AveProtectedData.ProtectWithBase64(CspCommunicationWrapper.UnWrapKey(userData));

        }

        public static string UnWrapAndProtectWithBase64(string userData)
        {

            return AveProtectedData.ProtectWithBase64(CspCommunicationWrapper.UnWrapKey(userData));

        }








        public static byte[] UnProtectAndWrap(byte[] userData)
        {

            return CspCommunicationWrapper.WrapKey(AveProtectedData.UnProtect(userData));

        }

        public static byte[] UnProtectWithBase64AndWrap(string userData)
        {

            return CspCommunicationWrapper.WrapKey(AveProtectedData.UnProtectWithBase64(userData));

        }

        public static string UnProtectAndWrapToBase64(byte[] userData)
        {

            return CspCommunicationWrapper.WrapKeyToBase64String(AveProtectedData.UnProtect(userData));

        }


        public static string UnProtectWithBase64AndWrapToBase64(string userData)
        {

            return CspCommunicationWrapper.WrapKeyToBase64String(AveProtectedData.UnProtectWithBase64(userData));

        }






        public static byte[] Protect(byte[] userData)
        {
            return AveProtectedData.Protect(userData);

        }


        public static byte[] UnProtect(byte[] userData)
        {

            return AveProtectedData.UnProtect(userData);

        }







        public static string ProtectWithBase64(byte[] userData)
        {
            return AveProtectedData.ProtectWithBase64(userData);

        }



        public static byte[] UnProtectWithBase64(string userData)
        {

            return AveProtectedData.UnProtectWithBase64(userData);

        }











    }
}
