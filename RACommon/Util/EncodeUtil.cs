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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class EncodeUtil
    {
        //public static string Encode(string data, byte[] encode = null)
        //{
        //    if (null == encode)
        //    {
        //        byte[] key = System.Text.ASCIIEncoding.ASCII.GetBytes(EntropyKey);
        //        return DesUtil.Encode(data, key, key);
        //    }
        //    else
        //    {
        //        return DesUtil.Encode(data, encode, encode);
        //    }
        //}

        ///// <summary>
        ///// Decode
        ///// </summary>
        //public static string Decode(string data, byte[] decode = null)
        //{
        //    if (null == decode)
        //    {
        //        byte[] key = System.Text.ASCIIEncoding.ASCII.GetBytes(EntropyKey);
        //        return DesUtil.Decode(data, key, key);
        //    }
        //    else
        //    {
        //        return DesUtil.Decode(data, decode, decode);
        //    }
        //}

        #region default key
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EncodeUtil));

        private static readonly byte[] SecurityKey = new byte[] { 114, 77, 127, 118, 101, 112, 101, 78, 116, 68, 108, 105, 110, 124, 103, 115, 127, 124 };

        public static string EntropyKey = GetEncryptKey();

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
        public static string GetEncryptKey()
        {
            byte[] tempBytes = GetKeyBytes(SecurityKey);
            return Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
        }


        public static string EncryptBySHA1(string key)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
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
        public static byte[] EncryptBySHA1ToByte(string key)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
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
            return Convert.ToBase64String(GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(key)));
        }
        public static string DecryptByCommunicationKey(string key)
        {

            try
            {
                key = CryptoUtil.ConvertBytesToString(GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(key));
            }
            catch(Exception e)
            {
                logger.Error("DecryptByCommunicationKey error", e);
            }
            return key;
        }
        #endregion

    }
}
