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
using System.Text;

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
        [Obsolete("This method will be deprecated and removed later.")]
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

        /// <summary>
        /// Invoke SharePoint API.
        /// </summary>
        /// <param name="userName">Check Current User</param>
        /// <param name="omFactory"></param>
        /// <returns></returns>
        public static bool CheckFarmAdmin(AveObjectModelFactory omFactory)
        {
            try
            {
                IAveFarm farm = omFactory.CreateFarm().Local;
                return farm.CurrentUserIsAdministrator();
            }
            catch (Exception ex)
            {
                log.Warn("Error happened when Check FarmAdmin.Reason:{0}.", ex.ToString());
            }
            return false;
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
            using (WindowsIdentity identity = GetUserIdentity(userName))
            {
                userName = identity.Name;
                StringBuilder sbuilder = new StringBuilder();
                byte[] sid = (byte[])GetPropertyValue(identity.User, "BinaryForm");
                #region Check Server Role
                if (aveCommonQueryService.CheckDatabaseServerRole(userName, sRole, sid))
                {
                    return true;//如果User直接具备ServerRole，即可确认有权限。
                }
                else
                {
                    var userGroups = identity.Groups;//此处有可能有效率问题。
                    foreach (var group in userGroups)
                    {
                        var account = group.Translate(typeof(NTAccount)) as NTAccount;
                        sbuilder.Append(account.Value);
                        sbuilder.Append(@"','");
                    }
                    sbuilder.Length -= 3;//去除最后的','
                    var gourpNames = sbuilder.ToString();
                    if (aveCommonQueryService.CheckDatabaseServerRole(gourpNames, sRole, sid))
                    {
                        return true;//如果User没有权限，去确认User所属的AD Group是否有权限，由于不能直接取出AD Group，故会取User的所有Group.如果任意一个Group具有权限，认为user也具有权限。
                    }
                }
                #endregion
                #region Check DB Role
                if (aveCommonQueryService.CheckDatabaseRole(userName, databaseRole, sid))
                {
                    return true;//没有Server Role权限但又DB Role权限，确认为有权限。
                }
                else
                {
                    if (sbuilder.Length > 0)
                    {
                        var groupNames = sbuilder.ToString();
                        return aveCommonQueryService.CheckDatabaseRole(groupNames, databaseRole, sid);//ServerRole无权限后check是否具有DB权限，同样包括当前User的check和User 所属AD Group的Check。
                    }
                    return false;
                }
                #endregion
            }
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

        public static bool CheckFullControlUserPolicyForAllZone(string userName, IAveWebApplication webApp, bool includeModify = false)
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
                        if (includeModify)
                        {
                            if ((grantRightsMask == (long)AveBasePermissions.FullMask || grantRightsMask == 4611688150878591999) && denyRightsMask == (long)AveBasePermissions.EmptyMask)
                            {
                                checkStatus = true;
                                break;
                            }
                        }
                        else
                        {
                            if (grantRightsMask == (long)AveBasePermissions.FullMask && denyRightsMask == (long)AveBasePermissions.EmptyMask)
                            {
                                checkStatus = true;
                                break;
                            }
                        }
                    }
                }
                if (!checkStatus)
                {
                    log.Debug("Can not find the user or group named \"{0}\" in Web Application policies. It doesn't have the Full Control permission. Start to check the upper groups permission.", userName);
                    using (var userIdentity = GetUserIdentity(userName))
                    {
                        var userGroups = userIdentity.Groups;
                        foreach (var group in userGroups)
                        {
                            var account = group.Translate(typeof(NTAccount)) as NTAccount;
                            if (CheckFullControlUserPolicyForAllZone(account.Value, webApp, includeModify))
                            {
                                checkStatus = true;
                                break;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.Warn("Error happened when Check FullControl UserPolicy For AllZone.UserName:{0}.Reason:{1}.", userName, ex);
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
        SPDataAccess
    }
}