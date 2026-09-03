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
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.SharePoint.ScanLocalNode.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.I18N.Core;

namespace RAFileSystem.SharePoint.ScanLocalNode
{
    public class ScanLocalNode : IScheduleJobWorker
    {

        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(ScanLocalNode));

        private static readonly int TransferDataCount = ExternalUtil.TransferDataCount;

        private string JobId => JobContext.Current.JobId;

        private bool HasSucceed { get; set; } = false;

        private bool HasFailed { get; set; } = false;

        private string JobCompleteMessage { get; set; } = string.Empty;

        private static readonly IReportService<JMJobDetails> JobDetailManager = JobContext.Current.JobDetailManager.Create();

        private static readonly IProgressService ProcessService = JobContext.Current.mProgressManager.Create();

        public void Bind(string msg)
        {

        }

        public void Run()
        {
            Logger.Info("Start run sharepoint on-prem scan local node job.");
            try
            {
                if (!CheckFarmIsAvailable())
                {
                    HasFailed = true;
                    JobCompleteMessage = "Can't find available farm.";
                    return;
                }
                ScanWebApplications();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run sharepoint on-prem scan local node job. Error: {e}");
                HasFailed = true;
                JobCompleteMessage = e.Message;
            }
            finally
            {
                JobCompleteAction();
            }
        }

        private bool CheckFarmIsAvailable()
        {
            return !string.IsNullOrEmpty(TreeBrowser.FarmId);
        }

