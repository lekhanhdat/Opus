using Aspose.Pdf.Operators;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using RACloudFS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FSActions
{
    public class FSMyHubDashboardProcessor
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(FSMyHubDashboardProcessor));
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private string JobId = string.Empty;
        private JobType mJobType;
        private IExplorerDao explorerDao;
        private JobContext jobContext = null;
        protected static readonly IExplorerDao ExplorerDao = new ExplorerDao(true);
        private const int CosmosPageSize = 5000;
        private const int SqlBatchSize = 500;
        private  IFSMyHubDashboardDao MyHubDashboardDao => new RMFSMyHubDashboardDao();
        private readonly FileSystemMyhubSelectedNodeDto selectedNode;
        private Guid ScopeId = Guid.Empty;
        public FSMyHubDashboardProcessor(string jobId, JobRunBy jobRunBy, string extension)
        {
            JobId = jobId;
            mJobType = JobType.FSMyHubDashboard;
            explorerDao = new ExplorerDao();
            jobContext = JobContext.GetInstance(jobId, JobType.FSMyHubDashboard);
            jobContext.ReportManager.StartUpdateJobProgress();
            selectedNode = SerializerHelper.DeserializeByDataContractSerializer<FileSystemMyhubSelectedNodeDto>(extension) ;
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                using (CheckJobStopScope jobStopScope = new CheckJobStopScope())
                {
                    mLog.Info($"Starting FS MyHub Dashboard job with JobId: {JobId}");
                    await CollectAndAggregateDashboardDataAsync(selectedNode);
                    SendDetail(JobDetailsStatus.Successful, string.Empty);

                    mLog.Info($"Completed FS MyHub Dashboard job with JobId: {JobId}");
                }
            }
            catch (Exception ex)
            {
                if (ex is AvePoint.RA.Contract.Exceptions.JobStopException || ex is AvePoint.RA.Contract.Global.Exceptions.JobStopException)
                {
                    mLog.Error($"FS MyHub Dashboard job was stopped by user.");
                    SendDetail(JobDetailsStatus.Successful, $"FS MyHub Dashboard job stopped");
                    jobContext.JobHasStopped = true;
                }
                else
                {
                    jobContext.HasErrorNode = true;
                    mLog.Error($"Error occurred while running FS MyHub Dashboard job with JobId: {JobId}. Exception: {ex.Message}");
                    SendDetail(JobDetailsStatus.Failed, $"FS MyHub Dashboard job failed. Exception: {ex.Message}");
                }
            }
            finally
            {
                jobContext.Finish();
            }
        }

        private async System.Threading.Tasks.Task CollectAndAggregateDashboardDataAsync(FileSystemMyhubSelectedNodeDto selectedNode)
        {
            try
            {
                CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                mLog.Info($"Collecting and aggregating dashboard data for FS MyHub Dashboard NodeId: {selectedNode.NodeId}");
                var aggregator = new FSDashboardAggregator();
                var recordsInSelectedNode = await ProcessSelectedNodeAsync(selectedNode, aggregator);
            }
            catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
            {
                mLog.Error($"FS MyHub Dashboard job was stopped by user.");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while collecting and aggregating dashboard data. Exception: {ex.Message}");
            }
            
        }


        private async System.Threading.Tasks.Task<long> ProcessSelectedNodeAsync(FileSystemMyhubSelectedNodeDto selectedNode, FSDashboardAggregator aggregator)
        {
            aggregator.Reset();
            aggregator.SetRootPath(selectedNode.FullPath);
            long recordCount = 0;
            string continuationToken = string.Empty;
            try
            {
                var queryDefinition = BuildQueryDefinition(selectedNode.FullPath);
                using(new PerformanceScope($"FSMyHubDashboardProcessor.ProcessSelectedNodeAsync-{selectedNode.NodeId}"))
                {
                    do
                    {
                        var queryResult = ExplorerDao.QueryPageBySql(queryDefinition, CosmosPageSize, continuationToken);
                        continuationToken = queryResult.Item2;
                        jobContext.ReportManager.IncreaseBase(queryResult.Item1.Count());
                        foreach (var record in queryResult.Item1)
                        {

                            CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                            if (ScopeId == Guid.Empty )
                            {
                                ScopeId = record.ScopeId;
                            }
                            aggregator.Accumulate(record);
                            jobContext.ReportManager.Increase();
                            recordCount++;
                        }
                    } while (!string.IsNullOrEmpty(continuationToken));
                }
            }
            catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
            {
                mLog.Error($"FS MyHub Dashboard job was stopped by user.");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while processing selected node: {selectedNode.NodeId}. Exception: {ex.Message}");
            }
            await PersistAggregationsAsync(selectedNode.NodeId, aggregator);
            return recordCount;
        }

        private static QueryDefinition BuildQueryDefinition(string path)
        {
            var sqlText = @"
                SELECT c.aveSiteId, c.scopeId, c.parentId, c.dirPath, c.createdBy, 
                       c.extensionForFile, c.jpmcFileSize, c.termId, 
                       c.classCode, c.metaInfo, c.nodeType, c.recordStatus, c.destroyedTime
                FROM c 
                WHERE c.sourceFlag = @sourceFlag 
                  AND (c.nodeType = @nodeType OR c.nodeType = @folderNodeType)
                  AND STARTSWITH(c.dirPath, @dirPath )
                  AND IS_DEFINED(c.dirPath)
                ORDER BY c.dirPath DESC";

            return new QueryDefinition(sqlText)
                .WithParameter("@sourceFlag", 2)
                .WithParameter("@nodeType", 2200) // file lelvel
                .WithParameter("@folderNodeType", 2100) // folder level
                .WithParameter("@dirPath", path);
        }

        private async System.Threading.Tasks.Task PersistAggregationsAsync(Guid nodeId ,FSDashboardAggregator aggregator)
        {
            try
            {
                mLog.Info("Persisting aggregated dashboard data for FS MyHub Dashboard.");
                var aggregatedData = aggregator.BuildResults();
                var utcNow = DateTime.UtcNow;
                var batch = new List<RMFSMyHubDashboard>();
                foreach (var kvp in aggregatedData)
                {
                    CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                    var NodeId = $"{kvp.Key}".ToLowerInvariant().ToMd5();
                    mLog.Info($"Generated NodeId: {NodeId} for persistence.");
                    batch.Add(new RMFSMyHubDashboard
                    {
                        NodeId = NodeId,
                        FullPath = kvp.Key,
                        ScopeId = ScopeId,
                        MetaData = JsonConvert.SerializeObject(kvp.Value),
                    });
                    if (batch.Count >= SqlBatchSize)
                    {
                        await FlushBatchAsync(batch);
                        batch.Clear();
                    }
                }
                if (batch.Count > 0)
                {
                    await FlushBatchAsync(batch);
                }
                mLog.Info("Finished persisting aggregated dashboard data.");

            }
            catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
            {
                mLog.Error($"FS MyHub Dashboard job was stopped by user.");
                throw;
            }
            catch (Exception ex) 
            {
                mLog.Error($"Error occurred while persisting aggregated dashboard data. Exception: {ex.Message}");
            }

        }

        private async System.Threading.Tasks.Task FlushBatchAsync(List<RMFSMyHubDashboard> batch)
        {
            if (batch.Count == 0)
            {
                return;
            }
            try
            {
                CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                await MyHubDashboardDao.AddOrUpdateBatchAsync(batch);
                mLog.Info($"Successfully flushed a batch of {batch.Count} aggregated records to the database.");
            }
            catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
            {
                mLog.Error($"FS MyHub Dashboard job was stopped by user.");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while flushing a batch of aggregated records to the database. Exception: {ex.Message}");
            }
        }

        private void SendDetail(JobDetailsStatus status, string comment = "")
        {
            jobContext.ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
            {
                Action = "RM_JS_JM_JobType_FSMyHubDashboard",
                Status = status,
                Comment = comment,
            });
        }
    }
}
