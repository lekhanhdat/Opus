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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using AvePoint.Common;
using System.Net;
using System.Net.Sockets;

namespace AvePoint.GCommon.Utility
{
    public class Impersonator
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private static ServiceEnvironment mEnv = new ServiceEnvironment(true);

        //Add string ".ipv6-literal.net" to IPv6address if needed;
        private const string AddRare = ".ipv6-literal.net";

        [DllImport("mpr.dll")]
        public static extern int WNetAddConnection2W(
            [MarshalAs(UnmanagedType.LPArray)] NETRESOURCEW[] lpNetResource,
            [MarshalAs(UnmanagedType.LPWStr)] string lpPassword,
            [MarshalAs(UnmanagedType.LPWStr)] string UserName,
            int dwFlags);

        [DllImport("mpr.dll")]
        public static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce);
        public static int SetupNetShare(string remotepath, string username, string password)
        {

            CancleNetShare(remotepath, 0, true);//DOC-39895å–æ¶ˆé“¾æŽ¥å¦åˆ™åˆ‡æ¢å¦ä¸€ä¸ªç”¨æˆ·çš„testçš„æ—¶å€™ä¸é€šè¿‡ï¼Œè¿”å›?219çš„é”™è¯¯ã€?
            NETRESOURCEW[] n = new NETRESOURCEW[1];
            n[0] = new NETRESOURCEW();

            //remotepath = ConvertIPv6AddressForNetShare(remotepath);

            n[0].dwType = 1;
            int dwFlags = 1;
            n[0].lpLocalName = null; //"X:";
            n[0].lpRemoteName = remotepath;
            n[0].lpProvider = null;

            mLog.Debug(n[0].ToString());

            int res = WNetAddConnection2W(n, password, username, dwFlags);
            return res;
        }

        //public static string ConvertIPv6AddressForNetShare(string remotepath)
        //{
        //    string HostPath;
        //    string oldPath = remotepath;
        //    try
        //    {
        //        //If The DocAve Is Not Enabled IPv6 Or The Remote Path Not Starts With "\\\\". Not Change And Return
        //        if (!AveEnv.IsIPv6 || !remotepath.StartsWith("\\\\"))
        //        {
        //            return remotepath;
        //        }
        //        if (remotepath.IndexOf(":") == -1)
        //        {
        //            //Simply It Is String Between "\\" And "\" Of  remotepath;
        //            HostPath = GetHostPath(remotepath);
        //            if (CheckIfAnIPv6Address(HostPath))
        //            {
        //                AddRareForIPv6(ref remotepath, HostPath);
        //            }
        //            return remotepath;
        //        }

        //        remotepath = remotepath.Replace(":", "-");
        //        HostPath = GetHostPath(remotepath);

        //        AddRareForIPv6(ref remotepath, HostPath);

        //        return remotepath;
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Warn("An error occurred while converting the ipv6 address:" + ex.ToString() + " address:" + remotepath);
        //        return oldPath;
        //    }
        //}

        private static string GetHostPath(string remotepath)
        {
            const int BeginI = 2;// For remotepath is like "\\hostname\sharefolder"
            int DestInt = 0;
            DestInt = remotepath.IndexOf("\\", BeginI);
            if (DestInt - BeginI > 0)
            {
                return remotepath.Substring(BeginI, DestInt - BeginI);
            }
            else
            {
                return remotepath.Substring(BeginI);
            }
        }

        private static bool CheckIfAnIPv6Address(string HostPath)
        {
            //If The "HostPath" Is IPv6 Return True... If The "HostPath" is Not IPAddress String But HostName Like "08x64Farm". Will Return False 
            //And Not To Add Rare String ".ipv6-literal.net",  This Is Right.
            if (HostPath.IndexOf("-") == -1)
            {
                return false;
            }
            //If HostPath Like "AAA-BBB" Not Is A IPAddress String
            else
            {
                if (HostPath.EndsWith(AddRare, StringComparison.OrdinalIgnoreCase))
                {
                    HostPath = HostPath.Replace(AddRare, null);
                }
                HostPath = HostPath.Replace("-", ":");
                try
                {
                    IPAddress[] TemIP = Dns.GetHostAddresses(HostPath);
                    foreach (IPAddress ip in TemIP)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    mLog.Warn("An error occurred while gettting host addresses:" + ex.ToString());
                    return false;
                }
            }
        }

        private static void AddRareForIPv6(ref string remotepath, string HostPath)
        {
            if (HostPath.EndsWith(AddRare, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            remotepath = remotepath.Replace(HostPath, HostPath + AddRare);
            return;
        }

        //add by sfang for cancle net share
        public static int CancleNetShare(string remotepath, int dwFlags, bool fForce)
        {
            //remotepath = ConvertIPv6AddressForNetShare(remotepath);
            int res = WNetCancelConnection2(remotepath, dwFlags, fForce);//æ–­å¼€IPCè¿žæŽ¥
            return res;
        }


        // private members for holding domain user account credentials
        public string username = String.Empty;
        public string password = String.Empty;
        public string domain = String.Empty;
        // this will hold the security context for reverting back to the client after impersonation operations are complete
        private WindowsImpersonationContext impersonationContext = null;

        // disable instantiation via default constructor
        private Impersonator()
        { }

        public Impersonator(string username, string domain, string password)
        {
            // set the properties used for domain user account
            this.username = username;
            this.domain = domain;
            this.password = password;
        }

        private WindowsIdentity Logon()
        {
            IntPtr handle = new IntPtr(0);
            handle = IntPtr.Zero;

            const int LOGON32_LOGON_INTERACTIVE = 2;
            const int LOGON32_PROVIDER_DEFAULT = 0;

            // attempt to authenticate domain user account
            bool logonSucceeded = StartProcess.VerifyUserByLogon(domain, username, password, ref handle);//LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref handle);

            if (!logonSucceeded)
            {
                // if the logon failed, get the error code and throw an exception
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception("User logon failed. Error Number: " + errorCode);
            }

            // if logon succeeds, create a WindowsIdentity instance
            WindowsIdentity winIdentity = new WindowsIdentity(handle);

            // close the open handle to the authenticated account
            CloseHandle(handle);

            return winIdentity;
        }

        public void Impersonate()
        {
            // authenticates the domain user account and begins impersonating it
            impersonationContext = Logon().Impersonate();
        }

        public void Undo()
        {
            // rever back to original security context which was store in the WindowsImpersonationContext instance
            impersonationContext.Undo();
        }

        public int CallProcessAsUser(string command, string mpath, int waittime)
        {
            IntPtr handle = new IntPtr(0);
            handle = IntPtr.Zero;

            const int LOGON32_LOGON_INTERACTIVE = 2;
            const int LOGON32_PROVIDER_DEFAULT = 3;

            // attempt to authenticate domain user account
            bool logonSucceeded = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref handle);

            if (!logonSucceeded)
            {
                // if the logon failed, get the error code and throw an exception
                int errorCode = Marshal.GetLastWin32Error();
                //				log.WriteEntry("User logon failed. Error Number: " + errorCode);
                return errorCode;
            }

            StartupInfo si = new StartupInfo();
            si.cb = Marshal.SizeOf(typeof(StartupInfo));
            ProcessInfo pi = new ProcessInfo();

            if (CreateProcessAsUserW(handle, null, command, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, mpath,
                ref si, out pi))
            {
                //				log.WriteEntry("AAA Call successfully.");
                if (waittime > 0)
                {
                    int k = WaitForSingleObject(pi.hProcess, waittime);
                    if (k != 0)
                        return 1;
                }
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
                return 0;
            }
            else
            {
                //				log.WriteEntry("AAA Call failed.");
                return Marshal.GetLastWin32Error();
            }

        }

        public int CallProcessWithUser(string command, string mpath)
        {
            StartupInfo si = new StartupInfo();
            si.cb = Marshal.SizeOf(typeof(StartupInfo));
            ProcessInfo pi = new ProcessInfo();


            if (CreateProcessWithLogonW(username, domain, password,
                LogonFlags.LOGON_WITH_PROFILE,
                command, null,
                0, IntPtr.Zero, mpath,
                ref si, out pi))
            {
                //mLog.Log(AveLogLevel.INFO, "EnvUtility000630", "");
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
                return 0;
            }
            else
            {
                //mLog.Log(AveLogLevel.INFO, "EnvUtility000637", "");
                return Marshal.GetLastWin32Error();
            }
        }

        [Flags]
        enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        [Flags]
        enum CreationFlags
        {
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ProcessInfo
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct StartupInfo
        {
            public int cb;
            public string reserved1;
            public string desktop;
            public string title;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public short reserved2;
            public int reserved3;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool CreateProcessWithLogonW(
            string principal,
            string authority,
            string password,
            LogonFlags logonFlags,
            string appName,
            string cmdLine,
            CreationFlags creationFlags,
            IntPtr environmentBlock,
            string currentDirectory,
            ref StartupInfo startupInfo,
            out ProcessInfo processInfo);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool CreateProcessAsUserW(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            out ProcessInfo lpProcessInformation
            );


        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr h);

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NETRESOURCEW
    {
        public int dwScope;
        public int dwType;
        public int dwDisplayType;
        public int dwUsage;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpLocalName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpRemoteName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpComment;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpProvider;
        public override String ToString()
        {
            String str = "LocalName: " + lpLocalName + " RemoteName: " + lpRemoteName
                + " Comment: " + lpComment + " lpProvider: " + lpProvider;
            return (str);
        }
    }

}
