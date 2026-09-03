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
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using System.Text.RegularExpressions;
    #endregion

    public interface IAccountManager
    {
        bool IsEnable();

        bool IsMemeberAlive(string loginName);

        List<UserDetail> GetUsers(string username, AccountSearchFlag flag, int findUserQuota = 200);
        List<UserDetail> GetUsers(string username);

        UserDetail GetUser(string username);
        UserDetail GetUser(string username, AccountSearchFlag flag);

        List<string> GetParentGroupCollection(string userName);
        /// <summary>
        ///     will not change the loginName when can not find the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        bool ExtractUserName(string username, ref string loginName);
        bool ExtractUserName(string username, ref string loginName, AccountSearchFlag flag);

        bool CheckUser(string username);
        bool CheckUser(string username, AccountSearchFlag flag);

        List<UserDetail> GetMembersInGroup(string groupName);
    }

    public class UserLoginNamePrefix
    {
        public static string SeparatedChar = "|";

        // the local user is same with the ad user
        public static string ADUserPrefix = "i:0#.w";
        public static string ADUserGroupName = "AD User";
        //the local group is same with the ad group
        public static string ADGroupPrefix = "c:0+.w";
        public static string ADGroupGroupName = "AD Group";
        //the NT or EveryOne Prefix 
        public static string NTPrefix = "c:0!.s";
        public static string EveryOnePrefix = "c:0(.s";

        public static string SharepointGroupUserType = "Sharepoint Group";
        public static string SharepointUserUserType = "Sharepoint User";                                               
        public static string FormUserPrefix = "i:0#.f";
        public static string FormRolePrefix = "c:0-.f";
        public static string FormUserGroupName = "Form User";
        public static string FormRoleGroupName = "Form Role";
        public static string RemoveLoginNamePrifix(string loginName)
        {
            if (StatWithLoginNamePrifix(loginName))
            {
                loginName = loginName.Substring(7, loginName.Length - 7);
            }
            return loginName;
        }

        public static string AddLoginNamePrifix(string loginName, AccountType type)
        {

            if (!StatWithLoginNamePrifix(loginName))
            {
                string prefix = string.Empty;
                switch (type)
                {
                    case AccountType.ADUser:
                    case AccountType.LocalUser:
                        prefix = UserLoginNamePrefix.ADUserPrefix + UserLoginNamePrefix.SeparatedChar;
                        break;
                    case AccountType.LocalGroup:
                    case AccountType.ADGroup:
                        prefix = UserLoginNamePrefix.ADGroupPrefix + UserLoginNamePrefix.SeparatedChar;
                        break;
                    case AccountType.FormRole:
                        prefix = UserLoginNamePrefix.FormRolePrefix + UserLoginNamePrefix.SeparatedChar;
                        break;
                    case AccountType.FormUser:
                        prefix = UserLoginNamePrefix.FormUserPrefix + UserLoginNamePrefix.SeparatedChar;
                        break;
                    default:
                        break;
                }
                loginName = prefix + loginName;
            }
            return loginName;
        }

        public static bool StatWithLoginNamePrifix(string loginName)
        {
            if (loginName.StartsWith(UserLoginNamePrefix.ADGroupPrefix, StringComparison.OrdinalIgnoreCase)
                ||loginName.StartsWith(UserLoginNamePrefix.FormRolePrefix, StringComparison.OrdinalIgnoreCase)
                ||loginName.StartsWith(UserLoginNamePrefix.ADUserPrefix, StringComparison.OrdinalIgnoreCase)
                || loginName.StartsWith(UserLoginNamePrefix.NTPrefix, StringComparison.OrdinalIgnoreCase)
                || loginName.StartsWith(UserLoginNamePrefix.EveryOnePrefix, StringComparison.OrdinalIgnoreCase)
                ||loginName.StartsWith(UserLoginNamePrefix.FormUserPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (loginName.IndexOf("|", StringComparison.OrdinalIgnoreCase) == 6)
            {
                string pattern = @"^[sS]-1-\d-\d*-\d*-\d*-\d*-\d*$";
                if (Regex.Matches(loginName, "[|\\\\]").Count >= 2)
                {
                    return true;
                }
                else if (Regex.IsMatch(loginName.Substring(7),pattern))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///将带有sharepoint标示的前缀去掉，因为通过LDAP，Form以及ADFS check的userdetail的loginName不会携带前缀，并且已经封装了groupName
        ///所以此方法只有check sharepoint user的时候调用
        /// </summary>
        /// <param name="ud">如果不带前缀，则不是cba认证，如果带，通过前缀确定是ad还是fba还是其他，如果是fba，除了截取前缀，还要把|改为：</param>
        public static UserDetail GetOriginalUserDetail(UserDetail ud)
        {
            string prefix = string.Empty;
            string provider = string.Empty;
            if (StatWithLoginNamePrifix(ud.LoginName))//SPUser not init ProviderName attribute
            {
                ud.SPLoginName = ud.LoginName;
                prefix = ud.LoginName.Substring(0, 6);
                ud.LoginName = ud.LoginName.Substring(7, ud.LoginName.Length - 7);
                if (prefix.Equals(FormUserPrefix, StringComparison.OrdinalIgnoreCase) || prefix.Equals(FormRolePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    ud.LoginName = ud.LoginName.Replace("|", ":");
                    if (string.IsNullOrEmpty(ud.UserType))
                    {
                        ud.UserType = prefix.Equals(FormUserPrefix, StringComparison.OrdinalIgnoreCase) ? UserLoginNamePrefix.FormUserGroupName : UserLoginNamePrefix.FormRoleGroupName;
                    }
                }
                else if (ud.LoginName.Contains("|"))//custom provider user
                {
                    provider = ud.LoginName.Split(new char[] { '|' })[0];
                    ud.ProviderName = provider;
                    if (string.IsNullOrEmpty(ud.UserType))
                    {
                        ud.UserType = string.Format("{0}[{1}]", provider, prefix);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(ud.UserType))
                    {
                        ud.UserType = prefix.Equals(ADUserPrefix, StringComparison.OrdinalIgnoreCase) ? UserLoginNamePrefix.ADUserGroupName : UserLoginNamePrefix.ADGroupGroupName;
                    }
                }
                ud.Prefix = prefix;
            }
            else
            {
                ud.SPLoginName = GetSPLoginName(ud);
            }
            return ud;
        }

        /// <summary>
        /// 从server端传来，在判断当前webapp是cba认证的前提下调用此方法，如果prefix不为空，更改LoginName
        /// </summary>
        /// <param name="ud"></param>
        public static UserDetail GetSPUserDetail(UserDetail ud)
        {
            if (!string.IsNullOrEmpty(ud.Prefix))//CBA user
            {
                if (ud.Prefix.Equals(FormUserPrefix, StringComparison.OrdinalIgnoreCase) || ud.Prefix.Equals(FormRolePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    ud.LoginName = ud.LoginName.Replace(":", "|");
                }
                ud.LoginName = string.Format("{0}|{1}", ud.Prefix, ud.LoginName);
            }
            return ud;
        }

        /// <summary>
        /// 当Prefix为空时，为AD User返回UserDetail的loginName;Prefix不为空时返回带Prefix的Login Name
        /// </summary>
        /// <param name="ud"></param>
        public static string GetSPLoginName(UserDetail ud)
        {
            string SPLoginName = ud.LoginName;
            if (!string.IsNullOrEmpty(ud.Prefix))//CBA user
            {
                if (ud.Prefix.Equals(FormUserPrefix, StringComparison.OrdinalIgnoreCase) || ud.Prefix.Equals(FormRolePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    SPLoginName = SPLoginName.Replace(":", "|");
                }
                SPLoginName = string.Format("{0}|{1}", ud.Prefix, SPLoginName);
            }
            return SPLoginName;
        }
    }
}
