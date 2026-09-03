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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace  AvePoint.Hybrid.Utility.Cryptography
{
    public class RAEncodeUtil
    {
        /// <summary>
        /// 加密成Base64String
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string data, string key = null)
        {
            if (null == key)
            {
                return DesEncrypt(data, EntropyKey, EntropyKey);
            }
            else
            {
                return DesEncrypt(data, key, key);
            }
        }

        public static byte[] Decrypt(string base64Str, string key = null)
        {
            if (null == key)
            {
                return DesDecrypt(base64Str, EntropyKey, EntropyKey);
            }
            else
            {
                return DesDecrypt(base64Str, key, key);
            }
        }

        
        //DES字符串解密
        public static byte[] DesDecrypt(string _strQ, string key, string iv)
        {
            _strQ = _strQ.Replace("%", "+");
            byte[] buffer = Convert.FromBase64String(_strQ);
            MemoryStream ms = new MemoryStream();
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv)), CryptoStreamMode.Write);
            encStream.Write(buffer, 0, buffer.Length);
            encStream.FlushFinalBlock();
            return ms.ToArray();
        }
        // DES字符串加密
        public static string DesEncrypt(string _strQ, string key, string iv)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(_strQ);
            MemoryStream ms = new MemoryStream();
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(ms, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv)), CryptoStreamMode.Write);
            encStream.Write(buffer, 0, buffer.Length);
            encStream.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray()).Replace("+", "%");
        }

        #region default key

        private static readonly byte[] SecurityKey = new byte[] { 114, 77, 127, 118, 101, 112, 101, 78, 116, 68, 108, 105, 110, 124, 103, 115, 127, 124 };

        private static string EntropyKey = GetEncryptKey();

        private static byte[] GetKeyBytes(byte[] securityKey)
        {
            var result = from a in securityKey where (int)a < 127 select a;
            byte[] rBytes = result.ToArray();
            List<byte> list = new List<byte>();
            //ArrayList list = new ArrayList();
            int index = 0;
            foreach (byte b in rBytes)
            {
                if (index > 14)
                {
                    break;
                }
                string lastNum = ((int)b).ToString().Substring(((int)b).ToString().Length - 1);
                switch (Convert.ToInt32(lastNum))
                {
                    case 0:
                    case 1:
                    case 3:
                    case 5:
                    case 6:
                    case 7:
                        list.Add(b);
                        break;
                    default:
                        break;

                }
                index++;
            }
            return list.ToArray();
        }

        private static string GetEncryptKey()
        {
            byte[] tempBytes = GetKeyBytes(SecurityKey);
            return Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
        }
        public static string EncryptBySHA1(string key)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.Hybrid.Utility.Cryptography.HashAlgorithm.SHA1);
            try
            {
                byte[] data = System.Text.Encoding.Default.GetBytes(key.ToLowerInvariant());

                byte[] hashData = hash.ComputeHash(data);
                hash.Clear();
                StringBuilder sbr = new StringBuilder();
                for (int i = 0; i < hashData.Length - 1; i++)
                {
                    sbr.Append(hashData[i].ToString("x").PadLeft(2, '0'));
                }
                return sbr.ToString();
            }
            finally
            {
                if (hash != null)
                {
                    hash.Clear();
                }
            }

        }
        private static byte[] EncryptBySHA1ToByte(string key)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.Hybrid.Utility.Cryptography.HashAlgorithm.SHA1);
            try
            {
                byte[] data = System.Text.Encoding.Default.GetBytes(key.ToLowerInvariant());

                byte[] hashData = hash.ComputeHash(data);
                hash.Clear();
                return hashData;
            }
            finally
            {
                if (hash != null)
                {
                    hash.Clear();
                }
            }

        }
        #endregion

        #region encrypt by DA
        public static string EncryptByCommunicationKey(string key)
        {
            return Convert.ToBase64String(CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(key)));
        }
        public static string DecryptByCommunicationKey(string key)
        {

            try
            {
                key = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(key));
            }
            catch
            {
                //Noncompliant
            }
            return key;
        }
        #endregion
    }
}
