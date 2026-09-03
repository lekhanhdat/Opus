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


using AvePoint.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    internal class AzureNativeMethods
    {
        // Fields
        private static _AuthIdentityToService authIdentityToServiceFuncPtr;
        private static _CloseEnumIdentitiesHandle closeEnumIdentitiesHandleFuncPtr;
        private static _CloseIdentityHandle closeIdentityHandleFuncPtr;
        private static _CreateIdentityHandle2 createIdentityHandle2FuncPtr;
        private static _EnumIdentitiesWithCachedCredentials enumIdentitiesWithCachedCredentialsFuncPtr;
        private static _GetAuthenticationStatus getAuthenticationStatusFuncPtr;
        private static string identityCrlDllPath = string.Empty;
        private const string IdentityCrlDllToLoadName = "MSOIDCLIL.DLL";
        private const string IdentityCrlInstallPathRegKeyName = "TargetDir";
        private const string IdentityCrlRegistrySubKey = @"Software\Microsoft\MSOIdentityCRL";
        private static bool initialized;
        private static _InitializeEx initializeExFuncPtr;
        private static _LogonIdentityEx logonIdentityExFuncPtr;
        private static _LogonIdentityExSSO logonIdentityExSSOFuncPtr;
        private static _NextIdentity nextIdentityFuncPtr;
        private static _PassportFreeMemory passportFreeMemoryFuncPtr;
        private static _SetCredential setCredentialFuncPtr;
        private static object syncObject = new object();
        private static _Uninitialize uninitializeFuncPtr;        

        // Methods
        internal AzureNativeMethods()
        {
            Initialize();
        }

        internal virtual int AuthIdentityToService([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string szServiceTarget, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string szServicePolicy, [In] uint dwTokenRequestFlags, out IntPtr szToken, out uint pdwResultFlags, out IntPtr ppbSessionKey, out uint pcbSessionKeyLength)
        {
            return authIdentityToServiceFuncPtr(hIdentity, szServiceTarget, szServicePolicy, dwTokenRequestFlags, out szToken, out pdwResultFlags, out ppbSessionKey, out pcbSessionKeyLength);
        }

        internal virtual int CloseEnumIdentitiesHandle(IntPtr hEnumHandle)
        {
            return closeEnumIdentitiesHandleFuncPtr(hEnumHandle);
        }

        internal virtual int CloseIdentityHandle([In] IntPtr hIdentity)
        {
            return closeIdentityHandleFuncPtr(hIdentity);
        }

        internal virtual int CreateIdentityHandle2([In, MarshalAs(UnmanagedType.LPWStr)] string wszFederationProvider, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string wszMemberName, [In] uint dwFlags, out IntPtr pihIdentity)
        {
            return createIdentityHandle2FuncPtr(wszFederationProvider, wszMemberName, dwFlags, out pihIdentity);
        }

        internal virtual int EnumIdentitiesWithCachedCredentials([In, Optional, MarshalAs(UnmanagedType.LPWStr)] string wszCachedCredType, out IntPtr peihEnumHandle)
        {
            return enumIdentitiesWithCachedCredentialsFuncPtr(wszCachedCredType, out peihEnumHandle);
        }

        internal virtual int GetAuthenticationStatus([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string wzServiceTarget, [In] uint dwVersion, out IntPtr ppStatus)
        {
            return getAuthenticationStatusFuncPtr(hIdentity, wzServiceTarget, dwVersion, out ppStatus);
        }

        private static Delegate GetFunctionPointer(IntPtr msoidcli, string methodName, Type delegateType)
        {
            IntPtr procAddress = GetProcAddress(msoidcli, methodName);
            if (procAddress == IntPtr.Zero)
            {
                int num = Marshal.GetLastWin32Error();
                throw new DynamicPInvokeException(string.Format(CultureInfo.InvariantCulture, "Failed to get address for method: {0} from library: {1}. GetLastError code: {2}", new object[] { methodName, identityCrlDllPath, num }));
            }
            return Marshal.GetDelegateForFunctionPointer(procAddress, delegateType);
        }

        private static string GetIdentityCrlDllPath()
        {            
            string dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\bin";
            if (Directory.Exists(dir))
            {
                return Path.Combine(dir, IdentityCrlDllToLoadName);
            }
            else
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\", IdentityCrlDllToLoadName);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr GetProcAddress([In] IntPtr hModule, [In, MarshalAs(UnmanagedType.LPStr)] string procname);
        private static void Initialize()
        {
            if (!initialized)
            {
                lock (syncObject)
                {
                    if (!initialized)
                    {
                        identityCrlDllPath = GetIdentityCrlDllPath();
                        IntPtr msoidcli = LoadLibrary(identityCrlDllPath);
                        if (msoidcli == IntPtr.Zero)
                        {
                            int num = Marshal.GetLastWin32Error();
                            throw new DynamicPInvokeException(string.Format(CultureInfo.InvariantCulture, "Failed to load library: {0}. GetLastError code: {1}", new object[] { identityCrlDllPath, num }));
                        }
                        closeIdentityHandleFuncPtr = (_CloseIdentityHandle)GetFunctionPointer(msoidcli, "CloseIdentityHandle", typeof(_CloseIdentityHandle));
                        logonIdentityExSSOFuncPtr = (_LogonIdentityExSSO)GetFunctionPointer(msoidcli, "LogonIdentityExSSO", typeof(_LogonIdentityExSSO));
                        logonIdentityExFuncPtr = (_LogonIdentityEx)GetFunctionPointer(msoidcli, "LogonIdentityEx", typeof(_LogonIdentityEx));
                        getAuthenticationStatusFuncPtr = (_GetAuthenticationStatus)GetFunctionPointer(msoidcli, "GetAuthenticationStatus", typeof(_GetAuthenticationStatus));
                        passportFreeMemoryFuncPtr = (_PassportFreeMemory)GetFunctionPointer(msoidcli, "PassportFreeMemory", typeof(_PassportFreeMemory));
                        authIdentityToServiceFuncPtr = (_AuthIdentityToService)GetFunctionPointer(msoidcli, "AuthIdentityToService", typeof(_AuthIdentityToService));
                        setCredentialFuncPtr = (_SetCredential)GetFunctionPointer(msoidcli, "SetCredential", typeof(_SetCredential));
                        createIdentityHandle2FuncPtr = (_CreateIdentityHandle2)GetFunctionPointer(msoidcli, "CreateIdentityHandle2", typeof(_CreateIdentityHandle2));
                        initializeExFuncPtr = (_InitializeEx)GetFunctionPointer(msoidcli, "InitializeEx", typeof(_InitializeEx));
                        uninitializeFuncPtr = (_Uninitialize)GetFunctionPointer(msoidcli, "Uninitialize", typeof(_Uninitialize));
                        enumIdentitiesWithCachedCredentialsFuncPtr = (_EnumIdentitiesWithCachedCredentials)GetFunctionPointer(msoidcli, "EnumIdentitiesWithCachedCredentials", typeof(_EnumIdentitiesWithCachedCredentials));
                        nextIdentityFuncPtr = (_NextIdentity)GetFunctionPointer(msoidcli, "NextIdentity", typeof(_NextIdentity));
                        closeEnumIdentitiesHandleFuncPtr = (_CloseEnumIdentitiesHandle)GetFunctionPointer(msoidcli, "CloseEnumIdentitiesHandle", typeof(_CloseEnumIdentitiesHandle));
                        initialized = true;
                    }
                }
            }
        }

        internal virtual int InitializeEx([In] ref Guid guid, [In] int lPPCRLVersion, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.LPArray)] IdcrlOption[] pOptions, [In] uint dwOptions)
        {
            return initializeExFuncPtr(ref guid, lPPCRLVersion, dwFlags, pOptions, dwOptions);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPStr)] string dllname);
        internal virtual int LogonIdentityEx([In] IntPtr hIdentity, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string authPolicy, [In] uint dwAuthFlags, [In, MarshalAs(UnmanagedType.LPArray)] RstParams[] pcRSTParams, [In] uint dwpcRSTParamsCount)
        {
            return logonIdentityExFuncPtr(hIdentity, authPolicy, dwAuthFlags, pcRSTParams, dwpcRSTParamsCount);
        }

        internal virtual int LogonIdentityExSSO([In] IntPtr hIdentity, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string authPolicy, [In] uint dwAuthFlags, [In] uint dwSSOFlags, [In, Out, Optional] PCUIParam2 pcUIParam2, [In, MarshalAs(UnmanagedType.LPArray)] RstParams[] pcRSTParams, [In] uint dwpcRSTParamsCount)
        {
            return logonIdentityExSSOFuncPtr(hIdentity, authPolicy, dwAuthFlags, dwSSOFlags, pcUIParam2, pcRSTParams, dwpcRSTParamsCount);
        }

        internal virtual int NextIdentity(IntPtr hEnumHandle, ref string wszMemberName)
        {
            IntPtr ptr = new IntPtr();
            int num = nextIdentityFuncPtr(hEnumHandle, ref ptr);
            wszMemberName = null;
            wszMemberName = Marshal.PtrToStringUni(ptr);
            return num;
        }

        internal virtual int PassportFreeMemory([In, Out] IntPtr pMemoryToFree)
        {
            return passportFreeMemoryFuncPtr(pMemoryToFree);
        }

        internal virtual int SetCredential([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string wszCredType, [In, MarshalAs(UnmanagedType.LPWStr)] string wszCredValue)
        {
            return setCredentialFuncPtr(hIdentity, wszCredType, wszCredValue);
        }

        internal virtual int Uninitialize()
        {
            return uninitializeFuncPtr();
        }

        // Nested Types
        private delegate int _AuthIdentityToService([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string szServiceTarget, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string szServicePolicy, [In] uint dwTokenRequestFlags, out IntPtr szToken, out uint pdwResultFlags, out IntPtr ppbSessionKey, out uint pcbSessionKeyLength);

        private delegate int _CloseEnumIdentitiesHandle(IntPtr hEnumHandle);

        private delegate int _CloseIdentityHandle([In] IntPtr hIdentity);

        private delegate int _CreateIdentityHandle2([In, MarshalAs(UnmanagedType.LPWStr)] string wszFederationProvider, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string wszMemberName, [In] uint dwFlags, out IntPtr pihIdentity);

        private delegate int _EnumIdentitiesWithCachedCredentials([In, Optional, MarshalAs(UnmanagedType.LPWStr)] string wszCachedCredType, out IntPtr peihEnumHandle);

        private delegate int _GetAuthenticationStatus([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string wzServiceTarget, [In] uint dwVersion, out IntPtr ppStatus);

        private delegate int _InitializeEx([In] ref Guid guid, [In] int lPPCRLVersion, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.LPArray)] AzureNativeMethods.IdcrlOption[] pOptions, [In] uint dwOptions);

        private delegate int _LogonIdentityEx([In] IntPtr Identity, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string authPolicy, [In] uint dwAuthFlags, [In, MarshalAs(UnmanagedType.LPArray)] AzureNativeMethods.RstParams[] pcRSTParams, [In] uint dwpcRSTParamsCount);

        private delegate int _LogonIdentityExSSO([In] IntPtr Identity, [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string authPolicy, [In] uint dwAuthFlags, [In] uint dwSsoFlags, [In, Out, Optional] AzureNativeMethods.PCUIParam2 pcUIParam2, [In, MarshalAs(UnmanagedType.LPArray)] AzureNativeMethods.RstParams[] pcRSTParams, [In] uint dwpcRSTParamsCount);

        private delegate int _NextIdentity(IntPtr hEnumHandle, ref IntPtr wszMemberName);

        private delegate int _PassportFreeMemory([In, Out] IntPtr pMemoryToFree);

        private delegate int _SetCredential([In] IntPtr hIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string wszCredType, [In, MarshalAs(UnmanagedType.LPWStr)] string wszCredValue);

        private delegate int _Uninitialize();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct IdcrlOption
        {
            public int EnvironmentId;
            public IntPtr EnvironmentValue;
            public uint EnvironmentLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class IdcrlStatusCurrent
        {
            internal int AuthState { get; set; }
            internal int AuthRequired { get; set; }
            internal int RequestStatus { get; set; }
            internal int UserInterfaceError { get; set; }
            internal string WebFlowUrl { get; set; }
        }

        internal enum IdentityFlag : uint
        {
            IdentityAllBit = 0x3ff,
            IdentityAuthStateEncrypted = 0x200,
            IdentityLoadFromPersistedStore = 0x100,
            IdentityShareAll = 0xff
        }

        internal enum LogOnFlag
        {
            LogOnIdentityAllBit = 0x1ff,
            LogOnIdentityAllowOffline = 1,
            LogOnIdentityAllowPersistentCookies = 8,
            LogOnIdentityAutoPartnerRedirect = 0x100,
            LogOnIdentityCreateOfflineHash = 4,
            LogOnIdentityDefault = 0,
            LogOnIdentityFederated = 0x40,
            LogOnIdentityForceOffline = 2,
            LogOnIdentityUseEasyIdAuth = 0x10,
            LogOnIdentityUseLinkedAccounts = 0x20,
            LogOnIdentityWindowsLiveId = 0x80
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal class PCUIParam2
        {
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RstParams
        {
            internal int CbSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string ServiceName;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string ServicePolicy;
            internal int TokenFlags;
            internal int TokenParams;
        }

        internal enum ServiceTokenFlags : uint
        {
            ServiceTokenAny = 0xff,
            ServiceTokenCertInMemoryPrivateKey = 0x10,
            ServiceTokenCompactWebSso = 4,
            ServiceTokenFromCache = 0x10000,
            ServiceTokenIgnoreCache = 0x20000,
            ServiceTokenLegacyPassport = 1,
            ServiceTokenRequestTypeNone = 0,
            ServiceTokenTypeProprietary = 1,
            ServiceTokenTypeSaml = 2,
            ServiceTokenWebSso = 2,
            ServiceTokenX509v3 = 8
        }

        internal enum SsoFlag
        {
            SsoAllBit = 15,
            SsoDefault = 0,
            SsoNoAutoSignIn = 2,
            SsoNoHandleError = 4,
            SsoNoUi = 1
        }

        internal enum SsoGroup
        {
            SsoGroupEnterprise = 0x20,
            SsoGroupLive = 0x10,
            SsoGroupNone = 0
        }

        internal enum UpdateFlag : uint
        {
            DefaultUpdatePolicy = 0,
            NoUI = 2,
            OfflineModeAllowed = 1,
            SendVersion = 0x10,
            SetExtendedError = 8,
            SkipConnectionCheck = 4,
            UpdateDefault = 0,
            UpdateFlagAllBit = 15
        }
    }

}
