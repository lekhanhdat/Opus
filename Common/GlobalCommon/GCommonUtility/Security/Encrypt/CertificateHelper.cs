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
    using System.Security.Cryptography.X509Certificates;
    using AvePoint.GCommon.Utility.Cloud;

    #endregion

    public class CertificateHelper
    {
        static X509Certificate2 docaveOnlineX509Certificate;

        public static X509Certificate2 DocAveOnlineCertificate
        {
            get
            {
                return GCommonRoleConfiguration.Encrypt_Certificate;
            }
        }
        /// <summary>
        /// 通过此证书获取keyvault信息
        /// </summary>
        public static X509Certificate2 CommunicateWithKeyVaultCertificate
        {
            get
            {
                if (docaveOnlineX509Certificate == null)
                {
                    var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    docaveOnlineX509Certificate = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        GCommonRoleConfiguration.KeyVaultCertThumbprint,
                        false)[0];
                    store.Close();
                }
                return docaveOnlineX509Certificate;
            }
        }
    }
}
