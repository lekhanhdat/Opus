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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using Microsoft.Exchange.WebServices.Data;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeTagController : IBackupController
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeExportController));
        private EXOConfiguration config = null;
        private Rule rule = null;
        private EXOExportBeforeArcInfo EXOExportBefArcInfo = null;

        internal ExchangeTagController(EXOConfiguration mConfig, Rule mRule, EXOExportBeforeArcInfo export)
        {
            this.config = mConfig;
            this.rule = mRule;
            this.EXOExportBefArcInfo = export;
        }
        public void Finish()
        {
            logger.Info("Tag action finished");
        }

        public void Process(EXOArchiveData node)
        {
            using (var performance = new PerformanceScope("ExchangeTagController.Process", "", true))
            {
                Item EXOItem = null;
                var status = JobDetailsStatus.None;
                string message = string.Empty;
                try
                {
                    if (EXOExportBefArcInfo != null && EXOExportBefArcInfo.EXOExport != null && EXOExportBefArcInfo.EXOExportPathGenerator != null)
                    {
                        ExchangeItemExport exoItemExport = new ExchangeItemExport(logger) { Configuration = config };
                        exoItemExport.EXOExportBeforeArcInfo = EXOExportBefArcInfo;
                        exoItemExport.VaultExport(node.ItemId, node, config.SubJobId, config.RuleName);
                    }
                    long itemSize = 0;
                    using (var performance0 = new PerformanceScope("ExchangeTagController.Item.Bind", "", true))
                    {
                        EXOItem = Item.Bind(config.service, new ItemId(node.ItemId)).GetAwaiter().GetResult();
                    }
                    itemSize = EXOItem.Size;
                    Guid labelId = Guid.Empty;
                    string ruleLabelName = rule.TagContentInfo.Where(t => t.Type == GCommon.Contract.StorageOptimization.Object.TagContentInfoType.RetentionLabel).First().Value;
                    if (TryGetLabelIdByName(ruleLabelName, out labelId))
                    {
                        try
                        {
                            TagLabel(EXOItem, labelId);
                            logger.Info(string.Format("Tag exchange item successful, item id : {0}.", node.ItemId));
                            status = JobDetailsStatus.Successful;
                        }
                        catch (Exception ex)
                        {
                            //config.ProgressDto.HasErrorNode = true;
                            status = JobDetailsStatus.Failed;
                            message = ex.Message;
                            logger.Info($"Tag exchange item failed,NodeFullPath:{node.FullPath}. item Subject : {EXOItem?.Id.ToString() ?? string.Empty}, reason : {ex.ToString()}.");
                            throw;
                        }
                    }
                    else
                    {
                        logger.Info($"Tag exchange item skip,CannotGetLabelByName:{ruleLabelName}. item id : {node.ItemId}.");
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
                    logger.Error($"Error occurred while tag item:{node.ItemId} Error:{ex.ToString()}");
                    throw;
                }
                finally
                {
                    EXOCommonUtil.AddDetail(EXOItem, node.FullPath, config.RuleName, string.Empty, status, "RM_EXODisposal_Action_Keep", message);
                }
            }
        }

        private bool TryGetLabelIdByName(string Name, out Guid labelId)
        {
            return config.RetentionLabel.TryGetValue(Name, out labelId);
        }

        private void TagLabel(Item item, Guid labelId)
        {
            using (var performance0 = new PerformanceScope("ExchangeTagController.TagLabel", "", true))
            {
                try
                {
                    item.PolicyTag = new PolicyTag();
                    item.PolicyTag.RetentionId = labelId;
                    item.PolicyTag.IsExplicit = true;
                    item.Update(ConflictResolutionMode.AlwaysOverwrite);
                }
                catch (Exception ex)
                {
                    logger.Error("Tag label failed.Exception:" + ex.ToString());
                    throw;
                }
            }
        }
    }
}
