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
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using AvePoint.Common;
using RAExportCommon.Properties;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.RACommonUtility.Encryption;
using RAExportCommon.VEOExportV2;

namespace RAExportCommon
{
    internal class AveCertificateOperation
    {

        private static X509Certificate2 pfx = new X509Certificate2(GetCertificate(), GetCertificatePass(), X509KeyStorageFlags.Exportable);
        private static RMAesEncryptorWrapper AesEncryptorWrapper => new();

        private static ExportSignatureInfo GetSignatureInfo()
        {
            SHA512WithRSASignature sHA = new SHA512WithRSASignature();
            return sHA.SignatureInfo();
        }
        private static string GetCertificatePass()
        {
            var info = GetSignatureInfo();
            string decryptedPassword = AesEncryptorWrapper.Decrypt(info.Password);
            return decryptedPassword;

            //return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("YSF2QGUjcCRvJWlebiZ0"));
        }
        private static byte[] GetCertificate()
        {
            var info = GetSignatureInfo();
            return AesEncryptorWrapper.Decrypt(info.Certificate);
        }

        /// <summary> 
        /// 取得证书私钥 
        /// </summary> 
        /// <param name="pfxPath">证书的绝对路径</param> 
        /// <returns></returns> 
        internal static String GetPrivateKey()
        {
            string privateKey = pfx.PrivateKey.ToXmlString(true);
            return privateKey;
        }

        /// <summary> 
        /// 取得证书的公钥 
        /// </summary> 
        /// <param name="cerPath">证书的绝对路径</param> 
        /// <returns></returns> 
        internal static String GetPublicKey()
        {
            string publicKey = pfx.PublicKey.Key.ToXmlString(false);
            return publicKey;
        }

        internal static X509Certificate2 GetX509Certificate2()
        {
            return pfx;
        }

        internal static byte[] ExportCertificateWithCertFormat()
        {
            return pfx.Export(X509ContentType.Cert);
        }
    }
}
