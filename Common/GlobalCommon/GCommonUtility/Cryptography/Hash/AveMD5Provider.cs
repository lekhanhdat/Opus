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
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography.Hash
{
    class AveMD5Provider : IHashAlgorithm, IDisposable
    {
        #region IHashAlgorithm Members

        public byte[] ComputeHash(byte[] value)
        {
            return computeHash(value, 0, value.Length);
        }

        public byte[] ComputeHash(byte[] value, int offset, int len)
        {
            return computeHash(value, offset, len);
        }

        public byte[] ComputeHash(System.IO.Stream stream)
        {

            byte[] value = new byte[stream.Length];
            var read = stream.Read(value, 0, value.Length);
            stream.Close();
            return computeHash(value, 0, read);
        }

        public void Clear()
        {
            //throw new NotImplementedException();
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

        #region ICryptography Members

        public CryptoMode FipsMode
        {
            get { return CryptoMode.FIPS; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region MD5 Hash Implement

        private byte[] computeHash(byte[] value, int offset, int len)
        {
            UInt32[] x = Bytes2Uint32s(value, offset, len);

            UInt32 A = 0x67452301;
            UInt32 B = 0xEFCDAB89;
            UInt32 C = 0x98BADCFE;
            UInt32 D = 0x10325476;

            for (int i = 0; i < x.Length; i += 16)
            {
                UInt32 a = A;
                UInt32 b = B;
                UInt32 c = C;
                UInt32 d = D;
                //4294967296*abs(sin(i)) 1<=i<=64
                FF(ref a, b, c, d, x[i + 0], 7, 0xD76AA478);
                FF(ref d, a, b, c, x[i + 1], 12, 0xE8C7B756);
                FF(ref c, d, a, b, x[i + 2], 17, 0x242070DB);
                FF(ref b, c, d, a, x[i + 3], 22, 0xC1BDCEEE);
                FF(ref a, b, c, d, x[i + 4], 7, 0xF57C0FAF);
                FF(ref d, a, b, c, x[i + 5], 12, 0x4787C62A);
                FF(ref c, d, a, b, x[i + 6], 17, 0xA8304613);
                FF(ref b, c, d, a, x[i + 7], 22, 0xFD469501);
                FF(ref a, b, c, d, x[i + 8], 7, 0x698098D8);
                FF(ref d, a, b, c, x[i + 9], 12, 0x8B44F7AF);
                FF(ref c, d, a, b, x[i + 10], 17, 0xFFFF5BB1);
                FF(ref b, c, d, a, x[i + 11], 22, 0x895CD7BE);
                FF(ref a, b, c, d, x[i + 12], 7, 0x6B901122);
                FF(ref d, a, b, c, x[i + 13], 12, 0xFD987193);
                FF(ref c, d, a, b, x[i + 14], 17, 0xA679438E);
                FF(ref b, c, d, a, x[i + 15], 22, 0x49B40821);

                GG(ref a, b, c, d, x[i + 1], 5, 0xF61E2562);
                GG(ref d, a, b, c, x[i + 6], 9, 0xC040B340);
                GG(ref c, d, a, b, x[i + 11], 14, 0x265E5A51);
                GG(ref b, c, d, a, x[i + 0], 20, 0xE9B6C7AA);
                GG(ref a, b, c, d, x[i + 5], 5, 0xD62F105D);
                GG(ref d, a, b, c, x[i + 10], 9, 0x2441453);
                GG(ref c, d, a, b, x[i + 15], 14, 0xD8A1E681);
                GG(ref b, c, d, a, x[i + 4], 20, 0xE7D3FBC8);
                GG(ref a, b, c, d, x[i + 9], 5, 0x21E1CDE6);
                GG(ref d, a, b, c, x[i + 14], 9, 0xC33707D6);
                GG(ref c, d, a, b, x[i + 3], 14, 0xF4D50D87);
                GG(ref b, c, d, a, x[i + 8], 20, 0x455A14ED);
                GG(ref a, b, c, d, x[i + 13], 5, 0xA9E3E905);
                GG(ref d, a, b, c, x[i + 2], 9, 0xFCEFA3F8);
                GG(ref c, d, a, b, x[i + 7], 14, 0x676F02D9);
                GG(ref b, c, d, a, x[i + 12], 20, 0x8D2A4C8A);

                HH(ref a, b, c, d, x[i + 5], 4, 0xFFFA3942);
                HH(ref d, a, b, c, x[i + 8], 11, 0x8771F681);
                HH(ref c, d, a, b, x[i + 11], 16, 0x6D9D6122);
                HH(ref b, c, d, a, x[i + 14], 23, 0xFDE5380C);
                HH(ref a, b, c, d, x[i + 1], 4, 0xA4BEEA44);
                HH(ref d, a, b, c, x[i + 4], 11, 0x4BDECFA9);
                HH(ref c, d, a, b, x[i + 7], 16, 0xF6BB4B60);
                HH(ref b, c, d, a, x[i + 10], 23, 0xBEBFBC70);
                HH(ref a, b, c, d, x[i + 13], 4, 0x289B7EC6);
                HH(ref d, a, b, c, x[i + 0], 11, 0xEAA127FA);
                HH(ref c, d, a, b, x[i + 3], 16, 0xD4EF3085);
                HH(ref b, c, d, a, x[i + 6], 23, 0x4881D05);
                HH(ref a, b, c, d, x[i + 9], 4, 0xD9D4D039);
                HH(ref d, a, b, c, x[i + 12], 11, 0xE6DB99E5);
                HH(ref c, d, a, b, x[i + 15], 16, 0x1FA27CF8);
                HH(ref b, c, d, a, x[i + 2], 23, 0xC4AC5665);

                II(ref a, b, c, d, x[i + 0], 6, 0xF4292244);
                II(ref d, a, b, c, x[i + 7], 10, 0x432AFF97);
                II(ref c, d, a, b, x[i + 14], 15, 0xAB9423A7);
                II(ref b, c, d, a, x[i + 5], 21, 0xFC93A039);
                II(ref a, b, c, d, x[i + 12], 6, 0x655B59C3);
                II(ref d, a, b, c, x[i + 3], 10, 0x8F0CCC92);
                II(ref c, d, a, b, x[i + 10], 15, 0xFFEFF47D);
                II(ref b, c, d, a, x[i + 1], 21, 0x85845DD1);
                II(ref a, b, c, d, x[i + 8], 6, 0x6FA87E4F);
                II(ref d, a, b, c, x[i + 15], 10, 0xFE2CE6E0);
                II(ref c, d, a, b, x[i + 6], 15, 0xA3014314);
                II(ref b, c, d, a, x[i + 13], 21, 0x4E0811A1);
                II(ref a, b, c, d, x[i + 4], 6, 0xF7537E82);
                II(ref d, a, b, c, x[i + 11], 10, 0xBD3AF235);
                II(ref c, d, a, b, x[i + 2], 15, 0x2AD7D2BB);
                II(ref b, c, d, a, x[i + 9], 21, 0xEB86D391);

                A = A + a;
                B = B + b;
                C = C + c;
                D = D + d;
            }

            byte[] result = new byte[16];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)(0xff & (A >> (i * 8)));
                result[i + 4] = (byte)(0xff & (B >> (i * 8)));
                result[i + 8] = (byte)(0xff & (C >> (i * 8)));
                result[i + 12] = (byte)(0xff & (D >> (i * 8)));
            }

            return result;
        }

        private UInt32[] Bytes2Uint32s(byte[] value, int offset, int len)
        {
            const int ChunkBits = 512;
            const int CONGRUENT_BITS = 448;
            const int ByteBit = 8;//1Byte=8Bits
            const int WordByte = 4;//1Word = 4Bytes
            const int WordBit = 32;//1Word = 32Bits

            if (value == null) throw new ArgumentNullException();
            if (offset < 0 || len < 0 || (offset + len) > value.Length) throw new ArgumentOutOfRangeException();

            int dataLength = len;
            int chunkCount = (((dataLength + ((ChunkBits - CONGRUENT_BITS) / ByteBit)) / (ChunkBits / ByteBit)) + 1) * (ChunkBits / WordBit);
            int chunkPosition = 0;

            UInt32[] chunks = new UInt32[chunkCount];

            int bytePosition = 0;
            int byteCount = 0;

            while (byteCount < dataLength)
            {
                chunkPosition = byteCount / WordByte;
                bytePosition = (byteCount % WordByte) * ByteBit;
                chunks[chunkPosition] = chunks[chunkPosition] | LShift(value[byteCount + offset], bytePosition);
                byteCount++;
            }

            chunkPosition = byteCount / WordByte;
            bytePosition = (byteCount % WordByte) * ByteBit;
            chunks[chunkPosition] = chunks[chunkPosition] | LShift(0x80, bytePosition);
            chunks[chunkCount - 2] = LShift((UInt32)dataLength, 3);
            chunks[chunkCount - 1] = RShift((UInt32)dataLength, 29);

            return chunks;
        }

        private UInt32 F(UInt32 x, UInt32 y, UInt32 z)
        {
            return (x & y) | ((~x) & z);
        }

        private UInt32 G(UInt32 x, UInt32 y, UInt32 z)
        {
            return (x & z) | (y & (~z));
        }

        private UInt32 H(UInt32 x, UInt32 y, UInt32 z)
        {
            return x ^ y ^ z;
        }

        private UInt32 I(UInt32 x, UInt32 y, UInt32 z)
        {
            return y ^ (x | (~z));
        }

        private void FF(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, int offset, UInt32 ac)
        {
            SUM(ref a, b, x, offset, ac, F(b, c, d));
        }

        private void GG(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, int offset, UInt32 ac)
        {
            SUM(ref a, b, x, offset, ac, G(b, c, d));
        }

        private void HH(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, int offset, UInt32 ac)
        {
            SUM(ref a, b, x, offset, ac, H(b, c, d));
        }

        private void II(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, int offset, UInt32 ac)
        {
            SUM(ref a, b, x, offset, ac, I(b, c, d));
        }

        private void SUM(ref UInt32 a, UInt32 b, UInt32 x, int offset, UInt32 ac, UInt32 sum)
        {
            a = a + sum + x + ac;
            a = a << offset | a >> (32 - offset);
            a = a + b;
        }

        private UInt32 LShift(UInt32 value, int offset)
        {
            return value << offset;
        }

        private UInt32 RShift(UInt32 value, int offset)
        {
            return value >> offset;
        }

        #endregion
    }
}
