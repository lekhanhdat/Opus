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




namespace AvePoint.Hybrid.Utility.OperationSystem
{
    #region using directives
    using AvePoint.Hybrid.Utility.Native;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using SD = System.Diagnostics;

    #endregion

    /// <summary>
    /// Manage the process start operation
    /// If you provide username and password, we use win32 API, because there is a limitation documented in the MSDN uri:
    /// http://msdn.microsoft.com/en-us/library/0w4h05yb(v=VS.85).aspx
    /// If the UserName and Password properties of the StartInfo instance are set, the unmanaged CreateProcessWithLogonW function
    /// is called, which starts the process in a new window even if the CreateNoWindow property value is true or the WindowStyle
    /// property value is Hidden.
    /// </summary>
    public class StartProcess
    {
        static AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Object syncRoot = new Object();
        static Boolean currentProcessUnderLocalSystem;

        //dwCreationFlags
        const Int32 CREATE_NEW_CONSOLE = 0x00000010;
        const Int32 CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        // SECURITY_IMPERSONATION_LEVEL
        const Int32 SecurityImpersonation = 2;

        // TOKEN_TYPE
        const Int32 TokenPrimary = 1;

        // Access Token constants
        const Int32 MAXIMUM_ALLOWED = 0x10000000;

        String userName = String.Empty;
        String password = String.Empty;
        String domain = String.Empty;
        String workingDir = String.Empty;

        ProcessAction startingAction;
        ProcessAction startedAction;

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
            currentProcessUnderLocalSystem = WindowsIdentity.GetCurrent().User.IsWellKnown(WellKnownSidType.LocalSystemSid);
        }

