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


namespace Microsoft365.Authentication
{
    using Microsoft365.Common.Logger;
    using System;
    using System.Security.Cryptography.X509Certificates;
    public static class CertificateValidator
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(CertificateValidator));
        public static bool IsCertificateValidate(X509Certificate2 certificate2)
        {
            try
            {
                logger.Info($"Start to validate certificate {certificate2?.Thumbprint}");
                if (certificate2 == null)
                {
                    logger.Info("Certificate2 is null");
                    return false;
                }

                if (!certificate2.HasPrivateKey)
                {
                    logger.Info($"Certificate2 {certificate2.Thumbprint} HasPrivateKey is false.");
                    return false;
                }

                if (certificate2.GetRSAPrivateKey() == null)
                {
                    logger.Info($"Certificate2 {certificate2.Thumbprint} PrivateKey is null.");
                    return false;
                }
                else
                {
                    logger.Info($"Certificate2 {certificate2.Thumbprint} has private key.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Info($"Certificate2 GetPrivateKey failed.Error:{ex}.");
                return false;
            }
        }
    }
}