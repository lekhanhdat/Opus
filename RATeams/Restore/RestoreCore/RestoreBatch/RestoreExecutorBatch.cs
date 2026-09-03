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

using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Archiver;
using ExchangeUtility.Graph;
using Microsoft.Graph.Models.Security;
using Office365GroupRestore.Worker;
using RAArchiverCommon;
using RATeams.Restore.RestoreCore.RestoreToStorage;
using Storage;
using static Google.Apis.Storage.v1.ObjectsResource;


namespace Office365GroupRestore
{
    public class RestoreExecutorBatch : IRestoreExecutor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreExecutorBatch));

        private IReportCenter _reporter;
        private RestoreConfig _config;
        private IEnumerable<RestoreDataBlockCollection> _data;


        private Dictionary<ExchangeDataBlockType, BaseRestoreHelperBatch> RestoreFactory = new Dictionary<ExchangeDataBlockType, BaseRestoreHelperBatch>();

        public RestoreExecutorBatch Build(RestoreConfig config)
        {
            this._config = config;
            return this;
        }
        public RestoreExecutorBatch Build(IReportCenter reporter)
        {
            this._reporter = reporter;
            return this;
        }
        public RestoreExecutorBatch Build(IEnumerable<RestoreDataBlockCollection> data)
        {
            this._data = data;
            return this;
        }


        public void PostAction()
        {
            RestoreFactory[ExchangeDataBlockType.Mailbox].PostAction();
        }

        public bool ArchiveTeams(bool makeSiteReadOnly)
        {
            return RestoreFactory[ExchangeDataBlockType.Mailbox].UpdateTeamsArchiveStatus(makeSiteReadOnly);
        }

        public bool UnArchiveTeams()
        {
            return RestoreFactory[ExchangeDataBlockType.Mailbox].UpdateTeamsUnarchiveStatus();
        }

        public bool IsTeamsArchived()
        {
            return RestoreFactory[ExchangeDataBlockType.Mailbox].IsTeamsArchived();
        }

        public void Execute()
        {
            //var restoreDataHandler = restoreDataHandlerBase as RestoreDataHandlerBatch;
            //this.config = config;
            ExchangeItemExportToStorage exchangeItemExportToStorage = null;
            PerformanceDataManager pDataManager = new PerformanceDataManager();
            pDataManager.Start();

            logger.Info("Start to execute the restore executor");
            foreach (var datablockCollection in _data)
            {
                logger.Info($"Process the datablockCollection, type:{datablockCollection.CollectionType}, count:{datablockCollection.ItemsCount}, Size: {datablockCollection.TotalSize}");
                pDataManager.CollectPerformanceData(datablockCollection.CollectionType, datablockCollection.TotalSize, datablockCollection.ItemsCount);
                switch (datablockCollection.CollectionType)
                {
                    case ExchangeDataBlockType.Finish:
                        if (RestoreFactory.Count > 0)
                        {
                            RestoreFactory[ExchangeDataBlockType.Mailbox].Dispose();
                        }
                        break;

                    case ExchangeDataBlockType.Exception:
                        var exceptionCollection = datablockCollection as ExceptionDataBlockCollection;
                        throw new DataBlockException(exceptionCollection.ExceptionMessage);
                    case ExchangeDataBlockType.Mailbox:
                        {
                            var restoreHelper = new MailboxRestoreHelperBatch();
                            RestoreFactory[ExchangeDataBlockType.Mailbox] = restoreHelper;
                            if (IsTeamsOutOfPlaceRestore())
                            {
                                restoreHelper.Build(_reporter).Build(_config).SendReport(datablockCollection.Items);
                                break;
                            }
                            using (var siteScope = new SiteStateTransitionScopeUtility(_config.BposInfo.SiteUrl, AvePoint.Wrapper.Common.SiteState.ReadOnly, _config.IsSupportLockedSite, true))
                            {
                                restoreHelper.Build(_reporter).Build(_config).Restore(datablockCollection.Items);
                            }
                            break;
                        }
                    case ExchangeDataBlockType.Folder:
                        {
                            if (IsTeamsOutOfPlaceRestore())
                                break;
                            if (!RestoreFactory.TryGetValue(datablockCollection.CollectionType, out var restoreHelper))
                            {
                                restoreHelper = new FolderRestoreHelperBatch(RestoreFactory[ExchangeDataBlockType.Mailbox]);
                                RestoreFactory[ExchangeDataBlockType.Folder] = restoreHelper;
                            }
                            restoreHelper.Restore(datablockCollection.Items);
                            break;
                        }
                    case ExchangeDataBlockType.Plan:
                        {
                            if (IsTeamsOutOfPlaceRestore())
                                break;
                            if (!RestoreFactory.TryGetValue(datablockCollection.CollectionType, out var restoreHelper))
                            {
                                restoreHelper = new PlanRestoreHelperBatch(RestoreFactory[ExchangeDataBlockType.Mailbox]);
                                RestoreFactory[ExchangeDataBlockType.Plan] = restoreHelper;
                            }
                            restoreHelper.Restore(datablockCollection.Items);
                            break;
                        }
                    case ExchangeDataBlockType.Task:
                        {
                            if (IsTeamsOutOfPlaceRestore())
                                break;
                            if (!RestoreFactory.TryGetValue(datablockCollection.CollectionType, out var restoreHelper))
                            {
                                restoreHelper = new TaskRestoreHelperBatch(RestoreFactory[ExchangeDataBlockType.Mailbox]);
                                RestoreFactory[ExchangeDataBlockType.Task] = restoreHelper;
                            }
                            restoreHelper.Restore(datablockCollection.Items);
                            break;
                        }
                    case ExchangeDataBlockType.Item:
                        {
                            if (!RestoreFactory.TryGetValue(datablockCollection.CollectionType, out var restoreHelper))
                            {
                                restoreHelper = new ItemRestoreHelperBatch(RestoreFactory[ExchangeDataBlockType.Mailbox]);
                                RestoreFactory[ExchangeDataBlockType.Item] = restoreHelper;
                            }
                            if (IsTeamsOutOfPlaceRestore())
                            {
                                using (var performance = new PerformanceScope("RestoreConversationItem", "", true))
                                {
                                    if (exchangeItemExportToStorage == null)
                                    {
                                        exchangeItemExportToStorage = new ExchangeItemExportToStorage(_config, _reporter);
                                    }
                                    exchangeItemExportToStorage.SetRestoreHelperBatch(restoreHelper);
                                    exchangeItemExportToStorage.RestoreToStorage(datablockCollection.CollectionType, datablockCollection.Items);
                                    break;
                                }
                            }
                            restoreHelper.Restore(datablockCollection.Items);
                            break;
                        }
                    case ExchangeDataBlockType.Post:
                    case ExchangeDataBlockType.Event:
                    case ExchangeDataBlockType.Attachment:
                        {
                            using (var performance = new PerformanceScope("RestoreEmailItem", "", true))
                            {
                                if (exchangeItemExportToStorage == null)
                                {
                                    exchangeItemExportToStorage = new ExchangeItemExportToStorage(_config, _reporter);
                                }
                                exchangeItemExportToStorage.RestoreToStorage(datablockCollection.CollectionType, datablockCollection.Items);
                                break;
                            }
                        }
                    case ExchangeDataBlockType.SiteCollection:
                    case ExchangeDataBlockType.Web:
                    case ExchangeDataBlockType.SiteList:
                    case ExchangeDataBlockType.SiteFolder:
                    case ExchangeDataBlockType.SiteAttachmentItem:
                    case ExchangeDataBlockType.SiteDocumentItem:
                    case ExchangeDataBlockType.SiteVersionItem:
                        {
                            using (var performance = new PerformanceScope("RestoreSiteItem", "", true))
                            {
                                if (exchangeItemExportToStorage == null)
                                {
                                    exchangeItemExportToStorage = new ExchangeItemExportToStorage(_config, _reporter);
                                }
                                exchangeItemExportToStorage.RestoreToStorage(datablockCollection.CollectionType, datablockCollection.Items);
                                break;
                            }
                        }
                }
            }
            SendEmailAndUploadZipIfExportRestore(exchangeItemExportToStorage);
            SubmitTaskAttachmentLink();
            pDataManager.Finish();
            logger.Info("Restore executor finish.");
        }


        private bool IsTeamsOutOfPlaceRestore()
        {
            return _config.JobType == (int)JobType.TeamsOutPlaceRestore;
        }

        private void SendEmailAndUploadZipIfExportRestore(ExchangeItemExportToStorage? exchangeItemExportToStorage)
        {
            try
            {
                if (exchangeItemExportToStorage != null && exchangeItemExportToStorage.CurrentFolderSize > 0)
                {
                    exchangeItemExportToStorage.UploadZipFileToStorage();
                }
            } 
            catch (Exception e)
            {
                logger.Error($"Upload zip fail, ex:{e}");
            }

            try
            {
                if (exchangeItemExportToStorage != null && (exchangeItemExportToStorage?.exportHasError == false || IsTeamsOutOfPlaceRestore()))
                {
                    ExportSendEmail sendEmail = new ExportSendEmail();
                    ParameterDto para = new ParameterDto() { ExportLocation = _config.DestinationDeviceSystemPath, ZipPassword = _config.ZipFilePassword, RestoreJobid = _config.RestoreJobId.Substring(0, _config.RestoreJobId.IndexOf('_')) };
                    sendEmail.SendEmailAsync(_config.NotificationUsers, para).GetAwaiter().GetResult();
                }
                else
                {
                    logger.Error("Export failed, no email will be sent.");
                }
                SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to send email. Reason: {0}", ex.ToString());
            }
        }

        private void SubmitTaskAttachmentLink()
        {
            try
            {
                if (RestoreConfig.NeedRecordTaskAttachmentsLink)
                {
                    logger.Info("Start sending Planner Task attachment urls.");
                    var urls = new List<string>();
                    urls.AddRange(TaskAttachmentLinkCollector.Collection);
                    TaskAttachmentLinkCollector.Close();
                    //stodo//new AvePoint.GCommon.JobManagement.AveJobManagementService().UpdateJobContext(urls, config.JobId, RestoreConfig.TenantGroupId, AvePoint.Common.AveEnv.GetAgentTempFolder(AvePoint.Common.ContextLevel.Process));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to send task attachment link. Reason: {0}", ex.ToString());
            }
        }

    }
}