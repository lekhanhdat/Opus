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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using RAExportCommon;
using System;

using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeItemExportV2 : EXOObjectBackup
    {
        public ExchangeItemExportV2(AvePoint.RA.Contract.Services.IRALogger log)
        {
            mLog = log;
        }

        public override int Backup(EXOArchiveData entity, string ruleName, string subJobid, int ruleLevel, string mediaName)
        {
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                //MailboxDto itemDto = ExchangeConvert.ConvertToMetameta(entity);
                try
                {
                    using (var performance = new PerformanceScope("ExchangeMailboxExport.ItemVaultExport", "", true))
                    {
                        this.VaultExport(entity.ItemId, entity, subJobid, ruleName);
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Export Exchange Item Metadata Error: {0}", e.ToString());
                    throw;
                }
                finally
                {
                }
            }
            catch (Exception e)
            {
                status = JobDetailsStatus.Failed;
                mLog.Error("Export Exchange Item Error: {0}", e.ToString());
                //Configuration.ProgressDto.HasErrorNode = true;
                throw;
            }
            finally
            {
                string tail = string.Empty;
                //fileatrrinfo.ToString();
                mLog.Info("Send backup tail to media: {0}", tail);
                //fileSender.BackupTail(tail, status == JobDetailsStatus.Succeed);
            }
            return 0;
        }

        internal void VaultExport(string mailItemId, EXOArchiveData entity, string subJobId, string ruleName, IExchangeItem EXOItem = null)
        {
            ExportStatus vaultState = null;
            JobDetailsStatus status = JobDetailsStatus.None;
            try
            {
                EXOItemPathGeneratorInfoV2 EXOItemPathInfo = null;
                EXOExportInfoV2 exportInfo = null;

                // For Backup Action
                if (EXOItem is null)
                {
                    using (var performance = new PerformanceScope("ExchangeMailboxExport.Item.Bind", "", true))
                    {
                        var factory = ExchangeFactoryProvider.Create(Configuration.IsSupportGraphAPI);
                        var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(this.Configuration.ExchangeNodeName);
                        EXOItem = factory.CreateItem(this.Configuration.MailboxId, entity.ItemId, entity.ParentFolderId, authObject);
                    }
                }

                EXOItemPathInfo = new EXOItemPathGeneratorInfoV2()
                {
                    EXOItem = EXOItem,
                    JobId = this.Configuration.SubJobId,
                    PhysicalDeviceDtoId = string.Empty,
                    service = Configuration.service,
                    MailAddress = Configuration.ExchangeNodeName,
                    MailFullPath = entity.FullPath,
                    ParentFolderName = EXOItem.ParentFolderDisplayName,
                    Credentials = Configuration.Credentials,
                };
                using (var performance = new PerformanceScope("ExchangeMailboxExport.GenerateEXOItemExportInfo", "", true))
                {
                    exportInfo = this.EXOExportBeforeArcInfo.EXOExportPathGenerator.GenerateEXOItemExportInfoV2(EXOItemPathInfo);
                    exportInfo.ContentFilePath = EnsureUniqueFilePath(exportInfo.FolderPath, exportInfo.ContentFilePath);
                }
                using (var performance = new PerformanceScope("ExchangeMailboxExport.ExportEXOItem", "", true))
                {
                    exportInfo.DisposalClassString = Configuration.CurrentRule.DisposalClass;
                    vaultState = this.EXOExportBeforeArcInfo.EXOExport.ExportEXOItem(EXOItem, exportInfo);
                }
                mLog.Info("vaultState ExportSize is:{0},ID is:{1},Status is:{2}.", vaultState == null ? "00000" : vaultState.ExportSize.ToString(), entity.ItemId, vaultState == null ? ExportState.Failed.ToString() : vaultState.State.ToString());
                if (vaultState == null)
                {
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = "" };
                    status = JobDetailsStatus.Failed;

                    mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, entity.FullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError4EXO);
                }
                if (vaultState.State == ExportState.Failed)
                {
                    mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, entity.FullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError4EXO);
                }
                else if (vaultState.State == ExportState.Succeed)
                {
                    vaultState.ErrorMessage = string.Empty;
                    status = JobDetailsStatus.Successful;
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Error in Export EXOItem:" + ex.ToString());
                vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                status = JobDetailsStatus.Failed;
                throw;
            }
            finally
            {
                EXOCommonUtil.AddDetail(EXOItem, entity.FullPath, ruleName, string.Empty, status, "RM_EXODisposal_Action_Export", vaultState?.ErrorMessage);
            }
        }
    }
}
