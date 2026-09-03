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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.Common;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPMembers : IDisposable, AvePoint.Wrapper.Restore.IAveSPMembers
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private Dictionary<int, object> mMapping;
        protected AveSPSite mAveParentSite;
        protected AveSPUserMappingManager mappingManager;
        protected IReport report = new AveWrapperReport();

        protected object reportPrivateLock = new object();

        internal MembersRestoreOption defaultOption = new MembersRestoreOption()
        {
            IsSiteLevel = false,
            OverWrite = true,
            SkipWithoutPermissions = false,
            NeedDeleteUser = true
        };

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
            internal set
            {
                mUserAndDomainMapping = value;
            }
        }

        protected ThreadSafeDictionary<string, int> UserLoginAndIdMapping
        {
            get
            {
                return userloginandIdMapping;
            }
        }
        
        private ThreadSafeDictionary<string, string> userloginCacheMapping = new ThreadSafeDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private ThreadSafeDictionary<string, int> userloginandIdMapping = new ThreadSafeDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        protected IAveRestoreStream mReader = null;
        protected AveFileRestoreStream mStream = null;
        // map the backup id with the restore id of user or group, 
        // key is the backup id, the value is the Member Info
        protected Dictionary<string, int> mPostGroup = new Dictionary<string, int>();
        protected List<string> mAllGroups = null;
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
                            mAllGroups.Add(group.LoginName.ToLower(CultureInfo.InvariantCulture));
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

        public MembersRestoreOption DefaultOption
        {
            get
            {
                return defaultOption;
            }
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
            if (UserLoginAndIdMapping != null)
            {
                UserLoginAndIdMapping.Clear();
            }
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

        private bool NeedSkipLocalBuiltInUser(AveUserInfo userInfo)
        {
            if (mAveParentSite.SPSite != null && mAveParentSite.SPSite.IsClassicWindowsModeAuthentication
               && userInfo.Login.Equals("c:0(.s|true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private bool NeedSkipUser(AveUserInfo userInfo, MembersRestoreOption option, ref AveReportResource key, ref AveStatus reportStatus, ISPImportProfiler profiler)
        {
            bool isNeedSkip = false;
            if ((option.SkipWithoutPermissions && NeedSkipWithoutPermissions(userInfo)))
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Url = ParentSite.SiteUrl, Type = SPObjectType.User, Title = userInfo.Login, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_NoPermissionOrActiveUser, userInfo.Login), Status = WrapperRestoreStatus.Skipped, Level = SPObjectLevel.SiteCollection }); }
                key = AveReportResource.Wrapper_Report_SkipTheNoPermissionUser;
                isNeedSkip = true;
            }
            else if (NeedSkipLocalBuiltInUser(userInfo) || NeedSkipClientBuiltInUser(userInfo))
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Url = ParentSite.SiteUrl, Type = SPObjectType.User, Title = userInfo.Login, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_SkipRestoreBuiltinUser, userInfo.Login), Status = WrapperRestoreStatus.Skipped, Level = SPObjectLevel.SiteCollection }); }
                key = AveReportResource.Wrapper_Report_SkipBuiltInUser;
                isNeedSkip = true;
            }
            if (isNeedSkip)
            {
                reportStatus = AveStatus.Skipped;
                if (option.CacheSkippedUserInfo)
                {
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, userInfo);
                }
                else
                {
                    AveSPMemberInfo memberInfo = new AveSPMemberInfo(string.Empty, -1, true);
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                }

            }
            return isNeedSkip;
        }

        public object GetMemberObjectByLogin(string login)
        {
            object member = null;
            int memberId;
            lock (userloginandIdMapping)
            {
                if (UserLoginAndIdMapping.TryGetValue(login, out memberId))
                {
                    member = UserAndDomainMapping.GetUserMapping(memberId);
                }
            }
            return member;
        }

        public virtual void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            foreach (var userinfo in allUsers)
            {
                RestoreUser(userinfo, option, profiler);
            }
        }

        public virtual void RestoreUsers(IList<AveUserInfo> allUsers, MembersRestoreOption option)
        {
            foreach (var userinfo in allUsers)
            {
                RestoreUser(userinfo, option, null);
            }
        }

        [Obsolete("Need Delete,because the profiler need be sent in")]
        public int RestoreUser(AveUserInfo userInfo, MembersRestoreOption option)
        {
            //TODO  Need Delete
            //var profiler = new DefaultRestoreSiteProfiler();
            return RestoreUser(userInfo, option, null);
        }

        public virtual int RestoreUser(AveUserInfo userInfo, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.User"))
            {
                AddUserLoginAndIdMapping(userInfo);
                if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Status = WrapperRestoreStatus.None, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_StartRestoreUser, userInfo.Login), Title = userInfo.Login, Type = SPObjectType.User, Url = mAveParentSite.SiteUrl }); }

                this.defaultOption = option.Clone();
                bool isNeedReport = true;
                int userId = AveSPMemberInfo.FakeId;
                AveStatus reportStatus = AveStatus.Successful;
                string srcLoginName = userInfo.Login;
                String objectTitle = mAveParentSite.SPSite.RootWeb.Title;
                AveReportResource key = AveReportResource.Wrapper_Report_None;
                IAveUser spUser = null;
                string newLogin = String.Empty;
                try
                {
                    AveSPMemberInfo memberInfo;
                    object memberObj = UserAndDomainMapping.GetUserMapping(userInfo.ID);
                    if (memberObj != null)
                    {
                        memberInfo = memberObj as AveSPMemberInfo;
                        if (memberInfo != null)
                        {
                            isNeedReport = false;
                            reportStatus = AveStatus.Skipped;
                            if (!memberInfo.IsUser)
                            {
                                //mLog.Log(AveLogLevel.WARN, string.Format("The user id:{0} is group id. LoginName:{1}", userInfo.ID, userInfo.Login));
                                log.Warn("The user id '{0}' is group id. LoginName:{1}", userInfo.ID, userInfo.Login);
                                key = AveReportResource.Wrapper_Report_TheUserIsGroup;
                                return userId = AveSPMemberInfo.FAKE_USER.NewId;
                            }
                            // This user has already been restored, just return its information.
                            return userId = memberInfo.NewId;
                        }
                        else
                        {
                            UserAndDomainMapping.RemoveOneUserMapping(userInfo.ID);
                        }
                    }
                    if (NeedSkipUser(userInfo, option, ref key, ref reportStatus, profiler))
                    {
                        return userId;
                    }
                    bool needUpdate = false;
                    bool isNewAdd = false;
               
                    spUser = GetOrAddUser(userInfo, out needUpdate, out isNewAdd,out newLogin, profiler);

                    // add non-new and add user to mapping list
                    if (spUser != null && !isNewAdd)
                    {
                        //ADO-61868
                        var tmp = UserAndDomainMapping.EnumUserMapping().Select(info => info.Value as AveSPMemberInfo).Where(info => info != null && info.NewId == spUser.ID).ToList<AveSPMemberInfo>();
                        if (tmp.Count > 0)
                        {
                            memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                            memberInfo.SourceInfo = userInfo;
                            UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                            key = AveReportResource.Wrapper_Report_UserHasRestored;
                            reportStatus = AveStatus.Skipped;
                            log.Log(AveLogLevel.WARN, String.Format(WrapperRestoreResource.UserHasRestored, spUser.ID));
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

                    if (spUser != null)
                    {
                        memberInfo = new AveSPMemberInfo(spUser.LoginName, spUser.ID, true);
                        RestoreUserSettings(spUser, userInfo, option, profiler);
                    }
                    else
                    {
                        //memberInfo = AveSPMemberInfo.FAKE_USER;
                        memberInfo = new AveSPMemberInfo(string.Empty, AveSPMemberInfo.FakeId, true);
                    }
                    memberInfo.SourceInfo = userInfo;
                    UserAndDomainMapping.AddUserMapping(userInfo.ID, memberInfo);
                    ParentSite.MappingManager.SiteMappingManager.AddUserLoginNameMapping(userInfo.Login, memberInfo.AccountName);

                    if (needUpdate)
                    {
                        UpdateUserInfoByNative(spUser, userInfo);
                    }

                    reportStatus = isNewAdd ? AveStatus.Successful : (memberInfo.NewId == AveSPMemberInfo.FakeId ? AveStatus.Failed : AveStatus.Skipped);
                    if (reportStatus == AveStatus.Skipped)
                    {
                        key = AveReportResource.Wrapper_Report_UserRestoreSkipError;
                    }
                    if (reportStatus == AveStatus.Failed)
                    {
                        key = AveReportResource.Wrapper_Report_UserRestoreFailedError;
                    }
                    return userId = memberInfo.NewId;
                }
                catch (AveSecurityTrimingException ex)
                {
                    string reportLoginName = !String.IsNullOrEmpty(newLogin) ? newLogin : srcLoginName;
                    log.Warn("An error occurred while restore user. {0}" + reportLoginName, ex);
                    reportStatus = AveStatus.Skipped;
                    key = AveReportResource.Wrapper_Report_NoPermissionToRestoreUser;
                    return AveSPMemberInfo.FakeId;
                }
                catch (Exception ex)
                {
                    string reportLoginName = !String.IsNullOrEmpty(newLogin) ? newLogin : srcLoginName;
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(reportLoginName, ex));
                    reportStatus = AveStatus.Failed;
                    key = AveReportResource.Wrapper_Report_UserRestoreFailedError;
                    return AveSPMemberInfo.FakeId;
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
                            reportLoginName = !String.IsNullOrEmpty(newLogin) ? newLogin : srcLoginName;
                        }
                        log.Debug("Restore user name: {0}, the restore status is {1}.", reportLoginName, reportStatus);
                        lock (reportPrivateLock)
                        {
                            report.AddDetail(new AveWrapperReportDto(reportLoginName, objectTitle, AveReportObjectType.User, reportStatus, key, reportLoginName));
                        }
                    }
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "spo-grid-all-users is SharePoint Online built-in user format")]
        protected bool NeedSkipClientBuiltInUser(AveUserInfo userInfo)
        {
            bool isBuiltinUser = false;
            if (userInfo.Login.StartsWith("i:0#.w|ylo001\\", StringComparison.OrdinalIgnoreCase)
                || userInfo.Login.StartsWith("c:0-.f|rolemanager|spo-grid-all-users/", StringComparison.OrdinalIgnoreCase))
            {
                isBuiltinUser = true;
            }
            return isBuiltinUser;
        }

        protected bool NeedSkipWithoutPermissions(AveUserInfo userInfo)
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

        private string GetBuiltInUserLoginNameMapping(AveUserInfo userInfo)
        {
            string mappingValue = string.Empty;
            if (userInfo.SystemID != null)
            {
                string sid = AveDirectoryServiceUtility.ConvertBytesToStringSid(userInfo.SystemID);
                if (sid.Equals("S-1-5-11", StringComparison.OrdinalIgnoreCase) || sid.Equals("S-1-5-19", StringComparison.OrdinalIgnoreCase))//NT AUTHORITY\authenticated users(S-1-5-11)和 NT AUTHORITY\local service(S-1-5-19)的过滤，loginname与语言环境相关
                {
                    mappingValue = GetBuiltInUserLoginName(sid);
                }
            }
            return mappingValue;
        }

        private string GetBuiltInUserLoginName(string sid)
        {
            string mappingValue = string.Empty;
            try
            {
                System.Security.Principal.SecurityIdentifier builtInUserIdentifier = new System.Security.Principal.SecurityIdentifier(sid);
                var userValue = builtInUserIdentifier.Translate(typeof(System.Security.Principal.NTAccount));
                mappingValue = userValue.Value;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting built in user login name by sid:{0}. Error message:{1}", sid, e.ToString());
            }
            return mappingValue;
        }

        [Obsolete("Need Delete,because the profiler need be sent in")]
        protected IAveUser GetOrAddUser(AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd)
        {
            //TODO  Need Delete
            //var profiler = new DefaultRestoreSiteProfiler();
            string newLogin = string.Empty;
            return GetOrAddUser(userInfo, out needUpdate, out isNewAdd, out newLogin, null);
        }

        protected IAveUser GetOrAddUser(AveUserInfo userInfo, out bool needUpdate, out bool isNewAdd, out string newLogin, ISPImportProfiler profiler)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.GetOrAddUser"))
            {
                IAveUser user = null;
                needUpdate = false;
                isNewAdd = false;
                IAvePrincipalInfo info = null;
                newLogin = string.Empty;
                try
                {
                    newLogin = GetMappingUserLogin(userInfo.Login, true);
                    if (userInfo.Login.EndsWith(newLogin, StringComparison.OrdinalIgnoreCase) && userInfo.SystemID != null && userInfo.SystemID.Length == 12)
                    {
                        string mappingBuiltInUserLoginName = GetBuiltInUserLoginNameMapping(userInfo);
                        newLogin = string.IsNullOrEmpty(mappingBuiltInUserLoginName) ? newLogin : mappingBuiltInUserLoginName;
                    }
                    IAveUtility utility = mAveParentSite.ObjectModelFactory.Utility;
                    info = utility.ResolvePrincipal(mAveParentSite.SPSite.RootWeb, newLogin, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                    if (info == null || (info.PrincipalID < 0 && !mAveParentSite.SPSite.IsOnlineSite))
                    {
                        //throw new AveSPInfoException(AveSPResource.GetString("UserCouldNotBeFound", new object[] { userInfo.Login }));
                        //throw new AveException("Cannot find user. LoginName:{0}.", new object[] { userInfo.Login });
                    }
                    else
                    {
                        user = mAveParentSite.SPSite.RootWeb.SiteUsers[info.LoginName];
                        string ownerLoginName = mAveParentSite.SPSite.Owner.LoginName;
                        //ADO-58292 SP2013环境MySite上存在两个同名的user（site Owner对应的user），一个带前缀("i:0#.w|")，一个不带。还原User时需要匹配到带前缀的user上。
                        if (ownerLoginName.IndexOf('|') > 0 && user.LoginName.Equals(ownerLoginName.Substring(ownerLoginName.IndexOf('|') + 1), StringComparison.OrdinalIgnoreCase))
                        {
                            user = mAveParentSite.SPSite.Owner;
                            newLogin = user.LoginName;
                        }
                        userInfo.Login = newLogin;
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = newLogin, Status = WrapperRestoreStatus.Successful, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_FindUserInDestinationSuccessfully, newLogin), Level = SPObjectLevel.SiteCollection }); }
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.DEBUG, string.Format("Cannot find user. LoginName:{0}\n error message:{1}", userInfo.Login, e));
                    log.Debug("Cannot find user. LoginName:{0}, error:{1}", userInfo.Login, e.ToString());
                }

                if (user == null)
                {
                    try
                    {
                        string login = info == null ? newLogin : info.LoginName;
                        try
                        {
                            //[ADO-56634] Sometimes we can't ensureuser if the AllowUnsafeUpdates property is false. Changed by Austin Han
                            if (!mAveParentSite.SPSite.RootWeb.AllowUnsafeUpdates)
                            {
                                mAveParentSite.SPSite.RootWeb.AllowUnsafeUpdates = true;
                            }
                            user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(login);
                            userInfo.Login = login;
                            isNewAdd = true;

                            if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = newLogin, Status = WrapperRestoreStatus.Successful, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserSuccessfully, newLogin), Level = SPObjectLevel.SiteCollection }); }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByNameError, e);
                            //Office not support place holder
                            if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                            {
                                string placeHoldAccount = mAveParentSite.GetPlaceHolderAccount();
                                if (string.IsNullOrEmpty(placeHoldAccount))
                                {
                                    throw;
                                }
                                string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(userInfo.Login);
                                if (!String.IsNullOrEmpty(mappingValue))
                                {
                                    userInfo.Login = mappingValue;
                                }
                                if (userInfo.Login.Equals("c:0(.s|true", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw;//All Authenticated Users  not need do place holder, ADO-49955, ADO-49963
                                }
                                //string existUserLogin = mAveParentSite.SPSite.GetUserLoginBySystemId(userInfo.SystemID);
                                //if (!string.IsNullOrEmpty(existUserLogin))
                                //{
                                //    if (!mAveParentSite.SPSite.ActiveDeletedUserBySystemId(userInfo.SystemID))
                                //    {
                                //        throw;
                                //    }
                                //    user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(login, false);
                                //    userInfo.Login = login;
                                //    isNewAdd = true;

                                //}
                                //else
                                //{
                                //    log.Info("Cannot add user:{0}, will use placeHolder to add user, placeholder:{1}", userInfo.Login, placeHoldAccount);
                                //    user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(placeHoldAccount);

                                //}

                                user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(placeHoldAccount);
                                userInfo.SystemID = userInfo.SystemID ?? Guid.NewGuid().ToByteArray();
                                this.mAveParentSite.SPSite.MigrateUser(user.LoginName, user.GetBinaryId(), userInfo.Login, userInfo.SystemID);
                                newLogin = userInfo.Login;
                                isNewAdd = true;
                                needUpdate = true;
                                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = newLogin, Status = WrapperRestoreStatus.Successful, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserSuccessfully, newLogin), Level = SPObjectLevel.SiteCollection }); }
                            }
                        }
                    }
                    catch (AveSecurityTrimingException e)
                    {
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = newLogin, Status = WrapperRestoreStatus.Failed, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserFailed, newLogin, e), Level = SPObjectLevel.SiteCollection }); }
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Type = SPObjectType.User, Url = ParentSite.SiteUrl, Title = newLogin, Status = WrapperRestoreStatus.Failed, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureUserFailed, newLogin, e), Level = SPObjectLevel.SiteCollection }); }
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(userInfo.Login, e));
                    }
                }

                return user;
            }
        }

        public IAveUser GetOrAddUser(string login)
        {
            AveUserInfo info = new AveUserInfo() { Login = login };
            bool needUpdate = false;
            bool isNewAdd = false;
            return GetOrAddUser(info, out needUpdate, out isNewAdd);
        }

        protected void RestoreUserSettings(IAveUser spUser, AveUserInfo userInfo, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreUserSettings"))
            {
                if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_UpdateUserSetting, spUser.LoginName), Status = WrapperRestoreStatus.None, Title = spUser.LoginName, Url = ParentSite.SiteUrl, Type = SPObjectType.User }); }

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

                        }
                        catch (Exception ex)
                        {
                            log.Warn("An error occurred while updating user regional settings. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                             mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, ex.ToString());
                        }
                    }
                    if (option.IsSiteLevel && option.UpdateAdminSetting && (userInfo.SiteAdmin != spUser.IsSiteAdmin))
                    {
                        spUser.IsSiteAdmin = userInfo.SiteAdmin;
                        changed = true;
                    }

                    if (changed)
                    {
                        spUser.Update();
                    }

                    if (option.NeedDeleteUser && userInfo.Deleted > 0 && userInfo.Deleted == userInfo.ID)
                    {
                        this.mAveParentSite.SPSite.RootWeb.SiteUsers.Remove(spUser.LoginName);
                        //this.ParentSite.MappingManager.SiteMappingManager.AddNeedDeletedUsersMapping(spUser.ID, DateTime.Now);
                    }
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_UpdateUserSettingSuccessful, spUser.LoginName), Status = WrapperRestoreStatus.Successful, Title = spUser.LoginName, Url = ParentSite.SiteUrl, Type = SPObjectType.User }); }
                }
                catch (AveSecurityTrimingException ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_UpdateUserSettingFailed, spUser.LoginName), Status = WrapperRestoreStatus.Failed, Title = spUser.LoginName, Url = ParentSite.SiteUrl, Type = SPObjectType.User }); }
                    log.Warn("An error occurred while updating user information. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                        mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, ex.ToString());
                    lock (reportPrivateLock)
                    {
                        report.AddDetail(new AveWrapperReportDto("UserSettings", userInfo.Title, AveReportObjectType.UserSettings, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreUserSetting, ex.Message));
                    }
                }
                catch (Exception e)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_UpdateUserSettingFailed, spUser.LoginName), Status = WrapperRestoreStatus.Failed, Title = spUser.LoginName, Url = ParentSite.SiteUrl, Type = SPObjectType.User }); }
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update user information. webUrl:{0}, loginName:{1}, userId:{2}\n error message:{3}", mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, e));
                    log.Warn("An error occurred while updating user information. WebUrl:{0}, LoginName:{1}, UserId:{2}, error:{3}",
                        mAveParentSite.SPSite.RootWeb.Url, spUser.LoginName, spUser.ID, e.ToString());
                }
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

        public string GetMappingUserLogin(string login, bool isDomainGroup, bool needMapping)
        {
            string logonName = mappingManager.GetMappingUserLogin(login, isDomainGroup, needMapping);
            var codeIndex = logonName.IndexOf("|", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(logonName) && codeIndex > 0)
            {
                if (mAveParentSite.SPSite != null && mAveParentSite.SPSite.IsClassicWindowsModeAuthentication)
                {
                    if (logonName.Equals("c:0!.s|windows", StringComparison.OrdinalIgnoreCase))
                    {
                        string mappingLoginName = GetBuiltInUserLoginName("S-1-5-11");
                        logonName = string.IsNullOrEmpty(mappingLoginName) ? @"NT AUTHORITY\authenticated users" : mappingLoginName;//claim 认证方式中的"c:0!.s|windows" user在windows认证方式中表示的是"NT AUTHORITY\authenticated users" 这个user
                    }
                    else
                    {
                        logonName = logonName.Substring(codeIndex + 1);
                    }
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

        public int RestoreGroup(AveGroupInfo groupInfo, MembersRestoreOption option)
        {
            //var profiler = new DefaultRestoreSiteProfiler();
            return RestoreGroup(groupInfo, option, null);
        }

        public int RestoreGroup(AveGroupInfo groupInfo, MembersRestoreOption option, ISPImportProfiler profiler)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Group"))
            {
                AddUserLoginAndIdMapping(groupInfo);
                if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Status = WrapperRestoreStatus.None, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_StartRestoreGroup, groupInfo.Title), Title = groupInfo.Title, Type = SPObjectType.Group, Url = ParentSite.SiteUrl }); }

                bool isNeedReport = true;
                AveStatus reportStatus = AveStatus.Successful;
                AveReportResource key = AveReportResource.Wrapper_Report_None;
                IAveGroup spGroup = null;
                try
                {
                    if (option.SkipWithoutPermissions == true && groupInfo.HasPermission.HasValue && !groupInfo.HasPermission.Value)
                    {
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Url = ParentSite.SiteUrl, Type = SPObjectType.Group, Title = groupInfo.Title, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_NoPermissionGroup, groupInfo.Title), Status = WrapperRestoreStatus.Skipped, Level = SPObjectLevel.SiteCollection }); }
                        reportStatus = AveStatus.Skipped;
                        key = AveReportResource.Wrapper_Report_SkipTheNoPermissionGroup;
                        return AveSPMemberInfo.FakeId;////没有权限，不还原
                    }
                    groupInfo.Title = mAveParentSite.GetNameByLanguageMapping(groupInfo.Title, AveLanguageMappingType.PermissionMapping);
                    AveSPMemberInfo memberInfo;
                    object memberObj = UserAndDomainMapping.GetUserMapping(groupInfo.ID);
                    if (memberObj != null)
                    {
                        memberInfo = memberObj as AveSPMemberInfo;
                        if (memberInfo != null)
                        {
                            isNeedReport = false;
                            // This group has already been restored before.
                            reportStatus = AveStatus.Skipped;
                            if (memberInfo.IsUser)
                            {
                                //mLog.Log(AveLogLevel.WARN, "The group id '{0}' is a user id. GroupTitle:{1}", groupInfo.ID, groupInfo.Title);
                                log.Warn("The group id '{0}' is a user id. GroupTitle:{1}", groupInfo.ID, groupInfo.Title);
                                key = AveReportResource.Wrapper_Report_TheGroupIsUser;
                                return AveSPMemberInfo.FAKE_GROUP.NewId;
                            }
                            return memberInfo.NewId;
                        }
                        //mMapping.Remove(groupInfo.ID);
                        UserAndDomainMapping.RemoveOneUserMapping(groupInfo.ID);
                    }
                    try
                    {
                        spGroup = mAveParentSite.SPSite.RootWeb.SiteGroups[groupInfo.Title];
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureGroupSuccessfully, groupInfo.Title), Status = WrapperRestoreStatus.Successful, Title = groupInfo.Title, Url = ParentSite.SiteUrl, Type = SPObjectType.Group }); }
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        reportStatus = AveStatus.Skipped;
                        key = AveReportResource.Wrapper_Report_NoPermissionToRestoreGroup;
                        //report.AddDetail(new AveWrapperReportDto("Group", groupInfo.Title, AveReportObjectType.Group, AveStatus.Skipped, "You don't have permission to add group. " + ex.Message));
                        log.Log(AveLogLevel.WARN, "An error occurred while add group.Group Title:{0}, error:{1}", groupInfo.Title, ex.ToString());
                    }
                    catch (Exception e)
                    {
                        //If group can not be found, SPException will throw, do not need to log 
                        if (!string.Equals(e.GetType().FullName, AveConstants.SP_EXCEPTION_STRING, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotFindGroup, groupInfo.Title, e);
                        }
                    }
                    bool isNewAdd = false;
                    if (spGroup == null)
                    {
                        try
                        {
                            mAveParentSite.SPSite.RootWeb.SiteGroups.Add(groupInfo.Title, mAveParentSite.SPSite.RootWeb.CurrentUser, null, groupInfo.Description);
                            spGroup = mAveParentSite.SPSite.RootWeb.SiteGroups[groupInfo.Title];
                            isNewAdd = true;
                            if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureGroupSuccessfully, groupInfo.Title), Status = WrapperRestoreStatus.Successful, Title = groupInfo.Title, Url = ParentSite.SiteUrl, Type = SPObjectType.Group }); }
                        }
                        catch (Exception e)
                        {
                            if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_EnsureGroupFailed, groupInfo.Title, e), Status = WrapperRestoreStatus.Failed, Title = groupInfo.Title, Url = ParentSite.SiteUrl, Type = SPObjectType.Group }); }
                            reportStatus = AveStatus.Failed;
                            log.Log(AveLogLevel.WARN, "An error occurred while add group.Group Title:{0}. Error:{1}", groupInfo.Title, e.ToString());
                        }
                    }
                    if (spGroup != null)
                    {
                        memberInfo = new AveSPMemberInfo(spGroup.Name, spGroup.ID, false);
                        memberInfo.SourceInfo = groupInfo;
                        UserAndDomainMapping.AddUserMapping(groupInfo.ID, memberInfo);
                        if (option.OverWrite || isNewAdd)
                        {
                            if (groupInfo.OwnerInfo != null)
                            {
                                RestoreUser(groupInfo.OwnerInfo, defaultOption, profiler);
                            }
                            IAveMember owner = FindMember(groupInfo.Owner, false);
                            if (owner == null)
                            {
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
                            RestoreGroupMembersAndSettings(spGroup, groupInfo, profiler);
                        }
                        //reportStatus = AveStatus.Successful;
                        return memberInfo.NewId;
                    }
                    else
                    {
                        //memberInfo = AveSPMemberInfo.FAKE_GROUP;
                        memberInfo = new AveSPMemberInfo(string.Empty, AveSPMemberInfo.FakeId, false);
                    }
                    memberInfo.SourceInfo = groupInfo;
                    UserAndDomainMapping.AddUserMapping(groupInfo.ID, memberInfo);
                    if (reportStatus == AveStatus.Skipped)
                    {
                        key = AveReportResource.Wrapper_Report_GroupRestoreSkipError;
                    }
                    if (reportStatus == AveStatus.Failed)
                    {
                        key = AveReportResource.Wrapper_Report_GroupRestoreFailedError;
                    }
                    //reportStatus = AveStatus.Successful;
                    return memberInfo.NewId;
                }
                finally
                {
                    if (isNeedReport)
                    {
                        string reportGroupName = spGroup != null && !String.IsNullOrEmpty(spGroup.Name) ? spGroup.Name : groupInfo.Title;
                        log.Debug("Restore group name: {0}, the restore status is {1}.", reportGroupName, reportStatus);
                        lock (reportPrivateLock)
                        {
                            report.AddDetail(new AveWrapperReportDto(reportGroupName, mAveParentSite.SPSite.RootWeb.Title, AveReportObjectType.Group, reportStatus, key));
                        }
                    }
                }
            }

        }

        protected IAveGroup FindAndUpdateGroup(AveGroupInfo groupInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.FindAndUpdateGroup"))
            {
                IAveGroup spGroup = null;
                try
                {
                    spGroup = mAveParentSite.SPSite.RootWeb.SiteGroups[groupInfo.Title];
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.DEBUG, string.Format("Get group errro. group title:{0}\n errro message:{1}", groupInfo.Title, e));
                    log.Debug("Get group error. Title:{0}, error:{1}", groupInfo.Title, e.ToString());
                }
                if (spGroup != null)
                {
                    //update group table
                    //TODO:(API)update group
                    //mAveSPSite.SqlConn.ClearParameters();
                    //mAveSPSite.SqlConn.AddParameter("@SiteId", mAveSPSite.SPSite.ID);
                    //mAveSPSite.SqlConn.AddParameter("@ID", spGroup.Id);
                    //mAveSPSite.SqlConn.UpdateTableRow(dic, "Groups", ",ID,Title,Owner,OwnerIsUser,", " WHERE SiteID=@SiteId and ID=@ID");
                }
                return spGroup;
            }
        }

        protected void RestoreGroupMembersAndSettings(IAveGroup group, AveGroupInfo groupInfo, ISPImportProfiler profiler)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreGroupMembersAndSettings"))
            {
                try
                {
                    bool changed = false;

                    if (groupInfo.Memberships != null)
                    {
                        foreach (int userId in groupInfo.Memberships)
                        {
                            try
                            {
                                IAveUser user = this.FindMember(userId, false) as IAveUser;
                                if (user == null)
                                {
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
                                        int newUserId = RestoreUser(needRestoreUser, defaultOption);
                                        user = this.FindMember(needRestoreUser.ID, false) as IAveUser;
                                    }
                                    if (user == null)
                                    {
                                        continue;
                                    }
                                }
                                if (group.Users.GetByLoginName(user.LoginName) == null)
                                {
                                    group.AddUser(user);
                                    changed = true;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while restore user to the group. UserID: {0}, GroupName: {1}, Error:{2}", userId, group.Name, e.ToString());
                            }
                        }
                    }
                    Type groupType = group.GetType();
                    if (String.IsNullOrEmpty(groupInfo.DLAlias))
                    {
                        groupInfo.DLAlias = null;
                    }
                    if (string.Compare(group.DistributionGroupAlias, groupInfo.DLAlias, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        //group.DistributionGroupAlias = groupInfo.DLAlias;
                        changed = true;
                        //AveAssemblyUtility.SetPropertyValue(group, "DistributionGroupAlias", groupInfo.DLAlias);
                        if (!string.IsNullOrEmpty(groupInfo.DLAlias))
                        {
                            log.Warn(String.Format("Group DLAlias information. DLAlias:{0}", groupInfo.DLAlias));
                        }

                        if (string.IsNullOrEmpty(group.DistributionGroupAlias))
                        {

                            group.CreateDistributionGroup(groupInfo.DLAlias);
                        }
                        else if (string.IsNullOrEmpty(groupInfo.DLAlias))
                        {
                            group.DeleteDistributionGroup();
                        }
                        else
                        {
                            ///Rename DistributeGroup 
                            ///Do not call to RenameDistributionGroup API to complete this action
                            group.DeleteDistributionGroup();
                            group.CreateDistributionGroup(groupInfo.DLAlias);
                        }
                    }
                    if (string.Compare(group.DistributionGroupErrorMessage, groupInfo.DLErrorMessage, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        group.DistributionGroupErrorMessage = groupInfo.DLErrorMessage;
                        changed = true;
                        //AveAssemblyUtility.SetPropertyValue(group, "DistributionGroupErrorMessage", groupInfo.DLErrorMessage);
                    }
                    if (string.Compare(group.Description, groupInfo.Description, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        group.Description = groupInfo.Description;
                        changed = true;
                    }
                    var groupItem = group.ParentWeb.SiteUserInfoList.GetItemById(group.ID);
                    if (groupItem != null && groupItem.Fields.ContainsField("Notes"))
                    {
                        bool update = false;
                        var aboutMe = (string)groupItem["Notes"];
                        string sourceAboutMe = groupInfo.AboutMe;
                        if (!string.IsNullOrEmpty(sourceAboutMe))
                        {
                            bool needReplaceLast = false;
                            sourceAboutMe = AveReplaceProcessor.ReplaceXmlLinks(sourceAboutMe, mAveParentSite.MappingManager, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, null, ref needReplaceLast);
                            if (string.Compare(aboutMe, sourceAboutMe, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                update = true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(aboutMe))
                            {
                                update = true;
                            }
                        }

                        if (update)
                        {
                            groupItem["Notes"] = sourceAboutMe;
                            groupItem.Update();
                        }
                    }
                    if (string.Compare(group.RequestToJoinLeaveEmailSetting, groupInfo.RequestEmail, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        group.RequestToJoinLeaveEmailSetting = groupInfo.RequestEmail;
                        changed = true;
                    }
                    if (groupInfo.Flags != null && groupInfo.Flags.IsAvailable)  //local backup
                    {
                        if (group.AllowRequestToJoinLeave != ((groupInfo.Flags.Value & 4) != 0))
                        {
                            group.AllowRequestToJoinLeave = !group.AllowRequestToJoinLeave;
                            changed = true;
                        }
                        if (group.AutoAcceptRequestToJoinLeave != ((groupInfo.Flags.Value & 8) != 0))
                        {
                            group.AutoAcceptRequestToJoinLeave = !group.AutoAcceptRequestToJoinLeave;
                            changed = true;
                        }
                        if (group.AllowMembersEditMembership != ((groupInfo.Flags.Value & 2) != 0))
                        {
                            group.AllowMembersEditMembership = !group.AllowMembersEditMembership;
                            changed = true;
                        }
                        if (group.OnlyAllowMembersViewMembership != ((groupInfo.Flags.Value & 1) != 0))
                        {
                            group.OnlyAllowMembersViewMembership = !group.OnlyAllowMembersViewMembership;
                            changed = true;
                        }
                    }
                    else //office365 backup
                    {
                        if (group.AllowRequestToJoinLeave != groupInfo.AllowRequestToJoinLeave)
                        {
                            group.AllowRequestToJoinLeave = groupInfo.AllowRequestToJoinLeave;
                            changed = true;
                        }
                        if (group.AutoAcceptRequestToJoinLeave != groupInfo.AutoAcceptRequestToJoinLeave)
                        {
                            group.AutoAcceptRequestToJoinLeave = groupInfo.AutoAcceptRequestToJoinLeave;
                            changed = true;
                        }
                        if (group.AllowMembersEditMembership != groupInfo.AllowMembersEditMembership)
                        {
                            group.AllowMembersEditMembership = groupInfo.AllowMembersEditMembership;
                            changed = true;
                        }
                        if (group.OnlyAllowMembersViewMembership != groupInfo.OnlyAllowMembersViewMembership)
                        {
                            group.OnlyAllowMembersViewMembership = groupInfo.OnlyAllowMembersViewMembership;
                            changed = true;
                        }
                    }

                    //group.AllowRequestToJoinLeave = (groupInfo.Flags & 4) != 0;
                    //group.AutoAcceptRequestToJoinLeave = (groupInfo.Flags & 8) != 0;
                    //group.AllowMembersEditMembership = (groupInfo.Flags & 2) != 0;
                    //group.OnlyAllowMembersViewMembership = (groupInfo.Flags & 1) != 0;
                    if (changed)//如果没有变化最好不要update，因为SharePoint每次update都会调用request走一遍
                    {
                        group.Update();
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_RestoreGroupSettingsSuccessfully, group.Name), Status = WrapperRestoreStatus.Successful, Title = group.Name, Type = SPObjectType.GroupSettings, Url = ParentSite.SiteUrl }); }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_RestoreGroupSettingsFailed, group.Name, ex), Status = WrapperRestoreStatus.Failed, Title = group.Name, Type = SPObjectType.GroupSettings, Url = ParentSite.SiteUrl }); }
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update group's members. SiteId:{0}, group title:{1}\n error message:{2}", mAveParentSite.SPSite.ID, groupInfo.Title, ex));
                    log.Warn("An error occurred while updating group's members. SiteId:{0}, Title:{1}, error:{2}", mAveParentSite.SPSite.ID, groupInfo.Title, ex.ToString());
                }
            }
        }

        public virtual void RestoreMembers(AveSecurityInfo securityInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreMembers"))
            {
                if (securityInfo.Users != null)
                {
                    foreach (AveUserInfo userInfo in securityInfo.Users)
                    {
                        try
                        {
                            RestoreUser(userInfo, defaultOption);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore user. user title:{0}, user id:{1}\n error message:{2}", userInfo.Title, userInfo.ID, e));
                        }
                    }
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
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore group. group title:{0}, group id:{1}\n error message:{2}", groupInfo.Title, groupInfo.ID, e));
                        }
                    }
                }
            }
        }

        public void RestoreGroupOwner()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreGroupOwner"))
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
                        //qlluo: Post action do not support report, remove it.
                        //report.AddDetail(new AveWrapperReportDto("GroupOwner", "GroupOwner", AveReportObjectType.GroupOwner, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreGroupOwner + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while RestoreGroupOwner. error:{0}", e.ToString());
                    }
                }
            }
        }

        public void LoadUsers(List<AveUserInfo> userInfos)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.LoadUsers"))
            {
                if (userInfos != null)
                {
                    foreach (AveUserInfo userInfo in userInfos)
                    {
                        //mMapping[userInfo.ID] = userInfo;
                        UserAndDomainMapping.AddUserMapping(userInfo.ID, userInfo);
                    }
                    AddUserLoginAndIdMappings(userInfos);
                }
            }
        }

        protected void AddUserLoginAndIdMappings(IList<AveUserInfo> allUsers)
        {
            foreach (var userInfo in allUsers.Where(user => UserAndDomainMapping.GetUserMapping(user.ID) != null))
            {
                AddUserLoginAndIdMapping(userInfo);
            }
        }

        private void AddUserLoginAndIdMapping(AveUserInfo userInfo)
        {
            if (userInfo != null && !string.IsNullOrEmpty(userInfo.Login))
            {
                lock (userloginandIdMapping)
                {
                    if (!UserLoginAndIdMapping.ContainsKey(userInfo.Login))
                    {
                        UserLoginAndIdMapping.Add(userInfo.Login, userInfo.ID);
                    }
                    else
                    {
                        if (userInfo.Deleted == 0)
                        {
                            UserLoginAndIdMapping[userInfo.Login] = userInfo.ID;
                        }
                    }
                }
            }
        }

        private void AddUserLoginAndIdMapping(AveGroupInfo groupInfo)
        {
            if (groupInfo != null && !string.IsNullOrEmpty(groupInfo.Title))
            {
                lock (userloginandIdMapping)
                {
                    if (!UserLoginAndIdMapping.ContainsKey(groupInfo.Title))
                    {
                        UserLoginAndIdMapping.Add(groupInfo.Title, groupInfo.ID);
                    }
                }
            }
        }

        public void LoadGroups(List<AveGroupInfo> groups)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.LoadGroups"))
            {
                if (groups != null)
                {
                    foreach (AveGroupInfo groupInfo in groups)
                    {
                        //mMapping[groupInfo.ID] = groupInfo;
                        UserAndDomainMapping.AddUserMapping(groupInfo.ID, groupInfo);
                        AddUserLoginAndIdMapping(groupInfo);
                    }
                }
            }
        }

        public void LoadMembers(AveSecurityInfo securityInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.LoadMembers"))
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="createIfNotExist">是否把源端备份数据还原，
        /// FALSE：走find逻辑，到目的端查找同名的Principal
        /// TRUE：把源端备份数据还原到目的端</param>
        /// <param name="useDefaultUser"></param>
        /// <returns></returns>
        public IAvePrincipal FindMember(int id, bool createIfNotExist)
        {
            return FindMember(id, createIfNotExist, false);
        }


        public IAvePrincipal FindMember(int id, bool createIfNotExist, bool useDefaultUser)
        {
            // find the AveSPMemberInfo from the mMapping
            // if mNewId is not zero, return mNewId
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.FindMember"))
            {
                try
                {
                    object member = UserAndDomainMapping.GetUserMapping(id);
                    if (createIfNotExist)
                    {
                        return GetOrAddPrincipal(member, useDefaultUser);
                    }
                    else
                    {
                        return FindPrincipal(member);
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while find member. member id:{0}, createIfExist:{1}\n error message:{2}", id, createIfNotExist, e));
                    log.Warn("An error occurred while finding member. MemberId:{0}, CreateIfExist:{1}, error:{2}", id, createIfNotExist, e.ToString());
                }
                return null;
            }
        }

        /// <summary>
        /// 如果member 为备份数据，则直接还原，如果为还原数据则返回对应的value
        /// </summary>
        /// <param name="member"></param>
        /// <param name="useDefaultUser"></param>
        /// <returns></returns>
        public IAvePrincipal GetOrAddPrincipal(Object member, bool useDefaultUser)
        {
            // find from mapping and create
            bool isUser = true;
            IAvePrincipal pricipal = null;
            int newId = AveSPMemberInfo.FakeId;

            if (member == null)
            {
                return null;
            }
            if (member is AveUserInfo)
            {
                newId = RestoreUser((AveUserInfo)member, defaultOption);
                isUser = true;
            }
            else if (member is AveGroupInfo)
            {
                newId = RestoreGroup((AveGroupInfo)member, defaultOption);
                isUser = false;
            }
            else if (member is AveSPMemberInfo)
            {
                var memberInfo = (AveSPMemberInfo)member;
                newId = memberInfo.NewId;
                isUser = memberInfo.IsUser;
            }

            if (newId != AveSPMemberInfo.FakeId && pricipal == null)
            {
                pricipal = GetPrincipalByID(newId, isUser);
            }
            if (newId == AveSPMemberInfo.FakeId && useDefaultUser)
            {
                pricipal = UserDefaultUser();
                if (pricipal == null)
                {
                    pricipal = GetPrincipalByID(mAveParentSite.CURRENT_USER_ID, isUser);
                }
            }
            return pricipal;
        }

        private IAvePrincipal UserDefaultUser()
        {
            IAvePrincipal pricipal = null;
            if (!string.IsNullOrEmpty(mAveParentSite.DefaultUser))
            {
                try
                {
                    pricipal = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(mAveParentSite.DefaultUser);
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(mAveParentSite.DefaultUser, e));
                }
            }
            return pricipal == null ? GetPrincipalByID(mAveParentSite.CURRENT_USER_ID, true) : pricipal;
        }

        /// <summary>
        /// Find member from destianton.
        /// </summary>
        /// <param name="member"></param>
        /// <returns> if not exist,retrun null.</returns>
        private IAvePrincipal FindPrincipal(Object member)
        {
            if (member != null)
            {
                var userInfo = member as AveUserInfo;
                if (userInfo != null)
                {
                    ////根据user mapping 找到目的端对应的userLoginName
                    var userLoginName = GetMappingUserLogin(userInfo.Login, true);
                    return mAveParentSite.SPSite.RootWeb.SiteUsers.GetByLoginName(userLoginName);
                }

                var groupInfo = member as AveGroupInfo;
                if (groupInfo != null)
                {
                    //根据language mapping 找到目的端对应的group title
                    var groupTitle = mAveParentSite.GetNameByLanguageMapping(groupInfo.Title, AveLanguageMappingType.PermissionMapping);
                    return mAveParentSite.SPSite.RootWeb.SiteGroups[groupTitle];
                }

                var memberInfo = member as AveSPMemberInfo;
                if (memberInfo != null)
                {
                    return GetPrincipalByID(memberInfo.NewId, memberInfo.IsUser);
                }
            }

            return null;
        }

        private IAvePrincipal GetPrincipalByID(int principalId, bool isUser)
        {
            if (isUser)
            {
                return mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(principalId);
            }
            return mAveParentSite.SPSite.RootWeb.SiteGroups.GetByID(principalId);
        }


        public int FindMemberId(int oldUserId)
        {
            return FindMemberId(oldUserId, true, true);
        }

        public int FindMemberId(int oldUserId, bool createIfNotExist)
        {
            return FindMemberId(oldUserId, createIfNotExist, false);
        }

        public int FindMemberId(int oldUserId, bool createIfNotExist, bool changeIfNotFound)
        {
            return FindMemberId(oldUserId, createIfNotExist, changeIfNotFound, true);
        }

        public int FindMemberId(int oldUserId, bool createIfNotExist, bool changeIfNotFound, bool usePlaceHolderUser)
        {
            //system account
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.FindMemberId"))
            {
                if (oldUserId == AveConstants.SYSTEM_ACCOUNT_ID)
                {
                    return oldUserId;
                }
                Tuple<int, string> userIdAndLoginName = FindMemberIdAndLoginNameFromUserMapping(oldUserId, !usePlaceHolderUser);  //FindMember 也会从同样的mapping 里找，但是如果传值createIfNotExist是false，会走一遍api 取对象，浪费效率，所以在这里单独加一个从cache 找的逻辑。 暂时不想为了代码结构来修改逻辑。
                if (userIdAndLoginName != null && userIdAndLoginName.Item1 != AveSPMemberInfo.FakeId)
                {
                    return userIdAndLoginName.Item1;
                }

                IAvePrincipal principal = FindMember(oldUserId, createIfNotExist, true);

                if (principal == null)
                {
                    if (defaultOption.RestoreInactiveUser)//For HSM first restore user.   在FindMmber 里会加入一个int.max 递减的id 值的对象，在这里只需取出这个对象的id 值。 这里是一个隐藏的逻辑， 所以看上去和上面code 一致。
                    {
                        userIdAndLoginName = FindMemberIdAndLoginNameFromUserMapping(oldUserId, !usePlaceHolderUser);
                        if (userIdAndLoginName != null && userIdAndLoginName.Item1 != AveSPMemberInfo.FakeId)
                        {
                            return userIdAndLoginName.Item1;
                        }
                    }

                    if (changeIfNotFound)
                    {
                        return mAveParentSite.CURRENT_USER_ID;
                    }
                    return AveSPMemberInfo.FakeId;
                }

                return principal.ID;
            }
        }

        internal Tuple<int, string> FindMemberIdAndLoginNameFromUserMapping(int oldUserId)
        {
            return FindMemberIdAndLoginNameFromUserMapping(oldUserId, false);
        }

        internal Tuple<int, string> FindMemberIdAndLoginNameFromUserMapping(int oldUserId, bool changePlaceHolder)
        {
            Tuple<int, string> userIdAndLoginName = null;
            object member = UserAndDomainMapping.GetUserMapping(oldUserId);
            var memberInfo = member as AveSPMemberInfo;
            if (memberInfo != null)
            {
                if (memberInfo.IsHSMInactiveUser && changePlaceHolder)
                {
                    userIdAndLoginName = new Tuple<int, string>(AveConstants.SYSTEM_ACCOUNT_ID, "System Account");
                }
                else
                {
                    userIdAndLoginName = new Tuple<int, string>(memberInfo.NewId, memberInfo.AccountName);
                }
            }
            return userIdAndLoginName;
        }

        //Doc2466 for workflow update user id in workflow instancedata.
        public int CreateAndFindMemberId(string oldLoginName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.CreateAndFindMemberId"))
            {
                int oldId = 0;
                int newId = 0;
                foreach (KeyValuePair<int, object> entry in UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value))
                {
                    if (entry.Value is AveSPMemberInfo)
                    {
                        AveSPMemberInfo memberInfo = (AveSPMemberInfo)entry.Value;
                        if (memberInfo.IsUser && string.Compare(memberInfo.AccountName, oldLoginName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            newId = memberInfo.NewId;
                            oldId = entry.Key;
                            break;
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
        }

        public void RestoreGroups(List<AveGroupInfo> groupsInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPMembers.RestoreGroups"))
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
        }

        public void SetDefaultOption(MembersRestoreOption option)
        {
            this.defaultOption = option.Clone();
        }

        public string EnsureUserWithCache(string loginName)
        {
            if (!userloginCacheMapping.ContainsKey(loginName))
            {
                IAveUser user = null;
                try
                {
                    var newLogin = GetMappingUserLogin(loginName, true);
                    user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(newLogin,true);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByUrlError, e.ToString());
                }
                if (user != null)
                {
                    userloginCacheMapping[loginName] = user.LoginName;
                }
                else
                {
                    userloginCacheMapping[loginName] = loginName;
                }
            }
            return userloginCacheMapping[loginName];
        }

    }

    public class AveSPMemberInfo
    {
        internal static readonly int FakeId = -1;
        public static readonly AveSPMemberInfo FAKE_USER = new AveSPMemberInfo(string.Empty, FakeId, true);
        public static readonly AveSPMemberInfo FAKE_GROUP = new AveSPMemberInfo(string.Empty, FakeId, false);
        public int NewId; // new id in the sharepoint server
        public string AccountName; // the security account name, use login name now.
        public bool IsUser; // ture is user, false is group
        public object SourceInfo;
        public bool IsHSMInactiveUser;

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

        public AveSPMemberInfo(string loginName, int newid, bool _user, bool isHSMInactiveUser)
            : this(loginName, newid, _user)
        {
            IsHSMInactiveUser = isHSMInactiveUser;
        }
    }
    
}
