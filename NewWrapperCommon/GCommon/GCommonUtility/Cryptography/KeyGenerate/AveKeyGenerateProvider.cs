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
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.IO;

namespace AvePoint.GCommon.Utility.Cryptography.KeyGenerate
{
    public class AveKeyGenerateProvider : IKeyGenerate, IDisposable
    {
        RNGCryptoServiceProvider generator;
        private MemoryStream stream = new MemoryStream(32);


        public AveKeyGenerateProvider()
        {
            if (generator == null)
            {
                generator = new RNGCryptoServiceProvider();
            }
            byte[] temp = new byte[16];

            generator.GetBytes(temp);
            TestResult(temp);

        }

        private void TestResult(byte[] result)
        {
            foreach(byte b in result) {

                if (stream.Length == stream.Capacity)
                {
                    byte[] buf1 = new byte[16];

                    byte[] buf2 = new byte[16];
                    stream.Position = 0;
                    stream.Read(buf1, 0, buf1.Length);
                    stream.Read(buf2, 0, buf2.Length);

                    if (CryptographyManagement.ArraysEqual<byte>(buf1, buf2))
                    {

                        CryptoModuleStateMachine.Process(CryptoEvent.ConditionalSelfTestFailed);
                        throw new Exception("KeyGenerate test failed");

                    }
                    else
                    {
                        stream.SetLength(0);
                        stream.Write(buf2, 0, buf2.Length);

                    }



                }
                stream.WriteByte(b);

            }


        }

        public byte[] GenerateKeyBytes(int length)
        {
            byte[] result = new byte[length];
            generator.GetNonZeroBytes(result);
            TestResult(result);
            return result;

        }

        public void GenerateKey(byte[] fillBytes)
        {
  
            generator.GetNonZeroBytes(fillBytes);
            TestResult(fillBytes);

        }

        public System.Security.SecureString GenerateKeyString(int length)
        {
            byte[] tmp = this.GenerateKeyBytes(length);
            char[] tmpChar = Encoding.UTF8.GetChars(tmp);
            SecureString result = new SecureString();
            foreach (char t in tmpChar)
            {
                result.AppendChar(t);

            }
            result.MakeReadOnly();
            return result;
        }


        public System.Security.SecureString GenerateVisibleKeyString(int length)
        {
            byte[] tmp = this.GenerateKeyBytes(length);
            for (int i = 0; i < length; i++)
            {
                if (tmp[i] < 33)
                {
                    tmp[i] = (byte)(tmp[i] + 32);
                }
                else if (tmp[i] > 126 && tmp[i] < 254)
                {
                    tmp[i] = (byte)(tmp[i] >> 1);
                }
                else if (tmp[i] >= 254)
                {
                    tmp[i] -= 10;
                    tmp[i] = (byte)(tmp[i] >> 1);
                }
            }
            char[] tmpChar = Encoding.UTF8.GetChars(tmp);
            SecureString result = new SecureString();
            foreach (char t in tmpChar)
            {
                result.AppendChar(t);

            }
            result.MakeReadOnly();
            return result;
        }
        //public SecureString GenerateKeyString(int length)
        //{
        //    SecureString reuslt = new SecureString();
        //    byte[] tmp = this.GenerateKeyBytes(length);
        //    char[] tmpChars = Encoding.UTF8.GetChars(tmp);
        //    foreach (char tmpChar in tmpChars)
        //    {

        //        reuslt.AppendChar(tmpChar);

        //    }
        //    reuslt.MakeReadOnly();
        //    return reuslt;
        //}


        #region ICryptography Members

        public CryptoMode FipsMode
        {
            get { return CryptoMode.FIPS; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion
    }

}
