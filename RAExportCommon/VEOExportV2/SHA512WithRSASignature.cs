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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Encryption;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using tempRSA = System.Security.Cryptography.RSA;
using AvePoint.RA.Service.Services;
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.RA.Contract.RMWeb;
using Cloud.Sdk.Data.AosModern;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using RAExportCommon.Properties;
using Microsoft.SharePoint.Client.Search.ContentPush;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Contract.Certficate;
using System.Runtime.ConstrainedExecution;
using Util.Security;
using AvePoint.GCommon.Utility.Cryptography.Hash;
using AvePoint.GCommon.Utility.Cryptography.AsymmetricEncryption;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Tenant;
using System.Security;

namespace RAExportCommon.VEOExportV2
{
    public class SHA512WithRSASignature : IDisposable
    {
        private static AveSha512 sha512 = new AveSha512();
        private static RSAEncryptionV1 RSA = new RSAEncryptionV1(AveCertificateOperation.GetX509Certificate2());

        internal static byte[] Signature(byte[] data)
        {
            byte[] SignData;
            SignData = AveSignatureBySHA1withRSA(data);
            return SignData;
        }

        //internal static byte[] Signature(Stream data)
        //{
        //    byte[] SignData;
        //    SignData = AveSignatureBySHA1withRSA(data);
        //    return SignData;
        //}

        /// <summary> 
        /// RSA Verify Signature
        /// </summary> 
        /// <param name="key">公钥证书</param> 
        /// <param name="data">待Verify加密的数据</param> 
        /// <param name="SignData">签名结果</param> 
        /// <returns></returns> 
        //internal static bool VerifySignature(X509Certificate2 cer, byte[] data, byte[] SignData)
        //{
        //    byte[] result;

        //    result = sha1.ComputeHash(data);
        //    RSAEncryptionV1 RSA = new RSAEncryptionV1(cer);
        //    return RSA.VerifyHash(result, SignData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        //}

        //internal static bool VerifySignature(byte[] data, byte[] SignData)
        //{
        //    byte[] sha1hash = sha1.ComputeHash(data);
        //    return RSA.VerifyHash(sha1hash, SignData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        //}

        private static byte[] AveSignatureBySHA1withRSA(byte[] data)//$$$$Confirm veo format?
        {
            byte[] sha1hash = sha512.ComputeHash(data);
            byte[] SignedHashValue;

            SignedHashValue = RSA.SignHash(sha1hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1); //fortify scan, veo format required Pkcs1. 
            return SignedHashValue;
        }

        //private static byte[] AveSignatureBySHA1withRSA(Stream data)
        //{
        //    byte[] sha1hash = sha1.ComputeHash(data);
        //    byte[] SignedHashValue;

        //    SignedHashValue = RSA.SignHash(sha1hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        //    return SignedHashValue;
        //}


        #region 废弃
        /// <summary> 
        /// RSA加密
        /// </summary> 
        /// <param name="key">私钥</param> 
        /// <param name="data">待加密的数据</param> 
        /// <returns></returns> 
        //private byte[] SignatureBySHA1(String key, byte[] data)
        //{
        //    byte[] result;
        //    SHA1 shaM = new SHA1Managed();
        //    result = shaM.ComputeHash(data);
        //    byte[] SignedHashValue;
        //    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
        //    RSA.FromXmlString(key);
        //    SignedHashValue = RSA.SignHash(result, CryptoConfig.MapNameToOID("SHA1"));
        //    //RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
        //    //RSAFormatter.SetHashAlgorithm("SHA1");
        //    //SignedHashValue = RSAFormatter.CreateSignature(result);
        //    return SignedHashValue;
        //}

        /// <summary> 
        /// RSA Verify Signature
        /// </summary> 
        /// <param name="key">公钥</param> 
        /// <param name="data">待Verify加密的数据</param> 
        /// <returns></returns> 
        //private bool VerifySignature(String key, byte[] data, byte[] SignData)
        //{
        //    byte[] result;
        //    SHA1 shaM = new SHA1Managed();
        //    result = shaM.ComputeHash(data);
        //    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
        //    RSA.FromXmlString(key);
        //    return RSA.VerifyHash(result, CryptoConfig.MapNameToOID("SHA1"), SignData);
        //}

