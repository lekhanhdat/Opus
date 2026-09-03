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





namespace AvePoint.GCommon.Utility.Cryptography.Encryption.Aes
{
    #region using directives
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    #endregion

    internal sealed class CapiSymmetricAlgorithm : ICryptoTransform, IDisposable
    {
        // Fields
        private int m_blockSize;
        private byte[] m_depadBuffer;
        private EncryptionMode m_encryptionMode;
        private SafeCapiKeyHandle m_key;
        private PaddingMode m_paddingMode;
        private SafeCspHandle m_provider;

        // Methods
        [SecurityCritical]
        public CapiSymmetricAlgorithm(int blockSize, int feedbackSize, SafeCspHandle provider, SafeCapiKeyHandle key, byte[] iv, CipherMode cipherMode, PaddingMode paddingMode, EncryptionMode encryptionMode)
        {
            this.m_blockSize = blockSize;
            this.m_encryptionMode = encryptionMode;
            this.m_paddingMode = paddingMode;
            this.m_provider = provider.Duplicate();
            this.m_key = SetupKey(key, ProcessIV(iv, blockSize, cipherMode), cipherMode, feedbackSize);
        }

        [SecurityCritical]
        private int DecryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            int num = 0;
            if ((this.m_paddingMode != PaddingMode.None) && (this.m_paddingMode != PaddingMode.Zeros))
            {
                if (this.m_depadBuffer != null)
                {
                    int count = this.RawDecryptBlocks(this.m_depadBuffer, 0, this.m_depadBuffer.Length);
                    Buffer.BlockCopy(this.m_depadBuffer, 0, outputBuffer, outputOffset, count);
                    Array.Clear(this.m_depadBuffer, 0, this.m_depadBuffer.Length);
                    outputOffset += count;
                    num += count;
                }
                else
                {
                    this.m_depadBuffer = new byte[this.InputBlockSize];
                }
                Buffer.BlockCopy(inputBuffer, (inputOffset + inputCount) - this.m_depadBuffer.Length, this.m_depadBuffer, 0, this.m_depadBuffer.Length);
                inputCount -= this.m_depadBuffer.Length;
            }
            if (inputCount > 0)
            {
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                num += this.RawDecryptBlocks(outputBuffer, outputOffset, inputCount);
            }
            return num;
        }

        private byte[] DepadBlock(byte[] block, int offset, int count)
        {
            int num = 0;
            switch (this.m_paddingMode)
            {
                case PaddingMode.None:
                case PaddingMode.Zeros:
                    num = 0;
                    break;

                case PaddingMode.PKCS7:
                    num = block[(offset + count) - 1];
                    if ((num <= 0) || (num > this.InputBlockSize))
                    {
                        throw new CryptographicException("Cryptography_InvalidPadding");
                    }
                    for (int i = (offset + count) - num; i < (offset + count); i++)
                    {
                        if (block[i] != num)
                        {
                            throw new CryptographicException("Cryptography_InvalidPadding");
                        }
                    }
                    break;

                case PaddingMode.ANSIX923:
                    num = block[(offset + count) - 1];
                    if ((num <= 0) || (num > this.InputBlockSize))
                    {
                        throw new CryptographicException("Cryptography_InvalidPadding");
                    }
                    for (int j = (offset + count) - num; j < ((offset + count) - 1); j++)
                    {
                        if (block[j] != 0)
                        {
                            throw new CryptographicException("Cryptography_InvalidPadding");
                        }
                    }
                    break;

                case PaddingMode.ISO10126:
                    num = block[(offset + count) - 1];
                    if ((num <= 0) || (num > this.InputBlockSize))
                    {
                        throw new CryptographicException("Cryptography_InvalidPadding");
                    }
                    break;

                default:
                    throw new CryptographicException("Cryptography_UnknownPaddingMode");
            }
            byte[] dst = new byte[count - num];
            Buffer.BlockCopy(block, offset, dst, 0, dst.Length);
            return dst;
        }

        [SecurityCritical]
        public void Dispose()
        {
            if (this.m_key != null)
            {
                this.m_key.Dispose();
            }
            if (this.m_provider != null)
            {
                this.m_provider.Dispose();
            }
            if (this.m_depadBuffer != null)
            {
                Array.Clear(this.m_depadBuffer, 0, this.m_depadBuffer.Length);
            }
        }

