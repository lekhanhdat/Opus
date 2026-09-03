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
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;

    #endregion

    /// <summary>
    /// 本类是和LSA打交道的工具类
    /// </summary>
    public class LsaUtility
    {
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
                Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);
                //allocate buffers
                domainName = new StringBuilder(nameSize);
                sid = Marshal.AllocHGlobal(sidSize);
                //lookup the SID for the account
                bool result = Win32Native.LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);
                if (!result)
                {
                    throw new Win32Exception(Win32Native.GetLastError());
                }
                else
                {
                    //initialize an empty Unicode-string
                    Win32Native.LSA_UNICODE_STRING systemName = new Win32Native.LSA_UNICODE_STRING();
                    //combine all policies
                    int access = (int)(
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
                    uint resultPolicy = Win32Native.LsaOpenPolicy(ref systemName, ref ObjectAttributes, access, out policyHandle);
                    int winErrorCode = (int)Win32Native.LsaNtStatusToWinError(resultPolicy);
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
                        userRights[0].Length = (UInt16)(privilegeName.Length * UnicodeEncoding.CharSize);
                        userRights[0].MaximumLength = (UInt16)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);
                        //add the right to the account
                        uint res = Win32Native.LsaAddAccountRights(policyHandle, sid, userRights, 1);
                        winErrorCode = (int)Win32Native.LsaNtStatusToWinError(res);
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