        private void JobCompleteAction()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while cleaning up job detail cache. Error: {e}");
                HasFailed = true;
                JobCompleteMessage = e.Message;
            }

            var jobStatus = JobStatus.Finished;
            if (HasSucceed && HasFailed)
            {
                jobStatus = JobStatus.FinishWithException;
            }
            else if (HasFailed && !HasSucceed)
            {
                jobStatus = JobStatus.Failed;
            }
            else if (!HasFailed && !HasSucceed)
            {
                JobCompleteMessage = "RM_JM_JS_NoItemChanged";
            }
            ProcessService.IncreaseToComplete();
            JobContext.Current.JobSummaryService.NotifyManager((int)jobStatus, JobId, true, JobCompleteMessage);
        }

        private void ScanWebApplications()
        {
            try
            {
                Logger.Info("Start scan web applications.");
                var hasFailed = false;
                var spWebAppNodes = TreeBrowser.GetWebApplications(ref hasFailed);
                if(!HasFailed)
                {
                    HasFailed = hasFailed;
                }
                var recordsWebAppNodes = GetRecordsLocalNodes(TreeBrowser.FarmId);
                UpdateWebAppIdToRecordId(spWebAppNodes, recordsWebAppNodes);
                ExcuteNodes(spWebAppNodes, recordsWebAppNodes);
                foreach (var webAppNode in spWebAppNodes)
                {
                    ScanSiteCollections(webAppNode);
                }
                Logger.Info("Finish scan web applications.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while scan web application. Error: {e}");
                HasFailed = true;
            }
        }

        private void UpdateWebAppIdToRecordId(HashSet<OnPremiseSPLocalNode> spWebAppNodes, HashSet<OnPremiseSPLocalNode> recordWebAppNodes)
        {
            var recordNodesMapping = new Dictionary<string, string>();

            foreach (var item in recordWebAppNodes)
            {
                recordNodesMapping.Add(item.ObjectId, item.Id);
            }
            foreach (var webApp in spWebAppNodes)
            {
                if (recordNodesMapping.TryGetValue(webApp.ObjectId, out var recordId))
                {
                    webApp.Id = recordId;
                }
            }
        }


        private void ScanSiteCollections(OnPremiseSPLocalNode webAppNode)
        {
            try
            {
                Logger.Info($"Start scan site collection under web application: [{webAppNode.ObjectId}] - [{webAppNode.Url.LogBase64()}]");
                var hasFailed = false;
                var spSiteNodes = TreeBrowser.GetSiteCollections(webAppNode, ref hasFailed);
                if(!HasFailed)
                {
                    HasFailed = hasFailed;
                }
                var recordsSiteNodes = GetRecordsLocalNodes(webAppNode.Id);
                ExcuteNodes(spSiteNodes, recordsSiteNodes);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while scan site collection. Error: {e}");
                HasFailed = true;
            }
        }

        private void ExcuteNodes(HashSet<OnPremiseSPLocalNode> spNodes, HashSet<OnPremiseSPLocalNode> recordNodes)
        {
            Logger.Info($"Start excute node. Count: [{spNodes.Count}].");

            ProcessService.IncreaseBase(spNodes.Count);

            var actionDic = CompareRecordAndSPNodes(spNodes, recordNodes);
            var successCount = 0;
            foreach(var action in actionDic)
            {
                if(action.Value.Count == 0)
                {
                    Logger.Info($"No record nodes need [{action.Key}]");
                    continue;
                }
                successCount += ExcuteNodesWithAction(action.Key, action.Value);
            }

            ProcessService.Increase(spNodes.Count);

            Logger.Info($"Finish excute node. Successful count: [{successCount}].");
        }

        private int ExcuteNodesWithAction(RMScanLocalNodeAction action, List<OnPremiseSPLocalNode> nodes)
        {
            var successCount = 0;
            var excuteCount = 0;
            var nodesCount = nodes.Count;
            Func<List<OnPremiseSPLocalNode>, OnPremSPScanNodeResult> actionFunc = null;
            try
            {
                Logger.Info($"Start excute node with [{action}], Node count: [{nodesCount}].");

                if (action == RMScanLocalNodeAction.Add)
                {
                    actionFunc = HybridApiClient.Instance.BatchAddRecordsLocalNodes;
                }
                else if (action == RMScanLocalNodeAction.Update)
                {
                    actionFunc = HybridApiClient.Instance.BatchUpdateRecordsLocalNodes;
                }
                else if(action == RMScanLocalNodeAction.Delete)
                {
                    actionFunc = HybridApiClient.Instance.BatchDeleteRecordsLocalNodes;
                }
                
                if(actionFunc != null)
                {
                    for(var excuteNodes = nodes.Take(TransferDataCount).ToList(); 
                        excuteCount < nodesCount;
                        excuteNodes = nodes.Skip(excuteCount).Take(TransferDataCount).ToList())
                    {
                        try
                        {
                            var count = excuteNodes.Count();
                            excuteCount += count;
                            Logger.Info($"Needs to be process this time node count: [{count}].");
                            var scanResult = actionFunc(excuteNodes);
                            if (scanResult == null || !scanResult.Success)
                            {
                                var errorMessage = scanResult?.ErrorMessage;
                                throw new Exception(errorMessage);
                            }

                            successCount += count;
                            Logger.Info($"Total processed node count: [{count}].");
                            AddJobDetail(action, excuteNodes, JobDetailsStatus.Successful);
                            HasSucceed = true;
                        }
                        catch(Exception e)
                        {
                            Logger.Error($"An error occurred while excute node with [{action}]. Error: {e}");
                            AddJobDetail(action, excuteNodes, JobDetailsStatus.Failed, e.Message);
                            HasFailed = true;
                        }
                    }
                }
                Logger.Info($"Finish excute node with [{action}], Successful count: [{successCount}].");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while excute node with [{action}]. Error: {e}");
                AddJobDetail(action, nodes, JobDetailsStatus.Failed, e.Message);
                HasFailed = true;
            }

            return successCount;
        }

        private HashSet<OnPremiseSPLocalNode> GetRecordsLocalNodes(string parentId)
        {
            var res = new HashSet<OnPremiseSPLocalNode>();
            try
            {
                Logger.Info($"Start get records local nodes under parent: {parentId}.");
                var pageIndex = 1;
                var count = 0;
                do
                {
                    var nodes = HybridApiClient.Instance.GetRecordsLocalNodes(pageIndex, TransferDataCount, parentId);
                    count = nodes.Count;
                    res.UnionWith(nodes);
                    pageIndex++;
                }
                while (count == TransferDataCount);
                Logger.Info($"Finish get records local nodes. Count: [{res.Count}].");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get records local nodes under parent: [{parentId}]. Error: {e}");
                HasFailed = true;
                throw;
            }
            return res;
        }

        private Dictionary<RMScanLocalNodeAction, List<OnPremiseSPLocalNode>> CompareRecordAndSPNodes(HashSet<OnPremiseSPLocalNode> spNodes, HashSet<OnPremiseSPLocalNode> recordNodes)
        {

            if (recordNodes == null)
            {
                throw new ArgumentNullException("recordNodes");
            }

            if (spNodes == null)
            {
                throw new ArgumentNullException("spNodes");
            }

            var res = new Dictionary<RMScanLocalNodeAction, List<OnPremiseSPLocalNode>>()
            {
                {RMScanLocalNodeAction.Add, new List<OnPremiseSPLocalNode>() },
                {RMScanLocalNodeAction.Update, new List<OnPremiseSPLocalNode>() },
                {RMScanLocalNodeAction.Delete, new List<OnPremiseSPLocalNode>() },
            };

            var deleteNodes = recordNodes.Except(spNodes);
            res[RMScanLocalNodeAction.Delete].AddRange(deleteNodes);
            Logger.Info($"Waiting delete node count: [{deleteNodes.Count()}].");

            var addNodes = spNodes.Except(recordNodes);
            res[RMScanLocalNodeAction.Add].AddRange(addNodes);
            Logger.Info($"Waiting add node count: [{addNodes.Count()}].");

            var recordNoDecideUpdateNodes = recordNodes.Intersect(spNodes);
            var spNoDecideUpdateNodes = spNodes.Intersect(recordNoDecideUpdateNodes);
            var updateNodes = CompareUpdateNodeProperties(recordNoDecideUpdateNodes, spNoDecideUpdateNodes);
            res[RMScanLocalNodeAction.Update].AddRange(updateNodes);
            Logger.Info($"Waiting update node count: [{updateNodes.Count()}]");

            return res;
        }

        private IEnumerable<OnPremiseSPLocalNode> CompareUpdateNodeProperties(IEnumerable<OnPremiseSPLocalNode> recordNodes, IEnumerable<OnPremiseSPLocalNode> spNodes)
        {
            return from recordNode in recordNodes
                   join spNode in spNodes
                   on recordNode.ObjectId equals spNode.ObjectId
                   where recordNode.Name != spNode.Name ||
                         recordNode.Url != spNode.Url ||
                         recordNode.Description != spNode.Description
                   select new OnPremiseSPLocalNode
                   {
                       Id = recordNode.Id,
                       ObjectId = spNode.ObjectId,
                       ParentId = spNode.ParentId,
                       FarmId = spNode.FarmId,
                       Url = spNode.Url,
                       Name = spNode.Name,
                       Description = spNode.Description,
                       NodeLevel = spNode.NodeLevel,
                       SiteCollectionType = spNode.SiteCollectionType,
                       SPVersion = spNode.SPVersion,
                       CreateTime = recordNode.CreateTime,
                       ModifiedDate = spNode.ModifiedDate
                   };
        }

        private string GetNodeTypeString(int nodeLevel)
        {
            switch (nodeLevel)
            {
                case (int)NodeLevel.WebApplication:
                    return "RM_JS_Rule_ObjectLevel_WebApplication";
                case (int)NodeLevel.SiteCollection:
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                default:
                    return string.Empty;
            }
        }

        private string GetActionTypeString(RMScanLocalNodeAction actionType)
        {
            switch (actionType)
            {
                case RMScanLocalNodeAction.Add:
                    return "RM_JS_SRN_Action_Add";
                case RMScanLocalNodeAction.Delete:
                    return "RM_JS_SRN_Action_Delete";
                case RMScanLocalNodeAction.Update:
                    return "RM_JS_SRN_Action_Update";
                default:
                    return string.Empty;
            }
        }

        private void AddJobDetail(RMScanLocalNodeAction action, List<OnPremiseSPLocalNode> nodes, JobDetailsStatus status, string exceptionComment = null)
        {
            Logger.Info($"Start add job detail. Action: [{action}], Count: [{nodes.Count}], Status: [{status}].");
            nodes.ForEach(item =>
            {
                JobDetailManager.Commit(new JMScanLocalNodesJobDetails
                {
                    ObjectName = item.Name,
                    FullPath = item.Url,
                    ItemType = GetNodeTypeString(item.NodeLevel),
                    Action = GetActionTypeString(action),
                    Status = status,
                    Comment = exceptionComment,
                    AgentName = OSInformation.HostName
                });
            });
            Logger.Info("Finish add job Detail.");
        }

        enum RMScanLocalNodeAction
        {
            Add = 0,
            Update = 1,
            Delete = 2
        }
    }
}
