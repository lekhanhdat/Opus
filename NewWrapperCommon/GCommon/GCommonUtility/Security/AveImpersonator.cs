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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    #endregion

    /// <summary>
    /// <example>根据模拟访问本地还是远程，可以按照下面的例子来做
    /// <code>
    /// using(AveImpersonator ai=new AveImpersonator(domain,username,password))
    /// {
    ///     ai.Impersonate();
    ///     ......do something locally......
    /// }
    /// OR
    /// using(AveImpersonator ai=new AveImpersonator(domain,username,password,true))
    /// {
    ///     ai.Impersonate();
    ///     ......do something remotely (eg: operate net share)........
    /// }
    /// </code>
    ///
    /// <remarks>
    ///    Make sure the AveImpernator never throw a exception in dispose method.
    ///
    ///    ******
    ///
    ///    As a matter of fact, .net framework will suppress the security context and
    ///    Windows Identity flow to the sub thread, That is, when you use impersonator
    ///    to change the thread identity, and then you start a thread to do something,
    ///    the security context of the newly created thread is the impersonated identity.
    ///    if you do something in the newly created thread, especially you check the
    ///    current security or windows current identity, at this point, the impersonator
    ///    has finish and dispose itself,  so the security context and windows identity
    ///    in the other thread will be disposed too, the core point is the handle is
    ///    platform level not the thread level.  Currently, there is no final solution to
    ///    solve the problem.
    ///
    ///    At docave level, you should not use the impersonator to check the security
    ///    and windows identity in another thread.
    ///
    ///    We find out a situation that match the condition in docave development, here
    ///    is the situation:
    ///
    ///    The docave will check the sqlserver use ado.net and windows authentication,
    ///    default ado.net will use sqlconnection class to check if can connect to
    ///    sqlserver or not,
    ///
    ///    It just turned out that the ADO.NET Connection Pooling facilities will create
    ///    background timers to handle cleanup of the connection pools the first time you
    ///    get a new connection from the pool.The really nasty part, however, was that
    ///    the timer is created with a random interval, which meant you could never be
    ///    sure when the process would crash: Sometimes it would happen just a few seconds
    ///    after disposing the WindowsIdentity instance, sometimes several minutes later.
    ///    This made figuring out the problem a lot harder.
    ///
    ///    the main point is, the sqlconnection will maintain a connection pool in
    ///    underlying connection and use a Timer to perform the  check.  unfortunately,
    ///    the timer will perform a impersonate. that is the problem.
    ///
    ///    sometimes, you will get the exception like this :
    ///
    ///    at System.Security.Principal.Win32.ImpersonateLoggedOnUser(SafeTokenHandle hToken)
    ///    at System.Security.Principal.WindowsIdentity.SafeImpersonate(SafeTokenHandle userToken
    ///         , WindowsIdentity wi, StackCrawlMark& stackMark)
    ///    at System.Security.SecurityContext.SetSecurityContext(SecurityContext sc, SecurityContext
    ///         prevSecurityContext, StackCrawlMark& stackMark)
    ///    at System.Threading.ExecutionContext.SetExecutionContext(ExecutionContext executionContext)
    ///    at System.Threading.ExecutionContext.runTryCode(Object userData)
    ///    at System.Runtime.CompilerServices.RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(TryCode
    ///         code, CleanupCode backoutCode, Object userData)
    ///    at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback
    ///         callback, Object state)
    ///    at System.Threading._TimerCallback.PerformTimerCallback(Object state)
    ///
    ///    This is make the condition is constant condition that if you use impersonator
    ///    facilities and the begin a timer in it's context, you definitely will get a
    ///    unhandled exception throw by the timer's thread.  As a temporary solution,
    ///    you can use the Application unhandleExceptionHandler event handler to catch
    ///    the exception , also in the ADO.net connection scenarios, we can set the connection
    ///    pooling status to no, this settings will disable the ado.net default connection
    ///    pool behavior. or you can use the impersonator win32 mode to avoid the .net
    ///    to check security context and windows identity.
    ///
    ///    Here we list the code that may crash you process when you use the impersonator.
    ///
    ///    <code>
    ///    using(AveImpersonator ai=new AveImpersonator(domain,username,password,true))
    ///    {
    ///        ai.Impersonate();
    ///        Timer timer = new Timer( (state) => {}, null, 3000, 3000);
    ///    }
    ///    Console.WriteLine("Wait for the crash!");
    ///    Console.ReadLine();
    ///    </code>
    ///
    ///    2012/8/1  we get a solution for this issue base on the following articles
    ///    http://msdn.microsoft.com/en-us/magazine/cc793966.aspx
    ///    http://msdn.microsoft.com/en-us/library/ms228965(v=vs.90).aspx
    ///
    ///    The issue which make the process crash is the unhandled exception in a thread
    ///    which start by .net framework, we change the clr's legacy unhandled exception
    ///    policy to avoid issue
    ///
    /// </remarks>
    /// </example>
    /// </summary>
    public sealed class AveImpersonator : IDisposable
    {
        /**
         *
         * remove logger, since not all the programs using this class have Log4Net configured
         *
        **/
        //static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        String domainName;
        String userName;
        String password;
        Boolean networkLogon;
        Boolean win32Mode;

        IntPtr userToken;
        WindowsIdentity identity;
        WindowsImpersonationContext wic;

        String cachedUserTokenKey;
        static Boolean cacheMode = true;
        static Dictionary<String, IntPtr> cachedUserTokens = new Dictionary<String, IntPtr>();

        public AveImpersonator(
            String userUPN,
            String pwd,
            Boolean networkLogon = true,
            Boolean usingWin32Mode = false)
            : this(String.Empty, userUPN, pwd, networkLogon, usingWin32Mode) { }

        public AveImpersonator(
            String domain,
            String user,
            String pwd,
            Boolean networkLogon = true,
            Boolean usingWin32Mode = false)
        {
            //Set the domain to string.empty string,which you can use UPN style user name to impersonate
            this.domainName = domain ?? String.Empty;
            this.userName = user;
            this.password = pwd;
            this.networkLogon = networkLogon;
            win32Mode = usingWin32Mode;
            cachedUserTokenKey = this.GetUserTokenCachedKey(domainName, userName, password, networkLogon);
        }

        public void Impersonate()
        {
            if (!String.IsNullOrEmpty(userName))
            {
                if (cachedUserTokens.ContainsKey(cachedUserTokenKey))
                    userToken = cachedUserTokens[cachedUserTokenKey];
                else userToken = CreateUserToken(this.domainName, this.userName, this.password, this.networkLogon);

                if (!win32Mode)
                {
                    identity = new WindowsIdentity(userToken);
                    if (cacheMode)
                    {
                        lock (cachedUserTokens)
                        {
                            if (!cachedUserTokens.ContainsKey(cachedUserTokenKey))
                            {
                                if (userToken != IntPtr.Zero)
                                {
                                    cachedUserTokens.Add(cachedUserTokenKey, userToken);
                                }
                            }
                        }
                    }
                    else
                    {
                        Win32Native.CloseHandle(userToken);// WindowsIdentity Constructor already duplicate the handle
                    }
                    wic = identity.Impersonate();

                    var lastWin32ErrorCode = Win32Native.GetLastError();
                    if (lastWin32ErrorCode != 0)
                    {
                        var messge = AveErrorCodeConverter.GetSystemMessage(lastWin32ErrorCode);
                        Trace.WriteLine(messge);
                        //throw new Win32Exception(lastWin32ErrorCode);
                    }

                }
                else
                {
                    Win32Native.ImpersonateLoggedOnUser(userToken);
                    var lastWin32ErrorCode = Win32Native.GetLastError();
                    if (lastWin32ErrorCode != 0)
                    {
                        var messge = AveErrorCodeConverter.GetSystemMessage(lastWin32ErrorCode);
                        Trace.WriteLine(messge);
                        //throw new Win32Exception(lastWin32ErrorCode);

                    }

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
                Win32Native.RevertToSelf();
            }
            else
            {
                Win32Native.RevertToSelf();
                if (cacheMode)
                {
                    lock (cachedUserTokens)
                    {
                        if (!cachedUserTokens.ContainsKey(cachedUserTokenKey))
                        {
                            if (userToken != IntPtr.Zero)
                            {
                                cachedUserTokens.Add(cachedUserTokenKey, userToken);
                            }
                        }
                    }
                }
                else
                {
                    if (userToken != IntPtr.Zero)
                    {
                        Win32Native.CloseHandle(userToken);
                        userToken = IntPtr.Zero;
                    }
                }
            }
        }

        public int GetLastError()
        {
            return Win32Native.GetLastError();
        }

        public static int GetLastErrCode()
        {
            return Win32Native.GetLastError();
        }

        /// <summary>
        /// In order to make sure that AveImpersonator which in using statement never throw exception,
        /// I change the AveImpernator dispose method, add try catch logic, this is a very common design
        /// of IDisposable interface , when you AveImpersonator in using statement
        /// </summary>
        public void Dispose()
        {
            try { this.Undo(); }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        String GetUserTokenCachedKey(String domainName, String username, String password, bool networkLogon)
        {
            var pwdHash = HashCodeHelper.ToMD5HashCode(password ?? String.Empty);
            return String.IsNullOrEmpty(domainName) ? userName + ":" + pwdHash + "?IsNetWorkLogon=" + networkLogon : domainName + "\\" + userName + ":" + pwdHash + "?IsNetWorkLogon=" + networkLogon;
        }

        private IntPtr CreateUserToken(string domainName, string username, string password, bool networkLogon)
        {
            if (Win32Native.RevertToSelf())
            {
                IntPtr token = IntPtr.Zero;
                if (!networkLogon)
                {
                    int logonType = Win32Native.LOGON32_LOGON_BATCH;
                    if (!OSInformation.UACEnabled)
                    {
                        logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                    }
                    string verifyUserAccountForceLogonType = System.Configuration.ConfigurationManager.AppSettings["verifyUserAccountForceLogonType"];
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
                    if (!Win32Native.LogonUserW(username, domainName, password, logonType, Win32Native.LOGON32_PROVIDER_DEFAULT, ref token))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        //logger.Error(String.Format("LogonUserW() failed: error={0}", errorCode));
                        throw new Win32Exception(errorCode);
                    }
                }
                else
                {
                    //This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
                    if (!Win32Native.LogonUser(username, domainName, password, Win32Native.LOGON32_LOGON_NEW_CREDENTIALS, Win32Native.LOGON32_PROVIDER_WINNT50, ref token))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        //logger.Error(String.Format("LogonUserW() failed: error={0}", errorCode));
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
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                //logger.Error(String.Format("RevertToSelf() failed: error={0}", errorCode));
                throw new Win32Exception(errorCode);
            }
        }
    }
}