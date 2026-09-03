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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using RAExportCommon;
using System;
using System.Linq;

namespace AvePoint.RA.RAExchange.Disposal.Action.ExchangeObjects.Tag
{
    public class ExchangeGraphTagController : IBackupController
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(ExchangeExportController));
        private EXOConfiguration _configuration = null;
        private Rule _rule = null;
        private EXOExportBeforeArcInfo _eXOExportBefArcInfo = null;

        public ExchangeGraphTagController(EXOConfiguration mConfig, Rule mRule, EXOExportBeforeArcInfo export)
        {
            this._configuration = mConfig;
            this._rule = mRule;
            this._eXOExportBefArcInfo = export;
        }
        public void Finish()
        {
            _logger.Info("Tag action finished");
        }

        public void Process(EXOArchiveData node)
        {
            using (var performance = new PerformanceScope("ExchangeGraphTagController.Process", "", true))
            {
                IExchangeItem EXOItem = null;
                var status = JobDetailsStatus.None;
                string message = string.Empty;
                try
                {
                    if (_eXOExportBefArcInfo != null && _eXOExportBefArcInfo.EXOExport != null && _eXOExportBefArcInfo.EXOExportPathGenerator != null)
                    {
                        ExchangeItemExportV2 exoItemExport = new ExchangeItemExportV2(_logger) { Configuration = _configuration };
                        exoItemExport.EXOExportBeforeArcInfo = _eXOExportBefArcInfo;
                        exoItemExport.VaultExport(node.ItemId, node, _configuration.SubJobId, _configuration.RuleName);
                    }
                    using (var performance0 = new PerformanceScope("ExchangeGraphTagController.Item.Bind", "", true))
                    {

                        var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(_configuration.ExchangeNodeName);
                        EXOItem = ExchangeFactoryProvider.Create(true).CreateItem(_configuration.MailboxId, node.ItemId, node.ParentFolderId, authObject);
                    }
                    Guid labelId = Guid.Empty;
                    string ruleLabelName = _rule.TagContentInfo.Where(t => t.Type == TagContentInfoType.RetentionLabel).First().Value;
                    if (TryGetLabelIdByName(ruleLabelName, out labelId))
                    {
                        try
                        {
                            TagLabel(EXOItem, labelId);
                            _logger.Info(string.Format("Tag exchange item successful, item id : {0}.", node.ItemId));
                            status = JobDetailsStatus.Successful;
                        }
                        catch (Exception ex)
                        {
                            status = JobDetailsStatus.Failed;
                            message = ex.Message;
                            _logger.Info($"Tag exchange item failed,NodeFullPath:{node.FullPath}. item Subject : {EXOItem?.ItemId.ToString() ?? string.Empty}, reason : {ex.ToString()}.");
                            throw;
                        }
                    }
                    else
                    {
                        _logger.Info($"Tag exchange item skip,CannotGetLabelByName:{ruleLabelName}. item id : {node.ItemId}.");
                        message = "RM_JM_Details_CannotGetLabelByName";
                        status = JobDetailsStatus.Failed;
                        throw new Exception("RM_JM_Details_CannotGetLabelByName");
                        //config.ProgressDto.HasErrorNode = true;
                    }
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = ex.Message;
                    }
                    status = JobDetailsStatus.Failed;
                    _logger.Error($"Error occurred while tag item:{node.ItemId} Error:{ex.ToString()}");
                    throw;
                }
                finally
                {
                    EXOCommonUtil.AddDetail(EXOItem, node.FullPath, _configuration.RuleName, string.Empty, status, "RM_EXODisposal_Action_Keep", message);
                }
            }

        }

        private bool TryGetLabelIdByName(string Name, out Guid labelId)
        {
            return _configuration.RetentionLabel.TryGetValue(Name, out labelId);
        }

        private void TagLabel(IExchangeItem item, Guid labelId)
        {
            using (var performance0 = new PerformanceScope("ExchangeGraphTagController.TagLabel", "", true))
            {
                try
                {
                    item.SetRetentionLabelAsync(labelId);
                }
                catch (Exception ex)
                {
                    _logger.Error("Tag label failed.Exception:" + ex.ToString());
                    throw;
                }
            }
        }
    }
}
