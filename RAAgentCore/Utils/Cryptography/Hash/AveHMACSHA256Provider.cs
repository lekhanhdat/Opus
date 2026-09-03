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

namespace AvePoint.Hybrid.Utility.Cryptography.Hash
{
    public class AveHMACSHA256Provider : IHashAlgorithm, IDisposable
    {
        #region IHashAlgorithm Members

        private byte[] key;

        public AveHMACSHA256Provider(SecureString key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException();
            }
            this.key = Computer64BytePassword(CryptoUtil.ConvertSecureStringToBytes(key));
        }

        public AveHMACSHA256Provider(byte[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException();
            }
            this.key = Computer64BytePassword(key);
        }

        public AveHMACSHA256Provider()
        {
            //hashProvider = new HMACSHA256();
        }

        public byte[] ComputeHash(byte[] value)
        {
            return hmac(key, value);
        }

        public byte[] ComputeHash(byte[] value, int offset, int len)
        {
            byte[] subArray = new byte[len];
            System.Buffer.BlockCopy(value, offset, subArray, 0, len);
            return hmac(key, subArray);
        }

        public byte[] ComputeHash(System.IO.Stream stream)
        {
            byte[] value = new byte[stream.Length];
            var realLength = stream.Read(value, 0, value.Length);
            stream.Close();
            if (realLength != value.Length)
            {
                throw new Exception("not read completely. please modify this method to ensure all content can be read.");
            }
            return hmac(key, value);
        }

        public void Clear()
        {
            
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
           
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

        #region
        byte[] Computer64BytePassword(byte[] pass)
        {
            if (pass.Length > 64)
            {
                string hash = String.Empty;
                AveSHA256Provider crypt = new AveSHA256Provider();
                byte[] crypto = crypt.ComputeHash(pass);
                hash = Convert.ToBase64String(crypto);
                foreach (byte bit in crypto)
                {
                    hash += bit.ToString("x2");
                }
                return Encoding.UTF8.GetBytes(hash);
            }
            else if (pass.Length < 64)
            {
                byte[] retvalue = new byte[64];
                Array.Copy(pass, retvalue, pass.Length);
                return retvalue;
            }

            return pass;
        }

         static byte[] xor(byte[] data, byte xor)
            {
                byte[] buffer = new Byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                    buffer[i] = Convert.ToByte(Convert.ToInt32(data[i]) ^ Convert.ToInt32(xor));

                return buffer;
            }

            /// <summary> 
            /// This function creates the proper HMAC-SHA256 response
            /// </summary>
            /// <param name="password">the password</param>
            /// <param name="challenge">the challenge</param >
            /// <returns>the hmac-sha256 response to send to InspIRCd</returns> 

            static byte[] hmac(byte[] pass, byte[] message)
            {

                byte[] ki = xor(pass, (byte)0x36);
                byte[] ko = xor(pass, (byte)0x5C);

                byte[] sha2 = sha256(ArrayContact(ki , message));
                byte[] sha1 = sha256(ArrayContact(ko , sha2));
                
                return sha1;
            }
           

            static byte[] sha256(byte[] message)
            {
                AveSHA256Provider test = new AveSHA256Provider();
                byte[] crypto = test.ComputeHash(message);
                return crypto;
            }

            static byte[] ArrayContact(byte[] array1, byte[] array2)
            {
                byte[] rv = new byte[array1.Length + array2.Length];
                System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
                System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
                return rv;
            }
        
        #endregion
    }

    class AveSHA256Provider
    {
        static UInt32[] K = new UInt32[] {
                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                 0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                 0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                 0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                 0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                 0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                 0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                 0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
                };

        UInt32[] H = new UInt32[] { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };



