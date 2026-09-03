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
namespace AvePoint.RA.ScheduleJob.Teams
{
    #region Namespaces

    using AvePoint.Archiver.Media;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
    using AvePoint.GCommon.Contract.StorageOptimization.Connector;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon.Utility.TransientFault;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Global.Utils;
    using AvePoint.RA.Common.JobService;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Common;
    using AvePoint.RA.Contract.DocAve;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.FunctionSetting;
    using AvePoint.RA.Contract.JobMonitor;
    using AvePoint.RA.Contract.Object;
    using AvePoint.RA.Contract.RMRuleManageMent;
    using AvePoint.RA.Contract.RMWeb;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.Setting;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.Contract.Schedule;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.DB.Model;
    using AvePoint.RA.RACommonUtility;
    using AvePoint.RA.RAExchange.Disposal;
    using AvePoint.RA.SharePoint.Archiver;
    using AvePoint.RA.SharePoint.Archiver.Common;
    using AvePoint.RA.SharePoint.ArchiverCommon;
    using AvePoint.RA.SharePoint.Common;
    using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
    using AvePoint.Wrapper.Common;
    using ExchangeUtility.Graph;
    using M365GroupTeam;
    using Microsoft.SqlServer.Management.SqlParser.Parser;
    using Newtonsoft.Json;
    using Office365GroupBackup;
    using RAArchiverCommon;
    using RAArchiverCommon.TeamsController;
    using RAGoogle.Archive.Scan;
    using RATeams;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NodeType = GCommon.Contract.Tree.Object.NodeType;

    #endregion

    public class TeamsRuleActionController : IRuleActionController
    {
        readonly RALogger logger = RALogger.GetInstance(typeof(TeamsRuleActionController));

        private String _jobId;
        private JobType _jobType;
        private int _relatedSubJobIndex = 1;
        private ExchangeOnlineMessage _jobMessage;
        private BackupOptions _options;
        private bool _enableCGScan;

        private IReportCenter _reporter { get; set; }
        private RMSPTreeNode? _treeNode;
        private string _groupMailbox;
        private string _groupSiteUrl;
        private string _o365Id;
        private string _mainJobId;
        private int _expectedVirtualSubJobCount = 1; // default value is 1 for group site
        private readonly int defaultTeamsPhaseCount = 2; // default value is 2 for teams data and mailbox subjob

        private JobManagementService _jobService { get; set; }
        private TeamsSODashboardWorker _teamsSODashboardWorker { get; set; }

        public bool IsSOMethod => _jobType == JobType.TeamsArchiverBackup || _jobType == JobType.SpecifyTeamsArchiverBackup;

        public bool ShouldRunInTeamsController()
        {
            return _treeNode?.Level == (int)NodeLevel.Office365GroupEntire || IsMailboxRealChildNode();
        }

        #region Service & DAO
        private IRMArchiverSettingsService ArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRMTeamsSettingsService TeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMScheduleDao ScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private ITeamsSettingTreeService TeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        #endregion


        public IRuleActionController Build(string jobId, JobType jobtype)
        {
            _jobId = jobId;
            _jobType = jobtype;
            _enableCGScan = KeyValueService.IsEnableCGScan();
            _jobService = new JobManagementService();
            _teamsSODashboardWorker = new TeamsSODashboardWorker();
            var mConfiguration = new ScheduleConfiguration(jobId);
            AveEnv.AgentJobFolder = Path.Combine(mConfiguration.ArchiveTemp, "Job");
            InitJobContext();
            CspCommunicationWrapper.CommunicationEncryptionKey = PlatformWindsorManager.GetService<ISettingProfileService>().GetCommunicationEncryptionKey();

            return this;
        }

        private int GetTotalPhases()
        {
            if (_jobType == JobType.TeamsPreScan)
            {
                return 0; // use normal update progress for pre-scan job
            }
            var totalPhases = 0;
            if (IsSOMethod && IsTeamsRealChildNode())
            {
                totalPhases += defaultTeamsPhaseCount;
            }

            totalPhases += _expectedVirtualSubJobCount;

            logger.Info($"Total phases for job {_jobId} is: {totalPhases}, expectedVirtualSubJobCount: {_expectedVirtualSubJobCount}");
            return totalPhases;
        }

