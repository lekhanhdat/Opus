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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention
{

    public class RMEnforceRetentionProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEnforceRetentionProcessor));
        public string CurrentJobId { private set; get; }

        #region Castle Properties
        private IRMNodeFlagDao _rMNodeFlagDao;
        public IRMNodeFlagDao RMNodeFlagDao
        {
            get { return _rMNodeFlagDao ?? (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao)); }
            set { _rMNodeFlagDao = value; }
        }
        #endregion

        private RetentionDataCache RetentionCache = RetentionDataCache.Instance;

        public RMEnforceRetentionProcessor(string jobId)
        {
            CurrentJobId = jobId;
            columnInternalNameMap = new Dictionary<Guid, string>();
        }
        private Dictionary<Guid, string> columnInternalNameMap = null;

        public async System.Threading.Tasks.Task RunNowAsync()
        {
            using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.RunNow"))
            {
                //以site collection分的sub job，所以获取到的都是site collecttion节点
                List<SPTreeNodeDto> siteNodes = new List<SPTreeNodeDto>();
                
                JobContext jobContext = JobContext.GetInstance(CurrentJobId, Contract.JobMonitor.JobType.EnforceRetention);
                jobContext.ReportManager.Increase(1);
                jobContext.ReportManager.StartUpdateJobProgress();
                SPTreeNodeDto groupNode = null;
                IAveSite aveSite = null;
                string jobSummary = string.Empty;
                try
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");
                        var currentLabel = RetentionCache.LabelStateInfo.CurrentLabel;
                        if (currentLabel == null)
                        {
                            logger.Warn("no retention label setting configured.");
                            return;
                        }
                        List<RMSPTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting);
                        siteNodes = tempList.ConvertAll(node => RMDtoConverter.ConvertRMTree2SPTree(node));
                        foreach (var site in siteNodes)
                        {
                            try
                            {
                                logger.Info($"Scan node:{site.FullPath}, Id:{site.SPObjectId}");
                                groupNode = SPTreeNodeManagement.GetGroupNode(site);
                                ThrowUtil.ThrowIfNull(groupNode, "group node info empty.");
                                //List<AveCamlQuery> camlQueries = null;
                                long lastScanTime = GetLastScanTimeFromDB(groupNode.SPObjectId, site.SPObjectId);
                                RetentionCache.CacheTermChange(lastScanTime);

                                var scanTermChanged = RetentionCache.TermRetentionMapping.Count > 0;
                                var scanSPChanged = lastScanTime != DateTime.MinValue.Ticks;
                                logger.Info($"Main job start time is {jobContext.MainJobStartTime}, sub job start time is:{jobContext.JobStartTime.Ticks}");
                                if (scanSPChanged || scanTermChanged)
                                {
                                    (var discoverSite, aveSite) = await GetDiscoverSiteAsync(site, jobContext.JobStartTime.Ticks, lastScanTime);
                                    using (discoverSite)
                                    {
                                        using (aveSite)
                                        {
                                            RetentionCache.CacheSPLabelInfo(aveSite);

                                            var internalName = GetBCSColumnInternalName(site, aveSite);
                                            if (string.IsNullOrEmpty(internalName))
                                            {
                                                logger.Warn($"site have no bcs column:{site.FullPath}");
                                                continue;
                                            }
                                            if (scanTermChanged)
                                            {
                                                CAMLManagerUtil.Init(aveSite);
                                            }
                                        }

                                        if (scanTermChanged)
                                        {
                                            logger.Info($"site:{site.FullPath}, term change:{string.Join(",", RetentionCache.TermRetentionMapping.Keys)}");
                                            var jobWorker = BuildJobWorker(SPDiscoverType.Full, discoverSite, site, jobContext, lastScanTime);
                                            await jobWorker.RunNowAsync();
                                        }
                                        //lastScanTime = DateTime.UtcNow.AddDays(-60).Ticks;
                                        else if (scanSPChanged)
                                        {
                                            if (NeedRunSearchDiscover(lastScanTime))
                                            {
                                                logger.Info($"site:{site.FullPath}, search scan. Last job time:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()}");
                                                var jobWorker = BuildJobWorker(SPDiscoverType.CAMLSearch, discoverSite, site, jobContext, lastScanTime);
                                                await jobWorker.RunNowAsync();
                                            }
                                            else
                                            {
                                                logger.Info($"site:{site.FullPath}, sp change date from:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()} to {new DateTime(jobContext.JobStartTime.Ticks, DateTimeKind.Utc).ToString()}");
                                                var jobWorker = BuildJobWorker(SPDiscoverType.Incremental, discoverSite, site, jobContext, lastScanTime);
                                                WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate = false;
                                                await jobWorker.RunNowAsync();
                                            }
                                        }
                                    }

                                }
                                if (!jobContext.NodeLevelError)
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
                                        NodeFlagType = (int)NodeFlagType.EnforceRetention
                                    });

                                }
                                CheckLabelExist(jobContext, site);

                            }
                            catch (JobStopException)
                            {
                                jobContext.JobHasStopped = true;
                                throw new JobStopException("the job has stopped.");
                            }
                            catch (LabelNotExistException)
                            {
                                var processingLabelName = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                                jobContext.HasErrorNode = true;
                                jobContext.NodeLevelError = true;
                                jobSummary = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}";
                                jobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                                {
                                    ObjectName = site?.Name,
                                    SourceURL = site?.FullPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = jobSummary
                                });
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Process Site error, Path:{site?.FullPath}, ERROR:{ex.ToString()}");
                                jobContext.HasErrorNode = true;
                                jobContext.NodeLevelError = true;
                                jobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                                {
                                    ObjectName = site?.Name,
                                    SourceURL = site?.FullPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = ex.Message,
                                });
                            }
                            finally
                            {
                                groupNode = null;
                                jobContext.NodeLevelError = false;
                                RetentionCache.Dispose();
                            }
                        }
                    }  
                }
                catch (JobStopException)
                {
                    jobContext.JobHasStopped = true;
                    logger.Warn("the job has stopped.");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error($"error occurred while Process Retention Job, ERROR:{e.ToString()}");
                    jobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = string.Empty,
                        SourceURL = string.Empty,
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                    });
                }
                finally
                {
                    jobContext.Finish(jobSummary);
                }

            }
        }


        //上次运行job是在59天以前，本次Job采用CAML Query方式，防止由于change log被冲掉了导致少查数据
        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }


        private RMEnforceRetentionBase BuildJobWorker(SPDiscoverType discoverType, AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext, long lastJobTicks)
        {
            using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.BuildJobWorker"))
            {
                RMSPDiscoverHelper discoverHelper = null;
                ISPDiscover sPDiscover = null;

                discoverHelper = new RMSPDiscoverHelper();
                sPDiscover = RMSPDiscoverFactory.CreateFactory(discoverHelper, discoverType);
                var retentionWorker = new RMEnforceRetentionBase(discoverSite, treeNode, jobContext);
                retentionWorker.Init(sPDiscover, discoverType,lastJobTicks);
                return retentionWorker;
            }

        }

        private void CheckLabelExist(JobContext jobContext, SPTreeNodeDto site) 
        {
            var processingLabelName = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
            if (RetentionDataCache.Instance.SPSiteRetentionLables != null && !RetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out AveComplianceTagInfo tagInfo))
            {
                if (!jobContext.HasSuccessNode) 
                {
                    //job 空跑, 添加skip提示.
                    jobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = site?.Name,
                        SourceURL = site?.FullPath,
                        Status = JobDetailsStatus.Skipped,
                        Comment = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}"
                    });
                }
            }
        }
        private long GetLastScanTimeFromDB(string gorupId, string nodeId)
        {
            var collectionTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.EnforceRetention, new Guid(gorupId), new Guid(nodeId));
            //if (DateTime.UtcNow.AddDays())
            return collectionTime;
        }

        private async Task<(AveBPOSAccountInfo,bool)> GetBposInfoAsync(AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSite)
        {
            Contract.Configurations.CustomAppConfigs configs = RA.Common.Configurations.RMGlobalConfiguration.AppConfig.CustomAppConfig;
            if (configs != null && configs.CustomApps != null && configs.CustomApps.Count > 0)
            {
                Contract.Configurations.CustomApp app = configs.CustomApps.FirstOrDefault(a => string.Equals(a.TenantId.ToString(), Contract.Tenant.TenantLocalValue.LogonGroupId, StringComparison.OrdinalIgnoreCase));
                if (app != null)
                {
                    return await PoolUserUtil.GetCustomBPOSInfoAsync(remoteSite, app.AppClientId);
                }
            }
            bool useSpecialApp = false;
            return (await PoolUserUtil.GetBPOSInfoAsync(remoteSite), useSpecialApp);
        }

        private async Task<(AveDiscoverSite,IAveSite)> GetDiscoverSiteAsync(SPTreeNodeDto site, long mainJobStartTime, long lastScanTime)
        {
            using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.initSite"))
            {
                IAveSite aveSite;
                var remoteSite = new SharePointSettingUtility().GetRemoteSiteCollection(site.SPObjectId.ToString());
                bool useSpecialApp = false;
                (var bposInfo, useSpecialApp) = await this.GetBposInfoAsync(remoteSite); //PoolUserUtil.GetBPOSInfo(remoteSite);
                logger.Info($"Use Special App Profile: {useSpecialApp}");
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel, useSpecialApp);
                aveSite = mfactory.CreateSite(site.FullPath);

                return lastScanTime == DateTime.MinValue.Ticks ? (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive),aveSite ): (new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(lastScanTime, DateTimeKind.Utc), new DateTime(mainJobStartTime, DateTimeKind.Utc)),aveSite);
            }

        }

        private string GetBCSColumnInternalName(SPTreeNodeDto treeNode, IAveSite aveSite)
        {
            using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser..GetSPSetting"))
            {
                var internalName = string.Empty;

                var scopeId = Guid.Parse(treeNode.SPObjectId);
                var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(treeNode).SPObjectId);

                if (columnInternalNameMap.ContainsKey(scopeId))
                {
                    return columnInternalNameMap[scopeId];
                }
                var columnName = new SharePointSettingUtility().GetMedataColumn(groupId);
                if (!string.IsNullOrEmpty(columnName))
                {
                    var field = GetTaxonomyField(aveSite.RootWeb.Fields, columnName);
                    if (field != null)
                    {
                        internalName = field.InternalName;
                        if (!columnInternalNameMap.ContainsKey(scopeId))
                        {
                            columnInternalNameMap.Add(scopeId, internalName);
                        }
                        RetentionDataCache.Instance.BCSColumnID = field.ID;
                    }
                    RetentionDataCache.Instance.BCSColumnInternalName = internalName;
                }
                return internalName;
            }


        }

        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            return fields.GetRecordTaxonomyField(rmFieldTitle);
        }

    }
}
