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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using GCommon.Utility.Cryptography;
    using GCommon.MicroKernel.MicroKernelIntentionImpl;
    using GCommon.Utility.ServiceVersion;
    using GCommon.MicroKernel;
    using AvePoint.GCommon.Utility;

    #endregion

    public class AveStaticEnv
    {
        public static void Setup()
        {
            var encryptionKeys = AgentCacheManager.GetCachedRegisterResult();
            if (encryptionKeys != null)
            {
                //CryptographyManagement.CryptoMode = (CryptoMode)encryptionKeys.Item2;
                CryptographyManagement.CryptoMode = FipsModeUtil.GetCryptoModeFromRegistry();

                CspCommunicationWrapper.CommunicationEncryptionKey = encryptionKeys.Item1;
                DefaultAuthInterseption.AuthorizationToken = CspCommunicationWrapper.AuthToken;
            }
            var clientPlatformVersion = ServiceVersionHelper.GetVersion(false);
            AppDomain.CurrentDomain.SetData(MicroKernelConstant.ClientPlatformVersion, clientPlatformVersion.ProductVersion);
            AppDomain.CurrentDomain.SetData(MicroKernelConstant.ClientPlatformDisplayVersion, clientPlatformVersion.DisplayVersion);
            HttpConfiguration.AllowUnsafeHeaderParsing();
            UpgradeSecurityProtocols();
        }

        private static void UpgradeSecurityProtocols()
        {
            if (Enum.IsDefined(typeof(System.Net.SecurityProtocolType), 12288))
            {
                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)(12288);//tls1.3
            }

            if (Enum.IsDefined(typeof(System.Net.SecurityProtocolType), 3072))
            {
                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)(3072);

                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)(192);
                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)(48);
                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)(768);
            }
        }
    }
}

