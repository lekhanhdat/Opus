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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using AvePoint.RA.Service.Services.AzureFileShare.RuleManagement;
using RADataSynchronize.TermCheck.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Exceptions;

namespace RAAzureFile.DataSync
{
    public class DataSyncProcessor
    {
        public static readonly RALogger Logger = RALogger.GetInstance(typeof(DataSyncProcessor));

        private static readonly IRMAzureFileShareConnectionService AzureFileShareConnectionService =
    PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

        private static readonly IRMAzureFileShareSyncJobProcessInfoDao AzureFileShareSyncJobProcessInfoDao =
    PlatformWindsorManager.GetService<IRMAzureFileShareSyncJobProcessInfoDao>();

        private static readonly IJobMonitorService JobMonitorService =
    PlatformWindsorManager.GetService<IJobMonitorService>();

        private static readonly ConcurrentQueue<Tuple<AzureFileShareApiItem, AzureFileSettingDto, UniqueIdUtil>> NeedProcessFileQueue = new ConcurrentQueue<Tuple<AzureFileShareApiItem, AzureFileSettingDto, UniqueIdUtil>>();

        private static readonly int BatchReadMaxItemsCount = 1000;

        private static readonly int MaxDegreeOfParallelism = 5;

        private static readonly TimeSpan DefaultTaskThreadSleepTime = TimeSpan.FromMilliseconds(200);

        private static readonly long JobStartTime = DateTime.UtcNow.Ticks;

        private static readonly string TenantId = TenantLocalValue.LogonGroupId;

        private static readonly string UserName = TenantLocalValue.LogonUserEmail;

        private static AzureFileShareTreeNode SelectedNode;

        private static long LatestSyncJobProcessTime = 0;

        private static bool ProcessFileTaskNeedQuit = false;

        public static async Task ProcessAsync(string subJobId)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    DataSyncJobManager.Init(subJobId);
                    var jobContent = DataSyncJobManager.GetJobContent(subJobId);
                    var selectedNode = SelectedNode = JsonConvert.DeserializeObject<AzureFileShareTreeNode>(jobContent);
                    Logger.Info($"Selected run data sync job node [{selectedNode.Id}], real id [{selectedNode.RealId}], connection [{selectedNode.ConnectionId}]");

                    AzureFileShareApiContext apiContext;
                    bool isAvailable = false;
                    (isAvailable, apiContext) = await ConnectionIsAvailableAsync(selectedNode.ConnectionId);

                    if (!isAvailable)
                    {
                        DataSyncJobManager.SetJobFailed("RM_JS_JMD_AF_ConnectionNotAvailable");
                        return;
                    }

                    if (selectedNode.Level == RMNodeLevel.AzureFileShareConnection)
                    {
                        selectedNode.FullPath = apiContext.ConnectionFullUrl;
                    }

                    if (!SelectedNodeIsAvailable(selectedNode, apiContext, out var directoryClient))
                    {
                        DataSyncJobManager.SetJobFailed("RM_JS_JMD_AF_NodeNotAvailable" + I18NEntity.Separator + selectedNode.FullPath);
                        return;
                    }

                    DataSyncFailedItemManager.Initialization(selectedNode.Id);

                    LatestSyncJobProcessTime = AzureFileShareSyncJobProcessInfoDao.GetLastJobProcessTime(new Guid(selectedNode.Id), selectedNode.FullPath);

                    await DataSyncSettingInfoManager.InitializationAsync(apiContext, directoryClient);

                    var tasks = StartProcessFilesTask();
                    await ProcessAsync(directoryClient);
                    Logger.Info($"The all directory scan completed.");
                    ProcessFileTaskNeedQuit = true;

                    Task.WaitAll(tasks);

                    DataSyncCosmosDBManager.Commit();

                    ProcessHasTermChangedFiles();

                    DataSyncCosmosDBManager.WaitComplete();

