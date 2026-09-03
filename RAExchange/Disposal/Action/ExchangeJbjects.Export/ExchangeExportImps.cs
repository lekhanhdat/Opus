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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using ExchangeUtility;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeMailboxExport : EXOObjectBackup
    {
        public ExchangeMailboxExport(AvePoint.RA.Contract.Services.IRALogger log)
        {
            mLog = log;
        }
        public override int Backup(EXOArchiveData entity, string ruleName, string subJobid, int ruleLevel, string mediaName)
        {
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                //MailboxDto mailboxDto = ExchangeConvert.ConvertToMetameta(entity);
                try
                {
                    using (var performance = new PerformanceScope("ExchangeMailboxExport.MailBoxVaultExport", "", true))
                    {
                        this.VaultExport(entity, subJobid, ruleName, mediaName);
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Export Exchange Mailbox Metadata Error: {0}", e.ToString());
                    status = JobDetailsStatus.Failed;
                    throw;
                }
                finally
                {
                    //current.BackupStatus = FileHeaderStatus.Complete;
                }
            }
            catch (Exception e)
            {
                mLog.Error("Export Exchange Mailbox Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                //current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            return 0;
        }

        internal void VaultExport(EXOArchiveData entity, string subJobId, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
            //var reportInfo = new ReportInfoDto()
            //{
            //    Url = entity.FullPath,
            //    Size = vaultState.ExportSize,
            //    SubJobID = subJobId,
            //    JobDetailEntityType = GCommon.Contract.Server.Job.Object.JobReportDetailEntityType.Export,
            //    Status = (JobDetailsStatus)vaultState.State,
            //    Action = ReportAction.Export,
            //    CacheNodeType = (int)ExchangeCacheNodeType.Mailbox,
            //    RuleName = ruleName,
            //    Message = string.Empty,
            //};
            //Configuration.JobReportDtoV2.AddReport(reportInfo);
        }
    }

    internal class ExchangeFolderExport : EXOObjectBackup
    {
        public ExchangeFolderExport(AvePoint.RA.Contract.Services.IRALogger log)
        {
            mLog = log;
        }
        public override int Backup(EXOArchiveData entity, string ruleName, string subJobid, int ruleLevel, string mediaName)
        {
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                //MailboxDto folderDto = ExchangeConvert.ConvertToMetameta(entity);
                try
                {
                    using (var performance = new PerformanceScope("ExchangeMailboxExport.FolderVaultExport", "", true))
                    {
                        this.VaultExport(entity, subJobid, ruleName, mediaName);
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Export Exchange Folder Metadata Error: {0}", e.ToString());
                    status = JobDetailsStatus.Failed;
                    throw;
                }
                finally
                {
                    //current.BackupStatus = FileHeaderStatus.Complete;
                }
            }
            catch (Exception e)
            {
                mLog.Error("Export Exchange Folder Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                //current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            return 0;
        }

        internal void VaultExport(EXOArchiveData entity, string subJobId, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
            //var reportInfo = new ReportInfoDto()
            //{
            //    Url = entity.FullPath,
            //    Size = vaultState.ExportSize,
            //    SubJobID = subJobId,
            //    JobDetailEntityType = GCommon.Contract.Server.Job.Object.JobReportDetailEntityType.Export,
            //    Status = (JobDetailsStatus)vaultState.State,
            //    Action = ReportAction.Export,
            //    CacheNodeType = (int)ExchangeCacheNodeType.Folder,
            //    RuleName = ruleName,
            //    Message = string.Empty,
            //};
            //Configuration.JobReportDtoV2.AddReport(reportInfo);
        }
    }

    internal class ExchangeItemExport : EXOObjectBackup
    {
        public ExchangeItemExport(AvePoint.RA.Contract.Services.IRALogger log)
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

        internal void VaultExport(string mailItemId, EXOArchiveData entity, string subJobId, string ruleName)
        {
            ExportStatus vaultState = null;
            Item EXOItem = null;
            JobDetailsStatus status = JobDetailsStatus.None;
            try
            {
                EXOItemPathGeneratorInfo EXOItemPathInfo = null;
                EXOExportInfo exportInfo = null;
                using (var performance = new PerformanceScope("ExchangeMailboxExport.Item.Bind", "", true))
                {
                    EXOItem = Item.Bind(Configuration.service, new ItemId(mailItemId)).GetAwaiter().GetResult();
                }
                Folder EXOFolder = null;
                using (var performance = new PerformanceScope("ExchangeMailboxExport.Folder.Bind", "", true))
                {
                    EXOFolder = Folder.Bind(Configuration.service, EXOItem.ParentFolderId).GetAwaiter().GetResult();
                }
                EXOItemPathInfo = new EXOItemPathGeneratorInfo()
                {
                    EXOItem = EXOItem,
                    JobId = this.Configuration.SubJobId,
                    PhysicalDeviceDtoId = string.Empty,
                    service = Configuration.service,
                    MailAddress = Configuration.ExchangeNodeName,
                    MailFullPath = entity.FullPath,
                    ParentFolderName = EXOFolder.DisplayName,
                    Credentials = Configuration.Credentials,
                };
                using (var performance = new PerformanceScope("ExchangeMailboxExport.GenerateEXOItemExportInfo", "", true))
                {
                    exportInfo = this.EXOExportBeforeArcInfo.EXOExportPathGenerator.GenerateEXOItemExportInfo(EXOItemPathInfo);
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