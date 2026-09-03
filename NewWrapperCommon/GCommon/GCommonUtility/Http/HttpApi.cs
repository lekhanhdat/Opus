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
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using AvePoint.GCommon.Security.AccessControl;

namespace AvePoint.GCommon.Utility
{
    public class HttpApi : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(HttpApi), false);

        internal const uint ERROR_NO_MORE_ITEMS = 259;
        internal const uint ERROR_INSUFFICIENT_BUFFER = 122;
        internal const uint NO_ERROR = 0;
        internal const uint HTTP_INITIALIZE_CONFIG = 2;
        internal const uint HTTP_SERVICE_CONFIG_SSL_FLAG_USE_DS_MAPPER = 0x00000001;
        internal const uint HTTP_SERVICE_CONFIG_SSL_FLAG_NEGOTIATE_CLIENT_CERT = 0x00000002;
        internal const uint HTTP_SERVICE_CONFIG_SSL_FLAG_NO_RAW_FILTER = 0x00000004;
        internal const uint ERROR_ALREADY_EXISTS = 183;

        public HttpApi()
        {
            var version = new HTTPAPI_VERSION();
            version.HttpApiMajorVersion = 1;
            version.HttpApiMinorVersion = 0;

            HttpApi.HttpInitialize(version, HttpApi.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
        }

        ~HttpApi()
        {
            this.Dispose(false);
        }

        protected void Dispose(bool p)
        {
            HttpApi.HttpTerminate(HttpApi.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        public Dictionary<string, SecurityDescriptor> QueryHttpNamespaceAcls()
        {
            Dictionary<string, SecurityDescriptor> nsTable = new Dictionary<string, SecurityDescriptor>(StringComparer.OrdinalIgnoreCase);

            HTTP_SERVICE_CONFIG_URLACL_QUERY query = new HTTP_SERVICE_CONFIG_URLACL_QUERY();
            query.QueryDesc = HTTP_SERVICE_CONFIG_QUERY_TYPE.HttpServiceConfigQueryNext;

            IntPtr pQuery = Marshal.AllocHGlobal(Marshal.SizeOf(query));

            try
            {
                uint retval = NO_ERROR;
                for (query.dwToken = 0; true; query.dwToken++)
                {
                    Marshal.StructureToPtr(query, pQuery, false);

                    try
                    {
                        uint returnSize = 0;

                        // Get Size
                        retval = HttpQueryServiceConfiguration(IntPtr.Zero, HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, pQuery, (uint) Marshal.SizeOf(query), IntPtr.Zero, 0, ref returnSize, IntPtr.Zero);

                        if (retval == ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }
                        if (retval != ERROR_INSUFFICIENT_BUFFER)
                        {
                            throw new Exception("HttpQueryServiceConfiguration returned unexpected error code.");
                        }

                        IntPtr pConfig = Marshal.AllocHGlobal((IntPtr)returnSize);

                        try
                        {
                            retval = HttpApi.HttpQueryServiceConfiguration(IntPtr.Zero, HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, pQuery, (uint)Marshal.SizeOf(query), pConfig, returnSize, ref returnSize, IntPtr.Zero);

                            if (retval == NO_ERROR)
                            {
                                var config = (HTTP_SERVICE_CONFIG_URLACL_SET)Marshal.PtrToStructure(pConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));

                                SecurityDescriptor descriptor = null;
                                try
                                {
                                    descriptor = SecurityDescriptor.SecurityDescriptorFromString(config.ParamDesc.pStringSecurityDescriptor, false);
                                }
                                catch(Exception ex)
                                {
                                    descriptor = new GCommon.Security.AccessControl.SecurityDescriptor();
                                    descriptor.DACL = new GCommon.Security.AccessControl.AccessControlList();
                                    logger.Warn("get security descriptor from {0} failed:{1}", config.ParamDesc.pStringSecurityDescriptor, ex);
                                }
                                nsTable.Add(config.KeyDesc.pUrlPrefix, descriptor);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pConfig);
                        }
                    }
                    finally
                    {
                        Marshal.DestroyStructure(pQuery, typeof(HTTP_SERVICE_CONFIG_URLACL_QUERY));
                    }
                }

                if (retval != ERROR_NO_MORE_ITEMS)
                {
                    throw new Exception("HttpQueryServiceConfiguration returned unexpected error code.");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pQuery);
            }

            return nsTable;
        }

        public void SetHttpNamespaceAcl(string urlPrefix, SecurityDescriptor acl)
        {
            HTTP_SERVICE_CONFIG_URLACL_SET urlAclConfig = new HTTP_SERVICE_CONFIG_URLACL_SET();
            urlAclConfig.KeyDesc.pUrlPrefix = urlPrefix;
            urlAclConfig.ParamDesc.pStringSecurityDescriptor = acl.ToString();

            IntPtr pUrlAclConfig = Marshal.AllocHGlobal(Marshal.SizeOf(urlAclConfig));

            Marshal.StructureToPtr(urlAclConfig, pUrlAclConfig, false);

            try
            {
                uint retval = HttpApi.HttpSetServiceConfiguration(IntPtr.Zero, HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, pUrlAclConfig, (uint)Marshal.SizeOf(urlAclConfig), IntPtr.Zero);

                if (retval != 0)
                {
                    throw new ExternalException("Error Setting Configuration: " + Util.GetErrorMessage(retval));
                }
            }
            finally
            {
                if (pUrlAclConfig != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(pUrlAclConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                    Marshal.FreeHGlobal(pUrlAclConfig); ;
                }
            }
        }

        public void RemoveHttpHamespaceAcl(string urlPrefix)
        {
            HTTP_SERVICE_CONFIG_URLACL_SET urlAclConfig = new HTTP_SERVICE_CONFIG_URLACL_SET();
            urlAclConfig.KeyDesc.pUrlPrefix = urlPrefix;
            //urlAclConfig.ParamDesc.pStringSecurityDescriptor = acl.ToString();

            IntPtr pUrlAclConfig = Marshal.AllocHGlobal(Marshal.SizeOf(urlAclConfig));

            Marshal.StructureToPtr(urlAclConfig, pUrlAclConfig, false);

            try
            {
                uint retval = HttpApi.HttpDeleteServiceConfiguration(IntPtr.Zero, HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, pUrlAclConfig, (uint)Marshal.SizeOf(urlAclConfig), IntPtr.Zero);

                if (retval != 0)
                {
                    throw new ExternalException("Error Setting Configuration: " + Util.GetErrorMessage(retval));
                }
            }
            finally
            {
                if (pUrlAclConfig != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(pUrlAclConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                    Marshal.FreeHGlobal(pUrlAclConfig); ;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="certificateThumbprint"></param>
        /// <param name="overwrite"></param>
        public static void BindCertificate(string ipAddress, int port, string certificateThumbprint, bool overwrite)
        {
            BindCertificate(ipAddress, port, DecodeHexString(certificateThumbprint), overwrite);
        }

        private static byte[] DecodeHexString(string hexString)
        {
            byte[] buffer;
            if (hexString == null) throw new ArgumentNullException("hexString");
            bool flag = false;
            int num = 0;
            int length = hexString.Length;
            if (length >= 2 && hexString[0] == '0' && hexString[1] == 'x' || hexString[1] == 'X')
            {
                length = hexString.Length - 2;
                num = 2;
            }
            if (length % 2 != 0 && length % 3 != 2) throw new ArgumentOutOfRangeException("hexString", hexString, string.Empty);
            if (length >= 3 && hexString[num + 2] == ' ')
            {
                flag = true;
                buffer = new byte[length / 3 + 1];
            }
            else
                buffer = new byte[length / 2];
            for (int i = 0; num < hexString.Length; i++)
            {
                int num4 = ConvertHexDigit(hexString[num]);
                int num3 = ConvertHexDigit(hexString[num + 1]);
                buffer[i] = (byte)(num3 | num4 << 4);
                if (flag) num++;
                num += 2;
            }
            return buffer;
        }

        private static int ConvertHexDigit(char val)
        {
            if (val <= '9' && val >= '0') return (val - '0');
            if (val >= 'a' && val <= 'f') return (val - 'a' + 10);
            if (val < 'A' || val > 'F') throw new ArgumentOutOfRangeException("val", val, string.Empty);
            return (val - 'A' + 10);
        }

        /// <summary>
        /// Bind certificate to port
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="hash"></param>
        /// <param name="overWrite"></param>
        public static void BindCertificate(string ipAddress, int port, byte[] hash, bool overWrite)
        {
            var retVal = NO_ERROR; // NOERROR = 0

            var httpApiVersion = new HTTPAPI_VERSION(1, 0);
            retVal = HttpInitialize(httpApiVersion, HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if (NO_ERROR == retVal)
            {
                var configSslSet = new HTTP_SERVICE_CONFIG_SSL_SET();
                var httpServiceConfigSslKey = new HTTP_SERVICE_CONFIG_SSL_KEY();
                var configSslParam = new HTTP_SERVICE_CONFIG_SSL_PARAM();

                var ip = IPAddress.Parse(ipAddress);

                var ipEndPoint = new IPEndPoint(ip, port);
                // serialize the endpoint to a SocketAddress and create an array to hold the values.  Pin the array.
                var socketAddress = ipEndPoint.Serialize();
                var socketBytes = new byte[socketAddress.Size];
                var handleSocketAddress = GCHandle.Alloc(socketBytes, GCHandleType.Pinned);
                // Should copy the first 16 bytes (the SocketAddress has a 32 byte buffer, the size will only be 16,
                //which is what the SOCKADDR accepts
                for (var i = 0; i < socketAddress.Size; ++i)
                {
                    socketBytes[i] = socketAddress[i];
                }

                httpServiceConfigSslKey.pIpPort = handleSocketAddress.AddrOfPinnedObject();

                var handleHash = GCHandle.Alloc(hash, GCHandleType.Pinned);
                configSslParam.AppId = Guid.NewGuid();
                configSslParam.DefaultCertCheckMode = 0;
                configSslParam.DefaultFlags = HTTP_SERVICE_CONFIG_SSL_FLAG_NEGOTIATE_CLIENT_CERT;
                configSslParam.DefaultRevocationFreshnessTime = 0;
                configSslParam.DefaultRevocationUrlRetrievalTimeout = 0;
                configSslParam.pSslCertStoreName = StoreName.My.ToString();
                configSslParam.pSslHash = handleHash.AddrOfPinnedObject(); 
                configSslParam.SslHashLength = hash.Length;
                configSslSet.ParamDesc = configSslParam;
                configSslSet.KeyDesc = httpServiceConfigSslKey;

                var pInputConfigInfo = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(HTTP_SERVICE_CONFIG_SSL_SET)));
                Marshal.StructureToPtr(configSslSet, pInputConfigInfo, false);

                retVal = HttpSetServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo,
                    pInputConfigInfo,
                    (uint)Marshal.SizeOf(configSslSet),
                    IntPtr.Zero);

                if (ERROR_ALREADY_EXISTS == retVal && overWrite)  // ERROR_ALREADY_EXISTS = 183
                {
                    retVal = HttpDeleteServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo,
                    pInputConfigInfo,
                    (uint)Marshal.SizeOf(configSslSet),
                    IntPtr.Zero);

                    if (NO_ERROR == retVal)
                    {
                        retVal = HttpSetServiceConfiguration(IntPtr.Zero,
                            HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo,
                            pInputConfigInfo,
                            (uint)Marshal.SizeOf(configSslSet),
                            IntPtr.Zero);
                    }
                }

                handleSocketAddress.Free();
                handleHash.Free();

                Marshal.FreeCoTaskMem(pInputConfigInfo);
                HttpTerminate(HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            }

            if (NO_ERROR != retVal)
            {
                //throw new Win32Exception(Convert.ToInt32(retVal));
                logger.Warn("bind certificate to {0}:{1} with overwrite:{2} failed:{3}", ipAddress, port, overWrite, new Win32Exception(Convert.ToInt32(retVal)));
            }
        }

        /// <summary>
        /// sid
        /// </summary>
        /// <param name="port"></param>
        /// <param name="sid"></param>
        public static void AddHttpsAclUrlWithSid(int port, string sid)
        {
            try
            {
                var identity = SecurityIdentity.SecurityIdentityFromString(sid, true);

                AddHttpsAclUrl(port, identity);
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot get identity from sid:{0}, details:{1}", sid, ex);
            }
        }

        /// <summary>
        /// well known sid
        /// </summary>
        /// <param name="port"></param>
        /// <param name="sidType"></param>
        public static void AddHttpsAclUrl(int port, WELL_KNOWN_SID_TYPE sidType)
        {
            try
            {
                var identity = SecurityIdentity.SecurityIdentityFromWellKnownSid(sidType);

                AddHttpsAclUrl(port, identity);
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot get identity from well known sid:{0}, details:{1}", sidType, ex);
            }
        }

        /// <summary>
        /// user name 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="userNameOrGroupName"></param>
        public static void AddHttpsAclUrl(int port, string userNameOrGroupName)
        {
            try
            {
                var identity = SecurityIdentity.SecurityIdentityFromName(userNameOrGroupName);

                AddHttpsAclUrl(port, identity);
            }
            catch(Exception ex)
            {
                logger.Warn("Cannot get identity from name:{0}, details:{1}", userNameOrGroupName, ex);
            }
        }

        /// <summary>
        /// Add Https Acl Url
        /// </summary>
        /// <param name="port"></param>
        /// <param name="userNameOrGroupName"></param>
        private static void AddHttpsAclUrl(int port, SecurityIdentity securityIdentity)
        {
            var httpUrl = string.Format("http://+:{0}/", port);
            var httpsUrl = string.Format("https://+:{0}/", port);
            try
            {
                using (var httpApi = new HttpApi())
                {
                    var coll = httpApi.QueryHttpNamespaceAcls();

                    SecurityDescriptor securityDescriptor = null;
                    
                    if(coll.ContainsKey(httpUrl))
                    {
                        httpApi.RemoveHttpHamespaceAcl(httpUrl);
                        logger.Info("Remove http url acl according to {0}", httpUrl);
                    }

                    if (coll.TryGetValue(httpsUrl, out securityDescriptor))
                    {
                        AccessControlEntry entry = null;

                        if (securityDescriptor.DACL != null)
                        {
                            logger.Info("The acl detail info of url:{0} is {1}", httpsUrl, securityDescriptor.DACL.DetailInfo());
                        }
                        logger.Info("The current acl of url:{0} is {1}", httpsUrl, securityDescriptor);

                        foreach (var item in securityDescriptor.DACL)
                        {
                            if (item.AccountSID.SID.Equals(securityIdentity.SID, StringComparison.OrdinalIgnoreCase))
                            {
                                entry = item;
                                break;
                            }
                        }

                        if (entry == null)
                        {
                            entry = new GCommon.Security.AccessControl.AccessControlEntry(securityIdentity);
                            entry.AceType = AceType.AccessAllowed;
                            entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);

                            securityDescriptor.DACL.Add(entry);
                            logger.Info("Remove the url:{0} and add it with acl:{1}", httpsUrl, securityDescriptor);
                            httpApi.RemoveHttpHamespaceAcl(httpsUrl);
                            httpApi.SetHttpNamespaceAcl(httpsUrl, securityDescriptor);
                        }
                        else if (!entry.Contains(GCommon.Security.AccessControl.AceRights.GenericExecute))
                        {
                            entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);
                            logger.Info("Remove the url:{0} and add it with acl:{1}", httpsUrl, securityDescriptor);
                            httpApi.RemoveHttpHamespaceAcl(httpsUrl);
                            httpApi.SetHttpNamespaceAcl(httpsUrl, securityDescriptor);
                        }
                    }
                    else
                    {
                        var entry = new AccessControlEntry(securityIdentity);
                        entry.AceType = AceType.AccessAllowed;
                        entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);

                        securityDescriptor = new SecurityDescriptor();
                        securityDescriptor.DACL = new AccessControlList();
                        securityDescriptor.DACL.Add(entry);

                        httpApi.SetHttpNamespaceAcl(httpsUrl, securityDescriptor);
                        logger.Info("Add http namespace:{0} with acl:{1}", httpsUrl, securityDescriptor);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot add urlAcl for:{0} with account:{1}, details:{2}", httpsUrl, securityIdentity.Name, ex);
            }
        }

        /*
         * ULONG HttpInitialize(
         *     HTTPAPI_VERSION Version,
         *     ULONG Flags,
         *     PVOID pReserved
         * );
         */
        [DllImport("Httpapi.dll")]
        internal static extern uint HttpInitialize(HTTPAPI_VERSION Version, uint Flags, IntPtr pReserved);

        /*
         * ULONG HttpTerminate(
         *     ULONG Flags,
         *     PVOID pReserved
         * );
         */
        [DllImport("Httpapi.dll")]
        internal static extern uint HttpTerminate(uint Flags, IntPtr pReserved);

        /*
         * ULONG HttpSetServiceConfiguration(
         *     HANDLE ServiceHandle,
         *     HTTP_SERVICE_CONFIG_ID ConfigId,
         *     PVOID pConfigInformation,
         *     ULONG ConfigInformationLength,
         *     LPOVERLAPPED pOverlapped
         * );
         */
        [DllImport("Httpapi.dll")]
        internal static extern uint HttpSetServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pConfigInformation, uint ConfigInformationLength, IntPtr pOverlapped);

        /*
         * ULONG HttpQueryServiceConfiguration(
         *     HANDLE ServiceHandle,
         *     HTTP_SERVICE_CONFIG_ID ConfigId,
         *     PVOID pInputConfigInfo,
         *     ULONG InputConfigInfoLength,
         *     PVOID pOutputConfigInfo,
         *     ULONG OutputConfigInfoLength,
         *     PULONG pReturnLength,
         *     LPOVERLAPPED pOverlapped
         * );
         */
        [DllImport("Httpapi.dll")]
        internal static extern uint HttpQueryServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pInputConfigInfo, uint InputConfigLength, IntPtr pOutputConfigInfo, uint OutputConfigInfoLength, ref uint pReturnLength, IntPtr pOverlapped);

        /*
         * ULONG HttpDeleteServiceConfiguration(
         *     HANDLE ServiceHandle,
         *     HTTP_SERVICE_CONFIG_ID ConfigId,
         *     PVOID pConfigInformation,
         *     ULONG ConfigInformationLength,
         *     LPOVERLAPPED pOverlapped
         * );
         */
        [DllImport("Httpapi.dll")]
        internal static extern uint HttpDeleteServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pConfigInformation, uint ConfigInformationLength, IntPtr pOverlapped);
    }

    /*
     * typedef struct _HTTPAPI_VERSION
     * {
     *     USHORT HttpApiMajorVersion;
     *     USHORT HttpApiMinorVersion;
     * } HTTPAPI_VERSION,  *PHTTPAPI_VERSION;
     */
    internal struct HTTPAPI_VERSION
    {
        public ushort HttpApiMajorVersion;
        public ushort HttpApiMinorVersion;

        public HTTPAPI_VERSION(ushort majorVersion, ushort minorVersion)
        {
            HttpApiMajorVersion = majorVersion;
            HttpApiMinorVersion = minorVersion;
        }
    }

    /*
     * typedef enum _HTTP_SERVICE_CONFIG_ID
     * {
     *     HttpServiceConfigIPListenList,
     *     HttpServiceConfigSSLCertInfo,
     *     HttpServiceConfigUrlAclInfo,
     *     HttpServiceConfigTimeout,
     *     HttpServiceConfigMax
     * }HTTP_SERVICE_CONFIG_ID,  *PHTTP_SERVICE_CONFIG_ID;
     */
    internal enum HTTP_SERVICE_CONFIG_ID
    {
        HttpServiceConfigIPListenList,
        HttpServiceConfigSSLCertInfo,
        HttpServiceConfigUrlAclInfo,
        HttpServiceConfigTimeout,
        HttpServiceConfigMax
    }

    /*
     * typedef struct _HTTP_SERVICE_CONFIG_URLACL_QUERY {
     *     HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
     *     HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
     *     DWORD dwToken;
     * } HTTP_SERVICE_CONFIG_URLACL_QUERY,  *PHTTP_SERVICE_CONFIG_URLACL_QUERY;
     */
    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_QUERY
    {
        public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
        public uint dwToken;
    }

    /*
     * typedef enum _HTTP_SERVICE_CONFIG_QUERY_TYPE
     * {
     *     HttpServiceConfigQueryExact,
     *     HttpServiceConfigQueryNext,
     *     HttpServiceConfigQueryMax
     * } HTTP_SERVICE_CONFIG_QUERY_TYPE,  *PHTTP_SERVICE_CONFIG_QUERY_TYPE;
     */
    internal enum HTTP_SERVICE_CONFIG_QUERY_TYPE
    {
        HttpServiceConfigQueryExact,
        HttpServiceConfigQueryNext,
        HttpServiceConfigQueryMax
    }

    /*
     * typedef struct _HTTP_SERVICE_CONFIG_URLACL_KEY
     * {
     *     PWSTR pUrlPrefix;
     * } HTTP_SERVICE_CONFIG_URLACL_KEY, *PHTTP_SERVICE_CONFIG_URLACL_KEY;
     */
    internal struct HTTP_SERVICE_CONFIG_URLACL_KEY
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pUrlPrefix;
    }

    /*
     * typedef struct _HTTP_SERVICE_CONFIG_URLACL_SET
     * {
     *     HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
     *     HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
     * } HTTP_SERVICE_CONFIG_URLACL_SET,  *PHTTP_SERVICE_CONFIG_URLACL_SET;
     */
    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_SET
    {
        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
    }

    /*
     * typedef struct _HTTP_SERVICE_CONFIG_URLACL_PARAM
     * {
     *     PWSTR pStringSecurityDescriptor;
     * } HTTP_SERVICE_CONFIG_URLACL_PARAM,  *PHTTP_SERVICE_CONFIG_URLACL_PARAM;
     */
    internal struct HTTP_SERVICE_CONFIG_URLACL_PARAM
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pStringSecurityDescriptor;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_SET
    {
        public HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_SSL_PARAM ParamDesc;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_KEY
    {
        public IntPtr pIpPort;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct HTTP_SERVICE_CONFIG_SSL_PARAM
    {
        public int SslHashLength;
        public IntPtr pSslHash;
        public Guid AppId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pSslCertStoreName;
        public uint DefaultCertCheckMode;
        public int DefaultRevocationFreshnessTime;
        public int DefaultRevocationUrlRetrievalTimeout;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlIdentifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlStoreName;
        public uint DefaultFlags;
    }
}