                    if (!DataSyncFailedItemManager.IsLimitExceeded && DataSyncFailedItemManager.StorageFailedItems())
                    {
                        AzureFileShareSyncJobProcessInfoDao.UpsertLastJobProcessTime(new Guid(selectedNode.Id), selectedNode.FullPath);
                        Logger.Info($"Is upsert selected node: [{selectedNode.Id}], real id: [{selectedNode.RealId}] last sync job process time.");
                    }
                    PerformanceMonitor.WritePerformanceResult();
                    DataSyncJobManager.SetJobFinished();
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process job. Error: {e}");
                DataSyncJobManager.SetJobFailed(e.Message);
            }
        }

        private static async Task<(bool, AzureFileShareApiContext)> ConnectionIsAvailableAsync(Guid connectionId)
        {
            AzureFileShareApiContext apiContext = null;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope()) 
                {
                    var connectionItem = await AzureFileShareConnectionService.GetAsync(connectionId);
                    var connectionInfo = AzureFileShareConnectionConverter.ConvertToConnectionInfo(connectionItem);
                    apiContext = new AzureFileShareApiContext(connectionInfo);
                    return (apiContext.EnsureConnection(), apiContext);
                }    
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while checked connection is available. Error: {e}");
                return (false, apiContext);
            }
        }

        private static bool SelectedNodeIsAvailable(AzureFileShareTreeNode selectedTreeNode, AzureFileShareApiContext apiContext, out AzureFileShareApiDirectoryClient directoryClient)
        {
            directoryClient = null;
            try
            {
                directoryClient = new AzureFileShareApiDirectoryClient(apiContext, selectedTreeNode.FullPath);
                return directoryClient.Exist();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while check selected node is available. Error: {e}");
                return false;
            }
        }

        private static async Task ProcessAsync(AzureFileShareApiDirectoryClient selectedDirectoryClient)
        {
            var runningSyncJobsScopeIds = JobMonitorService.GetRunningJobsScopeId(AvePoint.RA.Contract.JobMonitor.JobType.AzureFileShareDataSynchronisation);
            var directoryQueue = new Queue<AzureFileShareApiDirectoryClient>();
            directoryQueue.Enqueue(selectedDirectoryClient);
            while (directoryQueue.Any())
            {
                var directory = directoryQueue.Dequeue();
                try
                {
                    if (!directory.IsRoot &&
                        directory.Id.ToString() != SelectedNode.Id &&
                        runningSyncJobsScopeIds.Any(item => item == directory.Id.ToString()))
                    {
                        Logger.Warn($"Current azure file share directory [{directory.Id}] has running data synchronisation job. Skipped it.");
                        continue;
                    }

                    using (new PerformanceScope("AzureFileShare:DataSync:DirectoryScan", "", true))
                    {
                        var settingInfo = await DataSyncSettingInfoManager.LoadSettingInfoAsync(directory);

                        var itemCount = directory.GetSubItemsCount();
                        Logger.Info($"The directory [{directory.Id}] sub items count [{itemCount}].");
                        var idUtil = new UniqueIdUtil(TenantId, itemCount + 1);
                        for (var skipCount = 0; skipCount < itemCount; skipCount += BatchReadMaxItemsCount)
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                var items = directory.GetSubDirectoriesAndFiles(skipCount, BatchReadMaxItemsCount);
                                if (skipCount + items.Count > itemCount)
                                {
                                    var subItemsCount = directory.GetSubItemsCount();
                                    idUtil = new UniqueIdUtil(TenantId, subItemsCount - itemCount + 1);
                                    itemCount = subItemsCount;
                                }
                                foreach (var item in items)
                                {
                                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                                    {
                                        if (item.IsDirectory)
                                        {
                                            directoryQueue.Enqueue(item.ToDirectoryClient());
                                            continue;
                                        }

                                        if (DataSyncFailedItemManager.HasPreviouslyFailedItem(item.Id) ||
                                            item.Modified > LatestSyncJobProcessTime ||
                                            (settingInfo.DeployTermMethod == DeployTermMethod.UseDefaultTerm && settingInfo.NeedCheckDefaultValue) ||
                                            (settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification && settingInfo.RunAutoFullJob))
                                        {
                                            NeedProcessFileQueue.Enqueue(Tuple.Create(item, settingInfo, idUtil));
                                        }
                                    }
                                }
                            }
                        }

                        Logger.Info($"The directory [{directory.Id}] scan sub items succeed.");
                        ProcessDirectory(directory, idUtil);
                        await DataSyncSettingInfoManager.ResetSettingInfoAsync(directory);
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("the job has stopped.");
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process directory: [{directory.Id}]. Error: {e}");
                    DataSyncJobManager.AddFailedJobDetail(directory, e.Message);
                    DataSyncFailedItemManager.AddFailedItem(directory);
                }
            }
        }

        private static Task[] StartProcessFilesTask()
        {
            try
            {
                var tasks = new List<Task>();
                for (var i = 0; i < MaxDegreeOfParallelism; i++)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        tasks.Add(Task.Run(() => TenantUtil.RunUnderTenant(TenantId, UserName, ProcessFile)));
                    }
                }
                return tasks.ToArray();
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            
        }

        private static void ProcessDirectory(AzureFileShareApiDirectoryClient directoryClient, UniqueIdUtil idUtil)
        {
            using (new PerformanceScope("AzureFileShare:DataSync:ProcessDirectory", "", true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope()) 
                    {
                        if (directoryClient.IsRoot)
                        {
                            Logger.Warn("The can't diroectory is root. Skipped it.");
                            return;
                        }

                        if (!DataSyncFailedItemManager.HasPreviouslyFailedItem(directoryClient.Id) &&
                            directoryClient.Modified <= LatestSyncJobProcessTime)
                        {
                            return;
                        }

                        if (!DataSyncCosmosDBManager.TryGet(directoryClient.Id, out var existItem))
                        {
                            existItem = new Record
                            {
                                RecordStatus = 1,
                                CreateDate = Convert.ToInt32(new DateTime(directoryClient.Created).ToString("yyyyMMdd")),
                                TimeCreated = directoryClient.Created,
                                RecordsId = idUtil.GenerateUniqueId(),
                            };
                        }

                        existItem.CollectTime = DateTime.UtcNow.Ticks;
                        existItem.SourceFlag = (int)SourceFlag.AzureFileShare;
                        existItem.Id = directoryClient.Id;
                        existItem.ParentId = directoryClient.ParentId;
                        existItem.LeafName = directoryClient.Name;
                        existItem.DirPath = directoryClient.FullPath.Substring(0, directoryClient.FullPath.LastIndexOf('/'));
                        existItem.ExtensionForFile = "RM_RDM_RecordDetails_DataType_AzureFileDirectory";
                        existItem.ItemId = directoryClient.Id;
                        existItem.NodeId = directoryClient.Id;
                        existItem.NodeType = (int)RMNodeLevel.AzureFileShareDirectory;
                        existItem.TimeModified = directoryClient.Modified;
                        existItem.ExternalId = directoryClient.RealId;
                        existItem.AveSiteId = SelectedNode.ConnectionId.ToString();
                        existItem.ScopeId = SelectedNode.ConnectionId;
                        existItem.ContainerId = SelectedNode.ContainerId;

                        DataSyncCosmosDBManager.Add(existItem);
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("the job has stopped.");
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process directory. Error: {e}");
                    throw e;
                }
            }
        }

        private static void ProcessFile()
        {
            while (!ProcessFileTaskNeedQuit || NeedProcessFileQueue.Any())
            {
                if (!NeedProcessFileQueue.TryDequeue(out var needProcessFileItem))
                {
                    Task.Delay(DefaultTaskThreadSleepTime).GetAwaiter().GetResult();
                    continue;
                }
                try
                {
                    using (new PerformanceScope("AzureFileShare:DataSync:ProcessFile", "", true))
                    {
                        var fileItem = needProcessFileItem.Item1;
                        var settingInfo = needProcessFileItem.Item2;

                        var exist = DataSyncCosmosDBManager.TryGet(fileItem.Id, out var existItem);
                        if (exist &&
                            existItem.TermId != Guid.Empty &&
                            (
                                settingInfo.DeployTermMethod == DeployTermMethod.NoDefaultTerm ||
                                (settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification &&
                                settingInfo.AutoJobOption != AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override) ||
                                (settingInfo.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                                (!settingInfo.NeedCheckDefaultValue || settingInfo.ApplyExistType != (int)ApplyExistingTermType.OverWrite))
                            ))
                        {
                            if(existItem.TimeModified < fileItem.Modified)
                            {
                                var modifiedItem = ConvertFileItemToRecord(fileItem, existItem, needProcessFileItem.Item3);
                                DataSyncCosmosDBManager.Add(modifiedItem);
                                Logger.Info($"Modified item [{fileItem.Id} - {fileItem.RealId}] properties.");
                                continue;
                            }
                            Logger.Info($"The item [{fileItem.Id} - {fileItem.RealId}] already exist in record and not setting overwrite. Skipped it.");
                            continue;
                        }

                        var termInfo = DataSyncTermManager.GetMatchedTermInfo(fileItem, settingInfo);
                        if(termInfo != null && 
                            exist && 
                            existItem.TermId != Guid.Empty && 
                            new Guid(termInfo.TermId) == existItem.TermId &&
                            existItem.TimeModified == fileItem.Modified)
                        {
                            Logger.Info($"The item [{fileItem.Id} - {fileItem.RealId}] latest term is equals will apply term. Skipped it.");
                            continue;
                        }

                        var record = ConvertFileItemToRecord(fileItem, existItem, needProcessFileItem.Item3);
                        if (termInfo != null && termInfo.IsManually)
                        {
                            Logger.Warn($"The item [{fileItem.Id} - {fileItem.RealId}] is used manually setting.");
                            DataSyncCosmosDBManager.Add(record);
                            continue;
                        }

                        ApplyTermInfo(record, termInfo);

                        var fileInfo = AzureFileShareRecordConverter.ConvertAzureFileItem2AzureFileInfo(fileItem);

                        ApplyRuleInfo(fileInfo, record, termInfo?.TermId);

                        DataSyncCosmosDBManager.Add(record);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process file [{needProcessFileItem.Item1.Id}], realId [{needProcessFileItem.Item1.RealId}]. Error: {e}");
                    DataSyncFailedItemManager.AddFailedItem(needProcessFileItem.Item1);
                    DataSyncJobManager.AddFailedJobDetail(needProcessFileItem.Item1, e.Message);
                }
                finally
                {
                    Task.Delay(DefaultTaskThreadSleepTime).GetAwaiter().GetResult();
                }
            }
        }

        private static void ProcessHasTermChangedFiles()
        {
            try
            {
                if (LatestSyncJobProcessTime == 0)
                {
                    Logger.Warn($"Current node [{SelectedNode.Id}] is first run data sync job. Skip process has term changed files.");
                    return;
                }

                var changedTerms = DataSyncTermManager.GetHasChangedTermIds(LatestSyncJobProcessTime);
                Logger.Info($"Changed terms [{string.Join(", ", changedTerms)}] after [{LatestSyncJobProcessTime}].");
                if (changedTerms.Count == 0)
                {
                    return;
                }

                var items = DataSyncCosmosDBManager.QueryItems(item =>
                    item.SourceFlag == (int)SourceFlag.AzureFileShare &&
                    item.CollectTime < JobStartTime &&
                    changedTerms.Contains(item.TermId)
                );
                foreach (var batchItems in items)
                {
                    foreach (var item in batchItems)
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            ProcessHasTermChangedFile(item);
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process has term changed files. Error: {e}");
            }
        }

        private static void ProcessHasTermChangedFile(Record item)
        {
            using (new PerformanceScope("AzureFileShare:DataSync:ProcessHasTermChangedFile", "", true))
            {
                try
                {
                    var fileInfo = AzureFileShareRecordConverter.ConvertAzureFileItem2AzureFileInfo(item);
                    ApplyRuleInfo(fileInfo, item, item.TermId == Guid.Empty ? string.Empty : item.TermId.ToString());
                    item.CollectTime = DateTime.UtcNow.Ticks;

                    DataSyncCosmosDBManager.Add(item);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process file [{item.Id}] term changed operation. Error: {e}");
                    DataSyncFailedItemManager.AddFailedItem(item);
                    DataSyncJobManager.AddFailedJobDetail(item, e.Message);
                }
            }
        }

        private static void ApplyTermInfo(Record record, TermInfo termInfo)
        {
            if (termInfo.TermIsDeprecated || termInfo.TermIsRemoved)
            {
                throw new Exception("RM_FS_DisposalDetail_TermIsInvalid" + I18NEntity.Separator + termInfo.TermName);
            }

            record.TermId = new Guid(termInfo.TermId);
            record.TermName = termInfo.TermName;
        }

        private static void ApplyRuleInfo(AzureFileInfo fileInfo, Record record, string termId)
        {
            record.RuleId = Guid.Empty;
            record.RuleLevel = (int)PolicyLevel.None;
            record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

            if (!DataSyncTermRuleInfoManager.TryGetTermRelatedRule(termId, out var termRelatedRules))
            {
                Logger.Warn($"The term [{termId}] is not related rules.");
                return;
            }

            var matchedRule = new AzureFileShareRuleManagement(termRelatedRules).MatchPotentialRule(fileInfo, true);
            if (matchedRule == null)
            {
                Logger.Warn($"The item [{record.Id} - {record.AveSiteId}] is not match any rule.");
                return;
            }

            var ruleInfo = matchedRule.Item1;
            var dueDate = matchedRule.Item2;
            record.RuleId = string.IsNullOrEmpty(ruleInfo.Id) ? Guid.Empty : new Guid(ruleInfo.Id);
            record.RuleLevel = (int)ruleInfo.PolicyLevel;
            record.DisposalDueDate = record.PreviosDisposalDueDate = dueDate == default ? AvePoint.RA.Contract.Common.DueDateUtil.NextJob : DateTime.UtcNow.Add(dueDate).Ticks;
            if (record.HoldStatus)
            {
                if (record.DisposalDueDate == AvePoint.RA.Contract.Common.DueDateUtil.NextJob)
                {
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.NextJob;//record.HoldReleaseTime;
                }
                if (record.DisposalDueDate < record.HoldReleaseTime)
                {
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = record.HoldReleaseTime;
                }

            }
        }

        private static Record ConvertFileItemToRecord(AzureFileShareApiItem item, Record existItem, UniqueIdUtil idUtil)
        {
            if (existItem == null)
            {
                existItem = new Record
                {
                    RecordStatus = 1,
                    CreateDate = Convert.ToInt32(new DateTime(item.Created).ToString("yyyyMMdd")),
                    TimeCreated = item.Created,
                    RecordsId = idUtil.GenerateUniqueId()
                };
            }

            var ext = Path.GetExtension(item.Name);
            var type = ext.IndexOf(".") == 0 ? ext.Substring(1) : "";

            existItem.CollectTime = DateTime.UtcNow.Ticks;
            existItem.SourceFlag = (int)SourceFlag.AzureFileShare;
            existItem.Id = item.Id;
            existItem.ParentId = item.ParentId;
            existItem.LeafName = item.Name;
            existItem.DirPath = item.FullPath.Substring(0, item.FullPath.LastIndexOf('/'));
            //existItem.FullPath = item.FullPath;
            existItem.ExtensionForFile = type;
            existItem.ItemId = item.Id;
            existItem.NodeId = item.Id;
            existItem.NodeType = (int)RMNodeLevel.AzureFileShareFile;
            existItem.TimeModified = item.Modified;
            existItem.ExternalId = item.RealId;
            existItem.AveSiteId = SelectedNode.ConnectionId.ToString();
            existItem.ScopeId = SelectedNode.ConnectionId;
            existItem.ContainerId = SelectedNode.ContainerId;

            var metaInfo = new RecordMetaInfo
            {
                FileSize = item.Size ?? 0,
                LastAccessTime = item.LastAccessTime
            };
            existItem.MetaInfo = JsonConvert.SerializeObject(metaInfo);

            return existItem;
        }
    }
}
