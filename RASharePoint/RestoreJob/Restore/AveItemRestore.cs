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



using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Item.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.ActionOnly.SPActionOnly;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource;
using AvePoint.Wrapper.Restore;
using Cloud.Sdk.Data.AosModern;
using HSMAzureCommon;
using LS.SPWorkflowProcessor;
using Media.Service.ArchiverBackup.Restore;
using Microsoft365.Authentication;
using RAArchiverCommon;
using RAArchiverCommon.TeamsController;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using static Org.BouncyCastle.Math.EC.ECCurve;
using ItemDependencyOption = AvePoint.GCommon.Contract.Server.GranularRestore.Object.ItemDependencyOption;

namespace AvePoint.Item.Restore
{
    [AveCodeReview("2012/06/15", "Qinglong.luo@avepoint.com", "Fxcop Rule", new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1 }, null, true)]
    public class AveItemRestore : AveRestoreBase
    {
        protected AveListSettingInfo listSettingInfo;
        protected AveSPFolder aveListRootFolder;
        protected AveSPFolder aveFolder;
        public AveSPSite AveSite;
        public AveSPWeb AveWeb;
        public AveSPList AveList;
        public AveSPAppManager appManager;
        protected string mListPath;
        private bool hasCheckServiceAccountForSensitivityLabel = false;
        private IAveRequest mServiceAccountRequestForSensitivityLabel;
        private bool hasCheckAppProfileForSensitivityLabel = false;
        private IAveRequest mAppProfileRequestForSensitivityLabel;
        private readonly object mLock = new object();
        private Dictionary<string,string> mContainerName = new Dictionary<string,string>();
        private int mDesMaxVersionBeforeAdd; // to save item's max version in destination before do any changes.
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private string mDesTempItemName = string.Empty;
        private int mNewCreatedLCD = -1;
        private string sourceSiteUrl;
        protected string targetSiteUrl;
        #region Restore to SPO
        protected string targetWebUrl;
        protected string targetListUrl;
        protected string targetFolderUrl;
        protected string sourceLibUrl;
        protected string sourceFolderUrl;
        protected string lastSelectedFolderUrl;
        //protected bool isParentFolderProcessed;
        protected bool isSelectedFolderProcessed;
        protected bool isSendVirtualReport;
        #endregion
        protected IAveORecords Record
        {
            get
            {
                IAveORecords records = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.Auto).CreateRecords();
                return records;
            }
        }

        #region Cache for stub deleting when restore with site mapping
        public AveSPSite oriAveSite;
        public AveSPWeb oriAveWeb;
        protected bool isOriginalSiteExist = false;
        #endregion

        private bool _isHandledChannelSiteDefaultLib = false;

        private IAveRequest ServiceAccountRequestForSensitivityLabel
        {
            get
            {
                lock (mLock)
                {
                    try
                    {
                        if (mServiceAccountRequestForSensitivityLabel != null)
                        {
                            return mServiceAccountRequestForSensitivityLabel;
                        }
                        if (hasCheckServiceAccountForSensitivityLabel)
                        {
                            return mServiceAccountRequestForSensitivityLabel = null;
                        }
                        AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig.GetServiceAccount();
                        if (siteAccount == null)
                        {
                            log.Info("[SensitivityLabel]Service account is null.");
                            mServiceAccountRequestForSensitivityLabel = null;
                        }
                        else
                        {
                            log.Info($"[SensitivityLabel]Service account is {siteAccount.UserName}.");
                            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, siteAccount, AveContextKind.ClientObjectModel);
                            var site = factory.CreateSite(AveSite.SiteUrl);
                            mServiceAccountRequestForSensitivityLabel = site.Request;
                        }
                        hasCheckServiceAccountForSensitivityLabel = true;
                        return mServiceAccountRequestForSensitivityLabel;
                    }
                    catch (Exception e)
                    {
                        hasCheckServiceAccountForSensitivityLabel = true;
                        mServiceAccountRequestForSensitivityLabel = null;
                        log.Info($"Get service account failed {e.ToString()}");
                        return null;
                    }
                }
            }
        }

        private IAveRequest AppProfileRequestForSensitivityLabel
        {
            get
            {
                lock (mLock)
                {
                    try
                    {
                        if (mAppProfileRequestForSensitivityLabel != null)
                        {
                            return mAppProfileRequestForSensitivityLabel;
                        }
                        if (hasCheckAppProfileForSensitivityLabel)
                        {
                            return mAppProfileRequestForSensitivityLabel = null;
                        }
                        Cloud.Sdk.Data.AosModern.AppProfileInfo app = ItemRestoreConfig.BPOSSiteCollectionConfig.GetAppProfileForSensitivityLabel();
                        if (app == null)
                        {
                            log.Info("[SensitivityLabel]AppProfile is null.");
                            mAppProfileRequestForSensitivityLabel = null;
                            throw new Exception("app is null");
                        }
                        var clientId = app.AppClientId;
                        var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(app.TenantId).GetAwaiter().GetResult().AdminUrl;
                        var accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                        {
                            TenantId = app.TenantId,
                            AdminUrl = adminUrl,
                            ClientId = clientId,
                            ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                            TenantGroupId = TenantLocalValue.LogonGroupId,
                            AppType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp,
                            AuthenticationProfileId = app.Id,
                            AADEnvironment = (AveAzureEnvironment)app.AADEnvironment,
                            //AppCert = apponlyCertificate
                        };
                        log.Info($"[SensitivityLabel]AppProfile is {app.Name}.");
                        AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, accountInfo, AveContextKind.ClientObjectModel);
                        var site = factory.CreateSite(AveSite.SiteUrl);
                        mAppProfileRequestForSensitivityLabel = site.Request;
                        mAppProfileRequestForSensitivityLabel.InitMIPService(app.TenantId, null, Util.MIP.Cloud.Commercial);

                        hasCheckAppProfileForSensitivityLabel = true;
                        return mAppProfileRequestForSensitivityLabel;
                    }
                    catch (Exception e)
                    {
                        hasCheckAppProfileForSensitivityLabel = true;
                        mAppProfileRequestForSensitivityLabel = null;
                        log.Info($"Get AppProfile failed {e.ToString()}");
                        return null;
                    }
                }
            }
        }

        private IAveTenant tenant;

        private IAveListItem ResolveArchivedCheckItem(IAveListItem restoredItem, string realName)
        {
            if (restoredItem != null)
            {
                return restoredItem;
            }

            try
            {
                if (AveWeb?.SPWeb == null || aveFolder?.SPFolder == null || string.IsNullOrWhiteSpace(realName))
                {
                    return null;
                }

                var fileUrl = $"{aveFolder.SPFolder.ServerRelativeUrl.TrimEnd('/')}/{realName}";
                var file = AveWeb.SPWeb.GetFile(fileUrl);
                if (file?.Exists == true && file.Item != null)
                {
                    return file.Item;
                }
            }
            catch (Exception ex)
            {
                log.Warn($"ResolveArchivedCheckItem failed. realName:{realName}, error:{ex}");
            }

            return null;
        }

        public override void Init()
        {
            base.Init();
            ReplaceType = Config.DestinationInfo.ReplaceType;
        }

        public override void PostProcess()
        {
            base.PostProcess();
            LastPostAction(AveSite, AveWeb, AveList);
            DisposeWeb();
            DisposeSite();
            LinkFileCommon.FlushStubFileRecordCache();
        }
        private GCommon.Contract.CentralAdmin.Object.AppType ConvertIdentityTypeToAppType(IdentityProviderType providerType)
        {
            return providerType switch
            {
                IdentityProviderType.Office365 => GCommon.Contract.CentralAdmin.Object.AppType.Office365,
                IdentityProviderType.SharePoint => GCommon.Contract.CentralAdmin.Object.AppType.SharePoint,
                IdentityProviderType.Exchange => GCommon.Contract.CentralAdmin.Object.AppType.Exchange,
                IdentityProviderType.CustomAzureApp => GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp,
                IdentityProviderType.CustomDelegateApp => GCommon.Contract.CentralAdmin.Object.AppType.CustomDelegateApp,
                IdentityProviderType.CloudRecords => GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords,
                _ => GCommon.Contract.CentralAdmin.Object.AppType.Office365,
            };
        }
        private AveBPOSAccountInfo GetBPOSAccountInfo(RestoreContentDto aveSiteDto)
        {
            AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
            try
            {
                log.Info("GetBPOSAccountInfo Name is:{0},siteAccount is null:{1},SiteURL:{2}.", aveSiteDto.Name, siteAccount == null, aveSiteDto.SrcUrl);
                if (siteAccount == null)
                {
                    RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(aveSiteDto.SrcUrl);
                    if (remoteSiteCollection != null && !string.IsNullOrEmpty(remoteSiteCollection.TenantId))
                    {
                        log.Info($"GetBPOSAccountInfo remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.");
                        siteAccount = PoolUserUtil.GetBPOSInfo2Async(remoteSiteCollection).Result;
                        log.Info($"GetBPOSAccountInfo finished remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
                    }
                    else
                    {
                        var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
                        foreach (var temp in profiles)
                        {
                            log.Info($"GetBPOSAccountInfo siteAccount == null profile Name is:{temp.Name}.DomainName:{temp.DomainName}.");
                            if (aveSiteDto.Name.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
                            {
                                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(temp.TenantId).GetAwaiter().GetResult().AdminUrl;

                                siteAccount = new Wrapper.Common.AveBPOSAccountInfo()
                                {
                                    TenantId = temp.TenantId,
                                    AdminUrl = adminUrl,
                                    ClientId = temp.AppClientId,
                                    ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                                    TenantGroupId = TenantLocalValue.LogonGroupId,
                                    AuthenticationProfileId = temp.Id,
                                    AppType = ConvertIdentityTypeToAppType(temp.Type),
                                    AADEnvironment = (Microsoft365.Authentication.AveAzureEnvironment)temp.AADEnvironment,
                                    //AppCert = apponlyCertificate
                                };
                                break;
                            }
                        }
                    }
                }
                //For modern authentication.
                else if (siteAccount != null && siteAccount.ConnectionType == BposConnectionType.ServiceAccount)
                {
                    siteAccount = new AveBPOSAccountInfo()
                    {
                        UserName = string.IsNullOrEmpty(siteAccount.UserName) ? Config.ArchiverConfigForMedia?.UserName : siteAccount.UserName,
                        Password = string.IsNullOrEmpty(siteAccount.Password.ToPlainString()) ? CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(Config.ArchiverConfigForMedia?.Password)).ToSecureString() : siteAccount.Password,
                        AdminUrl = string.IsNullOrEmpty(siteAccount.AdminUrl) ? Config.ArchiverConfigForMedia?.AdminUrl : siteAccount.AdminUrl,
                        TenantId = string.IsNullOrEmpty(siteAccount.TenantId) ? null : siteAccount.TenantId,
                        TenantGroupId = siteAccount.TenantGroupId,
                    };
                    if (string.IsNullOrEmpty(siteAccount.UserName))
                    {
                        var userName = RA.Common.Aos.RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, siteAccount.TenantId).FirstOrDefault();
                        siteAccount.UserName = userName?.UserName ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(siteAccount.Password.ToPlainString()))
                    {
                        siteAccount.Password = RA.Common.Aos.RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, siteAccount.UserName).ToSecureString();
                    }
                    log.Info("Restore site need reset site account. SiteName is:{0},TenantId is:{1},SiteURL:{2}.", aveSiteDto.Name, null, aveSiteDto.SrcUrl);
                }
            }
            catch (Exception eg)
            {
                log.Info("Try get user information & url error {0}", eg.ToString());
            }
            return siteAccount;
        }
        public void RestoreSiteForEndUser(RestoreContentDto aveSiteDto)
        {
            mContainerName = GetContainerLevelPathForRestore(OopStubUrl);
            if (mContainerName.ContainsKey(NodeLevel.SiteCollection.ToString()))
            {
                string siteUrl = mContainerName.GetValue(NodeLevel.SiteCollection.ToString());
                aveSiteDto.Name = siteUrl;
                aveSiteDto.SrcUrl = siteUrl;
                aveSiteDto.SrcName = siteUrl;
            }
            AveBPOSAccountInfo siteAccount = GetBPOSAccountInfo(aveSiteDto);
            ProcessPostAction(aveSiteDto, ref AveSite, ref AveWeb, ref AveList);
            DisposeSite();
            DisposeWeb();
            AveWeb = null;
            AveList = null;
            this.aveFolder = null;
            this.aveListRootFolder = null;
            AveSite = new AveSPSite(aveSiteDto.Name, aveSiteDto.ParentName, null, Config.ContextKind, siteAccount);//SAAS-12070 由于site存在null这种情况，影响到contextKind的获取，这里修改为从config获取
            //AveSite.SourceHeaderSiteId = aveSiteDto.UniqueId;
            AveMetadata metadata;
            AveSiteInfo siteInfo = null;
            while ((metadata = RestoreStream.ReadMetadata()) != null)
            {
                if (metadata.MetadataType == AveMetadataType.SiteBasicInfo)
                {
                    log.Info("Begin restore site level SiteBasicInfo.");
                    siteInfo = metadata.GetMetadata<AveSiteInfo>();
                }
                if (metadata.MetadataType == AveMetadataType.Users)
                {
                    log.Info("Begin restore site level Users.");
                    var users = metadata.GetMetadata<List<AveUserInfo>>();
                    if (users != null)
                    {
                        AveSite.SPMembers.LoadUsers(users);
                    }
                }
                if (metadata.MetadataType == AveMetadataType.Groups)
                {
                    log.Info("Begin restore site level Groups.");
                    var groups = metadata.GetMetadata<List<AveGroupInfo>>();
                    if (groups != null)
                    {
                        AveSite.SPMembers.LoadGroups(groups);
                    }
                }
            }
            AveSite.GetSPSite(siteInfo);
            AveSite.SetContentDBId(Config.DestinationInfo.ContentDBId);
            if (this.Config.UserDomainMapping != null && this.Config.UserDomainMapping.IsInit)
            {
                AveSite.SetUserMapping(Config.UserDomainMapping.UserMappings, Config.UserDomainMapping.DomainMappings, Config.UserDomainMapping.DefaultUser);
                AveSite.SetPlaceHolderAccount(Config.UserDomainMapping.UserPlaceHolderAccount);
            }
            AveSite.SetLookupSourceValue(Config.UseSourceLookupValue);
            AveSite.SetRestoreOption(aveSiteDto.RestoreOption);
            AveSite.OverWriteNavigation = AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.OverWrite);
            AveSite.DisableSPEventReceiver();
            AveSite.KeepDefaultValue = Config.KeepColumnDefaultValue;
            AveSite.IsOutOfPlaceRestore = Config.IsOutOfPlaceRestore;
            if (ItemRestoreConfig.BPOSSiteCollectionConfig.GroupSiteEmails.ContainsKey(aveSiteDto.Name))
            {
                AveSite.GroupSiteEmail = ItemRestoreConfig.BPOSSiteCollectionConfig.GroupSiteEmails[aveSiteDto.Name];
            }
            GlobalRestoreOptionWorker.GlobalRestoreOption = Config.RestoreGlobalOption;
            GlobalRestoreOptionWorker.CheckSiteGlobalSetting(AveSite.ObjectModelFactory, AveSite.SiteUrl, aveSiteDto, new SecurityRestoreOption());
            log.Info("Begin restore RestoreSite Metadata.");
        }
        public void RestoreWebForEndUser(RestoreContentDto aveWebDto)
        {
            if (mContainerName.ContainsKey(NodeLevel.SiteCollection.ToString()))
            {
                string siteUrl = mContainerName.GetValue(NodeLevel.SiteCollection.ToString());
                string webUrl = mContainerName.GetValue(NodeLevel.Site.ToString());
                aveWebDto.Name = webUrl;
                if (webUrl != ".")
                {
                    aveWebDto.SrcUrl = siteUrl + "/" + webUrl;
                }
                else
                {
                    aveWebDto.SrcUrl = siteUrl;
                }
                aveWebDto.SrcName = webUrl;
            }
            ProcessPostAction(aveWebDto, ref AveSite, ref AveWeb, ref AveList);
            DisposeWeb();
            AveWeb = new AveSPWeb(AveSite, aveWebDto.Name);
            var securityRestoreOption = new SecurityRestoreOption
            {
                PromotePermissionToRootWeb = true,
                IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
            };
            GlobalRestoreOptionWorker.CheckWebGlobalSetting(AveSite, AveWeb.Name, aveWebDto, securityRestoreOption);
            AveWeb.SetRestoreOption(aveWebDto.RestoreOption);
            AveWeb.GetWebSelf();
            RecordRestoredFile.CurrentWebId = AveWeb.SPWeb.ID;
        }
        public void RestoreFolderForEndUser(RestoreContentDto aveFolderDto)
        {
            if (mContainerName.ContainsKey(NodeLevel.Folder.ToString()))
            {
                string siteUrl = mContainerName.GetValue(NodeLevel.SiteCollection.ToString());
                string webUrl = mContainerName.GetValue(NodeLevel.Site.ToString());
                string listUrl = mContainerName.GetValue(NodeLevel.List.ToString());
                string folderUrl = mContainerName.GetValue(NodeLevel.Folder.ToString());
                if (webUrl != ".")
                {
                    aveFolderDto.SrcUrl = siteUrl + "/" + webUrl + "/" + listUrl + "/" + folderUrl;
                    aveFolderDto.Name = webUrl + "\\" + listUrl + "\\" + folderUrl;
                    aveFolderDto.SrcName = webUrl + "\\" + listUrl + "\\" + folderUrl;
                }
                else
                {
                    aveFolderDto.SrcUrl = siteUrl + "/" + listUrl + "/" + folderUrl;
                    aveFolderDto.Name = ".\\" + listUrl + "\\" + folderUrl;
                    aveFolderDto.SrcName = ".\\" + listUrl + "\\" + folderUrl;
                }

            }
            else
            {
                return;
            }
            string parentPath = this.mListPath;
            if (!aveFolderDto.Name.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new AveException(@"Looks up a localized string similar to The folder does not belong to the current list. Folder Path: {0} List Path: {1}.", aveFolderDto.Name, this.mListPath);
            }
            string subPath = aveFolderDto.Name.Substring(parentPath.Length).TrimStart('\\');
            var currentFolder = aveFolder;
            this.aveFolder = null;
            this.aveFolder = GenerateFolder(currentFolder, subPath);
            if (aveFolder.ParentFolder != null && (aveFolder.ParentFolder.SPFolder == null || !aveFolder.ParentFolder.SPFolder.Exists))
            {
                aveFolder.ParentFolder.InitSPFolder(false);
            }
            if (aveFolder.SPFolder == null)
            {
                aveFolder.InitSPFolder(false);
            }
        }
        public void RestoreListForEndUser(RestoreContentDto aveListDto)
        {
            if (mContainerName.ContainsKey(NodeLevel.List.ToString()))
            {
                string siteUrl = mContainerName.GetValue(NodeLevel.SiteCollection.ToString());
                string webUrl = mContainerName.GetValue(NodeLevel.Site.ToString());
                string listUrl = mContainerName.GetValue(NodeLevel.List.ToString());
                if (webUrl != ".")
                {
                    aveListDto.SrcUrl = siteUrl + "/" + webUrl + "/" + listUrl;
                    aveListDto.Name = webUrl+"\\"+ listUrl;
                    aveListDto.SrcName = webUrl + "\\" + listUrl;
                }
                else
                {
                    aveListDto.SrcUrl = siteUrl + "/" + listUrl;
                    aveListDto.Name = ".\\"+listUrl;
                    aveListDto.SrcName = ".\\" + listUrl;
                }

            }
            string webNameWithSlash = AveWeb.Name + "\\";
            string listName = aveListDto.Name;
            string subName = string.Empty;
            listName = listName.Substring(webNameWithSlash.Length);
            int pos = listName.IndexOf('\\');
            if (pos >= 0)
            {
                subName = listName.Substring(pos + 1, listName.Length - pos - 1);
                listName = listName.Substring(0, pos);
            }
            ProcessPostAction(aveListDto, ref AveSite, ref AveWeb, ref AveList);
            this.aveListRootFolder = null;
            this.aveFolder = null;
            AveList = new AveSPList(AveWeb, listName);
            AveList.DecodeNameForSpecialChar();
            AveListInfo listInfo = new AveListInfo();
            listInfo.IsOopRestoreList = true;
            listInfo.ServerRelativeUrl = AveWeb.ServerRelativeUrl + '/' + listName;
            listInfo.Id = Guid.NewGuid();
            AveMetadata metadata;
            string fieldSchemaXml = string.Empty; //save the field schema in source list
            while ((metadata = RestoreStream.ReadMetadata()) != null)
            {
                if (metadata.MetadataType == AveMetadataType.ListField)
                {
                    fieldSchemaXml = metadata.GetMetadata<string>();
                    AveList.AveFields.LoadFields(fieldSchemaXml);
                    break;
                }
            }
            AveList.GetSPListByOption(listInfo, ListRestoreOption.TitleAndUrl);
            try
            {
                if (AveList.NeedContinue)
                {
                    this.aveListRootFolder = new AveSPFolder(AveList, string.Empty);
                    this.mListPath = webNameWithSlash + listName;
                    //AddParentFolder(this.mListPath, this.mAveListRootFolder);
                    this.aveFolder = this.aveListRootFolder;
                    if (!string.IsNullOrEmpty(subName))
                    {
                        this.aveFolder = GenerateFolder(aveFolder, subName);
                        this.aveFolder.InitSPFolder();
                        //AddParentFolder(this.mListPath + "\\" + subName, this.mAveFolder);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn(@"localized string similar to An error occurred while restoring a list. Title: {0}{1},{2}", AveWeb.Name, aveListDto.Name, e.ToString());
            }
        }
        private Dictionary<string, string> GetContainerLevelPathForRestore(string fullPath)
        {
            RestoreContentDto aveSiteDto = new RestoreContentDto();
            string containerPath = fullPath.Substring(0, fullPath.LastIndexOf('/'));
            aveSiteDto.Name = containerPath;
            aveSiteDto.SrcUrl = containerPath;
            aveSiteDto.SrcName = containerPath;
            Dictionary<string, string> result = new Dictionary<string, string>();
            var path = containerPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 2 && path[0].StartsWith("https")) //https://m365x54972334.sharepoint.com/sites/TestSite0506/05212//..
            {
                int i = 0;
                bool isRootSite = false;
                string siteCollection;
                if (path[2].Equals("sites", StringComparison.OrdinalIgnoreCase) || path[2].Equals("teams", StringComparison.OrdinalIgnoreCase) || path[2].Equals("personal", StringComparison.OrdinalIgnoreCase))
                {
                    siteCollection = path[0] + "//" + path[1] + "/" + path[2] + "/" + path[3];
                    aveSiteDto.Name = siteCollection;
                    aveSiteDto.SrcUrl = siteCollection;
                    aveSiteDto.SrcName = siteCollection;
                }
                else
                {
                    siteCollection = path[0] + "//" + path[1];
                    isRootSite = true;
                }
                log.Info($"[RestoreForEndUser]-[GetContainerLevelPathForRestore]: Get the BPOS Account Info. Site: {aveSiteDto.SrcUrl}");
                AveBPOSAccountInfo siteAccount = GetBPOSAccountInfo(aveSiteDto);
                while (i < path.Length)
                {
                    try
                    {
                        AveSite = new AveSPSite(siteCollection, aveSiteDto.ParentName, null, Config.ContextKind, siteAccount);
                        AveMetadata metadata;
                        AveSiteInfo siteInfo = null;
                        AveSite.GetSPSite(siteInfo);
                        if (AveSite.SPSite != null)
                        {
                            result.Add(NodeLevel.SiteCollection.ToString(), siteCollection);
                            break;
                        }
                        else
                        {
                            siteCollection = path[0] + "//" + path[1];
                            isRootSite = true;
                            log.Warn($"this site maybe root site,so creat new url:{siteCollection}");
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error($"something went wrong went genarat site.error:{e}");
                        throw;
                    }
                    i++;
                }
                int j = isRootSite ? 2 : 4;
                string webUrl = path[j];
                while (j < path.Length)
                {
                    try
                    {
                        AveWeb = new AveSPWeb(AveSite, webUrl);
                        AveWeb.GetWebSelf();
                        if (AveWeb.SPWeb != null)
                        {
                            webUrl = webUrl + "/" + path[j + 1];
                            j++;
                        }
                        else
                        {
                            log.Warn($"get web failed,it may be list name,name:{webUrl},j={j}");
                            if (j == 2 || j == 4)
                            {
                                result.Add(NodeLevel.Site.ToString(), ".");
                                break;
                            }
                            else
                            {
                                result.Add(NodeLevel.Site.ToString(), webUrl.Substring(0, webUrl.LastIndexOf("/")));
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error($"get web failed,name:{webUrl},error:{e},j={j}");
                        throw;
                    }

                }
                result.Add(NodeLevel.List.ToString(), path[j]);
                string folderUrl = string.Empty;
                for (int k = j + 1; k < path.Length; k++)
                {
                    folderUrl = folderUrl + "\\" + path[k];
                }
                if (!string.IsNullOrEmpty(folderUrl))
                {
                    result.Add(NodeLevel.Folder.ToString(), folderUrl.TrimStart('\\'));
                }
            }
            return result;
        }
        protected RestoreContentDto ConvertRestoreContentDtoForArchiverOOPRestore(RestoreContentDto restoreContentDto)
        {
            
            RestoreContentDto desRestoreContentDto = restoreContentDto;
            try
            {
                switch (desRestoreContentDto.Type)
                {
                    case AveConstants.TYPE_SITE:
                        sourceSiteUrl = desRestoreContentDto.SrcUrl;
                        log.Info($"ConvertRestoreContentDtoForArchiverOOPRestore IsArchiverOOPRestore Type:{desRestoreContentDto.Type}.oopMappingSiteUrl:{targetSiteUrl}." +
                            $"RestoreContentDto.Name:{desRestoreContentDto.Name}." +
                            $"RestoreContentDto.SrcName:{desRestoreContentDto.SrcName}." +
                            $"RestoreContentDto.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                        desRestoreContentDto.Name = targetSiteUrl;
                        desRestoreContentDto.SrcName = targetSiteUrl;
                        desRestoreContentDto.SrcUrl = targetSiteUrl;
                        break;

                    case AveConstants.TYPE_WEB:
                        if (IsRestoreToSPO && !string.IsNullOrEmpty(targetWebUrl))
                        {
                            log.Info($"ConvertRestoreContentDtoForArchiverOOPRestore IsArchiverOOPRestore Type:{desRestoreContentDto.Type}.oopMappingSiteUrl:{targetWebUrl}." +
                            $"RestoreContentDto.Name:{desRestoreContentDto.Name}." +
                            $"RestoreContentDto.SrcName:{desRestoreContentDto.SrcName}." +
                            $"RestoreContentDto.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                            if (DestInfo.IsRootWeb)
                            {
                                desRestoreContentDto.Name = DestInfo.WebName;
                            }
                            else
                            {
                                // fm: subsite1/sub1a
                                desRestoreContentDto.Name = DestInfo.WebPath.Substring(AveSite.ServerRelativeUrl.Length).Trim('/');
                            }
                            desRestoreContentDto.SrcName = desRestoreContentDto.Name;
                            desRestoreContentDto.SrcUrl = targetWebUrl;
                        }
                        desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                        break;
                    case AveConstants.TYPE_LIST:
                        sourceLibUrl = desRestoreContentDto.SrcUrl;
                        if (IsRestoreToSPO && !string.IsNullOrEmpty(targetListUrl))
                        {
                            log.Info($"ConvertRestoreContentDtoForArchiverOOPRestore IsArchiverOOPRestore Type:{desRestoreContentDto.Type}.oopMappingSiteUrl:{targetListUrl}." +
                            $"RestoreContentDto.Name:{desRestoreContentDto.Name}." +
                            $"RestoreContentDto.SrcName:{desRestoreContentDto.SrcName}." +
                            $"RestoreContentDto.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                            // "subsite1/sub1a\\Documents"
                            // ".\\Documents"
                            if (DestInfo.IsRootWeb)
                            {
                                desRestoreContentDto.Name = DestInfo.WebName + "\\" + DestInfo.ListName;
                            }
                            else
                            {
                                desRestoreContentDto.Name = DestInfo.WebPath.Substring(AveSite.ServerRelativeUrl.Length).Trim('/') + "\\" + DestInfo.ListName;
                            }
                            desRestoreContentDto.SrcName = desRestoreContentDto.Name;
                            desRestoreContentDto.SrcUrl = targetListUrl;
                        }
                        desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                        break;
                    case AveConstants.TYPE_FOLDER:
                        if (IsRestoreToSPO)
                        {
                            log.Info($"ConvertRestoreContentDtoForArchiverOOPRestore IsArchiverOOPRestore Type:{desRestoreContentDto.Type}.oopMappingSiteUrl:{targetFolderUrl}." +
                                $"RestoreContentDto.Name:{desRestoreContentDto.Name}." +
                                $"RestoreContentDto.SrcName:{desRestoreContentDto.SrcName}." +
                                $"RestoreContentDto.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                            if (!string.IsNullOrEmpty(targetFolderUrl))
                            {
                                if (!string.IsNullOrEmpty(sourceFolderUrl))
                                {
                                    desRestoreContentDto.SrcUrl = targetFolderUrl + desRestoreContentDto.SrcUrl.Substring(sourceFolderUrl.Length);
                                    desRestoreContentDto.Name = mListPath + "\\" + desRestoreContentDto.SrcUrl.Substring(targetListUrl.Length).Trim('/').Replace("/", "\\");
                                    desRestoreContentDto.SrcName = desRestoreContentDto.Name; // ".\\Documents\\..."
                                    break;
                                }
                                log.Warn($"ConvertRestoreContentDtoForArchiverOOPRestore sourceFolderUrl is null or empty when restore folder. Type:{desRestoreContentDto.Type}.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                                sourceFolderUrl = desRestoreContentDto.SrcUrl;
                                desRestoreContentDto.Name = AveList.Name + "\\" + targetFolderUrl.Split('/').Last(); ;
                                desRestoreContentDto.SrcName = desRestoreContentDto.Name;

                                desRestoreContentDto.SrcUrl = targetFolderUrl;
                                
                            }
                            else
                            {
                                log.Warn($"ConvertRestoreContentDtoForArchiverOOPRestore targetFolderUrl is null or empty when restore folder. Type:{desRestoreContentDto.Type}.SrcUrl:{desRestoreContentDto.SrcUrl}.");
                                desRestoreContentDto.SrcUrl = targetListUrl + desRestoreContentDto.SrcUrl.Substring(sourceLibUrl.Length);
                            }
                            break;
                        }
                        desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                        break;
                    case AveConstants.TYPE_PROJECT:
                    case AveConstants.TYPE_APP:
                    case AveConstants.TYPE_DOCUMENT:
                    case AveConstants.TYPE_LISTITEM:
                    case AveConstants.TYPE_ATTACHMENTS:
                    case AveConstants.TYPE_VERSION:
                    case AveConstants.TYPE_LISTITEMVERSION:
                        if (IsRestoreToSPO)
                        {
                            if (string.IsNullOrEmpty(sourceLibUrl) && string.IsNullOrEmpty(sourceFolderUrl))
                            {
                                desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                                break;
                            }

                            if (string.IsNullOrEmpty(sourceFolderUrl))
                            {
                                // should not
                                log.Warn($"ConvertRestoreContentDtoForArchiverOOPRestore sourceFolderUrl is null or empty when restore item, but the type is below folder. Type:{desRestoreContentDto.Type}.SrcUrl:{desRestoreContentDto.SrcUrl.LogBase64()}.");
                                desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                            }
                            else if (!string.IsNullOrEmpty(targetFolderUrl))
                            {
                                desRestoreContentDto.SrcUrl = targetFolderUrl + desRestoreContentDto.SrcUrl.Substring(sourceFolderUrl.Length);
                            }
                            else if (!string.IsNullOrEmpty(targetListUrl))
                            {
                                desRestoreContentDto.SrcUrl = targetListUrl + desRestoreContentDto.SrcUrl.Substring(sourceLibUrl.Length);
                            }
                            break;
                        }
                        desRestoreContentDto.SrcUrl = targetSiteUrl + desRestoreContentDto.SrcUrl.Substring(sourceSiteUrl.Length);
                        break;

                    default:
                        log.Warn($"ConvertRestoreContentDtoForArchiverOOPRestore wrong type:{desRestoreContentDto.Type}.");
                        break;
                }
            }
            catch (Exception e)
            {
                log.Error($"ConvertRestoreContentDtoForArchiverOOPRestore error.Type:{desRestoreContentDto.Type}.Message:{e}.");
            }
            return desRestoreContentDto;
        }
        public override void RestoreSite(RestoreContentDto aveSiteDto)
        {
            string sourceUrl = aveSiteDto.SrcUrl;
            if (IsEnduserRestore&&!string.IsNullOrEmpty(OopStubUrl))
            {
                RestoreSiteForEndUser(aveSiteDto);
            }
            else
            {
                RestoreSiteForOpus(aveSiteDto);
            }
            RecordRestoredFile.InitSiteUrl(aveSiteDto.SrcUrl, sourceUrl);
        }
        public void RestoreSiteForOpus(RestoreContentDto aveSiteDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreSite"))
            {
                AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
                log.Info($"RestoreSiteForOpus the siteAccount info:TenantId:{siteAccount?.TenantId},ClientId:{siteAccount?.ClientId},AuthenticationProfileId:{siteAccount?.AuthenticationProfileId}");
                try
                {
                    if(aveSiteDto.SrcName != aveSiteDto.SrcUrl)
                    {
                        log.Warn($"Restore dto srcName : {aveSiteDto.SrcName} diff with srcUrl : {aveSiteDto.SrcUrl}");
                    }

                    if (IsRestoreToSPO || IsAdvancedRestore)
                    {
                        targetSiteUrl = DestInfo.SiteCollectionUrl;
                        Config.RestoreGlobalOption.ContainerSetting = ContainerSetting.None;
                        //InitOriginalAveObject(aveSiteDto, true);
                        aveSiteDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveSiteDto);
                        if (!string.Equals(siteAccount.TenantId, DestInfo.TenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Info($"Tenant id is different. source tenant id:{siteAccount?.TenantId}, dest tenant id:{DestInfo.TenantId}. Need to reset site account with dest tenant id.");
                            siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[targetSiteUrl];
                        }
                    }
                    else
                    {
                        var mapping = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(aveSiteDto.SrcName);
                        if (mapping != null && !string.IsNullOrEmpty(mapping.TargetSiteUrl))
                        {
                            Config.RestoreGlobalOption.ContainerSetting = ContainerSetting.None;
                            log.Info($"this site need to mapping new site url,source:{aveSiteDto.SrcName},target:{mapping.TargetSiteUrl}");
                            targetSiteUrl = mapping.TargetSiteUrl;
                            //InitOriginalAveObject(aveSiteDto);
                            aveSiteDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveSiteDto);
                        }
                        if (TeamsRestoreState.mappingSiteURLs.TryGetValue(aveSiteDto.SrcUrl, out var mappedSiteUrl) && !string.Equals(aveSiteDto.SrcUrl, mappedSiteUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            Config.RestoreGlobalOption.ContainerSetting = ContainerSetting.None;
                            log.Info($"TeamsRestoreState this site need to mapping new site url,source:{aveSiteDto.SrcName},target:{mappedSiteUrl}");
                            targetSiteUrl = mappedSiteUrl;
                            aveSiteDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveSiteDto);
                        }
                    }

                    log.Info("RestoreSiteForOpus RestoreContentDto Name is:{0},siteAccount is null:{1},SiteURL:{2}.", aveSiteDto.Name, siteAccount == null, aveSiteDto.SrcUrl);
                    if (siteAccount == null)
                    {
                        log.Info($"siteAccount is null when restore");
                        RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(aveSiteDto.SrcUrl);
                        if (remoteSiteCollection != null && !string.IsNullOrEmpty(remoteSiteCollection.TenantId))
                        {
                            log.Info($"RestoreSiteForOpus remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.");
                            siteAccount = PoolUserUtil.GetBPOSInfo2Async(remoteSiteCollection).Result;
                            log.Info($"RestoreSiteForOpus finished remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
                        }
                        else
                        {
                            string hostURL = new Uri(aveSiteDto.SrcUrl).Scheme + @"://" + new Uri(aveSiteDto.SrcUrl).Authority;
                            RemoteSiteCollection sameTenantRemoteSiteCollection = RemoteNodeDao.GetRemoteSiteCollectionByHostUrl(hostURL);
                            if (sameTenantRemoteSiteCollection != null && !string.IsNullOrEmpty(sameTenantRemoteSiteCollection.TenantId))
                            {
                                log.Info($"RestoreSiteForOpus remoteSiteCollection != null TenantID:{sameTenantRemoteSiteCollection.TenantId}.HostURL:{hostURL}.sameTenantRemoteSiteCollection:{sameTenantRemoteSiteCollection.url}.");
                                siteAccount = PoolUserUtil.GetBPOSInfo2Async(sameTenantRemoteSiteCollection).Result;
                                log.Info($"RestoreSiteForOpus finished remoteSiteCollection != null TenantID:{sameTenantRemoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
                            }
                            else
                            {
                                var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
                                foreach (var temp in profiles)
                                {
                                    log.Info($"RestoreSiteForOpus siteAccount == null profile Name is:{temp.Name}.DomainName:{temp.DomainName}.");
                                    if (aveSiteDto.Name.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(temp.TenantId).GetAwaiter().GetResult().AdminUrl;

                                        siteAccount = new Wrapper.Common.AveBPOSAccountInfo()
                                        {
                                            TenantId = temp.TenantId,
                                            AdminUrl = adminUrl,
                                            ClientId = temp.AppClientId,
                                            ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                                            TenantGroupId = TenantLocalValue.LogonGroupId,
                                            AuthenticationProfileId = temp.Id,
                                            AppType = ConvertIdentityTypeToAppType(temp.Type),
                                            AADEnvironment = (Microsoft365.Authentication.AveAzureEnvironment)temp.AADEnvironment,
                                            //AppCert = apponlyCertificate
                                        };
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //For modern authentication.
                    else if (siteAccount != null && siteAccount.ConnectionType == BposConnectionType.ServiceAccount)
                    {
                        siteAccount = new AveBPOSAccountInfo()
                        {
                            UserName = string.IsNullOrEmpty(siteAccount.UserName) ? Config.ArchiverConfigForMedia?.UserName : siteAccount.UserName,
                            Password = siteAccount.Password.IsNullOrEmpty() ? CspCommunicationWrapper.UnWrapKeyToSecureString(Config.ArchiverConfigForMedia?.Password) : siteAccount.Password,
                            AdminUrl = string.IsNullOrEmpty(siteAccount.AdminUrl) ? Config.ArchiverConfigForMedia?.AdminUrl : siteAccount.AdminUrl,
                            TenantId = string.IsNullOrEmpty(siteAccount.TenantId) ? null : siteAccount.TenantId,
                            TenantGroupId = siteAccount.TenantGroupId,
                        };
                        if (string.IsNullOrEmpty(siteAccount.UserName))
                        {
                            var firstServiceAccount = RA.Common.Aos.RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, siteAccount.TenantId).FirstOrDefault();
                            siteAccount.UserName = firstServiceAccount?.UserName ?? string.Empty;
                        }
                        if (siteAccount.Password.IsNullOrEmpty())
                        {
                            siteAccount.Password = RA.Common.Aos.RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, siteAccount.UserName).ToSecureStringWithEmptyCheck();
                        }
                        log.Info("Restore site need reset site account. SiteName is:{0},TenantId is:{1},SiteURL:{2}.", aveSiteDto.Name, null, aveSiteDto.SrcUrl);
                    }
                }
                catch (Exception eg)
                {
                    log.Info("Try get user information & url error {0}", eg.ToString());
                }
                bool conflictWithRecycleBin = false;
                try
                {
                    if(siteAccount == null)
                    {
                        log.Error("SiteAccount is null");
                        throw new Exception("Site Account is null");
                    }
                    //AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
                    log.Info("get user for delete recyclebin site {0}.", aveSiteDto.Name);
                    AveObjectModelFactory tenantFactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, siteAccount, AveContextKind.ClientObjectModel);
                    log.Info("RestoreSiteForOpus.O365 Admin Url is : {0}.", siteAccount.AdminUrl);
                    IAveTenant aveTenant = tenantFactory.CreateTenant(siteAccount.AdminUrl);
                    var geoLocationInfo = aveTenant.GetTenantGeoLocationinfo();
                    if (geoLocationInfo != null && geoLocationInfo.Count > 1)
                    {
                        foreach (var location in geoLocationInfo)
                        {
                            if (aveSiteDto.SrcUrl.StartsWith(location.RootSiteUrl) || aveSiteDto.SrcUrl.StartsWith(location.MySiteHostUrl))
                            {
                                siteAccount.AdminUrl = location.TenantAdminUrl;
                                log.Info($"RestoreSiteForOpus.O365 Admin New Url is : {siteAccount.AdminUrl}.SiteUrl:{aveSiteDto.SrcUrl}.");
                            }
                        }
                    }
                    log.Info("Create Container Admin URL {0}", siteAccount.AdminUrl);
                    IAveSite site = tenantFactory.CreateAdminCenterSite(siteAccount.AdminUrl);
                    log.Info("Create admin center site: {0} successfully.", siteAccount.AdminUrl);
                    tenant = tenantFactory.CreateTenant(site);//aveSiteDto.SiteUrl);
                    conflictWithRecycleBin = tenant.SiteExistsAnywhere(aveSiteDto.Name) == Microsoft.SharePoint.Client.SiteExistence.Recycled;
                }
                catch (Exception e1)
                {
                    log.Error("Check RecycleBin Conflict Error {0}", e1.ToString());
                }

                var reportDto = new AveRestoreReportDto { Type = aveSiteDto.Type.ToString(), Title = aveSiteDto.Name,PathMD5 = aveSiteDto.ItemPathMd5 };//Path = aveSiteDto.Name

                if (conflictWithRecycleBin)
                {
                    reportDto.Status = RestoreStatus.Failed;
                    reportDto.Path = aveSiteDto.Name;
                    reportDto.SourcePath = aveSiteDto.Name;
                    if (!BaseSPActionProcessor.IsOnedrive(aveSiteDto.Name))
                    {
                        reportDto.ErrorMessage = "RM_JM_RestoreFaild_SCExistInRecycleBin_ErrorMessage";
                    }
                    else
                    {
                        reportDto.ErrorMessage = "RM_JM_RestoreFaild_OnedriveExistInRecycleBin_ErrorMessage";
                    }                        
                    AddReport(reportDto);
                    Report.IsRootNodeError = true;
                    throw new Exception(reportDto.ErrorMessage);
                }

                ProcessPostAction(aveSiteDto, ref AveSite, ref AveWeb, ref AveList);
                
                DisposeSite();
                DisposeWeb();
                bool? isSiteExistInDest = null;
                try
                {
                    AveWeb = null;
                    AveList = null;
                    this.aveFolder = null;
                    this.aveListRootFolder = null;
                    AveSite = new AveSPSite(aveSiteDto.Name, aveSiteDto.ParentName, null, Config.ContextKind, siteAccount);//SAAS-12070 由于site存在null这种情况，影响到contextKind的获取，这里修改为从config获取
                    AveSite.SourceHeaderSiteId = aveSiteDto.UniqueId;
                    //AveSite = new AveSPSite(aveSiteDto.Name, aveSiteDto.ParentName, null, ItemRestoreConfig.BPOSSiteCollectionConfig.GetContextKind(aveSiteDto.Name), siteAccount); //SAAS-13149 这里的site可能不存在所以这里所用的构造函数不能使用带有site实例化的。
                    //AveSite = new AveSPSite(aveSiteDto.Name, aveSiteDto.ParentName,ItemRestoreConfig.BPOSSiteCollectionConfig.GetContextKind(aveSiteDto.Name), siteAccount);
                    //AveSite.NavigationRestoreSetting = NavigationRestoreSetting.MoveBoth;

                    //if (IsRestoreToSPO)
                    //{
                    //    AveSite.GetSPSite();
                    //    GlobalRestoreOptionWorker.GlobalRestoreOption = Config.RestoreGlobalOption;
                    //    return;
                    //}

                    #region=====================Set Option=====================
                    AveSite.SetContentDBId(Config.DestinationInfo.ContentDBId);
                    if (this.Config.UserDomainMapping != null && this.Config.UserDomainMapping.IsInit)
                    {
                        AveSite.SetUserMapping(Config.UserDomainMapping.UserMappings, Config.UserDomainMapping.DomainMappings, Config.UserDomainMapping.DefaultUser);
                        AveSite.SetPlaceHolderAccount(Config.UserDomainMapping.UserPlaceHolderAccount);
                    }
                    AveSite.SetLookupSourceValue(Config.UseSourceLookupValue);
                    AveSite.SetRestoreOption(aveSiteDto.RestoreOption);
                    AveSite.OverWriteNavigation = AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.OverWrite);
                    //we only use language mapping in out of place
                    if (Config.IsOutOfPlaceRestore)
                    {
                        AveSite.SetLanguageForNew(Config.DestinationInfo.Language);
                        AveSite.SetLanguageMapping(AveLanguageProcesser.GetLanguageInstance(AveEnv.AgentRootFolder, Config.JobDir));
                        InitNewLangageMapping();
                        if (mNewCreatedLCD != -1 && !Config.DisableLanguageMapping)
                        {
                            AveSite.SetLanguageForNew((uint)mNewCreatedLCD);
                        }
                    }
                    AveSite.DisableSPEventReceiver();
                    AveSite.KeepDefaultValue = Config.KeepColumnDefaultValue;
                    AveSite.IsOutOfPlaceRestore = Config.IsOutOfPlaceRestore;
                    if (ItemRestoreConfig.BPOSSiteCollectionConfig.GroupSiteEmails.ContainsKey(aveSiteDto.Name))
                    {
                        AveSite.GroupSiteEmail = ItemRestoreConfig.BPOSSiteCollectionConfig.GroupSiteEmails[aveSiteDto.Name];
                    }
                    #endregion

                    GlobalRestoreOptionWorker.GlobalRestoreOption = Config.RestoreGlobalOption;
                    GlobalRestoreOptionWorker.CheckSiteGlobalSetting(AveSite.ObjectModelFactory, AveSite.SiteUrl, aveSiteDto, new SecurityRestoreOption());

                    using (AvePerformanceScope metaPc = new AvePerformanceScope("GranularRestore.RestoreSite.Metadata"))
                    {
                        AveMetadata metadata;
                        //AveSPUserProfile userProfile = null;
                        bool isMySite = false;
                        bool userProfileNeedRestore = false;
                        bool isUserProfileServiceAvailable = true;
                        log.Info("Begin restore RestoreSite Metadata.");
                        while ((metadata = RestoreStream.ReadMetadata()) != null)
                        {
                            switch (metadata.MetadataType)
                            {
                                case AveMetadataType.SiteBasicInfo:
                                    log.Info("Begin restore site level SiteBasicInfo.");
                                    var siteInfo = metadata.GetMetadata<AveSiteInfo>();

                                    if (IsRestoreToSPO || IsAdvancedRestore)
                                    {
                                        AveSite.GetSiteSelf(siteInfo);
                                        if (isOriginalSiteExist)
                                        {
                                            oriAveSite.GetSiteSelf();
                                            log.Info("Get original site information finish.");
                                        }
                                        break;
                                    }
                                    if (AveSite.CheckRestoreOption(AveRestoreMode.Replace) && ReplaceType.Equals(AveConstants.TYPE_SITE))
                                    {
                                        bool exist = ReplaceWorker.DeleteSite(Config.ObjectModelFactory, AveSite, Config.IncludeProjectsData);
                                        NullableBooleanExtension.SetIfValueNotExist(ref isSiteExistInDest, exist);
                                    }
                                    AveSite.DestinationURL = Config.IsOutOfPlaceRestore ? aveSiteDto.ParentName : siteInfo.WebAppUrl;
                                    //AveSite.SetSiteCreationAccount(aveSiteDto.OwnerLogin, siteInfo);
                                    try
                                    {
                                        //Dictionary<string, object> createSiteInfo = new Dictionary<string, object>();
                                        if (Config.EventCategory == EventCategorys.DocAveAgentService.StorageOptimization_SP2010_Archiver_Restore
                                            && ItemRestoreConfig.BPOSSiteCollectionConfig.IsNodeArchivered.ContainsKey(aveSiteDto.SrcUrl)
                                            && ItemRestoreConfig.BPOSSiteCollectionConfig.IsNodeArchivered[aveSiteDto.SrcUrl] == true)
                                        {
                                            AveCreateSiteInfo createSiteInfo = new AveCreateSiteInfo();
                                            createSiteInfo.UserName = string.IsNullOrEmpty(Config.ArchiverConfigForMedia.UserName) ? siteAccount.UserName : Config.ArchiverConfigForMedia.UserName;
                                            createSiteInfo.Password = Config.ArchiverConfigForMedia.Password;
                                            createSiteInfo.AdminUrl = Config.ArchiverConfigForMedia.AdminUrl;
                                            if (Config.ArchiverConfigForMedia.UseBackupStorageQuota)
                                            {
                                                if (siteInfo.StorageMaximumLevel == 0)
                                                {
                                                    //api create site时使用的MB为单位，所以这里将其转换为MB为单位
                                                    createSiteInfo.StorageQuota = 26214400;//25600GB
                                                }
                                                else
                                                {
                                                    //api create site时使用的MB为单位，备份的为MB
                                                    createSiteInfo.StorageQuota = siteInfo.StorageMaximumLevel;
                                                }
                                            }
                                            else
                                            {
                                                //SAAS-23918 control传过来的值是以GB为单位，api create site时使用的MB为单位，所以这里将其转换为MB为单位
                                                createSiteInfo.StorageQuota = (Config.ArchiverConfigForMedia.StorageQuota) * 1024;
                                            }
                                            if (Config.ArchiverConfigForMedia.UseBackupResourceQuota)
                                            {
                                                if (siteInfo.UserCodeMaximumLevel <= 1E-06)
                                                {
                                                    createSiteInfo.ResourceQuota = 300;
                                                }
                                                else
                                                {
                                                    createSiteInfo.ResourceQuota = siteInfo.UserCodeMaximumLevel;
                                                }
                                            }
                                            else
                                            {
                                                createSiteInfo.ResourceQuota = Config.ArchiverConfigForMedia.ResourceQuota;
                                            }
                                            log.Info("Create site info AdminUrl:{0}, StorageQuota:{1}, ResourceQuota:{2}.", Config.ArchiverConfigForMedia.AdminUrl, createSiteInfo.StorageQuota, createSiteInfo.ResourceQuota);
                                            createSiteInfo.EventCategory = Config.EventCategory;
                                            createSiteInfo.CustomerId = IdentityManager.IdentityContent;
                                            //createSiteInfo.AosApiUrl = GlobalRoleConfiguration.PortalApiURL;
                                            if (Config.SpecifyUser != null)
                                            {
                                                createSiteInfo.SiteOwnerUPN = Config.SpecifyUser.UserPrincipalName;
                                                log.Info($"Create site Config.Owner LoginName:{Config.SpecifyUser.UserPrincipalName}, DisplayName:{Config.SpecifyUser.DisplayName}, Email:{Config.SpecifyUser.Email}.");
                                            }
                                            AveSite.RestoreSiteSelf(siteInfo, createSiteInfo);
                                        }
                                        else
                                        {
                                            AveSite.IsNewCreated = TeamsRestoreState.IsNewCreateSite(siteInfo.Url);
                                            if (Config.SpecifyUser != null)
                                            {
                                                AveSite.RestoreSiteSelf(siteInfo,null,Config.SpecifyUser);
                                            }
                                            else
                                            {
                                                AveSite.RestoreSiteSelf(siteInfo);
                                            }
                                            
                                        }
                                        //using (var report = AveSite.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveSiteDto));
                                        //}
                                        if (isOriginalSiteExist)
                                        {
                                            oriAveSite.GetSPSite(siteInfo);
                                            log.Info("Get original site information finish.");
                                        }
                                    }
                                    finally
                                    {
                                        NullableBooleanExtension.SetIfValueNotExist(ref isSiteExistInDest, !AveSite.IsNewCreated);
                                    }
                                    isMySite = AveSite.SourceSiteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
                                    break;

                                case AveMetadataType.SiteProperty:
                                    log.Info("Begin restore site level SiteProperty.");
                                    var settingInfo = metadata.GetMetadata<AveSiteSettingInfo>();
                                    AveSite.SourceSiteSettingInfo = settingInfo;
                                    if (AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreProperty))
                                    {
                                        AveSite.RestoreSiteProperty(settingInfo);
                                        //using (var report = AveSite.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveSiteDto));
                                        //}
                                    }
                                    break;

                                case AveMetadataType.SiteFeature:
                                    log.Info("Begin restore site level SiteFeature.");
                                    try
                                    {
                                        if (AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreProperty))
                                        {
                                            using (var featureManager = new AveSPFeature(AveSite))
                                            {
                                                featureManager.Restore(metadata.GetMetadata<AveFeatureInfoBox>());
                                                //using (var report = featureManager.GetReport())
                                                //{
                                                //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveSiteDto));
                                                //}
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, @"Looks up a localized string similar to An error occurred while resotring site features. Error:{0}.", e);
                                    }
                                    break;

                                case AveMetadataType.Users:
                                    log.Info("Begin restore site level Users.");
                                    var users = metadata.GetMetadata<List<AveUserInfo>>();
                                    if (users != null)
                                    {
                                        if (!IsRestoreToSPO && AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                        {
                                            using (new AvePerformanceScope("GranularRestore.RestoreSite.Users"))
                                            {
                                                //we have to restore site adminsitrators in site level restore, so we use the mothod with 2 arguments and set "siteLevel" to true
                                                AveSite.SPMembers.MultiThreadRestoreUsers(users, true, false, Config.ExcludeGroupWithoutPermissions);
                                            }
                                        }
                                        else
                                        {
                                            AveSite.SPMembers.LoadUsers(users);
                                        }
                                    }
                                    break;

                                case AveMetadataType.Groups:
                                    log.Info("Begin restore site level Groups.");
                                    var groups = metadata.GetMetadata<List<AveGroupInfo>>();
                                    if (groups != null)
                                    {
                                        log.Info($"restore site,group count is:{groups.Count}.");
                                        foreach (var g in groups)
                                        {
                                            log.Info($"restore site,group name is :{g.Title}.members:{g.Members?.Count}");
                                        }
                                        if (!IsRestoreToSPO && AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                        {
                                            using (new AvePerformanceScope("GranularRestore.RestoreSite.Groups"))
                                            {
                                                log.Info($"restore site,restore Groups.");
                                                AveSite.SPMembers.RestoreGroups(groups, true, Config.ExcludeGroupWithoutPermissions);
                                            }
                                        }
                                        else
                                        {
                                            log.Info($"restore site,just LoadGroups.");
                                            AveSite.SPMembers.LoadGroups(groups);
                                        }
                                    }
                                    break;
                                case AveMetadataType.AudienceCache:
                                    log.Info("Begin restore site level AudienceCache.");
                                    if (AveEnv.IsMoss)
                                    {
                                        var audienceManager = new AveAudienceManager(AveSite);
                                        audienceManager.GenerateIDMapping(metadata.GetMetadata<Dictionary<string, string>>());
                                    }
                                    break;
                                #region User Profile
                                //case AveMetadataType.UserProfileProperties:
                                //    var propertyInfos = metadata.GetMetadata<List<AvePropertyInfo>>();
                                //    if (AveEnv.IsMoss &&
                                //        (Config.IncludeUserProfile || isMySite))
                                //    {
                                //        try
                                //        {
                                //            userProfile = new AveSPUserProfile(AveSite, false);
                                //            userProfile.EnableTag = true;
                                //            userProfile.RestoreUserProfileProperties(propertyInfos);
                                //        }
                                //        catch (Exception ex)
                                //        {
                                //            isUserProfileServiceAvailable = false;
                                //            var report = new AveRestoreReportDto
                                //            {
                                //                EntityType = GCommon.Contract.Server.Job.Object.JobReportDetailEntityType.Configuration,
                                //                RelatedObjectTitle = aveSiteDto.Name,
                                //                Name = AveSite.SPSite.Owner.LoginName,
                                //                Type = aveSiteDto.Type.ToString(),
                                //                Status = RestoreStatus.Failed,
                                //                Path = aveSiteDto.Name,
                                //                Title = aveSiteDto.Name,
                                //                SourcePath = aveSiteDto.SrcUrl
                                //            };
                                //            report.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(ex, RestoreReportKey.Item_UserProfleFaild.ToString(), RestoreReportResource.Item_UserProfleFaild, ex.Message);
                                //            AddReport(report);
                                //            log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreUserProfileFailedEventMessage(aveSiteDto.Name, ex));
                                //        }
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfile:
                                //    AveUserProfileInfo profileInfo = metadata.GetMetadata<AveUserProfileInfo>();
                                //    //目的端User Profile Service不可用，跳过User Profile的相关还原
                                //    userProfileNeedRestore = (Config.IncludeUserProfile || profileInfo.LoginName.Equals(AveSite.SourceSiteInfo.OwnerLogin, StringComparison.OrdinalIgnoreCase)) && isUserProfileServiceAvailable;
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.Restore(profileInfo);
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfileTag:
                                //    AveSocialTagInfo tagInfo = metadata.GetMetadata<AveSocialTagInfo>();
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.RestoreTag(tagInfo);
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfileColleague:
                                //    AveColleagueInfo colleagueInfo = metadata.GetMetadata<AveColleagueInfo>();
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.RestoreColleague(colleagueInfo);
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfileComment:
                                //    AveSocialCommentInfo info = metadata.GetMetadata<AveSocialCommentInfo>();
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.RestoreComment(info);
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfileDetail:
                                //    List<AveUserProfileValueInfo> vaueInfo = metadata.GetMetadata<List<AveUserProfileValueInfo>>();
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.RestoreDetails(vaueInfo);
                                //    }
                                //    break;
                                //case AveMetadataType.UserProfileMembership:
                                //    AveMembershipInfo memberInfo = metadata.GetMetadata<AveMembershipInfo>();
                                //    if (userProfile != null && userProfileNeedRestore)
                                //    {
                                //        userProfile.RestoreMembership(memberInfo);
                                //    }
                                //    break;
                                #endregion
                                case AveMetadataType.SiteSearchInfo:
                                    log.Info("Begin restore site level SiteSearchInfo.");
                                    if (AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreProperty))
                                    {
                                        var searchInfo = metadata.GetMetadata<AveSearchInfo>();
                                        if (searchInfo != null)
                                        {
                                            var searchManager = new AveSPSearch(AveSite);
                                            searchManager.Restore(searchInfo);
                                            //using (var report = searchManager.GetReport())
                                            //{
                                            //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveSiteDto));
                                            //}
                                        }
                                    }
                                    break;

                                case AveMetadataType.LanguageFile:
                                    log.Info("Begin restore site level LanguageFile.");
                                    if (AveSite.AveLanguageProcesser == null)
                                    {
                                        break;
                                    }
                                    var languageInfo = metadata.GetMetadata<AveLanguageInfo>();
                                    if (languageInfo != null)
                                    {
                                        AveSite.RestoreLanguageFile(languageInfo);
                                    }
                                    if (AveSite.SrcLanguageId != AveSite.SPSite.RootWeb.Language && !Config.DisableLanguageMapping)
                                    {
                                        AveSite.AveLanguageProcesser.LoadMapping(string.Empty, AveSite.SrcLanguageId, AveSite.SPSite.RootWeb.Language, this.Config.LanguageMappingInfo.LanguageMappingString);
                                    }

                                    break;
                                case AveMetadataType.MetadataService:
                                    log.Info("Begin restore site level Metadata Service.");
                                    //if (!GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting.CheckRestoreSecurityOnly())
                                    if (AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreProperty) || Config.ContainerConflictResolution == ConflictResolutionType.Merge)
                                    {
                                        log.Info("real start restore site level Metadata Service.");
                                        var termStoreInfos = metadata.GetMetadata<List<AveTermStoreInfo>>();
                                        AveSite.MetadataService = new AveMetadataService(AveSite);
                                        AveSite.MetadataService.SkipGlobalTermGroup = Config.SkipGlobalTermGroup;
                                        AveSite.MetadataService.SkipLocalTermGroup = Config.SkipLocalTermGroup;
                                        if (Config.JobType == 28 || Config.JobType == 60)
                                        {
                                            log.Info("Archiver Job restore site level Metadata Service.");
                                            AveSite.MetadataService.SkipGlobalTermGroup = false;
                                            AveSite.MetadataService.SkipLocalTermGroup = false;
                                        }
                                        AveSite.MetadataService.Restore(termStoreInfos);
                                        //using (var report = AveSite.MetadataService.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveSiteDto));
                                        //}
                                    }
                                    else
                                    {
                                        log.Info("not restore site level Metadata Service.");
                                    }
                                    break;
                                default:
                                    //TO DO
                                    break;
                            }
                        }
                    }


                    AveSite.ReloadSite();
                    reportDto.Path = AveSite.SiteUrl;
                    reportDto.Title = AveSite.SPSite.RootWeb.Title;
                    reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                    log.Info(@"Looks up a localized string similar to Restoring site finished. Site Name: {0}.", aveSiteDto.Name);
                    //if (Config.EventCategory == EventCategorys.DocAveAgentService.StorageOptimization_SP2010_Archiver_Restore
                    //    && ItemRestoreConfig.BPOSSiteCollectionConfig.IsNodeArchivered.ContainsKey(aveSiteDto.SrcUrl)
                    //    && ItemRestoreConfig.BPOSSiteCollectionConfig.IsNodeArchivered[aveSiteDto.SrcUrl] == true)
                    //{
                    //    log.Info("Begin Add Site Collection After Restore");
                    //    string templateTitle = string.Empty;
                    //    IAveWebTemplateCollection webTemplates = AveSite.SPSite.GetWebTemplates(AveSite.SPSite.RootWeb.Language);
                    //    foreach (IAveWebTemplate webTemplate in webTemplates)
                    //    {
                    //        if (string.Equals(webTemplate.Name, AveSite.SPSite.RootWeb.Template, StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            templateTitle = webTemplate.Title;
                    //            break;
                    //        }
                    //    }
                    //    IMArchiverJobManagementService archiverJobManagementService = JobReportServiceFactory.CreateArchiverJobManagementService();
                    //    RemoteSiteCollection siteCollection = new RemoteSiteCollection()
                    //    {
                    //        domain = AveSite.BPOSUserAccountInfo.Domain,
                    //        username = AveSite.BPOSUserAccountInfo.UserName,
                    //        AdminUrl = Config.ArchiverConfigForMedia.AdminUrl == null ? string.Empty : Config.ArchiverConfigForMedia.AdminUrl,
                    //        //agentGroupId = string.Empty,
                    //        url = AveSite.SiteUrl,
                    //        BPOSMould = AveAPIType.BPOS_S.ToString(),
                    //        TemplateName = AveSite.SPSite.RootWeb.Template,
                    //        SPVersion = "15.0.0.0",//AveSite.SPSite.SPVersion,
                    //        Name = AveSite.SPSite.RootWeb.Title,
                    //        TemplateTitle = templateTitle,
                    //        IsPublicWebSite = AveSite.SPSite.IsPublish,
                    //        SiteCollectionType = !AveSite.SPSite.RootWeb.WebTemplate.Equals("TENANTADMIN", StringComparison.OrdinalIgnoreCase) ? SiteCollectionType.Normal : SiteCollectionType.AdminCenter,
                    //        NodeType = RemoveNodeType.SiteCollection,
                    //        ServiceAccountId = string.IsNullOrEmpty(siteAccount.UserName) ? string.Empty : HashCodeHelper.ToMD5HashCode(siteAccount.UserName.ToLowerInvariant()),//Config.ArchiverConfigForMedia.ServiceAccountId,
                    //        TenantId = siteAccount.TenantId,
                    //        AuthType = siteAccount.ConnectionType == global::GCommon.M365Authentication.Contract.M365AuthenticationContract_BposConnectionType.AppToken ? AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken : AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount,
                    //        AppType = GCommon.Contract.CentralAdmin.Object.AppType.Office365,
                    //        ScanSource = RemoteNodeScanSource.AOS,
                    //    };
                    //    if (AveSite.SPSite.RootWeb.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        string starString = "i:0#.f|membership|";
                    //        string ownerEmail = AveSite.SPSite.Owner.LoginName.Substring(starString.Length);  //site 的owner email可能会不存在，通过loginName进行截取。
                    //        siteCollection.NodeType = RemoveNodeType.SkyDrivePro;
                    //        siteCollection.Name = ownerEmail;
                    //    }
                    //    archiverJobManagementService.AddSiteCollectionAfterRestore(siteCollection, Config.ArchiverConfigForMedia.SitesGroupName, Config.TenantGroupId);
                    //    log.Info("Add Site Collection to Site Group,site collection url:{0}", siteCollection.url);
                    //}
                }
                catch (SkipException e)
                {
                    log.Info("An error occurred while restore site. {0}", aveSiteDto.Name, e);
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveSiteDto.SrcUrl);
                    //reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveSiteDto.Name, e.Message);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore site. {0}", aveSiteDto.Name, ex);
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveSiteDto.SrcUrl);
                    //reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(ex, RestoreReportKey.Item_RestoreSiteError.ToString(), RestoreReportResource.Item_RestoreSiteError, aveSiteDto.Name, ex.Message);
                    DisposeSite();
                    Report.IsRootNodeError = true;
                }
                catch (IncorrectUserNameOrPasswordException e)
                {
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveSiteDto.SrcUrl);
                    //reportDto.Status = RestoreStatus.Failed;
                    DisposeSite();
                    Report.IsRootNodeError = true;
                }
                catch (AveSkipLockSiteException e)
                {
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(AveSite.SiteUrl, e));
                    log.Error($"restore site failed,this site locked,error:{e.ToString()}");
                    reportDto.Status = RestoreStatus.Failed;
                    reportDto.ErrorMessage = "RM_AR_Restore_SiteLocked_ErrorMessage";
                    DisposeSite();
                    Report.IsRootNodeError = true;
                }
                catch (AveExceedStorageLimitException e)
                {
                    log.Error($"restore site failed,this site locked,site storage size limit,error:{e.ToString()}");
                    reportDto.Status = RestoreStatus.Failed;
                    reportDto.ErrorMessage = "RM_JM_SiteStorageLimit_ErrorMessage";
                    DisposeSite();
                    Report.IsRootNodeError = true;
                }
                catch (PasswordExpiredException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(AveSite.SiteUrl, e));
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveSiteDto.SrcUrl);
                    Report.IsRootNodeError = true;
                    if (AveSite.BPOSUserAccountInfo == null)
                    {
                        log.Error($"An error occurred while restore site {aveSiteDto.SrcUrl}, BPOSUserAccountInfo is null. Ex: {e}");
                        reportDto.ErrorMessage = "RM_AR_Restore_SiteUserAccountNull_ErrorMessage";
                    }
                    else if (I18NEntity.GetString(e.Message).Equals(e.Message))
                    {
                        reportDto.ErrorMessage = $"An error occurred while restoring the site collection.Error: {e.Message}.";
                    }
                    else
                    {
                        reportDto.ErrorMessage = e.Message;
                    }
                    reportDto.Status = RestoreStatus.Failed;
                    
                    DisposeSite();
                    if (e is SystemException && e.HResult.Equals(0x80070005))//SAAS-22696 如果不throw出去，summary的comment不会有内容
                    {
                        log.Error("An error occurred while restore site. {0}", aveSiteDto.Name, e);
                        AveWrapperI18NException i18nException = e as AveWrapperI18NException;
                        i18nException.Key = WrapperReportResourceKey.Wrapper_AccessDenied.ToString();
                        throw;
                    }
                }
                finally
                {
                    reportDto.SourcePath = aveSiteDto.SrcUrl;
                    CheckFileTail(reportDto);
                    if (isSiteExistInDest == true && aveSiteDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default && reportDto.Status == RestoreStatus.Success)
                    {
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                    reportDto.SetOption(aveSiteDto.RestoreOption.mAveRestoreMode, isSiteExistInDest, reportDto.Status);
                    AddReport(reportDto);
                }

            }

        }

        private void InitOriginalAveObject(RestoreContentDto aveSiteDto, bool useSiteMapping = false)
        {
            log.Info($"Start Init for original ave object");
            AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
            log.Info($"RestoreSiteForOpus the siteAccount info:TenantId:{siteAccount?.TenantId},ClientId:{siteAccount?.ClientId},AuthenticationProfileId:{siteAccount?.AuthenticationProfileId}");
            try
            {
                if (aveSiteDto.SrcName != aveSiteDto.SrcUrl)
                {
                    log.Warn($"Restore dto srcName : {aveSiteDto.SrcName} diff with srcUrl : {aveSiteDto.SrcUrl}");
                }
                log.Info("RestoreSiteForOpus RestoreContentDto Name is:{0},siteAccount is null:{1},SiteURL:{2}.", aveSiteDto.Name, siteAccount == null, aveSiteDto.SrcUrl);
                if (siteAccount == null)
                {
                    log.Info($"siteAccount is null when restore");
                    RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(aveSiteDto.SrcUrl);
                    if (remoteSiteCollection != null && !string.IsNullOrEmpty(remoteSiteCollection.TenantId))
                    {
                        log.Info($"RestoreSiteForOpus remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.");
                        siteAccount = PoolUserUtil.GetBPOSInfo2Async(remoteSiteCollection).Result;
                        log.Info($"RestoreSiteForOpus finished remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
                    }
                    else
                    {
                        string hostURL = new Uri(aveSiteDto.SrcUrl).Scheme + @"://" + new Uri(aveSiteDto.SrcUrl).Authority;
                        RemoteSiteCollection sameTenantRemoteSiteCollection = RemoteNodeDao.GetRemoteSiteCollectionByHostUrl(hostURL);
                        if (sameTenantRemoteSiteCollection != null && !string.IsNullOrEmpty(sameTenantRemoteSiteCollection.TenantId))
                        {
                            log.Info($"RestoreSiteForOpus remoteSiteCollection != null TenantID:{sameTenantRemoteSiteCollection.TenantId}.HostURL:{hostURL}.sameTenantRemoteSiteCollection:{sameTenantRemoteSiteCollection.url}.");
                            siteAccount = PoolUserUtil.GetBPOSInfo2Async(sameTenantRemoteSiteCollection).Result;
                            log.Info($"RestoreSiteForOpus finished remoteSiteCollection != null TenantID:{sameTenantRemoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
                        }
                        else
                        {
                            var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
                            foreach (var temp in profiles)
                            {
                                log.Info($"RestoreSiteForOpus siteAccount == null profile Name is:{temp.Name}.DomainName:{temp.DomainName}.");
                                if (aveSiteDto.Name.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
                                {
                                    var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(temp.TenantId).GetAwaiter().GetResult().AdminUrl;

                                    siteAccount = new Wrapper.Common.AveBPOSAccountInfo()
                                    {
                                        TenantId = temp.TenantId,
                                        AdminUrl = adminUrl,
                                        ClientId = temp.AppClientId,
                                        ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                                        TenantGroupId = TenantLocalValue.LogonGroupId,
                                        AuthenticationProfileId = temp.Id,
                                        AppType = ConvertIdentityTypeToAppType(temp.Type),
                                        AADEnvironment = (Microsoft365.Authentication.AveAzureEnvironment)temp.AADEnvironment,
                                        //AppCert = apponlyCertificate
                                    };
                                    break;
                                }
                            }
                        }
                    }
                }
                //For modern authentication.
                else if (siteAccount != null && siteAccount.ConnectionType == BposConnectionType.ServiceAccount)
                {
                    siteAccount = new AveBPOSAccountInfo()
                    {
                        UserName = string.IsNullOrEmpty(siteAccount.UserName) ? Config.ArchiverConfigForMedia?.UserName : siteAccount.UserName,
                        Password = siteAccount.Password.IsNullOrEmpty() ? CspCommunicationWrapper.UnWrapKeyToSecureString(Config.ArchiverConfigForMedia?.Password) : siteAccount.Password,
                        AdminUrl = string.IsNullOrEmpty(siteAccount.AdminUrl) ? Config.ArchiverConfigForMedia?.AdminUrl : siteAccount.AdminUrl,
                        TenantId = string.IsNullOrEmpty(siteAccount.TenantId) ? null : siteAccount.TenantId,
                        TenantGroupId = siteAccount.TenantGroupId,
                    };
                    if (string.IsNullOrEmpty(siteAccount.UserName))
                    {
                        var firstServiceAccount = RA.Common.Aos.RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, siteAccount.TenantId).FirstOrDefault();
                        siteAccount.UserName = firstServiceAccount?.UserName ?? string.Empty;
                    }
                    if (siteAccount.Password.IsNullOrEmpty())
                    {
                        siteAccount.Password = RA.Common.Aos.RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, siteAccount.UserName).ToSecureStringWithEmptyCheck();
                    }
                    log.Info("Restore site need reset site account. SiteName is:{0},TenantId is:{1},SiteURL:{2}.", aveSiteDto.Name, null, aveSiteDto.SrcUrl);
                }
            }
            catch (Exception eg)
            {
                log.Info("Try get user information & url error {0}", eg.ToString());
            }
            //bool conflictWithRecycleBin = false;
            try
            {
                if (siteAccount == null)
                {
                    log.Error("SiteAccount is null");
                    throw new Exception("Site Account is null");
                }
                //AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
                log.Info("get user for delete recyclebin site {0}.", aveSiteDto.Name);
                AveObjectModelFactory tenantFactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, siteAccount, AveContextKind.ClientObjectModel);
                log.Info("RestoreSiteForOpus.O365 Admin Url is : {0}.", siteAccount.AdminUrl);
                IAveTenant aveTenant = tenantFactory.CreateTenant(siteAccount.AdminUrl);
                var geoLocationInfo = aveTenant.GetTenantGeoLocationinfo();
                if (geoLocationInfo != null && geoLocationInfo.Count > 1)
                {
                    foreach (var location in geoLocationInfo)
                    {
                        if (aveSiteDto.SrcUrl.StartsWith(location.RootSiteUrl) || aveSiteDto.SrcUrl.StartsWith(location.MySiteHostUrl))
                        {
                            siteAccount.AdminUrl = location.TenantAdminUrl;
                            log.Info($"RestoreSiteForOpus.O365 Admin New Url is : {siteAccount.AdminUrl}.SiteUrl:{aveSiteDto.SrcUrl}.");
                        }
                    }
                }
                log.Info("Create Container Admin URL {0}", siteAccount.AdminUrl);
                IAveSite site = tenantFactory.CreateAdminCenterSite(siteAccount.AdminUrl);
                log.Info("Create admin center site: {0} successfully.", siteAccount.AdminUrl);
                var oriTenant = tenantFactory.CreateTenant(site);//aveSiteDto.SiteUrl);
                //isOriginalSiteExist = !IsSiteExistInRecycleBin(oriTenant, aveSiteDto.Name);
                var oriSiteProps = oriTenant.GetSitePropertiesByUrl(aveSiteDto.Name);

                if (oriSiteProps == null)
                {
                    log.Info($"The orginal site may be deleted. SiteUrl {aveSiteDto.Name}");
                    isOriginalSiteExist = false;
                    return;
                }
                var siteUrl = aveSiteDto.Name;
                if (oriSiteProps.Template.StartsWith("REDIRECTSITE#"))
                {
                    log.Info($"The orginal site url may be changed. SiteUrl {aveSiteDto.Name}, Template: {oriSiteProps.Template}");
                    if (useSiteMapping)
                    {
                        var mapping = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(aveSiteDto.SrcName);
                        if (mapping != null && !string.IsNullOrEmpty(mapping.TargetSiteUrl))
                        {
                            log.Info($"this original site has mapping to new site url,source:{aveSiteDto.SrcName},target:{mapping.TargetSiteUrl}");
                            siteUrl = mapping.TargetSiteUrl;
                        }
                    }
                }

                oriAveWeb = null;
                oriAveSite = new AveSPSite(siteUrl, aveSiteDto.ParentName, null, Config.ContextKind, siteAccount);

                isOriginalSiteExist = true;
                log.Info($"Init original site success. SiteUrl {siteUrl}, isOriginalSiteExist: {isOriginalSiteExist}");
            }
            catch (Exception e1)
            {
                if (e1.Message.Contains("Cannot get site"))
                {
                    log.Info($"The orginal site is deleted, no need to tracking it. SiteUrl {aveSiteDto.Name}, Ex: {e1}");
                }
                else
                {
                    log.Error("Init Original Site Error {0}", e1.ToString());
                }
                isOriginalSiteExist = false;
            }
        }

        private void InitNewLangageMapping()
        {
            if (this.Config != null && this.Config.LanguageMappingInfo != null)
            {
                var customLanguageMapping = Config.LanguageMappingInfo.LanguageMappingString;
                if (!string.IsNullOrEmpty(customLanguageMapping))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(customLanguageMapping);
                    XmlNodeList xNodes = xDoc.SelectNodes("/LanguageMapping/Language");
                    if (xNodes.Count > 0)
                    {
                        string id = xNodes[1].Attributes["id"].Value;//现在只有1对1的LanguageMapping，暂时用此方法取到目的端语言。
                        mNewCreatedLCD = int.TryParse(id, out mNewCreatedLCD) ? mNewCreatedLCD : -1;
                    }
                }
            }
        }
        public override void RestoreWeb(RestoreContentDto aveWebDto)
        {
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                RestoreWebForEndUser(aveWebDto);
            }
            else
            {
                RestoreWebForOpus(aveWebDto);
            }
        }
        public void RestoreWebForOpus(RestoreContentDto aveWebDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreWeb"))
            {
                //var needAddReport = true;
                var oriWebName = aveWebDto.Name;
                if (IsRestoreToSPO)
                {
                    if (!string.IsNullOrEmpty(targetWebUrl))
                    {
                        if (!AveWeb.ServerRelativeUrl.Equals(DestInfo.WebPath, StringComparison.OrdinalIgnoreCase))
                        {
                            var subsiteNames = DestInfo.WebPath.Substring(AveSite.ServerRelativeUrl.Length).Trim('/').Split('/');
                            var tempWebPath = AveSite.ServerRelativeUrl;
                            foreach (var name in subsiteNames)
                            {
                                tempWebPath = tempWebPath + "/" + name;
                                this.AveWeb = new AveSPWeb(AveSite, name);
                                AveWeb.GetWebSelf();
                            }
                        }
                        //return;
                    }

                    isSelectedFolderProcessed = false;
                    lastSelectedFolderUrl = null;
                    sourceFolderUrl = null;
                    sourceLibUrl = null;
                    targetWebUrl = WebUtil.MakeFullUrl(targetSiteUrl, DestInfo.WebPath);
                }

                if (!string.IsNullOrEmpty(targetSiteUrl))
                {
                    aveWebDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveWebDto);
                }
                var reportDto = new AveRestoreReportDto { Type = aveWebDto.Type.ToString(), Title = aveWebDto.Name, PathMD5 = aveWebDto.ItemPathMd5 };
                if (AveSite == null)
                {
                    if (aveWebDto.IsSelected)
                    {
                        reportDto.Status = RestoreStatus.ContainerFailed;
                    }
                    else
                    {
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                        //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveWebDto.SrcUrl);
                    reportDto.SourcePath = aveWebDto.SrcUrl;
                    
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindWebParent.ToString(), RestoreReportResource.Item_CanNotFindWebParent, reportDto.Title);
                    AddReport(reportDto);
                    return;
                }
                ProcessPostAction(aveWebDto, ref AveSite, ref AveWeb, ref AveList);
                DisposeWeb();
                AveList = null;
                this.aveFolder = null;
                this.aveListRootFolder = null;
                bool? isWebExistInDest = null;
                try
                {
                    string failedString = "Web";
                    if (HardMakeRestoreFailedValue() == failedString)
                    {
                        log.Warn($"this is hard make restore failed,level :{failedString}");
                        throw new Exception("this is test error");
                    }
                    AveWeb = new AveSPWeb(AveSite, aveWebDto.Name);
                    if (isOriginalSiteExist && oriAveSite?.SPSite != null)
                    {
                        oriAveWeb = new AveSPWeb(oriAveSite, oriWebName);
                        oriAveWeb.GetWebSelf();
                        log.Info($"Original web get success. WebUrl:{oriWebName}");
                    }
                    var securityRestoreOption = new SecurityRestoreOption
                    {
                        PromotePermissionToRootWeb = true,
                        IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                    };
                    GlobalRestoreOptionWorker.CheckWebGlobalSetting(AveSite, AveWeb.Name, aveWebDto, securityRestoreOption);
                    AveWeb.SetRestoreOption(aveWebDto.RestoreOption);
                    //NOTE:we use the the destLanguge of the web's site now.
                    if (Config.IsOutOfPlaceRestore && AveSite.SPSite != null)
                    {
                        if (mNewCreatedLCD != -1)
                        {
                            AveWeb.SetLanguageForNew((uint)mNewCreatedLCD);
                        }
                    }
                    AveWeb.RestoringWeb.IsIncludingRecycleBinData = Config.IncludingRecycleBinData;
                    AveWeb.RestorePermissionLevel = Config.RestorePermissionLevel;
                    AveMetadata metadata;
                    log.Info("Begin restore RestoreWeb Metadata.");
                    while ((metadata = RestoreStream.ReadMetadata()) != null)
                    {
                        switch (metadata.MetadataType)
                        {
                            case AveMetadataType.WebBasicInfo:
                                log.Info("Begin restore web level WebBasicInfo.");
                                var webInfo = metadata.GetMetadata<AveWebInfo>();
                                if (IsRestoreToSPO || IsAdvancedRestore)
                                {
                                    AveWeb.GetWebSelf(webInfo);
                                    break;
                                }
                                if (AveWeb.CheckRestoreOption(AveRestoreMode.Replace) && ReplaceType.Equals(AveConstants.TYPE_WEB))
                                {
                                    bool exist = ReplaceWorker.DeleteWeb(AveSite, AveWeb.Name, Config.IncludeProjectsData);
                                    NullableBooleanExtension.SetIfValueNotExist(ref isWebExistInDest, exist);
                                }
                                try
                                {
                                    AveWeb.RestoreWebSelf(webInfo);
                                    //using (var report = AveWeb.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                finally
                                {
                                    NullableBooleanExtension.SetIfValueNotExist(ref isWebExistInDest, !AveWeb.IsNewCreated);
                                }
                                if (AveWeb.RestoringWeb.NeedSkipped)
                                {
                                    string message = AveWeb.ReportMessage;
                                    reportDto.Status = RestoreStatus.Skipped;
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        reportDto.ErrorMessage = message;
                                    }
                                    return;
                                }
                                break;

                            case AveMetadataType.WebProperty:
                                log.Info("Begin restore web level WebProperty.");
                                var webSetting = metadata.GetMetadata<AveWebSettingInfo>();
                                if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    AveWeb.RestoreWebProperty(webSetting, Config.IncludeCustomPropertyBags);
                                    //using (var report = AveWeb.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    AveWeb.WebSettingInfo = webSetting;
                                }
                                break;

                            case AveMetadataType.MetadataService:
                                log.Info("Begin restore web level MetadataService.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                //if (!GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting.CheckRestoreSecurityOnly())
                                {
                                    log.Info("real start restore web level MetadataService.");
                                    var termStoreInfos = metadata.GetMetadata<List<AveTermStoreInfo>>();
                                    AveSite.MetadataService = new AveMetadataService(AveSite);
                                    AveSite.MetadataService.SkipGlobalTermGroup = Config.SkipGlobalTermGroup;
                                    AveSite.MetadataService.SkipLocalTermGroup = Config.SkipLocalTermGroup;
                                    AveSite.MetadataService.Restore(termStoreInfos);
                                    //using (var report = AveSite.MetadataService.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    log.Info("not restore web level MetadataService.");
                                }
                                break;

                            case AveMetadataType.WebField:
                                log.Info("Begin restore web level WebField.");
                                //if (IsRestoreToSPO)
                                //{
                                //    var fieldSchemaXml = AveWeb.SPWeb.Fields.SchemaXml;
                                //    AveWeb.Fields.LoadFields(fieldSchemaXml);
                                //    break;
                                //}
                                var schemaXml = metadata.GetMetadata<string>();
                                if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    AveWeb.Fields.RestoreFields(schemaXml, Config.FieldRestoreOption);
                                    //using (var report = AveWeb.Fields.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    AveWeb.Fields.LoadFields(schemaXml);
                                }
                                break;

                            case AveMetadataType.WebContentType:
                                log.Info("Begin restore web level WebContentType.");
                                //if (IsRestoreToSPO)
                                //{
                                //    var webCT = AveWeb.SPWeb.ContentTypes.GetContentTypeInfos(true);
                                //    AveWeb.ContentTypes.LoadContentTypes(webCT);
                                //    break;
                                //}
                                var webCTCollectionInfo = metadata.GetMetadata<AveContentTypeCollectionInfo>();
                                if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    //TODO: Get Content Type display Name Mapping
                                    var renameTable = new Dictionary<string, string>();
                                    AveWeb.ContentTypes.RestoreContentTypes(webCTCollectionInfo, renameTable, Config.ContentTypeRestoreOption);
                                    AveWeb.UpdateDocumentSetCT();
                                    //using (var report = AveWeb.ContentTypes.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    AveWeb.ContentTypes.LoadContentTypes(webCTCollectionInfo);
                                }
                                break;

                            case AveMetadataType.Navigation:
                                log.Info("Begin restore web level Navigation.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var navigationInfoList = metadata.GetMetadata<AveNavigationInfoList>();

                                    using (var navManager = new AveSPNavigation(AveWeb))
                                    {
                                        navManager.AddToNavNodesCache(navigationInfoList);
                                    }
                                    //AveWeb.ClearWebNavigation(); // clean all the navigation when over write 在AveNavigationImport的方法中clean navigation
                                    //using (var report = AveWeb.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                break;

                            case AveMetadataType.WebFeature:
                                log.Info("Begin restore web level WebFeature.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var featureInfoBox = metadata.GetMetadata<AveFeatureInfoBox>();
                                    using (var featureManager = new AveSPFeature(AveWeb))
                                    {
                                        featureManager.Restore(featureInfoBox);
                                        //using (var report = featureManager.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                        //}
                                    }
                                }
                                break;

                            case AveMetadataType.Users:
                                log.Info("Begin restore web level Users.");
                                var users = metadata.GetMetadata<List<AveUserInfo>>();
                                if (users != null)
                                {
                                    if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                    {
                                        AveWeb.ParentSite.SPMembers.MultiThreadRestoreUsers(users, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }
                                    else
                                    {
                                        AveWeb.needListRestore = true;
                                        AveSite.SPMembers.LoadUsers(users);
                                    }
                                }
                                break;

                            case AveMetadataType.Groups:
                                log.Info("Begin restore web level Groups.");
                                var groups = metadata.GetMetadata<List<AveGroupInfo>>();
                                if (groups != null)
                                {
                                    log.Info($"restore web,group count is:{groups.Count}.");
                                    foreach (var g in groups)
                                    {
                                        log.Info($"restore web,group name is :{g.Title}.members:{g.Members?.Count}");
                                    }
                                    if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                    {
                                        log.Info($"restore web,restore Groups.");
                                        AveWeb.ParentSite.SPMembers.RestoreGroups(groups, true, Config.ExcludeGroupWithoutPermissions);
                                    }
                                    else
                                    {
                                        log.Info($"restore web,just LoadGroups.");
                                        AveWeb.needListRestore = true;
                                        AveSite.SPMembers.LoadGroups(groups);
                                    }
                                }
                                break;

                            case AveMetadataType.Roles:
                                log.Info("Begin restore web level Roles.");
                                //if (IsRestoreToSPO)
                                //{
                                //    var webRoles = AveWeb.SPWeb.RolesSerializer.GetObjectData() as List<AveRoleInfo>;
                                //    AveWeb.Roles = webRoles;
                                //    break;
                                //}
                                if (!IsRestoreToSPO && AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                {
                                    var roles = metadata.GetMetadata<List<AveRoleInfo>>();
                                    var security = new AveWebSecurity(AveWeb);
                                    if (AveWeb.WebSettingInfo.HasUniqueRoleAssignments.IsAvailable)
                                    {
                                        security.SourceHasUniqueRoleAssignment = AveWeb.WebSettingInfo.HasUniqueRoleAssignments.Value;
                                    }
                                    security.RestoreRoles(roles, securityRestoreOption);
                                    //using (var report = security.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    AveWeb.Roles = metadata.GetMetadata<List<AveRoleInfo>>();
                                    AveWeb.needListRestore = true;
                                }
                                break;

                            case AveMetadataType.RoleAssignment:
                                log.Info("Begin restore web level RoleAssignment.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                {
                                    var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                    var security = new AveWebSecurity(AveWeb);
                                    if (AveWeb.WebSettingInfo.HasUniqueRoleAssignments.IsAvailable)
                                    {
                                        security.SourceHasUniqueRoleAssignment = AveWeb.WebSettingInfo.HasUniqueRoleAssignments.Value;
                                    }
                                    security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                    //using (var report = security.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                else
                                {
                                    AveWeb.needListRestore = true;
                                }
                                break;

                            case AveMetadataType.WebEventReceiver:
                                log.Info("Begin restore web level WebEventReceiver.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var eventReceivers = metadata.GetMetadata<List<AveEventReceiverInfo>>();
                                    AveSPEventReceiver aveEventReceivers = AveSPEventReceiver.CreateInstance(AveWeb);
                                    aveEventReceivers.RestoreEventReceivers(eventReceivers);
                                }
                                break;

                            case AveMetadataType.LanguageFile:
                                log.Info("Begin restore web level LanguageFile.");
                                if (AveSite.AveLanguageProcesser == null)
                                {
                                    break;
                                }
                                var languageInfo = metadata.GetMetadata<AveLanguageInfo>();
                                if (languageInfo != null)
                                {
                                    AveWeb.ParentSite.RestoreLanguageFile(languageInfo);
                                }
                                //add for language mapping
                                if (AveWeb.Name != AveConstants.ROOT_WEB && AveWeb.WebSrcLanguageId != AveWeb.SPWeb.Language && !Config.DisableLanguageMapping)
                                {
                                    if (AveWeb.ParentSite.AveLanguageProcesser != null)
                                    {
                                        AveWeb.ParentSite.AveLanguageProcesser.LoadMapping(
                                            string.Empty, AveWeb.WebSrcLanguageId,
                                            AveWeb.SPWeb.Language, this.Config.LanguageMappingInfo.LanguageMappingString);
                                    }
                                }
                                else if (AveWeb.Name != AveConstants.ROOT_WEB && AveWeb.WebSrcLanguageId == AveWeb.SPWeb.Language
                                         && AveWeb.ParentSite.AveLanguageProcesser != null)
                                {
                                    AveWeb.ParentSite.AveLanguageProcesser.FieldMapping.Clear();
                                    AveWeb.ParentSite.AveLanguageProcesser.ListMapping.Clear();
                                    AveWeb.ParentSite.AveLanguageProcesser.PermissionMapping.Clear();
                                }
                                break;
                            //case AveMetadataType.DocumentTagging:
                            //    if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite) &&
                            //        AveSPEnv.IsMoss)
                            //    {
                            //        var DTs = metadata.GetMetadata<List<AveDocumentTaggingInfo>>();
                            //        var documentTagging = new AveDocumentTagging(AveWeb.SPWeb.Url + "/", AveSite);
                            //        documentTagging.Restore(DTs);
                            //    }
                            //    break;
                            #region Social Tag and Comment
                            //case AveMetadataType.SocialTag:
                            //    if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite) &&
                            //        AveSPEnv.IsMoss)
                            //    {
                            //        List<AveSocialTagInfo> tagInfos = metadata.GetMetadata<List<AveSocialTagInfo>>();
                            //        AveSPSocialTag socialTags = new AveSPSocialTag(AveWeb.SPWeb.Url + "/", AveSite);
                            //        socialTags.Restore(tagInfos);
                            //    }
                            //    break;

                            //case AveMetadataType.SocialComment:
                            //    if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite) &&
                            //        AveSPEnv.IsMoss)
                            //    {
                            //        List<AveSocialCommentInfo> commentInfos = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                            //        AveSPSocialComment socialComment = new AveSPSocialComment(AveWeb.SPWeb.Url + "/", AveSite);
                            //        socialComment.Restore(commentInfos);
                            //    }
                            //    break;
                            #endregion
                            case AveMetadataType.SiteSearchInfo:
                                log.Info("Begin restore web level SiteSearchInfo.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var searchInfo = metadata.GetMetadata<AveSearchInfo>();
                                    if (searchInfo != null)
                                    {
                                        var searchManager = new AveSPSearch(AveWeb);
                                        searchManager.Restore(searchInfo);
                                    }
                                }
                                break;
                            #region workflow
                            case AveMetadataType.WebCTWorkflowAssociation:
                                log.Info("Begin restore web level WebCTWorkflowAssociation.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var ctWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                    WFConflictResolution ctWFResolution = WFConflictResolution.Instance;
                                    ctWFResolution.WebContentTypeAssociation = true;
                                    foreach (AveWorkflowInfo unit in ctWFInfo)
                                    {
                                        try
                                        {
                                            if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite))
                                            {
                                                string contentTypeId = string.Empty;
                                                if ((contentTypeId = AveWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(unit.CTId)) != null)
                                                {

                                                    IAveContentType ct = AveWeb.SPWeb.ContentTypes[Config.ObjectModelFactory.CreateContentTypeId(contentTypeId)];
                                                    if (ct == null)
                                                    {
                                                        log.Warn(string.Format("can't find the specify content type {0} when restore web content type workflow association.", unit.CTName));
                                                    }
                                                    ctWFResolution.AssociationParentObject = ct;
                                                    ctWFResolution.RestoreAssociationData(unit);
                                                }
                                            }
                                            else
                                            {
                                                ctWFResolution.CacheAssociationData(unit);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            log.Warn("Restore Web content type workflow association error." + e.ToString());
                                        }
                                    }
                                    //using (var report = ctWFResolution.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                break;
                            case AveMetadataType.WebWorkflowAssociation:
                                log.Info("Begin restore web level WebWorkflowAssociation.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                {
                                    var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                    var wfResolution = WFConflictResolution.Instance;
                                    wfResolution.WebContentTypeAssociation = false;
                                    wfResolution.AssociationParentObject = AveWeb.SPWeb;
                                    foreach (var unit in wfInfo)
                                    {
                                        if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite))
                                        {
                                            wfResolution.RestoreAssociationData(unit);
                                        }
                                        else
                                        {
                                            wfResolution.CacheAssociationData(unit);
                                        }
                                    }
                                    //using (var report = wfResolution.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                break;
                            case AveMetadataType.ProjectWorkflowAssociation:
                                log.Info("Begin restore web level ProjectWorkflowAssociation.");
                                //if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))
                                //{
                                var projectWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                var projectWFResolution = WFConflictResolution.Instance;
                                projectWFResolution.WebContentTypeAssociation = false;
                                projectWFResolution.AssociationParentObject = AveWeb.SPWeb;
                                foreach (var unit in projectWFInfo)
                                {
                                    if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        projectWFResolution.RestoreProjectAssociationData(unit);
                                    }
                                    else
                                    {
                                        projectWFResolution.CacheProjectAssociationData(unit);
                                    }
                                }
                                //using (var report = projectWFResolution.GetReport())
                                //{
                                //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                //}
                                //}
                                break;

                            case AveMetadataType.WebWorkflowInstance:
                                log.Info("Begin restore web level WebWorkflowInstance.");
                                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.OverWrite))
                                {
                                    var wfInstanceInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                    WFConflictResolution wfInstanceResolution = WFConflictResolution.Instance;
                                    foreach (var unit in wfInstanceInfo)
                                    {
                                        var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                                        wfInstanceResolution.HandleInstanceConflict(wfAssociationUnit, AveWeb.SPWeb);
                                    }
                                    //using (var report = wfInstanceResolution.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveWebDto));
                                    //}
                                }
                                break;
                            #endregion

                            #region Project

                            case AveMetadataType.ProjectCalendar:
                                log.Info("Begin restore web level ProjectCalendar.");
                                var calendarInfos = metadata.GetMetadata<List<AveProjectCalendarInfo>>();

                                break;

                            case AveMetadataType.ProjectLookupTable:
                                log.Info("Begin restore web level ProjectLookupTable.");
                                var lookupTableInfos = metadata.GetMetadata<List<AveProjectLookupTableInfo>>();
                                AveSite.PWASettings.RestoreLookupTable(lookupTableInfos);
                                break;

                            case AveMetadataType.ProjectCustomField:
                                log.Info("Begin restore web level ProjectCustomField.");
                                var customFieldInfos = metadata.GetMetadata<List<AveProjectCustomFieldInfo>>();
                                AveSite.PWASettings.RestoreCustomFields(customFieldInfos);
                                break;

                            case AveMetadataType.ProjectEnterpriseResource:
                                log.Info("Begin restore web level ProjectEnterpriseResource.");
                                var resoureInfos = metadata.GetMetadata<List<AveProjectEnterpriseResourceInfo>>();
                                AveSite.PWASettings.RestoreEnterpriseResource(resoureInfos);
                                break;

                            case AveMetadataType.ProjectPhase:
                                log.Info("Begin restore web level ProjectPhase.");
                                var phaseInfos = metadata.GetMetadata<List<AveProjectPhaseInfo>>();
                                AveSite.PWASettings.RestorePhase(phaseInfos);
                                break;

                            case AveMetadataType.ProjectStage:
                                log.Info("Begin restore web level ProjectStage.");
                                var stageInfos = metadata.GetMetadata<List<AveProjectStageInfo>>();
                                AveSite.PWASettings.CacheStageInfo(stageInfos);
                                break;

                            case AveMetadataType.ProjectTimesheet:
                                //var timeSheetInfos = metadata.GetMetadata<List<AveProjectTimeSheetInfo>>();
                                //AveWeb.SPWeb.PWASettingSerializer.RestoreTimeSheet(timeSheetInfos);
                                break;

                            case AveMetadataType.ProjectEnterpriseProjectType:
                                log.Info("Begin restore web level ProjectEnterpriseProjectType.");
                                var eptInfos = metadata.GetMetadata<List<AveProjectEnterpriseProjectTypeInfo>>();
                                AveSite.PWASettings.CacheEnterpriseProjectType(eptInfos);
                                break;

                            case AveMetadataType.ProjectTimeline:
                                log.Info("Begin restore web level ProjectTimeline.");
                                string timeline = metadata.GetMetadata<string>();
                                AveSite.PWASettings.CacheTimeline(timeline);
                                break;

                            #endregion


                            default:
                                //TODO
                                break;
                        }
                    }
                    RecordRestoredFile.CurrentWebId = AveWeb.SPWeb.ID;
                    reportDto.Title = AveWeb.SPWeb.Title;
                    reportDto.Path = AveWeb.SPWeb.Url;
                    reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                    log.Info(@"Looks up a localized string similar to Restoring web finished. Web Name: {0}.", aveWebDto.Name);
                }
                catch (SkipException e)
                {
                    log.Info(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveWebDto.Name, e);
                    string message = e.Message;
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveWebDto.SrcUrl);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, AveWeb.Name, e.Message);
                    DisposeWeb();
                }
                catch (AveSecurityTrimingException e)
                {
                    log.Warn("An error occurred while restore web. {0}", aveWebDto.Name, e);
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveWebDto.SrcUrl);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_SecurityWebSkipped.ToString(), RestoreReportResource.Item_SecurityWebSkipped, AveWeb.Name, e.Message);
                    DisposeWeb();
                }
                catch (AveWrapperSkipException e)
                {
                    log.Warn("skip to restore app web .{0}", aveWebDto.Name, e);
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveWebDto.SrcUrl);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveWebDto.Name, e.Message);
                    DisposeWeb();
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreWebFailedEventMessage(aveWebDto.Name, e));
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(aveWebDto.SrcUrl);
                    reportDto.Status = RestoreStatus.Failed;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_RestoreWebError.ToString(), RestoreReportResource.Item_RestoreWebError, aveWebDto.Name, e.Message);
                    DisposeWeb();
                }
                finally
                {
                    reportDto.SourcePath = aveWebDto.SrcUrl;
                    CheckFileTail(reportDto);
                    if (isWebExistInDest == true && aveWebDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default && reportDto.Status == RestoreStatus.Success)
                    {
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                    reportDto.SetOption(aveWebDto.RestoreOption.mAveRestoreMode, isWebExistInDest, reportDto.Status);
                    AddReport(reportDto);

                    if (IsRestoreToSPO && !isSendVirtualReport)
                    {
                        reportDto.Size = 0;
                        reportDto.Status = RestoreStatus.Skipped; 
                        if (DestInfo.IsRootWeb)
                        {
                            AddVirtualReport(reportDto);
                        }
                        else
                        {
                            var webNames = AveWeb.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            var tempWebPath = AveSite.SiteUrl;
                            foreach (var name in webNames)
                            {
                                tempWebPath = WebUtil.MakeFullUrl(tempWebPath, name);
                                reportDto.Path = reportDto.SourcePath = tempWebPath;
                                reportDto.Title = name;
                                AddVirtualReport(reportDto);
                            }
                        }
                    }
                }
            }
        }
        private string HardMakeRestoreFailedValue()
        {
            var result = string.Empty;
            var setting = KeyValueDao.GetValueByKey(KeyNameCollection.MakeRestoreFailed);
            if (setting == null) return result;

            return setting.Value;
        }
        public static void TestRoleAssignment(SecurityRestoreOption securityRestoreOption, List<AveRoleAssignmentInfo> roleAssignments, AveSPWeb AveWeb)
        {
            var security = new AveWebSecurity(AveWeb);
            security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
        }
        public override void RestoreList(RestoreContentDto aveListDto)
        {
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                RestoreListForEndUser(aveListDto);
            }
            else
            {
                RestoreListForOpus(aveListDto);
            }
        }
        public void RestoreListForOpus(RestoreContentDto aveListDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreList"))
            {
                if (IsRestoreToSPO)
                {
                    isSelectedFolderProcessed = false;
                    lastSelectedFolderUrl = null;
                    sourceFolderUrl = null;
                    targetListUrl = WebUtil.MakeFullUrl(targetSiteUrl, DestInfo.ListPath);
                }

                if (!string.IsNullOrEmpty(targetSiteUrl))
                {
                    aveListDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveListDto);
                }
                var reportDto = new AveRestoreReportDto { Type = aveListDto.Type.ToString(), Title = ReportAbsolutePath.GetTitle(aveListDto.Name), PathMD5 = aveListDto.ItemPathMd5 };
                if (AveWeb == null || AveWeb.RestoringWeb.NeedSkipped)
                {
                    if (aveListDto.IsAppData)
                    {
                        return;
                    }
                    if (aveListDto.IsSelected)
                    {
                        reportDto.Status = RestoreStatus.ContainerFailed;
                    }
                    else
                    {
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                    reportDto.SourcePath = aveListDto.SrcUrl;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindListParent.ToString(), RestoreReportResource.Item_CanNotFindListParent, reportDto.Title);
                    AddReport(reportDto);
                    return;
                }
                string webNameWithSlash = AveWeb.Name + "\\";
                string listName = aveListDto.Name;
                string subName = string.Empty;
                listName = listName.Substring(webNameWithSlash.Length);
                int pos = listName.IndexOf('\\');
                if (pos >= 0)
                {
                    subName = listName.Substring(pos + 1, listName.Length - pos - 1);
                    listName = listName.Substring(0, pos);
                }
                reportDto.Title = listName;
                //var listSRUrl=AveWeb.ServerRelativeUrl+'/'+listName;
                //reportDto.Path = ReportAbsolutePath.GetListAP(AveSite.SPSite.Url, AveSite.ServerRelativeUrl, listSRUrl);
                ProcessPostAction(aveListDto, ref AveSite, ref AveWeb, ref AveList);

                bool? isListExist = null;
                try
                {
                    this.aveListRootFolder = null;
                    this.aveFolder = null;
                    string failedString = "List";
                    if (HardMakeRestoreFailedValue() == failedString)
                    {
                        log.Warn($"this is hard make restore failed,level :{failedString}");
                        throw new Exception("this is test error");
                    }
                    if (!aveListDto.Name.StartsWith(webNameWithSlash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new AveException(@"Looks up a localized string similar to Illegal list name. Web Name: {0} List Name: {1}.", AveWeb.Name, aveListDto.Name);
                    }
                    if (listName.Equals("TaxonomyHiddenList", StringComparison.OrdinalIgnoreCase))
                    {
                        AveList = new AveSPList(AveWeb, listName) { NeedContinue = false };
                        return;//TaxonomyHiddenList, 不overwrite 或者delete list item 
                    }
                    AveList = new AveSPList(AveWeb, listName);
                    //Add function to decode path. '%1' to '%', '%2' to '\'
                    AveList.DecodeNameForSpecialChar();
                    string nameWithoutSpecialChar = AveConverter.DecodeSpecialChar(listName);
                    aveListDto.SrcName = aveListDto.SrcName.Replace(listName, nameWithoutSpecialChar);
                    reportDto.Title = reportDto.Title.Replace(listName, nameWithoutSpecialChar);

                    //if (IsRestoreToSPO)
                    //{
                    //    AveList.GetListSelf();
                    //    if (AveList.NeedContinue)
                    //    {
                    //        this.aveListRootFolder = new AveSPFolder(AveList, string.Empty);
                    //        aveListRootFolder.InitSPFolder(false);
                    //        this.mListPath = webNameWithSlash + listName;
                    //        //AddParentFolder(this.mListPath, this.mAveListRootFolder);
                    //        this.aveFolder = this.aveListRootFolder;
                    //        if (!string.IsNullOrEmpty(subName))
                    //        {
                    //            this.aveFolder = GenerateFolder(aveFolder, subName);
                    //            this.aveFolder.InitSPFolder();
                    //            //AddParentFolder(this.mListPath + "\\" + subName, this.mAveFolder);
                    //        }
                    //    }
                    //    return;
                    //}

                    var securityRestoreOption = new SecurityRestoreOption()
                    {
                        IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                    };

                    if (string.Compare(listName, "{System Folder}", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        GlobalRestoreOptionWorker.CheckListGlobalSetting(AveWeb, AveList.Name, aveListDto, securityRestoreOption);
                        #region Create field if not exist
                        AveList.AveFields.SetIfCreateFieldIfNotExist(Config.CreateFieldIfNotExist);
                        #endregion
                        AveList.SetRestoreOption(aveListDto.RestoreOption);
                        AppendItemMapping.RemoveAll();
                        AveList.RestoringFolder.IsIncludingRecycleBinData = (Config).IncludingRecycleBinData;
                        AveList.MoveConnectorSetting = !Config.IsOutOfPlaceRestore || Config.JobCategory == (int)PlanCategory.ArchiverRestore;
                        AveMetadata metadata;
                        string fieldSchemaXml = string.Empty; //save the field schema in source list
                        while ((metadata = RestoreStream.ReadMetadata()) != null)
                        {
                            if (!AveList.NeedContinue)
                            {
                                throw new SkipException("Looks up a localized string similar to The system list need skip to be restored..");
                            }
                            switch (metadata.MetadataType)
                            {
                                case AveMetadataType.ListBasicInfo:
                                    var listInfo = metadata.GetMetadata<AveListInfo>();
                                    if (IsRestoreToSPO || IsAdvancedRestore)
                                    {
                                        AveList.GetListSelf(listInfo);
                                        break;
                                    }
                                    if (AveList.CheckRestoreOption(AveRestoreMode.Replace) &&
                                        ReplaceType.Equals(AveConstants.TYPE_LIST) && !AveWeb.IsNewCreated)
                                    {
                                        bool exist = ReplaceWorker.DeleteList(AveWeb, AveList.Name);
                                        NullableBooleanExtension.SetIfValueNotExist(ref isListExist, exist);
                                    }
                                    try
                                    {
                                        if (NeedFindListByUrl(listInfo, listName))
                                        {
                                            AveList.RestoreListSelf(listInfo, true, ListRestoreOption.TitleAndUrl);
                                        }
                                        else
                                        {
                                            AveList.RestoreListSelf(listInfo, true);
                                        }
                                        HandleTeamsChannelDefaulLibrary(AveList);
                                    }
                                    finally
                                    {
                                        NullableBooleanExtension.SetIfValueNotExist(ref isListExist, !AveList.IsNewCreated);
                                    }
                                    break;
                                case AveMetadataType.ListProperty:
                                    var listsetting = metadata.GetMetadata<AveListSettingInfo>();
                                    listSettingInfo = listsetting;
                                    if (!IsRestoreToSPO && AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        AveList.RestoreListProperty(listsetting);
                                        AveList.RestoreListRootFolder();
                                        //AddReport(AveRestoreReportDto.Parse(AveList.GetReport().GetDetails(), aveListDto));
                                    }
                                    else
                                    {
                                        AveList.ListSettingInfo = listsetting;
                                    }
                                    break;

                                #region User & Group Cache, we may need it in the future in item level

                                case AveMetadataType.UserCache:
                                    AveUserList userList = metadata.GetMetadata<AveUserList>();
                                    AveList.ParentWeb.ParentSite.SPMembers.MultiThreadRestoreUsers(userList.Users, false, false, Config.ExcludeGroupWithoutPermissions);
                                    break;
                                //case AveMetadataType.GroupCache:
                                //    AveGroupList groupList = metadata.GetMetadata<AveGroupList>();
                                //    foreach (AveGroupInfo groupInfo in groupList.Groups)
                                //    {
                                //        AveList.ParentWeb.ParentSite.SPMembers.RestoreUsers(groupInfo.Members, false, false, false);
                                //    }
                                //    AveList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                                //    break;
                                #endregion

                                case AveMetadataType.ListField:
                                    //if (IsRestoreToSPO)
                                    //{
                                    //    fieldSchemaXml = AveList.SPList.Fields.SchemaXml;
                                    //    AveList.AveFields.LoadFields(fieldSchemaXml);
                                    //    break;
                                    //}
                                    fieldSchemaXml = metadata.GetMetadata<string>();
                                    if (!IsRestoreToSPO && AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        AveList.AveFields.RestoreFields(fieldSchemaXml, Config.FieldRestoreOption);
                                        //using (var report = AveList.AveFields.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveListDto));
                                        //}
                                    }
                                    else
                                    {
                                        AveList.AveFields.LoadFields(fieldSchemaXml);
                                    }
                                    break;

                                case AveMetadataType.ListContentType:
                                    //if (IsRestoreToSPO)
                                    //{
                                    //    AveList.AveContentTypes.LoadContentTypes(AveList.SPList.ContentTypes.GetContentTypeInfos(true));
                                    //    break;
                                    //}
                                    var ctInfos = metadata.GetMetadata<AveContentTypeCollectionInfo>();
                                    if (!IsRestoreToSPO && AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        try
                                        {
                                            AveList.AveContentTypes.RestoreContentTypes(ctInfos, Config.ContentTypeRestoreOption);
                                            AveList.AveContentTypes.LoadContentTypes(ctInfos);
                                            //using (var report = AveList.AveContentTypes.GetReport())
                                            //{
                                            //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveListDto));
                                            //}
                                        }
                                        catch (Exception e)
                                        {
                                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list content type. ListId:{0}, ListTitle:{1}\n{2}", AveList.SPList.ID,
                                                     AveList.SPList.Title, e));
                                        }
                                    }
                                    else
                                    {
                                        AveList.AveContentTypes.LoadContentTypes(ctInfos);
                                    }
                                    break;

                                case AveMetadataType.RoleAssignment:
                                    if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.RestoreSecurity))
                                    {
                                        try
                                        {
                                            log.Info("Begin restore ListLevel RoleAssignment.");
                                            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                            AveObjectSecurity listSecurity = AveObjectSecurity.CreateInstance(AveList);
                                            if (AveList.ListSettingInfo.HasUniqueRoleAssigntments.IsAvailable)
                                            {
                                                listSecurity.SourceHasUniqueRoleAssignment = AveList.ListSettingInfo.HasUniqueRoleAssigntments.Value;
                                            }
                                            listSecurity.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                            //AddReport(AveRestoreReportDto.Parse(listSecurity.GetReport().GetDetails(), aveListDto));
                                        }
                                        catch (Exception e)
                                        {
                                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list role assignments. ListId:{0}, ListTitle:{1}\n{2}", AveList.SPList == null ? Guid.Empty : AveList.SPList.ID, AveList.SPList == null ? string.Empty : AveList.SPList.Title, e));
                                        }
                                    }
                                    break;

                                case AveMetadataType.ListEventReceiver:
                                    if (AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        var eventReceivers = metadata.GetMetadata<List<AveEventReceiverInfo>>();
                                        AveSPEventReceiver aveEventReceivers =
                                            AveSPEventReceiver.CreateInstance(AveList);
                                        aveEventReceivers.RestoreEventReceivers(eventReceivers);
                                    }
                                    break;

                                case AveMetadataType.DocImmedSubscriptions:
                                    if (AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPListAlert(AveList);
                                        foreach (var iAlertInfo in iAlertInfos)
                                        {
                                            alert.RestoreAlert(iAlertInfo, false);
                                        }
                                    }
                                    break;
                                case AveMetadataType.DocSchedSubscriptions:
                                    if (AveList.CheckRestoreOption(AveList.IsNewCreated,
                                                                   AveRestoreMode.RestoreProperty))
                                    {
                                        var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPListAlert(AveList);
                                        foreach (var sAlertInfo in sAlertInfos)
                                        {
                                            alert.RestoreAlert(sAlertInfo, true);
                                        }
                                    }
                                    break;
                                //case AveMetadataType.SocialTag:
                                //    var listUrl = GetListUrlForNoteBoardWebPart();
                                //    if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.RestoreProperty) && AveSPEnv.IsMoss && listUrl != string.Empty)
                                //    {
                                //        List<AveSocialTagInfo> tags = metadata.GetMetadata<List<AveSocialTagInfo>>();
                                //        AveSPSocialTag socialTag = new AveSPSocialTag(listUrl, AveList.ParentWeb.ParentSite);
                                //        socialTag.Restore(tags);
                                //    }
                                //    break;
                                //case AveMetadataType.SocialComment:
                                //    listUrl = GetListUrlForNoteBoardWebPart();
                                //    if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.RestoreProperty) && AveSPEnv.IsMoss && listUrl != string.Empty)
                                //    {
                                //        List<AveSocialCommentInfo> comments = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                                //        AveSPSocialComment socialComment = new AveSPSocialComment(listUrl, AveList.ParentWeb.ParentSite);
                                //        socialComment.Restore(comments);
                                //    }
                                //    break;
                                case AveMetadataType.ListWorkflowAssociation:
                                    try
                                    {
                                        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                        WFConflictResolution wfResolution = WFConflictResolution.Instance;
                                        wfResolution.WebContentTypeAssociation = false;
                                        wfResolution.AssociationOption = WFAssociationConflictResolutionOption.UpdateOverwrite;
                                        wfResolution.AssociationParentObject = AveList.SPList;
                                        foreach (AveWorkflowInfo unit in wfInfo)
                                        {
                                            if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.OverWrite))
                                            {
                                                wfResolution.RestoreAssociationData(unit);
                                            }
                                            else
                                            {
                                                wfResolution.CacheAssociationData(unit);
                                            }
                                        }
                                        //using (var report = wfResolution.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveListDto));
                                        //}
                                        break;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error("An error occurred while restoring list workflow association. ListId:{0}, ListTitle:{1}, e: {2}", AveList?.SPList?.ID, AveList?.SPList?.Title, e);
                                        if (IsRestoreToSPO)
                                        {
                                            break;
                                        }
                                        throw;
                                    }
                                case AveMetadataType.ListCTWorkflowAssociation:
                                    var ctWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                    WFConflictResolution ctWFResolution = WFConflictResolution.Instance;
                                    ctWFResolution.AssociationOption = WFAssociationConflictResolutionOption.UpdateOverwrite;
                                    ctWFResolution.WebContentTypeAssociation = false;
                                    foreach (AveWorkflowInfo unit in ctWFInfo)
                                    {
                                        if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.OverWrite))
                                        {
                                            if (AveList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(unit.CTId))
                                            {
                                                IAveContentType ct = AveList.SPList.ContentTypes.GetById(AveList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[unit.CTId].ToString());//SAAS-21766 由于保存整个CT占空间比较大，所以我们只保存ID，然后去获取
                                                ctWFResolution.AssociationParentObject = ct;
                                                ctWFResolution.RestoreAssociationData(unit);
                                            }
                                        }
                                        else
                                        {
                                            ctWFResolution.CacheAssociationData(unit);
                                        }
                                    }
                                    //using (var report = ctWFResolution.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveListDto));
                                    //}
                                    break;
                                case AveMetadataType.MetadataService:
                                    //if (!GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting.CheckRestoreSecurityOnly())
                                    if (AveList.CheckRestoreOption(AveList.IsNewCreated, AveRestoreMode.RestoreProperty))
                                    {
                                        log.Info("real start restore list MetadataService");
                                        var termStoreInfos = metadata.GetMetadata<List<AveTermStoreInfo>>();
                                        AveList.ParentSite.MetadataService = new AveMetadataService(AveList.ParentSite);
                                        AveList.ParentSite.MetadataService.SkipGlobalTermGroup = Config.SkipGlobalTermGroup;
                                        AveList.ParentSite.MetadataService.SkipLocalTermGroup = Config.SkipLocalTermGroup;
                                        AveList.ParentSite.MetadataService.Restore(termStoreInfos);
                                        //using (var report = AveSite.MetadataService.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveListDto));
                                        //}
                                    }
                                    else
                                    {
                                        log.Info("not restore list MetadataService");
                                    }
                                    break;
                                //case AveMetadataType.ProjectBasic:
                                //    var projectInfo = metadata.GetMetadata<AveProjectInfo>();
                                //    if (AveList.CheckRestoreOption(AveList.IsNewCreated,
                                //                                   AveRestoreMode.RestoreProperty))
                                //    {
                                //        var project = new AveSPProject(AveSite, projectInfo.Name);
                                //        project.ImportTaskListProject(projectInfo, RestoreStream, AveList.SPList.ID, aveListDto.RestoreOption);
                                //    }
                                //    break;
                                default:
                                    break;
                            }

                        }

                        //for items under the list
                        if (AveList.NeedContinue)
                        {
                            if (AveList.SPList != null)
                            {
                                string schemaXml = string.IsNullOrEmpty(fieldSchemaXml)
                                                       ? AveList.SPList.Fields.SchemaXml
                                                       : fieldSchemaXml;
                                AveList.AveFields.LoadFields(schemaXml);
                            }
                        }
                    }

                    try
                    {
                        if (AveList.NeedContinue)
                        {
                            this.aveListRootFolder = new AveSPFolder(AveList, string.Empty);
                            this.mListPath = webNameWithSlash + listName;
                            //AddParentFolder(this.mListPath, this.mAveListRootFolder);
                            this.aveFolder = this.aveListRootFolder;
                            if (!string.IsNullOrEmpty(subName))
                            {
                                this.aveFolder = GenerateFolder(aveFolder, subName);
                                this.aveFolder.InitSPFolder();
                                //AddParentFolder(this.mListPath + "\\" + subName, this.mAveFolder);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn(@"Looks up a localized string similar to An error occurred while restoring a list. Title: {0}{1},{2}", AveWeb.Name, aveListDto.Name, e.ToString());
                    }
                    //确保还原成功的时候url能打开。
                    if (AveList != null && AveList.RootFolder != null)
                    {
                        if (AveList.SPList != null && !string.IsNullOrEmpty(AveList.SPList.Title))
                        {
                            reportDto.Title = AveList.SPList.Title;
                        }
                        reportDto.Path = AveWeb.SPWeb.Url + '/' + AveList.RootFolder.Url;
                    }
                    reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                    log.Info(@"Looks up a localized string similar to Restoring list finished. List Name: {0}.", aveListDto.Name);
                }
                catch (SkipException e)
                {
                    log.Info(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveListDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    if (I18NEntity.HasKey(e.Key))
                    {
                        reportDto.ErrorMessage = e.Key;
                    }
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveListDto.Name, e.Message);
                    AveList = null;
                }
                catch (AveSecurityTrimingException e)
                {
                    log.Warn("An error occurred while restore list self. {0}", aveListDto.Name, e.Message);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_SecurityListSkipped.ToString(), RestoreReportResource.Item_SecurityListSkipped, aveListDto.Name, AveWeb.Name, e.Message);
                    AveList = null;
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreListFailedEventMessage(AveList?.Name, e));
                    reportDto.Status = RestoreStatus.Failed;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_RestoreListError.ToString(), RestoreReportResource.Item_RestoreListError, AveWeb.Name, aveListDto.Name, e.Message);
                    if(I18NEntity.HasKey(e.Message))
                    {
                        reportDto.ErrorMessage = e.Message;
                    }

                    if (e.Message.Contains("This site has the maximum number of lists and libraries"))
                    {
                        reportDto.ErrorMessage = "RM_JM_RestoreFaild_OutOfListCountLimit_ErrorMessage";
                    }
                    else if(e.Message.Equals("RM_RS_SkipRestoreSiteAppCatalogBecauseUnEnable"))
                    {
                        reportDto.Status = RestoreStatus.Exception;
                    }
                    AveList = null;
                }
                finally
                {
                    if (AveList == null || AveList.NeedContinue)//AveList.NeedContinue is false, do not add to report.
                    {
                        reportDto.SourcePath = aveListDto.SrcUrl;
                        CheckFileTail(reportDto);
                        if (isListExist == true && aveListDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default && reportDto.Status == RestoreStatus.Success)
                        {
                            if (!AveSite.IsNewCreated || !string.Equals(AveList?.RootFolder?.Url ?? "", "Shared Documents", StringComparison.OrdinalIgnoreCase))
                            {
                                reportDto.Status = RestoreStatus.Skipped;
                            }
                        }

                        if (IsRestoreToSPO && isSendVirtualReport)
                        {
                            log.Info("Already send report for this list, skip. ListName: {0}.", listName);
                        }
                        else
                        {
                            reportDto.SetOption(aveListDto.RestoreOption.mAveRestoreMode, isListExist, reportDto.Status);
                            if (!string.Equals(listName, "{System Folder}", StringComparison.OrdinalIgnoreCase))
                            {
                                AddReport(reportDto);
                                isSendVirtualReport = IsRestoreToSPO && string.IsNullOrEmpty(DestInfo.FolderPath) && !string.IsNullOrEmpty(DestInfo.ListPath) && string.Equals(DestInfo.ListPath, AveList.RootFolder.ServerRelativeUrl);
                            }
                        } 
                    }
                }
            }

        }

        public void HandleTeamsChannelDefaulLibrary(AveSPList aveList)
        {
            var changed = false;
            try
            {
                if (/*Config.JobType != (int)JobType.TeamsArchiverRestore || */ _isHandledChannelSiteDefaultLib) return;

                if (!aveList.IsNewCreated && TeamsRestoreState.IsChannelSiteDefaultLibrary(aveList.Url, out var isNewLyCreated))
                {
                    aveList.IsNewCreated = isNewLyCreated;
                    log.Info("Teams channel site default library, set IsNewCreated to {0}. ListUrl: {1}.", isNewLyCreated, aveList.Url);
                    changed = true;
                }
                //...
                if (changed)
                {
                    _isHandledChannelSiteDefaultLib = true;
                    log.Info("Handled Teams channel site default library. ListName: {0}.", aveList.Url);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while HandleTeamsChannelDefaulLibrary. ListName:{0}, error message: {1}", aveList.Name, e);
            }
        }

        /// <summary>
        /// OneDrive备份时，将这两个List的title写死成了Documents和SiteAssets。 考虑到老数据，在还原时处理。
        /// </summary>
        /// <param name="listInfo"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        private bool NeedFindListByUrl(AveListInfo listInfo, string listTitle)
        {
            if (listInfo.BaseTemplate == 700 ||//Documents list in OneDrive.
                listInfo.BaseTemplate == 101 && listTitle.Equals("SiteAssets", StringComparison.OrdinalIgnoreCase))//Site Assets list in OneDrive.
            {
                return true;
            }
            return false;
        }

        public override void RestoreProject(RestoreContentDto projectDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreEnterpriseProject"))
            {
                string projectName = projectDto.Name.Contains("\\") ? projectDto.Name.Substring(projectDto.Name.LastIndexOf("\\") + 1) : projectDto.Name;
                var reportDto = new AveRestoreReportDto { Type = projectDto.Type.ToString(), Title = ReportAbsolutePath.GetTitle(projectName) };
                if (AveSite == null)
                {
                    //reportDto.Title = ReportAbsolutePath.GetReportTitle(projectDto.SrcUrl);
                    reportDto.SourcePath = projectDto.SrcUrl;
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindProjectParent.ToString(), RestoreReportResource.Item_CanNotFindProjectParent, reportDto.Title);
                    AddReport(reportDto);
                    return;
                }
                if (AveWeb == null || AveWeb.RestoringWeb.NeedSkipped)
                {
                    reportDto.SourcePath = projectDto.SrcUrl;
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindProjectParent.ToString(), RestoreReportResource.Item_CanNotFindProjectParent, reportDto.Title);
                    AddReport(reportDto);
                    return;
                }
                reportDto.SourcePath = projectDto.SrcUrl;
                reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                bool? isProjectExist = null;
                try
                {
                    ProcessPostAction(projectDto, ref AveSite, ref AveWeb, ref AveList);

                    AveSPProject aveSPProject = new AveSPProject(AveSite, projectName);
                    aveSPProject.SetRestoreOption(projectDto.RestoreOption);

                    if (aveSPProject.CheckRestoreOption(AveRestoreMode.Default))
                    {
                        if (AveSite.SPSite.Projects.GetByName(projectName) != null)
                        {
                            throw new SkipException(WrapperReportResourceKey.Wrapper_SkipProject.ToString(), WrapperRestoreResource.Wrapper_SkippedProject);
                        }
                    }
                    AveMetadata metadata = null;
                    while ((metadata = RestoreStream.ReadMetadata()) != null)
                    {
                        switch (metadata.MetadataType)
                        {
                            case AveMetadataType.UserCache:
                                AveUserList userList = metadata.GetMetadata<AveUserList>();
                                AveSite.SPMembers.MultiThreadRestoreUsers(userList.Users, false, false, Config.ExcludeGroupWithoutPermissions);
                                break;

                            case AveMetadataType.ProjectBasic:
                                var projectInfo = metadata.GetMetadata<AveProjectInfo>();
                                if (aveSPProject.CheckRestoreOption(AveRestoreMode.Replace) &&
                                       ReplaceType.Equals(AveConstants.TYPE_PROJECT) && !AveWeb.IsNewCreated)
                                {
                                    bool exist = ReplaceWorker.DeleteProjet(AveWeb, projectName);
                                    NullableBooleanExtension.SetIfValueNotExist(ref isProjectExist, exist);
                                }
                                try
                                {
                                    aveSPProject.Import(RestoreStream, projectInfo, projectDto.RestoreOption);
                                    AveSite.MappingManager.ProjectMappingManager.AddProjectTaskIdMapping(projectInfo.OriginalId, projectInfo.NewId);
                                    AveSite.MappingManager.ProjectMappingManager.AddProjectTaskIdMapping(projectInfo.SummaryTaskId, projectInfo.NewSummaryTaskId);
                                }
                                finally
                                {
                                    NullableBooleanExtension.SetIfValueNotExist(ref isProjectExist, !projectInfo.IsNewCreated);
                                }
                                break;
                            default:
                                break;
                        }
                    }


                }
                catch (SkipException e)
                {
                    log.Info(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", projectName, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, projectDto.Name, e.Message);
                }
                catch (Exception ex)
                {
                    log.Warn("restore project failed. project name:{0}, error message:{1}", projectName, ex.ToString());
                    reportDto.Status = RestoreStatus.Failed;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(ex, RestoreReportKey.Item_RestoreProjectError.ToString(), RestoreReportResource.Item_RestoreProjectError, projectDto.Name, ex.Message);
                }
                finally
                {
                    CheckFileTail(reportDto);
                    reportDto.SetOption(projectDto.RestoreOption.mAveRestoreMode, isProjectExist, reportDto.Status);
                    AddReport(reportDto);
                }
            }
        }

        /// <summary>
        /// 只还原App Definition
        /// </summary>
        /// <param name="aveAppDto"></param>
        [SuppressMessage("Microsoft.Globalization", "CA1307:StringComparison")]
        public override void RestoreApp(RestoreContentDto aveAppDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreApp"))
            {
                var reportDto = new AveRestoreReportDto { Type = aveAppDto.Type.ToString(), Title = ReportAbsolutePath.GetTitle(aveAppDto.Name) };
                reportDto.SourcePath = aveAppDto.SrcUrl;
                reportDto.Size = 0;
                AveRestoreMode realRestoreMode = AveRestoreMode.Default;
                if (AveWeb == null || AveWeb.RestoringWeb.NeedSkipped)
                {
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindAppParent.ToString(), RestoreReportResource.Item_CanNotFindAppParent, reportDto.Title);
                    AddReport(reportDto);
                    return;
                }
                try
                {
                    appManager = new AveSPAppManager(AveWeb);
                    appManager.SetRestoreOption(aveAppDto.RestoreOption);
                    AveMetadata metadata;
                    while ((metadata = RestoreStream.ReadMetadata()) != null)
                    {
                        switch (metadata.MetadataType)
                        {
                            case AveMetadataType.AppPackageInfo:

                                var appPackageInfo = metadata.GetMetadata<AveAppPackageInfo>();
                                appManager.SetStream(base.RestoreStream);
                                realRestoreMode = (AveRestoreMode)appManager.RestoreAppSelf(appPackageInfo);
                                break;
                            //other app info, for example app security
                            default:
                                //should not be here
                                break;
                        }
                    }
                    if (appManager.AppInstance != null)
                    {
                        if (appManager.AppInstance.AppWebFullUrl == null)
                        {
                            log.Info("app url: {0}", appManager.AppInstance.LaunchUrl);
                            reportDto.Path = (appManager.AppInstance.LaunchUrl != null && appManager.AppInstance.LaunchUrl.IsAbsoluteUri) ? appManager.AppInstance.LaunchUrl.GetLeftPart(UriPartial.Authority) : string.Empty;
                        }
                        else
                        {
                            reportDto.Path = appManager.AppInstance.AppWebFullUrl.ToString();
                        }
                        reportDto.Status = RestoreStatus.Success;
                    }
                }
                catch (AveWrapperSkipException e)
                {
                    log.Warn("skip to restore app.{0}", aveAppDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveAppDto.Name, e.Message);
                }
                catch (Exception ex)
                {
                    log.Warn("Restore App Error. {0}", ex.ToString());
                    reportDto.Status = RestoreStatus.Failed;
                    if (I18NEntity.GetString(ex.Message) != ex.Message)
                    {
                        reportDto.ErrorMessage = ex.Message;
                    }
                    else if (ex.InnerException != null && I18NEntity.GetString(ex.InnerException.Message) != ex.InnerException.Message)
                    {
                        reportDto.ErrorMessage = ex.InnerException.Message;
                    }
                }
                finally
                {
                    reportDto.SetOption(aveAppDto.RestoreOption.mAveRestoreMode, realRestoreMode != AveRestoreMode.Restore, reportDto.Status);
                    AddReport(reportDto);
                }
            }
        }


        public override void RestoreFolder(RestoreContentDto aveFolderDto)
        {
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                RestoreFolderForEndUser(aveFolderDto);
            }
            else
            {
                RestoreFolderForOpus(aveFolderDto);
            }
        }
        public void RestoreFolderForOpus(RestoreContentDto aveFolderDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreFolder"))
            {
                var isDestFolder = false;
                if (IsRestoreToSPO)
                {
                    var destFolderPath = string.IsNullOrEmpty(DestInfo.FolderPath) ? DestInfo.ListPath : DestInfo.FolderPath;
                    if (string.IsNullOrEmpty(targetFolderUrl))
                    {
                        targetFolderUrl = WebUtil.MakeFullUrl(targetSiteUrl, destFolderPath);
                    }

                    // validate for any descendant, not just direct children
                    var isParentFolderProcessed = !string.IsNullOrEmpty(sourceFolderUrl) && !string.IsNullOrEmpty(lastSelectedFolderUrl) 
                        && isSelectedFolderProcessed 
                        && aveFolderDto.SrcUrl.StartsWith(lastSelectedFolderUrl + "/", StringComparison.OrdinalIgnoreCase);

                    if (!aveFolderDto.ParentIsSelected && !isParentFolderProcessed)
                    {
                        SetSourceFolderUrl(aveFolderDto, true);
                    }

                    //lastSelectedFolderUrl = aveFolderDto.SrcUrl;

                    InitTargetParentFolders(destFolderPath);

                    if (!aveFolderDto.IsSelected && !aveFolderDto.ParentIsSelected && !isParentFolderProcessed)
                    {
                        return;
                    }
                    isDestFolder = true;
                }

                if (!string.IsNullOrEmpty(targetSiteUrl))
                {
                    aveFolderDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveFolderDto);
                }
                var reportDto = new AveRestoreReportDto { Type = aveFolderDto.Type.ToString(), Title = ReportAbsolutePath.GetTitle(aveFolderDto.Name), PathMD5 = aveFolderDto.ItemPathMd5 }; //Path = aveFolderDto.Name
                if (AveList != null && AveList.NeedContinue == false)
                {
                    //List Skipped,we should not add item\folder under the list to report.
                    return;
                }
                if (AveList == null || this.aveListRootFolder == null)
                {
                    if (aveFolderDto.IsAppData)
                    {
                        return;
                    }
                    if (aveFolderDto.IsSelected)
                    {
                        reportDto.Status = RestoreStatus.ContainerFailed;
                    }
                    else
                    {
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                    reportDto.SourcePath = aveFolderDto.SrcUrl;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindFolderParent.ToString(), RestoreReportResource.Item_CanNotFindFolderParent, aveFolderDto.Name);
                    AddReport(reportDto);
                    return;
                }
                bool? isFolderExist = null;
                try
                {
                    string failedString = "Folder";
                    if (HardMakeRestoreFailedValue() == failedString)
                    {
                        log.Warn($"this is hard make restore failed,level :{failedString}");
                        throw new Exception("this is test error");
                    }
                    string parentPath = this.mListPath;
                    if (!aveFolderDto.Name.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new AveException(@"Looks up a localized string similar to The folder does not belong to the current list. Folder Path: {0} List Path: {1}.", aveFolderDto.Name, this.mListPath);
                    }
                    string subPath = aveFolderDto.Name.Substring(parentPath.Length).TrimStart('\\');
                    string nameWithoutSpecialChar = AveConverter.DecodeSpecialChar(mListPath);
                    aveFolderDto.SrcName = aveFolderDto.SrcName.Replace(mListPath, nameWithoutSpecialChar);
                    reportDto.Title = reportDto.Title.Replace(mListPath, nameWithoutSpecialChar);
                    var currentFolder = aveFolder;
                    this.aveFolder = null;
                    this.aveFolder = GenerateFolder(currentFolder, subPath);

                    //if (IsRestoreToSPO && !isDestFolder)
                    //{
                    //    aveFolder.InitSPFolder(aveFolderDto.ParentIsSelected || aveFolderDto.IsSelected);
                    //    return;
                    //}

                    if (aveFolder.ParentFolder != null && (aveFolder.ParentFolder.SPFolder == null || !aveFolder.ParentFolder.SPFolder.Exists))
                    {
                        aveFolder.ParentFolder.InitSPFolder(true);
                    }
                    var securityRestoreOption = new SecurityRestoreOption()
                    {
                        IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                    };
                    GlobalRestoreOptionWorker.CheckFolderGlobalSetting(aveFolder, aveFolderDto, securityRestoreOption);
                    this.aveFolder.SetRestoreOption(aveFolderDto.RestoreOption);
                    aveFolder.ParentList.BackupListSetting();
                    this.aveFolder.RestoringItem.IsIncludingRecycleBinData = (Config).IncludingRecycleBinData;

                    if (string.IsNullOrEmpty(subPath)) //Restore to list Root Folder
                    {
                    }
                    else if (string.Compare(aveFolderDto.Name, "{System Folder}", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.aveFolder.InitSPFolder();
                    }
                    else
                    {
                        AveMetadata metadata;
                        var data = new Dictionary<string, object>();
                        //this.aveFolder.ParentList.BackupListSetting();
                        while ((metadata = RestoreStream.ReadMetadata()) != null)
                        {
                            switch (metadata.MetadataType)
                            {
                                case AveMetadataType.DocProperty:
                                    data.Clear();
                                    metadata.GetMetadata(data);
                                    #region User & Group Cache, we may need it in the future in item level

                                    AveMetadata userCacheMetadata = RestoreStream.TryReadMetadata(AveMetadataType.UserCache);
                                    if (userCacheMetadata != null)
                                    {
                                        var userList = userCacheMetadata.GetMetadata<AveUserList>();
                                        this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.MultiThreadRestoreUsers(userList.Users, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }

                                    AveMetadata groupCacheMetadata = RestoreStream.TryReadMetadata(AveMetadataType.GroupCache);
                                    if (groupCacheMetadata != null && WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks)
                                    {
                                        AveGroupList groupList = groupCacheMetadata.GetMetadata<AveGroupList>();
                                        this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                                    }

                                    #endregion
                                    try
                                    {
                                        //restore document MMS
                                        var metaData = RestoreStream.TryReadMetadata(AveMetadataType.MetadataService);
                                        if (metaData != null)
                                        {
                                            List<AveTermStoreInfo> termStoreInfos = metaData.GetMetadata<List<AveTermStoreInfo>>();
                                            this.aveFolder.ParentSite.MetadataService.Restore(termStoreInfos);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error("Failed restore document meta data, due to {0}", e);
                                    }
                                    AveMetadata userDataMetadata = RestoreStream.TryReadMetadata(AveMetadataType.DocData);
                                    var userData = new Dictionary<string, object>();
                                    if (userDataMetadata != null)
                                    {
                                        userDataMetadata.GetMetadata(userData);
                                    }
                                    AveMetadata dataJuntionMetadata = RestoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                                    List<Dictionary<string, object>> dataJunction = null;
                                    if (dataJuntionMetadata != null)
                                    {
                                        dataJunction = dataJuntionMetadata.GetMetadata<List<Dictionary<string, object>>>();
                                    }
                                    #region Item Dependency
                                    ItemLevelRestoreItemCTAndFields(userData, dataJunction, aveFolder);
                                    #endregion
                                    if (this.aveFolder.CheckRestoreOption(AveRestoreMode.Default) && ReplaceWorker.ExistFolder(AveList, this.aveFolder))
                                    {
                                        log.Info("Skip restore folder {0} because it already exists.", this.aveFolder?.SPFolder?.ServerRelativeUrl);
                                        isFolderExist = true;
                                        continue;
                                    }
                                    if (this.aveFolder.CheckRestoreOption(AveRestoreMode.Replace) &&
                                        ReplaceType.Equals(AveConstants.TYPE_FOLDER))
                                    {
                                        bool exist = ReplaceWorker.DeleteFolder(AveList, this.aveFolder);
                                        NullableBooleanExtension.SetIfValueNotExist(ref isFolderExist, exist);
                                    }
                                    try
                                    {
                                        this.aveFolder.RestoreSelf(data, userData, dataJunction);
                                        //using (var report = this.aveFolder.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    finally
                                    {
                                        NullableBooleanExtension.SetIfValueNotExist(ref isFolderExist, !this.aveFolder.IsNewCreated);
                                    }
                                    break;

                                case AveMetadataType.RoleAssignment:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.RestoreSecurity) || GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting.CheckRestoreSecurityOnly())
                                    {
                                        log.Info("Begin restore FolderLevel RoleAssignment.");
                                        var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                        AveObjectSecurity security = AveObjectSecurity.CreateInstance(this.aveFolder.AveSPItem);
                                        security.SourceHasUniqueRoleAssignment = aveFolder.AveSPItem.HasUniqueRoleAssignments;
                                        security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                        //using (var report = security.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;

                                case AveMetadataType.DocImmedSubscriptions:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPFolderAlert(this.aveFolder);
                                        foreach (var iAlertInfo in iAlertInfos)
                                        {
                                            alert.RestoreAlert(iAlertInfo, false);
                                        }
                                        //using (var report = alert.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;

                                case AveMetadataType.DocSchedSubscriptions:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPFolderAlert(this.aveFolder);
                                        foreach (var sAlertInfo in sAlertInfos)
                                        {
                                            alert.RestoreAlert(sAlertInfo, true);
                                        }
                                        //using (var report = alert.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;
                                //#region Social Tag and Comment
                                //case AveMetadataType.SocialTag:
                                //    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite) &&
                                //        AveEnv.IsMoss)
                                //    {
                                //        List<AveSocialTagInfo> tagInfos = metadata.GetMetadata<List<AveSocialTagInfo>>();
                                //        AveSPSocialTag socialTags = new AveSPSocialTag(this.aveFolder.TagUrl, this.aveFolder.ParentSite);
                                //        socialTags.Restore(tagInfos);
                                //    }
                                //    break;

                                //case AveMetadataType.SocialComment:
                                //    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite) &&
                                //        AveEnv.IsMoss)
                                //    {
                                //        List<AveSocialCommentInfo> commentInfos = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                                //        AveSPSocialComment socialComment = new AveSPSocialComment(this.aveFolder.TagUrl, this.aveFolder.ParentSite);
                                //        using (new AvePerformanceScope("GranularRestore.RestoreDocument.SocialComment"))
                                //        {
                                //            socialComment.Restore(commentInfos);
                                //        }
                                //    }
                                //    break;
                                //#endregion
                                case AveMetadataType.WorkflowInstance:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                        WFConflictResolution wfResolution = WFConflictResolution.Instance;
                                        foreach (var unit in wfInfo)
                                        {
                                            var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                                            wfResolution.HandleInstanceConflict(wfAssociationUnit, aveFolder.AveSPItem.SPListItem);
                                        }
                                        //using (var report = wfResolution.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    reportDto.Path = AveWeb.SPWeb.Url + '/' + aveFolder.SPFolder.Url;
                    reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                    //log.Info(RestoreResource.Item_AIRRestoreFolderCurrentFolder, aveFolderDto.Name);
                }
                catch (SkipException e)
                {
                    log.Warn("Skip this folder while restore folder Name:{0} ,Error:{1}", aveFolderDto.Name, e.Message);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveFolder.Name, e.Message);
                    //this.aveFolder = null;
                }
                catch (AveSecurityTrimingException e)
                {
                    log.Warn("An error occurred while restore folder. Name:{0} ,Error:{1}", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_SecurityFolderSkipped.ToString(), RestoreReportResource.Item_SecurityFolderSkipped, aveFolderDto.Name, e.Message);
                    this.aveFolder = null;
                }
                catch (TeamChannalFolderUpdateFailed e)
                {
                    log.Warn("An error occurred while restore folder,TeamChannalFolderUpdateFailed. Name:{0} ,Error:{1}", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    reportDto.ErrorMessage = "RM_RS_RestoreChannelFolderError";
                    //this.aveFolder = null;
                }
                catch (Exception e)
                {
                    log.Error(@"Looks up a localized string similar to An error occurred while restoring a folder. Path: {0} {1}.", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Failed;
                    if (e.Message != null && e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                    {
                        reportDto.ErrorMessage = "StorageOptimization13_SOARDeleteOfficeLockFile";
                    }
                    else if (e.Message != null && e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                    {
                        reportDto.ErrorMessage = "StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed";
                    }
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_RestoreFolderErrorReport.ToString(), RestoreReportResource.Item_RestoreFolderErrorReport, aveFolderDto.Name, e.Message);
                    this.aveFolder = null;
                }
                reportDto.SourcePath = aveFolderDto.SrcUrl;
                if (isFolderExist == true && aveFolderDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default && reportDto.Status == RestoreStatus.Success)
                {
                    reportDto.Status = RestoreStatus.Skipped;
                }
                reportDto.SetOption(aveFolderDto.RestoreOption.mAveRestoreMode, isFolderExist, reportDto.Status);
                if (!AveList.IsSystemList)
                {
                    AddReport(reportDto);
                }

                if (IsRestoreToSPO && isDestFolder)
                {
                    AddVirtualReport(reportDto);
                }
            }
        }

        protected void InitTargetParentFolders(string destFolderPath)
        {
            if (destFolderPath.Length > aveFolder.ServerRelativeUrl.Length)
            {
                if (!aveFolder.ServerRelativeUrl.Equals(destFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    var parentFolderNames = destFolderPath.Substring(DestInfo.ListPath.Length).Trim('/').Split('/');
                    var tempFolderPath = targetListUrl;
                    foreach (var name in parentFolderNames)
                    {
                        tempFolderPath = WebUtil.MakeFullUrl(tempFolderPath, name);
                        var currentFolder = aveFolder;
                        this.aveFolder = null;
                        this.aveFolder = new AveSPFolder(currentFolder, name);
                        aveFolder.InitSPFolder(false);

                        if (!isSendVirtualReport)
                        {
                            var virtualReportDto = new AveRestoreReportDto { Type = "F", Title = ReportAbsolutePath.GetTitle(aveFolder.Name) };

                            virtualReportDto.Size = 0;
                            virtualReportDto.Status = RestoreStatus.Skipped;
                            virtualReportDto.Path = virtualReportDto.SourcePath = tempFolderPath;
                            virtualReportDto.Title = name;
                            AddVirtualReport(virtualReportDto);
                            isSendVirtualReport = IsRestoreToSPO && !string.IsNullOrEmpty(DestInfo.FolderPath) && string.Equals(DestInfo.FolderPath, aveFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
        }

        protected AveRestoreMode RestoreDocument(AveSPDoc aveDoc, RestoreContentDto dto, AveRestoreReportDto reportDto, ref bool? isDocumentExist, IAveRestoreStream restoreStream)
        {
            using (new AvePerformanceScope("GranularRestore.RestoreDocument"))
            {
                string aveDocNameOriginal = dto.Name;
                var restoreMode = aveDoc.RestoreOption.mAveRestoreMode;
                AveMetadata metadata;
                var data = new Dictionary<string, object>();
                var securityRestoreOption = new SecurityRestoreOption()
                {
                    IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                };
                GlobalRestoreOptionWorker.CheckDocumentGlobalSetting(aveFolder, dto, securityRestoreOption);
                //Backup the setting of parent list and then change it for item restore.
                aveDoc.ParentFolder.ParentList.BackupListSetting();
                while ((metadata = restoreStream.ReadMetadata()) != null)
                {
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DocProperty:
                            data.Clear();
                            data = metadata.GetMetadata<Dictionary<string, object>>();
                            #region User & Group Cache, we may need it in the future in item level
                            AveMetadata userCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.UserCache);
                            if (userCacheMetadata != null)
                            {
                                var userList = userCacheMetadata.GetMetadata<AveUserList>();
                                lock (LockerDispatcher.GetLocker("UserInfoLock"))
                                {
                                    foreach (AveUserInfo userInfo in userList.Users)
                                    {
                                        aveDoc.ParentSite.SPMembers.RestoreUser(userInfo, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }
                                }
                            }
                            bool hasSensitivityLabels = false;
                            if (data.ContainsKey("_IpLabelId") && !string.IsNullOrEmpty(data["_IpLabelId"].ToString()))
                            {
                                log.Info("[SensitivityLabel]This file has Sensitivity Labels.");
                                hasSensitivityLabels = true;
                            }
                            else if (aveDoc.CheckIfHasSensitivityLabels())
                            {
                                log.Info("[SensitivityLabel]This file in the sharepoint has Sensitivity Labels.");
                                hasSensitivityLabels = true;
                            }
                            AveMetadata groupCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.GroupCache);
                            if (groupCacheMetadata != null && WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks)
                            {
                                AveGroupList groupList = groupCacheMetadata.GetMetadata<AveGroupList>();
                                this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                            }

                            #endregion
                            try
                            {
                                //restore document MMS
                                var metaData = restoreStream.TryReadMetadata(AveMetadataType.MetadataService);
                                if (metaData != null)
                                {
                                    List<AveTermStoreInfo> termStoreInfos = metaData.GetMetadata<List<AveTermStoreInfo>>();
                                    aveDoc.ParentSite.MetadataService.Restore(termStoreInfos);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("Failed restore document meta data, due to {0}", e);
                            }
                            AveMetadata userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                            var userData = new Dictionary<string, object>();
                            if (userDataMetadata != null)
                            {
                                userData = userDataMetadata.GetMetadata<Dictionary<string, object>>();
                            }
                            AveMetadata dataJuntionMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                            List<Dictionary<string, object>> dataJunction = null;
                            if (dataJuntionMetadata != null)
                            {
                                dataJunction = dataJuntionMetadata.GetMetadata<List<Dictionary<string, object>>>();
                            }

                            List<AveWebPartBaseInfo> webParts = null;
                            AveMetadata webpartMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocWebPart);
                            if (webpartMetadata != null)
                            {
                                webParts = webpartMetadata.GetMetadata<List<AveWebPartBaseInfo>>();
                            }
                            if (ItemVersionFilter.EnableVersionFilter &&
                                !IsRelatedVersionsContainsThis(data, aveDoc.AveSPItem, restoreStream))
                            {
                                throw new SkipException("Looks up a localized string similar to The version is filtered out..");
                            }

                            #region Item Dependency
                            ItemLevelRestoreItemCTAndFields(userData, dataJunction, aveDoc);
                            #endregion

                            #region conflict resolution
                            if (aveDoc.CheckRestoreOption(AveRestoreMode.AppendANewVersion))
                            {
                                if (AddNewVersionForDuplicateItem(data, aveDoc))
                                {
                                    //reportDto.Path = AveItemRestoreUtility.GetItemVersionString(aveDoc.Name, (int)data["UIVersion"]);
                                    this.aveFolder.RestoringItem.ResetNewItemValues(true, aveDoc.Name, aveDoc.Name);
                                    NullableBooleanExtension.SetIfValueNotExist(ref isDocumentExist, true);//Append a new version
                                }
                                else
                                {
                                    restoreMode = AveRestoreMode.Default;
                                }
                            }
                            if ((data.ContainsKey("IsCurrentVersion") && (bool)data["IsCurrentVersion"]) || userData.ContainsKey("#tp_IsCurrent") && (bool)userData["#tp_IsCurrent"])
                            {
                                reportDto.Title = aveDoc.Name;
                                reportDto.Path = ReportAbsolutePath.GetDocumentAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                            }
                            else
                            {
                                reportDto.Title = aveDoc.Name + ":" + GetUIVersionString(Convert.ToInt32(data["UIVersion"]));
                                reportDto.Path = ReportAbsolutePath.GetDocumentVersionAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, AveWeb.SPWeb.Url, Convert.ToInt32(data["UIVersion"]), aveFolder.SPFolder.Url, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                            }
                            #endregion
                            AveRestoreResult result = AveRestoreResult.Normal;
                            try
                            {
                                if(SetNowAsRestoreFileModifyTime && userData.ContainsKey("Modified"))
                                {
                                    userData["Modified"] = DateTime.UtcNow;
                                }
                                if (hasSensitivityLabels)
                                {
                                    if (ServiceAccountRequestForSensitivityLabel != null)
                                    {
                                        // if has service account, we will use service account request to restore file.
                                        SensitivityLabelRestoreOption sensitivityLabelRestoreOption = new SensitivityLabelRestoreOption()
                                        {
                                            method = SensitivityLabelRestoreMethod.ServiceAccount,
                                            Request = ServiceAccountRequestForSensitivityLabel,
                                        };
                                        log.Info("[SensitivityLabel]Has service account, we will use service account request to restore file.");
                                        result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts, sensitivityLabelRestoreOption);
                                    }
                                    else if (dto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM)
                                    {
                                        //aveItemDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM：true说明勾选了current version进行了还原
                                        //则current version不需要进行解密SensitivityLabel，直接还原即可把SensitivityLabel添加
                                        //对于version 则进行解密，否则还原会失败
                                        if ((data.ContainsKey("IsCurrentVersion") && (bool)data["IsCurrentVersion"]) || userData.ContainsKey("#tp_IsCurrent") && (bool)userData["#tp_IsCurrent"])
                                        {
                                            log.Info("[SensitivityLabel]current version RestoreSelf.");
                                            result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                        }
                                        else if (AppProfileRequestForSensitivityLabel != null)
                                        {
                                            SensitivityLabelRestoreOption sensitivityLabelRestoreOption = new SensitivityLabelRestoreOption()
                                            {
                                                method = SensitivityLabelRestoreMethod.AppProfile,
                                                Request = AppProfileRequestForSensitivityLabel,
                                            };
                                            log.Info("[SensitivityLabel]Has AppProfile, we will use app request to restore file.");
                                            result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts, sensitivityLabelRestoreOption);
                                        }
                                        else
                                        {
                                            result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                        }
                                    }
                                    else
                                    {
                                        result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                    }
                                }
                                else
                                {
                                    result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                }

                                if (result == AveRestoreResult.Normal)
                                {
                                    log.Info("IsRemoveTheStubAfterRestore and result is Normal, so RemoveArchiveStub.");
                                    RemoveArchiveStub(aveDoc, aveDocNameOriginal, dto.UniqueId.ToString(), dto.StubType);
                                }
                                else
                                {
                                    log.Info($"IsRemoveTheStubAfterRestore and result is :{result.ToString()}, so skip RemoveArchiveStub.");
                                }

                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (AveWarningException e)
                            {
                                throw new SkipException(e.Message);
                            }
                            finally
                            {
                                if (result == AveRestoreResult.SkipRecycleBinData)
                                {
                                    throw new SkipException("This item conflicts with recycle bin and conflict resolution is skip.");
                                }
                                if (aveDoc.ConflictWithDocument.HasValue && result != AveRestoreResult.SkipTheSameItem)
                                {
                                    NullableBooleanExtension.SetIfValueNotExist(ref isDocumentExist, aveDoc.ConflictWithDocument.Value);
                                }
                                if (this.aveFolder.RestoringItem.NeedSkipped && !GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                                {
                                    throw new SkipException(this.aveFolder.RestoringItem.NeedSkippedKey, this.aveFolder.RestoringItem.NeedSkippedReason);
                                }
                            }
                            break;

                        case AveMetadataType.RoleAssignment:
                            if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.RestoreSecurity) ||
                                GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                            {
                                if (string.IsNullOrEmpty(OopStubUrl))
                                {
                                    log.Info("Begin restore DocumentLevel RoleAssignment.");
                                    var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                    AveObjectSecurity security = AveObjectSecurity.CreateInstance(aveDoc.AveSPItem);
                                    security.SourceHasUniqueRoleAssignment = aveDoc.AveSPItem.HasUniqueRoleAssignments;
                                    security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                    using (var report = security.GetReport())
                                    {
                                        AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                    }
                                }
                            }
                            break;

                        case AveMetadataType.DocImmedSubscriptions:
                            if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                AveSPAlert alert = new AveSPDocAlert(aveDoc);
                                foreach (var iAlertInfo in iAlertInfos)
                                {
                                    alert.RestoreAlert(iAlertInfo, false);
                                }
                                using (var report = alert.GetReport())
                                {
                                    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                }
                            }
                            break;

                        case AveMetadataType.DocSchedSubscriptions:
                            if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                AveSPAlert alert = new AveSPDocAlert(aveDoc);
                                foreach (var sAlertInfo in sAlertInfos)
                                {
                                    alert.RestoreAlert(sAlertInfo, true);
                                }
                                using (var report = alert.GetReport())
                                {
                                    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                }
                            }
                            break;

                        //case AveMetadataType.DocumentTagging:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite) &&
                        //        AveEnv.IsMoss)
                        //    {
                        //        List<AveDocumentTaggingInfo> DTs = metadata.GetMetadata<List<AveDocumentTaggingInfo>>();
                        //        AveDocumentTagging documentTagging = new AveDocumentTagging(aveDoc.TagUrl, aveDoc.ParentSite);
                        //        documentTagging.Restore(DTs);
                        //        using (var report = documentTagging.GetReport())
                        //        {
                        //            AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                        //        }
                        //    }
                        //    break;
                        //#region Social Tag and Comment
                        //case AveMetadataType.SocialTag:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite) &&
                        //        AveEnv.IsMoss)
                        //    {
                        //        List<AveSocialTagInfo> tagInfos = metadata.GetMetadata<List<AveSocialTagInfo>>();
                        //        AveSPSocialTag socialTags = new AveSPSocialTag(aveDoc.TagUrl, aveDoc.ParentSite);
                        //        socialTags.Restore(tagInfos);
                        //        AddReport(AveRestoreReportDto.Parse(socialTags.GetReport().GetDetails(), dto));
                        //    }
                        //    break;

                        //case AveMetadataType.SocialComment:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite) &&
                        //        AveEnv.IsMoss)
                        //    {
                        //        List<AveSocialCommentInfo> commentInfos = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                        //        AveSPSocialComment socialComment = new AveSPSocialComment(aveDoc.TagUrl, aveDoc.ParentSite);
                        //        socialComment.Restore(commentInfos);
                        //        AddReport(AveRestoreReportDto.Parse(socialComment.GetReport().GetDetails(), dto));
                        //    }
                        //    break;
                        //#endregion
                        case AveMetadataType.WorkflowInstance:
                            if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                WFConflictResolution wfResolution = WFConflictResolution.Instance;
                                foreach (var unit in wfInfo)
                                {
                                    var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                                    wfResolution.HandleInstanceConflict(wfAssociationUnit, aveDoc.AveSPItem.SPListItem);
                                }
                                using (var report = wfResolution.GetReport())
                                {
                                    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                return restoreMode;
            }
            //aveDoc.DealSolution();
        }

        private bool TryAddStubTypeToList(List<string> stubTypes, string stubType)
        {
            if(!stubTypes.Any(i => i == stubType))
            {
                stubTypes.Add(stubType);
                return true;
            }
            return false;
        }
        protected void RemoveArchiveStub(AveSPDoc aveDoc, string aveDocNameOriginal, string aveDocIdOriginal, string stubType, ARMigrationRestoreFileInfo restoreFileInfo = null)
        {
            if(stubType == "null" || aveDocNameOriginal.Contains(":"))
            {
                log.Info($"RemoveArchiveStub stubType is null,skip remove stub,stubType:{stubType}.");
                return;
            }

            //Archive stub will add .aspx/.txt at the end of file name
            var mappings = new Dictionary<string, string>
            {
                { "Aspx", aveDocNameOriginal + ".aspx" },
                { "Html", aveDocNameOriginal + ".html" },
                { "Txt", aveDocNameOriginal + ".txt" },
                { "Link", aveDocNameOriginal + ".url" },
            };
            List<string> possiblyStubTypes = new List<string>();

            if(IsEnduserRestore && !string.IsNullOrEmpty(PossiblyStubType))
            {
                possiblyStubTypes.Add(PossiblyStubType);
            }
            else
            {
                if (!string.IsNullOrEmpty(PossiblyStubType))
                {
                    possiblyStubTypes.Add(PossiblyStubType);
                }

                if (!string.IsNullOrEmpty(stubType))
                {
                    TryAddStubTypeToList(possiblyStubTypes, stubType);
                }

                foreach (var item in mappings)
                {
                    TryAddStubTypeToList(possiblyStubTypes, item.Key);
                }
            }

            try
            {
                log.Info(string.Format($"Begin remove stub file. stub type:{stubType}."));

                foreach (string possiblyType in possiblyStubTypes)
                {
                    var isProcessedStub = false;
                    if (!mappings.TryGetValue(possiblyType, out var stubName))
                    {
                        log.Error($"Invalid stub type: {possiblyType}");
                        continue;
                    }

                    var stubUrl = aveDoc.ParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + stubName;
                    var stubFile = AveWeb.SPWeb.GetFile(stubUrl);
                    if (stubFile.Exists)
                    {
                        if(PossiblyStubType != possiblyType)
                        {
                            log.Info($"Switch the most possibly stub type from {PossiblyStubType} to {possiblyType}");
                            PossiblyStubType = possiblyType;
                        }
                       
                        if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
                        {
                            log.Info("the restore job is end user oop restore");
                            KeepStubPermission(stubFile, aveDoc);
                        }
                        if ((stubFile.Item != null
                        && stubFile.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                        && stubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                        && stubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                        || !string.IsNullOrEmpty(OopStubUrl)
                        || IsRestoreToSPO
                        || IsForceDeleteStub
                        )
                        {
                            if (restoreFileInfo != null) // for migration restore 
                            {
                                log.Info($"stub exist need delete. stub path:{stubUrl.LogBase64()}, stub type: {possiblyType}");
                                restoreFileInfo.rowid = stubFile.Item.ID;
                                restoreFileInfo.StubPath = stubUrl;
                            }
                            else
                            {
                                try
                                {
                                    stubFile.Delete();
                                    log.Info($"delete stub file successful. stub path:{stubUrl.LogBase64()}.");
                                    LinkFileCommon.DeleteStubFileRecord(AveSite.SPSite.ID.ToString(), aveDocIdOriginal);
                                }
                                catch (Exception exp)
                                {
                                    log.Info($"delete file exception: {exp.Message},stub path:{stubUrl.LogBase64()}. retry action.");
                                    Record.UndeclareItemAsRecord(stubFile.Item);
                                    if (ArchiverCommonStaticMethod.IsHaveRecordLabel(stubFile.Item))
                                    {
                                        log.Info("This Stub File is locked by record label File.FileName:{0}", stubUrl.LogBase64());
                                        stubFile.Item.SetComplianceTagOnBulkItems("");
                                    }
                                    stubFile.Delete();
                                    log.Info($"delete stub file successful. stub path:{stubUrl.LogBase64()}.");
                                    LinkFileCommon.DeleteStubFileRecord(AveSite.SPSite.ID.ToString(), aveDocIdOriginal);
                                }
                            }
                        }
                        else
                        {
                            log.Warn($"the type of stub is not a stub file,type:{stubType}, AveListItem is null: {stubFile.Item == null}");
                        }
                        //log.Info($"Delete stub file : {stubUrl} successful.");
                        //Stub删除成功后退出循环，减少一次实例化File
                        isProcessedStub = true;
                    }
                    else
                    {
                        log.Info(string.Format($"stub type: {System.IO.Path.GetExtension(stubName)} does not exist in library.stub path:{stubUrl.LogBase64()}, stub type: {possiblyType}.WebURL:{AveWeb.SPWeb.ServerRelativeUrl}."));
                    }

                    if (isOriginalSiteExist && oriAveWeb?.SPWeb != null)
                    {
                        log.Info("try to delete stub from original site.");
                        //var oriStubUrl = aveDoc.ParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + stubName;
                        string oriStubUrl = string.Empty;
                        if (IsRestoreToSPO)
                        {
                            var targetLocation = string.IsNullOrEmpty(DestInfo.FolderPath) ? DestInfo.ListPath : DestInfo.FolderPath;
                            var originLocation = string.IsNullOrEmpty(sourceFolderUrl) ? sourceLibUrl : sourceFolderUrl;
                            oriStubUrl = originLocation + stubUrl.Substring(targetLocation.Length);
                            if (!string.Equals(sourceSiteUrl, oriAveSite.SPSite.Url, StringComparison.OrdinalIgnoreCase))
                            {
                                oriStubUrl = oriStubUrl.Replace(sourceSiteUrl, oriAveSite.SPSite.Url);
                            }
                        }
                        else
                        {
                            oriStubUrl = stubUrl.Replace(AveSite.ServerRelativeUrl, oriAveSite.ServerRelativeUrl);
                        }

                        var oriStubFile = oriAveWeb.SPWeb.GetFile(oriStubUrl);
                        if (oriStubFile.Exists)
                        {
                            if (PossiblyStubType != possiblyType)
                            {
                                log.Info($"Switch the most possibly stub type from {PossiblyStubType} to {possiblyType}");
                                PossiblyStubType = possiblyType;
                            }

                            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
                            {
                                log.Info("the restore job is end user oop restore");
                                KeepStubPermission(oriStubFile, aveDoc);
                            }
                            if ((oriStubFile.Item != null
                            && oriStubFile.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                            && oriStubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                            && oriStubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                            || IsRestoreToSPO
                            || !string.IsNullOrEmpty(OopStubUrl)
                            || IsForceDeleteStub
                            )
                            {
                                if (restoreFileInfo != null) // for migration restore 
                                {
                                    log.Info($"stub exist need delete. stub path:{stubUrl.LogBase64()}, stub type: {possiblyType}");
                                    restoreFileInfo.OriStubRowId = oriStubFile.Item.ID;
                                    restoreFileInfo.OriStubPath = oriStubUrl;
                                    restoreFileInfo.OriParentListId = oriStubFile.ParentList.ID;
                                    restoreFileInfo.AveDocIdOriginal = aveDocIdOriginal;
                                }
                                else
                                {
                                    try
                                    {
                                        oriStubFile.Delete();
                                        log.Info($"delete original stub file successful. original stub path:{oriStubUrl.LogBase64()}.");
                                        LinkFileCommon.DeleteStubFileRecord(oriAveSite.SPSite.ID.ToString(), aveDocIdOriginal);
                                    }
                                    catch (Exception exp)
                                    {
                                        log.Info($"delete original stub file exception: {exp.Message}, original stub path:{oriStubUrl.LogBase64()}. retry action.");
                                        Record.UndeclareItemAsRecord(oriStubFile.Item);
                                        if (ArchiverCommonStaticMethod.IsHaveRecordLabel(oriStubFile.Item))
                                        {
                                            log.Info("This Stub File is locked by record label File.FileName:{0}", stubUrl.LogBase64());
                                            oriStubFile.Item.SetComplianceTagOnBulkItems("");
                                        }
                                        oriStubFile.Delete();
                                        log.Info($"delete original stub file successful. original stub path:{oriStubUrl.LogBase64()}.");
                                        LinkFileCommon.DeleteStubFileRecord(oriAveSite.SPSite.ID.ToString(), aveDocIdOriginal);
                                    }
                                }
                            }
                            else
                            {
                                log.Warn($"the type of original stub is not a stub file,type:{stubType}, AveListItem is null: {oriStubFile.Item == null}");
                            }
                            //log.Info($"Delete stub file : {stubUrl} successful.");
                            //Stub删除成功后退出循环，减少一次实例化File
                            isProcessedStub = true;
                        }
                        else
                        {
                            log.Info(string.Format("stub type: {0} does not exist in original library.", System.IO.Path.GetExtension(stubName)));
                        }
                    }

                    if (isProcessedStub)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in remove archive stub. stub name : {0}, reason : {1}.", aveDoc.Name, ex.ToString()));
            }
        }

        private void KeepStubPermission(IAveFile stubFile,AveSPDoc restoreFile)
        {
            try
            {
                IAveRoleAssignment spRoleAssignment = null;
                Wrapper.Restore.AveObjectSecurity.AveSecurityParameters securityParam = new AveObjectSecurity.AveSecurityParameters();
                securityParam.aveSPWeb = AveList.ParentWeb;
                if (!restoreFile.SPFile.Item.HasUniqueRoleAssignments)
                {
                    restoreFile.SPFile.Item.BreakRoleInheritance(false, true);
                }
                foreach (var role in stubFile.Item.RoleAssignments)
                {
                    try
                    {
                        securityParam.roleAssignments = restoreFile.SPFile.Item.RoleAssignments;
                        var id = role.PrincipalId;
                        log.Info($"keep stub permission id {id}");
                        IAvePrincipal member = securityParam.aveSPWeb.ParentSite.SPSite.RootWeb.SiteUsers.GetByID(id);
                        IAvePrincipal group = securityParam.aveSPWeb.ParentSite.SPSite.RootWeb.SiteGroups.GetByID(id);
                        //var roleId = role.RoleDefinitionBindings.FirstOrDefault().ID;
                        spRoleAssignment = securityParam.roleAssignments.CreateRoleAssignment(member ?? group);
                        spRoleAssignment.RoleDefinitionBindings.Add(role.RoleDefinitionBindings.FirstOrDefault());
                        securityParam.roleAssignments.Add(spRoleAssignment);
                    }
                    catch (Exception e)
                    {
                        log.Error($"keep stub permission failed,restore next role,error:{e}");
                    }
                }

            }
            catch (Exception e)
            {
                log.Error($"keep stub permission failed,error:{e}");
            }
        }
        private void EnableVersioning()
        {

            if (AveList != null && AveList.SPList != null && !AveList.SPList.EnableVersioning)
            {
                try
                {
                    AveList.SPList.EnableVersioning = true;
                    AveList.SPList.Update();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, @"Looks up a localized string similar to An error occurred while adding new version for duplicate item. Error Message: {0}.", e.ToString());
                    AveList.ReloadList();
                    AveList.SPList.EnableVersioning = true;
                    AveList.SPList.Update();
                }
            }

        }

        private void EnableVersioning(AveSPListItem aveListItem)
        {

            if (aveListItem.ParentList != null && aveListItem.ParentList.SPList != null && !aveListItem.ParentList.SPList.EnableVersioning)
            {
                try
                {
                    aveListItem.ParentList.SPList.EnableVersioning = true;
                    aveListItem.ParentList.SPList.Update();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, @"Looks up a localized string similar to An error occurred while adding new version for duplicate item. Error Message: {0}.", e.ToString());
                    aveListItem.ParentList.ReloadList();
                    aveListItem.ParentList.SPList.EnableVersioning = true;
                    aveListItem.ParentList.SPList.Update();
                }
            }
        }

        private AveRestoreMode RestoreListItem(AveSPListItem aveListItem, RestoreContentDto dto, AveRestoreReportDto reportDto, ref bool? isListItemExist, IAveRestoreStream restoreStream)
        {
            using (new AvePerformanceScope("GranularRestore.RestoreListItem"))
            {
                var restoreMode = aveListItem.RestoreOption.mAveRestoreMode;
                AveMetadata metadata;
                if (aveListItem.ParentList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
                {
                    AveMetadata userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                    var userData = new Dictionary<string, object>();
                    if (userDataMetadata != null)
                    {
                        userDataMetadata.GetMetadata(userData);
                        aveListItem.RestoreUserInfo(userData);
                    }
                    return restoreMode;
                }
                var option = new SecurityRestoreOption
                {
                    NeedRestore = true,
                    IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                };
                var data = new Dictionary<string, object>();
                aveListItem.ParentList.BackupListSetting();
                while ((metadata = restoreStream.ReadMetadata()) != null)
                {
                    data.Clear();
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DocProperty:
                            data = metadata.GetMetadata<Dictionary<string, object>>();

                            #region User & Group Cache, we may need it in the future in item level

                            AveMetadata userCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.UserCache);
                            if (userCacheMetadata != null)
                            {
                                var userList = userCacheMetadata.GetMetadata<AveUserList>();
                                lock (LockerDispatcher.GetLocker("UserInfoLock"))
                                {
                                    foreach (AveUserInfo userInfo in userList.Users)
                                    {
                                        aveListItem.ParentList.ParentWeb.ParentSite.SPMembers.RestoreUser(userInfo, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }
                                }
                            }

                            AveMetadata groupCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.GroupCache);
                            if (groupCacheMetadata != null && WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks)
                            {
                                AveGroupList groupList = groupCacheMetadata.GetMetadata<AveGroupList>();
                                this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                            }

                            #endregion

                            AveMetadata userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                            try
                            {
                                //restore document MMS
                                var metaData = restoreStream.TryReadMetadata(AveMetadataType.MetadataService);
                                if (metaData != null)
                                {
                                    List<AveTermStoreInfo> termStoreInfos = metaData.GetMetadata<List<AveTermStoreInfo>>();
                                    aveListItem.ParentSite.MetadataService.Restore(termStoreInfos);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("Failed restore document meta data, due to {0}", e);
                            }
                            var userData = new Dictionary<string, object>();
                            if (userDataMetadata != null)
                            {
                                userData = userDataMetadata.GetMetadata<Dictionary<string, object>>();
                                if (userData.ContainsKey("#tp_GUID"))  //SAAS-11351 Archiver通过该属性获取ListItem
                                {
                                    data["GUID"] = userData["#tp_GUID"];
                                }
                            }
                            AveMetadata dataJuntionMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                            List<Dictionary<string, object>> dataJunction = null;
                            if (dataJuntionMetadata != null)
                            {
                                dataJunction = dataJuntionMetadata.GetMetadata<List<Dictionary<string, object>>>();
                            }
                            GlobalRestoreOptionWorker.CheckListItemGlobalSetting(aveFolder, dto, option, userData);
                            if (GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                            {
                                data["RestoreSecurityOnly"] = true;
                            }
                            if (aveListItem.IsWorkflowTask(userData))
                            {
                                //workflow task单独还原没有意义。
                                log.Warn("skip restore workflow task, it is associated with workflow instance.");
                                throw new SkipException(WrapperRestoreReportResource.Wrapper_SkippedWorkflowTaskItem);
                            }
                            if (aveListItem.ParentList.SPList.BaseTemplate == AveListTemplateType.AccessRequest)
                            {
                                log.Warn(WrapperRestoreReportResource.Wrapper_SkippedAccessRequestListItem);
                                throw new SkipException(WrapperReportResourceKey.Wrapper_SkippedAccessRequestListItem.ToString(), WrapperRestoreReportResource.Wrapper_SkippedAccessRequestListItem);
                            }
                            if (ItemVersionFilter.EnableVersionFilter &&
                                !IsRelatedVersionsContainsThis(data, aveListItem.AveSPItem, restoreStream,true))
                            {
                                log.Warn("skip the listitem due to the version filter.");
                                throw new SkipException("Looks up a localized string similar to The version is filtered out..");
                            }
                            #region Item Dependency
                            ItemLevelRestoreItemCTAndFields(userData, dataJunction, aveListItem);
                            #endregion
                            #region conflict resolution
                            if (aveListItem.CheckRestoreOption(AveRestoreMode.AppendANewVersion))
                            {
                                if (AddNewVersionForDuplicateItem(data, aveListItem))
                                {
                                    //reportDto.Path = AveItemRestoreUtility.GetItemVersionString(aveListItem.Name, (int)data["UIVersion"]);
                                    this.aveFolder.RestoringItem.ResetNewItemValues(true, aveListItem.Name, aveListItem.Name);
                                    NullableBooleanExtension.SetIfValueNotExist(ref isListItemExist, true);//Append a new version
                                }
                            }
                            int itemId = 0;
                            int indexNum = aveListItem.Name.IndexOf("_.");
                            if (indexNum >= 0)
                            {
                                int.TryParse(aveListItem.Name.Substring(0, indexNum), out itemId);
                            }
                            reportDto.Path = ReportAbsolutePath.GetListItemAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, this.aveFolder.ServerRelativeUrl, AveList.SPList.DefaultDisplayFormUrl, itemId, (int)data["UIVersion"]);
                            #endregion
                            AveRestoreResult result = AveRestoreResult.Normal;
                            try
                            {
                                result = aveListItem.RestoreSelf(data, userData, dataJunction);
                                if (aveListItem.SPListItem != null)
                                {
                                    if (!string.IsNullOrEmpty(aveListItem.TagUrl))
                                    {
                                        reportDto.Path = aveListItem.TagUrl + "&VersionNo=" + aveListItem.SPListItem["_UIVersion"].ToString();
                                    }
                                    if ((data.ContainsKey("IsCurrentVersion") && (bool)data["IsCurrentVersion"]) || userData.ContainsKey("#tp_IsCurrent") && (bool)userData["#tp_IsCurrent"])
                                    {
                                        reportDto.Title = aveListItem.SPListItem["FileLeafRef"].ToString();
                                    }
                                    else
                                    {
                                        reportDto.Title = aveListItem.SPListItem["FileLeafRef"].ToString() + GetUIVersionString((int)aveListItem.SPListItem["_UIVersion"]);
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (AveWarningException e)
                            {
                                log.Warn("skip the listitem due to the warning exception.");
                                throw new SkipException(e.Message);
                            }
                            finally
                            {
                                if (result == AveRestoreResult.SkipRecycleBinData)
                                {
                                    log.Warn("skip the listitem due to conflicted with recycle bin.");
                                    throw new SkipException("This item conflicts with recycle bin and conflict resolution is skip.");
                                }
                                if (aveListItem.ConflictWithDocument.HasValue && result != AveRestoreResult.SkipTheSameItem)
                                {
                                    NullableBooleanExtension.SetIfValueNotExist(ref isListItemExist, aveListItem.ConflictWithDocument.Value);
                                }
                                if (this.aveFolder.RestoringItem.NeedSkipped)
                                {
                                    log.Warn("skip the listitem due to the item need skipped.");
                                    throw new SkipException(this.aveFolder.RestoringItem.NeedSkippedKey, this.aveFolder.RestoringItem.NeedSkippedReason);
                                }
                            }
                            if (this.aveFolder.RestoringItem.NeedSkipped && !GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                            {
                                return restoreMode;
                            }
                            if (result == AveRestoreResult.SkipTheSameItem)
                            {
                                restoreMode = AveRestoreMode.Default;
                                return restoreMode;
                            }
                            break;

                        case AveMetadataType.RoleAssignment:
                            if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.RestoreSecurity) ||
                                GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                            {
                                log.Info("Begin restore ListItem RoleAssignment.");
                                var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                AveObjectSecurity security = AveObjectSecurity.CreateInstance(aveListItem.AveSPItem);
                                security.SourceHasUniqueRoleAssignment = aveListItem.AveSPItem.HasUniqueRoleAssignments;
                                security.RestoreRoleAssignments(roleAssignments, option);
                                AddReport(AveRestoreReportDto.Parse(security.GetReport().GetDetails(), dto));
                            }
                            break;

                        case AveMetadataType.DocImmedSubscriptions:
                            if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                AveSPAlert alert = new AveSPItemAlert(aveListItem);
                                foreach (var iAlertInfo in iAlertInfos)
                                {
                                    alert.RestoreAlert(iAlertInfo, false);
                                }
                                AddReport(AveRestoreReportDto.Parse(alert.GetReport().GetDetails(), dto));
                            }
                            break;

                        case AveMetadataType.DocSchedSubscriptions:
                            if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                AveSPAlert alert = new AveSPItemAlert(aveListItem);
                                foreach (var sAlertInfo in sAlertInfos)
                                {
                                    alert.RestoreAlert(sAlertInfo, true);
                                }
                                AddReport(AveRestoreReportDto.Parse(alert.GetReport().GetDetails(), dto));
                            }
                            break;
                        //#region Social Tag and Comment
                        //case AveMetadataType.SocialTag:
                        //    if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.OverWrite) &&
                        //        AveEnv.IsMoss)
                        //    {
                        //        List<AveSocialTagInfo> tagInfos = metadata.GetMetadata<List<AveSocialTagInfo>>();
                        //        AveSPSocialTag socialTags = new AveSPSocialTag(aveListItem.TagUrl, aveListItem.ParentSite);
                        //        socialTags.Restore(tagInfos);
                        //        AddReport(AveRestoreReportDto.Parse(socialTags.GetReport().GetDetails(), dto));
                        //    }
                        //    break;

                        //case AveMetadataType.SocialComment:
                        //    if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.OverWrite) &&
                        //        AveEnv.IsMoss)
                        //    {
                        //        List<AveSocialCommentInfo> commentInfos = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                        //        AveSPSocialComment socialComment = new AveSPSocialComment(aveListItem.TagUrl, aveListItem.ParentSite);
                        //        socialComment.Restore(commentInfos);
                        //        AddReport(AveRestoreReportDto.Parse(socialComment.GetReport().GetDetails(), dto));
                        //    }
                        //    break;
                        //#endregion
                        case AveMetadataType.WorkflowInstance:
                            if (aveListItem.CheckRestoreOption(aveListItem.IsNewCreated, AveRestoreMode.OverWrite))
                            {
                                var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                WFConflictResolution wfResolution = WFConflictResolution.Instance;
                                foreach (var unit in wfInfo)
                                {
                                    var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                                    wfResolution.HandleInstanceConflict(wfAssociationUnit, aveListItem.SPListItem);
                                }
                                using (var report = wfResolution.GetReport())
                                {
                                    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                return restoreMode;
            }
        }

        protected bool AddNewVersionForDuplicateItem(Dictionary<string, object> data, AveSPDoc sPDoc)
        {
            bool isNewVersion = false;
            try
            {
                //begin DOC-67916  if the file is system file we do not need to create new version for it
                if (AveList.Name.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase)
                    || sPDoc.ParentFolder.ServerRelativeUrl.TrimEnd('/').Equals(AveList.SPList.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase)
                    || (data.ContainsKey("IsViewPage") && (bool)data["IsViewPage"] == true)
                    || !data.ContainsKey("DoclibRowId"))
                {
                    return false;
                }
                if (data.ContainsKey("BiggestVersionModified"))
                {
                    if (!AppendItemMapping.ContainsKeyAppendVersion(sPDoc.Name))
                    {
                        bool needAppend = sPDoc.NeedAppendNewVersion((DateTime)data["BiggestVersionModified"]);
                        AppendItemMapping.AddToMappingAppendVersion(sPDoc.Name, needAppend);
                    }
                    if (!AppendItemMapping.GetValueAppendVersion(sPDoc.Name))
                    {
                        return false;
                    }
                }
                //end DOC-67916
                if (data != null && data.ContainsKey("UIVersion"))
                {
                    string tempName = this.aveFolder.SPFolder.ServerRelativeUrl.TrimEnd('/') + "/" + sPDoc.Name;
                    if (string.IsNullOrEmpty(this.mDesTempItemName) || !this.mDesTempItemName.Equals(tempName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.mDesMaxVersionBeforeAdd = 0;
                        this.mDesTempItemName = tempName;

                        //IAveFile sf = sPDoc.SPFile;
                        IAveFile sf = AveWeb.SPWeb.GetFile(tempName);
                        if (sf != null && sf.Exists)
                        {
                            EnableVersioning();
                            this.mDesMaxVersionBeforeAdd = sf.UIVersion;
                            int tempDesMaxVersion = GetNewVersionForDuplicateItem(this.mDesMaxVersionBeforeAdd, Convert.ToInt32(data["UIVersion"]));
                            data["UIVersion"] = tempDesMaxVersion;
                            this.mDesMaxVersionBeforeAdd = tempDesMaxVersion;
                            isNewVersion = true;
                        }
                    }
                    else
                    {
                        if (this.mDesMaxVersionBeforeAdd > 0) //if desMaxVersionBeforeAdd is 0, we don't change version here.
                        {
                            int tempDesMaxVersion = GetNewVersionForDuplicateItem(this.mDesMaxVersionBeforeAdd, Convert.ToInt32(data["UIVersion"]));
                            data["UIVersion"] = tempDesMaxVersion;
                            this.mDesMaxVersionBeforeAdd = tempDesMaxVersion;
                            isNewVersion = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while adding new version for duplicate document. Error Message: {0}.", ex.ToString());
            }
            return isNewVersion;
        }

        private bool AddNewVersionForDuplicateItem(Dictionary<string, object> data, AveSPListItem sPListItem)
        {
            bool isNewVersion = false;
            try
            {
                if (data.ContainsKey("BiggestVersionModified"))
                {
                    if (!AppendItemMapping.ContainsKeyAppendVersion(sPListItem.Name))
                    {
                        bool needAppend = sPListItem.NeedAppendNewVersion((DateTime)data["BiggestVersionModified"]);
                        AppendItemMapping.AddToMappingAppendVersion(sPListItem.Name, needAppend);
                    }
                    if (!AppendItemMapping.GetValueAppendVersion(sPListItem.Name))
                    {
                        return false;
                    }
                }
                if (data != null && data.ContainsKey("UIVersion"))
                {
                    IAveListItem item = sPListItem.GetCurrentSPListItem(data);
                    string tempName = string.Empty;
                    if (item == null)
                    {
                        tempName = sPListItem.ParentList.Name + "\\" + sPListItem.Name;
                    }
                    else
                    {
                        tempName = item.UniqueId.ToString();
                    }
                    if (string.IsNullOrEmpty(mDesTempItemName) ||
                        (!mDesTempItemName.Equals(tempName, StringComparison.OrdinalIgnoreCase)
                        && !mDesTempItemName.Equals(sPListItem.ParentList.Name + "\\" + sPListItem.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        mDesMaxVersionBeforeAdd = 0;
                        mDesTempItemName = tempName;
                        int newVersion;
                        if (item != null)
                        {
                            EnableVersioning(sPListItem);
                            mDesMaxVersionBeforeAdd = (int)item["_UIVersion"];//AveSite.QueryService.GetCurrentUIVersion(sPListItem.AveSPItem.SiteId, aveFolder.Id, item.UniqueId);
                            newVersion = GetNewVersionForDuplicateItem(mDesMaxVersionBeforeAdd, (int)data["UIVersion"]);
                            data["UIVersion"] = newVersion;
                            this.mDesMaxVersionBeforeAdd = newVersion;
                            isNewVersion = true;
                        }
                    }
                    else
                    {
                        if (this.mDesMaxVersionBeforeAdd > 0) //if desMaxVersionBeforeAdd is 0, we don't change version here.
                        {
                            int tempDesMaxVersion = GetNewVersionForDuplicateItem(mDesMaxVersionBeforeAdd, (int)data["UIVersion"]);
                            data["UIVersion"] = tempDesMaxVersion;
                            this.mDesMaxVersionBeforeAdd = tempDesMaxVersion;
                            isNewVersion = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while adding new version for duplicate item. Error Message: {0}.", ex.ToString());
            }
            return isNewVersion;
        }

        private void RestoreAttachment(AveSPAttachment aveAttachment, IAveRestoreStream restoreStream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreAttachment"))
            {
                AveMetadata metadata = restoreStream?.TryReadMetadata(AveMetadataType.DocProperty);
                Dictionary<string, object> data = metadata?.GetMetadata<Dictionary<string, object>>();

                try
                {
                    aveAttachment.RestoreAttachment(Config.RestoreType == RestoreType.InPlace, data);
                }
                catch (AveRestoreException ex)
                {
                    if (ex.Message == AveRestoreResult.Omit.ToString())
                    {
                        throw new SkipException("Parent item is skipped.");
                    }
                }

                if (data != null && data.Any())
                {
                    aveAttachment.UpdateAllDocsPropertyByNative((DateTime)data["Created"], (DateTime)data["Modified"]);
                }
            }
        }

        public override void RestoreItem(RestoreContentDto aveItemDto)
        {
            this.RestoreItem(aveItemDto, base.RestoreStream);
        }

        public virtual void RestoreItem(RestoreContentDto aveItemDto, IAveRestoreStream restoreStream)
        {
            if (IsRestoreToSPO)
            {
                var destFolderPath = string.IsNullOrEmpty(DestInfo.FolderPath) ? DestInfo.ListPath : DestInfo.FolderPath;
                if (!isSelectedFolderProcessed)
                {
                    SetSourceFolderUrl(aveItemDto, false);
                }

                if (string.IsNullOrEmpty(targetFolderUrl))
                {
                    targetFolderUrl = WebUtil.MakeFullUrl(targetSiteUrl, destFolderPath);
                }

                InitTargetParentFolders(destFolderPath);
            }
            string oldUrl = aveItemDto.SrcUrl;
            string srcPathAppendName = string.Empty;
            string appendName = string.Empty;

            if (oldUrl.Contains('\\'))
            {
                oldUrl = oldUrl.Replace('\\', '/');
            }
            if (!string.IsNullOrEmpty(targetSiteUrl))
            {
                aveItemDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveItemDto);
            }
            var reportDto = new AveRestoreReportDto { Type = aveItemDto.Type.ToString(), Title = aveItemDto.Name, PathMD5 = aveItemDto.ItemPathMd5, DestinationUrl = string.Empty };//Path = aveItemDto.Name       
            IAveListItem restoredItem = null;
            if (AveList != null && AveList.NeedContinue == false)
            {
                //List Skipped,we should not add item\folder under the list to report.
                return;
            }
            if (this.aveFolder == null)
            {
                if (aveItemDto.IsAppData)
                {
                    return;
                }
                if (aveItemDto.IsSelected)
                {
                    reportDto.Status = RestoreStatus.ContainerFailed;
                }
                else
                {
                    reportDto.Status = RestoreStatus.Skipped;
                }
                reportDto.SourcePath = aveItemDto.SrcUrl;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindItemParent.ToString(), RestoreReportResource.Item_CanNotFindItemParent, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if (aveItemDto.IsFailed)
            {
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Status = RestoreStatus.Skipped;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_SkipBackupFailedItem.ToString(), RestoreReportResource.Item_SkipBackupFailedItem, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if ((this.aveFolder.SPFolder == null && GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyMerge)
                || (this.aveFolder.SPFolder == null && GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyOverWrite))
            {
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Status = RestoreStatus.Skipped;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_GlobalRestoreOptionWorkerSkip.ToString(), RestoreReportResource.Item_GlobalRestoreOptionWorkerSkip, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if (this.aveFolder.SPFolder == null || !aveFolder.SPFolder.Exists)
            {
                this.aveFolder.InitSPFolder(true);
            }
            this.aveFolder.RestoringItem.IsIncludingRecycleBinData = (Config).IncludingRecycleBinData;
            string realName = aveItemDto.Name;
            int index = realName.IndexOf(':');
            bool isVersion = false;
            if (index >= 0)
            {
                realName = realName.Substring(0, index);
                isVersion = true;
                reportDto.Version = aveItemDto.Name.Substring(index + 1);
            }
            //用于Report.Option
            bool? isItemExistInDestination = null;
            //SAAS-44975，特定用户SP中有文件，选择version还原skip掉
            //暂不支持这种case，如果支持再放开
            //RECO-21172 不带着current version使用skip或者overrite方式还原文件直接跳过
            if (aveItemDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default || aveItemDto.RestoreOption.mAveRestoreMode == AveRestoreMode.OverWrite)
            {
                //不做任何操作
                log.Info($"Skip restoring files using skip or overwrite without current version.");
            }
            else
            {
                ResetRestoreModeForArchiver(aveItemDto, isVersion);
            }
            AveRestoreMode restoreMode = aveItemDto.RestoreOption.mAveRestoreMode;
            try
            {
                using (AvePerformanceScope pcAll = new AvePerformanceScope("GranularRestore.RestoreItem"))
                {
                    switch (aveItemDto.Type)
                    {
                        case AveConstants.TYPE_DOCUMENT:
                        case AveConstants.TYPE_VERSION:
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.Document"))
                            {
                                if (!CheckSODataNeedRestore(restoreStream))
                                {
                                    throw new SkipException("Looks up a localized string similar to This is a Storage Manager/Connector stub.If you want to restore it,please modify configration file..");
                                }
                                var aveDoc = new AveSPDoc(this.aveFolder, aveItemDto.Name);
                                restoredItem = aveDoc?.AveSPItem?.SPListItem;
                                aveDoc.SetStream(restoreStream);
                                aveDoc.SetRestoreOption(aveItemDto.RestoreOption);
                                var mode = ResetDocNameIfNeedAppend(aveDoc, realName, ref isItemExistInDestination, restoreStream);
                                aveDoc.RestoreOption.mAveRestoreMode = mode;
                                try
                                {
                                    if (mode == AveRestoreMode.Append && Item.Restore.AppendItemMapping.ContainsKeyAppendName(realName))
                                    {
                                        log.Info($"Processing appendName for document");
                                        appendName = Item.Restore.AppendItemMapping.GetValueAppendName(realName);
                                        log.Info($"Get append name: {appendName}");
                                        string dest = aveItemDto.SrcUrl;
                                        log.Info($"DestinationURl: {dest}");
                                        dest = dest.Replace('\\', '/');
                                        int lastSeparatorIndex = dest.LastIndexOf('/');
                                        string renamedPath = lastSeparatorIndex >= 0
                                                            ? dest[..(lastSeparatorIndex + 1)] + appendName
                                                            : appendName;
                                        srcPathAppendName = renamedPath;
                                        log.Info($"SrcPathWithAppendName: {srcPathAppendName}");
                                    }                    
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Get append name failed and set append name failed for {realName}, error: {ex}");
                                }
                                restoreMode = RestoreDocument(aveDoc, aveItemDto, reportDto, ref isItemExistInDestination, restoreStream);
                                reportDto.Size = restoreStream.ContentLength;
                            }
                            break;

                        case AveConstants.TYPE_LISTITEM:
                        case AveConstants.TYPE_LISTITEMVERSION:
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.ListItem"))
                            {
                                string tempName = aveItemDto.Name;
                                //For folder version
                                if (tempName.StartsWith(":", StringComparison.Ordinal))
                                {
                                    var folderVersion = new AveSPFolder(this.aveFolder.ParentFolder, this.aveFolder.SPFolder.Name);
                                    this.aveFolder.SetRestoreOption(aveItemDto.RestoreOption);
                                    AveMetadata metadata;
                                    var data = new Dictionary<string, object>();
                                    while ((metadata = restoreStream.ReadMetadata()) != null)
                                    {
                                        switch (metadata.MetadataType)
                                        {
                                            case AveMetadataType.DocProperty:
                                                data.Clear();
                                                metadata.GetMetadata(data);
                                                AveMetadata userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                                                var userData = new Dictionary<string, object>();
                                                if (userDataMetadata != null)
                                                {
                                                    userDataMetadata.GetMetadata(userData);
                                                }

                                                AveMetadata dataJuntionMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                                                List<Dictionary<string, object>> dataJunction = new List<Dictionary<string, object>>();
                                                if (dataJuntionMetadata != null)
                                                {
                                                    dataJuntionMetadata.GetMetadata(dataJunction);
                                                }
                                                folderVersion.RestoreSelf(data, userData, dataJunction);
                                                int folderId = data["DoclibRowId"] is DBNull ? -1 : (int)data["DoclibRowId"];
                                                reportDto.Path = ReportAbsolutePath.GetFolderVersionAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, this.AveList.SPList.DefaultDisplayFormUrl, folderId, (int)data["UIVersion"]);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    var aveListItem = new AveSPListItem(this.aveFolder, aveItemDto.Name);
                                    restoredItem = aveListItem?.AveSPItem?.SPListItem;
                                    aveListItem.SetRestoreOption(aveItemDto.RestoreOption);
                                    //aveListItem.RestoreOption.mAveRestoreMode = ResetListItemNameIfNeedAppend(aveListItem, realName, ref isItemExistInDestination, restoreStream);
                                    //#region Rename report.Name if appended
                                    //if (!string.Equals(realName, aveListItem.Name, StringComparison.OrdinalIgnoreCase))
                                    //{
                                    //    reportDto.Path = reportDto.Path.Replace(realName, aveListItem.Name);
                                    //}
                                    //#endregion
                                    restoreMode = RestoreListItem(aveListItem, aveItemDto, reportDto, ref isItemExistInDestination, restoreStream);
                                }
                                reportDto.Size = restoreStream.ContentLength > 0? restoreStream.ContentLength : 1024 * 1024;
                            }
                            break;

                        case AveConstants.TYPE_ATTACHMENTS:
                            if (this.AveList == null)
                            {
                                log.Error("AveList is null when restore item for attachments type");
                                throw new ArgumentNullException(nameof(this.AveList));
                            }
                            this.AveList.DisableListVersionSettings();
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.Attachment"))
                            {
                                if (GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                                {
                                    log.Log(AveLogLevel.INFO, "Looks up a localized string similar to Attachment will skip while restore security only..");
                                    throw new SkipException("Looks up a localized string similar to Attachment will skip while restore security only..");
                                }
                                if (!CheckSODataNeedRestore(restoreStream)) //this.aveFolder.RestoringItem.NeedSkipped ||
                                {
                                    //throw new SkipException(RestoreResource.Item_SkipRestoreStub);
                                    throw new SkipException(this.aveFolder.RestoringItem.NeedSkippedReason);
                                }
                                string attachmentName = aveItemDto.Name.Substring(aveItemDto.Name.IndexOf(':') + 1);
                                if (AppendItemMapping.ContainsKeyAppendName(realName))
                                {
                                    aveItemDto.Name = AppendItemMapping.GetValueAppendName(realName) + ":" + attachmentName;
                                }
                                var aveAtta = new AveSPAttachment(this.AveList, aveItemDto.Name);
                                aveAtta.SetStream(restoreStream);
                                aveAtta.SetRestoreOption(aveItemDto.RestoreOption);
                                //NullableBooleanExtension.SetIfValueNotExist(ref isItemExistInDestination, aveAtta.IsAttachmentExists());
                                RestoreAttachment(aveAtta, restoreStream);
                                isItemExistInDestination = false;
                                reportDto.Path = ReportAbsolutePath.GetAttachmentAP(AveList.Url, aveAtta.AttachmentInfo.RowId, attachmentName);
                                reportDto.Size = restoreStream.ContentLength;
                            }
                            break;
                    }
                    //log.Info(RestoreResource.Item_AIRRestoreItem, aveItemDto.Name, aveItemDto.Type);
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn(@"An error occurred while restore item. {0}", aveItemDto.Name, ex);
                reportDto.Status = RestoreStatus.Skipped;
                reportDto.ErrorMessage = ex.Message;
            }
            catch (SkipException e)
            {
                log.Info(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveItemDto.ItemPathMd5, e);
                reportDto.Path = null;
                reportDto.Status = RestoreStatus.Skipped;
                reportDto.ErrorMessage = e.Message;
            }
            catch (Exception e)
            {
                if (reportDto?.Path?.EndsWith("Forms/Document Set/docsethomepage.aspx") == true)
                {
                    log.Info(@$"docsethomepage.aspx is system file,error:{e}");
                    reportDto.Status = RestoreStatus.Skipped;
                }
                else if (aveFolder.RestoringItem.NeedSkipped)
                {
                    log.Info(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveItemDto.ItemPathMd5, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    reportDto.ErrorMessage = e.Message;
                }
                else
                {
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreItemFailedEventMessage(aveItemDto.Name, e));
                    reportDto.Status = RestoreStatus.Failed;
                    if (e.Message != null)
                    {
                        if (e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                        {
                            var archivedCheckItem = ResolveArchivedCheckItem(restoredItem, realName);
                            var isArchivedItem = SPSettingsUtility.ShouldSkipArchivedItem(archivedCheckItem);
                            log.Info($"Restore label-blocked item. restoredItemNull:{restoredItem == null}, archivedCheckItemNull:{archivedCheckItem == null}, isArchivedItem:{isArchivedItem}, item:{aveItemDto?.Name}");
                            reportDto.ErrorMessage = isArchivedItem
                                ? "RM_ArchiveBy365_Detail_Skip"
                                : "StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed";
                        }
                        else if (e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                        {
                            reportDto.ErrorMessage = "StorageOptimization13_SOARDeleteOfficeLockFile";
                        }
                        else
                        {
                            reportDto.ErrorMessage = e.Message;
                        }
                    }
                }
                reportDto.Path = null;
            }
            if (!(isItemExistInDestination.HasValue && isItemExistInDestination.Value))
            {
                AppendItemMapping.AddToMappingAppendVersion(realName, true);
            }
            reportDto.SetOption(restoreMode, isItemExistInDestination, reportDto.Status);
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                string resultUrl = OopStubUrl.Substring(0, OopStubUrl.LastIndexOf('.'));
                reportDto.SourcePath = resultUrl;
                reportDto.Path = aveItemDto.OopSourceUrl.Replace("\\","/");
            }
            else
            {
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Path = string.Empty;
                log.Info($"setting last value AveItemRestore");

                if (!string.IsNullOrEmpty(srcPathAppendName))
                {
                    log.Info($"AveItemRestore, Having appendSrcPath");
                    reportDto.DestinationUrl = srcPathAppendName;
                    log.Info($"AveItemRestore, DestinationUrl: {reportDto.DestinationUrl}");

                    if (!string.IsNullOrEmpty(oldUrl))
                    {
                        reportDto.SourcePath = oldUrl;
                        log.Info($"OldUrl: {reportDto.SourcePath}");
                    }
                }
                else
                {
                    log.Info($"AveItemRestore, other option case");
                    string replacePathh = reportDto.SourcePath.Replace("\\", "/");
                    reportDto.DestinationUrl = replacePathh;
                    log.Info($"AveItemRestore, DestinationUrl: {reportDto.DestinationUrl}");
                    if (IsRestoreToSPO)
                    {
                        reportDto.SourcePath = oldUrl;
                    }
                }
            }
            CheckFileTail(reportDto, restoreStream);
            if (!AveList.IsSystemList)
            {
                reportDto.GetConflictResolution(restoreMode);
                AddReport(reportDto);
                if (reportDto.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful)
                {
                    if(aveItemDto.Type == AveConstants.TYPE_DOCUMENT && aveItemDto.Name != null && !aveItemDto.Name.Contains(":"))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileCurrentVersionCount++;
                    }
                    else if(aveItemDto.Type == AveConstants.TYPE_VERSION || 
                        (aveItemDto?.Name != null && aveItemDto.Name.Contains(":") && aveItemDto.Type == AveConstants.TYPE_DOCUMENT))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileHisVersionCount++;
                    }

                    if (aveItemDto.Type == AveConstants.TYPE_LISTITEM || aveItemDto.Type == AveConstants.TYPE_LISTITEMVERSION
                        || aveItemDto.Type == AveConstants.TYPE_DOCUMENT || aveItemDto.Type == AveConstants.TYPE_VERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionCountFotTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionExpireSumTime += SOArchiverJobInfoStatistics.Instance.MainJobStartTime - aveItemDto.ArchiveTime;
                    }
                    if (aveItemDto.Type == AveConstants.TYPE_LISTITEM || aveItemDto.Type == AveConstants.TYPE_LISTITEMVERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += ContractConstants.ITEMSIZEFORLICENSE;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, aveItemDto.SrcUrl);
                    }
                    else
                    {
                        if (aveItemDto.Type != AveConstants.TYPE_ATTACHMENTS)
                        {
                            RecordRestoredFile.InsertIntoTable(aveItemDto.StorageId, aveItemDto.Id, aveItemDto.ItemPathMd5, aveItemDto.BackUpJobId, reportDto.SourcePath);
                        }
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += reportDto.Size;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(reportDto.Size, aveItemDto.SrcUrl);
                    }
                }
            }
        }


        // need handle the SrcUrl due to different format of the Url from different backup job type
        public void SetSourceFolderUrl(RestoreContentDto dto, bool isFolder)
        {
            if (dto == null || string.IsNullOrEmpty(dto.SrcUrl))
            {
                log.Warn($"SrcUrl is null or empty for item: {dto?.SrcName.LogBase64()}, type: {dto?.Type}");
                return;
            }

            string srcUrl = dto.SrcUrl;
            int tempIndex;

            // folder
            if (isFolder)
            {
                // take parent folder if selected item is folder, otherwise take itself as source folder, this is because for folder we want to restore the folder structure correctly, but for file we just care about the file itself.
                if (dto.IsSelected)
                {
                    tempIndex = srcUrl.LastIndexOf('/');
                    isSelectedFolderProcessed = true;
                    lastSelectedFolderUrl = srcUrl;
                    log.Info("Source folder url is {0}.", sourceFolderUrl);
                }
                else if (!string.Equals(sourceFolderUrl, srcUrl, StringComparison.OrdinalIgnoreCase))
                {
                    tempIndex = srcUrl.Length; // take all the source url
                    isSelectedFolderProcessed = false;
                    log.Info("Parent folder url is {0}.", sourceFolderUrl);
                }
                else
                {
                    log.Info($"Source folder url is the same as current one: {srcUrl} for item: {dto.SrcName}, type: {dto.Type}, skip update.");
                    return;
                }
            }
            // doc/doc version
            else
            {
                // Common SrcUrl format "folder path\file name"
                tempIndex = srcUrl.LastIndexOf('\\');
                if (tempIndex < 0)
                {
                    // Some other case "folder path/file name"
                    tempIndex = srcUrl.LastIndexOf('/');
                }

                // fallback if the above 2 cases are not hit
                if (tempIndex < 0 && !string.IsNullOrEmpty(dto.SrcName))
                {
                    int nameIndex = srcUrl.LastIndexOf(dto.SrcName, StringComparison.OrdinalIgnoreCase);
                    if (nameIndex > 0)
                    {
                        tempIndex = nameIndex - 1;
                    }
                }
            }

            if (tempIndex < 0)
            {
                // return back srcUrl as default if the format is unexpected
                sourceFolderUrl = isFolder ? srcUrl : "";
                log.Warn($"Unexpected SrcUrl: {srcUrl.LogBase64()}, sourceFolderUrl: {sourceFolderUrl}, item pathmd5: {dto.ItemPathMd5}, type: {dto.Type}");
                return;
            }

            srcUrl = srcUrl.Substring(0, tempIndex);
            if (!string.Equals(sourceFolderUrl, srcUrl, StringComparison.OrdinalIgnoreCase))
            {
                log.Info($"Set sourceFolderUrl to {srcUrl}, item pathmd5: {dto.ItemPathMd5}, type: {dto.Type}");
                sourceFolderUrl = srcUrl;
            }
            else
            {
                log.Info($"SourceFolderUrl is not change: {sourceFolderUrl}, skip update. item pathmd5: {dto.ItemPathMd5}, type: {dto.Type}");
            }
        }

        protected void ResetRestoreModeForArchiver(RestoreContentDto aveItemDto, bool isVersion)
        {
            //if (Config.JobType != 28 && Config.JobType != 60)
            //{
            //    return;
            //}
            if (aveItemDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM || !isVersion)
            {
                return;
            }
            //If you want to restore a single document version in Archiver, we will treat this action as AppendANewVersion all the time.
            if (aveItemDto.Type == AveConstants.TYPE_DOCUMENT || aveItemDto.Type == AveConstants.TYPE_VERSION)
            {
                log.Debug("This is a document version: " + aveItemDto.Name);
            }
            else if (aveItemDto.Type == AveConstants.TYPE_LISTITEM || aveItemDto.Type == AveConstants.TYPE_LISTITEMVERSION)
            {
                log.Debug("This is a ListItem version: " + aveItemDto.Name);
            }
            aveItemDto.RestoreOption.ResetRestoreMode((int)AveRestoreMode.AppendANewVersion);
        }

        #region Append
        protected AveRestoreMode ResetDocNameIfNeedAppend(AveSPDoc doc, string realName, ref bool? isItemExistInDestination, IAveRestoreStream restoreStream)
        {
            return ResetItemNameIfNeedAppend(doc, realName, doc.ResetAvailableName, doc.ResetName, ref isItemExistInDestination, restoreStream);
        }

        private AveRestoreMode ResetItemNameIfNeedAppend(RestoreableObject item, string realName, Func<DateTime, string> ResetAvailableName, Action<string> ResetName, ref bool? isItemExistInDestination, IAveRestoreStream restoreStream)
        {
            if (NeedAppend(item))
            {
                lock (LockerDispatcher.GetLocker("NameMappingLock"))
                {
                    if (!AppendItemMapping.ContainsKeyAppendName(realName))
                    {
                        string newName = ResetAvailableName(SourceLastModifiedTime(restoreStream));
                        AppendItemMapping.AddToMappingAppendName(realName, newName);
                        ResetName(AppendItemMapping.GetValueAppendName(realName));
                    }
                    else
                    {
                        ResetName(AppendItemMapping.GetValueAppendName(realName));
                    }
                    if (!string.Equals(realName, AppendItemMapping.GetValueAppendName(realName), StringComparison.Ordinal))
                    {
                        NullableBooleanExtension.SetIfValueNotExist(ref isItemExistInDestination, true);//Append
                        return AveRestoreMode.Append;
                    }
                    else
                    {
                        return AveRestoreMode.Default;
                    }
                }
            }
            else if (item.CheckRestoreOption(AveRestoreMode.Append))
            {
                return AveRestoreMode.Default;
            }
            return item.RestoreOption.mAveRestoreMode;
        }

        private bool NeedAppend(RestoreableObject itemObject)
        {
            if (!itemObject.CheckRestoreOption(AveRestoreMode.Append))
            {
                return false;
            }
            if (itemObject is AveSPDoc)
            {
                AveSPDoc doc = itemObject as AveSPDoc;
                if (doc.ParentFolder == null || doc.ParentFolder.ParentList == null)
                {
                    return false;
                }
                //dont need to append file if itemObject is system file or in system list
                return !(AppendUtility.CheckIsSystemList(doc.ParentFolder.ParentList) || AppendUtility.CheckIsSystemFile(doc));
            }
            if (itemObject is AveSPListItem)
            {
                AveSPListItem aveSPListItem = itemObject as AveSPListItem;
                return !AppendUtility.CheckIsSystemList(aveSPListItem.ParentList);
            }
            return false;
        }

        #endregion

        /// <summary>
        /// throw AveCloseException if media send error message in fileTail
        /// </summary>
        protected void CheckFileTail(AveRestoreReportDto reportDto, IAveRestoreStream restoreStream = null)
        {
            string tail;
            if (restoreStream == null)
            {
                tail = ContentReader.GetFileTail();
            }
            else
            {
                tail = restoreStream.ReadTail();
            }
            if (!string.IsNullOrEmpty(tail))
            {
                reportDto.Status = RestoreStatus.Failed;
                reportDto.ErrorMessage = tail;
                log.Error($"Restore failed from {reportDto.SourcePath} to {reportDto.Path}, error: {tail}");
                //SAAS-13142 comment中不能出现media的相关字眼，所以此处修改message的生成方式
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_ReceivErrorMessage.ToString(), RestoreReportResource.Item_ReceivErrorMessage, tail);
                //log.Error("AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_Unknown.ToString(), tail);");
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_Unknown.ToString(), tail);
            }
        }

        private DateTime SourceLastModifiedTime(IAveRestoreStream restoreStream)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Item.Restore.AveItemRestore.SourceLastModifiedTime"))
            {

                AveMetadata metadata;
                DateTime sourceDateTime = DateTime.MinValue;
                if ((metadata = restoreStream.TryReadMetadata(AveMetadataType.DocProperty)) != null)
                {
                    var allDocData = metadata.GetMetadata<Dictionary<string, object>>();
                    if (allDocData != null && allDocData.ContainsKey("BiggestVersionModified"))
                    {
                        DateTime.TryParse(allDocData["BiggestVersionModified"].ToString(), out sourceDateTime);
                    }
                }
                return sourceDateTime;
            }
        }
        /// <summary>
        /// Throw a SkipException if RestoreSOData is true And is out of place And backup stub only
        /// </summary>
        /// <returns>return fasle only if the document is SO Data and RestoreSOData Option is false</returns>
        protected bool CheckSODataNeedRestore(IAveRestoreStream restoreStream)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("GranularRestore.CheckSODataNeedRestore"))
            {
                AveMetadata metadata;
                AveStorageInfo storageInfo;
                if ((metadata = restoreStream.TryReadMetadata(AveMetadataType.DocStorageInfo)) != null)
                {
                    if ((storageInfo = metadata.GetMetadata<AveStorageInfo>()) != null)
                    {
                        if (!Config.RestoreSOData)
                        {
                            return storageInfo.StorageType == AveStorageType.None;
                        }
                        else if (Config.IsOutOfPlaceRestore && storageInfo.IsBackupLinkForArchivedData)
                        {
                            throw new SkipException("Looks up a localized string similar to The Storage Manager/Connector stubs cannot be restored in an out of place restore if you backup the stubs only..");
                        }
                    }
                }
                return true;
            }

        }
        internal int GetNewVersionForDuplicateItem(int dMaxVersion, int sVersion)
        {
            int minor = dMaxVersion % 512;
            int maxjor = dMaxVersion / 512;
            int newVersion;
            if (minor > 0)
            {
                newVersion = dMaxVersion + 1;  //SAAS-11353 修改新的小version为最后的version+1
                //newVersion = (maxjor + 1) * 512 + sVersion;
            }
            else
            {
                newVersion = (maxjor + 1) * 512; ;   //SAAS-11530 修改新的大version为最后version+1.
                //newVersion = maxjor * 512 + sVersion;
            }
            return newVersion;
        }

        private void DisposeSite()
        {
            if (AveSite != null)
            {
                AveSite.Dispose();
                AveSite = null;
            }
        }

        private void DisposeWeb()
        {
            if (AveWeb != null)
            {
                AveWeb.Dispose();
                AveWeb = null;
            }

            if (oriAveWeb != null)
            {
                oriAveWeb.Dispose();
                oriAveWeb = null;
            }
        }


        protected AveSPFolder GenerateFolder(AveSPFolder curentFolder, string subPath)
        {
            if (string.IsNullOrEmpty(subPath))
            {
                return this.aveListRootFolder;
            }
            var parentFolder = aveListRootFolder;
            string folderName = subPath;
            if (subPath.IndexOf('\\') > 0)
            {
                if (curentFolder == null)
                {
                    curentFolder = aveListRootFolder;
                }
                folderName = subPath.Substring(subPath.LastIndexOf('\\') + 1);
                parentFolder = GetFolder(curentFolder, GetFolderPath(curentFolder), subPath.Substring(0, subPath.LastIndexOf('\\')));

            }
            return new AveSPFolder(parentFolder, folderName);
        }

        private string GetFolderPath(AveSPFolder folder)
        {
            if (folder.ParentFolder == null) return string.Empty;
            string parentPath = GetFolderPath(folder.ParentFolder);
            return String.Concat(parentPath, "\\", folder.Name).Trim('\\'); ;
        }

        private AveSPFolder GetFolder(AveSPFolder curentFolder, string curentFolderPath, string folderPath)
        {
            if (curentFolderPath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
            {
                while (!string.Equals(curentFolderPath, folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    curentFolderPath = curentFolderPath.Substring(0, curentFolderPath.Length - curentFolder.Name.Length).TrimEnd('\\');
                    curentFolder = curentFolder.ParentFolder;
                }
                return curentFolder;
            }

            string parentFolderPath = string.Empty;
            string folderName = folderPath;
            if (folderPath.IndexOf('\\') > 0)
            {
                parentFolderPath = folderPath.Substring(0, folderPath.LastIndexOf('\\'));
                folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
            }
            var parentFolder = GetFolder(curentFolder, curentFolderPath, parentFolderPath);
            curentFolder = new AveSPFolder(parentFolder, folderName);
            curentFolder.InitSPFolder(true);
            return curentFolder;
        }

        private static void ProcessPostAction(RestoreContentDto dto, ref AveSPSite site, ref AveSPWeb web, ref AveSPList list)
        {
            try
            {
                if ((dto.Type == AveConstants.TYPE_LIST || dto.Type == AveConstants.TYPE_WEB ||
                     dto.Type == AveConstants.TYPE_SITE) && list != null)
                {
                    AvePostAction.ListPostAction(list);
                    list = null;
                }
                if (dto.Type == AveConstants.TYPE_PROJECT)
                {
                    if (list != null)
                    {
                        AvePostAction.ListPostAction(list);
                        list = null;
                    }
                    if (site != null)
                    {
                        AvePostAction.ProjectPostAction(site);
                    }
                }
                if ((dto.Type == AveConstants.TYPE_WEB || dto.Type == AveConstants.TYPE_SITE) && web != null)
                {
                    AvePostAction.WebPostAction(web, false);
                    web = null;
                }
                if (dto.Type == AveConstants.TYPE_SITE && site != null)
                {
                    AvePostAction.SitePostAction(site);
                    site = null;
                }
            }
            catch (Exception e)
            {
                log.Warn(@"Looks up a localized string similar to An error occurred while doing post action. Error Message:{0}.", e);
            }
        }

        private static void LastPostAction(AveSPSite site, AveSPWeb web, AveSPList list)
        {
            if (list != null)
            {
                AvePostAction.ListPostAction(list);
            }
            if (web != null)
            {
                AvePostAction.WebPostAction(web, true);
            }
            if (site != null)
            {
                AvePostAction.SitePostAction(site);
            }
        }

        protected bool IsRelatedVersionsContainsThis(Dictionary<string, object> data, AveSPItem item, IAveRestoreStream restoreStream, bool isItem = false)
        {
            bool isIncluded = true;
            try
            {
                if (isItem)
                {
                    return isIncluded;
                }
                else
                {
                    //bool isVersion = data.ContainsKey("IsUserDocVersion") ? (bool)data["IsUserDocVersion"] : false;
                    int rowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
                    ItemVersionFilter cF = ItemVersionFilter.GetInstance(item, restoreStream.TryReadMetadata(AveMetadataType.DocVersions), rowId);
                    if (cF != null && !cF.RestoreVersions.Contains((int)data["UIVersion"]))
                    {
                        isIncluded = false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Looks up a localized string similar to Cannot check whether this version needs to be restored and it will be restored by default.\n{0}.", ex.ToString());
            }
            return isIncluded;
        }
        protected string GetUIVersionString(int uiVersion)
        {
            return (uiVersion / 512).ToString() + '.' + (uiVersion % 512).ToString();
        }

        public void ItemLevelRestoreItemCTAndFields(Dictionary<string, object> userData, List<Dictionary<string, object>> junctionData, RestoreableObject aveObject)
        {
            if (GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly() ||
                WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
            {
                EnsureRequiredFieldLink(userData, aveObject);
                return;
            }
            using (AvePerformanceScope scope = new AvePerformanceScope("GranularRestore.ItemLevelRestoreCTAndFields"))
            {
                AveSPItem aveSPItem;
                if (aveObject is AveSPDoc)
                {
                    aveSPItem = (aveObject as AveSPDoc).AveSPItem;
                }
                else if (aveObject is AveSPListItem)
                {
                    aveSPItem = (aveObject as AveSPListItem).AveSPItem;
                }
                else if (aveObject is AveSPFolder)
                {
                    aveSPItem = (aveObject as AveSPFolder).EnsureCTFieldItem;
                }
                else
                {
                    throw new ArgumentNullException("Looks up a localized string similar to Argument can not be null while restore item dependency..");
                }

                AveFieldRestoreOption fieldRestoreOptions = new AveFieldRestoreOption();
                bool itemSchemaDependency = false;
                bool skipItemWhenConflict = false;
                fieldRestoreOptions.FindOption = new FieldFindOption[] { FieldFindOption.FindBySchema, FieldFindOption.FindById, FieldFindOption.FindByInternalName, FieldFindOption.FindByStaticName };

                AveContentTypeRestoreOption ContentTypeRestoreOption = new AveContentTypeRestoreOption();
                ContentTypeRestoreOption.FindOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindBySchema, ContentTypeFindOption.FindById, ContentTypeFindOption.FindByName, ContentTypeFindOption.FindByParent };
                ContentTypeRestoreOption.FindScope = new ContentTypeFindScope[] { ContentTypeFindScope.Current, ContentTypeFindScope.Parent, ContentTypeFindScope.Children };
                ContentTypeRestoreOption.CreateOption = new ContentTypeCreateOption[] { ContentTypeCreateOption.UseId, ContentTypeCreateOption.ForceCreate, ContentTypeCreateOption.UseParent };
                ContentTypeRestoreOption.GetParentOption = GetParentContentTypeOption.Default;
                switch (Config.ItemDependencyType)
                {
                    case ItemDependencyOption.NotRestore:
                        itemSchemaDependency = false;
                        skipItemWhenConflict = true;
                        break;
                    case ItemDependencyOption.Overwrite:
                        itemSchemaDependency = true;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Overwrite;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Overwrite;
                        break;
                    case ItemDependencyOption.Append:
                        itemSchemaDependency = true;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Append;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.AppendDestinationWin;
                        break;
                    case ItemDependencyOption.SkipConfilctItem:
                        itemSchemaDependency = true;
                        skipItemWhenConflict = true;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Skip;
                        break;
                    case ItemDependencyOption.IgnoreDifference:
                        itemSchemaDependency = true;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Skip;
                        break;
                }
                try
                {
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IsRestoreToSPOLibOrFolder)
                    {
                        skipItemWhenConflict = true;
                        itemSchemaDependency = false;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Skip;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                    }
                    aveSPItem.EnsureItemSchemaDependency(userData, junctionData, itemSchemaDependency, !itemSchemaDependency, skipItemWhenConflict, ContentTypeRestoreOption, fieldRestoreOptions, ThrowExceptionWhenRestoreItemCTAndFields);
                }
                catch (AveSchemaDependencyConflictException ce)
                {
                    throw new SkipException(ce.Message, ce);
                }
                catch (AveSchemaDependencyNotFoundException ne)
                {
                    throw new SkipException(ne.Message, ne);
                }
            }
        }

        public void EnsureRequiredFieldLink(Dictionary<string, object> userData, RestoreableObject aveObject)
        {
            log.Info("End User Job EnsureRequiredFieldLink");
            AveSPItem aveSPItem;
            if (aveObject is AveSPDoc)
            {
                aveSPItem = (aveObject as AveSPDoc).AveSPItem;
            }
            else if (aveObject is AveSPListItem)
            {
                aveSPItem = (aveObject as AveSPListItem).AveSPItem;
            }
            else if (aveObject is AveSPFolder)
            {
                aveSPItem = (aveObject as AveSPFolder).EnsureCTFieldItem;
            }
            else
            {
                log.Warn("Looks up a localized string similar to Argument can not be null while restore item dependency.");
                aveSPItem = null;
            }
            try
            {
                if (aveSPItem != null)
                {
                    aveSPItem.EnsureRequiredFieldLink(userData);
                }
            }
            catch (Exception ex)
            {
                log.Warn($"End User Job EnsureRequiredFieldLink.Message:{ex}.");
            }
        }

    }
}

