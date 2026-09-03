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
using System.Linq;
using System.Collections.Generic;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore;

namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPMembers
    {
        System.Collections.Generic.List<string> AllGroups { get; }
        string ConvertDomainGroupAcountToSid(string account);
        int CreateAndFindMemberId(string oldLoginName);
        void Dispose();
        IAvePrincipal FindMember(int id, bool createIfNotExist);
        IAvePrincipal FindMember(int id, bool createIfNotExist, bool useDefaultUser);

        int FindMemberId(int oldUserId, bool createIfNotExist);
        int FindMemberId(int oldUserId);
        string GetMappingUserLogin(string login);
        string GetMappingUserLogin(string login, bool isDomainGroup, bool needMapping);
        string GetMappingUserLogin(string login, bool needMapping);
        object GetMemberObjectByLogin(string login);
        IAveUser GetOrAddUser(string login);
        IAvePrincipal GetOrAddPrincipal(Object member, bool useDefaultUser);
        IReport GetReport();
        void LoadGroups(System.Collections.Generic.List<AveGroupInfo> groups);
        void LoadMembers(AveSecurityInfo securityInfo);
        void LoadUsers(System.Collections.Generic.List<AveUserInfo> userInfos);
        int RestoreGroup(AveGroupInfo groupInfo, MembersRestoreOption option);
        int RestoreGroup(AveGroupInfo groupInfo, MembersRestoreOption option, ISPImportProfiler profiler);
        void RestoreGroupOwner();
        void RestoreGroups(System.Collections.Generic.List<AveGroupInfo> groupsInfo);
        void RestoreMembers(AveSecurityInfo securityInfo);
        IAveRestoreStream RestoreStream { get; set; }
        void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option, ISPImportProfiler profiler);
        void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option);
        int RestoreUser(AveUserInfo userInfo, MembersRestoreOption option);
        IAveUserAndDomainMapping UserAndDomainMapping { get; }
        void SetDefaultOption(MembersRestoreOption option);

        MembersRestoreOption DefaultOption { get; }
    }

    public interface IAveSPMembersMultiThread : IAveSPMembers
    {
        
    }
    public class MembersRestoreOption
    {
        //是否在site 级别还原user
        public bool IsSiteLevel { get; set; } 
        
        //是否还原user/group  属性
        public bool OverWrite = true;

        //是否还原没有权限的user / group
        public bool SkipWithoutPermissions { get; set; }

        //是否还原覆盖目的端user 的administrator 属性
        public bool UpdateAdminSetting { get; set; }

        //是否将目的端已存在User，使用源端删除状态
        public bool NeedDeleteUser = true;

        public bool CacheSkippedUserInfo { get; set; }

        public bool RestoreInactiveUser { get; set; }

        public MembersRestoreOption Clone()
        {
            return new MembersRestoreOption {
                IsSiteLevel = this.IsSiteLevel,
                OverWrite = this.OverWrite,
                SkipWithoutPermissions = this.SkipWithoutPermissions,
                UpdateAdminSetting = this.UpdateAdminSetting,
                NeedDeleteUser = this.NeedDeleteUser,
                CacheSkippedUserInfo = this.CacheSkippedUserInfo,
                RestoreInactiveUser = this.RestoreInactiveUser
            };
        }
    }

    public class AveSPUserMappingManager
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Func<string, string> GetMappingLoginName;

        private Func<string, string> GetMappingDomainName;

        private Dictionary<string, string> userMapping;
        private Dictionary<string, string> domainMapping;

        public AveSPUserMappingManager(Func<string, string> userMapping, Func<string, string> domainMapping)
        {
            this.GetMappingLoginName = userMapping;
            this.GetMappingDomainName = domainMapping;
        }

        public AveSPUserMappingManager(Dictionary<string, string> userMapping, Dictionary<string, string> domainMapping)
        {
            this.userMapping = userMapping;
            this.domainMapping = domainMapping;
            this.GetMappingLoginName = GetMappingLoginNameBeforeAdd;
            this.GetMappingDomainName = GetMappingDomainNameBeforeAdd;
        }

        public string GetMappingUserLogin(string login, bool isDomainGroup, bool needMapping)
        {
            //if (login.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase)
            //    || login.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase)
            //    || login.Equals("NT AUTHORITY\\local service", StringComparison.OrdinalIgnoreCase))
            //{
            //    return login;
            //}
            
            if (!IsSP10FBAUser(login))
            {
                if (!needMapping)
                {
                    return login;
                }
            }
            else
            {
                if (!WrapperConfiguration.ReplaceUserPrefix)
                {
                    return login;
                }
            }
            var resultLogin = this.GetMappingLoginName(login);
            if (string.IsNullOrEmpty(resultLogin))
            {
                var fixedChars = string.Empty;
                var fixedCharIndex = login.IndexOf('|');
                if (fixedCharIndex > 0)
                {
                    fixedChars = login.Substring(0, fixedCharIndex + 1);
                    var realLogin = login.Substring(fixedCharIndex + 1);
                    if (realLogin.IndexOf('|') > 0)
                    {
                        if (fixedChars.EndsWith(".f|", StringComparison.OrdinalIgnoreCase)
                         || fixedChars.EndsWith(".m|", StringComparison.OrdinalIgnoreCase)
                         || fixedChars.EndsWith(".r|", StringComparison.OrdinalIgnoreCase))
                        {//SP10 FBA User Format
                            var providerName = realLogin.Substring(0, realLogin.IndexOf('|')) + ":";
                            var username = realLogin.Substring(realLogin.IndexOf('|') + 1);
                            resultLogin = providerName + username;
                            if (needMapping)
                            {
                                var fbaMappingResult = GetMappingLoginForFBAUser(providerName, username);
                                if (string.Equals(resultLogin, fbaMappingResult, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (realLogin.IndexOf('@') > 0)
                                    {//Office 365 user use email format.
                                        fixedCharIndex = login.IndexOf('|', fixedCharIndex + 1);
                                        fixedChars = login.Substring(0, fixedCharIndex + 1);
                                        realLogin = login.Substring(fixedCharIndex + 1);
                                        string cbaMappingResult = GetMappingLoginForCBAUser(fixedChars, realLogin);
                                        resultLogin = cbaMappingResult;
                                    }
                                }
                                else
                                {
                                    resultLogin = fbaMappingResult;
                                }
                            }
                        }
                        else
                        {
                            if (realLogin.IndexOfAny(new Char[] { '\\', '@' }) > 0)
                            {//CBA user
                                fixedCharIndex = login.IndexOf('|', fixedCharIndex + 1);
                                fixedChars = login.Substring(0, fixedCharIndex + 1);
                                realLogin = login.Substring(fixedCharIndex + 1);
                                resultLogin = GetMappingLoginForCBAUser(fixedChars, realLogin);
                            }
                            else if (login.IndexOf('|', fixedCharIndex + 1) > 0)
                            {
                                resultLogin = GetMappingLoginForCBAUser(fixedChars, realLogin) ?? login;
                            }
                            else
                            {//ADFS domain Group,STS group
                                resultLogin = login;
                            }
                        }
                    }
                    else
                    {
                        if (realLogin.IndexOfAny(new Char[] { '\\', '@' }) > 0)
                        {//CBA Windows User
                            resultLogin = GetMappingLoginForCBAUser(fixedChars, realLogin);
                        }
                        else
                        {//c:0(.s|true, not need mapping
                            resultLogin = login;
                        }
                    }
                }
                else
                {//Classical AD User
                    if (login.IndexOf('\\') > 0)
                    {
                        var domain = login.Substring(0, login.IndexOf('\\'));
                        var username = login.Substring(login.IndexOf('\\') + 1);
                        resultLogin = GetMappingLoginForADUser(domain, username);
                    }
                    else if (login.IndexOf(':') > 0)
                    {//07 FBA User
                        var providerName = login.Substring(0, login.IndexOf(':') + 1);
                        var username = login.Substring(login.IndexOf(':') + 1);
                        resultLogin = GetMappingLoginForFBAUser(providerName, username);
                    }
                    else
                    {
                        mLog.Info("Unknown user format:{0} .", login);
                        resultLogin = login;
                    }
                }
            }

            if (!login.Equals(resultLogin, StringComparison.OrdinalIgnoreCase))
            {
                mLog.Info("Mapping user from:{0} to {1}.", login, resultLogin);
            }
            return resultLogin;
        }

        private bool IsSP10FBAUser(string login)
        {
            return (login.IndexOf(".f|", StringComparison.OrdinalIgnoreCase) > 0
             || login.IndexOf(".m|", StringComparison.OrdinalIgnoreCase) > 0
             || login.IndexOf(".r|", StringComparison.OrdinalIgnoreCase) > 0);
        }

        private string GetMappingLoginForADUser(string domain, string username)
        {
            var resultLogin = string.Format("{0}\\{1}", domain, username);
            var mappingDomainName = this.GetMappingDomainName(domain);
            if (!String.IsNullOrEmpty(mappingDomainName))
            {//mapping domain
                resultLogin = ConcatMappingDomainAndUser(mappingDomainName, username);
            }
            else
            {
                var mappingUsername = this.GetMappingLoginName(username);
                if (!string.IsNullOrEmpty(mappingUsername))
                {//mapping username
                    resultLogin = mappingUsername;
                    if (mappingUsername.IndexOfAny(new char[] { '|', ':' }) <= 0)
                    {
                        resultLogin = ConcatMappingDomainAndUser(domain, mappingUsername);
                    }
                }
            }
            return resultLogin;
        }

        private string GetMappingLoginForFBAUser(string providerName, string username)
        {
            var needReplaceChars = new char[] { ';', ',', '|', '%' };
            foreach (var c in needReplaceChars)
            {
                string hexchar = String.Format("%{0:x}", (int)c);
                if (username.Contains(hexchar))
                {
                    username = username.Replace(hexchar, c.ToString());
                }
            }
            var resultLogin = this.GetMappingLoginName(providerName + username);
            if (string.IsNullOrEmpty(resultLogin))
            {//mapping full name
                var mappingDomainName = this.GetMappingDomainName(providerName);
                if (!String.IsNullOrEmpty(mappingDomainName))
                {//mapping provider
                    resultLogin = ConcatMappingDomainAndUser(mappingDomainName, username);
                }
                else
                {
                    var mappingUsername = this.GetMappingLoginName(username);
                    if (!string.IsNullOrEmpty(mappingUsername))
                    {//mapping username
                        resultLogin = mappingUsername;
                        if (mappingUsername.IndexOfAny(new char[] { '|', ':' }) <= 0)
                        {
                            resultLogin = ConcatMappingDomainAndUser(providerName, mappingUsername);
                        }
                    }
                    else
                    {//Not mapped
                        resultLogin = providerName + username;
                    }
                }
            }
            return resultLogin;
        }

        private string GetMappingLoginForCBAUser(string fixedChars, string loginName)
        {
            var loginSplitStrings = loginName.Split('\\', '@', '|');
            var domain = loginSplitStrings[0];
            var username = loginSplitStrings[1];
            bool isEmail = loginName.Contains('@');
            if (isEmail)
            {
                domain = loginSplitStrings[1];
                username = loginSplitStrings[0];
            }

            var resultLogin = fixedChars + loginName;
            var mappingDomainName = this.GetMappingDomainNameForEmail(fixedChars, domain, isEmail);
            if (loginName.IndexOf('|') > 0 && !String.IsNullOrEmpty(mappingDomainName)
                && !mappingDomainName.Contains("{0}"))// 需要判断已经在设置mapping 时写好格式的情况
            {
                mappingDomainName = mappingDomainName + "|{0}";
            }
            if (!String.IsNullOrEmpty(mappingDomainName))
            {//mapping full domain
                resultLogin = ConcatMappingDomainAndUser(mappingDomainName, username);
            }
            else
            {
                var mappingUsername = this.GetMappingLoginName(loginName);
                if (!string.IsNullOrEmpty(mappingUsername))
                {//mapping login
                    resultLogin = mappingUsername;
                    if (mappingUsername.IndexOfAny(new char[] { '|', ':' }) <= 0)
                    {
                        resultLogin = fixedChars + mappingUsername;
                    }
                }
                else
                {
                    mappingDomainName = this.GetMappingDomainNameForEmail(string.Empty, domain, isEmail);
                    if (!String.IsNullOrEmpty(mappingDomainName))
                    {//mapping small domain
                        resultLogin = ConcatMappingDomainAndUser(mappingDomainName, username);
                        if (resultLogin.IndexOfAny(new char[] { '|', ':' }) <= 0)
                        {
                            resultLogin = fixedChars + resultLogin;
                        }
                    }
                    else
                    {
                        mappingUsername = this.GetMappingLoginName(username);
                        if (!string.IsNullOrEmpty(mappingUsername))
                        {//mapping username
                            resultLogin = mappingUsername;
                            if (mappingUsername.IndexOfAny(new char[] { '|', ':' }) <= 0)
                            {
                                resultLogin = ConcatMappingDomainAndUser(fixedChars + domain, mappingUsername);
                            }
                        }
                    }
                }
            }
            return resultLogin;
        }

        private string GetMappingDomainNameForEmail(string fixChars, string domainName, bool isEnail)
        {
            if (isEnail)
            {
                var result = GetMappingDomainName(string.Format("{0}{1}@{2}", fixChars, "{0}", domainName));
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            return GetMappingDomainName(fixChars + domainName);
        }

        private string ConcatMappingDomainAndUser(string mappingDomainName, string username)
        {
            if (mappingDomainName.EndsWith(":", StringComparison.Ordinal))
            {
                return mappingDomainName + username;
            }
            else if (mappingDomainName.Contains("{0}"))
            {//mapping to ADFS
                return string.Format(mappingDomainName, username);
            }
            else
            {
                return string.Format("{0}\\{1}", mappingDomainName, username);
            }
        }

        private string GetMappingLoginNameBeforeAdd(string username)
        {
            return GetMappingLoginNameInMapping(userMapping, username);
        }

        private string GetMappingDomainNameBeforeAdd(string domain)
        {
            return GetMappingLoginNameInMapping(domainMapping, domain);
        }

        private string GetMappingLoginNameInMapping(Dictionary<string, string> mapping, string username)
        {
            if (mapping != null && mapping.ContainsKey(username))
            {
                return mapping[username];
            }
            return null;
        }

    }
}
