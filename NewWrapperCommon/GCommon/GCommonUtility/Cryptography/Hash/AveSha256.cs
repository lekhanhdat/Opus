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
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography.Hash
{
    public class AveSha256 : IHashAlgorithm, IDisposable
    {
        SHA256CryptoServiceProvider hashProvider = new SHA256CryptoServiceProvider();

        #region IHashAlgorithm Members

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
            return new byte[] { 128, 67, 148, 189, 21, 57, 231, 68, 57, 138, 66, 180, 128, 210, 11, 135, 203, 129, 42, 124, 77, 241, 33, 32, 116, 149, 44, 114, 41, 14, 239, 69 };
        }

        #endregion
    }
}
