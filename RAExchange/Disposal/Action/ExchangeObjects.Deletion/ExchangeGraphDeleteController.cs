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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAExchange.Disposal.Action;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using RAExportCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.RAExchange.Disposal.Action.ExchangeObjects.Deletion
{
    public class ExchangeGraphDeleteController : IBackupController
    {
        private readonly Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeGraphDeleteController));

        private IExplorerDao _explorerDao;
        protected IExplorerDao ExplorerDao
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

        private IRMEXOLabelDao _rMEXOLabelDao = PlatformWindsorManager.GetService<IRMEXOLabelDao>();

        private bool _exsiteHoldItem = false;

        private EXOConfiguration _configuration = null;

        private EXOExportBeforeArcInfo _eXOExportBefArcInfo = null;
        private IWorkplaceHoldDao WorkplaceHoldDao => PlatformWindsorManager.GetService<IWorkplaceHoldDao>();
        private static readonly ConcurrentDictionary<string, long> _workspaceReleaseTimeCache = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        public ExchangeGraphDeleteController(EXOConfiguration configuration, EXOExportBeforeArcInfo eXOExportBeforeArcInfo)
        {
            _configuration = configuration;

            _eXOExportBefArcInfo = eXOExportBeforeArcInfo;
            try
            {
                if (ExplorerDao.Exist(a => a.HoldStatus == true) || WorkplaceHoldDao.ExistWorkspaceHold().GetAwaiter().GetResult())
                {
                    logger.Info($"check exsit holdStatus == true success,exsit hold items");
                    _exsiteHoldItem = true;
                }
                else
                {
                    logger.Info($"check exsit holdStatus == true success,not exsit hold items");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"check exsit holdStatus == true failed,error:{ex}");
                _exsiteHoldItem = true;
            }
        }

        public void Finish()
        {
            logger.Info("Delete action finished");
        }

        public void Process(EXOArchiveData node)
        {
            //TODO Add folder level and other types here, also need to add report for multi process
            var status = JobDetailsStatus.Successful;
            string comment = string.Empty;
            IExchangeItem EXOItem = null;
            try
            {
                var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(_configuration.ExchangeNodeName);
                EXOItem = ExchangeFactoryProvider.Create(true).CreateItem(_configuration.MailboxId, node.ItemId, node.ParentFolderId, authObject);

                if (_eXOExportBefArcInfo != null && _eXOExportBefArcInfo.EXOExport != null && _eXOExportBefArcInfo.EXOExportPathGenerator != null)
                {
                    ExchangeItemExportV2 exoItemExport = new ExchangeItemExportV2(logger) { Configuration = _configuration };
                    exoItemExport.EXOExportBeforeArcInfo = _eXOExportBefArcInfo;
                    exoItemExport.VaultExport(node.ItemId, node, _configuration.SubJobId, _configuration.RuleName, EXOItem);
                }

                if (_exsiteHoldItem)
                {
                    if (CheckItemIsRecordsHold(node.ItemId))
                    {
                        logger.Info("Item is RecordsHold.Path:{0}.", node.ItemId);
                        status = JobDetailsStatus.Skipped;
                        comment = "RM_Job_SkipHoldEXOItem";
                        return;
                    }
                }

                CheckRetentionlabel(EXOItem, node);
                var report = GetDestructionReport(node, EXOItem);
                EXOItem.DeleteAsync(true).GetAwaiter().GetResult();
                AddToDestructionCache(report);
                UpdateExploreDB(node.ItemId, 2);
                logger.Info(string.Format("Delete exchange item successful, item id : {0}.", node.ItemId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in delete exchange item:{node.FullPath}.Message: {ex.ToString()}.");
                //config.ProgressDto.HasErrorNode = true;
                status = JobDetailsStatus.Failed;
                comment = ex.Message;
                throw;
            }
            finally
            {
                EXOCommonUtil.AddDetail(EXOItem, node.FullPath, _configuration.RuleName, string.Empty, status, "RM_EXODisposal_Action_Delete", comment);
            }
        }

        private void CheckRetentionlabel(IExchangeItem eXOItem, EXOArchiveData node)
        {
            try
            {
                string labelName = _configuration.ExoRetentionLabelCache.Where(e => e.Value == eXOItem.RetentionLabel).Select(e => e.Value).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(labelName)
                    && _rMEXOLabelDao.Exist(x => x.LabelName == labelName && x.Status == 1 && x.Type == 0))
                {
                    logger.Info("Current item is label file and Records remove label and delete.NodeId:{0}.", node.ItemId);

                    eXOItem.RemovePolicyTag();

                    logger.Info("Delete label  success.NodeId:{0}", node.ItemId);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete Retention label.NodeId:{0} error :{1}", node.ItemId, e.ToString());
            }
        }

        private DestructionReport GetDestructionReport(EXOArchiveData node, IExchangeItem message)
        {
            using (var performance = new PerformanceScope("ExchangeDeleteController.GetDestructionReport", "", true))
            {
                try
                {
                    DestructionReport destructionReport = new DestructionReport()
                    {
                        NodeId = node.ItemId,
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        RuleID = new Guid(_configuration.CurrentRule.Id),
                        SortTicks = Snowflake.Instance().GetTicks().ToString(),
                        JsonMeta = GetJsonMeta(node, message),
                        FullPath = node.FullPath,
                        ActionType = (int)ActionType.ArchiverAndRemove
                    };
                    return destructionReport;
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while generating destruction report. ItemId:{node.ItemId} Error:{e.ToString()}");
                    return null;
                }
            }
        }

        private void AddToDestructionCache(DestructionReport destructionReport)
        {
            if (destructionReport == null)
            {
                return;
            }
            DestructionFactory.GetInstance(_configuration.mailboxStringId, _configuration.SubJobId).InsertValueToDB(new List<DestructionReport>() { destructionReport });
        }

        private string GetJsonMeta(EXOArchiveData node, IExchangeItem item)
        {
            ArchiverExchangeOnlineDto mailboxDto = new ArchiverExchangeOnlineDto()
            {
                Title = item.ItemName,
                TermValue = new Guid(node.TermId),
                CreatedTime = item.Created.Ticks,
                CDLastModifiedTime = item.Modified.Ticks,
            };

            return JsonConvert.SerializeObject(mailboxDto);
        }

        private void UpdateExploreDB(string nodeID, int updateStatus)
        {
            using (var performance = new PerformanceScope("ExchangeGraphDeleteController.UpdateExploreDB", "", true))
            {
                Guid recordID = RAExchange.Common.IDGenerator.GetRecordId(_configuration.ExchangeNodeName, nodeID);
                //if (config.isRAJob && config.explorerDao != null)
                {
                    try
                    {
                        Record record = ExplorerDao.ReadById(new Guid(_configuration.MailboxRealGuid), recordID);
                        if (record != null)
                        {
                            if (_configuration.CurrentRule.IsManualApproval)
                            {
                                ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(new Guid(_configuration.MailboxRealGuid), recordID, updateStatus);
                                _configuration.AddHistory(record);
                            }
                            else
                            {
                                ExplorerDao.UpdateRecordStatusAndDestroyedTime(new Guid(_configuration.MailboxRealGuid), recordID, updateStatus);
                            }
                            logger.Info("Update Record Status successful By MailboxRealGuid.");
                        }
                        else
                        {
                            logger.Info("Current object:{0} doesn't exist in explore.", recordID);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// EXO老数据兼容，先用AOS真实MailboxID，再用DAOTreeNodeID
        /// </summary>
        private bool CheckItemIsRecordsHold(string itemId)
        {
            bool isRecordsHold = false;
            //Explore Hold文件默认不处理.
            Guid recordID = RAExchange.Common.IDGenerator.GetRecordId(_configuration.ExchangeNodeName, itemId);
            Record record = ExplorerDao.ReadById(new Guid(_configuration.MailboxRealGuid), recordID);
            if (record != null && record.HoldStatus == true)
            {
                isRecordsHold = true;
            }
            if (record != null && !string.IsNullOrWhiteSpace(record.AveSiteId))
            {
                long currentTicks = DateTime.UtcNow.Ticks;
                if (!_workspaceReleaseTimeCache.TryGetValue(record.AveSiteId, out long workspaceReleaseTime) || workspaceReleaseTime <= currentTicks)
                {
                    workspaceReleaseTime = WorkplaceHoldDao.GetReleaseTimeByAveSiteIdAsync(record.AveSiteId).GetAwaiter().GetResult();
                    if (workspaceReleaseTime > currentTicks)
                    {
                        _workspaceReleaseTimeCache[record.AveSiteId] = workspaceReleaseTime;
                    }
                    else
                    {
                        _workspaceReleaseTimeCache.TryRemove(record.AveSiteId, out _);
                    }
                }

                logger.Info("Current workspaceId {0} workspace hold:{1}.", record.AveSiteId, workspaceReleaseTime);

                if (workspaceReleaseTime > currentTicks)
                {
                    isRecordsHold = true;
                }
            }
            return isRecordsHold;
        }
    }
}
