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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AutoInstallationCommon.Utility
{
    public sealed class AveImpersonator : IDisposable
    {
        private static readonly bool cacheMode = false;
        private static readonly Dictionary<string, IntPtr> cachedUserTokens = new Dictionary<string, IntPtr>();
        private readonly string domainName;
        private readonly bool networkLogon;
        private readonly string password;
        private readonly string userName;
        private readonly bool win32Mode;
        private WindowsIdentity identity;

        private IntPtr userToken;
        private WindowsImpersonationContext wic;

        public AveImpersonator(string domain, string user, string pwd, bool networkLogon = true,
            bool usingWin32Mode = false)
        {
            domainName = domain;
            userName = user;
            password = pwd;
            this.networkLogon = networkLogon;
            win32Mode = usingWin32Mode;
        }

        public void Dispose()
        {
            Undo();
        }

        public void Impersonate()
        {
            if (!string.IsNullOrEmpty(userName))
            {
                if (cachedUserTokens.ContainsKey(domainName + "\\" + userName))
                    userToken = cachedUserTokens[domainName + "\\" + userName];
                else
                    userToken = CreateUserToken(domainName, userName, password, networkLogon);
                if (!win32Mode)
                {
                    identity = new WindowsIdentity(userToken);
                    if (cacheMode)
                    {
                        if (!cachedUserTokens.ContainsKey(domainName + "\\" + userName))
                            cachedUserTokens.Add(domainName + "\\" + userName, userToken);
                    }
                    else
                    {
                        Win32Native.CloseHandle(userToken); // WindowsIdentity Constructor already duplicate the handle
                    }

                    wic = identity.Impersonate();
                }
                else
                {
                    Win32Native.ImpersonateLoggedOnUser(userToken);
                }
            }
        }

        public void Undo()
        {
            if (!win32Mode)
            {
                if (wic != null)
                {
                    wic.Dispose();
                    wic = null;
                }

                if (identity != null)
                {
                    identity.Dispose();
                    identity = null;
                }
            }
            else
            {
                Win32Native.RevertToSelf();
                if (cacheMode)
                {
                    if (!cachedUserTokens.ContainsKey(domainName + "\\" + userName))
                        cachedUserTokens.Add(domainName + "\\" + userName, userToken);
                }
                else
                {
                    Win32Native.CloseHandle(userToken);
                    userToken = IntPtr.Zero;
                }
            }
        }

        private IntPtr CreateUserToken(string domainName, string username, string password, bool networkLogon)
        {
            if (Win32Native.RevertToSelf())
            {
                var token = IntPtr.Zero;
                if (!networkLogon)
                {
                    if (!Win32Native.LogonUserW(username, domainName, password, Win32Native.LOGON32_LOGON_INTERACTIVE,
                        Win32Native.LOGON32_PROVIDER_DEFAULT, ref token))
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode);
                    }

                    //5.x, we use Batch logon for win08, interactive logon for win03; but we can't find information about this on msdn or google even it works
                    //Win32Native.LogonUserW(username, domainName, password, Win32Native.LOGON32_LOGON_BATCH, Win32Native.LOGON32_PROVIDER_DEFAULT, ref token)
                }
                else
                {
                    //This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
                    if (!Win32Native.LogonUserW(username, domainName, password,
                        Win32Native.LOGON32_LOGON_NEW_CREDENTIALS, Win32Native.LOGON32_PROVIDER_WINNT50, ref token))
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode);
                    }
                }

                return token;

                //// we don't need Deplicate the token, using the token LogonUserW created is ok.
                ////The DuplicateTokenEx function allows you to create a primary token that you can use in the CreateProcessAsUser function. 
                ////Note that the DuplicateToken function can create only impersonation tokens, which are not valid for CreateProcessAsUser.   
                //IntPtr duplicateToken = IntPtr.Zero;
                //if (!Win32Native.DuplicateToken(token, Win32Native.SECURITY_IMPERSONATION_LEVEL_IMPERSONATION, ref duplicateToken))
                //{
                //    throw new Win32Exception(Marshal.GetLastWin32Error());
                //}
                //else
                //{
                //    //close the token created by LogonUserW
                //    if (token != IntPtr.Zero)
                //    {
                //        Win32Native.CloseHandle(token);
                //        token = IntPtr.Zero;
                //    }
                //}
                //return duplicateToken;
            }

            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }
        }
    }
}