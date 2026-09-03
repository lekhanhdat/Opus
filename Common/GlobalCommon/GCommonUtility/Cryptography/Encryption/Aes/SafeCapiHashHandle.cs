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
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32.SafeHandles;
    #endregion

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeCapiHashHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Methods
        private SafeCapiHashHandle()
            : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32")]
        private static extern bool CryptDestroyHash(IntPtr hHash);
        protected override bool ReleaseHandle()
        {
            return CryptDestroyHash(base.handle);
        }

        // Properties
        public static SafeCapiHashHandle InvalidHandle
        {
            get
            {
                SafeCapiHashHandle handle = new SafeCapiHashHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }
    }
}
