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
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography.Hash
{
    public class HmacSha1 : IHashAlgorithm, IDisposable
    {
        #region IHashAlgorithm Members

        private HMACSHA1 hashProvider;
        
        public HmacSha1(SecureString key)
        {
            hashProvider = new HMACSHA1(CryptoUtil.ConvertSecureStringToBytes(key), false);

        }

        public HmacSha1(byte[] key) 
        {
            hashProvider = new HMACSHA1(key, false);
        }

        public HmacSha1()
        {
            hashProvider = new HMACSHA1();
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
            return new byte[] { 180, 196, 162, 182, 32, 238, 169, 130, 237, 184, 195, 177, 117, 117, 180, 156, 251, 94, 46, 210 };
        }

        #endregion
    }
}
