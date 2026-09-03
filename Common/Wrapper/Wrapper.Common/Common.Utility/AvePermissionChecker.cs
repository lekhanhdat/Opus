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
using System.Reflection;
using System.Security.Principal;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AvePermissionChecker
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region Farm Admin Permission
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName">If userName is null or empty，Check Current User</param>
        /// <param name="omFactory"></param>
        /// <returns></returns>
        public static bool CheckFarmAdmin(string userName, AveObjectModelFactory omFactory)
        {
            bool hasPermssion = false;
            try
            {
                IAveFarm farm = omFactory.CreateFarm();
                if (farm == null)
                {
                    log.Warn("Cannot get local farm while doing CheckFarmAdmin.");
                    return hasPermssion;
                }
                if (string.IsNullOrEmpty(userName))
                {
                    //if (farm.CurrentUserIsAdministrator())
                    //{
                    //    hasPermission = true;
                    //}
                }
                else
                {
                    using (IAveSite adminSite = omFactory.CreateAdministrationWebApplication().Local.Sites[0])
                    {
                        using (IAveWeb adminWeb = adminSite.RootWeb)
                        {
                            IAveGroup farmAdminGroup = adminWeb.Groups["Farm Administrators"];
                            if (farmAdminGroup != null)
                            {
                                foreach (IAveUser user in farmAdminGroup.Users)
                                {
                                    if (user.LoginName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasPermssion = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error happened when Check FarmAdmin.Reason:{0}.", ex.ToString());
            }
            return hasPermssion;
        }
        #endregion

        #region SQL DataBase Permission
        /// <summary>
        /// Check user whether has permission for this content database
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="dataSource"></param>
        /// <param name="databaseName"></param>
        /// <param name="sRole"></param>
        /// <param name="databaseRole"></param>
        /// <returns></returns>
        public static bool CheckSiteSQLPermission(string userName, ServerRole sRole, DatabaseRole databaseRole, IAveCommonQueryService aveCommonQueryService)
        {
            WindowsIdentity identity = GetUserIdentity(userName);
            userName = identity.Name;
            byte[] sid = (byte[])GetPropertyValue(identity.User, "BinaryForm");
            if (aveCommonQueryService.CheckDatabaseServerRole(userName, sRole, sid))
            {
                return true;
            }
            if (!aveCommonQueryService.CheckDatabaseRole(userName, databaseRole, sid))
            {
                return false;
            }
            return true;
        }



        private static WindowsIdentity GetUserIdentity(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return WindowsIdentity.GetCurrent();
            }
            else
            {
                if (userName.Contains("\\"))
                {
                    userName = userName.Substring(userName.IndexOf('\\') + 1);
                }
                return new WindowsIdentity(userName);
            }
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            BindingFlags flags = BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public
                                                                  | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                                                                  | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase;
            Type objType = obj.GetType();
            PropertyInfo property = objType.GetProperty(propertyName, flags);
            if (property != null)
            {
                return property.GetValue(obj, null);
            }
            return null;
        }
        #endregion

        #region All Zone Full Control Permission
        public static bool CheckFullControlUserPolicyForAllZone(string userName, string webAppUrl, AveObjectModelFactory omFactory)
        {
            try
            {
                IAveWebApplication webApp = omFactory.CreateWebApplication(webAppUrl);
                return CheckFullControlUserPolicyForAllZone(userName, webApp);
            }
            catch (Exception ex)
            {
                log.Warn("Error happened when get WebApplication.WebApp Url:{0}.Reason:{1}.", webAppUrl, ex.ToString());
            }
            return false;
        }

        public static bool CheckFullControlUserPolicyForAllZone(string userName, IAveWebApplication webApp)
        {
            bool checkStatus = false;
            try
            {
                IAvePolicyCollection policyCollection = webApp.Policies;
                foreach (IAvePolicy policy in policyCollection)
                {
                    string temp = policy.UserName;
                    if (temp.Contains("|"))
                    {
                        temp = temp.Substring(temp.IndexOf('|') + 1);
                    }
                    if (!temp.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    else
                    {
                        long grantRightsMask = 0;
                        long denyRightsMask = 0;
                        foreach (IAvePolicyRole role in policy.PolicyRoleBindings)
                        {
                            grantRightsMask = grantRightsMask | (long)role.GrantRightsMask;
                            denyRightsMask = denyRightsMask | (long)role.DenyRightsMask;
                        }
                        if (grantRightsMask == (long)AveBasePermissions.FullMask && denyRightsMask == (long)AveBasePermissions.EmptyMask)
                        {
                            checkStatus = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error happened when Check FullControl UserPolicy For AllZone.UserName:{0}.Reason:{1}.", userName, ex.ToString());
            }
            return checkStatus;
        }
        #endregion
    }

    public enum ServerRole
    {
        none,
        sysadmin,
        dbcreator,
        securityadmin,
    }

    public enum DatabaseRole
    {
        none,
        db_owner,
    }
}