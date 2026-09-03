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
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography.Hash
{
    class AveMd5 : IHashAlgorithm, IDisposable
    {
        MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();

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

        #region ICryptography Members

        public CryptoMode FipsMode
        {
            get { return CryptoMode.NoneFIPS; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            hashProvider.Clear();
        }

        #endregion

        #region IHashAlgorithm Members


        public byte[] GetTestData()
        {
            return Encoding.UTF8.GetBytes("DocAve Encryption Test Data");
        }

        public byte[] GetTestResult()
        {
            return new byte[] { 211, 37, 254, 133, 182, 76, 198, 113, 198, 237, 48, 197, 217, 2, 115, 113 };
        }

        #endregion
    }
}
