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
namespace AvePoint.Office365.Api.AIR.IPC
{
    using System;
    using System.Runtime.InteropServices;

    internal static class UnsafeNativeMethods
    {
        internal const string g_MSIPCDllName = "msipc.dll";

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetGlobalProperty(
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [Out] out IntPtr ppvProperty);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcSetGlobalProperty(
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropId,
                                [In] IntPtr pvProperty);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetTemplateList(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcConnectionInfo connectionInfo,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In, MarshalAs(UnmanagedType.U4)] uint lcid,
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
                                [In] IntPtr reserved,
                                [Out] out IntPtr pTil);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetTemplateIssuerList(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcConnectionInfo connectionInfo,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
                                [In] IntPtr reserved,
                                [Out] out IntPtr pTemplateIssuers);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcCreateLicenseFromTemplateID(
                                [In, MarshalAs(UnmanagedType.LPWStr)] string wszTemplateID,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In] IntPtr pvReserved,
                                [Out] out SafeInformationProtectionLicenseHandle phLicense);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcCreateLicenseFromScratch(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcTemplateIssuer pTemplateIssuer,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In] IntPtr pvReserved,
                                [Out] out SafeInformationProtectionLicenseHandle phLicense);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcSerializeLicense(
                                [In] IntPtr pvLicenseInfo,
                                [In, MarshalAs(UnmanagedType.U4)] SerializationInputType dwType,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
                                [Out] out SafeInformationProtectionKeyHandle phKey,
                                [Out] out IntPtr pvLicense);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcSetLicenseProperty(
                                [In] SafeInformationProtectionLicenseHandle hLicense,
                                [In, MarshalAs(UnmanagedType.Bool)] bool fDelete,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [In] IntPtr pvProperty);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetLicenseProperty(
                                [In] SafeInformationProtectionLicenseHandle hLicense,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [In, MarshalAs(UnmanagedType.U4)] uint lcid,
                                [Out] out IntPtr ppvProperty);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetSerializedLicenseProperty(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcBuffer pvLicense,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [In] SafeInformationProtectionKeyHandle hKey,
                                [In, MarshalAs(UnmanagedType.U4)] uint lcid,
                                [Out] out IntPtr ppvProperty);

        [DllImport(g_MSIPCDllName, EntryPoint = "IpcGetSerializedLicenseProperty", SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetSerializedLicensePropertyWithoutKey(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcBuffer pvLicense,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [In] IntPtr pKey,
                                [In, MarshalAs(UnmanagedType.U4)] uint lcid,
                                [Out] out IntPtr ppvProperty);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetKey(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcBuffer pvLicense,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
                                [In] IntPtr pvReserved,
                                [Out] out SafeInformationProtectionKeyHandle phKey);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcCreateOAuth2Token(
                                string wszAccessToken,
                                [Out] out SafeInformationProtectionTokenHandle ppv);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcEncrypt(
                                [In] SafeInformationProtectionKeyHandle hKey,
                                [In] uint dwBlockNumber,
                                [In, MarshalAs(UnmanagedType.Bool)] bool fFinal,
                                [In] byte[] pbInput,
                                [In] uint cbInput,
                                [Out] byte[] pbOutput,
                                [In] uint cbOutput,
                                [Out] out uint pcbResult);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcDecrypt(
                                [In] SafeInformationProtectionKeyHandle hKey,
                                [In] uint dwBlockNumber,
                                [In, MarshalAs(UnmanagedType.Bool)] bool fFinal,
                                [In] byte[] pbInput,
                                [In] uint cbInput,
                                [Out] byte[] pbOutput,
                                [In] uint cbOutput,
                                [Out] out uint pcbResult);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcAccessCheck(
                                [In] SafeInformationProtectionKeyHandle hKey,
                                [In, MarshalAs(UnmanagedType.LPWStr)]
                                        string wszRequestedRight,
                                [Out, MarshalAs(UnmanagedType.Bool)]
                                        out bool bAccessGranted);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetKeyProperty(
                                [In] SafeInformationProtectionKeyHandle hKey,
                                [In, MarshalAs(UnmanagedType.U4)] uint dwPropID,
                                [In] IntPtr pvReserved,
                                [Out] out IntPtr ppvProperty);


        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcCloseHandle(
                                [In] IntPtr handle);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern void IpcFreeMemory(
                                [In] IntPtr handle);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcProtectWindow(
                                [In] IntPtr hwnd);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcUnprotectWindow(
                                [In] IntPtr hwnd);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcGetErrorMessageText(
                                [In] int hrError, 
                                [In, MarshalAs(UnmanagedType.U4)] uint dwLanguageId,
                                [Out] out IntPtr ppwszErrorMessageText);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "#44")]
        internal static extern int IpcpUpdateHostnameRedirectionCache(
                                string wszOrgUrl,
                                string wszRedirectUrl,
                                [In, MarshalAs(UnmanagedType.Bool)] bool fDelete);

        [DllImport(g_MSIPCDllName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcRegisterLicense(
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcBuffer pvLicense,
                                [In] IntPtr pvReserved,
                                [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
                                string wszContentName,
                                [In, MarshalAs(UnmanagedType.Bool)] bool fSendRegistrationMail);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDllDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string lpPathName);
    }
}
