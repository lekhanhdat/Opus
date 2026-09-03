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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using AvePoint.RA.Service.Services.Tenant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SecurityTool = AvePoint.GCommon.Utility.SecurityUtils;
namespace AvePoint.RA.Service.Services.Explorer
{
    [Audit]
    public class ArchivedContentDownloadService : RMServiceBase, IArchivedContentDownloadService
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService<IRMMyhubServices>();
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();
        public IDownloadDataInfoDao DownloadDataInfoDao { get; set; }
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        [Audit(Module = AuditModule.DownloadCenter, Category = AuditCategory.DownloadCenter, Action = AuditAction.DownloadArchivedContent, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public FileTransferStream DownloadArchivedContent(List<Guid> ids, bool isMyhub = false)
        {
            FileTransferStream stream = null;
            try
            {
                var customerId = TenantLocalValue.LogonGroupId;
                
                // temp directory for download file
                var downloadPath = Path.Combine(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], Guid.NewGuid().ToString());
                var contentInfos = DownloadDataInfoDao.GetDownloadDataInfos(ids, new List<int>() { (int)DownloadContentJobStatus.Finished });
                string multipleDownloadPath = string.Empty;
                string newFileFullPath = string.Empty;
                bool isNewOpus = TenantService.IsNewOpusTenant();
                Logger.Info($"Begin to download archived content. Record count:{ids.Count} ContentInfo count:{contentInfos?.Count}");
                if (contentInfos != null && contentInfos.Count > 0)
                {
                    if (!isMyhub)
                    {
                        //Audit trail JPMC
                        if (contentInfos[0].DownloadType == DownloadContentType.DownloadRCCReport)
                        {
                            List<RMMyhubReportAuditItem> auditItems = RMMyhubServices.GetMyhubReports(ids, (int)MyhubReportJobType.DownloadRCCReport, false);
                            FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)FSAuditType.JpmcDownloadRCCReport, (int)MyhubReportJobType.DownloadRCCReport);
                        }
                        else if (contentInfos[0].DownloadType == DownloadContentType.HistoryContent)
                        {
                            List<RMMyhubReportAuditItem> auditItems = RMMyhubServices.GetMyhubReports(ids, (int)MyhubReportJobType.HistoryContent, false);
                            FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)FSAuditType.DownloadDisposalHistory, (int)MyhubReportJobType.HistoryContent);
                        }
                        // end Audit trail JPMC
                    }
                    long? FileTotalSize = 0;
                    foreach(var info in contentInfos)
                    {
                        FileTotalSize += info.FileSize == null ? 0 : info.FileSize;
                    }
                    if (FileTotalSize > 100 * 1024 * 1024)
                    {
                        return stream;
                    }
                    if (contentInfos.Count > 1)
                    {
                        multipleDownloadPath = downloadPath;
                        downloadPath = downloadPath + "/" + I18N.Core.I18NEntity.GetString("RM_DC_DownloadMultipleArchivedContent");
                    }
                    if (!Directory.Exists(downloadPath))
                    {
                        Directory.CreateDirectory(downloadPath);
                    }
                    using (var performance = new PerformanceScope($"ArchivedContentDownloadService.DownloadArchivedContent.Count:{contentInfos?.Count}"))
                    {

                        var otherTypes = new DownloadContentType[] { 
                            DownloadContentType.HistoryContent, DownloadContentType.LoanPickListContent, DownloadContentType.DestructionPickListContent,
                            DownloadContentType.ReportContent,DownloadContentType.UnderReviewContent,DownloadContentType.WaitingForDisposalContent,
                            DownloadContentType.ExportSearchRecords , DownloadContentType.ExportDiscoveryProfile,
                            DownloadContentType.DisposalExtendContent , DownloadContentType.RelatedRecordsContent, DownloadContentType.ExportTermStructure,
                            DownloadContentType.PhysicalBuklExport, DownloadContentType.JobReportContent, DownloadContentType.MachineLearningExportReport,
                            DownloadContentType.ExportSiteMetrics,DownloadContentType.ExportSettings,DownloadContentType.ExportIndex,DownloadContentType.Others,
                            DownloadContentType.DiscoveryExportRowDataJob, DownloadContentType.ReturnLoanHistory, DownloadContentType.ExportConflictSettingDetail,
                            DownloadContentType.ExportRestoreCenterSeachResult,DownloadContentType.ExportDeduplicationReport,DownloadContentType.ExportSCMapping,
                            DownloadContentType.ExportSCWhitelist, DownloadContentType.ExportSCBlacklist, DownloadContentType.ExportSPSOSetting, DownloadContentType.ExportTeamsSOSetting,
                            DownloadContentType.DiscoveryExportDuplicationReport, DownloadContentType.DownloadRCCReport, DownloadContentType.DiscoveryExportExcludeList, DownloadContentType.ExportHoldRecords,
                            DownloadContentType.SharePointSiteMetricsReport, DownloadContentType.MovePickListContent
                        };
                        foreach (var info in contentInfos)
                        {
                            try
                            {
                                FileInfo fi = null;
                                bool isArchivedOldData = false;
                                if (isNewOpus)
                                {
                                    if (otherTypes.Contains(info.DownloadType))
                                    {
                                        RAStorageUtil.DownloadRecordsArchivedContentToFile(SecurityTool.SafeCombinePath(customerId, info.JobId + ".zip"), SecurityTool.SafeCombinePath(downloadPath, info.Name));
                                        fi = new FileInfo(SecurityTool.SafeCombinePath(downloadPath, info.Name));
                                        newFileFullPath = SecurityTool.SafeCombinePath(downloadPath, info.Name);
                                    }
                                    else
                                    {
                                        var archivedDataDownloadPath = SecurityTool.SafeCombinePath(downloadPath, info.JobId);
                                        if (!Directory.Exists(archivedDataDownloadPath))
                                        {
                                            Directory.CreateDirectory(archivedDataDownloadPath);
                                        }
                                        RAStorageUtil.DownloadRecordsArchivedContentToFile(SecurityTool.SafeCombinePath(TenantLocalValue.LogonGroupId, info.JobId, info.Name), SecurityTool.SafeCombinePath(archivedDataDownloadPath, info.Name));
                                        var filePath = SecurityTool.SafeCombinePath(archivedDataDownloadPath, info.Name);
                                        if (File.Exists(filePath))
                                        {
                                            fi = new FileInfo(SecurityTool.SafeCombinePath(archivedDataDownloadPath, info.Name));
                                            newFileFullPath = SecurityTool.SafeCombinePath(archivedDataDownloadPath, info.Name);
                                        }
                                        else
                                        {
                                            //此处用于兼容老数据
                                            RAStorageUtil.DownloadRecordsArchivedContentToFile(SecurityTool.SafeCombinePath(customerId, info.JobId + ".dat"), SecurityTool.SafeCombinePath(downloadPath, info.JobId + ".dat"));
                                            fi = new FileInfo(SecurityTool.SafeCombinePath(downloadPath, info.JobId + ".dat"));
                                            isArchivedOldData = true;
                                            if (!File.Exists(fi.FullName))
                                            {
                                                throw new Exception("Can not download Data to local,The file may have been deleted ,or this file upload in other storage");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (otherTypes.Contains(info.DownloadType))
                                    {
                                        RAStorageUtil.DownloadArchivedContentToFile(SecurityTool.SafeCombinePath(customerId, info.JobId + ".zip"), SecurityTool.SafeCombinePath(downloadPath, info.Name));
                                        fi = new FileInfo(SecurityTool.SafeCombinePath(downloadPath, info.Name));
                                        newFileFullPath = SecurityTool.SafeCombinePath(downloadPath, info.Name);
                                    }
                                    else
                                    {
                                        RAStorageUtil.DownloadArchivedContentToFile(SecurityTool.SafeCombinePath(customerId, info.JobId + ".dat"), SecurityTool.SafeCombinePath(downloadPath, info.JobId + ".dat"));
                                        fi = new FileInfo(SecurityTool.SafeCombinePath(downloadPath, info.JobId + ".dat"));
                                        isArchivedOldData = true;
                                    }
                                }
                                //rename file with real name
                                if (isArchivedOldData)
                                {
                                    string newName = info.Name;
                                    var archivedDataDownloadPath = SecurityTool.SafeCombinePath(downloadPath, info.JobId);
                                    if (!Directory.Exists(archivedDataDownloadPath))
                                    {
                                        Directory.CreateDirectory(archivedDataDownloadPath);
                                    }
                                    FileInfo newFile = new FileInfo(SecurityTool.SafeCombinePath(archivedDataDownloadPath, newName));
                                    fi.MoveTo(SecurityTool.SafeCombinePath(archivedDataDownloadPath, newName));
                                    newFileFullPath = SecurityTool.SafeCombinePath(archivedDataDownloadPath, newName);
                                    Logger.Info($"Rename file success. Id:{info.RecordsId}");
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Error occurred while downloading archived content. Id:{info.RecordsId} Error:{e.ToString()}");
                            }
                        }
                    }


                    string fileName = string.Empty;
                    string fileFullPath = string.Empty;
                    if (contentInfos.Count == 1)
                    {
                        fileName = contentInfos[0].Name;
                        fileFullPath = newFileFullPath;
                        stream = new FileTransferStream(fileFullPath, downloadPath, FileMode.Open);
                    }
                    else
                    {
                        ZipUtil.ZipFolder(downloadPath, downloadPath + JobMonitorConstants.ZIP, Encoding.UTF8);
                        fileFullPath = downloadPath + JobMonitorConstants.ZIP;
                        stream = new FileTransferStream(fileFullPath, multipleDownloadPath, FileMode.Open);
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error occurred while downloading archived content. Ids:{string.Join(",", ids)} Error:{e.ToString()}");
            }
            return stream;
        }

        public void DeleteExpiredData(string jobId)
        {
            using (var performance = new PerformanceScope($"ArchivedContentDownloadService.DeleteArchivedCount.JobId:{jobId}"))
            {
                var customerId = TenantLocalValue.LogonGroupId;
                bool isNewOpusTenant = TenantService.IsNewOpusTenant();
                RAStorageUtil.DeleteExpiredArchivedContent(SecurityTool.SafeCombinePath(customerId, jobId + ".dat"), isNewOpusTenant);
                RAStorageUtil.DeleteExpiredArchivedContent(SecurityTool.SafeCombinePath(customerId, jobId + ".zip"), isNewOpusTenant);
                RAStorageUtil.DeleteExpiredArchivedContent(SecurityTool.SafeCombinePath(customerId, jobId), isNewOpusTenant);
            }
        }
    }
}