        public byte[] ComputeHash(byte[] message)
        {

            int count = (message.Length * 8 + 1) % 512 - 448;

            int length = message.Length;
            int N = (message.Length * 8 + 1) % 512 - 448 < 0 ? (message.Length * 8 + 1) / 512 + 1 : (message.Length * 8 + 1) / 512 + 2;


            UInt32[] M = Bytes2Uint32s(message, 0, message.Length);
            UInt32[] W = new UInt32[64];
            for (int i = 0; i < N; i++)
            {
                UInt32 a = H[0]; UInt32 b = H[1]; UInt32 c = H[2]; UInt32 d = H[3]; UInt32 e = H[4]; UInt32 f = H[5]; UInt32 g = H[6]; UInt32 h = H[7];
                //将要处理的数据导入到W中
                for (int t = 0; t < 64; t++)
                {
                    if (t < 16)
                    {
                        W[t] = M[i * 16 + t];
                    }
                    else
                    {
                        W[t] = (sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16]) & 0xffffffff;
                    }
                }

                for (int j = 0; j < 64; j++)
                {
                    UInt32 T1 = h + Sigma1(e) + Ch(e, f, g) + K[j] + W[j];
                    UInt32 T2 = Sigma0(a) + Maj(a, b, c);
                    h = g;
                    g = f;
                    f = e;
                    e = (d + T1) & 0xffffffff;
                    d = c;
                    c = b;
                    b = a;
                    a = (T1 + T2) & 0xffffffff;
                }

                H[0] = (H[0] + a) & 0xffffffff;
                H[1] = (H[1] + b) & 0xffffffff;
                H[2] = (H[2] + c) & 0xffffffff;
                H[3] = (H[3] + d) & 0xffffffff;
                H[4] = (H[4] + e) & 0xffffffff;
                H[5] = (H[5] + f) & 0xffffffff;
                H[6] = (H[6] + g) & 0xffffffff;
                H[7] = (H[7] + h) & 0xffffffff;
            }

            byte[] result = new byte[32];

            for (int i = 0; i < H.Length; i++)
            {
                int j = 4 * i;
                result[j + 3] = (byte)(H[i] & 0xFF);
                result[j + 2] = (byte)(H[i] >> 8 & 0xFF);
                result[j + 1] = (byte)(H[i] >> 16 & 0xFF);
                result[j] = (byte)(H[i] >> 24 & 0xFF);
            }
            return result;
        }

        private UInt32 SHR(int n, UInt32 x)
        {
            return ((x & 0xFFFFFFFF) >> n);
        }

        private UInt32 ROTR(int n, UInt32 x)
        {
            return (SHR(n, x) | (x << (32 - n)));
        }

        private UInt32 Ch(UInt32 x, UInt32 y, UInt32 z)
        {
            return ((x) & (y)) ^ ((~(x)) & (z));
        }

        private UInt32 Maj(UInt32 x, UInt32 y, UInt32 z)
        {
            return ((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z));
        }

        private UInt32 Sigma0(UInt32 x)
        {
            return ROTR(2, x) ^ ROTR(13, x) ^ ROTR(22, x);
        }

        private UInt32 Sigma1(UInt32 x)
        {
            return ROTR(6, x) ^ ROTR(11, x) ^ ROTR(25, x);
        }

        private UInt32 sigma0(UInt32 x)
        {
            return ROTR(7, x) ^ ROTR(18, x) ^ SHR(3, x);
        }

        private UInt32 sigma1(UInt32 x)
        {
            return ROTR(17, x) ^ ROTR(19, x) ^ SHR(10, x);
        }

        private byte[] ArrayContact(byte[] array1, byte[] array2)
        {
            byte[] rv = new byte[array1.Length + array2.Length];
            System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
            return rv;
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
                bytePosition = (3 - byteCount % WordByte) * ByteBit;
                chunks[chunkPosition] = chunks[chunkPosition] | LShift(value[byteCount + offset], bytePosition);
                byteCount++;
            }

            chunkPosition = byteCount / WordByte;
            bytePosition = (3 - byteCount % WordByte) * ByteBit;
            chunks[chunkPosition] = chunks[chunkPosition] | LShift(0x80, bytePosition);
            chunks[chunkCount - 2] = RShift((UInt32)dataLength, 29);
            chunks[chunkCount - 1] = LShift((UInt32)dataLength, 3);

            return chunks;
        }

        private UInt32 LShift(UInt32 value, int offset)
        {
            return value << offset;
        }

        private UInt32 RShift(UInt32 value, int offset)
        {
            return value >> offset;
        }
    }

}
