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
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection;
using AutoInstallation.Contract.ActiveDirectory;
using AutoInstallationCommon.ActiveDirectory;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;
using NameType = AutoInstallation.Contract.ActiveDirectory.NameType;

namespace AutoInstallationCommon.Utility.Handler
{
    public class CommonCheckUserHandler
    {
        public delegate void CheckFailed(string msg);

        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public event CheckFailed OnCheckUserFailed;
        public event CheckFailed OnCheckPasswordFailed;

        public bool VerifyExist(NameInfo nameInfo)
        {
            var ret = false;
            try
            {
                if (string.IsNullOrEmpty(nameInfo.Domain) || nameInfo.Domain == "." ||
                    nameInfo.Domain == Environment.MachineName)
                    using (var pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
                    {
                        nameInfo.FullName = pc.Name + "\\" + nameInfo.UserName;
                        var up = UserPrincipal.FindByIdentity(pc, nameInfo.UserName);
                        ret = up != null;
                    }
                else
                    using (var pc = new PrincipalContext(ContextType.Domain, nameInfo.Domain))
                    {
                        var up = UserPrincipal.FindByIdentity(pc, nameInfo.UserName);
                        ret = up != null;
                    }

                //localContext.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_CHECKUSERERROR, ex.ToString());
                ret = false;
            }

            return ret;
        }

        public bool VerifyPassword(NameInfo nameInfo, string password)
        {
            var ret = false;
            try
            {
                if (string.IsNullOrEmpty(nameInfo.Domain) || nameInfo.Domain == "." ||
                    nameInfo.Domain == Environment.MachineName)
                    using (var pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
                    {
                        nameInfo.FullName = pc.Name + "\\" + nameInfo.UserName;
                        return pc.ValidateCredentials(nameInfo.UserName, password);
                    }

                using (var pc = new PrincipalContext(ContextType.Domain, nameInfo.Domain))
                {
                    return pc.ValidateCredentials(nameInfo.UserName, password);
                }

                //localContext.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_CHECKUSERERROR, ex.ToString());
                ret = false;
            }

            return ret;
        }

        /// <summary>
        ///     由于现在的设计没有people picker控件，所以如果用户不输入域名就在local中找
        ///     如果要支持people picker那么这里要改
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool VerifyLocalAdmin(NameInfo nameInfo, string password)
        {
            var ret = false;
            logger.Info(LOGRESX.COMMONUTILITYLOG_STARTCHECKUSER, nameInfo.FullName);
            try
            {
                //PrincipalContext localContext = new PrincipalContext(ContextType.Machine);
                //GroupPrincipal gp = GroupPrincipal.FindByIdentity(localContext, "Administrators");
                //logger.Info(gp.Name);
                //logger.Info(gp.DisplayName);
                //logger.Info(gp.SamAccountName);
                //logger.Info(gp.UserPrincipalName);
                var users = GetUserInAdministrators();
                if (string.IsNullOrEmpty(nameInfo.Domain) || nameInfo.Domain == "." ||
                    nameInfo.Domain == Environment.MachineName)
                    using (var pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
                    {
                        nameInfo.FullName = pc.Name + "\\" + nameInfo.UserName;
                        ret = VerifyUser(pc, nameInfo.UserName, password, users);
                    }
                else
                    try
                    {
                        using (var pc = new PrincipalContext(ContextType.Domain, nameInfo.Domain))
                        {
                            ret = VerifyUser(pc, nameInfo.UserName, password, users);
                        }
                    }
                    catch (PrincipalServerDownException ex)
                    {
                        logger.Error(LOGRESX.COMMONUTILITYLOG_CONNECTDOMAINERROR, ex.ToString());
                        //OnCheckUserFailed(GUIRESX.COMMONUTILITY_CHECKUSERERROR);
                    }

                //localContext.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_CHECKUSERERROR, ex.ToString());
                ret = false;
                //if (OnCheckPasswordFailed != null) OnCheckPasswordFailed(GUIRESX.COMMONUTILITY_UNKNOWNCHECKUSERERROR);
            }

            return ret;
        }

        private bool VerifyUser(GroupPrincipal gp, PrincipalContext pc, string name, string password)
        {
            var ret = false;
            var up = UserPrincipal.FindByIdentity(pc, name);
            if (up != null)
            {
                if (up.IsMemberOf(gp))
                {
                    if (pc.ValidateCredentials(name, password))
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                        //if (OnCheckPasswordFailed != null)
                        //    OnCheckPasswordFailed(GUIRESX.COMMONUTILITY_CHECKPASSWORDERROR);
                    }
                }
                else
                {
                    ret = false;
                    //if (OnCheckUserFailed != null) OnCheckUserFailed(GUIRESX.COMMONUTILITY_USERPERMISSIONERROR);
                }
            }
            else
            {
                ret = false;
                //if (OnCheckUserFailed != null) OnCheckUserFailed(GUIRESX.COMMONUTILITY_CHECKUSERERROR);
            }

            return ret;
        }

        private bool VerifyUser(PrincipalContext pc, string name, string password, List<string> users)
        {
            var ret = false;
            var up = UserPrincipal.FindByIdentity(pc, name);
            if (up != null)
            {
                if (users.Any(item => item.ToLower().IndexOf(pc.Name.ToLower() + "/" + name.ToLower()) != -1))
                {
                    if (pc.ValidateCredentials(name, password))
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                        //if (OnCheckPasswordFailed != null)
                        //    OnCheckPasswordFailed(GUIRESX.COMMONUTILITY_CHECKPASSWORDERROR);
                    }
                }
                else
                {
                    ret = false;
                    //if (OnCheckUserFailed != null) OnCheckUserFailed(GUIRESX.COMMONUTILITY_USERPERMISSIONERROR);
                }
            }
            else
            {
                ret = false;
                //if (OnCheckUserFailed != null) OnCheckUserFailed(GUIRESX.COMMONUTILITY_CHECKUSERERROR);
            }

            return ret;
        }

        public static string CheckUser(string domain, string username, string password, string indentity)
        {
            var result = string.Empty;
            try
            {
                result = CheckUserByActiveDirectoryObject(domain, username, password, indentity);
                if (string.IsNullOrEmpty(result))
                    using (var pc = new PrincipalContext(ContextType.Domain, domain, username, password))
                    {
                        var up = UserPrincipal.FindByIdentity(pc, indentity);
                        if (up != null)
                            result = pc.Name + "\\" + up.SamAccountName;
                        else
                            logger.Error("Could not find user:{0}.", username);
                    }
            }
            catch (Exception ex)
            {
                logger.Error("Check user failed.Error:{0}", ex.ToString());
            }

            return result;
        }

        public static string CheckUserByActiveDirectoryObject(string domain, string username, string password,
            string indentity)
        {
            var result = string.Empty;
            try
            {
                List<ActiveDirectoryObject> activeDirectoryObjectList = null;
                activeDirectoryObjectList = new List<ActiveDirectoryObject>();
                var domainControl = new ActiveDirectoryDomain(domain, username, password);
                var searcher = domainControl.CreateDefaultSearcher();
                foreach (SearchResult sr in searcher.WildcardYieldSearch(indentity))
                {
                    var item = domainControl.CreateEntry(sr).ToActiveDirectoryObject();
                    activeDirectoryObjectList.Add(item);
                }

                if (activeDirectoryObjectList.Count > 0) result = activeDirectoryObjectList[0].MSDS_PrincipalName;
            }
            catch (Exception ex)
            {
                logger.Error("CheckUserByActiveDirectoryObject failed.Error:{0}", ex);
            }

            return result;
        }

        public static NameInfo AnalyzeName(string name)
        {
            string[] domainAndName = null;
            var nameInfo = new NameInfo();
            nameInfo.FullName = name;

            if (name.Contains("\\"))
            {
                domainAndName = name.Split('\\');
                nameInfo.Type = NameType.Classic;
                nameInfo.Domain = domainAndName[0];
                nameInfo.UserName = domainAndName[1];
            }
            else if (name.Contains("@")) //用户名中不存在@,组名中可能存在,带@的组名，推荐使用domain\group形式 
            {
                var result = new string[2];
                var lastAt = name.LastIndexOf('@');
                nameInfo.UserName = name.Substring(0, lastAt);
                nameInfo.Domain = name.Substring(lastAt + 1);
                nameInfo.Type = NameType.UPN;
            }
            else
            {
                nameInfo.Type = NameType.SingleName;
                nameInfo.UserName = name;
            }

            return nameInfo;
        }

        public static NameInfo CheckAndAnalyzeName(string name)
        {
            string[] domainAndName = null;
            var nameInfo = new NameInfo();
            nameInfo.FullName = name;

            if (name.Contains("\\"))
            {
                domainAndName = name.Split('\\');
                nameInfo.Type = NameType.Classic;
                nameInfo.Domain = domainAndName[0];
                nameInfo.UserName = domainAndName[1];
            }
            else
            {
                nameInfo = null;
            }

            return nameInfo;
        }

        private static List<string> GetUserInAdministrators()
        {
            var ret = new List<string>();
            var localRoot = new DirectoryEntry("WinNT://" + Environment.MachineName + ",Computer");
            DirectoryEntry group = null;

            group = localRoot.Children.Find("Administrators", "Group");

            var members = group.Invoke("Members", null);
            foreach (var member in (IEnumerable) members)
            {
                var userInGroup = new DirectoryEntry(member);
                ret.Add(userInGroup.Path);
            }

            return ret;
        }
    }
}