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
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    #endregion

    internal static class CapiNative
    {
        // Methods
        [SecurityCritical]
        internal static SafeCspHandle AcquireCsp(string keyContainer, string providerName, ProviderType providerType, CryptAcquireContextFlags flags, bool throwPlatformException)
        {
            SafeCspHandle phProv = null;
            if (UnsafeNativeMethods.CryptAcquireContext(out phProv, keyContainer, providerName, providerType, flags))
            {
                return phProv;
            }
            int hr = Marshal.GetLastWin32Error();
            if (!throwPlatformException || ((hr != -2146893801) && (hr != -2146893799)))
            {
                throw new CryptographicException(hr);
            }
            throw new PlatformNotSupportedException("Cryptography_PlatformNotSupported");
        }

        [SecurityCritical]
        internal static byte[] ExportSymmetricKey(SafeCapiKeyHandle key)
        {

            int pdwDataLen = 0;
            if (!UnsafeNativeMethods.CryptExportKey(key, SafeCapiKeyHandle.InvalidHandle, 8, 0, null, ref pdwDataLen))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr != 0xea)
                {
                    throw new CryptographicException(hr);
                }
            }
            byte[] pbData = new byte[pdwDataLen];
            if (!UnsafeNativeMethods.CryptExportKey(key, SafeCapiKeyHandle.InvalidHandle, 8, 0, pbData, ref pdwDataLen))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            int srcOffset = Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int));
            byte[] dst = new byte[BitConverter.ToInt32(pbData, Marshal.SizeOf(typeof(BLOBHEADER)))];
            Buffer.BlockCopy(pbData, srcOffset, dst, 0, dst.Length);
            return dst;
        }

        internal static string GetAlgorithmName(AlgorithmId algorithm)
        {
            return algorithm.ToString().ToUpper(CultureInfo.InvariantCulture);
        }

        [SecurityCritical]
        internal static byte[] GetHashParameter(SafeCapiHashHandle hashHandle, HashParameter parameter)
        {
            int pdwDataLen = 0;
            if (!UnsafeNativeMethods.CryptGetHashParam(hashHandle, parameter, null, ref pdwDataLen, 0))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            byte[] pbData = new byte[pdwDataLen];
            if (!UnsafeNativeMethods.CryptGetHashParam(hashHandle, parameter, pbData, ref pdwDataLen, 0))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (pdwDataLen != pbData.Length)
            {
                byte[] dst = new byte[pdwDataLen];
                Buffer.BlockCopy(pbData, 0, dst, 0, pdwDataLen);
                pbData = dst;
            }
            return pbData;
        }

        [SecurityCritical]
        internal static T GetProviderParameterStruct<T>(SafeCspHandle provider, ProviderParameter parameter, ProviderParameterFlags flags) where T : struct
        {
            T local;

            int pdwDataLen = 0;
            IntPtr zero = IntPtr.Zero;
            if (!UnsafeNativeMethods.CryptGetProvParam(provider, parameter, zero, ref pdwDataLen, flags))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr == 0x103)
                {
                    return default(T);
                }
                if (hr != 0xea)
                {
                    throw new CryptographicException(hr);
                }
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    zero = Marshal.AllocCoTaskMem(pdwDataLen);
                }
                if (!UnsafeNativeMethods.CryptGetProvParam(provider, parameter, zero, ref pdwDataLen, flags))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                local = (T)Marshal.PtrToStructure(zero, typeof(T));
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            return local;
        }

        internal static int HResultForVerificationResult(SignatureVerificationResult verificationResult)
        {
            switch (verificationResult)
            {
                case SignatureVerificationResult.AssemblyIdentityMismatch:
                case SignatureVerificationResult.PublicKeyTokenMismatch:
                case SignatureVerificationResult.PublisherMismatch:
                    return -2146762749;

                case SignatureVerificationResult.ContainingSignatureInvalid:
                    return -2146869232;
            }
            return (int)verificationResult;
        }

        [SecurityCritical]
        internal static unsafe SafeCapiKeyHandle ImportSymmetricKey(SafeCspHandle provider, AlgorithmId algorithm, byte[] key)
        {
            int num = (Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int))) + key.Length;
            byte[] dst = new byte[num];
            fixed (byte* numRef = dst)
            {
                BLOBHEADER* blobheaderPtr = (BLOBHEADER*)numRef;
                blobheaderPtr->bType = KeyBlobType.PlainText;
                blobheaderPtr->bVersion = 2;
                blobheaderPtr->reserved = 0;
                blobheaderPtr->aiKeyAlg = algorithm;
                int* numPtr = (int*)(numRef + Marshal.SizeOf(blobheaderPtr[0]));
                numPtr[0] = key.Length;
            }
            Buffer.BlockCopy(key, 0, dst, Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int)), key.Length);
            SafeCapiKeyHandle phKey = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (!UnsafeNativeMethods.CryptImportKey(provider, dst, dst.Length, SafeCapiKeyHandle.InvalidHandle, KeyFlags.Exportable, out phKey))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if ((phKey != null) && !phKey.IsInvalid)
                {
                    phKey.SetCsp(provider);
                }
            }
            return phKey;
        }

        [SecurityCritical]
        internal static void SetKeyParameter(SafeCapiKeyHandle key, KeyParameter parameter, int value)
        {
            SetKeyParameter(key, parameter, BitConverter.GetBytes(value));
        }

        [SecurityCritical]
        internal static void SetKeyParameter(SafeCapiKeyHandle key, KeyParameter parameter, byte[] value)
        {

            if (!UnsafeNativeMethods.CryptSetKeyParam(key, parameter, value, 0))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        // Nested Types
        internal enum AlgorithmClass
        {
            DataEncryption = 0x6000,
            Hash = 0x8000
        }

        internal enum AlgorithmId
        {
            Aes128 = 0x660e,
            Aes192 = 0x660f,
            Aes256 = 0x6610,
            MD5 = 0x8003,
            None = 0,
            Sha1 = 0x8004,
            Sha256 = 0x800c,
            Sha384 = 0x800d,
            Sha512 = 0x800e
        }

        internal enum AlgorithmSubId
        {
            Aes128 = 14,
            Aes192 = 15,
            Aes256 = 0x10,
            MD5 = 3,
            Sha1 = 4,
            Sha256 = 12,
            Sha384 = 13,
            Sha512 = 14
        }

        internal enum AlgorithmType
        {
            Any = 0,
            Block = 0x600
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BLOBHEADER
        {
            public CapiNative.KeyBlobType bType;
            public byte bVersion;
            public short reserved;
            public CapiNative.AlgorithmId aiKeyAlg;
        }

        [Flags]
        internal enum CryptAcquireContextFlags
        {
            None = 0,
            VerifyContext = -268435456
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTOAPI_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        internal enum ErrorCode
        {
            BadAlgorithmId = -2146893816,
            BadData = -2146893819,
            KeysetNotDefined = -2146893799,
            MoreData = 0xea,
            NoMoreItems = 0x103,
            ProviderTypeNotDefined = -2146893801,
            Success = 0
        }

        internal enum HashParameter
        {
            AlgorithmId = 1,
            HashSize = 4,
            HashValue = 2,
            None = 0
        }

        internal enum KeyBlobType : byte
        {
            PlainText = 8
        }

        [Flags]
        internal enum KeyFlags
        {
            None,
            Exportable
        }

        internal enum KeyParameter
        {
            IV = 1,
            Mode = 4,
            ModeBits = 5,
            None = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROV_ENUMALGS
        {
            public CapiNative.AlgorithmId aiAlgId;
            public int dwBitLen;
            public int dwNameLen;
            //[FixedBuffer(typeof(byte), 20)]
            unsafe public fixed  byte szName[20];
            // Nested Types
            [StructLayout(LayoutKind.Sequential, Size = 20), UnsafeValueType, CompilerGenerated]
            public struct FixedBuffer0
            {
                public byte FixedElementField;
            }
        }

        internal static class ProviderNames
        {
            // Fields
            public const string MicrosoftEnhancedRsaAes = "Microsoft Enhanced RSA and AES Cryptographic Provider";
            public const string MicrosoftEnhancedRsaAesPrototype = "Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype)";
        }

        internal enum ProviderParameter
        {
            None,
            EnumerateAlgorithms
        }

        [Flags]
        internal enum ProviderParameterFlags
        {
            None,
            RestartEnumeration
        }

        internal enum ProviderType
        {
            None = 0,
            RsaAes = 0x18
        }

        [SecurityCritical(SecurityCriticalScope.Everything), SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods
        {
            // Methods
            [DllImport("mscorwks")]
            public static extern int _AxlPublicKeyBlobToPublicKeyToken(ref CapiNative.CRYPTOAPI_BLOB pCspPublicKeyBlob, out SafeAxlBufferHandle ppwszPublicKeyToken);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool CryptAcquireContext(out SafeCspHandle phProv, string pszContainer, string pszProvider, CapiNative.ProviderType dwProvType, CapiNative.CryptAcquireContextFlags dwFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptCreateHash(SafeCspHandle hProv, CapiNative.AlgorithmId Algid, SafeCapiKeyHandle hKey, int dwFlags, out SafeCapiHashHandle phHash);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptDecrypt(SafeCapiKeyHandle hKey, SafeCapiHashHandle hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, IntPtr pbData, [In, Out] ref int pdwDataLen);
            [return: MarshalAs(UnmanagedType.Bool)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32")]
            public static extern bool CryptDuplicateKey(SafeCapiKeyHandle hKey, IntPtr pdwReserved, int dwFlags, out SafeCapiKeyHandle phKey);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptEncrypt(SafeCapiKeyHandle hKey, SafeCapiHashHandle hHash, [MarshalAs(UnmanagedType.Bool)] bool Final, int dwFlags, IntPtr pbData, [In, Out] ref int pdwDataLen, int dwBufLen);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptExportKey(SafeCapiKeyHandle hKey, SafeCapiKeyHandle hExpKey, int dwBlobType, int dwExportFlags, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbData, [In, Out] ref int pdwDataLen);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptGenKey(SafeCspHandle hProv, CapiNative.AlgorithmId Algid, CapiNative.KeyFlags dwFlags, out SafeCapiKeyHandle phKey);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptGenRandom(SafeCspHandle hProv, int dwLen, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbBuffer);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptGetHashParam(SafeCapiHashHandle hHash, CapiNative.HashParameter dwParam, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbData, [In, Out] ref int pdwDataLen, int dwFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptGetProvParam(SafeCspHandle hProv, CapiNative.ProviderParameter dwParam, IntPtr pbData, [In, Out] ref int pdwDataLen, CapiNative.ProviderParameterFlags dwFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptHashData(SafeCapiHashHandle hHash, [MarshalAs(UnmanagedType.LPArray)] byte[] pbData, int dwDataLen, int dwFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptImportKey(SafeCspHandle hProv, [MarshalAs(UnmanagedType.LPArray)] byte[] pbData, int dwDataLen, SafeCapiKeyHandle hPubKey, CapiNative.KeyFlags dwFlags, out SafeCapiKeyHandle phKey);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("advapi32", SetLastError = true)]
            public static extern bool CryptSetKeyParam(SafeCapiKeyHandle hKey, CapiNative.KeyParameter dwParam, [MarshalAs(UnmanagedType.LPArray)] byte[] pbData, int dwFlags);
        }
    }
}