        [SecurityCritical]
        private unsafe int EncryptBlocks(byte[] buffer, int offset, int count)
        {
            int pdwDataLen = count;
            fixed (byte* numRef = &(buffer[offset]))
            {
                if (!CapiNative.UnsafeNativeMethods.CryptEncrypt(this.m_key, SafeCapiHashHandle.InvalidHandle, false, 0, new IntPtr((void*)numRef), ref pdwDataLen, buffer.Length - offset))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return pdwDataLen;
        }

        [SecurityCritical]
        private byte[] PadBlock(byte[] block, int offset, int count)
        {
            byte[] dst = null;
            int num = this.InputBlockSize - (count % this.InputBlockSize);
            switch (this.m_paddingMode)
            {
                case PaddingMode.None:
                    if ((count % this.InputBlockSize) != 0)
                    {
                        throw new CryptographicException("Cryptography_PartialBlock");
                    }
                    dst = new byte[count];
                    Buffer.BlockCopy(block, offset, dst, 0, dst.Length);
                    return dst;

                case PaddingMode.PKCS7:
                    dst = new byte[count + num];
                    Buffer.BlockCopy(block, offset, dst, 0, count);
                    for (int i = count; i < dst.Length; i++)
                    {
                        dst[i] = (byte)num;
                    }
                    return dst;

                case PaddingMode.Zeros:
                    if (num == this.InputBlockSize)
                    {
                        num = 0;
                    }
                    dst = new byte[count + num];
                    Buffer.BlockCopy(block, offset, dst, 0, count);
                    return dst;

                case PaddingMode.ANSIX923:
                    dst = new byte[count + num];
                    Buffer.BlockCopy(block, 0, dst, 0, count);
                    dst[dst.Length - 1] = (byte)num;
                    return dst;

                case PaddingMode.ISO10126:
                    dst = new byte[count + num];
                    CapiNative.UnsafeNativeMethods.CryptGenRandom(this.m_provider, dst.Length - 1, dst);
                    Buffer.BlockCopy(block, 0, dst, 0, count);
                    dst[dst.Length - 1] = (byte)num;
                    return dst;
            }
            throw new CryptographicException("Cryptography_UnknownPaddingMode");
        }

        private static byte[] ProcessIV(byte[] iv, int blockSize, CipherMode cipherMode)
        {
            byte[] dst = null;
            if (iv != null)
            {
                if ((blockSize / 8) > iv.Length)
                {
                    throw new CryptographicException("Cryptography_InvalidIVSize");
                }
                dst = new byte[blockSize / 8];
                Buffer.BlockCopy(iv, 0, dst, 0, dst.Length);
                return dst;
            }
            if (cipherMode != CipherMode.ECB)
            {
                throw new CryptographicException("Cryptography_MissingIV");
            }
            return dst;
        }

        [SecurityCritical]
        private unsafe int RawDecryptBlocks(byte[] buffer, int offset, int count)
        {
            int pdwDataLen = count;
            fixed (byte* numRef = &(buffer[offset]))
            {
                if (!CapiNative.UnsafeNativeMethods.CryptDecrypt(this.m_key, SafeCapiHashHandle.InvalidHandle, false, 0, new IntPtr((void*)numRef), ref pdwDataLen))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return pdwDataLen;
        }


        [SecurityCritical]
        private static SafeCapiKeyHandle SetupKey(SafeCapiKeyHandle key, byte[] iv, CipherMode cipherMode, int feedbackSize)
        {

            SafeCapiKeyHandle handle = key.Duplicate();
            CapiNative.SetKeyParameter(handle, CapiNative.KeyParameter.Mode, (int)cipherMode);
            if (cipherMode != CipherMode.ECB)
            {
                CapiNative.SetKeyParameter(handle, CapiNative.KeyParameter.IV, iv);
            }
            if ((cipherMode == CipherMode.CFB) || (cipherMode == CipherMode.OFB))
            {
                CapiNative.SetKeyParameter(handle, CapiNative.KeyParameter.ModeBits, feedbackSize);
            }
            return handle;
        }

        [SecurityCritical]
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (inputCount <= 0)
            {
                throw new ArgumentOutOfRangeException("inputCount");
            }
            if ((inputCount % this.InputBlockSize) != 0)
            {
                throw new ArgumentOutOfRangeException("inputCount", "Cryptography_MustTransformWholeBlock");
            }
            if (inputCount > (inputBuffer.Length - inputOffset))
            {
                throw new ArgumentOutOfRangeException("inputCount", "Cryptography_TransformBeyondEndOfBuffer");
            }
            if (outputBuffer == null)
            {
                throw new ArgumentNullException("outputBuffer");
            }
            if (inputCount > (outputBuffer.Length - outputOffset))
            {
                throw new ArgumentOutOfRangeException("outputOffset", "Cryptography_TransformBeyondEndOfBuffer");
            }
            if (this.m_encryptionMode == EncryptionMode.Encrypt)
            {
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                return this.EncryptBlocks(outputBuffer, outputOffset, inputCount);
            }
            return this.DecryptBlocks(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        [SecurityCritical]
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (inputCount < 0)
            {
                throw new ArgumentOutOfRangeException("inputCount");
            }
            if (inputCount > (inputBuffer.Length - inputOffset))
            {
                throw new ArgumentOutOfRangeException("inputCount", "Cryptography_TransformBeyondEndOfBuffer");
            }
            byte[] buffer = null;
            if (this.m_encryptionMode == EncryptionMode.Encrypt)
            {
                buffer = this.PadBlock(inputBuffer, inputOffset, inputCount);
                if (buffer.Length > 0)
                {
                    this.EncryptBlocks(buffer, 0, buffer.Length);
                }
            }
            else
            {
                if ((inputCount % this.InputBlockSize) != 0)
                {
                    throw new CryptographicException("Cryptography_PartialBlock");
                }
                byte[] dst = null;
                if (this.m_depadBuffer == null)
                {
                    dst = new byte[inputCount];
                    Buffer.BlockCopy(inputBuffer, inputOffset, dst, 0, inputCount);
                }
                else
                {
                    dst = new byte[this.m_depadBuffer.Length + inputCount];
                    Buffer.BlockCopy(this.m_depadBuffer, 0, dst, 0, this.m_depadBuffer.Length);
                    Buffer.BlockCopy(inputBuffer, inputOffset, dst, this.m_depadBuffer.Length, inputCount);
                }
                if (dst.Length > 0)
                {
                    int count = this.RawDecryptBlocks(dst, 0, dst.Length);
                    buffer = this.DepadBlock(dst, 0, count);
                }
                else
                {
                    buffer = new byte[0];
                }
            }
            return buffer;
        }

        // Properties
        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }

        public int InputBlockSize
        {
            get
            {
                return (this.m_blockSize / 8);
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return (this.m_blockSize / 8);
            }
        }
    }
}
