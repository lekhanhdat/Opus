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
namespace Microsoft365.Authentication.ServiceEndPoint
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    internal static class DnsResolver
    {
        internal static class NativeMethods
        {
            internal enum DnsQueryTypes
            {
                DNS_TYPE_CNAME = 5,
                DNS_TYPE_MX = 15,
                DNS_TYPE_TEXT,
                DNS_TYPE_SRV = 33
            }

            [Flags]
            internal enum DnsQueryOptions
            {
                DNS_QUERY_BYPASS_CACHE = 8,
                DNS_QUERY_TREAT_AS_FQDN = 4096
            }

            internal enum DnsQueryErrorCode
            {
                NO_ERROR,
                ERROR_TIMEOUT = 1460,
                DNS_ERROR_RCODE_SERVER_FAILURE = 9002,
                DNS_ERROR_RCODE_NAME_ERROR,
                DNS_INFO_NO_RECORDS = 9501,
                DNS_ERROR_BAD_PACKET,
                DNS_ERROR_INVALID_NAME_CHAR = 9560
            }

            internal struct CNameRecord
            {
                public IntPtr pNext;

                public string pName;

                public short wType;

                public short wDataLength;

                public int flags;

                public int dwTtl;

                public int dwReserved;

                public IntPtr pNameHost;
            }

            [DllImport("dnsapi", CharSet = CharSet.Unicode, EntryPoint = "DnsQuery_W", ExactSpelling = true, SetLastError = true)]
            internal static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszName, DnsQueryTypes wType, DnsQueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

            [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern void DnsRecordListFree(IntPtr pRecordList, int freeType);
        }

        internal class TypeDependencies
        {
            internal virtual PlatformID GetEnvironmentOSVersionPlatform()
            {
                return Environment.OSVersion.Platform;
            }

            internal virtual NativeMethods.DnsQueryErrorCode QueryDns(string cnameAlias, NativeMethods.DnsQueryOptions dnsQueryOptions, ref IntPtr queryResultList)
            {
                return (NativeMethods.DnsQueryErrorCode)NativeMethods.DnsQuery(ref cnameAlias, NativeMethods.DnsQueryTypes.DNS_TYPE_CNAME, dnsQueryOptions, 0, ref queryResultList, 0);
            }
        }

        private static TypeDependencies dependencies = new TypeDependencies();

        internal static TypeDependencies Dependencies
        {
            get
            {
                return dependencies;
            }
            set
            {
                dependencies = value;
            }
        }

        public static IList<string> ResolveCNameRecord(string cnameAlias)
        {
            if (Dependencies.GetEnvironmentOSVersionPlatform() != PlatformID.Win32NT)
            {
                throw new NotSupportedException("Native QueryDns() requires Windows NT or later.");
            }
            IList<string> cnameTarget = new List<string>();
            IntPtr queryResultList = IntPtr.Zero;
            try
            {
                NativeMethods.DnsQueryErrorCode result = Dependencies.QueryDns(cnameAlias, NativeMethods.DnsQueryOptions.DNS_QUERY_BYPASS_CACHE | NativeMethods.DnsQueryOptions.DNS_QUERY_TREAT_AS_FQDN, ref queryResultList);
                if (result != NativeMethods.DnsQueryErrorCode.NO_ERROR)
                {
                    if (result != NativeMethods.DnsQueryErrorCode.DNS_ERROR_RCODE_NAME_ERROR && result != NativeMethods.DnsQueryErrorCode.DNS_INFO_NO_RECORDS)
                    {
                        throw new DnsResolverException(string.Format(CultureInfo.InvariantCulture, "DnsQuery failed with {0}.", new object[]
                        {
                            result
                        }));
                    }
                }
                else
                {
                    IntPtr queryResult = queryResultList;
                    while (!queryResult.Equals(IntPtr.Zero))
                    {
                        NativeMethods.CNameRecord cnameRecord = (NativeMethods.CNameRecord)Marshal.PtrToStructure(queryResult, typeof(NativeMethods.CNameRecord));
                        if (cnameRecord.wType == 5)
                        {
                            cnameTarget.Add(Marshal.PtrToStringAuto(cnameRecord.pNameHost));
                        }
                        queryResult = cnameRecord.pNext;
                    }
                }
            }
            catch (Win32Exception ex)
            {
                throw new DnsResolverException(string.Format(CultureInfo.InvariantCulture, "DnsQuery failed with {0}.", new object[]
                {
                    ex.Message
                }), ex);
            }
            finally
            {
                if (queryResultList != IntPtr.Zero)
                {
                    NativeMethods.DnsRecordListFree(queryResultList, 0);
                }
            }
            return cnameTarget;
        }
    }
}