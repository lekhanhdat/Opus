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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using GCommon;
    using GCommon.Utility;
    using SD = System.Diagnostics;

    #endregion

    /// <summary>
    /// Manage the process start operation. 
    /// 
    /// <remarks>
    /// If you provide username and password, we use win32 API,
    /// because there is a limitation documented in the MSDN uri:
    /// http://msdn.microsoft.com/en-us/library/0w4h05yb(v=VS.85).aspx
    /// If the UserName and Password properties of the StartInfo instance are set, the unmanaged 
    /// CreateProcessWithLogonW function is called, which starts the process in a new window even
    /// if the CreateNoWindow property value is true or the WindowStyle property value is Hidden.
    /// </remarks>
    /// 
    /// <remarks>
    /// In previous version of this class, we use start method in LocalSystem account in most cases,
    /// when we start process in a process not in localsystem and the username is not null or empty,
    /// we directly use the process identity to start the process. but this is not always correct,
    /// in some cases, we may start process in a process not running as localsystem and the username 
    /// also is not null or empty. So we add a serial of method StartEx or StartXXXEx for these kind
    /// of cases. please be sure that because we use username as judgment, it not so natural and sliently,
    /// maybe we need restructure the code in future.   
    /// </remarks>
    /// 
    /// </summary>
    public class StartProcess
    {
        static readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static readonly Object syncRoot = new Object();
        static readonly Boolean currentProcessUnderLocalSystem;
        static readonly IProfilerChecker profilerChecker = new ProfilerChecker();

        //dwCreationFlags
        // ReSharper disable InconsistentNaming
        const Int32 CREATE_NEW_CONSOLE = 0x00000010;
        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        const Int32 CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        // ReSharper restore InconsistentNaming

        // SECURITY_IMPERSONATION_LEVEL
        const Int32 SecurityImpersonation = 2;

        // TOKEN_TYPE
        const Int32 TokenPrimary = 1;

        // Access Token constants
        // ReSharper disable InconsistentNaming
        const Int32 MAXIMUM_ALLOWED = 0x10000000;
        // ReSharper restore InconsistentNaming

        readonly String userName = String.Empty;
        readonly String password = String.Empty;
        readonly String domain = String.Empty;
        readonly String workingDir = String.Empty;

        ProcessAction startingAction;
        ProcessAction startedAction;

        /// <summary>
        /// process starting
        /// </summary>
        public event ProcessAction Starting
        {
            add
            {
                lock (syncRoot)
                {
                    this.startingAction += value;
                }
            }
            remove
            {
                lock (syncRoot)
                {
                    this.startingAction -= value;
                }
            }
        }
        /// <summary>
        /// process started
        /// </summary>
        public event ProcessAction Started
        {
            add
            {
                lock (syncRoot)
                {
                    this.startedAction += value;
                }
            }
            remove
            {
                lock (syncRoot)
                {
                    this.startedAction -= value;
                }
            }
        }

        static StartProcess()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity == null) return;
            if (windowsIdentity.User != null)
                currentProcessUnderLocalSystem = windowsIdentity.User.IsWellKnown(WellKnownSidType.LocalSystemSid);
        }

        /// <summary>
        /// if the username and password were not provider,use the .net method to start
        /// </summary>
        /// <param name="domain">domain name</param>
        /// <param name="username">user name</param>
        /// <param name="password">password</param>
        /// <param name="workingDir">process working directory</param>
        public StartProcess(
            String domain,
            String username,
            String password,
            String workingDir)
        {
            this.domain = domain;
            this.userName = username;
            this.password = password;
            this.workingDir = workingDir;
        }

        #region public method

        /// <summary>
        /// start process and get the process object
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <returns></returns>
        public SD.Process Start(String imagePath, String args)
        {
            return Start(imagePath, args, null);
        }

        /// <summary>
        /// start process and get the process object, this method is implemented 
        /// of the process is not under local system, username is not null and 
        /// want to use this username and password to start the process
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <returns></returns>
        public SD.Process StartEx(String imagePath, String args)
        {
            return Start(imagePath, args, null, false);
        }

        /// <summary>
        /// start a process with environment variable
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public SD.Process Start(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            return Start(imagePath, args, environmentVariable, true);
        }

        /// <summary>
        /// start process and get the process object, this method is implemented 
        /// of the process is not under local system, username is not null and 
        /// want to use this username and password to start the process
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public SD.Process StartEx(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            return Start(imagePath, args, environmentVariable, false);
        }
        /// <summary>
        /// start a process with environment variable
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        ///<param name="isDirectlyStart">directly start or not </param>
        /// <returns></returns>
        internal SD.Process Start(String imagePath, String args, Dictionary<String, String> environmentVariable, Boolean isDirectlyStart)
        {
            SD.Process result;
            if (String.IsNullOrEmpty(imagePath))
                throw new Exception("Cannot start new process: input command is empty");
            if ((currentProcessUnderLocalSystem == false || String.IsNullOrEmpty(userName)) && isDirectlyStart)
                result = this.StartDirectly(imagePath, args, environmentVariable);
            else result = this.StartWithWin32(imagePath, args, environmentVariable);
            return result;
        }

        /// <summary>
        /// start process and wait for the exit code
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <returns></returns>
        public Int32 StartAndGetExitCode(String imagePath, String args)
        {
            return this.StartAndGetExitCode(imagePath, args, null);
        }

        /// <summary>
        /// start process and wait for the exit code,this method is implemented 
        /// of the process is not under local system, username is not null and 
        /// want to use this username and password to start the process
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <returns></returns>
        public Int32 StartAndGetExitCodeEx(String imagePath, String args)
        {
            return this.StartAndGetExitCode(imagePath, args, null, false);
        }

        /// <summary>
        /// start process and wait for the exit code
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public int StartAndGetExitCode(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            return this.StartAndGetExitCode(imagePath, args, environmentVariable, true);
        }

        /// <summary>
        /// start process and wait for the exit code,this method is implemented 
        /// of the process is not under local system, username is not null and 
        /// want to use this username and password to start the process
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public int StartAndGetExitCodeEx(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            return this.StartAndGetExitCode(imagePath, args, environmentVariable, false);
        }

        /// <summary>
        /// start process and wait for the exit code
        /// </summary>
        /// <param name="imagePath">exe full path</param>
        /// <param name="args">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        ///<param name="isDirectlyStart">directly start or not </param>
        /// <returns></returns>
        internal int StartAndGetExitCode(String imagePath, String args, Dictionary<String, String> environmentVariable, Boolean isDirectlyStart)
        {
            int result;
            if (String.IsNullOrEmpty(imagePath))
                throw new Exception("Cannot start new process: input command is empty");
            if ((currentProcessUnderLocalSystem == false || String.IsNullOrEmpty(userName)) && isDirectlyStart)
                result = this.StartDirectlyAndWaitExit(imagePath, args, environmentVariable);
            else result = this.StartWithWin32AndWaitExit(imagePath, args, environmentVariable);
            return result;
        }

        public void LoadUserProfile(object sender, ProcessEventArgs args)
        {
            var result = false;
            var profile = new PROFILEINFO();
            using (new AveAppPoolExecuter())
            {
                profile.lpUserName = string.Format("{0}\\{1}", args.Domain, args.Username);
                profile.dwSize = (uint)Marshal.SizeOf(profile);
                result = NativeMethods.LoadUserProfileW(args.UserToken, ref profile);
            }
            var errorcode01 = Marshal.GetLastWin32Error();
        }
        #endregion

        #region protected virtual fire event method

        protected virtual void OnProcessStarting(ProcessEventArgs args)
        {
            var temp = this.startingAction;
            this.FireEvent(temp, args);
        }

        protected virtual void OnProcessStarted(ProcessEventArgs args)
        {
            var temp = this.startedAction;
            this.FireEvent(temp, args);
        }

        void FireEvent(ProcessAction processAction, ProcessEventArgs args)
        {
            if (processAction != null)
            {
                Array.ForEach(processAction.GetInvocationList(), action =>
                {
                    try
                    {
                        action.DynamicInvoke(this, args);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("When invoke a method error, details:[{0}] ", e.ToString());
                    }
                });
            }
        }

        #endregion

        #region private method

        SD.Process StartDirectly(String filePath, String args, Dictionary<String, String> environmentVariable)
        {
            environmentVariable = profilerChecker.Check(filePath, environmentVariable);
            var processStartInfo = new ProcessStartInfo(filePath, args)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (environmentVariable != null)
            {
                foreach (var envEntry in environmentVariable)
                {
                    processStartInfo.EnvironmentVariables[envEntry.Key] = envEntry.Value;
                }
            }
            var processEventArgs = this.BuildEventArgs(String.Format("\"{0}\" {1}", filePath, args), environmentVariable);
            this.OnProcessStarting(processEventArgs);
            var result = SD.Process.Start(processStartInfo);
            this.OnProcessStarted(processEventArgs);
            return result;
        }

        SD.Process StartWithWin32(String imageName, String args, Dictionary<String, String> environmentVariable)
        {
            environmentVariable = profilerChecker.Check(imageName, environmentVariable);
            var userToken = IntPtr.Zero;
            var processInfo = new Win32Native.PROCESS_INFORMATION();
            try
            {
                userToken = CreateUserToken(this.domain, this.userName, this.password);
                // create process
                var gcHandle = new GCHandle();
                var imageFullPath = String.Format("\"{0}\" {1}", imageName, args);
                var secAttr = this.GetProcessSecurityAttributes();
                var creationFlag = this.GetProcessCreateFlags(environmentVariable);
                var startInfo = this.GetProcessStartInfo();
                var envPtr = this.GetProcessEnvironmentVariable(environmentVariable, gcHandle);

                var processEventArgs = this.BuildEventArgs(imageFullPath, environmentVariable);
                processEventArgs.UserToken = userToken;
                processEventArgs.CreationFlags = creationFlag;
                this.OnProcessStarting(processEventArgs);
                if (!Win32Native.CreateProcessAsUserW(
                    userToken,
                    null,
                    imageFullPath,
                    ref secAttr,
                    ref secAttr,
                    false,
                    processEventArgs.CreationFlags,
                    envPtr,
                    null,
                    ref startInfo,
                    ref processInfo))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var errorMessage = this.FormatMessage(errorCode);
                    logger.Error(String.Format("Win32 function CreateProcessAsUserW failed:win32 error code:{0}, detail:{1}", errorCode, errorMessage));
                    throw new Exception(String.Format("An error occurred while creating the process. win32 error code:{0}, detail:{1}", errorCode, errorMessage), new Win32Exception(errorCode));
                }
                this.OnProcessStarted(processEventArgs);

                this.ReleaseGCHandle(gcHandle, envPtr);
                return SD.Process.GetProcessById((int)processInfo.dwProcessId);
            }
            finally
            {
                if (processInfo.hThread != IntPtr.Zero)
                    Win32Native.CloseHandle(processInfo.hThread);
                if (processInfo.hProcess != IntPtr.Zero)
                    Win32Native.CloseHandle(processInfo.hProcess);
                if (userToken != IntPtr.Zero)
                    Win32Native.CloseHandle(userToken);
            }
        }

        /// <summary>
        /// This method is the same as the class of AveErrorCodeConverter
        /// </summary>
        /// <param name="errorCode">the win32 error code defined in winbase.h
        /// you can access the uri :http://msdn.microsoft.com/en-us/library/cc231199(v=prot.10).aspx
        /// for more information.
        /// </param>
        /// <returns></returns>
        String FormatMessage(Int32 errorCode)
        {
            return new Win32Exception(errorCode).Message;
        }

        Int32 StartDirectlyAndWaitExit(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            var process = StartDirectly(imagePath, args, environmentVariable);
            process.WaitForExit();
            return process.ExitCode;
        }

        Int32 StartWithWin32AndWaitExit(String imagePath, String args, Dictionary<String, String> environmentVariable)
        {
            var process = StartWithWin32(imagePath, args, environmentVariable);
            uint exitCode = 0;
            Win32Native.WaitForSingleObject(process.Handle, int.MaxValue);
            Win32Native.GetExitCodeProcess(process.Handle, ref exitCode);
            return (int)exitCode;
        }

        //From MSDN: If UAC is enabled, LogonUserW returns the restricted token for interactive sessions under some conditions.
        //The details of this behavior should be documented.
        //What conditions ??????
        IntPtr CreateUserToken(
            // ReSharper disable ParameterHidesMember
             String domain,
            // ReSharper restore ParameterHidesMember
             String username,
            // ReSharper disable ParameterHidesMember
             String password)
        // ReSharper restore ParameterHidesMember
        {
            int errorCode;
            if (Win32Native.RevertToSelf())
            {
                var token = IntPtr.Zero;
                var logonType = Win32Native.LOGON32_LOGON_BATCH;
                if (!OSInformation.UACEnabled)
                {
                    logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                }
                var startProcessForceLogonType = ConfigurationManager.AppSettings["startProcessForceLogonType"];
                if (!string.IsNullOrEmpty(startProcessForceLogonType))
                {
                    if (string.Compare(startProcessForceLogonType, "batch", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_BATCH;
                    }
                    if (string.Compare(startProcessForceLogonType, "interactive", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                    }
                }
                if (!Win32Native.LogonUserW(username, domain, password, logonType, Win32Native.LOGON32_PROVIDER_DEFAULT, ref token))
                {
                    errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("LogonUserW() failed: error={0}", errorCode));
                    throw new Win32Exception(errorCode);
                }
                if (!OSInformation.UACEnabled) return token;

                ////The DuplicateTokenEx function allows you to create a primary token that you can use in the CreateProcessAsUser function.
                ////Note that the DuplicateToken function can create only impersonation tokens, which are not valid for CreateProcessAsUser.
                var duplicateToken = IntPtr.Zero;
                var sa = new Win32Native.SECURITY_ATTRIBUTES { bInheritHandle = false };
                sa.nLength = Marshal.SizeOf(sa);
                sa.lpSecurityDescriptor = (IntPtr)0;
                if (!Win32Native.DuplicateTokenEx(token, MAXIMUM_ALLOWED, ref sa, SecurityImpersonation, TokenPrimary, ref duplicateToken))
                {
                    errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("DuplicateTokenEx() failed: error={0}", errorCode));
                    throw new Win32Exception(errorCode);
                }
                //close the token created by LogonUserW
                if (token != IntPtr.Zero)
                {
                    Win32Native.CloseHandle(token);
                    // ReSharper disable RedundantAssignment
                    token = IntPtr.Zero;
                    // ReSharper restore RedundantAssignment
                }
                return duplicateToken;
            }
            errorCode = Marshal.GetLastWin32Error();
            logger.Error(String.Format("RevertToSelf() failed: error={0}", errorCode));
            throw new Win32Exception(errorCode);
        }

        Int32 GetProcessCreateFlags(Dictionary<String, String> environmentVariable)
        {
            var result = CREATE_NEW_CONSOLE;
            if (environmentVariable != null)
                result |= CREATE_UNICODE_ENVIRONMENT;
            return result;
        }

        Win32Native.STARTUPINFO GetProcessStartInfo()
        {
            var startInfo = new Win32Native.STARTUPINFO();
            startInfo.cb = Marshal.SizeOf(startInfo);
            return startInfo;
        }

        Win32Native.SECURITY_ATTRIBUTES GetProcessSecurityAttributes()
        {
            var secAttr = new Win32Native.SECURITY_ATTRIBUTES();
            secAttr.nLength = Marshal.SizeOf(secAttr);
            return secAttr;
        }

        // ReSharper disable RedundantAssignment
        IntPtr GetProcessEnvironmentVariable(Dictionary<String, String> environmentVariable, GCHandle environmentHandle)
        // ReSharper restore RedundantAssignment
        {
            var result = IntPtr.Zero;
            if (environmentVariable != null)
            {
                environmentHandle = GCHandle.Alloc(ToByteArray(environmentVariable), GCHandleType.Pinned);
                result = environmentHandle.AddrOfPinnedObject();
            }
            return result;
        }

        Byte[] ToByteArray(Dictionary<String, String> environmentVariable)
        {
            var builder = new StringBuilder();
            foreach (var envEntity in environmentVariable)
            {
                builder.Append(envEntity.Key);
                builder.Append('=');
                builder.Append(envEntity.Value);
                builder.Append('\0');
            }
            builder.Append('\0');
            var result = Encoding.Unicode.GetBytes(builder.ToString());
            if (result.Length > 0xffff)
                throw new InvalidOperationException();
            return result;
        }

        void ReleaseGCHandle(GCHandle handle, IntPtr environmentAllcationHandle)
        {
            if (environmentAllcationHandle != IntPtr.Zero && handle.IsAllocated)
            {
                handle.Free();
            }
        }

        ProcessEventArgs BuildEventArgs(String imageName, Dictionary<String, String> environmentVariable)
        {
            return new ProcessEventArgs
            {
                Domain = this.domain,
                Password = this.password,
                Username = this.userName,
                WorkingDirectory = this.workingDir,
                EnvironmentVariables = environmentVariable,
                ImageName = imageName
            };
        }

        #endregion
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct PROFILEINFO
    {

        /// DWORD->unsigned int
        public uint dwSize;

        /// DWORD->unsigned int
        public uint dwFlags;

        /// LPTSTR->LPWSTR->WCHAR*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        public string lpUserName;

        /// LPTSTR->LPWSTR->WCHAR*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        public string lpProfilePath;

        /// LPTSTR->LPWSTR->WCHAR*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        public string lpDefaultPath;

        /// LPTSTR->LPWSTR->WCHAR*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        public string lpServerName;

        /// LPTSTR->LPWSTR->WCHAR*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        public string lpPolicyPath;

        /// HANDLE->void*
        public System.IntPtr hProfile;
    }

    public partial class NativeMethods
    {
        public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
        public const string SE_AUDIT_NAME = "SeAuditPrivilege";
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
        public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
        public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";
        public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
        public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
        public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
        public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
        public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
        public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
        public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
        public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
        public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
        public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
        public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        public const string SE_TCB_NAME = "SeTcbPrivilege";
        public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
        public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
        public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";


        public const int TOKEN_QUERY = 0x00000008;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        /// Return Type: BOOL->int
        ///hToken: HANDLE->void*
        ///lpProfileInfo: LPPROFILEINFO->_PROFILEINFO*
        [System.Runtime.InteropServices.DllImportAttribute("Userenv.dll")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool LoadUserProfile([System.Runtime.InteropServices.InAttribute()] System.IntPtr hToken, ref PROFILEINFO lpProfileInfo);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LoadUserProfileW(IntPtr hToken, ref PROFILEINFO lpProfileInfo);

        [DllImport("Netapi32.dll", SetLastError = true)]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("Netapi32.dll")]
        public extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);
    }
}