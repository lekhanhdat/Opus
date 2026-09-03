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




using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.ObjectModel.Common;
using AvePoint.ObjectModel.WebService;
using AvePoint.RA.Browser.Handler;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Graph;
using ExchangeUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace AvePoint.SharePointBrowser.Office365
{
    public class Office365BrowserMessageHandler : IBrowserHandler
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<string> GroupNames = new List<string>();
        private BrowserMessage InitMessages(BrowserMessage message)
        {
            BrowserMessage returnInfo = new BrowserMessage();
            returnInfo.BrowserContracts = new List<BrowserContractBase>();
            returnInfo.BrowserContract = new Office365MessageContract();
            if (message.BrowserContracts == null)
            {
                message.BrowserContracts = new List<BrowserContractBase>();
                message.BrowserContracts.Add(message.BrowserContract);
            }
            return returnInfo;
        }

        public BrowserMessage HandleMessage(ServiceDto agentInfo, BrowserMessage message, AveObjectModelFactory objectModel)
        {
            switch (message.MsgType)
            {
                case GCommon.Contract.Common.MessageType.CheckUserHasPermission:
                    return CheckUserPermission(message, objectModel);

                default:
                    BrowserMessage returnInfo = InitMessages(message);
                    var result = returnInfo.BrowserContract as Office365MessageContract;
                    result.Result.ErrorInfo = ErrorInfo.Unknown;
                    result.Result.Status = false;
                    return returnInfo;
            }
        }

        private BrowserMessage CheckUserPermission(BrowserMessage message, AveObjectModelFactory objectModel)
        {
            BrowserMessage returnInfo = InitMessages(message);
            var results = returnInfo.BrowserContracts;
            var contracts = message.MessageContract;
            if (message.MsgType == GCommon.Contract.Common.MessageType.CheckUserHasPermission)
            {
                CheckOffice365UserPermission(contracts, results, objectModel);
            }
            if (returnInfo.BrowserContracts != null && returnInfo.BrowserContracts.Count > 0)
            {
                if (returnInfo.BrowserContracts[0] is Office365MessageContract)
                {
                    returnInfo.BrowserContract = returnInfo.BrowserContracts[0] as Office365MessageContract;
                    returnInfo.MessageContract= returnInfo.BrowserContracts[0] as Office365MessageContract;
                }
            }
            return returnInfo;
        }

        #region Office 365 check user permission

        private void CheckOffice365UserPermission(Office365MessageContract contracts, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Office365MessageContract contract = contracts;
            contract.NeedCheckedUserUPN = contract.NeedCheckedUserMail;
            string realEmail = GraphHelper.GetEmailStringByUPN(contract.NeedCheckedUserMail, objectModel.AccountInfo);
            sw.Stop();
            mLog.Info($"linkRestoreReport Check office365 user permission the UPN is {contract.NeedCheckedUserMail},cost time:{sw.ElapsedMilliseconds}");
            switch (contract.CheckPermissionAction)
            {
                case CheckPermissionAction.ArchiverStubFile:
                    CheckLeaveStubOffice365UserPermission(contract, results, objectModel);
                    break;
                case CheckPermissionAction.SiteOwner:
                    contract.NeedCheckedUserMail = realEmail;
                    CheckUserSiteOwnerPermissionForSharePointSite(contract, results, objectModel);
                    break;
                case CheckPermissionAction.SiteOwnerOrSiteMember:
                    contract.NeedCheckedUserMail = realEmail;
                    CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite(contract, results, objectModel);
                    break;
                case CheckPermissionAction.SiteOwnerOrSpecialGroup:
                    contract.NeedCheckedUserMail = realEmail;
                    CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite(contract, results, objectModel);
                    break;
                case CheckPermissionAction.SiteOwnerOrSiteMemberGroupOrSiteVisitor:
                    contract.NeedCheckedUserMail = realEmail;
                    CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite(contract, results, objectModel);
                    break;
                case CheckPermissionAction.GroupOwner:
                    CheckGroupOwnerOffice365UserPermission(contract, results, objectModel);
                    break;
                case CheckPermissionAction.GroupOwnerOrMember:
                    CheckGroupOwnerOrMemberOffice365UserPermission(contract, results, objectModel);
                    break;
                default:
                    {
                        mLog.Warn($"Can not find action of {contract.CheckPermissionAction}");
                        break;
                    }
            }
        }

        private void CheckLeaveStubOffice365UserPermission(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                //special unicode
                string fileUrl = System.Web.HttpUtility.UrlDecode(contract.FileFullUrl);

                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                using (IAveSite site = objectModel.CreateSite(contract.SiteCollectionUrl))
                {
                    sw1.Stop();
                    mLog.Info($"linkRestoreReport CheckLeaveStubOffice365UserPermission CreateSite cost time:{sw1.ElapsedMilliseconds}");
                    Stopwatch sw8 = new Stopwatch();
                    sw8.Start();
                    Stopwatch sw9 = new Stopwatch();
                    sw9.Start();
                    var mResult = new Office365MessageContract() { Result = new Result() { Status = true } };
                    AveSiteServiceHelper helper = new AveSiteServiceHelper();
                    var tempurl = fileUrl.Substring(0, fileUrl.LastIndexOf('/')+1);
                    string webUrl = helper.TryToRectifySiteUrl(tempurl, site);
                    sw9.Stop();
                    mLog.Info($"linkRestoreReport helper.TryToRectifySiteUrl cost time:{sw9.ElapsedMilliseconds}.webUrl:{webUrl}.SiteURL:{contract.SiteCollectionUrl}.");
                    if (site.RootWeb.Template == "GROUP#0")
                    {
                        if (!String.IsNullOrEmpty(contract.NeedCheckedGroupId))
                        {
                            RMGroup checkGroupType = null;
                            try
                            {
                                var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                if (appProfile == null)
                                {
                                    Logger.Warn($"CheckLeaveStubOffice365UserPermission can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                                    appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                }
                                var groupSite = new RMGraphGroupManager(appProfile);
                                checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                            }
                            catch (System.Exception ex)
                            {
                                checkGroupType = null;
                                Logger.Error($"An error occur when GetGroup.Message:{ex.ToString()}.");
                            }
                            if (checkGroupType != null)
                            {
                                if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                                {
                                    mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                                }
                                else
                                {
                                    foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                                    {
                                        bool isTeamGroupSites = rpo.ToString().Contains("Team");
                                        if (isTeamGroupSites)
                                        {
                                            mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Stopwatch sw10 = new Stopwatch();
                    sw10.Start();
                    string webServerRelativeUrl = GetWebServerServerRelativeUrl(webUrl, site);
                    if (contract.UseSiteMapping)
                    {
                        mLog.Info($"CheckLeaveStubOffice365UserPermission UseSiteMapping is true.");
                        webServerRelativeUrl = string.Empty;
                    }
                    sw10.Stop();
                    mLog.Info($"linkRestoreReport GetWebServerServerRelativeUrl cost time:{sw10.ElapsedMilliseconds}");
                    sw8.Stop();
                    mLog.Info($"linkRestoreReport check stub permission after create site and before open web cost:{sw8.ElapsedMilliseconds}");
                    Stopwatch sw2 = new Stopwatch();
                    sw2.Start();
                    using (IAveWeb web = site.OpenWeb(webServerRelativeUrl))
                    {
                        sw2.Stop();
                        mLog.Info($"linkRestoreReport CheckLeaveStubOffice365UserPermission open web cost time:{sw2.ElapsedMilliseconds}.webServerRelativeUrl:{webServerRelativeUrl}.");
                        if (!web.Exists)
                        {
                            mLog.Warn("Can not find web {0}.", webServerRelativeUrl);
                        }
                        List<string> stubFiles = null;
                        if (!string.IsNullOrEmpty(contract.StubType))
                        {
                            switch (contract.StubType)
                            {
                                case "Aspx":
                                    stubFiles = new List<string> { ".aspx" };
                                    break;
                                case "Html":
                                    stubFiles = new List<string> { ".html" };
                                    break;
                                case "Txt":
                                    stubFiles = new List<string> { ".txt" };
                                    break;
                                case "Link":
                                    stubFiles = new List<string> { ".url" };
                                    break;
                                case "None":
                                    stubFiles = new List<string> { "None" };
                                    break;
                                default:
                                    stubFiles = new List<string> { ".aspx", ".html", ".txt", ".url" };
                                    break;
                            }
                        }
                        else
                        {
                            stubFiles = new List<string> { ".aspx", ".html", ".txt", ".url" };
                        }
                        bool mGotStubFile = false;
                        string loginName = string.Empty;
                        IAveUser user = null;
                        try
                        {
                            //user = site.RootWeb.AllUsers.GetByEmail(contract.NeedCheckedUserMail);
                            Stopwatch sw3 = new Stopwatch();
                            sw3.Start();
                            user = site.RootWeb.AllUsers.GetByUPNName(contract.NeedCheckedUserMail);
                            sw3.Stop();
                            mLog.Info($"linkRestoreReport Success get user by email,cost time:{sw3.ElapsedMilliseconds}.NeedCheckedUserMail:{contract.NeedCheckedUserMail}.Email:{user.Email}.NoPrefixLoginName:{user.NoPrefixLoginName}.NoPrefixLoginNameForArchiver:{user.NoPrefixLoginNameForArchiver}.LoginName{user.LoginName}.");
                            if (user == null)
                            {
                                Stopwatch sw4 = new Stopwatch();
                                sw4.Start();
                                mLog.Info("Get user by UPN is null, start get user by email.");
                                user = site.RootWeb.AllUsers.GetByEmail(contract.NeedCheckedUserMail);
                                sw4.Stop();
                                mLog.Info($"linkRestoreReport Success get user by email cost time:{sw4.ElapsedMilliseconds}.NeedCheckedUserMail:{contract.NeedCheckedUserMail}.Email:{user.Email}.NoPrefixLoginName:{user.NoPrefixLoginName}.LoginName{user.LoginName}.");
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("Can not find user in the web." + e.ToString());
                        }
                        if (user == null)
                        {
                            mLog.Warn(string.Format("Can not find user in the web. file Url is : {0}.", fileUrl));
                            loginName = "i:0#.f|membership|" + contract.NeedCheckedUserMail;
                        }
                        else
                        {
                            loginName = user.LoginName;
                        }

                        foreach (var stub in stubFiles)
                        {
                            var stubUrl = string.Empty;
                            if (stub == "None")
                            {
                                stubUrl = contract.FileFullUrl;
                            }
                            else
                            {
                                stubUrl = contract.FileFullUrl + stub;
                            }
                            string serverRelativeUrl = AveUrlUtility.GetServerRelativeUrl(stubUrl);
                            Stopwatch sw5 = new Stopwatch();
                            sw5.Start();
                            var file = web.GetFile(serverRelativeUrl);
                            sw5.Stop();
                            mLog.Info($"linkRestoreReport get exist stub file cost:{sw5.ElapsedMilliseconds}.FileServerRelativeUrl:{serverRelativeUrl}.");
                            if (file.Exists)
                            {
                                mGotStubFile = true;
                                if (file.Item == null)
                                {
                                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                    mLog.Warn(string.Format("Can not find Office365 file item. file Url is : {0}.", fileUrl));
                                }
                                else
                                {
                                    Stopwatch sw6 = new Stopwatch();
                                    sw6.Start();
                                    var per = file.Item.GetUserEffectivePermissions(loginName);
                                    sw6.Stop();
                                    //var sitePer = site.RootWeb.GetUserEffectivePermissions(user.LoginName);
                                    //if (sitePer.HasFlag(AveBasePermissions.FullMask))
                                    //{
                                    //    results.Add(new Office365MessageContract() { Result = new Result() { Status = true } });
                                    //}
                                    mLog.Info($"linkRestoreReport Get BasePermissions cost time:{sw6.ElapsedMilliseconds} {per.ToString()}.loginName:{loginName}.stubType:{stub}.");
                                    if (per.HasFlag(AveBasePermissions.OpenItems | AveBasePermissions.ViewPages | AveBasePermissions.ViewListItems | AveBasePermissions.ViewPages))
                                    {
                                        results.Add(mResult);
                                    }
                                    else
                                    {
                                        if (stub == "None")
                                        {
                                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.PermissionError } });
                                        }
                                        else
                                        {
                                            mLog.Warn(string.Format("The user don't have open file permission. file Url is : {0}.", fileUrl));
                                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.InsufficientPrivileges } });
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        if (!mGotStubFile)
                        {
                            if (contract.StubType == "None")
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.OopStubNotFound } });
                                return;
                            }
                            Stopwatch sw7 = new Stopwatch();
                            sw7.Start();
                            mLog.Warn(string.Format("Can not find file. file Url is : {0}.", fileUrl));
                            var per = web.GetUserEffectivePermissions(loginName);
                            sw7.Stop();
                            mLog.Info($"linkRestoreReport not find stub file,Get BasePermissions cost time:{sw7.ElapsedMilliseconds}  {per.ToString()}.loginName:{loginName}.");
                            if (per.HasFlag(AveBasePermissions.OpenItems | AveBasePermissions.ViewPages | AveBasePermissions.ViewListItems | AveBasePermissions.ViewPages))
                            {
                                results.Add(mResult);
                            }
                            else
                            {
                                mLog.Warn(string.Format("The user don't have open file permission. file Url is : {0}.", fileUrl));
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.InsufficientPrivileges } });
                            }
                        }
                    }
                }
            }
            catch (IncorrectUserNameOrPasswordException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.UnAuthorized } });
                mLog.Warn(string.Format("An error occurred while Checking CheckLeaveStubOffice365UserPermission UserNameOrPassword error:{0}.", px.ToString()));
            }
            catch (PasswordExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.PasswordExpired } });
                mLog.Warn(string.Format("An error occurred while Checking CheckLeaveStubOffice365UserPermission Password error:{0}.", px.ToString()));
            }
            catch (Office365SiteExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.BadUrl } });
                mLog.Warn(string.Format("An error occurred while Checking CheckLeaveStubOffice365UserPermission Office365SiteExpiredException error:{0}.", px.ToString()));
            }
            catch (AveSecurityTrimingException se)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException } });
                mLog.Warn(string.Format("An error occurred while Checking CheckLeaveStubOffice365UserPermission AveSecurityTrimingException error:{0}.", se.ToString()));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SiteLockException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                        return;
                    }
                    if (ex.InnerException is SocketException socketException)
                    {
                        if (socketException.ErrorCode == (int)HttpStatusCode.NotFound)
                        {
                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                            return;
                        }
                        else
                        {
                            mLog.Warn($"SocketException ErrorCode {socketException.ErrorCode}");
                        }
                    }
                    if (ex.InnerException is System.IO.FileNotFoundException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    if (ex.InnerException is WebException webException)
                    {
                        if (webException != null)
                        {
                            var response = webException.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                return;
                            }
                        }
                    }
                    if (ex.Message.Contains("The status code is \"NotFound\""))
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    else
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                    }
                }
                else if (ex is AveSkipLockSiteException)
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                    return;
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckLeaveStubOffice365UserPermission error:{0}.", ex.ToString()));
            }
        }

        private void CheckUserSiteOwnerPermissionForSharePointSite(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                using (IAveSite site = objectModel.CreateSite(contract.SiteCollectionUrl))
                {
                    using (IAveWeb web = site.OpenWeb())
                    {
                        UserDetail needCheckUser = new UserDetail() { Email = contract.NeedCheckedUserMail };
                        try
                        {
                            IAvePrincipalInfo member = objectModel.Utility.ResolvePrincipal(site.RootWeb, contract.NeedCheckedUserUPN, AvePrincipalType.User, AvePrincipalSource.All, null, false);
                            needCheckUser.SPLoginName = member == null ? string.Empty : member.LoginName;
                            mLog.Info($"CheckUserSiteOwnerPermissionForSharePointSite UserDetail.SPLoginName:{needCheckUser.SPLoginName}.site.RootWeb.Template:{site.RootWeb.Template}.");
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"ResolvePrincipal error {e.ToString()}");
                        }
                        if (site.RootWeb.Template.StartsWith("SPSPERS#"))
                        {
                            #region onedrive
                            var owner = site.Owner;
                            var isOwner = CheckUserSitePermission(new List<IAveUser>() { owner }, needCheckUser, objectModel, site);
                            if (isOwner)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title,IsReadOnlySite= site.ReadOnly } });
                                return;
                            }
                            #endregion
                        }
                        else
                        {
                            var mResult = new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } };
                            if (site.RootWeb.Template == "GROUP#0")
                            {
                                if (!String.IsNullOrEmpty(contract.NeedCheckedGroupId))
                                {
                                    RMGroup checkGroupType = null;
                                    try
                                    {
                                        var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        if (appProfile == null)
                                        {
                                            Logger.Warn($"CheckUserSiteOwnerPermissionForSharePointSite can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                                            appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        }
                                        var groupSite = new RMGraphGroupManager(appProfile);
                                        checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                                    }
                                    catch (System.Exception ex)
                                    {
                                        checkGroupType = null;
                                        Logger.Error($"An error occur when GetGroup.Message:{ex.ToString()}.");
                                    }
                                    if (checkGroupType != null)
                                    {
                                        if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                                        {
                                            mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                                        }
                                        else
                                        {
                                            foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                                            {
                                                bool isTeamGroupSites = rpo.ToString().Contains("Team");
                                                if (isTeamGroupSites)
                                                {
                                                    mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #region group site or other site
                            var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                            var users = ownerGroup.Users.ToList();

                            bool isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, users);
                            if (isValid)
                            {
                                results.Add(mResult);
                                return;
                            }
                            #endregion
                        }

                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.UserNotOwnerForSharePointSite } });
                    }
                }
            }
            catch (IncorrectUserNameOrPasswordException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.UnAuthorized } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerPermissionForSharePointSite UserNameOrPassword error:{0}.", px.ToString()));
            }
            catch (PasswordExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.PasswordExpired } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerPermissionForSharePointSite Password error:{0}.", px.ToString()));
            }
            catch (Office365SiteExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.BadUrl } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerPermissionForSharePointSite Office365SiteExpiredException error:{0}.", px.ToString()));
            }
            catch (AveSecurityTrimingException se)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerPermissionForSharePointSite AveSecurityTrimingException error:{0}.", se.ToString()));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SiteLockException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                        return;
                    }
                    if (ex.InnerException is SocketException socketException)
                    {
                        if (socketException.ErrorCode == (int)HttpStatusCode.NotFound)
                        {
                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                            return;
                        }
                        else
                        {
                            mLog.Warn($"SocketException ErrorCode {socketException.ErrorCode}");
                        }
                    }
                    if (ex.InnerException is System.IO.FileNotFoundException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    if (ex.InnerException is WebException webException)
                    {
                        if (webException != null)
                        {
                            var response = webException.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                return;
                            }
                        }
                    }
                    if (ex.Message.Contains("The status code is \"NotFound\""))
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    else
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                    }
                }
                else if (ex is AveSkipLockSiteException)
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                    return;
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerPermissionForSharePointSite error:{0}.", ex.ToString()));
            }
        }

        private void CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                using (IAveSite site = objectModel.CreateSite(contract.SiteCollectionUrl))
                {
                    using (IAveWeb web = site.OpenWeb())
                    {
                        UserDetail needCheckUser = new UserDetail() { Email = contract.NeedCheckedUserMail };
                        try
                        {
                            IAvePrincipalInfo member = objectModel.Utility.ResolvePrincipal(site.RootWeb, contract.NeedCheckedUserUPN, AvePrincipalType.User, AvePrincipalSource.All, null, false);
                            needCheckUser.SPLoginName = member == null ? string.Empty : member.LoginName;
                            mLog.Info($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite UserDetail.SPLoginName:{needCheckUser.SPLoginName}.site.RootWeb.Template:{site.RootWeb.Template}.");
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"ResolvePrincipal error {e.ToString()}");
                        }
                        if (site.RootWeb.Template.StartsWith("SPSPERS#"))
                        {
                            #region onedrive
                            var owner = site.Owner;
                            var isOwner = CheckUserSitePermission(new List<IAveUser>() { owner }, needCheckUser, objectModel, site);
                            if (isOwner)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } });
                                return;
                            }
                            #endregion
                        }
                        else
                        {
                            var mResult = new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } };
                            if (site.RootWeb.Template == "GROUP#0")
                            {
                                if (!String.IsNullOrEmpty(contract.NeedCheckedGroupId))
                                {
                                    RMGroup checkGroupType = null;
                                    try
                                    {
                                        var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        if (appProfile == null)
                                        {
                                            Logger.Warn($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                                            appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        }
                                        var groupSite = new RMGraphGroupManager(appProfile);
                                        checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                                    }
                                    catch (System.Exception ex)
                                    {
                                        checkGroupType = null;
                                        Logger.Error($"An error occur when GetGroup.Message:{ex.ToString()}.");
                                    }
                                    if (checkGroupType != null)
                                    {
                                        if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                                        {
                                            mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                                        }
                                        else
                                        {
                                            foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                                            {
                                                bool isTeamGroupSites = rpo.ToString().Contains("Team");
                                                if (isTeamGroupSites)
                                                {
                                                    mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #region group site or other site
                            //Check Site Owner Permission
                            var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                            if (ownerGroup != null)
                            {
                                if (ownerGroup.Users != null)
                                {
                                    var ownerUsers = ownerGroup.Users.ToList();

                                    bool isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, ownerUsers);
                                    if (isValid)
                                    {
                                        results.Add(mResult);
                                        return;
                                    }
                                }
                                else
                                {
                                    mLog.Warn($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite AssociatedOwnerGroup Users is null.");
                                }
                            }
                            else
                            {
                                mLog.Warn($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite AssociatedOwnerGroup is null.");
                            }
                            //Check Site Member Permission
                            var memberGroup = site.RootWeb.AssociatedMemberGroup;
                            if (memberGroup != null) 
                            {
                                if (memberGroup.Users != null)
                                {
                                    var memberUsers = memberGroup.Users.ToList();

                                    bool isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, memberUsers);
                                    if (isValid)
                                    {
                                        results.Add(mResult);
                                        return;
                                    }
                                }
                                else
                                {
                                    mLog.Warn($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite AssociatedMemberGroup Users is null.");
                                }
                            }
                            else
                            {
                                mLog.Warn($"CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite AssociatedMemberGroup is null.");
                            }
                            #endregion
                        }

                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.UserNotOwnerOrMemberForSharePointSite } });
                    }
                }
            }
            catch (IncorrectUserNameOrPasswordException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.UnAuthorized } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite UserNameOrPassword error:{0}.", px.ToString()));
            }
            catch (PasswordExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.PasswordExpired } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite Password error:{0}.", px.ToString()));
            }
            catch (Office365SiteExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.BadUrl } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite Office365SiteExpiredException error:{0}.", px.ToString()));
            }
            catch (AveSecurityTrimingException se)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite AveSecurityTrimingException error:{0}.", se.ToString()));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SiteLockException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                        return;
                    }
                    if (ex.InnerException is SocketException socketException)
                    {
                        if (socketException.ErrorCode == (int)HttpStatusCode.NotFound)
                        {
                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                            return;
                        }
                        else
                        {
                            mLog.Warn($"SocketException ErrorCode {socketException.ErrorCode}");
                        }
                    }
                    if (ex.InnerException is System.IO.FileNotFoundException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    if (ex.InnerException is WebException webException)
                    {
                        if (webException != null)
                        {
                            var response = webException.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                return;
                            }
                        }
                    }
                    if (ex.Message.Contains("The status code is \"NotFound\""))
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    else
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                    }
                }
                else if (ex is AveSkipLockSiteException)
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                    return;
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberPermissionForSharePointSite error:{0}.", ex.ToString()));
            }
        }

        private void CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                using (IAveSite site = objectModel.CreateSite(contract.SiteCollectionUrl))
                {
                    using (IAveWeb web = site.OpenWeb())
                    {
                        UserDetail needCheckUser = new UserDetail() { Email = contract.NeedCheckedUserMail };
                        try
                        {
                            IAvePrincipalInfo member = objectModel.Utility.ResolvePrincipal(site.RootWeb, contract.NeedCheckedUserUPN, AvePrincipalType.User, AvePrincipalSource.All, null, false);
                            needCheckUser.SPLoginName = member == null ? string.Empty : member.LoginName;
                            mLog.Info($"CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite UserDetail.SPLoginName:{needCheckUser.SPLoginName}.site.RootWeb.Template:{site.RootWeb.Template}.");
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"ResolvePrincipal error {e.ToString()}");
                        }
                        if (site.RootWeb.Template.StartsWith("SPSPERS#"))
                        {
                            #region onedrive
                            var owner = site.Owner;
                            var isOwner = CheckUserSitePermission(new List<IAveUser>() { owner }, needCheckUser, objectModel, site);
                            if (isOwner)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } });
                                return;
                            }
                            #endregion
                        }
                        else
                        {
                            var mResult = new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } };
                            if (site.RootWeb.Template == "GROUP#0")
                            {
                                if (!String.IsNullOrEmpty(contract.NeedCheckedGroupId))
                                {
                                    RMGroup checkGroupType = null;
                                    try
                                    {
                                        var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        if (appProfile == null)
                                        {
                                            Logger.Warn($"CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                                            appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        }
                                        var groupSite = new RMGraphGroupManager(appProfile);
                                        checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                                    }
                                    catch (System.Exception ex)
                                    {
                                        checkGroupType = null;
                                        Logger.Error($"An error occur when GetGroup.Message:{ex.ToString()}.");
                                    }
                                    if (checkGroupType != null)
                                    {
                                        if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                                        {
                                            mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                                        }
                                        else
                                        {
                                            foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                                            {
                                                bool isTeamGroupSites = rpo.ToString().Contains("Team");
                                                if (isTeamGroupSites)
                                                {
                                                    mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #region group site or other site
                            //Check Site Owner Permission
                            var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                            var ownerUsers = ownerGroup.Users.ToList();

                            bool isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, ownerUsers);
                            if (isValid)
                            {
                                results.Add(mResult);
                                return;
                            }

                            //Check Site Member Permission
                            var memberGroup = site.RootWeb.AssociatedMemberGroup;
                            var memberUsers = memberGroup.Users.ToList();

                            isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, memberUsers);
                            if (isValid)
                            {
                                results.Add(mResult);
                                return;
                            }

                            //Check Site Visitor Permission
                            var visitorGroup = site.RootWeb.AssociatedVisitorGroup;
                            var visitorUsers = visitorGroup.Users.ToList();

                            isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, visitorUsers);
                            if (isValid)
                            {
                                results.Add(mResult);
                                return;
                            }
                            #endregion
                        }

                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.UserNotOwnerOrMemberOrVisitorForSharePointSite } });
                    }
                }
            }
            catch (IncorrectUserNameOrPasswordException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.UnAuthorized } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite UserNameOrPassword error:{0}.", px.ToString()));
            }
            catch (PasswordExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.PasswordExpired } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite Password error:{0}.", px.ToString()));
            }
            catch (Office365SiteExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.BadUrl } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite Office365SiteExpiredException error:{0}.", px.ToString()));
            }
            catch (AveSecurityTrimingException se)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite AveSecurityTrimingException error:{0}.", se.ToString()));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SiteLockException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                        return;
                    }
                    if (ex.InnerException is SocketException socketException)
                    {
                        if (socketException.ErrorCode == (int)HttpStatusCode.NotFound)
                        {
                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                            return;
                        }
                        else
                        {
                            mLog.Warn($"SocketException ErrorCode {socketException.ErrorCode}");
                        }
                    }
                    if (ex.InnerException is System.IO.FileNotFoundException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    if (ex.InnerException is WebException webException)
                    {
                        if (webException != null)
                        {
                            var response = webException.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                return;
                            }
                        }
                    }
                    if (ex.Message.Contains("The status code is \"NotFound\""))
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    else
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                    }
                }
                else if (ex is AveSkipLockSiteException)
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                    return;
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSiteMemberOrSiteVisitorPermissionForSharePointSite error:{0}.", ex.ToString()));
            }
        }

        private void CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                using (IAveSite site = objectModel.CreateSite(contract.SiteCollectionUrl))
                {
                    using (IAveWeb web = site.OpenWeb())
                    {
                        UserDetail needCheckUser = new UserDetail() { Email = contract.NeedCheckedUserMail };
                        try
                        {
                            IAvePrincipalInfo member = objectModel.Utility.ResolvePrincipal(site.RootWeb, contract.NeedCheckedUserUPN, AvePrincipalType.User, AvePrincipalSource.All, null, false);
                            needCheckUser.SPLoginName = member == null ? string.Empty : member.LoginName;
                            mLog.Info($"CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite UserDetail.SPLoginName:{needCheckUser.SPLoginName}.site.RootWeb.Template:{site.RootWeb.Template}.");
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"ResolvePrincipal error {e.ToString()}");
                        }
                        if (site.RootWeb.Template.StartsWith("SPSPERS#"))
                        {
                            #region onedrive
                            var owner = site.Owner;
                            var isOwner = CheckUserSitePermission(new List<IAveUser>() { owner }, needCheckUser, objectModel, site);
                            if (isOwner)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } });
                                return;
                            }
                            #endregion
                        }
                        else
                        {
                            var mResult = new Office365MessageContract() { Result = new Result() { Status = true, Title = web.Title, IsReadOnlySite = site.ReadOnly } };
                            if (site.RootWeb.Template == "GROUP#0")
                            {
                                if (!String.IsNullOrEmpty(contract.NeedCheckedGroupId))
                                {
                                    RMGroup checkGroupType = null;
                                    try
                                    {
                                        var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        if (appProfile == null)
                                        {
                                            Logger.Warn($"CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                                            appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                        }
                                        var groupSite = new RMGraphGroupManager(appProfile);
                                        checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                                    }
                                    catch (System.Exception ex)
                                    {
                                        checkGroupType = null;
                                        Logger.Error($"An error occur when GetGroup.Message:{ex.ToString()}.");
                                    }
                                    if (checkGroupType != null)
                                    {
                                        if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                                        {
                                            mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                                        }
                                        else
                                        {
                                            foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                                            {
                                                bool isTeamGroupSites = rpo.ToString().Contains("Team");
                                                if (isTeamGroupSites)
                                                {
                                                    mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #region group site or other site
                            var ownerGroup = site.RootWeb.AssociatedOwnerGroup;
                            var users = ownerGroup.Users.ToList();

                            var isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, users);
                            if (isValid)
                            {
                                results.Add(mResult);
                                return;
                            }

                            var specifiedGroupNames = ParseSpecifiedSharePointGroupNames(contract.SpecifiedGroupNameForSharePointSite);
                            var specifiedGroups = site.RootWeb.Groups
                                .Where(g => specifiedGroupNames.Any(groupName => IsGroupNameMatched(g.Name, groupName)))
                                .ToList();

                            if (specifiedGroups.Any())
                            {
                                foreach (var specifiedGroup in specifiedGroups)
                                {
                                    var specifiedGroupUsers = specifiedGroup.Users.ToList();
                                    isValid = ValidateUserPermission(contract, objectModel, site, needCheckUser, specifiedGroupUsers);
                                    if (isValid)
                                    {
                                        results.Add(mResult);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                mLog.Warn($"Current SpecifiedGroupNameForSharePointSite can't find in SharePoint:{contract.SpecifiedGroupNameForSharePointSite}.");
                            }
                            #endregion
                        }

                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.UserNotOwnerOrSpecifiedGroupForSharePointSite } });
                    }
                }
            }
            catch (IncorrectUserNameOrPasswordException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.UnAuthorized } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite UserNameOrPassword error:{0}.", px.ToString()));
            }
            catch (PasswordExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.PasswordExpired } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite Password error:{0}.", px.ToString()));
            }
            catch (Office365SiteExpiredException px)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.BadUrl } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite error:{0}.", px.ToString()));
            }
            catch (AveSecurityTrimingException se)
            {
                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException } });
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite error:{0}.", se.ToString()));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SiteLockException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                        return;
                    }
                    if (ex.InnerException is SocketException socketException)
                    {
                        if (socketException.ErrorCode == (int)HttpStatusCode.NotFound)
                        {
                            results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                            return;
                        }
                        else
                        {
                            mLog.Warn($"SocketException ErrorCode {socketException.ErrorCode}");
                        }
                    }
                    if (ex.InnerException is System.IO.FileNotFoundException)
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    if (ex.InnerException is WebException webException)
                    {
                        if (webException != null)
                        {
                            var response = webException.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                                return;
                            }
                        }
                    }
                    if (ex.Message.Contains("The status code is \"NotFound\""))
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound } });
                        return;
                    }
                    else
                    {
                        results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                    }
                }
                else if (ex is AveSkipLockSiteException)
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked } });
                    return;
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckUserSiteOwnerOrSpecialGroupPermissionForSharePointSite error:{0}.", ex.ToString()));
            }
        }

        private static List<string> ParseSpecifiedSharePointGroupNames(string specifiedGroupNames)
        {
            if (string.IsNullOrWhiteSpace(specifiedGroupNames))
            {
                return new List<string>();
            }

            return specifiedGroupNames
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsGroupNameMatched(string actualGroupName, string configuredPattern)
        {
            if (string.IsNullOrWhiteSpace(actualGroupName) || string.IsNullOrWhiteSpace(configuredPattern))
            {
                return false;
            }

            if (!configuredPattern.Contains("*"))
            {
                return actualGroupName.EqualIgnoreCase(configuredPattern);
            }

            var startsWithWildcard = configuredPattern.StartsWith("*");
            var endsWithWildcard = configuredPattern.EndsWith("*");
            var startIndex = startsWithWildcard ? 1 : 0;
            var endIndex = endsWithWildcard ? configuredPattern.Length - 1 : configuredPattern.Length;
            var coreLength = endIndex - startIndex;
            if (coreLength <= 0)
            {
                return false;
            }

            var core = configuredPattern.Substring(startIndex, coreLength);
            if (core.Contains("*"))
            {
                return false;
            }

            if (startsWithWildcard && endsWithWildcard)
            {
                return actualGroupName.IndexOf(core, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (startsWithWildcard)
            {
                return actualGroupName.EndsWith(core, StringComparison.OrdinalIgnoreCase);
            }

            if (endsWithWildcard)
            {
                return actualGroupName.StartsWith(core, StringComparison.OrdinalIgnoreCase);
            }

            return actualGroupName.EqualIgnoreCase(configuredPattern);
        }

        private void CheckGroupOwnerOffice365UserPermission(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                var mResult = new Office365MessageContract() { Result = new Result() { Status = true } };
                if (!string.IsNullOrEmpty(contract.NeedCheckedGroupId))
                {
                    var userDetails = GraphHelper.GetGroupOwnersById(contract.NeedCheckedGroupId, objectModel.AccountInfo).ToList();
                    //var checkGroupType = GraphHelper.GetGroup(contract.NeedCheckedGroupId, objectModel.AccountInfo);
                    var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                    if (appProfile == null)
                    {
                        Logger.Warn($"CheckGroupOwnerOffice365UserPermission can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                        appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                    }
                    var groupSite = new RMGraphGroupManager(appProfile);
                    var checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                    if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                    {
                        mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                    }
                    else
                    {
                        foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                        {
                            bool isTeamGroupSites = rpo.ToString().Contains("Team");
                            if (isTeamGroupSites)
                            {
                                mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                break;
                            }
                        }
                    }
                    if (userDetails.Exists(u => u.Mail.EqualIgnoreCase(contract.NeedCheckedUserMail) || u.UserPrincipalName.EqualIgnoreCase(contract.NeedCheckedUserMail)))
                    {
                        results.Add(mResult);
                        return;
                    }
                    else
                    {
                        mLog.Warn($"CheckGroupOwnerOffice365UserPermission userDetails is not in group owner.NeedCheckedUserMail:{contract.NeedCheckedUserMail}.");
                        if (userDetails == null || userDetails.Count == 0)
                        {
                            mLog.Warn($"CheckGroupOwnerOffice365UserPermission userDetails is null.NeedCheckedGroupId:{contract.NeedCheckedGroupId}.");
                        }
                        else if (userDetails.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var graphUser in userDetails)
                            {
                                sb.AppendFormat("GroupOwner.UserMail:{0}.UserPrincipalName:{1}.{2}.", graphUser.Mail, graphUser.UserPrincipalName, Environment.NewLine);
                            }
                            mLog.Warn($"CheckGroupOwnerOffice365UserPermission userDetails is not in GroupOwner.UserCollection:{sb.ToString()}.");
                        }
                    }
                }
                else
                {
                    mLog.Warn($"cannot find group id in Office365MessageContract");
                }
                mResult.Result.Status = false;
                mResult.Result.ErrorInfo = ErrorInfo.InsufficientPrivileges;
                results.Add(mResult);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Request_ResourceNotFound"))
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.NotFound } });
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckSiteCollectionOffice365UserPermission error:{0}.", ex.ToString()));
            }
        }
        private void CheckGroupOwnerOrMemberOffice365UserPermission(Office365MessageContract contract, List<BrowserContractBase> results, AveObjectModelFactory objectModel)
        {
            try
            {
                var mResult = new Office365MessageContract() { Result = new Result() { Status = true } };
                if (!string.IsNullOrEmpty(contract.NeedCheckedGroupId))
                {
                    var ownersDetails = GraphHelper.GetGroupOwnersById(contract.NeedCheckedGroupId, objectModel.AccountInfo).ToList();
                    var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                    if (appProfile == null)
                    {
                        Logger.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission can not find opus app,need to find aosp app:365tenant:{objectModel.AccountInfo.TenantId}");
                        appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                    }
                    var groupSite = new RMGraphGroupManager(appProfile);
                    var checkGroupType = groupSite.GetGroup(contract.NeedCheckedGroupId).GetAwaiter().GetResult();
                    if (checkGroupType.ResourceProvisioningOptions != null && checkGroupType.ResourceProvisioningOptions.Count() == 0)
                    {
                        mResult.Result.SiteCollectionType = SiteCollectionType.Group;
                    }
                    else
                    {
                        foreach (var rpo in checkGroupType.ResourceProvisioningOptions)
                        {
                            bool isTeamGroupSites = rpo.ToString().Contains("Team");
                            if (isTeamGroupSites)
                            {
                                mResult.Result.SiteCollectionType = SiteCollectionType.Teams;
                                break;
                            }
                        }
                    }
                    if (ownersDetails.Exists(u => u.Mail.EqualIgnoreCase(contract.NeedCheckedUserMail) || u.UserPrincipalName.EqualIgnoreCase(contract.NeedCheckedUserMail)))
                    {
                        results.Add(mResult);
                        return;
                    }
                    else
                    {
                        mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission ownersDetails is not in group owner.NeedCheckedUserMail:{contract.NeedCheckedUserMail}.");
                        if (ownersDetails == null || ownersDetails.Count == 0)
                        {
                            mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission ownersDetails is null.NeedCheckedGroupId:{contract.NeedCheckedGroupId}.");
                        }
                        else if (ownersDetails.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var graphUser in ownersDetails)
                            {
                                sb.AppendFormat("GroupOwner.UserMail:{0}.UserPrincipalName:{1}.{2}.", graphUser.Mail, graphUser.UserPrincipalName, Environment.NewLine);
                            }
                            mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission ownersDetails is not in GroupOwner.UserCollection:{sb.ToString()}.");
                        }
                    }
                    var membersDetails = GraphHelper.GetGroupMemberById(contract.NeedCheckedGroupId, objectModel.AccountInfo).ToList();
                    if (membersDetails.Exists(u => u.Mail.EqualIgnoreCase(contract.NeedCheckedUserMail) || u.UserPrincipalName.EqualIgnoreCase(contract.NeedCheckedUserMail)))
                    {
                        results.Add(mResult);
                        return;
                    }
                    else
                    {
                        mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission membersDetails is not in group owner.NeedCheckedUserMail:{contract.NeedCheckedUserMail}.");
                        if (membersDetails == null || membersDetails.Count == 0)
                        {
                            mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission membersDetails is null.NeedCheckedGroupId:{contract.NeedCheckedGroupId}.");
                        }
                        else if (membersDetails.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var graphUser in membersDetails)
                            {
                                sb.AppendFormat("GroupOwner.UserMail:{0}.UserPrincipalName:{1}.{2}.", graphUser.Mail, graphUser.UserPrincipalName, Environment.NewLine);
                            }
                            mLog.Warn($"CheckGroupOwnerOrMemberOffice365UserPermission membersDetails is not in GroupOwner.UserCollection:{sb.ToString()}.");
                        }
                    }
                }
                else
                {
                    mLog.Warn($"cannot find group id in Office365MessageContract");
                }
                mResult.Result.Status = false;
                mResult.Result.ErrorInfo = ErrorInfo.UserNotGroupOwnerOrMember;
                results.Add(mResult);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Request_ResourceNotFound"))
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.NotFound } });
                }
                else
                {
                    results.Add(new Office365MessageContract() { Result = new Result() { Status = false, ErrorInfo = ErrorInfo.Unknown } });
                }
                mLog.Warn(string.Format("An error occurred while Checking CheckSiteCollectionOffice365UserPermission error:{0}.", ex.ToString()));
            }
        }

        // not use this method to check domain group permission anymore
        private bool CheckUserSitePermission(List<IAveUser> users, UserDetail verifyUser, AveObjectModelFactory objectModel, IAveSite site)
        {
            var accounts = users.Where(item => item.ID != 1073741823).ToList();
            if (accounts.Count == 0)
            {
                Logger.Warn($"Can't find site owners.");
                return false;
            }
            foreach (var o in users)
            {
                if (o.IsDomainGroup)
                {
                    #region old logic
                    //var ret = CheckOnlineMembersInGroup(o.Name, o.LoginName, objectModel, verifyUser);
                    //if (ret)
                    //{
                    //    mLog.Info($"Find user in the group {o.Name}");
                    //    return true;
                    //}
                    #endregion
                    mLog.Warn($"user {o.LoginName} is a domain group, should check the group members with Graph API");
                }
                else
                {
                    var userHasPermission = CheckUserSitePermission(o.Email, o.LoginName, verifyUser.Email, verifyUser.SPLoginName, objectModel, site);
                    if (userHasPermission)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckUserSitePermission(List<UserDetail> users, UserDetail verifyUser, AveObjectModelFactory objectModel, IAveSite site)
        {
            bool userHasPermission = false;
            foreach (UserDetail user in users)
            {
                if (GroupNames.Contains(user.DisplayName))
                {
                    mLog.Info($"GroupNames contains this group name,continue,name:{user.DisplayName}");
                    continue;
                }
                if (user.AccountType == AccountType.ADGroup)
                {
                    if (!GroupNames.Contains(user.DisplayName))
                    {
                        mLog.Info($"Add group name into GroupNames,name:{user.DisplayName}");
                        GroupNames.Add(user.DisplayName);
                    }
                    string groupId = GetGroupIdWithLoginName(user.LoginName);
                    Guid tempGroupId = new Guid();
                    if (!Guid.TryParse(groupId, out tempGroupId))
                    {
                        groupId = user.Id;
                    }
                    if (!string.IsNullOrEmpty(groupId))
                    {
                        var userDetails = GetOnlineMembersInGroup(groupId, user.DisplayName, objectModel);
                        userHasPermission = CheckUserSitePermission(userDetails, verifyUser, objectModel, site);
                        if (userHasPermission)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        mLog.Warn($"cannot find group id by user {user.LoginName}");
                    }
                }
                else
                {
                    userHasPermission = CheckUserSitePermission(user.Email, user.LoginName, verifyUser.Email, verifyUser.SPLoginName, objectModel, site);
                }
                if (userHasPermission)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userLoginName"></param>
        /// <param name="verifyUserEmail">AOS取到是User Principle Name，不是User Email</param>
        /// <param name="verifySPLogin"></param>
        /// <param name="objectModel"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool CheckUserSitePermission(string userEmail, string userLoginName, string verifyUserEmail, string verifySPLogin, AveObjectModelFactory objectModel, IAveSite site)
        {
            mLog.Info($"CheckUserSitePermission.userEmail:{userEmail}.userLoginName:{userLoginName}.verifyUserEmail:{verifyUserEmail}.verifySPLogin:{verifySPLogin}.");
            if (!string.IsNullOrEmpty(verifyUserEmail) && !string.IsNullOrEmpty(userEmail))
            {
                if (userEmail.Equals(verifyUserEmail, StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Info($"CheckUserSitePermission success by Email.");
                    return true;
                }
                else
                {
                    mLog.Info($"CheckUserSitePermission failed by Email.");
                    return false;
                }
            }
            //当UPN Name和Email不一致时，通过比较LoginName进行兼容
            else if (!string.IsNullOrEmpty(userLoginName) && !string.IsNullOrEmpty(verifySPLogin))
            {
                string userSPLogin = string.Empty;
                if (site != null)
                {
                    IAvePrincipalInfo member = objectModel.Utility.ResolvePrincipal(site.RootWeb, userLoginName, AvePrincipalType.SecurityGroup | AvePrincipalType.SharePointGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false);
                    userSPLogin = member == null ? string.Empty : member.LoginName;
                    mLog.Info($"CheckUserSitePermission.ownerSPLogin:{userSPLogin}.");
                }
                if (userSPLogin.Equals(verifySPLogin, StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Info($"CheckUserSitePermission success by userSPLogin.");
                    return true;
                }
                else if (userLoginName.Equals(verifySPLogin, StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Info($"CheckUserSitePermission success by userLoginName.");
                    return true;
                }
            }
            else
            {
                mLog.Warn($"CheckUserSitePermission failed by userLoginName.");
            }
            mLog.Info($"CheckUserSitePermission all ways failed.");
            return false;
        }

        public List<UserDetail> GetOnlineMembersInGroup(string groupId, string groupName, AveObjectModelFactory mObjectModel)
        {
            List<UserDetail> users = new List<UserDetail>();
            bool isOwnerGroup = false;
            string newGroupId = string.Empty;
            if (groupId.EndsWith("_o", StringComparison.OrdinalIgnoreCase))
            {
                isOwnerGroup = true;
                newGroupId = groupId.TrimEnd('o').TrimEnd('_');
            }
            Guid groupIdFromName = Guid.Empty;
            try
            {
                groupIdFromName = new Guid(isOwnerGroup ? newGroupId : groupId);
            }
            catch (Exception ex)
            {
                mLog.Warn("try to get group id to guid failed, error {0}", ex.ToString());
                groupIdFromName = Guid.Empty;
            }
            if (groupIdFromName == Guid.Empty)
            {
                mLog.Info("Begin to get GetOnlineMembersInGroup1,group name {0},group id {1}", groupName, groupId);
                try
                {
                    groupIdFromName = new Guid(GraphHelper.GetGroupIdByDisplayName(groupName, mObjectModel.AccountInfo));
                }
                catch (Exception ex)
                {
                    mLog.Warn($"Failed GetGroupIdByDisplayName by MS Graph API, message {ex}");
                }
                mLog.Info("end to get GetOnlineMembersInGroup1 with name ,id is {0}", groupIdFromName);
            }
            List<GraphUser> ownerMembers = null;
            List<GraphUser> groupMembers = null;
            try
            {
                if (isOwnerGroup)
                {
                    ownerMembers = GraphHelper.GetGroupOwnersById(groupIdFromName.ToString(), mObjectModel.AccountInfo).ToList();
                }
                else
                {
                    groupMembers = GraphHelper.GetGroupMemberById(groupIdFromName.ToString(), mObjectModel.AccountInfo).ToList();
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("error to get group members with id from name {0}", ex.ToString());
            }
            if (isOwnerGroup)
            {
                if (ownerMembers != null && ownerMembers.Count > 0)
                {
                    mLog.Info("Group owner members count is {0}", ownerMembers.Count);
                    foreach (var ownerMember in ownerMembers)
                    {
                        if (!string.IsNullOrEmpty(ownerMember.DisplayName))
                        {
                            UserDetail user = new UserDetail()
                            {
                                Id = ownerMember.Id,
                                DisplayName = ownerMember.DisplayName,
                                Email = ownerMember.Mail,
                                LoginName = ownerMember.UserPrincipalName,
                                AccountType = AccountType.ADUser
                            };
                            users.Add(user);
                        }
                    }
                }
                else
                {
                    mLog.Info("Group owner members count is null");
                }
            }
            else
            {
                if (groupMembers == null)
                {
                    mLog.Info("Get group members is null, will try to reget with real group name");
                    try
                    {
                        groupMembers = GraphHelper.GetGroupMembersByDisplayName(GetRealGroupName(groupName), mObjectModel.AccountInfo).ToList();
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Error to GetGroupMembersByDisplayName by Graph API, message {0}", ex.ToString());
                    }
                    //members = shellRequest.GetGroupMembers(GetRealGroupName(groupName));
                }
                if (groupMembers != null && groupMembers.Count > 0)
                {
                    mLog.Info("Group member members count is {0}", groupMembers.Count);
                    foreach (var member in groupMembers)
                    {
                        if (!string.IsNullOrEmpty(member.DisplayName))
                        {
                            UserDetail user = new UserDetail();
                            user.Id = member.Id;
                            user.DisplayName = member.DisplayName;
                            user.Email = member.Mail;
                            user.LoginName = member.UserPrincipalName;
                            user.AccountType = AccountType.ADUser;
                            if (member.OdataType.Equals("#microsoft.graph.group"))
                            {
                                RMGroup adGroup = null;
                                try
                                {
                                    var appProfile = PoolUserUtil.GetBPOSInfoAsync(mObjectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                    var groupSite = new RMGraphGroupManager(appProfile);
                                    adGroup = groupSite.GetGroup(member.Id).GetAwaiter().GetResult();
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("Error to GetGroup with id failed, message {0}", ex.ToString());
                                }
                                if (adGroup != null)
                                {
                                    if ((bool)adGroup.MailEnabled)
                                    {
                                        user.LoginName = adGroup.Mail;
                                    }
                                    else
                                    {
                                        user.LoginName = adGroup.DisplayName;
                                    }
                                }
                                user.AccountType = AccountType.ADGroup;
                            }
                            else if (member.OdataType.Equals("#microsoft.graph.user"))
                            {
                                if (string.IsNullOrEmpty(user.LoginName))
                                {
                                    GraphUser aduser = null;
                                    try
                                    {
                                        aduser = GraphHelper.GetGraphUser(member.Id, mObjectModel.AccountInfo);
                                    }
                                    catch (Exception ex)
                                    {
                                        mLog.Warn("Error to GetGraphUser with id failed, message {0}", ex.ToString());
                                    }
                                    if (aduser != null)
                                    {
                                        user.LoginName = aduser.UserPrincipalName;
                                    }
                                }
                                user.AccountType = AccountType.ADUser;
                            }
                            else
                            {
                                mLog.Info("the ad group try to get is null,name {0}", member.DisplayName);
                                continue;
                            }
                            users.Add(user);
                        }
                    }
                }
                else
                { mLog.Info("Group  members count is null"); }
            }
            return users;
        }

        public string GetGroupIdWithLoginName(string loginName)
        {
            try
            {
                string[] loginNameSlip = loginName.Split('|');
                if (loginNameSlip.Length > 0)
                {
                    string groupID = loginNameSlip.Last();
                    return groupID;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("failed to get group id with login name,error {0}", ex.ToString());
            }
            return string.Empty;
        }

        private static string GetWebServerServerRelativeUrl(string webUrl, IAveSite site)
        {
            if (webUrl.TrimEnd('/').Length == site.Url.TrimEnd('/').Length)
            {
                return string.Empty;
            }
            else
            {
                int hostLength = site.Url.TrimEnd('/').Length - site.ServerRelativeUrl.TrimEnd('/').Length;
                return webUrl.Substring(hostLength, webUrl.Length - hostLength);
            }
        }

        private bool CheckWhetherUserInADGroup(RMGroup group, AveObjectModelFactory mObjectModel, UserDetail userDetail)
        {
            var members = GraphHelper.GetGroupMembersByDisplayName(group.DisplayName, mObjectModel.AccountInfo);
            foreach (var member in members)
            {
                if (member.OdataType.Equals("#microsoft.graph.group"))
                {
                    RMGroup adGroup = null;
                    try
                    {
                        var appProfile = PoolUserUtil.GetBPOSInfoAsync(mObjectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                        var groupSite = new RMGraphGroupManager(appProfile);
                        adGroup = groupSite.GetGroup(member.Id).GetAwaiter().GetResult();
                        return CheckWhetherUserInADGroup(adGroup, mObjectModel, userDetail);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Error to GetGroup with id failed, message {0}", ex.ToString());
                    }

                }
                else if (member.OdataType.Equals("#microsoft.graph.user"))
                {
                    if (string.Equals(member.Mail, userDetail.Email, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Info("the ad group try to get is null,name {0}", member.DisplayName);
                    continue;
                }
            }
            return false;
        }

        private bool CheckOnlineMembersInGroup(string groupName, string loginName, AveObjectModelFactory mObjectModel, UserDetail userDetail)
        {
            try
            {
                IList<GraphUser> members = new List<GraphUser>();
                try
                {
                    members = GraphHelper.GetGroupMembersByDisplayName(groupName, mObjectModel.AccountInfo);
                }
                catch (Exception ex)
                {
                    mLog.Error($"An error occurred while get members by group name use graph api. {ex}");
                }
                try
                {
                    if (members == null || members.Count == 0)
                    {
                        members = GraphHelper.GetGroupMembersByDisplayName(GetRealGroupName(groupName), mObjectModel.AccountInfo);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error($"An error occurred  while get members by group real name use graph api. {ex}");
                }

                try
                {
                    if (members == null || members.Count == 0)
                    {
                        members = GraphHelper.GetGroupMemberById(GetGroupId(loginName).ToString(), mObjectModel.AccountInfo);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error($"An error occurred  while get members by group Id user graph api. {ex}");
                }

                if (members != null && members.Count > 0)
                {
                    foreach (var member in members)
                    {
                        if (!string.IsNullOrEmpty(member.DisplayName))
                        {
                            if (member.OdataType.Equals("#microsoft.graph.group"))
                            {
                                RMGroup adGroup = null;
                                try
                                {
                                    var appProfile = PoolUserUtil.GetBPOSInfoAsync(mObjectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                                    var groupSite = new RMGraphGroupManager(appProfile);
                                    adGroup = groupSite.GetGroup(member.Id).GetAwaiter().GetResult();
                                    var re = CheckWhetherUserInADGroup(adGroup, mObjectModel, userDetail);
                                    if (re)
                                    {
                                        return true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("Error to GetGroup with id failed, message {0}", ex.ToString());
                                }

                            }
                            else if (member.OdataType.Equals("#microsoft.graph.user"))
                            {
                                if (string.Equals(member.Mail, userDetail.Email, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    return true;
                                }
                                //GraphUser aduser = null;
                                //try
                                //{
                                //    aduser = GroupSiteHelper.GetGraphUser(member.Id, mObjectModel.AccountInfo);
                                //}
                                //catch (Exception ex)
                                //{
                                //    mLog.Warn("Error to GetGraphUser with id failed, message {0}", ex.ToString());
                                //}
                            }
                            else
                            {
                                mLog.Info("the ad group try to get is null,name {0}", member.DisplayName);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Info("an error occurred while get the ad group members. {0} {1}", groupName, ex);
                throw;
            }
            return false;
        }

        private Guid GetGroupId(string loginName)
        {
            if (loginName.IndexOf('|') > -1)
            {
                var groupId = "";
                var temp = loginName.Split('|');
                if (temp.Length > 1)
                {
                    groupId = temp[2];
                }
                else
                {
                    groupId = temp[1];
                }
                var realGroupId = groupId.Replace("_o", "").Trim();
                return new Guid(realGroupId);
            }
            return new Guid(loginName);
        }

        private string GetRealGroupName(string groupName)
        {
            string realGroupName = groupName;
            List<string> membersLanguage = new List<string>() {
                "اعضاء", "Üzvlər", "kideak", "Članova", "Членове", "membres", "成员", "成員", "Članovi", "Členy",
                "Medlemmer", "اعضاء", "Leden", "Members", "Liikmed", "Jäsenet", "Membres", "membros", "Mitglieder", "Μέλη",
                "חברים", "सदस्यों", "Tagok", "anggota", "baill", "Membri", "メンバー", "мүшелері", "멤버", "Dalībnieki",
                "Nariai", "членови", "Ahli-ahli", "Medlemmer", "Członków", "Membros de", "Membros de", "Membri", "Членов", "Чланови",
                "Чланови", "Členov", "Člani", "Miembros", "Medlemmar", "Medlemmar", "Üyeler", "Членів", "Thành viên", "Aelodau", "Owners"
            };
            foreach (string name in membersLanguage)
            {
                if (groupName.EndsWith(" " + name, StringComparison.OrdinalIgnoreCase)) //Office 365 type的group加入site collection的时候，display name会自动添加“ Members”这个后缀(部分语言在前面)，所以需要删除后缀后才能get到对应AD group
                {
                    realGroupName = groupName.Substring(0, groupName.LastIndexOf(" " + name, StringComparison.OrdinalIgnoreCase));
                    break;
                }
                if (groupName.StartsWith(name + " ", StringComparison.OrdinalIgnoreCase))
                {
                    realGroupName = groupName.Substring((name + " ").Length);
                    break;
                }
            }
            return realGroupName;
        }

        private bool ValidateUserPermission(Office365MessageContract contract, AveObjectModelFactory objectModel, IAveSite site, UserDetail needCheckUser, List<IAveUser> users)
        {
            // Check direct users
            mLog.Info($"ValidateUserPermission: Checking direct users for '{needCheckUser.Email}' in site '{site.Url}'. Total users to check: {users.Count}.");
            foreach (var user in users)
            {
                if (!user.IsDomainGroup)
                {
                    var hasPermission = CheckUserSitePermission(new List<IAveUser>() { user }, needCheckUser, objectModel, site);
                    if (hasPermission)
                    {
                        mLog.Info($"ValidateUserPermission: User '{needCheckUser.Email}' has direct permission through '{user.Email}'.");
                        return true;
                    }
                }
            }

            // Batch-check domain groups
            var domainGroupUsers = users.Where(u => u.IsDomainGroup).ToList();
            mLog.Info($"ValidateUserPermission: Checking domain groups for '{needCheckUser.Email}' in site '{site.Url}'. Total domainGroupUsers to check: {domainGroupUsers.Count}.");
            if (domainGroupUsers.Count > 0)
            {
                var hasDomainGroupPermission = CheckDomainGroupUsersPermission(domainGroupUsers, contract.NeedCheckedUserUPN, needCheckUser, objectModel, site);
                if (hasDomainGroupPermission)
                {
                    mLog.Info($"ValidateUserPermission: User '{needCheckUser.Email}' has permission through domain group membership.");
                    return true;
                }
            }

            return false;
        }

        private List<string> CheckUserMemberGroupsBatch(string userUpn, List<string> groupIds, RMGraphGroupManager groupManager)
        {
            try
            {
                if (groupIds == null || groupIds.Count == 0)
                {
                    return new List<string>();
                }
                
                var matchedGroupIds = groupManager.CheckUserMemberGroups(userUpn, groupIds).GetAwaiter().GetResult();
                return matchedGroupIds ?? new List<string>();
            }
            catch (Exception ex)
            {
                mLog.Warn($"CheckUserMemberGroupsBatch failed for user '{userUpn}'. Error: {ex}");
                return new List<string>();
            }
        }

        private bool CheckDomainGroupUsersPermission(List<IAveUser> domainGroupUsers, string userUpnOrEmail, UserDetail needCheckUser, AveObjectModelFactory objectModel, IAveSite site)
        {
            var securityGroupIds = new List<string>();
            var m365OwnerClaimUsers = new List<IAveUser>();

            foreach (var user in domainGroupUsers)
            {
                string groupId = GetGroupIdWithLoginName(user.LoginName);
                if (string.IsNullOrEmpty(groupId))
                {
                    mLog.Warn($"CheckDomainGroupUsersPermission: Cannot extract group ID from login name: {user.LoginName}");
                    continue;
                }

                // M365 Group owner claim (has _o suffix) - keep old logic
                if (groupId.EndsWith("_o", StringComparison.OrdinalIgnoreCase))
                {
                    m365OwnerClaimUsers.Add(user);
                    continue;
                }

                // Security group - validate GUID and add to batch
                if (Guid.TryParse(groupId, out var parsedGroupId))
                {
                    securityGroupIds.Add(parsedGroupId.ToString());
                }
                else
                {
                    mLog.Warn($"CheckDomainGroupUsersPermission: Group ID '{groupId}' is not a valid GUID for user '{user.Name}'.");
                }
            }

            // Batch-check security groups via checkMemberGroups API (20 per request)
            if (securityGroupIds.Count > 0)
            {
                mLog.Info($"CheckDomainGroupUsersPermission: Batch checking {securityGroupIds.Count} security group(s) for user '{userUpnOrEmail}'.");
                const int batchSize = 20; // limit per Graph API documentation

                // Get the app profile for Graph API authentication
                var appProfile = PoolUserUtil.GetBPOSInfoAsync(objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                if (appProfile == null)
                {
                    mLog.Warn($"CheckUserMemberGroupsBatch: Cannot find OPUS app, trying AOSP app. TenantId: {objectModel.AccountInfo.TenantId}");
                    appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, objectModel.AccountInfo.TenantId).GetAwaiter().GetResult();
                }

                if (appProfile == null)
                {
                    mLog.Error("CheckUserMemberGroupsBatch: No app profile found.");
                }
                else
                {
                    var groupManager = new RMGraphGroupManager(appProfile);

                    foreach (var batchGroupIds in securityGroupIds.Chunk(batchSize))
                    {
                        var matchedIds = CheckUserMemberGroupsBatch(userUpnOrEmail, batchGroupIds.ToList(), groupManager);
                        if (matchedIds.Count > 0)
                        {
                            mLog.Info($"CheckDomainGroupUsersPermission: User '{userUpnOrEmail}' found in security group(s): {string.Join(", ", matchedIds)}");
                            return true;
                        }
                    }
                }
            }

            // Keep old logic for handle M365 Group owner claims (should have no nesting risk)
            foreach (var ownerClaimUser in m365OwnerClaimUsers)
            {
                string groupId = GetGroupIdWithLoginName(ownerClaimUser.LoginName);
                if (!string.IsNullOrEmpty(groupId))
                {
                    mLog.Info($"CheckDomainGroupUsersPermission: Checking M365 Group owner claim '{ownerClaimUser.Name}'.");
                    var userDetails = GetOnlineMembersInGroup(groupId, ownerClaimUser.Name, objectModel);
                    var isMember = CheckUserSitePermission(userDetails, needCheckUser, objectModel, site);
                    if (isMember)
                    {
                        mLog.Info($"CheckDomainGroupUsersPermission: User found in M365 Group owner claim '{ownerClaimUser.Name}'.");
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
