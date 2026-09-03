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
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using Microsoft.Win32.SafeHandles;
    #endregion

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeCapiKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Fields
        private IntPtr m_csp;

        // Methods
        private SafeCapiKeyHandle()
            : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32", SetLastError = true)]
        private static extern bool CryptContextAddRef(IntPtr hProv, IntPtr pdwReserved, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32")]
        private static extern bool CryptDestroyKey(IntPtr hKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32")]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        internal SafeCapiKeyHandle Duplicate()
        {
            SafeCapiKeyHandle phKey = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (!CapiNative.UnsafeNativeMethods.CryptDuplicateKey(this, IntPtr.Zero, 0, out phKey))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (((phKey != null) && !phKey.IsInvalid) && (this.m_csp != IntPtr.Zero))
                {
                    phKey.SetCsp(this.m_csp);
                }
            }
            return phKey;
        }

        protected override bool ReleaseHandle()
        {
            bool flag = CryptDestroyKey(base.handle);
            bool flag2 = true;
            if (this.m_csp != IntPtr.Zero)
            {
                flag2 = CryptReleaseContext(this.m_csp, 0);
            }
            return (flag && flag2);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void SetCsp(SafeCspHandle parentCsp)
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                parentCsp.DangerousAddRef(ref success);
                this.SetCsp(parentCsp.DangerousGetHandle());
            }
            finally
            {
                if (success)
                {
                    parentCsp.DangerousRelease();
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void SetCsp(IntPtr parentCsp)
        {
            int hr = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (!CryptContextAddRef(parentCsp, IntPtr.Zero, 0))
                {
                    hr = Marshal.GetLastWin32Error();
                }
                else
                {
                    this.m_csp = parentCsp;
                }
            }
            if (hr != 0)
            {
                throw new CryptographicException(hr);
            }
        }

        // Properties
        internal static SafeCapiKeyHandle InvalidHandle
        {
            get
            {
                SafeCapiKeyHandle handle = new SafeCapiKeyHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }
    }
}
