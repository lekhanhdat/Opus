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
namespace AvePoint.StorageOptimization.Archiver.Service.Impl
{
    using AvePoint.Common.RemoteNode.Impl;
    using AvePoint.Cryptography;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
    using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
    using AvePoint.GCommon.Contract.SharePointBrowser;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.ObjectModel.Common;
    using AvePoint.RA.Browser.Handler;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Aos;
    using AvePoint.RA.Common.Cryptography;
    using AvePoint.RA.Common.SharePointBrowser;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.Contract;
    using AvePoint.RA.Contract.RMWeb.Setting;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Model;
    using AvePoint.RA.RACommonUtility.Browser;
    using AvePoint.RA.Service.Services;
    using AvePoint.RA.SharePoint.Common;
    using AvePoint.RAI.Core;
    using AvePoint.SharePointBrowser;
    using AvePoint.SharePointBrowser.Office365;
    using AvePoint.Wrapper.Common;
    using Cloud.Sdk.Data.Aos;
    using Google.Apis.PeopleService.v1.Data;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GroupOrTeamSitePermissionSetting = GCommon.Contract.Server.EndUserRestoreSetting.GroupOrTeamSitePermissionSetting;

    public class ArchiverService : RMServiceBase, IArchiverService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ArchiverService));
        private IEndUserRestoreSettingService EndUserSetting => PlatformWindsorManager.GetService<IEndUserRestoreSettingService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public IRemoteO365AccountService RemoteO365AccountService { get; set; }
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        public IRemoteNodeService remoteNodeService { get => new RemoteNodeService(); set { } }
        private IRMRestoreSiteMappingDao RestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        public IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private Tuple<string,string> AppIdAndAdminUrlForAOSP { get; set; }

        /// <summary>
        /// 1.当前接口用于SharePointSite
        /// 2.当前接口用于StubRestoreLink
        /// </summary>
        public Result CheckUserPermission(RemoteSiteCollection remoteSiteCollection, BposInfo bposInfo, CheckPermissionAction checkAction, string userMail, string teamID, string specifiedGroupNameForSPSite, string fileUrl = "",string stubType="",bool useSiteMapping = false)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var contract = new Office365MessageContract();
            contract.SiteCollectionUrl = remoteSiteCollection.url;
            contract.SitesGroupId = TenantThreadLocalValue.LogonGroupId;
            contract.FileFullUrl = Decrypt(fileUrl);
            contract.BPOSInfo = bposInfo;
            if (contract.BPOSInfo == null)
            {
                contract.BPOSInfo = new BposInfo();
            }
            if (!string.IsNullOrEmpty(stubType))
            {
                contract.StubType = stubType;
            }
            else
            {
                logger.Warn("stubType not exsit when check user permission");
            }
            contract.NeedCheckedUserMail = userMail;
            contract.CheckPermissionAction = checkAction;
            contract.NeedCheckedGroupId = teamID;
            contract.SpecifiedGroupNameForSharePointSite = specifiedGroupNameForSPSite;
            contract.UseSiteMapping = useSiteMapping;
            BrowserMessage message = new BrowserMessage() { MsgType = GCommon.Contract.Common.MessageType.CheckUserHasPermission, TenantGroupId = TenantLocalValue.LogonGroupId };
            message.MessageContract = contract;
            RABrowserContract mContract = new RABrowserContract(JsonConvert.SerializeObject(message),BrowserType.CheckEndUserPermission, contract.NeedCheckedUserMail, TenantLocalValue.LogonGroupId, contract.TenantId);
            //var result = RABrowserUtil.Instance.SendBrowseMessage<BrowserMessage>(mContract);
            try
            {
                var accountInfo = GetBPOSBySiteUrlAsync(remoteSiteCollection).GetAwaiter().GetResult();
                var factory = new AveClientObjectModelFactory(remoteSiteCollection.url, accountInfo);
                WrapperRuntime.CurrentContext.ModelFactory = factory;
                var handler = new Office365BrowserMessageHandler();
                sw.Stop();
                logger.Info($"linkRestoreReport before check permission cost time:{sw.ElapsedMilliseconds}");
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();
                var result = handler.HandleMessage(message.AgentInfo, message, factory);
                sw2.Stop();
                logger.Info($"linkRestoreReport handler.HandleMessage cost time:{sw2.ElapsedMilliseconds}");
                //var result = BrowseInvoker.SendBrowseMessage(message);
                if (result == null || result.BrowserContract == null)
                {
                    logger.Warn("Validation result is null.");
                    return new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown };
                }
                return (result.MessageContract).Result;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while handle message.Error: {e.ToString()}");
                var errorResult = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown };
                if (e.Message.Equals("RM_JM_AppProfile_NotFoundError"))
                {
                    errorResult.ErrorDetail = "RM_JM_AppProfile_NotFoundError";
                    errorResult.ErrorInfo = ErrorInfo.ActiveAppProfileNotFound;
                }
                return errorResult;
            }
        }
        private async Task<AveBPOSAccountInfo> GetBPOSBySiteUrlAsync(RemoteSiteCollection remoteSiteCollection)
        {
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            if (bposInfo == null || !bposInfo.ExsitAppProfile)
            {
                //try find aosp app
                bposInfo = PoolUserUtil.GetAOSPBPOSInfo(remoteSiteCollection.TenantId);
            }
            return bposInfo;
        }
        private string Decrypt(string pwd)
        {
            try
            {
                return Encoding.UTF8.GetString(CspCommunicationWrapper.UnWrapKey(pwd));
            }
            catch (Exception ex)
            {
                logger.Error("Decrypt pwd error: {0}", ex.ToString());
                return pwd;
            }
        }
        public Result CheckUserPermissionForGroupOrTeamSite(RemoteSiteCollection remoteSiteCollection, BposInfo bposInfo, string userMail, string groupId)
        {
            var contract = new Office365MessageContract();
            contract.SitesGroupId = TenantThreadLocalValue.LogonGroupId;
            contract.SiteCollectionUrl = remoteSiteCollection.url;
            contract.BPOSInfo = bposInfo;
            if (contract.BPOSInfo == null)
            {
                contract.BPOSInfo = new BposInfo();
            }
            contract.NeedCheckedUserMail = userMail;
            var endUserResSetting = EndUserSetting.GetEndUserRestoreSetting();
            if (endUserResSetting == null || endUserResSetting.PermissionSetting == null)
            {
                logger.Warn("the EndUserRestoreSetting is null,please check the field in ProfileTable");
                contract.CheckPermissionAction = CheckPermissionAction.GroupOwner;
            }
            else if (endUserResSetting.PermissionSetting?.TeamsAndGroup == GroupOrTeamSitePermissionSetting.OwnerOrMembler)
            {
                contract.CheckPermissionAction = CheckPermissionAction.GroupOwnerOrMember;
            }
            else
            {
                contract.CheckPermissionAction = CheckPermissionAction.GroupOwner;
            }
            logger.Info($"EndUserRestoreSetting PermissionSetting GroupOrTeamSitePermissionSetting is:{contract.CheckPermissionAction}.");
            contract.NeedCheckedGroupId = groupId;
            BrowserMessage message = new BrowserMessage() { MsgType = GCommon.Contract.Common.MessageType.CheckUserHasPermission, TenantGroupId = TenantLocalValue.LogonGroupId };
            message.MessageContract = contract;
            RABrowserContract mContract = new RABrowserContract(JsonConvert.SerializeObject(message), BrowserType.CheckEndUserPermission, contract.NeedCheckedUserMail, TenantLocalValue.LogonGroupId, contract.TenantId);
            //var result = RABrowserUtil.Instance.SendBrowseMessage<BrowserMessage>(mContract);
            try
            {
                var accountInfo = GetBPOSBySiteUrlAsync(remoteSiteCollection).GetAwaiter().GetResult();
                var factory = new AveClientObjectModelFactory(remoteSiteCollection.url, accountInfo);
                WrapperRuntime.CurrentContext.ModelFactory = factory;
                var handler = new Office365BrowserMessageHandler();
                var result = handler.HandleMessage(message.AgentInfo, message, factory);
                //var result = BrowseInvoker.SendBrowseMessage(message);
                if (result == null || result.BrowserContract == null)
                {
                    logger.Warn("Validation result is null.");
                    return new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown };
                }
                return (result.MessageContract).Result;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while handle message.Error: {e.ToString()}");
                var errorResult = new Result() { Status = false, ErrorInfo = GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown };
                if (e.Message.Equals("RM_JM_AppProfile_NotFoundError"))
                {
                    errorResult.ErrorDetail = "RM_JM_AppProfile_NotFoundError";
                    errorResult.ErrorInfo = ErrorInfo.ActiveAppProfileNotFound;
                }
                return errorResult;
            }
        }

        #region Run Job Method for RevIM

        #endregion


        public async Task<ArchiverStubLink> ParseStubStringAsync(string stubString)
        {
            logger.Info($"Out Log For Debug decode stubString:{stubString}");
            string specicalString = "+--force-renderer-accessibility";
            if (stubString.IndexOf(specicalString) > 0)
            {
                stubString = stubString.Substring(0, stubString.IndexOf(specicalString));
                logger.Info($"ParseStubString contains force-renderer-accessibility.NewStubString:{stubString}");
            }
            ArchiverStubLink stubLink = null;
            var masterKeys = await SettingProfileDao.LoadAllByTypeAsync((int)SettingProfilesType.EndUserStubLinkMasterKey);
            if (masterKeys == null || !masterKeys.Any())
            {
                logger.Error("The EndUserRestoreMasterKey count is 0.");
                return null;
            }
            else
            {
                logger.Info($"The EndUserRestoreMasterKey count is {masterKeys.Count}.");
            }

            byte[] inPut;
            try
            {
                inPut = Convert.FromBase64String(stubString);
            }
            catch (Exception ex)
            {
                logger.Error("Parse stub string failed because stubString is not valid Base64. Error: {0}", ex.ToString());
                return null;
            }

            StubLinkDetails ld = null;
            for (int keyIndex = 0; keyIndex < masterKeys.Count; keyIndex++)
            {
                try
                {
                    ld = ParseStubLinkDetails(masterKeys[keyIndex]?.Settings, inPut);
                    logger.Info($"Parse stub string successfully with EndUserRestoreMasterKey index {keyIndex + 1}.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Parse stub string failed with EndUserRestoreMasterKey index {keyIndex + 1}. Error: {ex}");
                }
            }

            if (ld == null)
            {
                logger.Error("Parse stub string failed with all EndUserRestoreMasterKeys.");
                return null;
            }

            stubLink = ConvertStubLink2ArchiverStubLink(ld);
            logger.Info($"Parse Stub string successfully {stubLink.PathMD5}. Will get Archive History by url {stubLink.SiteUrl} and id {stubLink.JobID}");
            var index = await ArchiverSiteMasterIndexService.GetLatestSiteCollectionNodeInfoByUrlAsync(stubLink.SiteUrl);
            if(index == null)
            {
                logger.Info($"unable found index record, sc:{stubLink.SiteUrl}");
                var needCheckSCs = await RestoreSiteMappingDao.GetSourceSCUrlsByTargetSCUrlAsync(stubLink.SiteUrl);
                foreach (var sc in needCheckSCs)
                {
                    logger.Info($"try found index record, sc:{sc}");
                    index = await ArchiverSiteMasterIndexService.GetLatestSiteCollectionNodeInfoByUrlAsync(sc);
                    if (index != null)
                    {
                        logger.Info($"Find the site by site mapping.url:{sc}");
                        break;
                    }
                }
            }

            if (index == null)
            {
                logger.Error("Can not find the job index by url.");
                stubLink.HasArchiveHistory = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(stubLink.JobID))
                {
                    ArchiverIndexSubInfo subInfo = null;
                    logger.Info($"Get ArchiveHistory by id {stubLink.JobID}");
                    if (stubLink.JobID.Contains("_"))
                    {
                        subInfo = await ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(stubLink.JobID);
                    }
                    else
                    {
                        subInfo = await ArchiverIndexSubInfoDao.GetSubInfoByJobIdAsync(stubLink.JobID);
                    }
                    if (subInfo == null)
                    {
                        logger.Error("Can not find the job index by id.");
                        stubLink.HasArchiveHistory = false;
                    }
                    else
                    {
                        if (subInfo.DeletedStatus != (int)RA.Contract.RMWeb.JobMonitor.DeletedStatus.Normal)
                        {
                            logger.Error($"this job has soft,id:{stubLink.JobID}");
                            stubLink.HasArchiveHistory = false;
                        }
                    }
                }
            }
            logger.Info($"Get ArchiveHistory successfully. {stubLink.JobID}");
            return stubLink;
        }

        private ArchiverStubLink ConvertStubLink2ArchiverStubLink(StubLinkDetails ld)
        {
            ArchiverStubLink link = new ArchiverStubLink();
            link.FileServerRelativeUrl = ld.FileServerRelativeUrl;
            link.JobID = ld.JobID;
            link.PathMD5 = ld.PathMD5;
            link.SiteUrl = ld.SiteUrl;
            link.TenantID = ld.TenantID;
            link.User = ld.User;
            link.HasArchiveHistory = true;
            link.StubType = ld.StubType;
            link.StubId = ld.StubId;
            link.FileSize = ld.FileSize;
            link.StubProductSource = ld.StubProductSource;
            return link;
        }

        private StubLinkDetails ParseStubLinkDetails(string masterKeyString, byte[] inPut)
        {
            if (string.IsNullOrEmpty(masterKeyString))
            {
                throw new ArgumentException("Master key is empty.", nameof(masterKeyString));
            }

            var decrypBytes = AuthenticatedEncryption.Decrypt(Convert.FromBase64String(masterKeyString), inPut);
            byte[] decompressedData = new byte[0];
            byte[] temp = new byte[4096];
            using (var mso = new MemoryStream(decrypBytes))
            {
                using (var gs = new GZipStream(mso, CompressionMode.Decompress))
                {
                    int readLen;
                    while ((readLen = gs.Read(temp, 0, 4096)) != 0)
                    {
                        AppendBytes(ref decompressedData, temp, 0, readLen);
                    }
                }
            }

            string decrypString = Encoding.UTF8.GetString(decompressedData);
            return new StubLinkDetails(decrypString);
        }

        private void AppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }

 

        public EndUserRestoreSettingUIDto GetEndUserRestoreSetting()
        {
            var endUserSetting = EndUserSetting.GetEndUserRestoreSetting();
            if (endUserSetting == null)
            {
                endUserSetting = new EndUserRestoreSettingUIDto()
                {
                    IsAllowRestore = true,
                    IsCustomizeStubRestorePage = false,
                    IsRestoreArchivedTier = false,
                    PermissionSetting = new GCommon.Contract.Server.EndUserRestoreSetting.EndUserPermissionSetting() { IsRestoreStubLink = false, IsRestoreSiteCollection = false, IsRestoreGroupTeamSite = false },
                };
            }
            else if (endUserSetting.PermissionSetting == null)
            {
                endUserSetting.PermissionSetting = new GCommon.Contract.Server.EndUserRestoreSetting.EndUserPermissionSetting();
            }
            return endUserSetting;
        }
        public Tuple<string, string> GetAOSPAppIdForRestore(string m365tenantId)
        {
            try
            {
                //try find aosp app
                var bposInfo = PoolUserUtil.GetAOSPBPOSInfo(m365tenantId);
                AppIdAndAdminUrlForAOSP = new Tuple<string, string>(bposInfo?.Id, bposInfo?.AdminUrl);
                return AppIdAndAdminUrlForAOSP;
            }
            catch (Exception e)
            {
                logger.Error($"GetAOSPAppIdForRestore failed,error:{e}");
                return null;
            }
        }
        public bool IsExportSizeReachLimited()
        {
            //var profile = ProfileDao.GetByType((int)ProfileType.DataSizeAccumulate).FirstOrDefault();
            //if (profile == null)
            //{
            //    return false;
            //}
            //else
            //{
            //    if (Convert.ToInt32(profile.Description) == DateTime.UtcNow.Month)
            //    {
            //        return Convert.ToInt64(profile.Extension) >= (long)100 * 1024 * 1024 * 1024;//100GB
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            return false;//delete this
        }

        private class StubLinkDetails
        {
            private readonly string SplitChar = "|#|";
            public string TenantID { get; }
            public string SiteUrl { get; }
            public string FileServerRelativeUrl { get; }
            public string PathMD5 { get; }
            public string JobID { get; }
            public string User { get; }
            public string StubType { get; }
            public string StubId { get; }
            public string FileSize { get; }
            public StubProductSource StubProductSource { get; }

            public StubLinkDetails(string tenantID, string siteUrl, string fileServerRelativeUrl, string pathMD5, string jobID, string user,string stubType)
            {
                this.TenantID = tenantID;
                this.SiteUrl = siteUrl;
                this.FileServerRelativeUrl = fileServerRelativeUrl;
                this.PathMD5 = pathMD5;
                this.JobID = jobID;
                this.User = user;
                this.StubType = stubType;
            }

            public StubLinkDetails(string hybridString)
            {
                var array = hybridString.Split(new string[] { SplitChar }, StringSplitOptions.None);
                this.TenantID = array[0];
                this.SiteUrl = array[1];
                this.FileServerRelativeUrl = array[2];
                this.PathMD5 = array[3];
                this.JobID = array[4];
                this.User = array[5];
                if (array.Length == 6)
                {
                    logger.Warn($"StubLinkDetails Length is 6 which belong to old archiver stub.hybridString:{hybridString}.");
                }
                //array.Length == 8 is WPP new stub.
                else if (array.Length == 7 || array.Length == 8)
                {
                    this.StubType = array[6];
                }
                else if (array.Length == 9)
                {
                    this.StubType = array[6];
                    this.StubId = array[7];
                    this.FileSize = array[8];
                }
                else if (array.Length == 10)
                {
                    this.StubType = array[6];
                    this.StubId = array[7];
                    this.FileSize = array[8];
                    this.StubProductSource = (StubProductSource)Convert.ToUInt32(array[9]);
                }
                else
                {
                    logger.Error($"StubLinkDetails error Length.hybridString:{hybridString}.");
                    throw new Exception("Invalid data format.");
                }
            }

            public override string ToString()
            {
                return $"{TenantID}{SplitChar}{SiteUrl}{SplitChar}{FileServerRelativeUrl}{SplitChar}{PathMD5}{SplitChar}{JobID}{SplitChar}{User}{SplitChar}{StubType}";
            }
        }

        #region Permission


        public SOReturnMessage CheckPermissionForStubRestoreLink(RemoteSiteCollection site, string fileUrl, string userMail, string stubType,bool useSiteMapping = false)
        {
            logger.Info("Check item open permission.");
            SOReturnMessage returnMessage = new SOReturnMessage();

            try
            {
                var bposInfo = new BposInfo();
                //bposInfo.ResetBposInfo(TenantThreadLocalValue.LogonGroupId, site.TenantId);
                //if (string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.Username) &&
                //    string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.AppId))
                //{
                //    logger.Warn("CheckPermissionForStubRestoreLink. EmptyCredential");
                //    returnMessage.FailedType = FailedType.EmptyCredential;
                //    returnMessage.MessageType = SOMessageType.Failed;
                //    return returnMessage;
                //}
                var hasPermission = CheckUserPermission(site, bposInfo, CheckPermissionAction.ArchiverStubFile, userMail, site.ObjectId, string.Empty, fileUrl, stubType, useSiteMapping);
                if (hasPermission.SiteCollectionType == SiteCollectionType.Teams)
                {
                    returnMessage.SiteCollectionType = NodeType.O365TeamSites;
                }
                else if (hasPermission.SiteCollectionType == SiteCollectionType.Group)
                {
                    returnMessage.SiteCollectionType = NodeType.O365GroupSites;
                }
                else { }
                if (hasPermission.Status)
                {
                    returnMessage.MessageType = SOMessageType.Successful;
                }
                else
                {
                    returnMessage.MessageType = SOMessageType.Failed;
                    if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException)
                    {
                        logger.Warn($"Check item open permission. SecurityTrimingException {userMail}");
                        returnMessage.FailedType = FailedType.SecurityTrimingException;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound)
                    {
                        logger.Warn($"Check item open permission. NodeNotExisting {userMail}");
                        returnMessage.FailedType = FailedType.SecurityTrimingException;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserCannotFound || hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.InsufficientPrivileges)
                    {
                        logger.Warn($"Check item open permission. UserCannotFound|InsufficientPrivileges {userMail}");
                        returnMessage.FailedType = FailedType.InsufficientPrivilegesForStub;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked)
                    {
                        logger.Warn($"Check item open permission. SiteCollectionLocked {userMail}");
                        returnMessage.FailedType = FailedType.SiteCollectionLocked;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown)
                    {
                        logger.Warn("Check item open permission. UnknownError");
                        returnMessage.FailedType = FailedType.None;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.PermissionError)
                    {
                        logger.Warn("Check item open permission. PermissionError");
                        returnMessage.FailedType = FailedType.PermissionError;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.OopStubNotFound)
                    {
                        logger.Warn("Check item open permission. OopStubNotFound");
                        returnMessage.FailedType = FailedType.StubFileNotExsit;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.ActiveAppProfileNotFound)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. ActiveAppProfileNotFound");
                        returnMessage.FailedType = FailedType.ActiveAppProfileNotFound;
                    }
                    else
                    {
                        logger.Warn($"Check item open permission. InsufficientPrivilegesForStub {userMail}");
                        returnMessage.FailedType = FailedType.InsufficientPrivilegesForStub;
                    }
                }

            }
            catch (Exception e)
            {
                logger.Warn($"CheckForItemOpenPermisson {userMail} error: {e.ToString()}");
                returnMessage.MessageType = SOMessageType.Failed;
                returnMessage.FailedType = FailedType.PermissionError;
            }
            logger.Info("end check item open permission.");
            return returnMessage;
        }

        public SOReturnMessage CheckPermissionForSharePointSite(RemoteSiteCollection site, string userMail)
        {
            logger.Info("Check Permission For SharePoint Site.");
            SOReturnMessage returnMessage = new SOReturnMessage();
            try
            {
                var spSiteCheckPermissionAction = CheckPermissionAction.SiteOwner;
                EndUserRestoreSettingUIDto endUserResSetting = EndUserSetting.GetEndUserRestoreSetting();
                if (endUserResSetting == null || endUserResSetting.PermissionSetting == null)
                {
                    if (endUserResSetting == null)
                    {
                        endUserResSetting = new EndUserRestoreSettingUIDto() { PermissionSetting = new GCommon.Contract.Server.EndUserRestoreSetting.EndUserPermissionSetting() };
                    }
                    if (endUserResSetting.PermissionSetting == null)
                    {
                        endUserResSetting.PermissionSetting = new GCommon.Contract.Server.EndUserRestoreSetting.EndUserPermissionSetting();
                    }
                    logger.Warn("the EndUserRestoreSetting is null,please check the field in ProfileTable when CheckPermissionForSharePointSite");
                    spSiteCheckPermissionAction = CheckPermissionAction.SiteOwner;
                }
                logger.Info($"EndUserRestoreSetting PermissionSetting SharePointSitePermissionSetting is:{endUserResSetting.PermissionSetting.SiteCollection}.");
                switch (endUserResSetting.PermissionSetting.SiteCollection)
                {
                    case GCommon.Contract.Server.EndUserRestoreSetting.SharePointSitePermissionSetting.SiteOwner:
                        spSiteCheckPermissionAction = CheckPermissionAction.SiteOwner;
                        break;
                    case GCommon.Contract.Server.EndUserRestoreSetting.SharePointSitePermissionSetting.SiteOwnerAndSiteMemberGroup:
                        spSiteCheckPermissionAction = CheckPermissionAction.SiteOwnerOrSiteMember;
                        break;
                    case GCommon.Contract.Server.EndUserRestoreSetting.SharePointSitePermissionSetting.SiteOwnerAndSpecialGroup:
                        spSiteCheckPermissionAction = CheckPermissionAction.SiteOwnerOrSpecialGroup;
                        break;
                    case GCommon.Contract.Server.EndUserRestoreSetting.SharePointSitePermissionSetting.SiteOwnerAndSiteMemberGroupAndSiteVisitor:
                        spSiteCheckPermissionAction = CheckPermissionAction.SiteOwnerOrSiteMemberGroupOrSiteVisitor;
                        break;
                    default:
                        spSiteCheckPermissionAction = CheckPermissionAction.SiteOwner;
                        break;
                }
                var bposInfo = new BposInfo();
                //bposInfo.ResetBposInfo(TenantThreadLocalValue.LogonGroupId, site.TenantId);
                //if (string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.Username) &&
                //    string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.AppId))
                //{
                //    logger.Warn("CheckPermissionForSharePointSite. EmptyCredential");
                //    returnMessage.FailedType = FailedType.EmptyCredential;
                //    returnMessage.MessageType = SOMessageType.Failed;
                //    return returnMessage;
                //}
                var hasPermission = CheckUserPermission(site, bposInfo, spSiteCheckPermissionAction, userMail, site.ObjectId, endUserResSetting.PermissionSetting.SiteCollectionSpecialGroupNames);
                returnMessage.IsReadOnlySite = hasPermission.IsReadOnlySite;
                if (hasPermission.SiteCollectionType == SiteCollectionType.Teams)
                {
                    returnMessage.SiteCollectionType = NodeType.O365TeamSites;
                }
                else if (hasPermission.SiteCollectionType == SiteCollectionType.Group)
                {
                    returnMessage.SiteCollectionType = NodeType.O365GroupSites;
                }
                if (hasPermission.Status)
                {
                    returnMessage.MessageType = SOMessageType.Successful;
                    returnMessage.SiteTitle = hasPermission.Title;
                }
                else
                {
                    returnMessage.MessageType = SOMessageType.Failed;
                    if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.SecurityTrimingException)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. SecurityTrimingException");
                        returnMessage.FailedType = FailedType.SecurityTrimingException;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. NodeNotExisting");
                        returnMessage.FailedType = FailedType.SecurityTrimingException;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserCannotFound || hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.InsufficientPrivileges)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UserCannotFound|InsufficientPrivileges");
                        returnMessage.FailedType = FailedType.InsufficientPrivilegesForSite;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.SiteCollectionLocked)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. SiteCollectionLocked");
                        returnMessage.FailedType = FailedType.SiteCollectionLocked;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserNotOwnerForSharePointSite)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UserNotOwnerForSharePointSite");
                        returnMessage.FailedType = FailedType.UserNotOwnerForSharePointSite;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserNotOwnerOrMemberForSharePointSite)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UserNotOwnerOrMemberForSharePointSite");
                        returnMessage.FailedType = FailedType.UserNotOwnerOrMemberForSharePointSite;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserNotOwnerOrMemberOrVisitorForSharePointSite)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UserNotOwnerOrMemberOrVisitorForSharePointSite");
                        returnMessage.FailedType = FailedType.UserNotOwnerOrMemberOrVisitorForSharePointSite;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserNotOwnerOrSpecifiedGroupForSharePointSite)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UserNotOwnerOrSpecifiedGroupForSharePointSite");
                        returnMessage.FailedType = FailedType.UserNotOwnerOrSpecifiedGroupForSharePointSite;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.ActiveAppProfileNotFound)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. ActiveAppProfileNotFound");
                        returnMessage.FailedType = FailedType.ActiveAppProfileNotFound;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown)
                    {
                        logger.Warn("CheckPermissionForSharePointSite. UnknownError");
                        returnMessage.FailedType = FailedType.None;
                    }
                    else
                    {
                        logger.Warn("CheckPermissionForSharePointSite. InsufficientPrivilegesForSite");
                        returnMessage.FailedType = FailedType.InsufficientPrivilegesForSite;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"CheckPermissionForSharePointSite : {e.ToString()}");
            }
            logger.Info("end Check Permission For SharePoint Site.");
            return returnMessage;
        }


        public SOReturnMessage CheckPermissionForGroupOrTeamSite(RemoteSiteCollection site, string groupId, string userMail)
        {
            logger.Info("Check Permission For Group Or Team Site.");
            SOReturnMessage returnMessage = new SOReturnMessage();
            try
            {
                var bposInfo = new BposInfo();
                //bposInfo.ResetBposInfo(TenantThreadLocalValue.LogonGroupId, site.TenantId);
                //if (string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.Username) &&
                //    string.IsNullOrEmpty(bposInfo?.UserAccountInfo?.AppId))
                //{
                //    logger.Warn("CheckPermissionForGroupOrTeamSite. EmptyCredential");
                //    returnMessage.FailedType = FailedType.EmptyCredential;
                //    returnMessage.MessageType = SOMessageType.Failed;
                //    return returnMessage;
                //}
                var hasPermission = CheckUserPermissionForGroupOrTeamSite(site, bposInfo, userMail, groupId);
                if (hasPermission.SiteCollectionType == SiteCollectionType.Teams)
                {
                    returnMessage.SiteCollectionType = NodeType.O365TeamSites;
                }
                else
                {
                    returnMessage.SiteCollectionType = NodeType.O365GroupSites;
                }
                if (hasPermission.Status)
                {
                    returnMessage.MessageType = SOMessageType.Successful;
                    returnMessage.SiteTitle = hasPermission.Title;
                }
                else
                {
                    returnMessage.MessageType = SOMessageType.Failed;
                    if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.InsufficientPrivileges)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. InsufficientPrivileges");
                        returnMessage.FailedType = FailedType.UserNotGroupOwner;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserCannotFound)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. UserCannotFound");
                        returnMessage.FailedType = FailedType.UserNotGroupOwner;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.UserNotGroupOwnerOrMember)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. UserNotGroupOwnerOrMember");
                        returnMessage.FailedType = FailedType.UserNotGroupOwnerOrMember;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.NotFound)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. Request_ResourceNotFound");
                        returnMessage.FailedType = FailedType.RequestResourceNotFound;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.ActiveAppProfileNotFound)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. ActiveAppProfileNotFound");
                        returnMessage.FailedType = FailedType.ActiveAppProfileNotFound;
                    }
                    else if (hasPermission.ErrorInfo == GCommon.Contract.SharePointBrowser.ErrorInfo.Unknown)
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. UnknownError");
                        returnMessage.FailedType = FailedType.None;
                    }
                    else
                    {
                        logger.Warn("CheckPermissionForGroupOrTeamSite. None");
                        returnMessage.FailedType = FailedType.None;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"CheckPermissionForGroupOrTeamSite : {e.ToString()}");
                returnMessage.MessageType = SOMessageType.Failed;
            }
            logger.Info("end Check Permission For Group Or Team Site.");
            return returnMessage;
        }
        #endregion

    }
}
