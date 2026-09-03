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



namespace AvePoint.GCommon
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.ComponentModel;

    #endregion

    /// <summary>
    /// 本类是和LSA打交道的工具类
    /// </summary>
    public class LsaUtility
    {
        /// <summary>
        /// 根据提供的privilege name，找到在这个user rights下的所有user。
        /// </summary>
        /// <param name="privilegeName"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateAccountsWithUserRight(string privilegeName)
        {
            IntPtr policyHandle = IntPtr.Zero;
            List<string> accounts = new List<string>();

            try
            {
                //combine all policies
                int access = (int) (
                                       Win32Native.LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                       Win32Native.LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                       Win32Native.LSA_AccessPolicy.POLICY_NOTIFICATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                       Win32Native.LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                       Win32Native.LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION);

                //these attributes are not used, but LsaOpenPolicy wants them to exists
                Win32Native.LSA_OBJECT_ATTRIBUTES ObjectAttributes = new Win32Native.LSA_OBJECT_ATTRIBUTES();
                ObjectAttributes.Length = 0;
                ObjectAttributes.RootDirectory = IntPtr.Zero;
                ObjectAttributes.Attributes = 0;
                ObjectAttributes.SecurityDescriptor = IntPtr.Zero;
                ObjectAttributes.SecurityQualityOfService = IntPtr.Zero;

                //initialize an empty Unicode-string
                Win32Native.LSA_UNICODE_STRING systemName = new Win32Native.LSA_UNICODE_STRING();

                //get a policy handle
                uint resultPolicy = Win32Native.LsaOpenPolicy(ref systemName, ref ObjectAttributes, access,
                                                              out policyHandle);
                int winErrorCode = (int) Win32Native.LsaNtStatusToWinError(resultPolicy);
                if (winErrorCode != 0)
                {
                    throw new Win32Exception(winErrorCode);
                }

                Win32Native.LSA_UNICODE_STRING userRight = new Win32Native.LSA_UNICODE_STRING();
                userRight.Buffer = Marshal.StringToHGlobalUni(privilegeName);
                userRight.Length = (UInt16) (privilegeName.Length*UnicodeEncoding.CharSize);
                userRight.MaximumLength = (UInt16) ((privilegeName.Length + 1)*UnicodeEncoding.CharSize);

                IntPtr rightsArray = IntPtr.Zero;
                uint count = 0;
                uint result = Win32Native.LsaEnumerateAccountsWithUserRight(policyHandle, userRight, out rightsArray,
                                                                            out count);
                winErrorCode = (int) Win32Native.LsaNtStatusToWinError(result);
                if (winErrorCode != 0)
                {
                    throw new Win32Exception(winErrorCode);
                }

                IntPtr ptr = rightsArray;
                for (int i = 0; i < count; i++)
                {

                    Win32Native.SID_NAME_USE sidUse = Win32Native.SID_NAME_USE.SidTypeUnknown;
                    StringBuilder name = new StringBuilder(0x100);
                    StringBuilder domain = new StringBuilder(0x100);
                    int cbDomainName = 0x100;
                    int cbName = 0x100;

                    Win32Native.LSA_ENUMERATION_INFORMATION structure =
                        (Win32Native.LSA_ENUMERATION_INFORMATION)
                        Marshal.PtrToStructure(ptr, typeof (Win32Native.LSA_ENUMERATION_INFORMATION));

                    byte[] sid = new byte[28];
                    Marshal.Copy(structure.Sid, sid, 0, 28);

                    if (Win32Native.LookupAccountSid(null, sid, name, ref cbName, domain, ref cbDomainName,
                                                     ref sidUse))
                    {
                        accounts.Add(string.Format("{0}\\{1}", domain, name));
                    }

                    ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(typeof (Win32Native.LSA_ENUMERATION_INFORMATION)));
                }
            }
            finally
            {
                if (policyHandle != IntPtr.Zero)
                {
                    Win32Native.LsaClose(policyHandle);
                }
            }

            return accounts;
        }

        /// <summary>
        /// 根据提供的User name，找到这个user所在的user rights组。
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetUserRightsWithAccount(string accountName)
        {
            List<string> userRights = new List<string>();
            IntPtr policyHandle = IntPtr.Zero;
            IntPtr sid = IntPtr.Zero;
            int sidSize = 0;
            StringBuilder domainName = new StringBuilder();
            int nameSize = 0;
            int accountType = 0;

            try
            {
                //initialize an empty Unicode-string
                Win32Native.LSA_UNICODE_STRING systemName = new Win32Native.LSA_UNICODE_STRING();

                //these attributes are not used, but LsaOpenPolicy wants them to exists
                Win32Native.LSA_OBJECT_ATTRIBUTES ObjectAttributes = new Win32Native.LSA_OBJECT_ATTRIBUTES();
                ObjectAttributes.Length = 0;
                ObjectAttributes.RootDirectory = IntPtr.Zero;
                ObjectAttributes.Attributes = 0;
                ObjectAttributes.SecurityDescriptor = IntPtr.Zero;
                ObjectAttributes.SecurityQualityOfService = IntPtr.Zero;

                //combine all policies
                int access = (int) (
                                       Win32Native.LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                       Win32Native.LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                       Win32Native.LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                       Win32Native.LSA_AccessPolicy.POLICY_NOTIFICATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                       Win32Native.LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                       Win32Native.LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                       Win32Native.LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                       Win32Native.LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                   );

                //get required buffer size
                Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize,
                                              ref accountType);
                //allocate buffers
                domainName = new StringBuilder(nameSize);
                sid = Marshal.AllocHGlobal(sidSize);
                //lookup the SID for the account
                bool result = Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName,
                                                            ref nameSize, ref accountType);
                if (!result)
                {
                    throw new Win32Exception(Win32Native.GetLastError());
                }

                //get a policy handle
                uint resultPolicy = Win32Native.LsaOpenPolicy(ref systemName, ref ObjectAttributes, access,
                                                              out policyHandle);
                int winErrorCode = (int) Win32Native.LsaNtStatusToWinError(resultPolicy);
                if (winErrorCode != 0)
                {
                    throw new Win32Exception(winErrorCode);
                }

                IntPtr rightsArray = IntPtr.Zero;
                uint count = 0;
                uint result2 = Win32Native.LsaEnumerateAccountRights(policyHandle, sid, out rightsArray, out count);
                winErrorCode = (int) Win32Native.LsaNtStatusToWinError(result2);
                if (winErrorCode != 0)
                {
                    throw new Win32Exception(winErrorCode);
                }

                IntPtr ptr = rightsArray;
                for (int i = 0; i < count; i++)
                {
                    Win32Native.LSA_UNICODE_STRING structure =
                        (Win32Native.LSA_UNICODE_STRING)
                        Marshal.PtrToStructure(ptr, typeof (Win32Native.LSA_UNICODE_STRING));
                    char[] destination = new char[structure.Length/sizeof (char)];
                    Marshal.Copy(structure.Buffer, destination, 0, destination.Length);
                    string userRight = new string(destination, 0, destination.Length);
                    userRights.Add(userRight);

                    ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(typeof (Win32Native.LSA_UNICODE_STRING)));
                }
            }
            finally
            {
                if (policyHandle != IntPtr.Zero)
                {
                    Win32Native.LsaClose(policyHandle);
                }
                if (sid != IntPtr.Zero)
                {
                    Win32Native.FreeSid(sid);
                }
            }

            return userRights;
        }

        /// <summary>
        /// 这个函数用来给一个用户添加 user rights
        /// </summary>
        /// <param name="targetAccountName">the account name we will add user rights</param>
        /// <param name="privilegeName">user rights eg: SeBatchLogonRight</param>
        public static void SetUserRights(string accountName, string privilegeName)
        {
            IntPtr sid = IntPtr.Zero;
            int sidSize = 0;
            StringBuilder domainName = new StringBuilder();
            int nameSize = 0;
            int accountType = 0;
            IntPtr policyHandle = IntPtr.Zero;

            try
            {
                //get required buffer size
                Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize,
                                              ref accountType);
                //allocate buffers
                domainName = new StringBuilder(nameSize);
                sid = Marshal.AllocHGlobal(sidSize);
                //lookup the SID for the account
                bool result = Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName,
                                                            ref nameSize, ref accountType);
                if (!result)
                {
                    throw new Win32Exception(Win32Native.GetLastError());
                }
                else
                {
                    //initialize an empty Unicode-string
                    Win32Native.LSA_UNICODE_STRING systemName = new Win32Native.LSA_UNICODE_STRING();
                    //combine all policies
                    int access = (int) (
                                           Win32Native.LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                           Win32Native.LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                           Win32Native.LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                           Win32Native.LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                           Win32Native.LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                           Win32Native.LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                           Win32Native.LSA_AccessPolicy.POLICY_NOTIFICATION |
                                           Win32Native.LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                           Win32Native.LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                           Win32Native.LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                           Win32Native.LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                           Win32Native.LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                           Win32Native.LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                       );
                    //these attributes are not used, but LsaOpenPolicy wants them to exists
                    Win32Native.LSA_OBJECT_ATTRIBUTES ObjectAttributes = new Win32Native.LSA_OBJECT_ATTRIBUTES();
                    ObjectAttributes.Length = 0;
                    ObjectAttributes.RootDirectory = IntPtr.Zero;
                    ObjectAttributes.Attributes = 0;
                    ObjectAttributes.SecurityDescriptor = IntPtr.Zero;
                    ObjectAttributes.SecurityQualityOfService = IntPtr.Zero;
                    //get a policy handle
                    uint resultPolicy = Win32Native.LsaOpenPolicy(ref systemName, ref ObjectAttributes, access,
                                                                  out policyHandle);
                    int winErrorCode = (int) Win32Native.LsaNtStatusToWinError(resultPolicy);
                    if (winErrorCode != 0)
                    {
                        throw new Win32Exception(winErrorCode);
                    }
                    else
                    {
                        //Now that we have the SID an the policy,we can add rights to the account.
                        //initialize an Unicode-string for the privilege name
                        Win32Native.LSA_UNICODE_STRING[] userRights = new Win32Native.LSA_UNICODE_STRING[1];
                        userRights[0] = new Win32Native.LSA_UNICODE_STRING();
                        userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                        userRights[0].Length = (UInt16) (privilegeName.Length*UnicodeEncoding.CharSize);
                        userRights[0].MaximumLength = (UInt16) ((privilegeName.Length + 1)*UnicodeEncoding.CharSize);
                        //add the right to the account
                        uint res = Win32Native.LsaAddAccountRights(policyHandle, sid, userRights, 1);
                        winErrorCode = (int) Win32Native.LsaNtStatusToWinError(res);
                        if (winErrorCode != 0)
                        {
                            throw new Win32Exception(winErrorCode);
                        }
                    }
                }
            }
            finally
            {
                if (policyHandle != IntPtr.Zero)
                {
                    Win32Native.LsaClose(policyHandle);
                    policyHandle = IntPtr.Zero;
                }
                if (sid != IntPtr.Zero)
                {
                    Win32Native.FreeSid(sid);
                    sid = IntPtr.Zero;
                }
            }
        }
    }
}
