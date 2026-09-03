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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Core.Util;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using PnP.Framework.Modernization.Extensions;
using PnP.Framework.Modernization.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    public class SharePointArchiverActionDetailWorker : AbstractJobDetailWorker
    {
        internal string CREATE_SUMMAYTABLE_SQL { get; set; }
        internal string INSERT_SUMMAYDATA_SQL { get; set; }
        internal string SUMMAY_TABLE_NAME { get; set; }
        private IRMOptimizationSettingInfoDao RMOptimizationSettingInfoDao => PlatformWindsorManager.GetService<IRMOptimizationSettingInfoDao>();

        private readonly JobReportShardHelper _shardHelper = new(JobMonitorConstants.MAX_ROWS_PER_RPT_FILE);

        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            string reportFilePath = null;
            if (jobInfo.NeedQueryFromUploadLocation)
            {
                reportFilePath = GetReportFilePath(jobInfo);
                if (JobServiceUtility.IsSubJob(jobInfo.Id))
                {
                    reportFilePath = _shardHelper.GetOrCreateShardFile(jobInfo, reportFilePath, TABLE_NAME, 0, path => { });
                }
            }
            else
            {
                reportFilePath = DownloadReports(jobInfo);
            }
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            totalCount = base.GetCountForDetail(reportFilePath, base.SELECT_DETAIL_COUNT_SQL, jobInfo);
            return GetData(PageSize, StartPage, conditionFilter, jobInfo);
        }
        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = null;
            if (jobInfo.NeedQueryFromUploadLocation)
            {
                reportFilePath = GetReportFilePath(jobInfo);
                if (JobServiceUtility.IsSubJob(jobInfo.Id))
                {
                    reportFilePath = _shardHelper.GetOrCreateShardFile(jobInfo, reportFilePath, TABLE_NAME, 0, path => { });
                }
            }
            else
            {
                reportFilePath = DownloadReports(jobInfo);
            }
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetData(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            return result;
        }

        public override void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            string orderCondition = "ActionTab, rowid";
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, orderCondition, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, conditionFilter, orderCondition, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            var details = jobDetails.Where(item => item is JMArchiverActionJobDetails);
            if (details != null && details.Count() > 0)
            {
                InitCreateTableSQLString();
                string basePath = base.NeedCreateTable(jobInfo);
                string reportFilePath = _shardHelper.GetOrCreateShardFile(jobInfo, basePath, TABLE_NAME, details.Count(), createTable: CreateTableNew);
                JobDetailDao.SaveDataIntoTable(reportFilePath, details, this.INSERT_DATA_SQL);
            }
            var summaryDetails = jobDetails.Where(item => item is JMSOSummaryDetails);
            if (summaryDetails != null && summaryDetails.Count() > 0)
            {
                InitCreateSummaryTableSQLString();

                string reportFilePath = GetReportFilePath(jobInfo);
                if (JobServiceUtility.IsSubJob(jobInfo.Id))
                {
                    reportFilePath = _shardHelper.GetOrCreateShardFile(jobInfo, reportFilePath, SUMMAY_TABLE_NAME, 0, createTable: CreateSOSummaryTable);
                }

                CreateSOSummaryTable(reportFilePath);

                JobDetailDao.SaveDataIntoTable(reportFilePath, summaryDetails, this.INSERT_SUMMAYDATA_SQL);
            }
        }

        protected void CreateSOSummaryTable(string reportFilePath)
        {
            lock (createTableLocker)
            {
                if (!CheckFileExist(reportFilePath) || !JobDetailDao.IsExistTable(reportFilePath, SUMMAY_TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
                {
                    try
                    {
                        CheckAndCreateDirectory(reportFilePath);
                        SQLCommond.ExecuteNonQuery(reportFilePath, CREATE_SUMMAYTABLE_SQL);
                        logger.Debug("Successfulfull to create table {0}.", SUMMAY_TABLE_NAME);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("failed to create table {0}.", SUMMAY_TABLE_NAME);
                        logger.Error(ex.ToString());
                    }
                }
            }
        }

        public override bool UploadReports(BaseJobDto jobInfo)
        {
            string reportFilePath = GetReportFilePath(jobInfo);
            if (JobServiceUtility.IsSubJob(jobInfo.Id))
            {
                string basePath = base.NeedCreateTable(jobInfo);
                reportFilePath = _shardHelper.GetOrCreateShardFile(jobInfo, basePath, TABLE_NAME, 0, createTable: CreateTableNew);
            }
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                try
                {
                    logger.Info($"start to upload file");
                    var tenantFolderName = GetBlobFolderUrl(reportFilePath);
                    var blobName = new StringBuilder();
                    if (!string.IsNullOrEmpty(tenantFolderName))
                    {
                        blobName.Append(tenantFolderName).Append("/");
                    }
                    blobName.Append(Path.GetFileName(reportFilePath));
                    RAStorageUtil.UploadReportBlob(blobName.ToString(), reportFilePath);
                    logger.Info($"finish to upload blob name:{blobName}");
                    DeleteFile(reportFilePath);
                    logger.Info($"finish to delete file.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error uploading file {reportFilePath}: {ex.Message}");
                    return false;
                }
            }
            else
            {
                logger.Warn($"Report file path is empty or file does not exist: {reportFilePath}");
            }
            return true;
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_SharePoint_Archiver_Report, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_SharePoint_Archiver_Report, TABLE_NAME);
        }

        public void InitCreateSummaryTableSQLString()
        {
            SUMMAY_TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
            CREATE_SUMMAYTABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_SharePoint_Archiver_SUMMARYReport, SUMMAY_TABLE_NAME);
            INSERT_SUMMAYDATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_SharePoint_Archiver_SUMMARYReport, SUMMAY_TABLE_NAME);
        }

        public override void ClearJobSummaryDetails(BaseJobDto jobInfo)
        {
            try
            {
                string reportFilePath = DownloadReports(jobInfo);
                SQLCommond.ExecuteNonQuery(reportFilePath, $"delete from JobSummaryDetail");
            }
            catch (Exception ex)
            {
                logger.Error("failed to clear JobSummaryDetail table.");
            }
        }

        public override JMJobDetails GetDataForJobSummaryDetails(string conditionFilter, BaseJobDto jobInfo)
        {
            JMJobDetails result = new JMSOSummaryDetails() { ActionStatistics = new List<ActionStatistics>() };
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            logger.Info("filePath:{0},file exist:{1},table exist:{2}", reportFilePath, isRPTExist, isTableInRPTExist);
            
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                if (jobInfo.JobType == (int)JobType.DiscoverOptimization 
                    || jobInfo.JobType == (int)JobType.DiscoveryPreScan 
                    || jobInfo.JobType == (int)JobType.DiscoveryAOSPOptimization)
                {
                    AddDiscoverOptimizationSettingsToDetails(ref result, jobInfo);
                }
                return result;
            }
            result = JobDetailDao.GetDataForSOSummaryDetails(reportFilePath, "select * from JobSummaryDetail", jobInfo);
            //totalCount = base.GetCountForDetail(reportFilePath, "select count(*) from JobSummaryDetail", jobInfo);

            if (jobInfo.JobType == (int)JobType.DiscoverOptimization 
                || jobInfo.JobType == (int)JobType.DiscoveryPreScan
                || jobInfo.JobType == (int)JobType.DiscoveryAOSPOptimization)
            {
                AddDiscoverOptimizationSettingsToDetails(ref result, jobInfo);
            }
            
            return result;
        }

        private void AddDiscoverOptimizationSettingsToDetails(ref JMJobDetails result, BaseJobDto jobInfo)
        {
            try
            {
                RMOptimizationSettingInfo history = null;
                RMOptimizationSettingInfo tempDBHistory = RMOptimizationSettingInfoDao.GetSettingInfoByKeys(TenantLocalValue.LogonGroupId, jobInfo.Id);
                if (tempDBHistory == null)
                {
                    logger.Info("this job summery is old summery that need query azure table");
                    var tempTableHistorey = RMRecordStorageAzureTableContext.RMDiscoverDataOptimizationSettingsHistory.FirstOrDefault(i => i.PartitionKey.Equals(TenantLocalValue.LogonGroupId, StringComparison.CurrentCultureIgnoreCase) && i.RowKey.Equals(jobInfo.Id, StringComparison.CurrentCultureIgnoreCase)).GetAwaiter().GetResult();
                    if (tempTableHistorey != null)
                    {
                        history = new RMOptimizationSettingInfo();
                        history.Settings = tempTableHistorey.Settings;
                    }
                    logger.Info("finish query azure table");
                }
                else
                {
                    logger.Info("this job summery is new summery that need query db");
                    history = tempDBHistory;
                }
                if (history != null)
                {
                    JMSOSummaryDetails newResult = null;
                    if (result == null)
                    {
                        newResult = new JMSOSummaryDetails();
                        newResult.ActionStatistics = new List<ActionStatistics>();
                    }
                    else
                    {
                        newResult = (JMSOSummaryDetails)result;
                    }
                    var discoveryJobSettings = SerializerHelper.DeserializeByJsonSerializer<DataOptimizationSettingsForJobHistory>(history.Settings);
                    if (discoveryJobSettings != null)
                    {
                        DOJobSettingsStatistics jobSettingsStatistics = new DOJobSettingsStatistics();
                        jobSettingsStatistics.ScopeSettings.MS365DataTypeStr = GetMS365DataTypeI18NStr(discoveryJobSettings);
                        jobSettingsStatistics.ScopeSettings.ModifiedTimeRangeStr = GetModifiedRangeI18NStr(discoveryJobSettings);
                        jobSettingsStatistics.ScopeSettings.SizeRangeStr = GetSizeRangeI18NStr(discoveryJobSettings);
                        jobSettingsStatistics.ScopeSettings.FileCatagorysStr = discoveryJobSettings?.ScopeSettings?.FileCatagorysStr;

                        if (discoveryJobSettings.ScopeSettings == null || discoveryJobSettings.ScopeSettings.MS365DataType == MS365DataType.Phl)
                        {
                            jobSettingsStatistics.ScopeSettings.FileCatagorysStr = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                        }
                        else if (jobSettingsStatistics.ScopeSettings.FileCatagorysStr.Contains("RM_FA_FileType_Empty"))
                        {
                            jobSettingsStatistics.ScopeSettings.FileCatagorysStr = jobSettingsStatistics.ScopeSettings.FileCatagorysStr.Replace("RM_FA_FileType_Empty", I18NEntity.GetString("RM_FA_FileType_Empty"));
                        }

                        jobSettingsStatistics.DefinitionAndActionSettings.DefinitionsStr = I18NEntity.GetString(discoveryJobSettings?.DefinitionAndActionSettings?.DefinitionsStr);
                        string fileActionTemp = discoveryJobSettings?.DefinitionAndActionSettings?.DocumentActionStr;
                        if (!string.IsNullOrEmpty(fileActionTemp))
                        {
                            if (fileActionTemp.Contains("RM_FA_DataOptimize_File_ArchiveAndRemove"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_FA_DataOptimize_File_ArchiveAndRemove", I18NEntity.GetString("RM_FA_DataOptimize_File_ArchiveAndRemove"));
                            }
                            if (fileActionTemp.Contains("RM_FA_DataOptimize_File_LeaveStub"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_FA_DataOptimize_File_LeaveStub", I18NEntity.GetString("RM_FA_DataOptimize_File_LeaveStub"));
                            }
                            if (fileActionTemp.Contains("RM_FA_DataOptimize_File_RemoveFile"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_FA_DataOptimize_File_RemoveFile", I18NEntity.GetString("RM_FA_DataOptimize_File_RemoveFile"));
                            }
                            if (fileActionTemp.Contains("RM_JS_Rule_ArchiveVersionAndDestroyFile"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_JS_Rule_ArchiveVersionAndDestroyFile", I18NEntity.GetString("RM_JS_Rule_ArchiveVersionAndDestroyFile"));
                            }
                            if (fileActionTemp.Contains("RM_JS_RDM_CreateRule_Options_Backup"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_JS_RDM_CreateRule_Options_Backup", I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Backup"));
                            }
                            if (fileActionTemp.Contains("RM_JS_Audit_ArchiveVersionAndDestroyFile"))
                            {
                                fileActionTemp = fileActionTemp.Replace("RM_JS_Audit_ArchiveVersionAndDestroyFile", I18NEntity.GetString("RM_JS_Audit_ArchiveVersionAndDestroyFile"));
                            }
                        }
                        jobSettingsStatistics.DefinitionAndActionSettings.DocumentActionStr = fileActionTemp;
                        jobSettingsStatistics.DefinitionAndActionSettings.DocumentVersionActionStr = I18NEntity.GetString(discoveryJobSettings?.DefinitionAndActionSettings?.DocumentVersionActionStr);
                        newResult.ActionStatistics.Add(jobSettingsStatistics);
                        result = newResult;
                    }
                }
                else
                {
                    logger.Warn($"Can not find settings history in azure table, JobId {jobInfo.Id}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"AddDiscoverOptimizationSettingsToDetails exception, JobId {jobInfo.Id}, message {e}");
            }
        }

        private string GetModifiedRangeI18NStr(DataOptimizationSettingsForJobHistory settingsHistory)
        {
            string modifiedTimeFrom = string.Empty;
            string modifiedTimeTo = string.Empty;
            if (settingsHistory.ScopeSettings.WithoutDateQueryParameter.From <= -1)
            {
                modifiedTimeFrom = $"0 {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
            }
            else
            {
                var from = settingsHistory.ScopeSettings.WithoutInDateDataInfos.FirstOrDefault(i => i.Id == settingsHistory.ScopeSettings.WithoutDateQueryParameter.From);
                if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }

            if (settingsHistory.ScopeSettings.WithoutDateQueryParameter.To >= 999)
            {
                modifiedTimeTo = I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max");
            }
            else
            {
                var to = settingsHistory.ScopeSettings.WithoutInDateDataInfos.FirstOrDefault(i => i.Id == settingsHistory.ScopeSettings.WithoutDateQueryParameter.To);
                if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }
            return string.Format(I18NEntity.GetString("ExchangeOnline.Service_642972b7-1c4c-48e0-b94e-d968795edd09"), modifiedTimeFrom, modifiedTimeTo);
        }

        private string GetSizeRangeI18NStr(DataOptimizationSettingsForJobHistory settingsHistory)
        {
            if (settingsHistory.ScopeSettings.SizeRangeQueryParameter.SizeRange == 0 || settingsHistory.ScopeSettings.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None)
            {
                return settingsHistory.ScopeSettings.SizeRangeStr = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
            }
            else
            {
                return settingsHistory.ScopeSettings.SizeRangeStr;
            }
        }

        private string GetMS365DataTypeI18NStr(DataOptimizationSettingsForJobHistory settingsHistory)
        {
            if (settingsHistory.ScopeSettings == null || settingsHistory.ScopeSettings.MS365DataType == MS365DataType.Phl)
            {
                return I18NEntity.GetString("RM_FA_DataOptimize_PreservationHoldLibraryTitle");
            }
            else
            {
                return I18NEntity.GetString("RM_FA_DataOptimize_SharepointOrOneDriveTitle");
            }
        }
    }
}
