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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace AvePoint.GCommon.Utility.Cryptography
{
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Signdata is unmodifiable as the cause of being referenced.")]
    public class ProductInfoUtil
    {

        private static AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string PRODUCT_SINGDATA_TAG_NAME = "configuration/properties/Signdata";
        private const string PRODUCT_CHECKSUM_TAG_NAME = "configuration/properties/Checksum";

        //DocAveDefaultProductInfomation
        private static byte[] DefaultSignature = { 41, 23, 67, 65, 164, 197, 244, 137, 15, 171, 198, 115, 90, 140, 18, 113, 130, 72, 212, 49, 255, 70, 247, 45, 116, 217, 65, 137, 12, 125, 184, 75, 44, 174, 159, 57, 61, 236, 219, 18, 178, 149, 42, 182, 142, 120, 169, 73, 210, 132, 211, 101, 118, 14, 164, 202, 28, 164, 205, 155, 211, 194, 79, 6, 91, 175, 32, 10, 133, 0, 232, 224, 12, 60, 13, 40, 218, 57, 32, 131, 197, 195, 81, 123, 16, 159, 15, 57, 160, 215, 119, 139, 136, 119, 8, 160, 0, 205, 84, 183, 67, 17, 128, 89, 238, 196, 20, 188, 160, 92, 122, 101, 226, 24, 176, 242, 142, 73, 169, 139, 217, 154, 192, 251, 30, 205, 169, 35, 176, 116, 111, 225, 9, 247, 129, 57, 76, 2, 65, 13, 22, 68, 112, 244, 26, 150, 94, 121, 212, 152, 204, 51, 9, 125, 218, 23, 182, 208, 221, 30, 112, 51, 197, 181, 232, 206, 82, 57, 237, 162, 6, 86, 188, 30, 59, 167, 95, 100, 233, 138, 63, 159, 58, 162, 253, 165, 201, 126, 185, 78, 8, 142, 31, 191, 19, 160, 92, 35, 153, 174, 124, 125, 212, 190, 28, 160, 102, 196, 83, 134, 159, 235, 46, 232, 195, 71, 10, 34, 211, 218, 87, 15, 25, 68, 61, 209, 148, 27, 18, 170, 197, 210, 113, 29, 123, 44, 109, 203, 99, 104, 86, 207, 140, 173, 87, 63, 218, 199, 183, 241, 109, 155, 168, 207, 225, 67 };
        private static byte[] DefaultHash = { 253, 23, 172, 3, 139, 96, 204, 188, 216, 50, 222, 112, 192, 15, 252, 59, 91, 74, 96, 59 };

        private static bool _isReleasePackage = false;

        public static bool IsReleasePackage
        {
            get { return _isReleasePackage; }
            set { _isReleasePackage = value; }
        }

        public static void InitProductInfo()
        {
            mLog.Debug("Begin verifying the product information...");
            string certPath = string.Empty;
            IsReleasePackage = VerifyProductInfo(certPath, true);
            mLog.Debug(string.Format("Product Information: {0}.", IsReleasePackage));
        }

        public static void WriteProductInfoForUpdate(string path)
        {
            string productFile = System.IO.Path.Combine(path, "ServiceProductInformation.config");
            if (!File.Exists(productFile))
            {
                using (Stream writer = File.Open(productFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                        XmlComment comment = doc.CreateComment(@"/********************************************************************
*
*  PROPRIETARY and CONFIDENTIAL
*
*  This file is licensed from, and is a trade secret of:
*
*                   AvePoint, Inc.
*                   3 Second Street, Suite 803
*                   Jersey City, NJ 07311
*                   United States of America
*                   Telephone: +1-800-661-6588
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
*  Copyright © 2001-2011 AvePoint® Inc. All Rights Reserved. 
*
*  Unpublished - All rights reserved under the copyright laws of the United States.
*/");
                        doc.AppendChild(declaration);
                        doc.AppendChild(comment);
                        XmlElement configuration = doc.CreateElement("configuration");
                        XmlElement properties = doc.CreateElement("properties");
                        XmlElement signdata = doc.CreateElement("Signdata");
                        XmlElement checksum = doc.CreateElement("Checksum");
                        XmlText signText = doc.CreateTextNode(Convert.ToBase64String(DefaultSignature));
                        XmlText checksumText = doc.CreateTextNode(Convert.ToBase64String(DefaultHash));
                        signdata.AppendChild(signText);
                        checksum.AppendChild(checksumText);
                        properties.AppendChild(signdata);
                        properties.AppendChild(checksum);
                        configuration.AppendChild(properties);
                        doc.AppendChild(configuration);
                        doc.Save(writer);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            //else
            //{
            //    WriteProductInfo(productFile, Convert.ToBase64String(DefaultSignature), Convert.ToBase64String(DefaultHash));
            //}
        }

        public static void WriteProductInfo(string path, string signdata, string checksum)
        {
            if (File.Exists(path))
            {
                try
                {
                    if (string.IsNullOrEmpty(signdata))
                    {
                        signdata = string.Empty;
                    }
                    if (string.IsNullOrEmpty(checksum))
                    {
                        checksum = string.Empty;
                    }

                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(path);
                    doc.SelectSingleNode(PRODUCT_SINGDATA_TAG_NAME).InnerText = signdata;
                    doc.SelectSingleNode(PRODUCT_CHECKSUM_TAG_NAME).InnerText = checksum;
                    doc.Save(path);

                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                throw new Exception(string.Format("Cannot find the product configuration file in the specified path: {0}.", path));
            }

        }

        private static bool VerifyProductInfo(string cerPath, bool isControl)
        {
            try
            {
                ProductSignDataDto signDataDto = GetProductInfomation(isControl);

                if (signDataDto == null || string.IsNullOrEmpty(signDataDto.SignData) || string.IsNullOrEmpty(signDataDto.Chekcsum))
                {
                    return false;
                }

                byte[] signdata = Convert.FromBase64String(signDataDto.SignData);
                byte[] checksum = Convert.FromBase64String(signDataDto.Chekcsum);

                X509Certificate2 x509 = CertificateUtil.LoadCertificate(cerPath, string.Empty);

                if (x509 != null)
                {
                    AsymmetricAlgorithm publicKey = x509.PublicKey.Key;
                    //RSACryptoServiceProvider rsaProviderPublic = (RSACryptoServiceProvider)publicKey;
                    IAsymmetricEncryption rsaProviderPublic = AsymmetricEncryptionFactory.GetAsymmetricEncryption(AsymmetricEncryptionAlgorithm.RSA, x509);
                    bool isRelease = rsaProviderPublic.VerifyHash(checksum, CryptoConfig.MapNameToOID("SHA1"), signdata);
                    return isRelease;
                }
                else
                {
                    mLog.Warn("Cannot find the certificate that is used to verify the data.");
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while verifying the product information.");
                mLog.Error(e.Message, e);
            }
            return false;
        }

        private static ProductSignDataDto GetProductInfomation(bool isControl)
        {
            string controlProductFilePath = GetControlProductFilePath();
            string productDir = isControl ? controlProductFilePath : AppDomain.CurrentDomain.BaseDirectory;
            string productFile = System.IO.Path.Combine(productDir, "ServiceProductInformation.config");

            if (!File.Exists(productFile))
            {
                mLog.Error(string.Format("Cannot find the product configuration file in the specified path: {0}.", productFile));
                return new ProductSignDataDto()
                {
                    SignData = string.Empty,
                    Chekcsum = string.Empty
                };
            }

            System.IO.FileStream productFileStream = System.IO.File.OpenRead(productFile);
            System.Xml.XmlDocument reader = new System.Xml.XmlDocument();
            try
            {

                reader.Load(productFileStream);
                //ProductSignDataDto signInfo = new ProductSignDataDto();
                ProductSignDataDto signInfo = new ProductSignDataDto()
                {
                    SignData = reader.SelectSingleNode(PRODUCT_SINGDATA_TAG_NAME).InnerText,
                    Chekcsum = reader.SelectSingleNode(PRODUCT_CHECKSUM_TAG_NAME).InnerText
                };

                return signInfo;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while searching the product configuration file.");
                mLog.Error(e.Message, e);
                return new ProductSignDataDto()
                {
                    SignData = string.Empty,
                    Chekcsum = string.Empty
                };
            }
            finally
            {
                productFileStream.Close();
            }
        }

        /// <summary>
        /// 由于Control和TimerService走的是一套逻辑，所以如果启动的是TimerService的话，需要找本层目录
        /// </summary>
        /// <returns>control 或 timer service的配置文件的目录</returns>
        private static string GetControlProductFilePath()
        {
            string controlProductFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(controlProductFilePath))
            {
                controlProductFilePath = Directory.GetParent(controlProductFilePath).FullName;
            }
            return controlProductFilePath;
        }




        private class ProductSignDataDto
        {
            public string SignData;
            public string Chekcsum;
        }

        private class CertificateUtil
        {
            private static byte[] ReadFile(string filePath)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AvePoint.GCommon.Utility.certificate.cer"))
                    {
                        int size = (int)stream.Length;
                        byte[] data = new byte[size];
                        size = stream.Read(data, 0, size);
                        return data;
                    }
                }
                return null;
            }

            public static X509Certificate2 LoadCertificate(string path, string password)
            {
                byte[] data = ReadFile(path);
                if (data != null && data.Length > 0)
                {
                    X509Certificate2 x509 = new X509Certificate2();
                    if (string.IsNullOrEmpty(password))
                    {
                        x509.Import(data);
                    }
                    else
                    {
                        x509.Import(data, ConverPassword(password), X509KeyStorageFlags.DefaultKeySet);
                    }
                    return x509;
                }
                return null;
            }

            private static SecureString ConverPassword(string password)
            {
                char[] pwd = password.ToCharArray();
                SecureString retValue = new SecureString();
                foreach (char c in pwd)
                {
                    retValue.AppendChar(c);
                }
                return retValue;
            }
        }
    }
}
