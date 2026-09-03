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
//namespace AvePoint.Hybrid.ClientCore
//{
//    using System;
//    using System.Security.Cryptography;
//    using System.Security.Cryptography.X509Certificates;
//    using System.Text;
//    using System.Xml;

//    public static class RSAHelper
//    {
//        public static RSA CreateRsaProviderFromXml(string xml)
//        {
//            XmlDocument xmlDoc = new XmlDocument();
//            xmlDoc.LoadXml(xml);

//            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
//            {
//                RSAParameters parameters = new RSAParameters();
//                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
//                {
//                    switch (node.Name)
//                    {
//                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
//                    }
//                }
//                var rsa = RSA.Create();
//                rsa.ImportParameters(parameters);
//                return rsa;
//            }
//            else
//            {
//                throw new Exception("Invalid XML RSA key.");
//            }
//        }
//        public static string Sha1SingData(this X509Certificate2 cert, string plainText)
//        {
//            if (!cert.HasPrivateKey)
//                throw new InvalidOperationException(
//                    "Certificate with public key can't be used for signature purpose ");
//            var data = Encoding.UTF8.GetBytes(plainText);
//            var sha1 = new SHA1CryptoServiceProvider();
//            var hashbytes = sha1.ComputeHash(data);
//            var signatrueFormatter = new RSAPKCS1SignatureFormatter(cert.PrivateKey);
//            signatrueFormatter.SetHashAlgorithm("SHA1");
//            var signature = signatrueFormatter.CreateSignature(hashbytes);
//            return Convert.ToBase64String(signature);
//        }

//        public static bool VerifyData(this X509Certificate2 cert, string plainText, string signature)
//        {
//            var data = Encoding.UTF8.GetBytes(plainText);
//            var signatureData = Convert.FromBase64String(signature);
//            var rsa = (RSA)cert.PublicKey.Key;
//            return rsa.VerifyData(data, signatureData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
//        }
//    }
//}
