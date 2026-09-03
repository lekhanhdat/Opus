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
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.Hybrid.Utility.Cryptography.Hash
{
    public class HmacSHA256 : IHashAlgorithm, IDisposable
    {
         #region IHashAlgorithm Members

        private HMACSHA256 hashProvider;
        
        public HmacSHA256(SecureString key)
        {
            hashProvider = new HMACSHA256(CryptoUtil.ConvertSecureStringToBytes(key));
        }

        public HmacSHA256(byte[] key) 
        {
            hashProvider = new HMACSHA256(key);
        }

        public HmacSHA256()
        {
            hashProvider = new HMACSHA256();

        }

        public byte[] ComputeHash(byte[] value)
        {
            return hashProvider.ComputeHash(value);
        }

        public byte[] ComputeHash(byte[] value, int offset, int len)
        {
            return hashProvider.ComputeHash(value, offset, len);
        }

        public byte[] ComputeHash(System.IO.Stream stream)
        {
            return hashProvider.ComputeHash(stream);
        }

        public void Clear()
        {
            hashProvider.Clear();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            hashProvider.Clear();
        }

        #endregion

        #region ICryptography Members

        public CryptoMode FipsMode
        {
            get { return CryptoMode.FIPS; }
        }

        #endregion

        #region IHashAlgorithm Members


        public byte[] GetTestData()
        {
            return Encoding.UTF8.GetBytes("DocAve Encryption Test Data");
        }

        public byte[] GetTestResult()
        {
            return new byte[] { 17, 21, 204, 236, 168, 207, 215, 86, 228, 69, 248, 3, 45, 183, 11, 148, 154, 22, 161, 46, 126, 35, 88, 28, 214, 238, 154, 5, 223, 77, 3, 191 };
        }

        #endregion
    }
}
