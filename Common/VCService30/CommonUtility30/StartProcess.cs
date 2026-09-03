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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;



namespace AvePoint.Common
{
    public class StartProcess
    {
        #region P/Invoke Definitions

        #region LogonUserW

        //dwLogonType
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_BATCH = 4;
        //dwLogonProvider
        const int LOGON32_PROVIDER_WINNT50 = 3;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool LogonUserW(
            String lpszUsername,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken
            );
        #endregion

        #region DuplicateTokenEx

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DuplicateTokenEx(IntPtr tokenHandle, int
             dwDesiredAccess,
             ref SECURITY_ATTRIBUTES lpTokenAttributes, int
            SECURITY_IMPERSONATION_LEVEL,
             int TOKEN_TYPE, ref IntPtr dupeTokenHandle);


        // SECURITY_IMPERSONATION_LEVEL
        const int SecurityAnonymous = 0;
        const int SecurityIdentification = 1;
        const int SecurityImpersonation = 2;
        const int SecurityDelegation = 3;

        // TOKEN_TYPE
        const int TokenPrimary = 1;
        const int TokenImpersonation = 2;

        // Access Token constants
        const int MAXIMUM_ALLOWED = 0x10000000;

        #endregion

        #region CreateProcessAsUserW

        //dwCreationFlags
        const int CREATE_NEW_CONSOLE = 0x00000010;
        const int CREATE_NO_WINDOW = 0x08000000;

