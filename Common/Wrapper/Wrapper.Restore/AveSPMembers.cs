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
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using RESTORERES = AvePoint.Wrapper.Resource.WrapperRestoreReportResource;
using System.Text.RegularExpressions;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPMembers : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private Dictionary<int, object> mMapping;
        private AveSPSite mAveParentSite;
        private AveSPUserMappingManager mappingManager;
        protected IReport report = new AveWrapperReport();

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }
        protected IAveUserAndDomainMapping mUserAndDomainMapping;
        public IAveUserAndDomainMapping UserAndDomainMapping
        {
            get
            {
                if (mUserAndDomainMapping == null)
                {
                    mUserAndDomainMapping = new AveUserAndDomainMapping();
                }
                return mUserAndDomainMapping;
            }
        }
        private IAveRestoreStream mReader = null;
        private AveFileRestoreStream mStream = null;
        // map the backup id with the restore id of user or group, 
        // key is the backup id, the value is the Member Info
        private Dictionary<string, int> mPostGroup = new Dictionary<string, int>();
        private List<string> mAllGroups = null;
        public List<string> AllGroups
        {
            get
            {
                if (mAllGroups == null)
                {
                    mAllGroups = new List<string>();
                    foreach (IAveGroup group in mAveParentSite.SPSite.RootWeb.SiteGroups)
                    {
                        if (!string.IsNullOrEmpty(group.LoginName))
                        {
                            mAllGroups.Add(group.LoginName.ToLower());
                        }
                    }
                }
                return mAllGroups;
            }
        }

        public IAveRestoreStream RestoreStream
        {
            get { return mReader; }
            set { mReader = value; }
        }

        public AveSPMembers(AveSPSite aveSite)
        {
            //mMapping = new Dictionary<int, object>();
            mAveParentSite = aveSite;
            this.mappingManager = new AveSPUserMappingManager(this.UserAndDomainMapping.GetMappingLoginNameBeforeAdd, this.UserAndDomainMapping.GetMappingDomainNameBeforeAdd);
        }

        public void Dispose()
        {
            if (mStream != null)
            {
                mStream.Dispose();
            }
            //if (File.Exists(mUserInfoDataPath))
            //{
            //    try
            //    {
            //        File.Delete(mUserInfoDataPath);
            //    }
            //    catch (Exception e)
            //    {
            //        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.DeleteFileError, e.ToString());
            //    }
            //}
            mAveParentSite = null;
            mReader = null;
            UserAndDomainMapping.Dispose();
        }

        //public void SetFBAStatus(IAveWebApplication webApp)
        //{
        //    if (mFBAManager == null)
        //    {
        //        mFBAManager = new AveFBAManager();
        //    }
        //    mFBAManager.SetFBAStatus(webApp);
        //}

        public IReport GetReport()
        {
            return this.report;
        }

        public int RestoreUser(AveUserInfo userInfo)
        {
            return RestoreUser(userInfo, false);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel)
        {
            return RestoreUser(userInfo, siteLevel, false);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel, bool notOverWrite)
        {
            return RestoreUser(userInfo, siteLevel, notOverWrite, false);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions)
        {
            return RestoreUser(userInfo, siteLevel, notOverWrite, false, null, false, true);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions, bool isNeedReport)
        {
            return RestoreUser(userInfo, siteLevel, notOverWrite, false, null, false, isNeedReport);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions, IAvePrincipalInfo principalInfo, bool isPrincipalLoaded)
        {
            return RestoreUser(userInfo, siteLevel, notOverWrite, skipUserWithoutPermissions, principalInfo, isPrincipalLoaded, true);
        }

        public int RestoreUser(AveUserInfo userInfo, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions, IAvePrincipalInfo principalInfo, bool isPrincipalLoaded, bool isNeedReport)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.User"))
            {
#endif
                int userId = -1;
                AveStatus reportStatus = AveStatus.Successful;
                string srcLoginName = userInfo.Login;
                string reportMsg = string.Empty;
                try
                {
                    if (NeedSkipUser(skipUserWithoutPermissions, userInfo, ref reportStatus, ref reportMsg, ref userId))
                    {
                        return userId;
                    }

                    bool needUpdate = false;
                    bool isNewAdd = false;
                    IAveUser spUser = null;
                    //SAAS-28616 修改内容对应"Everyone Except External users"转移不过去
                    //if (userInfo.Login.StartsWith("c:0-.f|rolemanager|spo-grid-all-users/"))
                    //{
                    //    spUser = FindSpecialAccount(userInfo.Login);
                    //    if (spUser != null)
                    //    {
                    //        userInfo.Login = spUser.LoginName;
                    //    }
                    //}
                    //else
                    //{
                    if (userInfo.DomainGroup)
                    {
                        spUser = TryAddDomainGroup(GetMappingDomainGroupName(userInfo.Login, userInfo.Title, true), userInfo, out needUpdate, out isNewAdd, ref reportMsg);
                    }
                    else
                    {
                        spUser = isPrincipalLoaded ? TryAddUser(GetMappingUserLogin(userInfo.Login, true), userInfo, out needUpdate, out isNewAdd, ref reportMsg)
                            : GetOrAddUser(userInfo, out needUpdate, out isNewAdd, ref reportMsg, principalInfo, false);
                    }
                    //}

                    AveSPMemberInfo memberInfo = null;
                    // add non-new and add user to mapping list
                    if (spUser != null && !isNewAdd && notOverWrite)
                    {
                        memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                        //mMapping[userInfo.ID] = memberInfo;
                        memberInfo.SourceInfo = userInfo;
                        UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                        return spUser.ID;
                    }

                    if (spUser != null)
                    {
                        memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                        RestoreUserSettings(spUser, userInfo, siteLevel);
                    }
                    else
                    {
                        reportMsg = RESTORERES.Wrapper_UserNotExist;
                        memberInfo = AveSPMemberInfo.FAKE_USER;
                    }
                    //mMapping[userInfo.ID] = memberInfo;
                    memberInfo.SourceInfo = userInfo;
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);

                    if (needUpdate)
                    {
                        UpdateUserInfoByNative(spUser, userInfo);
                    }

                    reportStatus = isNewAdd ? AveStatus.Successful : (memberInfo.NewId == -1 ? AveStatus.Failed : AveStatus.Skipped);
                    return userId = memberInfo.NewId;
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore user. {0}" + userInfo.Title, ex);
                    reportStatus = AveStatus.Skipped;
                    reportMsg = "Access Denied, You don't have permission to restore user. ";
                    return -1;
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(userInfo.Login, ex));
                    reportStatus = AveStatus.Failed;
                    reportMsg = ex.Message;
                    return -1;
                }
                finally
                {
                    if (isNeedReport)
                    {
                        report.AddDetail(new AveWrapperReportDto(srcLoginName, string.Empty, AveReportObjectType.User, reportStatus, reportMsg));
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public void MultiThreadRestoreUsers(List<AveUserInfo> userInfos, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions)
        {
            try
            {
                AveTaskExecutor taskExecutor = new AveTaskExecutor(20);
                ICollection<DelegateTask> tasks = new List<DelegateTask>();
                foreach (AveUserInfo userInfo in userInfos)
                {
                    int userId = -1;
                    string reportMsg = "";
                    AveStatus reportStatus = AveStatus.Successful;
                    if (NeedSkipUser(skipUserWithoutPermissions, userInfo, ref reportStatus, ref reportMsg, ref userId))
                    {
                        continue;
                    }
                    tasks.Add(() => { RestoreUser(userInfo, siteLevel, notOverWrite, skipUserWithoutPermissions, null, true); });
                }
                taskExecutor.Execute(tasks);
            }
            catch (Exception e)
            {
                log.Error("failed to restore user due to: {0}", e.ToString());
            }
        }

        public void MultiThreadRestoreUsers(List<AveUserInfo> userInfos, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions, IAvePrincipalInfo principalInfo, bool isPrincipalLoaded)
        {
            try
            {
                AveTaskExecutor taskExecutor = new AveTaskExecutor(20);
                ICollection<DelegateTask> tasks = new List<DelegateTask>();
                foreach (AveUserInfo userInfo in userInfos)
                {
                    int userId = -1;
                    string reportMsg = "";
                    AveStatus reportStatus = AveStatus.Successful;
                    if (NeedSkipUser(skipUserWithoutPermissions, userInfo, ref reportStatus, ref reportMsg, ref userId))
                    {
                        continue;
                    }
                    tasks.Add(() => { RestoreUser(userInfo, siteLevel, notOverWrite, skipUserWithoutPermissions, principalInfo, isPrincipalLoaded); });
                }
                taskExecutor.Execute(tasks);
            }
            catch (Exception e)
            {
                log.Error("failed to restore user due to: {0}", e.ToString());
            }
        }

        public void RestoreUsers(List<AveUserInfo> userInfos, bool siteLevel, bool notOverWrite, bool skipUserWithoutPermissions)
        {
            foreach (AveUserInfo userInfo in userInfos)
            {
                RestoreUser(userInfo, siteLevel, notOverWrite, skipUserWithoutPermissions);
            }
        }

        private bool NeedSkipWithoutPermissions(AveUserInfo userInfo)
        {
            if ((userInfo.HasPermission.HasValue && !userInfo.HasPermission.Value) || !userInfo.IsActive || userInfo.Deleted != 0)
            {
                if (userInfo.ID != AveConstants.SYSTEM_ACCOUNT_ID)
                {
                    return true;
                }
            }
            return false;
        }

        private bool NeedSkipUser(bool skipUserWithoutPermissions, AveUserInfo userInfo, ref AveStatus reportStatus, ref string reportMsg, ref int returnId)
        {
            if (skipUserWithoutPermissions && NeedSkipWithoutPermissions(userInfo))
            {
                returnId = -1;
                reportStatus = AveStatus.Skipped;
                return true;//没有权限，不还原
            }
            AveSPMemberInfo memberInfo = UserAndDomainMapping.GetUserMapping(userInfo.ID) as AveSPMemberInfo;
            if (memberInfo != null)
            {
                reportStatus = AveStatus.Skipped;
                if (!memberInfo.IsUser)
                {
                    log.Warn("The user id '{0}' is group id. LoginName:{1}", userInfo.ID, userInfo.Login);
                    reportMsg = string.Format("The user id '{0}' is group id. LoginName:{1}", userInfo.ID, userInfo.Login);
                    returnId = AveSPMemberInfo.FAKE_USER.NewId;
                    return true;
                }
                // This user has already been restored, just return its information.
                returnId = memberInfo.NewId;
                return true;
            }
            return false;
        }

        private IAveUser GetOrAddUser(AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd)
        {
            string errorMessage = string.Empty;
            return GetOrAddUser(userInfo, out needUpdate, out isNewAdd, ref errorMessage);
        }

        private bool CheckUserExistence(AveUserInfo userInfo, ref IAveUser user, ref string newLogin, IAvePrincipalInfo principalInfo, bool isPrincipalLoaded)
        {
            try
            {
                newLogin = string.IsNullOrEmpty(newLogin) ? GetMappingUserLogin(userInfo.Login, true) : newLogin;
                //if (userInfo.Login.StartsWith("c:0-.f|rolemanager|spo-grid-all-users/"))
                //{
                //    user = FindSpecialAccount(userInfo.Login);
                //    if (user != null)
                //    {
                //        userInfo.Login = user.LoginName;
                //    }
                //} else if(principalInfo == null && !isPrincipalLoaded)
                if (principalInfo == null && !isPrincipalLoaded)
                {
                    IAveUtility utility = mAveParentSite.ObjectModelFactory.Utility;
                    principalInfo = utility.ResolvePrincipal(mAveParentSite.SPSite.RootWeb, newLogin, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                }
                if (principalInfo != null)
                {
                    if (principalInfo.PrincipalID >= 0)
                    {
                        user = mAveParentSite.SPSite.RootWeb.SiteUsers[principalInfo.LoginName];
                        userInfo.Login = newLogin;
                        newLogin = principalInfo.LoginName;
                        return user != null;
                    }
                    newLogin = principalInfo.LoginName;
                }
                //else
                //{
                //    newLogin = userInfo.Login;
                //}
                return false;
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Debug("Cannot find user. LoginName:{0}, error:{1}", userInfo.Login, e.ToString());
                return false;
            }
        }



        private IAveUser TryAddUser(string loginName, AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd, ref string errorMessage)
        {
            IAveUser user = null;
            needUpdate = false;
            isNewAdd = false;
            try
            {
                try
                {
                    user = mAveParentSite.SPSite.RootWeb.EnsureUser(loginName);
                    userInfo.Login = loginName;
                    isNewAdd = true;
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Warn("Failed to restore the user:{0},exception:{1}", loginName, e);
                    //string placeHoldAccount = mAveParentSite.GetPlaceHolderAccount();
                    //if (string.IsNullOrEmpty(placeHoldAccount))
                    //{
                    //    throw;
                    //}
                    string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(userInfo.Login);
                    if (!String.IsNullOrEmpty(mappingValue))
                    {
                        userInfo.Login = mappingValue;
                    }
                    string existUserLogin = mAveParentSite.SPSite.GetUserLoginBySystemId(userInfo.SystemID);
                    if (!string.IsNullOrEmpty(existUserLogin))
                    {
                        throw new UserAlreadyMappedException(userInfo.Login, existUserLogin);
                    }
                    string defaultUser = mAveParentSite.DefaultUser;
                    if (!string.IsNullOrEmpty(defaultUser))
                    {
                        log.Info("Cannot add user:{0}, will try default user, default username:{1}", userInfo.Login, defaultUser);
                        //当配置default user为domain group的title时，需要先获取其login name，再进行ensure.
                        if (!defaultUser.Contains("@") && !defaultUser.Contains("|"))
                        {
                            try
                            {
                                defaultUser = mAveParentSite.SPSite.RootWeb.GetDomainGroupLoginName(defaultUser);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("The user is not domain group, login name:{0},error:{1}", loginName, ex);
                            }
                        }
                        user = mAveParentSite.SPSite.RootWeb.EnsureUser(defaultUser);
                        userInfo.Login = defaultUser;
                    }
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(userInfo.Login, e));
                errorMessage = e.Message;
            }

            return user;
        }
        private IAveUser TryAddDomainGroup(string groupName, AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd, ref string errorMessage)
        {
            IAveUser user = null;
            needUpdate = false;
            isNewAdd = false;
            try
            {
                try
                {
                    user = mAveParentSite.SPSite.RootWeb.EnsureUser(groupName);
                    userInfo.Login = user.LoginName;
                    isNewAdd = true;
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (AveWrapperUserNotFoundOrNotUniqueException e)
                {
                    log.Warn("Failed to restore the domaingroup:{0} with domaingroup name,exception:{1}", groupName, e);//不同类型的Domaingroup,Name相同情况下，调用Ensureuser方法会抛出此异常，但SPO中已经添加了指定Name的DomainGroup
                    user = mAveParentSite.SPSite.RootWeb.EnsureUser(groupName);
                    userInfo.Login = user.LoginName;
                    isNewAdd = true;
                }
                catch (Exception e)
                {
                    log.Warn("Failed to restore the domaingroup:{0} with domaingroup,exception:{1}", groupName, e);
                    try
                    {
                        string tempLoginName = mAveParentSite.SPSite.RootWeb.GetDomainGroupLoginName(groupName);
                        if (!string.IsNullOrEmpty(tempLoginName))
                        {
                            user = mAveParentSite.SPSite.RootWeb.EnsureUser(tempLoginName);
                            userInfo.Login = user.LoginName;
                            isNewAdd = true;
                        }
                        else
                        {
                            var loginName = GetMappingUserLogin(userInfo.Login, false, true);
                            user = mAveParentSite.SPSite.RootWeb.EnsureUser(loginName);
                            userInfo.Login = user.LoginName;
                            isNewAdd = true;
                            log.Info($"get group by loginName,loginName:{loginName}");
                        }
                        log.Info($"[SAAS-38616]groupName is {groupName}, tempLoginName is {tempLoginName}, isNewAdd is {isNewAdd}");
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Failed to restore the domaingroup:{0} with loginname,exception:{1}", groupName, ex);
                    }
                    finally
                    {
                        if (user == null)
                        {
                            string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(userInfo.Login);
                            if (!String.IsNullOrEmpty(mappingValue))
                            {
                                userInfo.Login = mappingValue;
                            }
                            string existUserLogin = mAveParentSite.SPSite.GetUserLoginBySystemId(userInfo.SystemID);
                            if (!string.IsNullOrEmpty(existUserLogin))
                            {
                                throw new UserAlreadyMappedException(userInfo.Login, existUserLogin);
                            }
                            string defaultUser = mAveParentSite.DefaultUser;
                            if (!string.IsNullOrEmpty(defaultUser))
                            {
                                log.Info("Cannot add user:{0}, will try default user, default username:{1}", userInfo.Login, defaultUser);
                                user = mAveParentSite.SPSite.RootWeb.EnsureUser(defaultUser);
                                userInfo.Login = defaultUser;
                            }
                        }
                    }
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(userInfo.Login, e));
                errorMessage = e.Message;
            }
            return user;
        }

        private IAveUser GetOrAddUser(AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd, ref string errorMessage)
        {
            return GetOrAddUser(userInfo, out needUpdate, out isNewAdd, ref errorMessage, null, false);
        }

        private IAveUser GetOrAddUser(AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd, ref string errorMessage, IAvePrincipalInfo principalInfo, bool isPrincipalLoaded)
        {
            IAveUser user = null;
            needUpdate = false;
            isNewAdd = false;
            string newLogin = string.Empty;
            bool isUserExist = CheckUserExistence(userInfo, ref user, ref newLogin, principalInfo, isPrincipalLoaded);
            if (!isUserExist)
            {
                user = TryAddUser(newLogin, userInfo, out needUpdate, out isNewAdd, ref errorMessage);
            }
            return user;
        }

        public IAveUser GetOrAddUser(string login)
        {
            AveUserInfo info = new AveUserInfo() { Login = login };
            bool needUpdate = false;
            bool isNewAdd = false;
            return GetOrAddUser(info, out needUpdate, out isNewAdd);
        }

        private void RestoreUserSettings(IAveUser spUser, AveUserInfo userInfo, bool siteLevel)
        {
            try
            {

                //The Regional Settings only can be changed by the user self.

                bool sRegionalIsNull = !userInfo.CalendarType.HasValue;
                bool dRegionlIsNull = spUser.RegionalSettings == null;
                bool needUpdateRegionalSettings = mAveParentSite.SPSite.RootWeb.CurrentUser.ID != spUser.ID;

                if (sRegionalIsNull && dRegionlIsNull)
                {
                    needUpdateRegionalSettings = false;
                }

                bool changed = false;

                if (needUpdateRegionalSettings)
                {
                    try
                    {
                        if (spUser.RegionalSettings == null)
                        {
                            spUser.RegionalSettings = mAveParentSite.ObjectModelFactory.CreateRegionalSettings(this.mAveParentSite.SPSite.RootWeb, true);
                            changed = true;
                            // spUser.RegionalSettings = new SPRegionalSettings(
                        }
                        short tempValue = userInfo.WorkDayEndHour.HasValue ? userInfo.WorkDayEndHour.Value : AveWebsTableColumnValue.WorkDayEndHour;
                        if (spUser.RegionalSettings.WorkDayEndHour != tempValue)
                        {
                            spUser.RegionalSettings.WorkDayEndHour = tempValue;
                            changed = true;
                        }
                        tempValue = userInfo.WorkDays.HasValue ? userInfo.WorkDays.Value : AveWebsTableColumnValue.WorkDays;
                        if (spUser.RegionalSettings.WorkDays != tempValue)
                        {
                            spUser.RegionalSettings.WorkDays = tempValue;
                            changed = true;
                        }
                        tempValue = userInfo.WorkDayStartHour.HasValue ? userInfo.WorkDayStartHour.Value : AveWebsTableColumnValue.WorkDayStartHour;
                        if (spUser.RegionalSettings.WorkDayStartHour != tempValue)
                        {
                            spUser.RegionalSettings.WorkDayStartHour = tempValue;
                            changed = true;
                        }
                        tempValue = userInfo.CalendarType.HasValue ? userInfo.CalendarType.Value : AveWebsTableColumnValue.CalendarType;
                        if (spUser.RegionalSettings.CalendarType != tempValue)
                        {
                            spUser.RegionalSettings.CalendarType = tempValue;
                            changed = true;
                        }
                        tempValue = userInfo.AdjustHijriDays.HasValue ? userInfo.AdjustHijriDays.Value : AveWebsTableColumnValue.AdjustHijriDays;
                        if (spUser.RegionalSettings.AdjustHijriDays != tempValue)
                        {
                            spUser.RegionalSettings.AdjustHijriDays = tempValue;
                            changed = true;
                        }
                        tempValue = userInfo.AltCalendarType.HasValue ? userInfo.AltCalendarType.Value : AveWebsTableColumnValue.AlternateCalendarType;
                        if (spUser.RegionalSettings.AlternateCalendarType != tempValue)
                        {
                            spUser.RegionalSettings.AlternateCalendarType = tempValue;
                            changed = true;
                        }
                        //spUser.RegionalSettings.WorkDayEndHour = userInfo.WorkDayEndHour.HasValue ? userInfo.WorkDayEndHour.Value : AveWebsTableColumnValue.WorkDayEndHour;
                        //spUser.RegionalSettings.WorkDays = userInfo.WorkDays.HasValue ? userInfo.WorkDays.Value : AveWebsTableColumnValue.WorkDays;
                        //spUser.RegionalSettings.WorkDayStartHour = userInfo.WorkDayStartHour.HasValue ? userInfo.WorkDays.Value : AveWebsTableColumnValue.WorkDayStartHour;

                        //spUser.RegionalSettings.CalendarType = userInfo.CalendarType.HasValue ? userInfo.CalendarType.Value : AveWebsTableColumnValue.CalendarType;
                        //spUser.RegionalSettings.AdjustHijriDays = userInfo.AdjustHijriDays.HasValue ? userInfo.AdjustHijriDays.Value : AveWebsTableColumnValue.AdjustHijriDays;
                        //spUser.RegionalSettings.AlternateCalendarType = userInfo.AltCalendarType.HasValue ? userInfo.AltCalendarType.Value : AveWebsTableColumnValue.AlternateCalendarType;

                        if (userInfo.Time24.HasValue)
                        {
                            if (spUser.RegionalSettings.Time24 != userInfo.Time24.Value)
                            {
                                spUser.RegionalSettings.Time24 = !spUser.RegionalSettings.Time24;
                                changed = true;
                            }
                        }


                        if (userInfo.CalendarViewOptions.HasValue)
                        {
                            try
                            {
                                tempValue = (short)(((userInfo.CalendarViewOptions.Value & 0x1F) >> 3) % 3);
                                if (spUser.RegionalSettings.FirstWeekOfYear != tempValue)
                                {
                                    spUser.RegionalSettings.FirstWeekOfYear = tempValue;
                                    changed = true;
                                }
                                //spUser.RegionalSettings.FirstWeekOfYear = (short)(((userInfo.CalendarViewOptions.Value & 0x1F) >> 3) % 3);

                                uint firstDayOfWeek = (uint)(userInfo.CalendarViewOptions.Value & 0x07);
                                if (firstDayOfWeek < 0 || firstDayOfWeek > 6)
                                {
                                    firstDayOfWeek = spUser.RegionalSettings.FirstDayOfWeek;
                                }
                                //spUser.RegionalSettings.FirstDayOfWeek = firstDayOfWeek;
                                if (spUser.RegionalSettings.FirstDayOfWeek != firstDayOfWeek)
                                {
                                    spUser.RegionalSettings.FirstDayOfWeek = firstDayOfWeek;
                                    changed = true;
                                }

                                //spUser.RegionalSettings.ShowWeeks = (int)(userInfo.CalendarViewOptions.Value & 0x20) != 0 ? true : false;
                                bool showWeeks = (int)(userInfo.CalendarViewOptions.Value & 0x20) != 0 ? true : false;
                                if (spUser.RegionalSettings.ShowWeeks != showWeeks)
                                {
                                    spUser.RegionalSettings.ShowWeeks = showWeeks;
                                    changed = true;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while set user regionalSettings. error:{0}", e.ToString());
                            }
                        }
                        else
                        {
                            if (spUser.RegionalSettings.FirstWeekOfYear != 0)
                            {
                                spUser.RegionalSettings.FirstWeekOfYear = 0;
                                changed = true;
                            }
                            if (spUser.RegionalSettings.FirstDayOfWeek != 0)
                            {
                                spUser.RegionalSettings.FirstDayOfWeek = 0;
                                changed = true;
                            }
                            if (spUser.RegionalSettings.ShowWeeks)
                            {
                                spUser.RegionalSettings.ShowWeeks = false;
                                changed = true;
                            }
                            //spUser.RegionalSettings.FirstWeekOfYear = 0;
                            //spUser.RegionalSettings.FirstDayOfWeek = 0;
                            //spUser.RegionalSettings.ShowWeeks = false;
                        }

                        if (userInfo.Locale.HasValue)   // if set default value then the "PM" property will be error
                        {
                            //spUser.RegionalSettings.LocaleId = (uint)userInfo.Locale.Value;
                            uint localeId = (uint)userInfo.Locale.Value;
                            if (spUser.RegionalSettings.LocaleId != localeId)
                            {
                                spUser.RegionalSettings.LocaleId = localeId;
                                changed = true;
                            }
                        }
                        ushort timeZoneId = userInfo.TimeZone.HasValue ? (ushort)userInfo.TimeZone.Value : AveUserInfoTableColumnValue.TimeZone;
                        if (spUser.RegionalSettings.TimeZone.ID != timeZoneId)
                        {
                            spUser.RegionalSettings.TimeZone.ID = timeZoneId;
                            changed = true;
                        }

                        //spUser.RegionalSettings.TimeZone.ID = userInfo.TimeZone.HasValue ? (ushort)userInfo.TimeZone.Value : AveUserInfoTableColumnValue.TimeZone;
                        if (siteLevel && userInfo.SiteAdmin && !spUser.IsSiteAdmin)
                        {
                            spUser.IsSiteAdmin = userInfo.SiteAdmin;
                            changed = true;
                        }

                        if (changed)
                        {
                            spUser.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update user regional settings. webUrl:{0}, loginName:{1}, userId:{2}\n error message:{3}", mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, ex));
                        log.Warn("An error occurred while updating user regional settings. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                         mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, ex.ToString());
                    }
                }
                else
                {
                    if (siteLevel && userInfo.SiteAdmin && !spUser.IsSiteAdmin)
                    {
                        spUser.IsSiteAdmin = true;
                        spUser.Update();
                    }
                }
                if (userInfo.Deleted > 0 && userInfo.Deleted == userInfo.ID)
                {
                    this.mAveParentSite.SPSite.RootWeb.SiteUsers.Remove(spUser.LoginName);
                }
                //mAveSPSite.SqlConn.ClearParameters();
                //mAveSPSite.SqlConn.AddParameter("@SiteId", mAveSPSite.SPSite.ID);
                //mAveSPSite.SqlConn.AddParameter("@Id", spUser.ID);
                //mAveSPSite.SqlConn.UpdateTableRow(dic, "UserInfo", ",tp_SystemID,tp_login,tp_Email,tp_Title,tp_Notes,tp_ID,tp_Deleted,tp_IsActive,", " WHERE tp_SiteID=@SiteId and tp_ID=@Id");
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn("An error occurred while updating user information. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                    mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, ex.ToString());
                report.AddDetail(new AveWrapperReportDto("UserSettings", userInfo.Title, AveReportObjectType.UserSettings, AveStatus.Skipped, "You don't have permission to restore user setting. " + ex.Message));
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update user information. webUrl:{0}, loginName:{1}, userId:{2}\n error message:{3}", mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, e));
                log.Warn("An error occurred while updating user information. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                    mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, e.ToString());
            }
        }

        #region Get User Mapping
        public string GetMappingUserLogin(string login)
        {
            return GetMappingUserLogin(login, true);
        }

        public string GetMappingUserLogin(string login, bool needMapping)
        {
            return GetMappingUserLogin(login, false, needMapping);
        }

        public string GetMappingDomainGroupName(string loginName, string title, bool needMapping)
        {
            if (!string.IsNullOrEmpty(title))
            {
                loginName = GetMappingUserLogin(title, needMapping);
            }
            else
            {
                loginName = GetMappingUserLogin(loginName, needMapping);
            }
            return loginName;
        }

        public string GetMappingUserLogin(string login, bool isDomainGroup, bool needMapping)
        {
            string logonName = mappingManager.GetMappingUserLogin(login, isDomainGroup, needMapping);
            if (!string.IsNullOrEmpty(logonName) && logonName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
            {
                if (mAveParentSite.SPSite != null && mAveParentSite.SPSite.IsClassicWindowsModeAuthentication)
                {
                    logonName = logonName.Substring("i:0#.w|".Length);
                }
            }
            return logonName;
        }

        #endregion

        public string ConvertDomainGroupAcountToSid(string account)
        {
            string temp = AveDirectoryServiceUtility.GetSidFromAccount(account);
            if (!string.IsNullOrEmpty(temp))
            {
                return temp;
            }
            return account;
        }

        private void UpdateUserInfoByNative(IAveUser _user, AveUserInfo old)
        {
            string realUserInformationList = "User Information List";
            //string realUserInformationList = mAveParentSite.GetNameByLanguageMapping(userInformationList, AveLanguageMappingType.ListMapping);

            mAveParentSite.SPSite.UpdateUserInfo(realUserInformationList, _user.ID, old);
        }

        public int RestoreGroup(AveGroupInfo groupInfo)
        {
            return RestoreGroup(groupInfo, true);
        }
        public int RestoreGroup(AveGroupInfo groupInfo, bool overWrite)
        {
            return RestoreGroup(groupInfo, true, false);
        }

        public int RestoreGroup(AveGroupInfo groupInfo, bool overWrite, bool skipGroupWithoutPermissions)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Group"))
            {
#endif
                //过滤ShareLink Group
                if (AveSPUtility.MatchShareLink.IsMatch(groupInfo.Title))
                {
                    UserAndDomainMapping.AddUserMapping(groupInfo.ID, groupInfo);
                    return 0;
                }
                string reportMsg = string.Empty;
                AveStatus reportStatus = AveStatus.Successful;
                try
                {
                    if (skipGroupWithoutPermissions == true && groupInfo.HasPermission.HasValue && !groupInfo.HasPermission.Value)
                    {
                        reportStatus = AveStatus.Skipped;
                        reportMsg = "No permission";
                        return -1;////没有权限，不还原
                    }
                    groupInfo.Title = mAveParentSite.GetNameByLanguageMapping(groupInfo.Title, AveLanguageMappingType.PermissionMapping);
                    AveSPMemberInfo memberInfo;
                    object memberObj = UserAndDomainMapping.GetUserMapping(groupInfo.ID);
                    if (memberObj != null)
                    {
                        memberInfo = memberObj as AveSPMemberInfo;
                        if (memberInfo != null)
                        {
                            // This group has already been restored before.
                            reportStatus = AveStatus.Skipped;
                            if (memberInfo.IsUser)
                            {
                                //mLog.Log(AveLogLevel.WARN, "The group id '{0}' is a user id. GroupTitle:{1}", groupInfo.ID, groupInfo.Title);
                                log.Warn("The group id '{0}' is a user id. GroupTitle:{1}", groupInfo.ID, groupInfo.Title);
                                reportMsg = string.Format("The group id '{0}' is a user id. GroupTitle:{1}", groupInfo.ID, groupInfo.Title);
                                return AveSPMemberInfo.FAKE_GROUP.NewId;
                            }
                            return memberInfo.NewId;
                        }
                        //mMapping.Remove(groupInfo.ID);
                        UserAndDomainMapping.RemoveOneUserMapping(groupInfo.ID);
                    }
                    IAveGroup spGroup = null;
                    try
                    {
                        spGroup = mAveParentSite.SPSite.RootWeb.SiteGroups[groupInfo.Title];
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        reportStatus = AveStatus.Skipped;
                        reportMsg = "Access Denied, You don't have permission to restore group. ";
                        //report.AddDetail(new AveWrapperReportDto("Group", groupInfo.Title, AveReportObjectType.Group, AveStatus.Skipped, "You don't have permission to add group. " + ex.Message));
                        log.Log(AveLogLevel.WARN, "An error occurred while add group.Group Title:{0}, error:{1}", groupInfo.Title, ex.ToString());
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotFindGroup, groupInfo.Title, e);
                    }
                    bool isNewAdd = false;
                    if (spGroup == null)
                    {
                        try
                        {
                            mAveParentSite.SPSite.RootWeb.SiteGroups.Add(groupInfo.Title, mAveParentSite.SPSite.RootWeb.CurrentUser, null, groupInfo.Description);
                            spGroup = mAveParentSite.SPSite.RootWeb.SiteGroups[groupInfo.Title];
                            isNewAdd = true;
                            //新创建SharePoint group时，user会自动加到group中,在此处添加清空操作 SAAS-28576
                            foreach (IAveUser user in spGroup.Users)
                            {
                                spGroup.RemoveUser(user);
                            }
                        }
                        catch (Exception e)
                        {
                            reportStatus = AveStatus.Failed;
                            reportMsg = string.Format("An error occurred while add group.Group Title:{0}, error:{1}", groupInfo.Title, e.ToString());
                            log.Log(AveLogLevel.WARN, "An error occurred while add group.Group Title:{0}, error:{1}", groupInfo.Title, e.ToString());
                        }
                    }
                    if (spGroup != null)
                    {
                        memberInfo = new AveSPMemberInfo(spGroup.Name, spGroup.ID, false);
                        memberInfo.SourceInfo = groupInfo;
                        //mMapping[groupInfo.ID] = memberInfo;
                        //log.Info("spGroup is not null,name:{0},Id:{1}", spGroup.Name, spGroup.ID);
                        UserAndDomainMapping.AddUserMapping(groupInfo.ID, memberInfo);
                        log.Info("Add MemberInfo to UserMapping. spGroup is not null. Group Name Is {0}, Group ID is {1}.SPGroupName:{2},SPGroupId:{3}", memberInfo.AccountName, groupInfo.ID,spGroup.Name,spGroup.ID);
                        if (overWrite || isNewAdd)
                        {
                            if (groupInfo.OwnerInfo != null)
                            {
                                RestoreUser(groupInfo.OwnerInfo);
                            }
                            IAveMember owner = FindMember(groupInfo.Owner, false);
                            if (owner == null)
                            {
                            log.Info("Need post update group owner.Group:{0},OwnerId:{1}",groupInfo.Title,groupInfo.Owner);
                                mPostGroup[groupInfo.Title] = groupInfo.Owner;
                            }
                            else
                            {
                                try
                                {
                                    if ((spGroup.Owner == null) || (spGroup.Owner != null && spGroup.Owner.ID != owner.ID))
                                    {
                                        spGroup.Owner = owner;
                                        spGroup.Update();
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while set Group Owner. Group Title:{0}. error:{1}", spGroup.Name, e.ToString());
                                }
                            }
                            RestoreGroupMembersAndSettings(spGroup, groupInfo);
                        }
                        //reportStatus = AveStatus.Successful;
                        return memberInfo.NewId;
                    }
                    else
                    {
                        memberInfo = AveSPMemberInfo.FAKE_GROUP;
                    }
                    //mMapping[groupInfo.ID] = memberInfo;
                    UserAndDomainMapping.AddUserMapping(groupInfo.ID, memberInfo);
                    log.Info("Add MemberInfo to UserMapping.  Group Name Is {0}, Group ID is {1}.", memberInfo.AccountName, groupInfo.ID);
                    //reportStatus = AveStatus.Successful;
                    return memberInfo.NewId;
                }
                finally
                {
                    report.AddDetail(new AveWrapperReportDto(groupInfo.Title, string.Empty, AveReportObjectType.Group, reportStatus, reportMsg));
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreGroups(List<AveGroupInfo> groupInfos, bool overWrite, bool skipGroupWithoutPermissions)
        {
            foreach (AveGroupInfo group in groupInfos)
            {
                RestoreGroup(group, overWrite, skipGroupWithoutPermissions);
            }
        }

        private void RestoreGroupMembersAndSettings(IAveGroup group, AveGroupInfo groupInfo)
        {
            try
            {
                bool changed = false;
                log.Info("Group Source Info.Title:{0},AllowMembersEditMembership:{1},OnlyAllowMembersViewMembership:{2}", 
                    groupInfo.Title, 
                    groupInfo.AllowMembersEditMembership, 
                    groupInfo.OnlyAllowMembersViewMembership);
                log.Info("Group Info.Title:{0},AllowMembersEditMembership:{1},OnlyAllowMembersViewMembership:{2}", 
                    group.Name, 
                    group.AllowMembersEditMembership, 
                    group.OnlyAllowMembersViewMembership);
                if (groupInfo.Memberships != null)
                {
                    foreach (int userId in groupInfo.Memberships)
                    {
                        IAveUser user = this.FindMember(userId, false) as IAveUser;
                        if (user == null)
                        {
                            log.Warn("User {0} was not found in restored user cache,begin to restore it with member info in groupInfo of {1}", 
                                userId,
                                groupInfo.Title);
                            AveUserInfo needRestoreUser = null;
                            foreach (AveUserInfo userInfo in groupInfo.Members)
                            {
                                if (userInfo.ID == userId)
                                {
                                    needRestoreUser = userInfo;
                                    break;
                                }
                            }
                            if (needRestoreUser != null)
                            {
                                log.Info("Begin to restore user in group.User {0}({1})",
                                    needRestoreUser.Title,
                                    needRestoreUser.ID);
                                int newUserId = RestoreUser(needRestoreUser);
                                user = this.FindMember(needRestoreUser.ID, false) as IAveUser;
                                if (user == null)
                                {
                                    log.Warn("User {0}({1}) was not found after restore user.Try to find it with newUserId if newUserId is avaliable.",
                                        needRestoreUser.Title,
                                        needRestoreUser.ID);
                                    if (newUserId > 0)
                                    {
                                        user = ParentSite.SPSite.RootWeb.Users.GetByID(newUserId);
                                        if (user == null)
                                        {
                                            log.Warn("Find user with newUserId {0} failed.Skip restoring the specific member to this group.", newUserId);
                                        }
                                    }
                                    else
                                    {
                                        log.Warn("NewUserId {0} is not valid,this user may not restored.", newUserId);
                                    }
                                }
                            }
                            else
                            {
                                log.Warn("Information of user with id {0} was not found in group's members.,GroupTitle:{1}", userId, groupInfo.Title);
                            }
                            if (user == null)
                            {
                                log.Warn("User object is null, skip restoring user with id {0} to current group {1}({2})",
                                    userId,
                                    group.Name,
                                    group.ID);
                                continue;
                            }
                        }
                        var groupUser=group.Users.GetByLoginName(user.LoginName);
                        if (groupUser == null)
                        {
                            try
                            {
                                log.Info("Add user {0}({1}) to group {2}(3)",
                                    user.LoginName, user.ID, group.Name, group.ID);
                                group.AddUser(user);
                                changed = true;
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Restore group member failed.Add {0}({1}) to group {2}(3). Error:{4}",
                                    user.LoginName, user.ID, group.Name, group.ID,
                                    ex);
                            }
                        }
                        else
                        {
                            log.Info("User {0}({1}) already exist in group {2}({3}),don't need to add it again.RestoredUser:{4}({5})",
                                groupUser.Name,
                                groupUser.ID,
                                group.Name,
                                group.ID,
                                user.Name,
                                user.ID);
                        }
                    }
                }
                else
                {
                    log.Warn("Group {0}'s member ship is null,don't restore group members.",groupInfo.Title);
                }
                Type groupType = group.GetType();
                if (String.IsNullOrEmpty(groupInfo.DLAlias))
                {
                    groupInfo.DLAlias = null;
                }
                if (string.Compare(group.DistributionGroupAlias, groupInfo.DLAlias, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    group.DistributionGroupAlias = groupInfo.DLAlias;
                    changed = true;
                    //AveAssemblyUtility.SetPropertyValue(group, "DistributionGroupAlias", groupInfo.DLAlias);
                }
                // can not update this property, so remove it
                //if (string.Compare(group.DistributionGroupErrorMessage, groupInfo.DLErrorMessage, StringComparison.OrdinalIgnoreCase) != 0)
                //{
                //    group.DistributionGroupErrorMessage = groupInfo.DLErrorMessage;
                //    changed = true;
                //    //AveAssemblyUtility.SetPropertyValue(group, "DistributionGroupErrorMessage", groupInfo.DLErrorMessage);
                //}
                if (string.Compare(group.Description, groupInfo.Description, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (groupInfo.Description != null)
                    {
                        group.Description = groupInfo.Description;
                        changed = true;
                    }
                }
                if (string.Compare(group.RequestToJoinLeaveEmailSetting, groupInfo.RequestEmail, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    group.RequestToJoinLeaveEmailSetting = groupInfo.RequestEmail;
                    changed = true;
                }
                if (group.AllowRequestToJoinLeave != groupInfo.AllowRequestToJoinLeave)   //SAAS-8191 备份时Flags无法实现，添加相应属性并判断。
                {
                    group.AllowRequestToJoinLeave = !group.AllowRequestToJoinLeave;
                    changed = true;
                }
                if (group.AutoAcceptRequestToJoinLeave != groupInfo.AutoAcceptRequestToJoinLeave)
                {
                    group.AutoAcceptRequestToJoinLeave = !group.AutoAcceptRequestToJoinLeave;
                    changed = true;
                }
                if (group.AllowMembersEditMembership != groupInfo.AllowMembersEditMembership)
                {
                    group.AllowMembersEditMembership = !group.AllowMembersEditMembership;
                    changed = true;
                }
                if (group.OnlyAllowMembersViewMembership != groupInfo.OnlyAllowMembersViewMembership)
                {
                    group.OnlyAllowMembersViewMembership = !group.OnlyAllowMembersViewMembership;
                    changed = true;
                }

                //group.AllowRequestToJoinLeave = (groupInfo.Flags & 4) != 0;
                //group.AutoAcceptRequestToJoinLeave = (groupInfo.Flags & 8) != 0;
                //group.AllowMembersEditMembership = (groupInfo.Flags & 2) != 0;
                //group.OnlyAllowMembersViewMembership = (groupInfo.Flags & 1) != 0;
                if (changed)//如果没有变化最好不要update，因为SharePoint每次update都会调用request走一遍
                {
                    log.Info("Update group setting");
                    group.Update();
                    log.Info("After update,group Info.Title:{0},AllowMembersEditMembership:{1},OnlyAllowMembersViewMembership:{2}", group.Name, group.AllowMembersEditMembership, group.OnlyAllowMembersViewMembership);
                }
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update group's members. SiteId:{0}, group title:{1}\n error message:{2}", mAveParentSite.SPSite.ID, groupInfo.Title, ex));
                log.Warn("An error occurred while updating group's members. SiteId:{0}, Title:{1}, error:{2}", mAveParentSite.SPSite.ID, groupInfo.Title, ex.ToString());
            }
        }

        public void RestoreMembers(AveSecurityInfo securityInfo)
        {
            if (securityInfo.Users != null)
            {
                foreach (AveUserInfo userInfo in securityInfo.Users)
                {
                    try
                    {
                        RestoreUser(userInfo);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore user. user title:{0}, user id:{1}\n error message:{2}", userInfo.Title, userInfo.ID, e));
                        //mLog.Log(AveLogLevel.WARN, "WP10RTSPMembe639 {0} , {1}, {2}", userInfo.Title, userInfo.ID, e);
                    }
                }
            }
            if (securityInfo.Groups != null)
            {
                foreach (AveGroupInfo groupInfo in securityInfo.Groups)
                {
                    try
                    {
                        RestoreGroup(groupInfo);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore group. group title:{0}, group id:{1}\n error message:{2}", groupInfo.Title, groupInfo.ID, e));
                    }
                }
            }
        }

        public void RestoreGroupOwner()
        {
            foreach (string name in mPostGroup.Keys)
            {
                int ownerId = mPostGroup[name];
                if (ownerId <= 0)
                {
                    continue;
                }
                try
                {
                    IAveGroup group = mAveParentSite.SPSite.RootWeb.SiteGroups[name];
                    IAveMember owner = FindMember(ownerId, false);
                    if (owner != null)
                    {
                        if (owner.ID != group.Owner.ID)
                        {
                            group.Owner = owner;
                            group.Update();
                        }
                    }
                    else
                    {
                        log.Warn("Cannot find owner. Group Title:{0}", group.Name);
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while RestoreGroupOwner. error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("GroupOwner", "GroupOwner", AveReportObjectType.GroupOwner, AveStatus.Skipped, "You don't have permission to restore Group Owner. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while RestoreGroupOwner. error:{0}", e.ToString());
                }
            }
        }

        public void LoadUsers(List<AveUserInfo> userInfos)
        {
            if (userInfos != null)
            {
                foreach (AveUserInfo userInfo in userInfos)
                {
                    //mMapping[userInfo.ID] = userInfo;
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, userInfo);
                }
            }
        }

        public void LoadGroups(List<AveGroupInfo> groups)
        {
            if (groups != null)
            {
                foreach (AveGroupInfo groupInfo in groups)
                {
                    //mMapping[groupInfo.ID] = groupInfo;
                    UserAndDomainMapping.AddUserMapping(groupInfo.ID, groupInfo);
                }
            }
        }

        public void LoadMembers(AveSecurityInfo securityInfo)
        {
            if (securityInfo.Users != null)
            {
                foreach (AveUserInfo userInfo in securityInfo.Users)
                {
                    //mMapping[userInfo.ID] = userInfo;
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, userInfo);
                }
            }
            if (securityInfo.Groups != null)
            {
                foreach (AveGroupInfo groupInfo in securityInfo.Groups)
                {
                    //mMapping[groupInfo.ID] = groupInfo;
                    UserAndDomainMapping.AddUserMapping(groupInfo.ID, groupInfo);
                }
            }
        }

        /// <summary>
        /// Find the correspond member in the current site
        /// If cannot find, create it if createIfNotExist==true.
        /// </summary>
        /// <param name="id">the old id</param>
        /// <param name="createIfNotExist">if need create the member if it does not exist in the site</param>
        /// <returns>
        /// the found member or
        /// null if cannot find it
        /// </returns>
        public IAvePrincipal FindMember(int id, bool createIfNotExist, bool skipGroupWithoutPermissions)
        {
            // find the AveSPMemberInfo from the mMapping
            // if mNewId is not zero, return mNewId
            try
            {
                object member = UserAndDomainMapping.GetUserMapping(id);
                if (member == null)
                {
                    return null;
                }
                int newId;
                bool isUser;
                if (member is AveUserInfo && createIfNotExist)
                {
                    newId = RestoreUser((AveUserInfo)member);
                    isUser = true;
                }
                else if (member is AveGroupInfo && createIfNotExist)
                {
                    newId = RestoreGroup((AveGroupInfo)member, true, skipGroupWithoutPermissions);
                    isUser = false;
                }
                else if (member is AveSPMemberInfo)
                {
                    newId = ((AveSPMemberInfo)member).NewId;
                    isUser = ((AveSPMemberInfo)member).IsUser;
                }
                else
                {
                    return null;
                }

                if (isUser && newId == AveSPMemberInfo.FAKE_USER.NewId)
                {
                    // This user has already been restored before and it failed.
                    // So we just return null to show we cannot find this user.
                    return null;
                }
                if (!isUser && newId == AveSPMemberInfo.FAKE_GROUP.NewId)
                {
                    // This group has already been restored before and it failed.
                    // So we just return null to show we cannot find this group.
                    return null;
                }
                if (newId != 0)
                {
                    if (isUser)
                    {
                        return mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(newId);
                    }
                    else
                    {
                        return mAveParentSite.SPSite.RootWeb.SiteGroups.GetByID(newId);
                    }
                }
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while find member. member id:{0}, createIfExist:{1}\n error message:{2}", id, createIfNotExist, e));
                log.Warn("An error occurred while finding member. MemberId:{0}, CreateIfExist:{1}, error:{2}", id, createIfNotExist, e.ToString());
            }
            return null;
        }

        public IAvePrincipal FindMember(int id, bool createIfNotExist)
        {
            return FindMember(id, createIfNotExist, false);
        }

        public IAvePrincipal FindMember(int id)
        {
            // find the AveSPMemberInfo from the mMapping
            // if mNewId is not zero, return mNewId
            try
            {
                object member = UserAndDomainMapping.GetUserMapping(id);
                int newId;
                bool isUser = true;
                if (member == null)
                {
                    newId = 0;
                }
                if (member is AveUserInfo)
                {
                    newId = RestoreUser((AveUserInfo)member);
                    isUser = true;
                }
                else if (member is AveGroupInfo)
                {
                    newId = RestoreGroup((AveGroupInfo)member);
                    isUser = false;
                }
                else if (member is AveSPMemberInfo)
                {
                    newId = ((AveSPMemberInfo)member).NewId;
                    isUser = ((AveSPMemberInfo)member).IsUser;
                }
                else
                {
                    newId = 0;
                }

                if (isUser && newId == AveSPMemberInfo.FAKE_USER.NewId)
                {
                    // This user has already been restored before and it failed.
                    // So we just return null to show we cannot find this user.
                    newId = 0;
                }
                if (!isUser && newId == AveSPMemberInfo.FAKE_GROUP.NewId)
                {
                    // This group has already been restored before and it failed.
                    // So we just return null to show we cannot find this group.
                    newId = 0;
                }
                if (newId == 0)
                {
                    string defaultUser = mAveParentSite.DefaultUser;
                    if (!string.IsNullOrEmpty(defaultUser))
                    {
                        try
                        {
                            newId = mAveParentSite.SPSite.RootWeb.EnsureUser(defaultUser).ID;
                        }
                        catch (Exception e)
                        {
                            log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(defaultUser, e));
                            newId = mAveParentSite.CURRENT_USER_ID;
                        }
                    }
                    else
                    {
                        newId = mAveParentSite.CURRENT_USER_ID;
                    }
                }
                if (newId != 0)
                {
                    if (isUser)
                    {
                        return mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(newId);
                    }
                    else
                    {
                        return mAveParentSite.SPSite.RootWeb.SiteGroups.GetByID(newId);
                    }
                }
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while find member. member id:{0}, createIfExist:{1}\n error message:{2}", id, createIfNotExist, e));
                log.Warn("An error occurred while finding member. MemberId:{0}, error:{2}", id, e.ToString());
            }
            return null;
        }

        //find new userId from oldUserId
        public int FindMemberId(int oldUserId)
        {
            //system account
            if (oldUserId == AveConstants.SYSTEM_ACCOUNT_ID)
            {
                return oldUserId;
            }
            int userId = FindMemberId(oldUserId, true);

            if (userId == -1)
            {
                string defaultUser = mAveParentSite.DefaultUser;
                if (!string.IsNullOrEmpty(defaultUser))
                {
                    try
                    {
                        return mAveParentSite.SPSite.RootWeb.EnsureUser(defaultUser).ID;
                    }
                    catch (Exception e)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(defaultUser, e));
                    }
                }
                return mAveParentSite.CURRENT_USER_ID;
            }
            return userId;
        }

        //Doc2466 for workflow update user id in workflow instancedata.
        public int CreateAndFindMemberId(string oldLoginName)
        {
            int oldId = 0;
            int newId = 0;
            foreach (KeyValuePair<int, object> entry in UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                if (entry.Value is AveSPMemberInfo)
                {
                    AveSPMemberInfo memberInfo = (AveSPMemberInfo)entry.Value;
                    if (memberInfo.IsUser)
                    {
                        if (string.Compare(memberInfo.AccountName, oldLoginName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            newId = memberInfo.NewId;
                            oldId = entry.Key;
                            break;
                        }
                        else if (memberInfo.AccountName.LastIndexOf('|') > 0)
                        {
                            string tempName = memberInfo.AccountName.Substring(memberInfo.AccountName.LastIndexOf("|") + 1);  //SAAS-14470 有前缀的时候去掉前缀进行比较。
                            if (string.Compare(tempName, oldLoginName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                newId = memberInfo.NewId;
                                oldId = entry.Key;
                                break;
                            }
                        }
                    }
                }
                else if (entry.Value is AveUserInfo)
                {
                    if (string.Compare(((AveUserInfo)entry.Value).Login, oldLoginName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        oldId = entry.Key;
                        newId = 0;
                    }
                }
            }
            if (oldId == 0)
            {
                return mAveParentSite.CURRENT_USER_ID;
            }
            if (newId > 0)
            {
                return newId;
            }
            if (newId == 0)
            {
                return FindMemberId(oldId);
            }
            return mAveParentSite.CURRENT_USER_ID;
        }

        public string CreateAndFindMemberLoginName(string oldLoginName)
        {
            var mappingLoginName = GetMappingUserLogin(oldLoginName);
            IAvePrincipalInfo info = mAveParentSite.ObjectModelFactory.Utility.ResolvePrincipal(mAveParentSite.SPSite.RootWeb, mappingLoginName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
            if (info != null)
            {
                return info.LoginName;
            }

            IAveUser user = this.mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(mAveParentSite.CURRENT_USER_ID);
            return user != null ? user.LoginName : null;
        }

        public void RestoreGroups(List<AveGroupInfo> groupsInfo)
        {
            IAveGroupCollection currentGroups = mAveParentSite.SPSite.RootWeb.SiteGroups;
            List<AveGroupCreationInformation> needCreateGroups = new List<AveGroupCreationInformation>();
            foreach (AveGroupInfo groupInfo in groupsInfo)
            {
                AveGroupCreationInformation gci = new AveGroupCreationInformation();
                gci.Title = groupInfo.Title;
                gci.Description = groupInfo.Description;
                needCreateGroups.Add(gci);
            }
            currentGroups.Add(needCreateGroups);
        }

        //public Dictionary<int, object> Mapping { get { return mMapping; } }


        /// <summary>
        /// Find the correspond member id in the current site
        /// If cannot find, create it if createIfNotExist==true.
        /// </summary>
        /// <param name="id">the old id</param>
        /// <param name="createIfNotExist">if need create the member if it does not exist in the site</param>
        /// <returns>
        /// the found member or
        /// null if cannot find it
        /// </returns>
        public int FindMemberId(int id, bool createIfNotExist)
        {
            // find the AveSPMemberInfo from the mMapping
            // if mNewId is not zero, return mNewId
            try
            {
                object member = UserAndDomainMapping.GetUserMapping(id);
                if (member == null)
                {
                    return -1;
                }

                int newId;
                if (member is AveUserInfo)
                {
                    newId = RestoreUser((AveUserInfo)member);
                }
                else if (member is AveGroupInfo)
                {
                    newId = RestoreGroup((AveGroupInfo)member);
                }
                else if (member is AveSPMemberInfo)
                {
                    newId = ((AveSPMemberInfo)member).NewId;
                }
                else
                {
                    return -1;
                }

                if (newId != 0)
                {
                    return newId;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while find member. member id:{0}, createIfExist:{1}\n error message:{2}", id, createIfNotExist, e));
                //mLog.Warn(e, "An error occurred while finding member. MemberId:{0}, CreateIfExist:{1}", id, createIfNotExist);
            }
            return -1;
        }
    }

    public class AveSPMemberInfo
    {
        public static readonly AveSPMemberInfo FAKE_USER = new AveSPMemberInfo(string.Empty, -1, true);
        public static readonly AveSPMemberInfo FAKE_GROUP = new AveSPMemberInfo(string.Empty, -1, false);
        public int NewId; // new id in the sharepoint server
        public string AccountName; // the security account name, use login name now.
        public bool IsUser; // ture is user, false is group
        public object SourceInfo;


        public AveSPMemberInfo()
        {
            NewId = 0;
            AccountName = null;
            IsUser = true;
        }

        public AveSPMemberInfo(string loginName, int newid, bool _user)
        {
            NewId = newid;
            AccountName = loginName;
            IsUser = _user;
        }

        public override string ToString()
        {
            return string.Format("NewId:{0},AccountName:{1},IsUser:{2},SourceInfoIsNull:{3}", NewId,AccountName,IsUser,SourceInfo==null);
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
            if (login.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase)
                || login.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase)
                || login.Equals("NT AUTHORITY\\local service", StringComparison.OrdinalIgnoreCase))
            {
                return login;
            }
            if (!needMapping && !IsSP10FBAUser(login))
            {
                return login;
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
                                        if (!string.Equals(cbaMappingResult, login))
                                        {
                                            resultLogin = cbaMappingResult;
                                        }
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
            var loginSplitStrings = loginName.Split('\\', '@');
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
            if (mappingDomainName.EndsWith(":"))
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
    }
}
