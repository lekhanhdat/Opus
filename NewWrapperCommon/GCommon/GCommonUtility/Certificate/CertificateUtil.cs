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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AvePoint.GCommon
{
    public class CertificateManagementUtil
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(CertificateManagementUtil));

        public static string GetCertificateChainStatus(string certThumbprint)
        {
            StringBuilder chainStatus = new StringBuilder();
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
                    if (col.Count > 0)
                    {
                        X509Certificate2 cert = col[0];
                        X509Chain certChain = new X509Chain();
                        certChain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        bool buildResult = certChain.Build(cert);
                        chainStatus.AppendLine("Build result:" + buildResult.ToString());
                        foreach (var status in certChain.ChainStatus)
                        {
                            chainStatus.AppendLine(string.Format("Status: {0} Description: {1}", status.Status, status.StatusInformation));
                        }
                    }
                    else
                    {
                        chainStatus.AppendLine("Cannot find certificate by thumbprint: " + certThumbprint);
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            catch (Exception ex)
            {
                chainStatus.AppendLine("An error occurred while checking certificate chain status: " + ex.ToString());
            }
            return chainStatus.ToString();
        }

        public static bool CertificateCanDoKeyExchange(string certThumbprint)
        {
            bool canDoKeyExchange = false;
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
                    if (col.Count > 0)
                    {
                        X509Certificate2 cert = col[0];
                        if (cert.HasPrivateKey)
                        {
                            RSACryptoServiceProvider privateKey = cert.PrivateKey as RSACryptoServiceProvider;
                            string keyFileLocation = FindKeyFileLocation(privateKey.CspKeyContainerInfo.UniqueKeyContainerName);
                            string keyFilePath = Path.Combine(keyFileLocation, privateKey.CspKeyContainerInfo.UniqueKeyContainerName);
                            logger.Info("Friendly Name:{0} Subject:{1} Thumbprint:{2}", cert.FriendlyName, cert.Subject, cert.Thumbprint);
                            logger.Info("Key File Path: {0}", keyFilePath);
                            logger.Info("Key Type: {0}", privateKey.CspKeyContainerInfo.KeyNumber);
                            canDoKeyExchange = privateKey.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange;
                        }
                        else
                        {
                            throw new Exception("The certificate doesn't have private key.");
                        }
                    }
                    else
                    {
                        throw new Exception("cannot find certificate by thumbprint: " + certThumbprint);
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while checking certificate key exchange property. {0}", ex.ToString());
            }
            return canDoKeyExchange;
        }

        static string FindKeyFileLocation(string keyFileName)
        {
            string commonAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string path1 = commonAppDataPath + @"\Microsoft\Crypto\RSA\MachineKeys";
            string[] files = Directory.GetFiles(path1, keyFileName);
            if (files.Length > 0)
            {
                return path1;
            }
            string appDatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path2 = appDatapath + @"\Microsoft\Crypto\RSA\";
            string[] directories = Directory.GetDirectories(path2);
            if (directories.Length > 0)
            {
                foreach (string dir in directories)
                {
                    files = Directory.GetFiles(dir, keyFileName);
                    if (files.Length != 0)
                    {
                        return dir;
                    }
                }
            }
            return "Private key exists but is not accessible";
        }

        public static bool ValidateCertificate(byte[] data, string password, ref int keySize)
        {
            bool result = false;
            try
            {
                X509Certificate2 cert = new X509Certificate2(data, password);
                var keys = cert.GetPublicKey();
                keySize = cert.PrivateKey.KeySize;
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error("validate certificate error, message: {0}", ex.ToString());
            }
            return result;
        }
    }

    public class BuiltInCertificates
    {
        public const string DocAveBuiltInCertificate = "EFB6AAA03D17268BAD4DE3D4E09FC05E24C1B3C8";
        public const string DocAveBuiltInCertificateEx = "E17BEDE931C319865ABA0673E153177F5557735B";
        public const string DocAveBuiltInCertificateSHA2 = "E0F8C3E969970E90383B70112127B4CAE6BF7E4E";

        public const string DocAveBuiltInCertificateWithSpace = "EF B6 AA A0 3D 17 26 8B AD 4D E3 D4 E0 9F C0 5E 24 C1 B3 C8";
        public const string DocAveBuiltInCertificateExWithSpace = "E1 7B ED E9 31 C3 19 86 5A BA 06 73 E1 53 17 7F 55 57 73 5B";
        public const string DocAveBuiltInCertificateSHA2WithSpace = "E0 F8 C3 E9 69 97 0E 90 38 3B 70 11 21 27 B4 CA E6 BF 7E 4E";

    }
}