        //dwFlags of STARTUPINFO
        const int STARTF_USESHOWWINDOW = 0x0000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true,
           CallingConvention = CallingConvention.StdCall)]
        private static extern bool CreateProcessAsUserW(
            IntPtr hToken,
            String lpApplicationName,
            String lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
            );
        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true,
           CallingConvention = CallingConvention.StdCall)]
        public static extern bool CloseHandle(IntPtr hObject);

        #endregion

        [DllImport("Kernel32.dll")]
        static extern bool GetExitCodeProcess(System.IntPtr hProcess, ref uint lpExitCode);

        [DllImport("Kernel32.dll")]
        public static extern uint WaitForSingleObject(System.IntPtr hHandle, uint dwMilliseconds);

        public String UserName = String.Empty;
        public String Password = String.Empty;
        public String Domain = String.Empty;
        public String ProxyPath = String.Empty;
        public String WorkingDir = String.Empty;

        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public StartProcess(string _domain, string _name, string _password, string _proxyPath, string _workingDir)
        {
            Domain = _domain;
            UserName = _name;
            Password = _password;
            ProxyPath = _proxyPath;
            WorkingDir = _workingDir;
        }

        public static void VerifyAccount(string domain, string username, string password, out IntPtr hUserToken, out IntPtr ptoken)
        {
            hUserToken = IntPtr.Zero;
            ptoken = IntPtr.Zero;

            string ver = OSInformation.OSName;
            ver = ver.ToLower();

            // logon as user
            if (OSInformation.OSVersionNumber < 60)
            {
                //the system is not windows 2008

                if (AveEnv.SwitchDefaultLogonType)
                {
                    if (!LogonUserW(username, domain, password,
                               LOGON32_LOGON_BATCH,
                               LOGON32_PROVIDER_DEFAULT,
                               ref hUserToken))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        logger.Error(String.Format("LogonUserW() failed in " + ver + ": error={0}", errorCode));
                        throw new Exception(String.Format("An error occurred while verifying the account information. Error code = {0}", errorCode));
                    }
                    ptoken = hUserToken;

                }
                else
                {
                    if (!LogonUserW(username, domain, password,
                            LOGON32_LOGON_INTERACTIVE,
                            LOGON32_PROVIDER_DEFAULT,
                            ref hUserToken))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        logger.Error(String.Format("LogonUserW() failed in " + ver + ": error={0}", errorCode));
                        throw new Exception(String.Format("An error occurred while verifying the account information. Error code = {0}", errorCode));
                    }
                    ptoken = hUserToken;
                }
            }
            else
            {
                //the system is windows 2008
                //In windows2008 ,we use Batch as default. In windows 2003 ,we use Interactive as default
                if (!AveEnv.SwitchDefaultLogonType)
                {
                    if (!LogonUserW(username, domain, password,
                            LOGON32_LOGON_BATCH,
                            LOGON32_PROVIDER_DEFAULT,
                            ref hUserToken))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        logger.Error(String.Format("LogonUserW() failed in " + ver + ": error={0}", errorCode));
                        throw new Exception(String.Format("An error occurred while verifying the account information. Error code = {0}", errorCode));
                    }
                }
                else
                {
                    if (!LogonUserW(username, domain, password,
                            LOGON32_LOGON_INTERACTIVE,
                            LOGON32_PROVIDER_DEFAULT,
                            ref hUserToken))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        logger.Error(String.Format("LogonUserW() failed in " + ver + ": error={0}", errorCode));
                        throw new Exception(String.Format("An error occurred while verifying the account information. Error code = {0}", errorCode));
                    }
                }

                // Setting security attributes
                SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                sa.bInheritHandle = false;
                sa.nLength = Marshal.SizeOf(sa);
                sa.lpSecurityDescriptor = (IntPtr)0;
                if (!DuplicateTokenEx(hUserToken, MAXIMUM_ALLOWED, ref sa, SecurityImpersonation, TokenPrimary, ref ptoken))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("DuplicateTokenEx() failed: error={0}", errorCode));
                    throw new Exception(String.Format("An error occurred while verifying the account information. Error code = {0}", errorCode));
                }
            }
        }

        private Process StartWithWin32(String sFile, String sArgs)
        {
            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr ptoken = IntPtr.Zero;

            try
            {
                VerifyAccount(Domain, UserName, Password, out hUserToken, out ptoken);
           
                // craete process
                SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
                secAttr.nLength = Marshal.SizeOf(secAttr);

                STARTUPINFO startInfo = new STARTUPINFO();
                startInfo.cb = Marshal.SizeOf(startInfo);
                String sCmd = String.Format("\"{0}\" {1}", sFile, sArgs);
                if (!CreateProcessAsUserW(ptoken, null, sCmd,
                        ref secAttr, ref secAttr,
                        false, CREATE_NEW_CONSOLE, (IntPtr)0, null,
                        ref startInfo, ref processInfo))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("CreateProcessAsUserW() failed: error={0}", errorCode));
                    throw new Exception(String.Format("An error occurred while creating the process. Error code = {0}", errorCode));
                }

                return Process.GetProcessById((int)processInfo.dwProcessId);
            }
            finally
            {
                if (processInfo.hThread != IntPtr.Zero)
                    CloseHandle(processInfo.hThread);

                if (processInfo.hProcess != IntPtr.Zero)
                    CloseHandle(processInfo.hProcess);

                if (hUserToken != IntPtr.Zero)
                    CloseHandle(hUserToken);

                if (ptoken != IntPtr.Zero)
                    CloseHandle(ptoken);
            }
        }

        private int StartWithWin32AndGetExitCode(String sFile, String sArgs)
        {
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr ptoken = IntPtr.Zero;

            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

            try
            {
                VerifyAccount(Domain, UserName, Password, out hUserToken, out ptoken);

                // craete process
                SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
                secAttr.nLength = Marshal.SizeOf(secAttr);

                STARTUPINFO startInfo = new STARTUPINFO();
                startInfo.cb = Marshal.SizeOf(startInfo);
                String sCmd = String.Format("\"{0}\" {1}", sFile, sArgs);
                if (!CreateProcessAsUserW(ptoken, null, sCmd,
                        ref secAttr, ref secAttr,
                        false, CREATE_NEW_CONSOLE, (IntPtr)0, null,
                        ref startInfo, ref processInfo))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("CreateProcessAsUserW() failed: error={0}", errorCode));
                    throw new Exception(String.Format("An error occurred while creating the process. Error code = {0}", errorCode));
                }
                uint mUint = 0;
                WaitForSingleObject(processInfo.hProcess, int.MaxValue);
                GetExitCodeProcess(processInfo.hProcess, ref mUint);
                return (int)mUint;
            }
            finally
            {
                if (processInfo.hThread != IntPtr.Zero)
                    CloseHandle(processInfo.hThread);

                if (processInfo.hProcess != IntPtr.Zero)
                    CloseHandle(processInfo.hProcess);

                if (hUserToken != IntPtr.Zero)
                    CloseHandle(hUserToken);

                if (ptoken != IntPtr.Zero)
                    CloseHandle(ptoken);
            }
        }

        private Process StartFromProxy(String sFile, String sArgs)
        {
            using (StreamWriter sw = new StreamWriter(WorkingDir + "\\start.txt", true, Encoding.UTF8))
            {
                sw.WriteLine();
                sw.WriteLine(UserName);
                sw.WriteLine(Domain);
                sw.WriteLine(Password);
                sw.WriteLine(sFile + " " + sArgs);
                sw.Close();
            }

            string parg = "-r start.txt";
            ProcessStartInfo psi = new ProcessStartInfo(ProxyPath, parg);
            psi.WorkingDirectory = WorkingDir;
            psi.UseShellExecute = false;
            return Process.Start(psi);
        }

        private int StartFromProxyAndGetExitCode(String sFile, String sArgs)
        {
            using (StreamWriter sw = new StreamWriter(WorkingDir + "\\start.txt", true, Encoding.UTF8))
            {
                sw.WriteLine();
                sw.WriteLine(UserName);
                sw.WriteLine(Domain);
                sw.WriteLine(Password);
                sw.WriteLine(sFile + " " + sArgs);
                sw.Close();
            }

            string parg = "-r start.txt";
            ProcessStartInfo psi = new ProcessStartInfo(ProxyPath, parg);
            psi.WorkingDirectory = WorkingDir;
            psi.UseShellExecute = false;
            Process mProcess = null;
            mProcess = Process.Start(psi);
            mProcess.WaitForExit();
            return mProcess.ExitCode;
        }

        private Process StartDirectly(String sFile, String sArgs)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo(sFile, sArgs);
            psInfo.WorkingDirectory = WorkingDir;
            psInfo.UseShellExecute = false;
            return Process.Start(psInfo);
        }

        private int StartDirectlyAndGetExitCode(String sFile, String sArgs)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo(sFile, sArgs);
            psInfo.WorkingDirectory = WorkingDir;
            psInfo.UseShellExecute = false;
            Process mProcess = null;
            mProcess = Process.Start(psInfo);
            mProcess.WaitForExit();
            return mProcess.ExitCode;
        }

        public Process Start(String sFile, String sArgs)
        {
            if (String.IsNullOrEmpty(sFile))
                throw new Exception("Cannot start new process: input command is empty");

            if (!String.IsNullOrEmpty(ProxyPath))
                return StartFromProxy(sFile, sArgs);
            else if (String.IsNullOrEmpty(UserName))
                return StartDirectly(sFile, sArgs);
            else
                return StartWithWin32(sFile, sArgs);
        }

        public int StartAndGetExitCode(String sFile, String sArgs)
        {
            if (String.IsNullOrEmpty(sFile))
                throw new Exception("Cannot start new process: input command is empty");

            if (!String.IsNullOrEmpty(ProxyPath))
                return StartFromProxyAndGetExitCode(sFile, sArgs);
            else if (String.IsNullOrEmpty(UserName))
                return StartDirectlyAndGetExitCode(sFile, sArgs);
            else
                return StartWithWin32AndGetExitCode(sFile, sArgs);
        }

        public static bool VerifyUserByLogon(string domain, string username, string password, ref IntPtr token)
        {
            bool logonSuccessfully = false;

            try
            {
                int logonType = LOGON32_LOGON_INTERACTIVE;

                #region get logonType by configuration and system
                bool isUACEnabled = false;
                try
                {
                    isUACEnabled = OSInformation.UACEnabled;
                }
                catch (Exception ex)
                {
                    logger.Warn("Get Windows Version Failed:" + ex.ToString());
                }

                if (isUACEnabled)
                {
                    if (!AveEnv.SwitchDefaultLogonType)
                    {
                        logonType = LOGON32_LOGON_BATCH;
                    }
                    else
                    {
                        logonType = LOGON32_LOGON_INTERACTIVE;
                    }
                }
                else
                {
                    if (AveEnv.SwitchDefaultLogonType)
                    {
                        logonType = LOGON32_LOGON_BATCH;
                    }
                    else
                    {
                        logonType = LOGON32_LOGON_INTERACTIVE;
                    }
                }
                #endregion

                if (!LogonUserW(username, domain, password, logonType, 0, ref token))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Warn(String.Format("Logon failure: User:{0}\\{1}\tError Code={2}", domain, username, errorCode));
                    logonSuccessfully = false;
                }
                else
                {
                    logonSuccessfully = true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("verify user:{0}\\{1} failed:{2}", domain, username, ex.ToString()));
            }

            return logonSuccessfully;
        }
        public static void CloseHandleByWIN32(IntPtr token)
        {
            try
            {
                if (token != null && token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }
            catch
            {
            }
        }
        public static bool VerifyUserByLogon(string domain, string username, string password)
        {
            bool logonSuccessfully = false;
            IntPtr token = IntPtr.Zero;
            logonSuccessfully = VerifyUserByLogon(domain, username, password, ref token);
            CloseHandleByWIN32(token);
            return logonSuccessfully;
        }
    }

}
