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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AI;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Report;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl;
using AvePoint.RA.VectorDataCenter.Models;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Newtonsoft.Json;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAGoogle.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.OneDriveExplorerSync
{
    public class RMOneDriveExplorerProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMOneDriveExplorerProcessor));

        #region Castle Properties
        private IRMNodeFlagDao _rMNodeFlagDao;
        public IRMNodeFlagDao RMNodeFlagDao
        {
            get { return _rMNodeFlagDao ?? (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao)); }
            set { _rMNodeFlagDao = value; }
        }
        private IOneDriveSettingDao mOneDriveSettingDao;
        protected IOneDriveSettingDao OneDriveSettingDao
        {
            get
            {
                if (mOneDriveSettingDao == null)
                {
                    mOneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
                }
                return mOneDriveSettingDao;
            }
        }
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        public IRMKeyValueDao RMKeyValueDao { set; get; } = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private IRMMLTrainingModelDao RMMLTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        #endregion

        private JobContext jobContext = null;
        private Dictionary<Guid, string> columnInternalNameMap = null;
        private RMOneDriveRetentionDataCache RetentionCache = RMOneDriveRetentionDataCache.Instance;
        private RMOneDriveExplorerDataCache ExplorerCache = RMOneDriveExplorerDataCache.Instance;
        private Dictionary<Guid, RMOneDriveSetting> mGruopSettingMap = new Dictionary<Guid, RMOneDriveSetting>();
        private bool isCosmosBulkOperationEnabled = true; //是否开启了批量插入数据到cosmos db
        private bool isNeedAddOrUpdateVector = true;

        public RMOneDriveExplorerProcessor(string jobId)
        {
            columnInternalNameMap = new Dictionary<Guid, string>();
            jobContext = JobContext.GetInstance(jobId, Contract.JobMonitor.JobType.OneDriveDataSynchronisation);
            jobContext.ReportManager.StartUpdateJobProgress();
            //var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                JobType = Contract.JobMonitor.JobType.OneDriveDataSynchronisation,
                MainJobId = jobContext.MainJobId,
                SubJobId = jobContext.SubJobId
            });
        }


        public async System.Threading.Tasks.Task RunNowAsync()
        {
            using (var performance = new PerformanceScope("RMOneDriveExplorerProcessor.RunNow"))
            {
                CompoundDisposalStatistics.Instance.StartStatistic();
                //以site collection分的sub job，所以获取到的都是site collecttion节点
                List<SPTreeNodeDto> siteNodes = new List<SPTreeNodeDto>();

                SPTreeNodeDto groupNode = null;
                IAveSite aveSite = null;
                try
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        WrapperConfiguration.CheckFileContentDismatch = false;
                        logger.Info($"EnableCheckFileContentDismatch is {WrapperConfiguration.CheckFileContentDismatch}");
                        ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");
                        List<RMSPTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting);
                        if (!string.IsNullOrWhiteSpace(jobContext.JobContextContent))
                        {
                            mGruopSettingMap = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMOneDriveSetting>>(jobContext.JobContextContent);
                            logger.Info("Group level setting count:{0} ids:{1}", mGruopSettingMap.Count, string.Join(",", mGruopSettingMap.Keys));
                        }
                        siteNodes = tempList.ConvertAll(node => RMDtoConverter.ConvertRMTree2SPTree(node));
                        using var scopeTokenUsage = TokenUsageCache.Begin();
                        logger.Info($"start to count Token usage");
                        foreach (var site in siteNodes)
                        {
                            bool useGroupSetting = false;
                            string siteId = string.Empty;
                            try
                            {
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    logger.Info($"Scan node:{site.FullPath}, Id:{site.SPObjectId}");
                                    groupNode = SPTreeNodeManagement.GetGroupNode(site);
                                    ThrowUtil.ThrowIfNull(groupNode, "group node info empty.");
                                    var groupId = Guid.Parse(GetGroupNode(site).SPObjectId);
                                    Guid mSiteId = Guid.Empty;
                                    if (site.Level != NodeLevel.WebApplication)
                                    {
                                        mSiteId = new Guid(GetSiteCollectionNode(site).SPObjectId);
                                    }
                                    RMOneDriveSetting setting = OneDriveSettingDao.GetSettingInfoByScope(groupId, mSiteId, new Guid(site.SPObjectId));
                                    if (setting == null)
                                    {
                                        useGroupSetting = true;
                                        logger.Info("Site level setting is null, will use group level setting.");
                                        if (mGruopSettingMap.ContainsKey(groupId))
                                        {
                                            setting = mGruopSettingMap[groupId];
                                        }
                                        else
                                        {
                                            setting = OneDriveSettingDao.LoadOneDriveSetting(groupId, Guid.Empty);
                                        }
                                    }
                                    await HandleAddOrUpdateVectorTerm(setting);
                                    long lastScanTime = GetLastScanTimeFromDB(groupNode.SPObjectId, site.SPObjectId);
                                    RetentionCache.CacheTermChange(lastScanTime);
                                    AveObjectModelFactory discoverObjectModelFactory = null;
                                    logger.Info($"Main job start time is {jobContext.MainJobStartTime}, sub job start time is:{jobContext.JobStartTime.Ticks}");
                                    (var discoverSite, aveSite, discoverObjectModelFactory) = await GetDiscoverSiteAsync(site, jobContext.JobStartTime.Ticks, lastScanTime);
                                    using (discoverSite)
                                    {
                                        siteId = discoverSite.SiteID.ToString();
                                        using (aveSite)
                                        {
                                            Guid bcsColumnID = Guid.Empty;
                                            // var internalName = GetBCSColumnInternalName(site, aveSite, ref bcsColumnID);
                                            //if (string.IsNullOrEmpty(internalName))
                                            //{
                                            //    logger.Warn($"site doesn't have bcs column:{site.FullPath}");
                                            //    continue;
                                            //}
                                            RetentionCache.CacheSPLabelInfo(aveSite);
                                            RMOneDriveExplorerDataCache.Instance.InitSiteLevelCache(siteId, new RMSPExplorerSiteLevelCache
                                            {
                                                AveSiteId = site.ID,
                                                BCSColumnInternalName = "",
                                                BCSColumnID = bcsColumnID,
                                                HasErrorNode = false,
                                                SPSiteId = aveSite.ID
                                            });

                                            RMOneDriveExplorerBase jobWorker = null;
                                            var allSettingsUnderSite = OneDriveSettingDao.LoadOneDriveSettingsUnderSite(mSiteId);
                                            if (useGroupSetting)
                                            {
                                                allSettingsUnderSite.Add(setting);
                                            }

                                            List<RMOneDriveSetting> allWebSettings = allSettingsUnderSite.Where(s => s.SiteId != Guid.Empty && s.WebId != Guid.Empty && s.ListId == Guid.Empty).ToList();

                                            if (NeedRunFullJob(allSettingsUnderSite) || lastScanTime == DateTime.MinValue.Ticks)
                                            {
                                                bool forceUpdate = RMKeyValueDao.GetForceUpdate();
                                                logger.Info($"site:{site.FullPath}, full scan. Is force update: [{forceUpdate}]");
                                                jobWorker = BuildJobWorker(SPDiscoverType.Full, discoverSite, site, jobContext, setting, mSiteId, aveSite, discoverObjectModelFactory, lastScanTime, jobContext.JobStartTime.Ticks, allWebSettings, isCosmosBulkOperationEnabled, null, forceUpdate);
                                                await jobWorker.RunNowAsync();
                                            }
                                            else if (NeedRunSearchDiscover(lastScanTime))
                                            {
                                                logger.Info($"site:{site.FullPath}, search scan. Last job time:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()}");
                                                jobWorker = BuildJobWorker(SPDiscoverType.CAMLSearch, discoverSite, site, jobContext, setting, mSiteId, aveSite, discoverObjectModelFactory, lastScanTime, jobContext.MainJobStartTime, allWebSettings, isCosmosBulkOperationEnabled);
                                                await jobWorker.RunNowAsync();
                                            }
                                            else
                                            {
                                                logger.Info($"site:{site.FullPath}, sp inc date from:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()} Ticks:{lastScanTime} to {new DateTime(jobContext.JobStartTime.Ticks, DateTimeKind.Utc).ToString()} Ticks:{jobContext.JobStartTime.Ticks}");
                                                jobWorker = BuildJobWorker(SPDiscoverType.Incremental, discoverSite, site, jobContext, setting, mSiteId, aveSite, discoverObjectModelFactory, lastScanTime, jobContext.JobStartTime.Ticks, allWebSettings, isCosmosBulkOperationEnabled);
                                                await jobWorker.RunNowAsync();
                                            }
                                            // process records by term rule changed.
                                            if (lastScanTime != DateTime.MinValue.Ticks)
                                            {
                                                jobWorker.ProcessTermChangedItems(lastScanTime);
                                            }
                                        }

                                        if (!ExplorerCache.SiteLevelCache[siteId].HasErrorNode)
                                        {
                                            logger.Info($"update the site node flag info.");
                                            RMNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag()
                                            {
                                                NodeId = new Guid(site.SPObjectId),
                                                Title = site.Name,
                                                FullPath = site.FullPath,
                                                CollectionTime = jobContext.JobStartTime.Ticks,
                                                GroupId = new Guid(groupNode.SPObjectId),
                                                IsRemoved = false,
                                                NodeFlagType = (int)NodeFlagType.OneDriveExplorerSync
                                            });
                                        }
                                        if (!useGroupSetting)
                                        {
                                            await OneDriveSettingDao.SetSettingJobTimeAsync(setting.ScopeId, setting.SiteId);
                                        }
                                    }
                                }
                            }
                            catch (JobStopException)
                            {
                                jobContext.JobHasStopped = true;
                                throw new JobStopException("the job has stopped.");
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Process Site error, Path:{site?.FullPath}, ERROR:{ex.ToString()}");
                                jobContext.HasErrorNode = true;
                                jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                                {
                                    ObjectName = site?.Name,
                                    FullPath = site?.FullPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = ex.Message,
                                });
                            }
                            finally
                            {
                                if (isCosmosBulkOperationEnabled)
                                {
                                    CosmosBulkOperator.Instance.Reset();
                                }
                                if (ExplorerCache.SiteLevelCache.ContainsKey(siteId))
                                {
                                    ExplorerCache.SiteLevelCache[siteId].Dispose();
                                }

                            }
                        }
                        var total = scopeTokenUsage.End();
                        logger.Info($"Grand total token usage: {total}");
                    }
                }
                catch (JobStopException)
                {
                    jobContext.JobHasStopped = true;
                    logger.Warn("the job has stopped.");
                    throw new JobStopException("the job has stopped.");
                }
                catch (Exception e)
                {
                    jobContext.HasErrorNode = true;
                    logger.Error($"error occurred while Process Sync Job, ERROR:{e.ToString()}");
                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = string.Empty,
                        FullPath = string.Empty,
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                    });
                }
                finally
                {
                    CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                    RMMLManualApprovalEmailSender.Commit(jobContext.MainJobId);
                    UpdateMovedData();
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                    if (jobContext.HasErrorNode)
                    {
                        jobContext.Finish("RM_SS_CommonErrorMessage");
                    }
                    else
                    {
                        jobContext.Finish();
                    }
                    RetentionCache.Dispose();
                }
            }
        }

        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }

        private void UpdateMovedData()
        {
            try
            {
                if (RMOneDriveExplorerBoardCache.Instance.MovedDataCache != null && RMOneDriveExplorerBoardCache.Instance.MovedDataCache.Count > 0)
                {
                    ExplorerDao.UpdateAll(r => RMOneDriveExplorerBoardCache.Instance.MovedDataCache.Keys.Contains(r.Id), r => { r.MetaInfo = RemoveDataStatus(r.MetaInfo); });
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while updating moved data. Error:{0}", e.ToString());
            }
        }

        private string RemoveDataStatus(string metaInfo)
        {
            var meta = JsonConvert.DeserializeObject<RecordMetaInfo>(metaInfo);
            meta.DataStatus = (int)DataStatus.None;
            return JsonConvert.SerializeObject(meta);
        }

        private async Task HandleAddOrUpdateVectorTerm(RMOneDriveSetting setting)
        {
            try 
            {
                if (isNeedAddOrUpdateVector && setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                    {
                        if (RMKeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot)
                        {
                            logger.Info("Handle Add or Update vector for the term");
                            var mlTerms = RMMLTermDao.GetAllMLTerm();
                            foreach (var term in mlTerms)
                            {
                                try
                                {
                                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                                    var queryService = await QueryService.CreateWithRAIProvider(vectorStore, new CosineSimilarityCalculator());
                                    var metaData = await queryService.QueryMetaDataByTermId(term.Id);
                                    if (!string.IsNullOrEmpty(term.Description) && !metaData.EqualsIgnoreCase(term.Description))
                                    {
                                        logger.Info($"The term {term.Id} do not have vector or do have description change");
                                        var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                                        await vectorizationService.StoreTermAsync(new TermDescription
                                        {
                                            Id = term.Id,
                                            Name = term.Name,
                                            Description = term.Description
                                        });
                                    }
                                    else
                                    {
                                        logger.Info($"Skip update or create vector for term {term?.Id}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"Add or update the vector for term {term?.Id} has errors: {ex}");
                                }
                            }
                            isNeedAddOrUpdateVector = false;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error($"Handle add or update vector term has error {e}");
            }
        }

        /// <summary>
        /// 判断主job是否需要按照Full进行discover，以下情况需要按照Full进行discover，确保可以查询出所有web/list
        ///1.site collection下有节点使用的auto classification，并且勾选了了select all或者criteria中使用了older than条件
        ///2.site collection下有节点使用的use default，并且勾选了select all
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private bool NeedRunFullJob(List<RMOneDriveSetting> settings)
        {
            foreach (var setting in settings)
            {
                //TODO Need Derek Review
                if (/*setting.IsSyncData && */setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                {
                    if (setting.RunAutoFullJob)
                    {
                        logger.Info("Has auto full setting, should run full job. Setting id:{0}", setting.ScopeId);
                        return true;
                    }

                    if (HasAutoOlderThanRule(setting.AutoClassificationRules))
                    {
                        logger.Info("Has auto older than rule, should run full job. Setting id:{0}", setting.ScopeId);
                        return true;
                    }
                }

                //TODO Need Derek Review
                if (/*setting.IsSyncData && */setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                {
                    if (setting.NeedCheckDefaultValue)
                    {
                        logger.Info("Has setting that needs to check default value, should run full job. Setting id:{0}", setting.ScopeId);
                        return true;
                    }
                }

                if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                {
                    if (setting.RunAutoFullJob)
                    {
                        logger.Info("Has ai full setting, should run full job. Setting id:{0}", setting.ScopeId);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasAutoOlderThanRule(string autoRulesStr)
        {
            List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(autoRulesStr);
            foreach (var autoRule in autoRules)
            {
                if (!autoRule.IsDefaultRule)
                {
                    foreach (var filterGroup in autoRule.FilterGroups)
                    {
                        if (filterGroup.Filters.Any(f => f.Condition == ArchiverFilterCondition.OlderThan))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected SPTreeNodeDto GetGroupNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        protected SPTreeNodeDto GetSiteCollectionNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        private RMOneDriveExplorerBase BuildJobWorker(SPDiscoverType discoverType, AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext, RMOneDriveSetting setting, Guid siteId, IAveSite site, AveObjectModelFactory aveObjectModelFactory, long lastScanTime, long mainJobStartTime, List<RMOneDriveSetting> allSettingsUnderSite, bool bulkImport, List<AveCamlQuery> camlQueries = null, bool forceUpdate = false)
        {
            using (var performance = new PerformanceScope("RMOneDriveExplorerProcessor.BuildJobWorker"))
            {
                RMSPDiscoverHelper discoverHelper = null;
                ISPDiscover sPDiscover = null;

                discoverHelper = new RMSPDiscoverHelper();
                sPDiscover = RMSPDiscoverFactory.CreateFactory(discoverHelper, discoverType);
                var retentionWorker = new RMOneDriveExplorerBase(discoverSite, treeNode, jobContext, setting, siteId, site, aveObjectModelFactory, lastScanTime, mainJobStartTime, allSettingsUnderSite);

                retentionWorker.Init(sPDiscover, discoverType, bulkImport, forceUpdate);
                return retentionWorker;
            }

        }
        private long GetLastScanTimeFromDB(string gorupId, string nodeId)
        {
            return RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.OneDriveExplorerSync, new Guid(gorupId), new Guid(nodeId));
        }

        private async Task<(AveDiscoverSite, IAveSite, AveObjectModelFactory)> GetDiscoverSiteAsync(SPTreeNodeDto site, long mainJobStartTime, long lastScanTime)
        {
            using (var performance = new PerformanceScope("RMOneDriveExplorerProcessor.initSite"))
            {
                IAveSite aveSite;
                AveObjectModelFactory aveObjectModelFactory;
                var remoteSite = new SharePointSettingUtility().GetRemoteSiteCollection(site.SPObjectId.ToString());
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                aveSite = mfactory.CreateSite(site.FullPath);
                aveObjectModelFactory = mfactory;
                return lastScanTime == DateTime.MinValue.Ticks ? (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive), aveSite, aveObjectModelFactory) : (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(lastScanTime, DateTimeKind.Utc), new DateTime(mainJobStartTime, DateTimeKind.Utc)), aveSite, aveObjectModelFactory);
            }
        }

    }
}
