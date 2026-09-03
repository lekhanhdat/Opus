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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using ExchangeBackupUtility;
using ExchangeUtility;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using RAExportCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeDeleteController : IBackupController
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeExportController));
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
        private bool ExsiteHoldItem = false;
        private IRMEXOLabelDao mRMEXOLabelDao;
        public IRMEXOLabelDao RMEXOLabelDao
        {
            get
            {
                if (mRMEXOLabelDao == null)
                {
                    mRMEXOLabelDao = (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao));
                }
                return mRMEXOLabelDao;
            }
        }

        private EXOConfiguration config = null;
        private ExchangeDeletionUtil deletionUtil = null;
        private EXOExportBeforeArcInfo EXOExportBefArcInfo = null;

        private IWorkplaceHoldDao WorkplaceHoldDao => PlatformWindsorManager.GetService<IWorkplaceHoldDao>();
        private static readonly ConcurrentDictionary<string, long> _workspaceReleaseTimeCache = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        public ExchangeDeleteController(EXOConfiguration mConfig, EXOExportBeforeArcInfo EXOExportBefArcInfo)
        {
            config = mConfig;
            deletionUtil = new ExchangeDeletionUtil();
            this.EXOExportBefArcInfo = EXOExportBefArcInfo;
            try
            {
                if (ExplorerDao.Exist(a => a.HoldStatus == true) || WorkplaceHoldDao.ExistWorkspaceHold().GetAwaiter().GetResult())
                {
                    logger.Info($"check exsit holdStatus == true success,exsit hold items");
                    ExsiteHoldItem = true;
                }
                else
                {
                    logger.Info($"check exsit holdStatus == true success,not exsit hold items");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"check exsit holdStatus == true failed,error:{ex}");
                ExsiteHoldItem = true;
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
            Item EXOItem = null;
            try
            {
                if (EXOExportBefArcInfo != null && EXOExportBefArcInfo.EXOExport != null && EXOExportBefArcInfo.EXOExportPathGenerator != null)
                {
                    ExchangeItemExport exoItemExport = new ExchangeItemExport(logger) { Configuration = config };
                    exoItemExport.EXOExportBeforeArcInfo = EXOExportBefArcInfo;
                    exoItemExport.VaultExport(node.ItemId, node, config.SubJobId, config.RuleName);
                }
                EXOItem = Item.Bind(config.service, new ItemId(node.ItemId)).GetAwaiter().GetResult();
                if (ExsiteHoldItem)
                {
                    if (CheckItemIsRecordsHold(node.ItemId))
                    {
                        logger.Info("Item is RecordsHold.Path:{0}.", node.ItemId);
                        status =  JobDetailsStatus.Skipped;
                        comment = "RM_Job_SkipHoldEXOItem";
                        return;
                    }
                }
                long itemSize = 0;
                itemSize = EXOItem.Size;

                CheckRetentionlabel(EXOItem, node);
                var report = GetDestructionReport(node, EXOItem);
                deletionUtil.DeleteExchangeItem(EXOItem);
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
                EXOCommonUtil.AddDetail(EXOItem, node.FullPath, config.RuleName, string.Empty, status, "RM_EXODisposal_Action_Delete", comment);
            }
        }

        private DestructionReport GetDestructionReport(EXOArchiveData node, Item EXOItem)
        {
            using (var performance = new PerformanceScope("ExchangeDeleteController.GetDestructionReport", "", true))
            {
                try
                {
                    DestructionReport destructionReport = new DestructionReport()
                    {
                        NodeId = node.ItemId,
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        RuleID = new Guid(config.CurrentRule.Id),
                        SortTicks = Snowflake.Instance().GetTicks().ToString(),
                        JsonMeta = GetJsonMeta(node, EXOItem),
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
            DestructionFactory.GetInstance(config.mailboxStringId, config.SubJobId).InsertValueToDB(new List<DestructionReport>() { destructionReport });
        }

        private string GetJsonMeta(EXOArchiveData node, Item EXOItem)
        {
            ArchiverExchangeOnlineDto mailboxDto = new ArchiverExchangeOnlineDto()
            {
                Title = GetItemName(EXOItem),
                TermValue = new Guid(node.TermId),
                CreatedTime = EXOItem.DateTimeCreated.Ticks,
                CDLastModifiedTime = EXOItem.LastModifiedTime.Ticks,
            };

            return JsonConvert.SerializeObject(mailboxDto);
        }

        private string GetItemName(Item item)
        {
            var itemName = item.Subject;
            if (string.IsNullOrEmpty(itemName))     //SAAS-10111
            {
                var contact = item as Contact;
                if (contact != null)
                {
                    Microsoft.Exchange.WebServices.Data.EmailAddress address;
                    if (contact.EmailAddresses.TryGetValue(EmailAddressKey.EmailAddress1, out address))
                    {
                        itemName = address.Address;
                    }
                }
            }
            return itemName;
        }

        private void CheckRetentionlabel(Item exoItem, EXOArchiveData node)
        {
            try
            {
                Guid retentionId = exoItem.PolicyTag != null && exoItem.PolicyTag.RetentionId != Guid.Empty ? exoItem.PolicyTag.RetentionId : Guid.Empty;
                string labelName = config.ExoRetentionLabelCache.ContainsKey(retentionId) ? config.ExoRetentionLabelCache[retentionId] : string.Empty;
                if (!string.IsNullOrWhiteSpace(labelName)
                    && RMEXOLabelDao.Exist(x => x.LabelName == labelName && x.Status == 1 && x.Type == 0))
                {
                    logger.Info("Current item is label file and Records remove label and delete.NodeId:{0}.", node.ItemId);
                    exoItem.PolicyTag = null;
                    exoItem.Update(ConflictResolutionMode.AutoResolve);
                    logger.Info("Delete label  success.NodeId:{0}", node.ItemId);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete Retention label.NodeId:{0} error :{1}", node.ItemId, e.ToString());
            }
        }

        /// <summary>
        /// EXO老数据兼容，先用AOS真实MailboxID，再用DAOTreeNodeID
        /// </summary>
        private void UpdateExploreDB(string nodeID, int updateStatus)
        {
            using (var performance = new PerformanceScope("ExchangeDeleteController.UpdateExploreDB", "", true))
            {
                Guid recordID = AvePoint.RA.RAExchange.Common.IDGenerator.GetRecordId(config.ExchangeNodeName, nodeID);
                //if (config.isRAJob && config.explorerDao != null)
                {
                    try
                    {
                        Record record = ExplorerDao.ReadById(new Guid(config.MailboxRealGuid), recordID);
                        if (record != null)
                        {
                            if (config.CurrentRule.IsManualApproval)
                            {
                                ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(new Guid(config.MailboxRealGuid), recordID, updateStatus);
                                config.AddHistory(record);
                            }
                            else
                            {
                                ExplorerDao.UpdateRecordStatusAndDestroyedTime(new Guid(config.MailboxRealGuid), recordID, updateStatus);
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
            Guid recordID = AvePoint.RA.RAExchange.Common.IDGenerator.GetRecordId(config.ExchangeNodeName, itemId);
            Record record = ExplorerDao.ReadById(new Guid(config.MailboxRealGuid), recordID);
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

                if (workspaceReleaseTime > currentTicks)
                {
                    isRecordsHold = true;
                }
            }
            return isRecordsHold;
        }
    }
}
