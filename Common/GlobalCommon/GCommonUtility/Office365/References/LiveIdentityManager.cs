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
namespace AvePoint.Common.Office365
{
    using AvePoint.GCommon;
    using Microsoft.Online.Administration.Automation;
    using Microsoft.Online.Administration.Automation.PSModule.Resources.IdcrlWrapper;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    internal class LiveIdentityManager
    {
        // Fields
        private const string CredentialTypePassword = "ps:password";
        private const int IdcrlAuthStateAuthenticatedPassword = 0x48803;
        private const int IdcrlCurrentVersion = 1;
        private IntPtr identityPtr;
        private bool initialized;
        private readonly NativeMethods nativeMethods;
        private const int ResultCode = 0;
        private Guid serviceGuid;
        private AveLogger logger = AveLogger.GetInstance(typeof(LiveIdentityManager));

        // Methods
        public LiveIdentityManager() : this(new NativeMethods())
        {}

        internal LiveIdentityManager(NativeMethods nativeLibrary)
        {
            this.identityPtr = IntPtr.Zero;
            this.serviceGuid = Guid.NewGuid();
            this.nativeMethods = nativeLibrary;
        }

        internal void CloseIdentity()
        {
            if (IntPtr.Zero != this.identityPtr && this.nativeMethods.CloseIdentityHandle(this.identityPtr) == 0) this.identityPtr = IntPtr.Zero;
        }

        private void FreeResource(ref IntPtr resource)
        {
            if (IntPtr.Zero != resource && this.nativeMethods.PassportFreeMemory(resource) == 0) resource = IntPtr.Zero;
        }

        internal void GetAuthenticationStatus(string siteName)
        {
            IntPtr zero = IntPtr.Zero;
            try
            {
                int errorCodeParameter = this.nativeMethods.GetAuthenticationStatus(this.identityPtr, siteName, 1, out zero);
                if (errorCodeParameter < 0)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Messages.FailGetAuthState, new object[] { errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                    throw new WindowsLiveException(errorCodeParameter, message);
                }
                var structure = new NativeMethods.IdcrlStatusCurrent();
                Marshal.PtrToStructure(zero, structure);
                if (0x48803 != structure.AuthState)
                {
                    string str2 = string.Format(CultureInfo.InvariantCulture, Messages.FailGetAuthInvalidState, new object[] { "0x" + structure.AuthState.ToString("X", CultureInfo.InvariantCulture), "0x" + structure.RequestStatus.ToString("X", CultureInfo.InvariantCulture), errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                    throw new WindowsLiveException(structure.RequestStatus, str2);
                }
            }
            finally
            {
                this.FreeResource(ref zero);
            }
        }

        internal string GetLoggedOnUser()
        {
            IntPtr ptr;
            string str = WindowsIdentity.GetCurrent().Name.ToLower();
            logger.Info("ConnectMsolService GetLoggedOnUser Enumerating identities to get upn of the user {0}", str);
            string[] strArray = str.Split(new char[] { '\\' });
            if (strArray.Length == 1 && str.Contains("@")) return str;
            int num = this.nativeMethods.EnumIdentitiesWithCachedCredentials(null, out ptr);
            logger.Info("ConnectMsolService GetLoggedOnUser EnumIdentitiesWithCachedCredentials returned {0}",num);
            string wszMemberName = null;
            string str4 = null;
            if (num == 0)
            {
                while (this.nativeMethods.NextIdentity(ptr, ref wszMemberName) == 0)
                {
                    logger.Info("ConnectMsolService GetLoggedOnUser Next identity returned to get upn of the user {0}", wszMemberName);
                    if (!string.IsNullOrEmpty(wszMemberName) && wszMemberName.Contains(strArray[1]))
                    {
                        str4 = wszMemberName;
                        break;
                    }
                }
            }
            this.nativeMethods.CloseEnumIdentitiesHandle(ptr);
            if (num != 0 || str4 == null) throw new WindowsLiveException(Messages.FailAuthIdentityToService);
            return str4;
        }

        internal virtual void Initialize(string environment)
        {
            this.Uninitialize();
            NativeMethods.IdcrlOption[] pOptions = null;
            uint dwOptions = 0;
            GCHandle handle = new GCHandle();
            try
            {
                if (!string.IsNullOrEmpty(environment))
                {
                    pOptions = new NativeMethods.IdcrlOption[1];
                    dwOptions = 1;
                    byte[] bytes = Encoding.Unicode.GetBytes(environment);
                    byte[] buffer2 = new byte[bytes.Length];
                    handle = GCHandle.Alloc(buffer2, GCHandleType.Pinned);
                    IntPtr destination = handle.AddrOfPinnedObject();
                    Marshal.Copy(bytes, 0, destination, bytes.Length);
                    pOptions[0].EnvironmentId = 0x40;
                    pOptions[0].EnvironmentValue = destination;
                    pOptions[0].EnvironmentLength = (uint)bytes.Length;
                }
                int errorCodeParameter = 0;
                errorCodeParameter = this.nativeMethods.InitializeEx(ref this.serviceGuid, 1, 0, pOptions, dwOptions);
                if (errorCodeParameter < 0)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, "Failed to initialize the environment: {0} , HR: {1}", new object[] { environment, errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                    throw new WindowsLiveException(errorCodeParameter, message);
                }
                this.initialized = true;
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
            }
        }

        internal void LogonPassport(string policy, string siteName)
        {
            var pcRSTParams = new NativeMethods.RstParams[1];
            pcRSTParams[0].CbSize = 0;
            pcRSTParams[0].ServiceName = siteName;
            pcRSTParams[0].ServicePolicy = policy;
            pcRSTParams[0].TokenFlags = 0;
            pcRSTParams[0].TokenParams = 0;
            //int errorCodeParameter = this.nativeMethods.LogonIdentityEx(this.identityPtr, policy, 0, pcRSTParams, (uint)pcRSTParams.Length);
            int errorCodeParameter = this.nativeMethods.LogonIdentityExSSO(this.identityPtr, policy, 0x80, 1, null, pcRSTParams, (uint)pcRSTParams.Length);
            this.GetAuthenticationStatus(siteName);
            if (errorCodeParameter < 0)
            {
                string message = string.Format(CultureInfo.InvariantCulture, Messages.FailLoginIdentity, new object[] { policy, errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                throw new WindowsLiveException(errorCodeParameter, message);
            }
        }

        internal virtual string LogonService(string siteName, string policy)
        {
            string str;
            IntPtr zero = IntPtr.Zero;
            IntPtr ppbSessionKey = IntPtr.Zero;
            uint pcbSessionKeyLength = 0;
            uint pdwResultFlags = 0;
            int errorCodeParameter = 0;
            errorCodeParameter = this.nativeMethods.AuthIdentityToService(this.identityPtr, siteName, policy, 0x20000, out zero, out pdwResultFlags, out ppbSessionKey, out pcbSessionKeyLength);
            if (errorCodeParameter < 0)
            {
                string message = string.Format(CultureInfo.InvariantCulture, Messages.FailAuthIdentityToService, new object[] { siteName, policy, errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                throw new WindowsLiveException(errorCodeParameter, message);
            }
            try
            {
                str = Marshal.PtrToStringUni(zero);
            }
            finally
            {
                this.FreeResource(ref zero);
                this.FreeResource(ref ppbSessionKey);
            }
            return str;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public string LogOnUser(string federationProviderId, string userName, string password, string siteName, string policy, string environment)
        {
            this.CloseIdentity();
            this.Initialize(environment);
            this.OpenIdentity(federationProviderId, userName, password);
            this.LogonPassport(policy, siteName);
            return this.LogonService(siteName, policy);
        }

        internal void OpenIdentity(string federationProviderId, string userName, string password)
        {
            try
            {
                int errorCodeParameter = 0;
                errorCodeParameter = this.nativeMethods.CreateIdentityHandle2(federationProviderId, userName, 0xff, out this.identityPtr);
                if (errorCodeParameter < 0)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Messages.FailCreateIdentityHandle, new object[] { userName, errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                    throw new WindowsLiveException(errorCodeParameter, message);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    errorCodeParameter = this.nativeMethods.SetCredential(this.identityPtr, "ps:password", password);
                    if (errorCodeParameter < 0) throw new WindowsLiveException(errorCodeParameter, Messages.FailSetCredential);
                }
            }
            catch
            {
                this.CloseIdentity();
                throw;
            }
        }

        internal void Uninitialize()
        {
            if (this.initialized)
            {
                int errorCodeParameter = 0;
                errorCodeParameter = this.nativeMethods.Uninitialize();
                if (errorCodeParameter < 0)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Messages.FailUninitialize, new object[] { errorCodeParameter.ToString(CultureInfo.InvariantCulture) });
                    throw new WindowsLiveException(errorCodeParameter, message);
                }
                this.initialized = false;
            }
        }
    }
}