        public async Task RunAsync()
        {
            try
            {
                logger.Info("Controller run now. JobId: {0}.", _jobId);

                var totalPhases = GetTotalPhases();

                _reporter = (new ReportCenter()).Build(_jobType, _jobId, totalPhases);

                // Initialize progress tracking for this job
                if (JobServiceUtility.NewJobDetailsJobs.Contains((int)_jobType))
                {
                    JobExecutionProgressStatisticExecutor.Instance.InitializeJobExecutionProgressStatictics(
                        scope: _groupMailbox,
                        subJobId: _jobId,
                        mainJobId: _mainJobId,
                        jobType: (int)_jobType,
                        isInitStartTime: true,
                        isTeams: true);
                }

                if (IsMailboxRealChildNode())
                {
                    logger.Info("Run job for Mailbox Child Node Level: {0}.", ((NodeLevel)_treeNode.Level).ToString());
                    //RunRelatedSubJobForMailbox(null);
                }
                else if (IsTeamsRealChildNode())
                {
                    logger.Info("Run job for Teams Child Node Level: {0}.", ((NodeLevel)_treeNode.Level).ToString());
                    await RunJobForTeamsAsync();
                    await UpdateArchiveInfo(_groupMailbox);
                }
                else
                {
                    logger.Info("Run job for other Node Level: {0}.", ((NodeLevel)_treeNode.Level).ToString());
                    _reporter.AddReportRecord(M365GroupTeam.ReportUtil.CreateReportDto(_groupMailbox, 0, null, StatisticsLevel.TeamsGroup, ActionTab.Scan), isHold: true);

                    var treeNodeDto = RMDtoConverter.ConvertRMTree2SPTree(_treeNode);
                    var sampleTreeNode = RMDtoConverter.ConvertSPTree2RMSampleTree(treeNodeDto);

                    
                    await RunSubJobForSPOAsync(null, sampleTreeNode, _treeNode.Parent);
                    JobExecutionProgressStatisticExecutor.Instance.ResetJobId(isTeams: true);

                    _reporter.ResetReportManager(_jobId, TeamsDisposalState.IsSiteHasMatchRule);
                    await UpdateArchiveInfo(_treeNode.GetTeamsNode().Name);
                }
            }
            catch (JobStopException stop)
            {
                logger.Error(stop.ToString());
                _reporter.StopJob();
                JobExecutionProgressStatisticExecutor.Instance.ResetJobId(isTeams: true);
                JobExecutionProgressStatisticExecutor.Instance.UpdateJobStatus(JobStatus.Stopped);
                throw;
            }
            catch (GraphAPIException e)
            {
                logger.Error("An graph api error occurred while initializing backup configuration. Reason: {0}.", e);
                string message = e.Message;
                if(e.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    message = "RM_SO_AppPermissionNotEnough";
                }
                _reporter.SetErrorMessage(message);
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
                JobExecutionProgressStatisticExecutor.Instance.FinishProgress(_reporter.GetJobStatus());
                JobExecutionProgressStatisticExecutor.Instance.Dispose();

                _reporter.EndDisposalStatistic(_mainJobId);
                _reporter.Finish();
            }
        }

        private async Task UpdateArchiveInfo(string groupMailbox)
        {
            // simulate job no need update archive info
            if (string.IsNullOrEmpty(groupMailbox) || _jobType == JobType.TeamsPreScan)
            {
                return;
            }

            logger.Info($"Start updating archiveInfo. groupMailbox: {groupMailbox}.");
            await _teamsSODashboardWorker.UpdateTeamsGroupArchivedInfo(groupMailbox);
        }

        private void InitJobContext()
        {
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(_jobId, true);
            var jobContextSettings = subJobWithContext.JobContext?.Settings;
            ThrowUtil.ThrowIfNullOrEmpty(jobContextSettings, "job context info empty.");

            this._treeNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContextSettings).FirstOrDefault();
            var teamsNode = _treeNode.GetTeamsNode();
            this._groupMailbox = teamsNode.Name;
            this._o365Id = subJobWithContext.O365TenantId;
            this._mainJobId = subJobWithContext.ParentId;


            (var groupSite, var relatedSites) = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsNode.TeamsId, true);
            _groupSiteUrl = groupSite.url;
            _expectedVirtualSubJobCount += relatedSites.Count; // add channel sites

