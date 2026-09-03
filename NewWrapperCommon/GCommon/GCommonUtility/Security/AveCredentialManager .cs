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



namespace AvePoint.GCommon
{
    #region using directives
    using System.ComponentModel;
    using System.DirectoryServices;
    using System;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using AvePoint.GCommon.Utility;
    using System.DirectoryServices.AccountManagement;
    using System.Configuration;
    #endregion

    /// <summary>
    /// 此类用来提供对用户名密码等的管理操作
    /// </summary>
    public class AveCredentialManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///验证用户名密码，此方法只能验证域内用户，是通过判断可否登录到本机来进行验证
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool VerifyUserAccountByLogon(string domain, string username, string password)
        {
            var result = VerifyUserAccountByLogonEx(domain, username, password);

            return result.Item1;
        }

        public static Tuple<bool, int> VerifyUserAccountByLogonEx(string domain, string username, string password)
        {
            IntPtr token = IntPtr.Zero;
            try
            {
                int logonType = Win32Native.LOGON32_LOGON_BATCH;
                if (!OSInformation.UACEnabled)
                {
                    logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                }
                string verifyUserAccountForceLogonType = ConfigurationManager.AppSettings["verifyUserAccountForceLogonType"];
                if (!string.IsNullOrEmpty(verifyUserAccountForceLogonType))
                {
                    if (string.Compare(verifyUserAccountForceLogonType, "batch", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_BATCH;
                    }
                    if (string.Compare(verifyUserAccountForceLogonType, "interactive", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                    }
                }
                if (!Win32Native.LogonUserW(username, domain, password, logonType, Win32Native.LOGON32_PROVIDER_DEFAULT, ref token))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Warn(String.Format("Logon failure: User:{0}\\{1}\tError Code={2}", domain, username, errorCode));
                    return new Tuple<bool, int>(false, errorCode);
                }
                return new Tuple<bool, int>(true, 0);
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    Win32Native.CloseHandle(token);
                    token = IntPtr.Zero;
                }
            }
        }

        public static bool VerifyDomainUserAccount(string domain, string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
            {
                // validate the credentials 
                return pc.ValidateCredentials(username, password);
            }
        }

        public static bool VerifyMachineUserAccount(string machineName, string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, machineName))
            {
                // validate the credentials 
                return pc.ValidateCredentials(username, password);
            }
        }

    }
}