        public StartProcess(String workingDir)
        {
            this.workingDir = workingDir;
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
        /// <param name="sFile">exe full path</param>
        /// <param name="sArgs">arguments</param>
        /// <returns></returns>
        public SD.Process Start(String sFile, String sArgs)
        {
            return Start(sFile, sArgs, null);
        }

        /// <summary>
        /// start a process with environment variable
        /// </summary>
        /// <param name="sFile">exe full path</param>
        /// <param name="sArgs">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public SD.Process Start(String sFile, String sArgs, Dictionary<String, String> environmentVariable)
        {
            var result = default(SD.Process);
            if (String.IsNullOrEmpty(sFile))
                throw new Exception("Cannot start new process: input command is empty");
            //farm admin or dbo is not required in DocAve online, to make it simple, use local system
            if (currentProcessUnderLocalSystem == true || String.IsNullOrEmpty(userName))
                result = this.StartDirectly(sFile, sArgs, environmentVariable);
            else result = this.StartWithWin32(sFile, sArgs, environmentVariable);
            logger.Info("Start Process {0},Args:{1},currentProcessUnderLocalSystem:{2},ProcessId:{3}",
                sFile,sArgs,currentProcessUnderLocalSystem,result==null?"ProcessIsNull":Convert.ToString(result.Id));
            return result;
        }

        /// <summary>
        /// start process and wait for the exit code
        /// </summary>
        /// <param name="sFile">exe full path</param>
        /// <param name="sArgs">arguments</param>
        /// <returns></returns>
        public Int32 StartAndGetExitCode(String sFile, String sArgs)
        {
            return this.StartAndGetExitCode(sFile, sArgs, null);
        }

        /// <summary>
        /// start process and wait for the exit code
        /// </summary>
        /// <param name="sFile">exe full path</param>
        /// <param name="sArgs">arguments</param>
        /// <param name="environmentVariable">processs environment variable</param>
        /// <returns></returns>
        public int StartAndGetExitCode(String sFile, String sArgs, Dictionary<String, String> environmentVariable)
        {
            var result = default(Int32);
            if (String.IsNullOrEmpty(sFile))
                throw new Exception("Cannot start new process: input command is empty");
            if (currentProcessUnderLocalSystem == false || String.IsNullOrEmpty(userName))
                result = this.StartDirectlyAndWaitExit(sFile, sArgs, environmentVariable);
            else result = this.StartWithWin32AndWaitExit(sFile, sArgs, environmentVariable);
            return result;
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
                Array.ForEach<Delegate>(processAction.GetInvocationList(),
                action =>
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
            var result = default(SD.Process);
            var processStartInfo = new ProcessStartInfo(filePath, args);
            processStartInfo.WorkingDirectory = workingDir;
            processStartInfo.UseShellExecute = false;
            if (environmentVariable != null)
            {
                foreach (var envEntry in environmentVariable)
                {
                    processStartInfo.EnvironmentVariables[envEntry.Key] = envEntry.Value;
                }
            }
            var processEventArgs = this.BuildEventArgs(String.Format("\"{0}\" {1}", filePath, args), environmentVariable);
            this.OnProcessStarting(processEventArgs);
            result = SD.Process.Start(processStartInfo);
            this.OnProcessStarted(processEventArgs);
            return result;
        }

        SD.Process StartWithWin32(String imageName, String args, Dictionary<String, String> environmentVariable)
        {
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
                this.OnProcessStarting(processEventArgs);
                if (!Win32Native.CreateProcessAsUserW(
                    userToken,
                    null,
                    imageFullPath,
                    ref secAttr,
                    ref secAttr,
                    false,
                    creationFlag,
                    envPtr,
                    null,
                    ref startInfo,
                    ref processInfo))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var errorMessage = this.FormatMessage(errorCode);
                    logger.Error(String.Format("Win32 function CreateProcessAsUserW failed:win32 error code:{0}, detail:{1}", errorCode, errorMessage));
                    throw new Exception(String.Format("An error occurred while creating the process. win32 error code:{0}, detail:{1}", errorCode, errorMessage));
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

        Int32 StartDirectlyAndWaitExit(String filePath, String args, Dictionary<String, String> environmentVariable)
        {
            var process = StartDirectly(filePath, args, environmentVariable);
            process.WaitForExit();
            return process.ExitCode;
        }

        Int32 StartWithWin32AndWaitExit(String sFile, String sArgs, Dictionary<String, String> environmentVariable)
        {
            var process = StartWithWin32(sFile, sArgs, environmentVariable);
            uint exitCode = 0;
            Win32Native.WaitForSingleObject(process.Handle, int.MaxValue);
            Win32Native.GetExitCodeProcess(process.Handle, ref exitCode);
            return (int)exitCode;
        }

        //From MSDN: If UAC is enabled, LogonUserW returns the restricted token for interactive sessions under some conditions.
        //The details of this behavior should be documented.
        //What conditions ??????
        IntPtr CreateUserToken(
             String domain,
             String username,
             String password)
        {
            if (Win32Native.RevertToSelf())
            {
                IntPtr token = IntPtr.Zero;
                int logonType = Win32Native.LOGON32_LOGON_BATCH;
                if (!OSInformation.UACEnabled)
                {
                    logonType = Win32Native.LOGON32_LOGON_INTERACTIVE;
                }
                string startProcessForceLogonType = ConfigurationManager.AppSettings["startProcessForceLogonType"];
                if (!string.IsNullOrEmpty(startProcessForceLogonType))
                {
                    if (string.Compare(startProcessForceLogonType, "batch", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_BATCH;
                    }
                    if (string.Compare(startProcessForceLogonType, "interactive", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        logonType = Win32Native.LOGON32_LOGON_BATCH;
                    }
                }
                if (!Win32Native.LogonUserW(username, domain, password, logonType, Win32Native.LOGON32_PROVIDER_DEFAULT, ref token))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("LogonUserW() failed: error={0}", errorCode));
                    throw new Win32Exception(errorCode);
                }
                if (!OSInformation.UACEnabled) return token;

                ////The DuplicateTokenEx function allows you to create a primary token that you can use in the CreateProcessAsUser function.
                ////Note that the DuplicateToken function can create only impersonation tokens, which are not valid for CreateProcessAsUser.
                var duplicateToken = IntPtr.Zero;
                var sa = new Win32Native.SECURITY_ATTRIBUTES();
                sa.bInheritHandle = false;
                sa.nLength = Marshal.SizeOf(sa);
                sa.lpSecurityDescriptor = (IntPtr)0;
                if (!Win32Native.DuplicateTokenEx(token, MAXIMUM_ALLOWED, ref sa, SecurityImpersonation, TokenPrimary, ref duplicateToken))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    logger.Error(String.Format("DuplicateTokenEx() failed: error={0}", errorCode));
                    throw new Win32Exception(errorCode);
                }
                else
                {
                    //close the token created by LogonUserW
                    if (token != IntPtr.Zero)
                    {
                        Win32Native.CloseHandle(token);
                        token = IntPtr.Zero;
                    }
                }
                return duplicateToken;
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                logger.Error(String.Format("RevertToSelf() failed: error={0}", errorCode));
                throw new Win32Exception(errorCode);
            }
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

        IntPtr GetProcessEnvironmentVariable(Dictionary<String, String> environmentVariable, GCHandle environmentHandle)
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
            var result = default(Byte[]);
            var builder = new StringBuilder();
            foreach (var envEntity in environmentVariable)
            {
                builder.Append(envEntity.Key);
                builder.Append('=');
                builder.Append(envEntity.Value);
                builder.Append('\0');
            }
            builder.Append('\0');
            result = Encoding.Unicode.GetBytes(builder.ToString());
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
}