            if (_jobType == JobType.TeamsRecordsDisposal)
            {
                WrapperConfiguration.IsProcessApprovalDatasOnly = _treeNode.IsProcessApprovalDatasOnly;
                // default value is true for recheck rule, if the setting exist, will override the default value
                // otherwise keep true to make sure the rule can be rechecked in whole process.
                WrapperConfiguration.IsRecheckRule = true;
                if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    var isRecheckRuleSetting = FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule).GetAwaiter().GetResult();
                    if (bool.TryParse(isRecheckRuleSetting, out bool isRecheckRule))
                    {
                        WrapperConfiguration.IsRecheckRule = isRecheckRule;
                    }
                    logger.Info($"current is recheck rule status is :{WrapperConfiguration.IsRecheckRule}");
                }
            }
        }

        private async Task RunJobForTeamsAsync()
        {
            _jobMessage = _jobService.GetBackupMessage(_jobId, _jobType, _treeNode);

            _options = new BackupOptions(_jobMessage);

            _options = ConfigMedia(_options);

            //AuthorizationManager.Instance.Init(_jobMessage.EmailBposInfoMap);

            IRuleActionScanner teamsScanner = new TeamsRuleActionScanner(_jobMessage, _options, _reporter);

            if (teamsScanner.HasRuleForMailboxSubNodes && IsSOMethod && _enableCGScan)
            {
                logger.Warn($"SO method and CG scan and exist teams level rule, will skip archive for mail box");
                return;
            }
            
            await teamsScanner.RunAsync();

            var ruleIDs = teamsScanner.GetAllRulesForTeams();
            Dictionary<string, Rule> rules = ruleIDs.ToDictionary(rid => rid, rid => teamsScanner.RuleManagement.BuildRule(rid));
            var teamsRuleId = ruleIDs.FirstOrDefault(rid => rules[rid]?.TeamsRule?.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Teams);

            if (string.IsNullOrEmpty(teamsRuleId))
            {
                if (!teamsScanner.HasRuleForSPOSubNodes)
                {
                    logger.Warn($"Teams/Group node is not match rule and has no lower level rule, skip process Teams node: {_groupMailbox}");
                    return;
                }

                if (IsSOMethod && _reporter.DecreaseTotalPhases(defaultTeamsPhaseCount))
                {
                    logger.Info($"HasRuleForSPOSubNodes but not Teams, Decreased total phases by [{defaultTeamsPhaseCount}] for Teams data and Mailbox subjob");
                }
            }
            /*if (_jobType == JobType.TeamsPreScan && !string.IsNullOrEmpty(teamsRuleId))
            {
                logger.Info($"Scan job finish with Teams/Group match rule. No need to process more");
                return;
            }*/
            ActionType teamsRuleAction = await ProcessTeamsActionAsync(ruleIDs, rules, teamsScanner);

            await RunRelatedSubJobsAsync(teamsScanner, teamsRuleId);

            if (_options.JobType == (int)JobType.TeamsPreScan || _options.JobType == (int)JobType.TeamsRecordsDisposal || string.IsNullOrEmpty(teamsRuleId) )
            {
                logger.Info($"Skip run disposal for teams group object");
                return;
            }

            if(teamsRuleAction == ActionType.ArchiverAndRemove)
            {
                await UpdateRelatedSPSitesToMasterIndex();
            }

            try
            {
                using var teamsScope = TryUnarchiveTeamsForDisposal();
                using (new CheckJobStopScope()) { }
                Office365GroupBackup.RService.TeamsDisposalService.Build(_reporter, rules[teamsRuleId]?.Name, teamsScope.IsTeamsUnarchivedForLockedChannelSite).RunDisposal();
                RealDeleteRelatedSites();
            }
            catch (JobStopException stop)
            {
                logger.Error(stop.ToString());
                _reporter.StopJob();
                throw;
            }
            catch (Exception e)
            {
                logger.Warn($"Failed to run the disposal job. error:{e}");
            }

        }

        private M365APIUtility TryUnarchiveTeamsForDisposal()
        {
            var m365APIUtility = new M365APIUtility();
            try
            {
                if (!TeamsDisposalState.HasChannelSiteReadOnly)
                {
                    logger.Info($"Teams group {_groupMailbox} has no channel site read-only, skip TryUnarchiveTeamsForDisposal.");
                    return m365APIUtility;
                }
                var m365APIService = Office365GroupBackup.RService.TeamsDisposalService?.M365APIService;
                if (m365APIService == null)
                {
                    logger.Info($"Teams disposal service M365APIService is null, skip TryUnarchiveTeamsForDisposal.");
                    return m365APIUtility;
                }

                if (m365APIService.TeamsService is MicrosoftTeamsWithGraph teamsService
                && m365APIService.TeamsServiceForDelegate is MicrosoftTeamsWithGraph teamsService4Delegate
                && m365APIService.GroupService is Microsoft365GroupServiceWithGraph groupService)
                {
                    m365APIUtility = new M365APIUtility(_groupMailbox, _groupSiteUrl, _treeNode.O365TenantId, _treeNode.TeamsId,
                        teamsService, teamsService4Delegate, groupService);
                    if (!m365APIUtility.TryUnarchiveTeamsForLockedChannelSite(false))
                    {
                        logger.Warn($"Failed to unarchive Teams.");
                        throw new Exception("^Failed to unarchive Teams for channel site. Please check the Teams state and try again. Contact support if the problem persists.");
                    }

                    return m365APIUtility;
                }

                logger.Warn($"Teams service is not MicrosoftTeamsWithGraph, cannot TryUnarchiveTeamsForDisposal. TeamsService type: {Office365GroupBackup.RService.TeamsDisposalService.M365APIService.TeamsService.GetType().FullName}, TeamsServiceForDelegate type: {Office365GroupBackup.RService.TeamsDisposalService.M365APIService.TeamsServiceForDelegate.GetType().FullName}, {Office365GroupBackup.RService.TeamsDisposalService.M365APIService.GroupService.GetType().FullName}, GroupService type: ");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to TryUnarchiveTeamsForDisposal. error:{e}");
                m365APIUtility.Dispose();
                throw;
            }

            return m365APIUtility;
        }

        private async Task<ActionType> ProcessTeamsActionAsync(List<string> ruleIDs, Dictionary<string, Rule> rules, IRuleActionScanner teamsScanner)
        {
            var action = ActionType.ArchiverAndRemove;
            var needArchiveTeamsData = false;
            foreach (var ruleid in ruleIDs)
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    var rule = rules[ruleid];

                    WrapperConfiguration.MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving ? true : (rule.MoveToAnotherTierType == (int)Storage.AccessTierType.Other || rule.MoveToAnotherTierType == null) ? false : true;
                    WrapperConfiguration.MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : (rule.MoveToAnotherTierType == null ? 0 : rule.MoveToAnotherTierType);
                    logger.Info($"Process rule id : {ruleid}, ruleName:{rule.Name}, MoveToArchiverTierWhenArchiving:{WrapperConfiguration.MoveToArchiverTierWhenArchiving}, MoveToAnotherTierType:{WrapperConfiguration.MoveToAnotherTierType}");

                    var data = teamsScanner.GetData(ruleid);

                    _options.CurrentRule = rule;

                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                            {
                                needArchiveTeamsData = true;
                                await new RuleActionWorker().Build(data, _treeNode).Build(_reporter).Build(_options).RunAsync();
                                break;
                            }
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            {
                                break;
                            }
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            {

                            }
                            break;
                    }
                }
                catch (JobStopException stop)
                {
                    logger.Error(stop.ToString());
                    _reporter.StopJob();
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to process teams rule [{ruleid}]. error:{ex}");
                }
            }

            if (needArchiveTeamsData)
            {
                await DaoService.CommonSiteMasterIndexDao.UpdateMergeIndexStateAsync(_jobId);
            }

            return action;
        }

        private void RealDeleteRelatedSites()
        {
            if (!TeamsDisposalState.IsGroupDeleted)
            {
                logger.Info($"Group site {_groupSiteUrl} is not deleted, skip delete related sites.");
                return;
            }

            logger.Info($"Start final disposal related sites for group site: {_groupSiteUrl}");

            var remoteSite = RemoteNodeDao.GetRemoteSiteCollectionByListUrl(_groupSiteUrl);
            var bposInfo = CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite).GetAwaiter().GetResult();
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(_groupSiteUrl, bposInfo, AveContextKind.ClientObjectModel);

            foreach (var siteUrl in TeamsDisposalState.GetAllDisposalSuccessfulChannelSites())
            {
                RealDeleteRelatedSite(aveObjectModelFactory, siteUrl);
            }

            if (TeamsDisposalState.IsGroupSiteDisposalSuccessful)
            {
                RealDeleteRelatedSite(aveObjectModelFactory, _groupSiteUrl, true);
            }
        }

        private void RealDeleteRelatedSite(AveObjectModelFactory aveObjectModelFactory, string siteUrl, bool isGroupSite = false)
        {
            logger.Info($"Start final disposal site: {siteUrl}");
            try
            {
                using (new CheckJobStopScope()) { }
                if (HasSpecialCharacters(siteUrl))
                {
                    _reporter.SetErrorMessage("StorageOptimization_SOARArchiverDeletionDeleteSiteHaveSpecialCharacter");
                    logger.Warn($"Cannot remove site with special characters.");
                    return;
                }


                string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, siteUrl);
                logger.Info("O365 Admin Url is : {0}.", mAdminUrl);
                IAveTenant aveTenant = null;
                AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
                {
                    aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                });
                var geoLocationInfo = aveTenant?.GetTenantGeoLocationinfo();
                if (geoLocationInfo != null && geoLocationInfo.Count > 1)
                {
                    foreach (var location in geoLocationInfo)
                    {
                        if (siteUrl.StartsWith(location.RootSiteUrl) || siteUrl.StartsWith(location.MySiteHostUrl))
                        {
                            mAdminUrl = location.TenantAdminUrl;
                            logger.Info($"GetTenantGeoLocationinfo.O365 Admin New Url is : {mAdminUrl}.SiteUrl:{siteUrl}.");
                            AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
                            {
                                aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                            });
                        }
                    }
                }

                using var scope = new SiteStateTransitionScope(siteUrl, aveObjectModelFactory, SiteState.Unlock);
                // group site can be locked normally so need to handle support locked site
                if (isGroupSite && _treeNode.SupportLockedSite)
                {
                    scope.TryConvertToTargetStatus();
                }
                // wait for about 10 minutes until the group site can be deleted
                int retryCount = 0;
                while (retryCount++ < 60)
                {
                    try
                    {
                        if (IsSiteExist(aveTenant, siteUrl))
                        {
                            logger.Info($"Try to remove the site {siteUrl}");
                            aveTenant.RemoveSite(siteUrl);
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred while deleting site, {ex}");
                        if (!isGroupSite)
                        {
                            break;
                        }
                    }

                    System.Threading.Thread.Sleep(10 * 1000);
                }

                try
                {
                    bool conflictWithRecycleBin = IsSiteExistInRecycleBin(aveTenant, siteUrl);
                    if (conflictWithRecycleBin)
                    {
                        logger.Info($"Site exists in recycle bin.");
                        aveTenant.RemoveDeletedSite(siteUrl);
                        logger.Info($"Remove site from RecycleBin success.");
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Remove from RecycleBin site {siteUrl} Error {e}");
                }

                ArchiverJobManagementService archiverJobManagementService = new ArchiverJobManagementService();
                new AveTaskRetryHelper(5, true).ExecuteWithRetryMechanism(() =>
                {
                    //add retry logic due to AOS API not stable.
                    archiverJobManagementService.UpdateSiteCollectionAfterAchiveredAsync(siteUrl, true, TenantLocalValue.LogonGroupId, "").Wait();

                    try
                    {
                        logger.Info("DeleteSiteInRecords.siteUrl:{0}.", siteUrl);
                        try
                        {
                            RemoteNodeDao.DeleteRemoteSiteCollectionsByUrl(new List<string>() { siteUrl });
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error in DeleteSiteInRecords, reason : {ex.ToString()}.");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while deleting site in records. Error:{0}", e.ToString());
                    }

                });
            }
            catch (JobStopException stop)
            {
                logger.Error(stop.ToString());
                _reporter.StopJob();
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while removing site, {ex}");
            }


            bool IsSiteExist(IAveTenant tenant, string siteUrl)
            {
                var exist = false;
                try
                {
                    var properties = tenant.GetSitePropertiesByUrl(siteUrl);
                    exist = true;
                }
                catch (Exception e)
                {
                    logger.Info("Check site exist failed {0}.Error:{1}", siteUrl, e);
                }
                return exist;
            }

            bool IsSiteExistInRecycleBin(IAveTenant tenant, string siteUrl)
            {
                var exist = false;
                try
                {
                    var properties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                    exist = true;
                }
                catch (Exception e)
                {
                    logger.Info("Check site in recycle bin failed {0}.Error:{1}", siteUrl, e);
                }
                return exist;
            }

            bool HasSpecialCharacters(string siteUrl)
            {
                char[] specialCharacter = { '&', '^' };
                int siteUrlContainsSpecial = siteUrl.IndexOfAny(specialCharacter);
                return siteUrlContainsSpecial >= 0;
            }
        }

        private async Task UpdateRelatedSPSitesToMasterIndex()
        {
            try
            {
                logger.Info("Start update related SP sites into master index.");
                using (new CheckJobStopScope()) { }
                var allSites = new List<string>();
                var channelSites = TeamsDisposalState.GetAllArchivedChannelSites();
                allSites.AddRange(channelSites);
                (var extensionStr, var indexId) = await DaoService.CommonSiteMasterIndexDao.GetExtensionAsync(_jobId);
                if (string.IsNullOrEmpty(extensionStr) || string.IsNullOrEmpty(indexId))
                {
                    logger.Warn($"Could not found master index extension with JobId: [{_jobId}].");
                    return;
                }

                var extObj = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(extensionStr);
                extObj.SPGroupSiteURL = _groupSiteUrl;
                extObj.ChannelSiteRelativeURLs = allSites.Select(s => new Uri(s).LocalPath).ToList();
                extObj.IsMicrosoftTeam = _options.IsMicrosoftTeam;
                extObj.IsChannelSiteReadOnly = TeamsDisposalState.IsTeamsArchived && TeamsDisposalState.HasChannelSiteReadOnly;
                extensionStr = SerializerHelper.SerializeByDataContractSerializer(extObj);
                await DaoService.CommonSiteMasterIndexDao.UpdateExtensionAsync(indexId, extensionStr);
                logger.Info($"Succeed update related SP sites into master index. isTeams: {extObj.IsMicrosoftTeam}, isArchivedTeams: {TeamsDisposalState.IsTeamsArchived}, IsChannelSiteReadOnly: {extObj.IsChannelSiteReadOnly}");
            }
            catch (JobStopException stop)
            {
                logger.Error(stop.ToString());
                _reporter.StopJob();
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Update related SPSites to MasterIndex failed. {ex}");
                throw;
            }
        }

        private bool IsTeamsRealChildNode()
        {
            var teamsChildNodeLevels = new HashSet<int>()
            {
                (int)NodeLevel.Office365GroupEntire,    // channel, planer... in the future
            };
            return teamsChildNodeLevels.Contains(_treeNode.Level);
        }

        private bool IsMailboxRealChildNode()
        {
            if (_jobType == JobType.TeamsRecordsDisposal)
            {
                return false;
            }
            var mailboxRelatedNodeLevels = new HashSet<int>()
            {
                (int)NodeLevel.ExchangeOnlineMailbox,    // mailbox folder, mailbox item... in the future
            };
            return mailboxRelatedNodeLevels.Contains(_treeNode.Level);
        }

        private string CreateSubJob(int currentSubjobIndex, JobType jobType, object jobSettings, string scope)
        {
            try
            {
                using (new CheckJobStopScope()) { }
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
                    O365TenantId = _o365Id,
                };
                subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings) };
                subJob.String1 = scope;
                SubJobDao.CreateJob(subJob);
                logger.Info($"Create sub job {subJob.Id} sucessfull, type {subJob.JobType}, Scope {scope}");
                return subJobId;
            }
            catch (JobStopException stop)
            {
                logger.Error(stop.ToString());
                _reporter.StopJob();
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while creating a sub job on {scope}. Reason: {ex}");
                throw;
            }
        }

        private async Task RunRelatedSubJobsAsync(IRuleActionScanner scanner, string teamsRuleId)
        {
            try
            {
                var willArchiveMailbox = scanner.HasRuleForMailboxSubNodes
                && _options.JobType != (int)JobType.TeamsPreScan;
                var delegateAppUserAdded = PrepareDelegateAppUserForMailbox(willArchiveMailbox, out var graphMgr, out var delegateAppUserId);

                if (scanner.HasRuleForSPOSubNodes || !string.IsNullOrEmpty(teamsRuleId))
                {
                    logger.Info($"start run RunRelatedSubJobForSPOAsync. teamsRuleId: {teamsRuleId}");
                    await RunRelatedSubJobForSPOAsync(teamsRuleId);
                }

                if (willArchiveMailbox)
                {
                    if (delegateAppUserAdded)
                    {
                        graphMgr.WaitDelegateAppUserCanAccessConversationData(delegateAppUserId);
                    }

                    RunRelatedSubJobForMailbox(teamsRuleId);

                    if (delegateAppUserAdded)
                    {
                        RemoveDelegateAppUserFromPrivateTeam(graphMgr, delegateAppUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _reporter.HasErrorNode = true;
                logger.Error($"Error occurred while running sub jobs. {ex}");
            }
            
        }

        private bool PrepareDelegateAppUserForMailbox(bool willArchiveMailbox, out Common.GraphApi.GroupMailAndCalendar.GraphConversationAndCalendarManager graphMgr, out string delegateAppUserId)
        {
            graphMgr = null;
            delegateAppUserId = null;
            if (!willArchiveMailbox)
            {
                return false;
            }
            graphMgr = new Common.GraphApi.GroupMailAndCalendar.GraphConversationAndCalendarManager(_o365Id, _groupMailbox, _treeNode.Id);
            return graphMgr.EnsureDelegateAppUserAsMemberForPrivateTeam(false, out delegateAppUserId);
        }

        private void RemoveDelegateAppUserFromPrivateTeam(Common.GraphApi.GroupMailAndCalendar.GraphConversationAndCalendarManager graphMgr, string delegateAppUserId)
        {
            if (graphMgr == null || string.IsNullOrWhiteSpace(delegateAppUserId))
            {
                return;
            }

            try
            {
                graphMgr.RemoveDelegateAppUserFromTeamMembers(delegateAppUserId);
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while remove delegate app user from private Team members, mailbox : {_groupMailbox}, reason : {ex}.");
            }
        }

        private async Task RunRelatedSubJobForSPOAsync(string teamsRuleId)
        {
            var hasTeamsLevelRule = !string.IsNullOrEmpty(teamsRuleId);
            var availableNodes = await BuildAvailableNodesToRunSPJob(hasTeamsLevelRule);

            List<RMSPSampleTreeNode> canRunSite = new List<RMSPSampleTreeNode>();
            foreach (var siteNode in availableNodes) 
            {
                if (hasTeamsLevelRule && _jobType != JobType.TeamsPreScan)
                {
                    var isGroupSite = siteNode.FullPath.Equals(_groupSiteUrl, StringComparison.OrdinalIgnoreCase);
                    if (isGroupSite)
                    {
                        siteNode.ChannelType = (int)TeamsChannelType.None;
                        if (!TeamsDisposalState.AllowGroupSiteDisposal)
                        {
                            logger.Warn($"The group site {siteNode.FullPath} is not allowed to run disposal job.");
                            DecreasePhase(siteNode.FullPath);
                            continue;
                        }
                    }
                    else
                    {
                        if (!TeamsDisposalState.IsAllowDisposalChannelSite(siteNode.FullPath, out var channelType))
                        {
                            logger.Warn($"The channel site {siteNode.FullPath} is not allowed to run disposal job. channelType: {channelType}");
                            DecreasePhase(siteNode.FullPath);
                            continue;
                        }
                        siteNode.ChannelType = (int)channelType;
                    }
                }
                canRunSite.Add(siteNode);
            }

            if(canRunSite.IsNullOrEmpty())
            {
                logger.Info($"No site collection can run sub job for SPO.");
                return;
            }

            foreach (var siteNode in canRunSite)
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    logger.Info($"start RunSubJobForSPOAsync for site {siteNode.FullPath}, teamsRuleId: {teamsRuleId}, channelType: {(TeamsChannelType)siteNode.ChannelType}");

                    await RunSubJobForSPOAsync(teamsRuleId, siteNode, _treeNode);
                }
                catch (JobStopException stop)
                {
                    logger.Error(stop.ToString());
                    _reporter.StopJob();
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while running sub job on {siteNode.Name}. Reason: {e}");
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.ResetJobId();
                }
            }
            JobExecutionProgressStatisticExecutor.Instance.ResetJobId(isTeams: true);

            _reporter.ResetReportManager(_jobId, TeamsDisposalState.IsSiteHasMatchRule);
        }

        private void DecreasePhase(string sitePath = null)
        {
            if (_reporter.DecreaseTotalPhases(defaultTeamsPhaseCount))
            {
                logger.Info($"Decreased total phases for {(sitePath == null ? "mailbox" : $"site {sitePath}")} due to not create subjob.");
            }
        }

        private async Task<List<RMSPSampleTreeNode>> BuildAvailableNodesToRunSPJob(bool hasTeamsLevelRule)
        {
            var treeNodeDto = RMDtoConverter.ConvertRMTree2SPTree(_treeNode);
            var browseTreeNode = RMDtoConverter.ConvertSPTree2RMSampleTree(treeNodeDto);
            var availableNodes = new List<RMSPSampleTreeNode>();
            logger.Info($"Start BuildAvailableNodesToRunSPJob . HasTeamsLevelRule: {hasTeamsLevelRule}, UserArchiverImportFile: {_jobMessage.Config.UserArchiverImportFile} ");

            if (browseTreeNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                List<RMSPTreeNode> sites = await TeamsTreeService.BrowseDirectSitesByTeamNode(treeNodeDto);
                logger.Info($"process node is teams group level node. Load all site under teams {browseTreeNode.FullPath}, count: {sites.Count}.");
                if (sites.IsNullOrEmpty())
                {
                    return availableNodes;
                }

                #region assemble site nodes for record disposal job 
                if (_jobType == JobType.TeamsRecordsDisposal)
                {
                    List<string> mBreakTreeNode = new List<string>();
                    var parentId = ScheduleService.GetProfileId(_treeNode) + "|";

                    var treeNodes = ScheduleDao.GetDisposalBreakNodes(parentId);
                    foreach (var item in treeNodes)
                    {

                        var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                        if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                        {
                            continue;
                        }
                        mBreakTreeNode.Add(node.FullPath);
                    }
                    await TeamsSettingsService.LoadSiteSettingsUnderTeamsNodeAsync(sites, _treeNode);
                    //await LoadTeamsSettingUnderGroupAsync(sites, _treeNode);
                    //this.LoadSPSetting(sites);
                    foreach (var site in sites)
                    {
                        var hasSchedule = mBreakTreeNode.Contains(site.FullPath);
                        if ((site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable || !WrapperConfiguration.IsRecheckRule) && !hasSchedule)
                        {
                            availableNodes.Add(RMDtoConverter.ConvertSPTree2RMSampleTree(RMDtoConverter.ConvertRMTree2SPTree(site)));
                        }
                        else
                        {
                            logger.Warn($"This site collection has unique setting, skip it. Url: {site.FullPath}, " +
                                $"EnableRecordManagement: {site.EnableRecordManagement}, hasSchedule: {hasSchedule}");
                        }
                    }
                }
                #endregion

                #region assemble site nodes for SO job
                else if (_jobType == JobType.TeamsArchiverBackup || _jobType == JobType.SpecifyTeamsArchiverBackup)
                {
                    var breakInheritObjIDs = new Dictionary<Guid, int>();
                    ArchiverSettingDao.LoadArchiverSettingsUnderTeams(new Guid(browseTreeNode.TeamsId), true)
                        .ForEach(s => breakInheritObjIDs.TryAdd(s.SPObjectId, s.EnableArchiverManagement));

                    foreach (var site in sites)
                    {
                        var hasUniqueSetting = breakInheritObjIDs.TryGetValue(new Guid(site.SPObjectId), out int isEnableArchiveManager);

                        if (!hasUniqueSetting
                            || (hasTeamsLevelRule && isEnableArchiveManager == (int)EnableRecordManagementSetting.Enable) 
                            || _jobMessage.Config.UserArchiverImportFile)
                        {
                            logger.Info($"This site collection will be processed, hasUniqueSetting: {hasUniqueSetting}, isEnableArchiveManager: {isEnableArchiveManager}, Url: {site.FullPath}");
                            availableNodes.Add(RMDtoConverter.ConvertSPTree2RMSampleTree(RMDtoConverter.ConvertRMTree2SPTree(site)));
                        }
                        else
                        {
                            logger.Warn($"This site collection has unique setting, skip it. Url: {site.FullPath}");
                        }
                    }
                }
                #endregion

                #region assemble site nodes for pre scan job
                else if (_jobType == JobType.TeamsPreScan)
                {
                    var breakInheritObjIDs = ArchiverSettingDao.LoadArchiverSettingsUnderTeams(new Guid(browseTreeNode.TeamsId), true).Select(s => s.SPObjectId);
                    foreach (var site in sites)
                    {
                        if (!breakInheritObjIDs.Contains(new Guid(site.SPObjectId)))
                        {
                            logger.Info($"This site collection will be scanned, Url: {site.FullPath}");
                            availableNodes.Add(RMDtoConverter.ConvertSPTree2RMSampleTree(RMDtoConverter.ConvertRMTree2SPTree(site)));
                        }
                        else
                        {
                            logger.Warn($"This site collection has unique setting, skip it. Url: {site.FullPath}");
                        }
                    }
                }
                #endregion

                else
                {
                    logger.Warn($"Job type {_jobType} is not supported to run SP job.");
                }
            }
            else
            {
                availableNodes.Add(browseTreeNode);
                logger.Info($"process node has level {((NodeLevel)browseTreeNode.Level).ToString()}.");
            }

            return availableNodes;
        }

        private async Task RunSubJobForSPOAsync(string teamsRuleId, RMSPSampleTreeNode treeNode, RMSPTreeNode parentNode)
        {
            RMSPTreeNode siteTreeNode = _jobType switch
            {
                JobType.TeamsArchiverBackup or JobType.TeamsPreScan or JobType.SpecifyTeamsArchiverBackup => ArchiverSettingsService.LoadSampleNodeSettings(treeNode, ScheduleType.TeamsArchiveJobSchedule),
                JobType.TeamsRecordsDisposal => await TeamsSettingsService.LoadSampleNodeSettingsAsync(treeNode),
                _ => throw new NotSupportedException($"Job type {_jobType} is not supported for Teams sub job.")
            };

            siteTreeNode.Parent = parentNode;
            siteTreeNode.SkipRemoveContentAndDestroyAction = _treeNode.SkipRemoveContentAndDestroyAction;
            siteTreeNode.IsEnableSuperUserDecrypt = _treeNode.IsEnableSuperUserDecrypt;
            siteTreeNode.IsManagedMetadataService = _treeNode.IsManagedMetadataService;
            siteTreeNode.UserArchiverImportFile = _treeNode.UserArchiverImportFile;
            //siteTreeNode.IsEnableRemoveRetentionLabel = _treeNode.IsEnableRemoveRetentionLabel;
            siteTreeNode.SupportLockedSite = _treeNode.SupportLockedSite;
            siteTreeNode.SupportArchivedTeams = _treeNode.SupportArchivedTeams;
            siteTreeNode.IsProcessApprovalDatasOnly = siteTreeNode.IsProcessApprovalDatasOnly? siteTreeNode.IsProcessApprovalDatasOnly:parentNode.IsProcessApprovalDatasOnly;
            siteTreeNode.NodeType = (TeamsChannelType)treeNode.ChannelType switch
            {
                TeamsChannelType.Private => (int)NodeType.TeamPrivateChannel,
                TeamsChannelType.Shared => (int)NodeType.TeamSharedChannel,
                _ => (int)NodeType.TeamChannel
            };

            List<RMSPSampleTreeNode> notConflictSiteCollections = GetRunableSiteCollection(_groupMailbox, [treeNode]);
            if ((notConflictSiteCollections != null && notConflictSiteCollections.Any(sc => sc.FullPath.Equals(treeNode.FullPath)))
                || (siteTreeNode.Rules != null && CheckTheJobHasJustDeleteOnlyRule(siteTreeNode.Rules)))
            {
                var subJobId = CreateSubJob(
                    _relatedSubJobIndex++,
                    _jobType,
                    new List<RMSPTreeNode>() { siteTreeNode },
                    treeNode.FullPath);
                JobContext.GetInstance(subJobId, _jobType);

                if (_reporter.AdvanceToNextPhase())
                {
                    logger.Info($"Advance to next phase for subjob of site {treeNode.FullPath}, virtual subjob: {subJobId}");
                }
                
                await new DisposalActivityManagementProcessor(subJobId, _jobType).RunNowAsync(teamsRuleId);

                if(await DaoService.ArchiverSiteMasterIndexDao.ExistsArchivedDataAsync(treeNode.FullPath))
                {
                    TeamsDisposalState.AddArchivedChannelSite(treeNode.FullPath);
                }
            }
            else
            {
                logger.Info($"skip process sc:{treeNode.FullPath}");
                DecreasePhase(treeNode.FullPath);
            }
        }

        private bool CheckTheJobHasJustDeleteOnlyRule(List<RMSimpleRule> scopeApplyRules)
        {
            bool onlyHasVersionDeletionRule = true;
            foreach (RMSimpleRule simpleRule in scopeApplyRules)
            {
                try
                {
                    RMMiscProfile rMMiscProfile = MiscProfileDao.Load(simpleRule.RuleId.ToString());
                    Rule rule = AvePoint.GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<Rule>(rMMiscProfile.Extension);

                    if (rule.TeamsRule != null)
                    {
                        if (rule.TeamsRule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.TeamsRule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                        {
                            logger.Info($"teams teams rule Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                        }
                        else
                        {
                            logger.Info($"teams teams rule Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                            onlyHasVersionDeletionRule = false;
                            break;
                        }
                    }
                    else
                    {
                        if (rule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                        {
                            logger.Info($"teams level Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                        }
                        else
                        {
                            logger.Info($"teams level Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                            onlyHasVersionDeletionRule = false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    onlyHasVersionDeletionRule = false;
                    logger.Warn($"teams Error check only has version deletion rule for retention job in progress.Message:{ex}.");
                    break;
                }

            }
            return onlyHasVersionDeletionRule;
        }

        private List<RMSPSampleTreeNode> GetRunableSiteCollection(string teamName ,List<RMSPSampleTreeNode> allSites)
        {
            if(_jobType == JobType.TeamsPreScan)
            {
                return allSites;
            }
            if (allSites.IsNullOrEmpty())
            {
                return new List<RMSPSampleTreeNode>();
            }
            Dictionary<string, List<string>> teamsFilter = new Dictionary<string, List<string>>();
            teamsFilter.Add(teamName, allSites.Select(site => site.FullPath).ToList());
            Dictionary<string, List<string>> runningUrls = RMJobService.GetRunningTeamsArchiverJobSiteUrl(JobTypeConstants.ArchiveTeamsConflictType, true, teamsFilter, this._mainJobId);
            return RuleSPTreeUtil.FilterTeamsAvailableNodeByRunningUrl(allSites, runningUrls);
        }

        private void RunRelatedSubJobForMailbox(string teamsRuleId)
        {
            if (!TeamsDisposalState.AllowGroupSiteDisposal)
            {
                logger.Warn($"The mailbox {_groupMailbox} is not allowed to run disposal job.");
                DecreasePhase();
                return;
            }

            if(IsSOMethod && _enableCGScan)
            {
                logger.Warn($"SO method and CG scan, will skip run job for mail box");
                DecreasePhase();
                return;
            }

            var nodeDto = RMDtoConverter.ConvertRMSPTree2EXOTreeNodeDto(_treeNode);
            var exoNode = RMDtoConverter.ConvertTreeNodeDto2RMExchangeTree(nodeDto);
            exoNode.SiteCollectionUrl = _groupSiteUrl;
            var subJobId = CreateSubJob(
                _relatedSubJobIndex++,
                JobType.MailBoxBackup,
                new List<RMEXOTreeNode>() { exoNode },
                _groupMailbox);

            if (_reporter.AdvanceToNextPhase())
            {
                logger.Info($"Advance to next phase for subjob of mailbox, virtual subjob: {subJobId}");
            }

            EXOBackupProcessor enforceRuleActionProcessor = new EXOBackupProcessor();
            enforceRuleActionProcessor.RunNow(subJobId, teamsRuleId);
            _reporter.UpdateStatistics(enforceRuleActionProcessor.ScanActionStatistics,ActionTab.Scan);
            _reporter.UpdateStatistics(enforceRuleActionProcessor.BackupActionStatistics, ActionTab.Backup);
            _reporter.UpdateStatistics(enforceRuleActionProcessor.OtherActionStatistics, ActionTab.Action);
            _reporter.ResetReportManager(_jobId);

            if (!TeamsDisposalState.IsExchangeDisposalSuccessful)
            {
                logger.Info($"The mailbox {_groupMailbox} disposal failed, will mark the job has error.");
                _reporter.HasErrorNode = true;
                //return;
            }
            // execute follow statement after mailbox disposal successful
        }

        private BackupOptions ConfigMedia(BackupOptions options)
        {
            if (_options.JobType == (int)JobType.TeamsPreScan)
            {
                logger.Warn($"Teams pre scan job does not need to open archive cache");
                return options;
            }
            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
            AvePoint.Media.Service.DomainModel.MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
            AvePoint.Media.Service.DomainModel.MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");

            options.cacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            options.cacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            options.cacheSetting.Extension.Path.Add(new PathMap()
            {
                DiskInfo = new DiskInfoDto()
                {
                    Path = BackgroundSettings.GetInstance().ArchiveCache,
                    Type = DeviceType.LocalPath,
                    Password = string.Empty,
                    UserName = string.Empty,
                    Usage = null
                }
            });

            options.cacheService = PlatformWindsorManager.GetService<ICacheService>() as CacheService;
            options.cacheService.Open(options.cacheSetting, true);



            var indexDeviceDto = PlatformWindsorManager.GetService<IStorageDeviceService>().GetIndexDevice();
            options.IndexLogicalDeviceDto = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);

            return options;
        }

    }
}