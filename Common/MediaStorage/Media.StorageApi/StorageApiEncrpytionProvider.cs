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


using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using Storage.Util;
using System;
using System.IO;
using System.Text;

namespace AvePoint.Media.StorageApi
{
    public class StorageApiEncrpytionProvider : IStorageEncrpytionProvider
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(StorageApiEncrpytionProvider));
        public string DescriptCommunicationPassword(string ePass)
        {
            if (string.IsNullOrEmpty(ePass))
            {
                return ePass;
            }
            try
            {
                byte[] psBinary = CspCommunicationWrapper.UnWrapKey(ePass);
                return CryptoUtil.ConvertBytesToString(psBinary);
            }
            catch (Exception tx)
            {
                logger.Error("Decrypt password failed by using the communication key, " + tx.Message, tx);
                throw;
            }
           
        }

        public string DecryptPassword(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            try
            {
                byte[] psBinary = CspCrossPlatformExchangeWrapper.UnWrapKey(value);
                return CryptoUtil.ConvertBytesToString(psBinary);
            }
            catch (Exception e)
            {
                logger.Error("Decrypt password failed by using the hardcode key - " + e.Message, e);
                return value;
            }
        }

        public string EncryptPassword(string cleartextPassword)
        {
            if (string.IsNullOrEmpty(cleartextPassword))
            {
                return cleartextPassword;
            }
            byte[] psBinary = CryptoUtil.ConvertStringToBytes(cleartextPassword);
            return CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(psBinary);
        }

        public string GetMD5Hash(string input)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            // 遍历字节数组，将每一个字节转换为十六进制字符串后，追加到StringBuilder实例的结尾
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // 返回一个十六进制字符串
            return sBuilder.ToString();
        }


        public string Base64Encoded128BitMD5Digest(byte[] buffer)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(buffer);
            return Convert.ToBase64String(data);
        }

        public string Base64Encoded128BitMD5Digest(Stream buffer)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(buffer);
            return Convert.ToBase64String(data);
        }

        public string Base64Encoded128BitMD5Digest(byte[] buffer, int offset, int length)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(buffer, offset, length);
            return Convert.ToBase64String(data);
        }
    }

    public class SecretUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SecretUtil));
        internal static StorageApiEncrpytionProvider Provider = new StorageApiEncrpytionProvider();

        public static string DescriptCommunicationPassword(string ePass)
        {
            if (string.IsNullOrEmpty(ePass))
            {
                return ePass;
            }
            try
            {
                return Provider.DescriptCommunicationPassword(ePass);
            }
            catch (Exception tx)
            {
                logger.Error("Decrypt password failed by using the communication key, " + tx.Message, tx);
                throw;
            }
        }

        public static string Decrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            try
            {
                return Provider.DecryptPassword(value);
            }
            catch (Exception e)
            {
                logger.Error("Decrypt password failed by using the hardcode key - " + e.Message, e);
                throw;
            }
        }

        public static string EncryptPassword(string cleartextPassword)
        {
            return Provider.EncryptPassword(cleartextPassword);
        }
    }
}
