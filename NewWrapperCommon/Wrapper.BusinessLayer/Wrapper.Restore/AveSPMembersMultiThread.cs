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

using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    //Restore user with multiple thread.
    public class AveSPMembersMultiThread : AveSPMembers, IAveSPMembersMultiThread
    {
        AveTaskExecutor executor;
        private int mInactiveUserId = int.MaxValue;
        public AveSPMembersMultiThread(AveSPMembers beforeMembers, AveSPSite aveSite)
            : this(aveSite)
        {
            this.defaultOption = beforeMembers.defaultOption;
            this.RestoreStream = beforeMembers.RestoreStream;
            this.UserAndDomainMapping = beforeMembers.UserAndDomainMapping;
            this.mappingManager = new AveSPUserMappingManager(this.UserAndDomainMapping.GetMappingLoginNameBeforeAdd, this.UserAndDomainMapping.GetMappingDomainNameBeforeAdd);
        }

        public AveSPMembersMultiThread(AveSPSite aveSite)
            : base(aveSite)
        {
            executor = new AveTaskExecutor(20);
        }
        /// <summary>
        /// If the site is online, do not resolve the principle, only ensure the user with multiple thread.
        /// </summary>

        public void RestoreUsers(IList<AveUserInfo> allUsers, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions)
        {
            var option = new MembersRestoreOption
            {
                IsSiteLevel = siteLevel,
                OverWrite = !notOverWrite,
                SkipWithoutPermissions = skipUserWithoutPermissions,
                UpdateAdminSetting = false,
                NeedDeleteUser = true
            };
            RestoreUsers(allUsers, option, null);
            //此处改为新接口。[CodeReview]
        }

        [Obsolete("Need Delete,because the profiler need be sent in")]
        public override void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option)
        {
            //TODO  Need Delete
            //var profiler = new DefaultRestoreSiteProfiler();
            RestoreUsers(allUsers, option, null);
        }
        public override void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            List<Task> ensureUserTasks = new List<Task>();
            foreach (AveUserInfo userinfo in allUsers)
            {
                ensureUserTasks.Add(() => { RestoreUser(userinfo, option, profiler); });
            }
            executor.Execute(ensureUserTasks);
            AddUserLoginAndIdMappings(allUsers);
        }

        /// <summary>
        /// Online do not need Resolve Pinciple.
        /// </summary>
        public override int RestoreUser(AveUserInfo userInfo, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Status = WrapperRestoreStatus.None, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_StartRestoreUser, userInfo.Login), Title = userInfo.Login, Type = SPObjectType.User, Url = ParentSite.SiteUrl }); }

            bool isNeedReport = true;
            //userId从未使用过，删除此变量。[CodeReview]
            AveStatus reportStatus = AveStatus.Successful;
            string srcLoginName = userInfo.Login;
            String objectTitle = mAveParentSite.SPSite.RootWeb.Title;
            AveReportResource key = AveReportResource.Wrapper_Report_None;
            IAveUser spUser = null;
            try
            {
                //没有权限，不还原
                if ((option.SkipWithoutPermissions && NeedSkipWithoutPermissions(userInfo)))
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Url = ParentSite.SiteUrl, Type = SPObjectType.User, Title = userInfo.Login, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_NoPermissionOrActiveUser, userInfo.Login), Status = WrapperRestoreStatus.Skipped, Level = SPObjectLevel.SiteCollection }); }
                    reportStatus = AveStatus.Skipped;
                    return 0;
                }
                AveSPMemberInfo memberInfo = UserAndDomainMapping.GetUserMapping(userInfo.ID) as AveSPMemberInfo;
                if (memberInfo != null)
                {
                    isNeedReport = false;
                    reportStatus = AveStatus.Skipped;
                    if (!memberInfo.IsUser)
                    {
                        log.Warn("The user id '{0}' is group id. LoginName:{1}", userInfo.ID, userInfo.Login);
                        key = AveReportResource.Wrapper_Report_TheUserIsGroup;
                        //reportMsg = string.Format(WrapperReportResource.Wrapper_Report_TheUserIsGroup, userInfo.ID, userInfo.Login);
                    }
                    return 0;
                }
                else
                {
                    UserAndDomainMapping.RemoveOneUserMapping(userInfo.ID);
                }
                bool isNewAdd = false;
                //isNewAdd并非在所有分支下都被赋值使用out并不合理，改为ref，省去代码中重复赋初值。同时needUpdate这个参数并未使用在方法中，删除此变量。[CodeReview]
                spUser = GetOrAddUser(userInfo, ref isNewAdd, profiler);
                bool incativeUserRestored = false;
                if (spUser == null)
                {
                    if (option.RestoreInactiveUser)  // now only for hsm, azure api can use placeholder,client api not.
                    {
                        mInactiveUserId -= 1;
                        incativeUserRestored = true;
                        memberInfo = new AveSPMemberInfo(userInfo.Login, mInactiveUserId, true, true);
                    }
                    else
                    {
                        memberInfo = new AveSPMemberInfo(userInfo.Login, -1, true);
                    }
                }
                else
                {
                    //ADO-61868
                    if (!isNewAdd)
                    {
                        var tmp = UserAndDomainMapping.EnumUserMapping().Select(info => info.Value as AveSPMemberInfo).Where(info => info != null && info.NewId == spUser.ID).ToList<AveSPMemberInfo>();
                        if (tmp.Count > 0)
                        {
                            memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                            memberInfo.SourceInfo = userInfo;
                            UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                            key = AveReportResource.Wrapper_Report_UserHasRestored;
                            //  reportMsg = String.Format(WrapperReportResource.Wrapper_Report_UserHasRestored, spUser.ID);
                            reportStatus = AveStatus.Skipped;
                            log.Warn(String.Format(WrapperRestoreResource.UserHasRestored, spUser.ID));
                            return spUser.ID;
                        }
                        if (!option.OverWrite)
                        {
                            memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                            memberInfo.SourceInfo = userInfo;
                            UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                            return spUser.ID;
                        }
                    }
                    memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                    RestoreUserSettings(spUser, userInfo, option, profiler);
                }
                memberInfo.SourceInfo = userInfo;
                UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                if (incativeUserRestored)//ADO-212864 only for HSM 
                {
                    reportStatus = AveStatus.Failed;
                }
                else
                {
                    reportStatus = isNewAdd ? AveStatus.Successful : (memberInfo.NewId == -1 ? AveStatus.Failed : AveStatus.Skipped);
                }
                if (reportStatus == AveStatus.Skipped)
                {
                    key = AveReportResource.Wrapper_Report_UserRestoreSkipError;
                }
                if (reportStatus == AveStatus.Failed)
                {
                    key = AveReportResource.Wrapper_Report_UserRestoreFailedError;
                }
                return memberInfo.NewId;
            }
            catch (AveSecurityTrimingException ex)
            {
                string reportLoginName = !String.IsNullOrEmpty(userInfo.Login) ? userInfo.Login : srcLoginName;
                log.Warn("An error occurred while restore user. {0}" + reportLoginName, ex);
                reportStatus = AveStatus.Skipped;
                key = AveReportResource.Wrapper_Report_NoPermissionToRestoreUser;
                return -1;
            }
            catch (Exception ex)
            {
                string reportLoginName = !String.IsNullOrEmpty(userInfo.Login) ? userInfo.Login : srcLoginName;
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(reportLoginName, ex));
                reportStatus = AveStatus.Failed;
                key = AveReportResource.Wrapper_Report_UserRestoreFailedError;
                return -1;
            }
            finally
            {
                if (isNeedReport)
                {
                    string reportLoginName = string.Empty;
                    if (userInfo.DomainGroup)
                    {
                        if (spUser != null)
                        {
                            reportLoginName = spUser.Name;
                        }
                        else
                        {
                            reportLoginName = userInfo.Title;
                        }
                    }
                    else
                    {
                        reportLoginName = !String.IsNullOrEmpty(userInfo.Login) ? userInfo.Login : srcLoginName;
                    }
                    if (reportStatus != AveStatus.Successful)
                    {
                        log.Info("Restore user name: {0}, the restore status is {1}.", reportLoginName, reportStatus);
                    }
                    lock (reportPrivateLock)
                    {
                        report.AddDetail(new AveWrapperReportDto(reportLoginName, objectTitle, AveReportObjectType.User, reportStatus, key));
                    }
                }
            }
        }

        private string GetUserLoginWithoutFixedChars(string userlogin, char index)
        {
            var fixedCharIndex = userlogin.LastIndexOf(index);
            if (fixedCharIndex < 0)
            {
                return userlogin;
            }
            return userlogin.Substring(fixedCharIndex + 1);
        }

        private bool HasNoMapping(string mappingLogin, string userlogin, bool isDomainGroup)
        {
            //On-premise user login name not changed without user mappping
            if (string.Equals(mappingLogin, userlogin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            //ADO-182461 Online的Domain group在mapping后会按照FBA的方式修改loginname(provider:loginname)导致判断是否存在 user mapping的判断出错，
            //此处对于domain group将前缀以及provider 都移除掉，之比较login name本身
            if (isDomainGroup)
            {
                return string.Equals(GetUserLoginWithoutFixedChars(mappingLogin, ':'), GetUserLoginWithoutFixedChars(userlogin, '|'), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        protected IAveUser GetOrAddUser(AveUserInfo userInfo, ref bool isNewAdd, ISPImportProfiler profiler)
        {
            IAveUser user = null;
            var newLogin = GetMappingUserLogin(userInfo.Login, true);
            var hasNoMapping = HasNoMapping(newLogin, userInfo.Login, userInfo.DomainGroup);
            userInfo.Login = newLogin;
            user = FindUser(userInfo, hasNoMapping);
            if (user == null)
            {
                try
                {
                    user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(userInfo.Login);
                    isNewAdd = true;
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = userInfo.Login, Status = WrapperRestoreStatus.Successful, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserSuccessfully, userInfo.Login), Level = SPObjectLevel.SiteCollection }); }
                }
                catch (Exception e)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = userInfo.Login, Status = WrapperRestoreStatus.Failed, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserFailed, userInfo.Login, e), Level = SPObjectLevel.SiteCollection }); }
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(userInfo.Login, e));
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = userInfo.Login, Status = WrapperRestoreStatus.Successful, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_FindUserInDestinationSuccessfully, userInfo.Login), Level = SPObjectLevel.SiteCollection }); }
            }
            return user;
        }

        private IAveUser FindUser(AveUserInfo userInfo, bool hasNoMapping)
        {
            IAveUser user = null;
            try
            {
                if (hasNoMapping)
                {
                    bool reachMax = false;
                    IAveUtility utility = mAveParentSite.ObjectModelFactory.Utility;
                    var searchTitle = userInfo.Title;
                    if (userInfo.DomainGroup)
                    {
                        var index = userInfo.Title.IndexOf('\\');
                        if (index > 0)
                        {
                            searchTitle = userInfo.Title.Substring(index + 1);
                        }
                    }
                    var infos = utility.SearchPrincipals(mAveParentSite.SPSite.RootWeb, searchTitle, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, int.MaxValue, out reachMax);
                    foreach (var info in infos)
                    {
                        if (string.Equals(info.DisplayName, searchTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Debug("Change the source user loginName for search. Source login:{0}. Source DisplayName:{1}. Destination login:{2}. Destination DisplayName:{3}", userInfo.Login, userInfo.Title, info.LoginName, info.DisplayName);
                            userInfo.Login = info.LoginName;
                            break;
                        }
                    }
                }
                user = mAveParentSite.SPSite.RootWeb.SiteUsers[userInfo.Login];
            }
            catch (Exception e)
            {
                log.Debug("Cannot find user. LoginName:{0}, error:{1}", userInfo.Login, e);
            }
            return user;
        }

        public override void RestoreMembers(AveSecurityInfo securityInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreMembers"))
            {
                if (securityInfo.Users != null)
                {
                    RestoreUsers(securityInfo.Users, defaultOption, null);
                    //此处改为新接口。[CodeReview]
                }
                if (securityInfo.Groups != null)
                {
                    foreach (AveGroupInfo groupInfo in securityInfo.Groups)
                    {
                        try
                        {
                            RestoreGroup(groupInfo, defaultOption);
                        }
                        catch (Exception e)
                        {
                            log.Warn(string.Format("An error occurred while restore group. group title:{0}, group id:{1}\n error message:{2}", groupInfo.Title, groupInfo.ID, e));
                        }
                    }
                }
            }
        }
    }
}
