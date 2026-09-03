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

namespace AvePoint.RA.ScheduleJob.Teams;

#region Namespaces

using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Item.Restore;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ExchangeBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using AvePoint.Wrapper.Common;
using M365GroupTeam;
using Office365GroupRestore;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAArchiverCommon.TeamsController;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#endregion

public class RestoreActionController
     : IRestoreActionController
{
    readonly RALogger logger = RALogger.GetInstance(typeof(RestoreActionController));

    private String _jobId;
    private String _mainJobId;
    private JobType _jobType;
    private int _subJobIndex = 1;
    private ERMessage _jobMessage;
    private RestoreConfig _config;
    private IReportCenter _reporter { get; set; }
    private JobManagementService _jobService;
    private RestoreSettingAndTree _restoreSettingAndTree;
    private string _groupSiteUrl;
    private string _m365TenantId;
    private IEnumerable<string> _allArchivedChannelSiteURLs;

    private IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService<IBrowseTreeService>();
    private IRMArchiverSettingsService ArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
    private IRMTeamsSettingsService TeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
    private IJobMonitorDao MainJobDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
    private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
    private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

    public IRestoreActionController Build(string jobId, JobType jobtype)
    {
        _jobId = jobId;
        _jobType = jobtype;
        _jobService = new JobManagementService();
        var mConfiguration = new ScheduleConfiguration(jobId);
        AveEnv.AgentJobFolder = Path.Combine(mConfiguration.ArchiveTemp, "Job");
        InitJobContext();

        CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
        {
            JobType = jobtype,
            MainJobId = _mainJobId,
            SubJobId = jobId
        });
        CompoundDisposalStatistics.Instance.StartStatistic();

        CspCommunicationWrapper.CommunicationEncryptionKey = PlatformWindsorManager.GetService<ISettingProfileService>().GetCommunicationEncryptionKey();
        return this;
    }

    public async Task RunAsync()
    {
        try
        {
            logger.Info("Controller run now. JobId: {0}.", _jobId);

            _reporter = (new ReportCenter()).Build(_jobType, _jobId);

            _jobMessage = _jobService.GetRestoreMessage(_jobId, _jobType, this._restoreSettingAndTree.Setting, this._restoreSettingAndTree.Tree.FirstOrDefault(), _groupSiteUrl, _m365TenantId);

            _config = new RestoreConfig(_jobMessage)
            {
                IsSupportLockedSite = _restoreSettingAndTree.Setting.IsSupportLockedSite,
            };

            //_options = BuildMedia(_options);

            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
            AvePoint.Media.Service.DomainModel.MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
            AvePoint.Media.Service.DomainModel.MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");

            //AuthorizationManager.Instance.Init(_jobMessage.EmailBposInfoMap);

            Rehydrate();

            IRestoreService restoreService = new ExchangeRestoreService();

            restoreService.Open(_config);

            using (var handler = new RestoreDataHandlerBatch())//stodo WorkerServiceLocator.GetRequiredService<IRestoreDataHandlerBase>())
            {
                handler.Start(_config, restoreService);

                var data = handler.GetDateBlockCollection();

                var restoreWorker = new RestoreExecutorBatch();
                //var executor = Message.Config.RestoreType switch
                //{
                //    EORestoreType.InPlace or
                //    EORestoreType.OutOfPlace => WorkerServiceLocator.GetService<IRestoreExecutor>(s => s.IsType(typeof(RestoreExecutorBatch))),
                //    //EORestoreType.ToStorage or
                //    //_ => WorkerServiceLocator.GetService<IRestoreExecutor>(s => s.IsType(typeof(RestoreToStorageExecutorBatch))),
                //};

                restoreWorker.Build(_reporter).Build(data).Build(_config).Execute();
                restoreService.DeleteTempFile();

                PerformanceMonitor.WritePerformanceResult();

                if (_config.RestoreType == GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.EORestoreType.InPlace)
                {
                    await RestoreRelateSPDataAsync(restoreWorker);
                }

                try
                {
                    restoreWorker.PostAction();
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while PostAction. Ex: {e}.");
                }
            }
        }
        catch (AveSkipLockSiteException ex)
        {
            logger.Error($"An error occurred while restoring related SP data. The site {ex.SiteCollectionUrl} is locked. Ex: {ex}.");
            _reporter.SetErrorMessage(ex.Message);
            return;
        }
        catch (Exception e)
        {
            logger.Error("An error occurred while initializing backup configuration. Reason: {0}.", e);
            var message = (e is ArgumentNullException && e.Message.Contains("Parameter name: BposInfo")) ? "Service.Common_bee74828-5326-4778-9905-6866c92196a1" : e.Message;
            _reporter.SetErrorMessage(message);
            return;
        }
        finally
        {
            CompoundDisposalStatistics.Instance.PrepareEndStatistic();
            CompoundDisposalStatistics.Instance.WaitEndStatistic();
            _reporter.Finish();
        }
    }

    private void InitJobContext()
    {
        RMSubJob subJobWithContext = SubJobDao.GetSubJob(_jobId, true);
        this._m365TenantId = subJobWithContext.O365TenantId;
        var jobContextSettings = subJobWithContext.JobContext?.Settings;
        ThrowUtil.ThrowIfNullOrEmpty(jobContextSettings, "job context info empty.");
        this._restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(jobContextSettings);
        this._mainJobId = subJobWithContext.ParentId;

        var groupMailbox = this._restoreSettingAndTree.Tree.FirstOrDefault().Name;
        this.LoadAllArchivedSiteURLs(groupMailbox);

        logger.Info($"Support locked sites for Teams & Groups: {_restoreSettingAndTree.Setting.IsSupportLockedSite}");
    }

    private void LoadAllArchivedSiteURLs(string groupMailbox)
    {
        var allIndex = DaoService.CommonSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(groupMailbox);
        HashSet<string> allChannelSiteUrls = new HashSet<string>();
        string rootSiteUrl = null;
        foreach (var index in allIndex)
        {
            if(string.IsNullOrEmpty(index.Extension))
            {
                continue;
            }
            var extOjb = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(index.Extension);
            if (string.IsNullOrEmpty(extOjb?.SPGroupSiteURL))
            {
                continue;
            }

            if (string.IsNullOrEmpty(this._groupSiteUrl))
            {
                this._groupSiteUrl = extOjb.SPGroupSiteURL;
                rootSiteUrl = new Uri(extOjb.SPGroupSiteURL).GetLeftPart(UriPartial.Authority);
            }

            if(extOjb.ChannelSiteRelativeURLs != null)
            {
                foreach (var relativeUrl in extOjb.ChannelSiteRelativeURLs)
                {
                    var channelSiteUrl = rootSiteUrl + relativeUrl;
                    if (!this._groupSiteUrl.EqualIgnoreCase(channelSiteUrl))
                    {
                        allChannelSiteUrls.Add(channelSiteUrl);
                    }
                }
            }

            TeamsRestoreState.IsChannelSiteReadOnly |= extOjb.IsChannelSiteReadOnly;
        }
        _allArchivedChannelSiteURLs = allChannelSiteUrls;
    }

    private async Task RestoreRelateSPDataAsync(RestoreExecutorBatch restoreExecutorBatch)
    {
        logger.Info($"start restore for related SP data.");
        try
        {
            var teamsTreeNodeDto = this._restoreSettingAndTree.Tree.FirstOrDefault();
            var browseTreeNode = RMDtoConverter.ConvertSPTree2RMSampleTree(teamsTreeNodeDto);
            var groupMailbox = teamsTreeNodeDto.Name;

            logger.Info($"process node is teams group level node. Load all site under teams {groupMailbox}, channel site count: {_allArchivedChannelSiteURLs.Count()}.");
            if (TeamsRestoreState.IsAllowRestoreGroupSite)
            {
                await RestoreRelatedSPSite(_groupSiteUrl);
            }
            else
            {
                logger.Info($"Not allow restore group site, maybe group restore fail.");
            }

            bool isTeamsUnarchivedForLockedChannelSite = !_restoreSettingAndTree.Setting.IsSupportLockedSite;
            foreach (var channelSiteUrl in _allArchivedChannelSiteURLs)
            {
                if (!TeamsRestoreState.IsAllowRestoreChannelSite(channelSiteUrl))
                {
                    logger.Warn($"The channel site {channelSiteUrl} is not allowed to run restore job.");
                    continue;
                }
                if (!isTeamsUnarchivedForLockedChannelSite)
                {
                    // Only check the first channel site, if it is locked, unarchive Teams for all channel sites.
                    isTeamsUnarchivedForLockedChannelSite = await IsTeamsUnarchivedForLockedChannelSiteAsync(restoreExecutorBatch, channelSiteUrl);
                }
                await RestoreRelatedSPSite(channelSiteUrl);
            }
            if (_restoreSettingAndTree.Setting.IsSupportLockedSite && isTeamsUnarchivedForLockedChannelSite)
            {
                var archiveResult = TryDoActionWithTryUnlockTeamsSite("ArchiveTeams", SiteState.ReadOnly, () => restoreExecutorBatch.ArchiveTeams(true));
                if (archiveResult)
                {
                    logger.Info($"Successfully archive Teams after restore related SP data.");
                }
                else
                {
                    logger.Warn($"Failed to archive Teams after restore related SP data.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to restore related SP data. Exception: {ex}");
            throw;
        }
        finally
        {
            // reset the report jobcontext back from virtual subjob to the teams subjob
            _reporter.ResetReportManager(_jobId);
        }
    }

    private async Task<bool> IsTeamsUnarchivedForLockedChannelSiteAsync(RestoreExecutorBatch restoreExecutorBatch, string channelSiteUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(channelSiteUrl)
                || !restoreExecutorBatch.IsTeamsArchived())
            {
                return false;
            }
            logger.Info($"The Teams is archived, will check if the channel site {channelSiteUrl} is locked.");
            var isSiteLocked = await IsSiteLockedAsync(channelSiteUrl, SiteState.ReadOnly);
            var result = isSiteLocked.HasValue && isSiteLocked.Value;
            if (result)
            {
                logger.Info($"The channel site {channelSiteUrl} is locked, will temporarily unarchive Teams.");
                var unarchiveResult = TryDoActionWithTryUnlockTeamsSite("UnarchiveTeams", SiteState.ReadOnly, () => restoreExecutorBatch.UnArchiveTeams());
                if (!unarchiveResult)
                {
                    logger.Warn($"Failed to unarchive Teams for channel site {channelSiteUrl}.");
                    return false;
                }
                logger.Info($"The Teams has been temporarily unarchived.");
            }
            return result;
        }
        catch (AveSkipLockSiteException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.Error($"Failed to check if the channel site {channelSiteUrl} is processed for unarchive Teams. Exception: {e}");
            return false;
        }
    }

    private T TryDoActionWithTryUnlockTeamsSite<T>(string actionName, SiteState siteState, Func<T> action)
    {
        logger.Info($"Start to try do action '{actionName}' with try unlock teams site.");
        try
        {
            using var _ = new SiteStateTransitionScopeUtility(_groupSiteUrl, siteState, true, true);
            T result = action();
            return result;
        }
        catch (AveSkipLockSiteException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to try do action '{actionName}' with try unlock teams site: {ex.Message}", ex);
            return default;
        }
    }

    private async Task<bool?> IsSiteLockedAsync(string siteUrl, SiteState checkingState)
    {
        try
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                return null;
            }
            var remoteSiteCollection = new RemoteSiteCollection()
            {
                url = siteUrl,
                TenantId = _m365TenantId,
            };
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, siteUrl);
            logger.Info($"O365 Admin Url is : {mAdminUrl}");
            var aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
            if (aveTenant.TryGetAdminUrlForMultiGeoTenant(siteUrl, out string geoAdminUrl))
            {
                logger.Info($"O365 Tenant is a multiple geo tenant, admin Url is : {geoAdminUrl}");
                mAdminUrl = geoAdminUrl;
                aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
            }
            var siteProps = aveTenant.GetSitePropertiesByUrl(siteUrl);
            logger.Info($"Current site lock state is: {siteProps.LockState}");
            if (siteProps.LockState.EqualIgnoreCase(checkingState.ToString()))
            {
                logger.Info($"Current site is locked.");
                return true;
            }
        }
        catch (Exception e)
        {
            logger.Info($"Error occur when check site lock. Message: {e}.");
        }
        return false;
    }

    private async Task RestoreRelatedSPSite(string siteUrl)
    {
        var siteNode = new SPTreeNodeDto()
        {
            Name = siteUrl,
            DisplayName = siteUrl,
            FullPath = siteUrl,
            SitePath = siteUrl,
            CanChildrenBeLoaded = true,
            CheckNumber = 1,
            ChildrenLoaded = true,
            Expanded = true,
            IncludeNew = IncludeNewState.Checked,
            Level = NodeLevel.SiteCollection,
            SelectAll = SelectAllState.Checked,
            Property = PropertyState.Checked,
            Security = SecurityState.Checked,
            //ID = ?,
            //SPObjectId = ?,
        };

        var restoreSetting = new RestoreSettingAndTree()
        {
            Tree = new List<SPTreeNodeDto>() { siteNode },
            Setting = _restoreSettingAndTree.Setting,
            JobId = _jobId,
            SiteGroupId = _restoreSettingAndTree.SiteGroupId,
            IsEndUserJob = _restoreSettingAndTree.IsEndUserJob,
            ConnectionString = _restoreSettingAndTree.ConnectionString,
            NodeType = _restoreSettingAndTree.NodeType,
            IsOpusArchivedDownloadJob = _restoreSettingAndTree.IsOpusArchivedDownloadJob,
            RealRunJobUser = TenantLocalValue.LogonUserEmail,
            IsRecenterExport = _restoreSettingAndTree.IsRecenterExport,
            oopStubUrl = _restoreSettingAndTree.oopStubUrl,
            BackUpJobId = _restoreSettingAndTree.BackUpJobId,
        };

        var subJobId = CreateSubJob(_subJobIndex++, _jobType, restoreSetting, siteNode.FullPath);

        AbstractAveItemRestore archiverRestore = new AveItemRestoreMain(subJobId, _jobType);
        WrapperRuntime.ClearGlobalContext();
        await archiverRestore.RunNowAsync();
    }

    private string CreateSubJob(int currentSubjobIndex, JobType jobType, object jobSettings, string scope)
    {
        string subJobId = string.Format(_jobId + "{0:D3}", currentSubjobIndex);
        var subJob = new RMSubJob()
        {
            Id = subJobId,
            ParentId = _mainJobId,
            StartTime = DateTime.UtcNow.Ticks,
            JobType = (int)jobType,
            Progress = 0,
            Status = (int)JobStatus.InProgress,
            Weight = 0,
            Runable = RecordsConstants.SubJob_Runnable_Exclude,
        };
        subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings) };
        subJob.String1 = scope;
        SubJobDao.CreateJob(subJob);
        logger.Info($"Create sub job {subJob.Id} sucessfull, type {subJob.JobType}, Scope {scope}");
        return subJobId;
    }

    private void Rehydrate()
    {
        //logger.Info("Rehydrate: Start to rehydrate.");
        //TryRehydrate();
        //logger.Info("Rehydrate: End to rehydrate.");
        //ResetVolume();
        //logger.Info("Rehydrate: End to reset volume.");
    }

    //private void RemoveDeletedSites()
    //{
    //    var siteUrl = _allArchivedChannelSiteURLs.FirstOrDefault();
    //    if (string.IsNullOrEmpty(siteUrl)) return;
    //    AveBPOSAccountInfo siteAccount = null;
    //    try
    //    {
    //        RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
    //        if (remoteSiteCollection != null && !string.IsNullOrEmpty(remoteSiteCollection.TenantId))
    //        {
    //            logger.Info($"RestoreSiteForOpus remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.");
    //            siteAccount = PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection).Result;
    //            logger.Info($"RestoreSiteForOpus finished remoteSiteCollection != null TenantID:{remoteSiteCollection.TenantId}.siteAccount is null:{siteAccount == null}.");
    //        }
    //        else
    //        {
    //            var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
    //            foreach (var temp in profiles)
    //            {
    //                logger.Info($"RestoreSiteForOpus siteAccount == null profile Name is:{temp.Name}.DomainName:{temp.DomainName}.");
    //                if (siteUrl.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
    //                {
    //                    var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(temp.TenantId).GetAwaiter().GetResult().AdminUrl;

    //                    siteAccount = new Wrapper.Common.AveBPOSAccountInfo()
    //                    {
    //                        TenantId = temp.TenantId,
    //                        AdminUrl = adminUrl,
    //                        ClientId = temp.AppClientId,
    //                        ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
    //                        TenantGroupId = TenantLocalValue.LogonGroupId,
    //                        AuthenticationProfileId = temp.Id,
    //                        AppType = ConvertIdentityTypeToAppType(temp.Type),
    //                        AADEnvironment = (Microsoft365.Authentication.AveAzureEnvironment)temp.AADEnvironment,
    //                        //AppCert = apponlyCertificate
    //                    };
    //                    break;
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception eg)
    //    {
    //        logger.Info("Try get user information & url error {0}", eg.ToString());
    //    }
    //    try
    //    {
    //        if (siteAccount == null)
    //        {
    //            logger.Error("SiteAccount is null");
    //            throw new Exception("Site Account is null");
    //        }
    //        //AveBPOSAccountInfo siteAccount = ItemRestoreConfig.BPOSSiteCollectionConfig[aveSiteDto.Name];
    //        //logger.Info("get user for delete recyclebin site {0}.", aveSiteDto.Name);
    //        AveObjectModelFactory tenantFactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, siteAccount, AveContextKind.ClientObjectModel);
    //        logger.Info("RestoreSiteForOpus.O365 Admin Url is : {0}.", siteAccount.AdminUrl);
    //        IAveTenant aveTenant = tenantFactory.CreateTenant(siteAccount.AdminUrl);
    //        var geoLocationInfo = aveTenant.GetTenantGeoLocationinfo();
    //        if (geoLocationInfo != null && geoLocationInfo.Count > 1)
    //        {
    //            foreach (var location in geoLocationInfo)
    //            {
    //                if (siteUrl.StartsWith(location.RootSiteUrl) || siteUrl.StartsWith(location.MySiteHostUrl))
    //                {
    //                    siteAccount.AdminUrl = location.TenantAdminUrl;
    //                    //logger.Info($"RestoreSiteForOpus.O365 Admin New Url is : {siteAccount.AdminUrl}.SiteUrl:{aveSiteDto.SrcUrl}.");
    //                }
    //            }
    //        }
    //        logger.Info("Create Container Admin URL {0}", siteAccount.AdminUrl);
    //        IAveSite site = tenantFactory.CreateAdminCenterSite(siteAccount.AdminUrl);
    //        logger.Info("Create admin center site: {0} successfully.", siteAccount.AdminUrl);
    //        var tenant = tenantFactory.CreateTenant(site);//aveSiteDto.SiteUrl);
    //        foreach (var channelSite in _allArchivedChannelSiteURLs)
    //        {
    //            try
    //            {
    //                bool conflictWithRecycleBin = IsSiteExistInRecycleBin(tenant, channelSite);
    //                if (conflictWithRecycleBin)
    //                {
    //                    string message = string.Format("Site {0} conflict with an existing site in recycle bin.", channelSite);
    //                    tenant.RemoveDeletedSite(channelSite);
    //                    logger.Info($"Remove site {channelSite} from RecycleBin success.");
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                logger.Error($"Remove from RecycleBin site {channelSite} Error {e}");
    //            }
    //        }
    //    }
    //    catch (Exception e1)
    //    {
    //        logger.Error("Remove from RecycleBin Error {0}", e1.ToString());
    //    }
    //}
    //private bool IsSiteExistInRecycleBin(IAveTenant tenant, string siteUrl)
    //{
    //    var exist = false;
    //    try
    //    {
    //        var properties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
    //        exist = true;
    //    }
    //    catch (Exception e)
    //    {
    //        logger.Info("Check site in recycle bin failed {0}.Error:{1}", siteUrl, e);
    //    }
    //    return exist;
    //}

    //private GCommon.Contract.CentralAdmin.Object.AppType ConvertIdentityTypeToAppType(Cloud.Sdk.Data.AosModern.IdentityProviderType providerType)
    //{
    //    return providerType switch
    //    {
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.Office365 => GCommon.Contract.CentralAdmin.Object.AppType.Office365,
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.SharePoint => GCommon.Contract.CentralAdmin.Object.AppType.SharePoint,
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.Exchange => GCommon.Contract.CentralAdmin.Object.AppType.Exchange,
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomAzureApp => GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp,
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomDelegateApp => GCommon.Contract.CentralAdmin.Object.AppType.CustomDelegateApp,
    //        Cloud.Sdk.Data.AosModern.IdentityProviderType.CloudRecords => GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords,
    //        _ => GCommon.Contract.CentralAdmin.Object.AppType.Office365,
    //    };
    //}

}