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
    internal sealed class SafeCspHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Methods
        private SafeCspHandle()
            : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32", SetLastError = true)]
        private static extern bool CryptContextAddRef(SafeCspHandle hProv, IntPtr pdwReserved, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32")]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        public SafeCspHandle Duplicate()
        {
            SafeCspHandle handle2;
            
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = base.DangerousGetHandle();
                int hr = 0;
                SafeCspHandle handle = new SafeCspHandle();
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (!CryptContextAddRef(this, IntPtr.Zero, 0))
                    {
                        hr = Marshal.GetLastWin32Error();
                    }
                    else
                    {
                        handle.SetHandle(ptr);
                    }
                }
                if (hr != 0)
                {
                    handle.Dispose();
                    throw new CryptographicException(hr);
                }
                handle2 = handle;
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return handle2;
        }

        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(base.handle, 0);
        }
    }
}
