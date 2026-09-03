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

namespace MediaDataIO
{
    /// <summary>
    /// Please don't change the flag values 
    /// </summary>
    internal class TransferFlag
    {
        /*
            |COMPRESSED_Flag1|COMPRESSED_Flag2|CompressionMethod|
            |0|0|ZLib|
            |1|0|Brotli|
            |0|1||
            |1|1||
        */
        internal const byte COMPRESSED_Flag1 = 1 << 6;
        internal const byte COMPRESSED_Flag2 = 1 << 7;
        //public static readonly byte AGENT_ENCRYPTED = 1 << 5;//32
        //public static readonly byte AGENT_COMPRESSED = 1 << 4;//16
        internal const byte MEDIA_ENCRYPTED = 1 << 3;//8
        internal const byte MEDIA_COMPRESSED = 1 << 2;//4

        internal static bool IsModeSet(byte srcMode, byte destinationMode) => (srcMode & destinationMode) == destinationMode;
    }

    public static class TransferFlagBuilder
    {
        public static byte Build(EncryptionMethods encryption, CompressionMethods compression)
        {
            var encryptionByte = encryption switch
            {
                EncryptionMethods.None => 0,
                EncryptionMethods.AesCbc => TransferFlag.MEDIA_ENCRYPTED,
                _ => 0
            };

            var compressionByte = compression switch
            {
                CompressionMethods.None => 0,
                CompressionMethods.Zlib => TransferFlag.MEDIA_COMPRESSED,
                CompressionMethods.Brotli => TransferFlag.MEDIA_COMPRESSED | TransferFlag.COMPRESSED_Flag1,
                _ => 0
            };

            return (byte)(encryptionByte | compressionByte);
        }

        public static bool IsMediaEncrypted(this byte value)
        {
            return TransferFlag.IsModeSet(value, TransferFlag.MEDIA_ENCRYPTED);
        }

        public static bool IsCompressed(this byte value)
        {
            return value.GetCompressMethod() != CompressionMethods.None;
        }

        public static bool IsZlibCompressed(this byte value)
        {
            return value.GetCompressMethod() == CompressionMethods.Zlib;
        }

        public static bool IsBrotliCompressed(this byte value)
        {
            return value.GetCompressMethod() == CompressionMethods.Brotli;
        }

        public static CompressionMethods GetCompressMethod(this byte value)
        {
            return (TransferFlag.IsModeSet(value, TransferFlag.MEDIA_COMPRESSED), TransferFlag.IsModeSet(value, TransferFlag.COMPRESSED_Flag1)) switch
            {
                (bool isCompressed, bool compressFlag1) when !isCompressed => CompressionMethods.None,
                (bool isCompressed, bool compressFlag1) when isCompressed && (!compressFlag1) => CompressionMethods.Zlib,
                (bool isCompressed, bool compressFlag1) when isCompressed && compressFlag1 => CompressionMethods.Brotli,
                _ => throw new NotSupportedException()
            };
        }
    }

    public enum EncryptionMethods
    {
        None = 0,
        AesCbc = 1
    }

    public enum CompressionMethods
    {
        None = 0,
        Zlib = 1,
        Brotli = 2
    }
}