        public static (string publicKey, string privateKey) GenerateKeys(byte[] certBytes, string password)
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(certBytes, password, X509KeyStorageFlags.Exportable);
                if (cert.HasPrivateKey)
                {
                    tempRSA rsa = cert.GetRSAPrivateKey();
                    if (rsa == null)
                        throw new CryptographicException("No private key found in certificate.");
                    string publicKey = rsa.ExportRSAPublicKeyPem();
                    string privateKey = rsa.ExportEncryptedPkcs8PrivateKeyPem(password, new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 10000));
                    return (publicKey, privateKey);
                }
                else
                {
                    throw new CryptographicException("Certificate does not contain a private key.");
                }
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }

        public static X509Certificate2 CreateSelfSignedCertificateForTenant(string logonGroupId, DateTime? notAfter = null)
        {
            string subjectName = $"CN={logonGroupId}";
            DateTime expiryDate = notAfter ?? DateTime.Now.AddYears(30);

            using (var rsa = tempRSA.Create(2048))
            {
                var certRequest = new CertificateRequest(
                    subjectName,
                    rsa,
                    HashAlgorithmName.SHA512,
                    RSASignaturePadding.Pss
                );
                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        keyUsages: X509KeyUsageFlags.DigitalSignature,
                        critical: false
                    )
                );
                certRequest.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(key: certRequest.PublicKey, critical: false)
                );
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                new Oid("1.3.6.1.5.5.7.3.1", "Server Authentication")
                    }, false
                ));

                var cert = certRequest.CreateSelfSigned(DateTimeOffset.Now, expiryDate);

                byte[] data = cert.Export(X509ContentType.Pfx);
                return new X509Certificate2(data, "", X509KeyStorageFlags.Exportable);
            }
        }

        public static string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            using (var rng = RandomNumberGenerator.Create())
            {
                var password = new StringBuilder();
                byte[] randomBytes = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomBytes);
                    var index = randomBytes[0] % validChars.Length;
                    password.Append(validChars[index]);
                }
                return password.ToString();
            }
        }

        public ExportSignatureInfo SignatureInfo()
        {
            SettingProfilesDao profilesDao = new SettingProfilesDao();
            RMAesEncryptorWrapper AesEncryptorWrapper = new RMAesEncryptorWrapper();
            var exportSignature = profilesDao.LoadByType((int)SettingProfilesType.ExportSignatureForVEOInfo);
            if (exportSignature == null)
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                var cert = CreateSelfSignedCertificateForTenant(TenantLocalValue.LogonGroupId);
                var pass = GenerateRandomPassword().ToSecureString();
                var certBytes = cert.Export(X509ContentType.Pfx, pass.ToPlainString());
                (string publicKey, string privateKey) = GenerateKeys(certBytes, pass.ToPlainString());
                info.Certificate = AesEncryptorWrapper.Encrypt(certBytes);
                info.Password = AesEncryptorWrapper.Encrypt(pass.ToPlainString());
                info.Thumbprint = cert.Thumbprint;
                info.PrivateKey = AesEncryptorWrapper.Encrypt(privateKey);
                info.PublicKey = AesEncryptorWrapper.Encrypt(publicKey);

                SettingProfileDto profileDto = new SettingProfileDto();
                profileDto.Id = Guid.NewGuid().ToString();
                profileDto.Name = SettingProfilesType.ExportSignatureForVEOInfo.ToString();
                profileDto.Type = (int)SettingProfilesType.ExportSignatureForVEOInfo;
                profileDto.Settings = JsonSerializer.Serialize(info);
                profilesDao.Create(profileDto);
                return info;
            }
            else
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                info = JsonSerializer.Deserialize<ExportSignatureInfo>(exportSignature.Settings);
                return info;
            }
        }

        #endregion

        public void Dispose()
        {
            if (sha512 != null)
            {
                sha512.Dispose();
            }
            if (RSA != null)
            {
                RSA.Dispose();
            }
        }
    }
}
