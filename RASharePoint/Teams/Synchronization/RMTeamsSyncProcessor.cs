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
using System.Threading.Tasks;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Newtonsoft.Json;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;

namespace AvePoint.RA.SharePoint.Teams.Synchronization
{
    public class RMTeamsSyncProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMTeamsSyncProcessor));

        #region Castle Properties
        private readonly IRMNodeFlagDao RMNodeFlagDao = PlatformWindsorManager.GetService<IRMNodeFlagDao>();
        private readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly ITeamsSettingDao TeamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private readonly IRMTeamsSettingsService TeamsSettingService = PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
        private readonly ITeamsSettingTreeService RMTeamsTreeService = PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        #endregion

        private JobContext jobContext = null;
        private Dictionary<Guid, string> columnInternalNameMap = null;

        private RMSPExplorerDataCache ExplorerCache = null;
        private bool isCosmosBulkOperationEnabled = true;

        public RMTeamsSyncProcessor(string jobId)
        {
            RMSPExplorerDataCache.SourceFlag = SourceFlag.Teams;
            ExplorerCache = RMSPExplorerDataCache.Instance;
            columnInternalNameMap = new Dictionary<Guid, string>();
            jobContext = JobContext.GetInstance(jobId, JobType.TeamsDataSynchronisation);
            jobContext.ReportManager.StartUpdateJobProgress();
            isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();

            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                MainJobId = jobContext.MainJobId,
                SubJobId = jobContext.SubJobId,
                JobType = JobType.TeamsDataSynchronisation
            });
        }

        public async Task RunNowAsync()
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcesser.RunNow"))
            {
                CompoundDisposalStatistics.Instance.StartStatistic();
                List<SPTreeNodeDto> availableSites = new();
                SPTreeNodeDto groupNode = null;
                IAveSite aveSite = null;
                try
                {
                    WrapperConfiguration.JobDir = AveEnv.AgentTempFolder;
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");
                        List<RMSPTreeNode> targetNodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting);
                        foreach(var targetNode in targetNodes)
                        {
                            availableSites = await GetAvailableSitesFromTreeNode(targetNode);
                            _logger.Info($"The sites is available to scan, URL [{string.Join(", ", availableSites)}].");
                            foreach (var site in availableSites)
                            {
                                string siteId = string.Empty;
                                try
                                {
                                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                                    {
                                        _logger.Info($"Scan node:{site.FullPath}, Id:{site.SPObjectId}, TeamsID:{site.TeamsId}.");
                                        groupNode = SPTreeNodeManagement.GetGroupNode(site);
                                        ThrowUtil.ThrowIfNull(groupNode, "group node info empty.");
                                        long lastScanTime = GetLastScanTimeFromDB(groupNode.SPObjectId, site.SPObjectId);
                                        _logger.Info($"Main job start time is {jobContext.MainJobStartTime}, sub job start time is:{jobContext.JobStartTime.Ticks}");
                                        (var discoverSite, aveSite) = await GetDiscoverSiteAsync(site, jobContext.JobStartTime.Ticks, lastScanTime);
                                        using (discoverSite)
                                        {
                                            siteId = discoverSite.SiteID.ToString();
                                            using (aveSite)
                                            {
                                                Guid bcsColumnID = Guid.Empty;
                                                var internalName = GetBCSColumnInternalName(site, aveSite, ref bcsColumnID);
                                                _logger.Info($"BCS column internal name: [{internalName}]");
                                                if (string.IsNullOrEmpty(internalName))
                                                {
                                                    _logger.Warn($"site doesn't have bcs column:{site.FullPath}");
                                                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                                                    {
                                                        ObjectName = site?.Name,
                                                        FullPath = site?.FullPath,
                                                        Status = JobDetailsStatus.Skipped,
                                                        Comment = "RM_SPS_DS_NotFoundBCSColumn",
                                                    });
                                                    continue;
                                                }

                                                RMSPExplorerDataCache.Instance.InitSiteLevelCache(siteId, new RMSPExplorerSiteLevelCache
                                                {
                                                    TeamsId = new Guid(site.TeamsId),
                                                    AveSiteId = site.ID,
                                                    BCSColumnInternalName = internalName,
                                                    BCSColumnID = bcsColumnID,
                                                    HasErrorNode = false,
                                                    SPSiteId = aveSite.ID
                                                });

                                                RMTeamsExplorer jobWorker = null;
                                                if (lastScanTime == DateTime.MinValue.Ticks)
                                                {
                                                    bool forceUpdate = RMKeyValueDao.GetForceUpdate();
                                                    _logger.Info($"site:{site.FullPath}, full scan. Is force update: [{forceUpdate}]");
                                                    jobWorker = BuildJobWorker(SPDiscoverType.Full, discoverSite, site, jobContext, lastScanTime, jobContext.MainJobStartTime, isCosmosBulkOperationEnabled, null, forceUpdate);
                                                    await jobWorker.RunNowAsync();
                                                }
                                                else if (NeedRunSearchDiscover(lastScanTime))
                                                {
                                                    _logger.Info($"site:{site.FullPath}, search scan. Last job time:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()}");
                                                    jobWorker = BuildJobWorker(SPDiscoverType.CAMLSearch, discoverSite, site, jobContext, lastScanTime, jobContext.MainJobStartTime, isCosmosBulkOperationEnabled);
                                                    await jobWorker.RunNowAsync();
                                                    jobWorker.ProcessTermChangedItems(lastScanTime);
                                                }
                                                else
                                                {
                                                    _logger.Info($"Site: {site.FullPath}, sp inc date from:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()} Ticks:{lastScanTime} to {new DateTime(jobContext.JobStartTime.Ticks, DateTimeKind.Utc).ToString()} Ticks:{jobContext.JobStartTime.Ticks}");
                                                    jobWorker = BuildJobWorker(SPDiscoverType.Incremental, discoverSite, site, jobContext, lastScanTime, jobContext.JobStartTime.Ticks, isCosmosBulkOperationEnabled);
                                                    await jobWorker.RunNowAsync();
                                                    jobWorker.ProcessTermChangedItems(lastScanTime);
                                                }
                                                await jobWorker.FailedItemsInSiteAsync();
                                                await jobWorker.ProcessInheritedParentTermItemsAsync();
                                            }
                                            var siteCache = ExplorerCache.SiteLevelCache[siteId];
                                            if (!siteCache.HasErrorNode && !siteCache.HasSkippedLifecycleList)
                                            {
                                                _logger.Info($"Begin update the site node flag info into db. Collect time [{jobContext.JobStartTime.Ticks}]");
                                                RMNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag()
                                                {
                                                    NodeId = new Guid(site.SPObjectId),
                                                    Title = site.Name,
                                                    FullPath = site.FullPath,
                                                    CollectionTime = jobContext.JobStartTime.Ticks,
                                                    GroupId = new Guid(groupNode.SPObjectId),
                                                    IsRemoved = false,
                                                    NodeFlagType = (int)NodeFlagType.TeamsSync
                                                });
                                            }

                                        }
                                    }

                                }
                                catch (JobStopException)
                                {
                                    jobContext.JobHasStopped = true;
                                    throw new JobStopException("The job has stopped.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error($"Process Site error, Path:{site?.FullPath}, ERROR:{ex.ToString()}");
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
                        }
                    }
                }
                catch (JobStopException)
                {
                    jobContext.JobHasStopped = true;
                    _logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    jobContext.HasErrorNode = true;
                    _logger.Error($"An error occurred while process sync job, ERROR:{e.ToString()}");
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
                    UpdateMovedData();
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                    jobContext.Finish();
                    PerformanceMonitor.WritePerformanceResult();
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
                if (RMExplorerBoardCache.Instance.MovedDataCache != null && RMExplorerBoardCache.Instance.MovedDataCache.Count > 0)
                {
                    ExplorerDao.UpdateAll(r => RMExplorerBoardCache.Instance.MovedDataCache.Keys.Contains(r.Id), r => { r.MetaInfo = RemoveDataStatus(r.MetaInfo); });
                }
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while updating moved data. Error:{0}", e.ToString());
            }
        }

        private string RemoveDataStatus(string metaInfo)
        {
            var meta = JsonConvert.DeserializeObject<RecordMetaInfo>(metaInfo);
            meta.DataStatus = (int)DataStatus.None;
            return JsonConvert.SerializeObject(meta);
        }

        private RMTeamsExplorer BuildJobWorker(SPDiscoverType discoverType, AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext, long lastJobTicks, long mainJobTicks, bool bulkImport, List<AveCamlQuery> camlQueries = null, bool forceUpdate = false)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.BuildJobWorker"))
            {
                RMSPDiscoverHelper discoverHelper = null;
                ISPDiscover sPDiscover = null;
                discoverHelper = new RMSPDiscoverHelper();
                sPDiscover = RMSPDiscoverFactory.CreateFactory(discoverHelper, discoverType);
                var retentionWorker = new RMTeamsExplorer(discoverSite, treeNode, jobContext);
                retentionWorker.Init(sPDiscover, discoverType, lastJobTicks, mainJobTicks, bulkImport, forceUpdate);
                return retentionWorker;
            }
        }

        private long GetLastScanTimeFromDB(string gorupId, string nodeId)
        {
            return RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.TeamsSync, new Guid(gorupId), new Guid(nodeId));
        }

        private async Task<(AveBPOSAccountInfo, bool)> GetBposInfoAsync(GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSite)
        {
            bool useSpecialApp = false;
            CustomAppConfigs configs = RMGlobalConfiguration.AppConfig.CustomAppConfig;
            if (configs != null && configs.CustomApps != null && configs.CustomApps.Count > 0)
            {
                CustomApp app = configs.CustomApps.FirstOrDefault(a => string.Equals(a.TenantId.ToString(), Contract.Tenant.TenantLocalValue.LogonGroupId, StringComparison.OrdinalIgnoreCase));
                if (app != null)
                {
                    return await PoolUserUtil.GetCustomBPOSInfoAsync(remoteSite, app.AppClientId);
                }
            }
            useSpecialApp = false;
            return (await PoolUserUtil.GetBPOSInfoAsync(remoteSite), useSpecialApp);
        }

        private async Task<(AveDiscoverSite, IAveSite)> GetDiscoverSiteAsync(SPTreeNodeDto site, long mainJobStartTime, long lastScanTime)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.initSite"))
            {
                IAveSite aveSite;
                var remoteSite = new SharePointSettingUtility().GetRemoteSiteCollection(site.SPObjectId.ToString());
                bool useSpecialApp = false;
                (var bposInfo, useSpecialApp) = await GetBposInfoAsync(remoteSite); //PoolUserUtil.GetBPOSInfo(remoteSite);
                _logger.Info($"Use Special App Profile: {useSpecialApp}");
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel, useSpecialApp);
                aveSite = mfactory.CreateSite(site.FullPath);

                return lastScanTime == DateTime.MinValue.Ticks ? (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive), aveSite) : (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(lastScanTime, DateTimeKind.Utc), new DateTime(mainJobStartTime, DateTimeKind.Utc)), aveSite);
            }
        }

        private string GetBCSColumnInternalName(SPTreeNodeDto treeNode, IAveSite aveSite, ref Guid bcsColumnID)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor..GetSPSetting"))
            {
                var internalName = string.Empty;

                var scopeId = Guid.Parse(treeNode.SPObjectId);
                var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(treeNode).SPObjectId);

                if (columnInternalNameMap.ContainsKey(scopeId))
                {
                    return columnInternalNameMap[scopeId];
                }
                var columnName = TeamsSettingDao.GetMedataColumn(groupId);
                if (!string.IsNullOrEmpty(columnName))
                {
                    _logger.Info("Column name on group:{0}, groupId {1}", columnName, groupId);
                    var field = GetTaxonomyField(aveSite.RootWeb.Fields, columnName);
                    if (field != null)
                    {
                        internalName = field.InternalName;
                        if (!columnInternalNameMap.ContainsKey(scopeId))
                        {
                            columnInternalNameMap.Add(scopeId, internalName);
                        }
                        bcsColumnID = field.ID;
                    }

                }
                return internalName;
            }


        }

        private IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            return fields.GetRecordTaxonomyField(rmFieldTitle);
        }

        private async Task<List<SPTreeNodeDto>> GetAvailableSitesFromTreeNode(RMSPTreeNode targetNode)
        {
            if (targetNode.Level != (int)NodeLevel.Office365GroupEntire) return new();
            var siteNodes = await RMTeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(targetNode));
            await TeamsSettingService.LoadSiteSettingsUnderTeamsNodeAsync(siteNodes, targetNode);
            TeamsSettingService.FilterSitesModified(siteNodes, out List<RMSPTreeNode> modifiedSites);
            if(IsHasContainerLevelInheritChanged(targetNode))
            {
                _logger.Info($"The container level inherit option has changed, need to resync all sites under the team:{targetNode.Name}");
                modifiedSites = siteNodes;
            }
            return modifiedSites.ConvertAll(site => RMDtoConverter.ConvertRMTree2SPTree(site));
        }

        private bool IsHasContainerLevelInheritChanged(RMSPTreeNode targetNode)
        {
            var teamsId = targetNode.TeamsId;
            var groupId = targetNode.Parent.Id;
            var result = TeamsSettingDao.CheckHasInheritChanged(new Guid(groupId), new Guid(teamsId));
            if (!result)
            {
                result = TeamsSettingDao.CheckGroupHasInheritChanged(new Guid(groupId));
            }
            else
            {
                _logger.Info($"The inherit option has changed for team:{targetNode.Name}");
                TeamsSettingDao.UpdateChangedInheritOptionFlag(new Guid(groupId), new Guid(teamsId));
            }

            var hasInheritChangedUnderGroup = TeamsSettingDao.CheckHasInheritChangedUnderGroup(new Guid(groupId));
            if(!hasInheritChangedUnderGroup)
            {
                _logger.Info($"The inherit option has changed for group:{targetNode.Name}");
                TeamsSettingDao.UpdateChangedInheritOptionFlag(new Guid(groupId), Guid.Empty);
            }

            return result;
        }
    }
}
